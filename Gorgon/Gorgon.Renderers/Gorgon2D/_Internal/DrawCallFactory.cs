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
// Created: June 7, 2018 4:18:45 PM
// 
#endregion

using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A factory used to produce draw calls.
    /// </summary>
    internal sealed class DrawCallFactory
    {
        #region Variables.
        // The allocater used to create draw calls.
        private readonly GorgonDrawCallPoolAllocator<GorgonDrawIndexCall> _drawIndexAllocator;
        // The allocater used to create draw calls.
        private readonly GorgonDrawCallPoolAllocator<GorgonDrawCall> _drawAllocator;
        // The builder used to build a draw call.
        private readonly GorgonDrawIndexCallBuilder _drawIndexBuilder;
        // The builder used to build a draw call.
        private readonly GorgonDrawCallBuilder _drawBuilder;
        // The builder used to build states.
        private readonly GorgonPipelineStateBuilder _stateBuilder;
        // The default texture to use if the renderable does not have one.
        private readonly GorgonTexture2DView _defaultTexture;
        // The current input layout.
        private readonly GorgonInputLayout _inputLayout;
        #endregion

        #region Properties.

        /// <summary>
        /// Property to set or return the projection view matrix buffer.
        /// </summary>
        public GorgonConstantBufferView ProjectionViewBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the buffer used to hold alpha test data.
        /// </summary>
        public GorgonConstantBufferView AlphaTestBuffer
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve a draw call from the pool and, initialize it.
        /// </summary>
        /// <param name="renderable">The renderable to evaluate.</param>
        /// <param name="batchState">The current global state for the batch.</param>
        /// <param name="renderer">The renderer that will be </param>
        /// <returns>The draw call.</returns>
        public GorgonDrawCall GetDrawCall(BatchRenderable renderable, Gorgon2DBatchState batchState, ObjectRenderer renderer)
        {
            // If we haven't specified values for our first texture and sampler, then do so now.
            batchState.PixelShader.RwSrvs[0] = renderable.Texture ?? _defaultTexture;
            batchState.PixelShader.RwSamplers[0] = renderable.TextureSampler ?? GorgonSamplerState.Default;

            return _drawBuilder.ConstantBuffers(ShaderType.Vertex, batchState.VertexShader.RwConstantBuffers)
                               .ConstantBuffers(ShaderType.Pixel, batchState.PixelShader.RwConstantBuffers)
                               .ShaderResources(ShaderType.Vertex, batchState.VertexShader.RwSrvs)
                               .ShaderResources(ShaderType.Pixel, batchState.PixelShader.RwSrvs)
                               .SamplerStates(ShaderType.Vertex, batchState.VertexShader.RwSamplers)
                               .SamplerStates(ShaderType.Pixel, batchState.PixelShader.RwSamplers)
                               .PipelineState(_stateBuilder.PixelShader(batchState.PixelShader.Shader)
                                                           .VertexShader(batchState.VertexShader.Shader)
                                                           .DepthStencilState(batchState.DepthStencilState)
                                                           .RasterState(batchState.RasterState)
                                                           .BlendState(batchState.BlendState))
                               .VertexBuffer(_inputLayout, renderer.VertexBuffer)
                               .Build(_drawAllocator);
        }

        /// <summary>
        /// Function to retrieve a draw indexed call from the pool and,  initialize it.
        /// </summary>
        /// <param name="renderable">The renderable to evaluate.</param>
        /// <param name="batchState">The current global state for the batch.</param>
        /// <param name="renderer">The renderer that will be </param>
        /// <returns>The draw call.</returns>
        public GorgonDrawIndexCall GetDrawIndexCall(BatchRenderable renderable, Gorgon2DBatchState batchState, ObjectRenderer renderer)
        {
            // If we haven't specified values for our first texture and sampler, then do so now.
            batchState.PixelShader.RwSrvs[0] = renderable.Texture ?? _defaultTexture;
            batchState.PixelShader.RwSamplers[0] = renderable.TextureSampler ?? GorgonSamplerState.Default;

            return _drawIndexBuilder.ConstantBuffers(ShaderType.Vertex, batchState.VertexShader.RwConstantBuffers)
                                    .ConstantBuffers(ShaderType.Pixel, batchState.PixelShader.RwConstantBuffers)
                                    .ShaderResources(ShaderType.Vertex, batchState.VertexShader.RwSrvs)
                                    .ShaderResources(ShaderType.Pixel, batchState.PixelShader.RwSrvs)
                                    .SamplerStates(ShaderType.Vertex, batchState.VertexShader.RwSamplers)
                                    .SamplerStates(ShaderType.Pixel, batchState.PixelShader.RwSamplers)
                                    .PipelineState(_stateBuilder.PixelShader(batchState.PixelShader.Shader)
                                                                .VertexShader(batchState.VertexShader.Shader)
                                                                .DepthStencilState(batchState.DepthStencilState)
                                                                .RasterState(batchState.RasterState)
                                                                .BlendState(batchState.BlendState))
                                    .IndexBuffer(renderer.IndexBuffer)
                                    .VertexBuffer(_inputLayout, renderer.VertexBuffer)
                                    .Build(_drawIndexAllocator);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCallFactory"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating states.</param>
        /// <param name="defaultTexture">The default texture to use as a fallback when no texture is passed by renderable.</param>
        /// <param name="inputLayout">The input layout defining a vertex type.</param>
        public DrawCallFactory(GorgonGraphics graphics, GorgonTexture2DView defaultTexture, GorgonInputLayout inputLayout)
        {
            _inputLayout = inputLayout;
            _drawIndexAllocator = new GorgonDrawCallPoolAllocator<GorgonDrawIndexCall>(16);
            _drawAllocator = new GorgonDrawCallPoolAllocator<GorgonDrawCall>(16);
            _drawIndexBuilder = new GorgonDrawIndexCallBuilder();
            _drawBuilder = new GorgonDrawCallBuilder();
            _stateBuilder = new GorgonPipelineStateBuilder(graphics);
            _defaultTexture = defaultTexture;
        }
        #endregion
    }
}
