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
// Created: July 3, 2018 8:48:01 PM
// 
#endregion

using DX = SharpDX;

namespace Gorgon.Core
{
    /// <summary>
    /// Extension methods for fixing an issue in the SharpDX RectangleF class where the Contains method only takes a Rectangle and not a RectangleF.
    /// </summary>
    public static class SharpDXRectangleFFixes
    {
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="rect">The source rectangle to compare.</param>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <returns><b>true</b> if the rectangle is contained within the other rectangle, or <b>false</b> if not.</returns>
        public static bool Contains(this DX.RectangleF rect, DX.RectangleF value) => (rect.X <= value.X) && (value.Right <= rect.Right) && (rect.Y <= value.Y) && value.Bottom <= rect.Bottom;
    }
}
