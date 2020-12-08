#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: February 1, 2020 10:56:27 AM
// 
#endregion

using System;
using System.Threading;
using System.Windows.Forms;

namespace Gorgon.UI
{
    /// <summary>
    /// A form for displaying a progress meter panel on an application.
    /// </summary>
    internal partial class FormProgress : Form
    {
        // Flag to indicate that the form is in the middle of a refresh.
        private int _isRefreshing;

        /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }

            base.OnFormClosing(e);            
        }

        /// <summary>Raises the <see cref="Form.Shown"/> event.</summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (Parent == null)
            {
                return;
            }

            CenterToParent();
        }

        /// <summary>Raises the <see cref="Form.Activated"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            CenterToParent();
        }

        /// <summary>Handles the Resize event of the Progress control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Progress_Resize(object sender, EventArgs e)
        {
            if (!IsHandleCreated)
            {
                return;
            }
            CenterToParent();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.KeyUp"/> event.</summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Escape)
            {
                Progress.Cancel();
            }
        }

        /// <summary>Raises the <see cref="Form.Load"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CenterToParent();
            Focus();
            Progress.Focus();
        }        

        /// <summary>Forces the control to invalidate its client area and immediately redraw itself and any child controls.</summary>
        public override void Refresh()
        {
            base.Refresh();

            if (Interlocked.Exchange(ref _isRefreshing, 1) == 1)
            {
                return;
            }

            try
            {
                if ((!IsDisposed) && (!Disposing))
                {
                    CenterToParent();
                    Focus();
                    Progress.Focus();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isRefreshing, 0);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="FormProgress"/> class.</summary>
        public FormProgress() => InitializeComponent();
    }
}
