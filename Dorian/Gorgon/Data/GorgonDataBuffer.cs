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
// Created: Sunday, September 11, 2011 4:31:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Data
{
	/// <summary>
	/// A data buffer for holding arbitrary byte data.
	/// </summary>
	public class GorgonDataBuffer
		: IDisposable
	{
		#region Classes.
		/// <summary>
		/// A locked region of the buffer.
		/// </summary>
		/// <remarks>When locking a buffer, be sure to unlock it as soon as you're done.  Otherwise memory will remain pinned and the performance of the garbage collector will suffer.</remarks>
		public class LockRegion
			: IDisposable
		{
			#region Variables.
			private bool _disposed = false;					// Flag to indicate that the object was disposed.
			private GCHandle _handle = default(GCHandle);	// Handle for the locked region.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the data buffer that owns this lock.
			/// </summary>
			public GorgonDataBuffer Owner
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the size of the lock in bytes.
			/// </summary>
			public int LockSize
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the offset of the locked region within the actual data.
			/// </summary>
			public int LockOffset
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the pointer used for the lock.
			/// </summary>
			public IntPtr LockPointer
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return a byte within the locked region.
			/// </summary>
			public byte this[int index]
			{
				get
				{
					if ((index < 0) || (index > LockSize))
						throw new IndexOutOfRangeException();

					return Owner._data[index];
				}
				set
				{
					if ((index < 0) || (index > LockSize))
						throw new IndexOutOfRangeException();

					Owner._data[index] = value;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to read data from the buffer.
			/// </summary>
			/// <param name="data">Array to read data into.</param>
			/// <param name="bufferPosition">Position in the lock region to read from.</param>
			/// <param name="index">Index in the array to start writing into.</param>
			/// <param name="elementCount">Number of elements in the array.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> or the <paramref name="elementCount"/> parameters are less than zero.
			/// <para>-or-</para>
			/// <para>Thrown if the index + size is larger than the number of elements in the source array.</para>
			/// </exception>
			public void ReadRange(byte[] data, int bufferPosition, int index, int elementCount)
			{
				IntPtr pointer = LockPointer + bufferPosition;

				if (bufferPosition < 0)
					throw new ArgumentException("bufferPosition", "Buffer position cannot be less than zero.");
				if (bufferPosition + elementCount > LockSize)
					throw new ArgumentOutOfRangeException("bufferPosition + size", "Buffer position and size cannot be greater than the length of the buffer.");

				pointer.CopyTo(data, index, elementCount);
			}

			/// <summary>
			/// Function to read data from the buffer.
			/// </summary>
			/// <typeparam name="T">Type of data to read.</typeparam>
			/// <param name="data">Array to read data into.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			public void ReadRange(byte[] data)
			{
				if (data == null)
					throw new ArgumentNullException("data");

				ReadRange(data, 0, 0, data.Length);
			}

			/// <summary>
			/// Function to read data from the buffer.
			/// </summary>
			/// <typeparam name="T">Type of data to read.</typeparam>
			/// <param name="data">Array to read data into.</param>
			/// <param name="bufferPosition">Position in the lock region to read from.</param>
			/// <param name="index">Index in the array to start writing into.</param>
			/// <param name="elementCount">Number of elements in the array.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> or the <paramref name="elementCount"/> parameters are less than zero.
			/// <para>-or-</para>
			/// <para>Thrown if the index + size is larger than the number of elements in the source array.</para>
			/// </exception>
			public void ReadRange<T>(T[] data, int bufferPosition, int index, int elementCount)
				where T : struct
			{
				IntPtr pointer = LockPointer + bufferPosition;

				if (bufferPosition < 0)
					throw new ArgumentException("bufferPosition", "Buffer position cannot be less than zero.");
				if (bufferPosition + (elementCount * Marshal.SizeOf(typeof(T))) > LockSize)
					throw new ArgumentOutOfRangeException("bufferPosition + size", "Buffer position and size cannot be greater than the length of the buffer.");

				pointer.CopyTo<T>(data, index, elementCount * Marshal.SizeOf(typeof(T)));
			}

			/// <summary>
			/// Function to read data from the buffer.
			/// </summary>
			/// <typeparam name="T">Type of data to read.</typeparam>
			/// <param name="data">Array to read data into.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			public void ReadRange<T>(T[] data)
				where T : struct
			{
				if (data == null)
					throw new ArgumentNullException("data");

				ReadRange<T>(data, 0, 0, data.Length);
			}

			/// <summary>
			/// Function to write data into the buffer.
			/// </summary>
			/// <typeparam name="T">Type of data to read.</typeparam>
			/// <param name="bufferPosition">Position in the buffer to read from.</param>
			public T Read<T>(int bufferPosition)
				where T : struct
			{
				T[] result = new T[] {default(T)};
				ReadRange<T>(result, bufferPosition, 0, 1);

				return result[0];
			}

			/// <summary>
			/// Function to write data into the buffer.
			/// </summary>
			/// <param name="data">Data to write.</param>
			/// <param name="bufferPosition">Position in the lock region to write into.</param>
			/// <param name="index">Index in the array to start reading from.</param>
			/// <param name="elementCount">Number of elements in the array.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> or the <paramref name="elementCount"/> parameters are less than zero.
			/// <para>-or-</para>
			/// <para>Thrown if the index + size is larger than the number of elements in the source array.</para>
			/// </exception>
			public void WriteRange(byte[] data, int bufferPosition, int index, int elementCount)
			{
				IntPtr pointer = LockPointer + bufferPosition;

				if (bufferPosition < 0)
					throw new ArgumentException("bufferPosition", "Buffer position cannot be less than zero.");
				if (bufferPosition + elementCount > LockSize)
					throw new ArgumentOutOfRangeException("bufferPosition + size", "Buffer position and size cannot be greater than the length of the buffer.");

				Marshal.Copy(data, index, LockPointer, elementCount);
			}

			/// <summary>
			/// Function to write data into the buffer.
			/// </summary>
			/// <param name="data">Data to write.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			public void WriteRange(byte[] data)
			{
				if (data == null)
					throw new ArgumentNullException("data");

				WriteRange(data, 0, 0, data.Length);
			}

			/// <summary>
			/// Function to write data into the buffer.
			/// </summary>
			/// <typeparam name="T">Type of data to write.</typeparam>
			/// <param name="data">Data to write.</param>
			/// <param name="bufferPosition">Position in the lock region to write into.</param>
			/// <param name="index">Index in the array to start reading from.</param>
			/// <param name="elementCount">Number of elements in the array.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> or the <paramref name="elementCount"/> parameters are less than zero.
			/// <para>-or-</para>
			/// <para>Thrown if the index + size is larger than the number of elements in the source array.</para>
			/// </exception>
			public void WriteRange<T>(T[] data, int bufferPosition, int index, int elementCount)
				where T : struct
			{
				IntPtr pointer = LockPointer + bufferPosition;

				if (bufferPosition < 0)
					throw new ArgumentException("bufferPosition", "Buffer position cannot be less than zero.");
				if (bufferPosition + (elementCount * Marshal.SizeOf(typeof(T))) > LockSize)
					throw new ArgumentOutOfRangeException("bufferPosition + size", "Buffer position and size cannot be greater than the length of the buffer.");

				pointer.CopyFrom<T>(data, index, elementCount * Marshal.SizeOf(typeof(T)));
			}

			/// <summary>
			/// Function to write data into the buffer.
			/// </summary>
			/// <typeparam name="T">Type of data to write.</typeparam>
			/// <param name="data">Data to write.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
			public void WriteRange<T>(T[] data)
				where T : struct
			{
				if (data == null)
					throw new ArgumentNullException("data");

				WriteRange<T>(data, 0, 0, data.Length);
			}

			/// <summary>
			/// Function to write data into the buffer.
			/// </summary>
			/// <typeparam name="T">Type of data to write.</typeparam>
			/// <param name="data">Data to write.</param>
			/// <param name="bufferPosition">Position in the buffer to write into.</param>
			public void Write<T>(T data, int bufferPosition)
				where T : struct
			{
				T[] dataArray = new T[] { data };
				WriteRange<T>(dataArray, bufferPosition, 0, 1);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="LockRegion"/> class.
			/// </summary>
			/// <param name="buffer">The buffer that owns this lock.</param>
			/// <param name="offset">The offset within the buffer at which the lock begins.</param>
			/// <param name="size">The size of the lock.</param>
			internal LockRegion(GorgonDataBuffer buffer, int offset, int size)
			{
				Owner = buffer;
				LockOffset = offset;
				LockSize = size;

				// Pin the data so the garbage collector doesn't screw with us.
				_handle = GCHandle.Alloc(buffer._data, GCHandleType.Pinned);
				LockPointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer._data, offset);
			}

			/// <summary>
			/// Releases unmanaged resources and performs other cleanup operations before the
			/// <see cref="LockRegion"/> is reclaimed by garbage collection.
			/// </summary>
			~LockRegion()
			{
				Dispose(false);
			}
			#endregion			
		
			#region IDisposable Members
			/// <summary>
			/// Releases unmanaged and - optionally - managed resources
			/// </summary>
			/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
			private void Dispose(bool disposing)
			{
				if (!_disposed)
				{
					if (disposing)
					{
						if (Owner._lockedRegions.Contains(this))
							Owner._lockedRegions.Remove(this);
					}

					if ((LockPointer != IntPtr.Zero) && (_handle.IsAllocated))
					{
						_handle.Free();
						LockPointer = IntPtr.Zero;
					}

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
		#endregion

		#region Variables.
		private byte[] _data = null;						// Data for the buffer.
		private List<LockRegion> _lockedRegions = null;		// A list of locked regions.
		private bool _disposed = false;						// Flag to indicate that the buffer has been disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the size of the buffer in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to unlock all the locked regions.
		/// </summary>
		/// <remarks>This method will force all locked regions to unlock and will leave those objects in an unknown state.</remarks>
		public void Unlock()
		{
			while (_lockedRegions.Count > 0)
				_lockedRegions[0].Dispose();
		}

		/// <summary>
		/// Function to lock a region of memory for reading and writing.
		/// </summary>
		/// <param name="offset">Offset into the buffer to start the lock at.</param>
		/// <param name="size">Number of bytes in the buffer to lock.</param>
		/// <returns>A lock object that will enable read/write access to the buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="offset"/> is less than zero or the <paramref name="size"/> parameter is less than one.
		/// <para>-or-</para>
		/// <para>Thrown when the region is already locked.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the offset and size parameters are larger than the size of the buffer.</exception>
		public LockRegion Lock(int offset, int size)
		{
			LockRegion newLock = null;

			if (offset < 0)
				throw new ArgumentException("Offset must be greater than zero.", "offset");

			if (size < 1)
				throw new ArgumentException("Size must be greater than one byte.", "size");

			if (offset + size > Size)
				throw new ArgumentOutOfRangeException("offset + size", "Offset and size must be less than the size of the buffer.");

			// Don't allow overlap.
			foreach (var region in _lockedRegions)
			{
				int lockSpace = region.LockOffset + region.LockSize;
				int requestedLockSpace = offset + size;

				// Check inside the requested region.
				if (((region.LockOffset >= offset) && (region.LockOffset < requestedLockSpace)) || ((lockSpace > offset) && (lockSpace < requestedLockSpace)))
					throw new ArgumentException("offset", "The offset '0x" + offset.FormatHex() + "' is already locked by another region.");
				else
				{
					// Check the ends
					if (((offset >= region.LockOffset) && (offset < lockSpace)) || ((requestedLockSpace > region.LockOffset) && (requestedLockSpace < lockSpace)))
						throw new ArgumentException("offset", "The offset '0x" + offset.FormatHex() + "' is already locked by another region.");
				}
			}

			newLock = new LockRegion(this, offset, size);
			_lockedRegions.Add(newLock);

			return newLock;
		}

		/// <summary>
		/// Function to lock a region of memory for reading and writing.
		/// </summary>
		/// <param name="size">Number of bytes in the buffer to lock.</param>
		/// <returns>A lock object that will enable read/write access to the buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="size"/> parameter is less than one.
		/// <para>-or-</para>
		/// <para>Thrown when the region is already locked.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the size parameter is larger than the size of the buffer.</exception>
		public LockRegion Lock(int size)
		{
			return Lock(0, size);
		}

		/// <summary>
		/// Function to lock a region of memory for reading and writing.
		/// </summary>
		/// <returns>A lock object that will enable read/write access to the buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the region is already locked.</exception>
		public LockRegion Lock()
		{
			return Lock(0, _data.Length);
		}

		/// <summary>
		/// Function to free the data in the buffer.
		/// </summary>
		public void Free()
		{
			Allocate(0);
		}

		/// <summary>
		/// Function to allocate data to the buffer.
		/// </summary>
		/// <param name="size">Size to allocate.</param>
		/// <remarks>This will unlock all locked data objects and will clear the contents of the buffer.
		/// <para>Passing 0 to the <paramref name="size"/> parameter will empty the buffer.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is less than zero.</exception>
		public void Allocate(int size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException("size", "Size must be a positive number.");

			Unlock();
			_data = new byte[size];
			Size = size;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataBuffer"/> class.
		/// </summary>
		public GorgonDataBuffer()
			: this(0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataBuffer"/> class.
		/// </summary>
		/// <param name="size">The size to allocate, in bytes.</param>
		public GorgonDataBuffer(int size)
		{
			_lockedRegions = new List<LockRegion>();
			Allocate(size);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					Unlock();

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
