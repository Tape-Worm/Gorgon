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

namespace GorgonLibrary.Math
{
	/// <summary>
	/// A random number generator for floating point and integer values.
	/// </summary>
	public static class GorgonRandom
	{
		#region Variables.
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a random floating point number.
		/// </summary>
		/// <param name="start">Starting value for the random number.</param>
		/// <param name="end">Ending value for the random number range.  This value is not inclusive.</param>
		/// <returns>The random number from <paramref name="start"/> to <paramref name="end"/>.</returns>
		public static float RandomSingle(float start, float end)
		{
			end -= 0.000001f;

			if (start < end)
				return (float)_rnd.NextDouble() * (end - start) + start;
			else
				return (float)_rnd.NextDouble() * (start - end) + end;
		}

		/// <summary>
		/// Function to return a random floating point number.
		/// </summary>
		/// <param name="maxValue">The highest number for random values, this value is not inclusive.</param>
		/// <returns>A random number</returns>.
		public static float RandomSingle(float maxValue)
		{
			return RandomSingle(0, maxValue);
		}

		/// <summary>
		/// Function to return a random floating point number.
		/// </summary>
		/// <returns>A random number</returns>
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
