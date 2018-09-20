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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

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
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // Flag to indicate that this node is included in the project.
        private bool _included;
        // The name of the file.
        private string _name;
        // The physical file system path to the node.
        private string _physicalPath;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public abstract bool AllowChildCreation
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether or not the node is included in the project.
        /// </summary>
        public bool Included
        {
            get => _included;
            set
            {
                if (_included == value)
                {
                    return;
                }

                OnPropertyChanging();
                _included = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the child nodes for this node.
        /// </summary>
        public ObservableCollection<IFileExplorerNodeVm> Children
        {
            get;
            private set;
        }

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
        #endregion

        #region Methods.
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

            _physicalPath = injectionParameters.FileSystemObject?.FullName ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.FileSystemObject), nameof(injectionParameters));

            // This is the root node if we have no parent.
            _name = injectionParameters.Parent == null ? "/" : injectionParameters.FileSystemObject.Name;
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.BusyState), nameof(injectionParameters));

            // Optional.
            _parent = injectionParameters.Parent;

            Children = injectionParameters.Children ?? new ObservableCollection<IFileExplorerNodeVm>();
            // Determine if we are included in the project or not.
            _included = injectionParameters.Project.Metadata.IncludedPaths.Any(item => string.Equals(item.Path, FullPath, StringComparison.OrdinalIgnoreCase));
        }

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
        public abstract Task DeleteNodeAsync(IFileSystemService fileSystemService, Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null);

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when renaming.</param>
        /// <param name="newName">The new name for the node.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService"/>, or the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public abstract void RenameNode(IFileSystemService fileSystemService, string newName);

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
        #endregion
    }
}
