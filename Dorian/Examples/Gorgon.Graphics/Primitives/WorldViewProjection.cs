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
// Created: Thursday, August 7, 2014 10:29:17 PM
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
	/// The world/view/projection matrix.
	/// </summary>
	class WorldViewProjection
		: IDisposable
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The constant buffer to update.
		private readonly GorgonConstantBuffer _buffer;
		// Buffer data.
		private readonly GorgonDataStream _bufferData;
		// Size of a matrix, in bytes.
		private readonly int _matrixSize = DirectAccess.SizeOf<Matrix>();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the world matrix.
		/// </summary>
		/// <param name="world">World matrix to update.</param>
		public void UpdateWorldMatrix(ref Matrix world)
		{
			unsafe
			{
				var data = (byte*)_bufferData.UnsafePointer;

				DirectAccess.WriteValue(data, ref world);

				_buffer.Update(_bufferData);
			}
		}

		/// <summary>
		/// Function to update the screen width/screen height.
		/// </summary>
		/// <param name="fov">Field of view, in degrees.</param>
		/// <param name="screenWidth">Width of the screen.</param>
		/// <param name="screenHeight">Height of the screen.</param>
		/// <param name="nearZ">Near Z plane.</param>
		/// <param name="farZ">Far Z plane.</param>
		public void UpdateProjection(float fov, float screenWidth, float screenHeight, float nearZ = 0.25f, float farZ = 1000.0f)
		{
			float aspect = screenWidth / screenHeight;
			Matrix projection;

			Matrix.PerspectiveFovLH(fov.Radians(), aspect, nearZ, farZ, out projection);

			unsafe
			{
				var data = (byte*)_bufferData.UnsafePointer;

				DirectAccess.WriteValue(data + (_matrixSize * 2), ref projection);

				_buffer.Update(_bufferData);
			}
		}
		#endregion

		#region Constructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="WorldViewProjection"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface to use.</param>
		public WorldViewProjection(GorgonGraphics graphics)
		{
			Matrix dummy = Matrix.Identity;

			_buffer = graphics.Buffers.CreateConstantBuffer("WVPBuffer",
			                                                new GorgonConstantBufferSettings
			                                                {
				                                                SizeInBytes = _matrixSize * 3,
				                                                Usage = BufferUsage.Default
			                                                });

			_bufferData = new GorgonDataStream(_buffer.SizeInBytes);

			unsafe
			{
				var data = (byte *)_bufferData.UnsafePointer;

				DirectAccess.WriteValue(data, ref dummy);
				DirectAccess.WriteValue((data + _matrixSize), ref dummy);
				DirectAccess.WriteValue((data + (_matrixSize * 2)), ref dummy);
			}

			_buffer.Update(_bufferData);

			graphics.Shaders.VertexShader.ConstantBuffers[0] = _buffer;
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
