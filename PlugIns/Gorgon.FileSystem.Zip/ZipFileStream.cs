
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
// Created: Monday, June 27, 2011 9:31:05 AM
// 

using Gorgon.IO.Zip.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Gorgon.IO.Zip;

/// <summary>
/// A stream used to read zip files
/// </summary>
internal class ZipFileStream
    : GorgonFileSystemStream
{
    // Flag to inidcate that the object was disposed.
    private bool _disposed;
    private ZipInputStream _zipStream;      // Input stream for the zip file.
    private long _position;                 // Position in the stream.
    private long _basePosition;             // Base position in the stream.
    private long _length;                   // File length.

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => _zipStream.CanSeek;

    /// <inheritdoc/>
    public override bool CanTimeout => _zipStream.CanTimeout;

    /// <inheritdoc/>
    public override long Length => _length;

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

            _zipStream.Position = _basePosition + _position;
        }
    }

    /// <inheritdoc/>
    public override int WriteTimeout
    {
        get => _zipStream.WriteTimeout;
        set => _zipStream.WriteTimeout = value;
    }

    /// <inheritdoc/>
    public override int ReadTimeout
    {
        get => _zipStream.ReadTimeout;
        set => _zipStream.ReadTimeout = value;
    }

    /// <summary>
    /// Function to get the zip entry stream.
    /// </summary>
    /// <param name="file">File in the zip file to read.</param>
    /// <param name="stream">Stream to the zip file.</param>
    private void GetZipEntryStream(IGorgonVirtualFile file, Stream stream)
    {
        ZipEntry entry;

        _zipStream = new ZipInputStream(stream);

        while ((entry = _zipStream.GetNextEntry()) is not null)
        {
            if (!entry.IsFile)
            {
                continue;
            }

            string newPath = entry.Name;
            string filePath = file.PhysicalFile.FullPath[(file.PhysicalFile.FullPath.LastIndexOf(':') + 1)..];

            if (!newPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                newPath = "/" + newPath;
            }

            if (!string.Equals(newPath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _length = _zipStream.Length;
            _basePosition = _zipStream.Position;
            return;
        }

        _zipStream?.Dispose();

        throw new FileNotFoundException(string.Format(Resources.GORFS_ZIP_ERR_FILE_NOT_FOUND, file.PhysicalFile.FullPath));
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
            _zipStream.Dispose();
        }

        _disposed = true;

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override void Close() => _zipStream.Close();

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
    public override void Flush() => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize) => _zipStream.CopyTo(destination, bufferSize);

    /// <inheritdoc/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => _zipStream.CopyToAsync(destination, bufferSize, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _zipStream.ReadAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _zipStream.BeginRead(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public override int EndRead(IAsyncResult asyncResult) => _zipStream.EndRead(asyncResult);

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        if (_position >= Length)
        {
            return 0;
        }

        int result = _zipStream.Read(buffer);

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
            throw new EndOfStreamException(Resources.GORFS_ZIP_ERR_EOS);
        }

        if (_position < 0)
        {
            throw new EndOfStreamException(Resources.GORFS_ZIP_ERR_BOS);
        }

        _zipStream.Position = _position + _basePosition;

        return _position;
    }

    /// <inheritdoc/>
    public override int ReadByte()
    {
        if (_position >= Length)
        {
            return -1;
        }

        int result = _zipStream.ReadByte();

        _position += result;

        return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipFileStream"/> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="stream">The stream.</param>
    internal ZipFileStream(IGorgonVirtualFile file, Stream stream)
        : base(file, stream) => GetZipEntryStream(file, stream);

}
