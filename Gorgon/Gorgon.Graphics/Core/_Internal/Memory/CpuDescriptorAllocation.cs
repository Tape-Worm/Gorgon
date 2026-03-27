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
// Created: January 3, 2026 2:09:37 PM
//

using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An allocation of descriptors from the descriptor heap.
/// </summary>
/// <param name="heap"><inheritdoc cref="Heap" path="/summary"/></param>
/// <param name="handle"><inheritdoc cref="Handle" path="/summary"/></param>
/// <param name="count"><inheritdoc cref="Count" path="/summary"/></param>
/// <param name="cpuHandle"><inheritdoc cref="CpuHandle" path="/summary"/></param>
internal readonly struct CpuDescriptorAllocation(CpuDescriptorHeap? heap, ulong handle, uint count, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    : IEquatable<CpuDescriptorAllocation>
{
    /// <summary>
    /// The heap that this allocation comes from.
    /// </summary>
    public readonly CpuDescriptorHeap? Heap = heap;

    /// <summary>
    /// The handle to the location in the virtual block.
    /// </summary>
    public readonly ulong Handle = handle;

    /// <summary>
    /// The number of descriptors in the allocation.
    /// </summary>
    public readonly uint Count = count;

    /// <summary>
    /// The CPU handle for the descriptor.
    /// </summary>
    public readonly D3D12_CPU_DESCRIPTOR_HANDLE CpuHandle = cpuHandle;

    /// <summary>
    /// A null allocation.
    /// </summary>
    public static readonly CpuDescriptorAllocation Null = new(null, 0UL, 0u, D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT);

    /// <summary>
    /// Property to return whether this allocation is <b>null</b> or not.
    /// </summary>
    public readonly bool IsNull => Equals(in Null);

    /// <inheritdoc cref="Equals(CpuDescriptorAllocation)"/>
    public readonly bool Equals(ref readonly CpuDescriptorAllocation allocation) => Handle == allocation.Handle && Handle == allocation.Handle;

    /// <inheritdoc/>
    public bool Equals(CpuDescriptorAllocation other) => Equals(in other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CpuDescriptorAllocation alloc && Equals(in alloc);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Heap, Handle);
}
