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
// Created: June 23, 2026 7:23:18 PM
//

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides transient GPU memory for uploading data.
/// </summary>
/// <remarks>
/// <para>
/// This allows applications to allocate a block of transient memory on the GPU that can be written to directly without need of the <see cref="GorgonResourceCopier"/>. This functionality is typically used 
/// when copying large amounts of data to the GPU without requiring a costly intermediate storage object like an array.
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// The underlying GPU memory is transient and is only available for the lifetime of a single frame. This is why this memory is a <c>ref struct</c> type, it helps ensure the memory cannot be used across 
/// frames by not allowing the memory to be kept alive at the object level.
/// </para>
/// </note>
/// </para>
/// <para>
/// Due to how Gorgon works internally, the data in transient GPU memory needs to be placed into a <see cref="GorgonGpuBuffer"/> so it can be used with the bindless mechanism. This is done by calling the 
/// <see cref="GorgonCommandList.UploadGpuMemoryToBuffer(ref readonly GorgonGpuUploadMemory, GorgonGpuBufferCommon, long)"/> method on a <see cref="GorgonCommandList"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonResourceCopier"/>
/// <seealso cref="GorgonGpuBuffer"/>
/// <seealso cref="GorgonCommandList"/>
public unsafe readonly ref struct GorgonGpuUploadMemory
{
    /// <summary>
    /// The transient GPU memory allocation information.
    /// </summary>
    internal readonly CpuBufferAllocation Allocation;

    /// <summary>
    /// The size of the upload memory block, in bytes.
    /// </summary>
    public readonly long SizeInBytes;

    /// <summary>
    /// Property to return whether the allocation is still valid or not.
    /// </summary>
    public readonly bool IsValid => Allocation.IsAvailable;

    /// <summary>
    /// Function to write a single value to transient memory.
    /// </summary>
    /// <typeparam name="T">The type of data to write.</typeparam>
    /// <param name="value">The value to write into memory.</param>
    /// <param name="offset">The offset, in bytes, in memory to start writing at.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="offset"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the number of bytes to copy plus the <paramref name="offset"/> exceeds <see cref="SizeInBytes"/>.</exception>
    /// <exception cref="GorgonException">Thrown if the memory is no longer valid for use.</exception>
    /// <remarks>
    /// <para>
    /// Since the transient memory is only valid for a single frame, applications must ensure they do not try to write to the memory after the current frame has ended. If an attempt to write to the memory 
    /// is made, then an exception will be thrown.
    /// </para>
    /// </remarks>
    public void Write<T>(in T value, long offset = 0) where T : unmanaged
    {
        if (!IsValid)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORGFX_ERR_TRANSIENT_MEMORY_NOT_VALID);
        }

        long size = sizeof(T);

        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (offset + size > SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, offset, size, SizeInBytes));
        }

        T* ptr = (T*)Unsafe.AsPointer(in value);

        NativeMemory.Copy(ptr, Allocation.CpuPointer + offset, (nuint)size);
    }

    /// <summary>
    /// Function to write a span of values to transient memory.
    /// </summary>
    /// <inheritdoc cref="Write{T}(in T, long)" path="/typeparam"/>
    /// <param name="values">The span containing the values to copy.</param>
    /// <param name="offset"><inheritdoc cref="Write{T}(in T, long)" path="/param[@name='offset']"/></param>
    /// <inheritdoc cref="Write{T}(in T, long)" path="/exception"/>
    /// <inheritdoc cref="Write{T}(in T, long)" path="/remarks"/>
    public void WriteRange<T>(ReadOnlySpan<T> values, long offset = 0) where T : unmanaged
    {
        if (!IsValid)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORGFX_ERR_TRANSIENT_MEMORY_NOT_VALID);
        }

        if (values.IsEmpty)
        {
            return;
        }

        long size = (long)sizeof(T) * values.Length;

        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (offset + size > SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, offset, size, SizeInBytes));
        }

        fixed (T* ptr = values)
        {
            NativeMemory.Copy(ptr, Allocation.CpuPointer + offset, (nuint)size);
        }
    }

    /// <summary>
    /// Function to write the contents of native memory pointed at by a <see cref="GorgonPtr{T}"/> to transient memory.
    /// </summary>
    /// <inheritdoc cref="Write{T}(in T, long)" path="/typeparam"/>
    /// <param name="pointer">The pointer to the memory to copy.</param>
    /// <param name="offset"><inheritdoc cref="Write{T}(in T, long)" path="/param[@name='offset']"/></param>
    /// <inheritdoc cref="Write{T}(in T, long)" path="/exception"/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="pointer"/> is equal to <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <inheritdoc cref="Write{T}(in T, long)" path="/remarks"/>
    public void WritePointer<T>(GorgonPtr<T> pointer, long offset = 0) where T : unmanaged
    {
        if (!IsValid)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORGFX_ERR_TRANSIENT_MEMORY_NOT_VALID);
        }

        if (pointer.Equals(GorgonPtr<T>.NullPtr))
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (offset + pointer.SizeInBytes > SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, offset, pointer.SizeInBytes, SizeInBytes));
        }

        NativeMemory.Copy((void*)pointer, Allocation.CpuPointer + offset, (nuint)pointer.SizeInBytes);
    }

    /// <summary>
    /// Function to return a slice of the transient memory to use.
    /// </summary>
    /// <param name="offset">The offset, in bytes, to start the slice at.</param>
    /// <param name="count">[Optional] The number of bytes to slice.</param>    
    /// <returns>A new <see cref="GorgonGpuUploadMemory"/> for the specified slice region.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/>, plus the <paramref name="count"/> are larger than the <see cref="SizeInBytes"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><para>Thrown if the <paramref name="offset"/> parameter is less than 0.</para>
    /// <para>Thrown if the <paramref name="count"/>, if specified, is less than 1.</para>
    /// </exception>
    /// <exception cref="GorgonException"><inheritdoc cref="Write{T}(in T, long)" path="/exception[@cref='T:Gorgon.Core.GorgonException'"/></exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is not supplied, then the <see cref="SizeInBytes"/> minus the <paramref name="offset"/> is used.
    /// </para>
    /// <para>
    /// Since the transient memory is only valid for a single frame, applications must ensure they do not try to slice the memory after the current frame has ended. If an attempt to slice the memory 
    /// is made, then an exception will be thrown.
    /// </para>
    /// </remarks>
    public GorgonGpuUploadMemory Slice(long offset, long? count = null)
    {
        if (!IsValid)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORGFX_ERR_TRANSIENT_MEMORY_NOT_VALID);
        }

        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        long bytesToCopy = count ?? (SizeInBytes - offset);

        ArgumentOutOfRangeException.ThrowIfLessThan(bytesToCopy, 1, nameof(count));

        if (offset + bytesToCopy > SizeInBytes)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_OVERRUN, offset, bytesToCopy, SizeInBytes));
        }

        CpuBufferAllocation newAlloc = new(Allocation.Heap, Allocation.Handle, Allocation.Offset + (ulong)offset);

        return new GorgonGpuUploadMemory(newAlloc, bytesToCopy);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuUploadMemory"/> value type.
    /// </summary>
    /// <param name="allocation">The allocation backing store to use.</param>
    /// <param name="size">The size of the transient memory.</param>
    internal GorgonGpuUploadMemory(CpuBufferAllocation allocation, long size)
    {
        Allocation = allocation;
        SizeInBytes = size;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuUploadMemory"/> value type.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with the upload memory.</param>
    /// <param name="size">The size, in bytes, of the upload memory block.</param>
    internal GorgonGpuUploadMemory(GorgonGraphics graphics, long size)
    {
        SizeInBytes = size;
        graphics.Memory.UploadHeaps.Allocate((ulong)size, graphics.Adapter.HasTightAlignmentSupport ? 0 : D3D12.D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT, out Allocation);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuUploadMemory"/> value type.
    /// </summary>
    public GorgonGpuUploadMemory()
    {
        SizeInBytes = 0;
        Allocation = CpuBufferAllocation.Null;
    }
}
