#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 27, 2020 4:49:31 PM
// 
#endregion

using System.Collections.Concurrent;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.UI;
using BCnEncode = BCnEncoder.Encoder;
using DX = SharpDX;

namespace Gorgon.Graphics.Imaging;

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
/// Operations for the <see cref="IGorgonImageUpdateFluent"/> interface.
/// </summary>
public partial class GorgonImage
    : IGorgonImageUpdateFluent
{
    #region Variables.
    // Commands to execute when EndUpdate is called.
    private readonly ConcurrentQueue<Action> _commands = new();
    // WIC utility functions for editing the image.
    private WicUtilities _wic;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to perform image format conversion.
    /// </summary>
    /// <param name="format">The format to convert into.</param>
    /// <param name="dithering">The type of dithering to apply.</param>
    private void PerformFormatConversion(BufferFormat format, ImageDithering dithering)
    {
        if (format == Format)
        {
            return;
        }

        // If we've asked for 4 bit per channel BGRA, then we have to convert the base image to B8R8G8A8,and then convert manually (no support in WIC).
        if (format == BufferFormat.B4G4R4A4_UNorm)
        {
            ConvertToB4G4R4A4(dithering);
            return;
        }

        // If we're currently using B4G4R4A4, then manually convert (no support in WIC).
        if (Format == BufferFormat.B4G4R4A4_UNorm)
        {
            ConvertFromB4G4R4A4(format);
            return;
        }

        var destInfo = new GorgonFormatInfo(format);

        GorgonImage newImage = _wic.ConvertToFormat(this, format, dithering, FormatInfo.IsSRgb, destInfo.IsSRgb);

        UpdateImagePtr(ref newImage);
    }

    /// <summary>
    /// Function to update the image data for this image with the image data from another image.
    /// </summary>
    /// <param name="newImage">The new image containing the image data to take ownership of.</param>
    private void UpdateImagePtr(ref GorgonImage newImage)
    {
        _imageData.Dispose();
        _imageInfo = newImage._imageInfo;
        SizeInBytes = CalculateSizeInBytes(_imageInfo);
        FormatInfo = new GorgonFormatInfo(_imageInfo.Format);

        _imagePtr = _imageData = newImage._imageData;
        _imageBuffers = newImage._imageBuffers;

        // Don't allow usage of this image after we're done.  It no longer owns the pointer.
        newImage = null;
    }

    /// <summary>
    /// Function to convert the pixel data in the buffers from B8G8R4A8 (or R8G8B8A8) to B4G4R4A4.
    /// </summary>
    /// <param name="dest">The destination buffer to receive the newly formatted data.</param>
    /// <param name="src">The source buffer to containing the source pixels to convert.</param>
    private static void ConvertPixelsToB4G4R4A4(IGorgonImageBuffer dest, IGorgonImageBuffer src)
    {
        unsafe
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
    }

    /// <summary>
    /// Function to convert the pixel data in the buffers from B4G4R4A4 to B8G8R4A8 or R8G8B8A8.
    /// </summary>
    /// <param name="dest">The destination buffer to receive the newly formatted data.</param>
    /// <param name="src">The source buffer to containing the source pixels to convert.</param>
    private unsafe void ConvertPixelsFromB4G4R4A4(IGorgonImageBuffer dest, IGorgonImageBuffer src)
    {
        unsafe
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
    }

    /// <summary>
    /// Function to convert the image from B4G4R4A4.
    /// </summary>
    /// <param name="destFormat">The destination format.</param>
    /// <returns>The updated image.</returns>
    private GorgonImage ConvertFromB4G4R4A4(BufferFormat destFormat)
    {
        // If we're converting to R8G8B8A8 or B8G8R8A8, then use those formats, otherwise, default to B8G8R8A8 as an intermediate buffer.
        BufferFormat tempFormat = (destFormat is not BufferFormat.B8G8R8A8_UNorm and not BufferFormat.R8G8B8A8_UNorm) ? BufferFormat.B8G8R8A8_UNorm : destFormat;

        // Create an worker image in B8G8R8A8 format.
        var destInfo = new GorgonImageInfo(ImageType, tempFormat)
        {
            Depth = Depth,
            Height = Height,
            Width = Width,
            ArrayCount = ArrayCount,
            MipCount = MipCount
        };

        // Our destination image for B8G8R8A8 or R8G8B8A8.
        var destImage = new GorgonImage(destInfo);

        // We have to manually upsample from R4G4B4A4 to B8R8G8A8.
        // Because we're doing this manually, dithering won't be an option unless 
        for (int array = 0; array < ArrayCount; ++array)
        {
            for (int mip = 0; mip < MipCount; ++mip)
            {
                int depthCount = GetDepthCount(mip);

                for (int depth = 0; depth < depthCount; depth++)
                {
                    IGorgonImageBuffer destBuffer = destImage.Buffers[mip, ImageType == ImageType.Image3D ? depth : array];
                    IGorgonImageBuffer srcBuffer = Buffers[mip, ImageType == ImageType.Image3D ? depth : array];

                    ConvertPixelsFromB4G4R4A4(destBuffer, srcBuffer);
                }
            }
        }

        UpdateImagePtr(ref destImage);

        // If the destination format is not R8G8B8A8 or B8G8R8A8, then we need to do more conversion.
        if (destFormat != Format)
        {
            PerformFormatConversion(destFormat, ImageDithering.None);
        }

        return this;
    }

    /// <summary>
    /// Function to convert the image to B4G4R4A4.
    /// </summary>
    /// <param name="dithering">Dithering to apply to the converstion to B8G8R8A8.</param>
    /// <returns>The updated image.</returns>
    private GorgonImage ConvertToB4G4R4A4(ImageDithering dithering)
    {
        var destInfo = new GorgonImageInfo(ImageType, BufferFormat.B4G4R4A4_UNorm)
        {
            Depth = Depth,
            Height = Height,
            Width = Width,
            ArrayCount = ArrayCount,
            MipCount = MipCount
        };

        // This is our working buffer for B4G4R4A4.
        var workingImage = new GorgonImage(destInfo);
        GorgonImage tempBuffer = null;

        try
        {
            // If necessary, convert to B8G8R8A8. Otherwise, we'll just downsample directly.
            if (Format is not BufferFormat.B8G8R8A8_UNorm and not BufferFormat.R8G8B8A8_UNorm)
            {
                tempBuffer = new GorgonImage(this);
                tempBuffer.BeginUpdate()
                          .ConvertToFormat(BufferFormat.B8G8R8A8_UNorm, dithering)
                          .EndUpdate();
            }
            else
            {
                tempBuffer = this;
            }

            // The next step is to manually downsample to R4G4B4A4.
            // Because we're doing this manually, dithering won't be an option unless unless we've downsampled from a much higher bit format when converting to B8G8R8A8.
            for (int array = 0; array < ArrayCount; ++array)
            {
                for (int mip = 0; mip < MipCount; ++mip)
                {
                    int depthCount = GetDepthCount(mip);

                    for (int depth = 0; depth < depthCount; depth++)
                    {
                        IGorgonImageBuffer destBuffer = workingImage.Buffers[mip, destInfo.ImageType == ImageType.Image3D ? depth : array];
                        IGorgonImageBuffer srcBuffer = tempBuffer.Buffers[mip, tempBuffer.ImageType == ImageType.Image3D ? depth : array];

                        ConvertPixelsToB4G4R4A4(destBuffer, srcBuffer);
                    }
                }
            }

            tempBuffer.UpdateImagePtr(ref workingImage);

            if (this != tempBuffer)
            {
                UpdateImagePtr(ref tempBuffer);
            }

            return this;
        }
        finally
        {
            if (tempBuffer != this)
            {
                tempBuffer.Dispose();
            }
        }
    }

    /// <summary>
    /// Function to generate a new mip map chain.
    /// </summary>
    /// <param name="mipCount">The number of mip map levels.</param>
    /// <param name="filter">[Optional] The filter to apply when copying the data from one mip level to another.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the updated mip map data.</returns>
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
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.GenerateMipMaps(int mipCount, ImageFilter filter)
    {
        void DoGenerateMipMaps()
        {
            int maxMips = CalculateMaxMipCount(this);

            // If we specify 0, then generate a full chain.
            if ((mipCount <= 0) || (mipCount > maxMips))
            {
                mipCount = maxMips;
            }

            // If we don't have any mip levels, then return the image as-is.
            if (mipCount < 2)
            {
                return;
            }

            var destSettings = new GorgonImageInfo(this)
            {
                MipCount = mipCount
            };

            var newImage = new GorgonImage(destSettings);

            // Copy the top mip level from the source image to the dest image.
            for (int array = 0; array < ArrayCount; ++array)
            {
                GorgonPtr<byte> buffer = newImage.Buffers[0, array].Data;
                int size = buffer.SizeInBytes;
                Buffers[0, array].Data.CopyTo(buffer, count: size);
            }

            // If we have 4 bits per channel, then we need to convert to 8 bit per channel to make WIC happy.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                newImage.BeginUpdate()
                        .ConvertToFormat(BufferFormat.R8G8B8A8_UNorm, ImageDithering.None)
                        .EndUpdate();
            }

            _wic.GenerateMipImages(newImage, filter);

            // Convert back if we asked for 4 bit per channel.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                newImage.BeginUpdate()
                        .ConvertToFormat(BufferFormat.B4G4R4A4_UNorm, ImageDithering.None)
                        .EndUpdate();
            }

            UpdateImagePtr(ref newImage);
        }

        _commands.Enqueue(DoGenerateMipMaps);
        return this;
    }

    /// <summary>
    /// Function to crop the image to the rectangle passed to the parameters.
    /// </summary>
    /// <param name="cropRect">The rectangle that will be used to crop the image.</param>
    /// <param name="newDepth">The new depth for the image (for <see cref="ImageType.Image3D"/> images).</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the resized image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="newDepth"/> parameter is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// This method will crop the existing image a smaller version of itself as a new <see cref="IGorgonImage"/>. If the sizes are the same, or the <paramref name="cropRect"/> is larger than the size 
    /// of the original image, then no changes will be made.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.Crop(DX.Rectangle cropRect, int? newDepth)
    {
        newDepth ??= Depth.Max(1);

        if ((newDepth < 1) && (ImageType == ImageType.Image3D))
        {
            throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(newDepth));
        }

        void DoCrop()
        {
            // Only use the appropriate dimensions.
            switch (ImageType)
            {
                case ImageType.Image1D:
                    cropRect.Height = Height;
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    newDepth = Depth;
                    break;
            }

            // If the intersection of the crop rectangle and the source buffer are the same (and the depth is the same), then we don't need to crop.
            var bufferRect = new DX.Rectangle(0, 0, Width, Height);
            var clipRect = DX.Rectangle.Intersect(cropRect, bufferRect);

            if ((bufferRect.Equals(ref clipRect)) && (newDepth == Depth))
            {
                return;
            }

            int calcMipLevels = CalculateMaxMipCount(cropRect.Width, cropRect.Height, newDepth.Value).Min(MipCount);
            GorgonImage newImage = _wic.Resize(this, cropRect.X, cropRect.Y, cropRect.Width, cropRect.Height, newDepth.Value, calcMipLevels, ImageFilter.Point, ResizeMode.Crop);

            // Convert back to 4 bit per channel.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                newImage.BeginUpdate()
                        .ConvertToFormat(BufferFormat.B4G4R4A4_UNorm, ImageDithering.None)
                        .EndUpdate();
            }

            // Send the data over to the new image.
            UpdateImagePtr(ref newImage);
        }

        _commands.Enqueue(DoCrop);
        return this;
    }

    /// <summary>
    /// Function to expand an image width, height, and/or depth.
    /// </summary>
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
    /// If the new size of the image is smaller than that of this image, then the new size is constrained to the old size. Cropping is not supported by this method. 
    /// </para>
    /// <para>
    /// If a user wishes to resize the image, then call the <see cref="IGorgonImageUpdateFluent.Resize"/> method, of if they wish to crop an image, use the <see cref="IGorgonImageUpdateFluent.Crop"/> method.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.Expand(int newWidth, int newHeight, int? newDepth, ImageExpandAnchor anchor)
    {
        newDepth ??= Depth.Max(1);

        void DoExpand()
        {
            // Constrain to the correct sizes.
            newWidth = newWidth.Max(Width);
            newHeight = newHeight.Max(Height);
            newDepth = newDepth.Value.Max(Depth);

            // Only use the appropriate dimensions.
            switch (ImageType)
            {
                case ImageType.Image1D:
                    newHeight = Height;
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    newDepth = Depth;
                    break;
            }

            // We don't shink with this method, use the Crop method for that.
            if ((newWidth <= Width) && (newHeight <= Height) && (newDepth <= Depth))
            {
                return;
            }

            DX.Point position = DX.Point.Zero;

            switch (anchor)
            {
                case ImageExpandAnchor.UpperMiddle:
                    position = new DX.Point((newWidth / 2) - (Width / 2), 0);
                    break;
                case ImageExpandAnchor.UpperRight:
                    position = new DX.Point(newWidth - Width, 0);
                    break;
                case ImageExpandAnchor.MiddleLeft:
                    position = new DX.Point(0, (newHeight / 2) - (Height / 2));
                    break;
                case ImageExpandAnchor.Center:
                    position = new DX.Point((newWidth / 2) - (Width / 2), (newHeight / 2) - (Height / 2));
                    break;
                case ImageExpandAnchor.MiddleRight:
                    position = new DX.Point(newWidth - Width, (newHeight / 2) - (Height / 2));
                    break;
                case ImageExpandAnchor.BottomLeft:
                    position = new DX.Point(0, newHeight - Height);
                    break;
                case ImageExpandAnchor.BottomMiddle:
                    position = new DX.Point((newWidth / 2) - (Width / 2), newHeight - Height);
                    break;
                case ImageExpandAnchor.BottomRight:
                    position = new DX.Point(newWidth - Width, newHeight - Height);
                    break;
            }

            int calcMipLevels = CalculateMaxMipCount(newWidth, newHeight, newDepth.Value).Min(MipCount);
            GorgonImage workingImage = _wic.Resize(this, position.X, position.Y, newWidth, newHeight, newDepth.Value, calcMipLevels, ImageFilter.Point, ResizeMode.Expand);

            // Convert back to 4 bit per channel.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                workingImage.BeginUpdate()
                            .ConvertToFormat(BufferFormat.B4G4R4A4_UNorm, ImageDithering.None)
                            .EndUpdate();
            }

            // Send the data over to the new image.
            UpdateImagePtr(ref workingImage);
        }

        _commands.Enqueue(DoExpand);
        return this;
    }

    /// <summary>
    /// Function to resize the image to a new width, height and/or depth.
    /// </summary>
    /// <param name="newWidth">The new width for the image.</param>
    /// <param name="newHeight">The new height for the image (for <see cref="ImageType.Image2D"/> and <see cref="ImageType.ImageCube"/> images).</param>
    /// <param name="newDepth">The new depth for the image (for <see cref="ImageType.Image3D"/> images).</param>
    /// <param name="filter">[Optional] The type of filtering to apply to the scaled image to help smooth larger and smaller images.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the resized image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="newWidth"/>, <paramref name="newHeight"/>, or <paramref name="newDepth"/> parameters are less than 1.</exception>
    /// <exception cref="GorgonException">Thrown if there was an error during resizing regarding image pixel format conversion due to the type of <paramref name="filter"/> applied.</exception>
    /// <remarks>
    /// <para>
    /// This method will change the size of an existing image and return a larger or smaller version of itself as a new <see cref="IGorgonImage"/>. 
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.Resize(int newWidth, int newHeight, int? newDepth, ImageFilter filter)
    {
        if (newWidth < 1)
        {
            throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_WIDTH_TOO_SMALL, nameof(newWidth));
        }

        newDepth ??= Depth.Max(1);

        if ((newHeight < 1) && ((ImageType == ImageType.Image2D) || (ImageType == ImageType.ImageCube)))
        {
            throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_HEIGHT_TOO_SMALL, nameof(newHeight));
        }

        if ((newDepth < 1) && (ImageType == ImageType.Image3D))
        {
            throw new ArgumentOutOfRangeException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(newDepth));
        }

        void DoResize()
        {
            // Only use the appropriate dimensions.
            switch (ImageType)
            {
                case ImageType.Image1D:
                    newHeight = Height;
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    newDepth = Depth;
                    break;
            }

            // If we haven't actually changed the size, then skip out.
            if ((newWidth == Width) && (newHeight == Height) && (newDepth == Depth))
            {
                return;
            }

            int calcMipLevels = CalculateMaxMipCount(newWidth, newHeight, newDepth.Value).Min(MipCount);
            GorgonImage newImage = _wic.Resize(this, 0, 0, newWidth, newHeight, newDepth.Value, calcMipLevels, filter, ResizeMode.Scale);

            // Convert back to 4 bit per channel.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                newImage.BeginUpdate()
                        .ConvertToFormat(BufferFormat.B4G4R4A4_UNorm, ImageDithering.None)
                        .EndUpdate();
            }

            UpdateImagePtr(ref newImage);
        }

        _commands.Enqueue(DoResize);
        return this;
    }

    /// <summary>
    /// Function to convert the pixel format of an image into another pixel format.
    /// </summary>
    /// <param name="format">The new pixel format for the image.</param>
    /// <param name="dithering">[Optional] Flag to indicate the type of dithering to perform when the bit depth for the <paramref name="format"/> is lower than the original bit depth.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data with the converted pixel format.</returns>
    /// <exception cref="GorgonException">Thrown when the <paramref name="format"/> is set to <see cref="BufferFormat.Unknown"/>.
    /// <para>-or-</para>
    /// <para>Thrown when the original format could not be converted into the desired <paramref name="format"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this to convert an image format from one to another. The conversion functionality uses Windows Imaging Components (WIC) to perform the conversion.
    /// </para>
    /// <para>
    /// Because this method uses WIC, not all formats will be convertible. To determine if a format can be converted, use the <see cref="CanConvertToFormat"/> method. 
    /// </para>
    /// <para>
    /// For the <see cref="BufferFormat.B4G4R4A4_UNorm"/> format, Gorgon has to perform a manual conversion since that format is not supported by WIC. Because of this, the 
    /// <paramref name="dithering"/> flag will be ignored when downsampling to that format.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.ConvertToFormat(BufferFormat format, ImageDithering dithering)
    {
        if ((format == BufferFormat.Unknown) || (!CanConvertToFormat(format)))
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
        }

        void DoConvertToFormat() => PerformFormatConversion(format, dithering);

        _commands.Enqueue(DoConvertToFormat);
        return this;
    }

    /// <summary>
    /// Function to convert the image data from a premultiplied format.
    /// </summary>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data with the premultiplied alpha removed from the pixel data.</returns>
    /// <remarks>
    /// <para>
    /// Use this to convert an image from a premultiplied format. This takes each Red, Green and Blue element and divides them by the Alpha element.
    /// </para>
    /// <para>
    /// If the image does not contain alpha then the method will return right away and no alterations to the image will be performed.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.ConvertFromPremultipedAlpha()
    {
        void DoConvertFromPremultipedAlpha()
        {
            if (!FormatInfo.HasAlpha)
            {
                return;
            }

#if NET6_0_OR_GREATER
            _imageInfo = _imageInfo with
            {
                HasPreMultipliedAlpha = false
            };
#endif

            int arrayOrDepth = ImageType == ImageType.Image3D ? Depth : ArrayCount;

            for (int mip = 0; mip < MipCount; ++mip)
            {
                for (int i = 0; i < arrayOrDepth; ++i)
                {
                    IGorgonImageBuffer buffer = Buffers[mip, i];
                    GorgonPtr<byte> ptr = buffer.Data;
                    int rowPitch = buffer.PitchInformation.RowPitch;

                    for (int y = 0; y < buffer.Height; ++y)
                    {
                        ImageUtilities.RemovePremultipliedScanline(in ptr, rowPitch, in ptr, rowPitch, buffer.Format);
                        ptr += rowPitch;
                    }
                }
            }
        }

        _commands.Enqueue(DoConvertFromPremultipedAlpha);

        return this;
    }

    /// <summary>
    /// Function to convert the image data into a premultiplied format.
    /// </summary>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data with the premultiplied alpha pixel data.</returns>
    /// <remarks>
    /// <para>
    /// Use this to convert an image to a premultiplied format. This takes each Red, Green and Blue element and multiplies them by the Alpha element.
    /// </para>
    /// <para>
    /// If the image does not contain alpha then the method will return right away and no alterations to the image will be performed.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.ConvertToPremultipliedAlpha()
    {
        void DoConvertToPremultipliedAlpha()
        {
            if (!FormatInfo.HasAlpha)
            {
                return;
            }

#if NET6_0_OR_GREATER
            _imageInfo = _imageInfo with
            {
                HasPreMultipliedAlpha = true
            };
#endif

            int arrayOrDepth = ImageType == ImageType.Image3D ? Depth : ArrayCount;

            for (int mip = 0; mip < MipCount; ++mip)
            {
                for (int i = 0; i < arrayOrDepth; ++i)
                {
                    IGorgonImageBuffer buffer = Buffers[mip, i];
                    GorgonPtr<byte> ptr = buffer.Data;
                    int rowPitch = buffer.PitchInformation.RowPitch;

                    for (int y = 0; y < buffer.Height; ++y)
                    {
                        ImageUtilities.SetPremultipliedScanline(in ptr, rowPitch, in ptr, rowPitch, buffer.Format);
                        ptr += rowPitch;
                    }
                }
            }
        }

        _commands.Enqueue(DoConvertToPremultipliedAlpha);

        return this;
    }

    /// <summary>
    /// Function to compress the image data using block compression.
    /// </summary>
    /// <param name="compressionFormat">The block compression format to use. Must be one of the BCn formats.</param>
    /// <param name="useBC1Alpha">[Optional] <b>true</b> if using BC1 (DXT1) compression, and the image contains alpha, <b>false</b> if no alpha is in the image.</param>
    /// <param name="quality">[Optional] The quality level for the compression.</param>
    /// <param name="multithreadCompression">[Optional] <b>true</b> to use multithreading while compressing the data, <b>false</b> to only use a single thread.</param>
    /// <exception cref="ArgumentException">Thrown when the compression format is not supported.</exception>
    /// <exception cref="NotSupportedException">Thrown if the image <see cref="Format"/> is not supported.</exception>
    /// <remarks>
    /// <para>
    /// This method will compress the data within the image using the standardized block compression formats. The <see cref="BufferFormat"/> enum contains 7 levels of 
    /// block compression named BC1 - BC7. The features of each compression level are documented at <a target="_blank" href="https://docs.microsoft.com/en-us/windows/win32/direct3d11/texture-block-compression-in-direct3d-11"/>.
    /// </para>
    /// <para>
    /// Block compression is, by nature, a lossy compression format. Thus some fidelity will be lost when compressing the data, it is recommended that images be compressed as a last stage in 
    /// processing. Because block compression lays the image data out differently than standard image data, the functionality provided for modifying an image 
    /// (e.g. <see cref="IGorgonImageUpdateFluent.Resize"/>) will not work and will throw an exception if used on block compressed data.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Because block compressed data is lossy, it is not recommended that images be decompressed and compressed over and over as it will degrade the image fidelity severely. Thus, the 
    /// recommendation to perform block compression only after all other processing is done.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The image data being compressed must be able to convert to a 32bit RGBA format, and must be uncompressed. If it cannot, then an exception will be thrown.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The <see cref="BufferFormat.BC6H_Sf16"/>, and <see cref="BufferFormat.BC6H_Uf16"/> block compression types are not implemented at this time. If it is used, then an exception will be thrown. This 
    /// may change in the future.
    /// </para>
    /// <para>
    /// Typeless formats are not supported at all, and will not be.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// The higher levels of block compression provide much better image quality, however they perform much slower. Especially when single threaded, it is recommended that the 
    /// <paramref name="multithreadCompression"/> value be set to <b>true</b> when using BC7 compression.
    /// </para>
    /// <para>
    /// When using BC1 compression, optional 1-bit alpha channel data can be stored with the image data. The developer must specify whether to use the alpha data or not via the <paramref name="useBC1Alpha"/> 
    /// parameter.
    /// </para>
    /// <para>
    /// Unlike the <see cref="Decompress"/> method, this method does not return an image. This is done to signify that the compress operation is the last operation in a chain of calls via the image 
    /// modification fluent interface.
    /// </para>
    /// </remarks>
    /// <seealso cref="Decompress"/>
    IGorgonImageUpdateFinalize IGorgonImageUpdateFluent.Compress(BufferFormat compressionFormat, bool useBC1Alpha, BcCompressionQuality quality, bool multithreadCompression)
    {
        var formatInfo = new GorgonFormatInfo(compressionFormat);

        if ((!formatInfo.IsCompressed) || (formatInfo.IsTypeless) || (compressionFormat == BufferFormat.BC6H_Sf16) || (compressionFormat == BufferFormat.BC6H_Uf16))
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, compressionFormat), nameof(compressionFormat));
        }

        if (FormatInfo.IsTypeless)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
        }

        void DoCompress()
        {
            GorgonImage workImage = this;

            if ((FormatInfo.Group != BufferFormat.R8G8B8A8_Typeless)
                && (FormatInfo.Group != BufferFormat.B8G8R8A8_Typeless)
                && (CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm)))
            {
                workImage = new GorgonImage(this);
                workImage.BeginUpdate()
                         .ConvertToFormat(BufferFormat.R8G8B8A8_UNorm, ImageDithering.None)
                         .EndUpdate();
            }

            var encoder = new BCnEncode.BcEncoder
            {
                LuminanceAsRed = true
            };

            var info = new GorgonImageInfo(ImageType, compressionFormat)
            {
                MipCount = MipCount,
                ArrayCount = ArrayCount,
                Depth = Depth,
                HasPreMultipliedAlpha = HasPreMultipliedAlpha,
                Width = Width,
                Height = Height
            };

            var newBuffer = new GorgonNativeBuffer<byte>(CalculateSizeInBytes(info));
            ImageBufferList bufferList;

            try
            {
                bufferList = new ImageBufferList(info);
                bufferList.CreateBuffers(newBuffer.Pointer);

                for (int arrayIndex = 0; arrayIndex < ArrayCount; ++arrayIndex)
                {
                    for (int mip = 0; mip < MipCount; ++mip)
                    {
                        for (int depthSlice = 0; depthSlice < GetDepthCount(mip); ++depthSlice)
                        {
                            int depthArrayIndex = ImageType != ImageType.Image3D ? arrayIndex : depthSlice;
                            IGorgonImageBuffer buffer = workImage.Buffers[mip, depthArrayIndex];
                            using GorgonNativeBuffer<byte> compressedData = encoder.EncodeToRawBytes(buffer.Data,
                                                                                                      buffer.Width,
                                                                                                      buffer.Height,
                                                                                                      useBC1Alpha,
                                                                                                      buffer.Format,
                                                                                                      compressionFormat,
                                                                                                      (BCnEncode.CompressionQuality)quality,
                                                                                                      multithreadCompression);
                            // Replace the data in the source image buffer with our newly compressed data.
                            compressedData.CopyTo((Span<byte>)bufferList[mip, depthArrayIndex].Data);
                        }
                    }
                }
            }
            catch
            {
                newBuffer?.Dispose();
                throw;
            }
            finally
            {
                if (workImage != this)
                {
                    workImage?.Dispose();
                }
            }

            _imageData?.Dispose();
            _imagePtr = _imageData = null;

            SanitizeInfo(info);
            FormatInfo = new GorgonFormatInfo(info.Format);
            SizeInBytes = newBuffer.SizeInBytes;
            _imagePtr = _imageData = newBuffer;
            _imageBuffers = bufferList;
        }

        _commands.Enqueue(DoCompress);
        return this;
    }

    /// <summary>Function to finalize the update of the image and apply all changes.</summary>
    /// <param name="cancel">[Optional] <b>true</b> to cancel the operations, or <b>false</b> to commit them.</param>
    /// <returns>The original interface for the image that was updated.</returns>
    IGorgonImage IGorgonImageUpdateFinalize.EndUpdate(bool cancel)
    {
        try
        {
            while (_commands.Count > 0)
            {
                _commands.TryDequeue(out Action command);

                if (!cancel)
                {
                    command();
                }
            }
        }
        finally
        {
            _isEditing = false;

            // Get rid of our little helper when we're done.
            WicUtilities wic = Interlocked.Exchange(ref _wic, null);
            wic?.Dispose();
        }
        return this;
    }
    #endregion
}
