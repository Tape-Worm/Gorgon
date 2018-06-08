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
        private readonly GorgonDrawCallPoolAllocator<GorgonDrawIndexCall> _drawAllocator;
        // The builder used to build a draw call.
        private readonly GorgonDrawIndexCallBuilder _drawBuilder;
        // The builder used to build states.
        private readonly GorgonPipelineStateBuilder _stateBuilder;
        // The last draw call that was issued.
        private GorgonDrawIndexCall _lastCall;
        // The last pipeline state used.
        private GorgonPipelineState _lastState;
        // The default texture to use if the renderable does not have one.
        private readonly GorgonTexture2DView _defaultTexture;
        // The current input layout.
        private readonly GorgonInputLayout _inputLayout;
        // The previous sprite.
        private GorgonSprite _prevSprite;
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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve a draw indexed call from the pool and, optionally, initialize it.
        /// </summary>
        /// <param name="renderable">The renderable to evaluate.</param>
        /// <param name="batchState">The current global state for the batch.</param>
        /// <param name="renderer">The renderer that will be </param>
        /// <returns>The draw call.</returns>
        public GorgonDrawIndexCall GetDrawIndexCall(GorgonSprite renderable, Gorgon2DBatchState batchState, ObjectRenderer renderer)
        {
            bool needStateUpdate = (_lastState == null) 
                                   || (_lastCall?.PipelineState != _lastState) 
                                   || (_lastState.PixelShader != batchState.PixelShader)
                                   || (_lastState.VertexShader != batchState.VertexShader)
                                   || (_lastState.BlendStates[0] != batchState.BlendState)
                                   || (_lastState.DepthStencilState != batchState.DepthStencilState)
                                   || (_lastState.RasterState != batchState.RasterState);

            bool needResourceUpdate = (_lastCall == null)
                                      || (!_prevSprite.Equals(renderable))
                                      || (_lastCall.IndexBuffer != renderer.IndexBuffer)
                                      || (_lastCall.VertexBufferBindings[0] != renderer.VertexBuffer)
                                      || (_lastCall.VertexShader.ConstantBuffers[0] != ProjectionViewBuffer);

            _prevSprite = renderable;

            if ((!needResourceUpdate) && (!needStateUpdate))
            {
                return _lastCall;
            }

            // Update the state only if we need to.
            if (needStateUpdate)
            {
                _lastState = _stateBuilder.PixelShader(batchState.PixelShader)
                                          .VertexShader(batchState.VertexShader)
                                          .BlendState(batchState.BlendState)
                                          .Build();

                if (!needResourceUpdate)
                {
                    _lastCall = _drawBuilder.PipelineState(_lastState)
                                            .Build(_drawAllocator);

                    return _lastCall;
                }
            }
            
            _lastCall = _drawBuilder.PipelineState(_lastState)
                                    .ConstantBuffer(ShaderType.Vertex, ProjectionViewBuffer)
                                    .IndexBuffer(renderer.IndexBuffer)
                                    .VertexBuffer(_inputLayout, renderer.VertexBuffer)
                                    .ShaderResource(ShaderType.Pixel, renderable.Texture ?? _defaultTexture)
                                    .Build(_drawAllocator);
            return _lastCall;
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
            _drawAllocator = new GorgonDrawCallPoolAllocator<GorgonDrawIndexCall>(3);
            _drawBuilder = new GorgonDrawIndexCallBuilder();
            _stateBuilder = new GorgonPipelineStateBuilder(graphics);
            _defaultTexture = defaultTexture;
        }
        #endregion
    }
}
