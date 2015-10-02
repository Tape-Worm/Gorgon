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
	class RamDiskWriterStream
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
		/// <inheritdoc/>
		public override bool CanRead => _blobStream.CanRead;

		/// <inheritdoc/>
		public override bool CanSeek => _blobStream.CanSeek && (((_fileMode != FileMode.CreateNew) && (_fileMode != FileMode.Truncate)) || ((_fileMode == FileMode.OpenOrCreate) && (_virtualFile.Size > 0)));

		/// <inheritdoc/>
		public override bool CanWrite => _blobStream.CanWrite && _fileMode != FileMode.Open;

		/// <inheritdoc/>
		public override long Length => _blobStream.Length;

		/// <inheritdoc/>
		public override long Position
		{
			get
			{
				return _blobStream.Position;
			}
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
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public override void Flush()
		{
			_blobStream.Flush();
		}

		/// <inheritdoc/>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek)
			{
				throw new IOException(Resources.GORFS_ERR_RAMDISK_FILE_NOT_SEEKABLE);
			}

			return _blobStream.Seek(offset, origin);
		}

		/// <inheritdoc/>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

			if ((fileMode == FileMode.Create) || ((fileMode == FileMode.OpenOrCreate) && (file.Size == 0)))
			{
				_blobStream.SetLength(0);
				file.PhysicalFile = new PhysicalFileInfo(file.PhysicalFile.FullPath, DateTime.Now, 0, file.Directory.FullPath);
			}

			if (fileMode == FileMode.Truncate)
			{
				_blobStream.SetLength(0);
				file.PhysicalFile = new PhysicalFileInfo(file.PhysicalFile.FullPath, file.CreateDate, 0, file.Directory.FullPath, 0, DateTime.Now);
			}

			_mountPoint = mountPoint;
			_virtualFile = file;
			_fileMode = fileMode;
		}
		#endregion
	}
}
