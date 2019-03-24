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
// Created: March 24, 2019 10:39:40 AM
// 
#endregion

using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Graphics.Imaging;
using System;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Defines what data to use as a mask when determining clip boundaries.
    /// </summary>
    internal enum ClipMask
    {
        /// <summary>
        /// Use the alpha channel of the image.
        /// </summary>
        Alpha = 0,
        /// <summary>
        /// Use the color channels of the image.
        /// </summary>
        Color = 1
    }

    /// <summary>
    /// A clipper that automatically defines the bounds of the sprite based on pixel data.
    /// </summary>
    internal interface IPickClipperService
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the region that was clipped.
        /// </summary>
        DX.RectangleF Rectangle
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the flag to determine which data to use as a mask when determining clip boundaries.
        /// </summary>
        ClipMask ClipMask
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the masking value to determine which data to stop at when determining clip boundaries.
        /// </summary>
        /// <remarks>
        /// If the <see cref="ClipMask"/> value is set to <see cref="SpriteEditor.ClipMask.Alpha"/>, then only the <see cref="GorgonColor.Alpha"/> component of the value is used, otherwise the 
        /// <see cref="GorgonColor.Red"/>, <see cref="GorgonColor.Green"/>, <see cref="GorgonColor.Blue"/> and <see cref="GorgonColor.Alpha"/> values are used.
        /// </remarks>
        GorgonColor ClipMaskValue
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the pixel data for the texture.
        /// </summary>
        IGorgonImageBuffer ImageData
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set or return the function used to transform the window client area mouse position to the image pixel space.
        /// </summary>
        Func<DX.Vector2, DX.Vector2> TransformMouseToImage
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to handle the mouse up event.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse cursor, relative to the image.</param>
        /// <param name="mouseButton">The mouse button that was released.</param>
        /// <param name="modifierKeys">The modifier keys held down while the mouse button was released.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if not.</returns>
        bool MouseUp(DX.Vector2 mousePosition, MouseButtons mouseButton, Keys modifierKeys);
        #endregion
    }
}