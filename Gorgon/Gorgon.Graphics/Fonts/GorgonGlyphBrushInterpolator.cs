#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Sunday, March 2, 2014 9:34:26 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// An interpolation value used to weight the color blending in a gradient brush.
	/// </summary>
	public struct GorgonGlyphBrushInterpolator
		: IEquatable<GorgonGlyphBrushInterpolator>, IComparable<GorgonGlyphBrushInterpolator>
	{
		#region Variables.
		/// <summary>
		/// The interpolation weight.
		/// </summary>
		public readonly float Weight;
		/// <summary>
		/// The interpolation color.
		/// </summary>
		public readonly GorgonColor Color;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to write the interpolation value to a chunk writer.
		/// </summary>
		internal void WriteChunk(GorgonChunkWriter writer)
		{
			writer.WriteFloat(Weight);
			writer.Write(Color);
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left value to compare.</param>
		/// <param name="right">Right value to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonGlyphBrushInterpolator left, ref GorgonGlyphBrushInterpolator right)
		{
			return left.Weight.EqualsEpsilon(right.Weight);
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_FONT_BRUSH_INTERPOL_TOSTRING,
			                     Weight,
			                     Color.Red,
			                     Color.Green,
			                     Color.Blue,
			                     Color.Alpha);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Weight);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonGlyphBrushInterpolator)
			{
				return ((GorgonGlyphBrushInterpolator)obj).Equals(this);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Operator to compare two instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to compare two instances for inequality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to determine if one instance is less than the other.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if the left is less than the right, <b>false</b> if not.</returns>
		public static bool operator <(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right)
		{
			return left.Weight < right.Weight;
		}

		/// <summary>
		/// Operator to determine if one instance is greater than the other.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if the left is greater than the right, <b>false</b> if not.</returns>
		public static bool operator >(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right)
		{
			return left.Weight > right.Weight;
		}

		/// <summary>
		/// Operator to determine if one instance is less than or equal to the other.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if the left is less than or equal to the right, <b>false</b> if not.</returns>
		public static bool operator <=(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right)
		{
			return left.Weight < right.Weight || Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to determine if one instance is greater than or equal to the other.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if the left is greater than or equal to the right, <b>false</b> if not.</returns>
		public static bool operator >=(GorgonGlyphBrushInterpolator left, GorgonGlyphBrushInterpolator right)
		{
			return left.Weight > right.Weight || Equals(ref left, ref right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyphBrushInterpolator"/> struct.
		/// </summary>
		/// <param name="reader">The chunk reader to retrieve the values from.</param>
		internal GorgonGlyphBrushInterpolator(GorgonChunkReader reader)
		{
			Weight = reader.ReadFloat();
			Color = reader.Read<GorgonColor>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyphBrushInterpolator"/> struct.
		/// </summary>
		/// <param name="weight">The weight in the interpolation.</param>
		/// <param name="color">The color at the interpolation weight.</param>
		public GorgonGlyphBrushInterpolator(float weight, GorgonColor color)
		{
			Weight = weight.Min(1.0f).Max(0);
			Color = color;
		}
		#endregion

		#region IEquatable<GorgonGlyphBrushInterpolator> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonGlyphBrushInterpolator other)
		{
			return Equals(ref this, ref other);
		}
		#endregion

		#region IComparable<GorgonGlyphBrushInterpolator> Members
		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
		/// </returns>
		public int CompareTo(GorgonGlyphBrushInterpolator other)
		{
			if (Weight < other.Weight)
			{
				return -1;
			}

			return Weight > other.Weight ? 1 : 0;
		}
		#endregion
	}
}
