
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: May 25, 2020 9:52:13 PM
// 


using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ExtractSpriteTool.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ExtractSpriteTool;

/// <summary>
/// The extraction UI view model
/// </summary>
internal class Extract
    : EditorToolViewModelBase<ExtractParameters>, IExtract
{

    // The data used for extraction.
    private SpriteExtractionData _extractData;
    // The settings for the plug in.
    private ExtractSpriteToolSettings _settings;
    // The service used to create the sprites.
    private ISpriteExtractorService _extractor;
    // The progress data for the extraction task.
    private ProgressData _extractProgressData;
    // The cancellation token source for cancelling tasks.
    private CancellationTokenSource _cancelSource;
    // The list of extracted sprites.
    private IReadOnlyList<GorgonSprite> _extractedSprites;
    // The currently executing task.
    private Task _currentTask;
    // Flag to indicate that sprite generation is occuring.
    private bool _isGenerating;
    // Flag to indicate that sprite preview mode is active.
    private bool _isInSpritePreview;
    // Current sprite being previewed.
    private int _currentPreviewSprite;
    // The file that contains the texture to extract from.
    private IContentFile _textureFile;



    /// <summary>
    /// Property to return the progress status of the extraction operation.
    /// </summary>
    public ref readonly ProgressData ExtractTaskProgress => ref _extractProgressData;

    /// <summary>
    /// Property to return the texture used to extract the sprites.
    /// </summary>
    public GorgonTexture2DView Texture => _extractData.Texture;

    /// <summary>
    /// Property to return the flag to indicate that sprite generation is executing.
    /// </summary>
    public bool IsGenerating
    {
        get => _isGenerating;
        private set
        {
            if (_isGenerating == value)
            {
                return;
            }

            OnPropertyChanging();
            _isGenerating = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether sprite preview mode is active or not.
    /// </summary>
    public bool IsInSpritePreview
    {
        get => _isInSpritePreview;
        set
        {
            if (value == _isInSpritePreview)
            {
                return;
            }

            OnPropertyChanging();
            _isInSpritePreview = value;
            OnPropertyChanged();
        }
    }


    /// <summary>
    /// Property to return the number of sprites for previewing.
    /// </summary>
    public int SpritePreviewCount => _extractedSprites?.Count ?? 0;

    /// <summary>
    /// Property to return the current preview sprite index.
    /// </summary>
    public int CurrentPreviewSprite
    {
        get => _currentPreviewSprite;
        private set
        {
            if (value == _currentPreviewSprite)
            {
                return;
            }

            OnPropertyChanging();
            _currentPreviewSprite = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the number of columns/rows in the grid.
    /// </summary>
    public DX.Size2 GridSize
    {
        get => _extractData.GridSize;
        set
        {
            if (_extractData.GridSize == value)
            {
                return;
            }

            OnPropertyChanging();
            _extractData.GridSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the maximum columns and rows allowed in the grid.
    /// </summary>
    public DX.Size2 MaxGridSize => _extractData.MaxGridSize;

    /// <summary>
    /// Property to set or return the offset of the grid, in pixels.
    /// </summary>
    public DX.Point GridOffset
    {
        get => _extractData.GridOffset;
        set
        {
            if (_extractData.GridOffset == value)
            {
                return;
            }

            OnPropertyChanging();
            _extractData.GridOffset = value;
            OnPropertyChanged();

            NotifyPropertyChanged(nameof(MaxGridSize));
        }
    }


    /// <summary>
    /// Property to set or return the size of a grid cell.
    /// </summary>
    public DX.Size2 CellSize
    {
        get => _extractData.CellSize;
        set
        {
            if (_extractData.CellSize == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.GridCellSize = _extractData.CellSize = value;
            OnPropertyChanged();

            NotifyPropertyChanged(nameof(MaxGridSize));
        }
    }

    /// <summary>
    /// Property to set or return whether the extractor should skip empty regions based on a color value.
    /// </summary>
    public bool SkipEmpty
    {
        get => _extractData.SkipEmpty;
        set
        {
            if (_extractData.SkipEmpty == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.AllowEmptySpriteSkip = _extractData.SkipEmpty = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the mask color to use when skipping empty regions.
    /// </summary>
    public GorgonColor SkipMaskColor
    {
        get => _extractData.SkipColor;
        private set
        {
            if (_extractData.SkipColor == value)
            {
                return;
            }

            OnPropertyChanging();
            _extractData.SkipColor = value;
            _settings.SkipColor = GorgonColor.ToARGB(value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the maximum number of array indices.
    /// </summary>
    public int MaxArrayCount => _extractData.Texture.Texture.ArrayCount - _extractData.StartArrayIndex;

    /// <summary>
    /// Property to return the maximum number of array indices.
    /// </summary>
    public int MaxArrayIndex => _extractData.Texture.Texture.ArrayCount - 1;

    /// <summary>
    /// Property to set or return the number of array indices to use.
    /// </summary>
    public int ArrayCount
    {
        get => _extractData.ArrayCount;
        set
        {
            if (_extractData.ArrayCount == value)
            {
                return;
            }

            OnPropertyChanging();
            _extractData.ArrayCount = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the starting array index to use.
    /// </summary>
    public int StartArrayIndex
    {
        get => _extractData.StartArrayIndex;
        set
        {
            value = value.Min(MaxArrayIndex).Max(0);
            if (_extractData.StartArrayIndex == value)
            {
                return;
            }

            OnPropertyChanging();
            _extractData.StartArrayIndex = value;
            OnPropertyChanged();

            NotifyPropertyChanged(nameof(MaxArrayCount));
        }
    }

    /// <summary>
    /// Property to return the list of sprites retrieved from extraction.
    /// </summary>
    public IReadOnlyList<GorgonSprite> Sprites
    {
        get => _extractedSprites;
        private set
        {
            if (_extractedSprites == value)
            {
                return;
            }

            NotifyPropertyChanging(nameof(CurrentPreviewSprite));
            NotifyPropertyChanging(nameof(SpritePreviewCount));

            OnPropertyChanging();
            _extractedSprites = value;
            OnPropertyChanged();

            NotifyPropertyChanged(nameof(SpritePreviewCount));
            NotifyPropertyChanged(nameof(CurrentPreviewSprite));
        }
    }

    /// <summary>
    /// Property to return the command used to assign the color used to skip empty regions.
    /// </summary>
    public IEditorCommand<object> SetEmptySpriteMaskColorCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to go to the next preview sprite.
    /// </summary>
    public IEditorCommand<object> NextPreviewSpriteCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to go to the previous preview sprite.
    /// </summary>
    public IEditorCommand<object> PrevPreviewSpriteCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command that will generate the sprite data.
    /// </summary>
    public IEditorAsyncCommand<object> GenerateSpritesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to cancel sprite generation.
    /// </summary>
    public IEditorCommand<object> CancelSpriteGenerationCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to save the sprite data.
    /// </summary>
    public IEditorCommand<SaveSpritesArgs> SaveSpritesCommand
    {
        get;
    }



    /// <summary>
    /// Function to assign the color used to mask out empty regions.
    /// </summary>
    private void DoSetMaskColor()
    {
        try
        {
            GorgonColor? newColor = HostServices.ColorPicker.GetColor(_extractData.SkipColor);

            if (newColor is null)
            {
                return;
            }

            SkipMaskColor = newColor.Value;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREST_ERR_PICKING_COLOR);
        }
    }

    /// <summary>
    /// Function to determine if the sprite index can be decremented to the previous sprite index.
    /// </summary>
    /// <returns><b>true</b> if the index can be decremented, <b>false</b> if not.</returns>
    private bool CanPrevSprite() => (_extractedSprites is not null) && (_extractedSprites.Count > 0) && (IsInSpritePreview) && (_currentPreviewSprite > 0);

    /// <summary>
    /// Function to decrement the current preview sprite index.
    /// </summary>
    private void DoPrevSprite()
    {
        try
        {
            --CurrentPreviewSprite;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREST_ERR_PREVIEW_SPRITE);
        }
    }

    /// <summary>
    /// Function to determine if the sprite index can be incremented to the next sprite index.
    /// </summary>
    /// <returns><b>true</b> if the index can be incremented, <b>false</b> if not.</returns>
    private bool CanNextSprite() => (_extractedSprites is not null) && (_extractedSprites.Count > 0) && (IsInSpritePreview) && (_currentPreviewSprite + 1 < _extractedSprites.Count);

    /// <summary>
    /// Function to increment the current preview sprite index.
    /// </summary>
    private void DoNextSprite()
    {
        try
        {
            ++CurrentPreviewSprite;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREST_ERR_PREVIEW_SPRITE);
        }
    }

    /// <summary>
    /// Function to perform the generation of the sprites from the grid data.
    /// </summary>
    private async Task DoGenerateSpritesAsync()
    {
        void UpdateProgress(ProgressData progress) => _extractProgressData = progress;

        IGorgonImage imageData = null;
        Task<IReadOnlyList<GorgonSprite>> spriteGenTask;
        Task<IGorgonImage> getImageTask;
        bool inSpritePreview = IsInSpritePreview;

        try
        {
            _cancelSource = new CancellationTokenSource();

            IsInSpritePreview = false;
            IsGenerating = true;

            _currentTask = getImageTask = _extractor.GetSpriteTextureImageDataAsync(_extractData);
            imageData = await getImageTask;
            _currentTask = null;

            if (_cancelSource.Token.IsCancellationRequested)
            {
                return;
            }

            _currentTask = spriteGenTask = Task.Run(() => _extractor.ExtractSprites(_extractData, imageData, UpdateProgress, _cancelSource.Token), _cancelSource.Token);
            IReadOnlyList<GorgonSprite> sprites = await spriteGenTask;

            // The user cancelled if we have no sprites.
            if (sprites.Count == 0)
            {
                return;
            }

            if (CurrentPreviewSprite >= sprites.Count)
            {
                CurrentPreviewSprite = sprites.Count - 1;
            }

            Sprites = sprites;
        }
        catch (OperationCanceledException)
        {
            // User cancelled, leave.
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREST_ERR_GEN_SPRITES);
        }
        finally
        {
            IsInSpritePreview = inSpritePreview;
            HostServices.BusyService.SetIdle();
            imageData?.Dispose();
            _currentTask = null;
            IsGenerating = false;
        }
    }

    /// <summary>
    /// Function to determine if the sprite generation can be canceled or not.
    /// </summary>
    /// <returns><b>true</b> if it can be canceled, <b>false</b> if not.</returns>
    private bool CanCancelSpriteGeneration() => (_cancelSource is not null) && (!_cancelSource.IsCancellationRequested) && (_currentTask is not null) && (IsGenerating);

    /// <summary>
    /// Function to cancel an active sprite generation process.
    /// </summary>
    private void DoCancelSpriteGeneration()
    {
        try
        {
            _cancelSource.Cancel();
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("ERROR: Error cancelling sprite generation.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }


    /// <summary>
    /// Function to determine if the sprites can be saved at this time.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the sprites can be saved, <b>false</b> if not.</returns>
    private bool CanSaveSprites(SaveSpritesArgs args) => (Sprites is not null) && (Sprites.Count > 0) && (!IsGenerating);

    /// <summary>
    /// Function to save the sprites to the file system.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoSaveSprites(SaveSpritesArgs args)
    {
        try
        {
            string lastUsedPath = _settings.LastOutputDir;

            if (string.IsNullOrWhiteSpace(lastUsedPath))
            {
                lastUsedPath = "/";
            }

            string newPath = HostServices.FolderBrowser.GetFolderPath(lastUsedPath, Resources.GOREST_CAPTION_FOLDER_SELECT, Resources.GOREST_DESC_FOLDER_SELECT);

            if (string.IsNullOrWhiteSpace(newPath))
            {
                args.Cancel = true;
                return;
            }
            HostServices.BusyService.SetBusy();

            string baseFileName = $"{Path.GetFileNameWithoutExtension(_textureFile.Name)} Sprite";

            _extractor.SaveSprites(newPath, baseFileName, Sprites, _textureFile);

            _settings.LastOutputDir = newPath;
        }
        catch (Exception ex)
        {
            args.Cancel = true;
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREST_ERR_SAVE);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>Function to determine the action to take when this tool is closing.</summary>
    /// <returns>
    ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
    /// <remarks>
    ///   <para>
    /// Tool plug in developers can override this method to verify changes, or perform last minute updates as needed.
    /// </para>
    ///   <para>
    /// This is set up as an asynchronous method so that users may save their data asynchronously to keep the UI usable.
    /// </para>
    /// </remarks>
    protected async override Task<bool> OnCloseToolTaskAsync()
    {
        // If we're executing a task, wait for it to finish.
        if (_currentTask is null)
        {
            return true;
        }


        if (HostServices.MessageDisplay.ShowConfirmation(Resources.GOREST_CONFIRM_EXTRACT_IN_PROGRESS) == MessageResponse.No)
        {
            return false;
        }

        try
        {
            // Force a cancellation of the running task.
            _cancelSource?.Cancel();

            await _currentTask;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("ERROR: Error stopping the extraction process.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
        return true;
    }

    /// <summary>Function to initialize the tool.</summary>
    /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
    /// <remarks>
    ///   <para>
    /// This is required in order to inject initialization data for the view model from an external source (i.e. the editor host application). Developers will use this area to perform setup of
    /// the editor view model, and validate any information being injected.
    /// </para>
    ///   <para>
    /// Ideally this can be used to create resources, child view models, and other objects that need to be initialized when the view model is created.
    /// </para>
    ///   <para>
    /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
    /// </para>
    /// </remarks>
    protected override void OnInitialize(ExtractParameters injectionParameters)
    {
        base.OnInitialize(injectionParameters);

        _settings = injectionParameters.Settings;
        _extractData = injectionParameters.ExtractionData;
        _extractor = injectionParameters.Extractor;
        _textureFile = injectionParameters.TextureFile;

        if (_extractData.GridOffset.X > _extractData.Texture.Width - _extractData.CellSize.Width)
        {
            _extractData.GridOffset = new DX.Point(0, _extractData.GridOffset.Y);
        }

        if (_extractData.GridOffset.Y > _extractData.Texture.Height - _extractData.CellSize.Height)
        {
            _extractData.GridOffset = new DX.Point(_extractData.GridOffset.X, 0);
        }

        _extractData.StartArrayIndex = _extractData.StartArrayIndex.Min(_extractData.Texture.ArrayCount - 1);
        _extractData.ArrayCount = _extractData.ArrayCount.Min(_extractData.Texture.ArrayCount - _extractData.StartArrayIndex);

        if ((_extractData.GridSize.Width == 0) || (_extractData.GridSize.Height == 0))
        {
            _extractData.GridSize = MaxGridSize;
        }
        else
        {
            _extractData.GridSize = new DX.Size2(_extractData.GridSize.Width.Min(MaxGridSize.Width), _extractData.GridSize.Height.Min(MaxGridSize.Height));
        }
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    protected override void OnUnload()
    {
        _cancelSource?.Dispose();
        base.Unload();
    }



    /// <summary>Initializes a new instance of the <see cref="Extract"/> class.</summary>
    public Extract()
    {
        SetEmptySpriteMaskColorCommand = new EditorCommand<object>(DoSetMaskColor);
        NextPreviewSpriteCommand = new EditorCommand<object>(DoNextSprite, CanNextSprite);
        PrevPreviewSpriteCommand = new EditorCommand<object>(DoPrevSprite, CanPrevSprite);
        GenerateSpritesCommand = new EditorAsyncCommand<object>(DoGenerateSpritesAsync);
        CancelSpriteGenerationCommand = new EditorCommand<object>(DoCancelSpriteGeneration, CanCancelSpriteGeneration);
        SaveSpritesCommand = new EditorCommand<SaveSpritesArgs>(DoSaveSprites, CanSaveSprites);
    }

}
