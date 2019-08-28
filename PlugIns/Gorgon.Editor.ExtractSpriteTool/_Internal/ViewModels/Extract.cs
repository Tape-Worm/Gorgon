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
// Created: April 24, 2019 11:10:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace Gorgon.Editor.ExtractSpriteTool
{
    /// <summary>
    /// The view model for the main UI.
    /// </summary>
    internal class Extract
        : ViewModelBase<ExtractParameters>, IExtract
    {
        #region Variables.
        // The data used to extract sprites.
        private SpriteExtractionData _extractData;
        // The plug in settings.
        private ExtractSpriteToolSettings _settings;
        // The currently active task.
        private Task _currentTask;
        // The task for generating sprites.
        private Task _spriteGenTask;
        // The message display service.
        private IMessageDisplayService _messageDisplay;
        // The progress data for the extraction task.
        private ProgressData _extractProgressData;
        // The list of extracted sprites.
        private IReadOnlyList<GorgonSprite> _extractedSprites;
        // The service used to build the sprites.
        private IExtractorService _extractor;
        // The cancellation token source for cancelling tasks.
        private CancellationTokenSource _cancelSource;
        // The busy state service.
        private IBusyStateService _busyService;
        // Color picker service.
        private IColorPickerService _colorPicker;
        // The project file system folder browser.
        private IFileSystemFolderBrowseService _folderBrowser;
        // The file for the sprite texture.
        private IContentFile _textureFile;
        // Flag to indicate that we are in sprite preview.
        private bool _inSpritePreview;
        // The current sprite index for the sprite preview.
        private int _spriteIndex;
        // The current preview array index.
        private int _arrayIndex;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the progress status of the extraction operation.
        /// </summary>
        public ref readonly ProgressData ExtractTaskProgress => ref _extractProgressData;

        /// <summary>
        /// Property to set or return whether we are in sprite preview mode.
        /// </summary>
        public bool InSpritePreview
        {
            get => _inSpritePreview;
            set
            {
                if (_inSpritePreview == value)
                {
                    return;
                }

                OnPropertyChanging();
                _inSpritePreview = value;
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(SpritePreviewCount));
                NotifyPropertyChanged(nameof(CurrentPreviewSprite));
            }
        }

        /// <summary>
        /// Property to return the current preview sprite index.
        /// </summary>
        public int CurrentPreviewSprite
        {
            get => _spriteIndex;
            private set
            {
                if (_spriteIndex == value)
                {
                    return;
                }

                OnPropertyChanging();
                _spriteIndex = value.Min(_extractedSprites?.Count ?? 0).Max(0);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the number of sprites for previewing.
        /// </summary>
        public int SpritePreviewCount => _extractedSprites?.Count ?? 0;

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

                OnPropertyChanging();
                _extractedSprites = value;
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(SpritePreviewCount));
                NotifyPropertyChanged(nameof(CurrentPreviewSprite));
            }
        }

        /// <summary>
        /// Property to set or return the task used to generate the sprite data.
        /// </summary>
        public Task SpriteGenerationTask
        {
            get => _spriteGenTask;
            private set
            {
                if (_spriteGenTask == value)
                {
                    return;
                }

                OnPropertyChanging();
                _currentTask = _spriteGenTask = value;
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
                _extractData.CellSize = value;
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(MaxGridSize));
            }
        }

        /// <summary>
        /// Property to return whether or not the texture is an array.
        /// </summary>
        public bool HasArray => _extractData.Texture.Texture.ArrayCount > 1;


        /// <summary>
        /// Property to return the maximum array indices in the texture for previewing.
        /// </summary>
        public int PreviewArrayCount => _extractData.Texture.Texture.ArrayCount;

        /// <summary>
        /// Property to return the current array index being previewed.
        /// </summary>
        public int CurrentPreviewArrayIndex
        {
            get => _arrayIndex;
            private set
            {
                if (_arrayIndex == value)
                {
                    return;
                }

                OnPropertyChanging();
                _arrayIndex = value;
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

        /// <summary>Property to return the texture that is to be rendered.</summary>
        public GorgonTexture2DView Texture
        {
            get => _extractData.Texture;
            private set
            {
                if (_extractData.Texture == value)
                {
                    return;
                }

                OnPropertyChanging();
                _extractData.Texture = value;
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(CellSize));
                NotifyPropertyChanged(nameof(MaxGridSize));
                NotifyPropertyChanged(nameof(MaxArrayIndex));
                NotifyPropertyChanged(nameof(MaxArrayCount));
                NotifyPropertyChanged(nameof(HasArray));
            }
        }

        /// <summary>
        /// Property to set or return whether the UI is in a maximized state or not.
        /// </summary>
        public bool IsMaximized
        {
            get => _settings.IsMaximized;
            set
            {
                if (_settings.IsMaximized == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.IsMaximized = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the color used when determining which sprites are considered empty.
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
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return whether to allow skipping empty sprites.
        /// </summary>
        public bool AllowSkipEmpty
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
        /// Property to return the command that will generate the sprite data.
        /// </summary>
        public IEditorCommand<object> GenerateSpritesCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to shut down any running operations.
        /// </summary>
        public IEditorAsyncCommand<object> ShutdownCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to assign the masking color used for skipping empty sprites.
        /// </summary>
        public IEditorCommand<object> SetEmptySpriteMaskColorCommand
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
        /// Property to return the command used to go to the next preview array index.
        /// </summary>
        public IEditorCommand<object> NextPreviewArrayCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to go to the previous preview array index.
        /// </summary>
        public IEditorCommand<object> PrevPreviewArrayCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to copy the current array preview index into the array range start value.
        /// </summary>
        public IEditorCommand<object> SendPreviewArrayToStartCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to shut down any running operations.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoShutdownAsync()
        {
            try
            {
                if ((_currentTask == null) || (_cancelSource == null))
                {
                    return;
                }

                _cancelSource.Cancel();

                await _currentTask;
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to perform the generation of the sprites from the grid data.
        /// </summary>
        private async void DoGenerateSpritesAsync()
        {
            void UpdateProgress(ProgressData progress) => _extractProgressData = progress;

            IGorgonImage imageData = null;
            Task<IReadOnlyList<GorgonSprite>> spriteGenTask;
            Task<IGorgonImage> getImageTask;

            try
            {
                _cancelSource = new CancellationTokenSource();

                _currentTask = getImageTask = _extractor.GetSpriteTextureImageDataAsync(_extractData);
                imageData = await getImageTask;
                _currentTask = null;

                if (_cancelSource.Token.IsCancellationRequested)
                {
                    return;
                }

                SpriteGenerationTask = spriteGenTask = Task.Run(() => _extractor.ExtractSprites(_extractData, imageData, UpdateProgress, _cancelSource.Token), _cancelSource.Token);
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
                _messageDisplay.ShowError(ex, Resources.GOREST_ERR_GEN_SPRITES);
            }
            finally
            {
                _busyService.SetIdle();
                imageData?.Dispose();
                SpriteGenerationTask = null;
            }
        }

        /// <summary>
        /// Function to determine if the skip masking color can be set.
        /// </summary>
        /// <returns><b>true</b> if the masking color can be set, <b>false</b> if not.</returns>
        private bool CanSetEmptyMaskColor() => AllowSkipEmpty;

        /// <summary>
        /// Function to set the masking color used to determine empty sprite space.
        /// </summary>
        private void DoSetEmptyMaskColor()
        {
            try
            {
                GorgonColor? newColor = _colorPicker.GetColor(_extractData.SkipColor);

                if (newColor == null)
                {
                    return;
                }

                SkipMaskColor = newColor.Value;
                _settings.SkipColor = newColor.Value.ToARGB();
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GOREST_ERR_PICKING_COLOR);
            }
        }

        /// <summary>
        /// Function to determine if the sprites can be saved at this time.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the sprites can be saved, <b>false</b> if not.</returns>
        private bool CanSaveSprites(SaveSpritesArgs args) => (Sprites != null) && (Sprites.Count > 0);

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

                string newPath = _folderBrowser.GetFolderPath(lastUsedPath, Resources.GOREST_CAPTION_FOLDER_SELECT, Resources.GOREST_DESC_FOLDER_SELECT);

                if (string.IsNullOrWhiteSpace(newPath))
                {
                    args.Cancel = true;
                    return;
                }
                _busyService.SetBusy();

                string baseFileName = $"{Path.GetFileNameWithoutExtension(_textureFile.Name)} Sprite";

                _extractor.SaveSprites(newPath, baseFileName, Sprites, _textureFile);

                _settings.LastOutputDir = newPath;
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                _messageDisplay.ShowError(ex, Resources.GOREST_ERR_SAVE);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if the sprite index can be decremented to the previous sprite index.
        /// </summary>
        /// <returns><b>true</b> if the index can be decremented, <b>false</b> if not.</returns>
        private bool CanPrevSprite() => (_extractedSprites != null) && (_extractedSprites.Count > 0) && (InSpritePreview) && (_spriteIndex > 0);

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
                _messageDisplay.ShowError(ex, Resources.GOREST_ERR_PREVIEW_SPRITE);
            }
        }

        /// <summary>
        /// Function to determine if the sprite index can be incremented to the next sprite index.
        /// </summary>
        /// <returns><b>true</b> if the index can be incremented, <b>false</b> if not.</returns>
        private bool CanNextSprite() => (_extractedSprites != null) && (_extractedSprites.Count > 0) && (InSpritePreview) && (_spriteIndex + 1 < _extractedSprites.Count);

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
                _messageDisplay.ShowError(ex, Resources.GOREST_ERR_PREVIEW_SPRITE);
            }
        }

        /// <summary>
        /// Function to determine if the sprite index can be decremented to the previous array index.
        /// </summary>
        /// <returns><b>true</b> if the index can be decremented, <b>false</b> if not.</returns>
        private bool CanPrevArray() => (HasArray) && (_arrayIndex > 0);

        /// <summary>
        /// Function to decrement the current preview sprite index.
        /// </summary>
        private void DoPrevArray()
        {
            try
            {
                --CurrentPreviewArrayIndex;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GOREST_ERR_PREVIEW_ARRAY);
            }
        }

        /// <summary>
        /// Function to determine if the sprite index can be incremented to the next array index.
        /// </summary>
        /// <returns><b>true</b> if the index can be incremented, <b>false</b> if not.</returns>
        private bool CanNextArray() => (HasArray) && (_arrayIndex + 1 < PreviewArrayCount);

        /// <summary>
        /// Function to increment the current preview sprite index.
        /// </summary>
        private void DoNextArray()
        {
            try
            {
                ++CurrentPreviewArrayIndex;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GOREST_ERR_PREVIEW_ARRAY);
            }
        }

        /// <summary>
        /// Function to determine if the preview array index can be sent to the array start.
        /// </summary>
        /// <returns><b>true</b> if the value can be copied, <b>false</b> if not.</returns>
        private bool CanSendToArrayStart() => HasArray;

        /// <summary>
        /// Function to send the current preview array index value into the starting array range index.
        /// </summary>
        private void DoSendToArrayStart()
        {
            try
            {
                StartArrayIndex = _arrayIndex;
            }
            catch (Exception ex)
            {
                Log.Print("[ERROR] Unable to write the array index. This shouldn't happen.", LoggingLevel.Simple);
                Log.LogException(ex);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(ExtractParameters injectionParameters)
        {
            _extractData = injectionParameters.Data ?? throw new ArgumentMissingException(nameof(injectionParameters.Data), nameof(injectionParameters));
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
            _extractor = injectionParameters.Extractor ?? throw new ArgumentMissingException(nameof(injectionParameters.Extractor), nameof(injectionParameters));
            _busyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(injectionParameters.BusyService), nameof(injectionParameters));
            _colorPicker = injectionParameters.ColorPicker ?? throw new ArgumentMissingException(nameof(injectionParameters.ColorPicker), nameof(injectionParameters));
            _folderBrowser = injectionParameters.FolderBrowser ?? throw new ArgumentMissingException(nameof(injectionParameters.FolderBrowser), nameof(injectionParameters));
            _textureFile = injectionParameters.TextureFile ?? throw new ArgumentMissingException(nameof(injectionParameters.TextureFile), nameof(injectionParameters));

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
        public override void OnUnload()
        {
            _extractData.Texture = null;
            base.OnUnload();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ExtractSpriteTool.Extract"/> class.</summary>
        public Extract()
        {
            GenerateSpritesCommand = new EditorCommand<object>(DoGenerateSpritesAsync);
            ShutdownCommand = new EditorAsyncCommand<object>(DoShutdownAsync);
            SetEmptySpriteMaskColorCommand = new EditorCommand<object>(DoSetEmptyMaskColor, CanSetEmptyMaskColor);
            SaveSpritesCommand = new EditorCommand<SaveSpritesArgs>(DoSaveSprites, CanSaveSprites);
            NextPreviewSpriteCommand = new EditorCommand<object>(DoNextSprite, CanNextSprite);
            PrevPreviewSpriteCommand = new EditorCommand<object>(DoPrevSprite, CanPrevSprite);
            NextPreviewArrayCommand = new EditorCommand<object>(DoNextArray, CanNextArray);
            PrevPreviewArrayCommand = new EditorCommand<object>(DoPrevArray, CanPrevArray);
            SendPreviewArrayToStartCommand = new EditorCommand<object>(DoSendToArrayStart, CanSendToArrayStart);
        }
        #endregion
    }
}
