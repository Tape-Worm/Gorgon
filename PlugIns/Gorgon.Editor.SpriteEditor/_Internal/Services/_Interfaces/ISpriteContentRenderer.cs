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
// Created: March 15, 2019 11:53:17 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Provides rendering functionality for the sprite editor.
    /// </summary>
    internal interface ISpriteContentRenderer
        : IDisposable
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the offset for scrolling the render region.
        /// </summary>
        DX.Vector2 ScrollOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the color to clear the background with.
        /// </summary>
        GorgonColor BackgroundColor
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current zoom scaling value.
        /// </summary>
        float ZoomScaleValue
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether or not we're animating.
        /// </summary>
        bool IsAnimating
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the current size of the sprite texture, in pixels.
        /// </summary>
        /// <param name="sprite">The currently active sprite.</param>
        /// <returns>The size of the sprite texture, after transformation, in pixels.</returns>
        DX.RectangleF GetSpriteTextureSize(ISpriteContent sprite);

        /// <summary>
        /// Function to render the sprite content.
        /// </summary>
        /// <param name="zoom">The amount of zoom to apply to the render area.</param>
        /// <param name="tool">The active tool for the editor.</param>
        void Render(ZoomLevels zoom, SpriteEditTool tool);

        /// <summary>
        /// Function to assign the sprite to use.
        /// </summary>
        /// <param name="sprite">The sprite to assign.</param>
        void SetSprite(ISpriteContent sprite);

        /// <summary>
        /// Function to load all required resources for the renderer.
        /// </summary>
        void Load();
        #endregion
    }
}
