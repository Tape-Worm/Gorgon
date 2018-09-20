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
// Created: September 4, 2018 10:48:10 PM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Services;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system directory.
    /// </summary>
    internal class FileExplorerDirectoryNodeVm
        : FileExplorerNodeCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public override bool AllowChildCreation => true;

        /// <summary>
        /// Property to return the type of node.
        /// </summary>
        public override NodeType NodeType => NodeType.Directory;

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        public override string FullPath => Parent == null ? "/" : (Parent.FullPath + Name).FormatDirectory('/');

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        public override string ImageName => "folder_20x20.png";

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public override bool AllowDelete => true;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to delete the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when deleting.</param>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService" /> parameter is <b>null</b>.</exception>        
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter passes a file system information that contains name of the node being deleted, so callers can use that information for their own purposes.
        /// </para>
        /// </remarks>
        public override async Task DeleteNodeAsync(IFileSystemService fileSystemService, Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            // Delete the physical objects first. If we fail here, our node will survive.
            // We do this asynchronously because deleting a directory with a lot of files may take a while.
            bool dirDeleted = await Task.Run(() => fileSystemService.DeleteDirectory(PhysicalPath, onDeleted, cancelToken ?? CancellationToken.None));

            // If, for some reason, our directory was not deleted, then do not remove the node.
            if (!dirDeleted)
            {
                return;
            }

            // Drop us from the parent list.
            // This will begin a chain reaction that will remove us from the UI.
            Parent.Children.Remove(this);
            Parent = null;
        }

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when renaming.</param>
        /// <param name="newName">The new name for the node.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService"/>, or the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public override void RenameNode(IFileSystemService fileSystemService, string newName)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            PhysicalPath = fileSystemService.RenameDirectory(PhysicalPath, newName);
            Name = newName;
            NotifyPropertyChanged(nameof(FullPath));
        }
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
