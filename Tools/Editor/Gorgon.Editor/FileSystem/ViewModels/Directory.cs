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
using System.Collections.ObjectModel;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system directory.
    /// </summary>
    internal class Directory
        : ViewModelBase<DirectoryParameters, IHostServices>, IDirectory, IExcludable
    {
        #region Variables.
        // The directory wrapped by the view model.
        private IGorgonVirtualDirectory _directory;
        // The physical directory represented by this view model.
        private string _physicalPath;
        // The parent directory for this directory.
        private IDirectory _parent;
        // Flag to indicate whether the directory is marked for a cut operation.
        private bool _isCut;
        // Flag to indicate that the directory is excluded from a packed file system.
        private bool _isExcluded;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return a flag to indicate whether the directory was marked for a cut operation.
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

        /// <summary>Property to return the ID for the directory.</summary>
        public string ID
        {
            get;
        }

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        public string FullPath => _directory.FullPath;

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        public string ImageName => "folder_20x20.png";

        /// <summary>Property to return the physical path to the node.</summary>
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

        /// <summary>Property to return the actions that can be performed on this directory.</summary>
        public DirectoryActions AvailableActions => DirectoryActions.Copy | DirectoryActions.Move | DirectoryActions.Rename | DirectoryActions.Delete | DirectoryActions.ExcludeFromPackedFile;

        /// <summary>Property to return the directories that exist under the root directory.</summary>
        public ObservableCollection<IDirectory> Directories
        {
            get;
        } = new ObservableCollection<IDirectory>();

        /// <summary>Property to return the files that exist under the this directory.</summary>
        public ObservableCollection<IFile> Files
        {
            get;
        } = new ObservableCollection<IFile>();

        /// <summary>Property to return the parent of this directory.</summary>
        public IDirectory Parent
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
        /// Property to return the command to retrieve total size, in bytes, of all files in this directory, and any subdirectories under this directory.
        /// </summary>
        public IEditorCommand<GetSizeInBytesCommandArgs> GetSizeInBytesCommand
        {
            get;
        }

        /// <summary>Property to return the name of this object.</summary>
        /// <remarks>For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this
        /// property.</remarks>
        public string Name => _directory.Name;

        /// <summary>Property to return the command to update all children of this directory if it's been renamed.</summary>
        public IEditorCommand<object> ParentRenamedCommand
        {
            get;
        }

        /// <summary>Property to return the command to rename this directory.</summary>
        public IEditorCommand<RenameArgs> RenameCommand
        {
            get;
        }

        /// <summary>Property to set or return a flag to indicate that the directory can be excluded from a packed file system.</summary>
        public bool IsExcluded
        {
            get => ((Parent is IExcludable parent) && (parent.IsExcluded)) || _isExcluded;
            set
            {
                if (_isExcluded == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isExcluded = value;
                OnPropertyChanged();

                foreach (IExcludable child in Directories.Traverse(d => d.Directories).OfType<IExcludable>())
                {
                    child.IsExcluded = value;
                }
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to calculate the total size, in bytes, for the files contained within this directory, and any sub directory within this directory.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoGetSizeInBytes(GetSizeInBytesCommandArgs args) => args.SizeInBytes = this.GetTotalByteCount(args.Recursive);

        /// <summary>
        /// Function to notify this directory that its parent has been renamed.
        /// </summary>
        private void DoParentRenamed()
        {
            try
            {
                NotifyPropertyChanged(nameof(FullPath));
                NotifyPropertyChanged(nameof(PhysicalPath));

                foreach (IFile file in Files)
                {
                    if ((file.ParentRenamedCommand is not null) && (file.ParentRenamedCommand.CanExecute(null)))
                    {
                        file.ParentRenamedCommand.Execute(null);
                    }
                }

                foreach (IDirectory directory in Directories)
                {
                    if ((directory.ParentRenamedCommand is not null) && (directory.ParentRenamedCommand.CanExecute(null)))
                    {
                        directory.ParentRenamedCommand.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex);
            }
        }

        /// <summary>
        /// Function to determine if the directory can be renamed or not.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the directory can be renamed, <b>false</b> if not.</returns>
        private bool CanRename(RenameArgs args) => (AvailableActions & DirectoryActions.Rename) == DirectoryActions.Rename;

        /// <summary>
        /// Function to rename the directory.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoRename(RenameArgs args)
        {
            try
            {
                NotifyPropertyChanged(nameof(Name));
                NotifyPropertyChanged(nameof(FullPath));
                NotifyPropertyChanged(nameof(PhysicalPath));

                foreach (IDirectory subDir in Directories)
                {
                    if ((subDir.ParentRenamedCommand is not null) && (subDir.ParentRenamedCommand.CanExecute(null)))
                    {
                        subDir.ParentRenamedCommand.Execute(null);
                    }
                }

                foreach (IFile file in Files)
                {
                    if ((file.ParentRenamedCommand is not null) && (file.ParentRenamedCommand.CanExecute(null)))
                    {
                        file.ParentRenamedCommand.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(DirectoryParameters injectionParameters)
        {
            _directory = injectionParameters.VirtualDirectory;
            Parent = injectionParameters.Parent;
            PhysicalPath = injectionParameters.PhysicalPath;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Directory" /> class.
        /// </summary>
        public Directory()
        {
            ID =  Guid.NewGuid().ToString("N");
            GetSizeInBytesCommand = new EditorCommand<GetSizeInBytesCommandArgs>(DoGetSizeInBytes);
            ParentRenamedCommand = new EditorCommand<object>(DoParentRenamed);
            RenameCommand = new EditorCommand<RenameArgs>(DoRename, CanRename);
        }
        #endregion
    }
}
