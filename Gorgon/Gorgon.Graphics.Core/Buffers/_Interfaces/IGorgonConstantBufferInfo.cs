#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 28, 2016 8:42:36 PM
// 
#endregion


using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides information on how to set up a constant buffer.
/// </summary>
/// <remarks>
/// <para>
/// This provides an immutable view of the constant buffer information so that it cannot be modified after the buffer is created.
/// </para>
/// </remarks>
public interface IGorgonConstantBufferInfo
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the intended usage flags for this texture.
    /// </summary>
    /// <remarks>
    /// This value is defaulted to <see cref="ResourceUsage.Default"/>.
    /// </remarks>
    ResourceUsage Usage
    {
        get;
    }

    /// <summary>
    /// Property to return the number of bytes to allocate for the buffer.
    /// </summary>
    /// <remarks>
    /// <note type="important">
    /// <para>
    /// <para>
    /// A <see cref="BufferType.Constant"/> buffer, must set the size to be a multiple of 16. Constant buffer alignment rules require that they be sized to the nearest 16 bytes.
    /// </para>
    /// <para>
    /// If the buffer is not sized to a multiple of 16, Gorgon will attempt to adjust the size to fit the alignment requirement.
    /// </para>
    /// </para>
    /// </note>
    /// </remarks>
    int SizeInBytes
    {
        get;
    }
}
