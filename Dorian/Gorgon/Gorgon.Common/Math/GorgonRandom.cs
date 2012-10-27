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
// Created: Sunday, October 14, 2012 4:37:30 PM
// 
#endregion

// Parts of this are based on the Simplex noise for C# code by Heikki Törmälä.
// The original code can be found at http://code.google.com/p/simplexnoise/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// A random number generator for floating point and integer values.
	/// </summary>
	public static class GorgonRandom
	{
		#region Variables.
        private static byte[] _permutations = new byte[]        // Perlin noise permutations for gradients.
              {
                131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
                190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
                88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
                77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
                102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
                135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
                5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
                223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
                129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
                251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
                49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
                138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
                151,160,137,91,90,15,
                131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
                190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
                88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
                77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
                102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
                135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
                5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
                223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
                129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
                251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
                49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
                138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180 
              };			
		private static int _seed;
		private static Random _rnd = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the random seed value.
		/// </summary>
		public static int Seed
		{
			get
			{
				return _seed;
			}
			set
			{
				_seed = value;
				_rnd = new Random(_seed);
			}
		}

		/// <summary>
		/// Property to set or return the Perlin noise permutation array.
		/// </summary>
		public static byte[] PerlinPermutations
		{
			get
			{
				return _permutations;
			}
			set
			{
				int length = 0;

				if (value == null)
					return;

				if (value.Length > _permutations.Length)
					length = _permutations.Length;
				else
					length = value.Length;

				for (int i = 0; i < length; i++)
					_permutations[i] = value[i];
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to return the gradient value for the perlin noise generator.
        /// </summary>
        /// <param name="hash">Hash value to use.</param>
        /// <param name="x">Value to retrieve the gradient from.</param>
        /// <returns>The gradient value.</returns>
        private static float PerlinGradient(int hash, float x)
        {
            hash = hash & 15;
            return (((hash & 8) != 0) ? -(1.0f + (hash & 7)) : (1.0f + (hash & 7))) * x;
        }

        /// <summary>
        /// Function to return the gradient value for the perlin noise generator.
        /// </summary>
        /// <param name="hash">Hash value to use.</param>
        /// <param name="value">Value to retrieve the gradient from.</param>
        /// <returns>The gradient value.</returns>
        private static float PerlinGradient(int hash, Vector2 value)
        {
            hash = hash & 7;

            if (hash >= 4)
                value = new Vector2(value.Y, value.X);

            return ((hash & 1) != 0 ? -value.X : value.X) + ((hash & 2) != 0 ? -2.0f * value.Y : 2.0f * value.Y);
        }
		
		/// <summary>
		/// Function to generate 1 dimensional simplex noise.
		/// </summary>
		/// <param name="value">The value to use to generate the Perline noise value.</param>
		/// <returns>The Perlin noise value.</returns>
		public static float Perlin(float value)
		{
			int index0 = (int)value.FastFloor();
			int index1 = index0 + 1;
			float x0 = value - index0;
			float x1 = x0 - 1.0f;
			float n0, n1;
			float t0 = 1.0f - x0 * x0;

			t0 *= t0;
			n0 = t0 * t0 * PerlinGradient(_permutations[index0 & 0xff], x0);

			float t1 = 1.0f - x1 * x1;
			t1 *= t1;
			n1 = t1 * t1 * PerlinGradient(_permutations[index1 & 0xff], x1);

			// The maximum value of this noise is 8*(3/4)^4 = 2.53125
			// A factor of 0.395 scales to fit exactly within [-1,1]
			return 0.395f * (n0 + n1);
		}

		/// <summary>
		/// Function to generate 2 dimensional simplex noise.
		/// </summary>
		/// <param name="value">The value to use to generate the Perline noise value.</param>
		/// <returns>The Perlin noise value.</returns>
		public static float Perlin(Vector2 value)
		{
			const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
			const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

			float n0 = 0.0f;		// Noise contributions from the three corners
			float n1 = 0.0f;
			float n2 = 0.0f;

			// Skew the input space to determine which simplex cell we're in
			float s = (value.X + value.Y) * F2; // Hairy factor for 2D
			Vector2 values = new Vector2(value.X + s, value.Y + s);
			int i = (int)values.X.FastFloor();
			int j = (int)values.Y.FastFloor();
			float t = (float)(i + j) * G2;
			Vector2 unskew = new Vector2(i - t, j - t);
			Vector2 distance = new Vector2(value.X - unskew.X, value.Y - unskew.Y);

			int offset1 = 0;  // upper triangle, YX order: (0,0)->(0,1)->(1,1)
			int offset2 = 1;

			// For the 2D case, the simplex shape is an equilateral triangle.
			// Determine which simplex we are in.
			if (distance.X > distance.Y)
			{
				// lower triangle, XY order: (0,0)->(1,0)->(1,1)
				offset1 = 1;
				offset2 = 0;
			}

			// A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
			// a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
			// c = (3-sqrt(3))/6
			Vector2 value1 = new Vector2(distance.X - offset1 + G2, distance.Y - offset2 + G2);					// Offsets for middle corner in (x,y) unskewed coords
			Vector2 value2 = new Vector2(distance.X - 1.0f + 2.0f * G2, distance.Y - 1.0f + 2.0f * G2);			// Offsets for last corner in (x,y) unskewed coords

			// Wrap the integer indices at 256, to avoid indexing _permutations[] out of bounds
			int index0 = i % 256;
			int index1 = j % 256;

			// Calculate the contribution from the three corners
			float t0 = 0.5f - distance.X * distance.X - distance.Y * distance.Y;
			float t1 = 0.5f - value1.X * value1.X - value1.Y * value1.Y;
			float t2 = 0.5f - value2.X * value2.X - value2.Y * value2.Y;

			if (t0 >= 0.0f)
			{
				t0 *= t0;
				n0 = t0 * t0 * PerlinGradient(_permutations[index0 + _permutations[index1]], distance);
			}

			if (t1 >= 0.0f)
			{
				t1 *= t1;
				n1 = t1 * t1 * PerlinGradient(_permutations[index0 + offset1 + _permutations[index1 + offset2]], value1);
			}

			if (t2 >= 0.0f)
			{
				t2 *= t2;
				n2 = t2 * t2 * PerlinGradient(_permutations[index0 + 1 + _permutations[index1 + 1]], value2);
			}

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to return values in the interval [-1,1].
			// The scale factor is preliminary!		
			return 40.0f * (n0 + n1 + n2);
		}

		/// <summary>
		/// Function to return a random floating point number.
		/// </summary>
		/// <param name="start">Starting value for the random number.</param>
		/// <param name="end">Ending value for the random number range.  This value is inclusive.</param>
		/// <returns>The random number from <paramref name="start"/> to <paramref name="end"/>.</returns>
		public static float RandomSingle(float start, float end)
		{
			if (start < end)
				return (float)_rnd.NextDouble() * (end - start) + start;
			else
				return (float)_rnd.NextDouble() * (start - end) + end;
		}

		/// <summary>
		/// Function to return a random floating point number.
		/// </summary>
		/// <param name="maxValue">The highest number for random values, this value is inclusive.</param>
		/// <returns>A random number</returns>.
		public static float RandomSingle(float maxValue)
		{
			return RandomSingle(0, maxValue);
		}

		/// <summary>
		/// Function to return a random floating point number between 0 and 1.0f.
		/// </summary>
		/// <returns>A random number between 0 and 1.</returns>
		public static float RandomSingle()
		{
			return (float)_rnd.NextDouble();
		}

		/// <summary>
		/// Function to return a non-negative random 32 bit integer.
		/// </summary>
		/// <param name="start">Starting value for the random number.</param>
		/// <param name="end">Ending value for the random number range.  This value is not inclusive.</param>
		/// <returns>The random number from <paramref name="start"/> to <paramref name="end"/>.</returns>
		public static int RandomInt32(int start, int end)
		{
			return _rnd.Next(start, end);
		}

		/// <summary>
		/// Function to return a non-negative random 32 bit integer.
		/// </summary>
		/// <param name="maxValue">The highest number for random values, this value is not inclusive.</param>
		/// <returns>A random number</returns>.
		public static int RandomInt32(int maxValue)
		{
			return _rnd.Next(maxValue);
		}

		/// <summary>
		/// Function to return a non-negative random 32 bit integer.
		/// </summary>
		/// <returns>A random number</returns>
		public static int RandomInt32()
		{
			return _rnd.Next();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonRandom" /> class.
		/// </summary>
		static GorgonRandom()
		{
			Seed = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;
		}
		#endregion
	}
}
