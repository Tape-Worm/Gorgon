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
using System.Drawing;
using System.Threading;
using Gorgon.Core.Extensions;
using Gorgon.Graphics.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics
{
    // ReSharper disable InconsistentNaming
	#region The Formats.
    /// <summary>
	/// Various buffer formats supported for textures, rendertargets, swap chains and display modes.
	/// </summary>
	public enum BufferFormat
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
	#endregion
    // ReSharper restore InconsistentNaming

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
	/// Information about a row and slice pitch for a format.
	/// </summary>
	/// <remarks>A pitch is the number of bytes it takes to move to another section.  For example, in a texture, the 
	/// row pitch would be the number of bytes it takes to move to the next line in the image, and a slice pitch is the 
	/// number of bytes required to move to the next depth slice in a 3D texture.</remarks>
	public struct GorgonFormatPitch
		: IEquatable<GorgonFormatPitch>
    {
        #region Variables.
        /// <summary>
		/// The number of bytes per line of data.
		/// </summary>
		/// <remarks>In a texture, this would indicate the number of bytes necessary for one row of pixel data.</remarks>
		public readonly int RowPitch;

		/// <summary>
		/// The number of bytes per slice of data.
		/// </summary>
		/// <remarks>In a 3D texture, this would indicate the number of bytes per level of depth.</remarks>
		public readonly int SlicePitch;

		/// <summary>
		/// The number of blocks in a compressed format.
		/// </summary>
		public readonly Size BlockCount;
        #endregion

        #region Methods.
        /// <summary>
		/// Function to compare two instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public static bool Equals(ref GorgonFormatPitch left, ref GorgonFormatPitch right)
		{
			return ((left.RowPitch == right.RowPitch) && (left.SlicePitch == right.SlicePitch)
					&& (left.BlockCount.Width == right.BlockCount.Width) && (left.BlockCount.Height == right.BlockCount.Height));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonFormatPitch)
			{
				return Equals((GorgonFormatPitch)obj);	
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(RowPitch).GenerateHash(SlicePitch).GenerateHash(BlockCount.Width).GenerateHash(BlockCount.Height);
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return BlockCount.IsEmpty
				       ? string.Format(Resources.GORGFX_FMTPITCH_TOSTR, RowPitch, SlicePitch)
				       : string.Format(Resources.GORGFX_FMTPITCH_COMPRESSED_TOSTR, RowPitch, SlicePitch, BlockCount.Width,
				                       BlockCount.Height);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not</returns>
		public static bool operator ==(GorgonFormatPitch left, GorgonFormatPitch right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><c>true</c> if not equal, <c>false</c> if they are.</returns>
		public static bool operator !=(GorgonFormatPitch left, GorgonFormatPitch right)
		{
			return !Equals(ref left, ref right);
		}
        #endregion

        #region Constructor.
        /// <summary>
		/// Initializes a new instance of the <see cref="GorgonFormatPitch" /> struct.
		/// </summary>
		/// <param name="rowPitch">The row pitch.</param>
		/// <param name="slicePitch">The slice pitch.</param>
		/// <param name="blockCount">Number of compressed blocks in a compressed format.</param>
		public GorgonFormatPitch(int rowPitch, int slicePitch, Size blockCount)
		{
			RowPitch = rowPitch;
			SlicePitch = slicePitch;
			BlockCount = blockCount;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFormatPitch" /> struct.
		/// </summary>
		/// <param name="rowPitch">The row pitch.</param>
		/// <param name="slicePitch">The slice pitch.</param>
		public GorgonFormatPitch(int rowPitch, int slicePitch)
		{
			RowPitch = rowPitch;
			SlicePitch = slicePitch;
			BlockCount = Size.Empty;
		}
        #endregion

        #region IEquatable<GorgonFormatPitch> Members
        /// <summary>
		/// Function to compare two instances for equality.
		/// </summary>
		/// <param name="other">The other instance to compare to this one.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public bool Equals(GorgonFormatPitch other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}	

	/// <summary>
	/// Retrieves information about the format.
	/// </summary>
	public static class GorgonBufferFormatInfo
	{
		#region Classes.
		/// <summary>
		/// Information about a specific GorgonBufferFormat.
		/// </summary>
		public class FormatData
		{
			#region Variables.
			private int _sizeInBytes;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the group for the format.
			/// </summary>
			public BufferFormat Group
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the format.
			/// </summary>
			public BufferFormat Format
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether the format is typeless or not.
			/// </summary>
			public bool IsTypeless
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether the format is palettized or not.
			/// </summary>
			public bool IsPalettized
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the bit depth for the format.
			/// </summary>
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
			public bool HasDepth
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether the format has a stencil component.
			/// </summary>
			public bool HasStencil
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether this format is an sRGB format or not.
			/// </summary>
			public bool IssRGB
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
			public bool IsPacked
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return whether the pixel format is compressed or not.
			/// </summary>
			public bool IsCompressed
			{
				get;
				private set;
			}
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
					case BufferFormat.D24_UIntNormal_S8_UInt:
					case BufferFormat.D32_Float_S8X24_UInt:
						HasDepth = true;
						HasStencil = true;
						break;
					case BufferFormat.D32_Float:
					case BufferFormat.D16_UIntNormal:
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
					case BufferFormat.R1_UIntNormal:
						BitDepth = 1;
						break;
					case BufferFormat.BC1:
					case BufferFormat.BC1_UIntNormal:
					case BufferFormat.BC1_UIntNormal_sRGB:
					case BufferFormat.BC4:
					case BufferFormat.BC4_UIntNormal:
					case BufferFormat.BC4_IntNormal:
						BitDepth = 4;
						break;
					case BufferFormat.R8:
					case BufferFormat.R8_UIntNormal:
					case BufferFormat.R8_UInt:
					case BufferFormat.R8_IntNormal:
					case BufferFormat.R8_Int:
					case BufferFormat.A8_UIntNormal:
					case BufferFormat.BC2:
					case BufferFormat.BC2_UIntNormal:
					case BufferFormat.BC2_UIntNormal_sRGB:
					case BufferFormat.BC3:
					case BufferFormat.BC3_UIntNormal:
					case BufferFormat.BC3_UIntNormal_sRGB:
					case BufferFormat.BC5:
					case BufferFormat.BC5_UIntNormal:
					case BufferFormat.BC5_IntNormal:
					case BufferFormat.BC6H:
					case BufferFormat.BC6H_UF16:
					case BufferFormat.BC6H_SF16:
					case BufferFormat.BC7:
					case BufferFormat.BC7_UIntNormal:
					case BufferFormat.BC7_UIntNormal_sRGB:
						BitDepth = 8;
						break;
					case BufferFormat.R8G8:
					case BufferFormat.R8G8_UIntNormal:
					case BufferFormat.R8G8_UInt:
					case BufferFormat.R8G8_IntNormal:
					case BufferFormat.R8G8_Int:
					case BufferFormat.R16:
					case BufferFormat.R16_Float:
					case BufferFormat.D16_UIntNormal:
					case BufferFormat.R16_UIntNormal:
					case BufferFormat.R16_UInt:
					case BufferFormat.R16_IntNormal:
					case BufferFormat.R16_Int:
					case BufferFormat.B5G6R5_UIntNormal:
					case BufferFormat.B5G5R5A1_UIntNormal:
						BitDepth = 16;
						break;
					case BufferFormat.R10G10B10A2:
					case BufferFormat.R10G10B10A2_UIntNormal:
					case BufferFormat.R10G10B10A2_UInt:
					case BufferFormat.R11G11B10_Float:
					case BufferFormat.R8G8B8A8:
					case BufferFormat.R8G8B8A8_UIntNormal:
					case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					case BufferFormat.R8G8B8A8_UInt:
					case BufferFormat.R8G8B8A8_IntNormal:
					case BufferFormat.R8G8B8A8_Int:
					case BufferFormat.R16G16:
					case BufferFormat.R16G16_Float:
					case BufferFormat.R16G16_UIntNormal:
					case BufferFormat.R16G16_UInt:
					case BufferFormat.R16G16_IntNormal:
					case BufferFormat.R16G16_Int:
					case BufferFormat.R32:
					case BufferFormat.D32_Float:
					case BufferFormat.R32_Float:
					case BufferFormat.R32_UInt:
					case BufferFormat.R32_Int:
					case BufferFormat.R24G8:
					case BufferFormat.D24_UIntNormal_S8_UInt:
					case BufferFormat.R24_UIntNormal_X8:
					case BufferFormat.X24_G8_UInt:
					case BufferFormat.R9G9B9E5_SharedExp:
					case BufferFormat.R8G8_B8G8_UIntNormal:
					case BufferFormat.G8R8_G8B8_UIntNormal:
					case BufferFormat.B8G8R8A8_UIntNormal:
					case BufferFormat.B8G8R8X8_UIntNormal:
					case BufferFormat.R10G10B10_XR_BIAS_A2_UIntNormal:
					case BufferFormat.B8G8R8A8:
					case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
					case BufferFormat.B8G8R8X8:
					case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
						BitDepth = 32;
						break;
					case BufferFormat.R16G16B16A16:
					case BufferFormat.R16G16B16A16_Float:
					case BufferFormat.R16G16B16A16_UIntNormal:
					case BufferFormat.R16G16B16A16_UInt:
					case BufferFormat.R16G16B16A16_IntNormal:
					case BufferFormat.R16G16B16A16_Int:
					case BufferFormat.R32G32:
					case BufferFormat.R32G32_Float:
					case BufferFormat.R32G32_UInt:
					case BufferFormat.R32G32_Int:
					case BufferFormat.R32G8X24:
					case BufferFormat.D32_Float_S8X24_UInt:
					case BufferFormat.R32_Float_X8X24:
					case BufferFormat.X32_G8X24_UInt:
						BitDepth = 64;
						break;
					case BufferFormat.R32G32B32:
					case BufferFormat.R32G32B32_Float:
					case BufferFormat.R32G32B32_UInt:
					case BufferFormat.R32G32B32_Int:
						BitDepth = 96;
						break;
					case BufferFormat.R32G32B32A32:
					case BufferFormat.R32G32B32A32_Float:
					case BufferFormat.R32G32B32A32_UInt:
					case BufferFormat.R32G32B32A32_Int:
						BitDepth = 128;
						break;
					default:
						BitDepth = 0;
						break;
				}
			}

			/// <summary>
			/// Function to retrieve whether this format is an sRGB format or not.
			/// </summary>
			/// <param name="format">Format to look up.</param>
			private void GetsRGBState(BufferFormat format)
			{
				switch (format)
				{
					case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					case BufferFormat.BC1_UIntNormal_sRGB:
					case BufferFormat.BC2_UIntNormal_sRGB:
					case BufferFormat.BC3_UIntNormal_sRGB:
					case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
					case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
					case BufferFormat.BC7_UIntNormal_sRGB: 
						IssRGB = true;
						break;
					default:
						IssRGB = false;
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
					case BufferFormat.BC1:
					case BufferFormat.BC1_UIntNormal:
					case BufferFormat.BC1_UIntNormal_sRGB:
					case BufferFormat.BC2:
					case BufferFormat.BC2_UIntNormal:
					case BufferFormat.BC2_UIntNormal_sRGB:
					case BufferFormat.BC3:
					case BufferFormat.BC3_UIntNormal:
					case BufferFormat.BC3_UIntNormal_sRGB:
					case BufferFormat.BC4:
					case BufferFormat.BC4_IntNormal:
					case BufferFormat.BC4_UIntNormal:
					case BufferFormat.BC5:
					case BufferFormat.BC5_IntNormal:
					case BufferFormat.BC5_UIntNormal:
					case BufferFormat.BC6H:
					case BufferFormat.BC6H_SF16:
					case BufferFormat.BC6H_UF16:
					case BufferFormat.BC7:
					case BufferFormat.BC7_UIntNormal:
					case BufferFormat.BC7_UIntNormal_sRGB:
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
                    case BufferFormat.B8G8R8A8:
					case BufferFormat.B8G8R8A8_UIntNormal:
					case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
						Group = BufferFormat.B8G8R8A8;
						break;
                    case BufferFormat.B8G8R8X8:
					case BufferFormat.B8G8R8X8_UIntNormal:
					case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
						Group = BufferFormat.B8G8R8X8;
						break;
                    case BufferFormat.BC1:
					case BufferFormat.BC1_UIntNormal:
					case BufferFormat.BC1_UIntNormal_sRGB:
						Group = BufferFormat.BC1;
						break;
                    case BufferFormat.BC2:
					case BufferFormat.BC2_UIntNormal:
					case BufferFormat.BC2_UIntNormal_sRGB:
						Group = BufferFormat.BC2;
						break;
                    case BufferFormat.BC3:
					case BufferFormat.BC3_UIntNormal:
					case BufferFormat.BC3_UIntNormal_sRGB:
						Group = BufferFormat.BC3;
						break;
                    case BufferFormat.BC4:
					case BufferFormat.BC4_UIntNormal:
					case BufferFormat.BC4_IntNormal:
						Group = BufferFormat.BC4;
						break;
                    case BufferFormat.BC5:
					case BufferFormat.BC5_UIntNormal:
					case BufferFormat.BC5_IntNormal:
						Group = BufferFormat.BC5;
						break;
                    case BufferFormat.BC6H:
					case BufferFormat.BC6H_UF16:
					case BufferFormat.BC6H_SF16:
						Group = BufferFormat.BC6H;
						break;
                    case BufferFormat.BC7:
					case BufferFormat.BC7_UIntNormal:
					case BufferFormat.BC7_UIntNormal_sRGB:
						Group = BufferFormat.BC7;
						break;
                    case BufferFormat.R10G10B10A2:
					case BufferFormat.R10G10B10A2_UIntNormal:
					case BufferFormat.R10G10B10A2_UInt:
						Group = BufferFormat.R10G10B10A2;
						break;
                    case BufferFormat.R16:
					case BufferFormat.R16_Float:
					case BufferFormat.R16_UIntNormal:
					case BufferFormat.R16_UInt:
					case BufferFormat.R16_IntNormal:
					case BufferFormat.R16_Int:
						Group = BufferFormat.R16;
						break;
                    case BufferFormat.R16G16:
					case BufferFormat.R16G16_Float:
					case BufferFormat.R16G16_UIntNormal:
					case BufferFormat.R16G16_UInt:
					case BufferFormat.R16G16_IntNormal:
					case BufferFormat.R16G16_Int:
						Group = BufferFormat.R16G16;
						break;
                    case BufferFormat.R16G16B16A16:
					case BufferFormat.R16G16B16A16_Float:
					case BufferFormat.R16G16B16A16_UIntNormal:
					case BufferFormat.R16G16B16A16_UInt:
					case BufferFormat.R16G16B16A16_IntNormal:
					case BufferFormat.R16G16B16A16_Int:
						Group = BufferFormat.R16G16B16A16;
						break;
                    case BufferFormat.R32:
					case BufferFormat.R32_Float:
					case BufferFormat.R32_UInt:
					case BufferFormat.R32_Int:
						Group = BufferFormat.R32;
						break;
                    case BufferFormat.R32G32:
					case BufferFormat.R32G32_Float:
					case BufferFormat.R32G32_UInt:
					case BufferFormat.R32G32_Int:
						Group = BufferFormat.R32G32;
						break;
                    case BufferFormat.R32G32B32:
					case BufferFormat.R32G32B32_Float:
					case BufferFormat.R32G32B32_UInt:
					case BufferFormat.R32G32B32_Int:
						Group = BufferFormat.R32G32B32;
						break;
                    case BufferFormat.R32G32B32A32:
					case BufferFormat.R32G32B32A32_Float:
					case BufferFormat.R32G32B32A32_UInt:
					case BufferFormat.R32G32B32A32_Int:
						Group = BufferFormat.R32G32B32A32;
						break;
                    case BufferFormat.R8:
					case BufferFormat.R8_UIntNormal:
					case BufferFormat.R8_UInt:
					case BufferFormat.R8_IntNormal:
					case BufferFormat.R8_Int:
						Group = BufferFormat.R8;
						break;
                    case BufferFormat.R8G8:
					case BufferFormat.R8G8_UIntNormal:
					case BufferFormat.R8G8_UInt:
					case BufferFormat.R8G8_IntNormal:
					case BufferFormat.R8G8_Int:
						Group = BufferFormat.R8G8;
						break;
					case BufferFormat.R8G8B8A8:
					case BufferFormat.R8G8B8A8_UIntNormal:
					case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					case BufferFormat.R8G8B8A8_UInt:
					case BufferFormat.R8G8B8A8_IntNormal:
					case BufferFormat.R8G8B8A8_Int:
						Group = BufferFormat.R8G8B8A8;
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
					case BufferFormat.R32G32B32A32:
					case BufferFormat.R32G32B32:
					case BufferFormat.R16G16B16A16:
					case BufferFormat.R32G32:
					case BufferFormat.R32G8X24:
					case BufferFormat.R32_Float_X8X24:
					case BufferFormat.X32_G8X24_UInt:
					case BufferFormat.R10G10B10A2:
					case BufferFormat.R8G8B8A8:
					case BufferFormat.R16G16:
					case BufferFormat.R32:
					case BufferFormat.R24G8:
					case BufferFormat.R24_UIntNormal_X8:
					case BufferFormat.X24_G8_UInt:
					case BufferFormat.R8G8:
					case BufferFormat.R16:
					case BufferFormat.R8:
					case BufferFormat.BC1:
					case BufferFormat.BC2:
					case BufferFormat.BC3:
					case BufferFormat.BC4:
					case BufferFormat.BC5:
					case BufferFormat.B8G8R8A8:
					case BufferFormat.B8G8R8X8:
					case BufferFormat.BC6H:
					case BufferFormat.BC7:
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
					case BufferFormat.R32G32B32A32:
					case BufferFormat.R32G32B32A32_Float:
					case BufferFormat.R32G32B32A32_UInt:
					case BufferFormat.R32G32B32A32_Int:
					case BufferFormat.R16G16B16A16:
					case BufferFormat.R16G16B16A16_Float:
					case BufferFormat.R16G16B16A16_UIntNormal:
					case BufferFormat.R16G16B16A16_UInt:
					case BufferFormat.R16G16B16A16_IntNormal:
					case BufferFormat.R16G16B16A16_Int:
					case BufferFormat.R10G10B10A2:
					case BufferFormat.R10G10B10A2_UIntNormal:
					case BufferFormat.R10G10B10A2_UInt:
					case BufferFormat.R8G8B8A8:
					case BufferFormat.R8G8B8A8_UIntNormal:
					case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					case BufferFormat.R8G8B8A8_UInt:
					case BufferFormat.R8G8B8A8_IntNormal:
					case BufferFormat.R8G8B8A8_Int:
					case BufferFormat.A8_UIntNormal:
					case BufferFormat.BC2:
					case BufferFormat.BC2_UIntNormal:
					case BufferFormat.BC2_UIntNormal_sRGB:
					case BufferFormat.BC3:
					case BufferFormat.BC3_UIntNormal:
					case BufferFormat.BC3_UIntNormal_sRGB:
					case BufferFormat.B5G5R5A1_UIntNormal:
					case BufferFormat.B8G8R8A8_UIntNormal:
					case BufferFormat.R10G10B10_XR_BIAS_A2_UIntNormal:
					case BufferFormat.B8G8R8A8:
					case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
					case BufferFormat.BC7:
					case BufferFormat.BC7_UIntNormal:
					case BufferFormat.BC7_UIntNormal_sRGB:
						HasAlpha = true;
						break;
					default:
						HasAlpha = false;
						break;
				}
			}

			/// <summary>
			/// Function to parse the format.
			/// </summary>
			/// <param name="format">Format to parse.</param>
			private void ParseFormat(BufferFormat format)
			{
				GetGroup(format);
				GetCompressedState(format);
				GetTypelessState(format);
				GetAlphaChannel(format);
				GetsRGBState(format);
				GetDepthState(format);
				GetBitDepth(format);

			    IsPacked = (format == BufferFormat.R8G8_B8G8_UIntNormal) || (format == BufferFormat.G8R8_G8B8_UIntNormal);
			}

			/// <summary>
			/// Function to return pitch information for this format.
			/// </summary>
			/// <param name="width">The width of the pixel data.</param>
			/// <param name="height">The height of the pixel data.</param>
			/// <param name="flags">Legacy flags for counting the bits per pixel and data alignment.</param>
			/// <returns>The pitch information for the format.</returns>
			public GorgonFormatPitch GetPitch(int width, int height, PitchFlags flags)
			{
				int rowPitch;

			    // Do calculations for compressed formats.
				if (IsCompressed)
				{
					int bpb;

					switch (Format)
					{
						case BufferFormat.BC1:
						case BufferFormat.BC1_UIntNormal:
						case BufferFormat.BC1_UIntNormal_sRGB:
						case BufferFormat.BC4:
						case BufferFormat.BC4_IntNormal:
						case BufferFormat.BC4_UIntNormal:
							bpb = 8;
							break;
						default:
							bpb = 16;
							break;
					}

					int widthCounter = 1.Max((width + 3) / 4);
					int heightCounter = 1.Max((height + 3) / 4);
					rowPitch = widthCounter * bpb;

					return new GorgonFormatPitch(widthCounter * bpb, heightCounter * rowPitch, new Size(widthCounter, heightCounter));
				}

				if (IsPacked)
				{
					rowPitch = ((width + 1) >> 1) >> 2;
					return new GorgonFormatPitch(rowPitch, rowPitch * height, Size.Empty);
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
					rowPitch = ((width * bitsPerPixel + 31) / 32) * sizeof(int);
				}
				else
				{
					rowPitch = ((width * bitsPerPixel + 7) / 8);
				}

				return new GorgonFormatPitch(rowPitch, rowPitch * height, Size.Empty);
			}

			/// <summary>
			/// Function to compute the scan lines for a format, given a specific height.
			/// </summary>
			/// <param name="height">Height of the image.</param>
			/// <returns>The number of scan lines.</returns>
			/// <remarks>
			/// This will compute the number of scan lines for an image that uses the format that this information describes.  If the format is 
			/// <see cref="P:GorgonLibrary.Graphics.GorgonBufferFormatInfo.FormatData.IsCompressed">compressed</see>, then this method will 
			/// compute the scanline count based on the maximum size between 1 and a block size multiple of 4.  If the format is not compressed, 
			/// then it will just return the height value passed in.
			/// </remarks>
			public int Scanlines(int height)
			{
				return IsCompressed ? 1.Max((height + 3) / 4) : height;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="FormatData"/> class.
			/// </summary>
			/// <param name="format">The format to evaluate.</param>
			internal FormatData(BufferFormat format)
			{
				Format = format;
				Group = BufferFormat.Unknown;

				if (format != BufferFormat.Unknown)
				{
					ParseFormat(format);
				}
			}
			#endregion
		}
		#endregion

		#region Variables.
	    private static int _syncInc;  
		private static readonly IDictionary<BufferFormat, FormatData> _formats;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the format information.
		/// </summary>
		private static void GetFormatInfo()
		{
		    try
		    {
		        if (Interlocked.Increment(ref _syncInc) > 1)
		        {
		            return;
		        }

		        var formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));

		        foreach (var format in formats)
		        {
		            _formats.Add(format, new FormatData(format));
		        }
		    }
		    finally
		    {
		        Interlocked.Decrement(ref _syncInc);
		    }
		}

		/// <summary>
		/// Function to retrieve information about a format.
		/// </summary>
		/// <param name="format">Format to retrieve information about.</param>
		/// <returns>The information for the format.  If the format is unknown, then the data for the Unknown GorgonBufferFormat will be returned.</returns>
		public static FormatData GetInfo(BufferFormat format)
		{
			return !_formats.ContainsKey(format) ? _formats[BufferFormat.Unknown] : _formats[format];
		}

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonBufferFormatInfo"/> class.
		/// </summary>
		static GorgonBufferFormatInfo()
		{
			_formats = new Dictionary<BufferFormat, FormatData>();
			GetFormatInfo();
		}
		#endregion
	}
}
