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
// Created: Friday, July 22, 2011 3:32:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// Utility to convert values between Gorgon and Direct 3D.
	/// </summary>
	static class D3DConvert
	{
		#region Methods.
		/// <summary>
		/// Function to convert a Gorgon video mode into a D3D display mode.
		/// </summary>
		/// <param name="mode">D3D Mode to convert.</param>
		/// <returns>The Gorgon video mode.</returns>
		public static GorgonVideoMode Convert(DisplayMode mode)
		{
			return new GorgonVideoMode(mode.Width, mode.Height, mode.RefreshRate, 1, Convert(mode.Format));
		}

		/// <summary>
		/// Function to convert a D3D buffer format into a Gorgon format.
		/// </summary>
		/// <param name="format">D3D buffer format to convert.</param>
		/// <returns>A Gorgon buffer format.</returns>
		public static GorgonBufferFormat Convert(Format format)
		{
			switch (format)
			{
				case Format.X8R8G8B8:
				case Format.A8B8G8R8:
				case Format.A8R8G8B8:
					return GorgonBufferFormat.R8G8B8A8_UIntNorm;
				case Format.A8:
					return GorgonBufferFormat.A8_UIntNorm;
				case Format.A2R10G10B10:
				case Format.A2B10G10R10:
					return GorgonBufferFormat.R10G10B10A2_Typeless;
				case Format.G16R16:
					return GorgonBufferFormat.R16G16_UIntNorm;
				case Format.A16B16G16R16:
					return GorgonBufferFormat.R16G16B16A16_UIntNorm;
				case Format.L8:
					return GorgonBufferFormat.R8_UIntNorm;
				case Format.V8U8:
					return GorgonBufferFormat.R8G8_IntNorm;
				case Format.Q8W8V8U8:
					return GorgonBufferFormat.R8G8B8A8_IntNorm;
				case Format.V16U16:
					return GorgonBufferFormat.R16G16_IntNorm;
				case Format.R8G8_B8G8:
					return GorgonBufferFormat.G8R8_G8B8_UIntNorm;
				case Format.G8R8_G8B8:
					return GorgonBufferFormat.R8G8_B8G8_UIntNorm;
				case Format.Dxt1:
				case Format.Dxt2:
					return GorgonBufferFormat.BC1_UIntNorm;
				case Format.Dxt3:
					return GorgonBufferFormat.BC2_UIntNorm;
				case Format.Dxt4:
				case Format.Dxt5:
					return GorgonBufferFormat.BC3_UIntNorm;
				case Format.D16:
				case Format.D16Lockable:
					return GorgonBufferFormat.D16_UIntNorm;
				case Format.D32SingleLockable:
					return GorgonBufferFormat.D32_Float;
				case Format.D24S8:
					return GorgonBufferFormat.D24_UIntNorm_S8_UInt;
				case Format.L16:
					return GorgonBufferFormat.R16_UIntNorm;
				case Format.Index16:
					return GorgonBufferFormat.R16_UInt;
				case Format.Index32:
					return GorgonBufferFormat.R32_UInt;
				case Format.Q16W16V16U16:
					return GorgonBufferFormat.R16G16B16A16_Int;
				case Format.R16F:
					return GorgonBufferFormat.R16_Float;
				case Format.G16R16F:
					return GorgonBufferFormat.R16G16_Float;
				case Format.A16B16G16R16F:
					return GorgonBufferFormat.R16G16B16A16_Float;
				case Format.R32F:
					return GorgonBufferFormat.R32_Float;
				case Format.G32R32F:
					return GorgonBufferFormat.R32G32_Float;
				case Format.A32B32G32R32F:
					return GorgonBufferFormat.R32G32B32A32_Float;
				default:
					return GorgonBufferFormat.Unknown;
			}
		}

		/// <summary>
		/// Function to convert a buffer format to a D3D format.
		/// </summary>
		/// <param name="format">Gorgon buffer format to convert.</param>
		/// <param name="canLock">TRUE if a buffer can be locked, FALSE if not.</param>
		/// <returns>The D3D format.</returns>
		public static Format Convert(GorgonBufferFormat format, bool canLock)
		{
			switch (format)
			{
				case GorgonBufferFormat.R8G8B8A8_UIntNorm:
				case GorgonBufferFormat.R8G8B8A8_UIntNorm_sRGB:
					return Format.A8R8G8B8;
				case GorgonBufferFormat.A8_UIntNorm:
					return Format.A8;
				case GorgonBufferFormat.R10G10B10A2_Typeless:
					return Format.A2B10G10R10;
				case GorgonBufferFormat.B8G8R8A8_UIntNorm:
				case GorgonBufferFormat.B8G8R8A8_UIntNorm_sRGB:
					return Format.A8B8G8R8;
				case GorgonBufferFormat.R16G16_UIntNorm:
					return Format.G16R16;
				case GorgonBufferFormat.R16G16B16A16_UIntNorm:
					return Format.A16B16G16R16;
				case GorgonBufferFormat.R8_UIntNorm:
					return Format.L8;
				case GorgonBufferFormat.R8G8_IntNorm:
					return Format.V8U8;
				case GorgonBufferFormat.R8G8B8A8_IntNorm:
					return Format.Q8W8V8U8;
				case GorgonBufferFormat.R16G16_IntNorm:
					return Format.V16U16;
				case GorgonBufferFormat.G8R8_G8B8_UIntNorm:
					return Format.R8G8_B8G8;
				case GorgonBufferFormat.R8G8_B8G8_UIntNorm:
					return Format.G8R8_G8B8;
				case GorgonBufferFormat.BC1_UIntNorm:
				case GorgonBufferFormat.BC1_UIntNorm_sRGB:
					return Format.Dxt1;
				case GorgonBufferFormat.BC2_UIntNorm:
				case GorgonBufferFormat.BC2_UIntNorm_sRGB:
					return Format.Dxt3;
				case GorgonBufferFormat.BC3_UIntNorm:
				case GorgonBufferFormat.BC3_UIntNorm_sRGB:
					return Format.Dxt5;
				case GorgonBufferFormat.D16_UIntNorm:
					if (!canLock)
						return Format.D16;
					else
						return Format.D16Lockable;
				case GorgonBufferFormat.D32_Float:
					return Format.D32SingleLockable;
				case GorgonBufferFormat.D24_UIntNorm_S8_UInt:
					return Format.D24S8;
				case GorgonBufferFormat.R16_UIntNorm:
					return Format.L16;
				case GorgonBufferFormat.R16_UInt:
					return Format.Index16;
				case GorgonBufferFormat.R32_UInt:
					return Format.Index32;
				case GorgonBufferFormat.R16G16B16A16_IntNorm:
					return Format.Q16W16V16U16;
				case GorgonBufferFormat.R16_Float:
					return Format.R16F;
				case GorgonBufferFormat.R16G16_Float:
					return Format.G16R16F;
				case GorgonBufferFormat.R16G16B16A16_Float:
					return Format.A16B16G16R16F;
				case GorgonBufferFormat.R32_Float:
					return Format.R32F;
				case GorgonBufferFormat.R32G32_Float:
					return Format.G32R32F;
				case GorgonBufferFormat.R32G32B32A32_Float:
					return Format.A32B32G32R32F;
				default:
					return Format.Unknown;
			}
		}

		/// <summary>
		/// Function to convert Gorgon comparison flags to the D3D compare value.
		/// </summary>
		/// <param name="compareFlags">Comparison flags to convert.</param>
		/// <returns>The Direct3D comparison flag.</returns>
		public static Compare Convert(GorgonCompareFlags compareFlags)
		{
			switch (compareFlags)
			{
				case GorgonCompareFlags.Equal:
					return Compare.Equal;
				case GorgonCompareFlags.Greater:
					return Compare.Greater;
				case GorgonCompareFlags.GreaterEqual:
					return Compare.GreaterEqual;
				case GorgonCompareFlags.Less:
					return Compare.Less;
				case GorgonCompareFlags.LessEqual:
					return Compare.LessEqual;
				case GorgonCompareFlags.Never:
					return Compare.Never;
				case GorgonCompareFlags.NotEqual:
					return Compare.NotEqual;
				default:
					return Compare.Always;
			}
		}
		#endregion
	}
}
