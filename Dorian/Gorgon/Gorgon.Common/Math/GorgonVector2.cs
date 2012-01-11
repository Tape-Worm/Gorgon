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

using System;
using System.ComponentModel;
using System.Drawing;
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
	[Serializable(), StructLayout(LayoutKind.Sequential, Pack = 4), TypeConverter(typeof(Design.Vector2DTypeConverter))]
	public struct GorgonVector2
		: IEquatable<GorgonVector2>
	{
		#region Variables.
		/// <summary>
		/// Empty vector.
		/// </summary>
		public readonly static GorgonVector2 Zero = new GorgonVector2(0);
		/// <summary>
		/// Unit vector.
		/// </summary>
		public readonly static GorgonVector2 Unit = new GorgonVector2(1.0f);
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
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the lowest values of the two vectors.</param>
		public static void Floor(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out GorgonVector2 result)
		{
			result.X = (vector1.X < vector2.X) ? vector1.X : vector2.X;
			result.Y = (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y;
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the lowest values of the two vectors.</returns>
		public static GorgonVector2 Floor(GorgonVector2 vector1, GorgonVector2 vector2)
		{
			return new GorgonVector2((vector1.X < vector2.X) ? vector1.X : vector2.X, (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y);
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the highest values of the two vectors.</param>
		public static void Ceiling(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out GorgonVector2 result)
		{
			result.X = (vector1.X > vector2.X) ? vector1.X : vector2.X;
			result.Y = (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y;
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the highest values of the two vectors.</returns>
		public static GorgonVector2 Ceiling(GorgonVector2 vector1, GorgonVector2 vector2)
		{
			return new GorgonVector2((vector1.X > vector2.X) ? vector1.X : vector2.X, (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y);
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
			result.X = (vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X));
			result.Y = (vertex1Coordinate.Y + (vertex2Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y));
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
			return new GorgonVector2((vertex1Coordinate.X + (vertex2Weight * (vertex2Coordinate.X - vertex1Coordinate.X))) + (vertex3Weight * (vertex3Coordinate.X - vertex1Coordinate.X)),
				(vertex1Coordinate.Y + (vertex2Weight * (vertex2Coordinate.Y - vertex1Coordinate.Y))) + (vertex3Weight * (vertex3Coordinate.Y - vertex1Coordinate.Y)));
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
		public static void CatmullRom(ref GorgonVector2 value1, ref GorgonVector2 value2, ref GorgonVector2 value3, ref GorgonVector2 value4, float amount, out GorgonVector2 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			result.X = 0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) + 
				(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) + 
				((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed));

			result.Y = 0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + 
				(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) + 
				((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed));
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
		public static GorgonVector2 CatmullRom(GorgonVector2 value1, GorgonVector2 value2, GorgonVector2 value3, GorgonVector2 value4, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			return new GorgonVector2(0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
									(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) +
									((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
								0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
									(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) +
									((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed)));
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
		public static void Hermite(ref GorgonVector2 value1, ref GorgonVector2 tangent1, ref GorgonVector2 value2, ref GorgonVector2 tangent2, float amount, out GorgonVector2 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			result.X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4);
			result.Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
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
		public static GorgonVector2 Hermite(GorgonVector2 value1, GorgonVector2 tangent1, GorgonVector2 value2, GorgonVector2 tangent2, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			return new GorgonVector2((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
								(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4));
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
		public static void Lerp(ref GorgonVector2 start, ref GorgonVector2 end, float amount, out GorgonVector2 result)
		{
			result.X = start.X + ((end.X - start.X) * amount);
			result.Y = start.Y + ((end.Y - start.Y) * amount);
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
		public static GorgonVector2 Lerp(GorgonVector2 start, GorgonVector2 end, float amount)
		{
			return new GorgonVector2(start.X + ((end.X - start.X) * amount), start.Y + ((end.Y - start.Y) * amount));
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
		/// <returns>The squared distance between the vectors.</returns>
		public static float DistanceSquared(GorgonVector2 vector1, GorgonVector2 vector2)
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
		public static void Distance(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out float result)
		{
			float x = vector1.X - vector2.X;
			float y = vector1.Y - vector2.Y;

			result = GorgonMathUtility.Sqrt((x * x) + (y * y));
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

			result.X = start.X + ((end.X - start.X) * weight);
			result.Y = start.Y + ((end.Y - start.Y) * weight);
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
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <param name="result">Rounded vector.</param>
		public static void Round(ref GorgonVector2 vector, out GorgonVector2 result)
		{
			result.X = GorgonMathUtility.RoundInt(vector.X);
			result.Y = GorgonMathUtility.RoundInt(vector.Y);
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
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <param name="result">The negated vector.</param>
		public static void Negate(ref GorgonVector2 vector, out GorgonVector2 result)
		{
			result.X = -vector.X;
			result.Y = -vector.Y;
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
		/// Function to add two vectors together.
		/// </summary>
		/// <param name="left">Left vector to add.</param>
		/// <param name="right">Right vector to add.</param>
		/// <param name="result">The combined vectors.</param>
		public static void Add(ref GorgonVector2 left, ref GorgonVector2 right, out GorgonVector2 result)
		{
			result.X = left.X + right.X;
			result.Y = left.Y + right.Y;
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
		/// Function to subtract two vectors from each other.
		/// </summary>
		/// <param name="left">Left vector to subtract.</param>
		/// <param name="right">Right vector to subtract.</param>
		/// <param name="result">The difference between the two vectors.</param>
		public static void Subtract(ref GorgonVector2 left, ref GorgonVector2 right, out GorgonVector2 result)
		{
			result.X = left.X - right.X;
			result.Y = left.Y - right.Y;
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
		/// Function to divide a vector by another vector.
		/// </summary>
		/// <param name="dividend">The vector to be divided.</param>
		/// <param name="divisor">The vector that will divide the dividend.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static void Divide(ref GorgonVector2 dividend, ref GorgonVector2 divisor, out GorgonVector2 result)
		{
			result.X = dividend.X / divisor.X;
			result.Y = dividend.Y / divisor.Y;
		}

		/// <summary>
		/// Function to divide a vector by another vector.
		/// </summary>
		/// <param name="dividend">The vector to be divided.</param>
		/// <param name="divisor">The vector that will divide the dividend.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
		public static GorgonVector2 Divide(GorgonVector2 dividend, GorgonVector2 divisor)
		{
			return new GorgonVector2(dividend.X / divisor.X, dividend.Y / divisor.Y);
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
			result.X = dividend.X / divisor;
			result.Y = dividend.Y / divisor;
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
		/// Function to multiply two vectors together.
		/// </summary>
		/// <param name="left">Left vector to multiply.</param>
		/// <param name="right">Right vector to multiply.</param>
		/// <param name="result">The product of the two vectors.</param>
		public static void Multiply(ref GorgonVector2 left, ref GorgonVector2 right, out GorgonVector2 result)
		{
			result.X = left.X * right.X;
			result.Y = left.Y * right.Y;
		}

		/// <summary>
		/// Function to multiply two vectors together.
		/// </summary>
		/// <param name="left">Left vector to multiply.</param>
		/// <param name="right">Right vector to multiply.</param>
		/// <returns>The product of the two vectors.</returns>
		public static GorgonVector2 Multiply(GorgonVector2 left, GorgonVector2 right)
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
			result.X = left.X * scalar;
			result.Y = left.Y * scalar;
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
		/// Function to normalize this vector.
		/// </summary>
		public void Normalize()
		{
			float length = Length;

			if (!GorgonMathUtility.EqualFloat(length, 0.0f, 0.000001f))
				return;

			float invLen = 1.0f / length;

			X *= invLen;
			Y *= invLen;
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="vector1">Vector to perform the cross product with.</param>
		/// <param name="vector2">Vector to perform the cross product with.</param>
		/// <param name="result">A new vector containing the cross product.</param>
		public static void CrossProduct(ref GorgonVector2 vector1, ref GorgonVector2 vector2, out GorgonVector2 result)
		{
			result.X = vector2.X * vector1.Y - vector2.Y * vector1.X;
			result.Y = vector2.Y * vector1.X - vector2.X * vector1.Y;
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="vector1">Vector to perform the cross product with.</param>
		/// <param name="vector2">Vector to perform the cross product with.</param>
		/// <returns>A new vector containing the cross product.</returns>
		public static GorgonVector2 CrossProduct(GorgonVector2 vector1, GorgonVector2 vector2)
		{
			return new GorgonVector2(vector2.X * vector1.Y - vector2.Y * vector1.X, vector2.Y * vector1.X - vector2.X * vector1.Y);
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
			GorgonVector2 result = GorgonVector2.Zero;

			Clamp(ref value, ref min, ref max, out result);

			return result;
		}

		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <param name="result">The resulting transformed vector.</param>
		public static void Transform(ref GorgonMatrix m, ref GorgonVector2 vec, out GorgonVector2 result)
		{
			float scale = 1.0f / (m.m41 * vec.X) + (m.m42 * vec.Y) + m.m44;

			result.X = ((m.m11 * vec.X) + (m.m12 * vec.Y) + m.m14) * scale;
			result.Y = ((m.m21 * vec.X) + (m.m22 * vec.Y) + m.m24) * scale;
		}


		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <returns>The resulting transformed vector.</returns>
		public static GorgonVector2 Transform(GorgonMatrix m, GorgonVector2 vec)
		{
			float scale = 1.0f / (m.m41 * vec.X) + (m.m42 * vec.Y) + m.m44;
			return new GorgonVector2(((m.m11 * vec.X) + (m.m12 * vec.Y) + m.m14) * scale, ((m.m21 * vec.X) + (m.m22 * vec.Y) + m.m24) * scale);
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

			result.X = ((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz)));
			result.Y = ((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz)));
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

		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			IEquatable<GorgonVector2> equate = obj as IEquatable<GorgonVector2>;

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
		public static bool Equals(ref GorgonVector2 vector1, ref GorgonVector2 vector2)
		{
			return GorgonMathUtility.EqualFloat(vector1.X, vector2.X) && GorgonMathUtility.EqualFloat(vector1.Y, vector2.Y);
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
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format("2D Vector-> X:{0}, Y:{1}",X,Y);
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
		/// Operator to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector2 operator *(GorgonVector2 left, GorgonVector2 right)
		{
			GorgonVector2 result;
			Multiply(ref left, ref right, out result);
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
		/// Operator to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector2 operator /(GorgonVector2 left, GorgonVector2 right)
		{
			GorgonVector2 result;
			Divide(ref left, ref right, out result);
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
		#endregion

		#region IEquatable<Vector2D> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonVector2 other)
		{
			return (GorgonMathUtility.EqualFloat(other.X, X) && GorgonMathUtility.EqualFloat(other.Y, Y));
		}
		#endregion
	}
}
