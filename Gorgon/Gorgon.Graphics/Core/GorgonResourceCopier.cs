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

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

#region Parameters.
/// <summary>
/// Parameters used to copy a buffer into a texture sub resoruce.
/// </summary>
public readonly ref struct CopyBufferToTextureParams
{
    /// <summary>
    /// An empty texture sub resource parameter.
    /// </summary>
    public static CopyTextureSubResourceParams Empty => default;

    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => (SourceOffset == 0)
                && (DestinationArrayIndex == 0) && (DestinationMipLevel == 0) && (DestinationPlane == 0);

    /// <summary>
    /// Property to return the offset, in bytes, in the buffer to start copying from.
    /// </summary>
    public readonly long SourceOffset
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the destination array index.
    /// </summary>
    /// <remarks>
    /// If the destination texture has a texture type of <see cref="TextureType.Texture3D"/>, then this value is ignored.
    /// </remarks>
    public readonly short DestinationArrayIndex
    {
        get;
        init;
    } = 0;


    /// <summary>
    /// Property to return the mip level on the destination texture to copy into.
    /// </summary>
    public readonly short DestinationMipLevel
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the destination format plane index on the destination texture to copy into.
    /// </summary>
    public readonly byte DestinationPlane
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyBufferToTextureParams"/> value type.
    /// </summary>
    public CopyBufferToTextureParams()
    {
    }
}

/// <summary>
/// Parameters used to copy a texture sub resource to another texture sub resource.
/// </summary>
/// <seealso cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)"/>
public readonly ref struct CopyTextureSubResourceParams
{
    /// <summary>
    /// An empty texture sub resource parameter.
    /// </summary>
    public static CopyTextureSubResourceParams Empty => default;

    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => (SourceRegion.Equals(in GorgonBox.Empty)) && (SourceMipLevel == 0) && (SourcePlane == 0)
                && (DestinationX == 0) && (DestinationY == 0) && (DestinationZOrArrayIndex == 0) && (DestinationMipLevel == 0) && (DestinationPlane == 0);

    /// <summary>
    /// Property to return the region on the source texture to copy. 
    /// </summary>
    /// <remarks>
    /// This region will contain either be the depth range for a 3D texture, or the range of array indices for a 1D or 2D texture array.
    /// </remarks>
    public readonly GorgonBox SourceRegion
    {
        get;
        init;
    } = GorgonBox.Empty;

    /// <summary>
    /// Property to return the mip level on the source texture to copy from.
    /// </summary>
    public readonly short SourceMipLevel
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the source format plane index on the source texture to copy from.
    /// </summary>
    public readonly byte SourcePlane
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the horizontal destination position in the destination texture.
    /// </summary>
    public readonly int DestinationX
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the vertical destination position in the destination texture.
    /// </summary>
    /// <remarks>
    /// This only applies to textures with a texture type of <see cref="TextureType.Texture2D"/>, or <see cref="TextureType.Texture3D"/>.
    /// </remarks>
    public readonly int DestinationY
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the depth destination position in the texture or the array index.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the destination texture has a texture type of <see cref="TextureType.Texture1D"/>, or <see cref="TextureType.Texture2D"/>, then this value represents the array index for the texture array.
    /// </para>
    /// <para>
    /// If the destination texture has a texture type of <see cref="TextureType.Texture3D"/>, then this value represents the depth slice in the depth texture.
    /// </para>
    /// </remarks>
    public readonly short DestinationZOrArrayIndex
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the mip level on the destination texture to copy into.
    /// </summary>
    public readonly short DestinationMipLevel
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the destination format plane index on the destination texture to copy into.
    /// </summary>
    public readonly byte DestinationPlane
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyTextureSubResourceParams"/> value type.
    /// </summary>
    public CopyTextureSubResourceParams()
    {
    }
}
#endregion

/// <summary>
/// Functionality to copy data into a <see cref="GorgonGpuBuffer_OLDE"/> or a <see cref="GorgonTexture"/> from CPU memory on the GPU copy queue, or from a <see cref="BufferUsage.Download"/> buffer into CPU 
/// memory.
/// </summary>
/// <remarks>
/// <para>
/// This provides functionality for applications to write data into <see cref="GorgonGpuBuffer_OLDE"/> or <see cref="GorgonTexture"/> objects from CPU memory. It also provides functionality to read data from a 
/// <see cref="GorgonGpuBuffer_OLDE"/> with a <see cref="BufferUsage"/> of <see cref="BufferUsage.Download"/> into standard CPU addressable memory like an array, <see cref="Span{T}"/>, or a 
/// <see cref="GorgonPtr{T}"/>.
/// </para>
/// <para>
/// The type provides a fluent interface that allows applications to chain multiple copy operations together by returning the <see cref="IGorgonResourceWriter"/> interface from the <see cref="BeginUpload"/> method. 
/// This allows applications to write data from CPU addressable memory into the GPU.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGpuBuffer_OLDE"/>
/// <seealso cref="GorgonTexture"/>
/// <seealso cref="IGorgonResourceWriter"/>
/// <seealso cref="GorgonPtr{T}"/>
/// <seealso cref="BufferUsage"/>
public unsafe sealed class GorgonResourceCopier
    : IDisposable, IGorgonResourceWriter
{
    private static readonly string _cmdListName = $"{nameof(GorgonResourceCopier)} Command List (Copy Queue)";
    private GorgonCommandList? _commandList;
    private CommandAllocator? _commandAllocator;
    private CommandQueue _commandQueue;
    private readonly CommandQueue[] _commandQueues = new CommandQueue[2];
    private int _batchState;
    private bool _hasDelayedWrites;

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
            if (_batchState == int.MaxValue)
            {
                return;
            }

            ((IGorgonResourceWriter)this).End();
        }
    }

    /// <summary>
    /// Function to prep the copier for delayed writing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining), MemberNotNull(nameof(_commandAllocator), nameof(_commandList))]
    private void PrepDelayedWrites()
    {
        _hasDelayedWrites = true;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("For old buffer types.")]
    private static void WriteCpuBuffer(GorgonGpuBuffer_OLDE buffer, void* data, long offset, long count)
    {
        void* dest = (void *)(buffer.CpuData + offset);

        NativeMemory.Copy(data, dest, (nuint)count);

        if (buffer.Usage == BufferUsage.DynamicPerFrame)
        {
            buffer.DynamicDataChanged();
        }
    }

    /// <summary>
    /// Function to write values to a buffer.
    /// </summary>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="data">The pointer to the data to write.</param>
    /// <param name="offset">The offset, in bytes, within the <paramref name="buffer"/> to start writing at.</param>
    /// <param name="count">The number of bytes to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteCpuBuffer(GorgonGpuBuffer buffer, void* data, ulong offset, ulong count)
    {
        ref readonly CpuBufferAllocation allocation = ref buffer.GetTransientBufferData();

        Debug.Assert(allocation.IsAvailable, $"Transient heap for buffer '{buffer.Name}' is not valid.");

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);

        void* dest = allocation.CpuPointer + offset;

        NativeMemory.Copy(data, dest, (nuint)count);

        buffer.NeedsDataUpload = true;
    }

    /// <inheritdoc cref="WriteCpuBuffer"/>    
    [Obsolete("For old buffer types")]
    private void WriteGpuBuffer(GorgonGpuBuffer_OLDE buffer, void* data, ulong offset, ulong count)
    {
        PrepDelayedWrites();        

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);

        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination, true);

        Graphics.UploadHeaps.Allocate(count, Graphics.Adapter.HasTightAlignmentSupport ? 0 : D3D12.D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT, out CpuBufferAllocation allocation);
        NativeMemory.Copy(data, allocation.CpuPointer, (nuint)count);

        Debug.Assert(allocation.IsAvailable, "The returned resource heap allocation is not valid.");

        _commandList.D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)buffer.D3DResource.Get(), offset, (PID3D12Resource2)allocation.Heap.D3DResource.Get(), allocation.Offset, count);        
    }

    /// <inheritdoc cref="WriteCpuBuffer(GorgonGpuBuffer, void*, ulong, ulong)"/>    
    private void WriteGpuBuffer(GorgonGpuBuffer buffer, void* data, ulong offset, ulong count)
    {
        PrepDelayedWrites();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);

        _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination, true);

        Graphics.UploadHeaps.Allocate(count, Graphics.Adapter.HasTightAlignmentSupport ? 0 : D3D12.D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT, out CpuBufferAllocation allocation);
        NativeMemory.Copy(data, allocation.CpuPointer, (nuint)count);

        Debug.Assert(allocation.IsAvailable, "The returned resource heap allocation is not valid.");

        ref readonly GpuBufferAllocation bufferAllocation = ref buffer.GpuAllocation;

        // Not in the mega buffer, so we have to copy directly into it.
        if (bufferAllocation.IsNull)
        {
            _commandList.D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)buffer.D3DResource.Get(), offset, (PID3D12Resource2)allocation.Heap.D3DResource.Get(), allocation.Offset, count);
            return;
        }

        _commandList.D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)buffer.D3DResource.Get(), bufferAllocation.Offset + offset, (PID3D12Resource2)allocation.Heap.D3DResource.Get(), allocation.Offset, count);
    }

    /// <summary>
    /// Function to validate the parameters for the functions in the writer.
    /// </summary>
    /// <param name="buffer">The buffer being written into.</param>
    /// <param name="offset">The offset, in bytes, within the buffer to start writing at.</param>
    /// <param name="count">The total number of items within the buffer.</param>
    /// <param name="typeSize">The size of an individual item, in bytes, within the buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="offset"/>, is less than 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="count"/> is less than 0.</para>
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the <paramref name="count"/> is greater than the <see cref="GorgonGpuBuffer_OLDE.SizeInBytes">size</see> if the buffer.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="buffer"/> has a usage of <see cref="BufferUsage.Download"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("For old buffer types.")]
    private static void ValidateRangeParams(GorgonGpuBuffer_OLDE buffer, long offset, long count, int typeSize)
    {
        if (buffer.Usage == BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_WRITE_TO_DOWNLOAD_BUFFER, buffer.Name));
        }

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
    /// Function to validate the parameters for the functions in the writer.
    /// </summary>
    /// <param name="buffer">The buffer being written into.</param>
    /// <param name="offset">The offset, in bytes, within the buffer to start writing at.</param>
    /// <param name="count">The total number of items within the buffer.</param>
    /// <param name="typeSize">The size of an individual item, in bytes, within the buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="offset"/>, is less than 0.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="count"/> is less than 0.</para>
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the <paramref name="count"/> is greater than the <see cref="GorgonGpuBuffer_OLDE.SizeInBytes">size</see> if the buffer.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="buffer"/> has a usage of <see cref="BufferUsage.Download"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateRangeParams(GorgonGpuBuffer buffer, long offset, long count, int typeSize)
    {
        if (buffer.Usage == BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_WRITE_TO_DOWNLOAD_BUFFER, buffer.Name));
        }

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
    /// <param name="destination"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destination']"/></param>
    /// <param name="sourceRegion">The horizontal range on the texture to copy.</param>
    /// <param name="sourceArrayIndex">The index in the texture array to copy.</param>
    /// <param name="sourceMipLevel"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourceMipLevel']"/></param>
    /// <param name="sourcePlane"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourcePlane']"/></param>
    /// <param name="destinationX"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationX']"/></param>
    /// <param name="destinationY"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationY']"/></param>
    /// <param name="destinationZOrArrayIndex"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationZOrArrayIndex']"/></param>
    /// <param name="destinationMipLevel"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationMipLevel']"/></param>
    /// <param name="destinationPlane"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationPlane']"/></param>
    /// <param name="isFullSubResource"><b>true</b> if the full sub resource is being copied, <b>false</b> if not.</param>
    private void Copy1DTexture(GorgonTexture source, GorgonTexture destination, GorgonRange<int> sourceRegion, int sourceArrayIndex, int sourceMipLevel, int sourcePlane, int destinationX, int destinationY, int destinationZOrArrayIndex, int destinationMipLevel, int destinationPlane, bool isFullSubResource)
    {
        uint sourceSubIndex = (uint)source.GetSubResourceIndex(sourceMipLevel, sourceArrayIndex, sourcePlane);
        uint destSubIndex = (uint)destination.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);

        if ((ReferenceEquals(source, destination)) && (sourceSubIndex == destSubIndex))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_INTO_ITSELF, source.Name, sourceSubIndex));
        }

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepDelayedWrites();

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
    /// <param name="destination"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destination']"/></param>
    /// <param name="sourceRegion">The rectangular range on the texture to copy.</param>
    /// <param name="sourceArrayIndex"><inheritdoc cref="Copy1DTexture" path="/param[@name='sourceArrayIndex']"/></param>
    /// <param name="sourceMipLevel"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourceMipLevel']"/></param>
    /// <param name="sourcePlane"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourcePlane']"/></param>
    /// <param name="destinationX"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationX']"/></param>
    /// <param name="destinationY"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationY']"/></param>
    /// <param name="destinationZOrArrayIndex"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationZOrArrayIndex']"/></param>
    /// <param name="destinationMipLevel"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationMipLevel']"/></param>
    /// <param name="destinationPlane"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationPlane']"/></param>
    /// <param name="isFullSubResource"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='isFullSubResource']"/></param>
    private void Copy2DTexture(GorgonTexture source, GorgonTexture destination, GorgonRectangle sourceRegion, int sourceArrayIndex, int sourceMipLevel, int sourcePlane, int destinationX, int destinationY, int destinationZOrArrayIndex, int destinationMipLevel, int destinationPlane, bool isFullSubResource)
    {
        uint sourceSubIndex = (uint)source.GetSubResourceIndex(sourceMipLevel, sourceArrayIndex, sourcePlane);
        uint destSubIndex = (uint)destination.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);

        if ((ReferenceEquals(source, destination)) && (sourceSubIndex == destSubIndex))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_INTO_ITSELF, source.Name, sourceSubIndex));
        }

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepDelayedWrites();

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
    /// <param name="destination"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destination']"/></param>
    /// <param name="sourceRegion">The box range on the texture to copy.</param>
    /// <param name="sourceMipLevel"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourceMipLevel']"/></param>
    /// <param name="sourcePlane"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='sourcePlane']"/></param>
    /// <param name="destinationX"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationX']"/></param>
    /// <param name="destinationY"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationY']"/></param>
    /// <param name="destinationZOrArrayIndex"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationZOrArrayIndex']"/></param>
    /// <param name="destinationMipLevel"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationMipLevel']"/></param>
    /// <param name="destinationPlane"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='destinationPlane']"/></param>
    /// <param name="isFullSubResource"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture)" path="/param[@name='isFullSubResource']"/></param>
    private void Copy3DTexture(GorgonTexture source, GorgonTexture destination, GorgonBox sourceRegion, int sourceMipLevel, int sourcePlane, int destinationX, int destinationY, int destinationZOrArrayIndex, int destinationMipLevel, int destinationPlane, bool isFullSubResource)
    {
        uint sourceSubIndex = (uint)source.GetSubResourceIndex(sourceMipLevel, 0, sourcePlane);
        uint destSubIndex = (uint)destination.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);

        if ((ReferenceEquals(source, destination)) && (sourceSubIndex == destSubIndex))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_INTO_ITSELF, source.Name, sourceSubIndex));
        }

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepDelayedWrites();

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
    private CopyTextureSubResourceParams Clip(GorgonTexture source, GorgonTexture destination, ref readonly CopyTextureSubResourceParams parameters, out bool sourceIsFullSubResource)
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
            return CopyTextureSubResourceParams.Empty;
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

        return new CopyTextureSubResourceParams
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
    /// <param name="source"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)" path="/param[@name='source']"/></param>
    /// <param name="destination"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)" path="/param[@name='destination']"/></param>
    /// <param name="parameters"><inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)" path="/param[@name='parameters']"/></param>
    /// <inheritdoc cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture, in CopyTextureSubResourceParams)" path="/exception"/>
    private static void ValidateCopyTexture(GorgonTexture source, GorgonTexture destination, in CopyTextureSubResourceParams parameters)
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
    /// Function initialize the copier from an existing list.
    /// </summary>
    /// <param name="commandQueue">The command queue to use when copying.</param>
    /// <param name="list">The command list to use.</param>
    /// <returns>A <see cref="IGorgonResourceWriter"/> fluent interface.</returns>
    /// <remarks>
    /// <para>
    /// Applications must call this before writing data to a <see cref="GorgonGpuBuffer_OLDE"/>. When finished writing data, the application must then call the <see cref="IGorgonResourceWriter.End"/> method. 
    /// </para>
    /// <para>
    /// This method returns a <see cref="IGorgonResourceWriter"/> interface that allows an application to perform multiple writes across multiple buffers. Applications can use these write operations in 
    /// multi-threaded operations to allow data uploads to buffers simultaneously. This allows for taking advantage of the parallelism provided by the GPU and CPU.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="IGorgonResourceWriter"/>
    internal IGorgonResourceWriter InitFromList(CommandQueue commandQueue, GorgonCommandList list)
    {
        _batchState = int.MaxValue;
        _commandAllocator = list.Allocator;
        _commandQueue = commandQueue;
        _commandList = list;

        if (commandQueue == Graphics.GraphicsQueue)
        {
            _commandQueues[0] = Graphics.CopyQueue;
            _commandQueues[1] = Graphics.ComputeQueue;
        }

        if (commandQueue == Graphics.ComputeQueue)
        {
            _commandQueues[0] = Graphics.GraphicsQueue;
            _commandQueues[1] = Graphics.CopyQueue;
        }

        // Release any previous resources that may have been used in copy operations.
        _commandQueue.Tracker.Signal();

        return this;
    }

    /// <summary>
    /// Function begin a batch upload to copy data from CPU memory into to GPU resources.
    /// </summary>
    /// <returns>A <see cref="IGorgonResourceWriter"/> fluent interface.</returns>
    /// <exception cref="GorgonException">Thrown if the method has been called already.</exception>
    /// <remarks>
    /// <para>
    /// Applications must call this before writing data to a <see cref="GorgonGpuBuffer_OLDE"/>. When finished writing data, the application must then call the <see cref="IGorgonResourceWriter.End"/> method. 
    /// </para>
    /// <para>
    /// This method returns a <see cref="IGorgonResourceWriter"/> interface that allows an application to perform multiple writes across multiple buffers. Applications can use these write operations in 
    /// multi-threaded operations to allow data uploads to buffers simultaneously. This allows for taking advantage of the parallelism provided by the GPU and CPU.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer_OLDE"/>
    /// <seealso cref="IGorgonResourceWriter"/>
    public IGorgonResourceWriter BeginUpload()
    {        
        if (Interlocked.Exchange(ref _batchState, 1) != 0)
        {
            throw new GorgonException(GorgonResult.CannotInitialize, Resources.GORGFX_ERR_BATCH_STARTED);
        }

        Graphics.SignalAll();

        return this;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ValueTask IGorgonResourceWriter.EndAsync()
    {
        if ((_batchState == int.MaxValue) || (Interlocked.CompareExchange(ref _batchState, 2, 1) != 1))
        {
            return ValueTask.CompletedTask;
        }

        if (!_hasDelayedWrites)
        {            
            Cleanup();
            return ValueTask.CompletedTask;
        }

        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");

        ulong fence = 0;

        unsafe
        {
            _commandList.Close();

            _commandQueue.Execute(_commandList);

            fence = _commandQueue.IncrementFence();

            for (int i = 0; i < _commandQueues.Length; ++i)
            {
                _commandQueues[i].IncrementFence();
            }
        }

        // Spin up a background task to wait for the GPU.
        return new ValueTask(Task.Run(() =>
        {
            try
            {
                _commandQueue.WaitForFence(fence, Timeout.Infinite);
            }
            finally
            {
                _commandQueue.ListPool.Return(_commandList);
                Cleanup();
            }
        }));
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

        for (int i = 0; i < _commandQueues.Length; ++i)
        {
            _commandQueues[i].IncrementFence();
        }

        try
        {            
            _commandQueue.WaitForFence(fence, Timeout.Infinite);
        }
        finally
        {
            _commandQueue.ListPool.Return(_commandList);
            Cleanup();            
        }        
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.SetBarrier(GorgonTexture texture, BarrierSync sync, BarrierAccess access, BarrierLayout layout, GorgonSubResourceRange? subResources, bool discard, bool force)
    {
        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");
        _commandList.SetBarrier(texture, sync, access, layout, subResources, discard, force);
        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.SetBarrier(GorgonGpuBuffer_OLDE buffer, BarrierSync sync, BarrierAccess access, bool force)
    {
        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");
        _commandList.SetBarrier(buffer, sync, access, force);
        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.SetBarrier(GorgonGpuBuffer buffer, BarrierSync sync, BarrierAccess access, bool force)
    {
        Debug.Assert(_commandList is not null && _commandAllocator is not null, "Command list and/or allocator are null.");
        _commandList.SetBarrier(buffer, sync, access, force);
        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGorgonResourceWriter IGorgonResourceWriter.CopyValue<T>(in T value, GorgonGpuBuffer_OLDE buffer, long offset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        int typeSize = sizeof(T);

        ValidateRangeParams(buffer, offset, 1, typeSize);

        fixed (T* valuePtr = &value)
        {
            switch (buffer.Usage)
            {
                case BufferUsage.Upload:
                case BufferUsage.DynamicPerFrame:
                    WriteCpuBuffer(buffer, (byte*)valuePtr, offset, typeSize);
                    break;
                case BufferUsage.Default:
                    WriteGpuBuffer(buffer, (byte*)valuePtr, (ulong)offset, (ulong)typeSize);
                    break;
            }
        }

        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGorgonResourceWriter IGorgonResourceWriter.CopyRange<T>(ReadOnlySpan<T> values, GorgonGpuBuffer_OLDE buffer, long offset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if (values.IsEmpty)
        {
            return this;
        }

        int typeSize = sizeof(T);

        ValidateRangeParams(buffer, offset, values.Length, typeSize);

        fixed (T* valuePtr = values)
        {
            switch (buffer.Usage)
            {
                case BufferUsage.Upload:
                case BufferUsage.DynamicPerFrame:
                    WriteCpuBuffer(buffer, (byte*)valuePtr, offset, values.Length * typeSize);
                    break;
                case BufferUsage.Default:
                    WriteGpuBuffer(buffer, (byte*)valuePtr, (ulong)offset, (ulong)(values.Length * typeSize));
                    break;
            }
        }

        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGorgonResourceWriter IGorgonResourceWriter.CopyPointer<T>(GorgonPtr<T> pointer, GorgonGpuBuffer_OLDE buffer, long offset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if (pointer.Equals(GorgonPtr<T>.NullPtr))
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        if (pointer.Length == 0)
        {
            return this;
        }

        ValidateRangeParams(buffer, offset, pointer.Length, pointer.TypeSize);

        switch (buffer.Usage)
        {
            case BufferUsage.Upload:
            case BufferUsage.DynamicPerFrame:
                WriteCpuBuffer(buffer, (void*)pointer, offset, pointer.SizeInBytes);
                break;
            case BufferUsage.Default:
                WriteGpuBuffer(buffer, (void*)pointer, (ulong)offset, (ulong)pointer.SizeInBytes);
                break;
        }

        return this;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGorgonResourceWriter IGorgonResourceWriter.CopyPointer<T>(GorgonPtr<T> pointer, GorgonGpuBuffer buffer, long offset)
    {
        if (_batchState is not 1 and not int.MaxValue)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BATCH_NOT_STARTED);
        }

        if (pointer.Equals(GorgonPtr<T>.NullPtr))
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        if (pointer.Length == 0)
        {
            return this;
        }

        ValidateRangeParams(buffer, offset, pointer.Length, pointer.TypeSize);

        switch (buffer.Usage)
        {
            case BufferUsage.Upload:
                throw new NotImplementedException("Not done yet.");
            case BufferUsage.DynamicPerFrame:
                WriteCpuBuffer(buffer, (void*)pointer, (ulong)offset, (ulong)pointer.SizeInBytes);
                break;
            case BufferUsage.Default:
                WriteGpuBuffer(buffer, (void*)pointer, (ulong)offset, (ulong)pointer.SizeInBytes);
                break;
        }

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.CopyBuffer(GorgonGpuBuffer_OLDE source, GorgonGpuBuffer_OLDE destination, long sourceOffset, long destinationOffset, long? count)
    {
        if (source.Usage == BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_DOWNLOAD, source.Name));
        }

        ID3D12Resource* srcResource = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destResource = (PID3D12Resource2)destination.D3DResource.Get();
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

        PrepDelayedWrites();

        if (source.Usage != BufferUsage.DynamicPerFrame)
        {
            _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource, destination.Usage == BufferUsage.DynamicPerFrame);
        }

        if (destination.Usage != BufferUsage.DynamicPerFrame)
        {
            _commandList.SetBarrier(destination, BarrierSync.Copy, BarrierAccess.CopyDestination, true);
        }

        // If we're just doing a straight up copy from a buffer of the same size, with no offset, then just dump it straight in
        // using copy resource (should be faster).
        if ((source.Usage != BufferUsage.DynamicPerFrame)
            && (destination.Usage != BufferUsage.DynamicPerFrame)
            && (sourceOffset == 0) && (destinationOffset == 0) 
            && (source.SizeInBytes == destination.SizeInBytes) 
            && (sizeToCopy == source.SizeInBytes) 
            && (srcResource != destResource))
        {
            _commandList.D3DGraphicsCommandList.Get()->CopyResource(srcResource, destResource);
            return this;
        }

        ulong dynamicSrcOffset = (ulong)destinationOffset;
        ulong dynamicDestOffset = (ulong)sourceOffset;

        if (source.Usage == BufferUsage.DynamicPerFrame)
        {            
            ref readonly CpuBufferAllocation resource = ref source.GetGpuAddress();
            Debug.Assert(resource.IsAvailable, $"Source resource '{source.Name}' not available.");
            srcResource = (PID3D12Resource2)resource.Heap.D3DResource.Get();
            dynamicSrcOffset += resource.Offset;
        }

        if (destination.Usage == BufferUsage.DynamicPerFrame)
        {
            if (source.Usage == BufferUsage.DynamicPerFrame)
            {
                // If we have a source and dest that are dynamic, we can copy everything.
                GorgonPtr<byte> srcPtr = source.CpuData.Slice(sourceOffset, sizeToCopy);
                GorgonPtr<byte> destPtr = destination.CpuData.Slice(destinationOffset, sizeToCopy);
                srcPtr.CopyTo(destPtr);                
            }

            ref readonly CpuBufferAllocation resource = ref destination.GetGpuAddress();
            Debug.Assert(resource.IsAvailable, $"Destination resource '{destination.Name}' not available.");
            destResource = (PID3D12Resource2)resource.Heap.D3DResource.Get();
            dynamicDestOffset += resource.Offset;
        }

        _commandList.D3DGraphicsCommandList.Get()->CopyBufferRegion(destResource, dynamicDestOffset, srcResource, dynamicSrcOffset, (ulong)sizeToCopy);

        _commandQueue.Tracker.TrackResource(source.D3DResource);
        _commandQueue.Tracker.TrackResource(destination.D3DResource);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.CopyImageToTexture(IGorgonImage image, GorgonTexture texture)
    {        
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

            PrepDelayedWrites();

            _commandQueue.Tracker.TrackResource(texture.D3DResource);

            Graphics.UploadHeaps.Allocate((ulong)working.SizeInBytes, (int)texture.Info.Alignment, out CpuBufferAllocation allocation);

            Debug.Assert(allocation.IsAvailable, $"Could not allocate upload memory for texture '{texture.Name}'");

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
                    GorgonSubResourceInfo subResource = texture.SubResources[texture.GetSubResourceIndex(m, a, 0)];
                    IGorgonImageBuffer buffer = working.Buffers[m, a];

                    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
                    {
                        Offset = offsetCalc,
                        Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)working.Format, (uint)buffer.Width, (uint)buffer.Height, (uint)depth, (uint)buffer.PitchInformation.RowPitch)
                    };
                    D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);
                    D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)subResource.SubResourceIndex);
                    D3D12_BOX box = new(0, 0, 0, buffer.Width.Min(subResource.Width), buffer.Height.Min(subResource.Height), buffer.Depth.Min(subResource.Depth));

                    _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, 0, &src, &box);

                    depth >>= 1;

                    if (depth < 1)
                    {
                        depth = 1;
                    }

                    offsetCalc += (ulong)(buffer.SizeInBytes * depth);
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
    IGorgonResourceWriter IGorgonResourceWriter.CopyImageToTexture(IGorgonImageBuffer imageBuffer, GorgonTexture texture, short destinationMipLevel, short destinationZOrArrayIndex, byte destinationPlane)
    {
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

        PrepDelayedWrites();

        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        int destResourceIndex = texture.GetSubResourceIndex(destinationMipLevel, destinationZOrArrayIndex, destinationPlane);
        int srcResourceIndex = texture.GetSubResourceIndex(imageBuffer.MipLevel, imageBuffer.DepthSliceIndex, 0);
        GorgonSubResourceInfo srcInfo = texture.SubResources[srcResourceIndex];
        GorgonSubResourceInfo destInfo = texture.SubResources[destResourceIndex];

        Graphics.UploadHeaps.Allocate((ulong)srcInfo.SizeInBytes, (int)texture.Info.Alignment, out CpuBufferAllocation allocation);

        Debug.Assert(allocation.IsAvailable, $"Could not allocate upload memory for texture '{texture.Name}'");

        // Copy to upload resource.
        NativeMemory.Copy((void*)imageBuffer.ImageData, allocation.CpuPointer, (nuint)imageBuffer.SizeInBytes);

        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        // Copy to final resource.
        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
        {
            Offset = allocation.Offset,
            Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)imageBuffer.Format, (uint)imageBuffer.Width, (uint)imageBuffer.Height, 1, (uint)imageBuffer.PitchInformation.RowPitch)
        };
        D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);
        D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)destResourceIndex);
        D3D12_BOX box = new(0, 0, 0, destInfo.Width.Min(imageBuffer.Width), destInfo.Height.Min(imageBuffer.Height), 1);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, 0, &src, &box);

        return this;
    }

    /// <inheritdoc/>
    public IGorgonResourceWriter WaitForCopy()
    {
        if (_commandQueue.Type != D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY)
        {
            return this;
        }

        ulong copyQueueFence = Graphics.GraphicsQueue.FenceValue;
        _commandQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Graphics.GraphicsQueue.D3DFence.Get(), copyQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_WAIT_FAILED, nameof(Graphics.CopyQueue), nameof(Graphics.GraphicsQueue)));
        return this;
    }

    /// <inheritdoc/>
    public IGorgonResourceWriter WaitForCompute()
    {
        if (_commandQueue.Type != D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY)
        {
            return this;
        }

        ulong computeQueueFence = Graphics.ComputeQueue.FenceValue;
        _commandQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Graphics.ComputeQueue.D3DFence.Get(), computeQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_WAIT_FAILED, nameof(Graphics.CopyQueue), nameof(Graphics.ComputeQueue)));
        return this;
    }

    /// <inheritdoc/>
    public void CopyTextureToImage(GorgonTexture texture, IGorgonImage image)
    {
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

        _commandAllocator = Graphics.CopyQueue.AllocatorPool.Get(_cmdListName);
        _commandList = Graphics.CopyQueue.ListPool.Get(_cmdListName, _commandAllocator);

        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        Graphics.DownloadHeaps.Allocate((ulong)image.SizeInBytes, (int)texture.Info.Alignment, out CpuBufferAllocation allocation);

        ulong offsetCalc = allocation.Offset;

        int arrayCount = image.ArrayCount.Min(texture.ArrayCount);
        int mipCount = image.MipCount.Min(texture.MipCount);
        int depth = image.Depth.Min(texture.Depth);

        Debug.Assert(allocation.IsAvailable, $"Could not allocate download memory for the image.");

        texture.Info.ToD3DResourceDesc(out D3D12_RESOURCE_DESC1 desc);

        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        try
        {
            for (int a = 0; a < arrayCount; ++a)
            {
                for (int m = 0; m < mipCount; ++m)
                {
                    int subResourceIndex = texture.GetSubResourceIndex(m, a);
                    GorgonSubResourceInfo subResource = texture.SubResources[subResourceIndex];
                    IGorgonImageBuffer buffer = image.Buffers[m, a];

                    D3D12_TEXTURE_COPY_LOCATION src = new((PID3D12Resource2)texture.D3DResource.Get(), (uint)subResourceIndex);

                    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
                    {
                        Offset = offsetCalc,
                        Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)texture.Format, (uint)buffer.Width.Min(subResource.Width), (uint)buffer.Height.Min(subResource.Height), (uint)depth, (uint)buffer.PitchInformation.RowPitch)
                    };

                    D3D12_TEXTURE_COPY_LOCATION dest = new((PID3D12Resource2)allocation.Heap.D3DResource.Get(), in footPrint);

                    _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&dest, 0, 0, 0, &src, null);

                    depth >>= 1;

                    if (depth < 1)
                    {
                        depth = 1;
                    }

                    offsetCalc += (ulong)(buffer.SizeInBytes * depth);
                }
            }

            _commandList.D3DGraphicsCommandList.Get()->Close();

            Graphics.CopyQueue.Execute(_commandList);
            ulong fence = Graphics.CopyQueue.IncrementFence();
            Graphics.ComputeQueue.IncrementFence();
            Graphics.GraphicsQueue.IncrementFence();

            Graphics.DownloadHeaps.Signal();
            Graphics.CopyQueue.AllocatorPool.Signal();

            Graphics.CopyQueue.WaitForFence(fence, Timeout.Infinite);

            NativeMemory.Copy(allocation.CpuPointer, (void*)image.ImageData, (nuint)image.SizeInBytes);

            if (originalFormat != image.Format)
            {
                Graphics.Log.Print($"The image format ({image.Format}) was different from the texture '{texture.Name}' format ({texture.Format}). The image will be restored to its original format.", LoggingLevel.Verbose);
                image.BeginUpdate().ConvertToFormat(originalFormat).EndUpdate();
            }
        }
        finally
        {
            Graphics.CopyQueue.ListPool.Return(_commandList);
            Cleanup();
        }
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.CopyTextureToBuffer(GorgonTexture texture, GorgonGpuBuffer_OLDE buffer, long destinationOffset)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(destinationOffset, 0);

        if ((buffer.SizeInBytes - destinationOffset) < texture.SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, destinationOffset, texture.SizeInBytes, buffer.SizeInBytes - destinationOffset), nameof(buffer));
        }

        ID3D12Resource* srcResource = (PID3D12Resource2)texture.D3DResource.Get();
        ID3D12Resource* destResource;
        ulong resourceOffset = (ulong)destinationOffset;

        if (buffer.Usage == BufferUsage.DynamicPerFrame)
        {
            ref readonly CpuBufferAllocation resource = ref buffer.GetGpuAddress();
            Debug.Assert(resource.IsAvailable, $"Destination resource '{buffer.Name}' not available.");
            destResource = (PID3D12Resource2)resource.Heap.D3DResource.Get();
            resourceOffset += resource.Offset;
        }
        else
        {
            destResource = (PID3D12Resource2)buffer.D3DResource.Get();
        }

        PrepDelayedWrites();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        texture.Info.ToD3DResourceDesc(out D3D12_RESOURCE_DESC1 desc);
        
        for (int i = 0; i < texture.SubResources.Count; ++i)
        {
            GorgonSubResourceInfo subInfo = texture.SubResources[i];
            D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = subInfo.ToD3DPlacedSubResourceFootPrint(texture.Format, resourceOffset);
            D3D12_TEXTURE_COPY_LOCATION srcLoc = new(srcResource, (uint)i);
            D3D12_TEXTURE_COPY_LOCATION destLoc = new(destResource, in footPrint);

            _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, force: buffer.Usage == BufferUsage.DynamicPerFrame);
            if (buffer.Usage != BufferUsage.DynamicPerFrame)
            {
                _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination, true);
            }

            _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, 0, 0, 0, &srcLoc, null);            
        }

        return this;
    }


    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.CopyBufferToTexture(GorgonGpuBuffer_OLDE buffer, GorgonTexture texture, CopyBufferToTextureParams parameters)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(parameters.SourceOffset, 0);

        if (buffer.Usage == BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_DOWNLOAD, buffer.Name));
        }

        // We don't need to limit the destination values because the GetSubResourceIndex method will ensure we can't go beyond the limits of the 
        // texture sub resources.
        int subResourceIndex = texture.GetSubResourceIndex(parameters.DestinationMipLevel, parameters.DestinationArrayIndex, parameters.DestinationPlane);
        GorgonSubResourceInfo subInfo = texture.SubResources[subResourceIndex];
                
        if ((buffer.SizeInBytes - parameters.SourceOffset) < subInfo.SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, parameters.SourceOffset, subInfo.SizeInBytes, buffer.SizeInBytes - parameters.SourceOffset), nameof(buffer));
        }

        ID3D12Resource* srcResource;
        ulong resourceOffset = (ulong)parameters.SourceOffset;

        if (buffer.Usage == BufferUsage.DynamicPerFrame)
        {
            ref readonly CpuBufferAllocation resource = ref buffer.GetGpuAddress();
            Debug.Assert(resource.IsAvailable, $"Source resource '{buffer.Name}' not available.");
            srcResource = (PID3D12Resource2)resource.Heap.D3DResource.Get();
            resourceOffset += resource.Offset;
        }
        else
        {
            srcResource = (PID3D12Resource2)buffer.D3DResource.Get();
        }
        ID3D12Resource* destResource = (PID3D12Resource2)texture.D3DResource.Get();

        PrepDelayedWrites();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = subInfo.ToD3DPlacedSubResourceFootPrint(texture.Format, resourceOffset);
        D3D12_TEXTURE_COPY_LOCATION srcLoc = new(srcResource, in footPrint);
        D3D12_TEXTURE_COPY_LOCATION destLoc = new(destResource, (uint)subResourceIndex);

        if (buffer.Usage != BufferUsage.DynamicPerFrame)
        {
            _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopySource);
        }
        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, 0, 0, 0, &srcLoc, null);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.CopyBufferToTexture(GorgonGpuBuffer_OLDE buffer, GorgonTexture texture, long sourceOffset)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sourceOffset, 0);

        if (buffer.Usage == BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_DOWNLOAD, buffer.Name));
        }

        if ((buffer.SizeInBytes - sourceOffset) < texture.SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, sourceOffset, texture.SizeInBytes, buffer.SizeInBytes - sourceOffset), nameof(buffer));
        }

        ID3D12Resource* srcResource;
        ulong resourceOffset = (ulong)sourceOffset;

        if (buffer.Usage == BufferUsage.DynamicPerFrame)
        {
            ref readonly CpuBufferAllocation resource = ref buffer.GetGpuAddress();
            Debug.Assert(resource.IsAvailable, $"Source resource '{buffer.Name}' not available.");
            srcResource = (PID3D12Resource2)resource.Heap.D3DResource.Get();
            resourceOffset += resource.Offset;
        }
        else
        {
            srcResource = (PID3D12Resource2)buffer.D3DResource.Get();
        }
        ID3D12Resource* destResource = (PID3D12Resource2)texture.D3DResource.Get();

        PrepDelayedWrites();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        if (buffer.Usage != BufferUsage.DynamicPerFrame)
        {
            _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopySource);
        }
        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        int depth = texture.Depth;  

        for (int a = 0; a < texture.ArrayCount; ++a)
        {
            for (int m = 0; m < texture.MipCount; ++m)
            {
                for (int p = 0; p < Graphics.FormatSupport[texture.Format].PlaneCount; ++p)
                {
                    int subResourceIndex = texture.GetSubResourceIndex(a, m, p);
                    GorgonSubResourceInfo subResourceInfo = texture.SubResources[subResourceIndex];

                    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = new()
                    {
                        Offset = resourceOffset,
                        Footprint = new D3D12_SUBRESOURCE_FOOTPRINT((DXGI_FORMAT)texture.Format, (uint)subResourceInfo.Width, (uint)subResourceInfo.Height, (uint)subResourceInfo.Depth, (uint)subResourceInfo.RowPitch)
                    };

                    resourceOffset += (ulong)subResourceInfo.Offset;

                    D3D12_TEXTURE_COPY_LOCATION srcLoc = new(srcResource, in footPrint);
                    D3D12_TEXTURE_COPY_LOCATION destLoc = new(destResource, (uint)subResourceIndex);

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
    IGorgonResourceWriter IGorgonResourceWriter.CopyTextureToBuffer(GorgonTexture texture, short sourceMipLevel, short sourceZOrArrayIndex, byte sourcePlane, GorgonGpuBuffer_OLDE buffer, long destinationOffset)
    {
        ID3D12Resource* srcResource = (PID3D12Resource2)texture.D3DResource.Get();
        ID3D12Resource* destResource;
        ulong resourceOffset = (ulong)destinationOffset;

        if (buffer.Usage == BufferUsage.DynamicPerFrame)
        {
            ref readonly CpuBufferAllocation resource = ref buffer.GetGpuAddress();
            Debug.Assert(resource.IsAvailable, $"Destination resource '{buffer.Name}' not available.");
            destResource = (PID3D12Resource2)resource.Heap.D3DResource.Get();
            resourceOffset += resource.Offset;
        }
        else
        {
            destResource = (PID3D12Resource2)buffer.D3DResource.Get();
        }

        sourceMipLevel = sourceMipLevel.Min((short)(texture.MipCount - 1)).Max(0);

        int width = texture.GetMipWidth(sourceMipLevel);
        int height = texture.GetMipHeight(sourceMipLevel);
        short depth = texture.GetMipDepth(sourceMipLevel);

        if (texture.Type != TextureType.Texture3D)
        {
            sourceZOrArrayIndex = sourceZOrArrayIndex.Min((short)(texture.ArrayCount - 1)).Max(0);
        }

        int subResourceIndex = texture.GetSubResourceIndex(sourceMipLevel, sourceZOrArrayIndex, sourcePlane);

        GorgonSubResourceInfo subInfo = texture.SubResources[subResourceIndex];

        ArgumentOutOfRangeException.ThrowIfNotEqual(buffer.SizeInBytes, subInfo.SizeInBytes, nameof(buffer));

        PrepDelayedWrites();

        _commandQueue.Tracker.TrackResource(buffer.D3DResource);
        _commandQueue.Tracker.TrackResource(texture.D3DResource);

        texture.Info.ToD3DResourceDesc(out D3D12_RESOURCE_DESC1 desc);

        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = texture.SubResources[subResourceIndex].ToD3DPlacedSubResourceFootPrint(texture.Format, 0);
        footPrint.Offset = resourceOffset;

        D3D12_TEXTURE_COPY_LOCATION srcLoc = new(srcResource, (uint)subResourceIndex);
        D3D12_TEXTURE_COPY_LOCATION destLoc = new(destResource, in footPrint);

        if (buffer.Usage != BufferUsage.DynamicPerFrame)
        {
            _commandList.SetBarrier(buffer, BarrierSync.Copy, BarrierAccess.CopyDestination);
        }
        _commandList.SetBarrier(texture, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource, force: true);

        _commandList.D3DGraphicsCommandList.Get()->CopyTextureRegion(&destLoc, 0, 0, 0, &srcLoc, null);

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.CopyTexture(GorgonTexture source, GorgonTexture destination, in CopyTextureSubResourceParams parameters)
    {
        ValidateCopyTexture(source, destination, in parameters);

        GorgonBox srcRegion = parameters.SourceRegion;

        CopyTextureSubResourceParams newParameters = Clip(source, destination, in parameters, out bool isFullSubResource);

        if (newParameters.IsEmpty)
        {
            return this;
        }

        switch (source.Type)
        {
            case TextureType.Texture1D:
                Copy1DTexture(source, destination, new GorgonRange<int>(srcRegion.Left, srcRegion.Right), srcRegion.Front, newParameters.SourceMipLevel, newParameters.SourcePlane, newParameters.DestinationX, newParameters.DestinationY, newParameters.DestinationZOrArrayIndex, newParameters.DestinationMipLevel, newParameters.DestinationPlane, isFullSubResource);
                break;
            case TextureType.Texture2D:
                Copy2DTexture(source, destination, (GorgonRectangle)srcRegion, srcRegion.Front, newParameters.SourceMipLevel, newParameters.SourcePlane, newParameters.DestinationX, newParameters.DestinationY, newParameters.DestinationZOrArrayIndex, newParameters.DestinationMipLevel, newParameters.DestinationPlane, isFullSubResource);
                break;
            case TextureType.Texture3D:
                Copy3DTexture(source, destination, srcRegion, newParameters.SourceMipLevel, newParameters.SourcePlane, newParameters.DestinationX, newParameters.DestinationY, newParameters.DestinationZOrArrayIndex, newParameters.DestinationMipLevel, newParameters.DestinationPlane, isFullSubResource);
                break;
            default:
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, source.Type), nameof(source));
        }

        return this;
    }

    /// <inheritdoc/>
    IGorgonResourceWriter IGorgonResourceWriter.CopyTexture(GorgonTexture source, GorgonTexture destination)
    {
        ValidateCopyTexture(source, destination, CopyTextureSubResourceParams.Empty);

        GorgonBox srcDimensions = new(0, 0, 0, source.Width, source.Height, source.Depth);

        if ((destination.Type != source.Type) || (source.FormatInfo.Group != destination.FormatInfo.Group)
            || (source.MipCount != destination.MipCount) || (source.ArrayCount != destination.ArrayCount)
            || (source.Width != destination.Width) || (source.Height != destination.Height) || (source.Depth != destination.Depth))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORGFX_ERR_CANNOT_COPY_TEXTURE_NOT_SAME, source.Name, destination.Name));
        }

        ID3D12Resource* srcRes = (PID3D12Resource2)source.D3DResource.Get();
        ID3D12Resource* destRes = (PID3D12Resource2)destination.D3DResource.Get();

        PrepDelayedWrites();

        _commandList.SetBarrier(source, BarrierSync.Copy, BarrierAccess.CopySource, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopySource);
        _commandList.SetBarrier(destination, BarrierSync.Copy, BarrierAccess.CopyDestination, _commandQueue.Type == D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY ? BarrierLayout.Common : BarrierLayout.CopyDestination, force: true);

        _commandQueue.Tracker.TrackResource(source.D3DResource);
        _commandQueue.Tracker.TrackResource(destination.D3DResource);

        _commandList.D3DGraphicsCommandList.Get()->CopyResource(destRes, srcRes);

        return this;
    }

#pragma warning disable CA1822 // Mark members as static
    /// <inheritdoc/>
    public void CopyToPointer<T>(GorgonGpuBuffer_OLDE buffer, GorgonPtr<T> pointer)
        where T : unmanaged
    {
        if (buffer.Usage != BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_BUFFER_USAGE_NOT_DOWNLOAD, buffer.Name));
        }

        nuint sizeInBytes = (nuint)buffer.SizeInBytes.Min(pointer.SizeInBytes);
        NativeMemory.Copy((void*)buffer.CpuData, (void*)pointer, sizeInBytes);
    }

    /// <inheritdoc/>
    public void CopyToRange<T>(GorgonGpuBuffer_OLDE buffer, Span<T> range)
        where T : unmanaged
    {
        if (buffer.Usage != BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_BUFFER_USAGE_NOT_DOWNLOAD, buffer.Name));
        }

        int typeSize = sizeof(T);
        fixed (T* tPtr = range)
        {
            nuint sizeInBytes = (nuint)buffer.SizeInBytes.Min(typeSize * range.Length);

            NativeMemory.Copy((void*)buffer.CpuData, tPtr, sizeInBytes);
        }
    }

    /// <inheritdoc/>
    public void CopyToValue<T>(GorgonGpuBuffer_OLDE buffer, ref T value)
        where T : unmanaged
    {
        if (buffer.Usage != BufferUsage.Download)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_BUFFER_USAGE_NOT_DOWNLOAD, buffer.Name));
        }

        int typeSize = sizeof(T);
        fixed (T* tPtr = &value)
        {
            nuint sizeInBytes = (nuint)buffer.SizeInBytes.Min(typeSize);

            NativeMemory.Copy((void*)buffer.CpuData, tPtr, sizeInBytes);
        }
    }
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonResourceCopier"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that is associated with this writer.</param>
    public GorgonResourceCopier(GorgonGraphics graphics)
    {
        Graphics = graphics;
        _commandQueue = graphics.CopyQueue;
        _commandQueues[0] = graphics.GraphicsQueue;
        _commandQueues[1] = graphics.ComputeQueue;
    }
}