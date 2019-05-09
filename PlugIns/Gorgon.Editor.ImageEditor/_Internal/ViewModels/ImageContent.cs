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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Diagnostics;
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
using System.Diagnostics;
using System.Text;
using System.ComponentModel;
using Gorgon.Collections;
using System.Collections.Specialized;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The image editor content.
    /// </summary>
    internal class ImageContent
        : EditorContentCommon<ImageContentParameters>, IImageContent
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
        // The service used to update the image data.
        private IImageUpdaterService _imageUpdater;
        // Information about the current video adapter.
        private IGorgonVideoAdapterInfo _videoAdapter;
        // The currently active panel.
        private IHostedPanelViewModel _currentPanel;
        // The service used to edit the image with an external editor.
        private IImageExternalEditService _externalEditor;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether mip maps are supported for the current format.
        /// </summary>
        public bool MipSupport => (_formatSupport != null) && (ImageData != null)
            && (_formatSupport.ContainsKey(CurrentPixelFormat)) 
            && (_formatSupport[CurrentPixelFormat].FormatSupport & BufferFormatSupport.Mip) == BufferFormatSupport.Mip;

        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => ImageEditorCommonConstants.ContentType;

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

        /// <summary>Property to return the currently active panel.</summary>
        public IHostedPanelViewModel CurrentPanel
        {
            get => _currentPanel;
            private set
            {
                if (_currentPanel == value)
                {
                    return;
                }

                if (_currentPanel != null)
                {
                    _currentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;
                    _currentPanel.IsActive = false;
                }

                OnPropertyChanging();
                _currentPanel = value;
                OnPropertyChanged();

                if (_currentPanel != null)
                {
                    _currentPanel.IsActive = true;
                    _currentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
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
        public IEditorCommand<object> ImportFileCommand
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
        /// Porperty to return the command used to edit the image in an external application.
        /// </summary>
        public IEditorCommand<object> EditInAppCommand
        {
            get;
        }
        #endregion

        #region Methods.        
        /// <summary>Handles the CollectionChanged event of the CodecPlugInPaths control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void CodecPlugInPaths_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => BuildCodecList(ImageData);

        /// <summary>Handles the PropertyChanged event of the CurrentPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentPanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

                BusyState.SetBusy();

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
                    ImageData = image;

                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    BusyState.SetIdle();
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
                    BusyState.SetBusy();

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
                        ImageData = redoImage;

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
                        _imageUpdater.CopyTo(importImage, ImageData, CurrentMipLevel, startArrayOrDepth, alignment);
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
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
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
                    BusyState.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task task = RedoAction(null, CancellationToken.None);

            // If we had an error, do not record the undo state.
            if (!task.IsFaulted)
            {
                _undoService.Record(string.Format(Resources.GORIMG_UNDO_DESC_IMPORT, file), UndoAction, RedoAction, importUndoArgs, importUndoArgs);
            }
        }

        /// <summary>
        /// Function to determine if an image file can be imported into the current image.
        /// </summary>
        /// <returns><b>true</b> if the image can be imported, <b>false</b> if not.</returns>
        private bool CanImportFile() => (ImageData != null) && ((ImageType == ImageType.Image2D) || (ImageType == ImageType.ImageCube) || (ImageType == ImageType.Image3D));

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

            return MessageDisplay.ShowConfirmation(confirmMessage);
        }

        /// <summary>
        /// Function to convert the imported image pixel data to the same format as our image.
        /// </summary>
        /// <param name="imageData">The imported image data.</param>
        /// <param name="originalFormat">The original, actual, format of the source image file.</param>
        /// <returns><b>true</b> if image was converted successfully, or <b>false</b> if the image could not be converted.</returns>
        private bool ConvertImportFilePixelFormat(IGorgonImage imageData, BufferFormat originalFormat)
        {
            // Convert to our target image format before doing anything.
            if (ImageData.Format != imageData.Format)
            {
                if (!imageData.CanConvertToFormat(ImageData.Format))
                {
                    MessageDisplay.ShowError(string.Format(Resources.GORIMG_ERR_CANNOT_CONVERT, originalFormat, CurrentPixelFormat));
                    return false;
                }

                imageData.ConvertToFormat(ImageData.Format);
            }

            return true;
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
        private void DoImportFile()
        {
            IGorgonImage importImage = null;
            IGorgonVirtualFile tempFile = null;
            BufferFormat originalFormat = BufferFormat.Unknown;
            FileInfo sourceFile = null;

            try
            {
                if (ConfirmImageDataOverwrite() == MessageResponse.No)
                {
                    return;
                }

                BusyState.SetBusy();
                (sourceFile, importImage, tempFile, originalFormat) = _imageIO.ImportImage();

                if ((sourceFile == null) || (importImage == null) || (tempFile == null) || (originalFormat == BufferFormat.Unknown))
                {
                    return;
                }


                if (!ConvertImportFilePixelFormat(importImage, originalFormat))
                {
                    return;
                }
                BusyState.SetIdle();

                CropOrResizeSettings.ImportFileDirectory = sourceFile.Directory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
                if (CheckForCropResize(importImage, sourceFile.Name))
                {
                    return;
                }

                ImportImageData(sourceFile.Name, importImage, CropResizeMode.None, Alignment.UpperLeft, ImageFilter.Point, false);
                _settings.LastImportExportPath = CropOrResizeSettings.ImportFileDirectory;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                if (tempFile != null)
                {
                    _imageIO.ScratchArea.DeleteFile(tempFile.FullPath);
                }
                importImage?.Dispose();
                BusyState.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if the image dimensions editor can be shown.
        /// </summary>
        /// <returns><b>true</b> if the editor can be shown, <b>false</b> if not.</returns>
        private bool CanShowImageDimensions() => (ImageData != null) && (DimensionSettings?.UpdateImageInfoCommand != null) && (DimensionSettings.UpdateImageInfoCommand.CanExecute(ImageData));

        /// <summary>
        /// Function to determine if the mip map generation settings can be shown.
        /// </summary>
        /// <returns><b>true</b> if the settings can be shown, <b>false</b> if not.</returns>
        private bool CanShowMipGeneration() => (ImageData != null) && (MipSupport) && (MipMapSettings?.UpdateImageInfoCommand != null) && (MipMapSettings.UpdateImageInfoCommand.CanExecute(ImageData));

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
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
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
                if ((ImageData.MipCount > 1) && (MessageDisplay.ShowConfirmation(Resources.GORIMG_CONFIRM_GEN_MIPMAP) == MessageResponse.No))
                {
                    return;
                }

                MipMapSettings.UpdateImageInfoCommand?.Execute(ImageData);
                CurrentPanel = MipMapSettings;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
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
            Stream inStream = null;
            Stream outStream = null;
            IGorgonVirtualFile workFile = null;

            ShowWaitPanel(string.Format(Resources.GORIMG_TEXT_SAVING, File.Name));

            try
            {
                // Persist the image to a new working file so that block compression won't be applied to our current working file.   
                workFile = await Task.Run(() =>_imageIO.SaveImageFile(Guid.NewGuid().ToString("N"), ImageData, CurrentPixelFormat));

                inStream = workFile.OpenStream();
                outStream = File.OpenWrite();

                await inStream.CopyToAsync(outStream);

                inStream.Dispose();
                outStream.Dispose();

                File.Refresh();

                ContentState = ContentState.Unmodified;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_SAVE_CONTENT);
            }
            finally
            {
                inStream?.Dispose();
                outStream?.Dispose();

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
        private bool CanUndo() => _undoService.CanUndo && CurrentPanel == null;

        /// <summary>
        /// Function to determine if a redo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
        private bool CanRedo() => _undoService.CanRedo && CurrentPanel == null;

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
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_REDO);
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
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UNDO);
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

                result.Add(format);
            }

            return result;
        }


        /// <summary>
        /// Function to determine if format conversion can happen.
        /// </summary>
        /// <param name="format">The format to convert to.</param>
        /// <returns><b>true</b> if the conversion can take place, <b>false</b> if not.</returns>
        private bool CanConvertFormat(BufferFormat format) => (format == ImageData.Format) || (format == BufferFormat.Unknown) ? false : true;

        /// <summary>
        /// Function to create an undo cache file from the working file.
        /// </summary>
        /// <returns>The undo cache file.</returns>
        private IGorgonVirtualFile CreateUndoCacheFile()
        {
            string undoFilePath = Path.Combine(_undoCacheDir.FullPath, $"u_{Guid.NewGuid().ToString("N")}");
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
            catch(Exception ex)
            {
                Log.Print($"[ERROR] Unable to delete the undo cache file at '{undoFile.PhysicalFile.FullPath}'.", LoggingLevel.Simple);
                Log.LogException(ex);
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

                BusyState.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();
                    ImageData = image;
                    CurrentPixelFormat = undoArgs.Format;
                    ContentState = ContentState.Modified;

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, string.Format(Resources.GORIMG_ERR_CONVERT_FORMAT, format));
                }
                finally
                {
                    inStream?.Dispose();
                    BusyState.SetIdle();
                }

                return Task.CompletedTask;
            }

            Task RedoAction(ConvertUndoArgs redoArgs, CancellationToken cancelToken)
            {
                GorgonFormatInfo srcFormat = ImageData.FormatInfo;
                IGorgonVirtualFile undoFile = null;

                try
                {
                    BusyState.SetBusy();

                    var destFormat = new GorgonFormatInfo(format);

                    // Ensure that we can actually convert.
                    if (!destFormat.IsCompressed)
                    {
                        if (!ImageData.CanConvertToFormat(format))
                        {
                            string message = string.Format(Resources.GORIMG_ERR_CANNOT_CONVERT, srcFormat.Format, format);
                            MessageDisplay.ShowError(message);
                            return Task.FromException(new InvalidCastException(message));
                        }
                    }
                    else if (!_imageIO.CanHandleBlockCompression)                    {
                        
                        string message = string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format);
                        MessageDisplay.ShowError(message);
                        return Task.FromException(new InvalidCastException(message));
                    }

                    undoFile = CreateUndoCacheFile();                    
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, format);
                    
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
                    MessageDisplay.ShowError(ex, string.Format(Resources.GORIMG_ERR_CONVERT_FORMAT, format));
                    DeleteUndoCacheFile(undoFile);
                    convertUndoArgs = null;
                    return Task.FromException(ex);
                }
                finally
                {
                    BusyState.SetIdle();
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
                        MessageDisplay.ShowError(Resources.GORIMG_ERR_EXPORT_CONVERT);
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
                    if (MessageDisplay.ShowConfirmation(string.Format(Resources.GORIMG_CONFIRM_EXPORT_LIMITS, codec.CodecDescription, missingSupport)) == MessageResponse.No)
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
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CODEC_ASSIGN);
            }
            finally
            {
                exportImage?.Dispose();
                BusyState.SetIdle();
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
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
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

                BusyState.SetBusy();

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
                    NotifyPropertyChanged(nameof(MipCount));
                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    BusyState.SetIdle();
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
                    if ((redoArgs?.RedoFile == null) && ((((DimensionSettings.Width < ImageData.Width) || (DimensionSettings.Height < ImageData.Height))
                            && (DimensionSettings.CurrentMode != CropResizeMode.Resize))
                        || (DimensionSettings.DepthSlicesOrArrayIndices < arrayOrDepthCount)
                        || (DimensionSettings.MipLevels < ImageData.MipCount)))
                    {
                        if (MessageDisplay.ShowConfirmation(Resources.GORIMG_CROP_LOSE_DATA) == MessageResponse.No)
                        {
                            return Task.CompletedTask;
                        }
                    }

                    BusyState.SetBusy();

                    undoFile = CreateUndoCacheFile();

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

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);

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
                    BusyState.SetIdle();
                    CurrentPanel = null;
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
        private bool CanGenMips() => ImageData == null ? false : MipSupport;

        /// <summary>
        /// Function to generate mip maps for the current image.
        /// </summary>
        private void DoGenMips()
        {
            ImportDimensionUndoArgs mipGenUndoArgs = null;

            Task UndoAction(ImportDimensionUndoArgs undoArgs, CancellationToken cancelToken)
            {
                Stream inStream = null;

                BusyState.SetBusy();

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
                    ImageData = image;

                    if (CurrentMipLevel >= MipCount)
                    {
                        CurrentMipLevel = MipCount - 1;
                    }

                    NotifyPropertyChanged(nameof(MipCount));
                    NotifyImageUpdated(nameof(ImageData));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                }
                finally
                {
                    inStream?.Dispose();
                    BusyState.SetIdle();
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
                    BusyState.SetBusy();

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

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);

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
                    BusyState.SetIdle();
                    CurrentPanel = null;
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
            if (!_formatSupport.ContainsKey(CurrentPixelFormat))
            {
                return false;
            }

            if ((imageType == ImageType.ImageCube)
                && ((_formatSupport[CurrentPixelFormat].FormatSupport & BufferFormatSupport.TextureCube) != BufferFormatSupport.TextureCube))
            {
                return false;
            }

            if (((ImageType == ImageType.Image2D) && (imageType == ImageType.ImageCube))
                || ((ImageType == ImageType.ImageCube) && (imageType == ImageType.Image2D)))
            {
                return true;
            }

            // Ensure that the current pixel format is supported.
#pragma warning disable IDE0046 // Convert to conditional expression
            if (!_formatSupport[CurrentPixelFormat].IsTextureFormat(ImageType))
            {
                return false;
            }

            return (imageType != ImageType.Image3D) 
                || ((Width <= _videoAdapter.MaxTexture3DWidth) && (Height <= _videoAdapter.MaxTexture3DHeight));
#pragma warning restore IDE0046 // Convert to conditional expression
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

                BusyState.SetBusy();

                try
                {
                    if (undoArgs.UndoFile == null)
                    {
                        return Task.CompletedTask;
                    }

                    inStream = undoArgs.UndoFile.OpenStream();
                    (IGorgonImage image, _, _) = _imageIO.LoadImageFile(inStream, _workingFile.Name);
                    ImageData.Dispose();
                    ImageData = image;

                    NotifyImageUpdated(nameof(ImageType));

                    inStream.Dispose();

                    // This file will be re-created on redo.
                    DeleteUndoCacheFile(undoArgs.UndoFile);
                    undoArgs.UndoFile = null;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CHANGE_TYPE);
                }
                finally
                {
                    inStream?.Dispose();
                    BusyState.SetIdle();
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
                                && (MessageDisplay.ShowConfirmation(Resources.GORIMG_CONFIRM_3D_TO_2D) == MessageResponse.No))
                            {
                                return Task.CompletedTask;
                            }

                            BusyState.SetBusy();
                            newImage = _imageUpdater.ConvertTo2D(ImageData, newImageType == ImageType.ImageCube);
                            break;
                        case ImageType.Image3D:
                            if ((ArrayCount > 1)
                                && (MessageDisplay.ShowConfirmation(Resources.GORIMG_CONFIRM_ARRAY_TO_VOLUME) == MessageResponse.No))
                            {
                                return Task.CompletedTask;
                            }

                            BusyState.SetBusy();
                            newImage = _imageUpdater.ConvertToVolume(ImageData);
                            break;
                        default:
                            return Task.CompletedTask;
                    }
                    
                    ImageData.Dispose();
                    ImageData = newImage;

                    undoFile = CreateUndoCacheFile();
                    _workingFile = _imageIO.SaveImageFile(File.Name, ImageData, ImageData.Format);

                    if (redoArgs == null)
                    {
                        redoArgs = imageTypeUndoArgs = new ImageTypeUndoArgs();
                    }

                    redoArgs.ImageType = newImageType;
                    redoArgs.UndoFile = undoFile;

                    NotifyImageUpdated(nameof(ImageType));
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CHANGE_TYPE);
                    DeleteUndoCacheFile(undoFile);
                    imageTypeUndoArgs = null;
                    return Task.FromException(ex);
                }
                finally
                {
                    BusyState.SetIdle();
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
        /// Function to edit the current image in an application.
        /// </summary>
        private void DoEditInApp()
        {
            IGorgonImage workImage = null;
            IGorgonVirtualFile workImageFile = null;
            Stream imageStream = null;

            BusyState.SetBusy();

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
                    Log.Print($"Image pixel format is [{workImage.Format}], but PNG codec does not support this format. Image will be converted to [{BufferFormat.B8G8R8A8_UNorm}].", LoggingLevel.Verbose);
                    if (!workImage.CanConvertToFormat(BufferFormat.B8G8R8A8_UNorm))
                    {
                        MessageDisplay.ShowError(string.Format(Resources.GORIMG_ERR_EXPORT_CONVERT, workImage.Format));
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
                    Log.Print($"Image pixel format changed.  Is now [{workImage.Format}], will be converted to [{ImageData.Format}] to match current format.", LoggingLevel.Verbose);
                    if (!workImage.CanConvertToFormat(ImageData.Format))
                    {
                        MessageDisplay.ShowError(string.Format(Resources.GORIMG_ERR_EXPORT_CONVERT, workImage.Format));
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
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
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
                    catch(Exception ex)
                    {
                        Log.Print("Error deleting working image file.", LoggingLevel.All);
                        Log.LogException(ex);
                    }
                }

                workImage?.Dispose();
                BusyState.SetIdle();
            }
        }

        /// <summary>
        /// Function to build the list of codecs that support the current image.
        /// </summary>
        /// <param name="image">The image to evaluate.</param>
        private void BuildCodecList(IGorgonImage image)
        {
            Log.Print("Building codec list for the current image.", LoggingLevel.Verbose);

            _codecs.Clear();
            Codecs.Clear();

            if (image == null)
            {
                Log.Print("[WARNING] No image was found.", LoggingLevel.Simple);
                return;
            }

            if (_imageIO.InstalledCodecs.Codecs.Count == 0)
            {
                Log.Print("[WARNING] No image codecs were found.  This should not happen.", LoggingLevel.Simple);
                return;
            }

            foreach (IGorgonImageCodec codec in _imageIO.InstalledCodecs.Codecs.Where(item => item.CanEncode).OrderBy(item => item.Codec))
            {
                Log.Print($"Adding codec '{codec.CodecDescription} ({codec.Codec})'", LoggingLevel.Verbose);
                Codecs.Add(codec);
                foreach (string extension in codec.CodecCommonExtensions)
                {
                    _codecs.Add((new GorgonFileExtension(extension), codec));
                }
            }
        }

        /// <summary>Function to determine the action to take when this content is closing.</summary>
        /// <returns>
        ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
        /// <remarks>PlugIn authors should override this method to confirm whether save changed content, continue without saving, or cancel the operation entirely.</remarks>
        protected override async Task<bool> OnCloseContentTask()
        {
            if (ContentState == ContentState.Unmodified)
            {
                return true;
            }

            MessageResponse response = MessageDisplay.ShowConfirmation(string.Format(Resources.GORIMG_CONFIRM_CLOSE, File.Name), allowCancel: true);
            
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

            _cropResizeSettings = injectionParameters.CropResizeSettings ?? throw new ArgumentMissingException(nameof(injectionParameters.CropResizeSettings), nameof(injectionParameters));
            _dimensionSettings = injectionParameters.DimensionSettings ?? throw new ArgumentMissingException(nameof(injectionParameters.DimensionSettings), nameof(injectionParameters));
            _mipMapSettings = injectionParameters.MipMapSettings ?? throw new ArgumentMissingException(nameof(injectionParameters.MipMapSettings), nameof(injectionParameters));
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
            _workingFile = injectionParameters.WorkingFile ?? throw new ArgumentMissingException(nameof(injectionParameters.WorkingFile), nameof(injectionParameters));            
            ImageData = injectionParameters.Image ?? throw new ArgumentMissingException(nameof(injectionParameters.Image), nameof(injectionParameters));
            _formatSupport = injectionParameters.FormatSupport ?? throw new ArgumentMissingException(nameof(injectionParameters.FormatSupport), nameof(injectionParameters));
            _imageIO = injectionParameters.ImageIOService ?? throw new ArgumentMissingException(nameof(injectionParameters.ImageIOService), nameof(injectionParameters));
            _undoService = injectionParameters.UndoService ?? throw new ArgumentMissingException(nameof(injectionParameters.UndoService), nameof(injectionParameters));
            _imageUpdater = injectionParameters.ImageUpdater ?? throw new ArgumentMissingException(nameof(injectionParameters.ImageUpdater), nameof(injectionParameters));
            _videoAdapter = injectionParameters.VideoAdapterInfo ?? throw new ArgumentMissingException(nameof(injectionParameters.VideoAdapterInfo), nameof(injectionParameters));
            _externalEditor = injectionParameters.ExternalEditorService ?? throw new ArgumentMissingException(nameof(injectionParameters.ExternalEditorService), nameof(injectionParameters));            
            _format = injectionParameters.OriginalFormat;

            _cropResizeSettings.OkCommand = new EditorCommand<object>(DoCropResize, CanCropResize);
            _dimensionSettings.OkCommand = new EditorCommand<object>(DoUpdateImageDimensions, CanUpdateDimensions);
            _mipMapSettings.OkCommand = new EditorCommand<object>(DoGenMips, CanGenMips);

            BuildCodecList(ImageData);

            _pixelFormats = GetFilteredFormats();

            _undoCacheDir = _imageIO.ScratchArea.CreateDirectory("/undocache");

            _dimensionSettings.MipSupport = MipSupport;

            if (_settings.CodecPlugInPaths != null)
            {
                _settings.CodecPlugInPaths.CollectionChanged += CodecPlugInPaths_CollectionChanged;
            }
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            ImageData?.Dispose();

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
                Log.Print("There was an error cleaning up the working file.", LoggingLevel.Verbose);
                Log.LogException(ex);
            }

            base.OnUnload();
        }        

        /// <summary>Function to determine if an object can be dropped.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <returns>
        ///   <c>true</c> if this instance can drop the specified drag data; otherwise, <c>false</c>.</returns>
        public bool CanDrop(IContentFileDragData dragData)
        {
            // Only perform special operations if the dragged type is an image, otherwise, fall back.
            if ((dragData?.File == null)
                || (!dragData.File.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string dataType))
                || (!string.Equals(dataType, ImageEditorCommonConstants.ContentType, StringComparison.OrdinalIgnoreCase)))
            {                
                return false;
            }

            if (CurrentPanel != null)
            {
                dragData.Cancel = true;
                return false;
            }

            return true;
        }

        /// <summary>Function to drop the payload for a drag drop operation.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <param name="afterDrop">[Optional] The method to execute after the drop operation is completed.</param>
        public void Drop(IContentFileDragData dragData, Action afterDrop = null)
        {
            IGorgonImage importImage = null;
            BufferFormat originalFormat;
            IGorgonVirtualFile tempFile = null;
            Stream imageFile = null;

            try
            {
                if (ConfirmImageDataOverwrite() == MessageResponse.No)
                {
                    dragData.Cancel = true;
                    return;
                }

                // Get the image we're importing.    
                // If we drag ourselves, we don't need to clone it.
                BusyState.SetBusy();
                imageFile = dragData.File.OpenRead();
                (importImage, tempFile, originalFormat) = _imageIO.LoadImageFile(imageFile, $"Import_{dragData.File.Name}_{Guid.NewGuid():N}");
                imageFile.Close();

                if (!ConvertImportFilePixelFormat(importImage, originalFormat))
                {
                    dragData.Cancel = true;
                    return;
                }
                BusyState.SetIdle();

                CropOrResizeSettings.ImportFileDirectory = null;
                if (CheckForCropResize(importImage, dragData.File.Name))
                {
                    return;
                }

                ImportImageData(dragData.File.Name, importImage, CropResizeMode.None, Alignment.UpperLeft, ImageFilter.Point, false);
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
                dragData.Cancel = true;
            }
            finally
            {
                imageFile?.Dispose();

                if (tempFile != null)
                {
                    _imageIO.ScratchArea.DeleteFile(tempFile.FullPath);
                }
                
                importImage?.Dispose();
                BusyState.SetIdle();
            }
        }

        /// <summary>Function to determine if an object can be dropped.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <returns>
        ///   <b>true</b> if the data can be dropped, <b>false</b> if not.</returns>
        public bool CanDrop(IExplorerFilesDragData dragData)
        {
            // Only perform special operations if the dragged type is an image, otherwise, fall back.
            if ((dragData?.Files == null)
                || (dragData.Files.Count != 1))
            {
                dragData.Cancel = true;
                return false;
            }

            if (CurrentPanel != null)
            {
                dragData.Cancel = true;
                return false;
            }

            // Ensure the file being imported is actually supported.
            var ext = new GorgonFileExtension(Path.GetExtension(dragData.Files[0]));

            if (_codecs.All(item => item.codec.CanDecode && item.extension.Equals(ext)))
            {
                dragData.Cancel = true;
                return false;
            }

            return true;
        }

        /// <summary>Function to drop the payload for a drag drop operation.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <param name="afterDrop">[Optional] The method to execute after the drop operation is completed.</param>
        public void Drop(IExplorerFilesDragData dragData, Action afterDrop = null)
        {
            IGorgonImage importImage = null;
            BufferFormat originalFormat;
            IGorgonVirtualFile tempFile = null;
            FileInfo sourceFile = null;

            try
            {
                if (ConfirmImageDataOverwrite() == MessageResponse.No)
                {
                    dragData.Cancel = true;
                    return;
                }

                IGorgonImageCodec codec = null;
                string file = dragData.Files[0];
                var ext = new GorgonFileExtension(Path.GetExtension(file));

                codec = _codecs.FirstOrDefault(item => item.extension.Equals(ext)).codec;
                Debug.Assert(codec != null, $"Could not locate code for {file}");

                BusyState.SetBusy();
                (sourceFile, importImage, tempFile, originalFormat) = _imageIO.ImportImage(codec, file);

                if ((sourceFile == null) || (importImage == null) || (tempFile == null) || (originalFormat == BufferFormat.Unknown))
                {
                    return;
                }

                if (!ConvertImportFilePixelFormat(importImage, originalFormat))
                {
                    return;
                }
                BusyState.SetIdle();

                CropOrResizeSettings.ImportFileDirectory = sourceFile.Directory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
                if (CheckForCropResize(importImage, sourceFile.Name))
                {
                    return;
                }

                ImportImageData(sourceFile.Name, importImage, CropResizeMode.None, Alignment.UpperLeft, ImageFilter.Point, false);
                _settings.LastImportExportPath = CropOrResizeSettings.ImportFileDirectory;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                if (tempFile != null)
                {
                    _imageIO.ScratchArea.DeleteFile(tempFile.FullPath);
                }
                importImage?.Dispose();

                BusyState.SetIdle();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageContent class.</summary>
        public ImageContent()
        {
            UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
            RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
            ExportImageCommand = new EditorCommand<IGorgonImageCodec>(DoExportImage);
            ConvertFormatCommand = new EditorCommand<BufferFormat>(DoConvertFormat, CanConvertFormat);
            SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveImageTask, CanSaveImage);
            ChangeImageTypeCommand = new EditorCommand<ImageType>(DoChangeImageType, CanChangeImageType);
            ImportFileCommand = new EditorCommand<object>(DoImportFile, CanImportFile);
            ShowImageDimensionsCommand = new EditorCommand<object>(DoShowImageDimensions, CanShowImageDimensions);
            ShowMipGenerationCommand = new EditorCommand<object>(DoShowMipGeneration, CanShowMipGeneration);
            EditInAppCommand = new EditorCommand<object>(DoEditInApp, () => ImageData != null);            
        }
        #endregion
    }
}
