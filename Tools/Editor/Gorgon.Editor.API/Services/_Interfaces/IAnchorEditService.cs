#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: July 11, 2020 9:54:43 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Editor.Rendering;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to edit an anchor point on a sprite.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this service when rendering the UI for editing the <see cref="GorgonSprite.Anchor"/> on a <see cref="GorgonSprite"/>. Plug in developers should use this to provide interaction with the anchor 
    /// point. 
    /// </para>
    /// </remarks>
    public interface IAnchorEditService
    {
        #region Events.
        /// <summary>
        /// Event triggered when the anchor position is updated.
        /// </summary>
        event EventHandler AnchorChanged;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the position of the sprite anchor.
        /// </summary>
        DX.Vector2 AnchorPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the center position of the sprite.
        /// </summary>
        DX.Vector2 CenterPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the camera for the renderer.
        /// </summary>
        IGorgon2DCamera Camera
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether we're in the middle of a drag operation or not.
        /// </summary>
        bool IsDragging
        {
            get;
        }

        
        #endregion

        #region Methods.
        /// <summary>
        /// Function to intercept keyboard key presses.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        /// <returns><b>true</b> if the event is handled, <b>false</b> if not.</returns>
        bool KeyDown(PreviewKeyDownEventArgs e);

        /// <summary>
        /// Function called when the mouse button is pressed.
        /// </summary>
        /// <param name="args">The mouse event arguments.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        bool MouseDown(MouseArgs args);

        /// <summary>
        /// Function called when the mouse button is moved.
        /// </summary>
        /// <param name="args">The mouse event arguments.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        bool MouseMove(MouseArgs args);

        /// <summary>
        /// Function called when the mouse button is released.
        /// </summary>
        /// <param name="args">The event arguments..</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        bool MouseUp(MouseArgs args);

        /// <summary>
        /// Function to reset the anchor value.
        /// </summary>
        void Reset();

        /// <summary>
        /// Function to render the anchor UI.
        /// </summary>
        void Render();
        #endregion
    }
}