#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 6, 2017 10:47:10 PM
// 
#endregion

using System;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Provides functionality for blitting a texture to the currently active <see cref="GorgonGraphics.RenderTargets">render target</see>.
	/// </summary>
	class TextureBlitter
		: IDisposable
	{
        #region Constants.
        // The name for the blitter pipeline state cache group.
	    private const string BlitterGroupName = "$$__GORGON_BLITTER_STATE_CACHE__$$xXx420XxXN00b";
        #endregion

        #region Variables.
        // The graphics interface that owns this instance.
	    private readonly GorgonGraphics _graphics;
        // The vertices used to blit the texture.
        private readonly BltVertex[] _vertices = new BltVertex[4];
		// The vertex shader for blitting the texture.
		private GorgonVertexShader _vertexShader;
		// The pixel shader for blitting the texture.
		private GorgonPixelShader _pixelShader;
		// The input layout for the blit vertices.
		private GorgonInputLayout _inputLayout;
		// The bindings for the vertex buffer.
		private GorgonVertexBufferBindings _vertexBufferBindings;
		// World/view/projection matrix.
		private GorgonConstantBuffer _wvpBuffer;
		// Flag used to determine if the blitter is initialized or not.
		private bool _initializedFlag;
		// The draw call used to blit the texture.
		private readonly GorgonDrawCall _drawCall;
		// Flag to indicate that the world/view/projection needs updating.
		private bool _needsWvpUpdate = true;
        // The bounds of the most recent target.
	    private DX.Rectangle? _targetBounds;
	    // The default pipeline state if no custom pipeline state was set by the user.
	    private GorgonPipelineStateInfo _pipelineStateInfo;
        // The previously used states by this blitter.
	    private readonly GorgonPipelineStateGroup<long> _cachedStates;
	    #endregion

        #region Methods.
		/// <summary>
        /// Function to update the projection data.
        /// </summary>
        private void UpdateProjection()
		{
			if ((!_needsWvpUpdate)
                || (_graphics.RenderTargets[0] == null))
			{
				return;
			}

		    GorgonRenderTargetView target = _graphics.RenderTargets[0];
            
			DX.Matrix.OrthoOffCenterLH(0,
						   target.Width,
						   target.Height,
						   0,
						   0,
						   1.0f,
						   out DX.Matrix projectionMatrix);
            
		    GorgonPointerAlias data = _wvpBuffer.Lock(D3D11.MapMode.WriteDiscard);
            data.Write(ref projectionMatrix);
            _wvpBuffer.Unlock(ref data);

		    _targetBounds = target.Bounds;
            _needsWvpUpdate = false;
		}

        /// <summary>
        /// Function to update the pipeline state for the blitter.
        /// </summary>
        /// <param name="pixelShader">The pixel shader used to override the default pixel shader.</param>
        /// <param name="blendState">The blend state to apply.</param>
	    private void UpdateState(GorgonPixelShader pixelShader, GorgonBlendState blendState)
	    {
            // Generate a key to pull the pipeline state from a local cache.
	        long key = ((pixelShader.ID & 0xffff) << 12) | (blendState.ID & 0xfff);

	        if (!_cachedStates.TryGetValue(key, out GorgonPipelineState state))
	        {
                // We don't have this guy cached, so pull it from the root cache.
                state = _graphics.GetPipelineState(_pipelineStateInfo);
                _cachedStates.Cache(key, state);
            }

	        _drawCall.PipelineState = state; 
	    }

		/// <summary>
		/// Function to initialize the blitter.
		/// </summary>
		private void Initialize()
		{
			// We've been initialized, so leave.
			if (_initializedFlag)
			{
				return;
			}

			_vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics.VideoDevice, Resources.GraphicsShaders, "GorgonBltVertexShader", GorgonGraphics.IsDebugEnabled);
			_pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics.VideoDevice, Resources.GraphicsShaders, "GorgonBltPixelShader", GorgonGraphics.IsDebugEnabled);

			_inputLayout = GorgonInputLayout.CreateUsingType<BltVertex>(_graphics.VideoDevice, _vertexShader);

			_vertexBufferBindings = new GorgonVertexBufferBindings(_inputLayout)
				                    {
					                    [0] = new GorgonVertexBufferBinding(new GorgonVertexBuffer("Gorgon Blitter Vertex Buffer",
					                                                                                _graphics,
					                                                                                new GorgonVertexBufferInfo
					                                                                                {
						                                                                                Usage = D3D11.ResourceUsage.Dynamic,
						                                                                                SizeInBytes = BltVertex.Size * 4
					                                                                                }), BltVertex.Size)
				                    };

			_wvpBuffer = new GorgonConstantBuffer("Gorgon Blitter WVP Buffer", _graphics, new GorgonConstantBufferInfo
				                                                                            {
					                                                                            Usage = D3D11.ResourceUsage.Dynamic,
																								SizeInBytes = DX.Matrix.SizeInBytes
				                                                                            });

			// Finish initalizing the draw call.
			_drawCall.VertexBuffers = _vertexBufferBindings;
			_drawCall.VertexShaderConstantBuffers[0] = _wvpBuffer;

			_pipelineStateInfo = new GorgonPipelineStateInfo
			                        {
			                            VertexShader = _vertexShader,
			                            PixelShader = _pixelShader,
			                            BlendStates = new[]
			                                        {
			                                            GorgonBlendState.NoBlending
			                                        }
			                        };

            UpdateState(_pixelShader, GorgonBlendState.NoBlending);

			_initializedFlag = true;
		}

	    /// <summary>
	    /// Function to blit the texture to the specified render target.
	    /// </summary>
	    /// <param name="texture">The texture that will be blitted to the render target.</param>
	    /// <param name="destRect">The layout area to blit the texture into.</param>
	    /// <param name="sourceOffset">The offset within the source texture to start blitting from.</param>
	    /// <param name="color">The color used to tint the diffuse value of the texture.</param>
	    /// <param name="clip"><b>true</b> to clip the contents of the texture if the destination is larger/small than the size of the texture.</param>
	    /// <param name="blendState">The blending state to apply.</param>
	    /// <param name="samplerState">The sampler state to apply.</param>
	    /// <param name="pixelShader">The pixel shader used to override the default pixel shader.</param>
	    /// <param name="pixelShaderConstants">The pixel shader constant buffers to use.</param>
	    public void Blit(GorgonTextureView texture,
	                     DX.Rectangle destRect,
	                     DX.Point sourceOffset,
	                     GorgonColor color,
	                     bool clip,
	                     GorgonBlendState blendState,
	                     GorgonSamplerState samplerState,
	                     GorgonPixelShader pixelShader,
	                     GorgonConstantBuffers pixelShaderConstants)
	    {
	        if ((texture == null)
	            || (_graphics.RenderTargets[0] == null))
	        {
	            return;
	        }

	        GorgonRenderTargetView currentView = _graphics.RenderTargets[0];

	        // We need to update the projection/view if the size of the target changes.
	        if ((_targetBounds == null)
	            || (currentView.Width != _targetBounds.Value.Width)
	            || (currentView.Height != _targetBounds.Value.Height))
	        {
	            _needsWvpUpdate = true;
	        }

	        UpdateProjection();

	        // Apply the states.
	        _drawCall.PixelShaderResourceViews[0] = texture;
	        _drawCall.PixelShaderSamplers[0] = samplerState ?? GorgonSamplerState.Default;

	        // Apply the correct pipeline state.
	        if (blendState == null)
	        {
	            blendState = GorgonBlendState.NoBlending;
	        }

	        if (pixelShader == null)
	        {
	            pixelShader = _pixelShader;
	        }

	        if ((_pipelineStateInfo.PixelShader != pixelShader)
	            || (_pipelineStateInfo.BlendStates[0] != blendState))
	        {
	            _pipelineStateInfo.PixelShader = pixelShader;
	            _pipelineStateInfo.BlendStates[0] = blendState;
	            UpdateState(_pipelineStateInfo.PixelShader, _pipelineStateInfo.BlendStates[0]);
	        }

	        // Apply pixel shader constants as needed.
	        if ((pixelShaderConstants != null) && (_pixelShader != _pipelineStateInfo.PixelShader))
	        {
	            ref (int Start, int Count) buffers = ref pixelShaderConstants.GetDirtyItems();

	            for (int i = buffers.Start; i < buffers.Start + buffers.Count; ++i)
	            {
	                _drawCall.PixelShaderConstantBuffers[i] = pixelShaderConstants[i];
	            }
	        }
	        else
	        {
	            _drawCall.PixelShaderConstantBuffers.Clear();
	        }

	        // Calculate position on the texture.
	        DX.Vector2 topLeft = texture.Texture.ToTexel(sourceOffset);
	        DX.Vector2 bottomRight = texture.Texture.ToTexel(clip ? new DX.Point(destRect.Width, destRect.Height) : new DX.Point(texture.Width, texture.Height));

	        // Update the vertices.
	        _vertices[0] = new BltVertex
	                       {
	                           Position = new DX.Vector4(destRect.X, destRect.Y, 0, 1.0f),
	                           Uv = topLeft,
	                           Color = color
	                       };
	        _vertices[1] = new BltVertex
	                       {
	                           Position = new DX.Vector4(destRect.Right, destRect.Y, 0, 1.0f),
	                           Uv = new DX.Vector2(bottomRight.X, topLeft.Y),
	                           Color = color
	                       };
	        _vertices[2] = new BltVertex
	                       {
	                           Position = new DX.Vector4(destRect.X, destRect.Bottom, 0, 1.0f),
	                           Uv = new DX.Vector2(topLeft.X, bottomRight.Y),
	                           Color = color
	                       };
	        _vertices[3] = new BltVertex
	                       {
	                           Position = new DX.Vector4(destRect.Right, destRect.Bottom, 0, 1.0f),
	                           Uv = new DX.Vector2(bottomRight.X, bottomRight.Y),
	                           Color = color
	                       };

	        // Copy to the vertex buffer.
	        GorgonPointerAlias data = _vertexBufferBindings[0].VertexBuffer.Lock(D3D11.MapMode.WriteDiscard);
	        data.WriteRange(_vertices);
	        _vertexBufferBindings[0].VertexBuffer.Unlock(ref data);

	        _graphics.Submit(_drawCall);
	    }

	    /// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
            _wvpBuffer?.Dispose();
			_vertexBufferBindings?[0].VertexBuffer?.Dispose();
			_inputLayout?.Dispose();
			_vertexShader?.Dispose();
			_pixelShader?.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="TextureBlitter"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create the required objects for blitting.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
		public TextureBlitter(GorgonGraphics graphics)
		{
			_graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));

		    _cachedStates = _graphics.GetPipelineStateGroup<long>(BlitterGroupName);

			_drawCall = new GorgonDrawCall
			            {
				            PrimitiveTopology = D3D.PrimitiveTopology.TriangleStrip,
				            VertexCount = _vertices.Length,
				            PixelShaderSamplers =
				            {
					            [0] = GorgonSamplerState.Default
				            }
			            };

		    Initialize();
        }
	    #endregion
	}
}
