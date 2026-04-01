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
/// Common settings used for creating buffer types.
/// </summary>
public abstract record class GorgonCommonBufferInfo
{
    /// <inheritdoc cref="IGorgonCommonBufferInfo.SizeInBytes"/>
    /// <remarks>
    /// <para>
    /// This value must be greater than 0.
    /// </para>
    /// </remarks>
    public long SizeInBytes
    {
        get;
        init;
    }

    /// <inheritdoc cref="IGorgonCommonBufferInfo.Usage"/>
    public BufferUsage Usage
    {
        get;
        init;
    }

    /// <inheritdoc cref="IGorgonCommonBufferInfo.IsUnorderedAccess"/>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool IsUnorderedAccess
    {
        get;
        init;
    } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCommonBufferInfo"/> class.
    /// </summary>
    /// <param name="sizeInBytes">The size of the buffer, in bytes.</param>
    /// <param name="usage">The intended usage for the buffer.</param>
    protected GorgonCommonBufferInfo(long sizeInBytes, BufferUsage usage)
    {
        SizeInBytes = sizeInBytes;
        Usage = usage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCommonBufferInfo"/> class.
    /// </summary>
    /// <param name="copy">The record to copy.</param>
    protected GorgonCommonBufferInfo(GorgonCommonBufferInfo copy)
    {
        SizeInBytes = copy.SizeInBytes;
        Usage = copy.Usage;
        IsUnorderedAccess = copy.IsUnorderedAccess;
    }
}

