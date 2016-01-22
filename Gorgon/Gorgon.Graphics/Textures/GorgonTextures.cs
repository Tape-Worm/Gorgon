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
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Native;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Textures interface.
	/// </summary>
	public sealed class GorgonTextures
	{
		#region Variables.
	    private static readonly object _syncLock = new object();
		private readonly GorgonGraphics _graphics;
		private GorgonTexture2D _logo;
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
                lock(_syncLock)
			    {
                    // If we're in a deferred context, then return the logo from the immediate context.
                    if (_graphics.IsDeferred)
                    {
                        return _graphics.ImmediateContext.Textures.GorgonLogo;
                    }

				    if (_logo != null)
				    {
					    return _logo;
				    }

				    _logo = _graphics.Textures.CreateTexture<GorgonTexture2D>("Gorgon.Logo", Resources.Gorgon_2_x_Logo_Small);

				    // Don't track this image.
				    _graphics.RemoveTrackedObject(_logo);
			    }

			    return _logo;
			}
		}

		/// <summary>
		/// Property to return the maximum number of array indices for 1D and 2D textures.
		/// </summary>
		public int MaxArrayCount => 2048;

		/// <summary>
		/// Property to return the maximum width of a 1D or 2D texture.
		/// </summary>
		public int MaxWidth
		{
			get
			{
				switch (_graphics.VideoDevice.SupportedFeatureLevel)
				{
					case DeviceFeatureLevel.Sm4:
					case DeviceFeatureLevel.Sm41:
						return 8192;
					default:
						return 16384;
				}
			}
		}

		/// <summary>
		/// Property to return the maximum height of a 2D texture.
		/// </summary>
		public int MaxHeight
		{
			get
			{
				switch (_graphics.VideoDevice.SupportedFeatureLevel)
				{
					case DeviceFeatureLevel.Sm4:
					case DeviceFeatureLevel.Sm41:
						return 8192;
					default:
						return 16384;
				}
			}
		}

		/// <summary>
		/// Property to return the maximum width of a volume (3D) texture.
		/// </summary>
		public int Max3DWidth => 2048;

		/// <summary>
		/// Property to return the maximum height of a volume (3D) texture.
		/// </summary>
		public int Max3DHeight => 2048;

		/// <summary>
		/// Property to return the maximum depth of a texture.
		/// </summary>
		public int MaxDepth => 2048;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the width and height as powers of two.
		/// </summary>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <param name="depth">Depth of the texture.</param>
		/// <returns>The width, height and depth bumped to the nearest power of two.</returns>
		private static Tuple<int, int, int> GetPow2Size(int width, int height, int depth)
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
		internal void ValidateTexture3D(ref ITextureSettings settings)
		{
			if (((settings.Usage == BufferUsage.Dynamic) || (settings.Usage == BufferUsage.Staging))
				&& (settings.AllowUnorderedAccessViews))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UNORDERED_NO_STAGING_DYNAMIC);
			}

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_SNorm;

			if (settings.Width < 0)
				settings.Width = 0;

			if (settings.Height < 0)
				settings.Height = 0;

			// Mip maps also require power of 2 sizing for volume textures.
			if (settings.MipCount != 1)
			{
				if (!_graphics.VideoDevice.SupportsMipMaps(settings.Format))
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_NO_MIP_SUPPORT, settings.Format));
				}

				Tuple<int, int, int> newSize = GetPow2Size(settings.Width, settings.Height, settings.Depth);
				settings.Width = newSize.Item1;
				settings.Height = newSize.Item2;
				settings.Depth = newSize.Item3;
			}

            if ((_graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.Sm5)
                && (settings.AllowUnorderedAccessViews))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UAV_REQUIRES_SM5);
            }

            // Check texture size if using a compressed format.
			var formatInfo = new GorgonBufferFormatInfo(settings.Format);

			if (formatInfo.IsCompressed)
			{
				if (((settings.Width % 4) != 0)
					|| ((settings.Height % 4) != 0))
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_BC_SIZE_NOT_MOD_4);
				}
			}

			if (settings.Width > Max3DWidth)
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_WIDTH_INVALID, ImageType.Image3D, settings.Width, Max3DWidth));
			if (settings.Height > Max3DHeight)
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_HEIGHT_INVALID, ImageType.Image3D, settings.Width, Max3DHeight));
			if (settings.Depth > MaxDepth)
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_DEPTH_INVALID, ImageType.Image3D, settings.Width, MaxDepth));

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports3DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_FORMAT_NOT_SUPPORTED, settings.Format, ImageType.Image3D));
		}

		/// <summary>
		/// Function to perform clean up of resources.
		/// </summary>
		internal void CleanUp()
		{
		    if (_logo != null)
		    {
		        _logo.Dispose();
		    }

		    _logo = null;			
		}

		/// <summary>
		/// Function to validate the 2D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		internal void ValidateTexture2D(ref ITextureSettings settings)
		{
			if (((settings.Usage == BufferUsage.Dynamic) || (settings.Usage == BufferUsage.Staging))
				&& (settings.AllowUnorderedAccessViews))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UNORDERED_NO_STAGING_DYNAMIC);
			}

			if (settings.ArrayCount < 1)
			{
				settings.ArrayCount = 1;
			}

			if (settings.ArrayCount > _graphics.Textures.MaxArrayCount)
			{
				settings.ArrayCount = _graphics.Textures.MaxArrayCount;
			}

			if (settings.IsTextureCube)
			{
				if ((settings.ArrayCount != 6) && (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.Sm4))
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_CUBE_NEEDS_MAX_SIX_SM4);
				}

				if ((settings.ArrayCount % 6) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_CUBE_MULTIPLE_OF_SIX);
				}
			}

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if ((settings.MipCount > 1)
				&& (!_graphics.VideoDevice.SupportsMipMaps(settings.Format)))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_NO_MIP_SUPPORT, settings.Format));
			}

			if (settings.Multisampling.Count == 0)
				settings.Multisampling = new GorgonMultisampling(1, 0);

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_SNorm;

			if (settings.Width < 0)
				settings.Width = 0;

			if (settings.Height < 0)
				settings.Height = 0;
			
			// Check texture size if using a compressed format.
			var formatInfo = new GorgonBufferFormatInfo(settings.Format);

			if (formatInfo.IsCompressed)
			{
				if (((settings.Width % 4) != 0)
					|| ((settings.Height % 4) != 0))
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_BC_SIZE_NOT_MOD_4);
				}
			}

            if ((_graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.Sm5)
                && (settings.AllowUnorderedAccessViews))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UAV_REQUIRES_SM5);
            }

			if (settings.Width > MaxWidth)
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_WIDTH_INVALID, ImageType.Image2D, settings.Width, MaxWidth));
			if (settings.Height > MaxHeight)
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_HEIGHT_INVALID, ImageType.Image2D, settings.Height, MaxHeight));

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports2DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_FORMAT_NOT_SUPPORTED, settings.Format, ImageType.Image2D));			
		}

		/// <summary>
		/// Function to validate the 1D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		internal void ValidateTexture1D(ref ITextureSettings settings)
		{
			if (((settings.Usage == BufferUsage.Dynamic) || (settings.Usage == BufferUsage.Staging))
			    && (settings.AllowUnorderedAccessViews))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UNORDERED_NO_STAGING_DYNAMIC);
			}

			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.ArrayCount > _graphics.Textures.MaxArrayCount)
			{
				settings.ArrayCount = _graphics.Textures.MaxArrayCount;
			}

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_SNorm;

			if (settings.Width < 0)
				settings.Width = 0;

			if ((settings.MipCount > 1)
				&& (!_graphics.VideoDevice.SupportsMipMaps(settings.Format)))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_NO_MIP_SUPPORT, settings.Format));
			}

			// Check texture size if using a compressed format.
			var formatInfo = new GorgonBufferFormatInfo(settings.Format);

			if (formatInfo.IsCompressed)
			{
				if ((settings.Width % 4) != 0)
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_BC_SIZE_NOT_MOD_4);
				}
			}

            if ((_graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.Sm5)
                && (settings.AllowUnorderedAccessViews))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UAV_REQUIRES_SM5);
            }

			if (settings.Width > MaxWidth)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_WIDTH_INVALID, ImageType.Image1D, settings.Width, MaxWidth));
			}

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports1DTextureFormat(settings.Format))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_TEXTURE_FORMAT_NOT_SUPPORTED, settings.Format, ImageType.Image1D));
			}
		}

        /// <summary>
        /// Function to create a texture from a list of System.Drawing.Image objects.
        /// </summary>
        /// <param name="name">Name of the texture.</param>
        /// <param name="images">Images to load.</param>
        /// <param name="options">[Optional] Settings for the conversion process.</param>
        /// <typeparam name="T">Type of texture.</typeparam>
        /// <returns>The clone of the GDI+ images contained within a new texture.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="images"/> parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
        /// <para>-or-</para>
        /// <para>Thrown when the images parameter is NULL (<i>Nothing</i> in VB.Net) or empty.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown when the texture could not be created.</exception>
        /// <remarks>
        /// This method will create a new <see cref="Gorgon.Graphics.GorgonTexture">GorgonTexture</see> object from a list of <see cref="System.Drawing.Image">System.Drawing.Images</see>.
        /// The method will copy the image information and do a best fit conversion.
        /// <para>If type is a 1D or 2D texture, then the array can be laid out as mip slices and array indices, if it is a 3D texture, then it will be laid out as mip slices and depth slices. 
        /// If the MipCount is set to 1, then only the first image will be processed IF there is only one image in the list.</para>
        /// <para><strong>For 1D and 2D images:</strong> If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
        /// of the list will be taken as the array size. The layout of the image list is processed in the following order (assuming the MipCount = 2, and the array count is = 4):</para>
        /// <para>
        /// <code>
        /// images[0]: Array Index 0, Mip Level 0<br/>
        /// images[1]: Array Index 0, Mip Level 1<br/>
        /// images[2]: Array Index 0, Mip Level 2<br/>
        /// images[3]: Array Index 0, Mip Level 3<br/>
        /// images[4]: Array Index 1, Mip Level 0<br/>
        /// images[5]: Array Index 1, Mip Level 1<br/>
        /// images[6]: Array Index 1, Mip Level 2<br/>
        /// images[7]: Array Index 1, Mip Level 3<br/>
        /// </code>
        /// </para>
        /// <para><strong>For 3D images:</strong> If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
        /// of the list will be taken as the depth size. The layout of the image list is processed in the following order (assuming the MipCount = 2, and the depth is = 4):</para>
        /// <para>
        /// <code>
        /// images[0]: Mip Level 0, Depth slice 0<br/>
        /// images[1]: Mip Level 0, Depth slice 1<br/>
        /// images[2]: Mip Level 0, Depth slice 2<br/>
        /// images[3]: Mip Level 0, Depth slice 3<br/>
        /// images[4]: Mip Level 1, Depth slice 4<br/>
        /// images[5]: Mip Level 1, Depth slice 5<br/>
        /// </code>
        /// The depth is shrunk by a power of 2 for each mip level.  So, at mip level 0 we have 4 depth slices, and at mip level 1 we have 2.  If we had a third mip level, then 
        /// the depth would be 1 at that mip level.
        /// </para>
        /// <para>3D textures MUST have a width, height and depth that is a power of 2 if mip maps are to be used.  If the image does not meet the criteria, then an exception will be thrown.
        /// </para>
        /// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>Width</term><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <term>Height</term><description>For 2D and 3D images only. The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <term>Depth</term><description>For 3D images only. This sets the depth for the image.  The default value is set to 1.  If there are no mip-maps (i.e. MipCount = 1), then the number of elements in the list will be used as the depth size.</description>
        /// </item>
        /// <item>
        /// <term>Format</term><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <term>MipCount</term><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <term>ArrayCount</term><description>For 1D/2D images only.  Gorgon will generate the requested number of image arrays from the source image.  Set to 1 if no image arrays are required.</description>
        /// </item>
        /// <item>
        /// <term>Dither</term><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <term>Filter</term><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <term>UseClipping</term><description>Set to <b>true</b> to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is <b>false</b>.</description>
        /// </item>
        /// <item>
        /// <term>ViewFormat</term><description>The format for the default shader view applied to the texture.  If this value is set to Unknown, then the texture format will be used.  The default value is Unknown.</description>
        /// </item>
        /// <item>
        /// <term>AllowUnorderedAccess</term><description><b>true</b> to allow unordered access views to be used with this texture, <b>false</b> to disallow.  The default value is <b>false</b>.</description>
        /// </item>
        /// <item>
        /// <term>Multisampling</term><description>For 2D textures only and only if the <paramref name="images"/> parameter contains 1 image.  Multisampling values to apply to the texture.  The default is a count of 1 and a quality of 0 (no multisampling).</description>
        /// </item>
        /// </list>
        /// <para>The list of images must be large enough to accomodate the number of mip map levels and the depth at each mip level, must not contain any NULL (<i>Nothing</i> in VB.Net) elements and all images must use
        /// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until mip count and each depth for each mip level are used.  Unlike other overloads, 
        /// this method will NOT auto-generate mip-maps and will only use the images provided.</para>
        /// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
        /// <para>This method should not be called from a deferred graphics context.</para>
        /// </remarks>
        public T CreateTexture<T>(string name, IList<Image> images, GorgonGDIOptions options = null)
            where T : GorgonTexture
        {
            Type textureType = typeof(T);
            var imageType = ImageType.Unknown;

            if (images == null)
            {
                throw new ArgumentNullException(nameof(images));
            }

            if (images.Count == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(images));
            }

            // If we only have 1 image, then use that one.
            if (images.Count == 1)
            {
                return CreateTexture<T>(name, images[0], options);
            }

            if (textureType == typeof(GorgonTexture1D))
            {
                imageType = ImageType.Image1D;
            }
            else if (textureType == typeof(GorgonTexture2D))
            {
                imageType = ImageType.Image2D;
            }
            else if (textureType == typeof(GorgonTexture3D))
            {
                imageType = ImageType.Image3D;
            }

            if (imageType == ImageType.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_IMAGE_TYPE_INVALID);
            }

            if (options == null)
            {
                options = new GorgonGDIOptions();
            }

            using (GorgonImageData data = GorgonImageData.CreateFromGDIImage(images, imageType, options))
            {
                var settings = (ITextureSettings)data.Settings;

				var info = new GorgonBufferFormatInfo(settings.Format);

                // If we're using unordered access, then make the format typeless.
	            if ((info.Group == BufferFormat.Unknown)
					|| (!options.AllowUnorderedAccess))
	            {
		            return CreateTexture<T>(name, data, settings);
	            }

	            // Remap the shader view.
	            if (settings.ShaderViewFormat == BufferFormat.Unknown)
	            {
		            settings.ShaderViewFormat = settings.Format;
	            }

	            settings.Format = info.Group;

	            return CreateTexture<T>(name, data, settings);
            }
        }

        /// <summary>
        /// Function to create a texture from a System.Drawing.Image object.
        /// </summary>
        /// <typeparam name="T">Type of texture.</typeparam>
        /// <param name="name">Name of the texture.</param>
        /// <param name="image">GDI+ image to convert to a texture.</param>
        /// <param name="options">[Optional] Options for conversion.</param>
        /// <returns>A texture containing the same data as the GDI+ image.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="image"/> parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        /// <exception cref="GorgonException">Thrown when the texture could not be created.</exception>
        /// <remarks>
        /// This method will create a new <see cref="Gorgon.Graphics.GorgonTexture">GorgonTexture</see> object from a <see cref="System.Drawing.Image"/>. 
        /// The method will copy the image information and do a best fit conversion.
        /// <para>This overload only applies to 1D and 2D textures only.  3D textures are not supported.</para>
        /// <para>The <paramref name="options"/> parameter controls how the <paramref name="image" /> is converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>Width</term><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <term>Height</term><description>The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <term>Depth</term><description>This is ignored for 1D and 2D images. The image</description>
        /// </item>
        /// <item>
        /// <term>Format</term><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <term>MipCount</term><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 0 to generate a full mip-map chain, or set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <term>ArrayCount</term><description>This is ignored for this overload.</description>
        /// </item>
        /// <item>
        /// <term>Dither</term><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <term>Filter</term><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <term>UseClipping</term><description>Set to <b>true</b> to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is <b>false</b>.</description>
        /// </item>
        /// <item>
        /// <description>ViewFormat</description><description>The format for the default shader view applied to the texture.  If this value is set to Unknown, then the texture format will be used.  The default value is Unknown.</description>
        /// </item>
        /// <item>
        /// <description>AllowUnorderedAccess</description><description><b>true</b> to allow unordered access views to be used with this texture, <b>false</b> to disallow.  The default value is <b>false</b>.</description>
        /// </item>
        /// <item>
        /// <description>Multisampling</description><description>For 2D textures only.  Multisampling values to apply to the texture.  The default is a count of 1 and a quality of 0 (no multisampling).</description>
        /// </item>
        /// </list>
        /// <para>This method should not be called from a deferred graphics context.</para>
        /// </remarks>
        public T CreateTexture<T>(string name, Image image, GorgonGDIOptions options = null)
            where T : GorgonTexture
        {
            Type textureType = typeof(T);
            var imageType = ImageType.Unknown;

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (options == null)
            {
                options = new GorgonGDIOptions();
            }

            if (textureType == typeof(GorgonTexture1D))
            {
                imageType = ImageType.Image1D;
            }
            else if (textureType == typeof(GorgonTexture2D))
            {
                imageType = ImageType.Image2D;
            }

            if (imageType == ImageType.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_IMAGE_TYPE_INVALID);
            }
            
            using (GorgonImageData data = GorgonImageData.CreateFromGDIImage(image, imageType, options))
            {
                var settings = (ITextureSettings)data.Settings;

                if ((imageType == ImageType.Image2D)
                    && ((options.Multisampling.Count > 1) || (options.Multisampling.Quality > 0)))
                {
                    settings.Multisampling = options.Multisampling;
                }

	            var info = new GorgonBufferFormatInfo(settings.Format);

				// If we're using unordered access, then make the format typeless.
				if ((info.Group == BufferFormat.Unknown)
					|| (!options.AllowUnorderedAccess))
	            {
		            return CreateTexture<T>(name, data, settings);
	            }

	            // Remap the shader view.
	            if (settings.ShaderViewFormat == BufferFormat.Unknown)
	            {
		            settings.ShaderViewFormat = settings.Format;
	            }

	            settings.Format = info.Group;

	            return CreateTexture<T>(name, data, settings);
            }
        }

		/// <summary>
		/// Function to load texture data from a file.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.  Must inherit from <see cref="Gorgon.Graphics.GorgonTexture">GorgonTexture</see>.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="filePath">Path to the texture image file.</param>
		/// <param name="codec">Codec used to load the image data.</param>
		/// <returns>The texture populated with image data from the stream.</returns>
		/// <remarks>This will load a texture from a file.  The file must have been encoded by a supported image codec. 
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
        /// codecs may be defined and used to load a texture.
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath "/> parameter is NULL (<i>Nothing</i> in VB.Net).
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
		/// <exception cref="GorgonException">Thrown if the texture type was not recognized.</exception>
		public T FromFile<T>(string name, string filePath, GorgonImageCodec codec)
			where T : GorgonTexture
		{
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(filePath));
            }

			using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return FromStream<T>(name, fileStream, (int)fileStream.Length, codec);
			}
		}

		/// <summary>
		/// Function to load texture data from a byte array.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.  Must inherit from <see cref="Gorgon.Graphics.GorgonTexture">GorgonTexture</see>.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="data">Byte array containing the texture data.</param>
		/// <param name="codec">Codec used to load the image data.</param>
		/// <returns>The texture populated with image data from the stream.</returns>
		/// <remarks>This will load a texture from a byte array.  The texture data in the array must have been encoded by a supported image codec.  
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
		/// codecs may be defined and used to load a texture.
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
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
		/// <exception cref="GorgonException">Thrown if the texture type was not recognized.</exception>
		public T FromMemory<T>(string name, byte[] data, GorgonImageCodec codec)
			where T : GorgonTexture
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}
			
			using (IGorgonPointer pointer = new GorgonPointerPinned<byte>(data))
			{
				return FromStream<T>(name, new GorgonDataStream(pointer), data.Length, codec);
			}
		}

		/// <summary>
		/// Function to load texture data from a stream.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.  Must inherit from <see cref="Gorgon.Graphics.GorgonTexture">GorgonTexture</see>.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream containing the texture data to load.</param>
		/// <param name="length">Length of the texture data, in bytes.</param>
		/// <param name="codec">Codec used to load the image data.</param>
		/// <returns>The texture populated with image data from the stream.</returns>
		/// <remarks>This will load a texture from a stream.  The texture data in the stream must have been encoded by a supported image codec.  
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
        /// codecs may be defined and used to load a texture.
        /// <para>This method should not be called from a deferred graphics context.</para>
        /// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
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
		/// <exception cref="GorgonException">Thrown if the texture type was not recognized.</exception>
		public T FromStream<T>(string name, Stream stream, int length, GorgonImageCodec codec)
			where T : GorgonTexture
		{
            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }

            using (GorgonImageData imageData = GorgonImageData.FromStream(stream, length, codec))
			{
				var settings = (ITextureSettings)imageData.Settings;

				// Apply texture specific settings from the codec.
				settings.Usage = codec.Usage;
				settings.ShaderViewFormat = codec.ViewFormat;
				settings.AllowUnorderedAccessViews = codec.AllowUnorderedAccess;

                // Apply multisampling.
                if ((settings.ImageType == ImageType.Image2D)
                    && (settings.MipCount == 1)
                    && (settings.ArrayCount == 1)
                    && ((codec.Multisampling.Count > 1) || (codec.Multisampling.Quality > 0)))
                {
                    settings.Multisampling = codec.Multisampling;
                }

				var info = new GorgonBufferFormatInfo(settings.Format);

				// If we've opted for unordered access views, then make the current format typeless.
				if ((info.Group == BufferFormat.Unknown)
					|| (!settings.AllowUnorderedAccessViews))
				{
					return CreateTexture<T>(name, imageData, settings);
				}

				// Remap the shader view.
				if (settings.ShaderViewFormat == BufferFormat.Unknown)
				{
					settings.ShaderViewFormat = settings.Format;
				}

				settings.Format = info.Group;

				return CreateTexture<T>(name, imageData, settings);
			}
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
		/// <returns>A new 2D texture.
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture3D CreateTexture(string name, int width, int height, int depth, BufferFormat format, BufferUsage usage)
		{
			return CreateTexture(name, new GorgonTexture3DSettings
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
		/// <returns>A new 2D texture.
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture2D CreateTexture(string name, int width, int height, BufferFormat format, BufferUsage usage)
		{
			return CreateTexture(name, new GorgonTexture2DSettings
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
		/// <returns>A new 1D texture.
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture1D CreateTexture(string name, int width, BufferFormat format, BufferUsage usage)
		{
			return CreateTexture(name, new GorgonTexture1DSettings
			{
				Width = width,
				Format = format,
				MipCount = 1,
				ArrayCount = 1,
				Usage = usage
			});
		}

		/// <summary>
		/// Function to create a new 1D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <returns>A new texture.</returns>
        /// <remarks>This method should not be called from a deferred graphics context.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="settings"/> parameters are NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture1D CreateTexture(string name, GorgonTexture1DSettings settings)
		{
            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }

            if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			// We cannot create an immutable texture without some initialization data.
			if (settings.Usage == BufferUsage.Immutable)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_IMMUTABLE_REQUIRES_DATA);
			}

			var textureSettings = settings as ITextureSettings;
			ValidateTexture1D(ref textureSettings);
			var texture = new GorgonTexture1D(_graphics, name, textureSettings);

			texture.Initialize(null);

			_graphics.AddTrackedObject(texture);
			return texture;
		}

		/// <summary>
		/// Function to create a new 2D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <returns>A new texture.</returns>
        /// <remarks>This method should not be called from a deferred graphics context.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="settings"/> parameters are NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture2D CreateTexture(string name, GorgonTexture2DSettings settings)
		{
            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }

            if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			// We cannot create an immutable texture without some initialization data.
			if (settings.Usage == BufferUsage.Immutable)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_IMMUTABLE_REQUIRES_DATA);
			}

			var textureSettings = settings as ITextureSettings;
			ValidateTexture2D(ref textureSettings);
			var texture = new GorgonTexture2D(_graphics, name, textureSettings);

			texture.Initialize(null);

			_graphics.AddTrackedObject(texture);
			return texture;
		}

		/// <summary>
		/// Function to create a new 3D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <returns>A new texture.</returns>
        /// <remarks>This method should not be called from a deferred graphics context.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="settings"/> parameters are NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture3D CreateTexture(string name, GorgonTexture3DSettings settings)
		{
            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }

            if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			// We cannot create an immutable texture without some initialization data.
			if (settings.Usage == BufferUsage.Immutable)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_TEXTURE_IMMUTABLE_REQUIRES_DATA);
			}

			var textureSettings = settings as ITextureSettings;
			ValidateTexture3D(ref textureSettings);
			var texture = new GorgonTexture3D(_graphics, name, settings);

			texture.Initialize(null);

			_graphics.AddTrackedObject(texture);
			return texture;
		}

		/// <summary>
		/// Function to create a new texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <param name="settingsOverride">[Optional] Texture settings that override the settings from the image.</param>
		/// <typeparam name="T">The type of texture to use.</typeparam>
		/// <returns>A new texture.</returns>
		/// <remarks>This will create a new texture from the image data specified.
		/// <para>The texture settings width, height, depth, mip count, array count, and format will use the settings from the <paramref name="data"/> parameter.</para>
        /// <para>This method should not be called from a deferred graphics context.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="data"/> parameters are NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if there is no data to upload to the texture.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the texture could not be created.</exception>
		public T CreateTexture<T>(string name, GorgonImageData data, ITextureSettings settingsOverride = null)
			where T : GorgonTexture
		{
			T texture = null;

            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }

            if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Buffers.Count == 0)
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(data));
			}

			// Get the settings from the image data.
			var settings = settingsOverride ?? (ITextureSettings)data.Settings;

			switch (data.Settings.ImageType)
			{
				case ImageType.Image1D:
					ValidateTexture1D(ref settings);
					texture = new GorgonTexture1D(_graphics, name, settings) as T;
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					ValidateTexture2D(ref settings);
					texture = new GorgonTexture2D(_graphics, name, settings) as T;
					break;
				case ImageType.Image3D:
					ValidateTexture3D(ref settings);
					texture = new GorgonTexture3D(_graphics, name, settings) as T;
					break;
			}

			if (texture == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, data.Settings.ImageType));
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
