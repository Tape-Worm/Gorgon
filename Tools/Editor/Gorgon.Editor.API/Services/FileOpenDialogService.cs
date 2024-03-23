
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: September 24, 2018 12:48:59 PM
// 

using Gorgon.Editor.Properties;
using Gorgon.UI;

namespace Gorgon.Editor.Services;

/// <summary>
/// A service used to show a dialog for opening a file
/// </summary>
public class FileOpenDialogService
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
    public string DialogTitle
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
    /// Function to retrieve the parent form for the message box.
    /// </summary>
    /// <returns>The form to use as the owner.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    private static Form GetParentForm()
    {
        if (Form.ActiveForm is not null)
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
    private OpenFileDialog GetDialog(bool allowMultiSelect) => new()
    {
        Title = string.IsNullOrWhiteSpace(DialogTitle) ? Resources.GOREDIT_TITLE_OPEN_FILE : DialogTitle,
        ValidateNames = true,
        SupportMultiDottedExtensions = true,
        Multiselect = allowMultiSelect,
        AutoUpgradeEnabled = true,
        CheckFileExists = true,
        CheckPathExists = true,
        Filter = FileFilter ?? string.Empty,
        InitialDirectory = InitialDirectory?.FullName,
        RestoreDirectory = true,
        FileName = InitialFilePath
    };

    /// <summary>
    /// Function to retrieve a single file name.
    /// </summary>
    /// <returns>The selected file path, or <b>null</b> if cancelled.</returns>
    public virtual string GetFilename()
    {
        OpenFileDialog dialog = null;

        try
        {
            dialog = GetDialog(false);

            return dialog.ShowDialog(GetParentForm()) == DialogResult.Cancel ? null : dialog.FileName;
        }
        finally
        {
            dialog?.Dispose();
        }
    }
}
