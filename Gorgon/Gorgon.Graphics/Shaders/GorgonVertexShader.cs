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
// Created: Thursday, December 15, 2011 9:43:36 AM
// 
#endregion

using SharpDX.D3DCompiler;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A vertex shader object.
	/// </summary>
	public class GorgonVertexShader
		: GorgonShader
	{
		#region Variables.
		private bool _disposed;					// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D vertex shader.
		/// </summary>
		internal D3D.VertexShader D3DShader
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
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

		    D3DShader = new D3D.VertexShader(Graphics.D3DDevice, byteCode)
			{
			    DebugName = "Gorgon Vertex Shader '" + Name + "'"
			};
		}

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
				    if (Graphics.Shaders.VertexShader.Current == this)
				    {
				        Graphics.Shaders.VertexShader.Current = null;
				    }

				    if (D3DShader != null)
				    {
				        D3DShader.Dispose();
				    }
				}

				D3DShader = null;

				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexShader"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this vertex shader.</param>
		/// <param name="name">The name of the vertex shader.</param>
		/// <param name="entryPoint">Entry point for the vertex shader.</param>
		internal GorgonVertexShader(GorgonGraphics graphics, string name, string entryPoint)
			: base(graphics, name, ShaderType.Vertex, entryPoint)	
		{

		}
		#endregion
	}
}
