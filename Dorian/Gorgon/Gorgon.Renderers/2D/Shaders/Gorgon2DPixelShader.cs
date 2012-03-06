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
// Created: Friday, February 17, 2012 4:22:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A pixel shader for the Gorgon 2D interface.
	/// </summary>
	public class Gorgon2DPixelShader
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;																	// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the pixel shader.
		/// </summary>
		internal GorgonPixelShader Shader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the Gorgon2D interface that created this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the textures for the pixel shader.
		/// </summary>
		public GorgonShaderState<GorgonPixelShader>.ShaderTextures Textures
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture samplers for the pixel shader.
		/// </summary>
		public GorgonShaderState<GorgonPixelShader>.TextureSamplerState Samplers
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the constant buffer list for a pixel shader.
		/// </summary>
		public GorgonShaderState<GorgonPixelShader>.ShaderConstantBuffers ConstantBuffers
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DPixelShader"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		/// <param name="name">The name of the pixel shader.</param>
		internal Gorgon2DPixelShader(Gorgon2D gorgon2D, string name)
			: base(name)
		{
			Gorgon2D = gorgon2D;

			ConstantBuffers = Gorgon2D.Graphics.Shaders.PixelShader.ConstantBuffers;
			Textures = Gorgon2D.Graphics.Shaders.PixelShader.Textures;
			Samplers = Gorgon2D.Graphics.Shaders.PixelShader.TextureSamplers;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				Gorgon2D.TrackedObjects.Remove(this);

				if (Shader != null)
					Shader.Dispose();

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);	
		}
		#endregion
	}
}
