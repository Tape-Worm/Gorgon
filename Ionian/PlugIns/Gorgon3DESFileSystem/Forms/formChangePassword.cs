#region MIT.
// 
// Gorgon_x64.
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
// Created: Wednesday, April 02, 2008 7:49:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.FileSystems.Design;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Interface to allow the user to change the password.
    /// </summary>
    public partial class formChangePassword 
        : GorgonLibrary.FileSystems.Design.formAuthentication
    {
        #region Variables.
        private PasswordData _password = null;          // Password structure.
        private string _oldPass = string.Empty;         // Old password.
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                _password.SetPassword(textNewPassword.Text);
                AuthorizationData = _password as IAuthData;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The password is not valid.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the textOldPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void textOldPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateForm();
        }

        /// <summary>
        /// Function to validate the form.
        /// </summary>
        private void ValidateForm()
        {
            if (AuthorizationData == null)
                textOldPassword.Enabled = false;

            buttonOK.Enabled = false;
            if ((textOldPassword.Text == _oldPass) || (!textOldPassword.Enabled))
            {
                if (textNewPassword.Text == textRetype.Text)
                    buttonOK.Enabled = true;
            }            
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {            
            base.OnLoad(e);

            ValidateForm();

            if (AuthorizationData != null)
                _oldPass = Encoding.UTF8.GetString(AuthorizationData.Data);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="formChangePassword"/> class.
        /// </summary>
        public formChangePassword()
        {
            InitializeComponent();
            _password = new PasswordData();
        }
        #endregion
    }
}
