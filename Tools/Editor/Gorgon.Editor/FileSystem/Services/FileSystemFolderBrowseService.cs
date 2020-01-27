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
// Created: April 28, 2019 11:08:29 AM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.ViewModels;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// An interface used to browse the file system folder structure.
    /// </summary>
    internal class FileSystemFolderBrowseService
        : IFileSystemFolderBrowseService
    {
        #region Properties.
        /// <summary>Property to set or return the file system root for the currently loaded project.</summary>
        public IFileExplorer FileSystem
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the parent form for the message box.
        /// </summary>
        /// <returns>The form to use as the owner.</returns>
        private static Form GetParentForm() => Form.ActiveForm ?? (Application.OpenForms.Count > 1 ? Application.OpenForms[Application.OpenForms.Count - 1] : GorgonApplication.MainForm);

        /// <summary>Function to retrieve a path from the file system.</summary>
        /// <param name="initialPath">The starting path to select.</param>
        /// <param name="caption">The caption for the dialog.</param>
        /// <param name="description">The description of what the browser is supposed to be doing.</param>
        /// <returns>The selected path, or <b>null</b> if canceled.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="initialPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="initialPath"/> parameter is emtpy.</exception>
        /// <exception cref="IOException">Thrown if no <see cref="FileSystemRoot"/> is set.</exception>
        public string GetFolderPath(string initialPath, string caption, string description)
        {
            if (initialPath == null)
            {
                throw new ArgumentNullException(nameof(initialPath));
            }

            if (string.IsNullOrWhiteSpace(initialPath))
            {
                throw new ArgumentEmptyException(nameof(initialPath));
            }

            if (FileSystem == null)
            {
                throw new IOException(Resources.GOREDIT_ERR_NO_ROOT);
            }

            IDirectory initialDirectory = initialPath == "/" ? FileSystem.Root 
                                                             : FileSystem.Root.Directories.Traverse(d => d.Directories)
                                                                    .FirstOrDefault(d => string.Equals(d.FullPath, initialPath, StringComparison.OrdinalIgnoreCase));

            if (initialDirectory == null)
            {
                initialDirectory = FileSystem.Root;
            }

            using (var browser = new FormFileSystemFolderBrowser()
            {
                Text = caption,
                Description = description
            })
            {

                browser.SetDataContext(FileSystem);
                browser.SetInitialPath(initialDirectory);
                return browser.ShowDialog(GetParentForm()) != DialogResult.OK ? null : browser.CurrentDirectory.FormatDirectory('/');
            }
        }
        #endregion
    }
}
