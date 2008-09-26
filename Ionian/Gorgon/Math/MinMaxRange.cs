#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 11:44:43 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GorgonLibrary
{
	/// <summary>
	/// Value type for storing a minimum and maximum value.
	/// </summary>
	public struct MinMaxRange
	{
		#region Variables.
		private bool _empty;		// Flag to indicate that the range is empty.
		private int _minimum;		// Minimum value.
		private int _maximum;		// Maximum value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the maximum value.
		/// </summary>
		public int Maximum
		{
			get
			{
				return _maximum;
			}
			set
			{
				_empty = false;
				_maximum = value;
			}
		}

		/// <summary>
		/// Property to set or return the Minimum value.
		/// </summary>
		public int Minimum
		{
			get
			{
				return _minimum;
			}
			set
			{
				_empty = false;
				_minimum = value;
			}
		}

		/// <summary>
		/// Property to return an empty range.
		/// </summary>
		public static MinMaxRange Empty
		{
			get
			{
				return new MinMaxRange(true);
			}
		}

		/// <summary>
		/// Property to return whether this structure is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return _empty;
			}
		}

		/// <summary>
		/// Property to return the range (distance between the minimum and maximum values.
		/// </summary>
		public int Range
		{
			get
			{
				if (_empty)
					return 0;

				return Maximum - Minimum;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to shrink by a particular amount.
		/// </summary>
		/// <param name="amount">Amount to shrink values by.</param>
		public void Shrink(int amount)
		{
			_empty = false;
			Minimum += amount;
			Maximum -= amount;
		}

		/// <summary>
		/// Function to expand by a particular amount.
		/// </summary>
		/// <param name="amount">Amount to expand values by.</param>
		public void Expand(int amount)
		{
			_empty = false;
			Minimum -= amount;
			Maximum += amount;
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <param name="inclusive">TRUE to include the minimum/maximum values, FALSE to include only between the two values.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(int value, bool inclusive)
		{
			if (_empty)
				return false;

			if (inclusive)
				return ((value >= Minimum) && (value <= Maximum));
			else
				return ((value > Minimum) && (value < Maximum));
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(int value)
		{
			return InRange(value, true);
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <param name="inclusive">TRUE to include the minimum/maximum values, FALSE to include only between the two values.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(MinMaxRange value, bool inclusive)
		{
			if ((_empty) || (value.IsEmpty))
				return false;

			if (inclusive)
				return ((value.Minimum >= Minimum) && (value.Minimum <= Maximum) && (value.Maximum >= Minimum) && (value.Minimum <= Maximum));
			else
				return ((value.Minimum > Minimum) && (value.Minimum < Maximum) && (value.Maximum > Minimum) && (value.Minimum < Maximum));
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(MinMaxRange value)
		{
			return InRange(value, true);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if obj and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is MinMaxRange)
			{
				if ((((MinMaxRange)obj).Minimum == this.Minimum) && (((MinMaxRange)obj).Maximum == this.Maximum) && (((MinMaxRange)obj).IsEmpty == this.IsEmpty))
					return true;
				else
					return false;
			}
			else
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
			return Minimum.GetHashCode() ^ Maximum.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return "MinMaxRange:\n\tMinimum: " + Minimum.ToString() + " Maximum: " + Maximum.ToString() + "\n\tRange:" + Range.ToString() + "\n\tEmpty: " + _empty.ToString();
		}

		/// <summary>
		/// Function to add together two ranges.
		/// </summary>
		/// <param name="range1">First range to add.</param>
		/// <param name="range2">Second range to add.</param>
		/// <returns>Sum of the two ranges.</returns>
		public static MinMaxRange Add(MinMaxRange range1, MinMaxRange range2)
		{
			return new MinMaxRange(range1.Minimum + range2.Minimum, range1.Maximum + range2.Maximum);
		}

		/// <summary>
		/// Function to subtract two ranges.
		/// </summary>
		/// <param name="range1">First range to subtract.</param>
		/// <param name="range2">Second range to subtract.</param>
		/// <returns>Difference between the two ranges.</returns>
		public static MinMaxRange Subtract(MinMaxRange range1, MinMaxRange range2)
		{
			return new MinMaxRange(range1.Minimum - range2.Minimum, range1.Maximum - range2.Maximum);
		}

		/// <summary>
		/// Function to multiply together two ranges.
		/// </summary>
		/// <param name="range1">First range to multiply.</param>
		/// <param name="range2">Second range to multiply.</param>
		/// <returns>Product of the two ranges.</returns>
		public static MinMaxRange Multiply(MinMaxRange range1, MinMaxRange range2)
		{
			return new MinMaxRange(range1.Minimum * range2.Minimum, range1.Maximum * range2.Maximum);
		}

		/// <summary>
		/// Function to divide two ranges.
		/// </summary>
		/// <param name="range1">First range to divide.</param>
		/// <param name="range2">Second range to divide.</param>
		/// <returns>Quotient of the two ranges.</returns>
		public static MinMaxRange Divide(MinMaxRange range1, MinMaxRange range2)
		{
			return new MinMaxRange(range1.Minimum / range2.Minimum, range1.Maximum / range2.Maximum);
		}

		/// <summary>
		/// Function to round, up or down, a floating point min max range to an integer min max range.
		/// </summary>
		/// <param name="range">Floating point range to round.</param>
		/// <returns>Rounded integer range.</returns>
		public static MinMaxRange Round(MinMaxRangeF range)
		{
			return new MinMaxRange(MathUtility.RoundInt(range.Minimum), MathUtility.RoundInt(range.Maximum));
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator for addition.
		/// </summary>
		/// <param name="range1">First range to add.</param>
		/// <param name="range2">Second range to add.</param>
		/// <returns>Sum of the two ranges.</returns>
		public static MinMaxRange operator +(MinMaxRange range1, MinMaxRange range2)
		{
			return Add(range1, range2);
		}

		/// <summary>
		/// Operator to subtract two ranges.
		/// </summary>
		/// <param name="range1">First range to subtract.</param>
		/// <param name="range2">Second range to subtract.</param>
		/// <returns>Difference between the two ranges.</returns>
		public static MinMaxRange operator -(MinMaxRange range1, MinMaxRange range2)
		{
			return Subtract(range1, range2);
		}

		/// <summary>
		/// Operator to multiply together two ranges.
		/// </summary>
		/// <param name="range1">First range to multiply.</param>
		/// <param name="range2">Second range to multiply.</param>
		/// <returns>Product of the two ranges.</returns>
		public static MinMaxRange operator *(MinMaxRange range1, MinMaxRange range2)
		{
			return Multiply(range1, range2);
		}

		/// <summary>
		/// Operator to divide two ranges.
		/// </summary>
		/// <param name="range1">First range to divide.</param>
		/// <param name="range2">Second range to divide.</param>
		/// <returns>Quotient of the two ranges.</returns>
		public static MinMaxRange operator /(MinMaxRange range1, MinMaxRange range2)
		{
			return Divide(range1, range2);
		}

		/// <summary>
		/// Operator to convert a Point structure to a min/max structure.
		/// </summary>
		/// <param name="point">Point to convert.</param>
		/// <returns>Min max range.</returns>
		public static implicit operator MinMaxRange(Point point)
		{
			return new MinMaxRange(point.X, point.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector structure to a min/max structure.
		/// </summary>
		/// <param name="vector">Vector to convert.</param>
		/// <returns>Min max range.</returns>
		public static explicit operator MinMaxRange(Vector2D vector)
		{
			return new MinMaxRange((int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Operator to convert a floating point min/max structure to an integer min/max structure.
		/// </summary>
		/// <param name="minmax">Floating point min/max structure to convert..</param>
		/// <returns>Integer min max range.</returns>
		public static explicit operator MinMaxRange(MinMaxRangeF minmax)
		{
			return new MinMaxRange((int)minmax.Minimum, (int)minmax.Maximum);
		}

		/// <summary>
		/// Operator to convert a Size structure to a min/max structure.
		/// </summary>
		/// <param name="size">Size to convert.</param>
		/// <returns>Min max range.</returns>
		public static implicit operator MinMaxRange(Size size)
		{
			return new MinMaxRange(size.Width, size.Height);
		}

		/// <summary>
		/// Operator to convert a min/max structure to a Point structure.
		/// </summary>
		/// <param name="minmax">Min/max to convert.</param>
		/// <returns>Point containing min/max values..</returns>
		public static implicit operator Point(MinMaxRange minmax)
		{
			return new Point(minmax.Minimum, minmax.Maximum);
		}

		/// <summary>
		/// Operator to convert a min/max structure to a Size structure.
		/// </summary>
		/// <param name="minmax">Min/max to convert.</param>
		/// <returns>Size containing min/max values..</returns>
		public static implicit operator Size(MinMaxRange minmax)
		{
			return new Size(minmax.Minimum, minmax.Maximum);
		}

		/// <summary>
		/// Operator to convert a min/max structure to a 2D vector structure.
		/// </summary>
		/// <param name="minmax">Min/max to convert.</param>
		/// <returns>Vector containing min/max values..</returns>
		public static implicit operator Vector2D(MinMaxRange minmax)
		{
			return new Vector2D(minmax.Minimum, minmax.Maximum);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		public MinMaxRange(int min, int max)
		{
			if (min > max)
			{
				_minimum = max;
				_maximum = min;
			}
			else
			{
				_minimum = min;
				_maximum = max;
			}

			_empty = false;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="empty">TRUE if structure is empty, FALSE if not.</param>
		internal MinMaxRange(bool empty)
		{
			_minimum = int.MinValue;
			_maximum = int.MaxValue;
			_empty = empty;
		}
		#endregion
	}

	/// <summary>
	/// Value type for storing a minimum and maximum floating point value.
	/// </summary>
	public struct MinMaxRangeF
	{
		#region Variables.
		private bool _empty;		// Flag to indicate that the range is empty.
		private float _minimum;		// Minimum value.
		private float _maximum;		// Maximum value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the maximum value.
		/// </summary>
		public float Maximum
		{
			get
			{
				return _maximum;
			}
			set
			{
				_empty = false;
				_maximum = value;
			}
		}

		/// <summary>
		/// Property to set or return the Minimum value.
		/// </summary>
		public float Minimum
		{
			get
			{
				return _minimum;
			}
			set
			{
				_empty = false;
				_minimum = value;
			}
		}

		/// <summary>
		/// Property to return an empty range.
		/// </summary>
		public static MinMaxRangeF Empty
		{
			get
			{
				return new MinMaxRangeF(true);
			}
		}

		/// <summary>
		/// Property to return whether this structure is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return _empty;
			}
		}

		/// <summary>
		/// Property to return the range (distance between the minimum and maximum values.
		/// </summary>
		public float Range
		{
			get
			{
				if (_empty)
					return 0;

				return Maximum - Minimum;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to shrink by a particular amount.
		/// </summary>
		/// <param name="amount">Amount to shrink values by.</param>
		public void Shrink(float amount)
		{
			_empty = false;
			Minimum += amount;
			Maximum -= amount;
		}

		/// <summary>
		/// Function to expand by a particular amount.
		/// </summary>
		/// <param name="amount">Amount to expand values by.</param>
		public void Expand(float amount)
		{
			_empty = false;
			Minimum -= amount;
			Maximum += amount;
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <param name="inclusive">TRUE to include the minimum/maximum values, FALSE to include only between the two values.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(float value, bool inclusive)
		{
			if (_empty)
				return false;

			if (inclusive)
				return ((value >= Minimum) && (value <= Maximum));
			else
				return ((value > Minimum) && (value < Maximum));
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(float value)
		{
			return InRange(value, true);
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <param name="inclusive">TRUE to include the minimum/maximum values, FALSE to include only between the two values.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(MinMaxRangeF value, bool inclusive)
		{
			if ((_empty) || (value.IsEmpty))
				return false;

			if (inclusive)
				return ((value.Minimum >= Minimum) && (value.Minimum <= Maximum) && (value.Maximum >= Minimum) && (value.Minimum <= Maximum));
			else
				return ((value.Minimum > Minimum) && (value.Minimum < Maximum) && (value.Maximum > Minimum) && (value.Minimum < Maximum));
		}

		/// <summary>
		/// Function to determine if a value falls within the range.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <returns>TRUE if in the range, FALSE if outside.</returns>
		public bool InRange(MinMaxRangeF value)
		{
			return InRange(value, true);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if obj and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is MinMaxRangeF)
			{
				if ((((MinMaxRangeF)obj).Minimum == this.Minimum) && (((MinMaxRangeF)obj).Maximum == this.Maximum) && (((MinMaxRangeF)obj).IsEmpty == this.IsEmpty))
					return true;
				else
					return false;
			}
			else
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
			return Minimum.GetHashCode() ^ Maximum.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return "MinMaxRangeF:\n\tMinimum: " + Minimum.ToString() + " Maximum: " + Maximum.ToString() + "\n\tRange:" + Range.ToString() + "\n\tEmpty: " + _empty.ToString();
		}

		/// <summary>
		/// Function to add together two ranges.
		/// </summary>
		/// <param name="range1">First range to add.</param>
		/// <param name="range2">Second range to add.</param>
		/// <returns>Sum of the two ranges.</returns>
		public static MinMaxRangeF Add(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return new MinMaxRangeF(range1.Minimum + range2.Minimum, range1.Maximum + range2.Maximum);
		}

		/// <summary>
		/// Function to subtract two ranges.
		/// </summary>
		/// <param name="range1">First range to subtract.</param>
		/// <param name="range2">Second range to subtract.</param>
		/// <returns>Difference between the two ranges.</returns>
		public static MinMaxRangeF Subtract(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return new MinMaxRangeF(range1.Minimum - range2.Minimum, range1.Maximum - range2.Maximum);
		}

		/// <summary>
		/// Function to multiply together two ranges.
		/// </summary>
		/// <param name="range1">First range to multiply.</param>
		/// <param name="range2">Second range to multiply.</param>
		/// <returns>Product of the two ranges.</returns>
		public static MinMaxRangeF Multiply(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return new MinMaxRangeF(range1.Minimum * range2.Minimum, range1.Maximum * range2.Maximum);
		}

		/// <summary>
		/// Function to divide two ranges.
		/// </summary>
		/// <param name="range1">First range to divide.</param>
		/// <param name="range2">Second range to divide.</param>
		/// <returns>Quotient of the two ranges.</returns>
		public static MinMaxRangeF Divide(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return new MinMaxRangeF(range1.Minimum / range2.Minimum, range1.Maximum / range2.Maximum);
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator for equality testing.
		/// </summary>
		/// <param name="range1">First range to test.</param>
		/// <param name="range2">Second range to test.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			if ((MathUtility.EqualFloat(range1.Minimum, range2.Minimum)) && (MathUtility.EqualFloat(range1.Maximum, range2.Maximum)))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Operator for inequality testing.
		/// </summary>
		/// <param name="range1">First range to test.</param>
		/// <param name="range2">Second range to test.</param>
		/// <returns>TRUE if not equal, FALSE if not.</returns>
		public static bool operator !=(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return !(range1 == range2);
		}

		/// <summary>
		/// Operator for addition.
		/// </summary>
		/// <param name="range1">First range to add.</param>
		/// <param name="range2">Second range to add.</param>
		/// <returns>Sum of the two ranges.</returns>
		public static MinMaxRangeF operator +(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return Add(range1, range2);
		}

		/// <summary>
		/// Operator to subtract two ranges.
		/// </summary>
		/// <param name="range1">First range to subtract.</param>
		/// <param name="range2">Second range to subtract.</param>
		/// <returns>Difference between the two ranges.</returns>
		public static MinMaxRangeF operator -(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return Subtract(range1, range2);
		}

		/// <summary>
		/// Operator to multiply together two ranges.
		/// </summary>
		/// <param name="range1">First range to multiply.</param>
		/// <param name="range2">Second range to multiply.</param>
		/// <returns>Product of the two ranges.</returns>
		public static MinMaxRangeF operator *(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return Multiply(range1, range2);
		}

		/// <summary>
		/// Operator to divide two ranges.
		/// </summary>
		/// <param name="range1">First range to divide.</param>
		/// <param name="range2">Second range to divide.</param>
		/// <returns>Quotient of the two ranges.</returns>
		public static MinMaxRangeF operator /(MinMaxRangeF range1, MinMaxRangeF range2)
		{
			return Divide(range1, range2);
		}

		/// <summary>
		/// Operator to convert an integer min max range structure to a floating point min/max structure.
		/// </summary>
		/// <param name="minmax">Integer range to convert.</param>
		/// <returns>Floating point min max range.</returns>
		public static implicit operator MinMaxRangeF(MinMaxRange minmax)
		{
			return new MinMaxRangeF(minmax.Minimum, minmax.Maximum);
		}

		/// <summary>
		/// Operator to convert a PointF structure to a min/max structure.
		/// </summary>
		/// <param name="point">PointF to convert.</param>
		/// <returns>Min max range.</returns>
		public static implicit operator MinMaxRangeF(PointF point)
		{
			return new MinMaxRangeF(point.X, point.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector structure to a min/max structure.
		/// </summary>
		/// <param name="vector">Vector to convert.</param>
		/// <returns>Min max range.</returns>
		public static implicit operator MinMaxRangeF(Vector2D vector)
		{
			return new MinMaxRangeF(vector.X, vector.Y);
		}

		/// <summary>
		/// Operator to convert a SizeF structure to a min/max structure.
		/// </summary>
		/// <param name="size">SizeF to convert.</param>
		/// <returns>Min max range.</returns>
		public static implicit operator MinMaxRangeF(SizeF size)
		{
			return new MinMaxRangeF(size.Width, size.Height);
		}

		/// <summary>
		/// Operator to convert a min/max structure to a PointF structure.
		/// </summary>
		/// <param name="minmax">Min/max to convert.</param>
		/// <returns>PointF containing min/max values..</returns>
		public static implicit operator PointF(MinMaxRangeF minmax)
		{
			return new PointF(minmax.Minimum, minmax.Maximum);
		}

		/// <summary>
		/// Operator to convert a min/max structure to a SizeF structure.
		/// </summary>
		/// <param name="minmax">Min/max to convert.</param>
		/// <returns>SizeF containing min/max values..</returns>
		public static implicit operator SizeF(MinMaxRangeF minmax)
		{
			return new SizeF(minmax.Minimum, minmax.Maximum);
		}

		/// <summary>
		/// Operator to convert a min/max structure to a 2D vector structure.
		/// </summary>
		/// <param name="minmax">Min/max to convert.</param>
		/// <returns>Vector containing min/max values..</returns>
		public static implicit operator Vector2D(MinMaxRangeF minmax)
		{
			return new Vector2D(minmax.Minimum, minmax.Maximum);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		public MinMaxRangeF(float min, float max)
		{
			if (min > max)
			{
				_minimum = max;
				_maximum = min;
			}
			else
			{
				_minimum = min;
				_maximum = max;
			}

			_empty = false;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="empty">TRUE if structure is empty, FALSE if not.</param>
		internal MinMaxRangeF(bool empty)
		{
			_minimum = float.MinValue;
			_maximum = float.MaxValue;
			_empty = empty;
		}
		#endregion
	}
}
