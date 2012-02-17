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
// Created: Thursday, February 16, 2012 7:23:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A shader for the 2D renderer.
	/// </summary>
	public class Gorgon2DShader
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that this object has been disposed.
		private GorgonPixelShader _pixelShader = null;						// Current pixel shader.
		private GorgonVertexShader _vertexShader = null;					// Current vertex shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the 2D interface that owns this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current pixel shader.
		/// </summary>
		internal virtual GorgonPixelShader PixelShader
		{
			get
			{
				return _pixelShader;
			}
			set
			{
				if (value != _pixelShader)
				{
					_pixelShader = value;
					Gorgon2D.Graphics.Shaders.PixelShader.Current = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		internal virtual GorgonVertexShader VertexShader
		{
			get
			{
				return _vertexShader;
			}
			set
			{
				if (value != _vertexShader)
				{
					_vertexShader = value;
					Gorgon2D.Graphics.Shaders.VertexShader.Current = value;
				}
			}
		}

		/// <summary>
		/// Property to return the textures for the pixel shader.
		/// </summary>
		public GorgonShaderState<GorgonPixelShader>.ShaderTextures Textures
		{
			get
			{
				if (Gorgon2D.Graphics.Shaders.PixelShader == null)
					return null;

				return Gorgon2D.Graphics.Shaders.PixelShader.Textures;
			}
		}

		/// <summary>
		/// Property to return the texture samplers for the pixel shader.
		/// </summary>
		public GorgonShaderState<GorgonPixelShader>.TextureSamplerState Samplers
		{
			get
			{
				if (Gorgon2D.Graphics.Shaders.PixelShader == null)
					return null;

				return Gorgon2D.Graphics.Shaders.PixelShader.TextureSamplers;
			}
		}

		/// <summary>
		/// Property to return the constant buffer list for a pixel shader.
		/// </summary>
		public GorgonShaderState<GorgonPixelShader>.ShaderConstantBuffers PSConstantBuffers
		{
			get
			{
				if (Gorgon2D.Graphics.Shaders.PixelShader == null)
					return null;

				return Gorgon2D.Graphics.Shaders.PixelShader.ConstantBuffers;
			}
		}

		/// <summary>
		/// Property to return the constant buffer list for a vertex shader.
		/// </summary>
		public GorgonShaderState<GorgonVertexShader>.ShaderConstantBuffers VSConstantBuffers
		{
			get
			{
				if (Gorgon2D.Graphics.Shaders.VertexShader == null)
					return null;

				return Gorgon2D.Graphics.Shaders.VertexShader.ConstantBuffers;
			}
		}
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DShader"/> class.
		/// </summary>
		/// <param name="graphics2D">The 2D graphics interface that owns this object.</param>
		/// <param name="name">Name of the shader.</param>
		internal Gorgon2DShader(Gorgon2D graphics2D, string name)
			: base(name)
		{
			Gorgon2D = graphics2D;
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
				if (disposing)
				{
					Gorgon2D.TrackedObjects.Remove(this);
				}

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
