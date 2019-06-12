#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: September 7, 2018 12:35:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Gorgon.Core;
using Gorgon.Editor.Native;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to manage the project file system.
    /// </summary>
    internal class FileSystemService
        : IFileSystemService
    {
        #region Constants.
        // Flags used to recycle deleted files/directories.
        private const Shell32.FileOperationFlags RecycleFlags = Shell32.FileOperationFlags.FOF_SILENT | Shell32.FileOperationFlags.FOF_NOCONFIRMATION | Shell32.FileOperationFlags.FOF_WANTNUKEWARNING;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the root directory for the file system.
        /// </summary>
        /// <value>The root directory.</value>
        /// <remarks>All file system operations will take place under this directory.</remarks>
        public DirectoryInfo RootDirectory
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function used to clean up a directory if an operation is cancelled, or an error occurs.
        /// </summary>
        /// <param name="directory">The directory to clean up.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private void CleanupDirectory(DirectoryInfo directory)
        {
            bool tryMove = false;

            while (directory.Exists)
            {
                try
                {
                    // Sometimes, if we cannot delete the directory (because some outside force has locked it), we can move it and then delete it.
                    if (tryMove)
                    {
                        directory.MoveTo(Path.Combine(directory.Parent.FullName, Guid.NewGuid().ToString("N")));
                        directory.Refresh();
                    }

                    directory.Delete(true);
                }
                catch
                {
                    if (!tryMove)
                    {
                        return;
                    }
                    else
                    {
                        tryMove = true;
                    }
                }
            }
        }

        /// <summary>
        /// Function to check the root of a path and ensure it matches our root directory.
        /// </summary>
        /// <param name="directory">The directory to evaluate.</param>
        private void CheckRootOfPath(DirectoryInfo directory)
        {
            if (string.Equals(RootDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar), directory.FullName.FormatDirectory(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            DirectoryInfo parent = directory.Parent;

            // Walk up the tree and find our root.
            while (parent != null)
            {
                if (string.Equals(RootDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar), parent.FullName.FormatDirectory(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                parent = parent.Parent;
            }


            throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GOREDIT_ERR_PATH_IS_NOT_IN_PROJECT_FILESYSTEM, directory.FullName, RootDirectory.FullName));
        }

        /// <summary>
        /// Function to retrieve a list of sub directories under the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory that contains the sub directories.</param>
        /// <param name="recursive">[Optional] <b>true</b> to retrieve all directories nested in sub directories, or <b>false</b> to just retrieve all directories in the top level.</param>
        /// <returns>A list of directories in the directory.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path" /> parameter is empty.</exception>
        public IReadOnlyList<DirectoryInfo> GetDirectories(string path, bool recursive = true)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            var directory = new DirectoryInfo(path.FormatDirectory(Path.DirectorySeparatorChar));

            if (!directory.Exists)
            {
                return Array.Empty<DirectoryInfo>();
            }

            CheckRootOfPath(directory);

            var directories = directory.GetDirectories("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            var excluded = new List<DirectoryInfo>();

            foreach (DirectoryInfo item in directories)
            {
                if (((item.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                            || ((item.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
                {
                    excluded.Add(item);
                }

                // Check the parents to ensure none of them are hidden.
                DirectoryInfo parent = item.Parent;

                // An ancestor is hidden or a system directory, then exclude any children.
                while (!string.Equals(parent.FullName.FormatDirectory(Path.DirectorySeparatorChar), directory.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    if (((parent.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                                || ((parent.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
                    {
                        excluded.Add(item);
                    }

                    parent = parent.Parent;
                }
            }

            foreach (DirectoryInfo item in excluded)
            {
                directories.Remove(item);
            }

            return directories;
        }

        /// <summary>
        /// Function to retrieve a list of files under the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory that contains the files.</param>
        /// <param name="recursive">[Optional] <b>true</b> to retrieve all files nested in sub directories, or <b>false</b> to just retrieve all files in the top level.</param>
        /// <returns>A list of files in the directory.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path" /> parameter is empty.</exception>
        public IReadOnlyList<FileInfo> GetFiles(string path, bool recursive = true)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            var directory = new DirectoryInfo(path.FormatDirectory(Path.DirectorySeparatorChar));

            CheckRootOfPath(directory);

            return !directory.Exists
                ? (Array.Empty<FileInfo>())
                : directory.GetFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                            .Where(item => (item.Attributes & FileAttributes.Directory) != FileAttributes.Directory
                                        && (item.Attributes & FileAttributes.System) != FileAttributes.System
                                        && (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
                                        && (!string.Equals(item.Name, CommonEditorConstants.EditorMetadataFileName, StringComparison.OrdinalIgnoreCase)))
                            .ToArray();
        }

        /// <summary>
        /// Function to generate a file name for a destination directory, based on whether or not it already exists.
        /// </summary>
        /// <param name="path">The path to the desired file name.</param>
        /// <returns>The new file name, or the original file name if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        public string GenerateFileName(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            var file = new FileInfo(path);
            string fileDirectory = file.Directory.FullName;
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(file.Name);
            int count = 0;

            while (file.Exists)
            {
                string newPath = Path.Combine(fileDirectory, $"{fileNameNoExtension} ({++count})");

                if (!string.IsNullOrWhiteSpace(file.Extension))
                {
                    newPath += file.Extension;
                }

                file = new FileInfo(newPath);
            }

            return file.Name;
        }

        /// <summary>
        /// Function to generate a directory name for a destination directory, based on whether or not it already exists.
        /// </summary>
        /// <param name="path">The path to the desired directory name.</param>
        /// <returns>The new file name, or the original directory name if it did not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        public string GenerateDirectoryName(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            var directory = new DirectoryInfo(path);
            string directoryName = directory.Name;
            int count = 0;

            while ((directory.Exists) || ((directory.Attributes & FileAttributes.Directory) != FileAttributes.Directory))
            {
                directory = new DirectoryInfo(Path.Combine(directory.Parent.FullName, $"{directoryName} ({++count})"));
            }

            return directory.Name;
        }

        /// <summary>
        /// Function to determine if a directory exists or not.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns><b>true</b> if the directory exists, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="path"/> is not under the file system root.</exception>
        public bool DirectoryExists(DirectoryInfo path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            CheckRootOfPath(path);

            return path.Exists;
        }

        /// <summary>
        /// Function to determine if a file exists or not.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns><b>true</b> if the file exists, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="path"/> is not under the file system root.</exception>
        public bool FileExists(FileInfo path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            CheckRootOfPath(path.Directory);

            return path.Exists;
        }

        /// <summary>
        /// Function to create an empty 1-byte file.
        /// </summary>
        /// <param name="parentDir">The physical parent directory for the file.</param>
        /// <param name="name">The name of the file.</param>
        /// <returns>A file information obejct representing the file on the disk.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="parentDir"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public FileInfo CreateEmptyFile(DirectoryInfo parentDir, string name)
        {
            if (parentDir == null)
            {
                throw new ArgumentNullException(nameof(parentDir));
            }

            if (!parentDir.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            var result = new FileInfo(Path.Combine(parentDir.FullName, name));

            // Write a single byte to it so we know that we have the file.
            using (Stream stream = result.OpenWrite())
            {
                stream.WriteByte(0);
            }

            result.Refresh();

            return result;
        }

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory on the physical file system.</param>
        /// <param name="newDirName">The name of the new directory.</param>
        /// <returns>A new directory information object for the new directory.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentDirectory"/>, or the <paramref name="newDirName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newDirName"/> parameter is empty.</exception>
        public DirectoryInfo CreateDirectory(DirectoryInfo parentDirectory, string newDirName)
        {
            if (parentDirectory == null)
            {
                throw new ArgumentNullException(nameof(parentDirectory));
            }

            if (newDirName == null)
            {
                throw new ArgumentNullException(nameof(newDirName));
            }

            if (string.IsNullOrWhiteSpace(newDirName))
            {
                throw new ArgumentEmptyException(nameof(newDirName));
            }

            var newDir = new DirectoryInfo(Path.Combine(parentDirectory.FullName, newDirName));

            if (!newDir.Exists)
            {
                newDir.Create();
                newDir.Refresh();
            }

            return newDir;
        }

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory on the physical file system.</param>
        /// <returns>A new directory information object for the new directory.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentDirectory"/> parameter is <b>null</b>.</exception>
        public DirectoryInfo CreateDirectory(DirectoryInfo parentDirectory)
        {
            if (parentDirectory == null)
            {
                throw new ArgumentNullException(nameof(parentDirectory));
            }

            CheckRootOfPath(parentDirectory);

            int count = 0;
            string newDirName = Path.Combine(parentDirectory.FullName, Resources.GOREDIT_NEW_DIR_NAME);

            // Ensure the new directory name is avaialble.
            while (Directory.Exists(newDirName))
            {
                newDirName = $"{Path.Combine(parentDirectory.FullName, Resources.GOREDIT_NEW_DIR_NAME)} ({++count})";
            }

            var directory = new DirectoryInfo(newDirName);

            // Create the new directory.
            directory.Create();
            directory.Refresh();

            return directory;
        }

        /// <summary>
        /// Function to rename a directory.
        /// </summary>
        /// <param name="directoryPath">The physical file system path to the directory to rename.</param>
        /// <param name="newName">The new directory name.</param>
        /// <returns>The full physical file system path of the new directory name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public void RenameDirectory(DirectoryInfo directoryPath, string newName)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            CheckRootOfPath(directoryPath);

            // Since windows uses a case insensitive file system, we need to ensure we're not just changing the case of the name.
            // And if we are, we need to allow the rename to happen regardless.
            if ((string.Equals(directoryPath.Name, newName, StringComparison.CurrentCultureIgnoreCase))
                && (!string.Equals(directoryPath.Name, newName, StringComparison.CurrentCulture)))
            {
                directoryPath.MoveTo(Path.Combine(directoryPath.Parent.FullName, Guid.NewGuid().ToString("N")));
                directoryPath.Refresh();
            }

            directoryPath.MoveTo(Path.Combine(directoryPath.Parent.FullName, newName));
            directoryPath.Refresh();
        }

        /// <summary>
        /// Function to rename a file.
        /// </summary>
        /// <param name="filePath">The physical file system path to the file to rename.</param>
        /// <returns>The full physical file system path of the new file name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public void RenameFile(FileInfo filePath, string newName)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            CheckRootOfPath(filePath.Directory);

            // Since windows uses a case insensitive file system, we need to ensure we're not just changing the case of the name.
            // And if we are, we need to allow the rename to happen regardless.
            if ((string.Equals(filePath.Name, newName, StringComparison.CurrentCultureIgnoreCase))
                && (!string.Equals(filePath.Name, newName, StringComparison.CurrentCulture)))
            {
                filePath.MoveTo(Path.Combine(filePath.Directory.FullName, Guid.NewGuid().ToString("N")));
                filePath.Refresh();
            }

            filePath.MoveTo(Path.Combine(filePath.Directory.FullName, newName));
            filePath.Refresh();
        }

        /// <summary>
        /// Function to delete a file.
        /// </summary>
        /// <param name="filePath">The path to the file on the physical file system to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
        public void DeleteFile(FileInfo filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            CheckRootOfPath(filePath.Directory);

            // Send the file to the recycle bin.
            if (!Shell32.SendToRecycleBin(filePath.FullName, RecycleFlags))
            {
                throw new IOException(string.Format(Resources.GOREDIT_ERR_DELETE, filePath.Name));
            }
        }

        /// <summary>
        /// Function to delete a directory.
        /// </summary>
        /// <param name="directoryPath">The physical file system path to the directory to delete.</param>
        /// <param name="onDelete">The method to call when a directory or a child of the directory is deleted.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <returns><b>true</b> if the directory was deleted, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDelete"/> parameter sends a file system information object that contains the name of the item currently being deleted.
        /// </para>
        /// </remarks>
        public bool DeleteDirectory(DirectoryInfo directoryPath, Action<FileSystemInfo> onDelete, CancellationToken cancelToken)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            // If the directory is already gone, then we don't care.  
            if (!directoryPath.Exists)
            {
                return true;
            }

            CheckRootOfPath(directoryPath);

            IEnumerable<FileSystemInfo> subItems = directoryPath.GetFiles("*", SearchOption.AllDirectories)
                .Cast<FileSystemInfo>()
                .Concat(directoryPath.GetDirectories("*", SearchOption.AllDirectories)
                                     .OrderByDescending(item => item.FullName.Length));
            
            foreach (FileSystemInfo info in subItems)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }

                onDelete?.Invoke(info);

                if (!Shell32.SendToRecycleBin(info.FullName, RecycleFlags))
                {
                    return false;
                }                
            }

            if (cancelToken.IsCancellationRequested)
            {
                return false;
            }

            onDelete?.Invoke(directoryPath);

            if (!Shell32.SendToRecycleBin(directoryPath.FullName, RecycleFlags))
            {
                return false;
            }

            directoryPath.Refresh();
            return !directoryPath.Exists;
        }

        /// <summary>
        /// Function to export a file to a directory on the physical file system.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destPath">The destination directory path.</param>        
        /// <param name="onCopy">The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destPath"/> was not found.</exception>
        /// <exception cref="GorgonException">Thrown when the <paramref name="filePath"/> is not located under the file system root.</exception>
        public void ExportFile(FileInfo filePath, FileInfo destPath, Action<long, long> exportProgress, CancellationToken cancelToken, byte[] writeBuffer = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destPath == null)
            {
                throw new ArgumentNullException(nameof(destPath));
            }

            if (!filePath.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath.Name));
            }
            
            if (!destPath.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destPath.Directory.Name));
            }

            CheckRootOfPath(filePath.Directory);

            BlockCopyFile(filePath, destPath, exportProgress, writeBuffer, cancelToken);
        }

        /// <summary>
        /// Function to perform the copy of a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFile">The destination file.</param>
        /// <param name="progressCallback">The method that reports the file copy progress back.</param>        
        /// <param name="writeBuffer">The buffer used to write out the file in chunks.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// <para>
        /// This performs a block copy of a file from one location to another. These locations can be anywhere on the physical file system(s) and are not checked.  
        /// </para>
        /// </remarks>
        private void BlockCopyFile(FileInfo filePath, FileInfo destFile, Action<long, long> progressCallback, byte[] writeBuffer, CancellationToken cancelToken)
        {
            long fileSize = filePath.Length;
            long maxBlockSize;
            long copied = 0;

            if ((writeBuffer == null) || (writeBuffer.Length == 0))
            {
                maxBlockSize = 81920L.Min(fileSize);
                writeBuffer = new byte[maxBlockSize];
            }
            else
            {
                maxBlockSize = writeBuffer.Length;
            }

            progressCallback(0, fileSize);

            using (Stream inStream = filePath.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (Stream outStream = destFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // If we're under 4096 BYTES, then we can copy as-is, and we can report a 1:1 file copy.
                    if (fileSize <= maxBlockSize)
                    {
                        inStream.CopyToStream(outStream, (int)fileSize, writeBuffer);
                        progressCallback(fileSize, fileSize);
                        return;
                    }

                    // Otherwise, we need to break up the file into chunks to get reporting of file copy progress.
                    int blockSize = (int)(maxBlockSize.Min(fileSize));

                    while (fileSize > 0)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }

                        inStream.CopyToStream(outStream, blockSize, writeBuffer);
                        fileSize -= blockSize;
                        copied += blockSize;
                        blockSize = (int)(maxBlockSize.Min(fileSize));

                        progressCallback(copied, filePath.Length);
                    }
                }
            }
        }

        /// <summary>Function to copy a file to another location.</summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFile">The destination file.</param>
        /// <param name="progressCallback">The method that reports the file copy progress back.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <param name="writeBuffer">[Optional] The buffer used to write out the file in chunks.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath" />, <paramref name="destFile" />, or the <paramref name="progressCallback" /> method is <b>null</b>.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath" /> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destFile" /> was not found.</exception>
        /// <exception cref="GorgonException">Thrown when the <paramref name="filePath" />, <paramref name="destFile" />, or the <paramref name="progressCallback" /> method is <b>null</b>.</exception>
        public void CopyFile(FileInfo filePath, FileInfo destFile, Action<long, long> progressCallback, CancellationToken cancelToken, byte[] writeBuffer = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destFile == null)
            {
                throw new ArgumentNullException(nameof(destFile));
            }

            if (progressCallback == null)
            {
                throw new ArgumentNullException(nameof(progressCallback));
            }

            CheckRootOfPath(filePath.Directory);
            CheckRootOfPath(destFile.Directory);

            if (!filePath.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath.Name));
            }

            if (!destFile.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destFile.Directory.Name));
            }

            BlockCopyFile(filePath, destFile, progressCallback, writeBuffer, cancelToken);
        }

        /// <summary>
        /// Function to move a directory to another location.
        /// </summary>
        /// <param name="directoryPath">The path to the directory.</param>
        /// <param name="destDirectoryPath">The destination name and path.</param>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="destDirectoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="destDirectoryPath"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="directoryPath"/>, or the parent directory for <paramref name="destDirectoryPath"/> was not found.</exception>
        public void MoveDirectory(DirectoryInfo directoryPath, string destDirectoryPath)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (destDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                throw new ArgumentEmptyException(nameof(destDirectoryPath));
            }

            var dest = new DirectoryInfo(destDirectoryPath);

            CheckRootOfPath(directoryPath);
            CheckRootOfPath(dest);

            if (!directoryPath.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, directoryPath.Name));
            }

            if (!dest.Parent.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, dest.Parent.Name));
            }

            // If we're moving to the same place, then we cannot proceed.
            if (string.Equals(directoryPath.FullName, dest.FullName, StringComparison.OrdinalIgnoreCase))
            {
                if ((dest.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    throw new IOException(Resources.GOREDIT_ERR_CANNOT_MOVE_DUPE_DIR);
                }
                else
                {
                    throw new IOException(Resources.GOREDIT_ERR_CANNOT_MOVE_FILE_DIR);
                }
            }

            try
            {
                directoryPath.MoveTo(dest.FullName);
                directoryPath.Refresh();
            }
            catch
            {
                if (dest.Exists)
                {
                    CleanupDirectory(dest);
                }
                
                throw;
            }
        }


        /// <summary>
        /// Function to move a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFile">The destination file.</param>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destFileNamePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destFileNamePath"/> was not found.</exception>
        public void MoveFile(FileInfo filePath, FileInfo destFile)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destFile == null)
            {
                throw new ArgumentNullException(nameof(destFile));
            }

            CheckRootOfPath(filePath.Directory);
            CheckRootOfPath(destFile.Directory);

            if (!filePath.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath.Name));
            }

            if (!destFile.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destFile.Name));
            }

            // If we're moving to the same place, then we don't need to do anything.
            if (string.Equals(filePath.FullName, destFile.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // For some incredibly stupid reason, we have to delete an existing file (why can't we overwrite??)
            if (destFile.Exists)
            {
                destFile.Delete();
            }

            filePath.MoveTo(destFile.FullName);
            filePath.Refresh();
        }

        /// <summary>
        /// Function to delete all files and directories in the file system.
        /// </summary>
        public void DeleteAll()
        {
            FileInfo[] files = RootDirectory.GetFiles("*", SearchOption.AllDirectories)
                                            .Where(item => ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                                                        && ((item.Attributes & FileAttributes.System) != FileAttributes.System)
                                                        && (!string.Equals(item.Name, CommonEditorConstants.EditorMetadataFileName, StringComparison.OrdinalIgnoreCase)))
                                            .ToArray();

            DirectoryInfo[] subDirs = RootDirectory.GetDirectories("*", SearchOption.AllDirectories)
                                                   .Where(item => (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                                                   .OrderByDescending(item => item.FullName.Length)
                                                   .ToArray();

            foreach (FileInfo file in files)
            {
                Shell32.SendToRecycleBin(file.FullName, RecycleFlags);
            }

            foreach (DirectoryInfo dir in subDirs)
            {
                if (dir.Exists)
                {
                    Shell32.SendToRecycleBin(dir.FullName, RecycleFlags);
                }                
            }
        }

        /// <summary>
        /// Function to import a file from the external physical file system.
        /// </summary>
        /// <param name="file">The file being imported.</param>
        /// <param name="dest">The destination for the imported file.</param>
        /// <param name="importProgress">The method used to report the progress of the import.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <param name="writeBuffer">[Optional] The buffer used to copy the file in blocks.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/>, <paramref name="dest"/>, or the <paramref name="importProgress"/> parameter is <b>null</b></exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory in the <paramref name="dest"/> does not exist.</exception>
        /// <exception cref="GorgonException">Thrown when the directory in the <paramref name="dest"/> parameter is not under the file system root.</exception>
        public void ImportFile(FileInfo file, FileInfo dest, Action<long, long> importProgress, CancellationToken cancelToken, byte[] writeBuffer = null)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            if (importProgress == null)
            {
                throw new ArgumentNullException(nameof(importProgress));
            }

            CheckRootOfPath(dest.Directory);

            if (!file.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, file.Name));
            }

            if (!dest.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, dest.Directory.Name));
            }                        

            BlockCopyFile(file, dest, importProgress, writeBuffer, cancelToken);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemService"/> class.
        /// </summary>
        /// <param name="rootDirectory">The root directory for the file system.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="rootDirectory"/> parameter is <b>null</b>.</exception>
        public FileSystemService(DirectoryInfo rootDirectory) => RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        #endregion
    }
}
