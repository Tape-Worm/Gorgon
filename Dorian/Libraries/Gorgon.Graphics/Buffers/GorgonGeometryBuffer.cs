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
// Created: Sunday, September 25, 2011 7:33:01 PM
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
	public enum GeometryBufferUsage
	{
		/// <summary>
		/// Default usage.  This will create a static buffer.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Buffer can only be written to.
		/// </summary>
		WriteOnly = 1,
		/// <summary>
		/// Buffer is a dynamic buffer.
		/// </summary>
		Dynamic = 2,
		/// <summary>
		/// Buffer is a 16 bit index buffer.
		/// </summary>
		Indices16 = 4,
		/// <summary>
		/// Buffer is a 16 bit index buffer.
		/// </summary>
		Indices32 = 8,
		/// <summary>
		/// Buffer can expect pre-clipped vertices.  This only applies to buffers that contain vertices.
		/// </summary>
		PreClipped = 16
	}

	/// <summary>
	/// A buffer object used to hold geometry information such as vertices and indices.
	/// </summary>
	public abstract class GorgonGeometryBuffer
		: ILockableBuffer
	{
		#region Variables.
		private GorgonDataStream _lockStream = null;			// Stream used to read from and/or write to the buffer.
		private int _lockOffsetBytes = 0;						// Lock offset in bytes.
		private int _lockSizeBytes = 0;							// Lock size in bytes.
		private int _sizeBytes = 0;								// Size of the buffer, in bytes.
		private bool _isIndexBuffer = false;					// Flag to indicate that this is an index buffer.
		private bool _disposed = false;							// Flag to indicate that the buffer was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the device window that created this object.
		/// </summary>
		public GorgonDeviceWindow Owner
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of vertex elements for the buffer. 
		/// </summary>
		/// <remarks>This has no meaning if the Indices16 or Indices32 usage flag is applied to the buffer.</remarks>
		public GorgonVertexElementList VertexElements
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the slot that this buffer is supposed to use.		
		/// </summary>
		/// <remarks>This has no meaning if the Indices16 or Indices32 usage flag is applied to the buffer.</remarks>
		public int Slot
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of each individual data element in the buffer.
		/// </summary>
		public int ElementSize
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of elements in the buffer.
		/// </summary>
		public int ElementCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of locked elements in the buffer.
		/// </summary>
		public int LockedElementCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the offset of the lock into the buffer, in elements.
		/// </summary>
		public int LockedElementOffset
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the usage for this buffer.
		/// </summary>
		public GeometryBufferUsage Usage
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether this buffer is dynamic or not.
		/// </summary>
		public bool IsDynamic
		{
			get
			{
				return ((Usage & GeometryBufferUsage.Dynamic) == GeometryBufferUsage.Dynamic);
			}
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

		/// <summary>
		/// Function to lock the entire buffer.
		/// </summary>
		/// <param name="flags">Flags to use for locking.</param>
		public GorgonDataStream Lock(LockFlags flags)
		{
			return Lock(0, ElementCount, flags);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGeometryBuffer"/> class.
		/// </summary>
		/// <param name="deviceWindow">The device window that created this buffer.</param>
		/// <param name="elementCount">Number of elements in the buffer.</param>
		/// <param name="usage">The usage for the buffer.</param>
		/// <param name="elements">A list of vertex elements for the buffer.</param>
		/// <param name="slot">The slot that the buffer is meant to use.</param>
		protected GorgonGeometryBuffer(GorgonDeviceWindow deviceWindow, int elementCount, GeometryBufferUsage usage, GorgonVertexElementList elements, int slot)
		{
			IsLocked = false;

			Owner = deviceWindow;
			Slot = slot;
			Usage = usage;
			ElementCount = elementCount;

			if ((usage & GeometryBufferUsage.Indices16) == GeometryBufferUsage.Indices16)
			{
				ElementSize = 2;
				Slot = 0;
				_isIndexBuffer = true;
			}

			if ((usage & GeometryBufferUsage.Indices32) == GeometryBufferUsage.Indices32)
			{
				ElementSize = 4;
				Slot = 0;
				_isIndexBuffer = true;
			}

			if (!_isIndexBuffer)
			{
				if ((elements == null) || (elements.Count == 0))
					throw new ArgumentException("A vertex buffer requires a list of vertex elements.", "elements");

				var findSlot = elements.Where(item => item.Slot == Slot);
				if (findSlot.Count() == 0)
					throw new ArgumentException("There is no vertex element in slot " + slot.ToString() + ".", "slot");

				VertexElements = elements;
				ElementSize = elements.GetSlotSize(slot);
			}

			_sizeBytes = ElementSize * ElementCount;
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
		/// Property to return the offset into the locked region, by item.
		/// </summary>
		int ILockableBuffer.LockOffset
		{
			get
			{
				return _lockOffsetBytes;
			}
		}

		/// <summary>
		/// Property to return the size of the lock, in bytes.
		/// </summary>
		int ILockableBuffer.LockSize
		{
			get
			{
				return _lockSizeBytes;
			}
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		int ILockableBuffer.Size
		{
			get
			{
				return _sizeBytes;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to lock the buffer for reading/writing.
		/// </summary>
		/// <param name="elementOffset">Offset into the buffer to start writing at, in elements.</param>
		/// <param name="elementCount">Number of elements to read or write.</param>
		/// <param name="flags">Flags for the type of lock to perform.</param>
		/// <returns>
		/// A <see cref="GorgonLibrary.Native.GorgonDataStream">GorgonDataStream</see> object.
		/// </returns>
		/// <remarks>When the buffer is locked, the user must call the <see cref="M:GorgonLibrary.Graphics.GorgonVertexBuffer.Unlock">Unlock</see> method to release the buffer when finished reading/writing.
		/// <para></para>
		/// </remarks>
		public GorgonDataStream Lock(int elementOffset, int elementCount, LockFlags flags)
		{
			if (elementOffset + elementCount > ElementCount)
				throw new ArgumentOutOfRangeException("The offset and the size cannot be larger than the total size of the buffer.");

			if (elementOffset < 0)
				throw new ArgumentException("Cannot be less than zero.", "offset");
			if (elementCount < 0)
				throw new ArgumentException("Cannot be less than zero.", "size");

			// With static buffers, we must lock with no flags.
			if ((flags != LockFlags.None) && (!IsDynamic))
				flags = LockFlags.None;

			// When discarding, just lock the entire thing because discard will return a new buffer.
			if (flags == LockFlags.Discard)
			{
				elementOffset = 0;
				elementCount = ElementCount;
			}

			if (IsLocked)
				Unlock();

			_lockStream = LockImpl(elementOffset * ElementSize, elementCount * ElementSize, flags);

			IsLocked = true;
			LockedElementCount = elementCount;
			LockedElementOffset = elementOffset;
			_lockOffsetBytes = LockedElementOffset * ElementSize;
			_lockSizeBytes = LockedElementCount * ElementSize;			

			return _lockStream;
		}

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		public void Unlock()
		{
			if (!IsLocked)
				return;

			// Remove the buffer.
			if (_lockStream != null)
				_lockStream.Dispose();

			UnlockImpl();

			_lockStream = null;
			IsLocked = false;
			LockedElementCount = _lockSizeBytes = 0;
			LockedElementOffset = _lockOffsetBytes = 0;
		}
		#endregion
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
				{
					if (IsLocked)
						Unlock();

					if (Owner.VertexBufferSlots.Contains(this))
						Owner.VertexBufferSlots.Remove(this);

					((IObjectTracker)Owner).RemoveTrackedObject(this);
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
}
