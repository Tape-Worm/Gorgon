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
// Created: March 22, 2026 6:54:13 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An allocation of descriptors from the GPU descriptor heap.
/// </summary>
/// <param name="handle"><inheritdoc cref="Handle" path="/summary"/></param>
/// <param name="offset"><inheritdoc cref="Offset" path="/summary"/></param>
/// <param name="count"><inheritdoc cref="Count" path="/summary"/></param>
internal readonly struct GpuDescriptorAllocation(ulong handle, int offset, uint count)
    : IEquatable<GpuDescriptorAllocation>
{
    /// <summary>
    /// A null allocation.
    /// </summary>
    public static readonly GpuDescriptorAllocation Null = new(0, 0, 0);

    /// <summary>
    /// The handle to the location in the virtual block.
    /// </summary>
    public readonly ulong Handle = handle;

    /// <summary>
    /// The offset of the descriptor within the heap.
    /// </summary>
    public readonly int Offset = offset;

    /// <summary>
    /// The number of descriptors in the allocation.
    /// </summary>
    public readonly uint Count = count;

    /// <summary>
    /// Property to return whether this allocation is <b>null</b> or not.
    /// </summary>
    public readonly bool IsNull => Equals(in Null);

    /// <inheritdoc cref="Equals(GpuDescriptorAllocation)"/>    
    public readonly bool Equals(ref readonly GpuDescriptorAllocation allocation) => Handle == allocation.Handle;

    /// <inheritdoc/>
    public bool Equals(GpuDescriptorAllocation other) => Equals(in other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GpuDescriptorAllocation alloc && Equals(in alloc);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();
}
