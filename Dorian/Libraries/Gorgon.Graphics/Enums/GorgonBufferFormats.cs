#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, July 25, 2011 8:10:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A list of various buffer formats supported by back end graphics APIs
	/// </summary>
	/// <remarks>Some APIs will need to map to equivalent buffer formats (internally within their own plug-ins) where applicable.  However, some APIs won't support some formats, and calls using these formats may not work if an equivalent format is not found.</remarks>
	public enum GorgonBufferFormat
	{
		/// <summary>
		/// The Unknown buffer format.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// The R32G32B32A32 buffer format.
		/// </summary>
		R32G32B32A32 = 1,
		/// <summary>
		/// The R32G32B32A32_Float buffer format.
		/// </summary>
		R32G32B32A32_Float = 2,
		/// <summary>
		/// The R32G32B32A32_UInt buffer format.
		/// </summary>
		R32G32B32A32_UInt = 3,
		/// <summary>
		/// The R32G32B32A32_Int buffer format.
		/// </summary>
		R32G32B32A32_Int = 4,
		/// <summary>
		/// The R32G32B32 buffer format.
		/// </summary>
		R32G32B32 = 5,
		/// <summary>
		/// The R32G32B32_Float buffer format.
		/// </summary>
		R32G32B32_Float = 6,
		/// <summary>
		/// The R32G32B32_UInt buffer format.
		/// </summary>
		R32G32B32_UInt = 7,
		/// <summary>
		/// The R32G32B32_Int buffer format.
		/// </summary>
		R32G32B32_Int = 8,
		/// <summary>
		/// The R16G16B16A16 buffer format.
		/// </summary>
		R16G16B16A16 = 9,
		/// <summary>
		/// The R16G16B16A16_Float buffer format.
		/// </summary>
		R16G16B16A16_Float = 10,
		/// <summary>
		/// The R16G16B16A16_UIntNormal buffer format.
		/// </summary>
		R16G16B16A16_UIntNormal = 11,
		/// <summary>
		/// The R16G16B16A16_UInt buffer format.
		/// </summary>
		R16G16B16A16_UInt = 12,
		/// <summary>
		/// The R16G16B16A16_IntNormal buffer format.
		/// </summary>
		R16G16B16A16_IntNormal = 13,
		/// <summary>
		/// The R16G16B16A16_Int buffer format.
		/// </summary>
		R16G16B16A16_Int = 14,
		/// <summary>
		/// The R32G32 buffer format.
		/// </summary>
		R32G32 = 15,
		/// <summary>
		/// The R32G32_Float buffer format.
		/// </summary>
		R32G32_Float = 16,
		/// <summary>
		/// The R32G32_UInt buffer format.
		/// </summary>
		R32G32_UInt = 17,
		/// <summary>
		/// The R32G32_Int buffer format.
		/// </summary>
		R32G32_Int = 18,
		/// <summary>
		/// The R32G8X24 buffer format.
		/// </summary>
		R32G8X24 = 19,
		/// <summary>
		/// The D32_Float_S8X24_UInt buffer format.
		/// </summary>
		D32_Float_S8X24_UInt = 20,
		/// <summary>
		/// The R32_Float_X8X24 buffer format.
		/// </summary>
		R32_Float_X8X24 = 21,
		/// <summary>
		/// The X32_G8X24_UInt buffer format.
		/// </summary>
		X32_G8X24_UInt = 22,
		/// <summary>
		/// The R10G10B10A2 buffer format.
		/// </summary>
		R10G10B10A2 = 23,
		/// <summary>
		/// The R10G10B10A2_UIntNormal buffer format.
		/// </summary>
		R10G10B10A2_UIntNormal = 24,
		/// <summary>
		/// The R10G10B10A2_UInt buffer format.
		/// </summary>
		R10G10B10A2_UInt = 25,
		/// <summary>
		/// The R11G11B10_Float buffer format.
		/// </summary>
		R11G11B10_Float = 26,
		/// <summary>
		/// The R8G8B8A8 buffer format.
		/// </summary>
		R8G8B8A8 = 27,
		/// <summary>
		/// The R8G8B8A8_UIntNormal buffer format.
		/// </summary>
		R8G8B8A8_UIntNormal = 28,
		/// <summary>
		/// The R8G8B8A8_UIntNormal_SRGB buffer format.
		/// </summary>
		R8G8B8A8_UIntNormal_SRGB = 29,
		/// <summary>
		/// The R8G8B8A8_UInt buffer format.
		/// </summary>
		R8G8B8A8_UInt = 30,
		/// <summary>
		/// The R8G8B8A8_IntNormal buffer format.
		/// </summary>
		R8G8B8A8_IntNormal = 31,
		/// <summary>
		/// The R8G8B8A8_Int buffer format.
		/// </summary>
		R8G8B8A8_Int = 32,
		/// <summary>
		/// The R16G16 buffer format.
		/// </summary>
		R16G16 = 33,
		/// <summary>
		/// The R16G16_Float buffer format.
		/// </summary>
		R16G16_Float = 34,
		/// <summary>
		/// The R16G16_UIntNormal buffer format.
		/// </summary>
		R16G16_UIntNormal = 35,
		/// <summary>
		/// The R16G16_UInt buffer format.
		/// </summary>
		R16G16_UInt = 36,
		/// <summary>
		/// The R16G16_IntNormal buffer format.
		/// </summary>
		R16G16_IntNormal = 37,
		/// <summary>
		/// The R16G16_Int buffer format.
		/// </summary>
		R16G16_Int = 38,
		/// <summary>
		/// The R32 buffer format.
		/// </summary>
		R32 = 39,
		/// <summary>
		/// The D32_Float buffer format.
		/// </summary>
		D32_Float = 40,
		/// <summary>
		/// The R32_Float buffer format.
		/// </summary>
		R32_Float = 41,
		/// <summary>
		/// The R32_UInt buffer format.
		/// </summary>
		R32_UInt = 42,
		/// <summary>
		/// The R32_Int buffer format.
		/// </summary>
		R32_Int = 43,
		/// <summary>
		/// The R24G8 buffer format.
		/// </summary>
		R24G8 = 44,
		/// <summary>
		/// The D24_UIntNormal_S8_UInt buffer format.
		/// </summary>
		D24_UIntNormal_S8_UInt = 45,
		/// <summary>
		/// The R24_UIntNormal_X8 buffer format.
		/// </summary>
		R24_UIntNormal_X8 = 46,
		/// <summary>
		/// The X24_G8_UInt buffer format.
		/// </summary>
		X24_G8_UInt = 47,
		/// <summary>
		/// The R8G8 buffer format.
		/// </summary>
		R8G8 = 48,
		/// <summary>
		/// The R8G8_UIntNormal buffer format.
		/// </summary>
		R8G8_UIntNormal = 49,
		/// <summary>
		/// The R8G8_UInt buffer format.
		/// </summary>
		R8G8_UInt = 50,
		/// <summary>
		/// The R8G8_IntNormal buffer format.
		/// </summary>
		R8G8_IntNormal = 51,
		/// <summary>
		/// The R8G8_Int buffer format.
		/// </summary>
		R8G8_Int = 52,
		/// <summary>
		/// The R16 buffer format.
		/// </summary>
		R16 = 53,
		/// <summary>
		/// The R16_Float buffer format.
		/// </summary>
		R16_Float = 54,
		/// <summary>
		/// The D16_UIntNormal buffer format.
		/// </summary>
		D16_UIntNormal = 55,
		/// <summary>
		/// The R16_UIntNormal buffer format.
		/// </summary>
		R16_UIntNormal = 56,
		/// <summary>
		/// The R16_UInt buffer format.
		/// </summary>
		R16_UInt = 57,
		/// <summary>
		/// The R16_IntNormal buffer format.
		/// </summary>
		R16_IntNormal = 58,
		/// <summary>
		/// The R16_Int buffer format.
		/// </summary>
		R16_Int = 59,
		/// <summary>
		/// The R8 buffer format.
		/// </summary>
		R8 = 60,
		/// <summary>
		/// The R8_UIntNormal buffer format.
		/// </summary>
		R8_UIntNormal = 61,
		/// <summary>
		/// The R8_UInt buffer format.
		/// </summary>
		R8_UInt = 62,
		/// <summary>
		/// The R8_IntNormal buffer format.
		/// </summary>
		R8_IntNormal = 63,
		/// <summary>
		/// The R8_Int buffer format.
		/// </summary>
		R8_Int = 64,
		/// <summary>
		/// The A8_UIntNormal buffer format.
		/// </summary>
		A8_UIntNormal = 65,
		/// <summary>
		/// The R1_UIntNormal buffer format.
		/// </summary>
		R1_UIntNormal = 66,
		/// <summary>
		/// The R9G9B9E5_SharedExp buffer format.
		/// </summary>
		R9G9B9E5_SharedExp = 67,
		/// <summary>
		/// The R8G8_B8G8_UIntNormal buffer format.
		/// </summary>
		R8G8_B8G8_UIntNormal = 68,
		/// <summary>
		/// The G8R8_G8B8_UIntNormal buffer format.
		/// </summary>
		G8R8_G8B8_UIntNormal = 69,
		/// <summary>
		/// The BC1 buffer format.
		/// </summary>
		BC1 = 70,
		/// <summary>
		/// The BC1_UIntNormal buffer format.
		/// </summary>
		BC1_UIntNormal = 71,
		/// <summary>
		/// The BC1_UIntNormal_SRGB buffer format.
		/// </summary>
		BC1_UIntNormal_SRGB = 72,
		/// <summary>
		/// The BC2 buffer format.
		/// </summary>
		BC2 = 73,
		/// <summary>
		/// The BC2_UIntNormal buffer format.
		/// </summary>
		BC2_UIntNormal = 74,
		/// <summary>
		/// The BC2_UIntNormal_SRGB buffer format.
		/// </summary>
		BC2_UIntNormal_SRGB = 75,
		/// <summary>
		/// The BC3 buffer format.
		/// </summary>
		BC3 = 76,
		/// <summary>
		/// The BC3_UIntNormal buffer format.
		/// </summary>
		BC3_UIntNormal = 77,
		/// <summary>
		/// The BC3_UIntNormal_SRGB buffer format.
		/// </summary>
		BC3_UIntNormal_SRGB = 78,
		/// <summary>
		/// The BC4 buffer format.
		/// </summary>
		BC4 = 79,
		/// <summary>
		/// The BC4_UIntNormal buffer format.
		/// </summary>
		BC4_UIntNormal = 80,
		/// <summary>
		/// The BC4_IntNormal buffer format.
		/// </summary>
		BC4_IntNormal = 81,
		/// <summary>
		/// The BC5 buffer format.
		/// </summary>
		BC5 = 82,
		/// <summary>
		/// The BC5_UIntNormal buffer format.
		/// </summary>
		BC5_UIntNormal = 83,
		/// <summary>
		/// The BC5_IntNormal buffer format.
		/// </summary>
		BC5_IntNormal = 84,
		/// <summary>
		/// The B5G6R5_UIntNormal buffer format.
		/// </summary>
		B5G6R5_UIntNormal = 85,
		/// <summary>
		/// The B5G5R5A1_UIntNormal buffer format.
		/// </summary>
		B5G5R5A1_UIntNormal = 86,
		/// <summary>
		/// The B8G8R8A8_UIntNormal buffer format.
		/// </summary>
		B8G8R8A8_UIntNormal = 87,
		/// <summary>
		/// The B8G8R8X8_UIntNormal buffer format.
		/// </summary>
		B8G8R8X8_UIntNormal = 88,
		/// <summary>
		/// The R10G10B10_XR_Bias_A2_UIntNormal buffer format.
		/// </summary>
		R10G10B10_XR_Bias_A2_UIntNormal = 89,
		/// <summary>
		/// The B8G8R8A8 buffer format.
		/// </summary>
		B8G8R8A8 = 90,
		/// <summary>
		/// The B8G8R8A8_UIntNormal_SRGB buffer format.
		/// </summary>
		B8G8R8A8_UIntNormal_SRGB = 91,
		/// <summary>
		/// The B8G8R8X8 buffer format.
		/// </summary>
		B8G8R8X8 = 92,
		/// <summary>
		/// The B8G8R8X8_UIntNormal_SRGB buffer format.
		/// </summary>
		B8G8R8X8_UIntNormal_SRGB = 93,
		/// <summary>
		/// The BC6 buffer format.
		/// </summary>
		BC6 = 94,
		/// <summary>
		/// The BC6_UFloat16 buffer format.
		/// </summary>
		BC6_UFloat16 = 95,
		/// <summary>
		/// The BC6_SFloat16 buffer format.
		/// </summary>
		BC6_SFloat16 = 96,
		/// <summary>
		/// The BC7 buffer format.
		/// </summary>
		BC7 = 97,
		/// <summary>
		/// The BC7_UIntNormal buffer format.
		/// </summary>
		BC7_UIntNormal = 98,
		/// <summary>
		/// The BC7_UIntNormal_SRGB buffer format.
		/// </summary>
		BC7_UIntNormal_SRGB = 99,
		// These values are custom to Gorgon and are used to help older APIs handle the formats.
		/// <summary>
		/// 32 bit back buffer format with 2 bit alpha.
		/// </summary>
		BackBuffer_A2R10G10B10 = 100,
		/// <summary>
		/// 32 bit back buffer format.
		/// </summary>
		BackBuffer_A8R8G8B8 = 101,
		/// <summary>
		/// 32 bit back buffer format with no alpha.
		/// </summary>
		BackBuffer_X8R8G8B8 = 102,
		/// <summary>
		/// 24 bit depth buffer.
		/// </summary>
		Depth_D24_X8 = 103,
		/// <summary>
		/// 24 bit depth buffer, 4 bit stencil.
		/// </summary>
		Depth_D24_X4S4 = 104,
		/// <summary>
		/// 24 bit depth buffer, 8 bit stencil.
		/// </summary>
		Depth_D24_S8 = 105,
		/// <summary>
		/// 32 bit depth buffer.
		/// </summary>
		Depth_D32 = 106,
		/// <summary>
		/// 32 bit floating point depth buffer format.
		/// </summary>
		Depth_D32Float_Lockable = 107,
		/// <summary>
		/// 16 bit index buffer.
		/// </summary>
		Index_16 = 108,
		/// <summary>
		/// 32 bit index buffer.
		/// </summary>
		Index_32 = 109,
		/// <summary>
		/// Vertex buffer data.
		/// </summary>
		Vertex = 110,
		/// <summary>
		/// DXT1 Compressed format.
		/// </summary>
		DXT1 = 111,
		/// <summary>
		/// DXT2 Compressed format.
		/// </summary>
		DXT2 = 112,
		/// <summary>
		/// DXT3 Compressed format.
		/// </summary>
		DXT3 = 113,
		/// <summary>
		/// DXT4 Compressed format.
		/// </summary>
		DXT4 = 114,
		/// <summary>
		/// DXT5 Compressed format.
		/// </summary>
		DXT5 = 115,
		/// <summary>
		/// 16 bit floating point red channel.
		/// </summary>
		Float_R16 = 116,
		/// <summary>
		/// 32 bit floating point with 16 bits for the red and green channel.
		/// </summary>
		Float_G16R16 = 117,
		/// <summary>
		/// 64 bit floating point with 16 bits for red, green, blue and alpha.
		/// </summary>
		Float_A16B16G16R16 = 118,
		/// <summary>
		/// FourCC multi element texture (uncompressed).
		/// </summary>
		FourCC_Multi2_ARGB8 = 119,
		/// <summary>
		/// FourCC 16 bit packed format.
		/// </summary>
		FourCC_G8R8_G8B8 = 120,
		/// <summary>
		/// FourCC 16 bit packed format.
		/// </summary>
		FourCC_R8G8_B8G8 = 121,
		/// <summary>
		/// FourCC UYVY format.
		/// </summary>
		FourCC_UYVY = 122,
		/// <summary>
		/// FourCC YUV2 format.
		/// </summary>
		FourCC_YUV2 = 123,
		/// <summary>
		/// 32 bit floating point in red channel.
		/// </summary>
		Float_R32 = 124,
		/// <summary>
		/// 64 bit floating point with 32 bit green and red channels.
		/// </summary>
		Float_G32R32 = 125,
		/// <summary>
		/// 128 bit floating point with 32 bit red, green, blue and alpha channels.
		/// </summary>
		Float_A32B32G32R32 = 126,
		/// <summary>
		/// 16 bit bump map with luminance.
		/// </summary>
		Bump_L6V5U5 = 127,
		/// <summary>
		/// 32 bit bump map with luminance.
		/// </summary>
		Bump_X8L8V8U8 = 128,
		/// <summary>
		/// 32 bit bump map with 2 bit alpha.
		/// </summary>
		Bump_A2W10V10U10 = 129,
		/// <summary>
		/// 16 bit bump map.
		/// </summary>
		Bump_V8U8 = 130,
		/// <summary>
		/// 32 bit bump map.
		/// </summary>
		Bump_Q8W8V8Y8 = 131,
		/// <summary>
		/// 32 bit bump map with 16 bit components.
		/// </summary>
		Bump_V16U16 = 132,
		/// <summary>
		/// 64 bit bump map.
		/// </summary>
		Bump_Q16W16V16U16 = 133,
		/// <summary>
		/// 16 bit normal compression.
		/// </summary>
		Normal_CxV8U8 = 134,

	}
}
