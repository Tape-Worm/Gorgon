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
/// <param name="globalState">The global resource barrier state tracker.</param>
internal unsafe class BarrierManager(GlobalBarrierState globalState)
{
    // The initial capacity of the sub resource barrier lists.
    private const int InitalSubResourceCapacity = 128;

    private readonly Dictionary<ulong, GorgonBufferBarrier> _currentBuffers = [];
    private readonly Dictionary<ulong, GorgonBufferBarrier> _pendingBuffers = [];
    private readonly Dictionary<ulong, GorgonTextureBarrier> _currentTextures = [];
    private readonly Dictionary<ulong, GorgonTextureBarrier> _pendingTextures = [];
    private readonly Dictionary<ulong, List<GorgonSubResourceRange>> _currentSubResources = [];
    private readonly Dictionary<ulong, List<GorgonSubResourceRange>> _pendingSubResources = [];
    private readonly GlobalBarrierState _globalState = globalState;

    /// <summary>
    /// Property to return whether there are any pending barriers.
    /// </summary>
    public bool IsEmpty => _pendingBuffers.Count == 0 && _pendingTextures.Count == 0;    

    /// <summary>
    /// Function to determine if this sub resource range for this key intersects with another key.
    /// </summary>
    /// <param name="subResource">The sub resource range to compare.</param>
    /// <param name="other">The other sub resource range.</param>
    /// <returns><b>true</b> if the key intersects with the sub resource range, <b>false</b> if not.</returns>
    private static bool Intersects(ref readonly GorgonSubResourceRange subResource, ref readonly GorgonSubResourceRange other)
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
    /// Function to merge overlapping sub resource ranges.
    /// </summary>
    /// <param name="subResource">The sub resource being used.</param>
    /// <param name="subResources">The list of sub resources to update.</param>
    private static void MergeSubResources(ref readonly GorgonSubResourceRange subResource, List<GorgonSubResourceRange> subResources)
    {
        if (subResources.Count == 0)
        {
            subResources.Add(subResource);
            return;
        }

        short minMipLevel = short.MaxValue;
        short minArrayIndex = short.MaxValue;
        byte minPlane = byte.MaxValue;
        short maxMipCount = short.MinValue;
        short maxArrayCount = short.MinValue;
        byte maxPlaneCount = 0;

        int i = 0;
        int mergeCount = 0;

        while (i < subResources.Count)
        {
            GorgonSubResourceRange pending = subResources[i];

            if (!Intersects(in subResource, in pending))
            {
                ++i;
                continue;
            }

            minMipLevel = minMipLevel.Min(pending.FirstMipLevel);
            minArrayIndex = minArrayIndex.Min(pending.FirstArrayIndex);
            minPlane = minPlane.Min(pending.FirstPlane);
            maxMipCount = maxMipCount.Max((short)(pending.FirstMipLevel + pending.MipLevelCount));
            maxArrayCount = maxArrayCount.Max((short)(pending.FirstArrayIndex + pending.ArrayCount));
            maxPlaneCount = maxPlaneCount.Max((byte)(pending.FirstPlane + pending.PlaneCount));

            subResources.RemoveAt(i);
            ++mergeCount;
        }

        // No barriers found to merge, so all we need to do is add our barrier.
        if (mergeCount == 0)
        {
            subResources.Add(subResource);
            return;
        }

        minMipLevel = minMipLevel.Min(subResource.FirstMipLevel);
        minArrayIndex = minArrayIndex.Min(subResource.FirstArrayIndex);
        minPlane = minPlane.Min(subResource.FirstPlane);
        maxMipCount = maxMipCount.Max((short)(subResource.FirstMipLevel + subResource.MipLevelCount));
        maxArrayCount = maxArrayCount.Max((short)(subResource.FirstArrayIndex + subResource.ArrayCount));
        maxPlaneCount = maxPlaneCount.Max((byte)(subResource.FirstPlane + subResource.PlaneCount));

        // Get the union of the barrier ranges.
        subResources.Add(new GorgonSubResourceRange(minMipLevel, (short)(maxMipCount - minMipLevel),
                                                         minArrayIndex, (short)(maxArrayCount - minArrayIndex),
                                                         minPlane, (byte)(maxPlaneCount - minPlane)));
    }

    /// <summary>
    /// Function to constrain the sub resource values to the texture limits.
    /// </summary>
    /// <param name="texture">The texture used to determine limits.</param>
    /// <param name="requestedRange">The request range.</param>    
    private static void ConstrainSubResource(GorgonTexture texture, ref GorgonSubResourceRange requestedRange)
    {
        byte formatPlaneCount = texture.Graphics.FormatSupport[texture.Format].PlaneCount;

        if (requestedRange.FirstMipLevel == -1)
        {
            if (!requestedRange.Equals(in GorgonSubResourceRange.All))
            {
                requestedRange = GorgonSubResourceRange.All;
            }
            return;
        }

        short mipLevel = requestedRange.FirstMipLevel.Min((short)(texture.MipCount - 1)).Max(0);
        short arrayIndex = requestedRange.FirstArrayIndex.Min((short)(texture.ArrayCount - 1)).Max(0);
        byte planeIndex = requestedRange.FirstPlane.Min((byte)(formatPlaneCount - 1)).Max(0);
        short mipCount = requestedRange.MipLevelCount.Min((short)(texture.MipCount - mipLevel)).Max(1);
        short arrayCount = requestedRange.ArrayCount.Min((short)(texture.ArrayCount - arrayIndex)).Max(1);
        byte planeCount = requestedRange.PlaneCount.Min((byte)(formatPlaneCount - planeIndex)).Max(1);

        if ((mipLevel == 0) && (arrayIndex == 0) && (planeIndex == 0) && (mipCount == texture.MipCount) && (arrayCount == texture.ArrayCount) && (planeCount == formatPlaneCount))
        {
            requestedRange = GorgonSubResourceRange.All;
            return;
        }

        if ((mipLevel != requestedRange.FirstMipLevel) || (arrayIndex != requestedRange.FirstArrayIndex) || (planeIndex != requestedRange.FirstPlane)
            || (mipCount != requestedRange.MipLevelCount) || (arrayCount != requestedRange.ArrayCount) || (planeCount != requestedRange.PlaneCount))
        {
            requestedRange = new(mipLevel, mipCount, arrayIndex, arrayCount, planeIndex, planeCount);
        }
    }


    /// <summary>
    /// Function to populate the buffer barrier list.
    /// </summary>
    /// <param name="barriers">The barrier array to populate.</param>
    private void GatherBufferBarriers(D3D12_BUFFER_BARRIER* barriers)
    {
        int index = 0;

        foreach (KeyValuePair<ulong, GorgonBufferBarrier> barrier in _pendingBuffers)
        {
            // If we get a null ref, then we'll let it die here. That means something got messed up when we allocated the barrier earlier and is a bug.
            ref GorgonBufferBarrier current = ref CollectionsMarshal.GetValueRefOrNullRef(_currentBuffers, barrier.Key);

            barriers[index++] = barrier.Value.ToD3DBufferBarrier(current.Sync, current.Access);
            current = barrier.Value;
        }

        _pendingBuffers.Clear();
    }

    /// <summary>
    /// Function to count all the sub resource ranges to return the total barrier count.
    /// </summary>
    /// <returns>The total number of barriers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTextureBarriers()
    {
        int result = 0;

        if (_pendingTextures.Count == 0)
        {
            return 0;
        }

        foreach (KeyValuePair<ulong, GorgonTextureBarrier> pending in _pendingTextures)
        {
            result += _pendingSubResources[pending.Key].Count;
        }

        return result;
    }

    /// <summary>
    /// Function to populate the texture barrier list.
    /// </summary>
    /// <param name="barriers">The barrier array to populate.</param>
    private void GatherTextureBarriers(D3D12_TEXTURE_BARRIER* barriers)
    {
        int index = 0;

        foreach (KeyValuePair<ulong, GorgonTextureBarrier> pending in _pendingTextures)
        {
            ref GorgonTextureBarrier current = ref CollectionsMarshal.GetValueRefOrNullRef(_currentTextures, pending.Key);

            Debug.Assert(!Unsafe.IsNullRef(in current), $"The current state for resource ID {pending.Key} is missing.");

            GorgonTextureBarrier pendingBarrier = pending.Value;

            // Only allow discard when moving from read -> write bits (access after). Doing read -> read + discard makes no sense.
            // Disable this for now and come back to it.
            /*
            if ((pendingBarrier.Discard) && (current.Layout != BarrierLayout.None))
            {
                pendingBarrier = pendingBarrier with
                {
                    Discard = false
                };
            }
            */

            Span<GorgonSubResourceRange> ranges = CollectionsMarshal.AsSpan(_pendingSubResources[pending.Key]);
            List<GorgonSubResourceRange> currentSubResources = _currentSubResources[pending.Key];
            currentSubResources.Clear();

            for (int i = 0; i < ranges.Length; ++i)
            {
                ref readonly GorgonSubResourceRange r = ref ranges[i];
                D3D12_BARRIER_SUBRESOURCE_RANGE range = r.ToD3DBarrierSubResourceRange();

                barriers[index++] = pendingBarrier.ToD3DTextureBarrier(current.Sync, current.Access, current.Layout, in range);
                currentSubResources.Add(r);
            }

            ranges.Clear();
            current = pendingBarrier;
        }

        _pendingTextures.Clear();
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
            ref readonly GlobalBarrier globalBarrier = ref _globalState.GetState(buffer.ResourceID);

            current = new(buffer, globalBarrier.Sync, globalBarrier.Access);
        }

        // Redundant state.
        if (current.Equals(newBarrier))
        {
            return;
        }

        ref GorgonBufferBarrier pending = ref CollectionsMarshal.GetValueRefOrAddDefault(_pendingBuffers, buffer.ResourceID, out exists);

        if ((exists) && (pending.Equals(newBarrier)))
        {
            return;
        }

        pending = newBarrier;
    }

    /// <summary>
    /// Function to add a barrier for a buffer.
    /// </summary>
    /// <param name="buffer">The buffer resource to set a barrier on.</param>
    /// <param name="sync">The resource synchronization value.</param>
    /// <param name="access">The resource access level.</param>
    [Obsolete("For old buffers.")]
    public void AddBarrier(GorgonGpuBuffer_OLDE buffer, BarrierSync sync, BarrierAccess access)
    {
        // If we're using a dynamic-per-frame buffer, then we cannot change its barrier.
        if (buffer.Usage == BufferUsage.DynamicPerFrame)
        {
            return;
        }

        GorgonBufferBarrier newBarrier = new(buffer, sync, access);

        ref GorgonBufferBarrier current = ref CollectionsMarshal.GetValueRefOrAddDefault(_currentBuffers, buffer.ResourceID, out bool exists);

        if (!exists)
        {
            ref readonly GlobalBarrier globalBarrier = ref _globalState.GetState(buffer.ResourceID);

            current = new(buffer, globalBarrier.Sync, globalBarrier.Access);
        }

        // Redundant state.
        if (current.Equals(newBarrier))
        {
            return;
        }

        ref GorgonBufferBarrier pending = ref CollectionsMarshal.GetValueRefOrAddDefault(_pendingBuffers, buffer.ResourceID, out exists);

        if ((exists) && (pending.Equals(newBarrier)))
        {
            return;
        }

        pending = newBarrier;
    }

    /// <summary>
    /// Function to add a barrier for a texture.
    /// </summary>
    /// <param name="texture">The texture resource to set a barrier on.</param>
    /// <param name="sync">The resource synchronization value.</param>
    /// <param name="access">The resource access level.</param>
    /// <param name="layout">The layout for the texture.</param>
    /// <param name="subResourceRange">[Optional] The subresources on the texture to apply the barrier to.</param>
    /// <param name="discard">[Optional] <b>true</b> to discard the resource content, <b>false</b> to leave as-is.</param>
    /// <returns><b>true</b> if a barrier was created, <b>false</b> if not.</returns>
    public bool AddBarrier(GorgonTexture texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResourceRange = null, bool discard = false)
    {
        GorgonTextureBarrier newBarrier = new(texture, sync, access, layout)
        {
            Discard = discard
        };

        GorgonSubResourceRange subResource = subResourceRange is null ? GorgonSubResourceRange.All : subResourceRange.Value;
        ConstrainSubResource(texture, ref subResource);

        ref GorgonTextureBarrier current = ref CollectionsMarshal.GetValueRefOrAddDefault(_currentTextures, texture.ResourceID, out bool exists);
        ref List<GorgonSubResourceRange>? currentSubResources = ref CollectionsMarshal.GetValueRefOrAddDefault(_currentSubResources, texture.ResourceID, out _);
        ref List<GorgonSubResourceRange>? pendingSubResources = ref CollectionsMarshal.GetValueRefOrAddDefault(_pendingSubResources, texture.ResourceID, out _);

        currentSubResources ??= [];
        pendingSubResources ??= [];

        if (!exists)
        {
            ref readonly GlobalBarrier global = ref _globalState.GetState(texture.ResourceID);
            current = new GorgonTextureBarrier(texture, global.Sync, global.Access, global.Layout);

            if (currentSubResources.Count > 0)
            {
                currentSubResources.Clear();
            }

            if (global.SubResources is not null)
            {
                currentSubResources.AddRange(global.SubResources);
            }
        }

        // Check to see if this sub resource change is redundant.
        if ((current.Equals(newBarrier)) && (currentSubResources.Contains(subResource)))
        {
            return false;
        }

        ref GorgonTextureBarrier pending = ref CollectionsMarshal.GetValueRefOrAddDefault(_pendingTextures, texture.ResourceID, out exists);

        if ((exists) && (pending.Equals(newBarrier)) && (pendingSubResources.Contains(subResource)))
        {
            return false;
        }

        pending = newBarrier;

        // If we've specified that the full resource is to be covered, then clear the current pending list.
        if (subResource.Equals(in GorgonSubResourceRange.All))
        {
            if (pendingSubResources.Count > 0)
            {
                pendingSubResources.Clear();
            }
            pendingSubResources.Add(subResource);
            return true;
        }

        // Merge sub resource ranges. We need to do this because sub resource ranges cannot overlap.
        MergeSubResources(in subResource, pendingSubResources);
        return true;
    }

    /// <summary>
    /// Function to submit the barriers to the command list.
    /// </summary>
    /// <param name="list">The command list to append the barriers on.</param>
    public void Submit(ref readonly ComPtr<ID3D12GraphicsCommandList10> list)
    {
        if (IsEmpty)
        {
            return;
        }

        uint groupCount = 0;
        int textureBarrierCount = CountTextureBarriers();
        int bufferBarrierCount = _pendingBuffers.Count;
        D3D12_BARRIER_GROUP* groups = stackalloc D3D12_BARRIER_GROUP[2];

        if (_pendingBuffers.Count > 0)
        {            
            D3D12_BUFFER_BARRIER* bufferBarriers = stackalloc D3D12_BUFFER_BARRIER[bufferBarrierCount];
            GatherBufferBarriers(bufferBarriers);
            groups[groupCount++] = new D3D12_BARRIER_GROUP((uint)bufferBarrierCount, bufferBarriers);
        }

        if (textureBarrierCount > 0)
        {
            D3D12_TEXTURE_BARRIER* textureBarriers = stackalloc D3D12_TEXTURE_BARRIER[textureBarrierCount];
            GatherTextureBarriers(textureBarriers);
            groups[groupCount++] = new D3D12_BARRIER_GROUP((uint)textureBarrierCount, textureBarriers);
        }

        list.Get()->Barrier(groupCount, groups);
    }

    /// <summary>
    /// Function to copy the current internal state to the global state.
    /// </summary>
    public void CopyCurrentToGlobal()
    {
        _globalState.CaptureBufferState(_currentBuffers);
        _globalState.CaptureTextureState(_currentTextures, _currentSubResources);
    }


    /// <summary>
    /// Function to clear the recorded barrier information.
    /// </summary>
    public void Clear()
    {
        _currentBuffers.Clear();
        _currentTextures.Clear();

        foreach (KeyValuePair<ulong, List<GorgonSubResourceRange>> subRes in _currentSubResources)
        {
            subRes.Value.Clear();            
        }

        if (_currentSubResources.Count > 4096)
        {
            _currentSubResources.Clear();
            _currentSubResources.TrimExcess();
        }

        _pendingTextures.Clear();
        _pendingBuffers.Clear();        

        foreach (KeyValuePair<ulong, List<GorgonSubResourceRange>> subRes in _pendingSubResources)
        {
            subRes.Value.Clear();
        }

        if (_pendingSubResources.Count > 4096)
        {
            _pendingSubResources.Clear();
            _pendingSubResources.TrimExcess();
        }
    }
}
