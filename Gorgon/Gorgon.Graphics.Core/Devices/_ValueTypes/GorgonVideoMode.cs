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
// Created: November 7, 2017 12:57:57 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using SharpDX.DXGI;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how the display mode should be scaled when the mode is not native to the display.
/// </summary>
public enum ModeScaling
{
    /// <summary>
    /// No scaling type is specified.
    /// </summary>
    Unspecified = DisplayModeScaling.Unspecified,
    /// <summary>
    /// Center the image on the display and keep the width and height.
    /// </summary>
    Center = DisplayModeScaling.Centered,
    /// <summary>
    /// Stretch the image to the full width and height of the native display width and height.
    /// </summary>
    Stretch = DisplayModeScaling.Stretched
}

/// <summary>
/// Defines the ordering of the scanlines on the display for a video mode.
/// </summary>
public enum ModeScanlineOrder
{
    /// <summary>
    /// The scanline ordering is not defined.
    /// </summary>
    Unspecified = DisplayModeScanlineOrder.Unspecified,
    /// <summary>
    /// The image is scanned beginning with the lower field.
    /// </summary>
    LowerFirst = DisplayModeScanlineOrder.LowerFieldFirst,
    /// <summary>
    /// The image is scanned beginning with the upper field.
    /// </summary>
    UpperFirst = DisplayModeScanlineOrder.UpperFieldFirst,
    /// <summary>
    /// The image is progressively scanned.
    /// </summary>
    Progressive = DisplayModeScanlineOrder.Progressive
}

/// <summary>
/// Information about a full screen video mode provided by a <see cref="IGorgonVideoOutputInfo"/>.
/// </summary>
public readonly struct GorgonVideoMode
    : IGorgonEquatableByRef<GorgonVideoMode>, IComparable<GorgonVideoMode>
{
    #region Variables.
    /// <summary>
    /// A representation of an invalid video mode.
    /// </summary>
    public static readonly GorgonVideoMode InvalidMode;

    /// <summary>
    /// The width, in pixels, for the video mode.
    /// </summary>
    public readonly int Width;
    /// <summary>
    /// The height, in pixels, for the video mode.
    /// </summary>
    public readonly int Height;
    /// <summary>
    /// The image buffer format for the display mode.
    /// </summary>
    public readonly BufferFormat Format;
    /// <summary>
    /// Flag to indicate whether this mode supports stereo rendering.
    /// </summary>
    public readonly bool SupportsStereo;
    /// <summary>
    /// The refresh rate represented as a rational number.
    /// </summary>
    public readonly GorgonRationalNumber RefreshRate;
    /// <summary>
    /// The type of scaling available to the video mode.
    /// </summary>
    public readonly ModeScaling Scaling;
    /// <summary>
    /// The type of scanline ordering performed when drawing the image on the display for this mode.
    /// </summary>
    public readonly ModeScanlineOrder ScanlineOrder;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the size for the video mode.
    /// </summary>
    public DX.Size2 Size => new(Width, Height);
    #endregion

    #region Methods.
    /// <summary>
    /// Function to determine if two instances are equal or not.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns><b>true</b> if the modes are equal, <b>false</b> if not.</returns>
    public static bool Equals(in GorgonVideoMode left, in GorgonVideoMode right) => left.Width == right.Width
               && left.Height == right.Height
               && left.Format == right.Format
               && left.RefreshRate.Equals(right.RefreshRate)
               && left.Scaling == right.Scaling
               && left.ScanlineOrder == right.ScanlineOrder
               && left.SupportsStereo == right.SupportsStereo;

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance. </param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
    public override bool Equals(object obj) => obj is GorgonVideoMode mode && mode.Equals(this);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Width, Height, Format, RefreshRate, Scaling, ScanlineOrder, SupportsStereo);

    /// <summary>Returns the fully qualified type name of this instance.</summary>
    /// <returns>The fully qualified type name.</returns>
    public override string ToString() =>
        string.Format(Resources.GORGFX_TOSTR_VIDEO_MODE, Width, Height, Format, (float)RefreshRate.Numerator / RefreshRate.Denominator);

    /// <summary>
    /// Function to compare this instance with another.
    /// </summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(in GorgonVideoMode other) => Equals(in this, in other);

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(GorgonVideoMode other) => Equals(in this, in other);

    /// <summary>Compares the left instance with the right object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object. </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero the left instance precedes the right in the sort order.  Zero the left instance occurs in the same position in the sort order as the right. Greater than zero the left instance follows the right in the sort order. </returns>
    public static int CompareTo(in GorgonVideoMode left, in GorgonVideoMode right) => Equals(in left, in right)
            ? 0
            : ((left.Width < right.Width)
            || (left.Height < right.Height)
            || (left.Format < right.Format)
            || (left.RefreshRate < right.RefreshRate)
            || (left.Scaling < right.Scaling)
            || (left.ScanlineOrder < right.ScanlineOrder)
            || (!left.SupportsStereo)) ? -1 : 1;

    /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object. </summary>
    /// <param name="other">An object to compare with this instance. </param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order. </returns>
    int IComparable<GorgonVideoMode>.CompareTo(GorgonVideoMode other) => CompareTo(in this, in other);

    /// <summary>
    /// Operator to determine if the left instance is less than the right instance.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is less than the right, <b>false</b> if not.</returns>
    public static bool operator <(GorgonVideoMode left, GorgonVideoMode right) => CompareTo(in left, in right) < 0;

    /// <summary>
    /// Operator to determine if the left instance is greater than the right instance.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is greater than the right, <b>false</b> if not.</returns>
    public static bool operator >(GorgonVideoMode left, GorgonVideoMode right) => CompareTo(in left, in right) > 0;

    /// <summary>
    /// Operator to determine if the left instance is less than or equal to the right instance.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is less than or equal to the right, <b>false</b> if not.</returns>
    public static bool operator <=(GorgonVideoMode left, GorgonVideoMode right) => Equals(in left, in right) || CompareTo(in left, in right) < 0;

    /// <summary>
    /// Operator to determine if the left instance is greater than or equal to the right instance.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if left is greater than or equal to the right, <b>false</b> if not.</returns>
    public static bool operator >=(GorgonVideoMode left, GorgonVideoMode right) => Equals(in left, in right) || CompareTo(in left, in right) > 0;

    /// <summary>
    /// Operator to compare two instances for equality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonVideoMode left, GorgonVideoMode right) => Equals(in left, in right);

    /// <summary>
    /// Operator to compare two instances for inequality.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(GorgonVideoMode left, GorgonVideoMode right) => !Equals(in left, in right);
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVideoMode" /> struct.
    /// </summary>
    /// <param name="modeDesc">The DXGI mode description to copy.</param>
    internal GorgonVideoMode(ModeDescription1 modeDesc)
    {
        Width = modeDesc.Width;
        Height = modeDesc.Height;
        RefreshRate = modeDesc.RefreshRate.ToGorgonRational();
        SupportsStereo = modeDesc.Stereo;
        Scaling = (ModeScaling)modeDesc.Scaling;
        ScanlineOrder = (ModeScanlineOrder)modeDesc.ScanlineOrdering;
        Format = (BufferFormat)modeDesc.Format;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
    /// </summary>
    /// <param name="width">The width of the mode, in pixels.</param>
    /// <param name="height">The height of the mode, in pixels.</param>
    /// <param name="format">The pixel format for the mode.</param>
    /// <param name="refreshRate">[Optional] The refresh rate for the video mode.</param>
    /// <param name="scaling">[Optional] The type of scaling supported.</param>
    /// <param name="scanlineOrder">[Optional] The scanline order supported.</param>
    /// <param name="steroSupport">[Optional] <b>true</b> if the mode should support stereo rendering, or <b>false</b> if not.</param>
    public GorgonVideoMode(int width,
                           int height,
                           BufferFormat format,
                           GorgonRationalNumber? refreshRate = null,
                           ModeScaling scaling = ModeScaling.Unspecified,
                           ModeScanlineOrder scanlineOrder = ModeScanlineOrder.Unspecified,
                           bool steroSupport = false)
    {
        Width = width;
        Height = height;
        Format = format;
        RefreshRate = refreshRate ?? GorgonRationalNumber.Empty;
        Scaling = scaling;
        ScanlineOrder = scanlineOrder;
        SupportsStereo = steroSupport;
    }
    #endregion
}
