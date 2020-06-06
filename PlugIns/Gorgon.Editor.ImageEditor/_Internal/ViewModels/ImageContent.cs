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
// Created: October 30, 2018 7:58:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The image editor content.
    /// </summary>
    internal class ImageContent
        : ContentEditorViewModelBase<ImageContentParameters>, IImageContent
    {
        #region Undo Args.
        /// <summary>
        /// The arguments used to undo/redo a conversion.
        /// </summary>
        private class ConvertUndoArgs
        {
            /// <summary>
            /// The file used to store the undo data.
            /// </summary>
            public IGorgonVirtualFile UndoFile;

            /// <summary>
            /// The format of the image for redo.
            /// </summary>
            public BufferFormat Format;
        }

        /// <summary>
        /// The arguments used to undo/redo the alpha set operation.
        /// </summary>
        private class SetAlphaUndoArgs
        {
            /// <summary>
            /// The file used to store the undo data.
            /// </summary>
            public IGorgonVirtualFile UndoFile;

            /// <summary>
            /// The alpha value used.
            /// </summary>
            public int Alpha;

            /// <summary>
            /// The minimum/maximum alpha threshold.
            /// </summary>
            public GorgonRange MinMax;

            /// <summary>
            /// The mip level used when setting alpha.
            /// </summary>
            public int MipLevel;

            /// <summary>
            /// The array index (or depth slice) used when setting alpha.
            /// </summary>
            public int ArrayIndex;
        }

        /// <summary>
        /// The arguments used to undo/redo effects applied to the image.
        /// </summary>
        private class EffectsUndoArgs
        {
            /// <summary>
            /// The file used to store the undo data.
            /// </summary>
            public IGorgonVirtualFile UndoFile;

            /// <summary>
            /// The file used to store the redo data.
            /// </summary>
            public IGorgonVirtualFile RedoFile;
        }

        /// <summary>
        /// The arguments used to undo/redo an image import or dimension update.
        /// </summary>
        private class ImportDimensionUndoArgs
        {
            /// <summary>
            /// The file used to store the undo data.
            /// </summary>
            public IGorgonVirtualFile UndoFile;

            /// <summary>
            /// The file used to store the redo data.
            /// </summary>
            public IGorgonVirtualFile RedoFile;
        }

        /// <summary>
        /// The arguments used to undo/redo an image type modification.
        /// </summary>
        private class ImageTypeUndoArgs
        {
            /// <summary>
            /// The file used to store the undo data.
            /// </summary>
            public IGorgonVirtualFile UndoFile;

            /// <summary>
            /// The type of image for redo.
            /// </summary>
            public ImageType ImageType;
        }
        #endregion

        #region Constants.
        /// <summary>
        /// The attribute key name for the image codec attribute.
        /// </summary>
        public const string CodecAttr = "ImageCodec";
        /// <summary>
        /// The attribute key name for the premultiplied alpha flag.
        /// </summary>
        public const string PremultipliedAttr = "PremultipliedAlpha";
        #endregion

        #region Variables.
        // The list of available codecs matched by extension.
        private readonly List<(GorgonFileExtension extension, IGorgonImageCodec codec)> _codecs = new List<(GorgonFileExtension extension, IGorgonImageCodec codec)>();
        // The directory to store the undo cache data.
        private IGorgonVirtualDirectory _undoCacheDir;
        // The format support information for the current video card.
        private IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> _formatSupport;
        // The available pixel formats, based on codec.
        private ObservableCollection<BufferFormat> _pixelFormats = new ObservableCollection<BufferFormat>();
        // The settings for the image editor plugin.
        private ISettings _settings;
        // The file used for working changes.
        private IGorgonVirtualFile _workingFile;
        // The service used to read/write image data.
        private IImageIOService _imageIO;
        // The pixel format for the file.
        private BufferFormat _format;
        // The undo service.
        private IUndoService _undoService;
        // The current mip level.
        private int _currentMipLevel;
        // The current array index.
        private int _currentArrayindex;
        // The current depth slice.
        private int _currentDepthSlice;
        // The view model used to update cropping/resizing settings for import.
        private ICropResizeSettings _cropResizeSettings;
        // The view model used to update the image dimensions.
        private IDimensionSettings _dimensionSettings;
        // The view model used to generate mip map levels.
        private IMipMapSettings _mipMapSettings;
        // The view model used to apply set the alpha value.
        private IAlphaSettings _alphaSettings;
        // The service used to update the image data.
        private IImageUpdaterService _imageUpdater;
        // Information about the current video adapter.
        private IGorgonVideoAdapterInfo _videoAdapter;
        // The currently active panel.
        private IHostedPanelViewModel _currentPanel;
        // The service used to edit the image with an external editor.
        private IImageExternalEditService _externalEditor;
        // Flag to indicate that the image should use premultiplied alpha.
        private bool _isPremultiplied;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether mip maps are supported for the current format.
        /// </summary>
        public bool MipSupport => (_formatSupport != null) && (ImageData != null)
            && (_formatSupport.ContainsKey(CurrentPixelFormat))
            && (_formatSupport[CurrentPixelFormat].FormatSupport & BufferFormatSupport.Mip) == BufferFormatSupport.Mip;

        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => CommonEditorContentTypes.ImageType;

        /// <summary>
        /// Property to return whether this image is premultiplied.
        /// </summary>
        public bool IsPremultiplied
        {
            get => _isPremultiplied;
            set
            {
                if (value == _isPremultiplied)
                {
                    return;
                }

                OnPropertyChanging();
                _isPremultiplied = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the image data.
        /// </summary>
        public IGorgonImage ImageData
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the list of codecs available.
        /// </summary>
        public ObservableCollection<IGorgonImageCodec> Codecs
        {
            get;
        } = new ObservableCollection<IGorgonImageCodec>();

        /// <summary>
        /// Property to return the list of available image pixel formats (based on codec).
        /// </summary>
        public ObservableCollection<BufferFormat> PixelFormats
        {
            get => _pixelFormats;
            private set
            {
                if (_pixelFormats == value)
                {
                    return;
                }

                OnPropertyChanging();
                _pixelFormats = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the command used to export an image.</summary>
        public IEditorCommand<IGorgonImageCodec> ExportImageCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the current pixel format for the image.
        /// </summary>
        public BufferFormat CurrentPixelFormat
        {
            get => _format;
            private set
            {
                if (_format == value)
                {
                    return;
                }

                OnPropertyChanging();
                _format = value;
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(MipSupport));

                if (DimensionSettings != null)
                {
                    DimensionSettings.MipSupport = MipSupport;
                }
            }
        }

        /// <summary>
        /// Property to return the command used to convert the image pixel format.
        /// </summary>
        public IEditorCommand<BufferFormat> ConvertFormatCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the format support information for the current video card.
        /// </summary>
        public IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
        {
            get => _formatSupport;
            private set
            {
                if (_formatSupport == value)
                {
                    return;
                }

                OnPropertyChanging();
                _formatSupport = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command to execute when an undo operation is requested.
        /// </summary>
        public IEditorCommand<object> UndoCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when an undo operation is requested.
        /// </summary>
        public IEditorCommand<object> RedoCommand
        {
            get;
        }

        /// <summary>Property to return the type of image that is loaded.</summary>
        public ImageType ImageType => ImageData?.ImageType ?? ImageType.Unknown;

        /// <summary>Property to return the number of mip maps in the image.</summary>
        public int MipCount => ImageData.MipCount;

        /// <summary>Property to return the number of array indices in the image.</summary>
        public int ArrayCount => ImageData.ArrayCount;

        /// <summary>Property to return the number of depth slices in the image.</summary>
        public int DepthCount => ImageData.GetDepthCount(CurrentMipLevel);

        /// <summary>Property to set or return the current mip map level.</summary>
        public int CurrentMipLevel
        {
            get => _currentMipLevel;
            set
            {
                if (_currentMipLevel == value)
                {
                    return;
                }

                if ((value < 0) || (value >= MipCount))
                {
                    return;
                }

                OnPropertyChanging();
                _currentMipLevel = value;
                OnPropertyChanged();

                if (ImageType != ImageType.Image3D)
                {
                    return;
                }

                NotifyPropertyChanged(nameof(DepthCount));
                CurrentDepthSlice = CurrentDepthSlice.Min(DepthCount - 1).Max(0);
            }
        }

        /// <summary>Property to set or return the current array index.</summary>
        public int CurrentArrayIndex
        {
            get => _currentArrayindex;
            set
            {
                if (_currentArrayindex == value)
                {
                    return;
                }

                if ((value < 0) || (value >= ArrayCount))
                {
                    return;
                }

                OnPropertyChanging();
                _currentArrayindex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the current depth slice.</summary>
        public int CurrentDepthSlice
        {
            get => _currentDepthSlice;
            set
            {
                if (_currentDepthSlice == value)
                {
                    return;
                }

                if ((value < 0) || (value >= DepthCount))
                {
                    return;
                }

                OnPropertyChanging();
                _currentDepthSlice = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the width of the image, in pixels.
        /// </summary>
        public int Width => ImageData.Width;

        /// <summary>
        /// Property to return the height of the image, in pixels.
        /// </summary>
        public int Height => ImageData.Height;

        /// <summary>
        /// Property to return the view model for picking images to import into the current image.
        /// </summary>
        public IImagePicker ImagePicker
        {
            get;
            private set;
        }

        /// <summary>Property to return the view model for the cropping/resizing settings.</summary>        
        public ICropResizeSettings CropOrResizeSettings
        {
            get => _cropResizeSettings;
            private set
            {
                if (_cropResizeSettings == value)
                {
                    return;
                }

                OnPropertyChanging();
                _cropResizeSettings = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the view model for the mip map generation settings.
        /// </summary>
        public IMipMapSettings MipMapSettings
        {
            get => _mipMapSettings;
            private set
            {
                if (_mipMapSettings == value)
                {
                    return;
                }

                OnPropertyChanging();
                _mipMapSettings = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the view model for the set alpha settings.
        /// </summary>
        public IAlphaSettings AlphaSettings
        {
            get => _alphaSettings;
            private set
            {
                if (_alphaSettings == value)
                {
                    return;
                }

                OnPropertyChanging();
                _alphaSettings = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the view model for the dimension editing settings.
        /// </summary>
        public IDimensionSettings DimensionSettings
        {
            get => _dimensionSettings;
            private set
            {
                if (_dimensionSettings == value)
                {
                    return;
                }

                OnPropertyChanging();
                _dimensionSettings = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the context object for image effects.
        /// </summary>
        public IFxContext FxContext
        {
            get;
            private set;
        }

        /// <summary>Property to return the currently active panel.</summary>
        public IHostedPanelViewModel CurrentPanel
        {
            get => _currentPanel;
            set
            {
                if (_currentPanel == value)
                {
                    return;
                }

                if (_currentPanel != null)
                {
                    _currentPanel.PropertyChanged -= CurrentHostedPanel_PropertyChanged;
                    _currentPanel.IsActive = false;
                }

                OnPropertyChanging();
                _currentPanel = value;
                OnPropertyChanged();

                if (_currentPanel != null)
                {
                    _currentPanel.IsActive = true;
                    _currentPanel.PropertyChanged += CurrentHostedPanel_PropertyChanged;
                }
            }
        }

        /// <summary>Property to return the command to execute when changing the image type.</summary>
        /// <value>The change image type command.</value>
        public IEditorCommand<ImageType> ChangeImageTypeCommand
        {
            get;
        }

        /// <summary>Property to import an image file into the current image as an array index or depth slice.</summary>        
        public IEditorAsyncCommand<float> ImportFileCommand
        {
            get;
        }

        /// <summary>Property to return the command used to show the image dimensions editor.</summary>
        public IEditorCommand<object> ShowImageDimensionsCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the mip map generation settings.
        /// </summary>
        public IEditorCommand<object> ShowMipGenerationCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the set alpha settings.
        /// </summary>
        public IEditorCommand<object> ShowSetAlphaCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the FX items.
        /// </summary>
        public IEditorCommand<object> ShowFxCommand
        {
            get;
        }

        /// <summary>
        /// Porperty to return the command used to edit the image in an external application.
        /// </summary>
        public IEditorCommand<object> EditInAppCommand
        {
            get;
        }

        /// <summary>Property to return the command used to set the image to use premultiplied alpha.</summary>
        public IEditorCommand<bool> PremultipliedAlphaCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to copy an image from the project into the current image at the selected array/depth/mip index.
        /// </summary>
        public IEditorAsyncCommand<CopyToImageArgs> CopyToImageCommand
        {
            get;
        }
        #endregion

        #region Methods.        
        /// <summary>Handles the CollectionChanged event of the CodecPlugInPaths control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void CodecPlugInPaths_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => BuildCodecList(ImageData);

        /// <summary>Handles the PropertyChanged event of the CurrentHostedPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentHostedPanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IHostedPanelViewModel.IsActive):
                    if ((CurrentPanel != null) && (!CurrentPanel.IsActive))
                    {
                        CurrentPanel = null;
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to notify when the entire image structure has been updated.
        /// </summary>
        /// <param name="mainProperty">The name of the primary property to notify on.</param>
        private void NotifyImageUpdated(string mainProperty)
        {
            NotifyPropertyChanged(mainProperty);
            NotifyPropertyChanged(nameof(MipCount));
            NotifyPropertyChanged(nameof(DepthCount));
            NotifyPropertyChanged(nameof(ArrayCount));

            ContentState = ContentState.Modified;
        }


        /// <summary>
        /// Function to perform the image import by bringing in a new image to replace the current mip level and array index, or depth slice. 
        /// </summary>
        /// <param name="fileName">The name of the file being imported.</param>
        /// <param name="importImage">The image being imported.</param>
        /// <param name="cropResizeMode">The type of resize operation.</param>
        /// <param name="alignment">The alignment of the source image, relative to the destination.</param>
        /// <param name="imageFilter">The filter to apply if resizing.</param>
        /// <param name="preserveAspect"><b>true</b> to preserve the aspect ratio, <b>false</b> to ignore it.</param>
        private void ImportImageData(string file, IGorgonImage importImage, CropResizeMode cropResizeMode, Alignment alignment, ImageFilter imageFilter, bool preserveAspect)
        {
            ImportDimensionUndoArgs importUndoArgs = null;

            Task UndoAction(ImportDimensionUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();

                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;
                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(ImportDimensionUndoArgs redoArgs, CancellationToken cancelToken)
            {
                IGorgonVirtualFile undoFile = null;
                IGorgonVirtualFile redoFile = null;
                Stream redoFileStream = null;

                try
                {
                    HostServices.BusyService.SetBusy();

                    int startArrayOrDepth = ImageType == ImageType.Image3D ? CurrentDepthSlice : CurrentArrayIndex;
                    int width = ImageData.Buffers[CurrentMipLevel, startArrayOrDepth].Width;
                    int height = ImageData.Buffers[CurrentMipLevel, startArrayOrDepth].Height;
                    var newSize = new DX.Size2(width, height);

                    NotifyPropertyChanging(nameof(ImageData));

                    undoFile = CreateUndoCacheFile();
                    if (redoArgs?.RedoFile != null)
                    {
                        // Just reuse the image data in the redo cache item.                        
                        redoFileStream = redoArgs.RedoFile.OpenStream();
                        (IGorgonImage redoImage, _, _) = _imageIO.LoadImageFile(redoFileStream, _workingFile.Name);
                        redoFileStream.Dispose();

                        ImageData.Dispose();
                        NotifyPropertyChanging(nameof(ImageData));
                        ImageData = redoImage;
                        NotifyPropertyChanged(nameof(ImageData));

                        DeleteUndoCacheFile(redoArgs.RedoFile);
                    }
                    else
                    {
                        switch (cropResizeMode)
                        {
                            case CropResizeMode.Crop:
                                // Crop the source image down to the same size as this image.
                                _imageUpdater.CropTo(importImage, newSize, alignment);
                                break;
                            case CropResizeMode.Resize:
                                _imageUpdater.Resize(importImage, newSize, imageFilter, preserveAspect);
                                break;
                        }

                        // By default, just copy straight in.
                        _imageUpdater.CopyTo(importImage, ImageData, CurrentMipLevel, startArrayOrDepth, alignment, false);
                    }

                    // Save the updated data to the working file.
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);
                    redoFile = CreateUndoCacheFile();

                    NotifyImageUpdated(nameof(ImageData));

                    if (redoArgs == null)
                    {
                        redoArgs = importUndoArgs = new ImportDimensionUndoArgs();
                    }

                    redoArgs.RedoFile = redoFile;
                    redoArgs.UndoFile = undoFile;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                    if (undoFile != null)
                    {
                        DeleteUndoCacheFile(undoFile);
                    }

                    if (redoFile != null)
                    {
                        redoFileStream?.Dispose();
                        DeleteUndoCacheFile(redoFile);
                    }

                    importUndoArgs = null;
                    return Task.FromException(ex);
                }
                finally
                {
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(string.Format(Resources.GORIMG_UNDO_DESC_IMPORT, file), UndoAction, RedoAction, importUndoArgs, importUndoArgs);
                // Need to call this so the UI can register our updated undo stack.
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to determine if an image file can be imported into the current image.
        /// </summary>
        /// <param name="_">Not used.</param>
        /// <returns><b>true</b> if the image can be imported, <b>false</b> if not.</returns>
        private bool CanImportFile(float _) 
        {
            if ((ImageData == null) || (CurrentPanel != null) || (CommandContext != null))
            {
                return false;
            }
            
            IReadOnlyList<string> selectedFiles = ContentFileManager.GetSelectedFiles();

            return selectedFiles.Select(file => ContentFileManager.GetFile(file))
                        .Where(file => file != null)
                        .Any(file => (file.Metadata.ContentMetadata != null)
                                    && (file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string dataType))
                                    && (string.Equals(dataType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Function to confirm the overwrite of data in the image.
        /// </summary>
        /// <returns>The response from the user.</returns>
        private MessageResponse ConfirmImageDataOverwrite()
        {
            string confirmMessage = string.Empty;

            switch (ImageType)
            {
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    confirmMessage = string.Format(Resources.GORIMG_CONFIRM_OVERWRITE_ARRAY_INDEX, CurrentArrayIndex + 1, CurrentMipLevel + 1);
                    break;
                case ImageType.Image3D:
                    confirmMessage = string.Format(Resources.GORIMG_CONFIRM_OVERWRITE_DEPTH_SLICE, CurrentDepthSlice + 1, CurrentMipLevel + 1);
                    break;
            }

            return HostServices.MessageDisplay.ShowConfirmation(confirmMessage);
        }

        /// <summary>
        /// Function to check whether the image needs cropping or resizing.
        /// </summary>
        /// <param name="importImage">The imported image.</param>
        /// <param name="imageFileName">The name of the file being imported.</param>
        /// <returns><b>true</b> to cropping/resizing is required, <b>false</b> if not.</returns>
        private bool CheckForCropResize(IGorgonImage importImage, string imageFileName)
        {
            int arrayOrDepth = ImageType == ImageType.Image3D ? CurrentDepthSlice : CurrentArrayIndex;
            int width = ImageData.Buffers[CurrentMipLevel, arrayOrDepth].Width;
            int height = ImageData.Buffers[CurrentMipLevel, arrayOrDepth].Height;

            if ((width == importImage.Width) && (height == importImage.Height))
            {
                return false;
            }

            CurrentPanel = null;

            CropOrResizeSettings.AllowedModes = ((ImageData.Width < importImage.Width) || (ImageData.Height < importImage.Height)) ? (CropResizeMode.Crop | CropResizeMode.Resize) : CropResizeMode.Resize;
            if ((CropOrResizeSettings.CurrentMode & CropOrResizeSettings.AllowedModes) != CropOrResizeSettings.CurrentMode)
            {
                CropOrResizeSettings.CurrentMode = CropResizeMode.None;
            }
            else if ((CropOrResizeSettings.CurrentMode == CropResizeMode.None) && (((CropOrResizeSettings.AllowedModes) & (CropResizeMode.Crop)) == CropResizeMode.Crop))
            {
                CropOrResizeSettings.CurrentMode = CropResizeMode.Crop;
            }

            CropOrResizeSettings.ImportFile = imageFileName;
            // Take a copy of the image here because we'll need to destroy it later.
            CropOrResizeSettings.ImportImage = importImage.Clone();
            CropOrResizeSettings.TargetImageSize = new DX.Size2(width, height);
            CurrentPanel = CropOrResizeSettings;

            return true;
        }

        /// <summary>
        /// Function to import an image file into the current array index/depth slice.
        /// </summary>
        /// <param name="dpi">The DPI for the display.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoImportFileAsync(float dpi)
        {
            IReadOnlyList<string> selectedFiles = null;

            try
            {
                selectedFiles = ContentFileManager.GetSelectedFiles();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }

            await DoCopyToImageAsync(new CopyToImageArgs(selectedFiles, dpi));
        }

        /// <summary>
        /// Function to determine if the image dimensions editor can be shown.
        /// </summary>
        /// <returns><b>true</b> if the editor can be shown, <b>false</b> if not.</returns>
        private bool CanShowImageDimensions() => (ImageData != null) && (DimensionSettings?.UpdateImageInfoCommand != null) && (DimensionSettings.UpdateImageInfoCommand.CanExecute(ImageData))
                && (CurrentPanel == null) && (CommandContext == null);

        /// <summary>
        /// Function to determine if the mip map generation settings can be shown.
        /// </summary>
        /// <returns><b>true</b> if the settings can be shown, <b>false</b> if not.</returns>
        private bool CanShowMipGeneration() => (ImageData != null) && (MipSupport) && (MipMapSettings?.UpdateImageInfoCommand != null) && (MipMapSettings.UpdateImageInfoCommand.CanExecute(ImageData))
                && (CurrentPanel == null) && (CommandContext == null);

        /// <summary>
        /// Function to show or hide the image dimensions editor.
        /// </summary>
        private void DoShowImageDimensions()
        {
            try
            {
                if (CurrentPanel == DimensionSettings)
                {
                    CurrentPanel = null;
                    return;
                }

                DimensionSettings.UpdateImageInfoCommand.Execute(ImageData);
                DimensionSettings.MipSupport = MipSupport;
                CurrentPanel = DimensionSettings;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
        }

        /// <summary>
        /// Function to show or hide the mip map generation settings.
        /// </summary>
        private void DoShowMipGeneration()
        {
            try
            {
                if (CurrentPanel == MipMapSettings)
                {
                    return;
                }

                // Ensure that the user is aware we'll be wrecking their current mips.
                if ((ImageData.MipCount > 1) && (HostServices.MessageDisplay.ShowConfirmation(Resources.GORIMG_CONFIRM_GEN_MIPMAP) == MessageResponse.No))
                {
                    return;
                }

                MipMapSettings.UpdateImageInfoCommand?.Execute(ImageData);
                CurrentPanel = MipMapSettings;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
        }

        /// <summary>
        /// Function to determine if the current content can be saved.
        /// </summary>
        /// <param name="saveReason">The reason why the content is being saved.</param>
        /// <returns><b>true</b> if the content can be saved, <b>false</b> if not.</returns>
        private bool CanSaveImage(SaveReason saveReason) => ContentState != ContentState.Unmodified;

        /// <summary>
        /// Function to save the image back to the project file system.
        /// </summary>
        /// <param name="saveReason">The reason why the content is being saved.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoSaveImageTask(SaveReason saveReason)
        {
            IGorgonVirtualFile workFile = null;

            ShowWaitPanel(string.Format(Resources.GORIMG_TEXT_SAVING, File.Name));

            try
            {
                // Persist the image to a new working file so that block compression won't be applied to our current working file.   
                workFile = await Task.Run(() => _imageIO.SaveImageFile(Guid.NewGuid().ToString("N"), ImageData, CurrentPixelFormat));

                await SaveContentFileAsync(workFile);

                File.Metadata.Attributes[PremultipliedAttr] = IsPremultiplied.ToString(CultureInfo.InvariantCulture);

                File.Refresh();

                ContentState = ContentState.Unmodified;
            }
            catch (Exception ex)
            {
                if (saveReason != SaveReason.ContentShutdown)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_SAVE_CONTENT);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // Remove the temp working file.
                if (workFile != null)
                {
                    _imageIO.ScratchArea.DeleteFile(workFile.FullPath);
                }

                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function to determine if an undo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
        private bool CanUndo() => (_undoService.CanUndo) && (CurrentPanel == null) && (CommandContext == null);

        /// <summary>
        /// Function to determine if a redo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
        private bool CanRedo() => (_undoService.CanRedo) && (CurrentPanel == null) && (CommandContext == null);

        /// <summary>
        /// Function called when a redo operation is requested.
        /// </summary>
        private async void DoRedoAsync()
        {
            try
            {
                await _undoService.Redo();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_REDO);
            }
        }

        /// <summary>
        /// Function called when an undo operation is requested.
        /// </summary>
        private async void DoUndoAsync()
        {
            try
            {
                await _undoService.Undo();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UNDO);
            }
        }

        /// <summary>
        /// Function to retrieve the list of pixel formats applicable to each codec type.
        /// </summary>
        /// <returns>A list of formats.</returns>
        private ObservableCollection<BufferFormat> GetFilteredFormats()
        {
            var result = new ObservableCollection<BufferFormat>();
            IReadOnlyList<BufferFormat> supportedFormats = _imageIO.DefaultCodec.SupportedPixelFormats;

            if (ImageData.FormatInfo.IsCompressed)
            {
                // Assume our block compressed format expands to R8G8B8A8
                supportedFormats = BufferFormat.R8G8B8A8_UNorm.CanConvertToAny(supportedFormats);

                // Do not provide block compressed formats if we can't convert them.
                if (_imageIO.CanHandleBlockCompression)
                {
                    supportedFormats = supportedFormats
                        .Concat(supportedFormats.Where(item => (new GorgonFormatInfo(item)).IsCompressed))
                        .ToArray();
                }
            }

            foreach (BufferFormat format in supportedFormats.OrderBy(item => item.ToString()))
            {
                if ((!_formatSupport.TryGetValue(format, out IGorgonFormatSupportInfo supportInfo))
                    || (!supportInfo.IsTextureFormat(ImageData.ImageType))
                    || (supportInfo.IsDepthBufferFormat))
                {
                    continue;
                }

                var formatInfo = new GorgonFormatInfo(format);

                if ((formatInfo.IsPacked) || (formatInfo.IsTypeless))
                {
                    continue;
                }

                if ((!ImageData.FormatInfo.IsCompressed) && (!formatInfo.IsCompressed) && (!ImageData.CanConvertToFormat(format)))
                {
                    continue;
                }

                if ((!formatInfo.IsCompressed) && (!supportInfo.IsRenderTargetFormat))
                {
                    continue;
                }

                result.Add(format);
            }

            return result;
        }


        /// <summary>
        /// Function to determine if format conversion can happen.
        /// </summary>
        /// <param name="format">The format to change to.</param>
        /// <returns><b>true</b> if the conversion can take place, <b>false</b> if not.</returns>
        private bool CanConvertFormat(BufferFormat format) => format == BufferFormat.Unknown ? (CurrentPanel == null) && (CommandContext == null) : format != ImageData.Format;

        /// <summary>
        /// Function to create an undo cache file from the working file.
        /// </summary>
        /// <returns>The undo cache file.</returns>
        private IGorgonVirtualFile CreateUndoCacheFile()
        {
            string undoFilePath = Path.Combine(_undoCacheDir.FullPath, $"u_{Guid.NewGuid():N}");
            using (Stream outStream = _imageIO.ScratchArea.OpenStream(undoFilePath, FileMode.Create))
            using (Stream inStream = _workingFile.OpenStream())
            {
                inStream.CopyTo(outStream);
            }

            return _imageIO.ScratchArea.FileSystem.GetFile(undoFilePath);
        }

        /// <summary>
        /// Function to delete the temporary undo file.
        /// </summary>
        /// <param name="undoFile">The file holding the undo information.</param>
        private void DeleteUndoCacheFile(IGorgonVirtualFile undoFile)
        {
            // If we errored out, then delete the undo file.
            if (undoFile == null)
            {
                return;
            }

            try
            {
                _imageIO.ScratchArea.DeleteFile(undoFile.FullPath);
            }
            catch (Exception ex)
            {
                HostServices.Log.Print($"ERROR: Unable to delete the undo cache file at '{undoFile.PhysicalFile.FullPath}'.", LoggingLevel.Simple);
                HostServices.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to convert the image pixel format.
        /// </summary>
        /// <param name="format">The format to convert into.</param>
        private void DoConvertFormat(BufferFormat format)
        {
            ConvertUndoArgs convertUndoArgs = null;

            Task UndoAction(ConvertUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();

                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;
                    NotifyPropertyChanged(nameof(ImageData));

                    CurrentPixelFormat = undoArgs.Format;
                    ContentState = ContentState.Modified;

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORIMG_ERR_CONVERT_FORMAT, format));
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(ConvertUndoArgs redoArgs, CancellationToken cancelToken)
            {
                GorgonFormatInfo srcFormat = ImageData.FormatInfo;
                IGorgonVirtualFile undoFile = null;

                try
                {
                    HostServices.BusyService.SetBusy();

                    // Don't convert the same format.
                    if (format == ImageData.Format)
                    {
                        return Task.FromException(new InvalidCastException("Will not see me"));
                    }

                    var destFormat = new GorgonFormatInfo(format);

                    // Ensure that we can actually convert.
                    if (!destFormat.IsCompressed)
                    {
                        if (!ImageData.CanConvertToFormat(format))
                        {
                            string message = string.Format(Resources.GORIMG_ERR_CANNOT_CONVERT, srcFormat.Format, format);
                            HostServices.MessageDisplay.ShowError(message);
                            return Task.FromException(new InvalidCastException(message));
                        }
                    }
                    else if (!_imageIO.CanHandleBlockCompression)
                    {
                        string message = string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format);
                        HostServices.MessageDisplay.ShowError(message);
                        return Task.FromException(new InvalidCastException(message));
                    }

                    undoFile = CreateUndoCacheFile();
                    NotifyPropertyChanging(nameof(ImageData));
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, format);
                    NotifyPropertyChanged(nameof(ImageData));

                    if (redoArgs == null)
                    {
                        redoArgs = convertUndoArgs = new ConvertUndoArgs();
                    }

                    redoArgs.Format = CurrentPixelFormat;
                    redoArgs.UndoFile = undoFile;
                    CurrentPixelFormat = format;

                    ContentState = ContentState.Modified;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORIMG_ERR_CONVERT_FORMAT, format));
                    DeleteUndoCacheFile(undoFile);
                    convertUndoArgs = null;
                    return Task.FromException(ex);
                }
                finally
                {
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(string.Format(Resources.GORIMG_UNDO_DESC_FORMAT_CONVERT, format), UndoAction, RedoAction, convertUndoArgs, convertUndoArgs);
            }
        }

        /// <summary>
        /// Function to determine if the image can be exported.
        /// </summary>
        /// <param name="_">Not used.</param>
        /// <returns><b>true</b> if the image can be exported, <b>false</b> if not.</returns>
        private bool CanExportImage(IGorgonImageCodec _) => (CurrentPanel == null) && (CommandContext == null) && (_codecs.Count > 0);

        /// <summary>
        /// Function to export this image to the physical file system.
        /// </summary>
        /// <param name="codec">The codec to use when exporting.</param>
        private void DoExportImage(IGorgonImageCodec codec)
        {
            var missingSupport = new StringBuilder();
            IGorgonImage exportImage = ImageData.Clone();
            IGorgonImage workImage = exportImage;

            try
            {
                if (!codec.SupportedPixelFormats.Contains(ImageData.Format))
                {
                    missingSupport.AppendFormat("{0} {1}", Resources.GORIMG_TEXT_IMAGE_FORMAT, CurrentPixelFormat);
                    // Convert to the first supported format.
                    BufferFormat convertFormat = BufferFormat.Unknown;

                    foreach (BufferFormat format in codec.SupportedPixelFormats)
                    {
                        if (workImage.CanConvertToFormat(format))
                        {
                            convertFormat = format;
                            break;
                        }
                    }

                    if (convertFormat == BufferFormat.Unknown)
                    {
                        HostServices.MessageDisplay.ShowError(Resources.GORIMG_ERR_EXPORT_CONVERT);
                        return;
                    }

                    workImage.ConvertToFormat(convertFormat);
                }

                if ((!codec.SupportsMultipleFrames) && (ArrayCount > 1))
                {
                    if (missingSupport.Length > 0)
                    {
                        missingSupport.Append("\r\n");
                    }

                    missingSupport.Append(Resources.GORIMG_TEXT_ARRAY_INDICES);
                    exportImage = _imageUpdater.GetArrayIndexAsImage(workImage, CurrentArrayIndex);
                    workImage.Dispose();
                    workImage = exportImage;
                }
                else if ((!codec.SupportsDepth) && (workImage.ImageType == ImageType.Image3D))
                {
                    if (missingSupport.Length > 0)
                    {
                        missingSupport.Append("\r\n");
                    }

                    missingSupport.Append(Resources.GORIMG_TEXT_DEPTH_SLICES);
                    exportImage = _imageUpdater.GetDepthSliceAsImage(workImage, CurrentDepthSlice);
                    workImage.Dispose();
                    workImage = exportImage;
                }

                if ((!codec.SupportsMipMaps) && (MipCount > 1))
                {
                    if (missingSupport.Length > 0)
                    {
                        missingSupport.Append("\r\n");
                    }

                    missingSupport.Append(Resources.GORIMG_TEXT_MIP_MAPS);
                    exportImage = _imageUpdater.GetMipLevelAsImage(workImage, CurrentMipLevel);
                    workImage.Dispose();
                    workImage = exportImage;
                }

                if (missingSupport.Length > 0)
                {
                    if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORIMG_CONFIRM_EXPORT_LIMITS, codec.CodecDescription, missingSupport)) == MessageResponse.No)
                    {
                        return;
                    }
                }

                FileInfo exportedFile = _imageIO.ExportImage(File, exportImage, codec);

                if (exportedFile == null)
                {
                    return;
                }

                _settings.LastImportExportPath = exportedFile.Directory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CODEC_ASSIGN);
            }
            finally
            {
                exportImage?.Dispose();
                HostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if an image can be cropped/resized.
        /// </summary>
        /// <returns><b>true</b> if the image can be cropped or resized, <b>false</b> if not.</returns>
        private bool CanCropResize() => CropOrResizeSettings.ImportImage != null;

        /// <summary>
        /// Function to perform a cropping/resizing operation.
        /// </summary>
        private void DoCropResize()
        {
            IGorgonImage image = CropOrResizeSettings.ImportImage;

            try
            {
                ImportImageData(CropOrResizeSettings.ImportFile, image, CropOrResizeSettings.CurrentMode,
                    CropOrResizeSettings.CurrentMode == CropResizeMode.Resize ? Alignment.UpperLeft : CropOrResizeSettings.CurrentAlignment,
                    CropOrResizeSettings.ImageFilter, CropOrResizeSettings.PreserveAspect);

                if ((!string.IsNullOrWhiteSpace(CropOrResizeSettings.ImportFileDirectory)) && (Directory.Exists(CropOrResizeSettings.ImportFileDirectory)))
                {
                    _settings.LastImportExportPath = CropOrResizeSettings.ImportFileDirectory;
                }
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                image.Dispose();
                CropOrResizeSettings.ImportImage = null;
                CurrentPanel = null;
            }
        }

        /// <summary>
        /// Function to determine if an image can be cropped/resized.
        /// </summary>
        /// <returns><b>true</b> if the image can be cropped or resized, <b>false</b> if not.</returns>
        private bool CanUpdateDimensions()
        {
            if (ImageData == null)
            {
                return false;
            }

            switch (ImageData.ImageType)
            {
                case ImageType.Image3D:
                    return (ImageData.Width != DimensionSettings.Width)
                        || (ImageData.Height != DimensionSettings.Height)
                        || (ImageData.MipCount != DimensionSettings.MipLevels)
                        || (ImageData.Depth != DimensionSettings.DepthSlicesOrArrayIndices);
                default:
                    return (ImageData.Width != DimensionSettings.Width)
                        || (ImageData.Height != DimensionSettings.Height)
                        || (ImageData.MipCount != DimensionSettings.MipLevels)
                        || (ImageData.ArrayCount != DimensionSettings.DepthSlicesOrArrayIndices);
            }
        }

        /// <summary>
        /// Function to update the image dimensions.
        /// </summary>
        private void DoUpdateImageDimensions()
        {
            ImportDimensionUndoArgs dimensionUndoArgs = null;

            Task UndoAction(ImportDimensionUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }                    

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();
                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;

                    if (CurrentMipLevel >= MipCount)
                    {
                        CurrentMipLevel = MipCount - 1;
                    }

                    // Ensure we don't exceed the ranges for our dimensions.
                    if (CurrentArrayIndex >= ArrayCount)
                    {
                        CurrentArrayIndex = ArrayCount - 1;
                    }

                    if (CurrentDepthSlice >= DepthCount)
                    {
                        CurrentDepthSlice = DepthCount - 1;
                    }

                    NotifyPropertyChanged(nameof(Width));
                    NotifyPropertyChanged(nameof(Height));
                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(ImportDimensionUndoArgs redoArgs, CancellationToken cancelToken)
            {
                Stream redoFileStream = null;
                IGorgonVirtualFile undoFile = null;
                IGorgonVirtualFile redoFile = null;
                IGorgonImage newImage = null;
                int arrayOrDepthCount = ImageData.ImageType == ImageType.Image3D ? ImageData.Depth : ImageData.ArrayCount;

                try
                {
                    var formatInfo = new GorgonFormatInfo(CurrentPixelFormat);

                    // Block compressed files must be a multiple of 4 for width and height. 
                    if ((formatInfo.IsCompressed) &&
                        (((DimensionSettings.Width % 4) != 0) || ((DimensionSettings.Height % 4) != 0)))
                    {
                        HostServices.MessageDisplay.ShowError(string.Format(Resources.GORIMG_ERR_COMPRESSED_RESIZE_MULTIPLE_4, CurrentPixelFormat, DimensionSettings.Width, DimensionSettings.Height));
                        return Task.FromException(new OperationCanceledException());
                    }

                    if ((redoArgs?.RedoFile == null) && ((((DimensionSettings.Width < ImageData.Width) || (DimensionSettings.Height < ImageData.Height))
                            && (DimensionSettings.CurrentMode != CropResizeMode.Resize))
                        || (DimensionSettings.DepthSlicesOrArrayIndices < arrayOrDepthCount)
                        || (DimensionSettings.MipLevels < ImageData.MipCount)))
                    {
                        if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORIMG_CROP_LOSE_DATA) == MessageResponse.No)
                        {
                            return Task.FromException(new OperationCanceledException());
                        }
                    }

                    HostServices.BusyService.SetBusy();

                    undoFile = CreateUndoCacheFile();

                    if (redoArgs?.RedoFile != null)
                    {
                        // Just reuse the image data in the redo cache item.                        
                        redoFileStream = redoArgs.RedoFile.OpenStream();
                        (IGorgonImage redoImage, _, _) = _imageIO.LoadImageFile(redoFileStream, _workingFile.Name);
                        redoFileStream.Dispose();

                        ImageData.Dispose();
                        NotifyPropertyChanging(nameof(ImageData));
                        ImageData = redoImage;
                        NotifyPropertyChanged(nameof(ImageData));

                        DeleteUndoCacheFile(redoArgs.RedoFile);
                    }
                    else
                    {
                        // Handle width/height.
                        if (DimensionSettings.CurrentMode != CropResizeMode.Resize)
                        {
                            if ((DimensionSettings.Width > ImageData.Width)
                                || (DimensionSettings.Height > ImageData.Height))
                            {

                                ImageData.Expand(DimensionSettings.Width, DimensionSettings.Height, DepthCount,
                                    (ImageExpandAnchor)DimensionSettings.CropAlignment);
                            }

                            if ((DimensionSettings.Width <= ImageData.Width)
                                || (DimensionSettings.Height <= ImageData.Height))
                            {
                                _imageUpdater.CropTo(ImageData, new DX.Size2(DimensionSettings.Width, DimensionSettings.Height), DimensionSettings.CropAlignment);
                            }
                        }
                        else
                        {
                            _imageUpdater.Resize(ImageData, new DX.Size2(DimensionSettings.Width, DimensionSettings.Height), DimensionSettings.ImageFilter, false);
                        }

                        // Handle depth slices/array indices.
                        if (DimensionSettings.DepthSlicesOrArrayIndices != arrayOrDepthCount)
                        {
                            newImage = _imageUpdater.ChangeArrayOrDepthCount(ImageData, DimensionSettings.DepthSlicesOrArrayIndices);

                            if (newImage != ImageData)
                            {
                                ImageData?.Dispose();
                                NotifyPropertyChanging(nameof(ImageData));
                                ImageData = newImage;
                            }
                        }

                        // Handle mip levels.
                        if (DimensionSettings.MipLevels != ImageData.MipCount)
                        {
                            newImage = _imageUpdater.ChangeMipCount(ImageData, DimensionSettings.MipLevels);
                            if (newImage != ImageData)
                            {
                                ImageData?.Dispose();
                                NotifyPropertyChanging(nameof(ImageData));
                                ImageData = newImage;
                            }
                        }
                    }

                    // Ensure we aren't on a mip level that doesn't exist.
                    if (CurrentMipLevel >= MipCount)
                    {
                        CurrentMipLevel = MipCount - 1;
                    }

                    // Update the current values to fit within our updated ranges.
                    if (ImageType == ImageType.Image3D)
                    {
                        if (CurrentDepthSlice >= DepthCount)
                        {
                            CurrentDepthSlice = DepthCount - 1;
                        }
                    }
                    else
                    {
                        if (CurrentArrayIndex >= ArrayCount)
                        {
                            CurrentArrayIndex = ArrayCount - 1;
                        }
                    }

                    NotifyPropertyChanged(nameof(Width));
                    NotifyPropertyChanged(nameof(Height));
                    NotifyImageUpdated(nameof(ImageData));

                    // Save the updated data to the working file.
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);
                    redoFile = CreateUndoCacheFile();

                    if (redoArgs == null)
                    {
                        redoArgs = dimensionUndoArgs = new ImportDimensionUndoArgs();
                    }

                    redoArgs.UndoFile = undoFile;
                    redoArgs.RedoFile = redoFile;

                    CurrentPanel = null;

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);

                    if (newImage != ImageData)
                    {
                        newImage?.Dispose();
                    }

                    if (undoFile != null)
                    {
                        DeleteUndoCacheFile(undoFile);
                    }

                    if (redoFile != null)
                    {
                        redoFileStream?.Dispose();
                        DeleteUndoCacheFile(redoFile);
                    }

                    dimensionUndoArgs = null;

                    return Task.FromException(ex);
                }
                finally
                {
                    redoFileStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(Resources.GORIMG_UNDO_DESC_DIMENSIONS, UndoAction, RedoAction, dimensionUndoArgs, dimensionUndoArgs);
                // Need to call this so the UI can register our updated undo stack.
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to determine if mip maps can be generated.
        /// </summary>
        /// <returns><b>true</b> if the image can be cropped or resized, <b>false</b> if not.</returns>
        private bool CanGenMips() => ImageData != null && MipSupport;

        /// <summary>
        /// Function to generate mip maps for the current image.
        /// </summary>
        private void DoGenMips()
        {
            ImportDimensionUndoArgs mipGenUndoArgs = null;

            Task UndoAction(ImportDimensionUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    NotifyPropertyChanging(nameof(ImageData));

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();
                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;

                    if (CurrentMipLevel >= MipCount)
                    {
                        CurrentMipLevel = MipCount - 1;
                    }

                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(ImportDimensionUndoArgs redoArgs, CancellationToken cancelToken)
            {
                Stream redoFileStream = null;
                IGorgonVirtualFile undoFile = null;
                IGorgonVirtualFile redoFile = null;

                try
                {
                    HostServices.BusyService.SetBusy();

                    undoFile = CreateUndoCacheFile();

                    NotifyPropertyChanging(nameof(ImageData));

                    int prevMipCount = MipCount;

                    if (redoArgs?.RedoFile != null)
                    {
                        // Just reuse the image data in the redo cache item.                        
                        redoFileStream = redoArgs.RedoFile.OpenStream();
                        (IGorgonImage redoImage, _, _) = _imageIO.LoadImageFile(redoFileStream, _workingFile.Name);
                        redoFileStream.Dispose();

                        ImageData.Dispose();
                        ImageData = redoImage;

                        DeleteUndoCacheFile(redoArgs.RedoFile);
                    }
                    else
                    {
                        int mipCount = MipMapSettings.MipLevels;
                        int currentMip = CurrentMipLevel.Min(mipCount - 1).Max(0);

                        ImageData.GenerateMipMaps(mipCount, MipMapSettings.MipFilter);

                        CurrentMipLevel = currentMip;
                    }

                    // Save the updated data to the working file.
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);
                    redoFile = CreateUndoCacheFile();

                    if (redoArgs == null)
                    {
                        redoArgs = mipGenUndoArgs = new ImportDimensionUndoArgs();
                    }

                    redoArgs.UndoFile = undoFile;
                    redoArgs.RedoFile = redoFile;

                    if (MipCount != prevMipCount)
                    {
                        NotifyPropertyChanged(nameof(MipCount));
                    }

                    NotifyPropertyChanged(nameof(ImageData));
                    ContentState = ContentState.Modified;

                    CurrentPanel = null;

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);

                    if (undoFile != null)
                    {
                        DeleteUndoCacheFile(undoFile);
                    }

                    if (redoFile != null)
                    {
                        redoFileStream?.Dispose();
                        DeleteUndoCacheFile(redoFile);
                    }

                    mipGenUndoArgs = null;

                    return Task.FromException(ex);
                }
                finally
                {
                    redoFileStream?.Dispose();
                    HostServices.BusyService.SetIdle();                    
                }
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(Resources.GORIMG_UNDO_DESC_MIP_GEN, UndoAction, RedoAction, mipGenUndoArgs, mipGenUndoArgs);
                // Need to call this so the UI can register our updated undo stack.
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to determine if the image change be changed to the specified type.
        /// </summary>
        /// <param name="imageType">The type of image.</param>
        /// <returns><b>true</b> if the image can change types, <b>false</b> if not.</returns>
        private bool CanChangeImageType(ImageType imageType)
        {
            if ((CurrentPanel != null) || (CommandContext != null))
            {
                return false;
            }

            if (!_formatSupport.ContainsKey(CurrentPixelFormat))
            {
                return false;
            }

            switch (imageType)
            {
                case ImageType.ImageCube when ((_formatSupport[CurrentPixelFormat].FormatSupport & BufferFormatSupport.TextureCube) != BufferFormatSupport.TextureCube):
                case ImageType.Image3D when ((Width > _videoAdapter.MaxTexture3DWidth) || (Height > _videoAdapter.MaxTexture3DHeight)):
                    return false;
                case ImageType.Image2D when ImageType == ImageType.ImageCube:
                case ImageType.ImageCube when ImageType == ImageType.Image2D:
                    return true;
                default:
                    return _formatSupport[CurrentPixelFormat].IsTextureFormat(ImageType);
            }
        }

        /// <summary>
        /// Function to convert the image type to another type.
        /// </summary>
        /// <param name="newImageType">The type to convert into.</param>
        private void DoChangeImageType(ImageType newImageType)
        {
            ImageTypeUndoArgs imageTypeUndoArgs = null;

            if (newImageType == ImageType)
            {
                return;
            }

            Task UndoAction(ImageTypeUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();

                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;
                    NotifyImageUpdated(nameof(ImageType));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CHANGE_TYPE);
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(ImageTypeUndoArgs redoArgs, CancellationToken cancelToken)
            {
                IGorgonVirtualFile undoFile = null;
                IGorgonImage newImage;

                try
                {
                    switch (newImageType)
                    {
                        case ImageType.ImageCube:
                        case ImageType.Image2D:
                            if ((DepthCount > 1)
                                && (HostServices.MessageDisplay.ShowConfirmation(Resources.GORIMG_CONFIRM_3D_TO_2D) == MessageResponse.No))
                            {
                                return Task.CompletedTask;
                            }

                            HostServices.BusyService.SetBusy();
                            newImage = _imageUpdater.ConvertTo2D(ImageData, newImageType == ImageType.ImageCube);
                            break;
                        case ImageType.Image3D:
                            if ((ArrayCount > 1)
                                && (HostServices.MessageDisplay.ShowConfirmation(Resources.GORIMG_CONFIRM_ARRAY_TO_VOLUME) == MessageResponse.No))
                            {
                                return Task.CompletedTask;
                            }

                            HostServices.BusyService.SetBusy();
                            newImage = _imageUpdater.ConvertToVolume(ImageData);
                            break;
                        default:
                            return Task.CompletedTask;
                    }

                    ImageData.Dispose();
                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = newImage;

                    undoFile = CreateUndoCacheFile();
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);

                    if (redoArgs == null)
                    {
                        redoArgs = imageTypeUndoArgs = new ImageTypeUndoArgs();
                    }

                    redoArgs.ImageType = newImageType;
                    redoArgs.UndoFile = undoFile;

                    NotifyPropertyChanging(nameof(ImageType));
                    NotifyImageUpdated(nameof(ImageType));
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CHANGE_TYPE);
                    DeleteUndoCacheFile(undoFile);
                    imageTypeUndoArgs = null;
                    return Task.FromException(ex);
                }
                finally
                {
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(string.Format(Resources.GORIMG_UNDO_DESC_IMAGE_TYPE, newImageType), UndoAction, RedoAction, imageTypeUndoArgs, imageTypeUndoArgs);
            }
        }

        /// <summary>
        /// Function to determine if the image can be edited in an external application.
        /// </summary>
        /// <returns></returns>
        private bool CanEditInApp() => (CurrentPanel == null) && (CommandContext == null);

        /// <summary>
        /// Function to edit the current image in an application.
        /// </summary>
        private void DoEditInApp()
        {
            IGorgonImage workImage = null;
            IGorgonVirtualFile workImageFile = null;
            Stream imageStream = null;

            HostServices.BusyService.SetBusy();

            try
            {
                // We will extract the buffer for the current array/depth and mip and edit that.
                // Most apps won't support editing images with volume data, array data, or mip levels, so we'll let them work on the image part one at a time.
                // This should give us the most flexibility.
                IGorgonImageBuffer buffer = ImageData.Buffers[CurrentMipLevel, ImageType == ImageType.Image3D ? CurrentDepthSlice : CurrentArrayIndex];

                workImage = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, ImageData.Format)
                {
                    Depth = 1,
                    ArrayCount = 1,
                    MipCount = 1,
                    Width = buffer.Width,
                    Height = buffer.Height
                });

                buffer.CopyTo(workImage.Buffers[0]);

                // Get the PNG codec, this is a pretty common format and should be supported by all image editors.
                // The only issue we may run into is if the format of the image is not supported by the codec, in that case we can try to convert it.
                IGorgonImageCodec codec = _imageIO.InstalledCodecs.Codecs.First(item => item is GorgonCodecPng);

                // Doesn't support our format, so convert it.
                if (!codec.SupportedPixelFormats.Contains(workImage.Format))
                {
                    HostServices.Log.Print($"Image pixel format is [{workImage.Format}], but PNG codec does not support this format. Image will be converted to [{BufferFormat.B8G8R8A8_UNorm}].", LoggingLevel.Verbose);
                    if (!workImage.CanConvertToFormat(BufferFormat.B8G8R8A8_UNorm))
                    {
                        HostServices.MessageDisplay.ShowError(string.Format(Resources.GORIMG_ERR_EXPORT_CONVERT, workImage.Format));
                        return;
                    }

                    workImage.ConvertToFormat(BufferFormat.B8G8R8A8_UNorm);
                }

                // Finally, write out the image data so the external image editor can have at it.
                string fileName = $"{File.Name}_extern_edit.png";
                workImageFile = _imageIO.SaveImageFile(fileName, workImage, workImage.Format, codec);

                workImage.Dispose();

                // Launch the editor.
                if (!_externalEditor.EditImage(workImageFile))
                {
                    return;
                }

                // Reload the image.
                imageStream = workImageFile.OpenStream();
                workImage = codec.LoadFromStream(imageStream);
                imageStream.Dispose();

                if (workImage.Format != ImageData.Format)
                {
                    HostServices.Log.Print($"Image pixel format changed.  Is now [{workImage.Format}], will be converted to [{ImageData.Format}] to match current format.", LoggingLevel.Verbose);
                    if (!workImage.CanConvertToFormat(ImageData.Format))
                    {
                        HostServices.MessageDisplay.ShowError(string.Format(Resources.GORIMG_ERR_EXPORT_CONVERT, workImage.Format));
                        return;
                    }

                    workImage.ConvertToFormat(ImageData.Format);
                }

                CropOrResizeSettings.ImportFileDirectory = null;
                if (CheckForCropResize(workImage, workImageFile.Name))
                {
                    return;
                }

                ImportImageData(workImageFile.Name, workImage, CropResizeMode.None, Alignment.UpperLeft, ImageFilter.Point, false);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                imageStream?.Dispose();

                // Clean up after ourselves.
                if (workImageFile != null)
                {
                    try
                    {
                        _imageIO.ScratchArea.DeleteFile(workImageFile.FullPath);
                    }
                    catch (Exception ex)
                    {
                        HostServices.Log.Print("Error deleting working image file.", LoggingLevel.All);
                        HostServices.Log.LogException(ex);
                    }
                }

                workImage?.Dispose();
                HostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if the file can be imported into the image.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns><b>true</b> if the file can be imported, <b>false</b> if not.</returns>
        private bool CanCopyToImage(CopyToImageArgs args)
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if ((args?.ContentFilePaths == null) || (args.ContentFilePaths.Count == 0) || (CurrentPanel != null) || (CommandContext == FxContext))
            {
                args.Cancel = true;
                return false;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return args.ContentFilePaths.Select(file => ContentFileManager.GetFile(file))
                        .Where(file => file != null)
                        .Any(file => (file.Metadata.ContentMetadata != null)
                                    && (file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string dataType))
                                    && (string.Equals(dataType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Function to import a single image with the same size as the target image.
        /// </summary>
        /// <param name="importItem">The image item to import.</param>
        private async Task ImportSingleImageSameSizeAsync(ImagePickerImportData importItem)
        {
            if (ConfirmImageDataOverwrite() == MessageResponse.No)
            {
                return;
            }

            IGorgonImage importImage = null;

            // Get the image we're importing.    
            ShowWaitPanel(Resources.GORIMG_TEXT_IMPORTING);
            await Task.Run(() =>
            {
                var ddsCodec = new GorgonCodecDds();

                using (Stream imageStream = _imageIO.ScratchArea.OpenStream(importItem.FromFile.FullPath, FileMode.Open))
                {
                    (importImage, _, _) = _imageIO.LoadImageFile(imageStream, $"/Import/Import_{importItem.FromFile.Name}_{Guid.NewGuid():N}");
                }

                importImage.ConvertToFormat(ImageData.Format);
            });
            
            ImportImageData(Path.GetFileName(importItem.OriginalFilePath), importImage, CropResizeMode.None, Alignment.UpperLeft, ImageFilter.Point, false);
            HideWaitPanel();

            _imageIO.ScratchArea.DeleteFile(importItem.FromFile.FullPath);
        }

        /// <summary>
        /// Function to determine if the import process can be finalized or not.
        /// </summary>
        /// <returns><b>true</b> if the process can be finalized, or <b>false</b> if not.</returns>
        private bool CanFinalizeImport() => (ImagePicker.ChangedSubResources.Count > 0) && (!ImagePicker.NeedsTransformation) && (!ImagePicker.SourceHasMultipleSubresources);

        /// <summary>
        /// Function to finalize the import process.
        /// </summary>
        private void DoFinalizeImport()
        {
            ImportDimensionUndoArgs importUndoArgs = null;

            Task UndoAction(ImportDimensionUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();

                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;
                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(ImportDimensionUndoArgs redoArgs, CancellationToken cancelToken)
            {
                IGorgonVirtualFile undoFile = null;
                IGorgonVirtualFile redoFile = null;
                Stream redoFileStream = null;

                try
                {
                    HostServices.BusyService.SetBusy();

                    NotifyPropertyChanging(nameof(ImageData));

                    undoFile = CreateUndoCacheFile();
                    if (redoArgs?.RedoFile != null)
                    {
                        // Just reuse the image data in the redo cache item.                        
                        redoFileStream = redoArgs.RedoFile.OpenStream();
                        (IGorgonImage redoImage, _, _) = _imageIO.LoadImageFile(redoFileStream, _workingFile.Name);
                        redoFileStream.Dispose();

                        NotifyPropertyChanging(nameof(ImageData));
                        ImageData.Dispose();
                        ImageData = redoImage;
                        NotifyPropertyChanged(nameof(ImageData));

                        DeleteUndoCacheFile(redoArgs.RedoFile);
                    }
                    else
                    {
                        NotifyPropertyChanging(nameof(ImageData));
                        ImageData.Dispose();
                        ImageData = ImagePicker.ImageData.Clone();
                        NotifyPropertyChanged(nameof(ImageData));
                    }

                    // Save the updated data to the working file.
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);
                    redoFile = CreateUndoCacheFile();

                    NotifyImageUpdated(nameof(ImageData));

                    if (redoArgs == null)
                    {
                        redoArgs = importUndoArgs = new ImportDimensionUndoArgs();
                    }

                    redoArgs.RedoFile = redoFile;
                    redoArgs.UndoFile = undoFile;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                    if (undoFile != null)
                    {
                        DeleteUndoCacheFile(undoFile);
                    }

                    if (redoFile != null)
                    {
                        redoFileStream?.Dispose();
                        DeleteUndoCacheFile(redoFile);
                    }

                    importUndoArgs = null;
                    return Task.FromException(ex);
                }
                finally
                {
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(Resources.GORIMG_UNDO_DESC_IMPORT_SOURCE, UndoAction, RedoAction, importUndoArgs, importUndoArgs);
                // Need to call this so the UI can register our updated undo stack.
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to copy an image into the currently selected array/depth/mip index.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoCopyToImageAsync(CopyToImageArgs args)
        {
            IGorgonImage importImage = null;
            IGorgonVirtualFile tempFile = null;
            Stream imageStream = null;
            var cancelSource = new CancellationTokenSource();            
            IReadOnlyList<ImagePickerImportData> imports = Array.Empty<ImagePickerImportData>();

            void CancelAction() => cancelSource?.Cancel();

            try
            {
                // Get our list of image files.
                IContentFile[] imageFiles = args.ContentFilePaths.Select(imageFile => ContentFileManager.GetFile(imageFile))
                                                                 .Where(imageFile => (imageFile?.Metadata.ContentMetadata != null)
                                                                        && (imageFile.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string dataType))
                                                                        && (string.Equals(dataType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
                                                                 .ToArray();

                UpdateProgress(imageFiles[0].Path.Ellipses(45, true), 0, Resources.GORIMG_TEXT_LOADING_IMAGES, CancelAction);

                imports = await Task.Run(() =>
                {
                    Stream stream = null;
                    var importedFiles = new List<ImagePickerImportData>();
                    IGorgonImageInfo originalMetadata;

                    _imageIO.ScratchArea.CreateDirectory("/Import/");

                    for (int i = 0; i < imageFiles.Length; ++i)
                    {
                        IContentFile imageFile = imageFiles[i];
                        if (cancelSource.Token.IsCancellationRequested)
                        {
                            return null;
                        }

                        try
                        {
                            UpdateProgress(imageFile.Path.Ellipses(45, true), (float)i / imageFiles.Length, Resources.GORIMG_TEXT_IMPORTING, CancelAction);

                            stream = ContentFileManager.OpenStream(imageFile.Path, FileMode.Open);
                            (importImage, tempFile, originalMetadata) = _imageIO.LoadImageAsThumbnail(stream, $"/Import/Thumb_{imageFile.Name}_{Guid.NewGuid():N}", 128, args.ThumbnailDpi);
                            stream.Close();

                            importedFiles.Add(new ImagePickerImportData(imageFile.Path, tempFile, importImage, originalMetadata));

                            UpdateProgress(imageFile.Path.Ellipses(45, true), (float)(i + 1) / imageFiles.Length, Resources.GORIMG_TEXT_IMPORTING, CancelAction);

                            Thread.Sleep(16);
                        }
                        finally
                        {
                            stream?.Close();
                        }                        
                    }

                    return importedFiles;
                }, cancelSource.Token);

                HideProgress();

                if (cancelSource.IsCancellationRequested)
                {
                    return;
                }

                // If we've only imported a single image, and its width/height is the same as this image and depth/array/mip count is 1 for both, then there's no need for the picker.
                // So just straight up import it.
                if ((imports.Count == 1) 
                    && (imports[0].OriginalMetadata.Width == Width) 
                    && (imports[0].OriginalMetadata.Height == Height) 
                    && (imports[0].OriginalMetadata.ArrayCount == 1)
                    && (imports[0].OriginalMetadata.Depth == 1)
                    && (imports[0].OriginalMetadata.MipCount == 1))
                {                    
                    await ImportSingleImageSameSizeAsync(imports[0]);
                    return;
                }

                // If we have more than 1 image selected, or the import image is not the same size as the destination image, then we can use the 
                // picker to update the image.
                var imgPickerArgs = new ActivateImagePickerArgs(imports, ImageData)
                {
                    CurrentArrayIndexDepthSlice = ImageType == ImageType.Image3D ? _currentDepthSlice : _currentArrayindex,
                    MipLevel = _currentMipLevel
                };
                if ((ImagePicker.ActivateCommand == null) || (!ImagePicker.ActivateCommand.CanExecute(imgPickerArgs)))
                {
                    return;
                }

                ImagePicker.ActivateCommand.Execute(imgPickerArgs);

                if (imgPickerArgs.Cancel)
                {
                    return;
                }

                HostServices.BusyService.SetBusy();

                // Blow away any files we generated to keep the system clean.
                foreach (ImagePickerImportData thumbnail in imports)
                {
                    if (thumbnail?.FromFile == null)
                    {
                        continue;
                    }

                    _imageIO.ScratchArea.DeleteFile(thumbnail.FromFile.FullPath);
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing.
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                // Ensure our thumbnails are cleaned up.
                foreach (ImagePickerImportData thumb in imports)
                {
                    thumb?.Thumbnail?.Dispose();
                }

                HostServices.BusyService.SetIdle();
                HideProgress();
                HideWaitPanel();
                imageStream?.Dispose();

                if ((tempFile != null) && (_imageIO.ScratchArea.FileSystem.GetFile(tempFile.FullPath) != null))
                {                                        
                    _imageIO.ScratchArea.DeleteFile(tempFile.FullPath);
                }

                importImage?.Dispose();
                HostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to build the list of codecs that support the current image.
        /// </summary>
        /// <param name="image">The image to evaluate.</param>
        private void BuildCodecList(IGorgonImage image)
        {
            HostServices.Log.Print("Building codec list for the current image.", LoggingLevel.Verbose);

            _codecs.Clear();
            Codecs.Clear();

            if (image == null)
            {
                HostServices.Log.Print("WARNING: No image was found.", LoggingLevel.Simple);
                return;
            }

            if (_imageIO.InstalledCodecs.Codecs.Count == 0)
            {
                HostServices.Log.Print("WARNING: No image codecs were found.  This should not happen.", LoggingLevel.Simple);
                return;
            }

            foreach (IGorgonImageCodec codec in _imageIO.InstalledCodecs.Codecs.Where(item => item.CanEncode).OrderBy(item => item.Codec))
            {
                HostServices.Log.Print($"Adding codec '{codec.CodecDescription} ({codec.Codec})'", LoggingLevel.Verbose);
                Codecs.Add(codec);
                foreach (string extension in codec.CodecCommonExtensions)
                {
                    _codecs.Add((new GorgonFileExtension(extension), codec));
                }
            }
        }

        /// <summary>
        /// Function to determine if the set alpha settings panel can be shown or not.
        /// </summary>
        /// <returns></returns>
        private bool CanShowSetAlphaValue() => (ImageData != null)
                && (CurrentPanel == null)
                && (CommandContext == null)
                && (ImageData.FormatInfo.HasAlpha)
                && (ImageData.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm));

        /// <summary>
        /// Function to show the set alpha settings panel.
        /// </summary>
        private void DoShowSetAlphaValue()
        {
            try
            {
                if (CurrentPanel == AlphaSettings)
                {
                    return;
                }

                CurrentPanel = AlphaSettings;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
        }

        /// <summary>
        /// Function to set the alpha value for the image.
        /// </summary>
        private void DoSetAlphaValue()
        {
            SetAlphaUndoArgs alphaUndoArgs = null;

            Task UndoAction(SetAlphaUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    NotifyPropertyChanging(nameof(ImageData));

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();
                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;

                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(SetAlphaUndoArgs redoArgs, CancellationToken cancelToken)
            {
                Stream redoFileStream = null;
                IGorgonVirtualFile undoFile = null;
                IGorgonVirtualFile redoFile = null;
                IGorgonImage newImage = null;

                try
                {
                    HostServices.BusyService.SetBusy();

                    undoFile = CreateUndoCacheFile();

                    NotifyPropertyChanging(nameof(ImageData));

                    if (redoArgs == null)
                    {
                        redoArgs = alphaUndoArgs = new SetAlphaUndoArgs()
                        {
                            Alpha = AlphaSettings.AlphaValue,
                            MinMax = AlphaSettings.UpdateRange,
                            MipLevel = CurrentMipLevel,
                            ArrayIndex = ImageType == ImageType.Image3D ? CurrentDepthSlice : CurrentArrayIndex
                        };

                        _settings.LastAlphaValue = redoArgs.Alpha;
                        _settings.LastAlphaRange = redoArgs.MinMax;
                    }

                    newImage = _imageUpdater.SetAlphaValue(ImageData, redoArgs.MipLevel, redoArgs.ArrayIndex, redoArgs.Alpha, redoArgs.MinMax);

                    // Save the updated data to the working file.
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);
                    redoFile = CreateUndoCacheFile();

                    redoArgs.UndoFile = undoFile;
                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = newImage;
                    NotifyPropertyChanged(nameof(ImageData));
                    ContentState = ContentState.Modified;

                    CurrentPanel = null;

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);

                    if (undoFile != null)
                    {
                        DeleteUndoCacheFile(undoFile);
                    }

                    if (redoFile != null)
                    {
                        redoFileStream?.Dispose();
                        DeleteUndoCacheFile(redoFile);
                    }

                    alphaUndoArgs = null;

                    return Task.FromException(ex);
                }
                finally
                {
                    redoFileStream?.Dispose();
                    HostServices.BusyService.SetIdle();                    
                }
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(Resources.GORIMG_UNDO_DESC_SET_ALPHA, UndoAction, RedoAction, alphaUndoArgs, alphaUndoArgs);
                // Need to call this so the UI can register our updated undo stack.
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to determine if the premultiplied alpha can be set.
        /// </summary>
        /// <param name="_">The value for the flag (not used here).</param>
        /// <returns><b>true</b> if premultiplied alpha can be set or not, <b>false</b> if not.</returns>
        private bool CanSetPremultipliedAlpha(bool _) => (ImageData != null)
                && (ImageData.FormatInfo.HasAlpha)
                && (ImageData.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm));

        /// <summary>
        /// Function to set whether the image uses premultiplied alpha or not.
        /// </summary>
        /// <param name="value"><b>true</b> to set the flag, <b>false</b> to unset it.</param>
        private void DoSetPremultipliedAlpha(bool value)
        {
            try
            {
                IsPremultiplied = value;
                ContentState = ContentState.Modified;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
        }

        /// <summary>
        /// Function to determine if the FX options can be shown.
        /// </summary>
        /// <returns><b>true</b> if the fx options can be shown, <b>false</b> if not.</returns>
        private bool CanShowFx() => (CurrentPanel == null) && (CommandContext == null);

        /// <summary>
        /// Function to show the FX options.
        /// </summary>
        private void DoShowFx()
        {
            if ((FxContext.SetImageCommand != null) || (!FxContext.SetImageCommand.CanExecute(ImageData)))
            {
                FxContext.SetImageCommand.Execute(ImageData);
            }
            else
            {
                return;
            }

            try
            {
                CommandContext = FxContext;               
            }
            catch (Exception ex)
            {
                HostServices.Log.Print("[Error] Error showing fx options.", LoggingLevel.Simple);
                HostServices.Log.LogException(ex);
            }
        }


        /// <summary>
        /// Function to determine if the effects can be applied to the main image.
        /// </summary>
        /// <returns><b>true</b> if the effects can be applied, <b>false</b> if not.</returns>
        private bool CanApplyFx() => (FxContext.EffectsUpdated) && (CurrentPanel == null);

        /// <summary>
        /// Function to apply the effects to the target image.
        /// </summary>
        private void DoApplyFx()
        {
            EffectsUndoArgs effectsUndoArgs = null;

            Task UndoAction(EffectsUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                HostServices.BusyService.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();

                    NotifyPropertyChanging(nameof(ImageData));
                    ImageData = image;
                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_APPLYING_EFFECTS);
                }
                finally
                {
                    inStream?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(EffectsUndoArgs redoArgs, CancellationToken cancelToken)
            {
                IGorgonVirtualFile undoFile = null;
                IGorgonVirtualFile redoFile = null;
                Stream redoFileStream = null;
                IGorgonImage alteredImage = null;

                try
                {
                    HostServices.BusyService.SetBusy();

                    undoFile = CreateUndoCacheFile();
                    NotifyPropertyChanging(nameof(ImageData));

                    if (redoArgs?.RedoFile != null)
                    {
                        // Just reuse the image data in the redo cache item.                        
                        redoFileStream = redoArgs.RedoFile.OpenStream();
                        (IGorgonImage redoImage, _, _) = _imageIO.LoadImageFile(redoFileStream, _workingFile.Name);
                        redoFileStream.Dispose();

                        ImageData.Dispose();
                        ImageData = redoImage;

                        DeleteUndoCacheFile(redoArgs.RedoFile);
                    }
                    else
                    {
                        int arrayDepth = ImageType == ImageType.Image3D ? CurrentDepthSlice : CurrentArrayIndex;                        
                        FxContext.FxService.EffectImage.Buffers[0].CopyTo(ImageData.Buffers[CurrentMipLevel, arrayDepth]);
                        // We are done with the data for now, so we can deallocate it.
                        FxContext.FxService.SetImage(null, 0, 0);
                    }

                    NotifyPropertyChanged(nameof(ImageData));

                    // Save the updated data to the working file.
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);
                    redoFile = CreateUndoCacheFile();

                    if (redoArgs == null)
                    {
                        redoArgs = effectsUndoArgs = new EffectsUndoArgs();
                    }

                    redoArgs.RedoFile = redoFile;
                    redoArgs.UndoFile = undoFile;

                    ContentState = ContentState.Modified;
                    CommandContext = null;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_APPLYING_EFFECTS);
                    if (undoFile != null)
                    {
                        DeleteUndoCacheFile(undoFile);
                    }

                    if (redoFile != null)
                    {
                        redoFileStream?.Dispose();
                        DeleteUndoCacheFile(redoFile);
                    }

                    effectsUndoArgs = null;
                    return Task.FromException(ex);
                }
                finally
                {
                    alteredImage?.Dispose();
                    HostServices.BusyService.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(Resources.GORIMG_UNDO_DESC_EFFECTS, UndoAction, RedoAction, effectsUndoArgs, effectsUndoArgs);
                // Need to call this so the UI can register our updated undo stack.
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>Function to determine the action to take when this content is closing.</summary>
        /// <returns>
        ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
        /// <remarks>PlugIn authors should override this method to confirm whether save changed content, continue without saving, or cancel the operation entirely.</remarks>
        protected override async Task<bool> OnCloseContentTaskAsync()
        {
            if (ContentState == ContentState.Unmodified)
            {
                return true;
            }

            MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORIMG_CONFIRM_CLOSE, File.Name), allowCancel: true);

            switch (response)
            {
                case MessageResponse.Yes:
                    await DoSaveImageTask(SaveReason.UserSave);
                    return true;
                case MessageResponse.Cancel:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>Function to initialize the content.</summary>
        /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
        protected override void OnInitialize(ImageContentParameters injectionParameters)
        {
            base.OnInitialize(injectionParameters);

            ImagePicker = injectionParameters.ImagePicker;
            _cropResizeSettings = injectionParameters.CropResizeSettings;
            _dimensionSettings = injectionParameters.DimensionSettings;
            _mipMapSettings = injectionParameters.MipMapSettings;
            _alphaSettings = injectionParameters.AlphaSettings;
            FxContext = injectionParameters.FxContext;
            _settings = injectionParameters.Settings;
            _workingFile = injectionParameters.WorkingFile;
            ImageData = injectionParameters.Image;
            _formatSupport = injectionParameters.FormatSupport;
            _imageIO = injectionParameters.ImageIOService;
            _undoService = injectionParameters.UndoService;
            _imageUpdater = injectionParameters.ImageUpdater;
            _videoAdapter = injectionParameters.VideoAdapterInfo;
            _externalEditor = injectionParameters.ExternalEditorService;
            _format = injectionParameters.OriginalFormat;
            
            _cropResizeSettings.OkCommand = new EditorCommand<object>(DoCropResize, CanCropResize);
            _dimensionSettings.OkCommand = new EditorCommand<object>(DoUpdateImageDimensions, CanUpdateDimensions);
            _mipMapSettings.OkCommand = new EditorCommand<object>(DoGenMips, CanGenMips);
            _alphaSettings.OkCommand = new EditorCommand<object>(DoSetAlphaValue);
            ImagePicker.OkCommand = new EditorCommand<object>(DoFinalizeImport, CanFinalizeImport);

            if (injectionParameters.File.Metadata.Attributes.TryGetValue(PremultipliedAttr, out string premultiplied))
            {
                if (!bool.TryParse(premultiplied, out _isPremultiplied))
                {
                    _isPremultiplied = false;
                }
            }

            BuildCodecList(ImageData);

            _pixelFormats = GetFilteredFormats();

            _undoCacheDir = _imageIO.ScratchArea.CreateDirectory("/undocache");

            _dimensionSettings.MipSupport = MipSupport;

            if (_settings.CodecPlugInPaths != null)
            {
                _settings.CodecPlugInPaths.CollectionChanged += CodecPlugInPaths_CollectionChanged;
            }

            FxContext.ApplyCommand = new EditorCommand<object>(DoApplyFx, CanApplyFx);
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            ImageData?.Dispose();
            FxContext.ApplyCommand = null;
            FxContext?.OnUnload();

            try
            {
                if (_settings.CodecPlugInPaths != null)
                {
                    _settings.CodecPlugInPaths.CollectionChanged -= CodecPlugInPaths_CollectionChanged;
                }

                CurrentPanel = null;

                if (_workingFile != null)
                {
                    _imageIO.ScratchArea.DeleteFile(_workingFile.FullPath);
                    _workingFile = null;
                }

                if (_undoCacheDir != null)
                {
                    _imageIO.ScratchArea.DeleteDirectory(_undoCacheDir.FullPath);
                    _undoCacheDir = null;
                }
            }
            catch (Exception ex)
            {
                HostServices.Log.Print("There was an error cleaning up the working file.", LoggingLevel.Verbose);
                HostServices.Log.LogException(ex);
            }

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageContent class.</summary>
        public ImageContent()
        {
            UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
            RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
            ExportImageCommand = new EditorCommand<IGorgonImageCodec>(DoExportImage, CanExportImage);
            ConvertFormatCommand = new EditorCommand<BufferFormat>(DoConvertFormat, CanConvertFormat);
            SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveImageTask, CanSaveImage);
            ChangeImageTypeCommand = new EditorCommand<ImageType>(DoChangeImageType, CanChangeImageType);
            ImportFileCommand = new EditorAsyncCommand<float>(DoImportFileAsync, CanImportFile);
            ShowImageDimensionsCommand = new EditorCommand<object>(DoShowImageDimensions, CanShowImageDimensions);
            ShowMipGenerationCommand = new EditorCommand<object>(DoShowMipGeneration, CanShowMipGeneration);
            EditInAppCommand = new EditorCommand<object>(DoEditInApp, CanEditInApp);
            PremultipliedAlphaCommand = new EditorCommand<bool>(DoSetPremultipliedAlpha, CanSetPremultipliedAlpha);
            ShowSetAlphaCommand = new EditorCommand<object>(DoShowSetAlphaValue, CanShowSetAlphaValue);
            CopyToImageCommand = new EditorAsyncCommand<CopyToImageArgs>(DoCopyToImageAsync, CanCopyToImage);
            ShowFxCommand = new EditorCommand<object>(() => DoShowFx(), CanShowFx);            
        }
        #endregion
    }
}
