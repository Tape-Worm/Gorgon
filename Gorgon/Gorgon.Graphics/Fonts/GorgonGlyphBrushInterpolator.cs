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
using Gorgon.Math;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// An interpolation value used to weight the color blending in a gradient brush.
	/// </summary>
	public class GorgonGlyphBrushInterpolator
		: IEquatable<GorgonGlyphBrushInterpolator>, IComparable<GorgonGlyphBrushInterpolator>
	{
		#region Properties.
		/// <summary>
		/// Property to return the interpolation weight.
		/// </summary>
		public float Weight
		{
			get;
		}

		/// <summary>
		/// Property to return the interpolation color.
		/// </summary>
		public GorgonColor Color
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_TOSTR_FONT_BRUSH_INTERPOLATION,
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
		#endregion

		#region Constructor/Destructor.
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
			if (other == null)
			{
				return false;
			}

			return other.Weight.EqualsEpsilon(Weight) && Color.Equals(other.Color);
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
			if (other == null)
			{
				return -1;
			}

			if (Weight < other.Weight)
			{
				return -1;
			}

			return Weight > other.Weight ? 1 : 0;
		}
		#endregion
	}
}
