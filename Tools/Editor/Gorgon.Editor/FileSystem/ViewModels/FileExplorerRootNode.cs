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
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The root node for the file system.
    /// </summary>
    internal class FileExplorerRootNode
        : FileExplorerDirectoryNodeVm
    {
        #region Variables.
        // Metadata for the root node.
        private readonly ProjectItemMetadata _metadata = new ProjectItemMetadata();
        // The root directory.
        private DirectoryInfo _directory;
        // The path to the root node.
        private string _physicalPath;
        #endregion

        #region Properties.
        /// <summary>Property to set or return whether this node is visible.</summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.</value>
        public override bool Visible
        {
            get => true;
            set
            {
                // Cannot hide root node.
            }
        }

        /// <summary>Property to set or return the metadata for the node.</summary>
        public override ProjectItemMetadata Metadata
        {
            get => _metadata;
            set
            {
                // No meta data can be attached to a root node.
            }
        }

        /// <summary>Property to set or return whether the node is open for editing.</summary>
        public override bool IsOpen
        {
            get => false;
            set
            {
                // The root node is never opened.
            }
        }

        /// <summary>Property to return the parent node for this node.</summary>
        public override IFileExplorerNodeVm Parent => null;

        /// <summary>Property to return the name for the node.</summary>
        public override string Name => "/";

        /// <summary>Property to return the full path to the node.</summary>
        public override string FullPath => "/";

        /// <summary>Property to return the physical path to the node.</summary>
        public override string PhysicalPath => _physicalPath;

        /// <summary>Property to return whether or not the allow this node to be deleted.</summary>
        public override bool AllowDelete => false;

        /// <summary>Property to set or return whether the node is in an expanded state or not (if it has children).</summary>
        public override bool IsExpanded
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
        protected override void OnInitialize(FileExplorerNodeParameters injectionParameters)
        {
            if (injectionParameters.Project == null)
            {
                throw new ArgumentMissingException(nameof(ViewModelCommonParameters.Project), nameof(injectionParameters));
            }

            if ((injectionParameters.Project?.FileSystemDirectory == null) || (!injectionParameters.Project.FileSystemDirectory.Exists))
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, injectionParameters.Project?.FileSystemDirectory?.FullName ?? "NULL"));
            }

            _directory = injectionParameters.Project.FileSystemDirectory;
            _physicalPath = _directory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
        }

        /// <summary>Function called when the parent of this node is moved.</summary>
        /// <param name="newNode">The new node representing this node under the new parent.</param>
        protected override void OnNotifyParentMoved(IFileExplorerNodeVm newNode)
        {
            // Do nothing, root nodes cannot move.
        }

        /// <summary>
        /// Function to refresh the node's underlying data.
        /// </summary>
        public override void Refresh()
        {
            _directory?.Refresh();
            _physicalPath = _directory?.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            foreach (IFileExplorerNodeVm node in Children.Traverse(n => n.Children))
            {
                node.Refresh();
            }
        }

        /// <summary>Function to delete the node.</summary>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// The <paramref name="onDeleted"/> parameter passes the node that being deleted, so callers can use that information for their own purposes.
        public override Task DeleteNodeAsync(Action<IFileExplorerNodeVm> onDeleted = null, CancellationToken? cancelToken = null) => Task.FromResult<object>(null);

        /// <summary>
        /// Function to move this node into another node.
        /// </summary>
        /// <param name="copyNodeData">The parameters used for moving the node.</param>
        /// <returns>The udpated node.</returns>
        public override IFileExplorerNodeVm MoveNode(CopyNodeData copyNodeData) => this;

        /// <summary>Function to rename the node.</summary>
        /// <param name="newName">The new name for the node.</param>
        public override void RenameNode(string newName)
        {
            // Do nothing.  Root nodes cannot be renamed.
        }

        /// <summary>
        /// Function to retrieve the size of the data on the physical file system.
        /// </summary>        
        /// <returns>The size of the data on the physical file system, in bytes.</returns>
        /// <remarks>
        /// <para>
        /// For nodes with children, this will sum up the size of each item in the <see cref="Children"/> list.  For items that do not have children, then only the size of the immediate item is returned.
        /// </para>
        /// </remarks>
        public override long GetSizeInBytes() => Children.Count == 0 ? 0 : Children.Traverse(n => n.Children).Sum(n => n.GetSizeInBytes());
        #endregion
    }
}
