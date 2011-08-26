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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// Utility for formatting numeric values into hexadecimal strings.
	/// </summary>
	public static class GorgonHexFormatter
	{
		#region Methods.
		/// <summary>
		/// Function to format a byte value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Byte value to format.</param>
		/// <returns>The formatted byte value.</returns>
		public static string Format(byte value)
		{
			return value.ToString("x").PadLeft(2, '0');
		}

		/// <summary>
		/// Function to format a short value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Short value to format.</param>
		/// <returns>The formatted short value.</returns>
		public static string Format(short value)
		{
			return value.ToString("x").PadLeft(4, '0');
		}

		/// <summary>
		/// Function to format an unsigned short value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Unsigned short value to format.</param>
		/// <returns>The formatted unsigned short value.</returns>
		public static string Format(ushort value)
		{
			return value.ToString("x").PadLeft(4, '0');
		}

		/// <summary>
		/// Function to format an integer value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Integer value to format.</param>
		/// <returns>The formatted integer value.</returns>
		public static string Format(int value)
		{
			return value.ToString("x").PadLeft(8, '0');
		}

		/// <summary>
		/// Function to format an unsigned integer value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Unsigned integer value to format.</param>
		/// <returns>The formatted unsigned integer value.</returns>
		public static string Format(uint value)
		{
			return value.ToString("x").PadLeft(8, '0');
		}

		/// <summary>
		/// Function to format a long value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Long value to format.</param>
		/// <returns>The formatted long value.</returns>
		public static string Format(long value)
		{
			return value.ToString("x").PadLeft(16, '0');
		}

		/// <summary>
		/// Function to format an unsigned long value into a hexadecimal string.
		/// </summary>
		/// <param name="value">Unsigned long value to format.</param>
		/// <returns>The formatted unsigned long value.</returns>
		public static string Format(ulong value)
		{
			return value.ToString("x").PadLeft(16, '0');
		}

		/// <summary>
		/// Function to format a pointer (IntPtr) value into a hexadecimal string.
		/// </summary>
		/// <param name="pointer">Pointer to format.</param>
		/// <returns>The formatted address of the pointer.</returns>
		/// <remarks>This function will take into account whether the application is x64 or x86 and will format accordingly.</remarks>
		public static string Format(IntPtr pointer)
		{
			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
				return pointer.ToInt64().ToString("x").PadLeft(16, '0');
			else
				return pointer.ToInt32().ToString("x").PadLeft(8, '0');
		}
		#endregion
	}
}
