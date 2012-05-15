#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, May 06, 2012 10:39:08 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Font list box control.
	/// </summary>
	class FontListBox
		: ListBox
	{
		#region Variables.
		private StringFormat _format = null;					// String format.
		private bool _disposed = false;							// Flag to indicate that we've disposed the object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the editor service to use.
		/// </summary>
		public IWindowsFormsEditorService Service
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"/> and its child controls and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_format != null)
						_format.Dispose();
				}

				_format = null;
				_disposed = true;
			}
			base.Dispose(disposing);
		}
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ListBox.DrawItem"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs"/> that contains the event data.</param>
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			if ((e.Index < 0) || (e.Index >= Items.Count))
				return;

			if (_format == null)
			{
				_format = new StringFormat(StringFormatFlags.NoWrap);
				_format.Trimming = StringTrimming.EllipsisCharacter;
				_format.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
				_format.Alignment = StringAlignment.Near;
				_format.LineAlignment = StringAlignment.Center;

				if ((this.RightToLeft & System.Windows.Forms.RightToLeft.No) != System.Windows.Forms.RightToLeft.No)
					_format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}

			e.DrawBackground();

			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
				e.DrawFocusRectangle();

			string fontName = Items[e.Index].ToString();
			if (Program.CachedFonts.ContainsKey(fontName))
			{
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				SizeF measure = e.Graphics.MeasureString(fontName, this.Font, e.Bounds.Width, _format);
				RectangleF textBounds = new RectangleF(e.Bounds.Width - measure.Width + e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
				RectangleF fontBounds = new RectangleF(e.Bounds.Left, e.Bounds.Top, textBounds.X - 2, e.Bounds.Height);
				e.Graphics.DrawString(fontName, Program.CachedFonts[fontName], Brushes.Black, fontBounds, _format);
				e.Graphics.DrawString(fontName, this.Font, Brushes.Black, textBounds, _format);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="e">Event object with the details</param>
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);

			if (Service != null)
				Service.CloseDropDown();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FontListBox"/> class.
		/// </summary>
		public FontListBox()
		{
			Items.Clear();

			foreach (var font in Program.CachedFonts)
				Items.Add(font.Key);

			this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.ItemHeight = 19;
			this.Width = 256;
		}
		#endregion

	}
}
