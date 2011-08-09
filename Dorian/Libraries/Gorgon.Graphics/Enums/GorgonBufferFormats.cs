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
	/// A listing of backbuffer formats for swap chains and device windows.
	/// </summary>
	public enum GorgonDisplayFormat
	{
		/// <summary>
		/// A display format for 10 bit R, G, and B and 2 bit Alpha.
		/// </summary>
		A2R10G10B10 = 0,
		/// <summary>
		/// A display format for 8 bit R, G, and B and 8 bit Alpha.
		/// </summary>
		A8R8G8B8 = 1,
		/// <summary>
		/// A display format for 8 bit R, G, and B.
		/// </summary>
		X8R8G8B8 = 2,
		/// <summary>
		/// Unknown format.
		/// </summary>
		Unknown = 3
	}

	/// <summary>
	/// A listing of depth buffer formats.
	/// </summary>
	public enum GorgonDepthBufferFormat
	{
		/// <summary>
		/// A 15 bit depth buffer and 1 bit stencil format.
		/// </summary>
		D15S1 = 0,
		/// <summary>
		/// A 16 bit depth buffer format (lockable).
		/// </summary>
		D16_Lockable = 1,
		/// <summary>
		/// A 16 bit depth buffer format.
		/// </summary>
		D16 = 2,
		/// <summary>
		/// A 24 bit depth buffer format.
		/// </summary>
		D24X8 = 3,
		/// <summary>
		/// A 24 bit depth and 4 bit stencil buffer format.
		/// </summary>
		D24X4S4 = 4,
		/// <summary>
		/// A 24 bit depth and 8 bit stencil buffer format.
		/// </summary>
		D24S8 = 5,
		/// <summary>
		/// A 24 bit floating point depth and 8 bit stencil buffer format.
		/// </summary>
		D24_Float_S8 = 6,
		/// <summary>
		/// A 32 bit floating point depth buffer format (lockable).
		/// </summary>
		D32_Float_Lockable = 7,
		/// <summary>
		/// A 32 bit depth buffer format.
		/// </summary>
		D32 = 8,
		/// <summary>
		/// An 8 bit stencil format.
		/// </summary>
		X24S8 = 9,
		/// <summary>
		/// Unknown depth buffer format.
		/// </summary>
		Unknown = 10
	}

	/// <summary>
	/// A listing of buffer formats.
	/// </summary>
	public enum GorgonBufferFormat
	{
		/// <summary>
		/// Unknown format.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// 4 component 128 bit typeless format.
		/// </summary>
		R32G32B32A32_NoType = 1,
		/// <summary>
		/// 4 component 128 bit floating point format.
		/// </summary>
		R32G32B32A32_Float = 2,
		/// <summary>
		/// 4 component 128 bit unsigned integer format.
		/// </summary>
		R32G32B32A32_UInt = 3,
		/// <summary>
		/// 4 component 128 bit signed integer format.
		/// </summary>
		R32G32B32A32_Int = 4,
		/// <summary>
		/// 3 component 96 bit typeless format.
		/// </summary>
		R32G32B32_Typeless = 5,
		/// <summary>
		/// 3 component 96 bit floating point format.
		/// </summary>
		R32G32B32_Float = 6,
		/// <summary>
		/// 3 component 96 bit unsigned integer format.
		/// </summary>
		R32G32B32_UInt = 7,
		/// <summary>
		/// 3 component 96 bit signed integer format.
		/// </summary>
		R32G32B32_Int = 8,
		/// <summary>
		/// 4 component 64 bit typeless format.
		/// </summary>
		R16G16B16A16_Typeless = 9,
		/// <summary>
		/// 4 component 64 bit floating point format.
		/// </summary>
		R16G16B16A16_Float = 10,
		/// <summary>
		/// 4 component 64 bit unsigned integer format.
		/// </summary>
		R16G16B16A16_UIntNorm = 11,
		/// <summary>
		/// 4 component 64 bit unsigned integer format.
		/// </summary>
		R16G16B16A16_UInt = 12,
		/// <summary>
		/// 4 component 64 bit signed integer format.
		/// </summary>
		R16G16B16A16_IntNorm = 13,
		/// <summary>
		/// 4 component 64 bit signed integer format.
		/// </summary>
		R16G16B16A16_Int = 14,
		/// <summary>
		/// 2 component 64 bit typeless format.
		/// </summary>
		R32G32_Typeless = 15,
		/// <summary>
		/// 2 component 64 bit floating point format.
		/// </summary>
		R32G32_Float = 16,
		/// <summary>
		/// 2 component 64 bit unsigned integer format.
		/// </summary>
		R32G32_UInt = 17,
		/// <summary>
		/// 2 component 64 bit signed integer format.
		/// </summary>
		R32G32_Int = 18,
		/// <summary>
		/// 3 component 64 bit typeless format.
		/// </summary>
		R32G8X24_Typeless = 19,
		/// <summary>
		/// 1 32 bit floating point component, 2 unsigned integer components (32 bits total).
		/// </summary>
		D32_Float_S8X24_UInt = 20,
		/// <summary>
		/// 1 32 bit floating point component, 2 typeless components (32 bits total).
		/// </summary>
		R32_Float_X8X24_Typeless = 21,
		/// <summary>
		/// 1 32 bit typeless component, 2 unsigned integer components (32 bits total).
		/// </summary>
		X32_Typeless_G8X24_UInt = 22,
		/// <summary>
		/// 4 component 32 bit typeless format.
		/// </summary>
		R10G10B10A2_Typeless = 23,
		/// <summary>
		/// 4 component 32 bit unsigned integer format.
		/// </summary>
		R10G10B10A2_UIntNorm = 24,
		/// <summary>
		/// 4 component 32 bit unsigned integer format.
		/// </summary>
		R10G10B10A2_UInt = 25,
		/// <summary>
		/// 3 component 32 bit floating point format.
		/// </summary>		
		/// <remarks>Three partial-precision floating-point numbers encodeded into a single 32-bit value (a variant of s10e5). There are no sign bits, and there is a 5-bit biased (15) exponent for each channel, 6-bit mantissa for R and G, and a 5-bit mantissa for B.</remarks>
		R11G11B10_Float = 26,
		/// <summary>
		/// 4 component 32 bit typeless format.
		/// </summary>
		R8G8B8A8_Typeless = 27,
		/// <summary>
		/// 4 component 32 bit unsigned integer format.
		/// </summary>
		R8G8B8A8_UIntNorm = 28,
		/// <summary>
		/// 4 component 32 bit unsigned normalized integer sRGB format 
		/// </summary>
		R8G8B8A8_UIntNorm_sRGB = 29,
		/// <summary>
		/// 4 component 32 bit unsigned integer format.
		/// </summary>
		R8G8B8A8_UInt = 30,
		/// <summary>
		/// 4 component 32 bit signed normalized integer format.
		/// </summary>
		R8G8B8A8_IntNorm = 31,
		/// <summary>
		/// 4 component 32 bit signed integer format.
		/// </summary>
		R8G8B8A8_Int = 32,
		/// <summary>
		/// 2 component 32 bit typeless format.
		/// </summary>
		R16G16_Typeless = 33,
		/// <summary>
		/// 2 component 32 bit floating point format.
		/// </summary>
		R16G16_Float = 34,
		/// <summary>
		/// 2 component 32 bit unsigned normalized integer format.
		/// </summary>
		R16G16_UIntNorm = 35,
		/// <summary>
		/// 2 component 32 bit unsigned integer format.
		/// </summary>
		R16G16_UInt = 36,
		/// <summary>
		/// 2 component 32 bit signed normalized integer format.
		/// </summary>
		R16G16_IntNorm = 37,
		/// <summary>
		/// 2 component 32 bit signed integer format.
		/// </summary>
		R16G16_Int = 38,
		/// <summary>
		/// 1 component 32 bit typeless format.
		/// </summary>
		R32_Typeless = 39,
		/// <summary>
		/// 1 component 32 bit floating point format.
		/// </summary>
		D32_Float = 40,
		/// <summary>
		/// 1 component 32 bit floating point format.
		/// </summary>
		R32_Float = 41,
		/// <summary>
		/// 1 component 32 bit unsigned integer format.
		/// </summary>
		R32_UInt = 42,
		/// <summary>
		/// 1 component 32 bit signed integer format.
		/// </summary>
		R32_Int = 43,
		/// <summary>
		/// 2 component 32 bit typeless format.
		/// </summary>
		R24G8_Typeless = 44,
		/// <summary>
		/// 32 bit z-buffer format with 24 bits of normalized unsigned integer and 8 bit unsigned integer for the stencil buffer.
		/// </summary>
		D24_UIntNorm_S8_UInt = 45,
		/// <summary>
		/// 32 bit format with 24 bits of normalized unsigned integer and 8 bit typeless data.
		/// </summary>
		R24_UIntNorm_X8_Typeless = 46,
		/// <summary>
		/// 32 bit format with 24 bits of typeless data and 8 bits of unsigned integer data.
		/// </summary>
		X24_Typeless_G8_UInt = 47,
		/// <summary>
		/// 2 component 16 bit typeless format.
		/// </summary>
		R8G8_Typeless = 48,
		/// <summary>
		/// 2 component 16 bit normalized unsigned integer format.
		/// </summary>
		R8G8_UIntNorm = 49,
		/// <summary>
		/// 2 component 16 bit unsigned integer format.
		/// </summary>
		R8G8_UInt = 50,
		/// <summary>
		/// 2 component 16 bit normalized signed integer format.
		/// </summary>
		R8G8_IntNorm = 51,
		/// <summary>
		/// 2 component 16 bit signed integer format.
		/// </summary>
		R8G8_Int = 52,
		/// <summary>
		/// 1 component 16 bit typeless format.
		/// </summary>
		R16_Typeless = 53,
		/// <summary>
		/// 1 component 16 bit floating point format.
		/// </summary>
		R16_Float = 54,
		/// <summary>
		/// 1 component 16 bit normalized unsigned integer format.
		/// </summary>
		D16_UIntNorm = 55,
		/// <summary>
		/// 1 component 16 bit normalized unsigned integer format.
		/// </summary>
		R16_UIntNorm = 56,
		/// <summary>
		/// 1 component 16 bit unsigned integer format.
		/// </summary>
		R16_UInt = 57,
		/// <summary>
		/// 1 component 16 bit normalized signed integer format.
		/// </summary>
		R16_IntNorm = 58,
		/// <summary>
		/// 1 component 16 bit signed integer format.
		/// </summary>
		R16_Int = 59,
		/// <summary>
		/// 1 component 8 bit typeless format.
		/// </summary>
		R8_Typeless = 60,
		/// <summary>
		/// 1 component 8 bit normalized unsigned integer format.
		/// </summary>
		R8_UIntNorm = 61,
		/// <summary>
		/// 1 component 8 bit unsigned integer format.
		/// </summary>
		R8_UInt = 62,
		/// <summary>
		/// 1 component 8 bit normalized signed integer format.
		/// </summary>
		R8_IntNorm = 63,
		/// <summary>
		/// 1 component 8 bit signed integer format.
		/// </summary>
		R8_Int = 64,
		/// <summary>
		/// 1 component 8 bit normalized unsigned integer format.
		/// </summary>
		A8_UIntNorm = 65,
		/// <summary>
		/// 1 component 1 bit normalized unsigned integer format.
		/// </summary>
		R1_UIntNorm = 66,
		/// <summary>
		/// 4 component floating point format.
		/// </summary>
		/// <remarks>Three partial-precision floating-point numbers encoded into a single 32-bit value all sharing the same 5-bit exponent (variant of s10e5). There is no sign bit, and there is a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel.</remarks>
		R9G9B9E5_SharedExponent = 67,
		/// <summary>
		/// 4 component 32 bit normalized unsigned integer format.
		/// </summary>
		R8G8_B8G8_UIntNorm = 68,
		/// <summary>
		/// 4 component 32 bit normalized unsigned integer format.
		/// </summary>
		G8R8_G8B8_UIntNorm = 69,
		/// <summary>
		/// 4 component typeless block compression format.
		/// </summary>
		BC1_Typeless = 70,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format.
		/// </summary>
		BC1_UIntNorm = 71,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format for sRGB data.
		/// </summary>
		BC1_UIntNorm_sRGB = 72,
		/// <summary>
		/// 4 component typeless block compression format.
		/// </summary>
		BC2_Typeless = 73,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format.
		/// </summary>
		BC2_UIntNorm = 74,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format for sRGB data.
		/// </summary>
		BC2_UIntNorm_sRGB = 75,
		/// <summary>
		/// 4 component typeless block compression format.
		/// </summary>
		BC3_Typeless = 76,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format.
		/// </summary>
		BC3_UIntNorm = 77,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format for sRGB data.
		/// </summary>
		BC3_UIntNorm_sRGB = 78,
		/// <summary>
		/// 4 component typeless block compression format.
		/// </summary>
		BC4_Typeless = 79,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format.
		/// </summary>
		BC4_UIntNorm = 80,
		/// <summary>
		/// 4 component normalized signed integer block compression format.
		/// </summary>
		BC4_IntNorm = 81,
		/// <summary>
		/// 4 component typeless block compression format.
		/// </summary>
		BC5_Typeless = 82,
		/// <summary>
		/// 4 component normalized unsigned integer block compression format.
		/// </summary>
		BC5_UIntNorm = 83,
		/// <summary>
		/// 4 component normalized signed integer block compression format.
		/// </summary>
		BC5_IntNorm = 84,
		/// <summary>
		/// 3 component 16 bit normalized unsigned integer format.
		/// </summary>
		B5G6R5_UIntNorm = 85,
		/// <summary>
		/// 4 component 16 bit normalized unsigned integer format.
		/// </summary>
		B5G5R5A1_UIntNorm = 86,
		/// <summary>
		/// 4 component 32 bit normalized unsigned integer format.
		/// </summary>
		B8G8R8A8_UIntNorm = 87,
		/// <summary>
		/// 4 component 32 bit normalized unsigned integer format.
		/// </summary>
		B8G8R8X8_UIntNorm = 88,
		/// <summary>
		/// 4 component 32 bit typeless format that supports 2 bit alpha.
		/// </summary>
		R10G10B10_XR_Bias_A2_UIntNorm = 89,
		/// <summary>
		/// 4 component 32 bit typess format.
		/// </summary>
		B8G8R8A8_Typeless = 90,
		/// <summary>
		/// 4 component 32 bit normalized unsigned integer format for sRGB data.
		/// </summary>
		B8G8R8A8_UIntNorm_sRGB = 91,
		/// <summary>
		/// 4 component 32 bit typeless format.
		/// </summary>
		B8G8R8X8_Typeless = 92,
		/// <summary>
		/// 4 component 32 bit normalized unsigned integer format for sRGB data.
		/// </summary>
		B8G8R8X8_UIntNorm_sRGB = 93,
		/// <summary>
		/// A typeless block compression format.
		/// </summary>
		BC6H_Typeless = 94,
		/// <summary>
		/// A block compression format.
		/// </summary>
		BC6H_UF16 = 95,
		/// <summary>
		/// A block compression format.
		/// </summary>
		BC6H_SF16 = 96,
		/// <summary>
		/// A typeless block compression format.
		/// </summary>
		BC7_Typeless = 97,
		/// <summary>
		/// A normalized unsigned integer block compression format.
		/// </summary>
		BC7_UIntNorm = 98,
		/// <summary>
		/// A normalized unsigned integer block compression format for sRGB data.
		/// </summary>
		BC7_UIntNorm_sRGB = 99,
		/// <summary>
		/// 4 component 32 bit unsigned integer format.
		/// </summary>
		R8G8B8X8_UInt = 100
	}
}
