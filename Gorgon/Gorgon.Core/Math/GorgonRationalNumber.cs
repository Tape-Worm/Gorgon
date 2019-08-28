#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction,cluding without limitation the rights
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
using Gorgon.Properties;

namespace Gorgon.Math
{
    /// <summary>
    /// A representation of a rational number.
    /// </summary>
    public readonly struct GorgonRationalNumber
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
        public static bool Equals(GorgonRationalNumber left, GorgonRationalNumber right) => left.Numerator == right.Numerator
                   && left.Denominator == right.Denominator;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => Denominator == 0
                       ? string.Format(Resources.GOR_TOSTR_RATIONAL, Numerator, Denominator, "NaN")
                       : string.Format(Resources.GOR_TOSTR_RATIONAL, Numerator, Denominator, (decimal)Numerator / Denominator);

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode() => 281.GenerateHash(Numerator).GenerateHash(Denominator);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj) => obj is GorgonRationalNumber rational ? rational.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(GorgonRationalNumber other) => Equals(this, other);

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object. 
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref  name="other"/> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref  name="other"/>. Greater than zero This instance follows <paramref  name="other"/> in the sort order. 
        /// </returns>
        /// <param name="other">An object to compare with this instance. </param>
        public int CompareTo(GorgonRationalNumber other) => Equals(this, other) ? 0 : this < other ? -1 : 1;
        #endregion

        #region Operators.
        /// <summary>
        /// Performs an implicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">The value convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator decimal(GorgonRationalNumber value) => (decimal)value.Numerator / value.Denominator;

        /// <summary>
        /// Performs an explicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="double"/>.
        /// </summary>
        /// <param name="value">The value convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator double(GorgonRationalNumber value) => (double)value.Numerator / value.Denominator;

        /// <summary>
        /// Performs an explicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator float(GorgonRationalNumber value) => (float)value.Numerator / value.Denominator;

        /// <summary>
        /// Performs an explicit conversion from <see cref="GorgonRationalNumber"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator int(GorgonRationalNumber value) => value.Numerator / value.Denominator;

        /// <summary>
        /// Performs an implicit conversion from <see cref="int"/> to <see cref="GorgonRationalNumber"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator GorgonRationalNumber(int value) => new GorgonRationalNumber(value, 1);

        /// <summary>
        /// Operator to compare two instances for equality.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(GorgonRationalNumber left, GorgonRationalNumber right) => Equals(left, right);

        /// <summary>
        /// Operator to compare two instances for inequality.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(GorgonRationalNumber left, GorgonRationalNumber right) => !Equals(left, right);

        /// <summary>
        /// Operator to determine if the left rational is less than the right rational.
        /// </summary>
        /// <param name="left">Left rational to compare.</param>
        /// <param name="right">Right rational to compare.</param>
        /// <returns><b>true</b> if the <paramref  name="left"/> is less than the <paramref  name="right"/>, <b>false</b> if not.</returns>
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
        /// <returns><b>true</b> if the <paramref  name="left"/> is less than or equal to the <paramref  name="right"/>, <b>false</b> if not.</returns>
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
        /// <returns><b>true</b> if the <paramref  name="left"/> is greater than the <paramref  name="right"/>, <b>false</b> if not.</returns>
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
        /// <returns><b>true</b> if the <paramref  name="left"/> is greater than or equal to the <paramref  name="right"/>, <b>false</b> if not.</returns>
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
        /// <exception cref="DivideByZeroException">Thrown when the <paramref  name="denominator"/> is 0.</exception>
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
