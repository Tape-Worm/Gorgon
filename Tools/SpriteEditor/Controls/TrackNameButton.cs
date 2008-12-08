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
// Created: Sunday, December 07, 2008 10:39:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Visual element to display the track name.
	/// </summary>
	public partial class TrackNameButton : UserControl
	{
		#region Variables.
		private Drawing.Font _nameFont = null;									// Name font.
		private Drawing.Drawing2D.LinearGradientBrush _backBrush = null;		// Background cell brush.
		private Drawing.Drawing2D.LinearGradientBrush _hiliteBrush = null;		// Hilite cell brush.
		private Drawing.Drawing2D.LinearGradientBrush _backSelBrush = null;		// Background selected cell brush.
		private Drawing.Drawing2D.LinearGradientBrush _hiliteSelBrush = null;	// Hilite selected cell brush.
		private Drawing.SolidBrush _textBrush = null;							// Text brush.
		private bool _selected = false;											// Flag to indicate that the control is selected.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the track name.
		/// </summary>
		public string TrackName
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the track was selected or not.
		/// </summary>
		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
				Invalidate();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Drawing.StringFormat format = null;

			if (!Visible)
				return;

			try
			{
				format = new Drawing.StringFormat(Drawing.StringFormat.GenericDefault);
				format.Alignment = Drawing.StringAlignment.Center;
				format.LineAlignment = Drawing.StringAlignment.Center;
				if (!string.IsNullOrEmpty(TrackName))
				{
					if (!Selected)
					{
						e.Graphics.FillRectangle(_backBrush, _backBrush.Rectangle);
						e.Graphics.FillRectangle(_hiliteBrush, _hiliteBrush.Rectangle);
						e.Graphics.DrawString(TrackName, Font, _textBrush, new Drawing.PointF(ClientSize.Width / 2.0f, ClientSize.Height / 2.0f), format);
					}
					else
					{
						e.Graphics.FillRectangle(_backSelBrush, _backSelBrush.Rectangle);
						e.Graphics.FillRectangle(_hiliteSelBrush, _hiliteSelBrush.Rectangle);
						e.Graphics.DrawString(TrackName, _nameFont, _textBrush, new Drawing.PointF(ClientSize.Width / 2.0f, ClientSize.Height / 2.0f), format);
					}
				}
				else
					e.Graphics.DrawString("UNKNOWN TRACK TYPE.", _nameFont, _textBrush, new Drawing.PointF(0, 0), format);
			}
			finally
			{
				if (format != null)
					format.Dispose();

				format = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackNameButton"/> class.
		/// </summary>
		public TrackNameButton()
		{
			InitializeComponent();

			TrackName = string.Empty;
			_backBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 32), Drawing.Color.Black, Drawing.Color.DarkGray, 90.0f);
			_hiliteBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 16), Drawing.Color.White, Drawing.Color.Gray, 90.0f);
			_backSelBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 32), Drawing.Color.DarkBlue, Drawing.Color.LightBlue, 90.0f);
			_hiliteSelBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 16), Drawing.Color.White, Drawing.Color.LightBlue, 90.0f);
			_textBrush = new Drawing.SolidBrush(Drawing.Color.White);

			_nameFont = new Drawing.Font(this.Font.Name, Font.Size, Drawing.FontStyle.Bold);
		}
		#endregion
	}
}
