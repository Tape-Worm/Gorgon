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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The image editor content.
    /// </summary>
    internal class ImageContent
        : EditorContentCommon<ImageContentParameters>, IImageContent
    {
        #region Constants.
        /// <summary>
        /// The attribute key name for the image codec attribute.
        /// </summary>
        public const string CodecAttr = "ImageCodec";
        #endregion

        #region Variables.
        // The image.
        private IGorgonImage _image;
        // The codec used by the image.
        private IGorgonImageCodec _codec;
        // The available image codecs.        
        private IReadOnlyList<IGorgonImageCodec> _codecs;
        #endregion

        #region Properties.
        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => ImageEditorCommonConstants.ContentType;

        /// <summary>
        /// Property to set or return the image codec used by the image content.
        /// </summary>
        public IGorgonImageCodec CurrentCodec
        {
            get => _codec;
            set
            {
                if (_codec == value)
                {
                    return;
                }

                Debug.Assert(value != null, "Codec should never be null.");
                OnPropertyChanging();
                _codec = value;
                File.Metadata.Attributes[CodecAttr] = value.GetType().FullName;
                OnPropertyChanged();                
            }
        }

        /// <summary>Property to return the name of the content.</summary>
        public string ContentName => File.Name;
        #endregion

        #region Methods.        

        /// <summary>Function to initialize the content.</summary>
        /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
        protected override void OnInitialize(ImageContentParameters injectionParameters)
        {
            base.OnInitialize(injectionParameters);

            _codec = injectionParameters.Codec ?? throw new ArgumentMissingException(nameof(injectionParameters.Codec), nameof(injectionParameters));
            _codecs = injectionParameters.Codecs ?? throw new ArgumentMissingException(nameof(injectionParameters.Codecs), nameof(injectionParameters));
            _image = injectionParameters.Image ?? throw new ArgumentMissingException(nameof(ImageContentParameters.Image), nameof(injectionParameters));
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
        }
        #endregion

        #region Constructor/Finalizer.
        /*/// <summary>Initializes a new instance of the EditorContentCommon class.</summary>
        /// <param name="file">The file for the content.</param>
        /// <param name="image">The image data.</param>
        /// <param name="codecs">The available codec list.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/>, <paramref name="image"/>, or the <paramref name="codecs"/> parameter is <b>null</b>.</exception>
        public ImageContent(IContentFile file, IGorgonImage image, IReadOnlyList<IGorgonImageCodec> codecs)
            : base(file)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));
            _codecs = codecs ?? throw new ArgumentNullException(nameof(codecs));
        }*/
        #endregion
    }
}
