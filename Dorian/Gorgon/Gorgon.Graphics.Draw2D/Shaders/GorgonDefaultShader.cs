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
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the default vertex shader.
		/// </summary>
		public GorgonVertexShader DefaultVertex
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default pixel diffuse shader.
		/// </summary>
		public GorgonPixelShader DefaultPixelDiffuse
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default pixel texture shader.
		/// </summary>
		public GorgonPixelShader DefaultPixelTextured
		{
			get;
			private set;
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
					if (DefaultVertex != null)
						DefaultVertex.Dispose();
					if (DefaultPixelTextured != null)
						DefaultPixelTextured.Dispose();
					if (DefaultPixelDiffuse != null)
						DefaultPixelDiffuse.Dispose();
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to set the default shaders.
		/// </summary>
		public void SetDefault()
		{
			VertexShader = DefaultVertex;
			PixelShader = DefaultPixelDiffuse;
			Gorgon2D.Shaders.UpdateGorgonTransformation();
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
			DefaultVertex = gorgon2D.Graphics.Shaders.CreateVertexShader("Default_Basic_Vertex_Shader", "GorgonVertexShader", shaderSource);
			DefaultPixelTextured = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_Texture", "GorgonPixelShaderTexture", shaderSource);
			DefaultPixelDiffuse = gorgon2D.Graphics.Shaders.CreatePixelShader("Default_Basic_Pixel_Shader_No_Texture", "GorgonPixelShaderNoTexture", shaderSource);
		}
		#endregion
	}
}
