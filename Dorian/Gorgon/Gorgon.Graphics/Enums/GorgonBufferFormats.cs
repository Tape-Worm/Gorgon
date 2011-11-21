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
using SharpDX.DXGI;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Various buffer formats supported for textures, rendertargets, swap chains and display modes.
	/// </summary>
	public enum GorgonBufferFormat
	{
		/// <summary>
		/// Unknown format.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// The R32G32B32A32 format.
		/// </summary>
		R32G32B32A32 = 1,
		/// <summary>
		/// The R32G32B32A32_Float format.
		/// </summary>
		R32G32B32A32_Float = 2,
		/// <summary>
		/// The R32G32B32A32_UInt format.
		/// </summary>
		R32G32B32A32_UInt = 3,
		/// <summary>
		/// The R32G32B32A32_Int format.
		/// </summary>
		R32G32B32A32_Int = 4,
		/// <summary>
		/// The R32G32B32 format.
		/// </summary>
		R32G32B32 = 5,
		/// <summary>
		/// The R32G32B32_Float format.
		/// </summary>
		R32G32B32_Float = 6,
		/// <summary>
		/// The R32G32B32_UInt format.
		/// </summary>
		R32G32B32_UInt = 7,
		/// <summary>
		/// The R32G32B32_Int format.
		/// </summary>
		R32G32B32_Int = 8,
		/// <summary>
		/// The R16G16B16A16 format.
		/// </summary>
		R16G16B16A16 = 9,
		/// <summary>
		/// The R16G16B16A16_Float format.
		/// </summary>
		R16G16B16A16_Float = 10,
		/// <summary>
		/// The R16G16B16A16_UIntNormal format.
		/// </summary>
		R16G16B16A16_UIntNormal = 11,
		/// <summary>
		/// The R16G16B16A16_UInt format.
		/// </summary>
		R16G16B16A16_UInt = 12,
		/// <summary>
		/// The R16G16B16A16_IntNormal format.
		/// </summary>
		R16G16B16A16_IntNormal = 13,
		/// <summary>
		/// The R16G16B16A16_Int format.
		/// </summary>
		R16G16B16A16_Int = 14,
		/// <summary>
		/// The R32G32 format.
		/// </summary>
		R32G32 = 15,
		/// <summary>
		/// The R32G32_Float format.
		/// </summary>
		R32G32_Float = 16,
		/// <summary>
		/// The R32G32_UInt format.
		/// </summary>
		R32G32_UInt = 17,
		/// <summary>
		/// The R32G32_Int format.
		/// </summary>
		R32G32_Int = 18,
		/// <summary>
		/// The R32G8X24 format.
		/// </summary>
		R32G8X24 = 19,
		/// <summary>
		/// The D32_Float_S8X24_UInt format.
		/// </summary>
		D32_Float_S8X24_UInt = 20,
		/// <summary>
		/// The R32_Float_X8X24 format.
		/// </summary>
		R32_Float_X8X24 = 21,
		/// <summary>
		/// The X32_G8X24_UInt format.
		/// </summary>
		X32_G8X24_UInt = 22,
		/// <summary>
		/// The R10G10B10A2 format.
		/// </summary>
		R10G10B10A2 = 23,
		/// <summary>
		/// The R10G10B10A2_UIntNormal format.
		/// </summary>
		R10G10B10A2_UIntNormal = 24,
		/// <summary>
		/// The R10G10B10A2_UInt format.
		/// </summary>
		R10G10B10A2_UInt = 25,
		/// <summary>
		/// The R11G11B10_Float format.
		/// </summary>
		R11G11B10_Float = 26,
		/// <summary>
		/// The R8G8B8A8 format.
		/// </summary>
		R8G8B8A8 = 27,
		/// <summary>
		/// The R8G8B8A8_UIntNormal format.
		/// </summary>
		R8G8B8A8_UIntNormal = 28,
		/// <summary>
		/// The R8G8B8A8_UIntNormal_sRGB format.
		/// </summary>
		R8G8B8A8_UIntNormal_sRGB = 29,
		/// <summary>
		/// The R8G8B8A8_UInt format.
		/// </summary>
		R8G8B8A8_UInt = 30,
		/// <summary>
		/// The R8G8B8A8_IntNormal format.
		/// </summary>
		R8G8B8A8_IntNormal = 31,
		/// <summary>
		/// The R8G8B8A8_Int format.
		/// </summary>
		R8G8B8A8_Int = 32,
		/// <summary>
		/// The R16G16 format.
		/// </summary>
		R16G16 = 33,
		/// <summary>
		/// The R16G16_Float format.
		/// </summary>
		R16G16_Float = 34,
		/// <summary>
		/// The R16G16_UIntNormal format.
		/// </summary>
		R16G16_UIntNormal = 35,
		/// <summary>
		/// The R16G16_UInt format.
		/// </summary>
		R16G16_UInt = 36,
		/// <summary>
		/// The R16G16_IntNormal format.
		/// </summary>
		R16G16_IntNormal = 37,
		/// <summary>
		/// The R16G16_Int format.
		/// </summary>
		R16G16_Int = 38,
		/// <summary>
		/// The R32 format.
		/// </summary>
		R32 = 39,
		/// <summary>
		/// The D32_Float format.
		/// </summary>
		D32_Float = 40,
		/// <summary>
		/// The R32_Float format.
		/// </summary>
		R32_Float = 41,
		/// <summary>
		/// The R32_UInt format.
		/// </summary>
		R32_UInt = 42,
		/// <summary>
		/// The R32_Int format.
		/// </summary>
		R32_Int = 43,
		/// <summary>
		/// The R24G8 format.
		/// </summary>
		R24G8 = 44,
		/// <summary>
		/// The D24_UIntNormal_S8_UInt format.
		/// </summary>
		D24_UIntNormal_S8_UInt = 45,
		/// <summary>
		/// The R24_UIntNormal_X8 format.
		/// </summary>
		R24_UIntNormal_X8 = 46,
		/// <summary>
		/// The X24_G8_UInt format.
		/// </summary>
		X24_G8_UInt = 47,
		/// <summary>
		/// The R8G8 format.
		/// </summary>
		R8G8 = 48,
		/// <summary>
		/// The R8G8_UIntNormal format.
		/// </summary>
		R8G8_UIntNormal = 49,
		/// <summary>
		/// The R8G8_UInt format.
		/// </summary>
		R8G8_UInt = 50,
		/// <summary>
		/// The R8G8_IntNormal format.
		/// </summary>
		R8G8_IntNormal = 51,
		/// <summary>
		/// The R8G8_Int format.
		/// </summary>
		R8G8_Int = 52,
		/// <summary>
		/// The R16 format.
		/// </summary>
		R16 = 53,
		/// <summary>
		/// The R16_Float format.
		/// </summary>
		R16_Float = 54,
		/// <summary>
		/// The D16_UIntNormal format.
		/// </summary>
		D16_UIntNormal = 55,
		/// <summary>
		/// The R16_UIntNormal format.
		/// </summary>
		R16_UIntNormal = 56,
		/// <summary>
		/// The R16_UInt format.
		/// </summary>
		R16_UInt = 57,
		/// <summary>
		/// The R16_IntNormal format.
		/// </summary>
		R16_IntNormal = 58,
		/// <summary>
		/// The R16_Int format.
		/// </summary>
		R16_Int = 59,
		/// <summary>
		/// The R8 format.
		/// </summary>
		R8 = 60,
		/// <summary>
		/// The R8_UIntNormal format.
		/// </summary>
		R8_UIntNormal = 61,
		/// <summary>
		/// The R8_UInt format.
		/// </summary>
		R8_UInt = 62,
		/// <summary>
		/// The R8_IntNormal format.
		/// </summary>
		R8_IntNormal = 63,
		/// <summary>
		/// The R8_Int format.
		/// </summary>
		R8_Int = 64,
		/// <summary>
		/// The A8_UIntNormal format.
		/// </summary>
		A8_UIntNormal = 65,
		/// <summary>
		/// The R1_UIntNormal format.
		/// </summary>
		R1_UIntNormal = 66,
		/// <summary>
		/// The R9G9B9E5_SharedExp format.
		/// </summary>
		R9G9B9E5_SharedExp = 67,
		/// <summary>
		/// The R8G8_B8G8_UIntNormal format.
		/// </summary>
		R8G8_B8G8_UIntNormal = 68,
		/// <summary>
		/// The G8R8_G8B8_UIntNormal format.
		/// </summary>
		G8R8_G8B8_UIntNormal = 69,
		/// <summary>
		/// The BC1 format.
		/// </summary>
		BC1 = 70,
		/// <summary>
		/// The BC1_UIntNormal format.
		/// </summary>
		BC1_UIntNormal = 71,
		/// <summary>
		/// The BC1_UIntNormal_sRGB format.
		/// </summary>
		BC1_UIntNormal_sRGB = 72,
		/// <summary>
		/// The BC2 format.
		/// </summary>
		BC2 = 73,
		/// <summary>
		/// The BC2_UIntNormal format.
		/// </summary>
		BC2_UIntNormal = 74,
		/// <summary>
		/// The BC2_UIntNormal_sRGB format.
		/// </summary>
		BC2_UIntNormal_sRGB = 75,
		/// <summary>
		/// The BC3 format.
		/// </summary>
		BC3 = 76,
		/// <summary>
		/// The BC3_UIntNormal format.
		/// </summary>
		BC3_UIntNormal = 77,
		/// <summary>
		/// The BC3_UIntNormal_sRGB format.
		/// </summary>
		BC3_UIntNormal_sRGB = 78,
		/// <summary>
		/// The BC4 format.
		/// </summary>
		BC4 = 79,
		/// <summary>
		/// The BC4_UIntNormal format.
		/// </summary>
		BC4_UIntNormal = 80,
		/// <summary>
		/// The BC4_IntNormal format.
		/// </summary>
		BC4_IntNormal = 81,
		/// <summary>
		/// The BC5 format.
		/// </summary>
		BC5 = 82,
		/// <summary>
		/// The BC5_UIntNormal format.
		/// </summary>
		BC5_UIntNormal = 83,
		/// <summary>
		/// The BC5_IntNormal format.
		/// </summary>
		BC5_IntNormal = 84,
		/// <summary>
		/// The B5G6R5_UIntNormal format.
		/// </summary>
		B5G6R5_UIntNormal = 85,
		/// <summary>
		/// The B5G5R5A1_UIntNormal format.
		/// </summary>
		B5G5R5A1_UIntNormal = 86,
		/// <summary>
		/// The B8G8R8A8_UIntNormal format.
		/// </summary>
		B8G8R8A8_UIntNormal = 87,
		/// <summary>
		/// The B8G8R8X8_UIntNormal format.
		/// </summary>
		B8G8R8X8_UIntNormal = 88,
		/// <summary>
		/// The R10G10B10_XR_BIAS_A2_UIntNormal format.
		/// </summary>
		R10G10B10_XR_BIAS_A2_UIntNormal = 89,
		/// <summary>
		/// The B8G8R8A8 format.
		/// </summary>
		B8G8R8A8 = 90,
		/// <summary>
		/// The B8G8R8A8_UIntNormal_sRGB format.
		/// </summary>
		B8G8R8A8_UIntNormal_sRGB = 91,
		/// <summary>
		/// The B8G8R8X8 format.
		/// </summary>
		B8G8R8X8 = 92,
		/// <summary>
		/// The B8G8R8X8_UIntNormal_sRGB format.
		/// </summary>
		B8G8R8X8_UIntNormal_sRGB = 93,
		/// <summary>
		/// The BC6H format.
		/// </summary>
		BC6H = 94,
		/// <summary>
		/// The BC6H_UF16 format.
		/// </summary>
		BC6H_UF16 = 95,
		/// <summary>
		/// The BC6H_SF16 format.
		/// </summary>
		BC6H_SF16 = 96,
		/// <summary>
		/// The BC7 format.
		/// </summary>
		BC7 = 97,
		/// <summary>
		/// The BC7_UIntNormal format.
		/// </summary>
		BC7_UIntNormal = 98,
		/// <summary>
		/// The BC7_UIntNormal_sRGB format.
		/// </summary>
		BC7_UIntNormal_sRGB = 99
	}
}
