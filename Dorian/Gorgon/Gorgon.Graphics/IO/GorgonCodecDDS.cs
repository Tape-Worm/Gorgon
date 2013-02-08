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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GorgonLibrary.Native;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.IO
{
    #region Enums.
    /// <summary>
	/// DDS Flags.
	/// </summary>
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
		RGB44 = 0x100,
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
	[Flags()]
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
	[Flags()]
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
	[Flags()]
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
		/// Lunminance data.
		/// </summary>
		Luminance = 0x20000,
		/// <summary>
		/// Lunminance + alpha data.
		/// </summary>
		LuminanceAlpha = 0x20001,
		/// <summary>
		/// Alpha data.
		/// </summary>
		Alpha = 0x2,
		/// <summary>
		/// Paletted/indexed data.
		/// </summary>
		PaletterIndexed = 0x20
	}

	/// <summary>
	/// DDS surface flags.
	/// </summary>
	[Flags()]
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
	[Flags()]
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
    #endregion

    /// <summary>
	/// A codec to handle reading/writing DDS files.
	/// </summary>
    /// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
    /// image formats, or use one of the predefined image codecs available in Gorgon.
    /// <para>The codec accepts and returns a <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> type, which is filled from or read into the encoded file.</para>
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
			public uint Reserved;
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
			public uint SizeInBytes;
			/// <summary>
			/// Flags for the format.
			/// </summary>
			public DDSPixelFormatFlags Flags;
			/// <summary>
			/// FOURCC value.
			/// </summary>
			public uint FourCC;
			/// <summary>
			/// Number of bits per pixel.
			/// </summary>
			public uint BitCount;
			/// <summary>
			/// Bit mask for the R component.
			/// </summary>
			public uint RBitMask;
			/// <summary>
			/// Bit mask for the G component.
			/// </summary>
			public uint GBitMask;
			/// <summary>
			/// Bit mask for the B component.
			/// </summary>
			public uint BBitMask;
			/// <summary>
			/// Bit mask for the A component.
			/// </summary>
			public uint ABitMask;
			
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
		private static DDSPixelFormat _pfDxt1 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('D', 'X', 'T', '1'), 0, 0, 0, 0, 0);		// DXT1		
		private static DDSPixelFormat _pfDxt2 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('D', 'X', 'T', '2'), 0, 0, 0, 0, 0);		// DXT2
		private static DDSPixelFormat _pfDxt3 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('D', 'X', 'T', '3'), 0, 0, 0, 0, 0);		// DXT3
		private static DDSPixelFormat _pfDxt4 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('D', 'X', 'T', '4'), 0, 0, 0, 0, 0);		// DXT4
		private static DDSPixelFormat _pfDxt5 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('D', 'X', 'T', '5'), 0, 0, 0, 0, 0);		// DXT5
		private static DDSPixelFormat _pfBC4U = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('B', 'C', '4', 'U'), 0, 0, 0, 0, 0);		// BC4 Unsigned
		private static DDSPixelFormat _pfBC4S = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('B', 'C', '4', 'S'), 0, 0, 0, 0, 0);		// BC4 Signed
		private static DDSPixelFormat _pfBC5U = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('B', 'C', '5', 'U'), 0, 0, 0, 0, 0);		// BC5 Unsigned
		private static DDSPixelFormat _pfBC5S = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('B', 'C', '5', 'S'), 0, 0, 0, 0, 0);		// BC5 Signed
		private static DDSPixelFormat _pfR8G8_B8G8 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('R', 'G', 'B', 'G'), 0, 0, 0, 0, 0);		// R8G8_B8G8
		private static DDSPixelFormat _pfG8R8_G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('G', 'R', 'G', 'B'), 0, 0, 0, 0, 0);		// G8R8_G8B8
		private static DDSPixelFormat _pfA8R8G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);			// A8R8G8B8
		private static DDSPixelFormat _pfX8R8G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);			// X8R8G8B8
		private static DDSPixelFormat _pfA8B8G8R8 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000);			// A8B8G8R8
		private static DDSPixelFormat _pfX8B8G8R8 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000);			// X8B8G8R8
		private static DDSPixelFormat _pfG16R16 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000);				// G16R16
		private static DDSPixelFormat _pfR5G6B5 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 16, 0x0000f800, 0x000007e0, 0x0000001f, 0x00000000);				// R5G6B5
		private static DDSPixelFormat _pfA1R5G5B5 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 16, 0x00007c00, 0x000003e0, 0x0000001f, 0x00008000);			// A1R5G5B5A1
		private static DDSPixelFormat _pfA4R4G4B4 = new DDSPixelFormat(DDSPixelFormatFlags.RGBA, 0, 16, 0x00000f00, 0x000000f0, 0x0000000f, 0x0000f000);			// A4R4G4B4		
		private static DDSPixelFormat _pfR8G8B8 = new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);				// R8G8B8
		private static DDSPixelFormat _pfL8 = new DDSPixelFormat(DDSPixelFormatFlags.Luminance, 0, 8, 0xff, 0x00, 0x00, 0x00);									// L8
		private static DDSPixelFormat _pfL16 = new DDSPixelFormat(DDSPixelFormatFlags.Luminance, 0, 16, 0xffff, 0x0000, 0x0000, 0x0000);							// L16
		private static DDSPixelFormat _pfA8L8 = new DDSPixelFormat(DDSPixelFormatFlags.LuminanceAlpha, 0, 16, 0x00ff, 0x0000, 0x0000, 0xff00);						// A8L8
		private static DDSPixelFormat _pfA8 = new DDSPixelFormat(DDSPixelFormatFlags.Alpha, 0, 8, 0x00, 0x00, 0x00, 0xff);											// A8
		private static DDSPixelFormat _pfDX10 = new DDSPixelFormat(DDSPixelFormatFlags.FourCC, GorgonCodecDDS.MakeFourCC('D', 'X', '1', '0'), 0, 0, 0, 0, 0);			// DX10 extension

		private DDSLegacyConversion[] _legacyMapping = new []								// Mappings for legacy formats.
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
			new DDSLegacyConversion(BufferFormat.BC4_UIntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('A','T','I','1'), 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.BC5_UIntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, MakeFourCC('A','T','I','2'), 0, 0, 0, 0, 0)),
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
			new DDSLegacyConversion(BufferFormat.B5G6R5_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.RGB332, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0,  8, 0xe0, 0x1c, 0x03, 0x00)),
			new DDSLegacyConversion(BufferFormat.R8_UIntNormal, DDSConversionFlags.None, _pfL8),
			new DDSLegacyConversion(BufferFormat.R16_UIntNormal, DDSConversionFlags.None, _pfL16),
			new DDSLegacyConversion(BufferFormat.R8G8_UIntNormal, DDSConversionFlags.None, _pfA8L8),
			new DDSLegacyConversion(BufferFormat.A8_UIntNormal,DDSConversionFlags.None,_pfA8),
			new DDSLegacyConversion(BufferFormat.R16G16B16A16_UIntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R16G16B16A16_IntNormal, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R16_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R16G16_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R16G16B16A16_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R32G32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R32G32B32A32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R32_Float, DDSConversionFlags.None, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)),
			new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.Palette | DDSConversionFlags.A8P8, new DDSPixelFormat(DDSPixelFormatFlags.PaletterIndexed, 0, 16, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.Palette, new DDSPixelFormat(DDSPixelFormatFlags.PaletterIndexed, 0, 8, 0, 0, 0, 0)),
			new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.RGB4444, _pfA4R4G4B4),
			new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.NoAlpha | DDSConversionFlags.RGB4444, new DDSPixelFormat(DDSPixelFormatFlags.RGB, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)),
			new DDSLegacyConversion(BufferFormat.R8G8B8A8_UIntNormal, DDSConversionFlags.Expand | DDSConversionFlags.RGB44, new DDSPixelFormat(DDSPixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0))
		};

        private BufferFormat[] _formats = null;         // Buffer formats.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the flags used when converting legacy pixel formats to the buffer format.
        /// </summary>
        /// <remarks>
        /// Use this to control how Gorgon converts data from a legacy DDS file format.  This only applies to DDS files generated with files saved by Direct3D 9 interfaces.
        /// <para>The property only applies when decoding image data.  Encoding operations ignore this property.</para>
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
			set;
		}


		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public override string CodecDescription
		{
			get 
			{
				return "Direct Draw Surface";
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
				return ((uint)((byte)c1)) | (((uint)((byte)c2)) << 8) | (((uint)((byte)c3)) << 16) | (((uint)((byte)c4)) << 24);
			}
		}

        /// <summary>
        /// Function to read the optional DX10 header.
        /// </summary>
        /// <param name="reader">Reader interface for the stream.</param>
        /// <param name="header">DDS header.</param>
        /// <param name="flags">Conversion flags.</param>
        /// <returns>A new image settings object.</returns>
        private IImageSettings ReadDX10Header(GorgonBinaryReader reader, ref DDSHeader header, out DDSConversionFlags flags)
        {
            IImageSettings settings = null;
            DX10Header dx10Header = default(DX10Header);

            dx10Header = reader.ReadValue<DX10Header>();
            flags = DDSConversionFlags.DX10;

            // Default the array count if it's not valid.
            if (dx10Header.ArrayCount <= 0)
            {
                throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  The array count is 0 or less than 0.");
            }           

            switch (dx10Header.ResourceDimension)
            {
                case ImageType.Image1D:
                    settings = new GorgonTexture1DSettings();
                    settings.ArrayCount = (int)dx10Header.ArrayCount;
                    settings.Height = 1;
                    settings.Depth = 1;
                    break;
                case ImageType.Image2D:
                    settings = new GorgonTexture2DSettings();

                    // Get cube texture settings.
                    if ((dx10Header.MiscFlags & DDSHeaderMiscFlags.TextureCube) == DDSHeaderMiscFlags.TextureCube)
                    {
                        settings.ArrayCount = (int)dx10Header.ArrayCount * 6;
                        ((GorgonTexture2DSettings)settings).IsTextureCube = true;
                    }
                    settings.Height = (int)header.Height;
                    settings.Depth = 1;
					settings.ArrayCount = (int)dx10Header.ArrayCount;
                    break;
                case ImageType.Image3D:
                    if ((header.Flags & DDSHeaderFlags.Volume) != DDSHeaderFlags.Volume)
                    {
                        throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  Image claims to be a 3D image, but is missing volume flag.");
                    }

                    settings = new GorgonTexture3DSettings();
                    settings.ArrayCount = 1;
                    settings.Height = (int)header.Height;
                    settings.Depth = (int)header.Depth;
                    break;
                default:
                    throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  The image data is not 1D, 2D or 3D.");
            }

            settings.Width = (int)header.Width;           

            // Ensure the format is correct.
            for (int i = 0; i < _formats.Length; i++)
            {
                if (_formats[i] == dx10Header.Format)
                {
                    settings.Format = dx10Header.Format;

                    if (settings.Format != BufferFormat.Unknown)
                    {
                        return settings;
                    }
                }
            }

            throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  The buffer format is unrecognized.");
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
            BufferFormat result = BufferFormat.Unknown;
            DDSLegacyConversion conversion = default(DDSLegacyConversion);

            foreach (var ddsFormat in _legacyMapping)
            {
                if ((format.Flags & ddsFormat.PixelFormat.Flags) != 0)
                {
                    // Check to see if the FOURCC values match.
                    if ((ddsFormat.PixelFormat.Flags & DDSPixelFormatFlags.FourCC) == DDSPixelFormatFlags.FourCC)
                    {
                        if (ddsFormat.PixelFormat.FourCC == format.FourCC)
                        {
                            conversion = ddsFormat;
                            break;
                        }
                    }
                    else if ((ddsFormat.PixelFormat.Flags & DDSPixelFormatFlags.PaletterIndexed) == DDSPixelFormatFlags.PaletterIndexed)
                    {
                        // If indexed, then check the bit count.
                        if (ddsFormat.PixelFormat.BitCount == format.BitCount)
                        {
                            conversion = ddsFormat;
                            break;
                        }
                    }
                    else if (ddsFormat.PixelFormat.BitCount == format.BitCount)
                    {
                        // If the bit masks are the same, then use this one.
                        if ((ddsFormat.PixelFormat.RBitMask == format.RBitMask)
                            && (ddsFormat.PixelFormat.GBitMask == format.GBitMask)
                            && (ddsFormat.PixelFormat.BBitMask == format.BBitMask)
                            && (ddsFormat.PixelFormat.ABitMask == format.ABitMask))
                        {
                            conversion = ddsFormat;
                            break;
                        }
                    }
                }
            }

            conversionFlags = conversion.Flags;
            result = conversion.Format;

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
        /// <param name="reader">Reader interface for the stream.</param>
        private IImageSettings ReadHeader(GorgonBinaryReader reader, out DDSConversionFlags conversionFlags)
        {
            IImageSettings settings = null;
            DDSHeader header = new DDSHeader();            
            uint magicNumber = 0;

			// Start with no conversion.
			conversionFlags = DDSConversionFlags.None;

            // Read the magic # from the header.
            magicNumber = reader.ReadUInt32();

            // If the magic number doesn't match, then this is not a DDS file.
            if (magicNumber != MagicNumber)
            {
                throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.");
            }

            // Read the header from the file.
            header = reader.ReadValue<DDSHeader>();

            if (header.Size != DirectAccess.SizeOf<DDSHeader>())
            {
                throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  There was a size mismatch.  Expected: [" 
                        + DirectAccess.SizeOf<DDSHeader>().ToString() + "], got: [" + header.Size.ToString() + "]");
            }

            if (header.PixelFormat.SizeInBytes != DirectAccess.SizeOf<DDSPixelFormat>())
            {
                throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  There was a pixel format size mismatch.  Expected: ["
                        + DirectAccess.SizeOf<DDSPixelFormat>().ToString() + "], got: [" + header.PixelFormat.SizeInBytes.ToString() + "]");
            }

            // Ensure that we have at least one mip level.
            if (header.MipCount == 0)
            {
                header.MipCount = 1;
            }

            // Get DX 10 header information.
            if (((header.PixelFormat.Flags & DDSPixelFormatFlags.FourCC) == DDSPixelFormatFlags.FourCC) && (header.PixelFormat.FourCC == _pfDX10.FourCC))
            {
                settings = ReadDX10Header(reader, ref header, out conversionFlags);
            }
            else
            {
                if ((header.Flags & DDSHeaderFlags.Volume) == DDSHeaderFlags.Volume)
                {
                    settings = new GorgonTexture3DSettings();
                    settings.ArrayCount = 1;
                    settings.Depth = (int)header.Depth;
                }
                else
                {
                    settings = new GorgonTexture2DSettings();
                    settings.ArrayCount = 1;
                    settings.Depth = 1;
                    if ((header.Caps2 & DDSCAPS2.CubeMap) == DDSCAPS2.CubeMap)
                    {
                        // Only allow all faces.
                        if ((header.Caps2 & DDSCAPS2.AllFaces) != DDSCAPS2.AllFaces)
                        {
                            throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  Image is a cube map, but faces are missing from the cube.");
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
                    throw new System.IO.IOException("The data in the stream cannot be decoded as a DDS file.  The pixel format cannot be converted to a suitable buffer format.");
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
                        if (conversionFlags != DDSConversionFlags.None)
                        {
                            conversionFlags |= DDSConversionFlags.Swizzle;
                        }
                        break;
                    case BufferFormat.B8G8R8X8_UIntNormal:
                        settings.Format = BufferFormat.R8G8B8A8_UIntNormal;
                        if (conversionFlags != DDSConversionFlags.None)
                        {
                            conversionFlags  |= DDSConversionFlags.Swizzle | DDSConversionFlags.NoAlpha;
                        }
                        break;
                    case BufferFormat.B8G8R8A8:
                        settings.Format = BufferFormat.R8G8B8A8;
                        if (conversionFlags != DDSConversionFlags.None)
                        {
                            conversionFlags  |= DDSConversionFlags.Swizzle;
                        }
                        break;
                    case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
                        settings.Format = BufferFormat.R8G8B8A8_UIntNormal_sRGB;
                        if (conversionFlags != DDSConversionFlags.None)
                        {
                            conversionFlags  |= DDSConversionFlags.Swizzle;
                        }
                        break;

                    case BufferFormat.B8G8R8X8:
                        settings.Format = BufferFormat.R8G8B8A8;
                        if (conversionFlags != DDSConversionFlags.None)
                        {
                            conversionFlags  |= DDSConversionFlags.Swizzle | DDSConversionFlags.NoAlpha;
                        }
                        break;
                    case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
                        settings.Format = BufferFormat.R8G8B8A8_UIntNormal_sRGB;
                        if (conversionFlags != DDSConversionFlags.None)
                        {
                            conversionFlags  |= DDSConversionFlags.Swizzle | DDSConversionFlags.NoAlpha;
                        }
                        break;
                }
            }

			// Special flag for handling 16bpp formats
			if ((LegacyConversionFlags & DDSFlags.No16BPP) == DDSFlags.No16BPP)
			{
				switch (settings.Format)
				{
					case BufferFormat.B5G6R5_UIntNormal:
					case BufferFormat.B5G5R5A1_UIntNormal:
						settings.Format = BufferFormat.R8G8B8A8_UIntNormal;
						if (conversionFlags != DDSConversionFlags.None)
						{
							conversionFlags |= DDSConversionFlags.Expand;
							if (settings.Format == BufferFormat.B5G6R5_UIntNormal)
							{
								conversionFlags |= DDSConversionFlags.NoAlpha;
							}
						}
						break;
				}
			}

            return settings;
        }

		/// <summary>
		/// Function to write out the DDS header to the stream.
		/// </summary>
		/// <param name="settings">Meta data for the image header.</param>
		/// <param name="writer">Writer interface for the stream.</param>
		private void WriteHeader(IImageSettings settings, GorgonBinaryWriter writer)
		{
			DDSHeader header = new DDSHeader();
			DDSPixelFormat? format = null;
			DDSFlags flags = DDSFlags.None;
			var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.Format);

			if ((settings.ArrayCount > 1) && ((settings.ArrayCount != 6) || ((settings.ImageType != ImageType.Image2D) && (settings.ImageType != ImageType.ImageCube))))
			{
				flags = DDSFlags.ForceDX10;
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
					default:
						throw new ArgumentException("Cannot convert the format '" + settings.Format.ToString() + "' to a legacy format.", "settings");
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
			writer.WriteValue<DDSHeader>(header);

			// If we didn't map a legacy format, then use the DX 10 header.
			if (format == null)
			{
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

				writer.WriteValue<DX10Header>(dx10Header);
			}			
		}

		/// <summary>
		/// Function to read a scan line from the image data.
		/// </summary>
		/// <param name="reader">Reader interface for the stream.</param>
		/// <param name="row">Row index.</param>
		/// <param name="buffer">Current image buffer.</param>
		/// <param name="rowPitch">Row pitch of the buffer.</param>
		/// <param name="slicePitch">Slice pitch of the buffer.</param>
		/// <param name="conversionFlags">Conversion flags.</param>
		/// <param name="setAlpha">TRUE to set alpha to off, FALSE to preserve.</param>
		/// <param name="legacyScanLine">TRUE to use legacy scanlines, FALSE to something, something.</param>
		private void ReadScanLine(GorgonBinaryReader reader, int row, void *buffer, int rowPitch, int slicePitch, DDSConversionFlags conversionFlags, bool setAlpha, bool legacyScanLine)
		{
			if ((conversionFlags & DDSConversionFlags.Expand) == DDSConversionFlags.Expand)
			{
				// Perform expansion.
			}
			else if ((conversionFlags & DDSConversionFlags.Swizzle) == DDSConversionFlags.Swizzle)
			{
				// Perform swizzle.
			}
			else
			{
				// Perform straight copy.
			}
		}

		/// <summary>
		/// Function to perform the copying of image data into the buffer.
		/// </summary>
		/// <param name="reader">Reader interface for the stream.</param>
		/// <param name="image">Image data.</param>
		/// <param name="pitchFlags">Flags used to determine pitch when expanding pixels.</param>
		/// <param name="conversionFlags">Flags used for conversion between legacy formats and the current format.</param>
		/// <param name="palette">Palette used in indexed conversion.</param>
		private void CopyImageData(GorgonBinaryReader reader, GorgonImageData image, PitchFlags pitchFlags, DDSConversionFlags conversionFlags, GorgonColor[] palette)
		{
			var formatInfo = GorgonBufferFormatInfo.GetInfo(image.Settings.Format);
			GorgonFormatPitch pitchInfo = default(GorgonFormatPitch);
			int sizeInBytes = 0;

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
				else if (((conversionFlags & DDSConversionFlags.RGB44) == DDSConversionFlags.RGB44)
							|| ((conversionFlags & DDSConversionFlags.RGB332) == DDSConversionFlags.RGB332)
							|| ((conversionFlags & DDSConversionFlags.Palette) == DDSConversionFlags.Palette))
				{
					pitchFlags |= PitchFlags.BPP8;
				}
			}

			// Get the size of the image in bytes.
			pitchInfo = formatInfo.GetPitch(image.Settings.Width, image.Settings.Height, pitchFlags);
			sizeInBytes = GorgonImageData.GetSizeInBytes(image.Settings, pitchFlags);
			
			bool setAlpha = ((conversionFlags & DDSConversionFlags.NoAlpha) == DDSConversionFlags.NoAlpha);
			bool legacyScanLine = ((conversionFlags & DDSConversionFlags.Swizzle) == DDSConversionFlags.Swizzle);

			switch (image.Settings.ImageType)
			{
				case ImageType.Image1D:
				case ImageType.Image2D:
					for (int array = 0; array < image.Settings.ArrayCount; array++)
					{
						for (int mipLevel = 0; mipLevel < image.Settings.MipCount; mipLevel++)
						{
							// Get our destination buffer.
							var buffer = image[array, mipLevel];

							// Get compressed data.
							if ((formatInfo.IsCompressed) || (conversionFlags == DDSConversionFlags.None))
							{
								reader.Read(buffer.Data.UnsafePointer, buffer.PitchInformation.SlicePitch);
							}
                            else
                            {
                                byte* bufferPointer = (byte*)buffer.Data.UnsafePointer;

                                // If not compressed, then read each scan line.
                                for (int h = 0; h < buffer.Height; h++)
                                {
                                    bufferPointer += buffer.PitchInformation.SlicePitch;
                                }
                            }
						}
					}
					break;
			}
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the data to load.</param>
		/// <returns>
		/// The image data that was in the stream.
		/// </returns>
		protected internal override GorgonImageData LoadFromStream(System.IO.Stream stream)
		{
			GorgonImageData imageData = null;
			DDSConversionFlags flags = DDSConversionFlags.None;
			IImageSettings settings = null;
			GorgonColor[] palette = null;


			using (var reader = new GorgonBinaryReader(stream, true))
			{
				// Read the header information.
				settings = ReadHeader(reader, out flags);

				// Create our image data structure.
				imageData = new GorgonImageData(settings);

				// We have a palette, either create a new one or clone the assigned one.
				if ((flags & DDSConversionFlags.Palette) == DDSConversionFlags.Palette)
				{
					palette = new GorgonColor[256];
					if (Palette != null)
					{						
						int count = Palette.Count > 256 ? 256 : Palette.Count;

						for (int i = 0; i < count; i++)
						{
							palette[i] = Palette[i];
						}
					}
				}

				try
				{
					// Copy the data from the stream to the buffer.
					CopyImageData(reader, imageData, ((LegacyConversionFlags & DDSFlags.LegacyDWORD) == DDSFlags.LegacyDWORD) ? PitchFlags.LegacyDWORD : PitchFlags.None, flags, palette);
				}
				catch 
				{
					// Clean up any memory allocated if we can't copy the image.
					if (imageData != null)
					{
						imageData.Dispose();
					}

					throw;
				}
			}

			return imageData;
		}

		/// <summary>
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="GorgonLibrary.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		protected internal override void SaveToStream(GorgonImageData imageData, System.IO.Stream stream)
		{
			if (imageData == null)
			{
				throw new ArgumentNullException("imageData");
			}

			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new System.IO.IOException("The stream is read-only.");
			}

			// Use a binary writer.
			using (GorgonBinaryWriter writer = new GorgonBinaryWriter(stream, true))
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
								var buffer = imageData[array, mipLevel];
								
								writer.Write(buffer.Data.UnsafePointer, buffer.PitchInformation.SlicePitch);
							}
						}
						break;
					case ImageType.Image3D:
						int depth = imageData.Settings.Depth;
						for (int mipLevel = 0; mipLevel < imageData.Settings.MipCount; mipLevel++)
						{							
							for (int slice = 0; slice < depth; slice++)
							{
								var buffer = imageData[0, mipLevel, slice];
								writer.Write(buffer.Data.UnsafePointer, buffer.PitchInformation.SlicePitch);
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
		/// <param name="stream">Stream used to read the metadata.</param>
		/// <returns>
		/// The image meta data as a <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see> value.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
		/// <para>-or-</para>
		/// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override IImageSettings GetMetaData(System.IO.Stream stream)
		{
			DDSConversionFlags flags = DDSConversionFlags.None;

			using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
			{
				return this.ReadHeader(reader, out flags);
			}
		}

		/// <summary>
		/// Function to determine if this codec can read the file or not.
		/// </summary>
		/// <param name="stream">Stream used to read the file information.</param>
		/// <returns>
		/// TRUE if the codec can read the file, FALSE if not.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override bool CanBeRead(System.IO.Stream stream)
        {
            uint magicNumber = 0;
            long position = stream.Position;

            if (!stream.CanRead)
            {
                throw new System.IO.IOException("Stream is write-only.");
            }

            if (!stream.CanSeek)
            {
                throw new System.IO.IOException("The stream cannot perform seek operations.");
            }

            using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
            {
                magicNumber = reader.ReadUInt32();
            }

            stream.Position = position;
            return magicNumber == MagicNumber;
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecDDS" /> class.
		/// </summary>
		internal GorgonCodecDDS()
		{
			this.CodecCommonExtensions = new string[] { "DDS" };
            _formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));

            LegacyConversionFlags = DDSFlags.None;
		}
		#endregion
	}
}
