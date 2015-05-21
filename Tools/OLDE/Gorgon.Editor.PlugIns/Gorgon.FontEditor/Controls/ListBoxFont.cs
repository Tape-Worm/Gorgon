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
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Gorgon.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Font list box control.
	/// </summary>
	sealed class ListBoxFont
		: ListBox
    {
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
        /// Raises the <see cref="E:System.Windows.Forms.ListBox.MeasureItem" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MeasureItemEventArgs" /> that contains the event data.</param>
	    protected override void OnMeasureItem(MeasureItemEventArgs e)
	    {
	        base.OnMeasureItem(e);

	        e.ItemHeight = 20;
	    }

	    /// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ListBox.DrawItem"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs"/> that contains the event data.</param>
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;

		    if ((e.Index < 0)
		        || (e.Index >= Items.Count))
		    {
		        return;
		    }

		    if (RightToLeft == RightToLeft.Yes)
		    {
		        flags |= TextFormatFlags.RightToLeft;
		    }

		    e.DrawBackground();

		    if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
		    {
		        e.DrawFocusRectangle();
		    }

		    string fontName = Items[e.Index].ToString();
	        Font cachedFont;

		    if (!GorgonFontEditorPlugIn.CachedFonts.TryGetValue(fontName, out cachedFont))
		    {
		        cachedFont = Font;
		    }

		    e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		    Size measure = TextRenderer.MeasureText(e.Graphics, fontName, Font, e.Bounds.Size, flags);
		    var textBounds = new Rectangle(e.Bounds.Width - measure.Width + e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
		    var fontBounds = new Rectangle(e.Bounds.Left, e.Bounds.Top, textBounds.X - 2, e.Bounds.Height);
		    TextRenderer.DrawText(e.Graphics, fontName, cachedFont, fontBounds, e.ForeColor, e.BackColor, flags);
		    TextRenderer.DrawText(e.Graphics, fontName, Font, textBounds, e.ForeColor, e.BackColor, flags);
		}

        /// <summary>
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	    protected override void OnResize(EventArgs e)
	    {
	        base.OnResize(e);

	        Invalidate();
	    }

	    /// <summary>
		/// </summary>
		/// <param name="e">Event object with the details</param>
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);

		    if (Service != null)
		    {
		        Service.CloseDropDown();
		    }
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ListBoxFont"/> class.
		/// </summary>
		public ListBoxFont()
		{
		    DoubleBuffered = true;

			Items.Clear();

		    foreach (var font in FontFamily.Families)
		    {
		        Items.Add(font.Name);
		    }
            
            Width = 256;
        }
		#endregion
	}
}
