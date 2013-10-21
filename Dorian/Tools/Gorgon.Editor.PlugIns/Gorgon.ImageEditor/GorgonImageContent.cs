#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, October 21, 2013 8:08:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Image content.
    /// </summary>
    class GorgonImageContent
        : ContentObject
    {
        #region Variables.
        private GorgonImageContentPanel _contentPanel;                          // Panel used to display the content.
        private GorgonImageData _image;                                         // Image data.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether this content has properties that can be manipulated in the properties tab.
        /// </summary>
        public override bool HasProperties
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Property to return the type of content.
        /// </summary>
        public override string ContentType
        {
            get
            {
                return Resources.GORIMG_CONTENT_TYPE;
            }
        }

        /// <summary>
        /// Property to return whether the content object supports a renderer interface.
        /// </summary>
        public override bool HasRenderer
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to persist the content data to a stream.
        /// </summary>
        /// <param name="stream">Stream that will receive the data.</param>
        protected override void OnPersist(System.IO.Stream stream)
        {
        }

        /// <summary>
        /// Function to read the content data from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the content data.</param>
        protected override void OnRead(System.IO.Stream stream)
        {
            GorgonImageCodec selectedCodec = null;

            // Find the correct codec.
            foreach (var codec in GorgonImageEditorPlugIn.Codecs.Where(codec => codec.Value.IsReadable(stream)))
            {
                selectedCodec = codec.Value;
            }

            _image = GorgonImageData.FromStream(stream, (int)stream.Length, selectedCodec);
        }

        /// <summary>
        /// Function called when the content is being initialized.
        /// </summary>
        /// <returns>
        /// A control to place in the primary interface window.
        /// </returns>
        protected override ContentPanel OnInitialize()
        {
            _contentPanel = new GorgonImageContentPanel();

            return _contentPanel;
        }

        /// <summary>
        /// Function to retrieve a thumbnail image for the content plug-in.
        /// </summary>
        /// <returns>
        /// The image for the thumbnail of the content.
        /// </returns>
        /// <remarks>
        /// The size of the thumbnail should be set to 128x128.
        /// </remarks>
        public override Image GetThumbNailImage()
        {
            Image resultImage;
            float aspect = (float)_image[0].Width / _image[0].Height;
            var newSize = new Size(128, 128);

            // Resize our image.
            if ((_image[0].Width != 128)
                || (_image[0].Height != 128))
            {
                if (aspect > 1.0f)
                {
                    newSize.Height = (int)(128 / aspect);
                }
                else
                {
                    newSize.Width = (int)(128 * aspect);
                }

                _image.Resize(newSize.Width, newSize.Height, false, ImageFilter.Fant);
            }

            try
            {
                resultImage = _image[0].ToGDIImage();
            }
            catch (GorgonException gex)
            {
                // If we get format not supported, then we should just return a 
                // generic picture.  There's no reason to make a big deal about it.
                if (gex.ResultCode != GorgonResult.FormatNotSupported)
                {
                    throw;
                }

                // Return a generic picture.
                resultImage = (Image)Resources.image_128x128.Clone();
            }

            return resultImage;
        }

        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageContent"/> class.
        /// </summary>
        /// <param name="name">Name of the content.</param>
        public GorgonImageContent(string name)
            : base(name)
        {
            HasThumbnail = true;
        }
        #endregion
    }
}
