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
// Created: January 16, 2026 8:54:38 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TerraFX.Interop.WinRT;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An allocation for a GPU/CPU accessible resource.
/// </summary>
/// <param name="heap"><inheritdoc cref="Heap" path="/summary"/></param>
/// <param name="handle"><inheritdoc cref="Handle" path="/summary"/></param>
/// <param name="offset"><inheritdoc cref="Offset" path="/summary"/></param>
internal readonly unsafe struct CpuBufferAllocation(CpuBufferHeap? heap, ulong handle, ulong offset)
    : IEquatable<CpuBufferAllocation>
{
    /// <summary>
    /// <inheritdoc cref="GpuDescriptorAllocation.Null"/>
    /// </summary>
    public static readonly CpuBufferAllocation Null = new(null, 0, 0);

    /// <summary>
    /// The upload/download heap that this allocation resides in.
    /// </summary>
    public readonly CpuBufferHeap? Heap = heap;

    /// <summary>
    /// The virtual allocation handle associated with the allocation.
    /// </summary>
    public readonly ulong Handle = handle;

    /// <summary>
    /// The offset within the heap for the resource data.
    /// </summary>
    public readonly ulong Offset = offset;

    /// <summary>
    /// Property to return whether this resource allocation can still be used in the current frame.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Heap))]
    public bool IsAvailable => (Heap is not null) && (Heap.GfxFence <= 0) && (Heap.ComputeFence <= 0) && (Heap.CopyFence <= 0) && (Heap.CpuPointer is not null) && (Heap.GpuAddress != 0);

    /// <summary>
    /// Property to return the address in the GPU memory for the resource data.
    /// </summary>
    public readonly ulong GpuAddress => ((Heap is null) || (Heap.GpuAddress == 0)) ? 0UL : Heap.GpuAddress + Offset;

    /// <summary>
    /// Property to return the pointer to the CPU addressable memory that the application can write.
    /// </summary>
    public readonly byte* CpuPointer => ((Heap is null) || (Heap.CpuPointer is null)) ? null : Heap.CpuPointer + Offset;

    /// <inheritdoc cref="Equals(CpuBufferAllocation)">
    public bool Equals(ref readonly CpuBufferAllocation other) => Heap == other.Heap && Handle == other.Handle;

    /// <inheritdoc/>
    public bool Equals(CpuBufferAllocation other) => Equals(in other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CpuBufferAllocation alloc && Equals(in alloc);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Heap, Handle);
}
