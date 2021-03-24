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
// Created: December 5, 2018 9:37:55 PM
// 
#endregion

using System;
using System.ComponentModel;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The main staging view for accessing file operations, settings, etc...
    /// </summary>
    internal partial class Stage
        : EditorBaseControl
    {
        #region Events.
        /// <summary>
        /// Event triggered when the back button is clicked.
        /// </summary>
        public event EventHandler BackClicked;
        /// <summary>
        /// Event triggered when the browse button is clicked.
        /// </summary>
        public event EventHandler BrowseClicked;
        /// <summary>
        /// Event triggered when the open button is clicked.
        /// </summary>
        public event EventHandler OpenPackFileClicked;
        /// <summary>
        /// Event triggered when the Save As button is clicked, or a new project is saved for the first time.
        /// </summary>
        public event EventHandler<SaveEventArgs> SaveClicked;
        #endregion

        #region Variables.
        // Flag to indicate that we're in start up mode.
        private bool _isStartup = true;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the stage should be shown in start up configuration or not.
        /// </summary>
        [Browsable(false)]
        public bool IsStartup
        {
            get => _isStartup;
            set
            {
                SuspendLayout();
                ButtonSaveAs.Visible = PanelBack.Visible = !value;
                _isStartup = value;
                ResumeLayout(true);
            }
        }

        /// <summary>
        /// Property to set or return whether the application can save a packed file as at this time.
        /// </summary>
        [Browsable(false)]
        public bool CanSavePackedFile
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
            get => ButtonOpenPackFile.Enabled;
            set => ButtonOpenPackFile.Enabled = value;
        }
        #endregion

        #region Methods.
        /// <summary>Function to update the current view state.</summary>
        private void SetViewState()
        {
            NewProject.Visible = CheckNew.Checked;
            Recent.Visible = CheckRecent.Checked;
            SettingsPanel.Visible = CheckSettings.Checked;
        }

        /// <summary>Handles the Click event of the CheckNew control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckNew_Click(object sender, EventArgs e) => SetViewState();

        /// <summary>Handles the Click event of the CheckRecent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckRecent_Click(object sender, EventArgs e) => SetViewState();

        /// <summary>Handles the Click event of the ButtonSettings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckSettings_Click(object sender, EventArgs e) => SetViewState();

        /// <summary>Handles the Click event of the ButtonOpenPackFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonOpenPackFile_Click(object sender, EventArgs e)
        {
            EventHandler handler = OpenPackFileClicked;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the Click event of the ButtonSaveAs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSaveAs_Click(object sender, EventArgs e)
        {
            EventHandler<SaveEventArgs> handler = SaveClicked;
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
            EventHandler handler = BrowseClicked;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // If we have recent items, then start with the recent items list instead of the new project item.
            if (Recent.HasItems)
            {
                CheckRecent.PerformClick();
            }
            else
            {
                CheckNew.PerformClick();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Stage"/> class.</summary>
        public Stage() => InitializeComponent();
        #endregion
    }
}
