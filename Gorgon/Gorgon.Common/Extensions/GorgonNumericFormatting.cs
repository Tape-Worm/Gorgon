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
// Created: Thursday, August 25, 2011 8:10:31 PM
// 
#endregion

using System;
using System.Globalization;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;

namespace Gorgon.Core
{
	/// <summary>
	/// Utility for formatting numeric values into various types of strings.
	/// </summary>
	public static class GorgonNumericFormattingExtension
	{
		#region Methods.
        /// <summary>
        /// FUnction to return a string formatted to the current culture.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="sizeType">Size value to use.</param>
        /// <returns>The string formatted to the current UI culture.</returns>
        private static string GetCultureString(double value, string sizeType)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:0.0} {1}", value, sizeType);
        }

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this byte amount)
		{
		    return GetCultureString(amount, Resources.GOR_UNIT_MEM_BYTES);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this short amount)
		{
		    double scale = System.Math.Abs(amount) / 1024.0;

		    return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this ushort amount)
		{
		    double scale = amount / 1024.0;

		    return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this int amount)
		{
		    double scale = System.Math.Abs(amount) / 1073741824.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
		    }

		    scale = amount / 1048576.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
		    }

		    scale = amount / 1024.0;

		    return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this uint amount)
		{
            double scale = amount / 1073741824.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
            }

            scale = amount / 1048576.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
            }

            scale = amount / 1024.0;

		    return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this long amount)
		{
		    double scale = System.Math.Abs(amount) / 1125899906842624.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
		    }

		    scale = amount / 1099511627776.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
		    }

			scale = amount / 1073741824.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
		    }

		    scale = amount / 1048576.0;

			if (scale >= 1.0)
			{
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
			}

			scale = amount / 1024.0;

		    return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this ulong amount)
		{
		    double scale = amount / 1125899906842624.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
		    }

		    scale = amount / 1099511627776.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
		    }

			scale = amount / 1073741824.0;

		    if (scale >= 1.0)
		    {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
		    }

		    scale = amount / 1048576.0;

			if (scale >= 1.0)
			{
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
			}

			scale = amount / 1024.0;

		    return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this float amount)
		{
		    double scale = System.Math.Abs(amount) / 1125899906842624.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
            }

            scale = amount / 1099511627776.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
            }

            scale = amount / 1073741824.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
            }

            scale = amount / 1048576.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
            }

            scale = amount / 1024.0;

            return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
        }

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemory(this double amount)
		{
		    double scale = System.Math.Abs(amount) / 1125899906842624.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
            }

            scale = amount / 1099511627776.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
            }

            scale = amount / 1073741824.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
            }

            scale = amount / 1048576.0;

            if (scale >= 1.0)
            {
                return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
            }

            scale = amount / 1024.0;

            return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES);
        }

		/// <summary>
		/// Function to format a byte value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Byte value to format.</param>
		/// <returns>The formatted byte value.</returns>
		public static string FormatHex(this byte value)
		{
			return value.ToString("x").PadLeft(2, '0');
		}

		/// <summary>
		/// Function to format a short value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Short value to format.</param>
		/// <returns>The formatted short value.</returns>
		public static string FormatHex(this short value)
		{
			return value.ToString("x").PadLeft(4, '0');
		}

		/// <summary>
		/// Function to format an unsigned short value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Unsigned short value to format.</param>
		/// <returns>The formatted unsigned short value.</returns>
		public static string FormatHex(this ushort value)
		{
			return value.ToString("x").PadLeft(4, '0');
		}

		/// <summary>
		/// Function to format an integer value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Integer value to format.</param>
		/// <returns>The formatted integer value.</returns>
		public static string FormatHex(this int value)
		{
			return value.ToString("x").PadLeft(8, '0');
		}

		/// <summary>
		/// Function to format an unsigned integer value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Unsigned integer value to format.</param>
		/// <returns>The formatted unsigned integer value.</returns>
		public static string FormatHex(this uint value)
		{
			return value.ToString("x").PadLeft(8, '0');
		}

		/// <summary>
		/// Function to format a long value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Long value to format.</param>
		/// <returns>The formatted long value.</returns>
		public static string FormatHex(this long value)
		{
			return value.ToString("x").PadLeft(16, '0');
		}

		/// <summary>
		/// Function to format an unsigned long value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Unsigned long value to format.</param>
		/// <returns>The formatted unsigned long value.</returns>
		public static string FormatHex(this ulong value)
		{
			return value.ToString("x").PadLeft(16, '0');
		}

		/// <summary>
		/// Function to format a pointer (IntPtr) value into a hexadecimal string.
		/// </summary>
		/// <param name="pointer">Pointer to format.</param>
		/// <returns>The formatted address of the pointer.</returns>
		/// <remarks>This method will take into account whether the application is x64 or x86 and will format accordingly.</remarks>
		public static string FormatHex(this IntPtr pointer)
		{
		    return GorgonComputerInfo.PlatformArchitecture == PlatformArchitecture.x64
		               ? pointer.ToInt64().ToString("x").PadLeft(16, '0')
		               : pointer.ToInt32().ToString("x").PadLeft(8, '0');
		}
	    #endregion
	}
}
