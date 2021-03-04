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
    /// A builder class used to create instanced, indexed draw calls using fluent calls.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The draw call builder object allow applications to build the immutable draw call objects needed to send data and state information to the GPU.
    /// </para>
    /// <para>
    /// A draw call is an immutable object that contains all of the state required to render mesh information. For each mesh an application needs to render, an single draw call should be issued via the 
    /// <see cref="GorgonGraphics.Submit(GorgonInstancedIndexCall, in GorgonColor?, int, int)"/> method.  
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
    /// <seealso cref="GorgonInstancedIndexCall"/>
    public class GorgonInstancedIndexCallBuilder
        : GorgonDrawCallBuilderCommon<GorgonInstancedIndexCallBuilder, GorgonInstancedIndexCall>
    {
        #region Methods.
        /// <summary>
        /// Function to create a new draw call.
        /// </summary>
        /// <param name="allocator">The allocator to use when creating draw call objects.</param>
        /// <returns>A new draw call.</returns>
        protected override GorgonInstancedIndexCall OnCreate(IGorgonAllocator<GorgonInstancedIndexCall> allocator) => allocator is null ? new GorgonInstancedIndexCall() : allocator.Allocate();

        /// <summary>
        /// Function to reset the properties of the draw call to the draw call passed in.
        /// </summary>
        /// <param name="drawCall">The draw call to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonInstancedIndexCallBuilder OnResetTo(GorgonInstancedIndexCall drawCall)
        {
            DrawCall.IndexStart = drawCall.IndexStart;
            DrawCall.BaseVertexIndex = drawCall.BaseVertexIndex;
            DrawCall.IndexCountPerInstance = drawCall.IndexCountPerInstance;
            DrawCall.StartInstanceIndex = drawCall.StartInstanceIndex;
            DrawCall.InstanceCount = drawCall.InstanceCount;
            DrawCall.IndexBuffer = drawCall.IndexBuffer;
            return this;
        }

        /// <summary>
        /// Function to clear the draw call.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonInstancedIndexCallBuilder OnClear()
        {
            DrawCall.IndexStart = 0;
            DrawCall.BaseVertexIndex = 0;
            DrawCall.IndexCountPerInstance = 0;
            DrawCall.StartInstanceIndex = 0;
            DrawCall.InstanceCount = 0;
            DrawCall.IndexBuffer = null;
            return this;
        }

        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        /// <returns></returns>
        protected override void OnUpdate(GorgonInstancedIndexCall finalCopy)
        {
            finalCopy.IndexStart = DrawCall.IndexStart;
            finalCopy.BaseVertexIndex = DrawCall.BaseVertexIndex;
            finalCopy.IndexCountPerInstance = DrawCall.IndexCountPerInstance;
            finalCopy.StartInstanceIndex = DrawCall.StartInstanceIndex;
            finalCopy.InstanceCount = DrawCall.InstanceCount;
            finalCopy.IndexBuffer = DrawCall.IndexBuffer;
        }

        /// <summary>
        /// Function to assign an index buffer to the draw call.
        /// </summary>
        /// <param name="buffer">The buffer to assign.</param>
        /// <param name="indexStart">[Optional] The first index in the index buffer to render.</param>
        /// <param name="indexCountPerInstance">[Optional] The number of indices to render, per instance.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="indexStart"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="indexCountPerInstance"/> parameter is less than 1.</para>
        /// </exception>
        public GorgonInstancedIndexCallBuilder IndexBuffer(GorgonIndexBuffer buffer, int indexStart = 0, int? indexCountPerInstance = null)
        {
            if (indexStart < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexStart), Resources.GORGFX_ERR_INDEX_TOO_SMALL);
            }

            if ((indexCountPerInstance != null) && (indexCountPerInstance < 1))
            {
                throw new ArgumentOutOfRangeException(nameof(indexCountPerInstance), Resources.GORGFX_ERR_INDEX_COUNT_TOO_SMALL);
            }

            DrawCall.IndexStart = indexStart;
            DrawCall.IndexCountPerInstance = indexCountPerInstance ?? 0;
            DrawCall.IndexBuffer = buffer;
            return this;
        }

        /// <summary>
        /// Function to set the first index, and the number of indices to render in the draw call.
        /// </summary>
        /// <param name="indexStart">The first index in the index buffer to render.</param>
        /// <param name="indexCountPerInstance">The number of indices to render, per instance.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="indexStart"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="indexCountPerInstance"/> parameter is less than 1.</para>
        /// </exception>
        public GorgonInstancedIndexCallBuilder IndexRange(int indexStart, int indexCountPerInstance)
        {
            if (indexStart < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexStart), Resources.GORGFX_ERR_INDEX_TOO_SMALL);
            }

            if (indexCountPerInstance < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCountPerInstance), Resources.GORGFX_ERR_INDEX_COUNT_TOO_SMALL);
            }

            DrawCall.IndexStart = indexStart;
            DrawCall.IndexCountPerInstance = indexCountPerInstance;
            return this;
        }

        /// <summary>
        /// Function to set the base vertex index.
        /// </summary>
        /// <param name="baseVertexIndex">The base vertex index to set.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="baseVertexIndex"/> parameter is less than 0.</exception>
        public GorgonInstancedIndexCallBuilder BaseVertexIndex(int baseVertexIndex)
        {
            if (baseVertexIndex < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(baseVertexIndex), Resources.GORGFX_ERR_VERTEX_INDEX_TOO_SMALL);
            }

            DrawCall.BaseVertexIndex = baseVertexIndex;

            return this;
        }

        /// <summary>
        /// Function to set the starting instance index, and the number of instances to draw.
        /// </summary>
        /// <param name="startInstanceIndex">The starting index for the the first instance.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startInstanceIndex"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="instanceCount"/> parameter is less than 1.</para>
        /// </exception>
        public GorgonInstancedIndexCallBuilder InstanceRange(int startInstanceIndex, int instanceCount)
        {
            if (startInstanceIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startInstanceIndex), Resources.GORGFX_ERR_INSTANCE_START_INVALID);
            }

            if (instanceCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(instanceCount), Resources.GORGFX_ERR_INSTANCE_COUNT_INVALID);
            }

            DrawCall.StartInstanceIndex = startInstanceIndex;
            DrawCall.InstanceCount = instanceCount;
            return this;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonInstancedIndexCallBuilder"/> class.
        /// </summary>
        public GorgonInstancedIndexCallBuilder()
            : base(new GorgonInstancedIndexCall())
        {

        }
        #endregion
    }
}
