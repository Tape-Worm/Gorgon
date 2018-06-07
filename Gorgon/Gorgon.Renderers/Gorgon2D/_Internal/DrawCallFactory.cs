using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A factory used to produce draw calls.
    /// </summary>
    class DrawCallFactory
    {
        #region Variables.
        // The allocater used to create draw calls.
        private readonly GorgonDrawCallPoolAllocator<GorgonDrawIndexCall> _drawAllocator;
        // The builder used to build a draw call.
        private readonly GorgonDrawIndexCallBuilder _drawBuilder;
        // The builder used to build states.
        private readonly GorgonPipelineStateBuilder _stateBuilder;
        // The last renderable passed to this method.
        private Gorgon2DRenderable _lastRenderable;
        // The last draw call that was issued.
        private GorgonDrawIndexCall _lastCall;
        // The last pipeline state used.
        private GorgonPipelineState _lastState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the default texture to render.
        /// </summary>
        public GorgonTexture2DView DefaultTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the default pixel shader.
        /// </summary>
        public GorgonPixelShader DefaultPixelShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the default vertex shader.
        /// </summary>
        public GorgonVertexShader DefaultVertexShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the projection view matrix buffer.
        /// </summary>
        public GorgonConstantBufferView ProjectionViewBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current input layout.
        /// </summary>
        public GorgonInputLayout InputLayout
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the active vertex buffer.
        /// </summary>
        public GorgonVertexBufferBinding VertexBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the active index buffer.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        private bool GlobalStateEqual()
        {
            if (_lastCall == null)
            {
                return false;
            }

            return (_lastCall.PipelineState.PixelShader == DefaultPixelShader)
                   && (_lastCall.PipelineState.VertexShader == DefaultVertexShader)
                   && (_lastCall.VertexShader.ConstantBuffers[0] == ProjectionViewBuffer)
                   && (_lastCall.IndexBuffer == IndexBuffer)
                   && (_lastCall.VertexBufferBindings[0] == VertexBuffer);
        }

        /// <summary>
        /// Function to retrieve a draw indexed call from the pool and, optionally, initialize it.
        /// </summary>
        /// <param name="renderable">The renderable to evaluate.</param>
        /// <returns>The draw call.</returns>
        public GorgonDrawIndexCall GetDrawIndexCall(Gorgon2DRenderable renderable)
        {
            if ((_lastRenderable != null)
                && (_lastCall != null)
                && (_lastRenderable.Equals(renderable))
                && (GlobalStateEqual()))
            {
                return _lastCall;
            }

            _lastRenderable = renderable;

            if ((_lastState == null) || (_lastState != _lastCall?.PipelineState))
            {
                _lastState = _stateBuilder.PixelShader(DefaultPixelShader)
                                          .VertexShader(DefaultVertexShader)
                                          .BlendState(renderable.BlendState)
                                          .Build();
            }

            _lastCall = _drawBuilder.PipelineState(_lastState)
                                    .ConstantBuffer(ShaderType.Vertex, ProjectionViewBuffer)
                                    .IndexBuffer(IndexBuffer)
                                    .VertexBuffer(InputLayout, VertexBuffer)
                                    .ShaderResource(ShaderType.Pixel, renderable.Texture ?? DefaultTexture)
                                    .Build(_drawAllocator);
            return _lastCall;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCallFactory"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating states.</param>
        public DrawCallFactory(GorgonGraphics graphics)
        {
            _drawAllocator = new GorgonDrawCallPoolAllocator<GorgonDrawIndexCall>(10922);
            _drawBuilder = new GorgonDrawIndexCallBuilder();
            _stateBuilder = new GorgonPipelineStateBuilder(graphics);
        }
        #endregion
    }
}
