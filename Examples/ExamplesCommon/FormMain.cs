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
// Created: August 23, 2018 4:45:05 PM
// 
#endregion

using System;
using System.Threading;
using System.Windows.Forms;

namespace Gorgon.Examples
{
    /// <summary>
    /// The main form for the example application.
    /// </summary>
    public partial class FormMain : Form
    {
        #region Properties.
        /// <summary>
        /// Property to return the synchronization context for this window.
        /// </summary>
        public SynchronizationContext CurrentSyncContext
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the please wait label is visible.
        /// </summary>
        public bool IsLoaded
        {
            get => !LabelPleaseWait.Visible;
            set
            {
                if (InvokeRequired)
                {
                    CurrentSyncContext.Post(arg => IsLoaded = value, null);
                    return;
                }
                LabelPleaseWait.Visible = !value;
            }
        }
        #endregion

        #region Methods.
        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.</summary>
        /// <param name="e">An <see cref="System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CenterToScreen();
        }

        /// <summary>
        /// Function to update the text of the status label.
        /// </summary>
        /// <param name="newText">The new text to apply.</param>
        public void UpdateStatus(string newText)
        {
            if (InvokeRequired)
            {
                CurrentSyncContext.Post(arg => UpdateStatus(arg?.ToString()), newText);
                return;
            }
            LabelPleaseWait.Text = string.IsNullOrWhiteSpace(newText) ? "Example is loading, please wait..." : newText;

            LabelPleaseWait.Refresh();
            Application.DoEvents();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormMain"/> class.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            CurrentSyncContext = SynchronizationContext.Current;
        }
        #endregion
    }
}
