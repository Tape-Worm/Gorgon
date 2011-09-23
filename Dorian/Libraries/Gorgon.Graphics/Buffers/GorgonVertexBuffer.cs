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
// Created: Thursday, September 22, 2011 11:22:24 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Flags for vertex/index buffer usage.
	/// </summary>
	[Flags()]
	public enum VertexIndexBufferUsage
	{
		/// <summary>
		/// Default usage.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Buffer can only be written to.
		/// </summary>
		WriteOnly = 1,
	}
	
	/// <summary>
	/// Buffer to hold vertex information.
	/// </summary>
	public abstract class GorgonVertexBuffer
		: ILockableBuffer
	{
		#region Variables.
		private GorgonDataStream _lock = null;			// Locked data stream.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the usage for this buffer.
		/// </summary>
		public VertexIndexBufferUsage Usage
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the buffer is dynamic or static.
		/// </summary>
		public bool IsDynamic
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the buffer does the clipping or not.
		/// </summary>
		public bool IsClipping
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform the lock on the vertex buffer.
		/// </summary>
		/// <param name="offset">Offset, in bytes, within the buffer to start the lock.</param>
		/// <param name="size">Size, in bytes, to lock.</param>
		/// <param name="flags">Flag to apply to the lock.</param>
		/// <returns>A data stream containing the locked buffer data.</returns>
		protected abstract GorgonDataStream LockImpl(int offset, int size, LockFlags flags);

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		protected abstract void UnlockImpl();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBuffer"/> class.
		/// </summary>
		/// <param name="size">The size of the buffer, in bytes.</param>
		/// <param name="usage">The usage scheme for this buffer.</param>
		/// <param name="dynamic">TRUE to use a dynamic buffer, FALSE to use a static buffer.</param>
		/// <param name="preClipped">TRUE if the data being put into the buffer is already clipped, FALSE if not.</param>
		protected GorgonVertexBuffer(int size, VertexIndexBufferUsage usage, bool dynamic, bool preClipped)
		{
			Usage = usage;
			IsDynamic = dynamic;
			IsClipping = !preClipped;
		}
		#endregion

		#region ILockableBuffer Members
		#region Properties.
		/// <summary>
		/// Property to return whether the buffer is locked or not.
		/// </summary>
		public bool IsLocked
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the offset into the locked region, in bytes.
		/// </summary>
		public int LockOffset
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of the lock, in bytes.
		/// </summary>
		public int LockSize
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to lock the buffer for reading/writing.
		/// </summary>
		/// <param name="offset">Offset into the buffer to start writing at, in bytes.</param>
		/// <param name="size">Number of bytes to write.</param>
		/// <param name="flags">Flags for the type of lock to perform.</param>
		/// <returns>
		/// A <see cref="GorgonLibrary.Native.GorgonDataStream">GorgonDataStream</see> object.
		/// </returns>
		/// <remarks>When the buffer is locked, the user must call the <see cref="M:GorgonLibrary.Graphics.GorgonVertexBuffer.Unlock">Unlock</see> method to release the buffer when finished reading/writing.
		/// <para></para>
		/// </remarks>
		public GorgonDataStream Lock(int offset, int size, LockFlags flags)
		{
			if (offset + size > Size)
				throw new ArgumentOutOfRangeException("The offset and the size cannot be larger than the total size of the buffer.");

			if (offset < 0)
				throw new ArgumentException("Cannot be less than zero.", "offset");
			if (size < 0)
				throw new ArgumentException("Cannot be less than zero.", "size");

			_lock = LockImpl(offset, size, flags);

			IsLocked = true;
			LockOffset = offset;
			LockSize = size;

			return _lock;
		}

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		public void Unlock()
		{
			if (!IsLocked)
				return;

			UnlockImpl();

			// Remove the buffer.
			if (_lock != null)
				_lock.Dispose();

			_lock = null;
			IsLocked = false;
			LockSize = 0;
			LockOffset = 0;
		}
		#endregion
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected abstract void Dispose(bool disposing);

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
