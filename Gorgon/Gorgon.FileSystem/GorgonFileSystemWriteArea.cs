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
// Created: Wednesday, September 23, 2015 7:21:55 PM
// 
#endregion

using System;
using System.IO;
using System.Text;
using System.Linq;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <inheritdoc cref="IGorgonFileSystemWriteArea{T}"/>
	public class GorgonFileSystemWriteArea
		: IGorgonFileSystemWriteArea<FileStream>
	{
		#region Variables.
		// Locking synchronization for multiple threads.
		private readonly object _syncLock = new object();
		// The mount point for the write area.
		private readonly GorgonFileSystemMountPoint _mountPoint;
		// The file system to link with the write area.
		private readonly GorgonFileSystem _fileSystem;
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public IGorgonFileSystem FileSystem => _fileSystem;

		/// <inheritdoc/>
		public string WriteLocation
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to prepare the specified directory for use as a file system write area.
		/// </summary>
		private void PrepareWriteArea()
		{
			lock (_syncLock)
			{
				// If the area specified by the write directory does not exist yet, then 
				// create it.
				var writeDir = new DirectoryInfo(WriteLocation);

				if (!writeDir.Exists)
				{
					writeDir.Create();
				}
			}
		}


		/// <summary>
		/// Function to return the write area path for a given virtual file system path.
		/// </summary>
		/// <param name="path">The path to evaluate.</param>
		/// <returns>The location to write.</returns>
		private string GetWriteDirectoryPath(string path)
		{
			if (path == "/")
			{
				return WriteLocation;
			}

			var physicalPath = new StringBuilder(WriteLocation);

			if (path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				physicalPath.Append(path.FormatDirectory(Path.DirectorySeparatorChar), 1, path.Length - 1);
			}
			else
			{
				physicalPath.Append(path.FormatDirectory(Path.DirectorySeparatorChar));
			}

			return physicalPath.ToString();
		}

		/// <summary>
		/// Function to return the write area path for a given virtual file.
		/// </summary>
		/// <param name="directoryName">Formatted directory name.</param>
		/// <param name="fileName">Formatted file name.</param>
		/// <returns>The location to write.</returns>
		private string GetWriteFilePath(string directoryName, string fileName)
		{
			return GetWriteDirectoryPath(directoryName) + fileName;
		}

		/// <inheritdoc/>
		public void Mount()
		{
			PrepareWriteArea();

			Unmount();

			lock (_syncLock)
			{
				FileSystem.Mount(WriteLocation, "/");
			}
		}

		/// <inheritdoc/>
		public void Unmount()
		{
			lock (_syncLock)
			{
				if (!FileSystem.MountPoints.Contains(_mountPoint))
				{
					return;
				}

				FileSystem.Unmount(_mountPoint);
			}
		}

		/// <inheritdoc/>
		public IGorgonVirtualDirectory CreateDirectory(string path)
		{
			PrepareWriteArea();

			VirtualDirectory result = _fileSystem.InternalRootDirectory.Directories.Add(_mountPoint, path);

			string writePath = GetWriteDirectoryPath(path);

			var dirInfo = new DirectoryInfo(writePath);

			if (dirInfo.Exists)
			{
				return result;
			}

			dirInfo.Create();

			return result;
		}

		/// <inheritdoc/>
		public void DeleteDirectory(string path)
		{
			// If the writable area does not exist at all, then we have nothing to 
			// delete.
			var dirInfo = new DirectoryInfo(WriteLocation);

			// If we're not "deleting" the root, then just kill the subdirectory.
			if (path != "/")
			{
				VirtualDirectory directory = _fileSystem.InternalGetDirectory(path);

				if (directory == null)
				{
					throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
				}

				directory.Parent.Directories.Remove(directory);

				if (!dirInfo.Exists)
				{
					return;
				}

				dirInfo = new DirectoryInfo(GetWriteDirectoryPath(path));
				dirInfo.Delete(true);
				return;
			}

			// Otherwise, clear the file system files and directories.
			_fileSystem.InternalRootDirectory.Directories.Clear();
			_fileSystem.InternalRootDirectory.Files.Clear();

			if (!dirInfo.Exists)
			{
				return;
			}

			PrepareWriteArea();
			dirInfo = new DirectoryInfo(WriteLocation);
			var directories = dirInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
			var files = dirInfo.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

			foreach (var directoryPath in directories)
			{
				directoryPath.Delete(true);
			}

			foreach (var file in files)
			{
				file.Delete();
			}
		}

		/// <inheritdoc/>
		public FileStream OpenStream(string path, FileMode mode)
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

			// We're opening an existing file, so check it if we require the file to be present.
			if ((file == null) && ((mode == FileMode.Truncate) || (mode == FileMode.Open)))
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
			}

			if (file != null)
			{
				return new FileSystemWriteStream(_mountPoint, GetWriteFilePath(file.Directory.FullPath, file.Name), file, mode);
			}

			PrepareWriteArea();

			string directoryPath = Path.GetDirectoryName(path);
			string fileName = Path.GetFileName(path);

			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(string.Format(Resources.GORFS_ERR_NO_FILENAME, path), nameof(path));
			}

			directoryPath = string.IsNullOrWhiteSpace(directoryPath) ? "/" : directoryPath.FormatDirectory('/');

			VirtualDirectory directory = _fileSystem.InternalGetDirectory(directoryPath);

			if (directory == null)
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryPath));
			}

			FileStream result = null;

			try
			{
				file = directory.Files.Add(_mountPoint, new PhysicalFileInfo(GetWriteFilePath(directoryPath, fileName), DateTime.Now, 0, directory.FullPath + fileName.FormatFileName()));
				result = new FileSystemWriteStream(_mountPoint, file.PhysicalFile.FullPath, file, mode);
			}
			catch
			{
				// If we've got an error, and the file's been added, then get rid of it.
				if ((file != null) && (directory.Files.Contains(file.FullPath)))
				{
					directory.Files.Remove(file);
				}

				result?.Dispose();
				throw;
			}

			return result;
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

			// If the writable area does not exist at all, then we have nothing to 
			// delete.
			var dirInfo = new DirectoryInfo(WriteLocation);

			if (!dirInfo.Exists)
			{
				return;
			}

			string writePath = GetWriteFilePath(file.Directory.FullPath, file.Name);

			if (!File.Exists(writePath))
			{
				return;
			}

			File.Delete(writePath);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemWriteArea"/> class.
		/// </summary>
		/// <param name="fileSystem">A file system used to track the updates when writing.</param>
		/// <param name="writeLocation">The directory on the physical file system to actually write data into.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, or the <paramref name="writeLocation"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="writeLocation"/> is empty.</exception>
		public GorgonFileSystemWriteArea(GorgonFileSystem fileSystem, string writeLocation)
		{
			if (fileSystem == null)
			{
				throw new ArgumentNullException(nameof(fileSystem));
			}

			if (writeLocation == null)
			{
				throw new ArgumentNullException(nameof(writeLocation));
			}

			if (string.IsNullOrWhiteSpace(writeLocation))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(writeLocation));
			}

			// We need the concrete type in here because we need access to its internals.
			_fileSystem = fileSystem;
			WriteLocation = writeLocation.FormatDirectory(Path.DirectorySeparatorChar);
			_mountPoint = new GorgonFileSystemMountPoint(fileSystem.DefaultProvider, WriteLocation, "/");
		}
		#endregion
	}
}
