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
	/// Object to hold vertex data.
	/// </summary>
	/// <remarks>
	/// A vertex buffer can be used to batch up vertex data to send to the card as a packet
	/// of vertex information.  This will allow for faster rendering, as the fewer DrawPrimitive
	/// statements you have, the better.  Since Direct3D implicitly uses Vertex Buffers, we 
	/// should as well.  For anyone who wants to port this to OpenGL: it'd be interesting to
	/// see how much VBOs differ from Vertex Buffers.
	/// </remarks>
	public class VertexBuffer : DataBuffer
	{
		#region Variables.
		private int	_vertexSize;			    // Size of a single vertex.
		private int	_vertexCount;			    // Number of vertices in this buffer.
		private int	_vertexLockOffset;		    // Lock offset in vertices.
		private int	_vertexLockLength;		    // Lock length in vertices.
		private D3D.VertexBuffer _vertexBuffer;	// Direct 3D Vertex buffer.
		private DX.GraphicsStream _stream;		// Data stream for the locked buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D vertex buffer.
		/// </summary>
		internal D3D.VertexBuffer D3DVertexBuffer
		{
			get
			{
				return _vertexBuffer;
			}
		}

		/// <summary>
		/// Property to set or get the number of vertices for this buffer.
		/// </summary>
		public int VertexCount
		{
			get
			{
				return _vertexCount;
			}
			set
			{
				_vertexCount = value;
				_size = _vertexCount * _vertexSize;
				Refresh();
			}
		}

		/// <summary>
		/// Property to set or get the size of each vertex in the buffer in bytes.
		/// </summary>
		public int VertexSize
		{
			get
			{
				return _vertexSize;
			}
			set
			{
				_vertexSize = value;
				_size = _vertexCount * _vertexSize;
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the lock offset in vertices.
		/// </summary>
		public override int LockOffset
		{
			get
			{
				return _vertexLockOffset;
			}
		}

		/// <summary>
		/// Property to return the length of locked data in vertices.
		/// </summary>
		public override int LockLength
		{
			get
			{
				return _vertexLockLength;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to lock the vertex buffer for writing.
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

				_stream = _vertexBuffer.Lock(offset, length, Converter.Convert(flags));
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
		/// Function to unlock and destroy the vertex buffer.
		/// </summary>
		internal void Release()
		{
			if (_locked)
				Unlock();

			if (_vertexBuffer != null)
			{
				_vertexBuffer.Dispose();
				_vertexBuffer = null;
			}
		}

		/// <summary>
		/// Function to (re)create the vertex buffer.
		/// </summary>
		public override void Refresh()
		{
			D3D.Usage flags;		// Flags for vertex buffer.
			D3D.Pool pool;			// Pool for vertex buffer.

			Release();

			// If the size of the buffer is 0, then exit.
			if (_size == 0)
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

				// Create the vertex buffer.
				_vertexBuffer = new D3D.VertexBuffer(typeof(byte), _size, Gorgon.Screen.Device, flags, D3D.VertexFormats.None, pool);
			}
			catch (Exception ex)
			{
				throw new CannotCreateException(string.Empty, GetType(), ex);
			}
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="vertexoffset">Vertex to start at.</param>
		/// <param name="vertexcount">Number of vertices to lock.</param>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>This function will lock by vertex, and account for vertex size.</remarks>
		public void Lock(int vertexoffset,int vertexcount,BufferLockFlags flags)
		{
			// If we've discarded the buffer, lock the whole thing.
			// This is because vertex buffers return a new and blank
			// buffer when discard is specified.
			if (flags == BufferLockFlags.Discard)
			{
				_vertexLockOffset = 0;
				_vertexLockLength = _vertexCount;
			}
			else
			{
				_vertexLockOffset = vertexoffset;
				_vertexLockLength = vertexcount;
			}

			LockBuffer(vertexoffset * _vertexSize, vertexcount * _vertexSize, flags);
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="vertexcount">Number of vertices to lock.</param>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>This function will lock by vertex, and account for vertex size.</remarks>
		public void Lock(int vertexcount, BufferLockFlags flags)
		{
			_vertexLockOffset = 0;

			// If we've discarded the buffer, lock the whole thing.
			// This is because vertex buffers return a new and blank
			// buffer when discard is specified.
			if (flags == BufferLockFlags.Discard)
				_vertexLockLength = _vertexCount;
			else
				_vertexLockLength = vertexcount;

			LockBuffer(0, vertexcount * _vertexSize, flags);
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>
		/// This function will lock by vertex, and account for vertex size.  This function will
		/// also lock the entire buffer.</remarks>
		public void Lock(BufferLockFlags flags)
		{
			_vertexLockOffset = 0;
			_vertexLockLength = _vertexCount;

			LockBuffer(0, _vertexCount * _vertexSize, flags);
		}

		/// <summary>
		/// Function to unlock a locked buffer.
		/// </summary>
		public override void Unlock()
		{
			if (!_locked)
				throw new NotLockedException(GetType(), null);

			_stream.Dispose();
			_vertexBuffer.Unlock();
			_pointer = IntPtr.Zero;

			_lockStart = -1;
			_lockSize = 0;
			_vertexLockOffset = -1;
			_vertexLockLength = 0;

			_locked = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="vertexsize">Size of a single vertex in bytes.</param>
		/// <param name="numvertices">Total number of vertices for this buffer.</param>
		/// <param name="bufferusage">Usage flags for the buffer.</param>		
		public VertexBuffer(int vertexsize, int numvertices, BufferUsage bufferusage) : base(bufferusage)
		{
			_vertexSize = vertexsize;
			_vertexCount = numvertices;
			_size = vertexsize * numvertices;
			_vertexLockOffset = -1;
			_vertexLockLength = 0;

			// Force software processing if no transform/lighting acceleration is present.
			if (!Gorgon.Driver.TnL)
				_usage |= BufferUsage.ForceSoftware;

			Refresh();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~VertexBuffer()
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
