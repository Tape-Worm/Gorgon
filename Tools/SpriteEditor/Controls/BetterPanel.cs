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
// Created: Sunday, December 07, 2008 6:15:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Panel control with some "fixes".
	/// </summary>
	public class BetterPanel
		: Panel
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object has been disposed.
		private Point _location = Point.Empty;	// Scroll position.
		private Point _scroll = Point.Empty;	// Scroll position.
		private bool _oldScroll = false;		// Old scrolling flag.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to show scroll bars.
		/// </summary>
		[System.ComponentModel.Browsable(true)]
		public bool ShowScrollBars
		{
			get
			{
				return HScroll;
			}
			set
			{
				HScroll = value;
				VScroll = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Scroll event of the BetterPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ScrollEventArgs"/> instance containing the event data.</param>
		private void BetterPanel_Scroll(object sender, ScrollEventArgs e)
		{
			_location = DisplayRectangle.Location;
		}

		/// <summary>
		/// Calculates the scroll offset to the specified child control.
		/// </summary>
		/// <param name="activeControl">The child control to scroll into view.</param>
		/// <returns>
		/// The upper-left hand <see cref="T:System.Drawing.Point"/> of the display area relative to the client area required to scroll the control into view.
		/// </returns>
		protected override System.Drawing.Point ScrollToControl(Control activeControl)
		{
			if (!_oldScroll)
				return _location;
			else
				return base.ScrollToControl(activeControl);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"/> and its child controls and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!_disposed)
			{
				if (disposing)
					Scroll -= new ScrollEventHandler(BetterPanel_Scroll);
				_disposed = true;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
		}

		/// <summary>
		/// Function to find and scroll to the specified control.
		/// </summary>
		/// <param name="control">Control to scroll to.</param>
		public void FindAndScrollControl(Control control)
		{
			if (control == null)
				throw new ArgumentNullException("control");

			Scroll -= new ScrollEventHandler(BetterPanel_Scroll);
			try
			{
				_oldScroll = true;
				this.ScrollControlIntoView(control);
			}
			finally
			{
				_oldScroll = false;
			}
			Scroll += new ScrollEventHandler(BetterPanel_Scroll);
		}

		/// <summary>
		/// Function to set the scrolling offset.
		/// </summary>
		/// <param name="offset">Offset to scroll.</param>
		public void ScrollControls(Point offset)
		{
			foreach (Control control in Controls)
			{
				control.Location = new Point(control.Location.X + _scroll.X, control.Location.Y + _scroll.Y);
				control.Location = new Point(control.Location.X - offset.X, control.Location.Y - offset.Y);
			}

			_scroll = offset;
		}

		/// <summary>
		/// Function to reset the scroll.
		/// </summary>
		public void ResetScroll()
		{
			foreach (Control control in Controls)
				control.Location = new Point(control.Location.X + _scroll.X, control.Location.Y + _scroll.Y);
			_scroll = Point.Empty;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="BetterPanel"/> class.
		/// </summary>
		public BetterPanel()
		{			
			Scroll += new ScrollEventHandler(BetterPanel_Scroll);
		}
		#endregion
	}
}
