﻿/*
* Copyright (c) 2007-2010 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

// TW - 01162012 - Updated to add overloads that I wanted from Gorgon (such as System.Drawing constructors and conversions).

using System;
using System.Collections.Generic;			// TW - Used for IList instead of an array, more general that way.
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace SlimMath
{
	/// <summary>
	/// Represents a two dimensional mathematical vector.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[TypeConverter(typeof(SlimMath.Design.Vector2Converter))]
	public struct Vector2 : IEquatable<Vector2>, IFormattable
	{
		/// <summary>
		/// The size of the <see cref="SlimMath.Vector2"/> type, in bytes.
		/// </summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2));

		/// <summary>
		/// A <see cref="SlimMath.Vector2"/> with all of its components set to zero.
		/// </summary>
		public static readonly Vector2 Zero = new Vector2();

		/// <summary>
		/// The X unit <see cref="SlimMath.Vector2"/> (1, 0).
		/// </summary>
		public static readonly Vector2 UnitX = new Vector2(1.0f, 0.0f);

		/// <summary>
		/// The Y unit <see cref="SlimMath.Vector2"/> (0, 1).
		/// </summary>
		public static readonly Vector2 UnitY = new Vector2(0.0f, 1.0f);

		/// <summary>
		/// A <see cref="SlimMath.Vector2"/> with all of its components set to one.
		/// </summary>
		public static readonly Vector2 One = new Vector2(1.0f, 1.0f);

		/// <summary>
		/// The X component of the vector.
		/// </summary>
		public float X;

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		public float Y;

		/// <summary>
		/// Initializes a new instance of the <see cref="SlimMath.Vector2"/> struct.
		/// </summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Vector2(float value)
		{
			X = value;
			Y = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SlimMath.Vector2"/> struct.
		/// </summary>
		/// <param name="x">Initial value for the X component of the vector.</param>
		/// <param name="y">Initial value for the Y component of the vector.</param>
		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SlimMath.Vector2"/> struct.
		/// </summary>
		/// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with two elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
		public Vector2(IList<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Count < 2)
				throw new ArgumentOutOfRangeException("values", "There must be two and only two input values for Vector2.");

			X = values[0];
			Y = values[1];
		}

		#region For Gorgon.
		/// <summary>
		/// Function to round the vector elements to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <param name="result">Rounded vector.</param>
		public static void Round(ref Vector2 vector, out Vector2 result)
		{
			result = new Vector2(Utilities.RoundInt(vector.X), Utilities.RoundInt(vector.Y));
		}

		/// <summary>
		/// Function to round the vector elements to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <returns>Rounded vector.</returns>
		public static Vector2 Round(Vector2 vector)
		{
			return new Vector2(Utilities.RoundInt(vector.X), Utilities.RoundInt(vector.Y));
		}

		/// <summary>
		/// Operator to convert a System.Drawing.System.Drawing.Point to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.System.Drawing.Point to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2(System.Drawing.Point point)
		{
			return new Vector2(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.System.Drawing.PointF to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.System.Drawing.PointF to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2(System.Drawing.PointF point)
		{
			return new Vector2(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.System.Drawing.Size to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.System.Drawing.Size to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2(System.Drawing.Size point)
		{
			return new Vector2(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.System.Drawing.SizeF to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.System.Drawing.SizeF to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2(System.Drawing.SizeF point)
		{
			return new Vector2(point);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.System.Drawing.Point.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static explicit operator System.Drawing.Point(Vector2 vector)
		{
			return new System.Drawing.Point((int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.System.Drawing.PointF.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static implicit operator System.Drawing.PointF(Vector2 vector)
		{
			return new System.Drawing.PointF(vector.X, vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.System.Drawing.Size.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static explicit operator System.Drawing.Size(Vector2 vector)
		{
			return new System.Drawing.Size((int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.System.Drawing.SizeF.
		/// </summary>
		/// <param name="vector">2D Gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static implicit operator System.Drawing.SizeF(Vector2 vector)
		{
			return new System.Drawing.SizeF(vector.X, vector.Y);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector2"/> struct.
		/// </summary>
		/// <param name="point">A System.Drawing point.</param>
		public Vector2(System.Drawing.PointF point)
		{
			X = point.X;
			Y = point.Y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector2"/> struct.
		/// </summary>
		/// <param name="point">A System.Drawing point.</param>
		public Vector2(System.Drawing.Point point)
		{
			X = point.X;
			Y = point.Y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector2"/> struct.
		/// </summary>
		/// <param name="size">A System.Drawing size.</param>
		public Vector2(System.Drawing.SizeF size)
		{
			X = size.Width;
			Y = size.Height;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector2"/> struct.
		/// </summary>
		/// <param name="point">A System.Drawing point.</param>
		public Vector2(System.Drawing.Size point)
		{
			X = point.Width;
			Y = point.Height;
		}
		#endregion

		/// <summary>
		/// Gets a value indicting whether this instance is normalized.
		/// </summary>
		public bool IsNormalized
		{
			get { return Math.Abs((X * X) + (Y * Y) - 1f) < Utilities.ZeroTolerance; }
		}

		/// <summary>
		/// Calculates the length of the vector.
		/// </summary>
		/// <remarks>
		/// <see cref="SlimMath.Vector2.LengthSquared"/> may be preferred when only the relative length is needed
		/// and speed is of the essence.
		/// </remarks>
		public float Length
		{
			get { return (float)Math.Sqrt((X * X) + (Y * Y)); }
		}

		/// <summary>
		/// Calculates the squared length of the vector.
		/// </summary>
		/// <remarks>
		/// This property may be preferred to <see cref="SlimMath.Vector2.Length"/> when only a relative length is needed
		/// and speed is of the essence.
		/// </remarks>
		public float LengthSquared
		{
			get { return (X * X) + (Y * Y); }
		}

		/// <summary>
		/// Gets or sets the component at the specified index.
		/// </summary>
		/// <value>The value of the X or Y component, depending on the index.</value>
		/// <param name="index">The index of the component to access. Use 0 for the X component and 1 for the Y component.</param>
		/// <returns>The value of the component at the specified index.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>
		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return X;
					case 1: return Y;
				}

				throw new ArgumentOutOfRangeException("index", "Indices for Vector2 run from 0 to 1, inclusive.");
			}

			set
			{
				switch (index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					default: throw new ArgumentOutOfRangeException("index", "Indices for Vector2 run from 0 to 1, inclusive.");
				}
			}
		}

		/// <summary>
		/// Converts the vector into a unit vector.
		/// </summary>
		public void Normalize()
		{
			float length = Length;
			if (length > Utilities.ZeroTolerance)
			{
				float inv = 1.0f / length;
				X *= inv;
				Y *= inv;
			}
		}

		/// <summary>
		/// Reverses the direction of a given vector.
		/// </summary>
		public void Negate()
		{
			X = -X;
			Y = -Y;
		}

		/// <summary>
		/// Takes the absolute value of each component.
		/// </summary>
		public void Abs()
		{
			this.X = Math.Abs(X);
			this.Y = Math.Abs(Y);
		}

		/// <summary>
		/// Creates an array containing the elements of the vector.
		/// </summary>
		/// <returns>A two-element array containing the components of the vector.</returns>
		public float[] ToArray()
		{
			return new float[] { X, Y };
		}

		#region Transcendentals
		/// <summary>
		/// Takes the square root of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root of.</param>
		/// <param name="result">When the method completes, contains a vector that is the square root of the input vector.</param>
		public static void Sqrt(ref Vector2 value, out Vector2 result)
		{
			result.X = (float)Math.Sqrt(value.X);
			result.Y = (float)Math.Sqrt(value.Y);
		}

		/// <summary>
		/// Takes the square root of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root of.</param>
		/// <returns>A vector that is the square root of the input vector.</returns>
		public static Vector2 Sqrt(Vector2 value)
		{
			Vector2 temp;
			Sqrt(ref value, out temp);
			return temp;
		}

		/// <summary>
		/// Takes the reciprocal of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <param name="result">When the method completes, contains a vector that is the reciprocal of the input vector.</param>
		public static void Reciprocal(ref Vector2 value, out Vector2 result)
		{
			result.X = 1.0f / value.X;
			result.Y = 1.0f / value.Y;
		}

		/// <summary>
		/// Takes the reciprocal of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <returns>A vector that is the reciprocal of the input vector.</returns>
		public static Vector2 Reciprocal(Vector2 value)
		{
			Vector2 temp;
			Reciprocal(ref value, out temp);
			return temp;
		}

		/// <summary>
		/// Takes the square root of each component in the vector and than takes the reciprocal of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root and recpirocal of.</param>
		/// <param name="result">When the method completes, contains a vector that is the square root and reciprocal of the input vector.</param>
		public static void ReciprocalSqrt(ref Vector2 value, out Vector2 result)
		{
			result.X = 1.0f / (float)Math.Sqrt(value.X);
			result.Y = 1.0f / (float)Math.Sqrt(value.Y);
		}

		/// <summary>
		/// Takes the square root of each component in the vector and than takes the reciprocal of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root and recpirocal of.</param>
		/// <returns>A vector that is the square root and reciprocal of the input vector.</returns>
		public static Vector2 ReciprocalSqrt(Vector2 value)
		{
			Vector2 temp;
			ReciprocalSqrt(ref value, out temp);
			return temp;
		}

		/// <summary>
		/// Takes e raised to the component in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each component of.</param>
		/// <param name="result">When the method completes, contains a vector that has e raised to each of the components in the input vector.</param>
		public static void Exp(ref Vector2 value, out Vector2 result)
		{
			result.X = (float)Math.Exp(value.X);
			result.Y = (float)Math.Exp(value.Y);
		}

		/// <summary>
		/// Takes e raised to the component in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each component of.</param>
		/// <returns>A vector that has e raised to each of the components in the input vector.</returns>
		public static Vector2 Exp(Vector2 value)
		{
			Vector2 temp;
			Exp(ref value, out temp);
			return temp;
		}

		/// <summary>
		/// Takes the sine and than the cosine of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the sine and cosine of.</param>
		/// <param name="sinResult">When the method completes, contains the sine of each component in the input vector.</param>
		/// <param name="cosResult">When the method completes, contains the cpsome pf each component in the input vector.</param>
		public static void SinCos(ref Vector2 value, out Vector2 sinResult, out Vector2 cosResult)
		{
			sinResult.X = (float)Math.Sin(value.X);
			sinResult.Y = (float)Math.Sin(value.Y);

			cosResult.X = (float)Math.Cos(value.X);
			cosResult.Y = (float)Math.Cos(value.Y);
		}

		/// <summary>
		/// Takes the sine of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the sine of.</param>
		/// <param name="result">When the method completes, a vector that contains the sine of each component in the input vector.</param>
		public static void Sin(ref Vector2 value, out Vector2 result)
		{
			result.X = (float)Math.Sin(value.X);
			result.Y = (float)Math.Sin(value.Y);
		}

		/// <summary>
		/// Takes the sine of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the sine of.</param>
		/// <returns>A vector that contains the sine of each component in the input vector.</returns>
		public static Vector2 Sin(Vector2 value)
		{
			Vector2 temp;
			Sin(ref value, out temp);
			return temp;
		}

		/// <summary>
		/// Takes the cosine of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the cosine of.</param>
		/// <param name="result">When the method completes, contains a vector that contains the cosine of each component in the input vector.</param>
		public static void Cos(ref Vector2 value, out Vector2 result)
		{
			result.X = (float)Math.Cos(value.X);
			result.Y = (float)Math.Cos(value.Y);
		}

		/// <summary>
		/// Takes the cosine of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the cosine of.</param>
		/// <returns>A vector that contains the cosine of each component in the input vector.</returns>
		public static Vector2 Cos(Vector2 value)
		{
			Vector2 temp;
			Cos(ref value, out temp);
			return temp;
		}

		/// <summary>
		/// Takes the tangent of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <param name="result">When the method completes, contains a vector that contains the tangent of each component in the input vector.</param>
		public static void Tan(ref Vector2 value, out Vector2 result)
		{
			result.X = (float)Math.Tan(value.X);
			result.Y = (float)Math.Tan(value.Y);
		}

		/// <summary>
		/// Takes the tangent of each component in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <returns>A vector that contains the tangent of each component in the input vector.</returns>
		public static Vector2 Tan(Vector2 value)
		{
			Vector2 temp;
			Tan(ref value, out temp);
			return temp;
		}
		#endregion

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="left">The first vector to add.</param>
		/// <param name="right">The second vector to add.</param>
		/// <param name="result">When the method completes, contains the sum of the two vectors.</param>
		public static void Add(ref Vector2 left, ref Vector2 right, out Vector2 result)
		{
			result = new Vector2(left.X + right.X, left.Y + right.Y);
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="left">The first vector to add.</param>
		/// <param name="right">The second vector to add.</param>
		/// <returns>The sum of the two vectors.</returns>
		public static Vector2 Add(Vector2 left, Vector2 right)
		{
			return new Vector2(left.X + right.X, left.Y + right.Y);
		}

		/// <summary>
		/// Subtracts two vectors.
		/// </summary>
		/// <param name="left">The first vector to subtract.</param>
		/// <param name="right">The second vector to subtract.</param>
		/// <param name="result">When the method completes, contains the difference of the two vectors.</param>
		public static void Subtract(ref Vector2 left, ref Vector2 right, out Vector2 result)
		{
			result = new Vector2(left.X - right.X, left.Y - right.Y);
		}

		/// <summary>
		/// Subtracts two vectors.
		/// </summary>
		/// <param name="left">The first vector to subtract.</param>
		/// <param name="right">The second vector to subtract.</param>
		/// <returns>The difference of the two vectors.</returns>
		public static Vector2 Subtract(Vector2 left, Vector2 right)
		{
			return new Vector2(left.X - right.X, left.Y - right.Y);
		}

		/// <summary>
		/// Scales a vector by the given value.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <param name="scalar">The amount by which to scale the vector.</param>
		/// <param name="result">When the method completes, contains the scaled vector.</param>
		public static void Multiply(ref Vector2 value, float scalar, out Vector2 result)
		{
			result = new Vector2(value.X * scalar, value.Y * scalar);
		}

		/// <summary>
		/// Scales a vector by the given value.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <param name="scalar">The amount by which to scale the vector.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 Multiply(Vector2 value, float scalar)
		{
			return new Vector2(value.X * scalar, value.Y * scalar);
		}

		/// <summary>
		/// Modulates a vector with another by performing component-wise multiplication.
		/// </summary>
		/// <param name="left">The first vector to modulate.</param>
		/// <param name="right">The second vector to modulate.</param>
		/// <param name="result">When the method completes, contains the modulated vector.</param>
		public static void Modulate(ref Vector2 left, ref Vector2 right, out Vector2 result)
		{
			result = new Vector2(left.X * right.X, left.Y * right.Y);
		}

		/// <summary>
		/// Modulates a vector with another by performing component-wise multiplication.
		/// </summary>
		/// <param name="left">The first vector to modulate.</param>
		/// <param name="right">The second vector to modulate.</param>
		/// <returns>The modulated vector.</returns>
		public static Vector2 Modulate(Vector2 left, Vector2 right)
		{
			return new Vector2(left.X * right.X, left.Y * right.Y);
		}

		/// <summary>
		/// Scales a vector by the given value.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <param name="scalar">The amount by which to scale the vector.</param>
		/// <param name="result">When the method completes, contains the scaled vector.</param>
		public static void Divide(ref Vector2 value, float scalar, out Vector2 result)
		{
			result = new Vector2(value.X / scalar, value.Y / scalar);
		}

		/// <summary>
		/// Scales a vector by the given value.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <param name="scalar">The amount by which to scale the vector.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 Divide(Vector2 value, float scalar)
		{
			return new Vector2(value.X / scalar, value.Y / scalar);
		}

		/// <summary>
		/// Reverses the direction of a given vector.
		/// </summary>
		/// <param name="value">The vector to negate.</param>
		/// <param name="result">When the method completes, contains a vector facing in the opposite direction.</param>
		public static void Negate(ref Vector2 value, out Vector2 result)
		{
			result = new Vector2(-value.X, -value.Y);
		}

		/// <summary>
		/// Reverses the direction of a given vector.
		/// </summary>
		/// <param name="value">The vector to negate.</param>
		/// <returns>A vector facing in the opposite direction.</returns>
		public static Vector2 Negate(Vector2 value)
		{
			return new Vector2(-value.X, -value.Y);
		}

		/// <summary>
		/// Takes the absolute value of each component.
		/// </summary>
		/// <param name="value">The vector to take the absolute value of.</param>
		/// <param name="result">When the method completes, contains a vector that has all positive components.</param>
		public static void Abs(ref Vector2 value, out Vector2 result)
		{
			result = new Vector2(Math.Abs(value.X), Math.Abs(value.Y));
		}

		/// <summary>
		/// Takes the absolute value of each component.
		/// </summary>
		/// <param name="value">The vector to take the absolute value of.</param>
		/// <returns>A vector that has all positive components.</returns>
		public static Vector2 Abs(Vector2 value)
		{
			return new Vector2(Math.Abs(value.X), Math.Abs(value.Y));
		}

		/// <summary>
		/// Returns a <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
		/// </summary>
		/// <param name="value1">A <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="value2">A <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="value3">A <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
		/// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
		/// <param name="result">When the method completes, contains the 2D Cartesian coordinates of the specified point.</param>
		public static void Barycentric(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, float amount1, float amount2, out Vector2 result)
		{
			result = new Vector2((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X)),
				(value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y)));
		}

		/// <summary>
		/// Returns a <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
		/// </summary>
		/// <param name="value1">A <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="value2">A <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="value3">A <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
		/// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
		/// <returns>A new <see cref="SlimMath.Vector2"/> containing the 2D Cartesian coordinates of the specified point.</returns>
		public static Vector2 Barycentric(Vector2 value1, Vector2 value2, Vector2 value3, float amount1, float amount2)
		{
			Vector2 result;
			Barycentric(ref value1, ref value2, ref value3, amount1, amount2, out result);
			return result;
		}

		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		/// <param name="result">When the method completes, contains the clamped value.</param>
		public static void Clamp(ref Vector2 value, ref Vector2 min, ref Vector2 max, out Vector2 result)
		{
			float x = value.X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;

			float y = value.Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;

			result = new Vector2(x, y);
		}

		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		/// <returns>The clamped value.</returns>
		public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
		{
			Vector2 result;
			Clamp(ref value, ref min, ref max, out result);
			return result;
		}

		/// <summary>
		/// Calculates the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">When the method completes, contains the distance between the two vectors.</param>
		/// <remarks>
		/// <see cref="SlimMath.Vector2.DistanceSquared(ref Vector2, ref Vector2, out float)"/> may be preferred when only the relative distance is needed
		/// and speed is of the essence.
		/// </remarks>
		public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;

			result = (float)Math.Sqrt((x * x) + (y * y));
		}

		/// <summary>
		/// Calculates the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The distance between the two vectors.</returns>
		/// <remarks>
		/// <see cref="SlimMath.Vector2.DistanceSquared(Vector2, Vector2)"/> may be preferred when only the relative distance is needed
		/// and speed is of the essence.
		/// </remarks>
		public static float Distance(Vector2 value1, Vector2 value2)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;

			return (float)Math.Sqrt((x * x) + (y * y));
		}

		/// <summary>
		/// Calculates the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector</param>
		/// <param name="result">When the method completes, contains the squared distance between the two vectors.</param>
		/// <remarks>Distance squared is the value before taking the square root. 
		/// Distance squared can often be used in place of distance if relative comparisons are being made. 
		/// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
		/// compare the distance between A and B to the distance between A and C. Calculating the two distances 
		/// involves two square roots, which are computationally expensive. However, using distance squared 
		/// provides the same information and avoids calculating two square roots.
		/// </remarks>
		public static void DistanceSquared(ref Vector2 value1, ref Vector2 value2, out float result)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;

			result = (x * x) + (y * y);
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
		public static float DistanceSquared(Vector2 value1, Vector2 value2)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;

			return (x * x) + (y * y);
		}

		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="left">First source vector.</param>
		/// <param name="right">Second source vector.</param>
		/// <param name="result">When the method completes, contains the dot product of the two vectors.</param>
		public static void Dot(ref Vector2 left, ref Vector2 right, out float result)
		{
			result = (left.X * right.X) + (left.Y * right.Y);
		}

		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="left">First source vector.</param>
		/// <param name="right">Second source vector.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public static float Dot(Vector2 left, Vector2 right)
		{
			return (left.X * right.X) + (left.Y * right.Y);
		}

		/// <summary>
		/// Calculates a vector that is perpendicular to the given vector.
		/// </summary>
		/// <param name="value">The vector to base the perpendicular vector on.</param>
		/// <param name="result">When the method completes, contains the perpendicular vector.</param>
		/// <remarks>
		/// This method finds the perpendicular vector using a 90 degree counterclockwise rotation.
		/// </remarks>
		public static void Perp(ref Vector2 value, out Vector2 result)
		{
			result.X = -value.Y;
			result.Y = value.X;
		}

		/// <summary>
		/// Calculates a vector that is perpendicular to the given vector.
		/// </summary>
		/// <param name="value">The vector to base the perpendicular vector on.</param>
		/// <returns>The perpendicular vector.</returns>
		/// <remarks>
		/// This method finds the perpendicular vector using a 90 degree counterclockwise rotation.
		/// </remarks>
		public static Vector2 Perp(Vector2 value)
		{
			Vector2 result;
			Perp(ref value, out result);
			return result;
		}

		/// <summary>
		/// Calculates the perp dot product.
		/// </summary>
		/// <param name="left">First source vector.</param>
		/// <param name="right">Second source vector.</param>
		/// <param name="result">When the method completes, contains the perp dot product of the two vectors.</param>
		/// <remarks>
		/// The perp dot product is defined as taking the dot product of the perpendicular vector
		/// of the left vector with the right vector.
		/// </remarks>
		public static void PerpDot(ref Vector2 left, ref Vector2 right, out float result)
		{
			Vector2 temp;
			Perp(ref left, out temp);

			Dot(ref temp, ref right, out result);
		}

		/// <summary>
		/// Calculates the perp dot product.
		/// </summary>
		/// <param name="left">First source vector.</param>
		/// <param name="right">Second source vector.</param>
		/// <returns>The perp dot product of the two vectors.</returns>
		/// <remarks>
		/// The perp dot product is defined as taking the dot product of the perpendicular vector
		/// of the left vector with the right vector.
		/// </remarks>
		public static float PerpDot(Vector2 left, Vector2 right)
		{
			float result;
			PerpDot(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Converts the vector into a unit vector.
		/// </summary>
		/// <param name="value">The vector to normalize.</param>
		/// <param name="result">When the method completes, contains the normalized vector.</param>
		public static void Normalize(ref Vector2 value, out Vector2 result)
		{
			result = value;
			result.Normalize();
		}

		/// <summary>
		/// Converts the vector into a unit vector.
		/// </summary>
		/// <param name="value">The vector to normalize.</param>
		/// <returns>The normalized vector.</returns>
		public static Vector2 Normalize(Vector2 value)
		{
			value.Normalize();
			return value;
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
		public static void Lerp(ref Vector2 start, ref Vector2 end, float amount, out Vector2 result)
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
		/// <returns>The linear interpolation of the two vectors.</returns>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
		/// </remarks>
		public static Vector2 Lerp(Vector2 start, Vector2 end, float amount)
		{
			Vector2 result;
			Lerp(ref start, ref end, amount, out result);
			return result;
		}

		/// <summary>
		/// Performs a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">When the method completes, contains the cubic interpolation of the two vectors.</param>
		public static void SmoothStep(ref Vector2 start, ref Vector2 end, float amount, out Vector2 result)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));

			result.X = start.X + ((end.X - start.X) * amount);
			result.Y = start.Y + ((end.Y - start.Y) * amount);
		}

		/// <summary>
		/// Performs a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>The cubic interpolation of the two vectors.</returns>
		public static Vector2 SmoothStep(Vector2 start, Vector2 end, float amount)
		{
			Vector2 result;
			SmoothStep(ref start, ref end, amount, out result);
			return result;
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
		public static void Hermite(ref Vector2 value1, ref Vector2 tangent1, ref Vector2 value2, ref Vector2 tangent2, float amount, out Vector2 result)
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
		/// <returns>The result of the Hermite spline interpolation.</returns>
		public static Vector2 Hermite(Vector2 value1, Vector2 tangent1, Vector2 value2, Vector2 tangent2, float amount)
		{
			Vector2 result;
			Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
			return result;
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
		public static void CatmullRom(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, ref Vector2 value4, float amount, out Vector2 result)
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
		/// <returns>A vector that is the result of the Catmull-Rom interpolation.</returns>
		public static Vector2 CatmullRom(Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount)
		{
			Vector2 result;
			CatmullRom(ref value1, ref value2, ref value3, ref value4, amount, out result);
			return result;
		}

		/// <summary>
		/// Returns a vector containing the largest components of the specified vectors.
		/// </summary>
		/// <param name="value1">The first source vector.</param>
		/// <param name="value2">The second source vector.</param>
		/// <param name="result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
		public static void Max(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
		{
			result.X = (value1.X > value2.X) ? value1.X : value2.X;
			result.Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
		}

		/// <summary>
		/// Returns a vector containing the largest components of the specified vectors.
		/// </summary>
		/// <param name="value1">The first source vector.</param>
		/// <param name="value2">The second source vector.</param>
		/// <returns>A vector containing the largest components of the source vectors.</returns>
		public static Vector2 Max(Vector2 value1, Vector2 value2)
		{
			Vector2 result;
			Max(ref value1, ref value2, out result);
			return result;
		}

		/// <summary>
		/// Returns a vector containing the smallest components of the specified vectors.
		/// </summary>
		/// <param name="value1">The first source vector.</param>
		/// <param name="value2">The second source vector.</param>
		/// <param name="result">When the method completes, contains an new vector composed of the smallest components of the source vectors.</param>
		public static void Min(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
		{
			result.X = (value1.X < value2.X) ? value1.X : value2.X;
			result.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
		}

		/// <summary>
		/// Returns a vector containing the smallest components of the specified vectors.
		/// </summary>
		/// <param name="value1">The first source vector.</param>
		/// <param name="value2">The second source vector.</param>
		/// <returns>A vector containing the smallest components of the source vectors.</returns>
		public static Vector2 Min(Vector2 value1, Vector2 value2)
		{
			Vector2 result;
			Min(ref value1, ref value2, out result);
			return result;
		}

		/// <summary>
		/// Returns the reflection of a vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <param name="result">When the method completes, contains the reflected vector.</param>
		/// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
		/// whether the original vector was close enough to the surface to hit it.</remarks>
		public static void Reflect(ref Vector2 vector, ref Vector2 normal, out Vector2 result)
		{
			float dot = (vector.X * normal.X) + (vector.Y * normal.Y);

			result.X = vector.X - ((2.0f * dot) * normal.X);
			result.Y = vector.Y - ((2.0f * dot) * normal.Y);
		}

		/// <summary>
		/// Returns the reflection of a vector off a surface that has the specified normal. 
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <returns>The reflected vector.</returns>
		/// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
		/// whether the original vector was close enough to the surface to hit it.</remarks>
		public static Vector2 Reflect(Vector2 vector, Vector2 normal)
		{
			Vector2 result;
			Reflect(ref vector, ref normal, out result);
			return result;
		}

		/// <summary>
		/// Returns the fraction of a vector off a surface that has the specified normal and index.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <param name="index">Index of refraction.</param>
		/// <param name="result">When the method completes, contains the refracted vector.</param>
		public static void Refract(ref Vector2 vector, ref Vector2 normal, float index, out Vector2 result)
		{
			float cos1;
			Dot(ref vector, ref normal, out cos1);

			float radicand = 1.0f - (index * index) * (1.0f - (cos1 * cos1));

			if (radicand < 0.0f)
			{
				result = Vector2.Zero;
			}
			else
			{
				float cos2 = (float)Math.Sqrt(radicand);
				result = (index * vector) + ((cos2 - index * cos1) * normal);
			}
		}

		/// <summary>
		/// Returns the fraction of a vector off a surface that has the specified normal and index.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="normal">Normal of the surface.</param>
		/// <param name="index">Index of refraction.</param>
		/// <returns>The refracted vector.</returns>
		public static Vector2 Refract(Vector2 vector, Vector2 normal, float index)
		{
			Vector2 result;
			Refract(ref vector, ref normal, index, out result);
			return result;
		}

		/// <summary>
		/// Orthogonalizes a list of vectors.
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
		public static void Orthogonalize(IList<Vector2> destination, IList<Vector2> source)
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
				Vector2 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					newvector -= (Vector2.Dot(destination[r], newvector) / Vector2.Dot(destination[r], destination[r])) * destination[r];
				}

				destination[i] = newvector;
			}
		}

		/// <summary>
		/// Orthonormalizes a list of vectors.
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
		public static void Orthonormalize(IList<Vector2> destination, IList<Vector2> source)
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
				Vector2 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					newvector -= Vector2.Dot(destination[r], newvector) * destination[r];
				}

				newvector.Normalize();
				destination[i] = newvector;
			}
		}

		/// <summary>
		/// Transforms a 2D vector by the given <see cref="SlimMath.Quaternion"/> rotation.
		/// </summary>
		/// <param name="vector">The vector to rotate.</param>
		/// <param name="rotation">The <see cref="SlimMath.Quaternion"/> rotation to apply.</param>
		/// <param name="result">When the method completes, contains the transformed <see cref="SlimMath.Vector4"/>.</param>
		public static void Transform(ref Vector2 vector, ref Quaternion rotation, out Vector2 result)
		{
			float x = rotation.X + rotation.X;
			float y = rotation.Y + rotation.Y;
			float z = rotation.Z + rotation.Z;
			float wz = rotation.W * z;
			float xx = rotation.X * x;
			float xy = rotation.X * y;
			float yy = rotation.Y * y;
			float zz = rotation.Z * z;

			float num1 = (1.0f - yy - zz);
			float num2 = (xy - wz);
			float num3 = (xy + wz);
			float num4 = (1.0f - xx - zz);

			result = new Vector2(
				(vector.X * num1) + (vector.Y * num2),
				(vector.X * num3) + (vector.Y * num4));
		}

		/// <summary>
		/// Transforms a 2D vector by the given <see cref="SlimMath.Quaternion"/> rotation.
		/// </summary>
		/// <param name="vector">The vector to rotate.</param>
		/// <param name="rotation">The <see cref="SlimMath.Quaternion"/> rotation to apply.</param>
		/// <returns>The transformed <see cref="SlimMath.Vector4"/>.</returns>
		public static Vector2 Transform(Vector2 vector, Quaternion rotation)
		{
			Vector2 result;
			Transform(ref vector, ref rotation, out result);
			return result;
		}

		/// <summary>
		/// Transforms an array of vectors by the given <see cref="SlimMath.Quaternion"/> rotation.
		/// </summary>
		/// <param name="source">The array of vectors to transform.</param>
		/// <param name="rotation">The <see cref="SlimMath.Quaternion"/> rotation to apply.</param>
		/// <param name="destination">The array for which the transformed vectors are stored.
		/// This array may be the same array as <paramref name="source"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		public static void Transform(IList<Vector2> source, ref Quaternion rotation, IList<Vector2> destination)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			float x = rotation.X + rotation.X;
			float y = rotation.Y + rotation.Y;
			float z = rotation.Z + rotation.Z;
			float wz = rotation.W * z;
			float xx = rotation.X * x;
			float xy = rotation.X * y;
			float yy = rotation.Y * y;
			float zz = rotation.Z * z;

			float num1 = (1.0f - yy - zz);
			float num2 = (xy - wz);
			float num3 = (xy + wz);
			float num4 = (1.0f - xx - zz);

			for (int i = 0; i < source.Count; ++i)
			{
				destination[i] = new Vector2(
					(source[i].X * num1) + (source[i].Y * num2),
					(source[i].X * num3) + (source[i].Y * num4));
			}
		}

		/// <summary>
		/// Transforms a 2D vector by the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <param name="result">When the method completes, contains the transformed <see cref="SlimMath.Vector4"/>.</param>
		public static void Transform(ref Vector2 vector, ref Matrix transform, out Vector4 result)
		{
			result = new Vector4(
				(vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41,
				(vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42,
				(vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43,
				(vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44);
		}

		/// <summary>
		/// Transforms a 2D vector by the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="vector">The source vector.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <returns>The transformed <see cref="SlimMath.Vector4"/>.</returns>
		public static Vector4 Transform(Vector2 vector, Matrix transform)
		{
			Vector4 result;
			Transform(ref vector, ref transform, out result);
			return result;
		}

		/// <summary>
		/// Transforms an array of 2D vectors by the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="source">The array of vectors to transform.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <param name="destination">The array for which the transformed vectors are stored.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		public static void Transform(IList<Vector2> source, ref Matrix transform, IList<Vector4> destination)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			for (int i = 0; i < source.Count; ++i)
			{
				Vector2 src = source[i];
				Vector4 dest = Vector4.Zero;
				Transform(ref src, ref transform, out dest);
				destination[i] = dest;
			}
		}

		/// <summary>
		/// Performs a coordinate transformation using the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="coordinate">The coordinate vector to transform.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <param name="result">When the method completes, contains the transformed coordinates.</param>
		/// <remarks>
		/// A coordinate transform performs the transformation with the assumption that the w component
		/// is one. The four dimensional vector obtained from the transformation operation has each
		/// component in the vector divided by the w component. This forces the wcomponent to be one and
		/// therefore makes the vector homogeneous. The homogeneous vector is often prefered when working
		/// with coordinates as the w component can safely be ignored.
		/// </remarks>
		public static void TransformCoordinate(ref Vector2 coordinate, ref Matrix transform, out Vector2 result)
		{
			Vector4 vector = new Vector4();
			vector.X = (coordinate.X * transform.M11) + (coordinate.Y * transform.M21) + transform.M41;
			vector.Y = (coordinate.X * transform.M12) + (coordinate.Y * transform.M22) + transform.M42;
			vector.Z = (coordinate.X * transform.M13) + (coordinate.Y * transform.M23) + transform.M43;
			vector.W = 1f / ((coordinate.X * transform.M14) + (coordinate.Y * transform.M24) + transform.M44);

			result = new Vector2(vector.X * vector.W, vector.Y * vector.W);
		}

		/// <summary>
		/// Performs a coordinate transformation using the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="coordinate">The coordinate vector to transform.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <returns>The transformed coordinates.</returns>
		/// <remarks>
		/// A coordinate transform performs the transformation with the assumption that the w component
		/// is one. The four dimensional vector obtained from the transformation operation has each
		/// component in the vector divided by the w component. This forces the wcomponent to be one and
		/// therefore makes the vector homogeneous. The homogeneous vector is often prefered when working
		/// with coordinates as the w component can safely be ignored.
		/// </remarks>
		public static Vector2 TransformCoordinate(Vector2 coordinate, Matrix transform)
		{
			Vector2 result;
			TransformCoordinate(ref coordinate, ref transform, out result);
			return result;
		}

		/// <summary>
		/// Performs a coordinate transformation on an array of vectors using the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="source">The array of coordinate vectors to trasnform.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <param name="destination">The array for which the transformed vectors are stored.
		/// This array may be the same array as <paramref name="source"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		/// <remarks>
		/// A coordinate transform performs the transformation with the assumption that the w component
		/// is one. The four dimensional vector obtained from the transformation operation has each
		/// component in the vector divided by the w component. This forces the wcomponent to be one and
		/// therefore makes the vector homogeneous. The homogeneous vector is often prefered when working
		/// with coordinates as the w component can safely be ignored.
		/// </remarks>
		public static void TransformCoordinate(IList<Vector2> source, ref Matrix transform, IList<Vector2> destination)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			for (int i = 0; i < source.Count; ++i)
			{
				Vector2 src = source[i];
				Vector2 dest = Vector2.Zero;
				TransformCoordinate(ref src, ref transform, out dest);
				destination[i] = dest;
			}
		}

		/// <summary>
		/// Performs a normal transformation using the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="normal">The normal vector to transform.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <param name="result">When the method completes, contains the transformed normal.</param>
		/// <remarks>
		/// A normal transform performs the transformation with the assumption that the w component
		/// is zero. This causes the fourth row and fourth collumn of the matrix to be unused. The
		/// end result is a vector that is not translated, but all other transformation properties
		/// apply. This is often prefered for normal vectors as normals purely represent direction
		/// rather than location because normal vectors should not be translated.
		/// </remarks>
		public static void TransformNormal(ref Vector2 normal, ref Matrix transform, out Vector2 result)
		{
			result = new Vector2(
				(normal.X * transform.M11) + (normal.Y * transform.M21),
				(normal.X * transform.M12) + (normal.Y * transform.M22));
		}

		/// <summary>
		/// Performs a normal transformation using the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="normal">The normal vector to transform.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <returns>The transformed normal.</returns>
		/// <remarks>
		/// A normal transform performs the transformation with the assumption that the w component
		/// is zero. This causes the fourth row and fourth collumn of the matrix to be unused. The
		/// end result is a vector that is not translated, but all other transformation properties
		/// apply. This is often prefered for normal vectors as normals purely represent direction
		/// rather than location because normal vectors should not be translated.
		/// </remarks>
		public static Vector2 TransformNormal(Vector2 normal, Matrix transform)
		{
			Vector2 result;
			TransformNormal(ref normal, ref transform, out result);
			return result;
		}

		/// <summary>
		/// Performs a normal transformation on an array of vectors using the given <see cref="SlimMath.Matrix"/>.
		/// </summary>
		/// <param name="source">The array of normal vectors to transform.</param>
		/// <param name="transform">The transformation <see cref="SlimMath.Matrix"/>.</param>
		/// <param name="destination">The array for which the transformed vectors are stored.
		/// This array may be the same array as <paramref name="source"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		/// <remarks>
		/// A normal transform performs the transformation with the assumption that the w component
		/// is zero. This causes the fourth row and fourth collumn of the matrix to be unused. The
		/// end result is a vector that is not translated, but all other transformation properties
		/// apply. This is often prefered for normal vectors as normals purely represent direction
		/// rather than location because normal vectors should not be translated.
		/// </remarks>
		public static void TransformNormal(IList<Vector2> source, ref Matrix transform, IList<Vector2> destination)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			for (int i = 0; i < source.Count; ++i)
			{
				Vector2 src = source[i];
				Vector2 dest = Vector2.Zero;
				TransformNormal(ref src, ref transform, out dest);
				destination[i] = dest;
			}
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="left">The first vector to add.</param>
		/// <param name="right">The second vector to add.</param>
		/// <returns>The sum of the two vectors.</returns>
		public static Vector2 operator +(Vector2 left, Vector2 right)
		{
			return new Vector2(left.X + right.X, left.Y + right.Y);
		}

		/// <summary>
		/// Assert a vector (return it unchanged).
		/// </summary>
		/// <param name="value">The vector to assert (unchange).</param>
		/// <returns>The asserted (unchanged) vector.</returns>
		public static Vector2 operator +(Vector2 value)
		{
			return value;
		}

		/// <summary>
		/// Subtracts two vectors.
		/// </summary>
		/// <param name="left">The first vector to subtract.</param>
		/// <param name="right">The second vector to subtract.</param>
		/// <returns>The difference of the two vectors.</returns>
		public static Vector2 operator -(Vector2 left, Vector2 right)
		{
			return new Vector2(left.X - right.X, left.Y - right.Y);
		}

		/// <summary>
		/// Reverses the direction of a given vector.
		/// </summary>
		/// <param name="value">The vector to negate.</param>
		/// <returns>A vector facing in the opposite direction.</returns>
		public static Vector2 operator -(Vector2 value)
		{
			return new Vector2(-value.X, -value.Y);
		}

		/// <summary>
		/// Scales a vector by the given value.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <param name="scalar">The amount by which to scale the vector.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 operator *(float scalar, Vector2 value)
		{
			return new Vector2(value.X * scalar, value.Y * scalar);
		}

		/// <summary>
		/// Scales a vector by the given value.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <param name="scalar">The amount by which to scale the vector.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 operator *(Vector2 value, float scalar)
		{
			return new Vector2(value.X * scalar, value.Y * scalar);
		}

		/// <summary>
		/// Scales a vector by the given value.
		/// </summary>
		/// <param name="value">The vector to scale.</param>
		/// <param name="scalar">The amount by which to scale the vector.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 operator /(Vector2 value, float scalar)
		{
			return new Vector2(value.X / scalar, value.Y / scalar);
		}

		/// <summary>
		/// Tests for equality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator ==(Vector2 left, Vector2 right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Tests for inequality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator !=(Vector2 left, Vector2 right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.Vector2"/> to <see cref="SlimMath.Vector3"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Vector3(Vector2 value)
		{
			return new Vector3(value, 0.0f);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.Vector2"/> to <see cref="SlimMath.Vector4"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Vector4(Vector2 value)
		{
			return new Vector4(value, 0.0f, 0.0f);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(string format)
		{
			if (format == null)
				return ToString();

			return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(IFormatProvider formatProvider)
		{
			return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
		}

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
			if (format == null)
				ToString(formatProvider);

			return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified <see cref="SlimMath.Vector2"/> is equal to this instance.
		/// </summary>
		/// <param name="other">The <see cref="SlimMath.Vector2"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="SlimMath.Vector2"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(Vector2 other)
		{
			return (this.X == other.X) && (this.Y == other.Y);
		}

		/// <summary>
		/// Determines whether the specified <see cref="SlimMath.Vector2"/> is equal to this instance using an epsilon value.
		/// </summary>
		/// <param name="other">The <see cref="SlimMath.Vector2"/> to compare with this instance.</param>
		/// <param name="epsilon">The amount of error allowed.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="SlimMath.Vector2"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(Vector2 other, float epsilon)
		{
			return ((float)Math.Abs(other.X - X) < epsilon &&
				(float)Math.Abs(other.Y - Y) < epsilon);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType() != GetType())
				return false;

			return Equals((Vector2)obj);
		}

#if SlimDX1xInterop
		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.Vector2"/> to <see cref="SlimDX.Vector2"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator SlimDX.Vector2(Vector2 value)
		{
			return new SlimDX.Vector2(value.X, value.Y);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimDX.Vector2"/> to <see cref="SlimMath.Vector2"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Vector2(SlimDX.Vector2 value)
		{
			return new Vector2(value.X, value.Y);
		}
#endif

#if WPFInterop
		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.Vector2"/> to <see cref="System.Windows.Point"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator System.Windows.Point(Vector2 value)
		{
			return new System.Windows.Point(value.X, value.Y);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="System.Windows.Point"/> to <see cref="SlimMath.Vector2"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator Vector2(System.Windows.Point value)
		{
			return new Vector2((float)value.X, (float)value.Y);
		}
#endif

#if XnaInterop
		/// <summary>
		/// Performs an implicit conversion from <see cref="SlimMath.Vector2"/> to <see cref="Microsoft.Xna.Framework.Vector2"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Microsoft.Xna.Framework.Vector2(Vector2 value)
		{
			return new Microsoft.Xna.Framework.Vector2(value.X, value.Y);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="Microsoft.Xna.Framework.Vector2"/> to <see cref="SlimMath.Vector2"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Vector2(Microsoft.Xna.Framework.Vector2 value)
		{
			return new Vector2(value.X, value.Y);
		}
#endif
	}
}
