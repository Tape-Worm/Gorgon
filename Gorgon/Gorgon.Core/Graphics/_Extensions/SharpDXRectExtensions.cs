
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: March 15, 2019 12:38:55 PM
// 


using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics;

/// <summary>
/// Extension methods for the SharpDX rectangle and rectanglef types
/// </summary>
public static class SharpDXRectExtensions
{
    /// <summary>
    /// Function to compare viewports for equality using a read only reference.
    /// </summary>
    /// <param name="thisView">The view being compared.</param>
    /// <param name="other">The other view being compared.</param>
    /// <returns><b>true</b> if the two instances are equal, <b>false</b> if not.</returns>
    public static bool Equals(in this DX.ViewportF thisView, in DX.ViewportF other) => (thisView.X.EqualsEpsilon(other.X))
            && (thisView.Y.EqualsEpsilon(other.Y))
            && (thisView.Width.EqualsEpsilon(other.Width))
            && (thisView.Height.EqualsEpsilon(other.Height))
            && (thisView.MinDepth.EqualsEpsilon(other.MinDepth))
            && (thisView.MaxDepth.EqualsEpsilon(other.MaxDepth));

    /// <summary>
    /// Function to compare rectangles for equality using a read only reference.
    /// </summary>
    /// <param name="thisRect">The rectangle being compared.</param>
    /// <param name="other">The other view being compared.</param>
    /// <returns><b>true</b> if the two instances are equal, <b>false</b> if not.</returns>
    public static bool Equals(in this DX.RectangleF thisRect, in DX.RectangleF other) => (thisRect.X.EqualsEpsilon(other.X))
            && (thisRect.Y.EqualsEpsilon(other.Y))
            && (thisRect.Width.EqualsEpsilon(other.Width))
            && (thisRect.Height.EqualsEpsilon(other.Height));

    /// <summary>
    /// Function to compare rectangles for equality using a read only reference.
    /// </summary>
    /// <param name="thisRect">The rectangle being compared.</param>
    /// <param name="other">The other view being compared.</param>
    /// <returns><b>true</b> if the two instances are equal, <b>false</b> if not.</returns>
    public static bool Equals(in this DX.Rectangle thisRect, in DX.Rectangle other) => (thisRect.X == other.X)
            && (thisRect.Y == other.Y)
            && (thisRect.Width == other.Width)
            && (thisRect.Height == other.Height);

    /// <summary>
    /// Function to determine if a point is contained within a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to evaluate.</param>
    /// <param name="point">The point to evaluate.</param>
    /// <returns><b>true</b> if the point lies within the rectangle, <b>false</b> if not.</returns>
    public static bool Contains(this DX.Rectangle rect, DX.Point point) => rect.Contains(point.X, point.Y);

    /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
    /// <param name="rect">The source rectangle to compare.</param>
    /// <param name="value">The rectangle to evaluate.</param>
    /// <returns><b>true</b> if the rectangle is contained within the other rectangle, or <b>false</b> if not.</returns>
    public static bool Contains(this DX.RectangleF rect, DX.RectangleF value) => (rect.X <= value.X) && (value.Right <= rect.Right) && (rect.Y <= value.Y) && value.Bottom <= rect.Bottom;

    /// <summary>
    /// Function to convert an integer rectangle to a floating point rectangle/
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted rectangle.</returns>
    public static DX.RectangleF ToRectangleF(this DX.Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>
    /// Function to truncate the rectangle coordinates to the whole number portion of their values.
    /// </summary>
    /// <param name="rect">The rectangle to truncate.</param>
    /// <returns>The truncated rectangle.</returns>
    /// <remarks>
    /// <para>
    /// This method converts the coordinates to integer values without applying rounding.
    /// </para>
    /// </remarks>
    public static DX.RectangleF Truncate(this DX.RectangleF rect) => new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

    /// <summary>
    /// Function to set the rectangle coordinates to the nearest integer values that are lower than or equal to the original values.
    /// </summary>
    /// <param name="rect">The rectangle to floor.</param>
    /// <returns>The truncated rectangle.</returns>
    public static DX.RectangleF Floor(this DX.RectangleF rect) => new(rect.X.FastFloor(), rect.Y.FastFloor(), rect.Width.FastFloor(), rect.Height.FastFloor());

    /// <summary>
    /// Function to set the rectangle coordinates to the nearest integer values that are higher than or equal to the original values.
    /// </summary>
    /// <param name="rect">The rectangle to floor.</param>
    /// <returns>The truncated rectangle.</returns>
    public static DX.RectangleF Ceiling(this DX.RectangleF rect) => new(rect.X.FastCeiling(), rect.Y.FastCeiling(), rect.Width.FastCeiling(), rect.Height.FastCeiling());

    /// <summary>
    /// Function to convert a floating point rectangle to an integer rectangle/
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted rectangle.</returns>
    /// <remarks>
    /// <para>
    /// This method converts the coordinates to integer values without applying rounding.
    /// </para>
    /// </remarks>
    public static DX.Rectangle ToRectangle(this DX.RectangleF rect) => new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
}
