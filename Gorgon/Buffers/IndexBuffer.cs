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
// Created: Tuesday, July 19, 2005 2:19:12 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using SharpUtilities;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object to hold index data.
	/// </summary>
	/// <remarks>
	/// An index buffer is used to store the ordering of indices.
	/// </remarks>
	public class IndexBuffer 
		: DataBuffer
	{
		#region Variables.
		private IndexBufferType	_indexSize;			// Size of a single index.
		private int	_indexLockOffset;				// Lock offset in indices.
		private int	_indexLockLength;				// Lock length in indices.
		private D3D.IndexBuffer	_indexBuffer;		// Direct 3D Index buffer.
		private DX.GraphicsStream _stream;			// Data stream for the locked buffer.		
		private Type _indexType;					// Indexing type.		
		private int _indexCount;					// Number of indices in this buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D index buffer.
		/// </summary>
		internal D3D.IndexBuffer D3DIndexBuffer
		{
			get
			{
				return _indexBuffer;
			}
		}

		/// <summary>
		/// Property to set or get the number of indices for this buffer.
		/// </summary>
		public int IndexCount
		{
			get
			{
				return _indexCount;
			}
			set
			{
				_indexCount = value;
				_size = _indexCount * Marshal.SizeOf(_indexType);
				Refresh();
			}
		}

		/// <summary>
		/// Property to set or get the size of each index in the buffer in bytes.
		/// </summary>
		public IndexBufferType IndexType
		{
			get
			{
				return _indexSize;
			}
			set
			{
				_indexSize = value;

				if (value == IndexBufferType.Index16)
					_indexType = typeof(ushort);
				else
					_indexType = typeof(uint);

				_size = _indexCount * Marshal.SizeOf(_indexType);
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the lock offset in indices.
		/// </summary>
		public override int LockOffset
		{
			get
			{
				return _indexLockOffset;
			}
		}

		/// <summary>
		/// Property to return the length of locked data in indices.
		/// </summary>
		public override int LockLength
		{
			get
			{
				return _indexLockLength;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to lock the index buffer for writing.
		/// </summary>
		/// <param name="offset">Offset to start the lock at, in bytes.</param>
		/// <param name="length">Length of the lock, in bytes.</param>
		/// <param name="flags">Locking flags.</param>
		protected override void LockBuffer(int offset, int length, BufferLockFlags flags)
		{
			if (offset + length > _size)
				throw new BufferOverflowedException(offset + length, true, null);

			try
			{
				// Only allow a complete discard if the buffer is static.
				if (((_usage & BufferUsage.Static) != 0) && (flags != BufferLockFlags.Normal))
					flags = BufferLockFlags.Normal;

				// Unlock if the buffer is locked.
				if (_locked)
					Unlock();

				_stream = _indexBuffer.Lock(offset, length, Converter.Convert(flags));
				_pointer = _stream.InternalData;

				_lockStart = offset;
				_lockSize = length;
				_locked = true;
			}
			catch(Exception ex)
			{
				throw new CannotLockException(GetType(), ex);
			}
		}

		/// <summary>
		/// Function to unlock and destroy the index buffer.
		/// </summary>
		internal void Release()
		{
			if (_locked)
				Unlock();

			if (_indexBuffer != null)
			{
				_indexBuffer.Dispose();
				_indexBuffer = null;
			}
		}

		/// <summary>
		/// Function to unlock a locked buffer.
		/// </summary>
		public override void Unlock()
		{
			if (!_locked)
				throw new NotLockedException(GetType(), null);

			_stream.Dispose();
			_indexBuffer.Unlock();
			_pointer = IntPtr.Zero;

			_lockStart = -1;
			_lockSize = 0;
			_indexLockOffset = -1;
			_indexLockLength = 0;

			_locked = false;
		}

		/// <summary>
		/// Function to (re)create the index buffer.
		/// </summary>
		public override void Refresh()
		{
			D3D.Usage flags;		// Flags for index buffer.
			D3D.Pool pool;			// Pool for vertex buffer.

			Release();

			// Do nothing if the index count is 0.
			if (_indexCount == 0)
				return;

			flags = Converter.Convert(_usage);

			try
			{
				if ((_usage & BufferUsage.UseSystemMemory) != 0)
					pool = D3D.Pool.SystemMemory;
				else
				{
					// If we're using a dynamic buffer, then put it into the default memory pool.
					if ((_usage & BufferUsage.Dynamic) != 0)
						pool = D3D.Pool.Default;
					else
						pool = D3D.Pool.Managed;
				}

				// Create the index buffer.
				_indexBuffer = new D3D.IndexBuffer(_indexType, _indexCount, Gorgon.Screen.Device, flags, pool);
			}
			catch (Exception ex)
			{
				throw new CannotCreateException(string.Empty, GetType(), ex);
			}
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="indexoffset">Index to start at.</param>
		/// <param name="indexcount">Number of indices to count.</param>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>This function will lock by index, and account for index size.</remarks>
		public void Lock(int indexoffset,int indexcount,BufferLockFlags flags)
		{				
			// If we've discarded the buffer, lock the whole thing.
			// This is because index buffers return a new and blank
			// buffer when discard is specified.
			if (flags == BufferLockFlags.Discard)
			{
				_indexLockOffset = 0;
				_indexLockLength = _indexCount;
			} 
			else
			{
				_indexLockOffset = indexoffset;
				_indexLockLength = indexcount;
			}

			LockBuffer(indexoffset * Marshal.SizeOf(_indexType),indexcount * Marshal.SizeOf(_indexType),flags);
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="indexcount">Number of indices to count.</param>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>This function will lock by index, and account for index size.</remarks>
		public void Lock(int indexcount, BufferLockFlags flags)
		{
			_indexLockOffset = 0;

			// If we've discarded the buffer, lock the whole thing.
			// This is because index buffers return a new and blank
			// buffer when discard is specified.
			if (flags == BufferLockFlags.Discard)
				_indexLockLength = _indexCount;
			else
				_indexLockLength = indexcount;

			LockBuffer(0, indexcount * Marshal.SizeOf(_indexType), flags);
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>
		/// This function will lock by index, and account for index size.  This function will
		/// also lock the entire buffer.</remarks>
		public void Lock(BufferLockFlags flags)
		{
			_indexLockOffset = 0;
			_indexLockLength = _indexCount;

			LockBuffer(0, _indexCount * Marshal.SizeOf(_indexType), flags);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="indextype">Type of index buffer to use.</param>
		/// <param name="numindices">Total number of indices for this buffer.</param>		
		/// <param name="bufferusage">Usage flags for the buffer.</param>
		public IndexBuffer(IndexBufferType indextype, int numindices, BufferUsage bufferusage) : base(bufferusage)
		{
			_indexSize = indextype;
			if (indextype == IndexBufferType.Index16)
				_indexType = typeof(ushort);
			else
			{
				if (!Gorgon.Driver.SupportIndex32)
					throw new InvalidIndexSizeException(null);

				_indexType = typeof(uint);			
			}
			_indexCount = numindices;
			_size = Marshal.SizeOf(_indexType) * numindices;
			_indexLockOffset = -1;
			_indexLockLength = 0;

			// Force software processing if no transform/lighting acceleration is present.
			if (!Gorgon.Driver.TnL)
				_usage |= BufferUsage.ForceSoftware;

			Refresh();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~IndexBuffer()
		{
			Dispose(false);
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to dispose all resources, FALSE to only release unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Release();
		}
		#endregion
	}
}
