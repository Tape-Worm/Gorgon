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
// Created: November 10, 2018 11:23:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// Parameters to pass to the <see cref="ImageContent"/> view model.
    /// </summary>
    internal class ImageContentParameters
        : ContentViewModelInjectionCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return the image dimension editor view model.
        /// </summary>
        public IDimensionSettings DimensionSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the crop/resize settings view model.
        /// </summary>
        public ICropResizeSettings CropResizeSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the mip map generation view model.
        /// </summary>
        public IMipMapSettings MipMapSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the service used to load/save image data.
        /// </summary>
        public IImageIOService ImageIOService
        {
            get;
        }

        /// <summary>
        /// Property to return the file used to storing working changes.
        /// </summary>
        public IGorgonVirtualFile WorkingFile
        {
            get;
        }

        /// <summary>
        /// Property to return the image to edit.
        /// </summary>
        public IGorgonImage Image
        {
            get;
        }

        /// <summary>
        /// Property to return the format support information for the current video card.
        /// </summary>
        public IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
        {
            get;
        }

        /// <summary>
        /// Property to return the information about the currently active video adapter.
        /// </summary>
        public IGorgonVideoAdapterInfo VideoAdapterInfo
        {
            get;
        }

        /// <summary>
        /// Property to return the settings for the image editor plugin.
        /// </summary>
        public ISettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the original format for the image.
        /// </summary>
        public BufferFormat OriginalFormat
        {
            get;
        }

        /// <summary>
        /// Property to return the service used to update the image.
        /// </summary>
        public IImageUpdaterService ImageUpdater
        {
            get;
        }

        /// <summary>
        /// Property to return the undo service for the editor.
        /// </summary>
        public IUndoService UndoService
        {
            get;
        }

        /// <summary>
        /// Property to return the external editor service used to update an image.
        /// </summary>
        public IImageExternalEditService ExternalEditorService
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageContentVmParameters class.</summary>
        /// <param name="file">The file for the image content.</param>
        /// <param name="settings">The settings for the image editor.</param>
        /// <param name="cropResizeSettings">The crop/resize settings view model.</param>
        /// <param name="dimensionSettings">The image dimensions settings view model.</param>
        /// <param name="mipMapSettings">The mip map generation settings view model.</param>
        /// <param name="imageData">The image data and related information.</param>
        /// <param name="videoAdapter">Information about the current video adapter.</param>
        /// <param name="formatSupport">A list of <see cref="IGorgonFormatSupportInfo"/> objects for each pixel format.</param>
        /// <param name="services">The services required by the image editor.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when any required child parameters are <b>null</b>.</exception>
        public ImageContentParameters(IContentFile file,
            ISettings settings,
            ICropResizeSettings cropResizeSettings,
            IDimensionSettings dimensionSettings,
            IMipMapSettings mipMapSettings,
            (IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) imageData,
            IGorgonVideoAdapterInfo videoAdapter,
            IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> formatSupport,
            ImageEditorServices services)
            : base(file, services.CommonServices ?? throw new ArgumentNullException(nameof(services)))
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            WorkingFile = imageData.workingFile ?? throw new ArgumentNullException(nameof(imageData.workingFile));
            Image = imageData.image ?? throw new ArgumentNullException(nameof(imageData.image));
            FormatSupport = formatSupport ?? throw new ArgumentNullException(nameof(formatSupport));
            ImageIOService = services.ImageIO ?? throw new ArgumentMissingException(nameof(services.ImageIO), nameof(services));
            UndoService = services.UndoService ?? throw new ArgumentMissingException(nameof(services.UndoService), nameof(services));
            ImageUpdater = services.ImageUpdater ?? throw new ArgumentMissingException(nameof(services.ImageUpdater), nameof(services));
            ExternalEditorService = services.ExternalEditorService ?? throw new ArgumentMissingException(nameof(services.ExternalEditorService), nameof(services));
            CropResizeSettings = cropResizeSettings ?? throw new ArgumentNullException(nameof(cropResizeSettings));
            DimensionSettings = dimensionSettings ?? throw new ArgumentNullException(nameof(dimensionSettings));
            MipMapSettings = mipMapSettings ?? throw new ArgumentNullException(nameof(mipMapSettings));
            VideoAdapterInfo = videoAdapter ?? throw new ArgumentNullException(nameof(videoAdapter));
            OriginalFormat = imageData.originalFormat;
        }
        #endregion
    }
}
