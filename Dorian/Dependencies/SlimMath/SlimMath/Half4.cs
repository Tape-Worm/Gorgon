/*
* Copyright (c) 2007-2010 SlimMath Group
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace SlimMath
{
    /// <summary>
    /// Defines a four component vector, using half precision floating point coordinates.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Half4 : IEquatable<Half4>, IFormattable
    {
        /// <summary>
        /// Gets or sets the X component of the vector.
        /// </summary>
        /// <value>The X component of the vector.</value>
        public Half X;

        /// <summary>
        /// Gets or sets the Y component of the vector.
        /// </summary>
        /// <value>The Y component of the vector.</value>
        public Half Y;

        /// <summary>
        /// Gets or sets the Z component of the vector.
        /// </summary>
        /// <value>The Z component of the vector.</value>
        public Half Z;

        /// <summary>
        /// Gets or sets the W component of the vector.
        /// </summary>
        /// <value>The W component of the vector.</value>
        public Half W;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SlimMath.Half4"/> structure.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        /// <param name="z">The Z component.</param>
        /// <param name="w">The W component.</param>
        public Half4(Half x, Half y, Half z, Half w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SlimMath.Half4"/> structure.
        /// </summary>
        /// <param name="value">The value to set for the X, Y, Z, and W components.</param>
        public Half4(Half value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
            this.W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SlimMath.Half4"/> structure.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        /// <param name="z">The Z component.</param>
        /// <param name="w">The W component.</param>
        public Half4(float x, float y, float z, float w)
        {
            this.X = (Half)x;
            this.Y = (Half)y;
            this.Z = (Half)z;
            this.W = (Half)w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SlimMath.Half4"/> structure.
        /// </summary>
        /// <param name="value">The value to set for the X, Y, Z, and W components.</param>
        public Half4(float value)
        {
            this.X = this.Y = this.Z = this.W = (Half)value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlimMath.Half4"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z, and W components of the vector. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Half4(IList<float> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Count < 4)
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Half4.");

            X = (Half)values[0];
            Y = (Half)values[1];
            Z = (Half)values[2];
            W = (Half)values[3];
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public Half[] ToArray()
        {
            return new Half[] { X, Y, Z, W };
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Half4 left, Half4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Half4 left, Half4 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="T:SlimMath.Vector4"/> to <see cref="T:SlimMath.Half4"/>.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The converted value.</returns>
        public static explicit operator Half4(Vector4 value)
        {
            return new Half4(value.X, value.Y, value.Z, value.W);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:SlimMath.Half4"/> to <see cref="T:SlimMath.Vector4"/>.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The converted value.</returns>
        public static implicit operator Vector4(Half4 value)
        {
            return new Vector4(value.X, value.Y, value.Z, value.W);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} Q:{3}", X, Y, Z, W);
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

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
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

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified object instances are considered equal. 
        /// </summary>
        /// <param name="value1"/>
        /// <param name="value2"/>
        /// <returns>
        /// <c>true</c> if <paramref name="value1"/> is the same instance as <paramref name="value2"/> or 
        /// if both are <c>null</c> references or if <c>value1.Equals(value2)</c> returns <c>true</c>; otherwise, <c>false</c>.</returns>
        public static bool Equals(ref Half4 value1, ref Half4 value2)
        {
            return (value1.X == value2.X) && (value1.Y == value2.Y) && (value1.Z == value2.Z) && (value1.W == value2.W);
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to the specified object. 
        /// </summary>
        /// <param name="other">Object to make the comparison with.</param>
        /// <returns>
        /// <c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
        public bool Equals(Half4 other)
        {
            return (this.X == other.X) && (this.Y == other.Y) && (this.Z == other.Z) && (this.W == other.W);
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object. 
        /// </summary>
        /// <param name="obj">Object to make the comparison with.</param>
        /// <returns>
        /// <c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return Equals((Half4)obj);
        }
    }
}
