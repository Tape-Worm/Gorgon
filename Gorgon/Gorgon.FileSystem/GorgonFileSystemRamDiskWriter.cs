using System;
using System.IO;
using System.Linq;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <inheritdoc cref="IGorgonFileSystemWriteArea{T}"/>
	public class GorgonFileSystemRamDiskWriter
		: IGorgonFileSystemWriteArea<Stream>
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

			if ((mode == FileMode.Truncate) || (mode == FileMode.Open))
			{
				if (file == null)
				{
					throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
				}

				return new RamDiskWriterStream(_mountPoint, _ramFiles.OpenForWrite(path, mode), file, mode);
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

				MemoryStream ramStream = _ramFiles.OpenForWrite(file.FullPath, mode);
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
