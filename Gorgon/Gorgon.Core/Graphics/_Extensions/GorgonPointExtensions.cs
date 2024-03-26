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
// Created: December 18, 2023 7:06:47 PM
//

using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Gorgon.Graphics;

/// <summary>
/// Extension methods for the <see cref="Point"/> and <see cref="PointF"/> types.
/// </summary>
public static class GorgonPointExtensions
{
    /// <summary>
    /// Function to convert a <see cref="Point"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this Point point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert a <see cref="PointF"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>The converted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this PointF point) => new(point.X, point.Y);

    /// <summary>
    /// Function to convert a <see cref="Vector2"/> to a <see cref="PointF"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The converted point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF ToPointF(this Vector2 vector) => new(vector.X, vector.Y);

    /// <summary>
    /// Function to convert a <see cref="Vector2"/> to a <see cref="PointF"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The converted point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point ToPoint(this Vector2 vector) => new((int)vector.X, (int)vector.Y);
}
