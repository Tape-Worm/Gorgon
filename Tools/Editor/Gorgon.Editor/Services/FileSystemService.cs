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
using System.Threading.Tasks;
using Gorgon.Core;
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
        #region Variables.

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
        /// Function to convert a file system path to a physical path.
        /// </summary>
        /// <param name="path">The file system path.</param>
        /// <param name="isDirectory"><b>true</b> if the path is for a directory, or <b>false</b> if not.</param>
        /// <returns>The path on the physical file system.</returns>
        private string ConvertPathToPhysical(string path, bool isDirectory)
        {
            if (isDirectory)
            {
                if (path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(1);
                }

                path = path.FormatDirectory(Path.DirectorySeparatorChar);
            }
            else
            {
                string dirName = Path.GetDirectoryName(path)?.FormatDirectory('/') ?? string.Empty;

                if (dirName.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    dirName = dirName.Substring(1);
                }

                string fileName = Path.GetFileName(path);
                path = Path.Combine(dirName?.FormatDirectory(Path.DirectorySeparatorChar) ?? string.Empty, fileName);
            }

            return Path.Combine(RootDirectory.FullName, path);
        }

        /// <summary>
        /// Function to create a directory a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory.</param>
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

            parentDirectory = ConvertPathToPhysical(parentDirectory, true);

            int count = 0;
            string newDirName = Path.Combine(parentDirectory, Resources.GOREDIT_NEW_DIR_NAME);
                        
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
        /// <param name="directoryPath">The path to the directory to rename.</param>
        /// <param name="newName">The new directory name.</param>
        /// <returns>The full file system path of the new directory name.</returns>
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

            directoryPath = ConvertPathToPhysical(directoryPath, true);

            var directory = new DirectoryInfo(directoryPath);
            directory.MoveTo(Path.Combine(directory.Parent.FullName, newName));

            return directory.ToFileSystemPath(RootDirectory);
        }

        /// <summary>
        /// Function to rename a file.
        /// </summary>
        /// <param name="filePath">The path to the file to rename.</param>
        /// <param name="newName">The new name of the file.</param>
        /// <returns>The full file system path of the new file name.</returns>
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

            filePath = ConvertPathToPhysical(filePath, false);

            var file = new FileInfo(filePath);
            file.MoveTo(Path.Combine(file.Directory.FullName, newName));

            return file.ToFileSystemPath(RootDirectory);
        }

        /// <summary>
        /// Function to delete a file.
        /// </summary>
        /// <param name="filePath">The path to the file to delete.</param>
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

            filePath = ConvertPathToPhysical(filePath, false);

            var file = new FileInfo(filePath);

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
        /// <param name="directoryPath">The path to the directory to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/> parameter is empty.</exception>
        public void DeleteDirectory(string directoryPath)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentEmptyException(nameof(directoryPath));
            }

            directoryPath = ConvertPathToPhysical(directoryPath, true);

            var directory = new DirectoryInfo(directoryPath);

            // If the directory is already gone, then we don't care.  
            if (!directory.Exists)
            {
                return;
            }
                        
            directory.Delete(true);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemService"/> class.
        /// </summary>
        /// <param name="workspace">The workspace directory.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
        public FileSystemService(DirectoryInfo workspace) => RootDirectory = workspace ?? throw new ArgumentNullException(nameof(workspace));
        #endregion
    }
}
