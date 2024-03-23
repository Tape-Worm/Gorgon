
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: March 15, 2019 12:38:55 PM
// 

using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics;

/// <summary>
/// Extension methods for the SharpDX rectangle and rectanglef types
/// </summary>
public static class SharpDXRectExtensions
{
    /// <summary>
    /// Function to compare viewports for equality using a read only reference.
    /// </summary>
    /// <param name="thisView">The view being compared.</param>
    /// <param name="other">The other view being compared.</param>
    /// <returns><b>true</b> if the two instances are equal, <b>false</b> if not.</returns>
    public static bool Equals(ref readonly this DX.ViewportF thisView, ref readonly DX.ViewportF other) => (thisView.X.EqualsEpsilon(other.X))
            && (thisView.Y.EqualsEpsilon(other.Y))
            && (thisView.Width.EqualsEpsilon(other.Width))
            && (thisView.Height.EqualsEpsilon(other.Height))
            && (thisView.MinDepth.EqualsEpsilon(other.MinDepth))
            && (thisView.MaxDepth.EqualsEpsilon(other.MaxDepth));
}
