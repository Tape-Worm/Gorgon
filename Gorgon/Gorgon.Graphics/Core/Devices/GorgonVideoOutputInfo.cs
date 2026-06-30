
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, January 7, 2016 7:15:06 PM
// 

using System.Buffers;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Windows.Web.AtomPub;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An enumeration that indicates how the back buffers should be rotated to fit the physical rotation of a monitor
/// </summary>
public enum RotationMode
{
    /// <summary>
    /// No rotation mode specified.
    /// </summary>
    Unspecified = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_UNSPECIFIED,
    /// <summary>
    /// No rotation.
    /// </summary>
    Identity = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_IDENTITY,
    /// <summary>
    /// Rotated 90 degrees.
    /// </summary>
    Rotate90 = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE90,
    /// <summary>
    /// Rotated 180 degrees.
    /// </summary>
    Rotate180 = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE180,
    /// <summary>
    /// Rotated 270 degrees.
    /// </summary>
    Rotate270 = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE270
}

/// <summary>
/// Color space types.
/// </summary>
public enum ColorSpace
{
    /// <summary>
    /// This is the standard definition for sRGB. 
    /// </summary>
    RGBFullG22NoneP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_FULL_G22_NONE_P709,

    /// <summary>
    /// This is the standard definition for scRGB, and is usually used with 16-bit integer, 16-bit floating point, or 32-bit floating point color channels.
    /// </summary>
    RGBFullG10NoneP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_FULL_G10_NONE_P709,

    /// <summary>
    /// <para>
    /// This is the standard definition for ITU-R Recommendation BT.709. Note that due to the inclusion of a linear segment, the transfer curve looks similar to a pure exponential gamma of 1.9.
    /// </para>
    /// <para>
    /// This is usually used with 8 or 10 bit color channels.
    ///</para>
    /// </summary>
    RGBStudioG22NoneP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_STUDIO_G22_NONE_P709,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    RGBStudioG22NoneP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_STUDIO_G22_NONE_P2020,

    /// <summary>
    /// Reserved.
    /// </summary>
#pragma warning disable CA1700 // Do not name enum values 'Reserved'
    Reserved = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RESERVED,
#pragma warning restore CA1700 // Do not name enum values 'Reserved'

    /// <summary>
    /// This definition is commonly used for JPG, and is usually used with 8, 10, or 12 bit color channels.
    /// </summary>
    YCBCRFullG22NoneP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_FULL_G22_NONE_P709_X601,

    /// <summary>
    /// This definition is commonly used for MPEG2, and is usually used with 8, 10, or 12 bit color channels.
    /// </summary>
    YCBCRStudioG22LeftP601 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G22_LEFT_P601,

    /// <summary>
    /// This is sometimes used for H.264 camera capture, and is usually used with 8, 10, or 12 bit color channels.
    /// </summary>
    YCBCRFullG22LeftP601 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_FULL_G22_LEFT_P601,

    /// <summary>
    /// This definition is commonly used for H.264 and HEVC, and is usually used with 8, 10, or 12 bit color channels.
    /// </summary>
    YCBCRStudioG22LeftP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G22_LEFT_P709,

    /// <summary>
    /// This is sometimes used for H.264 camera capture, and is usually used with 8, 10, or 12 bit color channels.
    /// </summary>
    YCBCRFullG22LeftP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_FULL_G22_LEFT_P709,

    /// <summary>
    /// This definition may be used by HEVC, and is usually used with 10 or 12 bit color channels
    /// </summary>
    YCBCRStudioG22LeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G22_LEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRFullG22LeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_FULL_G22_LEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    RGBFullG2084NoneP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_FULL_G2084_NONE_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRStudioG2084LeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G2084_LEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    RGBStudioG2084NoneP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_STUDIO_G2084_NONE_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRStudioG22TopLeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G22_TOPLEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRStudioG2084TopLeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G2084_TOPLEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    RGBFullG22NoneP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_FULL_G22_NONE_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRStudioGHLGTopLeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_GHLG_TOPLEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRFullGHLGTopLeftP20202 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_FULL_GHLG_TOPLEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    RGBStudioG24NoneP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_STUDIO_G24_NONE_P709,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    RGBStudioG24NoneP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_RGB_STUDIO_G24_NONE_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRStudioG24LeftP709 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G24_LEFT_P709,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRStudioG24LeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G24_LEFT_P2020,

    /// <summary>
    /// This is usually used with 10 or 12 bit color channels.
    /// </summary>
    YCBCRStudioG24TopLeftP2020 = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_YCBCR_STUDIO_G24_TOPLEFT_P2020,

    /// <summary>
    /// A custom definition is used.
    /// </summary>
    Custom = DXGI_COLOR_SPACE_TYPE.DXGI_COLOR_SPACE_CUSTOM
}

/// <summary>
/// Provides information about an output on a <see cref="GorgonVideoAdapterInfo"/> object.
/// </summary>
/// <param name="Name">The friendly name of the video output.</param>
/// <param name="Index">The index of the output on the video adapter.
/// <para>
/// <note type="warning">
/// <para>
/// This is not the same as the index in the <see cref="GorgonVideoAdapterInfo.Outputs"/> list on the <see cref="GorgonVideoAdapterInfo"/>
/// </para>
/// </note>
/// </para>
/// </param>
/// <param name="DeviceHandle">The native handle for the output device.</param>
/// <param name="Bounds">The physical boundaries of the output device, typically the desktop boundaries.
/// <para>
/// The desktop coordinates depend on the dots per inch (DPI) of the desktop. For more information about writing DPI-aware Win32 applications, 
/// see <a target="_blank" href="https://msdn.microsoft.com/en-us/library/bb173068.aspx">High DPI</a>.
/// </para>
/// </param>
/// <param name="IsAttached">Flag to indicate whether the device is attached or not.</param>
/// <param name="Rotation">The value that indicates how the output image is rotated by the output</param>
/// <param name="VideoModes">The list of video modes supported by this output</param>
/// <param name="BitsPerColorChannel">The value that indicates the number of bits per color channel.</param>
/// <param name="ColorSpace">The value that indicates the current advanced color capabilities of the display attached to this output. Specifically, whether it's capable of reproducing color and luminance 
/// values outside of the sRGB color space.
/// <para>
/// A value of <see cref="ColorSpace.RGBFullG22NoneP709"/> indicates that the display is limited to SDR/sRGB. A value of <see cref="ColorSpace.RGBFullG2084NoneP2020"/> indicates that the display 
/// supports advanced color capabilities. <see cref="ColorSpace.RGBFullG10NoneP709"/> is currently not a color space that displays use; it's simply an intermediary swap-chain color space.
/// </para>
/// </param>
/// <param name="WhitePoint">The value that indicates the white point, in xy coordinates, of the display attached to this output.
/// <para>
/// This value will usually come from the EDID of the corresponding display or sometimes from an override.
/// </para>
/// </param>
/// <param name="RedPrimary">The value that indicates the red color primary, in xy coordinates, of the display attached to this output.
/// <para>
/// This value will usually come from the EDID of the corresponding display or sometimes from an override.
/// </para>
/// </param>
/// <param name="GreenPrimary">The value that indicates the green color primary, in xy coordinates, of the display attached to this output.
/// <para>
/// This value will usually come from the EDID of the corresponding display or sometimes from an override.
/// </para>
/// </param>
/// <param name="BluePrimary">The value that indicates the blue color primary, in xy coordinates, of the display attached to this output.
/// <para>
/// This value will usually come from the EDID of the corresponding display or sometimes from an override.
/// </para>
/// </param>
/// <param name="MinimumLuminance">The value that indicates the minimum luminance, in nits, that the display attached to this output is capable of rendering.
/// <para>
/// Content should not exceed this minimum value for optimal rendering. This value will usually come from the EDID of the corresponding display or sometimes from an override.
/// </para>
/// </param>
/// <param name="MaximumLuminance">The value that indicates the maximum luminance, in nits, that the display attached to this output is capable of rendering.
/// <para>
/// This value is likely only valid for a small area of the panel. Content should not exceed this minimum value for optimal rendering. This value will usually come from the EDID of the corresponding 
/// display or sometimes from an override.
/// </para>
/// </param>
/// <param name="MaximumFullFrameLuminance">The value that indicates the maximum luminance, in nits, that the display attached to this output is capable of rendering.
/// <para>
/// Unlike <see cref="MaximumLuminance"/>, this value is valid for a color that fills the entire area of the panel. Content should not exceed this value across the entire panel for optimal rendering. This 
/// value will usually come from the EDID of the corresponding display or sometimes from an override.
/// </para>
/// </param>
/// <remarks>
/// <para>
/// An output is typically a physical connection between the video adapter and another device, such as a monitor.
/// </para>
/// </remarks>
/// <seealso cref="GorgonVideoAdapterInfo"/>
public record class GorgonVideoOutputInfo(string Name, 
                                          int Index, 
                                          nint DeviceHandle, 
                                          GorgonRectangle Bounds, 
                                          bool IsAttached,
                                          RotationMode Rotation,
                                          IReadOnlyList<GorgonVideoMode> VideoModes,
                                          int BitsPerColorChannel,
                                          ColorSpace ColorSpace,
                                          Vector2 WhitePoint,
                                          Vector2 RedPrimary,
                                          Vector2 GreenPrimary,
                                          Vector2 BluePrimary,
                                          float MinimumLuminance,
                                          float MaximumLuminance,
                                          float MaximumFullFrameLuminance)
    : IGorgonNamedObject
{
    /// <summary>
    /// An empty/invalid video adapter output. Primarily used to indicate that no output has been selected.
    /// </summary>
    public static readonly GorgonVideoOutputInfo Empty = new("NULL", -1, nint.Zero, GorgonRectangle.Empty, false, RotationMode.Unspecified, [], 0, ColorSpace.Custom, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, 0, 0, 0);

    /// <summary>
    /// Function to determine if the specified format can be used for display.
    /// </summary>
    /// <param name="d3dDevice">The active Direct3D device.</param>
    /// <param name="format">The format to evaluate.</param>
    /// <returns><b>true</b> if the format is suppported for display, or <b>false</b> if not.</returns>
    private static unsafe bool IsDisplayFormat(ComPtr<ID3D12Device14> d3dDevice, DXGI_FORMAT format)
    {
        D3D12_FEATURE_DATA_FORMAT_SUPPORT support = new()
        {
            Format = format
        };

        if (d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_FORMAT_SUPPORT, &support, (uint)Unsafe.SizeOf<D3D12_FEATURE_DATA_FORMAT_SUPPORT>()).FAILED)
        {
            return false;
        }

        return (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DISPLAY) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DISPLAY;
    }

    /// <summary>
    /// Function to enumerate the video modes for a specific output.
    /// </summary>
    /// <param name="output">The output to evaluate.</param>
    /// <param name="d3dDevice">The Direct3D device used to filter display modes.</param>
    /// <returns>A list of available full screen video modes.</returns>
    private static unsafe List<GorgonVideoMode> EnumerateVideoModes(ComPtr<IDXGIOutput6> output, ComPtr<ID3D12Device14> d3dDevice)
    {
        List<GorgonVideoMode> result = [];

        DXGI_FORMAT[] formats = [.. Enum.GetValues<DXGI_FORMAT>().Where(item => IsDisplayFormat(d3dDevice, item))];

        foreach (DXGI_FORMAT format in formats)
        {
            uint modeCount = 0;

            if ((output.Get()->GetDisplayModeList1(format, DXGI.DXGI_ENUM_MODES_SCALING | DXGI.DXGI_ENUM_MODES_INTERLACED | DXGI.DXGI_ENUM_MODES_STEREO, &modeCount, null).FAILED) || (modeCount == 0))
            {
                continue;
            }

            // Use the array pool since we need a new array for each display format. Should cut down on garabge.
            DXGI_MODE_DESC1[] modes = ArrayPool<DXGI_MODE_DESC1>.Shared.Rent((int)modeCount);

            try
            {
                fixed (DXGI_MODE_DESC1* modePtr = modes)
                {
                    if (output.Get()->GetDisplayModeList1(format, DXGI.DXGI_ENUM_MODES_SCALING | DXGI.DXGI_ENUM_MODES_INTERLACED | DXGI.DXGI_ENUM_MODES_STEREO, &modeCount, modePtr).FAILED)
                    {
                        continue;
                    }

                    for (int i = 0; i < modeCount; ++i)
                    {
                        result.Add(modes[i].ToGorgonVideoMode());
                    }
                }
            }
            finally
            {
                ArrayPool<DXGI_MODE_DESC1>.Shared.Return(modes);
            }
        }

        return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVideoOutputInfo" /> class.
    /// </summary>
    /// <param name="index">The index of the output.</param>
    /// <param name="output">The output to evaluate.</param>
    /// <param name="d3dDevice">The Direct3D device used to filter display modes.</param>
    /// <exception cref="Win32Exception">Thrown if the output description would not be retrieved.</exception>
    internal unsafe static GorgonVideoOutputInfo FromD3D(int index, ComPtr<IDXGIOutput6> output, ComPtr<ID3D12Device14> d3dDevice)
    {
        DXGI_OUTPUT_DESC1 desc = default;

        output.Get()->GetDesc1(&desc)
            .ThrowIfFailed(err => throw new Win32Exception(err.Value));

        return new(((ReadOnlySpan<char>)desc.DeviceName).Trim('\0').ToString(),
            index,
            desc.Monitor,
            GorgonRectangle.FromLTRB(desc.DesktopCoordinates.left, desc.DesktopCoordinates.top, desc.DesktopCoordinates.right, desc.DesktopCoordinates.bottom),
            desc.AttachedToDesktop,
            (RotationMode)desc.Rotation,
            EnumerateVideoModes(output, d3dDevice),
            (int)desc.BitsPerColor,                        
            (ColorSpace)desc.ColorSpace,
            new Vector2(desc.WhitePoint[0], desc.WhitePoint[1]),
            new Vector2(desc.RedPrimary[0], desc.RedPrimary[1]),
            new Vector2(desc.GreenPrimary[0], desc.GreenPrimary[1]),
            new Vector2(desc.BluePrimary[0], desc.BluePrimary[1]),
            desc.MinLuminance,
            desc.MaxLuminance,
            desc.MaxFullFrameLuminance);
    }
}
