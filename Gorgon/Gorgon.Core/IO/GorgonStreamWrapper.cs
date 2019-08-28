#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 14, 2015 11:15:04 AM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// Wraps a parent stream object into a stream object with a smaller boundary.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sometimes it's necessary take a <see cref="Stream"/> and divide it into sections to ensure there are no overruns. This object will take an existing stream object and allows the user to 
    /// set a start position within that stream, and give a range of readable bytes to break the parent stream into a smaller stream. This gives the protection that a stream provides against 
    /// buffer overruns all while working within the confines of a smaller part of the parent stream.
	/// </para>
	/// <para>
	/// When reading or writing, the parent stream will have its position manipulated by this stream to locate the start of this stream plus the current position of this stream. Once these operations 
	/// are complete, the parent stream <see cref="P:System.IO.Stream.Position"/> will be set back to the original position it was in prior to the read/write operation. This ensures that when this 
	/// object is finished its work, the original parent position will remain unaffected by this stream.
	/// </para>
	/// <note>
	/// It is not necessary to call the <see cref="M:System.IO.Stream.Dispose"/> method or the <see cref="M:System.IO.Stream.Close"/> method on this object since there is nothing to close 
	/// and ownership of the parent stream resides with the creator of that stream (i.e. this type does <i>not</i> take ownership of a parent stream). In fact, closing or disposing of this object 
	/// does nothing.
	/// </note>
	/// <note type="caution">
	/// This object is <i>not</i> thread safe. If multiple wrappers are pointing to the same parent stream, and multiple threads use these wrappers, the read/write cursor will be desynchronized. 
	/// Because of this limitation, all the asynchronous I/O operations will throw an exception.
	/// </note> 
    /// </remarks>
    public class GorgonStreamWrapper
        : Stream
    {
        #region Variables.
        // Position within the parent stream, offset by the number of bytes specified in the constructor.
        private readonly long _parentOffset;
        // The current position within this stream, relative to the parent position.
        private long _currentPosition;
        // The length of this child stream.
        private long _streamLength;
        // Flag to indicate whether the read can be written to or not.
        private readonly bool _readWrite;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the parent of this stream.
        /// </summary>
        public Stream ParentStream
        {
            get;
        }

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        /// <returns>
        /// A value that determines whether the current stream can time out.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool CanTimeout => ParentStream.CanTimeout;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead => ParentStream.CanRead;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek => true;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite => ParentStream.CanWrite && _readWrite;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Length => _streamLength;

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Position
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;

                if (_currentPosition < 0)
                {
                    _currentPosition = 0;
                }

                if (_currentPosition > _streamLength)
                {
                    _currentPosition = _streamLength;
                }
            }
        }
        #endregion

        #region Methods.		

        /// <summary>
        /// Begins an asynchronous read operation. (Consider using <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)"/> instead; see the Remarks section.)
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> that represents the asynchronous read, which could still be pending.
        /// </returns>
        /// <param name="buffer">The buffer to read the data into. </param><param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data read from the stream. </param><param name="count">The maximum number of bytes to read. </param><param name="callback">An optional asynchronous callback, to be called when the read is complete. </param><param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests. </param><exception cref="T:System.IO.IOException">Attempted an asynchronous read past the end of the stream, or a disk error occurs. </exception><exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><exception cref="T:System.NotSupportedException">The current Stream implementation does not support the read operation. </exception><filterpriority>2</filterpriority>
        /// <exception cref="NotSupportedException">This stream does not support asynchronous I/O with this method.</exception>
        [Obsolete("Use the ReadAsync method instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

        /// <summary>
        /// Begins an asynchronous write operation. (Consider using <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)"/> instead; see the Remarks section.)
        /// </summary>
        /// <returns>
        /// An IAsyncResult that represents the asynchronous write, which could still be pending.
        /// </returns>
        /// <param name="buffer">The buffer to write data from. </param><param name="offset">The byte offset in <paramref name="buffer"/> from which to begin writing. </param><param name="count">The maximum number of bytes to write. </param><param name="callback">An optional asynchronous callback, to be called when the write is complete. </param><param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests. </param><exception cref="T:System.IO.IOException">Attempted an asynchronous write past the end of the stream, or a disk error occurs. </exception><exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><exception cref="T:System.NotSupportedException">The current Stream implementation does not support the write operation. </exception><filterpriority>2</filterpriority>
        /// <exception cref="NotSupportedException">This stream does not support asynchronous I/O with this method.</exception>
        [Obsolete("Use the WriteAsync method instead.")]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

        /// <summary>
        /// Waits for the pending asynchronous read to complete. (Consider using <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)"/> instead; see the Remarks section.)
        /// </summary>
        /// <returns>
        /// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
        /// </returns>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish. </param><exception cref="T:System.ArgumentNullException"><paramref name="asyncResult"/> is null. </exception><exception cref="T:System.ArgumentException">A handle to the pending read operation is not available.-or-The pending operation does not support reading.</exception><exception cref="T:System.InvalidOperationException"><paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream.</exception><exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception><filterpriority>2</filterpriority>
        /// <exception cref="NotSupportedException">This stream does not support asynchronous I/O with this method.</exception>
        [Obsolete("Use the ReadAsync method instead.")]
        public override int EndRead(IAsyncResult asyncResult) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

        /// <summary>
        /// Ends an asynchronous write operation. (Consider using <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)"/> instead; see the Remarks section.)
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request. </param><exception cref="T:System.ArgumentNullException"><paramref name="asyncResult"/> is null. </exception><exception cref="T:System.ArgumentException">A handle to the pending write operation is not available.-or-The pending operation does not support writing.</exception><exception cref="T:System.InvalidOperationException"><paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream.</exception><exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception><filterpriority>2</filterpriority>
        /// <exception cref="NotSupportedException">This stream does not support asynchronous I/O with this method.</exception>
        [Obsolete("Use the WriteAsync method instead.")]
        public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous flush operation.
        /// </returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"/>.</param><exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
        public override Task FlushAsync(CancellationToken cancellationToken) => ParentStream.FlushAsync(cancellationToken);

        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous copy operation.
        /// </returns>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param><param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 81920.</param><param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="destination"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="bufferSize"/> is negative or zero.</exception><exception cref="T:System.ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception><exception cref="T:System.NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!destination.CanWrite)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            if (!CanRead)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
            }

            long lastPosition = ParentStream.Position;

            try
            {
                ParentStream.Position = _currentPosition + _parentOffset;

                await ParentStream.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);

                _currentPosition = Length;
            }
            finally
            {
                ParentStream.Position = lastPosition;
            }
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the <paramref name="count"/> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. 
        /// </returns>
        /// <param name="buffer">The buffer to write the data into.</param><param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param><param name="count">The maximum number of bytes to read.</param><param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception><exception cref="T:System.NotSupportedException">The stream does not support reading.</exception><exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception><exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation. </exception>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
            }

            if (!ParentStream.CanRead)
            {
                throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
            }

            int actualCount = count;

            if (actualCount > _streamLength - _currentPosition)
            {
                actualCount = (int)(_streamLength - _currentPosition);
            }

            if (actualCount <= 0)
            {
                return 0;
            }

            long lastPosition = ParentStream.Position;

            try
            {
                ParentStream.Position = _currentPosition + _parentOffset;

                int result = await ParentStream.ReadAsync(buffer, offset, actualCount, cancellationToken).ConfigureAwait(false);

                _currentPosition += result;

                return result;
            }
            finally
            {
                ParentStream.Position = lastPosition;
            }
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        /// <param name="buffer">The buffer to write data from.</param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.</param><param name="count">The maximum number of bytes to write.</param><param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception><exception cref="T:System.NotSupportedException">The stream does not support writing.</exception><exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception><exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation. </exception>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
            }

            if (!ParentStream.CanWrite)
            {
                throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            long lastPosition = ParentStream.Position;

            try
            {
                ParentStream.Position = _parentOffset + _currentPosition;

                await ParentStream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);

                _currentPosition += count;

                if (_currentPosition > _streamLength)
                {
                    _streamLength += count;
                }
            }
            finally
            {
                ParentStream.Position = lastPosition;
            }
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
        public override void WriteByte(byte value)
        {
            if (!ParentStream.CanWrite)
            {
                throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            long lastPosition = ParentStream.Position;

            try
            {
                ParentStream.Position = _parentOffset + _currentPosition;

                ParentStream.WriteByte(value);

                ++_currentPosition;

                // Expand the size of the stream if larger than what we currently have.
                if (_currentPosition > _streamLength)
                {
                    ++_streamLength;
                }
            }
            finally
            {
                ParentStream.Position = lastPosition;
            }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
        public override void Flush() => ParentStream.Flush();

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
        public override int ReadByte()
        {
            if (!ParentStream.CanRead)
            {
                throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
            }

            // Don't advance any further than our boundaries.
            if ((_currentPosition + 1) > _streamLength)
            {
                return -1;
            }

            long lastPosition = ParentStream.Position;

            try
            {
                ParentStream.Position = _currentPosition + _parentOffset;

                int result = ParentStream.ReadByte();

                _currentPosition++;

                return result;
            }
            finally
            {
                ParentStream.Position = lastPosition;
            }
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. </param><param name="count">The maximum number of bytes to be read from the current stream. </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
            }

            if (!ParentStream.CanRead)
            {
                throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
            }

            int actualCount = count;

            if (actualCount > _streamLength - _currentPosition)
            {
                actualCount = (int)(_streamLength - _currentPosition);
            }

            if (actualCount <= 0)
            {
                return 0;
            }

            long lastPosition = ParentStream.Position;

            try
            {
                ParentStream.Position = _currentPosition + _parentOffset;

                int result = ParentStream.Read(buffer, offset, actualCount);

                _currentPosition += result;

                return result;
            }
            finally
            {
                ParentStream.Position = lastPosition;
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _currentPosition = offset;
                    break;
                case SeekOrigin.End:
                    // We're adding the offset because offset should be a negative value.
                    _currentPosition = _streamLength + offset;
                    break;
                case SeekOrigin.Current:
                    _currentPosition += offset;
                    break;
            }

            if (_currentPosition < 0)
            {
                _currentPosition = 0;
            }

            if (_currentPosition > _streamLength)
            {
                _currentPosition = _streamLength;
            }

            return _currentPosition;
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            if (value < 0)
            {
                value = 0;
            }

            if (value > (ParentStream.Length - ParentStream.Position))
            {
                value = ParentStream.Length - ParentStream.Position;
            }

            _streamLength = value;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param><param name="count">The number of bytes to be written to the current stream. </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/>  is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception><exception cref="T:System.IO.IOException">An I/O error occured, such as the specified file cannot be found.</exception><exception cref="T:System.NotSupportedException">The stream does not support writing.</exception><exception cref="T:System.ObjectDisposedException"><see cref="M:System.IO.Stream.Write(System.Byte[],System.Int32,System.Int32)"/> was called after the stream was closed.</exception><filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
            }

            if (!ParentStream.CanWrite)
            {
                throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            long lastPosition = ParentStream.Position;

            try
            {
                ParentStream.Position = _parentOffset + _currentPosition;

                ParentStream.Write(buffer, offset, count);
                _currentPosition += count;

                if (_currentPosition > _streamLength)
                {
                    _streamLength += count;
                }
            }
            finally
            {
                ParentStream.Position = lastPosition;
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStreamWrapper"/> class.
        /// </summary>
        /// <param name="parentStream">The parent of this stream.</param>
        /// <param name="streamStart">The position in the parent stream to start at, in bytes.</param>
        /// <param name="streamSize">The number of bytes to partition from the parent stream.</param>
        /// <param name="allowWrite">[Optional] <b>true</b> to allow writing to the stream, <b>false</b> to make the stream read-only.</param>
        /// <remarks>
        /// If writing to the stream, the <paramref name="streamSize"/> is ignored.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentStream"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="streamStart"/> or the <paramref name="streamSize"/> parameters are less than 0</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Stream.CanSeek"/> property on the parent stream is <b>false</b>.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="parentStream"/> is read only and the <paramref name="allowWrite"/> flag is set to <b>true</b>.</para>
        /// </exception>
        public GorgonStreamWrapper(Stream parentStream, long streamStart, long streamSize, bool allowWrite = true)
        {
            if (parentStream == null)
            {
                throw new ArgumentNullException(nameof(parentStream));
            }

            if ((streamStart < 0) || (streamSize < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(streamStart), Resources.GOR_ERR_STREAM_POS_OUT_OF_RANGE);
            }

            if (!parentStream.CanSeek)
            {
                throw new ArgumentException(Resources.GOR_ERR_STREAM_PARENT_NEEDS_SEEK, nameof(parentStream));
            }

            if ((!parentStream.CanWrite) && (_readWrite))
            {
                throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_READONLY, nameof(parentStream));
            }

            _readWrite = allowWrite;
            _streamLength = streamSize;
            ParentStream = parentStream;
            _parentOffset = ParentStream.Position + streamStart;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStreamWrapper"/> class.
        /// </summary>
        /// <param name="parentStream">The parent of this stream.</param>
        /// <param name="streamStart">The position in the parent stream to start at, in bytes.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentStream"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="streamStart"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>The <paramref name="streamStart"/> is larger than or equal to the size of the <paramref name="parentStream"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Stream.CanSeek"/> property on the parent stream is <b>false</b>.</exception>
        public GorgonStreamWrapper(Stream parentStream, long streamStart)
            : this(parentStream, streamStart, parentStream?.Length - streamStart ?? 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStreamWrapper"/> class.
        /// </summary>
        /// <param name="parentStream">The parent of this stream.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentStream"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Stream.CanSeek"/> property on the parent stream is <b>false</b>.</exception>
        public GorgonStreamWrapper(Stream parentStream)
            : this(parentStream, 0, parentStream?.Length ?? 0)
        {
        }
        #endregion
    }
}
