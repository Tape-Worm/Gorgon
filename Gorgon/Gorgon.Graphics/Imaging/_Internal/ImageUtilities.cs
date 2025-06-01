
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
// Created: June 29, 2016 10:49:02 PM
// 

using Gorgon.Core;
#if GDI_PLUS
using Gorgon.Graphics.Imaging.GdiPlus.Properties;
#else
using Gorgon.Graphics.Imaging.Properties;
#endif
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// Utilities to facilitate in manipulating image data
/// </summary>
internal static class ImageUtilities
{
    /// <summary>
    /// Function to expand a 16BPP scan line in an image to a 32BPP RGBA line.
    /// </summary>
    /// <param name="src">The pointer to the source data.</param>
    /// <param name="srcPitch">The pitch of the source data.</param>
    /// <param name="srcFormat">Format to convert from.</param>
    /// <param name="dest">The pointer to the destination data.</param>
    /// <param name="destPitch">The pitch of the destination data.</param>
    /// <param name="bitFlags">Image bit conversion control flags.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="srcFormat" /> is not a 16 BPP format.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
    /// <remarks>
    /// <para>
    /// Use this to expand a 16 BPP (B5G6R5 or B5G5R5A1 format) into a 32 BPP R8G8B8A8 (normalized unsigned integer) format.
    /// </para>
    /// </remarks>
    public static void Expand16BPPScanline(GorgonPtr<byte> src, int srcPitch, BufferFormat srcFormat, GorgonPtr<byte> dest, int destPitch, ImageBitFlags bitFlags)
    {
        GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
        GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

        if (src == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(src));
        }

        if (dest == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
        {
            ushort srcPixel = (srcPtr++).Value;
            uint R = 0, G = 0, B = 0, A = 0;

            switch (srcFormat)
            {
                case BufferFormat.B5G6R5_UNorm:
                    R = (uint)((srcPixel & 0xF800) >> 11);
                    G = (uint)((srcPixel & 0x07E0) >> 5);
                    B = (uint)(srcPixel & 0x001F);
                    R = ((R << 3) | (R >> 2));
                    G = ((G << 2) | (G >> 4)) << 8;
                    B = ((B << 3) | (B >> 2)) << 16;
                    A = 0xFF000000;
                    break;
                case BufferFormat.B5G5R5A1_UNorm:
                    R = (uint)((srcPixel & 0x7C00) >> 10);
                    G = (uint)((srcPixel & 0x03E0) >> 5);
                    B = (uint)(srcPixel & 0x001F);
                    R = ((R << 3) | (R >> 2));
                    G = ((G << 3) | (G >> 2)) << 8;
                    B = ((B << 3) | (B >> 2)) << 16;
                    A = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
                            ? 0xFF000000
                            : (((srcPixel & 0x8000) != 0) ? 0xFF000000 : 0);
                    break;
                case BufferFormat.B4G4R4A4_UNorm:
                    A = (uint)((srcPixel & 0xF000) >> 12);
                    R = (uint)((srcPixel & 0xF00) >> 8);
                    G = (uint)((srcPixel & 0xF0) >> 4);
                    B = (uint)(srcPixel & 0xF);
                    R = ((R << 4) | R);
                    G = ((G << 4) | G) << 8;
                    B = ((B << 4) | B) << 16;
                    A = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
                            ? 0xFF000000
                            : ((A << 4) | A) << 24;
                    break;
            }

            (destPtr++).Value = R | G | B | A;
        }
    }

    /// <summary>
    /// Function to copy (or update in-place) with bits swizzled to match another format.
    /// </summary>
    /// <param name="src">The pointer to the source data.</param>
    /// <param name="srcPitch">The pitch of the source data.</param>
    /// <param name="dest">The pointer to the destination data.</param>
    /// <param name="destPitch">The pitch of the destination data.</param>
    /// <param name="format">Format of the destination buffer.</param>
    /// <param name="bitFlags">Image bit conversion control flags.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
    /// <remarks>
    /// <para>
    /// Use this method to copy a single scanline and swizzle the bits of an image and (optionally) set an opaque constant alpha value.
    /// </para>
    /// </remarks>
    public static void SwizzleScanline(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, int destPitch, BufferFormat format, ImageBitFlags bitFlags)
    {
        int size = srcPitch.Min(destPitch);
        uint r, g, b, a, pixel;

        if (src == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(src));
        }

        if (dest == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        if (format == BufferFormat.Unknown)
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format),
                                        nameof(format));
        }

        GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
        GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

        switch (format)
        {
            case BufferFormat.R10G10B10A2_Typeless:
            case BufferFormat.R10G10B10A2_UInt:
            case BufferFormat.R10G10B10A2_UNorm:
            case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
                for (int i = 0; i < size; i += 4)
                {
                    if (src != dest)
                    {
                        pixel = (srcPtr++).Value;
                    }
                    else
                    {
                        pixel = (destPtr).Value;
                    }

                    r = ((pixel & 0x3FF00000) >> 20);
                    g = (pixel & 0x000FFC00);
                    b = ((pixel & 0x000003FF) << 20);
                    a = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xC0000000 : pixel & 0xC0000000;

                    (destPtr++).Value = r | g | b | a;
                }
                return;
            case BufferFormat.R8G8B8A8_Typeless:
            case BufferFormat.R8G8B8A8_UNorm:
            case BufferFormat.R8G8B8A8_UNorm_SRgb:
            case BufferFormat.B8G8R8A8_UNorm:
            case BufferFormat.B8G8R8X8_UNorm:
            case BufferFormat.R8G8B8A8_UInt:
            case BufferFormat.R8G8B8A8_SInt:
            case BufferFormat.B8G8R8A8_Typeless:
            case BufferFormat.B8G8R8A8_UNorm_SRgb:
            case BufferFormat.B8G8R8X8_Typeless:
            case BufferFormat.B8G8R8X8_UNorm_SRgb:
                for (int i = 0; i < size; i += 4)
                {
                    if (src != dest)
                    {
                        pixel = (srcPtr++).Value;
                    }
                    else
                    {
                        pixel = (destPtr).Value;
                    }

                    r = ((pixel & 0xFF0000) >> 16);
                    g = (pixel & 0x00FF00);
                    b = ((pixel & 0x0000FF) << 16);
                    a = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : pixel & 0xFF000000;

                    (destPtr++).Value = r | g | b | a;
                }
                return;
        }

        if (src != dest)
        {
            src.CopyTo(dest);
        }
    }

    /// <summary>
    /// Function to copy a scanline from the source to the destination.
    /// </summary>
    /// <param name="src">The source data to copy.</param>
    /// <param name="srcPitch">The pitch of the source data scanline.</param>
    /// <param name="dest">The destination buffer that will receive the copied data.</param>
    /// <param name="format">The format used to copy.</param>
    /// <param name="flipHorizontal"><b>true</b> to write horizontal pixel values from right to left, or <b>false</b> to write left to right.</param>
    /// <returns><b>true</b> if the line contains all 0 alpha values, <b>false</b> if not.</returns>
    public static bool CopyScanline(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, BufferFormat format, bool flipHorizontal)
    {
        bool result = true;

        // Do a straight copy.
        switch (format)
        {
            case BufferFormat.R32G32B32A32_Typeless:
            case BufferFormat.R32G32B32A32_Float:
            case BufferFormat.R32G32B32A32_UInt:
            case BufferFormat.R32G32B32A32_SInt:
                {
                    uint alphaMask = (format == BufferFormat.R32G32B32A32_Float)
                                         ? 0x3F800000
                                         : ((format == BufferFormat.R32G32B32A32_SInt) ? 0x7FFFFFFF : 0xFFFFFFFF);

                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < srcPitch; i += 16)
                    {
                        uint alpha = (srcPtr + 3).Value & alphaMask;

                        if (alpha != 0)
                        {
                            result = false;
                        }

                        // If not in place copy, then copy to destination.
                        if (dest == src)
                        {
                            continue;
                        }

                        if (!flipHorizontal)
                        {
                            (destPtr++).Value = (srcPtr++).Value;
                            (destPtr++).Value = (srcPtr++).Value;
                            (destPtr++).Value = (srcPtr++).Value;
                            (destPtr++).Value = (srcPtr++).Value;
                        }
                        else
                        {
                            (destPtr - 3).Value = (srcPtr++).Value;
                            (destPtr - 2).Value = (srcPtr++).Value;
                            (destPtr - 1).Value = (srcPtr++).Value;
                            (destPtr).Value = (srcPtr++).Value;
                            destPtr -= 4;
                        }
                    }
                }
                return result;
            case BufferFormat.R16G16B16A16_Typeless:
            case BufferFormat.R16G16B16A16_Float:
            case BufferFormat.R16G16B16A16_UNorm:
            case BufferFormat.R16G16B16A16_UInt:
            case BufferFormat.R16G16B16A16_SNorm:
            case BufferFormat.R16G16B16A16_SInt:
                {
                    uint alphaMask = 0xFFFF0000;

                    switch (format)
                    {
                        case BufferFormat.R16G16B16A16_SInt:
                        case BufferFormat.R16G16B16A16_SNorm:
                            alphaMask = 0x7FFF0000;
                            break;
                    }

                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < srcPitch; i += 8)
                    {
                        uint alpha = (srcPtr + 1).Value & alphaMask;

                        if (alpha != 0)
                        {
                            result = false;
                        }

                        // If not in-place copy, then copy from the source.
                        if (src == dest)
                        {
                            continue;
                        }

                        if (!flipHorizontal)
                        {
                            (destPtr++).Value = (srcPtr++).Value;
                            (destPtr++).Value = (srcPtr++).Value;
                        }
                        else
                        {
                            (destPtr - 1).Value = (srcPtr++).Value;
                            (destPtr).Value = (srcPtr++).Value;
                            destPtr -= 2;
                        }
                    }
                }
                return result;
            case BufferFormat.R10G10B10A2_Typeless:
            case BufferFormat.R10G10B10A2_UNorm:
            case BufferFormat.R10G10B10A2_UInt:
            case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < srcPitch; i += 4)
                    {
                        uint pixel = (srcPtr++).Value;
                        uint alpha = pixel & 0xC0000000;

                        if (alpha != 0)
                        {
                            result = false;
                        }

                        if (dest == src)
                        {
                            continue;
                        }

                        if (!flipHorizontal)
                        {
                            (destPtr++).Value = pixel;
                        }
                        else
                        {
                            (destPtr--).Value = pixel;
                        }
                    }
                }
                return result;
            case BufferFormat.R8G8B8A8_Typeless:
            case BufferFormat.R8G8B8A8_UNorm:
            case BufferFormat.R8G8B8A8_UNorm_SRgb:
            case BufferFormat.R8G8B8A8_UInt:
            case BufferFormat.R8G8B8A8_SNorm:
            case BufferFormat.R8G8B8A8_SInt:
            case BufferFormat.B8G8R8A8_UNorm:
            case BufferFormat.B8G8R8A8_Typeless:
            case BufferFormat.B8G8R8A8_UNorm_SRgb:
                {
                    uint alphaMask = (format is BufferFormat.R8G8B8A8_SInt or BufferFormat.R8G8B8A8_SNorm) ? 0x7F000000 : 0xFF000000;

                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < srcPitch; i += 4)
                    {
                        uint pixel = (srcPtr++).Value;
                        uint alpha = pixel & alphaMask;

                        if (alpha != 0)
                        {
                            result = false;
                        }

                        if (src == dest)
                        {
                            continue;
                        }

                        if (!flipHorizontal)
                        {
                            (destPtr++).Value = pixel;
                        }
                        else
                        {
                            (destPtr--).Value = pixel;
                        }
                    }
                }
                return result;
            case BufferFormat.B5G5R5A1_UNorm:
            case BufferFormat.B4G4R4A4_UNorm:
                {
                    ushort alphaMask = (ushort)(format == BufferFormat.B5G5R5A1_UNorm ? 0x8000 : 0xF000);
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < srcPitch; i += 2)
                    {
                        ushort pixel = (srcPtr++).Value;
                        int alpha = pixel & alphaMask;

                        if (alpha != 0)
                        {
                            result = false;
                        }

                        if (src == dest)
                        {
                            continue;
                        }

                        // If not in-place copy, then copy from the source.
                        if (!flipHorizontal)
                        {
                            (destPtr++).Value = pixel;
                        }
                        else
                        {
                            (destPtr--).Value = pixel;
                        }
                    }
                }
                return result;
            case BufferFormat.R8_UNorm:
            case BufferFormat.B5G6R5_UNorm:
                if (dest == src)
                {
                    return false;
                }

                if (flipHorizontal)
                {
                    GorgonPtr<byte> srcPtr = src;
                    GorgonPtr<byte> destPtr = dest;

                    for (int x = 0; x < srcPitch; ++x)
                    {
                        (destPtr--).Value = (srcPtr++).Value;
                    }
                    return false;
                }

                src.CopyTo(dest);

                return false;
            case BufferFormat.A8_UNorm:
                {
                    GorgonPtr<byte> srcPtr = src;
                    GorgonPtr<byte> destPtr = dest;

                    for (int x = 0; x < srcPitch; ++x)
                    {
                        byte alpha = (srcPtr++).Value;

                        if (alpha != 0)
                        {
                            result = false;
                        }

                        if (dest == src)
                        {
                            continue;
                        }

                        if (!flipHorizontal)
                        {
                            (destPtr++).Value = alpha;
                        }
                        else
                        {
                            (destPtr--).Value = alpha;
                        }
                    }
                }
                return result;
        }

        return false;
    }

    /// <summary>
    /// Function to copy (or update in-place) a line with opaque alpha substituion (if required).
    /// </summary>
    /// <param name="src">The pointer to the source data.</param>
    /// <param name="srcPitch">The pitch of the source data.</param>
    /// <param name="dest">The pointer to the destination data.</param>
    /// <param name="destPitch">The pitch of the destination data.</param>
    /// <param name="format">Format of the destination buffer.</param>
    /// <param name="bitFlags">Image bit conversion control flags.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
    /// <remarks>Use this method to copy a single scanline of an image and (optionally) set an opaque constant alpha value.</remarks>
    public static void CopyScanline(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, int destPitch, BufferFormat format, ImageBitFlags bitFlags)
    {
        if (src == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(src));
        }

        if (dest == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        if (format == BufferFormat.Unknown)
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
        }

        int size = (src == dest) ? destPitch : (srcPitch.Min(destPitch));

        if ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
        {
            // Do a straight copy.
            switch (format)
            {
                case BufferFormat.R32G32B32A32_Typeless:
                case BufferFormat.R32G32B32A32_Float:
                case BufferFormat.R32G32B32A32_UInt:
                case BufferFormat.R32G32B32A32_SInt:
                    {
                        uint alpha = (format == BufferFormat.R32G32B32A32_Float) ? 0x3F800000
                                            : ((format == BufferFormat.R32G32B32A32_SInt) ? 0x7FFFFFFF : 0xFFFFFFFF);

                        GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                        GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                        for (int i = 0; i < size; i += 16)
                        {
                            // If not in-place copy, then copy from the source.
                            if (src != dest)
                            {
                                (destPtr++).Value = (srcPtr++).Value;
                                (destPtr++).Value = (srcPtr++).Value;
                                (destPtr++).Value = (srcPtr++).Value;
                            }

                            (destPtr++).Value = alpha;
                        }
                    }
                    return;
                case BufferFormat.R16G16B16A16_Typeless:
                case BufferFormat.R16G16B16A16_Float:
                case BufferFormat.R16G16B16A16_UNorm:
                case BufferFormat.R16G16B16A16_UInt:
                case BufferFormat.R16G16B16A16_SNorm:
                case BufferFormat.R16G16B16A16_SInt:
                    {
                        ushort alpha = 0xFFFF;

                        switch (format)
                        {
                            case BufferFormat.R16G16B16A16_Float:
                                alpha = 0x3C00;
                                break;
                            case BufferFormat.R16G16B16A16_SInt:
                            case BufferFormat.R16G16B16A16_SNorm:
                                alpha = 0x7FFF;
                                break;
                        }

                        GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                        GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                        for (int i = 0; i < size; i += 8)
                        {
                            // If not in-place copy, then copy from the source.
                            if (src != dest)
                            {
                                destPtr.Value = srcPtr.Value;
                                srcPtr += 4;
                            }
                            destPtr += 3;
                            (destPtr++).Value = alpha;
                        }
                    }
                    return;
                case BufferFormat.R10G10B10A2_Typeless:
                case BufferFormat.R10G10B10A2_UNorm:
                case BufferFormat.R10G10B10A2_UInt:
                case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
                    {
                        GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                        GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                        for (int i = 0; i < size; i += 4)
                        {
                            // If not in-place copy, then copy from the source.
                            if (src != dest)
                            {
                                destPtr.Value = (srcPtr.Value) & 0x3FFFFFFF;
                                srcPtr++;
                            }
                            destPtr.Value |= 0xC0000000;
                            destPtr++;
                        }
                    }
                    return;
                case BufferFormat.R8G8B8A8_Typeless:
                case BufferFormat.R8G8B8A8_UNorm:
                case BufferFormat.R8G8B8A8_UNorm_SRgb:
                case BufferFormat.R8G8B8A8_UInt:
                case BufferFormat.R8G8B8A8_SNorm:
                case BufferFormat.R8G8B8A8_SInt:
                case BufferFormat.B8G8R8A8_UNorm:
                case BufferFormat.B8G8R8A8_Typeless:
                case BufferFormat.B8G8R8A8_UNorm_SRgb:
                    {
                        uint alpha = (format is BufferFormat.R8G8B8A8_SInt or BufferFormat.R8G8B8A8_SNorm) ? 0x7F000000 : 0xFF000000;

                        GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                        GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                        for (int i = 0; i < size; i += 4)
                        {
                            // If not in-place copy, then copy from the source.
                            if (src != dest)
                            {
                                destPtr.Value = (srcPtr.Value) & 0xFFFFFF;
                                srcPtr++;
                            }
                            destPtr.Value |= alpha;
                            destPtr++;
                        }
                    }
                    return;
                case BufferFormat.B4G4R4A4_UNorm:
                    {
                        GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                        GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                        for (int i = 0; i < size; i += 2)
                        {
                            // If not in-place copy, then copy from the source.
                            if (src != dest)
                            {
                                (destPtr++).Value = (ushort)((srcPtr++).Value | 0xF000);
                            }
                            else
                            {
                                (destPtr++).Value |= 0xF000;
                            }
                        }
                    }
                    return;
                case BufferFormat.B5G5R5A1_UNorm:
                    {
                        GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                        GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                        for (int i = 0; i < size; i += 2)
                        {
                            // If not in-place copy, then copy from the source.
                            if (src != dest)
                            {
                                (destPtr++).Value = (ushort)((srcPtr++).Value | 0x8000);
                            }
                            else
                            {
                                (destPtr++).Value |= 0x8000;
                            }
                        }
                    }
                    return;
                case BufferFormat.A8_UNorm:
                    dest.Fill(0xff);
                    return;
            }
        }

        // Copy if not doing an in-place update.
        if (dest != src)
        {
            src.CopyTo(dest);
        }
    }

    /// <summary>
    /// Function to update a line with opaque alpha substituion.
    /// </summary>
    /// <param name="ptr">The pointer to the image data.</param>
    /// <param name="pitch">The pitch of the image data.</param>
    /// <param name="format">Format of the destination buffer.</param>
    /// <param name="alphaValue">The alpha value to set.</param>
    /// <param name="minAlpha">The minimum alpha value to overwrite.</param>
    /// <param name="maxAlpha">The maximum alpha value to overwrite.</param>
    /// <remarks>Use this method to copy a single scanline of an image and (optionally) set an opaque constant alpha value.</remarks>
    public static void SetAlphaScanline(GorgonPtr<byte> ptr, int pitch, BufferFormat format, float alphaValue, float minAlpha, float maxAlpha)
    {
        switch (format)
        {
            case BufferFormat.R32G32B32A32_Float:
                {
                    GorgonPtr<float> srcPtr = (GorgonPtr<float>)ptr;

                    for (int i = 0; i < pitch; i += 16)
                    {
                        float srcAlpha = (srcPtr + 3).Value;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            (srcPtr + 3).Value = alphaValue;
                        }

                        srcPtr += 4;
                    }
                }
                break;
            case BufferFormat.R32G32B32A32_Typeless:
            case BufferFormat.R32G32B32A32_UInt:
            case BufferFormat.R32G32B32A32_SInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)ptr;

                    for (int i = 0; i < pitch; i += 16)
                    {
                        uint srcAlpha = (srcPtr + 3).Value;

                        if ((srcAlpha >= (uint)minAlpha) && (srcAlpha <= (uint)maxAlpha))
                        {
                            (srcPtr + 3).Value = (uint)alphaValue;
                        }

                        srcPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_Float:
                {
                    GorgonPtr<Half> srcPtr = (GorgonPtr<Half>)ptr;
                    Half alpha = (Half)alphaValue;
                    Half min = (Half)minAlpha;
                    Half max = (Half)maxAlpha;

                    for (int i = 0; i < pitch; i += 8)
                    {
                        Half srcAlpha = (srcPtr + 3).Value;

                        if ((srcAlpha >= min) && (srcAlpha <= max))
                        {
                            (srcPtr + 3).Value = alpha;
                        }

                        srcPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_Typeless:
            case BufferFormat.R16G16B16A16_UNorm:
            case BufferFormat.R16G16B16A16_UInt:
            case BufferFormat.R16G16B16A16_SNorm:
            case BufferFormat.R16G16B16A16_SInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;

                    for (int i = 0; i < pitch; i += 8)
                    {
                        ushort srcAlpha = (srcPtr + 3).Value;

                        if ((srcAlpha >= (ushort)minAlpha) && (srcAlpha <= (ushort)maxAlpha))
                        {
                            (srcPtr + 3).Value = (ushort)alphaValue;
                        }

                        srcPtr += 4;
                    }
                }
                return;
            case BufferFormat.R10G10B10A2_Typeless:
            case BufferFormat.R10G10B10A2_UNorm:
            case BufferFormat.R10G10B10A2_UInt:
            case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)ptr;

                    for (int i = 0; i < pitch; i += 4)
                    {
                        uint srcAlpha = (((srcPtr.Value) & 0xC0000000) >> 30) & 3;

                        if ((srcAlpha >= (uint)minAlpha) && (srcAlpha <= (uint)maxAlpha))
                        {
                            uint destValue = srcPtr.Value & 0x3FFFFFFF;
                            srcPtr.Value = (destValue | ((uint)alphaValue & 3) << 30);
                        }

                        ++srcPtr;
                    }
                }
                return;
            case BufferFormat.R8G8B8A8_Typeless:
            case BufferFormat.R8G8B8A8_UNorm:
            case BufferFormat.R8G8B8A8_UNorm_SRgb:
            case BufferFormat.R8G8B8A8_UInt:
            case BufferFormat.B8G8R8A8_Typeless:
            case BufferFormat.B8G8R8A8_UNorm:
            case BufferFormat.B8G8R8A8_UNorm_SRgb:
            case BufferFormat.R8G8B8A8_SNorm:
            case BufferFormat.R8G8B8A8_SInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)ptr;

                    for (int i = 0; i < pitch; i += 4)
                    {
                        uint srcAlpha = ((srcPtr.Value) & 0xFF000000) >> 24;

                        if ((srcAlpha >= (uint)minAlpha) && (srcAlpha <= (uint)maxAlpha))
                        {
                            uint destValue = (srcPtr.Value) & 0xFFFFFF;
                            srcPtr.Value = destValue | ((uint)alphaValue & 0xFF) << 24;
                        }
                        ++srcPtr;
                    }
                }
                break;
            case BufferFormat.B5G5R5A1_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;

                    for (int i = 0; i < pitch; i += 2)
                    {
                        ushort destValue = (ushort)(srcPtr.Value & 0x7fff);
                        srcPtr.Value = (ushort)(destValue | (ushort)(((ushort)alphaValue & 0x1) << 15));
                        ++srcPtr;
                    }
                }
                return;
            case BufferFormat.B4G4R4A4_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;

                    for (int i = 0; i < pitch; i += 2)
                    {
                        ushort srcAlpha = (ushort)(((srcPtr.Value) & 0xF000) >> 12);

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            ushort destValue = (ushort)((srcPtr.Value) & 0xFFF);
                            srcPtr.Value = (ushort)(destValue | (ushort)(((ushort)alphaValue & 0xF) << 12));
                        }
                        ++srcPtr;
                    }
                }
                return;
        }
    }

    /// <summary>
    /// Function to update a line with premultiplied alpha.
    /// </summary>
    /// <param name="src">The pointer to the source data.</param>
    /// <param name="srcPitch">The pitch of the source data.</param>
    /// <param name="dest">The pointer to the destination data.</param>
    /// <param name="destPitch">The pitch of the destination data.</param>
    /// <param name="format">Format of the destination buffer.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
    /// <remarks>Use this method to copy a single scanline of an image and (optionally) set an opaque constant alpha value.</remarks>
    public static void SetPremultipliedScanline(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, int destPitch, BufferFormat format)
    {
        if (src == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(src));
        }

        if (dest == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        if (format == BufferFormat.Unknown)
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
        }

        int size = (src == dest) ? destPitch : (srcPitch.Min(destPitch));

        switch (format)
        {
            case BufferFormat.R32G32B32A32_SInt:
            case BufferFormat.R32G32B32A32_Typeless:
            case BufferFormat.R32G32B32A32_UInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        uint srcAlpha = (srcPtr + 3).Value;

                        if (srcAlpha == 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        uint c1 = srcPtr.Value / srcAlpha;
                        uint c2 = ((srcPtr + 1).Value) / srcAlpha;
                        uint c3 = ((srcPtr + 2).Value) / srcAlpha;

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R32G32B32A32_Float:
                {
                    GorgonPtr<float> srcPtr = (GorgonPtr<float>)src;
                    GorgonPtr<float> destPtr = (GorgonPtr<float>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        float srcAlpha = (srcPtr + 3).Value;

                        float c1 = srcPtr.Value * srcAlpha;
                        float c2 = ((srcPtr + 1).Value) * srcAlpha;
                        float c3 = ((srcPtr + 2).Value) * srcAlpha;

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_SNorm:
            case BufferFormat.R16G16B16A16_SInt:
            case BufferFormat.R16G16B16A16_Typeless:
            case BufferFormat.R16G16B16A16_UNorm:
            case BufferFormat.R16G16B16A16_UInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        ushort srcAlpha = (srcPtr + 3).Value;

                        if (srcAlpha == 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        ushort c1 = (ushort)(srcPtr.Value / srcAlpha);
                        ushort c2 = (ushort)(((srcPtr + 1).Value) / srcAlpha);
                        ushort c3 = (ushort)(((srcPtr + 2).Value) / srcAlpha);

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_Float:
                {
                    GorgonPtr<Half> srcPtr = (GorgonPtr<Half>)src;
                    GorgonPtr<Half> destPtr = (GorgonPtr<Half>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        Half srcAlpha = (srcPtr + 3).Value;

                        Half c1 = (srcPtr.Value * srcAlpha);
                        Half c2 = (((srcPtr + 1).Value) * srcAlpha);
                        Half c3 = (((srcPtr + 2).Value) * srcAlpha);

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R10G10B10A2_Typeless:
            case BufferFormat.R10G10B10A2_UNorm:
            case BufferFormat.R10G10B10A2_UInt:
            case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 4)
                    {
                        uint pixel = srcPtr.Value;
                        uint color = pixel & 0x3FFFFFFF;
                        float srcAlpha = (((pixel & 0xC0000000) >> 30) & 3) / 4.0f;

                        uint c1 = (uint)(((color >> 20) & 0x3FF) * srcAlpha);
                        uint c2 = (uint)(((color >> 10) & 0x3FF) * srcAlpha);
                        uint c3 = (uint)((color & 0x3ff) * srcAlpha);

                        color = (c1 << 20) | (c2 << 10) | c3;

                        destPtr.Value = (pixel & 0xC0000000) | color;

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
            case BufferFormat.R8G8B8A8_SNorm:
            case BufferFormat.R8G8B8A8_SInt:
            case BufferFormat.R8G8B8A8_Typeless:
            case BufferFormat.R8G8B8A8_UNorm:
            case BufferFormat.R8G8B8A8_UNorm_SRgb:
            case BufferFormat.R8G8B8A8_UInt:
            case BufferFormat.B8G8R8A8_Typeless:
            case BufferFormat.B8G8R8A8_UNorm:
            case BufferFormat.B8G8R8A8_UNorm_SRgb:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 4)
                    {
                        uint pixel = srcPtr.Value;
                        uint color = pixel & 0xFFFFFF;
                        float srcAlpha = ((pixel & 0xFF000000) >> 24) / 255.0f;

                        uint c1 = (uint)(((color >> 16) & 0xFF) * srcAlpha);
                        uint c2 = (uint)(((color >> 8) & 0xFF) * srcAlpha);
                        uint c3 = (uint)((color & 0xFF) * srcAlpha);

                        color = (c1 << 16) | (c2 << 8) | c3;

                        destPtr.Value = (pixel & 0xFF000000) | color;

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
            case BufferFormat.B4G4R4A4_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 4)
                    {
                        ushort pixel = srcPtr.Value;
                        ushort color = (ushort)(pixel & 0xfff);
                        float srcAlpha = ((pixel & 0xF000) >> 12) / 16.0f;

                        ushort c1 = (ushort)(((color >> 8) & 0xF) * srcAlpha);
                        ushort c2 = (ushort)(((color >> 4) & 0xF) * srcAlpha);
                        ushort c3 = (ushort)((color & 0xF) * srcAlpha);

                        color = (ushort)((c1 << 8) | (c2 << 4) | c3);

                        destPtr.Value = (ushort)((pixel & 0xF000) | color);

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
        }
    }

    /// <summary>
    /// Function to update a line with premultiplied alpha.
    /// </summary>
    /// <param name="src">The pointer to the source data.</param>
    /// <param name="srcPitch">The pitch of the source data.</param>
    /// <param name="dest">The pointer to the destination data.</param>
    /// <param name="destPitch">The pitch of the destination data.</param>
    /// <param name="format">Format of the destination buffer.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
    /// <remarks>Use this method to copy a single scanline of an image and (optionally) set an opaque constant alpha value.</remarks>
    public static void RemovePremultipliedScanline(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, int destPitch, BufferFormat format)
    {
        if (src == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(src));
        }

        if (dest == GorgonPtr<byte>.NullPtr)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        if (format == BufferFormat.Unknown)
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
        }

        int size = (src == dest) ? destPitch : (srcPitch.Min(destPitch));

        switch (format)
        {
            case BufferFormat.R32G32B32A32_SInt:
            case BufferFormat.R32G32B32A32_Typeless:
            case BufferFormat.R32G32B32A32_UInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        uint srcAlpha = (srcPtr + 3).Value;

                        if (srcAlpha == 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        uint c1 = srcPtr.Value * srcAlpha;
                        uint c2 = ((srcPtr + 1).Value) * srcAlpha;
                        uint c3 = ((srcPtr + 2).Value) * srcAlpha;

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R32G32B32A32_Float:
                {
                    GorgonPtr<float> srcPtr = (GorgonPtr<float>)src;
                    GorgonPtr<float> destPtr = (GorgonPtr<float>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        float srcAlpha = (srcPtr + 3).Value;
                        bool zeroAlpha = srcAlpha.EqualsEpsilon(0);

                        float c1 = zeroAlpha ? 0 : srcPtr.Value / srcAlpha;
                        float c2 = zeroAlpha ? 0 : ((srcPtr + 1).Value) / srcAlpha;
                        float c3 = zeroAlpha ? 0 : ((srcPtr + 2).Value) / srcAlpha;

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_SNorm:
            case BufferFormat.R16G16B16A16_SInt:
            case BufferFormat.R16G16B16A16_Typeless:
            case BufferFormat.R16G16B16A16_UNorm:
            case BufferFormat.R16G16B16A16_UInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        ushort srcAlpha = (srcPtr + 3).Value;

                        if (srcAlpha == 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        ushort c1 = (ushort)(srcPtr.Value * srcAlpha);
                        ushort c2 = (ushort)(((srcPtr + 1).Value) * srcAlpha);
                        ushort c3 = (ushort)(((srcPtr + 2).Value) * srcAlpha);

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_Float:
                {
                    GorgonPtr<Half> srcPtr = (GorgonPtr<Half>)src;
                    GorgonPtr<Half> destPtr = (GorgonPtr<Half>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        Half srcAlpha = ((srcPtr + 3).Value);
                        bool zeroAlpha = srcAlpha == Half.Zero;

                        Half c1 = zeroAlpha ? Half.Zero : (srcPtr.Value / srcAlpha);
                        Half c2 = zeroAlpha ? Half.Zero : (((srcPtr + 1).Value) / srcAlpha);
                        Half c3 = zeroAlpha ? Half.Zero : (((srcPtr + 2).Value) / srcAlpha);

                        destPtr.Value = c1;
                        ((destPtr + 1).Value) = c2;
                        ((destPtr + 2).Value) = c3;

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R10G10B10A2_Typeless:
            case BufferFormat.R10G10B10A2_UNorm:
            case BufferFormat.R10G10B10A2_UInt:
            case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 4)
                    {
                        uint pixel = srcPtr.Value;
                        uint color = pixel & 0x3FFFFFFF;
                        float srcAlpha = (((pixel & 0xC0000000) >> 30) & 3) / 4.0f;
                        bool zeroAlpha = srcAlpha.EqualsEpsilon(0);

                        uint c1 = zeroAlpha ? 0 : (uint)(((color >> 20) & 0x3FF) / srcAlpha);
                        uint c2 = zeroAlpha ? 0 : (uint)(((color >> 10) & 0x3FF) / srcAlpha);
                        uint c3 = zeroAlpha ? 0 : (uint)((color & 0x3ff) / srcAlpha);

                        color = (c1 << 20) | (c2 << 10) | c3;

                        destPtr.Value = (pixel & 0xC0000000) | color;

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
            case BufferFormat.R8G8B8A8_SNorm:
            case BufferFormat.R8G8B8A8_SInt:
            case BufferFormat.R8G8B8A8_Typeless:
            case BufferFormat.R8G8B8A8_UNorm:
            case BufferFormat.R8G8B8A8_UNorm_SRgb:
            case BufferFormat.R8G8B8A8_UInt:
            case BufferFormat.B8G8R8A8_Typeless:
            case BufferFormat.B8G8R8A8_UNorm:
            case BufferFormat.B8G8R8A8_UNorm_SRgb:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 4)
                    {
                        uint pixel = srcPtr.Value;
                        uint color = pixel & 0xFFFFFF;
                        float srcAlpha = ((pixel & 0xFF000000) >> 24) / 255.0f;
                        bool zeroAlpha = srcAlpha.EqualsEpsilon(0);

                        uint c1 = zeroAlpha ? 0 : (uint)(((color >> 16) & 0xFF) / srcAlpha);
                        uint c2 = zeroAlpha ? 0 : (uint)(((color >> 8) & 0xFF) / srcAlpha);
                        uint c3 = zeroAlpha ? 0 : (uint)((color & 0xFF) / srcAlpha);

                        color = (c1 << 16) | (c2 << 8) | c3;

                        destPtr.Value = (pixel & 0xFF000000) | color;

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
            case BufferFormat.B4G4R4A4_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 4)
                    {
                        ushort pixel = srcPtr.Value;
                        ushort color = (ushort)(pixel & 0xfff);
                        float srcAlpha = ((pixel & 0xF000) >> 12) / 16.0f;
                        bool zeroAlpha = srcAlpha.EqualsEpsilon(0);

                        ushort c1 = zeroAlpha ? (ushort)0 : (ushort)(((color >> 8) & 0xF) / srcAlpha);
                        ushort c2 = zeroAlpha ? (ushort)0 : (ushort)(((color >> 4) & 0xF) / srcAlpha);
                        ushort c3 = zeroAlpha ? (ushort)0 : (ushort)((color & 0xF) / srcAlpha);

                        color = (ushort)((c1 << 8) | (c2 << 4) | c3);

                        destPtr.Value = (ushort)((pixel & 0xF000) | color);

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
        }
    }

    /// <summary>
    /// Function to expand a 24 bit per pixel scanline into a 32 bit per pixel scanline.
    /// </summary>
    /// <param name="src">The source data to expand.</param>
    /// <param name="srcPitch">The number of bytes for a scanline in the source data.</param>
    /// <param name="dest">The pointer to the destination buffer to fill.</param>
    /// <param name="reverse"><b>true</b> to fill the destination from the right side, <b>false</b> to fill from the left.</param>
    public static void Expand24BPPScanLine(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, bool reverse)
    {
        GorgonPtr<byte> srcPtr = src;
        GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

        for (int x = 0; x < srcPitch; x += 3)
        {
            uint pixel = (uint)(((srcPtr++).Value) | ((srcPtr++).Value << 8) | ((srcPtr++).Value << 16) | 0xFF000000);

            if (reverse)
            {
                (destPtr--).Value = pixel;
            }
            else
            {
                (destPtr++).Value = pixel;
            }
        }
    }

    /// <summary>
    /// Function to compress a 32 bit scanline to a 24 bit bit scanline.
    /// </summary>
    /// <param name="src">The pointer to the source data.</param>
    /// <param name="srcPitch">The pitch of the source data.</param>
    /// <param name="dest">The pointer to the destination data.</param>
    /// <param name="destPitch">The pitch of the destination data.</param>
    /// <param name="swizzle"><b>true</b> to swap the R and B components, <b>false</b> to leave as is.</param>
    public static void Compress24BPPScanLine(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, int destPitch, bool swizzle)
    {
        GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
        GorgonPtr<byte> destPtr = dest;
        GorgonPtr<byte> endPtr = destPtr + destPitch;

        for (int srcCount = 0; srcCount < srcPitch; srcCount += 4)
        {
            uint pixel = (srcPtr++).Value;

            // Ensure we don't have a buffer overrun.
            if (destPtr + 2 > endPtr)
            {
                return;
            }

            (destPtr++).Value = (byte)(swizzle ? ((pixel & 0xFF0000) >> 16) : (pixel & 0xFF));       //R (or B)
            (destPtr++).Value = (byte)((pixel & 0xFF00) >> 8);                                   //G
            (destPtr++).Value = (byte)(swizzle ? (pixel & 0xFF) : ((pixel & 0xFF0000) >> 16));     //B (or R)
        }
    }
}
