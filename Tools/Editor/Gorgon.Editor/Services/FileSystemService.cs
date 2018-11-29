﻿#region MIT
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
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Native;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to manage the project file system.
    /// </summary>
    internal class FileSystemService
        : IFileSystemService
    {
        #region Classes.
        /// <summary>
        /// Arguments to pass the copy task.
        /// </summary>
        private class CopyTaskArgs
        {
            /// <summary>
            /// Property to return the directory to use as the root of the copy operation.
            /// </summary>
            public DirectoryInfo RootDirectory
            {
                get;
            }

            /// <summary>
            /// Property to set or return the destination for the copy operation.
            /// </summary>
            public DirectoryInfo Destination
            {
                get;
                set;
            }

            /// <summary>
            /// Property to return the total number of items to copy.
            /// </summary>
            public int TotalItemCount
            {
                get;
            }

            /// <summary>
            /// Property to set or return the current item count.
            /// </summary>
            public int CurrentCount
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the previously selected conflict resolution option.
            /// </summary>
            public FileSystemConflictResolution ConflictResolution
            {
                get;
                set;
            } = FileSystemConflictResolution.Exception;

            /// <summary>
            /// Property to return the action to execute to report progress for the copy.
            /// </summary>
            public Action<FileSystemInfo, FileSystemInfo, int, int> OnCopyProgress
            {
                get;
            }

            /// <summary>
            /// Property to return the function to execute to handle a file conflict.
            /// </summary>
            public Func<FileSystemInfo, FileSystemInfo, FileSystemConflictResolution> OnResolveConflict
            {
                get;
            }

            /// <summary>
            /// Property to set or return a list of files to copy.
            /// </summary>
            public IReadOnlyList<FileInfo> Files
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return a list of directories to copy.
            /// </summary>
            public IReadOnlyList<DirectoryInfo> Directories
            {
                get;
                set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CopyTaskArgs"/> class.
            /// </summary>
            /// <param name="importArguments">Arguments from an import process.</param>
            /// <param name="totalItemCount">The number of items to copy.</param>
            public CopyTaskArgs(ImportArgs importArguments, int totalItemCount)
            {
                TotalItemCount = totalItemCount;
                RootDirectory = importArguments.ImportRoot;
                Destination = importArguments.DestinationDirectory;
                OnCopyProgress = importArguments.OnImportFile;
                OnResolveConflict = importArguments.ConflictResolver;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CopyTaskArgs"/> class.
            /// </summary>
            /// <param name="root">The root for the source directory to copy.</param>
            /// <param name="destDir">The dest directory to copy into.</param>
            /// <param name="copyArguments">The copy arguments.</param>
            /// <param name="totalItemCount">The total item count.</param>
            public CopyTaskArgs(DirectoryInfo root, DirectoryInfo destDir, CopyDirectoryArgs copyArguments, int totalItemCount)
            {
                TotalItemCount = totalItemCount;
                RootDirectory = root;
                Destination = destDir;
                OnCopyProgress = copyArguments.OnCopyProgress;
                OnResolveConflict = copyArguments.OnResolveConflict;
            }
        }
        #endregion

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
        /// Function to asynchronously copy files to a destination
        /// </summary>
        /// <param name="args">The settings for the file copy operation.</param>
        /// <param name="destination">The destination for the file/directory data.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <returns><b>true</b> if the copy was successful, <b>false</b> if it was canceled.</returns>
        private async Task<bool> CopyFilesAsync(CopyTaskArgs args, DirectoryInfo destination, CancellationToken cancelToken)
        {
            foreach (FileInfo file in args.Files.Where(item => ((item.Attributes & FileAttributes.System) != FileAttributes.System)
                                                        && ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)))
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }

                string newPath = Path.Combine(destination.FullName, file.Name);

                var newFile = new FileInfo(newPath);

                // If the file exists, then we need to resolve this conflict.
                if (newFile.Exists)
                {
                    if ((newFile.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        // Automatically rename if we have the same path as a directory. We cannot overwrite a directory with a file.
                        args.ConflictResolution = FileSystemConflictResolution.Rename;
                    }
                    else
                    {
                        if ((args.ConflictResolution != FileSystemConflictResolution.OverwriteAll) && (args.ConflictResolution != FileSystemConflictResolution.RenameAll))
                        {
                            args.ConflictResolution = args.OnResolveConflict?.Invoke(file, newFile.Directory) ?? FileSystemConflictResolution.Exception;
                        }
                    }

                    switch (args.ConflictResolution)
                    {
                        case FileSystemConflictResolution.Overwrite:
                        case FileSystemConflictResolution.OverwriteAll:
                            break;
                        case FileSystemConflictResolution.Rename:
                        case FileSystemConflictResolution.RenameAll:
                            newFile = new FileInfo(Path.Combine(newFile.DirectoryName, GenerateFileName(newFile.FullName)));
                            break;
                        case FileSystemConflictResolution.Cancel:
                            return false;
                        default:
                            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GOREDIT_ERR_CANNOT_COPY_DUPE, file.FullName));
                    }
                }

                args.OnCopyProgress?.Invoke(file, newFile, args.CurrentCount, args.TotalItemCount);

                await Task.Run(() =>
                {
                    // This file already exists
                    if (newFile.Exists)
                    {
                        // Just touch the file in this case, to make it appear as though we've overwritten it.
                        newFile.LastWriteTime = DateTime.Now;
                        newFile.Refresh();
                        return;
                    }

                    file.CopyTo(newFile.FullName, true);
                    newFile.Refresh();
                }, cancelToken);

                args.OnCopyProgress?.Invoke(file, newFile, ++args.CurrentCount, args.TotalItemCount);
            }

            return true;
        }

        /// <summary>
        /// Function to create the task used to copy file/directory data from one place to another.
        /// </summary>
        /// <param name="args">The arguments used to copy.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <returns><b>true</b> if the items were copied, <b>false</b> if not.</returns>
        private async Task<bool> CreateCopyTask(CopyTaskArgs args, CancellationToken cancelToken)
        {
            if ((cancelToken.IsCancellationRequested)
                || (args.Directories.Count == 0))
            {
                return false;
            }

            bool result = false;

            foreach (DirectoryInfo directory in args.Directories.OrderBy(item => item.FullName.Length))
            {
                args.Files = directory.GetFiles("*", SearchOption.TopDirectoryOnly)
                                      .Where(item => (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
                                                  && (item.Attributes & FileAttributes.System) != FileAttributes.System)
                                      .ToArray();
                DirectoryInfo subDir;

                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }

                // If we're copying the root of the file system, do not replicate the directory.
                if (!string.Equals(RootDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar), directory.FullName.FormatDirectory(Path.DirectorySeparatorChar)))                    
                {
                    string newDirPath;

                    // If the source is rooted in the file system, then remap to the new path.
                    newDirPath = RemapDirectory(directory, args.Destination, args.RootDirectory);
                    subDir = new DirectoryInfo(newDirPath);

                    args.OnCopyProgress?.Invoke(directory, subDir, args.CurrentCount, args.TotalItemCount);

                    if (!subDir.Exists)
                    {
                        subDir.Create();
                        subDir.Refresh();
                    }
                    else
                    {
                        // If there's a file in our way, then rename our directory.
                        while ((subDir.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                        {
                            subDir = new DirectoryInfo(Path.Combine(subDir.Parent.FullName, GenerateDirectoryName(subDir.FullName)));

                            if (!subDir.Exists)
                            {
                                subDir.Create();
                                subDir.Refresh();
                            }
                        }
                    }
                }
                else
                {
                    subDir = args.Destination;
                }

                ++args.CurrentCount;

                // Give the user a fighting chance to cancel the operation.
                await Task.Delay(16);

                result = await CopyFilesAsync(args, subDir, cancelToken);
            }

            // Wait for 500 milliseconds at the end so the animation for the progress bar can actually finish (otherwise the bar never completes).
            await Task.Delay(500);

            return result;
        }

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
                    // Sometimes, if we cannot move the directory (because some outside force has locked it), we can move it and then delete it.
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
        /// Function to copy a directory, and all of its child items to the specified path.
        /// </summary>
        /// <param name="args">The arguments used for copying.</param>
        /// <param name="destDir">The path to the destination directory for the copy.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <param name="includeSourceDirectory"><b>true</b> to include the source directory in the copy, or <b>false</b> to only use the contents of the source directory only.</param>
        /// <returns>The directory object for the copied directory, or <b>null</b> if nothing was copied (e.g. the operation was cancelled).</returns>
        private async Task<DirectoryInfo> CreateDirectoryCopyTask(CopyDirectoryArgs args, CancellationToken cancelToken, bool includeSourceDirectory)
        {
            var directories = new List<DirectoryInfo>();
            {
                directories.Add(args.SourceDirectory);
            }

            directories.AddRange(GetDirectories(args.SourceDirectory.FullName));

            // For progress, we'll need to get the total number of files.
            int totalItemCount = directories.Concat<FileSystemInfo>(GetFiles(args.SourceDirectory.FullName)).Count();

            if ((cancelToken.IsCancellationRequested) || (totalItemCount < 1))
            {
                return null;
            }

            DirectoryInfo source = args.SourceDirectory;

            if (!string.Equals(source.FullName.FormatDirectory(Path.DirectorySeparatorChar), 
                                    RootDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            {
                source = source.Parent;
            }

            var copyArgs = new CopyTaskArgs(source, args.DestinationDirectory, args, totalItemCount)
            {                
                Directories = directories
            };

            var destDir = new DirectoryInfo(Path.Combine(args.DestinationDirectory.FullName, args.SourceDirectory.Name));

            try
            {
                bool result = await CreateCopyTask(copyArgs, cancelToken);

                destDir.Refresh();

                if (!result)
                {
                    if (destDir.Exists)
                    {
                        await Task.Run(() => CleanupDirectory(destDir));
                    }
                    return null;
                }
            }
            catch
            {
                if (destDir.Exists)
                {
                    await Task.Run(() => CleanupDirectory(destDir));
                }
                throw;
            }

            return destDir;
        }

        /// <summary>
        /// Function to copy a directory, and all of its child items to the specified path.
        /// </summary>
        /// <param name="importSettings">The importer settings.</param>
        /// <param name="sourceDirs">The source directories to import.</param>
        /// <param name="sourceFiles">The source files to import.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <returns><b>true</b> if the copy was successful, <b>false</b> if it was canceled.</returns>
        private async Task<bool> CreateImportCopyTask(ImportArgs importSettings, IReadOnlyList<DirectoryInfo> sourceDirs, IReadOnlyList<FileInfo> sourceFiles, CancellationToken cancelToken)
        {
            var directories = new List<DirectoryInfo>();
            int totalItemCount = sourceDirs.Count + sourceFiles.Count;

            foreach (DirectoryInfo subDir in sourceDirs)
            {
                directories.Add(subDir);

                IReadOnlyList<DirectoryInfo> childDirs = subDir.GetDirectories("*", SearchOption.AllDirectories)
                                                        .Where(item => ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                                                                    && ((item.Attributes & FileAttributes.System) != FileAttributes.System))
                                                        .ToArray();
                directories.AddRange(childDirs);

                // For progress, we'll need to get the total number of files.
                totalItemCount += childDirs.Count + subDir.GetFiles("*", SearchOption.AllDirectories)
                                                        .Where(item => ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                                                                    && ((item.Attributes & FileAttributes.System) != FileAttributes.System))
                                                        .Count();
            }

            // Copy all sub-directories and files.
            var directoryCopyArgs = new CopyTaskArgs(importSettings, totalItemCount)
            {
                Directories = directories                
            };

            var fileCopyArgs = new CopyTaskArgs(importSettings, totalItemCount)
            {
                Files = sourceFiles
            };

            bool result = await CreateCopyTask(directoryCopyArgs, cancelToken);

            if (!result)
            {
                return false;
            }

            fileCopyArgs.CurrentCount = directoryCopyArgs.CurrentCount;
            fileCopyArgs.ConflictResolution = directoryCopyArgs.ConflictResolution;

            result = await CopyFilesAsync(fileCopyArgs, importSettings.DestinationDirectory, cancelToken);

            return result;
        }

        /// <summary>
        /// Function to remap a directory to the a new destination directory path.
        /// </summary>
        /// <param name="directory">The source directory to remap.</param>
        /// <param name="newDirectory">The base directory used to replace.</param>
        /// <param name="root">The common root directory for the source.</param>
        /// <returns>The remapped path.</returns>
        private string RemapDirectory(DirectoryInfo directory, DirectoryInfo newDirectory, DirectoryInfo root)
        {
            string dirPath = directory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
            string parentPath = root.FullName.FormatDirectory(Path.DirectorySeparatorChar);
            string newPath = newDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            return string.Equals(dirPath, newPath, StringComparison.OrdinalIgnoreCase)
                ? directory.FullName
                : Path.Combine(newPath, dirPath.Substring(parentPath.Length)).FormatDirectory(Path.DirectorySeparatorChar);
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
                return new DirectoryInfo[0];
            }

            CheckRootOfPath(directory);

            var directories = directory.GetDirectories("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            var excluded = new List<DirectoryInfo>();

            foreach (DirectoryInfo item in directories)
            {
                if (((item.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                            || ((item.Attributes & FileAttributes.System) == FileAttributes.System)
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
                                || ((parent.Attributes & FileAttributes.System) == FileAttributes.System)
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
                ? (new FileInfo[0])
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
                    newPath = Path.ChangeExtension(newPath, file.Extension);
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
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        public bool DirectoryExists(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            var dir = new DirectoryInfo(path);
            CheckRootOfPath(dir);

            return dir.Exists;
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

            var file = new FileInfo(path);
            CheckRootOfPath(file.Directory);

            return file.Exists;
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
                throw new IOException(string.Format(Resources.GOREDIT_ERR_DELETE, filePath));
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
        /// Function to copy a directory, and all of its child items to the specified path.
        /// </summary>
        /// <param name="copySettings">The settings used for the directory copy.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <returns>The directory object for the copied directory, or <b>null</b> if nothing was copied (e.g. the operation was cancelled).</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copySettings" /> parameter is <b>null</b>.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <see cref="CopyDirectoryArgs.SourceDirectoryPath"/>, or the <see cref="CopyDirectoryArgs.DestinationDirectoryPath"/> was not found.</exception>
        public Task<DirectoryInfo> CopyDirectoryAsync(CopyDirectoryArgs copySettings, CancellationToken cancelToken)
        {
            if (copySettings == null)
            {
                throw new ArgumentNullException(nameof(copySettings));
            }

            CheckRootOfPath(copySettings.SourceDirectory);
            CheckRootOfPath(copySettings.DestinationDirectory);

            if (!copySettings.SourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, copySettings.SourceDirectory.FullName));
            }

            if (!copySettings.DestinationDirectory.Parent.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, copySettings.DestinationDirectory.Parent.FullName));
            }

            return CreateDirectoryCopyTask(copySettings, cancelToken, true);
        }

        /// <summary>
        /// Function to import the specified paths into a virtual file system location.
        /// </summary>
        /// <param name="importSettings">The import settings.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <returns><b>true</b> if the operation was completed successfully, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="importSettings"/> parameter is <b>null</b>.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the <see cref="ImportArgs.ImportRootPath"/> does not exist.</exception>
        /// <remarks>
        /// <para>
        /// This method copies the file (and directory) data for a list of physical file system paths to a virtual file system physical location.
        /// </para>
        /// </remarks>
        public Task<bool> ImportIntoDirectoryAsync(ImportArgs importSettings, CancellationToken cancelToken)
        {
            if (importSettings == null)
            {
                throw new ArgumentNullException(nameof(importSettings));
            }

            CheckRootOfPath(importSettings.DestinationDirectory);

            if (!importSettings.ImportRoot.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, importSettings.ImportRoot.FullName));
            }

            if (importSettings.Items.Count == 0)
            {
                return Task.FromResult(true);
            }

            var subDirs = new List<DirectoryInfo>();
            var files = new List<FileInfo>();

            // Gather the file system information objects for each path.
            foreach (string path in importSettings.Items.Where(item => !string.IsNullOrWhiteSpace(item)))
            {
                if (Directory.Exists(path))
                {
                    subDirs.Add(new DirectoryInfo(path));
                }
                else if (File.Exists(path))
                {
                    files.Add(new FileInfo(path));
                }
            }
                       
            return CreateImportCopyTask(importSettings, subDirs, files, cancelToken);            
        }

        /// <summary>
        /// Function to export the specified directory into a physical file system location.
        /// </summary>
        /// <param name="exportSettings">The settings used for the export.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="exportSettings"/> parameter is <b>null</b>.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <see cref="CopyDirectoryArgs.SourceDirectory"/> was not found.</exception>
        public Task ExportDirectoryAsync(CopyDirectoryArgs exportSettings, CancellationToken cancelToken)
        {
            if (exportSettings == null)
            {
                throw new ArgumentNullException(nameof(exportSettings));
            }

            if (!exportSettings.SourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, exportSettings.SourceDirectory.FullName));
            }

            CheckRootOfPath(exportSettings.SourceDirectory);

            if (!exportSettings.DestinationDirectory.Exists)
            {
                exportSettings.DestinationDirectory.Create();
                exportSettings.DestinationDirectory.Refresh();
            }

            return CreateDirectoryCopyTask(exportSettings, cancelToken, false);
        }

        /// <summary>
        /// Function to export a file to a directory on the physical file system.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destDirPath">The destination directory path.</param>        
        /// <param name="onCopy">The method to call when a file is about to be copied.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destDirPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destDirPath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destDirPath"/> was not found.</exception>
        public Task ExportFileAsync(string filePath, string destDirPath, Action<FileSystemInfo> onCopy)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destDirPath == null)
            {
                throw new ArgumentNullException(nameof(destDirPath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(destDirPath))
            {
                throw new ArgumentEmptyException(nameof(destDirPath));
            }

            var sourceFile = new FileInfo(filePath);
            var dir = new DirectoryInfo(destDirPath);
            
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destDirPath));
            }

            CheckRootOfPath(sourceFile.Directory);

            if (!sourceFile.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath));
            }

            onCopy?.Invoke(sourceFile);

            return Task.Run(() => sourceFile.CopyTo(Path.Combine(dir.FullName, sourceFile.Name), true));
        }

        /// <summary>
        /// Function to copy a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFileNamePath">The destination file name and path.</param>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destFileNamePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="destFileNamePath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destFileNamePath"/> was not found.</exception>
        public void CopyFile(FileInfo filePath, string destFileNamePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destFileNamePath == null)
            {
                throw new ArgumentNullException(nameof(destFileNamePath));
            }

            if (string.IsNullOrWhiteSpace(destFileNamePath))
            {
                throw new ArgumentEmptyException(nameof(destFileNamePath));
            }

            var destFile = new FileInfo(destFileNamePath);

            CheckRootOfPath(filePath.Directory);
            CheckRootOfPath(destFile.Directory);

            if (!filePath.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath));
            }

            if (!destFile.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destFileNamePath));
            }

            filePath.CopyTo(destFile.FullName, true);
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
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, directoryPath));
            }

            if (!dest.Parent.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, dest.Parent.FullName));
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
        /// <param name="destFileNamePath">The destination file name and path.</param>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="destFileNamePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="destFileNamePath"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="destFileNamePath"/> was not found.</exception>
        public void MoveFile(FileInfo filePath, string destFileNamePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (destFileNamePath == null)
            {
                throw new ArgumentNullException(nameof(destFileNamePath));
            }

            if (string.IsNullOrWhiteSpace(destFileNamePath))
            {
                throw new ArgumentEmptyException(nameof(destFileNamePath));
            }

            var destFile = new FileInfo(destFileNamePath);

            CheckRootOfPath(filePath.Directory);
            CheckRootOfPath(destFile.Directory);

            if (!filePath.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, filePath));
            }

            if (!destFile.Directory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, destFileNamePath));
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
