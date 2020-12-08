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
// Created: Monday, June 27, 2011 9:01:45 AM
// 
#endregion

using System;
using System.IO;

namespace Gorgon.IO
{
    /// <summary>
    /// A file stream for the Gorgon file system.
    /// </summary>
    public class GorgonFileSystemStream
        : Stream
    {
        #region Variables.
        // Base stream to use.
        private Stream _baseStream;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to close the underlying stream when this stream is closed.
        /// </summary>
        protected bool CloseUnderlyingStream
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the file being read/written.
        /// </summary>
        protected IGorgonVirtualFile FileEntry
        {
            get;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead => _baseStream.CanRead;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite => _baseStream.CanWrite;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek => _baseStream.CanSeek;

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        /// <value></value>
        /// <returns>A value that determines whether the current stream can time out.</returns>
        public override bool CanTimeout => _baseStream.CanTimeout;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <value></value>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length => _baseStream.Length;

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value></value>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out.
        /// </summary>
        /// <value></value>
        /// <returns>A value, in miliseconds, that determines how long the stream will attempt to read before timing out.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="P:System.IO.Stream.ReadTimeout"/> method always throws an <see cref="InvalidOperationException"/>. </exception>
        public override int ReadTimeout
        {
            get => _baseStream.ReadTimeout;
            set => _baseStream.ReadTimeout = value;
        }

        /// <summary>
        /// Gets or sets a value, in milliseconds, that determines how long the stream will attempt to write before timing out.
        /// </summary>
        /// <value></value>
        /// <returns>A value, in milliseconds, that determines how long the stream will attempt to write before timing out.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="P:System.IO.Stream.WriteTimeout"/> method always throws an <see cref="InvalidOperationException"/>. </exception>
        public override int WriteTimeout
        {
            get => _baseStream.WriteTimeout;
            set => _baseStream.WriteTimeout = value;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_baseStream != null)
                {
                    if (CloseUnderlyingStream)
                    {
                        _baseStream.Dispose();
                    }
                }
                _baseStream = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data read from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>
        /// An <see cref="IAsyncResult"/> that represents the asynchronous read, which could still be pending.
        /// </returns>
        /// <exception cref="IOException">Attempted an asynchronous read past the end of the stream, or a disk error occurs. </exception>
        /// <exception cref="ArgumentException">One or more of the arguments is invalid. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="NotSupportedException">The current Stream implementation does not support the read operation. </exception>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _baseStream.BeginRead(buffer, offset, count, callback, state);

        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>
        /// An IAsyncResult that represents the asynchronous write, which could still be pending.
        /// </returns>
        /// <exception cref="IOException">Attempted an asynchronous write past the end of the stream, or a disk error occurs. </exception>
        /// <exception cref="ArgumentException">One or more of the arguments is invalid. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="NotSupportedException">The current Stream implementation does not support the write operation. </exception>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _baseStream.BeginWrite(buffer, offset, count, callback, state);

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>
        /// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 	<paramref name="asyncResult"/> is null. </exception>
        /// <exception cref="ArgumentException">
        /// 	<paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream. </exception>
        /// <exception cref="IOException">The stream is closed or an internal error has occurred.</exception>
        public override int EndRead(IAsyncResult asyncResult) => _baseStream.EndRead(asyncResult);

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
        /// <exception cref="ArgumentNullException">
        /// 	<paramref name="asyncResult"/> is null. </exception>
        /// <exception cref="ArgumentException">
        /// 	<paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream. </exception>
        /// <exception cref="IOException">The stream is closed or an internal error has occurred.</exception>
        public override void EndWrite(IAsyncResult asyncResult) => _baseStream.EndWrite(asyncResult);

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        public override void Flush() => _baseStream.Flush();

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        /// <exception cref="ArgumentNullException">
        /// 	<paramref name="buffer"/> is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int ReadByte() => _baseStream.ReadByte();

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value) => _baseStream.SetLength(value);

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
        /// <exception cref="ArgumentNullException">
        /// 	<paramref name="buffer"/> is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support writing. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void WriteByte(byte value) => _baseStream.WriteByte(value);
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileSystemStream"/> class.
        /// </summary>
        /// <param name="file">File being read/written.</param>
        /// <param name="baseStream">The underlying stream to use for this stream.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseStream"/> or the <paramref name="file"/> parameter is <b>null</b>.</exception>
        protected internal GorgonFileSystemStream(IGorgonVirtualFile file, Stream baseStream)
        {
            CloseUnderlyingStream = true;
            _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            FileEntry = file ?? throw new ArgumentNullException(nameof(file));

            // Reset the position to the beginning.
            if (_baseStream.CanSeek)
            {
                _baseStream.Position = 0;
            }
        }
        #endregion
    }
}
