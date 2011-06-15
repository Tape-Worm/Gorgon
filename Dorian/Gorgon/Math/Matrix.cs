#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 11:44:39 AM
// 
#endregion

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary
{
	/// <summary>
	/// A value type representing a 4x4 matrix.
	/// </summary>
	/// <remarks>
	/// A matrix is used to track linear transformations and their coefficients.
	/// They are used often in 3D graphics to facilitate the transformation of 
	/// polygonal data.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct Matrix
	{
		#region Variables.
		/// <summary>Matrix row 1 values.</summary>
		public float m11,m12,m13,m14;
		/// <summary>Matrix row 2 values.</summary>
		public float m21,m22,m23,m24;
		/// <summary>Matrix row 3 values.</summary>
		public float m31,m32,m33,m34;
		/// <summary>Matrix row 4 values.</summary>
		public float m41,m42,m43,m44;

		// An empty matrix.
		private readonly static Matrix _zero = new Matrix(0,0,0,0,
														0,0,0,0,
														0,0,0,0,
														0,0,0,0);
		// An identity matrix.
		private readonly static Matrix _identity = new Matrix(1.0f,0,0,0,
														0,1.0f,0,0,
														0,0,1.0f,0,
														0,0,0,1.0f);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a zero matrix.
		/// </summary>
		public static Matrix Zero
		{
			get
			{
				return _zero;
			}
		}

		/// <summary>
		/// Property to return an identity matrix.
		/// </summary>
		public static Matrix Identity
		{
			get
			{
				return _identity;
			}
		}

		/// <summary>
		/// Property to return the adjoint of this matrix.
		/// </summary>
		private Matrix Adjoint
		{
			get
			{
				float v0 = m22 * (m33 * m44 - m43 * m34) - m23 * (m32 * m44 - m42 * m34) + m24 * (m32 * m43 - m42 * m33);
				float v1 = -(m12 * (m33 * m44 - m43 * m34) - m13 * (m32 * m44 - m42 * m34) + m14 * (m32 * m43 - m42 * m33));
				float v2 = m12 * (m23 * m44 - m43 * m24) - m13 * (m22 * m44 - m42 * m24) + m14 * (m22 * m43 - m42 * m23);
				float v3 = -(m12 * (m23 * m34 - m33 * m24) - m13 * (m22 * m34 - m32 * m24) + m14 * (m22 * m33 - m32 * m23));
				float v4 = -(m21 * (m33 * m44 - m43 * m34) - m23 * (m31 * m44 - m41 * m34) + m24 * (m31 * m43 - m41 * m33));
				float v5 = m11 * (m33 * m44 - m43 * m34) - m13 * (m31 * m44 - m41 * m34) + m14 * (m31 * m43 - m41 * m33);
				float v6 = -(m11 * (m23 * m44 - m43 * m24) - m13 * (m21 * m44 - m41 * m24) + m14 * (m21 * m43 - m41 * m23));
				float v7 = m11 * (m23 * m34 - m33 * m24) - m13 * (m21 * m34 - m31 * m24) + m14 * (m21 * m33 - m31 * m23);
				float v8 = m21 * (m32 * m44 - m42 * m34) - m22 * (m31 * m44 - m41 * m34) + m24 * (m31 * m42 - m41 * m32);
				float v9 = -(m11 * (m32 * m44 - m42 * m34) - m12 * (m31 * m44 - m41 * m34) + m14 * (m31 * m42 - m41 * m32));
				float v10 = m11 * (m22 * m44 - m42 * m24) - m12 * (m21 * m44 - m41 * m24) + m14 * (m21 * m42 - m41 * m22);
				float v11 = -(m11 * (m22 * m34 - m32 * m24) - m12 * (m21 * m34 - m31 * m24) + m14 * (m21 * m32 - m31 * m22));
				float v12 = -(m21 * (m32 * m43 - m42 * m33) - m22 * (m31 * m43 - m41 * m33) + m23 * (m31 * m42 - m41 * m32));
				float v13 = m11 * (m32 * m43 - m42 * m33) - m12 * (m31 * m43 - m41 * m33) + m13 * (m31 * m42 - m41 * m32);
				float v14 = -(m11 * (m22 * m43 - m42 * m23) - m12 * (m21 * m43 - m41 * m23) + m13 * (m21 * m42 - m41 * m22));
				float v15 = m11 * (m22 * m33 - m32 * m23) - m12 * (m21 * m33 - m31 * m23) + m13 * (m21 * m32 - m31 * m22);

				return new Matrix(v0,v1,v2,v3,v4,v5,v6,v7,v8,v9,v10,v11,v12,v13,v14,v15);
			}
		}

		/// <summary>
		/// Property to return a value from the matrix by its row and column index.
		/// </summary>
		public float this[int row,int column]
		{
			get
			{
				// Catch out of bounds.
				if ((row < 1) || (row > 4))
					throw new ArgumentOutOfRangeException("row", "Matrix element [" + row.ToString() + "x" + column.ToString() + "] is invalid.");
				if ((column < 1) || (column > 4))
					throw new ArgumentOutOfRangeException("column", "Matrix element [" + row.ToString() + "x" + column.ToString() + "] is invalid.");

				unsafe
				{
					fixed(float *element = &m11)
						return *(element + ((4 * (row - 1)) + (column - 1)));
				}
			}
			set
			{
				// Catch out of bounds.
				if ((row < 1) || (row > 4))
					throw new ArgumentOutOfRangeException("row", "Matrix element [" + row.ToString() + "x" + column.ToString() + "] is invalid.");
				if ((column < 1) || (column > 4))
					throw new ArgumentOutOfRangeException("column", "Matrix element [" + row.ToString() + "x" + column.ToString() + "] is invalid.");

				unsafe
				{
					fixed(float *element = &m11)
						*(element + ((4 * (row - 1)) + (column - 1))) = value;
				}
			}
		}

		/// <summary>
		/// Property to return the transpose of this matrix.
		/// </summary>
		public Matrix Transpose
		{
			get
			{
				return new Matrix(m11,m21,m31,m41,m12,m22,m32,m42,m13,m23,m33,m43,m14,m24,m34,m44);
			}
		}

		/// <summary>
		/// Property to return the determinant of this matrix.
		/// </summary>
		public float Determinant
		{
			get
			{
				return m11 * (m22 * (m33 * m44 - m43 * m34) - m23 * (m32 * m44 - m42 * m34) + m24 * (m32 * m43 - m42 * m33)) - 
						m12 * (m21 * (m33 * m44 - m43 * m34) - m23 * (m31 * m44 - m41 * m34) + m24 * (m31 * m43 - m41 * m33)) + 
						m13 * (m21 * (m32 * m44 - m42 * m34) - m22 * (m31 * m44 - m41 * m34) + m24 * (m31 * m42 - m41 * m32)) - 
						m14 * (m21 * (m32 * m43 - m42 * m33) - m22 * (m31 * m43 - m41 * m33) + m23 * (m31 * m42 - m41 * m32));
			}
		}
		
		/// <summary>
		/// Property to return the inverse of this matrix.
		/// </summary>
		public Matrix Inverse
		{
			get
			{
				return Adjoint * (1.0f/Determinant);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set a column in the matrix.
		/// </summary>
		/// <param name="column">Column number to set.</param>
		/// <param name="values">Values to fill the column with.</param>
		private void SetColumn(int column,Vector3D values)
		{
			if ((column < 1) || (column > 4))
				throw new ArgumentOutOfRangeException("column", "Column [" + column.ToString() + "] is invalid.");
			this[1,column] = values.X;
			this[2,column] = values.Y;
			this[3,column] = values.Z;
		}

		/// <summary>
		/// Function to extract a column of values from the matrix.
		/// </summary>
		/// <param name="column">Column to extract.</param>
		/// <returns>A vector containing the values of the column from the matrix.</returns>
		public Vector3D Column(int column)
		{
			if ((column<1) || (column>4))
				throw new ArgumentOutOfRangeException("column", "Column [" + column.ToString() + "] is invalid.");

			return new Vector3D(this[1,column],this[2,column],this[3,column]);
		}

		/// <summary>
		/// Function to set the values for the matrix based on Vectors representing the axes.
		/// </summary>
		/// <param name="x">X axis.</param>
		/// <param name="y">Y axis.</param>
		/// <param name="z">Z axis.</param>
		public void FromAxes(Vector3D x,Vector3D y,Vector3D z)
		{
			SetColumn(1,x);
			SetColumn(2,y);
			SetColumn(3,z);
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
		public void Translate(Vector3D vec)
		{
			Translate(vec.X,vec.Y,vec.Z);
		}

		/// <summary>
		/// Function to scale the matrix by a vector.
		/// </summary>
		/// <param name="vec">Vector to scale this matrix with.</param>
		public void Scale(Vector3D vec)
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
			float radDegs = MathUtility.Radians(degrees);
			float sin = MathUtility.Sin(radDegs);
			float cos = MathUtility.Cos(radDegs);

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
			float radDegs = MathUtility.Radians(degrees);
			float sin = MathUtility.Sin(radDegs);
			float cos = MathUtility.Cos(radDegs);

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
			float radDegs = MathUtility.Radians(degrees);
			float sin = MathUtility.Sin(radDegs);
			float cos = MathUtility.Cos(radDegs);

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
			// This code is optimized to produce the required rotation matrix
			// using as few multiplies, additions, and assignments as possible.
			// Or so I've read...

			// Yaw cosine & sine values.
			float radYaw = MathUtility.Radians(yaw);
			float ycos = MathUtility.Cos(radYaw);
			float ysin = MathUtility.Sin(radYaw);
			// Pitch cosine & sine values.
			float radPitch = MathUtility.Radians(pitch);
			float pcos = MathUtility.Cos(radPitch);
			float psin = MathUtility.Sin(radPitch);
			// Roll cosine & sine values.
			float radRoll = MathUtility.Radians(roll);
			float rcos = MathUtility.Cos(radRoll);
			float rsin = MathUtility.Sin(radRoll);

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
		/// Function to transform and project a vector into world space.
		/// </summary>
		/// <param name="vec">Vector to transform.</param>
		/// <returns>Transformed vector.</returns>
		public Vector3D ProjectVector(Vector3D vec)
		{
			Vector3D res = new Vector3D();	// Resultant vector.
			float w;						// Divisor for the result.

			w = 1.0f / (m41 + m42 + m43 + m44);

			res.X = ((m11 * vec.X) + (m12 * vec.Y) + (m13 * vec.Z) + m14) * w;
			res.Y = ((m21 * vec.X) + (m22 * vec.Y) + (m23 * vec.Z) + m24) * w;
			res.Z = ((m31 * vec.X) + (m32 * vec.Y) + (m33 * vec.Z) + m34) * w;

			return res;
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
		/// Function to multiply two matrices together.
		/// </summary>
		/// <param name="left">Matrix to multiply.</param>
		/// <param name="right">Matrix to multiply with.</param>
		/// <returns>Product of two matrices.</returns>
		public static Matrix Multiply(Matrix left,Matrix right)
		{
			Matrix res = new Matrix();	// Resultant matrix.

			res.m11 = left.m11 * right.m11 + left.m12 * right.m21 + left.m13 * right.m31 + left.m14 * right.m41;
			res.m12 = left.m11 * right.m12 + left.m12 * right.m22 + left.m13 * right.m32 + left.m14 * right.m42;
			res.m13 = left.m11 * right.m13 + left.m12 * right.m23 + left.m13 * right.m33 + left.m14 * right.m43;
			res.m14 = left.m11 * right.m14 + left.m12 * right.m24 + left.m13 * right.m34 + left.m14 * right.m44;

			res.m21 = left.m21 * right.m11 + left.m22 * right.m21 + left.m23 * right.m31 + left.m24 * right.m41;
			res.m22 = left.m21 * right.m12 + left.m22 * right.m22 + left.m23 * right.m32 + left.m24 * right.m42;
			res.m23 = left.m21 * right.m13 + left.m22 * right.m23 + left.m23 * right.m33 + left.m24 * right.m43;
			res.m24 = left.m21 * right.m14 + left.m22 * right.m24 + left.m23 * right.m34 + left.m24 * right.m44;

			res.m31 = left.m31 * right.m11 + left.m32 * right.m21 + left.m33 * right.m31 + left.m34 * right.m41;
			res.m32 = left.m31 * right.m12 + left.m32 * right.m22 + left.m33 * right.m32 + left.m34 * right.m42;
			res.m33 = left.m31 * right.m13 + left.m32 * right.m23 + left.m33 * right.m33 + left.m34 * right.m43;
			res.m34 = left.m31 * right.m14 + left.m32 * right.m24 + left.m33 * right.m34 + left.m34 * right.m44;

			res.m41 = left.m41 * right.m11 + left.m42 * right.m21 + left.m43 * right.m31 + left.m44 * right.m41;
			res.m42 = left.m41 * right.m12 + left.m42 * right.m22 + left.m43 * right.m32 + left.m44 * right.m42;
			res.m43 = left.m41 * right.m13 + left.m42 * right.m23 + left.m43 * right.m33 + left.m44 * right.m43;
			res.m44 = left.m41 * right.m14 + left.m42 * right.m24 + left.m43 * right.m34 + left.m44 * right.m44;

			return res;
		}

		/// <summary>
		/// Function to multiply a matrix by a 3D vector.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="vec">Vector to multiply with.</param>
		/// <returns>A new vector containing the product.</returns>
		public static Vector3D Multiply(Matrix mat,Vector3D vec)
		{			
			Vector3D res = new Vector3D();	// Resultant vector.
			
			res.X = ((mat[1,1] * vec.X) + (mat[1,2] * vec.Y) + (mat[1,3] * vec.Z));
			res.Y = ((mat[2,1] * vec.X) + (mat[2,2] * vec.Y) + (mat[2,3] * vec.Z));
			res.Z = ((mat[3,1] * vec.X) + (mat[3,2] * vec.Y) + (mat[3,3] * vec.Z));

			return res;
		}

		/// <summary>
		/// Function to multiply a matrix by a 4D vector.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="vec">4D Vector to multiply with.</param>
		/// <returns>A new 4D vector containing the product.</returns>
		public static Vector4D Multiply(Matrix mat,Vector4D vec)
		{			
			Vector4D res = new Vector4D();	// Resultant vector.

			res.X = ((mat.m11 * vec.X) + (mat.m21 * vec.Y) + (mat.m31 * vec.Z) + (mat.m41 * vec.W));
			res.Y = ((mat.m12 * vec.X) + (mat.m22 * vec.Y) + (mat.m32 * vec.Z) + (mat.m42 * vec.W));
			res.Z = ((mat.m13 * vec.X) + (mat.m23 * vec.Y) + (mat.m33 * vec.Z) + (mat.m43 * vec.W));
			res.W = ((mat.m14 * vec.X) + (mat.m24 * vec.Y) + (mat.m34 * vec.Z) + (mat.m44 * vec.W));

			return res;
		}

		/// <summary>
		/// Function to multiply a matrix by a scalar value.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>Product of the multiplication</returns>
		public static Matrix Multiply(Matrix mat,float scalar)
		{
			Matrix res = new Matrix();		// Result.

			res.m11 = mat.m11 * scalar; 
			res.m12 = mat.m12 * scalar;
			res.m13 = mat.m13 * scalar;
			res.m14 = mat.m14 * scalar;
			
			res.m21 = mat.m21 * scalar; 
			res.m22 = mat.m22 * scalar;
			res.m23 = mat.m23 * scalar;
			res.m24 = mat.m24 * scalar;

			res.m31 = mat.m31 * scalar; 
			res.m32 = mat.m32 * scalar;
			res.m33 = mat.m33 * scalar;
			res.m34 = mat.m34 * scalar;

			res.m41 = mat.m41 * scalar; 
			res.m42 = mat.m42 * scalar;
			res.m43 = mat.m43 * scalar;
			res.m44 = mat.m44 * scalar;

			return res;
		}

		/// <summary>
		/// Function to add two matrices together.
		/// </summary>
		/// <param name="left">Matrix to add with.</param>
		/// <param name="right">Matrix to add.</param>
		/// <returns>Combined matrices.</returns>
		public static Matrix Add(Matrix left,Matrix right)
		{
			Matrix res = new Matrix();		// Resultant matrix.

			res.m11 = left.m11 + right.m11;
			res.m12 = left.m12 + right.m12;
			res.m13 = left.m13 + right.m13;
			res.m14 = left.m14 + right.m14;

			res.m21 = left.m21 + right.m21;
			res.m22 = left.m22 + right.m22;
			res.m23 = left.m23 + right.m23;
			res.m24 = left.m24 + right.m24;

			res.m31 = left.m31 + right.m31;
			res.m32 = left.m32 + right.m32;
			res.m33 = left.m33 + right.m33;
			res.m34 = left.m34 + right.m34;

			res.m41 = left.m41 + right.m41;
			res.m42 = left.m42 + right.m42;
			res.m43 = left.m43 + right.m43;
			res.m44 = left.m44 + right.m44;

			return res;
		}

		/// <summary>
		/// Function to subtract two matrices.
		/// </summary>
		/// <param name="left">Matrix to subtract from.</param>
		/// <param name="right">Matrix to subtract.</param>
		/// <returns>Matrix containing the difference.</returns>
		public static Matrix Subtract(Matrix left,Matrix right)
		{
			Matrix res = new Matrix();		// Resultant matrix.

			res.m11 = left.m11 - right.m11;
			res.m12 = left.m12 - right.m12;
			res.m13 = left.m13 - right.m13;
			res.m14 = left.m14 - right.m14;

			res.m21 = left.m21 - right.m21;
			res.m22 = left.m22 - right.m22;
			res.m23 = left.m23 - right.m23;
			res.m24 = left.m24 - right.m24;

			res.m31 = left.m31 - right.m31;
			res.m32 = left.m32 - right.m32;
			res.m33 = left.m33 - right.m33;
			res.m34 = left.m34 - right.m34;

			res.m41 = left.m41 - right.m41;
			res.m42 = left.m42 - right.m42;
			res.m43 = left.m43 - right.m43;
			res.m44 = left.m44 - right.m44;

			return res;
		}

		/// <summary>
		/// Function to negate a matrix.
		/// </summary>
		/// <param name="matrix">Matrix to negate.</param>
		/// <returns>Negative matrix.</returns>
		public static Matrix Negate(Matrix matrix)
		{
			Matrix res = new Matrix();		// Resultant.

			res.m11 = -matrix.m11;res.m12 = -matrix.m12;res.m13 = -matrix.m13;res.m14 = -matrix.m14;
			res.m21 = -matrix.m21;res.m22 = -matrix.m22;res.m23 = -matrix.m23;res.m24 = -matrix.m24;
			res.m31 = -matrix.m31;res.m32 = -matrix.m32;res.m33 = -matrix.m33;res.m34 = -matrix.m34;
			res.m41 = -matrix.m41;res.m42 = -matrix.m42;res.m43 = -matrix.m43;res.m44 = -matrix.m44;

			return res;
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to multiply two matrices together.
		/// </summary>
		/// <param name="left">Matrix to multiply.</param>
		/// <param name="right">Matrix to multiply with.</param>
		/// <returns>Product of two matrices.</returns>
		public static Matrix operator *(Matrix left,Matrix right)
		{
			return Multiply(left,right);
		}

        /// <summary>
        /// Operator to multiply a matrix by a 4D vector.
        /// </summary>
        /// <param name="mat">Matrix to multiply.</param>
        /// <param name="vec">Vector to multiply.</param>
        /// <returns>A new vector containing the product.</returns>
        public static Vector4D operator *(Matrix mat, Vector4D vec)
        {
            return Multiply(mat, vec);
        }

		/// <summary>
		/// Operator to multiply a matrix by a vector.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="vec">Vector to multiply with.</param>
		/// <returns>A new vector containing the product.</returns>
		public static Vector3D operator *(Matrix mat,Vector3D vec)
		{
			return Multiply(mat,vec);
		}

		/// <summary>
		/// Operator to multiply a matrix by a scalar value.
		/// </summary>
		/// <param name="mat">Matrix to multiply.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>Product of the multiplication</returns>
		public static Matrix operator *(Matrix mat,float scalar)
		{
			return Multiply(mat,scalar);
		}

		/// <summary>
		/// Operator to add two matrices together.
		/// </summary>
		/// <param name="left">Matrix to add with.</param>
		/// <param name="right">Matrix to add.</param>
		/// <returns>Combined matrices.</returns>
		public static Matrix operator +(Matrix left,Matrix right)
		{
			return Add(left,right);
		}

		/// <summary>
		/// Operator to subtract two matrices.
		/// </summary>
		/// <param name="left">Matrix to subtract from.</param>
		/// <param name="right">Matrix to subtract.</param>
		/// <returns>Matrix containing the difference.</returns>
		public static Matrix operator -(Matrix left,Matrix right)
		{
			return Subtract(left,right);
		}

		/// <summary>
		/// Operator to negate a matrix.
		/// </summary>
		/// <param name="matrix">Matrix to negate.</param>
		/// <returns>Negative matrix.</returns>
		public static Matrix operator -(Matrix matrix)
		{
			return Negate(matrix);
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Constructor.
		/// </summary>
		public Matrix(params float[] matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException("matrix");
			else if (matrix.Length != 16)
				throw new GorgonException("\"matrix\" must have a length of 16");

			m11 = matrix[0]; m12 = matrix[1]; m13 = matrix[2]; m14 = matrix[3];
			m21 = matrix[4]; m22 = matrix[5]; m23 = matrix[6]; m24 = matrix[7];
			m31 = matrix[8]; m32 = matrix[9]; m33 = matrix[10]; m34 = matrix[11];
			m41 = matrix[12]; m42 = matrix[13]; m43 = matrix[14]; m44 = matrix[15];			
		}
		#endregion
	}
}
