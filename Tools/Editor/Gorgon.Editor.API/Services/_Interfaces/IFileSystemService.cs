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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to interact with the file system of the project.
    /// </summary>
    public interface IFileSystemService
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
        /// Function to create a directory a new directory.
        /// </summary>
        /// <param name="parentDirectory">The parent directory.</param>
        /// <returns>A new directory information object for the new directory.</returns>
        DirectoryInfo CreateDirectory(string parentDirectory);

        /// <summary>
        /// Function to rename a directory.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to rename.</param>
        /// <param name="newName">The new directory name.</param>
        /// <returns>The full file system path of the new directory name.</returns>
        string RenameDirectory(string directoryPath, string newName);

        /// <summary>
        /// Function to rename a file.
        /// </summary>
        /// <param name="filePath">The path to the file to rename.</param>
        /// <param name="newName">The new name of the file.</param>
        /// <returns>The full file system path of the new file name.</returns>
        string RenameFile(string filePath, string newName);

        /// <summary>
        /// Function to delete a directory.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to delete.</param>
        void DeleteDirectory(string directoryPath);

        /// <summary>
        /// Function to delete a file.
        /// </summary>
        /// <param name="filePath">The path to the file to delete.</param>
        void DeleteFile(string filePath);
        #endregion
    }
}
