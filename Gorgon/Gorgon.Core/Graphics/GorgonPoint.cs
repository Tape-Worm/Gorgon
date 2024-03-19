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
// Created: December 18, 2023 5:28:51 PM
//

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Properties;
using Gorgon.Json;
using Gorgon.Math;
using Newtonsoft.Json;

namespace Gorgon.Graphics;

/// <summary>
/// Defines a single point in space using <see cref="int"/> coordinates.
/// </summary>
/// <remarks>
/// <para>
/// This data structure is used to easily pass around 2D integer coordinates.
/// </para>
/// <para>
/// If floating point coordinates are required, use the .NET <see cref="Vector2"/> type.
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// This value type merely represents the position of a point in space, it does not draw a point. 
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <param name="x">The horiztonal position of the point.</param>
/// <param name="y">The vertical position of the point.</param>
[StructLayout(LayoutKind.Sequential, Pack = 4), JsonConverter(typeof(GorgonPointJsonConverter))]
public struct GorgonPoint(int x, int y)
    : IEquatable<GorgonPoint>
{
    /// <summary>
    /// The size of the this value, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Unsafe.SizeOf<GorgonPoint>();

    /// <summary>
    /// A point that has its position set to 0 for the x and y coordinate.
    /// </summary>
    public static readonly GorgonPoint Zero = new(0, 0);

    /// <summary>
    /// The horizontal position of the point.
    /// </summary>
    public int X = x;

    /// <summary>
    /// The vertical position of the point.
    /// </summary>
    public int Y = y;

    /// <summary>
    /// Function to convert this point to a <see cref="Point"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point ToPoint(GorgonPoint point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert a <see cref="Point"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint FromPoint(Point point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert this point to a <see cref="PointF"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF ToPointF(GorgonPoint point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert a <see cref="PointF"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint FromPointF(PointF point) => new((int)point.X, (int)point.Y);

    /// <summary>
    /// Function to convert this point to a <see cref="Size"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size ToSize(GorgonPoint point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert this a <see cref="Size"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The converted size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint FromSize(Size size) => new(size.Width, size.Height);

    /// <summary>
    /// Function to convert this point to a <see cref="SizeF"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SizeF ToSizeF(GorgonPoint point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert this a <see cref="SizeF"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The converted size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint FromSizeF(SizeF size) => new((int)size.Width, (int)size.Height);

    /// <summary>
    /// Function to convert this point to a <see cref="Vector2"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(GorgonPoint point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert a <see cref="Vector2"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="vector">The 2D vector to convert.</param>
    /// <returns>The converted 2D vector.</returns>
    public static GorgonPoint FromVector2(Vector2 vector) => new((int)vector.X, (int)vector.Y);

    /// <summary>
    /// Function to round the values of the <see cref="Vector2"/> and return a new <see cref="GorgonPoint"/> with the rounded values.
    /// </summary>
    /// <param name="vector">The vector to round.</param>
    /// <returns>A new <see cref="GorgonPoint"/> with the rounded values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint Round(Vector2 vector) => new((int)vector.X.Round(rounding: MidpointRounding.AwayFromZero), (int)vector.Y.Round(rounding: MidpointRounding.AwayFromZero));

    /// <summary>
    /// Function to ceiling the values of the <see cref="Vector2"/> and return a new <see cref="GorgonPoint"/> with the ceiling values.
    /// </summary>
    /// <param name="vector">The vector to ceiling.</param>
    /// <returns>A new <see cref="GorgonPoint"/> with the ceiling values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint Ceiling(Vector2 vector) => new((int)vector.X.FastCeiling(), (int)vector.Y.FastCeiling());

    /// <summary>
    /// Function to floor the values of the <see cref="Vector2"/> and return a new <see cref="GorgonPoint"/> with the floor values.
    /// </summary>
    /// <param name="vector">The vector to floor.</param>
    /// <returns>A new <see cref="GorgonPoint"/> with the floor values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint Floor(Vector2 vector) => new((int)vector.X.FastFloor(), (int)vector.Y.FastFloor());

    /// <summary>
    /// Function to add two <see cref="GorgonPoint"/> values together.
    /// </summary>
    /// <param name="left">The left point to add.</param>
    /// <param name="right">The right point to add.</param>
    /// <returns>The sum of the two points.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint Add(GorgonPoint left, GorgonPoint right) => new(left.X + right.X, left.Y + right.Y);

    /// <summary>
    /// Function to subtract two <see cref="GorgonPoint"/> values.
    /// </summary>
    /// <param name="left">The left point to subtract.</param>
    /// <param name="right">The right point to subtract.</param>
    /// <returns>The difference between the two points.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint Subtract(GorgonPoint left, GorgonPoint right) => new(left.X - right.X, left.Y - right.Y);

    /// <summary>
    /// Function to multiply a <see cref="GorgonPoint"/> value and a scalar value.
    /// </summary>
    /// <param name="left">The left point to multiply.</param>
    /// <param name="scalar">The scalar value to multiply.</param>
    /// <returns>The product of the point and the scalar value..</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint Multiply(GorgonPoint left, int scalar) => new(left.X * scalar, left.Y * scalar);

    /// <summary>
    /// Function to divide a <see cref="GorgonPoint"/> value and a scalar value.
    /// </summary>
    /// <param name="left">The left point to divide.</param>
    /// <param name="scalar">The scalar value to divide.</param>
    /// <returns>The quotient of the point and the scalar value..</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPoint Divide(GorgonPoint left, int scalar) => new(left.X / scalar, left.Y / scalar);

    /// <summary>
    /// Function to compare two points for equality.
    /// </summary>
    /// <param name="left">The left point to compare.</param>
    /// <param name="right">The right point to compare.</param>
    /// <returns><b>true</b> if the two values are equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(GorgonPoint left, GorgonPoint right) => (left.X == right.X) && (left.Y == right.Y);

    /// <summary>
    /// Function to check this point with another for equality.
    /// </summary>
    /// <param name="other">The other point to compare.</param>
    /// <returns><b>true</b> if the two values are equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(GorgonPoint other) => Equals(this, other);

    /// <summary>
    /// Function to determine equality for this value and another value.
    /// </summary>
    /// <param name="obj">The value to compare with.</param>
    /// <returns><b>true</b> if the two values are equal, <b>false</b> if not.</returns>
    public override readonly bool Equals(object obj) => obj is GorgonPoint pt ? Equals(this, pt) : base.Equals(obj);

    /// <summary>
    /// Function to provide a hash code for the type.
    /// </summary>
    /// <returns>The hash code for the type.</returns>
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Function to deconstruct this type into its components.
    /// </summary>
    /// <param name="x">The horizontal position.</param>
    /// <param name="y">The vertical position.</param>
    public readonly void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }    

    /// <summary>
    /// Function to return a string representation of the type.
    /// </summary>
    /// <returns>The string representation of the type.</returns>
    public override readonly string ToString() => string.Format(Resources.GOR_TOSTR_POINT, X, Y);

    /// <summary>
    /// Operator to convert a <see cref="GorgonPoint"/> to a <see cref="Vector2"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    public static implicit operator Vector2(GorgonPoint point) => ToVector2(point);

    /// <summary>
    /// Operator to convert a <see cref="Vector2"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static explicit operator GorgonPoint(Vector2 vector) => FromVector2(vector);

    /// <summary>
    /// Operator to convert a <see cref="GorgonPoint"/> to a <see cref="Point"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    public static implicit operator Point(GorgonPoint point) => ToPoint(point);

    /// <summary>
    /// Operator to convert a <see cref="GorgonPoint"/> to a <see cref="PointF"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    public static implicit operator PointF(GorgonPoint point) => ToPointF(point);

    /// <summary>
    /// Operator to convert a <see cref="Point"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    public static implicit operator GorgonPoint(Point point) => FromPointF(point);

    /// <summary>
    /// Operator to convert a <see cref="PointF"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    public static explicit operator GorgonPoint(PointF point) => FromPointF(point);

    /// <summary>
    /// Operator to convert a <see cref="GorgonPoint"/> to a <see cref="Size"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    public static implicit operator Size(GorgonPoint point) => ToSize(point);

    /// <summary>
    /// Operator to convert a <see cref="GorgonPoint"/> to a <see cref="SizeF"/> type.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    public static implicit operator SizeF(GorgonPoint point) => ToSizeF(point);

    /// <summary>
    /// Operator to convert a <see cref="Size"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    public static implicit operator GorgonPoint(Size size) => FromSize(size);

    /// <summary>
    /// Operator to convert a <see cref="SizeF"/> to a <see cref="GorgonPoint"/> type.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    public static explicit operator GorgonPoint(SizeF size) => FromSizeF(size);

    /// <summary>
    /// Operator to add two <see cref="GorgonPoint"/> values together.
    /// </summary>
    /// <param name="left">The left point to add.</param>
    /// <param name="right">The right point to add.</param>
    /// <returns>The sum of the two points.</returns>
    public static GorgonPoint operator +(GorgonPoint left, GorgonPoint right) => Add(left, right);

    /// <summary>
    /// Operator to subtract two <see cref="GorgonPoint"/> values.
    /// </summary>
    /// <param name="left">The left point to subtract.</param>
    /// <param name="right">The right point to subtract.</param>
    /// <returns>The difference between the two points.</returns>
    public static GorgonPoint operator -(GorgonPoint left, GorgonPoint right) => Subtract(left, right);

    /// <summary>
    /// Operator to multiply a <see cref="GorgonPoint"/> value and a scalar value.
    /// </summary>
    /// <param name="point">The point to multiply.</param>
    /// <param name="scalar">The scalar value to multiply.</param>
    /// <returns>The product of the point and the scalar value..</returns>
    public static GorgonPoint operator *(GorgonPoint point, int scalar) => Multiply(point, scalar);

    /// <summary>
    /// Operator to multiply a <see cref="GorgonPoint"/> value and a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar value to multiply.</param>
    /// <param name="point">The point to multiply.</param>
    /// <returns>The product of the point and the scalar value..</returns>
    public static GorgonPoint operator *(int scalar, GorgonPoint point) => Multiply(point, scalar);

    /// <summary>
    /// Function to divide a <see cref="GorgonPoint"/> value and a scalar value.
    /// </summary>
    /// <param name="left">The left point to divide.</param>
    /// <param name="scalar">The scalar value to divide.</param>
    /// <returns>The quotient of the point and the scalar value..</returns>
    public static GorgonPoint operator /(GorgonPoint left, int scalar) => Divide(left, scalar);

    /// <summary>
    /// Operator to compare two points for equality.
    /// </summary>
    /// <param name="left">The left point to compare.</param>
    /// <param name="right">The right point to compare.</param>
    /// <returns><b>true</b> if the two values are equal, <b>false</b> if not.</returns>
    public static bool operator ==(GorgonPoint left, GorgonPoint right) => Equals(left, right);

    /// <summary>
    /// Operator to compare two points for inequality.
    /// </summary>
    /// <param name="left">The left point to compare.</param>
    /// <param name="right">The right point to compare.</param>
    /// <returns><b>true</b> if the two values are not equal, <b>false</b> if they are.</returns>
    public static bool operator !=(GorgonPoint left, GorgonPoint right) => !Equals(left, right);
}