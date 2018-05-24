#region MIT
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

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A builder class used to create or update draw calls using fluent calls.
    /// </summary>
    public class GorgonDrawIndexCallBuilder
        : GorgonDrawCallBuilderCommon<GorgonDrawIndexCallBuilder, GorgonDrawIndexCall>
    {
        #region Methods.
        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        /// <returns></returns>
        protected override void Update(GorgonDrawIndexCall finalCopy)
        {
            finalCopy.BaseVertexIndex = WorkingDrawCall.BaseVertexIndex;
            finalCopy.IndexStart = WorkingDrawCall.IndexStart;
            finalCopy.IndexCount = WorkingDrawCall.IndexCount;
            finalCopy.IndexBuffer = WorkingDrawCall.IndexBuffer;
        }

        /// <summary>
        /// Function to assign an index buffer to the draw call.
        /// </summary>
        /// <param name="buffer">The buffer to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDrawIndexCallBuilder IndexBuffer(GorgonIndexBuffer buffer)
        {
            WorkingDrawCall.IndexBuffer = buffer;
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
        /// <remarks>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public GorgonDrawIndexCallBuilder IndexRange(int indexStart, int indexCount)
        {
#if DEBUG
            if (indexStart < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexStart), Resources.GORGFX_ERR_INDEX_TOO_SMALL);
            }

            if (indexCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCount), Resources.GORGFX_ERR_INDEX_COUNT_TOO_SMALL);
            }
#endif
            WorkingDrawCall.IndexStart = indexStart;
            WorkingDrawCall.IndexCount = indexCount;
            return this;
        }

        /// <summary>
        /// Function to set the base vertex index.
        /// </summary>
        /// <param name="baseVertexIndex">The base vertex index to set.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <remarks>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public GorgonDrawIndexCallBuilder BaseVertexIndex(int baseVertexIndex)
        {
#if DEBUG
            if (baseVertexIndex < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(baseVertexIndex), Resources.GORGFX_ERR_VERTEX_INDEX_TOO_SMALL);
            }
#endif
            WorkingDrawCall.BaseVertexIndex = baseVertexIndex;

            return this;
        }

        /// <summary>
        /// Function to reset the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public override GorgonDrawIndexCallBuilder Reset()
        {
            WorkingDrawCall.IndexBuffer = null;
            WorkingDrawCall.BaseVertexIndex = 0;
            WorkingDrawCall.IndexStart = 0;
            WorkingDrawCall.IndexCount = 0;
            return this;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawIndexCallBuilder"/> class.
        /// </summary>
        /// <param name="callToUpdate">[Optional] A previously created draw call to update.</param>
        public GorgonDrawIndexCallBuilder(GorgonDrawIndexCall callToUpdate = null)
            : base(callToUpdate)
        {
        }
        #endregion
    }
}
