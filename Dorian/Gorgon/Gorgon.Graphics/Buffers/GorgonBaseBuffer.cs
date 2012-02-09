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
// Created: Monday, January 09, 2012 7:06:42 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D11 = SharpDX.Direct3D11;
using GorgonLibrary.Native;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Buffer usage types.
	/// </summary>
	public enum BufferUsage
	{
		/// <summary>
		/// Allows read/write access to the buffer from the GPU.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Can only be read by the GPU, cannot be written to or read from by the CPU, and cannot be written to by the GPU.
		/// </summary>
		/// <remarks>Pre-initialize any buffer created with this usage, or else you will not be able to after it's been created.</remarks>
		Immutable = 1,
		/// <summary>
		/// Allows read access by the GPU and write access by the CPU.
		/// </summary>
		Dynamic = 2,
		/// <summary>
		/// Allows reading/writing by the CPU and can be copied to a GPU compatiable buffer (but not used directly by the GPU).
		/// </summary>
		Staging = 3
	}

	/// <summary>
	/// Flags used when locking the buffer for reading/writing.
	/// </summary>
	[Flags()]
	public enum BufferLockFlags
	{
		/// <summary>
		/// Lock the buffer for reading.
		/// </summary>
		/// <remarks>This flag is mutually exclusive.</remarks>
		Read = 1,
		/// <summary>
		/// Lock the buffer for writing.
		/// </summary>
		Write = 2,
		/// <summary>
		/// Lock the buffer for writing, but guarantee that we will not overwrite a part of the buffer that's already in use.
		/// </summary>
		NoOverwrite = 4,
		/// <summary>
		/// Lock the buffer for writing, but mark its contents as invalid.
		/// </summary>
		Discard = 8
	}

	/// <summary>
	/// A base buffer object.
	/// </summary>
	public abstract class GorgonBaseBuffer
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the D3D CPU access flags.
		/// </summary>
		internal D3D11.CpuAccessFlags D3DCPUAccessFlags
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the D3D usages.
		/// </summary>
		internal D3D11.ResourceUsage D3DUsage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the Direct 3D buffer.
		/// </summary>
		internal D3D11.Buffer D3DBuffer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the graphics interface that created this buffer.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the buffer is locked or not.
		/// </summary>
		public bool IsLocked
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the usage for this buffer.
		/// </summary>
		public BufferUsage BufferUsage
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
		protected internal abstract void Initialize(GorgonDataStream data);

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>A data stream containing the buffer data.</returns>		
		protected abstract GorgonDataStream LockImpl(BufferLockFlags lockFlags);

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected internal abstract void UnlockImpl();

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
		/// <param name="size">The number of bytes to write.</param>
		protected abstract void UpdateImpl(GorgonDataStream stream, int offset, int size);

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
		/// <param name="size">The number of bytes to write.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>Please note that constant buffers don't use the <paramref name="offset"/> and <paramref name="size"/> parameters.</para>
		/// <para>This method will respect the <see cref="P:GorgonLibrary.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(GorgonDataStream stream, int offset, int size)
		{
			GorgonDebug.AssertNull<GorgonDataStream>(stream, "stream");

			if (BufferUsage != GorgonLibrary.Graphics.BufferUsage.Default)
				throw new InvalidOperationException("Cannot use Update on a non-default buffer.");

			UpdateImpl(stream, offset, size);
		}

		/// <summary>
		/// Function to unlock a locked buffer.
		/// </summary>
		public void Unlock()
		{
			if (!IsLocked)
				return;

			UnlockImpl();

			IsLocked = false;
		}

		/// <summary>
		/// Function to lock the buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">The flags to use when locking the buffer.</param>
		/// <returns>A data stream pointing to the memory used by the buffer.</returns>
		/// <remarks>A data stream locked with this method does not have to be disposed of.  After it is <see cref="M:GorgonLibrary.Graphics.GorgonBaseBuffer.Unlock">unlocked</see>, the memory pointed 
		/// at by the stream will be considered invalid.  However, for the sake of following practice, it is a good idea to call the Dispose method 
		/// on the resulting data stream when finished.
		/// <para>This method only works on buffers with a Dynamic or Staging usage.  Immutable or default buffers will throw an exception when an attempt 
		/// is made to lock them.</para>
		/// <para>Some buffers may raise an exception with locking with certain <paramref name="lockFlags"/>.  This is dependant upon the type of buffer.</para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer is already locked.
		/// <para>-or-</para>
		/// <para>Thrown when the usage for the buffer does not allow the buffer to be locked.</para>		
		/// </exception>		
		/// <exception cref="System.ArgumentException">Thrown when a constant buffer is locked with any other flag other than Discard.
		/// <para>-or-</para>
		/// <para>Thrown when an index/vertex buffer is locked with with a read flag, or a write flag without discard or nooverwrite.</para>
		/// </exception>
		public GorgonDataStream Lock(BufferLockFlags lockFlags)
		{
			GorgonDataStream result = null;

			if (IsLocked)
				throw new InvalidOperationException("The buffer is already locked.");

			if (BufferUsage == BufferUsage.Default)
				throw new InvalidOperationException("A buffer with default usage cannot be locked.");

			if (BufferUsage == BufferUsage.Immutable)
				throw new InvalidOperationException("The buffer is immutable and cannot be locked.");

			result = LockImpl(lockFlags);

			IsLocked = true;

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create this object.</param>
		/// <param name="usage">Usage for this buffer.</param>
		/// <param name="size">The size of the buffer, in bytes.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected GorgonBaseBuffer(GorgonGraphics graphics, BufferUsage usage, int size)			
		{
			GorgonDebug.AssertNull<GorgonGraphics>(graphics, "graphics");

			Size = size;
			Graphics = graphics;
			BufferUsage = usage;

			D3DUsage = (D3D11.ResourceUsage)usage;
			switch (usage)
			{
				case BufferUsage.Dynamic:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.Write;
					break;
				case BufferUsage.Immutable:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.None;
					break;
				case BufferUsage.Staging:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.Write | D3D11.CpuAccessFlags.Read;
					break;
				default:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.None;
					break;
			}			

		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					Graphics.RemoveTrackedObject(this);

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
