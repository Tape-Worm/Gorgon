#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Saturday, April 19, 2008 11:44:35 AM
// 
#endregion

using System;

namespace GorgonLibrary
{
	/// <summary>
	/// Utility class for mathematics.
	/// </summary>
	/// <remarks>
	/// This just provides a convenient interface for commonly used mathematical functions and constants.
	/// </remarks>
	public static class MathUtility
	{
		#region Constants.
		private const float degConvert = ((float)Math.PI / 180.0f);		// Constant containing the value used to convert degrees to radians.
		private const float radConvert = (180.0f / (float)Math.PI);		// Constant containing the value used to convert radians to degrees.

		/// <summary>
		/// Constant containing the value of PI.
		/// </summary>
		public const float PI = (float)Math.PI;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to round a floating point value to an integer.
		/// </summary>
		/// <param name="value">Value to round.</param>
		/// <returns>Rounded value.</returns>
		public static int RoundInt(float value)
		{
			return (int)Math.Round(value);
		}

		/// <summary>
		/// Function to round a floating point value to an integer.
		/// </summary>
		/// <param name="value">Value to round.</param>
		/// <returns>Rounded value.</returns>
		public static float Round(float value)
		{
			return (float)Math.Round(value);
		}

		/// <summary>
		/// Function to round a floating point value.
		/// </summary>
		/// <param name="value">Value to round.</param>
		/// <param name="decimalPlaceCount">Number of decimal places to return.</param>
		/// <returns>Rounded value.</returns>
		public static float Round(float value, int decimalPlaceCount)
		{
			return (float)Math.Round(value, decimalPlaceCount);
		}

		/// <summary>
		/// Function to round a floating point value.
		/// </summary>
		/// <param name="value">Value to round.</param>
		/// <param name="decimalPlaceCount">Number of decimal places to return.</param>
		/// <param name="rounding">Determines how to round mid point numbers.</param>
		/// <returns>Rounded value.</returns>
		public static float Round(float value, int decimalPlaceCount, MidpointRounding rounding)
		{
			return (float)Math.Round(value, decimalPlaceCount, rounding);
		}

		/// <summary>
		/// Function to return the cosine of an angle in radians.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>Cosine.</returns>
		public static float Cos(float radians)
		{
			return (float)Math.Cos(radians);
		}

		/// <summary>
		/// Function to return the sine of an angle in radians.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>Sine.</returns>
		public static float Sin(float radians)
		{
			return (float)Math.Sin(radians);
		}

		/// <summary>
		/// Function to return the tangent of an angle in radians.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>Tangent.</returns>
		public static float Tan(float radians)
		{
			return (float)Math.Tan(radians);
		}

		/// <summary>
		/// Function to return the arc cosine in radians.
		/// </summary>
		/// <param name="cosine">Cosine value to convert.</param>
		/// <returns>Angle in radians.</returns>
		public static float ACos(float cosine)
		{
			return (float)Math.Acos(cosine);
		}

		/// <summary>
		/// Function to return the arc sine in radians.
		/// </summary>
		/// <param name="sine">Sine value to convert.</param>
		/// <returns>Angle in radians.</returns>
		public static float ASin(float sine)
		{
			return (float)Math.Asin(sine);
		}

		/// <summary>
		/// Function to return the arc tangent in radians.
		/// </summary>
		/// <param name="y">Sin value to convert.</param>
		/// <param name="x">Cosine value to convert.</param>
		/// <returns>Angle in radians.</returns>
		public static float ATan(float y,float x)
		{
			return (float)Math.Atan2(y,x);
		}

		/// <summary>
		/// Function to return the square root of a number.
		/// </summary>
		/// <param name="sqvalue">Number to get the square root of.</param>
		/// <returns>Square root of the number.</returns>
		public static float Sqrt(float sqvalue)
		{
			return (float)Math.Sqrt(sqvalue);
		}

		/// <summary>
		/// Function to return the inverse square root of a number.
		/// </summary>
		/// <param name="sqvalue">Number to get the inverse square root of.</param>
		/// <returns>Inverted square root.</returns>
		public static float InverseSqrt(float sqvalue)
		{
			return 1.0f/Sqrt(sqvalue);
		}

		/// <summary>
		/// Function to return the absolute value of a number.
		/// </summary>
		/// <param name="number">Number to get the absolute value of.</param>
		/// <returns>Absolute value of the number.</returns>
		public static float Abs(float number)
		{
			return Math.Abs(number);
		}

		/// <summary>
		/// Function to return the absolute value of a number.
		/// </summary>
		/// <param name="number">Number to get the absolute value of.</param>
		/// <returns>Absolute value of the number.</returns>
		public static int Abs(int number)
		{
			return Math.Abs(number);
		}

		/// <summary>
		/// Function to return if two floating point numbers are equal within a threshold.
		/// </summary>
		/// <param name="value1">First float value to compare.</param>
		/// <param name="value2">Second float value to compare.</param>
		/// <param name="epsilon">Tolerance of floating point error.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool EqualFloat(float value1,float value2,float epsilon)
		{
			if (Abs(value2 - value1) <= epsilon)
				return true;
			
			return false;
		}

		/// <summary>
		/// Function to return if two floating point numbers are equal.
		/// </summary>
		/// <param name="value1">First float value to compare.</param>
		/// <param name="value2">Second float value to compare.</param>		
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool EqualFloat(float value1,float value2)
		{
			return EqualFloat(value1,value2,0.00001f);
		}

		/// <summary>
		/// Function to convert degrees into radians.
		/// </summary>
		/// <param name="degrees">Degrees</param>
		/// <returns>Radian equivilant of degrees.</returns>
		public static float Radians(float degrees)
		{
			return degConvert * degrees;
		}

		/// <summary>
		/// Function to convert radians into degrees.
		/// </summary>
		/// <param name="radians">Radians</param>
		/// <returns>Degree equivilant of radians.</returns>
		public static float Degrees(float radians)
		{
			return radConvert * radians;
		}		

		/// <summary>
		/// Function to return the highest of two values.
		/// </summary>
		/// <returns>Highest of the values supplied.</returns>
		public static float Max(float value1,float value2)
		{
			if (value1 > value2)
				return value1;
			
			return value2;
		}

		/// <summary>
		/// Function to return the lowest of two values.
		/// </summary>
		/// <returns>Highest of the values supplied.</returns>
		public static float Min(float value1,float value2)
		{
			if (value1 < value2)
				return value1;
			
			return value2;
		}

		/// <summary>
		/// Function to return the highest of two values.
		/// </summary>
		/// <returns>Highest of the values supplied.</returns>
		public static short Max(short value1,short value2)
		{
			if (value1 > value2)
				return value1;
			
			return value2;
		}

		/// <summary>
		/// Function to return the lowest of two values.
		/// </summary>
		/// <returns>Highest of the values supplied.</returns>
		public static short Min(short value1,short value2)
		{
			if (value1 < value2)
				return value1;
			
			return value2;			
		}

		/// <summary>
		/// Function to return the highest of two values.
		/// </summary>
		/// <returns>Highest of the values supplied.</returns>
		public static int Max(int value1,int value2)
		{
			if (value1 > value2)
				return value1;
			
			return value2;
		}

		/// <summary>
		/// Function to return the lowest of two values.
		/// </summary>
		/// <returns>Highest of the values supplied.</returns>
		public static int Min(int value1,int value2)
		{
			if (value1 < value2)
				return value1;
			
			return value2;
		}
		#endregion
	}
}
