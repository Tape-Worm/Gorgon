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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor;
using System.Threading;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system file.
    /// </summary>
    internal class FileExplorerFileNodeVm
        : ViewModelBase<FileExplorerNodeParameters>, IFileExplorerNodeVm
    {
        #region Variables.
        // The parent for this node.
        private IFileExplorerNodeVm _parent;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // Flag to indicate whether the file node is included or not.
        private bool _included;
        // The name of the file.
        private string _name;
        // The full path to the file.
        private string _fullPath;
        // The path to the file on the physical file system.
        private string _physicalPath;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public bool AllowChildCreation => false;

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
            private set
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
        public NodeType NodeType => NodeType.File;

        /// <summary>
        /// Property to return the name for the node.
        /// </summary>
        public string Name
        {
            get => _name;
            private set
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
        public string FullPath => (_parent == null ? "/" : _parent.FullPath) + Name;

        /// <summary>
        /// Property to return the physical path to the node.
        /// </summary>
        public string PhysicalPath
        {
            get => _physicalPath;
            private set
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
        /// Property to return the image name to use for the node type.
        /// </summary>
        public string ImageName => "generic_file_20x20.png";

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public bool AllowDelete => true;
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
            
            _name = injectionParameters.FileSystemObject?.Name ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.FileSystemObject), nameof(injectionParameters));
            _physicalPath = injectionParameters.FileSystemObject.FullName;
            _fullPath = injectionParameters.FileSystemObject.ToFileSystemPath(injectionParameters.Project.ProjectWorkSpace);
            _parent = injectionParameters.Parent ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.Parent), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(FileExplorerNodeParameters.BusyState), nameof(injectionParameters));

            Children = injectionParameters.Children ?? new ObservableCollection<IFileExplorerNodeVm>();

            // Determine if we are included in the project or not.
            _included = injectionParameters.Project.Metadata.IncludedPaths.Any(item => string.Equals(item.Path, FullPath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when renaming.</param>
        /// <param name="newName">The new name for the node.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService"/>, or the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public void RenameNode(IFileSystemService fileSystemService, string newName)
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

            PhysicalPath = fileSystemService.RenameFile(PhysicalPath, newName);
            Name = newName;
            NotifyPropertyChanged(nameof(FullPath));
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
        /// The <paramref name="onDeleted"/> parameter is not used for this type.
        /// </para>
        /// </remarks>
        public Task DeleteNodeAsync(IFileSystemService fileSystemService, Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            // There's no need to make this asynchronous, nor have a status display associated with it.  
            var tcs = new TaskCompletionSource<object>();

            // Delete the physical object first. If we fail here, our node will survive.
            fileSystemService.DeleteFile(PhysicalPath);

            // Drop us from the parent list.
            // This will begin a chain reaction that will remove us from the UI.
            Parent.Children.Remove(this);
            Parent = null;

            return tcs.Task;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerFileNodeVm" /> class.
        /// </summary>
        public FileExplorerFileNodeVm()
        {            
        }
        #endregion
    }
}
