﻿// 
// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: Sunday, October 14, 2012 4:37:30 PM
// 

using System.Numerics;
using CenterCLR.XorRandomGenerator;
using Gorgon.Math;
using Simplex;

namespace Gorgon.Core;

/// <summary>
/// A random number generator for floating point and integer values.
/// </summary>
/// <remarks>
/// <para>
/// This class expands upon the functionality of the <see cref="Random"/> class by providing <see cref="float"/> random numbers, and ranges for <see cref="double"/> and <see cref="float"/> random numbers.
/// </para>
/// <para>
/// It also provides a simplex noise implementation for generation of repeatable random noise.
/// </para> 
/// <para>
/// The simplex noise functionality uses the Noise.cs functionality built by Benjamin Ward, which was based on another implementation by Heikki Törmälä.  Both of which are derived from the original Java 
/// implementation by Stefan Gustavson.
/// <list type="bullet">
///     <item>
///         <term>Benjamin Ward</term>
///         <description><a target="_blank" href="https://github.com/WardBenjamin/SimplexNoise">https://github.com/WardBenjamin/SimplexNoise</a></description>
///     </item>
///     <item>
///         <term>Heikki Törmälä</term>
///         <description><a target="_blank" href="https://github.com/Xpktro/simplexnoise/blob/master/SimplexNoise/Noise.cs">https://github.com/Xpktro/simplexnoise/blob/master/SimplexNoise/Noise.cs</a></description>
///     </item>
///     <item>
///         <term>Stefan Gustavson</term>
///         <description><a target="_blank" href="http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/">http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/</a></description>
///     </item>
/// </list> 
/// </para> 
/// </remarks>
/// <seealso cref="Random"/>
public static class GorgonRandom
{
    // Seed used to generate random numbers.
    private static int _seed;
    // Random number generator.    
    private static XorRandom _rnd;

    /// <summary>
    /// Property to set or return the random seed value.
    /// </summary>    
    public static int Seed
    {
        get => _seed;        
        set
        {
            Noise.Seed = value;
            _seed = value;
            _rnd = new XorRandom(_seed);
        }
    }

    /// <summary>
    /// Property to set or return the Simplex noise permutation array.
    /// </summary>
    /// <remarks>
    /// This is used to modify the random values generated by the Simplex noise algorithm.
    /// </remarks>
    public static IReadOnlyList<byte> SimplexPermutations
    {
        get => Noise.Perm;
        set
        {
            if (value is null)
            {
                return;
            }

            Noise.Perm = [.. value];
        }
    }

    /// <summary>
    /// Function to generate simplex noise from a value.
    /// </summary>
    /// <param name="seed">The <see cref="float"/> value to use to generate the simplex noise value.</param>
    /// <returns>A <see cref="float"/> representing the simplex noise value.</returns>
    /// <remarks>
    /// <para>
    /// Simplex noise values similar to Perlin noise but with fewer artifacts and better performance. 
    /// </para>
    /// <para>
    /// This produces predictable random numbers based on the seed <paramref name="seed"/> passed to the method. 
    /// </para>
    /// </remarks>
    public static float SimplexNoise(float seed) => 1.0f + (Noise.Generate(seed) - 1.0f);

    /// <summary>
    /// Function to generate simplex noise from a 2D value.
    /// </summary>
    /// <param name="seed">The vector used to generate the random value.</param>
    /// <returns>A <see cref="float"/> representing the simplex noise value.</returns>
    /// <remarks>
    /// <para>
    /// Simplex noise values similar to Perlin noise but with fewer artifacts and better performance. 
    /// </para>
    /// <para>
    /// This produces predictable random numbers based on the seed <paramref name="seed"/> passed to the method. 
    /// </para>
    /// </remarks>
    public static float SimplexNoise(Vector2 seed) => 1.0f + (Noise.Generate(seed.X, seed.Y) - 1.0f);

    /// <summary>
    /// Function to generate simplex noise from a 3D value.
    /// </summary>
    /// <param name="seed">The vector used to generate the random value.</param>
    /// <returns>A <see cref="float"/> representing the simplex noise value.</returns>
    /// <remarks>
    /// <para>
    /// Simplex noise values similar to Perlin noise but with fewer artifacts and better performance. 
    /// </para>
    /// <para>
    /// This produces predictable random numbers based on the <paramref name="seed"/> values passed to the method. 
    /// </para>
    /// </remarks>
    public static float SimplexNoise(Vector3 seed) => 1.0f + (Noise.Generate(seed.X, seed.Y, seed.Z) - 1.0f);

    /// <summary>
    /// Function to return a random <see cref="double"/> number.
    /// </summary>
    /// <param name="start">The starting value for the random number.</param>
    /// <param name="end">The ending value for the random number range.  This value is inclusive.</param>
    /// <returns>A random <see cref="double"/> value between <paramref name="start"/> to <paramref name="end"/>.</returns>
    /// <remarks>
    /// This overload generates a random <see cref="double"/> number between the range of <paramref name="start"/> and <paramref name="end"/>.
    /// </remarks>
    public static double RandomDouble(double start, double end) => start.EqualsEpsilon(end) ? start : (_rnd.NextDouble() * (end - start)) + start;

    /// <summary>
    /// Function to return a random <see cref="double"/> number.
    /// </summary>
    /// <param name="maxValue">The highest number for random values, this value is inclusive.</param>
    /// <returns>A random <see cref="double"/> value.</returns>.
    /// <remarks>
    /// This overload generates a random <see cref="double"/> number between the range of 0 and <paramref name="maxValue"/>.
    /// </remarks>
    public static double RandomDouble(double maxValue) => RandomDouble(0, maxValue);

    /// <summary>
    /// Function to return a random <see cref="double"/> number.
    /// </summary>
    /// <returns>A random <see cref="double"/> value between 0.0 and 1.0.</returns>
    public static double RandomDouble() => _rnd.NextDouble();

    /// <summary>
    /// Function to return a random <see cref="float"/> number.
    /// </summary>
    /// <param name="start">The starting value for the random number.</param>
    /// <param name="end">The ending value for the random number range.  This value is inclusive.</param>
    /// <returns>A random <see cref="float"/> value between <paramref name="start"/> to <paramref name="end"/>.</returns>
    /// <remarks>
    /// This overload generates a random <see cref="float"/> number between the range of <paramref name="start"/> and <paramref name="end"/>.
    /// </remarks>
    public static float RandomSingle(float start, float end) => start.EqualsEpsilon(end) ? start : (_rnd.NextSingle() * (end - start)) + start;

    /// <summary>
    /// Function to return a random <see cref="float"/> number.
    /// </summary>
    /// <param name="maxValue">The highest number for random values, this value is inclusive.</param>
    /// <returns>A random <see cref="float"/> value.</returns>.
    /// <remarks>
    /// This overload generates a random <see cref="float"/> number between the range of 0 and <paramref name="maxValue"/>.
    /// </remarks>
    public static float RandomSingle(float maxValue) => RandomSingle(0, maxValue);

    /// <summary>
    /// Function to return a random <see cref="float"/> number.
    /// </summary>
    /// <returns>A random <see cref="float"/> value between 0.0f and 1.0f.</returns>
    public static float RandomSingle() => _rnd.NextSingle();

    /// <summary>
    /// Function to return a non-negative random <see cref="int"/>.
    /// </summary>
    /// <param name="start">Starting value for the random number.</param>
    /// <param name="end">Ending value for the random number range.  This value is inclusive.</param>
    /// <returns>The random <see cref="int"/> value within the range of <paramref name="start"/> to <paramref name="end"/> (inclusive).</returns>
    /// <remarks>
    /// This overload generates a random <see cref="int"/> number between the range of <paramref name="start"/> and <paramref name="end"/> (inclusive).
    /// </remarks>
    public static int RandomInt32(int start, int end) => _rnd.Next(start, end);

    /// <summary>
    /// Function to return a non-negative random <see cref="int"/>.
    /// </summary>
    /// <param name="maxValue">The highest number for random values, this value is inclusive.</param>
    /// <returns>A random number</returns>.
    /// <remarks>
    /// This overload generates a random <see cref="int"/> number between the range of 0 and <paramref name="maxValue"/>-1.
    /// </remarks>
    public static int RandomInt32(int maxValue) => _rnd.Next(maxValue);

    /// <summary>
    /// Function to return a non-negative random <see cref="int"/>.
    /// </summary>
    /// <returns>A random <see cref="int"/> value between 0 and <see cref="int.MaxValue"/>-1.</returns>
    public static int RandomInt32() => _rnd.Next();

    /// <summary>
    /// Function to return a non-negative random <see cref="long"/>.
    /// </summary>
    /// <param name="start">Starting value for the random number.</param>
    /// <param name="end">Ending value for the random number range.  This value is inclusive.</param>
    /// <returns>The random <see cref="long"/> value within the range of <paramref name="start"/> to <paramref name="end"/> (inclusive).</returns>
    /// <remarks>
    /// This overload generates a random <see cref="long"/> number between the range of <paramref name="start"/> and <paramref name="end"/> (inclusive).
    /// </remarks>
    public static long RandomInt64(int start, int end) => _rnd.NextInt64(start, end);

    /// <summary>
    /// Function to return a non-negative random <see cref="long"/>.
    /// </summary>
    /// <param name="maxValue">The highest number for random values, this value is inclusive.</param>
    /// <returns>A random number</returns>.
    /// <remarks>
    /// This overload generates a random <see cref="long"/> number between the range of 0 and <paramref name="maxValue"/>-1.
    /// </remarks>
    public static long RandomInt64(int maxValue) => _rnd.NextInt64(maxValue);

    /// <summary>
    /// Function to return a non-negative random <see cref="long"/>.
    /// </summary>
    /// <returns>A random <see cref="int"/> value between 0 and <see cref="long.MaxValue"/>-1.</returns>
    public static long RandomInt64() => _rnd.NextInt64();

    /// <summary>
    /// Initializes the <see cref="GorgonRandom" /> class.
    /// </summary>    
    static GorgonRandom() => Seed = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;
}