using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// Equality comparer for the a <see cref="GorgonGlyphBrushInterpolator"/>.
	/// </summary>
	public class GorgonGlyphBrushInterpolatorEqualityComparer
		: IEqualityComparer<GorgonGlyphBrushInterpolator>
	{
		#region IEqualityComparer<GorgonGlyphBrushInterpolator> Members
		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <see cref="GorgonGlyphBrushInterpolator"/> to compare.</param>
		/// <param name="y">The second object of type <see cref="GorgonGlyphBrushInterpolator"/> to compare.</param>
		/// <returns><b>true</b> if the specified objects are equal; otherwise, <b>false</b> if not.</returns>
		public bool Equals(GorgonGlyphBrushInterpolator x, GorgonGlyphBrushInterpolator y)
		{
			if ((x == null) && (y == null))
			{
				return true;
			}

			if ((x == null) || (y == null))
			{
				return false;
			}

			return x.Equals(y);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public int GetHashCode(GorgonGlyphBrushInterpolator obj)
		{
			return obj == null ? 0 : obj.GetHashCode();
		}

		#endregion
	}
}
