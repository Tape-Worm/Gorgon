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
// Created: Thursday, April 26, 2012 8:50:30 AM
// 
#endregion

namespace Gorgon.UI;

/// <summary>
/// Values for aligning.
/// </summary>
public enum Alignment
{
    /// <summary>
    /// No alignment.
    /// </summary>
    None = 0,
    /// <summary>
    /// Upper left corner.
    /// </summary>
    UpperLeft = 1,
    /// <summary>
    /// Vertically centered, and left aligned.
    /// </summary>
    CenterLeft = 2,
    /// <summary>
    /// Lower left corner.
    /// </summary>
    LowerLeft = 3,
    /// <summary>
    /// Centered horizontally, aligned to the top.
    /// </summary>
    UpperCenter = 4,
    /// <summary>
    /// Centered vertically and horizontally.
    /// </summary>
    Center = 5,
    /// <summary>
    /// Centered horizontally, aligned to the bottom.
    /// </summary>
    LowerCenter = 6,
    /// <summary>
    /// Upper right corner.
    /// </summary>
    UpperRight = 7,
    /// <summary>
    /// Vertically centered, aligned to the right.
    /// </summary>
    CenterRight = 8,
    /// <summary>
    /// Lower right corner.
    /// </summary>
    LowerRight = 9
}
