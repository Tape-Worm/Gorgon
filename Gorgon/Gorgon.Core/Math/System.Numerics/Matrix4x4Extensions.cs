
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
// Created: February 12, 2021 12:16:12 PM
// 


using System.Runtime.CompilerServices;
using Gorgon.Math;

namespace System.Numerics;

/// <summary>
/// Extension methods for the 4x4 matrix type
/// </summary>
/// <remarks>
/// This contains code adapted from <a href="https://github.com/sharpdx/SharpDX">SharpDX</a>
/// </remarks>
public static class Matrix4x4Extensions
{
    /// <summary>
    /// Function to get the up <see cref="Vector3"/> of the matrix; that is M21, M22, and M23.
    /// </summary>
    /// <param name="matrix">The matrix to read.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetUpVector(this ref readonly Matrix4x4 matrix) => new(matrix.M21, matrix.M22, matrix.M23);

    /// <summary>
    /// Function to set the up <see cref="Vector3"/> of the matrix; that is M21, M22, and M23.
    /// </summary>
    /// <param name="matrix">The matrix to update.</param>
    /// <param name="upVector">The value to apply.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetUpVector(this ref Matrix4x4 matrix, Vector3 upVector)
    {
        matrix.M21 = upVector.X;
        matrix.M22 = upVector.Y;
        matrix.M23 = upVector.Z;
    }

    /// <summary>
    /// Function to get the right <see cref="Vector3"/> of the matrix; that is M11, M12, and M13.
    /// </summary>
    /// <param name="matrix">The matrix to read.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetRightVector(this ref readonly Matrix4x4 matrix) => new(matrix.M11, matrix.M12, matrix.M13);

    /// <summary>
    /// Function to set the right <see cref="Vector3"/> of the matrix; that is M11, M12, and M13.
    /// </summary>
    /// <param name="matrix">The matrix to update.</param>
    /// <param name="rightVector">The value to apply.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetRightVector(this ref Matrix4x4 matrix, Vector3 rightVector)
    {
        matrix.M11 = rightVector.X;
        matrix.M12 = rightVector.Y;
        matrix.M13 = rightVector.Z;
    }

    /// <summary>
    /// Function to get the forward <see cref="Vector3"/> of the matrix; that is -M31, -M32, and -M33.
    /// </summary>
    /// <param name="matrix">The matrix to read.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetForwardVector(this ref readonly Matrix4x4 matrix) => new(-matrix.M31, -matrix.M32, -matrix.M33);

    /// <summary>
    /// Function to set the forward <see cref="Vector3"/> of the matrix; that is -M31, -M32, and -M33.
    /// </summary>
    /// <param name="matrix">The matrix to update.</param>
    /// <param name="forwardVector">The value to apply.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetForwardVector(this ref Matrix4x4 matrix, Vector3 forwardVector)
    {
        matrix.M31 = forwardVector.X;
        matrix.M32 = forwardVector.Y;
        matrix.M33 = forwardVector.Z;
    }

    /// <summary>
    /// Function to set the translation vector on the matrix.
    /// </summary>
    /// <param name="matrix">The matrix to update.</param>
    /// <param name="value">The value to apply.</param>
    public static void SetTranslation(this ref Matrix4x4 matrix, Vector3 value)
    {
        matrix.M41 = value.X;
        matrix.M42 = value.Y;
        matrix.M43 = value.Z;
    }

    /// <summary>
    /// Function to get the translation vector from the matrix.
    /// </summary>
    /// <param name="matrix">The matrix to read.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetTranslation(this ref readonly Matrix4x4 matrix) => new(matrix.M41, matrix.M42, matrix.M43);

    /// <summary>
    /// Function to set the scale vector on the matrix.
    /// </summary>
    /// <param name="matrix">The matrix to update.</param>
    /// <param name="value">The value to apply.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetScale(this ref Matrix4x4 matrix, Vector3 value)
    {
        matrix.M41 = value.X;
        matrix.M42 = value.Y;
        matrix.M43 = value.Z;
    }

    /// <summary>
    /// Function to get the scale vector from the matrix.
    /// </summary>
    /// <param name="matrix">The matrix to read.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetScale(this ref readonly Matrix4x4 matrix) => new(matrix.M41, matrix.M42, matrix.M43);

    /// <summary>
    /// Function to retrieve a row for the matrix as a 4D vector.
    /// </summary>
    /// <param name="matrix">The matrix to read from.</param>
    /// <param name="rowIndex">The index of the row to retrieve (0 - 3).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="rowIndex"/> is less than 0, or greater than 3.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 GetRow(this ref readonly Matrix4x4 matrix, int rowIndex) => rowIndex switch
    {
        0 => new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14),
        1 => new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24),
        2 => new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34),
        3 => new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44),
        _ => throw new ArgumentOutOfRangeException(nameof(rowIndex)),
    };

    /// <summary>
    /// Function to assign a row to the matrix with a 4D vector.
    /// </summary>
    /// <param name="matrix">The matrix to update from.</param>
    /// <param name="rowIndex">The index of the row to assign (0 - 3).</param>
    /// <param name="result">The matrix to assign to the row.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="rowIndex"/> is less than 0, or greater than 3.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetRow(this ref Matrix4x4 matrix, int rowIndex, Vector4 result)
    {
        switch (rowIndex)
        {
            case 0:
                matrix.M11 = result.X;
                matrix.M12 = result.Y;
                matrix.M13 = result.Z;
                matrix.M14 = result.W;
                return;
            case 1:
                matrix.M21 = result.X;
                matrix.M22 = result.Y;
                matrix.M23 = result.Z;
                matrix.M24 = result.W;
                return;
            case 2:
                matrix.M31 = result.X;
                matrix.M32 = result.Y;
                matrix.M33 = result.Z;
                matrix.M34 = result.W;
                return;
            case 3:
                matrix.M41 = result.X;
                matrix.M42 = result.Y;
                matrix.M43 = result.Z;
                matrix.M44 = result.W;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }
    }

    /// <summary>
    /// Function to retrieve a column for the matrix as a 4D vector.
    /// </summary>
    /// <param name="matrix">The matrix to read from.</param>
    /// <param name="columnIndex">The index of the column to retrieve (0 - 3).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thcolumnn when the <paramref name="columnIndex"/> is less than 0, or greater than 3.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 GetColumn(this ref readonly Matrix4x4 matrix, int columnIndex) => columnIndex switch
    {
        0 => new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41),
        1 => new Vector4(matrix.M12, matrix.M22, matrix.M32, matrix.M42),
        2 => new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43),
        3 => new Vector4(matrix.M14, matrix.M24, matrix.M34, matrix.M44),
        _ => throw new ArgumentOutOfRangeException(nameof(columnIndex)),
    };

    /// <summary>
    /// Function to assign a column to the matrix with a 4D vector.
    /// </summary>
    /// <param name="matrix">The matrix to update from.</param>
    /// <param name="columnIndex">The index of the column to assign (0 - 3).</param>
    /// <param name="result">The matrix to assign to the column.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thcolumnn when the <paramref name="columnIndex"/> is less than 0, or greater than 3.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetColumn(this ref Matrix4x4 matrix, int columnIndex, Vector4 result)
    {
        switch (columnIndex)
        {
            case 0:
                matrix.M11 = result.X;
                matrix.M21 = result.Y;
                matrix.M31 = result.Z;
                matrix.M41 = result.W;
                return;
            case 1:
                matrix.M12 = result.X;
                matrix.M22 = result.Y;
                matrix.M32 = result.Z;
                matrix.M42 = result.W;
                return;
            case 2:
                matrix.M13 = result.X;
                matrix.M23 = result.Y;
                matrix.M33 = result.Z;
                matrix.M43 = result.W;
                return;
            case 3:
                matrix.M14 = result.X;
                matrix.M24 = result.Y;
                matrix.M34 = result.Z;
                matrix.M44 = result.W;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
        }
    }

    /// <summary>
    /// Function to retrieve a matrix with absolute values.
    /// </summary>
    /// <param name="matrix">The matrix to evaluate.</param>
    /// <param name="absMatrix">The matrix with absolute values.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Abs(this ref readonly Matrix4x4 matrix, out Matrix4x4 absMatrix)
    {
        absMatrix = default;
        absMatrix.M11 = matrix.M11.Abs();
        absMatrix.M12 = matrix.M12.Abs();
        absMatrix.M13 = matrix.M13.Abs();
        absMatrix.M14 = matrix.M14.Abs();

        absMatrix.M21 = matrix.M21.Abs();
        absMatrix.M22 = matrix.M22.Abs();
        absMatrix.M23 = matrix.M23.Abs();
        absMatrix.M24 = matrix.M24.Abs();

        absMatrix.M31 = matrix.M31.Abs();
        absMatrix.M32 = matrix.M32.Abs();
        absMatrix.M33 = matrix.M33.Abs();
        absMatrix.M34 = matrix.M34.Abs();

        absMatrix.M41 = matrix.M41.Abs();
        absMatrix.M42 = matrix.M42.Abs();
        absMatrix.M43 = matrix.M43.Abs();
        absMatrix.M44 = matrix.M44.Abs();
    }
}
