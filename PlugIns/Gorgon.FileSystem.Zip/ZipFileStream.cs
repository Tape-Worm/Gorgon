
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

using System.Diagnostics.CodeAnalysis;
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
    // Input stream for the zip file.
    private ZipInputStream _zipStream;
    // Position in the stream.
    private long _position;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanTimeout => false;

    /// <inheritdoc/>
    public override long Length => FileEntry.Size;

    /// <summary>
    /// Property to return the currrent position within the stream.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown if the property is set.</exception>
    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override int WriteTimeout
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override int ReadTimeout
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Function to get the zip entry stream.
    /// </summary>
    /// <param name="file">File in the zip file to read.</param>
    /// <param name="stream">Stream to the zip file.</param>
    [MemberNotNull(nameof(_zipStream))]
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
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => throw new NotSupportedException();

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Flush() => _zipStream.Flush();

    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => _zipStream.FlushAsync();

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

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize)
    {
        _zipStream.CopyTo(destination, bufferSize);
        _position = Length;
    }

    /// <inheritdoc/>
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        await _zipStream.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);
        _position = Length;
    }

    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int result = await _zipStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        _position += result;
        return result;
    }

    /// <inheritdoc/>
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => _zipStream.BeginRead(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public override int EndRead(IAsyncResult asyncResult)
    {
        int result = _zipStream.EndRead(asyncResult);

        _position += result;

        return result;
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        if (_position >= Length)
        {
            return -1;
        }

        int result = _zipStream.Read(buffer);

        _position += result;
        return result;
    }

    /// <summary>
    /// Not supported by this stream.
    /// </summary>
    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

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
        : base(file, Null) => GetZipEntryStream(file, stream);

}
