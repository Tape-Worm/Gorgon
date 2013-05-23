#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, May 20, 2013 11:16:34 PM
// 
#endregion

using System;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using GorgonLibrary.IO;

// TODO: Add Save() for buffers.
namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Base buffer interface.
	/// </summary>
	public abstract class GorgonBaseBuffer
		: GorgonResource
	{
		#region Variables.
		private DX.DataStream _lockStream;			// Stream used to lock the buffer for read/write.
		#endregion
		
		#region Properties.
		/// <summary>
		/// Property to set or return the D3D CPU access flags.
		/// </summary>
		internal D3D.CpuAccessFlags D3DCPUAccessFlags
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the D3D usages.
		/// </summary>
		internal D3D.ResourceUsage D3DUsage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the D3D buffer object.
		/// </summary>
		internal D3D.Buffer D3DBuffer
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
			protected set;
		}

		/// <summary>
		/// Property to return the settings for the buffer.
		/// </summary>
		public IBufferSettings Settings
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the size of the resource, in bytes.
		/// </summary>
		public override int SizeInBytes
		{
			get
			{
				return Settings.SizeInBytes;
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected abstract void InitializeImpl(GorgonDataStream data);

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>A data stream containing the buffer data.</returns>		
		protected virtual GorgonDataStream LockImpl(BufferLockFlags lockFlags)
		{
#if DEBUG
			if (((lockFlags & BufferLockFlags.Discard) != BufferLockFlags.Discard)
				|| ((lockFlags & BufferLockFlags.Write) != BufferLockFlags.Write))
			{
				throw new ArgumentException(Resources.GORGFX_BUFFER_LOCK_NOT_WRITE_DISCARD, "lockFlags");
			}

			if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
			{
				throw new ArgumentException(Resources.GORGFX_BUFFER_NO_OVERWRITE_NOT_VALID, "lockFlags");
			}
#endif

			Graphics.Context.MapSubresource(D3DBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.None, out _lockStream);

			return new GorgonDataStream(_lockStream.DataPointer, (int)_lockStream.Length);
		}

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected virtual void UnlockImpl()
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
		protected virtual void UpdateImpl(GorgonDataStream stream, int offset, int size)
		{
			Graphics.Context.UpdateSubresource(
				new DX.DataBox
				{
					DataPointer = (stream.PositionPointer + offset),
					RowPitch = size,
					SlicePitch = 0
				},
				D3DResource);
		}

        /// <summary>
        /// Function to retrieve the staging buffer for this buffer.
        /// </summary>
        /// <returns>The staging buffer for this buffer.</returns>
	    protected abstract GorgonBaseBuffer GetStagingBufferImpl();

		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		internal void Initialize(GorgonDataStream data)
		{
			Gorgon.Log.Print("Creating {0} {1}...", LoggingLevel.Verbose, GetType().FullName, Name);
			InitializeImpl(data);
			D3DBuffer = (D3D.Buffer)D3DResource;
		}

        /// <summary>
        /// Function to return this buffer as a staging buffer.
        /// </summary>
        /// <typeparam name="T">Type of buffer.</typeparam>
        /// <returns>The new staging buffer.</returns>
        public T GetStagingBuffer<T>()
            where T : GorgonBaseBuffer
        {
#if DEBUG
            if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (Settings.Usage != BufferUsage.Staging))
            {
                throw new NotSupportedException("Feature level SM2_a_b video devices cannot build a staging buffer from a non-staging buffer.");
            }

            if (Settings.Usage == BufferUsage.Immutable)
            {
                throw new NotSupportedException("Textures with a usage of Immutable cannot be used as staging textures.");
            }
#endif

            return (T)GetStagingBufferImpl();
        }

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

			if (Settings.Usage == GorgonLibrary.Graphics.BufferUsage.Immutable)
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
			if (Settings.Usage == GorgonLibrary.Graphics.BufferUsage.Immutable)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_IMMUTABLE);
			}
#endif
			Graphics.Context.CopySubresourceRegion(buffer.D3DResource, 0, new D3D.ResourceRegion
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
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>This method will respect the <see cref="GorgonLibrary.IO.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(GorgonDataStream stream)
		{
			GorgonDebug.AssertNull(stream, "stream");

#if DEBUG
			if (Settings.Usage != GorgonLibrary.Graphics.BufferUsage.Default)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
			}
#endif

			UpdateImpl(stream, 0, (int)(stream.Length - stream.Position));
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

			if ((Settings.Usage == BufferUsage.Default) || (Settings.Usage == BufferUsage.Immutable))
			{
				throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_BUFFER_USAGE_CANT_LOCK, Settings.Usage));
			}
#endif

			GorgonDataStream result = LockImpl(lockFlags);

			IsLocked = true;

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseBuffer" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create this object.</param>
		/// <param name="name">Name of the buffer.</param>
		/// <param name="settings">Settings for the buffer.</param>
		protected GorgonBaseBuffer(GorgonGraphics graphics, string name, IBufferSettings settings)
			: base(graphics, name)
		{
			Settings = settings;

			D3DUsage = (D3D.ResourceUsage)Settings.Usage;

			// Determine access rights.
			switch (Settings.Usage)
			{
				case BufferUsage.Dynamic:
					D3DCPUAccessFlags = D3D.CpuAccessFlags.Write;
					break;
				case BufferUsage.Immutable:
					D3DCPUAccessFlags = D3D.CpuAccessFlags.None;
					break;
				case BufferUsage.Staging:
					D3DCPUAccessFlags = D3D.CpuAccessFlags.Write | D3D.CpuAccessFlags.Read;
					break;
				default:
					D3DCPUAccessFlags = D3D.CpuAccessFlags.None;
					break;
			}
		}
		#endregion
	}
}
