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
// Created: November 28, 2018 4:20:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The root node for the file system.
    /// </summary>
    internal class FileExplorerRootNode
        : ViewModelBase<ViewModelCommonParameters>, IFileExplorerNodeVm
    {
        #region Variables.
        // Metadata for the root node.
        private readonly ProjectItemMetadata _metadata = new ProjectItemMetadata();
        // The root directory.
        private DirectoryInfo _directory;
        #endregion

        #region Properties.
        /// <summary>Property to set or return the metadata for the node.</summary>
        public ProjectItemMetadata Metadata
        {
            get => _metadata;
            set
            {
                // No meta data can be attached to a root node.
            }
        }

        /// <summary>Property to set or return whether the file has changes.</summary>
        public bool IsChanged
        {
            get => false;
            set
            {
                // Cannot change the root node.
            }
        }

        /// <summary>Property to return whether this node represents content or not.</summary>
        public bool IsContent => false;

        /// <summary>Property to set or return whether to mark this node as "cut" or not.</summary>
        public bool IsCut
        {
            get => false;
            set
            {
                // The root node cannot be cut.
            }
        }

        /// <summary>Property to set or return whether the node is open for editing.</summary>
        public bool IsOpen
        {
            get => false;
            set
            {
                // The root node is never opened.
            }
        }

        /// <summary>Property to return the child nodes for this node.</summary>
        public ObservableCollection<IFileExplorerNodeVm> Children
        {
            get;
        } = new ObservableCollection<IFileExplorerNodeVm>();

        /// <summary>Property to return the parent node for this node.</summary>
        public IFileExplorerNodeVm Parent => null;

        /// <summary>Property to return the type of node.</summary>
        public NodeType NodeType => NodeType.Directory;

        /// <summary>Property to return the name for the node.</summary>
        public string Name => "/";

        /// <summary>Property to return the full path to the node.</summary>
        public string FullPath => "/";

        /// <summary>Property to return the physical path to the node.</summary>
        public string PhysicalPath
        {
            get;
            private set;
        }

        /// <summary>Property to return the image name to use for the node type.</summary>
        public string ImageName => "folder_20x20.png";

        /// <summary>Property to return whether to allow child node creation for this node.</summary>
        public bool AllowChildCreation => true;

        /// <summary>Property to return whether or not the allow this node to be deleted.</summary>
        public bool AllowDelete => false;

        /// <summary>Property to set or return whether the node is in an expanded state or not (if it has children).</summary>
        public bool IsExpanded
        {
            get => true;
            set
            {
                // The root node is always expanded.
            }
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(ViewModelCommonParameters injectionParameters)
        {
            if (injectionParameters.Project == null)
            {
                throw new ArgumentMissingException(nameof(ViewModelCommonParameters.Project), nameof(injectionParameters));
            }

            if ((injectionParameters.Project?.ProjectWorkSpace == null) || (!injectionParameters.Project.ProjectWorkSpace.Exists))
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, injectionParameters.Project?.ProjectWorkSpace?.FullName ?? "NULL"));
            }

            _directory = injectionParameters.Project.ProjectWorkSpace;
            PhysicalPath = _directory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Function to refresh the node's underlying data.
        /// </summary>
        public void Refresh()
        {
            _directory?.Refresh();
            PhysicalPath = _directory?.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            foreach (IFileExplorerNodeVm node in Children.Traverse(n => n.Children))
            {
                node.Refresh();
            }
        }

        /// <summary>Function to assign the appropriate content plug in to a node.</summary>
        /// <param name="contentPlugins">The plug ins to evaluate.</param>
        /// <param name="deepScan">[Optional] <b>true</b> to perform a more in depth scan for the associated plug in type, <b>false</b> to use the node metadata exclusively.</param>
        /// <returns>
        ///   <b>true</b> if a plug in was associated, <b>false</b> if not.</returns>
        /// <remarks>
        /// If the <paramref name="deepScan" /> parameter is set to <b>true</b>, then the lookup for the plug ins will involve opening the file using each plug in to find a matching plug in for the node
        /// file type. This, obviously, is much slower, so should only be used when the node metadata is not sufficient for association information.
        /// </remarks>
        public bool AssignContentPlugin(IContentPluginManagerService contentPlugins, bool deepScan = false) => false;

        /// <summary>Function to copy this node to another node.</summary>
        /// <param name="destNode">The destination node that will receive the copy.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns><b>null</b> since root nodes cannot be copied.</returns>
        /// <remarks>The <paramref name="onCopy" /> callback method sends the file system node being copied, the destination file system node, the current item #, and the total number of items to copy.</remarks>
        public Task<IFileExplorerNodeVm> CopyNodeAsync(IFileExplorerNodeVm destNode, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null) => Task.FromResult<IFileExplorerNodeVm>(null);


        /// <summary>Function to delete the node.</summary>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>The <paramref name="onDeleted" /> parameter passes a file system information that contains name of the node being deleted, so callers can use that information for their own purposes.</remarks>
        public Task DeleteNodeAsync(Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null) => Task.FromResult<object>(null);

        /// <summary>Function to export the contents of this node to the physical file system.</summary>
        /// <param name="destPath">The path to the directory on the physical file system that will receive the contents.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>The <paramref name="onCopy" /> callback method sends the file system node being copied, the destination file system node, the current item #, and the total number of items to copy.</remarks>
#warning We should actually put this in.
        public Task ExportAsync(string destPath, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null) => Task.FromResult<object>(null);

        /// <summary>Function to determine if this node is an ancestor of the specified parent node.</summary>
        /// <param name="parent">The parent node to look for.</param>
        /// <returns><b>true</b> if the node is an ancestor, <b>false</b> if not.</returns>        
        public bool IsAncestorOf(IFileExplorerNodeVm parent) => parent != this;

        /// <summary>Function to move this node to another node.</summary>
        /// <param name="destNode">The node that will receive this node as a child.</param>
        /// <returns><b>false</b> since root nodes cannot be moved.</returns>
        public bool MoveNode(IFileExplorerNodeVm destNode) => false;

        /// <summary>Function to notify that the parent of this node was moved.</summary>
        /// <param name="newNode">The new node representing this node under the new parent.</param>
        public void NotifyParentMoved(IFileExplorerNodeVm newNode)
        {
            // Do nothing.  Root nodes will never move.
        }

        /// <summary>Function to rename the node.</summary>
        /// <param name="newName">The new name for the node.</param>
        public void RenameNode(string newName)
        {
            // Do nothing.  Root nodes cannot be renamed.
        }
        #endregion
    }
}
