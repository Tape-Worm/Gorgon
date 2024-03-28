// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: February 1, 2024 2:27:03 PM
//

using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// Slices a parent stream object into a sub stream object with a smaller boundary.
/// </summary>
/// <remarks>
/// <para>
/// Sometimes it's necessary take a <see cref="Stream"/> and divide it into a sub stream. This object will take an existing stream object and allows the user to set a start position within that stream, and 
/// give a range of readable bytes to break the parent stream into a smaller stream. This gives the protection that a stream provides against buffer overruns all while working within the confines of a 
/// smaller part of the parent stream.
/// </para>
/// <para>
/// When reading or writing, the parent stream will have its position manipulated by this stream to locate the start of this stream plus the current position of this stream. Once these operations 
/// are complete, the parent stream <see cref="Stream.Position"/> will be set back to the original position it was in prior to the read/write operation. This ensures that when this 
/// object is finished its work, the original parent position will remain unaffected by this stream.
/// </para>
/// <para>
/// <note type="information">
/// It is not necessary to call the <see cref="Stream.Dispose(bool)"/> method or the <see cref="Stream.Close"/> method on this object since there is nothing to close 
/// and ownership of the parent stream resides with the creator of that stream (i.e. this type does <i>not</i> take ownership of a parent stream). In fact, closing or disposing of this object 
/// does nothing.
/// </note>
/// </para>
/// <note type="warning">
/// This object is <i>not</i> thread safe. If multiple wrappers are pointing to the same parent stream, and multiple threads use these wrappers, the read/write cursor will be desynchronized. 
/// Because of this limitation, all the asynchronous I/O operations will throw an exception.
/// </note> 
/// </remarks>
public class GorgonSubStream
    : Stream
{
    // Position within the parent stream, offset by the number of bytes specified in the constructor.
    private readonly long _parentOffset;
    // The current position within this stream, relative to the parent position.
    private long _currentPosition;
    // The length of this child stream.
    private long _streamLength;
    // Flag to indicate whether the read can be written to or not.
    private readonly bool _readWrite;

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
    /// <exception cref="NotSupportedException">A class derived from Stream does not support seeking. </exception><exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
    public override long Length => _streamLength;

    /// <summary>
    /// When overridden in a derived class, gets or sets the position within the current stream.
    /// </summary>
    /// <returns>
    /// The current position within the stream.
    /// </returns>
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

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">This object is not thread safe. As such, async methods will not work.</exception>
    public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">This object is not thread safe. As such, async methods will not work.</exception>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">This object is not thread safe. As such, async methods will not work.</exception>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">This object is not thread safe. As such, async methods will not work.</exception>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">This object is not thread safe. As such, async methods will not work.</exception>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">This object is not thread safe. As such, async methods will not work.</exception>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);

    /// <summary>
    /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <param name="value">The byte to write to the stream.</param>
    /// <exception cref="IOException">The stream does not support writing, or the stream is already closed.</exception>
    public override void WriteByte(byte value)
    {
        if (!CanWrite)
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
    public override void Flush() => ParentStream.Flush();

    /// <summary>
    /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
    /// </summary>
    /// <returns>
    /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
    /// </returns>
    /// <exception cref="IOException">The stream does not support reading. </exception>    
    public override int ReadByte()
    {
        if (!CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
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
    /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
    /// </returns>
    /// <param name="buffer">A span of bytes</param>
    /// <exception cref="IOException">The stream does not support reading.</exception>
    public override int Read(Span<byte> buffer)
    {
        if (!CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
        }

        if (buffer.IsEmpty)
        {
            return 0;
        }

        long lastPosition = ParentStream.Position;

        try
        {
            ParentStream.Position = _currentPosition + _parentOffset;

            int result = ParentStream.Read(buffer);

            _currentPosition += result;

            return result;
        }
        finally
        {
            ParentStream.Position = lastPosition;
        }
    }

    /// <summary>
    /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
    /// </returns>
    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and 
    /// (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
    /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
    /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
    /// <exception cref="IOException">The stream does not support reading.</exception>
    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if ((offset + count) > buffer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
        }

        if (!CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
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
    /// Sets the position within the current stream.
    /// </summary>
    /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
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
    /// Sets the length of the current stream.
    /// </summary>
    /// <param name="value">The desired length of the current stream in bytes.</param>
    /// <exception cref="IOException">Thrown if the stream is read only.</exception>
    public override void SetLength(long value)
    {
        if (!CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

        if (value < 0)
        {
            value = 0;
        }

        if (value > (ParentStream.Length - _parentOffset))
        {
            value = ParentStream.Length - _parentOffset;
        }

        _streamLength = value;
    }

    /// <summary>
    /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
    /// </summary>
    /// <param name="buffer">A read only span of bytes.</param>
    /// <exception cref="IOException">The stream does not support writing.</exception>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (!CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

        long lastPosition = ParentStream.Position;

        try
        {
            ParentStream.Position = _parentOffset + _currentPosition;

            ParentStream.Write(buffer);
            _currentPosition += buffer.Length;

            if (_currentPosition > _streamLength)
            {
                _streamLength += buffer.Length;
            }
        }
        finally
        {
            ParentStream.Position = lastPosition;
        }
    }

    /// <summary>
    /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
    /// </summary>
    /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
    /// <param name="count">The number of bytes to be written to the current stream.</param>
    /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
    /// <exception cref="IOException">The stream does not support writing.</exception>
    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if ((offset + count) > buffer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
        }

        if (!CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
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

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSubStream"/> class.
    /// </summary>
    /// <param name="parentStream">The parent of this stream.</param>
    /// <param name="streamStart">[Optional] The position in the parent stream to start at, in bytes.</param>
    /// <param name="streamSize">[Optional] The number of bytes to partition from the parent stream.</param>
    /// <param name="allowWrite">[Optional] <b>true</b> to allow writing to the stream, <b>false</b> to make the stream read-only.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentStream"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="streamStart"/> or the <paramref name="streamSize"/> parameters are less than 0</exception>
    /// <exception cref="ArgumentException">Thrown when the <see cref="Stream.CanSeek"/> property on the parent stream is <b>false</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="parentStream"/> is read only and the <paramref name="allowWrite"/> flag is set to <b>true</b>.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the <paramref name="streamStart"/> is greater than or equal to the length of the <paramref name="parentStream"/>.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="streamSize"/> is greater than the <paramref name="parentStream"/> length minus the <paramref name="streamStart"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will create a sub stream by defining the parent stream, the starting position and size for the sub stream. The resulting stream will start at the offset defined by the 
    /// <paramref name="streamStart"/> parameter, which equates to the sub stream <see cref="Position"/> of 0.
    /// </para>
    /// <para>
    /// When <paramref name="streamStart"/> is not defined, then the sub stream will start at the current <see cref="Stream.Position"/> of the <paramref name="parentStream"/>. If it is defined, then the 
    /// offset will be the <paramref name="parentStream"/>'s current <see cref="Stream.Position"/> plus the number of bytes defined by <paramref name="streamStart"/>.
    /// </para>
    /// <para>
    /// When <paramref name="streamSize"/> is not defined, then the sub stream will take up to the <paramref name="parentStream"/>'s <see cref="Stream.Length"/> minus the <paramref name="streamStart"/>. So 
    /// a <paramref name="streamStart"/> of 0, and no <paramref name="streamSize"/> defined, the entire parent stream (minus the current position) will be covered. If it is defined, then the size will be 
    /// the number of bytes requested.
    /// </para>
    /// </remarks>
    public GorgonSubStream(Stream parentStream, long streamStart = 0, long? streamSize = null, bool allowWrite = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(streamStart, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(streamSize ?? 0, 0);

        if ((streamStart >= parentStream.Length) && (parentStream.Length > 0))
        {
            throw new EndOfStreamException();
        }

        if (!parentStream.CanSeek)
        {
            throw new ArgumentException(Resources.GOR_ERR_STREAM_PARENT_NEEDS_SEEK, nameof(parentStream));
        }

        if ((!parentStream.CanWrite) && (allowWrite))
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

        _parentOffset = parentStream.Position + streamStart;
        _readWrite = allowWrite;
        _streamLength = streamSize ?? parentStream.Length - _parentOffset;

        if (_streamLength > parentStream.Length - _parentOffset)
        {
            throw new EndOfStreamException();
        }

        ParentStream = parentStream;        
    }
}
