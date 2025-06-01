// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: May 10, 2025 3:39:42 PM
//

using Windows.Win32;

namespace Gorgon.Graphics.Imaging.Wic;

/// <summary>
/// WIC format GUIDs.
/// </summary>
internal static class PixelFormat
{
    /// <summary>
    /// Format is not important or no format.
    /// </summary>
    public static readonly Guid FormatDontCare = PInvoke.GUID_WICPixelFormatDontCare;
    /// <summary>
    /// 1 bit per pixel, palette indexed.
    /// </summary>
    public static readonly Guid Format1bppIndexed = PInvoke.GUID_WICPixelFormat1bppIndexed;
    /// <summary>
    /// 2 bits per pixel, palette indexed.
    /// </summary>
    public static readonly Guid Format2bppIndexed = PInvoke.GUID_WICPixelFormat2bppIndexed;
    /// <summary>
    /// 4 bits per pixel, palette indexed.
    /// </summary>
    public static readonly Guid Format4bppIndexed = PInvoke.GUID_WICPixelFormat4bppIndexed;
    /// <summary>
    /// 2 bits per pixel, palette indexed.
    /// </summary>
    public static readonly Guid Format8bppIndexed = PInvoke.GUID_WICPixelFormat8bppIndexed;
    /// <summary>
    /// Black and white (1 bit).
    /// </summary>
    public static readonly Guid FormatBlackAndWhite = PInvoke.GUID_WICPixelFormatBlackWhite;
    /// <summary>
    /// 2 bits per pixel, gray scale.
    /// </summary>
    public static readonly Guid Format2bppGray = PInvoke.GUID_WICPixelFormat2bppGray;
    /// <summary>
    /// 4 bits per pixel, gray scale.
    /// </summary>
    public static readonly Guid Format4bppGray = PInvoke.GUID_WICPixelFormat4bppGray;
    /// <summary>
    /// 8 bits per pixel, gray scale.
    /// </summary>
    public static readonly Guid Format8bppGray = PInvoke.GUID_WICPixelFormat8bppGray;
    /// <summary>
    /// 8 bits per pixel, alpha channel.
    /// </summary>
    public static readonly Guid Format8bppAlpha = PInvoke.GUID_WICPixelFormat8bppAlpha;
    /// <summary>
    /// 16 bits per pixel, 5 bit RGB channels, 1 bit unused.
    /// </summary>
    public static readonly Guid Format16bppBGR555 = PInvoke.GUID_WICPixelFormat16bppBGR555;
    /// <summary>
    /// 16 bits per pixel, 5 bit R and B channels, 6 bits green channel.
    /// </summary>
    public static readonly Guid Format16bppBGR565 = PInvoke.GUID_WICPixelFormat16bppBGR565;
    /// <summary>
    /// 16 bits per pixel, 5 bit RGB channels, 1 bit alpha channel.
    /// </summary>
    public static readonly Guid Format16bppBGRA5551 = PInvoke.GUID_WICPixelFormat16bppBGRA5551;
    /// <summary>
    /// 16 bits per pixel, gray scale.
    /// </summary>
    public static readonly Guid Format16bppGray = PInvoke.GUID_WICPixelFormat16bppGray;
    /// <summary>
    /// 24 bits per pixel, 8 bit BGR channels.
    /// </summary>
    public static readonly Guid Format24bppBGR = PInvoke.GUID_WICPixelFormat24bppBGR;
    /// <summary>
    /// 24 bits per pixel, 8 bit RGB channels.
    /// </summary>
    public static readonly Guid Format24bppRGB = PInvoke.GUID_WICPixelFormat24bppRGB;
    /// <summary>
    /// 32 bits per pixel, 8 bit BGR channels.
    /// </summary>
    public static readonly Guid Format32bppBGR = PInvoke.GUID_WICPixelFormat32bppBGR;
    /// <summary>
    /// 32 bits per pixel, 8 bit BGR channels, 8 bit alpha channel.
    /// </summary>
    public static readonly Guid Format32bppBGRA = PInvoke.GUID_WICPixelFormat32bppBGRA;
    /// <summary>
    /// 32 bits per pixel, 8 bit BGR channels, 8 bits alpha channel. Alpha is premultiplied.
    /// </summary>
    public static readonly Guid Format32bppPBGRA = PInvoke.GUID_WICPixelFormat32bppPBGRA;
    /// <summary>
    /// 32 bits per pixel, gray scale, single floating point.
    /// </summary>
    public static readonly Guid Format32bppGrayFloat = PInvoke.GUID_WICPixelFormat32bppGrayFloat;
    /// <summary>
    /// 32 bits per pixel, 8 bit RGB channels.
    /// </summary>
    public static readonly Guid Format32bppRGB = PInvoke.GUID_WICPixelFormat32bppRGB;
    /// <summary>
    /// 32 bits per pixel, 8 bit RGB channels, 8 bit alpha channel.
    /// </summary>
    public static readonly Guid Format32bppRGBA = PInvoke.GUID_WICPixelFormat32bppRGBA;
    /// <summary>
    /// 32 bits per pixel, 8 bit RGB channels, 8 bits alpha channel. Alpha is premultiplied.
    /// </summary>
    public static readonly Guid Format32bppPRGBA = PInvoke.GUID_WICPixelFormat32bppPRGBA;
    /// <summary>
    /// 48 bits per pixel, 16 bit RGB channels.
    /// </summary>
    public static readonly Guid Format48bppRGB = PInvoke.GUID_WICPixelFormat48bppRGB;
    /// <summary>
    /// 48 bits per pixel, 16 bit BGR channels.
    /// </summary>
    public static readonly Guid Format48bppBGR = PInvoke.GUID_WICPixelFormat48bppBGR;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels.
    /// </summary>
    public static readonly Guid Format64bppRGB = PInvoke.GUID_WICPixelFormat64bppRGB;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels, 16 bit alpha channel.
    /// </summary>
    public static readonly Guid Format64bppRGBA = PInvoke.GUID_WICPixelFormat64bppRGBA;
    /// <summary>
    /// 64 bits per pixel, 16 bit BGR channels, 16 bit alpha channel.
    /// </summary>
    public static readonly Guid Format64bppBGRA = PInvoke.GUID_WICPixelFormat64bppBGRA;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels, 16 bit alpha channel. Alpha is premultiplied.
    /// </summary>
    public static readonly Guid Format64bppPRGBA = PInvoke.GUID_WICPixelFormat64bppPRGBA;
    /// <summary>
    /// 64 bits per pixel, 16 bit BGR channels, 16 bit alpha channel. Alpha is premultiplied.
    /// </summary>
    public static readonly Guid Format64bppPBGRA = PInvoke.GUID_WICPixelFormat64bppPBGRA;
    /// <summary>
    /// 16 bits per pixel, grayscale, fixed point.
    /// </summary>
    public static readonly Guid Format16bppGrayFixedPoint = PInvoke.GUID_WICPixelFormat16bppGrayFixedPoint;
    /// <summary>
    /// 32 bits per pixel, 10 bit BGR channels, 2 bits unused.
    /// </summary>
    public static readonly Guid Format32bppBGR101010 = PInvoke.GUID_WICPixelFormat32bppBGR101010;
    /// <summary>
    /// 48 bits per pixel, 16 bit RGB channels, fixed point.
    /// </summary>
    public static readonly Guid Format48bppRGBFixedPoint = PInvoke.GUID_WICPixelFormat48bppRGBFixedPoint;
    /// <summary>
    /// 48 bits per pixel, 16 bit BGR channels, fixed point.
    /// </summary>
    public static readonly Guid Format48bppBGRFixedPoint = PInvoke.GUID_WICPixelFormat48bppBGRFixedPoint;
    /// <summary>
    /// 96 bits per pixel, 32 bit RGB channels, fixed point.
    /// </summary>
    public static readonly Guid Format96bppRGBFixedPoint = PInvoke.GUID_WICPixelFormat96bppRGBFixedPoint;
    /// <summary>
    /// 96 bits per pixel, 32 bit RGB channels, floating point.
    /// </summary>
    public static readonly Guid Format96bppRGBFloat = PInvoke.GUID_WICPixelFormat96bppRGBFloat;
    /// <summary>
    /// 128 bits per pixel, 32 bit RGB channels, 32 bit alpha channel, floating point.
    /// </summary>
    public static readonly Guid Format128bppRGBAFloat = PInvoke.GUID_WICPixelFormat128bppRGBAFloat;
    /// <summary>
    /// 128 bits per pixel, 32 bit RGB channels, 32 bit alpha channel, floating point. Alpha is premultiplied.
    /// </summary>
    public static readonly Guid Format128bppPRGBAFloat = PInvoke.GUID_WICPixelFormat128bppPRGBAFloat;
    /// <summary>
    /// 128 bits per pixel, 32 bit RGB channels, 32 bits unused, floating point
    /// </summary>
    public static readonly Guid Format128bppRGBFloat = PInvoke.GUID_WICPixelFormat128bppRGBFloat;
    /// <summary>
    /// 32 bits per pixel. CMYK format.
    /// </summary>
    public static readonly Guid Format32bppCMYK = PInvoke.GUID_WICPixelFormat32bppCMYK;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels, 16 bit alpha, fixed point.
    /// </summary>
    public static readonly Guid Format64bppRGBAFixedPoint = PInvoke.GUID_WICPixelFormat64bppRGBAFixedPoint;
    /// <summary>
    /// 64 bits per pixel, 16 bit BGR channels, 16 bit alpha, fixed point.
    /// </summary>
    public static readonly Guid Format64bppBGRAFixedPoint = PInvoke.GUID_WICPixelFormat64bppBGRAFixedPoint;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels, 16 bits unused, fixed point.
    /// </summary>
    public static readonly Guid Format64bppRGBFixedPoint = PInvoke.GUID_WICPixelFormat64bppRGBFixedPoint;
    /// <summary>
    /// 128 bits per pixel, 32 bit RGB channels, 32 bit alpha, fixed point.
    /// </summary>
    public static readonly Guid Format128bppRGBAFixedPoint = PInvoke.GUID_WICPixelFormat128bppRGBAFixedPoint;
    /// <summary>
    /// 128 bits per pixel, 32 bit RGB channels, 32 bits unused, fixed point.
    /// </summary>
    public static readonly Guid Format128bppRGBFixedPoint = PInvoke.GUID_WICPixelFormat128bppRGBFixedPoint;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels, 16 bit apha, half floating point.
    /// </summary>
    public static readonly Guid Format64bppRGBAHalf = PInvoke.GUID_WICPixelFormat64bppRGBAHalf;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels, 16 bit apha, half floating point. Alpha is premultiplied.
    /// </summary>
    public static readonly Guid Format64bppPRGBAHalf = PInvoke.GUID_WICPixelFormat64bppPRGBAHalf;
    /// <summary>
    /// 64 bits per pixel, 16 bit RGB channels, 16 bits unused, half floating point.
    /// </summary>
    public static readonly Guid Format64bppRGBHalf = PInvoke.GUID_WICPixelFormat64bppRGBHalf;
    /// <summary>
    /// 48 bits per pixel, 16 bit RGB channels, half floating point.
    /// </summary>
    public static readonly Guid Format48bppRGBHalf = PInvoke.GUID_WICPixelFormat48bppRGBHalf;
    /// <summary>
    /// 32 bits per pixel. Encodes three 16-bit floating-point values in 4 bytes, as follows: Three unsigned 8-bit mantissas for the R, G, and B channels, plus a shared 8-bit exponent. This format provides 16-bit floating-point precision in a smaller pixel representation.
    /// </summary>
    public static readonly Guid Format32bppRGBE = PInvoke.GUID_WICPixelFormat32bppRGBE;
    /// <summary>
    /// 16 bits per pixel, grayscale, half floating point.
    /// </summary>
    public static readonly Guid Format16bppGrayHalf = PInvoke.GUID_WICPixelFormat16bppGrayHalf;
    /// <summary>
    /// 32 bits per pixel, grayscale, fixed point.
    /// </summary>
    public static readonly Guid Format32bppGrayFixedPoint = PInvoke.GUID_WICPixelFormat32bppGrayFixedPoint;
    /// <summary>
    /// 32 bits per pixel, 10 bit RGB, 2 bit alpha channel.
    /// </summary>
    public static readonly Guid Format32bppRGBA1010102 = PInvoke.GUID_WICPixelFormat32bppRGBA1010102;
    /// <summary>
    /// 32 bits per pixel, 10 bit RGB, 2 bit alpha channel. 
    /// </summary>
    public static readonly Guid Format32bppRGBA1010102XR = PInvoke.GUID_WICPixelFormat32bppRGBA1010102XR;
    /// <summary>
    /// 32 bits per pixel, 10 bit RGB, 2 bit alpha channel. Red is in most significant bits.
    /// </summary>
    public static readonly Guid Format32bppR10G10B10A2 = PInvoke.GUID_WICPixelFormat32bppR10G10B10A2;
    /// <summary>
    /// 32 bits per pixel, 10 bit RGB, 2 bit alpha channel. Red is in most significant bits. HDR format.
    /// </summary>
    public static readonly Guid Format32bppR10G10B10A2HDR10 = PInvoke.GUID_WICPixelFormat32bppR10G10B10A2HDR10;
    /// <summary>
    /// 64 bits per pixel, CMYK format.
    /// </summary>
    public static readonly Guid Format64bppCMYK = PInvoke.GUID_WICPixelFormat64bppCMYK;
    /// <summary>
    /// 24 bits per pixel, 3 channels.
    /// </summary>
    public static readonly Guid Format24bpp3Channels = PInvoke.GUID_WICPixelFormat24bpp3Channels;
    /// <summary>
    /// 24 bits per pixel, 4 channels.
    /// </summary>
    public static readonly Guid Format32bpp4Channels = PInvoke.GUID_WICPixelFormat32bpp4Channels;
    /// <summary>
    /// 40 bits per pixel, 5 channels.
    /// </summary>
    public static readonly Guid Format40bpp5Channels = PInvoke.GUID_WICPixelFormat40bpp5Channels;
    /// <summary>
    /// 48 bits per pixel, 6 channels.
    /// </summary>
    public static readonly Guid Format48bpp6Channels = PInvoke.GUID_WICPixelFormat48bpp6Channels;
    /// <summary>
    /// 56 bits per pixel, 7 channels.
    /// </summary>
    public static readonly Guid Format56bpp7Channels = PInvoke.GUID_WICPixelFormat56bpp7Channels;
    /// <summary>
    /// 64 bits per pixel, 8 channels.
    /// </summary>
    public static readonly Guid Format64bpp8Channels = PInvoke.GUID_WICPixelFormat64bpp8Channels;
    /// <summary>
    /// 48 bits per pixel, 3 channels.
    /// </summary>
    public static readonly Guid Format48bpp3Channels = PInvoke.GUID_WICPixelFormat48bpp3Channels;
    /// <summary>
    /// 64 bits per pixel, 4 channels.
    /// </summary>
    public static readonly Guid Format64bpp4Channels = PInvoke.GUID_WICPixelFormat64bpp4Channels;
    /// <summary>
    /// 80 bits per pixel, 5 channels.
    /// </summary>
    public static readonly Guid Format80bpp5Channels = PInvoke.GUID_WICPixelFormat80bpp5Channels;
    /// <summary>
    /// 96 bits per pixel, 6 channels.
    /// </summary>
    public static readonly Guid Format96bpp6Channels = PInvoke.GUID_WICPixelFormat96bpp6Channels;
    /// <summary>
    /// 112 bits per pixel, 7 channels.
    /// </summary>
    public static readonly Guid Format112bpp7Channels = PInvoke.GUID_WICPixelFormat112bpp7Channels;
    /// <summary>
    /// 128 bits per pixel, 8 channels.
    /// </summary>
    public static readonly Guid Format128bpp8Channels = PInvoke.GUID_WICPixelFormat128bpp8Channels;
    /// <summary>
    /// 40 bits per pixel, CMYK format, with alpha channel.
    /// </summary>
    public static readonly Guid Format40bppCMYKAlpha = PInvoke.GUID_WICPixelFormat40bppCMYKAlpha;
    /// <summary>
    /// 80 bits per pixel, CMYK format, with alpha channel.
    /// </summary>
    public static readonly Guid Format80bppCMYKAlpha = PInvoke.GUID_WICPixelFormat80bppCMYKAlpha;
    /// <summary>
    /// 32 bits per pixel, 3 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format32bpp3ChannelsAlpha = PInvoke.GUID_WICPixelFormat32bpp3ChannelsAlpha;
    /// <summary>
    /// 40 bits per pixel, 4 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format40bpp4ChannelsAlpha = PInvoke.GUID_WICPixelFormat40bpp4ChannelsAlpha;
    /// <summary>
    /// 48 bits per pixel, 5 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format48bpp5ChannelsAlpha = PInvoke.GUID_WICPixelFormat48bpp5ChannelsAlpha;
    /// <summary>
    /// 56 bits per pixel, 6 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format56bpp6ChannelsAlpha = PInvoke.GUID_WICPixelFormat56bpp6ChannelsAlpha;
    /// <summary>
    /// 64 bits per pixel, 7 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format64bpp7ChannelsAlpha = PInvoke.GUID_WICPixelFormat64bpp7ChannelsAlpha;
    /// <summary>
    /// 72 bits per pixel, 8 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format72bpp8ChannelsAlpha = PInvoke.GUID_WICPixelFormat72bpp8ChannelsAlpha;
    /// <summary>
    /// 64 bits per pixel, 3 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format643ChannelsAlpha = PInvoke.GUID_WICPixelFormat64bpp3ChannelsAlpha;
    /// <summary>
    /// 80 bits per pixel, 4 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format80bpp4ChannelsAlpha = PInvoke.GUID_WICPixelFormat80bpp4ChannelsAlpha;
    /// <summary>
    /// 96 bits per pixel, 5 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format96bpp5ChannelsAlpha = PInvoke.GUID_WICPixelFormat96bpp5ChannelsAlpha;
    /// <summary>
    /// 112 bits per pixel, 6 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format112bpp6ChannelsAlpha = PInvoke.GUID_WICPixelFormat112bpp6ChannelsAlpha;
    /// <summary>
    /// 128 bits per pixel, 7 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format128bpp7ChannelsAlpha = PInvoke.GUID_WICPixelFormat128bpp7ChannelsAlpha;
    /// <summary>
    /// 144 bits per pixel, 8 channels, with alpha channel.
    /// </summary>
    public static readonly Guid Format144bpp8ChannelsAlpha = PInvoke.GUID_WICPixelFormat144bpp8ChannelsAlpha;
    /// <summary>
    /// 
    /// </summary>
    public static readonly Guid Format8bppY = PInvoke.GUID_WICPixelFormat8bppY;
    /// <summary>
    /// 
    /// </summary>
    public static readonly Guid Format8bppCb = PInvoke.GUID_WICPixelFormat8bppCb;
    /// <summary>
    /// 
    /// </summary>
    public static readonly Guid Format8bppCr = PInvoke.GUID_WICPixelFormat8bppCr;
    /// <summary>
    /// 
    /// </summary>
    public static readonly Guid Format16bppCbCr = PInvoke.GUID_WICPixelFormat16bppCbCr;
    /// <summary>
    /// 
    /// </summary>
    public static readonly Guid Format16bppyQuantizedDctCofficients = PInvoke.GUID_WICPixelFormat16bppYQuantizedDctCoefficients;
    /// <summary>
    /// 
    /// </summary>
    public static readonly Guid Format16bppCbQuantizedDctCoefficients = PInvoke.GUID_WICPixelFormat16bppCbQuantizedDctCoefficients;
    /// <summary>
    /// 
    /// </summary>
    public static readonly Guid Format16bppCrQuantizedDctCoefficients = PInvoke.GUID_WICPixelFormat16bppCrQuantizedDctCoefficients;
}
