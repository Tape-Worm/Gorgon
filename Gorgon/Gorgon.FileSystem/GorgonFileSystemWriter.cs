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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;
using Gorgon.PlugIns;

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
	public class GorgonFileSystemWriter
		: GorgonPlugIn, IGorgonFileSystemWriter<FileStream>
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
		/// <summary>
		/// Property to return the file system linked to this writable area.
		/// </summary>
		public IGorgonFileSystem FileSystem => _fileSystem;

		/// <summary>
		/// Property to return the location on the physical file system to use as the writable area for a <see cref="IGorgonFileSystem"/>.
		/// </summary>
		/// <remarks>
		/// This value may return <b>null</b> or an empty string if there's no actual location on a physical file system (e.g. the file system is located in memory).
		/// </remarks>
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
        private string GetWriteFilePath(string directoryName, string fileName) => GetWriteDirectoryPath(directoryName) + fileName;

        /// <summary>
        /// Function to copy data from a file system to the file system linked to this writer.
        /// </summary>
        /// <param name="sourceFileSystem">The file system to copy from.</param>
        /// <param name="progress">The callback for copy progress.</param>
        /// <param name="allowOverwrite">Flag to indicate whether to allow overwriting files or not.</param>
        /// <param name="token">The cancellation token for asynchronous copy.</param>
        /// <returns>A tuple containing the count of the directories and files copied.</returns>
        private (int DirectoryCount, int FileCount)? CopyInternal(IGorgonFileSystem sourceFileSystem,
		                                                          Func<GorgonWriterCopyProgress, bool> progress,		                                                          
		                                                          bool allowOverwrite,
                                                                  CancellationToken token)
		{
			int directoryCount = 0;
			int fileCount = 0;

			// Enumerate files and directories from the source.
			IGorgonVirtualFile[] files = sourceFileSystem.FindFiles("/", "*").ToArray();
			IGorgonVirtualDirectory[] directories = sourceFileSystem.FindDirectories("/", "*").ToArray();

			if ((files.Length == 0) && (directories.Length == 0))
			{
				return (0, 0);
			}

			// Create all the directories.
			foreach (IGorgonVirtualDirectory directory in directories)
			{
				if (token.IsCancellationRequested)
				{
					return null;
				}

				CreateDirectory(directory.FullPath);
				++directoryCount;
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
					return null;
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
			
			return CopyInternal(sourceFileSystem, copyProgress, allowOverwrite, CancellationToken.None);
		}

		/// <summary>
		/// Function to asynchronously copy the contents of a file system to the writable area.
		/// </summary>
		/// <param name="sourceFileSystem">The <see cref="IGorgonFileSystem"/> to copy.</param>
		/// <param name="cancelToken">The <see cref="CancellationToken"/> used to cancel an in progress copy.</param>
		/// <param name="copyProgress">A method callback used to track the progress of the copy operation.</param>
		/// <param name="allowOverwrite">[Optional] <b>true</b> to allow overwriting of files that already exist in the file system with the same path, <b>false</b> to throw an exception when a file with the same path is encountered.</param>
		/// <returns>A <see cref="Tuple{T1,T2}"/> containing the number of directories (<c>item1</c>) and the number of files (<c>item2</c>) copied, or <b>null</b> if the operation was cancelled.</returns>
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
		public Task<(int DirectoryCount, int FileCount)?> CopyFromAsync(IGorgonFileSystem sourceFileSystem, CancellationToken cancelToken, Func<GorgonWriterCopyProgress, bool> copyProgress = null, bool allowOverwrite = true)
		{
			if (sourceFileSystem == null)
			{
				throw new ArgumentNullException(nameof(sourceFileSystem));
			}

			// ReSharper disable MethodSupportsCancellation
			return Task.Run(() => CopyInternal(sourceFileSystem, copyProgress, allowOverwrite, cancelToken), cancelToken);
			// ReSharper restore MethodSupportsCancellation
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
			PrepareWriteArea();

			Unmount();

			lock (_syncLock)
			{
				FileSystem.Mount(WriteLocation, "/");
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
			lock (_syncLock)
			{
				if (!FileSystem.MountPoints.Contains(_mountPoint))
				{
					return;
				}

				FileSystem.Unmount(_mountPoint);
			}
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
			IEnumerable<DirectoryInfo> directories = dirInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
			IEnumerable<FileSystemInfo> files = dirInfo.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

			foreach (DirectoryInfo directoryPath in directories)
			{
				directoryPath.Delete(true);
			}

			foreach (FileSystemInfo file in files)
			{
				file.Delete();
			}
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
		public FileStream OpenStream(string path, FileMode mode)
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
		/// Initializes a new instance of the <see cref="GorgonFileSystemWriter"/> class.
		/// </summary>
		/// <param name="fileSystem">A file system used to track the updates when writing.</param>
		/// <param name="writeLocation">The directory on the physical file system to actually write data into.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, or the <paramref name="writeLocation"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="writeLocation"/> is empty.</exception>
		public GorgonFileSystemWriter(GorgonFileSystem fileSystem, string writeLocation)
			: base(Resources.GORFS_FOLDER_WRITER_FS_DESC)
		{
			if (writeLocation == null)
			{
				throw new ArgumentNullException(nameof(writeLocation));
			}

			if (string.IsNullOrWhiteSpace(writeLocation))
			{
				throw new ArgumentEmptyException(nameof(writeLocation));
			}

			// We need the concrete type in here because we need access to its internals.
			_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
			WriteLocation = writeLocation.FormatDirectory(Path.DirectorySeparatorChar);
			_mountPoint = new GorgonFileSystemMountPoint(fileSystem.DefaultProvider, WriteLocation, "/");
		}
		#endregion
	}
}
