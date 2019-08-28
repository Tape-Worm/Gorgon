#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 4, 2019 7:51:22 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.UI.Controls
{
    /// <summary>
    /// A base control for editor sub panels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Content plug in developers that require a sub panel to display properties, settings, etc... must inherit this control type on their <see cref="UserControl"/>.
    /// </para>
    /// </remarks>
    public partial class EditorSubPanelCommon
        : EditorBaseControl
    {
        #region Variables.
        /// <summary>
        /// The panel that will hold the body of the sub panel.
        /// </summary>
        protected Panel PanelBody;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the OK button is clicked.
        /// </summary>
        [Category("Behavior"), Description("Triggered when the OK button is clicked.")]
        public event EventHandler Submit;

        /// <summary>
        /// Event triggered when the Cancel button is clicked.
        /// </summary>
        [Category("Behavior"), Description("Triggered when the Cancel button is clicked.")]
        public event EventHandler Cancel;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the text to display in the caption.
        /// </summary>
        [Browsable(true), Category("Appearance"), Description("Sets the text to display in the panel caption"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string Text
        {
            get => base.Text;
            set => base.Text = LabelCaption.Text = value ?? string.Empty;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the Click event of the ButtonOK control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonOK_Click(object sender, EventArgs e) => OnSubmit();

        /// <summary>Handles the Click event of the ButtonCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCancel_Click(object sender, EventArgs e) => OnCancel();

        /// <summary>
        /// Function to submit the change.
        /// </summary>
        protected virtual void OnSubmit()
        {
            EventHandler handler = Submit;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to cancel the change.
        /// </summary>
        protected virtual void OnCancel()
        {
            EventHandler handler = Cancel;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function called to validate the OK button.
        /// </summary>
        /// <returns><b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
        protected virtual bool OnValidateOk() => true;

        /// <summary>
        /// Function to validate the state of the OK button.
        /// </summary>
        protected void ValidateOk() => ButtonOK.Enabled = OnValidateOk();

        /// <summary>Raises the <see cref="UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            ValidateOk();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="EditorSubPanelCommon"/> class.</summary>
        public EditorSubPanelCommon() => InitializeComponent();
        #endregion

    }
}
