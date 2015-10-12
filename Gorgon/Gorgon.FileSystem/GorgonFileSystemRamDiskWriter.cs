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
// Created: Friday, October 2, 2015 8:29:21 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;
using Gorgon.Plugins;

namespace Gorgon.IO
{
	/// <inheritdoc cref="IGorgonFileSystemWriter{T}"/>
	[ExcludeAsPlugin]
	public class GorgonFileSystemRamDiskWriter
		: GorgonPlugin, IGorgonFileSystemWriter<Stream>
	{
		#region Variables.
		// The file system provider.
		private readonly GorgonFileSystemRamDiskProvider _provider;
		// The ram disk file system.
		private readonly RamDiskFileSystem _ramFiles;
		// The mount point for the writer.
		private GorgonFileSystemMountPoint _mountPoint;
		// The file system linked to this provider.
		private readonly GorgonFileSystem _fileSystem;
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public IGorgonFileSystem FileSystem => _fileSystem;

		/// <inheritdoc/>
		/// <remarks>
		/// This value is set to a constant value of <c>::\\Memory</c>. This is required for the <see cref="GorgonFileSystemRamDiskProvider"/> to mount its files/directories.
		/// </remarks>
		public string WriteLocation => @"::\\Memory";
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy data from a file system to the file system linked to this writer.
		/// </summary>
		/// <param name="progress">The callback for copy progress.</param>
		/// <param name="token">The cancellation token for asynchronous copy.</param>
		/// <param name="allowOverwrite">Flag to indicate whether to allow overwriting files or not.</param>
		/// <param name="files">The files in the source file system to copy.</param>
		/// <param name="directories">The directories in the source file system to copy.</param>
		/// <returns>A tuple containing the count of the directories and files copied.</returns>
		private Tuple<int, int> CopyInternal(Func<GorgonWriterCopyProgress, bool> progress,
											 CancellationToken token,
											 bool allowOverwrite,
											 IGorgonVirtualFile[] files,
											 IGorgonVirtualDirectory[] directories)
		{
			int directoryCount = 0;
			int fileCount = 0;

			// Create all the directories.
			foreach (IGorgonVirtualDirectory directory in directories)
			{
				if (token.IsCancellationRequested)
				{
					return new Tuple<int, int>(directoryCount, fileCount);
				}

				CreateDirectory(directory.FullPath);
				++directoryCount;
			}

			foreach (IGorgonVirtualFile file in files)
			{
				IGorgonVirtualFile destFile = FileSystem.GetFile(file.FullPath);

				if (token.IsCancellationRequested)
				{
					return new Tuple<int, int>(directoryCount, fileCount);
				}

				// Do not allow overwrite if the user requested it.
				if ((destFile != null) && (!allowOverwrite))
				{
					throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFile.FullPath));
				}

				// Copy the file.
				using (Stream sourceStream = file.OpenStream())
				{
					using (Stream destStream = OpenStream(file.FullPath, FileMode.Create))
					{
						sourceStream.CopyTo(destStream, 80000);
					}
				}

				++fileCount;

				if (progress == null)
				{
					continue;
				}

				if (!progress(new GorgonWriterCopyProgress(file, fileCount, files.Length, directories.Length)))
				{
					return new Tuple<int, int>(directoryCount, fileCount);
				}
			}

			return new Tuple<int, int>(directoryCount, fileCount);
		}

		/// <inheritdoc/>
		public Tuple<int, int> CopyFrom(IGorgonFileSystem sourceFileSystem, Func<GorgonWriterCopyProgress, bool> copyProgress = null, bool allowOverwrite = true)
		{
			if (sourceFileSystem == null)
			{
				throw new ArgumentNullException(nameof(sourceFileSystem));
			}

			IGorgonVirtualFile[] files = sourceFileSystem.FindFiles("/", "*").ToArray();
			IGorgonVirtualDirectory[] directories = sourceFileSystem.FindDirectories("/", "*").ToArray();

			if ((files.Length == 0) && (directories.Length == 0))
			{
				return new Tuple<int, int>(0, 0);
			}

			return CopyInternal(copyProgress, new CancellationToken(false), allowOverwrite, files, directories);
		}

		/// <inheritdoc/>
		public async Task<Tuple<int, int>> CopyFromAsync(IGorgonFileSystem sourceFileSystem, CancellationToken cancelToken, Func<GorgonWriterCopyProgress, bool> copyProgress = null, bool allowOverwrite = true)
		{
			if (sourceFileSystem == null)
			{
				throw new ArgumentNullException(nameof(sourceFileSystem));
			}

			// ReSharper disable MethodSupportsCancellation
			var data = await Task.Run(() =>
			{
				IGorgonVirtualFile[] files = sourceFileSystem.FindFiles("/", "*").ToArray();
				IGorgonVirtualDirectory[] directories = sourceFileSystem.FindDirectories("/", "*").ToArray();

				return new Tuple<IGorgonVirtualFile[], IGorgonVirtualDirectory[]>(files, directories);
			});

			if ((data.Item1.Length == 0) && (data.Item2.Length == 0))
			{
				return new Tuple<int, int>(0, 0);
			}

			return await Task.Run(() => CopyInternal(copyProgress, cancelToken, allowOverwrite, data.Item1, data.Item2));
			// ReSharper restore MethodSupportsCancellation
		}

		/// <inheritdoc/>
		public IGorgonVirtualDirectory CreateDirectory(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
			}

			VirtualDirectory result = _fileSystem.InternalRootDirectory.Directories.Add(_mountPoint, path);

			if (path == "/")
			{
				return result;
			}

			_ramFiles.AddDirectoryPath(path);

			return result;
		}

		/// <inheritdoc/>
		public void DeleteDirectory(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
			}

			// If we're not "deleting" the root, then just kill the subdirectory.
			if (path != "/")
			{
				VirtualDirectory directory = _fileSystem.InternalGetDirectory(path);

				if (directory == null)
				{
					throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
				}

				directory.Parent.Directories.Remove(directory);
				_ramFiles.RemoveDirectoryPath(directory.FullPath);
				return;
			}

			// Otherwise, clear the file system files and directories.
			_fileSystem.InternalRootDirectory.Directories.Clear();
			_fileSystem.InternalRootDirectory.Files.Clear();
			_ramFiles.Clear();
		}

		/// <inheritdoc/>
		public void DeleteFile(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
			}

			VirtualFile file = _fileSystem.InternalGetFile(path);

			if (file == null)
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
			}

			file.Directory.Files.Remove(file);
			_ramFiles.DeleteFile(file.FullPath);
		}

		/// <inheritdoc/>
		public void Mount()
		{
			Unmount();

			_mountPoint = FileSystem.Mount(_provider.Prefix, "/");
		}

		/// <inheritdoc/>
		public Stream OpenStream(string path, FileMode mode)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
			}

			string directoryPart = Path.GetDirectoryName(path);
			directoryPart = string.IsNullOrWhiteSpace(directoryPart) ? "/" : directoryPart.FormatDirectory('/');

			VirtualFile file = _fileSystem.InternalGetFile(path);

			if ((file == null) && ((mode == FileMode.Truncate) || (mode == FileMode.Open) || (mode == FileMode.Append)))
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
			}

			if (file != null)
			{
				return new RamDiskWriterStream(_mountPoint, _ramFiles.OpenForWrite(path), file, mode);
			}

			string fileName = Path.GetFileName(path);
			VirtualDirectory directory = _fileSystem.InternalGetDirectory(directoryPart);

			if (directory == null)
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryPart));
			}

			RamDiskFileInfo? ramFileInfo = null;

			try
			{
				path = directory.FullPath + fileName.FormatFileName();
				file = directory.Files.Add(_mountPoint, new PhysicalFileInfo(_provider.Prefix + "::" + path, DateTime.Now, 0, path));

				MemoryStream ramStream = _ramFiles.OpenForWrite(file.FullPath);
				ramFileInfo = _ramFiles.GetFileInfo(file.FullPath);

				return new RamDiskWriterStream(_mountPoint, ramStream, file, mode);
			}
			catch
			{
				// If we've got an error, and the file's been added, then get rid of it.
				if ((file != null) && (directory.Files.Contains(file.FullPath)))
				{
					directory.Files.Remove(file);
				}

				if (ramFileInfo != null)
				{
					_ramFiles.DeleteFile(ramFileInfo.Value.FullPath);
				}

				throw;
			}
		}

		/// <inheritdoc/>
		public void Unmount()
		{
			if (!FileSystem.MountPoints.Contains(_mountPoint))
			{
				return;
			}

			FileSystem.Unmount(_mountPoint);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemRamDiskWriter"/> class.
		/// </summary>
		/// <param name="fileSystem">A file system used to track the updates when writing.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem" /> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="fileSystem"/> does not contain a <see cref="GorgonFileSystemRamDiskProvider"/>.</exception>
		public GorgonFileSystemRamDiskWriter(GorgonFileSystem fileSystem)
			: base(Resources.GORFS_RAMDISK_WRITER_FS_DESC)
		{
			if (fileSystem == null)
			{
				throw new ArgumentNullException(nameof(fileSystem));
			}

			_provider = (from providers in fileSystem.Providers
			             let ramProvider = providers as GorgonFileSystemRamDiskProvider
			             select ramProvider)
				.FirstOrDefault();

			if (_provider == null)
			{
				throw new ArgumentException(Resources.GORFS_ERR_NO_RAMDISK_PROVIDER, nameof(fileSystem));
			}

			_ramFiles = _provider.FileData;
			_fileSystem = fileSystem;
		}
		#endregion
	}
}
