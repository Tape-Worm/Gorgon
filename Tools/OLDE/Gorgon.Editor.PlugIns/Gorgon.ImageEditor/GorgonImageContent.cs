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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Design;
using Gorgon.Editor.ImageEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Image content.
    /// </summary>
    sealed class GorgonImageContent
        : ContentObject, IImageEditorContent
	{
		#region Value Types.
		/// <summary>
		/// Parameters to pass to the texture shader(s).
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Size = 16, Pack = 16)]
	    private struct TextureParams
		{
			/// <summary>
			/// Size of the structure, in bytes.
			/// </summary>
			public readonly static int Size = DirectAccess.SizeOf<TextureParams>();

			/// <summary>
			/// Depth slice index.
			/// </summary>
		    public float DepthSlice;
			/// <summary>
			/// Array index.
			/// </summary>
		    public float ArrayIndex;
			/// <summary>
			/// Mip map level.
			/// </summary>
		    public float MipLevel;
	    }
		#endregion

		#region Classes.
		/// <summary>
		/// Values for the file watcher.
		/// </summary>
	    private class FileWatchValues
	    {
			/// <summary>
			/// Property to set or return whether the file being watched has been changed.
			/// </summary>
			public bool IsChanged
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the file name of the file being watched.
			/// </summary>
			public string Filename
			{
				get;
				set;
			}
	    }
		#endregion

		#region Variables.
		// Compression format strings.
	    private readonly Dictionary<BufferFormat, string> _compFormats = new Dictionary<BufferFormat, string>
	                                                                     {
		                                                                     {
			                                                                     BufferFormat.BC1_UIntNormal,
			                                                                     "BC1_UNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC1_UIntNormal_sRGB,
			                                                                     "BC1_UNORM_SRGB"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC2_UIntNormal,
			                                                                     "BC2_UNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC2_UIntNormal_sRGB,
			                                                                     "BC2_UNORM_SRGB"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC3_UIntNormal,
			                                                                     "BC3_UNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC3_UIntNormal_sRGB,
			                                                                     "BC3_UNORM_SRGB"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC4_UIntNormal,
			                                                                     "BC4_UNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC4_IntNormal,
			                                                                     "BC4_SNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC5_UIntNormal,
			                                                                     "BC5_UNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC5_IntNormal,
			                                                                     "BC5_SNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC6H_SF16,
			                                                                     "BC6H_SF16"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC6H_UF16,
			                                                                     "BC6H_UF16"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC7_UIntNormal,
			                                                                     "BC7_UNORM"
		                                                                     },
		                                                                     {
			                                                                     BufferFormat.BC7_UIntNormal_sRGB,
			                                                                     "BC7_UNORM_SRGB"
		                                                                     }
	                                                                     };

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
        private int _depthSlice;												// Current depth/array index.
        private GorgonConstantBuffer _texParamBuffer;							// Parameters for textures.
	    private readonly byte[] _copyBuffer = new byte[80000];					// 80k copy buffer.
        private BufferFormat _blockCompression = BufferFormat.Unknown;          // Block compression type.
	    private GorgonImageCodec _codec;										// The codec used.
	    private GorgonImageCodec _originalCodec;								// The original codec.
	    private BufferFormat _originalBlockCompression = BufferFormat.Unknown;	// Original block compression state.
	    private int _mipLevel;													// The current mip-map level to view.
	    private int _arrayIndex;												// The current array index.
        private GorgonPixelShader _1DShader;                                    // 1D pixel shader.
        private GorgonPixelShader _1DArrayShader;                               // 1D array pixel shader.
        private GorgonPixelShader _2DShader;                                    // 2D pixel shader.
        private GorgonPixelShader _2DArrayShader;                               // 2D array pixel shader.
        private GorgonPixelShader _3DShader;                                    // 3D pixel shader.
	    private GorgonImageEditorPlugIn _plugIn;								// Image editor plug-in.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the current buffer for the selected mip, array index, or depth slice.
		/// </summary>
		[Browsable(false)]
	    public GorgonImageBuffer Buffer
	    {
			get
			{
				if (Image == null)
				{
					return null;
				}

				return ImageType == ImageType.Image3D ? Image.Buffers[_mipLevel, _depthSlice] : Image.Buffers[_mipLevel, _arrayIndex];
			}
	    }

		/// <summary>
		/// Property to return the number of depth levels for the current mipmap level.
		/// </summary>
		[Browsable(false)]
		public int DepthMipCount
		{
			get
			{
				return Image == null ? 1 : Image.GetDepthCount(_mipLevel);
			}
		}

		/// <summary>
		/// Property to return the array index for the image.
		/// </summary>
		[Browsable(false)]
	    public int ArrayIndex
	    {
			get
			{
				return _arrayIndex;
			}
			set
			{
				if (Image == null)
				{
					return;
				}

				if (value < 0)
				{
					value = 0;
				}

				if (value >= ArrayCount)
				{
					value = ArrayCount - 1;
				}

				_arrayIndex = value;

				SetTextureParams();
			}
	    }

		/// <summary>
		/// Property to set or return the mip map level.
		/// </summary>
		[Browsable(false)]
	    public int MipLevel
	    {
			get
			{
				return _mipLevel;
			}
			set
			{
				if (Image == null)
				{
					return;
				}

				if (value < 0)
				{
					value = 0;
				}

				if (value >= MipCount)
				{
					value = MipCount - 1;
				}

				_mipLevel = value;

				if (ImageType != ImageType.Image3D)
				{
					SetTextureParams();
					return;
				}

				// Update the depth slice based on the number of current mip levels.
				DepthSlice = DepthSlice;
			}
	    }

		/// <summary>
		/// Property to return the path to the executable that can open this file type.
		/// </summary>
		[Browsable(false)]
	    public string ExePath
	    {
		    get;
		    private set;
	    }

		/// <summary>
		/// Property to return the friendly name of the executable file that open this file type.
		/// </summary>
		[Browsable(false)]
	    public string ExeName
	    {
		    get;
		    private set;
	    }

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
        public int DepthSlice
        {
            get
            {
                return _depthSlice;
            }
            set
            {
                if ((ImageType != ImageType.Image3D) 
					|| (Image == null))
                {
                    return;
                }

	            _depthSlice = value;

                if (_depthSlice < 0)
                {
                    _depthSlice = 0;
                }

                if (_depthSlice >= DepthMipCount)
                {
                    _depthSlice = DepthMipCount - 1;
                }

	            SetTextureParams();
            }
        }

        /// <summary>
        /// Property to return the current texture drawing pixel shader.
        /// </summary>
		[Browsable(false)]
        public GorgonPixelShader PixelShader
        {
            get
            {
                switch (ImageType)
                {
                    case ImageType.Image1D:
                        return ArrayCount > 1 ? _1DArrayShader : _1DShader;
                    case ImageType.Image2D:
					case ImageType.ImageCube:
                        return ArrayCount > 1 ? _2DArrayShader : _2DShader;
                    case ImageType.Image3D:
                        return _3DShader;
                    default:
                        return null;
                }
            }
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
		TypeConverter(typeof(BufferFormatTypeConverter)),
		DefaultValue(typeof(BufferFormat), "Unknown"),
		RefreshProperties(RefreshProperties.All)]
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
        TypeConverter(typeof(CompressionTypeConverter)),
		DefaultValue(typeof(BufferFormat), "Unknown"),
		RefreshProperties(RefreshProperties.All)]
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
		DefaultValue(1),
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
		DefaultValue(1),
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
		DefaultValue(1),
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
		LocalDisplayName(typeof(Resources), "PROP_MIPCOUNT_NAME"),
		DefaultValue(1),
		RefreshProperties(RefreshProperties.All)]
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

				// Don't go above the maximum size for this image.
	            int maxMips = GorgonImageData.GetMaxMipCount(Width, Height, Depth);
	            if (value > maxMips)
	            {
		            value = maxMips;
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
		LocalDisplayName(typeof(Resources), "PROP_ARRAYCOUNT_NAME"),
		DefaultValue(1),
		RefreshProperties(RefreshProperties.All)]
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
		DefaultValue(typeof(ImageType), "Image2D"),
		RefreshProperties(RefreshProperties.All),
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

		        Cursor.Current = Cursors.WaitCursor;
		        try
		        {
			        ValidateImageProperties();

					GetExecutable();

					NotifyPropertyChanged();
		        }
		        finally
		        {
			        Cursor.Current = Cursors.Default;
		        }
	        }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to set the texture parameters for the current shader.
		/// </summary>
	    private void SetTextureParams()
		{
			var texParams = new TextureParams
			                {
				                ArrayIndex = ImageType == ImageType.Image3D ? 0 : _arrayIndex,
				                MipLevel = MipCount > 1 ? _mipLevel : 0,
				                DepthSlice = ImageType == ImageType.Image3D ? (_depthSlice / (float)(DepthMipCount - 1).Max(1)) : 0
			                };

			_texParamBuffer.Update(ref texParams);
		}

		/// <summary>
		/// Function to perform the actual processing of the image.
		/// </summary>
		/// <param name="newSettings">Settings to apply.</param>
	    private void ProcessTransform(IImageSettings newSettings)
		{
			BufferFormat convertFormat = newSettings.Format;
			int convertWidth = newSettings.Width;
			int convertHeight = newSettings.ImageType == ImageType.Image1D ? 1 : newSettings.Height;
			newSettings.Format = Image.Settings.Format;
			newSettings.Width = Image.Settings.Width;

			if ((Image.Settings.ImageType != ImageType.Image1D)
				&& (newSettings.ImageType != ImageType.Image1D))
			{
				newSettings.Height = Image.Settings.Height;
			}

			GorgonImageData newImage = null;

			try
			{
				newImage = new GorgonImageData(newSettings);
				Image.CopyTo(newImage);

				// Convert the format.
				if (convertFormat != newSettings.Format)
				{
					newImage.ConvertFormat(convertFormat);
				}

				// Resize the image.
				if ((convertWidth != newSettings.Width)
				    || (convertHeight != newSettings.Height))
				{
					newImage.Resize(convertWidth, convertHeight, false, GorgonImageEditorPlugIn.Settings.ScaleFilter);
				}

				if (Image != _original)
				{
					Image.Dispose();
				}

				Image = newImage;
				_imageSettings = newImage.Settings.Clone();

				
				if (ImageType != ImageType.Image3D)
				{
					// Reset the array index to ensure it gets clipped to any new range.
					ArrayIndex = ArrayIndex;
				}

				// Reset the mip level to same value to ensure it gets clipped to any new range (this also cascades to depth slice as well).
				MipLevel = MipLevel;
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
				throw new ArgumentException(string.Format(Resources.GORIMG_ERR_TEXTURE_FORMAT_CANNOT_MIP, format), "format");
			}

			if ((type != ImageType.Image3D) && ((arrayCount < 1)
				|| (arrayCount > Graphics.Textures.MaxArrayCount)))
			{
				throw new ArgumentOutOfRangeException("arrayCount", string.Format(Resources.GORIMG_ERR_ARRAY_COUNT_INVALID, Graphics.Textures.MaxArrayCount));
			}

			int calcMaxMip = GorgonImageData.GetMaxMipCount(Width, Height, Depth);

			if ((mipCount < 1)
				|| (mipCount > calcMaxMip))
			{
				throw new ArgumentOutOfRangeException("mipCount", string.Format(Resources.GORIMG_ERR_MIP_COUNT_INVALID, calcMaxMip));
			}


			// Ensure that we can resize.
			if (width < 1)
			{
				throw new ArgumentOutOfRangeException("width", Resources.GORIMG_ERR_IMAGE_SIZE_TOO_SMALL);
			}

			switch (type)
			{
				case ImageType.Image1D:
                    if (!Graphics.VideoDevice.Supports1DTextureFormat(format))
                    {
                        throw new ArgumentException(string.Format(Resources.GORIMG_ERR_INVALID_FORMAT, format, "1D"), "format");    
                    }

					if (width > Graphics.Textures.MaxWidth)
					{
						throw new ArgumentOutOfRangeException("width", string.Format(Resources.GORIMG_ERR_IMAGE_1D_SIZE_TOO_LARGE, Graphics.Textures.MaxWidth));
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
                        throw new ArgumentException(string.Format(Resources.GORIMG_ERR_INVALID_FORMAT, ImageFormat, "2D"), "format");
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
							throw new ArgumentOutOfRangeException("arrayCount", string.Format(Resources.GORIMG_ERR_ARRAY_COUNT_INVALID, Graphics.Textures.MaxArrayCount));
						}
					}

					// Ensure that we can resize.
					if (width > Graphics.Textures.MaxWidth)
					{
						throw new ArgumentOutOfRangeException("width", string.Format(Resources.GORIMG_ERR_IMAGE_1D_SIZE_TOO_LARGE, Graphics.Textures.MaxWidth));
					}

					if (height < 1)
					{
						throw new ArgumentOutOfRangeException("height", Resources.GORIMG_ERR_IMAGE_SIZE_TOO_SMALL);
					}

					if (height > Graphics.Textures.MaxHeight)
					{
						throw new ArgumentOutOfRangeException("height", string.Format(Resources.GORIMG_ERR_IMAGE_2D_SIZE_TOO_LARGE, Graphics.Textures.MaxHeight));
					}

                    break;
                case ImageType.Image3D:
                    if (!Graphics.VideoDevice.Supports3DTextureFormat(ImageFormat))
                    {
                        throw new ArgumentException(string.Format(Resources.GORIMG_ERR_INVALID_FORMAT, ImageFormat, "3D"), "format");
                    }

		            if ((MipCount > 1)
						&& (!_imageSettings.IsPowerOfTwo))
		            {
			            throw new ArgumentException(Resources.GORIMG_ERR_TEXTURE_MIP_POW_2, "type");
		            }

					// Ensure that we can resize.
					if (width > Graphics.Textures.Max3DWidth)
					{
						throw new ArgumentOutOfRangeException("width", string.Format(Resources.GORIMG_ERR_IMAGE_3D_SIZE_TOO_LARGE, Graphics.Textures.Max3DWidth));
					}

					if (height < 1)
					{
						throw new ArgumentOutOfRangeException("height", Resources.GORIMG_ERR_IMAGE_SIZE_TOO_SMALL);
					}

					if (height > Graphics.Textures.Max3DHeight)
					{
						throw new ArgumentOutOfRangeException("height", string.Format(Resources.GORIMG_ERR_IMAGE_3D_SIZE_TOO_LARGE, Graphics.Textures.Max3DHeight));
					}

					if (depth < 1)
					{
						throw new ArgumentOutOfRangeException("depth", Resources.GORIMG_ERR_IMAGE_SIZE_TOO_SMALL);
					}

					if (depth > Graphics.Textures.MaxDepth)
					{
						throw new ArgumentOutOfRangeException("depth", string.Format(Resources.GORIMG_ERR_IMAGE_3D_SIZE_TOO_LARGE, Graphics.Textures.MaxDepth));
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
					throw new InvalidCastException(string.Format(Resources.GORIMG_ERR_UNKNOWN_IMAGE_TYPE, type));
			}

			ProcessTransform(newSettings);
		}

        /// <summary>
        /// Function to return whether an image format can be block compressed.
        /// </summary>
        /// <returns><b>true</b> if format can be block compressed, <b>false</b> if not.</returns>
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

	        bool blockCompressionDisabled = !Codec.SupportsBlockCompression || ((Width % 4) != 0) || ((Height % 4) != 0);

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
	                         || _imageSettings.ImageType == ImageType.Image3D
	                         || (_imageSettings.ImageType == ImageType.ImageCube && Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)));

            DisableProperty("MipCount", !Codec.SupportsMipMaps);

            DisableProperty("ImageFormat", Codec.SupportedFormats.Count() < 2);

            DisableProperty("Height", _imageSettings.ImageType == ImageType.Image1D);

            DisableProperty("Depth",
                            !Codec.SupportsDepth
							|| _imageSettings.ImageType == ImageType.Image1D
                            || _imageSettings.ImageType == ImageType.Image2D
                            || _imageSettings.ImageType == ImageType.ImageCube);

            DisableProperty("ImageType", !Codec.SupportsDepth && !Codec.SupportsArray);

	        DisableProperty("Codec", _plugIn.CodecDropDownList.Count(CodecSupportsImage) <= 1);
        }

		/// <summary>
		/// Functon to compress the current image into a block compressed image.
		/// </summary>
		/// <param name="stream">Stream that will receive the compressed data.</param>
	    private void CompressBCImage(Stream stream)
	    {
			Process texConvProcess = null;
			GorgonImageData tempImage = null;

			if ((_contentPanel == null)
				|| (_contentPanel.ParentForm == null))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORIMG_ERR_CANNOT_COMPRESS);
			}

			string texConvPath = GorgonApplication.ApplicationDirectory + "texconv.exe";

			// If we can't find our converter, then we're out of luck.
			if (!File.Exists(texConvPath))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORIMG_ERR_CANNOT_COMPRESS);
			}

			// We'll need to copy this data and decompress it in an external source.
			string tempFilePath = Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) + Path.ChangeExtension(Path.GetRandomFileName(), ".dds");
			string tempFileDirectory = Path.GetDirectoryName(tempFilePath).FormatDirectory(Path.DirectorySeparatorChar);
			string outputName = tempFileDirectory + "comp_" + Path.GetFileName(tempFilePath);

			Debug.Assert(!string.IsNullOrWhiteSpace(tempFileDirectory), "No temporary directory!");

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				var ddsCodec = new GorgonCodecDDS();
				
				// Save our image to the temporary area.
				Image.Save(tempFilePath, ddsCodec);

				// If we can't find our converter, then we're out of luck.
				if (!File.Exists(texConvPath))
				{
					DeleteTempImageFile(tempFilePath);
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORIMG_ERR_CANNOT_COMPRESS);
				}

				var info = new ProcessStartInfo
				{
					Arguments =
						"-f " + _compFormats[BlockCompression] + " -m " + MipCount + " -fl " + 
						(Graphics.VideoDevice.SupportedFeatureLevel > DeviceFeatureLevel.SM4_1 ? "11.0" : "10.0") + " -ft DDS -o \"" +
						Path.GetDirectoryName(tempFilePath) + "\" -nologo -px comp_ \"" + tempFilePath + "\"",
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
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORIMG_ERR_CANNOT_COMPRESS);
				}

				// Wait until we're done converting.
				texConvProcess.WaitForExit();

				// Check standard the error stream for errors.
				string errorData = texConvProcess.StandardError.ReadToEnd();

				if (!string.IsNullOrWhiteSpace(errorData))
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORIMG_ERR_CANNOT_COMPRESS + "\n\n" + errorData);
				}

				errorData = texConvProcess.StandardOutput.ReadToEnd();

				// Check for invalid parameters.
				if (errorData.StartsWith("Invalid value", StringComparison.OrdinalIgnoreCase))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          Resources.GORIMG_ERR_CANNOT_COMPRESS + "\n\n" + errorData.Substring(0, errorData.IndexOf("\n", StringComparison.OrdinalIgnoreCase)));
				}

				// Check for failure.
				if (errorData.Contains("FAILED"))
				{
					// Get rid of the temporary path and use the file name.
					errorData = errorData.Replace(tempFilePath, "\"" + Name + "\"");
					throw new GorgonException(GorgonResult.CannotCreate,
											  Resources.GORIMG_ERR_CANNOT_COMPRESS + "\n\n" + errorData);
				}
				
				texConvProcess.Close();
				texConvProcess = null;

				// Copy the data into our image stream.
				tempImage = GorgonImageData.FromFile(outputName, ddsCodec);
				tempImage.Save(stream, Codec);
			}
			finally
			{
				if (tempImage != null)
				{
					tempImage.Dispose();
				}

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
		/// Function to decompress a block compressed image into an uncompressed texture format.
		/// </summary>
		/// <param name="stream">Stream containing the image format.</param>
		/// <param name="size">Size of the image data within the stream, in bytes.</param>
		/// <param name="codec">The codec to use for reading the file.</param>
		/// <param name="compressionFormat">The compression format.</param>
		/// <param name="mipLevels">Number of desired mip map levels.</param>
		/// <returns>The image data for the decompressed image.</returns>
	    private GorgonImageData DecompressBCImage(Stream stream, int size, GorgonImageCodec codec, BufferFormat compressionFormat, int mipLevels)
		{
			Process texConvProcess = null;

			if ((_contentPanel == null)
				|| (_contentPanel.ParentForm == null))
			{
				return null;
			}

			string texConvPath = GorgonApplication.ApplicationDirectory + "texconv.exe";

			// If we can't find our converter, then we're out of luck.
			if (!File.Exists(texConvPath))
			{
				return null;
			}

			// We'll need to copy this data and decompress it in an external source.
			string tempFilePath = Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) + Path.ChangeExtension(Path.GetRandomFileName(), ".dds");
			string tempFileDirectory = Path.GetDirectoryName(tempFilePath).FormatDirectory(Path.DirectorySeparatorChar);
			string outputName = tempFileDirectory + "decomp_" + Path.GetFileName(tempFilePath);

			Debug.Assert(!string.IsNullOrWhiteSpace(tempFileDirectory), "No temporary directory!");

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// Copy our file.
				using (Stream outStream = File.Open(tempFilePath, FileMode.Create, FileAccess.Write))
				{
					if (!(codec is GorgonCodecDDS))
					{
						// If the image is not already a DDS image, then convert it to one so the texture conversion utility can 
						// use it.
						using (GorgonImageData tempImage = GorgonImageData.FromStream(stream, size, codec))
						{
							tempImage.Save(tempFilePath, new GorgonCodecDDS());
						}
					}
					else
					{
						// Else just copy it out as-is and save us some memory.
						int remaining = size;

						while (remaining > 0)
						{
							int readLength = remaining > _copyBuffer.Length ? _copyBuffer.Length : remaining;

							int readAmount = stream.Read(_copyBuffer, 0, readLength);
							outStream.Write(_copyBuffer, 0, readAmount);

							remaining -= readAmount;
						}
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
						           "-f " + _d3dFormats[compressionFormat] + " -m " + mipLevels + " -fl " +
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
					GorgonDialogs.ErrorBox(_contentPanel.ParentForm, Resources.GORIMG_ERR_DECOMPRESSING, string.Empty, errorData, true);
					return null;
				}

				errorData = texConvProcess.StandardOutput.ReadToEnd();

				// Check for invalid parameters.
				if (errorData.StartsWith("Invalid value", StringComparison.OrdinalIgnoreCase))
				{
					GorgonDialogs.ErrorBox(_contentPanel.ParentForm,
					                       Resources.GORIMG_ERR_DECOMPRESSING,
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
				errorData = errorData.Replace(tempFilePath, "\"" + Name + "\"");
				GorgonDialogs.ErrorBox(_contentPanel.ParentForm,
				                       Resources.GORIMG_ERR_DECOMPRESSING,
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
        /// Function called when the content is reverted back to its original state.
        /// </summary>
        /// <returns>
        /// <b>true</b> if reverted, <b>false</b> if not.
        /// </returns>
        protected override bool OnRevert()
        {
            if (_original == Image)
            {
                return false;
            }

            Image.Dispose();
            Image = _original;
            _imageSettings = _original.Settings.Clone();
            FormatInformation = GorgonBufferFormatInfo.GetInfo(_imageSettings.Format);
            _codec = _originalCodec;
            _blockCompression = _originalBlockCompression;
            ValidateImageProperties();

            return true;
        }

        /// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
	    protected override void Dispose(bool disposing)
	    {
		    if (!_disposed)
		    {
				GorgonImageEditorPlugIn.Settings.Save();

		        if (_1DArrayShader != null)
		        {
		            _1DArrayShader.Dispose();
		        }

		        if (_1DShader != null)
		        {
		            _1DShader.Dispose();
		        }

		        if (_2DArrayShader != null)
		        {
		            _2DArrayShader.Dispose();
		        }

		        if (_2DShader != null)
		        {
		            _2DShader.Dispose();
		        }

		        if (_3DShader != null)
		        {
		            _3DShader.Dispose();
		        }

		        if (_texParamBuffer != null)
		        {
		            _texParamBuffer.Dispose();
		        }

				if ((Image != null) && (Image != _original))
				{
					Image.Dispose();
				}

			    EditorFile = null;

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

		        _1DShader = _1DArrayShader = null;
		        _2DShader = _2DArrayShader = null;
		        _3DShader = null;
		        _texParamBuffer = null;
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
		    if (BlockCompression == BufferFormat.Unknown)
		    {
			    // Write the image out.
			    Image.Save(stream, Codec);
			}
		    else
		    {
			    CompressBCImage(stream);
		    }

			// Change the codec type.
			EditorFile.Attributes["Codec"] = Codec.GetType().FullName;

			_originalCodec = Codec;

			// Make this image the original.
		    if (_original == Image)
		    {
			    return;
		    }
	        
		    _original.Dispose();
		    _original = Image;
        }
		
		/// <summary>
		/// Function to retrieve the proper codec to use for the image.
		/// </summary>
		/// <param name="file">The editor file to evaluate.</param>
		/// <param name="stream">The stream to read.</param>
		/// <returns>The codec for the stream.</returns>
	    private GorgonImageCodec GetCodecForStream(EditorFile file, Stream stream)
		{
			GorgonImageCodec codec;

			// Find the codec in the editor file attributes.
			string codecTypeName;

			if (file.Attributes.TryGetValue("Codec", out codecTypeName))
			{
				// Find a codec with the appropriate type name.
				codec = _plugIn.CodecDropDownList.FirstOrDefault(item => string.Equals(item.GetType().FullName, codecTypeName, StringComparison.OrdinalIgnoreCase));

				if ((codec != null) && (codec.IsReadable(stream)))
				{
					return (GorgonImageCodec)Activator.CreateInstance(codec.GetType());
				}
			}

			// We didn't have codec information in the meta data, fall back to the file extension.
			string fileExtension = Path.GetExtension(file.FilePath);

			if ((!_plugIn.Codecs.TryGetValue(new GorgonFileExtension(fileExtension), out codec))
				|| (!codec.IsReadable(stream)))
			{
				return null;
			}

			return (GorgonImageCodec)Activator.CreateInstance(codec.GetType());
		}

		/// <summary>
		/// Function to find path to the executable that can open this file.
		/// </summary>
	    private void GetExecutable()
		{
			string tempFilePath = null;

			try
			{
				ExePath = string.Empty;
				ExeName = string.Empty;

				// Save a temporary copy of the file.
				tempFilePath = SaveToTemporaryFile();

				if (!Win32API.HasAssociation(tempFilePath))
				{
					return;
				}

				ExePath = Win32API.GetAssociatedExecutable(tempFilePath);

				if (string.IsNullOrWhiteSpace(ExePath))
				{
					return;
				}

				ExeName = Win32API.GetFriendlyExeName(ExePath);
			}
			finally
			{
				DeleteTempImageFile(tempFilePath);
			}
		}

	    /// <summary>
        /// Function to read the content data from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the content data.</param>
        protected override void OnRead(Stream stream)
	    {
			// Get the appropriate image codec.
		    _originalCodec = _codec = GetCodecForStream(EditorFile, stream);

		    if (_codec == null)
		    {
			    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_CODEC_NONE_FOUND, EditorFile.Filename));
		    }

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

			// If we've previously loaded an image, and it's not the same as the original image,
			// then get rid of it.
			if ((Image != null)
				&& (_original != null)
				&& (Image != _original))
			{
				Image.Dispose();
				Image = null;
			}

		    IImageSettings settings = Codec.GetMetaData(stream);
			FormatInformation = GorgonBufferFormatInfo.GetInfo(settings.Format);

		    long position = stream.Position;

		    if (FormatInformation.IsCompressed)
		    {
				_originalBlockCompression = _blockCompression = settings.Format;
			    Image = DecompressBCImage(stream, (int)stream.Length, Codec, settings.Format, settings.MipCount);
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

			// Find the executable associated with the file.
			GetExecutable();
	
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
	        CanChangeBCImages = File.Exists(GorgonApplication.ApplicationDirectory + "texconv.exe");

	        _contentPanel = new GorgonImageContentPanel(this, GetRawInput());

	        _swap = Graphics.Output.CreateSwapChain("TextureDisplay",
	                                                new GorgonSwapChainSettings
	                                                {
														Window = _contentPanel.panelTextureDisplay,
														Format = BufferFormat.R8G8B8A8_UIntNormal
	                                                });

			Renderer = Graphics.Output.Create2DRenderer(_swap);

          
            GorgonShaderMacro[] macros = null;

            if (Graphics.VideoDevice.SupportedFeatureLevel >= DeviceFeatureLevel.SM4_1)
            {
                macros = new[]
                         {
                             new GorgonShaderMacro("SM41")
                         };
            }
            
            _1DShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("1D Texture",
                                                                      "Gorgon1DTextureView",
                                                                      Resources.ImageViewShaders,
                                                                      macros);

            _1DArrayShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("1D Texture array",
                                                                      "Gorgon1DTextureArrayView",
                                                                      Resources.ImageViewShaders,
                                                                      macros);

            _2DShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("2D Texture",
                                                                      "Gorgon2DTextureView",
                                                                      Resources.ImageViewShaders,
                                                                      macros);

            _2DArrayShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("2D Texture array",
                                                                      "Gorgon2DTextureArrayView",
                                                                      Resources.ImageViewShaders,
                                                                      macros);


            _3DShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("3D Texture",
                                                                      "Gorgon3DTextureView",
                                                                      Resources.ImageViewShaders,
                                                                      macros);

            // Create our depth/array index value for the shaders.
            _texParamBuffer = Graphics.Buffers.CreateConstantBuffer("DepthArrayIndex",
                                                                         new GorgonConstantBufferSettings
                                                                         {
                                                                             SizeInBytes = TextureParams.Size

                                                                         });
	        
			var texParams = new TextureParams();
            _texParamBuffer.Update(ref texParams);

            Graphics.Shaders.PixelShader.ConstantBuffers[1] = _texParamBuffer;

	        _contentPanel.CreateResources();

            return _contentPanel;
        }

		/// <summary>
		/// Function to delete a temporary image file.
		/// </summary>
		/// <param name="path">The path to the temporary image file.</param>
		public static void DeleteTempImageFile(string path)
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
		/// Function to save the image to a temporary file.
		/// </summary>
		/// <returns>The path to the temporary file, or an empty string if the file was not created.</returns>
		public string SaveToTemporaryFile()
		{
			if ((Codec == null)
			    || (!Codec.CodecCommonExtensions.Any()))
			{
				return string.Empty;
			}

			string tempFilePath = Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) +
								  Path.ChangeExtension(Path.GetRandomFileName(), Codec.CodecCommonExtensions.FirstOrDefault());

			// Save the image to a temporary location.
			Image.Save(tempFilePath, Codec);

			return tempFilePath;
		}


		/// <summary>
		/// Function to determine if the codec supports the current image information.
		/// </summary>
		/// <param name="codec">The codec to evaluate.</param>
		/// <returns><b>true</b> if the codec supports the current image information, <b>false</b> if not.</returns>
	    public bool CodecSupportsImage(GorgonImageCodec codec)
	    {
			bool hasArray = (ArrayCount > 1 && codec.SupportsArray) || ArrayCount == 1;
			bool hasDepth = (Depth > 1 && codec.SupportsDepth) || Depth == 1;
			bool hasMips = (MipCount > 1 && codec.SupportsMipMaps) || MipCount == 1;
		    bool hasImageType;
			bool hasFormat = codec.SupportedFormats.Any(format => format == ImageFormat);
			bool hasBC = (codec.SupportsBlockCompression && BlockCompression != BufferFormat.Unknown) ||
						 BlockCompression == BufferFormat.Unknown;

		    switch (ImageType)
		    {
                case ImageType.Image1D:
                case ImageType.Image2D:
		            hasImageType = true;
		            break;
		        case ImageType.Image3D:
		            hasImageType = Codec.SupportsDepth;
		            break;
                case ImageType.ImageCube:
		            hasImageType = Codec.SupportsArray;
		            break;
                default:
		            hasImageType = false;
		            break;
		    }

			return hasBC && hasArray & hasDepth && hasMips && hasImageType && hasFormat;
	    }

		/// <summary>
		/// Function to set the array index for the image.
		/// </summary>
		/// <param name="arrayIndex">Current array index.</param>
	    public void SetCubeArrayIndex(int arrayIndex)
	    {
			var texParams = new TextureParams
			{
				ArrayIndex = arrayIndex,
				MipLevel = MipCount > 1 ? _mipLevel : 0,
				DepthSlice = 0
			};

			_texParamBuffer.Update(ref texParams);
		}

		/// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
	    public override void Draw()
		{
			Renderer.Target = _swap;

			if (ImageType != ImageType.ImageCube)
			{
				_contentPanel.Draw();
			}
			else
			{
				_contentPanel.DrawUnwrappedCube();
			}

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

		/// <summary>
		/// Function to determine if the currently selected mip/array/depth buffer has data in it.
		/// </summary>
		/// <returns><b>true</b> if the buffer has data, <b>false</b> if not.</returns>
	    public unsafe bool ImageBufferHasData()
		{
			var bufferPtr = (byte*)Buffer.Data.UnsafePointer;

			for (int y = 0; y < Buffer.Height; y++)
			{
				byte* data = bufferPtr + (y * Buffer.PitchInformation.RowPitch);

				for (int x = 0; x < Buffer.PitchInformation.RowPitch; x++)
				{
					if ((*(data++)) != 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Function to convert the image to a format that's compatible with the image buffer currently selected.
		/// </summary>
		/// <param name="image">Image to convert.</param>
		/// <param name="crop"><b>true</b> to crop the image instead of resize, <b>false</b> to resize.</param>
		/// <param name="filter">The filter to apply when resizing.</param>
		/// <param name="preserveAspect"><b>true</b> to preserve the aspect ratio of the source image, <b>false</b> to ignore it.</param>
		/// <param name="imageAlign">Image alignment.</param>
		public void ConvertImageToBuffer(GorgonImageData image, bool crop, ImageFilter filter, bool preserveAspect, ContentAlignment imageAlign)
		{
			try
			{
				// If we're using the original image, then create a new image to work on.
				if (Image == _original)
				{
					Image = new GorgonImageData(_original.Settings);
					_original.CopyTo(Image);
					_imageSettings = Image.Settings.Clone();
				}

				// If the formats are mismatched, then change it.
				if (image.Settings.Format != ImageFormat)
				{
					image.ConvertFormat(ImageFormat);
				}


				var offset = Point.Empty;

				// If the buffer is not the same size as the image we've loaded, then resize the image.
				if (((Buffer.Width != image.Settings.Width)
				    || (Buffer.Height != image.Settings.Height))
					&& (!crop))
				{
				    var newSize = new Vector2(Buffer.Width, Buffer.Height);

				    if (preserveAspect)
				    {
				        if (image.Settings.Width > image.Settings.Height)
				        {
				            newSize.Y *= (float)image.Settings.Height / image.Settings.Width;
				        }
                        else
				        {
							newSize.X *= (float)image.Settings.Width / image.Settings.Height;
                        }
				    }

					image.Resize((int)newSize.X, (int)newSize.Y, false, filter);
				}

				switch (imageAlign)
				{
					case ContentAlignment.TopCenter:
						offset.X = (int)(Buffer.Width / 2.0f - image.Buffers[0].Width / 2.0f);
						break;
					case ContentAlignment.TopRight:
						offset.X = Buffer.Width - image.Buffers[0].Width;
						break;
					case ContentAlignment.MiddleLeft:
						offset.Y = (int)(Buffer.Height / 2.0f - image.Buffers[0].Height / 2.0f);
						break;
					case ContentAlignment.MiddleRight:
						offset.X = Buffer.Width - image.Buffers[0].Width;
						offset.Y = (int)(Buffer.Height / 2.0f - image.Buffers[0].Height / 2.0f);
						break;
					case ContentAlignment.BottomLeft:
						offset.Y = Buffer.Height - image.Buffers[0].Height;
						break;
					case ContentAlignment.BottomCenter:
						offset.X = (int)(Buffer.Width / 2.0f - image.Buffers[0].Width / 2.0f);
						offset.Y = Buffer.Height - image.Buffers[0].Height;
						break;
					case ContentAlignment.BottomRight:
						offset.X = Buffer.Width - image.Buffers[0].Width;
						offset.Y = Buffer.Height - image.Buffers[0].Height;
						break;
					case ContentAlignment.MiddleCenter:
						offset.X = (int)(Buffer.Width / 2.0f - image.Buffers[0].Width / 2.0f);
						offset.Y = (int)(Buffer.Height / 2.0f - image.Buffers[0].Height / 2.0f);
						break;
				}

				unsafe
				{
					// Clear the buffer so we don't get garbage left over.
					DirectAccess.ZeroMemory(Buffer.Data.UnsafePointer, (int)Buffer.Data.Length);
				}

				// Copy the data into the buffer.
				image.Buffers[0].CopyTo(Buffer, null, offset.X, offset.Y);
				
				NotifyPropertyChanged("ImageImport", null);
			}
			finally
			{
				ValidateImageProperties();
			}
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="editorFile">Editor file linked to the image.</param>
		/// <param name="stream">Stream containing the image data.</param>
		/// <param name="topLevelOnly"><b>true</b> to retrieve only the first mip map level and/or array index.</param>
		/// <returns>
		/// An image data object containing the image.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="editorFile"/>, or the <paramref name="stream"/> parameters are NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="GorgonException">Thrown when the no codec could be found for the image data.</exception>
	    public GorgonImageData ReadFrom(EditorFile editorFile, Stream stream, bool topLevelOnly)
	    {
			if (editorFile == null)
			{
				throw new ArgumentNullException("editorFile");
			}

			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

		    GorgonImageCodec codec = GetCodecForStream(editorFile, stream);

			if (codec == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_CODEC_NONE_FOUND, editorFile.FilePath));
			}

			// ReSharper disable once InvertIf
			if (topLevelOnly)
			{
				codec.MipCount = 1;
				codec.ArrayCount = 1;
			}

			// If the image is a compressed image, then decompress it so we can manipulate it.
			IImageSettings metaData = codec.GetMetaData(stream);
			var formatInfo = GorgonBufferFormatInfo.GetInfo(metaData.Format);

			return formatInfo.IsCompressed
					   ? DecompressBCImage(stream, (int)stream.Length, codec, metaData.Format, metaData.MipCount)
					   : GorgonImageData.FromStream(stream, (int)stream.Length, codec);
	    }

		/// <summary>
		/// Function to edit the file in an associated external application.
		/// </summary>
	    public void EditExternal()
		{
			Process exeProcess = null;
			FileSystemWatcher watcher = null;
			string filePath = string.Empty;
			GorgonImageData newImage = null;
			var watchValues = new FileWatchValues();

			try
			{
				filePath = SaveToTemporaryFile();

				var info = new ProcessStartInfo
				           {
					           FileName = ExePath,
					           ErrorDialogParentHandle = _contentPanel.Handle,
					           ErrorDialog = true,
					           UseShellExecute = false,
					           WorkingDirectory = Path.GetDirectoryName(filePath).FormatDirectory(Path.DirectorySeparatorChar),
					           Arguments = "\"" + filePath + "\"",
					           CreateNoWindow = false
				           };

				if ((_contentPanel != null)
				    && (_contentPanel.ParentForm != null))
				{
					_contentPanel.ParentForm.Visible = false;
				}

				watchValues.Filename = Path.GetFileName(filePath);
				watchValues.IsChanged = false;

				// Monitor this file for changes.
				watcher = new FileSystemWatcher
				          {
					          NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Size,
					          Path = Path.GetDirectoryName(filePath).FormatDirectory(Path.DirectorySeparatorChar),
					          InternalBufferSize = 4 * 1024 * 1024,
					          Filter = "*" + Path.GetExtension(filePath),
					          IncludeSubdirectories = false,
					          EnableRaisingEvents = true
				          };

				
				watcher.Changed += (sender, args) =>
				                   {
					                   // If our file did not change, then leave.
					                   if (!string.Equals(args.Name, watchValues.Filename, StringComparison.OrdinalIgnoreCase))
					                   {
						                   return;
					                   }

					                   watchValues.IsChanged = true;
				                   };

				exeProcess = Process.Start(info);

				if (exeProcess == null)
				{
					throw new GorgonException(GorgonResult.AccessDenied);
				}

				exeProcess.WaitForExit();

				// If the file didn't change, then get out.
				if (!watchValues.IsChanged)
				{
					return;
				}
				
				// Reload the image data.
				using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					IImageSettings newSettings = Codec.GetMetaData(stream);
					FormatInformation = GorgonBufferFormatInfo.GetInfo(newSettings.Format);

					// If the file is compressed, then decompress it.
					if ((FormatInformation.IsCompressed)
					    && (Codec.SupportsBlockCompression))
					{
						_blockCompression = newSettings.Format;
						newImage = DecompressBCImage(stream, (int)stream.Length, Codec, _blockCompression, newSettings.MipCount);
					}

					// If we couldn't decompress the file, or it wasn't compressed, then load it as normal.
					if (newImage == null)
					{
						if (FormatInformation.IsCompressed)
						{
							CanChangeBCImages = false;
						}

						newImage = GorgonImageData.FromStream(stream, (int)stream.Length, Codec);
					}
				}

				if (Image != _original)
				{
					Image.Dispose();
				}

				Image = newImage;
				_imageSettings = Image.Settings.Clone();

				NotifyPropertyChanged("ImageEditExternal", null);
				ValidateImageProperties();
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
				if (watcher != null)
				{
					watcher.Dispose();
				}

				if ((_contentPanel != null)
				    && (_contentPanel.ParentForm != null))
				{
					_contentPanel.ParentForm.Visible = true;
				}

				if (exeProcess != null)
				{
					exeProcess.Close();
					exeProcess.Dispose();
				}

				DeleteTempImageFile(filePath);
			}
	    }

		/// <summary>
		/// Function to generate mip map data for this image.
		/// </summary>
	    public void GenerateMipMaps()
	    {
			if (Image == null)
			{
				return;
			}

			Image.GenerateMipMaps(MipCount, GorgonImageEditorPlugIn.Settings.MipFilter);
			NotifyPropertyChanged("MipGenerate", null);
			ValidateImageProperties();
	    }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageContent"/> class.
        /// </summary>
        /// <param name="settings">Settings for the image content.</param>
        /// <param name="plugIn">The plug-in that created this content.</param>
        public GorgonImageContent(GorgonImageEditorPlugIn plugIn, GorgonImageContentSettings settings)
            : base(settings)
        {
			PlugIn = _plugIn = plugIn;

	        EditorFile = settings.EditorFile;
            HasThumbnail = true;

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

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the image to load.</param>
		void IImageEditorContent.Load(Stream stream)
		{
			if ((stream == null)
				|| (stream.Length == 0))
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_CONTENT_NOT_IMAGE, EditorFile.FilePath));
			}

			OnRead(stream);
		}
        #endregion
    }
}
