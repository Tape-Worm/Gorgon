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
using GorgonLibrary.IO;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold a set of indices.
	/// </summary>
	public class GorgonIndexBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private DX.DataStream _lockStream = null;								// Stream used when locking.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the buffer uses 32 bit indices or not.
		/// </summary>
		public bool Is32Bit
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to set or return the view for the resource.
		/// </summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public override GorgonResourceView View
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			if (Graphics.Input.IndexBuffer == this)
				Graphics.Input.IndexBuffer = null;

			if (IsLocked)
				Unlock();

			if (D3DResource != null)
			{
				GorgonRenderStatistics.IndexBufferCount--;
				GorgonRenderStatistics.IndexBufferSize -= D3DBuffer.Description.SizeInBytes;

				D3DResource.Dispose();
				D3DResource = null;
			}			
		}

		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected override void InitializeImpl(GorgonDataStream data)
		{
			D3D11.BufferDescription desc = new D3D11.BufferDescription();

			desc.BindFlags = D3D11.BindFlags.IndexBuffer;
			desc.CpuAccessFlags = D3DCPUAccessFlags;
			desc.OptionFlags = D3D11.ResourceOptionFlags.None;
			desc.SizeInBytes = SizeInBytes;
			desc.StructureByteStride = 0;
			desc.Usage = D3DUsage;

			if (data == null)
				D3DResource = new D3D11.Buffer(Graphics.D3DDevice, desc);
			else
			{
				long position = data.Position;

				using (DX.DataStream stream = new DX.DataStream(data.PositionPointer, data.Length - position, true, true))
					D3DResource = new D3D11.Buffer(Graphics.D3DDevice, stream, desc);
			}

			GorgonRenderStatistics.IndexBufferCount++;
			GorgonRenderStatistics.IndexBufferSize += ((D3D11.Buffer)D3DResource).Description.SizeInBytes;

#if DEBUG
			D3DResource.DebugName = "Gorgon Index Buffer #" + Graphics.GetGraphicsObjectOfType<GorgonIndexBuffer>().Count().ToString(); 
#endif
		}

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>
		/// A data stream containing the buffer data.
		/// </returns>
		protected override GorgonDataStream LockImpl(BufferLockFlags lockFlags)
		{
			D3D11.MapMode mapMode = D3D11.MapMode.Write;

			// Read is mutually exclusive.
			if ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read)
				throw new ArgumentException("Cannot read a index buffer.", "lockFlags");

			if (lockFlags == BufferLockFlags.Write)
				throw new ArgumentException("Index buffer must use nooverwrite or discard when locking.", "lockFlags");

			if ((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
				mapMode = D3D11.MapMode.WriteDiscard;

			if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
				mapMode = D3D11.MapMode.WriteNoOverwrite;

			Graphics.Context.MapSubresource(D3DBuffer, mapMode, D3D11.MapFlags.None, out _lockStream);
			return new GorgonDataStream(_lockStream.DataPointer, (int)_lockStream.Length);
		}

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected internal override void UnlockImpl()
		{
			Graphics.Context.UnmapSubresource(D3DBuffer, 0);
			_lockStream.Dispose();
			_lockStream = null;
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
		/// <param name="size">The number of bytes to write.</param>
		protected override void UpdateImpl(GorgonDataStream stream, int offset, int size)
		{
			Graphics.Context.UpdateSubresource(
				new DX.DataBox()
				{
					DataPointer = stream.PositionPointer,
					RowPitch = size
				},
				D3DResource,
				0,
				new D3D11.ResourceRegion()
				{
					Left = offset,
					Right = offset + size,
					Top = 0,
					Bottom = 1,
					Front = 0,
					Back = 1
				});
		}

		/// <summary>
		/// Function to update the entire buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>This method will respect the <see cref="P:GorgonLibrary.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(GorgonDataStream stream)
		{
			UpdateImpl(stream, 0, 0);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonIndexBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		/// <param name="usage">The buffer usage</param>
		/// <param name="size">The size.</param>
		/// <param name="is32Bit">TRUE to use 32 bit indices, FALSE to use 16 bit.</param>
		internal GorgonIndexBuffer(GorgonGraphics graphics, BufferUsage usage, int size, bool is32Bit)
			: base(graphics, usage, size)
		{
			Is32Bit = is32Bit;
		}
		#endregion
	}
}
