#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 2, 2020 11:45:37 PM
// 
#endregion

using System.Runtime.CompilerServices;

namespace System.Numerics
{
    /// <summary>
    /// Extension methods for working with values in the System.Numerics namespace.
    /// </summary>
    public static class NumericsExtensions
    {
        /// <summary>
        /// Function to set a row on a 4x4 matrix.
        /// </summary>
        /// <param name="matrix">The matrix to update.</param>
        /// <param name="rowIndex">The index of the row to update.</param>
        /// <param name="rowValues">The values to assign to the row.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="rowIndex"/> is less than 0, or greater than 3.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRow(this ref Matrix4x4 matrix, int rowIndex, in Vector4 rowValues)
        {
            switch (rowIndex)
            {
                case 0:
                    matrix.M11 = rowValues.X;
                    matrix.M12 = rowValues.Y;
                    matrix.M13 = rowValues.Z;
                    matrix.M14 = rowValues.W;
                    break;
                case 1:
                    matrix.M21 = rowValues.X;
                    matrix.M22 = rowValues.Y;
                    matrix.M23 = rowValues.Z;
                    matrix.M24 = rowValues.W;
                    break;
                case 2:
                    matrix.M31 = rowValues.X;
                    matrix.M32 = rowValues.Y;
                    matrix.M33 = rowValues.Z;
                    matrix.M34 = rowValues.W;
                    break;
                case 3:
                    matrix.M41 = rowValues.X;
                    matrix.M42 = rowValues.Y;
                    matrix.M43 = rowValues.Z;
                    matrix.M44 = rowValues.W;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }
        }

        /// <summary>
        /// Function to get a row on a 4x4 matrix.
        /// </summary>
        /// <param name="matrix">The matrix to read from.</param>
        /// <param name="rowIndex">The index of the row to read.</param>
        /// <param name="rowValues">The values to read from the row.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="rowIndex"/> is less than 0, or greater than 3.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRow(this in Matrix4x4 matrix, int rowIndex, out Vector4 rowValues)
        {
            switch (rowIndex)
            {
                case 0:
                    rowValues = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
                    break;
                case 1:
                    rowValues = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
                    break;
                case 2:
                    rowValues = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
                    break;
                case 3:
                    rowValues = new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }
        }

        /// <summary>
        /// Function to set a column on a 4x4 matrix.
        /// </summary>
        /// <param name="matrix">The matrix to update.</param>
        /// <param name="columnIndex">The index of the column to update.</param>
        /// <param name="columnValues">The values to assign to the column.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="columnIndex"/> is less than 0, or greater than 3.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColumn(this ref Matrix4x4 matrix, int columnIndex, in Vector4 columnValues)
        {
            switch (columnIndex)
            {
                case 0:
                    matrix.M11 = columnValues.X;
                    matrix.M21 = columnValues.Y;
                    matrix.M31 = columnValues.Z;
                    matrix.M41 = columnValues.W;
                    break;
                case 1:
                    matrix.M12 = columnValues.X;
                    matrix.M22 = columnValues.Y;
                    matrix.M32 = columnValues.Z;
                    matrix.M42 = columnValues.W;
                    break;
                case 2:
                    matrix.M13 = columnValues.X;
                    matrix.M23 = columnValues.Y;
                    matrix.M33 = columnValues.Z;
                    matrix.M43 = columnValues.W;
                    break;
                case 3:
                    matrix.M14 = columnValues.X;
                    matrix.M24 = columnValues.Y;
                    matrix.M34 = columnValues.Z;
                    matrix.M44 = columnValues.W;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }
        }

        /// <summary>
        /// Function to get a column on a 4x4 matrix.
        /// </summary>
        /// <param name="matrix">The matrix to read from.</param>
        /// <param name="columnIndex">The index of the column to read.</param>
        /// <param name="columnValues">The values to read from the column.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="columnIndex"/> is less than 0, or greater than 3.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetColumn(this in Matrix4x4 matrix, int columnIndex, out Vector4 columnValues)
        {
            switch (columnIndex)
            {
                case 0:
                    columnValues = new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41);
                    break;
                case 1:
                    columnValues = new Vector4(matrix.M21, matrix.M22, matrix.M32, matrix.M42);
                    break;
                case 2:
                    columnValues = new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43);
                    break;
                case 3:
                    columnValues = new Vector4(matrix.M14, matrix.M24, matrix.M34, matrix.M44);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }
        }

        /// <summary>
        /// Function to perform a matrix multiplication via reference values.
        /// </summary>
        /// <param name="left">The left reference to multiply.</param>
        /// <param name="right">The right reference to multiply.</param>
        /// <param name="result">The product of the multiplication.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(this in Matrix4x4 left, in Matrix4x4 right, out Matrix4x4 result) => result = Matrix4x4.Multiply(left, right);

        /// <summary>
        /// Function to perform a matrix multiplication via reference values.
        /// </summary>
        /// <param name="left">The left reference to multiply.</param>
        /// <param name="right">A floating point value to multiply.</param>
        /// <param name="result">The product of the multiplication.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(this in Matrix4x4 left, float right, out Matrix4x4 result) => result = Matrix4x4.Multiply(left, right);

        /// <summary>
        /// Function to perform an addition between two matrix values.
        /// </summary>
        /// <param name="left">The left matrix to add.</param>
        /// <param name="right">The right matrix to add.</param>
        /// <param name="result">The sum of both matrix values.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this in Matrix4x4 left, in Matrix4x4 right, out Matrix4x4 result) => result = Matrix4x4.Add(left, right);

        /// <summary>
        /// Function to perform a subtraction between two matrix values.
        /// </summary>
        /// <param name="left">The left matrix to subtract.</param>
        /// <param name="right">The right matrix to subtract.</param>
        /// <param name="result">The difference between both matrix values.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(this in Matrix4x4 left, in Matrix4x4 right, out Matrix4x4 result) => result = Matrix4x4.Subtract(left, right);

        /// <summary>
        /// Function to transpose the matrix.
        /// </summary>
        /// <param name="matrix">The matrix to transpose.</param>
        /// <param name="result">The transposed matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transpose(this in Matrix4x4 matrix, out Matrix4x4 result) => result = Matrix4x4.Transpose(matrix);

        /// <summary>
        /// Function to decompose a matrix into its scale, rotation and translation properties.
        /// </summary>
        /// <param name="matrix">The matrix to decompse.</param>
        /// <param name="scale">The scale applied to the matrix.</param>
        /// <param name="rotation">The rotation applied to the matrix.</param>
        /// <param name="translation">The translation applied to the matrix.</param>
        /// <returns><b>true</b> if decomposed successfully, <b>false</b> if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Decompose(this in Matrix4x4 matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation) => Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);

        /// <summary>
        /// Function to invert a matrix.
        /// </summary>
        /// <param name="matrix">The matrix to invert.</param>
        /// <param name="result">The inverted matrix.</param>        
        /// <returns><b>true</b> if the inversion was successful, <b>false</b> if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Invert(this in Matrix4x4 matrix, out Matrix4x4 result) => Matrix4x4.Invert(matrix, out result);

        /// <summary>
        /// Function to perform a linear interpolation between 2 matrices.
        /// </summary>
        /// <param name="matrix1">The starting matrix to interpolate from.</param>
        /// <param name="matrix2">The ending matrix to interpolate to.</param>
        /// <param name="amount">The amount of interpolation, between 0 and 1.</param>
        /// <param name="result">The resulting interpolated matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(this in Matrix4x4 matrix1, in Matrix4x4 matrix2, float amount, out Matrix4x4 result) => result = Matrix4x4.Lerp(matrix1, matrix2, amount);

        /// <summary>
        /// Function to negate a matrix by multiplying its values by -1.
        /// </summary>
        /// <param name="matrix">The matrix to negate.</param>
        /// <param name="result">The negated matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Negate(this in Matrix4x4 matrix, out Matrix4x4 result) => result = Matrix4x4.Negate(matrix);

        /// <summary>
        /// Function to transform a matrix by a quaternion.
        /// </summary>
        /// <param name="matrix">The matrix to transform.</param>
        /// <param name="rotation">The quaternion used to transform the matrix.</param>
        /// <param name="result">The transformed matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(this in Matrix4x4 matrix, in Quaternion rotation, out Matrix4x4 result) => result = Matrix4x4.Transform(matrix, rotation);

        /// <summary>
        /// Function to transform a 2D vector by a matrix.
        /// </summary>
        /// <param name="vec2">The vector to transform.</param>
        /// <param name="matrix">The matrix used to transform the vector.</param>
        /// <param name="result">The result of the transformation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(this in Vector2 vec2, in Matrix4x4 matrix, out Vector2 result) => result = Vector2.Transform(vec2, matrix);

        /// <summary>
        /// Function to transform a 2D normal vector by a matrix.
        /// </summary>
        /// <param name="vec2">The vector to transform.</param>
        /// <param name="matrix">The matrix used to transform the vector.</param>
        /// <param name="result">The result of the transformation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TransformNormal(this in Vector2 vec2, in Matrix4x4 matrix, out Vector2 result) => result = Vector2.TransformNormal(vec2, matrix);

        /// <summary>
        /// Function to transform a 3D vector by a matrix.
        /// </summary>
        /// <param name="vec3">The vector to transform.</param>
        /// <param name="matrix">The matrix used to transform the vector.</param>
        /// <param name="result">The result of the transformation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(this in Vector3 vec3, in Matrix4x4 matrix, out Vector3 result) => result = Vector3.Transform(vec3, matrix);

        /// <summary>
        /// Function to transform a 3D normal vector by a matrix.
        /// </summary>
        /// <param name="vec3">The vector to transform.</param>
        /// <param name="matrix">The matrix used to transform the vector.</param>
        /// <param name="result">The result of the transformation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TransformNormal(this in Vector3 vec3, in Matrix4x4 matrix, out Vector3 result) => result = Vector3.TransformNormal(vec3, matrix);

        /// <summary>
        /// Function to transform a 4D vector by a matrix.
        /// </summary>
        /// <param name="vec4">The vector to transform.</param>
        /// <param name="matrix">The matrix used to transform the vector.</param>
        /// <param name="result">The result of the transformation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(this in Vector4 vec4, in Matrix4x4 matrix, out Vector4 result) => result = Vector4.Transform(vec4, matrix);

        /// <summary>
        /// Function to create a quaternion from a rotation matrix.
        /// </summary>
        /// <param name="matrix">The matrix to use when creating the quaternion.</param>
        /// <param name="result">The new quaternion.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateQuaternion(this in Matrix4x4 matrix, out Quaternion result) => result = Quaternion.CreateFromRotationMatrix(matrix);

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
        public static void Shadow(this in Plane plane, in Vector4 lightDirection, out Matrix4x4 result)
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
        public static void Reflect(this in Plane plane, out Matrix4x4 result)
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
        /// <param name="result">When the method completes, contains the vector in screen space.</param>
        public static void Project(in this Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, in Matrix4x4 worldViewProjection, out Vector3 result)
        {
            vector.Transform(in worldViewProjection, out Vector3 v);
            result = new Vector3(((1.0f + v.X) * 0.5f * width) + x, ((1.0f - v.Y) * 0.5f * height) + y, (v.Z * (maxZ - minZ)) + minZ);
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
        /// <param name="result">When the method completes, contains the vector in object space.</param>
        public static void Unproject(in this Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, in Matrix4x4 worldViewProjection, out Vector3 result)
        {
            Vector3 v = default;

            worldViewProjection.Invert(out Matrix4x4 matrix);

            v.X = (((vector.X - x) / width) * 2.0f) - 1.0f;
            v.Y = -((((vector.Y - y) / height) * 2.0f) - 1.0f);
            v.Z = (vector.Z - minZ) / (maxZ - minZ);

            v.Transform(in matrix, out result);
        }
    }
}
