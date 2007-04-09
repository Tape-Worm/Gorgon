#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Tuesday, July 19, 2005 1:45:21 AM
// 
#endregion

using System;
using SharpUtilities;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Abstract class for a buffer object.
	/// </summary>
	/// <remarks>
	/// Buffers are typically used to store the content data.  Vertex and Index buffers will
	/// derive from here.
	/// </remarks>
	public abstract class DataBuffer : IDisposable
	{
		#region Variables.
		/// <summary>Size of the buffer in bytes.</summary>
		protected int _size;
		/// <summary>Pointer returned when buffer is locked.</summary>
		protected IntPtr _pointer;
		/// <summary>Offset of locked data in the buffer.</summary>
		protected int _lockStart;
		/// <summary>Amount of data locked.</summary>
		protected int _lockSize;
		/// <summary>Usage flags for this buffer (these may or may not apply depending on the buffer type).</summary>
		protected BufferUsage _usage;
		/// <summary>Flag to indicate that the buffer is locked.</summary>
		protected bool _locked;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to check and see if a buffer is locked or not.
		/// </summary>
		public bool BufferIsLocked
		{
			get
			{
				return _locked;
			}
		}

		/// <summary>
		/// Property to return the size of the buffer in bytes.
		/// </summary>
		public int BufferSize
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the usage flags for this buffer.
		/// </summary>
		public BufferUsage BufferUsage
		{
			get
			{
				return _usage;
			}
			set
			{
				_usage = value;
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the offset of the lock in bytes.
		/// </summary>
		public virtual int LockOffset
		{
			get
			{
				return _lockStart;
			}
		}

		/// <summary>
		/// Property to return the length of the locked area in bytes.
		/// </summary>
		public virtual int LockLength
		{
			get
			{
				return _lockSize;
			}
		}

		/// <summary>
		/// Property to return the pointer returned by the lock.
		/// </summary>
		public IntPtr LockPointer
		{
			get
			{
				if (!_locked)
					throw new NotLockedException(GetType(), null);

				return _pointer;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to lock the buffer.
		/// </summary>
		/// <param name="offset">Offset in bytes to start lock at.</param>
		/// <param name="length">Length of data to lock in bytes.</param>
		/// <param name="flags">Locking flags.</param>
		protected abstract void LockBuffer(int offset, int length, BufferLockFlags flags);

		/// <summary>
		/// Function to (re)initialize the buffer.  
		/// This will destroy all data contained within the buffer.
		/// </summary>
		public abstract void Refresh();

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		public abstract void Unlock();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="bufferusage">Usage flags for the buffer.</param>
		public DataBuffer(BufferUsage bufferusage)
		{
			_size = 0;
			_lockStart = -1;
			_lockSize = 0;			
			_locked = false;
			_pointer = IntPtr.Zero;
			_usage = bufferusage;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~DataBuffer()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected abstract void Dispose(bool disposing);

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
