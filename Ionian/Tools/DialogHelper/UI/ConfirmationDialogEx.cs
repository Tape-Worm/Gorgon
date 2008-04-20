#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 12:19:09 PM
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
	/// Dialog for confirmation + plus to all option.
	/// </summary>
	public partial class ConfirmationDialogEx 
        : ConfirmationDialog
	{
		#region Variables.

		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Apply to all to result.
			if ((checkToAll.Checked) && (_result != ConfirmationResult.Cancel))
				_result |= ConfirmationResult.ToAll;
		}

		/// <summary>
		/// Form load event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnLoad(EventArgs e)
		{			
			base.OnLoad(e);

			MessageHeight = 150;
		}

		/// <summary>
		/// Function to perform the actual drawing of the dialog.
		/// </summary>
		/// <param name="g">Graphics object to use.</param>
		protected override void DrawDialog(System.Drawing.Graphics g)
		{
			float maxTextHeight;                // Maximum text height.

			// Get size.
			maxTextHeight = AdjustSize(g, checkToAll.Height);

			// Adjust buttons.
			if (ShowCancel)
			{
				buttonCancel.Left = ClientSize.Width - buttonCancel.Width - 8;
				buttonNo.Left = buttonCancel.Left - buttonNo.Width - 8;
				OKButton.Left = buttonNo.Left - OKButton.Width - 8;
			}
			else
			{
				buttonNo.Left = ClientSize.Width - buttonNo.Width - 8;
				OKButton.Left = buttonNo.Left - OKButton.Width - 8;
			}

			buttonNo.Top = buttonCancel.Top = OKButton.Top = checkToAll.Top - OKButton.Height - 4;
			checkToAll.Top = ClientSize.Height - 4 - checkToAll.Height;

			DrawMessage(g, maxTextHeight);			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public ConfirmationDialogEx()
		{
			InitializeComponent();
		}
		#endregion
	}
}

