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
// Created: Tuesday, July 19, 2005 10:51:23 PM
// 
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using GorgonLibrary.Graphics;
using DX = SlimDX;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// A vertex and index caching system.
	/// </summary>
	/// <remarks>
	/// This object will store our vertices so that we may read them and modify them
	/// at any time.  
	/// </remarks>
	public class VertexCache<V>
		: IDataCache<V>, IDisposable, IDeviceStateObject
		where V : struct
	{
		#region Variables.
		private VertexBuffer _vertexBuffer;			// Vertex buffers for the cache.
		private V[] _vertices;						// Vertices for buffers.
		private int _vertexCount;				    // Number of vertices.		
		private VertexType _vertexType;			    // Vertex declaration.
		private bool _compiled;						// Vertex buffer stream has been compiled?
		private BufferUsages _vertexBufferUsage;		// Usage flags for each buffer.
		private int _verticesWritten;				// Number of vertices written.
		private int _vertexOffset;					// Offset to start writing at in the buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the vertex type.
		/// </summary>
		public VertexType VertexType
		{
			get
			{
				return _vertexType;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException();

				if (_vertexType != null)
					_vertexType.Dispose();

				_vertexType = value.Clone();
			}
		}

		/// <summary>
		/// Property to return or set a vertex buffer in the cache.
		/// </summary>
		public VertexBuffer VertexBuffer
		{
			get
			{
				// Force a compile upon retrieval.
				Update(false);

				return _vertexBuffer;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public VertexCache()
		{
			_vertexCount = 0;
			_vertexType = new VertexType();
			_vertexBuffer = null;
			_vertices = null;
			_compiled = false;
			_vertexBufferUsage = BufferUsages.Static | BufferUsages.WriteOnly;
			_verticesWritten = 0;
			_vertexOffset = 0;

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
				// Destroy vertex buffers.
				if (_vertexBuffer != null)
				{
					_vertexBuffer.Dispose();
					_vertexBuffer = null;
				}

				_vertexType.Dispose();
				DeviceStateList.Remove(this);
			}

			_vertices = null;
			_vertexType = null;
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
			if (_vertexBuffer != null)
			{
				_vertexBuffer.Release();
				_compiled = false;
			}
		}

		/// <summary>
		/// Function to that's called when the device enters a reset state.
		/// </summary>
		public void DeviceReset()
		{
			// Reset pointers.
			_verticesWritten = 0;
			_vertexOffset = 0;

			// Recompile the buffers.
			Update(true);
		}
		#endregion

		#region IDataCache<V> Members
		#region Properties.
		/// <summary>
		/// Property to set or return the number of vertices within the cache.
		/// </summary>
		/// <value>
		/// An integer containing a total count of the vertices within the cache.
		/// </value>
		/// <remarks>
		/// Use this to set the size of the vertex cache (size is the number of vertices, NOT bytes).
		/// <para>This will represent the number of vertices, and not the exact size per vertex stream.</para>
		/// </remarks>
		public int Count
		{
			get
			{
				return _vertexCount;
			}
			set
			{
				_vertexCount = value;

				if (_vertexBuffer != null)
				{
					_vertexBuffer.Dispose();
					_vertexBuffer = null;
				}

				if (value > 0)
					_vertices = new V[_vertexCount];
				else
					_vertices = null;
				_compiled = false;
			}
		}

		/// <summary>
		/// Property to set or return the vertex buffer usage per stream.
		/// </summary>
		public BufferUsages BufferUsage
		{
			get
			{
				return _vertexBufferUsage;
			}
			set
			{
				_vertexBufferUsage = value;
			}
		}

		/// <summary>
		/// Property to return the vertices stored in the cache.
		/// </summary>
		public V[] CachedData
		{
			get
			{
				return _vertices;
			}
		}

		/// <summary>
		/// Read only property to determine if the underlying vertex buffers need to be updated.
		/// </summary>
		/// <value>Returns TRUE if an update is necessary, FALSE if not.</value>
		public bool NeedsUpdate
		{
			get
			{
				return !_compiled;
			}
		}

		/// <summary>
		/// Property to return the amount of data written to the buffer already.
		/// </summary>
		public int DataWritten
		{
			get
			{
				return _verticesWritten;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > _vertexCount)
					value = _vertexCount;

				_verticesWritten = value;
			}
		}

		/// <summary>
		/// Property to return the start of the data in the buffer.
		/// </summary>
		public int DataOffset
		{
			get
			{
				if ((_vertexOffset - _verticesWritten) < 0)
					return 0;
				else
					return _vertexOffset - _verticesWritten;
			}
			set
			{
				if (value > _vertexCount)
					value = _vertexCount;
				if (value < 0)
					value = 0;
				_vertexOffset = value;
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
		public void WriteData(int sourceStart, int destinationStart, int length, V[] data)
		{
			for (int i = 0; i < length; i++)
				_vertices[destinationStart + i] = data[sourceStart + i];
			_compiled = false;
			_verticesWritten = length + destinationStart;
		}

		/// <summary>
		/// Function to update the vertex buffers with the current information held in the cache.
		/// </summary>
		/// <param name="isRestoring">TRUE if this is being called due to a device restoration, FALSE if not.</param>
		/// <remarks>This function is used to send the data to the vertex buffer(s).</remarks>
		public void Update(bool isRestoring)
		{
			// Do nothing if we're not in need of a compilation.
			if ((!NeedsUpdate) && (!isRestoring))
				return;

			// Do nothing if no vertices defined yet.
			if ((_vertexCount > 0) && (!_compiled) && (_vertices != null))
			{
				// First, check to see if the buffer has been created, or released.
				if (_vertexBuffer == null)
					_vertexBuffer = new VertexBuffer(_vertexType.VertexSize(0), _vertexCount, _vertexBufferUsage);
				else
				{
					// Buffer has been modified?
					if (_vertexBuffer.VertexSize != _vertexType.VertexSize(0))
						_vertexBuffer.VertexSize = _vertexType.VertexSize(0);
					if (_vertexBuffer.BufferUsage != _vertexBufferUsage)
						_vertexBuffer.BufferUsage = _vertexBufferUsage;
					if ((_vertexBuffer.VertexCount != _vertexCount) || (isRestoring))
						_vertexBuffer.VertexCount = _vertexCount;
				}

				// Now copy data into the buffers.
				if (((_vertexOffset + _verticesWritten) < _vertexCount) && ((_vertexBufferUsage & BufferUsages.Dynamic) != 0) && (_vertexOffset > 0))
				{
					_vertexBuffer.Lock(_vertexOffset, _verticesWritten, BufferLockFlags.NoOverwrite);
					_vertexOffset += _verticesWritten;
				}
				else
				{
					_vertexOffset = 0;
					_vertexBuffer.Lock(BufferLockFlags.Discard);
				}
				_vertexBuffer.Write<V>(_vertices, _vertexOffset, _verticesWritten);
				_vertexBuffer.Unlock();
				_compiled = true;
			}
		}

		/// <summary>
		/// Function to release the underlying vertex buffers.
		/// </summary>
		/// <remarks>
		/// Use this function to clear out any of the vertex buffers.
		/// <para>
		/// Use this with care as it will invalidate any vertex buffers that are being referenced elsewhere.
		/// </para>
		/// </remarks>
		public void Release()
		{
			if ((_vertexBuffer != null) && ((_vertexBuffer.BufferUsage & BufferUsages.Dynamic) != 0))
			{
				_vertexBuffer.Release();
				_compiled = false;
			}
		}
		#endregion
		#endregion
	}
}
