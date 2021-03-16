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
        /// Function to set up common states for a draw call type.
        /// </summary>
        /// <typeparam name="TB">The type of draw call builder.</typeparam>
        /// <typeparam name="TD">The type of draw call to return.</typeparam>
        /// <param name="builder">The builder to pass in.</param>
        /// <param name="renderable">The renderable being rendered.</param>
        /// <param name="batchState">The current batch level state.</param>
        /// <param name="renderer">The renderer used to send renderable data to the GPU.</param>
        private void SetCommonStates<TB, TD>(GorgonDrawCallBuilderCommon<TB, TD> builder, BatchRenderable renderable, Gorgon2DBatchState batchState, ObjectRenderer renderer)
            where TB : GorgonDrawCallBuilderCommon<TB, TD>
            where TD : GorgonDrawCallCommon
        {
            // If we haven't specified values for our first texture and sampler, then do so now.
            batchState.PixelShaderState.RwSrvs[0] = renderable.Texture ?? _defaultTexture;
            batchState.PixelShaderState.RwSamplers[0] = renderable.TextureSampler ?? GorgonSamplerState.Default;

            builder.ConstantBuffers(ShaderType.Vertex, batchState.VertexShaderState.RwConstantBuffers)
                   .ConstantBuffers(ShaderType.Pixel, batchState.PixelShaderState.RwConstantBuffers)
                   .ShaderResources(ShaderType.Vertex, batchState.VertexShaderState.RwSrvs)
                   .ShaderResources(ShaderType.Pixel, batchState.PixelShaderState.RwSrvs)
                   .SamplerStates(ShaderType.Vertex, batchState.VertexShaderState.RwSamplers)
                   .SamplerStates(ShaderType.Pixel, batchState.PixelShaderState.RwSamplers)
                   .PipelineState(_stateBuilder.PixelShader(batchState.PixelShaderState.Shader)
                                               .VertexShader(batchState.VertexShaderState.Shader)
                                               .DepthStencilState(batchState.DepthStencilState)
                                               .RasterState(batchState.RasterState)
                                               .BlendState(batchState.BlendState)
                                               .PrimitiveType(renderable.PrimitiveType));

            // If we don't supply a renderer type, then don't assign a vertex buffer.
            if (renderer is null)
            {
                return;
            }

            builder.VertexBuffer(_inputLayout, renderer.VertexBuffer);
        }

        /// <summary>
        /// Function to retrieve a draw call from the pool and, initialize it.
        /// </summary>
        /// <param name="renderable">The renderable to evaluate.</param>
        /// <param name="batchState">The current global state for the batch.</param>
        /// <param name="renderer">The renderer that will be </param>
        /// <returns>The draw call.</returns>
        public GorgonDrawCall GetDrawCall(BatchRenderable renderable, Gorgon2DBatchState batchState, ObjectRenderer renderer)
        {
            SetCommonStates(_drawBuilder, renderable, batchState, renderer);
            return _drawBuilder.Build(_drawAllocator);
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
            SetCommonStates(_drawIndexBuilder, renderable, batchState, renderer);
            return _drawIndexBuilder.IndexBuffer(renderer.IndexBuffer)
                                    .Build(_drawIndexAllocator);
        }

        /// <summary>
        /// Function to retrieve a draw indexed call from the pool and,  initialize it.
        /// </summary>
        /// <param name="renderable">The renderable to evaluate.</param>
        /// <param name="batchState">The current global state for the batch.</param>
        /// <param name="indexBuffer">The index buffer to use when creating the draw call.</param>
        /// <param name="vertexBuffer">The vertex buffer binding to use when creating the draw call.</param>
        /// <param name="layout">The vertex input layout.</param>
        /// <returns>The draw call.</returns>
        public GorgonDrawIndexCall GetDrawIndexCall(BatchRenderable renderable, Gorgon2DBatchState batchState, GorgonIndexBuffer indexBuffer, GorgonVertexBufferBinding vertexBuffer, GorgonInputLayout layout)
        {
            SetCommonStates(_drawIndexBuilder, renderable, batchState, null);
            return _drawIndexBuilder.VertexBuffer(layout, vertexBuffer)
                                    .IndexBuffer(indexBuffer)
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
            _drawIndexAllocator = new GorgonDrawCallPoolAllocator<GorgonDrawIndexCall>(128);
            _drawAllocator = new GorgonDrawCallPoolAllocator<GorgonDrawCall>(128);
            _drawIndexBuilder = new GorgonDrawIndexCallBuilder();
            _drawBuilder = new GorgonDrawCallBuilder();
            _stateBuilder = new GorgonPipelineStateBuilder(graphics);
            _defaultTexture = defaultTexture;
        }
        #endregion
    }
}
