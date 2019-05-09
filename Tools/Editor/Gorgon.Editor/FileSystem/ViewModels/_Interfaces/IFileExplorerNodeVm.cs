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
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The type of node.
    /// </summary>
    internal enum NodeType
    {
        /// <summary>
        /// The node is a directory.
        /// </summary>
        Directory = 0,
        /// <summary>
        /// The node is a file.
        /// </summary>
        File = 1,
        /// <summary>
        /// The node is a link to another file.
        /// </summary>
        Link = 2
    }

    /// <summary>
    /// A node view model for the file explorer.
    /// </summary>
    internal interface IFileExplorerNodeVm
        : IViewModel, IGorgonNamedObject
    {
        #region Properties.
        /// <summary>
        /// Property to return the list of items dependent upon this node
        /// </summary>
        ObservableCollection<IFileExplorerNodeVm> Dependencies
        {
            get;
        }

        /// <summary>
        /// Property to set or return the metadata for the node.
        /// </summary>
        ProjectItemMetadata Metadata
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether this node is visible.
        /// </summary>
        bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the file has changes.
        /// </summary>
        bool IsChanged
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
        /// Property to set or return whether the node is open for editing.
        /// </summary>
        bool IsOpen
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
        /// Property to return the full path to the node.
        /// </summary>
        string FullPath
        {
            get;
        }

        /// <summary>
        /// Property to return the path to the linked node.
        /// </summary>
        string LinkPath
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
        /// Function to retrieve the size of the data on the physical file system.
        /// </summary>        
        /// <returns>The size of the data on the physical file system, in bytes.</returns>
        /// <remarks>
        /// <para>
        /// For nodes with children, this will sum up the size of each item in the <see cref="Children"/> list.  For items that do not have children, then only the size of the immediate item is returned.
        /// </para>
        /// </remarks>
        long GetSizeInBytes();

        /// <summary>
        /// Function to notify that the parent of this node was moved.
        /// </summary>
        /// <param name="newNode">The new node representing this node under the new parent.</param>
        void NotifyParentMoved(IFileExplorerNodeVm newNode);

        /// <summary>
        /// Function to determine if this node is an ancestor of the specified node.
        /// </summary>
        /// <param name="node">The node to look for.</param>
        /// <returns><b>true</b> if the node is an ancestor, <b>false</b> if not.</returns>
        bool IsAncestorOf(IFileExplorerNodeVm node);

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="newName">The new name for the node.</param>
        void RenameNode(string newName);

        /// <summary>
        /// Function to delete this node.
        /// </summary>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter passes the node that being deleted, so callers can use that information for their own purposes.
        /// </para>
        /// </remarks>
        Task DeleteNodeAsync(Action<IFileExplorerNodeVm> onDeleted = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to copy the file node into another node.
        /// </summary>
        /// <param name="copyNodeData">The data containing information about what to copy.</param>
        /// <returns>The newly copied node.</returns>
        Task<IFileExplorerNodeVm> CopyNodeAsync(CopyNodeData copyNodeData);

        /// <summary>
        /// Function to move this node into another node.
        /// </summary>
        /// <param name="copyNodeData">The parameters used for moving the node.</param>
        /// <returns>The udpated node.</returns>
        IFileExplorerNodeVm MoveNode(CopyNodeData copyNodeData);

        /// <summary>
        /// Function to export the contents of this node to the physical file system.
        /// </summary>
        /// <param name="exportNodeData">The parameters used for exporting the node.</param>
        /// <returns>A task for asynchronous operation.</returns>
        Task ExportAsync(ExportNodeData exportNodeData);

        /// <summary>
        /// Function to refresh the node's underlying data.
        /// </summary>
        void Refresh();
        #endregion
    }
}
