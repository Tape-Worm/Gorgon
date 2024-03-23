
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 4, 2019 12:49:00 PM
// 

using Gorgon.Editor.Properties;
using Gorgon.UI;

namespace Gorgon.Editor.Services;

/// <summary>
/// A service used to show a dialog for saving a file
/// </summary>
public class FileSaveDialogService
    : IFileDialogService
{
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
    /// Function to retrieve the parent form for the message box.
    /// </summary>
    /// <returns>The form to use as the owner.</returns>
    private static Form GetParentForm() => Form.ActiveForm ?? (Application.OpenForms.Count > 1 ? Application.OpenForms[Application.OpenForms.Count - 1] : GorgonApplication.MainForm);

    /// <summary>
    /// Function to return the dialog.
    /// </summary>
    /// <returns>The save file dialog.</returns>
    private SaveFileDialog GetDialog() => new()
    {
        Title = string.IsNullOrWhiteSpace(DialogTitle) ? Resources.GOREDIT_TITLE_SAVE_FILE : DialogTitle,
        FileName = string.IsNullOrWhiteSpace(InitialFilePath) ? string.Empty : InitialFilePath,
        ValidateNames = true,
        SupportMultiDottedExtensions = true,
        AutoUpgradeEnabled = true,
        Filter = FileFilter ?? string.Empty,
        InitialDirectory = InitialDirectory?.FullName,
        RestoreDirectory = true,
        AddExtension = false,
        CreatePrompt = false,
        OverwritePrompt = true,
        CheckPathExists = false,
        CheckFileExists = false
    };

    /// <summary>
    /// Function to retrieve a single file name.
    /// </summary>
    /// <returns>The selected file path, or <b>null</b> if cancelled.</returns>
    public virtual string GetFilename()
    {
        SaveFileDialog dialog = null;

        try
        {
            dialog = GetDialog();

            return dialog.ShowDialog(GetParentForm()) == DialogResult.Cancel ? null : dialog.FileName;
        }
        finally
        {
            dialog?.Dispose();
        }
    }
}
