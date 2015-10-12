#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, October 11, 2015 11:51:54 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace Gorgon.Math
{
	/// <summary>
	/// A representation of a rational number.
	/// </summary>
	public struct GorgonRationalNumber
		: IEquatable<GorgonRationalNumber>, IComparable<GorgonRationalNumber>
	{
		#region Variables.
		/// <summary>
		/// An empty rational number.
		/// </summary>
		public static readonly GorgonRationalNumber Empty = new GorgonRationalNumber();

		/// <summary>
		/// The numerator for the number.
		/// </summary>
		public readonly int Numerator;

		/// <summary>
		/// The denominator for the number.
		/// </summary>
		public readonly int Denominator;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare two instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if the instances are equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonRationalNumber left, ref GorgonRationalNumber right)
		{
			return left.Numerator == right.Numerator
			       && left.Denominator == right.Denominator;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Denominator == 0
				       ? string.Format(Resources.GOR_TOSTR_RATIONAL, Numerator, Denominator, "NaN")
				       : string.Format(Resources.GOR_TOSTR_RATIONAL, Numerator, Denominator, (decimal)Numerator / Denominator);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Numerator).GenerateHash(Denominator);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is GorgonRationalNumber)
			{
				return ((GorgonRationalNumber)obj).Equals(this);
			}
			return base.Equals(obj);
		}

		/// <inheritdoc/>
		public bool Equals(GorgonRationalNumber other)
		{
			return Equals(ref this, ref other);
		}

		/// <inheritdoc/>
		public int CompareTo(GorgonRationalNumber other)
		{
			if (Equals(ref this, ref other))
			{
				return 0;
			}

			if (this < other)
			{
				return -1;
			}

			return 1;
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Performs an implicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="decimal"/>.
		/// </summary>
		/// <param name="value">The value convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator decimal(GorgonRationalNumber value)
		{
			return (decimal)value.Numerator / value.Denominator;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="double"/>.
		/// </summary>
		/// <param name="value">The value convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator double (GorgonRationalNumber value)
		{
			return (double)value.Numerator / value.Denominator;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="float"/>.
		/// </summary>
		/// <param name="value">The value convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator float (GorgonRationalNumber value)
		{
			return (float)value.Numerator / value.Denominator;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="int"/>.
		/// </summary>
		/// <param name="value">The value convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator int (GorgonRationalNumber value)
		{
			return value.Numerator / value.Denominator;
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="int"/> to <see cref="GorgonRationalNumber"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator GorgonRationalNumber(int value)
		{
			return new GorgonRationalNumber(value, 1);
		}

		/// <summary>
		/// Operator to compare two instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonRationalNumber left, GorgonRationalNumber right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to compare two instances for inequality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonRationalNumber left, GorgonRationalNumber right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to determine if the left rational is less than the right rational.
		/// </summary>
		/// <param name="left">Left rational to compare.</param>
		/// <param name="right">Right rational to compare.</param>
		/// <returns><b>true</b> if the <paramref name="left"/> is less than the <paramref name="right"/>, <b>false</b> if not.</returns>
		public static bool operator <(GorgonRationalNumber left, GorgonRationalNumber right)
		{
			decimal leftRational = left;
			decimal rightRational = right;

			return leftRational < rightRational;
		}

		/// <summary>
		/// Operator to determine if the left rational is less than or equal to the right rational.
		/// </summary>
		/// <param name="left">Left rational to compare.</param>
		/// <param name="right">Right rational to compare.</param>
		/// <returns><b>true</b> if the <paramref name="left"/> is less than or equal to the <paramref name="right"/>, <b>false</b> if not.</returns>
		public static bool operator <=(GorgonRationalNumber left, GorgonRationalNumber right)
		{
			decimal leftRational = left;
			decimal rightRational = right;

			return leftRational <= rightRational;
		}

		/// <summary>
		/// Operator to determine if the left rational is greater than the right rational.
		/// </summary>
		/// <param name="left">Left rational to compare.</param>
		/// <param name="right">Right rational to compare.</param>
		/// <returns><b>true</b> if the <paramref name="left"/> is greater than the <paramref name="right"/>, <b>false</b> if not.</returns>
		public static bool operator >(GorgonRationalNumber left, GorgonRationalNumber right)
		{
			decimal leftRational = left;
			decimal rightRational = right;

			return leftRational < rightRational;
		}

		/// <summary>
		/// Operator to determine if the left rational is greater than or equal to the right rational.
		/// </summary>
		/// <param name="left">Left rational to compare.</param>
		/// <param name="right">Right rational to compare.</param>
		/// <returns><b>true</b> if the <paramref name="left"/> is greater than or equal to the <paramref name="right"/>, <b>false</b> if not.</returns>
		public static bool operator >=(GorgonRationalNumber left, GorgonRationalNumber right)
		{
			decimal leftRational = left;
			decimal rightRational = right;

			return leftRational >= rightRational;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRationalNumber"/> struct.
		/// </summary>
		/// <param name="numerator">The numerator for the number.</param>
		/// <param name="denominator">The denominator for the number.</param>
		/// <exception cref="DivideByZeroException">Thrown when the <paramref name="denominator"/> is 0.</exception>
		public GorgonRationalNumber(int numerator, int denominator)
		{
			if (denominator == 0)
			{
				throw new DivideByZeroException();
			}

			Numerator = numerator;
			Denominator = denominator;
		}
		#endregion
	}
}
