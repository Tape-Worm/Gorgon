// Gorgon.
// Copyright (C) 2023 Michael Winsor
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
// Created: December 18, 2023 2:22:03 PM
//

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Gorgon.Json;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Graphics;

/// <summary>
/// A structure with a width and height.
/// </summary>
/// <remarks>
/// <para>
/// This data structure is used to easily pass around 2D coordinates for a rectangle shape. The rectangle shape can be intersected and unioned. It also provides various methods to determine if another rectangle intersects, or is 
/// contained completely within a rectangle. 
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// This value type merely represents the dimensions of a rectangle, it does not draw a rectangle. 
/// </para>
/// </note>
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4), JsonConverter(typeof(GorgonRectangleJsonConverter))]
public struct GorgonRectangle
    : IEquatable<GorgonRectangle>
{
    /// <summary>
    /// The size of the this value, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Unsafe.SizeOf<GorgonRectangle>();

    /// <summary>
    /// An empty rectangle.
    /// </summary>
    public static readonly GorgonRectangle Empty = new()
    {
        X = 0,
        Y = 0,
        Width = 0,
        Height = 0
    };

    /// <summary>
    /// Horizontal position.
    /// </summary>
    public int X;
    /// <summary>
    /// Vertical position
    /// </summary>
    public int Y;
    /// <summary>
    /// Width of the rectangle.
    /// </summary>
    public int Width;
    /// <summary>
    /// Height of the rectangle.
    /// </summary>
    public int Height;

    /// <summary>
    /// Property to determine if the rectangle is empty.
    /// </summary>
    public readonly bool IsEmpty => Width == 0 && Height == 0 && X == 0 && Y == 0;

    /// <summary>
    /// Property to set or return the left value for the rectangle.
    /// </summary>
    public int Left
    {
        readonly get => X;
        set => X = value;
    }

    /// <summary>
    /// Property to set or return the top value for the rectangle.
    /// </summary>
    public int Top
    {
        readonly get => Y;
        set => Y = value;
    }

    /// <summary>
    /// Property to set or return the right value of the rectangle.
    /// </summary>
    public int Right
    {
        readonly get => unchecked(Width + X);
        set => Width = unchecked(value - X);
    }

    /// <summary>
    /// Property to set or return the bottom value of the rectangle.
    /// </summary>
    public int Bottom
    {
        readonly get => unchecked(Height + Y);
        set => Height = unchecked(value - Y);
    }

    /// <summary>
    /// Property to set or return the location of the rectangle.
    /// </summary>
    public GorgonPoint Location
    {
        readonly get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>
    /// Property to set or return the size of the rectangle.
    /// </summary>
    public GorgonPoint Size
    {
        readonly get => new(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    /// <summary>
    /// Property to return the top and left corner of the rectangle.
    /// </summary>
    public readonly GorgonPoint TopLeft => new(Left, Top);

    /// <summary>
    /// Property to return the bottom and left corner of the rectangle.
    /// </summary>
    public readonly GorgonPoint BottomLeft => new(Left, Bottom);

    /// <summary>
    /// Property to return the top and right corner of the rectangle.
    /// </summary>
    public readonly GorgonPoint TopRight => new(Right, Top);

    /// <summary>
    /// Property to return the bottom and right corner of the rectangle.
    /// </summary>
    public readonly GorgonPoint BottomRight => new(Right, Bottom);

    /// <summary>
    /// Property to return the center of the rectangle.
    /// </summary>
    public readonly GorgonPoint Center => new(Width / 2 + Left, Height / 2 + Top);

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override readonly string ToString() => string.Format(Resources.GOR_TOSTR_RECTANGLE, X, Y, Right, Bottom, Width, Height);

    /// <summary>
    /// Function to return a rectangle from left, top, right and bottom coordinates.
    /// </summary>
    /// <param name="left">Left coordinate</param>
    /// <param name="top">Top coordinate.</param>
    /// <param name="right">Right coordinate.</param>
    /// <param name="bottom">Bottom coordinate.</param>
    /// <returns>A new rectangle with the specified coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle FromLTRB(int left, int top, int right, int bottom) => new()
    {
        X = left,
        Y = top,
        Width = right - left,
        Height = bottom - top,
    };

    /// <summary>
    /// Function to convert a <see cref="GorgonRectangle"/> to a <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="RectangleF"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectangleF ToRectangleF(GorgonRectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>
    /// Function to convert a <see cref="RectangleF"/> to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="GorgonRectangle"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle FromRectangleF(RectangleF rect) => new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

    /// <summary>
    /// Function to convert a <see cref="GorgonRectangle"/> to a <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="RectangleF"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rectangle ToRectangle(GorgonRectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>
    /// Function to convert a <see cref="Rectangle"/> to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="GorgonRectangle"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle FromRectangle(Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>
    /// Operator to convert a <see cref="GorgonRectangle"/> to a <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="Rectangle"/>.</returns>
    public static implicit operator RectangleF(GorgonRectangle rect) => ToRectangleF(rect);

    /// <summary>
    /// Operator to convert a <see cref="GorgonRectangle"/> to a <see cref="RectangleF"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="RectangleF"/>.</returns>
    public static implicit operator Rectangle(GorgonRectangle rect) => ToRectangle(rect);

    /// <summary>
    /// Operator to convert a <see cref="RectangleF"/> to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="RectangleF"/>.</returns>
    public static explicit operator GorgonRectangle(RectangleF rect) => FromRectangleF(rect);

    /// <summary>
    /// Operator to convert a <see cref="Rectangle"/> to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The converted <see cref="RectangleF"/>.</returns>
    public static implicit operator GorgonRectangle(Rectangle rect) => FromRectangle(rect);

    /// <summary>
    /// Function to determine the intersection between two <see cref="GorgonRectangle"/> values.
    /// </summary>
    /// <param name="rectangle1">First rectangle to intersect.</param>
    /// <param name="rectangle2">Second rectangle to intersect.</param>
    /// <returns>The intersected rectangle or <see cref="Empty"/> if no intersection was found.</returns>    
    public static GorgonRectangle Intersect(GorgonRectangle rectangle1, GorgonRectangle rectangle2)
    {
        int left = rectangle2.Left.Max(rectangle1.Left);
        int top = rectangle2.Top.Max(rectangle1.Top);

        int right = rectangle2.Right.Min(rectangle1.Right);
        int bottom = rectangle2.Bottom.Min(rectangle1.Bottom);

        if ((right <= left) || (bottom <= top))
        {
            return Empty;
        }

        return FromLTRB(left, top, right, bottom);
    }

    /// <summary>
    /// Function to determine if this rectangle intersects with another rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to compare with.</param>
    /// <returns><b>true</b> if the rectangles intersect, <b>false</b> if the rectangles do not intersect.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IntersectsWith(GorgonRectangle rectangle) =>
            (rectangle.Left < Right) && (Left < rectangle.Right) &&
            (rectangle.Top < Bottom) && (Top < rectangle.Bottom);

    /// <summary>
    /// Function to determine if another <see cref="GorgonRectangle"/> is completely contained within this rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to evaluate.</param>
    /// <returns><b>true</b> if the rectangle is contained within this rectangle, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(GorgonRectangle rectangle) => (rectangle.Left >= Left) && (rectangle.Right <= Right)
                                                              && (rectangle.Top >= Top) && (rectangle.Bottom <= Bottom);

    /// <summary>
    /// Function to determine if a 3D point is contained within this <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="x">The X coordinate of the point to evaluate.</param>
    /// <param name="y">The Y coordinate of the point to evaluate.</param>
    /// <returns><b>true</b> if the point is contained within the rectangle, or <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(int x, int y) => (x >= Left) && (y >= Top) && (x <= Right) && (y <= Bottom);

    /// <summary>
    /// Function to determine if a 3D point is contained within this <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="point">The 2D point to evaluate.</param>
    /// <returns><b>true</b> if the point is contained within the rectangle, or <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(GorgonPoint point) => Contains(point.X, point.Y);

    /// <summary>
    /// Function to determine the union of two <see cref="GorgonRectangle"/> values.
    /// </summary>
    /// <param name="rectangle1">First rectangle to intersect.</param>
    /// <param name="rectangle2">Second rectangle to intersect.</param>
    /// <returns>The resulting intersected rectangle or <see cref="Empty"/> if the rectangles could not be unioned.</returns>
    public static GorgonRectangle Union(GorgonRectangle rectangle1, GorgonRectangle rectangle2)
    {
        int l = rectangle1.Left.Min(rectangle2.Left);
        int t = rectangle1.Top.Min(rectangle2.Top);
        int r = rectangle1.Right.Max(rectangle2.Right);
        int b = rectangle1.Bottom.Max(rectangle2.Bottom);

        if ((r < l) || (b < t))
        {
            return Empty;
        }

        return FromLTRB(l, t, r, b);
    }

    /// <summary>
    /// Function to return whether two instances are equal or not.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(GorgonRectangle left, GorgonRectangle right) => ((left.X == right.X) && (left.Y == right.Y)
            && (left.Width == right.Width) && (left.Height == right.Height));

    /// <summary>
    /// Determines whether the specified <see cref="object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
    /// </returns>
    public override readonly bool Equals(object? obj) => obj is GorgonRectangle rectangle ? rectangle.Equals(this) : base.Equals(obj);

    /// <summary>
    /// Operator to determine if 2 instances are equal.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonRectangle left, GorgonRectangle right) => Equals(left, right);

    /// <summary>
    /// Operator to determine if 2 instances are not equal.
    /// </summary>
    /// <param name="left">The left instance to compare.</param>
    /// <param name="right">The right instance to compare.</param>
    /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
    public static bool operator !=(GorgonRectangle left, GorgonRectangle right) => !Equals(left, right);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override readonly int GetHashCode() => HashCode.Combine(Right, Bottom, Left, Top);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the other parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(GorgonRectangle other) => Equals(this, other);

    /// <summary>
    /// Function to deconstruct this type into its components.
    /// </summary>
    /// <param name="x">The horizontal position.</param>
    /// <param name="y">The vertical position.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public readonly void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }

    /// <summary>
    /// Function to convert a <see cref="GorgonRectangle"/> to a <see cref="GorgonRectangleF"/> value.
    /// </summary>
    /// <param name="other">The rectangle to convert.</param>
    /// <returns>The converted rectangle value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangleF ToGorgonRectangleF(GorgonRectangle other) => new(other);

    /// <summary>
    /// Function to expand or shrink a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="rectangle">The rectangle to expand or shrink.</param>
    /// <param name="amount">The amount to expand or shrink by.</param>
    /// <returns>The expanded/shrunken rectangle.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="amount"/> value is negative, the rectangle will shrink in size by the amount specified.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle Expand(GorgonRectangle rectangle, int amount)
    {
        if (amount == 0)
        {
            return rectangle;
        }

        return new(rectangle.X - amount, rectangle.Y - amount, rectangle.Width + amount * 2, rectangle.Height + amount * 2);
    }

    /// <summary>
    /// Operator to implicitly convert this <see cref="GorgonRectangle"/> ot a <see cref="GorgonRectangleF"/>.
    /// </summary>
    /// <param name="rectangle">The rectangle to convert.</param>
    /// <returns>The converted rectangle value.</returns>
    public static implicit operator GorgonRectangleF(GorgonRectangle rectangle) => ToGorgonRectangleF(rectangle);

    /// <summary>
    /// Function to round the values of a <see cref="GorgonRectangleF"/> and convert to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="other">The rectangle to convert.</param>
    /// <returns>The converted rectangle value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle Round(GorgonRectangleF other) => new((int)other.X.Round(rounding: MidpointRounding.AwayFromZero), (int)other.Y.Round(rounding: MidpointRounding.AwayFromZero),
                                                                       (int)other.Width.Round(rounding: MidpointRounding.AwayFromZero), (int)other.Height.Round(rounding: MidpointRounding.AwayFromZero));

    /// <summary>
    /// Function to ceiling the values of a <see cref="GorgonRectangleF"/> and convert to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="other">The rectangle to convert.</param>
    /// <returns>The converted rectangle value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle Ceiling(GorgonRectangleF other) => new((int)other.X.FastCeiling(), (int)other.Y.FastCeiling(),
                                                                                     (int)other.Width.FastCeiling(), (int)other.Height.FastCeiling());

    /// <summary>
    /// Function to floor the values of a <see cref="GorgonRectangleF"/> and convert to a <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="other">The rectangle to convert.</param>
    /// <returns>The converted rectangle value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle Floor(GorgonRectangleF other) => new((int)other.X.FastFloor(), (int)other.Y.FastFloor(),
                                                                                   (int)other.Width.FastFloor(), (int)other.Height.FastFloor());

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonRectangle"/> value.
    /// </summary>
    /// <param name="location">The location of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    public GorgonRectangle(GorgonPoint location, GorgonPoint size)
    {
        Location = location;
        Size = size;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonRectangle"/> value.
    /// </summary>
    /// <param name="x">The horizontal position of the rectangle.</param>
    /// <param name="y">The vertical position of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public GorgonRectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
