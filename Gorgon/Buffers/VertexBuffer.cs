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
	public class VertexBuffer 
		: DataBuffer
	{
		#region Variables.
		private int _vertexSize;					// Size of a single vertex.
		private int _vertexCount;					// Number of vertices in this buffer.
		private int _vertexLockOffset;				// Lock offset in vertices.
		private int _vertexLockLength;				// Lock length in vertices.
		private D3D9.VertexBuffer _vertexBuffer;	// Direct 3D Vertex buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D vertex buffer.
		/// </summary>
		internal D3D9.VertexBuffer D3DVertexBuffer
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
				BufferSize = _vertexCount * _vertexSize;
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
				BufferSize = _vertexCount * _vertexSize;
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
		/// Function to create the vertex buffer.
		/// </summary>
		private void CreateVertexBuffer()
		{
			D3D9.Usage flags;		// Flags for vertex buffer.
			D3D9.Pool pool;			// Pool for vertex buffer.

			// If the size of the buffer is 0, then exit.
			if (BufferSize == 0)
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

				// Create the vertex buffer.
				_vertexBuffer = new D3D9.VertexBuffer(Gorgon.Screen.Device, BufferSize, flags, D3D9.VertexFormat.None, pool);
			}
			catch (Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotCreate, "There was an error creating the D3D vertex buffer.", ex);
			}
		}

		/// <summary>
		/// Function to retrieve the datastream for a locked object.
		/// </summary>
		/// <param name="lockFlags">Flags used in locking.</param>
		/// <returns>A data stream for a locked buffer.</returns>
		protected override SlimDX.DataStream GetDataStream(D3D9.LockFlags lockFlags)
		{
			return _vertexBuffer.Lock(LockOffsetInBytes, LockSizeInBytes, lockFlags);
		}

		/// <summary>
		/// Function to unlock and destroy the vertex buffer.
		/// </summary>
		internal void Release()
		{
			if (IsLocked)
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
			Release();
			CreateVertexBuffer();
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <param name="vertexoffset">Vertex to start at.</param>
		/// <param name="vertexcount">Number of vertices to lock.</param>
		/// <param name="flags">Flags for locking.</param>
		/// <remarks>This function will lock by vertex, and account for vertex size.</remarks>
		public void Lock(int vertexoffset, int vertexcount, BufferLockFlags flags)
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
			base.Unlock();

			_vertexBuffer.Unlock();

			_vertexLockOffset = -1;
			_vertexLockLength = 0;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="vertexsize">Size of a single vertex in bytes.</param>
		/// <param name="numvertices">Total number of vertices for this buffer.</param>
		/// <param name="bufferusage">Usage flags for the buffer.</param>		
		public VertexBuffer(int vertexsize, int numvertices, BufferUsages bufferusage)
			: base(bufferusage)
		{
			// Force software processing if no transform/lighting acceleration is present.
			if (!Gorgon.CurrentDriver.HardwareTransformAndLighting)
				BufferUsage |= BufferUsages.ForceSoftware;

            _vertexSize = vertexsize;
            _vertexCount = numvertices;
            _vertexLockOffset = -1;
            
            BufferSize = vertexsize * numvertices;			
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
