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
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Metadata;
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
        /// Property to set or return the metadata for the node.
        /// </summary>
        ProjectItemMetadata Metadata
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether this node represents content or not.
        /// </summary>
        bool IsContent
        {
            get;            
        }

        /// <summary>
        /// Property to set or return whether to mark this node as "cut" or not.
        /// </summary>
        bool IsCut
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

        /// <summary>
        /// Property to set or return whether the node is in an expanded state or not (if it has children).
        /// </summary>
        bool IsExpanded
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if this node is an ancestor of the specified parent node.
        /// </summary>
        /// <param name="parent">The parent node to look for.</param>
        /// <returns><b>true</b> if the node is an ancestor, <b>false</b> if not.</returns>
        bool IsAncestorOf(IFileExplorerNodeVm parent);

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="newName">The new name for the node.</param>
        /// <param name="projectItems">The list of items in the project.</param>
        void RenameNode(string newName, IDictionary<string, ProjectItemMetadata> projectItems);

        /// <summary>
        /// Function to delete the node.
        /// </summary>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter passes a file system information that contains name of the node being deleted, so callers can use that information for their own purposes.
        /// </para>
        /// </remarks>
        Task DeleteNodeAsync(Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to copy this node to another node.
        /// </summary>
        /// <param name="newPath">The node that will receive the the copy of this node.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns>The new node for the copied node.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onCopy"/> callback method sends the file system node being copied, the destination file system node, the current item #, and the total number of items to copy.
        /// </para>
        /// </remarks>
        Task<IFileExplorerNodeVm> CopyNodeAsync(IFileExplorerNodeVm destNode, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to move this node to another node.
        /// </summary>
        /// <param name="newPath">The node that will receive the the copy of this node.</param>
        /// <returns>The new node for the copied node.</returns>
        IFileExplorerNodeVm MoveNode(IFileExplorerNodeVm destNode);

        /// <summary>
        /// Function to export the contents of this node to the physical file system.
        /// </summary>
        /// <param name="destPath">The path to the directory on the physical file system that will receive the contents.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onCopy"/> callback method sends the file system node being copied, the destination file system node, the current item #, and the total number of items to copy.
        /// </para>
        /// </remarks>
        Task ExportAsync(string destPath, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to assign the appropriate content plug in to a node.
        /// </summary>
        /// <param name="contentPlugins">The plug ins to evaluate.</param>
        /// <param name="deepScan">[Optional] <b>true</b> to perform a more in depth scan for the associated plug in type, <b>false</b> to use the node metadata exclusively.</param>
        /// <returns><b>true</b> if a plug in was associated, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="deepScan"/> parameter is set to <b>true</b>, then the lookup for the plug ins will involve opening the file using each plug in to find a matching plug in for the node 
        /// file type. This, obviously, is much slower, so should only be used when the node metadata is not sufficient for association information.
        /// </para>
        /// </remarks>
        bool AssignContentPlugin(IContentPluginManagerService contentPlugins, bool deepScan = false);
        #endregion
    }
}
