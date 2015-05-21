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

using System;
using System.Collections.Generic;
using System.Drawing;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
	/// <summary>
	/// State changes.
	/// </summary>
	[Flags]
	enum StateChange
	{
		/// <summary>
		/// No state change.
		/// </summary>
		None = 0,
		/// <summary>
		/// Texture changed.
		/// </summary>
		Texture = 1,
		/// <summary>
		/// Shader changed.
		/// </summary>
		Shader = 2,
		/// <summary>
		/// Blending state changed.
		/// </summary>
		BlendState = 4,
		/// <summary>
		/// Primitive type changed.
		/// </summary>
		PrimitiveType = 8,
		/// <summary>
		/// Vertex buffer changed.
		/// </summary>
		VertexBuffer = 16,
		/// <summary>
		/// Index buffer changed.
		/// </summary>
		IndexBuffer = 32,
		/// <summary>
		/// Sampler state changed.
		/// </summary>
		Sampler = 64,
		/// <summary>
		/// Blending factor.
		/// </summary>
		BlendFactor = 128,
		/// <summary>
		/// Rasterizer state changed.
		/// </summary>
		Raster = 256,
		/// <summary>
		/// Depth/stencil state changed.
		/// </summary>
		DepthStencil = 512,
		/// <summary>
		/// Alpha test state changed.
		/// </summary>
		AlphaTest = 1024,
		/// <summary>
		/// Blending enable state changed.
		/// </summary>
		BlendEnable = 2048,
		/// <summary>
		/// Clipping enable state changed.
		/// </summary>
		ClipEnable = 4096,
		/// <summary>
		/// Depth stencil reference changed.
		/// </summary>
		DepthStencilReference = 8192
	}
	
	/// <summary>
    /// Holds a record of the current state of the graphics interface.
    /// </summary>
    public class Gorgon2DStateRecall
	{
		#region Variables.
		private readonly GorgonGraphics _graphics;                              // Graphics interface that was used to record.
        private GorgonPixelShader _pixelShader;									// Pixel shader.
        private GorgonVertexShader _vertexShader;								// Vertex shader.
        private GorgonBlendStates _blendStates;									// Blending states.
        private GorgonColor _blendFactor;										// Blending factor.
        private uint _blendSampleMask;											// Blending sample mask.
        private GorgonRasterizerStates _rasterStates;							// Rasterizer states.
        private GorgonTextureSamplerStates _samplerState;						// Sampler states.
        private GorgonShaderView _resource;									    // First pixel shader resource.
        private Dictionary<int, GorgonConstantBuffer> _vsConstantBuffers;		// The vertex shader constant buffers.
        private Dictionary<int, GorgonConstantBuffer> _psConstantBuffers;		// The pixel shader constant buffers.
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
	    private Gorgon2DAlphaTest _alphaTest;									// Alpha test values.
		private GorgonUnorderedAccessView[] _uavs;								// Unordered access views.
		#endregion

		#region Properties.
		/// <summary>
        /// Property to return the renderer that created this object.
        /// </summary>
        public Gorgon2D Gorgon2D
        {
            get;
            private set;
        }
		#endregion

		#region Methods.
		/// <summary>
        /// Function to save the state information to this object.
        /// </summary>
        private void Save()
        {
            _targets = _graphics.Output.GetRenderTargets();
			_uavs = _graphics.Output.GetUnorderedAccessViews();
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
            _depthStencilReference = _graphics.Output.DepthStencilState.StencilReference;
            _rasterStates.IsScissorTestingEnabled = false;
            _depthStencil = _graphics.Output.DepthStencilView;
            _viewports = _graphics.Rasterizer.GetViewports();
            _scissorTests = _graphics.Rasterizer.GetScissorRectangles();
			_alphaTest = new Gorgon2DAlphaTest(Gorgon2D.IsAlphaTestEnabled, GorgonRangeF.Empty);

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
        /// <param name="disposing">TRUE to restore states just for the graphics API on diposal of the 2D renderer, FALSE to restore all states.</param>
        internal void Restore(bool disposing)
        {
	        if (_uavs.Length > 0)
	        {
		        _graphics.Output.SetRenderTargets(_targets, _depthStencil, _targets.Length, _uavs);
	        }
	        else
	        {
				_graphics.Output.SetRenderTargets(_targets, _depthStencil);       
	        }

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
            _graphics.Output.DepthStencilState.StencilReference = _depthStencilReference;
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

	        if (!disposing)
	        {
		        Gorgon2D.PixelShader.AlphaTestValuesBuffer.Update(ref _alphaTest);
	        }
        }

		/// <summary>
		/// Function to apply any state changes.
		/// </summary>
		/// <param name="renderable">Renderable object to retrieve states from.</param>
		/// <param name="state">States that need updating.</param>
		internal void UpdateState(IRenderable renderable, StateChange state)
		{
			GorgonRenderable.DepthStencilStates depthStencil = renderable.DepthStencil;
			GorgonRenderable.BlendState blending = renderable.Blending;
			GorgonRenderable.TextureSamplerState sampler = renderable.TextureSampler;

			if ((state & StateChange.Texture) == StateChange.Texture)
			{
				_resource = _graphics.Shaders.PixelShader.Resources[0] = renderable.Texture;

				// If we have a texture change, and we have the default diffuse shader loaded, then switch to the textured shader, otherwise 
				// switch to the diffuse shader.
				Gorgon2D.PixelShader.TextureSwitch(renderable.Texture);
			}

			if ((state & StateChange.BlendFactor) == StateChange.BlendFactor)
			{
				_blendFactor = blending.BlendFactor;
				_graphics.Output.BlendingState.BlendFactor = blending.BlendFactor;
			}

			if ((state & StateChange.BlendState) == StateChange.BlendState)
			{
				_blendStates.RenderTarget0.IsBlendingEnabled = Gorgon2D.IsBlendingEnabled;
				_blendStates.RenderTarget0.AlphaOperation = blending.AlphaOperation;
				_blendStates.RenderTarget0.BlendingOperation = blending.BlendOperation;
				_blendStates.RenderTarget0.DestinationAlphaBlend = blending.DestinationAlphaBlend;
				_blendStates.RenderTarget0.DestinationBlend = blending.DestinationBlend;
				_blendStates.RenderTarget0.SourceAlphaBlend = blending.SourceAlphaBlend;
				_blendStates.RenderTarget0.SourceBlend = blending.SourceBlend;
				_blendStates.RenderTarget0.WriteMask = blending.WriteMask;
				_graphics.Output.BlendingState.States = _blendStates;
			}

			if ((state & StateChange.Sampler) == StateChange.Sampler)
			{
				_samplerState.HorizontalAddressing = sampler.HorizontalWrapping;
				_samplerState.VerticalAddressing = sampler.VerticalWrapping;
				_samplerState.BorderColor = sampler.BorderColor;
				_samplerState.TextureFilter = sampler.TextureFilter;
				_graphics.Shaders.PixelShader.TextureSamplers[0] = _samplerState;
			}

			if ((state & StateChange.Raster) == StateChange.Raster)
			{
				_rasterStates.IsScissorTestingEnabled = Gorgon2D.ClipRegion != null;
				_rasterStates.CullingMode = renderable.CullingMode;
				_rasterStates.IsMultisamplingEnabled = Gorgon2D.IsMultisamplingEnabled;
				_rasterStates.DepthBias = depthStencil.DepthBias;
				_graphics.Rasterizer.States = _rasterStates;
			}

			if ((state & StateChange.PrimitiveType) == StateChange.PrimitiveType)
			{
                _primitiveType = _graphics.Input.PrimitiveType = renderable.PrimitiveType;
			}

			if ((state & StateChange.IndexBuffer) == StateChange.IndexBuffer)
			{
				_indexBuffer = _graphics.Input.IndexBuffer = renderable.IndexBuffer;
			}

			if ((state & StateChange.VertexBuffer) == StateChange.VertexBuffer)
			{
                _vertexBuffer = _graphics.Input.VertexBuffers[0] = renderable.VertexBufferBinding;
			}

			if ((state & StateChange.AlphaTest) == StateChange.AlphaTest)
			{
				_alphaTest = new Gorgon2DAlphaTest(Gorgon2D.IsAlphaTestEnabled, renderable.AlphaTestValues);
				Gorgon2D.PixelShader.AlphaTestValuesBuffer.Update(ref _alphaTest);
			}

			if ((state & StateChange.DepthStencilReference) == StateChange.DepthStencilReference)
			{
                _depthStencilReference = _graphics.Output.DepthStencilState.StencilReference = depthStencil.StencilReference;
			}

			if ((state & StateChange.DepthStencil) != StateChange.DepthStencil)
			{
				return;
			}

			_depthStencilState.IsDepthEnabled = Gorgon2D.IsDepthBufferEnabled;
			_depthStencilState.IsDepthWriteEnabled = depthStencil.IsDepthWriteEnabled;
			_depthStencilState.DepthComparison = depthStencil.DepthComparison;
			_depthStencilState.StencilReadMask = depthStencil.StencilReadMask;
			_depthStencilState.StencilWriteMask = depthStencil.StencilWriteMask;
			_depthStencilState.IsStencilEnabled = Gorgon2D.IsStencilEnabled;
			_depthStencilState.StencilFrontFace.ComparisonOperator = depthStencil.FrontFace.ComparisonOperator;
			_depthStencilState.StencilFrontFace.DepthFailOperation = depthStencil.FrontFace.DepthFailOperation;
			_depthStencilState.StencilFrontFace.FailOperation = depthStencil.FrontFace.FailOperation;
			_depthStencilState.StencilFrontFace.PassOperation = depthStencil.FrontFace.PassOperation;
			_depthStencilState.StencilBackFace.ComparisonOperator = depthStencil.BackFace.ComparisonOperator;
			_depthStencilState.StencilBackFace.DepthFailOperation = depthStencil.BackFace.DepthFailOperation;
			_depthStencilState.StencilBackFace.FailOperation = depthStencil.BackFace.FailOperation;
			_depthStencilState.StencilBackFace.PassOperation = depthStencil.BackFace.PassOperation;
			_graphics.Output.DepthStencilState.States = _depthStencilState;
		}

		/// <summary>
		/// Function to determine what state changes are proposed by the current renderable.
		/// </summary>
		/// <param name="renderable">The renderable to test.</param>
		/// <returns>The states that require changes.</returns>
	    internal StateChange Compare(IRenderable renderable)
	    {
			var result = StateChange.None;
			GorgonRenderable.DepthStencilStates depthStencil = renderable.DepthStencil;
			GorgonRenderable.BlendState blending = renderable.Blending;
			GorgonRenderable.TextureSamplerState sampler = renderable.TextureSampler;
			GorgonShaderView textureView = null;

			var alphaTest = new Gorgon2DAlphaTest(Gorgon2D.IsAlphaTestEnabled, renderable.AlphaTestValues);

			// Get the current texture view.
			if (renderable.Texture != null)
			{
				textureView = renderable.Texture;
			}

			// Ensure that the stored resource is up to date.  If it's been changed outside of the
			// state management, then we can have issues (text does this).
			if ((textureView != _resource)
				|| (_resource != Gorgon2D.PixelShader.Resources[0]))
			{
				result |= StateChange.Texture;
			}

			if (Gorgon2D.IsBlendingEnabled == _blendStates.RenderTarget0.IsBlendingEnabled)
			{
				// If blending is enabled, then track the blending changes.
				if (Gorgon2D.IsBlendingEnabled)
				{
					if (!blending.BlendFactor.Equals(ref _blendFactor))
					{
						result |= StateChange.BlendFactor;
					}

					if ((blending.SourceBlend != _blendStates.RenderTarget0.SourceBlend)
						|| (blending.DestinationBlend != _blendStates.RenderTarget0.DestinationBlend)
						|| (blending.AlphaOperation != _blendStates.RenderTarget0.AlphaOperation)
						|| (blending.BlendOperation != _blendStates.RenderTarget0.BlendingOperation)
						|| (blending.SourceAlphaBlend != _blendStates.RenderTarget0.SourceAlphaBlend)
						|| (blending.DestinationAlphaBlend != _blendStates.RenderTarget0.DestinationAlphaBlend)
						|| (blending.WriteMask != _blendStates.RenderTarget0.WriteMask))
					{
						result |= StateChange.BlendState;
					}
				}
			}
			else
			{
				result |= StateChange.BlendState;
			}

			if ((sampler.TextureFilter != _samplerState.TextureFilter)
				|| (sampler.VerticalWrapping != _samplerState.VerticalAddressing)
				|| (sampler.HorizontalWrapping != _samplerState.HorizontalAddressing)
				|| (!sampler.BorderColor.Equals(ref _samplerState.BorderColor)))
			{
				result |= StateChange.Sampler;
			}

			if (((Gorgon2D.ClipRegion != null) != (_rasterStates.IsScissorTestingEnabled))
				|| (renderable.DepthStencil.DepthBias != _rasterStates.DepthBias)
				|| (renderable.CullingMode != _rasterStates.CullingMode)
				|| (Gorgon2D.IsMultisamplingEnabled != _rasterStates.IsMultisamplingEnabled))
			{
				result |= StateChange.Raster;
			}

			if (renderable.PrimitiveType != _graphics.Input.PrimitiveType)
			{
				result |= StateChange.PrimitiveType;
			}

			if (renderable.IndexBuffer != _graphics.Input.IndexBuffer)
			{
				result |= StateChange.IndexBuffer;
			}

			if (!renderable.VertexBufferBinding.Equals(ref _vertexBuffer))
			{
				result |= StateChange.VertexBuffer;
			}

			if (!Gorgon2DAlphaTest.Equals(ref _alphaTest, ref alphaTest))
			{
				result |= StateChange.AlphaTest;
			}
			
			if (depthStencil.StencilReference != _depthStencilReference)
			{
				result |= StateChange.DepthStencilReference;
			}

		    if ((Gorgon2D.IsDepthBufferEnabled != _depthStencilState.IsDepthEnabled)
		        || (Gorgon2D.IsStencilEnabled != _depthStencilState.IsStencilEnabled))
		    {
			    result |= StateChange.DepthStencil;
		    }
		    else
		    {
				if ((Gorgon2D.IsDepthBufferEnabled) &&
					(depthStencil.IsDepthWriteEnabled != _depthStencilState.IsDepthWriteEnabled)
					|| (depthStencil.DepthComparison != _depthStencilState.DepthComparison))
				{
					result |= StateChange.DepthStencil;
				}

				if ((Gorgon2D.IsStencilEnabled) && 
					((_depthStencilState.StencilReadMask != depthStencil.StencilReadMask)
						|| (_depthStencilState.StencilWriteMask != depthStencil.StencilWriteMask)
						|| (_depthStencilState.StencilFrontFace.ComparisonOperator != depthStencil.FrontFace.ComparisonOperator)
						|| (_depthStencilState.StencilFrontFace.DepthFailOperation != depthStencil.FrontFace.DepthFailOperation)
						|| (_depthStencilState.StencilFrontFace.FailOperation != depthStencil.FrontFace.FailOperation)
						|| (_depthStencilState.StencilFrontFace.PassOperation != depthStencil.FrontFace.PassOperation)
						|| (_depthStencilState.StencilBackFace.ComparisonOperator != depthStencil.BackFace.ComparisonOperator)
						|| (_depthStencilState.StencilBackFace.DepthFailOperation != depthStencil.BackFace.DepthFailOperation)
						|| (_depthStencilState.StencilBackFace.FailOperation != depthStencil.BackFace.FailOperation)
						|| (_depthStencilState.StencilBackFace.PassOperation != depthStencil.BackFace.PassOperation)))
				{
					result |= StateChange.DepthStencil;
				}
		    }

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DStateRecall"/> class.
        /// </summary>
        /// <param name="renderer">The renderer that is creating this object.</param>
        internal Gorgon2DStateRecall(Gorgon2D renderer)
        {
            Gorgon2D = renderer;
            _graphics = renderer.Graphics;

            Save();
		}
		#endregion
	}
}
