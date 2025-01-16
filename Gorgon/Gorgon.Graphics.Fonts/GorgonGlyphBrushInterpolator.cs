
// 
// Gorgon
// Copyright (C) 2014 Michael Winsor
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
// Created: Sunday, March 2, 2014 9:34:26 PM
// 

using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// An interpolation value used to weight the color blending in a gradient brush
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonGlyphBrushInterpolator"/> struct
/// </remarks>
/// <param name="weight">The weight in the interpolation.</param>
/// <param name="color">The color at the interpolation weight.</param>
public readonly struct GorgonGlyphBrushInterpolator(float weight, GorgonColor color)
        : IEquatable<GorgonGlyphBrushInterpolator>, IComparable<GorgonGlyphBrushInterpolator>
{
    /// <summary>
    /// Property to return the interpolation weight.
    /// </summary>
    public readonly float Weight = weight.Min(1.0f).Max(0);

    /// <summary>
    /// Property to return the interpolation color.
    /// </summary>
    public readonly GorgonColor Color = color;

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(Resources.GORGFX_TOSTR_FONT_BRUSH_INTERPOLATION,
                             Weight,
                             Color.Red,
                             Color.Green,
                             Color.Blue,
                             Color.Alpha);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) => (obj is GorgonGlyphBrushInterpolator brush) && (brush.Equals(this));

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Weight);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(GorgonGlyphBrushInterpolator other) => other.Weight.EqualsEpsilon(Weight) && Color.Equals(other.Color);

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
    /// </returns>
    public int CompareTo(GorgonGlyphBrushInterpolator other) => Weight < other.Weight ? -1 : Weight > other.Weight ? 1 : 0;

    /// <summary>
    /// Performs an equality check on two instances.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right) => left.Equals(right);

    /// <summary>
    /// Performs an inequality check on two instances.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right) => !(left == right);

    /// <summary>
    /// Performs a less than test on two instances.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if <paramref name="left"/> is less than <paramref name="right"/>, <b>false</b> if not.</returns>
    public static bool operator <(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Performs a less or equal to than test on two instances.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if <paramref name="left"/> is less than or equal to <paramref name="right"/>, <b>false</b> if not.</returns>
    public static bool operator <=(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Performs a greater than test on two instances.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if <paramref name="left"/> is greater than <paramref name="right"/>, <b>false</b> if not.</returns>
    public static bool operator >(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Performs a greater than or equal to than test on two instances.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>, <b>false</b> if not.</returns>
    public static bool operator >=(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right) => left.CompareTo(right) >= 0;

}
