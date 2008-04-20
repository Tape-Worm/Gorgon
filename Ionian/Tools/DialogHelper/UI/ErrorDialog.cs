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
// Created: Saturday, April 19, 2008 12:19:40 PM
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
	/// Dialog for error messages.
	/// </summary>
	public partial class ErrorDialog 
        : BaseDialog
	{
		#region Variables.
        private int _lastWidth;                 // Last used width.
		private string _errorDetails;			// Error details.
		private ErrorDialogIcons _errorIcon;	// Icon used for errors.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return or set the icon for the error dialog.
		/// </summary>
		public ErrorDialogIcons ErrorIcon
		{
			get
			{
				return _errorIcon;
			}
			set
			{
				_errorIcon = value;
				ValidateFunctions();
			}
		}

		/// <summary>
		/// Property to return or set the details of the error.
		/// </summary>
		public string ErrorDetails
		{
			get
			{
				return _errorDetails;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					value = string.Empty;

				_errorDetails = value;

				// Fix up line endings.				
				errorDetails.Text = value.Replace("\n", Environment.NewLine);
				
				ValidateFunctions();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the detailsButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void detailsButton_Click(object sender, EventArgs e)
		{
			if (detailsButton.Checked)
			{
				_lastWidth = Width;
				Height += 202;
				Width = 405;
				errorDetails.Visible = true;
			}
			else
			{
				errorDetails.Visible = false;
				Width = _lastWidth;
				Height -= 202;
			}

			this.Refresh();
		}

		/// <summary>
		/// OK button click event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
		
		/// <summary>
        /// Function to validate the buttons.
        /// </summary>
        protected override void ValidateFunctions()
        {
			if (_errorDetails == string.Empty)
				detailsButton.Enabled = false;
			else
				detailsButton.Enabled = true;


			// Turn off all images.
            iconBug.Visible = false;
            iconData.Visible = false;
            iconDisk.Visible = false;
            iconHardware.Visible = false;
            iconRound.Visible = false;
            imageIcon.Visible = false;

			// Show the proper icon.
			switch (_errorIcon)
			{
				case ErrorDialogIcons.Default:
                    iconRound.Visible = true;
					break;
				case ErrorDialogIcons.Box:
                    imageIcon.Visible = true;
					break;
				case ErrorDialogIcons.Bug:
                    iconBug.Visible = true;
					break;
				case ErrorDialogIcons.Disk:
                    iconDisk.Visible = true;
					break;
				case ErrorDialogIcons.Data:
                    iconData.Visible = true;
					break;
				case ErrorDialogIcons.Hardware:
                    iconHardware.Visible = true;
					break;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            MessageHeight = 256;
            _lastWidth = Width;
			OKButton.Focus();
			ValidateFunctions();
		}

		/// <summary>
		/// Function to perform the actual drawing of the message.
		/// </summary>
		/// <param name="g">Graphics object to use.</param>
        protected override void DrawDialog(System.Drawing.Graphics g)
		{
			float maxTextHeight;                // Maximum text height.

			// Get size.
			maxTextHeight = AdjustSize(g,0);

			// Relocate buttons.
			OKButton.Left = ClientSize.Width - OKButton.Width - 8;

			// Adjust for detail window.
			if (detailsButton.Checked)
			{
				detailsButton.Top = errorDetails.Top - 6 - detailsButton.Height;
				OKButton.Top = errorDetails.Top - 6 - OKButton.Height;
			}
			else
			{
				detailsButton.Top = ClientSize.Height - 6 - detailsButton.Height;
				OKButton.Top = ClientSize.Height - 6 - OKButton.Height;
			}

			// Adjust the position of the details box.
			errorDetails.Top = ClientSize.Height - 8 - errorDetails.Height;

			DrawMessage(g, maxTextHeight);			
		}
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public ErrorDialog()
		{
			// Initial height = 134.
			// Expanded height = 334.
			InitializeComponent();
			_errorDetails = "";
			_errorIcon = ErrorDialogIcons.Default;
		}
		#endregion
	}
}