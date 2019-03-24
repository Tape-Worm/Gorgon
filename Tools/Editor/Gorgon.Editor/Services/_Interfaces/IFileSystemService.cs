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
// Created: September 7, 2018 12:32:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.ViewModels;

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
        Exception = 5,
        /// <summary>
        /// The operation should skip this item.
        /// </summary>
        Skip = 6
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
        bool FileExists(FileInfo path);

        /// <summary>
        /// Function to determine if a directory exists or not.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns><b>true</b> if the directory exists, <b>false</b> if not.</returns>
        bool DirectoryExists(DirectoryInfo path);

        /// <summary>
        /// Function to create an empty 1-byte file.
        /// </summary>
        /// <param name="parentDir">The physical parent directory for the file.</param>
        /// <param name="name">The name of the file.</param>
        /// <returns>A file information obejct representing the file on the disk.</returns>
        FileInfo CreateEmptyFile(DirectoryInfo parentDir, string name);

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory on the physical file system.</param>
        /// <param name="newDirName">The name of the new directory.</param>
        /// <returns>A new directory information object for the new directory.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentDirectory"/>, or the <paramref name="newDirName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newDirName"/> parameter is empty.</exception>
        DirectoryInfo CreateDirectory(DirectoryInfo parentDirectory, string newDirName);

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory on the physical file system.</param>
        /// <returns>A new directory information object for the new directory.</returns>
        DirectoryInfo CreateDirectory(DirectoryInfo parentDirectory);

        /// <summary>
        /// Function to rename a directory.
        /// </summary>
        /// <param name="directoryPath">The physical file system path to the directory to rename.</param>
        /// <param name="newName">The new directory name.</param>
        /// <returns>The full physical file system path of the new directory name.</returns>
        void RenameDirectory(DirectoryInfo directoryPath, string newName);

        /// <summary>
        /// Function to rename a file.
        /// </summary>
        /// <param name="filePath">The physical file system path to the file to rename.</param>
        /// <param name="newName">The new name of the file.</param>        
        void RenameFile(FileInfo filePath, string newName);

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
        bool DeleteDirectory(DirectoryInfo directoryPath, Action<FileSystemInfo> onDelete, CancellationToken cancelToken);

        /// <summary>
        /// Function to delete a file.
        /// </summary>
        /// <param name="filePath">The path to the file on the physical file system to delete.</param>
        void DeleteFile(FileInfo filePath);

        /// <summary>
        /// Function to export a file to a directory on the physical file system.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destPath">The destination file path.</param>        
        /// <param name="exportProgress">The callback method used to report how much of the file has been exported.</param>
        /// <param name="cancelToken">A token used to cancel the operation.</param>
        /// <param name="writeBuffer">[Optional] The buffer used to write out the file in chunks.</param>
        void ExportFile(FileInfo filePath, FileInfo destPath, Action<long, long> exportProgress, CancellationToken cancelToken, byte[] writeBuffer = null);

        /// <summary>
        /// Function to copy a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFile">The destination file.</param>
        /// <param name="progressCallback">The method that reports the file copy progress back.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <param name="writeBuffer">[Optional] The buffer used to write out the file in chunks.</param>
        void CopyFile(FileInfo filePath, FileInfo destFile, Action<long, long> progressCallback, CancellationToken cancelToken, byte[] writeBuffer = null);

        /// <summary>
        /// Function to move a file to another location.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="destFile">The destination file.</param>        
        void MoveFile(FileInfo filePath, FileInfo destFile);

        /// <summary>
        /// Function to move a directory to another location.
        /// </summary>
        /// <param name="directoryPath">The path to the directory.</param>
        /// <param name="destDirectoryPath">The destination name and path.</param>        
        void MoveDirectory(DirectoryInfo directoryPath, string destDirectoryPath);

        /// <summary>
        /// Function to delete all files and directories in the file system.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Function to import a file from the external physical file system.
        /// </summary>
        /// <param name="file">The file being imported.</param>
        /// <param name="dest">The destination for the imported file.</param>
        /// <param name="importProgress">The method used to report the progress of the import.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <param name="writeBuffer">[Optional] The buffer used to copy the file in blocks.</param>
        void ImportFile(FileInfo file, FileInfo dest, Action<long, long> importProgress, CancellationToken cancelToken, byte[] writeBuffer = null);
        #endregion
    }
}
