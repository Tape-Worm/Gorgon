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
// Created: September 24, 2018 12:48:59 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to show a dialog for opening an editor file.
    /// </summary>
    internal class EditorFileOpenDialogService
        : IEditorFileOpenDialogService
    {
        #region Variables.
        // The previously selected file extension filter index.
        private int _lastSelectedFilterIndex;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the initial file path to use.
        /// </summary>
        public string InitialFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return a file filter.
        /// </summary>        
        public string FileFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the initial directory.
        /// </summary>        
        public DirectoryInfo InitialDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the title for the dialog.
        /// </summary>
        /// <value>The dialog title.</value>
        public string DialogTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the available file system providers.
        /// </summary>        
        public IFileSystemProviders Providers
        {
            get;
        }

        /// <summary>
        /// Property to return the settings for the application.
        /// </summary>        
        public EditorSettings Settings
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build a file system reader filter string for file dialogs.
        /// </summary>
        /// <returns>The string containing the file dialog filter.</returns>
        private string GetReaderDialogFilterString()
        {
            var result = new StringBuilder();
            var filter = new StringBuilder();
            var allFilter = new StringBuilder();

            IReadOnlyList<(string, IReadOnlyList<GorgonFileExtension>)> extensions = Providers.GetReaderFileExtensions().ToArray();

            if (extensions.Count == 0)
            {
                return Resources.GOREDIT_TEXT_ALL_FILES;
            }

            foreach ((string desc, IReadOnlyList<GorgonFileExtension> extensions) item in extensions)
            {
                filter.Length = 0;

                if (result.Length > 0)
                {
                    result.Append("|");
                }

                result.Append(item.desc);

                foreach (GorgonFileExtension extension in item.extensions)
                {
                    if (allFilter.Length > 0)
                    {
                        allFilter.Append(";");
                    }

                    if (filter.Length > 0)
                    {
                        filter.Append(";");
                    }

                    filter.Append("*.");
                    filter.Append(extension.Extension);
                    allFilter.Append("*.");
                    allFilter.Append(extension.Extension);
                }

                result.Append(" (");
                result.Append(filter);
                result.Append(")|");
                result.Append(filter);
            }

            if (allFilter.Length > 0)
            {
                if (result.Length > 0)
                {
                    result.Append("|");
                }

                result.Append(string.Format(Resources.GOREDIT_TEXT_SUPPORTED_FILES, allFilter));
            }

            if (result.Length > 0)
            {
                result.Append("|");
            }

            result.Append(Resources.GOREDIT_TEXT_ALL_FILES);

            return result.ToString();
        }

        /// <summary>
        /// Function to retrieve the parent form for the message box.
        /// </summary>
        /// <returns>The form to use as the owner.</returns>
        private static Form GetParentForm()
        {
            if (Form.ActiveForm != null)
            {
                return Form.ActiveForm;
            }

            return Application.OpenForms.Count > 1 ? Application.OpenForms[Application.OpenForms.Count - 1] : GorgonApplication.MainForm;
        }

        /// <summary>
        /// Function to return the dialog.
        /// </summary>
        /// <param name="allowMultiSelect"><b>true</b> to allow multiple file selection, or <b>false</b> to only allow single selection.</param>
        /// <returns>The open file dialog.</returns>
        private OpenFileDialog GetDialog(bool allowMultiSelect)
        {
            DirectoryInfo initialDirectory = InitialDirectory;

            if ((InitialDirectory == null) || (!InitialDirectory.Exists))
            {
                initialDirectory = new DirectoryInfo(Settings.LastOpenSavePath);
            }
                        
            if (!initialDirectory.Exists)
            {
                initialDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }

            return new OpenFileDialog
            {
                Title = string.IsNullOrWhiteSpace(DialogTitle) ? Resources.GOREDIT_TEXT_OPEN_EDITOR_FILE : DialogTitle,
                FileName = string.IsNullOrWhiteSpace(InitialFilePath) ? string.Empty : InitialFilePath,
                ValidateNames = true,
                SupportMultiDottedExtensions = true,
                Multiselect = allowMultiSelect,
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = FileFilter ?? GetReaderDialogFilterString(),
                InitialDirectory = initialDirectory.FullName,
                RestoreDirectory = true,
                FilterIndex = _lastSelectedFilterIndex
            };
        }

        /// <summary>
        /// Function to retrieve a single file name.
        /// </summary>
        /// <returns>The selected file path, or <b>null</b> if cancelled.</returns>
        public string GetFilename()
        {
            OpenFileDialog dialog = null;

            try
            {
                dialog = GetDialog(false);

                string result = dialog.ShowDialog(GetParentForm()) == DialogResult.Cancel ? null : dialog.FileName;

                _lastSelectedFilterIndex = dialog.FilterIndex;

                return result;
            }
            finally
            {
                dialog?.Dispose();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorFileOpenDialogService"/> class.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <param name="providers">The providers used for opening/saving files.</param>
        public EditorFileOpenDialogService(EditorSettings settings, IFileSystemProviders providers)
        {
            Settings = settings;
            Providers = providers;
        }
        #endregion
    }
}
