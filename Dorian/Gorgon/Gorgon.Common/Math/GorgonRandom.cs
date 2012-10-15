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
		private static Vector3[] _gradients = null;			// Gradients for Perlin noise.
		private static int[] _permutations = null;			// Perlin noise permutations for gradients.
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
