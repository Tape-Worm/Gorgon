#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: September 28, 2018 5:07:27 PM
// 
#endregion

using System;
using System.ComponentModel;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// A staging view used after the app is launched.
    /// </summary>
    internal partial class StageLive 
        : EditorBaseControl
    {
        #region Events.
        /// <summary>
        /// Event triggered when the back button is clicked.
        /// </summary>
        public event EventHandler BackClicked;
        /// <summary>
        /// Event triggered when the open project button is clicked.
        /// </summary>
        public event EventHandler OpenClicked;
        /// <summary>
        /// Event triggered when the Save As button is clicked, or a new project is saved for the first time.
        /// </summary>
        public event EventHandler<SaveEventArgs> Save;        
        #endregion

        #region Variables.

        #endregion
                
        #region Properties.
        /// <summary>
        /// Property to set or return whether the application can save at this time.
        /// </summary>
        [Browsable(false)]
        public bool CanSave
        {
            get => ButtonSave.Enabled;
            set => ButtonSave.Enabled = value;
        }

        /// <summary>
        /// Property to set or return whether the application can save as at this time.
        /// </summary>
        [Browsable(false)]
        public bool CanSaveAs
        {
            get => ButtonSaveAs.Enabled;
            set => ButtonSaveAs.Enabled = value;
        }

        /// <summary>
        /// Property to set or return whether files can be opened or not.
        /// </summary>
        [Browsable(false)]
        public bool CanOpen
        {
            get => ButtonOpenProject.Enabled;
            set => ButtonOpenProject.Enabled = value;
        }
        #endregion

        #region Methods.

        /// <summary>
        /// Handles the Click event of the ButtonSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            EventHandler<SaveEventArgs> handler = Save;
            handler?.Invoke(this, new SaveEventArgs(false));
        }

        /// <summary>
        /// Handles the Click event of the ButtonSaveAs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSaveAs_Click(object sender, EventArgs e)
        {
            EventHandler<SaveEventArgs> handler = Save;
            handler?.Invoke(this, new SaveEventArgs(true));
        }

        /// <summary>
        /// Handles the Click event of the ButtonBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonBack_Click(object sender, EventArgs e)
        {
            EventHandler handler = BackClicked;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the Click event of the ButtonOpenProject control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ButtonOpenProject_Click(object sender, EventArgs e)
        {
            EventHandler handler = OpenClicked;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the CheckedButtonChanged event of the ButtonGroup control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ButtonGroup_CheckedButtonChanged(object sender, EventArgs e)
        {
            if (CheckNewProject.Checked)
            {
                StageRecent.Visible = false;
                StageNewProject.Visible = true;
                StageNewProject.BringToFront();
            }
            else if (CheckRecent.Checked)
            {
                StageNewProject.Visible = false;
                StageRecent.Visible = true;
                StageRecent.BringToFront();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="StageLive"/> class.
        /// </summary>
        public StageLive() => InitializeComponent();
        #endregion
    }
}
