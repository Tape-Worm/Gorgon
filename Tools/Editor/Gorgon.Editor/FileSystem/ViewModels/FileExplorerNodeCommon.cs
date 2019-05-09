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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Metadata;
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
        // Flag to indicate whether this node is expanded or not.
        private bool _isExpanded;
        // Flag to indicate that the node is cut.
        private bool _isCut;
        // The metadata for the node.
        private ProjectItemMetadata _metadata;
        // Flag to indicate that the file was changed.
        private bool _isChanged;
        // The physical file system object represented by this node.
        private FileSystemInfo _physicalFileSystemObject;
        // Flag to indicate that this node is visible.
        private bool _isVisible = true;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the currently active project that this node is in.
        /// </summary>
        protected IProject Project
        {
            get;
            private set;                
        }

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

        /// <summary>Property to set or return whether this node is visible.</summary>                
        public virtual bool Visible
        {
            get => Parent == null ? true : (!Parent.Visible ? false : _isVisible);
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return whether the file has changes.</summary>
        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                if (_isChanged == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isChanged = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return whether this node represents content or not.</summary>
        public abstract bool IsContent
        {
            get;
        }
        
        /// <summary>Property to set or return whether the node is open for editing.</summary>
        public abstract bool IsOpen
        {
            get;
            set;
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
        /// Property to return the list of items dependent upon this node
        /// </summary>
        public ObservableCollection<IFileExplorerNodeVm> Dependencies
        {
            get;
        } = new ObservableCollection<IFileExplorerNodeVm>();

        /// <summary>
        /// Property to return the parent node for this node.
        /// </summary>
        public virtual IFileExplorerNodeVm Parent
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
        public virtual string Name => _physicalFileSystemObject?.Name;

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        public abstract string FullPath
        {
            get;
        }

        /// <summary>
        /// Property to return the path to the linked node.
        /// </summary>
        public string LinkPath => FullPath;

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
        public abstract string PhysicalPath
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the node is in an expanded state or not (if it has children).
        /// </summary>
        public virtual bool IsExpanded
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
        /// Function to retrieve the physical file system object for this node.
        /// </summary>
        /// <param name="path">The path to the physical file system object.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        protected void GetFileSystemObject(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            _physicalFileSystemObject = OnGetFileSystemObject(path);
        }

        /// <summary>
        /// Function to retrieve the physical file system object for this node.
        /// </summary>
        /// <param name="path">The path to the physical file system object.</param>
        /// <returns>Information about the physical file system object.</returns>
        protected abstract FileSystemInfo OnGetFileSystemObject(string path);

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
        /// Function called when the parent of this node is moved.
        /// </summary>
        /// <param name="newNode">The new node representing this node under the new parent.</param>
        protected virtual void OnNotifyParentMoved(IFileExplorerNodeVm newNode)
        {

        }

        /// <summary>
        /// Function called to refresh the underlying data for the node.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(FileExplorerNodeParameters injectionParameters)
        {
            Project = injectionParameters.Project ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.Project), nameof(injectionParameters));
            FileSystemService = injectionParameters.FileSystemService ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.FileSystemService), nameof(injectionParameters));
            ViewModelFactory = injectionParameters.ViewModelFactory ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.ViewModelFactory), nameof(injectionParameters));

            // This is the root node if we have no parent.    
            GetFileSystemObject(injectionParameters.PhysicalPath ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.PhysicalPath), nameof(injectionParameters)));
            MessageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.MessageDisplay), nameof(injectionParameters));
            BusyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.BusyService), nameof(injectionParameters));

            // Optional.
            _parent = injectionParameters.Parent;

            _metadata = injectionParameters.Metadata;
            Children = injectionParameters.Children ?? new ObservableCollection<IFileExplorerNodeVm>();
        }

        /// <summary>
        /// Function to notify that the parent of this node was moved.
        /// </summary>
        /// <param name="newNode">The new node representing this node under the new parent.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="newNode"/> parameter is <b>null</b>.</exception>
        public void NotifyParentMoved(IFileExplorerNodeVm newNode)
        {
            if (newNode == null)
            {
                throw new ArgumentNullException(nameof(newNode));
            }

            OnNotifyParentMoved(newNode);
        }

        /// <summary>
        /// Function to delete the node.
        /// </summary>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter passes the node that being deleted, so callers can use that information for their own purposes.
        /// </para>
        /// </remarks>
        public abstract Task DeleteNodeAsync(Action<IFileExplorerNodeVm> onDeleted = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="newName">The new name for the node.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public abstract void RenameNode(string newName);

        /// <summary>
        /// Function to determine if this node is an ancestor of the specified node.
        /// </summary>
        /// <param name="node">The node to look for.</param>
        /// <returns><b>true</b> if the node is an ancestor, <b>false</b> if not.</returns>
        public bool IsAncestorOf(IFileExplorerNodeVm node)
        {
            IFileExplorerNodeVm parentOfNode = node.Parent;

            // We're at the root, so we have nothing.
            if (parentOfNode == null)
            {
                return false;
            }

            while ((parentOfNode != null) && (parentOfNode.Parent != Parent))
            {
                parentOfNode = parentOfNode.Parent;

                if (parentOfNode == this)
                {
                    return true;
                }
            }

            return parentOfNode == this;
        }

        /// <summary>
        /// Function to move this node into another node.
        /// </summary>
        /// <param name="copyNodeData">The parameters used for moving the node.</param>
        /// <returns>The udpated node.</returns>
        public abstract IFileExplorerNodeVm MoveNode(CopyNodeData copyNodeData);

        /// <summary>
        /// Function to export the contents of this node to the physical file system.
        /// </summary>
        /// <param name="exportNodeData">The parameters ued for exporting the node.</param>
        /// <returns>A task for asynchronous operation.</returns>
        public abstract Task ExportAsync(ExportNodeData exportNodeData);

        /// <summary>
        /// Function to copy the file node into another node.
        /// </summary>
        /// <param name="copyNodeData">The data containing information about what to copy.</param>
        /// <returns>The newly copied node.</returns>
        public abstract Task<IFileExplorerNodeVm> CopyNodeAsync(CopyNodeData copyNodeData);

        /// <summary>
        /// Function to retrieve the size of the data on the physical file system.
        /// </summary>        
        /// <returns>The size of the data on the physical file system, in bytes.</returns>
        /// <remarks>
        /// <para>
        /// For nodes with children, this will sum up the size of each item in the <see cref="Children"/> list.  For items that do not have children, then only the size of the immediate item is returned.
        /// </para>
        /// </remarks>
        public abstract long GetSizeInBytes();
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerNodeCommon"/> class.
        /// </summary>
        /// <param name="copy">The node to copy.</param>
        internal FileExplorerNodeCommon(FileExplorerNodeCommon copy)
        {
            Project = copy.Project;
            BusyService = copy.BusyService;
            MessageDisplay = copy.MessageDisplay;
            IsExpanded = copy.IsExpanded;
            Metadata = copy.Metadata;
            Parent = copy.Parent;
            FileSystemService = copy.FileSystemService;
            ViewModelFactory = copy.ViewModelFactory;
            IsChanged = copy.IsChanged;            
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
