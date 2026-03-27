// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 26, 2025 1:53:23 PM
//

using System.Numerics;
using System.Runtime.CompilerServices;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines a viewport for rendering.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Get info from D3D help.
/// </para>
/// </remarks>
/// <param name="X">The horizontal position of the viewport, in pixels.</param>
/// <param name="Y">The vertical position of the viewport, in pixels.</param>
/// <param name="Width">The width of the viewport, in pixels.</param>
/// <param name="Height">The height of the viewport, in pixels.</param>
public record struct GorgonViewport(float X, float Y, float Width, float Height)
{
    /// <summary>
    /// An empty version of the <see cref="GorgonViewport"/>.
    /// </summary>
    public static readonly GorgonViewport Empty = new(0, 0, 0, 0)
    {
        MinimumDepth = 0f,
        MaximumDepth = 0f,
    };

    /// <summary>
    /// Property to return whether the viewport is <see cref="Empty"/> or not.
    /// </summary>
    public readonly bool IsEmpty => Equals(Empty);

    /// <summary>
    /// Property to return the minimum depth value for the viewport.
    /// </summary>
    public float MinimumDepth
    {
        get;
        init;
    } = 0.0f;

    /// <summary>
    /// Property to return the maximum depth value for the viewport.
    /// </summary>
    public float MaximumDepth
    {
        get;
        init;
    } = 1.0f;

    /// <summary>
    /// Function to convert the viewport into a D3D 12 viewport data structure.
    /// </summary>
    /// <returns>The D3D viewport.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly D3D12_VIEWPORT ToD3DViewport() => new(X, Y, Width, Height, MinimumDepth, MaximumDepth);

    /// <summary>
    /// Function to convert the viewport to a 2D <see cref="GorgonRectangleF"/>.
    /// </summary>
    /// <returns>The 2D rectangle.</returns>
    /// <seealso cref="GorgonRectangleF"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangleF ToGorgonRectangleF(GorgonViewport viewport) => new(viewport.X, viewport.Y, viewport.Width, viewport.Height);

    /// <summary>
    /// Function to convert the viewport to a 2D <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <returns>The 2D rectangle.</returns>
    /// <seealso cref="GorgonRectangle"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRectangle ToGorgonRectangle(GorgonViewport viewport) => GorgonRectangleF.ToRectangle(ToGorgonRectangleF(viewport));

    /// <summary>
    /// Operator to convert this viewport to a 2D <see cref="GorgonRectangleF"/>.
    /// </summary>
    /// <param name="viewport">The viewport to convert.</param>
    /// <returns>The 2D rectangle.</returns>
    /// <seealso cref="GorgonRectangleF"/>
    public static explicit operator GorgonRectangleF(GorgonViewport viewport) => ToGorgonRectangleF(viewport);

    /// <summary>
    /// Operator to convert this viewport to a 2D <see cref="GorgonRectangle"/>.
    /// </summary>
    /// <param name="viewport">The viewport to convert.</param>
    /// <returns>The 2D rectangle.</returns>
    /// <seealso cref="GorgonRectangle"/>
    public static explicit operator GorgonRectangle(GorgonViewport viewport) => ToGorgonRectangle(viewport);

    /// <summary>
    /// Function to convert the viewport to a 3D <see cref="GorgonBoxF"/>
    /// </summary>
    /// <param name="viewport">The viewport to convert.</param>
    /// <returns>The 3D box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBoxF ToGorgonBoxF(GorgonViewport viewport) => new(viewport.X, viewport.Y, viewport.MinimumDepth, viewport.Width, viewport.Height, viewport.MaximumDepth);

    /// <summary>
    /// Function to convert the viewport to a 3D <see cref="GorgonBox"/>
    /// </summary>
    /// <param name="viewport">The viewport to convert.</param>
    /// <returns>The 3D box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBox ToGorgonBox(GorgonViewport viewport)
    {
        GorgonBoxF box = ToGorgonBoxF(viewport);
        return GorgonBoxF.ToGorgonBox(in box);
    }

    /// <summary>
    /// Operator to convert this viewport to a 3D <see cref="GorgonBoxF"/>.
    /// </summary>
    /// <param name="viewport">The viewport to convert.</param>
    /// <returns>The 3D box.</returns>
    /// <seealso cref="GorgonRectangleF"/>
    public static explicit operator GorgonBoxF(GorgonViewport viewport) => ToGorgonBoxF(viewport);

    /// <summary>
    /// Operator to convert this viewport to a 3D <see cref="GorgonBox"/>.
    /// </summary>
    /// <param name="viewport">The viewport to convert.</param>
    /// <returns>The 3D box.</returns>
    /// <seealso cref="GorgonRectangle"/>
    public static explicit operator GorgonBox(GorgonViewport viewport) => ToGorgonBox(viewport);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonViewport"/> value type.
    /// </summary>
    /// <param name="box">The box defining the dimensions of the viewport.</param>
    public GorgonViewport(GorgonBoxF box)
        : this(box.X, box.Y, box.Width, box.Height)
    {
        MinimumDepth = box.Z;
        MaximumDepth = box.Depth;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonViewport"/> value type.
    /// </summary>
    /// <param name="box">The box defining the dimensions of the viewport.</param>
    public GorgonViewport(GorgonBox box)
        : this(box.X, box.Y, box.Width, box.Height)
    {
        MinimumDepth = box.Z;
        MaximumDepth = box.Depth;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonViewport"/> value type.
    /// </summary>
    /// <param name="rect">The rectangle defining the start location and size of the viewport.</param>
    public GorgonViewport(GorgonRectangleF rect)
        : this(rect.X, rect.Y, rect.Width, rect.Height)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonViewport"/> value type.
    /// </summary>
    /// <param name="rect">The rectangle defining the start location and size of the viewport.</param>
    public GorgonViewport(GorgonRectangle rect)
        : this(rect.X, rect.Y, rect.Width, rect.Height)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonViewport"/> value type.
    /// </summary>
    /// <param name="position">The position of the viewport, in pixels.</param>
    /// <param name="size">The size of the viewport, in pixels.</param>
    public GorgonViewport(GorgonPoint position, GorgonPoint size)
        : this(position.X, position.Y, size.X, size.Y)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonViewport"/> value type.
    /// </summary>
    /// <param name="position">The position of the viewport, in pixels.</param>
    /// <param name="size">The size of the viewport, in pixels.</param>
    public GorgonViewport(Vector2 position, Vector2 size)
        : this(position.X, position.Y, size.X, size.Y)
    {
    }
}
