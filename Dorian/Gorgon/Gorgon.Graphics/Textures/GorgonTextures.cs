#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and the sell
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
// Created: Thursday, February 09, 2012 7:59:01 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using GorgonLibrary.IO;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Textures interface.
	/// </summary>
	public sealed class GorgonTextures
	{
		#region Variables.
		private GorgonGraphics _graphics = null;
		private GorgonTexture2D _logo = null;
		private static int _incrementCount = 0;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the texture for the Gorgon logo.
		/// </summary>
		public GorgonTexture2D GorgonLogo
		{
			get
			{
				// Keep other threads from creating this image multiple times.
			    try
			    {
			        if (Interlocked.Increment(ref _incrementCount) == 1)
			        {
			            if (_logo == null)
			            {
			                _logo = _graphics.Textures.Create2DTextureFromGDIImage("Gorgon.Logo",
			                                                                       Properties.Resources.Gorgon_2_x_Logo_Small);

			                // Don't track this image.
			                _graphics.RemoveTrackedObject(GorgonLogo);
			            }
			        }
			    }
			    finally
			    {
                    Interlocked.Decrement(ref _incrementCount);
			    }

			    return _logo;
			}
		}

		/// <summary>
		/// Property to return the maximum width of a texture.
		/// </summary>
		public int MaxWidth
		{
			get
			{
				switch (_graphics.VideoDevice.SupportedFeatureLevel)
				{
					case DeviceFeatureLevel.SM2_a_b:
						return 4096;
					case DeviceFeatureLevel.SM4:
					case DeviceFeatureLevel.SM4_1:
						return 8192;
					default:
						return 16384;
				}
			}
		}

		/// <summary>
		/// Property to return the maximum height of a texture.
		/// </summary>
		public int MaxHeight
		{
			get
			{
				switch (_graphics.VideoDevice.SupportedFeatureLevel)
				{
					case DeviceFeatureLevel.SM2_a_b:
						return 4096;
					case DeviceFeatureLevel.SM4:
					case DeviceFeatureLevel.SM4_1:
						return 8192;
					default:
						return 16384;
				}
			}
		}

		/// <summary>
		/// Property to return the maximum depth of a texture.
		/// </summary>
		public int MaxDepth
		{
			get
			{
				return _graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b ? 512 : 2048;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve compatible texture settings from existing image settings.
		/// </summary>
		/// <param name="settings">Settings to derive settings from.</param>
		/// <returns>The compatible texture settings.</returns>
		private ITextureSettings GetTextureSettings(IImageSettings settings)
		{
			// If these settings are already texture settings, then just leave.
			if (settings is ITextureSettings)
			{
				return (ITextureSettings)settings;
			}

			switch (settings.ImageType)
			{
				case ImageType.Image1D:
					return new GorgonTexture1DSettings()
					{
						Width = settings.Width,
						ArrayCount = settings.ArrayCount,
						Format = settings.Format,
						MipCount = settings.MipCount,
						Usage = BufferUsage.Default,
						ViewFormat = BufferFormat.Unknown,
						ViewIsUnordered = false
					};
				case ImageType.Image2D:
				case ImageType.ImageCube:
					return new GorgonTexture2DSettings()
					{
						Width = settings.Width,
						Height = settings.Height,
						IsTextureCube = (settings.ImageType == ImageType.ImageCube),						
						ArrayCount = settings.ArrayCount,
						Format = settings.Format,
						MipCount = settings.MipCount,
						Usage = BufferUsage.Default,
						Multisampling = new GorgonMultisampling(1, 0),
						ViewFormat = BufferFormat.Unknown,
						ViewIsUnordered = false
					};
				case ImageType.Image3D:
					return new GorgonTexture3DSettings()
					{
						Width = settings.Width,
						Height = settings.Height,
						Depth = settings.Depth,
						Format = settings.Format,
						MipCount = settings.MipCount,
						Usage = BufferUsage.Default,						
						ViewFormat = BufferFormat.Unknown,
						ViewIsUnordered = false
					};					
				default:
					throw new ArgumentException("These image settings are not compatible with texture settings.", "settings");
			}
		}

		/// <summary>
		/// Function to return the width and height as powers of two.
		/// </summary>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <param name="depth">Depth of the texture.</param>
		/// <returns>The width, height and depth bumped to the nearest power of two.</returns>
		private Tuple<int, int, int> GetPow2Size(int width, int height, int depth)
		{
			// Do width.
			while ((width != 0) && ((width & (width - 1)) != 0))
			{
				width++;
			}

			// Do height.			
			while ((height != 0) && ((height & (height - 1)) != 0))
			{
				height++;
			}

			// Do depth.
			while ((depth != 0) && ((depth & (depth - 1)) != 0))
			{
				depth++;
			}

			return new Tuple<int, int, int>(width, height, depth);
		}

		/// <summary>
		/// Function to validate the 3D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		private void ValidateTexture3D(ref ITextureSettings settings)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_IntNormal;

			if (settings.Width < 0)
				settings.Width = 0;

			if (settings.Height < 0)
				settings.Height = 0;

			// Direct3D 9 video devices require resizing to power of two.
			// Mip maps also require power of 2 sizing for volume textures.
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (settings.MipCount != 1))
			{
				Tuple<int, int, int> newSize = GetPow2Size(settings.Width, settings.Height, settings.Depth);
				settings.Width = newSize.Item1;
				settings.Height = newSize.Item2;
				settings.Depth = newSize.Item3;
			}

			// Check texture size if using a compressed format.
			var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.Format);

			if (formatInfo.IsCompressed)
			{
				if ((settings.Width % 4) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "A compressed texture must have a width that is a multiple of 4.");
				}

				if ((settings.Height % 4) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "A compressed texture must have a height that is a multiple of 4.");
				}
			}

			if (settings.Width > MaxWidth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be less than " + MaxWidth.ToString() + ".");
			if (settings.Height > MaxHeight)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture height must be less than " + MaxHeight.ToString() + ".");
			if (settings.Depth > MaxDepth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture depth must be less than " + MaxDepth.ToString() + ".");

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports3DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture.  The format '" + settings.Format.ToString() + "' is not supported by the hardware.");
		}

		/// <summary>
		/// Function to perform clean up of resources.
		/// </summary>
		internal void CleanUp()
		{
			if (_logo != null)
				_logo.Dispose();
			_logo = null;			
		}

		/// <summary>
		/// Function to validate the 2D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		internal void ValidateTexture2D(ref ITextureSettings settings)
		{		
			
			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.ArrayCount > 2048)
				settings.ArrayCount = 2048;

			if (settings.IsTextureCube)
			{
				if ((settings.ArrayCount != 6) && ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
					throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture cube array.  SM2_a_b and SM4 devices require a maximum of 6 faces.");

				if ((settings.ArrayCount % 6) != 0)
					throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture cube array.  The array count is not a multiple of 6.");
			}

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Multisampling.Count == 0)
				settings.Multisampling = new GorgonMultisampling(1, 0);

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_IntNormal;

			if (settings.Width < 0)
				settings.Width = 0;

			if (settings.Height < 0)
				settings.Height = 0;
			
			// Check texture size if using a compressed format.
			var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.Format);

			if (formatInfo.IsCompressed)
			{
				if ((settings.Width % 4) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "A compressed texture must have a width that is a multiple of 4.");
				}

				if ((settings.Height % 4) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "A compressed texture must have a height that is a multiple of 4.");
				}
			}

			// Direct3D 9 video devices require resizing to power of two if there is more than 1 mip level.
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (settings.MipCount != 1))
			{
				Tuple<int, int, int> newSize = GetPow2Size(settings.Width, settings.Height, 0);
				settings.Width = newSize.Item1;
				settings.Height = newSize.Item2;
			}

			if (settings.Width > MaxWidth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be less than " + MaxWidth.ToString() + ".");
			if (settings.Height > MaxHeight)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture height must be less than " + MaxHeight.ToString() + ".");

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports2DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture.  The format '" + settings.Format.ToString() + "' is not supported by the hardware.");			
		}

		/// <summary>
		/// Function to validate the 1D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		private void ValidateTexture1D(ref ITextureSettings settings)
		{
			if (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				throw new GorgonException(GorgonResult.CannotCreate, "1 dimensional textures are not supported on SM2_a_b devices.");

			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_IntNormal;

			if (settings.Width < 0)
				settings.Width = 0;

			// Check texture size if using a compressed format.
			var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.Format);

			if (formatInfo.IsCompressed)
			{
				if ((settings.Width % 4) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "A compressed texture must have a width that is a multiple of 4.");
				}
			}

			// Direct3D 9 video devices require resizing to power of two if there is more than 1 mip level.
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (settings.MipCount != 1))
			{
				Tuple<int, int, int> newSize = GetPow2Size(settings.Width, 0, 0);
				settings.Width = newSize.Item1;
			}

			if (settings.Width > MaxWidth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be less than " + MaxWidth.ToString() + ".");

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports1DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture.  The format '" + settings.Format.ToString() + "' is not supported by the hardware.");
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image object.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="image">Image to load.</param>
		/// <returns>The clone of the GDI+ image contained within a new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="image"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when pixel format in the options cannot be converted or is not supported.</para></exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture1D">GorgonTexture1D</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// </remarks>
        public GorgonTexture1D Create1DTextureFromGDIImage(string name, Image image)
		{
			return Create1DTextureFromGDIImage(name, image, new GorgonGDIOptions());
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image object.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="image">Image to load.</param>
		/// <param name="options">Settings for the conversion process.</param>
		/// <returns>The clone of the GDI+ image contained within a new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="image"/> or the <paramref name="options"/> parameters is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when pixel format in the options cannot be converted or is not supported.</para></exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture1D">GorgonTexture1D</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>The <paramref name="options"/> parameter controls how the <paramref name="image" /> is converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <description>Height</description><description>This is ignored for 1D images.</description>
        /// </item>
        /// <item>
        /// <description>Depth</description><description>This is ignored for 1D images.</description>
        /// </item>
        /// <item>
        /// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 0 to generate a full mip-map chain, or set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <description>ArrayCount</description><description>This is ignored for this overload.</description>
        /// </item>
        /// <item>
        /// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public GorgonTexture1D Create1DTextureFromGDIImage(string name, Image image, GorgonGDIOptions options)
		{
			GorgonTexture1D result = null;

			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			using (GorgonImageData data = GorgonImageData.Create1DFromGDIImage(image, options))
			{
				result = CreateTexture<GorgonTexture1D>(name, data);
			}

			return result;
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image object.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="image">Image to load.</param>
		/// <returns>The clone of the GDI+ image contained within a new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="image"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when pixel format in the options cannot be converted or is not supported.</para></exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture2D">GorgonTexture2D</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// </remarks>
        public GorgonTexture2D Create2DTextureFromGDIImage(string name, Image image)
		{
			return Create2DTextureFromGDIImage(name, image, new GorgonGDIOptions());
		}

		/// <summary>
		/// Function to create a texture from a GDI+ image object.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="image">Image to load.</param>
		/// <param name="options">Settings for the conversion process.</param>
		/// <returns>The clone of the GDI+ image contained within a new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="image"/> or the <paramref name="options"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when pixel format in the options cannot be converted or is not supported.</para></exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture2D">GorgonTexture2D</see> object from a <see cref="System.Drawing.Image">GDI+ Image</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>The <paramref name="options"/> parameter controls how the <paramref name="image" /> is converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <description>Height</description><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <description>Depth</description><description>This is ignored for 2D images.</description>
        /// </item>
        /// <item>
        /// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 0 to generate a full mip-map chain, or set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <description>ArrayCount</description><description>This is ignored for this overload.</description>
        /// </item>
        /// <item>
        /// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public GorgonTexture2D Create2DTextureFromGDIImage(string name, Image image, GorgonGDIOptions options)
		{
			GorgonTexture2D result = null;

			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			using (GorgonImageData data = GorgonImageData.Create2DFromGDIImage(image, options))
			{
				result = CreateTexture<GorgonTexture2D>(name, data);
			}

			return result;
		}

		/// <summary>
		/// Function to create a texture array and the mip chain from a list of GDI+ image objects.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="images">Image to load.</param>
		/// <param name="options">Settings for the conversion process.</param>
		/// <returns>The clone of the GDI+ images contained within a new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="images"/> or the <paramref name="options"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
        /// <para>-or-</para>
        /// <para>Thrown when an image in the list is NULL.</para>		
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the array and mip count.</exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture1D">GorgonTexture1D</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>This overload is used to create image arrays and/or mip-map chains from a list of images.  If the MipCount and ArrayCount are set to 1, then
        /// only the first image will be processed.</para>
        /// <para>The layout of the image list is processed in the following order (assuming the ArrayCount = 2, and the MipCount = 4):</para>
        /// <code>
        /// images[0]: Array Index 0, Mip Level 0
        /// images[1]: Array Index 0, Mip Level 1
        /// images[2]: Array Index 0, Mip Level 2
        /// images[3]: Array Index 0, Mip Level 3
        /// images[4]: Array Index 1, Mip Level 0
        /// images[5]: Array Index 1, Mip Level 1
        /// images[6]: Array Index 1, Mip Level 2
        /// images[7]: Array Index 1, Mip Level 3
        /// </code>
        /// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <description>Height</description><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <description>Depth</description><description>This is ignored for 2D images.</description>
        /// </item>
        /// <item>
        /// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <description>ArrayCount</description><description>Gorgon will generate the requested number of image arrays from the source image.  Set to 1 if no image arrays are required.</description>
        /// </item>
        /// <item>
        /// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
        /// </item>
        /// </list>
        /// <para>The list of images must be large enough to accomodate the number of array indices and mip map levels (ArrayCount * MipCount), must not contain any NULL (Nothing in VB.Net) elements and all images must use
        /// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until ArrayCount * MipCount are used.  Unlike other overloads, this method will NOT auto-generate
        /// mip-maps and will only use the images provided.</para>
        /// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
        /// </remarks>
        public GorgonTexture1D Create1DTextureFromGDIImage(string name, IList<Image> images, GorgonGDIOptions options)
		{
			GorgonTexture1D result = null;

			if (images == null)
			{
				throw new ArgumentNullException("images");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			if (images.Count == 0)
			{
				throw new ArgumentException("The parameter must not be empty.", "images");
			}

			if (images.Count == 1)
			{
				return Create1DTextureFromGDIImage(name, images[0], options);
			}
			
			using (GorgonImageData data = GorgonImageData.Create1DFromGDIImage(images, options))
			{
				result = CreateTexture<GorgonTexture1D>(name, data);
			}

			return result;
		}

		/// <summary>
		/// Function to create a texture array and the mip chain from a list of GDI+ image objects.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="images">Image to load.</param>
		/// <param name="options">Settings for the conversion process.</param>
		/// <returns>The clone of the GDI+ images contained within a new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="images"/> or the <paramref name="options"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
        /// <para>-or-</para>
        /// <para>Thrown when an image in the list is NULL.</para>		
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the array and mip count.</exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture2D">GorgonTexture2D</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>This overload is used to create image arrays and/or mip-map chains from a list of images.  If the MipCount and ArrayCount are set to 1, then
        /// only the first image will be processed.</para>
        /// <para>The layout of the image list is processed in the following order (assuming the ArrayCount = 2, and the MipCount = 4):</para>
        /// <code>
        /// images[0]: Array Index 0, Mip Level 0
        /// images[1]: Array Index 0, Mip Level 1
        /// images[2]: Array Index 0, Mip Level 2
        /// images[3]: Array Index 0, Mip Level 3
        /// images[4]: Array Index 1, Mip Level 0
        /// images[5]: Array Index 1, Mip Level 1
        /// images[6]: Array Index 1, Mip Level 2
        /// images[7]: Array Index 1, Mip Level 3
        /// </code>
        /// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <description>Height</description><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <description>Depth</description><description>This is ignored for 2D images.</description>
        /// </item>
        /// <item>
        /// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <description>ArrayCount</description><description>Gorgon will generate the requested number of image arrays from the source image.  Set to 1 if no image arrays are required.</description>
        /// </item>
        /// <item>
        /// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
        /// </item>
        /// </list>
        /// <para>The list of images must be large enough to accomodate the number of array indices and mip map levels (ArrayCount * MipCount), must not contain any NULL (Nothing in VB.Net) elements and all images must use
        /// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until ArrayCount * MipCount are used.  Unlike other overloads, this method will NOT auto-generate
        /// mip-maps and will only use the images provided.</para>
        /// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
        /// </remarks>
        public GorgonTexture2D Create2DTextureFromGDIImage(string name, IList<Image> images, GorgonGDIOptions options)
		{
			GorgonTexture2D result = null;

			if (images == null)
			{
				throw new ArgumentNullException("images");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			if (images.Count == 0)
			{
				throw new ArgumentException("The parameter must not be empty.", "images");
			}

			if (images.Count == 1)
			{
				return Create2DTextureFromGDIImage(name, images[0], options);
			}

			using (GorgonImageData data = GorgonImageData.Create2DFromGDIImage(images, options))
			{
				result = CreateTexture<GorgonTexture2D>(name, data);
			}

			return result;
		}

		/// <summary>
		/// Function to create a 3D texture and the mip chain from a list of GDI+ image objects.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="images">Images to load.</param>
		/// <returns>The clone of the GDI+ images contained within a new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="images"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
        /// <para>-or-</para>
        /// <para>Thrown when an image in the list is NULL.</para>		
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the depth and mip count.</exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture3D">GorgonTexture3D</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>This overload is used to create a 3D image from a list of images.  If the MipCount is set to 1, then
        /// only the first image will be processed IF there is only one image in the list.  If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
        /// of the list will be taken as the depth size.</para>
        /// <para>The layout of the image list is processed in the following order (assuming the MipCount = 2, and the depth is = 4):</para>
        /// <code>
        /// images[0]: Mip Level 0, Depth slice 0.
        /// images[1]: Mip Level 0, Depth slice 1
        /// images[2]: Mip Level 0, Depth slice 2
        /// images[3]: Mip Level 0, Depth slice 3
        /// images[4]: Mip Level 1, Depth slice 4
        /// images[5]: Mip Level 1, Depth slice 5
        /// </code>
        /// <para>The depth is shrunk by a power of 2 for each mip level.  So, at mip level 0 we have 4 depth slices, and at mip level 1 we have 2.  If we had a third mip level, then 
        /// the depth would be 1 at that mip level.</para>
        /// <para>Note that unlike other image types, there is no array.  3D images do not support arrays and will ignore them.  Also note that a 3D image MUST have a width, height 
        /// and depth that is a power of 2 if mip maps are to be used.  If the image does not meet the criteria, then an exception will be thrown.
        /// </para>
        /// <para>The list of images must be large enough to accomodate the number of mip map levels and the depth at each mip level, must not contain any NULL (Nothing in VB.Net) elements and all images must use
        /// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until mip count and each depth for each mip level are used.  Unlike other overloads, 
        /// this method will NOT auto-generate mip-maps and will only use the images provided.</para>
        /// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
        /// </remarks>
        public GorgonTexture3D Create3DTextureFromGDIImage(string name, IList<Image> images)
		{
			return Create3DTextureFromGDIImage(name, images, new GorgonGDIOptions());
		}

		/// <summary>
		/// Function to create a 3D texture and the mip chain from a list of GDI+ image objects.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="images">Images to load.</param>
		/// <param name="options">Settings for the conversion process.</param>
		/// <returns>The clone of the GDI+ images contained within a new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="images"/>, <paramref name="options"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when the images parameter is NULL (Nothing in VB.Net) or empty.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the pixel format is unsupported or not the same across all images.</para>
        /// <para>-or-</para>
        /// <para>Thrown when an image in the list is NULL.</para>		
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the images parameter does not contain enough elements to satisfy the depth and mip count.</exception>
        /// <remarks>
        /// This method will create a new <see cref="GorgonLibrary.Graphics.GorgonTexture3D">GorgonTexture3D</see> object from a list of <see cref="System.Drawing.Image">GDI+ Images</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>This overload is used to create a 3D image from a list of images.  If the MipCount is set to 1, then
        /// only the first image will be processed IF there is only one image in the list.  If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
        /// of the list will be taken as the depth size.</para>
        /// <para>The layout of the image list is processed in the following order (assuming the MipCount = 2, and the depth is = 4):</para>
        /// <code>
        /// images[0]: Mip Level 0, Depth slice 0.
        /// images[1]: Mip Level 0, Depth slice 1
        /// images[2]: Mip Level 0, Depth slice 2
        /// images[3]: Mip Level 0, Depth slice 3
        /// images[4]: Mip Level 1, Depth slice 4
        /// images[5]: Mip Level 1, Depth slice 5
        /// </code>
        /// <para>The depth is shrunk by a power of 2 for each mip level.  So, at mip level 0 we have 4 depth slices, and at mip level 1 we have 2.  If we had a third mip level, then 
        /// the depth would be 1 at that mip level.</para>
        /// <para>Note that unlike other image types, there is no array.  3D images do not support arrays and will ignore them.  Also note that a 3D image MUST have a width, height 
        /// and depth that is a power of 2 if mip maps are to be used.  If the image does not meet the criteria, then an exception will be thrown.
        /// </para>
        /// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <description>Width</description><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <description>Height</description><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <description>Depth</description><description>This sets the depth for the image.  The default value is set to 1.  If there are no mip-maps (i.e. MipCount = 1), then the number of elements in the list will be used as the depth size.</description>
        /// </item>
        /// <item>
        /// <description>Format</description><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <description>MipCount</description><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <description>ArrayCount</description><description>Image arrays are not available for 3D images.</description>
        /// </item>
        /// <item>
        /// <description>Dither</description><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <description>Filter</description><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <description>UseClipping</description><description>Set to TRUE to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is FALSE.</description>
        /// </item>
        /// </list>
        /// <para>The list of images must be large enough to accomodate the number of mip map levels and the depth at each mip level, must not contain any NULL (Nothing in VB.Net) elements and all images must use
        /// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until mip count and each depth for each mip level are used.  Unlike other overloads, 
        /// this method will NOT auto-generate mip-maps and will only use the images provided.</para>
        /// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
        /// </remarks>
        public GorgonTexture3D Create3DTextureFromGDIImage(string name, IList<Image> images, GorgonGDIOptions options)
		{
			GorgonTexture3D result = null;

			if (images == null)
			{
				throw new ArgumentNullException("images");
			}

			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			if (images.Count == 0)
			{
				throw new ArgumentException("The parameter must not be empty.", "images");
			}

			using (GorgonImageData data = GorgonImageData.Create3DFromGDIImage(images, options))
			{
				result = CreateTexture<GorgonTexture3D>(name, data);
			}

			return result;
		}

		/// <summary>
		/// Function to load texture data from a file.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.  Must inherit from <see cref="GorgonLibrary.Graphics.GorgonTexture">GorgonTexture</see>.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="filePath">Path to the texture image file.</param>
		/// <param name="codec">Codec used to load the image data.</param>
		/// <returns>The texture populated with image data from the stream.</returns>
		/// <remarks>This will load a texture from a file.  The file must have been encoded by a supported image codec. 
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
        /// codecs may be defined and used to load a texture.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath "/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>The <paramref name="codec"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the file size is less than or equal to 0.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the data in the stream cannot be read by the image codec.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The image file is corrupted or unable to be read by a codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown if an attempt to read beyond the end of the stream is made.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the texture type was not recognized.</exception>
		public T FromFile<T>(string name, string filePath, GorgonImageCodec codec)
			where T : GorgonTexture
		{
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("The parameter must not be empty.", "filePath");
            }

			using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return FromStream<T>(name, fileStream, (int)fileStream.Length, codec);
			}
		}

		/// <summary>
		/// Function to load texture data from a byte array.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.  Must inherit from <see cref="GorgonLibrary.Graphics.GorgonTexture">GorgonTexture</see>.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="data">Byte array containing the texture data.</param>
		/// <param name="codec">Codec used to load the image data.</param>
		/// <returns>The texture populated with image data from the stream.</returns>
		/// <remarks>This will load a texture from a byte array.  The texture data in the array must have been encoded by a supported image codec.  
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
		/// codecs may be defined and used to load a texture.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="codec"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when size of the data parameter is less than or equal to 0.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the data in the array cannot be read by the image codec.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The image file is corrupted or unable to be read by a codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown if an attempt to read beyond the end of the stream is made.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the texture type was not recognized.</exception>
		public T FromMemory<T>(string name, byte[] data, GorgonImageCodec codec)
			where T : GorgonTexture
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			
			using (var memoryStream = new GorgonDataStream(data))
			{
				return FromStream<T>(name, memoryStream, data.Length, codec);
			}
		}

		/// <summary>
		/// Function to load texture data from a stream.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.  Must inherit from <see cref="GorgonLibrary.Graphics.GorgonTexture">GorgonTexture</see>.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream containing the texture data to load.</param>
		/// <param name="length">Length of the texture data, in bytes.</param>
		/// <param name="codec">Codec used to load the image data.</param>
		/// <returns>The texture populated with image data from the stream.</returns>
		/// <remarks>This will load a texture from a stream.  The texture data in the stream must have been encoded by a supported image codec.  
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
        /// codecs may be defined and used to load a texture.
        /// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="stream"/> parameter is NULL.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="codec"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="length"/> parameter is less than or equal to 0.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the data in the stream cannot be read by the image codec.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The image file is corrupted or unable to be read by a codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown if an attempt to read beyond the end of the stream is made.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the texture type was not recognized.</exception>
		public T FromStream<T>(string name, Stream stream, int length, GorgonImageCodec codec)
			where T : GorgonTexture
		{
			ITextureSettings settings = null;
			T result = null;

			using (GorgonImageData imageData = GorgonImageData.FromStream(stream, length, codec))
			{
				settings = GetTextureSettings(imageData.Settings);

				// Apply texture specific settings from the codec.
				settings.Usage = codec.Usage;
				settings.ViewFormat = codec.ViewFormat;
				settings.ViewIsUnordered = codec.ViewIsUnordered;

				switch (settings.ImageType)
				{
					case ImageType.Image1D:
						ValidateTexture1D(ref settings);
						result = CreateTexture<GorgonTexture1D>(name, imageData) as T;
						break;
					case ImageType.Image2D:
					case ImageType.ImageCube:
						ValidateTexture2D(ref settings);
						result = CreateTexture<GorgonTexture2D>(name, imageData) as T;
						break;
					case ImageType.Image3D:
						ValidateTexture3D(ref settings);
						result = CreateTexture<GorgonTexture3D>(name, imageData) as T;
						break;
					default:
						throw new GorgonException(GorgonResult.CannotCreate, "The texture type is unknown.");
				}
			}

			// This should never happen.
			if (result == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "The texture type is unknown.");
			}

			return result;
		}

		/// <summary>
		/// Function to create a new 3D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <param name="depth">Depth of the texture.</param>
		/// <param name="format">Format of the the texture.</param>
		/// <param name="usage">Usage for the texture.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture3D CreateTexture(string name, int width, int height, int depth, BufferFormat format, BufferUsage usage)
		{
			return CreateTexture<GorgonTexture3D>(name, new GorgonTexture3DSettings()
			{
				Width = width,
				Height = height,
				Depth = depth,
				Format = format,
				MipCount = 1,
				Usage = usage
			});
		}

		/// <summary>
		/// Function to create a new 2D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <param name="format">Format of the the texture.</param>
		/// <param name="usage">Usage for the texture.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture2D CreateTexture(string name, int width, int height, BufferFormat format, BufferUsage usage)
		{
			return CreateTexture<GorgonTexture2D>(name, new GorgonTexture2DSettings()
			{
				Width = width,
				Height = height,
				Format = format,
				MipCount = 1,
				ArrayCount = 1,
				Multisampling = new GorgonMultisampling(1, 0),
				Usage = usage
			});
		}

		/// <summary>
		/// Function to create a new 1D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="width">Width of the texture.</param>
		/// <param name="format">Format of the the texture.</param>
		/// <param name="usage">Usage for the texture.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture1D CreateTexture(string name, int width, BufferFormat format, BufferUsage usage)
		{
			return CreateTexture<GorgonTexture1D>(name, new GorgonTexture1DSettings()
			{
				Width = width,
				Format = format,
				MipCount = 1,
				ArrayCount = 1,
				Usage = usage
			});
		}

		/// <summary>
		/// Function to create a new texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <returns>A new texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="settings"/> parameters are NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public T CreateTexture<T>(string name, ITextureSettings settings)
			where T : GorgonTexture
		{
			Type type = typeof(T);
			T texture = null;

			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("The parameter must not be NULL or empty.", "name");
			}

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			// We cannot create an immutable texture without some initialization data.
			if (settings.Usage == BufferUsage.Immutable)
			{
				throw new ArgumentException("Immutable textures require initialization data.", "data");
			}

			switch (settings.ImageType)
			{
				case ImageType.Image1D:
					ValidateTexture1D(ref settings);
					texture = new GorgonTexture1D(_graphics, name, settings) as T;
					break;
				case ImageType.ImageCube:
				case ImageType.Image2D:
					ValidateTexture2D(ref settings);
					texture = new GorgonTexture2D(_graphics, name, settings) as T;
					break;
				case ImageType.Image3D:
					ValidateTexture3D(ref settings);
					texture = new GorgonTexture3D(_graphics, name, settings) as T;
					break;
				default:
					throw new ArgumentException("The texture type '" + type.FullName + "' is unknown.", "settings");
			}

			if (texture == null)
			{
				throw new ArgumentException("The texture type '" + type.FullName + "' is unknown.", "settings");
			}

			texture.Initialize(null);

			_graphics.AddTrackedObject(texture);
			return texture;
		}

		/// <summary>
		/// Function to create a new texture.
		/// </summary>
		/// <typeparam name="T">Type of texture to create.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <returns>A new texture.</returns>
		/// <remarks>This will create a new texture from the image data specified.
		/// <para>The texture settings width, height, depth, mip count, array count, and format will use the settings from the <paramref name="data"/> parameter.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="data"/> parameters are NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if there is no data to upload to the texture.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public T CreateTexture<T>(string name, GorgonImageData data)
			where T : GorgonTexture
		{
			ITextureSettings settings = null;
			Type type = typeof(T);
			T texture = null;

			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("The parameter must not be NULL or empty.", "name");
			}

			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			if (data.Count == 0)
			{
				throw new ArgumentException("There was no image data to upload to the texture.", "data");
			}

			// Get the settings from the image data.
			settings = GetTextureSettings(data.Settings);

			switch (settings.ImageType)
			{
				case ImageType.Image1D:
					ValidateTexture1D(ref settings);
					texture = new GorgonTexture1D(_graphics, name, settings) as T;
					break;
				case ImageType.ImageCube:
				case ImageType.Image2D:
					ValidateTexture2D(ref settings);
					texture = new GorgonTexture2D(_graphics, name, settings) as T;
					break;
				case ImageType.Image3D:
					ValidateTexture3D(ref settings);
					texture = new GorgonTexture3D(_graphics, name, settings) as T;
					break;
				default:
					throw new ArgumentException("The texture type '" + type.FullName + "' is unknown.", "settings");
			}

			if (texture == null)
			{
				throw new ArgumentException("The texture type '" + type.FullName + "' is unknown.", "settings");
			}

			texture.Initialize(data);

			_graphics.AddTrackedObject(texture);
			return texture;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextures"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		internal GorgonTextures(GorgonGraphics graphics)
		{
			_graphics = graphics;
		}
		#endregion
	}
}
