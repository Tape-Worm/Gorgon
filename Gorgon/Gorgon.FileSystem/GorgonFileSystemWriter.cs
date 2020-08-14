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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;
using Gorgon.Math;
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
        #region Events.
        /// <summary>
        /// Event triggered when a virtual directory has been added to the file system.
        /// </summary>
        public event EventHandler<VirtualDirectoryAddedEventArgs> VirtualDirectoryAdded;
        /// <summary>
        /// Event triggered when a virtual directory has been deleted from the file system.
        /// </summary>
        public event EventHandler<VirtualDirectoryDeletedEventArgs> VirtualDirectoryDeleted;
        /// <summary>
        /// Event triggered when a virtual directory has been renamed.
        /// </summary>
        public event EventHandler<VirtualDirectoryRenamedEventArgs> VirtualDirectoryRenamed;
        /// <summary>
        /// Event triggered when a virtual directory has been copied.
        /// </summary>
        public event EventHandler<VirtualDirectoryCopiedMovedEventArgs> VirtualDirectoryCopied;
        /// <summary>
        /// Event triggered when a virtual directory has been moved.
        /// </summary>
        public event EventHandler<VirtualDirectoryCopiedMovedEventArgs> VirtualDirectoryMoved;
        /// <summary>
        /// Event triggered before a phsyical file is imported into the file system.
        /// </summary>
        public event EventHandler<FileImportingArgs> FileImporting;
        /// <summary>
        /// Event triggered after a physical file is imported into the file system.
        /// </summary>
        public event EventHandler<FileImportedArgs> FileImported;
        /// <summary>
        /// Event triggered when directories, and files have been imported.
        /// </summary>
        public event EventHandler<ImportedEventArgs> Imported;
        /// <summary>
        /// Event triggered when virtual files were copied.
        /// </summary>
        public event EventHandler<VirtualFileCopiedMovedEventArgs> VirtualFileCopied;
        /// <summary>
        /// Event triggered when a virtual file has been deleted from the file system.
        /// </summary>
        public event EventHandler<VirtualFileDeletedEventArgs> VirtualFileDeleted;
        /// <summary>
        /// Event triggered when a virtual file has been opened for writing on the file system.
        /// </summary>
        public event EventHandler<VirtualFileOpenedEventArgs> VirtualFileOpened;
        /// <summary>
        /// Event triggered when a virtual file has had its write stream closed.
        /// </summary>
        public event EventHandler<VirtualFileClosedEventArgs> VirtualFileClosed;
        /// <summary>
        /// Event triggered when a virtual file has been renamed.
        /// </summary>
        public event EventHandler<VirtualFileRenamedEventArgs> VirtualFileRenamed;
        /// <summary>
        /// Event triggered when virtual files were moved.
        /// </summary>
        public event EventHandler<VirtualFileCopiedMovedEventArgs> VirtualFileMoved;
        #endregion

        #region Variables.
        // The list of invalid file name characters.
        private readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
        // Locking synchronization for multiple threads.
        private readonly object _syncLock = new object();
        // The mount point for the write area.
        private readonly GorgonFileSystemMountPoint _mountPoint;
        // The notifier used to update the file system.
        private readonly IGorgonFileSystemNotifier _notifier;
        // The callback used to delete file system items.
        private readonly Func<string, bool> _deleteAction;
        // The buffer used to block copy a file.
        private byte[] _writeBuffer;
        // The buffer used for building a path.
        private readonly StringBuilder _pathBuffer = new StringBuilder(1024);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the file system linked to this writable area.
        /// </summary>
        public IGorgonFileSystem FileSystem
        {
            get;
        }

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
        /// Function to replace a path with a new root path.
        /// </summary>
        /// <param name="dirToUpdate">The directory that will have its path updated.</param>
        /// <param name="pathToReplace">The path to replace.</param>
        /// <param name="pathReplacement">The path to use as the replacement.</param>
        /// <returns>The updated path.</returns>
        private string ReplacePath(IGorgonVirtualDirectory dirToUpdate, string pathToReplace, string pathReplacement)
        {
            _pathBuffer.Length = 0;

            if (dirToUpdate.Parent != null)
            {
                _pathBuffer.Append(dirToUpdate.FullPath);
                _pathBuffer.Remove(0, pathToReplace.Length);
            }

            // For the root directory, we only need to copy the files.
            _pathBuffer.Insert(0, pathReplacement);

            return _pathBuffer.ToString();
        }

        /// <summary>
        /// Function to retrieve a new file name that is guaranteed to not exist.
        /// </summary>
        /// <param name="dirPath">The path to the physical directory.</param>
        /// <param name="fileName">The desired name for the file.</param>
        /// <returns>The updated file name.</returns>
        private string GetNewName(string dirPath, string fileName)
        {
            string filePath = Path.Combine(dirPath, fileName);
            if ((!File.Exists(filePath)) && (!Directory.Exists(filePath)))
            {
                return fileName;
            }

            var newPath = new StringBuilder(fileName);
            string file = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            int counter = 1;

            while ((File.Exists(filePath)) || (Directory.Exists(filePath)))
            {
                newPath.Length = 0;
                newPath.Append(file);
                newPath.Append(" (");
                newPath.Append(counter++);
                newPath.Append(')');
                if (!string.IsNullOrWhiteSpace(ext))
                {
                    newPath.Append(ext);
                }

                filePath = Path.Combine(dirPath, newPath.ToString());
            }

            return newPath.ToString();
        }

        /// <summary>
        /// Function to retrieve a new file name that is guaranteed to not exist.
        /// </summary>
        /// <param name="dir">The directory to evaluate.</param>
        /// <param name="fileName">The desired name for the file.</param>
        /// <returns>The updated file name.</returns>
        private string GetNewName(IGorgonVirtualDirectory dir, string fileName)
        {
            if (!dir.Files.Contains(fileName))
            {
                return fileName;
            }

            var newPath = new StringBuilder(fileName);
            string file = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            int counter = 1;

            while (dir.Files.Contains(newPath.ToString()))
            {
                newPath.Length = 0;
                newPath.Append(file);
                newPath.Append(" (");
                newPath.Append(counter++);
                newPath.Append(')');
                if (!string.IsNullOrWhiteSpace(ext))
                {
                    newPath.Append(ext);
                }
            }

            return newPath.ToString();
        }

        /// <summary>
        /// Function to perform the copy of a file.
        /// </summary>
        /// <param name="srcPath">The path for the source stream.</param>
        /// <param name="destPath">The path for the destination stream.</param>
        /// <param name="virtDestPath">The virtual path for the destination.</param>
        /// <param name="inStream">The source stream to copy.</param>
        /// <param name="outStream">The destination stream for the file data.</param>
        /// <param name="progressCallback">The callback method used to report progress.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// <para>
        /// This performs a block copy of a file from one location to another. These locations can be anywhere on the physical file system(s) and are not checked.  
        /// </para>
        /// </remarks>
        private void BlockCopyStreams(string srcPath, string destPath, string virtDestPath, Stream inStream, Stream outStream, Action<string, double> progressCallback, CancellationToken cancelToken)
        {
            long maxBlockSize = _writeBuffer.Length;

            progressCallback?.Invoke(srcPath, 0);

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            long fileSize = inStream.Length;

            // If we're under the size of the write buffer, then we can copy as-is, and we can report a 1:1 file copy.
            if (fileSize <= maxBlockSize)
            {
                inStream.CopyToStream(outStream, (int)fileSize, _writeBuffer);
                progressCallback?.Invoke(srcPath, 1.0);
                return;
            }

            // Otherwise, we need to break up the file into chunks to get reporting of file copy progress.
            long blockSize = (maxBlockSize.Min(fileSize));

            while (fileSize > 0)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    outStream.Close();

                    // Don't leave 1/2 copied files laying around.
                    try
                    {
                        File.Delete(destPath);
                    }
                    catch
                    {
                        // Do nothing if we can't remove the file.
                    }

                    if (!string.IsNullOrWhiteSpace(virtDestPath))
                    {
                        _notifier.NotifyFileDeleted(virtDestPath);
                    }

                    return;
                }

                inStream.CopyToStream(outStream, (int)blockSize, _writeBuffer);
                fileSize -= blockSize;
                blockSize = maxBlockSize.Min(fileSize);

                progressCallback?.Invoke(srcPath, (double)inStream.Position / inStream.Length);
            }
        }

        /// <summary>
        /// Function to perform the copy of a file to the physical file system..
        /// </summary>
        /// <param name="srcFile">The file to copy.</param>
        /// <param name="destPath">The destination path on the physical file system to copy the file into.</param>
        /// <param name="progressCallback">The callback method used to report progress.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// <para>
        /// This performs a block copy of a file from one location to another. These locations can be anywhere on the physical file system(s) and are not checked.  
        /// </para>
        /// </remarks>
        private void BlockCopyFileToPhysical(IGorgonVirtualFile srcFile, string destPath, Action<string, double> progressCallback, CancellationToken cancelToken)
        {
            using (Stream inStream = srcFile.OpenStream())
            using (Stream outStream = File.Open(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BlockCopyStreams(srcFile.FullPath, destPath, null, inStream, outStream, progressCallback, cancelToken);
            }
        }

        /// <summary>
        /// Function to perform the copy of a file from the physical file system.
        /// </summary>
        /// <param name="srcFile">The file to copy from the physical file system.</param>
        /// <param name="destPath">The destination path on the virtual file system to copy the file into.</param>
        /// <param name="progressCallback">The callback method used to report progress.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// <para>
        /// This performs a block copy of a file from one location to another. These locations can be anywhere on the physical file system(s) and are not checked.  
        /// </para>
        /// </remarks>
        private void BlockCopyFileFromPhysical(string srcFile, string destPath, Action<string, double> progressCallback, CancellationToken cancelToken)
        {
            using (Stream inStream = File.Open(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outStream = OpenStream(destPath, FileMode.Create))
            {
                BlockCopyStreams(srcFile, GetWriteFilePath(Path.GetDirectoryName(destPath), Path.GetFileName(destPath)), destPath, inStream, outStream, progressCallback, cancelToken);
            }
        }

        /// <summary>
        /// Function to perform the copy of a file.
        /// </summary>
        /// <param name="srcFile">The file to copy.</param>
        /// <param name="destPath">The destination path on the virtual file system to copy the file into.</param>
        /// <param name="progressCallback">The callback method used to report progress.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// <para>
        /// This performs a block copy of a file from one location to another. These locations can be anywhere on the physical file system(s) and are not checked.  
        /// </para>
        /// </remarks>
        private void BlockCopyFile(IGorgonVirtualFile srcFile, string destPath, Action<string, double> progressCallback, CancellationToken cancelToken)
        {
            using (Stream inStream = srcFile.OpenStream())
            using (Stream outStream = OpenStream(destPath, FileMode.Create))
            {
                BlockCopyStreams(srcFile.FullPath, GetWriteFilePath(Path.GetDirectoryName(destPath), Path.GetFileName(destPath)), destPath, inStream, outStream, progressCallback, cancelToken);
            }
        }

        /// <summary>
        /// Function to prepare the specified directory for use as a file system write area.
        /// </summary>
        private void PrepareWriteArea()
        {
            lock (_syncLock)
            {
                // If the area specified by the write directory does not exist yet, then 
                // create it.
                if (!Directory.Exists(WriteLocation))
                {
                    Directory.CreateDirectory(WriteLocation);
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
        /// Function to delete a virtual file from the physical and virtual file systems.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        /// <returns><b>true</b> if the physical file was deleted, or <b>false</b> if not.</returns>
        private bool DeleteVirtualFile(IGorgonVirtualFile file)
        {
            bool result = false;
            string physicalPath = GetWriteFilePath(file.Directory.FullPath, file.Name);

            // If the file doesn't exist, then we don't need to actually delete it.
            if (File.Exists(physicalPath))
            {
                if (_deleteAction == null)
                {
                    File.Delete(physicalPath);
                }
                else
                {
                    if (!_deleteAction(physicalPath))
                    {
                        return false;
                    }
                }

                result = true;
            }

            _notifier.NotifyFileDeleted(file.FullPath);

            return result;
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
        /// <see cref="IGorgonFileSystem.Refresh()"/> method.
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

            path = path.FormatDirectory('/');

            PrepareWriteArea();            

            string writePath = GetWriteDirectoryPath(path);

            if (!Directory.Exists(writePath))
            {
                Directory.CreateDirectory(writePath);
            }

            IGorgonVirtualDirectory result = _notifier.NotifyDirectoryAdded(_mountPoint, path);

            EventHandler<VirtualDirectoryAddedEventArgs> handler = VirtualDirectoryAdded;
            handler?.Invoke(this, new VirtualDirectoryAddedEventArgs(result));
            
            return result;
        }

        /// <summary>
        /// Function to delete a directory from the writeable area with progress functionality.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        /// <param name="onDelete">[Optional] The callback method to execute when a directory, or file is deleted.</param>
        /// <param name="cancelToken">[Optional] The token used to determine if the operation should be canceled or not.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="path"/> could not be found.</exception>
        /// <remarks>
        /// <para>
        /// This will delete a directory, its sub directories, and any files under this directory and sub directories. It will remove all references to those files and directories from the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, 
        /// and will delete these items from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> if they exist in that location.
        /// </para>
        /// <para>
        /// When a directory is removed from the <see cref="FileSystem"/>, only the directory in the <see cref="WriteLocation"/> is physically removed. The items in the other file system mount points will be removed 
        /// from the mounted directory list(s), but not from the other mount points. That is, anything mounted outside of the write location will be preserved and can be re-added to the directory list(s) by calling 
        /// <see cref="IGorgonFileSystem.Refresh()"/>.
        /// </para>
        /// <para>
        /// This method provides the means to receive feedback during the deletion operation, and a means to cancel the operation. This is useful in cases where a UI is present and a delete operation can take a long time to return 
        /// to the user. The callback method takes a single string parameter which represents the full path to the directory,subdirectory, or file being deleted. 
        /// </para>
        /// <para>
        /// If the <paramref name="path"/> is set to the root of the file system (<c>/</c>), then the entire file system will be deleted, but the root directory will always remain.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted directories in the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// This method will use the delete callback action passed to the constructor (if supplied) to perform custom deletion of file system items. If no action is provided, then the files and directories 
        /// deleted by this method are erased and cannot be recovered easily.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualDirectoryDeleted"/> event.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonFileSystem.Refresh()"/>        
        public void DeleteDirectory(string path, Action<string> onDelete = null, CancellationToken? cancelToken = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IGorgonVirtualDirectory directory = FileSystem.GetDirectory(path);

            if (directory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
            }

            // Build up a list of directories to delete.
            // We'll sort these by full path length since it is impossible to have a child directory with a longer name than its parent.
            var directories = directory.Directories.Traverse(d => d.Directories)
                                                   .OrderByDescending(d => d.FullPath.Length)
                                                   .ToList();

            // If we've deleted a sub directory (i.e. not the root), then include it in the list as well.
            directories.Add(directory);

            if (cancelToken == null)
            {
                cancelToken = CancellationToken.None;
            }

            if (cancelToken.Value.IsCancellationRequested)
            {
                return;
            }

            PrepareWriteArea();

            string physicalPath;
            var deletedDirectories = new List<IGorgonVirtualDirectory>();
            var deletedFiles = new List<IGorgonVirtualFile>();

            // Function to trigger the event(s) upon operation completion.
            void OnDeleteComplete()
            {
                if (deletedFiles.Count > 0)
                {
                    EventHandler<VirtualFileDeletedEventArgs> fileDeleteHandler = VirtualFileDeleted;
                    fileDeleteHandler?.Invoke(this, new VirtualFileDeletedEventArgs(deletedFiles));
                }

                if (deletedDirectories.Count == 0)
                {
                    return;
                }

                EventHandler<VirtualDirectoryDeletedEventArgs> dirDeleteHandler = VirtualDirectoryDeleted;
                dirDeleteHandler?.Invoke(this, new VirtualDirectoryDeletedEventArgs(deletedDirectories));
            }

            foreach (IGorgonVirtualDirectory dir in directories)
            {
                if (cancelToken.Value.IsCancellationRequested)
                {
                    OnDeleteComplete();
                    return;
                }

                IGorgonVirtualFile[] files = dir.Files.ToArray();
                foreach (IGorgonVirtualFile file in files)
                {
                    if (cancelToken.Value.IsCancellationRequested)
                    {
                        OnDeleteComplete();
                        return;
                    }
                    

                    onDelete?.Invoke(file.FullPath);
                    if (DeleteVirtualFile(file))
                    {
                        deletedFiles.Add(file);
                    }
                }
                
                onDelete?.Invoke(dir.FullPath);
                physicalPath = GetWriteDirectoryPath(dir.FullPath);

                // Do not delete the root.
                if (dir.Parent == null)
                {
                    continue;
                }

                if (Directory.Exists(physicalPath))
                {
                    if (_deleteAction == null)
                    {
                        Directory.Delete(physicalPath);
                    }
                    else
                    {
                        if (!_deleteAction(physicalPath))
                        {
                            continue;
                        }
                    }

                    deletedDirectories.Add(dir);
                }

                _notifier.NotifyDirectoryDeleted(dir.FullPath);
            }

            OnDeleteComplete();
        }

        /// <summary>
        /// Function to delete a file from the file system.
        /// </summary>
        /// <param name="path">The path to the file to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file specified in the <paramref name="path"/> was not found in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This will delete a single file from the file system, and physically remove it from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>. 
        /// </para>
        /// <para>
        /// When a file is removed from the <see cref="FileSystem"/>, only the file in the <see cref="WriteLocation"/> is physically removed. The items in the other file system mount points will be removed 
        /// from the mounted directory list(s), but not from the other mount point physical locations. That is, anything mounted outside of the write location will be preserved and can be re-added to the 
        /// directory list(s) by calling <see cref="IGorgonFileSystem.Refresh()"/>.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted files in the 
        /// <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// This method will use the delete callback action passed to the constructor (if supplied) to perform custom deletion of file system items. If no action is provided, then the files and directories 
        /// deleted by this method are erased and cannot be recovered easily.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualFileDeleted"/> event.
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

            IGorgonVirtualFile file = FileSystem.GetFile(path);

            if (file == null)
            {
                throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
            }

            if (!DeleteVirtualFile(file))
            {
                return;
            }

            EventHandler<VirtualFileDeletedEventArgs> handler = VirtualFileDeleted;
            handler?.Invoke(this, new VirtualFileDeletedEventArgs(new[] { file }));
        }

        /// <summary>
        /// Function to delete multiple files from the file system.
        /// </summary>
        /// <param name="paths">The path to the files to delete.</param>
        /// <param name="onDelete">[Optional] The callback method to execute when a directory, or file is deleted.</param>
        /// <param name="cancelToken">[Optional] The token used to determine if the operation should be canceled or not.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="paths"/> parameter is <b>null</b>.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file specified in the <paramref name="paths"/> was not found in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This will delete multiple files from the file system, and physically remove them from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>. 
        /// </para>
        /// <para>
        /// This method provides the means to receive feedback during the deletion operation, and a means to cancel the operation. This is useful in cases where a UI is present and a delete operation can 
        /// take a long time to return to the user. The callback method takes a single string parameter which represents the full path to the file being deleted. 
        /// </para>
        /// <para>
        /// When a file is removed from the <see cref="FileSystem"/>, only the file in the <see cref="WriteLocation"/> is physically removed. The items in the other file system mount points will be removed 
        /// from the mounted directory list(s), but not from the other mount point physical locations. That is, anything mounted outside of the write location will be preserved and can be re-added to the 
        /// directory list(s) by calling <see cref="IGorgonFileSystem.Refresh()"/>.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted files in the 
        /// <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// This method will use the delete callback action passed to the constructor (if supplied) to perform custom deletion of file system items. If no action is provided, then the files and directories 
        /// deleted by this method are erased and cannot be recovered easily.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualFileDeleted"/> event.
        /// </para>
        /// </remarks>
        public void DeleteFiles(IEnumerable<string> paths, Action<string> onDelete = null, CancellationToken? cancelToken = null)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            IGorgonVirtualFile[] files = paths.Where(item => !string.IsNullOrWhiteSpace(item))
                                                  .Select(item => FileSystem.GetFile(item))                                      
                                                  .ToArray();

            if (files.Length == 0)
            {
                return;
            }

            var deletedFiles = new List<IGorgonVirtualFile>();
            if (cancelToken == null)
            {
                cancelToken = CancellationToken.None;
            }

            for (int i = 0; i < files.Length; ++i)
            {
                if (cancelToken.Value.IsCancellationRequested)
                {
                    break;
                }

                IGorgonVirtualFile file = files[i];
                onDelete?.Invoke(file.FullPath);

                if (file == null)
                {
                    throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, file.FullPath));
                }

                string physicalPath = GetWriteFilePath(file.Directory.FullPath, file.Name);

                if (DeleteVirtualFile(file))
                {
                    deletedFiles.Add(file);
                }
            }

            EventHandler<VirtualFileDeletedEventArgs> handler = VirtualFileDeleted;
            handler?.Invoke(this, new VirtualFileDeletedEventArgs(deletedFiles));
        }

        /// <summary>
        /// Function to rename a file.
        /// </summary>
        /// <param name="path">The path to the file to rename.</param>
        /// <param name="newName">The new name of the file.</param>
        /// <remarks>
        /// <para>
        /// This will change the name of the specified file in the <paramref name="path" /> to the name specified by <paramref name="newName" />. The <paramref name="newName" /> must only contain the name
        /// of the file, and no path information. This <paramref name="newName" /> must also not already exist as a file or directory name in the same <paramref name="path" />.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualFileRenamed"/> event.
        /// </para>
        /// </remarks>
        public void RenameFile(string path, string newName)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            IGorgonVirtualFile file = FileSystem.GetFile(path);

            if (file == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
            }

            if ((file.Directory.Directories.Contains(newName)) || (file.Directory.Files.Contains(newName)))
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, newName));
            }

            if (newName.Any(item => _invalidFileNameChars.IndexOf(item) != -1))
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_ILLEGAL_PATH, newName));
            }

            string oldName = file.Name;
            string oldPath = file.FullPath;
            string physicalPath = GetWriteFilePath(file.Directory.FullPath, file.Name);
            string newPhysicalPath = GetWriteFilePath(file.Directory.FullPath, newName);

            if (File.Exists(physicalPath))
            {
                File.Move(physicalPath, newPhysicalPath);
            }

            _notifier.NotifyFileRenamed(_mountPoint, oldPath, new PhysicalFileInfo(_mountPoint, newPhysicalPath));

            EventHandler<VirtualFileRenamedEventArgs> handler = VirtualFileRenamed;
            handler?.Invoke(this, new VirtualFileRenamedEventArgs(file, oldName));
        }

        /// <summary>
        /// Function to rename a directory.
        /// </summary>
        /// <param name="path">The path to the directory to rename.</param>
        /// <param name="newName">The new name of the directory.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="newName"/>, or the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/>, or the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="newName"/> is already in use by a file or directory.
        /// <para>-or-</para>
        /// <para>The <paramref name="newName"/> contains illegal characters.</para>
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory specified by <paramref name="path"/> does not exist in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This will change the name of the specified directory in the <paramref name="path"/> to the name specified by <paramref name="newName"/>. The <paramref name="newName"/> must only contain the name 
        /// of the directory, and no path information. This <paramref name="newName"/> must also not already exist as a file or directory name in the same <paramref name="path"/>.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualDirectoryRenamed"/> event.
        /// </para>
        /// </remarks>
        public void RenameDirectory(string path, string newName)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            IGorgonVirtualDirectory directory = FileSystem.GetDirectory(path);

            if (directory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
            }

            if ((directory.Directories.Contains(newName)) || (directory.Files.Contains(newName)))
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, newName));
            }

            // We cannot rename the root, so do nothing.
            if (directory.Parent == null)
            {
                return;
            }            

            if (newName.Any(item => _invalidFileNameChars.IndexOf(item) != -1))
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_ILLEGAL_PATH, newName));
            }

            string oldName = directory.Name;
            string oldPath = directory.FullPath;
            string physicalPath = GetWriteDirectoryPath(directory.FullPath);
            string newPhysicalPath = GetWriteDirectoryPath(directory.Parent.FullPath + newName);

            if (Directory.Exists(physicalPath))
            {
                // .NET has no facility (at least none that I'm aware of) to rename a directory, so we move it instead, hence why we need the full path for the new name.
                Directory.Move(physicalPath, newPhysicalPath);
            }

            _notifier.NotifyDirectoryRenamed(_mountPoint, oldPath, newPhysicalPath, newName);

            EventHandler<VirtualDirectoryRenamedEventArgs> handler = VirtualDirectoryRenamed;
            handler?.Invoke(this, new VirtualDirectoryRenamedEventArgs(directory, oldName));
        }

        /// <summary>
        /// Function to move a directory to another directory.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to move.</param>
        /// <param name="destDirectoryPath">The destination directory path that will receive the moved data.</param>
        /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> could not be located in the file system.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="directoryPath"/> is an ancestor of the <paramref name="destDirectoryPath"/>.
        /// <para>-or-</para>
        /// <para>Thrown if the <paramref name="directoryPath"/> is set to the root (<c>/</c>) directory.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method moves a directory, its sub directories and all files under those directories to a new directory path. 
        /// </para>
        /// <para>
        /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
        /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
        /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
        /// </para>
        /// <para>
        /// If the <paramref name="directoryPath"/>, and the <paramref name="destDirectoryPath"/> point to the same location, then this method will return immediately without doing anything. 
        /// </para>
        /// <para>
        /// When moving a directory the <paramref name="directoryPath"/> must not be an ancestor of the <paramref name="destDirectoryPath"/>, and the <paramref name="directoryPath"/> must not be the root 
        /// (<c>/</c>) directory. An exception will be thrown if either condition is true.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualDirectoryMoved"/> event.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonCopyCallbackOptions"/>
        public void MoveDirectory(string directoryPath, string destDirectoryPath, GorgonCopyCallbackOptions options = null)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentEmptyException(nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }

            IGorgonVirtualDirectory srcDirectory = FileSystem.GetDirectory(directoryPath);
            IGorgonVirtualDirectory destDirectory = FileSystem.GetDirectory(destDirectoryPath);

            if (srcDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryPath));
            }

            if (srcDirectory == FileSystem.RootDirectory)
            {
                throw new IOException(Resources.GORFS_ERR_CANNOT_MOVE_ROOT);
            }

            if (destDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destDirectoryPath));
            }

            // Ensure that the directory we're move is not an ancestor of the destination.
            if (destDirectory.GetParents().Any(item => item == srcDirectory))
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_MOVE_TO_CHILD, directoryPath, destDirectoryPath));
            }

            // If the source and destination are the same, then there's really nothing to do, so we can early exit.
            if (srcDirectory == destDirectory)
            {
                return;
            }

            try
            {
                PrepareWriteArea();

                _writeBuffer = ArrayPool<byte>.Shared.Rent(262144);
                var pathBuffer = new StringBuilder(1024);
                var dirsCopied = new List<(IGorgonVirtualDirectory src, IGorgonVirtualDirectory dest)>();
                var filesCopied = new List<(IGorgonVirtualFile src, IGorgonVirtualFile dest)>();
                CancellationToken cancelToken = options?.CancelToken ?? CancellationToken.None;
                Action<string, double> progressCallback = options?.ProgressCallback;
                Func<string, string, FileConflictResolution> conflictCallback = options?.ConflictResolutionCallback;
                FileConflictResolution conflictRes = FileConflictResolution.Overwrite;

                // Function to fire the event when the move operation completes.
                void OnMoveComplete()
                {
                    if ((dirsCopied.Count == 0) && (filesCopied.Count == 0))
                    {
                        return;
                    }

                    EventHandler<VirtualDirectoryCopiedMovedEventArgs> handler = VirtualDirectoryMoved;
                    handler?.Invoke(this, new VirtualDirectoryCopiedMovedEventArgs(destDirectory, dirsCopied, filesCopied));
                }

                progressCallback?.Invoke(srcDirectory.FullPath, 0);

                if (cancelToken.IsCancellationRequested)
                {
                    OnMoveComplete();
                    return;
                }

                var dirsToCopy = new List<IGorgonVirtualDirectory>
                {
                    srcDirectory
                };
                dirsToCopy.AddRange(srcDirectory.Directories.Traverse(d => d.Directories));
                var filesToCopy = dirsToCopy.SelectMany(f => f.Files)
                                            .GroupBy(f => f.Directory)
                                            .ToDictionary(d => d.Key, f => (IEnumerable<IGorgonVirtualFile>)f);

                for (int i = 0; i < dirsToCopy.Count; ++i)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        OnMoveComplete();
                        return;
                    }

                    IGorgonVirtualDirectory srcDir = dirsToCopy[i];
                    string destDirPath = ReplacePath(srcDir, (srcDirectory.Parent?.FullPath ?? srcDirectory.FullPath), destDirectory.FullPath);

                    progressCallback?.Invoke(srcDir.FullPath, 0);

                    // Create a new path for the copied files.
                    IGorgonVirtualDirectory destDir = FileSystem.GetDirectory(destDirPath);

                    if (destDir == null)
                    {
                        destDir = CreateDirectory(destDirPath);
                        dirsCopied.Add((srcDir, destDir));                        
                    }

                    if (!filesToCopy.TryGetValue(srcDir, out IEnumerable<IGorgonVirtualFile> files))
                    {
                        continue;
                    }

                    // Copy files for the current directory.
                    foreach (IGorgonVirtualFile srcFile in files)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            OnMoveComplete();
                            return;
                        }

                        string fileName = srcFile.Name;
                        string destFilePath = destDirPath + fileName;
                        bool fileExists = destDir.Files.Contains(fileName);
                        bool directoryExists = destDir.Directories.Contains(fileName);

                        if ((conflictRes != FileConflictResolution.OverwriteAll) && (conflictCallback != null)
                                && ((fileExists) || (directoryExists)))
                        {
                            if ((directoryExists) || (conflictRes != FileConflictResolution.SkipAll))
                            {
                                conflictRes = conflictCallback(srcFile.FullPath, destFilePath);
                            }

                            switch (conflictRes)
                            {                                
                                case FileConflictResolution.Skip:
                                case FileConflictResolution.SkipAll:
                                    continue;
                                case FileConflictResolution.Cancel:
                                    OnMoveComplete();
                                    return;
                                case FileConflictResolution.Exception:
                                    throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFilePath));
                            }
                        }

                        BlockCopyFile(srcFile, destFilePath, progressCallback, cancelToken);                        

                        // Delete the source file, we won't need it anymore.                        
                        if (File.Exists(srcFile.PhysicalFile.FullPath))
                        {
                            File.Delete(srcFile.PhysicalFile.FullPath);
                        }
                        _notifier.NotifyFileDeleted(srcFile.FullPath);

                        IGorgonVirtualFile destFile = FileSystem.GetFile(destFilePath);

                        if ((destFile != null) && (!cancelToken.IsCancellationRequested))
                        {
                            filesCopied.Add((srcFile, destFile));
                        }
                    }
                }

                foreach (IGorgonVirtualDirectory srcDir in dirsToCopy.OrderByDescending(item => item.FullPath.Length))
                {
                    if ((srcDir.Directories.Count > 0) || (srcDir.Files.Count > 0))
                    {
                        _notifier.NotifyDirectoryDeleted(srcDir.FullPath);
                        continue;
                    }

                    string physicalPath = _mountPoint.Provider.MapToPhysicalPath(srcDir.FullPath, _mountPoint);
                    if (Directory.Exists(physicalPath))
                    {
                        Directory.Delete(physicalPath);                        
                    }

                    // If we deleted the directory and we haven't created one (because it probably already existed), then just add a src directory 
                    // so we can update the UI.
                    if (!dirsCopied.Contains((srcDir, null)))
                    {
                        dirsCopied.Add((srcDir, null));
                    }

                    _notifier.NotifyDirectoryDeleted(srcDir.FullPath);
                }

                OnMoveComplete();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer);
            }
        }

        /// <summary>
        /// Function to copy a directory to another directory.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to copy.</param>
        /// <param name="destDirectoryPath">The destination directory path that will receive the copied data.</param>
        /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> could not be located in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This method copies a directory, its sub directories and all files under those directories to a new directory path. 
        /// </para>
        /// <para>
        /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
        /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
        /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualDirectoryCopied"/> event.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonCopyCallbackOptions"/>
        public void CopyDirectory(string directoryPath, string destDirectoryPath, GorgonCopyCallbackOptions options = null)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentEmptyException(nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }

            IGorgonVirtualDirectory srcDirectory = FileSystem.GetDirectory(directoryPath);
            IGorgonVirtualDirectory destDirectory = FileSystem.GetDirectory(destDirectoryPath);

            if (srcDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryPath));
            }

            if (destDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destDirectoryPath));
            }

            try
            {
                PrepareWriteArea();

                _writeBuffer = ArrayPool<byte>.Shared.Rent(262144);
                var pathBuffer = new StringBuilder(1024);
                var dirsCopied = new List<(IGorgonVirtualDirectory src, IGorgonVirtualDirectory dest)>();
                var filesCopied = new List<(IGorgonVirtualFile src, IGorgonVirtualFile dest)>();
                CancellationToken cancelToken = options?.CancelToken ?? CancellationToken.None;
                Action<string, double> progressCallback = options?.ProgressCallback;
                Func<string, string, FileConflictResolution> conflictCallback = options?.ConflictResolutionCallback;
                FileConflictResolution conflictRes = FileConflictResolution.Overwrite;

                // Function to fire the event when the copy operation completes.
                void OnCopyComplete()
                {
                    if ((dirsCopied.Count == 0) && (filesCopied.Count == 0))
                    {
                        return;
                    }

                    EventHandler<VirtualDirectoryCopiedMovedEventArgs> handler = VirtualDirectoryCopied;
                    handler?.Invoke(this, new VirtualDirectoryCopiedMovedEventArgs(destDirectory, dirsCopied, filesCopied));
                }

                progressCallback?.Invoke(srcDirectory.FullPath, 0);

                if (cancelToken.IsCancellationRequested)
                {
                    OnCopyComplete();
                    return;
                }

                var dirsToCopy = new List<IGorgonVirtualDirectory>
                {
                    srcDirectory
                };
                dirsToCopy.AddRange(srcDirectory.Directories.Traverse(d => d.Directories));
                var filesToCopy = dirsToCopy.SelectMany(f => f.Files)
                                            .GroupBy(f => f.Directory)
                                            .ToDictionary(d => d.Key, f => (IEnumerable<IGorgonVirtualFile>)f);

                for (int i = 0; i < dirsToCopy.Count; ++i)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        OnCopyComplete();
                        return;
                    }

                    IGorgonVirtualDirectory srcDir = dirsToCopy[i];
                    string destDirPath = ReplacePath(srcDir, (srcDirectory.Parent?.FullPath ?? srcDirectory.FullPath), destDirectory.FullPath);

                    progressCallback?.Invoke(srcDir.FullPath, 0);

                    // Create a new path for the copied files.
                    IGorgonVirtualDirectory destDir = FileSystem.GetDirectory(destDirPath);

                    if (destDir == null)
                    {
                        destDir = CreateDirectory(destDirPath);
                        dirsCopied.Add((srcDir, destDir));
                    }
                                        
                    if (!filesToCopy.TryGetValue(srcDir, out IEnumerable<IGorgonVirtualFile> files))
                    {
                        continue;
                    }

                    // Copy files for the current directory.
                    foreach (IGorgonVirtualFile file in files)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            OnCopyComplete();
                            return;
                        }

                        string fileName = file.Name;
                        string destFilePath = destDirPath + fileName;
                        bool fileExists = destDir.Files.Contains(file.Name);
                        bool directoryExists = destDir.Directories.Contains(file.Name);

                        if ((conflictRes != FileConflictResolution.OverwriteAll) && (conflictCallback != null)
                            && ((fileExists) || (directoryExists)))
                        {
                            if ((directoryExists) || ((conflictRes != FileConflictResolution.RenameAll) && (conflictRes != FileConflictResolution.SkipAll)))
                            {
                                conflictRes = conflictCallback(file.FullPath, destFilePath);
                            }

                            switch (conflictRes)
                            {
                                case FileConflictResolution.Skip:
                                case FileConflictResolution.SkipAll:
                                    continue;
                                case FileConflictResolution.Rename:
                                case FileConflictResolution.RenameAll:
                                    fileName = GetNewName(destDir, fileName);
                                    destFilePath = destDirPath + fileName;
                                    break;
                                case FileConflictResolution.Cancel:
                                    OnCopyComplete();
                                    return;
                                case FileConflictResolution.Exception:
                                    throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFilePath));
                            }
                        }

                        BlockCopyFile(file, destFilePath, progressCallback, cancelToken);
                        IGorgonVirtualFile destFile = FileSystem.GetFile(destFilePath);

                        if ((destFile != null) && (!cancelToken.IsCancellationRequested))
                        {
                            filesCopied.Add((file, destFile));
                        }
                    }
                }

                OnCopyComplete();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer);
            }
        }

        /// <summary>
        /// Function to move files to another directory.
        /// </summary>
        /// <param name="filePaths">The path to the files to file.</param>
        /// <param name="destDirectoryPath">The destination directory path that will receive the moved data.</param>
        /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePaths"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when a file path in the <paramref name="filePaths"/> could not be located in the file system.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="destDirectoryPath"/> could not be located in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This method moves a file, or files, to a new directory path.
        /// </para>
        /// <para>
        /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
        /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
        /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualFileMoved"/> event.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonCopyCallbackOptions"/>
        public void MoveFiles(IEnumerable<string> filePaths, string destDirectoryPath, GorgonCopyCallbackOptions options = null)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException(nameof(filePaths));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }
            
            IGorgonVirtualDirectory destDirectory = FileSystem.GetDirectory(destDirectoryPath);

            if (destDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destDirectoryPath));
            }

            var files = new List<IGorgonVirtualFile>();

            foreach (string filePath in filePaths)
            {
                IGorgonVirtualFile file = FileSystem.GetFile(filePath);

                if (file == null)
                {
                    throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, filePath));
                }

                // Don't copy files to the same location.
                if (file.Directory == destDirectory)
                {
                    continue;
                }

                files.Add(file);
            }

            if (files.Count == 0)
            {
                return;
            }

            try
            {
                PrepareWriteArea();

                _writeBuffer = ArrayPool<byte>.Shared.Rent(262144);
                var pathBuffer = new StringBuilder(1024);
                var filesCopied = new List<(IGorgonVirtualFile src, IGorgonVirtualFile dest)>();
                CancellationToken cancelToken = options?.CancelToken ?? CancellationToken.None;
                Action<string, double> progressCallback = options?.ProgressCallback;
                Func<string, string, FileConflictResolution> conflictCallback = options?.ConflictResolutionCallback;
                FileConflictResolution conflictRes = FileConflictResolution.Overwrite;

                // Function to fire the event when the move operation completes.
                void OnMoveComplete()
                {
                    if (filesCopied.Count == 0)
                    {
                        return;
                    }

                    EventHandler<VirtualFileCopiedMovedEventArgs> handler = VirtualFileMoved;
                    handler?.Invoke(this, new VirtualFileCopiedMovedEventArgs(destDirectory, filesCopied));
                }

                progressCallback?.Invoke(files[0].FullPath, 0);

                if (cancelToken.IsCancellationRequested)
                {
                    OnMoveComplete();
                    return;
                }
                
                // Copy files for the current directory.
                foreach (IGorgonVirtualFile srcFile in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        OnMoveComplete();
                        return;
                    }

                    string fileName = srcFile.Name;
                    string destFilePath = destDirectory.FullPath + fileName;
                    bool fileExists = destDirectory.Files.Contains(fileName);
                    bool directoryExists = destDirectory.Directories.Contains(fileName);

                    if ((conflictRes != FileConflictResolution.OverwriteAll) && (conflictCallback != null)
                            && ((fileExists) || (directoryExists)))
                    {
                        if ((directoryExists) || (conflictRes != FileConflictResolution.SkipAll))
                        {
                            conflictRes = conflictCallback(srcFile.FullPath, destFilePath);
                        }

                        switch (conflictRes)
                        {
                            case FileConflictResolution.Skip:
                            case FileConflictResolution.SkipAll:
                                continue;
                            case FileConflictResolution.Cancel:
                                OnMoveComplete();
                                return;
                            case FileConflictResolution.Exception:
                                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFilePath));
                        }
                    }

                    BlockCopyFile(srcFile, destFilePath, progressCallback, cancelToken);

                    // Delete the source file, we won't need it anymore.                        
                    if (File.Exists(srcFile.PhysicalFile.FullPath))
                    {
                        File.Delete(srcFile.PhysicalFile.FullPath);
                    }
                    _notifier.NotifyFileDeleted(srcFile.FullPath);

                    IGorgonVirtualFile destFile = FileSystem.GetFile(destFilePath);

                    if ((destFile != null) && (!cancelToken.IsCancellationRequested))
                    {
                        filesCopied.Add((srcFile, destFile));
                    }
                }                

                OnMoveComplete();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer);
            }
        }

        /// <summary>
        /// Function to copy files to another directory.
        /// </summary>
        /// <param name="filePaths">The path to the files to copy.</param>
        /// <param name="destDirectoryPath">The destination directory path that will receive the copied data.</param>
        /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePaths"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when a file path in the <paramref name="filePaths"/> could not be located in the file system.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="destDirectoryPath"/> could not be located in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This method copies a file, or files, to a new directory path. 
        /// </para>
        /// <para>
        /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
        /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
        /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
        /// </para>
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualFileCopied"/> event.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonCopyCallbackOptions"/>
        public void CopyFiles(IEnumerable<string> filePaths, string destDirectoryPath, GorgonCopyCallbackOptions options = null)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException(nameof(filePaths));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }

            IGorgonVirtualDirectory destDirectory = FileSystem.GetDirectory(destDirectoryPath);

            if (destDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destDirectoryPath));
            }

            var files = new List<IGorgonVirtualFile>();

            foreach (string filePath in filePaths)
            {
                IGorgonVirtualFile file = FileSystem.GetFile(filePath);

                if (file == null)
                {
                    throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, filePath));
                }

                files.Add(file);
            }

            if (files.Count == 0)
            {
                return;
            }

            try
            {
                PrepareWriteArea();

                _writeBuffer = ArrayPool<byte>.Shared.Rent(262144);
                var pathBuffer = new StringBuilder(1024);
                var filesCopied = new List<(IGorgonVirtualFile src, IGorgonVirtualFile dest)>();
                CancellationToken cancelToken = options?.CancelToken ?? CancellationToken.None;
                Action<string, double> progressCallback = options?.ProgressCallback;
                Func<string, string, FileConflictResolution> conflictCallback = options?.ConflictResolutionCallback;
                FileConflictResolution conflictRes = FileConflictResolution.Overwrite;

                // Function to fire the event when the copy operation completes.
                void OnCopyComplete()
                {
                    if (filesCopied.Count == 0)
                    {
                        return;
                    }

                    EventHandler<VirtualFileCopiedMovedEventArgs> handler = VirtualFileCopied;
                    handler?.Invoke(this, new VirtualFileCopiedMovedEventArgs(destDirectory, filesCopied));
                }

                progressCallback?.Invoke(files[0].FullPath, 0);

                if (cancelToken.IsCancellationRequested)
                {
                    OnCopyComplete();
                    return;
                }

                // Copy files for the current directory.
                foreach (IGorgonVirtualFile file in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        OnCopyComplete();
                        return;
                    }

                    string fileName = file.Name;
                    string destFilePath = destDirectory.FullPath + fileName;
                    bool fileExists = destDirectory.Files.Contains(file.Name);
                    bool directoryExists = destDirectory.Directories.Contains(file.Name);

                    if ((conflictRes != FileConflictResolution.OverwriteAll) && (conflictCallback != null) 
                        && ((fileExists) || (directoryExists)))
                    {
                        if ((directoryExists) 
                            || ((conflictRes != FileConflictResolution.RenameAll) && (conflictRes != FileConflictResolution.SkipAll)))
                        {
                            // If we're copying the file to the same directory, we're essentially duplicating the file with a new name.
                            if (file == destDirectory.Files[file.Name])
                            {
                                conflictRes = FileConflictResolution.Rename;
                            }
                            else
                            {
                                conflictRes = conflictCallback(file.FullPath, destFilePath);
                            }
                        }

                        switch (conflictRes)
                        {
                            case FileConflictResolution.Skip:
                            case FileConflictResolution.SkipAll:
                                continue;
                            case FileConflictResolution.Rename:
                            case FileConflictResolution.RenameAll:
                                fileName = GetNewName(destDirectory, fileName);
                                destFilePath = destDirectory.FullPath + fileName;
                                break;
                            case FileConflictResolution.Cancel:
                                OnCopyComplete();
                                return;
                            case FileConflictResolution.Exception:
                                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFilePath));
                        }
                    }

                    BlockCopyFile(file, destFilePath, progressCallback, cancelToken);
                    IGorgonVirtualFile destFile = FileSystem.GetFile(destFilePath);

                    if ((destFile != null) && (!cancelToken.IsCancellationRequested))
                    {
                        filesCopied.Add((file, destFile));
                    }
                }

                OnCopyComplete();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer);
            }
        }

        /// <summary>
        /// Function to export files from the virtual file system to a directory on the physical file system.
        /// </summary>
        /// <param name="filePaths">The path to the files to export.</param>
        /// <param name="destDirectoryPath">The destination directory path on the physical file system that will receive the exported data.</param>
        /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePaths"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when a file path in the <paramref name="filePaths"/> could not be located in the file system.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="destDirectoryPath"/> could not be located in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This method exports the specified files to a directory on the physical file system. 
        /// </para>
        /// <para>
        /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
        /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
        /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonCopyCallbackOptions"/>
        public void ExportFiles(IEnumerable<string> filePaths, string destDirectoryPath, GorgonCopyCallbackOptions options = null)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException(nameof(filePaths));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }

            if (!Directory.Exists(destDirectoryPath))
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destDirectoryPath));
            }

            var files = new List<IGorgonVirtualFile>();

            foreach (string filePath in filePaths)
            {
                IGorgonVirtualFile file = FileSystem.GetFile(filePath);

                if (file == null)
                {
                    throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, filePath));
                }

                files.Add(file);
            }

            if (files.Count == 0)
            {
                return;
            }

            try
            {
                _writeBuffer = ArrayPool<byte>.Shared.Rent(262144);
                var pathBuffer = new StringBuilder(1024);
                CancellationToken cancelToken = options?.CancelToken ?? CancellationToken.None;
                Action<string, double> progressCallback = options?.ProgressCallback;
                Func<string, string, FileConflictResolution> conflictCallback = options?.ConflictResolutionCallback;
                FileConflictResolution conflictRes = FileConflictResolution.Overwrite;

                progressCallback?.Invoke(files[0].FullPath, 0);

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                // Copy files for the current directory.
                foreach (IGorgonVirtualFile file in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    string fileName = file.Name;
                    string destFilePath = Path.Combine(destDirectoryPath, fileName);
                    bool fileExists = File.Exists(destFilePath);
                    bool directoryExists = Directory.Exists(destFilePath);

                    if ((conflictRes != FileConflictResolution.OverwriteAll) && (conflictCallback != null)
                        && ((fileExists) || (directoryExists)))
                    {
                        if ((directoryExists) || ((conflictRes != FileConflictResolution.RenameAll) && (conflictRes != FileConflictResolution.SkipAll)))
                        {
                            conflictRes = conflictCallback(file.FullPath, destFilePath);
                        }

                        switch (conflictRes)
                        {
                            case FileConflictResolution.Skip:
                            case FileConflictResolution.SkipAll:
                                continue;
                            case FileConflictResolution.Rename:
                            case FileConflictResolution.RenameAll:
                                fileName = GetNewName(destDirectoryPath, fileName);
                                destFilePath = Path.Combine(destDirectoryPath, fileName);
                                break;
                            case FileConflictResolution.Cancel:
                                return;
                            case FileConflictResolution.Exception:
                                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFilePath));
                        }
                    }

                    BlockCopyFileToPhysical(file, destFilePath, progressCallback, cancelToken);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer);
            }
        }

        /// <summary>
        /// Function to export a directory from the virtual file system to a directory on the physical file system.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to export.</param>
        /// <param name="destDirectoryPath">The destination directory path on the physical file system that will receive the exported data.</param>
        /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> could not be located in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This method exports the virtual directory, its sub directories and all files under those directories to a directory on the physical file system. 
        /// </para>
        /// <para>
        /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
        /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
        /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonCopyCallbackOptions"/>
        public void ExportDirectory(string directoryPath, string destDirectoryPath, GorgonCopyCallbackOptions options = null)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentEmptyException(nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }

            IGorgonVirtualDirectory srcDirectory = FileSystem.GetDirectory(directoryPath);            

            if (srcDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryPath));
            }

            if (!Directory.Exists(destDirectoryPath))
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destDirectoryPath));
            }

            try
            {
                _writeBuffer = ArrayPool<byte>.Shared.Rent(262144);
                var pathBuffer = new StringBuilder(1024);
                CancellationToken cancelToken = options?.CancelToken ?? CancellationToken.None;
                Action<string, double> progressCallback = options?.ProgressCallback;
                Func<string, string, FileConflictResolution> conflictCallback = options?.ConflictResolutionCallback;
                FileConflictResolution conflictRes = FileConflictResolution.Overwrite;

                progressCallback?.Invoke(srcDirectory.FullPath, 0);

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                var dirsToCopy = new List<IGorgonVirtualDirectory>
                {
                    srcDirectory
                };
                dirsToCopy.AddRange(srcDirectory.Directories.Traverse(d => d.Directories));
                var filesToCopy = dirsToCopy.SelectMany(f => f.Files)
                                            .GroupBy(f => f.Directory)
                                            .ToDictionary(d => d.Key, f => (IEnumerable<IGorgonVirtualFile>)f);

                for (int i = 0; i < dirsToCopy.Count; ++i)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    IGorgonVirtualDirectory srcDir = dirsToCopy[i];
                    string sourcePhysical = srcDir.FullPath.Substring((srcDirectory.Parent != null ? srcDirectory.Parent.FullPath : FileSystem.RootDirectory.FullPath).Length);
                    string destDirPath = destDirectoryPath + sourcePhysical.FormatDirectory(Path.DirectorySeparatorChar);

                    progressCallback?.Invoke(srcDir.FullPath, 0);

                    if (!Directory.Exists(destDirPath))
                    {
                        Directory.CreateDirectory(destDirPath);
                    }

                    if (!filesToCopy.TryGetValue(srcDir, out IEnumerable<IGorgonVirtualFile> files))
                    {
                        continue;
                    }

                    // Copy files for the current directory.
                    foreach (IGorgonVirtualFile file in files)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }

                        string fileName = file.Name;
                        string destFilePath = Path.Combine(destDirPath, fileName);

                        bool fileExists = File.Exists(destFilePath);
                        bool directoryExists = Directory.Exists(destFilePath);

                        if ((conflictRes != FileConflictResolution.OverwriteAll) && (conflictCallback != null)
                            && ((fileExists) || (directoryExists)))
                        {
                            if ((directoryExists) || ((conflictRes != FileConflictResolution.RenameAll) && (conflictRes != FileConflictResolution.SkipAll)))
                            {
                                conflictRes = conflictCallback(file.FullPath, destFilePath);
                            }

                            switch (conflictRes)
                            {
                                case FileConflictResolution.Skip:
                                case FileConflictResolution.SkipAll:
                                    continue;
                                case FileConflictResolution.Rename:
                                case FileConflictResolution.RenameAll:
                                    fileName = GetNewName(destDirPath, fileName);
                                    destFilePath = Path.Combine(destDirPath, fileName);
                                    break;
                                case FileConflictResolution.Cancel:                                    
                                    return;
                                case FileConflictResolution.Exception:
                                    throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFilePath));
                            }
                        }

                        BlockCopyFileToPhysical(file, destFilePath, progressCallback, cancelToken);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer);
            }
        }

        /// <summary>
        /// Function to import files/directories from the physical file system to a directory on the virtual file system.
        /// </summary>
        /// <param name="paths">The paths to the files/directories on the physical file system to import.</param>
        /// <param name="destDirectoryPath">The destination directory path in the virtual file system that will receive the imported data.</param>
        /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="paths"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="destDirectoryPath"/> could not be located in the file system.</exception>
        /// <remarks>
        /// <para>
        /// This method imports files, and directories from the physical file system into a directory on the virtual file system.
        /// </para>
        /// <para>
        /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
        /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
        /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonCopyCallbackOptions"/>
        public void Import(IReadOnlyList<string> paths, string destDirectoryPath, GorgonCopyCallbackOptions options = null)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }

            if (paths.Count == 0)
            {
                return;
            }

            IGorgonVirtualDirectory destDirectory = FileSystem.GetDirectory(destDirectoryPath);

            if (destDirectory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destDirectoryPath));
            }

            try
            {
                PrepareWriteArea();

                _writeBuffer = ArrayPool<byte>.Shared.Rent(262144);
                var pathBuffer = new StringBuilder(1024);
                var dirsCopied = new List<IGorgonVirtualDirectory>();
                var filesCopied = new List<IGorgonVirtualFile>();
                CancellationToken cancelToken = options?.CancelToken ?? CancellationToken.None;
                Action<string, double> progressCallback = options?.ProgressCallback;
                Func<string, string, FileConflictResolution> conflictCallback = options?.ConflictResolutionCallback;
                FileConflictResolution conflictRes = FileConflictResolution.Overwrite;

                // Function to fire the event when the copy operation completes.
                void OnCopyComplete()
                {
                    if ((dirsCopied.Count == 0) && (filesCopied.Count == 0))
                    {
                        return;
                    }

                    EventHandler<ImportedEventArgs> handler = Imported;
                    handler?.Invoke(this, new ImportedEventArgs(destDirectory, dirsCopied, filesCopied));
                }

                progressCallback?.Invoke(paths[0], 0);

                if (cancelToken.IsCancellationRequested)
                {
                    OnCopyComplete();
                    return;
                }

                var dirsToCopy = new List<DirectoryInfo>();
                var filesToCopy = new List<FileInfo>();

                foreach (string path in paths.OrderBy(p => p.Length))
                {
                    if ((!File.Exists(path)) && (!Directory.Exists(path)))
                    {
                        continue;
                    }

                    if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        dirsToCopy.Add(new DirectoryInfo(path));
                    }
                    else
                    {
                        filesToCopy.Add(new FileInfo(path));
                    }
                }

                if ((filesToCopy.Count == 0) && (dirsToCopy.Count == 0))
                {
                    OnCopyComplete();
                    return;
                }

                DirectoryInfo importParent = dirsToCopy.Count == 0 ? filesToCopy[0].Directory : dirsToCopy[0].Parent;
                var beforeArgs = new FileImportingArgs();

                // Copies the files from the import directory.
                bool CopyFiles(DirectoryInfo parent, IReadOnlyList<FileInfo> files, IGorgonVirtualDirectory destDir)
                {
                    // Copy files for the current directory.
                    foreach (FileInfo file in files)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        EventHandler<FileImportingArgs> beforeHandler = FileImporting;
                        beforeArgs.PhysicalFilePath = file.FullName;

                        if (beforeHandler != null)
                        {
                            beforeHandler(this, beforeArgs);
                            if (string.IsNullOrWhiteSpace(beforeArgs.PhysicalFilePath))
                            {
                                continue;
                            }

                            if (cancelToken.IsCancellationRequested)
                            {
                                return false;
                            }
                        }

                        string fileName = Path.GetFileName(beforeArgs.PhysicalFilePath);
                        string destFilePath = destDir.FullPath + fileName;
                        bool fileExists = destDir.Files.Contains(fileName);
                        bool directoryExists = destDir.Directories.Contains(fileName);

                        if ((conflictRes != FileConflictResolution.OverwriteAll) && (conflictCallback != null)
                            && ((fileExists) || (directoryExists)))
                        {
                            if ((directoryExists) || ((conflictRes != FileConflictResolution.RenameAll) && (conflictRes != FileConflictResolution.SkipAll)))
                            {
                                conflictRes = conflictCallback(beforeArgs.PhysicalFilePath, destFilePath);
                            }

                            switch (conflictRes)
                            {
                                case FileConflictResolution.Skip:
                                case FileConflictResolution.SkipAll:
                                    continue;
                                case FileConflictResolution.Rename:
                                case FileConflictResolution.RenameAll:
                                    fileName = GetNewName(destDir, fileName);
                                    destFilePath = destDir.FullPath + fileName;
                                    break;
                                case FileConflictResolution.Cancel:
                                    return false;
                                case FileConflictResolution.Exception:
                                    throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFilePath));
                            }
                        }

                        BlockCopyFileFromPhysical(beforeArgs.PhysicalFilePath, destFilePath, progressCallback, cancelToken);
                        IGorgonVirtualFile destFile = FileSystem.GetFile(destFilePath);

                        if ((destFile != null) && (!cancelToken.IsCancellationRequested))
                        {
                            EventHandler<FileImportedArgs> afterHandler = FileImported;
                            afterHandler?.Invoke(this, new FileImportedArgs(beforeArgs.PhysicalFilePath, destFile));
                            filesCopied.Add(destFile);
                        }
                    }

                    return !cancelToken.IsCancellationRequested;
                }

                if (filesToCopy.Count > 0)
                {
                    if (!CopyFiles(importParent, filesToCopy, destDirectory))
                    {
                        dirsToCopy.Clear();
                    }
                }

                if (dirsToCopy.Count == 0)
                {
                    OnCopyComplete();
                    return;
                }

                // Get sub directories (if any).
                int dirCount = dirsToCopy.Count;
                for (int i = 0; i < dirCount; ++i)
                {
                    dirsToCopy.AddRange(dirsToCopy[i].EnumerateDirectories("*", SearchOption.AllDirectories));
                }

                for (int i = 0; i < dirsToCopy.Count; ++i)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        OnCopyComplete();
                        return;
                    }

                    DirectoryInfo srcDir = dirsToCopy[i];
                    string destDirPath = destDirectory.FullPath + srcDir.FullName.Substring(importParent.FullName.Length + 1).FormatDirectory('/');

                    progressCallback?.Invoke(srcDir.FullName, 0);

                    // Create a new path for the copied files.
                    IGorgonVirtualDirectory destDir = FileSystem.GetDirectory(destDirPath);

                    if (destDir == null)
                    {
                        destDir = CreateDirectory(destDirPath);
                        dirsCopied.Add(destDir);
                    }

                    filesToCopy.Clear();
                    filesToCopy.AddRange(srcDir.GetFiles("*", SearchOption.TopDirectoryOnly));

                    if (filesToCopy.Count != 0)
                    {
                        if (!CopyFiles(srcDir, filesToCopy, destDir))
                        {
                            break;
                        }
                    }
                }

                OnCopyComplete();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer);
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
        /// <para>
        /// Calling this method will trigger the <see cref="VirtualFileOpened"/> event, and when the file stream is closed the <see cref="VirtualFileClosed"/> event.
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

            string directoryPath = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            directoryPath = string.IsNullOrWhiteSpace(directoryPath) ? "/" : directoryPath.FormatDirectory('/');

            IGorgonVirtualDirectory directory = FileSystem.GetDirectory(directoryPath);

            if (directory == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryPath));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_NO_FILENAME, path));
            }

            fileName = fileName.FormatFileName();

            IGorgonVirtualFile file = FileSystem.GetFile(path);

            // We're opening an existing file, so check it if we require the file to be present.
            if ((file == null) && ((mode == FileMode.Truncate) || (mode == FileMode.Open)))
            {
                throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
            }

            // Function called when the stream is closed.
            void OnStreamClose(FileStream stream)
            {
                // Get the new file information and pass it to the file system.
                IGorgonPhysicalFileInfo fileInfo = new PhysicalFileInfo(_mountPoint, stream.Name);
                IGorgonVirtualFile updatedFile = _notifier.NotifyFileWriteStreamClosed(_mountPoint, fileInfo);

                EventHandler<VirtualFileClosedEventArgs> closeHandler = VirtualFileClosed;
                closeHandler?.Invoke(this, new VirtualFileClosedEventArgs(updatedFile, file == null));
            }

            PrepareWriteArea();

            FileStream result = null;

            result = new FileSystemWriteStream(GetWriteFilePath(directory.FullPath, fileName), mode)
            {
                OnCloseCallback = OnStreamClose
            };

            EventHandler<VirtualFileOpenedEventArgs> handler = VirtualFileOpened;
            handler?.Invoke(this, new VirtualFileOpenedEventArgs(file, result));

            return result;
        }        
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileSystemWriter"/> class.
        /// </summary>
        /// <param name="fileSystem">A file system used to track the updates when writing.</param>
        /// <param name="notifier">The notifier used to tell the file system that an update has occurred.</param>
        /// <param name="writeLocation">The directory on the physical file system to actually write data into.</param>
        /// <param name="deleteAction">[Optional] A method that can be used to delete a file system item instead of defaulting to erasing the item.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, <paramref name="notifier"/> or the <paramref name="writeLocation"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="writeLocation"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// When the <paramref name="deleteAction"/> parameter is supplied, the operation that is used to delete the file system item (e.g. a directory, or a file) is handled by the callback method supplied. 
        /// This is useful in cases where actually removing the file is not desirable. For example, on Windows, files could be moved to the recycle bin, or a custom area for storing deleted files may be 
        /// used. This callback method allows the developer to decide on how to best handle removal of file system items.
        /// </para>
        /// <para>
        /// The method passed to the <paramref name="deleteAction"/> parameter receives the full path to the item on the physical file system as its parameter, and returns a bool to indicate success 
        /// (<b>true</b>) or failure (<b>false</b>). Please note that it is up to the developer to determine what type of item is being deleted.
        /// </para>
        /// <para>
        /// If the <paramref name="deleteAction"/> is left as <b>null</b>, then the file system item is erased from the file system.
        /// </para>
        /// </remarks>
        public GorgonFileSystemWriter(IGorgonFileSystem fileSystem, IGorgonFileSystemNotifier notifier, string writeLocation, Func<string, bool> deleteAction = null)
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

            _deleteAction = deleteAction;
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            WriteLocation = writeLocation.FormatDirectory(Path.DirectorySeparatorChar);
            _mountPoint = new GorgonFileSystemMountPoint(fileSystem.DefaultProvider, WriteLocation, "/");
        }
        #endregion
    }
}
