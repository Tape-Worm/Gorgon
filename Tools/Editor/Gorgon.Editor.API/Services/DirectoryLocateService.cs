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
// Created: October 1, 2018 7:48:44 PM
// 
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.UI;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to locate a directory on the physical file system.
    /// </summary>
    public class DirectoryLocateService
        : IDirectoryLocateService
    {
        /// <summary>
        /// Function to retrieve the parent form for the message box.
        /// </summary>
        /// <returns>The form to use as the owner.</returns>
        private static Form GetParentForm() => Form.ActiveForm ?? (Application.OpenForms.Count > 1 ? Application.OpenForms[Application.OpenForms.Count - 1] : GorgonApplication.MainForm);

        /// <summary>
        /// Function to show an interface that allows directory selection.
        /// </summary>
        /// <param name="initialDir">The initial directory to use.</param>
        /// <param name="caption">[Optional] The caption for the dialog.</param>
        /// <param name="onSelected">[Optional] The method to call when a directory is selected.</param>
        /// <param name="onEntered">[Optional] The method to call when a directory is entered.</param>
        /// <returns>The selected directory, or <b>null</b> if cancelled.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="initialDir"/> parameter is <b>null</b>.</exception>
        public DirectoryInfo GetDirectory(DirectoryInfo initialDir, string caption = null, Action<FolderSelectedArgs> onSelected = null, Action<FolderSelectedArgs> onEntered = null)
        {
            if (initialDir == null)
            {
                throw new ArgumentNullException(nameof(initialDir));
            }

            if (!initialDir.Exists)
            {
                initialDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }

            void OnSelected(object sender, FolderSelectedArgs e)
            {
                onSelected(e);
            }

            void OnEntered(object sender, FolderSelectedArgs e)
            {
                onEntered(e);
            }

            Form parent = GetParentForm();
            FormDirectoryLocator locatorUI = null;

            try
            {
                locatorUI = new FormDirectoryLocator
                {
                    Text = string.IsNullOrWhiteSpace(caption) ? Resources.GOREDIT_TITLE_DEFAULT_DIR_LOCATOR : caption
                };

                locatorUI.CurrentDirectory = initialDir.FullName;

                if (onSelected != null)
                {
                    locatorUI.FolderSelected += OnSelected;
                }

                if (onEntered != null)
                {
                    locatorUI.FolderEntered += OnEntered;
                }

                return locatorUI.ShowDialog(parent) == DialogResult.Cancel ? null : new DirectoryInfo(locatorUI.CurrentDirectory);
            }
            finally
            {
                locatorUI.FolderEntered -= OnEntered;
                locatorUI.FolderSelected -= OnSelected;
                locatorUI?.Dispose();
            }
        }
    }
}
