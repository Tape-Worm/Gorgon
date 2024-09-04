
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Sunday, July 03, 2011 9:26:17 AM
// 

using Gorgon.IO.GorPack.Properties;
using ICSharpCode.SharpZipLib.BZip2;

namespace Gorgon.IO.GorPack;

/// <summary>
/// A stream used to read Gorgon bzip2 pack files
/// </summary>
internal class GorPackFileStream
    : GorgonFileSystemStream
{

    // Flag to indicate that the object was disposed.
    private bool _disposed;
    // Input stream for the bzip file.
    private readonly Stream _bzipStream;
    // Position in the stream.		    
    private long _position;
    // Base position in the stream.
    private readonly long _basePosition;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => _bzipStream.CanSeek;

    /// <inheritdoc/>
    public override bool CanTimeout => _bzipStream.CanTimeout;

    /// <inheritdoc/>
    public override long Length
    {
        get;
    }

    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value >= Length)
            {
                value = Length;
            }

            _position = value;

            _bzipStream.Position = _basePosition + _position;
        }
    }

    /// <inheritdoc/>
    public override int WriteTimeout
    {
        get => _bzipStream.WriteTimeout;
        set => _bzipStream.WriteTimeout = value;
    }

    /// <inheritdoc/>
    public override int ReadTimeout
    {
        get => _bzipStream.ReadTimeout;
        set => _bzipStream.ReadTimeout = value;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Close();
            _bzipStream.Dispose();
        }

        _disposed = true;

        base.Dispose(disposing);
    }

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override void Flush() => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override void WriteByte(byte value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => _bzipStream.BeginRead(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public override int EndRead(IAsyncResult asyncResult) => _bzipStream.EndRead(asyncResult);

    /// <inheritdoc/>
    public override void Close() => _bzipStream.Close();

    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize) => _bzipStream.CopyTo(destination, bufferSize);

    /// <inheritdoc/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => _bzipStream.CopyToAsync(destination, bufferSize, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _bzipStream.ReadAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        // If we're at the end of the stream, then leave.
        if (_position >= Length)
        {
            return 0;
        }

        int result = _bzipStream.Read(buffer);

        _position += result;

        return result;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = offset;
                break;
            case SeekOrigin.Current:
                _position += offset;
                break;
            case SeekOrigin.End:
                _position = Length + offset;
                break;
        }

        if (_position > Length)
        {
            throw new EndOfStreamException(Resources.GORFS_GORPACK_ERR_EOS);
        }

        if (_position < 0)
        {
            throw new EndOfStreamException(Resources.GORFS_GORPACK_ERR_BOS);
        }

        _bzipStream.Position = _basePosition + _position;

        return _position;
    }

    /// <summary>
    /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
    /// </summary>
    /// <returns>
    /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
    /// </returns>
    /// <exception cref="NotSupportedException">The stream does not support reading. </exception>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override int ReadByte()
    {
        if (_position >= Length)
        {
            return -1;
        }

        int result = _bzipStream.ReadByte();

        _position += result;

        return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorPackFileStream"/> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="stream">The stream.</param>
    /// <param name="compressedSize">Compression size for the file.</param>
    internal GorPackFileStream(IGorgonVirtualFile file, Stream stream, long? compressedSize)
        : base(file, stream)
    {
        stream.Position = file.PhysicalFile.Offset;     // Set the offset here.

        _bzipStream = compressedSize is not null ? new BZip2InputStream(stream) : stream;
        Length = file.Size;
        _basePosition = stream.Position;
    }
}
