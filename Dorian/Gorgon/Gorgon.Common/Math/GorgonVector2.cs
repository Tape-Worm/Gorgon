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
// Created: Tuesday, January 10, 2012 3:15:06 PM
// 
#endregion

//
//  Most of the code in this file was modified or taken directly from the SlimMath project by Mike Popoloski.
//  SlimMath may be downloaded from: http://code.google.com/p/slimmath/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// A 2 dimensional vector.
	/// </summary>
	/// <remarks>
	/// Vector mathematics are commonly used in graphical 3D applications.  And other
	/// spatial related computations.
	/// This type provides us a convienient way to use vectors and their operations.
	/// </remarks>
	[Serializable(), StructLayout(LayoutKind.Sequential, Pack = 4), TypeConverter(typeof(Design.GorgonVector2TypeConverter))]
	public struct GorgonVector2
		: IEquatable<GorgonVector2>, IFormattable
	{
		#region Variables.
		/// <summary>
		/// A vector with all elements set to zero.
		/// </summary>
		public readonly static GorgonVector2 Zero = new GorgonVector2(0);
		/// <summary>
		/// A vector with all elements set to one.
		/// </summary>
		public readonly static GorgonVector2 One = new GorgonVector2(1.0f);
		/// <summary>
		/// Unit X vector.
		/// </summary>
		public readonly static GorgonVector2 UnitX = new GorgonVector2(1.0f,0);
		/// <summary>
		/// Unit Y vector.
		/// </summary>
		public readonly static GorgonVector2 UnitY = new GorgonVector2(0,1.0f);
		/// <summary>
		/// The size of the vector in bytes.
		/// </summary>
		public readonly static int Size = Native.DirectAccess.SizeOf<GorgonVector2>();
		/// <summary>
		/// Horizontal position of the vector.
		/// </summary>
		public float X;
		/// <summary>
		/// Vertical position of the vector.
		/// </summary>
		public float Y;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the length of this vector.
		/// </summary>
		public float Length
		{
			get
			{
				return GorgonMathUtility.Sqrt(X * X + Y * Y);
			}
		}

		/// <summary>
		/// Property to return the length of this vector squared.
		/// </summary>
		public float LengthSquare
		{
			get
			{
				return X * X + Y * Y;
			}
		}

		/// <summary>
		/// Property to return whether this value is normalized or not.
		/// </summary>
		public bool IsNormalized
		{
			get 
			{
				return GorgonMathUtility.Abs((X * X + Y * Y) - 1f) < 1e-6f; 
			}
		}

		/// <summary>
		/// Property to set or return the components of the vector by an index.
		/// </summary>
		/// <remarks>The index must be between 0..1</remarks>
		/// <exception cref="System.IndexOutOfRangeException">The index was not between 0 and 1.</exception>
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
			this.X = GorgonMathUtility.Abs(X);
			this.Y = GorgonMathUtility.Abs(Y);
		}

		/// <summary>
		/// Function to get the absolute value of the vector.
		/// </summary>
		/// <param name="value">Vector to get the absolute value of.</param>
		/// <param name="result">The absolute value of the vector.</param>
		public static void Abs(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(GorgonMathUtility.Abs(value.X), GorgonMathUtility.Abs(value.Y));
		}

		/// <summary>
		/// Function to get the absolute value of the vector.
		/// </summary>
		/// <param name="value">Vector to get the absolute value of.</param>
		/// <returns>The absolute value of the vector.</returns>
		public static GorgonVector2 Abs(GorgonVector2 value)
		{
			return new GorgonVector2(GorgonMathUtility.Abs(value.X), GorgonMathUtility.Abs(value.Y));
		}

		/// <summary>
		/// Function to add two vectors together.
		/// </summary>
		/// <param name="left">Left vector to add.</param>
		/// <param name="right">Right vector to add.</param>
		/// <returns>The combined vectors.</returns>
		public static GorgonVector2 Add(GorgonVector2 left, GorgonVector2 right)
		{
			return new GorgonVector2(left.X + right.X, left.Y + right.Y);
		}

		/// <summary>
		/// Function to add two vectors together.
		/// </summary>
		/// <param name="left">Left vector to add.</param>
		/// <param name="right">Right vector to add.</param>
		/// <param name="result">The combined vectors.</param>
		public static void Add(ref GorgonVector2 left, ref GorgonVector2 right, out GorgonVector2 result)
		{
			result = new GorgonVector2(left.X + right.X, left.Y + right.Y);
		}

		/// <summary>
		/// Function to return the 2D Cartesian coordinates of a point in Barycentric coordinates relative to a 2D triangle.
		/// </summary>
		/// <param name="vertex1Coordinate">Coordinate of the first vertex in the 2D triangle.</param>
		/// <param name="vertex2Coordinate">Coordinate of the second vertex in the 2D triangle.</param>
		/// <param name="vertex3Coordinate">Coordinate of the third vertex in the 2D triangle.</param>
		/// <param name="vertex2Weight">The weighting factor for the <paramref name="vertex2Coordinate">2nd vertex</paramref>.</param>
		/// <param name="vertex3Weight">The weighting factor for the <paramref name="vertex3Coordinate">3rd vertex</paramref>.</param>
		/// <param name="result">The 2D Cartesian coordinates of the point.</param>
		public static void Barycentric(ref GorgonVector2 vertex1Coordinate, ref GorgonVector2 vertex2Coordinate, ref GorgonVector2 vertex3Coordinate, float vertex2Weight, float vertex3Weight, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				(vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X)),
				(vertex1Coordinate.Y + (vertex3Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y))
			);
		}

		/// <summary>
		/// Function to return the 2D Cartesian coordinates of a point in Barycentric coordinates relative to a 2D triangle.
		/// </summary>
		/// <param name="vertex1Coordinate">Coordinate of the first vertex in the 2D triangle.</param>
		/// <param name="vertex2Coordinate">Coordinate of the second vertex in the 2D triangle.</param>
		/// <param name="vertex3Coordinate">Coordinate of the third vertex in the 2D triangle.</param>
		/// <param name="vertex2Weight">The weighting factor for the <paramref name="vertex2Coordinate">2nd vertex</paramref>.</param>
		/// <param name="vertex3Weight">The weighting factor for the <paramref name="vertex3Coordinate">3rd vertex</paramref>.</param>
		/// <returns>The 2D Cartesian coordinates of the point.</returns>
		public static GorgonVector2 Barycentric(GorgonVector2 vertex1Coordinate, GorgonVector2 vertex2Coordinate, GorgonVector2 vertex3Coordinate, float vertex2Weight, float vertex3Weight)
		{
			return new GorgonVector2(
				(vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X)),
				(vertex1Coordinate.Y + (vertex3Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y))
			);
		}

		/// <summary>
		/// Function to calcualte a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">The result of the Catmull-Rom interpolation.</param>
		public static void CatmullRom(ref GorgonVector2 value1, ref GorgonVector2 value2, ref GorgonVector2 value3, ref GorgonVector2 value4, float amount, out GorgonVector2 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			result = new GorgonVector2(
				0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
				(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) +
				((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
				0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
				(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) +
				((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed))
			);
		}

		/// <summary>
		/// Function to calcualte a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The Catmull-Rom interpolation.</returns>
		public static GorgonVector2 CatmullRom(GorgonVector2 value1, GorgonVector2 value2, GorgonVector2 value3, GorgonVector2 value4, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			return new GorgonVector2(
				0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
				(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) +
				((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
				0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
				(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) +
				((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed))
			);
		}

		/// <summary>
		/// Function to calculate the cosine of each element in the vector.
		/// </summary>
		/// <param name="value">Vector to calculate.</param>
		/// <param name="result">The cosine of each element in the vector.</param>
		public static void Cos(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(GorgonMathUtility.Cos(value.X), GorgonMathUtility.Cos(value.Y));
		}

		/// <summary>
		/// Function to calculate the cosine of each element in the vector.
		/// </summary>
		/// <param name="value">Vector to calculate.</param>
		/// <returns>The cosine of each element in the vector.</returns>
		public static GorgonVector2 Cos(GorgonVector2 value)
		{
			return new GorgonVector2(GorgonMathUtility.Cos(value.X), GorgonMathUtility.Cos(value.Y));
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="vector1">Vector to perform the cross product with.</param>
		/// <param name="vector2">Vector to perform the cross product with.</param>
		/// <param name="result">A new vector containing the cross product.</param>
		public static void CrossProduct(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				vector2.X * vector1.Y - vector2.Y * vector1.X,
				vector2.Y * vector1.X - vector2.X * vector1.Y
			);
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="vector1">Vector to perform the cross product with.</param>
		/// <param name="vector2">Vector to perform the cross product with.</param>
		/// <returns>A new vector containing the cross product.</returns>
		public static GorgonVector2 CrossProduct(GorgonVector2 vector1, GorgonVector2 vector2)
		{
			return new GorgonVector2(
				vector2.X * vector1.Y - vector2.Y * vector1.X,
				vector2.Y * vector1.X - vector2.X * vector1.Y
			);
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the highest values of the two vectors.</param>
		public static void Ceiling(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				(vector1.X > vector2.X) ? vector1.X : vector2.X,
				(vector1.Y > vector2.Y) ? vector1.Y : vector2.Y
			);
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the highest values of the two vectors.</returns>
		public static GorgonVector2 Ceiling(GorgonVector2 vector1, GorgonVector2 vector2)
		{
			return new GorgonVector2(
				(vector1.X > vector2.X) ? vector1.X : vector2.X,
				(vector1.Y > vector2.Y) ? vector1.Y : vector2.Y
			);
		}

		/// <summary>
		/// Function to clamp a vector to a specified range.
		/// </summary>
		/// <param name="value">Value to clamp.</param>
		/// <param name="min">Minimum value to evaluate against.</param>
		/// <param name="max">Maximum value to evaluate against.</param>
		/// <param name="result">The clamped vector.</param>
		public static void Clamp(ref GorgonVector2 value, ref GorgonVector2 min, ref GorgonVector2 max, out GorgonVector2 result)
		{
			float x = value.X;
			float y = value.Y;

			x = (x < min.X) ? min.X : x;
			y = (y < min.Y) ? min.Y : y;
			x = (x > max.X) ? max.X : x;
			y = (y > max.Y) ? max.Y : y;

			result = new GorgonVector2(x, y);
		}

		/// <summary>
		/// Function to clamp a vector to a specified range.
		/// </summary>
		/// <param name="value">Value to clamp.</param>
		/// <param name="min">Minimum value to evaluate against.</param>
		/// <param name="max">Maximum value to evaluate against.</param>
		/// <returns>The clamped vector.</returns>
		public static GorgonVector2 Clamp(GorgonVector2 value, GorgonVector2 min, GorgonVector2 max)
		{
			float x = value.X;
			float y = value.Y;

			x = (x < min.X) ? min.X : x;
			y = (y < min.Y) ? min.Y : y;
			x = (x > max.X) ? max.X : x;
			y = (y > max.Y) ? max.Y : y;

			return new GorgonVector2(x, y);
		}

		/// <summary>
		/// Function to return the distance between two vectors.
		/// </summary>
		/// <param name="vector1">Starting vector.</param>
		/// <param name="vector2">Ending vector.</param>
		/// <param name="result">The distance between the vectors.</param>
		public static void Distance(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out float result)
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
		public static float Distance(GorgonVector2 vector1, GorgonVector2 vector2)
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
		public static void DistanceSquared(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out float result)
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
		public static float DistanceSquared(GorgonVector2 vector1, GorgonVector2 vector2)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			return (x * x) + (y * y);
		}

		/// <summary>
		/// Function to divide a vector by a floating point value.
		/// </summary>
		/// <param name="dividend">The vector to be divided.</param>
		/// <param name="divisor">The vector that will divide the dividend.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static void Divide(ref GorgonVector2 dividend, float divisor, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				dividend.X / divisor,
				dividend.Y / divisor
			);
		}

		/// <summary>
		/// Function to divide a vector by a floating point value.
		/// </summary>
		/// <param name="dividend">The vector to be divided.</param>
		/// <param name="divisor">The vector that will divide the dividend.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static GorgonVector2 Divide(GorgonVector2 dividend, float divisor)
		{
			return new GorgonVector2(dividend.X / divisor, dividend.Y / divisor);
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public static float DotProduct(GorgonVector2 left, GorgonVector2 right)
		{
			return (left.X * right.X) + (left.Y * right.Y);
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">The dot product of the two vectors.</param>
		public static void DotProduct(ref GorgonVector2 left, ref GorgonVector2 right, out float result)
		{
			result = (left.X * right.X) + (left.Y * right.Y);
		}

		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			if ((obj != null) && (obj is GorgonVector2))
				return Equals((GorgonVector2)obj);

			return false;
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonVector2 vector1, ref GorgonVector2 vector2)
		{
			return (vector1.X == vector2.X) && (vector1.Y == vector2.Y);
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="epsilon">Epsilon value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonVector2 vector1, ref GorgonVector2 vector2, float epsilon)
		{
			return (GorgonMathUtility.EqualFloat(vector1.X, vector1.X, epsilon)) && (GorgonMathUtility.EqualFloat(vector1.Y, vector2.Y, epsilon));
		}

		/// <summary>
		/// Function to return whether this vector and another are equal.
		/// </summary>
		/// <param name="vector">Vector to compare.</param>
		/// <param name="epsilon">Epsilon value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(GorgonVector2 vector, float epsilon)
		{
			return (GorgonMathUtility.EqualFloat(X, vector.X, epsilon)) && (GorgonMathUtility.EqualFloat(Y, vector.Y, epsilon));
		}

		/// <summary>
		/// Function to take e raised to the elements in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each element of.</param>
		/// <param name="result">A vector containing e raised to the power of the components in the vector.</param>
		public static void Exp(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				GorgonMathUtility.Exp(value.X),
				GorgonMathUtility.Exp(value.Y)
			);
		}

		/// <summary>
		/// Function to take e raised to the elements in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each element of.</param>
		/// <returns>A vector containing e raised to the power of the components in the vector.</returns>
		public static GorgonVector2 Exp(GorgonVector2 value)
		{
			return new GorgonVector2(
				GorgonMathUtility.Exp(value.X),
				GorgonMathUtility.Exp(value.Y)
			);
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the lowest values of the two vectors.</param>
		public static void Floor(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out GorgonVector2 result)
		{
			result = new GorgonVector2(
					(vector1.X < vector2.X) ? vector1.X : vector2.X,
					(vector1.Y < vector2.Y) ? vector1.Y : vector2.Y
				);
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the lowest values of the two vectors.</returns>
		public static GorgonVector2 Floor(GorgonVector2 vector1, GorgonVector2 vector2)
		{
			return new GorgonVector2(
					(vector1.X < vector2.X) ? vector1.X : vector2.X,
					(vector1.Y < vector2.Y) ? vector1.Y : vector2.Y
				);
		}

		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>The hash code for this object.</returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(X).GenerateHash(Y);
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
		public static void Hermite(ref GorgonVector2 value1, ref GorgonVector2 tangent1, ref GorgonVector2 value2, ref GorgonVector2 tangent2, float amount, out GorgonVector2 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			result = new GorgonVector2(
				(((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)
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
		public static GorgonVector2 Hermite(GorgonVector2 value1, GorgonVector2 tangent1, GorgonVector2 value2, GorgonVector2 tangent2, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			return new GorgonVector2(
				(((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)
			);
		}

		/// <summary>
		/// Function to calculate a linear interpolation between two vectors.
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
		public static void Lerp(ref GorgonVector2 start, ref GorgonVector2 end, float amount, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				start.X + ((end.X - start.X) * amount),
				start.Y + ((end.Y - start.Y) * amount)
			);
		}

		/// <summary>
		/// Function to calculate a linear interpolation between two vectors.
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
		public static GorgonVector2 Lerp(GorgonVector2 start, GorgonVector2 end, float amount)
		{
			return new GorgonVector2(
				start.X + ((end.X - start.X) * amount),
				start.Y + ((end.Y - start.Y) * amount)
			);
		}

		/// <summary>
		/// Function to return a vector containing the largest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">A vector with the maximum elements of each vector.</param>
		public static void Max(ref GorgonVector2 value1, ref GorgonVector2 value2, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				(value1.X > value2.X) ? value1.X : value2.X,
				(value1.Y > value2.Y) ? value1.Y : value2.Y
			);
		}

		/// <summary>
		/// Function to return a vector containing the largest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>A vector with the maximum elements of each vector.</returns>
		public static GorgonVector2 Max(GorgonVector2 value1, GorgonVector2 value2)
		{
			return new GorgonVector2(
				(value1.X > value2.X) ? value1.X : value2.X,
				(value1.Y > value2.Y) ? value1.Y : value2.Y
			);
		}

		/// <summary>
		/// Function to return a vector containing the smallest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">A vector with the minimum elements of each vector.</param>
		public static void Min(ref GorgonVector2 value1, ref GorgonVector2 value2, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				(value1.X < value2.X) ? value1.X : value2.X,
				(value1.Y < value2.Y) ? value1.Y : value2.Y
			);
		}

		/// <summary>
		/// Function to return a vector containing the smallest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>A vector with the minimum elements of each vector.</returns>
		public static GorgonVector2 Min(GorgonVector2 value1, GorgonVector2 value2)
		{
			return new GorgonVector2(
				(value1.X < value2.X) ? value1.X : value2.X,
				(value1.Y < value2.Y) ? value1.Y : value2.Y
			);
		}

		/// <summary>
		/// Function to modulate the elements of two vectors.
		/// </summary>
		/// <param name="left">First vector to modulate.</param>
		/// <param name="right">Second vector to modulate.</param>
		/// <param name="result">A modulated vector.</param>
		public static void Modulate(ref GorgonVector2 left, ref GorgonVector2 right, out GorgonVector2 result)
		{
			result = new GorgonVector2(left.X * right.X, left.Y * right.Y);
		}

		/// <summary>
		/// Function to modulate the elements of two vectors.
		/// </summary>
		/// <param name="left">First vector to modulate.</param>
		/// <param name="right">Second vector to modulate.</param>
		/// <returns>A modulated vector.</returns>
		public static GorgonVector2 Modulate(GorgonVector2 left, GorgonVector2 right)
		{
			return new GorgonVector2(left.X * right.X, left.Y * right.Y);
		}

		/// <summary>
		/// Function to multiply a vector by a floating point value.
		/// </summary>
		/// <param name="left">Left vector to multiply.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="result">The product of the vector and scalar.</param>
		public static void Multiply(ref GorgonVector2 left, float scalar, out GorgonVector2 result)
		{
			result = new GorgonVector2(left.X * scalar, left.Y * scalar);
		}

		/// <summary>
		/// Function to multiply a vector by a floating point value.
		/// </summary>
		/// <param name="left">Left vector to multiply.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>The product of the vector and scalar.</returns>
		public static GorgonVector2 Multiply(GorgonVector2 left, float scalar)
		{
			return new GorgonVector2(left.X * scalar, left.Y * scalar);
		}

		/// <summary>
		/// Function to normalize this vector into a unit vector.
		/// </summary>
		public void Normalize()
		{
			float length = Length;

			if (length <= 1e-6f)
				return;

			float invLen = 1.0f / length;

			X *= invLen;
			Y *= invLen;
		}

		/// <summary>
		/// Function to normalize a vector into a vector.
		/// </summary>
		/// <param name="vector">The vector to normalize.</param>
		/// <returns>The normalized vector.</returns>
		public static GorgonVector2 Normalize(GorgonVector2 vector)
		{
			float length = vector.Length;

			if (length > 1e-6f)
			{
				float invLen = 1.0f / length;

				return new GorgonVector2(
					vector.X * invLen,
					vector.Y * invLen
				);
			}
			else
				return vector;
		}

		/// <summary>
		/// Function to normalize a vector into a vector.
		/// </summary>
		/// <param name="vector">The vector to normalize.</param>
		/// <param name="result">The normalized vector.</param>
		public static void Normalize(ref GorgonVector2 vector, out GorgonVector2 result)
		{
			float length = vector.Length;

			if (length > 1e-6f)
			{
				float invLen = 1.0f / length;

				result = new GorgonVector2(
					vector.X * invLen,
					vector.Y * invLen
				);
			}
			else
				result = vector;
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <param name="result">The negated vector.</param>
		public static void Negate(ref GorgonVector2 vector, out GorgonVector2 result)
		{
			result = new GorgonVector2(-vector.X, -vector.Y);
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <returns>The negated vector.</returns>
		public static GorgonVector2 Negate(GorgonVector2 vector)
		{
			return new GorgonVector2(-vector.X, -vector.Y);
		}

		/// <summary>
		/// Function to negate this vector.
		/// </summary>
		public void Negate()
		{
			X = -X;
			Y = -Y;
		}

		/// <summary>
		/// Function to orthogonalize a list of vectors.
		/// </summary>
		/// <param name="destination">The list of orthogonalized vectors.</param>
		/// <param name="source">The list of vectors to orthogonalize.</param>
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
		public static void Orthogonalize(IList<GorgonVector2> destination, IList<GorgonVector2> source)
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
			if (destination.Count < source.Count())
				throw new ArgumentOutOfRangeException("destination", "The destination list must be of same length or larger length than the source list.");

			for (int i = 0; i < source.Count; ++i)
			{
				GorgonVector2 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					float topDot = 0.0f;
					float bottomDot = 0.0f;
					GorgonVector2 destVector = destination[r];

					GorgonVector2.DotProduct(ref destVector, ref newvector, out topDot);
					GorgonVector2.DotProduct(ref destVector, ref destVector, out bottomDot);

					GorgonVector2.Multiply(ref destVector, topDot/bottomDot, out destVector);
					GorgonVector2.Subtract(ref newvector, ref destVector, out newvector);
				}

				destination[i] = newvector;
			}
		}

		/// <summary>
		/// Function to orthonormalize a list of vectors.
		/// </summary>
		/// <param name="destination">The orthonormalized vectors.</param>
		/// <param name="source">The vectors to orthonormalize.</param>
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
		public static void Orthonormalize(IList<GorgonVector2> destination, IList<GorgonVector2> source)
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
				GorgonVector2 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					float dot = 0.0f;
					GorgonVector2 destVector = destination[r];

					GorgonVector2.DotProduct(ref destVector, ref newvector, out dot);
					GorgonVector2.Multiply(ref destVector, dot, out destVector);
					GorgonVector2.Subtract(newvector, destVector);
				}

				newvector.Normalize();
				destination[i] = newvector;
			}
		}
				
		/// <summary>
		/// Function to calculate a perpendicular vector from the vector passed in.
		/// </summary>
		/// <param name="value">Vector used to calculate the perpendicular..</param>
		/// <param name="result">The perpendicular vector.</param>
		/// <remarks>This method uses a 90 degree counterclockwise rotation.</remarks>
		public static void Perpendicular(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(-value.Y, value.X);
		}

		/// <summary>
		/// Function to calculate a perpendicular vector from the vector passed in.
		/// </summary>
		/// <param name="value">Vector used to calculate the perpendicular..</param>
		/// <returns>The perpendicular vector.</returns>
		/// <remarks>This method uses a 90 degree counterclockwise rotation.</remarks>
		public static GorgonVector2 Perpendicular(GorgonVector2 value)
		{
			return new GorgonVector2(-value.Y, value.X);
		}

		/// <summary>
		/// Function to calculate a perpendicular dot product.
		/// </summary>
		/// <param name="first">First vector.</param>
		/// <param name="second">Second vector.</param>
		/// <param name="result">The perpendicular dot product of the vectors.</param>
		/// <remarks>The perpendicular dot product is defined as taking the dot product of the perpendicular vector 
		/// of the left vector with the right vector.</remarks>
		public static void PerpendicularDotProduct(ref GorgonVector2 first, ref GorgonVector2 second, out float result)
		{
			GorgonVector2 temp = GorgonVector2.Zero;
			
			Perpendicular(ref first, out temp);
			DotProduct(ref temp, ref second, out result);
		}

		/// <summary>
		/// Function to calculate a perpendicular dot product.
		/// </summary>
		/// <param name="first">First vector.</param>
		/// <param name="second">Second vector.</param>
		/// <returns>The perpendicular dot product of the vectors.</returns>
		/// <remarks>The perpendicular dot product is defined as taking the dot product of the perpendicular vector 
		/// of the left vector with the right vector.</remarks>
		public static float PerpendicularDotProduct(GorgonVector2 first, GorgonVector2 second)
		{
			float result = 0.0f;
			GorgonVector2 temp = GorgonVector2.Zero;

			Perpendicular(ref first, out temp);
			DotProduct(ref temp, ref second, out result);

			return result;
		}

		/// <summary>
		/// Function to get the reciprocal of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <param name="result">The reciprocal of the vector.</param>
		public static void Reciprocal(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(1.0f / value.X, 1.0f / value.Y);
		}

		/// <summary>
		/// Function to get the reciprocal of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <returns>The reciprocal of the vector.</returns>
		public static GorgonVector2 Reciprocal(GorgonVector2 value)
		{
			return new GorgonVector2(1.0f / value.X, 1.0f / value.Y);
		}

		/// <summary>
		/// Function to find the square root of each element in the vector and then takes the reciprocal of each.
		/// </summary>
		/// <param name="value">The vector used to find the square root/recpirocal.</param>
		/// <param name="result">The square root and reciprocal of the vector.</param>
		public static void ReciprocalSqrt(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(1.0f / GorgonMathUtility.Sqrt(value.X), 1.0f / GorgonMathUtility.Sqrt(value.Y));
		}

		/// <summary>
		/// Function to find the square root of each element in the vector and then takes the reciprocal of each.
		/// </summary>
		/// <param name="value">The vector used to find the square root/recpirocal.</param>
		/// <returns>The square root and reciprocal of the vector.</returns>
		public static GorgonVector2 ReciprocalSqrt(GorgonVector2 value)
		{			
			return new GorgonVector2(1.0f / GorgonMathUtility.Sqrt(value.X), 1.0f / GorgonMathUtility.Sqrt(value.Y));
		}

		/// <summary>
		/// Function to return the reflection vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector to reflect.</param>
		/// <param name="normal">Normal on the surface.</param>
		/// <param name="result">The reflected vector.</param>
		/// <remarks>This only gives the direction of a reflection off a surface, it does not determine whether the original vector was close enough to the surface to hit it.</remarks>
		public static void Reflect(ref GorgonVector2 vector, ref GorgonVector2 normal, out GorgonVector2 result)
		{
			float dot = (vector.X * normal.X) + (vector.Y * normal.Y);
			result = new GorgonVector2(vector.X - ((2.0f * dot) * normal.X), vector.Y - ((2.0f * dot) * normal.Y));
		}

		/// <summary>
		/// Function to return the reflection vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector to reflect.</param>
		/// <param name="normal">Normal to the surface.</param>
		/// <returns>The reflected vector.</returns>
		/// <remarks>This only gives the direction of a reflection off a surface, it does not determine whether the original vector was close enough to the surface to hit it.</remarks>
		public static GorgonVector2 Reflect(GorgonVector2 vector, GorgonVector2 normal)
		{
			float dot = (vector.X * normal.X) + (vector.Y * normal.Y);
			return new GorgonVector2(vector.X - ((2.0f * dot) * normal.X), vector.Y - ((2.0f * dot) * normal.Y));
		}

		/// <summary>
		/// Function to return the refraction vector off a surface that has the specified normal and refraction index.
		/// </summary>
		/// <param name="vector">The source vector to refract.</param>
		/// <param name="normal">Normal to the surface.</param>
		/// <param name="index">Index of refraction.</param>
		/// <param name="result">The refracted vector.</param>
		public static void Refract(ref GorgonVector2 vector, ref GorgonVector2 normal, float index, out GorgonVector2 result)
		{
			float cos1;

			DotProduct(ref vector, ref normal, out cos1);

			float radicand = 1.0f - (index * index) * (1.0f - (cos1 * cos1));

			if (radicand < 0.0f)
				result = GorgonVector2.Zero;

			float cos2 = GorgonMathUtility.Sqrt(radicand);
			GorgonVector2 angleNormal = GorgonVector2.Zero;
			GorgonVector2 indexVector = GorgonVector2.Zero;

			Multiply(ref vector, index, out indexVector);
			Multiply(ref normal, cos2 - index * cos1, out angleNormal);
			Add(ref indexVector, ref angleNormal, out result);
		}

		/// <summary>
		/// Function to return the refraction vector off a surface that has the specified normal and refraction index.
		/// </summary>
		/// <param name="vector">The source vector to refract.</param>
		/// <param name="normal">Normal to the surface.</param>
		/// <param name="index">Index of refraction.</param>
		/// <returns>The refracted vector.</returns>
		public static GorgonVector2 Refract(GorgonVector2 vector, GorgonVector2 normal, float index)
		{
			float cos1;

			DotProduct(ref vector, ref normal, out cos1);

			float radicand = 1.0f - (index * index) * (1.0f - (cos1 * cos1));

			if (radicand < 0.0f)
				return GorgonVector2.Zero;

			float cos2 = GorgonMathUtility.Sqrt(radicand);
			GorgonVector2 angleNormal = GorgonVector2.Zero;
			GorgonVector2 indexVector = GorgonVector2.Zero;				

			Multiply(ref vector, index, out indexVector);
			Multiply(ref normal, cos2 - index * cos1, out angleNormal);
			Add(ref indexVector, ref angleNormal, out indexVector);

			return indexVector;
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <param name="result">Rounded vector.</param>
		public static void Round(ref GorgonVector2 vector, out GorgonVector2 result)
		{
			result = new GorgonVector2(GorgonMathUtility.RoundInt(vector.X), GorgonMathUtility.RoundInt(vector.Y));
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <returns>Rounded vector.</returns>
		public static GorgonVector2 Round(GorgonVector2 vector)
		{
			return new GorgonVector2(GorgonMathUtility.RoundInt(vector.X), GorgonMathUtility.RoundInt(vector.Y));
		}

		/// <summary>
		/// Function to find the sine of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the sine of.</param>
		/// <param name="result">The sine of each component.</param>
		public static void Sin(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(GorgonMathUtility.Sin(value.X), GorgonMathUtility.Sin(value.Y));
		}

		/// <summary>
		/// Function to find the sine of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the sine of.</param>
		/// <returns>The sine of each component.</returns>
		public static GorgonVector2 Sin(GorgonVector2 value)
		{
			return new GorgonVector2(GorgonMathUtility.Sin(value.X), GorgonMathUtility.Sin(value.Y));
		}

		/// <summary>
		/// Function to interpolate between the start and end points given a weight using cubic interpolation.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="weight">Weight of the end vector.</param>
		/// <param name="result">The interpolated vector.</param>
		/// <remarks>The <paramref name="weight"/> must be between 0.0 and 1.0.</remarks>
		public static void SmoothStep(ref GorgonVector2 start, ref GorgonVector2 end, float weight, out GorgonVector2 result)
		{
			weight = (weight > 1.0f) ? 1.0f : ((weight < 0.0f) ? 0.0f : weight);
			weight = (weight * weight) * (3.0f - (2.0f * weight));

			result = new GorgonVector2(
				start.X + ((end.X - start.X) * weight),
				start.Y + ((end.Y - start.Y) * weight)
			);
		}

		/// <summary>
		/// Function to interpolate between the start and end points given a weight using cubic interpolation.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="weight">Weight of the end vector.</param>
		/// <returns>The interpolated vector.</returns>
		/// <remarks>The <paramref name="weight"/> must be between 0.0 and 1.0.</remarks>
		public static GorgonVector2 SmoothStep(GorgonVector2 start, GorgonVector2 end, float weight)
		{
			weight = (weight > 1.0f) ? 1.0f : ((weight < 0.0f) ? 0.0f : weight);
			weight = (weight * weight) * (3.0f - (2.0f * weight));

			return new GorgonVector2(start.X + ((end.X - start.X) * weight),start.Y + ((end.Y - start.Y) * weight));
		}

		/// <summary>
		/// Function to subtract two vectors from each other.
		/// </summary>
		/// <param name="left">Left vector to subtract.</param>
		/// <param name="right">Right vector to subtract.</param>
		/// <param name="result">The difference between the two vectors.</param>
		public static void Subtract(ref GorgonVector2 left, ref GorgonVector2 right, out GorgonVector2 result)
		{
			result = new GorgonVector2(left.X - right.X, left.Y - right.Y);
		}

		/// <summary>
		/// Function to subtract two vectors from each other.
		/// </summary>
		/// <param name="left">Left vector to subtract.</param>
		/// <param name="right">Right vector to subtract.</param>
		/// <returns>The difference between the two vectors.</returns>
		public static GorgonVector2 Subtract(GorgonVector2 left, GorgonVector2 right)
		{
			return new GorgonVector2(left.X - right.X, left.Y - right.Y);
		}

		/// <summary>
		/// Function to calculate the tangent of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <param name="result">The tangent of each element.</param>
		public static void Tan(ref GorgonVector2 value, out GorgonVector2 result)
		{
			result = new GorgonVector2(
				GorgonMathUtility.Tan(value.X),
				GorgonMathUtility.Tan(value.Y)
			);
		}

		/// <summary>
		/// Function to calculate the tangent of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <returns>The tangent of each element.</returns>
		public static GorgonVector2 Tan(GorgonVector2 value)
		{
			return new GorgonVector2(
				GorgonMathUtility.Tan(value.X),
				GorgonMathUtility.Tan(value.Y)
			);
		}

		/// <summary>
		/// Function to convert the vector into a 2 element array.
		/// </summary>
		/// <returns>A 2 element array containing the elements of the vector.</returns>
		/// <remarks>X is in the first element, Y is in the second element.</remarks>
		public float[] ToArray()
		{
			return new[] { X, Y };
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <param name="provider">String formatting provider.</param>
		/// <returns>A string containing the type and values of this object.</returns>
		public string ToString(IFormatProvider provider)
		{
			return string.Format(provider, "X:{0} Y:{1}", X, Y);
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

			return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
		}

#warning Don't forget to implement these after Matrix and Quaternion are implemented (Transform/Coordinate/Normal).
		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="transform">Matrix used to transform the vector.</param>
		/// <param name="vector">Vector to transform.</param>
		/// <param name="result">The resulting transformed <see cref="GorgonLibrary.Math.GorgonVector4">GorgonVector4</see> vector.</param>
		public static void Transform(ref GorgonVector2 vector, ref GorgonMatrix transform, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				(vector.X * transform.m11) + (vector.Y * transform.m12) + transform.m14,
				(vector.X * transform.m21) + (vector.Y * transform.m22) + transform.m24,
				(vector.X * transform.m31) + (vector.Y * transform.m32) + transform.m34,
				(vector.X * transform.m41) + (vector.Y * transform.m42) + transform.m44);
		}


		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="transform">Matrix used to transform the vector.</param>
		/// <param name="vector">Vector to transform.</param>
		/// <returns>The resulting transformed <see cref="GorgonLibrary.Math.GorgonVector4">GorgonVector4</see> vector.</returns>
		public static GorgonVector4 Transform(GorgonVector2 vector, GorgonMatrix transform)
		{
			return new GorgonVector4(
				(vector.X * transform.m11) + (vector.Y * transform.m12) + transform.m14,
				(vector.X * transform.m21) + (vector.Y * transform.m22) + transform.m24,
				(vector.X * transform.m31) + (vector.Y * transform.m32) + transform.m34,
				(vector.X * transform.m41) + (vector.Y * transform.m42) + transform.m44);
		}

		/// <summary>
		/// Function to transform a 3D vector with a quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform.</param>
		/// <param name="vector">Vector to transform.</param>
		/// <param name="result">The transformed vector.</param>
		public static void Transform(ref GorgonQuaternion q, ref GorgonVector2 vector, out GorgonVector2 result)
		{
			float x = q.X + q.X;
			float y = q.Y + q.Y;
			float z = q.Z + q.Z;
			float wz = q.W * z;
			float xx = q.X * x;
			float xy = q.X * y;
			float yy = q.Y * y;
			float zz = q.Z * z;

			result = new GorgonVector2(
				((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))),
				((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz)))
			);
		}

		/// <summary>
		/// Function to transform a 3D vector with a quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform.</param>
		/// <param name="vector">Vector to transform.</param>
		/// <returns>The transformed vector.</returns>
		public static GorgonVector2 Transform(GorgonQuaternion q, GorgonVector2 vector)
		{
			float x = q.X + q.X;
			float y = q.Y + q.Y;
			float z = q.Z + q.Z;
			float wz = q.W * z;
			float xx = q.X * x;
			float xy = q.X * y;
			float yy = q.Y * y;
			float zz = q.Z * z;

			return new GorgonVector2(((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))), ((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz))));
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to perform equality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonVector2 left,GorgonVector2 right)
		{
			return (GorgonMathUtility.EqualFloat(left.X, right.X) && GorgonMathUtility.EqualFloat(left.Y, right.Y));
		}

		/// <summary>
		/// Operator to perform inequality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(GorgonVector2 left,GorgonVector2 right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector2 operator +(GorgonVector2 left, GorgonVector2 right)
		{
			GorgonVector2 result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector2 operator -(GorgonVector2 left, GorgonVector2 right)
		{
			GorgonVector2 result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to negate a vector.
		/// </summary>
		/// <param name="left">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static GorgonVector2 operator -(GorgonVector2 left)
		{
			GorgonVector2 result;
			Negate(ref left, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector2 operator *(GorgonVector2 left, float scalar)
		{
			GorgonVector2 result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector2 operator *(float scalar, GorgonVector2 right)
		{
			GorgonVector2 result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector2 operator /(GorgonVector2 left, float scalar)
		{
			GorgonVector2 result;
			Divide(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to convert a System.Drawing.Point to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.Point to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator GorgonVector2(Point point)
		{
			return new GorgonVector2(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.PointF to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.PointF to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator GorgonVector2(PointF point)
		{
			return new GorgonVector2(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.Size to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.Size to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator GorgonVector2(Size point)
		{
			return new GorgonVector2(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.SizeF to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.SizeF to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator GorgonVector2(SizeF point)
		{
			return new GorgonVector2(point);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.Point.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static explicit operator Point(GorgonVector2 vector)
		{
			return new Point((int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.PointF.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static implicit operator PointF(GorgonVector2 vector)
		{
			return new PointF(vector.X, vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.Size.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static explicit operator Size(GorgonVector2 vector)
		{
			return new Size((int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.SizeF.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static implicit operator SizeF(GorgonVector2 vector)
		{
			return new SizeF(vector.X, vector.Y);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector2"/> struct.
		/// </summary>
		/// <param name="x">Horizontal position of the vector.</param>
		/// <param name="y">Vertical posiition of the vector.</param>
		public GorgonVector2(float x,float y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector2"/> struct.
		/// </summary>
		/// <param name="value">The value used to initialize the vector.</param>
		public GorgonVector2(float value)
		{
			Y = X = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector2"/> struct.
		/// </summary>
		/// <param name="point">System.Drawing.PointF to initialize with.</param>
		public GorgonVector2(PointF point)
		{
			X = point.X;
			Y = point.Y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector2"/> struct.
		/// </summary>
		/// <param name="point">System.Drawing.Point to initialize with.</param>
		public GorgonVector2(Point point)
		{
			X = point.X;
			Y = point.Y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector2"/> struct.
		/// </summary>
		/// <param name="size">System.Drawing.Size to initialize with.</param>
		public GorgonVector2(Size size)
		{
			X = size.Width;
			Y = size.Height;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector2"/> struct.
		/// </summary>
		/// <param name="size">System.Drawing.SizeF to initialize with.</param>
		public GorgonVector2(SizeF size)
		{
			X = size.Width;
			Y = size.Height;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector2"/> struct.
		/// </summary>
		/// <param name="values">A list of values.</param>
		/// <remarks>Only the first two elements in the list will be taken.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="values"/> parameter has less than 2 elements.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the values parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonVector2(IList<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Count < 2)
				throw new ArgumentException("The number of elements must be at least 2 for a 2D vector.", "values");

			X = values[0];
			Y = values[1];
		}
		#endregion

		#region IEquatable<GorgonVector2> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonVector2 other)
		{
			return (X == other.X) && (Y == other.Y);
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

			return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
		}
		#endregion
	}
}
