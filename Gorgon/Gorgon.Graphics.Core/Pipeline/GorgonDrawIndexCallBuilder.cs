﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 23, 2018 1:44:30 PM
// 
#endregion

using System;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A builder class used to create indexed draw calls using fluent calls.
    /// </summary>
    /// <seealso cref="GorgonDrawIndexCall"/>
    public class GorgonDrawIndexCallBuilder
        : GorgonDrawCallBuilderCommon<GorgonDrawIndexCallBuilder, GorgonDrawIndexCall>
    {
        #region Methods.
        /// <summary>
        /// Function to create a new draw call.
        /// </summary>
        /// <param name="allocator">The allocator to use when creating draw call objects.</param>
        /// <returns>A new draw call.</returns>
        protected override GorgonDrawIndexCall OnCreate(GorgonRingPool<GorgonDrawIndexCall> allocator)
        {
            return allocator == null ? new GorgonDrawIndexCall() : allocator.Allocate();
        }

        /// <summary>
        /// Function to reset the properties of the draw call to the draw call passed in.
        /// </summary>
        /// <param name="drawCall">The draw call to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonDrawIndexCallBuilder OnResetTo(GorgonDrawIndexCall drawCall)
        {
            DrawCall.IndexBuffer = drawCall.IndexBuffer;
            DrawCall.BaseVertexIndex = drawCall.BaseVertexIndex;
            DrawCall.IndexStart = drawCall.IndexStart;
            DrawCall.IndexCount = drawCall.IndexCount;
            DrawCall.IndexBuffer = drawCall.IndexBuffer;
            return this;
        }

        /// <summary>
        /// Function to clear the draw call.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonDrawIndexCallBuilder OnClear()
        {
            DrawCall.IndexBuffer = null;
            DrawCall.BaseVertexIndex = 0;
            DrawCall.IndexStart = 0;
            DrawCall.IndexCount = 0;
            return this;
        }

        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        /// <returns></returns>
        protected override void OnUpdate(GorgonDrawIndexCall finalCopy)
        {
            finalCopy.BaseVertexIndex = DrawCall.BaseVertexIndex;
            finalCopy.IndexStart = DrawCall.IndexStart;
            finalCopy.IndexCount = DrawCall.IndexCount;
            finalCopy.IndexBuffer = DrawCall.IndexBuffer;
        }

        /// <summary>
        /// Function to assign an index buffer to the draw call.
        /// </summary>
        /// <param name="buffer">The buffer to assign.</param>
        /// <param name="indexStart">The first index in the index buffer to render.</param>
        /// <param name="indexCount">The number of indices to render.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="indexStart"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="indexCount"/> parameter is less than 1.</para>
        /// </exception>
        public GorgonDrawIndexCallBuilder IndexBuffer(GorgonIndexBuffer buffer, int indexStart, int indexCount)
        {
            if (indexStart < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexStart), Resources.GORGFX_ERR_INDEX_TOO_SMALL);
            }

            if (indexCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCount), Resources.GORGFX_ERR_INDEX_COUNT_TOO_SMALL);
            }

            DrawCall.IndexStart = indexStart;
            DrawCall.IndexCount = indexCount;
            DrawCall.IndexBuffer = buffer;
            return this;
        }

        /// <summary>
        /// Function to set the first index, and the number of indices to render in the draw call.
        /// </summary>
        /// <param name="indexStart">The first index in the index buffer to render.</param>
        /// <param name="indexCount">The number of indices to render.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="indexStart"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="indexCount"/> parameter is less than 1.</para>
        /// </exception>
        public GorgonDrawIndexCallBuilder IndexRange(int indexStart, int indexCount)
        {
            if (indexStart < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexStart), Resources.GORGFX_ERR_INDEX_TOO_SMALL);
            }

            if (indexCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCount), Resources.GORGFX_ERR_INDEX_COUNT_TOO_SMALL);
            }

            DrawCall.IndexStart = indexStart;
            DrawCall.IndexCount = indexCount;
            return this;
        }

        /// <summary>
        /// Function to set the base vertex index.
        /// </summary>
        /// <param name="baseVertexIndex">The base vertex index to set.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDrawIndexCallBuilder BaseVertexIndex(int baseVertexIndex)
        {
            if (baseVertexIndex < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(baseVertexIndex), Resources.GORGFX_ERR_VERTEX_INDEX_TOO_SMALL);
            }

            DrawCall.BaseVertexIndex = baseVertexIndex;

            return this;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawIndexCallBuilder"/> class.
        /// </summary>
        public GorgonDrawIndexCallBuilder()
            : base(new GorgonDrawIndexCall())
        {

        }
        #endregion
    }
}
