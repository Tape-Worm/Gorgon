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
// Created: Thursday, August 25, 2011 8:17:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Diagnostics
{
	/// <summary>
	/// Debugging utility methods.
	/// </summary>
	public static class GorgonDebug
	{
		#region Methods.
		/// <summary>
		/// Function to throw an exception if a string is NULL (Nothing in VB.Net) or empty.
		/// </summary>
		/// <param name="value">The value being passed.</param>
		/// <param name="paramName">The name of the parameter.</param>
		/// <remarks>This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.</remarks>
		public static void AssertParamString(string value, string paramName)
		{
#if DEBUG
			if (value == null)
				throw new ArgumentNullException(paramName);
			if (value == string.Empty)
				throw new ArgumentException("The parameter must not be a zero-length string.", paramName);
#endif
		}

		/// <summary>
		/// Function to throw an exception if a value is not between the range specified.
		/// </summary>
		/// <param name="value">Value to compare.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="minInclusive">TRUE if the minimum is inclusive in the range (i.e. value &lt; min).</param>
		/// <param name="maxInclusive">TRUE if the maximum is inclusive in the range (i.e. value &gt; max).</param>
		/// <param name="paramName">Name of the parameter.</param>
		public static void AssertParamRange(int value, int min, int max, bool minInclusive, bool maxInclusive, string paramName)
		{
#if DEBUG
			if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
				throw new ArgumentOutOfRangeException(paramName, "The value '" + value.ToString() + "' is less than the minimum value '" + min.ToString() + "'");

			if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
				throw new ArgumentOutOfRangeException(paramName, "The value '" + value.ToString() + "' is greater than the maximum value '" + max.ToString() + "'");
#endif
		}

		/// <summary>
		/// Function to throw an exception if a value is not between the range specified.
		/// </summary>
		/// <param name="value">Value to compare.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="paramName">Name of the parameter.</param>
		/// <remarks>This overload includes the min value in the comparison, but excludes the max value (i.e. value &lt; 0 and value &gt;= max).</remarks>
		public static void AssertParamRange(int value, int min, int max, string paramName)
		{
#if DEBUG
			AssertParamRange(value, min, max, true, false, paramName);
#endif
		}

		/// <summary>
		/// Function to determine if a range is valid for a collection.
		/// </summary>
		/// <param name="index">Index being requested.</param>
		/// <param name="count">Number of items in the collection.</param>
		public static void AssertRange(int index, int count)
		{
#if DEBUG
			if ((index < 0) || (index >= count))
				throw new IndexOutOfRangeException("Out of range.  The index '" + index.ToString() + "' must be between 0 and " + count + ".");
#endif
		}

		/// <summary>
		/// Function to throw an exception if an object is NULL (Nothing in VB.Net).
		/// </summary>
		/// <typeparam name="T">A reference type to evaluate.</typeparam>
		/// <param name="value">Value to evaluate.</param>
		/// <param name="paramName">Name of the parameter to evaluate.</param>
		public static void AssertNull<T>(T value, string paramName) where T : class
		{
#if DEBUG
			if (value == null)
				throw new ArgumentNullException(paramName);
#endif
		}
		#endregion
	}
}
