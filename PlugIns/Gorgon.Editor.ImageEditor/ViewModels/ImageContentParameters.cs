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
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

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
        /// Property to return the current codec.
        /// </summary>
        public IGorgonImageCodec Codec
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
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageContentVmParameters class.</summary>
        /// <param name="file">The file for the image content.</param>
        /// <param name="image">The image data.</param>
        /// <param name="currentCodec">The current codec for the image data.</param>
        /// <param name="codecs">The list of available codecs.</param>
        /// <param name="baseInjection">The base injection object used to transfer common objects from the application to the plug in view model.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file" />, <paramref name="image"/>, <paramref name="currentCodec"/>, <paramref name="codecs"/>, or the <paramref name="baseInjection"/> parameter is <b>null</b>.</exception>
        public ImageContentParameters(IContentFile file, IGorgonImage image, IGorgonImageCodec currentCodec, IReadOnlyList<IGorgonImageCodec> codecs, IViewModelInjection baseInjection)
            : base(file, baseInjection?.MessageDisplay ?? throw new ArgumentNullException(nameof(baseInjection)), baseInjection.BusyService)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
            Codec = currentCodec ?? throw new ArgumentNullException(nameof(currentCodec));
            Codecs = codecs ?? throw new ArgumentNullException(nameof(codecs));
            Log = baseInjection.Log;
        }
        #endregion
    }
}
