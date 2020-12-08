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
    public static class GorgonNumericsExtensions
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
    }
}
