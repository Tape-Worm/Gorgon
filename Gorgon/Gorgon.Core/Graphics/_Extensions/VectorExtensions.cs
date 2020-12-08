#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 6, 2019 10:19:56 PM
// 
#endregion

using System.Numerics;
using DX = SharpDX;
using Gorgon.Math;

namespace Gorgon.Graphics
{
    /// <summary>
    /// Extension methods for the SharpDX Vector types.
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
        public static Vector4 Truncate(this Vector4 vec) => new Vector4((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        public static Vector4 Floor(this Vector4 vec) => new Vector4(vec.X.FastFloor(), vec.Y.FastFloor(), vec.Z.FastFloor(), vec.W.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
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
        public static Vector3 Truncate(this Vector3 vec) => new Vector3((int)vec.X, (int)vec.Y, (int)vec.Z);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        public static Vector3 Floor(this Vector3 vec) => new Vector3(vec.X.FastFloor(), vec.Y.FastFloor(), vec.Z.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
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
        public static Vector2 Truncate(this Vector2 vec) => new Vector2((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        public static Vector2 Floor(this Vector2 vec) => new Vector2(vec.X.FastFloor(), vec.Y.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
        public static Vector2 Ceiling(this Vector2 vec) => new Vector2(vec.X.FastCeiling(), vec.Y.FastCeiling());

        /// <summary>
        /// Function to convert a vector into a size.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent size value.</returns>
        public static DX.Size2F ToSize2F(this Vector2 vec) => new DX.Size2F(vec.X, vec.Y);

        /// <summary>
        /// Function to convert a vector into a size.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent size value.</returns>
        public static DX.Size2 ToSize2(this Vector2 vec) => new DX.Size2((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to convert a vector into a point.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent point value.</returns>
        public static DX.Point ToPoint(this Vector2 vec) => new DX.Point((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to convert a point to a 2D vector.
        /// </summary>
        /// <param name="pt">The point to convert.</param>
        /// <returns>The equivalent 2D vector value.</returns>
        public static Vector2 ToVector2(this DX.Point pt) => new Vector2(pt.X, pt.Y);
    }
}
