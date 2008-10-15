#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Tuesday, July 19, 2005 2:19:12 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

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
		private IndexBufferType _indexSize;			// Size of a single index.
		private int _indexLockOffset;				// Lock offset in indices.
		private int _indexLockLength;				// Lock length in indices.
		private D3D9.IndexBuffer _indexBuffer;		// Direct 3D Index buffer.
		private Type _indexType;					// Indexing type.		
		private int _indexCount;					// Number of indices in this buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D index buffer.
		/// </summary>
		internal D3D9.IndexBuffer D3DIndexBuffer
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
				BufferSize = Marshal.SizeOf(_indexType) * _indexCount;
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

				BufferSize = Marshal.SizeOf(_indexType) * _indexCount;
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
		/// Function to create the index buffer.
		/// </summary>
		private void CreateIndexBuffer()
		{
			D3D9.Usage flags;		// Flags for index buffer.
			D3D9.Pool pool;			// Pool for vertex buffer.

			// Do nothing if the index count is 0.
			if (_indexCount == 0)
				return;

			flags = Converter.Convert(BufferUsage);

			try
			{
				if ((BufferUsage & BufferUsages.UseSystemMemory) == BufferUsages.UseSystemMemory)
					pool = D3D9.Pool.SystemMemory;
				else
				{
					// If we're using a dynamic buffer, then put it into the default memory pool.
					if ((BufferUsage & BufferUsages.Dynamic) == BufferUsages.Dynamic)
						pool = D3D9.Pool.Default;
					else
						pool = D3D9.Pool.Managed;
				}

				// Create the index buffer.
				_indexBuffer = new D3D9.IndexBuffer(Gorgon.Screen.Device, BufferSize, flags, pool, _indexSize == IndexBufferType.Index16);
			}
			catch (Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotCreate, "There was an error creating the D3D index buffer.", ex);
			}
		}

		/// <summary>
		/// Function to retrieve the datastream for a locked object.
		/// </summary>
		/// <param name="lockFlags">Flags used in locking.</param>
		/// <returns>A data stream for a locked buffer.</returns>
		protected override SlimDX.DataStream GetDataStream(SlimDX.Direct3D9.LockFlags lockFlags)
		{
			return _indexBuffer.Lock(LockOffsetInBytes, LockSizeInBytes, lockFlags);
		}

		/// <summary>
		/// Function to unlock and destroy the index buffer.
		/// </summary>
		internal void Release()
		{
			if (IsLocked)
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
			base.Unlock();
			
			_indexBuffer.Unlock();
			_indexLockOffset = -1;
			_indexLockLength = 0;
		}

		/// <summary>
		/// Function to (re)create the index buffer.
		/// </summary>
		public override void Refresh()
		{
			Release();
			CreateIndexBuffer();
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="indexoffset">Index to start at.</param>
		/// <param name="indexcount">Number of indices to count.</param>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>This function will lock by index, and account for index size.</remarks>
		public void Lock(int indexoffset, int indexcount, BufferLockFlags flags)
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

			LockBuffer(indexoffset * Marshal.SizeOf(_indexType), indexcount * Marshal.SizeOf(_indexType), flags);
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
		public IndexBuffer(IndexBufferType indextype, int numindices, BufferUsages bufferusage)
			: base(bufferusage)
		{
			_indexSize = indextype;
			if (indextype == IndexBufferType.Index16)
				_indexType = typeof(ushort);
			else
			{
				if (!Gorgon.CurrentDriver.SupportIndex32)
					throw new ArgumentException("The video card does not support 32 bit indices.", "indexType");

				_indexType = typeof(uint);
			}
            
            // Force software processing if no transform/lighting acceleration is present.
			if (!Gorgon.CurrentDriver.HardwareTransformAndLighting)
				BufferUsage |= BufferUsages.ForceSoftware;
        
            _indexCount = numindices;
            _indexLockOffset = -1;

            BufferSize = Marshal.SizeOf(_indexType) * _indexCount;
        }

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to dispose all resources, FALSE to only release unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Release();

			base.Dispose(disposing);
		}
		#endregion
	}
}
