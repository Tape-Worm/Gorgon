#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Sunday, July 06, 2008 2:54:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A default pixel shader that will just perform a pass through on the pixels.
	/// </summary>
	internal class DefaultPixelShader
		: PixelShader
	{
		#region Methods.
		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			if (!IsCompiled)
			{
#if !DEBUG
				CompileShader("pMain", Gorgon.CurrentDriver.PixelShaderVersion, ShaderCompileOptions.OptimizationLevel3);
#else
				CompileShader("pMain", Gorgon.CurrentDriver.PixelShaderVersion, ShaderCompileOptions.Debug);
#endif
			}

			// Default to the currently set texture.
			Parameters["defaultTexture"].SetValue(Gorgon.Renderer.GetImage(0));

			base.OnRenderBegin();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultVertexShader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		internal DefaultPixelShader(string name)
			: base(name)
		{
			ShaderCache.Shaders.Remove(name);

			ShaderSource = "sampler2D defaultTexture;" +
							"float4 pMain(float2 uv : TEXCOORD) : COLOR " +
							"{ " +
							"return tex2D(defaultTexture, uv); " +
							"}";

#if !DEBUG
			CompileShader("pMain", Gorgon.CurrentDriver.PixelShaderVersion, ShaderCompileOptions.OptimizationLevel3);
#else
			CompileShader("pMain", Gorgon.CurrentDriver.PixelShaderVersion, ShaderCompileOptions.Debug);
#endif
		}
		#endregion
	}
}
