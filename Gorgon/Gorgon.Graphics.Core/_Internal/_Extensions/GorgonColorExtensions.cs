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
// Created: November 11, 2017 12:27:27 PM
// 
#endregion

using SharpDX.Mathematics.Interop;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for the <see cref="GorgonColor"/> type.
/// </summary>
internal static class GorgonColorExtensions
{
    /// <summary>
    /// Function to convert a <see cref="GorgonColor"/> type to a SharpDX raw color 4 type.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The SharpDX raw color 4 type.</returns>
    public static RawColor4 ToRawColor4(this in GorgonColor color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    /// <summary>
    /// Function to convert a SharpDX raw color 4 type to a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The SharpDX raw color 4 type to convert.</param>
    /// <returns>A new <see cref="GorgonColor"/>.</returns>
    public static GorgonColor ToGorgonColor(this RawColor4 color) => new(color.R, color.G, color.B, color.A);

    /// <summary>
    /// Function to convert a <see cref="GorgonColor"/> type to a SharpDX raw color 3 type.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The SharpDX raw color 3 type.</returns>
    public static RawColor3 ToRawColor3(this in GorgonColor color) => new(color.Red, color.Green, color.Blue);

    /// <summary>
    /// Function to convert a SharpDX raw color 3 type to a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The SharpDX raw color 3 type to convert.</param>
    /// <returns>A new <see cref="GorgonColor"/>.</returns>
    public static GorgonColor ToGorgonColor(this RawColor3 color) => new(color.R, color.G, color.B, 1.0f);

    /// <summary>
    /// Function to convert a <see cref="GorgonColor"/> type to a SharpDX color 4 type.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The SharpDX raw color 4 type.</returns>
    public static DX.Color4 ToColor4(this in GorgonColor color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    /// <summary>
    /// Function to convert a SharpDX color 4 type to a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The SharpDX raw color 4 type to convert.</param>
    /// <returns>A new <see cref="GorgonColor"/>.</returns>
    public static GorgonColor ToGorgonColor(this DX.Color4 color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    /// <summary>
    /// Function to convert a <see cref="GorgonColor"/> type to a SharpDX color 3 type.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The SharpDX raw color 3 type.</returns>
    public static DX.Color3 ToColor3(this in GorgonColor color) => new(color.Red, color.Green, color.Blue);

    /// <summary>
    /// Function to convert a SharpDX color 3 type to a <see cref="GorgonColor"/>.
    /// </summary>
    /// <param name="color">The SharpDX raw color 3 type to convert.</param>
    /// <returns>A new <see cref="GorgonColor"/>.</returns>
    public static GorgonColor ToGorgonColor(this DX.Color3 color) => new(color.Red, color.Green, color.Blue, 1.0f);
}
