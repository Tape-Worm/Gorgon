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
using System.Windows.Forms;
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
        #region Events.
        /// <summary>
        /// Event triggered when the scroll values are updated.
        /// </summary>
        event EventHandler ScrollUpdated;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the offset for scrolling the render region.
        /// </summary>
        DX.Vector2 ScrollOffset
        {
            get;
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
        /// Property to return the current zoom scaling value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value will update while the zoom animation is running, so it may not be constant.
        /// </para>
        /// </remarks>
        float ZoomScaleValue
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not we're animating.
        /// </summary>
        bool IsAnimating
        {
            get;
        }
        
        /// <summary>
        /// Property to set or return the current texture array index to use.
        /// </summary>
        int TextureArrayIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the alpha for the background sprite texture.
        /// </summary>
        float TextureAlpha
        {
            get;
        }
        
        /// <summary>
        /// Property to return the currently active cursor.
        /// </summary>
        Cursor CurrentCursor
        {
            get;
        }

        /// <summary>
        /// Property to set or return the color of the sprite.
        /// </summary>
        IReadOnlyList<GorgonColor> SpriteColor
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return some information to display about the sprite given then current renderer context.
        /// </summary>
        string SpriteInfo
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to render the sprite content.
        /// </summary>
        void Render();

        /// <summary>
        /// Function to convert a point from client space into renderer space.
        /// </summary>
        /// <param name="point">The point coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        DX.Vector2 FromClient(DX.Vector2 point);

        /// <summary>
        /// Function to convert a point from client space into renderer space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        DX.RectangleF FromClient(DX.RectangleF rect);

        /// <summary>
        /// Function to convert a rectangle from renderer space into client space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        DX.RectangleF ToClient(DX.RectangleF rect);

        /// <summary>
        /// Function to convert a point from renderer space into client space.
        /// </summary>
        /// <param name="rect">The rectangle coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        DX.Vector2 ToClient(DX.Vector2 point);

        /// <summary>
        /// Function to set the current area on the image to look at.
        /// </summary>
        /// <param name="zoomScaleValue">The new scaling value to assign.</param>
        /// <param name="centerPoint">The center point of the zoomed area.</param>
        /// <param name="targetAlpha">The target alpha value for the background.</param>
        /// <param name="animate"><b>true</b> to animate the look at transition, <b>false</b> to snap to the specified values.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="centerPoint"/> must be a vector ranging from -1 to 1 on the X and Y axes, where -1, -1 equals the upper left corner, and 1, 1, equals the lower right corner of the image.
        /// </para>
        /// </remarks>
        void LookAt(float zoomScaleValue, DX.Vector2 centerPoint, float targetAlpha, bool animate);

        /// <summary>
        /// Function to load all required resources for the renderer.
        /// </summary>
        void Load();

        /// <summary>
        /// Function to unload all required resources for the renderer.
        /// </summary>
        void Unload();        
        #endregion
    }
}
