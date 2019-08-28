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
using System.Collections;
using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Diagnostics
{
    /// <summary>
    /// Extension methods used to check values for correctness at runtime in debug mode.
    /// </summary>
    public static class GorgonDebugExtensions
    {
        #region Methods.
        /// <summary>
        /// Function to throw an exception if a string is <b>null</b> or empty.
        /// </summary>
        /// <param name="value">The value being passed.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="keepWhitespace">[Optional] <b>true</b> to include whitespace when checking for an empty string, <b>false</b> if whitespace should be excluded when checking for an empty string.</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="keepWhitespace"/> value is set to <b>true</b>, then a value of <c>"     "</c> (5 spaces in this example) will not throw an exception, but if the parameter is set to 
        /// <b>false</b>, then an exception will be thrown as the string will be considered empty regardless of whitespace.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateString(this string value, string paramName, bool keepWhitespace = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (((!keepWhitespace) && (string.IsNullOrWhiteSpace(value)))
                || ((keepWhitespace) && (string.IsNullOrEmpty(value))))
            {
                throw new ArgumentEmptyException(paramName);
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="byte"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this byte value, string paramName, byte min, byte max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="sbyte"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this sbyte value, string paramName, sbyte min, sbyte max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="short"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this short value, string paramName, short min, short max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="ushort"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this ushort value, string paramName, ushort min, ushort max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="int"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this int value, string paramName, int min, int max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, max));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="uint"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this uint value, string paramName, uint min, uint max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="long"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this long value, string paramName, long min, long max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="ulong"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this ulong value, string paramName, ulong min, ulong max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="float"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this float value, string paramName, float min, float max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="double"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this double value, string paramName, double min, double max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to throw an exception if an <see cref="decimal"/> value is not between the range specified.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minInclusive">[Optional] <b>true</b> if the minimum is inclusive in the range (i.e. value &lt; min).</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> if the maximum is inclusive in the range (i.e. value &gt; max).</param>
        /// <remarks>
        /// <para>
        /// When the <paramref name="minInclusive"/>, or the <paramref name="maxInclusive"/> are set to <b>true</b>, then the <paramref name="value"/> is included in the range check. This means that 
        /// the check is <c><paramref name="value"/> &lt; <paramref name="min"/></c> for the minimum value, and <c><paramref name="value"/> &gt; <paramref name="max"/></c> for the maximum value. If 
        /// those parameters are set to false, then the range check is <c><paramref name="value"/> &lt;= <paramref name="min"/></c> for the minimum value and <c><paramref name="value"/> &gt;= <paramref name="max"/></c> 
        /// for the maximum value.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateRange(this decimal value, string paramName, decimal min, decimal max, bool minInclusive = true, bool maxInclusive = false)
        {
            if (((minInclusive) && (value < min)) || ((!minInclusive) && (value <= min)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, value, min));
            }

            if (((maxInclusive) && (value > max)) || ((!maxInclusive) && (value >= max)))
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, value, min));
            }
        }

        /// <summary>
        /// Function to determine if a range is valid for a collection.
        /// </summary>
        /// <param name="collection">The collection to evaluate.</param>
        /// <param name="index">Index being requested.</param>
        /// <remarks>
        /// <para>
        /// This will evaluate the <see cref="ICollection.Count"/> property and check to see that the <paramref name="index"/> parameter is not less than 0, and not greater than or equal to the count.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>		
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateIndex(this ICollection collection, int index)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if ((index < 0) || (index >= collection.Count))
            {
                throw new IndexOutOfRangeException(string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, index, collection.Count));
            }
        }

        /// <summary>
        /// Function to throw an exception if an object is <b>null</b>.
        /// </summary>
        /// <typeparam name="T">A reference type to evaluate. This must be a reference type.</typeparam>
        /// <param name="value">Value to evaluate.</param>
        /// <param name="paramName">Name of the parameter to evaluate.</param>
        /// <remarks>
        /// <para>
        /// This will evaluate the value of the type specified by <typeparamref name="T"/> for <b>null</b>.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateObject<T>(this T value, string paramName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Function to throw an exception if a <see cref="Nullable{T}"/> type is <b>null</b>.
        /// </summary>
        /// <typeparam name="T">A reference type to evaluate. This must be a value type.</typeparam>
        /// <param name="value">Value to evaluate.</param>
        /// <param name="paramName">Name of the parameter to evaluate.</param>
        /// <remarks>
        /// <para>
        /// This will evaluate the value of the nullable value type specified by <typeparamref name="T"/> for <b>null</b>.
        /// </para>
        /// <para>
        /// This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.
        /// </para>
        /// </remarks>
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void ValidateNullable<T>(this T? value, string paramName) where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
        #endregion
    }
}
