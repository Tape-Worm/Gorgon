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
// Created: September 25, 2018 1:13:08 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.IO;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.UI;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A locator dialog to find a workspace directory.
    /// </summary>
    internal partial class FormDirectoryLocator 
        : KryptonForm
    {
        #region Events.
        /// <summary>
        /// Event triggered when a folder is selected.
        /// </summary>
        public EventHandler<FolderSelectedArgs> FolderSelected;

        /// <summary>
        /// Event triggered when a folder is entered.
        /// </summary>
        public EventHandler<FolderSelectedArgs> FolderEntered;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the current directory.
        /// </summary>
        [Browsable(false)]
        public DirectoryInfo CurrentDirectory
        {
            get => WorkspaceBrowser.CurrentDirectory;
            set
            {
                WorkspaceBrowser.AssignInitialDirectory(value);
                ButtonOK.Enabled = value != null;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when a folder is entered.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void WorkspaceBrowser_FolderEntered(object sender, FolderSelectedArgs e)
        {
            CurrentDirectory = string.IsNullOrWhiteSpace(e.FolderPath) ? null : new DirectoryInfo(e.FolderPath);

            EventHandler<FolderSelectedArgs> handler = FolderEntered;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Function called when a folder is selected in the browser.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters..</param>
        private void WorkspaceBrowser_FolderSelected(object sender, FolderSelectedArgs e)
        {
            CurrentDirectory = string.IsNullOrWhiteSpace(e.FolderPath) ? null : new DirectoryInfo(e.FolderPath);

            EventHandler<FolderSelectedArgs> handler = FolderSelected;
            handler?.Invoke(this, e);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDirectoryLocator"/> class.
        /// </summary>
        public FormDirectoryLocator() => InitializeComponent();
        #endregion
    }
}
