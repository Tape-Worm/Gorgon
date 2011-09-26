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
// Created: Thursday, September 22, 2011 1:03:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Native;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A Direct 3D9 geometry buffer.
	/// </summary>
	class D3D9GeometryBuffer
		: GorgonGeometryBuffer, IUnmanagedObject
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the buffer was disposed.
		private Pool _pool = Pool.Managed;						// Memory pool for the buffer.
		private Usage _usage = SlimDX.Direct3D9.Usage.None;		// Usage flags for the buffer.
		private Device _device = null;							// Direct 3D device object.
		private ILockableBuffer _thisBuffer = null;				// Lockable buffer interface for this object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the vertex declaration.
		/// </summary>
		public VertexDeclaration D3DVertexDeclaration
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the underlying D3D index buffer.
		/// </summary>
		public IndexBuffer D3DIndexBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the underlying D3D vertex buffer.
		/// </summary>
		public VertexBuffer D3DVertexBuffer
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to translate the usage flags.
		/// </summary>
		private void GetUsage()
		{
			if ((Usage & GeometryBufferUsage.WriteOnly) == GeometryBufferUsage.WriteOnly)
				_usage = SlimDX.Direct3D9.Usage.WriteOnly;

			if ((Usage & GeometryBufferUsage.PreClipped) == GeometryBufferUsage.PreClipped)
				_usage |= SlimDX.Direct3D9.Usage.DoNotClip;

			if ((Usage & GeometryBufferUsage.Dynamic) == GeometryBufferUsage.Dynamic)
			{
				_usage |= SlimDX.Direct3D9.Usage.Dynamic;
				_pool = Pool.Default;
			}
		}

		/// <summary>
		/// Function to create a new vertex buffer.
		/// </summary>
		private void CreateVertexBuffer()
		{
			GetUsage();

			if (D3DVertexDeclaration == null)
				D3DVertexDeclaration = new VertexDeclaration(_device, D3DConvert.Convert(VertexElements));

			D3DVertexBuffer = new VertexBuffer(_device, _thisBuffer.Size, _usage, VertexFormat.None, _pool);
		}

		/// <summary>
		/// Function to create a new index buffer.
		/// </summary>
		private void CreateIndexBuffer()
		{
			GetUsage();

			D3DIndexBuffer = new IndexBuffer(_device, _thisBuffer.Size, _usage, _pool, (Usage & GeometryBufferUsage.Indices16) == GeometryBufferUsage.Indices16);
		}

		/// <summary>
		/// Function to perform the lock on the vertex buffer.
		/// </summary>
		/// <param name="offset">Offset, in bytes, within the buffer to start the lock.</param>
		/// <param name="size">Size, in bytes, to lock.</param>
		/// <param name="flags">Flag to apply to the lock.</param>
		/// <returns>
		/// A data stream containing the locked buffer data.
		/// </returns>
		protected override Native.GorgonDataStream LockImpl(int offset, int size, Native.LockFlags flags)
		{
			if (((Usage & GeometryBufferUsage.Indices32) == GeometryBufferUsage.Indices32) || ((Usage & GeometryBufferUsage.Indices16) == GeometryBufferUsage.Indices16))
			{
				if (D3DIndexBuffer == null)
					CreateIndexBuffer();

				return new D3D9DataStream(D3DIndexBuffer.Lock(offset, size, D3DConvert.Convert(flags)));
			}
			else
			{
				if (D3DVertexBuffer == null)
					CreateVertexBuffer();

				return new D3D9DataStream(D3DVertexBuffer.Lock(offset, size, D3DConvert.Convert(flags)));
			}
		}

		/// <summary>
		/// Function to unlock the buffer.
		/// </summary>
		protected override void UnlockImpl()
		{
			if (((Usage & GeometryBufferUsage.Indices32) == GeometryBufferUsage.Indices32) || ((Usage & GeometryBufferUsage.Indices16) == GeometryBufferUsage.Indices16))
			{
				if (D3DIndexBuffer != null)
					D3DIndexBuffer.Unlock();
			}
			else
			{
				if (D3DVertexBuffer != null)
					D3DVertexBuffer.Unlock();
			}			
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (IsLocked)
						Unlock();

					if (D3DVertexBuffer != null)
						D3DVertexBuffer.Dispose();
					if (D3DIndexBuffer != null)
						D3DIndexBuffer.Dispose();
					if (D3DVertexDeclaration != null)
						D3DVertexDeclaration.Dispose();

					D3DIndexBuffer = null;
					D3DVertexBuffer = null;
				}

				_disposed = true;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9GeometryBuffer"/> class.
		/// </summary>
		/// <param name="device">D3D device object.</param>
		/// <param name="deviceWindow">The device window that created this buffer.</param>
		/// <param name="elementCount">Number of elements in the buffer.</param>
		/// <param name="usage">The usage for the buffer.</param>
		/// <param name="elements">A list of vertex elements for the buffer.</param>
		/// <param name="slot">The slot that the buffer is meant to use.</param>
		public D3D9GeometryBuffer(Device device, GorgonDeviceWindow deviceWindow, int elementCount, GeometryBufferUsage usage, GorgonVertexElementList elements, int slot)
			: base(deviceWindow, elementCount, usage, elements, slot)
		{
			_device = device;
			_thisBuffer = this;
		}
		#endregion

		#region IUnmanagedObject Members
		/// <summary>
		/// Function called when a device is placed in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			if (IsLocked)
				Unlock();

			if ((_pool & Pool.Managed) != Pool.Managed)
			{
				if (D3DVertexBuffer != null)
					D3DVertexBuffer.Dispose();
				if (D3DIndexBuffer != null)
					D3DIndexBuffer.Dispose();
			}
		}

		/// <summary>
		/// Function called when a device is reset from a lost state.
		/// </summary>
		public void DeviceReset()
		{
			if ((_pool & Pool.Managed) != Pool.Managed)
			{
				if (((Usage & GeometryBufferUsage.Indices16) != GeometryBufferUsage.Indices16) && ((Usage & GeometryBufferUsage.Indices32) != GeometryBufferUsage.Indices32))
					CreateVertexBuffer();
				else
					CreateIndexBuffer();
			}
		}
		#endregion
	}
}
