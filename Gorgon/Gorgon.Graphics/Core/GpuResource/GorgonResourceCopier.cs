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
// Created: January 16, 2026 2:28:28 PM
//

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality to copy data into a <see cref="GorgonGpuBuffer"/>, <see cref="GorgonIndexBuffer"/> or a <see cref="GorgonTextureCommon"/> from CPU memory on the GPU copy queue, or from other buffers/textures.
/// </summary>
/// <remarks>
/// <para>
/// This provides functionality for applications to write data into <see cref="GorgonGpuBuffer"/>, <see cref="GorgonIndexBuffer"/> or <see cref="GorgonTextureCommon"/> objects from CPU memory. It also provides 
/// functionality to read data from a <see cref="GorgonGpuBuffer"/> into standard CPU addressable memory like an array, <see cref="Span{T}"/>, or a <see cref="GorgonPtr{T}"/>.
/// </para>
/// <para>
/// The type provides a fluent interface that allows applications to chain multiple copy operations together by returning the <see cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}"/> interface from the <see cref="BeginUpload"/> method. 
/// This allows applications to write data from CPU addressable memory into the GPU.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGpuBufferCommon"/>
/// <seealso cref="GorgonTextureCommon"/>
/// <seealso cref="GorgonIndexBuffer"/>
/// <seealso cref="GorgonGpuBuffer"/>
/// <seealso cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}"/>
/// <seealso cref="GorgonPtr{T}"/>
public unsafe sealed class GorgonResourceCopier
    : IDisposable, IGorgonResourceWriter
{
    private static readonly string _cmdListName = $"{nameof(GorgonResourceCopier)} Command List (Copy Queue)";
    private GorgonCommandList? _commandList;
    private CommandAllocator? _commandAllocator;
    private readonly CommandQueue _commandQueue;
    private int _batchState;
    private bool _hasDelayedWrites;
    private readonly CpuResourceHeapPool _uploadHeaps;
    private readonly CpuResourceHeapPool _downloadHeaps;

    /// <summary>
    /// Property to return the graphics interface associated with this writer.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if ((_batchState != int.MaxValue) && (_commandList is not null))
            {
                _commandList.Close();
                _commandQueue.ListPool.Return(_commandList);
            }

            Cleanup();
        }
    }

    /// <summary>
    /// Function to prep the copier for delayed writing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining), MemberNotNull(nameof(_commandAllocator), nameof(_commandList))]
    private void PrepUpload()
    {
        _hasDelayedWrites = true;
        _commandAllocator ??= _commandQueue.AllocatorPool.Get(_cmdListName);
        _commandList ??= _commandQueue.ListPool.Get(_cmdListName, _commandAllocator);
    }

    /// <summary>
    /// Function to prep the copier for delayed writing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining), MemberNotNull(nameof(_commandAllocator), nameof(_commandList))]
    private void PrepDownload()
    {
        _hasDelayedWrites = false;
        _commandAllocator ??= _commandQueue.AllocatorPool.Get(_cmdListName);
        _commandList ??= _commandQueue.ListPool.Get(_cmdListName, _commandAllocator);
    }

    /// <summary>
    /// Function to reset the batch back to original state.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Cleanup()
    {
        _commandAllocator = null;
        _commandList = null;
        _batchState = 0;
        _hasDelayedWrites = false;
    }

    /// <summary>
    /// Function to write values to a buffer.
    /// </summary>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="data">The pointer to the data to write.</param>
    /// <param name="offset">The offset, in bytes, within the <paramref name="buffer"/> to start writing at.</param>
    /// <param name="count">The number of bytes to write.</param>
    private void WriteBuffer(GorgonGpuBufferCommon buffer, void* data, ulong offset, ulong count)
    {
        PrepUpload();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        
        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination, true);

        _uploadHeaps.Allocate(count, Graphics.Adapter.HasTightAlignmentSupport ? 0 : D3D12.D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, $"The transient heap for '{buffer.Name}' is not valid.");

        _commandQueue.Tracker.TrackResource(allocation.Heap.D3DResource);

        NativeMemory.Copy(data, allocation.CpuPointer, (nuint)count);
        
        _commandList.D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)buffer.D3DResource.Get(), buffer.ResourceOffset + offset, (PID3D12Resource2)allocation.Heap.D3DResource.Get(), allocation.Offset, count);
    }

    /// <summary>
    /// Function to validate the parameters for the functions in the writer.
    /// </summary>
    /// <param name="buffer">The buffer being written into.</param>
    /// <param name="offset">The offset, in bytes, within the buffer to start writing at.</param>
    /// <param name="count">The total number of items within the buffer.</param>
    /// <param name="typeSize">The size of an individual item, in bytes, within the buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="offset"/>, is less than 0.</para>
    /// <para>Thrown if the <paramref name="count"/> is less than 0.</para>
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the <paramref name="count"/> is greater than the <see cref="GorgonGpuBufferCommon.SizeInBytes">size</see> if the buffer.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateRangeParams(GorgonGpuBufferCommon buffer, long offset, long count, int typeSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        long byteSize = count * typeSize;

        Debug.Assert(byteSize > 0, "The size in bytes of the write range is 0.");

        if (offset + byteSize > buffer.SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, offset, byteSize, buffer.SizeInBytes));
        }
    }

    /// <summary>
    /// Function to copy a 1D texture into another texture.
    /// </summary>
    /// <param name="source">The 1D texture to copy.</param>
    /// <param name="destination"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destination']"/></param>
    /// <param name="sourceRegion">The horizontal range on the texture to copy.</param>
    /// <param name="sourceArrayIndex">The index in the texture array to copy.</param>
    /// <param name="sourceMipLevel"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourceMipLevel']"/></param>
    /// <param name="sourcePlane"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourcePlane']"/></param>
    /// <param name="destinationX"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationX']"/></param>
    /// <param name="destinationY"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationY']"/></param>
    /// <param name="destinationZOrArrayIndex"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationZOrArrayIndex']"/></param>
    /// <param name="destinationMipLevel"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationMipLevel']"/></param>
    /// <param name="destinationPlane"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationPlane']"/></param>
    /// <param name="isFullSubResource"><b>true</b> if the full sub resource is being copied, <b>false</b> if not.</param>
    private void Copy1DTexture(GorgonTexture source, GorgonTexture destination, GorgonRange<int> sourceRegion, short sourceArrayIndex, short sourceMipLevel, byte sourcePlane, int destinationX, int destinationY, short destinationZOrArrayIndex, short destinationMipLevel, byte destinationPlane, bool isFullSubResource)
    {
        uint sourceSubIndex = (uint)source.GetSubResourceIndex(sourceMipLevel, sourceArrayIndex, sourcePlane);
        uint destSubIndex = (uint)destination.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);

        if ((ReferenceEquals(source, destination)) && (sourceSubIndex == destSubIndex))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_INTO_ITSELF, source.Name, sourceSubIndex));
        }

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepUpload();

        _commandQueue.Tracker.TrackResource(source.D3DResource);
        _commandQueue.Tracker.TrackResource(destination.D3DResource);

        GorgonSubResourceRange srcSubRange = new((short)sourceMipLevel, 1, (short)sourceArrayIndex, 1, (byte)sourcePlane, 1);
        GorgonSubResourceRange destSubRange = new((short)destinationMipLevel, 1, (short)destinationZOrArrayIndex, 1, (byte)destinationPlane, 1);

        _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, srcSubRange);
        _commandList.SetBarrier(destination, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, destSubRange, force: true);

        D3D12_BOX d3dBox = new(sourceRegion.Minimum, 0, 0, sourceRegion.Maximum, 1, 1);

        D3D12_TEXTURE_COPY_LOCATION srcLoc = new(srcRes, sourceSubIndex);
        D3D12_TEXTURE_COPY_LOCATION destLoc = new(destRes, destSubIndex);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, (uint)destinationX, (uint)destinationY, (uint)(destination.Type == TextureType.Texture3D ? destinationZOrArrayIndex : 0), &srcLoc, isFullSubResource ? null : &d3dBox);
    }

    /// <summary>
    /// Function to copy a 2D texture into another texture.
    /// </summary>
    /// <param name="source">The 2D texture to copy.</param>
    /// <param name="destination"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destination']"/></param>
    /// <param name="sourceRegion">The rectangular range on the texture to copy.</param>
    /// <param name="sourceArrayIndex"><inheritdoc cref="Copy1DTexture" path="/param[@name='sourceArrayIndex']"/></param>
    /// <param name="sourceMipLevel"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourceMipLevel']"/></param>
    /// <param name="sourcePlane"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourcePlane']"/></param>
    /// <param name="destinationX"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationX']"/></param>
    /// <param name="destinationY"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationY']"/></param>
    /// <param name="destinationZOrArrayIndex"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationZOrArrayIndex']"/></param>
    /// <param name="destinationMipLevel"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationMipLevel']"/></param>
    /// <param name="destinationPlane"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationPlane']"/></param>
    /// <param name="isFullSubResource"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='isFullSubResource']"/></param>
    private void Copy2DTexture(GorgonTexture source, GorgonTexture destination, GorgonRectangle sourceRegion, short sourceArrayIndex, short sourceMipLevel, byte sourcePlane, int destinationX, int destinationY, short destinationZOrArrayIndex, short destinationMipLevel, byte destinationPlane, bool isFullSubResource)
    {
        uint sourceSubIndex = (uint)source.GetSubResourceIndex(sourceMipLevel, sourceArrayIndex, sourcePlane);
        uint destSubIndex = (uint)destination.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);

        if ((ReferenceEquals(source, destination)) && (sourceSubIndex == destSubIndex))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_INTO_ITSELF, source.Name, sourceSubIndex));
        }

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepUpload();

        _commandQueue.Tracker.TrackResource(source.D3DResource);
        _commandQueue.Tracker.TrackResource(destination.D3DResource);

        GorgonSubResourceRange srcSubRange = new((short)sourceMipLevel, 1, (short)sourceArrayIndex, 1, (byte)sourcePlane, 1);
        GorgonSubResourceRange destSubRange = new((short)destinationMipLevel, 1, (short)destinationZOrArrayIndex, 1, (byte)destinationPlane, 1);

        _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, srcSubRange);
        _commandList.SetBarrier(destination, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, destSubRange, force: true);

        D3D12_BOX d3dBox = new(sourceRegion.Left, sourceRegion.Top, 0, sourceRegion.Right, sourceRegion.Bottom, 1);

        D3D12_TEXTURE_COPY_LOCATION srcLoc = new(srcRes, sourceSubIndex);
        D3D12_TEXTURE_COPY_LOCATION destLoc = new(destRes, destSubIndex);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, (uint)destinationX, (uint)destinationY, (uint)(destination.Type == TextureType.Texture3D ? destinationZOrArrayIndex : 0), &srcLoc, isFullSubResource ? null : &d3dBox);
    }

    /// <summary>
    /// Function to copy a 3D texture into another texture.
    /// </summary>
    /// <param name="source">The 3D texture to copy.</param>
    /// <param name="destination"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destination']"/></param>
    /// <param name="sourceRegion">The box range on the texture to copy.</param>
    /// <param name="sourceMipLevel"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourceMipLevel']"/></param>
    /// <param name="sourcePlane"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourcePlane']"/></param>
    /// <param name="destinationX"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationX']"/></param>
    /// <param name="destinationY"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationY']"/></param>
    /// <param name="destinationZOrArrayIndex"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationZOrArrayIndex']"/></param>
    /// <param name="destinationMipLevel"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationMipLevel']"/></param>
    /// <param name="destinationPlane"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationPlane']"/></param>
    /// <param name="isFullSubResource"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='isFullSubResource']"/></param>
    private void Copy3DTexture(GorgonTexture source, GorgonTexture destination, GorgonBox sourceRegion, short sourceMipLevel, byte sourcePlane, int destinationX, int destinationY, short destinationZOrArrayIndex, short destinationMipLevel, byte destinationPlane, bool isFullSubResource)
    {
        uint sourceSubIndex = (uint)source.GetSubResourceIndex(sourceMipLevel, 0, sourcePlane);
        uint destSubIndex = (uint)destination.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);

        if ((ReferenceEquals(source, destination)) && (sourceSubIndex == destSubIndex))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_INTO_ITSELF, source.Name, sourceSubIndex));
        }

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepUpload();

        _commandQueue.Tracker.TrackResource(source.D3DResource);
        _commandQueue.Tracker.TrackResource(destination.D3DResource);

        GorgonSubResourceRange srcSubRange = new((short)sourceMipLevel, 1, 0, 1, (byte)sourcePlane, 1);
        GorgonSubResourceRange destSubRange = new((short)destinationMipLevel, 1, (short)destinationZOrArrayIndex, 1, (byte)destinationPlane, 1);
                
        _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, srcSubRange);
        _commandList.SetBarrier(destination, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, destSubRange, force: true);

        D3D12_BOX d3dBox = new(sourceRegion.Left, sourceRegion.Top, sourceRegion.Front, sourceRegion.Right, sourceRegion.Bottom, sourceRegion.Back);

        D3D12_TEXTURE_COPY_LOCATION srcLoc = new(srcRes, sourceSubIndex);
        D3D12_TEXTURE_COPY_LOCATION destLoc = new(destRes, destSubIndex);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, (uint)destinationX, (uint)destinationY, (uint)(destination.Type == TextureType.Texture3D ? destinationZOrArrayIndex : 0), &srcLoc, isFullSubResource ? null : &d3dBox);
    }

    /// <summary>
    /// Function to clip the sub resource copy parameters to avoid overflow.
    /// </summary>
    /// <param name="source">The texture to copy.</param>
    /// <param name="destination">The texture that will receive the data.</param>
    /// <param name="parameters">The parameters for the sub resource to copy.</param>
    /// <param name="sourceIsFullSubResource">If the parameterss covers the entire resource, then this value will return <b>true</b>; otherwise <b>false</b>.</param>
    /// <returns>The updated and clipped copy parameters.</returns>
    private GorgonCopyTextureSubResource Clip(GorgonTexture source, GorgonTexture destination, ref readonly GorgonCopyTextureSubResource parameters, out bool sourceIsFullSubResource)
    {
        GorgonBox srcDims = new(0, 0, 0, source.GetMipWidth(parameters.SourceMipLevel), source.GetMipHeight(parameters.SourceMipLevel), source.Type == TextureType.Texture3D ? source.GetMipDepth(parameters.SourceMipLevel) : 1);
        GorgonBox destDims = new(0, 0, 0, destination.GetMipWidth(parameters.DestinationMipLevel), destination.GetMipHeight(parameters.DestinationMipLevel), destination.Type == TextureType.Texture3D ? destination.GetMipDepth(parameters.DestinationMipLevel) : 1);
        int maxPlaneCount = Graphics.FormatSupport[source.Format].PlaneCount;

        short sourceMipLevel = parameters.SourceMipLevel.Min((short)(source.MipCount - 1)).Max(0);
        short destinationMipLevel = parameters.DestinationMipLevel.Min((short)(destination.MipCount - 1)).Max(0);
        byte sourcePlane = parameters.SourcePlane.Min((byte)(maxPlaneCount - 1)).Max(0);
        byte destinationPlane = parameters.DestinationPlane.Min((byte)(maxPlaneCount - 1)).Max(0);
        GorgonBox sourceRegion = parameters.SourceRegion.IsEmpty ? srcDims : parameters.SourceRegion;

        sourceIsFullSubResource = sourceRegion.Equals(in srcDims);

        if ((parameters.DestinationX >= destDims.Width) || (parameters.DestinationY >= destDims.Height) || (parameters.DestinationZOrArrayIndex >= destDims.Depth) || ((destination.Type != TextureType.Texture3D) && (parameters.DestinationZOrArrayIndex < 0)))
        {            
            return GorgonCopyTextureSubResource.Empty;
        }        

        GorgonBox destRegion = new(parameters.DestinationX, parameters.DestinationY, parameters.DestinationZOrArrayIndex, sourceRegion.Width, sourceRegion.Height, sourceRegion.Depth);
        GorgonBox.Intersect(in sourceRegion, in srcDims, out sourceRegion);
        GorgonBox.Intersect(in destRegion, in destDims, out destRegion);

        if (parameters.DestinationX < 0)
        {
            sourceRegion.X -= parameters.DestinationX;
            sourceRegion.Width += parameters.DestinationX;
        }

        if ((source.Type != TextureType.Texture1D) && (parameters.DestinationY < 0))
        {
            sourceRegion.Y -= parameters.DestinationY;
            sourceRegion.Height += parameters.DestinationY;
        }

        if ((source.Type == TextureType.Texture3D) && (parameters.DestinationZOrArrayIndex < 0))
        {
            sourceRegion.Z -= parameters.DestinationZOrArrayIndex;
            sourceRegion.Depth += parameters.DestinationZOrArrayIndex;
        }

        int destinationX = destRegion.X;
        int destinationY = destination.Type != TextureType.Texture1D ? destRegion.Y : 0;
        short destinationZOrArrayIndex = (short)destRegion.Z;

        sourceRegion.Width = sourceRegion.Width.Min(destRegion.Width).Max(0);
        sourceRegion.Height = sourceRegion.Height.Min(destRegion.Height).Max(0);
        sourceRegion.Depth = sourceRegion.Depth.Min(destRegion.Depth).Max(0);

        return new GorgonCopyTextureSubResource
        {
            SourceRegion = sourceRegion,
            SourceMipLevel = sourceMipLevel,
            SourcePlane = sourcePlane,
            DestinationX = destinationX,
            DestinationY = destinationY,
            DestinationZOrArrayIndex = destinationZOrArrayIndex,
            DestinationMipLevel = destinationMipLevel,
            DestinationPlane = destinationPlane,
        };
    }

    /// <summary>
    /// Function to validate the settings for copying a texture to another.
    /// </summary>
    /// <param name="source"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/param[@name='source']"/></param>
    /// <param name="destination"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/param[@name='destination']"/></param>
    /// <param name="parameters"><inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/param[@name='parameters']"/></param>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)" path="/exception"/>
    private static void ValidateCopyTexture(GorgonTexture source, GorgonTexture destination, ref readonly GorgonCopyTextureSubResource parameters)
    {
        if (source.FormatInfo.Group != destination.FormatInfo.Group)
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_FORMAT_GROUPS_DIFFERENT, source.Name, source.FormatInfo.Group, destination.Name, destination.FormatInfo.Group));
        }

        bool isSourceEmpty;

        if (!destination.MultisampleInfo.Equals(source.MultisampleInfo))
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_ERR_MULTISAMPLE_SOURCE_DEST_DIFFERENT, source.MultisampleInfo, source.Name, destination.MultisampleInfo, destination.Name));
        }

        if (source.Type != TextureType.Texture3D)
        {
            GorgonRectangle sourceRect = (GorgonRectangle)parameters.SourceRegion;

            isSourceEmpty = sourceRect.IsEmpty;
        }
        else
        {
            isSourceEmpty = parameters.SourceRegion.IsEmpty;
        }

        if ((parameters.DestinationZOrArrayIndex == 0) && (parameters.DestinationX == 0) && (parameters.DestinationY == 0) && (!isSourceEmpty))
        {
            return;
        }

        if (source.IsDepthStencil)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DEPTH_STENCIL_CANNOT_BE_COPIED, source.Name), nameof(source));
        }

        if (!source.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_MULTISAMPLE_CANNOT_BE_COPIED, source.Name, source.MultisampleInfo), nameof(source));
        }

        if (destination.IsDepthStencil)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DEPTH_STENCIL_CANNOT_BE_COPIED, destination.Name), nameof(destination));
        }

        if (!destination.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_MULTISAMPLE_CANNOT_BE_COPIED, destination.Name, destination.MultisampleInfo), nameof(destination));
        }
    }

    /// <summary>
    /// Function to execute the download command list.
    /// </summary>
    private void ExecuteDownload()
    {
        Debug.Assert(_commandList is not null, "No command list to execute the download.");

        ulong fence = 0;
        CommandQueue copyQueue = Graphics.Queues.CopyQueue;

        // Finalize the command.
        try
        {
            _commandList.D3DGraphicsCommandList.Get()->Close();

            copyQueue.Execute(_commandList);
            fence = copyQueue.IncrementFence();

            copyQueue.WaitForFence(fence, Timeout.Infinite);
        }
        finally
        {
            _downloadHeaps.Signal();
            copyQueue.Tracker.Signal();
            copyQueue.AllocatorPool.Signal();
            copyQueue.ListPool.Return(_commandList);
        }
    }

    /// <summary>
    /// Function to perform a download of data from a GPU buffer to a pointer.
    /// </summary>
    /// <param name="buffer"><inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/param[@name='buffer']"/></param>
    /// <param name="destination">The destination pointer.</param>
    /// <param name="offset">The offset within the source buffer to start reading from.</param>
    /// <param name="typeSize">The sized of an element in memory.</param>
    /// <param name="length">The number items to copy.</param>
    private void CopyDownloadData(GorgonGpuBufferCommon buffer, void* destination, ulong offset, uint typeSize, ulong length)
    {
        ulong sizeInBytes = ((ulong)buffer.SizeInBytes - offset).Min(typeSize * length);

        // Grab some temporary memory from our download heap.
        _downloadHeaps.Allocate(sizeInBytes.Max(16), Graphics.Adapter.HasTightAlignmentSupport ? 0 : D3D12.D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, "The returned resource heap allocation is not valid.");

        _commandQueue.Tracker.Signal();


        PrepDownload();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);

        // Do the copy.
        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopySource, true);

        _commandList.D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)allocation.Heap.D3DResource.Get(),
            allocation.Offset,
            (PID3D12Resource2)buffer.D3DResource.Get(),
            buffer.ResourceOffset + offset,
            sizeInBytes);

        // This synchronously downloads the data from the GPU. It waits until the GPU is done with its
        // work before returning.  This safely allows us to capture the data into the destination.
        // TODO: Perhaps make an Async version? 
        ExecuteDownload();

        NativeMemory.Copy(allocation.CpuPointer, destination, (nuint)sizeInBytes);

        Cleanup();
    }

    /// <summary>
    /// Function begin a batch upload to copy data from CPU memory into to GPU resources.
    /// </summary>
    /// <returns>A <see cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}"/> fluent interface.</returns>
    /// <exception cref="GorgonException">Thrown if the method has been called already.</exception>
    /// <remarks>
    /// <para>
    /// Applications must call this before writing data to a <see cref="GorgonGpuBufferCommon"/>. When finished writing data, the application must then call the <see cref="IGorgonResourceWriter.End"/> method. 
    /// </para>
    /// <para>
    /// This method returns a <see cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}"/> interface that allows an application to perform multiple writes across multiple buffers. Applications can use these write operations in 
    /// multi-threaded operations to allow data uploads to buffers simultaneously. This allows for taking advantage of the parallelism provided by the GPU and CPU.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBufferCommon"/>
    /// <seealso cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}"/>
    public IGorgonResourceWriter BeginUpload()
    {        
        if (Interlocked.Exchange(ref _batchState, 1) != 0)
        {
            throw new GorgonException(GorgonResult.CannotInitialize, Resources.GORGFX_ERR_BATCH_STARTED);
        }

        return this;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    Task IGorgonResourceWriter.EndAsync()
    {
        if ((_batchState == int.MaxValue) || (Interlocked.CompareExchange(ref _batchState, 2, 1) != 1))
        {
            return Task.CompletedTask;
        }

        if (!_hasDelayedWrites)
        {            
            Cleanup();
            return Task.CompletedTask;
        }

        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");

        ulong fence = 0;

        unsafe
        {
            _commandList.Close();

            _commandQueue.Execute(_commandList);

            fence = _commandQueue.IncrementFence();
        }

        // Spin up a background task to wait for the GPU.
        return Task.Run(() =>
        {
            try
            {
                _commandQueue.WaitForFence(fence, Timeout.Infinite);
            }
            finally
            {
                _commandQueue.Tracker.Signal();
                _commandQueue.AllocatorPool.Signal();
                _uploadHeaps.Signal();
                _commandQueue.ListPool.Return(_commandList);
                Cleanup();
            }
        });
    }

    /// <inheritdoc/>
    void IGorgonResourceWriter.End()
    {
        if ((_batchState == int.MaxValue) || (Interlocked.CompareExchange(ref _batchState, 2, 1) != 1))
        {
            return;
        }

        if (!_hasDelayedWrites)
        {
            Cleanup();
            return;
        }

        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");

        _commandList.Close();
        _commandQueue.Execute(_commandList);
        ulong fence = _commandQueue.IncrementFence();

        try
        {            
            _commandQueue.WaitForFence(fence, Timeout.Infinite);
        }
        finally
        {
            _commandQueue.Tracker.Signal();
            _commandQueue.AllocatorPool.Signal();
            _uploadHeaps.Signal();
            _commandQueue.ListPool.Return(_commandList);
            Cleanup();            
        }        
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.SetBarrier(GorgonTextureCommon texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources, bool discard, bool force)
    {
        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");
        _commandList.SetBarrier(texture, sync, access, layout, subResources, discard, force);
        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.SetBarrier(GorgonGpuBufferCommon buffer, BarrierSync sync, BarrierAccess access, bool force)
    {
        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");
        _commandList.SetBarrier(buffer, sync, access, force);
        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyValue<Tv>(in Tv value, GorgonGpuBufferCommon buffer, long offset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        int typeSize = sizeof(Tv);

        ValidateRangeParams(buffer, offset, 1, typeSize);

        fixed (Tv* valuePtr = &value)
        {
            WriteBuffer(buffer, (byte*)valuePtr, (ulong)offset, (ulong)typeSize);
        }

        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyRange<Tv>(ReadOnlySpan<Tv> values, GorgonGpuBufferCommon buffer, long offset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if (values.IsEmpty)
        {
            return this;
        }

        int typeSize = sizeof(Tv);

        ValidateRangeParams(buffer, offset, values.Length, typeSize);

        ulong size = (ulong)(values.Length * typeSize);

        fixed (Tv* pointer = values)
        {
            WriteBuffer(buffer, pointer, (ulong)offset, size);
        }

        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyPointer<Tv>(GorgonPtr<Tv> pointer, GorgonGpuBufferCommon buffer, long offset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if (pointer.Equals(GorgonPtr<Tv>.NullPtr))
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        if (pointer.Length == 0)
        {
            return this;
        }

        ValidateRangeParams(buffer, offset, pointer.Length, pointer.TypeSize);
        WriteBuffer(buffer, (void*)pointer, (ulong)offset, (ulong)pointer.SizeInBytes);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyBuffer(GorgonGpuBufferCommon source, GorgonGpuBufferCommon destination, long sourceOffset, long destinationOffset, long? count)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        long sizeToCopy = count ?? (source.SizeInBytes - sourceOffset).Min(destination.SizeInBytes - destinationOffset);        

        ArgumentOutOfRangeException.ThrowIfLessThan(sourceOffset, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(destinationOffset, 0);

        if (count is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count.Value, 1, nameof(count));
        }

        if (sizeToCopy < 1)
        {
            return this;
        }

        if (sourceOffset + sizeToCopy > source.SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, sourceOffset, sizeToCopy, source.SizeInBytes), nameof(source));
        }

        if (destinationOffset + sizeToCopy > destination.SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, destinationOffset, sizeToCopy, destination.SizeInBytes), nameof(destination));
        }

        PrepUpload();

        _commandQueue.Tracker.TrackResource(source.D3DResource);
        _commandQueue.Tracker.TrackResource(destination.D3DResource);

        // If the buffers are pointing to the same resource (as in the mega buffer), then we need a combination of states.
        if (source.ResourceID == destination.ResourceID)
        {
            _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource | BarrierAccess.CopyDestination, true);
        }
        else
        {
            _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource);
            _commandList.SetBarrier(destination, BarrierSync.Copy, BarrierAccess.CopyDestination, true);
        }

        _commandList.D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)destination.D3DResource.Get(),
            destination.ResourceOffset + (ulong)destinationOffset,
            (PID3D12Resource2)source.D3DResource.Get(),
            source.ResourceOffset + (ulong)sourceOffset,
            (ulong)sizeToCopy);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyImageToTexture(IGorgonImage image, GorgonTexture texture)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        IGorgonImage working = image;

        try
        {
            if (!texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_IMAGE_TO_MULTISAMPLE_TEXTURE, texture.Name, texture.MultisampleInfo), nameof(texture));
            }

            if (texture.IsDepthStencil)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_IMAGE_TO_DEPTH_STENCIL, texture.Name), nameof(texture));
            }

            if (texture.Format != image.Format)
            {
                if (image.CanConvertToFormat(texture.Format))
                {
                    Graphics.Log.Print($"The image format ({image.Format}) is different from the texture '{texture.Name}' format ({texture.Format}). Image will be converted to match the texture format.", LoggingLevel.Verbose);

                    working = image.Copy()
                                   .BeginUpdate()
                                   .ConvertToFormat(texture.Format)
                                   .EndUpdate();
                }
                else
                {
                    throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_WITH_IMAGE_FORMAT, image.Format, texture.Format));
                }
            }

            PrepUpload();

            _commandQueue.Tracker.TrackResource(texture.D3DResource);

            _uploadHeaps.Allocate((ulong)working.SizeInBytes, texture.Info.Alignment, out CpuBufferAllocation allocation);
            Debug.Assert(allocation.IsAvailable, $"The returned resource heap allocation is not valid.");

            _commandQueue.Tracker.TrackResource(allocation.Heap.D3DResource);

            // Copy to upload resource.
            NativeMemory.Copy((void*)working.ImageData, allocation.CpuPointer, (nuint)working.SizeInBytes);

            _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

            ulong offsetCalc = allocation.Offset;
            int arrayCount = working.ArrayCount.Min(texture.ArrayCount);
            int mipCount = working.MipCount.Min(texture.MipCount);
            int depth = working.Depth.Min(texture.Depth);

            // Copy to final resource.
            for (int a = 0; a < arrayCount; ++a)
            {
                for (int m = 0; m < mipCount; ++m)
                {
                    // Planes are not supported by IGorgonImage, so leave at 0.
                    GorgonSubResourceInfo subResource = texture.SubResources[texture.GetSubResourceIndex((short)m, (short)a, 0)];
                    IGorgonImageBuffer buffer = working.Buffers[m, a];

                    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
                    {
                        Offset = offsetCalc,
                        Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)working.Format, (uint)subResource.Width, (uint)subResource.Height, (uint)depth, (uint)buffer.PitchInformation.RowPitch)
                    };
                    D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);
                    D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)subResource.SubResourceIndex);

                    _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, 0, &src, null);

                    offsetCalc += (ulong)(buffer.SizeInBytes * depth);
                    depth = (depth >> 1).Max(1);
                }
            }

            return this;
        }
        finally
        {
            if (!ReferenceEquals(working, image))
            {
                working?.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonTexture texture, short destinationMipLevel, short destinationZOrArrayIndex, byte destinationPlane)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if (!texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_IMAGE_TO_MULTISAMPLE_TEXTURE, texture.Name, texture.MultisampleInfo), nameof(texture));
        }

        if (texture.IsDepthStencil)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_IMAGE_TO_DEPTH_STENCIL, texture.Name), nameof(texture));
        }

        if (texture.Format != imageBuffer.Format)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_WITH_IMAGE_FORMAT, imageBuffer.Format, texture.Format));
        }

        int maxPlaneCount = Graphics.FormatSupport[texture.Format].PlaneCount;

        destinationZOrArrayIndex = texture.Type == TextureType.Texture3D ? destinationZOrArrayIndex.Min((short)(texture.Depth - 1)).Max(0) : destinationZOrArrayIndex.Min((short)(texture.ArrayCount - 1)).Max(0);
        destinationMipLevel = destinationMipLevel.Min((short)(texture.MipCount - 1)).Max(0);
        destinationPlane = destinationPlane.Min((byte)(maxPlaneCount - 1)).Max(0);

        PrepUpload();

        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        // Planes are not supported by IGorgonImage, so leave at 0.
        int destResourceIndex = texture.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);
        GorgonSubResourceInfo destInfo = texture.SubResources[destResourceIndex];

        _uploadHeaps.Allocate((ulong)imageBuffer.SizeInBytes, texture.Info.Alignment, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, $"The returned resource heap allocation is not valid.");

        _commandQueue.Tracker.TrackResource(allocation.Heap.D3DResource);

        // Copy to upload resource.
        NativeMemory.Copy((void*)imageBuffer.ImageData, allocation.CpuPointer, (nuint)imageBuffer.SizeInBytes);

        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        int bufferWidth = imageBuffer.Width.Min(destInfo.Width);
        int bufferHeight = imageBuffer.Height.Min(destInfo.Height);

        if (imageBuffer.FormatInformation.IsCompressed)
        {
            bufferWidth = (imageBuffer.PitchInformation.HorizontalBlockCount.Min(destInfo.Width >> 2)) << 2;
            bufferHeight = (imageBuffer.PitchInformation.VerticalBlockCount.Min(destInfo.Height >> 2)) << 2;
        }

        // Copy to final resource.
        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
        {
            Offset = allocation.Offset,
            Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)imageBuffer.Format, (uint)bufferWidth, (uint)bufferHeight, 1, (uint)imageBuffer.PitchInformation.RowPitch)
        };
        D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);
        D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)destResourceIndex);
        D3D12_BOX box = new(0, 0, 0, bufferWidth, bufferHeight, 1);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, texture.Type == TextureType.Texture3D ? (uint)destinationZOrArrayIndex : 0, &src, &box);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonVirtualTexture texture, long handle, short destinationDepthSlice)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if (texture.Format != imageBuffer.Format)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_WITH_IMAGE_FORMAT, imageBuffer.Format, texture.Format));
        }

        if ((!texture.TryGetAllocatedTileRegion(handle, out GorgonBox tileBox)) || (!texture.TryGetSubResources(handle, out short destinationMipLevel, out short destinationArrayIndex)))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_HANDLE_DOES_NOT_EXIST, handle, texture.Name));
        }

        texture.FromTiles(in tileBox, out GorgonBoxF texelBox, destinationMipLevel);
        texture.ToPixelBox(in texelBox, out GorgonBox pixelBox, destinationMipLevel);
        destinationDepthSlice = destinationDepthSlice.Max((short)pixelBox.Front).Min((short)(pixelBox.Back - 1));

        PrepUpload();

        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        // Planes are not supported by IGorgonImage, so leave at 0.
        int destResourceIndex = texture.GetSubResourceIndex(destinationMipLevel, destinationArrayIndex, 0);

        _uploadHeaps.Allocate((ulong)imageBuffer.SizeInBytes, texture.Info.Alignment, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, $"The returned resource heap allocation is not valid.");

        _commandQueue.Tracker.TrackResource(allocation.Heap.D3DResource);

        // Copy to upload resource.
        NativeMemory.Copy((void*)imageBuffer.ImageData, allocation.CpuPointer, (nuint)imageBuffer.SizeInBytes);

        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        int bufferWidth = imageBuffer.Width.Min(pixelBox.Width);
        int bufferHeight = imageBuffer.Height.Min(pixelBox.Height);

        if (imageBuffer.FormatInformation.IsCompressed)
        {
            bufferWidth = (imageBuffer.PitchInformation.HorizontalBlockCount.Min(pixelBox.Width >> 2)) << 2;
            bufferHeight = (imageBuffer.PitchInformation.VerticalBlockCount.Min(pixelBox.Height >> 2)) << 2;
        }

        // Copy to final resource.
        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
        {
            Offset = allocation.Offset,
            Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)imageBuffer.Format, (uint)bufferWidth, (uint)bufferHeight, 1, (uint)imageBuffer.PitchInformation.RowPitch)
        };
        D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);
        D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)destResourceIndex);
        D3D12_BOX box = new(0, 0, 0, bufferWidth, bufferHeight, 1);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, (uint)pixelBox.Left, (uint)pixelBox.Top, (uint)destinationDepthSlice, &src, &box);

        return this;
    }

    /// <summary>
    /// Function to make the GPU wait for the graphics queue if it's in the process of rendering data.
    /// </summary>
    /// <returns>The fluent interface for the resource copier.</returns>
    /// <exception cref="GorgonException">Thrown if there was a failure during the wait operation.</exception>
    /// <remarks>
    /// <para>
    /// This method is meant to make the copy queue wait for the graphics queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the graphics queue is busy rendering with a 
    /// texture required by the copy queue, this will allow the copy queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// The <see cref="GorgonGraphics"/> object encapsulates the graphics queue.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="WaitForCompute"/>
    public GorgonResourceCopier WaitForGraphics()
    {
        if (_commandQueue.Type != D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY)
        {
            return this;
        }

        ulong copyQueueFence = Graphics.Queues.GraphicsQueue.FenceValue;
        _commandQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Graphics.Queues.GraphicsQueue.D3DFence.Get(), copyQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_WAIT_FAILED, nameof(Graphics.Queues.CopyQueue), nameof(Graphics.Queues.GraphicsQueue)));
        return this;
    }

    /// <summary>
    /// Function to make the GPU wait for the compute queue if it's in the process of working with data.
    /// </summary>
    /// <inheritdoc cref="WaitForGraphics" path="/returns"/>
    /// <inheritdoc cref="WaitForGraphics" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method is meant to make the copy queue wait for the compute queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the compute queue is busy updating 
    /// a texture required by the copy queue, this will allow the copy queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// To make use of the graphics queue, developers can use the <see cref="GorgonComputeEngine"/> functionality.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonComputeEngine"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="WaitForGraphics"/>
    public GorgonResourceCopier WaitForCompute()
    {
        if (_commandQueue.Type != D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY)
        {
            return this;
        }

        ulong computeQueueFence = Graphics.Queues.ComputeQueue.FenceValue;
        _commandQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Graphics.Queues.ComputeQueue.D3DFence.Get(), computeQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_WAIT_FAILED, nameof(Graphics.Queues.CopyQueue), nameof(Graphics.Queues.ComputeQueue)));
        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, long destinationOffset)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(destinationOffset, 0);

        if (texture.SizeInBytes > (buffer.SizeInBytes - destinationOffset))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL, buffer.Name, texture.SizeInBytes));
        }

        PrepUpload();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource);
        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination, true);

        for (int i = 0; i < texture.SubResources.Count; ++i)
        {
            GorgonSubResourceInfo subInfo = texture.SubResources[i];
            D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = subInfo.ToD3DPlacedSubResourceFootPrint(texture.Format, buffer.ResourceOffset + (ulong)destinationOffset);
            D3D12_TEXTURE_COPY_LOCATION srcLoc = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)i);
            D3D12_TEXTURE_COPY_LOCATION destLoc = new((PID3D12Resource2)buffer.D3DResource.Get(), in footPrint);

            _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, 0, 0, 0, &srcLoc, null);            
        }

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, GorgonCopyBufferToTexture parameters)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(parameters.SourceOffset, 0);

        // We don't need to limit the destination values because the GetSubResourceIndex method will ensure we can't go beyond the limits of the 
        // texture sub resources.
        int subResourceIndex = texture.GetSubResourceIndex(parameters.DestinationMipLevel, parameters.DestinationArrayIndex, parameters.DestinationPlane);
        GorgonSubResourceInfo subInfo = texture.SubResources[subResourceIndex];        

        if (subInfo.SizeInBytes < (buffer.SizeInBytes - parameters.SourceOffset))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_SUB_RESOURCE_TOO_SMALL, texture.Name, subInfo.SizeInBytes, buffer.SizeInBytes - parameters.SourceOffset), nameof(texture));
        }

        PrepUpload();        

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = subInfo.ToD3DPlacedSubResourceFootPrint(texture.Format, 0);
        footPrint.Offset = (ulong)parameters.SourceOffset + buffer.ResourceOffset;
        footPrint.Footprint.RowPitch = (uint)(texture.FormatInfo.SizeInBytes * subInfo.Width);

        D3D12_TEXTURE_COPY_LOCATION srcLoc = new((PID3D12Resource2)buffer.D3DResource.Get(), in footPrint);
        D3D12_TEXTURE_COPY_LOCATION destLoc = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)subResourceIndex);
        D3D12_BOX box = new(0, 0, 0, subInfo.Width, subInfo.Height, subInfo.Depth);

        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopySource);
        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, 0, 0, 0, &srcLoc, &box);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyBufferToTexture(GorgonGpuBuffer buffer, GorgonTexture texture, long sourceOffset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(sourceOffset, 0);

        if (texture.SizeInBytes < (buffer.SizeInBytes - sourceOffset))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_TOO_SMALL, texture.Name, texture.SizeInBytes, buffer.SizeInBytes - sourceOffset), nameof(buffer));
        }

        PrepUpload();

        ulong resourceOffset = (ulong)sourceOffset + buffer.ResourceOffset;

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopySource);
        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        int depth = texture.Depth;  

        for (int a = 0; a < texture.ArrayCount; ++a)
        {
            for (int m = 0; m < texture.MipCount; ++m)
            {
                for (int p = 0; p < Graphics.FormatSupport[texture.Format].PlaneCount; ++p)
                {
                    int subResourceIndex = texture.GetSubResourceIndex((short)a, (short)m, (byte)p);
                    GorgonSubResourceInfo subResourceInfo = texture.SubResources[subResourceIndex];

                    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
                    {
                        Offset = resourceOffset,
                        Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)texture.Format, (uint)subResourceInfo.Width, (uint)subResourceInfo.Height, (uint)subResourceInfo.Depth, (uint)subResourceInfo.RowPitch)
                    };

                    resourceOffset += (ulong)subResourceInfo.Offset;

                    D3D12_TEXTURE_COPY_LOCATION srcLoc = new((PID3D12Resource2)buffer.D3DResource.Get(), in footPrint);
                    D3D12_TEXTURE_COPY_LOCATION destLoc = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)subResourceIndex);

                    _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, 0, 0, (uint)(texture.Type == TextureType.Texture3D ? depth : 0), &srcLoc, null);
                }

                depth >>= 1;

                if (depth < 1)
                {
                    depth = 1;
                }
            }
        }

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer buffer, GorgonCopyTextureToBuffer parameters)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(parameters.DestinationOffset, 0);

        byte planeCount = texture.Graphics.FormatSupport[texture.Format].PlaneCount;
        short sourceMipLevel = parameters.SourceMipLevel.Min((short)(texture.MipCount - 1)).Max(0);
        short sourceArrayIndex = (short)(texture.Type != TextureType.Texture3D ? parameters.SourceArrayIndex.Min((short)(texture.ArrayCount - 1)).Max(0) : 0);
        byte sourcePlane = parameters.SourcePlane.Min((byte)(planeCount - 1)).Max(0);
        int subResourceIndex = texture.GetSubResourceIndex(sourceMipLevel, sourceArrayIndex, sourcePlane);
        GorgonSubResourceInfo subInfo = texture.SubResources[subResourceIndex];

        if (subInfo.SizeInBytes > (buffer.SizeInBytes - parameters.DestinationOffset))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL, buffer.Name, subInfo.SizeInBytes));
        }

        PrepUpload();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = subInfo.ToD3DPlacedSubResourceFootPrint(texture.Format, 0);
        footPrint.Offset = (ulong)parameters.DestinationOffset + buffer.ResourceOffset;
        footPrint.Footprint.RowPitch = (uint)(texture.FormatInfo.SizeInBytes * subInfo.Width);        

        D3D12_TEXTURE_COPY_LOCATION srcLoc = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)subResourceIndex);
        D3D12_TEXTURE_COPY_LOCATION destLoc = new((PID3D12Resource2)buffer.D3DResource.Get(), in footPrint);

        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination);
        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, force: true);

        D3D12_BOX box = new(0, 0,  0, subInfo.Width, subInfo.Height, subInfo.Depth);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, 0, 0, 0, &srcLoc, &box);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyTexture(GorgonTexture source, GorgonTexture destination, ref readonly GorgonCopyTextureSubResource parameters)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        ValidateCopyTexture(source, destination, in parameters);

        GorgonCopyTextureSubResource newParameters = Clip(source, destination, in parameters, out bool isFullSubResource);

        if (newParameters.IsEmpty)
        {
            return this;
        }

        switch (source.Type)
        {
            case TextureType.Texture1D:
                Copy1DTexture(source, destination, new GorgonRange<int>(newParameters.SourceRegion.Left, newParameters.SourceRegion.Right), (short)newParameters.SourceRegion.Front, newParameters.SourceMipLevel, newParameters.SourcePlane, newParameters.DestinationX, newParameters.DestinationY, newParameters.DestinationZOrArrayIndex, newParameters.DestinationMipLevel, newParameters.DestinationPlane, isFullSubResource);
                break;
            case TextureType.Texture2D:
                Copy2DTexture(source, destination, (GorgonRectangle)newParameters.SourceRegion, (short)newParameters.SourceRegion.Front, newParameters.SourceMipLevel, newParameters.SourcePlane, newParameters.DestinationX, newParameters.DestinationY, newParameters.DestinationZOrArrayIndex, newParameters.DestinationMipLevel, newParameters.DestinationPlane, isFullSubResource);
                break;
            case TextureType.Texture3D:
                Copy3DTexture(source, destination, newParameters.SourceRegion, newParameters.SourceMipLevel, newParameters.SourcePlane, newParameters.DestinationX, newParameters.DestinationY, newParameters.DestinationZOrArrayIndex, newParameters.DestinationMipLevel, newParameters.DestinationPlane, isFullSubResource);
                break;
            default:
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, source.Type), nameof(source));
        }

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonCopyMethodsFluent<IGorgonResourceWriter>.CopyTexture(GorgonTexture source, GorgonTexture destination)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if ((destination.Type != source.Type) || (source.FormatInfo.Group != destination.FormatInfo.Group)
            || (source.MipCount != destination.MipCount) || (source.ArrayCount != destination.ArrayCount)
            || (source.Width != destination.Width) || (source.Height != destination.Height) || (source.Depth != destination.Depth))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_NOT_SAME, source.Name, destination.Name));
        }

        GorgonCopyTextureSubResource copyParams = new();
        ValidateCopyTexture(source, destination, in copyParams);

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepUpload();

        _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource);
        _commandList.SetBarrier(destination, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        _commandQueue.Tracker.TrackResource(source.D3DResource);
        _commandQueue.Tracker.TrackResource(destination.D3DResource);

        _commandList.D3DGraphicsCommandList.Get()->CopyResource(destRes, srcRes);

        return this;
    }

    /// <summary>
    /// Function to copy data from a GPU buffer to a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the buffer. Must be an unmanaged type.</typeparam>
    /// <param name="buffer">The buffer to copy data from.</param>
    /// <param name="destination">The pointer to the memory that will receive the contents of the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="destination"/> is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <remarks>
    /// <para>
    /// This copies data directly from the <paramref name="buffer"/> and into the memory pointed at by the <paramref name="destination"/>. This allows developers to read back data from the GPU for debugging 
    /// purposes, or other reasons.
    /// </para>
    /// <para type="ClipInfo">
    /// If the <paramref name="destination"/> is too small to hold the contents of <paramref name="buffer"/>, then the copy will only copy up to the number of bytes that can fit in the 
    /// <paramref name="destination"/>.
    /// </para>
    /// <para type="CopyCommon">
    /// <para>
    /// This method waits until the copy is fully completed on the GPU, and only then copies the data into the resulting <paramref name="destination"/>. This may cause a stall.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// As with all download operations from the GPU, this method is not performance friendly, and is not recommended for use in areas where performance is necessary. Best practice is to only transfer 
    /// data between other buffers on the GPU for maximum performance.
    /// </para>
    /// </note>
    /// </para>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPtr{T}"/>
    public void CopyToPointer<T>(GorgonGpuBufferCommon buffer, GorgonPtr<T> destination)
        where T : unmanaged
    {
        if (destination.Equals(GorgonPtr<T>.NullPtr))
        {
            throw new ArgumentNullException(nameof(destination));
        }

        CopyDownloadData(buffer, (void*)destination, 0, (uint)sizeof(T), (ulong)destination.Length);
    }

    /// <summary>
    /// Function to copy data from a GPU buffer to a <see cref="Span{T}"/>.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/typeparam"/></typeparam>
    /// <param name="buffer"><inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/param[@name='buffer']"/></param>
    /// <param name="destination">The span that will receive the contents of the buffer.</param>
    /// <exception cref="GorgonException">Thrown if the <paramref name="destination"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// This copies data directly from the <paramref name="buffer"/> and into the <paramref name="destination"/> value. This allows developers to read back data from the GPU for debugging purposes, or other 
    /// reasons.
    /// </para>
    /// <inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/remarks/para[@type='ClipInfo']"/>
    /// <inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/remarks/para[@type='CopyCommon']"/>
    /// </remarks>
    public void CopyToRange<T>(GorgonGpuBufferCommon buffer, Span<T> destination)
        where T : unmanaged
    {
        if (destination.IsEmpty)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_DEST_TOO_SMALL, 0, buffer.SizeInBytes));
        }

        fixed (void* tPtr = destination)
        {
            CopyDownloadData(buffer, tPtr, 0, (uint)sizeof(T), (ulong)destination.Length);
        }
    }

    /// <summary>
    /// Function to copy a buffer element to the specified value.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/typeparam"/></typeparam>
    /// <param name="buffer"><inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/param[@name='buffer']"/></param>
    /// <param name="destination">The value to copy the data into.</param>
    /// <param name="bufferOffset">[Optional] The offset, in bytes, in the buffer to start reading from.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="bufferOffset"/> parameter is less than 0.</exception>
    /// <exception cref="GorgonException">Thrown if the size of <typeparamref name="T"/> plus the <paramref name="bufferOffset"/> is larger than the size of the <paramref name="buffer"/>.</exception>
    /// <remarks>
    /// <para>
    /// This copies data directly from the <paramref name="buffer"/>, at the given <paramref name="bufferOffset"/> into the <paramref name="destination"/> value. This allows developers to read back data from 
    /// the GPU for debugging purposes, or other reasons.
    /// </para>
    /// <inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/remarks/para[@type='CopyCommon']"/>
    /// </remarks>
    public void CopyToValue<T>(GorgonGpuBufferCommon buffer, out T destination, long bufferOffset = 0)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferOffset, 0);

        int typeSize = sizeof(T);

        if (bufferOffset + typeSize > buffer.SizeInBytes)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL, buffer.Name, typeSize));
        }

        fixed (void* tPtr = &destination)
        {
            CopyDownloadData(buffer, tPtr, (ulong)bufferOffset, (uint)sizeof(T), 1);
        }
    }

    /// <summary>
    /// <inheritdoc cref="CopyToValue{T}(GorgonGpuBufferCommon, out T, long)"/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/typeparam"/></typeparam>
    /// <param name="buffer"><inheritdoc cref="CopyToPointer{T}(GorgonGpuBufferCommon, GorgonPtr{T})" path="/param[@name='buffer']"/></param>
    /// <param name="bufferOffset"><inheritdoc cref="CopyToValue{T}(GorgonGpuBufferCommon, out T, long)" path="/param[@name='bufferOffset']"/></param>
    /// <returns>The value in the buffer.</returns>
    /// <inheritdoc cref="CopyToValue{T}(GorgonGpuBufferCommon, out T, long)" path="/exception"/>
    /// <inheritdoc cref="CopyToValue{T}(GorgonGpuBufferCommon, out T, long)" path="/remarks"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T CopyToValue<T>(GorgonGpuBufferCommon buffer, long bufferOffset = 0)
        where T : unmanaged
    {
        CopyToValue(buffer, out T value, bufferOffset);
        return value;
    }

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonTextureCommon"/> into a <see cref="IGorgonImage"/>.
    /// </summary>
    /// <param name="texture">The texture to copy.</param>
    /// <param name="image">The image that will receive the texture data.</param>
    /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="texture"/> is an unresolved multi-sample texture.</para>
    /// <para>Thrown if the <paramref name="texture"/> is <see cref="GorgonTextureInfo.IsDepthStencil">configured to be used as a depth/stencil texture</see>.</para>
    /// </exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="image"/> format is not compatible with the <paramref name="texture"/> format.</exception>
    /// <remarks>
    /// <para>
    /// This method will copy the contents of a <see cref="GorgonTextureCommon"/> into a <see cref="IGorgonImage"/> so that applications can evaluate texture data on the CPU. This method copies the entire texture 
    /// to the image, if an application needs to more fine grained copying, use the <see cref="CopyTextureToImage(GorgonTexture, IGorgonImageBuffer, short, short, byte)"/> overload.
    /// </para>
    /// <para>
    /// If the <paramref name="texture"/> dimensions, array count (1D or 2D only), or mip count are not the same as those in the <paramref name="image"/>, then the method will only copy the minimum 
    /// dimensions, array count and/or mip count. For example, if the texture has 5 array indices, and the image only has 2 array indices, this method will only copy the first two indices. This ensures we 
    /// don't have an overrun when copying. 
    /// </para>
    /// <para>
    /// If the texture <see cref="GorgonTextureCommon.Format"/> does not match that of the <paramref name="image"/>, and the image can be converted to the format of the texture, the method will automatically do so 
    /// prior to copying into the texture. If it cannot convert the image due to an incompatible format, then an exception will be thrown.
    /// </para>
    /// <para type="Limits">
    /// This method also has the following limitations for the destination <paramref name="texture"/>.
    /// <list type="bullet">
    /// <item>
    ///     <description>Textures that are created for use as a <see cref="GorgonTextureInfo.IsDepthStencil">Depth/Stencil</see> cannot be used as a source. An exception will be thrown if an attempt to copy 
    ///     from a depth/stencil texture is made.</description>
    /// </item>
    /// <item>
    ///     <description>Textures that are created using <see cref="GorgonTextureInfo.MultisampleInfo">Multisampling</see> (i.e. a multi-sample value that is not equal to 
    ///     <see cref="GorgonMultisampleInfo.NoMultisampling"/>) cannot be used as a source. An exception will be thrown if an attempt to copy from a multi-sampled texture is made.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTextureCommon"/>
    /// <seealso cref="GorgonTextureInfo"/>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="CopyTextureToImage(GorgonTexture, IGorgonImageBuffer, short, short, byte)"/>
    public void CopyTextureToImage(GorgonTexture texture, IGorgonImage image)
    {
        if (!texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_FROM_MULTISAMPLE_TEXTURE, texture.Name, texture.MultisampleInfo), nameof(texture));
        }

        if (texture.IsDepthStencil)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_FROM_DEPTH_STENCIL, texture.Name), nameof(texture));
        }

        BufferFormat originalFormat = image.Format;

        if (texture.Format != image.Format)
        {
            if (image.CanConvertToFormat(texture.Format))
            {
                Graphics.Log.Print($"The image format ({image.Format}) is different from the texture '{texture.Name}' format ({texture.Format}). The image will temporarily converted to the texture format.", LoggingLevel.Verbose);
                image.BeginUpdate().ConvertToFormat(texture.Format).EndUpdate();
            }
            else
            {
                throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_WITH_IMAGE_FORMAT, image.Format, texture.Format));
            }
        }

        PrepDownload();

        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        _downloadHeaps.Allocate((ulong)image.SizeInBytes, texture.Info.Alignment, out CpuBufferAllocation allocation);
        Debug.Assert(allocation.IsAvailable, "The returned resource heap allocation is not valid.");

        _commandQueue.Tracker.TrackResource(allocation.Heap.D3DResource);

        ulong offsetCalc = allocation.Offset;
        int arrayCount = image.ArrayCount.Min(texture.ArrayCount);
        int mipCount = image.MipCount.Min(texture.MipCount);
        int depth = image.Depth.Min(texture.Depth);

        // Copy queues can only use Common layouts.
        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, force: true);

        try
        {
            for (int a = 0; a < arrayCount; ++a)
            {
                for (int m = 0; m < mipCount; ++m)
                {
                    int subResourceIndex = texture.GetSubResourceIndex((short)m, (short)a);
                    GorgonSubResourceInfo subResource = texture.SubResources[subResourceIndex];
                    IGorgonImageBuffer buffer = image.Buffers[m, a];

                    D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)subResourceIndex);

                    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
                    {
                        Offset = offsetCalc,
                        Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)texture.Format, (uint)subResource.Width, (uint)subResource.Height, (uint)depth, (uint)buffer.PitchInformation.RowPitch)
                    };

                    D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);

                    _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, 0, &src, null);

                    offsetCalc += (ulong)(buffer.SizeInBytes * depth);

                    depth = (depth >> 1).Max(1);                    
                }
            }

            ExecuteDownload();

            NativeMemory.Copy(allocation.CpuPointer, (void*)image.ImageData, (nuint)image.SizeInBytes);

            if (originalFormat != image.Format)
            {
                Graphics.Log.Print($"The image format ({image.Format}) was different from the texture '{texture.Name}' format ({texture.Format}). The image will be restored to its original format.", LoggingLevel.Verbose);
                image.BeginUpdate().ConvertToFormat(originalFormat).EndUpdate();
            }
        }
        finally
        {
            Cleanup();
        }
    }

    /// <summary>
    /// Function to copy the contents of a <see cref="GorgonTextureCommon"/> sub resource into a <see cref="IGorgonImageBuffer"/>.
    /// </summary>
    /// <param name="texture"><inheritdoc cref="CopyTextureToImage(GorgonTexture, IGorgonImage)" path="/param[@name='texture']"/></param>
    /// <param name="buffer">The image buffer that will receive the sub resource data.</param>
    /// <param name="sourceMipLevel">[Optional] The source mip level on the texture to copy the image data from.</param>
    /// <param name="sourceZOrArrayIndex">[Optional] The source depth slice on a 3D texture, or array index on a 1D or 2D texture array to copy the image data from.</param>
    /// <param name="sourcePlane">[Optional] The source format plane on the texture to copy the image data from.</param>
    /// <inheritdoc cref="CopyTextureToImage(GorgonTexture, IGorgonImage)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method will copy the contents of a sub resource in a <see cref="GorgonTextureCommon"/> into a <see cref="IGorgonImage"/> on a <see cref="IGorgonImage"/>. This method only copies one sub resource, if 
    /// the application needs to copy the entire image instead, use the <see cref="CopyTextureToImage(GorgonTexture, IGorgonImage)"/> overload.
    /// </para>
    /// <para>
    /// <inheritdoc cref="CopyTextureToImage(GorgonTexture, IGorgonImage)" path="/remarks/para[2]"/>
    /// </para>
    /// <para>
    /// <inheritdoc cref="CopyTextureToImage(GorgonTexture, IGorgonImage)" path="/remarks/para[3]"/>
    /// </para>
    /// <inheritdoc cref="IGorgonCopyMethodsFluent{IGorgonResourceWriter}.CopyImageToTexture(IGorgonImage, GorgonTexture)" path="/remarks/para[@type='Limits']"/>
    /// <para>
    /// If the <paramref name="sourceMipLevel"/>, <paramref name="sourceZOrArrayIndex"/>, and the <paramref name="sourcePlane"/> is not specified, then the first mip level, first array index 
    /// (or depth slice for a 3D texture), and the first format plane are used to copy the data from.
    /// </para>
    /// </remarks>
    public void CopyTextureToImage(GorgonTexture texture, IGorgonImageBuffer buffer, short sourceMipLevel = 0, short sourceZOrArrayIndex = 0, byte sourcePlane = 0)
    {
        if (!texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_FROM_MULTISAMPLE_TEXTURE, texture.Name, texture.MultisampleInfo), nameof(texture));
        }

        if (texture.IsDepthStencil)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_COPY_FROM_DEPTH_STENCIL, texture.Name), nameof(texture));
        }

        if (texture.Format != buffer.Format)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_WITH_IMAGE_FORMAT, buffer.Format, texture.Format));
        }

        int maxPlaneCount = Graphics.FormatSupport[texture.Format].PlaneCount;
        sourceZOrArrayIndex = texture.Type == TextureType.Texture3D ? sourceZOrArrayIndex.Min((short)(texture.Depth - 1)).Max(0) : sourceZOrArrayIndex.Min((short)(texture.ArrayCount - 1)).Max(0);
        sourceMipLevel = sourceMipLevel.Min((short)(texture.MipCount - 1)).Max(0);
        sourcePlane = sourcePlane.Min((byte)(maxPlaneCount - 1)).Max(0);

        try
        {

            PrepDownload();

            _commandQueue.Tracker.TrackResource(texture.D3DResource);

            int srcResourceIndex = texture.GetSubResourceIndex(sourceMipLevel, sourceZOrArrayIndex, sourcePlane);
            GorgonSubResourceInfo srcInfo = texture.SubResources[srcResourceIndex];

            _downloadHeaps.Allocate((ulong)buffer.SizeInBytes, texture.Info.Alignment, out CpuBufferAllocation allocation);
            Debug.Assert(allocation.IsAvailable, "The returned resource heap allocation is not valid.");

            _commandQueue.Tracker.TrackResource(allocation.Heap.D3DResource);

            // Copy queues can only use Common layouts.
            _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, force: true);

            D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)srcResourceIndex);

            int bufferWidth = buffer.Width.Min(srcInfo.Width);
            int bufferHeight = buffer.Height.Min(srcInfo.Height);

            if (texture.FormatInfo.IsCompressed)
            {
                bufferWidth = (buffer.PitchInformation.HorizontalBlockCount.Min(srcInfo.Width >> 2)) << 2;
                bufferHeight = (buffer.PitchInformation.VerticalBlockCount.Min(srcInfo.Height >> 2)) << 2;
            }

            D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
            {
                Offset = allocation.Offset,
                Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)texture.Format, (uint)bufferWidth, (uint)bufferHeight, 1, (uint)buffer.PitchInformation.RowPitch)
            };

            D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);
            D3D12_BOX box = new(0, 0, texture.Type == TextureType.Texture3D ? sourceZOrArrayIndex : 0,
                                        bufferWidth, bufferHeight, texture.Type == TextureType.Texture3D ? sourceZOrArrayIndex + 1 : 1);

            _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, 0, &src, &box);

            ExecuteDownload();

            NativeMemory.Copy(allocation.CpuPointer, (void*)buffer.ImageData, (nuint)buffer.SizeInBytes);
        }
        finally
        {
            Cleanup();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonResourceCopier"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceCopier(GorgonGraphics)" path="/param[@name='graphics']"/></param>
    /// <param name="list">The pre-allocated command list to use for copying.</param>
    internal GorgonResourceCopier(GorgonGraphics graphics, GorgonCommandList list)
    {
        Graphics = graphics;
        _uploadHeaps = Graphics.Memory.UploadHeaps;
        _downloadHeaps = Graphics.Memory.DownloadHeaps;
        _batchState = int.MaxValue;
        _commandAllocator = list.Allocator;
        _commandList = list;
        _commandQueue = list.Queue;        

        // Release any previous resources that may have been used in copy operations.
        _commandQueue.Tracker.Signal();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonResourceCopier"/> class.
    /// </summary>
    /// <param name="queue">The queue to use for copying.</param>
    internal GorgonResourceCopier(CommandQueue queue)
    {
        Graphics = queue.Graphics;
        _uploadHeaps = Graphics.Memory.UploadHeaps;
        _downloadHeaps = Graphics.Memory.DownloadHeaps;
        _commandQueue = queue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonResourceCopier"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that is associated with this writer.</param>
    public GorgonResourceCopier(GorgonGraphics graphics)
    {
        Graphics = graphics;
        _uploadHeaps = Graphics.Memory.UploadHeaps;
        _downloadHeaps = Graphics.Memory.DownloadHeaps;
        _commandQueue = Graphics.Queues.CopyQueue;
    }
}
