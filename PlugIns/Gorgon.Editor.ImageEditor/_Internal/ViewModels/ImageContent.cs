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
using Gorgon.Diagnostics;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

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
            /// The format for the image at this undo level.
            /// </summary>
            public BufferFormat Format;
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
        // The image.
        private IGorgonImage _image;
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
        #endregion

        #region Properties.
        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => ImageEditorCommonConstants.ContentType;

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
        #endregion

        #region Methods.        
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
            ShowWaitPanel(string.Format(Resources.GORIMG_TEXT_SAVING, File.Name));

            try
            {
                // Save the image.
                _workingFile = _imageIO.SaveImageFile(File.Name, _image, CurrentPixelFormat);

                using (Stream inStream = _workingFile.OpenStream())
                using (Stream outStream = File.OpenWrite())
                {
                    await inStream.CopyToAsync(outStream);
                }

                File.Refresh();

                ContentState = ContentState.Unmodified;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_SAVE_CONTENT);
            }
            finally
            {
                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function to determine if an undo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
        private bool CanUndo() => _undoService.CanUndo;

        /// <summary>
        /// Function to determine if a redo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
        private bool CanRedo() => _undoService.CanRedo;

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

            if (_image.FormatInfo.IsCompressed)
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
                    || (!supportInfo.IsTextureFormat(_image.ImageType))
                    || (supportInfo.IsDepthBufferFormat))
                {
                    continue;
                }

                var formatInfo = new GorgonFormatInfo(format);

                if ((formatInfo.IsPacked) || (formatInfo.IsTypeless))
                {
                    continue;
                }
                                
                if ((!_image.FormatInfo.IsCompressed) && (!formatInfo.IsCompressed) && (!_image.CanConvertToFormat(format)))
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
        private bool CanConvertFormat(BufferFormat format) => (format == _image.Format) || (format == BufferFormat.Unknown) ? false : true;

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
                    _image.Dispose();
                    _image = image;
                    CurrentPixelFormat = undoArgs.Format;
                    BuildCodecList(_image);

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
                GorgonFormatInfo srcFormat = _image.FormatInfo;
                IGorgonVirtualFile undoFile = null;

                try
                {
                    BusyState.SetBusy();

                    var destFormat = new GorgonFormatInfo(format);

                    // Ensure that we can actually convert.
                    if (!destFormat.IsCompressed)
                    {
                        if (!_image.CanConvertToFormat(format))
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
                    _imageIO.SaveImageFile(File.Name, _image, format);

                    CurrentPixelFormat = format;
                    BuildCodecList(_image);

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
                FileInfo exportedFile = _imageIO.ExportImage(File, _image, codec);

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
                if ((!codec.SupportsMipMaps) && (_image.MipCount > 1))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support mip maps, and image has {_image.MipCount} mip levels. Skipping...", LoggingLevel.Verbose);
                    continue;
                }

                if ((!codec.SupportsMultipleFrames) && (_image.ArrayCount > 1))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support arrays, and image has {_image.ArrayCount} array indices. Skipping...", LoggingLevel.Verbose);
                    continue;
                }

                if ((!codec.SupportsDepth) && (_image.ImageType == ImageType.Image3D))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support 3D (depth) images, and image is 3D. Skipping...", LoggingLevel.Verbose);
                    continue;
                }

                if (codec.SupportedPixelFormats.All(item => item != _image.Format))
                {
                    Log.Print($"Codec '{codec.CodecDescription} ({codec.Codec})' does not support the pixel format [{_image.Format}]. Skipping...", LoggingLevel.Verbose);
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

            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
            _workingFile = injectionParameters.WorkingFile ?? throw new ArgumentMissingException(nameof(injectionParameters.WorkingFile), nameof(injectionParameters));            
            _image = injectionParameters.Image ?? throw new ArgumentMissingException(nameof(injectionParameters.Image), nameof(injectionParameters));
            _formatSupport = injectionParameters.FormatSupport ?? throw new ArgumentMissingException(nameof(injectionParameters.FormatSupport), nameof(injectionParameters));
            _imageIO = injectionParameters.ImageIOService ?? throw new ArgumentMissingException(nameof(injectionParameters.ImageIOService), nameof(injectionParameters));
            _undoService = injectionParameters.UndoService ?? throw new ArgumentMissingException(nameof(injectionParameters.UndoService), nameof(injectionParameters));
            _format = injectionParameters.OriginalFormat;

            BuildCodecList(_image);

            _pixelFormats = GetFilteredFormats();

            _undoCacheDir = _imageIO.ScratchArea.CreateDirectory("/undocache");
        }

        /// <summary>
        /// Function to retrieve the image data for displaying on the view.
        /// </summary>
        /// <returns>The underlying image data for display.</returns>
        public IGorgonImage GetImage() => _image;

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            _image?.Dispose();

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
        }
        #endregion
    }
}
