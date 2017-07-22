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
// Created: July 5, 2017 2:53:14 PM
// 
#endregion

using System;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The base class for buffer shader views.
    /// </summary>
    /// <typeparam name="T">The type of buffer that will be linked to the view.</typeparam>
    public abstract class GorgonBufferViewBase<T>
        : GorgonShaderResourceView
        where T : GorgonBufferCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return the logging interface for debug logging.
        /// </summary>
        protected IGorgonLog Log
        {
            get;
        }

        /// <summary>
        /// Property to return the buffer associated with the view.
        /// </summary>
        public T Buffer
        {
            get;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        public int BufferSizeInBytes => Buffer.SizeInBytes;

        /// <summary>
        /// Property to return the starting element.
        /// </summary>
        public int StartElement
        {
            get;
        }

        /// <summary>
        /// Property to return the number of elements.
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
        /// Initializes a new instance of the <see cref="GorgonBufferViewBase{T}"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to bind to the view.</param>
        /// <param name="startingElement">The starting element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the buffer to view.</param>
        /// <param name="totalElementCount">The total number of elements in the buffer.</param>
        /// <param name="log">The logging interface used for debug logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="buffer"/> is a staging resource, or does not have a binding flag for shader access.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="startingElement"/> and the <paramref name="elementCount"/> are larger than the total number of elements.</exception>
        protected GorgonBufferViewBase(T buffer, int startingElement, int elementCount, int totalElementCount, IGorgonLog log)
            : base(buffer)
        {
            Log = log ?? GorgonLogDummy.DefaultInstance;
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

            if ((buffer.D3DBuffer.Description.Usage == D3D11.ResourceUsage.Staging)
                || ((buffer.D3DBuffer.Description.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
            {
                throw new ArgumentException(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_BOUND_TO_GPU, nameof(buffer));
            }

            // Ensure that the elements are within the total size of the buffer.
            if (startingElement + elementCount > totalElementCount)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GORGFX_ERR_BUFFER_VIEW_START_COUNT_OUT_OF_RANGE, startingElement, elementCount, totalElementCount));
            }

            TotalElementCount = totalElementCount;
            StartElement = startingElement;
            ElementCount = elementCount;
        }
        #endregion
    }
}
