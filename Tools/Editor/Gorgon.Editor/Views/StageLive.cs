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
        #endregion

        #region Variables.

        #endregion

        #region Properties.

        #endregion

        #region Methods.
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
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="StageLive"/> class.
        /// </summary>
        public StageLive() => InitializeComponent();
        #endregion
    }
}
