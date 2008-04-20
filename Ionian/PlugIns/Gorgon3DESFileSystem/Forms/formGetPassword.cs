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
// Created: Tuesday, April 01, 2008 9:40:02 PM
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
    /// Interface used to retrieve the password for the 3DES encryption.
    /// </summary>
    public partial class formGetPassword 
        : GorgonLibrary.FileSystems.Design.formAuthentication
    {
        #region Variables.
        private PasswordData _password;         // Password data.
        #endregion

        #region Properties.
        /// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                _password.SetPassword(textPassword.Text);
                AuthorizationData = _password as IAuthData;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The password is not valid.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the TextChanged event of the textPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void textPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateForm();
        }

        /// <summary>
        /// Function to validate the controls on the form.
        /// </summary>
        private void ValidateForm()
        {
            if (string.IsNullOrEmpty(textPassword.Text))
                buttonOK.Enabled = false;
            else
                buttonOK.Enabled = true;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="formGetPassword"/> class.
        /// </summary>
        public formGetPassword()
        {
            InitializeComponent();
            _password = new PasswordData();            
        }
        #endregion
    }
}
