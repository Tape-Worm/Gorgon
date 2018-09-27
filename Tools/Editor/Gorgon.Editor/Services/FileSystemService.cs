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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
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
        /// Function to check the root of a path and ensure it matches our root directory.
        /// </summary>
        /// <param name="directory">The directory to evaluate.</param>
        private void CheckRootOfPath(DirectoryInfo directory)
        {
            if (string.Equals(RootDirectory.FullName, directory.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            DirectoryInfo parent = directory.Parent;
            
            // Walk up the tree and find our root.
            while (parent != null)
            {
                if (string.Equals(RootDirectory.FullName, parent.FullName, StringComparison.OrdinalIgnoreCase))
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

            CheckRootOfPath(directory);

            return !directory.Exists
                ? (new DirectoryInfo[0])
                : directory.GetDirectories("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
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
                ? (new FileInfo[0])
                : directory.GetFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
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
                file = new FileInfo(Path.ChangeExtension(Path.Combine(fileDirectory, $"{fileNameNoExtension} ({++count})"), file.Extension));
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

            while (directory.Exists)
            {
                directory = new DirectoryInfo(Path.Combine(directory.Parent.FullName, $"{directoryName} ({++count})"));
            }

            return directory.Name;
        }


        /// <summary>
        /// Function to determine if a file exists or not.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns><b>true</b> if the file exists, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        public bool FileExists(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            CheckRootOfPath(new DirectoryInfo(path));

            return File.Exists(path);
        }

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory on the physical file system.</param>
        /// <returns>A new directory information object for the new directory.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentDirectory"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="parentDirectory"/> parameter is empty.</exception>
        public DirectoryInfo CreateDirectory(string parentDirectory)
        {
            if (parentDirectory == null)
            {
                throw new ArgumentNullException(nameof(parentDirectory));
            }

            if (string.IsNullOrWhiteSpace(parentDirectory))
            {
                throw new ArgumentEmptyException(nameof(parentDirectory));
            }

            var parent = new DirectoryInfo(parentDirectory);
            CheckRootOfPath(parent);
            
            int count = 0;
            string newDirName = Path.Combine(parent.FullName, Resources.GOREDIT_NEW_DIR_NAME);
                        
            // Ensure the new directory name is avaialble.
            while (Directory.Exists(newDirName))
            {
                newDirName = $"{Path.Combine(parentDirectory, Resources.GOREDIT_NEW_DIR_NAME)} ({++count})";                
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
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/>, or the <paramref name="newName"/> parameter is empty.</exception>
        public string RenameDirectory(string directoryPath, string newName)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentEmptyException(nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            var directory = new DirectoryInfo(directoryPath);
            CheckRootOfPath(directory);

            directory.MoveTo(Path.Combine(directory.Parent.FullName, newName));

            return directory.FullName;
        }

        /// <summary>
        /// Function to rename a file.
        /// </summary>
        /// <param name="filePath">The physical file system path to the file to rename.</param>
        /// <param name="newName">The new name of the file.</param>
        /// <returns>The full physical file system path of the new file name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/>, or the <paramref name="newName"/> parameter is empty.</exception>
        public string RenameFile(string filePath, string newName)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            var file = new FileInfo(filePath);
            CheckRootOfPath(file.Directory);

            file.MoveTo(Path.Combine(file.Directory.FullName, newName));

            return file.FullName;
        }

        /// <summary>
        /// Function to delete a file.
        /// </summary>
        /// <param name="filePath">The path to the file on the physical file system to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        public void DeleteFile(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            var file = new FileInfo(filePath);
            CheckRootOfPath(file.Directory);

            // If the directory is already gone, then we don't care.  
            if (!file.Exists)
            {
                return;
            }

            file.Delete();
        }

        /// <summary>
        /// Function to delete a directory.
        /// </summary>
        /// <param name="directoryPath">The physical file system path to the directory to delete.</param>
        /// <param name="onDelete">The method to call when a directory or a child of the directory is deleted.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <returns><b>true</b> if the directory was deleted, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDelete"/> parameter sends a file system information object that contains the name of the item currently being deleted.
        /// </para>
        /// </remarks>
        public bool DeleteDirectory(string directoryPath, Action<FileSystemInfo> onDelete, CancellationToken cancelToken)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentEmptyException(nameof(directoryPath));
            }

            var directory = new DirectoryInfo(directoryPath);
            CheckRootOfPath(directory);

            IEnumerable<FileSystemInfo> subItems = directory.GetFiles("*", SearchOption.AllDirectories)
                .Cast<FileSystemInfo>()
                .Concat(directory.GetDirectories("*", SearchOption.AllDirectories).OrderByDescending(item => item.FullName.Length));

            // If the directory is already gone, then we don't care.  
            if (!directory.Exists)
            {
                return true;
            }

            foreach (FileSystemInfo info in subItems)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }

                onDelete?.Invoke(info);

                info.Delete();
            }

            if (cancelToken.IsCancellationRequested)
            {
                return false;
            }

            onDelete?.Invoke(directory);
            directory.Delete(true);

            return true;
        }

        /// <summary>
        /// Function to copy a directory, and all of its child items to the specified path.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to copy.</param>
        /// <param name="destDirectoryPath">The path to the destination directory for the copy.</param>
        /// <param name="onCopy">The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <param name="conflictResolver">[Optional] A callback method used to resolve a file copy conflict.</param>
        /// <returns><b>true</b> if the copy was successful, <b>false</b> if it was canceled.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath" />, or the <paramref name="destDirectoryPath" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath" />, or the <paramref name="destDirectoryPath" /> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="directoryPath" /> or <paramref name="destDirectoryPath" /> was not found.</exception>
        /// <remarks>
        /// <para>
        /// THe <paramref name="onCopy"/> callback method sends the file system item being copied, the destination file system item, the current item #, and the total number of items to copy.
        /// </para>
        /// </remarks>
        public async Task<bool> CopyDirectoryAsync(string directoryPath, string destDirectoryPath, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy, CancellationToken cancelToken, Func<string, string, FileSystemConflictResolution> conflictResolver = null)
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

            var sourceDir = new DirectoryInfo(directoryPath.FormatDirectory(Path.DirectorySeparatorChar));
            var destDir = new DirectoryInfo(destDirectoryPath.FormatDirectory(Path.DirectorySeparatorChar));

            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, sourceDir.FullName));
            }

            if (!destDir.Parent.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destDir.Parent.FullName));
            }

            var directories = new List<DirectoryInfo>
            {
                sourceDir
            };
            directories.AddRange(GetDirectories(sourceDir.FullName));

            // For progress, we'll need to get the total number of files.
            int totalItemCount = directories.Concat<FileSystemInfo>(GetFiles(sourceDir.FullName)).Count();
            int items = 0;

            if ((cancelToken.IsCancellationRequested) || (totalItemCount < 1))
            {
                return false;
            }

            FileSystemConflictResolution resolution = FileSystemConflictResolution.Exception;

            foreach (DirectoryInfo directory in directories.OrderBy(item => item.FullName.Length))
            {
                IReadOnlyList<FileInfo> files = GetFiles(directory.FullName, false);
                string newDirPath = directory.FullName.Replace(sourceDir.FullName.FormatDirectory(Path.DirectorySeparatorChar), destDir.FullName).FormatDirectory(Path.DirectorySeparatorChar);

                var subDir = new DirectoryInfo(newDirPath);

                onCopy?.Invoke(directory, subDir, items, totalItemCount);

                if (!subDir.Exists)
                {
                    subDir.Create();
                    subDir.Refresh();
                }

                ++items;

                // Give the user a fighting chance to cancel the operation.
                await Task.Delay(16);

                foreach (FileInfo file in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return false;
                    }
                                        
                    string newPath = Path.Combine(subDir.FullName, file.Name);

                    var newFile = new FileInfo(newPath);

                    // If the file exists, then we need to resolve this conflict.
                    if (newFile.Exists)
                    {
                        if ((resolution != FileSystemConflictResolution.OverwriteAll) && (resolution != FileSystemConflictResolution.RenameAll))
                        {
                            resolution = conflictResolver?.Invoke(file.ToFileSystemPath(RootDirectory), newFile.ToFileSystemPath(RootDirectory)) ?? FileSystemConflictResolution.Exception;
                        }

                        switch (resolution)
                        {
                            case FileSystemConflictResolution.Overwrite:
                            case FileSystemConflictResolution.OverwriteAll:
                                break;
                            case FileSystemConflictResolution.Rename:
                            case FileSystemConflictResolution.RenameAll:
                                newFile = new FileInfo(GenerateFileName(newFile.FullName));
                                break;
                            case FileSystemConflictResolution.Cancel:
                                return false;
                            default:
                                throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GOREDIT_ERR_CANNOT_COPY, file.FullName));
                        }
                    }

                    onCopy?.Invoke(file, newFile, items, totalItemCount);

                    int startTick = Environment.TickCount;
                    await Task.Run(() =>
                    {
                        file.CopyTo(newFile.FullName, true);
                        newFile.Refresh();
                    }, cancelToken);

                    ++items;

                    int diff = (Environment.TickCount - startTick).Max(0);

                    // Give the user a fighting chance to cancel the operation.
                    if (diff < 16)
                    {
                        await Task.Delay(16 - diff);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Function to copy a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFileNamePath">The destination file name and path.</param>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destFileNamePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destFileNamePath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destFileNamePath"/> was not found.</exception>
        public void CopyFile(string filePath, string destFileNamePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destFileNamePath == null)
            {
                throw new ArgumentNullException(nameof(destFileNamePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(destFileNamePath))
            {
                throw new ArgumentEmptyException(nameof(destFileNamePath));
            }

            var sourceFile = new FileInfo(filePath);
            var destFile = new FileInfo(destFileNamePath);

            CheckRootOfPath(sourceFile.Directory);
            CheckRootOfPath(destFile.Directory);

            if (!sourceFile.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath));
            }

            if (!destFile.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destFileNamePath));
            }

            sourceFile.CopyTo(destFile.FullName, true);
        }

        /// <summary>
        /// Function to move a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFileNamePath">The destination file name and path.</param>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destFileNamePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destFileNamePath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destFileNamePath"/> was not found.</exception>
        public void MoveFile(string filePath, string destFileNamePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destFileNamePath == null)
            {
                throw new ArgumentNullException(nameof(destFileNamePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(destFileNamePath))
            {
                throw new ArgumentEmptyException(nameof(destFileNamePath));
            }

            var sourceFile = new FileInfo(filePath);
            var destFile = new FileInfo(destFileNamePath);

            CheckRootOfPath(sourceFile.Directory);
            CheckRootOfPath(destFile.Directory);

            if (!sourceFile.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath));
            }

            if (!destFile.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destFileNamePath));
            }

            // If we're moving to the same place, then we don't need to do anything.
            if (string.Equals(sourceFile.FullName, destFile.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            
            // For some incredibly stupid reason, we have to delete an existing file (why can't we overwrite??)
            if (destFile.Exists)
            {
                destFile.Delete();
            }

            sourceFile.MoveTo(destFile.FullName);
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
