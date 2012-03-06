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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
		/// Alpha testing value changed.
		/// </summary>
		AlphaTestValue = 64,
		/// <summary>
		/// Sampler state changed.
		/// </summary>
		Sampler = 128,
		/// <summary>
		/// Blending factor.
		/// </summary>
		BlendFactor = 256,
		/// <summary>
		/// Rasterizer state changed.
		/// </summary>
		Raster = 512,
		/// <summary>
		/// Depth/stencil state changed.
		/// </summary>
		DepthStencil = 1024,
		/// <summary>
		/// Alpha test enabled state changed.
		/// </summary>
		AlphaTestEnable = 2048,
		/// <summary>
		/// Blending enable state changed.
		/// </summary>
		BlendEnable = 4096,
		/// <summary>
		/// Clipping enable state changed.
		/// </summary>
		ClipEnable = 8192,
		/// <summary>
		/// Depth stencil reference changed.
		/// </summary>
		DepthStencilReference = 16384
	}

	/// <summary>
	/// Used to manage the states between objects.
	/// </summary>
	class GorgonStateManager
	{
		#region Variables.
		private GorgonGraphics _graphics = null;
		private Gorgon2D _gorgon2D = null;
		private GorgonBlendStates _blendState = GorgonBlendStates.DefaultStates;
		private GorgonRasterizerStates _rasterState = GorgonRasterizerStates.DefaultStates;
		private GorgonTextureSamplerStates _samplerState = GorgonTextureSamplerStates.DefaultStates;
		private GorgonVertexBufferBinding _vertexBuffer = default(GorgonVertexBufferBinding);
		private GorgonDepthStencilStates _depthState = GorgonDepthStencilStates.DefaultStates;
		private GorgonColor _blendFactor = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
		private Gorgon2DShaders _shaders = null;
		private GorgonTexture _texture = null;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to check to see if there are any state changes.
		/// </summary>
		/// <param name="renderable">Renderable object to use.</param>
		/// <returns></returns>
		public StateChange CheckState(IRenderable renderable)
		{
			StateChange result = StateChange.None;
			GorgonRenderable.DepthStencilStates depthStencil = renderable.DepthStencil;
			GorgonRenderable.BlendState blending = renderable.Blending;
			GorgonRenderable.TextureSamplerState sampler = renderable.TextureSampler;

			if (renderable.Texture != _texture)
			{
				result |= StateChange.Texture;

				if (((_shaders.PixelShader == _shaders.DefaultPixelShaderDiffuse) && (renderable.Texture != null)) ||
					(_shaders.PixelShader ==_shaders.DefaultPixelShaderTextured) && (renderable.Texture == null))
					result |= StateChange.Shader;
			}

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

			if (_gorgon2D.IsAlphaTestEnabled != _shaders.IsAlphaTestEnabled)
				result |= StateChange.AlphaTestEnable;

			if ((_gorgon2D.IsAlphaTestEnabled) && (renderable.AlphaTestValues != _shaders.AlphaTestValue))
				result |= StateChange.AlphaTestValue;

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
				_texture = _shaders.PixelShader.Textures[0] = renderable.Texture;
			
			if ((state & StateChange.Shader) == StateChange.Shader)
			{
				// If we're using the default shader, switch between the default no texture or textured pixel shader depending on our state.
				if (renderable.Texture != null)
					_shaders.PixelShader = _shaders.DefaultPixelShaderTextured;
				else
					_shaders.PixelShader = _shaders.DefaultPixelShaderDiffuse;
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
				_shaders.PixelShader.Samplers[0] = _samplerState;
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

			if ((state & StateChange.AlphaTestEnable) == StateChange.AlphaTestEnable)
				_shaders.IsAlphaTestEnabled = _gorgon2D.IsAlphaTestEnabled;

			if ((state & StateChange.AlphaTestValue) == StateChange.AlphaTestValue)
				_shaders.AlphaTestValue = renderable.AlphaTestValues;

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
			_texture = _shaders.PixelShader.Textures[0];
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
			_shaders = _gorgon2D.Shaders;
		}
		#endregion
	}
}
