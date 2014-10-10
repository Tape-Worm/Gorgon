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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Image content.
    /// </summary>
    sealed class GorgonImageContent
        : ContentObject, IImageEditorContent
    {
        #region Variables.
		// List of D3D formats for use with texconv.exe.
	    // ReSharper disable once InconsistentNaming
	    private readonly Dictionary<BufferFormat, string> _d3dFormats = new Dictionary<BufferFormat, string>
	    {
		    {
				BufferFormat.BC1_UIntNormal,
   				"B8G8R8A8_UNORM"
		    },
		    {
				BufferFormat.BC1_UIntNormal_sRGB,
   				"B8G8R8A8_UNORM_SRGB"
		    },
		    {
				BufferFormat.BC2_UIntNormal,
   				"B8G8R8A8_UNORM"
		    },
		    {
				BufferFormat.BC2_UIntNormal_sRGB,
   				"B8G8R8A8_UNORM_SRGB"
		    },
		    {
				BufferFormat.BC3_UIntNormal,
   				"R8G8B8A8_UNORM"
		    },
		    {
				BufferFormat.BC3_UIntNormal_sRGB,
   				"R8G8B8A8_UNORM_SRGB"
		    },
		    {
				BufferFormat.BC4_IntNormal,
   				"R8_SNORM"
		    },
		    {
				BufferFormat.BC4_UIntNormal,
   				"R8_UNORM"
		    },
		    {
				BufferFormat.BC5_IntNormal,
   				"R8G8_SNORM"
		    },
		    {
				BufferFormat.BC5_UIntNormal,
   				"R8G8_UNORM"
		    },
		    {
				BufferFormat.BC6H_SF16,
   				"R16G16B16A16_FLOAT"
		    },
		    {
				BufferFormat.BC6H_UF16,
   				"R16G16B16A16_FLOAT"
		    },
		    {
				BufferFormat.BC7_UIntNormal,
   				"R8G8B8A8_UNORM"
		    },
		    {
				BufferFormat.BC7_UIntNormal_sRGB,
   				"R8G8B8A8_UNORM_SRGB"
		    }
	    };

	    private bool _disposed;													// Flag to indicate that the object was disposed.
        private GorgonImageContentPanel _contentPanel;                          // Panel used to display the content.
        private GorgonSwapChain _swap;											// The swap chain to display our texture.
        private IImageSettings _imageSettings;                                  // The current settings for the image.
	    private GorgonImageData _original;										// Original image.
        private int _depthArrayIndex;                                           // Current depth/array index.
        private GorgonConstantBuffer _depthArrayIndexData;                      // Depth array index value.
	    private readonly byte[] _copyBuffer = new byte[80000];					// 80k copy buffer.
        private BufferFormat _blockCompression = BufferFormat.Unknown;          // Block compression type.
	    private GorgonImageCodec _codec;										// The codec used.
	    private GorgonImageCodec _originalCodec;								// The original codec used.
        #endregion

        #region Properties.
	    /// <summary>
		/// Property to return whether the image editor can modify block compressed images.
		/// </summary>
		[Browsable(false)]
	    public bool CanChangeBCImages
	    {
		    get;
		    private set;
	    }

        /// <summary>
        /// Property to set or return the depth/array index.
        /// </summary>
        [Browsable(false)]
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
		[Browsable(false)]
        public GorgonPixelShader Draw1D
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the 2D texture drawing pixel shader.
        /// </summary>
		[Browsable(false)]
        public GorgonPixelShader Draw2D
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the 3D texture drawing pixel shader.
        /// </summary>
		[Browsable(false)]
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
				if ((value == _imageSettings.Format)
					|| (_contentPanel == null))
	            {
		            return;
	            }

				TransformImage(ImageType, Width, Height, Depth, MipCount, ArrayCount, value);

                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the level of block compression used.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DATA"),
        LocalDescription(typeof(Resources), "PROP_BLOCKCOMP_DESC"),
        LocalDisplayName(typeof(Resources), "PROP_BLOCKCOMP_NAME"),
        TypeConverter(typeof(CompressionTypeConverter))]
        public BufferFormat BlockCompression
        {
            get
            {
                return _blockCompression;
            }
            set
            {
                if ((value == _blockCompression)
                    || (_contentPanel == null))
                {
                    return;
                }

                // This property is only applied when we save.
                _blockCompression = value;

                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the width of the image.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DIMENSIONS"),
        LocalDescription(typeof(Resources), "PROP_WIDTH_DESC"),
        LocalDisplayName(typeof(Resources), "PROP_WIDTH_NAME"),
		RefreshProperties(RefreshProperties.All)]
        public int Width
        {
            get
            {
                return _imageSettings.Width;
            }
            set
            {
	            if ((value == _imageSettings.Width)
					|| (_contentPanel == null))
	            {
		            return;
	            }

				TransformImage(ImageType, value, Height, Depth, MipCount, ArrayCount, ImageFormat);
	            
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the height of the image.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DIMENSIONS"),
		LocalDescription(typeof(Resources), "PROP_HEIGHT_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_HEIGHT_NAME"),
		RefreshProperties(RefreshProperties.All)]
		public int Height
        {
            get
            {
                return _imageSettings.Height;
            }
            set
            {
				if ((value == _imageSettings.Height)
					|| (_contentPanel == null))
				{
					return;
				}

				TransformImage(ImageType, Width, value, Depth, MipCount, ArrayCount, ImageFormat);

				NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the depth for a volume texture.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DIMENSIONS"),
		LocalDescription(typeof(Resources), "PROP_DEPTH_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_DEPTH_NAME"),
		RefreshProperties(RefreshProperties.All)]
		public int Depth
        {
            get
            {
                return _imageSettings.Depth;
            }
            set
            {
				if ((value == _imageSettings.Depth)
					|| (_contentPanel == null))
				{
					return;
				}

				TransformImage(ImageType, Width, Height, value, MipCount, ArrayCount, ImageFormat);

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
	            if ((value == _imageSettings.MipCount)
	                || (_contentPanel == null))
	            {
		            return;
	            }

				TransformImage(ImageType, Width, Height, Depth, value, ArrayCount, ImageFormat);

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
				if ((value == _imageSettings.ArrayCount)
					|| (_contentPanel == null))
	            {
		            return;
	            }

				TransformImage(ImageType, Width, Height, Depth, MipCount, value, ImageFormat);

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
                if ((value == _imageSettings.ImageType)
					|| (_contentPanel == null))
                {
                    return;
                }

				TransformImage(value, Width, Height, Depth, MipCount, ArrayCount, ImageFormat);

                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the image codec used for this image.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DATA"),
		LocalDescription(typeof(Resources), "PROP_CODEC_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_COEC_NAME"),
		TypeConverter(typeof(CodecFormatTypeConverter))]
        public GorgonImageCodec Codec
        {
	        get
	        {
		        return _codec;
	        }
	        set
	        {
		        if ((value == null)
					|| (value == _codec)
					|| (_contentPanel == null))
		        {
			        return;
		        }

		        _codec = value;

		        ValidateImageProperties();

				NotifyPropertyChanged();
	        }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to perform the actual processing of the image.
		/// </summary>
		/// <param name="newSettings">Settings to apply.</param>
	    private void ProcessTransform(IImageSettings newSettings)
		{
			// Don't change if we have the same settings as the original.
			if (IsSameAsOriginal(newSettings))
			{
				_imageSettings = _original.Settings.Clone();

				if (Image != _original)
				{
					Image.Dispose();
				}

				Image = _original;
				ValidateImageProperties();
				return;
			}

			BufferFormat convertFormat = newSettings.Format;
			int convertWidth = newSettings.Width;
			int convertHeight = newSettings.ImageType == ImageType.Image1D ? 1 : newSettings.Height;
			newSettings.Format = _original.Settings.Format;
			newSettings.Width = _original.Settings.Width;

			if (newSettings.ImageType != ImageType.Image1D)
			{
				newSettings.Height = _original.Settings.Height;
			}

			GorgonImageData newImage = null;

			try
			{
				newImage = new GorgonImageData(newSettings);
				_original.CopyTo(newImage);

				// Convert the format.
				if (convertFormat != _imageSettings.Format)
				{
					newImage.ConvertFormat(convertFormat);
				}

				// Resize the image.
				if ((convertWidth != _imageSettings.Width)
				    || (convertHeight != _imageSettings.Height))
				{
					newImage.Resize(convertWidth, convertHeight, false);
				}

				if (Image != _original)
				{
					Image.Dispose();
				}

				Image = newImage;
				_imageSettings = newImage.Settings.Clone();
			}
			catch
			{
				if (newImage != null)
				{
					newImage.Dispose();
				}

				throw;
			}
			finally
			{
				ValidateImageProperties();
			}
		}

		/// <summary>
		/// Function to perform transforms on the image data.
		/// </summary>
		/// <param name="type">Type of image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image (if applicable).</param>
		/// <param name="depth">Depth of the image (if applicable).</param>
		/// <param name="mipCount">Number of mip maps.</param>
		/// <param name="arrayCount">Number of array indices.</param>
		/// <param name="format">Pixel format for the image.</param>
	    private void TransformImage(ImageType type, int width, int height, int depth, int mipCount, int arrayCount, BufferFormat format)
		{
			IImageSettings newSettings;

			if (!Graphics.VideoDevice.SupportsMipMaps(format))
			{
				throw new ArgumentException(string.Format(Resources.GORIMG_TEXTURE_FORMAT_CANNOT_MIP, format), "format");
			}

			if ((type != ImageType.Image3D) && ((arrayCount < 1)
				|| (arrayCount > Graphics.Textures.MaxArrayCount)))
			{
				throw new ArgumentOutOfRangeException("arrayCount", string.Format(Resources.GORIMG_ARRAY_COUNT_INVALID, Graphics.Textures.MaxArrayCount));
			}

			int calcMaxMip = GorgonImageData.GetMaxMipCount(Width, Height, Depth);

			if ((mipCount < 1)
				|| (mipCount > calcMaxMip))
			{
				throw new ArgumentOutOfRangeException("mipCount", string.Format(Resources.GORIMG_MIP_COUNT_INVALID, calcMaxMip));
			}


			// Ensure that we can resize.
			if (width < 1)
			{
				throw new ArgumentOutOfRangeException("width", Resources.GORIMG_IMAGE_SIZE_TOO_SMALL);
			}

			switch (type)
			{
				case ImageType.Image1D:
                    if (!Graphics.VideoDevice.Supports1DTextureFormat(format))
                    {
                        throw new ArgumentException(string.Format(Resources.GORIMG_INVALID_FORMAT, format, "1D"), "format");    
                    }

					if (width > Graphics.Textures.MaxWidth)
					{
						throw new ArgumentOutOfRangeException("width", string.Format(Resources.GORIMG_IMAGE_1D_SIZE_TOO_LARGE, Graphics.Textures.MaxWidth));
					}

                    newSettings = new GorgonTexture1DSettings
                                  {
                                      ArrayCount = arrayCount,
                                      MipCount = mipCount,
                                      Format = format,
                                      Width = width
                                  };
                    
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    if (!Graphics.VideoDevice.Supports2DTextureFormat(format))
                    {
                        throw new ArgumentException(string.Format(Resources.GORIMG_INVALID_FORMAT, ImageFormat, "2D"), "format");
                    }

                    newSettings = new GorgonTexture2DSettings
                                  {
                                      ArrayCount = arrayCount,
                                      MipCount = mipCount,
                                      Format = format,
                                      Width = width,
                                      Height = height,
                                      IsTextureCube = type == ImageType.ImageCube
                                  };

					// If we have chosen an image cube type, then we need to ensure that the array size is set to a multiple of 6.
					if (type == ImageType.ImageCube)
					{
						while ((newSettings.ArrayCount % 6) != 0)
						{
							++newSettings.ArrayCount;
						}

						// If we adjusted the array count, then ensure we didn't exceed the maximum.
						if (newSettings.ArrayCount > Graphics.Textures.MaxArrayCount)
						{
							throw new ArgumentOutOfRangeException("arrayCount", string.Format(Resources.GORIMG_ARRAY_COUNT_INVALID, Graphics.Textures.MaxArrayCount));
						}
					}

					// Ensure that we can resize.
					if (width > Graphics.Textures.MaxWidth)
					{
						throw new ArgumentOutOfRangeException("width", string.Format(Resources.GORIMG_IMAGE_1D_SIZE_TOO_LARGE, Graphics.Textures.MaxWidth));
					}

					if (height < 1)
					{
						throw new ArgumentOutOfRangeException("height", Resources.GORIMG_IMAGE_SIZE_TOO_SMALL);
					}

					if (height > Graphics.Textures.MaxHeight)
					{
						throw new ArgumentOutOfRangeException("height", string.Format(Resources.GORIMG_IMAGE_2D_SIZE_TOO_LARGE, Graphics.Textures.MaxHeight));
					}

                    break;
                case ImageType.Image3D:
                    if (!Graphics.VideoDevice.Supports3DTextureFormat(ImageFormat))
                    {
                        throw new ArgumentException(string.Format(Resources.GORIMG_INVALID_FORMAT, ImageFormat, "3D"), "format");
                    }

		            if ((MipCount > 1)
						&& (!_imageSettings.IsPowerOfTwo))
		            {
			            throw new ArgumentException(Resources.GORIMG_TEXTURE_MIP_POW_2, "type");
		            }

					// Ensure that we can resize.
					if (width > Graphics.Textures.Max3DWidth)
					{
						throw new ArgumentOutOfRangeException("width", string.Format(Resources.GORIMG_IMAGE_3D_SIZE_TOO_LARGE, Graphics.Textures.Max3DWidth));
					}

					if (height < 1)
					{
						throw new ArgumentOutOfRangeException("height", Resources.GORIMG_IMAGE_SIZE_TOO_SMALL);
					}

					if (height > Graphics.Textures.Max3DHeight)
					{
						throw new ArgumentOutOfRangeException("height", string.Format(Resources.GORIMG_IMAGE_3D_SIZE_TOO_LARGE, Graphics.Textures.Max3DHeight));
					}

					if (depth < 1)
					{
						throw new ArgumentOutOfRangeException("depth", Resources.GORIMG_IMAGE_SIZE_TOO_SMALL);
					}

					if (depth > Graphics.Textures.MaxDepth)
					{
						throw new ArgumentOutOfRangeException("depth", string.Format(Resources.GORIMG_IMAGE_3D_SIZE_TOO_LARGE, Graphics.Textures.MaxDepth));
					}

					newSettings = new GorgonTexture3DSettings
					              {
						              MipCount = mipCount,
						              Format = format,
						              Width = width,
						              Height = height,
						              Depth = depth
					              };

					break;
				default:
					throw new InvalidCastException(string.Format(Resources.GORIMG_UNKNOWN_IMAGE_TYPE, type));
			}

			ProcessTransform(newSettings);
		}

        /// <summary>
        /// Function to return whether an image format can be block compressed.
        /// </summary>
        /// <returns>TRUE if format can be block compressed, FALSE if not.</returns>
        private bool FormatCanBlockCompress()
        {
            switch (ImageFormat)
            {
                case BufferFormat.R8G8B8A8_UIntNormal:
                case BufferFormat.B8G8R8X8_UIntNormal:
                case BufferFormat.B8G8R8A8_UIntNormal:
                case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
                case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
                case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
                case BufferFormat.R8_IntNormal:
                case BufferFormat.R8_UIntNormal:
                case BufferFormat.R8G8_IntNormal:
                case BufferFormat.R8G8_UIntNormal:
                case BufferFormat.R16G16B16A16_Float:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Function to validate the properties for an image.
        /// </summary>
        private void ValidateImageProperties()
        {
			if (((!CanChangeBCImages)
	            && (FormatInformation.IsCompressed))
				|| (HasOwner))
	        {
		        DisableProperty("ArrayCount", true);
				DisableProperty("Depth", true);
				DisableProperty("MipCount", true);
				DisableProperty("ImageFormat", true);
				DisableProperty("Height", true);
				DisableProperty("Width", true);
				DisableProperty("Depth",true);
				DisableProperty("ImageType", true);
                DisableProperty("BlockCompression", true);
				DisableProperty("Codec", true);
				return;
	        }

            bool blockCompressionDisabled = !Codec.SupportsBlockCompression;

            if (!blockCompressionDisabled)
            {
                // Ensure that the current format can be block compressed.
                blockCompressionDisabled = !FormatCanBlockCompress();
                
                if (blockCompressionDisabled)
                {
                    BlockCompression = BufferFormat.Unknown;
                }
            }
            else
            {
                BlockCompression = BufferFormat.Unknown;                
            }
            
            DisableProperty("BlockCompression", blockCompressionDisabled);
            DisableProperty("ArrayCount",
                            (!Codec.SupportsArray
                            || _imageSettings.ImageType == ImageType.Image3D));

            DisableProperty("MipCount", !Codec.SupportsMipMaps);

            DisableProperty("ImageFormat", Codec.SupportedFormats.Count() < 2);

            DisableProperty("Height", _imageSettings.ImageType == ImageType.Image1D);

            DisableProperty("Depth",
                            !Codec.SupportsDepth
							|| _imageSettings.ImageType == ImageType.Image1D
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
		/// Function to delete a temporary image file.
		/// </summary>
		/// <param name="path">The path to the temporary image file.</param>
	    private static void DeleteTempImageFile(string path)
	    {
		    try
		    {
			    if (!File.Exists(path))
			    {
				    return;
			    }

				File.Delete(path);
		    }
			// ReSharper disable once EmptyGeneralCatchClause
		    catch 
		    {
				// Intentionally left blank.
				// We don't care if the delete was not successful.
		    }
	    }

		/// <summary>
		/// Function to decompress a block compressed image into an uncompressed texture format.
		/// </summary>
		/// <param name="stream">Stream containing the image format.</param>
		/// <param name="size">Size of the image data within the stream, in bytes.</param>
		/// <returns>The image data for the decompressed image.</returns>
	    private GorgonImageData DecompressBCImage(Stream stream, int size)
		{
			Process texConvProcess = null;

			if ((_contentPanel == null)
				|| (_contentPanel.ParentForm == null))
			{
				return null;
			}

			string texConvPath = Gorgon.ApplicationDirectory + "texconv.exe";

			// If we can't find our converter, then we're out of luck.
			if (!File.Exists(texConvPath))
			{
				return null;
			}

			// We'll need to copy this data and decompress it in an external source.
			string tempFilePath = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(Filename));
			string tempFileDirectory = Path.GetDirectoryName(tempFilePath).FormatDirectory(Path.DirectorySeparatorChar);
			string outputName = tempFileDirectory + "decomp_" + Path.GetFileName(tempFilePath);

			Debug.Assert(!string.IsNullOrWhiteSpace(tempFileDirectory), "No temporary directory!");

			Cursor.Current = Cursors.WaitCursor;
			try
			{

				// Copy our file.
				using (Stream outStream = File.Open(tempFilePath, FileMode.Create, FileAccess.Write))
				{
					int remaining = size;

					while (remaining > 0)
					{
						int readLength = remaining > _copyBuffer.Length ? _copyBuffer.Length : remaining;

						int readAmount = stream.Read(_copyBuffer, 0, readLength);
						outStream.Write(_copyBuffer, 0, readAmount);

						remaining -= readAmount;
					}
				}

				// If we can't find our converter, then we're out of luck.
				if (!File.Exists(texConvPath))
				{
					DeleteTempImageFile(tempFilePath);
					return null;
				}

				var info = new ProcessStartInfo
				           {
					           Arguments =
						           "-f " + _d3dFormats[FormatInformation.Format] + " -fl " +
						           (Graphics.VideoDevice.SupportedFeatureLevel > DeviceFeatureLevel.SM4_1 ? "11.0" : "10.0") + " -ft DDS -o \"" +
						           Path.GetDirectoryName(tempFilePath) + "\" -nologo -px decomp_ \"" + tempFilePath + "\"",
					           ErrorDialog = true,
					           ErrorDialogParentHandle = _contentPanel.ParentForm.Handle,
					           FileName = texConvPath,
					           WorkingDirectory = tempFileDirectory,
					           UseShellExecute = false,
#if DEBUG
							   CreateNoWindow = false,
#else
							   CreateNoWindow = true,
#endif
							   RedirectStandardError = true,
							   RedirectStandardOutput = true
				           };

				texConvProcess = Process.Start(info);

				if (texConvProcess == null)
				{
					return null;
				}

				// Wait until we're done converting.
				texConvProcess.WaitForExit();

				// Check standard the error stream for errors.
				string errorData = texConvProcess.StandardError.ReadToEnd();

				if (!string.IsNullOrWhiteSpace(errorData))
				{
					GorgonDialogs.ErrorBox(_contentPanel.ParentForm, Resources.GORIMG_ERROR_DECOMPRESSING, string.Empty, errorData, true);
					return null;
				}

				errorData = texConvProcess.StandardOutput.ReadToEnd();

				// Check for invalid parameters.
				if (errorData.StartsWith("Invalid value", StringComparison.OrdinalIgnoreCase))
				{
					GorgonDialogs.ErrorBox(_contentPanel.ParentForm,
					                       Resources.GORIMG_ERROR_DECOMPRESSING,
					                       string.Empty,
					                       errorData.Substring(0, errorData.IndexOf("\n", StringComparison.OrdinalIgnoreCase)),
					                       true);
					return null;
				}

				// Check for failure.
				if (!errorData.Contains("FAILED"))
				{
					return GorgonImageData.FromFile(outputName, new GorgonCodecDDS());
				}

				// Get rid of the temporary path and use the file name.
				errorData = errorData.Replace(tempFilePath, "\"" + Filename + "\"");
				GorgonDialogs.ErrorBox(_contentPanel.ParentForm,
				                       Resources.GORIMG_ERROR_DECOMPRESSING,
				                       string.Empty,
				                       errorData,
				                       true);
				return null;
			}
			finally
			{
				if (texConvProcess != null)
				{
					texConvProcess.Close();
				}

				// Remove the copied file.
				DeleteTempImageFile(tempFilePath);
				DeleteTempImageFile(outputName);
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
		/// Function to update the filename for this content.
		/// </summary>
		/// <returns>
		/// The new file name.
		/// </returns>
	    private string RenameImageForCodec()
	    {
			if (Codec == null)
			{
				return null;
			}

			string extension = Path.GetExtension(Filename);
			var stringBuffer = new StringBuilder(Name.Length + 4);

			// The current filename is sufficient.
			if ((string.IsNullOrWhiteSpace(extension))
			    || (!Codec.CodecCommonExtensions.Any(item =>
			                                         {
				                                         stringBuffer.Length = 0;
				                                         stringBuffer.Append(".");
				                                         stringBuffer.Append(item);

				                                         return string.Equals(extension, stringBuffer.ToString(), StringComparison.OrdinalIgnoreCase);
			                                         })))
			{
				extension = Codec.CodecCommonExtensions.First();
			}

			// Otherwise, use the first common extension for the codec.
			stringBuffer.Length = 0;
			stringBuffer.Append(Name);
			stringBuffer.Append(".");
			stringBuffer.Append(extension);

			return stringBuffer.ToString();
	    }

	    /// <summary>
        /// Function to read the content data from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the content data.</param>
        protected override void OnRead(Stream stream)
        {
			// If we're not actuallying "editing" the image, but using this interface to load an image, 
			// then just leave so we can keep the image data lightweight.
		    if (_contentPanel == null)
		    {
				// Get image metadata first to determine if it's compressed.
				Image = GorgonImageData.FromStream(stream, (int)stream.Length, Codec);
				_imageSettings = Image.Settings;
				FormatInformation = GorgonBufferFormatInfo.GetInfo(Image.Settings.Format);
				return;
		    }

		    IImageSettings settings = Codec.GetMetaData(stream);
			FormatInformation = GorgonBufferFormatInfo.GetInfo(settings.Format);

		    long position = stream.Position;

		    if (FormatInformation.IsCompressed)
		    {
				_blockCompression = settings.Format;
			    Image = DecompressBCImage(stream, (int)stream.Length);
		    }

		    if (Image == null)
		    {
				// Reset the stream if we've attempted to load a compressed image.
			    stream.Position = position;

				// If this is a result of the image not being decompressed, then lock down editing on the compressed image.
			    if (FormatInformation.IsCompressed)
			    {
				    CanChangeBCImages = false;
			    }

			    Image = GorgonImageData.FromStream(stream, (int)stream.Length, Codec);
		    }

		    if (_original != null)
		    {
			    _original.Dispose();
			    _original = null;
		    }

		    _original = Image;
            _imageSettings = Image.Settings.Clone();
			FormatInformation = GorgonBufferFormatInfo.GetInfo(_imageSettings.Format);
	
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
	        CanChangeBCImages = File.Exists(Gorgon.ApplicationDirectory + "texconv.exe");

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
            float aspect = (float)Image.Buffers[0].Width / Image.Buffers[0].Height;
            var newSize = new Size(128, 128);

			// If this device can't support loading the image, then show
			// a thumbnail that will indicate that we can't load it.
	        if ((Image.Buffers[0].Width >= Graphics.Textures.MaxWidth)
	            || (Image.Buffers[0].Height >= Graphics.Textures.MaxHeight)
	            || (Image.Buffers[0].Depth >= Graphics.Textures.MaxDepth))
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
            if ((Image.Buffers[0].Width != 128)
                || (Image.Buffers[0].Height != 128))
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
                resultImage = Image.Buffers[0].ToGDIImage();
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
        /// <param name="settings">Settings for the image content.</param>
        public GorgonImageContent(GorgonImageContentSettings settings)
            : base(settings)
        {
            HasThumbnail = true;
	        _originalCodec = _codec = settings.Codec;

            // Make a default, empty, 2D settings object so we have something to work with.
            _imageSettings = new GorgonTexture2DSettings();

	        if (settings.ImageStream == null)
	        {
		        return;
	        }

			if (!Codec.IsReadable(settings.ImageStream))
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_CODEC_CANNOT_READ, settings.Name, Codec.CodecDescription));
			}

			OnRead(settings.ImageStream);
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
