#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Wednesday, November 15, 2006 3:08:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Form for resizing the borders.
	/// </summary>
	public partial class BorderSizer : Form
	{
		#region Events
		/// <summary>
		/// Event fired when the borders are updated.
		/// </summary>
		public event EventHandler BorderUpdated;
		#endregion

		#region Variables.
		private GorgonFont _font = null;		// Font we're using.
		private bool _noEvents = false;			// Flag to indicate that we should not fire events.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the font we're working with.
		/// </summary>
		internal GorgonFont GorgonFont
		{
			get
			{
				return _font;
			}
			set
			{
				_noEvents = true;
				if ((value != null) && (_font != value))
				{
					_font = value;
					_font.BorderUpdated += new EventHandler(_font_BorderUpdated);
				}

				_font_BorderUpdated(this, EventArgs.Empty);
				_noEvents = false;
			}
		}

		/// <summary>
		/// Handles the BorderUpdated event of the _font control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _font_BorderUpdated(object sender, EventArgs e)
		{
			_noEvents = true;
			numericLeft.Value = _font.LeftBorderOffset;
			numericTop.Value = _font.TopBorderOffset;
			numericRight.Value = _font.RightBorderOffset;
			numericBottom.Value = _font.BottomBorderOffset;
			_noEvents = false;
		}

		/// <summary>
		/// Property to set or return the left border.
		/// </summary>
		public int LeftBorder
		{
			get
			{
				return (int)numericLeft.Value;
			}
			set
			{
				_noEvents = true;
				numericLeft.Value = value;
				_noEvents = false;
			}
		}

		/// <summary>
		/// Property to set or return the right border.
		/// </summary>
		public int RightBorder
		{
			get
			{
				return (int)numericRight.Value;
			}
			set
			{
				_noEvents = true;
				numericRight.Value = value;
				_noEvents = false;
			}
		}

		/// <summary>
		/// Property to set or return the top border.
		/// </summary>
		public int TopBorder
		{
			get
			{
				return (int)numericTop.Value;
			}
			set
			{
				_noEvents = true;
				numericTop.Value = value;
				_noEvents = false;
			}
		}

		/// <summary>
		/// Property to set or return the bottom border.
		/// </summary>
		public int BottomBorder
		{
			get
			{
				return (int)numericBottom.Value;
			}
			set
			{
				_noEvents = true;
				numericBottom.Value = value;
				_noEvents = false;
			}
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public BorderSizer()
		{
			InitializeComponent();
		}
		#endregion
		
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			Hide();
			e.Cancel = true;
		}

		/// <summary>
		/// Handles the ValueChanged event of the numeric controls.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void numeric_ValueChanged(object sender, EventArgs e)
		{
			if (_noEvents)
				return;

			Cursor = Cursors.WaitCursor;
			_font.SetBorderOffsets((int)numericLeft.Value, (int)numericTop.Value, (int)numericRight.Value, (int)numericBottom.Value);
			if (BorderUpdated != null)
				BorderUpdated(this, EventArgs.Empty);
			Cursor = Cursors.Default;
		}
	}
}