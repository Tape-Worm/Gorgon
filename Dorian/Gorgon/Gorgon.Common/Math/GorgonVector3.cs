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

//
//  Most of the code in this file was modified or taken directly from the SlimMath project by Mike Popoloski.
//  SlimMath may be downloaded from: http://code.google.com/p/slimmath/
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// A 3 dimensional vector.
	/// </summary>
	/// <remarks>
	/// Vector mathematics are commonly used in graphical 3D applications.  And other
	/// spatial related computations.
	/// This valuetype provides us a convienient way to use vectors and their operations.
	/// </remarks>
	[Serializable(), StructLayout(LayoutKind.Sequential, Pack = 4), TypeConverter(typeof(Design.GorgonVector3TypeConverter))]
	public struct GorgonVector3		
		: IEquatable<GorgonVector3>, IFormattable
	{
		#region Variables.
		/// <summary>
		/// A vector with all elements set to zero.
		/// </summary>
		public readonly static GorgonVector3 Zero = new GorgonVector3(0);
		/// <summary>
		/// A vector with all elements set to one.
		/// </summary>
		public readonly static GorgonVector3 One = new GorgonVector3(1.0f);
		/// <summary>
		/// Unit X vector.
		/// </summary>
		public readonly static GorgonVector3 UnitX = new GorgonVector3(1.0f,0,0);
		/// <summary>
		/// Unit Y vector.
		/// </summary>
		public readonly static GorgonVector3 UnitY = new GorgonVector3(0,1.0f,0);
		/// <summary>
		/// Unit Z vector.
		/// </summary>
		public readonly static GorgonVector3 UnitZ = new GorgonVector3(0,0,1.0f);
		/// <summary>
		/// The size of the vector in bytes.
		/// </summary>
		public readonly static int Size = Native.DirectAccess.SizeOf<GorgonVector3>();
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
		/// Property to return whether this vector is normalized or not.
		/// </summary>
		public bool IsNormalized
		{
			get
			{
				return GorgonMathUtility.Abs((X * X + Y * Y + Z * Z) - 1f) < 1e-6f;
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
		/// Function to get the absolute value of the vector.
		/// </summary>
		public void Abs()
		{
			X = GorgonMathUtility.Abs(X);
			Y = GorgonMathUtility.Abs(Y);
			Z = GorgonMathUtility.Abs(Z);
		}

		/// <summary>
		/// Function to get the absolute value of the vector.
		/// </summary>
		/// <param name="value">Vector to get the absolute value of.</param>
		/// <param name="result">The absolute value of the vector.</param>
		public static void Abs(ref GorgonVector3 value, out GorgonVector3 result)
		{
			result = new GorgonVector3(GorgonMathUtility.Abs(value.X), GorgonMathUtility.Abs(value.Y), GorgonMathUtility.Abs(value.Z));
		}

		/// <summary>
		/// Function to get the absolute value of the vector.
		/// </summary>
		/// <param name="value">Vector to get the absolute value of.</param>
		/// <returns>The absolute value of the vector.</returns>
		public static GorgonVector3 Abs(GorgonVector3 value)
		{
			return new GorgonVector3(GorgonMathUtility.Abs(value.X), GorgonMathUtility.Abs(value.Y), GorgonMathUtility.Abs(value.Z));
		}

		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <param name="result">The combined vector.</param>
		public static void Add(ref GorgonVector3 left, ref GorgonVector3 right, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				left.X + right.X,
				left.Y + right.Y,
				left.Z + right.Z
			);
		}

		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>The combined vector.</returns>
		public static GorgonVector3 Add(GorgonVector3 left, GorgonVector3 right)		
		{
			return new GorgonVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		/// <summary>
		/// Function to calculate a <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 3D triangle.
		/// </summary>
		/// <param name="vertex1Coordinate">A <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertex2Coordinate">A <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertex3Coordinate">A <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertex2Coordinate"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertex3Coordinate"/>).</param>
		/// <param name="result">The 3D Cartesian coordinates of the specified point.</param>
		public static void Barycentric(ref GorgonVector3 vertex1Coordinate, ref GorgonVector3 vertex2Coordinate, ref GorgonVector3 vertex3Coordinate, float vertex2Weight, float vertex3Weight, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				(vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X)),
				(vertex1Coordinate.Y + (vertex2Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y)),
				(vertex1Coordinate.Z + (vertex2Weight * (vertex2Coordinate.Z - vertex1Coordinate.Z))) + (vertex3Weight * (vertex3Coordinate.Z - vertex1Coordinate.Z))
			);
		}

		/// <summary>
		/// Function to calculate a <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 3D triangle.
		/// </summary>
		/// <param name="vertex1Coordinate">A <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertex2Coordinate">A <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertex3Coordinate">A <see cref="GorgonVector3"/> containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertex2Coordinate"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertex3Coordinate"/>).</param>
		/// <returns>The 3D Cartesian coordinates of the specified point.</returns>
		public static GorgonVector3 Barycentric(GorgonVector3 vertex1Coordinate, GorgonVector3 vertex2Coordinate, GorgonVector3 vertex3Coordinate, float vertex2Weight, float vertex3Weight)
		{
			return new GorgonVector3((vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X)),
								(vertex1Coordinate.Y + (vertex2Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y)),
								(vertex1Coordinate.Z + (vertex2Weight * (vertex2Coordinate.Z - vertex1Coordinate.Z))) + (vertex3Weight * (vertex3Coordinate.Z - vertex1Coordinate.Z)));
		}

		/// <summary>
		/// Function to calculate a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">The Catmull-Rom interpolation.</param>
		public static void CatmullRom(ref GorgonVector3 value1, ref GorgonVector3 value2, ref GorgonVector3 value3, ref GorgonVector3 value4, float amount, out GorgonVector3 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			result = new GorgonVector3(
				0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
				(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) +
				((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
				0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
				(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) +
				((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed)),
				0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) +
				(((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) +
				((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed))
			);
		}

		/// <summary>
		/// Function to calculate a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The Catmull-Rom interpolation.</returns>
		public static GorgonVector3 CatmullRom(GorgonVector3 value1, GorgonVector3 value2, GorgonVector3 value3, GorgonVector3 value4, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			return new GorgonVector3(0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
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
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the highest values of the two vectors.</param>
		public static void Ceiling(ref GorgonVector3 vector1, ref GorgonVector3 vector2, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				(vector1.X > vector2.X) ? vector1.X : vector2.X,
				(vector1.Y > vector2.Y) ? vector1.Y : vector2.Y,
				(vector1.Z > vector2.Z) ? vector1.Z : vector2.Z
			);
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the highest values of the two vectors.</returns>
		public static GorgonVector3 Ceiling(GorgonVector3 vector1, GorgonVector3 vector2)
		{
			return new GorgonVector3((vector1.X > vector2.X) ? vector1.X : vector2.X, (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y, (vector1.Z > vector2.Z) ? vector1.Z : vector2.Z);
		}

		/// <summary>
		/// Function to calculate the cosine of the elements of the vector.
		/// </summary>
		/// <param name="vector">The vector used to calculate the cosine.</param>
		/// <param name="result">A vector containing the cosine values.</param>
		public static void Cos(ref GorgonVector3 vector, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				GorgonMathUtility.Cos(vector.X),
				GorgonMathUtility.Cos(vector.Y),
				GorgonMathUtility.Cos(vector.Z)
			);
		}

		/// <summary>
		/// Function to calculate the cosine of the elements of the vector.
		/// </summary>
		/// <param name="vector">The vector used to calculate the cosine.</param>
		/// <returns>A vector containing the cosine values.</returns>
		public static GorgonVector3 Cos(GorgonVector3 vector)
		{
			return new GorgonVector3(
				GorgonMathUtility.Cos(vector.X),
				GorgonMathUtility.Cos(vector.Y),
				GorgonMathUtility.Cos(vector.Z)
			);
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">Vector containing the cross product.</param>
		public static void CrossProduct(ref GorgonVector3 left, ref GorgonVector3 right, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				left.Y * right.Z - left.Z * right.Y,
				left.Z * right.X - left.X * right.Z,
				left.X * right.Y - left.Y * right.X
			);
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>Vector containing the cross product.</returns>
		public static GorgonVector3 CrossProduct(GorgonVector3 left, GorgonVector3 right)
		{
			return new GorgonVector3(left.Y * right.Z - left.Z * right.Y,
								left.Z * right.X - left.X * right.Z,
								left.X * right.Y - left.Y * right.X);
		}

		/// <summary>
		/// Function to return the distance between two vectors.
		/// </summary>
		/// <param name="vector1">Starting vector.</param>
		/// <param name="vector2">Ending vector.</param>
		/// <param name="result">The distance between the vectors.</param>
		public static void Distance(ref GorgonVector3 vector1, ref GorgonVector3 vector2, out float result)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			result = GorgonMathUtility.Sqrt((x * x) + (y * y));
		}

		/// <summary>
		/// Function to return the distance between two vectors.
		/// </summary>
		/// <param name="vector1">Starting vector.</param>
		/// <param name="vector2">Ending vector.</param>
		/// <returns>The distance between the vectors.</returns>
		public static float Distance(GorgonVector3 vector1, GorgonVector3 vector2)
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
		/// <param name="result">The squared distance between the vectors.</param>
		public static void DistanceSquared(ref GorgonVector3 vector1, ref GorgonVector3 vector2, out float result)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			result = (x * x) + (y * y);
		}

		/// <summary>
		/// Function to return the squared distance between two vectors.
		/// </summary>
		/// <param name="vector1">Starting vector.</param>
		/// <param name="vector2">Ending vector.</param>
		/// <returns>The squared distance between the vectors.</returns>
		public static float DistanceSquared(GorgonVector3 vector1, GorgonVector3 vector2)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			return (x * x) + (y * y);
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static void Divide(ref GorgonVector3 left, float scalar, out GorgonVector3 result)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;

			result = new GorgonVector3(
				left.X * inverse,
				left.Y * inverse,
				left.Z * inverse
			);
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static GorgonVector3 Divide(GorgonVector3 left, float scalar)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;
			return new GorgonVector3(left.X * inverse, left.Y * inverse, left.Z * inverse);
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">The dot product.</param>
		public static void DotProduct(ref GorgonVector3 left, ref GorgonVector3 right, out float result)
		{
			result = (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The dot product.</returns>
		public static float DotProduct(GorgonVector3 left, GorgonVector3 right)
		{
			return (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
		}

		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			if ((obj != null) && (obj is GorgonVector3))
				return Equals((GorgonVector3)obj);

			return false;
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonVector3 vector1, ref GorgonVector3 vector2)
		{
			return (vector1.X == vector2.X) && (vector1.Y == vector2.Y) && (vector1.Z == vector2.Z);
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="epsilon">Epsilon value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonVector3 vector1, ref GorgonVector3 vector2, float epsilon)
		{
			return (GorgonMathUtility.EqualFloat(vector1.X, vector1.X, epsilon)) && (GorgonMathUtility.EqualFloat(vector1.Y, vector2.Y, epsilon)) && (GorgonMathUtility.EqualFloat(vector1.Z, vector2.Z, epsilon));
		}

		/// <summary>
		/// Function to return whether this vector and another are equal.
		/// </summary>
		/// <param name="vector">Vector to compare.</param>
		/// <param name="epsilon">Epsilon value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(GorgonVector3 vector, float epsilon)
		{
			return (GorgonMathUtility.EqualFloat(X, vector.X, epsilon)) && (GorgonMathUtility.EqualFloat(Y, vector.Y, epsilon)) && (GorgonMathUtility.EqualFloat(Z, vector.Z, epsilon));
		}

		/// <summary>
		/// Function to take e raised to the elements in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each element of.</param>
		/// <param name="result">A vector containing e raised to the power of the components in the vector.</param>
		public static void Exp(ref GorgonVector3 value, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				GorgonMathUtility.Exp(value.X),
				GorgonMathUtility.Exp(value.Y),
				GorgonMathUtility.Exp(value.Z)
			);
		}

		/// <summary>
		/// Function to take e raised to the elements in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each element of.</param>
		/// <returns>A vector containing e raised to the power of the components in the vector.</returns>
		public static GorgonVector3 Exp(GorgonVector3 value)
		{
			return new GorgonVector3(
				GorgonMathUtility.Exp(value.X),
				GorgonMathUtility.Exp(value.Y),
				GorgonMathUtility.Exp(value.Z)
			);
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the lowest values of the two vectors.</param>
		public static void Floor(ref GorgonVector3 vector1, ref GorgonVector3 vector2, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				(vector1.X < vector2.X) ? vector1.X : vector2.X,
				(vector1.Y < vector2.Y) ? vector1.Y : vector2.Y,
				(vector1.Z < vector2.Z) ? vector1.Z : vector2.Z
			);
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the lowest values of the two vectors.</returns>
		public static GorgonVector3 Floor(GorgonVector3 vector1, GorgonVector3 vector2)
		{
			return new GorgonVector3((vector1.X < vector2.X) ? vector1.X : vector2.X, (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y, (vector1.Z < vector2.Z) ? vector1.Z : vector2.Z);
		}
		
		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>The hash code for this object.</returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(X).GenerateHash(Y).GenerateHash(Z);
		}

		/// <summary>
		/// Function to calculate a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">First source position vector.</param>
		/// <param name="tangent1">First source tangent vector.</param>
		/// <param name="value2">Second source position vector.</param>
		/// <param name="tangent2">Second source tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">The Hermite spline interpolation.</param>
		public static void Hermite(ref GorgonVector3 value1, ref GorgonVector3 tangent1, ref GorgonVector3 value2, ref GorgonVector3 tangent2, float amount, out GorgonVector3 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			result = new GorgonVector3(
				(((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
				(((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4)
			);
		}

		/// <summary>
		/// Function to calculate a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">First source position vector.</param>
		/// <param name="tangent1">First source tangent vector.</param>
		/// <param name="value2">Second source position vector.</param>
		/// <param name="tangent2">Second source tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The Hermite spline interpolation.</returns>
		public static GorgonVector3 Hermite(GorgonVector3 value1, GorgonVector3 tangent1, GorgonVector3 value2, GorgonVector3 tangent2, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			return new GorgonVector3((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
								(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
								(((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4));
		}

		/// <summary>
		/// Function to perform a linear interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0.0f and 1.0f indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">The linear interpolation of the two vectors.</param>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
		/// </remarks>
		public static void Lerp(ref GorgonVector3 start, ref GorgonVector3 end, float amount, out GorgonVector3 result)
		{
			result = new GorgonVector3(	
				start.X + ((end.X - start.X) * amount),
				start.Y + ((end.Y - start.Y) * amount),
				start.Z + ((end.Z - start.Z) * amount)
			);
		}

		/// <summary>
		/// Function to perform a linear interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>The linear interpolation of the two vectors.</returns>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
		/// </remarks>
		public static GorgonVector3 Lerp(GorgonVector3 start, GorgonVector3 end, float amount)
		{
			return new GorgonVector3(start.X + ((end.X - start.X) * amount), start.Y + ((end.Y - start.Y) * amount), start.Z + ((end.Z - start.Z) * amount));
		}

		/// <summary>
		/// Function to return a vector containing the largest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">A vector with the maximum elements of each vector.</param>
		public static void Max(ref GorgonVector3 value1, ref GorgonVector3 value2, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				(value1.X > value2.X) ? value1.X : value2.X,
				(value1.Y > value2.Y) ? value1.Y : value2.Y,
				(value1.Z > value2.Z) ? value1.Z : value2.Z
			);
		}

		/// <summary>
		/// Function to return a vector containing the largest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>A vector with the maximum elements of each vector.</returns>
		public static GorgonVector3 Max(GorgonVector3 value1, GorgonVector3 value2)
		{
			return new GorgonVector3(
				(value1.X > value2.X) ? value1.X : value2.X,
				(value1.Y > value2.Y) ? value1.Y : value2.Y,
				(value1.Z > value2.Z) ? value1.Z : value2.Z
			);
		}

		/// <summary>
		/// Function to return a vector containing the smallest elements of the two vectors.
		/// </summary>
		/// >param name="value1">The first vector.>/param>
		/// >param name="value2">The second vector.>/param>
		/// >param name="result">A vector with the minimum elements of each vector.>/param>
		public static void Min(ref GorgonVector3 value1, ref GorgonVector3 value2, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				(value1.X < value2.X) ? value1.X : value2.X,
				(value1.Y < value2.Y) ? value1.Y : value2.Y,
				(value1.Z < value2.Z) ? value1.Z : value2.Z
			);
		}

		/// >summary>
		/// Function to return a vector containing the smallest elements of the two vectors.
		/// >/summary>
		/// >param name="value1">The first vector.>/param>
		/// >param name="value2">The second vector.>/param>
		/// >returns>A vector with the minimum elements of each vector.>/returns>
		public static GorgonVector3 Min(GorgonVector3 value1, GorgonVector3 value2)
		{
			return new GorgonVector3(
				(value1.X < value2.X) ? value1.X : value2.X,
				(value1.Y < value2.Y) ? value1.Y : value2.Y,
				(value1.Z < value2.Z) ? value1.Z : value2.Z
			);
		}

		/// <summary>
		/// Function to modulate the elements of two vectors.
		/// </summary>
		/// <param name="left">First vector to modulate.</param>
		/// <param name="right">Second vector to modulate.</param>
		/// <param name="result">A modulated vector.</param>
		public static void Modulate(ref GorgonVector3 left, ref GorgonVector3 right, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				left.X * right.X,
				left.Y * right.Y,
				left.Z * right.Z
			);
		}

		/// <summary>
		/// Function to modulate the elements of two vectors.
		/// </summary>
		/// <param name="left">First vector to modulate.</param>
		/// <param name="right">Second vector to modulate.</param>
		/// <returns>A modulated vector.</returns>
		public static GorgonVector3 Modulate(GorgonVector3 left, GorgonVector3 right)
		{
			return new GorgonVector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="result">The product of the vector and the scalar value.</param>
		public static void Multiply(ref GorgonVector3 left, float scalar, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				left.X * scalar,
				left.Y * scalar,
				left.Z * scalar
			);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>The product of the vector and the scalar value.</returns>
		public static GorgonVector3 Multiply(GorgonVector3 left, float scalar)
		{
			return new GorgonVector3(left.X * scalar, left.Y * scalar, left.Z * scalar);
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <param name="result">The negated vector.</param>
		public static void Negate(ref GorgonVector3 vector, out GorgonVector3 result)
		{
			result = new GorgonVector3(-vector.X, -vector.Y, -vector.Z);
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <returns>The negated vector.</returns>
		public static GorgonVector3 Negate(GorgonVector3 vector)
		{
			return new GorgonVector3(-vector.X, -vector.Y, -vector.Z);
		}
				
		/// <summary>
		/// Function to normalize a vector into a unit vector.
		/// </summary>
		/// <param name="vector">Vector to normalize.</param>
		/// <param name="result">The normalized vector.</param>
		public static void Normalize(ref GorgonVector3 vector, out GorgonVector3 result)
		{
			float length = vector.Length;

			if (length > 1e-6f)
			{
				float invLength = 1.0f / length;

				result = new GorgonVector3(
					vector.X * invLength,
					vector.Y * invLength,
					vector.Z * invLength
				);
			}
			else
				result = vector;
		}

		/// <summary>
		/// Function to normalize a vector into a unit vector.
		/// </summary>
		/// <param name="vector">Vector to normalize.</param>
		/// <returns>The normalized vector.</returns>
		public static GorgonVector3 Normalize(GorgonVector3 vector)
		{
			float length = vector.Length;

			if (length > 1e-6f)
			{
				float invLength = 1.0f / length;

				return new GorgonVector3(
					vector.X * invLength,
					vector.Y * invLength,
					vector.Z * invLength
				);
			}
			else
				return vector;
		}

		/// <summary>
		/// Function to normalize the vector into a unit vector.
		/// </summary>
		public void Normalize()
		{
			float length = Length;

			if (length <= 1e-6f)
				return;

			float invLength = 1.0f / length;

			X *= invLength;
			Y *= invLength;
			Z *= invLength;
		}

		/// <summary>
		/// Function to orthogonalize a list of vectors.
		/// </summary>
		/// <param name="destination">A list of orthogonalized vectors.</param>
		/// <param name="source">A list of vectors to orthogonalize.</param>
		/// <remarks>
		/// <para>Orthogonalization is the process of making all vectors orthogonal to each other. This
		/// means that any given vector in the list will be orthogonal to any other given vector in the
		/// list.</para>
		/// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
		/// tend to be numerically unstable. The numeric stability decreases according to the vectors
		/// position in the list so that the first vector is the most stable and the last vector is the
		/// least stable.</para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		public static void Orthogonalize(IList<GorgonVector3> destination, IList<GorgonVector3> source)
		{
			//Uses the modified Gram-Schmidt process.
			//q1 = m1
			//q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
			//q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
			//q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3
			//q5 = ...

			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			for (int i = 0; i < source.Count; ++i)
			{
				GorgonVector3 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					GorgonVector3 destVector = destination[r];
					float dotTop = 0.0f;
					float dotBottom = 0.0f;

					GorgonVector3.DotProduct(ref destVector, ref newvector, out dotTop);
					GorgonVector3.DotProduct(ref destVector, ref destVector, out dotBottom);

					GorgonVector3.Multiply(ref destVector, dotTop / dotBottom, out destVector);
					GorgonVector3.Subtract(ref newvector, ref destVector, out newvector);
				}

				destination[i] = newvector;
			}
		}

		/// <summary>
		/// Function to orthonormalize a list of vectors.
		/// </summary>
		/// <param name="destination">The list of orthonormalized vectors.</param>
		/// <param name="source">The list of vectors to orthonormalize.</param>
		/// <remarks>
		/// <para>Orthonormalization is the process of making all vectors orthogonal to each
		/// other and making all vectors of unit length. This means that any given vector will
		/// be orthogonal to any other given vector in the list.</para>
		/// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
		/// tend to be numerically unstable. The numeric stability decreases according to the vectors
		/// position in the list so that the first vector is the most stable and the last vector is the
		/// least stable.</para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		public static void Orthonormalize(IList<GorgonVector3> destination, IList<GorgonVector3> source)
		{
			//Uses the modified Gram-Schmidt process.
			//Because we are making unit vectors, we can optimize the math for orthogonalization
			//and simplify the projection operation to remove the division.
			//q1 = m1 / |m1|
			//q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
			//q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|
			//q4 = (m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3) / |m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3|
			//q5 = ...

			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			for (int i = 0; i < source.Count; ++i)
			{
				GorgonVector3 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					float dot = 0.0f;
					GorgonVector3 destVector = destination[r];

					GorgonVector3.DotProduct(ref destVector, ref newvector, out dot);
					GorgonVector3.Multiply(ref destVector, dot, out destVector);
					GorgonVector3.Subtract(ref newvector, ref destVector, out newvector);
				}

				GorgonVector3.Normalize(ref newvector, out newvector);
				destination[i] = newvector;
			}
		}

		/// <summary>
		/// Function to project a 3D vector from object space into screen space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="worldViewProjection">World/view/projection matrix.</param>
		/// <param name="result">The vector in screen space.</param>
#warning You didn't implement this function you fucking idiot.
		public static void Project(ref GorgonVector3 vector, ref RectangleF viewRectangle, ref GorgonMinMaxF depthRange, ref GorgonMatrix worldViewProjection, out GorgonVector3 result)
		{			
			result = GorgonVector3.Zero;
		}

		/// <summary>
		/// Function to project a 3D vector from object space into screen space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="worldViewProjection">World/view/projection matrix.</param>
		/// <returns>The vector in screen space.</returns>
#warning You didn't implement this function you fucking idiot.
		public static GorgonVector3 Project(GorgonVector3 vector, RectangleF viewRectangle, GorgonMinMaxF depthRange, GorgonMatrix worldViewProjection)
		{
			return GorgonVector3.Zero;
		}

		/// <summary>
		/// Function to project a 3D vector from screen space into object space. 
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="worldViewProjection">World/view/projection matrix.</param>
		/// <param name="result">The vector in object space.</param>
#warning You didn't implement this function you fucking idiot.
		public static void Unproject(ref GorgonVector3 vector, ref RectangleF viewRectangle, ref GorgonMinMaxF depthRange, ref GorgonMatrix worldViewProjection, out GorgonVector3 result)
		{
			result = GorgonVector3.Zero;
		}

		/// <summary>
		/// Function to project a 3D vector from screen space into object space.  
		/// </summary>
		/// <param name="vector">The vector to project.</param>
		/// <param name="viewRectangle">The viewport representing screen space.</param>
		/// <param name="depthRange">The range for the minimum and maximum depth values.</param>
		/// <param name="worldViewProjection">World/view/projection matrix.</param>
		/// <returns>The vector in object space.</returns>
#warning You didn't implement this function you fucking idiot.
		public static GorgonVector3 Unproject(GorgonVector3 vector, Rectangle viewRectangle, GorgonMinMaxF depthRange, GorgonMatrix worldViewProjection)
		{
			return GorgonVector3.Zero;
		}

		/// <summary>
		/// Function to take the reciprocal of the elements in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <param name="result">The reciprocal of the vector.</param>
		public static void Reciprocal(ref GorgonVector3 value, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				1.0f / value.X,
				1.0f / value.Y,
				1.0f / value.Z
			);
		}

		/// <summary>
		/// Function to find the square root of each element in the vector and then takes the reciprocal of each.
		/// </summary>
		/// <param name="value">The vector to take the square root and recpirocal of.</param>
		/// <param name="result">The square root and reciprocal of the input vector.</param>
		public static void ReciprocalSqrt(ref GorgonVector3 value, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				1.0f / GorgonMathUtility.Sqrt(value.X),
				1.0f / GorgonMathUtility.Sqrt(value.Y),
				1.0f / GorgonMathUtility.Sqrt(value.Z)
			);
		}

		/// <summary>
		/// Function to take the reciprocal of the elements in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <param name="returns">The reciprocal of the vector.</returns>
		public static GorgonVector3 Reciprocal(GorgonVector3 value)
		{
			return new GorgonVector3(
				1.0f / value.X,
				1.0f / value.Y,
				1.0f / value.Z
			);
		}

		/// <summary>
		/// Function to find the square root of each element in the vector and then takes the reciprocal of each.
		/// </summary>
		/// <param name="value">The vector to take the square root and recpirocal of.</param>
		/// <returns>The square root and reciprocal of the input vector.</returns>
		public static GorgonVector3 ReciprocalSqrt(GorgonVector3 value)
		{
			return new GorgonVector3(
				1.0f / GorgonMathUtility.Sqrt(value.X),
				1.0f / GorgonMathUtility.Sqrt(value.Y),
				1.0f / GorgonMathUtility.Sqrt(value.Z)
			);
		}

		/// <summary>
		/// Function to calculate the reflection of a vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <param name="result">The reflected vector.</param>
		/// <remarks>This only gives the direction of a reflection off a surface, it does not determine 
		/// whether the original vector was close enough to the surface to hit it.</remarks>
		public static void Reflect(ref GorgonVector3 vector, ref GorgonVector3 normal, out GorgonVector3 result)
		{
			float dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

			result = new GorgonVector3(
				vector.X - ((2.0f * dot) * normal.X),
				vector.Y - ((2.0f * dot) * normal.Y),
				vector.Z - ((2.0f * dot) * normal.Z)
			);
		}

		/// <summary>
		/// Function to calculate the reflection of a vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <returns>The reflected vector.</returns>
		/// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
		/// whether the original vector was close enough to the surface to hit it.</remarks>
		public static GorgonVector3 Reflect(GorgonVector3 vector, GorgonVector3 normal)
		{
			float dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

			return new GorgonVector3(vector.X - ((2.0f * dot) * normal.X), vector.Y - ((2.0f * dot) * normal.Y), vector.Z - ((2.0f * dot) * normal.Z));
		}

		/// <summary>
		/// Function to return the refraction vector off a surface that has the specified normal and refraction index.
		/// </summary>
		/// <param name="vector">The source vector to refract.</param>
		/// <param name="normal">Normal to the surface.</param>
		/// <param name="index">Index of refraction.</param>
		/// <param name="result">The refracted vector.</param>
		public static void Refract(ref GorgonVector3 vector, ref GorgonVector3 normal, float index, out GorgonVector3 result)
		{
			float cos1;
			DotProduct(ref vector, ref normal, out cos1);

			float radicand = 1.0f - (index * index) * (1.0f - (cos1 * cos1));

			if (radicand < 0.0f)
			{
				result = GorgonVector3.Zero;
				return;
			}

			float cos2 = GorgonMathUtility.Sqrt(radicand);

			GorgonVector3 indexVector = GorgonVector3.Zero;
			GorgonVector3 normalVector = GorgonVector3.Zero;

			GorgonVector3.Multiply(ref indexVector, index, out indexVector);
			GorgonVector3.Multiply(ref normalVector, (cos2 - index * cos1), out normalVector);
			GorgonVector3.Add(ref indexVector, ref normalVector, out result);
		}

		/// <summary>
		/// Function to return the refraction vector off a surface that has the specified normal and refraction index.
		/// </summary>
		/// <param name="vector">The source vector to refract.</param>
		/// <param name="normal">Normal to the surface.</param>
		/// <param name="index">Index of refraction.</param>
		/// <returns>The refracted vector.</returns>
		public static GorgonVector3 Refract(GorgonVector3 vector, GorgonVector3 normal, float index)
		{
			float cos1;
			DotProduct(ref vector, ref normal, out cos1);

			float radicand = 1.0f - (index * index) * (1.0f - (cos1 * cos1));

			if (radicand < 0.0f)
				return GorgonVector3.Zero;

			float cos2 = GorgonMathUtility.Sqrt(radicand);

			GorgonVector3 indexVector = GorgonVector3.Zero;
			GorgonVector3 normalVector = GorgonVector3.Zero;

			GorgonVector3.Multiply(ref indexVector, index, out indexVector);
			GorgonVector3.Multiply(ref normalVector, (cos2 - index * cos1), out normalVector);
			GorgonVector3.Add(ref indexVector, ref normalVector, out indexVector);

			return indexVector;
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <param name="result">Rounded vector.</param>
		public static void Round(ref GorgonVector3 vector, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				GorgonMathUtility.RoundInt(vector.X),
				GorgonMathUtility.RoundInt(vector.Y),
				GorgonMathUtility.RoundInt(vector.Z)
			);
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <returns>Rounded vector.</returns>
		public static GorgonVector3 Round(GorgonVector3 vector)
		{
			return new GorgonVector3(GorgonMathUtility.RoundInt(vector.X), GorgonMathUtility.RoundInt(vector.Y), GorgonMathUtility.RoundInt(vector.Z));
		}

		/// <summary>
		/// Function to find the sine of the elements in the vector.
		/// </summary>
		/// <param name="vector">The vector to get the sine of.</param>
		/// <param name="result">The sine of the vector.</param>
		public static void Sin(ref GorgonVector3 vector, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				GorgonMathUtility.Sin(vector.X),
				GorgonMathUtility.Sin(vector.Y),
				GorgonMathUtility.Sin(vector.Z)
			);
		}

		/// <summary>
		/// Function to find the sine of the elements in the vector.
		/// </summary>
		/// <param name="vector">The vector to get the sine of.</param>
		/// <param name="result">The sine of the vector.</param>
		public static GorgonVector3 Sin(GorgonVector3 vector)
		{
			return new GorgonVector3(
				GorgonMathUtility.Sin(vector.X),
				GorgonMathUtility.Sin(vector.Y),
				GorgonMathUtility.Sin(vector.Z)
			);
		}

		/// <summary>
		/// Function to calculate a a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">The cubic interpolation of the two vectors.</param>
		public static void SmoothStep(ref GorgonVector3 start, ref GorgonVector3 end, float amount, out GorgonVector3 result)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));

			result = new GorgonVector3(
				start.X + ((end.X - start.X) * amount),
				start.Y + ((end.Y - start.Y) * amount),
				start.Z + ((end.Z - start.Z) * amount)
			);
		}

		/// <summary>
		/// Function to calculate a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>The cubic interpolation of the two vectors.</returns>
		public static GorgonVector3 SmoothStep(GorgonVector3 start, GorgonVector3 end, float amount)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));
			return new GorgonVector3(start.X + ((end.X - start.X) * amount), start.Y + ((end.Y - start.Y) * amount), start.Z + ((end.Z - start.Z) * amount));
		}

		/// <summary>
		/// Function to take the square root of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root of.</param>
		/// <param name="result">The square root of the vector.</param>
		public static void Sqrt(ref GorgonVector3 value, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				GorgonMathUtility.Sqrt(value.X),
				GorgonMathUtility.Sqrt(value.Y),
				GorgonMathUtility.Sqrt(value.Z)
			);
		}

		/// <summary>
		/// Function to take the square root of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root of.</param>
		/// <param name="result">The square root of the vector.</param>
		public static GorgonVector3 Sqrt(GorgonVector3 value)
		{
			return new GorgonVector3(
				GorgonMathUtility.Sqrt(value.X),
				GorgonMathUtility.Sqrt(value.Y),
				GorgonMathUtility.Sqrt(value.Z)
			);
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <param name="result">The difference between the two vectors.</param>
		public static void Subtract(ref GorgonVector3 left, ref GorgonVector3 right, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				left.X - right.X,
				left.Y - right.Y,
				left.Z - right.Z
			);
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>The difference between the two vectors.</returns>
		public static GorgonVector3 Subtract(GorgonVector3 left, GorgonVector3 right)
		{
			return new GorgonVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		/// <summary>
		/// Function to calculate the tangent of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <param name="result">The tangent of each element.</param>
		public static void Tan(ref GorgonVector3 value, out GorgonVector3 result)
		{
			result = new GorgonVector3(
				GorgonMathUtility.Tan(value.X),
				GorgonMathUtility.Tan(value.Y),
				GorgonMathUtility.Tan(value.Z)
			);
		}

		/// <summary>
		/// Function to calculate the tangent of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <returns>The tangent of each element.</returns>
		public static GorgonVector3 Tan(GorgonVector3 value)
		{
			return new GorgonVector3(
				GorgonMathUtility.Tan(value.X),
				GorgonMathUtility.Tan(value.Y),
				GorgonMathUtility.Tan(value.Z)
			);
		}

		/// <summary>
		/// Function to convert the vector into a 3 element array.
		/// </summary>
		/// <returns>A 3 element array containing the elements of the vector.</returns>
		/// <remarks>X is in the first element, Y is in the second element, Z is in the third element.</remarks>
		public float[] ToArray()
		{
			return new[] { X, Y, Z };
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <param name="provider">String formatting provider.</param>
		/// <returns>A string containing the type and values of this object.</returns>
		public string ToString(IFormatProvider provider)
		{
			return string.Format(provider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <param name="format">Format to use.</param>
		/// <returns>A string containing the type and values of this object.</returns>
		public string ToString(string format)
		{
			if (string.IsNullOrEmpty(format))
				return ToString();

			return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
		}

#warning Don't forget to implement these after Matrix and Quaternion are implemented (Transform/Coordinate/Normal).
		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <param name="result">The resulting transformed vector.</param>
		public static void Transform(ref GorgonMatrix m, ref GorgonVector3 vec, out GorgonVector3 result)
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
		public static GorgonVector3 Transform(GorgonMatrix m, GorgonVector3 vec)
		{
			float scale = 1.0f / (m.m41 * vec.X) + (m.m42 * vec.Y) + (m.m43 * vec.Z) + m.m44;

			return new GorgonVector3(((m.m11 * vec.X) + (m.m12 * vec.Y) + (m.m13 * vec.Z) + m.m14) * scale,
								((m.m21 * vec.X) + (m.m22 * vec.Y) + (m.m23 * vec.Z) + m.m24) * scale,
								((m.m31 * vec.X) + (m.m32 * vec.Y) + (m.m33 * vec.Z) + m.m34) * scale);
		}

		/// <summary>
		/// Function to transform a 3D vector with a quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform.</param>
		/// <param name="vector">Vector to transform.</param>
		/// <param name="result">The transformed vector.</param>
		public static void Transform(ref GorgonQuaternion q, ref GorgonVector3 vector, out GorgonVector3 result)
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
		public static GorgonVector3 Transform(GorgonQuaternion q, GorgonVector3 vector)
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

			return new GorgonVector3(((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))) + (vector.Z * (xz + wy)),
								((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz))) + (vector.Z * (yz - wx)),
								((vector.X * (xz - wy)) + (vector.Y * (yz + wx))) + (vector.Z * ((1.0f - xx) - yy)));
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to perform equality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonVector3 left,GorgonVector3 right)
		{
			return (GorgonMathUtility.EqualFloat(left.X, right.X) && GorgonMathUtility.EqualFloat(left.Y, right.Y) && GorgonMathUtility.EqualFloat(left.Z, right.Z));
		}

		/// <summary>
		/// Operator to perform inequality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(GorgonVector3 left,GorgonVector3 right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector3 operator +(GorgonVector3 left,GorgonVector3 right)
		{
			GorgonVector3 result;
			Add(ref left, ref right, out result);
			return result;
		}
		
		/// <summary>
		/// Operator to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector3 operator -(GorgonVector3 left,GorgonVector3 right)
		{
			GorgonVector3 result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to negate a vector.
		/// </summary>
		/// <param name="left">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static GorgonVector3 operator -(GorgonVector3 left)
		{
			GorgonVector3 result;
			Negate(ref left, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector3 operator *(GorgonVector3 left,float scalar)
		{
			GorgonVector3 result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector3 operator *(float scalar,GorgonVector3 right)
		{
			GorgonVector3 result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector3 operator /(GorgonVector3 left,float scalar)
		{
			GorgonVector3 result;
			Divide(ref left, scalar, out result);
			return result;
		}		

		/// <summary>
		/// Operator to convert a 2D vector into a 3D vector.
		/// </summary>
		/// <param name="vector">2D vector.</param>
		/// <returns>3D vector.</returns>
		public static implicit operator GorgonVector3(GorgonVector2 vector)
		{
			return new GorgonVector3(vector, 0.0f);
		}

		/// <summary>
		/// Operator to convert a 3D vector into a 2D vector.
		/// </summary>
		/// <param name="vector">3D vector to convert.</param>
		/// <returns>2D vector.</returns>
		public static explicit operator GorgonVector2(GorgonVector3 vector)
		{
			return new GorgonVector2(vector.X, vector.Y);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector3"/> struct.
		/// </summary>
		/// <param name="value">The value used to initialize the vector.</param>
		public GorgonVector3(float value)
		{
			Z = Y = X = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector3"/> struct.
		/// </summary>
		/// <param name="x">Horizontal position of the vector.</param>
		/// <param name="y">Vertical posiition of the vector.</param>
		/// <param name="z">Depth position of the vector.</param>
		public GorgonVector3(float x,float y,float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector3"/> struct.
		/// </summary>
		/// <param name="vector">The vector to copy.</param>
		/// <param name="z">Z coordinate for the vector.</param>
		public GorgonVector3(GorgonVector2 vector, float z)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector3"/> struct.
		/// </summary>
		/// <param name="values">The values for the vector.</param>
		/// <remarks>Only the first three elements in the list will be taken.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="values"/> parameter has less than 3 elements.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the values parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonVector3(IList<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Count < 3)
				throw new ArgumentException("The number of elements must be at least 3 for a 3D vector.", "values");

			X = values[0];
			Y = values[1];
			Z = values[2];
		}
		#endregion

		#region IEquatable<GorgonVector3> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonVector3 other)
		{
			return (GorgonMathUtility.EqualFloat(other.X, X) && GorgonMathUtility.EqualFloat(other.Y, Y) && GorgonMathUtility.EqualFloat(other.Z, Z));
		}
		#endregion

		#region IFormattable Members
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format))
				return ToString(formatProvider);

			return string.Format(formatProvider, "X:{0} Y:{1} Z:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));			
		}
		#endregion
	}
}
