#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 1, 2017 9:44:33 PM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging.GdiPlus
{
    /// <summary>
    /// Extension methods to use GDI+ <see cref="System.Drawing"/> bitmaps with <see cref="IGorgonImage"/>
    /// </summary>
    public static class GdiPlusExtensions
    {
        /// <summary>
        /// Function to transfer a 32 bit rgba image into a <see cref="IGorgonImageBuffer"/>.
        /// </summary>
        /// <param name="bitmapLock">The lock on the bitmap to transfer from.</param>
        /// <param name="buffer">The buffer to transfer into.</param>
	    private static void Transfer32Argb(BitmapData bitmapLock, IGorgonImageBuffer buffer)
        {
            unsafe
            {
                int* pixels = (int*)bitmapLock.Scan0.ToPointer();

                for (int y = 0; y < bitmapLock.Height; y++)
                {
                    // We only need the width here, as our pointer will handle the stride by virtue of being an int.
                    int* offset = pixels + (y * bitmapLock.Width);

                    int destOffset = y * buffer.PitchInformation.RowPitch;
                    for (int x = 0; x < bitmapLock.Width; x++)
                    {
                        // The DXGI format nomenclature is a little confusing as we tend to think of the layout as being highest to 
                        // lowest, but in fact, it is lowest to highest.
                        // So, we must convert to ABGR even though the DXGI format is RGBA. The memory layout is from lowest 
                        // (R at byte 0) to the highest byte (A at byte 3).
                        // Thus, R is the lowest byte, and A is the highest: A(24), B(16), G(8), R(0).
                        var color = new GorgonColor(*offset);
                        ref int destBuffer = ref buffer.Data.AsRef<int>(destOffset);
                        destBuffer = color.ToABGR();
                        offset++;
                        destOffset += 4;
                    }
                }
            }
        }

        /// <summary>
        /// Function to transfer a 24 bit rgb image into a <see cref="IGorgonImageBuffer"/>.
        /// </summary>
        /// <param name="bitmapLock">The lock on the bitmap to transfer from.</param>
        /// <param name="buffer">The buffer to transfer into.</param>
        private static void Transfer24Rgb(BitmapData bitmapLock, IGorgonImageBuffer buffer)
        {
            unsafe
            {
                byte* pixels = (byte*)bitmapLock.Scan0.ToPointer();

                for (int y = 0; y < bitmapLock.Height; y++)
                {
                    // We only need the width here, as our pointer will handle the stride by virtue of being an int.
                    byte* offset = pixels + (y * bitmapLock.Stride);

                    int destOffset = y * buffer.PitchInformation.RowPitch;
                    for (int x = 0; x < bitmapLock.Width; x++)
                    {
                        // The DXGI format nomenclature is a little confusing as we tend to think of the layout as being highest to 
                        // lowest, but in fact, it is lowest to highest.
                        // So, we must convert to ABGR even though the DXGI format is RGBA. The memory layout is from lowest 
                        // (R at byte 0) to the highest byte (A at byte 3).
                        // Thus, R is the lowest byte, and A is the highest: A(24), B(16), G(8), R(0).
                        byte b = *offset++;
                        byte g = *offset++;
                        byte r = *offset++;

                        var color = new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
                        ref int destBuffer = ref buffer.Data.AsRef<int>(destOffset);
                        destBuffer = color.ToABGR();
                        destOffset += 4;
                    }
                }
            }
        }

        /// <summary>
        /// Function to retrieve the pointer to locked bitmap data.
        /// </summary>
        /// <param name="bitmapData">The bitmap data returned from the lock.</param>
        /// <param name="offset">[Optional] The offset, in bytes, to start at.</param>
        /// <returns>A new pointer.</returns>
        private unsafe static GorgonPtr<byte> GetGdiImagePtr(BitmapData bitmapData, int offset = 0) => new GorgonPtr<byte>(bitmapData.Scan0 + offset, bitmapData.Stride);

        /// <summary>
        /// Function to convert an individual <see cref="IGorgonImageBuffer"/> to a GDI+ bitmap object.
        /// </summary>
        /// <param name="buffer">The buffer to convert.</param>
        /// <returns>A new GDI+ bitmap object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="buffer"/> is not a 32 bit <c>R8G8B8A8</c> format, or <c>B8G8R8*</c> format.</exception>
        /// <remarks>
        /// <para>
        /// This method will take a <see cref="IGorgonImageBuffer"/> and copy its data into a new 2D <see cref="Bitmap"/>. 
        /// </para>
        /// <para>
        /// Some format conversion is performed on the <paramref name="buffer"/> when it is imported. The format conversion will always convert to a pixel format of <c>Format32bppArgb</c> or 
        /// <c>Format24bppRgb</c>.  The following formats are supported for 32 bit conversion:
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_SInt"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_SNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UInt"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_Typeless"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_Typeless"/></term>
        ///     </item>
        /// </list>
        /// The following formats are supported for 24 bit conversion:
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_Typeless"/></term>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// If the source <paramref name="buffer"/> does not support any of the formats on the lists, then an exception will be thrown.
        /// </para>
        /// </remarks>
	    public static Bitmap ToBitmap(this IGorgonImageBuffer buffer)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            PixelFormat pixelFormat;
            bool needsSwizzle = false;

            switch (buffer.Format)
            {
                case BufferFormat.R8G8B8A8_UNorm:
                case BufferFormat.R8G8B8A8_UNorm_SRgb:
                case BufferFormat.R8G8B8A8_SInt:
                case BufferFormat.R8G8B8A8_SNorm:
                case BufferFormat.R8G8B8A8_UInt:
                case BufferFormat.R8G8B8A8_Typeless:
                    pixelFormat = PixelFormat.Format32bppArgb;
                    needsSwizzle = true;
                    break;
                case BufferFormat.B8G8R8A8_UNorm:
                case BufferFormat.B8G8R8A8_Typeless:
                case BufferFormat.B8G8R8A8_UNorm_SRgb:
                    pixelFormat = PixelFormat.Format32bppArgb;
                    break;
                case BufferFormat.B8G8R8X8_UNorm:
                case BufferFormat.B8G8R8X8_Typeless:
                case BufferFormat.B8G8R8X8_UNorm_SRgb:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    break;
                default:
                    throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, buffer.Format));
            }

            var result = new Bitmap(buffer.Width, buffer.Height, pixelFormat);

            unsafe
            {
                BitmapData destData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, pixelFormat);

                try
                {
                    for (int y = 0; y < buffer.Height; ++y)
                    {
                        GorgonPtr<byte> src = buffer.Data + (y * destData.Stride);
                        GorgonPtr<byte> dest = GetGdiImagePtr(destData, y * buffer.PitchInformation.RowPitch);

                        switch (pixelFormat)
                        {
                            case PixelFormat.Format32bppArgb:
                                if (!needsSwizzle)
                                {
                                    src.CopyTo(dest, count: destData.Stride.Min(buffer.PitchInformation.RowPitch));
                                    continue;
                                }

                                ImageUtilities.SwizzleScanline(src, buffer.PitchInformation.RowPitch, dest, destData.Stride, buffer.Format, ImageBitFlags.None);
                                break;
                            case PixelFormat.Format24bppRgb:
                                ImageUtilities.Compress24BPPScanLine(src, buffer.PitchInformation.RowPitch, dest, destData.Stride, true);
                                break;
                        }
                    }
                }
                finally
                {
                    result.UnlockBits(destData);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to copy the contents of an individual <see cref="IGorgonImageBuffer"/> to a GDI+ bitmap object.
        /// </summary>
        /// <param name="buffer">The buffer to convert.</param>
        /// <param name="bitmap">The bitmap that will receive the image data.</param>
        /// <returns>A new GDI+ bitmap object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/>, or the <paramref name="bitmap"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="buffer"/> and the <paramref name="bitmap"/> do not have the same width and height.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="buffer"/> is not a 32 bit <c>R8G8B8A8</c> format, or <c>B8G8R8*</c> format.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="bitmap"/> is not in a 32 bit ARGB format.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method will take a <see cref="IGorgonImageBuffer"/> and copy its data into a new 2D <see cref="Bitmap"/>. The <paramref name="buffer"/> and the <paramref name="bitmap"/> must have 
        /// an identical width and height. Otherwise, an exception will be thrown.
        /// </para>
        /// <para>
        /// Some format conversion is performed on the <paramref name="buffer"/> when it is imported. The format conversion will always convert to a pixel format of <c>Format32bppArgb</c> or 
        /// <c>Format24bppRgb</c>.  The following formats are supported for 32 bit conversion:
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_SInt"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_SNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UInt"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_Typeless"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_Typeless"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_Typeless"/></term>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// If the source <paramref name="buffer"/> does not support any of the formats on the lists, then an exception will be thrown.
        /// </para>
        /// </remarks>
	    public static void CopyTo(this IGorgonImageBuffer buffer, Bitmap bitmap)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (bitmap is null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            if ((bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                && (bitmap.PixelFormat != PixelFormat.Format32bppPArgb))
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, bitmap.PixelFormat));
            }

            if ((bitmap.Width != buffer.Width) || (bitmap.Height != buffer.Height))
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_BITMAP_SIZE_NOT_CORRECT, bitmap.Width, bitmap.Height, buffer.Width, buffer.Height));
            }

            bool needsSwizzle;

            switch (buffer.Format)
            {
                case BufferFormat.R8G8B8A8_UNorm:
                case BufferFormat.R8G8B8A8_UNorm_SRgb:
                case BufferFormat.R8G8B8A8_SInt:
                case BufferFormat.R8G8B8A8_SNorm:
                case BufferFormat.R8G8B8A8_UInt:
                case BufferFormat.R8G8B8A8_Typeless:
                    needsSwizzle = true;
                    break;
                case BufferFormat.B8G8R8A8_UNorm:
                case BufferFormat.B8G8R8A8_Typeless:
                case BufferFormat.B8G8R8A8_UNorm_SRgb:
                case BufferFormat.B8G8R8X8_UNorm:
                case BufferFormat.B8G8R8X8_Typeless:
                case BufferFormat.B8G8R8X8_UNorm_SRgb:
                    needsSwizzle = false;
                    break;
                default:
                    throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, buffer.Format));
            }

            BitmapData destData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            try
            {
                for (int y = 0; y < buffer.Height; ++y)
                {
                    GorgonPtr<byte> src = buffer.Data + (y * destData.Stride);
                    GorgonPtr<byte> dest = GetGdiImagePtr(destData, y * buffer.PitchInformation.RowPitch);

                    if (!needsSwizzle)
                    {
                        src.CopyTo(dest, count: destData.Stride.Min(buffer.PitchInformation.RowPitch));
                        continue;
                    }

                    ImageUtilities.SwizzleScanline(in src, buffer.PitchInformation.RowPitch, in dest, destData.Stride, buffer.Format, ImageBitFlags.None);
                }
            }
            finally
            {
                bitmap.UnlockBits(destData);
            }
        }

        /// <summary>
        /// Function to copy the contents of a GDI+ bitmap object to an individual <see cref="IGorgonImageBuffer"/>.
        /// </summary>
        /// <param name="bitmap">The bitmap to convert.</param>
        /// <param name="buffer">The buffer to that will receive the image data.</param>
        /// <returns>A new GDI+ bitmap object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bitmap"/>, or the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="bitmap"/> and the <paramref name="buffer"/> do not have the same width and height.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="buffer"/> is not a 32 bit <c>R8G8B8A8</c> format, or <c>B8G8R8*</c> format.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="bitmap"/> is not in a 32 bit ARGB format.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method will take a <see cref="IGorgonImageBuffer"/> and copy its data into a new 2D <see cref="Bitmap"/>. The <paramref name="buffer"/> and the <paramref name="bitmap"/> must have 
        /// an identical width and height. Otherwise, an exception will be thrown.
        /// </para>
        /// <para>
        /// Some format conversion is performed on the <paramref name="buffer"/> when it is imported. The format conversion will always convert to a pixel format of <c>Format32bppArgb</c> or 
        /// <c>Format24bppRgb</c>.  The following formats are supported for 32 bit conversion:
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_SInt"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_SNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_UInt"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.R8G8B8A8_Typeless"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8A8_Typeless"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_UNorm"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_UNorm_SRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="BufferFormat.B8G8R8X8_Typeless"/></term>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// If the source <paramref name="buffer"/> does not support any of the formats on the lists, then an exception will be thrown.
        /// </para>
        /// </remarks>
	    public static void CopyTo(this Bitmap bitmap, IGorgonImageBuffer buffer)
        {
            if (bitmap is null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if ((bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                && (bitmap.PixelFormat != PixelFormat.Format32bppPArgb))
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, bitmap.PixelFormat));
            }

            if ((bitmap.Width != buffer.Width) || (bitmap.Height != buffer.Height))
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_BITMAP_SIZE_NOT_CORRECT, bitmap.Width, bitmap.Height, buffer.Width, buffer.Height));
            }

            bool needsSwizzle;

            switch (buffer.Format)
            {
                case BufferFormat.R8G8B8A8_UNorm:
                case BufferFormat.R8G8B8A8_UNorm_SRgb:
                case BufferFormat.R8G8B8A8_SInt:
                case BufferFormat.R8G8B8A8_SNorm:
                case BufferFormat.R8G8B8A8_UInt:
                case BufferFormat.R8G8B8A8_Typeless:
                    needsSwizzle = true;
                    break;
                case BufferFormat.B8G8R8A8_UNorm:
                case BufferFormat.B8G8R8A8_Typeless:
                case BufferFormat.B8G8R8A8_UNorm_SRgb:
                case BufferFormat.B8G8R8X8_UNorm:
                case BufferFormat.B8G8R8X8_Typeless:
                case BufferFormat.B8G8R8X8_UNorm_SRgb:
                    needsSwizzle = false;
                    break;
                default:
                    throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, buffer.Format));
            }

            BitmapData srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                for (int y = 0; y < buffer.Height; ++y)
                {
                    GorgonPtr<byte> srcPtr = GetGdiImagePtr(srcData, y * srcData.Stride);
                    GorgonPtr<byte> destPtr = buffer.Data + (y * buffer.PitchInformation.RowPitch);

                    if (!needsSwizzle)
                    {
                        srcPtr.CopyTo(destPtr, count: srcData.Stride.Min(buffer.PitchInformation.RowPitch));
                        continue;
                    }

                    ImageUtilities.SwizzleScanline(in srcPtr, buffer.PitchInformation.RowPitch, in destPtr, srcData.Stride, buffer.Format, ImageBitFlags.None);
                }
            }
            finally
            {
                bitmap.UnlockBits(srcData);
            }
        }

        /// <summary>
        /// Function to convert a <see cref="Bitmap"/> into a <see cref="IGorgonImage"/>.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to convert.</param>
        /// <returns>A new <see cref="IGorgonImage"/> containing the data from the <see cref="Bitmap"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bitmap"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="bitmap"/> is not <see cref="PixelFormat.Format32bppArgb"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method will take a 2D <see cref="Bitmap"/> and copy its data into a new 2D <see cref="IGorgonImage"/>. The resulting <see cref="IGorgonImage"/> will only contain 1 array level, 
        /// and no mip map levels.
        /// </para>
        /// <para>
        /// Some format conversion is performed on the <paramref name="bitmap"/> when it is imported. The format conversion will always convert to the image format of 
        /// <see cref="BufferFormat.R8G8B8A8_UNorm"/>. Only the following GDI+ pixel formats are supported for conversion:
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="PixelFormat.Format32bppArgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="PixelFormat.Format32bppPArgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="PixelFormat.Format32bppRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="PixelFormat.Format24bppRgb"/></term>
        ///     </item>
        /// </list>
        /// If the source <paramref name="bitmap"/> does not support any of the formats on the list, then an exception will be thrown.
        /// </para>
        /// </remarks>
        public static IGorgonImage ToGorgonImage(this Bitmap bitmap)
        {
            if (bitmap is null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            if ((bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                && (bitmap.PixelFormat != PixelFormat.Format32bppPArgb)
                && (bitmap.PixelFormat != PixelFormat.Format32bppRgb)
                && (bitmap.PixelFormat != PixelFormat.Format24bppRgb))
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, bitmap.PixelFormat));
            }

            IGorgonImageInfo info = new GorgonImageInfo(ImageType.Image2D, BufferFormat.R8G8B8A8_UNorm)
            {
                Width = bitmap.Width,
                Height = bitmap.Height
            };

            IGorgonImage result = new GorgonImage(info);
            BitmapData bitmapLock = null;

            try
            {
                bitmapLock = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

                if ((bitmap.PixelFormat == PixelFormat.Format32bppArgb)
                    || (bitmap.PixelFormat == PixelFormat.Format32bppPArgb)
                    || (bitmap.PixelFormat == PixelFormat.Format32bppRgb))
                {
                    Transfer32Argb(bitmapLock, result.Buffers[0]);
                }
                else
                {
                    Transfer24Rgb(bitmapLock, result.Buffers[0]);
                }
            }
            catch
            {
                result.Dispose();
                throw;
            }
            finally
            {
                if (bitmapLock != null)
                {
                    bitmap.UnlockBits(bitmapLock);
                }
            }

            return result;
        }
    }
}
