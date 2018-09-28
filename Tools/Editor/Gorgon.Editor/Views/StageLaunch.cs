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
// Created: August 26, 2018 9:47:52 PM
// 
#endregion

using System;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The first stage screen shown when the application launches.
    /// </summary>
    internal partial class StageLaunch 
        : EditorBaseControl
    {
        #region Variables.

        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the open project button is clicked.
        /// </summary>
        public event EventHandler OpenClicked;
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the ButtonOpenProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonOpenProject_Click(object sender, EventArgs e)
        {
            EventHandler handler = OpenClicked;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the CheckedButtonChanged event of the ButtonGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonGroup_CheckedButtonChanged(object sender, EventArgs e)
        {
            if (CheckNewProject.Checked)
            {
                StageRecent.Visible = false;
                StageNewProject.Visible = true;
                StageNewProject.BringToFront();
            } else if (CheckRecent.Checked)
            {
                StageNewProject.Visible = false;
                StageRecent.Visible = true;
                StageRecent.BringToFront();
            }            
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="StageLaunch"/> class.
        /// </summary>
        public StageLaunch() => InitializeComponent();
        #endregion
    }
}
