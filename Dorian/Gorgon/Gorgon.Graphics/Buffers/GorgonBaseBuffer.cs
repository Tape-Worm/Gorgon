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
// Created: Monday, January 09, 2012 7:06:42 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Native;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Flags for buffer access.
	/// </summary>
	[Flags()]
	public enum BufferAccessFlags
	{
		/// <summary>
		/// Buffer is readable.
		/// </summary>
		CanRead = 1,
		/// <summary>
		/// Buffer is writeable.
		/// </summary>
		CanWrite = 2,
		/// <summary>
		/// Buffer can be accessed by the CPU.
		/// </summary>
		AllowCPU = 4
	}

	/// <summary>
	/// Flags used when locking the buffer for reading/writing.
	/// </summary>
	[Flags()]
	public enum BufferLockFlags
	{
		/// <summary>
		/// Lock the buffer for reading.
		/// </summary>
		Read = 1,
		/// <summary>
		/// Lock the buffer for writing.
		/// </summary>
		Write = 2,
		/// <summary>
		/// Discard the contents of the buffer when writing.
		/// </summary>
		Discard = 4
	}

	/// <summary>
	/// A base buffer object.
	/// </summary>
	public abstract class GorgonBaseBuffer
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that created this buffer.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the buffer is locked or not.
		/// </summary>
		public bool IsLocked
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the access flags for the buffer.
		/// </summary>
		public BufferAccessFlags BufferAccessFlags
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected internal abstract void Initialize(GorgonDataStream data);

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>A data stream containing the buffer data.</returns>		
		protected abstract void LockBuffer(BufferLockFlags lockFlags);

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected internal abstract void UnlockBuffer();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create this object.</param>
		/// <param name="accessFlags">Flags used to access the buffer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected GorgonBaseBuffer(GorgonGraphics graphics, BufferAccessFlags accessFlags)			
		{
			GorgonDebug.AssertNull<GorgonGraphics>(graphics, "graphics");

			Graphics = graphics;
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
					Graphics.RemoveTrackedObject(this);

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
