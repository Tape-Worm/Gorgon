
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
                case BufferFormat.A4B4G4R4_UNorm:
                    B = (uint)((srcPixel & 0xF000) >> 12);
                    R = (uint)((srcPixel & 0xF00) >> 8);
                    G = (uint)((srcPixel & 0xF0) >> 4);
                    A = (uint)(srcPixel & 0xF);
                    R = ((R << 4) | R);
                    G = ((G << 4) | G) << 8;
                    B = ((B << 4) | B) << 16;
                    A = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
                            ? 0xFF000000
                            : ((A << 4) | A) << 24;
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
    /// <returns><b>true</b> if the line contains all 0 alpha values, <b>false</b> if not.</returns>
    public static bool CopyScanline(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest, BufferFormat format)
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
                        if (dest != src)
                        {
                            (destPtr++).Value = srcPtr.Value;
                            (destPtr++).Value = (srcPtr + 1).Value;
                            (destPtr++).Value = (srcPtr + 2).Value;
                            (destPtr++).Value = (srcPtr + 3).Value;
                        }

                        srcPtr += 4;
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
                        if (src != dest)
                        {
                            (destPtr++).Value = srcPtr.Value;
                            (destPtr++).Value = (srcPtr + 1).Value;
                        }

                        srcPtr += 2;
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

                        (destPtr++).Value = pixel;
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

                        (destPtr++).Value = pixel;
                    }
                }
                return result;
            case BufferFormat.B5G5R5A1_UNorm:
            case BufferFormat.B4G4R4A4_UNorm:
            case BufferFormat.A4B4G4R4_UNorm:
                {
                    ushort alphaMask = format switch
                    {
                        BufferFormat.B5G5R5A1_UNorm => 0x8000,
                        BufferFormat.B4G4R4A4_UNorm => 0xF000,
                        BufferFormat.A4B4G4R4_UNorm => 0xF,
                        _ => 0
                    };
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

                        // If not in-place copy, then copy from the source.
                        if (src == dest)
                        {
                            continue;
                        }

                        (destPtr++).Value = pixel;
                    }
                }
                return result;
            case BufferFormat.R8_UNorm:
            case BufferFormat.B5G6R5_UNorm:
                if (dest == src)
                {
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

                        (destPtr++).Value = alpha;
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
                case BufferFormat.A4B4G4R4_UNorm:
                    {
                        GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                        GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                        for (int i = 0; i < size; i += 2)
                        {
                            // If not in-place copy, then copy from the source.
                            if (src != dest)
                            {
                                (destPtr++).Value = (ushort)((srcPtr++).Value | 0xF);
                            }
                            else
                            {
                                (destPtr++).Value |= 0xF;
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
    /// Function to set the alpha channel on a scanline.
    /// </summary>
    /// <param name="ptr">The pointer to the image data.</param>
    /// <param name="pitch">The number of bytes in the scanline to update.</param>
    /// <param name="format">Format of the destination buffer.</param>
    /// <param name="alphaValue">The normalized alpha value to set (0 to 1 for unsigned formats, -1 to 1 for signed formats).</param>
    /// <param name="minAlpha">The minimum normalized alpha value to overwrite.</param>
    /// <param name="maxAlpha">The maximum normalized alpha value to overwrite.</param>
    public static void SetAlphaScanline(GorgonPtr<byte> ptr, int pitch, BufferFormat format, float alphaValue, float minAlpha, float maxAlpha)
    {
        // Each format converts the alpha value into its own channel units once, and converts each pixel's alpha back into a normalized value before comparing it
        // with the range. Signed normalized formats treat their most negative value as -1, so those are clamped before the comparison.
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
                return;
            case BufferFormat.R32G32B32A32_Typeless:
            case BufferFormat.R32G32B32A32_UInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)ptr;
                    uint alpha = (uint)(alphaValue.Max(0.0f).Min(1.0f) * (double)uint.MaxValue).Round();

                    for (int i = 0; i < pitch; i += 16)
                    {
                        double srcAlpha = (srcPtr + 3).Value / (double)uint.MaxValue;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            (srcPtr + 3).Value = alpha;
                        }

                        srcPtr += 4;
                    }
                }
                return;
            case BufferFormat.R32G32B32A32_SInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)ptr;
                    uint alpha = (uint)(int)(alphaValue.Max(-1.0f).Min(1.0f) * (double)int.MaxValue).Round();

                    for (int i = 0; i < pitch; i += 16)
                    {
                        double srcAlpha = ((int)(srcPtr + 3).Value / (double)int.MaxValue).Max(-1.0);

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            (srcPtr + 3).Value = alpha;
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
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;
                    ushort alpha = (ushort)(alphaValue.Max(0.0f).Min(1.0f) * ushort.MaxValue).Round();

                    for (int i = 0; i < pitch; i += 8)
                    {
                        float srcAlpha = (srcPtr + 3).Value / (float)ushort.MaxValue;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            (srcPtr + 3).Value = alpha;
                        }

                        srcPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_SNorm:
            case BufferFormat.R16G16B16A16_SInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;
                    ushort alpha = (ushort)(short)(alphaValue.Max(-1.0f).Min(1.0f) * short.MaxValue).Round();

                    for (int i = 0; i < pitch; i += 8)
                    {
                        float srcAlpha = ((short)(srcPtr + 3).Value / (float)short.MaxValue).Max(-1.0f);

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            (srcPtr + 3).Value = alpha;
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
                    uint alpha = (uint)(alphaValue.Max(0.0f).Min(1.0f) * 3.0f).Round();

                    for (int i = 0; i < pitch; i += 4)
                    {
                        float srcAlpha = (srcPtr.Value >> 30) / 3.0f;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            srcPtr.Value = (srcPtr.Value & 0x3FFFFFFF) | (alpha << 30);
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
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)ptr;
                    uint alpha = (uint)(alphaValue.Max(0.0f).Min(1.0f) * byte.MaxValue).Round();

                    for (int i = 0; i < pitch; i += 4)
                    {
                        float srcAlpha = (srcPtr.Value >> 24) / (float)byte.MaxValue;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            srcPtr.Value = (srcPtr.Value & 0xFFFFFF) | (alpha << 24);
                        }

                        ++srcPtr;
                    }
                }
                return;
            case BufferFormat.R8G8B8A8_SNorm:
            case BufferFormat.R8G8B8A8_SInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)ptr;
                    uint alpha = (byte)(sbyte)(alphaValue.Max(-1.0f).Min(1.0f) * sbyte.MaxValue).Round();

                    for (int i = 0; i < pitch; i += 4)
                    {
                        float srcAlpha = ((sbyte)(srcPtr.Value >> 24) / (float)sbyte.MaxValue).Max(-1.0f);

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            srcPtr.Value = (srcPtr.Value & 0xFFFFFF) | (alpha << 24);
                        }

                        ++srcPtr;
                    }
                }
                return;
            case BufferFormat.B5G5R5A1_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;
                    ushort alpha = (ushort)alphaValue.Max(0.0f).Min(1.0f).Round();

                    for (int i = 0; i < pitch; i += 2)
                    {
                        float srcAlpha = srcPtr.Value >> 15;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            srcPtr.Value = (ushort)((srcPtr.Value & 0x7FFF) | (alpha << 15));
                        }

                        ++srcPtr;
                    }
                }
                return;
            case BufferFormat.A4B4G4R4_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;
                    ushort alpha = (ushort)(alphaValue.Max(0.0f).Min(1.0f) * 15.0f).Round();

                    for (int i = 0; i < pitch; i += 2)
                    {
                        float srcAlpha = (srcPtr.Value & 0xF) / 15.0f;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            srcPtr.Value = (ushort)((srcPtr.Value & 0xFFF0) | alpha);
                        }

                        ++srcPtr;
                    }
                }
                return;
            case BufferFormat.B4G4R4A4_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)ptr;
                    ushort alpha = (ushort)(alphaValue.Max(0.0f).Min(1.0f) * 15.0f).Round();

                    for (int i = 0; i < pitch; i += 2)
                    {
                        float srcAlpha = (srcPtr.Value >> 12) / 15.0f;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            srcPtr.Value = (ushort)((srcPtr.Value & 0x0FFF) | (alpha << 12));
                        }

                        ++srcPtr;
                    }
                }
                return;
            case BufferFormat.A8_UNorm:
                {
                    byte alpha = (byte)(alphaValue.Max(0.0f).Min(1.0f) * byte.MaxValue).Round();

                    for (int i = 0; i < pitch; ++i)
                    {
                        float srcAlpha = ptr[i] / (float)byte.MaxValue;

                        if ((srcAlpha >= minAlpha) && (srcAlpha <= maxAlpha))
                        {
                            ptr[i] = alpha;
                        }
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
            case BufferFormat.R32G32B32A32_Typeless:
            case BufferFormat.R32G32B32A32_UInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        double srcAlpha = (srcPtr + 3).Value / (double)uint.MaxValue;

                        destPtr.Value = (uint)((srcPtr).Value * srcAlpha).Round();
                        (destPtr + 1).Value = (uint)((srcPtr + 1).Value * srcAlpha).Round();
                        (destPtr + 2).Value = (uint)((srcPtr + 2).Value * srcAlpha).Round();

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R32G32B32A32_SInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        double srcAlpha = ((int)(srcPtr + 3).Value / (double)int.MaxValue).Max(0.0);

                        destPtr.Value = (uint)(int)((int)(srcPtr).Value * srcAlpha).Round();
                        (destPtr + 1).Value = (uint)(int)((int)(srcPtr + 1).Value * srcAlpha).Round();
                        (destPtr + 2).Value = (uint)(int)((int)(srcPtr + 2).Value * srcAlpha).Round();

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
            case BufferFormat.R16G16B16A16_Typeless:
            case BufferFormat.R16G16B16A16_UNorm:
            case BufferFormat.R16G16B16A16_UInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        float srcAlpha = (srcPtr + 3).Value / (float)ushort.MaxValue;

                        destPtr.Value = (ushort)((srcPtr).Value * srcAlpha).Round();
                        (destPtr + 1).Value = (ushort)((srcPtr + 1).Value * srcAlpha).Round();
                        (destPtr + 2).Value = (ushort)((srcPtr + 2).Value * srcAlpha).Round();

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_SNorm:
            case BufferFormat.R16G16B16A16_SInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        float srcAlpha = ((short)(srcPtr + 3).Value / (float)short.MaxValue).Max(0.0f);

                        destPtr.Value = (ushort)(short)((short)(srcPtr).Value * srcAlpha).Round();
                        (destPtr + 1).Value = (ushort)(short)((short)(srcPtr + 1).Value * srcAlpha).Round();
                        (destPtr + 2).Value = (ushort)(short)((short)(srcPtr + 2).Value * srcAlpha).Round();

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
                        float srcAlpha = (((pixel & 0xC0000000) >> 30) & 3) / 3.0f;

                        uint c1 = (uint)(((color >> 20) & 0x3FF) * srcAlpha).Round();
                        uint c2 = (uint)(((color >> 10) & 0x3FF) * srcAlpha).Round();
                        uint c3 = (uint)((color & 0x3ff) * srcAlpha).Round();

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

                        uint c1 = (uint)(((color >> 16) & 0xFF) * srcAlpha).Round();
                        uint c2 = (uint)(((color >> 8) & 0xFF) * srcAlpha).Round();
                        uint c3 = (uint)((color & 0xFF) * srcAlpha).Round();

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

                    for (int i = 0; i < size; i += sizeof(ushort))
                    {
                        ushort pixel = srcPtr.Value;
                        ushort color = (ushort)(pixel & 0xfff);
                        float srcAlpha = ((pixel & 0xF000) >> 12) / 15.0f;

                        ushort c1 = (ushort)(((color >> 8) & 0xF) * srcAlpha).Round();
                        ushort c2 = (ushort)(((color >> 4) & 0xF) * srcAlpha).Round();
                        ushort c3 = (ushort)((color & 0xF) * srcAlpha).Round();

                        color = (ushort)((c1 << 8) | (c2 << 4) | c3);

                        destPtr.Value = (ushort)((pixel & 0xF000) | color);

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
            case BufferFormat.A4B4G4R4_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += sizeof(ushort))
                    {
                        ushort pixel = srcPtr.Value;
                        ushort color = (ushort)(pixel & 0xfff0);
                        float srcAlpha = (pixel & 0xF) / 15.0f;

                        ushort c1 = (ushort)(((color >> 12) & 0xF) * srcAlpha).Round();
                        ushort c2 = (ushort)(((color >> 8) & 0xF) * srcAlpha).Round();
                        ushort c3 = (ushort)(((color >> 4) & 0xF) * srcAlpha).Round();

                        color = (ushort)((c1 << 12) | (c2 << 8) | (c3 << 4));

                        destPtr.Value = (ushort)((pixel & 0xF) | color);

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
            case BufferFormat.R32G32B32A32_Typeless:
            case BufferFormat.R32G32B32A32_UInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        double srcAlpha = (srcPtr + 3).Value / (double)uint.MaxValue;

                        if (srcAlpha <= 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        destPtr.Value = (uint)((srcPtr).Value / srcAlpha).Round().Min((double)uint.MaxValue);
                        (destPtr + 1).Value = (uint)((srcPtr + 1).Value / srcAlpha).Round().Min((double)uint.MaxValue);
                        (destPtr + 2).Value = (uint)((srcPtr + 2).Value / srcAlpha).Round().Min((double)uint.MaxValue);

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R32G32B32A32_SInt:
                {
                    GorgonPtr<uint> srcPtr = (GorgonPtr<uint>)src;
                    GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

                    for (int i = 0; i < size; i += 16)
                    {
                        double srcAlpha = ((int)(srcPtr + 3).Value / (double)int.MaxValue).Max(0.0);

                        if (srcAlpha <= 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        destPtr.Value = (uint)(int)((int)(srcPtr).Value / srcAlpha).Round().Max((double)int.MinValue).Min((double)int.MaxValue);
                        (destPtr + 1).Value = (uint)(int)((int)(srcPtr + 1).Value / srcAlpha).Round().Max((double)int.MinValue).Min((double)int.MaxValue);
                        (destPtr + 2).Value = (uint)(int)((int)(srcPtr + 2).Value / srcAlpha).Round().Max((double)int.MinValue).Min((double)int.MaxValue);

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
            case BufferFormat.R16G16B16A16_Typeless:
            case BufferFormat.R16G16B16A16_UNorm:
            case BufferFormat.R16G16B16A16_UInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        float srcAlpha = (srcPtr + 3).Value / (float)ushort.MaxValue;

                        if (srcAlpha <= 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        destPtr.Value = (ushort)((srcPtr).Value / srcAlpha).Round().Min((float)ushort.MaxValue);
                        (destPtr + 1).Value = (ushort)((srcPtr + 1).Value / srcAlpha).Round().Min((float)ushort.MaxValue);
                        (destPtr + 2).Value = (ushort)((srcPtr + 2).Value / srcAlpha).Round().Min((float)ushort.MaxValue);

                        srcPtr += 4;
                        destPtr += 4;
                    }
                }
                return;
            case BufferFormat.R16G16B16A16_SNorm:
            case BufferFormat.R16G16B16A16_SInt:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += 8)
                    {
                        float srcAlpha = ((short)(srcPtr + 3).Value / (float)short.MaxValue).Max(0.0f);

                        if (srcAlpha <= 0)
                        {
                            srcPtr += 4;
                            destPtr += 4;
                            continue;
                        }

                        destPtr.Value = (ushort)(short)((short)(srcPtr).Value / srcAlpha).Round().Max((float)short.MinValue).Min((float)short.MaxValue);
                        (destPtr + 1).Value = (ushort)(short)((short)(srcPtr + 1).Value / srcAlpha).Round().Max((float)short.MinValue).Min((float)short.MaxValue);
                        (destPtr + 2).Value = (ushort)(short)((short)(srcPtr + 2).Value / srcAlpha).Round().Max((float)short.MinValue).Min((float)short.MaxValue);

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
                        float srcAlpha = (((pixel & 0xC0000000) >> 30) & 3) / 3.0f;
                        bool zeroAlpha = srcAlpha.EqualsEpsilon(0);

                        uint c1 = zeroAlpha ? 0 : (uint)(((color >> 20) & 0x3FF) / srcAlpha).Round().Min(1023.0f);
                        uint c2 = zeroAlpha ? 0 : (uint)(((color >> 10) & 0x3FF) / srcAlpha).Round().Min(1023.0f);
                        uint c3 = zeroAlpha ? 0 : (uint)((color & 0x3ff) / srcAlpha).Round().Min(1023.0f);

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

                        uint c1 = zeroAlpha ? 0 : (uint)(((color >> 16) & 0xFF) / srcAlpha).Round().Min(255.0f);
                        uint c2 = zeroAlpha ? 0 : (uint)(((color >> 8) & 0xFF) / srcAlpha).Round().Min(255.0f);
                        uint c3 = zeroAlpha ? 0 : (uint)((color & 0xFF) / srcAlpha).Round().Min(255.0f);

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

                    for (int i = 0; i < size; i += sizeof(ushort))
                    {
                        ushort pixel = srcPtr.Value;
                        ushort color = (ushort)(pixel & 0xfff);
                        float srcAlpha = ((pixel & 0xF000) >> 12) / 15.0f;
                        bool zeroAlpha = srcAlpha.EqualsEpsilon(0);

                        ushort c1 = zeroAlpha ? (ushort)0 : (ushort)(((color >> 8) & 0xF) / srcAlpha).Round().Min(15.0f);
                        ushort c2 = zeroAlpha ? (ushort)0 : (ushort)(((color >> 4) & 0xF) / srcAlpha).Round().Min(15.0f);
                        ushort c3 = zeroAlpha ? (ushort)0 : (ushort)((color & 0xF) / srcAlpha).Round().Min(15.0f);

                        color = (ushort)((c1 << 8) | (c2 << 4) | c3);

                        destPtr.Value = (ushort)((pixel & 0xF000) | color);

                        ++srcPtr;
                        ++destPtr;
                    }
                }
                return;
            case BufferFormat.A4B4G4R4_UNorm:
                {
                    GorgonPtr<ushort> srcPtr = (GorgonPtr<ushort>)src;
                    GorgonPtr<ushort> destPtr = (GorgonPtr<ushort>)dest;

                    for (int i = 0; i < size; i += sizeof(ushort))
                    {
                        ushort pixel = srcPtr.Value;
                        ushort color = (ushort)(pixel & 0xfff0);
                        float srcAlpha = (pixel & 0xF) / 15.0f;
                        bool zeroAlpha = srcAlpha.EqualsEpsilon(0);

                        ushort c1 = zeroAlpha ? (ushort)0 : (ushort)(((color >> 12) & 0xF) / srcAlpha).Round().Min(15.0f);
                        ushort c2 = zeroAlpha ? (ushort)0 : (ushort)(((color >> 8) & 0xF) / srcAlpha).Round().Min(15.0f);
                        ushort c3 = zeroAlpha ? (ushort)0 : (ushort)(((color >> 4) & 0xF) / srcAlpha).Round().Min(15.0f);

                        color = (ushort)((c1 << 12) | (c2 << 8) | (c3 << 4));

                        destPtr.Value = (ushort)((pixel & 0xF) | color);

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
    public static void Expand24BPPScanLine(GorgonPtr<byte> src, int srcPitch, GorgonPtr<byte> dest)
    {
        GorgonPtr<byte> srcPtr = src;
        GorgonPtr<uint> destPtr = (GorgonPtr<uint>)dest;

        for (int x = 0; x < srcPitch; x += 3)
        {
            uint pixel = (uint)(((srcPtr++).Value) | ((srcPtr++).Value << 8) | ((srcPtr++).Value << 16) | 0xFF000000);

            (destPtr++).Value = pixel;
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
