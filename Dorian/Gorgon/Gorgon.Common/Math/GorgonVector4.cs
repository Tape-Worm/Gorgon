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
// Created: Tuesday, January 10, 2012 3:35:20 PM
// 
#endregion

//
//  Most of the code in this file was modified or taken directly from the SlimMath project by Mike Popoloski.
//  SlimMath may be downloaded from: http://code.google.com/p/slimmath/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// A 4 dimensional vector.
	/// </summary>
	/// <remarks>
	/// Vector mathematics are commonly used in graphical 3D applications.  And other
	/// spatial related computations.
	/// This valuetype provides us a convienient way to use vectors and their operations.
	/// </remarks>
	[Serializable(), StructLayout(LayoutKind.Sequential, Pack = 4), TypeConverter(typeof(Design.GorgonVector4TypeConverter))]
	public struct GorgonVector4
		: IEquatable<GorgonVector4>, IFormattable
	{
		#region Variables.
		/// <summary>
		/// A vector with all elements set to zero.
		/// </summary>
		public readonly static GorgonVector4 Zero = new GorgonVector4(0);
		/// <summary>
		/// A vector with all elements set to one.
		/// </summary>
		public readonly static GorgonVector4 One = new GorgonVector4(1.0f);
		/// <summary>
		/// Unit X vector.
		/// </summary>
		public readonly static GorgonVector4 UnitX = new GorgonVector4(1.0f,0,0,0.0f);
		/// <summary>
		/// Unit Y vector.
		/// </summary>
		public readonly static GorgonVector4 UnitY = new GorgonVector4(0,1.0f,0,0.0f);
		/// <summary>
		/// Unit Z vector.
		/// </summary>
		public readonly static GorgonVector4 UnitZ = new GorgonVector4(0,0,1.0f,0.0f);
		/// <summary>
		/// Unit W vector.
		/// </summary>
		public readonly static GorgonVector4 UnitW = new GorgonVector4(0, 0, 0, 1.0f);
		/// <summary>
		/// The size of the vector in bytes.
		/// </summary>
		public readonly static int Size = Native.DirectAccess.SizeOf<GorgonVector4>();
		/// <summary>
		/// Horizontal position of the vector.
		/// </summary>
		public float X;
		/// <summary>
		/// Vertical position of the vector.
		/// </summary>
		public float Y;
		/// <summary>
		/// Depth position of the vector.
		/// </summary>
		public float Z;
		/// <summary>
		/// Homogeneous unit.
		/// </summary>
		public float W;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the length of this vector.
		/// </summary>
		public float Length
		{
			get
			{
				return GorgonMathUtility.Sqrt(X * X + Y * Y + Z * Z + W * W);
			}
		}

		/// <summary>
		/// Property to return the length of this vector squared.
		/// </summary>
		public float LengthSquare
		{
			get
			{
				return X * X + Y * Y + Z * Z + W * W;
			}
		}

		/// <summary>
		/// Property to return whether the vector is normalized or not.
		/// </summary>
		public bool IsNormalized
		{
			get
			{
				return GorgonMathUtility.Abs((X * X + Y * Y + Z * Z + W * W) - 1f) < 1e-6f;
			}
		}

		/// <summary>
		/// Property to set or return the components of the vector by an index.
		/// </summary>
		/// <remarks>The index must be between 0..3.</remarks>
		/// <exception cref="System.IndexOutOfRangeException">The index was not between 0 and 3.</exception>
		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					case 3:
						return W;
					default:
						throw new IndexOutOfRangeException();
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					case 2:
						Z = value;
						break;
					case 3:
						W = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get the absolute value of each element.
		/// </summary>
		public void Abs()
		{
			X = GorgonMathUtility.Abs(X);
			Y = GorgonMathUtility.Abs(Y);
			Z = GorgonMathUtility.Abs(Z);
			W = GorgonMathUtility.Abs(W);
		}

		/// <summary>
		/// Function to get the absolute values for the elements in a vector.
		/// </summary>
		/// <param name="vector">Vector to get absolute values for.</param>
		/// <returns>The absolute values.</returns>
		public static GorgonVector4 Abs(GorgonVector4 vector)
		{
			return new GorgonVector4(
				GorgonMathUtility.Abs(vector.X),
				GorgonMathUtility.Abs(vector.Y),
				GorgonMathUtility.Abs(vector.Z),
				GorgonMathUtility.Abs(vector.W)
			);
		}

		/// <summary>
		/// Function to get the absolute values for the elements in a vector.
		/// </summary>
		/// <param name="vector">Vector to get absolute values for.</param>
		/// <param name="result">The absolute values.</param>
		public static void Abs(ref GorgonVector4 vector, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				GorgonMathUtility.Abs(vector.X),
				GorgonMathUtility.Abs(vector.Y),
				GorgonMathUtility.Abs(vector.Z),
				GorgonMathUtility.Abs(vector.W)
			);
		}

		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <param name="result">The combined vectors.</param>
		public static void Add(ref GorgonVector4 left, ref GorgonVector4 right, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				left.X + right.X,
				left.Y + right.Y,
				left.Z + right.Z,
				left.W + right.W
			);
		}

		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>The combined vectors.</returns>
		public static GorgonVector4 Add(GorgonVector4 left, GorgonVector4 right)
		{
			return new GorgonVector4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		/// <summary>
		/// Function to calculate a <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 4D triangle.
		/// </summary>
		/// <param name="vertexCoordinate1">A <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertexCoordinate2">A <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertexCoordinate3">A <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertexCoordinate2"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertexCoordinate3"/>).</param>
		/// <param name="result">The 4D Cartesian coordinates of the specified point.</param>
		public static void Barycentric(ref GorgonVector4 vertexCoordinate1, ref GorgonVector4 vertexCoordinate2, ref GorgonVector4 vertexCoordinate3, float vertex2Weight, float vertex3Weight, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				(vertexCoordinate1.X + (vertex2Weight * (vertexCoordinate2.X - vertexCoordinate1.X))) + (vertex3Weight * (vertexCoordinate3.X - vertexCoordinate1.X)),
				(vertexCoordinate1.Y + (vertex2Weight * (vertexCoordinate2.Y - vertexCoordinate1.Y))) + (vertex3Weight * (vertexCoordinate3.Y - vertexCoordinate1.Y)),
				(vertexCoordinate1.Z + (vertex2Weight * (vertexCoordinate2.Z - vertexCoordinate1.Z))) + (vertex3Weight * (vertexCoordinate3.Z - vertexCoordinate1.Z)),
				(vertexCoordinate1.W + (vertex2Weight * (vertexCoordinate2.W - vertexCoordinate1.W))) + (vertex3Weight * (vertexCoordinate3.W - vertexCoordinate1.W))
			);
		}

		/// <summary>
		/// Function to calculate a <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 4D triangle.
		/// </summary>
		/// <param name="vertexCoordinate1">A <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
		/// <param name="vertexCoordinate2">A <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
		/// <param name="vertexCoordinate3">A <see cref="GorgonVector4"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
		/// <param name="vertex2Weight">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="vertexCoordinate2"/>).</param>
		/// <param name="vertex3Weight">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="vertexCoordinate3"/>).</param>
		/// <returns>The 4D Cartesian coordinates of the specified point.</returns>
		public GorgonVector4 Barycentric(GorgonVector4 vertexCoordinate1, GorgonVector4 vertexCoordinate2, GorgonVector4 vertexCoordinate3, float vertex2Weight, float vertex3Weight)
		{
			return new GorgonVector4((vertexCoordinate1.X + (vertex2Weight * (vertexCoordinate2.X - vertexCoordinate1.X))) + (vertex3Weight * (vertexCoordinate3.X - vertexCoordinate1.X)),
								(vertexCoordinate1.Y + (vertex2Weight * (vertexCoordinate2.Y - vertexCoordinate1.Y))) + (vertex3Weight * (vertexCoordinate3.Y - vertexCoordinate1.Y)),
								(vertexCoordinate1.Z + (vertex2Weight * (vertexCoordinate2.Z - vertexCoordinate1.Z))) + (vertex3Weight * (vertexCoordinate3.Z - vertexCoordinate1.Z)),
								(vertexCoordinate1.W + (vertex2Weight * (vertexCoordinate2.W - vertexCoordinate1.W))) + (vertex3Weight * (vertexCoordinate3.W - vertexCoordinate1.W)));
		}

		/// <summary>
		/// Function to calculate a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">The Catmull-Rom interpolation.</param>
		public static void CatmullRom(ref GorgonVector4 value1, ref GorgonVector4 value2, ref GorgonVector4 value3, ref GorgonVector4 value4, float amount, out GorgonVector4 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			result = new GorgonVector4(
				0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
				(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) +
				((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
				0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
				(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) +
				((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed)),
				0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) +
				(((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) +
				((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed)),
				0.5f * ((((2.0f * value2.W) + ((-value1.W + value3.W) * amount)) +
				(((((2.0f * value1.W) - (5.0f * value2.W)) + (4.0f * value3.W)) - value4.W) * squared)) +
				((((-value1.W + (3.0f * value2.W)) - (3.0f * value3.W)) + value4.W) * cubed))
			);
		}

		/// <summary>
		/// Function to calculate a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The Catmull-Rom interpolation.</returns>
		public static GorgonVector4 CatmullRom(GorgonVector4 value1, GorgonVector4 value2, GorgonVector4 value3, GorgonVector4 value4, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;

			return new GorgonVector4(0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) +
								(((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) +
								((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed)),
								0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) +
								(((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) +
								((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed)),
								0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) +
								(((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) +
								((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed)),
								0.5f * ((((2.0f * value2.W) + ((-value1.W + value3.W) * amount)) +
								(((((2.0f * value1.W) - (5.0f * value2.W)) + (4.0f * value3.W)) - value4.W) * squared)) +
								((((-value1.W + (3.0f * value2.W)) - (3.0f * value3.W)) + value4.W) * cubed)));
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the highest values of the two vectors.</param>
		public static void Ceiling(ref GorgonVector4 vector1, ref GorgonVector4 vector2, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				(vector1.X > vector2.X) ? vector1.X : vector2.X,
				(vector1.Y > vector2.Y) ? vector1.Y : vector2.Y,
				(vector1.Z > vector2.Z) ? vector1.Z : vector2.Z,
				(vector1.W > vector2.W) ? vector1.W : vector2.W
			);
		}

		/// <summary>
		/// Function to return a vector containing the highest values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the highest values of the two vectors.</returns>
		public static GorgonVector4 Ceiling(GorgonVector4 vector1, GorgonVector4 vector2)
		{
			return new GorgonVector4((vector1.X > vector2.X) ? vector1.X : vector2.X, (vector1.Y > vector2.Y) ? vector1.Y : vector2.Y,
								(vector1.Z > vector2.Z) ? vector1.Z : vector2.Z, (vector1.W > vector2.W) ? vector1.W : vector2.W);
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">The cross product of the two vectors.</param>
		public static void CrossProduct(ref GorgonVector4 left, ref GorgonVector4 right, out GorgonVector4 result)
		{
			result.X = ((left.X * right.Y) - (left.Y * right.X)) + ((left.X * right.Z) - (left.Z * right.X)) + ((left.Y * right.Z) - (left.Z * right.Y));
			result.Y = ((left.Z * right.Y) - (left.Y * right.Z)) + ((left.Y * right.W) - (left.W * right.Y)) + ((left.Z * right.W) - (left.W * right.Z));
			result.Z = ((left.X * right.Z) - (left.Z * right.X)) + ((left.W * right.X) - (left.X * right.W)) + ((left.Z * right.W) - (left.W * right.Z));
			result.W = ((left.Y * right.X) - (left.X * right.Y)) + ((left.W * right.X) - (left.X * right.W)) + ((left.W * right.Y) - (left.Y * right.W));
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The cross product of the two vectors.</returns>
		public static GorgonVector4 CrossProduct(GorgonVector4 left, GorgonVector4 right)
		{
			return new GorgonVector4(((left.X * right.Y) - (left.Y * right.X)) + ((left.X * right.Z) - (left.Z * right.X)) + ((left.Y * right.Z) - (left.Z * right.Y)),
								((left.Z * right.Y) - (left.Y * right.Z)) + ((left.Y * right.W) - (left.W * right.Y)) + ((left.Z * right.W) - (left.W * right.Z)),
								((left.X * right.Z) - (left.Z * right.X)) + ((left.W * right.X) - (left.X * right.W)) + ((left.Z * right.W) - (left.W * right.Z)),
								((left.Y * right.X) - (left.X * right.Y)) + ((left.W * right.X) - (left.X * right.W)) + ((left.W * right.Y) - (left.Y * right.W)));
		}

		/// <summary>
		/// Function to calculate the cosine of the elements of the vector.
		/// </summary>
		/// <param name="vector">The vector used to calculate the cosine.</param>
		/// <param name="result">A vector containing the cosine values.</param>
		public static void Cos(ref GorgonVector4 vector, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				GorgonMathUtility.Cos(vector.X),
				GorgonMathUtility.Cos(vector.Y),
				GorgonMathUtility.Cos(vector.Z),
				GorgonMathUtility.Cos(vector.W)
			);
		}

		/// <summary>
		/// Function to calculate the cosine of the elements of the vector.
		/// </summary>
		/// <param name="vector">The vector used to calculate the cosine.</param>
		/// <returns>A vector containing the cosine values.</returns>
		public static GorgonVector4 Cos(GorgonVector4 vector)
		{
			return new GorgonVector4(
				GorgonMathUtility.Cos(vector.X),
				GorgonMathUtility.Cos(vector.Y),
				GorgonMathUtility.Cos(vector.Z),
				GorgonMathUtility.Cos(vector.W)
			);
		}

		/// <summary>
		/// Function to clamp a vector to a specified range.
		/// </summary>
		/// <param name="value">Value to clamp.</param>
		/// <param name="min">Minimum value to evaluate against.</param>
		/// <param name="max">Maximum value to evaluate against.</param>
		/// <param name="result">The clamped vector.</param>
		public static void Clamp(ref GorgonVector4 value, ref GorgonVector4 min, ref GorgonVector4 max, out GorgonVector4 result)
		{
			float x = value.X;
			float y = value.Y;
			float z = value.Z;
			float w = value.W;

			x = (x < min.X) ? min.X : x;
			y = (y < min.Y) ? min.Y : y;
			z = (z < min.Z) ? min.Z : z;
			w = (w < min.W) ? min.W : w;
			x = (x > max.X) ? max.X : x;
			y = (y > max.Y) ? max.Y : y;
			z = (z > max.Z) ? max.Z : z;
			w = (w > max.W) ? max.W : w;

			result = new GorgonVector4(x, y, z, w);
		}

		/// <summary>
		/// Function to clamp a vector to a specified range.
		/// </summary>
		/// <param name="value">Value to clamp.</param>
		/// <param name="min">Minimum value to evaluate against.</param>
		/// <param name="max">Maximum value to evaluate against.</param>
		/// <returns>The clamped vector.</returns>
		public static GorgonVector4 Clamp(GorgonVector4 value, GorgonVector4 min, GorgonVector4 max)
		{
			float x = value.X;
			float y = value.Y;
			float z = value.Z;
			float w = value.W;

			x = (x < min.X) ? min.X : x;
			y = (y < min.Y) ? min.Y : y;
			z = (z < min.Z) ? min.Z : z;
			w = (w < min.W) ? min.W : w;
			x = (x > max.X) ? max.X : x;
			y = (y > max.Y) ? max.Y : y;
			z = (z > max.Z) ? max.Z : z;
			w = (w > max.W) ? max.W : w;

			return new GorgonVector4(x, y, z, w);
		}

		/// <summary>
		/// Function to calculate the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The distance between the two vectors.</param>
		public static void Distance(ref GorgonVector4 value1, ref GorgonVector4 value2, out float result)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			result = GorgonMathUtility.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
		}

		/// <summary>
		/// Function to calculate the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The distance between the two vectors.</returns>
		public static float Distance(GorgonVector4 value1, GorgonVector4 value2)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			return GorgonMathUtility.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
		}

		/// <summary>
		/// Function to calculate the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The squared distance between the two vectors.</param>
		public static void DistanceSquared(ref GorgonVector4 value1, ref GorgonVector4 value2, out float result)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			result = ((x * x) + (y * y) + (z * z) + (w * w));
		}

		/// <summary>
		/// Function to calculate the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		public static float DistanceSquared(GorgonVector4 value1, GorgonVector4 value2)
		{
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
			float w = value1.W - value2.W;

			return ((x * x) + (y * y) + (z * z) + (w * w));
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <param name="result">The quotient vector.</param>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static void Divide(ref GorgonVector4 left, float scalar, out GorgonVector4 result)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;

			result = new GorgonVector4(left.X * inverse, left.Y * inverse, left.Z * inverse, left.W * inverse);
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>The quotient vector.</returns>
		/// <exception cref="System.DivideByZeroException">The divisor was 0.</exception>
		public static GorgonVector4 Divide(GorgonVector4 left, float scalar)
		{
			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;

			return new GorgonVector4(left.X * inverse, left.Y * inverse, left.Z * inverse, left.W * inverse);
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <param name="result">The dot product.</param>
		public static void DotProduct(ref GorgonVector4 left, ref GorgonVector4 right, out float result)
		{
			result = left.X * right.X +
					left.Y * right.Y +
					left.Z * right.Z +
					left.W * right.W;
		}

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="left">Left vector to use in the dot product operation.</param>
		/// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The dot product.</returns>
		public static float DotProduct(GorgonVector4 left, GorgonVector4 right)
		{
			return (left.X * right.X +
					left.Y * right.Y +
					left.Z * right.Z +
					left.W * right.W);
		}

		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj is GorgonVector4)
				return Equals((GorgonVector4)obj);

			return false;
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonVector4 vector1, ref GorgonVector4 vector2)
		{
			return (vector1.X == vector2.X) && (vector1.Y == vector2.Y) &&
				(vector1.Z == vector2.Z) && (vector1.W == vector2.W);
		}

		/// <summary>
		/// Function to return whether two vectors are equal.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="epsilon">Epsilon value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonVector4 vector1, ref GorgonVector4 vector2, float epsilon)
		{
			return (GorgonMathUtility.EqualFloat(vector1.X, vector1.X, epsilon)) && (GorgonMathUtility.EqualFloat(vector1.Y, vector2.Y, epsilon)) && (GorgonMathUtility.EqualFloat(vector1.Z, vector2.Z, epsilon) && (GorgonMathUtility.EqualFloat(vector1.W, vector2.W, epsilon)));
		}

		/// <summary>
		/// Function to return whether this vector and another are equal.
		/// </summary>
		/// <param name="vector">Vector to compare.</param>
		/// <param name="epsilon">Epsilon value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(GorgonVector4 vector, float epsilon)
		{
			return (GorgonMathUtility.EqualFloat(X, vector.X, epsilon)) && (GorgonMathUtility.EqualFloat(Y, vector.Y, epsilon)) && (GorgonMathUtility.EqualFloat(Z, vector.Z, epsilon) && (GorgonMathUtility.EqualFloat(W, vector.W, epsilon)));
		}

		/// <summary>
		/// Function to take e raised to the elements in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each element of.</param>
		/// <param name="result">A vector containing e raised to the power of the components in the vector.</param>
		public static void Exp(ref GorgonVector4 value, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				GorgonMathUtility.Exp(value.X),
				GorgonMathUtility.Exp(value.Y),
				GorgonMathUtility.Exp(value.Z),
				GorgonMathUtility.Exp(value.W)
			);
		}

		/// <summary>
		/// Function to take e raised to the elements in the vector.
		/// </summary>
		/// <param name="value">The value to take e raised to each element of.</param>
		/// <returns>A vector containing e raised to the power of the components in the vector.</returns>
		public static GorgonVector4 Exp(GorgonVector4 value)
		{
			return new GorgonVector4(
				GorgonMathUtility.Exp(value.X),
				GorgonMathUtility.Exp(value.Y),
				GorgonMathUtility.Exp(value.Z),
				GorgonMathUtility.Exp(value.W)
			);
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <param name="result">A vector containing the lowest values of the two vectors.</param>
		public static void Floor(ref GorgonVector4 vector1, ref GorgonVector4 vector2, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				(vector1.X < vector2.X) ? vector1.X : vector2.X,
				(vector1.Y < vector2.Y) ? vector1.Y : vector2.Y,
				(vector1.Z < vector2.Z) ? vector1.Z : vector2.Z,
				(vector1.W < vector2.W) ? vector1.W : vector2.W
			);
		}

		/// <summary>
		/// Function to return a vector containing the lower values of the two vector values passed in.
		/// </summary>
		/// <param name="vector1">Vector to compare.</param>
		/// <param name="vector2">Vector to compare.</param>
		/// <returns>A vector containing the lowest values of the two vectors.</returns>
		public static GorgonVector4 Floor(GorgonVector4 vector1, GorgonVector4 vector2)
		{
			return new GorgonVector4((vector1.X < vector2.X) ? vector1.X : vector2.X, (vector1.Y < vector2.Y) ? vector1.Y : vector2.Y,
								(vector1.Z < vector2.Z) ? vector1.Z : vector2.Z, (vector1.W < vector2.W) ? vector1.W : vector2.W);
		}


		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>The hash code for this object.</returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(X).GenerateHash(Y).GenerateHash(Z).GenerateHash(W);
		}

		/// <summary>
		/// Function to calculate a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">First source position vector.</param>
		/// <param name="tangent1">First source tangent vector.</param>
		/// <param name="value2">Second source position vector.</param>
		/// <param name="tangent2">Second source tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">The Hermite spline interpolation.</param>
		public static void Hermite(ref GorgonVector4 value1, ref GorgonVector4 tangent1, ref GorgonVector4 value2, ref GorgonVector4 tangent2, float amount, out GorgonVector4 result)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			result = new GorgonVector4(
				(((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
				(((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
				(((value1.W * part1) + (value2.W * part2)) + (tangent1.W * part3)) + (tangent2.W * part4)
			);
		}

		/// <summary>
		/// Function to calculate a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">First source position vector.</param>
		/// <param name="tangent1">First source tangent vector.</param>
		/// <param name="value2">Second source position vector.</param>
		/// <param name="tangent2">Second source tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The Hermite spline interpolation.</returns>
		public static GorgonVector4 Hermite(GorgonVector4 value1, GorgonVector4 tangent1, GorgonVector4 value2, GorgonVector4 tangent2, float amount)
		{
			float squared = amount * amount;
			float cubed = amount * squared;
			float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
			float part2 = (-2.0f * cubed) + (3.0f * squared);
			float part3 = (cubed - (2.0f * squared)) + amount;
			float part4 = cubed - squared;

			return new GorgonVector4((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
								(((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
								(((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
								(((value1.W * part1) + (value2.W * part2)) + (tangent1.W * part3)) + (tangent2.W * part4));
		}

		/// <summary>
		/// Function to calculate a linear interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0.0f and 1.0f indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">The linear interpolation of the two vectors.</param>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
		/// </remarks>
		public static void Lerp(ref GorgonVector4 start, ref GorgonVector4 end, float amount, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				start.X + ((end.X - start.X) * amount),
				start.Y + ((end.Y - start.Y) * amount),
				start.Z + ((end.Z - start.Z) * amount),
				start.W + ((end.W - start.W) * amount)
			);
		}

		/// <summary>
		/// Function to calculate a linear interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0.0f and 1.0f indicating the weight of <paramref name="end"/>.</param>
		/// <returns>The linear interpolation of the two vectors.</returns>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <code>start + (end - start) * amount</code>
		/// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
		/// </remarks>
		public static GorgonVector4 Lerp(GorgonVector4 start, GorgonVector4 end, float amount)
		{
			return new GorgonVector4(start.X + ((end.X - start.X) * amount),
								start.Y + ((end.Y - start.Y) * amount),
								start.Z + ((end.Z - start.Z) * amount),
								start.W + ((end.W - start.W) * amount));
		}

		/// <summary>
		/// Function to return a vector containing the largest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">A vector with the maximum elements of each vector.</param>
		public static void Max(ref GorgonVector4 value1, ref GorgonVector4 value2, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				(value1.X > value2.X) ? value1.X : value2.X,
				(value1.Y > value2.Y) ? value1.Y : value2.Y,
				(value1.Z > value2.Z) ? value1.Z : value2.Z,
				(value1.W > value2.W) ? value1.W : value2.W
			);
		}

		/// <summary>
		/// Function to return a vector containing the largest elements of the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>A vector with the maximum elements of each vector.</returns>
		public static GorgonVector4 Max(GorgonVector4 value1, GorgonVector4 value2)
		{
			return new GorgonVector4(
				(value1.X > value2.X) ? value1.X : value2.X,
				(value1.Y > value2.Y) ? value1.Y : value2.Y,
				(value1.Z > value2.Z) ? value1.Z : value2.Z,
				(value1.W > value2.W) ? value1.W : value2.W
			);
		}

		/// <summary>
		/// Function to return a vector containing the smallest elements of the two vectors.
		/// </summary>
		/// >param name="value1">The first vector.>/param>
		/// >param name="value2">The second vector.>/param>
		/// >param name="result">A vector with the minimum elements of each vector.>/param>
		public static void Min(ref GorgonVector4 value1, ref GorgonVector4 value2, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				(value1.X < value2.X) ? value1.X : value2.X,
				(value1.Y < value2.Y) ? value1.Y : value2.Y,
				(value1.Z < value2.Z) ? value1.Z : value2.Z,
				(value1.W < value2.W) ? value1.W : value2.W
			);
		}

		/// >summary>
		/// Function to return a vector containing the smallest elements of the two vectors.
		/// >/summary>
		/// >param name="value1">The first vector.>/param>
		/// >param name="value2">The second vector.>/param>
		/// >returns>A vector with the minimum elements of each vector.>/returns>
		public static GorgonVector4 Min(GorgonVector4 value1, GorgonVector4 value2)
		{
			return new GorgonVector4(
				(value1.X < value2.X) ? value1.X : value2.X,
				(value1.Y < value2.Y) ? value1.Y : value2.Y,
				(value1.Z < value2.Z) ? value1.Z : value2.Z,
				(value1.W < value2.W) ? value1.W : value2.W
			);
		}

		/// <summary>
		/// Function to modulate the elements of two vectors.
		/// </summary>
		/// <param name="left">First vector to modulate.</param>
		/// <param name="right">Second vector to modulate.</param>
		/// <param name="result">A modulated vector.</param>
		public static void Modulate(ref GorgonVector4 left, ref GorgonVector4 right, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				left.X * right.X,
				left.Y * right.Y,
				left.Z * right.Z,
				left.W * right.W
			);
		}

		/// <summary>
		/// Function to modulate the elements of two vectors.
		/// </summary>
		/// <param name="left">First vector to modulate.</param>
		/// <param name="right">Second vector to modulate.</param>
		/// <returns>A modulated vector.</returns>
		public static GorgonVector4 Modulate(GorgonVector4 left, GorgonVector4 right)
		{
			return new GorgonVector4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="result">The product of the vector and the scalar.</param>
		public static void Multiply(ref GorgonVector4 left, float scalar, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				left.X * scalar,
				left.Y * scalar,
				left.Z * scalar,
				left.W * scalar
			);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>The product of the vector and the scalar.</returns>
		public static GorgonVector4 Multiply(GorgonVector4 left, float scalar)
		{
			return new GorgonVector4(left.X * scalar, left.Y * scalar, left.Z * scalar, left.W * scalar);
		}

		/// <summary>
		/// Function to negate this vector.
		/// </summary>
		public void Negate()
		{
			X = -X;
			Y = -Y;
			Z = -Z;
			W = -W;
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <param name="result">The negated vector.</param>
		public static void Negate(ref GorgonVector4 vector, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				-vector.X,
				-vector.Y,
				-vector.Z,
				-vector.W
			);
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <returns>The negated vector.</returns>
		public static GorgonVector4 Negate(GorgonVector4 vector)
		{
			return new GorgonVector4(-vector.X, -vector.Y, -vector.Z, -vector.W);
		}
		
		/// <summary>
		/// Function to normalize a vector into a unit vector.
		/// </summary>
		/// <param name="vector">Vector to normalize.</param>
		/// <param name="result">The normalized vector.</param>
		public static void Normalize(ref GorgonVector4 vector, out GorgonVector4 result)
		{
			float length = vector.Length;

			if (length > 1e-6f)
			{
				float invLength = 1.0f / length;

				result = new GorgonVector4(
					vector.X * invLength,
					vector.Y * invLength,
					vector.Z * invLength,
					vector.W * invLength
				);
			}
			else
				result = vector;
		}

		/// <summary>
		/// Function to normalize a vector into a unit vector.
		/// </summary>
		/// <param name="vector">Vector to normalize.</param>
		/// <returns>The normalized vector.</returns>
		public static GorgonVector4 Normalize(GorgonVector4 vector)
		{
			float length = vector.Length;

			if (length > 1e-6f)
			{
				float invLength = 1.0f / length;

				return new GorgonVector4(
					vector.X * invLength,
					vector.Y * invLength,
					vector.Z * invLength,
					vector.W * invLength
				);
			}
			else
				return vector;
		}

		/// <summary>
		/// Function to normalize this vector.
		/// </summary>
		public void Normalize()
		{
			float length = Length;

			if (length <= 1e-6f)
				return;

			float invLength = 1.0f / length;

			X *= invLength;
			Y *= invLength;
			Z *= invLength;
			W *= invLength;
		}

		/// <summary>
		/// Function to orthogonalize a list of vectors.
		/// </summary>
		/// <param name="destination">A list of orthogonalized vectors.</param>
		/// <param name="source">A list of vectors to orthogonalize.</param>
		/// <remarks>
		/// <para>Orthogonalization is the process of making all vectors orthogonal to each other. This
		/// means that any given vector in the list will be orthogonal to any other given vector in the
		/// list.</para>
		/// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
		/// tend to be numerically unstable. The numeric stability decreases according to the vectors
		/// position in the list so that the first vector is the most stable and the last vector is the
		/// least stable.</para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		public static void Orthogonalize(IList<GorgonVector4> destination, IList<GorgonVector4> source)
		{
			//Uses the modified Gram-Schmidt process.
			//q1 = m1
			//q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
			//q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
			//q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3
			//q5 = ...

			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			for (int i = 0; i < source.Count; ++i)
			{
				GorgonVector4 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					GorgonVector4 destVector = destination[r];
					float dotTop = 0.0f;
					float dotBottom = 0.0f;

					GorgonVector4.DotProduct(ref destVector, ref newvector, out dotTop);
					GorgonVector4.DotProduct(ref destVector, ref destVector, out dotBottom);

					GorgonVector4.Multiply(ref destVector, dotTop / dotBottom, out destVector);
					GorgonVector4.Subtract(ref newvector, ref destVector, out newvector);
				}

				destination[i] = newvector;
			}
		}

		/// <summary>
		/// Function to orthonormalize a list of vectors.
		/// </summary>
		/// <param name="destination">The list of orthonormalized vectors.</param>
		/// <param name="source">The list of vectors to orthonormalize.</param>
		/// <remarks>
		/// <para>Orthonormalization is the process of making all vectors orthogonal to each
		/// other and making all vectors of unit length. This means that any given vector will
		/// be orthogonal to any other given vector in the list.</para>
		/// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
		/// tend to be numerically unstable. The numeric stability decreases according to the vectors
		/// position in the list so that the first vector is the most stable and the last vector is the
		/// least stable.</para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
		public static void Orthonormalize(IList<GorgonVector4> destination, IList<GorgonVector4> source)
		{
			//Uses the modified Gram-Schmidt process.
			//Because we are making unit vectors, we can optimize the math for orthogonalization
			//and simplify the projection operation to remove the division.
			//q1 = m1 / |m1|
			//q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
			//q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|
			//q4 = (m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3) / |m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3|
			//q5 = ...

			if (source == null)
				throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Count < source.Count)
				throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

			for (int i = 0; i < source.Count; ++i)
			{
				GorgonVector4 newvector = source[i];

				for (int r = 0; r < i; ++r)
				{
					float dot = 0.0f;
					GorgonVector4 destVector = destination[r];

					GorgonVector4.DotProduct(ref destVector, ref newvector, out dot);
					GorgonVector4.Multiply(ref destVector, dot, out destVector);
					GorgonVector4.Subtract(ref newvector, ref destVector, out newvector);
				}

				GorgonVector4.Normalize(ref newvector, out newvector);
				destination[i] = newvector;
			}
		}

		/// <summary>
		/// Function to take the reciprocal of the elements in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <param name="result">The reciprocal of the vector.</param>
		public static void Reciprocal(ref GorgonVector4 value, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				1.0f / value.X,
				1.0f / value.Y,
				1.0f / value.Z,
				1.0f / value.W
			);
		}

		/// <summary>
		/// Function to find the square root of each element in the vector and then takes the reciprocal of each.
		/// </summary>
		/// <param name="value">The vector to take the square root and recpirocal of.</param>
		/// <param name="result">The square root and reciprocal of the input vector.</param>
		public static void ReciprocalSqrt(ref GorgonVector4 value, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				1.0f / GorgonMathUtility.Sqrt(value.X),
				1.0f / GorgonMathUtility.Sqrt(value.Y),
				1.0f / GorgonMathUtility.Sqrt(value.Z),
				1.0f / GorgonMathUtility.Sqrt(value.W)
			);
		}

		/// <summary>
		/// Function to take the reciprocal of the elements in the vector.
		/// </summary>
		/// <param name="value">The vector to take the reciprocal of.</param>
		/// <param name="returns">The reciprocal of the vector.</returns>
		public static GorgonVector4 Reciprocal(GorgonVector4 value)
		{
			return new GorgonVector4(
				1.0f / value.X,
				1.0f / value.Y,
				1.0f / value.Z,
				1.0f / value.W
			);
		}

		/// <summary>
		/// Function to find the square root of each element in the vector and then takes the reciprocal of each.
		/// </summary>
		/// <param name="value">The vector to take the square root and recpirocal of.</param>
		/// <returns>The square root and reciprocal of the input vector.</returns>
		public static GorgonVector4 ReciprocalSqrt(GorgonVector4 value)
		{
			return new GorgonVector4(
				1.0f / GorgonMathUtility.Sqrt(value.X),
				1.0f / GorgonMathUtility.Sqrt(value.Y),
				1.0f / GorgonMathUtility.Sqrt(value.Z),
				1.0f / GorgonMathUtility.Sqrt(value.W)
			);
		}

		/// <summary>
		/// Function to find the sine of the elements in the vector.
		/// </summary>
		/// <param name="vector">The vector to get the sine of.</param>
		/// <param name="result">The sine of the vector.</param>
		public static void Sin(ref GorgonVector4 vector, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				GorgonMathUtility.Sin(vector.X),
				GorgonMathUtility.Sin(vector.Y),
				GorgonMathUtility.Sin(vector.Z),
				GorgonMathUtility.Sin(vector.W)
			);
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		/// <param name="result">Rounded vector.</param>
		public static void Round(ref GorgonVector4 vector, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				GorgonMathUtility.RoundInt(vector.X),
				GorgonMathUtility.RoundInt(vector.Y),
				GorgonMathUtility.RoundInt(vector.Z),
				GorgonMathUtility.RoundInt(vector.W)
			);
		}

		/// <summary>
		/// Function to round the vector components to their nearest whole values.
		/// </summary>
		/// <param name="vector">Vector to round.</param>
		public static GorgonVector4 Round(GorgonVector4 vector)
		{
			return new GorgonVector4(GorgonMathUtility.RoundInt(vector.X), GorgonMathUtility.RoundInt(vector.Y), GorgonMathUtility.RoundInt(vector.Z), GorgonMathUtility.RoundInt(vector.W));
		}

		/// <summary>
		/// Function to find the sine of the elements in the vector.
		/// </summary>
		/// <param name="vector">The vector to get the sine of.</param>
		/// <param name="result">The sine of the vector.</param>
		public static GorgonVector4 Sin(GorgonVector4 vector)
		{
			return new GorgonVector4(
				GorgonMathUtility.Sin(vector.X),
				GorgonMathUtility.Sin(vector.Y),
				GorgonMathUtility.Sin(vector.Z),
				GorgonMathUtility.Sin(vector.W)
			);
		}

		/// <summary>
		/// Function to calculate a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <param name="result">The cubic interpolation of the two vectors.</param>
		public static void SmoothStep(ref GorgonVector4 start, ref GorgonVector4 end, float amount, out GorgonVector4 result)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));

			result = new GorgonVector4(
				start.X + ((end.X - start.X) * amount),
				start.Y + ((end.Y - start.Y) * amount),
				start.Z + ((end.Z - start.Z) * amount),
				start.W + ((end.W - start.W) * amount)
			);
		}

		/// <summary>
		/// Function to calculate a cubic interpolation between two vectors.
		/// </summary>
		/// <param name="start">Start vector.</param>
		/// <param name="end">End vector.</param>
		/// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
		/// <returns>The cubic interpolation of the two vectors.</returns>
		public static GorgonVector4 SmoothStep(GorgonVector4 start, GorgonVector4 end, float amount)
		{
			amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
			amount = (amount * amount) * (3.0f - (2.0f * amount));

			return new GorgonVector4(start.X + ((end.X - start.X) * amount),
								start.Y + ((end.Y - start.Y) * amount),
								start.Z + ((end.Z - start.Z) * amount),
								start.W + ((end.W - start.W) * amount));
		}

		/// <summary>
		/// Function to take the square root of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root of.</param>
		/// <param name="result">The square root of the vector.</param>
		public static void Sqrt(ref GorgonVector4 value, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				GorgonMathUtility.Sqrt(value.X),
				GorgonMathUtility.Sqrt(value.Y),
				GorgonMathUtility.Sqrt(value.Z),
				GorgonMathUtility.Sqrt(value.W)
			);
		}

		/// <summary>
		/// Function to take the square root of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the square root of.</param>
		/// <param name="result">The square root of the vector.</param>
		public static GorgonVector4 Sqrt(GorgonVector4 value)
		{
			return new GorgonVector4(
				GorgonMathUtility.Sqrt(value.X),
				GorgonMathUtility.Sqrt(value.Y),
				GorgonMathUtility.Sqrt(value.Z),
				GorgonMathUtility.Sqrt(value.W)
			);
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <param name="result">The difference between the two vectors.</param>
		public static void Subtract(ref GorgonVector4 left, ref GorgonVector4 right, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				left.X - right.X,
				left.Y - right.Y,
				left.Z - right.Z,
				left.W - right.W
			);
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>The difference between the two vectors.</returns>
		public static GorgonVector4 Subtract(GorgonVector4 left, GorgonVector4 right)
		{
			return new GorgonVector4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		/// <summary>
		/// Function to calculate the tangent of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <param name="result">The tangent of each element.</param>
		public static void Tan(ref GorgonVector4 value, out GorgonVector4 result)
		{
			result = new GorgonVector4(
				GorgonMathUtility.Tan(value.X),
				GorgonMathUtility.Tan(value.Y),
				GorgonMathUtility.Tan(value.Z),
				GorgonMathUtility.Tan(value.W)
			);
		}

		/// <summary>
		/// Function to calculate the tangent of each element in the vector.
		/// </summary>
		/// <param name="value">The vector to take the tangent of.</param>
		/// <returns>The tangent of each element.</returns>
		public static GorgonVector4 Tan(GorgonVector4 value)
		{
			return new GorgonVector4(
				GorgonMathUtility.Tan(value.X),
				GorgonMathUtility.Tan(value.Y),
				GorgonMathUtility.Tan(value.Z),
				GorgonMathUtility.Tan(value.W)
			);
		}

		/// <summary>
		/// Function to convert the vector into a 4 element array.
		/// </summary>
		/// <returns>A 4 element array containing the elements of the vector.</returns>
		/// <remarks>X is in the first element, Y is in the second element, Z is in the third element, W is in the fourth element.</remarks>
		public float[] ToArray()
		{
			return new[] { X, Y, Z, W };
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format("4D Vector-> X:{0}, Y:{1}, Z:{2}, W:{3}", X, Y, Z, W);
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <param name="provider">String formatting provider.</param>
		/// <returns>A string containing the type and values of this object.</returns>
		public string ToString(IFormatProvider provider)
		{
			return string.Format(provider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <param name="format">Format to use.</param>
		/// <returns>A string containing the type and values of this object.</returns>
		public string ToString(string format)
		{
			if (string.IsNullOrEmpty(format))
				return ToString();

			return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
		}

#warning Don't forget to implement these after Matrix and Quaternion are implemented (Transform/Coordinate/Normal).
		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <param name="result">The resulting transformed vector.</param>
		public static void Transform(ref GorgonMatrix m, ref GorgonVector4 vec, out GorgonVector4 result)
		{
			result.X = ((m.m11 * vec.X) + (m.m12 * vec.Y) + (m.m13 * vec.Z) + m.m14) * vec.W;
			result.Y = ((m.m21 * vec.X) + (m.m22 * vec.Y) + (m.m23 * vec.Z) + m.m24) * vec.W;
			result.Z = ((m.m31 * vec.X) + (m.m32 * vec.Y) + (m.m33 * vec.Z) + m.m34) * vec.W;
			result.W = ((m.m41 * vec.X) + (m.m42 * vec.Y) + (m.m43 * vec.Z) + m.m44) * vec.W;
		}

		/// <summary>
		/// Function to transform a vector by a given matrix.
		/// </summary>
		/// <param name="m">Matrix used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <returns>The resulting transformed vector.</returns>
		public static GorgonVector4 Transform(GorgonMatrix m, GorgonVector4 vec)
		{
			return new GorgonVector4(((m.m11 * vec.X) + (m.m12 * vec.Y) + (m.m13 * vec.Z) + m.m14) * vec.W,
								((m.m21 * vec.X) + (m.m22 * vec.Y) + (m.m23 * vec.Z) + m.m24) * vec.W,
								((m.m31 * vec.X) + (m.m32 * vec.Y) + (m.m33 * vec.Z) + m.m34) * vec.W,
								((m.m41 * vec.X) + (m.m42 * vec.Y) + (m.m43 * vec.Z) + m.m44) * vec.W);
		}

		/// <summary>
		/// Function to transform a vector by a given quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <param name="result">The resulting transformed vector.</param>
		public static void Transform(ref GorgonQuaternion q, ref GorgonVector4 vec, out GorgonVector4 result)
		{
			float x = q.X + q.X;
			float y = q.Y + q.Y;
			float z = q.Z + q.Z;
			float wx = q.W * x;
			float wy = q.W * y;
			float wz = q.W * z;
			float xx = q.X * x;
			float xy = q.X * y;
			float xz = q.X * z;
			float yy = q.Y * y;
			float yz = q.Y * z;
			float zz = q.Z * z;

			result.X = ((vec.X * ((1.0f - yy) - zz)) + (vec.Y * (xy - wz))) + (vec.Z * (xz + wy));
			result.Y = ((vec.X * (xy + wz)) + (vec.Y * ((1.0f - xx) - zz))) + (vec.Z * (yz - wx));
			result.Z = ((vec.X * (xz - wy)) + (vec.Y * (yz + wx))) + (vec.Z * ((1.0f - xx) - yy));
			result.W = vec.W;
		}

		/// <summary>
		/// Function to transform a vector by a given quaternion.
		/// </summary>
		/// <param name="q">Quaternion used to transform the vector.</param>
		/// <param name="vec">Vector to transform.</param>
		/// <returns>The resulting transformed vector.</returns>
		public static GorgonVector4 Transform(GorgonQuaternion q, GorgonVector4 vec)
		{
			float x = q.X + q.X;
			float y = q.Y + q.Y;
			float z = q.Z + q.Z;
			float wx = q.W * x;
			float wy = q.W * y;
			float wz = q.W * z;
			float xx = q.X * x;
			float xy = q.X * y;
			float xz = q.X * z;
			float yy = q.Y * y;
			float yz = q.Y * z;
			float zz = q.Z * z;

			return new GorgonVector4(((vec.X * ((1.0f - yy) - zz)) + (vec.Y * (xy - wz))) + (vec.Z * (xz + wy)),
								((vec.X * (xy + wz)) + (vec.Y * ((1.0f - xx) - zz))) + (vec.Z * (yz - wx)),
								((vec.X * (xz - wy)) + (vec.Y * (yz + wx))) + (vec.Z * ((1.0f - xx) - yy)),
								vec.W);
		}
		#endregion

		#region Operators
		/// <summary>
		/// Operator to perform equality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonVector4 left,GorgonVector4 right)
		{
			return (GorgonMathUtility.EqualFloat(left.X, right.X) && GorgonMathUtility.EqualFloat(left.Y, right.Y) &&
						GorgonMathUtility.EqualFloat(left.Z, right.Z) && GorgonMathUtility.EqualFloat(left.W, right.W));
		}

		/// <summary>
		/// Operator to perform inequality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(GorgonVector4 left,GorgonVector4 right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector4 operator +(GorgonVector4 left,GorgonVector4 right)
		{
			GorgonVector4 result;
			Add(ref left, ref right, out result);
			return result;
		}
		
		/// <summary>
		/// Operator to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector4 operator -(GorgonVector4 left,GorgonVector4 right)
		{
			GorgonVector4 result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to negate a vector.
		/// </summary>
		/// <param name="vector">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static GorgonVector4 operator -(GorgonVector4 vector)
		{
			GorgonVector4 result;
			Negate(ref vector, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector4 operator *(GorgonVector4 left,float scalar)
		{
			GorgonVector4 result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector4 operator *(float scalar,GorgonVector4 right)
		{
			GorgonVector4 result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static GorgonVector4 operator /(GorgonVector4 left,float scalar)
		{
			GorgonVector4 result;
			Divide(ref left, scalar, out result);
			return result;
		}		

		/// <summary>
		/// Operator to convert a 2D vector into a 4D vector.
		/// </summary>
		/// <param name="vector">2D vector.</param>
		/// <returns>4D vector.</returns>
		public static implicit operator GorgonVector4(GorgonVector2 vector)
		{
			return new GorgonVector4(vector, 0.0f, 1.0f);
		}

		/// <summary>
		/// Operator to convert a 3D vector into a 4D vector.
		/// </summary>
		/// <param name="vector">3D vector</param>
		/// <returns>4D vector.</returns>
		public static implicit operator GorgonVector4(GorgonVector3 vector)
		{
			return new GorgonVector4(vector, 1.0f);
		}

		/// <summary>
		/// Operator to convert a 4D vector into a 3D vector.
		/// </summary>
		/// <param name="vector">4D vector to convert.</param>
		/// <returns>3D vector.</returns>
		public static explicit operator GorgonVector3(GorgonVector4 vector)
		{
			return new GorgonVector3(vector.X, vector.Y, vector.Z);
		}

		/// <summary>
		/// Operator to convert a 4D vector into a 2D vector.
		/// </summary>
		/// <param name="vector">4D vector to convert.</param>
		/// <returns>2D vector.</returns>
		public static explicit operator GorgonVector2(GorgonVector4 vector)
		{
			return new GorgonVector2(vector.X, vector.Y);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector4"/> struct.
		/// </summary>
		/// <param name="x">Horizontal position of the vector.</param>
		/// <param name="y">Vertical posiition of the vector.</param>
		/// <param name="z">Depth position of the vector.</param>
		/// <param name="w">W component of the vector.</param>
		public GorgonVector4(float x,float y,float z,float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector4"/> struct.
		/// </summary>
		/// <param name="value">The value used to initialize the vector.</param>
		public GorgonVector4(float value)
		{
			W = Z = Y = X = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector4"/> struct.
		/// </summary>
		/// <param name="vector">The vector to convert.</param>
		/// <param name="w">W component of the vector.</param>
		public GorgonVector4(GorgonVector3 vector, float w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector4"/> struct.
		/// </summary>
		/// <param name="vector">The vector to convert.</param>
		/// <param name="z">Z component of the vector.</param>
		/// <param name="w">W component of the vector.</param>
		public GorgonVector4(GorgonVector2 vector, float z, float w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVector4"/> struct.
		/// </summary>
		/// <param name="values">The values for the vector.</param>
		/// <remarks>Only the first four elements in the list will be taken.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="values"/> parameter has less than 4 elements.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the values parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonVector4(IEnumerable<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Count() < 4)
				throw new ArgumentException("The number of elements must be at least 4 for a 4D vector.", "values");

			X = values.ElementAt(0);
			Y = values.ElementAt(1);
			Z = values.ElementAt(2);
			W = values.ElementAt(3);
		}
		#endregion

		#region IEquatable<GorgonVector4> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonVector4 other)
		{
			return (other.X == X) && (other.Y == Y) && (other.Z == Z) && (other.W == W);
		}
		#endregion

		#region IFormattable Members
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format))
				return ToString(formatProvider);

			return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
		}
		#endregion
	}
}
