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
using System.Collections.Generic;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;

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
                                                              RasterState = GorgonRasterState.Default,
                                                              PixelShader = new ShaderStates<GorgonPixelShader>()
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
        /// Function to copy samplers.
        /// </summary>
        /// <param name="destStates">The destination sampler states.</param>
        /// <param name="srcStates">The sampler states to copy.</param>
        private static void CopySamplers(GorgonSamplerStates destStates, IReadOnlyList<GorgonSamplerState> srcStates)
        {
            if (srcStates == null)
            {
                destStates.Clear();
                return;
            }

            int count = destStates.Length.Min(srcStates.Count);

            for (int i = 0; i < count; ++i)
            {
                destStates[i] = srcStates[i];
            }
        }

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
        /// Function to assign a list of samplers to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="samplers">The samplers to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder SamplerStates(ShaderType shaderType, IReadOnlyList<GorgonSamplerState> samplers)
        {
            switch (shaderType)
            {
                case ShaderType.Pixel:
                    CopySamplers(_workState.PixelShader.RwSamplers, samplers);
                    break;
                case ShaderType.Vertex:
                case ShaderType.Geometry:
                case ShaderType.Domain:
                case ShaderType.Hull:
                case ShaderType.Compute:
                    throw new NotImplementedException();
            }

            return this;
        }

        /// <summary>
        /// Function to assign a sampler to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="index">[Optional] The index of the sampler.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder SamplerState(ShaderType shaderType, GorgonSamplerStateBuilder sampler, int index = 0)
        {
            return SamplerState(shaderType, sampler.Build(), index);
        }

        /// <summary>
        /// Function to set the current pixel shader on the pipeline.
        /// </summary>
        /// <param name="pixelShader">The pixel shader to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder PixelShader(GorgonPixelShader pixelShader)
        {
            _workState.PixelShader.Current = pixelShader;
            return this;
        }

        /// <summary>
        /// Function to assign a sampler to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="index">[Optional] The index of the sampler.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
        public GorgonPipelineStateBuilder SamplerState(ShaderType shaderType, GorgonSamplerState sampler, int index = 0)
        {
            if ((index < 0) || (index >= GorgonSamplerStates.MaximumSamplerStateCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    _workState.PixelShader.UpdateSampler(sampler, index);
                    break;
            }

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
            _workState.PixelShader.Current = pipeState.PixelShader.Current;
            CopySamplers(_workState.PixelShader.RwSamplers, pipeState.PixelShader.Samplers);
            
            return this;
        }

        /// <summary>
        /// Function to clear the current pipeline state.
        /// </summary>
        /// <returns>The fluent interface for the builder.</returns>
        public GorgonPipelineStateBuilder Clear()
        {
            _workState.RasterState = GorgonRasterState.Default;
            _workState.PixelShader.Current = null;
            _workState.PixelShader.RwSamplers.Clear();
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
