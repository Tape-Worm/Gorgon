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
// Created: February 7, 2020 2:39:25 PM
// 
#endregion

using System.Windows.Forms;
using DX = SharpDX;

namespace Gorgon.Editor.Rendering
{
    /// <summary>
    /// Arguments for the <see cref="DefaultContentRenderer{T}.OnMouseMove(MouseArgs)"/> method.
    /// </summary>
    public class MouseArgs
    {
        /// <summary>
        /// Property to return the mouse position in client space.
        /// </summary>
        /// <remarks>
        /// This returns the location of the mouse in relation to the control.
        /// </remarks>
        public DX.Point ClientPosition
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the mouse position in camera space.
        /// </summary>
        /// <remarks>
        /// This returns the location of the mouse in relation to the transformed (i.e. panning and scaling applied) content render region, where 0x0 is the center of the screen.
        /// </remarks>
        public DX.Vector2 CameraSpacePosition
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return which of the mouse buttons are held down (or released when the button is released).
        /// </summary>
        public MouseButtons MouseButtons
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the signed count of the number of detents the mouse wheel has rotated.
        /// </summary>
        public int MouseWheelDelta
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to set or return whether the mouse event has been handled.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the number of times a button has been clicked.
        /// </summary>
        public int ButtonClickCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return which modifier keys (CTRL, ALT, and Shift) are active during the mouse event.
        /// </summary>
        public Keys Modifiers => Control.ModifierKeys;
    }
}
