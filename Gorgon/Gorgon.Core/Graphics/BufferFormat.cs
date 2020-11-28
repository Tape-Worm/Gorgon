#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: November 8, 2017 2:10:42 PM
// 
#endregion

// ReSharper disable InconsistentNaming
namespace Gorgon.Graphics
{
    /// <summary>
    /// Buffer data formats.
    /// </summary>
    public enum BufferFormat
    {
        /// <summary>
        /// <para>
        /// The format is not known.
        /// </para>
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// <para>
        /// A four-component, 128-bit typeless format that supports 32 bits per channel including alpha. ¹
        /// </para>
        /// </summary>
        R32G32B32A32_Typeless = 1,
        /// <summary>
        /// <para>
        /// A four-component, 128-bit floating-point format that supports 32 bits per channel including alpha. 
        /// </para>
        /// </summary>
        R32G32B32A32_Float = 2,
        /// <summary>
        /// <para>
        /// A four-component, 128-bit unsigned-integer format that supports 32 bits per channel including alpha. ¹
        /// </para>
        /// </summary>
        R32G32B32A32_UInt = 3,
        /// <summary>
        /// <para>
        /// A four-component, 128-bit signed-integer format that supports 32 bits per channel including alpha. ¹
        /// </para>
        /// </summary>
        R32G32B32A32_SInt = 4,
        /// <summary>
        /// <para>
        /// A three-component, 96-bit typeless format that supports 32 bits per color channel.
        /// </para>
        /// </summary>
        R32G32B32_Typeless = 5,
        /// <summary>
        /// <para>
        /// A three-component, 96-bit floating-point format that supports 32 bits per color channel.
        /// </para>
        /// </summary>
        R32G32B32_Float = 6,
        /// <summary>
        /// <para>
        /// A three-component, 96-bit unsigned-integer format that supports 32 bits per color channel.
        /// </para>
        /// </summary>
        R32G32B32_UInt = 7,
        /// <summary>
        /// <para>
        /// A three-component, 96-bit signed-integer format that supports 32 bits per color channel.
        /// </para>
        /// </summary>
        R32G32B32_SInt = 8,
        /// <summary>
        /// <para>
        /// A four-component, 64-bit typeless format that supports 16 bits per channel including alpha.
        /// </para>
        /// </summary>
        R16G16B16A16_Typeless = 9,
        /// <summary>
        /// <para>
        /// A four-component, 64-bit floating-point format that supports 16 bits per channel including alpha.
        /// </para>
        /// </summary>
        R16G16B16A16_Float = 10,
        /// <summary>
        /// <para>
        /// A four-component, 64-bit unsigned-normalized-integer format that supports 16 bits per channel including alpha.
        /// </para>
        /// </summary>
        R16G16B16A16_UNorm = 11,
        /// <summary>
        /// <para>
        /// A four-component, 64-bit unsigned-integer format that supports 16 bits per channel including alpha.
        /// </para>
        /// </summary>
        R16G16B16A16_UInt = 12,
        /// <summary>
        /// <para>
        /// A four-component, 64-bit signed-normalized-integer format that supports 16 bits per channel including alpha.
        /// </para>
        /// </summary>
        R16G16B16A16_SNorm = 13,
        /// <summary>
        /// <para>
        /// A four-component, 64-bit signed-integer format that supports 16 bits per channel including alpha.
        /// </para>
        /// </summary>
        R16G16B16A16_SInt = 14,
        /// <summary>
        /// <para>
        /// A two-component, 64-bit typeless format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </para>
        /// </summary>
        R32G32_Typeless = 15,
        /// <summary>
        /// <para>
        /// A two-component, 64-bit floating-point format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </para>
        /// </summary>
        R32G32_Float = 16,
        /// <summary>
        /// <para>
        /// A two-component, 64-bit unsigned-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </para>
        /// </summary>
        R32G32_UInt = 17,
        /// <summary>
        /// <para>
        /// A two-component, 64-bit signed-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </para>
        /// </summary>
        R32G32_SInt = 18,
        /// <summary>
        /// <para>
        /// A two-component, 64-bit typeless format that supports 32 bits for the red channel, 8 bits for the green channel, and 24 bits are unused.
        /// </para>
        /// </summary>
        R32G8X24_Typeless = 19,
        /// <summary>
        /// <para>
        /// A 32-bit floating-point component, and two unsigned-integer components (with an additional 32 bits). This format supports 32-bit depth, 8-bit stencil, and 24 bits are unused.⁵
        /// </para>
        /// </summary>
        D32_Float_S8X24_UInt = 20,
        /// <summary>
        /// <para>
        /// A 32-bit floating-point component, and two typeless components (with an additional 32 bits). This format supports 32-bit red channel, 8 bits are unused, and 24 bits are unused.⁵
        /// </para>
        /// </summary>
        R32_Float_X8X24_Typeless = 21,
        /// <summary>
        /// <para>
        /// A 32-bit typeless component, and two unsigned-integer components (with an additional 32 bits). This format has 32 bits unused, 8 bits for green channel, and 24 bits are unused.
        /// </para>
        /// </summary>
        X32_Typeless_G8X24_UInt = 22,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit typeless format that supports 10 bits for each color and 2 bits for alpha.
        /// </para>
        /// </summary>
        R10G10B10A2_Typeless = 23,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 10 bits for each color and 2 bits for alpha.
        /// </para>
        /// </summary>
        R10G10B10A2_UNorm = 24,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-integer format that supports 10 bits for each color and 2 bits for alpha.
        /// </para>
        /// </summary>
        R10G10B10A2_UInt = 25,
        /// <summary>
        /// <para>
        /// Three partial-precision floating-point numbers encoded into a single 32-bit value (a variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent).
        /// </para>
        /// <para>
        /// There are no sign bits, and there is a 5-bit biased (15) exponent for each channel, 6-bit mantissa  for R and G, and a 5-bit mantissa for B, as shown in the following illustration.
        /// </para>
        /// </summary>
        R11G11B10_Float = 26,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit typeless format that supports 8 bits per channel including alpha.
        /// </para>
        /// </summary>
        R8G8B8A8_Typeless = 27,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits per channel including alpha.
        /// </para>
        /// </summary>
        R8G8B8A8_UNorm = 28,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized integer sRGB format that supports 8 bits per channel including alpha.
        /// </para>
        /// </summary>
        R8G8B8A8_UNorm_SRgb = 29,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-integer format that supports 8 bits per channel including alpha.
        /// </para>
        /// </summary>
        R8G8B8A8_UInt = 30,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit signed-normalized-integer format that supports 8 bits per channel including alpha.
        /// </para>
        /// </summary>
        R8G8B8A8_SNorm = 31,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit signed-integer format that supports 8 bits per channel including alpha.
        /// </para>
        /// </summary>
        R8G8B8A8_SInt = 32,
        /// <summary>
        /// <para>
        /// A two-component, 32-bit typeless format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </para>
        /// </summary>
        R16G16_Typeless = 33,
        /// <summary>
        /// <para>
        /// A two-component, 32-bit floating-point format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </para>
        /// </summary>
        R16G16_Float = 34,
        /// <summary>
        /// <para>
        /// A two-component, 32-bit unsigned-normalized-integer format that supports 16 bits each for the green and red channels.
        /// </para>
        /// </summary>
        R16G16_UNorm = 35,
        /// <summary>
        /// <para>
        /// A two-component, 32-bit unsigned-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </para>
        /// </summary>
        R16G16_UInt = 36,
        /// <summary>
        /// <para>
        /// A two-component, 32-bit signed-normalized-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </para>
        /// </summary>
        R16G16_SNorm = 37,
        /// <summary>
        /// <para>
        /// A two-component, 32-bit signed-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </para>
        /// </summary>
        R16G16_SInt = 38,
        /// <summary>
        /// <para>
        /// A single-component, 32-bit typeless format that supports 32 bits for the red channel.
        /// </para>
        /// </summary>
        R32_Typeless = 39,
        /// <summary>
        /// <para>
        /// A single-component, 32-bit floating-point format that supports 32 bits for depth.
        /// </para>
        /// </summary>
        D32_Float = 40,
        /// <summary>
        /// <para>
        /// A single-component, 32-bit floating-point format that supports 32 bits for the red channel.
        /// </para>
        /// </summary>
        R32_Float = 41,
        /// <summary>
        /// <para>
        /// A single-component, 32-bit unsigned-integer format that supports 32 bits for the red channel.
        /// </para>
        /// </summary>
        R32_UInt = 42,
        /// <summary>
        /// <para>
        /// A single-component, 32-bit signed-integer format that supports 32 bits for the red channel.
        /// </para>
        /// </summary>
        R32_SInt = 43,
        /// <summary>
        /// <para>
        /// A two-component, 32-bit typeless format that supports 24 bits for the red channel and 8 bits for the green channel.
        /// </para>
        /// </summary>
        R24G8_Typeless = 44,
        /// <summary>
        /// <para>
        /// A 32-bit z-buffer format that supports 24 bits for depth and 8 bits for stencil.
        /// </para>
        /// </summary>
        D24_UNorm_S8_UInt = 45,
        /// <summary>
        /// <para>
        /// A 32-bit format, that contains a 24 bit, single-component, unsigned-normalized integer, with an additional typeless 8 bits. This format has 24 bits red channel and 8 bits unused.
        /// </para>
        /// </summary>
        R24_UNorm_X8_Typeless = 46,
        /// <summary>
        /// <para>
        /// A 32-bit format, that contains a 24 bit, single-component, typeless format,  with an additional 8 bit unsigned integer component. This format has 24 bits unused and 8 bits green channel.
        /// </para>
        /// </summary>
        X24_Typeless_G8_UInt = 47,
        /// <summary>
        /// <para>
        /// A two-component, 16-bit typeless format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </para>
        /// </summary>
        R8G8_Typeless = 48,
        /// <summary>
        /// <para>
        /// A two-component, 16-bit unsigned-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </para>
        /// </summary>
        R8G8_UNorm = 49,
        /// <summary>
        /// <para>
        /// A two-component, 16-bit unsigned-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </para>
        /// </summary>
        R8G8_UInt = 50,
        /// <summary>
        /// <para>
        /// A two-component, 16-bit signed-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </para>
        /// </summary>
        R8G8_SNorm = 51,
        /// <summary>
        /// <para>
        /// A two-component, 16-bit signed-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </para>
        /// </summary>
        R8G8_SInt = 52,
        /// <summary>
        /// <para>
        /// A single-component, 16-bit typeless format that supports 16 bits for the red channel.
        /// </para>
        /// </summary>
        R16_Typeless = 53,
        /// <summary>
        /// <para>
        /// A single-component, 16-bit floating-point format that supports 16 bits for the red channel.
        /// </para>
        /// </summary>
        R16_Float = 54,
        /// <summary>
        /// <para>
        /// A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for depth.
        /// </para>
        /// </summary>
        D16_UNorm = 55,
        /// <summary>
        /// <para>
        /// A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for the red channel.
        /// </para>
        /// </summary>
        R16_UNorm = 56,
        /// <summary>
        /// <para>
        /// A single-component, 16-bit unsigned-integer format that supports 16 bits for the red channel.
        /// </para>
        /// </summary>
        R16_UInt = 57,
        /// <summary>
        /// <para>
        /// A single-component, 16-bit signed-normalized-integer format that supports 16 bits for the red channel.
        /// </para>
        /// </summary>
        R16_SNorm = 58,
        /// <summary>
        /// <para>
        /// A single-component, 16-bit signed-integer format that supports 16 bits for the red channel.
        /// </para>
        /// </summary>
        R16_SInt = 59,
        /// <summary>
        /// <para>
        /// A single-component, 8-bit typeless format that supports 8 bits for the red channel.
        /// </para>
        /// </summary>
        R8_Typeless = 60,
        /// <summary>
        /// <para>
        /// A single-component, 8-bit unsigned-normalized-integer format that supports 8 bits for the red channel.
        /// </para>
        /// </summary>
        R8_UNorm = 61,
        /// <summary>
        /// <para>
        /// A single-component, 8-bit unsigned-integer format that supports 8 bits for the red channel.
        /// </para>
        /// </summary>
        R8_UInt = 62,
        /// <summary>
        /// <para>
        /// A single-component, 8-bit signed-normalized-integer format that supports 8 bits for the red channel.
        /// </para>
        /// </summary>
        R8_SNorm = 63,
        /// <summary>
        /// <para>
        /// A single-component, 8-bit signed-integer format that supports 8 bits for the red channel.
        /// </para>
        /// </summary>
        R8_SInt = 64,
        /// <summary>
        /// <para>
        /// A single-component, 8-bit unsigned-normalized-integer format for alpha only.
        /// </para>
        /// </summary>
        A8_UNorm = 65,
        /// <summary>
        /// <para>
        /// A single-component, 1-bit unsigned-normalized integer format that supports 1 bit for the red channel. ².
        /// </para>
        /// </summary>
        R1_UNorm = 66,
        /// <summary>
        /// <para>
        /// Three partial-precision floating-point numbers encoded into a single 32-bit value all sharing the same 5-bit exponent (variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent).
        /// </para>
        /// <para>
        /// There is no sign bit, and there is a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel, as shown in the following illustration.
        /// </para>
        /// </summary>
        R9G9B9E5_Sharedexp = 67,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the UYVY format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and the G8 values are unique to each pixel. ³
        /// </para>
        /// </summary>
        R8G8_B8G8_UNorm = 68,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the YUY2 format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and the G8 values are unique to each pixel. ³
        /// </para>
        /// </summary>
        G8R8_G8B8_UNorm = 69,
        /// <summary>
        /// <para>
        /// Four-component typeless block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC1_Typeless = 70,
        /// <summary>
        /// <para>
        /// Four-component block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC1_UNorm = 71,
        /// <summary>
        /// <para>
        /// Four-component block-compression format for sRGB data. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC1_UNorm_SRgb = 72,
        /// <summary>
        /// <para>
        /// Four-component typeless block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC2_Typeless = 73,
        /// <summary>
        /// <para>
        /// Four-component block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC2_UNorm = 74,
        /// <summary>
        /// <para>
        /// Four-component block-compression format for sRGB data. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC2_UNorm_SRgb = 75,
        /// <summary>
        /// <para>
        /// Four-component typeless block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC3_Typeless = 76,
        /// <summary>
        /// <para>
        /// Four-component block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC3_UNorm = 77,
        /// <summary>
        /// <para>
        /// Four-component block-compression format for sRGB data. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC3_UNorm_SRgb = 78,
        /// <summary>
        /// <para>
        /// One-component typeless block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC4_Typeless = 79,
        /// <summary>
        /// <para>
        /// One-component block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC4_UNorm = 80,
        /// <summary>
        /// <para>
        /// One-component block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC4_SNorm = 81,
        /// <summary>
        /// <para>
        /// Two-component typeless block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC5_Typeless = 82,
        /// <summary>
        /// <para>
        /// Two-component block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC5_UNorm = 83,
        /// <summary>
        /// <para>
        /// Two-component block-compression format. For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC5_SNorm = 84,
        /// <summary>
        /// <para>
        /// A three-component, 16-bit unsigned-normalized-integer format that supports 5 bits for blue, 6 bits for green, and 5 bits for red.
        /// </para>
        /// </summary>
        B5G6R5_UNorm = 85,
        /// <summary>
        /// <para>
        /// A four-component, 16-bit unsigned-normalized-integer format that supports 5 bits for each color channel and 1-bit alpha.
        /// </para>
        /// </summary>
        B5G5R5A1_UNorm = 86,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8-bit alpha.
        /// </para>
        /// </summary>
        B8G8R8A8_UNorm = 87,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8 bits unused.
        /// </para>
        /// </summary>
        B8G8R8X8_UNorm = 88,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit 2.8-biased fixed-point format that supports 10 bits for each color channel and 2-bit alpha.
        /// </para>
        /// </summary>
        R10G10B10_Xr_Bias_A2_UNorm = 89,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit typeless format that supports 8 bits for each channel including alpha. ⁴
        /// </para>
        /// </summary>
        B8G8R8A8_Typeless = 90,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each channel including alpha. ⁴
        /// </para>
        /// </summary>
        B8G8R8A8_UNorm_SRgb = 91,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit typeless format that supports 8 bits for each color channel, and 8 bits are unused. ⁴
        /// </para>
        /// </summary>
        B8G8R8X8_Typeless = 92,
        /// <summary>
        /// <para>
        /// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each color channel, and 8 bits are unused. ⁴
        /// </para>
        /// </summary>
        B8G8R8X8_UNorm_SRgb = 93,
        /// <summary>
        /// <para>
        /// A typeless block-compression format. ⁴ For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC6H_Typeless = 94,
        /// <summary>
        /// <para>
        /// A block-compression format. ⁴ For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC6H_Uf16 = 95,
        /// <summary>
        /// <para>
        /// A block-compression format. ⁴ For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC6H_Sf16 = 96,
        /// <summary>
        /// <para>
        /// A typeless block-compression format. ⁴ For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC7_Typeless = 97,
        /// <summary>
        /// <para>
        /// A block-compression format. ⁴ For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC7_UNorm = 98,
        /// <summary>
        /// <para>
        /// A block-compression format. ⁴ For information about block-compression formats, see 
        /// </para>
        /// </summary>
        BC7_UNorm_SRgb = 99,
        /// <summary>
        /// <para>
        /// Most common YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. Supported view types are SRV, RTV, and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is V-&gt;R8,
        /// </para>
        /// <para>
        /// U-&gt;G8,
        /// </para>
        /// <para>
        /// Y-&gt;B8,
        /// </para>
        /// <para>
        /// and A-&gt;A8.
        /// </para>
        /// </summary>
        AYUV = 100,
        /// <summary>
        /// <para>
        /// 10-bit per channel packed YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT. For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is U-&gt;R10,
        /// </para>
        /// <para>
        /// Y-&gt;G10,
        /// </para>
        /// <para>
        /// V-&gt;B10,
        /// </para>
        /// <para>
        /// and A-&gt;A2.
        /// </para>
        /// </summary>
        Y410 = 101,
        /// <summary>
        /// <para>
        /// 16-bit per channel packed YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is U-&gt;R16,
        /// </para>
        /// <para>
        /// Y-&gt;G16,
        /// </para>
        /// <para>
        /// V-&gt;B16,
        /// </para>
        /// <para>
        /// and A-&gt;A16.
        /// </para>
        /// </summary>
        Y416 = 102,
        /// <summary>
        /// <para>
        /// Most common YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT. Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y-&gt;R8. For chrominance data view, the mapping to the view channel is U-&gt;R8 and
        /// </para>
        /// <para>
        /// V-&gt;G8.
        /// </para>
        /// </summary>
        NV12 = 103,
        /// <summary>
        /// <para>
        /// 10-bit per channel planar YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT. The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 10-bit format that uses 16 bits). If required, application shader code would have to enforce this manually.  From the runtime's point of view, DXGI_FORMAT_P010 is no different than DXGI_FORMAT_P016. Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. For UAVs, an additional valid chrominance data view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y-&gt;R16. For chrominance data view, the mapping to the view channel is U-&gt;R16 and
        /// </para>
        /// <para>
        /// V-&gt;G16.
        /// </para>
        /// </summary>
        P010 = 104,
        /// <summary>
        /// <para>
        /// 16-bit per channel planar YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT. Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. For UAVs, an additional valid chrominance data view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y-&gt;R16. For chrominance data view, the mapping to the view channel is U-&gt;R16 and
        /// </para>
        /// <para>
        /// V-&gt;G16.
        /// </para>
        /// </summary>
        P016 = 105,
        /// <summary>
        /// <para>
        /// 8-bit per channel planar YUV 4:2:0 video resource format. This format is subsampled where each pixel has its own Y value, but each 2x2 pixel block shares a single U and V value. The runtime requires that the width and height of all resources that are created with this format are multiples of 2. The runtime also requires that the left, right, top, and bottom members of any RECT that are used for this format are multiples of 2. This format differs from DXGI_FORMAT_NV12 in that the layout of the data within the resource is completely opaque to applications. Applications cannot use the CPU to map the resource and then access the data within the resource. You cannot use shaders with this format. Because of this behavior, legacy hardware that supports a non-NV12 4:2:0 layout (for example, YV12, and so on) can be used. Also, new hardware that has a 4:2:0 implementation better than NV12 can be used when the application does not need the data to be in a standard layout. 
        /// </para>
        /// </summary>
        Opaque420 = 106,
        /// <summary>
        /// <para>
        /// Most common YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0-&gt;R8,
        /// </para>
        /// <para>
        /// U0-&gt;G8,
        /// </para>
        /// <para>
        /// Y1-&gt;B8,
        /// </para>
        /// <para>
        /// and V0-&gt;A8.
        /// </para>
        /// </summary>
        YUY2 = 107,
        /// <summary>
        /// <para>
        /// 10-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 10-bit format that uses 16 bits). If required, application shader code would have to enforce this manually.  From the runtime's point of view, DXGI_FORMAT_Y210 is no different than DXGI_FORMAT_Y216. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0-&gt;R16,
        /// </para>
        /// <para>
        /// U-&gt;G16,
        /// </para>
        /// <para>
        /// Y1-&gt;B16,
        /// </para>
        /// <para>
        /// and V-&gt;A16.
        /// </para>
        /// </summary>
        Y210 = 108,
        /// <summary>
        /// <para>
        /// 16-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0-&gt;R16,
        /// </para>
        /// <para>
        /// U-&gt;G16,
        /// </para>
        /// <para>
        /// Y1-&gt;B16,
        /// </para>
        /// <para>
        /// and V-&gt;A16.
        /// </para>
        /// </summary>
        Y216 = 109,
        /// <summary>
        /// <para>
        /// Most common planar YUV 4:1:1 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT. Valid chrominance data view formats (width and height are each 1/4 of luminance view) for this video resource format are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y-&gt;R8. For chrominance data view, the mapping to the view channel is U-&gt;R8 and
        /// </para>
        /// <para>
        /// V-&gt;G8.
        /// </para>
        /// </summary>
        NV11 = 110,
        /// <summary>
        /// <para>
        /// 4-bit palletized YUV format that is commonly used for DVD subpicture.
        /// </para>
        /// </summary>
        AI44 = 111,
        /// <summary>
        /// <para>
        /// 4-bit palletized YUV format that is commonly used for DVD subpicture.
        /// </para>
        /// </summary>
        IA44 = 112,
        /// <summary>
        /// <para>
        /// 8-bit palletized format that is used for palletized RGB data when the processor processes ISDB-T data and for palletized YUV data when the processor processes BluRay data.
        /// </para>
        /// </summary>
        P8 = 113,
        /// <summary>
        /// <para>
        /// 8-bit palletized format with 8 bits of alpha that is used for palletized YUV data when the processor processes BluRay data.
        /// </para>
        /// </summary>
        A8P8 = 114,
        /// <summary>
        /// <para>
        /// A four-component, 16-bit unsigned-normalized integer format that supports 4 bits for each channel including alpha.
        /// </para>
        /// </summary>
        B4G4R4A4_UNorm = 115,
        /// <summary>
        /// <para>
        /// A video format; an 8-bit version of a hybrid planar 4:2:2 format.
        /// </para>
        /// </summary>
        P208 = 130,
        /// <summary>
        /// <para>
        /// An 8 bit YCbCrA 4:4 rendering format. 
        /// </para>
        /// </summary>
        V208 = 131,
        /// <summary>
        /// <para>
        /// An 8 bit YCbCrA 4:4:4:4 rendering format. 
        /// </para>
        /// </summary>
        V408 = 132
    }
}
