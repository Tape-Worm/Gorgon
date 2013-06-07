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
// Created: Thursday, May 9, 2013 9:39:03 PM
// 
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// Extension methods for the 2D renderer.
	/// </summary>
	public static class Gorgon2DExtensions
	{
		#region Classes.
		/// <summary>
		/// The current set of states when the renderer was started.
		/// </summary>
		private class PreviousStates
		{
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
		    private GorgonDepthStencil _depthStencil;                               // Default depth/stencil buffer.
			private GorgonIndexBuffer _indexBuffer;									// Index buffer.
			private GorgonVertexBufferBinding _vertexBuffer;						// Vertex buffer.
			private GorgonInputLayout _inputLayout;									// Input layout.
			private PrimitiveType _primitiveType;									// Primitive type.
			private GorgonDepthStencilStates _depthStencilState;					// Depth stencil states
			private int _depthStencilReference;										// Depth stencil reference.
		    private Rectangle[] _scissorTests;                                      // Scissor tests
		    private GorgonViewport[] _viewports;                                    // Viewports.

			/// <summary>
			/// Function to save the state information to this object.
			/// </summary>
			/// <param name="graphics">Graphics interface.</param>
			public void Save(GorgonGraphics graphics)
			{
				_targets = graphics.Output.GetRenderTargets();
				_indexBuffer = graphics.Input.IndexBuffer;
				_vertexBuffer = graphics.Input.VertexBuffers[0];
				_inputLayout = graphics.Input.Layout;
				_primitiveType = graphics.Input.PrimitiveType;
				_pixelShader = graphics.Shaders.PixelShader.Current;
				_vertexShader = graphics.Shaders.VertexShader.Current;
				_blendStates = graphics.Output.BlendingState.States;
				_blendFactor = graphics.Output.BlendingState.BlendFactor;
				_blendSampleMask = graphics.Output.BlendingState.BlendSampleMask;
				_rasterStates = graphics.Rasterizer.States;
				_samplerState = graphics.Shaders.PixelShader.TextureSamplers[0];
				_resource = graphics.Shaders.PixelShader.Resources[0];
				_depthStencilState = graphics.Output.DepthStencilState.States;
				_depthStencilReference = graphics.Output.DepthStencilState.DepthStencilReference;
				_rasterStates.IsScissorTestingEnabled = false;
				_depthStencil = graphics.Output.DepthStencilBuffer;
			    _viewports = graphics.Rasterizer.GetViewports();
			    _scissorTests = graphics.Rasterizer.GetScissorRectangles();

				_vsConstantBuffers = new Dictionary<int, GorgonConstantBuffer>();
				_psConstantBuffers = new Dictionary<int, GorgonConstantBuffer>();

				// Only store the constant buffers that we were using.
				// We need to store all the constant buffers because the effects 
				// make use of multiple constant slots.  Unlike the resource views,
				// where we know that we're only using the first item (all bets are 
				// off if a user decides to use another resource view slot), there's no
				// guarantee that we'll be only using 1 or 2 constant buffer slots.
				for (int i = 0; i < graphics.Shaders.VertexShader.ConstantBuffers.Count; i++)
				{
					if (graphics.Shaders.VertexShader.ConstantBuffers[i] != null)
					{
						_vsConstantBuffers[i] = graphics.Shaders.VertexShader.ConstantBuffers[i];
					}
				}

				for (int i = 0; i < graphics.Shaders.PixelShader.ConstantBuffers.Count; i++)
				{
					if (graphics.Shaders.PixelShader.ConstantBuffers[i] != null)
					{
						_psConstantBuffers[i] = graphics.Shaders.PixelShader.ConstantBuffers[i];
					}
				}
			}

			/// <summary>
			/// Function to restore the previous states.
			/// </summary>
			/// <param name="graphics">Graphics interface.</param>
			public void Restore(GorgonGraphics graphics)
			{
				graphics.Output.SetRenderTargets(_targets, _depthStencil);
                graphics.Rasterizer.SetViewports(_viewports);
                graphics.Rasterizer.SetScissorRectangles(_scissorTests);
				graphics.Input.IndexBuffer = _indexBuffer;
				graphics.Input.VertexBuffers[0] = _vertexBuffer;
				graphics.Input.Layout = _inputLayout;
				graphics.Input.PrimitiveType = _primitiveType;
				graphics.Shaders.PixelShader.Current = _pixelShader;
				graphics.Shaders.VertexShader.Current = _vertexShader;
				graphics.Output.BlendingState.BlendSampleMask = _blendSampleMask;
				graphics.Output.BlendingState.BlendFactor = _blendFactor;
				graphics.Output.BlendingState.States = _blendStates;
				graphics.Output.DepthStencilState.States = _depthStencilState;
				graphics.Output.DepthStencilState.DepthStencilReference = _depthStencilReference;
				graphics.Rasterizer.States = _rasterStates;
				graphics.Shaders.PixelShader.Resources[0] = _resource;
				graphics.Shaders.PixelShader.TextureSamplers[0] = _samplerState;

				// Restore any constant buffers.				
				foreach (var vsConstant in _vsConstantBuffers)
				{
					graphics.Shaders.VertexShader.ConstantBuffers[vsConstant.Key] = vsConstant.Value;
				}

				foreach (var psConstant in _psConstantBuffers)
				{
					graphics.Shaders.PixelShader.ConstantBuffers[psConstant.Key] = psConstant.Value;
				}
			}
		}
		#endregion

		#region Variables.
		private static PreviousStates _previousStates;
		private static Gorgon2D _currentRenderer;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to begin 2D rendering for this renderer.
		/// </summary>
		/// <param name="renderer">Renderer that will start rendering.</param>
		/// <remarks>This is used to remember previous states, and set the default states for the 2D renderer.
		/// <para>Calling the <see cref="GorgonLibrary.Renderers.Gorgon2DExtensions.End2D">End2D</see> method will restore the last set of render states prior to the Begin2D call.</para>
		/// <para>If this method is called more than once in succession (i.e. without a corresponding End2D), then it will do nothing.</para>
		/// <para>This is implicitly called by the constructor and does not need to be called after creating an instance of the 2D interface.</para>
		/// <para>If using multiple 2D renderers, this method should be called on the additional renderer before drawing anything with the additonal renderer.</para>
		/// <para></para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is NULL (Nothing in VB.Net).</exception>
		public static void Begin2D(this Gorgon2D renderer)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException("renderer");
			}

			if (_currentRenderer == renderer)
			{
				return;
			}
			
			// End the rendering on the other renderer.
			if (_currentRenderer != null)
			{
				// End the rendering on the previous renderer.
				End2D(_currentRenderer);
			}
			
			_previousStates = new PreviousStates();
			_previousStates.Save(renderer.Graphics);

			// Perform set up.
			renderer.Setup();

			// Set this as the active renderer.
			_currentRenderer = renderer;
		}

		/// <summary>
		/// Function to end 2D rendering for this renderer.
		/// </summary>
		/// <remarks>This will restore the states to their original values before a 2D renderer was started, or to before the last call of the <see cref="GorgonLibrary.Renderers.Gorgon2DExtensions.Begin2D">Begin2D</see> method.
		/// <para>When restoring, the viewport may not be reset when the initial render target is NULL (Nothing in VB.Net), and consequently will need to be set when a new render target is assigned to the <see cref="GorgonLibrary.Graphics.GorgonGraphics">Graphics</see> interface.</para>
		/// </remarks>		
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is NULL (Nothing in VB.Net).</exception>
		public static void End2D(this Gorgon2D renderer)
		{
			if ((_currentRenderer == null) || (_previousStates == null))
			{
				return;
			}

			if (renderer == null)
			{
				throw new ArgumentNullException("renderer");
			}

			if (renderer != _currentRenderer)
			{
				return;
			}

		    renderer.ClearCache();
			_previousStates.Restore(renderer.Graphics);
			_previousStates = null;
			_currentRenderer = null;
		}
		#endregion
	}
}
