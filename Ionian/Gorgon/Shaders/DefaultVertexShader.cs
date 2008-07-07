#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
