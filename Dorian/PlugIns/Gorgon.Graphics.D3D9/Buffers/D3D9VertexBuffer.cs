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
// Created: Thursday, September 22, 2011 1:03:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A Direct 3D9 vertex buffer.
	/// </summary>
	class D3D9VertexBuffer
		: GorgonVertexBuffer
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the buffer was disposed.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform the lock on the vertex buffer.
		/// </summary>
		/// <param name="offset">Offset, in bytes, within the buffer to start the lock.</param>
		/// <param name="size">Size, in bytes, to lock.</param>
		/// <param name="flags">Flag to apply to the lock.</param>
		/// <returns>
		/// A data stream containing the locked buffer data.
		/// </returns>
		protected override Native.GorgonDataStream LockImpl(int offset, int size, Native.LockFlags flags)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		protected override void UnlockImpl()
		{
			throw new NotImplementedException();
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
					Unlock();
				_disposed = true;
			}
		}
		#endregion
	}
}
