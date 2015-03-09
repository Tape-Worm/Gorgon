#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, March 9, 2015 12:37:10 AM
// 
#endregion

using System.Collections.Generic;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// A comparer used to determine equality between file extensions.
	/// </summary>
	public class GorgonFileExtensionComparer
		: IEqualityComparer<GorgonFileExtension>
	{
		#region IEqualityComparer<GorgonFileExtension> Members
		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <paramref name="x" /> to compare.</param>
		/// <param name="y">The second object of type <paramref name="y" /> to compare.</param>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		bool IEqualityComparer<GorgonFileExtension>.Equals(GorgonFileExtension x, GorgonFileExtension y)
		{
			return GorgonFileExtension.Equals(ref x, ref y);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		int IEqualityComparer<GorgonFileExtension>.GetHashCode(GorgonFileExtension obj)
		{
			return obj.GetHashCode();
		}
		#endregion
	}
}
