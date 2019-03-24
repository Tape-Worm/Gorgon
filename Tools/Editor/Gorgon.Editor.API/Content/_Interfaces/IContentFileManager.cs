#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: March 2, 2019 5:29:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Editor.Content
{
    /// <summary>
    /// Provides access to the file system for reading, writing, creating and deleting content files and directories.
    /// </summary>
    public interface IContentFileManager
    {
        #region Methods.
        /// <summary>
        /// Function to create a new directory
        /// </summary>
        /// <param name="path">The path to the directory.</param>        
        /// <returns><b>true</b> if the directory exists, <b>false</b> if not.</returns>
        void CreateDirectory(string path);

        /// <summary>
        /// Function to delete a directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="deleteChildren"><b>true</b> true to delete child directories and files, <b>false</b> to delete only this directory (must be empty).</param>
        void DeleteDirectory(string path, bool deleteChildren);

        /// <summary>
        /// Function to determine if a directory exists or not.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns><b>true</b> if the directory exists, <b>false</b> if not.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Function to retrieve a file based on the path specified.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A <see cref="IContentFile"/> if found, <b>null</b> if not.</returns>
        IContentFile GetFile(string path);

        /// <summary>
        /// Function to create a new content file on the path specified.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="dataStream">A callback method to write the file data.</param>
        /// <returns>The content file.</returns>
        IContentFile WriteFile(string path, Action<Stream> dataStream);
        #endregion
    }
}
