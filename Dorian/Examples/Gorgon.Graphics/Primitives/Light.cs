#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 11:43:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Native;
using SlimMath;

namespace GorgonLibrary.Graphics.Example
{
	/// <summary>
	/// A light to shine on our sad primitives
	/// </summary>
	class Light
		: IDisposable
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The constant buffer to update.
		private readonly GorgonConstantBuffer _buffer;
		// Buffer data.
		private readonly GorgonDataStream _bufferData;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the world matrix.
		/// </summary>
		/// <param name="position">The new position of the light.</param>
		public void UpdateLightPosition(ref Vector3 position)
		{
			unsafe
			{
				var data = (byte*)_bufferData.UnsafePointer;

				DirectAccess.WriteValue(data + Vector4.SizeInBytes, ref position);

				_buffer.Update(_bufferData);
			}
		}
		#endregion

		#region Constructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="WorldViewProjection"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface to use.</param>
		public Light(GorgonGraphics graphics)
		{
			_buffer = graphics.Buffers.CreateConstantBuffer("LightBuffer",
			                                                new GorgonConstantBufferSettings
			                                                {
				                                                SizeInBytes = 32,
				                                                Usage = BufferUsage.Default
			                                                });

			_bufferData = new GorgonDataStream(_buffer.SizeInBytes);

			unsafe
			{
				var data = (byte *)_bufferData.UnsafePointer;

				GorgonColor color = GorgonColor.White;
				Vector4 position = Vector4.Zero;

				DirectAccess.WriteValue(data, ref color);
				DirectAccess.WriteValue((data + Vector4.SizeInBytes), ref position);
			}

			_buffer.Update(_bufferData);

			graphics.Shaders.PixelShader.ConstantBuffers[0] = _buffer;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_bufferData != null)
				{
					_bufferData.Dispose();
				}

				if (_buffer != null)
				{
					_buffer.Dispose();
				}
			}

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
