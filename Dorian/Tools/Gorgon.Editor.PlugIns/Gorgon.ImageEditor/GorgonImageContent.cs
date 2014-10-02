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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Image content.
    /// </summary>
    class GorgonImageContent
        : ContentObject, IImageEditorContent
    {
        #region Variables.
	    private bool _disposed;													// Flag to indicate that the object was disposed.
        private GorgonImageContentPanel _contentPanel;                          // Panel used to display the content.
        private GorgonSwapChain _swap;											// The swap chain to display our texture.
        private IImageSettings _imageSettings;                                  // The current settings for the image.
	    private GorgonImageData _original;										// Original image.
        private int _depthArrayIndex;                                           // Current depth/array index.
        private GorgonConstantBuffer _depthArrayIndexData;                      // Depth array index value.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the depth/array index.
        /// </summary>
        public int DepthArrayIndex
        {
            get
            {
                return _depthArrayIndex;
            }
            set
            {
                if (_depthArrayIndex == value)
                {
                    return;
                }

                if (_depthArrayIndex < 0)
                {
                    _depthArrayIndex = 0;
                }

                int maxDepthArray = ImageType == ImageType.Image3D ? _imageSettings.Depth : _imageSettings.ArrayCount;

                if (_depthArrayIndex >= maxDepthArray)
                {
                    _depthArrayIndex = maxDepthArray - 1;
                }

                _depthArrayIndexData.Update(ref _depthArrayIndex);
            }
        }

        /// <summary>
        /// Property to return the 1D texture drawing pixel shader.
        /// </summary>
        public GorgonPixelShader Draw1D
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the 2D texture drawing pixel shader.
        /// </summary>
        public GorgonPixelShader Draw2D
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the 3D texture drawing pixel shader.
        /// </summary>
        public GorgonPixelShader Draw3D
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return information about the image format.
		/// </summary>
		[Browsable(false)]
	    public GorgonBufferFormatInfo.FormatData FormatInformation
	    {
		    get;
		    private set;
	    }

		/// <summary>
		/// Property to return the renderer interface.
		/// </summary>
		[Browsable(false)]
	    public Gorgon2D Renderer
	    {
		    get;
		    private set;
	    }

        /// <summary>
        /// Property to return whether this content has properties that can be manipulated in the properties tab.
        /// </summary>
        [Browsable(false)]
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
        [Browsable(false)]
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
        [Browsable(false)]
        public override bool HasRenderer
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Property to set or return the format for the image.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DATA"),
        LocalDescription(typeof(Resources), "PROP_IMAGEFORMAT_DESC"),
        LocalDisplayName(typeof(Resources), "PROP_IMAGEFORMAT_NAME"),
		TypeConverter(typeof(BufferFormatTypeConverter))]
        public BufferFormat ImageFormat
        {
            get
            {
                return _imageSettings.Format;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the width of the image.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DIMENSIONS"),
        LocalDescription(typeof(Resources), "PROP_WIDTH_DESC"),
        LocalDisplayName(typeof(Resources), "PROP_WIDTH_NAME")]
        public int Width
        {
            get
            {
                return _imageSettings.Width;
            }
            set
            {
	            if (value == _imageSettings.Width)
	            {
		            return;
	            }

	            SetSize(value, _imageSettings.Height, _imageSettings.Depth);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the height of the image.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DIMENSIONS"),
		LocalDescription(typeof(Resources), "PROP_HEIGHT_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_HEIGHT_NAME")]
		public int Height
        {
            get
            {
                return _imageSettings.Height;
            }
            set
            {
				if (value == _imageSettings.Height)
				{
					return;
				}

				SetSize(_imageSettings.Width, value, _imageSettings.Depth);
				NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the depth for a volume texture.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DIMENSIONS"),
		LocalDescription(typeof(Resources), "PROP_DEPTH_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_DEPTH_NAME")]
		public int Depth
        {
            get
            {
                return _imageSettings.Depth;
            }
            set
            {
				if (value == _imageSettings.Depth)
				{
					return;
				}

				SetSize(_imageSettings.Width, _imageSettings.Height, value);

                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the number of mip maps in the image.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_TEXTURE"),
		LocalDescription(typeof(Resources), "PROP_MIPCOUNT_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_MIPCOUNT_NAME")]
		public int MipCount
        {
            get
            {
                return _imageSettings.MipCount;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the number of array indices in the image.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_TEXTURE"),
		LocalDescription(typeof(Resources), "PROP_ARRAYCOUNT_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_ARRAYCOUNT_NAME")]
		public int ArrayCount
        {
            get
            {
                return _imageSettings.ArrayCount;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the type of image.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DIMENSIONS"),
		LocalDescription(typeof(Resources), "PROP_IMAGETYPE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_IMAGETYPE_NAME"),
		TypeConverter(typeof(ImageTypeTypeConverter))]
		public ImageType ImageType
        {
            get
            {
                return _imageSettings.ImageType;
            }
            set
            {
                if (value == _imageSettings.ImageType)
                {
                    return;
                }

                ChangeType(value);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the image codec used for this image.
        /// </summary>
        [Browsable(false)]
        public GorgonImageCodec Codec
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the properties for an image.
        /// </summary>
        private void ValidateImageProperties()
        {
            var info = GorgonBufferFormatInfo.GetInfo(_imageSettings.Format);

            DisableProperty("ArrayCount",
                            !Codec.SupportsArray
                            || _imageSettings.ImageType == ImageType.Image3D);

            DisableProperty("Depth", !Codec.SupportsDepth);

            DisableProperty("MipCount", !Codec.SupportsMipMaps);

            DisableProperty("ImageFormat", Codec.SupportedFormats.Count() < 2);

            DisableProperty("Height", _imageSettings.ImageType == ImageType.Image1D);

            DisableProperty("Depth",
                            _imageSettings.ImageType == ImageType.Image1D
                            || _imageSettings.ImageType == ImageType.Image2D
                            || _imageSettings.ImageType == ImageType.ImageCube);

            DisableProperty("ImageType", Codec.SupportsImageType.Count() < 2);
        }

		/// <summary>
		/// Function to determine if the settings for the current image are the same as the original.
		/// </summary>
		/// <param name="settings">Settings to test.</param>
		/// <returns>TRUE if the settings are the same, FALSE if not.</returns>
	    private bool IsSameAsOriginal(IImageSettings settings)
		{
			return ((settings.Width == _original.Settings.Width)
			        && (settings.Height == _original.Settings.Height)
			        && (settings.Depth == _original.Settings.Depth)
			        && (settings.Format == _original.Settings.Format)
			        && (settings.ArrayCount == _original.Settings.ArrayCount)
			        && (settings.MipCount == _original.Settings.MipCount)
                    && (settings.ImageType == _original.Settings.ImageType));
		}

        /// <summary>
        /// Function to change the image type.
        /// </summary>
        /// <param name="newType">The new image type.</param>
        private void ChangeType(ImageType newType)
        {
            IImageSettings newSettings = null;

            switch (newType)
            {
                case ImageType.Image1D:
                    if (!Graphics.VideoDevice.Supports1DTextureFormat(ImageFormat))
                    {
                        throw new InvalidCastException(string.Format(Resources.GORIMG_INVALID_FORMAT, ImageFormat, "1D"));    
                    }

                    newSettings = new GorgonTexture1DSettings
                                  {
                                      ArrayCount = ArrayCount,
                                      MipCount = MipCount,
                                      Format = ImageFormat,
                                      Width = Width
                                  };

                    
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    if (!Graphics.VideoDevice.Supports2DTextureFormat(ImageFormat))
                    {
                        throw new InvalidCastException(string.Format(Resources.GORIMG_INVALID_FORMAT, ImageFormat, "1D"));
                    }

                    newSettings = new GorgonTexture2DSettings
                                  {
                                      ArrayCount = ArrayCount,
                                      MipCount = MipCount,
                                      Format = ImageFormat,
                                      Width = Width,
                                      Height = Height,
                                      IsTextureCube = newType == ImageType.ImageCube
                                  };

                    // If we have chosen an image cube type, then we need to ensure that the array size is set to a multiple of 6.
                    if (newType == ImageType.ImageCube)
                    {
                        while ((newSettings.ArrayCount % 6) != 0)
                        {
                            ++newSettings.ArrayCount;
                        }
                    }
                    break;
                case ImageType.Image3D:
                    if (!Graphics.VideoDevice.Supports3DTextureFormat(ImageFormat))
                    {
                        throw new InvalidCastException(string.Format(Resources.GORIMG_INVALID_FORMAT, ImageFormat, "1D"));
                    }

                    newSettings = new GorgonTexture3DSettings
                                  {
                                      MipCount = MipCount,
                                      Format = ImageFormat,
                                      Width = Width,
                                      Height = Height,
                                      Depth = Depth
                                  };
                    break;
                default:
                    throw new InvalidCastException(string.Format(Resources.GORIMG_UNKNOWN_IMAGE_TYPE, newType));
            }

            // Do nothing if this image type is not different from the original.
            if (IsSameAsOriginal(newSettings))
            {
                _imageSettings = _original.Settings;

                if (Image == _original)
                {
                    return;
                }

                Image = _original;
                return;
            }

            GorgonImageData newImage = null;

            try
            {
                newImage = new GorgonImageData(newSettings);

                int depthArrayCount = (newSettings.ImageType == ImageType.Image3D
                                           ? newSettings.Depth
                                           : newSettings.ArrayCount).Min(_imageSettings.ImageType == ImageType.Image3D
                                                                             ? _imageSettings.Depth
                                                                             : _imageSettings.ArrayCount);

                // Copy the image data back into the new image type.
                for (int depth = 0; depth < depthArrayCount; ++depth)
                {
                    for (int mip = 0; mip < newSettings.MipCount; ++mip)
                    {
                        Image[mip, depth].CopyTo(newImage[mip, depth]);
                    }
                }
                
                if (Image != _original)
                {
                    Image.Dispose();
                }

                _imageSettings = newSettings;
                Image = newImage;
            }
            catch
            {
                if (newImage != null)
                {
                    newImage.Dispose();    
                }

                throw;
            }
            
            ValidateImageProperties();
        }

		/// <summary>
		/// Function to set the width and height of the image.
		/// </summary>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="depth">The depth of the image.</param>
	    private void SetSize(int width, int height, int depth)
	    {
			if ((width < 1)
				|| (height < 1)
				|| (depth < 1))
			{
				throw new ArgumentException(Resources.GORIMG_IMAGE_SIZE_TOO_SMALL);
			}

			// If we're changing back to the original width/height or depth, then just revert to the original image.
			var newSettings = _imageSettings.Clone();

			newSettings.Width = width;

			if (_imageSettings.ImageType != ImageType.Image1D)
			{
				newSettings.Height = height;
			}

			if (_imageSettings.ImageType == ImageType.Image3D)
			{
				newSettings.Depth = depth;
			}
			
			if (IsSameAsOriginal(newSettings))
			{
				_imageSettings = _original.Settings;

				if (Image == _original)
				{
					return;
				}

				Image.Dispose();
				Image = _original;

				return;
			}

			// We want to make adjustments to the original image, so this will be used to create a working copy.
			GorgonImageData newImage = null;

			try
			{
				// Ensure that our new dimensions are within tolerance.
				switch (_imageSettings.ImageType)
				{
					case ImageType.Image1D:
						if ((width > Graphics.Textures.MaxWidth)
						    || (height > 1)
						    || (depth > 1))
						{
							throw new ArgumentException(string.Format(Resources.GORIMG_IMAGE_1D_SIZE_TOO_LARGE, Graphics.Textures.MaxWidth));
						}

						newImage = _original.Clone();
						height = 1;
						break;
					case ImageType.Image2D:
					case ImageType.ImageCube:
						if ((width > Graphics.Textures.MaxWidth)
						    || (height > Graphics.Textures.MaxHeight)
						    || (depth > 1))
						{
							throw new ArgumentException(string.Format(Resources.GORIMG_IMAGE_2D_SIZE_TOO_LARGE, Graphics.Textures.MaxWidth, Graphics.Textures.MaxHeight));
						}

						newImage = _original.Clone();
						break;
					case ImageType.Image3D:
						if ((width > Graphics.Textures.MaxWidth)
						    || (height > Graphics.Textures.MaxHeight)
						    || (depth > Graphics.Textures.MaxDepth))
						{
							throw new ArgumentException(string.Format(Resources.GORIMG_IMAGE_3D_SIZE_TOO_LARGE,
							                                          Graphics.Textures.MaxWidth,
							                                          Graphics.Textures.MaxHeight,
							                                          Graphics.Textures.MaxDepth));
						}

						// If the depth has changed, then we need to do some special work to get it resized.
						if (depth != _imageSettings.Depth)
						{
							newSettings.Width = _imageSettings.Width;
							newSettings.Height = _imageSettings.Height;
							newSettings.Depth = depth;

							newImage = new GorgonImageData(newSettings);

							// Copy the image data into the new image.
							int minDepth = depth.Min(_imageSettings.Depth);

							_imageSettings = newImage.Settings;

							for (int i = 0; i < minDepth; ++i)
							{
								for (int mip = 0; i < newSettings.MipCount; ++i)
								{
									GorgonImageBuffer sourceBuffer = Image[mip, i];
									GorgonImageBuffer destBuffer = newImage[mip, i];

									sourceBuffer.CopyTo(destBuffer);
								}
							}

						}
						break;
                    default:
                        throw new InvalidCastException(string.Format(Resources.GORIMG_UNKNOWN_IMAGE_TYPE, _imageSettings.ImageType));
				}

				Debug.Assert(newImage != null, "Temporary image not created!");

				newImage.Resize(width, height, false);
				_imageSettings = newImage.Settings;

				if (Image != _original)
				{
					Image.Dispose();
				}

				Image = newImage;
			}
			catch
			{
				// Get rid of our working copy.
				if (newImage != null)
				{
					newImage.Dispose();
				}

				throw;
			}
	    }

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
	    protected override void Dispose(bool disposing)
	    {
		    if (!_disposed)
		    {
		        if (_depthArrayIndexData != null)
		        {
		            _depthArrayIndexData.Dispose();
		        }

				if ((Image != null) && (Image != _original))
				{
					Image.Dispose();
				}

				if (_original != null)
			    {
				    _original.Dispose();
			    }

			    if (Renderer != null)
			    {
				    Renderer.Dispose();
			    }

			    if (_swap != null)
			    {
				    _swap.Dispose();
			    }

		        _depthArrayIndexData = null;
			    _swap = null;
				Renderer = null;
			    Image = null;
			    _original = null;
		    }

		    _disposed = true;

		    base.Dispose(disposing);
	    }

	    /// <summary>
        /// Function to persist the content data to a stream.
        /// </summary>
        /// <param name="stream">Stream that will receive the data.</param>
        protected override void OnPersist(Stream stream)
        {
        }

		/// <summary>
		/// Function called when the name is about to be changed.
		/// </summary>
		/// <param name="proposedName">The proposed name for the content.</param>
		/// <returns>
		/// A valid name for the content.
		/// </returns>
	    protected override string ValidateName(string proposedName)
		{
			if (string.IsNullOrWhiteSpace(proposedName))
			{
				return string.Empty;
			}

			// If we have no codec, then we'll have to assume that the name is good for now.
			if (Codec == null)
			{
				return proposedName;
			}

			// Remember the previous extension.
			string extension = Path.GetExtension(Name);

			// If the current codec is OK with the extension, then let the proposed name pass.
			var stringBuffer = new StringBuilder(proposedName.Length);
			if (Codec.CodecCommonExtensions.Any(item =>
			                                    {
				                                    stringBuffer.Length = 0;
				                                    stringBuffer.Append(".");
				                                    stringBuffer.Append(item);

				                                    return proposedName.EndsWith(stringBuffer.ToString(), StringComparison.OrdinalIgnoreCase);
			                                    }))
			{
				return proposedName;
			}

			// Otherwise, attach the old extension.
			stringBuffer.Length = 0;
			stringBuffer.Append(proposedName);
			stringBuffer.Append(extension);

			return stringBuffer.ToString().FormatFileName();
		}

	    /// <summary>
        /// Function to read the content data from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the content data.</param>
        protected override void OnRead(Stream stream)
        {
            Image = GorgonImageData.FromStream(stream, (int)stream.Length, Codec);

		    if (_original != null)
		    {
			    _original.Dispose();
			    _original = null;
		    }

		    _original = Image;
            _imageSettings = Image.Settings;
	
            ValidateImageProperties();
        }

        /// <summary>
        /// Function called when the content is being initialized.
        /// </summary>
        /// <returns>
        /// A control to place in the primary interface window.
        /// </returns>
        protected override ContentPanel OnInitialize()
        {
	        _contentPanel = new GorgonImageContentPanel
	                        {
		                        Content = this
	                        };

	        _swap = Graphics.Output.CreateSwapChain("TextureDisplay",
	                                                new GorgonSwapChainSettings
	                                                {
														Window = _contentPanel.panelTextureDisplay,
														Format = BufferFormat.R8G8B8A8_UIntNormal
	                                                });

			Renderer = Graphics.Output.Create2DRenderer(_swap);

#if DEBUG
            Draw1D = Graphics.Shaders.CreateShader<GorgonPixelShader>("1D Texture",
                                                                      "Gorgon1DTextureView",
                                                                      Resources.ImageViewShaders);

            Draw2D = Graphics.Shaders.CreateShader<GorgonPixelShader>("2D Texture",
                                                                      "Gorgon2DTextureView",
                                                                      Resources.ImageViewShaders);

            Draw3D = Graphics.Shaders.CreateShader<GorgonPixelShader>("3D Texture",
                                                                      "Gorgon3DTextureView",
                                                                      Resources.ImageViewShaders);
#else
            Draw1D = Graphics.Shaders.CreateShader<GorgonPixelShader>("1D Texture",
                                                                      "Gorgon1DTextureView",
                                                                      Resources.ImageViewShaders,
                                                                      null,
                                                                      false);

            Draw2D = Graphics.Shaders.CreateShader<GorgonPixelShader>("2D Texture",
                                                                      "Gorgon2DTextureView",
                                                                      Resources.ImageViewShaders,
                                                                      null,
                                                                      false);


            Draw3D = Graphics.Shaders.CreateShader<GorgonPixelShader>("3D Texture",
                                                                      "Gorgon3DTextureView",
                                                                      Resources.ImageViewShaders,
                                                                      null,
                                                                      false);
#endif

            // Create our depth/array index value for the shaders.
            _depthArrayIndexData = Graphics.Buffers.CreateConstantBuffer("DepthArrayIndex",
                                                                         new GorgonConstantBufferSettings
                                                                         {
                                                                             SizeInBytes = 16
                                                                         });
            _depthArrayIndexData.Update(ref _depthArrayIndex);

            Graphics.Shaders.PixelShader.ConstantBuffers[1] = _depthArrayIndexData;
            

	        _contentPanel.CreateResources();

            return _contentPanel;
        }

		/// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
	    public override void Draw()
		{
			Renderer.Target = _swap;
			_contentPanel.Draw();

			Renderer.Render(1);
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

		    FormatInformation = GorgonBufferFormatInfo.GetInfo(Image.Settings.Format);

			// We can't thumbnail block compressed images, so show the default.
		    if (FormatInformation.IsCompressed)
		    {
			    return (Image)Resources.image_128x128.Clone();
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
		/// <param name="fileName">The file name of the file to load.</param>
		/// <param name="stream">The stream containing the file.</param>
	    public void Load(string fileName, Stream stream)
	    {
			if (!Codec.IsReadable(stream))
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_CODEC_CANNOT_READ, fileName, Codec.CodecDescription));
			}

			OnRead(stream);
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
	        Codec = codec;

            // Make a default, empty, 2D settings object so we have something to work with.
            _imageSettings = new GorgonTexture2DSettings();
        }
        #endregion

        #region IImageEditorContent Members

        /// <summary>
        /// Property to return the image held in the content object.
        /// </summary>
        [Browsable(false)]
        public GorgonImageData Image
        {
            get;
            private set;
        }
        #endregion
    }
}
