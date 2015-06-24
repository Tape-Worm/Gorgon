#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, February 6, 2013 5:53:58 PM
// 

// This code was adapted from:
// SharpDX by Alexandre Mutel (http://sharpdx.org)
// DirectXTex by Chuck Walburn (http://directxtex.codeplex.com)

#region SharpDX/DirectXTex licenses
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.IO
{
	// ReSharper disable ForCanBeConvertedToForeach

    #region Enums.
    /// <summary>
	/// DDS Flags.
	/// </summary>
	[Flags]
	public enum DDSFlags
		: uint
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0,		
		/// <summary>
		/// Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)
		/// </summary>
		LegacyDWORD = 0x1,
		/// <summary>
		/// Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8) 
		/// </summary>
		NoLegacyExpansion = 0x2,
		/// <summary>
		/// Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks
		/// </summary>
		NoR10B10G10A2Fix = 0x4,
		/// <summary>
		/// Convert DXGI 1.1 BGR formats to BufferFormat.R8G8B8A8_UIntNormal to avoid use of optional WDDM 1.1 formats
		/// </summary>
		ForceRGB = 0x8,
		/// <summary>
		/// Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats
		/// </summary>
		No16BPP = 0x10,
		/// <summary>
		/// Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)
		/// </summary>
		ForceDX10 = 0x10000		
	};

	/// <summary>
	/// Flags to convert older DDS files (pre-DX10).
	/// </summary>
	[Flags]
	enum DDSConversionFlags
	{
		/// <summary>
		/// No conversion flags.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Requires expanded pixel size.
		/// </summary>
		Expand = 0x1,
		/// <summary>
		/// Requires setting alpha to known value.
		/// </summary>
		NoAlpha = 0x2,
		/// <summary>
		/// BGR/RGB reordering required.
		/// </summary>
		Swizzle = 0x4,
		/// <summary>
		/// Source has an 8-bit palette.
		/// </summary>
		Palette = 0x8,
		/// <summary>
		/// Source is 24 bit format.
		/// </summary>
		RGB888 = 0x10,
		/// <summary>
		/// Source is 16 bit format.
		/// </summary>
		RGB565 = 0x20,
		/// <summary>
		/// Source is 16 bit format.
		/// </summary>
		RGB5551 = 0x40,
		/// <summary>
		/// Source is 16 bit format.
		/// </summary>
		RGB4444 = 0x80,
		/// <summary>
		/// Source is 8 bit format.
		/// </summary>
		A4L4 = 0x100,
		/// <summary>
		/// Source is 8 bit format.
		/// </summary>
		RGB332 = 0x200,
		/// <summary>
		/// Source is 16 bit format.
		/// </summary>
		RGB8332 = 0x400,
		/// <summary>
		/// Source has an 8 bit palette with an alpha channel.
		/// </summary>
		A8P8 = 0x800,
		/// <summary>
		/// DirectX 10 extension header.
		/// </summary>
		DX10 = 0x10000
	};

	/// <summary>
	/// Flags for the header.
	/// </summary>
	[Flags]
	enum DDSHeaderFlags
		: uint
	{
		/// <summary>
		/// File contains texture data.
		/// </summary>
		Texture = 0x1007,
		/// <summary>
		/// File contains mip-map data.
		/// </summary>
		MipMap = 0x20000,
		/// <summary>
		/// File contains volume texture data.
		/// </summary>
		Volume = 0x800000,
		/// <summary>
		/// Row pitch information.
		/// </summary>
		RowPitch = 0x8,
		/// <summary>
		/// Linear size information.
		/// </summary>
		LinearSize = 0x80000,
		/// <summary>
		/// Width.
		/// </summary>
		Width = 0x2,
		/// <summary>
		/// Height.
		/// </summary>
		Height = 0x4
	}

	/// <summary>
	/// Misc. flags for the header.
	/// </summary>
	[Flags]
	enum DDSHeaderMiscFlags
		: uint
	{
		/// <summary>
		/// Resource is a texture cube.
		/// </summary>
		TextureCube = 0x4
	}

	/// <summary>
	/// Flags for the pixel format.
	/// </summary>
	[Flags]
	enum DDSPixelFormatFlags
		: uint
	{
		/// <summary>
		/// Four CC.
		/// </summary>
		FourCC = 0x4,
		/// <summary>
		/// RGB data.
		/// </summary>
		RGB = 0x40,
		/// <summary>
		/// RGB + Alpha data.
		/// </summary>
		RGBA = 0x41,
		/// <summary>
		/// Luminance data.
		/// </summary>
		Luminance = 0x20000,
		/// <summary>
		/// Luminance + alpha data.
		/// </summary>
		LuminanceAlpha = 0x20001,
		/// <summary>
		/// Alpha data.
		/// </summary>
		Alpha = 0x2,
		/// <summary>
		/// Palette/indexed data.
		/// </summary>
		PaletteIndexed = 0x20
	}

	// ReSharper disable InconsistentNaming
	/// <summary>
	/// DDS surface flags.
	/// </summary>
	[Flags]
	enum DDSCAPS1
		: uint
	{
		/// <summary>
		/// Surface is a texture.
		/// </summary>
		Texture = 0x1000,
		/// <summary>
		/// Surface is a mip map level.
		/// </summary>
		MipMap = 0x400008,
		/// <summary>
		/// Surface is a cube map face.
		/// </summary>
		CubeMap = 0x8
	}

	/// <summary>
	/// DDS cube map directions.
	/// </summary>
	[Flags]
	enum DDSCAPS2
		: uint
	{
		/// <summary>
		/// Positive X face.
		/// </summary>
		PositiveX = 0x600,
		/// <summary>
		/// Negative X face.
		/// </summary>
		NegativeX = 0xa00,
		/// <summary>
		/// Positive Y face.
		/// </summary>
		PositiveY = 0x1200,
		/// <summary>
		/// Negative Y face.
		/// </summary>
		NegativeY = 0x2200,
		/// <summary>
		/// Positive Z face.
		/// </summary>
		PositiveZ = 0x4200,
		/// <summary>
		/// Negative Z face.
		/// </summary>
		NegativeZ = 0x8200,
		/// <summary>
		/// All cube map faces.
		/// </summary>
		AllFaces = PositiveX | NegativeX | PositiveY | NegativeY | PositiveZ | NegativeZ,
		/// <summary>
		/// Cube map.
		/// </summary>
		CubeMap = 0x200,
		/// <summary>
		/// Volume data.
		/// </summary>
		Volume = 0x200000
	}
	// ReSharper restore InconsistentNaming
    #endregion

    /// <summary>
	/// A codec to handle reading/writing DDS files.
	/// </summary>
    /// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
    /// image formats, or use one of the predefined image codecs available in Gorgon.
    /// <para>The codec accepts and returns a <see cref="Gorgon.Graphics.GorgonImageData">GorgonImageData</see> type, which is filled from or read into the encoded file.</para>
    /// <para>This DDS codec does not support the following legacy Direct3D 9 formats:
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
    public unsafe sealed class GorgonCodecDDS
		: GorgonImageCodec
	{
		#region Constants.
		private const uint MagicNumber = 0x20534444;		// The DDS file magic number: "DDS "        
		#endregion

		#region Value Types.
		/// <summary>
		/// DDS legacy conversion type.
		/// </summary>
		struct DDSLegacyConversion
		{
			/// <summary>
			/// Buffer format.
			/// </summary>
			public readonly BufferFormat Format;
			/// <summary>
			/// Conversion flags.
			/// </summary>
			public readonly DDSConversionFlags Flags;
			/// <summary>
			/// Pixel format.
			/// </summary>
			public readonly DDSPixelFormat PixelFormat;

			/// <summary>
			/// Initializes a new instance of the <see cref="DDSLegacyConversion" /> struct.
			/// </summary>
			/// <param name="format">The format.</param>
			/// <param name="flags">The flags.</param>
			/// <param name="pixelFormat">The pixel format.</param>
			public DDSLegacyConversion(BufferFormat format, DDSConversionFlags flags, DDSPixelFormat pixelFormat)
			{
				Format = format;
				Flags = flags;
				PixelFormat = pixelFormat;
			}
		}

		/// <summary>
		/// DDS file header.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DDSHeader
		{
			/// <summary>
			/// Size of the header structure.
			/// </summary>
			public uint Size;
			/// <summary>
			/// Header flags.
			/// </summary>
			public DDSHeaderFlags Flags;
			/// <summary>
			/// Height.
			/// </summary>
			public uint Height;
			/// <summary>
			/// Width.
			/// </summary>
			public uint Width;
			/// <summary>
			/// The size in pitch or linear values.
			/// </summary>
			public uint PitchOrLinearSize;
			/// <summary>
			/// Depth.  Only if Volume appears in header flags.
			/// </summary>
			public uint Depth;
			/// <summary>
			/// Mip map count.
			/// </summary>
			public uint MipCount;
			/// <summary>
			/// Reserved.
			/// </summary>
			private readonly uint Reserved1;
			private readonly uint Reserved2;
			private readonly uint Reserved3;
			private readonly uint Reserved4;
			private readonly uint Reserved5;
			private readonly uint Reserved6;
			private readonly uint Reserved7;
			private readonly uint Reserved8;
			private readonly uint Reserved9;
			private readonly uint ReservedA;
			private readonly uint ReservedB;
			/// <summary>
			/// Pixel format.
			/// </summary>
			public DDSPixelFormat PixelFormat;
			/// <summary>
			/// Capabilities #1.
			/// </summary>
			public DDSCAPS1 Caps1;
			/// <summary>
			/// Capabilities #2.
			/// </summary>
			public DDSCAPS2 Caps2;
			/// <summary>
			/// Capabilities #3.
			/// </summary>
			private readonly uint ReservedC;
			/// <summary>
			/// Capabilities #4.
			/// </summary>
			private readonly uint ReservedD;
			/// <summary>
			/// Reserved.
			/// </summary>
			private readonly uint ReservedE;
		}

		/// <summary>
		/// DDS DirectX 10 header.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DX10Header
		{
			/// <summary>
			/// Format.
			/// </summary>
			public BufferFormat Format;
			/// <summary>
			/// Resource dimension.
			/// </summary>
			public ImageType ResourceDimension;
			/// <summary>
			/// Miscellaneous flags.
			/// </summary>
			public DDSHeaderMiscFlags MiscFlags;
			/// <summary>
			/// Array count.
			/// </summary>
			public uint ArrayCount;
			/// <summary>
			/// Reserved.
			/// </summary>
			private readonly uint Reserved;
		}

		/// <summary>
		/// Pixel format descriptor.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DDSPixelFormat
		{
			/// <summary>
			/// Size of the format, in bytes.
			/// </summary>
			public readonly uint SizeInBytes;
			/// <summary>
			/// Flags for the format.
			/// </summary>
			public readonly DDSPixelFormatFlags Flags;
			/// <summary>
			/// FOURCC value.
			/// </summary>
			public readonly uint FourCC;
			/// <summary>
			/// Number of bits per pixel.
			/// </summary>
			public readonly uint BitCount;
			/// <summary>
			/// Bit mask for the R component.
			/// </summary>
			public readonly uint RBitMask;
			/// <summary>
			/// Bit mask for the G component.
			/// </summary>
			public readonly uint GBitMask;
			/// <summary>
			/// Bit mask for the B component.
			/// </summary>
			public readonly uint BBitMask;
			/// <summary>
			/// Bit mask for the A component.
			/// </summary>
			public readonly uint ABitMask;
			
			/// <summary>
			/// Initializes a new instance of the <see cref="DDSPixelFormat" /> struct.
			/// </summary>
			/// <param name="flags">The flags.</param>
			/// <param name="fourCC">The four CC.</param>
			/// <param name="bitCount">The bit count.</param>
			/// <param name="rMask">The r mask.</param>
			/// <param name="gMask">The g mask.</param>
			/// <param name="bMask">The b mask.</param>
			/// <param name="aMask">A mask.</param>
			public DDSPixelFormat(DDSPixelFormatFlags flags, uint fourCC, uint bitCount, uint rMask, uint gMask, uint bMask, uint aMask)
			{
				SizeInBytes = (uint)DirectAccess.SizeOf<DDSPixelFormat>();
				Flags = flags;
				FourCC = fourCC;
				BitCount = bitCount;
				RBitMask = rMask;
				GBitMask = gMask;
				BBitMask = bMask;
				ABitMask = aMask;
			}
		}
		#endregion

		#region Variables.
		private static readonly DDSPixelFormat _pfDxt1 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '1'), 0, 0, 0, 0, 0);		// DXT1		
	    private static readonly DDSPixelFormat _pfDxt2 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '2'), 0, 0, 0, 0, 0);     // DXT2
	    private static readonly DDSPixelFormat _pfDxt3 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '3'), 0, 0, 0, 0, 0);     // DXT3
	    private static readonly DDSPixelFormat _pfDxt4 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '4'), 0, 0, 0, 0, 0);		// DXT4
	    private static readonly DDSPixelFormat _pfDxt5 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('D', 'X', 'T', '5'), 0, 0, 0, 0, 0);		// DXT5
	    private static readonly DDSPixelFormat _pfBC4U = new DDSPixelFormat(DDSPixelFormatFlags.FourCC,	MakeFourCC('B', 'C', '4', 'U'), 0, 0, 0, 0, 0);		// BC4 Unsigned
	    private static readonly DDSPixelFormat _pfBC4S = new DDSPixelFormat(DDSPixelFormatFlags.FourCC,	MakeFourCC('B', 'C', '4', 'S'), 0, 0, 0, 0, 0);		// BC4 Signed
	    private static readonly DDSPixelFormat _pfBC5U = new DDSPixelFormat(DDSPixelFormatFlags.FourCC,	MakeFourCC('B', 'C', '5', 'U'), 0, 0, 0, 0, 0);		// BC5 Unsigned
	    private static readonly DDSPixelFormat _pfBC5S = new DDSPixelFormat(DDSPixelFormatFlags.FourCC,	MakeFourCC('B', 'C', '5', 'S'), 0, 0, 0, 0, 0);		// BC5 Signed
	    private static readonly DDSPixelFormat _pfR8G8_B8G8 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('R', 'G', 'B', 'G'), 0, 0, 0, 0, 0); // R8G8_B8G8
	    private static readonly DDSPixelFormat _pfG8R8_G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('G', 'R', 'G', 'B'), 0, 0, 0, 0, 0); // G8R8_G8B8
	    private static readonly DDSPixelFormat _pfA8R8G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000); // A8R8G8B8
	    private static readonly DDSPixelFormat _pfX8R8G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000); // X8R8G8B8
	    private static readonly DDSPixelFormat _pfA8B8G8R8 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000); // A8B8G8R8
	    private static readonly DDSPixelFormat _pfX8B8G8R8 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000); // X8B8G8R8
	    private static readonly DDSPixelFormat _pfG16R16 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000); // G16R16
	    private static readonly DDSPixelFormat _pfR5G6B5 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 16, 0x0000f800, 0x000007e0, 0x0000001f, 0x00000000); // R5G6B5
	    private static readonly DDSPixelFormat _pfA1R5G5B5 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 16, 0x00007c00, 0x000003e0, 0x0000001f, 0x00008000); // A1R5G5B5A1
	    private static readonly DDSPixelFormat _pfA4R4G4B4 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 16, 0x00000f00, 0x000000f0, 0x0000000f, 0x0000f000); // A4R4G4B4		
	    private static readonly DDSPixelFormat _pfR8G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000); // R8G8B8
	    private static readonly DDSPixelFormat _pfL8 = new DDSPixelFormat(DDSPixelFormatFlags.Luminance, 0, 8, 0xff, 0x00, 0x00, 0x00);						// L8
	    private static readonly DDSPixelFormat _pfL16 = new DDSPixelFormat(DDSPixelFormatFlags.Luminance, 0, 16, 0xffff, 0x0000, 0x0000, 0x0000);			// L16
	    private static readonly DDSPixelFormat _pfA8L8 = new DDSPixelFormat(DDSPixelFormatFlags.LuminanceAlpha, 0, 16, 0x00ff, 0x0000, 0x0000, 0xff00);		// A8L8
	    private static readonly DDSPixelFormat _pfA8 = new DDSPixelFormat(DDSPixelFormatFlags.Alpha, 0, 8, 0x00, 0x00, 0x00,0xff);							// A8
	    private static DDSPixelFormat _pfDX10 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('D', 'X', '1', '0'), 0, 0, 0, 0, 0);				// DX10 extension

        // Mappings for legacy formats.
	    private readonly DDSLegacyConversion[] _legacyMapping = 
		    {
			    new DDSLegacyConversion(BufferFormat.BC1_UIntNormal, DDSConversionFlags.None, _pfDxt1),
			    new DDSLegacyConversion(BufferFormat.BC2_UIntNormal, DDSConversionFlags.None, _pfDxt3),
			    new DDSLegacyConversion(BufferFormat.BC3_UIntNormal, DDSConversionFlags.None, _pfDxt5),
			    new DDSLegacyConversion(BufferFormat.BC2_UIntNormal, DDSConversionFlags.None, _pfDxt2),
			    new DDSLegacyConversion(BufferFormat.BC3_UIntNormal, DDSConversionFlags.None, _pfDxt4),
			    new DDSLegacyConversion(BufferFormat.BC4_UIntNormal, DDSConversionFlags.None, _pfBC4U),
			    new DDSLegacyConversion(BufferFormat.BC4_IntNormal, DDSConversionFlags.None, _pfBC4S),
			    new DDSLegacyConversion(BufferFormat.BC5_UIntNormal, DDSConversionFlags.None, _pfBC5U),
			    new DDSLegacyConversion(BufferFormat.BC4_IntNormal, DDSConversionFlags.None, _pfBC5S),
			    new DDSLegacyConversion(BufferFormat.BC4_UIntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('A', 'T', 'I', '1'), 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.BC5_UIntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('A', 'T', 'I', '2'), 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R8G8_B8G8_UIntNormal, DDSConversionFlags.None, _pfR8G8_B8G8),
			    new DDSLegacyConversion(BufferFormat.G8R8_G8B8_UIntNormal, DDSConversionFlags.None, _pfG8R8_G8B8),
			    new DDSLegacyConversion(BufferFormat.B8G8R8A8_UIntNormal, DDSConversionFlags.None, _pfA8R8G8B8),
			    new DDSLegacyConversion(BufferFormat.B8G8R8X8_UIntNormal, DDSConversionFlags.None, _pfX8R8G8B8),
			    new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.None, _pfA8B8G8R8),
			    new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.NoAlpha, _pfX8B8G8R8),
			    new DDSLegacyConversion(BufferFormat.R16G16_UIntNormal, DDSConversionFlags.None, _pfG16R16),
			    new DDSLegacyConversion(BufferFormat.R10G10B10A2_UIntNormal, DDSConversionFlags.Swizzle, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000)),
			    new DDSLegacyConversion(BufferFormat.R10G10B10A2_UIntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000)),
			    new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.NoAlpha | DDSConversionFlags.RGB888, _pfR8G8B8),
			    new DDSLegacyConversion(BufferFormat.B5G6R5_UIntNormal, DDSConversionFlags.RGB565, _pfR5G6B5),
			    new DDSLegacyConversion(BufferFormat.B5G5R5A1_UIntNormal, DDSConversionFlags.RGB5551, _pfA1R5G5B5),
			    new DDSLegacyConversion(BufferFormat.B5G5R5A1_UIntNormal, DDSConversionFlags.RGB5551 | DDSConversionFlags.NoAlpha, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x0000)),
			    new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.RGB8332, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 16, 0x00e0, 0x001c, 0x0003, 0xff00)),
			    new DDSLegacyConversion(BufferFormat.B5G6R5_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.RGB332, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 8, 0xe0, 0x1c, 0x03, 0x00)),
			    new DDSLegacyConversion(BufferFormat.R8_UIntNormal, DDSConversionFlags.None, _pfL8),
			    new DDSLegacyConversion(BufferFormat.R16_UIntNormal, DDSConversionFlags.None, _pfL16),
			    new DDSLegacyConversion(BufferFormat.R8G8_UIntNormal, DDSConversionFlags.None, _pfA8L8),
			    new DDSLegacyConversion(BufferFormat.A8_UIntNormal, DDSConversionFlags.None, _pfA8),
			    new DDSLegacyConversion(BufferFormat.R16G16B16A16_UIntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R16G16B16A16_IntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R16_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R16G16_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R16G16B16A16_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R32G32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R32G32B32A32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)),
			    new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.Palette | DDSConversionFlags.A8P8, new DDSPixelFormat(DDSPixelFormatFlags.PaletteIndexed, 0, 16, 0, 0, 0, 0)),
			    new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.Palette, new DDSPixelFormat(DDSPixelFormatFlags.PaletteIndexed, 0, 8, 0, 0, 0, 0)), 
				new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.RGB4444,_pfA4R4G4B4),
				new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.NoAlpha | DDSConversionFlags.RGB4444, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)),
			    new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.A4L4, new DDSPixelFormat(DDSPixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0))
		};

        private readonly BufferFormat[] _formats;                       // Buffer formats.
        private readonly IEnumerable<BufferFormat> _supportedFormats;   // List of formats supported by the DDS codec.
		private int _actualDepth;						                // Actual depth value.
		private int _actualArrayCount;					                // Actual array count.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the depth for the image.
		/// </summary>
		/// <remarks>Use this to control the depth of the image.  Note that no scaling will be applied if a depth is larger or smaller than the depth stored in the file.  Set the value to 0 use the 
		/// depth value in the file.
		/// <para>This will not convert a 1D/2D image into a 3D image.  The file must have been saved as a volume texture/image for this property to take effect.</para>
		/// <para>For most codecs, there will only be 1 depth level, so a setting of 0 would be the same as a setting of 1.  Depth image data is usually only found in DDS files.</para>
		/// <para>This property only applies to decoding image data.</para>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int Depth
		{
			get;
			set;
		}
		
		/// <summary>
        /// Property to set or return the flags used when converting legacy pixel formats to the buffer format.
        /// </summary>
        /// <remarks>
        /// Use this to control how Gorgon converts data from a legacy DDS file format.  This only applies to DDS files generated with files saved by Direct3D 9 interfaces.
        /// <para>The property applies to both encoding and decoding image data.</para>
        /// <para>The default value is None.</para></remarks>
        public DDSFlags LegacyConversionFlags
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the palette assigned to indexed images.
		/// </summary>
		/// <remarks>
		/// If this value is assigned, then this palette will be used for any indexed pixel formats.  
		/// <para>It is recommended to make an array of 256 entries for the palette.</para>
		/// <para>The property only applies when decoding image data.  Encoding operations ignore this property.</para>
		/// <para>The default value is NULL.</para>
		/// </remarks>
		public IList<GorgonColor> Palette
		{
			get;
			private set;
		}


		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public override string CodecDescription
		{
			get 
			{
				return Resources.GORGFX_IMAGE_DDS_CODEC_DESC;
			}
		}

		/// <summary>
		/// Property to return the abbreviated name of the codec (e.g. PNG).
		/// </summary>
		public override string Codec
		{
			get 
			{
				return "DDS";
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Property to return the data formats for the image.
        /// </summary>
        public override IEnumerable<BufferFormat> SupportedFormats
        {
            get
            {
                return _supportedFormats;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports image arrays.
        /// </summary>
        public override bool SupportsArray
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
        public override bool SupportsMipMaps
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
        public override bool SupportsDepth
        {
            get
            {
                return true;
            }
        }

		/// <summary>
		/// Property to return whether the image codec supports block compression.
		/// </summary>
	    public override bool SupportsBlockCompression
	    {
		    get
		    {
			    return true;
		    }
	    }

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
		private static DDSConversionFlags ExpansionFormat(DDSConversionFlags flags)
		{
			if ((flags & DDSConversionFlags.Palette) == DDSConversionFlags.Palette)
			{
				return ((flags & DDSConversionFlags.A8P8) == DDSConversionFlags.A8P8) ? DDSConversionFlags.A8P8 : DDSConversionFlags.Palette;
			}

			if ((flags & DDSConversionFlags.RGB888) == DDSConversionFlags.RGB888)
			{
				return DDSConversionFlags.RGB888;
			}

			if ((flags & DDSConversionFlags.RGB332) == DDSConversionFlags.RGB332)
			{
				return DDSConversionFlags.RGB332;
			}

			if ((flags & DDSConversionFlags.RGB8332) == DDSConversionFlags.RGB8332)
			{
				return DDSConversionFlags.RGB8332;
			}

			if ((flags & DDSConversionFlags.A4L4) == DDSConversionFlags.A4L4)
			{
				return DDSConversionFlags.A4L4;
			}

			return (flags & DDSConversionFlags.RGB4444) == DDSConversionFlags.RGB4444
				       ? DDSConversionFlags.RGB4444
				       : DDSConversionFlags.None;
		}

		/// <summary>
        /// Function to read the optional DX10 header.
        /// </summary>
        /// <param name="stream">The stream containing the header.</param>
        /// <param name="header">DDS header.</param>
        /// <param name="flags">Conversion flags.</param>
        /// <returns>A new image settings object.</returns>
        private IImageSettings ReadDX10Header(GorgonDataStream stream, ref DDSHeader header, out DDSConversionFlags flags)
        {
            IImageSettings settings;

			var dx10Header = stream.Read<DX10Header>();
            flags = DDSConversionFlags.DX10;

            // Default the array count if it's not valid.
            if (dx10Header.ArrayCount <= 0)
            {
				throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
            }           

            switch (dx10Header.ResourceDimension)
            {
                case ImageType.Image1D:
                    settings = new GorgonTexture1DSettings
	                    {
		                    ArrayCount = (int)dx10Header.ArrayCount
	                    };
		            break;
                case ImageType.Image2D:
                    settings = new GorgonTexture2DSettings
                               {
	                               Height = (int)header.Height,
								   ArrayCount = (int)dx10Header.ArrayCount
                               };

		            // Get cube texture settings.
		            if ((dx10Header.MiscFlags & DDSHeaderMiscFlags.TextureCube) == DDSHeaderMiscFlags.TextureCube)
		            {
			            settings.ArrayCount = (int)dx10Header.ArrayCount * 6;
			            ((GorgonTexture2DSettings)settings).IsTextureCube = true;
		            }
                    
                    break;
                case ImageType.Image3D:
                    if ((header.Flags & DDSHeaderFlags.Volume) != DDSHeaderFlags.Volume)
                    {
						throw new IOException(string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, dx10Header.ResourceDimension));
                    }

                    settings = new GorgonTexture3DSettings
	                    {
		                    Height = (int)header.Height,
		                    Depth = (int)header.Depth
	                    };
		            break;
                default:
					throw new IOException(string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, dx10Header.ResourceDimension));
            }

            settings.Width = (int)header.Width;           

            // Ensure the format is correct.
            for (int i = 0; i < _formats.Length; i++)
            {
	            if (_formats[i] != dx10Header.Format)
	            {
		            continue;
	            }

	            settings.Format = dx10Header.Format;

	            if (settings.Format != BufferFormat.Unknown)
	            {
		            return settings;
	            }
            }

            throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, dx10Header.Format));
        }

        /// <summary>
        /// Function to retrieve the correct BufferFormat from the header information.
        /// </summary>
        /// <param name="format">Format in the header.</param>
        /// <param name="flags">Flags to alter conversion behaviour.</param>
        /// <param name="conversionFlags">Flags to indicate the types of conversions.</param>
        /// <returns>The format of the buffer, or Unknown if the format is not supported.</returns>
        private BufferFormat GetFormat(ref DDSPixelFormat format, DDSFlags flags, out DDSConversionFlags conversionFlags)
        {
	        DDSLegacyConversion conversion = default(DDSLegacyConversion);

			for (int i = 0; i < _legacyMapping.Length; i++)
			{
				var ddsFormat = _legacyMapping[i];

				if ((format.Flags & ddsFormat.PixelFormat.Flags) == 0)
				{
					continue;
				}

				// Check to see if the FOURCC values match.
				if ((ddsFormat.PixelFormat.Flags & DDSPixelFormatFlags.FourCC) == DDSPixelFormatFlags.FourCC)
				{
				    if (ddsFormat.PixelFormat.FourCC != format.FourCC)
				    {
				        continue;
				    }

				    conversion = ddsFormat;
				    break;
				}

			    if ((ddsFormat.PixelFormat.Flags & DDSPixelFormatFlags.PaletteIndexed) == DDSPixelFormatFlags.PaletteIndexed)
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
            if (((flags & DDSFlags.NoLegacyExpansion) == DDSFlags.NoLegacyExpansion) && ((conversionFlags & DDSConversionFlags.Expand) == DDSConversionFlags.Expand))
            {
                return BufferFormat.Unknown;
            }

            // Don't fix up RGB101010A2.
            if ((result == BufferFormat.R10G10B10A2_UIntNormal) && ((flags & DDSFlags.NoR10B10G10A2Fix) == DDSFlags.NoR10B10G10A2Fix))
            {
                conversionFlags ^= DDSConversionFlags.Swizzle;
            }

            return result;
        }

		/// <summary>
		/// Function to read in the DDS header from a stream.
		/// </summary>
		/// <param name="stream">The stream containing the data to read.</param>
        /// <param name="size">Size of the image, in bytes.</param>
		/// <param name="conversionFlags">The conversion flags.</param>
		/// <returns>New image settings.</returns>
        private IImageSettings ReadHeader(GorgonDataStream stream, int size, out DDSConversionFlags conversionFlags)
        {			
            IImageSettings settings;

			// Start with no conversion.
			conversionFlags = DDSConversionFlags.None;

            // Read the magic # from the header.
            uint magicNumber = stream.Read<UInt32>();

            // If the magic number doesn't match, then this is not a DDS file.
            if (magicNumber != MagicNumber)
            {
                throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
            }            

            // Read the header from the file.
            var header = stream.Read<DDSHeader>();

            if ((header.Size != DirectAccess.SizeOf<DDSHeader>())
				|| (header.PixelFormat.SizeInBytes != DirectAccess.SizeOf<DDSPixelFormat>()))
            {
				throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
            }

            // Ensure that we have at least one mip level.
            if (header.MipCount == 0)
            {
                header.MipCount = 1;
            }

            // Get DX 10 header information.
            if (((header.PixelFormat.Flags & DDSPixelFormatFlags.FourCC) == DDSPixelFormatFlags.FourCC) && (header.PixelFormat.FourCC == _pfDX10.FourCC))
            {
                if (size < DirectAccess.SizeOf<DX10Header>() + DirectAccess.SizeOf<DDSHeader>() + sizeof(uint))
                {
                    throw new EndOfStreamException(Resources.GORGFX_STREAM_EOF);
                }

                settings = ReadDX10Header(stream, ref header, out conversionFlags);
            }
            else
            {
				// If we actually have a volume texture, or we want to make one.
                if ((header.Flags & DDSHeaderFlags.Volume) == DDSHeaderFlags.Volume)
                {
                    settings = new GorgonTexture3DSettings
	                    {
		                    Depth = (int)header.Depth.Max(1)
	                    };
                }
                else
                {
                    settings = new GorgonTexture2DSettings
	                    {
		                    ArrayCount = 1
	                    };
	                if ((header.Caps2 & DDSCAPS2.CubeMap) == DDSCAPS2.CubeMap)
                    {
                        // Only allow all faces.
                        if ((header.Caps2 & DDSCAPS2.AllFaces) != DDSCAPS2.AllFaces)
                        {
							throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
                        }

                        ((GorgonTexture2DSettings)settings).IsTextureCube = true;
                        settings.ArrayCount = 6;
                    }
                }

                settings.Width = (int)header.Width;
                settings.Height = (int)header.Height;
                settings.Format = GetFormat(ref header.PixelFormat, LegacyConversionFlags, out conversionFlags);

                if (settings.Format == BufferFormat.Unknown)
                {
                    throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, settings.Format));
                }
            }

			var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.Format);

			if (formatInfo.IsCompressed)
			{
				int modWidth = settings.Width % 4;
				int modHeight = settings.Height % 4;

				if ((modWidth != 0) || (modHeight != 0))
				{
					throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
				}
			}

            settings.MipCount = (int)header.MipCount;

            // Special flag for handling BGR DXGI 1.1 formats
            if ((LegacyConversionFlags & DDSFlags.ForceRGB) == DDSFlags.ForceRGB)
            {
                switch (settings.Format)
                {
                    case BufferFormat.B8G8R8A8_UIntNormal:
                        settings.Format = BufferFormat.R8G8B8A8_UIntNormal;
                        conversionFlags |= DDSConversionFlags.Swizzle;
                        break;
                    case BufferFormat.B8G8R8X8_UIntNormal:
                        settings.Format = BufferFormat.R8G8B8A8_UIntNormal;
                        conversionFlags  |= DDSConversionFlags.Swizzle | DDSConversionFlags.NoAlpha;
                        break;
                    case BufferFormat.B8G8R8A8:
                        settings.Format = BufferFormat.R8G8B8A8;
                        conversionFlags  |= DDSConversionFlags.Swizzle;
                        break;
                    case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
                        settings.Format = BufferFormat.R8G8B8A8_UIntNormal_sRGB;
                        conversionFlags  |= DDSConversionFlags.Swizzle;
                        break;

                    case BufferFormat.B8G8R8X8:
                        settings.Format = BufferFormat.R8G8B8A8;
                        conversionFlags  |= DDSConversionFlags.Swizzle | DDSConversionFlags.NoAlpha;
                        break;
                    case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
                        settings.Format = BufferFormat.R8G8B8A8_UIntNormal_sRGB;
                        conversionFlags  |= DDSConversionFlags.Swizzle | DDSConversionFlags.NoAlpha;
                        break;
                }
            }

			// Special flag for handling 16bpp formats
		    if ((LegacyConversionFlags & DDSFlags.No16BPP) != DDSFlags.No16BPP)
		    {
		        return settings;
		    }

		    switch (settings.Format)
		    {
		        case BufferFormat.B5G6R5_UIntNormal:
		        case BufferFormat.B5G5R5A1_UIntNormal:
		            settings.Format = BufferFormat.R8G8B8A8_UIntNormal;
		            conversionFlags |= DDSConversionFlags.Expand;
		            if (settings.Format == BufferFormat.B5G6R5_UIntNormal)
		            {
		                conversionFlags |= DDSConversionFlags.NoAlpha;
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
		private static void ExpandLegacyScanline(void* src, int srcPitch, DDSConversionFlags srcFormat, void* dest, int destPitch, BufferFormat destFormat, ImageBitFlags bitFlags, uint[] palette)
		{
			if (((srcFormat == DDSConversionFlags.RGB332) && (destFormat != BufferFormat.B5G6R5_UIntNormal))
				|| ((srcFormat != DDSConversionFlags.RGB332) && (destFormat != BufferFormat.R8G8B8A8_UIntNormal)))
			{
				throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, destFormat));
			}

			if (((srcFormat == DDSConversionFlags.Palette) || (srcFormat == DDSConversionFlags.A8P8)) && ((palette == null) || (palette.Length != 256)))
			{
				throw new IOException(string.Format(Resources.GORGFX_IMAGE_INDEXED_NO_PALETTE, 256));
			}

			switch(srcFormat)
			{
				case DDSConversionFlags.Palette:
					{
						var srcPtr = (byte *)src;
						var destPtr = (uint *)dest;

						// Copy indexed data.
						for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
						{
							*(destPtr++) = palette[*(srcPtr++)];
						}
					}
					break;
				case DDSConversionFlags.A4L4:
					{
						var srcPtr = (byte*)src;
						var destPtr = (uint*)dest;

						// Copy alpha luminance.
						for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
						{
							byte pixel = *(srcPtr++);

							uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)(((pixel & 0xF0)  << 24) | ((pixel & 0xF0 << 20)));
							var lum = (uint)((pixel & 0x0F << 4) | (pixel & 0x0F));

							*(destPtr++) = lum | (lum << 8) | (lum << 16) | alpha;
						}
					}
					break;
				case DDSConversionFlags.RGB332:
				{
					var srcPtr = (byte*)src;

					switch (destFormat)
					{
						case BufferFormat.R8G8B8A8_UIntNormal:
							{
								var destPtr = (uint*)dest;

								// Copy 8 bit RGB.
								for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
								{
									byte pixel = *(srcPtr++);

									var r = (uint)((pixel & 0xE0) | ((pixel & 0xE0) >> 3) | ((pixel & 0xC0) >> 6));
									var g = (uint)(((pixel & 0x1C) << 11) | ((pixel & 0x1C) << 8) | ((pixel & 0x18) << 5));
									var b = (uint)(((pixel & 0x03) << 22) | ((pixel & 0x03) << 20) | ((pixel & 0x03) << 18) | ((pixel & 0x03) << 16));

									*(destPtr++) = r | g | b | 0xFF000000;
								}
							}
							break;
						case BufferFormat.B5G6R5_UIntNormal:
							{
								var destPtr = (ushort*)dest;

								// Copy 8 bit RGB.
								for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 2)
								{
									byte pixel = *(srcPtr++);

									var r = (uint)(((pixel & 0xE0) << 8) | ((pixel & 0xC0) << 5));
									var g = (uint)(((pixel & 0x1C) << 6) | ((pixel & 0x1C) << 3));
									var b = (uint)(((pixel & 0x03) << 3) | ((pixel & 0x03) << 1) | ((pixel & 0x02) >> 1));

									*(destPtr++) = (ushort)(r | g | b);
								}
							}
							break;
					}
				}
					break;
				case DDSConversionFlags.A8P8:
					{
						var srcPtr = (ushort*)src;
						var destPtr = (uint*)dest;

						// Copy indexed data with alpha.
						for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
						{
							ushort pixel = *(srcPtr++);
							uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)((pixel & 0xFF00) << 16);

							*(destPtr++) = pixel | alpha;
						}
					}
					break;
				case DDSConversionFlags.RGB8332:
					{
						var srcPtr = (ushort*)src;
						var destPtr = (uint*)dest;

						// Copy 8 bit RGB with alpha.
						for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
						{
							var pixel = (ushort)(*(srcPtr++) & 0xFF);
							uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)((pixel & 0xFF00) << 16);

							var r = (uint)((pixel & 0xE0) | ((pixel & 0xE0) >> 3) | ((pixel & 0xC0) >> 6));
							var g = (uint)(((pixel & 0x1C) << 11) | ((pixel & 0x1C) << 8) | ((pixel & 0x18) << 5));
							var b = (uint)(((pixel & 0x03) << 22) | ((pixel & 0x03) << 20) | ((pixel & 0x03) << 18) | ((pixel & 0x03) << 16));

							*(destPtr++) = r | g | b | alpha;
						}
					}
					break;
				case DDSConversionFlags.RGB4444:
					{
						var srcPtr = (ushort*)src;
						var destPtr = (uint*)dest;

						// Copy 12 bit RGB with 4 bit alpha.
						for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
						{
							ushort pixel = *(srcPtr++);
							uint alpha = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : (uint)(((pixel & 0xF000) << 16) | ((pixel & 0xF000) << 12));

							var r = (uint)(((pixel & 0x0F00) >> 4) | ((pixel & 0x0F00) >> 8));
							var g = (uint)(((pixel & 0x00F0) << 4) | ((pixel & 0x00F0) << 8));
							var b = (uint)(((pixel & 0x000F) << 16) | ((pixel & 0x000F) << 20));

							*(destPtr++) = r | g | b | alpha;
						}
					}
					break;
				case DDSConversionFlags.RGB888:
					{
						var srcPtr = (byte*)src;
						var destPtr = (uint*)dest;

						// Copy 24 bit RGB.
						for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); ++srcCount, destCount += 4)
						{
							// 24 bit DDS files are encoded as BGR, need to swizzle.
							var b = (uint)(*(srcPtr++) << 16);
							var g = (uint)(*(srcPtr++) << 8);
							var r = *(srcPtr++);	

							*(destPtr++) = r | g | b | 0xFF000000;
						}
					}
					break;
			}			
		}

		/// <summary>
		/// Function to write out the DDS header to the stream.
		/// </summary>
		/// <param name="settings">Meta data for the image header.</param>
		/// <param name="writer">Writer interface for the stream.</param>
		private void WriteHeader(IImageSettings settings, GorgonBinaryWriter writer)
		{
			var header = new DDSHeader();
			DDSPixelFormat? format = null;
			DDSFlags flags = LegacyConversionFlags;
			var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.Format);

			if ((settings.ArrayCount > 1) && ((settings.ArrayCount != 6) || (settings.ImageType != ImageType.Image2D) || (settings.ImageType != ImageType.ImageCube)))
			{
				flags |= DDSFlags.ForceDX10;
			}

			// If we're not forcing the DX10 header, then do a legacy conversion.
			if ((flags & DDSFlags.ForceDX10) != DDSFlags.ForceDX10)
			{
				switch (settings.Format)
				{
					case BufferFormat.R8G8B8A8_UIntNormal:
						format = _pfA8B8G8R8;
						break;
					case BufferFormat.R16G16_UIntNormal:
						format = _pfG16R16;
						break;
					case BufferFormat.R8G8_UIntNormal:
						format = _pfA8L8;
						break;
					case BufferFormat.R16_UIntNormal:
						format = _pfL16;
						break;
					case BufferFormat.R8_UIntNormal:
						format = _pfL8;
						break;
					case BufferFormat.A8_UIntNormal:
						format = _pfA8;
						break;
					case BufferFormat.R8G8_B8G8_UIntNormal:
						format = _pfR8G8_B8G8;
						break;
					case BufferFormat.G8R8_G8B8_UIntNormal:
						format = _pfG8R8_G8B8;
						break;
					case BufferFormat.BC1_UIntNormal:
						format = _pfDxt1;
						break;
					case BufferFormat.BC2_UIntNormal:
						format = _pfDxt3;
						break;
					case BufferFormat.BC3_UIntNormal:
						format = _pfDxt5;
						break;
					case BufferFormat.BC4_UIntNormal:
						format = _pfBC4U;
						break;
					case BufferFormat.BC4_IntNormal:
						format = _pfBC4S;
						break;
					case BufferFormat.BC5_UIntNormal:
						format = _pfBC5U;
						break;
					case BufferFormat.BC5_IntNormal:
						format = _pfBC5S;
						break;
					case BufferFormat.B5G6R5_UIntNormal:
						format = _pfR5G6B5;
						break;
					case BufferFormat.B5G5R5A1_UIntNormal:
						format = _pfA1R5G5B5;
						break;
					case BufferFormat.B8G8R8A8_UIntNormal:
						format = _pfA8R8G8B8;
						break;
					case BufferFormat.B8G8R8X8_UIntNormal:
						format = _pfX8R8G8B8;
						break;
					case BufferFormat.R32G32B32A32_Float:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0);
						break;
					case BufferFormat.R16G16B16A16_Float:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0);
						break;
					case BufferFormat.R16G16B16A16_UIntNormal:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0);
						break;
					case BufferFormat.R16G16B16A16_IntNormal:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0);
						break;
					case BufferFormat.R32G32_Float:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0);
						break;
					case BufferFormat.R16G16_Float:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0);
						break;
					case BufferFormat.R32_Float:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0);
						break;
					case BufferFormat.R16_Float:
						format = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0);
						break;
				}
			}

			// Write the DDS magic # ID.
			writer.Write(MagicNumber);

			// Clear all values.
			DirectAccess.ZeroMemory(&header, DirectAccess.SizeOf<DDSHeader>());

			// Set up the header.
			header.Size = (uint)DirectAccess.SizeOf<DDSHeader>();
			header.Flags = DDSHeaderFlags.Texture;
			header.Caps1 = DDSCAPS1.Texture;

			// Get mip map info.
			if (settings.MipCount > 0)
			{
				header.Flags |= DDSHeaderFlags.MipMap;
				header.MipCount = (uint)settings.MipCount;

				if (settings.MipCount > 1)
				{
					header.Caps1 |= DDSCAPS1.MipMap;
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
                        header.Caps1 |= DDSCAPS1.CubeMap;
						header.Caps2 |= DDSCAPS2.AllFaces;
					}
					break;
				case ImageType.Image3D:
					header.Width = (uint)settings.Width;
					header.Height = (uint)settings.Height;
					header.Depth = (uint)settings.Depth;
					header.Flags |= DDSHeaderFlags.Volume;
					header.Caps2 |= DDSCAPS2.Volume;
					break;
				default:
					throw new IOException(string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, settings.ImageType));
			}

			// Get pitch information.
			var pitchInfo = formatInfo.GetPitch(settings.Width, settings.Height, PitchFlags.None);

			if (formatInfo.IsCompressed)
			{
				header.Flags |= DDSHeaderFlags.LinearSize;
				header.PitchOrLinearSize = (uint)pitchInfo.SlicePitch;
			}
			else
			{
				header.Flags |= DDSHeaderFlags.RowPitch;
				header.PitchOrLinearSize = (uint)pitchInfo.RowPitch;
			}
						
			// Get pixel format.
			header.PixelFormat = format ?? _pfDX10;

			// Write out the header.
			writer.WriteValue(header);

			// If we didn't map a legacy format, then use the DX 10 header.
			if (format != null)
			{
				return;
			}

			DX10Header dx10Header = default(DX10Header);

			dx10Header.Format = settings.Format;
			if (settings.ImageType != ImageType.ImageCube)
			{
				dx10Header.ResourceDimension = settings.ImageType;
				dx10Header.ArrayCount = (uint)settings.ArrayCount;
			}
			else
			{
				dx10Header.ResourceDimension = ImageType.Image2D;
				dx10Header.MiscFlags |= DDSHeaderMiscFlags.TextureCube;
				dx10Header.ArrayCount = (uint)(settings.ArrayCount / 6);
			}

			writer.WriteValue(dx10Header);
		}

		/// <summary>
		/// Function to perform the copying of image data into the buffer.
		/// </summary>
		/// <param name="stream">Stream that contains the image data.</param>
		/// <param name="image">Image data.</param>
		/// <param name="pitchFlags">Flags used to determine pitch when expanding pixels.</param>
		/// <param name="conversionFlags">Flags used for conversion between legacy formats and the current format.</param>
		/// <param name="palette">Palette used in indexed conversion.</param>
		private void CopyImageData(GorgonDataStream stream, GorgonImageData image, PitchFlags pitchFlags, DDSConversionFlags conversionFlags, uint[] palette)
		{
			var formatInfo = GorgonBufferFormatInfo.GetInfo(image.Settings.Format);

			// Get copy flag bits per pixel if we have an expansion.
			if ((conversionFlags & DDSConversionFlags.Expand) == DDSConversionFlags.Expand)
			{
				if ((conversionFlags & DDSConversionFlags.RGB888) == DDSConversionFlags.RGB888)
				{
					pitchFlags |= PitchFlags.BPP24;
				}
				else if (((conversionFlags & DDSConversionFlags.RGB565) == DDSConversionFlags.RGB565)
							|| ((conversionFlags & DDSConversionFlags.RGB5551) == DDSConversionFlags.RGB5551)
							|| ((conversionFlags & DDSConversionFlags.RGB4444) == DDSConversionFlags.RGB4444)
							|| ((conversionFlags & DDSConversionFlags.RGB332) == DDSConversionFlags.RGB8332)
							|| ((conversionFlags & DDSConversionFlags.RGB332) == DDSConversionFlags.A8P8))
				{
					pitchFlags |= PitchFlags.BPP16;
				}
				else if (((conversionFlags & DDSConversionFlags.A4L4) == DDSConversionFlags.A4L4)
							|| ((conversionFlags & DDSConversionFlags.RGB332) == DDSConversionFlags.RGB332)
							|| ((conversionFlags & DDSConversionFlags.Palette) == DDSConversionFlags.Palette))
				{
					pitchFlags |= PitchFlags.BPP8;
				}
			}

			// Get the size of the source image in bytes, and its pitch information.
			int sizeInBytes = GorgonImageData.GetSizeInBytes(image.Settings, pitchFlags);

			if (sizeInBytes > image.SizeInBytes)
			{
				throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
			}

			// If no conversion is to take place, then just do a straight dump into memory.
			if (((conversionFlags == DDSConversionFlags.None)
						|| (conversionFlags == DDSConversionFlags.DX10))
					&& (pitchFlags == PitchFlags.None) && (image.Settings.ArrayCount == _actualArrayCount) && (image.Settings.Depth == _actualDepth))
			{
                // First mip, array and depth slice is at the start of our image memory buffer.
                DirectAccess.MemoryCopy(image.Buffers[0].Data.BasePointer, stream.PositionPointer, sizeInBytes);
                return;
			}

			var expFlags = ImageBitFlags.None;

			if ((conversionFlags & DDSConversionFlags.NoAlpha) == DDSConversionFlags.NoAlpha)
			{
				expFlags |= ImageBitFlags.OpaqueAlpha;
			}

			if ((conversionFlags & DDSConversionFlags.Swizzle) == DDSConversionFlags.Swizzle)
			{
				expFlags |= ImageBitFlags.Legacy;
			}

			// Clip the depth.
			int depth = _actualDepth.Min(image.Settings.Depth);
			int arrayCount = _actualArrayCount.Min(image.Settings.ArrayCount);
			var srcPointer = (byte*)stream.PositionPointer;

			for (int array = 0; array < arrayCount; array++)
			{
				for (int mipLevel = 0; mipLevel < image.Settings.MipCount; mipLevel++)
				{
					// Get our destination buffer.
					var destBuffer = image.Buffers[mipLevel, array];
					var pitchInfo = formatInfo.GetPitch(destBuffer.Width, destBuffer.Height, pitchFlags);		
					var destPointer = (byte*)destBuffer.Data.BasePointer;

					for (int slice = 0; slice < depth; slice++)
					{
						// We're using compressed data, just copy.
						if (formatInfo.IsCompressed)
						{
							DirectAccess.MemoryCopy(destPointer, srcPointer, pitchInfo.SlicePitch.Min(destBuffer.PitchInformation.SlicePitch));
							destPointer += pitchInfo.SlicePitch;
							srcPointer += destBuffer.PitchInformation.SlicePitch;
							continue;
						}

						// Read each scan line if we require some form of conversion. 
						for (int h = 0; h < destBuffer.Height; h++)
						{
							if ((conversionFlags & DDSConversionFlags.Expand) == DDSConversionFlags.Expand)
							{
								// Perform expansion.
								if (((conversionFlags & DDSConversionFlags.RGB565) == DDSConversionFlags.RGB565)
									|| ((conversionFlags & DDSConversionFlags.RGB5551) == DDSConversionFlags.RGB5551))
								{
									Expand16BPPScanline(srcPointer, pitchInfo.RowPitch,
													((conversionFlags & DDSConversionFlags.RGB5551) == DDSConversionFlags.RGB5551) ? BufferFormat.B5G5R5A1_UIntNormal : BufferFormat.B5G6R5_UIntNormal,
													destPointer, destBuffer.PitchInformation.RowPitch, expFlags);
								}
								else
								{
									// If we're 8 bit or some other type of format, then expand to match.
									DDSConversionFlags expandLegacyFormat = ExpansionFormat(conversionFlags);

									if (expandLegacyFormat == DDSConversionFlags.None)
									{
										throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, expandLegacyFormat));
									}

									ExpandLegacyScanline(srcPointer, pitchInfo.RowPitch, expandLegacyFormat, destPointer, destBuffer.PitchInformation.RowPitch, image.Settings.Format, expFlags, palette);
								}
							}
							else if ((conversionFlags & DDSConversionFlags.Swizzle) == DDSConversionFlags.Swizzle)
							{
								// Perform swizzle.
                                SwizzleScanline(srcPointer, pitchInfo.RowPitch, destPointer, destBuffer.PitchInformation.RowPitch, image.Settings.Format, expFlags);
							}
							else
							{
								// Copy and set constant alpha (if necessary).
								CopyScanline(srcPointer, pitchInfo.RowPitch, destPointer, destBuffer.PitchInformation.RowPitch, image.Settings.Format, expFlags);
							}
								
							// Increment our pointer data by one line.
							srcPointer += pitchInfo.RowPitch;
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

        /// <summary>
        /// Function to load an image from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the data to load.</param>
        /// <param name="size">Size of the data to read, in bytes.</param>
        /// <returns>
        /// The image data that was in the stream.
        /// </returns>
		protected internal override GorgonImageData LoadFromStream(GorgonDataStream stream, int size)
		{
	        DDSConversionFlags flags;
	        uint[] palette = null;
                        
            if (size < DirectAccess.SizeOf<DDSHeader>() + sizeof(uint))
            {
                throw new EndOfStreamException(Resources.GORGFX_STREAM_EOF);
            }

			// Read the header information.
			IImageSettings settings = ReadHeader(stream, size, out flags);
	        _actualDepth = settings.Depth;
	        _actualArrayCount = settings.ArrayCount;
            
			// Override array/depth settings.
	        if (settings.ImageType == ImageType.Image3D)
	        {
		        if (Depth > 0)
		        {
			        settings.Depth = Depth;
		        }
	        }
	        else
	        {
		        if (ArrayCount > 0)
		        {
			        settings.ArrayCount = ArrayCount;
		        }
	        }

            var imageData = new GorgonImageData(settings);

            try
            {
			    // We have a palette, either create a new one or clone the assigned one.
			    if ((flags & DDSConversionFlags.Palette) == DDSConversionFlags.Palette)
			    {
                    const int paletteSize = sizeof(uint) * 256;

                    if (paletteSize > stream.Length - stream.Position)
                    {
                        throw new EndOfStreamException(Resources.GORGFX_STREAM_EOF);
                    }

				    palette = new uint[256];
				    if (Palette.Count > 0)
				    {
					    int count = Palette.Count.Min(256);

					    for (int i = 0; i < count; i++)
					    {
						    palette[i] = (uint)Palette[i].ToARGB();
					    }

                        // Skip past palette data since we're not using it.
                        stream.Position += paletteSize;
				    }
				    else
				    {
					    // Read from the stream if we haven't assigned a palette.
					    stream.ReadRange(palette, 0, 256);
				    }
			    }

				// Copy the data from the stream to the buffer.
	            CopyImageData(stream, imageData,
	                          ((LegacyConversionFlags & DDSFlags.LegacyDWORD) == DDSFlags.LegacyDWORD)
		                          ? PitchFlags.LegacyDWORD
		                          : PitchFlags.None, flags, palette);
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
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="Gorgon.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		protected internal override void SaveToStream(GorgonImageData imageData, Stream stream)
		{
			// Use a binary writer.
			using (var writer = new GorgonBinaryWriter(stream, true))
			{
				// Write the header for the file.
				WriteHeader(imageData.Settings, writer);

				// Write image data.
				switch (imageData.Settings.ImageType)
				{
					case ImageType.Image1D:
					case ImageType.Image2D:
					case ImageType.ImageCube:
						for (int array = 0; array < imageData.Settings.ArrayCount; array++)
						{
							for (int mipLevel = 0; mipLevel < imageData.Settings.MipCount; mipLevel++)
							{
								var buffer = imageData.Buffers[mipLevel, array];
								
								writer.Write(buffer.Data.BasePointer, buffer.PitchInformation.SlicePitch);
							}
						}
						break;
					case ImageType.Image3D:
						int depth = imageData.Settings.Depth;
						for (int mipLevel = 0; mipLevel < imageData.Settings.MipCount; mipLevel++)
						{							
							for (int slice = 0; slice < depth; slice++)
							{
								var buffer = imageData.Buffers[mipLevel, slice];
								writer.Write(buffer.Data.BasePointer, buffer.PitchInformation.SlicePitch);
							}

							if (depth > 1)
							{
								depth >>= 1;
							}
						}
						break;
				}
			}
		}

		/// <summary>
		/// Function to read file meta data.
		/// </summary>
		/// <param name="stream">Stream used to read the meta data.</param>
		/// <returns>
		/// The image meta data as a <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> value.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
		/// <para>-or-</para>
		/// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override IImageSettings GetMetaData(Stream stream)
		{
            long position = 0;
			int size = DirectAccess.SizeOf<DDSHeader>() + DirectAccess.SizeOf<DX10Header>() + sizeof(uint); // Allocate enough space to hold the header and the DX 10 header and the magic number.

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_STREAM_WRITE_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORGFX_STREAM_NO_SEEK);
            }

            if (stream.Length - stream.Position < size)
            {
                throw new EndOfStreamException(Resources.GORGFX_STREAM_EOF);
            }

            try
            {
                position = stream.Position;

	            DDSConversionFlags flags;
	            var gorgonDataStream = stream as GorgonDataStream;

	            if (gorgonDataStream != null)
                {
                    return ReadHeader(gorgonDataStream, size, out flags);
                }

                using (var memoryStream = new GorgonDataStream(size))
                {
	                stream.CopyToStream(memoryStream, size);
	                memoryStream.Position = 0;
                    return ReadHeader(memoryStream, size, out flags);
                }
            }
            finally
            {
                stream.Position = position;
            }
		}

		/// <summary>
		/// Function to determine if this codec can read the file or not.
		/// </summary>
		/// <param name="stream">Stream used to read the file information.</param>
		/// <returns>
		/// <b>true</b> if the codec can read the file, <b>false</b> if not.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override bool IsReadable(Stream stream)
        {
            uint magicNumber;
            long position = 0;

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_STREAM_WRITE_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORGFX_STREAM_NO_SEEK);
            }

            if (stream.Length - stream.Position < sizeof(uint) + DirectAccess.SizeOf<DDSHeader>())
            {
                return false;
            }

            try
            {
                position = stream.Position;
                using (var reader = new GorgonBinaryReader(stream, true))
                {
                    magicNumber = reader.ReadUInt32();
                }
            }
            finally
            {
                stream.Position = position;
            }
            return magicNumber == MagicNumber;
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecDDS" /> class.
		/// </summary>
		public GorgonCodecDDS()
		{
			Depth = 0;
			CodecCommonExtensions = new[] { "dds" };
            _formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));

		    _supportedFormats = from format in _formats
		                        let info = GorgonBufferFormatInfo.GetInfo(format)
		                        where format != BufferFormat.Unknown && !info.IsTypeless
		                        select format;

            LegacyConversionFlags = DDSFlags.None;
			Palette = new GorgonColor[256];
		}
		#endregion
	}

	// ReSharper restore ForCanBeConvertedToForeach
}
