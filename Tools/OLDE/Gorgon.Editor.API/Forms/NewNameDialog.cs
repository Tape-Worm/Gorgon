#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, March 17, 2013 4:46:11 PM
// 
#endregion

using System;
using Gorgon.Editor.Properties;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// Form to allow entry of a new name for an object.
	/// </summary>
	public partial class NewNameDialog 
		: FlatForm
	{
		#region Methods.
		/// <summary>
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			buttonOK.Enabled = !string.IsNullOrWhiteSpace(textName.Text);
		}

        /// <summary>
        /// Function to localize the form.
        /// </summary>
	    private void LocalizeControls()
        {
            Text = APIResources.GOREDIT_TEXT_NEW_NAME;
            labelName.Text = string.Format("{0}:", APIResources.GOREDIT_TEXT_NAME);
            buttonOK.Text = APIResources.GOREDIT_ACC_TEXT_OK;
            buttonCancel.Text = APIResources.GOREDIT_ACC_TEXT_CANCEL;
        }

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            LocalizeControls();
			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="NewNameDialog"/> class.
		/// </summary>
		public NewNameDialog()
		{
			InitializeComponent();
		}
		#endregion
	}
}
