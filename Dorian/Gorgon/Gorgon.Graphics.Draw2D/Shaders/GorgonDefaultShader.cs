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
// Created: Thursday, February 16, 2012 7:23:45 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// Default shaders for Gorgon.
	/// </summary>
	class GorgonDefaultShader
		: Gorgon2DShader
	{
		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the object was disposed.
		private GorgonVertexShader _defaultVertex;					// Property to set or return the default vertex shader.
		private GorgonPixelShader _defaultPixelDiffuse;				// Property to set or return the default pixel diffuse shader.
		private GorgonPixelShader _defaultPixelTextured;			// The default pixel texture shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current pixel shader.
		/// </summary>
		internal override GorgonPixelShader PixelShader
		{
			get
			{
				// Check to see if any textures are bound.
				if (Gorgon2D.Graphics.Shaders.PixelShader != null)
				{
					for (int i = 0; i < Gorgon2D.Graphics.Shaders.PixelShader.Textures.Count; i++)
					{
						if (Gorgon2D.Graphics.Shaders.PixelShader.Textures[i] != null)
							return _defaultPixelTextured;
					}
				}

				return _defaultPixelDiffuse;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		/// <value></value>
		internal override GorgonVertexShader VertexShader
		{
			get
			{
				return _defaultVertex;
			}
			set
			{
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_defaultVertex != null)
						_defaultVertex.Dispose();
					if (_defaultPixelTextured != null)
						_defaultPixelTextured.Dispose();
					if (_defaultPixelDiffuse != null)
						_defaultPixelDiffuse.Dispose();
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDefaultShader"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		public GorgonDefaultShader(Gorgon2D gorgon2D)
			: base(gorgon2D, "DefaultVertexPixelShaders")
		{
			string shaderSource = Encoding.UTF8.GetString(Properties.Resources.BasicSprite);			

			// Create default shaders.
			_defaultVertex = gorgon2D.Graphics.Shaders.CreateVertexShader("Default_Basic_Vertex_Shader", "GorgonVertexShader", shaderSource);
			_defaultPixelTextured = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_Texture", "GorgonPixelShaderTexture", shaderSource);
			_defaultPixelDiffuse = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_No_Texture", "GorgonPixelShaderNoTexture", shaderSource);
		}
		#endregion
	}
}
