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
	/// Depth buffer formats.
	/// </summary>
	public enum GorgonDepthBufferFormat
	{
		/// <summary>
		/// Unknown format.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// 16 bit depth buffer with 15 bit depth component and 1 bit stencil component.
		/// </summary>
		D15_UIntNormal_S1_UInt = 1,
		/// <summary>
		/// 16 bit depth buffer.
		/// </summary>
		D16_UIntNormal = 2,
		/// <summary>
		/// 16 bit depth buffer, lockable.
		/// </summary>
		D16_UIntNormal_Lockable = 3,
		/// <summary>
		/// 32 bit depth buffer, 24 bit depth component.
		/// </summary>
		D24_UIntNormal_X8 = 4,
		/// <summary>
		/// 32 bit depth buffer, 24 bit depth component and 4 bit stencil component.
		/// </summary>
		D24_UIntNormal_X4S4_UInt = 5,
		/// <summary>
		/// 32 bit depth buffer, 24 bit depth component and 8 bit stencil component.
		/// </summary>
		D24_UIntNormal_S8_UInt = 6,
		/// <summary>
		/// 32 bit depth buffer, 24 bit floating point component, and 8 bit stencil component.
		/// </summary>
		D24_Float_S8_UInt = 7,
		/// <summary>
		/// 32 bit depth buffer.
		/// </summary>
		D32_UIntNormal = 8,
		/// <summary>
		/// 32 bit floating point depth buffer, lockable.
		/// </summary>
		D32_Float_Lockable = 9,
		/// <summary>
		/// 32 bit floating point depth buffer, 8 bit stencil component.
		/// </summary>
		S8_Float_X24 = 10
	}

	/// <summary>
	/// Back buffer formats.
	/// </summary>
	public enum GorgonBackBufferFormat
	{
		/// <summary>
		/// Unknown back buffer format.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// 32 bit unsigned integer with 10 bit R, G, B components and a 2 bit alpha component.
		/// </summary>
		A2R10G10B10_UIntNormal = 1,
		/// <summary>
		/// 32 bit unsigned integer with 8 bit R, G, B components and an 8 bit alpha component.
		/// </summary>
		A8R8G8B8_UIntNormal = 2,
		/// <summary>
		/// 32 bit unsigned integer with 8 bit R, G and B components.
		/// </summary>
		X8_R8G8B8_UIntNormal = 3,
		/// <summary>
		/// 32 bit unsigned integer with 8 bit R, G, B and an 8 bit alpha component in sRGB.
		/// </summary>
		A8R8G8B8_UIntNormal_sRGB = 4
	}

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
		/// The R8G8B8A8_UIntNormal_sRGB buffer format.
		/// </summary>
		R8G8B8A8_UIntNormal_sRGB = 29,
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
		/// The BC1_UIntNormal_sRGB buffer format.
		/// </summary>
		BC1_UIntNormal_sRGB = 72,
		/// <summary>
		/// The BC2 buffer format.
		/// </summary>
		BC2 = 73,
		/// <summary>
		/// The BC2_UIntNormal buffer format.
		/// </summary>
		BC2_UIntNormal = 74,
		/// <summary>
		/// The BC2_UIntNormal_sRGB buffer format.
		/// </summary>
		BC2_UIntNormal_sRGB = 75,
		/// <summary>
		/// The BC3 buffer format.
		/// </summary>
		BC3 = 76,
		/// <summary>
		/// The BC3_UIntNormal buffer format.
		/// </summary>
		BC3_UIntNormal = 77,
		/// <summary>
		/// The BC3_UIntNormal_sRGB buffer format.
		/// </summary>
		BC3_UIntNormal_sRGB = 78,
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
		/// The B8G8R8A8_UIntNormal_sRGB buffer format.
		/// </summary>
		B8G8R8A8_UIntNormal_sRGB = 91,
		/// <summary>
		/// The B8G8R8X8 buffer format.
		/// </summary>
		B8G8R8X8 = 92,
		/// <summary>
		/// The B8G8R8X8_UIntNormal_sRGB buffer format.
		/// </summary>
		B8G8R8X8_UIntNormal_sRGB = 93,
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
		/// The BC7_UIntNormal_sRGB buffer format.
		/// </summary>
		BC7_UIntNormal_sRGB = 99,
		// This mode may not be supported by all APIs
		/// <summary>
		/// The X8_R8G8B8_UIntNormal buffer format.
		/// </summary>
		X8_R8G8B8_UIntNormal = 100
	}
}
