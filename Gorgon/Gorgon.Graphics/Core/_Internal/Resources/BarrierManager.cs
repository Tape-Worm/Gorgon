// Gorgon.
// Copyright (C) 2026 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 13, 2026 12:32:01 AM
//

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Native;
using Gorgon.Timing;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Manages resource barriers for command lists.
/// </summary>
/// <param name="graphics">The graphics interface associated with this manager.</param>
internal unsafe class BarrierManager(GorgonGraphics graphics)
{
    /// <summary>
    /// An entry for a texture barrier.
    /// </summary>
    /// <param name="Barrier">The barrier state for the texture.</param>
    /// <param name="Range">The contiguous range of the barrier across subresources.</param>
    /// <param name="TextureMipCount">The total mip map count for the texture.</param>
    /// <param name="TextureArrayCount">The total number of array indices for the texture.</param>
    /// <param name="TexturePlaneCount">The total number of planes for the texture format.</param>
    public readonly record struct TextureBarrierEntry(GorgonTextureBarrier Barrier, GorgonSubResourceRange Range, short TextureMipCount, short TextureArrayCount, byte TexturePlaneCount)
    {
        /// <summary>
        /// <inheritdoc cref="TextureBarrierEntry(GorgonTextureBarrier, GorgonSubResourceRange, short, short, byte)" path="/param[@name='Barrier']"/>
        /// </summary>
        public readonly GorgonTextureBarrier Barrier = Barrier;
        /// <summary>
        /// <inheritdoc cref="TextureBarrierEntry(GorgonTextureBarrier, GorgonSubResourceRange, short, short, byte)" path="/param[@name='Range']"/>
        /// </summary>
        public readonly GorgonSubResourceRange Range = Range;
        /// <summary>
        /// <inheritdoc cref="TextureBarrierEntry(GorgonTextureBarrier, GorgonSubResourceRange, short, short, byte)" path="/param[@name='TextureMipCount']"/>
        /// </summary>
        public readonly short TextureMipCount = TextureMipCount;
        /// <summary>
        /// <inheritdoc cref="TextureBarrierEntry(GorgonTextureBarrier, GorgonSubResourceRange, short, short, byte)" path="/param[@name='TextureBarrierCount']"/>
        /// </summary>
        public readonly short TextureArrayCount = TextureArrayCount;
        /// <summary>
        /// <inheritdoc cref="TextureBarrierEntry(GorgonTextureBarrier, GorgonSubResourceRange, short, short, byte)" path="/param[@name='TexturePlaneCount']"/>
        /// </summary>
        public readonly byte TexturePlaneCount = TexturePlaneCount;
    }

    // Masks to ensure "before" state compatibility with the copy and compute queues.
    private const BarrierSync LegalCopySyncMask = BarrierSync.None | BarrierSync.All | BarrierSync.Copy | BarrierSync.Split;
    private const BarrierAccess LegalCopyAccessMask = BarrierAccess.CopyDestination | BarrierAccess.CopySource | BarrierAccess.None;
    private const BarrierSync LegalComputeSyncMask = BarrierSync.None | BarrierSync.All | BarrierSync.ComputeShading | BarrierSync.Copy
                                                    | BarrierSync.ExecuteIndirect | BarrierSync.AllShading | BarrierSync.NonPixelShading | BarrierSync.ClearReadWriteView
                                                    | BarrierSync.Split;
    // Future: | BarrierSync.RayTracing
    //         | BarrierSync.BuildRayTracingAccelerationStructure
    //         | BarrierSync.CopyRayTracingAccelerationStructure
    //         | BarrierSync.EmitRayTracingAccelerationStructurePostBuildInformation
    private const BarrierAccess LegalComputeAccessMask = BarrierAccess.None | BarrierAccess.Common | BarrierAccess.VertexBuffer | BarrierAccess.ConstantBuffer
                                                       | BarrierAccess.ReadWrite | BarrierAccess.ShaderResource | BarrierAccess.IndirectArgument | BarrierAccess.CopyDestination
                                                       | BarrierAccess.CopySource;
    // Future: | BarrierAccess.RayTracingAccelerationStructureRead
    //         | BarrierAccess.RayTracingAccelerationStructureWrite

    // Future:
    // These masks are used to indicate which queue the resource was last used by (from its "before" layout).
    // We can use these to force a transition on the appropriate queue to make it compatible with the current executing queue.
    // For now, these values are not used and are here just for reference should we need to integrate such a system.
    private const BarrierLayout GraphicsOnlyLayoutMask = BarrierLayout.RenderTarget | BarrierLayout.DepthStencilWrite | BarrierLayout.DepthStencilRead | BarrierLayout.ResolveSource
                                                       | BarrierLayout.ResolveDestination | BarrierLayout.ShadingRateSource | BarrierLayout.GraphicsCommon | BarrierLayout.GraphicsGenericRead
                                                       | BarrierLayout.GraphicsReadWrite | BarrierLayout.GraphicsShaderResource | BarrierLayout.GraphicsCopySource | BarrierLayout.GraphicsCopyDestination;
    private const BarrierLayout ComputeOnlyLayoutMask = BarrierLayout.ComputeCommon | BarrierLayout.ComputeGenericRead | BarrierLayout.ComputeReadWrite | BarrierLayout.ComputeShaderResource
                                                      | BarrierLayout.ComputeCopySource | BarrierLayout.ComputeCopyDestination;
    // Accesses that write. A barrier after one of these can't be skipped, even when the state doesn't change, because it's what makes later work wait for the write.
    private const BarrierAccess WriteAccessMask = BarrierAccess.CopyDestination | BarrierAccess.ReadWrite;

    private readonly GorgonGraphics _graphics = graphics;
    private readonly GlobalBarrierState _globalState = graphics.Queues.GlobalBarriers;
    private readonly Dictionary<ulong, GorgonTextureBarrier> _initialTextures = [];
    private readonly Dictionary<ulong, GorgonBufferBarrier> _initialBuffers = [];
    private readonly Dictionary<ulong, GorgonBufferBarrier> _currentBuffers = [];
    private readonly Dictionary<ulong, GorgonBufferBarrier> _pendingBuffers = [];
    private readonly Dictionary<ulong, List<TextureBarrierEntry>> _pendingTextures = [];
    private readonly Dictionary<ulong, List<TextureBarrierEntry>> _currentTextures = [];
    private readonly Stack<List<TextureBarrierEntry>> _listPool = new(16384);

    /// <summary>
    /// Property to return whether there are any pending barriers.
    /// </summary>
    public bool IsEmpty => _pendingBuffers.Count == 0 && _pendingTextures.Count == 0;

    /// <summary>
    /// Function to allocate a new sub resource list for a texture barrier.
    /// </summary>
    /// <returns>The texture barrier from the pool, or if none are available, a new list.</returns>
    private List<TextureBarrierEntry> AllocateList()
    {
        if (!_listPool.TryPop(out List<TextureBarrierEntry>? result))
        {
            return [];
        }

        return result;
    }

    /// <summary>
    /// Function to deallocate a list of barriers and their sub resource lists.
    /// </summary>
    /// <param name="list">The list to deallocate.</param>
    private void DeallocateBarrierLists(Dictionary<ulong, List<TextureBarrierEntry>> list)
    {
        foreach (List<TextureBarrierEntry> entries in list.Values)
        {
            entries.Clear();
            _listPool.Push(entries);
        }
        list.Clear();
    }

    /// <summary>
    /// Function to determine if this sub resource range for this key intersects with another key.
    /// </summary>
    /// <param name="subResource">The sub resource range to compare.</param>
    /// <param name="other">The other sub resource range.</param>
    /// <returns><b>true</b> if the key intersects with the sub resource range, <b>false</b> if not.</returns>
    private static bool RangeIntersects(ref readonly GorgonSubResourceRange subResource, ref readonly GorgonSubResourceRange other)
    {
        GorgonRange<short> thisMipRange = new(subResource.FirstMipLevel, (short)(subResource.FirstMipLevel + subResource.MipLevelCount - 1));
        GorgonRange<short> otherMipRange = new(other.FirstMipLevel, (short)(other.FirstMipLevel + other.MipLevelCount - 1));
        GorgonRange<short> thisArrayRange = new(subResource.FirstArrayIndex, (short)(subResource.FirstArrayIndex + subResource.ArrayCount - 1));
        GorgonRange<short> otherArrayRange = new(other.FirstArrayIndex, (short)(other.FirstArrayIndex + other.ArrayCount - 1));
        GorgonRange<byte> thisPlaneRange = new(subResource.FirstPlane, (byte)(subResource.FirstPlane + subResource.PlaneCount - 1));
        GorgonRange<byte> otherPlaneRange = new(other.FirstPlane, (byte)(other.FirstPlane + other.PlaneCount - 1));

        return thisMipRange.Intersects(otherMipRange) && thisArrayRange.Intersects(otherArrayRange) && thisPlaneRange.Intersects(otherPlaneRange);
    }

    /// <summary>
    /// Function to find the difference of a region's range from a piece of a range and return the split regions.
    /// </summary>
    /// <param name="region">The region to break apart.</param>
    /// <param name="cut">The region to cut out.</param>
    /// <param name="output">The final list of regions after the split, excluding the cut piece.</param>
    /// <param name="intersectionRange">The range that caused the intersection between the cut and region.</param>
    /// <returns>The number of remaining pieces in the region.</returns>
    private static int RangeDifference(in TextureBarrierEntry region, in GorgonSubResourceRange cut, Span<GorgonSubResourceRange> output, out GorgonSubResourceRange intersectionRange)
    {
        int count = 0;

        GorgonSubResourceRange regionRange = region.Range.Equals(GorgonSubResourceRange.All) ? new(0, region.TextureMipCount, 0, region.TextureArrayCount, 0, region.TexturePlaneCount) : region.Range;
        GorgonSubResourceRange cutRange = cut.Equals(GorgonSubResourceRange.All) ? new(0, region.TextureMipCount, 0, region.TextureArrayCount, 0, region.TexturePlaneCount) : cut;

        if (!RangeIntersects(in regionRange, in cutRange))
        {
            intersectionRange = GorgonSubResourceRange.Empty;
            return -1;
        }

        int regionArrays = regionRange.FirstArrayIndex + regionRange.ArrayCount;
        int cutArrays = cutRange.FirstArrayIndex + cutRange.ArrayCount;
        int regionMips = regionRange.FirstMipLevel + regionRange.MipLevelCount;
        int cutMips = cutRange.FirstMipLevel + cutRange.MipLevelCount;
        int regionPlanes = regionRange.FirstPlane + regionRange.PlaneCount;
        int cutPlanes = cutRange.FirstPlane + cutRange.PlaneCount;

        int arrayStart = regionRange.FirstArrayIndex.Max(cutRange.FirstArrayIndex);
        int arrayEnd = regionArrays.Min(cutArrays);
        int mipStart = regionRange.FirstMipLevel.Max(cutRange.FirstMipLevel);
        int mipEnd = regionMips.Min(cutMips);
        int planeStart = regionRange.FirstPlane.Max(cutRange.FirstPlane);
        int planeEnd = regionPlanes.Min(cutPlanes);

        short overlapArrayCount = (short)(arrayEnd - arrayStart);
        short overlapMipCount = (short)(mipEnd - mipStart);
        byte overlapPlaneCount = (byte)(planeEnd - planeStart);

        if (regionRange.FirstArrayIndex < arrayStart)
        {
            output[count++] = new GorgonSubResourceRange(regionRange.FirstMipLevel, regionRange.MipLevelCount, regionRange.FirstArrayIndex, (short)(arrayStart - regionRange.FirstArrayIndex), regionRange.FirstPlane, regionRange.PlaneCount);
        }

        if (arrayEnd < regionArrays)
        {
            output[count++] = new GorgonSubResourceRange(regionRange.FirstMipLevel, regionRange.MipLevelCount, (short)arrayEnd, (short)(regionArrays - arrayEnd), regionRange.FirstPlane, regionRange.PlaneCount);
        }

        if (regionRange.FirstMipLevel < mipStart)
        {
            output[count++] = new GorgonSubResourceRange(regionRange.FirstMipLevel, (short)(mipStart - regionRange.FirstMipLevel), (short)arrayStart, overlapArrayCount, regionRange.FirstPlane, regionRange.PlaneCount);
        }

        if (mipEnd < regionMips)
        {
            output[count++] = new GorgonSubResourceRange((short)mipEnd, (short)(regionMips - mipEnd), (short)arrayStart, overlapArrayCount, regionRange.FirstPlane, regionRange.PlaneCount);
        }

        if (regionRange.FirstPlane < planeStart)
        {
            output[count++] = new GorgonSubResourceRange((short)mipStart, overlapMipCount, (short)arrayStart, overlapArrayCount, regionRange.FirstPlane, (byte)(planeStart - regionRange.FirstPlane));
        }

        if (planeEnd < regionPlanes)
        {
            output[count++] = new GorgonSubResourceRange((short)mipStart, overlapMipCount, (short)arrayStart, overlapArrayCount, (byte)planeEnd, (byte)(regionPlanes - planeEnd));
        }

        intersectionRange = new GorgonSubResourceRange((short)mipStart, overlapMipCount, (short)arrayStart, overlapArrayCount, (byte)planeStart, overlapPlaneCount);

        return count;
    }

    /// <summary>
    /// Function to queue up pending barriers across sub resources for a texture.
    /// </summary>
    /// <param name="newBarrier">The proposed barrier.</param>
    /// <param name="range">The sub resource range for the barrier.</param>
    /// <param name="barriers">The list of sub resource barriers for a given texture.</param>
    /// <param name="mipCount">The total number of mip levels on the texture.</param>
    /// <param name="arrayCount">The total number of array indices on the texture.</param>
    /// <param name="planeCount">The number of planes in the texture format.</param>
    private static void QueueSubResourceBarriers(ref readonly GorgonTextureBarrier newBarrier, ref readonly GorgonSubResourceRange range, List<TextureBarrierEntry> barriers, short mipCount, short arrayCount, byte planeCount)
    {
        Span<GorgonSubResourceRange> ranges = stackalloc GorgonSubResourceRange[6];

        for (int i = barriers.Count - 1; i >= 0; --i)
        {
            ref readonly TextureBarrierEntry entryRef = ref CollectionsMarshal.AsSpan(barriers)[i];
            int pieceCount = RangeDifference(in entryRef, in range, ranges, out _);

            // No overlap found. No need to touch the range.
            if (pieceCount == -1)
            {
                continue;
            }

            // Take a copy of this value before we scramble the guts of pending.
            TextureBarrierEntry entry = entryRef;

            int last = barriers.Count - 1;
            if (i < last)
            {
                barriers[i] = barriers[last];
            }

            barriers.RemoveAt(last);

            for (int j = 0; j < pieceCount; ++j)
            {
                barriers.Add(new TextureBarrierEntry(entry.Barrier, ranges[j], mipCount, arrayCount, planeCount));
            }
        }

        barriers.Add(new TextureBarrierEntry(newBarrier, range, mipCount, arrayCount, planeCount));
    }

    /// <summary>
    /// Function to constrain the sub resource values to the texture limits.
    /// </summary>
    /// <param name="texture">The texture used to determine limits.</param>
    /// <param name="requestedRange">The request range.</param>    
    /// <param name="formatPlaneCount">The number of planes in the texture format.</param>
    private static void ConstrainSubResource(GorgonTextureCommon texture, ref GorgonSubResourceRange requestedRange, byte formatPlaneCount)
    {
        if (requestedRange.FirstMipLevel == -1)
        {
            if (!requestedRange.Equals(in GorgonSubResourceRange.All))
            {
                requestedRange = GorgonSubResourceRange.All;
            }
            return;
        }

        short plane = requestedRange.FirstMipLevel.Min((short)(texture.MipCount - 1)).Max(0);
        short arrayIndex = requestedRange.FirstArrayIndex.Min((short)(texture.ArrayCount - 1)).Max(0);
        byte planeIndex = requestedRange.FirstPlane.Min((byte)(formatPlaneCount - 1)).Max(0);
        short mipCount = requestedRange.MipLevelCount.Min((short)(texture.MipCount - plane)).Max(1);
        short arrayCount = requestedRange.ArrayCount.Min((short)(texture.ArrayCount - arrayIndex)).Max(1);
        byte planeCount = requestedRange.PlaneCount.Min((byte)(formatPlaneCount - planeIndex)).Max(1);

        if ((plane == 0) && (arrayIndex == 0) && (planeIndex == 0) && (mipCount == texture.MipCount) && (arrayCount == texture.ArrayCount) && (planeCount == formatPlaneCount))
        {
            requestedRange = GorgonSubResourceRange.All;
            return;
        }

        if ((plane != requestedRange.FirstMipLevel) || (arrayIndex != requestedRange.FirstArrayIndex) || (planeIndex != requestedRange.FirstPlane)
            || (mipCount != requestedRange.MipLevelCount) || (arrayCount != requestedRange.ArrayCount) || (planeCount != requestedRange.PlaneCount))
        {
            requestedRange = new(plane, mipCount, arrayIndex, arrayCount, planeIndex, planeCount);
        }
    }

    /// <summary>
    /// Function to check the before layout state against what's allowed for a given queue.
    /// </summary>
    /// <param name="currentQueue">The queue that is executing.</param>
    /// <param name="layout">The before layout state.</param>
    /// <returns>The updated state.</returns>
    private BarrierLayout CheckQueueBeforeLayoutState(CommandQueue currentQueue, BarrierLayout layout)
    {
        if (currentQueue == _graphics.Queues.CopyQueue)
        {
            Debug.Assert(layout is BarrierLayout.None or BarrierLayout.Common, $"The before layout {layout} is not supported by the copy queue.");
        }

        if (currentQueue == _graphics.Queues.ComputeQueue)
        {
            Debug.Assert(layout is BarrierLayout.None or BarrierLayout.Common or BarrierLayout.GenericRead or BarrierLayout.ReadWrite or BarrierLayout.ShaderResource
                                                   or BarrierLayout.CopySource or BarrierLayout.CopyDestination or BarrierLayout.ComputeCommon or BarrierLayout.ComputeGenericRead
                                                   or BarrierLayout.ComputeReadWrite or BarrierLayout.ComputeShaderResource or BarrierLayout.ComputeCopySource
                                                   or BarrierLayout.ComputeCopyDestination or BarrierLayout.GraphicsQueueGenericReadFromCompute,
                                                   $"The before layout {layout} is not supported by the compute queue.");
        }

        return layout;
    }

    /// <summary>
    /// Function to check the before states against what's allowed for a given queue.
    /// </summary>
    /// <param name="currentQueue">The queue that is executing.</param>
    /// <param name="sync">The before sync state.</param>
    /// <param name="access">The access sync state.</param>
    /// <returns>The updated states.</returns>
    private (BarrierSync sync, BarrierAccess access) CheckQueueBeforeState(CommandQueue currentQueue, BarrierSync sync, BarrierAccess access)
    {
        // Check for legal copy queue values.        
        if (currentQueue == _graphics.Queues.CopyQueue)
        {
            if (((sync & ~LegalCopySyncMask) != 0) || ((access & ~LegalCopyAccessMask) != 0))
            {
                sync = BarrierSync.None;
                access = BarrierAccess.None;
            }
        }

        // Check for legal compute queue values.
        if (currentQueue == _graphics.Queues.ComputeQueue)
        {
            if (((sync & ~LegalComputeSyncMask) != 0) || ((access & ~LegalComputeAccessMask) != 0))
            {
                sync = BarrierSync.None;
                access = BarrierAccess.None;
            }
        }

        return (sync, access);
    }


    /// <summary>
    /// Function to populate the buffer barrier list.
    /// </summary>
    /// <param name="barriers">The barrier array to populate.</param>
    /// <param name="queue">The command queue this is executing on.</param>
    private void GatherBufferBarriers(D3D12_BUFFER_BARRIER* barriers, CommandQueue queue)
    {
        int index = 0;

        foreach (KeyValuePair<ulong, GorgonBufferBarrier> barrier in _pendingBuffers)
        {
            // If we get a null ref, then we'll let it die here. That means something got messed up when we allocated the barrier earlier and is a bug.
            ref GorgonBufferBarrier current = ref CollectionsMarshal.GetValueRefOrNullRef(_currentBuffers, barrier.Key);

            (BarrierSync sync, BarrierAccess access) = CheckQueueBeforeState(queue, current.Sync, current.Access);

            barriers[index++] = barrier.Value.ToD3DBufferBarrier(sync, access);
            current = barrier.Value;
        }

        _pendingBuffers.Clear();
    }

    /// <summary>
    /// Function to count all the sub resource ranges to return the total barrier count.
    /// </summary>
    /// <returns>The total number of barriers.</returns>
    private int CountTextureBarriers()
    {
        int result = 0;

        foreach (KeyValuePair<ulong, List<TextureBarrierEntry>> pending in _pendingTextures)
        {
            if (pending.Value.Count == 0)
            {
                continue;
            }

            ref readonly TextureBarrierEntry entry = ref CollectionsMarshal.AsSpan(pending.Value)[0];

            int totalSubResources = entry.TextureMipCount * entry.TextureArrayCount * entry.TexturePlaneCount;
            result += (pending.Value.Count * _currentTextures[pending.Key].Count).Min(totalSubResources);
        }

        return result;
    }

    /// <summary>
    /// Function to populate the texture barrier list.
    /// </summary>
    /// <param name="d3dBarriers">The barrier array to populate.</param>
    /// <param name="queue">The command queue this is executing on.</param>
    /// <returns>The actual count of texture sub resource barriers.</returns>
    private uint GatherTextureBarriers(D3D12_TEXTURE_BARRIER[] d3dBarriers, CommandQueue queue)
    {
        uint d3dBarrierCount = 0;
        Span<GorgonSubResourceRange> workingRanges = stackalloc GorgonSubResourceRange[6];

        foreach (KeyValuePair<ulong, List<TextureBarrierEntry>> barrierEntry in _pendingTextures)
        {
            // This has to be writable, so no by ref for us.
            List<TextureBarrierEntry> currentBarriers = _currentTextures[barrierEntry.Key];
            ReadOnlySpan<TextureBarrierEntry> pendingBarriers = CollectionsMarshal.AsSpan(barrierEntry.Value);
            ReadOnlySpan<TextureBarrierEntry> current = CollectionsMarshal.AsSpan(currentBarriers);

            for (int i = 0; i < pendingBarriers.Length; ++i)
            {
                ref readonly TextureBarrierEntry pendingEntry = ref pendingBarriers[i];

                for (int j = 0; j < current.Length; ++j)
                {
                    ref readonly TextureBarrierEntry currentEntry = ref current[j];

                    if ((currentEntry.Barrier.Equals(pendingEntry.Barrier)) && ((pendingEntry.Barrier.Access & WriteAccessMask) == BarrierAccess.None))
                    {
                        continue;
                    }

                    if (RangeDifference(in currentEntry, in pendingEntry.Range, workingRanges, out GorgonSubResourceRange intersectionRange) == -1)
                    {
                        continue;
                    }

                    (BarrierSync sync, BarrierAccess access) = CheckQueueBeforeState(queue, currentEntry.Barrier.Sync, currentEntry.Barrier.Access);
                    BarrierLayout layout = CheckQueueBeforeLayoutState(queue, currentEntry.Barrier.Layout);

                    D3D12_BARRIER_SUBRESOURCE_RANGE d3dRange = intersectionRange.ToD3DBarrierSubResourceRange();
                    d3dBarriers[d3dBarrierCount++] = pendingEntry.Barrier.ToD3DTextureBarrier(sync, access, layout, in d3dRange);
                }
            }

            for (int i = 0; i < pendingBarriers.Length; ++i)
            {
                ref readonly TextureBarrierEntry pendingEntry = ref pendingBarriers[i];
                QueueSubResourceBarriers(in pendingEntry.Barrier, in pendingEntry.Range, currentBarriers, pendingEntry.TextureMipCount, pendingEntry.TextureArrayCount, pendingEntry.TexturePlaneCount);
            }
        }

        DeallocateBarrierLists(_pendingTextures);

        return d3dBarrierCount;
    }

    /// <summary>
    /// Function to add a barrier for a buffer.
    /// </summary>
    /// <param name="buffer">The buffer resource to set a barrier on.</param>
    /// <param name="sync">The resource synchronization value.</param>
    /// <param name="access">The resource access level.</param>
    public void AddBarrier(GorgonGpuBufferCommon buffer, BarrierSync sync, BarrierAccess access)
    {
        GorgonBufferBarrier newBarrier = new(buffer, sync, access);

        ref GorgonBufferBarrier current = ref CollectionsMarshal.GetValueRefOrAddDefault(_currentBuffers, buffer.ResourceID, out bool exists);

        if (!exists)
        {
            current = newBarrier;
            _initialBuffers[buffer.ResourceID] = newBarrier;
            return;
        }

        // Redundant state. Writes are never redundant: the barrier makes later work wait for the write.
        if ((current.Equals(newBarrier)) && ((access & WriteAccessMask) == BarrierAccess.None))
        {
            _pendingBuffers.Remove(buffer.ResourceID);
            return;
        }

        _pendingBuffers[buffer.ResourceID] = newBarrier;
    }

    /// <summary>
    /// Function to add a barrier for a texture.
    /// </summary>
    /// <param name="commandList">The command list that the barrier is being set on.</param>
    /// <param name="texture">The texture resource to set a barrier on.</param>
    /// <param name="sync">The resource synchronization value.</param>
    /// <param name="access">The resource access level.</param>
    /// <param name="layout">The layout for the texture.</param>
    /// <param name="subResourceRange">[Optional] The subresources on the texture to apply the barrier to.</param>
    /// <param name="discard">[Optional] <b>true</b> to discard the resource content, <b>false</b> to leave as-is.</param>
    /// <exception cref="GorgonException">Thrown when the <paramref name="texture"/> was previously used and is in a state that is incompatible with the current object setting the barrier, or the texture was reset and used on the same command list.</exception>
    public void AddBarrier(GorgonCommandList commandList, GorgonTextureCommon texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResourceRange = null, bool discard = false)
    {
        static void CheckForResourceReuse(string textureName, string commandListName, BarrierSync sync, BarrierAccess access, List<TextureBarrierEntry> pendingBarriers, List<TextureBarrierEntry> currentBarriers)
        {
            List<TextureBarrierEntry> comparisonList = pendingBarriers.Count != 0 ? pendingBarriers : currentBarriers;

            if ((comparisonList.Count != 0)
                && ((sync != BarrierSync.None) || (access != BarrierAccess.None))
                && (comparisonList.All(b => b.Barrier.Sync == BarrierSync.None && b.Barrier.Access == BarrierAccess.None)))
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_RESOURCE_CANNOT_BE_USED_AGAIN, textureName, commandListName));
            }

        }

        static void QueueFullResourceBarrier(ref readonly GorgonTextureBarrier newBarrier, List<TextureBarrierEntry> pending, ReadOnlySpan<TextureBarrierEntry> current, short mipCount, short arrayCount, byte planeCount)
        {
            pending.Clear();

            if ((current.Length == 1) && (current[0].Barrier.Equals(newBarrier)) && ((newBarrier.Access & WriteAccessMask) == BarrierAccess.None))
            {
                return;
            }

            pending.Add(new TextureBarrierEntry(newBarrier, GorgonSubResourceRange.All, mipCount, arrayCount, planeCount));
        }

        GorgonTextureBarrier newBarrier = new(texture, sync, access, layout)
        {
            Discard = discard
        };

        byte formatPlaneCount = texture.Graphics.FormatSupport[texture.Format].PlaneCount;
        short arrayCount = texture.ArrayCount;
        short mipCount = texture.MipCount;
        GorgonSubResourceRange subResource = subResourceRange is null ? GorgonSubResourceRange.All : subResourceRange.Value;
        ConstrainSubResource(texture, ref subResource, formatPlaneCount);

        ref List<TextureBarrierEntry>? textureBarriers = ref CollectionsMarshal.GetValueRefOrAddDefault(_currentTextures, texture.ResourceID, out _);
        textureBarriers ??= AllocateList();

        if (textureBarriers.Count == 0)
        {
            textureBarriers.Add(new TextureBarrierEntry(newBarrier, GorgonSubResourceRange.All, mipCount, arrayCount, formatPlaneCount));
            _initialTextures[texture.ResourceID] = newBarrier;
            return;
        }

        ReadOnlySpan<TextureBarrierEntry> currentBarriers = CollectionsMarshal.AsSpan(textureBarriers);

        // For undefined textures, we should discard their contents on first go around.
        if ((currentBarriers.Length == 1) && (currentBarriers[0].Barrier.Layout is BarrierLayout.None))
        {
            newBarrier = newBarrier with
            {
                Discard = true
            };
            subResource = GorgonSubResourceRange.All;
        }
        else
        {
            // Cannot use discard without a layout of None.
            newBarrier = newBarrier with
            {
                Discard = false
            };
        }

        ref List<TextureBarrierEntry>? pendingBarriers = ref CollectionsMarshal.GetValueRefOrAddDefault(_pendingTextures, texture.ResourceID, out _);
        pendingBarriers ??= AllocateList();

        if (_graphics.IsInDebugMode)
        {
            // When in debug mode, ensure that we're not trying to reuse a texture sub resource that's already been reset (i.e. we're finished with that 
            // resource and never intend to use it on the same command list again).
            CheckForResourceReuse(texture.Name, commandList.Name, newBarrier.Sync, newBarrier.Access, pendingBarriers, textureBarriers);
        }

        if (subResource.Equals(in GorgonSubResourceRange.All))
        {
            QueueFullResourceBarrier(in newBarrier, pendingBarriers, currentBarriers, mipCount, arrayCount, formatPlaneCount);

            if (pendingBarriers.Count == 0)
            {
                _listPool.Push(pendingBarriers);
                _pendingTextures.Remove(texture.ResourceID);
            }
            return;
        }

        QueueSubResourceBarriers(in newBarrier, in subResource, pendingBarriers, mipCount, arrayCount, formatPlaneCount);
    }

    /// <summary>
    /// Function to resolve the initial barrier states for a resource.
    /// </summary>
    /// <param name="queue">The command queue that will be used to execute the command list.</param>
    /// <returns>A tuple containing the D3D12 buffer barriers, and the D3D12 texture barriers.</returns>
    public (ArraySegment<D3D12_BUFFER_BARRIER> BufferBarriers, ArraySegment<D3D12_TEXTURE_BARRIER> TextureBarriers) ResolveInitialStates(CommandQueue queue)
    {
        D3D12_BUFFER_BARRIER[] d3d12BufferBarriers = ArrayPool<D3D12_BUFFER_BARRIER>.Shared.Rent(_initialBuffers.Count);
        D3D12_TEXTURE_BARRIER[] d3d12TextureBarriers = [];
        int bufferCount = 0;
        int textureCount = 0;
        (BarrierSync Sync, BarrierAccess Access, BarrierLayout Layout, GorgonSubResourceRange Range) all = (BarrierSync.None, BarrierAccess.None, BarrierLayout.None, GorgonSubResourceRange.All);

        try
        {
            using (_globalState.Lock())
            {
                foreach (KeyValuePair<ulong, GorgonBufferBarrier> bufferBarrier in _initialBuffers)
                {
                    ref readonly GlobalBarrier globalBarrier = ref _globalState.GetState(bufferBarrier.Key);
                    ReadOnlySpan<(BarrierSync Sync, BarrierAccess Access, BarrierLayout, GorgonSubResourceRange)> barriers = CollectionsMarshal.AsSpan(globalBarrier.Barriers);
                    BarrierSync sync = barriers.IsEmpty ? BarrierSync.None : barriers[0].Sync;
                    BarrierAccess access = barriers.IsEmpty ? BarrierAccess.None : barriers[0].Access;

                    if ((sync == bufferBarrier.Value.Sync) && (access == bufferBarrier.Value.Access) && ((access & WriteAccessMask) == BarrierAccess.None))
                    {
                        continue;
                    }

                    (sync, access) = CheckQueueBeforeState(queue, sync, access);
                    d3d12BufferBarriers[bufferCount++] = bufferBarrier.Value.ToD3DBufferBarrier(sync, access);
                }

                int textureBarrierCount = 0;
                // Count global barriers for textures
                foreach (ulong resourceID in _initialTextures.Keys)
                {
                    ref readonly GlobalBarrier barrier = ref _globalState.GetState(resourceID);
                    textureBarrierCount += (barrier.Barriers?.Count ?? 0).Max(1);
                }

                d3d12TextureBarriers = ArrayPool<D3D12_TEXTURE_BARRIER>.Shared.Rent(textureBarrierCount);

                foreach (KeyValuePair<ulong, GorgonTextureBarrier> textureBarrier in _initialTextures)
                {
                    ref readonly GlobalBarrier globalBarrier = ref _globalState.GetState(textureBarrier.Key);
                    scoped ReadOnlySpan<(BarrierSync Sync, BarrierAccess Access, BarrierLayout, GorgonSubResourceRange)> barriers = CollectionsMarshal.AsSpan(globalBarrier.Barriers);

                    if (barriers.IsEmpty)
                    {
                        barriers = new ReadOnlySpan<(BarrierSync Sync, BarrierAccess Access, BarrierLayout, GorgonSubResourceRange)>(in all);
                    }

                    for (int i = 0; i < barriers.Length; ++i)
                    {
                        ref readonly (BarrierSync Sync, BarrierAccess Access, BarrierLayout Layout, GorgonSubResourceRange Range) barrier = ref barriers[i];

                        if ((barrier.Layout != BarrierLayout.Common) && (barrier.Layout != BarrierLayout.None) && (queue.Type != globalBarrier.QueueType))
                        {
                            throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_CROSS_QUEUE_BARRIER, textureBarrier.Key, queue.Type.ToGorgonObjectType(), globalBarrier.QueueType.ToGorgonObjectType()));
                        }

                        if ((barrier.Sync == textureBarrier.Value.Sync) && (barrier.Access == textureBarrier.Value.Access) && (barrier.Layout == textureBarrier.Value.Layout)
                            && ((barrier.Access & WriteAccessMask) == BarrierAccess.None))
                        {
                            continue;
                        }

                        (BarrierSync sync, BarrierAccess access) = CheckQueueBeforeState(queue, barrier.Sync, barrier.Access);
                        BarrierLayout layout = CheckQueueBeforeLayoutState(queue, barrier.Layout);

                        GorgonTextureBarrier finalBarrier = textureBarrier.Value with
                        {
                            Discard = layout == BarrierLayout.None
                        };

                        D3D12_BARRIER_SUBRESOURCE_RANGE subResRange = barrier.Range.ToD3DBarrierSubResourceRange();
                        d3d12TextureBarriers[textureCount++] = finalBarrier.ToD3DTextureBarrier(sync, access, layout, in subResRange);
                    }
                }

                return (new(d3d12BufferBarriers, 0, bufferCount), new(d3d12TextureBarriers, 0, textureCount));
            }
        }
        catch
        {
            ArrayPool<D3D12_BUFFER_BARRIER>.Shared.Return(d3d12BufferBarriers, true);
            ArrayPool<D3D12_TEXTURE_BARRIER>.Shared.Return(d3d12TextureBarriers, true);
            throw;
        }
    }

    /// <summary>
    /// Function to submit the barriers to the command list.
    /// </summary>
    /// <param name="list">The command list to append the barriers on.</param>
    public void Submit(GorgonCommandList list)
    {
        if (IsEmpty)
        {
            return;
        }

        uint groupCount = 0;
        uint actualTextureCount = 0;
        int expectedTetxureBarrierCount = CountTextureBarriers();
        int bufferBarrierCount = _pendingBuffers.Count;
        D3D12_BARRIER_GROUP* groups = stackalloc D3D12_BARRIER_GROUP[2];
        D3D12_TEXTURE_BARRIER[] d3dTextureBarriers = [];
        ArrayPool<D3D12_TEXTURE_BARRIER> pool = GorgonArrayPools<D3D12_TEXTURE_BARRIER>.GetBestPool(expectedTetxureBarrierCount);

        if (expectedTetxureBarrierCount > 0)
        {
            d3dTextureBarriers = pool.Rent(expectedTetxureBarrierCount);
        }

        try
        {
            if (_pendingBuffers.Count > 0)
            {
                D3D12_BUFFER_BARRIER* bufferBarriers = stackalloc D3D12_BUFFER_BARRIER[bufferBarrierCount];
                GatherBufferBarriers(bufferBarriers, list.Queue);
                groups[groupCount++] = new D3D12_BARRIER_GROUP((uint)bufferBarrierCount, bufferBarriers);
            }

            if (expectedTetxureBarrierCount > 0)
            {
                actualTextureCount = GatherTextureBarriers(d3dTextureBarriers, list.Queue);
            }

            if (actualTextureCount != 0)
            {
                fixed (D3D12_TEXTURE_BARRIER* barrierPtr = d3dTextureBarriers)
                {
                    groups[groupCount++] = new D3D12_BARRIER_GROUP(actualTextureCount, barrierPtr);
                    list.D3DGraphicsCommandList.Get()->Barrier(groupCount, groups);
                }
            }
            else if (groupCount != 0)
            {
                list.D3DGraphicsCommandList.Get()->Barrier(groupCount, groups);
            }
        }
        finally
        {
            pool.Return(d3dTextureBarriers, true);
        }
    }

    /// <summary>
    /// Function to copy the current internal state to the global state.
    /// </summary>
    /// <param name="queueType">The type of command queue used for the barrier states.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyCurrentToGlobal(D3D12_COMMAND_LIST_TYPE queueType)
    {
        using (_globalState.Lock())
        {
            _globalState.CaptureBufferState(queueType, _currentBuffers);
            _globalState.CaptureTextureState(queueType, _currentTextures);
        }
    }

    /// <summary>
    /// Function to clear the recorded barrier information.
    /// </summary>
    public void Clear()
    {
        _initialBuffers.Clear();
        _initialTextures.Clear();
        _currentBuffers.Clear();
        _pendingBuffers.Clear();

        DeallocateBarrierLists(_currentTextures);
        DeallocateBarrierLists(_pendingTextures);

        if (_listPool.Count > 16384)
        {
            _listPool.Clear();
            _listPool.TrimExcess(16384);
        }
    }
}