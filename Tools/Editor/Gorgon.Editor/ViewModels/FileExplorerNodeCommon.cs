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
// Created: September 19, 2018 10:32:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Common functionality for file system node view models.
    /// </summary>
    internal abstract class FileExplorerNodeCommon
        : ViewModelBase<FileExplorerNodeParameters>, IFileExplorerNodeVm
    {
        #region Variables.
        // The parent for this node.
        private IFileExplorerNodeVm _parent;
        // The name of the file.
        private string _name;
        // The physical file system path to the node.
        private string _physicalPath;
        // Flag to indicate whether this node is expanded or not.
        private bool _isExpanded;
        // Flag to indicate that the node is cut.
        private bool _isCut;
        // The metadata for the node.
        private ProjectItemMetadata _metadata;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the factory used to build view models.
        /// </summary>
        protected ViewModelFactory ViewModelFactory
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the file system service used to manipulate the underlying file system.
        /// </summary>
        protected IFileSystemService FileSystemService
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the busy state service.
        /// </summary>
        protected IBusyStateService BusyService
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the message display service.
        /// </summary>
        protected IMessageDisplayService MessageDisplay
        {
            get;
            private set;
        }

        /// <summary>Property to return whether this node represents content or not.</summary>
        public abstract bool IsContent
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether to mark this node as "cut" or not.
        /// </summary>
        public bool IsCut
        {
            get => _isCut;
            set
            {
                if (_isCut == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isCut = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public abstract bool AllowChildCreation
        {
            get;
        }

        /// <summary>
        /// Property to return the child nodes for this node.
        /// </summary>
        public ObservableCollection<IFileExplorerNodeVm> Children
        {
            get;
            protected set;
        } = new ObservableCollection<IFileExplorerNodeVm>();

        /// <summary>
        /// Property to return the parent node for this node.
        /// </summary>
        public IFileExplorerNodeVm Parent
        {
            get => _parent;
            protected set
            {
                if (_parent == value)
                {
                    return;
                }

                OnPropertyChanging();
                _parent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the type of node.
        /// </summary>
        public abstract NodeType NodeType
        {
            get;
        }

        /// <summary>
        /// Property to return the name for the node.
        /// </summary>
        public string Name
        {
            get => _name;
            protected set
            {
                if (string.Equals(value, _name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        public abstract string FullPath
        {
            get;
        }

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        public abstract string ImageName
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public abstract bool AllowDelete
        {
            get;
        }

        /// <summary>
        /// Property to return the physical path to the node.
        /// </summary>        
        public string PhysicalPath
        {
            get => _physicalPath;
            protected set
            {
                if (string.Equals(_physicalPath, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _physicalPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return whether the node is in an expanded state or not (if it has children).
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the metadata for the node.</summary>        
        public virtual ProjectItemMetadata Metadata
        {
            get => _metadata;
            set
            {
                if (_metadata == value)
                {
                    return;
                }

                OnPropertyChanging();
                _metadata = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the root parent node for this node.
        /// </summary>
        /// <returns>The root parent node.</returns>
        protected IFileExplorerNodeVm GetRoot()
        {
            if (Parent == null)
            {
                return this;
            }

            IFileExplorerNodeVm parent = this;

            while (parent.Parent != null)
            {
                parent = parent.Parent;                
            }

            return parent;
        }

        /// <summary>
        /// Function to assign the appropriate content plug in to a node.
        /// </summary>
        /// <param name="contentPlugins">The plug ins to evaluate.</param>
        /// <param name="deepScan"><b>true</b> to perform a more in depth scan for the associated plug in type, <b>false</b> to use the node metadata exclusively.</param>
        /// <returns><b>true</b> if a plug in was assigned, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentPlugins"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="deepScan" /> parameter is set to <b>true</b>, then the lookup for the plug ins will involve opening the file using each plug in to find a matching plug in for the node
        /// file type. This, obviously, is much slower, so should only be used when the node metadata is not sufficient for association information.
        /// </para>
        /// </remarks>
        protected virtual bool OnAssignContentPlugin(IContentPluginManagerService plugins, bool deepScan) => false;

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(FileExplorerNodeParameters injectionParameters)
        {
            if (injectionParameters.Project == null)
            {
                throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.Project), nameof(injectionParameters));
            }

            FileSystemService = injectionParameters.FileSystemService ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.FileSystemService), nameof(injectionParameters));
            ViewModelFactory = injectionParameters.ViewModelFactory ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.ViewModelFactory), nameof(injectionParameters));
            _physicalPath = injectionParameters.PhysicalPath ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.PhysicalPath), nameof(injectionParameters));

            // This is the root node if we have no parent.
            _name = injectionParameters.Parent == null ? "/" : (injectionParameters.Name ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.Name), nameof(injectionParameters)));
            MessageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.MessageDisplay), nameof(injectionParameters));
            BusyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.BusyService), nameof(injectionParameters));

            // Optional.
            _parent = injectionParameters.Parent;

            Children = injectionParameters.Children ?? new ObservableCollection<IFileExplorerNodeVm>();

            // Determine if we are included in the project or not.
            injectionParameters.Project.ProjectItems.TryGetValue(FullPath, out _metadata);
        }

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
        public abstract Task DeleteNodeAsync(Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="newName">The new name for the node.</param>
        /// <param name="projectItems">The list of items in the project.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="newName"/>, or the <paramref name="projectItems"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public abstract void RenameNode(string newName, IDictionary<string, ProjectItemMetadata> projectItems);

        /// <summary>
        /// Function to determine if this node is an ancestor of the specified parent node.
        /// </summary>
        /// <param name="parent">The parent node to look for.</param>
        /// <returns><b>true</b> if the node is an ancestor, <b>false</b> if not.</returns>
        public bool IsAncestorOf(IFileExplorerNodeVm parent)
        {
            IFileExplorerNodeVm parentOfMe = Parent;

            // We're at the root, so we have nothing.
            if (parentOfMe == null)
            {
                return false;
            }

            while ((parentOfMe != null) && (parent != parentOfMe))
            {
                parentOfMe = parentOfMe.Parent;
            }

            return parentOfMe != null;
        }

        /// <summary>
        /// Function to copy this node to another node.
        /// </summary>
        /// <param name="destNode">The dest node.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns>The new node for the copied node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destNode" /> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destNode" /> is unable to create child nodes.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="onCopy" /> callback method sends the file system item being copied, the destination file system item, the current item #, and the total number of items to copy.
        /// </para>
        /// </remarks>
        public abstract Task<IFileExplorerNodeVm> CopyNodeAsync(IFileExplorerNodeVm destNode, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to move this node to another node.
        /// </summary>
        /// <param name="newPath">The node that will receive the the copy of this node.</param>
        /// <returns>The new node for the copied node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destNode"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destNode"/> is unable to create child nodes.</exception>
        public abstract IFileExplorerNodeVm MoveNode(IFileExplorerNodeVm destNode);

        /// <summary>
        /// Function to export the contents of this node to the physical file system.
        /// </summary>
        /// <param name="destPath">The path to the directory on the physical file system that will receive the contents.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>The <paramref name="onCopy" /> callback method sends the file system node being copied, the destination file system node, the current item #, and the total number of items to copy.</remarks>
        public abstract Task ExportAsync(string destPath, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null);

        /// <summary>Function to assign the appropriate content plug in to a node.</summary>
        /// <param name="contentPlugins">The plug ins to evaluate.</param>
        /// <param name="deepScan">[Optional] <b>true</b> to perform a more in depth scan for the associated plug in type, <b>false</b> to use the node metadata exclusively.</param>
        /// <returns><b>true</b> if a plug in was associated, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentPlugins" /> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// If the <paramref name="deepScan" /> parameter is set to <b>true</b>, then the lookup for the plug ins will involve opening the file using each plug in to find a matching plug in for the node
        /// file type. This, obviously, is much slower, so should only be used when the node metadata is not sufficient for association information.
        /// </remarks>
        public bool AssignContentPlugin(IContentPluginManagerService contentPlugins, bool deepScan = false)
        {
            if (contentPlugins == null)
            {
                throw new ArgumentNullException(nameof(contentPlugins));
            }

            if (!IsContent)
            {
                return false;
            }

            return OnAssignContentPlugin(contentPlugins, deepScan);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerNodeCommon"/> class.
        /// </summary>
        /// <param name="copy">The node to copy.</param>
        internal FileExplorerNodeCommon(FileExplorerNodeCommon copy)
        {
            BusyService = copy.BusyService;
            MessageDisplay = copy.MessageDisplay;
            Name = copy.Name;
            IsExpanded = copy.IsExpanded;
            Metadata = copy.Metadata;
            PhysicalPath = copy.PhysicalPath;
            Parent = copy.Parent;
            FileSystemService = copy.FileSystemService;
            ViewModelFactory = copy.ViewModelFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerNodeCommon"/> class.
        /// </summary>
        protected FileExplorerNodeCommon()
        {

        }
        #endregion
    }
}
