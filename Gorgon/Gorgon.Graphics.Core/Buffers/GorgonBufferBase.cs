#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 9, 2016 3:54:15 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The type of data to be stored in the buffer.
    /// </summary>
    public enum BufferType
    {
        /// <summary>
        /// A generic raw buffer filled with byte data.
        /// </summary>
        Generic = 0,

        /// <summary>
        /// A constant buffer used to send data to a shader.
        /// </summary>
        Constant = 1,

        /// <summary>
        /// A vertex buffer used to hold vertex information.
        /// </summary>
        Vertex = 2,

        /// <summary>
        /// An index buffer used to hold index information.
        /// </summary>
        Index = 3,

        /// <summary>
        /// A structured buffer used to hold structured data.
        /// </summary>
        Structured = 4,

        /// <summary>
        /// A raw buffer used to hold raw byte data.
        /// </summary>
        Raw = 5,

        /// <summary>
        /// An indirect argument buffer.
        /// </summary>
        IndirectArgument = 6
    }

    /// <summary>
    /// A base class for buffers.
    /// </summary>
    public abstract class GorgonBufferBase
        : GorgonResource
    {
        #region Variables.
        // The address returned by the lock on the buffer.
        private GorgonPointerAlias _lockAddress;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the usage flags for the buffer.
        /// </summary>
        protected abstract D3D11.ResourceUsage Usage
        {
            get;
        }

        /// <summary>
        /// Property to return the log used for debugging.
        /// </summary>
        protected IGorgonLog Log
        {
            get;
        }

        /// <summary>
        /// Property to return the D3D 11 buffer.
        /// </summary>
        protected internal D3D11.Buffer D3DBuffer
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the type of data in the resource.
        /// </summary>
        public override ResourceType ResourceType => ResourceType.Buffer;

        /// <summary>
        /// Property to return the type of buffer.
        /// </summary>
        public BufferType BufferType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Property to return whether this buffer is locked for reading/writing or not.
        /// </summary>
        public bool IsLocked
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
        /// </para>
        /// </remarks>
        public override void Dispose()
        {
            // If we're locked, then unlock the buffer before destroying it.
            if ((IsLocked) && (_lockAddress != null) && (!_lockAddress.IsDisposed))
            {
                Unlock(ref _lockAddress);

                // Because the pointer is an alias, we don't really NEED to call this, but just for consistency we'll do so anyway.
                _lockAddress.Dispose();
            }

            base.Dispose();
        }

        /// <summary>
        /// Function to unlock a previously locked buffer.
        /// </summary>
        /// <param name="lockPointer">The pointer returned by the <see cref="Lock"/> method.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="lockPointer"/> was not created by the <see cref="Lock"/> method on this instance.</exception>
        /// <remarks>
        /// <para>
        /// Use this to unlock this buffer when it was previously locked by the <see cref="Lock"/> method. Buffers that were previously locked must always call this method or else the data passed to the 
        /// buffer will not be updated on the GPU. If the buffer was not locked, then this method does nothing. 
        /// </para>
        /// <para>
        /// The <paramref name="lockPointer"/> passed to this method is passed by reference so that it will be invalidated back to the calling application to avoid issues with reuse of an invalid pointer. 
        /// </para>
        /// <para>
        /// If the <paramref name="lockPointer"/> is was not created by this instance, then an exception will be thrown.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="Lock"/>
        public void Unlock(ref GorgonPointerAlias lockPointer)
        {
            if ((!IsLocked) || (lockPointer == null))
            {
                return;
            }

#if DEBUG
            if (lockPointer != _lockAddress)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_BUFFER_LOCK_NOT_VALID, nameof(lockPointer));
            }
#endif

            Graphics.D3DDeviceContext.UnmapSubresource(D3DBuffer, 0);

            // Reset the lock pointer back to null so applications can't reuse it.
            lockPointer = null;
            IsLocked = false;
        }

        /// <summary>
        /// Function to lock a buffer for reading or writing.
        /// </summary>
        /// <param name="mode">The type of access to the buffer data.</param>
        /// <returns>A <see cref="GorgonPointerAlias"/> used to read or write the data in the buffer.</returns>
        /// <exception cref="NotSupportedException">Thrown when if buffer does not have a usage of <c>Dynamic</c> or <c>Staging</c>.
        /// <para>-or-</para>
        /// <para>Thrown when if buffer does not have a usage of <c>Staging</c>, and the <paramref name="mode"/> is set to <c>Read</c> or <c>ReadWrite</c>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is already locked.</exception>
        /// <remarks>
        /// <para>
        /// This will lock the buffer so that the CPU can access the data within it. Because locks/unlocks can potentially be performance intensive, it is best practice to lock the buffer, do the work and 
        /// <see cref="Unlock"/> immediately. Holding a lock for a long time may cause performance issues.
        /// </para>
        /// <para>
        /// Unlike the <see cref="O:Gorgon.Graphics.GorgonBufferCommon.Update{T}">Update&lt;T&gt;</see> methods, this allows the CPU to change portions of the buffer every frame with little performance 
        /// penalty (this, of course, is dependent upon drivers, hardware, etc...). It also allows reading from the buffer if it was created with a usage of <c>Staging</c>.
        /// </para>
        /// <para>
        /// When the lock method returns, it returns a <see cref="GorgonPointerAlias"/> containing a pointer to the CPU memory that contains the buffer data. Applications can use this to access the 
        /// buffer data.
        /// </para>
        /// <para>
        /// The lock access is affected by the <paramref name="mode"/> parameter. A value of <c>Read</c> or <c>ReadWrite</c> will allow read access to the buffer data but only if the buffer has a 
        /// usage of <c>Staging</c>. Applications can use one of the <c>Write</c> flags to write to the buffer. For <c>Dynamic</c> buffers, it is ideal to use 
        /// <c>WriteNoOverwrite</c> to inform the GPU that you will not be overwriting parts of the buffer still being used for rendering by the GPU. If this cannot be guaranteed (for example, writing to 
        /// the beginning of the buffer at the start of a frame), then applications should use the <c>WriteDiscard</c> to instruct the GPU that the contents of the buffer are now invalidated and it will be 
        /// refreshed with new data entirely.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// When a buffer is locked, it <b><u>must</u></b> be unlocked with a call to <see cref="Unlock"/>. Failure to do so will impair performance greatly, and will keep the contents of the buffer 
        /// on the GPU from being updated.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public GorgonPointerAlias Lock(D3D11.MapMode mode)
        {
#if DEBUG
            if ((Usage != D3D11.ResourceUsage.Dynamic) && (Usage != D3D11.ResourceUsage.Staging))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_BUFFER_LOCK_NOT_DYNAMIC, Name, Usage));
            }

            if ((Usage != D3D11.ResourceUsage.Staging) && ((mode == D3D11.MapMode.Read) || (mode == D3D11.MapMode.ReadWrite)))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_BUFFER_ERR_WRITE_ONLY, Name, Usage));
            }

            if (IsLocked)
            {
                throw new InvalidOperationException(Resources.GORGFX_ERR_BUFFER_ALREADY_LOCKED);
            }
#endif

            Graphics.D3DDeviceContext.MapSubresource(D3DBuffer, mode, D3D11.MapFlags.None, out DX.DataStream stream);

            if (_lockAddress == null)
            {
                _lockAddress = new GorgonPointerAlias(stream.DataPointer, stream.Length);
            }
            else
            {
                _lockAddress.AliasPointer(stream.DataPointer, stream.Length);
            }

            stream.Dispose();

            IsLocked = true;
            return _lockAddress;
        }

        /// <summary>
        /// Function to copy the contents of this buffer into a destination buffer.
        /// </summary>
        /// <param name="buffer">The destination buffer that will receive the data.</param>
        /// <param name="sourceOffset">[Optional] Starting byte index to start copying from.</param>
        /// <param name="byteCount">[Optional] The number of bytes to copy.</param>
        /// <param name="destOffset">[Optional] The offset within the destination buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/> plus the <paramref name="byteCount"/> is less than 0 or larger than the size of this buffer.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destOffset"/> plus the <paramref name="byteCount"/> is less than 0 or larger than the size of the destination <paramref name="buffer"/>.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown when <paramref name="buffer"/> has a resource usage of <c>Immutable</c>.</exception>
        /// <remarks>
        /// <para>
        /// Use this method to copy the contents of this buffer into another.
        /// </para> 
        /// <para>
        /// The source and destination buffer offsets must fit within their range of their allocated space, as must the <paramref name="byteCount"/>. Otherwise, an exception will be thrown. Also, the 
        /// destination buffer must not be <c>Immutable</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, this method will only throw exceptions when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void CopyTo(GorgonBufferBase buffer, int sourceOffset = 0, int byteCount = 0, int destOffset = 0)
        {
            buffer.ValidateObject(nameof(buffer));

            sourceOffset = sourceOffset.Max(0);
            destOffset = destOffset.Max(0);

            if (byteCount < 1)
            {
                byteCount = SizeInBytes.Min(buffer.SizeInBytes);
            }

            int sourceByteIndex = sourceOffset + byteCount;
            int destByteIndex = destOffset + byteCount;

            sourceByteIndex.ValidateRange(nameof(sourceOffset), 0, buffer.SizeInBytes);
            destByteIndex.ValidateRange(nameof(destOffset), 0, buffer.SizeInBytes);

#if DEBUG
            if (buffer.D3DBuffer.Description.Usage == D3D11.ResourceUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_ERR_BUFFER_IS_IMMUTABLE);
            }
#endif
            Graphics.D3DDeviceContext.CopySubresourceRegion(buffer.D3DResource,
                                                            0,
                                                            new D3D11.ResourceRegion
                                                            {
                                                                Top = 0,
                                                                Bottom = 1,
                                                                Left = sourceOffset,
                                                                Right = sourceByteIndex,
                                                                Front = 0,
                                                                Back = 1
                                                            },
                                                            D3DResource,
                                                            0,
                                                            destOffset);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferBase" /> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="name">Name of this buffer.</param>
        /// <param name="log">[Optional] The log interface used for debug logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="name"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        protected GorgonBufferBase(GorgonGraphics graphics, string name, IGorgonLog log)
            : base(graphics, name)
        {
            Log = log ?? GorgonLogDummy.DefaultInstance;
        }
        #endregion
    }
}