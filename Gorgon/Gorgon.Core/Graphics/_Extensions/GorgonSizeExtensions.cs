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
/// Extension methods for the <see cref="Size"/> and <see cref="SizeF"/> types.
/// </summary>
public static class GorgonSizeExtensions
{
    /// <summary>
    /// Function to convert a <see cref="SizeF"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="size">The size to convert.</param>    
    /// <returns>The converted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this SizeF size) => new(size.Width, size.Height);

    /// <summary>
    /// Function to convert a <see cref="Size"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="size">The size to convert.</param>    
    /// <returns>The converted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this Size size) => new(size.Width, size.Height);

    /// <summary>
    /// Function to convert a <see cref="Vector2"/> to a <see cref="SizeF"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The converted size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SizeF ToSizeF(this Vector2 vector) => new(vector.X, vector.Y);

    /// <summary>
    /// Function to convert a <see cref="Vector2"/> to a <see cref="Size"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The converted size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size ToSize(this Vector2 vector) => new((int)vector.X, (int)vector.Y);
}
