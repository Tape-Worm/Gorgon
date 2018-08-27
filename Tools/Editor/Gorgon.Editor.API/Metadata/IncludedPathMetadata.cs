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
// Created: September 5, 2018 12:35:20 PM
// 
#endregion


using System;
using System.IO;
using Gorgon.Core;

namespace Gorgon.Editor.Metadata
{
    /// <summary>
    /// Indicates whether an item is included in the project or not.
    /// </summary>
    public class IncludedFileSystemPathMetadata
        : IGorgonNamedObject
    {
        #region Properties.
        /// <summary>
        /// Property to return the name of the object.
        /// </summary>
        string IGorgonNamedObject.Name => Path;            

        /// <summary>
        /// Property to return the full path.
        /// </summary>
        public string Path
        {
            get;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="IncludedFileSystemPathMetadata"/> class.
        /// </summary>
        /// <param name="root">The root of the file system.</param>
        /// <param name="fileSystemItem">The file system item to include.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="root"/>, or the <paramref name="fileSystemItem"/> parameter is <b>null</b>.</exception>
        public IncludedFileSystemPathMetadata(DirectoryInfo root, FileSystemInfo fileSystemItem)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (fileSystemItem == null)
            {
                throw new ArgumentNullException(nameof(fileSystemItem));
            }
            
            Path = fileSystemItem.ToFileSystemPath(root);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludedFileSystemPathMetadata"/> class.
        /// </summary>
        /// <param name="path">The path to the included item.</param>
        internal IncludedFileSystemPathMetadata(string path) => Path = path;
        #endregion
    }
}
