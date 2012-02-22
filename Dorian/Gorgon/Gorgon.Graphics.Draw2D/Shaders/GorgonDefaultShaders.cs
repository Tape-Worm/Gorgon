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
// Created: Friday, February 17, 2012 4:25:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A default pixel shader for diffuse only pixels with alpha testing.
	/// </summary>
	class GorgonDefaultPixelShaderDiffuse
		: Gorgon2DPixelShader
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDefaultPixelShaderDiffuse"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal GorgonDefaultPixelShaderDiffuse(Gorgon2D gorgon2D)
			: base(gorgon2D, "DefaultPixelShader_Diffuse_AlphaTest")
		{
			string shaderSource = Encoding.UTF8.GetString(Properties.Resources.BasicSprite);

#if DEBUG
			Shader = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_No_Texture_AlphaTest", "GorgonPixelShaderNoTextureAlphaTest", shaderSource, true);
#else
			Shader = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_No_Texture", "GorgonPixelShaderNoTextureAlphaTest", shaderSource, false);
#endif
		}
		#endregion
	}

	/// <summary>
	/// A default pixel shader for diffuse and textured pixels with alpha testing.
	/// </summary>
	class GorgonDefaultPixelShaderTextured
		: Gorgon2DPixelShader
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDefaultPixelShaderTextured"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal GorgonDefaultPixelShaderTextured(Gorgon2D gorgon2D)
			: base(gorgon2D, "DefaultPixelShader_Texture_AlphaTest")
		{
			string shaderSource = Encoding.UTF8.GetString(Properties.Resources.BasicSprite);

#if DEBUG
			Shader = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_Texture_AlphaTest", "GorgonPixelShaderTextureAlphaTest", shaderSource, true);
#else
			Shader = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_Texture", "GorgonPixelShaderTextureAlphaTest", shaderSource, false);
#endif
		}
		#endregion
	}

	/// <summary>
	/// A default vertex shader.
	/// </summary>
	class GorgonDefaultVertexShader
		: Gorgon2DVertexShader
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDefaultVertexShader"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal GorgonDefaultVertexShader(Gorgon2D gorgon2D)
			: base(gorgon2D, "DefaultVertexShader")
		{
			string shaderSource = Encoding.UTF8.GetString(Properties.Resources.BasicSprite);

#if DEBUG
			Shader = gorgon2D.Graphics.Shaders.CreateVertexShader("Default_Basic_Vertex_Shader", "GorgonVertexShader", shaderSource, true);
#else
			Shader = gorgon2D.Graphics.Shaders.CreateVertexShader("Default_Basic_Vertex_Shader", "GorgonVertexShader", shaderSource, false);
#endif
		}
		#endregion
	}
	
}
