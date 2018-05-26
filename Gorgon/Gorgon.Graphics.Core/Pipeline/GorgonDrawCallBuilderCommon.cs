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
// Created: May 23, 2018 12:18:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Common functionality for the a draw call fluent builder.
    /// </summary>
    /// <typeparam name="TB">The type of builder.</typeparam>
    /// <typeparam name="TDc">The type of draw call.</typeparam>
    public abstract class GorgonDrawCallBuilderCommon<TB, TDc>
        where TB : GorgonDrawCallBuilderCommon<TB, TDc>
        where TDc : GorgonDrawCallCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return the draw call being edited.
        /// </summary>
        protected TDc DrawCall
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy vertex buffer bindings from one draw call to another
        /// </summary>
        /// <param name="destBindings">The bindings to update.</param>
        /// <param name="srcBindings">The bindings to copy.</param>
        /// <param name="layout">The input layout.</param>
        private static void CopyVertexBuffers(GorgonVertexBufferBindings destBindings, IReadOnlyList<GorgonVertexBufferBinding> srcBindings, GorgonInputLayout layout)
        {
            if (destBindings == null)
            {
                destBindings = new GorgonVertexBufferBindings();
            }
            else
            {
                destBindings.Clear();
            }

            destBindings.InputLayout = layout;

            if (srcBindings == null)
            {
                return;
            }

            int count = srcBindings.Count.Min(GorgonVertexBufferBindings.MaximumVertexBufferCount);
            
            for (int i = 0; i < count; ++i)
            {
                destBindings[i] = srcBindings[i];
            }
        }

        /// <summary>
        /// Function to create a new draw call.
        /// </summary>
        /// <returns>A new draw call.</returns>
        protected abstract TDc OnCreate();

        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        protected abstract void OnUpdate(TDc finalCopy);

        /// <summary>
        /// Function to reset the properties of the draw call to the draw call passed in.
        /// </summary>
        /// <param name="drawCall">The draw call to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected abstract TB OnReset(TDc drawCall);

        /// <summary>
        /// Function to clear the draw call.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected abstract TB OnClear();

        /// <summary>
        /// Function to set the pipeline state for this draw call.
        /// </summary>
        /// <param name="pipelineState">The pipeline state to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pipelineState"/> parameter is <b>null</b>.</exception>
        public TB PipelineState(GorgonPipelineState pipelineState)
        {
            DrawCall.D3DState.PipelineState = pipelineState ?? throw new ArgumentNullException(nameof(pipelineState));
            return (TB)this;
        }

        /// <summary>
        /// Function to set the pipeline state for this draw call.
        /// </summary>
        /// <param name="pipelineState">The pipeline state to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        public TB PipelineState(GorgonPipelineStateBuilder pipelineState)
        {
            return PipelineState(pipelineState?.Build());
        }

        /// <summary>
        /// Function to set primitive topology for the draw call.
        /// </summary>
        /// <param name="primitiveType">The type of primitive to render.</param>
        /// <returns>The fluent builder interface.</returns>
        public TB PrimitiveType(PrimitiveType primitiveType)
        {
            DrawCall.D3DState.Topology = (D3D.PrimitiveTopology)primitiveType;
            return (TB)this;
        }

        /// <summary>
        /// Function to set a vertex buffer binding for the draw call.
        /// </summary>
        /// <param name="layout">The input layout to use.</param>
        /// <param name="binding">The vertex buffer binding to set.</param>
        /// <param name="slot">[Optional] The slot for the binding.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="layout"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0, or greater than/equal to <see cref="GorgonVertexBufferBindings.MaximumVertexBufferCount"/>.</exception>
        public TB VertexBuffer(GorgonInputLayout layout, in GorgonVertexBufferBinding binding, int slot = 0)
        {
            if ((slot < 0) || (slot >= GorgonVertexBufferBindings.MaximumVertexBufferCount))
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_INVALID_VERTEXBUFFER_SLOT, GorgonVertexBufferBindings.MaximumVertexBufferCount));
            }

            if (DrawCall.D3DState.VertexBuffers == null)
            {
                DrawCall.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
            }

            DrawCall.D3DState.VertexBuffers[slot] = binding;
            DrawCall.D3DState.VertexBuffers.InputLayout = layout ?? throw new ArgumentNullException(nameof(layout));
            return (TB)this;
        }

        /// <summary>
        /// Function to set the vertex buffer bindings for the draw call.
        /// </summary>
        /// <param name="layout">The input layout to use.</param>
        /// <param name="bindings">The vertex buffer bindings to set.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="layout"/> parameter is <b>null</b>.</exception>
        public TB VertexBufferBindings(GorgonInputLayout layout, IReadOnlyList<GorgonVertexBufferBinding> bindings)
        {
            CopyVertexBuffers(DrawCall.D3DState.VertexBuffers, bindings, layout ?? throw new ArgumentNullException(nameof(layout)));
            return (TB)this;
        }

        /// <summary>
        /// Function to return the draw call.
        /// </summary>
        /// <returns>The draw call created or updated by this builder.</returns>
        public TDc Build()
        {
            TDc final = OnCreate();

            if (final.D3DState.VertexBuffers == null)
            {
                final.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
            }
            
            CopyVertexBuffers(final.D3DState.VertexBuffers, DrawCall.VertexBufferBindings, DrawCall.InputLayout);
            final.D3DState.Topology = (D3D.PrimitiveTopology)DrawCall.PrimitiveType;
            final.D3DState.PipelineState = DrawCall.PipelineState;

            OnUpdate(final);

            return final;
        }

        /// <summary>
        /// Function to reset the builder to the specified draw call state.
        /// </summary>
        /// <param name="drawCall">[Optional] The specified draw call state to copy.</param>
        /// <returns>The fluent builder interface.</returns>
        public TB Reset(TDc drawCall = null)
        {
            if (drawCall == null)
            {
                return Clear();
            }

            DrawCall.D3DState.Topology = (D3D.PrimitiveTopology)drawCall.PrimitiveType;
            
            VertexBufferBindings(drawCall.InputLayout, drawCall.VertexBufferBindings);
            DrawCall.D3DState.PipelineState = new GorgonPipelineState(DrawCall.PipelineState);

            return OnReset(drawCall);
        }

        /// <summary>
        /// Function to clear the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public TB Clear()
        {
            DrawCall.D3DState.VertexBuffers.Clear();
            DrawCall.D3DState.Topology = D3D.PrimitiveTopology.TriangleList;
            DrawCall.D3DState.PipelineState.RasterState = null;
            DrawCall.D3DState.PipelineState.PixelShader.Current = null;
            DrawCall.D3DState.PipelineState.PixelShader.RwSamplers.Clear();

            return OnClear();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCallBuilder"/> class.
        /// </summary>
        /// <param name="drawCall">The worker draw call.</param>
        private protected GorgonDrawCallBuilderCommon(TDc drawCall)
        {
            DrawCall = drawCall;
            DrawCall.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
            DrawCall.D3DState.PipelineState = new GorgonPipelineState();
            DrawCall.D3DState.Topology = D3D.PrimitiveTopology.TriangleList;
        }
        #endregion
    }
}
