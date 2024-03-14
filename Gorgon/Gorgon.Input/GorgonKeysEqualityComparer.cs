#region MIT
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
// Created: Wednesday, July 15, 2015 8:59:32 PM
// 
#endregion

using System.Collections.Generic;
using System.Windows.Forms;

namespace Gorgon.Input;

/// <summary>
/// An equality comparer for the <see cref="Keys"/> enumeration.
/// </summary>
internal class GorgonKeysEqualityComparer
    : IEqualityComparer<Keys>
{
    #region IEqualityComparer<Keys> Members
    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object of type <see cref="Keys"/> to compare.</param>
    /// <param name="y">The second object of type <see cref="Keys"/> to compare.</param>
    /// <returns><b>true</b> if the specified objects are equal; otherwise, <b>false</b> if not.</returns>
    public bool Equals(Keys x, Keys y) => x == y;

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Keys" /> for which a hash code is to be returned.</param>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public int GetHashCode(Keys obj) => obj.GetHashCode();
    #endregion
}
