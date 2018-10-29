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
// Created: September 7, 2018 12:32:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A value indicating how to handle a conflict in the file system.
    /// </summary>
    public enum FileSystemConflictResolution
    {
        /// <summary>
        /// The operation should be canceled.
        /// </summary>
        Cancel = 0,
        /// <summary>
        /// The operation should overwrite the destination.
        /// </summary>
        Overwrite = 1,
        /// <summary>
        /// The operation should rename the destination.
        /// </summary>
        Rename = 2,
        /// <summary>
        /// The operation should overwrite the destination, and this should be the default for all conflicts from this point forward.
        /// </summary>
        OverwriteAll = 3,
        /// <summary>
        /// The operation should rename the destination, and this should be the default for all conflicts from this point forward.
        /// </summary>
        RenameAll = 4,
        /// <summary>
        /// An exception should be thrown.
        /// </summary>
        Exception = 5
    }


    /// <summary>
    /// A service used to interact with the file system of the project.
    /// </summary>
    internal interface IFileSystemService
    {
        #region Properties.
        /// <summary>
        /// Property to return the root directory for the file system.
        /// </summary>
        /// <remarks>
        /// All file system operations will take place under this directory.
        /// </remarks>
        DirectoryInfo RootDirectory
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve a list of sub directories under the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory that contains the sub directories.</param>
        /// <param name="recursive">[Optional] <b>true</b> to retrieve all directories nested in sub directories, or <b>false</b> to just retrieve all directories in the top level.</param>
        /// <returns>A list of directories in the directory.</returns>
        IReadOnlyList<DirectoryInfo> GetDirectories(string path, bool recursive = true);

        /// <summary>
        /// Function to retrieve a list of files under the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory that contains the files.</param>
        /// <param name="recursive">[Optional] <b>true</b> to retrieve all files nested in sub directories, or <b>false</b> to just retrieve all files in the top level.</param>
        /// <returns>A list of files in the directory.</returns>
        IReadOnlyList<FileInfo> GetFiles(string path, bool recursive = true);

        /// <summary>
        /// Function to generate a file name for a destination directory, based on whether or not it already exists.
        /// </summary>
        /// <param name="path">The path to the desired file name.</param>
        /// <returns>The new file name, or the original file name if it did not exist.</returns>
        string GenerateFileName(string path);

        /// <summary>
        /// Function to generate a directory name for a destination directory, based on whether or not it already exists.
        /// </summary>
        /// <param name="path">The path to the desired directory name.</param>
        /// <returns>The new file name, or the original directory name if it did not exist.</returns>
        string GenerateDirectoryName(string path);

        /// <summary>
        /// Function to determine if a file exists or not.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns><b>true</b> if the file exists, <b>false</b> if not.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Function to determine if a directory exists or not.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns><b>true</b> if the directory exists, <b>false</b> if not.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory on the physical file system.</param>
        /// <returns>A new directory information object for the new directory.</returns>
        DirectoryInfo CreateDirectory(string parentDirectory);

        /// <summary>
        /// Function to rename a directory.
        /// </summary>
        /// <param name="directoryPath">The physical file system path to the directory to rename.</param>
        /// <param name="newName">The new directory name.</param>
        /// <returns>The full physical file system path of the new directory name.</returns>
        string RenameDirectory(string directoryPath, string newName);

        /// <summary>
        /// Function to rename a file.
        /// </summary>
        /// <param name="filePath">The physical file system path to the file to rename.</param>
        /// <param name="newName">The new name of the file.</param>
        /// <returns>The full physical file system path of the new file name.</returns>
        string RenameFile(string filePath, string newName);

        /// <summary>
        /// Function to delete a directory.
        /// </summary>
        /// <param name="directoryPath">The physical file system path to the directory to delete.</param>
        /// <param name="onDelete">The method to call when a directory or a child of the directory is deleted.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <returns><b>true</b> if the directory was deleted, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDelete"/> parameter sends a file system information object that contains the name of the item currently being deleted.
        /// </para>
        /// </remarks>
        bool DeleteDirectory(string directoryPath, Action<FileSystemInfo> onDelete, CancellationToken cancelToken);

        /// <summary>
        /// Function to copy a directory, and all of its child items to the specified path.
        /// </summary>
        /// <param name="copySettings">The settings used for the directory copy.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <returns><b>true</b> if the copy was successful, <b>false</b> if it was canceled.</returns>
        Task<bool> CopyDirectoryAsync(CopyDirectoryArgs copySettings, CancellationToken cancelToken);

        /// <summary>
        /// Function to delete a file.
        /// </summary>
        /// <param name="filePath">The path to the file on the physical file system to delete.</param>
        void DeleteFile(string filePath);

        /// <summary>
        /// Function to export a file to a directory on the physical file system.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destDirPath">The destination directory path.</param>        
        /// <param name="onCopy">The method to call when a file is about to be copied.</param>
        Task ExportFileAsync(string filePath, string destDirPath, Action<FileSystemInfo> onCopy);

        /// <summary>
        /// Function to copy a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFileNamePath">The destination file name and path.</param>        
        void CopyFile(string filePath, string destFileNamePath);

        /// <summary>
        /// Function to move a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFileNamePath">The destination file name and path.</param>        
        void MoveFile(string filePath, string destFileNamePath);

        /// <summary>
        /// Function to move a directory to another location.
        /// </summary>
        /// <param name="directoryPath">The path to the directory.</param>
        /// <param name="destDirectoryPath">The destination name and path.</param>        
        void MoveDirectory(string directoryPath, string destDirectoryPath);

        /// <summary>
        /// Function to delete all files and directories in the file system.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Function to export the specified directory into a physical file system location.
        /// </summary>
        /// <param name="exportSettings">The settings used for the export.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        Task ExportDirectoryAsync(CopyDirectoryArgs exportSettings, CancellationToken cancelToken);

        /// <summary>
        /// Function to import the specified paths into a virtual file system location.
        /// </summary>
        /// <param name="importSettings">The import settings.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <returns><b>true</b> if the operation was completed successfully, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This method copies the file (and directory) data for a list of physical file system paths to a virtual file system physical location.
        /// </para>
        /// </remarks>
        Task<bool> ImportIntoDirectoryAsync(ImportArgs importSettings, CancellationToken cancelToken);
        #endregion
    }
}
