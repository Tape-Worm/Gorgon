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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system directory.
    /// </summary>
    internal class FileExplorerDirectoryNodeVm
        : ViewModelBase, IFileExplorerNodeVm
    {
        #region Variables.
        // The parent for this node.
        private IFileExplorerNodeVm _parent;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // The project data.
        private IProject _project;
        // Flag to indicate that this node is included in the project.
        private bool _included;
        // The name of the file.
        private string _name;
        // The full path to the file.
        private string _fullPath;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public bool AllowChildCreation => true;

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
        public NodeType NodeType => NodeType.Directory;

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
        public string FullPath
        {
            get => _fullPath;
            private set
            {
                if (string.Equals(value, _fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _fullPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        public string ImageName => "folder_20x20.png";

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public bool AllowDelete => true;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to delete the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when deleting.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService" /> parameter is <b>null</b>.</exception>        
        public async Task DeleteNodeAsync(IFileSystemService fileSystemService)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            // Delete the physical object first. If we fail here, our node will survive.
            // We do this asynchronously because deleting a directory with a lot of files may take a while.
            await Task.Run(() => fileSystemService.DeleteDirectory(FullPath));

            // Drop us from the parent list.
            // This will begin a chain reaction that will remove us from the UI.
            Parent.Children.Remove(this);
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

            FullPath = fileSystemService.RenameDirectory(FullPath, newName);
            Name = newName;
        }

        /// <summary>
        /// Function used to initialize the view model.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="directory">The virtual file system directory to use.</param>
        /// <param name="parent">The parent for this node.</param>
        /// <param name="children">The child nodes for this directory.</param>
        /// <param name="messageService">The message display service to use.</param>
        /// <param name="busyService">The busy state service to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="directory"/>, <paramref name="parent"/>, <paramref name="messageService"/>, or the <paramref name="busyService"/> parameter is <b>null</b>.</exception>
        public void Initialize(IProject project,
                               DirectoryInfo directory,
                               IFileExplorerNodeVm parent,
                               ObservableCollection<IFileExplorerNodeVm> children,
                               IMessageDisplayService messageService,
                               IBusyStateService busyService)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _name = directory?.Name ?? throw new ArgumentNullException(nameof(directory));
            _fullPath = directory.ToFileSystemPath(project.ProjectWorkSpace);
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            Children = children ?? new ObservableCollection<IFileExplorerNodeVm>();
            // Determine if we are included in the project or not.
            _included = project.Metadata.IncludedPaths.Any(item => string.Equals(item.Path, FullPath, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
