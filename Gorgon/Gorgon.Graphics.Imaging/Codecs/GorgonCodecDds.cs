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
// Created: August 16, 2016 12:57:49 PM
// 

// This code was adapted from:
// SharpDX by Alexandre Mutel (http://sharpdx.org)
// DirectXTex by Chuck Walburn (http://directxtex.codeplex.com)

#region SharpDX/DirectXTex licenses
// Copyright (c) 2010-2016 SharpDX - Alexandre Mutel
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTex http://directxtex.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the 
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and 
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to 
// the software.
// A "contributor" is any person that distributes its contribution under this 
// license.
// "Licensed patents" are a contributor's patent claims that read directly on 
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the 
// license conditions and limitations in section 3, each contributor grants 
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and 
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a 
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution 
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any 
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that 
// you claim are infringed by the software, your patent license from such 
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all 
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you 
// may do so only under this license by including a complete copy of this 
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that 
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license 
// cannot change. To the extent permitted under your local laws, the 
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.
#endregion
#endregion

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging.Codecs
{
    /// <summary>
    /// A codec to handle reading/writing DDDS files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This codec will read and write compressed or uncompressed (lossy, depending on pixel format) files using the Direct Draw Surface (DDS) format.
    /// </para>
    /// <para>
    /// This file format is the best suited for use with Gorgon as it supports a multitude of options and is far more flexible than other legacy formats such as TGA or PNG.
    /// </para>
    /// <para>
    /// While the DDS codec will support any format for Direct 3D 10 (except typeless formats) and above, it does not support the following legacy Direct3D 9 formats:
    /// <list type="bullet">
    ///     <item>
    ///         <description>BumpDuDv D3DFMT_V8U8</description>
    ///     </item>
    ///     <item>
    ///         <description>D3DFMT_Q8W8V8U8</description>
    ///     </item>
    ///     <item>
    ///         <description>D3DFMT_V16U16</description>
    ///     </item>
    ///     <item>
    ///         <description>D3DFMT_A2W10V10U10</description>
    ///     </item>
    ///     <item>
    ///         <description>BumpLuminance D3DFMT_L6V5U5</description>
    ///     </item>
    ///     <item>
    ///         <description>D3DFMT_X8L8V8U8</description>
    ///     </item>
    ///     <item>
    ///         <description>FourCC "UYVY" D3DFMT_UYVY</description>
    ///     </item>
    ///     <item>
    ///         <description>FourCC "YUY2" D3DFMT_YUY2</description>
    ///     </item>
    ///     <item>
    ///         <description>FourCC 117 D3DFMT_CxV8U8</description>
    ///     </item>
    ///     <item>
    ///         <description>ZBuffer D3DFMT_D16_LOCKABLE</description>
    ///     </item>
    ///     <item>
    ///         <description>FourCC 82 D3DFMT_D32F_LOCKABLE</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class GorgonCodecDds
        : GorgonImageCodec<IGorgonImageCodecEncodingOptions, GorgonDdsDecodingOptions>
    {
        #region Constants.
        // The DDS file magic number: "DDS "
        private const uint MagicNumber = 0x20534444;
        #endregion

        #region Variables.
        private static readonly DdsPixelFormat _pfDxt1 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '1'), 0, 0, 0, 0, 0);     // DXT1		
        private static readonly DdsPixelFormat _pfDxt2 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '2'), 0, 0, 0, 0, 0);     // DXT2
        private static readonly DdsPixelFormat _pfDxt3 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '3'), 0, 0, 0, 0, 0);     // DXT3
        private static readonly DdsPixelFormat _pfDxt4 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '4'), 0, 0, 0, 0, 0);     // DXT4
        private static readonly DdsPixelFormat _pfDxt5 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '5'), 0, 0, 0, 0, 0);     // DXT5
        private static readonly DdsPixelFormat _pfBC4U = new(DdsPixelFormatFlags.FourCC, MakeFourCC('B', 'C', '4', 'U'), 0, 0, 0, 0, 0);     // BC4 Unsigned
        private static readonly DdsPixelFormat _pfBC4S = new(DdsPixelFormatFlags.FourCC, MakeFourCC('B', 'C', '4', 'S'), 0, 0, 0, 0, 0);     // BC4 Signed
        private static readonly DdsPixelFormat _pfBC5U = new(DdsPixelFormatFlags.FourCC, MakeFourCC('B', 'C', '5', 'U'), 0, 0, 0, 0, 0);     // BC5 Unsigned
        private static readonly DdsPixelFormat _pfBC5S = new(DdsPixelFormatFlags.FourCC, MakeFourCC('B', 'C', '5', 'S'), 0, 0, 0, 0, 0);     // BC5 Signed
        private static readonly DdsPixelFormat _pfR8G8_B8G8 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('R', 'G', 'B', 'G'), 0, 0, 0, 0, 0); // R8G8_B8G8
        private static readonly DdsPixelFormat _pfG8R8_G8B8 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('G', 'R', 'G', 'B'), 0, 0, 0, 0, 0); // G8R8_G8B8
        private static readonly DdsPixelFormat _pfA8R8G8B8 = new(DdsPixelFormatFlags.RGBA, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000); // A8R8G8B8
        private static readonly DdsPixelFormat _pfX8R8G8B8 = new(DdsPixelFormatFlags.RGB, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000); // X8R8G8B8
        private static readonly DdsPixelFormat _pfA8B8G8R8 = new(DdsPixelFormatFlags.RGBA, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000); // A8B8G8R8
        private static readonly DdsPixelFormat _pfX8B8G8R8 = new(DdsPixelFormatFlags.RGB, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000); // X8B8G8R8
        private static readonly DdsPixelFormat _pfG16R16 = new(DdsPixelFormatFlags.RGB, 0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000); // G16R16
        private static readonly DdsPixelFormat _pfR5G6B5 = new(DdsPixelFormatFlags.RGB, 0, 16, 0x0000f800, 0x000007e0, 0x0000001f, 0x00000000); // R5G6B5
        private static readonly DdsPixelFormat _pfA1R5G5B5 = new(DdsPixelFormatFlags.RGBA, 0, 16, 0x00007c00, 0x000003e0, 0x0000001f, 0x00008000); // A1R5G5B5A1
        private static readonly DdsPixelFormat _pfA4R4G4B4 = new(DdsPixelFormatFlags.RGBA, 0, 16, 0x00000f00, 0x000000f0, 0x0000000f, 0x0000f000); // A4R4G4B4		
        private static readonly DdsPixelFormat _pfR8G8B8 = new(DdsPixelFormatFlags.RGB, 0, 24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000); // R8G8B8
        private static readonly DdsPixelFormat _pfL8 = new(DdsPixelFormatFlags.Luminance, 0, 8, 0xff, 0x00, 0x00, 0x00);                     // L8
        private static readonly DdsPixelFormat _pfL16 = new(DdsPixelFormatFlags.Luminance, 0, 16, 0xffff, 0x0000, 0x0000, 0x0000);           // L16
        private static readonly DdsPixelFormat _pfA8L8 = new(DdsPixelFormatFlags.LuminanceAlpha, 0, 16, 0x00ff, 0x0000, 0x0000, 0xff00);     // A8L8
        private static readonly DdsPixelFormat _pfA8 = new(DdsPixelFormatFlags.Alpha, 0, 8, 0x00, 0x00, 0x00, 0xff);                         // A8
        private static readonly DdsPixelFormat _pfDX10 = new(DdsPixelFormatFlags.FourCC, MakeFourCC('D', 'X', '1', '0'), 0, 0, 0, 0, 0);              // DX10 extension

        // Mappings for legacy formats.
        private readonly DdsLegacyConversion[] _legacyMapping =
            {
                new DdsLegacyConversion(BufferFormat.BC1_UNorm, DdsConversionFlags.None, _pfDxt1),
                new DdsLegacyConversion(BufferFormat.BC2_UNorm, DdsConversionFlags.None, _pfDxt3),
                new DdsLegacyConversion(BufferFormat.BC3_UNorm, DdsConversionFlags.None, _pfDxt5),
                new DdsLegacyConversion(BufferFormat.BC2_UNorm, DdsConversionFlags.None, _pfDxt2),
                new DdsLegacyConversion(BufferFormat.BC3_UNorm, DdsConversionFlags.None, _pfDxt4),
                new DdsLegacyConversion(BufferFormat.BC4_UNorm, DdsConversionFlags.None, _pfBC4U),
                new DdsLegacyConversion(BufferFormat.BC4_SNorm, DdsConversionFlags.None, _pfBC4S),
                new DdsLegacyConversion(BufferFormat.BC5_UNorm, DdsConversionFlags.None, _pfBC5U),
                new DdsLegacyConversion(BufferFormat.BC4_SNorm, DdsConversionFlags.None, _pfBC5S),
                new DdsLegacyConversion(BufferFormat.BC4_UNorm, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, MakeFourCC('A', 'T', 'I', '1'), 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.BC5_UNorm, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, MakeFourCC('A', 'T', 'I', '2'), 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R8G8_B8G8_UNorm, DdsConversionFlags.None, _pfR8G8_B8G8),
                new DdsLegacyConversion(BufferFormat.G8R8_G8B8_UNorm, DdsConversionFlags.None, _pfG8R8_G8B8),
                new DdsLegacyConversion(BufferFormat.B8G8R8A8_UNorm, DdsConversionFlags.None, _pfA8R8G8B8),
                new DdsLegacyConversion(BufferFormat.B8G8R8X8_UNorm, DdsConversionFlags.None, _pfX8R8G8B8),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.None, _pfA8B8G8R8),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.NoAlpha, _pfX8B8G8R8),
                new DdsLegacyConversion(BufferFormat.R16G16_UNorm, DdsConversionFlags.None, _pfG16R16),
                new DdsLegacyConversion(BufferFormat.R10G10B10A2_UNorm, DdsConversionFlags.Swizzle, new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 32, 0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000)),
                new DdsLegacyConversion(BufferFormat.R10G10B10A2_UNorm, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 32, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000)),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.NoAlpha | DdsConversionFlags.RGB888, _pfR8G8B8),
                new DdsLegacyConversion(BufferFormat.B5G6R5_UNorm, DdsConversionFlags.RGB565, _pfR5G6B5),
                new DdsLegacyConversion(BufferFormat.B5G5R5A1_UNorm, DdsConversionFlags.RGB5551, _pfA1R5G5B5),
                new DdsLegacyConversion(BufferFormat.B5G5R5A1_UNorm, DdsConversionFlags.RGB5551 | DdsConversionFlags.NoAlpha, new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x0000)),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.RGB8332, new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 16, 0x00e0, 0x001c, 0x0003, 0xff00)),
                new DdsLegacyConversion(BufferFormat.B5G6R5_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.RGB332, new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 8, 0xe0, 0x1c, 0x03, 0x00)),
                new DdsLegacyConversion(BufferFormat.R8_UNorm, DdsConversionFlags.None, _pfL8),
                new DdsLegacyConversion(BufferFormat.R16_UNorm, DdsConversionFlags.None, _pfL16),
                new DdsLegacyConversion(BufferFormat.R8G8_UNorm, DdsConversionFlags.None, _pfA8L8),
                new DdsLegacyConversion(BufferFormat.A8_UNorm, DdsConversionFlags.None, _pfA8),
                new DdsLegacyConversion(BufferFormat.R16G16B16A16_UNorm, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R16G16B16A16_SNorm, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R16_Float, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R16G16_Float, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R16G16B16A16_Float, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R32_Float, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R32G32_Float, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R32G32B32A32_Float, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R32_Float, DdsConversionFlags.None, new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.Palette | DdsConversionFlags.A8P8, new DdsPixelFormat(DdsPixelFormatFlags.PaletteIndexed, 0, 16, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.Palette, new DdsPixelFormat(DdsPixelFormatFlags.PaletteIndexed, 0, 8, 0, 0, 0, 0)),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.RGB4444,_pfA4R4G4B4),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.NoAlpha | DdsConversionFlags.RGB4444, new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)),
                new DdsLegacyConversion(BufferFormat.R8G8B8A8_UNorm, DdsConversionFlags.Expand | DdsConversionFlags.A4L4, new DdsPixelFormat(DdsPixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0))
        };

        // Supported buffer formats.
        private readonly BufferFormat[] _formats;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the pixel formats supported by the codec.
        /// </summary>
        public override IReadOnlyList<BufferFormat> SupportedPixelFormats => _formats;

        /// <summary>
        /// Property to return whether the codec supports decoding/encoding multiple frames or not.
        /// </summary>
        /// <remarks>
        /// For this codec, this means that the codec supports image/texture arrays natively.
        /// </remarks>
        public override bool SupportsMultipleFrames => true;

        /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
        public override bool SupportsMipMaps => true;

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
        public override bool SupportsDepth => true;

        /// <summary>
        /// Property to return whether the image codec supports block compression.
        /// </summary>
        public override bool SupportsBlockCompression => true;

        /// <summary>
        /// Property to return the friendly description of the format.
        /// </summary>
        public override string CodecDescription => Resources.GORIMG_DESC_DDS_CODEC;

        /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. PNG).
        /// </summary>
        public override string Codec => "DDS";
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a FOURCC value.
        /// </summary>
        /// <param name="c1">1st character.</param>
        /// <param name="c2">2nd character.</param>
        /// <param name="c3">3rd character.</param>
        /// <param name="c4">4th character.</param>
        /// <returns>The FOURCC value.</returns>
        private static uint MakeFourCC(char c1, char c2, char c3, char c4)
        {
            unchecked
            {
                return (((byte)c1)) | (((uint)((byte)c2)) << 8) | (((uint)((byte)c3)) << 16) | (((uint)((byte)c4)) << 24);
            }
        }

        /// <summary>
        /// Function to retrieve the correct format for pixel expansion.
        /// </summary>
        /// <param name="flags">Current conversion flags.</param>
        /// <returns>The correct format for pixel expansion, or None if no applicable format was found.</returns>
        private static DdsConversionFlags ExpansionFormat(DdsConversionFlags flags)
        {
            if ((flags & DdsConversionFlags.Palette) == DdsConversionFlags.Palette)
            {
                return ((flags & DdsConversionFlags.A8P8) == DdsConversionFlags.A8P8) ? DdsConversionFlags.A8P8 : DdsConversionFlags.Palette;
            }

            if ((flags & DdsConversionFlags.RGB888) == DdsConversionFlags.RGB888)
            {
                return DdsConversionFlags.RGB888;
            }

            if ((flags & DdsConversionFlags.RGB332) == DdsConversionFlags.RGB332)
            {
                return DdsConversionFlags.RGB332;
            }

            if ((flags & DdsConversionFlags.RGB8332) == DdsConversionFlags.RGB8332)
            {
                return DdsConversionFlags.RGB8332;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((flags & DdsConversionFlags.A4L4) == DdsConversionFlags.A4L4)
            {
                return DdsConversionFlags.A4L4;
            }

            return (flags & DdsConversionFlags.RGB4444) == DdsConversionFlags.RGB4444
                       ? DdsConversionFlags.RGB4444
                       : DdsConversionFlags.None;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to read the optional DX10 header.
        /// </summary>
        /// <param name="reader">The reader used to read the information from the underlying stream.</param>
        /// <param name="header">DDS header.</param>
        /// <param name="flags">Conversion flags.</param>
        /// <returns>A new image settings object.</returns>
        private GorgonImageInfo ReadDX10Header(GorgonBinaryReader reader, ref DdsHeader header, out DdsConversionFlags flags)
        {
            Dx10Header dx10Header = reader.ReadValue<Dx10Header>();
            flags = DdsConversionFlags.DX10;

            // If there's no array value here, then we're not getting what we expected and this file cannot be read.
            if (dx10Header.ArrayCount < 1)
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
            }

            BufferFormat format = BufferFormat.Unknown;

            // Ensure the format is correct.
            foreach (BufferFormat supportedFormat in _formats)
            {
                if (supportedFormat != dx10Header.Format)
                {
                    continue;
                }

                format = dx10Header.Format;
            }

            if (format == BufferFormat.Unknown)
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, dx10Header.Format));
            }

            var settings = new GorgonImageInfo(dx10Header.ResourceDimension, format);

            switch (settings.ImageType)
            {
                case ImageType.Image1D:
                    settings.ArrayCount = (int)dx10Header.ArrayCount;
                    break;
                case ImageType.Image2D:
                    // Make the array size a multiple of 6 if the DDS holds a texture cube map.
                    if ((dx10Header.MiscFlags & DdsHeaderMiscFlags.TextureCube) == DdsHeaderMiscFlags.TextureCube)
                    {
                        settings = new GorgonImageInfo(ImageType.ImageCube, format)
                        {
                            ArrayCount = (int)dx10Header.ArrayCount * 6
                        };
                    }
                    else
                    {
                        settings.ArrayCount = (int)dx10Header.ArrayCount;
                    }

                    settings.Height = (int)header.Height;

                    break;
                case ImageType.Image3D:
                    if ((header.Flags & DdsHeaderFlags.Volume) != DdsHeaderFlags.Volume)
                    {
                        throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                    }

                    settings.Height = (int)header.Height;
                    settings.Depth = (int)header.Depth;
                    break;
                default:
                    throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
            }

            settings.Width = (int)header.Width;

            return settings;
        }

        /// <summary>
        /// Function to retrieve the correct format from the header information.
        /// </summary>
        /// <param name="format">Format in the header.</param>
        /// <param name="flags">Flags to alter conversion behaviour.</param>
        /// <param name="conversionFlags">Flags to indicate the types of conversions.</param>
        /// <returns>The format of the buffer, or Unknown if the format is not supported.</returns>
        private BufferFormat GetFormat(ref DdsPixelFormat format, DdsLegacyFlags flags, out DdsConversionFlags conversionFlags)
        {
            DdsLegacyConversion conversion = default;

            foreach (DdsLegacyConversion ddsFormat in _legacyMapping)
            {
                if ((format.Flags & ddsFormat.PixelFormat.Flags) == 0)
                {
                    continue;
                }

                // Check to see if the FOURCC values match.
                if ((ddsFormat.PixelFormat.Flags & DdsPixelFormatFlags.FourCC) == DdsPixelFormatFlags.FourCC)
                {
                    if (ddsFormat.PixelFormat.FourCC != format.FourCC)
                    {
                        continue;
                    }

                    conversion = ddsFormat;
                    break;
                }

                if ((ddsFormat.PixelFormat.Flags & DdsPixelFormatFlags.PaletteIndexed) == DdsPixelFormatFlags.PaletteIndexed)
                {
                    // If indexed, then check the bit count.
                    if (ddsFormat.PixelFormat.BitCount != format.BitCount)
                    {
                        continue;
                    }

                    conversion = ddsFormat;
                    break;
                }

                if (ddsFormat.PixelFormat.BitCount != format.BitCount)
                {
                    continue;
                }

                // If the bit masks are the same, then use this one.
                if ((ddsFormat.PixelFormat.RBitMask != format.RBitMask)
                    || (ddsFormat.PixelFormat.GBitMask != format.GBitMask)
                    || (ddsFormat.PixelFormat.BBitMask != format.BBitMask)
                    || (ddsFormat.PixelFormat.ABitMask != format.ABitMask))
                {
                    continue;
                }

                conversion = ddsFormat;
                break;
            }

            conversionFlags = conversion.Flags;
            BufferFormat result = conversion.Format;

            if (conversion.Format == BufferFormat.Unknown)
            {
                return BufferFormat.Unknown;
            }

            // We do not want to expand the bit count to match, so we can't convert.
            if (((flags & DdsLegacyFlags.NoLegacyExpansion) == DdsLegacyFlags.NoLegacyExpansion) && ((conversionFlags & DdsConversionFlags.Expand) == DdsConversionFlags.Expand))
            {
                return BufferFormat.Unknown;
            }

            // Don't fix up RGB101010A2.
            if ((result == BufferFormat.R10G10B10A2_UNorm) && ((flags & DdsLegacyFlags.NoR10B10G10A2Fix) == DdsLegacyFlags.NoR10B10G10A2Fix))
            {
                conversionFlags ^= DdsConversionFlags.Swizzle;
            }

            return result;
        }

        /// <summary>
        /// Function to read in the DDS header from a stream.
        /// </summary>
        /// <param name="reader">The reader used to read data from the stream.</param>
        /// <param name="size">Size of the image, in bytes.</param>
        /// <param name="legacyFlags">Legacy flags to apply.</param>
        /// <param name="conversionFlags">The conversion flags.</param>
        /// <returns>New image settings.</returns>
        private IGorgonImageInfo ReadHeader(GorgonBinaryReader reader, long size, DdsLegacyFlags legacyFlags, out DdsConversionFlags conversionFlags)
        {
            GorgonImageInfo settings;

            // Start with no conversion.

            // Read the magic # from the header.
            uint magicNumber = reader.ReadUInt32();

            // If the magic number doesn't match, then this is not a DDS file.
            if (magicNumber != MagicNumber)
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
            }

            // Read the header from the file.
            DdsHeader header = reader.ReadValue<DdsHeader>();

            if (header.PixelFormat.SizeInBytes != Unsafe.SizeOf<DdsPixelFormat>())
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
            }

            // Ensure that we have at least one mip level.
            if (header.MipCount == 0)
            {
                header.MipCount = 1;
            }

            // Get DX 10 header information.
            if (((header.PixelFormat.Flags & DdsPixelFormatFlags.FourCC) == DdsPixelFormatFlags.FourCC) && (header.PixelFormat.FourCC == _pfDX10.FourCC))
            {
                if (size < Unsafe.SizeOf<Dx10Header>() + Unsafe.SizeOf<DdsHeader>() + sizeof(uint))
                {
                    throw new EndOfStreamException();
                }

                settings = ReadDX10Header(reader, ref header, out conversionFlags);
            }
            else
            {
                BufferFormat format = GetFormat(ref header.PixelFormat, legacyFlags, out conversionFlags);

                // If we actually have a volume texture, or we want to make one.
                if ((header.Flags & DdsHeaderFlags.Volume) == DdsHeaderFlags.Volume)
                {
                    settings = new GorgonImageInfo(ImageType.Image3D, format)
                    {
                        Depth = (int)header.Depth.Max(1)
                    };
                }
                else
                {
                    if ((header.Caps2 & DdsCaps2.CubeMap) == DdsCaps2.CubeMap)
                    {
                        // Only allow all faces.
                        if ((header.Caps2 & DdsCaps2.AllFaces) != DdsCaps2.AllFaces)
                        {
                            throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                        }

                        settings = new GorgonImageInfo(ImageType.ImageCube, format)
                        {
                            ArrayCount = 6,
                            Depth = 1
                        };
                    }
                    else
                    {
                        settings = new GorgonImageInfo(ImageType.Image2D, format)
                        {
                            ArrayCount = 1,
                            Depth = 1
                        };
                    }
                }

                settings.Width = (int)header.Width;
                settings.Height = (int)header.Height;

                if (settings.Format == BufferFormat.Unknown)
                {
                    throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, settings.Format));
                }
            }

            var formatInfo = new GorgonFormatInfo(settings.Format);

            if (formatInfo.IsCompressed)
            {
                int modWidth = settings.Width % 4;
                int modHeight = settings.Height % 4;

                if ((modWidth != 0) || (modHeight != 0))
                {
                    throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                }
            }

            settings.MipCount = (int)header.MipCount;

            // Special flag for handling BGR DXGI 1.1 formats
            if ((legacyFlags & DdsLegacyFlags.ForceRGB) == DdsLegacyFlags.ForceRGB)
            {
                switch (settings.Format)
                {
                    case BufferFormat.B8G8R8A8_UNorm:
                        settings.Format = BufferFormat.R8G8B8A8_UNorm;
                        conversionFlags |= DdsConversionFlags.Swizzle;
                        break;
                    case BufferFormat.B8G8R8X8_UNorm:
                        settings.Format = BufferFormat.R8G8B8A8_UNorm;
                        conversionFlags |= DdsConversionFlags.Swizzle | DdsConversionFlags.NoAlpha;
                        break;
                    case BufferFormat.B8G8R8A8_Typeless:
                        settings.Format = BufferFormat.R8G8B8A8_Typeless;
                        conversionFlags |= DdsConversionFlags.Swizzle;
                        break;
                    case BufferFormat.B8G8R8A8_UNorm_SRgb:
                        settings.Format = BufferFormat.R8G8B8A8_UNorm_SRgb;
                        conversionFlags |= DdsConversionFlags.Swizzle;
                        break;

                    case BufferFormat.B8G8R8X8_Typeless:
                        settings.Format = BufferFormat.R8G8B8A8_Typeless;
                        conversionFlags |= DdsConversionFlags.Swizzle | DdsConversionFlags.NoAlpha;
                        break;
                    case BufferFormat.B8G8R8X8_UNorm_SRgb:
                        settings.Format = BufferFormat.R8G8B8A8_UNorm_SRgb;
                        conversionFlags |= DdsConversionFlags.Swizzle | DdsConversionFlags.NoAlpha;
                        break;
                }
            }

            // Special flag for handling 16bpp formats
            if ((legacyFlags & DdsLegacyFlags.No16BPP) != DdsLegacyFlags.No16BPP)
            {
                return settings;
            }

            switch (settings.Format)
            {
                case BufferFormat.B5G6R5_UNorm:
                case BufferFormat.B5G5R5A1_UNorm:
                    settings.Format = BufferFormat.R8G8B8A8_UNorm;
                    conversionFlags |= DdsConversionFlags.Expand;
                    if (settings.Format == BufferFormat.B5G6R5_UNorm)
                    {
                        conversionFlags |= DdsConversionFlags.NoAlpha;
                    }
                    break;
            }

            return settings;
        }

        /// <summary>
        /// Function to expand out legacy formats.
        /// </summary>
        /// <param name="src">The pointer to the source data.</param>
        /// <param name="srcPitch">The pitch of the source data.</param>
        /// <param name="srcFormat">Format to convert from.</param>
        /// <param name="dest">The pointer to the destination data.</param>
        /// <param name="destPitch">The pitch of the destination data.</param>
        /// <param name="destFormat">The destination format.</param>
        /// <param name="bitFlags">Image bit conversion control flags.</param>
        /// <param name="palette">Palette to assigned to indexed images.</param>
        private static void ExpandLegacyScanline(in GorgonPtr<byte> src, int srcPitch, DdsConversionFlags srcFormat, in GorgonPtr<byte> dest, int destPitch, BufferFormat destFormat, ImageBitFlags bitFlags, uint[] palette)
        {
            if (((srcFormat == DdsConversionFlags.RGB332) && (destFormat != BufferFormat.B5G6R5_UNorm))
                || ((srcFormat != DdsConversionFlags.RGB332) && (destFormat != BufferFormat.R8G8B8A8_UNorm)))
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, destFormat));
            }

            uint[] actualPalette = palette;

            try
            {
                if (((srcFormat == DdsConversionFlags.Palette) || (srcFormat == DdsConversionFlags.A8P8)) && ((palette is null) || (actualPalette.Length != 256)))
                {
                    // Create an empty palette if we didn't supply one.
                    actualPalette = GorgonArrayPool<uint>.SharedTiny.Rent(256);
                }

                unsafe
                {
                    switch (srcFormat)
                    {
                        case DdsConversionFlags.Palette:
                            {
                                byte* srcPtr = (byte*)src;
                                uint* destPtr = (uint*)dest;

                                // Copy indexed data.
                                for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
                                {
                                    *(destPtr++) = actualPalette[*(srcPtr++)];
                                }
                            }
                            break;
                        case DdsConversionFlags.A4L4:
                            {
                                byte* srcPtr = (byte*)src;
                                uint* destPtr = (uint*)dest;

                                // Copy alpha luminance.
                                for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
                                {
                                    byte pixel = *(srcPtr++);

                                    uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)(((pixel & 0xF0) << 24) | ((pixel & (0xF0 << 20))));
                                    uint lum = (uint)((pixel & (0x0F << 4)) | (pixel & 0x0F));

                                    *(destPtr++) = lum | (lum << 8) | (lum << 16) | alpha;
                                }
                            }
                            break;
                        case DdsConversionFlags.RGB332:
                            {
                                byte* srcPtr = (byte*)src;

                                switch (destFormat)
                                {
                                    case BufferFormat.R8G8B8A8_UNorm:
                                        {
                                            uint* destPtr = (uint*)dest;

                                            // Copy 8 bit RGB.
                                            for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
                                            {
                                                byte pixel = *(srcPtr++);

                                                uint r = (uint)((pixel & 0xE0) | ((pixel & 0xE0) >> 3) | ((pixel & 0xC0) >> 6));
                                                uint g = (uint)(((pixel & 0x1C) << 11) | ((pixel & 0x1C) << 8) | ((pixel & 0x18) << 5));
                                                uint b = (uint)(((pixel & 0x03) << 22) | ((pixel & 0x03) << 20) | ((pixel & 0x03) << 18) | ((pixel & 0x03) << 16));

                                                *(destPtr++) = r | g | b | 0xFF000000;
                                            }
                                        }
                                        break;
                                    case BufferFormat.B5G6R5_UNorm:
                                        {
                                            ushort* destPtr = (ushort*)dest;

                                            // Copy 8 bit RGB.
                                            for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 2)
                                            {
                                                byte pixel = *(srcPtr++);

                                                uint r = (uint)(((pixel & 0xE0) << 8) | ((pixel & 0xC0) << 5));
                                                uint g = (uint)(((pixel & 0x1C) << 6) | ((pixel & 0x1C) << 3));
                                                uint b = (uint)(((pixel & 0x03) << 3) | ((pixel & 0x03) << 1) | ((pixel & 0x02) >> 1));

                                                *(destPtr++) = (ushort)(r | g | b);
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case DdsConversionFlags.A8P8:
                            {
                                ushort* srcPtr = (ushort*)src;
                                uint* destPtr = (uint*)dest;

                                // Copy indexed data with alpha.
                                for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
                                {
                                    ushort pixel = *(srcPtr++);
                                    uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)((pixel & 0xFF00) << 16);

                                    *(destPtr++) = pixel | alpha;
                                }
                            }
                            break;
                        case DdsConversionFlags.RGB8332:
                            {
                                ushort* srcPtr = (ushort*)src;
                                uint* destPtr = (uint*)dest;

                                // Copy 8 bit RGB with alpha.
                                for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
                                {
                                    ushort pixel = (ushort)(*(srcPtr++) & 0xFF);
                                    uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)((pixel & 0xFF00) << 16);

                                    uint r = (uint)((pixel & 0xE0) | ((pixel & 0xE0) >> 3) | ((pixel & 0xC0) >> 6));
                                    uint g = (uint)(((pixel & 0x1C) << 11) | ((pixel & 0x1C) << 8) | ((pixel & 0x18) << 5));
                                    uint b = (uint)(((pixel & 0x03) << 22) | ((pixel & 0x03) << 20) | ((pixel & 0x03) << 18) | ((pixel & 0x03) << 16));

                                    *(destPtr++) = r | g | b | alpha;
                                }
                            }
                            break;
                        case DdsConversionFlags.RGB4444:
                            {
                                ushort* srcPtr = (ushort*)src;
                                uint* destPtr = (uint*)dest;

                                // Copy 12 bit RGB with 4 bit alpha.
                                for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
                                {
                                    ushort pixel = *(srcPtr++);
                                    uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)(((pixel & 0xF000) << 16) | ((pixel & 0xF000) << 12));

                                    uint r = (uint)(((pixel & 0x0F00) >> 4) | ((pixel & 0x0F00) >> 8));
                                    uint g = (uint)(((pixel & 0x00F0) << 4) | ((pixel & 0x00F0) << 8));
                                    uint b = (uint)(((pixel & 0x000F) << 16) | ((pixel & 0x000F) << 20));

                                    *(destPtr++) = r | g | b | alpha;
                                }
                            }
                            break;
                        case DdsConversionFlags.RGB888:
                            {
                                byte* srcPtr = (byte*)src;
                                uint* destPtr = (uint*)dest;

                                // Copy 24 bit RGB.
                                for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
                                {
                                    // 24 bit DDS files are encoded as BGR, need to swizzle.
                                    uint b = (uint)(*(srcPtr++) << 16);
                                    uint g = (uint)(*(srcPtr++) << 8);
                                    byte r = *(srcPtr++);

                                    *(destPtr++) = r | g | b | 0xFF000000;
                                }
                            }
                            break;
                    }
                }
            }
            finally
            {
                GorgonArrayPool<uint>.SharedTiny.Return(actualPalette);
            }
        }

        /// <summary>
        /// Function to write out the DDS header to the stream.
        /// </summary>
        /// <param name="settings">Meta data for the image header.</param>
        /// <param name="writer">Writer interface for the stream.</param>
        /// <param name="flags">Legacy file format flags.</param>
        private void WriteHeader(IGorgonImageInfo settings, GorgonBinaryWriter writer, DdsLegacyFlags flags)
        {
            var header = new DdsHeader();
            DdsPixelFormat? format = null;
            var formatInfo = new GorgonFormatInfo(settings.Format);

            if ((settings.ArrayCount > 1) && ((settings.ArrayCount != 6) || (settings.ImageType != ImageType.Image2D) || (settings.ImageType != ImageType.ImageCube)))
            {
                flags |= DdsLegacyFlags.ForceDX10;
            }

            // If we're not forcing the DX10 header, then do a legacy conversion.
            if ((flags & DdsLegacyFlags.ForceDX10) != DdsLegacyFlags.ForceDX10)
            {
                switch (settings.Format)
                {
                    case BufferFormat.R8G8B8A8_UNorm:
                        format = _pfA8B8G8R8;
                        break;
                    case BufferFormat.R16G16_UNorm:
                        format = _pfG16R16;
                        break;
                    case BufferFormat.R8G8_UNorm:
                        format = _pfA8L8;
                        break;
                    case BufferFormat.R16_UNorm:
                        format = _pfL16;
                        break;
                    case BufferFormat.R8_UNorm:
                        format = _pfL8;
                        break;
                    case BufferFormat.A8_UNorm:
                        format = _pfA8;
                        break;
                    case BufferFormat.R8G8_B8G8_UNorm:
                        format = _pfR8G8_B8G8;
                        break;
                    case BufferFormat.G8R8_G8B8_UNorm:
                        format = _pfG8R8_G8B8;
                        break;
                    case BufferFormat.BC1_UNorm:
                        format = _pfDxt1;
                        break;
                    case BufferFormat.BC2_UNorm:
                        format = _pfDxt3;
                        break;
                    case BufferFormat.BC3_UNorm:
                        format = _pfDxt5;
                        break;
                    case BufferFormat.BC4_UNorm:
                        format = _pfBC4U;
                        break;
                    case BufferFormat.BC4_SNorm:
                        format = _pfBC4S;
                        break;
                    case BufferFormat.BC5_UNorm:
                        format = _pfBC5U;
                        break;
                    case BufferFormat.BC5_SNorm:
                        format = _pfBC5S;
                        break;
                    case BufferFormat.B5G6R5_UNorm:
                        format = _pfR5G6B5;
                        break;
                    case BufferFormat.B5G5R5A1_UNorm:
                        format = _pfA1R5G5B5;
                        break;
                    case BufferFormat.B8G8R8A8_UNorm:
                        format = _pfA8R8G8B8;
                        break;
                    case BufferFormat.B8G8R8X8_UNorm:
                        format = _pfX8R8G8B8;
                        break;
                    case BufferFormat.R32G32B32A32_Float:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0);
                        break;
                    case BufferFormat.R16G16B16A16_Float:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0);
                        break;
                    case BufferFormat.R16G16B16A16_UNorm:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0);
                        break;
                    case BufferFormat.R16G16B16A16_SNorm:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0);
                        break;
                    case BufferFormat.R32G32_Float:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0);
                        break;
                    case BufferFormat.R16G16_Float:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0);
                        break;
                    case BufferFormat.R32_Float:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0);
                        break;
                    case BufferFormat.R16_Float:
                        format = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0);
                        break;
                }
            }

            // Write the DDS magic # ID.
            writer.Write(MagicNumber);

            // Set up the header.
            header.Size = (uint)Unsafe.SizeOf<DdsHeader>();
            header.Flags = DdsHeaderFlags.Texture;
            header.Caps1 = DdsCaps1.Texture;

            // Get mip map info.
            if (settings.MipCount > 0)
            {
                header.Flags |= DdsHeaderFlags.MipMap;
                header.MipCount = (uint)settings.MipCount;

                if (settings.MipCount > 1)
                {
                    header.Caps1 |= DdsCaps1.MipMap;
                }
            }

            switch (settings.ImageType)
            {
                case ImageType.Image1D:
                    header.Width = (uint)settings.Width;
                    header.Depth = header.Height = 1;
                    break;
                case ImageType.ImageCube:
                case ImageType.Image2D:
                    header.Width = (uint)settings.Width;
                    header.Height = (uint)settings.Height;
                    header.Depth = 1;

                    if (settings.ImageType == ImageType.ImageCube)
                    {
                        header.Caps1 |= DdsCaps1.CubeMap;
                        header.Caps2 |= DdsCaps2.AllFaces;
                    }
                    break;
                case ImageType.Image3D:
                    header.Width = (uint)settings.Width;
                    header.Height = (uint)settings.Height;
                    header.Depth = (uint)settings.Depth;
                    header.Flags |= DdsHeaderFlags.Volume;
                    header.Caps2 |= DdsCaps2.Volume;
                    break;
                default:
                    throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
            }

            // Get pitch information.
            GorgonPitchLayout pitchInfo = formatInfo.GetPitchForFormat(settings.Width, settings.Height);

            if (formatInfo.IsCompressed)
            {
                header.Flags |= DdsHeaderFlags.LinearSize;
                header.PitchOrLinearSize = (uint)pitchInfo.SlicePitch;
            }
            else
            {
                header.Flags |= DdsHeaderFlags.RowPitch;
                header.PitchOrLinearSize = (uint)pitchInfo.RowPitch;
            }

            // Get pixel format.
            header.PixelFormat = format ?? _pfDX10;

            // Write out the header.
            writer.WriteValue(ref header);

            // If we didn't map a legacy format, then use the DX 10 header.
            if (format is not null)
            {
                return;
            }

            Dx10Header dx10Header = default;

            dx10Header.Format = settings.Format;
            if (settings.ImageType != ImageType.ImageCube)
            {
                dx10Header.ResourceDimension = settings.ImageType;
                dx10Header.ArrayCount = (uint)settings.ArrayCount;
            }
            else
            {
                dx10Header.ResourceDimension = ImageType.Image2D;
                dx10Header.MiscFlags |= DdsHeaderMiscFlags.TextureCube;
                dx10Header.ArrayCount = (uint)(settings.ArrayCount / 6);
            }

            writer.WriteValue(ref dx10Header);
        }

        /// <summary>
        /// Function to expand a scanline from a lower bit depth, to a higher bit depth.
        /// </summary>
        /// <param name="srcData">The pointer to the data to read from.</param>
        /// <param name="destData">The pointer to the data to write to.</param>
        /// <param name="srcRowPitch">The number of bytes for the source scanline.</param>
        /// <param name="destRowPitch">The number of bytes for the destination scanline.</param>
        /// <param name="format">The pixel format of the image data.</param>
        /// <param name="flags">Conversion flags used to determine how to expand the scanline.</param>
        /// <param name="imageBitFlags">Bit depth conversion flags.</param>
        /// <param name="palette">A palette, used to expand indexed 8 bpp image data.</param>
        private static void ExpandScanline(in GorgonPtr<byte> srcData, in GorgonPtr<byte> destData, int srcRowPitch, int destRowPitch, BufferFormat format, DdsConversionFlags flags, ImageBitFlags imageBitFlags, uint[] palette)
        {
            // Perform expansion.
            if (((flags & DdsConversionFlags.RGB565) == DdsConversionFlags.RGB565)
                || ((flags & DdsConversionFlags.RGB5551) == DdsConversionFlags.RGB5551))
            {
                BufferFormat expandFormat = BufferFormat.B5G6R5_UNorm;

                if ((flags & DdsConversionFlags.RGB5551) == DdsConversionFlags.RGB5551)
                {
                    expandFormat = BufferFormat.B5G5R5A1_UNorm;
                }

                if ((flags & DdsConversionFlags.RGB4444) == DdsConversionFlags.RGB4444)
                {
                    expandFormat = BufferFormat.B4G4R4A4_UNorm;
                }

                ImageUtilities.Expand16BPPScanline(in srcData, srcRowPitch, expandFormat, in destData, destRowPitch, imageBitFlags);
                
                return;
            }

            // If we're 8 bit or some other type of format, then expand to match.
            DdsConversionFlags expandLegacyFormat = ExpansionFormat(flags);

            if (expandLegacyFormat == DdsConversionFlags.None)
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, expandLegacyFormat));
            }

            ExpandLegacyScanline(in srcData, srcRowPitch, expandLegacyFormat, in destData, destRowPitch, format, imageBitFlags, palette);
        }

        /// <summary>
        /// Function to perform the copying of image data into the buffer.
        /// </summary>
        /// <param name="reader">The reader used to read the data from the underlying stream.</param>
        /// <param name="image">Image data.</param>
        /// <param name="pitchFlags">Flags used to determine pitch when expanding pixels.</param>
        /// <param name="conversionFlags">Flags used for conversion between legacy formats and the current format.</param>
        /// <param name="palette">Palette used in indexed conversion.</param>
        private void CopyImageData(GorgonBinaryReader reader, GorgonImage image, PitchFlags pitchFlags, DdsConversionFlags conversionFlags, uint[] palette)
        {
            var formatInfo = new GorgonFormatInfo(image.Format);

            // Get copy flag bits per pixel if we have an expansion.
            if ((conversionFlags & DdsConversionFlags.Expand) == DdsConversionFlags.Expand)
            {
                if ((conversionFlags & DdsConversionFlags.RGB888) == DdsConversionFlags.RGB888)
                {
                    pitchFlags |= PitchFlags.BPP24;
                }
                else if (((conversionFlags & DdsConversionFlags.RGB565) == DdsConversionFlags.RGB565)
                            || ((conversionFlags & DdsConversionFlags.RGB5551) == DdsConversionFlags.RGB5551)
                            || ((conversionFlags & DdsConversionFlags.RGB4444) == DdsConversionFlags.RGB4444)
                            || ((conversionFlags & DdsConversionFlags.RGB332) == DdsConversionFlags.RGB8332)
                            || ((conversionFlags & DdsConversionFlags.RGB332) == DdsConversionFlags.A8P8))
                {
                    pitchFlags |= PitchFlags.BPP16;
                }
                else if (((conversionFlags & DdsConversionFlags.A4L4) == DdsConversionFlags.A4L4)
                            || ((conversionFlags & DdsConversionFlags.RGB332) == DdsConversionFlags.RGB332)
                            || ((conversionFlags & DdsConversionFlags.Palette) == DdsConversionFlags.Palette))
                {
                    pitchFlags |= PitchFlags.BPP8;
                }
            }

            // Get the size of the source image in bytes, and its pitch information.
            int sizeInBytes = GorgonImage.CalculateSizeInBytes(image, pitchFlags);

            if (sizeInBytes > image.SizeInBytes)
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
            }

            // If no conversion is to take place, then just do a straight dump into memory.
            if (((conversionFlags == DdsConversionFlags.None)
                        || (conversionFlags == DdsConversionFlags.DX10))
                    && (pitchFlags == PitchFlags.None))
            {
                // First mip, array and depth slice is at the start of our image memory buffer.
                reader.ReadRange(image.ImageData, count: sizeInBytes);
                return;
            }

            ImageBitFlags expFlags = ImageBitFlags.None;

            if ((conversionFlags & DdsConversionFlags.NoAlpha) == DdsConversionFlags.NoAlpha)
            {
                expFlags |= ImageBitFlags.OpaqueAlpha;
            }

            if ((conversionFlags & DdsConversionFlags.Swizzle) == DdsConversionFlags.Swizzle)
            {
                expFlags |= ImageBitFlags.Legacy;
            }

            int depth = image.Depth;
            GorgonNativeBuffer<byte> lineBuffer = null;

            try
            {
                for (int array = 0; array < image.ArrayCount; array++)
                {
                    for (int mipLevel = 0; mipLevel < image.MipCount; mipLevel++)
                    {
                        // Get our destination buffer.
                        IGorgonImageBuffer destBuffer = image.Buffers[mipLevel, array];
                        GorgonPitchLayout pitchInfo = formatInfo.GetPitchForFormat(destBuffer.Width, destBuffer.Height, pitchFlags);
                        GorgonPtr<byte> destPointer = destBuffer.Data;

                        for (int slice = 0; slice < depth; slice++)
                        {
                            // We're using compressed data, just copy.
                            if (formatInfo.IsCompressed)
                            {
                                int size = pitchInfo.SlicePitch.Min(destBuffer.PitchInformation.SlicePitch);
                                reader.ReadRange(destBuffer.Data, count: size);
                                continue;
                            }

                            // Read each scan line if we require some form of conversion. 
                            for (int h = 0; h < destBuffer.Height; h++)
                            {
                                // Use this to read a line of data from the source.
                                if (lineBuffer is null)
                                {
                                    lineBuffer = new GorgonNativeBuffer<byte>(pitchInfo.RowPitch);
                                }

                                reader.ReadRange<byte>(lineBuffer, count: pitchInfo.RowPitch);

                                ref readonly GorgonPtr<byte> srcPointer = ref lineBuffer.Pointer;

                                if ((conversionFlags & DdsConversionFlags.Expand) == DdsConversionFlags.Expand)
                                {
                                    ExpandScanline(in srcPointer,
                                                   in destPointer,
                                                   pitchInfo.RowPitch,
                                                   destBuffer.PitchInformation.RowPitch,
                                                   image.Format,
                                                   conversionFlags,
                                                   expFlags,
                                                   palette);
                                }
                                else if ((conversionFlags & DdsConversionFlags.Swizzle) == DdsConversionFlags.Swizzle)
                                {
                                    // Perform swizzle.
                                    ImageUtilities.SwizzleScanline(in srcPointer, pitchInfo.RowPitch, in destPointer, destBuffer.PitchInformation.RowPitch, image.Format, expFlags);
                                }
                                else
                                {
                                    // Copy and set constant alpha (if necessary).
                                    ImageUtilities.CopyScanline(in srcPointer, pitchInfo.RowPitch, in destPointer, destBuffer.PitchInformation.RowPitch, image.Format, expFlags);
                                }

                                // Increment our pointer data by one line.
                                destPointer += destBuffer.PitchInformation.RowPitch;
                            }
                        }

                        if (depth > 1)
                        {
                            depth >>= 1;
                        }
                    }
                }
            }
            finally
            {
                lineBuffer?.Dispose();
            }            
        }

        /// <summary>
        /// Function to decode an image from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the image data to read.</param>
        /// <param name="size">The size of the image within the stream, in bytes.</param>
        /// <returns>A <see cref="IGorgonImage"/> containing the image data from the stream.</returns>
        /// <exception cref="GorgonException">Thrown when the image data in the stream has a pixel format that is unsupported.</exception>
        /// <remarks>
        /// <para>
        /// A codec must implement this method in order to decode the image data. 
        /// </para>
        /// <para>
        /// When the image is loaded, it is read in its native format into memory first, and then this method is called to decode the data in memory into a <see cref="IGorgonImage"/> object.  While this 
        /// consumes more memory, it is necessary when handling streams that do not have seek capability (e.g. <see cref="System.Net.Sockets.NetworkStream"/>).
        /// </para>
        /// </remarks>
        protected override IGorgonImage OnDecodeFromStream(Stream stream, long size)
        {
            uint[] palette = null;

            if (size < Unsafe.SizeOf<DdsHeader>() + sizeof(uint))
            {
                throw new EndOfStreamException();
            }

            var reader = new GorgonBinaryReader(stream, true);

            // Read the header information.
            IGorgonImageInfo settings = ReadHeader(reader, size, DecodingOptions?.LegacyFormatConversionFlags ?? DdsLegacyFlags.None, out DdsConversionFlags flags);

            var imageData = new GorgonImage(settings);

            try
            {
                // We have a palette, either create a new one or clone the assigned one.
                if ((flags & DdsConversionFlags.Palette) == DdsConversionFlags.Palette)
                {
                    const int paletteSize = sizeof(uint) * 256;

                    if (paletteSize > stream.Length - stream.Position)
                    {
                        throw new EndOfStreamException();
                    }

                    palette = new uint[256];

                    if ((DecodingOptions?.Palette is not null) && (DecodingOptions.Palette.Count > 0))
                    {
                        int count = DecodingOptions.Palette.Count.Min(256);

                        for (int i = 0; i < count; i++)
                        {
                            palette[i] = (uint)DecodingOptions.Palette[i].ToARGB();
                        }

                        // Skip past palette data since we're not using it.
                        count = 256;
                        while (count > 0)
                        {
                            reader.ReadUInt32();
                            --count;
                        }
                    }
                    else
                    {
                        // Read from the stream if we haven't assigned a palette.
                        reader.ReadRange(palette, 0, 256);
                    }
                }

                DdsLegacyFlags legacyFlags = DecodingOptions?.LegacyFormatConversionFlags ?? DdsLegacyFlags.None;

                // Copy the data from the stream to the buffer.
                CopyImageData(reader,
                              imageData,
                              ((legacyFlags & DdsLegacyFlags.LegacyDWORD) == DdsLegacyFlags.LegacyDWORD)
                                  ? PitchFlags.LegacyDWORD
                                  : PitchFlags.None,
                              flags,
                              palette);
            }
            catch
            {
                // Clean up any memory allocated if we can't copy the image.
                imageData.Dispose();

                throw;
            }

            return imageData;
        }

        /// <summary>
        /// Function to persist a <see cref="IGorgonImage"/> to a stream.
        /// </summary>
        /// <param name="imageData">A <see cref="IGorgonImage"/> to persist to the stream.</param>
        /// <param name="stream">The stream that will receive the image data.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="imageData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="stream"/> is read only.</exception>
        /// <exception cref="NotSupportedException">Thrown when the image data in the stream has a pixel format that is unsupported by the codec.</exception>
        /// <remarks>
        /// <para>
        /// When persisting image data via a codec, the image must have a format that the codec can recognize. This list of supported formats is provided by the 
        /// <see cref="GorgonImageCodec{TEncOpt, TDecOpt}.SupportedPixelFormats"/> property. Applications may convert their image data a supported format before saving the data using a codec.
        /// </para>
        /// </remarks>
        public override void Save(IGorgonImage imageData, Stream stream)
        {
            if (imageData is null)
            {
                throw new ArgumentNullException(nameof(imageData));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException(Resources.GORIMG_ERR_STREAM_IS_READONLY);
            }

            if (Array.IndexOf(_formats, imageData.Format) == -1)
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Format));
            }

            // Use a binary writer.
            using var writer = new GorgonBinaryWriter(stream, true);
            // Write the header for the file.
            WriteHeader(imageData, writer, DdsLegacyFlags.None);

            // Write image data.
            switch (imageData.ImageType)
            {
                case ImageType.Image1D:
                case ImageType.Image2D:
                case ImageType.ImageCube:
                    for (int array = 0; array < imageData.ArrayCount; array++)
                    {
                        for (int mipLevel = 0; mipLevel < imageData.MipCount; mipLevel++)
                        {
                            IGorgonImageBuffer buffer = imageData.Buffers[mipLevel, array];
                            writer.WriteRange(buffer.Data, count: buffer.PitchInformation.SlicePitch);
                        }
                    }
                    break;
                case ImageType.Image3D:
                    int depth = imageData.Depth;
                    for (int mipLevel = 0; mipLevel < imageData.MipCount; mipLevel++)
                    {
                        for (int slice = 0; slice < depth; slice++)
                        {
                            IGorgonImageBuffer buffer = imageData.Buffers[mipLevel, slice];
                            writer.WriteRange(buffer.Data, count: buffer.PitchInformation.SlicePitch);
                        }

                        if (depth > 1)
                        {
                            depth >>= 1;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to read the meta data for image data within a stream.
        /// </summary>
        /// <param name="stream">The stream containing the metadata to read.</param>
        /// <returns>
        /// The image meta data as a <see cref="IGorgonImageInfo"/> value.
        /// </returns>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
        /// <para>-or-</para>
        /// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>
        /// <para>
        /// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so 
        /// may cause undesirable results.
        /// </para> 
        /// </remarks>
        public override IGorgonImageInfo GetMetaData(Stream stream)
        {
            // Allocate enough space to hold the header and the DX 10 header and the magic number.
            int headerSize = Unsafe.SizeOf<Dx10Header>() + Unsafe.SizeOf<DdsHeader>() + sizeof(uint);
            long position = 0;

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
            }

            if (stream.Length - stream.Position < headerSize)
            {
                // If this isn't a DX10 DDS file, then check for the older format.
                headerSize = Unsafe.SizeOf<DdsHeader>() + sizeof(uint);

                if (stream.Length - stream.Position < headerSize)
                {
                    throw new EndOfStreamException();
                }
            }

            GorgonBinaryReader reader = null;

            try
            {
                position = stream.Position;
                reader = new GorgonBinaryReader(stream, true);
                return ReadHeader(reader, headerSize, DdsLegacyFlags.None, out _);
            }
            finally
            {
                stream.Position = position;
                reader?.Dispose();
            }
        }

        /// <summary>
        /// Function to determine if this codec can read the file or not.
        /// </summary>
        /// <param name="stream">Stream used to read the file information.</param>
        /// <returns>
        /// <b>true</b> if the codec can read the file, <b>false</b> if not.
        /// </returns>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        public override bool IsReadable(Stream stream)
        {
            uint magicNumber;
            long position = 0;

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
            }

            if (stream.Length - stream.Position < sizeof(uint) + Unsafe.SizeOf<DdsHeader>())
            {
                return false;
            }

            GorgonBinaryReader reader = null;

            try
            {
                position = stream.Position;
                reader = new GorgonBinaryReader(stream, true);
                magicNumber = reader.ReadUInt32();
            }
            finally
            {
                stream.Position = position;
                reader?.Dispose();
            }
            return magicNumber == MagicNumber;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecDds" /> class.
        /// </summary>
        /// <param name="decodingOptions">[Optional] Codec specific options to use when decoding image data.</param>
        public GorgonCodecDds(GorgonDdsDecodingOptions decodingOptions = null)
            : base(null, decodingOptions)
        {
            CodecCommonExtensions = new[] { "dds" };

            _formats = (from format in (BufferFormat[])Enum.GetValues(typeof(BufferFormat))
                        let info = new GorgonFormatInfo(format)
                        where format != BufferFormat.Unknown && !info.IsTypeless
                        select format).ToArray();
        }
        #endregion
    }
}
