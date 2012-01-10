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
// Created: Tuesday, January 10, 2012 3:32:31 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// Value type to represent a 3 dimensional vector.
	/// </summary>
	/// <remarks>
	/// Vector mathematics are commonly used in graphical 3D applications.  And other
	/// spatial related computations.
	/// This valuetype provides us a convienient way to use vectors and their operations.
	/// </remarks>
	[Serializable(), StructLayout(LayoutKind.Sequential, Pack = 4), TypeConverter(typeof(Design.Vector3DTypeConverter))]
	public struct Vector3D
		: IEquatable<Vector3D>
	{
		#region Variables.
		/// <summary>
		/// Empty vector.
		/// </summary>
		public readonly static Vector3D Zero = new Vector3D(0);
		/// <summary>
		/// Unit vector.
		/// </summary>
		public readonly static Vector3D Unit = new Vector3D(1.0f);
		/// <summary>
		/// Unit X vector.
		/// </summary>
		public readonly static Vector3D UnitX = new Vector3D(1.0f,0,0);
		/// <summary>
		/// Unit Y vector.
		/// </summary>
		public readonly static Vector3D UnitY = new Vector3D(0,1.0f,0);
		/// <summary>
		/// Unit Z vector.
		/// </summary>
		public readonly static Vector3D UnitZ = new Vector3D(0,0,1.0f);
		/// <summary>
		/// The size of the vector in bytes.
		/// </summary>
		public readonly static int Size = Marshal.SizeOf(typeof(Vector3D));
		/// <summary>
		/// Horizontal position of the vector.
		/// </summary>
		public float X;
		/// <summary>
		/// Vertical position of the vector.
		/// </summary>
		public float Y;
		/// <summary>
		/// Depth position of the vector.
		/// </summary>
		public float Z; 
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the length of this vector.
		/// </summary>
		public float Length
		{
			get
			{
				return GorgonMathUtility.Sqrt(X * X + Y * Y + Z * Z);
			}
		}

		/// <summary>
		/// Property to return the length of this vector squared.
		/// </summary>
		public float LengthSquare
		{
			get
			{
				return X * X + Y * Y + Z * Z;
			}
		}

		/// <summary>
		/// Property to set or return the components of the vector by an index.
		/// </summary>
		/// <remarks>The index must be between 0..2</remarks>
		/// <exception cref="System.IndexOutOfRangeException">The index was not between 0 and 2.</exception>
		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					default:
						throw new IndexOutOfRangeException();
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					case 2:
						Z = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <param name="result">The combined vector.</param>
		public static void Add(ref Vector3D left, ref Vector3D right, out Vector3D result)
		{
			result.X = left.X + right.X;
			result.Y = left.Y + right.Y;
			result.Z = left.Z + right.Z;
		}

		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>The combined vector.</returns>
		public static Vector3D Add(Vector3D left, Vector3D right)		
		{
			return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <param name="result">The difference between the two vectors.</param>
		public static void Subtract(ref Vector3D left, ref Vector3D right, out Vector3D result)
		{
			result.X = left.X - right.X;
			result.Y = left.Y - right.Y;
			result.Z = left.Z - right.Z;
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>The difference between the two vectors.</returns>
		public static Vector3D Subtract(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <param name="result">The negated vector.</param>
		public static void Negate(ref Vector3D vector, out Vector3D result)
		{
			result.X = -vector.X;
			result.Y = -vector.Y;
			result.Z = -vector.Z;
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <returns>The negated vector.</returns>
		public static Vector3D Negate(Vector3D vector)
		{
			return new Vector3D(-vector.X, -vector.Y, -vector.Z);
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static void Divide(ref Vector3D left, float scalar, out Vector3D result)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;

			result.X = left.X * inverse;
			result.Y = left.Y * inverse;
			result.Z = left.Z * inverse;
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static Vector3D Divide(Vector3D left, float scalar)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;
			return new Vector3D(left.X * inverse, left.Y * inverse, left.Z * inverse);
		}

		/// <summary>
		/// Function to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static void Divide(ref Vector3D left, ref Vector3D right, out Vector3D result)
		{
			result.X = left.X / right.X;
			result.Y = left.Y / right.Y;
			result.Z = left.Z / right.Z;
		}

		/// <summary>
		/// Function to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static Vector3D Divide(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		/// <summary>
		/// Function to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <param name="result">The product of the two vectors.</param>
		public static void Multiply(ref Vector3D left, ref Vector3D right, out Vector3D result)
		{
			result.X = left.X * right.X;
			result.Y = left.Y * right.Y;
			result.Z = left.Z * right.Z;
		}

		/// <summary>
		/// Function to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>The product of the two vectors.</returns>
		public static Vector3D Multiply(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="result">The product of the vector and the scalar value.</param>
		public static void Multiply(ref Vector3D left, float scalar, out Vector3D result)
		{
			result.X = left.X * scalar;
			result.Y = left.Y * scalar;
			result.Z = left.Z * scalar;
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>The product of the vector and the scalar value.</returns>
		public static Vector3D Multiply(Vector3D left, float scalar)
		{
			return new Vector3D(left.X * scalar, left.Y * scalar, left.Z * scalar);
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <param name="result">Rounded vector.</param>
		public static void Round(ref Vector3D vector, out Vector3D result)
		{
			result.X = GorgonMathUtility.RoundInt(vector.X);
			result.Y = GorgonMathUtility.RoundInt(vector.Y);
			result.Z = GorgonMathUtility.RoundInt(vector.Z);
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <returns>Rounded vector.</returns>
		public static Vector3D Round(Vector3D vector)
		{
			return new Vector3D(GorgonMathUtility.RoundInt(vector.X), GorgonMathUtility.RoundInt(vector.Y), GorgonMathUtility.RoundInt(vector.Z));
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">The dot product.</param>
		public static void DotProduct(ref Vector3D left, ref Vector3D right, out float result)
		{
			result = (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The dot product.</returns>
		public static float DotProduct(Vector3D left, Vector3D right)
		{
			return (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">Vector containing the cross product.</param>
		public static void CrossProduct(ref Vector3D left, ref Vector3D right, out Vector3D result)
		{
			result.X = left.Y * right.Z - left.Z * right.Y;
			result.Y = left.Z * right.X - left.X * right.Z;
			result.Z = left.X * right.Y - left.Y * right.X;
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>Vector containing the cross product.</returns>
		public static Vector3D CrossProduct(Vector3D left, Vector3D right)
		{
			return new Vector3D(left.Y * right.Z - left.Z * right.Y, 
								left.Z * right.X - left.X * right.Z,
								left.X * right.Y - left.Y * right.X);
		}

		/// <summary>
		/// Function to normalize the vector.
		/// </summary>
		public void Normalize()
		{
			float length = Length;

			if (GorgonMathUtility.EqualFloat(length, 0.0f, 0.000001f))
				return;

			float invLength = 1.0f / length;

			X *= invLength;
			Y *= invLength;
			Z *= invLength;
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the lowest values of the two vectors.</param>
		public static void Floor(ref Vector3D vector1, ref Vector3D vector2, out Vector3D result)
		{
			result.X = (vector1.X < vector2.X) ? vector1.X : vector2.X;
			result.Y = (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y;
			result.Z = (vector1.Z < vector2.Z) ? vector1.Z : vector2.Z;
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the lowest values of the two vectors.</returns>
		public static Vector3D Floor(Vector3D vector1, Vector3D vector2)
		{
			return new Vector3D((vector1.X < vector2.X) ? vector1.X : vector2.X, (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y, (vector1.Z < vector2.Z) ? vector1.Z : vector2.Z);
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the highest values of the two vectors.</param>
		public static void Ceiling(ref Vector3D vector1, ref Vector3D vector2, out Vector3D result)
		{
			result.X = (vector1.X > vector2.X) ? vector1.X : vector2.X;
			result.Y = (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y;
			result.Z = (vector1.Z > vector2.Z) ? vector1.Z : vector2.Z;
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the highest values of the two vectors.</returns>
		public static Vector3D Ceiling(Vector3D vector1, Vector3D vector2)
		{
			return new Vector3D((vector1.X > vector2.X) ? vector1.X : vector2.X, (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y, (vector1.Z > vector2.Z) ? vector1.Z : vector2.Z);
		}

		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			IEquatable<Vector3D> equate = obj as IEquatable<Vector3D>;

			if (equate != null)
				return equate.Equals(this);

			return false;
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref Vector3D vector1, ref Vector3D vector2)
		{
			return GorgonMathUtility.EqualFloat(vector1.X, vector2.X) && GorgonMathUtility.EqualFloat(vector1.Y, vector2.Y) && GorgonMathUtility.EqualFloat(vector1.Z, vector2.Z);
		}

		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>The hash code for this object.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() ^ Z.GetHashCode());
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format("3D Vector-> X:{0}, Y:{1}, Z:{2}",X,Y,Z);
		}
		
		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <param name="result">The resulting transformed vector.</param>
		public static void Transform(ref Matrix m, ref Vector3D vec, out Vector3D result)
		{
			float scale = 1.0f / (m.m41 * vec.X) + (m.m42 * vec.Y) + (m.m43 * vec.Z) + m.m44;

			result.X = ((m.m11 * vec.X) + (m.m12 * vec.Y) + (m.m13 * vec.Z) + m.m14) * scale;
			result.Y = ((m.m21 * vec.X) + (m.m22 * vec.Y) + (m.m23 * vec.Z) + m.m24) * scale;
			result.Z = ((m.m31 * vec.X) + (m.m32 * vec.Y) + (m.m33 * vec.Z) + m.m34) * scale;
		}

		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <returns>The resulting transformed vector.</returns>
		public static Vector3D Transform(Matrix m, Vector3D vec)
		{
			float scale = 1.0f / (m.m41 * vec.X) + (m.m42 * vec.Y) + (m.m43 * vec.Z) + m.m44;

			return new Vector3D(((m.m11 * vec.X) + (m.m12 * vec.Y) + (m.m13 * vec.Z) + m.m14) * scale,
								((m.m21 * vec.X) + (m.m22 * vec.Y) + (m.m23 * vec.Z) + m.m24) * scale,
								((m.m31 * vec.X) + (m.m32 * vec.Y) + (m.m33 * vec.Z) + m.m34) * scale);
		}

		/// <summary>
		/// Function to transform a 3D vector with a quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform.</param>
		/// <param name="vector">Vector to transform.</param>
		/// <param name="result">The transformed vector.</param>
		public static void Transform(ref Quaternion q, ref Vector3D vector, out Vector3D result)
		{
			float x = q.X + q.X;
			float y = q.Y + q.Y;
			float z = q.Z + q.Z;
			float wx = q.W * x;
			float wy = q.W * y;
			float wz = q.W * z;
			float xx = q.X * x;
			float xy = q.X * y;
			float xz = q.X * z;
			float yy = q.Y * y;
			float yz = q.Y * z;
			float zz = q.Z * z;

			result.X = ((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))) + (vector.Z * (xz + wy));
			result.Y = ((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz))) + (vector.Z * (yz - wx));
			result.Z = ((vector.X * (xz - wy)) + (vector.Y * (yz + wx))) + (vector.Z * ((1.0f - xx) - yy));
		}

		/// <summary>
		/// Function to transform a 3D vector with a quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform.</param>
		/// <param name="vector">Vector to transform.</param>
		/// <returns>The transformed vector.</returns>
		public static Vector3D Transform(Quaternion q, Vector3D vector)
		{
			float x = q.X + q.X;
			float y = q.Y + q.Y;
			float z = q.Z + q.Z;
			float wx = q.W * x;
			float wy = q.W * y;
			float wz = q.W * z;
			float xx = q.X * x;
			float xy = q.X * y;
			float xz = q.X * z;
			float yy = q.Y * y;
			float yz = q.Y * z;
			float zz = q.Z * z;

			return new Vector3D(((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))) + (vector.Z * (xz + wy)),
								((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz))) + (vector.Z * (yz - wx)),
								((vector.X * (xz - wy)) + (vector.Y * (yz + wx))) + (vector.Z * ((1.0f - xx) - yy)));
		}

		/// <summary>
		/// Returns a <see cref="Vector3D"/> containing the 3D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 3D triangle.
		/// </summary>
		/// <param name="vertex1Coordinate">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertex2Coordinate">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertex3Coordinate">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertex2Coordinate"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertex3Coordinate"/>).</param>
		/// <param name="result">When the method completes, contains the 3D Cartesian coordinates of the specified point.</param>
		public static void Barycentric(ref Vector3D vertex1Coordinate, ref Vector3D vertex2Coordinate, ref Vector3D vertex3Coordinate, float vertex2Weight, float vertex3Weight, out Vector3D result)
		{
			result.X = (vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X));
			result.Y = (vertex1Coordinate.Y + (vertex2Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y));
			result.Z = (vertex1Coordinate.Z + (vertex2Weight * (vertex2Coordinate.Z - vertex1Coordinate.Z))) + (vertex3Weight * (vertex3Coordinate.Z - vertex1Coordinate.Z));
		}

		/// <summary>
		/// Returns a <see cref="Vector3D"/> containing the 3D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 3D triangle.
		/// </summary>
		/// <param name="vertex1Coordinate">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertex2Coordinate">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertex3Coordinate">A <see cref="Vector3D"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertex2Coordinate"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertex3Coordinate"/>).</param>
		/// <returns>When the method completes, contains the 3D Cartesian coordinates of the specified point.</returns>
		public static Vector3D Barycentric(Vector3D vertex1Coordinate, Vector3D vertex2Coordinate, Vector3D vertex3Coordinate, float vertex2Weight, float vertex3Weight)
		{
			return new Vector3D((vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X)),
								(vertex1Coordinate.Y + (vertex2Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y)),
								(vertex1Coordinate.Z + (vertex2Weight * (vertex2Coordinate.Z - vertex1Coordinate.Z))) + (vertex3Weight * (vertex3Coordinate.Z - vertex1Coordinate.Z)));
		}

		/// <summary>
		/// Performs a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">When the method completes, contains the result of the Catmull-Rom interpolation.</param>
		public static void CatmullRom(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, ref Vector3D value4, float amount, out Vector3D result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			
			result.X = 0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) + 
				(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) + 
				((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed));

			result.Y = 0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + 
				(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) + 
				((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed));

			result.Z = 0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) + 
				(((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) + 
				((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed));
		}

		/// <summary>
		/// Performs a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>When the method completes, contains the result of the Catmull-Rom interpolation.</returns>
		public static Vector3D CatmullRom(Vector3D value1, Vector3D value2, Vector3D value3, Vector3D value4, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			return new Vector3D(0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) + 
								(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) + 
								((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
								0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + 
								(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) + 
								((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed)),
								0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) + 
								(((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) + 
								((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed)));
		}

		/// <summary>
		/// Performs a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">First source position vector.</param>
		/// <param name="tangent1">First source tangent vector.</param>
		/// <param name="value2">Second source position vector.</param>
		/// <param name="tangent2">Second source tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">When the method completes, contains the result of the Hermite spline interpolation.</param>
		public static void Hermite(ref Vector3D value1, ref Vector3D tangent1, ref Vector3D value2, ref Vector3D tangent2, float amount, out Vector3D result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			result.X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4);
			result.Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
			result.Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4);
		}

		/// <summary>
		/// Performs a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">First source position vector.</param>
		/// <param name="tangent1">First source tangent vector.</param>
		/// <param name="value2">Second source position vector.</param>
		/// <param name="tangent2">Second source tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>When the method completes, contains the result of the Hermite spline interpolation.</returns>
		public static Vector3D Hermite(Vector3D value1, Vector3D tangent1, Vector3D value2, Vector3D tangent2, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			return new Vector3D((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
								(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
								(((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4));
		}

		/// <summary>
		/// Performs a linear interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
		/// </remarks>
		public static void Lerp(ref Vector3D start, ref Vector3D end, float amount, out Vector3D result)
		{
			result.X = start.X + ((end.X - start.X) * amount);
			result.Y = start.Y + ((end.Y - start.Y) * amount);
			result.Z = start.Z + ((end.Z - start.Z) * amount);
		}

		/// <summary>
		/// Performs a linear interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>When the method completes, contains the linear interpolation of the two vectors.</returns>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
		/// </remarks>
		public static Vector3D Lerp(Vector3D start, Vector3D end, float amount)
		{
			return new Vector3D(start.X + ((end.X - start.X) * amount), start.Y + ((end.Y - start.Y) * amount), start.Z + ((end.Z - start.Z) * amount));
		}

		/// <summary>
		/// Performs a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">When the method completes, contains the cubic interpolation of the two vectors.</param>
		public static void SmoothStep(ref Vector3D start, ref Vector3D end, float amount, out Vector3D result)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));

			result.X = start.X + ((end.X - start.X) * amount);
			result.Y = start.Y + ((end.Y - start.Y) * amount);
			result.Z = start.Z + ((end.Z - start.Z) * amount);
		}

		/// <summary>
		/// Performs a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>When the method completes, contains the cubic interpolation of the two vectors.</returns>
		public static Vector3D SmoothStep(Vector3D start, Vector3D end, float amount)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));
			return new Vector3D(start.X + ((end.X - start.X) * amount), start.Y + ((end.Y - start.Y) * amount), start.Z + ((end.Z - start.Z) * amount));
		}

		/// <summary>
		/// Function to return the distance between two vectors.
		/// </summary>
		/// <param name="vector1">Starting vector.</param>
		/// <param name="vector2">Ending vector.</param>
		/// <returns>The distance between the vectors.</returns>
		public static float Distance(Vector3D vector1, Vector3D vector2)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			return GorgonMathUtility.Sqrt((x * x) + (y * y));
		}

		/// <summary>
		/// Function to return the squared distance between two vectors.
		/// </summary>
		/// <param name="vector1">Starting vector.</param>
		/// <param name="vector2">Ending vector.</param>
		/// <returns>The squared distance between the vectors.</returns>
		public static float DistanceSquared(Vector3D vector1, Vector3D vector2)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			return (x * x) + (y * y);
		}

		/// <summary>
		/// Function to return the distance between two vectors.
		/// </summary>
		/// <param name="vector1">Starting vector.</param>
		/// <param name="vector2">Ending vector.</param>
		/// <param name="result">The distance between the vectors.</param>
		public static void Distance(ref Vector3D vector1, ref Vector3D vector2, out float result)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			result = GorgonMathUtility.Sqrt((x * x) + (y * y));
		}

		/// <summary>
		/// Returns the reflection of a vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <param name="result">When the method completes, contains the reflected vector.</param>
		/// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
		/// whether the original vector was close enough to the surface to hit it.</remarks>
		public static void Reflect(ref Vector3D vector, ref Vector3D normal, out Vector3D result)
		{
			float dot = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);

			result.X = vector.X - ((2.0f * dot) * normal.X);
			result.Y = vector.Y - ((2.0f * dot) * normal.Y);
			result.Z = vector.Z - ((2.0f * dot) * normal.Z);
		}

		/// <summary>
		/// Returns the reflection of a vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <returns>When the method completes, contains the reflected vector.</returns>
		/// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
		/// whether the original vector was close enough to the surface to hit it.</remarks>
		public static Vector3D Reflect(Vector3D vector, Vector3D normal)
		{
			float dot = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);

			return new Vector3D(vector.X - ((2.0f * dot) * normal.X), vector.Y - ((2.0f * dot) * normal.Y), vector.Z - ((2.0f * dot) * normal.Z));
		}

		/// <summary>
		/// Projects a 3D vector from object space into screen space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <param name="world">The world matrix.</param>
		/// <param name="result">When the method completes, contains the vector in screen space.</param>
		public static void Project(ref Vector3D vector, ref RectangleF viewRectangle, ref OrpheusMinMaxF depthRange, ref Matrix projection, ref Matrix view, ref Matrix world, out Vector3D result)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);

			Vector4D.Transform(ref world, ref homoVector, out homoVector);
			Vector4D.Transform(ref view, ref homoVector, out homoVector);
			Vector4D.Transform(ref projection, ref homoVector, out homoVector);

			float invW = 1.0f / homoVector.W;
			float halfWidth = viewRectangle.Width * 0.5f;
			float halfHeight = viewRectangle.Width * 0.5f;

			result.X = ((homoVector.X * halfWidth * 0.5f) + (viewRectangle.X + halfWidth)) * invW;
			result.Y = ((homoVector.Y * -halfHeight) + (viewRectangle.Y + halfHeight)) * invW;
			result.Z = ((homoVector.Z * depthRange.Range) + depthRange.Minimum) * invW;
		}

		/// <summary>
		/// Projects a 3D vector from object space into screen space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <param name="result">When the method completes, contains the vector in screen space.</param>
		public static void Project(ref Vector3D vector, ref RectangleF viewRectangle, ref OrpheusMinMaxF depthRange, ref Matrix projection, ref Matrix view, out Vector3D result)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);

			Vector4D.Transform(ref view, ref homoVector, out homoVector);
			Vector4D.Transform(ref projection, ref homoVector, out homoVector);

			float invW = 1.0f / homoVector.W;

			result.X = ((homoVector.X * viewRectangle.Width * 0.5f) + viewRectangle.Y) * invW;
			result.Y = ((homoVector.Y * -viewRectangle.Height * 0.5f) + viewRectangle.Y) * invW;
			result.Z = ((homoVector.Z * depthRange.Range) + depthRange.Minimum) * invW;
		}

		/// <summary>
		/// Projects a 3D vector from object space into screen space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <param name="world">The world matrix.</param>
		/// <returns>When the method completes, contains the vector in screen space.</returns>
		/// <remarks>THIS IS NOT TESTED, BE SURE TO TEST THIS FUNCTION AT THE EARLIEST OPPORTUNITY.</remarks>
		public static Vector3D Project(Vector3D vector, RectangleF viewRectangle, OrpheusMinMaxF depthRange, Matrix projection, Matrix view, Matrix world)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);

			Vector4D.Transform(ref world, ref homoVector, out homoVector);
			Vector4D.Transform(ref view, ref homoVector, out homoVector);
			Vector4D.Transform(ref projection, ref homoVector, out homoVector);

			float invW = 1.0f / homoVector.W;

			return new Vector3D(((homoVector.X * viewRectangle.Width * 0.5f) + viewRectangle.Y) * invW,
								((homoVector.Y * -viewRectangle.Height * 0.5f) + viewRectangle.Y) * invW,
								((homoVector.Z * depthRange.Range) + depthRange.Minimum) * invW);
		}

		/// <summary>
		/// Projects a 3D vector from object space into screen space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <returns>When the method completes, contains the vector in screen space.</returns>
		/// <remarks>THIS IS NOT TESTED, BE SURE TO TEST THIS FUNCTION AT THE EARLIEST OPPORTUNITY.</remarks>
		public static Vector3D Project(Vector3D vector, RectangleF viewRectangle, OrpheusMinMaxF depthRange, Matrix projection, Matrix view)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);

			Vector4D.Transform(ref view, ref homoVector, out homoVector);
			Vector4D.Transform(ref projection, ref homoVector, out homoVector);

			float invW = 1.0f / homoVector.W;

			return new Vector3D(((homoVector.X * viewRectangle.Width * 0.5f) + viewRectangle.Y) * invW,
								((homoVector.Y * -viewRectangle.Height * 0.5f) + viewRectangle.Y) * invW,
								((homoVector.Z * depthRange.Range) + depthRange.Minimum) * invW);
		}
	
		/// <summary>
		/// Projects a 3D vector from screen space into object space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <param name="world">The world matrix.</param>
		/// <param name="result">When the method completes, contains the vector in object space.</param>
		/// <remarks>THIS IS NOT TESTED, BE SURE TO TEST THIS FUNCTION AT THE EARLIEST OPPORTUNITY.</remarks>
		public static void Unproject(ref Vector3D vector, ref RectangleF viewRectangle, ref OrpheusMinMaxF depthRange, ref Matrix projection, ref Matrix view, ref Matrix world, out Vector3D result)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);
			float scaleX, scaleY;
			Matrix invProjectViewWorld;

			// Get inverse matrix parts.
			Matrix.Multiply(ref world, ref view, out invProjectViewWorld);
			Matrix.Multiply(ref invProjectViewWorld, ref projection, out invProjectViewWorld);
			Matrix.Inverse(ref invProjectViewWorld, out invProjectViewWorld);

			scaleX = (1.0f / (viewRectangle.Width * 0.5f));
			scaleY = (1.0f / (-viewRectangle.Height * 0.5f));

			homoVector.X = (homoVector.X * scaleX) + (-viewRectangle.X * scaleX) - 1.0f;
			homoVector.Y = (homoVector.Y * scaleY) + (-viewRectangle.Y * scaleY) + 1.0f;
			homoVector.Z = (homoVector.Z * (1.0f / depthRange.Range)) - depthRange.Minimum;

			Vector4D.Transform(ref invProjectViewWorld, ref homoVector, out homoVector);

			result.X = homoVector.X;
			result.Y = homoVector.Y;
			result.Z = homoVector.Z;
		}

		/// <summary>
		/// Projects a 3D vector from screen space into object space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <param name="result">When the method completes, contains the vector in object space.</param>
		/// <remarks>THIS IS NOT TESTED, BE SURE TO TEST THIS FUNCTION AT THE EARLIEST OPPORTUNITY.</remarks>
		public static void Unproject(ref Vector3D vector, ref RectangleF viewRectangle, ref OrpheusMinMaxF depthRange, ref Matrix projection, ref Matrix view, out Vector3D result)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);
			float scaleX, scaleY;
			Matrix invProjectView;

			// Get inverse matrix parts.
			Matrix.Multiply(ref view, ref projection, out invProjectView);
			Matrix.Inverse(ref invProjectView, out invProjectView);

			scaleX = (1.0f / (viewRectangle.Width * 0.5f));
			scaleY = (1.0f / (-viewRectangle.Height * 0.5f));

			homoVector.X = (homoVector.X * scaleX) + (-viewRectangle.X * scaleX) - 1.0f;
			homoVector.Y = (homoVector.Y * scaleY) + (-viewRectangle.Y * scaleY) + 1.0f;
			homoVector.Z = (homoVector.Z * (1.0f / depthRange.Range)) - depthRange.Minimum;

			Vector4D.Transform(ref invProjectView, ref homoVector, out homoVector);

			result.X = homoVector.X;
			result.Y = homoVector.Y;
			result.Z = homoVector.Z;
		}

		/// <summary>
		/// Projects a 3D vector from screen space into object space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <param name="world">The world matrix.</param>
		/// <returns>When the method completes, contains the vector in object space.</returns>
		/// <remarks>THIS IS NOT TESTED, BE SURE TO TEST THIS FUNCTION AT THE EARLIEST OPPORTUNITY.</remarks>
		public static Vector3D Unproject(Vector3D vector, Rectangle viewRectangle, OrpheusMinMaxF depthRange, Matrix projection, Matrix view, Matrix world)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);
			float scaleX, scaleY;
			Matrix invProjectViewWorld;

			// Get inverse matrix parts.
			Matrix.Multiply(ref world, ref view, out invProjectViewWorld);
			Matrix.Multiply(ref invProjectViewWorld, ref projection, out invProjectViewWorld);
			Matrix.Inverse(ref invProjectViewWorld, out invProjectViewWorld);

			scaleX = (1.0f / (viewRectangle.Width * 0.5f));
			scaleY = (1.0f / (-viewRectangle.Height * 0.5f));

			homoVector.X = (homoVector.X * scaleX) + (-viewRectangle.X * scaleX) - 1.0f;
			homoVector.Y = (homoVector.Y * scaleY) + (-viewRectangle.Y * scaleY) + 1.0f;
			homoVector.Z = (homoVector.Z * (1.0f / depthRange.Range)) - depthRange.Minimum;

			Vector4D.Transform(ref invProjectViewWorld, ref homoVector, out homoVector);

			return new Vector3D(homoVector.X, homoVector.Y, homoVector.Z);
		}

		/// <summary>
		/// Projects a 3D vector from screen space into object space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="projection">The projection matrix.</param>
		/// <param name="view">The view matrix.</param>
		/// <returns>When the method completes, contains the vector in object space.</returns>
		/// <remarks>THIS IS NOT TESTED, BE SURE TO TEST THIS FUNCTION AT THE EARLIEST OPPORTUNITY.</remarks>
		public static Vector3D Unproject(Vector3D vector, Rectangle viewRectangle, OrpheusMinMaxF depthRange, Matrix projection, Matrix view)
		{
			Vector4D homoVector = new Vector4D(vector, 1.0f);
			float scaleX, scaleY;
			Matrix invProjectView;

			// Get inverse matrix parts.
			Matrix.Multiply(ref view, ref projection, out invProjectView);
			Matrix.Inverse(ref invProjectView, out invProjectView);

			scaleX = (1.0f / (viewRectangle.Width * 0.5f));
			scaleY = (1.0f / (-viewRectangle.Height * 0.5f));

			homoVector.X = (homoVector.X * scaleX) + (-viewRectangle.X * scaleX) - 1.0f;
			homoVector.Y = (homoVector.Y * scaleY) + (-viewRectangle.Y * scaleY) + 1.0f;
			homoVector.Z = (homoVector.Z * (1.0f / depthRange.Range)) - depthRange.Minimum;

			Vector4D.Transform(ref invProjectView, ref homoVector, out homoVector);

			return new Vector3D(homoVector.X, homoVector.Y, homoVector.Z);
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to perform equality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(Vector3D left,Vector3D right)
		{
			return (GorgonMathUtility.EqualFloat(left.X, right.X) && GorgonMathUtility.EqualFloat(left.Y, right.Y) && GorgonMathUtility.EqualFloat(left.Z, right.Z));
		}

		/// <summary>
		/// Operator to perform inequality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(Vector3D left,Vector3D right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static Vector3D operator +(Vector3D left,Vector3D right)
		{
			Vector3D result;
			Add(ref left, ref right, out result);
			return result;
		}
		
		/// <summary>
		/// Operator to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static Vector3D operator -(Vector3D left,Vector3D right)
		{
			Vector3D result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to negate a vector.
		/// </summary>
		/// <param name="left">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static Vector3D operator -(Vector3D left)
		{
			Vector3D result;
			Negate(ref left, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector3D operator *(Vector3D left,Vector3D right)
		{
			Vector3D result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector3D operator *(Vector3D left,float scalar)
		{
			Vector3D result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static Vector3D operator *(float scalar,Vector3D right)
		{
			Vector3D result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector3D operator /(Vector3D left,float scalar)
		{
			Vector3D result;
			Divide(ref left, scalar, out result);
			return result;
		}		

		/// <summary>
		/// Operator to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector3D operator /(Vector3D left,Vector3D right)
		{
			Vector3D result;
			Divide(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to convert a 2D vector into a 3D vector.
		/// </summary>
		/// <param name="vector">2D vector.</param>
		/// <returns>3D vector.</returns>
		public static implicit operator Vector3D(GorgonVector2 vector)
		{
			return new Vector3D(vector, 0.0f);
		}

		/// <summary>
		/// Operator to convert a 3D vector into a 2D vector.
		/// </summary>
		/// <param name="vector">3D vector to convert.</param>
		/// <returns>2D vector.</returns>
		public static explicit operator GorgonVector2(Vector3D vector)
		{
			return new GorgonVector2(vector.X, vector.Y);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Vector3D"/> struct.
		/// </summary>
		/// <param name="value">The value used to initialize the vector.</param>
		public Vector3D(float value)
		{
			Z = Y = X = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector3D"/> struct.
		/// </summary>
		/// <param name="x">Horizontal position of the vector.</param>
		/// <param name="y">Vertical posiition of the vector.</param>
		/// <param name="z">Depth position of the vector.</param>
		public Vector3D(float x,float y,float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector3D"/> struct.
		/// </summary>
		/// <param name="vector">The vector to copy.</param>
		/// <param name="z">Z coordinate for the vector.</param>
		public Vector3D(GorgonVector2 vector, float z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}
		#endregion

		#region IEquatable<Vector3D> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(Vector3D other)
		{
			return (GorgonMathUtility.EqualFloat(other.X, X) && GorgonMathUtility.EqualFloat(other.Y, Y) && GorgonMathUtility.EqualFloat(other.Z, Z));
		}
		#endregion
	}
}
