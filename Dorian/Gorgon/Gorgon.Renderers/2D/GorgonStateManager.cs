#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, February 21, 2012 6:45:30 AM
// 
#endregion

using System;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{	
	/// <summary>
	/// State changes.
	/// </summary>
	[Flags()]
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
	/// Used to manage the states between objects.
	/// </summary>
	class GorgonStateManager
	{
		#region Variables.
		private readonly GorgonGraphics _graphics;
		private readonly Gorgon2D _gorgon2D;
		private GorgonBlendStates _blendState = GorgonBlendStates.DefaultStates;
		private GorgonRasterizerStates _rasterState = GorgonRasterizerStates.CullBackFace;
		private GorgonTextureSamplerStates _samplerState = GorgonTextureSamplerStates.LinearFilter;
		private GorgonVertexBufferBinding _vertexBuffer = default(GorgonVertexBufferBinding);
		private GorgonDepthStencilStates _depthState = GorgonDepthStencilStates.NoDepthStencil;
		private GorgonColor _blendFactor = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
		private GorgonRangeF _alphaTestValue = GorgonRangeF.Empty;
		private bool _alphaTestEnabled;
		private GorgonShaderView _textureView;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the alpha testing state.
		/// </summary>
		/// <param name="enabled">TRUE to enable the alpha test, FALSE to disable it.</param>
		/// <param name="range">Range of values to include.</param>
		private void UpdateAlphaTest(bool enabled, GorgonRangeF range)
		{
			_alphaTestEnabled = enabled;
			_alphaTestValue = range;

			_gorgon2D.AlphaTestStream.Position = 0;
			_gorgon2D.AlphaTestStream.Write(_alphaTestEnabled);
			// Pad to 4 bytes, bool is 1 byte in .NET.
			_gorgon2D.AlphaTestStream.Write<byte>(0);
			_gorgon2D.AlphaTestStream.Write<byte>(0);
			_gorgon2D.AlphaTestStream.Write<byte>(0);
			_gorgon2D.AlphaTestStream.Write<float>(_alphaTestValue.Minimum);
			_gorgon2D.AlphaTestStream.Write<float>(_alphaTestValue.Maximum);
			_gorgon2D.AlphaTestStream.Position = 0;
			_gorgon2D.AlphaTestBuffer.Update(_gorgon2D.AlphaTestStream);
		}

		/// <summary>
		/// Function to check to see if there are any state changes.
		/// </summary>
		/// <param name="renderable">Renderable object to use.</param>
		/// <returns></returns>
		public StateChange CheckState(IRenderable renderable)
		{
			var result = StateChange.None;
			GorgonRenderable.DepthStencilStates depthStencil = renderable.DepthStencil;
			GorgonRenderable.BlendState blending = renderable.Blending;
			GorgonRenderable.TextureSamplerState sampler = renderable.TextureSampler;
            GorgonShaderView textureView = null;

            // Get the current texture view.
            if (renderable.Texture != null)
            {
                textureView = renderable.Texture;
            }

			if (textureView != _textureView)
				result |= StateChange.Texture;

			if (_gorgon2D.IsBlendingEnabled != _blendState.RenderTarget0.IsBlendingEnabled)
				result |= StateChange.BlendEnable;

			if (_gorgon2D.IsBlendingEnabled)
			{
				if (!blending.BlendFactor.Equals(_blendFactor))
					result |= StateChange.BlendFactor;

				if ((blending.SourceBlend != _blendState.RenderTarget0.SourceBlend)
					|| (blending.DestinationBlend != _blendState.RenderTarget0.DestinationBlend)					
					|| (blending.AlphaOperation != _blendState.RenderTarget0.AlphaOperation)
					|| (blending.BlendOperation != _blendState.RenderTarget0.BlendingOperation)
					|| (blending.SourceAlphaBlend != _blendState.RenderTarget0.SourceAlphaBlend)
					|| (blending.DestinationAlphaBlend != _blendState.RenderTarget0.DestinationAlphaBlend)
					|| (blending.WriteMask != _blendState.RenderTarget0.WriteMask))
					result |= StateChange.BlendState;
			}

			if ((sampler.TextureFilter != _samplerState.TextureFilter)
				|| (sampler.VerticalWrapping != _samplerState.VerticalAddressing)
				|| (sampler.HorizontalWrapping != _samplerState.HorizontalAddressing)
				|| (!sampler.BorderColor.Equals(_samplerState.BorderColor)))
				result |= StateChange.Sampler;

			if (((_gorgon2D.ClipRegion != null) != (_rasterState.IsScissorTestingEnabled))
				|| (renderable.DepthStencil.DepthBias != _rasterState.DepthBias)
				|| (renderable.CullingMode != _rasterState.CullingMode)
				|| (_gorgon2D.IsMultisamplingEnabled != _rasterState.IsMultisamplingEnabled))
				result |= StateChange.Raster;

			if (renderable.PrimitiveType != _graphics.Input.PrimitiveType)
				result |= StateChange.PrimitiveType;

			if (renderable.IndexBuffer != _graphics.Input.IndexBuffer)
				result |= StateChange.IndexBuffer;

			if (!renderable.VertexBufferBinding.Equals(_vertexBuffer))
				result |= StateChange.VertexBuffer;

			if ((_gorgon2D.IsAlphaTestEnabled != _alphaTestEnabled) || (((_gorgon2D.IsAlphaTestEnabled) && (!renderable.AlphaTestValues.Equals(_alphaTestValue)))))
				result |= StateChange.AlphaTest;

			if ((_gorgon2D.IsDepthBufferEnabled != _depthState.IsDepthEnabled) 
				|| (_gorgon2D.IsStencilEnabled != _depthState.IsStencilEnabled))
				result |= StateChange.DepthStencil;

			if (depthStencil.DepthStencilReference != _graphics.Output.DepthStencilState.DepthStencilReference)
				result |= StateChange.DepthStencilReference;

			if ((_gorgon2D.IsDepthBufferEnabled) &&
				(depthStencil.IsDepthWriteEnabled != _depthState.IsDepthWriteEnabled)
				|| (depthStencil.DepthComparison != _depthState.DepthComparison))
				result |= StateChange.DepthStencil;

			if (_gorgon2D.IsStencilEnabled)
			{
				if ((_depthState.StencilReadMask != depthStencil.StencilReadMask) 
					|| (_depthState.StencilWriteMask != depthStencil.StencilWriteMask)
					|| (_depthState.StencilFrontFace.ComparisonOperator != depthStencil.FrontFace.ComparisonOperator)
					|| (_depthState.StencilFrontFace.DepthFailOperation != depthStencil.FrontFace.DepthFailOperation)
					|| (_depthState.StencilFrontFace.FailOperation != depthStencil.FrontFace.FailOperation)
					|| (_depthState.StencilFrontFace.PassOperation != depthStencil.FrontFace.PassOperation)
					|| (_depthState.StencilBackFace.ComparisonOperator != depthStencil.BackFace.ComparisonOperator)
					|| (_depthState.StencilBackFace.DepthFailOperation != depthStencil.BackFace.DepthFailOperation)
					|| (_depthState.StencilBackFace.FailOperation != depthStencil.BackFace.FailOperation)
					|| (_depthState.StencilBackFace.PassOperation != depthStencil.BackFace.PassOperation))
					result |= StateChange.DepthStencil;
			}				

			return result;
		}

		/// <summary>
		/// Function to apply any state changes.
		/// </summary>
		/// <param name="renderable">Renderable object to retrieve states from.</param>
		/// <param name="state">States that need updating.</param>
		public void ApplyState(IRenderable renderable, StateChange state)
		{
			GorgonRenderable.DepthStencilStates depthStencil = renderable.DepthStencil;
			GorgonRenderable.BlendState blending = renderable.Blending;
			GorgonRenderable.TextureSamplerState sampler = renderable.TextureSampler;

			if ((state & StateChange.Texture) == StateChange.Texture)
			{
                _textureView = _graphics.Shaders.PixelShader.Resources[0] = renderable.Texture;

				// If we have a texture change, and we have the default diffuse shader loaded, then switch to the textured shader, otherwise 
				// switch to the diffuse shader.
				_gorgon2D.PixelShader.TextureSwitch(renderable.Texture);
			}
		
			if ((state & StateChange.BlendEnable) == StateChange.BlendEnable)
			{
				_blendState.RenderTarget0.IsBlendingEnabled = _gorgon2D.IsBlendingEnabled;
				_graphics.Output.BlendingState.States = _blendState;
			}

			if ((state & StateChange.BlendFactor) == StateChange.BlendFactor)
			{
				_blendFactor = blending.BlendFactor;
				_graphics.Output.BlendingState.BlendFactor = blending.BlendFactor;
			}

			if ((state & StateChange.BlendState) == StateChange.BlendState)
			{

				_blendState.RenderTarget0.AlphaOperation = blending.AlphaOperation;
				_blendState.RenderTarget0.BlendingOperation = blending.BlendOperation;
				_blendState.RenderTarget0.DestinationAlphaBlend = blending.DestinationAlphaBlend;
				_blendState.RenderTarget0.DestinationBlend = blending.DestinationBlend;
				_blendState.RenderTarget0.SourceAlphaBlend = blending.SourceAlphaBlend;
				_blendState.RenderTarget0.SourceBlend = blending.SourceBlend;
				_blendState.RenderTarget0.WriteMask = blending.WriteMask;
				_graphics.Output.BlendingState.States = _blendState;
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
				_rasterState.IsScissorTestingEnabled = (_gorgon2D.ClipRegion != null);
				_rasterState.CullingMode = renderable.CullingMode;
				_rasterState.IsMultisamplingEnabled = _gorgon2D.IsMultisamplingEnabled;
				_rasterState.DepthBias = depthStencil.DepthBias;
				_graphics.Rasterizer.States = _rasterState;
			}

			if ((state & StateChange.PrimitiveType) == StateChange.PrimitiveType)
				_graphics.Input.PrimitiveType = renderable.PrimitiveType;

			if ((state & StateChange.IndexBuffer) == StateChange.IndexBuffer)
				_graphics.Input.IndexBuffer = renderable.IndexBuffer;

			if ((state & StateChange.VertexBuffer) == StateChange.VertexBuffer)
			{
				_vertexBuffer = renderable.VertexBufferBinding;
				_graphics.Input.VertexBuffers[0] = renderable.VertexBufferBinding;
			}

			if ((state & StateChange.AlphaTest) == StateChange.AlphaTest)
				UpdateAlphaTest(_gorgon2D.IsAlphaTestEnabled, renderable.AlphaTestValues);

			if ((state & StateChange.DepthStencilReference) == StateChange.DepthStencilReference)
				_graphics.Output.DepthStencilState.DepthStencilReference = depthStencil.DepthStencilReference;

			if ((state & StateChange.DepthStencil) == StateChange.DepthStencil)
			{				
				_depthState.IsDepthEnabled = _gorgon2D.IsDepthBufferEnabled;
				_depthState.IsDepthWriteEnabled = depthStencil.IsDepthWriteEnabled;
				_depthState.DepthComparison = depthStencil.DepthComparison;
				_depthState.StencilReadMask = depthStencil.StencilReadMask;
				_depthState.StencilWriteMask = depthStencil.StencilWriteMask;
				_depthState.IsStencilEnabled = _gorgon2D.IsStencilEnabled;
				_depthState.StencilFrontFace.ComparisonOperator = depthStencil.FrontFace.ComparisonOperator;
				_depthState.StencilFrontFace.DepthFailOperation = depthStencil.FrontFace.DepthFailOperation;
				_depthState.StencilFrontFace.FailOperation = depthStencil.FrontFace.FailOperation;
				_depthState.StencilFrontFace.PassOperation = depthStencil.FrontFace.PassOperation;
				_depthState.StencilBackFace.ComparisonOperator = depthStencil.BackFace.ComparisonOperator;
				_depthState.StencilBackFace.DepthFailOperation = depthStencil.BackFace.DepthFailOperation;
				_depthState.StencilBackFace.FailOperation = depthStencil.BackFace.FailOperation;
				_depthState.StencilBackFace.PassOperation = depthStencil.BackFace.PassOperation;
				_graphics.Output.DepthStencilState.States = _depthState;
			}
		}

		/// <summary>
		/// Function to get the default states.
		/// </summary>
		public void GetDefaults()
		{
			_vertexBuffer = _graphics.Input.VertexBuffers[0];
			_blendState = _graphics.Output.BlendingState.States;
			_blendFactor = _graphics.Output.BlendingState.BlendFactor;
			_rasterState = _graphics.Rasterizer.States;
			_samplerState = _graphics.Shaders.PixelShader.TextureSamplers[0];
			_depthState = _graphics.Output.DepthStencilState.States;
			_textureView = _graphics.Shaders.PixelShader.Resources[0];
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStateManager"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		public GorgonStateManager(Gorgon2D gorgon2D)
		{
			_gorgon2D = gorgon2D;
			_graphics = _gorgon2D.Graphics;
		}
		#endregion
	}
}
