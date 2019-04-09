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
// Created: April 8, 2019 9:21:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
	/// <summary>
    /// The service used to edit sprite vertices.
    /// </summary>
    internal interface ISpriteVertexEditService
        : IDisposable
    {
        #region Events.
        /// <summary>Event triggered when the keyboard icon is clicked.</summary>
        event EventHandler KeyboardIconClicked;

        /// <summary>
        /// Event triggered when the vertex coordinates have been altered.
        /// </summary>
        event EventHandler VerticesChanged;

        /// <summary>
        /// Event triggered when a vertex is selected or deselected.
        /// </summary>
        event EventHandler VertexSelected;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether we're in the middle of a drag operation or not.
        /// </summary>
        bool IsDragging
        {
            get;
        }

        /// <summary>
        /// Property to set or return the vertices for the sprite.
        /// </summary>
        IReadOnlyList<DX.Vector2> Vertices
        {
            get;
            set;
        }

		/// <summary>
        /// Property to set or return the rectangular boundaries for the sprite.
        /// </summary>
		DX.RectangleF SpriteBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the mouse position in the client area of the primary rendering window.
        /// </summary>
        DX.Vector2 MousePosition
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the function used to transform a rectangle from local clip space to window client space.
        /// </summary>
        Func<DX.RectangleF, DX.RectangleF> RectToClient
        {
            get;
            set;
        }

		/// <summary>
        /// Property to set or return the function used to transform a point from local clip space to window client space.
        /// </summary>
		Func<DX.Vector2, DX.Vector2> PointToClient
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the function used to transform a point from window client space into local clip space.
        /// </summary>
        Func<DX.Vector2, DX.Vector2> PointFromClient
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the selected vertex index.
        /// </summary>
        int SelectedVertexIndex
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when a key is held down.
        /// </summary>
        /// <param name="key">The key that was held down.</param>
        /// <param name="modifiers">The modifier keys held down with the <paramref name="key"/>.</param>
        /// <returns><b>true</b> if the key was handled, <b>false</b> if it was not.</returns>
        bool KeyDown(Keys key, Keys modifiers);

        /// <summary>
        /// Function called when the mouse button is moved.
        /// </summary>
        /// <param name="button">The button that was held down while moving.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        bool MouseMove(MouseButtons button);

        /// <summary>
        /// Function called when the mouse button is pressed.
        /// </summary>
        /// <param name="button">The button that was pressed.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        bool MouseDown(MouseButtons button);

        /// <summary>
        /// Function called when the mouse button is released.
        /// </summary>
        /// <param name="button">The mouse button that was released.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        bool MouseUp(MouseButtons button);

        /// <summary>
        /// Function to render the editor UI.
        /// </summary>
        void Render();

		/// <summary>
        /// Function to refresh the UI of the editor.
        /// </summary>
        void Refresh();
		#endregion
    }
}
