#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: December 12, 2021 1:32:41 AM
// 
#endregion

using System.Data;
using Gorgon.Editor.Content;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// A dialog used to save files back to the file system.
/// </summary>
internal partial class FormSaveDialog 
    : Form
{
    #region Variables.
    // The file manager for the project.
    private IContentFileManager _fileManager;
    // The file type filter.
    private (string FileTypeKeyName, string FileType) _fileType;
    // The current file name.
    private string _currentFileName;
    // The current directory.
    private string _currentDirectory;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the project file manager/
    /// </summary>
    public IContentFileManager FileManager
    {
        get => _fileManager;
        set
        {
            _fileManager = value;
            FillFiles(null);
        }
    }

    /// <summary>
    /// Property to set or return the filter for file type key name, and file type
    /// </summary>
    public (string FileTypeKeyName, string FileType) FileTypeFilter
    {
        get => _fileType;
        set
        {
            _fileType = value;
            FillFiles(null);
        }
    }

    /// <summary>
    /// Property to set or return the current directory.
    /// </summary>
    public string CurrentDirectory
    {
        get => _currentDirectory?.FormatDirectory('/') ?? string.Empty;
        set
        {
            _currentDirectory = value?.FormatDirectory('/') ?? string.Empty;
            TextFileName.Text = _currentDirectory + CurrentFileName;
        }
    }

    /// <summary>
    /// Property to set or return the current file name.
    /// </summary>
    public string CurrentFileName
    {
        get => _currentFileName?.FormatFileName() ?? string.Empty;
        set
        {
            _currentFileName = value?.FormatFileName() ?? string.Empty;
            TextFileName.Text = CurrentDirectory + _currentFileName;
        }
    }

    /// <summary>
    /// Property to return the currently selected file path.
    /// </summary>
    public string SelectedFilePath
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate the controls on the form.
    /// </summary>
    private void ValidateControls()
    {
        string fileName = string.Empty;

        if (!string.IsNullOrWhiteSpace(TextFileName.Text))
        {
            fileName = Path.GetFileName(TextFileName.Text)?.FormatFileName();
        }

        ButtonOK.Enabled = (_fileManager is not null) && (!string.IsNullOrWhiteSpace(fileName));
    }

    /// <summary>Handles the TextChanged event of the TextFileName control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void TextFileName_TextChanged(object sender, EventArgs e) => ValidateControls();

    /// <summary>Handles the Click event of the ButtonOK control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonOK_Click(object sender, EventArgs e)
    {
        string directoryName = Path.GetDirectoryName(TextFileName.Text)?.FormatDirectory('/');
        string fileName = Path.GetFileName(TextFileName.Text)?.FormatFileName();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            DialogResult = DialogResult.None;
            return;
        }

        SelectedFilePath = string.Empty;

        if (string.IsNullOrWhiteSpace(directoryName))
        {
            directoryName = "/";
        }

        try
        {
            if (!_fileManager.DirectoryExists(directoryName))
            {
                GorgonDialogs.ErrorBox(GorgonApplication.MainForm, string.Format(Resources.GOREDIT_ERR_DIR_NOT_FOUND, directoryName));
                DialogResult = DialogResult.None;
                return;
            }

            IContentFile file = _fileManager.GetFile(TextFileName.Text);

            if (file is null)
            {
                SelectedFilePath = directoryName + fileName;
                return;
            }

            SelectedFilePath = directoryName + fileName;
            if (GorgonDialogs.ConfirmBox(GorgonApplication.MainForm, string.Format(Resources.GOREDIT_CONFIRM_OVERWRITE_FILE, SelectedFilePath)) == ConfirmationResult.No)
            {
                DialogResult = DialogResult.None;
            }
        }
        catch (Exception ex)
        {
            GorgonDialogs.ErrorBox(GorgonApplication.MainForm, ex);
            DialogResult = DialogResult.None;
        }
    }

    /// <summary>Function called when an entry is focused in the list.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event parameters.</param>
    private void FileExplorer_FileEntriesFocused(object sender, ContentFileEntriesFocusedArgs e)
    {
        CurrentDirectory = FileExplorer.CurrentDirectory;

        if (e.FocusedFiles.Count > 0)
        {                
            TextFileName.Text = e.FocusedFiles[0].FullPath;
            return;
        }

        TextFileName.Text = CurrentDirectory + CurrentFileName;
        ValidateControls();
    }

    /// <summary>
    /// Function to fill the file list.
    /// </summary>
    /// <param name="searchText">The current search text.</param>
    private void FillFiles(string searchText)
    {
        FileExplorer.Entries = null;

        if (FileManager is null)
        {                
            return;
        }

        Dictionary<string, ContentFileExplorerDirectoryEntry> dirs = new(StringComparer.OrdinalIgnoreCase);

        // Get the directories.
        IEnumerable<string> directories = _fileManager.EnumerateDirectories("/", "*", true).Prepend("/");

        foreach (string directoryName in directories.OrderBy(item => item))
        {
            dirs[directoryName.FormatDirectory('/')] = new ContentFileExplorerDirectoryEntry(directoryName.FormatDirectory('/'), []);
        }

        foreach (IContentFile file in _fileManager.EnumerateContentFiles("/", "*", true).OrderBy(item => item.Path))
        {
            if ((!string.IsNullOrWhiteSpace(FileTypeFilter.FileTypeKeyName)) && (!string.IsNullOrWhiteSpace(FileTypeFilter.FileType)))
            {
                if ((!file.Metadata.Attributes.TryGetValue(FileTypeFilter.FileTypeKeyName, out string fileType))
                    || (!string.Equals(fileType, FileTypeFilter.FileType, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
            }

            if ((!string.IsNullOrWhiteSpace(searchText)) && (file.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1))
            {
                continue;
            }

            string dirName = Path.GetDirectoryName(file.Path).FormatDirectory('/');

            if (!dirs.TryGetValue(dirName, out ContentFileExplorerDirectoryEntry dirEntry))
            {
                continue;
            }               

            var fileEntries = (List<ContentFileExplorerFileEntry>)dirEntry.Files;
            ContentFileExplorerFileEntry contentFile = new(file, dirEntry);
            fileEntries.Add(contentFile);
        }

        FileExplorer.Entries = [.. dirs.Values];
        ValidateControls();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FormSaveDialog" /> class.</summary>
    public FormSaveDialog() => InitializeComponent();
    #endregion
}
