
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: December 23, 2018 2:15:44 PM
// 


using System.Collections.Specialized;
using System.ComponentModel;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The view model for the content previewer
/// </summary>
internal class ContentPreview
    : ViewModelBase<ContentPreviewParameters, IHostServices>, IContentPreview
{

    // The directory path for thumbnails this session.
    private readonly static string _thumbnailPath = $"/Thumbnails/";
    // The file explorer view model, used to track selection changes.
    private IFileExplorer _fileExplorer;
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
    // The file system for handling temporary file data.
    private IGorgonFileSystemWriter<Stream> _tempFileSystem;
    // Flag to indicate that file events have been assigned.
    private int _fileEventsHooked;
    // Flag to enable or disable the preview.
    private bool _isEnabled = true;
    // Flag to force a refresh.
    private bool _forceRefresh;
    // Flag to indicate whether the preview is loading or not.
    private bool _loading;



    /// <summary>
    /// Property to return the task used for loading the image preview.
    /// </summary>
    public Task LoadingTask => _loadPreviewTask;

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

    /// <summary>
    /// Property to return whether the preview is loading or not.
    /// </summary>
    public bool IsLoading
    {
        get => _loading;
        private set
        {
            if (_loading == value)
            {
                return;
            }

            OnPropertyChanging();
            _loading = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the command used to refresh the preview image.</summary>
    public IEditorAsyncCommand<string> RefreshPreviewCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to reset the preview back to its inital state.
    /// </summary>
    public IEditorAsyncCommand<object> ResetPreviewCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether the preview is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
            {
                return;
            }

            OnPropertyChanging();
            _isEnabled = value;
            OnPropertyChanged();
        }
    }



    /// <summary>
    /// Function to load the preview thumbnail.
    /// </summary>
    /// <param name="file">The file </param>
    /// <returns></returns>
    private async Task LoadImagePreviewAsync(IContentFile file)
    {
        try
        {
            if (!_isEnabled)
            {
                PreviewImage?.Dispose();
                PreviewImage = null;
                _contentFile = null;
                return;
            }

            if (_loadPreviewTask is not null)
            {
                if (_loadPreviewTask.Status == TaskStatus.Faulted)
                {
                    _loadPreviewTask = null;
                    Title = string.Empty;
                    PreviewImage?.Dispose();
                    PreviewImage = null;
                    _contentFile = file;
                    return;
                }

                // Cancel the previous loading job.
                _cancelSource?.Cancel();

                // Discard this image since we'll be replacing it.
                (await _loadPreviewTask)?.Dispose();

                _loadPreviewTask = null;
            }

            if (file?.Metadata?.ContentMetadata is null)
            {
                Title = string.Empty;
                PreviewImage?.Dispose();
                PreviewImage = null;
                _contentFile = file;
                return;
            }

            string thumbnailName = file.Metadata.Thumbnail;
            if (string.IsNullOrWhiteSpace(thumbnailName))
            {
                thumbnailName = Guid.NewGuid().ToString("N");
            }

            string path = _thumbnailPath + thumbnailName;

            IsLoading = true;
            Title = file.Name;
            _cancelSource = new CancellationTokenSource();
            _loadPreviewTask = file.Metadata.ContentMetadata.GetThumbnailAsync(file, path, _cancelSource.Token);

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

            _contentFile = file;
        }
        catch (Exception ex)
        {
            Title = Resources.GOREDIT_ERR_ERROR;
            HostServices.Log.Print($"ERROR: Error loading thumbnail for '{file.Path}'.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
        finally
        {

            IsLoading = false;
        }
    }

    /// <summary>Handles the PropertyChanged event of the File control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        IFile file = (IFile)sender;

        switch (e.PropertyName)
        {
            case nameof(IFile.Name):
                Title = file.Name;
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the FileExplorer control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private async void FileExplorer_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFileExplorer.SelectedFiles):
                IContentFile file = null;

                if (_fileExplorer.SelectedFiles.Count > 0)
                {
                    file = _contentFileManager.GetFile(_fileExplorer.SelectedFiles[^1].FullPath);
                }

                _fileExplorer.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
                await LoadImagePreviewAsync(file);
                break;
            case nameof(IFileExplorer.SelectedDirectory):
                HookFileEvents();
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
            case nameof(IFileExplorer.SelectedFiles):
                _fileExplorer.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
                break;
            case nameof(IFileExplorer.SelectedDirectory):
                UnhookFileEvents();
                break;
        }
    }

    /// <summary>Handles the CollectionChanged event of the SelectedFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private async void SelectedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        IContentFile contentFile = null;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                contentFile = (IContentFile)e.NewItems[0];

                if (contentFile == _contentFile)
                {
                    return;
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems[0] != _contentFile)
                {
                    return;
                }

                contentFile = null;
                break;
        }

        await LoadImagePreviewAsync(contentFile);
    }

    /// <summary>
    /// Function to refresh the preview image for the content file.
    /// </summary>
    /// <param name="file">The file to refresh the preview for.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoRefreshPreview(string filePath)
    {
        try
        {
            IContentFile file = _contentFileManager.GetFile(filePath);
            IGorgonVirtualDirectory thumbDirectory = _tempFileSystem.FileSystem.GetDirectory(_thumbnailPath);

            thumbDirectory ??= _tempFileSystem.CreateDirectory(_thumbnailPath);

            // If the file already has a link to a thumbnail, remove it.
            string thumbnailName = file.Metadata.Thumbnail;
            if (!string.IsNullOrWhiteSpace(thumbnailName))
            {
                IGorgonVirtualFile thumbnailFile = _tempFileSystem.FileSystem.GetFile(thumbDirectory.FullPath + thumbnailName);

                if (thumbnailFile is not null)
                {
                    _tempFileSystem.DeleteFile(thumbnailFile.FullPath);
                }

                file.Metadata.Thumbnail = null;
            }

            // If our currently selected file is not the one being dispayed at this moment, then do not update the render window.
            if ((file != _contentFile) && (!_forceRefresh))
            {
                return;
            }

            // Reload.
            await LoadImagePreviewAsync(file);

            _forceRefresh = false;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print($"Error refreshing the preview image for '{filePath}'.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to reset the preview image back to its initial state.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoResetPreview()
    {
        try
        {
            await LoadImagePreviewAsync(null);
            _forceRefresh = true;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print($"Error resetting the preview image.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to unhook file events when a new file list is shown.
    /// </summary>
    private void UnhookFileEvents()
    {
        IDirectory directory = _fileExplorer.SelectedDirectory;

        if (directory is null)
        {
            return;
        }

        foreach (IFile file in directory.Files)
        {
            file.PropertyChanged -= File_PropertyChanged;
        }

        Interlocked.Exchange(ref _fileEventsHooked, 0);
    }

    /// <summary>
    /// Function to hook file events when a new file list is shown.
    /// </summary>
    private void HookFileEvents()
    {
        if (Interlocked.Exchange(ref _fileEventsHooked, 1) == 1)
        {
            return;
        }

        IDirectory directory = _fileExplorer.SelectedDirectory;

        if (directory is null)
        {
            return;
        }

        foreach (IFile file in directory.Files)
        {
            file.PropertyChanged += File_PropertyChanged;
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(ContentPreviewParameters injectionParameters)
    {
        _fileExplorer = injectionParameters.FileExplorer;
        _contentFileManager = injectionParameters.ContentFileManager;
        _tempFileSystem = injectionParameters.TempFileSystem;
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    protected async override void OnLoad()
    {
        _fileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
        _fileExplorer.PropertyChanging += FileExplorer_PropertyChanging;
        _fileExplorer.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
        HookFileEvents();

        if (_fileExplorer.SelectedFiles.Count == 0)
        {
            return;
        }

        string path = _fileExplorer.SelectedFiles[0].FullPath;

        await LoadImagePreviewAsync(_contentFileManager.GetFile(path));
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    protected override void OnUnload()
    {
        _cancelSource?.Cancel();

        UnhookFileEvents();
        _fileExplorer.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
        _fileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
        _fileExplorer.PropertyChanging -= FileExplorer_PropertyChanging;
    }



    /// <summary>Initializes a new instance of the <see cref="ContentPreview"/> class.</summary>
    public ContentPreview()
    {
        RefreshPreviewCommand = new EditorAsyncCommand<string>(DoRefreshPreview);
        ResetPreviewCommand = new EditorAsyncCommand<object>(DoResetPreview);
    }

}
