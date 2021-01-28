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

namespace Gorgon.Graphics
{
    /// <summary>
    /// Extension methods for vectors.
    /// </summary>
    public static class SharpDXVectorExtensions
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
        public static DX.Vector4 Truncate(this DX.Vector4 vec) => new DX.Vector4((int)vec.X, (int)vec.Y, (int)vec.Z, (int)vec.W);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Vector4 Floor(this DX.Vector4 vec) => new DX.Vector4(vec.X.FastFloor(), vec.Y.FastFloor(), vec.Z.FastFloor(), vec.W.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Vector4 Ceiling(this DX.Vector4 vec) => new DX.Vector4(vec.X.FastCeiling(), vec.Y.FastCeiling(), vec.Z.FastCeiling(), vec.W.FastCeiling());

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
        public static DX.Vector3 Truncate(this DX.Vector3 vec) => new DX.Vector3((int)vec.X, (int)vec.Y, (int)vec.Z);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Vector3 Floor(this DX.Vector3 vec) => new DX.Vector3(vec.X.FastFloor(), vec.Y.FastFloor(), vec.Z.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Vector3 Ceiling(this DX.Vector3 vec) => new DX.Vector3(vec.X.FastCeiling(), vec.Y.FastCeiling(), vec.Z.FastCeiling());

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
        public static DX.Vector2 Truncate(this DX.Vector2 vec) => new DX.Vector2((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to floor.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Vector2 Floor(this DX.Vector2 vec) => new DX.Vector2(vec.X.FastFloor(), vec.Y.FastFloor());

        /// <summary>
        /// Function to set the vector coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="vec">The vector to ceiling.</param>
        /// <returns>The truncated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Vector2 Ceiling(this DX.Vector2 vec) => new DX.Vector2(vec.X.FastCeiling(), vec.Y.FastCeiling());

        /// <summary>
        /// Function to convert a vector into a size.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent size value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Size2F ToSize2F(this DX.Vector2 vec) => new DX.Size2F(vec.X, vec.Y);

        /// <summary>
        /// Function to convert a vector into a size.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent size value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Size2 ToSize2(this DX.Vector2 vec) => new DX.Size2((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to convert a vector into a point.
        /// </summary>
        /// <param name="vec">The vector to convert.</param>
        /// <returns>The equivalent point value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Point ToPoint(this DX.Vector2 vec) => new DX.Point((int)vec.X, (int)vec.Y);

        /// <summary>
        /// Function to convert a point to a 2D vector.
        /// </summary>
        /// <param name="pt">The point to convert.</param>
        /// <returns>The equivalent 2D vector value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DX.Vector2 ToVector2(this DX.Point pt) => new DX.Vector2(pt.X, pt.Y);
    }
}
