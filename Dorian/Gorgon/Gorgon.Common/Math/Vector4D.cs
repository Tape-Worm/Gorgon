#region MIT.
// 
// Orpheus.
// Copyright (C) 2009 Michael Winsor
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
// Created: Monday, March 02, 2009 9:51:53 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace OrpheusFramework
{
	/// <summary>
	/// Value type to represent a 4 dimensional vector.
	/// </summary>
	/// <remarks>
	/// Vector mathematics are commonly used in graphical 3D applications.  And other
	/// spatial related computations.
	/// This valuetype provides us a convienient way to use vectors and their operations.
	/// </remarks>
	[Serializable(), StructLayout(LayoutKind.Sequential, Pack = 4), TypeConverter(typeof(Design.Vector4DConverter))]
	public struct Vector4D
		: IEquatable<Vector4D>
	{
		#region Variables.
		/// <summary>
		/// Empty vector.
		/// </summary>
		public readonly static Vector4D Zero = new Vector4D(0);
		/// <summary>
		/// Unit vector.
		/// </summary>
		public readonly static Vector4D Unit = new Vector4D(1.0f);
		/// <summary>
		/// Unit X vector.
		/// </summary>
		public readonly static Vector4D UnitX = new Vector4D(1.0f,0,0,0.0f);
		/// <summary>
		/// Unit Y vector.
		/// </summary>
		public readonly static Vector4D UnitY = new Vector4D(0,1.0f,0,0.0f);
		/// <summary>
		/// Unit Z vector.
		/// </summary>
		public readonly static Vector4D UnitZ = new Vector4D(0,0,1.0f,0.0f);
		/// <summary>
		/// Unit W vector.
		/// </summary>
		public readonly static Vector4D UnitW = new Vector4D(0, 0, 0, 1.0f);
		/// <summary>
		/// The size of the vector in bytes.
		/// </summary>
		public readonly static int Size = Marshal.SizeOf(typeof(Vector4D));
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
		/// <summary>
		/// Homogeneous unit.
		/// </summary>
		public float W;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the length of this vector.
		/// </summary>
		public float Length
		{
			get
			{
				return MathUtility.Sqrt(X * X + Y * Y + Z * Z + W * W);
			}
		}

		/// <summary>
		/// Property to return the length of this vector squared.
		/// </summary>
		public float LengthSquare
		{
			get
			{
				return X * X + Y * Y + Z * Z + W * W;
			}
		}

		/// <summary>
		/// Property to set or return the components of the vector by an index.
		/// </summary>
		/// <remarks>The index must be between 0..3.</remarks>
		/// <exception cref="System.IndexOutOfRangeException">The index was not between 0 and 3.</exception>
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
					case 3:
						return W;
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
					case 3:
						W = value;
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
		/// <param name="result">The combined vectors.</param>
		public static void Add(ref Vector4D left, ref Vector4D right, out Vector4D result)
		{
			result.X = left.X + right.X;
			result.Y = left.Y + right.Y;
			result.Z = left.Z + right.Z;
			result.W = left.W + right.W;
		}

		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>The combined vectors.</returns>
		public static Vector4D Add(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <param name="result">The difference between the two vectors.</param>
		public static void Subtract(ref Vector4D left, ref Vector4D right, out Vector4D result)
		{
			result.X = left.X - right.X;
			result.Y = left.Y - right.Y;
			result.Z = left.Z - right.Z;
			result.W = left.W - right.W;
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>The difference between the two vectors.</returns>
		public static Vector4D Subtract(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <param name="result">The negated vector.</param>
		public static void Negate(ref Vector4D vector, out Vector4D result)
		{
			result.X = -vector.X;
			result.Y = -vector.Y;
			result.Z = -vector.Z;
			result.W = -vector.W;
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <returns>The negated vector.</returns>
		public static Vector4D Negate(Vector4D vector)
		{
			return new Vector4D(-vector.X, -vector.Y, -vector.Z, -vector.W);
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static void Divide(ref Vector4D left, float scalar, out Vector4D result)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;

			result.X = left.X * inverse;
			result.Y = left.Y * inverse;
			result.Z = left.Z * inverse;
			result.W = left.W * inverse;
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static Vector4D Divide(Vector4D left, float scalar)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;

			return new Vector4D(left.X * inverse, left.Y * inverse, left.Z * inverse, left.W * inverse);
		}

		/// <summary>
		/// Function to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static void Divide(ref Vector4D left, ref Vector4D right, out Vector4D result)
		{
			result.X = left.X / right.X;
			result.Y = left.Y / right.Y;
			result.Z = left.Z / right.Z;
			result.W = left.W / right.W;
		}

		/// <summary>
		/// Function to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static Vector4D Divide(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		/// <summary>
		/// Function to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <param name="result">The product of the vectors.</param>
		public static void Multiply(ref Vector4D left, ref Vector4D right, out Vector4D result)
		{
			result.X = left.X * right.X;
			result.Y = left.Y * right.Y;
			result.Z = left.Z * right.Z;
			result.W = left.W * right.W;
		}

		/// <summary>
		/// Function to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>The product of the vectors.</returns>
		public static Vector4D Multiply(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="result">The product of the vector and the scalar.</param>
		public static void Multiply(ref Vector4D left, float scalar, out Vector4D result)
		{
			result.X = left.X * scalar;
			result.Y = left.Y * scalar;
			result.Z = left.Z * scalar;
			result.W = left.W * scalar;
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>The product of the vector and the scalar.</returns>
		public static Vector4D Multiply(Vector4D left, float scalar)
		{
			return new Vector4D(left.X * scalar, left.Y * scalar, left.Z * scalar, left.W * scalar);
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <param name="result">Rounded vector.</param>
		public static void Round(ref Vector4D vector, out Vector4D result)
		{
			result.X = MathUtility.RoundInt(vector.X);
			result.Y = MathUtility.RoundInt(vector.Y);
			result.Z = MathUtility.RoundInt(vector.Z);
			result.W = MathUtility.RoundInt(vector.W);
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		public static Vector4D Round(Vector4D vector)
		{
			return new Vector4D(MathUtility.RoundInt(vector.X),MathUtility.RoundInt(vector.Y),MathUtility.RoundInt(vector.Z),MathUtility.RoundInt(vector.W));
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the lowest values of the two vectors.</param>
		public static void Floor(ref Vector4D vector1, ref Vector4D vector2, out Vector4D result)
		{
			result.X = (vector1.X < vector2.X) ? vector1.X : vector2.X;
			result.Y = (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y;
			result.Z = (vector1.Z < vector2.Z) ? vector1.Z : vector2.Z;
			result.W = (vector1.W < vector2.W) ? vector1.W : vector2.W;
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the lowest values of the two vectors.</returns>
		public static Vector4D Floor(Vector4D vector1, Vector4D vector2)
		{
			return new Vector4D((vector1.X < vector2.X) ? vector1.X : vector2.X, (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y,
								(vector1.Z < vector2.Z) ? vector1.Z : vector2.Z, (vector1.W < vector2.W) ? vector1.W : vector2.W);
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the highest values of the two vectors.</param>
		public static void Ceiling(ref Vector4D vector1, ref Vector4D vector2, out Vector4D result)
		{
			result.X = (vector1.X > vector2.X) ? vector1.X : vector2.X;
			result.Y = (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y;
			result.Z = (vector1.Z > vector2.Z) ? vector1.Z : vector2.Z;
			result.W = (vector1.W > vector2.W) ? vector1.W : vector2.W;
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the highest values of the two vectors.</returns>
		public static Vector4D Ceiling(Vector4D vector1, Vector4D vector2)
		{
			return new Vector4D((vector1.X > vector2.X) ? vector1.X : vector2.X, (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y,
								(vector1.Z > vector2.Z) ? vector1.Z : vector2.Z, (vector1.W > vector2.W) ? vector1.W : vector2.W);
		}
		
		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">The dot product.</param>
		public static void DotProduct(ref Vector4D left, ref Vector4D right, out float result)
		{
			result = left.X * right.X +
					left.Y * right.Y +
					left.Z * right.Z +
					left.W * right.W;
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The dot product.</returns>
		public static float DotProduct(Vector4D left, Vector4D right)
		{
			return (left.X * right.X +
					left.Y * right.Y +
					left.Z * right.Z +
					left.W * right.W);
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">The cross product of the two vectors.</param>
		public static void CrossProduct(ref Vector4D left, ref Vector4D right, out Vector4D result)
		{
			result.X = ((left.X * right.Y) - (left.Y * right.X)) + ((left.X * right.Z) - (left.Z * right.X)) + ((left.Y * right.Z) - (left.Z * right.Y));
			result.Y = ((left.Z * right.Y) - (left.Y * right.Z)) + ((left.Y * right.W) - (left.W * right.Y)) + ((left.Z * right.W) - (left.W * right.Z));
			result.Z = ((left.X * right.Z) - (left.Z * right.X)) + ((left.W * right.X) - (left.X * right.W)) + ((left.Z * right.W) - (left.W * right.Z));
			result.W = ((left.Y * right.X) - (left.X * right.Y)) + ((left.W * right.X) - (left.X * right.W)) + ((left.W * right.Y) - (left.Y * right.W));
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The cross product of the two vectors.</returns>
		public static Vector4D CrossProduct(Vector4D left, Vector4D right)
		{
			return new Vector4D(((left.X * right.Y) - (left.Y * right.X)) + ((left.X * right.Z) - (left.Z * right.X)) + ((left.Y * right.Z) - (left.Z * right.Y)),
								((left.Z * right.Y) - (left.Y * right.Z)) + ((left.Y * right.W) - (left.W * right.Y)) + ((left.Z * right.W) - (left.W * right.Z)),
								((left.X * right.Z) - (left.Z * right.X)) + ((left.W * right.X) - (left.X * right.W)) + ((left.Z * right.W) - (left.W * right.Z)),
								((left.Y * right.X) - (left.X * right.Y)) + ((left.W * right.X) - (left.X * right.W)) + ((left.W * right.Y) - (left.Y * right.W)));
		}

		/// <summary>
		/// Function to normalize the vector.
		/// </summary>
		public void Normalize()
		{
			float length = Length;

			if (MathUtility.EqualFloat(length, 0.0f, 0.000001f))
				return;

			float invLength = 1.0f / length;

			X *= invLength;
			Y *= invLength;
			Z *= invLength;
			W *= invLength;
		}
		
		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			IEquatable<Vector4D> equate = obj as IEquatable<Vector4D>;

			if (equate == null)
				return equate.Equals(this);

			return false;
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref Vector4D vector1, ref Vector4D vector2)
		{
			return MathUtility.EqualFloat(vector1.X, vector2.X) && MathUtility.EqualFloat(vector1.Y, vector2.Y) && 
				MathUtility.EqualFloat(vector1.Z, vector2.Z) && MathUtility.EqualFloat(vector1.W, vector2.W);
		}

		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>The hash code for this object.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() ^ (Z.GetHashCode() ^ W.GetHashCode()));
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format("4D Vector-> X:{0}, Y:{1}, Z:{2}, W:{3}",X,Y,Z,W);
		}

		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <param name="result">The resulting transformed vector.</param>
		public static void Transform(ref Matrix m, ref Vector4D vec, out Vector4D result)
		{
			result.X = ((m.m11 * vec.X) + (m.m12 * vec.Y) + (m.m13 * vec.Z) + m.m14) * vec.W;
			result.Y = ((m.m21 * vec.X) + (m.m22 * vec.Y) + (m.m23 * vec.Z) + m.m24) * vec.W;
			result.Z = ((m.m31 * vec.X) + (m.m32 * vec.Y) + (m.m33 * vec.Z) + m.m34) * vec.W;
			result.W = ((m.m41 * vec.X) + (m.m42 * vec.Y) + (m.m43 * vec.Z) + m.m44) * vec.W;
		}

		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <returns>The resulting transformed vector.</returns>
		public static Vector4D Transform(Matrix m, Vector4D vec)
		{
			return new Vector4D(((m.m11 * vec.X) + (m.m12 * vec.Y) + (m.m13 * vec.Z) + m.m14) * vec.W,
								((m.m21 * vec.X) + (m.m22 * vec.Y) + (m.m23 * vec.Z) + m.m24) * vec.W,
								((m.m31 * vec.X) + (m.m32 * vec.Y) + (m.m33 * vec.Z) + m.m34) * vec.W,
								((m.m41 * vec.X) + (m.m42 * vec.Y) + (m.m43 * vec.Z) + m.m44) * vec.W);
		}

		/// <summary>
		/// Function to transform a vector by a given quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <param name="result">The resulting transformed vector.</param>
		public static void Transform(ref Quaternion q, ref Vector4D vec, out Vector4D result)
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

			result.X = ((vec.X * ((1.0f - yy) - zz)) + (vec.Y * (xy - wz))) + (vec.Z * (xz + wy));
			result.Y = ((vec.X * (xy + wz)) + (vec.Y * ((1.0f - xx) - zz))) + (vec.Z * (yz - wx));
			result.Z = ((vec.X * (xz - wy)) + (vec.Y * (yz + wx))) + (vec.Z * ((1.0f - xx) - yy));
			result.W = vec.W;
		}

		/// <summary>
		/// Function to transform a vector by a given quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <returns>The resulting transformed vector.</returns>
		public static Vector4D Transform(Quaternion q, Vector4D vec)
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

			return new Vector4D(((vec.X * ((1.0f - yy) - zz)) + (vec.Y * (xy - wz))) + (vec.Z * (xz + wy)),
								((vec.X * (xy + wz)) + (vec.Y * ((1.0f - xx) - zz))) + (vec.Z * (yz - wx)),
								((vec.X * (xz - wy)) + (vec.Y * (yz + wx))) + (vec.Z * ((1.0f - xx) - yy)),
								vec.W);
		}

		/// <summary>
		/// Returns a <see cref="Vector4D"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 4D triangle.
		/// </summary>
		/// <param name="vertexCoordinate1">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertexCoordinate2">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertexCoordinate3">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertexCoordinate2"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertexCoordinate3"/>).</param>
		/// <param name="result">When the method completes, contains the 4D Cartesian coordinates of the specified point.</param>
		public static void Barycentric(ref Vector4D vertexCoordinate1, ref Vector4D vertexCoordinate2, ref Vector4D vertexCoordinate3, float vertex2Weight, float vertex3Weight, out Vector4D result)
		{
			result.X = (vertexCoordinate1.X + (vertex2Weight * (vertexCoordinate2.X - vertexCoordinate1.X))) + (vertex3Weight * (vertexCoordinate3.X - vertexCoordinate1.X));
			result.Y = (vertexCoordinate1.Y + (vertex2Weight * (vertexCoordinate2.Y - vertexCoordinate1.Y))) + (vertex3Weight * (vertexCoordinate3.Y - vertexCoordinate1.Y));
			result.Z = (vertexCoordinate1.Z + (vertex2Weight * (vertexCoordinate2.Z - vertexCoordinate1.Z))) + (vertex3Weight * (vertexCoordinate3.Z - vertexCoordinate1.Z));
			result.W = (vertexCoordinate1.W + (vertex2Weight * (vertexCoordinate2.W - vertexCoordinate1.W))) + (vertex3Weight * (vertexCoordinate3.W - vertexCoordinate1.W));
		}

		/// <summary>
		/// Returns a <see cref="Vector4D"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 4D triangle.
		/// </summary>
		/// <param name="vertexCoordinate1">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertexCoordinate2">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertexCoordinate3">A <see cref="Vector4D"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertexCoordinate2"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertexCoordinate3"/>).</param>
		/// <returns>When the method completes, contains the 4D Cartesian coordinates of the specified point.</returns>
		public Vector4D Barycentric(Vector4D vertexCoordinate1, Vector4D vertexCoordinate2, Vector4D vertexCoordinate3, float vertex2Weight, float vertex3Weight)
		{
			return new Vector4D((vertexCoordinate1.X + (vertex2Weight * (vertexCoordinate2.X - vertexCoordinate1.X))) + (vertex3Weight * (vertexCoordinate3.X - vertexCoordinate1.X)),
								(vertexCoordinate1.Y + (vertex2Weight * (vertexCoordinate2.Y - vertexCoordinate1.Y))) + (vertex3Weight * (vertexCoordinate3.Y - vertexCoordinate1.Y)),
								(vertexCoordinate1.Z + (vertex2Weight * (vertexCoordinate2.Z - vertexCoordinate1.Z))) + (vertex3Weight * (vertexCoordinate3.Z - vertexCoordinate1.Z)),
								(vertexCoordinate1.W + (vertex2Weight * (vertexCoordinate2.W - vertexCoordinate1.W))) + (vertex3Weight * (vertexCoordinate3.W - vertexCoordinate1.W)));
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
		public static void CatmullRom(ref Vector4D value1, ref Vector4D value2, ref Vector4D value3, ref Vector4D value4, float amount, out Vector4D result)
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

			result.W = 0.5f * ((((2.0f * value2.W) + ((-value1.W + value3.W) * amount)) +
				(((((2.0f * value1.W) - (5.0f * value2.W)) + (4.0f * value3.W)) - value4.W) * squared)) +
				((((-value1.W + (3.0f * value2.W)) - (3.0f * value3.W)) + value4.W) * cubed));
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
		public static Vector4D CatmullRom(Vector4D value1, Vector4D value2, Vector4D value3, Vector4D value4, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			return new Vector4D(0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
								(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) +
								((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
								0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
								(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) +
								((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed)),
								0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) +
								(((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) +
								((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed)),
								0.5f * ((((2.0f * value2.W) + ((-value1.W + value3.W) * amount)) +
								(((((2.0f * value1.W) - (5.0f * value2.W)) + (4.0f * value3.W)) - value4.W) * squared)) +
								((((-value1.W + (3.0f * value2.W)) - (3.0f * value3.W)) + value4.W) * cubed)));
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
		public static void Hermite(ref Vector4D value1, ref Vector4D tangent1, ref Vector4D value2, ref Vector4D tangent2, float amount, out Vector4D result)
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
			result.W = (((value1.W * part1) + (value2.W * part2)) + (tangent1.W * part3)) + (tangent2.W * part4);
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
		public static Vector4D Hermite(Vector4D value1, Vector4D tangent1, Vector4D value2, Vector4D tangent2, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			return new Vector4D((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
								(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
								(((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
								(((value1.W * part1) + (value2.W * part2)) + (tangent1.W * part3)) + (tangent2.W * part4));
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
		public static void Lerp(ref Vector4D start, ref Vector4D end, float amount, out Vector4D result)
		{
			result.X = start.X + ((end.X - start.X) * amount);
			result.Y = start.Y + ((end.Y - start.Y) * amount);
			result.Z = start.Z + ((end.Z - start.Z) * amount);
			result.W = start.W + ((end.W - start.W) * amount);
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
		public static Vector4D Lerp(Vector4D start, Vector4D end, float amount)
		{
			return new Vector4D(start.X + ((end.X - start.X) * amount),
								start.Y + ((end.Y - start.Y) * amount),
								start.Z + ((end.Z - start.Z) * amount),
								start.W + ((end.W - start.W) * amount));
		}

		/// <summary>
		/// Performs a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">When the method completes, contains the cubic interpolation of the two vectors.</param>
		public static void SmoothStep(ref Vector4D start, ref Vector4D end, float amount, out Vector4D result)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));

			result.X = start.X + ((end.X - start.X) * amount);
			result.Y = start.Y + ((end.Y - start.Y) * amount);
			result.Z = start.Z + ((end.Z - start.Z) * amount);
			result.W = start.W + ((end.W - start.W) * amount);
		}

		/// <summary>
		/// Performs a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>When the method completes, contains the cubic interpolation of the two vectors.</returns>
		public static Vector4D SmoothStep(Vector4D start, Vector4D end, float amount)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));

			return new Vector4D(start.X + ((end.X - start.X) * amount),
								start.Y + ((end.Y - start.Y) * amount),
								start.Z + ((end.Z - start.Z) * amount),
								start.W + ((end.W - start.W) * amount));
		}

		/// <summary>
		/// Calculates the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The distance between the two vectors.</param>
		public static void Distance(ref Vector4D value1, ref Vector4D value2, out float result)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			result = MathUtility.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
		}

		/// <summary>
		/// Calculates the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The distance between the two vectors.</returns>
		public static float Distance(Vector4D value1, Vector4D value2)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			return MathUtility.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
		}

		/// <summary>
		/// Calculates the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The squared distance between the two vectors.</param>
		/// <remarks>Distance squared is the value before taking the square root. 
		/// Distance squared can often be used in place of distance if relative comparisons are being made. 
		/// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
		/// compare the distance between A and B to the distance between A and C. Calculating the two distances 
		/// involves two square roots, which are computationally expensive. However, using distance squared 
		/// provides the same information and avoids calculating two square roots.
		/// </remarks>
		public static void DistanceSquared(ref Vector4D value1, ref Vector4D value2, out float result)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			result = ((x * x) + (y * y) + (z * z) + (w * w));
		}

		/// <summary>
		/// Calculates the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		/// <remarks>Distance squared is the value before taking the square root. 
		/// Distance squared can often be used in place of distance if relative comparisons are being made. 
		/// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
		/// compare the distance between A and B to the distance between A and C. Calculating the two distances 
		/// involves two square roots, which are computationally expensive. However, using distance squared 
		/// provides the same information and avoids calculating two square roots.
		/// </remarks>
		public static float DistanceSquared(Vector4D value1, Vector4D value2)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			return ((x * x) + (y * y) + (z * z) + (w * w));
		}
		#endregion

		#region Operators
		/// <summary>
		/// Operator to perform equality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(Vector4D left,Vector4D right)
		{
			return (MathUtility.EqualFloat(left.X, right.X) && MathUtility.EqualFloat(left.Y, right.Y) &&
						MathUtility.EqualFloat(left.Z, right.Z) && MathUtility.EqualFloat(left.W, right.W));
		}

		/// <summary>
		/// Operator to perform inequality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(Vector4D left,Vector4D right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator +(Vector4D left,Vector4D right)
		{
			Vector4D result;
			Add(ref left, ref right, out result);
			return result;
		}
		
		/// <summary>
		/// Operator to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator -(Vector4D left,Vector4D right)
		{
			Vector4D result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to negate a vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static Vector4D operator -(Vector4D vector)
		{
			Vector4D result;
			Negate(ref vector, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator *(Vector4D left,Vector4D right)
		{
			Vector4D result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator *(Vector4D left,float scalar)
		{
			Vector4D result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator *(float scalar,Vector4D right)
		{
			Vector4D result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator /(Vector4D left,float scalar)
		{
			Vector4D result;
			Divide(ref left, scalar, out result);
			return result;
		}		

		/// <summary>
		/// Operator to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator /(Vector4D left,Vector4D right)
		{
			Vector4D result;
			Divide(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to convert a 2D vector into a 4D vector.
		/// </summary>
		/// <param name="vector">2D vector.</param>
		/// <returns>4D vector.</returns>
		public static implicit operator Vector4D(Vector2D vector)
		{
			return new Vector4D(vector, 0.0f, 1.0f);
		}

		/// <summary>
		/// Operator to convert a 3D vector into a 4D vector.
		/// </summary>
		/// <param name="vector">3D vector</param>
		/// <returns>4D vector.</returns>
		public static implicit operator Vector4D(Vector3D vector)
		{
			return new Vector4D(vector, 1.0f);
		}

        /// <summary>
        /// Operator to convert a 4D vector into a 3D vector.
        /// </summary>
        /// <param name="vector">4D vector to convert.</param>
        /// <returns>3D vector.</returns>
        public static explicit operator Vector3D(Vector4D vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Operator to convert a 4D vector into a 2D vector.
        /// </summary>
        /// <param name="vector">4D vector to convert.</param>
        /// <returns>2D vector.</returns>
        public static explicit operator Vector2D(Vector4D vector)
        {
            return new Vector2D(vector.X, vector.Y);
        }
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Vector4D"/> struct.
		/// </summary>
		/// <param name="x">Horizontal position of the vector.</param>
		/// <param name="y">Vertical posiition of the vector.</param>
		/// <param name="z">Depth position of the vector.</param>
		/// <param name="w">W component of the vector.</param>
		public Vector4D(float x,float y,float z,float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector4D"/> struct.
		/// </summary>
		/// <param name="value">The value used to initialize the vector.</param>
		public Vector4D(float value)
		{
			W = Z = Y = X = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector4D"/> struct.
		/// </summary>
		/// <param name="vector">The vector to convert.</param>
		/// <param name="w">W component of the vector.</param>
		public Vector4D(Vector3D vector, float w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector4D"/> struct.
		/// </summary>
		/// <param name="vector">The vector to convert.</param>
		/// <param name="z">Z component of the vector.</param>
		/// <param name="w">W component of the vector.</param>
		public Vector4D(Vector2D vector, float z, float w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}
		#endregion

		#region IEquatable<Vector4D> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(Vector4D other)
		{
			return (MathUtility.EqualFloat(other.X, X) && MathUtility.EqualFloat(other.Y, Y) && MathUtility.EqualFloat(other.Z, Z) && MathUtility.EqualFloat(other.W, W));
		}
		#endregion
	}
}
