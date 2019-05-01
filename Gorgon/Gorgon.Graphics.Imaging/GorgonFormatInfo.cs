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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;

namespace Gorgon.Graphics
{
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
        public static readonly GorgonFormatInfo UnknownFormatInfo = new GorgonFormatInfo(BufferFormat.Unknown);
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
		public bool IsPacked => (Format == BufferFormat.R8G8_B8G8_UNorm)
								|| (Format == BufferFormat.G8R8_G8B8_UNorm)
								|| (Format == BufferFormat.Y410)
								|| (Format == BufferFormat.Y416)
								|| (Format == BufferFormat.Y210)
								|| (Format == BufferFormat.Y216);

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
		public bool IsPalettized => Format == BufferFormat.A8P8 || Format == BufferFormat.P8;
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
		private void GetBitDepth(BufferFormat format)
		{
			switch (format)
			{
				case BufferFormat.R1_UNorm:
					BitDepth = 1;
					break;
				case BufferFormat.BC1_Typeless:
				case BufferFormat.BC1_UNorm:
				case BufferFormat.BC1_UNorm_SRgb:
				case BufferFormat.BC4_Typeless:
				case BufferFormat.BC4_UNorm:
				case BufferFormat.BC4_SNorm:
					BitDepth = 4;
					break;
				case BufferFormat.R8_Typeless:
				case BufferFormat.R8_UNorm:
				case BufferFormat.R8_UInt:
				case BufferFormat.R8_SNorm:
				case BufferFormat.R8_SInt:
				case BufferFormat.A8_UNorm:
				case BufferFormat.BC2_Typeless:
				case BufferFormat.BC2_UNorm:
				case BufferFormat.BC2_UNorm_SRgb:
				case BufferFormat.BC3_Typeless:
				case BufferFormat.BC3_UNorm:
				case BufferFormat.BC3_UNorm_SRgb:
				case BufferFormat.BC5_Typeless:
				case BufferFormat.BC5_UNorm:
				case BufferFormat.BC5_SNorm:
				case BufferFormat.BC6H_Typeless:
				case BufferFormat.BC6H_Uf16:
				case BufferFormat.BC6H_Sf16:
				case BufferFormat.BC7_Typeless:
				case BufferFormat.BC7_UNorm:
				case BufferFormat.BC7_UNorm_SRgb:
				case BufferFormat.P8:
				case BufferFormat.AI44:
				case BufferFormat.IA44:
					BitDepth = 8;
					break;
				case BufferFormat.NV11:
				case BufferFormat.NV12:
				case BufferFormat.Opaque420:
					BitDepth = 12;
					break;
				case BufferFormat.R8G8_Typeless:
				case BufferFormat.R8G8_UNorm:
				case BufferFormat.R8G8_UInt:
				case BufferFormat.R8G8_SNorm:
				case BufferFormat.R8G8_SInt:
				case BufferFormat.R16_Typeless:
				case BufferFormat.R16_Float:
				case BufferFormat.D16_UNorm:
				case BufferFormat.R16_UNorm:
				case BufferFormat.R16_UInt:
				case BufferFormat.R16_SNorm:
				case BufferFormat.R16_SInt:
				case BufferFormat.B5G6R5_UNorm:
				case BufferFormat.B5G5R5A1_UNorm:
				case BufferFormat.B4G4R4A4_UNorm:
				case BufferFormat.A8P8:
					BitDepth = 16;
					break;
				case BufferFormat.P010:
				case BufferFormat.P016:
					BitDepth = 24;
					break;
				case BufferFormat.R10G10B10A2_Typeless:
				case BufferFormat.R10G10B10A2_UNorm:
				case BufferFormat.R10G10B10A2_UInt:
				case BufferFormat.R11G11B10_Float:
				case BufferFormat.R8G8B8A8_Typeless:
				case BufferFormat.R8G8B8A8_UNorm:
				case BufferFormat.R8G8B8A8_UNorm_SRgb:
				case BufferFormat.R8G8B8A8_UInt:
				case BufferFormat.R8G8B8A8_SNorm:
				case BufferFormat.R8G8B8A8_SInt:
				case BufferFormat.R16G16_Typeless:
				case BufferFormat.R16G16_Float:
				case BufferFormat.R16G16_UNorm:
				case BufferFormat.R16G16_UInt:
				case BufferFormat.R16G16_SNorm:
				case BufferFormat.R16G16_SInt:
				case BufferFormat.R32_Typeless:
				case BufferFormat.D32_Float:
				case BufferFormat.R32_Float:
				case BufferFormat.R32_UInt:
				case BufferFormat.R32_SInt:
				case BufferFormat.R24G8_Typeless:
				case BufferFormat.D24_UNorm_S8_UInt:
				case BufferFormat.R24_UNorm_X8_Typeless:
				case BufferFormat.X24_Typeless_G8_UInt:
				case BufferFormat.R9G9B9E5_Sharedexp:
				case BufferFormat.R8G8_B8G8_UNorm:
				case BufferFormat.G8R8_G8B8_UNorm:
				case BufferFormat.B8G8R8A8_UNorm:
				case BufferFormat.B8G8R8X8_UNorm:
				case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
				case BufferFormat.B8G8R8A8_Typeless:
				case BufferFormat.B8G8R8A8_UNorm_SRgb:
				case BufferFormat.B8G8R8X8_Typeless:
				case BufferFormat.B8G8R8X8_UNorm_SRgb:
				case BufferFormat.AYUV:
				case BufferFormat.Y410:
				case BufferFormat.YUY2:
					BitDepth = 32;
					break;
				case BufferFormat.R16G16B16A16_Typeless:
				case BufferFormat.R16G16B16A16_Float:
				case BufferFormat.R16G16B16A16_UNorm:
				case BufferFormat.R16G16B16A16_UInt:
				case BufferFormat.R16G16B16A16_SNorm:
				case BufferFormat.R16G16B16A16_SInt:
				case BufferFormat.R32G32_Typeless:
				case BufferFormat.R32G32_Float:
				case BufferFormat.R32G32_UInt:
				case BufferFormat.R32G32_SInt:
				case BufferFormat.R32G8X24_Typeless:
				case BufferFormat.D32_Float_S8X24_UInt:
				case BufferFormat.R32_Float_X8X24_Typeless:
				case BufferFormat.X32_Typeless_G8X24_UInt:
				case BufferFormat.Y416:
				case BufferFormat.Y210:
				case BufferFormat.Y216:
					BitDepth = 64;
					break;
				case BufferFormat.R32G32B32_Typeless:
				case BufferFormat.R32G32B32_Float:
				case BufferFormat.R32G32B32_UInt:
				case BufferFormat.R32G32B32_SInt:
					BitDepth = 96;
					break;
				case BufferFormat.R32G32B32A32_Typeless:
				case BufferFormat.R32G32B32A32_Float:
				case BufferFormat.R32G32B32A32_UInt:
				case BufferFormat.R32G32B32A32_SInt:
					BitDepth = 128;
					break;
				default:
					BitDepth = 0;
					break;
			}
		}

		/// <summary>
		/// Function to retrieve whether this format is an SRgb format or not.
		/// </summary>
		/// <param name="format">Format to look up.</param>
		private void GetSRgbState(BufferFormat format)
		{
			switch (format)
			{
				case BufferFormat.R8G8B8A8_UNorm_SRgb:
				case BufferFormat.B8G8R8A8_UNorm_SRgb:
				case BufferFormat.B8G8R8X8_UNorm_SRgb:
				case BufferFormat.BC1_UNorm_SRgb:
				case BufferFormat.BC2_UNorm_SRgb:
				case BufferFormat.BC3_UNorm_SRgb:
				case BufferFormat.BC7_UNorm_SRgb:
					IsSRgb = true;
					break;
				default:
					IsSRgb = false;
					break;
			}
		}

        /// <summary>
        /// Function to retrieve the number of components that make up a format.
        /// </summary>
        /// <param name="format">The format to evaulate.</param>
	    private void GetComponentCount(BufferFormat format)
	    {
	        switch (format)
	        {
	            case BufferFormat.R32G32B32A32_Typeless:
	            case BufferFormat.R32G32B32A32_Float:
	            case BufferFormat.R32G32B32A32_UInt:
	            case BufferFormat.R32G32B32A32_SInt:
	            case BufferFormat.R16G16B16A16_Typeless:
	            case BufferFormat.R16G16B16A16_Float:
	            case BufferFormat.R16G16B16A16_UNorm:
	            case BufferFormat.R16G16B16A16_UInt:
	            case BufferFormat.R16G16B16A16_SNorm:
	            case BufferFormat.R16G16B16A16_SInt:
	            case BufferFormat.R10G10B10A2_Typeless:
	            case BufferFormat.R10G10B10A2_UNorm:
	            case BufferFormat.R10G10B10A2_UInt:
	            case BufferFormat.R8G8B8A8_Typeless:
	            case BufferFormat.R8G8B8A8_UNorm:
	            case BufferFormat.R8G8B8A8_UNorm_SRgb:
	            case BufferFormat.R8G8B8A8_UInt:
	            case BufferFormat.R8G8B8A8_SNorm:
	            case BufferFormat.R8G8B8A8_SInt:
	            case BufferFormat.R8G8_B8G8_UNorm:
	            case BufferFormat.G8R8_G8B8_UNorm:
	            case BufferFormat.BC1_Typeless:
	            case BufferFormat.BC1_UNorm:
	            case BufferFormat.BC1_UNorm_SRgb:
	            case BufferFormat.BC2_Typeless:
	            case BufferFormat.BC2_UNorm:
	            case BufferFormat.BC2_UNorm_SRgb:
	            case BufferFormat.BC3_Typeless:
	            case BufferFormat.BC3_UNorm:
	            case BufferFormat.BC3_UNorm_SRgb:
	            case BufferFormat.BC4_Typeless:
	            case BufferFormat.BC4_UNorm:
	            case BufferFormat.BC4_SNorm:
	            case BufferFormat.BC5_Typeless:
	            case BufferFormat.BC5_UNorm:
	            case BufferFormat.BC5_SNorm:
	            case BufferFormat.B5G5R5A1_UNorm:
	            case BufferFormat.B8G8R8A8_UNorm:
	            case BufferFormat.B8G8R8X8_UNorm:
	            case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
	            case BufferFormat.B8G8R8A8_Typeless:
	            case BufferFormat.B8G8R8A8_UNorm_SRgb:
	            case BufferFormat.B8G8R8X8_Typeless:
	            case BufferFormat.B8G8R8X8_UNorm_SRgb:
	            case BufferFormat.BC6H_Typeless:
	            case BufferFormat.BC6H_Uf16:
	            case BufferFormat.BC6H_Sf16:
	            case BufferFormat.BC7_Typeless:
	            case BufferFormat.BC7_UNorm:
	            case BufferFormat.BC7_UNorm_SRgb:
	            case BufferFormat.B4G4R4A4_UNorm:
	                ComponentCount = 4;
	                break;
	            case BufferFormat.R11G11B10_Float:
	            case BufferFormat.R32G32B32_Typeless:
	            case BufferFormat.R32G32B32_Float:
	            case BufferFormat.R32G32B32_UInt:
	            case BufferFormat.R32G32B32_SInt:
	            case BufferFormat.B5G6R5_UNorm:
	            case BufferFormat.R9G9B9E5_Sharedexp:
	                ComponentCount = 3;
                    break;
	            case BufferFormat.R32G32_Typeless:
	            case BufferFormat.R32G32_Float:
	            case BufferFormat.R32G32_UInt:
	            case BufferFormat.R32G32_SInt:
	            case BufferFormat.R32G8X24_Typeless:
	            case BufferFormat.R16G16_Typeless:
	            case BufferFormat.R16G16_Float:
	            case BufferFormat.R16G16_UNorm:
	            case BufferFormat.R16G16_UInt:
	            case BufferFormat.R16G16_SNorm:
	            case BufferFormat.R16G16_SInt:
	            case BufferFormat.R24G8_Typeless:
	            case BufferFormat.R8G8_Typeless:
	            case BufferFormat.R8G8_UNorm:
	            case BufferFormat.R8G8_UInt:
	            case BufferFormat.R8G8_SNorm:
	            case BufferFormat.R8G8_SInt:
	            case BufferFormat.AYUV:
	            case BufferFormat.YUY2:
	            case BufferFormat.NV11:
	            case BufferFormat.AI44:
	            case BufferFormat.IA44:
	            case BufferFormat.A8P8:
	                ComponentCount = 2;
	                break;
	            case BufferFormat.R8_Typeless:
	            case BufferFormat.R8_UNorm:
	            case BufferFormat.R8_UInt:
	            case BufferFormat.R8_SNorm:
	            case BufferFormat.R8_SInt:
	            case BufferFormat.A8_UNorm:
	            case BufferFormat.R1_UNorm:
	            case BufferFormat.Y410:
	            case BufferFormat.Y416:
	            case BufferFormat.NV12:
	            case BufferFormat.P010:
	            case BufferFormat.P016:
	            case BufferFormat.Opaque420:
	            case BufferFormat.Y210:
	            case BufferFormat.Y216:
	            case BufferFormat.P8:
	            case BufferFormat.P208:
	            case BufferFormat.V208:
	            case BufferFormat.V408:
	            case BufferFormat.X32_Typeless_G8X24_UInt:
	            case BufferFormat.R32_Float_X8X24_Typeless:
	            case BufferFormat.R32_Typeless:
	            case BufferFormat.D32_Float:
	            case BufferFormat.R32_Float:
	            case BufferFormat.R32_UInt:
	            case BufferFormat.R32_SInt:
	            case BufferFormat.R16_Typeless:
	            case BufferFormat.R16_Float:
	            case BufferFormat.D16_UNorm:
	            case BufferFormat.R16_UNorm:
	            case BufferFormat.R16_UInt:
	            case BufferFormat.R16_SNorm:
	            case BufferFormat.R16_SInt:
	            case BufferFormat.D24_UNorm_S8_UInt:
	            case BufferFormat.R24_UNorm_X8_Typeless:
	            case BufferFormat.X24_Typeless_G8_UInt:
	            case BufferFormat.D32_Float_S8X24_UInt:
	                ComponentCount = 1;
	                break;
                default:
                    ComponentCount = 0;
                    break;
	        }
        }

		/// <summary>
		/// Function to retrieve whether a buffer is compressed or not.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		private void GetCompressedState(BufferFormat format)
		{
			switch (format)
			{
				case BufferFormat.BC1_Typeless:
				case BufferFormat.BC1_UNorm:
				case BufferFormat.BC1_UNorm_SRgb:
				case BufferFormat.BC2_Typeless:
				case BufferFormat.BC2_UNorm:
				case BufferFormat.BC2_UNorm_SRgb:
				case BufferFormat.BC3_Typeless:
				case BufferFormat.BC3_UNorm:
				case BufferFormat.BC3_UNorm_SRgb:
				case BufferFormat.BC4_Typeless:
				case BufferFormat.BC4_SNorm:
				case BufferFormat.BC4_UNorm:
				case BufferFormat.BC5_Typeless:
				case BufferFormat.BC5_SNorm:
				case BufferFormat.BC5_UNorm:
				case BufferFormat.BC6H_Typeless:
				case BufferFormat.BC6H_Sf16:
				case BufferFormat.BC6H_Uf16:
				case BufferFormat.BC7_Typeless:
				case BufferFormat.BC7_UNorm:
				case BufferFormat.BC7_UNorm_SRgb:
					IsCompressed = true;
					break;
				default:
					IsCompressed = false;
					break;
			}
		}

		/// <summary>
		/// Function to determine which typeless group the format belongs to.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		private void GetGroup(BufferFormat format)
		{
			switch (format)
			{
				case BufferFormat.B8G8R8A8_Typeless:
				case BufferFormat.B8G8R8A8_UNorm:
				case BufferFormat.B8G8R8A8_UNorm_SRgb:
					Group = BufferFormat.B8G8R8A8_Typeless;
					break;
				case BufferFormat.B8G8R8X8_Typeless:
				case BufferFormat.B8G8R8X8_UNorm:
				case BufferFormat.B8G8R8X8_UNorm_SRgb:
					Group = BufferFormat.B8G8R8X8_Typeless;
					break;
				case BufferFormat.BC1_Typeless:
				case BufferFormat.BC1_UNorm:
				case BufferFormat.BC1_UNorm_SRgb:
					Group = BufferFormat.BC1_Typeless;
					break;
				case BufferFormat.BC2_Typeless:
				case BufferFormat.BC2_UNorm:
				case BufferFormat.BC2_UNorm_SRgb:
					Group = BufferFormat.BC2_Typeless;
					break;
				case BufferFormat.BC3_Typeless:
				case BufferFormat.BC3_UNorm:
				case BufferFormat.BC3_UNorm_SRgb:
					Group = BufferFormat.BC3_Typeless;
					break;
				case BufferFormat.BC4_Typeless:
				case BufferFormat.BC4_UNorm:
				case BufferFormat.BC4_SNorm:
					Group = BufferFormat.BC4_Typeless;
					break;
				case BufferFormat.BC5_Typeless:
				case BufferFormat.BC5_UNorm:
				case BufferFormat.BC5_SNorm:
					Group = BufferFormat.BC5_Typeless;
					break;
				case BufferFormat.BC6H_Typeless:
				case BufferFormat.BC6H_Uf16:
				case BufferFormat.BC6H_Sf16:
					Group = BufferFormat.BC6H_Typeless;
					break;
				case BufferFormat.BC7_Typeless:
				case BufferFormat.BC7_UNorm:
				case BufferFormat.BC7_UNorm_SRgb:
					Group = BufferFormat.BC7_Typeless;
					break;
				case BufferFormat.R10G10B10A2_Typeless:
				case BufferFormat.R10G10B10A2_UNorm:
				case BufferFormat.R10G10B10A2_UInt:
					Group = BufferFormat.R10G10B10A2_Typeless;
					break;
				case BufferFormat.R16_Typeless:
				case BufferFormat.R16_Float:
				case BufferFormat.R16_UNorm:
				case BufferFormat.R16_UInt:
				case BufferFormat.R16_SNorm:
				case BufferFormat.R16_SInt:
					Group = BufferFormat.R16_Typeless;
					break;
				case BufferFormat.R16G16_Typeless:
				case BufferFormat.R16G16_Float:
				case BufferFormat.R16G16_UNorm:
				case BufferFormat.R16G16_UInt:
				case BufferFormat.R16G16_SNorm:
				case BufferFormat.R16G16_SInt:
					Group = BufferFormat.R16G16_Typeless;
					break;
				case BufferFormat.R16G16B16A16_Typeless:
				case BufferFormat.R16G16B16A16_Float:
				case BufferFormat.R16G16B16A16_UNorm:
				case BufferFormat.R16G16B16A16_UInt:
				case BufferFormat.R16G16B16A16_SNorm:
				case BufferFormat.R16G16B16A16_SInt:
					Group = BufferFormat.R16G16B16A16_Typeless;
					break;
				case BufferFormat.R32_Typeless:
				case BufferFormat.R32_Float:
				case BufferFormat.R32_UInt:
				case BufferFormat.R32_SInt:
					Group = BufferFormat.R32_Typeless;
					break;
				case BufferFormat.R32G32_Typeless:
				case BufferFormat.R32G32_Float:
				case BufferFormat.R32G32_UInt:
				case BufferFormat.R32G32_SInt:
					Group = BufferFormat.R32G32_Typeless;
					break;
				case BufferFormat.R32G32B32_Typeless:
				case BufferFormat.R32G32B32_Float:
				case BufferFormat.R32G32B32_UInt:
				case BufferFormat.R32G32B32_SInt:
					Group = BufferFormat.R32G32B32_Typeless;
					break;
				case BufferFormat.R32G32B32A32_Typeless:
				case BufferFormat.R32G32B32A32_Float:
				case BufferFormat.R32G32B32A32_UInt:
				case BufferFormat.R32G32B32A32_SInt:
					Group = BufferFormat.R32G32B32A32_Typeless;
					break;
				case BufferFormat.R8_Typeless:
				case BufferFormat.R8_UNorm:
				case BufferFormat.R8_UInt:
				case BufferFormat.R8_SNorm:
				case BufferFormat.R8_SInt:
					Group = BufferFormat.R8_Typeless;
					break;
				case BufferFormat.R8G8_Typeless:
				case BufferFormat.R8G8_UNorm:
				case BufferFormat.R8G8_UInt:
				case BufferFormat.R8G8_SNorm:
				case BufferFormat.R8G8_SInt:
					Group = BufferFormat.R8G8_Typeless;
					break;
				case BufferFormat.R8G8B8A8_Typeless:
				case BufferFormat.R8G8B8A8_UNorm:
				case BufferFormat.R8G8B8A8_UNorm_SRgb:
				case BufferFormat.R8G8B8A8_UInt:
				case BufferFormat.R8G8B8A8_SNorm:
				case BufferFormat.R8G8B8A8_SInt:
					Group = BufferFormat.R8G8B8A8_Typeless;
					break;
				default:
					Group = BufferFormat.Unknown;
					break;
			}
		}

		/// <summary>
		/// Function to determine if the specified format is typeless.
		/// </summary>
		/// <param name="format">The format to check.</param>
		private void GetTypelessState(BufferFormat format)
		{
			switch (format)
			{
				case BufferFormat.R32G32B32A32_Typeless:
				case BufferFormat.R32G32B32_Typeless:
				case BufferFormat.R16G16B16A16_Typeless:
				case BufferFormat.R32G32_Typeless:
				case BufferFormat.R32G8X24_Typeless:
				case BufferFormat.R10G10B10A2_Typeless:
				case BufferFormat.R8G8B8A8_Typeless:
				case BufferFormat.R16G16_Typeless:
				case BufferFormat.R32_Typeless:
				case BufferFormat.R24G8_Typeless:
				case BufferFormat.R8G8_Typeless:
				case BufferFormat.R16_Typeless:
				case BufferFormat.R8_Typeless:
				case BufferFormat.BC1_Typeless:
				case BufferFormat.BC2_Typeless:
				case BufferFormat.BC3_Typeless:
				case BufferFormat.BC4_Typeless:
				case BufferFormat.BC5_Typeless:
				case BufferFormat.B8G8R8A8_Typeless:
				case BufferFormat.B8G8R8X8_Typeless:
				case BufferFormat.BC6H_Typeless:
				case BufferFormat.BC7_Typeless:
				case BufferFormat.Unknown:
					IsTypeless = true;
					break;
				default:
					IsTypeless = false;
					break;
			}
		}

		/// <summary>
		/// Function to determine if the format has an alpha channel.
		/// </summary>
		/// <param name="format">Format to check.</param>
		private void GetAlphaChannel(BufferFormat format)
		{
			switch (format)
			{
				case BufferFormat.R32G32B32A32_Typeless:
				case BufferFormat.R32G32B32A32_Float:
				case BufferFormat.R32G32B32A32_UInt:
				case BufferFormat.R32G32B32A32_SInt:
				case BufferFormat.R16G16B16A16_Typeless:
				case BufferFormat.R16G16B16A16_Float:
				case BufferFormat.R16G16B16A16_UNorm:
				case BufferFormat.R16G16B16A16_UInt:
				case BufferFormat.R16G16B16A16_SNorm:
				case BufferFormat.R16G16B16A16_SInt:
				case BufferFormat.R10G10B10A2_Typeless:
				case BufferFormat.R10G10B10A2_UNorm:
				case BufferFormat.R10G10B10A2_UInt:
				case BufferFormat.R8G8B8A8_Typeless:
				case BufferFormat.R8G8B8A8_UNorm:
				case BufferFormat.R8G8B8A8_UNorm_SRgb:
				case BufferFormat.R8G8B8A8_UInt:
				case BufferFormat.R8G8B8A8_SNorm:
				case BufferFormat.R8G8B8A8_SInt:
				case BufferFormat.A8_UNorm:
				case BufferFormat.BC1_Typeless:
				case BufferFormat.BC1_UNorm:
				case BufferFormat.BC1_UNorm_SRgb:
				case BufferFormat.BC2_Typeless:
				case BufferFormat.BC2_UNorm:
				case BufferFormat.BC2_UNorm_SRgb:
				case BufferFormat.BC3_Typeless:
				case BufferFormat.BC3_UNorm:
				case BufferFormat.BC3_UNorm_SRgb:
				case BufferFormat.B5G5R5A1_UNorm:
				case BufferFormat.B8G8R8A8_UNorm:
				case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
				case BufferFormat.B8G8R8A8_Typeless:
				case BufferFormat.B8G8R8A8_UNorm_SRgb:
				case BufferFormat.BC7_Typeless:
				case BufferFormat.BC7_UNorm:
				case BufferFormat.BC7_UNorm_SRgb:
				case BufferFormat.B4G4R4A4_UNorm:
				case BufferFormat.A8P8:
					HasAlpha = true;
					break;
				default:
					HasAlpha = false;
					break;
			}
		}

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
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "height+3")]
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "height+1")]
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "width+1")]
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "width+3")]
		public GorgonPitchLayout GetPitchForFormat(int width, int height, PitchFlags flags = PitchFlags.None)
		{
            int rowPitch;

            // Do calculations for compressed formats.
            if (IsCompressed)
			{
                int bpb;

                switch (Format)
				{
					case BufferFormat.BC1_Typeless:
					case BufferFormat.BC1_UNorm:
					case BufferFormat.BC1_UNorm_SRgb:
					case BufferFormat.BC4_Typeless:
					case BufferFormat.BC4_SNorm:
					case BufferFormat.BC4_UNorm:
						bpb = 8;
						break;
					default:
						bpb = 16;
						break;
				}
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
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "height+1")]
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "height+3")]
		public int CalculateScanlines(int height)
		{
			if (IsCompressed)
			{
				return 1.Max((height + 3) >> 2);
			}

			switch (Format)
			{
				// These are planar formats.
				case BufferFormat.NV11:
					return height << 1;
				case BufferFormat.NV12:
				case BufferFormat.P010:
				case BufferFormat.P016:
				case BufferFormat.Opaque420:
					return height + ((height + 1) >> 1);
				// All other formats report height as-is.
				default:
					return height;
			}
		}
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
}
