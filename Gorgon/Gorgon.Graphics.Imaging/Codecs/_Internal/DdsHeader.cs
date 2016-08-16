﻿#region MIT
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
// Created: August 16, 2016 1:03:56 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Native;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Flags to convert older DDS files (pre-DX10).
	/// </summary>
	[Flags]
	enum DdsConversionFlags
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
	enum DdsHeaderFlags
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
	enum DdsHeaderMiscFlags
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
	enum DdsPixelFormatFlags
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
	enum DdsCaps1
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
	enum DdsCaps2
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
	
	/// <summary>
	/// DDS legacy conversion type.
	/// </summary>
	struct DdsLegacyConversion
	{
		/// <summary>
		/// Buffer format.
		/// </summary>
		public readonly DXGI.Format Format;
		/// <summary>
		/// Conversion flags.
		/// </summary>
		public readonly DdsConversionFlags Flags;
		/// <summary>
		/// Pixel format.
		/// </summary>
		public readonly DdsPixelFormat PixelFormat;

		/// <summary>
		/// Initializes a new instance of the <see cref="DdsLegacyConversion" /> struct.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="flags">The flags.</param>
		/// <param name="pixelFormat">The pixel format.</param>
		public DdsLegacyConversion(DXGI.Format format, DdsConversionFlags flags, DdsPixelFormat pixelFormat)
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
	struct DdsHeader
	{
		/// <summary>
		/// Size of the header structure.
		/// </summary>
		public uint Size;
		/// <summary>
		/// Header flags.
		/// </summary>
		public DdsHeaderFlags Flags;
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
		public DdsPixelFormat PixelFormat;
		/// <summary>
		/// Capabilities #1.
		/// </summary>
		public DdsCaps1 Caps1;
		/// <summary>
		/// Capabilities #2.
		/// </summary>
		public DdsCaps2 Caps2;
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
	struct Dx10Header
	{
		/// <summary>
		/// Format.
		/// </summary>
		public DXGI.Format Format;
		/// <summary>
		/// Resource dimension.
		/// </summary>
		public ImageType ResourceDimension;
		/// <summary>
		/// Miscellaneous flags.
		/// </summary>
		public DdsHeaderMiscFlags MiscFlags;
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
	struct DdsPixelFormat
	{
		/// <summary>
		/// Size of the format, in bytes.
		/// </summary>
		public readonly uint SizeInBytes;
		/// <summary>
		/// Flags for the format.
		/// </summary>
		public readonly DdsPixelFormatFlags Flags;
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
		/// Initializes a new instance of the <see cref="DdsPixelFormat" /> struct.
		/// </summary>
		/// <param name="flags">The flags.</param>
		/// <param name="fourCC">The four CC.</param>
		/// <param name="bitCount">The bit count.</param>
		/// <param name="rMask">The r mask.</param>
		/// <param name="gMask">The g mask.</param>
		/// <param name="bMask">The b mask.</param>
		/// <param name="aMask">A mask.</param>
		public DdsPixelFormat(DdsPixelFormatFlags flags, uint fourCC, uint bitCount, uint rMask, uint gMask, uint bMask, uint aMask)
		{
			SizeInBytes = (uint)DirectAccess.SizeOf<DdsPixelFormat>();
			Flags = flags;
			FourCC = fourCC;
			BitCount = bitCount;
			RBitMask = rMask;
			GBitMask = gMask;
			BBitMask = bMask;
			ABitMask = aMask;
		}
	}
}
