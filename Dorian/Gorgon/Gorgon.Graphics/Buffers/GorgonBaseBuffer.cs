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
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using GorgonLibrary.IO;
using D3D11 = SharpDX.Direct3D11;

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
		Default = D3D11.ResourceUsage.Default,
		/// <summary>
		/// Can only be read by the GPU, cannot be written to or read from by the CPU, and cannot be written to by the GPU.
		/// </summary>
		/// <remarks>Pre-initialize any buffer created with this usage, or else you will not be able to after it's been created.</remarks>
		Immutable = D3D11.ResourceUsage.Immutable,
		/// <summary>
		/// Allows read access by the GPU and write access by the CPU.
		/// </summary>
		Dynamic = D3D11.ResourceUsage.Dynamic,
		/// <summary>
		/// Allows reading/writing by the CPU and can be copied to a GPU compatiable buffer (but not used directly by the GPU).
		/// </summary>
		Staging = D3D11.ResourceUsage.Staging
	}

	/// <summary>
	/// Flags used when locking the buffer for reading/writing.
	/// </summary>
	[Flags]
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
		: GorgonResource
	{
		#region Variables.
		private readonly int _size;					// Size of the buffer, in bytes.
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
		/// Property to set or return the D3D buffer object.
		/// </summary>
		internal D3D11.Buffer D3DBuffer
		{
			get;
			set;
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
		public override int SizeInBytes
		{
			get 
			{
				return _size;
			}
		}

		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType
		{
			get 
			{
				return ResourceType.Buffer;
			}
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
		protected abstract void InitializeImpl(GorgonDataStream data);

		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		internal void Initialize(GorgonDataStream data)
		{
			InitializeImpl(data);
			D3DBuffer = (D3D11.Buffer)D3DResource;
		}

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
		/// Function to copy the contents of the specified buffer to this buffer.
		/// </summary>
		/// <param name="buffer">Buffer to copy.</param>
		/// <remarks>This is used to copy data from one GPU buffer to another.  The size of the buffers must be the same.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="buffer"/> size is not equal to the size of this buffer.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when this buffer has a usage of Immutable.</exception>
		public void Copy(GorgonBaseBuffer buffer)
		{
#if DEBUG
		    if (buffer.SizeInBytes != SizeInBytes)
		    {
		        throw new ArgumentException(Resources.GORGFX_BUFFER_SIZE_MISMATCH, "buffer");
		    }

		    if (BufferUsage == GorgonLibrary.Graphics.BufferUsage.Immutable)
		    {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_IMMUTABLE);
		    }
#endif
			Graphics.Context.CopyResource(buffer.D3DResource, D3DResource);
		}

		/// <summary>
		/// Function to copy the contents of the specified buffer to this buffer.
		/// </summary>
		/// <param name="buffer">Buffer to copy.</param>
		/// <param name="sourceStartingIndex">Starting byte index to start copying from.</param>
		/// <param name="byteCount">The number of bytes to copy.</param>
		/// <remarks>This is used to copy data from one GPU buffer to another.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="sourceStartingIndex"/> is less than 0 or larger than the size of the source <paramref name="buffer"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="byteCount"/> + sourceStartIndex is greater than the size of the source buffer, or less than 0.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this buffer has a usage of Immutable.</exception>
		public void Copy(GorgonBaseBuffer buffer, int sourceStartingIndex, int byteCount)
		{
			Copy(buffer, sourceStartingIndex, byteCount, 0);
		}

		/// <summary>
		/// Function to copy the contents of the specified buffer to this buffer.
		/// </summary>
		/// <param name="buffer">Buffer to copy.</param>
		/// <param name="sourceStartingIndex">Starting byte index to start copying from.</param>
		/// <param name="byteCount">The number of bytes to copy.</param>
		/// <param name="destOffset">The offset within the destination buffer.</param>
		/// <remarks>This is used to copy data from one GPU buffer to another.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="sourceStartingIndex"/> is less than 0 or larger than the size of the source <paramref name="buffer"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="byteCount"/> + sourceStartIndex is greater than the size of the source buffer, or less than 0.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="destOffset"/> + byteCount is greater than the size of this buffer, or less than 0.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when this buffer has a usage of Immutable.</exception>
		public void Copy(GorgonBaseBuffer buffer, int sourceStartingIndex, int byteCount, int destOffset)
		{
			int endByteIndex = sourceStartingIndex + byteCount;

            GorgonDebug.AssertNull(buffer, "buffer");
            GorgonDebug.AssertParamRange(sourceStartingIndex, 0, buffer.SizeInBytes, "sourceStartingIndex");
            GorgonDebug.AssertParamRange(endByteIndex, 0, buffer.SizeInBytes, "sourceStartingIndex");
            GorgonDebug.AssertParamRange(destOffset, 0, SizeInBytes, "destOffset");

#if DEBUG
		    if (BufferUsage == GorgonLibrary.Graphics.BufferUsage.Immutable)
		    {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_IMMUTABLE);
		    }
#endif
			Graphics.Context.CopySubresourceRegion(buffer.D3DResource, 0, new D3D11.ResourceRegion
			    {
					Top = 0,
					Bottom = 1,
					Left = sourceStartingIndex,
					Right = endByteIndex,
					Front = 0,
					Back = 1
				}, D3DResource, 0, destOffset);
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
		/// <param name="size">The number of bytes to write.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>Please note that constant buffers don't use the <paramref name="offset"/> and <paramref name="size"/> parameters.</para>
		/// <para>This method will respect the <see cref="GorgonLibrary.IO.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(GorgonDataStream stream, int offset, int size)
		{
			GorgonDebug.AssertNull(stream, "stream");

#if DEBUG
		    if (BufferUsage != GorgonLibrary.Graphics.BufferUsage.Default)
		    {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
		    }
#endif

			UpdateImpl(stream, offset, size);
		}

		/// <summary>
		/// Function to unlock a locked buffer.
		/// </summary>
		public void Unlock()
		{
		    if (!IsLocked)
		    {
		        return;
		    }

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
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer is already locked.
		/// <para>-or-</para>
		/// <para>Thrown when the usage for the buffer does not allow the buffer to be locked.</para>		
		/// </exception>		
        /// <exception cref="System.ArgumentException">Thrown when a constant buffer is locked with any other flag other than Discard.
        /// <para>-or-</para>
        /// <para>Thrown when an index/vertex buffer is locked with with a read flag, or a write flag without discard or nooverwrite.</para>
        /// </exception>
        public GorgonDataStream Lock(BufferLockFlags lockFlags)
		{
#if DEBUG
		    if (IsLocked)
		    {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_ALREADY_LOCKED);
		    }

		    if ((BufferUsage == BufferUsage.Default) || (BufferUsage == BufferUsage.Immutable))
		    {
		        throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_USAGE_CANT_LOCK, BufferUsage));
		    }
#endif

			GorgonDataStream result = LockImpl(lockFlags);

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
			: base(graphics)
		{
			_size = size;
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
	}
}
