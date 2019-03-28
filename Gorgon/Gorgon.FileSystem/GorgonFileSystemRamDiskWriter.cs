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
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;
using Gorgon.Plugins;

namespace Gorgon.IO
{
	/// <summary>
	/// Specifies a writable area on the physical file system for a virtual file system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="IGorgonFileSystem"/> is a read only object that is only capable of returning existing files and directories from a physical file system. This is by design in order to keep the integrity 
	/// and security of the original file system intact. However, in some cases, the need to write data is required by the application, and that data should be reflected in the current file system. Thus, we 
	/// have the <see cref="IGorgonFileSystemWriter{T}"/> interface.
	/// </para>
	/// <para>
	/// This object will allow applications to define an area on a physical file system that can be written to by the application. This provides isolation from the main file system and gives a degree of security 
	/// when persisting data to an application. For example, if you have a zip file mounted in <c>/</c> and you want to write some data in a new directory, then you could create this object and provide a 
	/// path to the writable area: <c>c:\users\username\AppData\YourApplication\CustomData\</c>. When creating, or deleting a directory or file, all data will be shunted to that physical location. For example, 
	/// creating a directory named <c>CustomDirectory</c> would actually put the directory under <c>c:\users\AppData\YourApplication\CustomData\CustomDirectory</c>. Likewise, a file named <c>SomeFile.txt</c> would 
	/// be put under <c>>c:\users\username\AppData\YourApplication\CustomData\SomeFile.txt</c>.
	/// </para>
	/// <para>
	/// If the <see cref="IGorgonFileSystem"/> already has a file or directory mounted from a physical file system, then files from the write area will override those files, providing the most up to date copy of 
	/// the data in the physical file systems. There is no actual change to the original files and they will remain in their original location, untouched. Only the files in the directory designated to be the writable 
	/// area for a file system will be used for write operations.
	/// </para>
	/// <para> 
	/// <note type="tip">
	/// <para>
	/// When attaching a <see cref="IGorgonFileSystemWriter{T}"/> to a <see cref="IGorgonFileSystem"/>, the write area location is <u>always</u> mounted to the root of the virtual file system.
	/// </para>
	/// </note> 
	/// </para>
	/// <para>
	/// <note type="warning">
	/// <para>
	/// Because the <see cref="IGorgonFileSystem.Mount"/> method always overrides existing files and directories (with the same path) with files and directories from the last loaded physical file system, the write 
	/// area may have its files overridden if <see cref="IGorgonFileSystem.Mount"/> is called after linking the write area with a <see cref="IGorgonFileSystem"/>.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	/// <example>
	/// This example shows how to create a file system with the default provider, mount a directory to the root, and create a new file:
	/// <code language="csharp">
	/// <![CDATA[
	/// IGorgonFileSystem fileSystem = new GorgonFileSystem();
	/// IGorgonFileSystemWriter writeArea = new GorgonFileSystemWriter(fileSystem, @"C:\MyWritingSpot\");
	/// 
	/// // Mount a directory for this file system.
	/// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
	/// 
	/// // Ensure that we mount the write area to ensure that the files in the write directory 
	/// // are available.
	/// writeArea.Mount();
	/// 
	/// // Create a text file.
	/// using (Stream stream = writeArea.OpenStream("/AFile.txt", FileMode.Create))
	/// {
	///		using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
	///		{
	///			writer.WriteLine("This is a line of text.");
	///		}
	/// }
	/// 
	/// // This should retrieve the updated file.
	/// IGorgonVirtualFile file = fileSystem.GetFile("/AFile.txt");
	/// 
	/// ]]>
	/// </code>
	/// </example>
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
		/// <summary>
		/// Property to return the file system linked to this writable area.
		/// </summary>
		public IGorgonFileSystem FileSystem => _fileSystem;

		/// <summary>
		/// Property to return the location on the physical file system to use as the writable area for a <see cref="IGorgonFileSystem"/>.
		/// </summary>
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
        /// <param name="allowOverwrite">Flag to indicate whether to allow overwriting files or not.</param>
        /// <param name="files">The files in the source file system to copy.</param>
        /// <param name="directories">The directories in the source file system to copy.</param>
        /// <param name="token">The cancellation token for asynchronous copy.</param>
        /// <returns>A tuple containing the count of the directories and files copied.</returns>
        private (int DirectoryCount, int FileCount)? CopyInternal(Func<GorgonWriterCopyProgress, bool> progress,
		                                                          bool allowOverwrite,
		                                                          IGorgonVirtualFile[] files,
		                                                          IGorgonVirtualDirectory[] directories,
                                                                  CancellationToken token)
		{
			int directoryCount = 0;
			int fileCount = 0;

			// Create all the directories.
			foreach (IGorgonVirtualDirectory directory in directories)
			{
				if (token.IsCancellationRequested)
				{
					return (directoryCount, fileCount);
				}

				CreateDirectory(directory.FullPath);
				++directoryCount;

                if (token.IsCancellationRequested)
                {
                    return (0, 0);
                }
			}

			foreach (IGorgonVirtualFile file in files)
			{
				IGorgonVirtualFile destFile = FileSystem.GetFile(file.FullPath);

				if (token.IsCancellationRequested)
				{
					return null;
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
					return (directoryCount, fileCount);
				}
			}

			return (directoryCount, fileCount);
		}

		/// <summary>
		/// Function to copy the contents of a file system to the writable area.
		/// </summary> 
		/// <param name="sourceFileSystem">The <see cref="IGorgonFileSystem"/> to copy.</param>
		/// <param name="copyProgress">A method callback used to track the progress of the copy operation.</param>
		/// <param name="allowOverwrite">[Optional] <b>true</b> to allow overwriting of files that already exist in the file system with the same path, <b>false</b> to throw an exception when a file with the same path is encountered.</param>
		/// <returns>A <see cref="ValueTuple{T1,T2}"/> containing the number of directories (<c>item1</c>) and the number of files (<c>item2</c>) copied, or <b>null</b> if the operation was cancelled.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceFileSystem"/> parameter is <b>null</b>.</exception>
		/// <exception cref="IOException">Thrown when the a file exists in <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, and the <paramref name="allowOverwrite"/> parameter is set to <b>false</b>.</exception>
		/// <remarks>
		/// <para>
		/// This copies all the file and directory information from one file system, into the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/> linked to this writer. 
		/// </para>
		/// <para>
		/// When the <paramref name="allowOverwrite"/> is set to <b>false</b>, and a <see cref="IGorgonVirtualFile"/> already exists with the same path as another <see cref="IGorgonVirtualFile"/> in the 
		/// <paramref name="sourceFileSystem"/>, then an exception will be raised.
		/// </para>
		/// </remarks>
		public (int DirectoryCount, int FileCount)? CopyFrom(IGorgonFileSystem sourceFileSystem, Func<GorgonWriterCopyProgress, bool> copyProgress = null, bool allowOverwrite = true)
		{
			if (sourceFileSystem == null)
			{
				throw new ArgumentNullException(nameof(sourceFileSystem));
			}

			IGorgonVirtualFile[] files = sourceFileSystem.FindFiles("/", "*").ToArray();
			IGorgonVirtualDirectory[] directories = sourceFileSystem.FindDirectories("/", "*").ToArray();

            return (files.Length == 0) && (directories.Length == 0)
                ? ((int DirectoryCount, int FileCount)?)(0, 0)
                : CopyInternal(copyProgress, allowOverwrite, files, directories, CancellationToken.None);
        }

        /// <summary>
        /// Function to asynchronously copy the contents of a file system to the writable area.
        /// </summary>
        /// <param name="sourceFileSystem">The <see cref="IGorgonFileSystem"/> to copy.</param>
        /// <param name="cancelToken">The <see cref="CancellationToken"/> used to cancel an in progress copy.</param>
        /// <param name="copyProgress">A method callback used to track the progress of the copy operation.</param>
        /// <param name="allowOverwrite">[Optional] <b>true</b> to allow overwriting of files that already exist in the file system with the same path, <b>false</b> to throw an exception when a file with the same path is encountered.</param>
        /// <returns>A <see cref="ValueTuple{T1,T2}"/> containing the number of directories (<c>item1</c>) and the number of files (<c>item2</c>) copied, or <b>null</b> if the operation was cancelled.</returns>
        /// <remarks>
        /// <para>
        /// This copies all the file and directory information from one file system, into the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/> linked to this writer. 
        /// </para>
        /// <para>
        /// When the <paramref name="allowOverwrite"/> is set to <b>false</b>, and a <see cref="IGorgonVirtualFile"/> already exists with the same path as another <see cref="IGorgonVirtualFile"/> in the 
        /// <paramref name="sourceFileSystem"/>, then an exception will be raised.
        /// </para>
        /// <para>
        /// This version of the copy method allows for an asynchronous copy of a set of a files and directories from another <see cref="IGorgonFileSystem"/>. This method should be used when there is a large 
        /// amount of data to transfer between the file systems.
        /// </para>
        /// <para>
        /// Unlike the <see cref="IGorgonFileSystemWriter{T}.CopyFrom"/> method, this method will report the progress of the copy through the <paramref name="copyProgress"/> callback. This callback is a method that takes a 
        /// <see cref="GorgonWriterCopyProgress"/> value as a parameter that will report the current state, and will return a <see cref="bool"/> to indicate whether to continue the copy or not (<b>true</b> to 
        /// continue, <b>false</b> to stop). 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// The <paramref name="copyProgress"/> method does not switch back to the UI context. Ensure that you invoke any operations that update a UI on the appropriate thread (e.g <c>BeginInvoke</c> on a 
        /// WinForms UI element or <c>Dispatcher</c> on a WPF element).
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// This method also allows for cancellation of the copy operation by passing a <see cref="CancellationToken"/> to the <paramref name="cancelToken"/> parameter.
        /// </para>
        /// </remarks>
        public async Task<(int DirectoryCount, int FileCount)?> CopyFromAsync(IGorgonFileSystem sourceFileSystem, CancellationToken cancelToken, Func<GorgonWriterCopyProgress, bool> copyProgress = null, bool allowOverwrite = true)
		{
			if (sourceFileSystem == null)
			{
				throw new ArgumentNullException(nameof(sourceFileSystem));
			}

			// ReSharper disable MethodSupportsCancellation
			(IGorgonVirtualFile[] Files, IGorgonVirtualDirectory[] Directories) = await Task.Run(() =>
			{
				IGorgonVirtualFile[] files = sourceFileSystem.FindFiles("/", "*").ToArray();
				IGorgonVirtualDirectory[] directories = sourceFileSystem.FindDirectories("/", "*").ToArray();

				return (Files: files, Directories: directories);
			}).ConfigureAwait(false);

            return (Files.Length == 0) && (Directories.Length == 0)
                ? ((int DirectoryCount, int FileCount)?)(0, 0)
                : await Task.Run(() => CopyInternal(copyProgress, allowOverwrite, Files, Directories, cancelToken), cancelToken).ConfigureAwait(false);
            // ReSharper restore MethodSupportsCancellation
        }

        /// <summary>
        /// Function to create a new directory in the writable area on the physical file system.
        /// </summary>
        /// <param name="path">Path to the directory (or directories) to create.</param>
        /// <returns>A <see cref="IGorgonVirtualDirectory"/> representing the final directory in the <paramref name="path"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="path"/> is empty.
        /// <para>-or-</para>
        /// <para>Thrown if the <paramref name="path"/> does not contain any meaningful names.</para>
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when a part of the <paramref name="path"/> has the same name as a file name in the parent of the directory being created.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="path"/> is set to the root directory: <c>/</c>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a new directory within the physical file system directory specified by the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>. If the <paramref name="path"/> contains multiple directories that don't 
        /// exist (e.g. <c><![CDATA[/Exists/AlsoExists/DoesNotExist/DoesNotExistEither/]]></c>), then those directories will be created until the path is completely parsed. The file system will be updated 
        /// to ensure that those directories will exist and can be referenced.
        /// </para>
        /// <para>
        /// If the directory path contains a name that is the same as a file name within a directory (e.g. <c><![CDATA[/MyDirectory/SomeFile.txt/AnotherDirectory]]></c>, where <c>SomeFile.txt</c> already 
        /// exists as a file under <c>MyDirectory</c>), then an exception will be thrown.
        /// </para>
        /// <para>
        /// If the directory already exists (either in the <see cref="IGorgonFileSystem"/> or on the physical file system), then nothing will be done and the existing directory will be returned from the 
        /// method.
        /// </para>
        /// </remarks>
        public IGorgonVirtualDirectory CreateDirectory(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentEmptyException(nameof(path));
			}

			VirtualDirectory result = _fileSystem.InternalRootDirectory.Directories.Add(_mountPoint, path);

			if (path == "/")
			{
				return result;
			}

			_ramFiles.AddDirectoryPath(path);

			return result;
		}

		/// <summary>
		/// Function to delete a directory from the writable area.
		/// </summary>
		/// <param name="path">Path to the directory to delete.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
		/// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="path"/> could not be found.</exception>
		/// <remarks>
		/// <para>
		/// This will delete a directory, its sub directories, and any files under this directory and sub directories. It will remove all references to those files and directories from the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, 
		/// and will delete these items from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> if they exist in that location.
		/// </para>
		/// <para>
		/// When a directory is removed from the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, it will be removed all mounted file systems. However, the actual directory in the physical file systems will not be touched and the 
		/// deleted directory may be restored with a call to <see cref="IGorgonFileSystem.Refresh"/>. 
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// When restoring files with <see cref="IGorgonFileSystem.Refresh"/>, only the file system object will be updated. The method will not restore any deleted directories in the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <seealso cref="IGorgonFileSystem.Refresh"/>
		public void DeleteDirectory(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentEmptyException(nameof(path));
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

		/// <summary>
		/// Function to delete a file from the file system.
		/// </summary>
		/// <param name="path">The path to the file to delete.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
		/// <exception cref="FileNotFoundException">Thrown when the file referenced by the <paramref name="path"/> was not found.</exception>
		/// <remarks>
		/// <para>
		/// This will remove the <see cref="IGorgonVirtualFile"/> from the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, and will delete the physical file from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> if it exists there.
		/// </para>
		/// <para>
		/// When a file is removed from the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, it will be removed all mounted file systems. However, the actual file in the physical file systems will not be touched and the 
		/// deleted file may be restored with a call to <see cref="IGorgonFileSystem.Refresh"/>. 
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// When restoring files with <see cref="IGorgonFileSystem.Refresh"/>, only the file system object will be updated. The method will not restore any deleted files in the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void DeleteFile(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentEmptyException(nameof(path));
			}

			VirtualFile file = _fileSystem.InternalGetFile(path);

			if (file == null)
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
			}

			file.Directory.Files.Remove(file);
			_ramFiles.DeleteFile(file.FullPath);
		}

		/// <summary>
		/// Function to mount the directory specified by <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> into the file system.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will refresh the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/> with the most up to date contents of the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> by mounting the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> into the current <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Users should call this method immediately after creating the object in order to get the contents of the write area into the file system. If this is not called, the file system will not know what 
		/// files or directories are available in the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks> 
		public void Mount()
		{
			Unmount();

			_mountPoint = FileSystem.Mount(_provider.Prefix, "/");
		}

		/// <summary>
		/// Function to open a file for reading or for writing.
		/// </summary>
		/// <param name="path">The path to the file to read/write.</param>
		/// <param name="mode">The mode to determine how to read/write the file.</param>
		/// <returns>An open <see cref="FileStream"/> to the file.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="path"/> does not contain a file name.</para>
		/// </exception>
		/// <exception cref="FileNotFoundException">Thrown when the file referenced by the <paramref name="path"/> was not found and the <paramref name="mode"/> is set to <see cref="FileMode.Open"/> or <see cref="FileMode.Truncate"/>.</exception>
		/// <exception cref="DirectoryNotFoundException">Thrown when the directory in the <paramref name="path"/> was not found.</exception>
		/// <remarks>
		/// <para>
		/// This will open a file for reading or writing depending on the value passed to the <paramref name="mode"/> parameter. See the <see cref="FileMode"/> enumeration for more information about how these modes 
		/// affect the returned <see cref="Stream"/>.
		/// </para>
		/// <para>
		/// When the <paramref name="mode"/> parameter is set to <see cref="FileMode.Open"/>, or <see cref="FileMode.Truncate"/>, and the file does not exist in the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, then an exception will 
		/// be thrown.
		/// </para>
		/// <para>
		/// If the <paramref name="path"/> has a directory, for example <c><![CDATA[/MyDirectory/MyFile.txt]]></c>, and the directory <c><![CDATA[/MyDirectory]]></c> does not exist, an exception will be thrown.
		/// </para>
		/// </remarks>
		/// <seealso cref="FileMode"/>
		public Stream OpenStream(string path, FileMode mode)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentEmptyException(nameof(path));
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

		/// <summary>
		/// Function to unmount the directory specified by <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> from the file system.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will unmount the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> from the file system. All directories and files for the writable area will be removed from the associated <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>. 
		/// </para>
		/// <para>
		/// If another physical file system has files or directories with the same path as one in the write area, then they will be removed from the file system as well since the last mounted file system 
		/// (including the write area) will override the previous entries. To refresh and retrieve those items from the currently mounted file systems after unmounting the write area, call the 
		/// <see cref="IGorgonFileSystem.Refresh"/> method.
		/// </para>
		/// </remarks>
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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem" /> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileSystem"/> does not contain a <see cref="GorgonFileSystemRamDiskProvider"/>.</exception>
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

		    _ramFiles = _provider?.FileData ?? throw new ArgumentException(Resources.GORFS_ERR_NO_RAMDISK_PROVIDER, nameof(fileSystem));
			_fileSystem = fileSystem;
		}
		#endregion
	}
}
