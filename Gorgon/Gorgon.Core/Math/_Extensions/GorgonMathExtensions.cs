#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Friday, June 15, 2012 9:55:35 AM
// 
#endregion

using System;

namespace Gorgon.Math
{
	/// <summary>
	/// Extensions for mathematical operations on various numeric types.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This provides a fluent interface to numeric types (e.g. <see cref="float"/>, <see cref="double"/>, <see cref="int"/>, etc...) that will expose common mathematical functions without relying on 
	/// methods from <see cref="System.Math"/>. This makes it easy to chain together several functions to retrieve a result, for example:
	/// </para>
	/// <code language="csharp">
	/// int myValueTooBig = 150;
	/// int myValueTooSmall = 5;
	/// 
	/// // Ensure the value does not exceed 100, but is greater than 10.
	/// Console.WriteLine("{0}, {1}", myValueTooBig.Min(100).Max(10), myValueTooSmall.Min(100).Max(10));  
	///
	/// // Outputs: 100, 10
	/// </code>
	/// <para>
	/// Other mathematical functions are included, such as <see cref="O:Gorgon.Math.GorgonMathExtensions.Sin">Sine</see>, <see cref="O:Gorgon.Math.GorgonMathExtensions.Cos">Cosine</see>, <see cref="O:Gorgon.Math.GorgonMathExtensions.Tan">Tangent</see>, etc... 
	/// </para>
	/// </remarks>
	public static class GorgonMathExtensions
	{
		#region Constants.
		// Constant containing the value used to convert degrees to radians.
		private const float DegConvert = ((float)System.Math.PI / 180.0f);
		// Constant containing the value used to convert radians to degrees.
		private const float RadConvert = (180.0f / (float)System.Math.PI);		

		/// <summary>
		/// Constant value for &#x03C0;.
		/// </summary>
		public const float PI = 3.141593f;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the maximum value between two <see cref="byte"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static byte Max(this byte value1, byte value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="byte"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static byte Min(this byte value1, byte value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="ushort"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static ushort Max(this ushort value1, ushort value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="ushort"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static ushort Min(this ushort value1, ushort value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="short"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static short Max(this short value1, short value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="short"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static short Min(this short value1, short value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="uint"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static uint Max(this uint value1, uint value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="uint"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static uint Min(this uint value1, uint value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="int"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static int Max(this int value1, int value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="int"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static int Min(this int value1, int value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="ulong"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static ulong Max(this ulong value1, ulong value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="ulong"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static ulong Min(this ulong value1, ulong value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="long"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static long Max(this long value1, long value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="long"/> values..
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static long Min(this long value1, long value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="float"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static float Max(this float value1, float value2)
		{			
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="float"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static float Min(this float value1, float value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="double"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static double Max(this double value1, double value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="double"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static double Min(this double value1, double value2)
		{
			return (value1 < value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the maximum value between two <see cref="decimal"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The larger of the two values.</returns>
		public static decimal Max(this decimal value1, decimal value2)
		{
			return (value1 > value2) ? value1 : value2;
		}

		/// <summary>
		/// Function to return the minimum value between two <see cref="decimal"/> values.
		/// </summary>
		/// <param name="value1">The first value to test.</param>
		/// <param name="value2">The second value to test.</param>
		/// <returns>The smaller of the two values.</returns>
		public static decimal Min(this decimal value1, decimal value2)
		{
			return (value1 < value2) ? value1 : value2;
		}
		
		/// <summary>
		/// Function to return the absolute value of a <see cref="float"/> value.
		/// </summary>
		/// <param name="value">Value to evaluate.</param>
		/// <returns>The absolute value of <paramref name="value"/>.</returns>
		public static short Abs(this short value)
		{
			return System.Math.Abs(value);
		}

		/// <summary>
		/// Function to return the absolute value of an <see cref="int"/> value.
		/// </summary>
		/// <param name="value">Value to evaluate.</param>
		/// <returns>The absolute value of <paramref name="value"/>.</returns>
		public static int Abs(this int value)
		{
			return System.Math.Abs(value);
		}

		/// <summary>
		/// Function to return the absolute value of a <see cref="long"/> value.
		/// </summary>
		/// <param name="value">Value to evaluate.</param>
		/// <returns>The absolute value of <paramref name="value"/>.</returns>
		public static long Abs(this long value)
		{
			return System.Math.Abs(value);
		}

		/// <summary>
		/// Function to return the absolute value of a <see cref="double"/> value.
		/// </summary>
		/// <param name="value">Value to evaluate.</param>
		/// <returns>The absolute value of <paramref name="value"/>.</returns>
		public static double Abs(this double value)
		{
			return System.Math.Abs(value);
		}

		/// <summary>
		/// Function to return the absolute value of a <see cref="decimal"/> value.
		/// </summary>
		/// <param name="value">Value to evaluate.</param>
		/// <returns>The absolute value of <paramref name="value"/>.</returns>
		public static decimal Abs(this decimal value)
		{
			return System.Math.Abs(value);
		}

		/// <summary>
		/// Function to return the absolute value of a <see cref="float"/> value.
		/// </summary>
		/// <param name="value">Value to evaluate.</param>
		/// <returns>The absolute value of <paramref name="value"/>.</returns>
		public static float Abs(this float value)
		{
			return System.Math.Abs(value);
		}

		/// <summary>
		/// Function to round a <see cref="float"/> value to the nearest whole or fractional number.
		/// </summary>
		/// <param name="value">The value to round.</param>
		/// <param name="decimalCount">[Optional] The number of decimal places to round to.</param>
		/// <param name="rounding">[Optional] The type of rounding to perform.</param>
		/// <returns>The <see cref="float"/> value rounded to the nearest whole number.</returns>
		/// <remarks>  
		/// See <see cref="System.Math.Round(double,int,MidpointRounding)"/> for more information.
		/// </remarks>
		public static float Round(this float value, int decimalCount = 0, MidpointRounding rounding = MidpointRounding.ToEven)
		{
			return (float)(System.Math.Round(value, decimalCount, rounding));
		}

		/// <summary>
		/// Function to round a <see cref="decimal"/> value to the nearest whole or fractional number.
		/// </summary>
		/// <param name="value">The value to round.</param>
		/// <param name="decimalCount">[Optional] The number of decimal places to round to.</param>
		/// <param name="rounding">[Optional] The type of rounding to perform.</param>
		/// <returns>The <see cref="float"/> value rounded to the nearest whole number.</returns>
		/// <remarks>  
		/// See <see cref="System.Math.Round(decimal,int,MidpointRounding)"/> for more information.
		/// </remarks>
		public static decimal Round(this decimal value, int decimalCount = 0, MidpointRounding rounding = MidpointRounding.ToEven)
		{
			return System.Math.Round(value, decimalCount, rounding);
		}

		/// <summary>
		/// Function to round a <see cref="double"/> value to the nearest whole or fractional number.
		/// </summary>
		/// <param name="value">The value to round.</param>
		/// <param name="decimalCount">[Optional] The number of decimal places to round to.</param>
		/// <param name="rounding">[Optional] The type of rounding to perform.</param>
		/// <returns>The <see cref="float"/> value rounded to the nearest whole number.</returns>
		/// <remarks>  
		/// See <see cref="System.Math.Round(double,int,MidpointRounding)"/> for more information.
		/// </remarks>
		public static double Round(this double value, int decimalCount = 0, MidpointRounding rounding = MidpointRounding.ToEven)
		{
			return System.Math.Round(value, decimalCount, rounding);
		}

		/// <summary>
		/// Function to convert a <see cref="float"/> value representing a radian into an angle in degrees.
		/// </summary>
		/// <param name="radians">The value to convert.</param>
		/// <returns>The angle in degrees.</returns>
		public static float ToDegrees(this float radians)
		{
			return radians * RadConvert;
		}

		/// <summary>
		/// Function to convert a <see cref="float"/> value representing an angle in degrees into a radian value.
		/// </summary>
		/// <param name="degrees">The angle value to convert.</param>
		/// <returns>The angle in radians.</returns>
		public static float ToRadians(this float degrees)
		{
			return degrees * DegConvert;
		}

		/// <summary>
		/// Function to convert a <see cref="decimal"/> value representing a radian into an angle in degrees.
		/// </summary>
		/// <param name="radians">The value to convert.</param>
		/// <returns>The angle in degrees.</returns>
		public static decimal ToDegrees(this decimal radians)
		{
			return radians * (decimal)RadConvert;
		}

		/// <summary>
		/// Function to convert a <see cref="decimal"/> value representing an angle in degrees into a radian value.
		/// </summary>
		/// <param name="degrees">The angle value to convert.</param>
		/// <returns>The angle in radians.</returns>
		public static decimal ToRadians(this decimal degrees)
		{
			return degrees * (decimal)DegConvert;
		}


		/// <summary>
		/// Function to convert a <see cref="double"/> value representing a radian into an angle in degrees.
		/// </summary>
		/// <param name="radians">The value to convert.</param>
		/// <returns>The angle in degrees.</returns>
		public static double ToDegrees(this double radians)
		{
			return radians * RadConvert;
		}

		/// <summary>
		/// Function to convert a <see cref="double"/> value representing an angle in degrees into a radian value.
		/// </summary>
		/// <param name="degrees">The angle value to convert.</param>
		/// <returns>The angle in radians.</returns>
		public static double ToRadians(this double degrees)
		{
			return degrees * DegConvert;
		}

		/// <summary>
		/// Function to determine if a <see cref="float"/> value is equal to another within a given tolerance.
		/// </summary>
		/// <param name="left">The left value to compare.</param>
		/// <param name="right">The right value to compare.</param>
		/// <param name="epsilon">[Optional] The epsilon representing the error tolerance.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		/// <remarks>
		/// Floating point values are prone to error buildup due to their limited precision. Therefore, when performing a comparison between two floating point values: <c>4.23212f == 4.23212f</c> may 
		/// actually be <c>4.232120000005422f == 4.232120000005433f</c>. Obviously, the result will not be <b>true</b> when the values are actually considered equal. This method ensures that the comparison will 
		/// return true by removing the error through the <paramref name="epsilon"/> parameter.
		/// </remarks>
		public static bool EqualsEpsilon(this float left, float right, float epsilon = 1e-06f)
		{
			return Abs(right - left) <= epsilon;
		}

		/// <summary>
		/// Function to determine if a <see cref="double"/> value is equal to another within a given tolerance.
		/// </summary>
		/// <param name="left">The left value to compare.</param>
		/// <param name="right">The right value to compare.</param>
		/// <param name="epsilon">[Optional] The epsilon representing the error tolerance.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		/// <remarks>
		/// Floating point values are prone to error buildup due to their limited precision. Therefore, when performing a comparison between two floating point values: <c>4.23212f == 4.23212f</c> may 
		/// actually be <c>4.232120000005422f == 4.232120000005433f</c>. Obviously, the result will not be <b>true</b> when the values are actually considered equal. This method ensures that the comparison will 
		/// return true by removing the error through the <paramref name="epsilon"/> parameter.
		/// </remarks>
		public static bool EqualsEpsilon(this double left, double right, double epsilon = 1e-12)
        {
            return Abs(right - left) <= epsilon;
        }

        /// <summary>
		/// Function to return the inverse of the square root for a <see cref="double"/> value.
		/// </summary>
		/// <param name="value">The value to get the inverse square root of.</param>
		/// <returns>The inverted square root of the value.</returns>
		public static double InverseSqrt(this double value)
		{
			return 1.0 / System.Math.Sqrt(value);
		}

		/// <summary>
		/// Function to return the inverse of the square root for a <see cref="float"/> value.
		/// </summary>
		/// <param name="value">The value to get the inverse square root of.</param>
		/// <returns>The inverted square root of the value.</returns>
		public static float InverseSqrt(this float value)
		{
			return 1.0f / (float)System.Math.Sqrt(value);
		}

		/// <summary>
		/// Function to return the square root for a <see cref="double"/> value.
		/// </summary>
		/// <param name="value">The value to get the square root of.</param>
		/// <returns>The square root of the value.</returns>
		public static double Sqrt(this double value)
		{
			return System.Math.Sqrt(value);
		}

		/// <summary>
		/// Function to return the square root for a <see cref="float"/> value.
		/// </summary>
		/// <param name="value">The value to get the square root of.</param>
		/// <returns>The square root of the value.</returns>
		public static float Sqrt(this float value)
		{
			return (float)System.Math.Sqrt(value);
		}

		/// <summary>
		/// Function to return the sine value of a <see cref="float"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The sine value of the <paramref name="angle"/>.</returns>
		public static float Sin(this float angle)
		{
			return (float)System.Math.Sin(angle);
		}

		/// <summary>
		/// Function to return the sine value of a <see cref="decimal"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The sine value of the <paramref name="angle"/>.</returns>
		public static decimal Sin(this decimal angle)
		{
			return (decimal)System.Math.Sin((double)angle);
		}

		/// <summary>
		/// Function to return the sine value of a <see cref="double"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The sine value of the <paramref name="angle"/>.</returns>
		public static double Sin(this double angle)
		{
			return System.Math.Sin(angle);
		}

		/// <summary>
		/// Function to return the cosine value of a <see cref="float"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The cosine value of the <paramref name="angle"/>.</returns>
		public static float Cos(this float angle)
		{
			return (float)System.Math.Cos(angle);
		}

		/// <summary>
		/// Function to return the cosine value of a <see cref="decimal"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The cosine value of the <paramref name="angle"/>.</returns>
		public static decimal Cos(this decimal angle)
		{
			return (decimal)System.Math.Cos((double)angle);
		}

		/// <summary>
		/// Function to return the cosine value of a <see cref="double"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The cosine value of the <paramref name="angle"/>.</returns>
		public static double Cos(this double angle)
		{
			return System.Math.Cos(angle);
		}

		/// <summary>
		/// Function to return the tangent value of a <see cref="float"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The tangent value of the <paramref name="angle"/>.</returns>
		public static float Tan(this float angle)
		{
			return (float)System.Math.Tan(angle);
		}

		/// <summary>
		/// Function to return the tangent value of a <see cref="decimal"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The tangent value of the <paramref name="angle"/>.</returns>
		public static decimal Tan(this decimal angle)
		{
			return (decimal)System.Math.Tan((double)angle);
		}

		/// <summary>
		/// Function to return the tangent value of a <see cref="double"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The tangent value of the <paramref name="angle"/>.</returns>
		public static double Tan(this double angle)
		{
			return System.Math.Tan(angle);
		}

		/// <summary>
		/// Function to return the inverse sine value of a <see cref="float"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The inverse sine value of the <paramref name="angle"/>.</returns>
		public static float ASin(this float angle)
		{
			return (float)System.Math.Asin(angle);
		}

		/// <summary>
		/// Function to return the inverse sine value of a <see cref="decimal"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The inverse sine value of the <paramref name="angle"/>.</returns>
		public static decimal ASin(this decimal angle)
		{
			return (decimal)System.Math.Asin((double)angle);
		}

		/// <summary>
		/// Function to return the inverse sine value of a <see cref="double"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The inverse sine value of the <paramref name="angle"/>.</returns>
		public static double ASin(this double angle)
		{
			return System.Math.Asin(angle);
		}

		/// <summary>
		/// Function to return the inverse cosine value of a <see cref="float"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The inverse cosine value of the <paramref name="angle"/>.</returns>
		public static float ACos(this float angle)
		{
			return (float)System.Math.Acos(angle);
		}

		/// <summary>
		/// Function to return the inverse cosine value of a <see cref="decimal"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The inverse cosine value of the <paramref name="angle"/>.</returns>
		public static decimal ACos(this decimal angle)
		{
			return (decimal)System.Math.Acos((double)angle);
		}

		/// <summary>
		/// Function to return the inverse cosine value of a <see cref="double"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The inverse cosine value of the <paramref name="angle"/>.</returns>
		public static double ACos(this double angle)
		{
			return System.Math.Acos(angle);
		}

		/// <summary>
		/// Function to return the inverse tangent value of a <see cref="float"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The tangent sine value of the <paramref name="angle"/>.</returns>
		public static float ATan(this float angle)
		{
			return (float)System.Math.Atan(angle);
		}

		/// <summary>
		/// Function to return the inverse tangent value of a <see cref="decimal"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The tangent sine value of the <paramref name="angle"/>.</returns>
		public static decimal ATan(this decimal angle)
		{
			return (decimal)System.Math.Atan((double)angle);
		}

		/// <summary>
		/// Function to return the inverse tangent value of a <see cref="double"/> value representing an angle, in radians.
		/// </summary>
		/// <param name="angle">The angle, in radians.</param>
		/// <returns>The tangent sine value of the <paramref name="angle"/>.</returns>
		public static double ATan(this double angle)
		{
			return System.Math.Atan(angle);
		}

		/// <summary>
		/// Function to return the inverse tangent of two <see cref="float"/> values representing the horizontal and vertical offset of a slope.
		/// </summary>
		/// <param name="y">Vertical slope value to retrieve the inverse tangent from.</param>
		/// <param name="x">Horizontal slope value to retrieve the inverse tangent from.</param>
		/// <returns>The inverse tangent of the slope.</returns>
		public static float ATan(this float y, float x)
		{
			return (float)System.Math.Atan2(y, x);
		}

		/// <summary>
		/// Function to return the inverse tangent of two <see cref="decimal"/> values representing the horizontal and vertical offset of a slope.
		/// </summary>
		/// <param name="y">Vertical slope value to retrieve the inverse tangent from.</param>
		/// <param name="x">Horizontal slope value to retrieve the inverse tangent from.</param>
		/// <returns>The inverse tangent of the slope.</returns>
		public static decimal ATan(this decimal y, decimal x)
		{
			return (decimal)System.Math.Atan2((double)y, (double)x);
		}

		/// <summary>
		/// Function to return the inverse tangent of two <see cref="double"/> values representing the horizontal and vertical offset of a slope.
		/// </summary>
		/// <param name="y">Vertical slope value to retrieve the inverse tangent from.</param>
		/// <param name="x">Horizontal slope value to retrieve the inverse tangent from.</param>
		/// <returns>The inverse tangent of the slope.</returns>
		public static double ATan(this double y, double x)
		{
			return System.Math.Atan2(y, x);
		}

		/// <summary>
		/// Function to return <b><i>e</i></b> raised to a <see cref="double"/> value as the power.
		/// </summary>
		/// <param name="power">The value representing a power to raise to.</param>
		/// <returns><b><i>e</i></b> raised to the <paramref name="power"/> specified.</returns>
		/// <remarks>
		/// <b><i>e</i></b> is a constant value of ~2.71828.
		/// </remarks>
		public static double Exp(this double power)
		{
			return System.Math.Exp(power);
		}

		/// <summary>
		/// Function to return <b><i>e</i></b> raised to a <see cref="decimal"/> value as the power.
		/// </summary>
		/// <param name="power">The value representing a power to raise to.</param>
		/// <returns><b><i>e</i></b> raised to the <paramref name="power"/> specified.</returns>
		/// <remarks>
		/// <b><i>e</i></b> is a constant value of ~2.71828.
		/// </remarks>
		public static decimal Exp(this decimal power)
		{
			return (decimal)System.Math.Exp((double)power);
		}
		/// <summary>
		/// Function to return <b><i>e</i></b> raised to a <see cref="float"/> value as the power.
		/// </summary>
		/// <param name="power">The value representing a power to raise to.</param>
		/// <returns><b><i>e</i></b> raised to the <paramref name="power"/> specified.</returns>
		/// <remarks>
		/// <b><i>e</i></b> is a constant value of ~2.71828.
		/// </remarks>
		public static float Exp(this float power)
		{
			return (float)System.Math.Exp(power);
		}

		/// <summary>
		/// Function to raise a <see cref="double"/> to a specified power.
		/// </summary>
		/// <param name="value">The value to raise.</param>
		/// <param name="power">The value representing a power to raise to.</param>
		/// <returns>The <paramref name="value"/> raised to the specified <paramref name="power"/>.</returns>
		public static double Pow(this double value, double power)
		{
			return System.Math.Pow(value, power);
		}

		/// <summary>
		/// Function to raise a <see cref="decimal"/> to a specified power.
		/// </summary>
		/// <param name="value">The value to raise.</param>
		/// <param name="power">The value representing a power to raise to.</param>
		/// <returns>The <paramref name="value"/> raised to the specified <paramref name="power"/>.</returns>
		public static decimal Pow(this decimal value, decimal power)
		{
			return (decimal)System.Math.Pow((double)value, (double)power);
		}

		/// <summary>
		/// Function to raise a <see cref="float"/> to a specified power.
		/// </summary>
		/// <param name="value">The value to raise.</param>
		/// <param name="power">The value representing a power to raise to.</param>
		/// <returns>The <paramref name="value"/> raised to the specified <paramref name="power"/>.</returns>
		public static float Pow(this float value, float power)
		{
			return (float)System.Math.Pow(value, power);
		}

        /// <summary>
        /// Function to return the largest integer less than or equal to the specified <see cref="float"/> value.
        /// </summary>
        /// <param name="value">The value to find the floor for.</param>
        /// <returns>The largest integer less than or equal to <paramref name="value"/>.</returns>
        public static float FastFloor(this float value)
        {
	        var result = (int)value;

            return (value < result) ? result - 1 : result;
        }

		/// <summary>
		/// Function to return the largest integer greater than or equal to the specified <see cref="float"/> value.
		/// </summary>
		/// <param name="value">The value to find the ceiling for.</param>
		/// <returns>The largest integer greater than or equal to <paramref name="value"/>.</returns>
		public static float FastCeiling(this float value)
        {
	        var result = (int)value;

	        return (value > result) ? result + 1 : result;
        }

		/// <summary>
		/// Function to return the sign of an <see cref="int"/> value.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
		public static int Sign(this int value)
		{
			if (value == 0)
			{
				return 0;
			}

			return value < 0 ? -1 : 1;
		}

		/// <summary>
		/// Function to return the sign of a <see cref="long"/> value.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
		public static int Sign(this long value)
		{
			if (value == 0)
			{
				return 0;
			}

			return value < 0 ? -1 : 1;
		}

		/// <summary>
		/// Function to return the sign of a <see cref="sbyte"/> value.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
		public static int Sign(this sbyte value)
		{
			if (value == 0)
			{
				return 0;
			}

			return value < 0 ? -1 : 1;
		}

		/// <summary>
		/// Function to return the sign of a <see cref="short"/> value.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
		public static int Sign(this short value)
		{
			if (value == 0)
			{
				return 0;
			}

			return value < 0 ? -1 : 1;
		}

		/// <summary>
		/// Function to return the sign of a <see cref="decimal"/> value.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
		public static int Sign(this decimal value)
		{
			if (value == 0)
			{
				return 0;
			}

			return value < 0 ? -1 : 1;
		}

		/// <summary>
		/// Function to return the sign of a <see cref="float"/> value.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
		public static int Sign(this float value)
		{
			if (value.EqualsEpsilon(0))
			{
				return 0;
			}

			return value < 0 ? -1 : 1;
		}

		/// <summary>
		/// Function to return the sign of a <see cref="double"/> value.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
		public static int Sign(this double value)
		{
			if (value.EqualsEpsilon(0))
			{
				return 0;
			}

			return value < 0 ? -1 : 1;
		}
		#endregion
	}
}
