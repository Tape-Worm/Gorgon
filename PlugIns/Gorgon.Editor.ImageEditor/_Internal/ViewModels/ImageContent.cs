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
using Gorgon.Editor.Content;

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
        /// The arguments used to undo/redo an image import.
        /// </summary>
        private class ImportUndoArgs
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
        // The directory to store the undo cache data.
        private IGorgonVirtualDirectory _undoCacheDir;
        // The format support information for the current video card.
        private IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> _formatSupport;
        // The available pixel formats, based on codec.
        private ObservableCollection<BufferFormat> _pixelFormats = new ObservableCollection<BufferFormat>();
        // The settings for the image editor plugin.
        private ImageEditorSettings _settings;
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
        // The service used to update the image data.
        private IImageUpdaterService _imageUpdater;
        // Information about the current video adapter.
        private IGorgonVideoAdapterInfo _videoAdapter;
        #endregion

        #region Properties.
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
        #endregion

        #region Methods.        
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
        /// Function to determine if the current content can be saved.
        /// </summary>
        /// <returns><b>true</b> if the content can be saved, <b>false</b> if not.</returns>
        private bool CanSaveImage() => ContentState != ContentState.Unmodified;

        /// <summary>
        /// Function to save the image back to the project file system.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoSaveImageTask()
        {
            Stream inStream = null;
            Stream outStream = null;
            IGorgonVirtualFile workFile = null;

            ShowWaitPanel(string.Format(Resources.GORIMG_TEXT_SAVING, File.Name));

            try
            {
                // Persist the image to a new working file so that block compression won't be applied to our current working file.                
                workFile = _imageIO.SaveImageFile(Guid.NewGuid().ToString("N"), ImageData, CurrentPixelFormat);

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
        private bool CanUndo() => _undoService.CanUndo && !CropOrResizeSettings.IsActive;

        /// <summary>
        /// Function to determine if a redo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
        private bool CanRedo() => _undoService.CanRedo && !CropOrResizeSettings.IsActive;

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
        /// Function to delete 
        /// </summary>
        /// <param name="undoFile"></param>
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
                    BuildCodecList(ImageData);

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

                    CurrentPixelFormat = format;
                    BuildCodecList(ImageData);

                    if (redoArgs == null)
                    {
                        redoArgs = convertUndoArgs = new ConvertUndoArgs();
                    }

                    redoArgs.Format = srcFormat.Format;
                    redoArgs.UndoFile = undoFile;

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
            try
            {
                FileInfo exportedFile = _imageIO.ExportImage(File, ImageData, codec);

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
                BusyState.SetIdle();
            }
        }

        /// <summary>
        /// Function to cancel the crop/resize operation.
        /// </summary>
        private void DoCancelCropResize()
        {
            try
            {
                CropOrResizeSettings.IsActive = false;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
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
                CropOrResizeSettings.IsActive = false;
            }
        }

        /// <summary>
        /// Function to determine if the image change be changed to the specified type.
        /// </summary>
        /// <param name="imageType">The type of image.</param>
        /// <returns><b>true</b> if the image can change types, <b>false</b> if not.</returns>
        private bool CanChangeImageType(ImageType imageType)
        {
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
                        case ImageType.Image2D:
                            if ((DepthCount > 1)
                                && (MessageDisplay.ShowConfirmation(Resources.GORIMG_CONFIRM_3D_TO_2D) == MessageResponse.No))
                            {
                                return Task.CompletedTask;
                            }

                            BusyState.SetBusy();
                            newImage = _imageUpdater.ConvertTo2D(ImageData, false);
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
        /// Function to build the list of codecs that support the current image.
        /// </summary>
        /// <param name="image">The image to evaluate.</param>
        private void BuildCodecList(IGorgonImage image)
        {
            // TODO: This isn't well designed and is confusing to use, need to rethink how we do exports.
            Log.Print("Building codec list for the current image.", LoggingLevel.Verbose);

            Codecs.Clear();

            if (image == null)
            {
                Log.Print("[WARNING] No image was found.", LoggingLevel.Simple);
                return;
            }

            if (_imageIO.InstalledCodecs.Count == 0)
            {
                Log.Print("[WARNING] No image codecs were found.  This should not happen.", LoggingLevel.Simple);
                return;
            }

            foreach (IGorgonImageCodec codec in _imageIO.InstalledCodecs.Where(item => item.CanEncode).OrderBy(item => item.Codec))
            {
                if ((!codec.SupportsMipMaps) && (ImageData.MipCount > 1))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support mip maps, and image has {ImageData.MipCount} mip levels. Skipping...", LoggingLevel.Verbose);
                    continue;
                }

                if ((!codec.SupportsMultipleFrames) && (ImageData.ArrayCount > 1))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support arrays, and image has {ImageData.ArrayCount} array indices. Skipping...", LoggingLevel.Verbose);
                    continue;
                }

                if ((!codec.SupportsDepth) && (ImageData.ImageType == ImageType.Image3D))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support 3D (depth) images, and image is 3D. Skipping...", LoggingLevel.Verbose);
                    continue;
                }

                if (codec.SupportedPixelFormats.All(item => item != ImageData.Format))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support the pixel format [{ImageData.Format}]. Skipping...", LoggingLevel.Verbose);
                    continue;
                }

                Log.Print($"Adding codec '{codec.CodecDescription} ({codec.Codec})'", LoggingLevel.Verbose);
                Codecs.Add(codec);
            }
        }

        /// <summary>Function to determine the action to take when this content is closing.</summary>
        /// <returns>
        ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
        /// <remarks>Plugin authors should override this method to confirm whether save changed content, continue without saving, or cancel the operation entirely.</remarks>
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
                    await DoSaveImageTask();
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
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
            _workingFile = injectionParameters.WorkingFile ?? throw new ArgumentMissingException(nameof(injectionParameters.WorkingFile), nameof(injectionParameters));            
            ImageData = injectionParameters.Image ?? throw new ArgumentMissingException(nameof(injectionParameters.Image), nameof(injectionParameters));
            _formatSupport = injectionParameters.FormatSupport ?? throw new ArgumentMissingException(nameof(injectionParameters.FormatSupport), nameof(injectionParameters));
            _imageIO = injectionParameters.ImageIOService ?? throw new ArgumentMissingException(nameof(injectionParameters.ImageIOService), nameof(injectionParameters));
            _undoService = injectionParameters.UndoService ?? throw new ArgumentMissingException(nameof(injectionParameters.UndoService), nameof(injectionParameters));
            _imageUpdater = injectionParameters.ImageUpdater ?? throw new ArgumentMissingException(nameof(injectionParameters.ImageUpdater), nameof(injectionParameters));
            _videoAdapter = injectionParameters.VideoAdapterInfo ?? throw new ArgumentMissingException(nameof(injectionParameters.VideoAdapterInfo), nameof(injectionParameters));
            _format = injectionParameters.OriginalFormat;

            _cropResizeSettings.CancelCommand = new EditorCommand<object>(DoCancelCropResize);
            _cropResizeSettings.OkCommand = new EditorCommand<object>(DoCropResize, CanCropResize);

            BuildCodecList(ImageData);

            _pixelFormats = GetFilteredFormats();

            _undoCacheDir = _imageIO.ScratchArea.CreateDirectory("/undocache");
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            ImageData?.Dispose();

            try
            {                
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
                || (!dragData.File.Metadata.Attributes.TryGetValue(ContentTypeAttr, out string dataType))
                || (!string.Equals(dataType, ImageEditorCommonConstants.ContentType, StringComparison.OrdinalIgnoreCase)))
            {                
                return false;
            }

            if (CropOrResizeSettings.IsActive)
            {
                dragData.Cancel = true;
                return false;
            }

            return true;
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
            ImportUndoArgs importUndoArgs = null;           

            Task UndoAction(ImportUndoArgs undoArgs, CancellationToken cancelToken)
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

            Task RedoAction(ImportUndoArgs redoArgs, CancellationToken cancelToken)
            {
                IGorgonVirtualFile undoFile = null;
                IGorgonVirtualFile redoFile = null;
                Stream redoFileStream = null;

                try
                {
                    BusyState.SetBusy();

                    int startArrayOrDepth = ImageType == ImageType.Image3D ? CurrentDepthSlice : CurrentArrayIndex;
                    var newSize = new DX.Size2(Width, Height);

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
                        redoArgs = importUndoArgs = new ImportUndoArgs();
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
            if ((ImageData.Width == importImage.Width) && (ImageData.Height == importImage.Height))
            {
                return false;
            }

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
            CropOrResizeSettings.TargetImageSize = new DX.Size2(ImageData.Width, ImageData.Height);
            CropOrResizeSettings.IsActive = true;

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

                (sourceFile, importImage, tempFile, originalFormat) = _imageIO.ImportImage();

                if ((sourceFile == null) || (importImage == null) || (tempFile == null) || (originalFormat == BufferFormat.Unknown))
                {
                    return;
                }

                if (!ConvertImportFilePixelFormat(importImage, originalFormat))
                {
                    return;
                }

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
            }
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
                imageFile = dragData.File.OpenRead();
                (importImage, tempFile, originalFormat) = _imageIO.LoadImageFile(imageFile, dragData.File.Name);
                imageFile.Close();

                if (!ConvertImportFilePixelFormat(importImage, originalFormat))
                {
                    dragData.Cancel = true;
                    return;
                }

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
                if (tempFile != null)
                {
                    _imageIO.ScratchArea.DeleteFile(tempFile.FullPath);
                }
                
                importImage?.Dispose();
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
            SaveContentCommand = new EditorAsyncCommand<object>(DoSaveImageTask, CanSaveImage);
            ChangeImageTypeCommand = new EditorCommand<ImageType>(DoChangeImageType, CanChangeImageType);
            ImportFileCommand = new EditorCommand<object>(DoImportFile, CanImportFile);
        }
        #endregion
    }
}
