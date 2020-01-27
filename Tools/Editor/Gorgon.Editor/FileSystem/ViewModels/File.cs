#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: December 7, 2019 11:55:10 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A virtual file for the <see cref="IFileExplorer"/> view model.
    /// </summary>
    internal class File
        : ViewModelBase<FileParameters>, IFile, IContentFile
    {
        #region Constants.
        // The name of the default icon.
        private const string DefaultIconName = "generic_file_20x20.png";
        #endregion

        #region Variables.
        // The virtual file wrapped by this view model.
        private IGorgonVirtualFile _file;
        // The parent directory.
        private IDirectory _parent;
        // The service used to display messages.
        private IMessageDisplayService _messages;
        // The name of the image used for the file icon.
        private string _imageIcon = DefaultIconName;
        // The project metadata for the file.
        private ProjectItemMetadata _metadata;
        // Flag to indicate whether the file is open.
        private bool _isOpen;
        // Flag to indicate that the file was marked for cutting.
        private bool _isCut;
        // File type.
        private string _type = Resources.GOREDIT_TEXT_UNKNOWN;
        #endregion

        #region Events.
        /// <summary>Event triggered if this content file was deleted.</summary>
        public event EventHandler Deleted;

        /// <summary>Event triggered if the content was closed with the <see cref="OLDE_IContentFile.CloseContent"/> method.</summary>
        public event EventHandler Closed;

        /// <summary>Event triggered if this content file was renamed.</summary>
        public event EventHandler<ContentFileRenamedEventArgs> Renamed;

        /// <summary>Event triggered if the dependencies list for this file is updated.</summary>
        public event EventHandler DependenciesUpdated;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of file.
        /// </summary>
        public string Type
        {
            get => _type;
            private set
            {
                if (string.Equals(_type, value, StringComparison.CurrentCulture))
                {
                    return;
                }

                OnPropertyChanging();
                _type = value;
                OnPropertyChanged();
            }
        }


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

        /// <summary>Property to return the parent directory for this file.</summary>
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

        /// <summary>Property to return the size of the file, in bytes.</summary>
        public long SizeInBytes => _file.Size;

        /// <summary>Property to return the full path to the file in the virtual file system.</summary>
        public string FullPath => _file.FullPath;

        /// <summary>Property to return the full path of the file on the physical file system.</summary>
        public string PhysicalPath => _file.PhysicalFile.FullPath;

        /// <summary>Property to return the extension name for the file.</summary>
        public string Extension => _file.Extension;

        /// <summary>Property to return the name of the file (without file extension).</summary>
        public string BaseName => _file.BaseFileName;

        /// <summary>Property to return the name of this object.</summary>
        /// <remarks>For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this
        /// property.</remarks>
        public string Name => _file.Name;

        /// <summary>Property to return the command that is executed when the parent directory is renamed.</summary>
        public IEditorCommand<object> ParentRenamedCommand
        {
            get;
        }

        /// <summary>Property to return the name of the image to use for the file icon.</summary>
        public string ImageName
        {
            get => _imageIcon;
            private set
            {
                if (string.Equals(_imageIcon, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _imageIcon = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the list of dependencies for this content.</summary>
        public IReadOnlyList<IContentFile> Dependencies
        {
            get;
        }

        /// <summary>Property to set or return whether the file has changes.</summary>
        public bool IsChanged
        {
            get;
            set;
        }

        /// <summary>Property to return the path to the file.</summary>
        string IContentFile.Path => FullPath;

        /// <summary>Property to return the plugin associated with the file.</summary>
        public ContentPlugIn ContentPlugIn
        {
            get;
        }

        /// <summary>Property to set or return the metadata associated with the file.</summary>
        public ProjectItemMetadata Metadata
        {
            get => _metadata;
            set
            {
                if (_metadata == value)
                {
                    return;
                }

                OnPropertyChanging();
                _metadata = value ?? new ProjectItemMetadata();
                RefreshMetadata();
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return whether the file is open for editing or not.</summary>
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isOpen = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the ID for the file.</summary>
        public string ID => Metadata?.ID ?? Guid.Empty.ToString("N");

        /// <summary>
        /// Property to return the command used to refresh the file data.
        /// </summary>
        public IEditorCommand<object> RefreshCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to rename this directory.
        /// </summary>
        public IEditorCommand<RenameArgs> RenameCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to refresh the data for the file.
        /// </summary>
        private void RefreshFileData()
        {
            _file.PhysicalFile.Refresh();
            NotifyPropertyChanged(nameof(SizeInBytes));
            RefreshMetadata();
        }

        /// <summary>
        /// Function called when the parent directory is renamed.
        /// </summary>
        private void DoParentRenamed()
        {
            try
            {
                NotifyPropertyChanged(nameof(FullPath));
                NotifyPropertyChanged(nameof(PhysicalPath));
            }
            catch (Exception ex)
            {
                _messages.ShowError(ex);
            }
        }

        /// <summary>
        /// Function to refresh the data for the file.
        /// </summary>
        private void DoRefreshFileData()
        {
            try
            {
                RefreshFileData();
            }
            catch (Exception ex)
            {
                _messages.ShowError(ex);
            }
        }

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
            }
            catch (Exception ex)
            {
                _messages.ShowError(ex);
            }
        }

        /// <summary>
        /// Function to refresh the metadata for the file.
        /// </summary>
        private void RefreshMetadata()
        {
            ImageName = Metadata?.ContentMetadata?.SmallIconID.ToString("N") ?? DefaultIconName;
            Type = Metadata?.ContentMetadata?.ContentType ?? Resources.GOREDIT_TEXT_UNKNOWN;
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(FileParameters injectionParameters)
        {
            _file = injectionParameters.VirtualFile;
            _messages = injectionParameters.MessageDisplay;
            Parent = injectionParameters.Parent;
            Metadata = injectionParameters.Metadata;

            RefreshMetadata();
        }

        void IContentFile.CloseContent() => throw new NotImplementedException();

        void IContentFile.LinkContent(IContentFile child) => throw new NotImplementedException();

        void IContentFile.UnlinkContent(IContentFile child) => throw new NotImplementedException();

        void IContentFile.ClearLinks() => throw new NotImplementedException();

        /// <summary>Function to notify that the metadata should be refreshed.</summary>
        void IContentFile.RefreshMetadata() => RefreshMetadata();

        void IContentFile.SaveMetadata() => throw new NotImplementedException();

        /// <summary>Function called to refresh the information about the file.</summary>
        void IContentFile.Refresh() => RefreshFileData();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="File"/> class.</summary>
        public File()
        {
            ParentRenamedCommand = new EditorCommand<object>(DoParentRenamed);
            RefreshCommand = new EditorCommand<object>(DoRefreshFileData);
            RenameCommand = new EditorCommand<RenameArgs>(DoRename);
        }
        #endregion
    }
}
