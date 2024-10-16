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
// Created: February 12, 2021 1:03:15 PM
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

using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Represents an axis-aligned bounding box in three dimensional space
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct GorgonBoundingBox
    : IGorgonEquatableByRef<GorgonBoundingBox>
{
    /// <summary>
    /// A default, empty, bounding box.
    /// </summary>
    public static readonly GorgonBoundingBox Empty = default;

    /// <summary>
    /// The minimum point of the box.
    /// </summary>
    public readonly Vector3 Minimum;

    /// <summary>
    /// The maximum point of the box.
    /// </summary>
    public readonly Vector3 Maximum;

    /// <summary>
    /// Returns the width of the bounding box
    /// </summary>
    public float Width => Maximum.X - Minimum.X;

    /// <summary>
    /// Returns the height of the bounding box
    /// </summary>
    public float Height => Maximum.Y - Minimum.Y;

    /// <summary>
    /// Returns the height of the bounding box
    /// </summary>
    public float Depth => Maximum.Z - Minimum.Z;

    /// <summary>
    /// Returns the size of the bounding box
    /// </summary>
    public Vector3 Size => Maximum - Minimum;

    /// <summary>
    /// Returns the size of the bounding box
    /// </summary>
    public Vector3 Center => (Maximum + Minimum) * 0.5f;

    /// <summary>
    /// Property to return the left extent.
    /// </summary>
    public float Left => Minimum.X;

    /// <summary>
    /// Property to return the top extent.
    /// </summary>
    public float Top => Minimum.Y;

    /// <summary>
    /// Property to return the right extent.
    /// </summary>
    public float Right => Maximum.X;

    /// <summary>
    /// Property to return the bottom extent.
    /// </summary>
    public float Bottom => Maximum.Y;

    /// <summary>
    /// Property to return the front extent.
    /// </summary>
    public float Front => Minimum.Z;

    /// <summary>
    /// Property to return the back extent.
    /// </summary>
    public float Back => Maximum.Z;

    /// <summary>
    /// Property to return the top left point at the front of the AABB.
    /// </summary>
    public Vector3 TopLeftFront => Minimum;

    /// <summary>
    /// Property to return the top right point at the front of the AABB.
    /// </summary>
    public Vector3 TopRightFront => new(Maximum.X, Minimum.Y, Minimum.Z);

    /// <summary>
    /// Property to return the bottom left point at the front of the AABB.
    /// </summary>
    public Vector3 BottomLeftFront => new(Minimum.X, Maximum.Y, Minimum.Z);

    /// <summary>
    /// Property to return the bottom right point at the front of the AABB.
    /// </summary>
    public Vector3 BottomRightFront => new(Maximum.X, Maximum.Y, Minimum.Z);

    /// <summary>
    /// Property to return the top left point at the rear of the AABB.
    /// </summary>
    public Vector3 TopLeftBack => new(Minimum.X, Minimum.Y, Maximum.Z);

    /// <summary>
    /// Property to return the top right point at the rear of the AABB.
    /// </summary>
    public Vector3 TopRightBack => new(Maximum.X, Minimum.Y, Maximum.Z);

    /// <summary>
    /// Property to return the bottom left point at the rear of the AABB.
    /// </summary>
    public Vector3 BottomLeftBack => new(Minimum.X, Maximum.Y, Maximum.Z);

    /// <summary>
    /// Property to return the bottom right point at the rear of the AABB.
    /// </summary>
    public Vector3 BottomRightBack => Maximum;

    /// <summary>
    /// Property to return whether the AABB is empty or not.
    /// </summary>
    public bool IsEmpty => (Width.EqualsEpsilon(0)) && (Height.EqualsEpsilon(0)) && (Depth.EqualsEpsilon(0));

    /// <summary>
    /// Property to return each of the 8 corners for the AABB.
    /// </summary>
    public Vector3 this[int index] => index switch
    {
        0 => Minimum,
        1 => TopRightFront,
        2 => BottomLeftFront,
        3 => BottomRightFront,
        4 => TopLeftBack,
        5 => TopRightBack,
        6 => BottomLeftBack,
        7 => Maximum,
        _ => new Vector3(float.NaN),
    };

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingBox"/> that fully contains the given points.
    /// </summary>
    /// <param name="points">The points that will be contained by the box.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    public static void FromPoints(ReadOnlySpan<Vector3> points, out GorgonBoundingBox result)
    {
        if (points.IsEmpty)
        {
            result = default;
            return;
        }

        Vector3 min = new(float.MaxValue);
        Vector3 max = new(float.MinValue);

        for (int i = 0; i < points.Length; ++i)
        {
            min = min.Min(points[i]);
            max = min.Max(points[i]);
        }

        result = new GorgonBoundingBox(min, max);
    }

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingBox"/> that fully contains the given points.
    /// </summary>
    /// <param name="points">The points that will be contained by the box.</param>
    /// <returns>When the method completes, contains the newly constructed bounding box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBoundingBox FromPoints(ReadOnlySpan<Vector3> points)
    {
        FromPoints(points, out GorgonBoundingBox result);
        return result;
    }

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingBox"/> from a given sphere.
    /// </summary>
    /// <param name="sphere">The sphere that will designate the extents of the box.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FromSphere(GorgonBoundingSphere sphere, out GorgonBoundingBox result)
                           => result = new GorgonBoundingBox(new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius),
                                       new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius));

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingBox"/> from a given sphere.
    /// </summary>
    /// <param name="sphere">The sphere that will designate the extents of the box.</param>
    /// <returns>When the method completes, contains the newly constructed bounding box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBoundingBox FromSphere(GorgonBoundingSphere sphere)
    {
        FromSphere(sphere, out GorgonBoundingBox result);
        return result;
    }

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingBox"/> that is as large as the total combined area of the two specified boxes.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="value2">The second box to merge.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Merge(ref readonly GorgonBoundingBox value1, ref readonly GorgonBoundingBox value2, out GorgonBoundingBox result)
        => result = new GorgonBoundingBox(value1.Minimum.Min(value2.Minimum), value1.Maximum.Max(value2.Maximum));

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingBox"/> that is as large as the total combined area of the two specified boxes.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="value2">The second box to merge.</param>
    /// <returns>When the method completes, contains the newly constructed bounding box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBoundingBox Merge(GorgonBoundingBox value1, GorgonBoundingBox value2)
    {
        Merge(in value1, in value2, out GorgonBoundingBox result);
        return result;
    }

    /// <summary>
    /// Function to intersect two Axis Aligned Bounding Boxes.
    /// </summary>
    /// <param name="aabb1">The first axis aligned bounding box.</param>
    /// <param name="aabb2">The second axis aligned bounding box.</param>
    /// <param name="result">The intersection of both bounding boxes.</param>
    public static void Intersect(ref readonly GorgonBoundingBox aabb1, ref readonly GorgonBoundingBox aabb2, out GorgonBoundingBox result)
    {
        float left = aabb2.Minimum.X.Max(aabb1.Minimum.X);
        float top = aabb2.Minimum.Y.Max(aabb1.Minimum.Y);
        float front = aabb2.Minimum.Z.Max(aabb1.Minimum.Z);

        float right = aabb2.Maximum.X.Min(aabb1.Maximum.X);
        float bottom = aabb2.Maximum.Y.Min(aabb1.Maximum.Y);
        float back = aabb2.Maximum.Z.Min(aabb1.Maximum.Z);

        if ((right < left) || (bottom < top) || (back < front))
        {
            result = default;
            return;
        }

        result = new GorgonBoundingBox(new Vector3(left, top, front), new Vector3(right, bottom, back));
    }

    /// <summary>
    /// Function to intersect two Axis Aligned Bounding Boxes.
    /// </summary>
    /// <param name="aabb1">The first axis aligned bounding box.</param>
    /// <param name="aabb2">The second axis aligned bounding box.</param>
    /// <returns>The intersection of both bounding boxes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonBoundingBox Intersect(GorgonBoundingBox aabb1, GorgonBoundingBox aabb2)
    {
        Intersect(in aabb1, in aabb2, out GorgonBoundingBox result);
        return result;
    }

    /// <summary>
    /// Function to transform an AABB by a world matrix.
    /// </summary>
    /// <param name="aabb">The axis aligned bounding box to transform.</param>
    /// <param name="worldMatrix">The world matrix to multiply by.</param>
    /// <param name="result">The new transformed axis aligned bounding box.</param>
    public static void Transform(ref readonly GorgonBoundingBox aabb, ref readonly Matrix4x4 worldMatrix, out GorgonBoundingBox result)
    {
        Vector3 extent = Vector3.Subtract(aabb.Maximum, aabb.Center);
        worldMatrix.Abs(out Matrix4x4 absMatrix);

        Vector3 newCenter = Vector3.Transform(aabb.Center, worldMatrix);
        Vector3 newExtent = Vector3.TransformNormal(extent, absMatrix);

        result = new GorgonBoundingBox(Vector3.Subtract(newCenter, newExtent), Vector3.Add(newCenter, newExtent));
    }

    /// <summary>
    /// Function to transform an AABB by a world matrix.
    /// </summary>
    /// <param name="aabb">The axis aligned bounding box to transform.</param>
    /// <param name="worldMatrix">The world matrix to multiply by.</param>
    /// <returns>The new transformed axis aligned bounding box.</returns>
    public static GorgonBoundingBox Transform(GorgonBoundingBox aabb, Matrix4x4 worldMatrix)
    {
        Transform(in aabb, in worldMatrix, out GorgonBoundingBox result);
        return result;
    }

    /// <summary>
    /// Tests for equality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(GorgonBoundingBox left, GorgonBoundingBox right) => left.Equals(in right);

    /// <summary>
    /// Tests for inequality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(GorgonBoundingBox left, GorgonBoundingBox right) => !left.Equals(in right);

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, Resources.GORGFX_TOSTR_AABB, Minimum.X, Minimum.Y, Minimum.Z, Maximum.X, Maximum.Y, Maximum.Z);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Minimum, Maximum);

    /// <summary>
    /// Determines whether the specified <see cref="Vector4"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="Vector4"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Vector4"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GorgonBoundingBox value) => Equals(in this, in value);

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object value) => (value is GorgonBoundingBox bbox) ? Equals(in this, in bbox) : base.Equals(value);

    /// <summary>Deconstructs this instance.</summary>
    /// <param name="min">The minimum extent.</param>
    /// <param name="max">The maximum extent.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out Vector3 min, out Vector3 max)
    {
        min = Minimum;
        max = Maximum;
    }

    /// <summary>Deconstructs this instance.</summary>
    /// <param name="topLeftFront">The top left front corner.</param>
    /// <param name="topRightFront">The top right front corner.</param>
    /// <param name="bottomLeftFront">The bottom left front corner.</param>
    /// <param name="bottomRightFront">The bottom right front corner.</param>
    /// <param name="topLeftBack">The top left back corner.</param>
    /// <param name="topRightBack">The top right back corner.</param>
    /// <param name="bottomLeftBack">The bottom left back corner.</param>
    /// <param name="bottomRightBack">The bottom right back corner.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out Vector3 topLeftFront, out Vector3 topRightFront, out Vector3 bottomLeftFront, out Vector3 bottomRightFront,
                            out Vector3 topLeftBack, out Vector3 topRightBack, out Vector3 bottomLeftBack, out Vector3 bottomRightBack)
    {
        topLeftFront = TopLeftFront;
        topRightFront = TopRightFront;
        bottomLeftFront = BottomLeftFront;
        bottomRightFront = BottomRightFront;
        topLeftBack = TopLeftBack;
        topRightBack = TopRightBack;
        bottomLeftBack = BottomLeftBack;
        bottomRightBack = BottomRightBack;
    }

    /// <summary>
    /// Function to compare two values for equality.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(ref readonly GorgonBoundingBox left, ref readonly GorgonBoundingBox right) => left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum);

    /// <summary>Function to compare this instance with another.</summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns>
    ///   <b>true</b> if equal, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ref readonly GorgonBoundingBox other) => Equals(in this, in other);

    /// <summary>Initializes a new instance of the <see cref="GorgonBoundingBox" /> struct.</summary>
    /// <param name="minX">The minimum x.</param>
    /// <param name="minY">The minimum y.</param>
    /// <param name="minZ">The minimum z.</param>
    /// <param name="maxX">The maximum x.</param>
    /// <param name="maxY">The maximum y.</param>
    /// <param name="maxZ">The maximum z.</param>
    public GorgonBoundingBox(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        Minimum = new Vector3(minX, minY, minZ);
        Maximum = new Vector3(maxX, maxY, maxZ);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBoundingBox"/> struct.
    /// </summary>
    /// <param name="minimum">The minimum vertex of the bounding box.</param>
    /// <param name="maximum">The maximum vertex of the bounding box.</param>
    public GorgonBoundingBox(Vector3 minimum, Vector3 maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }
}
