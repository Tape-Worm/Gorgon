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
    /// Event arguments for the <see cref="GorgonFolderBrowser.FolderRenaming"/> event.
    /// </summary>
    public class FolderRenameArgs
        : CancelEventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to return the path to the directory being renamed.
        /// </summary>
        public string OldDirectoryPath
        {
            get;
        }

        /// <summary>
        /// Property to return the path to the new name for the directory.
        /// </summary>
        public string NewDirectoryPath
        {
            get;
        }

        /// <summary>
        /// Property to return the old name.
        /// </summary>
        public string OldName
        {
            get;
        }

        /// <summary>
        /// Property to return the new name.
        /// </summary>
        public string NewName
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether this event has handled the renaming of the directory on our behalf.
        /// </summary>
        public bool RenameHandled
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FolderRenameArgs"/> class.</summary>
        /// <param name="oldDirPath">The path to the directory being renamed.</param>
        /// <param name="oldName">The old name for the directory.</param>
        /// <param name="newDirPath">The path to the new directory name.</param>
        /// <param name="newName">The new name for the directory.</param>
        internal FolderRenameArgs(string oldDirPath, string oldName, string newDirPath, string newName)
        {
            OldDirectoryPath = oldDirPath;
            NewDirectoryPath = newDirPath;
            OldName = oldName;
            NewName = newName;
        }
        #endregion
    }
}
