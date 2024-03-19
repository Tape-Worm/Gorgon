
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
// Created: February 12, 2021 4:41:57 PM
// 


using System.Runtime.CompilerServices;

namespace System.Numerics;

/// <summary>
/// Extension methods for the Plane type
/// </summary>
public static class PlaneExtensions
{
    /// <summary>
    /// Function to build a matrix that can be used to flatten geometry into a shadow on the plane.
    /// </summary>
    /// <param name="plane">The plane to flatten the geometry onto.</param>
    /// <param name="lightDirection">The direction of the light. If the W component is 0, then the light is considered a directional light, otherwise it is considered a point light.</param>
    /// <param name="result">The matrix that contains the values used to create the shadow.</param>
    /// <remarks>
    /// <para>
    /// This code was adapted from SharpDX (https://github.com/sharpdx/SharpDX).
    /// </para>
    /// </remarks>
    public static void Shadow(this ref readonly Plane plane, Vector4 lightDirection, out Matrix4x4 result)
    {
        float dot = (plane.Normal.X * lightDirection.X) + (plane.Normal.Y * lightDirection.Y) + (plane.Normal.Z * lightDirection.Z) + (plane.D * lightDirection.W);
        float x = -plane.Normal.X;
        float y = -plane.Normal.Y;
        float z = -plane.Normal.Z;
        float d = -plane.D;

        result.M11 = (x * lightDirection.X) + dot;
        result.M21 = y * lightDirection.X;
        result.M31 = z * lightDirection.X;
        result.M41 = d * lightDirection.X;
        result.M12 = x * lightDirection.Y;
        result.M22 = (y * lightDirection.Y) + dot;
        result.M32 = z * lightDirection.Y;
        result.M42 = d * lightDirection.Y;
        result.M13 = x * lightDirection.Z;
        result.M23 = y * lightDirection.Z;
        result.M33 = (z * lightDirection.Z) + dot;
        result.M43 = d * lightDirection.Z;
        result.M14 = x * lightDirection.W;
        result.M24 = y * lightDirection.W;
        result.M34 = z * lightDirection.W;
        result.M44 = (d * lightDirection.W) + dot;
    }

    /// <summary>
    /// Function to build a matrix that can be used to reflect a vector about a plane.
    /// </summary>
    /// <param name="plane">The plane to reflect from.</param>
    /// <param name="result">The matrix that contains reflection values.</param>
    /// <remarks>
    /// <para>
    /// This code was adapted from SharpDX (https://github.com/sharpdx/SharpDX).
    /// </para>
    /// </remarks>
    public static void Reflect(this ref readonly Plane plane, out Matrix4x4 result)
    {
        float x = plane.Normal.X;
        float y = plane.Normal.Y;
        float z = plane.Normal.Z;
        float x2 = -2.0f * x;
        float y2 = -2.0f * y;
        float z2 = -2.0f * z;

        result.M11 = (x2 * x) + 1.0f;
        result.M12 = y2 * x;
        result.M13 = z2 * x;
        result.M14 = 0.0f;
        result.M21 = x2 * y;
        result.M22 = (y2 * y) + 1.0f;
        result.M23 = z2 * y;
        result.M24 = 0.0f;
        result.M31 = x2 * z;
        result.M32 = y2 * z;
        result.M33 = (z2 * z) + 1.0f;
        result.M34 = 0.0f;
        result.M41 = x2 * plane.D;
        result.M42 = y2 * plane.D;
        result.M43 = z2 * plane.D;
        result.M44 = 1.0f;
    }

    /// <summary>
    /// Calculates the dot product of the specified vector and plane.
    /// </summary>
    /// <param name="left">The source plane.</param>
    /// <param name="right">The source vector.</param>
    /// <returns>The dot product of the specified plane and vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Plane left, Vector4 right) => (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z) + (left.D * right.W);

    /// <summary>
    /// Calculates the dot product of a specified vector and the normal of the plane plus the distance value of the plane.
    /// </summary>
    /// <param name="left">The source plane.</param>
    /// <param name="right">The source vector.</param>
    /// <returns>The dot product of a specified vector and the normal of the Plane plus the distance value of the plane.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DotCoordinate(Plane left, Vector3 right) => (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z) + left.D;

    /// <summary>
    /// Calculates the dot product of the specified vector and the normal of the plane.
    /// </summary>
    /// <param name="left">The source plane.</param>
    /// <param name="right">The source vector.</param>
    /// <returns>The dot product of the specified vector and the normal of the plane.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DotNormal(Plane left, Vector3 right) => (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z);

    /// <summary>
    /// Changes the coefficients of the normal vector of the plane to make it of unit length.
    /// </summary>
    /// <param name="plane">The source plane.</param>
    /// <returns>The normalized plane.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Plane Normalize(Plane plane)
    {
        float magnitude = 1.0f / (float)(Math.Sqrt((plane.Normal.X * plane.Normal.X) + (plane.Normal.Y * plane.Normal.Y) + (plane.Normal.Z * plane.Normal.Z)));
        return new Plane(plane.Normal.X * magnitude, plane.Normal.Y * magnitude, plane.Normal.Z * magnitude, plane.D * magnitude);
    }

    /// <summary>
    /// Transforms a normalized plane by a quaternion rotation.
    /// </summary>
    /// <param name="plane">The normalized source plane.</param>
    /// <param name="rotation">The quaternion rotation.</param>
    /// <param name="result">When the method completes, contains the transformed plane.</param>
    public static void Transform(ref readonly Plane plane, ref readonly Quaternion rotation, out Plane result)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;
        float wx = rotation.W * x2;
        float wy = rotation.W * y2;
        float wz = rotation.W * z2;
        float xx = rotation.X * x2;
        float xy = rotation.X * y2;
        float xz = rotation.X * z2;
        float yy = rotation.Y * y2;
        float yz = rotation.Y * z2;
        float zz = rotation.Z * z2;

        float x = plane.Normal.X;
        float y = plane.Normal.Y;
        float z = plane.Normal.Z;

        result.Normal.X = ((x * ((1.0f - yy) - zz)) + (y * (xy - wz))) + (z * (xz + wy));
        result.Normal.Y = ((x * (xy + wz)) + (y * ((1.0f - xx) - zz))) + (z * (yz - wx));
        result.Normal.Z = ((x * (xz - wy)) + (y * (yz + wx))) + (z * ((1.0f - xx) - yy));
        result.D = plane.D;
    }

    /// <summary>
    /// Transforms an array of normalized planes by a quaternion rotation.
    /// </summary>
    /// <param name="planes">The array of normalized planes to transform.</param>
    /// <param name="rotation">The quaternion rotation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="planes"/> is <c>null</c>.</exception>
    public static void Transform(Span<Plane> planes, ref readonly Quaternion rotation)
    {
        if (planes.IsEmpty)
        {
            return;
        }

        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;
        float wx = rotation.W * x2;
        float wy = rotation.W * y2;
        float wz = rotation.W * z2;
        float xx = rotation.X * x2;
        float xy = rotation.X * y2;
        float xz = rotation.X * z2;
        float yy = rotation.Y * y2;
        float yz = rotation.Y * z2;
        float zz = rotation.Z * z2;

        for (int i = 0; i < planes.Length; ++i)
        {
            float x = planes[i].Normal.X;
            float y = planes[i].Normal.Y;
            float z = planes[i].Normal.Z;

            /*
             * Note:
             * Factor common arithmetic out of loop.
            */
            planes[i].Normal.X = ((x * ((1.0f - yy) - zz)) + (y * (xy - wz))) + (z * (xz + wy));
            planes[i].Normal.Y = ((x * (xy + wz)) + (y * ((1.0f - xx) - zz))) + (z * (yz - wx));
            planes[i].Normal.Z = ((x * (xz - wy)) + (y * (yz + wx))) + (z * ((1.0f - xx) - yy));
        }
    }

    /// <summary>
    /// Transforms a normalized plane by a matrix.
    /// </summary>
    /// <param name="plane">The normalized source plane.</param>
    /// <param name="transformation">The transformation matrix.</param>
    /// <param name="result">When the method completes, contains the transformed plane.</param>
    public static void Transform(ref readonly Plane plane, ref readonly Matrix4x4 transformation, out Plane result)
    {
        float x = plane.Normal.X;
        float y = plane.Normal.Y;
        float z = plane.Normal.Z;
        float d = plane.D;

        Matrix4x4.Invert(transformation, out Matrix4x4 inverse);

        result.Normal.X = (((x * inverse.M11) + (y * inverse.M12)) + (z * inverse.M13)) + (d * inverse.M14);
        result.Normal.Y = (((x * inverse.M21) + (y * inverse.M22)) + (z * inverse.M23)) + (d * inverse.M24);
        result.Normal.Z = (((x * inverse.M31) + (y * inverse.M32)) + (z * inverse.M33)) + (d * inverse.M34);
        result.D = (((x * inverse.M41) + (y * inverse.M42)) + (z * inverse.M43)) + (d * inverse.M44);
    }

    /// <summary>
    /// Transforms an array of normalized planes by a matrix.
    /// </summary>
    /// <param name="planes">The array of normalized planes to transform.</param>
    /// <param name="transformation">The transformation matrix.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Transform(Span<Plane> planes, ref readonly Matrix4x4 transformation)
    {
        if (planes.IsEmpty)
        {
            return;
        }

        for (int i = 0; i < planes.Length; ++i)
        {
            Transform(in planes[i], in transformation, out planes[i]);
        }
    }
}
