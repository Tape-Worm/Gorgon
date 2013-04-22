#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, June 24, 2011 9:58:10 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using GorgonLibrary.Properties;

namespace GorgonLibrary
{
	/// <summary>
	/// Interface used to define a range type.
	/// </summary>
	/// <typeparam name="T">Type of values stored in the range.</typeparam>
	internal interface IRange<out T>
	{
		/// <summary>
		/// Property to return the range between the two values.
		/// </summary>
		T Range
		{
			get;
		}

		/// <summary>
		/// Property to return whether the value is empty or not.
		/// </summary>
		bool IsEmpty
		{
			get;
		}
	}

	#region Double
	/// <summary>
	/// Value type to indicate a range of double values.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonMinMaxD
		: IRange<double>, IEquatable<GorgonMinMaxD>
	{
		#region Variables.
		/// <summary>
		/// Minimum value in the range.
		/// </summary>
		public double Minimum;
		/// <summary>
		/// Maximum value in the range.
		/// </summary>
		public double Maximum;

		/// <summary>
		/// Empty range.
		/// </summary>
		public static readonly GorgonMinMaxD Empty = new GorgonMinMaxD(0.0, 0.0);
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether the value falls within the range.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns>TRUE if the value falls into the range.</returns>
		public bool Contains(double value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>
		/// <param name="result">The smaller range.</param>
		public static void Shrink(ref GorgonMinMaxD range, double value, out GorgonMinMaxD result)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}
			else
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}

			if (min < max)
			{
				result.Minimum = min;
				result.Maximum = max;
			}
			else
			{
				result.Minimum = max;
				result.Maximum = min;
			}
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>		
		public static GorgonMinMaxD Shrink(GorgonMinMaxD range, double value)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}
			else
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}

			return new GorgonMinMaxD(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <param name="result">The larger range.</param>
		public static void Expand(ref GorgonMinMaxD range, double value, out GorgonMinMaxD result)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}
			else
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}

			if (min < max)
			{
				result.Minimum = min;
				result.Maximum = max;
			}
			else
			{
				result.Minimum = max;
				result.Maximum = min;
			}
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <returns>The larger range.</returns>
		public static GorgonMinMaxD Expand(GorgonMinMaxD range, double value)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}
			else
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}

			return new GorgonMinMaxD(min, max);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <param name="result">The total of both ranges.</param>
		public static void Add(ref GorgonMinMaxD left, ref GorgonMinMaxD right, out GorgonMinMaxD result)
		{
			result.Minimum = left.Minimum + right.Minimum;
			result.Maximum = left.Maximum + right.Maximum;
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <returns>The total of both ranges.</returns>
		public static GorgonMinMaxD Add(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			return new GorgonMinMaxD(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <param name="result">The difference of both ranges.</param>
		public static void Subtract(ref GorgonMinMaxD left, ref GorgonMinMaxD right, out GorgonMinMaxD result)
		{
			result.Minimum = left.Minimum - right.Minimum;
			result.Maximum = left.Maximum - right.Maximum;
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <returns>The difference of both ranges.</returns>
		public static GorgonMinMaxD Subtract(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			return new GorgonMinMaxD(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <param name="result">The product of both ranges.</param>
		public static void Multiply(ref GorgonMinMaxD left, ref GorgonMinMaxD right, out GorgonMinMaxD result)
		{
			result.Minimum = left.Minimum * right.Minimum;
			result.Maximum = left.Maximum * right.Maximum;
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <returns>The product of both ranges.</returns>
		public static GorgonMinMaxD Multiply(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			return new GorgonMinMaxD(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <param name="result">The product of the range and the scalar.</param>
		public static void Multiply(ref GorgonMinMaxD left, double scalar, out GorgonMinMaxD result)
		{
			result.Minimum = left.Minimum * scalar;
			result.Maximum = left.Maximum * scalar;
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <returns>The product of the range and the scalar.</returns>
		public static GorgonMinMaxD Multiply(GorgonMinMaxD left, double scalar)
		{
			return new GorgonMinMaxD(left.Minimum * scalar, left.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide two min-max ranges.
		/// </summary>
		/// <param name="left">Left range to divide</param>
		/// <param name="right">Right range to divide.</param>
		/// <param name="result">The quotient of both ranges.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the right Minimum or Maximum value is zero.</exception>
		public static void Divide(ref GorgonMinMaxD left, ref GorgonMinMaxD right, out GorgonMinMaxD result)
		{
			result.Minimum = left.Minimum / right.Minimum;
			result.Maximum = left.Maximum / right.Maximum;
		}

		/// <summary>
		/// Function to multiply two min-max ranges.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="right">Right range to divide.</param>
		/// <returns>The quotient of both ranges.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the right Minimum or Maximum value is zero.</exception>
		public static GorgonMinMaxD Divide(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			return new GorgonMinMaxD(left.Minimum / right.Minimum, left.Maximum / right.Maximum);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <param name="result">The quotient of the range and the scalar.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static void Divide(ref GorgonMinMaxD left, double scalar, out GorgonMinMaxD result)
		{
			result.Minimum = left.Minimum / scalar;
			result.Maximum = left.Maximum / scalar;
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <returns>The quotient of the range and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static GorgonMinMaxD Divide(GorgonMinMaxD left, double scalar)
		{
			return new GorgonMinMaxD(left.Minimum / scalar, left.Minimum / scalar);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			IEquatable<GorgonMinMaxD> equate = obj as IEquatable<GorgonMinMaxD>;

			if (equate != null)
				return equate.Equals(this);

			return false;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed floateger that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
// ReSharper disable NonReadonlyFieldInGetHashCode
			return 281.GenerateHash(Minimum).GenerateHash(Maximum);
// ReSharper restore NonReadonlyFieldInGetHashCode
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Min: {0}  Max: {1}  Range: {2}", Minimum, Maximum, Range);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMinMaxD" /> struct.
		/// </summary>
		/// <param name="minMax">The min max value to copy.</param>
		public GorgonMinMaxD(GorgonMinMaxD minMax)
			: this(minMax.Minimum, minMax.Maximum)
		{		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMinMaxD"/> struct.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public GorgonMinMaxD(double min, double max)
		{
			if (min < max)
			{
				Minimum = min;
				Maximum = max;
			}
			else
			{
				Maximum = min;
				Minimum = max;
			}
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonLibrary.GorgonMinMaxD"/> to <see cref="GorgonLibrary.GorgonMinMax"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonMinMax(GorgonMinMaxD range)
		{
			return new GorgonMinMax((int)range.Minimum, (int)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonLibrary.GorgonMinMaxD"/> to <see cref="GorgonLibrary.GorgonMinMaxF"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonMinMaxF(GorgonMinMaxD range)
		{
			return new GorgonMinMaxF((float)range.Minimum, (float)range.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			return left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			return !(left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum));
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxD operator +(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			GorgonMinMaxD result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxD operator -(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			GorgonMinMaxD result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxD operator *(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			GorgonMinMaxD result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxD operator /(GorgonMinMaxD left, GorgonMinMaxD right)
		{
			GorgonMinMaxD result;
			Divide(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxD operator *(GorgonMinMaxD left, double scalar)
		{
			GorgonMinMaxD result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="right">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxD operator *(double scalar, GorgonMinMaxD right)
		{
			GorgonMinMaxD result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxD operator /(GorgonMinMaxD left, double scalar)
		{
			GorgonMinMaxD result;
			Divide(ref left, scalar, out result);
			return result;
		}
		#endregion

		#region IRange<double> Members
		/// <summary>
		/// Property to return whether the range is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return !((Maximum != 0.0) || (Minimum != 0.0));
			}
		}

		/// <summary>
		/// Property to return the range between the two values.
		/// </summary>
		public double Range
		{
			get
			{
				return (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);
			}
		}
		#endregion

		#region IEquatable<GorgonMinMaxD> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonMinMaxD other)
		{
			return (Minimum == other.Minimum) && (Maximum == other.Maximum);
		}
		#endregion
	}
	#endregion

	#region Float
	/// <summary>
	/// Value type to indicate a range of floating point values.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonMinMaxF
		: IRange<float>, IEquatable<GorgonMinMaxF>
	{
		#region Variables.
		/// <summary>
		/// Minimum value in the range.
		/// </summary>
		public float Minimum;
		/// <summary>
		/// Maximum value in the range.
		/// </summary>
		public float Maximum;

		/// <summary>
		/// Empty range.
		/// </summary>
		public static readonly GorgonMinMaxF Empty = new GorgonMinMaxF(0.0f, 0.0f);
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether the value falls within the range.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns>TRUE if the value falls into the range.</returns>
		public bool Contains(float value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>
		/// <param name="result">The smaller range.</param>
		public static void Shrink(ref GorgonMinMaxF range, float value, out GorgonMinMaxF result)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}
			else
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}

			if (min < max)
			{
				result.Minimum = min;
				result.Maximum = max;
			}
			else
			{
				result.Minimum = max;
				result.Maximum = min;
			}
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>		
		public static GorgonMinMaxF Shrink(GorgonMinMaxF range, float value)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}
			else
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}

			return new GorgonMinMaxF(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <param name="result">The larger range.</param>
		public static void Expand(ref GorgonMinMaxF range, float value, out GorgonMinMaxF result)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}
			else
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}

			if (min < max)
			{
				result.Minimum = min;
				result.Maximum = max;
			}
			else
			{
				result.Minimum = max;
				result.Maximum = min;
			}
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <returns>The larger range.</returns>
		public static GorgonMinMaxF Expand(GorgonMinMaxF range, float value)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}
			else
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}

			return new GorgonMinMaxF(min, max);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <param name="result">The total of both ranges.</param>
		public static void Add(ref GorgonMinMaxF left, ref GorgonMinMaxF right, out GorgonMinMaxF result)
		{
			result.Minimum = left.Minimum + right.Minimum;
			result.Maximum = left.Maximum + right.Maximum;
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <returns>The total of both ranges.</returns>
		public static GorgonMinMaxF Add(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			return new GorgonMinMaxF(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <param name="result">The difference of both ranges.</param>
		public static void Subtract(ref GorgonMinMaxF left, ref GorgonMinMaxF right, out GorgonMinMaxF result)
		{
			result.Minimum = left.Minimum - right.Minimum;
			result.Maximum = left.Maximum - right.Maximum;
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <returns>The difference of both ranges.</returns>
		public static GorgonMinMaxF Subtract(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			return new GorgonMinMaxF(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <param name="result">The product of both ranges.</param>
		public static void Multiply(ref GorgonMinMaxF left, ref GorgonMinMaxF right, out GorgonMinMaxF result)
		{
			result.Minimum = left.Minimum * right.Minimum;
			result.Maximum = left.Maximum * right.Maximum;
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <returns>The product of both ranges.</returns>
		public static GorgonMinMaxF Multiply(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			return new GorgonMinMaxF(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <param name="result">The product of the range and the scalar.</param>
		public static void Multiply(ref GorgonMinMaxF left, float scalar, out GorgonMinMaxF result)
		{
			result.Minimum = left.Minimum * scalar;
			result.Maximum = left.Maximum * scalar;
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <returns>The product of the range and the scalar.</returns>
		public static GorgonMinMaxF Multiply(GorgonMinMaxF left, float scalar)
		{
			return new GorgonMinMaxF(left.Minimum * scalar, left.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide two min-max ranges.
		/// </summary>
		/// <param name="left">Left range to divide</param>
		/// <param name="right">Right range to divide.</param>
		/// <param name="result">The quotient of both ranges.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the right Minimum or Maximum value is zero.</exception>
		public static void Divide(ref GorgonMinMaxF left, ref GorgonMinMaxF right, out GorgonMinMaxF result)
		{
			result.Minimum = left.Minimum / right.Minimum;
			result.Maximum = left.Maximum / right.Maximum;
		}

		/// <summary>
		/// Function to multiply two min-max ranges.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="right">Right range to divide.</param>
		/// <returns>The quotient of both ranges.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the right Minimum or Maximum value is zero.</exception>
		public static GorgonMinMaxF Divide(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			return new GorgonMinMaxF(left.Minimum / right.Minimum, left.Maximum / right.Maximum);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <param name="result">The quotient of the range and the scalar.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static void Divide(ref GorgonMinMaxF left, float scalar, out GorgonMinMaxF result)
		{
			result.Minimum = left.Minimum / scalar;
			result.Maximum = left.Maximum / scalar;
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <returns>The quotient of the range and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static GorgonMinMaxF Divide(GorgonMinMaxF left, float scalar)
		{
			return new GorgonMinMaxF(left.Minimum / scalar, left.Minimum / scalar);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			IEquatable<GorgonMinMaxF> equate = obj as IEquatable<GorgonMinMaxF>;

			if (equate != null)
				return equate.Equals(this);

			return false;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed floateger that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
// ReSharper disable NonReadonlyFieldInGetHashCode
			return 281.GenerateHash(Minimum).GenerateHash(Maximum);
// ReSharper restore NonReadonlyFieldInGetHashCode
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Min: {0}  Max: {1}  Range: {2}", Minimum, Maximum, Range);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMinMaxF" /> struct.
		/// </summary>
		/// <param name="minMax">The min max value to copy.</param>
		public GorgonMinMaxF(GorgonMinMaxF minMax)
			: this(minMax.Minimum, minMax.Maximum)
		{		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMinMaxF"/> struct.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public GorgonMinMaxF(float min, float max)
		{
			if (min < max)
			{
				Minimum = min;
				Maximum = max;
			}
			else
			{
				Maximum = min;
				Minimum = max;
			}
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonLibrary.GorgonMinMaxF"/> to <see cref="GorgonLibrary.GorgonMinMax"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonMinMax(GorgonMinMaxF range)
		{
			return new GorgonMinMax((int)range.Minimum, (int)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonLibrary.GorgonMinMaxF"/> to <see cref="GorgonLibrary.GorgonMinMaxD"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonMinMaxD(GorgonMinMaxF range)
		{
			return new GorgonMinMaxD(range.Minimum, range.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			return left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			return !(left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum));
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxF operator +(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			GorgonMinMaxF result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxF operator -(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			GorgonMinMaxF result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxF operator *(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			GorgonMinMaxF result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxF operator /(GorgonMinMaxF left, GorgonMinMaxF right)
		{
			GorgonMinMaxF result;
			Divide(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxF operator *(GorgonMinMaxF left, float scalar)
		{
			GorgonMinMaxF result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="right">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxF operator *(float scalar, GorgonMinMaxF right)
		{
			GorgonMinMaxF result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMaxF operator /(GorgonMinMaxF left, float scalar)
		{
			GorgonMinMaxF result;
			Divide(ref left, scalar, out result);
			return result;
		}
		#endregion

		#region IRange<float> Members
		/// <summary>
		/// Property to return whether the range is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return !((Maximum != 0.0f) || (Minimum != 0.0f));
			}
		}

		/// <summary>
		/// Property to return the range between the two values.
		/// </summary>
		public float Range
		{
			get
			{
				return (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);
			}
		}
		#endregion

		#region IEquatable<GorgonMinMaxF> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonMinMaxF other)
		{
			return other.Minimum == Minimum && other.Maximum == Maximum;
		}
		#endregion
	}
	#endregion

	#region Int32
	/// <summary>
	/// Value type to indicate a range of integer values.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct GorgonMinMax
		: IRange<int>, IEquatable<GorgonMinMax>
	{
		#region Variables.
		/// <summary>
		/// Minimum value in the range.
		/// </summary>
		public int Minimum;
		/// <summary>
		/// Maximum value in the range.
		/// </summary>
		public int Maximum;

		/// <summary>
		/// Empty range.
		/// </summary>
		public static readonly GorgonMinMax Empty = new GorgonMinMax(0, 0);
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether the value falls within the range.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns>TRUE if the value falls into the range.</returns>
		public bool Contains(int value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>
		/// <param name="result">The smaller range.</param>
		public static void Shrink(ref GorgonMinMax range, int value, out GorgonMinMax result)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}
			else
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}

			if (min < max)
			{
				result.Minimum = min;
				result.Maximum = max;
			}
			else
			{
				result.Minimum = max;
				result.Maximum = min;
			}
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>		
		public static GorgonMinMax Shrink(GorgonMinMax range, int value)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}
			else
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}

			return new GorgonMinMax(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <param name="result">The larger range.</param>
		public static void Expand(ref GorgonMinMax range, int value, out GorgonMinMax result)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}
			else
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}

			if (min < max)
			{
				result.Minimum = min;
				result.Maximum = max;
			}
			else
			{
				result.Minimum = max;
				result.Maximum = min;
			}
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <returns>The larger range.</returns>
		public static GorgonMinMax Expand(GorgonMinMax range, int value)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - value;
				max = range.Maximum + value;
			}
			else
			{
				min = range.Minimum + value;
				max = range.Maximum - value;
			}

			return new GorgonMinMax(min, max);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <param name="result">The total of both ranges.</param>
		public static void Add(ref GorgonMinMax left, ref GorgonMinMax right, out GorgonMinMax result)
		{
			result.Minimum = left.Minimum + right.Minimum;
			result.Maximum = left.Maximum + right.Maximum;
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <returns>The total of both ranges.</returns>
		public static GorgonMinMax Add(GorgonMinMax left, GorgonMinMax right)
		{
			return new GorgonMinMax(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <param name="result">The difference of both ranges.</param>
		public static void Subtract(ref GorgonMinMax left, ref GorgonMinMax right, out GorgonMinMax result)
		{
			result.Minimum = left.Minimum - right.Minimum;
			result.Maximum = left.Maximum - right.Maximum;
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <returns>The difference of both ranges.</returns>
		public static GorgonMinMax Subtract(GorgonMinMax left, GorgonMinMax right)
		{
			return new GorgonMinMax(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <param name="result">The product of both ranges.</param>
		public static void Multiply(ref GorgonMinMax left, ref GorgonMinMax right, out GorgonMinMax result)
		{
			result.Minimum = left.Minimum * right.Minimum;
			result.Maximum = left.Maximum * right.Maximum;
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <returns>The product of both ranges.</returns>
		public static GorgonMinMax Multiply(GorgonMinMax left, GorgonMinMax right)
		{
			return new GorgonMinMax(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <param name="result">The product of the range and the scalar.</param>
		public static void Multiply(ref GorgonMinMax left, int scalar, out GorgonMinMax result)
		{
			result.Minimum = left.Minimum * scalar;
			result.Maximum = left.Maximum * scalar;
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <returns>The product of the range and the scalar.</returns>
		public static GorgonMinMax Multiply(GorgonMinMax left, int scalar)
		{
			return new GorgonMinMax(left.Minimum * scalar, left.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide two min-max ranges.
		/// </summary>
		/// <param name="left">Left range to divide</param>
		/// <param name="right">Right range to divide.</param>
		/// <param name="result">The quotient of both ranges.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the right Minimum or Maximum value is zero.</exception>
		public static void Divide(ref GorgonMinMax left, ref GorgonMinMax right, out GorgonMinMax result)
		{
			result.Minimum = left.Minimum / right.Minimum;
			result.Maximum = left.Maximum / right.Maximum;
		}

		/// <summary>
		/// Function to multiply two min-max ranges.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="right">Right range to divide.</param>
		/// <returns>The quotient of both ranges.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the right Minimum or Maximum value is zero.</exception>
		public static GorgonMinMax Divide(GorgonMinMax left, GorgonMinMax right)
		{
			return new GorgonMinMax(left.Minimum / right.Minimum, left.Maximum / right.Maximum);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <param name="result">The quotient of the range and the scalar.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static void Divide(ref GorgonMinMax left, int scalar, out GorgonMinMax result)
		{
			result.Minimum = left.Minimum / scalar;
			result.Maximum = left.Maximum / scalar;
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <returns>The quotient of the range and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static GorgonMinMax Divide(GorgonMinMax left, int scalar)
		{
			return new GorgonMinMax(left.Minimum / scalar, left.Minimum / scalar);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			var equate = obj as IEquatable<GorgonMinMax>;

		    if (equate != null)
		    {
		        return equate.Equals(this);
		    }

		    return false;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
            // ReSharper disable NonReadonlyFieldInGetHashCode
			return 281.GenerateHash(Maximum).GenerateHash(Minimum);
            // ReSharper restore NonReadonlyFieldInGetHashCode
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_GORGONRANGE_TOSTRING, Minimum, Maximum, Range);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMinMax" /> struct.
		/// </summary>
		/// <param name="minMax">The min max value to copy.</param>
		public GorgonMinMax(GorgonMinMax minMax)
			: this(minMax.Minimum, minMax.Maximum)
		{		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMinMax"/> struct.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public GorgonMinMax(int min, int max)
		{
			if (min < max)
			{
				Minimum = min;
				Maximum = max;
			}
			else
			{
				Maximum = min;
				Minimum = max;
			}
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonLibrary.GorgonMinMax"/> to <see cref="GorgonLibrary.GorgonMinMaxF"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonMinMaxF(GorgonMinMax range)
		{
			return new GorgonMinMaxF(range.Minimum, range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonLibrary.GorgonMinMax"/> to <see cref="GorgonLibrary.GorgonMinMaxD"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonMinMaxD(GorgonMinMax range)
		{
			return new GorgonMinMaxD(range.Minimum, range.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonMinMax left, GorgonMinMax right)
		{
			return left.Minimum == right.Minimum && left.Maximum == right.Maximum;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonMinMax left, GorgonMinMax right)
		{
			return !(left.Minimum == right.Minimum && left.Maximum == right.Maximum);
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMax operator +(GorgonMinMax left, GorgonMinMax right)
		{
			GorgonMinMax result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMax operator -(GorgonMinMax left, GorgonMinMax right)
		{
			GorgonMinMax result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMax operator *(GorgonMinMax left, GorgonMinMax right)
		{
			GorgonMinMax result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMax operator /(GorgonMinMax left, GorgonMinMax right)
		{
			GorgonMinMax result;
			Divide(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMax operator *(GorgonMinMax left, int scalar)
		{
			GorgonMinMax result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="right">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMax operator *(int scalar, GorgonMinMax right)
		{
			GorgonMinMax result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonMinMax operator /(GorgonMinMax left, int scalar)
		{
			GorgonMinMax result;
			Divide(ref left, scalar, out result);
			return result;
		}
		#endregion

		#region IRange<int> Members
		/// <summary>
		/// Property to return whether the range is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return !((Maximum != 0) || (Minimum != 0));
			}
		}

		/// <summary>
		/// Property to return the range between the two values.
		/// </summary>
		public int Range
		{
			get
			{
				return (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);
			}
		}
		#endregion

		#region IEquatable<GorgonMinMax> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonMinMax other)
		{
			return (Minimum == other.Minimum) && (Maximum == other.Maximum);
		}
		#endregion
	}
	#endregion
}
