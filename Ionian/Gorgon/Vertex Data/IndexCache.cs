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
// Created: Tuesday, September 20, 2005 12:01:32 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using DX = SlimDX;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Index cache system.
	/// </summary>
	/// <remarks>
	/// This object will store our indices so that we may read them and modify them
	/// at any time.  
	/// </remarks>
	public class IndexCache<T>
		: IDataCache<T>, IDisposable, IDeviceStateObject
		where T : struct
	{
		#region Variables.
		private int _indexCount;				    // Number of indices.
		private IndexBuffer _indexBuffer;		    // Index buffer.
		private BufferUsages _indexBufferUsage;		// Buffer usage for index buffer.
		private IndexBufferType _indexBufferType;	// Type of indices.
		private bool _compiled;				        // Index buffer has been compiled?
		private T[] _indices;					// Indices for index buffer.
		private int _indicesWritten;				// Number of indices written.
		private int _indexOffset;					// Index to start writing at in the buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the index buffer type to use.
		/// </summary>
		public IndexBufferType IndexBufferType
		{
			get
			{
				return _indexBufferType;
			}
			set
			{
				_indexBufferType = value;
				_compiled = false;
			}
		}

		/// <summary>
		/// Property to return the index buffer for this cache.
		/// </summary>
		public IndexBuffer IndexBuffer
		{
			get
			{
				Update(false);
				return _indexBuffer;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public IndexCache()
		{
			_indexBufferUsage = BufferUsages.Static | BufferUsages.WriteOnly;
			_indexBufferType = IndexBufferType.Index16;
			_indices = null;
			_indexCount = 0;
			_indexOffset = 0;
			_indicesWritten = 0;

			DeviceStateList.Add(this);
		}
		#endregion

		#region IDisposable Members.
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to remove only unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_indexBuffer != null)
					_indexBuffer.Dispose();

				DeviceStateList.Remove(this);
			}

			_indexBuffer = null;
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

		#region IDeviceStateObject Members
		/// <summary>
		/// Function that's called when the device enters a lost state.
		/// </summary>
		public void DeviceLost()
		{
			Release();
		}

		/// <summary>
		/// Function to force the loss of the data for the index cache.
		/// </summary>
		public void ForceRelease()
		{
			// Force the loss of the index buffer.
			if (_indexBuffer != null)
				_indexBuffer.Dispose();
			_indexBuffer = null;
			_compiled = false;
		}

		/// <summary>
		/// Function to that's called when the device enters a reset state.
		/// </summary>
		public void DeviceReset()
		{
			// Recompile the buffers.
			Update(true);
		}
		#endregion

		#region IDataCache<T> Members
		#region Properties.
		/// <summary>
		/// Property to set or return the usage of the index buffer.
		/// </summary>
		public BufferUsages BufferUsage
		{
			get
			{
				return _indexBufferUsage;
			}
			set
			{
				_indexBufferUsage = value;
				_compiled = false;
			}
		}

		/// <summary>
		/// Property to return an array of index data.
		/// </summary>
		public T[] CachedData
		{
			get
			{
				return _indices;
			}
		}


		/// <summary>
		/// Property to set or return the number of indices within the cache.
		/// </summary>
		/// <value>
		/// An integer containing a total count of the indices within the cache.
		/// </value>
		/// <remarks>
		/// Use this to set the size of the index cache (size is the number of indices, and NOT bytes).
		/// </remarks>
		public int Count
		{
			get
			{
				return _indexCount;
			}
			set
			{
				_indexCount = value;

				// Destroy previous buffer.
				if (_indexBuffer != null)
				{
					_indexBuffer.Dispose();
					_indexBuffer = null;
				}

				// Build the cache buffer.
				if (value > 0)
					_indices = new T[value];
				else
					_indices = null;

				_compiled = false;
			}
		}

		/// <summary>
		/// Read only property to determine if the underlying index buffer needs to be updated.
		/// </summary>
		/// <value>Returns TRUE if an update is necessary, FALSE if not.</value>
		public bool NeedsUpdate
		{
			get
			{
				return _compiled;
			}
		}

		/// <summary>
		/// Property to return the amount of data written to the buffer already.
		/// </summary>
		public int DataWritten
		{
			get
			{
				return _indicesWritten;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > _indexCount)
					value = _indexCount;

				_indicesWritten = value;
			}
		}

		/// <summary>
		/// Property to return the start of the data in the buffer.
		/// </summary>
		public int DataOffset
		{
			get
			{
				if ((_indexOffset - _indicesWritten) < 0)
					return 0;
				else
					return _indexOffset - _indicesWritten;
			}
			set
			{
				if (value > _indexCount)
					value = _indexCount;
				if (value < 0)
					value = 0;
				_indexOffset = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to write data to the cache.
		/// </summary>
		/// <param name="sourceStart">Start of the data in the source array of data.</param>
		/// <param name="destinationStart">Start in the destination buffer to write at.</param>
		/// <param name="length">Length of data to write.</param>
		/// <param name="data">Data to write.</param>
		public void WriteData(int sourceStart, int destinationStart, int length, T[] data)
		{
			for (int i = 0; i < length; i++)
				_indices[destinationStart + i] = data[sourceStart + i];
			_compiled = false;
			_indicesWritten = length + destinationStart;
		}

		/// <summary>
		/// Function to update the index buffer with the current information held in the cache.
		/// </summary>
		/// <param name="isRestoring">TRUE if this is being called due to a device restoration, FALSE if not.</param>
		/// <remarks>This function is used to send the data to the index buffer.</remarks>
		public void Update(bool isRestoring)
		{
			// Do nothing if we're not in need of a compilation.
			if ((_compiled) && (!isRestoring))
				return;

			// Update index buffer.
			if ((_indexCount > 0) && (!_compiled) && (_indices != null))
			{
				if (_indexBuffer == null)
					_indexBuffer = new IndexBuffer(_indexBufferType, _indexCount, _indexBufferUsage);
				else
				{
					if (_indexBuffer.IndexType != _indexBufferType)
						_indexBuffer.IndexType = _indexBufferType;
					if (_indexBuffer.BufferUsage != _indexBufferUsage)
						_indexBuffer.BufferUsage = _indexBufferUsage;
					if ((_indexBuffer.IndexCount != _indexCount) || (isRestoring))
						_indexBuffer.IndexCount = _indexCount;
				}

				// Copy the data into the index buffer.
				// Now copy data into the buffers.
				if (((_indexOffset + _indicesWritten) < _indexCount) && ((_indexBufferUsage & BufferUsages.Dynamic) != 0))
				{
					_indexBuffer.Lock(_indexOffset, _indicesWritten, BufferLockFlags.NoOverwrite);
					_indexOffset += _indicesWritten;
				}
				else
				{
					_indexOffset = 0;
					_indexBuffer.Lock(_indicesWritten, BufferLockFlags.Discard);
				}

				// Lock stream.
				_indexBuffer.Write<T>(_indices, _indexOffset, _indicesWritten);
				_indexBuffer.Unlock();
				_compiled = true;
			}
		}

		/// <summary>
		/// Function to release the underlying index buffer.
		/// </summary>
		/// <remarks>
		/// Use this function to clear out the index buffer.
		/// <para>
		/// Use this with care as it will invalidate any index buffers that are being referenced elsewhere.
		/// </para>
		/// </remarks>
		public void Release()
		{
			// Free up index buffer.
			if (_indexBuffer != null)
			{
				if ((_indexBuffer.BufferUsage & BufferUsages.Dynamic) > 0)
				{
					_indexBuffer.Release();
					_compiled = false;
				}
			}
		}
		#endregion
		#endregion
	}
}
