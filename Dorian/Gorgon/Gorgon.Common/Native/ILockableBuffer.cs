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
// Created: Thursday, September 22, 2011 12:23:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Native
{
	/// <summary>
	/// Flags for lockable buffers.
	/// </summary>
	public enum LockFlags
	{
		/// <summary>
		/// No flags.  Used with static buffers.
		/// </summary>
		None = 0,
		/// <summary>
		/// Buffer will be read-only when locked.
		/// </summary>
		ReadOnly = 1,
		/// <summary>
		/// Guarantees that the locked area is not overwriting data in the buffer. 
		/// </summary>
		NoOverwrite = 2,
		/// <summary>
		/// Discards the contents of the entire buffer.  Used with dynamic buffers.
		/// </summary>
		Discard = 3
	}

	/// <summary>
	/// Defines a buffer that must be locked before writing/reading.
	/// </summary>
	public interface ILockableBuffer
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return whether the buffer is locked or not.
		/// </summary>
		bool IsLocked
		{
			get;
		}

		/// <summary>
		/// Property to return the offset into the locked region, in bytes.
		/// </summary>
		int LockOffset
		{
			get;
		}

		/// <summary>
		/// Property to return the size of the lock, in bytes.
		/// </summary>
		int LockSize
		{
			get;
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		int Size
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to lock the buffer for reading/writing.
		/// </summary>
		/// <param name="offset">Offset into the buffer to start writing at, in bytes.</param>
		/// <param name="size">Number of bytes to write.</param>
		/// <param name="flags">Flags for the type of lock to perform.</param>
		/// <returns>A <see cref="GorgonLibrary.Native.GorgonDataStream">GorgonDataStream</see> object.</returns>
		/// <remarks>Use the resulting data stream object to access the data contained within the buffer.</remarks>
		GorgonDataStream Lock(int offset, int size, LockFlags flags);

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		void Unlock();
		#endregion
	}
}
