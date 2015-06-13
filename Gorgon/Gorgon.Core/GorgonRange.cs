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
using Gorgon.Core.Properties;
using Gorgon.Math;

namespace Gorgon.Core
{
	#region Double
	/// <summary>
	/// Value type to indicate a range of double values.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonRangeD
		: IEquatable<GorgonRangeD>, IComparable<GorgonRangeD>
	{
		#region Variables.
		/// <summary>
		/// Minimum value in the range.
		/// </summary>
		public readonly double Minimum;
		/// <summary>
		/// Maximum value in the range.
		/// </summary>
		public readonly double Maximum;

		/// <summary>
		/// Empty range.
		/// </summary>
		public static readonly GorgonRangeD Empty = new GorgonRangeD(0.0, 0.0);
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the range is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ((Maximum.EqualsEpsilon(0.0)) && (Minimum.EqualsEpsilon(0.0)));
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

        #region Methods.
        /// <summary>
		/// Function to return whether the value falls within the range.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range.</returns>
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
		public static void Shrink(ref GorgonRangeD range, double value, out GorgonRangeD result)
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

			result = new GorgonRangeD(min, max);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>		
		public static GorgonRangeD Shrink(GorgonRangeD range, double value)
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

			return new GorgonRangeD(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <param name="result">The larger range.</param>
		public static void Expand(ref GorgonRangeD range, double value, out GorgonRangeD result)
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

			result = new GorgonRangeD(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <returns>The larger range.</returns>
		public static GorgonRangeD Expand(GorgonRangeD range, double value)
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

			return new GorgonRangeD(min, max);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <param name="result">The total of both ranges.</param>
		public static void Add(ref GorgonRangeD left, ref GorgonRangeD right, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <returns>The total of both ranges.</returns>
		public static GorgonRangeD Add(GorgonRangeD left, GorgonRangeD right)
		{
			return new GorgonRangeD(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <param name="result">The difference of both ranges.</param>
		public static void Subtract(ref GorgonRangeD left, ref GorgonRangeD right, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <returns>The difference of both ranges.</returns>
		public static GorgonRangeD Subtract(GorgonRangeD left, GorgonRangeD right)
		{
			return new GorgonRangeD(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <param name="result">The product of both ranges.</param>
		public static void Multiply(ref GorgonRangeD left, ref GorgonRangeD right, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <returns>The product of both ranges.</returns>
		public static GorgonRangeD Multiply(GorgonRangeD left, GorgonRangeD right)
		{
			return new GorgonRangeD(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <param name="result">The product of the range and the scalar.</param>
		public static void Multiply(ref GorgonRangeD left, double scalar, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum * scalar, left.Maximum * scalar);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <returns>The product of the range and the scalar.</returns>
		public static GorgonRangeD Multiply(GorgonRangeD left, double scalar)
		{
			return new GorgonRangeD(left.Minimum * scalar, left.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <param name="result">The quotient of the range and the scalar.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static void Divide(ref GorgonRangeD left, double scalar, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum / scalar, left.Maximum / scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <returns>The quotient of the range and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static GorgonRangeD Divide(GorgonRangeD left, double scalar)
		{
			return new GorgonRangeD(left.Minimum / scalar, left.Minimum / scalar);
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
			var equate = obj as IEquatable<GorgonRangeD>;

			return equate != null && equate.Equals(this);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Minimum).GenerateHash(Maximum);
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
		/// Initializes a new instance of the <see cref="GorgonRangeD" /> struct.
		/// </summary>
		/// <param name="minMax">The min max value to copy.</param>
		public GorgonRangeD(GorgonRangeD minMax)
			: this(minMax.Minimum, minMax.Maximum)
		{		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRangeD"/> struct.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public GorgonRangeD(double min, double max)
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
		/// Performs an explicit conversion from <see cref="GorgonRangeD"/> to <see cref="GorgonRangeM"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRangeM(GorgonRangeD range)
		{
			return new GorgonRangeM((decimal)range.Minimum, (decimal)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRangeD"/> to <see cref="GorgonRange"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRange(GorgonRangeD range)
		{
			return new GorgonRange((int)range.Minimum, (int)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRangeD"/> to <see cref="GorgonRangeF"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRangeF(GorgonRangeD range)
		{
			return new GorgonRangeF((float)range.Minimum, (float)range.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonRangeD left, GorgonRangeD right)
		{
			return left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonRangeD left, GorgonRangeD right)
		{
			return !(left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum));
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeD operator +(GorgonRangeD left, GorgonRangeD right)
		{
			GorgonRangeD result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeD operator -(GorgonRangeD left, GorgonRangeD right)
		{
			GorgonRangeD result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeD operator *(GorgonRangeD left, GorgonRangeD right)
		{
			GorgonRangeD result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeD operator *(GorgonRangeD left, double scalar)
		{
			GorgonRangeD result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="right">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeD operator *(double scalar, GorgonRangeD right)
		{
			GorgonRangeD result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeD operator /(GorgonRangeD left, double scalar)
		{
			GorgonRangeD result;
			Divide(ref left, scalar, out result);
			return result;
		}

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator >(GorgonRangeD left, GorgonRangeD right)
        {
            return left.Range > right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator >=(GorgonRangeD left, GorgonRangeD right)
        {
            return left.Range >= right.Range;
        }

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <(GorgonRangeD left, GorgonRangeD right)
        {
            return left.Range < right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <=(GorgonRangeD left, GorgonRangeD right)
        {
            return left.Range <= right.Range;
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
		public bool Equals(GorgonRangeD other)
		{
			return (Minimum.EqualsEpsilon(other.Minimum)) && (Maximum.EqualsEpsilon(other.Maximum));
		}
		#endregion

        #region IComparable<GorgonRangeD> Members
        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(GorgonRangeD other)
        {
            return Range.CompareTo(other.Range);
        }
        #endregion
    }
	#endregion

	#region Decimal
	/// <summary>
	/// Value type to indicate a range of decimal values.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonRangeM
		: IEquatable<GorgonRangeM>, IComparable<GorgonRangeM>
	{
		#region Variables.
		/// <summary>
		/// Minimum value in the range.
		/// </summary>
		public readonly decimal Minimum;
		/// <summary>
		/// Maximum value in the range.
		/// </summary>
		public readonly decimal Maximum;
		
		/// <summary>
		/// Empty range.
		/// </summary>
		public static readonly GorgonRangeM Empty = new GorgonRangeM(0.0M, 0.0M);
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the range is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return !((Maximum != 0.0M) || (Minimum != 0.0M));
            }
        }

        /// <summary>
        /// Property to return the range between the two values.
        /// </summary>
        public decimal Range
        {
            get
            {
                return (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
		/// Function to return whether the value falls within the range.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range.</returns>
		public bool Contains(decimal value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>
		/// <param name="result">The smaller range.</param>
		public static void Shrink(ref GorgonRangeM range, decimal value, out GorgonRangeM result)
		{
			decimal min;
			decimal max;

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

			result = new GorgonRangeM(min, max);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>		
		public static GorgonRangeM Shrink(GorgonRangeM range, decimal value)
		{
			decimal min;
			decimal max;

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

			return new GorgonRangeM(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <param name="result">The larger range.</param>
		public static void Expand(ref GorgonRangeM range, decimal value, out GorgonRangeM result)
		{
			decimal min;
			decimal max;

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

			result = new GorgonRangeM(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <returns>The larger range.</returns>
		public static GorgonRangeM Expand(GorgonRangeM range, decimal value)
		{
			decimal min;
			decimal max;

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

			return new GorgonRangeM(min, max);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <param name="result">The total of both ranges.</param>
		public static void Add(ref GorgonRangeM left, ref GorgonRangeM right, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <returns>The total of both ranges.</returns>
		public static GorgonRangeM Add(GorgonRangeM left, GorgonRangeM right)
		{
			return new GorgonRangeM(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <param name="result">The difference of both ranges.</param>
		public static void Subtract(ref GorgonRangeM left, ref GorgonRangeM right, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <returns>The difference of both ranges.</returns>
		public static GorgonRangeM Subtract(GorgonRangeM left, GorgonRangeM right)
		{
			return new GorgonRangeM(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <param name="result">The product of both ranges.</param>
		public static void Multiply(ref GorgonRangeM left, ref GorgonRangeM right, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <returns>The product of both ranges.</returns>
		public static GorgonRangeM Multiply(GorgonRangeM left, GorgonRangeM right)
		{
			return new GorgonRangeM(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <param name="result">The product of the range and the scalar.</param>
		public static void Multiply(ref GorgonRangeM left, decimal scalar, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum * scalar, left.Maximum * scalar);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <returns>The product of the range and the scalar.</returns>
		public static GorgonRangeM Multiply(GorgonRangeM left, decimal scalar)
		{
			return new GorgonRangeM(left.Minimum * scalar, left.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <param name="result">The quotient of the range and the scalar.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static void Divide(ref GorgonRangeM left, decimal scalar, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum / scalar, left.Maximum / scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <returns>The quotient of the range and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static GorgonRangeM Divide(GorgonRangeM left, decimal scalar)
		{
			return new GorgonRangeM(left.Minimum / scalar, left.Minimum / scalar);
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
			var equate = obj as IEquatable<GorgonRangeM>;

			return equate != null && equate.Equals(this);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Minimum).GenerateHash(Maximum);
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
		/// Initializes a new instance of the <see cref="GorgonRangeM" /> struct.
		/// </summary>
		/// <param name="minMax">The min max value to copy.</param>
		public GorgonRangeM(GorgonRangeM minMax)
			: this(minMax.Minimum, minMax.Maximum)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRangeM"/> struct.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public GorgonRangeM(decimal min, decimal max)
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
		/// Performs an explicit conversion from <see cref="GorgonRangeM"/> to <see cref="GorgonRange"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRange(GorgonRangeM range)
		{
			return new GorgonRange((int)range.Minimum, (int)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRangeM"/> to <see cref="GorgonRangeF"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRangeF(GorgonRangeM range)
		{
			return new GorgonRangeF((float)range.Minimum, (float)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRangeM"/> to <see cref="GorgonRangeD"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRangeD(GorgonRangeM range)
		{
			return new GorgonRangeD((double)range.Minimum, (double)range.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonRangeM left, GorgonRangeM right)
		{
			return left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonRangeM left, GorgonRangeM right)
		{
			return !(left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum));
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeM operator +(GorgonRangeM left, GorgonRangeM right)
		{
			GorgonRangeM result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeM operator -(GorgonRangeM left, GorgonRangeM right)
		{
			GorgonRangeM result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeM operator *(GorgonRangeM left, GorgonRangeM right)
		{
			GorgonRangeM result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeM operator *(GorgonRangeM left, decimal scalar)
		{
			GorgonRangeM result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="right">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeM operator *(decimal scalar, GorgonRangeM right)
		{
			GorgonRangeM result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeM operator /(GorgonRangeM left, decimal scalar)
		{
			GorgonRangeM result;
			Divide(ref left, scalar, out result);
			return result;
		}

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator >(GorgonRangeM left, GorgonRangeM right)
        {
            return left.Range > right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator >=(GorgonRangeM left, GorgonRangeM right)
        {
            return left.Range >= right.Range;
        }

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <(GorgonRangeM left, GorgonRangeM right)
        {
            return left.Range < right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <=(GorgonRangeM left, GorgonRangeM right)
        {
            return left.Range <= right.Range;
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
		public bool Equals(GorgonRangeM other)
		{
			return (Minimum == other.Minimum) && (Maximum == other.Maximum);
		}
		#endregion

        #region IComparable<GorgonRangeM> Members
        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(GorgonRangeM other)
        {
            return Range.CompareTo(other.Range);
        }
        #endregion
	}
	#endregion

	#region Float
	/// <summary>
	/// Value type to indicate a range of floating point values.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonRangeF
		: IEquatable<GorgonRangeF>, IComparable<GorgonRangeF>
	{
		#region Variables.
		/// <summary>
		/// Minimum value in the range.
		/// </summary>
		public readonly float Minimum;
		/// <summary>
		/// Maximum value in the range.
		/// </summary>
		public readonly float Maximum;

		/// <summary>
		/// Empty range.
		/// </summary>
		public static readonly GorgonRangeF Empty = new GorgonRangeF(0.0f, 0.0f);
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the range is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ((Maximum.EqualsEpsilon(0.0f)) && (Minimum.EqualsEpsilon(0.0f)));
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

        #region Methods.
        /// <summary>
		/// Function to return whether the value falls within the range.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range.</returns>
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
		public static void Shrink(ref GorgonRangeF range, float value, out GorgonRangeF result)
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

			result = new GorgonRangeF(min, max);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>		
		public static GorgonRangeF Shrink(GorgonRangeF range, float value)
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

			return new GorgonRangeF(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <param name="result">The larger range.</param>
		public static void Expand(ref GorgonRangeF range, float value, out GorgonRangeF result)
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

			result = new GorgonRangeF(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <returns>The larger range.</returns>
		public static GorgonRangeF Expand(GorgonRangeF range, float value)
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

			return new GorgonRangeF(min, max);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <param name="result">The total of both ranges.</param>
		public static void Add(ref GorgonRangeF left, ref GorgonRangeF right, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <returns>The total of both ranges.</returns>
		public static GorgonRangeF Add(GorgonRangeF left, GorgonRangeF right)
		{
			return new GorgonRangeF(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <param name="result">The difference of both ranges.</param>
		public static void Subtract(ref GorgonRangeF left, ref GorgonRangeF right, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <returns>The difference of both ranges.</returns>
		public static GorgonRangeF Subtract(GorgonRangeF left, GorgonRangeF right)
		{
			return new GorgonRangeF(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <param name="result">The product of both ranges.</param>
		public static void Multiply(ref GorgonRangeF left, ref GorgonRangeF right, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <returns>The product of both ranges.</returns>
		public static GorgonRangeF Multiply(GorgonRangeF left, GorgonRangeF right)
		{
			return new GorgonRangeF(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <param name="result">The product of the range and the scalar.</param>
		public static void Multiply(ref GorgonRangeF left, float scalar, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum + scalar, left.Maximum + scalar);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <returns>The product of the range and the scalar.</returns>
		public static GorgonRangeF Multiply(GorgonRangeF left, float scalar)
		{
			return new GorgonRangeF(left.Minimum * scalar, left.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <param name="result">The quotient of the range and the scalar.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static void Divide(ref GorgonRangeF left, float scalar, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum / scalar, left.Maximum / scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <returns>The quotient of the range and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static GorgonRangeF Divide(GorgonRangeF left, float scalar)
		{
			return new GorgonRangeF(left.Minimum / scalar, left.Minimum / scalar);
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
			var equate = obj as IEquatable<GorgonRangeF>;

			return equate != null && equate.Equals(this);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Minimum).GenerateHash(Maximum);
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
		/// Initializes a new instance of the <see cref="GorgonRangeF" /> struct.
		/// </summary>
		/// <param name="minMax">The min max value to copy.</param>
		public GorgonRangeF(GorgonRangeF minMax)
			: this(minMax.Minimum, minMax.Maximum)
		{		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRangeF"/> struct.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public GorgonRangeF(float min, float max)
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
		/// Performs an explicit conversion from <see cref="GorgonRangeF"/> to <see cref="GorgonRange"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRange(GorgonRangeF range)
		{
			return new GorgonRange((int)range.Minimum, (int)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRangeF"/> to <see cref="GorgonRangeM"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator GorgonRangeM(GorgonRangeF range)
		{
			return new GorgonRangeM((decimal)range.Minimum, (decimal)range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRangeF"/> to <see cref="GorgonRangeD"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonRangeD(GorgonRangeF range)
		{
			return new GorgonRangeD(range.Minimum, range.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonRangeF left, GorgonRangeF right)
		{
			return left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonRangeF left, GorgonRangeF right)
		{
			return !(left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum));
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeF operator +(GorgonRangeF left, GorgonRangeF right)
		{
			GorgonRangeF result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeF operator -(GorgonRangeF left, GorgonRangeF right)
		{
			GorgonRangeF result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeF operator *(GorgonRangeF left, GorgonRangeF right)
		{
			GorgonRangeF result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeF operator *(GorgonRangeF left, float scalar)
		{
			GorgonRangeF result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="right">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeF operator *(float scalar, GorgonRangeF right)
		{
			GorgonRangeF result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRangeF operator /(GorgonRangeF left, float scalar)
		{
			GorgonRangeF result;
			Divide(ref left, scalar, out result);
			return result;
		}

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator >(GorgonRangeF left, GorgonRangeF right)
        {
            return left.Range > right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator >=(GorgonRangeF left, GorgonRangeF right)
        {
            return left.Range >= right.Range;
        }

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <(GorgonRangeF left, GorgonRangeF right)
        {
            return left.Range < right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <=(GorgonRangeF left, GorgonRangeF right)
        {
            return left.Range <= right.Range;
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
		public bool Equals(GorgonRangeF other)
		{
			return other.Minimum.EqualsEpsilon(Minimum) && other.Maximum.EqualsEpsilon(Maximum);
		}
		#endregion

        #region IComparable<GorgonRangeF> Members
        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(GorgonRangeF other)
        {
            return Range.CompareTo(other.Range);
        }
        #endregion
	}
	#endregion

	#region Int32
	/// <summary>
	/// Value type to indicate a range of integer values.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct GorgonRange
		: IEquatable<GorgonRange>, IComparable<GorgonRange>
	{
		#region Variables.
		/// <summary>
		/// Minimum value in the range.
		/// </summary>
		public readonly int Minimum;
		/// <summary>
		/// Maximum value in the range.
		/// </summary>
		public readonly int Maximum;

		/// <summary>
		/// Empty range.
		/// </summary>
		public static readonly GorgonRange Empty = new GorgonRange(0, 0);
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the range is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ((Maximum == 0) && (Minimum == 0));
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

        #region Methods.
        /// <summary>
		/// Function to return whether the value falls within the range.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range.</returns>
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
		public static void Shrink(ref GorgonRange range, int value, out GorgonRange result)
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

			result = new GorgonRange(min, max);
		}

		/// <summary>
		/// Function to shrink the range.
		/// </summary>
		/// <param name="range">Range to shrink.</param>
		/// <param name="value">Amount to shrink the range.</param>		
		public static GorgonRange Shrink(GorgonRange range, int value)
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

			return new GorgonRange(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <param name="result">The larger range.</param>
		public static void Expand(ref GorgonRange range, int value, out GorgonRange result)
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

			result = new GorgonRange(min, max);
		}

		/// <summary>
		/// Function to expand the range.
		/// </summary>
		/// <param name="range">Range to expand.</param>
		/// <param name="value">Amount to expand the range.</param>
		/// <returns>The larger range.</returns>
		public static GorgonRange Expand(GorgonRange range, int value)
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

			return new GorgonRange(min, max);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <param name="result">The total of both ranges.</param>
		public static void Add(ref GorgonRange left, ref GorgonRange right, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to add two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to add</param>
		/// <param name="right">Right range to add.</param>
		/// <returns>The total of both ranges.</returns>
		public static GorgonRange Add(GorgonRange left, GorgonRange right)
		{
			return new GorgonRange(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <param name="result">The difference of both ranges.</param>
		public static void Subtract(ref GorgonRange left, ref GorgonRange right, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to subtract two min-max ranges from each other.
		/// </summary>
		/// <param name="left">Left range to subtract.</param>
		/// <param name="right">Right range to subtract.</param>
		/// <returns>The difference of both ranges.</returns>
		public static GorgonRange Subtract(GorgonRange left, GorgonRange right)
		{
			return new GorgonRange(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <param name="result">The product of both ranges.</param>
		public static void Multiply(ref GorgonRange left, ref GorgonRange right, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply two min-max ranges together.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="right">Right range to multiply.</param>
		/// <returns>The product of both ranges.</returns>
		public static GorgonRange Multiply(GorgonRange left, GorgonRange right)
		{
			return new GorgonRange(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <param name="result">The product of the range and the scalar.</param>
		public static void Multiply(ref GorgonRange left, int scalar, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum + scalar, left.Maximum + scalar);
		}

		/// <summary>
		/// Function to multiply a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to multiply</param>
		/// <param name="scalar">Scalar value to multiply..</param>
		/// <returns>The product of the range and the scalar.</returns>
		public static GorgonRange Multiply(GorgonRange left, int scalar)
		{
			return new GorgonRange(left.Minimum * scalar, left.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <param name="result">The quotient of the range and the scalar.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static void Divide(ref GorgonRange left, int scalar, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum / scalar, left.Maximum / scalar);
		}

		/// <summary>
		/// Function to divide a min-max range by a scalar value.
		/// </summary>
		/// <param name="left">Left range to divide.</param>
		/// <param name="scalar">Scalar value to divide.</param>
		/// <returns>The quotient of the range and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown if the scalar value is zero.</exception>
		public static GorgonRange Divide(GorgonRange left, int scalar)
		{
			return new GorgonRange(left.Minimum / scalar, left.Minimum / scalar);
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
			var equate = obj as IEquatable<GorgonRange>;

		    return equate != null && equate.Equals(this);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Maximum).GenerateHash(Minimum);
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
		/// Initializes a new instance of the <see cref="GorgonRange" /> struct.
		/// </summary>
		/// <param name="minMax">The min max value to copy.</param>
		public GorgonRange(GorgonRange minMax)
			: this(minMax.Minimum, minMax.Maximum)
		{		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRange"/> struct.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public GorgonRange(int min, int max)
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
		/// Performs an explicit conversion from <see cref="GorgonRange"/> to <see cref="GorgonRangeF"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonRangeF(GorgonRange range)
		{
			return new GorgonRangeF(range.Minimum, range.Maximum);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRange"/> to <see cref="GorgonRangeM"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonRangeM(GorgonRange range)
		{
			return new GorgonRangeM(range.Minimum, range.Maximum);
		}


		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRange"/> to <see cref="GorgonRangeD"/>.
		/// </summary>
		/// <param name="range">The range.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonRangeD(GorgonRange range)
		{
			return new GorgonRangeD(range.Minimum, range.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonRange left, GorgonRange right)
		{
			return left.Minimum == right.Minimum && left.Maximum == right.Maximum;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonRange left, GorgonRange right)
		{
			return !(left.Minimum == right.Minimum && left.Maximum == right.Maximum);
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRange operator +(GorgonRange left, GorgonRange right)
		{
			GorgonRange result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRange operator -(GorgonRange left, GorgonRange right)
		{
			GorgonRange result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="right">The right range.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRange operator *(GorgonRange left, GorgonRange right)
		{
			GorgonRange result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRange operator *(GorgonRange left, int scalar)
		{
			GorgonRange result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar value.</param>
		/// <param name="right">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRange operator *(int scalar, GorgonRange right)
		{
			GorgonRange result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="left">The left range.</param>
		/// <param name="scalar">The right scalar value.</param>
		/// <returns>The result of the operator.</returns>
		public static GorgonRange operator /(GorgonRange left, int scalar)
		{
			GorgonRange result;
			Divide(ref left, scalar, out result);
			return result;
		}

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
	    public static bool operator >(GorgonRange left, GorgonRange right)
        {
            return left.Range > right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator >=(GorgonRange left, GorgonRange right)
        {
            return left.Range >= right.Range;
        }

        /// <summary>
        /// Implements the operator >.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <(GorgonRange left, GorgonRange right)
        {
            return left.Range < right.Range;
        }

        /// <summary>
        /// Implements the operator >=.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if left is greater than right.</returns>
        public static bool operator <=(GorgonRange left, GorgonRange right)
        {
            return left.Range <= right.Range;
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
		public bool Equals(GorgonRange other)
		{
			return (Minimum == other.Minimum) && (Maximum == other.Maximum);
		}
		#endregion

        #region IComparable<GorgonRange> Members
        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(GorgonRange other)
        {
            return Range.CompareTo(other.Range);
        }
        #endregion
	}
	#endregion
}
