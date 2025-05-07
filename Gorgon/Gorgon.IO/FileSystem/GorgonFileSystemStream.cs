// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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

using Gorgon.IO.FileSystem.Providers;
using Gorgon.Math;

namespace Gorgon.IO.FileSystem;

/// <summary>
/// A file stream for the Gorgon file system
/// </summary>
public class GorgonFileSystemStream
    : Stream
{
    // Base stream to use.
    private Stream _baseStream = Null;

    // Function called when the stream is closed.
    private Action<string, string> _onClose;

    // Flag to indicate that the stream is disposed.
    private bool _disposed;

    /// <summary>
    /// Property to return the virtual path to the file being read/written.
    /// </summary>
    public string VirtualPath
    {
        get;
    }

    /// <summary>
    /// Property to return the physical path to the file being read/written.
    /// </summary>
    public string PhysicalPath
    {
        get;
    }

    /// <inheritdoc/>
    public override bool CanRead => _baseStream.CanRead;

    /// <inheritdoc/>
    public override bool CanWrite => _baseStream.CanWrite;

    /// <inheritdoc/>
    public override bool CanSeek => _baseStream.CanSeek;

    /// <summary>
    /// Gets a value that determines whether the current stream can time out.
    /// </summary>
    /// <value></value>
    /// <returns>A value that determines whether the current stream can time out.</returns>
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

    /// <summary>
    /// Function used as a default callback if one is not provided.
    /// </summary>
    /// <param name="virtualPath">Not used.</param>
    /// <param name="physicalPath">Not used.</param>
    private void DummyCallback(string virtualPath, string physicalPath)
    {
        // Intentionaly blank.
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
            Stream baseStream = Interlocked.Exchange(ref _baseStream, Null);
            Action<string, string> onClose = Interlocked.Exchange(ref _onClose, DummyCallback);

            if (baseStream != Null)
            {
                baseStream.Close();
            }

            onClose(VirtualPath, PhysicalPath);
        }

        _disposed = true;

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

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count.Min((int)Length)));

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
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => _baseStream.Write(buffer);

    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _baseStream.WriteAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _baseStream.WriteAsync(buffer, offset, count, cancellationToken);

    /// <summary>
    /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <param name="value">The byte to write to the stream.</param>
    /// <exception cref="IOException">An I/O error occurs. </exception>
    /// <exception cref="NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
    public override void WriteByte(byte value) => _baseStream.WriteByte(value);

    /// <summary>
    /// Initializes a new instance of the<see cref = "GorgonFileSystemStream" /> class.
    /// </summary>
    /// <param name="virtualPath">The virtual path for the file.</param>
    /// <param name="physicalPath">The physical path for the file.</param>
    /// <param name="baseStream">The underlying stream to use for this stream.</param>
    /// <param name="onClose">The callback method that is called when the stream is closed by the user.</param>
    internal GorgonFileSystemStream(string virtualPath, string physicalPath, Stream baseStream, Action<string, string>? onClose)
    {
        _baseStream = baseStream;
        VirtualPath = virtualPath;
        PhysicalPath = physicalPath;
        _onClose = onClose ?? DummyCallback;

        // Reset the position to the beginning.
        if (_baseStream.CanSeek)
        {
            _baseStream.Position = 0;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystemStream"/> class.
    /// </summary>
    /// <param name="fileInfo">The file information used to open the file.</param>
    /// <param name="baseStream">The underlying stream to use for this stream.</param>
    /// <param name="onClose">The callback method that is called when the stream is closed by the user.</param>
    protected internal GorgonFileSystemStream(IGorgonPhysicalFileInfo fileInfo, Stream baseStream, Action<string, string>? onClose)
    {
        _baseStream = baseStream;
        VirtualPath = fileInfo.VirtualPath;
        PhysicalPath = fileInfo.FullPath;
        _onClose = onClose ?? DummyCallback;

        // Reset the position to the beginning.
        if (_baseStream.CanSeek)
        {
            _baseStream.Position = 0;
        }
    }
}
