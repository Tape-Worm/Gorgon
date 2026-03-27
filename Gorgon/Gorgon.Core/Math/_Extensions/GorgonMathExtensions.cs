// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: November 17, 2023 6:27:06 PM
//

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Gorgon.Math;

/// <summary>
/// Fluent extensions for mathematical operations on various numeric types.
/// </summary>
/// <remarks>
/// <para>
/// This provides a fluent interface to numeric types (e.g. <see cref="float"/>, <see cref="double"/>, <see cref="int"/>, etc...) that will expose common mathematical functions without relying on 
/// methods from <see cref="System.Math"/>. This makes it easy to chain together several functions to retrieve a result, for example:
/// </para>
/// <code language="csharp">
/// <![CDATA[
/// int myValueTooBig = 150;
/// int myValueTooSmall = 5;
/// 
/// // Ensure the value does not exceed 100, but is greater than 10.
/// Console.WriteLine($"{myValueTooBig.Min(100).Max(10)}, {myValueTooSmall.Min(100).Max(10)}");  
///
/// // Outputs: 100, 10
/// ]]>
/// </code>
/// <para>
/// Other mathematical functions are included, such as <see cref="Sin(float)"/>, <see cref="Cos(float)"/>, <see cref="Tan(float)"/>, etc... 
/// </para>
/// </remarks>
public static class GorgonMathExtensions
{
    // A decimal version of the PI constant.
    private const decimal DecimalPI = 3.14159265M;
    // Constant containing the value used to convert degrees to radians.
    private const float DegConvert = ((float)DecimalPI / 180.0f);
    // Constant containing the value used to convert degrees to radians.
    private const double DoubleDegConvert = ((double)DecimalPI / 180.0);
    // Constant containing the value used to convert degrees to radians.
    private const decimal DecimalDegConvert = (DecimalPI / 180.0M);
    // Constant containing the value used to convert radians to degrees.
    private const float RadConvert = (180.0f / (float)DecimalPI);
    // Constant containing the value used to convert radians to degrees.
    private const double DoubleRadConvert = (180.0 / (double)DecimalPI);
    // Constant containing the value used to convert radians to degrees.
    private const decimal DecimalRadConvert = (180.0M / DecimalPI);

    extension(byte value)
    {
        /// <summary>
        /// Function to return the maximum value between two <see cref="byte"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Max(byte value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="byte"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Min(byte value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Clamp(byte minValue, byte maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }

        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Wrap(byte min, byte max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Wrap(byte max, bool maxInclusive = true) => Wrap(value, (byte)0, max, maxInclusive);
    }

    extension(uint value)
    {
        /// <summary>
        /// Function to return the maximum value between two <see cref="uint"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Max(uint value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="uint"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Min(uint value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Clamp(uint minValue, uint maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }

        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Wrap(uint min, uint max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Wrap(uint max, bool maxInclusive = true) => Wrap(value, 0, max, maxInclusive);
    }

    extension(ushort value)
    {
        /// <summary>
        /// Function to return the maximum value between two <see cref="ushort"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Max(ushort value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="ushort"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Min(ushort value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Clamp(ushort minValue, ushort maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }

        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Wrap(ushort min, ushort max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Wrap(ushort max, bool maxInclusive = true) => Wrap(value, (ushort)0, max, maxInclusive);
    }

    extension(short value)
    {
        /// <summary>
        /// Function to return the maximum value between two <see cref="short"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Max(short value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="short"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Min(short value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to return the absolute value of a <see cref="float"/> value.
        /// </summary>
        /// <returns>The absolute value of the number.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Abs() => System.Math.Abs(value);

        /// <summary>
        /// Function to return the sign of a <see cref="short"/> value.
        /// </summary>
        /// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            if (value == 0)
            {
                return 0;
            }

            return value < 0 ? -1 : 1;
        }

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Clamp(short minValue, short maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }

        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Wrap(short min, short max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Wrap(short max, bool maxInclusive = true) => Wrap(value, (short)0, max, maxInclusive);
    }

    extension(int value)
    {
        /// <summary>
        /// Function to return the maximum value between two <see cref="int"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Max(int value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="int"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Min(int value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to multiply a 32 bit integer by a numerator value, and the divide by its denominator.
        /// </summary>
        /// <param name="numerator">The numerator to multiply with the number.</param>
        /// <param name="denominator">The denominator to divide with.</param>
        /// <returns>The result of the operation.</returns>
        /// <remarks>
        /// <para>
        /// This is an implementation of Microsoft's MulDiv function as implemented here: https://stackoverflow.com/a/25065519/1045720
        /// </para>
        /// <para>
        /// The return value is the result of the multiplication and division, rounded to the nearest integer. If the result is a positive half integer (ends in .5), it is rounded up. If the result is a negative 
        /// half integer, it is rounded down. 
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int MulDiv(int numerator, int denominator) => unchecked((int)(((long)value * numerator + denominator >> 1) / denominator));

        /// <summary>
        /// Function to return the absolute value of an <see cref="int"/> value.
        /// </summary>
        /// <returns>The absolute value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Abs() => System.Math.Abs(value);

        /// <summary>
        /// Function to return the sign of an <see cref="int"/> value.
        /// </summary>
        /// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            if (value == 0)
            {
                return 0;
            }

            return value < 0 ? -1 : 1;
        }

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clamp(int minValue, int maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }

        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Wrap(int min, int max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Wrap(int max, bool maxInclusive = true) => Wrap(value, 0, max, maxInclusive);
    }

    extension(long value)
    {
        /// <summary>
        /// Function to return the maximum value between two <see cref="long"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Max(long value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="long"/> values..
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Min(long value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to return the absolute value of a <see cref="long"/> value.
        /// </summary>
        /// <returns>The absolute value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Abs() => System.Math.Abs(value);

        /// <summary>
        /// Function to return the sign of a <see cref="long"/> value.
        /// </summary>
        /// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            if (value == 0)
            {
                return 0;
            }

            return value < 0 ? -1 : 1;
        }

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Clamp(long minValue, long maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }

        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Wrap(long min, long max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Wrap(long max, bool maxInclusive = true) => Wrap(value, 0L, max, maxInclusive);
    }

    extension(ulong value)
    {
        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Clamp(ulong minValue, ulong maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }


        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Wrap(ulong min, ulong max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Wrap(ulong max, bool maxInclusive = true) => Wrap(value, 0UL, max, maxInclusive);

        /// <summary>
        /// Function to return the maximum value between two <see cref="ulong"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Max(ulong value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="ulong"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Min(ulong value2) => (value < value2) ? value : value2;
    }

    extension(float value)
    {
        /// <summary>
        /// Function to return the absolute value of a <see cref="float"/> value.
        /// </summary>
        /// <returns>The absolute value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Abs() => MathF.Abs(value);

        /// <summary>
        /// Function to round a <see cref="float"/> value to the nearest whole or fractional number.
        /// </summary>
        /// <param name="decimalCount">[Optional] The number of decimal places to round to.</param>
        /// <param name="rounding">[Optional] The type of rounding to perform.</param>
        /// <returns>The <see cref="float"/> value rounded to the nearest whole number.</returns>
        /// <remarks>  
        /// See <see cref="System.Math.Round(double,int,MidpointRounding)"/> for more information.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Round(int decimalCount = 0, MidpointRounding rounding = MidpointRounding.AwayFromZero) => MathF.Round(value, decimalCount, rounding);

        /// <summary>
        /// Function to return the inverse of the square root for a <see cref="float"/> value.
        /// </summary>
        /// <returns>The inverted square root of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float InverseSqrt() => 1.0f / MathF.Sqrt(value);

        /// <summary>
        /// Function to return the square root for a <see cref="float"/> value.
        /// </summary>
        /// <returns>The square root of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sqrt() => MathF.Sqrt(value);

        /// <summary>
        /// Function to raise a <see cref="float"/> to a specified power.
        /// </summary>
        /// <param name="power">The value representing a power to raise to.</param>
        /// <returns>the value raised to the specified <paramref name="power"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Pow(float power) => MathF.Pow(value, power);

        /// <summary>
        /// Function to compute the logarithim of a value.
        /// </summary>
        /// <param name="power">The new base for the logarithm.</param>
        /// <returns>The logarithim value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Log(float power) => MathF.Log(value, power);

        /// <summary>
        /// Function to return the largest integer less than or equal to the specified <see cref="float"/> value.
        /// </summary>
        /// <returns>The largest integer less than or equal to the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float FastFloor()
        {
            int result = (int)value;

            return (value < result) ? result - 1 : result;
        }

        /// <summary>
        /// Function to return the largest integer greater than or equal to the specified <see cref="float"/> value.
        /// </summary>
        /// <returns>The largest integer greater than or equal to the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float FastCeiling()
        {
            int result = (int)value;

            return (value > result) ? result + 1 : result;
        }

        /// <summary>
        /// Function to return the sign of a <see cref="float"/> value.
        /// </summary>
        /// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            if (value.EqualsEpsilon(0))
            {
                return 0;
            }

            return value < 0 ? -1 : 1;
        }

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Clamp(float minValue, float maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }


        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Wrap(float min, float max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Wrap(float max, bool maxInclusive = true) => Wrap(value, 0.0f, max, maxInclusive);

        /// <summary>
        /// Function to convert a <see cref="float"/> value representing a radian into an angle in degrees.
        /// </summary>
        /// <returns>The angle in degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ToDegrees() => value * RadConvert;

        /// <summary>
        /// Function to convert a <see cref="float"/> value representing an angle in degrees into a radian value.
        /// </summary>
        /// <returns>The angle in radians.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ToRadians() => value * DegConvert;

        /// <summary>
        /// Function to determine if a <see cref="float"/> value is equal to another within a given tolerance.
        /// </summary>
        /// <param name="right">The right value to compare.</param>
        /// <param name="epsilon">[Optional] The epsilon representing the error tolerance.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// Floating point values are prone to error buildup due to their limited precision. Therefore, when performing a comparison between two floating point values: <c>4.23212f == 4.23212f</c> may 
        /// actually be <c>4.232120000005422f == 4.232120000005433f</c>. Obviously, the result will not be <b>true</b> when the values are actually considered equal. This method ensures that the comparison will 
        /// return true by removing the error through the <paramref name="epsilon"/> parameter.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsEpsilon(float right, float epsilon = 1e-06f) => Abs(right - value) <= epsilon;

        /// <summary>
        /// Function to return the sine value of a <see cref="float"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The sine value of the angle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sin() => MathF.Sin(value);

        /// <summary>
        /// Function to return the cosine value of a <see cref="float"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The cosine value of the angle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cos() => MathF.Cos(value);

        /// <summary>
        /// Function to return the tangent value of a <see cref="float"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The tangent value of the angle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Tan() => MathF.Tan(value);

        /// <summary>
        /// Function to wrap an angle between 0 and 360 degrees.
        /// </summary>
        /// <returns>The wrapped angle value.</returns>
        /// <remarks>
        /// <para>
        /// This method ensures that an angle remains within the 0 to 360 degree value range. If an angle exceeds the range, the method will add or remove the required amount to get the angle back into the 0 to 
        /// 360 degree range.
        /// </para>
        /// <para>
        /// For example, if the angle is -45.0f, then the return value will be 315.0f, or if the angle is 405.0f, the return value will be 45.0f.
        /// </para>
        /// </remarks>
        public float WrapAngle()
        {
            while (value > 360.0f)
            {
                value -= 360.0f;
            }

            while (value < 0.0f)
            {
                value += 360.0f;
            }

            return value;
        }

        /// <summary>
        /// Function to return the inverse sine value of a <see cref="float"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The inverse sine value of the value>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ASin() => MathF.Asin(value);

        /// <summary>
        /// Function to return the inverse cosine value of a <see cref="float"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The inverse cosine value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ACos() => MathF.Acos(value);

        /// <summary>
        /// Function to return the inverse tangent value of a <see cref="float"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The tangent sine value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ATan() => MathF.Atan(value);

        /// <summary>
        /// Function to return the inverse tangent of two <see cref="float"/> values representing the horizontal and vertical offset of a slope.
        /// </summary>
        /// <param name="x">Horizontal slope value to retrieve the inverse tangent from.</param>
        /// <returns>The inverse tangent of the slope.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ATan(float x) => MathF.Atan2(value, x);

        /// <summary>
        /// Function to return <b><i>e</i></b> raised to a <see cref="float"/> value as the power.
        /// </summary>
        /// <returns><b><i>e</i></b> raised to the value.</returns>
        /// <remarks>
        /// <b><i>e</i></b> is a constant value of ~2.71828.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Exp() => MathF.Exp(value);

        /// <summary>
        /// Function to perform an approximation of a sine calculation.
        /// </summary>
        /// <returns>The sine value for the angle.</returns>
        /// <remarks>
        /// <para>
        /// This method will produce an approximation of the value returned by <see cref="Sin(float)"/>. Because this is an approximation, this method should not be used when accuracy is important. 
        /// </para>
        /// <para>
        /// This version of the sine function has better performance than the <see cref="Sin(float)"/> method, and as such, should be used in performance intensive situations.
        /// </para>
        /// <para>
        /// This code was adapted from the <a href="http://www.gamedev.net" target="_blank">GameDev.Net</a> post by L.Spiro found here: 
        /// <a href="http://www.gamedev.net/topic/681723-faster-sin-and-cos/" target="_blank">http://www.gamedev.net/topic/681723-faster-sin-and-cos/</a>.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float FastSin()
        {
            int i32I = (int)(value * 0.31830988618379067153776752674503);  // 1 / PI.
            double radians = value - ((i32I) * 3.1415926535897932384626433832795);

            double fX2 = radians * radians;

            return (float)(((i32I & 1) == 1)
                       ? -radians * ((1.00000000000000000000e+00) +
                                  (fX2 * ((-1.66666671633720397949e-01) +
                                         (fX2 * ((8.33333376795053482056e-03) +
                                                (fX2 * ((-1.98412497411482036114e-04) +
                                                       (fX2 * ((2.75565571428160183132e-06) +
                                                              (fX2 * ((-2.50368472620721149724e-08) +
                                                                     (fX2 * ((1.58849267073435385100e-10) +
                                                                            (fX2 * (-6.58925550841432672300e-13)))))))))))))))
                       : radians * ((1.00000000000000000000e+00) +
                                 (fX2 * ((-1.66666671633720397949e-01) +
                                        (fX2 * ((8.33333376795053482056e-03) +
                                               (fX2 * ((-1.98412497411482036114e-04) +
                                                      (fX2 * ((2.75565571428160183132e-06) +
                                                             (fX2 * ((-2.50368472620721149724e-08) +
                                                                    (fX2 * ((1.58849267073435385100e-10) +
                                                                           (fX2 * (-6.58925550841432672300e-13))))))))))))))));
        }

        /// <summary>
        /// Function to perform an approximation of a cosine calculation.
        /// </summary>
        /// <returns>The cosine value for the angle.</returns>
        /// <remarks>
        /// <para>
        /// This method will produce an approximation of the value returned by <see cref="Cos(float)"/>. Because this is an approximation, this method should not be used when accuracy is important. 
        /// </para>
        /// <para>
        /// This version of the cosine function has better performance than the <see cref="Cos(float)"/> method, and as such, should be used in performance intensive situations.
        /// </para>
        /// <para>
        /// This code was adapted from the <a href="http://www.gamedev.net" target="_blank">GameDev.Net</a> post by L.Spiro found here: 
        /// <a href="http://www.gamedev.net/topic/681723-faster-sin-and-cos/" target="_blank">http://www.gamedev.net/topic/681723-faster-sin-and-cos/</a>.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float FastCos()
        {
            int i32I = (int)(value * 0.31830988618379067153776752674503);  // 1 / PI.
            double radians = value - ((i32I) * 3.1415926535897932384626433832795);

            double fX2 = radians * radians;

            return (float)(((i32I & 1) == 1)
                       ? -(1.00000000000000000000e+00) -
                         (fX2 * ((-5.00000000000000000000e-01) +
                                (fX2 * ((4.16666641831398010254e-02) +
                                       (fX2 * ((-1.38888671062886714935e-03) +
                                              (fX2 * ((2.48006890615215525031e-05) +
                                                     (fX2 * ((-2.75369927749125054106e-07) +
                                                            (fX2 * ((2.06207229069832465029e-09) +
                                                                   (fX2 * (-9.77507137733812925262e-12))))))))))))))
                       : (1.00000000000000000000e+00) +
                         (fX2 * ((-5.00000000000000000000e-01) +
                                (fX2 * ((4.16666641831398010254e-02) +
                                       (fX2 * ((-1.38888671062886714935e-03) +
                                              (fX2 * ((2.48006890615215525031e-05) +
                                                     (fX2 * ((-2.75369927749125054106e-07) +
                                                            (fX2 * ((2.06207229069832465029e-09) +
                                                                   (fX2 * (-9.77507137733812925262e-12)))))))))))))));
        }

        /// <summary>
        /// Function to linearly interpolate between two values given a weight amount.
        /// </summary>
        /// <param name="to">The ending value.</param>
        /// <param name="amount">The weighting amount.</param>
        /// <returns>The linearly interpolated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Lerp(float to, float amount) => ((to - value) * amount) + value;

        /// <summary>
        /// Function to return the maximum value between two <see cref="float"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(float value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="float"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(float value2) => (value < value2) ? value : value2;
    }

    extension(double value)
    {
        /// <summary>
        /// Function to return the sine value of a <see cref="double"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The sine value of the angle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Sin() => System.Math.Sin(value);

        /// <summary>
        /// Function to return the cosine value of a <see cref="double"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The cosine value of the angle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Cos() => System.Math.Cos(value);

        /// <summary>
        /// Function to return the tangent value of a <see cref="double"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The tangent value of the angle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Tan() => System.Math.Tan(value);

        /// <summary>
        /// Function to wrap an angle between 0 and 360 degrees.
        /// </summary>
        /// <returns>The wrapped angle value.</returns>
        /// <remarks>
        /// <para>
        /// This method ensures that an angle remains within the 0 to 360 degree value range. If an angle exceeds the range, the method will add or remove the required amount to get the angle back into the 0 to 
        /// 360 degree range.
        /// </para>
        /// <para>
        /// For example, if the angle is -45.0f, then the return value will be 315.0f, or if the angle is 405.0f, the return value will be 45.0f.
        /// </para>
        /// </remarks>
        public double WrapAngle()
        {
            while (value > 360.0)
            {
                value -= 360.0;
            }

            while (value < 0)
            {
                value += 360.0;
            }

            return value;
        }

        /// <summary>
        /// Function to return the inverse cosine value of a <see cref="double"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The inverse cosine value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ACos() => System.Math.Acos(value);

        /// <summary>
        /// Function to return the inverse sine value of a <see cref="double"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The inverse sine value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ASin() => System.Math.Asin(value);

        /// <summary>
        /// Function to return the maximum value between two <see cref="double"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Max(double value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="double"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Min(double value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to linearly interpolate between two values given a weight amount.
        /// </summary>
        /// <param name="to">The ending value.</param>
        /// <param name="amount">The weighting amount.</param>
        /// <returns>The linearly interpolated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Lerp(double to, double amount) => ((to - value) * amount) + value;

        /// <summary>
        /// Function to convert a <see cref="double"/> value representing an angle in degrees into a radian value.
        /// </summary>
        /// <returns>The angle in radians.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToRadians() => value * DoubleDegConvert;

        /// <summary>
        /// Function to convert a <see cref="double"/> value representing a radian into an angle in degrees.
        /// </summary>
        /// <returns>The angle in degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToDegrees() => value * DoubleRadConvert;

        /// <summary>
        /// Function to return the absolute value of a <see cref="double"/> value.
        /// </summary>
        /// <returns>The absolute value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Abs() => System.Math.Abs(value);

        /// <summary>
        /// Function to round a <see cref="double"/> value to the nearest whole or fractional number.
        /// </summary>
        /// <param name="decimalCount">[Optional] The number of decimal places to round to.</param>
        /// <param name="rounding">[Optional] The type of rounding to perform.</param>
        /// <returns>The <see cref="float"/> value rounded to the nearest whole number.</returns>
        /// <remarks>  
        /// See <see cref="System.Math.Round(double,int,MidpointRounding)"/> for more information.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Round(int decimalCount = 0, MidpointRounding rounding = MidpointRounding.AwayFromZero) => System.Math.Round(value, decimalCount, rounding);

        /// <summary>
        /// Function to return the inverse of the square root for a <see cref="double"/> value.
        /// </summary>
        /// <returns>The inverted square root of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double InverseSqrt() => 1.0 / System.Math.Sqrt(value);

        /// <summary>
        /// Function to return the square root for a <see cref="double"/> value.
        /// </summary>
        /// <returns>The square root of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Sqrt() => System.Math.Sqrt(value);

        /// <summary>
        /// Function to raise a <see cref="double"/> to a specified power.
        /// </summary>
        /// <param name="power">The value representing a power to raise to.</param>
        /// <returns>the value raised to the specified <paramref name="power"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Pow(double power) => System.Math.Pow(value, power);

        /// <summary>
        /// Function to compute the logarithim of a value.
        /// </summary>
        /// <param name="power">The new base for the logarithm.</param>
        /// <returns>The logarithim value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Log(double power) => System.Math.Log(value, power);

        /// <summary>
        /// Function to return the largest integer less than or equal to the specified <see cref="float"/> value.
        /// </summary>
        /// <returns>The largest integer less than or equal to the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float FastFloor()
        {
            int result = (int)value;

            return (value < result) ? result - 1 : result;
        }

        /// <summary>
        /// Function to return the largest integer greater than or equal to the specified <see cref="double"/> value.
        /// </summary>
        /// <returns>The largest integer greater than or equal to the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float FastCeiling()
        {
            int result = (int)value;

            return (value > result) ? result + 1 : result;
        }

        /// <summary>
        /// Function to return the sign of a <see cref="double"/> value.
        /// </summary>
        /// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            if (value.EqualsEpsilon(0))
            {
                return 0;
            }

            return value < 0 ? -1 : 1;
        }

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Clamp(double minValue, double maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }


        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Wrap(double min, double max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }


        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Wrap(double max, bool maxInclusive = true) => Wrap(value, 0.0, max, maxInclusive);

        /// <summary>
        /// Function to determine if a <see cref="double"/> value is equal to another within a given tolerance.
        /// </summary>
        /// <param name="right">The right value to compare.</param>
        /// <param name="epsilon">[Optional] The epsilon representing the error tolerance.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// Floating point values are prone to error buildup due to their limited precision. Therefore, when performing a comparison between two floating point values: <c>4.23212f == 4.23212f</c> may 
        /// actually be <c>4.232120000005422f == 4.232120000005433f</c>. Obviously, the result will not be <b>true</b> when the values are actually considered equal. This method ensures that the comparison will 
        /// return true by removing the error through the <paramref name="epsilon"/> parameter.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsEpsilon(double right, double epsilon = 1e-12) => Abs(right - value) <= epsilon;

        /// <summary>
        /// Function to return the inverse tangent of two <see cref="double"/> values representing the horizontal and vertical offset of a slope.
        /// </summary>
        /// <param name="x">Horizontal slope value to retrieve the inverse tangent from.</param>
        /// <returns>The inverse tangent of the slope.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ATan(double x) => System.Math.Atan2(value, x);

        /// <summary>
        /// Function to return the inverse tangent value of a <see cref="double"/> value representing an angle, in radians.
        /// </summary>
        /// <returns>The tangent sine value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ATan() => System.Math.Atan(value);

        /// <summary>
        /// Function to return <b><i>e</i></b> raised to a <see cref="double"/> value as the power.
        /// </summary>
        /// <returns><b><i>e</i></b> raised to the value.</returns>
        /// <remarks>
        /// <b><i>e</i></b> is a constant value of ~2.71828.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Exp() => System.Math.Exp(value);
    }

    extension(decimal value)
    {
        /// <summary>
        /// Function to return the maximum value between two <see cref="decimal"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The larger of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Max(decimal value2) => (value > value2) ? value : value2;

        /// <summary>
        /// Function to return the minimum value between two <see cref="decimal"/> values.
        /// </summary>
        /// <param name="value2">The second value to test.</param>
        /// <returns>The smaller of the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Min(decimal value2) => (value < value2) ? value : value2;

        /// <summary>
        /// Function to return the absolute value of a <see cref="decimal"/> value.
        /// </summary>
        /// <returns>The absolute value of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Abs() => System.Math.Abs(value);

        /// <summary>
        /// Function to round a <see cref="decimal"/> value to the nearest whole or fractional number.
        /// </summary>
        /// <param name="decimalCount">[Optional] The number of decimal places to round to.</param>
        /// <param name="rounding">[Optional] The type of rounding to perform.</param>
        /// <returns>The <see cref="float"/> value rounded to the nearest whole number.</returns>
        /// <remarks>  
        /// See <see cref="System.Math.Round(decimal,int,MidpointRounding)"/> for more information.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Round(int decimalCount = 0, MidpointRounding rounding = MidpointRounding.AwayFromZero) => decimal.Round(value, decimalCount, rounding);

        /// <summary>
        /// Function to return the sign of a <see cref="decimal"/> value.
        /// </summary>
        /// <returns>0 if the value is 0, -1 if the value is less than 0, and 1 if the value is greater than 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            if (value == 0)
            {
                return 0;
            }

            return value < 0 ? -1 : 1;
        }

        /// <summary>
        /// Function to clamp a value to the range specified by the minimum and maximum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Clamp(decimal minValue, decimal maxValue)
        {
            value = value.Min(maxValue);
            return value.Max(minValue);
        }

        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Wrap(decimal min, decimal max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Wrap(decimal max, bool maxInclusive = true) => Wrap(value, 0.0M, max, maxInclusive);

        /// <summary>
        /// Function to wrap an angle between 0 and 360 degrees.
        /// </summary>
        /// <returns>The wrapped angle value.</returns>
        /// <remarks>
        /// <para>
        /// This method ensures that an angle remains within the 0 to 360 degree value range. If an angle exceeds the range, the method will add or remove the required amount to get the angle back into the 0 to 
        /// 360 degree range.
        /// </para>
        /// <para>
        /// For example, if the angle is -45.0f, then the return value will be 315.0f, or if the angle is 405.0f, the return value will be 45.0f.
        /// </para>
        /// </remarks>
        public decimal WrapAngle()
        {
            while (value > 360.0M)
            {
                value -= 360.0M;
            }

            while (value < 0)
            {
                value += 360.0M;
            }

            return value;
        }
        /// <summary>
        /// Function to convert a <see cref="decimal"/> value representing an angle in degrees into a radian value.
        /// </summary>
        /// <returns>The angle in radians.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ToRadians() => value * DecimalDegConvert;

        /// <summary>
        /// Function to convert a <see cref="decimal"/> value representing a radian into an angle in degrees.
        /// </summary>
        /// <returns>The angle in degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ToDegrees() => value * DecimalRadConvert;

        /// <summary>
        /// Function to linearly interpolate between two values given a weight amount.
        /// </summary>
        /// <param name="to">The ending value.</param>
        /// <param name="amount">The weighting amount.</param>
        /// <returns>The linearly interpolated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Lerp(decimal to, decimal amount) => ((to - value) * amount) + value;
    }

    extension(sbyte value)
    {
        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Wrap(sbyte min, sbyte max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Wrap(sbyte max, bool maxInclusive = true) => Wrap(value, (sbyte)0, max, maxInclusive);
    }

    extension(BigInteger value)
    {
        /// <summary>
        /// Function to wrap a value to the minimum value when it exceeds the maximum.
        /// </summary>
        /// <param name="min">The minimum value to return if the max is exceeded.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or <paramref name="min"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BigInteger Wrap(BigInteger min, BigInteger max, bool maxInclusive = true)
        {
            if (((maxInclusive) && (value >= max)) || (value > max))
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Function to wrap a value to 0 when it exceeds the maximum.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="maxInclusive">[Optional] <b>true</b> to check if the value is greater than, or equal to the <paramref name="max"/>, <b>false</b> to only check if the value is greater than <paramref name="max"/>.</param>
        /// <returns>the value if less than or equal to <paramref name="max"/>, or 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BigInteger Wrap(BigInteger max, bool maxInclusive = true) => Wrap(value, BigInteger.Zero, max, maxInclusive);
    }
}
