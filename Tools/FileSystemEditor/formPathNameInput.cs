#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Monday, April 02, 2007 12:24:16 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Dialogs;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form used to edit and create path/file names.
	/// </summary>
	public partial class formPathNameInput 
		: Form
	{
		#region Variables.
		private string _invalidChars = string.Empty;								// Invalid character list.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the path name.
		/// </summary>
		public string PathName
		{
			get
			{
				return textName.Text;
			}
			set
			{
				textName.Text = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyPress event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyPressEventArgs"/> instance containing the event data.</param>
		private void textName_KeyPress(object sender, KeyPressEventArgs e)
		{
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				if ((invalidChar == e.KeyChar) && (e.KeyChar != '\b') && (e.KeyChar != '\r') && (e.KeyChar != 27))
				{
					UI.WarningBox(this, e.KeyChar.ToString() + " is an invalid character.\nThe following characters are not allowed:\n" + _invalidChars);
					e.Handled = true;
				}
			}

			ValidateForm();

			// Cancel on ESC.
			if (e.KeyChar == 27)
				DialogResult = DialogResult.Cancel;

			// Enter will accept the input.
			if ((e.KeyChar == '\r') && (buttonOK.Enabled))
				DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Function to validate the form & controls.
		/// </summary>
		private void ValidateForm()
		{
			buttonOK.Enabled = false;
			if (textName.Text != string.Empty)
				buttonOK.Enabled = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formPathNameInput()
		{
			InitializeComponent();

			// Get list of invalid characters.
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				if (Convert.ToInt32(invalidChar) > 32)
				{
					if (_invalidChars != string.Empty)
						_invalidChars += ", ";
					_invalidChars += invalidChar.ToString();
				}
			}
		}
		#endregion
	}
}