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
// Created: February 8, 2024 10:00:44 PM
//

namespace Gorgon.IO;

/// <summary>
/// A collection of available chunks within a <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon Chunk File(GCFF)</conceptualLink>.
/// </summary>
public interface IGorgonReadOnlyChunkCollection
    : IReadOnlyList<GorgonChunk>
{
    /// <summary>
    /// Property to return a chunk by a string identifier.
    /// </summary>
    /// <remarks>
    /// If the chunk is not found, then this property will return <b>null</b>.
    /// </remarks>
    GorgonChunk this[string chunkName]
    {
        get;
    }

    /// <summary>
    /// Property to return a chunk by its <see cref="long"/> ID.
    /// </summary>
    /// <remarks>
    /// If the chunk is not found, then this property will return <b>null</b>.
    /// </remarks>
    GorgonChunk this[ulong id]
    {
        get;
    }

    /// <summary>
    /// Function to return whether a chunk exists in this collection or not.
    /// </summary>
    /// <param name="chunkName">A text representation of the <see cref="long"/> chunk ID.</param>
    /// <returns><b>true</b> if a chunk exists with the specified <paramref name="chunkName"/>, <b>false</b> if not.</returns>
    bool Contains(string chunkName);

    /// <summary>
    /// Function to return whether a chunk exists in this collection or not.
    /// </summary>
    /// <param name="chunkID">The <see cref="long"/> ID of the chunk.</param>
    /// <returns><b>true</b> if a chunk exists with the specified <paramref name="chunkID"/>, <b>false</b> if not.</returns>
    bool Contains(ulong chunkID);

    /// <summary>
    /// Function to return whether a chunk exists in this collection or not.
    /// </summary>
    /// <param name="chunk">The chunk to find in the collection.</param>
    /// <returns><b>true</b> if the <paramref name="chunk"/> exists, <b>false</b> if not.</returns>
    bool Contains(GorgonChunk chunk);
}
