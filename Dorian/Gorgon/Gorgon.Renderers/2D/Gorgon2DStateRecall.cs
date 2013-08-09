#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Friday, August 09, 2013 8:13:16 PM
// 
#endregion

using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
    /// <summary>
    /// Holds a record of the current state of the graphics interface.
    /// </summary>
    public class Gorgon2DStateRecall
    {
        private GorgonGraphics _graphics;                                       // Graphics interface that was used to record.
        private GorgonPixelShader _pixelShader;									// Pixel shader.
        private GorgonVertexShader _vertexShader;								// Vertex shader.
        private GorgonBlendStates _blendStates;									// Blending states.
        private GorgonColor _blendFactor;										// Blending factor.
        private uint _blendSampleMask;											// Blending sample mask.
        private GorgonRasterizerStates _rasterStates;							// Rasterizer states.
        private GorgonTextureSamplerStates _samplerState;						// Sampler states.
        private GorgonShaderView _resource;									    // First pixel shader resource.
        private IDictionary<int, GorgonConstantBuffer> _vsConstantBuffers;		// The vertex shader constant buffers.
        private IDictionary<int, GorgonConstantBuffer> _psConstantBuffers;		// The pixel shader constant buffers.
        private GorgonRenderTargetView[] _targets;								// The default targets.
        private GorgonDepthStencilView _depthStencil;                           // Default depth/stencil view.
        private GorgonIndexBuffer _indexBuffer;									// Index buffer.
        private GorgonVertexBufferBinding _vertexBuffer;						// Vertex buffer.
        private GorgonInputLayout _inputLayout;									// Input layout.
        private PrimitiveType _primitiveType;									// Primitive type.
        private GorgonDepthStencilStates _depthStencilState;					// Depth stencil states
        private int _depthStencilReference;										// Depth stencil reference.
        private Rectangle[] _scissorTests;                                      // Scissor tests
        private GorgonViewport[] _viewports;                                    // Viewports.

        /// <summary>
        /// Property to return the renderer that created this object.
        /// </summary>
        internal Gorgon2D Renderer
        {
            get;
            private set;
        }

        /// <summary>
        /// Function to save the state information to this object.
        /// </summary>
        private void Save()
        {
            _targets = _graphics.Output.GetRenderTargets();
            _indexBuffer = _graphics.Input.IndexBuffer;
            _vertexBuffer = _graphics.Input.VertexBuffers[0];
            _inputLayout = _graphics.Input.Layout;
            _primitiveType = _graphics.Input.PrimitiveType;
            _pixelShader = _graphics.Shaders.PixelShader.Current;
            _vertexShader = _graphics.Shaders.VertexShader.Current;
            _blendStates = _graphics.Output.BlendingState.States;
            _blendFactor = _graphics.Output.BlendingState.BlendFactor;
            _blendSampleMask = _graphics.Output.BlendingState.BlendSampleMask;
            _rasterStates = _graphics.Rasterizer.States;
            _samplerState = _graphics.Shaders.PixelShader.TextureSamplers[0];
            _resource = _graphics.Shaders.PixelShader.Resources[0];
            _depthStencilState = _graphics.Output.DepthStencilState.States;
            _depthStencilReference = _graphics.Output.DepthStencilState.DepthStencilReference;
            _rasterStates.IsScissorTestingEnabled = false;
            _depthStencil = _graphics.Output.DepthStencilView;
            _viewports = _graphics.Rasterizer.GetViewports();
            _scissorTests = _graphics.Rasterizer.GetScissorRectangles();

            _vsConstantBuffers = new Dictionary<int, GorgonConstantBuffer>();
            _psConstantBuffers = new Dictionary<int, GorgonConstantBuffer>();

            // Only store the constant buffers that we were using.
            // We need to store all the constant buffers because the effects 
            // make use of multiple constant slots.  Unlike the resource views,
            // where we know that we're only using the first item (all bets are 
            // off if a user decides to use another resource view slot), there's no
            // guarantee that we'll be only using 1 or 2 constant buffer slots.
            for (int i = 0; i < _graphics.Shaders.VertexShader.ConstantBuffers.Count; i++)
            {
                if (_graphics.Shaders.VertexShader.ConstantBuffers[i] != null)
                {
                    _vsConstantBuffers[i] = _graphics.Shaders.VertexShader.ConstantBuffers[i];
                }
            }

            for (int i = 0; i < _graphics.Shaders.PixelShader.ConstantBuffers.Count; i++)
            {
                if (_graphics.Shaders.PixelShader.ConstantBuffers[i] != null)
                {
                    _psConstantBuffers[i] = _graphics.Shaders.PixelShader.ConstantBuffers[i];
                }
            }
        }

        /// <summary>
        /// Function to restore the previous states.
        /// </summary>
        internal void Restore()
        {
            _graphics.Output.SetRenderTargets(_targets, _depthStencil);
            _graphics.Rasterizer.SetViewports(_viewports);
            _graphics.Rasterizer.SetScissorRectangles(_scissorTests);
            _graphics.Input.IndexBuffer = _indexBuffer;
            _graphics.Input.VertexBuffers[0] = _vertexBuffer;
            _graphics.Input.Layout = _inputLayout;
            _graphics.Input.PrimitiveType = _primitiveType;
            _graphics.Shaders.PixelShader.Current = _pixelShader;
            _graphics.Shaders.VertexShader.Current = _vertexShader;
            _graphics.Output.BlendingState.BlendSampleMask = _blendSampleMask;
            _graphics.Output.BlendingState.BlendFactor = _blendFactor;
            _graphics.Output.BlendingState.States = _blendStates;
            _graphics.Output.DepthStencilState.States = _depthStencilState;
            _graphics.Output.DepthStencilState.DepthStencilReference = _depthStencilReference;
            _graphics.Rasterizer.States = _rasterStates;
            _graphics.Shaders.PixelShader.Resources[0] = _resource;
            _graphics.Shaders.PixelShader.TextureSamplers[0] = _samplerState;

            // Restore any constant buffers.				
            foreach (var vsConstant in _vsConstantBuffers)
            {
                _graphics.Shaders.VertexShader.ConstantBuffers[vsConstant.Key] = vsConstant.Value;
            }

            foreach (var psConstant in _psConstantBuffers)
            {
                _graphics.Shaders.PixelShader.ConstantBuffers[psConstant.Key] = psConstant.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DStateRecall"/> class.
        /// </summary>
        /// <param name="renderer">The renderer that is creating this object.</param>
        internal Gorgon2DStateRecall(Gorgon2D renderer)
        {
            Renderer = renderer;
            _graphics = renderer.Graphics;

            Save();
        }
    }
}
