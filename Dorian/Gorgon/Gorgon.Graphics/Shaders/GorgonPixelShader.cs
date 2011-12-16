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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A pixel shader object.
	/// </summary>
	public class GorgonPixelShader
		: GorgonShader
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object was disposed.
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
					if (Graphics.Shaders.PixelShader == this)
						Graphics.Shaders.PixelShader = null;

					if (D3DShader != null)
						D3DShader.Dispose();

					D3DShader = null;
				}
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to compile the shader.
		/// </summary>
		/// <param name="byteCode">Byte code for the shader.</param>
		protected override void CompileImpl(SharpDX.D3DCompiler.ShaderBytecode byteCode)
		{
			if (D3DShader != null)
				D3DShader.Dispose();

			D3DShader = new D3D.PixelShader(Graphics.VideoDevice.D3DDevice, byteCode, null);
			D3DShader.DebugName = "Gorgon Pixel Shader '" + Name + "'";			
		}

		/// <summary>
		/// Function to assign this shader and its states to the device.
		/// </summary>
		protected override void AssignImpl()
		{
			Graphics.Context.PixelShader.Set(D3DShader);
		}

		/// <summary>
		/// Function to apply a single constant buffer.
		/// </summary>
		/// <param name="slot">Slot to index.</param>
		/// <param name="buffer">Buffer to apply.</param>
		protected override void ApplyConstantBuffer(int slot, GorgonConstantBuffer buffer)
		{
			if (buffer != null)
				Graphics.Context.PixelShader.SetConstantBuffer(slot, buffer.D3DBuffer);
			else
				Graphics.Context.PixelShader.SetConstantBuffer(slot, null);
		}

		/// <summary>
		/// Function to apply multiple constant buffers.
		/// </summary>
		/// <param name="slot">Slot to index.</param>
		/// <param name="buffers">Buffers to apply.</param>
		protected override void ApplyConstantBuffers(int slot, IEnumerable<GorgonConstantBuffer> buffers)
		{
			var d3dbuffers = (from buffer in buffers
							  select (buffer == null ? null : buffer.D3DBuffer)).ToArray();

			Graphics.Context.PixelShader.SetConstantBuffers(slot, d3dbuffers);
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
