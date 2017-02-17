#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 22, 2016 9:11:13 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Imaging
{
	/// <summary>
	/// Extension methods to provide method chaining for a fluent interface for the <see cref="IGorgonImage"/> type.
	/// </summary>
	public static class GorgonImageExtensions
	{
		/// <summary>
		/// Function to convert the pixel data in the buffers from B8G8R4A8 (or R8G8B8A8) to B4G4R4A4.
		/// </summary>
		/// <param name="dest">The destination buffer to receive the newly formatted data.</param>
		/// <param name="src">The source buffer to containing the source pixels to convert.</param>
		private static unsafe void ConvertPixelsToB4G4R4A4(IGorgonImageBuffer dest, IGorgonImageBuffer src)
		{
			var destBufferPtr = (ushort*)dest.Data.Address;
			var srcBufferPtr = (uint*)src.Data.Address;

			for (int i = 0; i < src.PitchInformation.SlicePitch; i += sizeof(uint))
			{
				uint srcPixel = *(srcBufferPtr++);
				uint b, g, r, a;

				if (src.Format == DXGI.Format.B8G8R8A8_UNorm)
				{
					b = ((srcPixel >> 24) & 0xff) >> 4;
					g = ((srcPixel >> 16) & 0xff) >> 4;
					r = ((srcPixel >> 8) & 0xff) >> 4;
					a = (srcPixel & 0xff) >> 4;
				}
				else // Convert from R8G8B8A8.
				{
					r = ((srcPixel >> 24) & 0xff) >> 4;
					g = ((srcPixel >> 16) & 0xff) >> 4;
					b = ((srcPixel >> 8) & 0xff) >> 4;
					a = (srcPixel & 0xff) >> 4;
				}

				*(destBufferPtr++) = (ushort)(b << 12 | g << 8 | r << 4 | a);
			}
		}

		/// <summary>
		/// Function to convert the pixel data in the buffers from B4G4R4A4 to B8G8R4A8 or R8G8B8A8.
		/// </summary>
		/// <param name="dest">The destination buffer to receive the newly formatted data.</param>
		/// <param name="src">The source buffer to containing the source pixels to convert.</param>
		private static unsafe void ConvertPixelsFromB4G4R4A4(IGorgonImageBuffer dest, IGorgonImageBuffer src)
		{
			ushort* srcBufferPtr = (ushort*)src.Data.Address;
			uint* destBufferPtr = (uint*)dest.Data.Address;

			for (int i = 0; i < src.PitchInformation.SlicePitch; i += sizeof(ushort))
			{
				ushort srcPixel = *(srcBufferPtr++);

				int b = ((srcPixel >> 12) & 0xf);
				int g = ((srcPixel >> 8) & 0xf);
				int r = ((srcPixel >> 4) & 0xf);
				int a = (srcPixel & 0xf);

				// Adjust the values to fill out a 32 bit integer: If r == 0xc in the 16 bit format, then r == 0xcc in the 32 bit format by taking the value and 
				// shifting it left by 4 bits and OR'ing the original r value again. ((0xc << 4 = 0xc0) OR 0xc = 0xcc).
				a = ((a << 4) | a);
				r = ((r << 4) | r);
				g = ((g << 4) | g);
				b = ((b << 4) | b);

				uint value = (uint)((dest.Format == DXGI.Format.B8G8R8A8_UNorm)
						            ? (b << 24 | g << 16 | r << 8 | a)
						            // Convert to R8G8B8A8 (flipped for little endian).
						            : (b << 24 | a << 16 | r << 8 | g));
				*(destBufferPtr++) = value;
			}
		}

		/// <summary>
		/// Function to convert the image from B4G4R4A4.
		/// </summary>
		/// <param name="baseImage">The base image to convert.</param>
		/// <param name="destFormat">The destination format.</param>
		/// <returns>The updated image.</returns>
		private static IGorgonImage ConvertFromB4G4R4A4(IGorgonImage baseImage, DXGI.Format destFormat)
		{
			// If we're converting to R8G8B8A8 or B8G8R8A8, then use those formats, otherwise, default to B8G8R8A8 as an intermediate buffer.
			DXGI.Format tempFormat = ((destFormat != DXGI.Format.B8G8R8A8_UNorm) && (destFormat != DXGI.Format.R8G8B8A8_UNorm)) ? DXGI.Format.B8G8R8A8_UNorm : destFormat;
			
			// Create an worker image in B8G8R8A8 format.
			IGorgonImageInfo destInfo = new GorgonImageInfo(baseImage.Info.ImageType, tempFormat)
				                        {
					                        Depth = baseImage.Info.Depth,
					                        Height = baseImage.Info.Height,
					                        Width = baseImage.Info.Width,
					                        ArrayCount = baseImage.Info.ArrayCount,
					                        MipCount = baseImage.Info.MipCount
				                        };

			// Our destination image for B8G8R8A8 or R8G8B8A8.
			var destImage = new GorgonImage(destInfo);

			try
			{
				// We have to manually upsample from R4G4B4A4 to B8R8G8A8.
				// Because we're doing this manually, dithering won't be an option unless 
				for (int array = 0; array < baseImage.Info.ArrayCount; ++array)
				{
					for (int mip = 0; mip < baseImage.Info.MipCount; ++mip)
					{
						int depthCount = baseImage.GetDepthCount(mip);

						for (int depth = 0; depth < depthCount; depth++)
						{
							IGorgonImageBuffer destBuffer = destImage.Buffers[mip, baseImage.Info.ImageType == ImageType.Image3D ? depth : array];
							IGorgonImageBuffer srcBuffer = baseImage.Buffers[mip, baseImage.Info.ImageType == ImageType.Image3D ? depth : array];

							ConvertPixelsFromB4G4R4A4(destBuffer, srcBuffer);
						}
					}
				}

				// If the destination format is not R8G8B8A8 or B8G8R8A8, then we need to do more conversion.
				if (destFormat != destImage.Info.Format)
				{
					ConvertToFormat(destImage, destFormat);
				}

				// Update the base image with our worker image.
				baseImage.CopyFrom(destImage);

				return baseImage;
			}
			finally
			{
				destImage.Dispose();
			}
		}

		/// <summary>
		/// Function to convert the image to B4G4R4A4.
		/// </summary>
		/// <param name="baseImage">The base image to convert.</param>
		/// <param name="dithering">Dithering to apply to the converstion to B8G8R8A8.</param>
		/// <returns>The updated image.</returns>
		private static IGorgonImage ConvertToB4G4R4A4(IGorgonImage baseImage, ImageDithering dithering)
		{
			// This temporary image will be used to convert to B8G8R8A8.
			IGorgonImage newImage = baseImage;

			IGorgonImageInfo destInfo = new GorgonImageInfo(baseImage.Info.ImageType, DXGI.Format.B4G4R4A4_UNorm)
			{
				Depth = baseImage.Info.Depth,
				Height = baseImage.Info.Height,
				Width = baseImage.Info.Width,
				ArrayCount = baseImage.Info.ArrayCount,
				MipCount = baseImage.Info.MipCount
			};

			// This is our working buffer for B4G4R4A4.
			IGorgonImage destImage = new GorgonImage(destInfo);

			try
			{
				// If necessary, convert to B8G8R8A8. Otherwise, we'll just downsample directly.
				if ((newImage.Info.Format != DXGI.Format.B8G8R8A8_UNorm)
					&& (newImage.Info.Format != DXGI.Format.R8G8B8A8_UNorm))
				{
					newImage = baseImage.Clone();
					ConvertToFormat(newImage, DXGI.Format.B8G8R8A8_UNorm, dithering);
				}

				// The next step is to manually downsample to R4G4B4A4.
				// Because we're doing this manually, dithering won't be an option unless unless we've downsampled from a much higher bit format when converting to B8G8R8A8.
				for (int array = 0; array < newImage.Info.ArrayCount; ++array)
				{
					for (int mip = 0; mip < newImage.Info.MipCount; ++mip)
					{
						int depthCount = newImage.GetDepthCount(mip);

						for (int depth = 0; depth < depthCount; depth++)
						{
							IGorgonImageBuffer destBuffer = destImage.Buffers[mip, destInfo.ImageType == ImageType.Image3D ? depth : array];
							IGorgonImageBuffer srcBuffer = newImage.Buffers[mip, newImage.Info.ImageType == ImageType.Image3D ? depth : array];

							ConvertPixelsToB4G4R4A4(destBuffer, srcBuffer);
						}
					}
				}

				baseImage.CopyFrom(destImage);

				return baseImage;
			}
			finally
			{
				destImage.Dispose();

				if (newImage != baseImage)
				{
					newImage.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to generate a new mip map chain.
		/// </summary>
		/// <param name="baseImage">The image which will have its mip map chain updated.</param>
		/// <param name="mipCount">The number of mip map levels.</param>
		/// <param name="filter">[Optional] The filter to apply when copying the data from one mip level to another.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the updated mip map data.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This method will generate a new mip map chain for the <paramref name="mipCount"/>. If the current number of mip maps is not the same as the requested number, then the image buffer will be 
		/// adjusted to use the requested number of mip maps. If 0 is passed to <paramref name="mipCount"/>, then a full mip map chain is generated.
		/// </para>
		/// <para>
		/// Note that the <paramref name="mipCount"/> may not be honored depending on the current width, height, and depth of the image. Check the <see cref="IGorgonImage.Info"/> property on the returned 
		/// <see cref="IGorgonImage"/> to determine how many mip levels were actually generated.
		/// </para>
		/// </remarks>
		public static IGorgonImage GenerateMipMaps(this IGorgonImage baseImage, int mipCount, ImageFilter filter = ImageFilter.Point)
		{
			if (baseImage == null)
			{
				throw new ArgumentNullException(nameof(baseImage));
			}

			int maxMips = GorgonImage.CalculateMaxMipCount(baseImage.Info);

			// If we specify 0, then generate a full chain.
			if ((mipCount <= 0) || (mipCount > maxMips))
			{
				mipCount = maxMips;
			}

			GorgonImageInfo destSettings = new GorgonImageInfo(baseImage.Info)
			                               {
				                               MipCount = mipCount
			                               };

			var newImage = new GorgonImage(destSettings);
			var wic = new WicUtilities();

			try
			{
				// Copy the top mip level from the source image to the dest image.
				for (int array = 0; array < baseImage.Info.ArrayCount; ++array)
				{
					newImage.Buffers[0, array].Data.CopyFrom(baseImage.Buffers[0, array].Data, 
						(int)baseImage.Buffers[0, array].Data.Size * baseImage.Info.Depth);
				}

				if (mipCount < 2)
				{
					baseImage.CopyFrom(newImage);
					return baseImage;
				}

				wic.GenerateMipImages(newImage, filter);

				baseImage.CopyFrom(newImage);
				return baseImage;
			}
			finally
			{
				newImage.Dispose();
				wic.Dispose();
			}
		}

		/// <summary>
		/// Function to crop the image to the rectangle passed to the parameters.
		/// </summary>
		/// <param name="baseImage">The image to resize.</param>
		/// <param name="cropRect">The rectangle that will be used to crop the image.</param>
		/// <param name="newDepth">The new depth for the image (for <see cref="ImageType.Image3D"/> images).</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the resized image.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="newDepth"/> parameter is less than 1.</exception>
		/// <remarks>
		/// <para>
		/// This method will crop the existing image a smaller version of itself as a new <see cref="IGorgonImage"/>. If the sizes are the same, or the <paramref name="cropRect"/> is larger than the size 
		/// of the <paramref name="baseImage"/>, then no changes will be made.
		/// </para>
		/// </remarks>
		public static IGorgonImage Crop(this IGorgonImage baseImage, DX.Rectangle cropRect, int newDepth)
		{
			if (baseImage == null)
			{
				throw new ArgumentNullException(nameof(baseImage));
			}

			if ((newDepth < 1) && (baseImage.Info.ImageType == ImageType.Image3D))
			{
				throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(newDepth));
			}

			// Only use the appropriate dimensions.
			switch (baseImage.Info.ImageType)
			{
				case ImageType.Image1D:
					cropRect.Height = baseImage.Info.Height;
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					newDepth = baseImage.Info.Depth;
					break;
			}

			// If the intersection of the crop rectangle and the source buffer are the same (and the depth is the same), then we don't need to crop.
			var bufferRect = new DX.Rectangle(0, 0, baseImage.Info.Width, baseImage.Info.Height);
			var clipRect = DX.Rectangle.Intersect(cropRect, bufferRect);

			if ((bufferRect.Equals(ref clipRect)) && (newDepth == baseImage.Info.Depth))
			{
				return baseImage;
			}

			var wic = new WicUtilities();

			IGorgonImage newImage = null;

			try
			{
				int calcMipLevels = GorgonImage.CalculateMaxMipCount(cropRect.Width, cropRect.Height, newDepth).Min(baseImage.Info.MipCount);
				newImage = wic.Resize(baseImage, cropRect.X, cropRect.Y, cropRect.Width, cropRect.Height, newDepth, calcMipLevels, ImageFilter.Point, true);

				// Send the data over to the new image.
				baseImage.CopyFrom(newImage);

				return baseImage;
			}
			finally
			{
				newImage?.Dispose();
				wic.Dispose();
			}
		}

		/// <summary>
		/// Function to resize the image to a new width, height and/or depth.
		/// </summary>
		/// <param name="baseImage">The image to resize.</param>
		/// <param name="newWidth">The new width for the image.</param>
		/// <param name="newHeight">The new height for the image (for <see cref="ImageType.Image2D"/> and <see cref="ImageType.ImageCube"/> images).</param>
		/// <param name="newDepth">The new depth for the image (for <see cref="ImageType.Image3D"/> images).</param>
		/// <param name="filter">[Optional] The type of filtering to apply to the scaled image to help smooth larger and smaller images.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the resized image.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="newWidth"/>, <paramref name="newHeight"/>, or <paramref name="newDepth"/> parameters are less than 1.</exception>
		/// <exception cref="GorgonException">Thrown if there was an error during resizing regarding image pixel format conversion due to the type of <paramref name="filter"/> applied.</exception>
		/// <remarks>
		/// <para>
		/// This method will change the size of an existing image and return a larger or smaller version of itself as a new <see cref="IGorgonImage"/>. 
		/// </para>
		/// </remarks>
		public static IGorgonImage Resize(this IGorgonImage baseImage, int newWidth, int newHeight, int newDepth, ImageFilter filter = ImageFilter.Point)
		{
			if (baseImage == null)
			{
				throw new ArgumentNullException(nameof(baseImage));
			}

			if (newWidth < 1)
			{
				throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_WIDTH_TOO_SMALL, nameof(newWidth));
			}

			if ((newHeight < 1) && ((baseImage.Info.ImageType == ImageType.Image2D) || (baseImage.Info.ImageType == ImageType.ImageCube)))
			{
				throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_HEIGHT_TOO_SMALL, nameof(newHeight));
			}

			if ((newDepth < 1) && (baseImage.Info.ImageType == ImageType.Image3D))
			{
				throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(newDepth));
			}

			// Only use the appropriate dimensions.
			switch (baseImage.Info.ImageType)
			{
				case ImageType.Image1D:
					newHeight = baseImage.Info.Height;
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					newDepth = baseImage.Info.Depth;
					break;
			}

			// If we haven't actually changed the size, then skip out.
			if ((newWidth == baseImage.Info.Width) && (newHeight == baseImage.Info.Height) && (newDepth == baseImage.Info.Depth))
			{
				return baseImage;
			}

			var wic = new WicUtilities();

			IGorgonImage newImage = null;
			try
			{
				int calcMipLevels = GorgonImage.CalculateMaxMipCount(newWidth, newHeight, newDepth).Min(baseImage.Info.MipCount);
				newImage = wic.Resize(baseImage, 0, 0, newWidth, newHeight, newDepth, calcMipLevels, filter, false);

				baseImage.CopyFrom(newImage);

				return baseImage;
			}
			finally
			{
				newImage?.Dispose();
				wic.Dispose();				
			}
		}

		/// <summary>
		/// Function to convert the pixel format of an image into another pixel format.
		/// </summary>
		/// <param name="baseImage">The image to convert.</param>
		/// <param name="format">The new pixel format for the image.</param>
		/// <param name="dithering">[Optional] Flag to indicate the type of dithering to perform when the bit depth for the <paramref name="format"/> is lower than the original bit depth.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the image data with the converted pixel format.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is set to <c>Format.Unknown</c>.
		/// <para>-or-</para>
		/// <para>Thrown when the original format could not be converted into the desired <paramref name="format"/>.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// Use this to convert an image format from one to another. The conversion functionality uses Windows Imaging Components (WIC) to perform the conversion.
		/// </para>
		/// <para>
		/// Because this method uses WIC, not all formats will be convertible. To determine if a format can be converted, use the <see cref="GorgonImage.CanConvertToFormat"/> method. 
		/// </para>
		/// <para>
		/// For the <c>B4G4R4A4_UNorm</c> format, Gorgon has to perform a manual conversion since that format is not supported by WIC. Because of this, the <paramref name="dithering"/> flag will be ignored when 
		/// downsampling to that format.
		/// </para>
		/// </remarks>
		public static IGorgonImage ConvertToFormat(this IGorgonImage baseImage, DXGI.Format format, ImageDithering dithering = ImageDithering.None)
		{
			if (baseImage == null)
			{
				throw new ArgumentNullException(nameof(baseImage));
			}

			if ((format == DXGI.Format.Unknown) || (!baseImage.CanConvertToFormat(format)))
			{
				throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
			}

			if (format == baseImage.Info.Format)
			{
				return baseImage;
			}

			// If we've asked for 4 bit per channel BGRA, then we have to convert the base image to B8R8G8A8,and then convert manually (no support in WIC).
			if (format == DXGI.Format.B4G4R4A4_UNorm)
			{
				return ConvertToB4G4R4A4(baseImage, dithering);
			}

			// If we're currently using B4G4R4A4, then manually convert (no support in WIC).
			if (baseImage.Info.Format == DXGI.Format.B4G4R4A4_UNorm)
			{
				return ConvertFromB4G4R4A4(baseImage, format);
			}

			var destInfo = new GorgonFormatInfo(format);
			WicUtilities wic = null;

			IGorgonImage newImage = null;

			try
			{
				wic = new WicUtilities();
				newImage = wic.ConvertToFormat(baseImage, format, dithering, baseImage.FormatInfo.IsSRgb, destInfo.IsSRgb);

				baseImage.CopyFrom(newImage);

				return baseImage;
			}
			finally
			{
				newImage?.Dispose();
				wic?.Dispose();
			}
		}

		/// <summary>
		/// Function to convert the image data into a premultiplied format.
		/// </summary>
		/// <param name="baseImage">The image to convert.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the image data with the premultiplied alpha pixel data.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the original format could not be converted to <c>R8G8B8A8_UNorm</c>.</exception>
		/// <remarks>
		/// <para>
		/// Use this to convert an image to a premultiplied format. This takes each Red, Green and Blue element and multiplies them by the Alpha element.
		/// </para>
		/// <para>
		/// Because this method will only operate on <c>R8G8B8A8_UNorm</c> formattted image data, the image will be converted to that format and converted back to its original format after the alpha is 
		/// premultiplied. This may cause color fidelity issues. If the image cannot be converted, then an exception will be thrown. 
		/// </para>
		/// </remarks>
		public static IGorgonImage ConvertToPremultipliedAlpha(this IGorgonImage baseImage)
		{
			IGorgonImage newImage = null;

			if (baseImage == null)
			{
				throw new ArgumentNullException(nameof(baseImage));
			}

			try
			{
				// Worker image.
				var cloneImageInfo = new GorgonImageInfo(baseImage.Info)
				                     {
					                     HasPremultipliedAlpha = true
				                     };
				newImage = new GorgonImage(cloneImageInfo);
				newImage.CopyFrom(baseImage);			

				if (newImage.Info.Format != DXGI.Format.R8G8B8A8_UNorm)
				{
					if (!newImage.CanConvertToFormat(DXGI.Format.R8G8B8A8_UNorm))
					{
						throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, DXGI.Format.R8G8B8A8_UNorm), nameof(baseImage));
					}

					// Clone the image so we can convert it to the correct format.
					newImage.ConvertToFormat(DXGI.Format.R8G8B8A8_UNorm);
				}

				unsafe
				{
					int* imagePtr = (int *)(newImage.ImageData.Address);

					for (int i = 0; i < newImage.SizeInBytes; i += newImage.FormatInfo.SizeInBytes)
					{
						var color = GorgonColor.FromABGR(*imagePtr);
						color = new GorgonColor(color.Red * color.Alpha, color.Green * color.Alpha, color.Blue * color.Alpha, color.Alpha);
						*(imagePtr++) = color.ToABGR();
					}
				}

				if (newImage.Info.Format != baseImage.Info.Format)
				{
					newImage.ConvertToFormat(baseImage.Info.Format);
				}
				
				baseImage.CopyFrom(newImage);

				return baseImage;
			}
			finally
			{
				newImage?.Dispose();
			}
		}
	}
}
