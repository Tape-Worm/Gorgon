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

namespace GorgonLibrary.Graphics.Renderers
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
		AlphaTestEnable = 2048
	}

	/// <summary>
	/// Used to manage the states between objects.
	/// </summary>
	class GorgonStateManager
	{
		#region Variables.
		private GorgonGraphics _graphics = null;
		private Gorgon2D _gorgon2D = null;
		private BlendType _sourceBlend = BlendType.Zero;
		private BlendType _destBlend = BlendType.Zero;
		private BlendType _sourceAlphaBlend = BlendType.Zero;
		private BlendType _destAlphaBlend = BlendType.Zero;
		private BlendOperation _alphaOp = BlendOperation.Add;
		private BlendOperation _blendOp = BlendOperation.Add;
		private GorgonVertexBufferBinding _vertexBuffer = default(GorgonVertexBufferBinding);
		private ColorWriteMaskFlags _writeMask = ColorWriteMaskFlags.All;
		private GorgonColor _blendFactor = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
		private GorgonColor _borderColor = new GorgonColor(0, 0, 0, 0);
		private TextureAddressing _uWrap = TextureAddressing.Clamp;
		private TextureAddressing _vWrap = TextureAddressing.Clamp;
		private TextureFilter _filter = TextureFilter.Point;
		private CullingMode _cullMode = CullingMode.Front;
		private bool _multiSamplingEnable = false;
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
		public StateChange CheckState(GorgonRenderable renderable)
		{
			StateChange result = StateChange.None;

			if (renderable.Texture != _texture)
			{
				result |= StateChange.Texture;

				if (((_shaders.PixelShader == _shaders.DefaultPixelShaderDiffuse) && (renderable.Texture != null)) ||
					(_shaders.PixelShader ==_shaders.DefaultPixelShaderTextured) && (renderable.Texture == null))
					result |= StateChange.Shader;
			}

			if (!renderable.BlendFactor.Equals(_blendFactor))
				result |= StateChange.BlendFactor;

			if ((renderable.SourceBlend != _sourceBlend) 
				|| (renderable.DestinationBlend != _destBlend) 
				|| (renderable.AlphaOperation != _alphaOp) 
				|| (renderable.BlendOperation != _blendOp)
				|| (renderable.SourceAlphaBlend != _sourceAlphaBlend) 
				|| (renderable.DestinationAlphaBlend != _destAlphaBlend) 
				|| (renderable.WriteMask != _writeMask))
				result |= StateChange.BlendState;

			if ((renderable.TextureFilter != _filter) 
				|| (renderable.VerticalWrapping != _vWrap)
				|| (renderable.HorizontalWrapping != _uWrap) 
				|| (!renderable.BorderColor.Equals(_borderColor)))
				result |= StateChange.Sampler;

			if ((renderable.CullingMode != _cullMode) 
				|| (_gorgon2D.UseMultisampling != _multiSamplingEnable))
				result |= StateChange.Raster;

			if (renderable.PrimitiveType != _graphics.Input.PrimitiveType)
				result |= StateChange.PrimitiveType;

			if (renderable.IndexBuffer != _graphics.Input.IndexBuffer)
				result |= StateChange.IndexBuffer;

			if (!renderable.VertexBufferBinding.Equals(_vertexBuffer))
				result |= StateChange.VertexBuffer;

			if (renderable.IsAlphaTestEnabled != _shaders.IsAlphaTestEnabled)
				result |= StateChange.AlphaTestEnable;

			if ((renderable.AlphaTestValues != _shaders.AlphaTestValue) && (renderable.IsAlphaTestEnabled))
				result |= StateChange.AlphaTestValue;

			return result;
		}

		/// <summary>
		/// Function to apply any state changes.
		/// </summary>
		/// <param name="renderable">Renderable object to retrieve states from.</param>
		/// <param name="state">States that need updating.</param>
		public void ApplyState(GorgonRenderable renderable, StateChange state)
		{
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

			if ((state & StateChange.BlendFactor) == StateChange.BlendFactor)
			{
				_blendFactor = renderable.BlendFactor;
				_graphics.Output.BlendingState.BlendFactor = renderable.BlendFactor;
			}

			if ((state & StateChange.BlendState) == StateChange.BlendState)
			{
				_sourceBlend = renderable.SourceBlend;
				_destBlend = renderable.DestinationBlend;
				_sourceAlphaBlend = renderable.SourceAlphaBlend;
				_destAlphaBlend = renderable.DestinationAlphaBlend;
				_alphaOp = renderable.AlphaOperation;
				_blendOp = renderable.BlendOperation;
				_writeMask = renderable.WriteMask;
				_graphics.Output.BlendingState.States = renderable.BlendingState;
			}

			if ((state & StateChange.Sampler) == StateChange.Sampler)
			{
				_uWrap = renderable.HorizontalWrapping;
				_vWrap = renderable.VerticalWrapping;
				_borderColor = renderable.BorderColor;
				_filter = renderable.TextureFilter;
				_shaders.PixelShader.Samplers[0] = renderable.SamplerState;
			}

			if ((state & StateChange.Raster) == StateChange.Raster)
			{
				_cullMode = renderable.CullingMode;
				_multiSamplingEnable = _gorgon2D.UseMultisampling;
				_graphics.Rasterizer.States = renderable.RasterizerStates;
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
				_shaders.IsAlphaTestEnabled = renderable.IsAlphaTestEnabled;

			if ((state & StateChange.AlphaTestValue) == StateChange.AlphaTestValue)
				_shaders.AlphaTestValue = renderable.AlphaTestValues;
		}

		/// <summary>
		/// Function to get the default states.
		/// </summary>
		public void GetDefaults()
		{
			_vertexBuffer = _graphics.Input.VertexBuffers[0];

			_sourceBlend = _graphics.Output.BlendingState.States.RenderTarget0.SourceBlend;
			_destBlend = _graphics.Output.BlendingState.States.RenderTarget0.DestinationBlend;
			_sourceAlphaBlend = _graphics.Output.BlendingState.States.RenderTarget0.SourceAlphaBlend;
			_destAlphaBlend = _graphics.Output.BlendingState.States.RenderTarget0.DestinationAlphaBlend;
			_alphaOp = _graphics.Output.BlendingState.States.RenderTarget0.AlphaOperation;
			_blendOp = _graphics.Output.BlendingState.States.RenderTarget0.BlendingOperation;
			_writeMask = _graphics.Output.BlendingState.States.RenderTarget0.WriteMask;
			_texture = _shaders.PixelShader.Textures[0];
			_borderColor = _shaders.PixelShader.Samplers[0].BorderColor;
			_uWrap = _shaders.PixelShader.Samplers[0].HorizontalAddressing;
			_vWrap = _shaders.PixelShader.Samplers[0].VerticalAddressing;
			_filter = _shaders.PixelShader.Samplers[0].TextureFilter;
			_cullMode = _graphics.Rasterizer.States.CullingMode;
			_multiSamplingEnable = _graphics.Rasterizer.States.IsMultisamplingEnabled;
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
