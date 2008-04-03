#region LGPL.
// 
// Gorgon_x64.
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
// Created: Wednesday, April 02, 2008 7:49:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities.Utility;
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
                UI.ErrorBox(this, "The password is not valid.\n" + ex.Message);
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
