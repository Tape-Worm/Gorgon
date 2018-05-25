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
using Gorgon.Collections;
using Gorgon.Graphics.Core.Properties;

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
        private readonly GorgonPipelineState _workState = new GorgonPipelineState();
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
            if (state != null)
            {
                state.CopyTo(_workState.RasterState);
            }
            else
            {
                GorgonRasterState.Default.CopyTo(_workState.RasterState);
            }

            return this;
        }

        /// <summary>
        /// Function to assign a list of samplers to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="samplers">The samplers to assign.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder SamplerStates(ShaderType shaderType, IGorgonReadOnlyArray<GorgonSamplerState> samplers)
        {
            switch (shaderType)
            {
                case ShaderType.Pixel:
                    _workState.PixelShader.UpdateSamplers(samplers);
                    break;
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
        /// Function to assign a sampler to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="index">[Optional] The index of the sampler.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonPipelineStateBuilder SamplerState(ShaderType shaderType, GorgonSamplerState sampler, int index = 0)
        {
#if DEBUG
            if ((index < 0) || (index >= GorgonSamplerStates.MaximumSamplerStateCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
            }
#endif

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
            
            pipeState.CopyTo(_workState);
            return this;
        }

        /// <summary>
        /// Function to clear the current pipeline state.
        /// </summary>
        /// <returns>The fluent interface for the builder.</returns>
        public GorgonPipelineStateBuilder Clear()
        {
            GorgonRasterState.Default.CopyTo(_workState.RasterState);
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
