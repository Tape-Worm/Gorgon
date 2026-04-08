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

using System;
using System.Collections.Generic;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information that was used to build a <see cref="GorgonGpuBuffer"/>.
/// </summary>
public interface IGorgonGpuBufferInfo
    : IGorgonCommonBufferInfo
{
    /// <summary>
    /// Property to return the number of bytes to align the buffer by.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is used to pad the buffer alignment so that it, and its data, starts on a power of 2 boundary.
    /// </para>
    /// <para>
    /// Buffers used in a <see cref="GorgonStructuredBufferView"/> should set this value to the size of the data element within the buffer, especially if the element size is not a power of 2.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// For buffers meant to be used as a <see cref="GorgonConstantBufferView"/>, this value <b>MUST</b> be set to <see cref="GorgonConstantBufferView.AlignmentRequirement"/> (256 bytes).
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    int Alignment
    {
        get;
    }
}