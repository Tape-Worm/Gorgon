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
using System.IO;
using Gorgon.Editor.Content;
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
    /// Parameters to pass to the <see cref="ImageContent"/> view model.
    /// </summary>
    internal class ImageContentParameters
        : ContentViewModelInjectionCommon
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the content file representing this content.
        /// </summary>
        public IContentFile ContentFile
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
        /// Property to return the list of available codecs.
        /// </summary>
        public IReadOnlyList<IGorgonImageCodec> Codecs
        {
            get;
        }

        /// <summary>
        /// Property to return the codec used internally by the image editor plugin.
        /// </summary>
        public IGorgonImageCodec DefaultCodec
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
        /// Property to return the settings for the image editor plugin.
        /// </summary>
        public ImageEditorSettings Settings
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageContentVmParameters class.</summary>
        /// <param name="file">The file for the image content.</param>
        /// <param name="settings">The settings for the image editor.</param>
        /// <param name="image">The image data.</param>
        /// <param name="defaultCodec">The default codec used by the plug in.</param>
        /// <param name="formatSupport">A list of <see cref="IGorgonFormatSupportInfo"/> objects for each pixel format.</param>
        /// <param name="codecs">The list of available codecs.</param>
        /// <param name="scratchArea">The file system for temporary files.</param>
        /// <param name="baseInjection">The base injection object used to transfer common objects from the application to the plug in view model.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file" />, <paramref name="image"/>, <paramref name="currentCodec"/>, <paramref name="codecs"/>, or the <paramref name="baseInjection"/> parameter is <b>null</b>.</exception>
        public ImageContentParameters(IContentFile file, 
            ImageEditorSettings settings,
            IGorgonImage image, 
            IReadOnlyList<IGorgonImageCodec> codecs, 
            IGorgonImageCodec defaultCodec,
            IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> formatSupport,
            IGorgonFileSystemWriter<Stream> scratchArea,
            IViewModelInjection baseInjection)
            : base(file, scratchArea, new FileOpenDialogService(), new FileSaveDialogService(), baseInjection?.MessageDisplay ?? throw new ArgumentNullException(nameof(baseInjection)), baseInjection.BusyService)
        {
            Settings = settings;
            Image = image ?? throw new ArgumentNullException(nameof(image));
            Codecs = codecs ?? throw new ArgumentNullException(nameof(codecs));
            DefaultCodec = defaultCodec ?? throw new ArgumentNullException(nameof(defaultCodec));
            FormatSupport = formatSupport ?? throw new ArgumentNullException(nameof(formatSupport));
            Log = baseInjection.Log;
        }
        #endregion
    }
}
