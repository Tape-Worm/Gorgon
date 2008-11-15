#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 12:17:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Dialogs
{
    /// <summary>
    /// Base form for common dialogs.
    /// </summary>
    public partial class BaseDialog : Form
    {
        #region Variables.
        private string _message = string.Empty;             // Message to be displayed.
        private Point _textPosition = new Point(72, 2);     // Text position.
        private Size _maxTextSize = new Size(327, 152);     // Maximum text size.
        #endregion

        #region Properties
        /// <summary>
        /// Property to set or return the horizontal position of the text.
        /// </summary>
        protected int MessageLeft
        {
            get
            {
                return _textPosition.X;
            }
            set
            {
                _textPosition.X = value;                
            }
        }

        /// <summary>
        /// Property to set or return the vertical position of the text.
        /// </summary>
        protected int MessageTop
        {
            get
            {
                return _textPosition.Y;
            }
            set
            {
                _textPosition.Y = value;
            }
        }

        /// <summary>
        /// Property to set or return the maximum width of the text.
        /// </summary>
        protected int MessageWidth
        {
            get
            {
                return _maxTextSize.Width;
            }
            set
            {
                _maxTextSize.Width = value;
            }
        }

        /// <summary>
        /// Property to set or return the maximum height of the text.
        /// </summary>
        protected int MessageHeight
        {
            get
            { 
                return _maxTextSize.Height;
            }
            set
            {
                _maxTextSize.Height = value;
            }
        }

        /// <summary>
        /// Property to set or return the message for the dialog.
        /// </summary>
        [Browsable(false)]
        public virtual string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                ValidateFunctions();
            }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape)
				Close();
		}

		/// <summary>
        /// Function to validate the various functions bound to the form.
        /// </summary>
        protected virtual void ValidateFunctions()
        {
        }

        /// <summary>
        /// Form load event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (!DesignMode)
            {
                // Inherit the parent icon.
                if (Owner != null)
                    Icon = Owner.Icon;

                ValidateFunctions();
                OnSizeChanged(EventArgs.Empty);
                Visible = true;
            }
        }

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Shown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if ((!DesignMode) && (Owner != null))
				Icon = Owner.Icon;
		}

		/// <summary>
		/// Function to perform the actual drawing of the dialog.
		/// </summary>
		/// <param name="g">Graphics object to use.</param>
        protected virtual void DrawDialog(System.Drawing.Graphics g)
		{
		}

        /// <summary>
        /// Function to adjust the size of the form based on the details.
        /// </summary>
        /// <param name="g">Graphics interface.</param>
		/// <param name="margin">Places a margin at the bottom of the form.</param>
        /// <returns>The new maximum height of the client area.</returns>
        protected float AdjustSize(System.Drawing.Graphics g, int margin) 
        {
            SizeF textDimensions = SizeF.Empty;                     // Text dimensions
            float maxTextHeight;                                    // Maximum text height
            Size newClientSize = Size.Empty;                        // New client size.
            
            g.PageUnit = GraphicsUnit.Pixel;
            textDimensions = g.MeasureString(_message, Font, _maxTextSize.Width);

            // Resize the form if needed.
            newClientSize = ClientSize;

            if (textDimensions.Width + _textPosition.X > newClientSize.Width)
                newClientSize.Width = (int)textDimensions.Width + _textPosition.X + 8;
            
            maxTextHeight = textDimensions.Height;
            if (maxTextHeight > _maxTextSize.Height) 
                maxTextHeight = _maxTextSize.Height;

            if (maxTextHeight + 2 > newClientSize.Height - OKButton.Height - (10 + margin))
                newClientSize.Height = ((int)maxTextHeight) + 2 + OKButton.Height + 10 + margin;
			
            
            if ((ClientSize.Width != newClientSize.Width) || (ClientSize.Height != newClientSize.Height))
                ClientSize = newClientSize;

            return maxTextHeight;
        }

        /// <summary>
        /// Function to draw the dialog box message.
        /// </summary>    /// 
        /// <param name="g">Graphics interface.</param>
        /// <param name="maxTextHeight">Maximum height that the text will fit into.</param>
        protected void DrawMessage(System.Drawing.Graphics g, float maxTextHeight)
        {
            SizeF textDimensions = SizeF.Empty;         // Text dimensions
            Brush backBrush;                            // Brush

            backBrush = new SolidBrush(BackColor);
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            textDimensions = g.MeasureString(_message, Font, new SizeF(ClientSize.Width - MessageLeft, maxTextHeight));
            g.FillRectangle(backBrush, MessageLeft, MessageTop, textDimensions.Width, maxTextHeight);
            g.DrawString(_message, Font, Brushes.Black, new RectangleF(MessageLeft, MessageTop, textDimensions.Width, maxTextHeight));

            backBrush.Dispose();
        }

        /// <summary>
        /// Size changed event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            if (!DesignMode) 
            {
                // Reposition.
                if ((StartPosition == FormStartPosition.CenterScreen) || (StartPosition == FormStartPosition.CenterParent)) 
                {
                    if (Parent == null) 
                    {
                        Left = (Screen.FromControl(this).WorkingArea.Width / 2) - (Width / 2);
                        Top = (Screen.FromControl(this).WorkingArea.Height / 2) - (Height / 2);
                    } else
                    {
                        Left = (Parent.Width / 2) - (Width / 2);
                        Top = (Parent.Height / 2) - (Height / 2);
                    }
                }
            }
        }

		/// <summary>
		/// Raises the <see cref="E:Paint"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (!DesignMode)
				DrawDialog(e.Graphics);
		}
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public BaseDialog()
        {
            InitializeComponent();
        }
        #endregion
    }
}