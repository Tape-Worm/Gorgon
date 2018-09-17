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
// Created: September 4, 2018 10:20:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The type of node.
    /// </summary>
    internal enum NodeType
    {
        /// <summary>
        /// The node is a file.
        /// </summary>
        File = 0,
        /// <summary>
        /// The node is a directory.
        /// </summary>
        Directory = 1,
        /// <summary>
        /// The node is a link to another file.
        /// </summary>
        Link = 2
    }

    /// <summary>
    /// A node view model for the file explorer.
    /// </summary>
    internal interface IFileExplorerNodeVm
        : IViewModel
    {
        #region Properties.
        /// <summary>
        /// Property to set or return whether or not the node is included in the project.
        /// </summary>
        bool Included
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the child nodes for this node.
        /// </summary>
        ObservableCollection<IFileExplorerNodeVm> Children
        {
            get;
        }

        /// <summary>
        /// Property to return the parent node for this node.
        /// </summary>
        IFileExplorerNodeVm Parent
        {
            get;
        }

        /// <summary>
        /// Property to return the type of node.
        /// </summary>
        NodeType NodeType
        {
            get;
        }

        /// <summary>
        /// Property to return the name for the node.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        string FullPath
        {
            get;
        }

        /// <summary>
        /// Property to return the physical path to the node.
        /// </summary>
        string PhysicalPath
        {
            get;
        }

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        string ImageName
        {
            get;
        }

        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        bool AllowChildCreation
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        bool AllowDelete
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when renaming.</param>
        /// <param name="newName">The new name for the node.</param>
        void RenameNode(IFileSystemService fileSystemService, string newName);

        /// <summary>
        /// Function to delete the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when deleting.</param>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter passes a file system information that contains name of the node being deleted, so callers can use that information for their own purposes.
        /// </para>
        /// </remarks>
        Task DeleteNodeAsync(IFileSystemService fileSystemService, Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null);
        #endregion
    }
}
