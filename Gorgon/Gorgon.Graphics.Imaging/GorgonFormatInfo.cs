#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
#endregion

#region DirectXTex 
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
#endregion

using System.Diagnostics;
using Gorgon.Graphics.Imaging;
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
/// Provides information for a specific DXGI Format.
/// </summary>
/// <remarks>
/// <para>
/// This object will return the specifics for a DXGI Format, such as its bit depth, format grouping, and other information about the format. This is useful for determining how to handle a formatted element in a buffer at the byte level.
/// </para>
/// </remarks>
public class GorgonFormatInfo
{
    #region Variables.
    // The number of bytes used by a single element of this format type.
    private int _sizeInBytes;

    /// <summary>
    /// A default information type for the <see cref="BufferFormat.Unknown"/> format.
    /// </summary>
    public static readonly GorgonFormatInfo UnknownFormatInfo = new(BufferFormat.Unknown);
    #endregion

    #region Properties.
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
        private set;
    }

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
        private set;
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
        private set;
    }

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
        private set;
    }

    /// <summary>
    /// Property to return the size of the format, in bytes.
    /// </summary>
    public int SizeInBytes
    {
        get
        {
            if (_sizeInBytes != 0)
            {
                return _sizeInBytes;
            }

            // Can't have a data type smaller than 1 byte.
            if (BitDepth >= 8)
            {
                _sizeInBytes = BitDepth / 8;
            }
            else
            {
                _sizeInBytes = 1;
            }

            return _sizeInBytes;
        }
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
        private set;
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
        private set;
    }

    /// <summary>
    /// Property to return whether this format is an SRgb format or not.
    /// </summary>
    public bool IsSRgb
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return whether the format has an alpha component.
    /// </summary>
    public bool HasAlpha
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return whether the pixel format is packed or not.
    /// </summary>
    public bool IsPacked => Format is BufferFormat.R8G8_B8G8_UNorm or BufferFormat.G8R8_G8B8_UNorm or BufferFormat.Y410 or BufferFormat.Y416 or BufferFormat.Y210 or BufferFormat.Y216;

    /// <summary>
    /// Property to return whether the pixel format is compressed or not.
    /// </summary>
    /// <remarks>
    /// If this value returns <b>true</b>, then the format is meant for use with images that contain block compressed data.
    /// </remarks>
    public bool IsCompressed
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return whether this format uses an indexed palette or not.
    /// </summary>
    /// <remarks>
    /// For some 8 bit formats, the pixel value is an index into a larger palette of color values. For example, if the index 10 is mapped to a color value of R:64, G:32, B:128, then any pixels with the value 
    /// of 10 will use that color from the palette.
    /// </remarks>
    public bool IsPalettized => Format is BufferFormat.A8P8 or BufferFormat.P8;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve whether this format has a depth component or not.
    /// </summary>
    /// <param name="format">Format to look up.</param>
    private void GetDepthState(BufferFormat format)
    {
        switch (format)
        {
            case BufferFormat.D24_UNorm_S8_UInt:
            case BufferFormat.D32_Float_S8X24_UInt:
                HasDepth = true;
                HasStencil = true;
                break;
            case BufferFormat.D32_Float:
            case BufferFormat.D16_UNorm:
                HasStencil = false;
                HasDepth = true;
                break;
            default:
                HasDepth = false;
                HasStencil = false;
                break;
        }
    }

    /// <summary>
    /// Function to retrieve the number of bits required for the format.
    /// </summary>
    /// <param name="format">Format to evaluate.</param>
    private void GetBitDepth(BufferFormat format) => BitDepth = format switch
    {
        BufferFormat.R1_UNorm => 1,
        BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC4_Typeless or BufferFormat.BC4_UNorm or BufferFormat.BC4_SNorm => 4,
        BufferFormat.R8_Typeless or BufferFormat.R8_UNorm or BufferFormat.R8_UInt or BufferFormat.R8_SNorm or BufferFormat.R8_SInt or BufferFormat.A8_UNorm or BufferFormat.BC2_Typeless or BufferFormat.BC2_UNorm or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_Typeless or BufferFormat.BC3_UNorm or BufferFormat.BC3_UNorm_SRgb or BufferFormat.BC5_Typeless or BufferFormat.BC5_UNorm or BufferFormat.BC5_SNorm or BufferFormat.BC6H_Typeless or BufferFormat.BC6H_Uf16 or BufferFormat.BC6H_Sf16 or BufferFormat.BC7_Typeless or BufferFormat.BC7_UNorm or BufferFormat.BC7_UNorm_SRgb or BufferFormat.P8 or BufferFormat.AI44 or BufferFormat.IA44 => 8,
        BufferFormat.NV11 or BufferFormat.NV12 or BufferFormat.Opaque420 => 12,
        BufferFormat.R8G8_Typeless or BufferFormat.R8G8_UNorm or BufferFormat.R8G8_UInt or BufferFormat.R8G8_SNorm or BufferFormat.R8G8_SInt or BufferFormat.R16_Typeless or BufferFormat.R16_Float or BufferFormat.D16_UNorm or BufferFormat.R16_UNorm or BufferFormat.R16_UInt or BufferFormat.R16_SNorm or BufferFormat.R16_SInt or BufferFormat.B5G6R5_UNorm or BufferFormat.B5G5R5A1_UNorm or BufferFormat.B4G4R4A4_UNorm or BufferFormat.A8P8 => 16,
        BufferFormat.P010 or BufferFormat.P016 => 24,
        BufferFormat.R10G10B10A2_Typeless or BufferFormat.R10G10B10A2_UNorm or BufferFormat.R10G10B10A2_UInt or BufferFormat.R11G11B10_Float or BufferFormat.R8G8B8A8_Typeless or BufferFormat.R8G8B8A8_UNorm or BufferFormat.R8G8B8A8_UNorm_SRgb or BufferFormat.R8G8B8A8_UInt or BufferFormat.R8G8B8A8_SNorm or BufferFormat.R8G8B8A8_SInt or BufferFormat.R16G16_Typeless or BufferFormat.R16G16_Float or BufferFormat.R16G16_UNorm or BufferFormat.R16G16_UInt or BufferFormat.R16G16_SNorm or BufferFormat.R16G16_SInt or BufferFormat.R32_Typeless or BufferFormat.D32_Float or BufferFormat.R32_Float or BufferFormat.R32_UInt or BufferFormat.R32_SInt or BufferFormat.R24G8_Typeless or BufferFormat.D24_UNorm_S8_UInt or BufferFormat.R24_UNorm_X8_Typeless or BufferFormat.X24_Typeless_G8_UInt or BufferFormat.R9G9B9E5_Sharedexp or BufferFormat.R8G8_B8G8_UNorm or BufferFormat.G8R8_G8B8_UNorm or BufferFormat.B8G8R8A8_UNorm or BufferFormat.B8G8R8X8_UNorm or BufferFormat.R10G10B10_Xr_Bias_A2_UNorm or BufferFormat.B8G8R8A8_Typeless or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.B8G8R8X8_Typeless or BufferFormat.B8G8R8X8_UNorm_SRgb or BufferFormat.AYUV or BufferFormat.Y410 or BufferFormat.YUY2 => 32,
        BufferFormat.R16G16B16A16_Typeless or BufferFormat.R16G16B16A16_Float or BufferFormat.R16G16B16A16_UNorm or BufferFormat.R16G16B16A16_UInt or BufferFormat.R16G16B16A16_SNorm or BufferFormat.R16G16B16A16_SInt or BufferFormat.R32G32_Typeless or BufferFormat.R32G32_Float or BufferFormat.R32G32_UInt or BufferFormat.R32G32_SInt or BufferFormat.R32G8X24_Typeless or BufferFormat.D32_Float_S8X24_UInt or BufferFormat.R32_Float_X8X24_Typeless or BufferFormat.X32_Typeless_G8X24_UInt or BufferFormat.Y416 or BufferFormat.Y210 or BufferFormat.Y216 => 64,
        BufferFormat.R32G32B32_Typeless or BufferFormat.R32G32B32_Float or BufferFormat.R32G32B32_UInt or BufferFormat.R32G32B32_SInt => 96,
        BufferFormat.R32G32B32A32_Typeless or BufferFormat.R32G32B32A32_Float or BufferFormat.R32G32B32A32_UInt or BufferFormat.R32G32B32A32_SInt => 128,
        _ => 0,
    };

    /// <summary>
    /// Function to retrieve whether this format is an SRgb format or not.
    /// </summary>
    /// <param name="format">Format to look up.</param>
    private void GetSRgbState(BufferFormat format) => IsSRgb = format switch
    {
        BufferFormat.R8G8B8A8_UNorm_SRgb or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.B8G8R8X8_UNorm_SRgb or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_UNorm_SRgb or BufferFormat.BC7_UNorm_SRgb => true,
        _ => false,
    };

    /// <summary>
    /// Function to retrieve the number of components that make up a format.
    /// </summary>
    /// <param name="format">The format to evaulate.</param>
    private void GetComponentCount(BufferFormat format) => ComponentCount = format switch
    {
        BufferFormat.R32G32B32A32_Typeless or BufferFormat.R32G32B32A32_Float or BufferFormat.R32G32B32A32_UInt or BufferFormat.R32G32B32A32_SInt or BufferFormat.R16G16B16A16_Typeless or BufferFormat.R16G16B16A16_Float or BufferFormat.R16G16B16A16_UNorm or BufferFormat.R16G16B16A16_UInt or BufferFormat.R16G16B16A16_SNorm or BufferFormat.R16G16B16A16_SInt or BufferFormat.R10G10B10A2_Typeless or BufferFormat.R10G10B10A2_UNorm or BufferFormat.R10G10B10A2_UInt or BufferFormat.R8G8B8A8_Typeless or BufferFormat.R8G8B8A8_UNorm or BufferFormat.R8G8B8A8_UNorm_SRgb or BufferFormat.R8G8B8A8_UInt or BufferFormat.R8G8B8A8_SNorm or BufferFormat.R8G8B8A8_SInt or BufferFormat.R8G8_B8G8_UNorm or BufferFormat.G8R8_G8B8_UNorm or BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC2_Typeless or BufferFormat.BC2_UNorm or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_Typeless or BufferFormat.BC3_UNorm or BufferFormat.BC3_UNorm_SRgb or BufferFormat.BC4_Typeless or BufferFormat.BC4_UNorm or BufferFormat.BC4_SNorm or BufferFormat.BC5_Typeless or BufferFormat.BC5_UNorm or BufferFormat.BC5_SNorm or BufferFormat.B5G5R5A1_UNorm or BufferFormat.B8G8R8A8_UNorm or BufferFormat.B8G8R8X8_UNorm or BufferFormat.R10G10B10_Xr_Bias_A2_UNorm or BufferFormat.B8G8R8A8_Typeless or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.B8G8R8X8_Typeless or BufferFormat.B8G8R8X8_UNorm_SRgb or BufferFormat.BC6H_Typeless or BufferFormat.BC6H_Uf16 or BufferFormat.BC6H_Sf16 or BufferFormat.BC7_Typeless or BufferFormat.BC7_UNorm or BufferFormat.BC7_UNorm_SRgb or BufferFormat.B4G4R4A4_UNorm => 4,
        BufferFormat.R11G11B10_Float or BufferFormat.R32G32B32_Typeless or BufferFormat.R32G32B32_Float or BufferFormat.R32G32B32_UInt or BufferFormat.R32G32B32_SInt or BufferFormat.B5G6R5_UNorm or BufferFormat.R9G9B9E5_Sharedexp => 3,
        BufferFormat.R32G32_Typeless or BufferFormat.R32G32_Float or BufferFormat.R32G32_UInt or BufferFormat.R32G32_SInt or BufferFormat.R32G8X24_Typeless or BufferFormat.R16G16_Typeless or BufferFormat.R16G16_Float or BufferFormat.R16G16_UNorm or BufferFormat.R16G16_UInt or BufferFormat.R16G16_SNorm or BufferFormat.R16G16_SInt or BufferFormat.R24G8_Typeless or BufferFormat.R8G8_Typeless or BufferFormat.R8G8_UNorm or BufferFormat.R8G8_UInt or BufferFormat.R8G8_SNorm or BufferFormat.R8G8_SInt or BufferFormat.AYUV or BufferFormat.YUY2 or BufferFormat.NV11 or BufferFormat.AI44 or BufferFormat.IA44 or BufferFormat.A8P8 => 2,
        BufferFormat.R8_Typeless or BufferFormat.R8_UNorm or BufferFormat.R8_UInt or BufferFormat.R8_SNorm or BufferFormat.R8_SInt or BufferFormat.A8_UNorm or BufferFormat.R1_UNorm or BufferFormat.Y410 or BufferFormat.Y416 or BufferFormat.NV12 or BufferFormat.P010 or BufferFormat.P016 or BufferFormat.Opaque420 or BufferFormat.Y210 or BufferFormat.Y216 or BufferFormat.P8 or BufferFormat.P208 or BufferFormat.V208 or BufferFormat.V408 or BufferFormat.X32_Typeless_G8X24_UInt or BufferFormat.R32_Float_X8X24_Typeless or BufferFormat.R32_Typeless or BufferFormat.D32_Float or BufferFormat.R32_Float or BufferFormat.R32_UInt or BufferFormat.R32_SInt or BufferFormat.R16_Typeless or BufferFormat.R16_Float or BufferFormat.D16_UNorm or BufferFormat.R16_UNorm or BufferFormat.R16_UInt or BufferFormat.R16_SNorm or BufferFormat.R16_SInt or BufferFormat.D24_UNorm_S8_UInt or BufferFormat.R24_UNorm_X8_Typeless or BufferFormat.X24_Typeless_G8_UInt or BufferFormat.D32_Float_S8X24_UInt => 1,
        _ => 0,
    };

    /// <summary>
    /// Function to retrieve whether a buffer is compressed or not.
    /// </summary>
    /// <param name="format">Format of the buffer.</param>
    private void GetCompressedState(BufferFormat format) => IsCompressed = format switch
    {
        BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC2_Typeless or BufferFormat.BC2_UNorm or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_Typeless or BufferFormat.BC3_UNorm or BufferFormat.BC3_UNorm_SRgb or BufferFormat.BC4_Typeless or BufferFormat.BC4_SNorm or BufferFormat.BC4_UNorm or BufferFormat.BC5_Typeless or BufferFormat.BC5_SNorm or BufferFormat.BC5_UNorm or BufferFormat.BC6H_Typeless or BufferFormat.BC6H_Sf16 or BufferFormat.BC6H_Uf16 or BufferFormat.BC7_Typeless or BufferFormat.BC7_UNorm or BufferFormat.BC7_UNorm_SRgb => true,
        _ => false,
    };

    /// <summary>
    /// Function to determine which typeless group the format belongs to.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    private void GetGroup(BufferFormat format) => Group = format switch
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
    private void GetTypelessState(BufferFormat format) => IsTypeless = format switch
    {
        BufferFormat.R32G32B32A32_Typeless or BufferFormat.R32G32B32_Typeless or BufferFormat.R16G16B16A16_Typeless or BufferFormat.R32G32_Typeless or BufferFormat.R32G8X24_Typeless or BufferFormat.R10G10B10A2_Typeless or BufferFormat.R8G8B8A8_Typeless or BufferFormat.R16G16_Typeless or BufferFormat.R32_Typeless or BufferFormat.R24G8_Typeless or BufferFormat.R8G8_Typeless or BufferFormat.R16_Typeless or BufferFormat.R8_Typeless or BufferFormat.BC1_Typeless or BufferFormat.BC2_Typeless or BufferFormat.BC3_Typeless or BufferFormat.BC4_Typeless or BufferFormat.BC5_Typeless or BufferFormat.B8G8R8A8_Typeless or BufferFormat.B8G8R8X8_Typeless or BufferFormat.BC6H_Typeless or BufferFormat.BC7_Typeless or BufferFormat.Unknown => true,
        _ => false,
    };

    /// <summary>
    /// Function to determine if the format has an alpha channel.
    /// </summary>
    /// <param name="format">Format to check.</param>
    private void GetAlphaChannel(BufferFormat format) => HasAlpha = format switch
    {
        BufferFormat.R32G32B32A32_Typeless or BufferFormat.R32G32B32A32_Float or BufferFormat.R32G32B32A32_UInt or BufferFormat.R32G32B32A32_SInt or BufferFormat.R16G16B16A16_Typeless or BufferFormat.R16G16B16A16_Float or BufferFormat.R16G16B16A16_UNorm or BufferFormat.R16G16B16A16_UInt or BufferFormat.R16G16B16A16_SNorm or BufferFormat.R16G16B16A16_SInt or BufferFormat.R10G10B10A2_Typeless or BufferFormat.R10G10B10A2_UNorm or BufferFormat.R10G10B10A2_UInt or BufferFormat.R8G8B8A8_Typeless or BufferFormat.R8G8B8A8_UNorm or BufferFormat.R8G8B8A8_UNorm_SRgb or BufferFormat.R8G8B8A8_UInt or BufferFormat.R8G8B8A8_SNorm or BufferFormat.R8G8B8A8_SInt or BufferFormat.A8_UNorm or BufferFormat.BC1_Typeless or BufferFormat.BC1_UNorm or BufferFormat.BC1_UNorm_SRgb or BufferFormat.BC2_Typeless or BufferFormat.BC2_UNorm or BufferFormat.BC2_UNorm_SRgb or BufferFormat.BC3_Typeless or BufferFormat.BC3_UNorm or BufferFormat.BC3_UNorm_SRgb or BufferFormat.B5G5R5A1_UNorm or BufferFormat.B8G8R8A8_UNorm or BufferFormat.R10G10B10_Xr_Bias_A2_UNorm or BufferFormat.B8G8R8A8_Typeless or BufferFormat.B8G8R8A8_UNorm_SRgb or BufferFormat.BC7_Typeless or BufferFormat.BC7_UNorm or BufferFormat.BC7_UNorm_SRgb or BufferFormat.B4G4R4A4_UNorm or BufferFormat.A8P8 => true,
        _ => false,
    };

    /// <summary>
    /// Function to return pitch information for this format.
    /// </summary>
    /// <param name="width">The width of the data.</param>
    /// <param name="height">The height of the image pixel data.</param>
    /// <param name="flags">[Optional] Flags used to influence the row pitch.</param>
    /// <returns>The pitch information for the format.</returns>
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
            /*
int widthCounter = 1.Max((width + 3) / 4);
int heightCounter = 1.Max((height + 3) / 4);
rowPitch = widthCounter * bpb;*/

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

        if (IsPacked)
        {
            int slicePitch = 0;

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
                case BufferFormat.NV12:
                case BufferFormat.Opaque420:
                    rowPitch = ((width + 1) >> 1) << 1;
                    slicePitch = rowPitch * (height + ((height + 1) >> 1));
                    break;
                case BufferFormat.P010:
                case BufferFormat.P016:
                    rowPitch = ((width + 1) >> 1) << 2;
                    slicePitch = rowPitch * (height + ((height + 1) >> 1));
                    break;
                case BufferFormat.NV11:
                    rowPitch = ((width + 3) >> 2) << 2;
                    slicePitch = (rowPitch * height) << 1;
                    break;
                default:
                    rowPitch = 0;
                    break;
            }

            Debug.Assert(rowPitch != 0, "Format [" + Format + "] is a packed format. Cannot to extract pitch/slice info.");

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
    /// This will compute the number of scan lines for an image that uses the format that this information describes.  If the format is <see cref="IsCompressed"/>, then this method will compute the 
    /// scanline count based on the maximum size between 1 and a block size multiple of 4.  If the format is not compressed, then it will just return the height value passed in.
    /// </para>
    /// </remarks>
    public int CalculateScanlines(int height) => IsCompressed
            ? 1.Max((height + 3) >> 2)
            : Format switch
            {
                // These are planar formats.
                BufferFormat.NV11 => height << 1,
                BufferFormat.NV12 or BufferFormat.P010 or BufferFormat.P016 or BufferFormat.Opaque420 => height + ((height + 1) >> 1),
                // All other formats report height as-is.
                _ => height,
            };
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFormatInfo"/> class.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    /// <remarks>
    /// If the <paramref name="format"/> parameter is set to <see cref="BufferFormat.Unknown"/>, then the members of this object, except for <see cref="Group"/>, will be set to default values and may not be accurate. 
    /// </remarks>
    public GorgonFormatInfo(BufferFormat format)
    {
        Format = format;
        Group = BufferFormat.Unknown;

        if (format == BufferFormat.Unknown)
        {
            return;
        }

        GetGroup(format);
        GetCompressedState(format);
        GetTypelessState(format);
        GetAlphaChannel(format);
        GetSRgbState(format);
        GetDepthState(format);
        GetBitDepth(format);
        GetComponentCount(format);
    }
    #endregion
}
