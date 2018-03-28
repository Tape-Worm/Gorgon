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
// Created: March 27, 2018 3:36:42 PM
// 
#endregion

using System.Drawing;
using System.Windows.Forms;

namespace Gorgon.UI
{
    /// <summary>
    /// A pre filter for window messages so we can hook specific mouse messages for our window.
    /// </summary>
    internal class WindowMessagePreFilter
        : IMessageFilter
    {
        #region Constants.
        // Mouse move message.
        private const int WmMouseMove = 0x0200;
        // Mouse down message.
        private const int WmLButtonDown = 0x201;
        // Left button.
        private const int MkLButton = 1;
        // Right button.
        private const int MkRButton = 2;
        // Middle button.
        private const int MkMButton = 0x10;
        // Button 4.
        private const int MkX1 = 0x20;
        // Button 5.
        private const int MkX2 = 0x40;
        #endregion

        #region Variables.
        // Our flat form instance.
        private readonly GorgonFlatForm _flatForm;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to get the mouse buttons.
        /// </summary>
        /// <param name="wParam">The wParam from the window message.</param>
        /// <returns>The mouse buttons.</returns>
        private static MouseButtons GetButtons(int wParam)
        {
            MouseButtons buttons = MouseButtons.None;

            if ((wParam & MkLButton) == MkLButton)
            {
                buttons |= MouseButtons.Left;
            }

            if ((wParam & MkRButton) == MkRButton)
            {
                buttons |= MouseButtons.Right;
            }

            if ((wParam & MkMButton) == MkMButton)
            {
                buttons |= MouseButtons.Middle;
            }

            if ((wParam & MkX1) == MkX1)
            {
                buttons |= MouseButtons.XButton1;
            }

            if ((wParam & MkX2) == MkX2)
            {
                buttons |= MouseButtons.XButton2;
            }

            return buttons;
        }

        /// <summary>
        /// Function to handle the mouse move event.
        /// </summary>
        /// <param name="wParam">Parameter set 1.</param>
        /// <returns><b>true</b> to continue processing the message, or <b>false</b> to stop it from going further.</returns>
        private bool HandleMouseMove(int wParam)
        {
            MouseButtons buttons = GetButtons(wParam);

            Point clientMouse = _flatForm.PointToClient(Control.MousePosition);

            var e = new MouseEventArgs(buttons, 0, clientMouse.X, clientMouse.Y, 0);

            return _flatForm.Form_MouseMove(e);
        }

        /// <summary>
        /// Function to handle the mouse left button down event.
        /// </summary>
        /// <returns><b>true</b> to continue processing the message, or <b>false</b> to stop it from going further.</returns>
        private bool HandleLButtonDown()
        {
            Point clientMouse = _flatForm.PointToClient(Control.MousePosition);
            
            var e = new MouseEventArgs(MouseButtons.Left, 0, clientMouse.X, clientMouse.Y, 0);

            return _flatForm.Form_MouseDown(e);
        }

        /// <summary>Filters out a message before it is dispatched.</summary>
        /// <param name="m">The message to be dispatched. You cannot modify this message. </param>
        /// <returns>
        /// <see langword="true" /> to filter the message and stop it from being dispatched; <see langword="false" /> to allow the message to continue to the next filter or control.</returns>
        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WmMouseMove:
                    return HandleMouseMove(m.WParam.ToInt32());
                case WmLButtonDown:
                    return HandleLButtonDown();
            }

            return false;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowMessagePreFilter"/> class.
        /// </summary>
        /// <param name="form">The form we use for processing.</param>
        public WindowMessagePreFilter(GorgonFlatForm form)
        {
            _flatForm = form;
        }
        #endregion
    }
}
