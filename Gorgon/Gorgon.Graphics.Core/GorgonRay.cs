﻿
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
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
// Created: February 12, 2021 4:11:22 PM
// 

// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Represents a three dimensional line based on a point in space and a direction
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonRay"/> struct
/// </remarks>
/// <param name="position">The position in three dimensional space of the origin of the ray.</param>
/// <param name="direction">The normalized direction of the ray.</param>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct GorgonRay(Vector3 position, Vector3 direction)
        : IGorgonEquatableByRef<GorgonRay>
{
    /// <summary>
    /// The position in three dimensional space where the ray starts.
    /// </summary>
    public Vector3 Position = position;

    /// <summary>
    /// The normalized direction in which the ray points.
    /// </summary>
    public Vector3 Direction = direction;

    /// <summary>
    /// Calculates a world space <see cref="GorgonRay"/> from 2d screen coordinates.
    /// </summary>
    /// <param name="x">X coordinate on 2d screen.</param>
    /// <param name="y">Y coordinate on 2d screen.</param>
    /// <param name="viewport">The viewport to use.</param>
    /// <param name="worldViewProjection">The world, view, projection matrix.</param>
    /// <param name="result">The resulting world space ray.</param>
    public static void GetPickRay(int x, int y, ref readonly DX.ViewportF viewport, ref readonly Matrix4x4 worldViewProjection, out GorgonRay result)
    {
        Vector3 nearPoint = new(x, y, 0);
        Vector3 farPoint = new(x, y, 1);

        nearPoint = nearPoint.Unproject(viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth, in worldViewProjection);
        farPoint = farPoint.Unproject(viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth, in worldViewProjection);

        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

        result = new GorgonRay(nearPoint, direction);
    }

    /// <summary>
    /// Calculates a world space <see cref="GorgonRay"/> from 2d screen coordinates.
    /// </summary>
    /// <param name="x">X coordinate on 2d screen.</param>
    /// <param name="y">Y coordinate on 2d screen.</param>
    /// <param name="viewport">The viewport to use.</param>
    /// <param name="worldViewProjection">The world, view, projection matrix.</param>
    /// <returns>The resulting world space ray.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonRay GetPickRay(int x, int y, DX.ViewportF viewport, Matrix4x4 worldViewProjection)
    {
        GetPickRay(x, y, ref viewport, ref worldViewProjection, out GorgonRay result);
        return result;
    }

    /// <summary>
    /// Tests for equality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(GorgonRay left, GorgonRay right) => left.Equals(in right);

    /// <summary>
    /// Tests for inequality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(GorgonRay left, GorgonRay right) => !left.Equals(in right);

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override readonly string ToString() => string.Format(CultureInfo.CurrentCulture, Resources.GORGFX_TOSTR_RAY, Position.X, Position.Y, Position.Z, Direction.X, Direction.Y, Direction.Z);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override readonly int GetHashCode() => HashCode.Combine(Position, Direction);

    /// <summary>
    /// Determines whether the specified <see cref="Vector4"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="Vector4"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Vector4"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly bool IEquatable<GorgonRay>.Equals(GorgonRay value) => Equals(in this, in value);

    /// <summary>
    /// Function to compare two values for equality.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(ref readonly GorgonRay left, ref readonly GorgonRay right) => left.Position.Equals(right.Position) && left.Direction.Equals(right.Direction);

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override readonly bool Equals(object value) => (value is GorgonRay ray) ? Equals(in this, in ray) : base.Equals(value);

    /// <summary>Function to compare this instance with another.</summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns>
    ///   <b>true</b> if equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(ref readonly GorgonRay other) => Equals(in this, in other);
}
