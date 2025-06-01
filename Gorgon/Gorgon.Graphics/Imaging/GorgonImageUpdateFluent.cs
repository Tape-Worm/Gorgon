
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: December 27, 2020 4:49:31 PM
// 

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Graphics.Imaging.Wic;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// Operations for the <see cref="IGorgonImageUpdateFluent"/> interface
/// </summary>
public partial class GorgonImage
    : IGorgonImageUpdateFluent
{
    // SIMD masks for pixel values when converting to and from 16 bpp.
    private static readonly Vector256<ushort> _16bppAndMask = Vector256.Create<ushort>(0xf);
    private static readonly Vector256<uint> _32bppAndMask = Vector256.Create<uint>(0xf);
    private static readonly bool _isHwAccelerated = Vector256.IsHardwareAccelerated;

    // Flag to indicate that we're in the middle of an editing session.
    private bool _isEditing;
    // Commands to execute when EndUpdate is called.
    private readonly ConcurrentQueue<Action> _commands = new();
    // WIC utility functions for editing the image.
    private WicUtilities? _wic;

    /// <summary>
    /// Function to perform image format conversion.
    /// </summary>
    /// <param name="format">The format to convert into.</param>
    /// <param name="dithering">The type of dithering to apply.</param>
    private void PerformFormatConversion(BufferFormat format, ImageDithering dithering)
    {
        Debug.Assert(_wic is not null, "WIC interface is null.");

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

        GorgonFormatInfo destInfo = new(format);

        GorgonImage newImage = _wic.ConvertToFormat(this, format, dithering, FormatInfo.IsSRgb, destInfo.IsSRgb);

        UpdateImagePtr(newImage);
    }

    /// <summary>
    /// Function to update the image data for this image with the image data from another image.
    /// </summary>
    /// <param name="newImage">The new image containing the image data to take ownership of.</param>
    private void UpdateImagePtr(GorgonImage newImage)
    {
        Debug.Assert(newImage is not null, "New image is null");

        _imageBuffer?.Dispose();
        _imageInfo = newImage._imageInfo;
        SizeInBytes = CalculateSizeInBytes(_imageInfo);
        FormatInfo = new GorgonFormatInfo(_imageInfo.Format);

        ImageData = (GorgonPtr<byte>)(_imageBuffer = newImage._imageBuffer);
        _buffers = newImage._buffers;
    }

    /// <summary>
    /// Function to convert the pixel data in the buffers from B8G8RAA8 or R8G8B8A8 to B4G4R4A4.
    /// </summary>
    /// <param name="dest">The destination buffer to receive the newly formatted data.</param>
    /// <param name="src">The source buffer to containing the source pixels to convert.</param>
    private static void ConvertPixelsToB4G4R4A4(IGorgonImageBuffer dest, IGorgonImageBuffer src)
    {
        GorgonPtr<ushort> destBufferPtr = (GorgonPtr<ushort>)dest.ImageData;
        GorgonPtr<uint> srcBufferPtr = (GorgonPtr<uint>)src.ImageData;

        for (int i = 0; i < src.PitchInformation.SlicePitch; i += sizeof(uint))
        {
            uint srcPixel = (srcBufferPtr++).Value;
            uint b, g, r, a;

            a = ((srcPixel >> 24) & 0xff) >> 4;
            g = ((srcPixel >> 8) & 0xff) >> 4;

            if (src.Format == BufferFormat.B8G8R8A8_UNorm)
            {
                r = ((srcPixel >> 16) & 0xff) >> 4;
                b = (srcPixel & 0xff) >> 4;
            }
            else // Convert from R8G8B8A8.
            {
                b = ((srcPixel >> 16) & 0xff) >> 4;
                r = (srcPixel & 0xff) >> 4;
            }

            (destBufferPtr++).Value = (ushort)((a << 12) | (r << 8) | (g << 4) | b);
        }
    }

    /// <summary>
    /// Function to convert the pixel data in the buffers from B8G8RAA8 or R8G8B8A8 to B4G4R4A4 using SIMD.
    /// </summary>
    /// <param name="dest">The destination buffer to receive the newly formatted data.</param>
    /// <param name="src">The source buffer to containing the source pixels to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private unsafe static void ConvertPixelsToB4G4R4A4Simd(IGorgonImageBuffer dest, IGorgonImageBuffer src)
    {
        uint* srcPtr = (uint*)src.ImageData;
        ushort* destPtr = (ushort*)dest.ImageData;

        int count = Vector256<byte>.Count;
        long maxByteSize = src.SizeInBytes;
        long maxLength = ((maxByteSize - count) / count) >> 1;

        bool isBgr = src.Format is BufferFormat.B8G8R8A8_UNorm or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.B8G8R8X8_UNorm or BufferFormat.B8G8R8X8_UNorm_SRgb;

        for (long i = 0; i <= maxLength; ++i, maxByteSize -= (count << 1))
        {
            Vector256<uint> pixelsLo = Vector256.ShiftRightLogical(Unsafe.ReadUnaligned<Vector256<uint>>(srcPtr), 4);

            srcPtr += 8;

            Vector256<uint> pixelsHi = Vector256.ShiftRightLogical(Unsafe.ReadUnaligned<Vector256<uint>>(srcPtr), 4);

            Vector256<uint> aLo = Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsLo, 24), _32bppAndMask);
            Vector256<uint> bLo = !isBgr ? Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsLo, 16), _32bppAndMask) : Vector256.BitwiseAnd(pixelsLo, _32bppAndMask);
            Vector256<uint> gLo = Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsLo, 8), _32bppAndMask);
            Vector256<uint> rLo = !isBgr ? Vector256.BitwiseAnd(pixelsLo, _32bppAndMask) : Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsLo, 16), _32bppAndMask);

            Vector256<uint> aHi = Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsHi, 24), _32bppAndMask);
            Vector256<uint> bHi = !isBgr ? Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsHi, 16), _32bppAndMask) : Vector256.BitwiseAnd(pixelsHi, _32bppAndMask);
            Vector256<uint> gHi = Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsHi, 8), _32bppAndMask);
            Vector256<uint> rHi = !isBgr ? Vector256.BitwiseAnd(pixelsHi, _32bppAndMask) : Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixelsHi, 16), _32bppAndMask);

            Vector256<ushort> a16bpp = Vector256.ShiftLeft(Vector256.Narrow(aLo, aHi), 12);
            Vector256<ushort> r16bpp = Vector256.ShiftLeft(Vector256.Narrow(rLo, rHi), 8);
            Vector256<ushort> g16bpp = Vector256.ShiftLeft(Vector256.Narrow(gLo, gHi), 4);
            Vector256<ushort> b16bpp = Vector256.Narrow(bLo, bHi);

            Vector256<ushort> pixel = Vector256.BitwiseOr(Vector256.BitwiseOr(Vector256.BitwiseOr(a16bpp, r16bpp), g16bpp), b16bpp);

            pixel.Store(destPtr);

            srcPtr += 8;
            destPtr += 16;
        }

        if (maxByteSize <= 0)
        {
            return;
        }

        for (long i = 0; i < maxByteSize; ++i)
        {
            uint srcPixel = *srcPtr++;
            uint b, g, r, a;

            a = ((srcPixel >> 24) & 0xff) >> 4;
            g = ((srcPixel >> 8) & 0xff) >> 4;

            if (isBgr)
            {
                r = ((srcPixel >> 16) & 0xff) >> 4;
                b = (srcPixel & 0xff) >> 4;
            }
            else // Convert from R8G8B8A8.
            {
                b = ((srcPixel >> 16) & 0xff) >> 4;
                r = (srcPixel & 0xff) >> 4;
            }

            *destPtr++ = (ushort)((a << 12) | (r << 8) | (g << 4) | b);
        }
    }

    /// <summary>
    /// Function to convert the pixel data in the buffers from B4G4R4A4 to B8G8R8A8 or R8G8B8A8.
    /// </summary>
    /// <param name="dest">The destination buffer to receive the newly formatted data.</param>
    /// <param name="src">The source buffer to containing the source pixels to convert.</param>        
    private static void ConvertPixelsFromB4G4R4A4(IGorgonImageBuffer dest, IGorgonImageBuffer src)
    {
        GorgonPtr<ushort> srcBufferPtr = (GorgonPtr<ushort>)src.ImageData;
        GorgonPtr<uint> destBufferPtr = (GorgonPtr<uint>)dest.ImageData;

        for (int i = 0; i < src.PitchInformation.SlicePitch; i += sizeof(ushort))
        {
            ushort srcPixel = (srcBufferPtr++).Value;

            int a = ((srcPixel >> 12) & 0xf);
            int r = ((srcPixel >> 8) & 0xf);
            int g = ((srcPixel >> 4) & 0xf);
            int b = (srcPixel & 0xf);

            // Adjust the values to fill out a 32 bit integer: If r == 0xc in the 16 bit format, then r == 0xcc in the 32 bit format by taking the value and 
            // shifting it left by 4 bits and OR'ing the original r value again. ((0xc << 4 = 0xc0) OR 0xc = 0xcc).
            a = ((a << 4) | a);
            r = ((r << 4) | r);
            g = ((g << 4) | g);
            b = ((b << 4) | b);

            uint value = (uint)((dest.Format == BufferFormat.B8G8R8A8_UNorm)
                                ? ((a << 24) | (r << 16) | (g << 8) | b)
                                // Convert to R8G8B8A8 (flipped for little endian).
                                : ((a << 24) | (b << 16) | (g << 8) | r));
            (destBufferPtr++).Value = value;
        }
    }

    /// <summary>
    /// Function to convert the pixel data in the buffers from B4G4R4A4 to B8G8R8A8 or R8G8B8A8 using SIMD.
    /// </summary>
    /// <param name="dest">The destination buffer to receive the newly formatted data.</param>
    /// <param name="src">The source buffer to containing the source pixels to convert.</param>    
    private unsafe static void ConvertPixelsFromB4G4R4A4Simd(IGorgonImageBuffer dest, IGorgonImageBuffer src)
    {
        ushort* srcPtr = (ushort*)src.ImageData;
        uint* destPtr = (uint*)dest.ImageData;

        int count = Vector256<byte>.Count;
        long maxByteSize = dest.SizeInBytes;
        long maxLength = ((maxByteSize - count) / count) >> 1;

        bool isBgr = dest.Format is BufferFormat.B8G8R8A8_UNorm or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.B8G8R8X8_UNorm or BufferFormat.B8G8R8X8_UNorm_SRgb;

        for (long i = 0; i <= maxLength; ++i, maxByteSize -= (count << 1))
        {
            Vector256<ushort> pixels = Unsafe.ReadUnaligned<Vector256<ushort>>(srcPtr);

            Vector256<ushort> a = Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixels, 12), _16bppAndMask);
            Vector256<ushort> r = Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixels, 8), _16bppAndMask);
            Vector256<ushort> g = Vector256.BitwiseAnd(Vector256.ShiftRightLogical(pixels, 4), _16bppAndMask);
            Vector256<ushort> b = Vector256.BitwiseAnd(pixels, _16bppAndMask);

            (Vector256<uint> aLo, Vector256<uint> aHi) = Vector256.Widen(Vector256.BitwiseOr(Vector256.ShiftLeft(a, 4), a));
            (Vector256<uint> rLo, Vector256<uint> rHi) = Vector256.Widen(Vector256.BitwiseOr(Vector256.ShiftLeft(r, 4), r));
            (Vector256<uint> gLo, Vector256<uint> gHi) = Vector256.Widen(Vector256.BitwiseOr(Vector256.ShiftLeft(g, 4), g));
            (Vector256<uint> bLo, Vector256<uint> bHi) = Vector256.Widen(Vector256.BitwiseOr(Vector256.ShiftLeft(b, 4), b));

            aLo = Vector256.ShiftLeft(aLo, 24);
            if (!isBgr)
            {
                bLo = Vector256.ShiftLeft(bLo, 16);
            }
            else
            {
                rLo = Vector256.ShiftLeft(rLo, 16);
            }
            gLo = Vector256.ShiftLeft(gLo, 8);

            aHi = Vector256.ShiftLeft(aHi, 24);
            if (!isBgr)
            {
                bHi = Vector256.ShiftLeft(bHi, 16);
            }
            else
            {
                rLo = Vector256.ShiftLeft(rLo, 16);
            }
            gHi = Vector256.ShiftLeft(gHi, 8);

            Vector256<uint> pixel = Vector256.BitwiseOr(Vector256.BitwiseOr(Vector256.BitwiseOr(aLo, bLo), gLo), rLo);

            pixel.Store(destPtr);
            destPtr += 8;

            pixel = Vector256.BitwiseOr(Vector256.BitwiseOr(Vector256.BitwiseOr(aHi, bHi), gHi), rHi);

            pixel.Store(destPtr);
            destPtr += 8;
            srcPtr += 16;
        }

        if (maxByteSize <= 0)
        {
            return;
        }

        for (long i = 0; i < maxByteSize; ++i)
        {
            ushort srcPixel = *srcPtr++;

            int a = ((srcPixel >> 12) & 0xf);
            int r = ((srcPixel >> 8) & 0xf);
            int g = ((srcPixel >> 4) & 0xf);
            int b = (srcPixel & 0xf);

            // Adjust the values to fill out a 32 bit integer: If r == 0xc in the 16 bit format, then r == 0xcc in the 32 bit format by taking the value and 
            // shifting it left by 4 bits and OR'ing the original r value again. ((0xc << 4 = 0xc0) OR 0xc = 0xcc).
            a = ((a << 4) | a);
            r = ((r << 4) | r);
            g = ((g << 4) | g);
            b = ((b << 4) | b);

            uint value = (uint)(isBgr ? ((a << 24) | (r << 16) | (g << 8) | b)
                                // Convert to R8G8B8A8 (flipped for little endian).
                                : ((a << 24) | (b << 16) | (g << 8) | r));
            *destPtr++ = value;
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
        GorgonImageInfo destInfo = _imageInfo with
        {
            Format = tempFormat
        };

        // Our destination image for B8G8R8A8 or R8G8B8A8.
        GorgonImage destImage = new(destInfo);

        // We have to manually upsample from R4G4B4A4 to B8R8G8A8.
        // Because we're doing this manually, dithering won't be an option unless 
        for (int array = 0; array < ArrayCount; ++array)
        {
            for (int mip = 0; mip < MipCount; ++mip)
            {
                int depthCount = GetDepthCount(mip);

                for (int depth = 0; depth < depthCount; depth++)
                {
                    IGorgonImageBuffer destBuffer = destImage.Buffers[mip, ImageType == ImageDataType.Image3D ? depth : array];
                    IGorgonImageBuffer srcBuffer = Buffers[mip, ImageType == ImageDataType.Image3D ? depth : array];

                    if ((srcBuffer.SizeInBytes <= 262_144) || (!Vector256.IsHardwareAccelerated))
                    {
                        ConvertPixelsFromB4G4R4A4(destBuffer, srcBuffer);
                    }
                    else
                    {
                        ConvertPixelsFromB4G4R4A4Simd(destBuffer, srcBuffer);
                    }
                }
            }
        }

        UpdateImagePtr(destImage);

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
        GorgonImageInfo destInfo = _imageInfo with
        {
            Format = BufferFormat.B4G4R4A4_UNorm
        };

        // This is our working buffer for B4G4R4A4.
        GorgonImage workingImage = new(destInfo);
        GorgonImage tempBuffer = this;

        // If necessary, convert to B8G8R8A8. Otherwise, we'll just downsample directly.
        if (Format is not BufferFormat.B8G8R8A8_UNorm and not BufferFormat.R8G8B8A8_UNorm)
        {
            tempBuffer = new GorgonImage(this);
            tempBuffer.BeginUpdate()
                        .ConvertToFormat(BufferFormat.B8G8R8A8_UNorm, dithering)
                        .EndUpdate();
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
                    IGorgonImageBuffer destBuffer = workingImage.Buffers[mip, destInfo.ImageType == ImageDataType.Image3D ? depth : array];
                    IGorgonImageBuffer srcBuffer = tempBuffer.Buffers[mip, tempBuffer.ImageType == ImageDataType.Image3D ? depth : array];

                    if ((srcBuffer.SizeInBytes <= 262_144) || (!_isHwAccelerated))
                    {
                        ConvertPixelsToB4G4R4A4(destBuffer, srcBuffer);
                    }
                    else
                    {
                        ConvertPixelsToB4G4R4A4Simd(destBuffer, srcBuffer);
                    }
                }
            }
        }

        // Point at the same memory that the working image is pointing at.
        tempBuffer.UpdateImagePtr(workingImage);

        if (this != tempBuffer)
        {
            // Point at the same memory that temp buffer is pointing at.
            UpdateImagePtr(tempBuffer);
        }

        // We do not need to ever dispose of tempbuffer, 
        // If we do not do a pre-conversion on the image into BGRA8888, then it will be set to 'this'. Disposal would break us by destroying our image data.
        // If we do have a pre-converion, then 'this' image pointer will point at the same image data in 'tempbuffer'. Disposal would also break us by destroying our image data.
        return this;
    }

    /// <inheritdoc/>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.GenerateMipMaps(int mipCount, ImageFilter filter)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if (_wic is null)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORIMG_ERR_NOT_EDITING);
        }

        void DoGenerateMipMaps()
        {
            int maxMips = GorgonImageInfo.GetMaximumMipCount(Width, Height, Depth);

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

            GorgonImageInfo destSettings = _imageInfo with
            {
                MipCount = mipCount
            };

            GorgonImage newImage = new(destSettings);

            // Copy the top mip level from the source image to the dest image.
            for (int array = 0; array < ArrayCount; ++array)
            {
                GorgonPtr<byte> buffer = newImage.Buffers[0, array].ImageData;
                buffer.Slice(0, buffer.SizeInBytes).CopyTo(buffer.Slice(0, buffer.SizeInBytes));
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

            UpdateImagePtr(newImage);
        }

        _commands.Enqueue(DoGenerateMipMaps);
        return this;
    }

    /// <inheritdoc/>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.Crop(GorgonRectangle cropRect, int? newDepth)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if (_wic is null)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORIMG_ERR_NOT_EDITING);
        }

        newDepth ??= Depth.Max(1);

        if ((newDepth < 1) && (ImageType == ImageDataType.Image3D))
        {
            throw new ArgumentOutOfRangeException(nameof(newDepth), Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL);
        }

        void DoCrop()
        {
            // Only use the appropriate dimensions.
            switch (ImageType)
            {
                case ImageDataType.Image1D:
                    cropRect.Height = Height;
                    break;
                case ImageDataType.Image2D:
                case ImageDataType.ImageCube:
                    newDepth = Depth;
                    break;
            }

            // If the intersection of the crop rectangle and the source buffer are the same (and the depth is the same), then we don't need to crop.
            GorgonRectangle bufferRect = new(0, 0, Width, Height);
            GorgonRectangle clipRect = GorgonRectangle.Intersect(cropRect, bufferRect);

            if ((bufferRect.Equals(clipRect)) && (newDepth == Depth))
            {
                return;
            }

            int calcMipLevels = GorgonImageInfo.GetMaximumMipCount(cropRect.Width, cropRect.Height, newDepth.Value).Min(MipCount);
            GorgonImage newImage;

            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                using GorgonImage tempImage = new(this);
                tempImage.ConvertFromB4G4R4A4(BufferFormat.R8G8B8A8_UNorm);
                newImage = _wic.Resize(tempImage, cropRect.X, cropRect.Y, cropRect.Width, cropRect.Height, newDepth.Value, calcMipLevels, ImageFilter.Point, ResizeMode.Crop);
            }
            else
            {
                newImage = _wic.Resize(this, cropRect.X, cropRect.Y, cropRect.Width, cropRect.Height, newDepth.Value, calcMipLevels, ImageFilter.Point, ResizeMode.Crop);
            }

            // Convert back to 4 bit per channel.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                newImage.BeginUpdate()
                        .ConvertToFormat(BufferFormat.B4G4R4A4_UNorm, ImageDithering.None)
                        .EndUpdate();
            }

            // Send the data over to the new image.
            UpdateImagePtr(newImage);
        }

        _commands.Enqueue(DoCrop);
        return this;
    }

    /// <inheritdoc/>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.Expand(int newWidth, int newHeight, int? newDepth, GorgonPoint? offset)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if (_wic is null)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORIMG_ERR_NOT_EDITING);
        }

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
                case ImageDataType.Image1D:
                    newHeight = Height;
                    break;
                case ImageDataType.Image2D:
                case ImageDataType.ImageCube:
                    newDepth = Depth;
                    break;
            }

            // We don't shink with this method, use the Crop method for that.
            if ((newWidth <= Width) && (newHeight <= Height) && (newDepth <= Depth))
            {
                return;
            }

            offset ??= GorgonPoint.Zero;

            int calcMipLevels = GorgonImageInfo.GetMaximumMipCount(newWidth, newHeight, newDepth.Value).Min(MipCount);
            GorgonImage workingImage;

            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                using GorgonImage tempImage = new(this);
                tempImage.ConvertFromB4G4R4A4(BufferFormat.R8G8B8A8_UNorm);
                workingImage = _wic.Resize(tempImage, offset.Value.X, offset.Value.Y, newWidth, newHeight, newDepth.Value, calcMipLevels, ImageFilter.Point, ResizeMode.Expand);
            }
            else
            {
                workingImage = _wic.Resize(this, offset.Value.X, offset.Value.Y, newWidth, newHeight, newDepth.Value, calcMipLevels, ImageFilter.Point, ResizeMode.Expand);
            }

            // Convert back to 4 bit per channel.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                workingImage.BeginUpdate()
                            .ConvertToFormat(BufferFormat.B4G4R4A4_UNorm, ImageDithering.None)
                            .EndUpdate();
            }

            // Send the data over to the new image.
            UpdateImagePtr(workingImage);
        }

        _commands.Enqueue(DoExpand);
        return this;
    }

    /// <inheritdoc/>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.Resize(int newWidth, int newHeight, int? newDepth, ImageFilter filter)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if (_wic is null)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GORIMG_ERR_NOT_EDITING);
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(newWidth, 1);

        newDepth ??= Depth.Max(1);

        if (ImageType is ImageDataType.Image2D or ImageDataType.ImageCube)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(newHeight, 1);
        }

        if (ImageType == ImageDataType.Image3D)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(newDepth.Value, 1);
        }

        void DoResize()
        {
            // Only use the appropriate dimensions.
            switch (ImageType)
            {
                case ImageDataType.Image1D:
                    newHeight = Height;
                    break;
                case ImageDataType.Image2D:
                case ImageDataType.ImageCube:
                    newDepth = Depth;
                    break;
            }

            // If we haven't actually changed the size, then skip out.
            if ((newWidth == Width) && (newHeight == Height) && (newDepth == Depth))
            {
                return;
            }

            int calcMipLevels = GorgonImageInfo.GetMaximumMipCount(newWidth, newHeight, newDepth.Value).Min(MipCount);
            GorgonImage newImage;

            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                using GorgonImage tempImage = new(this);
                tempImage.ConvertFromB4G4R4A4(BufferFormat.R8G8B8A8_UNorm);
                newImage = _wic.Resize(tempImage, 0, 0, newWidth, newHeight, newDepth.Value, calcMipLevels, filter, ResizeMode.Scale);
            }
            else
            {
                newImage = _wic.Resize(this, 0, 0, newWidth, newHeight, newDepth.Value, calcMipLevels, filter, ResizeMode.Scale);
            }

            // Convert back to 4 bit per channel.
            if (Format == BufferFormat.B4G4R4A4_UNorm)
            {
                newImage.ConvertToB4G4R4A4(ImageDithering.None);
            }

            UpdateImagePtr(newImage);
        }

        _commands.Enqueue(DoResize);
        return this;
    }

    /// <inheritdoc/>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.ConvertToFormat(BufferFormat format, ImageDithering dithering)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if ((format == BufferFormat.Unknown) || (!CanConvertToFormat(format)))
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
        }

        void DoConvertToFormat() => PerformFormatConversion(format, dithering);

        _commands.Enqueue(DoConvertToFormat);
        return this;
    }

    /// <inheritdoc/>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.ConvertFromPremultipedAlpha()
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        void DoConvertFromPremultipedAlpha()
        {
            if (!FormatInfo.HasAlpha)
            {
                return;
            }

            _imageInfo = _imageInfo with
            {
                IsPremultiplied = false
            };

            int arrayOrDepth = ImageType == ImageDataType.Image3D ? Depth : ArrayCount;

            for (int mip = 0; mip < MipCount; ++mip)
            {
                for (int i = 0; i < arrayOrDepth; ++i)
                {
                    IGorgonImageBuffer buffer = Buffers[mip, i];
                    GorgonPtr<byte> ptr = buffer.ImageData;
                    int rowPitch = buffer.PitchInformation.RowPitch;

                    for (int y = 0; y < buffer.Height; ++y)
                    {
                        ImageUtilities.RemovePremultipliedScanline(ptr, rowPitch, ptr, rowPitch, buffer.Format);
                        ptr += rowPitch;
                    }
                }
            }
        }

        _commands.Enqueue(DoConvertFromPremultipedAlpha);

        return this;
    }

    /// <inheritdoc/>
    IGorgonImageUpdateFluent IGorgonImageUpdateFluent.ConvertToPremultipliedAlpha()
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        void DoConvertToPremultipliedAlpha()
        {
            if (!FormatInfo.HasAlpha)
            {
                return;
            }

            _imageInfo = _imageInfo with
            {
                IsPremultiplied = true
            };

            int arrayOrDepth = ImageType == ImageDataType.Image3D ? Depth : ArrayCount;

            for (int mip = 0; mip < MipCount; ++mip)
            {
                for (int i = 0; i < arrayOrDepth; ++i)
                {
                    IGorgonImageBuffer buffer = Buffers[mip, i];
                    GorgonPtr<byte> ptr = buffer.ImageData;
                    int rowPitch = buffer.PitchInformation.RowPitch;

                    for (int y = 0; y < buffer.Height; ++y)
                    {
                        ImageUtilities.SetPremultipliedScanline(ptr, rowPitch, ptr, rowPitch, buffer.Format);
                        ptr += rowPitch;
                    }
                }
            }
        }

        _commands.Enqueue(DoConvertToPremultipliedAlpha);

        return this;
    }

    /// <inheritdoc/>
    IGorgonImage IGorgonImageUpdateFluent.EndUpdate(bool cancel)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        try
        {
            if (cancel)
            {
                _commands.Clear();
            }

            while (!_commands.IsEmpty)
            {
                if (!_commands.TryDequeue(out Action? command))
                {
                    continue;
                }

                command();
            }
        }
        finally
        {
            _isEditing = false;

            // Get rid of our little helper when we're done.
            WicUtilities? wic = Interlocked.Exchange(ref _wic, null);
            wic?.Dispose();
        }
        return this;
    }

    /// <inheritdoc/>
    public IGorgonImageUpdateFluent BeginUpdate()
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if ((FormatInfo.IsCompressed) || (FormatInfo.IsTypeless))
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
        }

        if ((_isEditing) || (_wic is not null))
        {
            throw new InvalidOperationException(Resources.GORIMG_ERR_ALREADY_EDITING);
        }

        _isEditing = true;
        _wic = new WicUtilities();

        return this;
    }
}