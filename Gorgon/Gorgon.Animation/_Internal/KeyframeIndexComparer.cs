﻿
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 17, 2018 3:34:16 PM
// 

using Gorgon.Math;

namespace Gorgon.Animation;

/// <summary>
/// A comparer used to sort the indices of a key frame list based on frame time
/// </summary>
internal class KeyframeIndexComparer<T>
    : IComparer<T>
    where T : IGorgonKeyFrame
{
    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero
    /// <paramref name="x" /> is less than <paramref name="y" />.Zero
    /// <paramref name="x" /> equals <paramref name="y" />.Greater than zero
    /// <paramref name="x" /> is greater than <paramref name="y" />.</returns>
    public int Compare(T x, T y)
    {
        if ((x == null)
            || (y == null))
        {
            return -1;
        }

        if (x.Time.EqualsEpsilon(y.Time))
        {
            return 0;
        }

        return x.Time < y.Time ? -1 : 1;

    }
}
