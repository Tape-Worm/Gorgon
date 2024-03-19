// 
// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: Thursday, December 10, 2015 1:10:04 AM
// 

// Portions of this code is adapted from the DirectXTex library by Chuck Walburn:
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
//
// DirectXTex: https://github.com/Microsoft/DirectXTex

using System.Diagnostics;
using Gorgon.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics;

/// <summary>
/// Flags to handle legacy format types.
/// </summary>
[Flags]
public enum PitchFlags
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,
    /// <summary>
    /// Data is aligned to a DWORD boundary instead of a byte boundary.
    /// </summary>
    LegacyDWORD = 1,
    /// <summary>
    /// Pitch is 16-byte aligned instead of byte aligned.
    /// </summary>
    Align16Byte = 0x2,
    /// <summary>
    /// Pitch is 32-byte aligned instead of byte aligned.
    /// </summary>
    Align32Byte = 0x4,
    /// <summary>
    /// Pitch is 64-byte aligned instead of byte aligned.
    /// </summary>
    Align64Byte = 0x8,
    /// <summary>
    /// Pitch is 4096-byte aligned instead of byte aligned.
    /// </summary>
    Align4K = 0x200,
    /// <summary>
    /// Format uses 24 bits per pixel.
    /// </summary>
    BPP24 = 0x10000,
    /// <summary>
    /// Format uses 16 bits per pixel.
    /// </summary>
    BPP16 = 0x20000,
    /// <summary>
    /// Format uses 8 bits per pixel.
    /// </summary>
    BPP8 = 0x40000
}

/// <summary>
/// Provides information for a specific <see cref="BufferFormat"/>.
/// </summary>
/// <remarks>
/// <para>
/// This object will return the specifics for a <see cref="BufferFormat"/>, such as its bit depth, format grouping, and other information about the format. This is useful for determining how to handle a formatted 
/// element in a buffer at the byte level.
/// </para>
/// </remarks>
public class GorgonFormatInfo
{
    /// <summary>
    /// Property to return the group for the format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The group for a format is typically its typeless counterpart. To be grouped with another format, the format must have the same byte ordering, and the format size must be the same as the other 
    /// formats in the group.
    /// </para>
    /// <para>
    /// For example, <see cref="BufferFormat.R8G8B8A8_UNorm"/> would be a member of the group <see cref="BufferFormat.R8G8B8A8_Typeless"/>, as would <see cref="BufferFormat.R8G8B8A8_SInt"/>.
    /// </para>
    /// </remarks>
    public BufferFormat Group
    {
        get;
    } = BufferFormat.Unknown;

    /// <summary>
    /// Property to return the format that the information in this object is based on.
    /// </summary>
    public BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return the number of components for the format.
    /// </summary>
    public int ComponentCount
    {
        get;
    }

    /// <summary>
    /// Property to return whether the format is typeless or not.
    /// </summary>
    /// <remarks>
    /// When this value returns <b>true</b>, then the components of the format may be interpreted in any way. If not, then the components of the format are expected to be interpreted as a known type.
    /// </remarks>
    public bool IsTypeless
    {
        get;
    } = true;

    /// <summary>
    /// Property to return the bit depth for the format.
    /// </summary>
    /// <remarks>
    /// This is the number of bits in the format, not per component. For example, <see cref="BufferFormat.R8G8B8A8_UNorm"/> would be 32 bits, and <see cref="BufferFormat.B5G6R5_UNorm"/> would be 16 
    /// bits.
    /// </remarks>
    public int BitDepth
    {
        get;
    }

    /// <summary>
    /// Property to return the size of the format, in bytes.
    /// </summary>
    public int SizeInBytes
    {
        get;
    }

    /// <summary>
    /// Property to return whether the format has a depth component.
    /// </summary>
    /// <remarks>
    /// When this value returns <b>true</b>, then this format can be considered valid for use as a depth buffer format (this depends on available hardware support).
    /// </remarks>
    public bool HasDepth
    {
        get;
    }

    /// <summary>
    /// Property to return whether the format has a stencil component.
    /// </summary>
    /// <remarks>
    /// When this value returns <b>true</b>, then this format can be considered valid for use as a stencil buffer format (this depends on available hardware support).
    /// </remarks>
    public bool HasStencil
    {
        get;
    }

    /// <summary>
    /// Property to return whether this format is an SRgb format or not.
    /// </summary>
    public bool IsSRgb
    {
        get;
    }

    /// <summary>
    /// Property to return whether the format has an alpha component.
    /// </summary>
    public bool HasAlpha
    {
        get;
    }

    /// <summary>
    /// Property to return whether the format is a planar format.
    /// </summary>
    public bool IsPlanar
    {
        get;
    }

    /// <summary>
    /// Property to return whether the pixel format is packed or not.
    /// </summary>
    public bool IsPacked
    {
        get;
    }

    /// <summary>
    /// Property to return whether the pixel format is compressed or not.
    /// </summary>
    /// <remarks>
    /// If this value returns <b>true</b>, then the format is meant for use with images that contain block compressed data.
    /// </remarks>
    public bool IsCompressed
    {
        get;
    }

    /// <summary>
    /// Property to return whether this format uses an indexed palette or not.
    /// </summary>
    /// <remarks>
    /// For some 8 bit formats, the pixel value is an index into a larger palette of color values. For example, if the index 10 is mapped to a color value of R:64, G:32, B:128, then any pixels with the value 
    /// of 10 will use that color from the palette.
    /// </remarks>
    public bool IsPalettized => Format is BufferFormat.A8P8 or BufferFormat.P8;

    /// <summary>
    /// Function to retrieve the number of bytes a format occupies.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    /// <param name="bitDepth">The bit depth for the format.</param>
    /// <returns>The number of bytes for the format.</returns>
    private static int GetByteCount(BufferFormat format, int bitDepth)
    {
        if (format == BufferFormat.Unknown)
        {
            return 0;
        }

        int sizeInBytes;

        // Can't have a data type smaller than 1 byte.
        if (bitDepth >= 8)
        {
            sizeInBytes = bitDepth >> 3;
        }
        else
        {
            sizeInBytes = 1;
        }

        return sizeInBytes;
    }

    /// <summary>
    /// Function to retrieve whether this format has a depth component or not.
    /// </summary>
    /// <param name="format">Format to look up.</param>
    /// <returns>A tuple containing whether the format contains depth and stencil information.</returns>
    private static (bool hasDepth, bool hasStencil) GetDepthState(BufferFormat format) => format switch
    {
        BufferFormat.D24_UNorm_S8_UInt or BufferFormat.D32_Float_S8X24_UInt => (true, true),
        BufferFormat.D32_Float or BufferFormat.D16_UNorm => (true, false),
        _ => (false, false)
    };

    /// <summary>
    /// Function to determine if a format is a planar pixel format.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    /// <returns><b>true</b> if the format is a planar pixel format, <b>false</b> if not.</returns>
    private static bool GetIsPlanar(BufferFormat format) => format is BufferFormat.NV12 or BufferFormat.P010 or BufferFormat.P016 or BufferFormat.Opaque420 or BufferFormat.NV11
                                                            or BufferFormat.P208 or BufferFormat.V208 or BufferFormat.V408;

    /// <summary>
    /// Function to determine if a format is a packed pixel format.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    /// <returns><b>true</b> if the format is a packed pixel format, <b>false</b> if not.</returns>
    private static bool GetIsPacked(BufferFormat format) => format is BufferFormat.R8G8_B8G8_UNorm or BufferFormat.G8R8_G8B8_UNorm or BufferFormat.YUY2 or BufferFormat.Y210 or BufferFormat.Y216 or BufferFormat.Y410 or BufferFormat.Y416;

    /// <summary>
    /// Function to retrieve the number of bits required for the format.
    /// </summary>
    /// <param name="format">Format to evaluate.</param>
    /// <returns>The number of bits for the format.</returns>
    private static int GetBitDepth(BufferFormat format) => format switch
    {
        BufferFormat.Unknown => 0,
        BufferFormat.R32G32B32A32_Float => 128,
        BufferFormat.R32G32B32A32_Typeless => 128,
        BufferFormat.R32G32B32A32_UInt => 128,
        BufferFormat.R32G32B32A32_SInt => 128,
        BufferFormat.BC2_Typeless => 128,
        BufferFormat.BC2_UNorm => 128,
        BufferFormat.BC2_UNorm_SRgb => 128,
        BufferFormat.BC3_Typeless => 128,
        BufferFormat.BC3_UNorm => 128,
        BufferFormat.BC3_UNorm_SRgb => 128,
        BufferFormat.BC5_Typeless => 128,
        BufferFormat.BC5_UNorm => 128,
        BufferFormat.BC5_SNorm => 128,
        BufferFormat.BC6H_Typeless => 128,
        BufferFormat.BC6H_Uf16 => 128,
        BufferFormat.BC6H_Sf16 => 128,
        BufferFormat.BC7_Typeless => 128,
        BufferFormat.BC7_UNorm => 128,
        BufferFormat.BC7_UNorm_SRgb => 128,
        BufferFormat.R32G32B32_Typeless => 96,
        BufferFormat.R32G32B32_Float => 96,
        BufferFormat.R32G32B32_UInt => 96,
        BufferFormat.R32G32B32_SInt => 96,
        BufferFormat.BC1_Typeless => 64,
        BufferFormat.BC1_UNorm => 64,
        BufferFormat.BC1_UNorm_SRgb => 64,
        BufferFormat.BC4_Typeless => 64,
        BufferFormat.BC4_UNorm => 64,
        BufferFormat.BC4_SNorm => 64,
        BufferFormat.R32G32_Typeless => 64,
        BufferFormat.R32G32_Float => 64,
        BufferFormat.R32G32_UInt => 64,
        BufferFormat.R32G32_SInt => 64,
        BufferFormat.R32G8X24_Typeless => 64,
        BufferFormat.D32_Float_S8X24_UInt => 64,
        BufferFormat.R32_Float_X8X24_Typeless => 64,
        BufferFormat.X32_Typeless_G8X24_UInt => 64,
        BufferFormat.R16G16B16A16_Typeless => 64,
        BufferFormat.R16G16B16A16_Float => 64,
        BufferFormat.R16G16B16A16_UNorm => 64,
        BufferFormat.R16G16B16A16_UInt => 64,
        BufferFormat.R16G16B16A16_SNorm => 64,
        BufferFormat.R16G16B16A16_SInt => 64,
        BufferFormat.R32_Typeless => 32,
        BufferFormat.R32_Float => 32,
        BufferFormat.R32_UInt => 32,
        BufferFormat.R32_SInt => 32,
        BufferFormat.D32_Float => 32,
        BufferFormat.R24G8_Typeless => 32,
        BufferFormat.D24_UNorm_S8_UInt => 32,
        BufferFormat.R24_UNorm_X8_Typeless => 32,
        BufferFormat.X24_Typeless_G8_UInt => 32,
        BufferFormat.R16G16_Typeless => 32,
        BufferFormat.R16G16_Float => 32,
        BufferFormat.R16G16_UNorm => 32,
        BufferFormat.R16G16_UInt => 32,
        BufferFormat.R16G16_SNorm => 32,
        BufferFormat.R16G16_SInt => 32,
        BufferFormat.R10G10B10A2_Typeless => 32,
        BufferFormat.R10G10B10A2_UNorm => 32,
        BufferFormat.R10G10B10A2_UInt => 32,
        BufferFormat.R10G10B10_Xr_Bias_A2_UNorm => 32,
        BufferFormat.R11G11B10_Float => 32,
        BufferFormat.R8G8B8A8_Typeless => 32,
        BufferFormat.R8G8B8A8_UNorm => 32,
        BufferFormat.R8G8B8A8_UNorm_SRgb => 32,
        BufferFormat.R8G8B8A8_UInt => 32,
        BufferFormat.R8G8B8A8_SNorm => 32,
        BufferFormat.R8G8B8A8_SInt => 32,
        BufferFormat.B8G8R8A8_UNorm => 32,
        BufferFormat.B8G8R8X8_UNorm => 32,
        BufferFormat.B8G8R8A8_Typeless => 32,
        BufferFormat.B8G8R8A8_UNorm_SRgb => 32,
        BufferFormat.B8G8R8X8_Typeless => 32,
        BufferFormat.B8G8R8X8_UNorm_SRgb => 32,
        BufferFormat.R8G8_B8G8_UNorm => 32,
        BufferFormat.G8R8_G8B8_UNorm => 32,
        BufferFormat.Y410 => 32,
        BufferFormat.Y416 => 32,
        BufferFormat.AYUV => 32,
        BufferFormat.R9G9B9E5_SharedExp => 32,
        BufferFormat.R16_Typeless => 16,
        BufferFormat.R16_Float => 16,
        BufferFormat.D16_UNorm => 16,
        BufferFormat.R16_UNorm => 16,
        BufferFormat.R16_UInt => 16,
        BufferFormat.R16_SNorm => 16,
        BufferFormat.R16_SInt => 16,
        BufferFormat.R8G8_Typeless => 16,
        BufferFormat.R8G8_UNorm => 16,
        BufferFormat.R8G8_UInt => 16,
        BufferFormat.R8G8_SNorm => 16,
        BufferFormat.R8G8_SInt => 16,
        BufferFormat.B5G5R5A1_UNorm => 16,
        BufferFormat.B5G6R5_UNorm => 16,
        BufferFormat.B4G4R4A4_UNorm => 16,
        BufferFormat.A8P8 => 16,
        BufferFormat.P010 => 16,
        BufferFormat.P016 => 16,
        BufferFormat.Y210 => 16,
        BufferFormat.Y216 => 16,
        BufferFormat.YUY2 => 16,
        BufferFormat.NV11 => 16,
        BufferFormat.NV12 => 16,
        BufferFormat.Opaque420 => 16,
        BufferFormat.P208 => 16,
        BufferFormat.V208 => 16,
        BufferFormat.V408 => 16,
        BufferFormat.R8_Typeless => 8,
        BufferFormat.R8_UNorm => 8,
        BufferFormat.R8_UInt => 8,
        BufferFormat.R8_SNorm => 8,
        BufferFormat.R8_SInt => 8,
        BufferFormat.A8_UNorm => 8,
        BufferFormat.AI44 => 8,
        BufferFormat.IA44 => 8,
        BufferFormat.P8 => 8,
        BufferFormat.R1_UNorm => 8,
        _ => 0,
    };

    /// <summary>
    /// Function to retrieve whether this format is an SRgb format or not.
    /// </summary>
    /// <param name="format">Format to look up.</param>
    /// <returns><b>true</b> if the format is an sRGB format, or <b>false</b> if not.</returns>
    private static bool GetSRgbState(BufferFormat format) => format switch
    {
        BufferFormat.R8G8B8A8_UNorm_SRgb or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.B8G8R8X8_UNorm_SRgb or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_UNorm_SRgb or BufferFormat.BC7_UNorm_SRgb => true,
        _ => false
    };

    /// <summary>
    /// Function to retrieve the number of components that make up a format.
    /// </summary>
    /// <param name="format">The format to evaulate.</param>
    /// <returns>The number of components.</returns>
    private static int GetComponentCount(BufferFormat format) => format switch
    {
        BufferFormat.R32G32B32A32_Float => 4,
        BufferFormat.R32G32B32A32_Typeless => 4,
        BufferFormat.R32G32B32A32_UInt => 4,
        BufferFormat.R32G32B32A32_SInt => 4,
        BufferFormat.R16G16B16A16_Typeless => 4,
        BufferFormat.R16G16B16A16_Float => 4,
        BufferFormat.R16G16B16A16_UNorm => 4,
        BufferFormat.R16G16B16A16_UInt => 4,
        BufferFormat.R16G16B16A16_SNorm => 4,
        BufferFormat.R16G16B16A16_SInt => 4,
        BufferFormat.R10G10B10A2_Typeless => 4,
        BufferFormat.R10G10B10A2_UNorm => 4,
        BufferFormat.R10G10B10A2_UInt => 4,
        BufferFormat.R10G10B10_Xr_Bias_A2_UNorm => 4,
        BufferFormat.R8G8_B8G8_UNorm => 4,
        BufferFormat.G8R8_G8B8_UNorm => 4,
        BufferFormat.R8G8B8A8_Typeless => 4,
        BufferFormat.R8G8B8A8_UNorm => 4,
        BufferFormat.R8G8B8A8_UNorm_SRgb => 4,
        BufferFormat.R8G8B8A8_UInt => 4,
        BufferFormat.R8G8B8A8_SNorm => 4,
        BufferFormat.R8G8B8A8_SInt => 4,
        BufferFormat.B8G8R8A8_UNorm => 4,
        BufferFormat.B8G8R8A8_Typeless => 4,
        BufferFormat.B8G8R8A8_UNorm_SRgb => 4,
        BufferFormat.B5G5R5A1_UNorm => 4,
        BufferFormat.B4G4R4A4_UNorm => 4,
        BufferFormat.B8G8R8X8_UNorm => 3,
        BufferFormat.B5G6R5_UNorm => 3,
        BufferFormat.R32G32B32_Typeless => 3,
        BufferFormat.R32G32B32_Float => 3,
        BufferFormat.R32G32B32_UInt => 3,
        BufferFormat.R32G32B32_SInt => 3,
        BufferFormat.B8G8R8X8_Typeless => 3,
        BufferFormat.B8G8R8X8_UNorm_SRgb => 3,
        BufferFormat.R11G11B10_Float => 3,
        BufferFormat.R9G9B9E5_SharedExp => 3,
        BufferFormat.R32G32_Typeless => 2,
        BufferFormat.R32G32_Float => 2,
        BufferFormat.R32G32_UInt => 2,
        BufferFormat.R32G32_SInt => 2,
        BufferFormat.R32G8X24_Typeless => 2,
        BufferFormat.R16G16_Typeless => 2,
        BufferFormat.R16G16_Float => 2,
        BufferFormat.R16G16_UNorm => 2,
        BufferFormat.R16G16_UInt => 2,
        BufferFormat.R16G16_SNorm => 2,
        BufferFormat.R16G16_SInt => 2,
        BufferFormat.R24G8_Typeless => 2,
        BufferFormat.R8G8_Typeless => 2,
        BufferFormat.R8G8_UNorm => 2,
        BufferFormat.R8G8_UInt => 2,
        BufferFormat.R8G8_SNorm => 2,
        BufferFormat.R8G8_SInt => 2,
        BufferFormat.A8P8 => 2,
        BufferFormat.AI44 => 2,
        BufferFormat.IA44 => 2,
        BufferFormat.D24_UNorm_S8_UInt => 2,
        BufferFormat.D32_Float_S8X24_UInt => 2,
        BufferFormat.R32_Float_X8X24_Typeless => 1,
        BufferFormat.X32_Typeless_G8X24_UInt => 1,
        BufferFormat.R32_Typeless => 1,
        BufferFormat.R32_Float => 1,
        BufferFormat.R32_UInt => 1,
        BufferFormat.R32_SInt => 1,
        BufferFormat.D32_Float => 1,
        BufferFormat.R24_UNorm_X8_Typeless => 1,
        BufferFormat.X24_Typeless_G8_UInt => 1,
        BufferFormat.R16_Typeless => 1,
        BufferFormat.R16_Float => 1,
        BufferFormat.D16_UNorm => 1,
        BufferFormat.R16_UNorm => 1,
        BufferFormat.R16_UInt => 1,
        BufferFormat.R16_SNorm => 1,
        BufferFormat.R16_SInt => 1,
        BufferFormat.R8_Typeless => 1,
        BufferFormat.R8_UNorm => 1,
        BufferFormat.R8_UInt => 1,
        BufferFormat.R8_SNorm => 1,
        BufferFormat.R8_SInt => 1,
        BufferFormat.A8_UNorm => 1,
        BufferFormat.P8 => 1,
        BufferFormat.R1_UNorm => 1,
        _ => 0,
    };

    /// <summary>
    /// Function to retrieve whether a buffer is compressed or not.
    /// </summary>
    /// <param name="format">Format of the buffer.</param>
    /// <returns><b>true</b> if the format represents a block compressed format, <b>false</b> if not.</returns>
    private static bool GetCompressedState(BufferFormat format) => format switch
    {
        BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC2_Typeless or BufferFormat.BC2_UNorm or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_Typeless or BufferFormat.BC3_UNorm or BufferFormat.BC3_UNorm_SRgb or BufferFormat.BC4_Typeless or BufferFormat.BC4_SNorm or BufferFormat.BC4_UNorm or BufferFormat.BC5_Typeless or BufferFormat.BC5_SNorm or BufferFormat.BC5_UNorm or BufferFormat.BC6H_Typeless or BufferFormat.BC6H_Sf16 or BufferFormat.BC6H_Uf16 or BufferFormat.BC7_Typeless or BufferFormat.BC7_UNorm or BufferFormat.BC7_UNorm_SRgb => true,
        _ => false
    };

    /// <summary>
    /// Function to determine which typeless group the format belongs to.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    /// <returns>The group format.</returns>
    private static BufferFormat GetGroup(BufferFormat format) => format switch
    {
        BufferFormat.B8G8R8A8_Typeless or BufferFormat.B8G8R8A8_UNorm or BufferFormat.B8G8R8A8_UNorm_SRgb => BufferFormat.B8G8R8A8_Typeless,
        BufferFormat.B8G8R8X8_Typeless or BufferFormat.B8G8R8X8_UNorm or BufferFormat.B8G8R8X8_UNorm_SRgb => BufferFormat.B8G8R8X8_Typeless,
        BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb => BufferFormat.BC1_Typeless,
        BufferFormat.BC2_Typeless or BufferFormat.BC2_UNorm or BufferFormat.BC2_UNorm_SRgb => BufferFormat.BC2_Typeless,
        BufferFormat.BC3_Typeless or BufferFormat.BC3_UNorm or BufferFormat.BC3_UNorm_SRgb => BufferFormat.BC3_Typeless,
        BufferFormat.BC4_Typeless or BufferFormat.BC4_UNorm or BufferFormat.BC4_SNorm => BufferFormat.BC4_Typeless,
        BufferFormat.BC5_Typeless or BufferFormat.BC5_UNorm or BufferFormat.BC5_SNorm => BufferFormat.BC5_Typeless,
        BufferFormat.BC6H_Typeless or BufferFormat.BC6H_Uf16 or BufferFormat.BC6H_Sf16 => BufferFormat.BC6H_Typeless,
        BufferFormat.BC7_Typeless or BufferFormat.BC7_UNorm or BufferFormat.BC7_UNorm_SRgb => BufferFormat.BC7_Typeless,
        BufferFormat.R10G10B10A2_Typeless or BufferFormat.R10G10B10A2_UNorm or BufferFormat.R10G10B10A2_UInt => BufferFormat.R10G10B10A2_Typeless,
        BufferFormat.R16_Typeless or BufferFormat.R16_Float or BufferFormat.R16_UNorm or BufferFormat.R16_UInt or BufferFormat.R16_SNorm or BufferFormat.R16_SInt => BufferFormat.R16_Typeless,
        BufferFormat.R16G16_Typeless or BufferFormat.R16G16_Float or BufferFormat.R16G16_UNorm or BufferFormat.R16G16_UInt or BufferFormat.R16G16_SNorm or BufferFormat.R16G16_SInt => BufferFormat.R16G16_Typeless,
        BufferFormat.R16G16B16A16_Typeless or BufferFormat.R16G16B16A16_Float or BufferFormat.R16G16B16A16_UNorm or BufferFormat.R16G16B16A16_UInt or BufferFormat.R16G16B16A16_SNorm or BufferFormat.R16G16B16A16_SInt => BufferFormat.R16G16B16A16_Typeless,
        BufferFormat.R32_Typeless or BufferFormat.R32_Float or BufferFormat.R32_UInt or BufferFormat.R32_SInt => BufferFormat.R32_Typeless,
        BufferFormat.R32G32_Typeless or BufferFormat.R32G32_Float or BufferFormat.R32G32_UInt or BufferFormat.R32G32_SInt => BufferFormat.R32G32_Typeless,
        BufferFormat.R32G32B32_Typeless or BufferFormat.R32G32B32_Float or BufferFormat.R32G32B32_UInt or BufferFormat.R32G32B32_SInt => BufferFormat.R32G32B32_Typeless,
        BufferFormat.R32G32B32A32_Typeless or BufferFormat.R32G32B32A32_Float or BufferFormat.R32G32B32A32_UInt or BufferFormat.R32G32B32A32_SInt => BufferFormat.R32G32B32A32_Typeless,
        BufferFormat.R8_Typeless or BufferFormat.R8_UNorm or BufferFormat.R8_UInt or BufferFormat.R8_SNorm or BufferFormat.R8_SInt => BufferFormat.R8_Typeless,
        BufferFormat.R8G8_Typeless or BufferFormat.R8G8_UNorm or BufferFormat.R8G8_UInt or BufferFormat.R8G8_SNorm or BufferFormat.R8G8_SInt => BufferFormat.R8G8_Typeless,
        BufferFormat.R8G8B8A8_Typeless or BufferFormat.R8G8B8A8_UNorm or BufferFormat.R8G8B8A8_UNorm_SRgb or BufferFormat.R8G8B8A8_UInt or BufferFormat.R8G8B8A8_SNorm or BufferFormat.R8G8B8A8_SInt => BufferFormat.R8G8B8A8_Typeless,
        BufferFormat.B4G4R4A4_UNorm => BufferFormat.B4G4R4A4_UNorm,
        _ => BufferFormat.Unknown,
    };

    /// <summary>
    /// Function to determine if the specified format is typeless.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <returns><b>true</b> if the format is typeless, or <b>false</b> if not.</returns>
    private static bool GetTypelessState(BufferFormat format) => format switch
    {
        BufferFormat.R32G32B32A32_Typeless or BufferFormat.BC1_Typeless or BufferFormat.BC2_Typeless or BufferFormat.BC3_Typeless or BufferFormat.BC4_Typeless or BufferFormat.BC5_Typeless or BufferFormat.BC6H_Typeless or BufferFormat.BC7_Typeless or BufferFormat.R32G32B32_Typeless or BufferFormat.R32G32_Typeless or BufferFormat.R32G8X24_Typeless or BufferFormat.R32_Float_X8X24_Typeless or BufferFormat.X32_Typeless_G8X24_UInt or BufferFormat.R16G16B16A16_Typeless or BufferFormat.R32_Typeless or BufferFormat.R24G8_Typeless or BufferFormat.R24_UNorm_X8_Typeless or BufferFormat.R16G16_Typeless or BufferFormat.R10G10B10A2_Typeless or BufferFormat.R8G8B8A8_Typeless or BufferFormat.B8G8R8A8_Typeless or BufferFormat.B8G8R8X8_Typeless or BufferFormat.R16_Typeless or BufferFormat.R8G8_Typeless or BufferFormat.R8_Typeless => true,
        _ => false,
    };

    /// <summary>
    /// Function to determine if the format has an alpha channel.
    /// </summary>
    /// <param name="format">Format to check.</param>
    /// <returns><b>true</b> if the format contains an alpha channel, <b>false</b> if not.</returns>
    private static bool GetAlphaChannel(BufferFormat format) => format switch
    {
        BufferFormat.R32G32B32A32_Float or BufferFormat.R32G32B32A32_Typeless or BufferFormat.R32G32B32A32_UInt or BufferFormat.R32G32B32A32_SInt or BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC2_Typeless or BufferFormat.BC2_UNorm or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_Typeless or BufferFormat.BC3_UNorm or BufferFormat.BC3_UNorm_SRgb or BufferFormat.BC7_Typeless or BufferFormat.BC7_UNorm or BufferFormat.BC7_UNorm_SRgb or BufferFormat.R16G16B16A16_Typeless or BufferFormat.R16G16B16A16_Float or BufferFormat.R16G16B16A16_UNorm or BufferFormat.R16G16B16A16_UInt or BufferFormat.R16G16B16A16_SNorm or BufferFormat.R16G16B16A16_SInt or BufferFormat.R10G10B10A2_Typeless or BufferFormat.R10G10B10A2_UNorm or BufferFormat.R10G10B10A2_UInt or BufferFormat.R10G10B10_Xr_Bias_A2_UNorm or BufferFormat.R8G8B8A8_Typeless or BufferFormat.R8G8B8A8_UNorm or BufferFormat.R8G8B8A8_UNorm_SRgb or BufferFormat.R8G8B8A8_UInt or BufferFormat.R8G8B8A8_SNorm or BufferFormat.R8G8B8A8_SInt or BufferFormat.B8G8R8A8_UNorm or BufferFormat.B8G8R8A8_Typeless or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.B5G5R5A1_UNorm or BufferFormat.B4G4R4A4_UNorm or BufferFormat.A8_UNorm => true,
        _ => false,
    };

    /// <summary>
    /// Function to return pitch information for this format.
    /// </summary>
    /// <param name="width">The width of the data.</param>
    /// <param name="height">The height of the image pixel data.</param>
    /// <param name="flags">[Optional] Flags used to influence the row pitch.</param>
    /// <returns>The pitch information for the format.</returns>
    /// <exception cref="ArgumentException">Thrown if the format requires a height that is a multiple of 2, and the height is not a multiple of 2.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="flags"/> parameter is used to compensate in cases where the original image data is not laid out correctly (such as with older DirectDraw DDS images).
    /// </para>
    /// </remarks>
    public GorgonPitchLayout GetPitchForFormat(int width, int height, PitchFlags flags = PitchFlags.None)
    {
        int rowPitch;

        // Do calculations for compressed formats.
        if (IsCompressed)
        {
            int bpb = Format switch
            {
                BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC4_Typeless or BufferFormat.BC4_SNorm or BufferFormat.BC4_UNorm => 8,
                _ => 16,
            };

            long numBlocksWide = 0;
            if (width > 0)
            {
                numBlocksWide = (width + 3) / 4;
            }
            long numBlocksHigh = 0;
            if (height > 0)
            {
                numBlocksHigh = (height + 3) / 4;
            }
            long rowBytes = numBlocksWide * bpb;
            long numBytes = rowBytes * numBlocksHigh;

            return new GorgonPitchLayout((int)rowBytes, (int)numBytes, (int)numBlocksWide, (int)numBlocksHigh);
        }

        int slicePitch = 0;

        if (IsPacked)
        {
            switch (Format)
            {
                case BufferFormat.R8G8_B8G8_UNorm:
                case BufferFormat.G8R8_G8B8_UNorm:
                case BufferFormat.YUY2:
                    rowPitch = ((width + 1) >> 1) << 2;
                    slicePitch = rowPitch * height;
                    break;
                case BufferFormat.Y210:
                case BufferFormat.Y216:
                    rowPitch = ((width + 1) >> 1) << 4;
                    slicePitch = rowPitch * height;
                    break;
                default:
                    rowPitch = 0;
                    break;
            }

            // Cannot find anything about row/slice pitch for these formats.
            if (Format is not (BufferFormat.Y410 or BufferFormat.Y416))
            {
                Debug.Assert(rowPitch != 0, "Format [" + Format + "] is a packed format. But the pitch/slice info cannot be extracted - Confirm this is correct with DXGI_FORMAT documentation.");
            }

            return new GorgonPitchLayout(rowPitch, slicePitch);
        }

        if (IsPlanar)
        {
            switch (Format)
            {            
                case BufferFormat.NV12:
                case BufferFormat.Opaque420:
                    if ((height % 2) != 0)
                    {
                        throw new ArgumentException(string.Format(Resources.GOR_ERR_HEIGHT_MUST_BE_MULTIPLE_OF_2, height, Format), nameof(height));
                    }

                    rowPitch = ((width + 1) >> 1) << 1;
                    slicePitch = rowPitch * (height + ((height + 1) >> 1));
                    break;
                case BufferFormat.P010:
                case BufferFormat.P016:
                    if ((height % 2) != 0)
                    {
                        throw new ArgumentException(string.Format(Resources.GOR_ERR_HEIGHT_MUST_BE_MULTIPLE_OF_2, height, Format), nameof(height));
                    }

                    rowPitch = ((width + 1) >> 1) << 2;
                    slicePitch = rowPitch * (height + ((height + 1) >> 1));
                    break;
                case BufferFormat.NV11:
                    rowPitch = ((width + 3) >> 2) << 2;
                    slicePitch = (rowPitch * height) << 1;
                    break;
                case BufferFormat.P208:
                    rowPitch = ((width + 1) >> 1) << 1;
                    slicePitch = rowPitch * (height << 1);
                    break;
                case BufferFormat.V208:
                    if ((height % 2) != 0)
                    {
                        throw new ArgumentException(string.Format(Resources.GOR_ERR_HEIGHT_MUST_BE_MULTIPLE_OF_2, height, Format), nameof(height));
                    }

                    rowPitch = width;
                    slicePitch = rowPitch * (height + ((height + 1) >> 1) << 1);
                    break;
                case BufferFormat.V408:
                    rowPitch = width;
                    slicePitch = rowPitch * (height + ((height >> 1) << 2));
                    break;
                default:
                    rowPitch = 0;
                    break;
            }

            Debug.Assert(rowPitch != 0, "Format [" + Format + "] is a planar format. Cannot to extract pitch/slice info.");

            return new GorgonPitchLayout(rowPitch, slicePitch);
        }

        int bitsPerPixel = BitDepth;

        if ((flags & PitchFlags.BPP24) == PitchFlags.BPP24)
        {
            bitsPerPixel = 24;
        }
        else if ((flags & PitchFlags.BPP16) == PitchFlags.BPP16)
        {
            bitsPerPixel = 16;
        }
        else if ((flags & PitchFlags.BPP8) == PitchFlags.BPP8)
        {
            bitsPerPixel = 8;
        }

        // This is for handling old DirectDraw DDS files that didn't output
        // properly because of assumptions about pitch alignment.
        if ((flags & PitchFlags.LegacyDWORD) == PitchFlags.LegacyDWORD)
        {
            rowPitch = (((width * bitsPerPixel) + 31) / 32) * sizeof(int);
        }
        else if ((flags & PitchFlags.Align4K) == PitchFlags.Align4K)
        {
            rowPitch = (((width * bitsPerPixel) + 32767) / 32768) * 4096;
        }
        else if ((flags & PitchFlags.Align64Byte) == PitchFlags.Align64Byte)
        {
            rowPitch = (((width * bitsPerPixel) + 511) / 512) * 64;
        }
        else if ((flags & PitchFlags.Align32Byte) == PitchFlags.Align32Byte)
        {
            rowPitch = (((width * bitsPerPixel) + 255) / 256) * 32;
        }
        else if ((flags & PitchFlags.Align16Byte) == PitchFlags.Align16Byte)
        {
            rowPitch = (((width * bitsPerPixel) + 127) / 128) * 16;
        }
        else
        {
            rowPitch = (((width * bitsPerPixel) + 7) / 8);
        }

        return new GorgonPitchLayout(rowPitch, rowPitch * height);
    }

    /// <summary>
    /// Function to compute the scan lines for a format, given a specific height.
    /// </summary>
    /// <param name="height">The height, in pixels.</param>
    /// <returns>The number of scan lines.</returns>
    /// <remarks>
    /// <para>
    /// This will compute the number of scan lines for an image that uses the format that this information describes. If the format is <see cref="IsCompressed"/>, then this method will compute the 
    /// scanline count based on the maximum block size multiple of 4. If the format is not compressed, then it will just return the height value passed in.
    /// </para>
    /// </remarks>
    public int CalculateScanlines(int height) => IsCompressed
            ? 4.Max((height + 3) >> 2)
            : Format switch
            {
                // These are planar formats.
                BufferFormat.NV11 => height << 1,
                BufferFormat.NV12 or BufferFormat.P010 or BufferFormat.P016 or BufferFormat.Opaque420 => height + ((height + 1) >> 1),                
                // All other formats report height as-is.
                _ => height
            };

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFormatInfo"/> class.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    /// <remarks>
    /// <para>
    /// If the <paramref name="format"/> parameter is set to <see cref="BufferFormat.Unknown"/>, then the members of this object, except for <see cref="Group"/>, will be set to default values and will not be accurate. 
    /// </para>
    /// </remarks>
    public GorgonFormatInfo(BufferFormat format)
    {
        Format = format;

        if (format == BufferFormat.Unknown)
        {
            return;
        }

        Group = GetGroup(format);
        IsPacked = GetIsPacked(format);
        IsPlanar = GetIsPlanar(format);
        IsCompressed = GetCompressedState(format);
        IsTypeless = GetTypelessState(format);
        HasAlpha = GetAlphaChannel(format);
        IsSRgb = GetSRgbState(format);
        (HasDepth, HasStencil) = GetDepthState(format);
        BitDepth = GetBitDepth(format);
        ComponentCount = GetComponentCount(format);
        SizeInBytes = GetByteCount(format, BitDepth);
    }
}