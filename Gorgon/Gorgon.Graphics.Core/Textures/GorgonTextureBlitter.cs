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
using System.Collections.Generic;
using System.Threading;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Provides functionality for blitting a texture to the currently active <see cref="GorgonGraphics.RenderTargets">render target</see>.
	/// </summary>
	public class GorgonTextureBlitter
		: IDisposable
	{
        #region Constants.
        // The name for the blitter pipeline state cache group.
	    private const string BlitterGroupName = "$$__GORGON_BLITTER_STATE_CACHE__$$xXx420XxXN00b";

        /// <summary>
        /// The name of the shader file data used for include files that wish to use the include shader.
        /// </summary>
	    public const string BlitterShaderIncludeFileName = "__Gorgon_TextureBlitter_Shader__";
        #endregion

        #region Variables.
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
		// Flag used to indicate that we're in the middle of initialization.
		private int _initializingFlag;
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
        // A flag to indicate that the state has been altered.
	    private bool _stateChanged = true;
        // The previously used states by this blitter.
	    private readonly GorgonPipelineStateGroup _cachedStates;
	    #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not the blitter has been changed since the last render.
        /// </summary>
	    private bool HasChanges => ((PixelShaderConstants.IsDirty) && (_pipelineStateInfo.PixelShader != _pixelShader)) || (_stateChanged);

	    /// <summary>
	    /// Property to return the graphics interface that owns this object.
	    /// </summary>
	    public GorgonGraphics Graphics
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return any custom pixel shader constants to apply.
	    /// </summary>
	    public GorgonConstantBuffers PixelShaderConstants => _drawCall.PixelShaderConstantBuffers;

	    /// <summary>
        /// Property to return the vertex shader for this blitter.
        /// </summary>
	    public GorgonVertexShader VertexShader => _vertexShader;

	    /// <summary>
	    /// Property to return the pixel shader for the blitter.
	    /// </summary>
	    /// <remarks>
	    /// Setting this value to <b>null</b> will reset the pixel shader back to the default pixel shader used to render this blitter.
	    /// </remarks>
	    public GorgonPixelShader PixelShader
	    {
	        get => _pipelineStateInfo.PixelShader;
	        set
	        {
	            if (value == null)
	            {
	                value = _pixelShader;
	            }

	            if (_pipelineStateInfo.PixelShader == value)
	            {
	                return;
	            }

	            _pipelineStateInfo.PixelShader = value;
	            _stateChanged = true;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the custom blend state to apply.
	    /// </summary>
	    /// <remarks>
	    /// Setting this value to <b>null</b> will reset the blending to <see cref="GorgonBlendState.NoBlending"/>.
	    /// </remarks>
	    public GorgonBlendState BlendState
	    {
	        get => _pipelineStateInfo.BlendStates[0];
	        set
	        {
	            if (value == null)
	            {
	                value = GorgonBlendState.NoBlending;
	            }

	            if (_pipelineStateInfo.BlendStates[0] == value)
	            {
	                return;
	            }

	            _pipelineStateInfo.BlendStates[0] = value;
	            _stateChanged = true;
	        }
	    }

	    /// <summary>
	    /// Property to set or return the sampler to use.
	    /// </summary>
	    /// <remarks>
	    /// Setting this value to <b>null</b> will reset the sample to <see cref="GorgonSamplerState.Default"/>
	    /// </remarks>
	    public GorgonSamplerState Sampler
	    {
	        get => _drawCall.PixelShaderSamplers[0];
	        set
	        {
	            if (value == null)
	            {
	                value = GorgonSamplerState.Default;
	            }

	            if (_drawCall.PixelShaderSamplers[0] == value)
	            {
	                return;
	            }

	            _drawCall.PixelShaderSamplers[0] = value;
	            _stateChanged = true;
	        }
	    }

	    /// <summary>
	    /// Property to set or return whether to perform scaling if the blitting width/height does not match the source texture width/height.
	    /// </summary>
	    public bool ScaleBlitter
		{
			get;
			set;
		} = true;

        /// <summary>
        /// Property to set or return the diffuse color for the blit.
        /// </summary>
	    public GorgonColor Color
	    {
	        get;
	        set;
	    } = GorgonColor.White;
        #endregion

        #region Methods.
		/// <summary>
        /// Function to update the projection data.
        /// </summary>
        private void UpdateProjection()
		{
			if ((!_needsWvpUpdate)
                || (Graphics.RenderTargets[0] == null))
			{
				return;
			}

		    GorgonRenderTargetView target = Graphics.RenderTargets[0];
            
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
	    private void UpdateState()
	    {
	        if (!HasChanges)
	        {
	            return;
	        }

            // Generate a key to pull the pipeline state from a local cache.
	        long key = ((PixelShader.ID & 0xffff) << 24) | ((BlendState.ID & 0xfff) << 12) | (Sampler.ID & 0xfff);

	        GorgonPipelineState state;
	        if (!_cachedStates.TryGetValue(key, out state))
	        {
                // We don't have this guy cached, so pull it from the root cache.
                state = Graphics.GetPipelineState(_pipelineStateInfo);
                _cachedStates.Cache(key, state);
            }

	        _drawCall.PipelineState = state; 

            _stateChanged = false;
	    }

		/// <summary>
		/// Function to initialize the blitter.
		/// </summary>
		private void Initialize()
		{
			try
			{
				// Don't let other threads initialize. The thread that entered will initialize and the other thread will spin 
				// until the original is done.
				while (Interlocked.Exchange(ref _initializingFlag, 1) == 1)
				{
					var waiter = new SpinWait();

					// Wait for a bit until the previous thread is done.
					waiter.SpinOnce();
				}

				// We've been initialized, so leave.
				if (_initializedFlag)
				{
					return;
				}

				_vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics.VideoDevice, Resources.GraphicsShaders, "GorgonBltVertexShader", GorgonGraphics.IsDebugEnabled);
				_pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics.VideoDevice, Resources.GraphicsShaders, "GorgonBltPixelShader", GorgonGraphics.IsDebugEnabled);

				_inputLayout = GorgonInputLayout.CreateUsingType<BltVertex>(Graphics.VideoDevice, _vertexShader);

				_vertexBufferBindings = new GorgonVertexBufferBindings(_inputLayout)
				                        {
					                        [0] = new GorgonVertexBufferBinding(new GorgonVertexBuffer("Gorgon Blitter Vertex Buffer",
					                                                                                   Graphics,
					                                                                                   new GorgonVertexBufferInfo
					                                                                                   {
						                                                                                   Usage = D3D11.ResourceUsage.Dynamic,
						                                                                                   SizeInBytes = BltVertex.Size * 4
					                                                                                   }), BltVertex.Size)
				                        };

				_wvpBuffer = new GorgonConstantBuffer("Gorgon Blitter WVP Buffer", Graphics, new GorgonConstantBufferInfo
				                                                                              {
					                                                                              Usage = D3D11.ResourceUsage.Dynamic,
																								  SizeInBytes = DX.Matrix.SizeInBytes
				                                                                              });

				// Finish initalizing the draw call.
				_drawCall.VertexBuffers = _vertexBufferBindings;
				_drawCall.VertexShaderConstantBuffers[0] = _wvpBuffer;

			    _pipelineStateInfo = new GorgonPipelineStateInfo
			                            {
			                                PixelShader = _pixelShader,
			                                VertexShader = _vertexShader,
			                                BlendStates = new[]
			                                              {
			                                                  GorgonBlendState.NoBlending
			                                              }

			                            };

                UpdateState();

				_initializedFlag = true;
			}
			finally
			{
				Interlocked.Exchange(ref _initializingFlag, 0);
			}
		}

		/// <summary>
		/// Function to blit the texture to the specified render target.
		/// </summary>
		/// <param name="texture">The texture that will be blitted to the render target.</param>
		/// <param name="x">The horizontal position to blit to.</param>
		/// <param name="y">The vertical position to blit to.</param>
		/// <param name="width">The width to blit.</param>
		/// <param name="height">The height to blit.</param>
		/// <param name="srcX">[Optional] The horizontal coordinate on the texture to blit.</param>
		/// <param name="srcY">[Optional] The vertical coordinate on the texture to blit.</param>
		public void Blit(GorgonTexture texture, int x, int y, int width, int height, int srcX = 0, int srcY = 0)
		{
			if ((texture == null)
                || (Graphics.RenderTargets[0] == null))
			{
				return;
			}

			GorgonRenderTargetView currentView = Graphics.RenderTargets[0];

			// We need to update the projection/view if the size of the target changes.
			if ((_targetBounds == null)
                || (currentView.Width != _targetBounds.Value.Width)
				|| (currentView.Height != _targetBounds.Value.Height))
			{
				_needsWvpUpdate = true;
			}

			UpdateProjection();

			// Apply the correct sampler.
			_drawCall.PixelShaderResourceViews[0] = texture.DefaultShaderResourceView;

            // Apply the correct pipeline state.
            UpdateState();

			// Calculate position on the texture.
			DX.Vector2 topLeft = texture.ToTexel(new DX.Point(srcX, srcY));
			DX.Vector2 bottomRight = texture.ToTexel(ScaleBlitter ? new DX.Point(texture.Info.Width, texture.Info.Height) : new DX.Point(width, height));

			// Update the vertices.
			_vertices[0] = new BltVertex
			               {
				               Position = new DX.Vector4(x, y, 0, 1.0f),
				               Uv = topLeft,
                               Color = Color
			               };
		    _vertices[1] = new BltVertex
		                   {
		                       Position = new DX.Vector4(x + width, y, 0, 1.0f),
		                       Uv = new DX.Vector2(bottomRight.X, topLeft.Y),
		                       Color = Color
		                   };
		    _vertices[2] = new BltVertex
		                   {
		                       Position = new DX.Vector4(x, y + height, 0, 1.0f),
		                       Uv = new DX.Vector2(topLeft.X, bottomRight.Y),
		                       Color = Color
		                   };
		    _vertices[3] = new BltVertex
		                   {
		                       Position = new DX.Vector4(x + width, y + height, 0, 1.0f),
		                       Uv = new DX.Vector2(bottomRight.X, bottomRight.Y),
		                       Color = Color
		                   };

            // Copy to the vertex buffer.
		    GorgonPointerAlias data = _vertexBufferBindings[0].VertexBuffer.Lock(D3D11.MapMode.WriteDiscard);
            data.WriteRange(_vertices);
		    _vertexBufferBindings[0].VertexBuffer.Unlock(ref data);

            Graphics.Submit(_drawCall);
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
		/// Initializes a new instance of the <see cref="GorgonTextureBlitter"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create the required objects for blitting.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
		public GorgonTextureBlitter(GorgonGraphics graphics)
		{
			Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));

		    _cachedStates = Graphics.GetPipelineStateGroup(BlitterGroupName);

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
