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

