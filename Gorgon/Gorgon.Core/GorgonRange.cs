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
	/// A type that represents a range between two <see cref="double"/> values.
	/// </summary>
	/// <remarks>
	/// This a means to determine the range between a minimum <see cref="double"/> value and a maximum <see cref="double"/> value. Use this object to determine if a value falls within a specific range of 
	/// values.
	/// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct GorgonRangeD
		: IEquatable<GorgonRangeD>, IComparable<GorgonRangeD>
	{
		#region Variables.
		/// <summary>
		/// The minimum value in the range.
		/// </summary>
		public readonly double Minimum;
		/// <summary>
		/// The maximum value in the range.
		/// </summary>
		public readonly double Maximum;

		/// <summary>
		/// An empty range value.
		/// </summary>
		public static readonly GorgonRangeD Empty = new GorgonRangeD(0.0, 0.0);
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the range is empty or not.
        /// </summary>
        public bool IsEmpty => ((Maximum.EqualsEpsilon(0.0)) && (Minimum.EqualsEpsilon(0.0)));

		/// <summary>
        /// Property to return the range between the two values.
        /// </summary>
        public double Range => (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);

		#endregion

        #region Methods.
        /// <summary>
		/// Function to return whether the <see cref="double"/> value falls within this <see cref="GorgonRangeD"/>.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range, <b>false</b> if not.</returns>
		public bool Contains(double value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink a <see cref="GorgonRangeD"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRangeD"/> to shrink.</param>
		/// <param name="amount">The amount to shrink the <see cref="GorgonRangeD"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRangeD"/> value, decreased in size by <paramref name="amount"/>.</param>
		public static void Shrink(ref GorgonRangeD range, double amount, out GorgonRangeD result)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			result = new GorgonRangeD(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="amount"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>		
		/// <returns>A new <see cref="GorgonRangeD"/> value, decreased in size by <paramref name="amount"/>.</returns>
		public static GorgonRangeD Shrink(GorgonRangeD range, double amount)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			return new GorgonRangeD(min, max);
		}

		/// <summary>
		/// Function to expand a <see cref="GorgonRangeD"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRangeD"/> to expand.</param>
		/// <param name="amount">The amount to expand the <see cref="GorgonRangeD"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRangeD"/> value, increased in size by <paramref name="amount"/>.</param>
		public static void Expand(ref GorgonRangeD range, double amount, out GorgonRangeD result)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			result = new GorgonRangeD(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="amount"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <returns>A new <see cref="GorgonRangeD"/> value, increased in size by <paramref name="amount"/>.</returns>
		public static GorgonRangeD Expand(GorgonRangeD range, double amount)
		{
			double min;
			double max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			return new GorgonRangeD(min, max);
		}

		/// <summary>
		/// Function to add two <see cref="GorgonRangeD"/> values together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeD"/> value to add</param>
		/// <param name="right">The right <see cref="GorgonRangeD"/> value to add.</param>
		/// <param name="result">A new <see cref="GorgonRangeD"/> representing the total of both ranges.</param>
		public static void Add(ref GorgonRangeD left, ref GorgonRangeD right, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="right"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <returns>A new <see cref="GorgonRangeD"/> representing the total of both ranges.</returns>
		public static GorgonRangeD Add(GorgonRangeD left, GorgonRangeD right)
		{
			return new GorgonRangeD(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two <see cref="GorgonRangeD"/> ranges from each other.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeD"/> value to subtract.</param>
		/// <param name="right">The right <see cref="GorgonRangeD"/> value to subtract.</param>
		/// <param name="result">A new <see cref="GorgonRangeD"/> value representing the difference of both ranges.</param>
		public static void Subtract(ref GorgonRangeD left, ref GorgonRangeD right, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="right"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <returns>A new <see cref="GorgonRangeD"/> value representing the difference of both ranges.</returns>
		public static GorgonRangeD Subtract(GorgonRangeD left, GorgonRangeD right)
		{
			return new GorgonRangeD(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two <see cref="GorgonRangeD"/> ranges together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeD"/> value to multiply.</param>
		/// <param name="right">The right <see cref="GorgonRangeD"/> value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRangeD"/> value representing the product of both ranges.</param>
		public static void Multiply(ref GorgonRangeD left, ref GorgonRangeD right, out GorgonRangeD result)
		{
			result = new GorgonRangeD(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="right"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <returns>A new <see cref="GorgonRangeD"/> value representing the product of both ranges.</returns>
		public static GorgonRangeD Multiply(GorgonRangeD left, GorgonRangeD right)
		{
			return new GorgonRangeD(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a <see cref="GorgonRangeD"/> by a scalar <see cref="double"/> value.
		/// </summary>
		/// <param name="range"><see cref="Multiply(ref Gorgon.Core.GorgonRangeD,ref Gorgon.Core.GorgonRangeD,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="scalar">The <see cref="double"/> scalar value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRangeD"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		public static void Multiply(ref GorgonRangeD range, double scalar, out GorgonRangeD result)
		{
			result = new GorgonRangeD(range.Minimum * scalar, range.Maximum * scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="scalar"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <returns>A new <see cref="GorgonRangeD"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		public static GorgonRangeD Multiply(GorgonRangeD range, double scalar)
		{
			return new GorgonRangeD(range.Minimum * scalar, range.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a <see cref="GorgonRangeD"/> by a <see cref="double"/> value.
		/// </summary>
		/// <param name="range">The <see cref="GorgonRangeD"/> range value to divide.</param>
		/// <param name="scalar">The <see cref="double"/> scalar value to divide by.</param>
		/// <param name="result">A new <see cref="GorgonRangeD"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the <paramref name="scalar"/> value is zero.</exception>
		public static void Divide(ref GorgonRangeD range, double scalar, out GorgonRangeD result)
		{
			result = new GorgonRangeD(range.Minimum / scalar, range.Maximum / scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <param name="scalar"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></param>
		/// <returns>A new <see cref="GorgonRangeD"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		/// <exception cref="System.DivideByZeroException"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeD,double,out Gorgon.Core.GorgonRangeD)"/></exception>
		public static GorgonRangeD Divide(GorgonRangeD range, double scalar)
		{
			return new GorgonRangeD(range.Minimum / scalar, range.Minimum / scalar);
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
			if (obj is GorgonRangeD)
			{
				return ((GorgonRangeD)obj).Equals(this);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return Range.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_TOSTR_GORGONRANGE, Minimum, Maximum, Range);
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
	/// A type that represents a range between two <see cref="decimal"/> values.
	/// </summary>
	/// <remarks>
	/// This a means to determine the range between a minimum <see cref="decimal"/> value and a maximum <see cref="decimal"/> value. Use this object to determine if a value falls within a specific range of 
	/// values.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonRangeM
		: IEquatable<GorgonRangeM>, IComparable<GorgonRangeM>
	{
		#region Variables.
		/// <summary>
		/// The minimum value in the range.
		/// </summary>
		public readonly decimal Minimum;
		/// <summary>
		/// The maximum value in the range.
		/// </summary>
		public readonly decimal Maximum;

		/// <summary>
		/// An empty range value.
		/// </summary>
		public static readonly GorgonRangeM Empty = new GorgonRangeM(0.0M, 0.0M);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the range is empty or not.
		/// </summary>
		public bool IsEmpty => Maximum == 0 && Minimum == 0;

		/// <summary>
		/// Property to return the range between the two values.
		/// </summary>
		public decimal Range => (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);

		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether the <see cref="decimal"/> value falls within this <see cref="GorgonRangeM"/>.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range, <b>false</b> if not.</returns>
		public bool Contains(decimal value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink a <see cref="GorgonRangeM"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRangeM"/> to shrink.</param>
		/// <param name="amount">The amount to shrink the <see cref="GorgonRangeM"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRangeM"/> value, decreased in size by <paramref name="amount"/>.</param>
		public static void Shrink(ref GorgonRangeM range, decimal amount, out GorgonRangeM result)
		{
			decimal min;
			decimal max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			result = new GorgonRangeM(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="amount"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>		
		/// <returns>A new <see cref="GorgonRangeM"/> value, decreased in size by <paramref name="amount"/>.</returns>
		public static GorgonRangeM Shrink(GorgonRangeM range, decimal amount)
		{
			decimal min;
			decimal max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			return new GorgonRangeM(min, max);
		}

		/// <summary>
		/// Function to expand a <see cref="GorgonRangeM"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRangeM"/> to expand.</param>
		/// <param name="amount">The amount to expand the <see cref="GorgonRangeM"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRangeM"/> value, increased in size by <paramref name="amount"/>.</param>
		public static void Expand(ref GorgonRangeM range, decimal amount, out GorgonRangeM result)
		{
			decimal min;
			decimal max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			result = new GorgonRangeM(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="amount"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <returns>A new <see cref="GorgonRangeM"/> value, increased in size by <paramref name="amount"/>.</returns>
		public static GorgonRangeM Expand(GorgonRangeM range, decimal amount)
		{
			decimal min;
			decimal max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			return new GorgonRangeM(min, max);
		}

		/// <summary>
		/// Function to add two <see cref="GorgonRangeM"/> values together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeM"/> value to add</param>
		/// <param name="right">The right <see cref="GorgonRangeM"/> value to add.</param>
		/// <param name="result">A new <see cref="GorgonRangeM"/> representing the total of both ranges.</param>
		public static void Add(ref GorgonRangeM left, ref GorgonRangeM right, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="right"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <returns>A new <see cref="GorgonRangeM"/> representing the total of both ranges.</returns>
		public static GorgonRangeM Add(GorgonRangeM left, GorgonRangeM right)
		{
			return new GorgonRangeM(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two <see cref="GorgonRangeM"/> ranges from each other.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeM"/> value to subtract.</param>
		/// <param name="right">The right <see cref="GorgonRangeM"/> value to subtract.</param>
		/// <param name="result">A new <see cref="GorgonRangeM"/> value representing the difference of both ranges.</param>
		public static void Subtract(ref GorgonRangeM left, ref GorgonRangeM right, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="right"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <returns>A new <see cref="GorgonRangeM"/> value representing the difference of both ranges.</returns>
		public static GorgonRangeM Subtract(GorgonRangeM left, GorgonRangeM right)
		{
			return new GorgonRangeM(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two <see cref="GorgonRangeM"/> ranges together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeM"/> value to multiply.</param>
		/// <param name="right">The right <see cref="GorgonRangeM"/> value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRangeM"/> value representing the product of both ranges.</param>
		public static void Multiply(ref GorgonRangeM left, ref GorgonRangeM right, out GorgonRangeM result)
		{
			result = new GorgonRangeM(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="right"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <returns>A new <see cref="GorgonRangeM"/> value representing the product of both ranges.</returns>
		public static GorgonRangeM Multiply(GorgonRangeM left, GorgonRangeM right)
		{
			return new GorgonRangeM(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a <see cref="GorgonRangeM"/> by a scalar <see cref="decimal"/> value.
		/// </summary>
		/// <param name="range"><see cref="Multiply(ref Gorgon.Core.GorgonRangeM,ref Gorgon.Core.GorgonRangeM,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="scalar">The <see cref="decimal"/> scalar value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRangeM"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		public static void Multiply(ref GorgonRangeM range, decimal scalar, out GorgonRangeM result)
		{
			result = new GorgonRangeM(range.Minimum * scalar, range.Maximum * scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="scalar"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <returns>A new <see cref="GorgonRangeM"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		public static GorgonRangeM Multiply(GorgonRangeM range, decimal scalar)
		{
			return new GorgonRangeM(range.Minimum * scalar, range.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a <see cref="GorgonRangeM"/> by a <see cref="decimal"/> value.
		/// </summary>
		/// <param name="range">The <see cref="GorgonRangeM"/> range value to divide.</param>
		/// <param name="scalar">The <see cref="decimal"/> scalar value to divide by.</param>
		/// <param name="result">A new <see cref="GorgonRangeM"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the <paramref name="scalar"/> value is zero.</exception>
		public static void Divide(ref GorgonRangeM range, decimal scalar, out GorgonRangeM result)
		{
			result = new GorgonRangeM(range.Minimum / scalar, range.Maximum / scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <param name="scalar"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></param>
		/// <returns>A new <see cref="GorgonRangeM"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		/// <exception cref="System.DivideByZeroException"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeM,decimal,out Gorgon.Core.GorgonRangeM)"/></exception>
		public static GorgonRangeM Divide(GorgonRangeM range, decimal scalar)
		{
			return new GorgonRangeM(range.Minimum / scalar, range.Minimum / scalar);
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
			if (obj is GorgonRangeM)
			{
				return ((GorgonRangeM)obj).Equals(this);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return Range.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_TOSTR_GORGONRANGE, Minimum, Maximum, Range);
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
			return Minimum == other.Minimum && Maximum == other.Maximum;
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
	/// A type that represents a range between two <see cref="float"/> values.
	/// </summary>
	/// <remarks>
	/// This a means to determine the range between a minimum <see cref="float"/> value and a maximum <see cref="float"/> value. Use this object to determine if a value falls within a specific range of 
	/// values.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonRangeF
		: IEquatable<GorgonRangeF>, IComparable<GorgonRangeF>
	{
		#region Variables.
		/// <summary>
		/// The minimum value in the range.
		/// </summary>
		public readonly float Minimum;
		/// <summary>
		/// The maximum value in the range.
		/// </summary>
		public readonly float Maximum;

		/// <summary>
		/// An empty range value.
		/// </summary>
		public static readonly GorgonRangeF Empty = new GorgonRangeF(0.0f, 0.0f);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the range is empty or not.
		/// </summary>
		public bool IsEmpty => ((Maximum.EqualsEpsilon(0.0f)) && (Minimum.EqualsEpsilon(0.0f)));

		/// <summary>
		/// Property to return the range between the two values.
		/// </summary>
		public float Range => (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);

		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether the <see cref="float"/> value falls within this <see cref="GorgonRangeF"/>.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range, <b>false</b> if not.</returns>
		public bool Contains(float value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink a <see cref="GorgonRangeF"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRangeF"/> to shrink.</param>
		/// <param name="amount">The amount to shrink the <see cref="GorgonRangeF"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRangeF"/> value, decreased in size by <paramref name="amount"/>.</param>
		public static void Shrink(ref GorgonRangeF range, float amount, out GorgonRangeF result)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			result = new GorgonRangeF(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="amount"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>		
		/// <returns>A new <see cref="GorgonRangeF"/> value, decreased in size by <paramref name="amount"/>.</returns>
		public static GorgonRangeF Shrink(GorgonRangeF range, float amount)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			return new GorgonRangeF(min, max);
		}

		/// <summary>
		/// Function to expand a <see cref="GorgonRangeF"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRangeF"/> to expand.</param>
		/// <param name="amount">The amount to expand the <see cref="GorgonRangeF"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRangeF"/> value, increased in size by <paramref name="amount"/>.</param>
		public static void Expand(ref GorgonRangeF range, float amount, out GorgonRangeF result)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			result = new GorgonRangeF(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="amount"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <returns>A new <see cref="GorgonRangeF"/> value, increased in size by <paramref name="amount"/>.</returns>
		public static GorgonRangeF Expand(GorgonRangeF range, float amount)
		{
			float min;
			float max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			return new GorgonRangeF(min, max);
		}

		/// <summary>
		/// Function to add two <see cref="GorgonRangeF"/> values together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeF"/> value to add</param>
		/// <param name="right">The right <see cref="GorgonRangeF"/> value to add.</param>
		/// <param name="result">A new <see cref="GorgonRangeF"/> representing the total of both ranges.</param>
		public static void Add(ref GorgonRangeF left, ref GorgonRangeF right, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="right"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <returns>A new <see cref="GorgonRangeF"/> representing the total of both ranges.</returns>
		public static GorgonRangeF Add(GorgonRangeF left, GorgonRangeF right)
		{
			return new GorgonRangeF(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two <see cref="GorgonRangeF"/> ranges from each other.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeF"/> value to subtract.</param>
		/// <param name="right">The right <see cref="GorgonRangeF"/> value to subtract.</param>
		/// <param name="result">A new <see cref="GorgonRangeF"/> value representing the difference of both ranges.</param>
		public static void Subtract(ref GorgonRangeF left, ref GorgonRangeF right, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="right"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <returns>A new <see cref="GorgonRangeF"/> value representing the difference of both ranges.</returns>
		public static GorgonRangeF Subtract(GorgonRangeF left, GorgonRangeF right)
		{
			return new GorgonRangeF(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two <see cref="GorgonRangeF"/> ranges together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRangeF"/> value to multiply.</param>
		/// <param name="right">The right <see cref="GorgonRangeF"/> value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRangeF"/> value representing the product of both ranges.</param>
		public static void Multiply(ref GorgonRangeF left, ref GorgonRangeF right, out GorgonRangeF result)
		{
			result = new GorgonRangeF(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="right"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <returns>A new <see cref="GorgonRangeF"/> value representing the product of both ranges.</returns>
		public static GorgonRangeF Multiply(GorgonRangeF left, GorgonRangeF right)
		{
			return new GorgonRangeF(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a <see cref="GorgonRangeF"/> by a scalar <see cref="float"/> value.
		/// </summary>
		/// <param name="range"><see cref="Multiply(ref Gorgon.Core.GorgonRangeF,ref Gorgon.Core.GorgonRangeF,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="scalar">The <see cref="float"/> scalar value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRangeF"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		public static void Multiply(ref GorgonRangeF range, float scalar, out GorgonRangeF result)
		{
			result = new GorgonRangeF(range.Minimum * scalar, range.Maximum * scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="scalar"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <returns>A new <see cref="GorgonRangeF"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		public static GorgonRangeF Multiply(GorgonRangeF range, float scalar)
		{
			return new GorgonRangeF(range.Minimum * scalar, range.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a <see cref="GorgonRangeF"/> by a <see cref="float"/> value.
		/// </summary>
		/// <param name="range">The <see cref="GorgonRangeF"/> range value to divide.</param>
		/// <param name="scalar">The <see cref="float"/> scalar value to divide by.</param>
		/// <param name="result">A new <see cref="GorgonRangeF"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the <paramref name="scalar"/> value is zero.</exception>
		public static void Divide(ref GorgonRangeF range, float scalar, out GorgonRangeF result)
		{
			result = new GorgonRangeF(range.Minimum / scalar, range.Maximum / scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <param name="scalar"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></param>
		/// <returns>A new <see cref="GorgonRangeF"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		/// <exception cref="System.DivideByZeroException"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRangeF,float,out Gorgon.Core.GorgonRangeF)"/></exception>
		public static GorgonRangeF Divide(GorgonRangeF range, float scalar)
		{
			return new GorgonRangeF(range.Minimum / scalar, range.Minimum / scalar);
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
			if (obj is GorgonRangeF)
			{
				return ((GorgonRangeF)obj).Equals(this);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return Range.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_TOSTR_GORGONRANGE, Minimum, Maximum, Range);
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

		#region IEquatable<GorgonMinMaxD> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonRangeF other)
		{
			return (Minimum.EqualsEpsilon(other.Minimum)) && (Maximum.EqualsEpsilon(other.Maximum));
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
	/// A type that represents a range between two <see cref="int"/> values.
	/// </summary>
	/// <remarks>
	/// This a means to determine the range between a minimum <see cref="int"/> value and a maximum <see cref="int"/> value. Use this object to determine if a value falls within a specific range of 
	/// values.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GorgonRange
		: IEquatable<GorgonRange>, IComparable<GorgonRange>
	{
		#region Variables.
		/// <summary>
		/// The minimum value in the range.
		/// </summary>
		public readonly int Minimum;
		/// <summary>
		/// The maximum value in the range.
		/// </summary>
		public readonly int Maximum;

		/// <summary>
		/// An empty range value.
		/// </summary>
		public static readonly GorgonRange Empty = new GorgonRange(0, 0);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the range is empty or not.
		/// </summary>
		public bool IsEmpty => Maximum == 0 && Minimum == 0;

		/// <summary>
		/// Property to return the range between the two values.
		/// </summary>
		public int Range => (Minimum < Maximum) ? (Maximum - Minimum) : (Minimum - Maximum);

		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether the <see cref="int"/> value falls within this <see cref="GorgonRange"/>.
		/// </summary>
		/// <param name="value">Value to test.</param>
		/// <returns><b>true</b> if the value falls into the range, <b>false</b> if not.</returns>
		public bool Contains(int value)
		{
			return (Minimum < Maximum) ? (value >= Minimum && value <= Maximum) : (value >= Maximum && value <= Minimum);
		}

		/// <summary>
		/// Function to shrink a <see cref="GorgonRange"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRange"/> to shrink.</param>
		/// <param name="amount">The amount to shrink the <see cref="GorgonRange"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRange"/> value, decreased in size by <paramref name="amount"/>.</param>
		public static void Shrink(ref GorgonRange range, int amount, out GorgonRange result)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			result = new GorgonRange(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="amount"><inheritdoc cref="Shrink(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>		
		/// <returns>A new <see cref="GorgonRange"/> value, decreased in size by <paramref name="amount"/>.</returns>
		public static GorgonRange Shrink(GorgonRange range, int amount)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}
			else
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}

			return new GorgonRange(min, max);
		}

		/// <summary>
		/// Function to expand a <see cref="GorgonRange"/> by a specific amount.
		/// </summary>
		/// <param name="range">A <see cref="GorgonRange"/> to expand.</param>
		/// <param name="amount">The amount to expand the <see cref="GorgonRange"/> by.</param>
		/// <param name="result">A new <see cref="GorgonRange"/> value, increased in size by <paramref name="amount"/>.</param>
		public static void Expand(ref GorgonRange range, int amount, out GorgonRange result)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			result = new GorgonRange(min, max);
		}

		/// <summary>
		/// <inheritdoc cref="Expand(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="amount"><inheritdoc cref="Expand(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>
		/// <returns>A new <see cref="GorgonRange"/> value, increased in size by <paramref name="amount"/>.</returns>
		public static GorgonRange Expand(GorgonRange range, int amount)
		{
			int min;
			int max;

			if (range.Minimum < range.Maximum)
			{
				min = range.Minimum - amount;
				max = range.Maximum + amount;
			}
			else
			{
				min = range.Minimum + amount;
				max = range.Maximum - amount;
			}

			return new GorgonRange(min, max);
		}

		/// <summary>
		/// Function to add two <see cref="GorgonRange"/> values together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRange"/> value to add</param>
		/// <param name="right">The right <see cref="GorgonRange"/> value to add.</param>
		/// <param name="result">A new <see cref="GorgonRange"/> representing the total of both ranges.</param>
		public static void Add(ref GorgonRange left, ref GorgonRange right, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Add(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="right"><inheritdoc cref="Add(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/></param>
		/// <returns>A new <see cref="GorgonRange"/> representing the total of both ranges.</returns>
		public static GorgonRange Add(GorgonRange left, GorgonRange right)
		{
			return new GorgonRange(left.Minimum + right.Minimum, left.Maximum + right.Maximum);
		}

		/// <summary>
		/// Function to subtract two <see cref="GorgonRange"/> ranges from each other.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRange"/> value to subtract.</param>
		/// <param name="right">The right <see cref="GorgonRange"/> value to subtract.</param>
		/// <param name="result">A new <see cref="GorgonRange"/> value representing the difference of both ranges.</param>
		public static void Subtract(ref GorgonRange left, ref GorgonRange right, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="right"><inheritdoc cref="Subtract(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/></param>
		/// <returns>A new <see cref="GorgonRange"/> value representing the difference of both ranges.</returns>
		public static GorgonRange Subtract(GorgonRange left, GorgonRange right)
		{
			return new GorgonRange(left.Minimum - right.Minimum, left.Maximum - right.Maximum);
		}

		/// <summary>
		/// Function to multiply two <see cref="GorgonRange"/> ranges together.
		/// </summary>
		/// <param name="left">The left <see cref="GorgonRange"/> value to multiply.</param>
		/// <param name="right">The right <see cref="GorgonRange"/> value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRange"/> value representing the product of both ranges.</param>
		public static void Multiply(ref GorgonRange left, ref GorgonRange right, out GorgonRange result)
		{
			result = new GorgonRange(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/>
		/// </summary>
		/// <param name="left"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="right"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/></param>
		/// <returns>A new <see cref="GorgonRange"/> value representing the product of both ranges.</returns>
		public static GorgonRange Multiply(GorgonRange left, GorgonRange right)
		{
			return new GorgonRange(left.Minimum * right.Minimum, left.Maximum * right.Maximum);
		}

		/// <summary>
		/// Function to multiply a <see cref="GorgonRange"/> by a scalar <see cref="int"/> value.
		/// </summary>
		/// <param name="range"><see cref="Multiply(ref Gorgon.Core.GorgonRange,ref Gorgon.Core.GorgonRange,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="scalar">The <see cref="int"/> scalar value to multiply.</param>
		/// <param name="result">A new <see cref="GorgonRange"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		public static void Multiply(ref GorgonRange range, int scalar, out GorgonRange result)
		{
			result = new GorgonRange(range.Minimum * scalar, range.Maximum * scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="scalar"><inheritdoc cref="Multiply(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>
		/// <returns>A new <see cref="GorgonRange"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		public static GorgonRange Multiply(GorgonRange range, int scalar)
		{
			return new GorgonRange(range.Minimum * scalar, range.Minimum * scalar);
		}

		/// <summary>
		/// Function to divide a <see cref="GorgonRange"/> by a <see cref="int"/> value.
		/// </summary>
		/// <param name="range">The <see cref="GorgonRange"/> range value to divide.</param>
		/// <param name="scalar">The <see cref="int"/> scalar value to divide by.</param>
		/// <param name="result">A new <see cref="GorgonRange"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</param>
		/// <exception cref="System.DivideByZeroException">Thrown if the <paramref name="scalar"/> value is zero.</exception>
		public static void Divide(ref GorgonRange range, int scalar, out GorgonRange result)
		{
			result = new GorgonRange(range.Minimum / scalar, range.Maximum / scalar);
		}

		/// <summary>
		/// <inheritdoc cref="Divide(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/>
		/// </summary>
		/// <param name="range"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>
		/// <param name="scalar"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></param>
		/// <returns>A new <see cref="GorgonRange"/> representing the product of the <paramref name="range"/> and the <paramref name="scalar"/>.</returns>
		/// <exception cref="System.DivideByZeroException"><inheritdoc cref="Divide(ref Gorgon.Core.GorgonRange,int,out Gorgon.Core.GorgonRange)"/></exception>
		public static GorgonRange Divide(GorgonRange range, int scalar)
		{
			return new GorgonRange(range.Minimum / scalar, range.Minimum / scalar);
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
			if (obj is GorgonRange)
			{
				return ((GorgonRange)obj).Equals(this);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return Range.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_TOSTR_GORGONRANGE, Minimum, Maximum, Range);
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
			return left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonRange left, GorgonRange right)
		{
			return !(left.Minimum.Equals(right.Minimum) && left.Maximum.Equals(right.Maximum));
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

		#region IEquatable<GorgonMinMaxD> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonRange other)
		{
			return Minimum == other.Minimum && Maximum == other.Maximum;
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
