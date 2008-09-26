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
// Created: Saturday, April 19, 2008 12:18:37 PM
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
	/// Dialog for confirmation.
	/// </summary>
	public partial class ConfirmationDialog 
		: BaseDialog
	{		
		#region Variables.
		private bool _showCancel = false;								// Don't show the cancel button.

		/// <summary>
		/// Result of the dialog.
		/// </summary>
		protected ConfirmationResult _result = ConfirmationResult.None;	
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to show the cancel button.
		/// </summary>
		[Browsable(false)]
		public bool ShowCancel
		{
			get
			{
				return _showCancel;
			}
			set
			{
				_showCancel = value;

				if (!_showCancel)
				{
					buttonNo.Left = 150;
					buttonCancel.Visible = false;
				}
				else
				{
					buttonNo.Left = 77;
					buttonCancel.Visible = true;
				}
			}
		}

		/// <summary>
		/// Property to return the confirmation dialog result.
		/// </summary>
		public ConfirmationResult ConfirmationResult
		{
			get
			{
				return _result;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{			
			base.OnFormClosing(e);

			// Assume cancel.
			if (_result == ConfirmationResult.None)
			{
				if (_showCancel)
					_result = ConfirmationResult.Cancel;
				else
					_result = ConfirmationResult.No;
			}
		}

		/// <summary>
		/// Handles the Click event of the OKButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void OKButton_Click(object sender, EventArgs e)
		{
			_result = ConfirmationResult.Yes;
			Close();
		}

		/// <summary>
		/// Handles the Click event of the buttonCancel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonCancel_Click(object sender, EventArgs e)
		{
			_result = ConfirmationResult.Cancel;
			Close();
		}

		/// <summary>
		/// Handles the Click event of the buttonNo control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonNo_Click(object sender, EventArgs e)
		{
			_result = ConfirmationResult.No;
			Close();
		}
				
		/// <summary>
		/// Function to perform the actual drawing of the dialog.
		/// </summary>
		/// <param name="g">Graphics object to use.</param>
        protected override void DrawDialog(System.Drawing.Graphics g)
		{
			float maxTextHeight;                // Maximum text height.

			// Get size.
			maxTextHeight = AdjustSize(g,0);

			// Adjust buttons.
			if (_showCancel)
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

			buttonNo.Top = buttonCancel.Top = OKButton.Top = ClientSize.Height - OKButton.Height - 8;

			DrawMessage(g, maxTextHeight);			
		}

		/// <summary>
		/// Form load event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// Force focus.
			if (_showCancel)
				buttonCancel.Focus();
			else
				buttonNo.Focus();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public ConfirmationDialog()
		{
			InitializeComponent();
		}
		#endregion
	}
}

