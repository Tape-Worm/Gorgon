
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
// Created: November 7, 2017 12:57:57 PM
// 

using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how the display mode should be scaled when the mode is not native to the display
/// </summary>
public enum ModeScaling
{
    /// <summary>
    /// No scaling type is specified.
    /// </summary>
    Unspecified = DXGI_MODE_SCALING.DXGI_MODE_SCALING_UNSPECIFIED,
    /// <summary>
    /// Center the image on the display and keep the width and height.
    /// </summary>
    Center = DXGI_MODE_SCALING.DXGI_MODE_SCALING_CENTERED,
    /// <summary>
    /// Stretch the image to the full width and height of the native display width and height.
    /// </summary>
    Stretch = DXGI_MODE_SCALING.DXGI_MODE_SCALING_STRETCHED
}

/// <summary>
/// Defines the ordering of the scanlines on the display for a video mode
/// </summary>
public enum ModeScanlineOrder
{
    /// <summary>
    /// The scanline ordering is not defined.
    /// </summary>
    Unspecified = DXGI_MODE_SCANLINE_ORDER.DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED,
    /// <summary>
    /// The image is scanned beginning with the lower field.
    /// </summary>
    LowerFirst = DXGI_MODE_SCANLINE_ORDER.DXGI_MODE_SCANLINE_ORDER_LOWER_FIELD_FIRST,
    /// <summary>
    /// The image is scanned beginning with the upper field.
    /// </summary>
    UpperFirst = DXGI_MODE_SCANLINE_ORDER.DXGI_MODE_SCANLINE_ORDER_UPPER_FIELD_FIRST,
    /// <summary>
    /// The image is progressively scanned.
    /// </summary>
    Progressive = DXGI_MODE_SCANLINE_ORDER.DXGI_MODE_SCANLINE_ORDER_PROGRESSIVE
}

/// <summary>
/// Information about a full screen video mode provided by a <see cref="GorgonVideoOutputInfo"/>
/// </summary>
/// <param name="Width">The width, in pixels, for the video mode.</param>
/// <param name="Height">The height, in pixels, for the video mode.</param>
/// <param name="Format">The pixel format for the display mode.</param>
/// <param name="RefreshRate">The refresh rate represented as a rational number.</param>
/// <param name="SupportsStereo"><b>true</b> if whether this mode supports stereo rendering, <b>false</b> if not.</param>
/// <param name="Scaling">The type of scaling available to the video mode.</param>
/// <param name="ScanlineOrder">The type of scanline ordering performed when drawing the image on the display for this mode.</param>
public record struct GorgonVideoMode(int Width, int Height, BufferFormat Format, GorgonRationalNumber RefreshRate, bool SupportsStereo, ModeScaling Scaling, ModeScanlineOrder ScanlineOrder)
{
    /// <summary>
    /// A representation of an invalid video mode.
    /// </summary>
    public static readonly GorgonVideoMode InvalidMode = new();

    /// <summary>
    /// Property to return the size for the video mode.
    /// </summary>
    public readonly GorgonPoint Size => new(Width, Height);

    /// <inheritdoc/>
    public override readonly string ToString() => string.Format(Resources.GORGFX_STR_VIDEO_MODE, Width, Height, Format, (float)RefreshRate.Numerator / RefreshRate.Denominator);
}
