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
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold a set of vertices.
	/// </summary>
	public class GorgonVertexBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private DX.DataStream _lockStream;								// Stream used when locking.
		#endregion

		#region Properties.
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
			int bufferIndex = Graphics.Input.VertexBuffers.IndexOf(this);

			// Unbind this buffer if it's bound.
		    if (bufferIndex != -1)
		    {
		        Graphics.Input.VertexBuffers[bufferIndex] = GorgonVertexBufferBinding.Empty;
		    }

		    if (IsLocked)
		    {
		        Unlock();
		    }

		    if (D3DResource == null)
		    {
		        return;
		    }

		    GorgonRenderStatistics.VertexBufferCount--;
		    GorgonRenderStatistics.VertexBufferSize -= D3DBuffer.Description.SizeInBytes;

		    D3DResource.Dispose();
		    D3DResource = null;
		}

		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected override void InitializeImpl(GorgonDataStream data)
		{
			var desc = new D3D11.BufferDescription
			    {
			        BindFlags = D3D11.BindFlags.VertexBuffer,
			        CpuAccessFlags = D3DCPUAccessFlags,
			        OptionFlags = D3D11.ResourceOptionFlags.None,
			        SizeInBytes = SizeInBytes,
			        StructureByteStride = 0,
			        Usage = D3DUsage
			    };

		    if (data == null)
		    {
		        D3DResource = new D3D11.Buffer(Graphics.D3DDevice, desc);
		    }
		    else
		    {
		        long position = data.Position;

		        using(var stream = new DX.DataStream(data.PositionPointer, data.Length - position, true, true))
		        {
		            D3DResource = new D3D11.Buffer(Graphics.D3DDevice, stream, desc);
		        }
		    }

		    GorgonRenderStatistics.VertexBufferCount++;
			GorgonRenderStatistics.VertexBufferSize += ((D3D11.Buffer)D3DResource).Description.SizeInBytes;

#if DEBUG
			D3DResource.DebugName = "Gorgon Vertex Buffer #" + Graphics.GetGraphicsObjectOfType<GorgonVertexBuffer>().Count;
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
			var mapMode = D3D11.MapMode.Write;

#if DEBUG
			// Read is mutually exclusive.
		    if ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read)
		    {
		        throw new ArgumentException(Resources.GORGFX_BUFFER_WRITE_ONLY, "lockFlags");
		    }

		    if (lockFlags == BufferLockFlags.Write)
		    {
		        throw new ArgumentException(Resources.GORGFX_CBUFFER_LOCK_NOT_WRITE_DISCARD, "lockFlags");
		    }
#endif

		    if ((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
		    {
		        mapMode = D3D11.MapMode.WriteDiscard;
		    }

		    if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
		    {
		        mapMode = D3D11.MapMode.WriteNoOverwrite;
		    }

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
				new DX.DataBox
				    {
					DataPointer = stream.PositionPointer,
					RowPitch = size
				},
				D3DResource,
				0,
				new D3D11.ResourceRegion
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
		/// <para>This method will respect the <see cref="P:GorgonLibrary.IO.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(GorgonDataStream stream)
		{
            Update(stream, 0, (int)(stream.Length - stream.Position));
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		/// <param name="usage">The buffer usage</param>
		/// <param name="size">The size.</param>
		internal GorgonVertexBuffer(GorgonGraphics graphics, BufferUsage usage, int size)
			: base(graphics, usage, size)
		{
		}
		#endregion
	}
}
