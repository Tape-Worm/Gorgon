#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, January 03, 2012 8:22:34 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold a set of vertices.
	/// </summary>
	public class GorgonVertexBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private bool _disposed = false;											// Flag to indicate that the object was disposed.
		private GorgonBufferStream<GorgonVertexBuffer> _lockStream = null;		// Stream used when locking.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D vertex buffer.
		/// </summary>
		internal D3D11.Buffer D3DVertexBuffer
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected internal override void Initialize(GorgonDataStream data)
		{
			D3D11.BufferDescription desc = new D3D11.BufferDescription();

			desc.BindFlags = D3D11.BindFlags.VertexBuffer;
			desc.CpuAccessFlags = D3DCPUAccessFlags;
			desc.OptionFlags = D3D11.ResourceOptionFlags.BufferStructured;
			desc.SizeInBytes = Size;
			desc.StructureByteStride = 0;
			desc.Usage = D3DUsage;

			if (data == null)
				D3DVertexBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice, desc);
			else
			{
				long position = data.Position;

				using (DX.DataStream stream = new DX.DataStream(data.PositionPointer, data.Length - position, true, true))
					D3DVertexBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice, stream, desc);

				if (BufferUsage == BufferUsage.Default)
				{
					data.Position = position;
					_lockStream = new GorgonBufferStream<GorgonVertexBuffer>(this, Size);
					_lockStream.IsPersistent = true;
					_lockStream.Write(data.BasePointer, (int)(data.Length - position));
					_lockStream.Position = 0;
				}
			}
		}

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		protected override void LockBuffer(BufferLockFlags lockFlags)
		{
			D3D11.MapMode mapMode = D3D11.MapMode.Write;

			if (BufferUsage == BufferUsage.Default)
			{
				if (_lockStream == null)
				{
					_lockStream = new GorgonBufferStream<GorgonVertexBuffer>(this, Size);
					_lockStream.IsPersistent = true;
				}
				_lockStream.Position = 0;
				return;
			}

			if (_lockStream != null)
			{
				_lockStream.IsPersistent = false;
				_lockStream.Dispose();
				_lockStream = null;
			}

			if ((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
				mapMode = D3D11.MapMode.WriteDiscard;

			if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
				mapMode = D3D11.MapMode.WriteNoOverwrite;

			DX.DataStream stream = null;
			Graphics.Context.MapSubresource(D3DVertexBuffer, mapMode, D3D11.MapFlags.None, out stream);
			_lockStream = new GorgonBufferStream<GorgonVertexBuffer>(this, stream);
			_lockStream.IsPersistent = true;			
		}

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected internal override void UnlockBuffer()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="destIndex">Index of the sub data to use.</param>
		/// <param name="range2D">2D contraints for the buffer.</param>
		/// <param name="front">3D front face constraint for the buffer.</param>
		/// <param name="back">3D back face constraint for the buffer.</param>
		protected override void UpdateBuffer(GorgonDataStream stream, int destIndex, System.Drawing.Rectangle range2D, int front, int back)
		{
			throw new NotImplementedException();
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
					{
						if (_lockStream != null)
						{
							_lockStream.IsPersistent = false;
							_lockStream.Dispose();
							_lockStream = null;
						}
					}

					Graphics.RemoveTrackedObject(this);
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Function to get access to the data being stored in the buffer.
		/// </summary>
		/// <param name="flags">Flags to control how the data is to be used.</param>
		/// <returns>A data stream containing the data in the buffer.</returns>
		/// <returns>Returns a constant buffer stream used to write into the buffer.</returns>
		/// <remarks>Once done with writing to the buffer, ensure that the buffer is disposed so that the data can be uploaded to the video device.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer is already locked.
		/// <para>-or-</para>
		/// <para>Thrown when the buffer usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="flags"/> parameter has the Read flag.
		/// <para>-or-</para>
		/// <para>Thrown when the buffer usage is default and the flags parameter has the Discard flag.</para>
		/// </exception>
		public GorgonDataStream GetBuffer(BufferLockFlags flags)
		{
			if (IsLocked)
				throw new InvalidOperationException("The buffer is already locked.");

			// Read is mutually exclusive.
			if ((flags & BufferLockFlags.Read) == BufferLockFlags.Read)
				throw new ArgumentException("Cannot read a vertex buffer.", "flags");

			if (BufferUsage == BufferUsage.Immutable)
				throw new InvalidOperationException("The buffer is immutable and cannot be locked.");
				
			if (((flags & BufferLockFlags.Discard) == BufferLockFlags.Discard) && (BufferUsage == BufferUsage.Default))
				throw new ArgumentException("Cannot discard a buffer with default usage.", "flags");

			LockBuffer(flags);
			IsLocked = true;

			return _lockStream;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		/// <param name="size">The size.</param>
		/// <param name="usage">The buffer usage</param>
		internal GorgonVertexBuffer(GorgonGraphics graphics, int size, BufferUsage usage)
			: base(graphics, usage)
		{
			Size = size;
		}
		#endregion
	}
}
