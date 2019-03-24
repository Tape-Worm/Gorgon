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
// Created: March 25, 2019 9:53:42 AM
// 
#endregion

using DX = SharpDX;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model used to manipulate the settings for the plug in.
    /// </summary>
    internal interface ISettings
    {
        /// <summary>
        /// Property to set or return the type of masking to perform when picking a sprite using the <see cref="SpriteEditTool.SpritePick"/> tool.
        /// </summary>
        ClipMask ClipMaskType
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the sprite picker mask color.
        /// </summary>
        GorgonColor ClipMaskValue
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the position of the manual input window.
        /// </summary>
        DX.Rectangle? ManualRectInputBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the flag used to show a warning when a large image is being used with the <see cref="SpriteEditTool.SpritePick"/> tool.
        /// </summary>
        bool ShowImageSizeWarning
        {
            get;
            set;
        }
    }
}