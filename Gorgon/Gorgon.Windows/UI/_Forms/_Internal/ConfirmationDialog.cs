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
// Created: Saturday, June 18, 2011 4:20:43 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.UI
{
    /// <summary>
    /// Dialog for confirmation.
    /// </summary>
    internal partial class ConfirmationDialog
        : BaseDialog
    {
        #region Variables.
        // Don't show the cancel button.
        private bool _showCancel;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to show the cancel button.
        /// </summary>
        [Browsable(false)]
        public bool ShowCancel
        {
            get => _showCancel;
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
            get;
            protected set;
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
            if (ConfirmationResult != ConfirmationResult.None)
            {
                return;
            }

            ConfirmationResult = _showCancel ? ConfirmationResult.Cancel : ConfirmationResult.No;
        }

        /// <summary>
        /// Handles the Click event of the OKButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OKButton_Click(object sender, EventArgs e)
        {
            ConfirmationResult = ConfirmationResult.Yes;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the buttonCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            ConfirmationResult = ConfirmationResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the buttonNo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNo_Click(object sender, EventArgs e)
        {
            ConfirmationResult = ConfirmationResult.No;
            Close();
        }

        /// <summary>
        /// Function to perform the actual drawing of the dialog.
        /// </summary>
        /// <param name="g">Graphics object to use.</param>
        protected override void DrawDialog(System.Drawing.Graphics g)
        {
            // Get size.
            float maxTextHeight = AdjustSize(g, 0);

            // Adjust buttons.
            if (_showCancel)
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

            buttonNo.Top = buttonCancel.Top = buttonOK.Top = ClientSize.Height - buttonOK.Height - 8;

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
            {
                buttonCancel.Focus();
            }
            else
            {
                buttonNo.Focus();
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfirmationDialog()
        {
            ConfirmationResult = ConfirmationResult.None;
            InitializeComponent();
        }
        #endregion
    }
}

