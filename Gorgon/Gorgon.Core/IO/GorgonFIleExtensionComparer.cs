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

namespace Gorgon.IO;

/// <summary>
/// A comparer used to perform a comparison on two instances of <see cref="GorgonFileExtension"/>.
/// </summary>
public class GorgonFileExtensionComparer
    : IComparer<GorgonFileExtension>
{
    #region IComparer<GorgonFileExtension> Members
    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <para>
    /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.
    /// </para>
    /// <list type="table">
    ///		<listheader>
    ///			<term>Value</term>
    ///			<term>Meaning</term>
    ///		</listheader>
    ///		<item>
    ///			<term>
    ///			Less than zero
    ///			</term>
    ///			<term>
    ///			<paramref name="x" /> is less than <paramref name="y" />.
    ///			</term>			
    ///		</item>
    ///		<item>
    ///			<term>
    ///			Zero
    ///			</term>
    ///			<term>
    ///			<paramref name="x" /> equals <paramref name="y" />.
    ///			</term>
    ///		</item>
    ///		<item>
    ///			<term>
    ///			Greater than zero
    ///			</term>
    ///			<term>
    ///			<paramref name="x" /> is greater than <paramref name="y" />.
    ///			</term>
    ///		</item>
    /// </list>
    /// </returns>
    public int Compare(GorgonFileExtension x, GorgonFileExtension y) => string.Compare(x.Extension, y.Extension, StringComparison.OrdinalIgnoreCase);
    #endregion
}
