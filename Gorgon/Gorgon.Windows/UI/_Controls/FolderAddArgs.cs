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
// Created: April 28, 2019 10:27:15 AM
// 
#endregion

using System.ComponentModel;

namespace Gorgon.UI
{
    /// <summary>
    /// Event arguments for the <see cref="GorgonFolderBrowser.FolderAdding"/> event.
    /// </summary>
    public class FolderAddArgs
        : CancelEventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to return the path to the directory being created.
        /// </summary>
        public string DirectoryPath
        {
            get;
        }

        /// <summary>
        /// Property to return the path to the parent directory.
        /// </summary>
        public string ParentPath
        {
            get;
        }

        /// <summary>
        /// Property to return the name of the directory.
        /// </summary>
        public string DirectoryName
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether this event has handled the creation of the directory on our behalf.
        /// </summary>
        public bool CreationHandled
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FolderAddArgs"/> class.</summary>
        /// <param name="parentPath">The parent to the parent directory.</param>
        /// <param name="newDirPath">  The path to the new directory.</param>
        /// <param name="directoryName">The name of the directory.</param>
        internal FolderAddArgs(string parentPath, string newDirPath, string directoryName)
        {
            ParentPath = parentPath;
            DirectoryPath = newDirPath;
            DirectoryName = directoryName;
        }
        #endregion
    }
}
