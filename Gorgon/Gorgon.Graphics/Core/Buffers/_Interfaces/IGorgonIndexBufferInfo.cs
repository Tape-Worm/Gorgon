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
// Created: January 14, 2026 9:41:53 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information that was used to build a <see cref="GorgonIndexBuffer"/>.
/// </summary>
public interface IGorgonIndexBufferInfo
    : IGorgonCommonBufferInfo
{
    /// <summary>
    /// Property to return whether the buffer stores 32 bit indices or 16 bit indices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <b>true</b>, Gorgon will expect each element in the buffer to be 32 bits wide. Otherwise, if <b>false</b>, then each element must be 16 bits wide.
    /// </para>
    /// </remarks>
    bool Use32BitIndices
    {
        get;
    }
}