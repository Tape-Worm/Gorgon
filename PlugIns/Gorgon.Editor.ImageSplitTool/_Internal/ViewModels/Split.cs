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
// Created: May 7, 2019 11:50:04 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageSplitTool.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.Tools;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;

namespace Gorgon.Editor.ImageSplitTool;

/// <summary>
/// The image file list view model.
/// </summary>
internal class ImageSelection
    : ViewModelBase<SplitParameters, IHostContentServices>, ISplit
{
    #region Constants.
    // The directory path for thumbnails this session.
    private const string ThumbnailPath = "/Thumbnails/";
    #endregion

    #region Variables.
    // The service used to search through the files.
    private ISearchService<IContentFileExplorerSearchEntry> _searchService;
    // The list of selected files.
    private readonly List<ContentFileExplorerFileEntry> _selected = new();
    // The task used to load the preview.
    private Task<IGorgonImage> _loadPreviewTask;
    // The cancellation source.
    private CancellationTokenSource _previewCancelSource;
    // The task used for splitting the file.
    private Task _splitTask;
    // The cancellation token source for splitting the file.
    private CancellationTokenSource _splitCancelSource;
    // The image used for preview.
    private IGorgonImage _previewImage;
    // The file system used for writing temporary data.
    private IGorgonFileSystemWriter<Stream> _tempFileSystem;
    // Plug in settings.
    private ImageSplitToolSettings _settings;
    // The content file manager for the host application.
    private IContentFileManager _fileManager;
    // The service used to split up the image.
    private TextureAtlasSplitter _splitService;
    // The progress for the splitting operation.
    private string _progress = string.Empty;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the progress for the splitting operation.
    /// </summary>
    public string Progress
    {
        get => _progress;
        private set
        {
            if (string.Equals(_progress, value, StringComparison.CurrentCulture))
            {
                return;
            }

            OnPropertyChanging();
            _progress = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the output directory for the resulting images/sprites.
    /// </summary>
    public string OutputDirectory
    {
        get => _settings.LastOutputDir;
        set
        {
            if (value is null)
            {
                value = string.Empty;
            }

            if (string.Equals(_settings.LastOutputDir, value, StringComparison.Ordinal))
            {
                return;
            }

            OnPropertyChanging();
            _settings.LastOutputDir = value.FormatDirectory('/');
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the sprite file entries.
    /// </summary>
    public IReadOnlyList<ContentFileExplorerDirectoryEntry> SpriteFileEntries
    {
        get;
        private set;
    }

    /// <summary>Property to return the list of selected files.</summary>
    public IReadOnlyList<ContentFileExplorerFileEntry> SelectedFiles
    {
        get => _selected;
        private set
        {
            if (value is null)
            {
                value = Array.Empty<ContentFileExplorerFileEntry>();
            }

            if (value.SequenceEqual(_selected))
            {
                return;
            }

            OnPropertyChanging();
            _selected.Clear();
            _selected.AddRange(value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the command that will refresh the sprite preview data.
    /// </summary>
    public IEditorAsyncCommand<IReadOnlyList<ContentFileExplorerFileEntry>> RefreshSpritePreviewCommand
    {
        get;
    }

    /// <summary>Property to return the command used to search through the file list.</summary>
    public IEditorCommand<string> SearchCommand
    {
        get;
    }

    /// <summary>Property to set or return the command used confirm loading of the sprite files.</summary>
    public IEditorCommand<object> ConfirmLoadCommand
    {
        get;
        set;
    }

    /// <summary>Property to return the image used for previewing.</summary>
    public IGorgonImage PreviewImage => _previewImage;

    /// <summary>Property to return the folder selection command.</summary>
    public IEditorCommand<object> SelectFolderCommand
    {
        get;
    }

    /// <summary>Property to return the command used when the tool UI is about to be closed.</summary>
    /// <remarks>This command will be executed when the user shuts down the tool via some UI element (e.g. the 'X' in a window).</remarks>
    public IEditorAsyncCommand<CloseToolArgs> CloseToolCommand
    {
        get;
    }

    /// <summary>
    /// Function to split the images and write them to the file system.
    /// </summary>
    public IEditorAsyncCommand<object> SplitImageCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to cancel the operation.
    /// </summary>
    public IEditorAsyncCommand<CancelEventArgs> CancelCommand
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the PropertyChanged event of the File control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var entry = (ContentFileExplorerFileEntry)sender;

        switch (e.PropertyName)
        {
            case nameof(ContentFileExplorerFileEntry.IsSelected):
                if (entry.IsSelected)
                {
                    if (_selected.Contains(entry))
                    {
                        break;
                    }

                    _selected.Add(entry);
                }
                else
                {
                    _selected.Remove(entry);
                }

                NotifyPropertyChanged(nameof(SelectedFiles));
                break;
        }
    }

    /// <summary>
    /// Function called to refresh the sprite file preview data.
    /// </summary>
    /// <param name="files">The files that are focused in the UI.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoRefreshPreviewAsync(IReadOnlyList<ContentFileExplorerFileEntry> files)
    {
        try
        {
            NotifyPropertyChanging(nameof(PreviewImage));

            IGorgonVirtualDirectory thumbDirectory = _tempFileSystem.FileSystem.GetDirectory(ThumbnailPath);

            if (thumbDirectory is null)
            {
                thumbDirectory = _tempFileSystem.CreateDirectory(ThumbnailPath);
            }

            if (_loadPreviewTask is not null)
            {
                if (_loadPreviewTask.Status == TaskStatus.Faulted)
                {
                    _previewImage?.Dispose();
                    _previewImage = null;
                    _loadPreviewTask = null;

                    NotifyPropertyChanged(nameof(PreviewImage));
                    return;
                }

                _previewCancelSource?.Cancel();
                (await _loadPreviewTask)?.Dispose();
                _loadPreviewTask = null;                    
            }

            _previewImage?.Dispose();                

            if ((files is null) || (files.Count == 0))
            {
                _previewImage = null;
                NotifyPropertyChanged(nameof(PreviewImage));
                return;
            }

            IContentFile previewFile = files[files.Count - 1]?.File;

            if (previewFile is null)
            {
                _previewImage = null;
                NotifyPropertyChanged(nameof(PreviewImage));
                return;
            }

            // If the file already has a link to a thumbnail, remove it.
            string thumbnailName = previewFile.Metadata.Thumbnail;
            if (string.IsNullOrWhiteSpace(thumbnailName))
            {
                thumbnailName = Guid.NewGuid().ToString("N");
            }

            string filePath = thumbDirectory.FullPath + thumbnailName;
            _previewCancelSource = new CancellationTokenSource();
            _loadPreviewTask = previewFile.Metadata.ContentMetadata.GetThumbnailAsync(previewFile, filePath, _previewCancelSource.Token);
            IGorgonImage image = await _loadPreviewTask;

            if (_previewCancelSource.IsCancellationRequested)
            {
                image?.Dispose();
                _loadPreviewTask = null;
                _previewImage = null;
            }
            else
            {
                _previewImage = image;
            }

            NotifyPropertyChanged(nameof(PreviewImage));
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("ERROR: There was an error generating the image preview data.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if search is available or not.
    /// </summary>
    /// <param name="text">The search text, not used.</param>
    /// <returns><b>true</b> if the search is available, <b>false</b> if not.</returns>
    private bool CanSearch(string text) => SpriteFileEntries.Count > 0;

    /// <summary>
    /// Function to search the sprite files.
    /// </summary>
    /// <param name="text">The search term.</param>
    private void DoSearch(string text)
    {
        HostServices.BusyService.SetBusy();
        
        try
        {
            IEnumerable<IContentFileExplorerSearchEntry> results = _searchService.Search(text);

            for (int i = 0; i < SpriteFileEntries.Count; ++i)
            {
                ContentFileExplorerDirectoryEntry entry = SpriteFileEntries[i];

                if (string.IsNullOrWhiteSpace(text))
                {
                    entry.IsVisible = true;

                    for (int j = 0; j < entry.Files.Count; ++j)
                    {
                        ContentFileExplorerFileEntry fileEntry = entry.Files[j];
                        fileEntry.IsVisible = true;
                    }
                    continue;
                }

                entry.IsVisible = results.Any(item => item == entry);

                for (int j = 0; j < entry.Files.Count; ++j)
                {
                    ContentFileExplorerFileEntry fileEntry = entry.Files[j];

                    bool found = results.Any(item => item == fileEntry);
                    fileEntry.IsVisible = found;
                    if (found)
                    {
                        fileEntry.Parent.IsVisible = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIST_ERR_SEARCH);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to browse folders on the file system.
    /// </summary>
    private void DoBrowseFolders()
    {
        try
        {
            string outputDir = OutputDirectory;
            
            if (!_fileManager.DirectoryExists(outputDir))
            {
                outputDir = "/";
            }

            outputDir = HostServices.FolderBrowser.GetFolderPath(outputDir, Resources.GORIST_CAPTION_FOLDER_SELECT, Resources.GORIST_DESC_FOLDER_SELECT);

            if (string.IsNullOrWhiteSpace(outputDir))
            {
                return;
            }

            OutputDirectory = outputDir;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIST_ERR_FOLDER_SELECT);
        }
    }

    /// <summary>
    /// Function to determine if the image can be split.
    /// </summary>
    /// <returns><b>true</b> if the image can be split, <b>false</b> if not.</returns>
    private bool CanSplit() => (!string.IsNullOrEmpty(OutputDirectory)) && (SelectedFiles.Count > 0);

    /// <summary>
    /// Function to split the image into smaller images.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSplitAsync()
    {
        try
        {
            _splitCancelSource = new CancellationTokenSource();
            _splitTask = _splitService.SplitAsync(_selected, _settings.LastOutputDir, fileName => Progress = fileName, _splitCancelSource.Token);
            await _splitTask;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORIST_ERR_SPLIT);
        }
        finally
        {
            _splitTask = null;
            _splitCancelSource = null;
        }
    }

    /// <summary>
    /// Function to cancel the operation.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoCancelAsync(CancelEventArgs args)
    {
        try
        {
            if (_splitCancelSource is null)
            {
                return;
            }

            _splitCancelSource?.Cancel();

            if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORIST_CONFIRM_CLOSE) == MessageResponse.No)
            {
                args.Cancel = true;
            }

            if (_splitTask is not null)
            {
                await _splitTask;
            }

            _splitCancelSource = null;
            _splitTask = null;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("ERROR: There was an error cancelling the splitting operation.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(SplitParameters injectionParameters)
    {
        _settings = injectionParameters.Settings;
        _tempFileSystem = injectionParameters.TempFileSystem;
        _fileManager = injectionParameters.FileManager;
        _searchService = injectionParameters.SearchService;
        _splitService = injectionParameters.TextureSplitService;
        
        SpriteFileEntries = injectionParameters.Entries;

        _selected.Clear();
        _selected.AddRange(injectionParameters.Entries.SelectMany(item => item.Files).Where(item => item.IsSelected));
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        foreach (ContentFileExplorerFileEntry file in SpriteFileEntries.SelectMany(item => item.Files))
        {
            file.PropertyChanged += File_PropertyChanged;
        }
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    protected override void OnUnload()
    {
        foreach (ContentFileExplorerFileEntry file in SpriteFileEntries.SelectMany(item => item.Files))
        {
            file.PropertyChanged -= File_PropertyChanged;
        }

        if (_splitCancelSource is not null)
        {
            _splitCancelSource.Cancel();
        }

        if (_previewCancelSource is not null)
        {
            _previewCancelSource.Cancel();
        }

        IGorgonImage currentImage = Interlocked.Exchange(ref _previewImage, null);
        Interlocked.Exchange(ref _loadPreviewTask, null);
        Interlocked.Exchange(ref _splitTask, null);
        currentImage?.Dispose();

        base.OnUnload();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="ImageSelection"/> class.</summary>
    public ImageSelection()
    {
        SearchCommand = new EditorCommand<string>(DoSearch, CanSearch);
        RefreshSpritePreviewCommand = new EditorAsyncCommand<IReadOnlyList<ContentFileExplorerFileEntry>>(DoRefreshPreviewAsync);
        SelectFolderCommand = new EditorCommand<object>(DoBrowseFolders);
        SplitImageCommand = new EditorAsyncCommand<object>(DoSplitAsync, CanSplit);
        SearchCommand = new EditorCommand<string>(DoSearch, CanSearch);
        CancelCommand = new EditorAsyncCommand<CancelEventArgs>(DoCancelAsync);
    }
    #endregion
}
