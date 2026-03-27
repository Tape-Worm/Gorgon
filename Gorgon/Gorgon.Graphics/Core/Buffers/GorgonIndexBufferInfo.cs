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
// Created: January 14, 2026 9:42:47 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Settings used for creating a <see cref="GorgonIndexBuffer"/>.
/// </summary>
/// <param name="SizeInBytes">The size of the buffer, in bytes.</param>
/// <param name="Use32BitIndices"><b>true</b> to indicate that each element in the buffer will be 32 bits wide, or <b>false</b> to indicate that each element will be 16 bits wide.</param>
/// <param name="Usage">The intended usage for the buffer.</param>
/// <remarks>
/// <para>
/// The <paramref name="SizeInBytes"/> parameter must be greater than 0.
/// </para>
/// </remarks>
public record class GorgonIndexBufferInfo(long SizeInBytes, bool Use32BitIndices, BufferUsage Usage)
    : GorgonCommonBufferInfo(SizeInBytes, Usage)
{
    /// <summary>
    /// An empty instance of the <see cref="GorgonGpuBuffer_OLDE"/> type.
    /// </summary>
    public static readonly GorgonIndexBufferInfo Empty = new(0, false, BufferUsage.Default);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonIndexBufferInfo(GorgonIndexBufferInfo info)
        : base(info) => Use32BitIndices = info.Use32BitIndices;

    /// <summary>
    /// Initializes a new instance of the <see cref="IGorgonGpuBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonIndexBufferInfo(IGorgonIndexBufferInfo info)
        : this(info.SizeInBytes, info.Use32BitIndices, info.Usage)
    {
    }
}

