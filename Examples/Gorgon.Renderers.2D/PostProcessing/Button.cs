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
// Created: August 6, 2018 8:24:41 PM
// 
#endregion

using System;
using Gorgon.Graphics;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// The states for the button.
    /// </summary>
    public enum ButtonState
    {
        /// <summary>
        /// Button is active and not hovered.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Button has the mouse cursor over it.
        /// </summary>
        Hovered = 1
    }

    /// <summary>
    /// A button used to provide a GUI for the example.
    /// </summary>
    public class Button
    {
        #region Events.
        /// <summary>
        /// Event fired when the button is clicked.
        /// </summary>
        public event EventHandler Click;
        #endregion

        #region Variables.
        // Disabled background color
        private readonly GorgonColor _disabledBackColor = new GorgonColor(0, 0, 0, 0.3f);
        // Disabled foreground color
        private readonly GorgonColor _disabledForeColor = new GorgonColor(1, 1, 1, 0.5f);
        // Hovered background color
        private readonly GorgonColor _hoverBackColor = new GorgonColor(0, 0, 0.7f, 0.85f);
        // Hovered foreground color
        private readonly GorgonColor _hoverForeColor = new GorgonColor(0, 1, 1, 1.0f);
        // The standard background color.
        private readonly GorgonColor _backColor = new GorgonColor(0, 0, 0, 0.85f);
        // The standard foreground color.
        private readonly GorgonColor _foreColor = GorgonColor.White;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the state for the button.
        /// </summary>
        public ButtonState State
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the composition pass for this button.
        /// </summary>
        public IGorgon2DCompositorPass Pass
        {
            get;
        }

        /// <summary>
        /// Property to return the foreground color.
        /// </summary>
        public GorgonColor ForeColor
        {
            get
            {
                switch (State)
                {
                    case ButtonState.Hovered:
                        return IsDragging ? _foreColor : _hoverForeColor;
                    default:
                        return ((Pass.Enabled) || (IsDragging)) ? _foreColor : _disabledForeColor;
                }
            }
        }

        /// <summary>
        /// Property to return the background color.
        /// </summary>
        public GorgonColor BackColor
        {
            get
            {
                switch (State)
                {
                    case ButtonState.Hovered:
                        return IsDragging ? _backColor : _hoverBackColor;
                    default:
                        return ((Pass.Enabled) || (IsDragging)) ? _backColor : _disabledBackColor;
                }
            }
        }

        /// <summary>
        /// Property to set or return whether the button is dragging or not.
        /// </summary>
        public bool IsDragging
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the boundaries for the button.
        /// </summary>
        public DX.RectangleF Bounds
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the text for the button.
        /// </summary>
        public string Text => Pass.Name;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to trigger a button click.
        /// </summary>
        public void PerformClick()
        {
            EventHandler handler = Click;
            handler?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        /// <param name="pass">The composition pass.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pass"/> parameter is <b>null</b></exception>
        public Button(IGorgon2DCompositorPass pass) => Pass = pass ?? throw new ArgumentNullException(nameof(pass));
        #endregion
    }
}
