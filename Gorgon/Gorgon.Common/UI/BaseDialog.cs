#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, June 18, 2011 4:20:26 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using GorgonLibrary.Math;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// Base form for common dialogs.
	/// </summary>
	internal partial class BaseDialog 
        : Form
	{
		#region Variables.
		private string _message = string.Empty;             // Message to be displayed.
		private Point _textPosition = new Point(60, 2);		// Text position.
		private Size _maxTextSize;							// Maximum text size.
		#endregion

		#region Properties
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
		public int MessageHeight
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
		/// Property to set or return the image for the dialog.
		/// </summary>
		public Image DialogImage
		{
			get
			{
				return pictureDialog.Image;
			}
			set
			{
				pictureDialog.Image = value;
			}
		}

		/// <summary>
		/// Property to set or return the action for the default button.
		/// </summary>
		[Browsable(false)]
		public DialogResult ButtonAction
		{
			get
			{
				return buttonOK.DialogResult;
			}
			set
			{
				buttonOK.DialogResult = value;
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
		    {
		        Close();
		    }
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
		    if (DesignMode)
		    {
		        return;
		    }

			Screen currentScreen = Screen.FromControl(this);

			if (_maxTextSize.Height <= 0)
			{
				_maxTextSize.Height = currentScreen.WorkingArea.Height - (buttonOK.Height + 100);
			}

			_maxTextSize.Width = _maxTextSize.Width == 0 ? currentScreen.WorkingArea.Width / 4 : _maxTextSize.Width;

			// Inherit the parent icon.
		    if (Owner != null)
		    {
		        Icon = Owner.Icon;
		    }

		    ValidateFunctions();
		    OnSizeChanged(EventArgs.Empty);
		    Visible = true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Shown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

		    if ((!DesignMode) && (Owner != null))
		    {
		        Icon = Owner.Icon;
		    }
		}

		/// <summary>
		/// Function to perform the actual drawing of the dialog.
		/// </summary>
		/// <param name="g">Graphics object to use.</param>
		protected virtual void DrawDialog(Graphics g)
		{
		    // Get size.
			float maxTextHeight = AdjustSize(g, 0);

			// Relocate buttons.
			buttonOK.Left = ClientSize.Width - buttonOK.Width - 8;
			buttonOK.Top = ClientSize.Height - 6 - buttonOK.Height;
			DrawMessage(g, maxTextHeight);
		}

		/// <summary>
		/// Function to adjust the size of the form based on the details.
		/// </summary>
		/// <param name="g">Graphics interface.</param>
		/// <param name="margin">Places a margin at the bottom of the form.</param>
		/// <returns>The new maximum height of the client area.</returns>
		protected float AdjustSize(Graphics g, int margin) 
		{
		    g.PageUnit = GraphicsUnit.Pixel;
			SizeF textDimensions = g.MeasureString(_message, Font, _maxTextSize.Width);

			// Resize the form if needed.
			Size newClientSize = ClientSize;

			if (textDimensions.Width + _textPosition.X > newClientSize.Width)
				newClientSize.Width = (int)textDimensions.Width + _textPosition.X + 8;
			
			float maxTextHeight = textDimensions.Height;

		    if (maxTextHeight > _maxTextSize.Height)
		    {
		        maxTextHeight = _maxTextSize.Height;
		    }

		    if (maxTextHeight + 2 > newClientSize.Height - buttonOK.Height - (10 + margin))
		    {
		        newClientSize.Height = ((int) maxTextHeight) + 2 + buttonOK.Height + 10 + margin;
		    }
            
		    if ((ClientSize.Width != newClientSize.Width) || (ClientSize.Height != newClientSize.Height))
		    {
		        ClientSize = newClientSize;
		    }

		    return maxTextHeight;
		}

		/// <summary>
		/// Function to draw the dialog box message.
		/// </summary>
		/// <param name="g">Graphics interface.</param>
		/// <param name="maxTextHeight">Maximum height that the text will fit into.</param>
		protected void DrawMessage(Graphics g, float maxTextHeight)
		{
		    int borderHeight = buttonOK.Top - 5;

			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			SizeF textDimensions = g.MeasureString(_message, Font, new SizeF(_maxTextSize.Width - 2, maxTextHeight));
		    
            using (Brush backBrush = new SolidBrush(Color.FromArgb(255, 240, 240, 240)))
		    {
		        g.FillRectangle(backBrush, 0, 0, Size.Width, borderHeight);
		    }

		    g.FillRectangle(Brushes.White, 0, borderHeight + 1, Size.Width, DisplayRectangle.Bottom - borderHeight + 1);
			pictureDialog.Refresh();
			g.DrawLine(Pens.Black, new Point(0, borderHeight), new Point(Size.Width, borderHeight));
			g.DrawString(_message, Font, Brushes.Black, new RectangleF(_textPosition.X, 2, textDimensions.Width, maxTextHeight));
		}

		/// <summary>
		/// Size changed event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnSizeChanged(EventArgs e)
		{
		    if (DesignMode)
		    {
		        return;
		    }

		    // Reposition.
		    if ((StartPosition != FormStartPosition.CenterScreen) && (StartPosition != FormStartPosition.CenterParent))
		    {
		        return;
		    }

		    if (Parent == null) 
		    {
		        Left = (Screen.FromControl(this).WorkingArea.Width / 2) - (Width / 2);
		        Top = (Screen.FromControl(this).WorkingArea.Height / 2) - (Height / 2);
		    } 
            else
		    {
		        Left = (Parent.Width / 2) - (Width / 2);
		        Top = (Parent.Height / 2) - (Height / 2);
		    }
		}

        /// <summary>
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs" /> instance containing the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

            if (!DesignMode)
            {
                DrawDialog(e.Graphics);
            }
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