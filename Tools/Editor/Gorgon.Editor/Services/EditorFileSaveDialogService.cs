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

using System.Text;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.Services;

/// <summary>
/// A service used to show a dialog for saving an editor file.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EditorFileSaveDialogService"/> class.
/// </remarks>
/// <param name="settings">The application settings.</param>
/// <param name="providers">The providers used for opening/saving files.</param>
internal class EditorFileSaveDialogService(EditorSettings settings, FileSystemProviders providers)
        : IFileDialogService
{
    #region Variables.
    // The previously selected file extension filter index.
    private int _lastSelectedExtensionIndex = -1;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return a file filter.
    /// </summary>        
    public string FileFilter
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial file path to use.
    /// </summary>
    public string InitialFilePath
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
    public FileSystemProviders Providers
    {
        get;
    } = providers;

    /// <summary>
    /// Property to return the settings for the application.
    /// </summary>
    /// <value>The settings.</value>
    public EditorSettings Settings
    {
        get;
    } = settings;

    /// <summary>
    /// Property to set or return the currently active file writer plugin.
    /// </summary>        
    public FileWriterPlugIn CurrentWriter
    {
        get;
        set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to build a file system writer filter string for file dialogs.
    /// </summary>
    /// <param name="extensions">The extensions used by the available file writers.</param>
    /// <returns>The string containing the file dialog filter.</returns>
    private string GetWriterDialogFilterString(IReadOnlyList<(string desc, FileWriterPlugIn writer, IReadOnlyList<GorgonFileExtension> extensions)> extensions)
    {
        var result = new StringBuilder();
        var filter = new StringBuilder();

        foreach ((string desc, FileWriterPlugIn _, IReadOnlyList<GorgonFileExtension> extensions) item in extensions)
        {
            filter.Length = 0;

            if (result.Length > 0)
            {
                result.Append('|');
            }

            result.Append(item.desc);

            foreach (GorgonFileExtension extension in item.extensions)
            {
                if (filter.Length > 0)
                {
                    filter.Append(';');
                }

                filter.Append("*.");
                filter.Append(extension.Extension);
            }

            result.Append(" (");
            result.Append(filter);
            result.Append(")|");
            result.Append(filter);
        }

        return result.ToString();
    }

    /// <summary>
    /// Function to retrieve the parent form for the message box.
    /// </summary>
    /// <returns>The form to use as the owner.</returns>
    private static Form GetParentForm() => Form.ActiveForm ?? (Application.OpenForms.Count > 1 ? Application.OpenForms[Application.OpenForms.Count - 1] : GorgonApplication.MainForm);

    /// <summary>
    /// Function to find the extension filter index for the currently selected file writer.
    /// </summary>
    /// <param name="extensions">The extensions to evaluate.</param>
    private void FindCurrentWriterExtensionIndex(IReadOnlyList<(string desc, FileWriterPlugIn writer, IReadOnlyList<GorgonFileExtension> extensions)> extensions)
    {
        _lastSelectedExtensionIndex = -1;

        for (int i = 0; i < extensions.Count; ++i)
        {
            if (extensions[i].writer != CurrentWriter)
            {
                continue;
            }

            _lastSelectedExtensionIndex = i;
            return;
        }
    }

    /// <summary>
    /// Function to locate the nearest matching extension index based on previous file name.
    /// </summary>
    /// <param name="extensions">The extensions to evaluate.</param>
    private void FindNearestExtensionIndex(IReadOnlyList<(string desc, FileWriterPlugIn writer, IReadOnlyList<GorgonFileExtension> extensions)> extensions)
    {
        if ((_lastSelectedExtensionIndex < 0) || (_lastSelectedExtensionIndex >= extensions.Count))
        {
            _lastSelectedExtensionIndex = 0;
        }

        // Locate the previously selected file type by using the extension of the current file path.
        if (!string.IsNullOrWhiteSpace(InitialFilePath))
        {
            var currentExtension = new GorgonFileExtension(Path.GetExtension(InitialFilePath));

            for (int i = 0; i < extensions.Count; ++i)
            {
                if (!extensions[i].extensions.Any(item => item.Equals(currentExtension)))
                {
                    continue;
                }

                _lastSelectedExtensionIndex = i;
                return;
            }
        }
    }

    /// <summary>
    /// Function to return the dialog.
    /// </summary>
    /// <param name="extensions">The file extensions to evaluate.</param>
    /// <returns>The open file dialog.</returns>
    private SaveFileDialog GetDialog(IReadOnlyList<(string desc, FileWriterPlugIn writer, IReadOnlyList<GorgonFileExtension> extensions)> extensions)
    {
        DirectoryInfo initialDirectory = InitialDirectory;

        if ((InitialDirectory is null) || (!InitialDirectory.Exists))
        {
            initialDirectory = new DirectoryInfo(Settings.LastOpenSavePath);
        }

        if (!initialDirectory.Exists)
        {
            initialDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        if (CurrentWriter is not null)
        {
            FindCurrentWriterExtensionIndex(extensions);
        }

        if (_lastSelectedExtensionIndex < 0)
        {
            FindNearestExtensionIndex(extensions);
        }

        return new SaveFileDialog
        {
            Title = string.IsNullOrWhiteSpace(DialogTitle) ? Resources.GOREDIT_TEXT_SAVE_EDITOR_FILE : DialogTitle,
            FileName = string.IsNullOrWhiteSpace(InitialFilePath) ? string.Empty : InitialFilePath,
            ValidateNames = true,
            SupportMultiDottedExtensions = true,
            AutoUpgradeEnabled = true,
            Filter = FileFilter ?? GetWriterDialogFilterString(extensions),
            InitialDirectory = initialDirectory.FullName,
            RestoreDirectory = true,
            AddExtension = false,
            CreatePrompt = false,
            FilterIndex = _lastSelectedExtensionIndex + 1,
            OverwritePrompt = true,
            CheckPathExists = false,
            CheckFileExists = false
        };
    }

    /// <summary>
    /// Function to retrieve a single file name.
    /// </summary>
    /// <returns>The selected file path, or <b>null</b> if cancelled.</returns>
    public string GetFilename()
    {
        SaveFileDialog dialog = null;

        try
        {
            IReadOnlyList<(string desc, FileWriterPlugIn writer, IReadOnlyList<GorgonFileExtension> extensions)> extensions = Providers.GetWriterFileExtensions();

            dialog = GetDialog(extensions);

            string result = dialog.ShowDialog(GetParentForm()) == DialogResult.Cancel ? null : dialog.FileName;

            _lastSelectedExtensionIndex = dialog.FilterIndex - 1;

            if ((_lastSelectedExtensionIndex >= 0) && (_lastSelectedExtensionIndex <= extensions.Count))
            {
                CurrentWriter = extensions[_lastSelectedExtensionIndex].writer;
            }
            else
            {
                CurrentWriter = null;
            }

            return result;
        }
        finally
        {
            dialog?.Dispose();
        }
    }

    #endregion
}
