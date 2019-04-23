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
// Created: Saturday, June 18, 2011 4:21:02 PM
// 
#endregion

using System;
using System.Windows.Forms;

namespace Gorgon.UI
{
	/// <summary>
	/// Dialog for confirmation + plus to all option.
	/// </summary>
	internal partial class ConfirmationDialogEx 
		: ConfirmationDialog
	{
		#region Variables.

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
		    if ((checkToAll.Checked) && (ConfirmationResult != ConfirmationResult.Cancel))
		    {
		        ConfirmationResult |= ConfirmationResult.ToAll;
		    }
		}

		/// <summary>
		/// Form load event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnLoad(EventArgs e)
		{
		    MessageHeight -= checkToAll.Height;

			base.OnLoad(e);
		}

		/// <summary>
		/// Function to perform the actual drawing of the dialog.
		/// </summary>
		/// <param name="g">Graphics object to use.</param>
		protected override void DrawDialog(System.Drawing.Graphics g)
		{
		    // Get size.
			float maxTextHeight = AdjustSize(g, checkToAll.Height);

			// Adjust buttons.
			if (ShowCancel)
			{
				buttonCancel.Left = ClientSize.Width - buttonCancel.Width - 8;
				buttonNo.Left = buttonCancel.Left - buttonNo.Width - 8;
				buttonOK.Left = buttonNo.Left - buttonOK.Width - 8;
			}
			else
			{
				buttonNo.Left = ClientSize.Width - buttonNo.Width - 8;
				buttonOK.Left = buttonNo.Left - buttonOK.Width - 8;
			}

			buttonNo.Top = buttonCancel.Top = buttonOK.Top = ClientSize.Height - buttonOK.Height - 4;
            checkToAll.Top = buttonOK.Top + ((buttonOK.Height / 2) - (checkToAll.Height / 2));

            DrawMessage(g, maxTextHeight);			
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfirmationDialogEx() => InitializeComponent();
        #endregion
    }
}

