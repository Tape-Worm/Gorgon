#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: January 30, 2021 3:43:05 PM
// 
#endregion

using System.Runtime.CompilerServices;
using Gorgon.Math;
using DX = SharpDX;

namespace System.Numerics
{
    /// <summary>
    /// Extension methods for vectors.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Function to truncate the vector coordinates to the whole number portion of their values.
        /// </summary>
        /// <param name="vec">The vector to truncate.</param>
        /// <returns>The truncated vector.</returns>
        /// <remarks>
        /// <para>
        /// This method converts the coordinates to integer values without applying rounding.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Truncate(this Vector4 vec) => new Vector4((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Floor(this Vector4 vec) => new Vector4(vec.X.FastFloor(), vec.Y.FastFloor(), vec.Z.FastFloor(), vec.W.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Ceiling(this Vector4 vec) => new Vector4(vec.X.FastCeiling(), vec.Y.FastCeiling(), vec.Z.FastCeiling(), vec.W.FastCeiling());

        /// <summary>
        /// Function to truncate the vector coordinates to the whole number portion of their values.
        /// </summary>
        /// <param name="vec">The vector to truncate.</param>
        /// <returns>The truncated vector.</returns>
        /// <remarks>
        /// <para>
        /// This method converts the coordinates to integer values without applying rounding.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Truncate(this Vector3 vec) => new Vector3((int)vec.X, (int)vec.Y, (int)vec.Z);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(this Vector3 vec) => new Vector3(vec.X.FastFloor(), vec.Y.FastFloor(), vec.Z.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Ceiling(this Vector3 vec) => new Vector3(vec.X.FastCeiling(), vec.Y.FastCeiling(), vec.Z.FastCeiling());

        /// <summary>
        /// Function to truncate the vector coordinates to the whole number portion of their values.
        /// </summary>
        /// <param name="vec">The vector to truncate.</param>
        /// <returns>The truncated vector.</returns>
        /// <remarks>
        /// <para>
        /// This method converts the coordinates to integer values without applying rounding.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Truncate(this Vector2 vec) => new Vector2((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Floor(this Vector2 vec) => new Vector2(vec.X.FastFloor(), vec.Y.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Ceiling(this Vector2 vec) => new Vector2(vec.X.FastCeiling(), vec.Y.FastCeiling());

        /// <summary>
        /// Function to convert a vector into a size.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent size value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Size2F ToSize2F(this Vector2 vec) => new DX.Size2F(vec.X, vec.Y);

        /// <summary>
        /// Function to convert a vector into a size.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent size value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Size2 ToSize2(this Vector2 vec) => new DX.Size2((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to convert a vector into a point.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent point value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Point ToPoint(this Vector2 vec) => new DX.Point((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to convert a point to a 2D vector.
        /// </summary>
        /// <param name="pt">The point to convert.</param>
        /// <returns>The equivalent 2D vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this DX.Point pt) => new Vector2(pt.X, pt.Y);

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The minimum vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(this Vector2 left, Vector2 right) => new Vector2(left.X.Min(right.X), left.Y.Min(right.Y));

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The maximum vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(this Vector2 left, Vector2 right) => new Vector2(left.X.Max(right.X), left.Y.Max(right.Y));

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The minimum vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(this Vector3 left, Vector3 right) => new Vector3(left.X.Min(right.X), left.Y.Min(right.Y), left.Z.Min(right.Z));

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The maximum vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(this Vector3 left, Vector3 right) => new Vector3(left.X.Max(right.X), left.Y.Max(right.Y), left.Z.Max(right.Z));

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The minimum vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Min(this Vector4 left, Vector4 right) => new Vector4(left.X.Min(right.X), left.Y.Min(right.Y), left.Z.Min(right.Z), left.W.Min(right.W));

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The maximum vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Max(this Vector4 left, Vector4 right) => new Vector4(left.X.Max(right.X), left.Y.Max(right.Y), left.Z.Max(right.Z), left.W.Max(right.W));

        /// <summary>
        /// Projects a 3D vector from object space into screen space. 
        /// </summary>
        /// <param name="vector">The vector to project.</param>
        /// <param name="x">The X position of the viewport.</param>
        /// <param name="y">The Y position of the viewport.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        /// <param name="minZ">The minimum depth of the viewport.</param>
        /// <param name="maxZ">The maximum depth of the viewport.</param>
        /// <param name="worldViewProjection">The combined world-view-projection matrix.</param>
        /// <returns>The vector in screen space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Project(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix4x4 worldViewProjection)
        {
            var v = Vector3.Transform(vector, worldViewProjection);
            return new Vector3(((1.0f + v.X) * 0.5f * width) + x, ((1.0f - v.Y) * 0.5f * height) + y, (v.Z * (maxZ - minZ)) + minZ);
        }

        /// <summary>
        /// Projects a 3D vector from screen space into object space. 
        /// </summary>
        /// <param name="vector">The vector to project.</param>
        /// <param name="x">The X position of the viewport.</param>
        /// <param name="y">The Y position of the viewport.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        /// <param name="minZ">The minimum depth of the viewport.</param>
        /// <param name="maxZ">The maximum depth of the viewport.</param>
        /// <param name="worldViewProjection">The combined world-view-projection matrix.</param>
        /// <returns>The vector in object space</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Unproject(this Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, in Matrix4x4 worldViewProjection)
        {
            var v = new Vector3();
            Matrix4x4.Invert(worldViewProjection, out Matrix4x4 matrix);

            v.X = (((vector.X - x) / width) * 2.0f) - 1.0f;
            v.Y = -((((vector.Y - y) / height) * 2.0f) - 1.0f);
            v.Z = (vector.Z - minZ) / (maxZ - minZ);

            return Vector3.Transform(vector, matrix);
        }

    }
}
