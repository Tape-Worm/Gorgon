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
// Created: Wednesday, September 30, 2015 10:10:35 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
    /// <summary>
    /// A stream used to write data into the RAM disk.
    /// </summary>
    internal class RamDiskWriterStream
        : Stream
    {
        #region Variables.
        // The file mount point.
        private readonly GorgonFileSystemMountPoint _mountPoint;
        // A stream for the blob.
        private readonly MemoryStream _blobStream;
        // The virtual file entry to update when the stream is closed.
        private VirtualFile _virtualFile;
        // The file mode used to open the stream.
        private readonly FileMode _fileMode;
        // The ram disk provider for this stream.
        private readonly GorgonFileSystemRamDiskProvider _provider;
        #endregion

        #region Properties.
        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead => _blobStream.CanRead;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek => _blobStream.CanSeek && (((_fileMode != FileMode.CreateNew) && (_fileMode != FileMode.Truncate)) || ((_fileMode == FileMode.OpenOrCreate) && (_virtualFile.Size > 0)));

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite => _blobStream.CanWrite && _fileMode != FileMode.Open;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Length => _blobStream.Length;

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Position
        {
            get => _blobStream.Position;
            set
            {
                if (!CanSeek)
                {
                    throw new IOException(Resources.GORFS_ERR_RAMDISK_FILE_NOT_SEEKABLE);
                }

                _blobStream.Position = value;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if ((disposing) && (_virtualFile != null) && (CanWrite))
            {
                string physicalPath = _provider.Prefix + "::" + _virtualFile.FullPath;
                _virtualFile.PhysicalFile = new PhysicalFileInfo(physicalPath, _virtualFile.CreateDate, Length, _virtualFile.FullPath, 0, DateTime.Now);

                if (_virtualFile.MountPoint != _mountPoint)
                {
                    _virtualFile.MountPoint = _mountPoint;
                }

                var info = new RamDiskFileInfo(_virtualFile);
                _provider.FileData.UpdateFile(_virtualFile.FullPath, ref info);
            }

            if (disposing)
            {
                _blobStream.Position = 0;
            }

            _virtualFile = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
        public override void Flush() => _blobStream.Flush();

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. </param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
            {
                throw new IOException(Resources.GORFS_ERR_RAMDISK_FILE_NOT_SEEKABLE);
            }

            return _blobStream.Seek(offset, origin);
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param>
        /// <exception cref="NotSupportedException">This method is not supported by this implementation.</exception>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. </param><param name="count">The maximum number of bytes to be read from the current stream. </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
            {
                throw new IOException(Resources.GORFS_ERR_RAMDISK_FILE_IS_WRITE_ONLY);
            }

            if ((_fileMode == FileMode.Open)
                || ((_fileMode == FileMode.Append) && ((Position + count) > Length))
                || ((_fileMode == FileMode.OpenOrCreate) && (_virtualFile.Size == 0)))
            {
                throw new EndOfStreamException();
            }

            return _blobStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param><param name="count">The number of bytes to be written to the current stream. </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/>  is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception><exception cref="T:System.IO.IOException">An I/O error occured, such as the specified file cannot be found.</exception><exception cref="T:System.NotSupportedException">The stream does not support writing.</exception><exception cref="T:System.ObjectDisposedException"><see cref="M:System.IO.Stream.Write(System.Byte[],System.Int32,System.Int32)"/> was called after the stream was closed.</exception><filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
            {
                throw new IOException(Resources.GORFS_ERR_RAMDISK_FILE_IS_READ_ONLY);
            }

            _blobStream.Write(buffer, offset, count);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="RamDiskWriterStream"/> class.
        /// </summary>
        /// <param name="mountPoint">The mount point where the writable file is stored.</param>
        /// <param name="baseStream">The base memory stream from the provider.</param>
        /// <param name="file">The virtual file being written.</param>
        /// <param name="fileMode">Mode used to open the file.</param>
        public RamDiskWriterStream(GorgonFileSystemMountPoint mountPoint, MemoryStream baseStream, VirtualFile file, FileMode fileMode)
        {
            _provider = mountPoint.Provider as GorgonFileSystemRamDiskProvider;

            if (_provider == null)
            {
                throw new ArgumentException(Resources.GORFS_ERR_NO_RAMDISK_PROVIDER);
            }

            if ((fileMode == FileMode.CreateNew) && (file.Size > 0))
            {
                throw new IOException(Resources.GORFS_ERR_RAMDISK_FILE_ALREADY_EXISTS);
            }

            _blobStream = baseStream;

            switch (fileMode)
            {
                case FileMode.Create:
                case FileMode.OpenOrCreate when (file.Size == 0):
                    _blobStream.SetLength(0);
                    file.PhysicalFile = new PhysicalFileInfo(file.PhysicalFile.FullPath, DateTime.Now, 0, file.Directory.FullPath);
                    break;
                case FileMode.Truncate:
                    _blobStream.SetLength(0);
                    file.PhysicalFile = new PhysicalFileInfo(file.PhysicalFile.FullPath, file.CreateDate, 0, file.Directory.FullPath, 0, DateTime.Now);
                    break;
                case FileMode.Append:
                    _blobStream.Position = _blobStream.Length;
                    break;
            }

            _mountPoint = mountPoint;
            _virtualFile = file;
            _fileMode = fileMode;
        }
        #endregion
    }
}
