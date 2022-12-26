#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: November 10, 2018 10:36:44 PM
// 
#endregion

namespace Gorgon.Editor.UI;

/// <summary>
/// The zoom levels for image magnification.
/// </summary>
public enum ZoomLevels
{
    /// <summary>
    /// Custom resize value.
    /// </summary>
    Custom = 0,
    /// <summary>
    /// Resize to the client area of the window.
    /// </summary>
    ToWindow = 1,
    /// <summary>
    /// Shrink to 12% of normal size.
    /// </summary>
    Percent12 = 2,
    /// <summary>
    /// Shrink to 25% of normal size.
    /// </summary>
    Percent25 = 3,
    /// <summary>
    /// Shrink to 50% of normal size.
    /// </summary>
    Percent50 = 4,
    /// <summary>
    /// Set to full size.
    /// </summary>
    Percent100 = 5,
    /// <summary>
    /// Expand to 200% of normal size.
    /// </summary>
    Percent200 = 6,
    /// <summary>
    /// Expand to 400% of normal size.
    /// </summary>
    Percent400 = 7,
    /// <summary>
    /// Expand to 800% of normal size.
    /// </summary>
    Percent800 = 8,
    /// <summary>
    /// Expand to 1600% of normal size.
    /// </summary>
    Percent1600 = 9,
    /// <summary>
    /// Expand to 3200% of normal size.
    /// </summary>
    Percent3200 = 10,
    /// <summary>
    /// Expand to 6400% of normal size.
    /// </summary>
    Percent6400 = 11
}
