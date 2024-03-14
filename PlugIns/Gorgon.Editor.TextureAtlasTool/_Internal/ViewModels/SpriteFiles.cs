
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 7, 2019 11:50:04 AM
// 


using System.ComponentModel;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The sprite file list view model
/// </summary>
internal class SpriteFiles
    : ViewModelBase<SpriteFilesParameters, IHostContentServices>, ISpriteFiles
{

    // The directory path for thumbnails this session.
    private const string ThumbnailPath = "/Thumbnails/";



    // The service used to search through the files.
    private ISearchService<IContentFileExplorerSearchEntry> _searchService;
    // The list of selected files.
    private readonly List<ContentFileExplorerFileEntry> _selected = [];
    // The task used to load the preview.
    private Task<IGorgonImage> _loadPreviewTask;
    // The cancellation source.
    private CancellationTokenSource _cancelSource;
    // The image used for preview.
    private IGorgonImage _previewImage;
    // The file system used for writing temporary data.
    private IGorgonFileSystemWriter<Stream> _tempFileSystem;



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
            value ??= [];

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
    public IGorgonImage PreviewImage
    {
        get => _previewImage;
        private set
        {
            if (_previewImage == value)
            {
                return;
            }

            OnPropertyChanging();
            _previewImage = value;
            OnPropertyChanged();
        }
    }



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
            IGorgonVirtualDirectory thumbDirectory = _tempFileSystem.FileSystem.GetDirectory(ThumbnailPath);

            thumbDirectory ??= _tempFileSystem.CreateDirectory(ThumbnailPath);

            if (_loadPreviewTask is not null)
            {
                if (_loadPreviewTask.Status == TaskStatus.Faulted)
                {
                    PreviewImage?.Dispose();
                    PreviewImage = null;
                    _loadPreviewTask = null;
                    return;
                }

                _cancelSource?.Cancel();
                (await _loadPreviewTask)?.Dispose();
                _loadPreviewTask = null;
            }

            PreviewImage?.Dispose();

            if ((files is null) || (files.Count == 0))
            {
                PreviewImage = null;
                return;
            }

            IContentFile previewFile = files[files.Count - 1]?.File;

            if (previewFile?.Metadata?.ContentMetadata is null)
            {
                PreviewImage = null;
                return;
            }

            // If the file already has a link to a thumbnail, remove it.
            string thumbnailName = previewFile.Metadata.Thumbnail;
            if (string.IsNullOrWhiteSpace(thumbnailName))
            {
                thumbnailName = Guid.NewGuid().ToString("N");
            }

            string filePath = thumbDirectory.FullPath + thumbnailName;
            _cancelSource = new CancellationTokenSource();
            _loadPreviewTask = previewFile.Metadata.ContentMetadata.GetThumbnailAsync(previewFile, filePath, _cancelSource.Token);
            IGorgonImage image = await _loadPreviewTask;

            if (_cancelSource.IsCancellationRequested)
            {
                image?.Dispose();
                _loadPreviewTask = null;
                PreviewImage = null;
                return;
            }

            PreviewImage = image;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("ERROR: There was an error generating the sprite preview data.", LoggingLevel.Simple);
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
            HostServices.MessageDisplay.ShowError(ex, Resources.GORTAG_ERR_SEARCH);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(SpriteFilesParameters injectionParameters)
    {
        _tempFileSystem = injectionParameters.TempFileSystem;
        _searchService = injectionParameters.SearchService;
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

        _cancelSource?.Cancel();

        IGorgonImage currentImage = Interlocked.Exchange(ref _previewImage, null);
        Interlocked.Exchange(ref _loadPreviewTask, null);
        currentImage?.Dispose();

        base.OnUnload();
    }



    /// <summary>Initializes a new instance of the <see cref="SpriteFiles"/> class.</summary>
    public SpriteFiles()
    {
        SearchCommand = new EditorCommand<string>(DoSearch, CanSearch);
        RefreshSpritePreviewCommand = new EditorAsyncCommand<IReadOnlyList<ContentFileExplorerFileEntry>>(DoRefreshPreviewAsync);
    }

}
