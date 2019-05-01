#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 22, 2017 10:31:48 AM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides a read/write (unordered access) view for a <see cref="GorgonBuffer"/>, <see cref="GorgonVertexBuffer"/> or a <see cref="GorgonIndexBuffer"/>.
    /// </summary>
    /// <typeparam name="T">The type of buffer for this view. Must inherit from <see cref="GorgonBufferCommon"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a buffer. The buffer must have been created with the <see cref="BufferBinding.ReadWrite"/> flag in its binding property.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallCommon">draw call</see>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallCommon"/>
    public abstract class GorgonBufferReadWriteViewCommon<T>
        : GorgonReadWriteView
        where T : GorgonBufferCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return the buffer associated with this view.
        /// </summary>
        public T Buffer
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the offset of the view from first element in the buffer.
        /// </summary>
        public int StartElement
        {
            get;
        }

        /// <summary>
        /// Property to return the number of elements in the view.
        /// </summary>
        public int ElementCount
        {
            get;
        }

        /// <summary>
        /// Property to return the total number of elements in the <see cref="Buffer"/>.
        /// </summary>
        public int TotalElementCount
        {
            get;
        }

        /// <summary>
        /// Property to return the size of an element.
        /// </summary>
        public abstract int ElementSize
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferReadWriteViewCommon{T}"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="totalElementCount">The total number of elements in the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        protected GorgonBufferReadWriteViewCommon(T buffer, int elementStart, int elementCount, int totalElementCount)
            : base(buffer)
        {
            StartElement = elementStart;
            ElementCount = elementCount;
            TotalElementCount = totalElementCount;
            Buffer = buffer;
        }
        #endregion
    }
}
