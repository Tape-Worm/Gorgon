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
