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
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Graphics.Imaging
{
    /// <summary>
    /// An anchor for repositioning the image after the image has been expanded.
    /// </summary>
    public enum ImageExpandAnchor
    {
        /// <summary>
        /// Image is in the upper left corner.
        /// </summary>
        UpperLeft = Alignment.UpperLeft,
        /// <summary>
        /// Image is in the center at the top.
        /// </summary>
        UpperMiddle = Alignment.UpperCenter,
        /// <summary>
        /// Image is in the upper right corner.
        /// </summary>
        UpperRight = Alignment.UpperRight,
        /// <summary>
        /// Image is in the middle and to the left.
        /// </summary>
        MiddleLeft = Alignment.CenterLeft,
        /// <summary>
        /// Image is centered.
        /// </summary>
        Center = Alignment.Center,
        /// <summary>
        /// Image is in the middle and to the right.
        /// </summary>
        MiddleRight = Alignment.CenterRight,
        /// <summary>
        /// Image is in the bottom left corner.
        /// </summary>
        BottomLeft = Alignment.LowerLeft,
        /// <summary>
        /// Image is in the center at the bottom.
        /// </summary>
        BottomMiddle = Alignment.LowerCenter,
        /// <summary>
        /// Image is in the bottom right corner.
        /// </summary>
        BottomRight = Alignment.LowerRight
    }

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
            ushort* destBufferPtr = (ushort*)dest.Data;
            uint* srcBufferPtr = (uint*)src.Data;

            for (int i = 0; i < src.PitchInformation.SlicePitch; i += sizeof(uint))
            {
                uint srcPixel = *(srcBufferPtr++);
                uint b, g, r, a;

                if (src.Format == BufferFormat.B8G8R8A8_UNorm)
                {
                    a = ((srcPixel >> 24) & 0xff) >> 4;
                    r = ((srcPixel >> 16) & 0xff) >> 4;
                    g = ((srcPixel >> 8) & 0xff) >> 4;
                    b = (srcPixel & 0xff) >> 4;
                }
                else // Convert from R8G8B8A8.
                {
                    a = ((srcPixel >> 24) & 0xff) >> 4;
                    b = ((srcPixel >> 16) & 0xff) >> 4;
                    g = ((srcPixel >> 8) & 0xff) >> 4;
                    r = (srcPixel & 0xff) >> 4;
                }

                *(destBufferPtr++) = (ushort)((a << 12) | (r << 8) | (g << 4) | b);
            }
        }

        /// <summary>
        /// Function to convert the pixel data in the buffers from B4G4R4A4 to B8G8R4A8 or R8G8B8A8.
        /// </summary>
        /// <param name="dest">The destination buffer to receive the newly formatted data.</param>
        /// <param name="src">The source buffer to containing the source pixels to convert.</param>
        private static unsafe void ConvertPixelsFromB4G4R4A4(IGorgonImageBuffer dest, IGorgonImageBuffer src)
        {
            ushort* srcBufferPtr = (ushort*)src.Data;
            uint* destBufferPtr = (uint*)dest.Data;

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

                uint value = (uint)((dest.Format == BufferFormat.B8G8R8A8_UNorm)
                                    ? ((b << 24) | (g << 16) | (r << 8) | a)
                                    // Convert to R8G8B8A8 (flipped for little endian).
                                    : ((b << 24) | (a << 16) | (r << 8) | g));
                *(destBufferPtr++) = value;
            }
        }

        /// <summary>
        /// Function to convert the image from B4G4R4A4.
        /// </summary>
        /// <param name="baseImage">The base image to convert.</param>
        /// <param name="destFormat">The destination format.</param>
        /// <returns>The updated image.</returns>
        private static IGorgonImage ConvertFromB4G4R4A4(IGorgonImage baseImage, BufferFormat destFormat)
        {
            // If we're converting to R8G8B8A8 or B8G8R8A8, then use those formats, otherwise, default to B8G8R8A8 as an intermediate buffer.
            BufferFormat tempFormat = ((destFormat != BufferFormat.B8G8R8A8_UNorm) && (destFormat != BufferFormat.R8G8B8A8_UNorm)) ? BufferFormat.B8G8R8A8_UNorm : destFormat;

            // Create an worker image in B8G8R8A8 format.
            IGorgonImageInfo destInfo = new GorgonImageInfo(baseImage.ImageType, tempFormat)
            {
                Depth = baseImage.Depth,
                Height = baseImage.Height,
                Width = baseImage.Width,
                ArrayCount = baseImage.ArrayCount,
                MipCount = baseImage.MipCount
            };

            // Our destination image for B8G8R8A8 or R8G8B8A8.
            var destImage = new GorgonImage(destInfo);

            try
            {
                // We have to manually upsample from R4G4B4A4 to B8R8G8A8.
                // Because we're doing this manually, dithering won't be an option unless 
                for (int array = 0; array < baseImage.ArrayCount; ++array)
                {
                    for (int mip = 0; mip < baseImage.MipCount; ++mip)
                    {
                        int depthCount = baseImage.GetDepthCount(mip);

                        for (int depth = 0; depth < depthCount; depth++)
                        {
                            IGorgonImageBuffer destBuffer = destImage.Buffers[mip, baseImage.ImageType == ImageType.Image3D ? depth : array];
                            IGorgonImageBuffer srcBuffer = baseImage.Buffers[mip, baseImage.ImageType == ImageType.Image3D ? depth : array];

                            ConvertPixelsFromB4G4R4A4(destBuffer, srcBuffer);
                        }
                    }
                }

                // If the destination format is not R8G8B8A8 or B8G8R8A8, then we need to do more conversion.
                if (destFormat != destImage.Format)
                {
                    ConvertToFormat(destImage, destFormat);
                }

                // Update the base image with our worker image.
                destImage.CopyTo(baseImage);

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

            IGorgonImageInfo destInfo = new GorgonImageInfo(baseImage.ImageType, BufferFormat.B4G4R4A4_UNorm)
            {
                Depth = baseImage.Depth,
                Height = baseImage.Height,
                Width = baseImage.Width,
                ArrayCount = baseImage.ArrayCount,
                MipCount = baseImage.MipCount
            };

            // This is our working buffer for B4G4R4A4.
            IGorgonImage destImage = new GorgonImage(destInfo);

            try
            {
                // If necessary, convert to B8G8R8A8. Otherwise, we'll just downsample directly.
                if ((newImage.Format != BufferFormat.B8G8R8A8_UNorm)
                    && (newImage.Format != BufferFormat.R8G8B8A8_UNorm))
                {
                    newImage = baseImage.Clone();
                    ConvertToFormat(newImage, BufferFormat.B8G8R8A8_UNorm, dithering);
                }

                // The next step is to manually downsample to R4G4B4A4.
                // Because we're doing this manually, dithering won't be an option unless unless we've downsampled from a much higher bit format when converting to B8G8R8A8.
                for (int array = 0; array < newImage.ArrayCount; ++array)
                {
                    for (int mip = 0; mip < newImage.MipCount; ++mip)
                    {
                        int depthCount = newImage.GetDepthCount(mip);

                        for (int depth = 0; depth < depthCount; depth++)
                        {
                            IGorgonImageBuffer destBuffer = destImage.Buffers[mip, destInfo.ImageType == ImageType.Image3D ? depth : array];
                            IGorgonImageBuffer srcBuffer = newImage.Buffers[mip, newImage.ImageType == ImageType.Image3D ? depth : array];

                            ConvertPixelsToB4G4R4A4(destBuffer, srcBuffer);
                        }
                    }
                }

                destImage.CopyTo(baseImage);

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
        /// Note that the <paramref name="mipCount"/> may not be honored depending on the current width, height, and depth of the image. Check the width, height and/or depth property on the returned 
        /// <see cref="IGorgonImage"/> to determine how many mip levels were actually generated.
        /// </para>
        /// </remarks>
        public static IGorgonImage GenerateMipMaps(this IGorgonImage baseImage, int mipCount, ImageFilter filter = ImageFilter.Point)
        {
            if (baseImage == null)
            {
                throw new ArgumentNullException(nameof(baseImage));
            }

            int maxMips = GorgonImage.CalculateMaxMipCount(baseImage);

            // If we specify 0, then generate a full chain.
            if ((mipCount <= 0) || (mipCount > maxMips))
            {
                mipCount = maxMips;
            }

            // If we don't have any mip levels, then return the image as-is.
            if (mipCount < 2)
            {
                return baseImage;
            }

            var destSettings = new GorgonImageInfo(baseImage)
            {
                MipCount = mipCount
            };

            var newImage = new GorgonImage(destSettings);
            var wic = new WicUtilities();

            try
            {
                // Copy the top mip level from the source image to the dest image.
                for (int array = 0; array < baseImage.ArrayCount; ++array)
                {
                    GorgonNativeBuffer<byte> buffer = newImage.Buffers[0, array].Data;
                    int size = buffer.SizeInBytes;
                    baseImage.Buffers[0, array].Data.CopyTo(buffer, count: size);
                }

                // If we have 4 bits per channel, then we need to convert to 8 bit per channel to make WIC happy.
                if (baseImage.Format == BufferFormat.B4G4R4A4_UNorm)
                {
                    newImage.ConvertToFormat(BufferFormat.R8G8B8A8_UNorm);
                }

                wic.GenerateMipImages(newImage, filter);

                // Convert back if we asked for 4 bit per channel.
                if (baseImage.Format == BufferFormat.B4G4R4A4_UNorm)
                {
                    newImage.ConvertToFormat(BufferFormat.B4G4R4A4_UNorm);
                }

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

            if ((newDepth < 1) && (baseImage.ImageType == ImageType.Image3D))
            {
                throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(newDepth));
            }

            // Only use the appropriate dimensions.
            switch (baseImage.ImageType)
            {
                case ImageType.Image1D:
                    cropRect.Height = baseImage.Height;
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    newDepth = baseImage.Depth;
                    break;
            }

            // If the intersection of the crop rectangle and the source buffer are the same (and the depth is the same), then we don't need to crop.
            var bufferRect = new DX.Rectangle(0, 0, baseImage.Width, baseImage.Height);
            var clipRect = DX.Rectangle.Intersect(cropRect, bufferRect);

            if ((bufferRect.Equals(ref clipRect)) && (newDepth == baseImage.Depth))
            {
                return baseImage;
            }

            var wic = new WicUtilities();

            IGorgonImage newImage = null;

            try
            {
                int calcMipLevels = GorgonImage.CalculateMaxMipCount(cropRect.Width, cropRect.Height, newDepth).Min(baseImage.MipCount);
                newImage = wic.Resize(baseImage, cropRect.X, cropRect.Y, cropRect.Width, cropRect.Height, newDepth, calcMipLevels, ImageFilter.Point, ResizeMode.Crop);

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
        /// Function to expand an image width, height, and/or depth.
        /// </summary>
        /// <param name="baseImage">The image to expand.</param>
        /// <param name="newWidth">The new width of the image.</param>
        /// <param name="newHeight">The new height of the image.</param>
        /// <param name="newDepth">The new depth of the image.</param>
        /// <param name="anchor">[Optional] The anchor point for placing the image data after the image is expanded.</param>
        /// <returns>The expanded image.</returns>
        /// <remarks>
        /// <para>
        /// This will expand the size of an image, but not stretch the actual image data. This will leave a padding around the original image area filled with transparent pixels. 
        /// </para>
        /// <para>
        /// The image data can be repositioned in the new image by specifying an <paramref name="anchor"/> point. 
        /// </para>
        /// <para>
        /// If the new size of the image is smaller than that of the <paramref name="baseImage"/>, then the new size is constrained to the old size. Cropping is not supported by this method. 
        /// </para>
        /// <para>
        /// If a user wishes to resize the image, then call the <see cref="Resize"/> method, of if they wish to crop an image, use the <see cref="Crop"/> method.
        /// </para>
        /// </remarks>
	    public static IGorgonImage Expand(this IGorgonImage baseImage, int newWidth, int newHeight, int newDepth, ImageExpandAnchor anchor = ImageExpandAnchor.UpperLeft)
        {
            IGorgonImage workingImage = null;
            WicUtilities wic = null;

            try
            {
                // Constrain to the correct sizes.
                newWidth = newWidth.Max(baseImage.Width);
                newHeight = newHeight.Max(baseImage.Height);
                newDepth = newDepth.Max(baseImage.Depth);

                // Only use the appropriate dimensions.
                switch (baseImage.ImageType)
                {
                    case ImageType.Image1D:
                        newHeight = baseImage.Height;
                        break;
                    case ImageType.Image2D:
                    case ImageType.ImageCube:
                        newDepth = baseImage.Depth;
                        break;
                }

                // We don't shink with this method, use the Crop method for that.
                if ((newWidth <= baseImage.Width) && (newHeight <= baseImage.Height) && (newDepth <= baseImage.Depth))
                {
                    return baseImage;
                }

                wic = new WicUtilities();

                workingImage = new GorgonImage(new GorgonImageInfo(baseImage)
                {
                    Width = newWidth,
                    Height = newHeight,
                    Depth = newDepth
                });

                DX.Point position = DX.Point.Zero;

                switch (anchor)
                {
                    case ImageExpandAnchor.UpperMiddle:
                        position = new DX.Point((newWidth / 2) - (baseImage.Width / 2), 0);
                        break;
                    case ImageExpandAnchor.UpperRight:
                        position = new DX.Point(newWidth - baseImage.Width, 0);
                        break;
                    case ImageExpandAnchor.MiddleLeft:
                        position = new DX.Point(0, (newHeight / 2) - (baseImage.Height / 2));
                        break;
                    case ImageExpandAnchor.Center:
                        position = new DX.Point((newWidth / 2) - (baseImage.Width / 2), (newHeight / 2) - (baseImage.Height / 2));
                        break;
                    case ImageExpandAnchor.MiddleRight:
                        position = new DX.Point(newWidth - baseImage.Width, (newHeight / 2) - (baseImage.Height / 2));
                        break;
                    case ImageExpandAnchor.BottomLeft:
                        position = new DX.Point(0, newHeight - baseImage.Height);
                        break;
                    case ImageExpandAnchor.BottomMiddle:
                        position = new DX.Point((newWidth / 2) - (baseImage.Width / 2), newHeight - baseImage.Height);
                        break;
                    case ImageExpandAnchor.BottomRight:
                        position = new DX.Point(newWidth - baseImage.Width, newHeight - baseImage.Height);
                        break;
                }

                int calcMipLevels = GorgonImage.CalculateMaxMipCount(newWidth, newHeight, newDepth).Min(baseImage.MipCount);
                workingImage = wic.Resize(baseImage, position.X, position.Y, newWidth, newHeight, newDepth, calcMipLevels, ImageFilter.Point, ResizeMode.Expand);

                // Send the data over to the new image.
                workingImage.CopyTo(baseImage);

                return baseImage;
            }
            finally
            {
                workingImage?.Dispose();
                wic?.Dispose();
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

            if ((newHeight < 1) && ((baseImage.ImageType == ImageType.Image2D) || (baseImage.ImageType == ImageType.ImageCube)))
            {
                throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_HEIGHT_TOO_SMALL, nameof(newHeight));
            }

            if ((newDepth < 1) && (baseImage.ImageType == ImageType.Image3D))
            {
                throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(newDepth));
            }

            // Only use the appropriate dimensions.
            switch (baseImage.ImageType)
            {
                case ImageType.Image1D:
                    newHeight = baseImage.Height;
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    newDepth = baseImage.Depth;
                    break;
            }

            // If we haven't actually changed the size, then skip out.
            if ((newWidth == baseImage.Width) && (newHeight == baseImage.Height) && (newDepth == baseImage.Depth))
            {
                return baseImage;
            }

            var wic = new WicUtilities();

            IGorgonImage newImage = null;
            try
            {
                int calcMipLevels = GorgonImage.CalculateMaxMipCount(newWidth, newHeight, newDepth).Min(baseImage.MipCount);
                newImage = wic.Resize(baseImage, 0, 0, newWidth, newHeight, newDepth, calcMipLevels, filter, ResizeMode.Scale);

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
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="baseImage"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="format"/> is set to <see cref="BufferFormat.Unknown"/>.
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
        /// For the <see cref="BufferFormat.B4G4R4A4_UNorm"/> format, Gorgon has to perform a manual conversion since that format is not supported by WIC. Because of this, the 
        /// <paramref name="dithering"/> flag will be ignored when downsampling to that format.
        /// </para>
        /// </remarks>
        public static IGorgonImage ConvertToFormat(this IGorgonImage baseImage, BufferFormat format, ImageDithering dithering = ImageDithering.None)
        {
            if (baseImage == null)
            {
                throw new ArgumentNullException(nameof(baseImage));
            }

            if ((format == BufferFormat.Unknown) || (!baseImage.CanConvertToFormat(format)))
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
            }

            if (format == baseImage.Format)
            {
                return baseImage;
            }

            // If we've asked for 4 bit per channel BGRA, then we have to convert the base image to B8R8G8A8,and then convert manually (no support in WIC).
            if (format == BufferFormat.B4G4R4A4_UNorm)
            {
                return ConvertToB4G4R4A4(baseImage, dithering);
            }

            // If we're currently using B4G4R4A4, then manually convert (no support in WIC).
            if (baseImage.Format == BufferFormat.B4G4R4A4_UNorm)
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

                newImage.CopyTo(baseImage);

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
        /// <exception cref="ArgumentException">Thrown when image format is compressed.</exception>
        /// <remarks>
        /// <para>
        /// Use this to convert an image to a premultiplied format. This takes each Red, Green and Blue element and multiplies them by the Alpha element.
        /// </para>
        /// <para>
        /// If the image does not contain alpha then the method will return right away and no alterations to the image will be performed.
        /// </para>
        /// </remarks>
        public static IGorgonImage ConvertToPremultipliedAlpha(this IGorgonImage baseImage)
        {
            IGorgonImage newImage = null;

            if (baseImage == null)
            {
                throw new ArgumentNullException(nameof(baseImage));
            }

            if (!baseImage.FormatInfo.HasAlpha)
            {
                return baseImage;
            }

            if (baseImage.FormatInfo.IsCompressed)
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, baseImage.Format), nameof(baseImage));
            }

            try
            {                
                var cloneImageInfo = new GorgonImageInfo(baseImage)
                {
                    HasPreMultipliedAlpha = true
                };
                newImage = new GorgonImage(cloneImageInfo);
                baseImage.CopyTo(newImage);

                unsafe
                {
                    int arrayOrDepth = newImage.ImageType == ImageType.Image3D ? newImage.Depth : newImage.ArrayCount;

                    for (int mip = 0; mip < newImage.MipCount; ++mip)
                    {
                        for (int i = 0; i < arrayOrDepth; ++i)
                        {
                            IGorgonImageBuffer buffer = newImage.Buffers[mip, i];
                            byte* ptr = (byte*)buffer.Data;
                            int rowPitch = buffer.PitchInformation.RowPitch;

                            for (int y = 0; y < buffer.Height; ++y)
                            {
                                ImageUtilities.SetPremultipliedScanline(ptr, rowPitch, ptr, rowPitch, buffer.Format);
                                ptr += rowPitch;
                            }
                        }
                    }
                }

                newImage.CopyTo(baseImage);

                return baseImage;
            }
            finally
            {
                newImage?.Dispose();
            }
        }

        /// <summary>
        /// Function to set the alpha channel for a specific buffer in the image.
        /// </summary>
        /// <param name="buffer">The buffer to set the alpha channel on.</param>
        /// <param name="alphaValue">The value to set.</param>
        /// <param name="updateAlphaRange">[Optional] The range of alpha values in the buffer that will be updated.</param>
        /// <returns>The fluent interface for the buffer that was updated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if the buffer format is compressed.</exception>
        /// <remarks>
        /// <para>
        /// This will set the alpha channel for the image data in the <paramref name="buffer"/> to a discrete value specified by <paramref name="alphaValue"/>. 
        /// </para>
        /// <para>
        /// If the <paramref name="updateAlphaRange"/> parameter is set, then the alpha values in the <paramref name="buffer"/> will be examined and if the alpha value is less than the minimum range or 
        /// greater than the maximum range, then the <paramref name="alphaValue"/> will <b>not</b> be set on the alpha channel.
        /// </para>
        /// </remarks>
        public static IGorgonImageBuffer SetAlpha(this IGorgonImageBuffer buffer, float alphaValue, GorgonRangeF? updateAlphaRange = null)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            // If we don't have an alpha channel, then don't do anything.
            if (!buffer.FormatInformation.HasAlpha)
            {
                return buffer;
            }

            // We don't support compressed formats.
            if (buffer.FormatInformation.IsCompressed)
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, buffer.Format), nameof(buffer));
            }

            if (updateAlphaRange == null)
            {
                updateAlphaRange = new GorgonRangeF(0, 1);
            }            

            unsafe
            {
                byte* src = (byte*)buffer.Data;
                uint alpha = (uint)(alphaValue * 255.0f);
                uint min = (uint)(updateAlphaRange.Value.Minimum * 255.0f);
                uint max = (uint)(updateAlphaRange.Value.Maximum * 255.0f);

                for (int y = 0; y < buffer.Height; ++y)
                {
                    ImageUtilities.SetAlphaScanline(src, buffer.PitchInformation.RowPitch, src, buffer.PitchInformation.RowPitch, buffer.Format, alpha, min, max);
                    src += buffer.PitchInformation.RowPitch;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Function to determine if the source format can convert to any of the formats in the destination list.
        /// </summary>
        /// <param name="sourceFormat">The source format to compare.</param>
        /// <param name="destFormat">List of destination formats to compare.</param>
        /// <returns>An array of formats that the source format can be converted into, or an empty array if no conversion is possible.</returns>
        public static IReadOnlyList<BufferFormat> CanConvertToAny(this BufferFormat sourceFormat, IEnumerable<BufferFormat> destFormat)
        {
            if ((sourceFormat == BufferFormat.Unknown)
                || (destFormat == null))
            {
                return Array.Empty<BufferFormat>();
            }

            if (destFormat.All(item => item == sourceFormat))
            {
                return destFormat.ToArray();
            }

            using (var wic = new WicUtilities())
            {
                return wic.CanConvertFormats(sourceFormat, destFormat);
            }
        }
    }
}
