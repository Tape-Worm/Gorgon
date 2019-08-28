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
using System.Threading.Tasks;

namespace Gorgon.UI
{
    /// <summary>
    /// Event arguments for the <see cref="GorgonFolderBrowser.FolderDeleting"/> event.
    /// </summary>
    public class FolderDeleteArgs
        : CancelEventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to return the path to the directory being deleted.
        /// </summary>
        public string DirectoryPath
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether this event has handled the deletion of the directory on our behalf.
        /// </summary>
        public bool DeletionHandled
        {
            get;
            set;
        }

        /// <summary>
        /// Property to suppress the prompt to ask the user if they are sure they wish to delete.
        /// </summary>
        public bool SuppressPrompt
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return an asynchronous task for deleting.
        /// </summary>
        public Task DeleteTask
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FolderDeleteArgs"/> class.</summary>
        /// <param name="dirPath">The path to the directory to delete.</param>
        internal FolderDeleteArgs(string dirPath) => DirectoryPath = dirPath;
        #endregion
    }
}
