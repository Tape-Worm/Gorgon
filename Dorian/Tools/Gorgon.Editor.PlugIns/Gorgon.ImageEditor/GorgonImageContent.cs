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

using System.Drawing;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Image content.
    /// </summary>
    class GorgonImageContent
        : ContentObject, IImageEditorContent
    {
        #region Variables.
        private GorgonImageContentPanel _contentPanel;                          // Panel used to display the content.
        private readonly GorgonImageCodec _codec;								// Codec used for the image.
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
            Image = GorgonImageData.FromStream(stream, (int)stream.Length, _codec);
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
            float aspect = (float)Image[0].Width / Image[0].Height;
            var newSize = new Size(128, 128);

			// If this device can't support loading the image, then show
			// a thumbnail that will indicate that we can't load it.
	        if ((Image[0].Width >= Graphics.Textures.MaxWidth)
	            || (Image[0].Height >= Graphics.Textures.MaxHeight)
	            || (Image[0].Depth >= Graphics.Textures.MaxDepth))
	        {
		        return (Image)Resources.invalid_image_128x128.Clone();
	        }

			// Ensure that we support the image format as well.
	        switch (Image.Settings.ImageType)
	        {
			    case ImageType.Image1D:
			        if (!Graphics.VideoDevice.Supports1DTextureFormat(Image.Settings.Format))
			        {
						return (Image)Resources.invalid_image_128x128.Clone();
			        }
			        break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
			        if (!Graphics.VideoDevice.Supports2DTextureFormat(Image.Settings.Format))
			        {
						return (Image)Resources.invalid_image_128x128.Clone();
			        }
			        break;
				case ImageType.Image3D:
			        if (!Graphics.VideoDevice.Supports3DTextureFormat(Image.Settings.Format))
			        {
						return (Image)Resources.invalid_image_128x128.Clone();
			        }
			        break;
				default:
					return (Image)Resources.invalid_image_128x128.Clone();
	        }

            // Resize our image.
            if ((Image[0].Width != 128)
                || (Image[0].Height != 128))
            {
                if (aspect > 1.0f)
                {
                    newSize.Height = (int)(128 / aspect);
                }
                else
                {
                    newSize.Width = (int)(128 * aspect);
                }

                Image.Resize(newSize.Width, newSize.Height, false, ImageFilter.Fant);
            }

            try
            {
                resultImage = Image[0].ToGDIImage();
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

		/// <summary>
		/// Function to load the image from a file system file entry.
		/// </summary>
		/// <param name="file">File to load from.</param>
	    public void Load(GorgonFileSystemFileEntry file)
	    {
			using (var stream = file.OpenStream(false))
			{
				if (!_codec.IsReadable(stream))
				{
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_CODEC_CANNOT_READ, file.Name, _codec.CodecDescription));
				}

				OnRead(stream);
			}
	    }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageContent"/> class.
        /// </summary>
        /// <param name="name">Name of the content.</param>
        /// <param name="codec">The codec used for the image.</param>
        public GorgonImageContent(string name, GorgonImageCodec codec)
            : base(name)
        {
            HasThumbnail = true;
	        _codec = codec;
        }
        #endregion

        #region IImageEditorContent Members

        /// <summary>
        /// Property to return the image held in the content object.
        /// </summary>
        public GorgonImageData Image
        {
            get;
            private set;
        }
        #endregion
    }
}
