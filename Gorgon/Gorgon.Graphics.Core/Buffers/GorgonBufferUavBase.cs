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

using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides an unordered access view for a <see cref="GorgonBuffer"/>, <see cref="GorgonRawBuffer"/> or a <see cref="GorgonStructuredBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a buffer. The buffer must have been created with the <see cref="BufferBinding.UnorderedAccess"/> flag in its binding property.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallBase">draw call</see>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Unordered access views do not support multisampled <see cref="GorgonTexture"/>s.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonResource"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallBase"/>
    public abstract class GorgonBufferUavBase<T>
        : GorgonUnorderedAccessView
        where T : GorgonBufferBase
    {
        #region Properties.
        /// <summary>
        /// Property to return the log used for debug information.
        /// </summary>
        protected IGorgonLog Log
        {
            get;
        }

        /// <summary>
        /// Property to return the buffer associated with this view.
        /// </summary>
        public T Buffer
        {
            get;
        }

        /// <summary>
        /// Property to return the offset of the view from first element in the buffer.
        /// </summary>
        public int ElementStart
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
        public int TotalElementCount => Buffer.SizeInBytes / ElementSize;

        /// <summary>
        /// Property to return the size of an element.
        /// </summary>
        public abstract int ElementSize
        {
            get;
        }
        #endregion

        #region Methods.
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferUavBase{T}"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="log">The log used for debug information.</param>
        protected GorgonBufferUavBase(T buffer, int elementStart, int elementCount, IGorgonLog log)
            : base(buffer)
        {
            Log = log ?? GorgonLogDummy.DefaultInstance;
            ElementStart = elementStart;
            ElementCount = elementCount;
            Buffer = buffer;
        }
        #endregion
    }
}
