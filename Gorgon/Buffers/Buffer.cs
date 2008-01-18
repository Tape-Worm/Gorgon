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
using System.IO;
using DX = SlimDX;

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
		private int _size;							// Size of the buffer in bytes.
		private BufferUsages _usage;				// Usage flags for this buffer (these may or may not apply depending on the buffer type).
		private int _lockStart;						// Offset of locked data in the buffer.
		private int _lockSize;						// Amount of data locked.
		private bool _locked;						// Flag to indicate that the buffer is locked.
		private DX.DataStream _lockStream = null;	// Stream containing locked data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to check and see if a buffer is locked or not.
		/// </summary>
		public bool IsLocked
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
		public BufferUsages BufferUsage
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the datastream for a locked object.
		/// </summary>
		/// <param name="lockFlags">Flags used in locking.</param>
		/// <returns>A data stream for a locked buffer.</returns>
		protected abstract DX.DataStream GetDataStream(DX.Direct3D9.LockFlags lockFlags);

		/// <summary>
		/// Function to lock the buffer.
		/// </summary>
		/// <param name="offset">Offset in bytes to start lock at.</param>
		/// <param name="length">Length of data to lock in bytes.</param>
		/// <param name="flags">Locking flags.</param>
		protected void LockBuffer(int offset, int length, BufferLockFlags flags)
		{
			if (offset + length > BufferSize)
				throw new OverflowException("The buffer length is smaller than " + Convert.ToString(offset + length) + ".");

			// Only allow a complete discard if the buffer is static.
			if (((BufferUsage & BufferUsages.Static) != 0) && (flags != BufferLockFlags.Normal))
				flags = BufferLockFlags.Normal;

			try
			{
				// Unlock if the buffer is locked.
				if (IsLocked)
					Unlock();

				_lockStart = offset;
				_lockSize = length;

				_lockStream = GetDataStream(Converter.Convert(flags));

				_locked = true;
			}
			catch (Exception ex)
			{
				_lockStart = 0;
				_lockSize = 0;
				throw new CannotLockException(GetType(), ex);
			}
		}

		/// <summary>
		/// Function to write a piece of data to the buffer.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="data">Data to write.</param>
		public virtual void Write<T>(T data) 
			where T : struct
		{
			if (!IsLocked)
				throw new NotLockedException(GetType());

			_lockStream.Write<T>(data);
		}

		/// <summary>
		/// Function to write an array of data to the buffer.
		/// </summary>
		/// <typeparam name="T">Type of data array.</typeparam>
		/// <param name="data">Data array to write.</param>
		public virtual void Write<T>(T[] data)
			where T : struct
		{
			if (!IsLocked)
				throw new NotLockedException(GetType());

			_lockStream.WriteRange<T>(data);
		}

		/// <summary>
		/// Function to write an array of data to the buffer.
		/// </summary>
		/// <typeparam name="T">Type of data array.</typeparam>
		/// <param name="data">Data array to write.</param>
		/// <param name="startIndex">Index to start writing from.</param>
		/// <param name="count">Number of indices to write.</param>
		public virtual void Write<T>(T[] data, int startIndex, int count)
			where T : struct
		{
			if (!IsLocked)
				throw new NotLockedException(GetType());

			_lockStream.WriteRange<T>(data, startIndex, count);
		}

		/// <summary>
		/// Function to write an array of bytes to the buffer.
		/// </summary>
		/// <param name="data">Byte array to write.</param>
		/// <param name="startIndex">Index to start writing from.</param>
		/// <param name="count">Number of indices to write.</param>
		public virtual void Write(byte[] data, int startIndex, int count)
		{
			if (!IsLocked)
				throw new NotLockedException(GetType());

			_lockStream.Write(data, startIndex, count);
		}

		/// <summary>
		/// Function to write data from a pointer to the buffer.
		/// </summary>
		/// <param name="pointer">The pointer to copy data from.</param>
		/// <param name="count">The number of bytes to write.</param>
		public virtual void Write(IntPtr pointer, int count)
		{
			if (!IsLocked)
				throw new NotLockedException(GetType());

			_lockStream.WriteRange(pointer, count);
		}

		/// <summary>
		/// Function to read a piece of data from the buffer.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <returns>Data in the buffer.</returns>
		public virtual T Read<T>()
			where T : struct
		{
			if (!IsLocked)
				throw new NotLockedException(GetType());

			if ((BufferUsage & BufferUsages.WriteOnly) == BufferUsages.WriteOnly)
				throw new CannotReadException("Cannot read from a write-only buffer.");

			return _lockStream.Read<T>();
		}

		/// <summary>
		/// Function to read an array of data from the buffer.
		/// </summary>
		/// <typeparam name="T">Type of data array.</typeparam>
		/// <param name="count">Number of items to read.</param>
		/// <returns>An array of data from the buffer.</returns>
		public virtual T[] Read<T>(int count)
			where T : struct
		{
			if (!IsLocked)
				throw new NotLockedException(GetType());

			if ((BufferUsage & BufferUsages.WriteOnly) == BufferUsages.WriteOnly)
				throw new CannotReadException("Cannot read from a write-only buffer.");

			return _lockStream.ReadRange<T>(count);
		}

		/// <summary>
		/// Function to read an array of byte data from the stream.
		/// </summary>
		/// <param name="data">Array to hold the byte data.</param>
		/// <param name="offset">Starting position in the stream to read from.</param>
		/// <param name="count">Number of bytes to read.</param>
		/// <returns>The number of bytes actually read.</returns>
		public virtual int Read(byte[] data, int offset, int count)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			if (!IsLocked)
				throw new NotLockedException(GetType());

			if ((BufferUsage & BufferUsages.WriteOnly) == BufferUsages.WriteOnly)
				throw new CannotReadException("Cannot read from a write-only buffer.");

			return _lockStream.Read(data, offset, count);
		}

		/// <summary>
		/// Function to (re)initialize the buffer.  
		/// This will destroy all data contained within the buffer.
		/// </summary>
		public abstract void Refresh();

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		public virtual void Unlock()
		{
			if (!_locked)
				throw new NotLockedException(GetType(), null);

			_lockStream.Dispose();
			_lockStream = null;

			_lockStart = -1;
			_lockSize = 0;
			_locked = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="bufferusage">Usage flags for the buffer.</param>
		protected DataBuffer(BufferUsages bufferusage)
		{
			_lockStart = -1;
			_usage = bufferusage;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Clean up the locked stream.
				if (_lockStream != null)
					_lockStream.Dispose();
				_lockStream = null;
			}
		}

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
