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
// Created: Friday, February 1, 2013 12:51:14 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Bitmap = SharpDX.WIC.Bitmap;

namespace Gorgon.IO
{
	// ReSharper disable ForCanBeConvertedToForeach

	/// <summary>
	/// A utility class to convert a GDI+ image object into a Gorgon 2D image object.
	/// </summary>
	static class GorgonGDIImageConverter
	{
		// Formats that can be converted.
		private static readonly Tuple<PixelFormat, BufferFormat>[] _formatConversion =
		{
		    new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format64bppArgb, BufferFormat.R16G16B16A16_UNorm),
		    new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format32bppArgb, BufferFormat.R8G8B8A8_UNorm),
		    new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppGrayScale, BufferFormat.R16_UNorm),
		    new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppArgb1555, BufferFormat.B5G5R5A1_UNorm),
		    new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppRgb565, BufferFormat.B5G6R5_UNorm)
		};

		// Best fit format mapping.
		private static readonly Tuple<PixelFormat, PixelFormat>[] _bestFit =
		{
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format1bppIndexed, PixelFormat.Format32bppArgb),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format4bppIndexed, PixelFormat.Format32bppArgb),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format8bppIndexed, PixelFormat.Format32bppArgb),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format16bppRgb555, PixelFormat.Format16bppArgb1555),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format24bppRgb, PixelFormat.Format32bppArgb),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format48bppRgb, PixelFormat.Format64bppArgb),
		    new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format64bppPArgb, PixelFormat.Format64bppArgb)
		};

        /// <summary>
        /// Function to retrieve the corresponding buffer format for a given pixel format.
        /// </summary>
        /// <param name="format">Pixel format to look up.</param>
        /// <returns>The corresponding buffer format if found, Unknown if not.</returns>
        private static BufferFormat GetBufferFormat(PixelFormat format)
        {
            PixelFormat bestFormat = format;
            var result = BufferFormat.Unknown;

            // Get the best fit for the pixel format.
            for (int i = 0; i < _bestFit.Length; i++)
            {
                if (format == _bestFit[i].Item1)
                {
                    bestFormat = _bestFit[i].Item2;
                }
            }

            // Find the conversion format.
            for (int i = 0; i < _formatConversion.Length; i++)
            {
                if (bestFormat == _formatConversion[i].Item1)
                {
                    result = _formatConversion[i].Item2;
                }
            }

            return result;
        }

        /// <summary>
        /// Function to return the corresponding pixel format for a given buffer format.
        /// </summary>
        /// <param name="format">Format to look up.</param>
        /// <returns>The corresponding pixel format if found, NULL if not.</returns>
        private static PixelFormat? GetPixelFormat(BufferFormat format)
        {
            PixelFormat? result = null;

            for (int i = 0; i < _formatConversion.Length; i++)
            {
                if (format == _formatConversion[i].Item2)
                {
                    result = _formatConversion[i].Item1;
                }
            }

            switch (format)
            {
				case BufferFormat.B8G8R8X8_UNorm:
				case BufferFormat.B8G8R8A8_UNorm:
                case BufferFormat.B8G8R8X8_UNorm_SRgb:
                case BufferFormat.R8G8B8A8_UNorm_SRgb:
                case BufferFormat.B8G8R8A8_UNorm_SRgb:
                    return PixelFormat.Format32bppArgb;
            }

            return result;
        }

		/// <summary>
		/// Function to create 1D Gorgon image data from a single System.Drawing.Image.
		/// </summary>
		/// <param name="wic">Windows Imaging Component interface to use.</param>
		/// <param name="image">An image to convert.</param>
        /// <param name="options">Options for image conversion.</param>
		/// <returns>The converted image data.</returns>
		public static GorgonImageData Create1DImageDataFromImage(GorgonWICImage wic, Image image, GorgonGDIOptions options)
		{
			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(image.PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported,
				                          string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, image.PixelFormat));
			}

			if (options.Width < 1)
			{
				options.Width = image.Width;
			}

			options.MipCount = options.MipCount < 1
				                   ? GorgonImageData.GetMaxMipCount(options.Width)
				                   : options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width));

			// Create our settings.
			var settings = new GorgonTexture1DSettings
				{
				Width = options.Width,
				MipCount = options.MipCount,
				ArrayCount = 1,
				Format = options.Format,
				AllowUnorderedAccessViews = options.AllowUnorderedAccess,
                ShaderViewFormat = options.ViewFormat,
                Usage = options.Usage
			};

			// Create our image data.
			var data = new GorgonImageData(settings);
			
			// Using the image, convert to a WIC bitmap object.
			using (Bitmap bitmap = wic.CreateWICImageFromImage(image))
			{
				for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
				{
					var buffer = data.Buffers[mipLevel];

					// Convert to a GorgonImageData type container.
					wic.AddWICBitmapToImageData(bitmap, options.Filter, options.Dither, buffer, options.UseClipping);
				}
			}

			return data;
		}

		/// <summary>
		/// Function to create 1D Gorgon image data from multiple single System.Drawing.Images.
		/// </summary>
		/// <param name="wic">Windows Imaging Component interface to use.</param>
		/// <param name="images">Images to convert.</param>
        /// <param name="options">The options for the image conversion.</param>
		/// <returns>The converted image data.</returns>
		public static GorgonImageData Create1DImageDataFromImages(GorgonWICImage wic, IList<Image> images, GorgonGDIOptions options)
		{
			if (options.Format == BufferFormat.Unknown)
			{
                options.Format = GetBufferFormat(images[0].PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported,
										  string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, images[0].PixelFormat));
			}

			if (images.Any(item => item.PixelFormat != images[0].PixelFormat))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_IMAGE_MUST_BE_SAME_FORMAT, images[0].PixelFormat));
			}

			if (options.Width < 1)
			{
				options.Width = images[0].Width;
			}

			if (options.ArrayCount < 1)
			{
				options.ArrayCount = 1;
			}

			options.MipCount = options.MipCount < 1 ? 1 : options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width));

			// Create our settings.
			var settings = new GorgonTexture1DSettings
				{
				Width = options.Width,
				MipCount = options.MipCount,
				ArrayCount = options.ArrayCount,
				Format = options.Format,
				AllowUnorderedAccessViews = options.AllowUnorderedAccess,
				ShaderViewFormat = options.ViewFormat,
                Usage = options.Usage
			};
			
			if ((options.ArrayCount * options.MipCount) > images.Count)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_IMAGE_MIPCOUNT_ARRAYCOUNT_TOO_LARGE);
			}

			// Create our image data.
			var data = new GorgonImageData(settings);

			for (int array = 0; array < data.Settings.ArrayCount; array++)
			{
				for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
				{
					var image = images[array * data.Settings.MipCount + mipLevel];

                    if (image == null)
                    {
                        continue;
                    }

					// Using the image, convert to a WIC bitmap object.
					using (var bitmap = wic.CreateWICImageFromImage(image))
					{
						var buffer = data.Buffers[mipLevel, array];

						wic.AddWICBitmapToImageData(bitmap, options.Filter, options.Dither, buffer, options.UseClipping);
					}
				}
			}

			return data;
		}

		/// <summary>
		/// Function to create 2D Gorgon image data from a single System.Drawing.Image.
		/// </summary>
		/// <param name="wic">Windows Imaging Component interface to use.</param>
		/// <param name="image">An image to convert.</param>
		/// <param name="options">Options for conversion.</param>
		/// <returns>The converted image data.</returns>
		public static GorgonImageData Create2DImageDataFromImage(GorgonWICImage wic, Image image, GorgonGDIOptions options)
        {
			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(image.PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported,
										  string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, image.PixelFormat));
			}

			if (options.Width < 1)
			{
				options.Width = image.Width;
			}

			if (options.Height < 1)
			{
				options.Height = image.Height;
			}

			// Specify 0 to generate a full mip chain.
			options.MipCount = options.MipCount < 1
				                   ? GorgonImageData.GetMaxMipCount(options.Width, options.Height)
				                   : options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width, options.Height));
			
			// Create our settings.
			var settings = new GorgonTexture2DSettings
				{
				Width = options.Width,
				Height = options.Height,
				MipCount = options.MipCount,
				ArrayCount = 1,
				Format = options.Format,
				AllowUnorderedAccessViews = options.AllowUnorderedAccess,
                ShaderViewFormat = options.ViewFormat,
                Usage = options.Usage
			};

            // Create our image data.
            var data = new GorgonImageData(settings);
			
			// Using the image, convert to a WIC bitmap object.
			using (Bitmap bitmap = wic.CreateWICImageFromImage(image))
			{
				for (int mipLevel = 0; mipLevel < options.MipCount; mipLevel++)
				{
					var buffer = data.Buffers[mipLevel];
					wic.AddWICBitmapToImageData(bitmap, options.Filter, options.Dither, buffer, options.UseClipping);
				}
			}

            return data;
        }

		/// <summary>
		/// Function to create 2D Gorgon image data from multiple single System.Drawing.Images.
		/// </summary>
		/// <param name="wic">Windows Imaging Component interface to use.</param>
		/// <param name="images">Images to convert.</param>
		/// <param name="options">Options for conversion.</param>
		/// <returns>The converted image data.</returns>
		public static GorgonImageData Create2DImageDataFromImages(GorgonWICImage wic, IList<Image> images, GorgonGDIOptions options)
		{
			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(images[0].PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported,
										  string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, images[0].PixelFormat));
			}

			if (images.Any(item => item.PixelFormat != images[0].PixelFormat))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_IMAGE_MUST_BE_SAME_FORMAT, images[0].PixelFormat));
			}

			if (options.Width < 1)
			{
				options.Width = images[0].Width;
			}

			if (options.Height < 1)
			{
				options.Height = images[0].Height;
			}

			if (options.ArrayCount < 1)
			{
				options.ArrayCount = 1;
			}

			options.MipCount = options.MipCount < 1
				                   ? 1
				                   : options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width, options.Height));

			// Create our settings.
			var settings = new GorgonTexture2DSettings
				{
				Width = options.Width,
				Height = options.Height,
				MipCount = options.MipCount,
				ArrayCount = options.ArrayCount,
				Format = options.Format,
				AllowUnorderedAccessViews = options.AllowUnorderedAccess,
                ShaderViewFormat = options.ViewFormat,
                Usage = options.Usage
			};

			if ((options.ArrayCount * options.MipCount) > images.Count)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_IMAGE_MIPCOUNT_ARRAYCOUNT_TOO_LARGE);
			}

			// Create our image data.
			var data = new GorgonImageData(settings);
			
			for (int array = 0; array < data.Settings.ArrayCount; array++)
			{
				for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
				{
					var image = images[array * data.Settings.MipCount + mipLevel];

                    if (image == null)
                    {
                        continue;
                    }

					// Using the image, convert to a WIC bitmap object.
					using (var bitmap = wic.CreateWICImageFromImage(image))
					{
						var buffer = data.Buffers[mipLevel, array];

						wic.AddWICBitmapToImageData(bitmap, options.Filter, options.Dither, buffer, options.UseClipping);
					}
				}
			}

			return data;
		}

		/// <summary>
		/// Function to create a 3D Gorgon image data from multiple System.Drawing.Images.
		/// </summary>
		/// <param name="wic">Windows Imaging Component interface to use.</param>
		/// <param name="images">Images to convert.</param>
		/// <param name="options">Conversion options.</param>
		/// <returns>The converted image data.</returns>
		public static GorgonImageData Create3DImageDataFromImages(GorgonWICImage wic, IList<Image> images, GorgonGDIOptions options)
		{
			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(images[0].PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported,
										  string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, images[0].PixelFormat));
			}

			if (images.Any(item => item.PixelFormat != images[0].PixelFormat))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_IMAGE_MUST_BE_SAME_FORMAT, images[0].PixelFormat));
			}

			if (options.Width <= 0)
			{
				options.Width = images[0].Width;
			}

			if (options.Height <= 0)
			{
				options.Height = images[0].Height;
			}

			if (options.Depth < 1)
			{
				options.Depth = 1;
			}

			options.MipCount = options.MipCount < 1
				                   ? 1
				                   : options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width, options.Height,
				                                                                         options.Depth));

			// Set the depth to the number of images if there are no mip-maps.
			if ((images.Count > 1) && (options.MipCount == 1))
			{
				options.Depth = images.Count;
			}

			// Create our settings.
			var settings = new GorgonTexture3DSettings
				{
				Width = options.Width,
				Height = options.Height,
				Depth = options.Depth,
				MipCount = options.MipCount,
				Format = options.Format,
				AllowUnorderedAccessViews = options.AllowUnorderedAccess,
                ShaderViewFormat = options.ViewFormat,
                Usage = options.Usage
			};

			// Only volume textures that are size to a power of 2 can have mip maps.
			if ((!settings.IsPowerOfTwo) && (options.MipCount > 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_IMAGE_VOLUME_NOT_POWER_OF_TWO);
			}

			// Create our image data.
			var data = new GorgonImageData(settings);

			int depthMip = options.Depth;
			int imageIndex = 0;
			for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
			{
				if (imageIndex >= images.Count)
				{
					data.Dispose();
					throw new GorgonException(GorgonResult.CannotCreate,  Resources.GORGFX_IMAGE_VOLUME_MIPCOUNT_DEPTHCOUNT_TOO_LARGE);
				}

                for (int depth = 0; depth < depthMip; depth++)
                {
                    var image = images[imageIndex + depth];

                    // Skip NULL images.
                    if (image == null)
                    {
                        continue;
                    }

                    // Using the image, convert to a WIC bitmap object.
                    using (var bitmap = wic.CreateWICImageFromImage(image))
                    {
                        var buffer = data.Buffers[mipLevel, depth];
                        wic.AddWICBitmapToImageData(bitmap, options.Filter, options.Dither, buffer, options.UseClipping);
                    }
                }

				imageIndex += depthMip;

				// Decrease depth based on mip level.
				if (depthMip > 1)
				{
					depthMip >>= 1;
				}
			}

			return data;
		}

		/// <summary>
		/// Function to create an array of System.Drawing.Images from an image data object.
		/// </summary>
		/// <param name="imageData">Image data to process.</param>
		/// <returns>A list of GDI+ images.</returns>
		public static Image[] CreateGDIImagesFromImageData(GorgonImageData imageData)
		{
			PixelFormat? format = GetPixelFormat(imageData.Settings.Format);
			Bitmap[] bitmaps = null;
			Image[] images;

			if (format == null)
			{
				format = GetPixelFormat(imageData.Settings.Format);

				if (format == null)
				{
					throw new GorgonException(GorgonResult.FormatNotSupported,
											  string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, imageData.Settings.Format));
				}
			}

			using (var wic = new GorgonWICImage())
			{
				try
				{
					bitmaps = wic.CreateWICBitmapsFromImageData(imageData);
					images = new Image[bitmaps.Length];

					for (int i = 0; i < bitmaps.Length; i++)
					{
						images[i] = wic.CreateGDIImageFromWICBitmap(bitmaps[i], format.Value);
					}
				}
				finally
				{
					// Clean up.
					if (bitmaps != null)
					{
						foreach (var bitmap in bitmaps)
						{
							bitmap.Dispose();
						}
					}
				}
			}

			return images;
		}

        /// <summary>
        /// Function to create an array of System.Drawing.Images from a given texture.
        /// </summary>
        /// <param name="texture">Texture to evaluate.</param>
        /// <returns>A list of GDI+ images.</returns>
        public static Image[] CreateGDIImagesFromTexture(GorgonTexture_OLDEN texture)
        {
			using (var data = GorgonImageData.CreateFromTexture(texture))
			{
				return CreateGDIImagesFromImageData(data);
            }
        }
    }

	// ReSharper restore ForCanBeConvertedToForeach
}
