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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using WIC = SharpDX.WIC;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// Options for the GDI+ image texture import.
	/// </summary>
	/// <remarks>Use this to override the default beahvior of the FromGDI methods.</remarks>
	public class GorgonGDIOptions
	{
		#region Properties.
		/// <summary>
		/// Property to set or return a target width for the image.
		/// </summary>
		/// <remarks>
		/// This will resize the image width to the size specified.  If left at 0, then the default width from the GDI+ image will be used.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return a target height for the image.
		/// </summary>
		/// <remarks>
		/// This will resize the image height to the size specified.  If left at 0, then the default height from the GDI+ image will be used.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth for the resulting texture.
		/// </summary>
		/// <remarks>
		/// Setting this value will use an array of images as each depth slice in the volume texture.  Ensure that the array has the required number 
		/// of elements for each slice in each mip map level.  Each slice decreases by a power of 2 as the mip level increases.
		/// <para>Slices are arranged in the array as follows:</para>
        /// <code>
		/// Element[0]: Slice 0, Mip level 0
		/// Element[1]: Slice 1, Mip level 0
		/// Element[2]: Slice 2, Mip level 0
		/// Element[3]: Slice 3, Mip level 0
		/// Element[4]: Slice 4, Mip level 1
		/// Element[5]: Slice 5, Mip level 1
        /// </code>
		/// <para>Note that we only have 2 slices in the last 2 elements in the array, this is because the 2nd mip level has decreased the depth by a power of 2.</para>
		/// <para>This property is for 3D volume textures only, and is ignored on all other texture types.</para></remarks>
		public int Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format to convert the image into.
		/// </summary>
		/// <remarks>
		/// Set this property to Unknown to convert the image to the closest matching format.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip-map levels in the image.
		/// </summary>
		/// <remarks>Setting this value will create the requested number of mip-maps from the image.  If passing in an array of GDI+ images, then the 
		/// array will be used as the source for mip-maps.  The array should be formatted like this:
        /// <code>
		/// Element[0]: Mip level 0
		/// Element[1]: Mip level 1
		/// Element[2]: Mip level 2
		/// Element[3]: Mip level 3
		/// etc...
        /// </code>
		/// <para>If an array of images is used, then ensure that there are enough elements in the array to accomodate the requested number of mip maps.</para>
		/// <para>If this value is set to 0 and no image array is present, then Gorgon will generate a full mip-map chain.</para>
		/// <para>The default value is 1.</para>
		/// </remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of array indices in the image.
		/// </summary>
		/// <remarks>Setting this value will create array images from an array of GDI+ images.  The GDI+ Image array should be formatted like this: 
        /// <code>
		/// Element[0]: Array Index 0, Mip level 0
		/// Element[1]: Array Index 0, Mip level 1
		/// Element[2]: Array Index 1, Mip level 0
		/// Element[3]: Array Index 1, Mip level 1
        /// </code>
		/// <para>Note that each array should have the same number of mip levels.</para>
		/// <para>Arrays only apply to 1D and 2D images, 3D images will ignore this parameter.</para>
		/// <para>The default value is 1.</para>
		/// </remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the dithering type to use on images with lower bit depths.
		/// </summary>
		/// <remarks>The default value is None.</remarks>
		public ImageDithering Dither
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the filtering type to use on images that are resized.
		/// </summary>
		/// <remarks>This value has no effect if the size of the image is not changed.  The value also affects the mip-map chain if it is generated.
		/// <para>The default value is Point.</para>
		/// </remarks>
		public ImageFilter Filter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage flag for the texture generated from the GDI+ image(s).
		/// </summary>
		/// <remarks>The default value is Default.</remarks>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to use clipping on the image if it needs resizing.
		/// </summary>
		/// <remarks>This value will turn off scaling and instead crop the GDI+ image if it's too large to fit in the destination image.  If the image is smaller, then 
		/// it will be left at its current size.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool UseClipping
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGDIOptions" /> class.
		/// </summary>
		public GorgonGDIOptions()
		{
			Width = 0;
			Height = 0;
			Depth = 1;
			Format = BufferFormat.Unknown;
			MipCount = 1;
			ArrayCount = 1;
			Dither = ImageDithering.None;
			Filter = ImageFilter.Point;
			Usage = BufferUsage.Default;
			UseClipping = false;
		}
		#endregion
	}

	/// <summary>
	/// A utility class to convert a GDI+ image object into a Gorgon 2D image object.
	/// </summary>
	static class GorgonGDIImageConverter
	{
		// Formats that can be converted.
		private static Tuple<PixelFormat, BufferFormat>[] _formatConversion = new[] 
		{
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format64bppArgb, BufferFormat.R16G16B16A16_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format32bppArgb, BufferFormat.R8G8B8A8_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppGrayScale, BufferFormat.R16_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppArgb1555, BufferFormat.B5G5R5A1_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppRgb565, BufferFormat.B5G6R5_UIntNormal),
		};

		// Best fit format mapping.
		private static Tuple<PixelFormat, PixelFormat>[] _bestFit = new[]
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
            BufferFormat result = BufferFormat.Unknown;

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
                case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
                case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
                case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
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
			GorgonImageData data = null;

			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(image.PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new ArgumentException("The pixel format '" + image.PixelFormat.ToString() + "' is not supported.", "images");
			}

			if (options.Width < 1)
			{
				options.Width = image.Width;
			}

			if (options.MipCount < 1)
			{
				options.MipCount = GorgonImageData.GetMaxMipCount(options.Width);
			}
			else
			{
				options.MipCount = options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width));
			}

			// Create our settings.
			GorgonTexture1DSettings settings = new GorgonTexture1DSettings()
			{
				Width = options.Width,
				MipCount = options.MipCount,
				ArrayCount = 1,
				Format = options.Format
			};

			// Create our image data.
			data = new GorgonImageData(settings);
			
			// Using the image, convert to a WIC bitmap object.
			using (WIC.Bitmap bitmap = wic.CreateWICImageFromImage(image))
			{
				for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
				{
					var buffer = data[mipLevel];

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
			GorgonImageData data = null;
            
			if (options.Format == BufferFormat.Unknown)
			{
                options.Format = GetBufferFormat(images[0].PixelFormat);
			}

            if (options.Format == BufferFormat.Unknown)
			{
				throw new ArgumentException("The pixel format '" + images[0].PixelFormat.ToString() + "' is not supported.", "images");
			}

			if (images.Any(item => item == null))
			{
				throw new ArgumentException("The image array must contain non-NULL images.", "images");
			}

			if (images.Any(item => item.PixelFormat != images[0].PixelFormat))
			{
				throw new ArgumentException("The pixel format for all elements must be '" + images[0].PixelFormat.ToString() + "'.", "images");
			}

			if (options.Width < 1)
			{
				options.Width = images[0].Width;
			}

			if (options.ArrayCount < 1)
			{
				options.ArrayCount = 1;
			}

			if (options.MipCount < 1)
			{
				options.MipCount = 1;
			}
			else
			{
				options.MipCount = options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width));
			}

			// Create our settings.
			GorgonTexture1DSettings settings = new GorgonTexture1DSettings()
			{
				Width = options.Width,
				MipCount = options.MipCount,
				ArrayCount = options.ArrayCount,
				Format = options.Format
			};
			
			if ((options.ArrayCount * options.MipCount) > images.Count)
			{
				throw new ArgumentOutOfRangeException("images", "The mip level count and the array count exceed the length of the array.");
			}

			// Create our image data.
			data = new GorgonImageData(settings);

			for (int array = 0; array < data.Settings.ArrayCount; array++)
			{
				for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
				{
					var image = images[array * data.Settings.MipCount + mipLevel];

					// Using the image, convert to a WIC bitmap object.
					using (var bitmap = wic.CreateWICImageFromImage(image))
					{
						var buffer = data[array, mipLevel];

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
            GorgonImageData data = null;

			if (image == null)
			{
				throw new ArgumentNullException("image");
			}

			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(image.PixelFormat);
			}
			
			if (options.Format == BufferFormat.Unknown)
            {
				throw new ArgumentException("The pixel format '" + image.PixelFormat.ToString() + "' is not supported.", "images");
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
			if (options.MipCount < 1)
			{
				options.MipCount = GorgonImageData.GetMaxMipCount(options.Width, options.Height);
			}
			else
			{
				options.MipCount = options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width, options.Height));
			}
			
			// Create our settings.
			GorgonTexture2DSettings settings = new GorgonTexture2DSettings()
			{
				Width = options.Width,
				Height = options.Height,
				MipCount = options.MipCount,
				ArrayCount = 1,
				Format = options.Format
			};

            // Create our image data.
            data = new GorgonImageData(settings);
			
			// Using the image, convert to a WIC bitmap object.
			using (WIC.Bitmap bitmap = wic.CreateWICImageFromImage(image))
			{
				for (int mipLevel = 0; mipLevel < options.MipCount; mipLevel++)
				{
					var buffer = data[mipLevel];
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
			GorgonImageData data = null;

			if ((images == null) || (images.Count == 0))
			{
				throw new ArgumentException("The parameter must not be NULL or empty.", "images");
			}

			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(images[0].PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new ArgumentException("The pixel format '" + images[0].PixelFormat.ToString() + "' is not supported.", "images");
			}

			if (images.Any(item => item == null))
			{
				throw new ArgumentException("The image array must contain non-NULL images.", "images");
			}

			if (images.Any(item => item.PixelFormat != images[0].PixelFormat))
			{
				throw new ArgumentException("The pixel format for all elements must be '" + images[0].PixelFormat.ToString() + "'.", "images");
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

			if (options.MipCount < 1)
			{
				options.MipCount = 1;
			}
			else
			{
				options.MipCount = options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width, options.Height));
			}

			// Create our settings.
			GorgonTexture2DSettings settings = new GorgonTexture2DSettings()
			{
				Width = options.Width,
				Height = options.Height,
				MipCount = options.MipCount,
				ArrayCount = options.ArrayCount,
				Format = options.Format
			};

			if ((options.ArrayCount * options.MipCount) > images.Count)
			{
				throw new ArgumentOutOfRangeException("images", "The mip level count and the array count exceed the length of the array.");
			}

			// Create our image data.
			data = new GorgonImageData(settings);
			
			for (int array = 0; array < data.Settings.ArrayCount; array++)
			{
				for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
				{
					var image = images[array * data.Settings.MipCount + mipLevel];

					// Using the image, convert to a WIC bitmap object.
					using (var bitmap = wic.CreateWICImageFromImage(image))
					{
						var buffer = data[array, mipLevel];

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
			GorgonImageData data = null;

			if ((images == null) || (images.Count == 0))
			{
				throw new ArgumentException("The parameter must not be NULL or empty.", "images");
			}

			if (options.Format == BufferFormat.Unknown)
			{
				options.Format = GetBufferFormat(images[0].PixelFormat);
			}

			if (options.Format == BufferFormat.Unknown)
			{
				throw new ArgumentException("The pixel format '" + images[0].PixelFormat.ToString() + "' is not supported.", "images");
			}

			if (images.Any(item => item == null))
			{
				throw new ArgumentException("The image array must contain non-NULL images.", "images");
			}

			if (images.Any(item => item.PixelFormat != images[0].PixelFormat))
			{
				throw new ArgumentException("The pixel format for all elements must be '" + images[0].PixelFormat.ToString() + "'.", "images");
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

			if (options.MipCount < 1)
			{
				options.MipCount = 1;
			}
			else
			{
				options.MipCount = options.MipCount.Min(GorgonImageData.GetMaxMipCount(options.Width, options.Height, options.Depth));
			}

			// Set the depth to the number of images if there are no mip-maps.
			if ((images.Count > 1) && (options.MipCount == 1))
			{
				options.Depth = images.Count;
			}

			// Create our settings.
			GorgonTexture3DSettings settings = new GorgonTexture3DSettings()
			{
				Width = options.Width,
				Height = options.Height,
				Depth = options.Depth,
				MipCount = options.MipCount,
				Format = options.Format
			};

			// Only volume textures that are size to a power of 2 can have mip maps.
			if ((!settings.IsPowerOfTwo) && (options.MipCount > 1))
			{
				throw new ArgumentException("Cannot create a volume texture mip chain unless the dimensions are powers of 2.", "mipCount");
			}

			// Create our image data.
			data = new GorgonImageData(settings);

			int depthMip = options.Depth;
			int imageIndex = 0;
			for (int mipLevel = 0; mipLevel < data.Settings.MipCount; mipLevel++)
			{
				if (imageIndex >= images.Count)
				{
					data.Dispose();
					throw new ArgumentOutOfRangeException("images", "The mip level count and the depth slice count exceed the length of the array.");
				}

				for (int depth = 0; depth < depthMip; depth++)
				{
					var image = images[imageIndex + depth];

					// Using the image, convert to a WIC bitmap object.
					using (var bitmap = wic.CreateWICImageFromImage(image))
					{
						var buffer = data[0, mipLevel, depth];
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


		/*using (var stream = System.IO.File.Open(@"D:\unpak\myimage.gif", System.IO.FileMode.Create))
		{
			using (WIC.GifBitmapEncoder encoder = new WIC.GifBitmapEncoder(wic.Factory, WIC.ContainerFormatGuids.Gif,stream))
			{
				using (WIC.BitmapFrameEncode frame = new WIC.BitmapFrameEncode(encoder))
				{
					frame.Initialize();
					frame.SetSize(settings.Width, settings.Height);
					frame.SetResolution(currentImage.HorizontalResolution, currentImage.VerticalResolution);
					Guid bmpFormat = bitmap.PixelFormat;
					frame.SetPixelFormat(ref bmpFormat);
																				
					if (bmpFormat != bitmap.PixelFormat)
					{
						using (var convert = new WIC.FormatConverter(wic.Factory))
						{
							using (WIC.Palette palette = new WIC.Palette(wic.Factory))
							{													
								palette.Initialize(bitmap, 256, true);
								convert.Initialize(bitmap, bmpFormat, WIC.BitmapDitherType.ErrorDiffusion, palette, 63.75, WIC.BitmapPaletteType.Custom);

								int rowPitch = (32 * currentImage.Width + 7) / 8;
								int size = rowPitch * currentImage.Height;

								using (var converterstream = new GorgonLibrary.IO.GorgonDataStream(size))
								{
									convert.CopyPixels(rowPitch, converterstream.BasePointer, size);
									frame.Palette = palette;
									frame.WritePixels(currentImage.Height, converterstream.BasePointer, rowPitch, size);
								}
							}
						}
					}
					else
						frame.WriteSource(bitmap);
					frame.Commit();
					encoder.Commit();
				}
			}*/

    }
}
