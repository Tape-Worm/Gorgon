﻿// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: February 2, 2024 8:37:37 PM
//

using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// A chunk for the <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File Format (GCFF)</conceptualLink>.
/// </summary>
/// <param name="id">The identifier for the chunk.</param>
/// <param name="size">The size of the chunk, in bytes.</param>
/// <param name="offset">The offset within the file, in bytes.</param>
public readonly struct GorgonChunk(ulong id, int size, ulong offset)
        : IGorgonEquatableByRef<GorgonChunk>
{
    /// <summary>
    /// An empty chunk.
    /// </summary>
    public static readonly GorgonChunk EmptyChunk;

    /// <summary>
    /// The ID for the chunk.
    /// </summary>
    public readonly ulong ID = id;

    /// <summary>
    /// The size of the chunk, in bytes.
    /// </summary>
    public readonly int Size = size;

    /// <summary>
    /// The offset, in bytes, of the chunk within the chunked file.
    /// </summary>
    /// <remarks>This is relative to the header of the file.</remarks>
    public readonly ulong FileOffset = offset;

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => string.Format(Resources.GOR_TOSTR_GORGONCHUNK, ID.FormatHex(), FileOffset.FormatHex(), Size.FormatHex());

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode() => ID.GetHashCode();

    /// <summary>
    /// Function to compare two instances for equality.
    /// </summary>
    /// <param name="left">The first object of type <see cref="GorgonChunk"/> to compare.</param>
    /// <param name="right">The second object of type <see cref="GorgonChunk"/> to compare.</param>
    /// <returns><b>true</b> if the specified objects are equal; otherwise, <b>false</b> if not.</returns>
    public static bool Equals(ref readonly GorgonChunk left, ref readonly GorgonChunk right) => left.ID == right.ID;

    /// <summary>
    /// Determines whether the specified <see cref="object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is GorgonChunk chunk ? chunk.Equals(in this) : base.Equals(obj);

    /// <summary>
    /// Function to compare this instance with another.
    /// </summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(ref readonly GorgonChunk other) => Equals(in this, in other);

    /// <inheritdoc/>
    bool IEquatable<GorgonChunk>.Equals(GorgonChunk other) => Equals(in this, in other);

    /// <summary>
    /// Operator used to compare two instances for equality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> otherwise.</returns>
    public static bool operator ==(in GorgonChunk left, in GorgonChunk right) => Equals(in left, in right);

    /// <summary>
    /// Operator used to compare two instances for inequality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> otherwise.</returns>
    public static bool operator !=(in GorgonChunk left, in GorgonChunk right) => !Equals(in left, in right);
}
