// 
// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: August 18, 2018 10:41:23 AM
// 

using Gorgon.Math;

namespace Gorgon.Collections;

/// <summary>
/// A comparer for comparing floating point values using epsilon equality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonEpsilonFloatComparer"/> class.
/// </remarks>
/// <param name="epsilon">[Optional] The epsilon error value used to negate floating point drift.</param>
public class GorgonEpsilonFloatComparer(float epsilon = 1e-6f)
    : IComparer<float>
{
    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />.</returns>
    public int Compare(float x, float y) => x.EqualsEpsilon(y, epsilon) ? 0 : (x < y ? 1 : -1);
}
