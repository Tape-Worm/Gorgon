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

using System.Diagnostics.CodeAnalysis;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A list of data formats used to describe the layout of a buffer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// These values are defined as R, G, B and A components. Each value may also be considered as X, Y, Z and W components for buffers that use coordinate systems rather than color values. The numeric 
	/// values in the name of the format typically indicates the number of bits occupied by a component. For example, <c>R8G8B8A8</c> would indicate 4 components, each using 8 bits of space.
	/// </para>
	/// <para>
	/// Buffer formats may be typed or untyped. A typed format indicates the type of each component, while untyped indicates that there is not specific type associated with the component. For example, 
	/// <c>R8G8B8A8_UInt</c> would mean that all 4 components are unsigned integers, while <c>R32_Typeless</c> means that the 32 bit component has no specific type.
	/// </para>
	/// <para>
	/// The following table shows the modifiers used to describe the format type:
	/// <list type="table">
	///		<listheader>
	///			<term>Modifier</term>
	///			<description>Description</description>
	///		</listheader> 
	///		<item>
	///			<term>Float</term>
	///			<description>A floating-point value; 32-bit floating-point formats use IEEE 754 single-precision (s23e8 format): sign bit, 8-bit biased (127) exponent, and 23-bit mantissa. 16-bit floating-point formats use half-precision (s10e5 format): sign bit, 5-bit biased (15) exponent, and 10-bit mantissa.</description>
	///		</item>
	///		<item>
	///			<term>SInt</term>
	///			<description>Two's complement signed integer. For example, a 3-bit SINT represents the values -4, -3, -2, -1, 0, 1, 2, 3.</description>
	///		</item>
	///		<item>
	///			<term>SNorm</term>
	///			<description>Signed normalized integer; which is interpreted in a resource as a signed integer, and is interpreted in a shader as a signed normalized floating-point value in the range [-1, 1]. For an 2's complement number, the maximum value is 1.0f (a 5-bit value 01111 maps to 1.0f), and the minimum value is -1.0f (a 5-bit value 10000 maps to -1.0f). In addition, the second-minimum number maps to -1.0f (a 5-bit value 10001 maps to -1.0f). The resulting integer representations are evenly spaced floating-point values in the range (-1.0f...0.0f), and also a complementary set of representations for numbers in the range (0.0f...1.0f).</description>
	///		</item>
	///		<item>
	///			<term>SRgb</term>
	///			<description>
	///				<para>
	///					Standard RGB data, which roughly displays colors in a linear ramp of luminosity levels such that an average observer, under average viewing conditions, can view them on an average display.
	///				</para>
	///				<para>
	///					All 0's maps to 0.0f, and all 1's maps to 1.0f. The sequence of unsigned integer encodings between all 0's and all 1's represent a nonlinear progression in the floating-point interpretation of the numbers between 0.0f to 1.0f. For more detail, see the SRGB color standard, IEC 61996-2-1, at IEC(International Electrotechnical Commission).
	///				</para>
	///				<para>
	///					Conversion to or from sRGB space is automatically done by D3DX10 or D3DX9 texture-load functions.If a format with _SRGB has an A channel, the A channel is stored in Gamma 1.0f data; the R, G, and B channels in the format are stored in Gamma 2.2f data.
	///				</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>Typeless</term>
	///			<description>
	///				<para>
	///					Typeless data, with a defined number of bits. Typeless formats are designed for creating typeless resources; that is, a resource whose size is known, but whose data type is not yet fully defined. When a typeless resource is bound to a shader, the application or shader must resolve the format type (which must match the number of bits per component in the typeless format).
	///				</para>
	///				<para>
	///					A typeless format contains one or more subformats; each subformat resolves the data type.For example, in the R32G32B32 group, which defines types for three-component 96-bit data, there is one typeless format and three fully typed subformats.
	///				</para>
	///				<para> 
	///					<c>DXGI_FORMAT_R32G32B32_TYPELESS,</c><br/>
	///					<c>DXGI_FORMAT_R32G32B32_FLOAT,</c><br/>
	///					<c>DXGI_FORMAT_R32G32B32_UINT,</c><br/>
	///					<c>DXGI_FORMAT_R32G32B32_SINT</c><br/>
	///				</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>UInt</term>
	///			<description>Unsigned integer. For instance, a 3-bit UINT represents the values 0, 1, 2, 3, 4, 5, 6, 7.</description>
	///		</item>
	///		<item>
	///			<term>UNorm</term>
	///			<description>Unsigned normalized integer; which is interpreted in a resource as an unsigned integer, and is interpreted in a shader as an unsigned normalized floating-point value in the range [0, 1]. All 0's maps to 0.0f, and all 1's maps to 1.0f. A sequence of evenly spaced floating-point values from 0.0f to 1.0f are represented. For instance, a 2-bit UNORM represents 0.0f, 1/3, 2/3, and 1.0f.</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum BufferFormat
	{
		/// <summary>
		/// Unknown format.
		/// </summary>
		Unknown = DXGI.Format.Unknown,
		/// <summary>
		/// A four-component, 128-bit typeless format that supports 32 bits per channel including alpha.
		/// </summary>
		R32G32B32A32_Typeless = DXGI.Format.R32G32B32A32_Typeless,
		/// <summary>
		/// A four-component, 128-bit floating-point format that supports 32 bits per channel including alpha.
		/// </summary>
		R32G32B32A32_Float = DXGI.Format.R32G32B32A32_Float,
		/// <summary>
		/// A four-component, 128-bit unsigned-integer format that supports 32 bits per channel including alpha.
		/// </summary>
		R32G32B32A32_UInt = DXGI.Format.R32G32B32A32_UInt,
		/// <summary>
		/// A four-component, 128-bit signed-integer format that supports 32 bits per channel including alpha.
		/// </summary>
		R32G32B32A32_SInt = DXGI.Format.R32G32B32A32_SInt,
		/// <summary>
		/// A three-component, 96-bit typeless format that supports 32 bits per color channel.
		/// </summary>
		R32G32B32_Typeless = DXGI.Format.R32G32B32_Typeless,
		/// <summary>
		/// A three-component, 96-bit floating-point format that supports 32 bits per color channel.
		/// </summary>
		R32G32B32_Float = DXGI.Format.R32G32B32_Float,
		/// <summary>
		/// A three-component, 96-bit unsigned-integer format that supports 32 bits per color channel.
		/// </summary>
		R32G32B32_UInt = DXGI.Format.R32G32B32_UInt,
		/// <summary>
		/// A three-component, 96-bit signed-integer format that supports 32 bits per color channel.
		/// </summary>
		R32G32B32_SInt = DXGI.Format.R32G32B32_SInt,
		/// <summary>
		/// A four-component, 64-bit typeless format that supports 16 bits per channel including alpha.
		/// </summary>
		R16G16B16A16_Typeless = DXGI.Format.R16G16B16A16_Typeless,
		/// <summary>
		/// A four-component, 64-bit floating-point format that supports 16 bits per channel including alpha.
		/// </summary>
		R16G16B16A16_Float = DXGI.Format.R16G16B16A16_Float,
		/// <summary>
		/// A four-component, 64-bit unsigned-normalized-integer format that supports 16 bits per channel including alpha.
		/// </summary>
		R16G16B16A16_UNorm = DXGI.Format.R16G16B16A16_UNorm,
		/// <summary>
		/// A four-component, 64-bit unsigned-integer format that supports 16 bits per channel including alpha.
		/// </summary>
		R16G16B16A16_UInt = DXGI.Format.R16G16B16A16_UInt,
		/// <summary>
		/// A four-component, 64-bit signed-normalized-integer format that supports 16 bits per channel including alpha.
		/// </summary>
		R16G16B16A16_SNorm = DXGI.Format.R16G16B16A16_SNorm,
		/// <summary>
		/// A four-component, 64-bit signed-integer format that supports 16 bits per channel including alpha.
		/// </summary>
		R16G16B16A16_SInt = DXGI.Format.R16G16B16A16_SInt,
		/// <summary>
		/// A two-component, 64-bit typeless format that supports 32 bits for the red channel and 32 bits for the green channel.
		/// </summary>
		R32G32_Typeless = DXGI.Format.R32G32_Typeless,
		/// <summary>
		/// A two-component, 64-bit floating-point format that supports 32 bits for the red channel and 32 bits for the green channel.
		/// </summary>
		R32G32_Float = DXGI.Format.R32G32_Float,
		/// <summary>
		/// A two-component, 64-bit unsigned-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
		/// </summary>
		R32G32_UInt = DXGI.Format.R32G32_UInt,
		/// <summary>
		/// A two-component, 64-bit signed-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
		/// </summary>
		R32G32_SInt = DXGI.Format.R32G32_SInt,
		/// <summary>
		/// A two-component, 64-bit typeless format that supports 32 bits for the red channel, 8 bits for the green channel, and 24 bits are unused.
		/// </summary>
		R32G8X24_Typeless = DXGI.Format.R32G8X24_Typeless,
		/// <summary>
		/// A 32-bit floating-point component, and two unsigned-integer components (with an additional 32 bits). This format supports 32-bit depth, 8-bit stencil, and 24 bits are unused.
		/// </summary>
		D32_Float_S8X24_UInt = DXGI.Format.D32_Float_S8X24_UInt,
		/// <summary>
		/// A 32-bit floating-point component, and two typeless components (with an additional 32 bits). This format supports 32-bit red channel, 8 bits are unused, and 24 bits are unused.
		/// </summary>
		R32_Float_X8X24_Typeless = DXGI.Format.R32_Float_X8X24_Typeless,
		/// <summary>
		/// A 32-bit typeless component, and two unsigned-integer components (with an additional 32 bits). This format has 32 bits unused, 8 bits for green channel, and 24 bits are unused.
		/// </summary>
		X32_Typeless_G8X24_UInt = DXGI.Format.X32_Typeless_G8X24_UInt,
		/// <summary>
		/// A four-component, 32-bit typeless format that supports 10 bits for each color and 2 bits for alpha.
		/// </summary>
		R10G10B10A2_Typeless = DXGI.Format.R10G10B10A2_Typeless,
		/// <summary>
		/// A four-component, 32-bit unsigned-normalized-integer format that supports 10 bits for each color and 2 bits for alpha.
		/// </summary>
		R10G10B10A2_UNorm = DXGI.Format.R10G10B10A2_UNorm,
		/// <summary>
		/// A four-component, 32-bit unsigned-integer format that supports 10 bits for each color and 2 bits for alpha.
		/// </summary>
		R10G10B10A2_UInt = DXGI.Format.R10G10B10A2_UInt,
		/// <summary>
		/// Three partial-precision floating-point numbers encoded into a single 32-bit value (a variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent). There are no sign bits, and there is a 5-bit biased (15) exponent for each channel, 6-bit mantissa for R and G, and a 5-bit mantissa for B.
		/// </summary>
		R11G11B10_Float = DXGI.Format.R11G11B10_Float,
		/// <summary>
		/// A four-component, 32-bit typeless format that supports 8 bits per channel including alpha.
		/// </summary>
		R8G8B8A8_Typeless = DXGI.Format.R8G8B8A8_Typeless,
		/// <summary>
		/// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits per channel including alpha.
		/// </summary>
		R8G8B8A8_UNorm = DXGI.Format.R8G8B8A8_UNorm,
		/// <summary>
		/// A four-component, 32-bit unsigned-normalized integer sRGB format that supports 8 bits per channel including alpha.
		/// </summary>
		R8G8B8A8_UNorm_SRgb = DXGI.Format.R8G8B8A8_UNorm_SRgb,
		/// <summary>
		/// A four-component, 32-bit unsigned-integer format that supports 8 bits per channel including alpha.
		/// </summary>
		R8G8B8A8_UInt = DXGI.Format.R8G8B8A8_UInt,
		/// <summary>
		/// A four-component, 32-bit signed-normalized-integer format that supports 8 bits per channel including alpha.
		/// </summary>
		R8G8B8A8_SNorm = DXGI.Format.R8G8B8A8_SNorm,
		/// <summary>
		/// A four-component, 32-bit signed-integer format that supports 8 bits per channel including alpha.
		/// </summary>
		R8G8B8A8_SInt = DXGI.Format.R8G8B8A8_SInt,
		/// <summary>
		/// A two-component, 32-bit typeless format that supports 16 bits for the red channel and 16 bits for the green channel.
		/// </summary>
		R16G16_Typeless = DXGI.Format.R16G16_Typeless,
		/// <summary>
		/// A two-component, 32-bit floating-point format that supports 16 bits for the red channel and 16 bits for the green channel.
		/// </summary>
		R16G16_Float = DXGI.Format.R16G16_Float,
		/// <summary>
		/// A two-component, 32-bit unsigned-normalized-integer format that supports 16 bits each for the green and red channels.
		/// </summary>
		R16G16_UNorm = DXGI.Format.R16G16_UNorm,
		/// <summary>
		/// A two-component, 32-bit unsigned-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
		/// </summary>
		R16G16_UInt = DXGI.Format.R16G16_UInt,
		/// <summary>
		/// A two-component, 32-bit signed-normalized-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
		/// </summary>
		R16G16_SNorm = DXGI.Format.R16G16_SNorm,
		/// <summary>
		/// A two-component, 32-bit signed-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
		/// </summary>
		R16G16_SInt = DXGI.Format.R16G16_SInt,
		/// <summary>
		/// A single-component, 32-bit typeless format that supports 32 bits for the red channel.
		/// </summary>
		R32_Typeless = DXGI.Format.R32_Typeless,
		/// <summary>
		/// A single-component, 32-bit floating-point format that supports 32 bits for depth.
		/// </summary>
		D32_Float = DXGI.Format.D32_Float,
		/// <summary>
		/// A single-component, 32-bit floating-point format that supports 32 bits for the red channel.
		/// </summary>
		R32_Float = DXGI.Format.R32_Float,
		/// <summary>
		/// A single-component, 32-bit unsigned-integer format that supports 32 bits for the red channel.
		/// </summary>
		R32_UInt = DXGI.Format.R32_UInt,
		/// <summary>
		/// A single-component, 32-bit signed-integer format that supports 32 bits for the red channel.
		/// </summary>
		R32_SInt = DXGI.Format.R32_SInt,
		/// <summary>
		/// A two-component, 32-bit typeless format that supports 24 bits for the red channel and 8 bits for the green channel.
		/// </summary>
		R24G8_Typeless = DXGI.Format.R24G8_Typeless,
		/// <summary>
		/// A 32-bit z-buffer format that supports 24 bits for depth and 8 bits for stencil.
		/// </summary>
		D24_UNorm_S8_UInt = DXGI.Format.D24_UNorm_S8_UInt,
		/// <summary>
		/// A 32-bit format, that contains a 24 bit, single-component, unsigned-normalized integer, with an additional typeless 8 bits. This format has 24 bits red channel and 8 bits unused.
		/// </summary>
		R24_UNorm_X8_Typeless = DXGI.Format.R24_UNorm_X8_Typeless,
		/// <summary>
		/// A 32-bit format, that contains a 24 bit, single-component, typeless format, with an additional 8 bit unsigned integer component. This format has 24 bits unused and 8 bits green channel.
		/// </summary>
		X24_Typeless_G8_UInt = DXGI.Format.X24_Typeless_G8_UInt,
		/// <summary>
		/// A two-component, 16-bit typeless format that supports 8 bits for the red channel and 8 bits for the green channel.
		/// </summary>
		R8G8_Typeless = DXGI.Format.R8G8_Typeless,
		/// <summary>
		/// A two-component, 16-bit unsigned-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
		/// </summary>
		R8G8_UNorm = DXGI.Format.R8G8_UNorm,
		/// <summary>
		/// A two-component, 16-bit unsigned-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
		/// </summary>
		R8G8_UInt = DXGI.Format.R8G8_UInt,
		/// <summary>
		/// A two-component, 16-bit signed-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
		/// </summary>
		R8G8_SNorm = DXGI.Format.R8G8_SNorm,
		/// <summary>
		/// A two-component, 16-bit signed-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
		/// </summary>
		R8G8_SInt = DXGI.Format.R8G8_SInt,
		/// <summary>
		/// A single-component, 16-bit typeless format that supports 16 bits for the red channel.
		/// </summary>
		R16_Typeless = DXGI.Format.R16_Typeless,
		/// <summary>
		/// A single-component, 16-bit floating-point format that supports 16 bits for the red channel.
		/// </summary>
		R16_Float = DXGI.Format.R16_Float,
		/// <summary>
		/// A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for depth.
		/// </summary>
		D16_UNorm = DXGI.Format.D16_UNorm,
		/// <summary>
		/// A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for the red channel.
		/// </summary>
		R16_UNorm = DXGI.Format.R16_UNorm,
		/// <summary>
		/// A single-component, 16-bit unsigned-integer format that supports 16 bits for the red channel.
		/// </summary>
		R16_UInt = DXGI.Format.R16_UInt,
		/// <summary>
		/// A single-component, 16-bit signed-normalized-integer format that supports 16 bits for the red channel.
		/// </summary>
		R16_SNorm = DXGI.Format.R16_SNorm,
		/// <summary>
		/// A single-component, 16-bit signed-integer format that supports 16 bits for the red channel.
		/// </summary>
		R16_SInt = DXGI.Format.R16_SInt,
		/// <summary>
		/// A single-component, 8-bit typeless format that supports 8 bits for the red channel.
		/// </summary>
		R8_Typeless = DXGI.Format.R8_Typeless,
		/// <summary>
		/// A single-component, 8-bit unsigned-normalized-integer format that supports 8 bits for the red channel.
		/// </summary>
		R8_UNorm = DXGI.Format.R8_UNorm,
		/// <summary>
		/// A single-component, 8-bit unsigned-integer format that supports 8 bits for the red channel.
		/// </summary>
		R8_UInt = DXGI.Format.R8_UInt,
		/// <summary>
		/// A single-component, 8-bit signed-normalized-integer format that supports 8 bits for the red channel.
		/// </summary>
		R8_SNorm = DXGI.Format.R8_SNorm,
		/// <summary>
		/// A single-component, 8-bit signed-integer format that supports 8 bits for the red channel.
		/// </summary>
		R8_SInt = DXGI.Format.R8_SInt,
		/// <summary>
		/// A single-component, 8-bit unsigned-normalized-integer format for alpha only.
		/// </summary>
		A8_UNorm = DXGI.Format.A8_UNorm,
		/// <summary>
		/// A single-component, 1-bit unsigned-normalized integer format that supports 1 bit for the red channel. .
		/// </summary>
		R1_UNorm = DXGI.Format.R1_UNorm,
		/// <summary>
		/// Three partial-precision floating-point numbers encoded into a single 32-bit value all sharing the same 5-bit exponent (variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent). There is no sign bit, and there is a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel.
		/// </summary>
		R9G9B9E5_SharedExp = DXGI.Format.R9G9B9E5_Sharedexp,
		/// <summary>
		/// <para>
		/// A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the UYVY format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and the G8 values are unique to each pixel.
		/// </para>
		/// <para>
		/// Width must be even.
		/// </para>
		/// </summary>
		R8G8_B8G8_UNorm = DXGI.Format.R8G8_B8G8_UNorm,
		/// <summary>
		/// <para>
		/// A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the YUY2 format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and the G8 values are unique to each pixel.
		/// </para>
		/// <para>
		/// Width must be even.
		/// </para>
		/// </summary>
		G8R8_G8B8_UNorm = DXGI.Format.G8R8_G8B8_UNorm,
		/// <summary>
		/// Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC1_Typeless = DXGI.Format.BC1_Typeless,
		/// <summary>
		/// Four-component block-compression format. For information about block-compression formats, seeTexture Block Compression in Direct3D 11.
		/// </summary>
		BC1_UNorm = DXGI.Format.BC1_UNorm,
		/// <summary>
		/// Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC1_UNorm_SRgb = DXGI.Format.BC1_UNorm_SRgb,
		/// <summary>
		/// Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC2_Typeless = DXGI.Format.BC2_Typeless,
		/// <summary>
		/// Four-component block-compression format. For information about block-compression formats, seeTexture Block Compression in Direct3D 11.
		/// </summary>
		BC2_UNorm = DXGI.Format.BC2_UNorm,
		/// <summary>
		/// Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC2_UNorm_SRgb = DXGI.Format.BC2_UNorm_SRgb,
		/// <summary>
		/// Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC3_Typeless = DXGI.Format.BC3_Typeless,
		/// <summary>
		/// Four-component block-compression format. For information about block-compression formats, seeTexture Block Compression in Direct3D 11.
		/// </summary>
		BC3_UNorm = DXGI.Format.BC3_UNorm,
		/// <summary>
		/// Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC3_UNorm_SRgb = DXGI.Format.BC3_UNorm_SRgb,
		/// <summary>
		/// One-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC4_Typeless = DXGI.Format.BC4_Typeless,
		/// <summary>
		/// One-component block-compression format. For information about block-compression formats, seeTexture Block Compression in Direct3D 11.
		/// </summary>
		BC4_UNorm = DXGI.Format.BC4_UNorm,
		/// <summary>
		/// One-component block-compression format. For information about block-compression formats, seeTexture Block Compression in Direct3D 11.
		/// </summary>
		BC4_SNorm = DXGI.Format.BC4_SNorm,
		/// <summary>
		/// Two-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC5_Typeless = DXGI.Format.BC5_Typeless,
		/// <summary>
		/// Two-component block-compression format. For information about block-compression formats, seeTexture Block Compression in Direct3D 11.
		/// </summary>
		BC5_UNorm = DXGI.Format.BC5_UNorm,
		/// <summary>
		/// Two-component block-compression format. For information about block-compression formats, seeTexture Block Compression in Direct3D 11.
		/// </summary>
		BC5_SNorm = DXGI.Format.BC5_SNorm,
		/// <summary>
		/// <para>
		/// A three-component, 16-bit unsigned-normalized-integer format that supports 5 bits for blue, 6 bits for green, and 5 bits for red.
		/// </para>
		/// <para>
		/// <b>Direct3D 10 through Direct3D 11:</b>:  This value is defined for DXGI. However, Direct3D 10, 10.1, or 11 devices do not support this format.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		B5G6R5_UNorm = DXGI.Format.B5G6R5_UNorm,
		/// <summary>
		/// <para>
		/// A four-component, 16-bit unsigned-normalized-integer format that supports 5 bits for each color channel and 1-bit alpha.
		/// </para>
		/// <para>
		/// <b>Direct3D 10 through Direct3D 11:</b>:  This value is defined for DXGI. However, Direct3D 10, 10.1, or 11 devices do not support this format.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		B5G5R5A1_UNorm = DXGI.Format.B5G5R5A1_UNorm,
		/// <summary>
		/// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8-bit alpha.
		/// </summary>
		B8G8R8A8_UNorm = DXGI.Format.B8G8R8A8_UNorm,
		/// <summary>
		/// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8 bits unused.
		/// </summary>
		B8G8R8X8_UNorm = DXGI.Format.B8G8R8X8_UNorm,
		/// <summary>
		/// A four-component, 32-bit 2.8-biased fixed-point format that supports 10 bits for each color channel and 2-bit alpha.
		/// </summary>
		R10G10B10_Xr_Bias_A2_UNorm = DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm,
		/// <summary>
		/// A four-component, 32-bit typeless format that supports 8 bits for each channel including alpha. 4
		/// </summary>
		B8G8R8A8_Typeless = DXGI.Format.B8G8R8A8_Typeless,
		/// <summary>
		/// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each channel including alpha. 4
		/// </summary>
		B8G8R8A8_UNorm_SRgb = DXGI.Format.B8G8R8A8_UNorm_SRgb,
		/// <summary>
		/// A four-component, 32-bit typeless format that supports 8 bits for each color channel, and 8 bits are unused. 4
		/// </summary>
		B8G8R8X8_Typeless = DXGI.Format.B8G8R8X8_Typeless,
		/// <summary>
		/// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each color channel, and 8 bits are unused. 4
		/// </summary>
		B8G8R8X8_UNorm_SRgb = DXGI.Format.B8G8R8X8_UNorm_SRgb,
		/// <summary>
		/// A typeless block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC6H_Typeless = DXGI.Format.BC6H_Typeless,
		/// <summary>
		/// A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC6H_Uf16 = DXGI.Format.BC6H_Uf16,
		/// <summary>
		/// A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC6H_Sf16 = DXGI.Format.BC6H_Sf16,
		/// <summary>
		/// A typeless block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC7_Typeless = DXGI.Format.BC7_Typeless,
		/// <summary>
		/// A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC7_UNorm = DXGI.Format.BC7_UNorm,
		/// <summary>
		/// A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.
		/// </summary>
		BC7_UNorm_SRgb = DXGI.Format.BC7_UNorm_SRgb,
		/// <summary>
		/// <para>
		/// Most common YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. Supported view types are SRV, RTV, and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is V->R8, U->G8, Y->B8, and A->A8.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		AYUV = DXGI.Format.AYUV,
		/// <summary>
		/// <para>
		/// 10-bit per channel packed YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT. For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is U->R10, Y->G10, V->B10, and A->A2.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		Y410 = DXGI.Format.Y410,
		/// <summary>
		/// <para>
		/// 16-bit per channel packed YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is U->R16, Y->G16, V->B16, and A->A16.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		Y416 = DXGI.Format.Y416,
		/// <summary>
		/// <para>
		/// Most common YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT. Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R8. For chrominance data view, the mapping to the view channel is U->R8 and V->G8.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// Width and height must be even. Direct3D 11 staging resources and initData parameters for this format use (rowPitch * (height + (height / 2))) bytes. The first (SysMemPitch * height) bytes are the Y plane, the remaining (SysMemPitch * (height / 2)) bytes are the UV plane.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		NV12 = DXGI.Format.NV12,
		/// <summary>
		/// <para>
		/// 10-bit per channel planar YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT. The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 10-bit format that uses 16 bits). If required, application shader code would have to enforce this manually. From the runtime's point of view, DXGI_FORMAT_P010 is no different than DXGI_FORMAT_P016. Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. For UAVs, an additional valid chrominance data view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R16. For chrominance data view, the mapping to the view channel is U->R16 and V->G16.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// Width and height must be even. Direct3D 11 staging resources and initData parameters for this format use (rowPitch * (height + (height / 2))) bytes. The first (SysMemPitch * height) bytes are the Y plane, the remaining (SysMemPitch * (height / 2)) bytes are the UV plane.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		P010 = DXGI.Format.P010,
		/// <summary>
		/// <para>
		/// 16-bit per channel planar YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT. Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. For UAVs, an additional valid chrominance data view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R16. For chrominance data view, the mapping to the view channel is U->R16 and V->G16.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// Width and height must be even. Direct3D 11 staging resources and initData parameters for this format use (rowPitch * (height + (height / 2))) bytes. The first (SysMemPitch * height) bytes are the Y plane, the remaining (SysMemPitch * (height / 2)) bytes are the UV plane.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		P016 = DXGI.Format.P016,
		/// <summary>
		/// <para>
		/// 8-bit per channel planar YUV 4:2:0 video resource format. This format is subsampled where each pixel has its own Y value, but each 2x2 pixel block shares a single U and V value. The runtime requires that the width and height of all resources that are created with this format are multiples of 2. The runtime also requires that the left, right, top, and bottom members of any RECT that are used for this format are multiples of 2. This format differs from DXGI_FORMAT_NV12 in that the layout of the data within the resource is completely opaque to applications. Applications cannot use the CPU to map the resource and then access the data within the resource. You cannot use shaders with this format. Because of this behavior, legacy hardware that supports a non-NV12 4:2:0 layout (for example, YV12, and so on) can be used. Also, new hardware that has a 4:2:0 implementation better than NV12 can be used when the application does not need the data to be in a standard layout.
		/// </para>
		/// <para>
		/// Width and height must be even. Direct3D 11 staging resources and initData parameters for this format use (rowPitch * (height + (height / 2))) bytes.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		Opaque420 = DXGI.Format.Opaque420,
		/// <summary>
		/// <para>
		/// Most common YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0->R8, U0->G8, Y1->B8, and V0->A8.
		/// </para>
		/// <para>
		/// A unique valid view format for this video resource format is DXGI_FORMAT_R8G8_B8G8_UNORM. With this view format, the width of the view appears to be twice what the DXGI_FORMAT_R8G8B8A8_UNORM or DXGI_FORMAT_R8G8B8A8_UINT view would be when hardware reconstructs RGBA automatically on read and before filtering. This Direct3D hardware behavior is legacy and is likely not useful any more. With this view format, the mapping to the view channel is Y0->R8, U0-> G8[0], Y1->B8, and V0-> G8[1].
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// Width must be even.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		YUY2 = DXGI.Format.YUY2,
		/// <summary>
		/// <para>
		/// 10-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 10-bit format that uses 16 bits). If required, application shader code would have to enforce this manually. From the runtime's point of view, DXGI_FORMAT_Y210 is no different than DXGI_FORMAT_Y216. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0->R16, U->G16, Y1->B16, and V->A16.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// Width must be even.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		Y210 = DXGI.Format.Y210,
		/// <summary>
		/// <para>
		/// 16-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0->R16, U->G16, Y1->B16, and V->A16.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// Width must be even.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		Y216 = DXGI.Format.Y216,
		/// <summary>
		/// <para>
		/// Most common planar YUV 4:1:1 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT. Valid chrominance data view formats (width and height are each 1/4 of luminance view) for this video resource format are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R8. For chrominance data view, the mapping to the view channel is U->R8 and V->G8.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// Width must be a multiple of 4. Direct3D11 staging resources and initData parameters for this format use (rowPitch * height * 2) bytes. The first (SysMemPitch * height) bytes are the Y plane, the next ((SysMemPitch / 2) * height) bytes are the UV plane, and the remainder is padding.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		NV11 = DXGI.Format.NV11,
		/// <summary>
		/// <para>
		/// 4-bit palletized YUV format that is commonly used for DVD sub picture.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		AI44 = DXGI.Format.AI44,
		/// <summary>
		/// <para>
		/// 4-bit palletized YUV format that is commonly used for DVD sub picture.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		IA44 = DXGI.Format.IA44,
		/// <summary>
		/// <para>
		/// 8-bit palletized format that is used for palletized RGB data when the processor processes ISDB-T data and for palletized YUV data when the processor processes BluRay data.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		P8 = DXGI.Format.P8,
		/// <summary>
		/// <para>
		/// 8-bit palletized format with 8 bits of alpha that is used for palletized YUV data when the processor processes BluRay data.
		/// </para>
		/// <para>
		/// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		A8P8 = DXGI.Format.A8P8,
		/// <summary>
		/// <para>
		/// A four-component, 16-bit unsigned-normalized integer format that supports 4 bits for each channel including alpha.
		/// </para>
		/// <para>
		/// <b>Direct3D 11.1:</b>  This value is not supported until Windows 8.
		/// </para>
		/// </summary>
		B4G4R4A4_UNorm = DXGI.Format.B4G4R4A4_UNorm
	}
}
