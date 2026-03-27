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
// Created: March 23, 2026 8:29:07 PM
//

using System;
using System.Collections.Generic;
using System.Text;
using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An allocation block from the mega buffer.
/// </summary>
/// <param name="handle"><inheritdoc cref="Handle" path="/summary"/></param>
/// <param name="offset"><inheritdoc cref="Offset" path="/summary"/></param>
/// <param name="tileStart"><inheritdoc cref="TileStart" path="/summary"/></param>
/// <param name="tileEnd"><inheritdoc cref="TileEnd" path="/summary"/></param>
internal readonly struct GpuBufferAllocation(ulong handle, uint offset, ushort tileStart, ushort tileEnd)
    : IEquatable<GpuBufferAllocation>
{
    /// <summary>
    /// A representation for a null allocation.
    /// </summary>
    public static readonly GpuBufferAllocation Null = new();    

    /// <summary>
    /// The virtual allocation handle.
    /// </summary>
    public readonly ulong Handle = handle;

    /// <summary>
    /// The offset within the mega buffer that this allocation starts at.
    /// </summary>
    public readonly uint Offset = offset;

    /// <summary>
    /// The first tile that contains this allocation.
    /// </summary>
    public readonly ushort TileStart = tileStart;

    /// <summary>
    /// The last tile that contains the allocation.
    /// </summary>
    public readonly ushort TileEnd = tileEnd;

    /// <summary>
    /// Property to return if this allocation is <see cref="Null"/> or not.
    /// </summary>
    public readonly bool IsNull => Equals(in Null);

    /// <inheritdoc/>
    public readonly bool Equals(ref readonly GpuBufferAllocation other) => Handle == other.Handle;

    /// <inheritdoc/>
    public readonly bool Equals(GpuBufferAllocation other) => Equals(in other);

    /// <inheritdoc/>
    public readonly override bool Equals(object? obj) => obj is GpuBufferAllocation alloc && Equals(in alloc);

    /// <inheritdoc/>
    public readonly override int GetHashCode() => Handle.GetHashCode();
}
