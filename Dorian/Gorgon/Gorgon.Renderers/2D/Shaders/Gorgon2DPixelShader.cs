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
// Created: Monday, April 02, 2012 12:08:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A pixel shader state for the 2D renderer.
	/// </summary>
	public class Gorgon2DPixelShaderState
		: GorgonPixelShaderState
	{
		#region Variables.
		private Gorgon2D _gorgon2D = null;				// The 2D interface that owns this object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default diffuse pixel shader with alpha testing.
		/// </summary>
		internal GorgonPixelShader DefaultPixelShaderDiffuse
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default textured pixel shader with alpha testing.
		/// </summary>
		internal GorgonPixelShader DefaultPixelShaderTextured
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to set or return the current pixel shader.
		/// </summary>
		public override GorgonPixelShader Current
		{
			get
			{
				if ((Graphics.Shaders.PixelShader.Current == DefaultPixelShaderTextured) || (Graphics.Shaders.PixelShader.Current == DefaultPixelShaderDiffuse))
					return null;

				return Graphics.Shaders.PixelShader.Current;
			}
			set
			{
				if ((Graphics.Shaders.PixelShader.Current != value) || ((value == null) && (Graphics.Shaders.PixelShader.Current != DefaultPixelShaderTextured) && (Graphics.Shaders.PixelShader.Current != DefaultPixelShaderDiffuse)))
				{
					_gorgon2D.RenderObjects();

					if (value == null)
					{
						// If we have a texture in the first slot, then set the proper shader.
						if (Graphics.Shaders.PixelShader.Textures[0] == null)
							Graphics.Shaders.PixelShader.Current = DefaultPixelShaderDiffuse;
						else
							Graphics.Shaders.PixelShader.Current = DefaultPixelShaderTextured;
					}
					else
						Graphics.Shaders.PixelShader.Current = value;

					// Assign buffers.
					Graphics.Shaders.PixelShader.ConstantBuffers[0] = _gorgon2D.ProjectionViewBuffer;
					Graphics.Shaders.PixelShader.ConstantBuffers[1] = (_gorgon2D.IsAlphaTestEnabled ? _gorgon2D.AlphaTestBuffer : null);
				}
			}
		}

		/// <summary>
		/// Property to return the list of constant buffers for the pixel shader.
		/// </summary>
		public override GorgonShaderState<GorgonPixelShader>.ShaderConstantBuffers ConstantBuffers
		{
			get
			{
				return Graphics.Shaders.PixelShader.ConstantBuffers;
			}
		}

		/// <summary>
		/// Property to return the sampler states.
		/// </summary>
		/// <remarks>On a SM2_a_b device, and while using a Vertex Shader, setting a sampler will raise an exception.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the current video device is a SM2_a_b device.</exception>
		public override GorgonShaderState<GorgonPixelShader>.TextureSamplerState TextureSamplers
		{
			get
			{
				return Graphics.Shaders.PixelShader.TextureSamplers;
			}
		}

		/// <summary>
		/// Property to return the list of textures for the shaders.
		/// </summary>
		/// <remarks>On a SM2_a_b device, and while using a Vertex Shader, setting a texture will raise an exception.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the current video device is a SM2_a_b device.</exception>
		public override GorgonShaderState<GorgonPixelShader>.ShaderTextures Textures
		{
			get
			{
				return Graphics.Shaders.PixelShader.Textures;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when a texture is switched.
		/// </summary>
		/// <param name="texture">Texture to switch to.</param>
		internal void TextureSwitch(GorgonTexture texture)
		{
			// If we have a texture change, and we have the default diffuse shader loaded, then switch to the textured shader.
			if ((texture != null) && (Graphics.Shaders.PixelShader.Current == DefaultPixelShaderDiffuse))
				Current = DefaultPixelShaderTextured;

			// If we have a texture change, and we have the default textured shader loaded, then switch to the diffuse shader.
			if ((texture == null) && (Graphics.Shaders.PixelShader.Current == DefaultPixelShaderTextured))
				Current = DefaultPixelShaderDiffuse;
		}

		/// <summary>
		/// Function to perform clean up on the shader state.
		/// </summary>
		internal void CleanUp()
		{
			if (DefaultPixelShaderDiffuse != null)
				DefaultPixelShaderDiffuse.Dispose();
			if (DefaultPixelShaderTextured != null)
				DefaultPixelShaderTextured.Dispose();

			DefaultPixelShaderTextured = null;
			DefaultPixelShaderDiffuse = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DPixelShaderState"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal Gorgon2DPixelShaderState(Gorgon2D gorgon2D)
			: base(gorgon2D.Graphics)
		{
			_gorgon2D = gorgon2D;

#if DEBUG
			DefaultPixelShaderDiffuse = Graphics.Shaders.CreateShader<GorgonPixelShader>("Default_Basic_Pixel_Shader_Diffuse", "GorgonPixelShaderDiffuse", "#GorgonInclude \"Gorgon2DShaders\"", true);
			DefaultPixelShaderTextured = Graphics.Shaders.CreateShader<GorgonPixelShader>("Default_Basic_Pixel_Shader_Texture", "GorgonPixelShaderTextured", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			DefaultPixelShaderDiffuse = Graphics.Shaders.CreateShader<GorgonPixelShader>("Default_Basic_Pixel_Shader_Diffuse", "GorgonPixelShaderDiffuse", "#GorgonInclude \"Gorgon2DShaders\"", false);
			DefaultPixelShaderTextured = Graphics.Shaders.CreateShader<GorgonPixelShader>("Default_Basic_Pixel_Shader_Texture", "GorgonPixelShaderTextured", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif
		}
		#endregion
	}
}
