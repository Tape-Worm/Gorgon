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
        private BatchRenderable _prevRenderable;
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
        /// Function to retrieve a draw indexed call from the pool and, optionally, initialize it.
        /// </summary>
        /// <param name="renderable">The renderable to evaluate.</param>
        /// <param name="batchState">The current global state for the batch.</param>
        /// <param name="renderer">The renderer that will be </param>
        /// <returns>The draw call.</returns>
        public GorgonDrawIndexCall GetDrawIndexCall(BatchRenderable renderable, Gorgon2DBatchState batchState, ObjectRenderer renderer)
        {
            bool needStateUpdate = (_lastState == null)
                                   || (_lastCall?.PipelineState != _lastState)
                                   || (_lastState.PixelShader != batchState.PixelShader.Shader)
                                   || (_lastState.VertexShader != batchState.VertexShader.Shader)
                                   || (_lastState.BlendStates[0] != batchState.BlendState)
                                   || (_lastState.DepthStencilState != batchState.DepthStencilState)
                                   || (_lastState.RasterState != batchState.RasterState);

            bool needResourceUpdate = (_lastCall == null)
                                      || (_prevRenderable != renderable)
                                      || (renderable.StateChanged)
                                      || (_lastCall.IndexBuffer != renderer.IndexBuffer)
                                      || (_lastCall.VertexBufferBindings[0] != renderer.VertexBuffer)
                                      || (_lastCall.VertexShader.ConstantBuffers[0] != ProjectionViewBuffer)
                                      || (_lastCall.PixelShader.ConstantBuffers[0] != AlphaTestBuffer);

            bool needsVsConstantsUpdate = _lastCall == null;
            bool needsPsConstantsUpdate = _lastCall == null;
            bool needsVsSamplerUpdate = _lastCall == null;
            bool needsPsSamplerUpdate = _lastCall == null;
            bool needsVsSrvUpdate = _lastCall == null;
            bool needsPsSrvUpdate = _lastCall == null;

            // Check for resource switches.
            if (_lastCall != null)
            {
                if (_lastCall.VertexShader.ConstantBuffers.DirtyEquals(batchState.VertexShader.ConstantBuffers))
                {
                    needResourceUpdate = needsVsConstantsUpdate = true;
                }

                if (_lastCall.PixelShader.ConstantBuffers.DirtyEquals(batchState.PixelShader.ConstantBuffers))
                {
                    needResourceUpdate = needsPsConstantsUpdate = true;
                }

                if (_lastCall.VertexShader.ShaderResources.DirtyEquals(batchState.VertexShader.ShaderResources))
                {
                    needResourceUpdate = needsVsSrvUpdate = true;
                }

                if (_lastCall.PixelShader.ShaderResources.DirtyEquals(batchState.PixelShader.ShaderResources))
                {
                    needResourceUpdate = needsPsSrvUpdate = true;
                }

                if (_lastCall.VertexShader.Samplers.DirtyEquals(batchState.VertexShader.Samplers))
                {
                    needResourceUpdate = needsVsSamplerUpdate = true;
                }

                if (_lastCall.PixelShader.Samplers.DirtyEquals(batchState.PixelShader.Samplers))
                {
                    needResourceUpdate = needsPsSamplerUpdate = true;
                }
            }

            _prevRenderable = renderable;

            if ((!needResourceUpdate) && (!needStateUpdate))
            {
                return _lastCall;
            }

            // Update the state only if we need to.
            if (needStateUpdate)
            {
                _lastState = _stateBuilder.PixelShader(batchState.PixelShader.Shader)
                                          .VertexShader(batchState.VertexShader.Shader)
                                          .BlendState(batchState.BlendState)
                                          .Build();

                if (!needResourceUpdate)
                {
                    _lastCall = _drawBuilder.PipelineState(_lastState)
                                            .Build(_drawAllocator);

                    return _lastCall;
                }
            }

            if (needsVsConstantsUpdate)
            {
                _drawBuilder.ConstantBuffers(ShaderType.Vertex, batchState.VertexShader.ConstantBuffers, 1);
            }

            if (needsPsConstantsUpdate)
            {
                _drawBuilder.ConstantBuffers(ShaderType.Pixel, batchState.PixelShader.ConstantBuffers, 1);
            }

            if (needsVsSrvUpdate)
            {
                _drawBuilder.ShaderResources(ShaderType.Vertex, batchState.VertexShader.ShaderResources);
            }

            if (needsPsSrvUpdate)
            {
                _drawBuilder.ShaderResources(ShaderType.Pixel, batchState.PixelShader.ShaderResources, 1);
            }

            if (needsVsSamplerUpdate)
            {
                _drawBuilder.SamplerStates(ShaderType.Vertex, batchState.VertexShader.Samplers);
            }

            if (needsPsSamplerUpdate)
            {
                _drawBuilder.SamplerStates(ShaderType.Pixel, batchState.PixelShader.Samplers, 1);
            }

            _drawBuilder.PipelineState(_lastState)
                        .ConstantBuffer(ShaderType.Vertex, ProjectionViewBuffer)
                        .ConstantBuffer(ShaderType.Pixel, AlphaTestBuffer)
                        .IndexBuffer(renderer.IndexBuffer)
                        .VertexBuffer(_inputLayout, renderer.VertexBuffer)
                        .ShaderResource(ShaderType.Pixel, renderable.Texture ?? _defaultTexture)
                        .SamplerState(ShaderType.Pixel, renderable.TextureSampler ?? GorgonSamplerState.Default);


            _lastCall = _drawBuilder.Build(_drawAllocator);
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
