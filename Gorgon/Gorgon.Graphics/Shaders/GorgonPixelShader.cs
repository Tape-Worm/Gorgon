#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, December 15, 2011 12:49:30 PM
// 
#endregion

using SharpDX.D3DCompiler;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A pixel shader object.
	/// </summary>
	public class GorgonPixelShader
		: GorgonShader
	{
		#region Variables.
		private bool _disposed;			// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D pixel shader.
		/// </summary>
		internal D3D.PixelShader D3DShader
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
					// Disassociate any shaders after we've destroyed them.
				    if (Graphics.Shaders.PixelShader.Current == this)
				    {
				        Graphics.Shaders.PixelShader.Current = null;
				    }

				    if (D3DShader != null)
				    {
				        D3DShader.Dispose();
				    }

				    D3DShader = null;
				}

			    _disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to compile the shader.
		/// </summary>
		/// <param name="byteCode">Byte code for the shader.</param>
		protected override void CreateShader(ShaderBytecode byteCode)
		{
		    if (D3DShader != null)
		    {
		        D3DShader.Dispose();
		    }

		    D3DShader = new D3D.PixelShader(Graphics.D3DDevice, byteCode)
			    {
			        DebugName = "Gorgon Pixel Shader '" + Name + "'"
			    };
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPixelShader"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		/// <param name="name">The name of the pixel shader.</param>
		/// <param name="entryPoint">The entry point method for the shader.</param>
		internal GorgonPixelShader(GorgonGraphics graphics, string name, string entryPoint)
			: base(graphics, name, ShaderType.Pixel, entryPoint)
		{
		}
		#endregion
	}
}
