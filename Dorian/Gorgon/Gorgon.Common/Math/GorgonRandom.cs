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
        private static Vector3[] _gradients = null;			// Gradients for Perlin noise.
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
				CreatePerlinGradients();
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

            if (hash > 4)
                value = new Vector2(value.Y, value.X);

            return (((hash & 1) != 0) ? -value.X : value.X) + (((hash & 2) != 0) ? -2.0f * value.Y : 2.0f * value.Y);
        }

        /// <summary>
        /// Function to return the gradient value for the perlin noise generator.
        /// </summary>
        /// <param name="hash">Hash value to use.</param>
        /// <param name="value">Value to retrieve the gradient from.</param>
        /// <returns>The gradient value.</returns>
        private static float PerlinGradient(int hash, Vector3 value)
        {
            Vector2 uv = Vector2.Zero;

            hash = hash & 15;

            uv.X = 
        }

		/// <summary>
		/// Function to create the gradient values for the Perlin noise.
		/// </summary>
		private static void CreatePerlinGradients()
		{
			var values = Enumerable.Range(0, 256).ToList();
			_permutations = Enumerable.Repeat<int>(-1, 256).ToArray();
			_gradients = new Vector3[256];

			// Compute gradients and shuffle permutations.
			for (int i = 0; i < 256; i++)
			{
				float value = RandomSingle() * 2.0f - 1.0f;
				float range = (1.0f - (value * value)).Sqrt();
				float theta = ((float)System.Math.PI) * 2.0f * RandomSingle();
				int valueIndex = RandomInt32(values.Count);

				_permutations[i] = values[valueIndex];
				values.RemoveAt(valueIndex);

				_gradients[i].X = range * theta.Cos();
				_gradients[i].Y = range * theta.Sin();
				_gradients[i].Z = value;
			}
		}

		/// <summary>
		/// Function to look up the random gradient value at the specified coordinates.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="z">Depth position.</param>
		/// <param name="value">Value to dot with the index value.</param>
		/// <returns>The randomized value.</returns>
		private static float PerlinLattice3(int x, int y, int z, Vector3 value)
		{
			float result = 0.0f;
			int index = _permutations[(_permutations[(_permutations[z & 0xFF] + y) & 0xFF] + x) & 0xFF];

			Vector3.Dot(ref _gradients[index], ref value, out result);

			return result;
		}

		/// <summary>
		/// Function to look up the random gradient value at the specified coordinates.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="value">Value to dot with the index value.</param>
		/// <returns>The randomized value.</returns>
		private static float PerlinLattice2(int x, int y, Vector2 value)
		{
			float result = 0.0f;
			int index = _permutations[(_permutations[y & 0xFF] + x) & 0xFF];

			Vector2 gradient = (Vector2)_gradients[index];
			Vector2.Dot(ref gradient, ref value, out result);

			return result;
		}


		/// <summary>
		/// Function to retrieve a 2 dimensional Perlin noise value.
		/// </summary>
		/// <param name="source">The source value to use to generate the Perline noise value.</param>
		/// <returns>The Perlin noise value.</returns>
		public static float Perlin(Vector2 source)
		{
			int x = (int)System.Math.Floor(source.X);
			int y = (int)System.Math.Floor(source.Y);
			Vector2 diff = new Vector2(x - source.X, y - source.Y);
			Vector2 neg = new Vector2(diff.X - 1.0f, diff.Y - 1.0f);
			Vector2 smoothed = new Vector2(diff.X * diff.X * (3 - 2 * diff.X), diff.Y * diff.Y * (3 - 2 * diff.Y));

			Vector2 value1 = Vector2.Zero;
			Vector2 value2 = Vector2.Zero;

			value1.X = PerlinLattice2(x, y, diff);
			value2.X = PerlinLattice2(x + 1, y, new Vector2(neg.X, diff.Y));
			value1.Y = value1.X + (value2.X - value1.X) * smoothed.X;

			value1.X = PerlinLattice2(x, y + 1, new Vector2(diff.X, neg.Y));
			value2.X = PerlinLattice2(x + 1, y + 1, new Vector2(neg.X, neg.Y));
			value2.Y = value1.X + (value2.X - value1.X) * smoothed.X;

			return value1.Y + (value2.Y - value1.Y) * smoothed.Y;
		}

		/// <summary>
		/// Function to retrieve a 3 dimensional Perlin noise value.
		/// </summary>
		/// <param name="source">The source value to use to generate the Perline noise value.</param>
		/// <returns>The Perlin noise value.</returns>
		public static float Perlin(Vector3 source)
		{
			int x = (int)System.Math.Floor(source.X);
			int y = (int)System.Math.Floor(source.Y);
			int z = (int)System.Math.Floor(source.Z);
			Vector3 diff = new Vector3(x - source.X, y - source.Y, z - source.Z);
			Vector3 neg = new Vector3(diff.X - 1.0f, diff.Y - 1.0f, diff.Z - 1.0f);
			Vector3 smoothed = new Vector3(diff.X * diff.X * (3 - 2 * diff.X), diff.Y * diff.Y * (3 - 2 * diff.Y), diff.Z * diff.Z * (3 - 2 * diff.Z));

			Vector3 value1 = Vector3.Zero;
			Vector3 value2 = Vector3.Zero;

			value1.X = PerlinLattice3(x, y, z, diff);
			value2.X = PerlinLattice3(x + 1, y, z, new Vector3(neg.X, diff.Y, diff.Z));
			value1.Y = value1.X + (value2.X - value1.X) * smoothed.X;

			value1.X = PerlinLattice3(x, y + 1, z, new Vector3(diff.X, neg.Y, diff.Z));
			value2.X = PerlinLattice3(x + 1, y + 1, z, new Vector3(neg.X, neg.Y, diff.Z));
			value2.Y = value1.X + (value2.X - value1.X) * smoothed.X;
			value1.Z = value1.Y + (value2.Y - value1.Y) * smoothed.Y;

			value1.X = PerlinLattice3(x, y, z + 1, new Vector3(diff.X, diff.Y, neg.Z));
			value2.X = PerlinLattice3(x + 1, y, z + 1, new Vector3(neg.X, diff.Y, neg.Z));
			value1.Y = value1.X + (value2.X - value1.X) * smoothed.X;

			value1.X = PerlinLattice3(x, y + 1, z + 1, new Vector3(diff.X, neg.Y, neg.Z));
			value2.X = PerlinLattice3(x + 1, y + 1, z + 1, neg);
			value2.Y = value1.X + (value2.X - value1.X) * smoothed.X;

			value2.Z = value1.Y + (value2.Y - value1.Y) * smoothed.Y;

			return value1.Z + (value2.Z - value1.Z) * smoothed.Z;
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
