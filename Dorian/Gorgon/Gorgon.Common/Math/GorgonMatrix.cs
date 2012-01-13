#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, January 10, 2012 3:43:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// A 4x4 matrix.
	/// </summary>
	/// <remarks>
	/// A matrix is used to track linear transformations and their coefficients.
	/// They are used often in 3D graphics to facilitate the transformation of 
	/// polygonal data.
	/// <para>A Gorgon matrix is column major, which is the type normally used with mathematics.  A column major matrix would have 
	/// <para>Each element is prefixed with a 'm', and uses two numbers to denote the column and row.  e.g. m32 would mean the element at row 3, column 3.</para>
	/// </remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonMatrix
	{
		#region Variables.
		/// <summary>
		/// Row 1, column 1 of the matrix.
		/// </summary>
		public float m11;
		/// <summary>
		/// Row 1, column 2 of the matrix.
		/// </summary>
		public float m12;
		/// <summary>
		/// Row 1, column 3 of the matrix.
		/// </summary>
		public float m13;
		/// <summary>
		/// Row 1, column 4 of the matrix.
		/// </summary>
		public float m14;

		/// <summary>
		/// Row 2, column 1 of the matrix.
		/// </summary>
		public float m21;
		/// <summary>
		/// Row 2, column 2 of the matrix.
		/// </summary>
		public float m22;
		/// <summary>
		/// Row 2, column 3 of the matrix.
		/// </summary>
		public float m23;
		/// <summary>
		/// Row 2, column 4 of the matrix.
		/// </summary>
		public float m24;

		/// <summary>
		/// Row 3, column 1 of the matrix.
		/// </summary>
		public float m31;
		/// <summary>
		/// Row 3, column 2 of the matrix.
		/// </summary>
		public float m32;
		/// <summary>
		/// Row 3, column 3 of the matrix.
		/// </summary>
		public float m33;
		/// <summary>
		/// Row 3, column 4 of the matrix.
		/// </summary>
		public float m34;
		/// <summary>
		/// Row 4, column 1 of the matrix.
		/// </summary>
		public float m41;
		/// <summary>
		/// Row 4, column 2 of the matrix.
		/// </summary>
		public float m42;
		/// <summary>
		/// Row 4, column 3 of the matrix.
		/// </summary>
		public float m43;
		/// <summary>
		/// Row 4, column 4 of the matrix.
		/// </summary>
		public float m44;

		/// <summary>
		/// An empty matrix.
		/// </summary>
		public readonly static GorgonMatrix Zero = new GorgonMatrix(0,0,0,0,
														0,0,0,0,
														0,0,0,0,
														0,0,0,0);
		/// <summary>
		/// An identity matrix.
		/// </summary>
		public readonly static GorgonMatrix Identity = new GorgonMatrix(1.0f,0,0,0,
															0,1.0f,0,0,
															0,0,1.0f,0,
															0,0,0,1.0f);
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the transpose of a matrix.
		/// </summary>
		/// <param name="m">Matrix to transpose.</param>
		/// <param name="result">The transposed matrix.</param>
		public static void Transpose(ref GorgonMatrix m, out GorgonMatrix result)
		{
			result.m11 = m.m11;
			result.m12 = m.m21;
			result.m13 = m.m31;
			result.m14 = m.m41;

			result.m21 = m.m12;
			result.m22 = m.m22;
			result.m23 = m.m32;
			result.m24 = m.m42;

			result.m31 = m.m13;
			result.m32 = m.m23;
			result.m33 = m.m33;
			result.m34 = m.m43;

			result.m41 = m.m14;
			result.m42 = m.m24;
			result.m43 = m.m34;
			result.m44 = m.m44;
		}

		/// <summary>
		/// Function to return the transpose of a matrix.
		/// </summary>
		/// <param name="m">Matrix to transpose.</param>
		public static void Transpose(ref GorgonMatrix m)
		{
			Transpose(ref m, out m);
		}

		/// <summary>
		/// Function to return the determinant of the given matrix.
		/// </summary>
		/// <param name="m">Matrix used to find the determinant.</param>
		/// <returns>The determinant of the matrix.</returns>
		public static float Determinant(ref GorgonMatrix m)
		{
			return m.m11 * (m.m22 * (m.m33 * m.m44 - m.m43 * m.m34) - m.m23 * (m.m32 * m.m44 - m.m42 * m.m34) + m.m24 * (m.m32 * m.m43 - m.m42 * m.m33)) -
						m.m12 * (m.m21 * (m.m33 * m.m44 - m.m43 * m.m34) - m.m23 * (m.m31 * m.m44 - m.m41 * m.m34) + m.m24 * (m.m31 * m.m43 - m.m41 * m.m33)) +
						m.m13 * (m.m21 * (m.m32 * m.m44 - m.m42 * m.m34) - m.m22 * (m.m31 * m.m44 - m.m41 * m.m34) + m.m24 * (m.m31 * m.m42 - m.m41 * m.m32)) -
						m.m14 * (m.m21 * (m.m32 * m.m43 - m.m42 * m.m33) - m.m22 * (m.m31 * m.m43 - m.m41 * m.m33) + m.m23 * (m.m31 * m.m42 - m.m41 * m.m32));
		}

		/// <summary>
		/// Function to return the inverse of a matrix.
		/// </summary>
		/// <param name="m">Matrix to invert.</param>
		/// <param name="result">The inverted matrix.</param>
		/// <exception cref="System.DivideByZeroException">The matrix has no inverse because its determinant is 0.</exception>
		public static void Inverse(ref GorgonMatrix m, out GorgonMatrix result)
		{
			float invDeterm = 1.0f / Determinant(ref m);

			// Calculate the adjoint of the matrix.
			float v0 = (m.m22 * (m.m33 * m.m44 - m.m43 * m.m34) - m.m23 * (m.m32 * m.m44 - m.m42 * m.m34) + m.m24 * (m.m32 * m.m43 - m.m42 * m.m33)) * invDeterm;
			float v1 = (-(m.m12 * (m.m33 * m.m44 - m.m43 * m.m34) - m.m13 * (m.m32 * m.m44 - m.m42 * m.m34) + m.m14 * (m.m32 * m.m43 - m.m42 * m.m33))) * invDeterm;
			float v2 = (m.m12 * (m.m23 * m.m44 - m.m43 * m.m24) - m.m13 * (m.m22 * m.m44 - m.m42 * m.m24) + m.m14 * (m.m22 * m.m43 - m.m42 * m.m23)) * invDeterm;
			float v3 = (-(m.m12 * (m.m23 * m.m34 - m.m33 * m.m24) - m.m13 * (m.m22 * m.m34 - m.m32 * m.m24) + m.m14 * (m.m22 * m.m33 - m.m32 * m.m23))) * invDeterm;
			float v4 = (-(m.m21 * (m.m33 * m.m44 - m.m43 * m.m34) - m.m23 * (m.m31 * m.m44 - m.m41 * m.m34) + m.m24 * (m.m31 * m.m43 - m.m41 * m.m33))) * invDeterm;
			float v5 = (m.m11 * (m.m33 * m.m44 - m.m43 * m.m34) - m.m13 * (m.m31 * m.m44 - m.m41 * m.m34) + m.m14 * (m.m31 * m.m43 - m.m41 * m.m33)) * invDeterm;
			float v6 = (-(m.m11 * (m.m23 * m.m44 - m.m43 * m.m24) - m.m13 * (m.m21 * m.m44 - m.m41 * m.m24) + m.m14 * (m.m21 * m.m43 - m.m41 * m.m23))) * invDeterm;
			float v7 = (m.m11 * (m.m23 * m.m34 - m.m33 * m.m24) - m.m13 * (m.m21 * m.m34 - m.m31 * m.m24) + m.m14 * (m.m21 * m.m33 - m.m31 * m.m23)) * invDeterm;
			float v8 = (m.m21 * (m.m32 * m.m44 - m.m42 * m.m34) - m.m22 * (m.m31 * m.m44 - m.m41 * m.m34) + m.m24 * (m.m31 * m.m42 - m.m41 * m.m32)) * invDeterm;
			float v9 = (-(m.m11 * (m.m32 * m.m44 - m.m42 * m.m34) - m.m12 * (m.m31 * m.m44 - m.m41 * m.m34) + m.m14 * (m.m31 * m.m42 - m.m41 * m.m32))) * invDeterm;
			float v10 = (m.m11 * (m.m22 * m.m44 - m.m42 * m.m24) - m.m12 * (m.m21 * m.m44 - m.m41 * m.m24) + m.m14 * (m.m21 * m.m42 - m.m41 * m.m22)) * invDeterm;
			float v11 = (-(m.m11 * (m.m22 * m.m34 - m.m32 * m.m24) - m.m12 * (m.m21 * m.m34 - m.m31 * m.m24) + m.m14 * (m.m21 * m.m32 - m.m31 * m.m22))) * invDeterm;
			float v12 = (-(m.m21 * (m.m32 * m.m43 - m.m42 * m.m33) - m.m22 * (m.m31 * m.m43 - m.m41 * m.m33) + m.m23 * (m.m31 * m.m42 - m.m41 * m.m32))) * invDeterm;
			float v13 = (m.m11 * (m.m32 * m.m43 - m.m42 * m.m33) - m.m12 * (m.m31 * m.m43 - m.m41 * m.m33) + m.m13 * (m.m31 * m.m42 - m.m41 * m.m32)) * invDeterm;
			float v14 = (-(m.m11 * (m.m22 * m.m43 - m.m42 * m.m23) - m.m12 * (m.m21 * m.m43 - m.m41 * m.m23) + m.m13 * (m.m21 * m.m42 - m.m41 * m.m22))) * invDeterm;
			float v15 = (m.m11 * (m.m22 * m.m33 - m.m32 * m.m23) - m.m12 * (m.m21 * m.m33 - m.m31 * m.m23) + m.m13 * (m.m21 * m.m32 - m.m31 * m.m22)) * invDeterm;

			result.m11 = v0;
			result.m12 = v1;
			result.m13 = v2;
			result.m14 = v3;

			result.m21 = v4;
			result.m22 = v5;
			result.m23 = v6;
			result.m24 = v7;

			result.m31 = v8;
			result.m32 = v9;
			result.m33 = v10;
			result.m34 = v11;

			result.m41 = v12;
			result.m42 = v13;
			result.m43 = v14;
			result.m44 = v15;
		}


		/// <summary>
		/// Function to return the inverse of a matrix.
		/// </summary>
		/// <param name="m">Matrix to invert.</param>
		public static void Inverse(ref GorgonMatrix m)
		{
			Inverse(ref m, out m);
		}

		/// <summary>
		/// Function to set the values for the matrix based on Vectors representing the axes.
		/// </summary>
		/// <param name="x">X axis.</param>
		/// <param name="y">Y axis.</param>
		/// <param name="z">Z axis.</param>
		/// <param name="result">The resulting matrix built from the 3 vectors.</param>
		public static void FromAxes(ref GorgonVector3 x, ref GorgonVector3 y,ref GorgonVector3 z, out GorgonMatrix result)
		{
			result.m11 = x.X;
			result.m21 = x.Y;
			result.m31 = x.Z;
			result.m41 = 0.0f;

			result.m12 = y.X;
			result.m22 = y.Y;
			result.m32 = y.Z;
			result.m42 = 0.0f;

			result.m13 = z.X;
			result.m23 = z.Y;
			result.m33 = z.Z;
			result.m43 = 0.0f;

			result.m14 = 0.0f;
			result.m24 = 0.0f;
			result.m34 = 0.0f;
			result.m44 = 1.0f;
		}

		/// <summary>
		/// Function to perform a translation on the matrix.
		/// </summary>
		/// <param name="x">Amount to translate on the x axis.</param>
		/// <param name="y">Amount to translate on the y axis..</param>
		/// <param name="z">Amount to translate on the z axis.</param>
		public void Translate(float x,float y,float z)
		{
			m14 = x;
			m24 = y;
			m34 = z;
		}

		/// <summary>
		/// Function to scale the matrix.
		/// </summary>
		/// <param name="x">Amount to scale on the X axis.</param>
		/// <param name="y">Amount to scale on the Y axis.</param>
		/// <param name="z">Amount to scale on the Z axis.</param>
		public void Scale(float x,float y,float z)
		{
			m11 = x;
			m22 = y;
			m33 = z;
		}

		/// <summary>
		/// Function to perform a translation on the matrix with a vector..
		/// </summary>
		/// <param name="vec">Vector value to translate with</param>
		public void Translate(ref GorgonVector3 vec)
		{
			Translate(vec.X,vec.Y,vec.Z);
		}

		/// <summary>
		/// Function to scale the matrix by a vector.
		/// </summary>
		/// <param name="vec">Vector to scale this matrix with.</param>
		public void Scale(ref GorgonVector3 vec)
		{
			Scale(vec.X,vec.Y,vec.Z);
		}

		/// <summary>
		/// Function to rotate around the X axis.
		/// </summary>
		/// <param name="degrees">Amount to rotate in degrees.</param>
		public void RotateX(float degrees)
		{
			// Stored sin & cos values.
			float radDegs = GorgonMathUtility.Radians(degrees);
			float sin = GorgonMathUtility.Sin(radDegs);
			float cos = GorgonMathUtility.Cos(radDegs);

			m22 = cos;
			m23 = -sin;
			m32 = sin;
			m33 = cos;
		}

		/// <summary>
		/// Function to rotate around the Y axis.
		/// </summary>
		/// <param name="degrees">Amount to rotate in degrees.</param>
		public void RotateY(float degrees)
		{
			// Stored sin & cos values.
			float radDegs = GorgonMathUtility.Radians(degrees);
			float sin = GorgonMathUtility.Sin(radDegs);
			float cos = GorgonMathUtility.Cos(radDegs);

			m11 = cos;
			m13 = sin;
			m31 = -sin;
			m33 = cos;
		}

		/// <summary>
		/// Function to rotate around the Z axis.
		/// </summary>
		/// <param name="degrees">Amount to rotate in degrees.</param>
		public void RotateZ(float degrees)
		{
			// Stored sin & cos values.
			float radDegs = GorgonMathUtility.Radians(degrees);
			float sin = GorgonMathUtility.Sin(radDegs);
			float cos = GorgonMathUtility.Cos(radDegs);

			m11 = cos;
			m12 = -sin;
			m21 = sin;
			m22 = cos;
		}

		/// <summary>
		/// Function to create this matrix from euler angles.
		/// </summary>		
		/// <param name="yaw">Yaw (Y-axis) rotation in degrees.</param>
		/// <param name="pitch">Pitch (X-axis) rotation in degrees.</param>
		/// <param name="roll">Roll (Z-axis) rotation in degrees.</param>
		public void RotateEuler(float yaw, float pitch, float roll) 
		{
			// Yaw cosine & sine values.
			float radYaw = GorgonMathUtility.Radians(yaw);
			float ycos = GorgonMathUtility.Cos(radYaw);
			float ysin = GorgonMathUtility.Sin(radYaw);
			// Pitch cosine & sine values.
			float radPitch = GorgonMathUtility.Radians(pitch);
			float pcos = GorgonMathUtility.Cos(radPitch);
			float psin = GorgonMathUtility.Sin(radPitch);
			// Roll cosine & sine values.
			float radRoll = GorgonMathUtility.Radians(roll);
			float rcos = GorgonMathUtility.Cos(radRoll);
			float rsin = GorgonMathUtility.Sin(radRoll);

			// Common multiplication.
			float ySinpCos = ysin * pcos;
			float ySinpSin = ysin * psin;

			// Calculate rotation matrix.
			m11 = ycos * rcos;
			m12 = -ycos * rsin;
			m13 = ysin;
			m21 = ySinpSin * rcos + pcos * rsin;
			m22 = -ySinpSin * rsin + pcos * rcos;
			m23 = -psin * ycos;
			m31 = -ySinpCos * rcos + psin * rsin;
			m32 = ySinpCos * rsin + psin * rcos;
			m33 = pcos * ycos;
		}

		/// <summary>
		/// Function to output a string representation of the matrix.
		/// </summary>
		/// <returns>String containing matrix values.</returns>
		public override string ToString()
		{
			StringBuilder output = new StringBuilder();

			output.Append("4x4 Matrix:\n");

			output.AppendFormat("| {0,16:f6} | {1,16:f6} | {2,16:f6} | {3,16:f6} |\n",m11,m12,m13,m14);
			output.AppendFormat("| {0,16:f6} | {1,16:f6} | {2,16:f6} | {3,16:f6} |\n",m21,m22,m23,m24);
			output.AppendFormat("| {0,16:f6} | {1,16:f6} | {2,16:f6} | {3,16:f6} |\n",m31,m32,m33,m34);
			output.AppendFormat("| {0,16:f6} | {1,16:f6} | {2,16:f6} | {3,16:f6} |\n",m41,m42,m43,m44);

			return output.ToString();
		}

		/// <summary>
		/// Function to convert the matrix into an array of floating point values.
		/// </summary>
		/// <returns>An array of floating point values.</returns>
		public float[] ToArray()
		{
			return new[] {
				m11, m21, m31, m41,
				m12, m22, m32, m42,
				m13, m23, m33, m43,
				m14, m24, m34, m44
			};
		}

		/// <summary>
		/// Function to multiply two matrices together.
		/// </summary>
		/// <param name="left">Matrix to multiply.</param>
		/// <param name="right">Matrix to multiply with.</param>
		/// <param name="result">The product of the two matrices.</param>
		public static void Multiply(ref GorgonMatrix left,ref GorgonMatrix right, out GorgonMatrix result)
		{
			result.m11 = left.m11 * right.m11 + left.m12 * right.m21 + left.m13 * right.m31 + left.m14 * right.m41;
			result.m12 = left.m11 * right.m12 + left.m12 * right.m22 + left.m13 * right.m32 + left.m14 * right.m42;
			result.m13 = left.m11 * right.m13 + left.m12 * right.m23 + left.m13 * right.m33 + left.m14 * right.m43;
			result.m14 = left.m11 * right.m14 + left.m12 * right.m24 + left.m13 * right.m34 + left.m14 * right.m44;

			result.m21 = left.m21 * right.m11 + left.m22 * right.m21 + left.m23 * right.m31 + left.m24 * right.m41;
			result.m22 = left.m21 * right.m12 + left.m22 * right.m22 + left.m23 * right.m32 + left.m24 * right.m42;
			result.m23 = left.m21 * right.m13 + left.m22 * right.m23 + left.m23 * right.m33 + left.m24 * right.m43;
			result.m24 = left.m21 * right.m14 + left.m22 * right.m24 + left.m23 * right.m34 + left.m24 * right.m44;

			result.m31 = left.m31 * right.m11 + left.m32 * right.m21 + left.m33 * right.m31 + left.m34 * right.m41;
			result.m32 = left.m31 * right.m12 + left.m32 * right.m22 + left.m33 * right.m32 + left.m34 * right.m42;
			result.m33 = left.m31 * right.m13 + left.m32 * right.m23 + left.m33 * right.m33 + left.m34 * right.m43;
			result.m34 = left.m31 * right.m14 + left.m32 * right.m24 + left.m33 * right.m34 + left.m34 * right.m44;

			result.m41 = left.m41 * right.m11 + left.m42 * right.m21 + left.m43 * right.m31 + left.m44 * right.m41;
			result.m42 = left.m41 * right.m12 + left.m42 * right.m22 + left.m43 * right.m32 + left.m44 * right.m42;
			result.m43 = left.m41 * right.m13 + left.m42 * right.m23 + left.m43 * right.m33 + left.m44 * right.m43;
			result.m44 = left.m41 * right.m14 + left.m42 * right.m24 + left.m43 * right.m34 + left.m44 * right.m44;
		}

		/// <summary>
		/// Function to multiply a matrix by a 3D vector.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="vec">Vector to multiply with.</param>
		/// <param name="result">A new vector containing the product.</param>
		public static void Multiply(ref GorgonMatrix mat,ref GorgonVector3 vec, out GorgonVector3 result)
		{			
			result.X = ((mat.m11 * vec.X) + (mat.m12 * vec.Y) + (mat.m13 * vec.Z));
			result.Y = ((mat.m21 * vec.X) + (mat.m22 * vec.Y) + (mat.m23 * vec.Z));
			result.Z = ((mat.m31 * vec.X) + (mat.m32 * vec.Y) + (mat.m33 * vec.Z));
		}

		/// <summary>
		/// Function to multiply a matrix by a 4D vector.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="vec">4D Vector to multiply with.</param>
		/// <param name="result">A new 4D vector containing the product.</param>
		public static void Multiply(ref GorgonMatrix mat, ref GorgonVector4 vec, out GorgonVector4 result)
		{			
			result.X = ((mat.m11 * vec.X) + (mat.m21 * vec.Y) + (mat.m31 * vec.Z) + (mat.m41 * vec.W));
			result.Y = ((mat.m12 * vec.X) + (mat.m22 * vec.Y) + (mat.m32 * vec.Z) + (mat.m42 * vec.W));
			result.Z = ((mat.m13 * vec.X) + (mat.m23 * vec.Y) + (mat.m33 * vec.Z) + (mat.m43 * vec.W));
			result.W = ((mat.m14 * vec.X) + (mat.m24 * vec.Y) + (mat.m34 * vec.Z) + (mat.m44 * vec.W));
		}

		/// <summary>
		/// Function to multiply a matrix by a scalar value.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="result">Product of the multiplication.</param>
		public static void Multiply(ref GorgonMatrix mat, float scalar, out GorgonMatrix result)
		{
			result.m11 = mat.m11 * scalar; 
			result.m12 = mat.m12 * scalar;
			result.m13 = mat.m13 * scalar;
			result.m14 = mat.m14 * scalar;
			
			result.m21 = mat.m21 * scalar; 
			result.m22 = mat.m22 * scalar;
			result.m23 = mat.m23 * scalar;
			result.m24 = mat.m24 * scalar;

			result.m31 = mat.m31 * scalar; 
			result.m32 = mat.m32 * scalar;
			result.m33 = mat.m33 * scalar;
			result.m34 = mat.m34 * scalar;

			result.m41 = mat.m41 * scalar; 
			result.m42 = mat.m42 * scalar;
			result.m43 = mat.m43 * scalar;
			result.m44 = mat.m44 * scalar;
		}

		/// <summary>
		/// Function to add two matrices together.
		/// </summary>
		/// <param name="left">Matrix to add with.</param>
		/// <param name="right">Matrix to add.</param>
		/// <param name="result">The combined matrices.</param>
		public static void Add(ref GorgonMatrix left, ref GorgonMatrix right, out GorgonMatrix result)
		{
			result.m11 = left.m11 + right.m11;
			result.m12 = left.m12 + right.m12;
			result.m13 = left.m13 + right.m13;
			result.m14 = left.m14 + right.m14;

			result.m21 = left.m21 + right.m21;
			result.m22 = left.m22 + right.m22;
			result.m23 = left.m23 + right.m23;
			result.m24 = left.m24 + right.m24;

			result.m31 = left.m31 + right.m31;
			result.m32 = left.m32 + right.m32;
			result.m33 = left.m33 + right.m33;
			result.m34 = left.m34 + right.m34;

			result.m41 = left.m41 + right.m41;
			result.m42 = left.m42 + right.m42;
			result.m43 = left.m43 + right.m43;
			result.m44 = left.m44 + right.m44;
		}

		/// <summary>
		/// Function to subtract two matrices.
		/// </summary>
		/// <param name="left">Matrix to subtract from.</param>
		/// <param name="right">Matrix to subtract.</param>
		/// <param name="result">Matrix containing the difference between the left and right matrices.</param>
		public static void Subtract(ref GorgonMatrix left, ref GorgonMatrix right, out GorgonMatrix result)
		{
			result.m11 = left.m11 - right.m11;
			result.m12 = left.m12 - right.m12;
			result.m13 = left.m13 - right.m13;
			result.m14 = left.m14 - right.m14;

			result.m21 = left.m21 - right.m21;
			result.m22 = left.m22 - right.m22;
			result.m23 = left.m23 - right.m23;
			result.m24 = left.m24 - right.m24;

			result.m31 = left.m31 - right.m31;
			result.m32 = left.m32 - right.m32;
			result.m33 = left.m33 - right.m33;
			result.m34 = left.m34 - right.m34;

			result.m41 = left.m41 - right.m41;
			result.m42 = left.m42 - right.m42;
			result.m43 = left.m43 - right.m43;
			result.m44 = left.m44 - right.m44;
		}

		/// <summary>
		/// Function to negate a matrix.
		/// </summary>
		/// <param name="matrix">Matrix to negate.</param>
		/// <param name="result">The negated matrix.</param>
		public static void Negate(ref GorgonMatrix matrix, out GorgonMatrix result)
		{
			result.m11 = -matrix.m11;result.m12 = -matrix.m12;result.m13 = -matrix.m13;result.m14 = -matrix.m14;
			result.m21 = -matrix.m21;result.m22 = -matrix.m22;result.m23 = -matrix.m23;result.m24 = -matrix.m24;
			result.m31 = -matrix.m31;result.m32 = -matrix.m32;result.m33 = -matrix.m33;result.m34 = -matrix.m34;
			result.m41 = -matrix.m41;result.m42 = -matrix.m42;result.m43 = -matrix.m43;result.m44 = -matrix.m44;
		}

		/// <summary>
		/// Function to negate a matrix.
		/// </summary>
		/// <param name="matrix">Matrix to negate.</param>
		public static void Negate(ref GorgonMatrix matrix)
		{
			Negate(ref matrix, out matrix);
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to multiply two matrices together.
		/// </summary>
		/// <param name="left">Matrix to multiply.</param>
		/// <param name="right">Matrix to multiply with.</param>
		/// <returns>Product of two matrices.</returns>
		public static GorgonMatrix operator *(GorgonMatrix left,GorgonMatrix right)
		{
			GorgonMatrix result;

			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a matrix by a 4D vector.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="vec">Vector to multiply.</param>
		/// <returns>A new vector containing the product.</returns>
		public static GorgonVector4 operator *(GorgonMatrix mat, GorgonVector4 vec)
		{
			GorgonVector4 result;

			Multiply(ref mat, ref vec, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a matrix by a vector.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="vec">Vector to multiply with.</param>
		/// <returns>A new vector containing the product.</returns>
		public static GorgonVector3 operator *(GorgonMatrix mat,GorgonVector3 vec)
		{
			GorgonVector3 result;

			Multiply(ref mat, ref vec, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a matrix by a scalar value.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>Product of the multiplication</returns>
		public static GorgonMatrix operator *(GorgonMatrix mat,float scalar)
		{
			GorgonMatrix result;

			Multiply(ref mat, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to add two matrices together.
		/// </summary>
		/// <param name="left">Matrix to add with.</param>
		/// <param name="right">Matrix to add.</param>
		/// <returns>Combined matrices.</returns>
		public static GorgonMatrix operator +(GorgonMatrix left,GorgonMatrix right)
		{
			GorgonMatrix result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to subtract two matrices.
		/// </summary>
		/// <param name="left">Matrix to subtract from.</param>
		/// <param name="right">Matrix to subtract.</param>
		/// <returns>Matrix containing the difference.</returns>
		public static GorgonMatrix operator -(GorgonMatrix left,GorgonMatrix right)
		{
			GorgonMatrix result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to negate a matrix.
		/// </summary>
		/// <param name="matrix">Matrix to negate.</param>
		/// <returns>Negative matrix.</returns>
		public static GorgonMatrix operator -(GorgonMatrix matrix)
		{
			GorgonMatrix result;
			Negate(ref matrix, out result);
			return result;
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMatrix"/> struct.
		/// </summary>
		/// <param name="M11">Column 1, row 1 value.</param>
		/// <param name="M21">Column 2, row 1 value.</param>
		/// <param name="M31">Column 3, row 1 value.</param>
		/// <param name="M41">Column 4, row 1 value.</param>
		/// <param name="M12">Column 1, row 2 value.</param>
		/// <param name="M22">Column 2, row 2 value.</param>
		/// <param name="M32">Column 3, row 2 value.</param>
		/// <param name="M42">Column 4, row 2 value.</param>
		/// <param name="M12">Column 1, row 3 value.</param>
		/// <param name="M22">Column 2, row 3 value.</param>
		/// <param name="M32">Column 3, row 3 value.</param>
		/// <param name="M42">Column 4, row 3 value.</param>
		/// <param name="M12">Column 1, row 4 value.</param>
		/// <param name="M22">Column 2, row 4 value.</param>
		/// <param name="M32">Column 3, row 4 value.</param>
		/// <param name="M42">Column 4, row 4 value.</param>
		public GorgonMatrix(float M11, float M21, float M31, float M41,
			float M12, float M22, float M32, float M42,
			float M13, float M23, float M33, float M43,
			float M14,float M24,float M34,float M44)
		{
			m11 = M11; 
			m12 = M12; 
			m13 = M13; 
			m14 = M14;
			m21 = M21; 
			m22 = M22; 
			m23 = M23; 
			m24 = M24;
			m31 = M31; 
			m32 = M32; 
			m33 = M33; 
			m34 = M34;
			m41 = M41; 
			m42 = M42; 
			m43 = M43; 
			m44 = M44;			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMatrix"/> struct.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <remarks>Only the first 16 elements in the list will be taken.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="values"/> parameter has less than 4 elements.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the values parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonMatrix(IEnumerable<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Count() <= 16)
				throw new ArgumentException("The number of values must be at least 16 for a Matrix.", "values");

			m11 = values.ElementAt(0);
			m12 = values.ElementAt(1);
			m13 = values.ElementAt(2);
			m14 = values.ElementAt(3);
			m21 = values.ElementAt(4);
			m22 = values.ElementAt(5);
			m23 = values.ElementAt(6);
			m24 = values.ElementAt(7);
			m31 = values.ElementAt(8);
			m32 = values.ElementAt(9);
			m33 = values.ElementAt(10);
			m34 = values.ElementAt(11);
			m41 = values.ElementAt(12);
			m42 = values.ElementAt(13);
			m43 = values.ElementAt(14);
			m44 = values.ElementAt(15);
		}
		#endregion
	}
}
