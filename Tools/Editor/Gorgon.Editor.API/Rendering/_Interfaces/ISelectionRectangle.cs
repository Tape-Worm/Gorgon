#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 24, 2019 5:41:36 PM
// 
#endregion

using System;
using System.Numerics;
using Gorgon.Graphics;
using DX = SharpDX;

namespace Gorgon.Editor.Rendering
{
    /// <summary>
    /// Used to render a selection rectangle on the UI.
    /// </summary>
    public interface ISelectionRectangle
        : IDisposable
    {
        /// <summary>
        /// Property to set or return the color for the selection rectangle.
        /// </summary>
        GorgonColor Color
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the speed of the overlay animation.
        /// </summary>
        Vector2 Speed
        {
            get;
            set;
        }

        /// <summary>
        /// Function to draw the selection region.
        /// </summary>
        /// <param name="region">The region to draw.</param>
        void Draw(DX.RectangleF region);
    }
}