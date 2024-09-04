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
// Created: Monday, June 27, 2011 9:01:45 AM
// 

namespace Gorgon.IO;

/// <summary>
/// A file stream for the Gorgon file system
/// </summary>
public class GorgonFileSystemStream
    : Stream
{
    // Base stream to use.
    private Stream _baseStream = Null;

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

    /// <inheritdoc/>
    public override bool CanRead => _baseStream.CanRead;

    /// <inheritdoc/>
    public override bool CanWrite => _baseStream.CanWrite;

    /// <inheritdoc/>
    public override bool CanSeek => _baseStream.CanSeek;

    /// <inheritdoc/>
    public override bool CanTimeout => _baseStream.CanTimeout;

    /// <inheritdoc/>
    public override long Length => _baseStream.Length;

    /// <inheritdoc/>
    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    /// <inheritdoc/>
    public override int ReadTimeout
    {
        get => _baseStream.ReadTimeout;
        set => _baseStream.ReadTimeout = value;
    }

    /// <inheritdoc/>
    public override int WriteTimeout
    {
        get => _baseStream.WriteTimeout;
        set => _baseStream.WriteTimeout = value;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_baseStream is not null)
            {
                if (CloseUnderlyingStream)
                {
                    _baseStream.Dispose();
                }
            }

            _baseStream = Null;
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => _baseStream.BeginRead(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => _baseStream.BeginWrite(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public override int EndRead(IAsyncResult asyncResult) => _baseStream.EndRead(asyncResult);

    /// <inheritdoc/>
    public override void EndWrite(IAsyncResult asyncResult) => _baseStream.EndWrite(asyncResult);

    /// <inheritdoc/>
    public override void Flush() => _baseStream.Flush();

    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => _baseStream.FlushAsync(cancellationToken);

    /// <inhertidoc/>
    public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer) => _baseStream.Read(buffer);

    /// <inheritdoc/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _baseStream.ReadAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _baseStream.ReadAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public override int ReadByte() => _baseStream.ReadByte();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

    /// <inheritdoc/>
    public override void SetLength(long value) => _baseStream.SetLength(value);

    /// <inheritdoc/>
    public sealed override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => _baseStream.Write(buffer);

    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _baseStream.WriteAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _baseStream.WriteAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc/>
    public override void WriteByte(byte value) => _baseStream.WriteByte(value);

    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize) => _baseStream.CopyTo(destination, bufferSize);

    /// <inheritdoc/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => _baseStream.CopyToAsync(destination, bufferSize, cancellationToken);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystemStream"/> class.
    /// </summary>
    /// <param name="file">File being read/written.</param>
    /// <param name="baseStream">The underlying stream to use for this stream.</param>
    protected internal GorgonFileSystemStream(IGorgonVirtualFile file, Stream baseStream)
    {
        FileEntry = file;
        CloseUnderlyingStream = true;
        _baseStream = baseStream;

        // Reset the position to the beginning.
        if (_baseStream.CanSeek)
        {
            _baseStream.Position = 0;
        }
    }
}
