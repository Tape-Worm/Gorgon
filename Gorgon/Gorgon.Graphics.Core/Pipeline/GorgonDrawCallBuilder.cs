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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A builder class used to create or update draw calls using fluent calls.
    /// </summary>
    public class GorgonDrawCallBuilder
        : GorgonDrawCallBuilderCommon<GorgonDrawCallBuilder, GorgonDrawCall>
    {
        #region Methods.
        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        /// <returns></returns>
        protected override void Update(GorgonDrawCall finalCopy)
        {
            finalCopy.VertexCount = WorkingDrawCall.VertexCount;
            finalCopy.VertexStartIndex = WorkingDrawCall.VertexStartIndex;
        }

        /// <summary>
        /// Function to set the first vertex index, and the number of vertices to render in the draw call.
        /// </summary>
        /// <param name="index">The index of the first vertex in the vertex buffer to render.</param>
        /// <param name="count">The number of vertices to render.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="count"/> parameter is less than 1.</para>
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
        public GorgonDrawCallBuilder VertexRange(int index, int count)
        {
#if DEBUG
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), Resources.GORGFX_ERR_VERTEX_INDEX_TOO_SMALL);
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Resources.GORGFX_ERR_VERTEX_COUNT_TOO_SMALL);
            }
#endif
            WorkingDrawCall.VertexStartIndex = index;
            WorkingDrawCall.VertexCount = count;
            return this;
        }

        /// <summary>
        /// Function to reset the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public override GorgonDrawCallBuilder Reset()
        {
            base.Reset();
            WorkingDrawCall.VertexStartIndex = WorkingDrawCall.VertexCount = 0;
            return this;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCallBuilder"/> class.
        /// </summary>
        /// <param name="callToUpdate">[Optional] A previously created draw call to update.</param>
        public GorgonDrawCallBuilder(GorgonDrawCall callToUpdate = null)
            : base(callToUpdate)
        {
        }
        #endregion
    }
}
