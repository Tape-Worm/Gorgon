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
	/// A default vertex shader that will enable Gorgon to render in 2D mode.
	/// </summary>
	internal class DefaultVertexShader
		: VertexShader
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
				CompileShader("vMain", Gorgon.CurrentDriver.VertexShaderVersion, ShaderCompileOptions.OptimizationLevel3);
#else
				CompileShader("vMain", Gorgon.CurrentDriver.VertexShaderVersion, ShaderCompileOptions.Debug);
#endif
			}

			// Automatically send the default projection matrix for the current render target.
			Parameters["proj"].SetValue(Gorgon.CurrentRenderTarget.ProjectionMatrix);

			base.OnRenderBegin();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultVertexShader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		internal DefaultVertexShader(string name)
			: base(name)
		{
			ShaderCache.Shaders.Remove(name);

			ShaderSource = "float4x4 proj;" +
							"struct VTX_OUT " +
							"{ " +
							"float4 pos : POSITION; " +
							"float2 uv : TEXCOORD; " +
							"float4 diffuse : COLOR; " +
							"};" +
							"VTX_OUT vMain(float4 pos : POSITION, float2 uv : TEXCOORD, float4 diffuse : COLOR) " +
							"{ " +
							"VTX_OUT vtx; " +
							"vtx.pos = float4(mul(pos, proj).xyz, 1.0f); " +
							"vtx.uv = uv; " +
							"vtx.diffuse = diffuse; " +
							"return vtx;" +
							"}";
			
#if !DEBUG
			CompileShader("vMain", Gorgon.CurrentDriver.VertexShaderVersion, ShaderCompileOptions.OptimizationLevel3);
#else
			CompileShader("vMain", Gorgon.CurrentDriver.VertexShaderVersion, ShaderCompileOptions.Debug);
#endif
		}
		#endregion
	}
}
