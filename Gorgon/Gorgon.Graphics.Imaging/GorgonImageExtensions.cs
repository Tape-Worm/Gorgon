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
		/// Function to generate a new mip map chain.
		/// </summary>
		/// <param name="baseImage">The image which will have its mip map chain updated.</param>
		/// <param name="mipCount">The number of mip map levels.</param>
		/// <param name="filter">[Optional] The filter to apply when copying the data from one mip level to another.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the updated mip map data.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> parameter is <b>NULL</b>.</exception>
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
					newImage.CopyTo(baseImage);
					return baseImage;
				}

				wic.GenerateMipImages(newImage, filter);

				newImage.CopyTo(baseImage);
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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> parameter is <b>NULL</b>.</exception>
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
				newImage.CopyTo(baseImage);

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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> parameter is <b>NULL</b>.</exception>
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

				newImage.CopyTo(baseImage);

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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> is <b>NULL</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is set to <c>Format.Unknown</c>.
		/// <para>-or-</para>
		/// <para>Thrown when the original format could not be converted into the desired <paramref name="format"/>.</para>
		/// </exception>
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

			var destInfo = new GorgonFormatInfo(format);
			WicUtilities wic = null;

			IGorgonImage newImage = null;

			try
			{
				wic = new WicUtilities();
				newImage = wic.ConvertToFormat(baseImage, format, dithering, baseImage.FormatInfo.IsSRgb, destInfo.IsSRgb);

				newImage.CopyTo(baseImage);

				return baseImage;
			}
			finally
			{
				newImage?.Dispose();
				wic?.Dispose();
			}
		}
	}
}
