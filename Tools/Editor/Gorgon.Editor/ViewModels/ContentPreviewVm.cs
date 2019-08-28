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
// Created: December 23, 2018 2:15:44 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for the content previewer.
    /// </summary>
    internal class ContentPreviewVm
        : ViewModelBase<ContentPreviewVmParameters>, IContentPreviewVm
    {
        #region Variables.
        // The file explorer view model, used to track selection changes.
        private IFileExplorerVm _fileExplorer;
        // The file manager for content.
        private IContentFileManager _contentFileManager;
        // The image to display.
        private IGorgonImage _image;
        // The task used to load the preview.
        private Task<IGorgonImage> _loadPreviewTask;
        // The cancellation source.
        private CancellationTokenSource _cancelSource;
        // The title for the previewed content.
        private string _title = string.Empty;
        // The currently selected content file.
        private IContentFile _contentFile;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the thumbnail directory.
        /// </summary>
        public DirectoryInfo ThumbnailDirectory
        {
            get;
            private set;
        }

        /// <summary>Property to return the title for the previewed content.</summary>        
        public string Title
        {
            get => _title;
            private set
            {
                if (string.Equals(_title, value, StringComparison.CurrentCulture))
                {
                    return;
                }

                OnPropertyChanging();
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the preview image to display.</summary>
        public IGorgonImage PreviewImage
        {
            get => _image;
            private set
            {
                if (_image == value)
                {
                    return;
                }

                OnPropertyChanging();
                _image = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the command used to refresh the preview image.</summary>
        public IEditorAsyncCommand<IContentFile> RefreshPreviewCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load the preview thumbnail.
        /// </summary>
        /// <param name="file">The file </param>
        /// <returns></returns>
        private async Task LoadImagePreviewAsync(IContentFile file)
        {
            try
            {
                if (_loadPreviewTask != null)
                {
                    // Cancel the previous loading job.
                    _cancelSource.Cancel();

                    // Discard this image since we'll be replacing it.
                    (await _loadPreviewTask)?.Dispose();

                    _loadPreviewTask = null;
                }

                if (file?.Metadata?.ContentMetadata == null)
                {
                    Title = string.Empty;
                    PreviewImage?.Dispose();
                    PreviewImage = null;
                    _contentFile = file;
                    return;
                }

                if (!file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ThumbnailAttr, out string thumbnailName))
                {
                    thumbnailName = Guid.NewGuid().ToString("N");
                }

                var thumbNailFile = new FileInfo(Path.Combine(ThumbnailDirectory.FullName, thumbnailName));

                _cancelSource = new CancellationTokenSource();
                _loadPreviewTask = file.Metadata.ContentMetadata.GetThumbnailAsync(file, _contentFileManager, thumbNailFile, _cancelSource.Token);

                IGorgonImage image = await _loadPreviewTask;

                if (_cancelSource.Token.IsCancellationRequested)
                {
                    Title = string.Empty;
                    image?.Dispose();
                    PreviewImage?.Dispose();
                    PreviewImage = null;
                    _contentFile = file;
                    return;
                }

                PreviewImage?.Dispose();
                PreviewImage = image;
                Title = file.Name;

                _contentFile = file;
                _contentFile.Renamed += File_Renamed;
            }
            catch (Exception ex)
            {
                Program.Log.Print($"[ERROR] Error loading thumbnail for '{file.Path}'.", LoggingLevel.Simple);
                Program.Log.LogException(ex);
            }
        }

        /// <summary>Handles the PropertyChanged event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private async void FileExplorer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorerVm.SelectedNode):
                    var file = _fileExplorer.SelectedNode as IContentFile;
                    await LoadImagePreviewAsync(file);
                    break;
            }
        }

        /// <summary>Handles the PropertyChanging event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void FileExplorer_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorerVm.SelectedNode):
                    if (_contentFile != null)
                    {
                        _contentFile.Renamed -= File_Renamed;
                    }
                    break;
            }
        }

        /// <summary>
        /// Function called when a file is renamed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void File_Renamed(object sender, ContentFileRenamedEventArgs e) => Title = e.NewName;

        /// <summary>
        /// Function to refresh the preview image for the content file.
        /// </summary>
        /// <param name="file">The file to refresh the preview for.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoRefreshPreview(IContentFile file)
        {
            try
            {
                // If the file already has a link to a thumbnail, remove it.
                FileInfo thumbnailFile = null;
                if (file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ThumbnailAttr, out string thumbnailName))
                {
                    thumbnailFile = new FileInfo(Path.Combine(ThumbnailDirectory.FullName, thumbnailName));

                    if (thumbnailFile.Exists)
                    {
                        thumbnailFile.Delete();
                    }

                    file.Metadata.Attributes.Remove(thumbnailName);
                }

                // If our currently selected file is not the one being dispayed at this moment, then do not update the render window.
                if (file != _contentFile)
                {
                    return;
                }

                // Reload.
                await LoadImagePreviewAsync(file);
            }
            catch (Exception ex)
            {
                Log.Print($"Error refreshing the preview image for '{file.Path}'.", LoggingLevel.Simple);
                Log.LogException(ex);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(ContentPreviewVmParameters injectionParameters)
        {
            _fileExplorer = injectionParameters.FileExplorer ?? throw new ArgumentMissingException(nameof(ContentPreviewVmParameters.FileExplorer), nameof(injectionParameters));
            _contentFileManager = injectionParameters.ContentFileManager ?? throw new ArgumentMissingException(nameof(ContentPreviewVmParameters.ContentFileManager), nameof(injectionParameters));
            ThumbnailDirectory = injectionParameters.ThumbDirectory ?? throw new ArgumentMissingException(nameof(ContentPreviewVmParameters.ThumbDirectory), nameof(injectionParameters));
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        public override void OnLoad()
        {
            _fileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
            _fileExplorer.PropertyChanging += FileExplorer_PropertyChanging;
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            if (_contentFile != null)
            {
                _contentFile.Renamed -= File_Renamed;
            }
            _fileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
            _fileExplorer.PropertyChanging -= FileExplorer_PropertyChanging;
            _cancelSource?.Dispose();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ViewModels.ContentPreviewVm"/> class.</summary>
        public ContentPreviewVm() => RefreshPreviewCommand = new EditorAsyncCommand<IContentFile>(DoRefreshPreview);
        #endregion
    }
}
