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
// Created: March 26, 2026 9:01:06 PM
//

using System;
using System.Collections.Generic;
using System.Text;
using TerraFX.Interop.Windows;
using TerraFX.Interop.DirectX;
using Win32 = TerraFX.Interop.Windows.Windows;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common functionality for GPU buffers.
/// </summary>
/// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
/// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
/// <param name="info">Information used to create the buffer.</param>
public abstract class GorgonGpuBufferCommon(GorgonGraphics graphics, string name, GorgonCommonBufferInfo info)
        : GorgonGpuResource(graphics, name), IGorgonCommonBufferInfo
{
    private readonly GorgonCommonBufferInfo _info = info;

    /// <inheritdoc/>
    public long SizeInBytes => _info.SizeInBytes;

    /// <inheritdoc/>
    public BufferUsage Usage => _info.Usage;

    /// <inheritdoc/>
    public bool IsUnorderedAccess => _info.IsUnorderedAccess;

    /// <summary>
    /// Property to return the offset, in bytes, of a suballocated buffer within a larger buffer.
    /// </summary>
    internal abstract ulong ResourceOffset
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether the dynamic buffer has data that needs to be uploaded.
    /// </summary>
    internal bool NeedsDataUpload
    {
        get;
        private set;
    } = true;

    /// <summary>
    /// Function to validate the settings for the buffer.
    /// </summary>
    private protected abstract void ValidateInfo();

    /// <summary>
    /// Function to return the transient upload buffer for a dynamic buffer.
    /// </summary>
    /// <returns>A read only reference to the CPU buffer allocation backing this dynamic buffer.</returns>
    internal abstract ref readonly CpuBufferAllocation GetTransientBufferData();

    /// <summary>
    /// Function to copy CPU data to a dynamic buffer's transient buffer.
    /// </summary>
    /// <param name="data">The pointer to the data to copy.</param>
    /// <param name="destOffset">The offset within the transient buffer to start writing into.</param>
    /// <param name="count">The number of bytes to copy.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void CopyCpuData(void* data, ulong destOffset, ulong count)
    {
        if (Usage != BufferUsage.DynamicPerFrame)
        {
            return;
        }

        ref readonly CpuBufferAllocation allocation = ref GetTransientBufferData();
        Debug.Assert(allocation.IsAvailable, $"The transient heap for '{Name}' is not valid.");

        void* dest = allocation.CpuPointer + destOffset;

        NativeMemory.Copy(data, dest, (nuint)count);

        NeedsDataUpload = true;
    }

    /// <summary>
    /// Function to trigger an upload from the internal transient buffer to the backing default resource for the buffer object.
    /// </summary>
    /// <param name="list">The command list used to trigger the copy operation.</param>
    /// <param name="setBarrier"><b>true</b> to set the appropriate barrier before copying, <b>false</b> if the barrier has already been set externally.</param>
    /// <remarks>
    /// <para>
    /// This method only executes if the buffer usage is <see cref="BufferUsage.DynamicPerFrame"/>, and <see cref="NeedsDataUpload"/> is <b>true</b>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void FlushDynamicBuffer(GorgonCommandList list, bool setBarrier)
    {
        if ((Usage != BufferUsage.DynamicPerFrame) || (!NeedsDataUpload))
        {
            return;
        }

        ref readonly CpuBufferAllocation allocation = ref GetTransientBufferData();
        Debug.Assert(allocation.IsAvailable, $"The transient heap for '{Name}' is not valid.");

        if (setBarrier)
        {
            list.SetBarrier(this, BarrierSync.Copy, BarrierAccess.CopyDestination, true);
        }

        list.D3DGraphicsCommandList.Get()->CopyBufferRegion((PID3D12Resource2)D3DResource.Get(),
            ResourceOffset,
            (PID3D12Resource2)allocation.Heap.D3DResource.Get(),
            allocation.Offset,
            (ulong)SizeInBytes);

        NeedsDataUpload = false;
    }

#warning REMOVETHIS: Some stuff is still using it, but it'll need to be removed in short order.    
    private protected override ComPtr<ID3D12Resource2> OnCreateNative(out D3D12_RESOURCE_DESC1 desc)
    {
        desc = default;
        return default;
    }
}
