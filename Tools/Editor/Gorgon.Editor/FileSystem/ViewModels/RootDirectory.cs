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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The root directory for the editor virtual file system.
    /// </summary>
    internal class RootDirectory
        : ViewModelBase<RootDirectoryParameters>, IDirectory
    {
        #region Variables.
        // The root directory wrapped by the view model.
        private IGorgonVirtualDirectory _root;
        #endregion

        #region Properties.
        /// <summary>Property to return the ID for the directory.</summary>
        public string ID => "FFFFFFF1FFF1FFF1FFF1FFFFFFFFFFF1";

        /// <summary>Property to return the name for the node.</summary>
        public string Name => _root.Name;

        /// <summary>Property to return the full path to the node.</summary>
        public string FullPath => _root.FullPath;

        /// <summary>Property to return the physical path to the node.</summary>
        public string PhysicalPath => _root.MountPoint.PhysicalPath;

        /// <summary>Property to return the image name to use for the node type.</summary>
        public string ImageName => "folder_20x20.png";

        /// <summary>Property to return the directories that exist under the root directory.</summary>
        public ObservableCollection<IDirectory> Directories
        {
            get;
        } = new ObservableCollection<IDirectory>();

        /// <summary>Property to return the files that exist under the root directory.</summary>
        public ObservableCollection<IFile> Files
        {
            get;
        } = new ObservableCollection<IFile>();

        /// <summary>Property to return the actions that can be performed on this directory.</summary>
        public DirectoryActions AvailableActions => DirectoryActions.Delete | DirectoryActions.Copy;

        /// <summary>Property to return the parent of this directory.</summary>
        /// <remarks>If this value is <b>null</b>, then this directory is in the root of the file system.</remarks>
        public IDirectory Parent => null;

        /// <summary>
        /// Property to return the command to retrieve total size, in bytes, of all files in this directory, and any subdirectories under this directory.
        /// </summary>
        public IEditorCommand<GetSizeInBytesCommandArgs> GetSizeInBytesCommand
        {
            get;
        }

        /// <summary>Property to return the command to update all children of this directory if it's been renamed.</summary>
        /// <remarks>This directory can never be renamed, so this command will always be <b>null</b>.</remarks>
        public IEditorCommand<object> ParentRenamedCommand => null;

        /// <summary>Property to return the command to rename this directory.</summary>
        /// <remarks>This directory can never be renamed, so this command will always be <b>null</b>.</remarks>
        public IEditorCommand<RenameArgs> RenameCommand => null;

        /// <summary>Property to set or return a flag to indicate whether the directory was marked for a cut operation.</summary>
        public bool IsCut
        {
            get => false;
            set
            {
                // We cannot cut a directory.
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to calculate the total size, in bytes, for the files contained within this directory, and any sub directory within this directory.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoGetSizeInBytes(GetSizeInBytesCommandArgs args) => args.SizeInBytes = this.GetTotalByteCount(args.Recursive);

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(RootDirectoryParameters injectionParameters) => _root = injectionParameters.RootDirectory;
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="RootDirectory"/> class.</summary>
        public RootDirectory() => GetSizeInBytesCommand = new EditorCommand<GetSizeInBytesCommandArgs>(DoGetSizeInBytes);
        #endregion
    }
}
