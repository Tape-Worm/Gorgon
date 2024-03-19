
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
// Created: February 12, 2021 1:49:23 PM
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
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Renderers.Data;

/// <summary>
/// Represents a bounding sphere in three dimensional space
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonBoundingSphere"/> struct
/// </remarks>
/// <param name="center">The center of the sphere in three dimensional space.</param>
/// <param name="radius">The radius of the sphere.</param>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct GorgonBoundingSphere(in Vector3 center, float radius)
        : IGorgonEquatableByRef<GorgonBoundingSphere>
{

    /// <summary>
    /// A default, empty, bounding sphere.
    /// </summary>
    public static readonly GorgonBoundingSphere Empty = default;

    /// <summary>
    /// The center of the sphere in three dimensional space.
    /// </summary>
    public readonly Vector3 Center = center;

    /// <summary>
    /// The radius of the sphere.
    /// </summary>
    public readonly float Radius = radius;



    /// <summary>
    /// Property to return whether this bounding sphere is empty or not.
    /// </summary>
    public bool IsEmpty => Radius.EqualsEpsilon(0);



    /// <summary>
    /// Constructs a <see cref="GorgonBoundingSphere" /> that fully contains the given points.
    /// </summary>
    /// <param name="points">The points that will be contained by the sphere.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
    public static void FromPoints(Span<Vector3> points, out GorgonBoundingSphere result)
    {
        if (points.IsEmpty)
        {
            result = default;
            return;
        }

        //Find the center of all points.
        Vector3 center = Vector3.Zero;
        for (int i = 0; i < points.Length; ++i)
        {
            center = Vector3.Add(points[i], center);
        }

        //This is the center of our sphere.
        center /= points.Length;

        //Find the radius of the sphere
        float radius = 0f;
        for (int i = 0; i < points.Length; ++i)
        {
            //We are doing a relative distance comparison to find the maximum distance
            //from the center of our sphere.
            float distance = Vector3.DistanceSquared(center, points[i]);

            if (distance > radius)
            {
                radius = distance;
            }
        }

        //Construct the sphere.
        result = new GorgonBoundingSphere(center, radius.Sqrt());
    }

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingSphere"/> from a given box.
    /// </summary>
    /// <param name="box">The box that will designate the extents of the sphere.</param>
    /// <param name="result">When the method completes, the newly constructed bounding sphere.</param>
    public static void FromBox(in GorgonBoundingBox box, out GorgonBoundingSphere result)
    {
        Vector3 center = Vector3.Lerp(box.Minimum, box.Maximum, 0.5f);

        float x = box.Minimum.X - box.Maximum.X;
        float y = box.Minimum.Y - box.Maximum.Y;
        float z = box.Minimum.Z - box.Maximum.Z;

        result = new GorgonBoundingSphere(center, ((x * x) + (y * y) + (z * z)).Sqrt() * 0.5f);
    }

    /// <summary>
    /// Constructs a <see cref="GorgonBoundingSphere"/> that is the as large as the total combined area of the two specified spheres.
    /// </summary>
    /// <param name="value1">The first sphere to merge.</param>
    /// <param name="value2">The second sphere to merge.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
    public static void Merge(in GorgonBoundingSphere value1, in GorgonBoundingSphere value2, out GorgonBoundingSphere result)
    {
        Vector3 difference = value2.Center - value1.Center;

        float length = difference.Length();
        float radius = value1.Radius;
        float radius2 = value2.Radius;

        if (radius + radius2 >= length)
        {
            if (radius - radius2 >= length)
            {
                result = value1;
                return;
            }

            if (radius2 - radius >= length)
            {
                result = value2;
                return;
            }
        }

        Vector3 vector = difference * (1.0f / length);
        float min = -radius.Min(length - radius2);
        float max = (radius.Max(length + radius2) - min) * 0.5f;

        result = new GorgonBoundingSphere(value1.Center + vector * (max + min), max);
    }

    /// <summary>
    /// Tests for equality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in GorgonBoundingSphere left, in GorgonBoundingSphere right) => left.Equals(in right);

    /// <summary>
    /// Tests for inequality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in GorgonBoundingSphere left, in GorgonBoundingSphere right) => !left.Equals(in right);

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, Resources.GOR_TOSTR_BOUNDING_SPHERE, Center.X, Center.Y, Center.Z, Radius);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            return HashCode.Combine(Center, Radius);
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="Vector4"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="Vector4"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Vector4"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GorgonBoundingSphere value) => Equals(in value);

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object value) => (value is GorgonBoundingSphere sphere) ? this.Equals(in sphere) : base.Equals(value);

    /// <summary>Function to compare this instance with another.</summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns>
    ///   <b>true</b> if equal, <b>false</b> if not.</returns>
    public bool Equals(ref readonly GorgonBoundingSphere other) => (Center.Equals(other.Center)) && (Radius == other.Radius);


    /// <summary>Deconstructs this instance.</summary>
    /// <param name="center">The center of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out Vector3 center, out float radius)
    {
        center = Center;
        radius = Radius;
    }
}
