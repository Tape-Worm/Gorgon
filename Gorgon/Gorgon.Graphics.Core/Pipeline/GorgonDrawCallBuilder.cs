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
using Gorgon.Memory;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A builder class used to create basic draw calls using fluent calls.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The draw call builder object allow applications to build the immutable draw call objects needed to send data and state information to the GPU.
    /// </para>
    /// <para>
    /// A draw call is an immutable object that contains all of the state required to render mesh information. For each mesh an application needs to render, an single draw call should be issued via the
    /// <see cref="O:Gorgon.Graphics.Core.GorgonGraphics.Submit"/> methods.  
    /// </para>
    /// <para>
    /// State management is handled internally by Gorgon so that duplicate states are not set and thus, performance is not impacted by redundant states.
    /// </para>
    /// <para>
    /// Because a draw call is immutable, it is not possible to modify a draw call after it's been created. However, a copy of a draw call can be created using the
    /// <see cref="GorgonDrawCallBuilderCommon{TB,TDc}.ResetTo"/> method on the this object. Or, the builder can be modified after the creation of your draw call that needs to be updated and a new call may
    /// be built then.
    /// </para>
    /// <para>
    /// This builder type uses a fluent interface to assemble the draw call, its resources and its <see cref="GorgonPipelineState"/>. 
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonDrawCall"/>
    public class GorgonDrawCallBuilder
        : GorgonDrawCallBuilderCommon<GorgonDrawCallBuilder, GorgonDrawCall>
    {
        #region Methods.
        /// <summary>
        /// Function to create a new draw call.
        /// </summary>
        /// <param name="allocator">The allocator to use when creating draw call objects.</param>
        /// <returns>A new draw call.</returns>
        protected override GorgonDrawCall OnCreate(IGorgonAllocator<GorgonDrawCall> allocator) => allocator == null ? new GorgonDrawCall() : allocator.Allocate();

        /// <summary>
        /// Function to reset the properties of the draw call to the draw call passed in.
        /// </summary>
        /// <param name="drawCall">The draw call to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonDrawCallBuilder OnResetTo(GorgonDrawCall drawCall)
        {
            DrawCall.VertexStartIndex = drawCall.VertexStartIndex;
            DrawCall.VertexCount = drawCall.VertexCount;
            return this;
        }

        /// <summary>
        /// Function to clear the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonDrawCallBuilder OnClear()
        {
            DrawCall.VertexStartIndex = DrawCall.VertexCount = 0;
            return this;
        }

        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        /// <returns></returns>
        protected override void OnUpdate(GorgonDrawCall finalCopy)
        {
            finalCopy.VertexCount = DrawCall.VertexCount;
            finalCopy.VertexStartIndex = DrawCall.VertexStartIndex;
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
        public GorgonDrawCallBuilder VertexRange(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), Resources.GORGFX_ERR_VERTEX_INDEX_TOO_SMALL);
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Resources.GORGFX_ERR_VERTEX_COUNT_TOO_SMALL);
            }

            DrawCall.VertexStartIndex = index;
            DrawCall.VertexCount = count;
            return this;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCallBuilder"/> class.
        /// </summary>
        public GorgonDrawCallBuilder()
            : base(new GorgonDrawCall())
        {

        }
        #endregion
    }
}
