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

// Parts of this are based on the Simplex noise for Java code by Stefan Gustavson.
// The original code can be found at http://webstaff.itn.liu.se/~stegu/simplexnoise/SimplexNoise.java

using System;
using System.Collections.Generic;
using DX = SharpDX;
using Gorgon.Math;

namespace Gorgon.Core
{
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
	/// </remarks>
	/// <seealso cref="Random"/>
	public static class GorgonRandom
    {
        #region Constants.
		// Skewing factors for 2D Simplex noise.  F2 = 0.5*(sqrt(3.0)-1.0)
        private const float F2 = 0.366025403f;
		// G2 = (3.0-Math.sqrt(3.0))/6.0
        private const float G2 = 0.211324865f;
		// Skewing factors for 3D Simplex noise.
        private const float F3 = 0.333333333f;						
        private const float G3 = 0.166666667f;
		// Skewing factors for 4D Simplex noise.
        private const float F4 = 0.309016994f;						
        private const float G4 = 0.138196601f;

		// Amount to offset corner values in 2D.
        private const float OffsetAmount2D = -1.0f + 2.0f * G2;
		// Amount to offset third corner values in 3D.
        private const float OffsetAmount3D1 = 2.0f * G3;
		// Amount to offset last corner values in 3D.
        private const float OffsetAmount3D2 = -1.0f + 3.0f * G3;
		// Amount to offset third corner values in 4D.
        private const float OffsetAmount4D1 = 2.0f * G4;
		// Amount to offset third corner values in 4D.
        private const float OffsetAmount4D2 = 3.0f * G4;
		// Amount to offset last corner values in 4D.
        private const float OffsetAmount4D3 = -1.0f + 4.0f * G4;	
        #endregion

        #region Variables.
		// 1D and 2D and 3D gradient vectors.
		private static readonly DX.Vector3[] _grad3 = 
		{
			new DX.Vector3(1,1,0),new DX.Vector3(-1,1,0),new DX.Vector3(1,-1,0),new DX.Vector3(-1,-1,0),
			new DX.Vector3(1,0,1),new DX.Vector3(-1,0,1),new DX.Vector3(1,0,-1),new DX.Vector3(-1,0,-1),
			new DX.Vector3(0,1,1),new DX.Vector3(0,-1,1),new DX.Vector3(0,1,-1),new DX.Vector3(0,-1,-1)
		};

		// 4D hypercube gradient vectors.
		private static readonly DX.Vector4[] _grad4 =
		{
			new DX.Vector4(0, 1, 1, 1), new DX.Vector4(0, 1, 1, -1), new DX.Vector4(0, 1, -1, 1), new DX.Vector4(0, 1, -1, -1),
			new DX.Vector4(0, -1, 1, 1), new DX.Vector4(0, -1, 1, -1), new DX.Vector4(0, -1, -1, 1), new DX.Vector4(0, -1, -1, -1),
			new DX.Vector4(1, 0, 1, 1), new DX.Vector4(1, 0, 1, -1), new DX.Vector4(1, 0, -1, 1), new DX.Vector4(1, 0, -1, -1),
			new DX.Vector4(-1, 0, 1, 1), new DX.Vector4(-1, 0, 1, -1), new DX.Vector4(-1, 0, -1, 1), new DX.Vector4(-1, 0, -1, -1),
			new DX.Vector4(1, 1, 0, 1), new DX.Vector4(1, 1, 0, -1), new DX.Vector4(1, -1, 0, 1), new DX.Vector4(1, -1, 0, -1),
			new DX.Vector4(-1, 1, 0, 1), new DX.Vector4(-1, 1, 0, -1), new DX.Vector4(-1, -1, 0, 1), new DX.Vector4(-1, -1, 0, -1),
			new DX.Vector4(1, 1, 1, 0), new DX.Vector4(1, 1, -1, 0), new DX.Vector4(1, -1, 1, 0), new DX.Vector4(1, -1, -1, 0),
			new DX.Vector4(-1, 1, 1, 0), new DX.Vector4(-1, 1, -1, 0), new DX.Vector4(-1, -1, 1, 0), new DX.Vector4(-1, -1, -1, 0)
		};
		
		// Simplex noise permutations for gradients.
		private static readonly byte[] _permutations = 
        {
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

		// Simplex noise permutations pre modulo'd by 12 for performance.
		private static readonly byte[] _permMod12;
		// Simplex noise permutations pre modulo'd by 32 for performance.
        private static readonly byte[] _permMod32;  
		// Seed used to generate random numbers.
		private static int _seed;
		// Random number generator.
		private static Random _rnd;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the random seed value.
		/// </summary>
		public static int Seed
		{
			get => _seed;
			set
			{
				_seed = value;
				_rnd = new Random(_seed);
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
			get => _permutations;
			set
			{
			    if (value == null)
			    {
			        return;
			    }

			    int length = value.Count.Min(_permutations.Length);

			    for (int i = 0; i < length; i++)
			    {
				    byte byteValue = value[i];

			        _permutations[i] = byteValue;
				    _permMod12[i] = (byte)(byteValue % 12);
				    _permMod32[i] = (byte)(byteValue % 32);
			    }
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to generate 1 dimensional simplex noise.
		/// </summary>
		/// <param name="value">The <see cref="float"/> value to use to generate the simplex noise value.</param>
		/// <returns>A <see cref="float"/> representing the simplex noise value.</returns>
		/// <remarks>
		/// <para>
		/// Simplex noise values similar to Perlin noise but with fewer artifacts and better performance. 
		/// </para>
		/// <para>
		/// This produces predictable random numbers based on the seed <paramref name="value"/> passed to the method. 
		/// </para>
		/// </remarks>
		public static float SimplexNoise(float value)
		{
			DX.Vector2 noiseContrib = DX.Vector2.Zero;

			// Skew the input space to determine which simplex cell we're in
			var i = (int)value.FastFloor();
			var j = i + 1;

			float distance = value - i;

			// Wrap the integer indices at 256, to avoid indexing _permutations[] out of bounds
			int index0 = i & 255;
			int index1 = j & 255;

			// Create corner offsets.
			float endPoint = distance - 1;

			// Calculate the contribution from the three corners
			float t0 = (0.5f - distance * distance);
			float t1 = (0.5f - endPoint * endPoint);

			if (t0 > 0.0f)
			{
				var gradIndex0 = _grad3[_permMod12[index0 + _permutations[index1]] % 4].X;
				noiseContrib.X = t0 * t0 * t0 * t0 * gradIndex0 * distance;
			}

			// Skip last calculation if necessary.
			if (t1 <= 0.0f)
			{
				return 108.73f * noiseContrib.X;
			}

			var gradIndex1 = _grad3[_permMod12[index0 + 1 + _permutations[index1 + 1]] % 4].X;
			noiseContrib.Y = t1 * t1 * t1 * t1 * gradIndex1 * endPoint;

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to return values in the interval [-1,1].
			// The scale factor is preliminary!		
			return 108.73f * (noiseContrib.X + noiseContrib.Y);
		}

		/// <summary>
		/// Function to generate 2 dimensional simplex noise.
		/// </summary>
		/// <param name="value">The <see cref="DX.Vector2"/> value to use to generate the simplex noise value.</param>
		/// <returns>A <see cref="float"/> representing the simplex noise value.</returns>
		/// <remarks>
		/// <para>
		/// Simplex noise values similar to Perlin noise but with fewer artifacts and better performance. 
		/// </para>
		/// <para>
		/// This produces predictable random numbers based on the seed <paramref name="value"/> passed to the method. 
		/// </para>
		/// </remarks>
		public static float SimplexNoise(DX.Vector2 value)
		{
			DX.Vector3 noiseContrib = DX.Vector3.Zero;

			// Skew the input space to determine which simplex cell we're in
			float simplex = (value.X + value.Y) * F2;			
			var i = (int)(value.X + simplex).FastFloor();		
			var j = (int)(value.Y + simplex).FastFloor();		

			float t = (i + j) * G2;								

			var unskew = new DX.Vector2(i - t, j - t);				

			DX.Vector2 distance = value - unskew;

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
			
			// Create corner offsets.
			var corner1 = new DX.Vector2(distance.X - offset1 + G2, distance.Y - offset2 + G2);
			var corner2 = new DX.Vector2(distance.X + OffsetAmount2D, distance.Y + OffsetAmount2D);

			// Wrap the integer indices at 256, to avoid indexing _permutations[] out of bounds
			int index0 = i & 255;
			int index1 = j & 255;

			// Calculate the contribution from the three corners
			float t0 = (0.5f - distance.X * distance.X - distance.Y * distance.Y);
			float t1 = (0.5f - corner1.X * corner1.X - corner1.Y * corner1.Y);
			float t2 = (0.5f - corner2.X * corner2.X - corner2.Y * corner2.Y);
			float dot;

			DX.Vector3 vec3Value;
            if (t0 > 0.0f)
            {
	            vec3Value = _grad3[_permMod12[index0 + _permutations[index1]]];
                var gradIndex0 = new DX.Vector2(vec3Value.X, vec3Value.Y);
				DX.Vector2.Dot(ref gradIndex0, ref distance, out dot);
				noiseContrib.X = t0 * t0 * t0 * t0 * dot;
			}

			if (t1 > 0.0f)
			{
				vec3Value = _grad3[_permMod12[index0 + offset1 + _permutations[index1 + offset2]]];
                var gradIndex1 = new DX.Vector2(vec3Value.X, vec3Value.Y);
				DX.Vector2.Dot(ref gradIndex1, ref corner1, out dot);
				noiseContrib.Y = t1 * t1 * t1 * t1 * dot;
			}

			// Skip last calculation if necessary.
			if (t2 <= 0.0f)
			{
				return 70.0f * (noiseContrib.X + noiseContrib.Y);
			}

			vec3Value = _grad3[_permMod12[index0 + 1 + _permutations[index1 + 1]]];
            var gradIndex2 = new DX.Vector2(vec3Value.X, vec3Value.Y);
			DX.Vector2.Dot(ref gradIndex2, ref corner2, out dot);
			noiseContrib.Z = t2 * t2 * t2 * t2 * dot;

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to return values in the interval [-1,1].
			// The scale factor is preliminary!		
			return 70.0f * (noiseContrib.X + noiseContrib.Y + noiseContrib.Z);
		}

		/// <summary>
		/// Function to generate 3 dimensional simplex noise.
		/// </summary>
		/// <param name="value">The <see cref="DX.Vector3"/> value to use to generate the simplex noise value.</param>
		/// <returns>A <see cref="float"/> representing the simplex noise value.</returns>
		/// <remarks>
		/// <para>
		/// Simplex noise values similar to Perlin noise but with fewer artifacts and better performance. 
		/// </para>
		/// <para>
		/// This produces predictable random numbers based on the seed <paramref name="value"/> passed to the method. 
		/// </para>
		/// </remarks>
		public static float SimplexNoise(DX.Vector3 value)
		{
			DX.Vector4 contrib = DX.Vector4.Zero;			// Noise contributions from the four corners
			
			// Skew the input space to determine which simplex cell we're in
			float s = (value.X + value.Y + value.Z) * F3; // Very nice and simple skew factor for 3D
			var i = (int)(value.X + s).FastFloor();
			var j = (int)(value.Y + s).FastFloor();
			var k = (int)(value.Z + s).FastFloor();

			float t = (i + j + k) * G3;

			// Unskew the cell origin back to (x,y,z) space
			var unskew = new DX.Vector3(i - t, j - t, k - t);

			// The x,y,z distances from the cell origin
			DX.Vector3 distance = value - unskew;

			// For the 3D case, the simplex shape is a slightly irregular tetrahedron.
			// Determine which simplex we are in.

			int offset1, offset2, offset3; // Offsets for second corner of simplex in (i,j,k) coordinates
			int offset4, offset5, offset6; // Offsets for third corner of simplex in (i,j,k) coordinates

			if (distance.X >= distance.Y)
			{
				if (distance.Y >= distance.Z)
				{
					offset1 = 1;
					offset2 = 0;
					offset3 = 0;
					offset4 = 1;
					offset5 = 1;
					offset6 = 0;
				} // X Y Z order
				else if (distance.X >= distance.Z)
				{
					offset1 = 1;
					offset2 = 0;
					offset3 = 0;
					offset4 = 1;
					offset5 = 0;
					offset6 = 1;
				} // X Z Y order
				else
				{
					offset1 = 0;
					offset2 = 0;
					offset3 = 1;
					offset4 = 1;
					offset5 = 0;
					offset6 = 1;
				} // Z X Y order
			}
			else
			{
				// distance.X<distance.Y
				if (distance.Y < distance.Z)
				{
					offset1 = 0;
					offset2 = 0;
					offset3 = 1;
					offset4 = 0;
					offset5 = 1;
					offset6 = 1;
				} // Z Y X order
				else if (distance.X < distance.Z)
				{
					offset1 = 0;
					offset2 = 1;
					offset3 = 0;
					offset4 = 0;
					offset5 = 1;
					offset6 = 1;
				} // Y Z X order
				else
				{
					offset1 = 0;
					offset2 = 1;
					offset3 = 0;
					offset4 = 1;
					offset5 = 1;
					offset6 = 0;
				} // Y X Z order
			}

			// A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
			// a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
			// a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
			// c = 1/6.

			// Offsets for second corner in (x,y,z) coordinates
			var corner1 = new DX.Vector3(distance.X - offset1 + G3, distance.Y - offset2 + G3, distance.Z - offset3 + G3);

			// Offsets for third corner in (x,y,z) coordinates
			var corner2 = new DX.Vector3(distance.X - offset4 + OffsetAmount3D1, distance.Y - offset5 + OffsetAmount3D1,
				distance.Z - offset6 + OffsetAmount3D1);

			// Offsets for last corner in (x,y,z) coordinates
			var corner3 = new DX.Vector3(distance.X + OffsetAmount3D2, distance.Y + OffsetAmount3D2, distance.Z + OffsetAmount3D2);

			// Work out the hashed gradient indices of the four simplex corners
			int index0 = i & 255;
			int index1 = j & 255;
			int index2 = k & 255;

			// Calculate the contribution from the four corners
			float t0 = 0.5f - distance.X * distance.X - distance.Y * distance.Y - distance.Z * distance.Z;
			float t1 = 0.5f - corner1.X * corner1.X - corner1.Y * corner1.Y - corner1.Z * corner1.Z;
			float t2 = 0.5f - corner2.X * corner2.X - corner2.Y * corner2.Y - corner2.Z * corner2.Z;
			float t3 = 0.5f - corner3.X * corner3.X - corner3.Y * corner3.Y - corner3.Z * corner3.Z;
			float dot;
			DX.Vector3 grad;

			if (t0 > 0.0f)
			{
				grad = _grad3[_permMod12[index0 + _permutations[index1 + _permutations[index2]]]];
				DX.Vector3.Dot(ref grad, ref distance, out dot);
				contrib.X = t0 * t0 * t0 * t0 * dot;
			}

			if (t1 > 0.0f)
			{
				grad = _grad3[_permMod12[index0 + offset1 + _permutations[index1 + offset2 + _permutations[index2 + offset3]]]];
				DX.Vector3.Dot(ref grad, ref corner1, out dot);
				contrib.Y = t1 * t1 * t1 * t1 * dot;
			}

			if (t2 > 0.0f)
			{
				grad = _grad3[_permMod12[index0 + offset4 + _permutations[index1 + offset5 + _permutations[index2 + offset6]]]];
				DX.Vector3.Dot(ref grad, ref corner2, out dot);
				contrib.Z = t2 * t2 * t2 * t2 * dot;
			}

			if (t3 <= 0.0f)
			{
				return 32.0f * (contrib.X + contrib.Y + contrib.Z);
			}

			grad = _grad3[_permMod12[index0 + 1 + _permutations[index1 + 1 + _permutations[index2 + 1]]]];
			DX.Vector3.Dot(ref grad, ref corner3, out dot);
			contrib.W = t3 * t3 * t3 * t3 * dot;

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to stay just inside [-1,1]
			return 32.0f * (contrib.X + contrib.Y + contrib.Z + contrib.W);
		}

		/// <summary>
		/// Function to generate 4 dimensional simplex noise.
		/// </summary>
		/// <param name="value">The <see cref="DX.Vector4"/> value to use to generate the simplex noise value.</param>
		/// <returns>A <see cref="float"/> representing the simplex noise value.</returns>
		/// <remarks>
		/// <para>
		/// Simplex noise values similar to Perlin noise but with fewer artifacts and better performance. 
		/// </para>
		/// <para>
		/// This produces predictable random numbers based on the seed <paramref name="value"/> passed to the method. 
		/// </para>
		/// </remarks>
		public static float SimplexNoise(DX.Vector4 value)
		{
			// Noise contributions from the five corners
			DX.Vector4 contrib = DX.Vector4.Zero;

			// Skew the (x,y,z,w) space to determine which cell of 24 simplices we're in
			float s = (value.X + value.Y + value.Z + value.W) * F4; // Factor for 4D skewing

			var i = (int)(value.X + s).FastFloor();
			var j = (int)(value.Y + s).FastFloor();
			var k = (int)(value.Z + s).FastFloor();
			var l = (int)(value.W + s).FastFloor();

			float t = (i + j + k + l) * G4; // Factor for 4D unskewing

			// Unskew the cell origin back to (x,y,z,w) space
			var unskew = new DX.Vector4(i - t, j - t, k - t, l - t);

			// The x,y,z,w distances from the cell origin
			DX.Vector4.Subtract(ref value, ref unskew, out DX.Vector4 distance);

			// For the 4D case, the simplex is a 4D shape I won't even try to describe.
			// To find out which of the 24 possible simplices we're in, we need to
			// determine the magnitude ordering of x0, y0, z0 and w0.
			// Six pair-wise comparisons are performed between each possible pair
			// of the four coordinates, and the results are used to rank the numbers.
			int rankx = 0;
			int ranky = 0;
			int rankz = 0;
			int rankw = 0;
			if (distance.X > distance.Y)
			{
				rankx++;
			}
			else
			{
				ranky++;
			}
			if (distance.X > distance.Z)
			{
				rankx++;
			}
			else
			{
				rankz++;
			}
			if (distance.X > distance.W)
			{
				rankx++;
			}
			else
			{
				rankw++;
			}
			if (distance.Y > distance.Z)
			{
				ranky++;
			}
			else
			{
				rankz++;
			}
			if (distance.Y > distance.W)
			{
				ranky++;
			}
			else
			{
				rankw++;
			}
			if (distance.Z > distance.W)
			{
				rankz++;
			}
			else
			{
				rankw++;
			}

			// simplex[c] is a 4-vector with the numbers 0, 1, 2 and 3 in some order.
			// Many values of c will never occur, since e.g. x>y>z>w makes x<z, y<w and x<w
			// impossible. Only the 24 indices which have non-zero entries make any sense.
			// We use a thresholding to set the coordinates in turn from the largest magnitude.
			// Rank 3 denotes the largest coordinate.

			int offset1 = rankx >= 3 ? 1 : 0;
			int offset2 = ranky >= 3 ? 1 : 0;
			int offset3 = rankz >= 3 ? 1 : 0;
			int offset4 = rankw >= 3 ? 1 : 0;

			// Rank 2 denotes the second largest coordinate.
			int offset5 = rankx >= 2 ? 1 : 0;
			int offset6 = ranky >= 2 ? 1 : 0;
			int offset7 = rankz >= 2 ? 1 : 0;
			int offset8 = rankw >= 2 ? 1 : 0;

			// Rank 1 denotes the second smallest coordinate.
			int offset9 = rankx >= 1 ? 1 : 0;
			int offset10 = ranky >= 1 ? 1 : 0;
			int offset11 = rankz >= 1 ? 1 : 0;
			int offset12 = rankw >= 1 ? 1 : 0;

			// The fifth corner has all coordinate offsets = 1, so no need to compute that.

			// Offsets for second corner in (x,y,z,w) coordinates
			var corner1 = new DX.Vector4(distance.X - offset1 + G4, distance.Y - offset2 + G4, distance.Z - offset3 + G4, distance.W - offset4 + G4);
			// Offsets for third corner in (x,y,z,w) coordinates
			var corner2 = new DX.Vector4(distance.X - offset5 + OffsetAmount4D1, distance.Y - offset6 + OffsetAmount4D1, distance.Z - offset7 + OffsetAmount4D1, distance.W - offset8 + OffsetAmount4D1);
			// Offsets for fourth corner in (x,y,z,w) coordinates
			var corner3 = new DX.Vector4(distance.X - offset9 + OffsetAmount4D2, distance.Y - offset10 + OffsetAmount4D2, distance.Z - offset11 + OffsetAmount4D2, distance.W - offset12 + OffsetAmount4D2);
			// Offsets for last corner in (x,y,z,w) coordinates
			var corner4 = new DX.Vector4(distance.X + OffsetAmount4D3, distance.Y + OffsetAmount4D3, distance.Z + OffsetAmount4D3, distance.W + OffsetAmount4D3);

			// Work out the hashed gradient indices of the five simplex corners
			int index1 = i & 255;
			int index2 = j & 255;
			int index3 = k & 255;
			int index4 = l & 255;

			float t0 = 0.5f - distance.X * distance.X - distance.Y * distance.Y - distance.Z * distance.Z - distance.W * distance.W;
			float t1 = 0.5f - corner1.X * corner1.X - corner1.Y * corner1.Y - corner1.Z * corner1.Z - corner1.W * corner1.W;
			float t2 = 0.5f - corner2.X * corner2.X - corner2.Y * corner2.Y - corner2.Z * corner2.Z - corner2.W * corner2.W;
			float t3 = 0.5f - corner3.X * corner3.X - corner3.Y * corner3.Y - corner3.Z * corner3.Z - corner3.W * corner3.W;
			float t4 = 0.5f - corner4.X * corner4.X - corner4.Y * corner4.Y - corner4.Z * corner4.Z - corner4.W * corner4.W;
			DX.Vector4 grad;
			float dot;

			// Calculate the contribution from the five corners
			if (t0 > 0.0f)
			{
				grad = _grad4[_permMod32[index1 + _permutations[index2 + _permutations[index3 + _permutations[index4]]]]];
				DX.Vector4.Dot(ref grad, ref distance, out dot);
				contrib.X = t0 * t0 * t0 * t0 * dot;
			}

			if (t1 > 0.0f)
			{
				grad = _grad4[_permMod32[index1 + offset1 + _permutations[index2 + offset2 + _permutations[index3 + offset3 + _permutations[index4 + offset4]]]]];
				DX.Vector4.Dot(ref grad, ref corner1, out dot);
				contrib.Y = t1 * t1 * t1 * t1 * dot;
			}

			if (t2 > 0.0f)
			{
				grad = _grad4[_permMod32[index1 + offset5 + _permutations[index2 + offset6 + _permutations[index3 + offset7 + _permutations[index4 + offset8]]]]];
				DX.Vector4.Dot(ref grad, ref corner2, out dot);
				contrib.Z = t2 * t2 * t2 * t2 * dot;
			}

			if (t3 > 0.0f)
			{
				grad = _grad4[_permMod32[index1 + offset9 + _permutations[index2 + offset10 + _permutations[index3 + offset11 + _permutations[index4 + offset12]]]]];
				DX.Vector4.Dot(ref grad, ref corner3, out dot);
				contrib.W = t3 * t3 * t3 * t3 * dot;
			}

			if (t4 <= 0.0f)
			{
				return 27.0f * (contrib.X + contrib.Y + contrib.Z + contrib.W);
			}

			grad = _grad4[_permutations[index1 + 1 + _permutations[index2 + 1 + _permutations[index3 + 1 + _permutations[index4 + 1]]]] % 32];
			DX.Vector4.Dot(ref grad, ref corner4, out dot);
			float contrib5 = t4 * t4 * t4 * t4 * dot;

			// Sum up and scale the result to cover the range [-1,1]
			return 27.0f * (contrib.X + contrib.Y + contrib.Z + contrib.W + contrib5);
		}


		/// <summary>
		/// Function to return a random <see cref="float"/> number.
		/// </summary>
		/// <param name="start">The starting value for the random number.</param>
		/// <param name="end">The ending value for the random number range.  This value is inclusive.</param>
		/// <returns>A random <see cref="float"/> value between <paramref name="start"/> to <paramref name="end"/>.</returns>
		/// <remarks>
		/// This overload generates a random <see cref="float"/> number between the range of <paramref name="start"/> and <paramref name="end"/>.
		/// </remarks>
		public static float RandomSingle(float start, float end)
		{
			return start < end
				       ? (float)_rnd.NextDouble() * (end - start) + start
				       : (float)_rnd.NextDouble() * (start - end) + end;
		}

		/// <summary>
		/// Function to return a random <see cref="float"/> number.
		/// </summary>
		/// <param name="maxValue">The highest number for random values, this value is inclusive.</param>
		/// <returns>A random <see cref="float"/> value.</returns>.
		/// <remarks>
		/// This overload generates a random <see cref="float"/> number between the range of 0 and <paramref name="maxValue"/>.
		/// </remarks>
		public static float RandomSingle(float maxValue)
		{
			return RandomSingle(0, maxValue);
		}

		/// <summary>
		/// Function to return a random <see cref="float"/> number.
		/// </summary>
		/// <returns>A random <see cref="float"/> value between 0.0f and 1.0f.</returns>
		public static float RandomSingle()
		{
			return (float)_rnd.NextDouble();
		}

		/// <summary>
		/// Function to return a non-negative random <see cref="int"/>.
		/// </summary>
		/// <param name="start">Starting value for the random number.</param>
		/// <param name="end">Ending value for the random number range.  This value is not inclusive.</param>
		/// <returns>The random <see cref="int"/> value within the range of <paramref name="start"/> to <paramref name="end"/>.</returns>
		/// <remarks>
		/// This overload generates a random <see cref="int"/> number between the range of <paramref name="start"/> and <paramref name="end"/>-1.
		/// </remarks>
		public static int RandomInt32(int start, int end)
		{
			return _rnd.Next(start, end);
		}

		/// <summary>
		/// Function to return a non-negative random <see cref="int"/>.
		/// </summary>
		/// <param name="maxValue">The highest number for random values, this value is not inclusive.</param>
		/// <returns>A random number</returns>.
		/// <remarks>
		/// This overload generates a random <see cref="int"/> number between the range of 0 and <paramref name="maxValue"/>-1.
		/// </remarks>
		public static int RandomInt32(int maxValue)
		{
			return _rnd.Next(maxValue);
		}

		/// <summary>
		/// Function to return a non-negative random <see cref="int"/>.
		/// </summary>
		/// <returns>A random <see cref="int"/> value between 0 and <see cref="int.MaxValue"/>-1.</returns>
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
			_permMod12 = new byte[_permutations.Length];
            _permMod32 = new byte[_permutations.Length];

			for (int i = 0; i < _permMod12.Length; i++)
			{
				_permMod12[i] = (byte)(_permutations[i] % 12);
			    _permMod32[i] = (byte)(_permutations[i] % 32);
			}
		}
		#endregion
	}
}
