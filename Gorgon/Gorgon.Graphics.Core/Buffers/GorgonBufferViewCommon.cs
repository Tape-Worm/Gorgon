
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 5, 2017 2:53:14 PM
// 

namespace Gorgon.Graphics.Core;

/// <summary>
/// The base class for buffer shader views
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonBufferViewCommon"/> class
/// </remarks>
/// <param name="buffer">The buffer to bind to the view.</param>
/// <param name="startingElement">The starting element in the buffer to view.</param>
/// <param name="elementCount">The number of elements in the buffer to view.</param>
/// <param name="totalElementCount">The total number of elements in the buffer.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
public abstract class GorgonBufferViewCommon(GorgonBuffer buffer, int startingElement, int elementCount, int totalElementCount)
        : GorgonShaderResourceView(buffer)
{
    /// <summary>
    /// Property to return the buffer associated with the view.
    /// </summary>
    public GorgonBuffer Buffer
    {
        get;
        protected set;
    } = buffer;

    /// <summary>
    /// Property to return the starting element.
    /// </summary>
    public int StartElement
    {
        get;
    } = startingElement;

    /// <summary>
    /// Property to return the number of elements.
    /// </summary>
    public int ElementCount
    {
        get;
    } = elementCount;

    /// <summary>
    /// Property to return the total number of elements in the <see cref="Buffer"/>.
    /// </summary>
    public int TotalElementCount
    {
        get;
    } = totalElementCount;

    /// <summary>
    /// Property to return the size of an element, in bytes.
    /// </summary>
    public abstract int ElementSize
    {
        get;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        Buffer = null;
        base.Dispose();
    }
}
