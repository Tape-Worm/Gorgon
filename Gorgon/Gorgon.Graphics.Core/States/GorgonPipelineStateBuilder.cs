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
// Created: May 24, 2018 7:24:06 PM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A builder used to create pipeline render state objects.
    /// </summary>
    public class GorgonPipelineStateBuilder
        : IGorgonGraphicsObject
    {
        #region Variables.
        // The working state.
        private readonly GorgonPipelineState _workState = new GorgonPipelineState
                                                          {
                                                              RasterState = GorgonRasterState.Default
                                                          };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics interface used to build the pipeline state.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add a rasterizer state to this pipeline state.
        /// </summary>
        /// <param name="state">The rasterizer state to apply.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder RasterState(GorgonRasterStateBuilder state)
        {
            return RasterState(state?.Build());
        }

        /// <summary>
        /// Function to add a rasterizer state to this pipeline state.
        /// </summary>
        /// <param name="state">The rasterizer state to apply.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder RasterState(GorgonRasterState state)
        {
            _workState.RasterState = state ?? GorgonRasterState.Default;
            return this;
        }

        /// <summary>
        /// Function to set the current pixel shader on the pipeline.
        /// </summary>
        /// <param name="pixelShader">The pixel shader to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder PixelShader(GorgonPixelShader pixelShader)
        {
            _workState.PixelShader = pixelShader;
            return this;
        }

        /// <summary>
        /// Function to set the current vertex shader on the pipeline.
        /// </summary>
        /// <param name="vertexShader">The vertex shader to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder VertexShader(GorgonVertexShader vertexShader)
        {
            _workState.VertexShader = vertexShader;
            return this;
        }

        /// <summary>
        /// Function to set the current geometry shader on the pipeline.
        /// </summary>
        /// <param name="geometryShader">The geometry shader to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder GeometryShader(GorgonGeometryShader geometryShader)
        {
            _workState.GeometryShader = geometryShader;
            return this;
        }

        /// <summary>
        /// Function to set the current domain shader on the pipeline.
        /// </summary>
        /// <param name="domainShader">The domain shader to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder DomainShader(GorgonDomainShader domainShader)
        {
            _workState.DomainShader = domainShader;
            return this;
        }

        /// <summary>
        /// Function to set the current hull shader on the pipeline.
        /// </summary>
        /// <param name="hullShader">The domain shader to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder HullShader(GorgonHullShader hullShader)
        {
            _workState.HullShader = hullShader;
            return this;
        }

        /// <summary>
        /// Function to set the current compute shader on the pipeline.
        /// </summary>
        /// <param name="computeShader">The compute shader to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder ComputeShader(GorgonComputeShader computeShader)
        {
            _workState.ComputeShader = computeShader;
            return this;
        }


        /// <summary>
        /// Function to set primitive topology for the draw call.
        /// </summary>
        /// <param name="primitiveType">The type of primitive to render.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonPipelineStateBuilder PrimitiveType(PrimitiveType primitiveType)
        {
            _workState.PrimitiveType = primitiveType;
            return this;
        }

        /// <summary>
        /// Function to reset the pipeline state to the specified state passed in to the method.
        /// </summary>
        /// <param name="pipeState">The pipeline state to copy.</param>
        /// <returns>The fluent interface for the builder.</returns>
        public GorgonPipelineStateBuilder Reset(GorgonPipelineState pipeState)
        {
            if (pipeState == null)
            {
                Clear();
                return this;
            }

            _workState.RasterState = pipeState.RasterState;
            _workState.PixelShader = pipeState.PixelShader;
            _workState.VertexShader = pipeState.VertexShader;
            _workState.GeometryShader = pipeState.GeometryShader;
            _workState.DomainShader = pipeState.DomainShader;
            _workState.HullShader = pipeState.HullShader;
            _workState.ComputeShader = pipeState.ComputeShader;
            _workState.PrimitiveType = pipeState.PrimitiveType;
            
            return this;
        }

        /// <summary>
        /// Function to clear the current pipeline state.
        /// </summary>
        /// <returns>The fluent interface for the builder.</returns>
        public GorgonPipelineStateBuilder Clear()
        {
            _workState.PrimitiveType = Core.PrimitiveType.TriangleList;
            _workState.RasterState = GorgonRasterState.Default;
            _workState.PixelShader = null;
            _workState.VertexShader = null;
            _workState.GeometryShader = null;
            _workState.DomainShader = null;
            _workState.HullShader = null;
            _workState.ComputeShader = null;
            return this;
        }

        /// <summary>
        /// Function to build a pipeline state.
        /// </summary>
        /// <returns>A new pipeline state.</returns>
        public GorgonPipelineState Build()
        {
            // Build the actual state.
            return Graphics.CachePipelineState(_workState);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPipelineStateBuilder"/> class.
        /// </summary>
        /// <param name="graphics">The graphics object that will build the pipeline state.</param>
        public GorgonPipelineStateBuilder(GorgonGraphics graphics)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        }
        #endregion
    }
}
