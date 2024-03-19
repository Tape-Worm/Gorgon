
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
// Created: April 6, 2019 10:19:56 PM
// 


using System.Numerics;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics;

/// <summary>
/// Extension methods for the SharpDX Size 2 types
/// </summary>
public static class SharpDXSize2Extensions
{
    /// <summary>
    /// Function to truncate the size coordinates to the whole number portion of their values.
    /// </summary>
    /// <param name="size">The size to truncate.</param>
    /// <returns>The truncated size.</returns>
    /// <remarks>
    /// <para>
    /// This method converts the coordinates to integer values without applying rounding.
    /// </para>
    /// </remarks>
    public static DX.Size2F Truncate(this DX.Size2F size) => new((int)size.Width, (int)size.Height);

    /// <summary>
    /// Function to set the size coordinates to the nearest integer values that are lower than or equal to the original values.
    /// </summary>
    /// <param name="size">The size to floor.</param>
    /// <returns>The truncated size.</returns>
    public static DX.Size2F Floor(this DX.Size2F size) => new(size.Width.FastFloor(), size.Height.FastFloor());

    /// <summary>
    /// Function to set the size coordinates to the nearest integer values that are higher than or equal to the original values.
    /// </summary>
    /// <param name="size">The size to ceiling.</param>
    /// <returns>The truncated size.</returns>
    public static DX.Size2F Ceiling(this DX.Size2F size) => new(size.Width.FastCeiling(), size.Height.FastCeiling());

    /// <summary>
    /// Function to convert a size into a vector.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The equivalent vector value.</returns>
    public static Vector2 ToVector2(this DX.Size2F size) => new(size.Width, size.Height);

    /// <summary>
    /// Function to convert a size into an integer size.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The equivalent size value.</returns>
    public static DX.Size2 ToSize2(this DX.Size2F size) => new((int)size.Width, (int)size.Height);

    /// <summary>
    /// Function to convert an integer size to a point.
    /// </summary>
    /// <param name="size">The size value to convert.</param>
    /// <returns>The point value.</returns>
    public static GorgonPoint ToPoint(this DX.Size2 size) => new(size.Width, size.Height);

    /// <summary>
    /// Function to convert an integer size to a point.
    /// </summary>
    /// <param name="point">The point value to convert.</param>
    /// <returns>The size value.</returns>
    public static DX.Size2 ToSize2(this GorgonPoint point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert a size into an floating point size.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The equivalent size value.</returns>
    public static DX.Size2F ToSize2F(this DX.Size2 size) => new(size.Width, size.Height);

    /// <summary>
    /// Function to convert a size into a vector.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The equivalent vector value.</returns>
    public static Vector2 ToVector2(this DX.Size2 size) => new(size.Width, size.Height);

    /// <summary>
    /// Function to convert a size into a vector.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The equivalent vector value.</returns>
    public static Vector3 ToVector3(this DX.Size2 size) => new(size.Width, size.Height, 0);

    /// <summary>
    /// Function to convert a size into a vector.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The equivalent vector value.</returns>
    public static Vector4 ToVector4(this DX.Size2 size) => new(size.Width, size.Height, 0, 0);
}
