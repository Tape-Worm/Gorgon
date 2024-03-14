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
// Created: May 5, 2019 5:03:12 PM
// 
#endregion

using System.Collections;
using System.ComponentModel;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI.Views;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// A file explorer used to display editor content files.
/// </summary>
public partial class ContentFileExplorer
    : EditorBaseControl
{
    #region Classes.
    /// <summary>
    /// A comparer used to sort the files and directories.
    /// </summary>
    /// <remarks>Initializes a new instance of the <see cref="FileComparer"/> class.</remarks>
    /// <param name="gridView">The grid view.</param>
    /// <param name="directory">The directory.</param>
    /// <param name="directoryName">Name of the directory.</param>
    /// <param name="file">The file.</param>
    private class FileComparer(DataGridView gridView, DataGridViewColumn directory, DataGridViewColumn directoryName, DataGridViewColumn file)
                : IComparer<DataGridViewRow>, IComparer
    {
        #region Variables.
        // The grid containing the data to sort.
        private readonly DataGridView _grid = gridView;
        // The column containing the directory flag.
        private readonly DataGridViewColumn _columnDirectory = directory;
        // The column containing the directory path.
        private readonly DataGridViewColumn _columnDirectoryName = directoryName;
        // The column containing the file path.
        private readonly DataGridViewColumn _columnFile = file;
        #endregion

        #region Methods.
        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero
        /// <paramref name="x" /> is less than <paramref name="y" />.Zero
        /// <paramref name="x" /> equals <paramref name="y" />.Greater than zero
        /// <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
        public int Compare(DataGridViewRow x, DataGridViewRow y)
        {

            bool isDirLeft = _grid.Rows[x.Index].Cells[_columnDirectory.Index].Value.IfNull(false);
            bool isDirRight = _grid.Rows[y.Index].Cells[_columnDirectory.Index].Value.IfNull(false);

            string dirLeft = _grid.Rows[x.Index].Cells[_columnDirectoryName.Index].Value.IfNull(string.Empty);
            string dirRight = _grid.Rows[y.Index].Cells[_columnDirectoryName.Index].Value.IfNull(string.Empty);

            string nameLeft = _grid.Rows[x.Index].Cells[_columnFile.Index].Value.IfNull(string.Empty);
            string nameRight = _grid.Rows[y.Index].Cells[_columnFile.Index].Value.IfNull(string.Empty);

            int direction = _columnFile.HeaderCell.SortGlyphDirection == SortOrder.Ascending ? 1 : -1;
            int sortResult = string.Compare(dirLeft, dirRight, StringComparison.CurrentCultureIgnoreCase) * direction;

            if (sortResult != 0)
            {
                return sortResult;
            }

            return (isDirLeft) && (!isDirRight)
                ? -1
                : (!isDirLeft) && (isDirRight) ? 1 : string.Compare(nameLeft, nameRight, StringComparison.CurrentCultureIgnoreCase) * direction;
        }

        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero
        /// <paramref name="x" /> is less than <paramref name="y" />. Zero
        /// <paramref name="x" /> equals <paramref name="y" />. Greater than zero
        /// <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        int IComparer.Compare(object x, object y) => Compare((DataGridViewRow)x, (DataGridViewRow)y);

        #endregion
        #region Constructor.
        #endregion
    }
    #endregion

    #region Events.
    /// <summary>
    /// Event triggered when a search term is entered into the search box.
    /// </summary>
    [Category("Behavior"), Description("Triggered when a search term is entered in the search box.")]
    public event EventHandler<GorgonSearchEventArgs> Search;

    /// <summary>
    /// Event triggered when a file entry is selected.
    /// </summary>
    [Category("Behavior"), Description("Triggered when a file entry is selected.")]
    public event EventHandler<ContentFileEntrySelectedEventArgs> FileEntrySelected;

    /// <summary>
    /// Event triggered when the files are focused/highlighted on the file list.
    /// </summary>
    [Category("Behavior"), Description("Triggered when file entries are focused on the file list.")]
    public event EventHandler<ContentFileEntriesFocusedArgs> FileEntriesFocused;

    /// <summary>
    /// Event tiggered when a file entry is unselected.
    /// </summary>
    [Category("Behavior"), Description("Triggered when a file entry is unselected.")]
    public event EventHandler<ContentFileEntrySelectedEventArgs> FileEntryUnselected;
    #endregion

    #region Variables.
    // The directory font.
    private readonly Font _dirFont;
    // The comparer used to sort the grid.
    private readonly FileComparer _fileComparer;
    // The list of file explorer entries.
    private IReadOnlyList<ContentFileExplorerDirectoryEntry> _entries = [];
    // Cross reference for rows and entries.
    private readonly Dictionary<DataGridViewRow, ContentFileExplorerFileEntry> _rowFilesXref = [];
    private readonly Dictionary<DataGridViewRow, ContentFileExplorerDirectoryEntry> _rowDirsXref = [];
    private readonly Dictionary<ContentFileExplorerFileEntry, DataGridViewRow> _fileRowsXref = [];
    private readonly Dictionary<ContentFileExplorerDirectoryEntry, DataGridViewRow> _dirRowsXref = [];
    // Icon associations with the files.
    private readonly Dictionary<string, Image> _icons = new(StringComparer.OrdinalIgnoreCase);
    // The checkbox in the grid header.
    private CheckBox _checkBoxHeader;
    // The currently selected directory (for single select mode).
    private string _currentDirectory;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the list of entries to display.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<ContentFileExplorerDirectoryEntry> Entries
    {
        get => _entries;
        set
        {
            if (_entries == value)
            {
                return;
            }

            UnassignEvents();
            _entries = value;

            if (!IsHandleCreated)
            {
                return;
            }

            FillGrid();
        }
    }

    /// <summary>
    /// Property to set or return whether to use single selection, or multi-selection.
    /// </summary>
    [Browsable(true), Category("Behavior"), Description("Sets whether to allow single or multiple selections."), DefaultValue(true)]
    public bool MultiSelect
    {
        get => GridFiles.MultiSelect;
        set 
        {
            if (value == GridFiles.MultiSelect)
            {
                return;
            }

            GridFiles.MultiSelect = value;                

            GetCheckboxHeader();
        }
    }

    /// <summary>
    /// Property to set or return whether search is available or not.
    /// </summary>
    [Browsable(true), Category("Behavior"), Description("Sets whether search is available or not."), DefaultValue(true)]
    public bool ShowSearch
    {
        get => TextSearch.Visible;
        set => TextSearch.Visible = value;
    }

    /// <summary>
    /// Property to set or return the text to display in the file column header.
    /// </summary>
    [Browsable(true), Category("Appearance"), Description("Sets the text to display in the file column header."), DefaultValue("File")]
    public string FileColumnText
    {
        get => ColumnLocation.HeaderText;
        set
        {
            if (string.Equals(value, ColumnFullFilePath.HeaderText, StringComparison.CurrentCulture))
            {
                return;
            }

            ColumnLocation.HeaderText = value;
        }
    }

    /// <summary>
    /// Property to return the current directory.
    /// </summary>
    /// <remarks>
    /// This will only return a value if <see cref="MultiSelect"/> is <b>false</b>.
    /// </remarks>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CurrentDirectory => _currentDirectory;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to build the cache for the file icons.
    /// </summary>
    private void BuildIconCache()
    {
        foreach (KeyValuePair<string, Image> icon in _icons)
        {
            icon.Value?.Dispose();
        }

        _icons.Clear();

        if ((_entries is null) || (_entries.Count == 0))
        {
            return;
        }

        IEnumerable<ContentFileExplorerFileEntry> files = _entries.SelectMany(item => item.Files);

        foreach (ContentFileExplorerFileEntry file in files)
        {
            if ((string.IsNullOrWhiteSpace(file.AssociationType)) || (_icons.ContainsKey(file.AssociationType)) || (file.FileIcon is null))
            {
                continue;
            }

            _icons[file.AssociationType] = (Image)file.FileIcon.Clone();
        }
    }

    /// <summary>
    /// Function to unassign events tied to the entries.
    /// </summary>
    private void UnassignEvents()
    {
        if (_entries is null)
        {
            return;
        }

        foreach (ContentFileExplorerDirectoryEntry dirEntry in _entries)
        {
            dirEntry.PropertyChanged -= DirEntry_PropertyChanged;

            foreach (ContentFileExplorerFileEntry fileEntry in dirEntry.Files)
            {
                fileEntry.PropertyChanged -= FileEntry_PropertyChanged;
            }
        }
    }

    /// <summary>
    /// Function to populate the grid with the file entries.
    /// </summary>
    private void FillGrid()
    {
        GridFiles.Rows.Clear();
        _rowFilesXref.Clear();
        _rowDirsXref.Clear();
        _fileRowsXref.Clear();
        _dirRowsXref.Clear();

        if ((_entries is null) || (_entries.Count == 0))
        {
            BuildIconCache();
            LabelNoFiles.Visible = true;
            GridFiles.Enabled = TextSearch.Enabled = false;
            return;
        }

        LabelNoFiles.Visible = false;
        GridFiles.Enabled = TextSearch.Enabled = true;
        var rows = new List<DataGridViewRow>();
        var selected = new List<DataGridViewRow>();

        foreach (ContentFileExplorerDirectoryEntry dirEntry in _entries)
        {
            DataGridViewRow row = CreateRow(dirEntry);
            rows.Add(row);
            _rowDirsXref[row] = dirEntry;
            _dirRowsXref[dirEntry] = row;
            row.Cells[ColumnLocation.Index].ToolTipText = dirEntry.FullPath;

            foreach (ContentFileExplorerFileEntry fileEntry in dirEntry.Files)
            {
                row = CreateRow(fileEntry);
                rows.Add(row);
                _rowFilesXref[row] = fileEntry;
                _fileRowsXref[fileEntry] = row;
                row.Cells[ColumnLocation.Index].ToolTipText = fileEntry.FullPath;

                if (fileEntry.IsSelected)
                {
                    selected.Add(row);
                }

                fileEntry.PropertyChanged += FileEntry_PropertyChanged;
            }

            dirEntry.PropertyChanged += DirEntry_PropertyChanged;
        }

        GridFiles.Rows.AddRange([.. rows]);
        GridFiles.ClearSelection();
        foreach (DataGridViewRow row in selected)
        {
            row.Selected = true;
        }

        if (selected.Count > 0)
        {
            GridFiles.FirstDisplayedScrollingRowIndex = selected[^1].Index;
        }

        SetCheckState();

        BuildIconCache();
    }

    /// <summary>Handles the PropertyChanged event of the Entry control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DirEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var entry = (ContentFileExplorerDirectoryEntry)sender;

        if (!_dirRowsXref.TryGetValue(entry, out DataGridViewRow row))
        {
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(ContentFileExplorerDirectoryEntry.IsVisible):
                if (row.Visible != entry.IsVisible)
                {
                    row.Visible = entry.IsVisible;
                }
                break;
            case nameof(ContentFileExplorerDirectoryEntry.IsExpanded):
                if (row.Cells[ColumnSelected.Index].Value.IfNull(false) != entry.IsExpanded)
                {
                    row.Cells[ColumnSelected.Index].Value = entry.IsExpanded;
                }
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the FileEntry control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void FileEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var entry = (ContentFileExplorerFileEntry)sender;

        if (!_fileRowsXref.TryGetValue(entry, out DataGridViewRow row))
        {
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(ContentFileExplorerFileEntry.IsVisible):
                if (row.Visible != entry.IsVisible)
                {
                    row.Visible = entry.IsVisible;
                }
                break;
            case nameof(ContentFileExplorerFileEntry.IsSelected):
                if (row.Cells[ColumnSelected.Index].Value.IfNull(false) != entry.IsSelected)
                {
                    row.Cells[ColumnSelected.Index].Value = entry.IsSelected;

                    if (entry.IsSelected)
                    {
                        EventHandler<ContentFileEntrySelectedEventArgs> selectHandler = FileEntrySelected;
                        selectHandler?.Invoke(this, new ContentFileEntrySelectedEventArgs(entry));
                    }
                    else
                    {
                        EventHandler<ContentFileEntrySelectedEventArgs> unselectHandler = FileEntryUnselected;
                        unselectHandler?.Invoke(this, new ContentFileEntrySelectedEventArgs(entry));
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Function to display a checkbox in the first column header.
    /// </summary>
    private void GetCheckboxHeader()
    {
        if (_checkBoxHeader is not null)
        {
            _checkBoxHeader.Click -= CheckboxHeader_Click;
            GridFiles.Controls.Remove(_checkBoxHeader);
            _checkBoxHeader.Dispose();
            _checkBoxHeader = null;
        }

        if (!MultiSelect)
        {
            return;
        }

        Rectangle rect = GridFiles.GetCellDisplayRectangle(0, -1, true);
        // set checkbox header to center of header cell. +1 pixel to position 
        rect.Y = 3;
        rect.X = rect.Location.X + (rect.Width / 4);
        _checkBoxHeader = new CheckBox
        {
            Name = "GridHeaderCheckBox",
            AutoSize = true,
            Location = rect.Location,
            TabStop = false
        };

        _checkBoxHeader.Click += CheckboxHeader_Click;
        GridFiles.Controls.Add(_checkBoxHeader);

        SetCheckState();
    }

    /// <summary>Handles the Search event of the TextSearch control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GorgonSearchEventArgs"/> instance containing the event data.</param>
    private void TextSearch_Search(object sender, GorgonSearchEventArgs e) => OnSearch(new GorgonSearchEventArgs(e.SearchText));

    /// <summary>Handles the KeyUp event of the TextSearch control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    private void TextSearch_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Enter:
                OnSearch(new GorgonSearchEventArgs(TextSearch.Text));
                break;
            case Keys.Escape:
                OnSearch(null);
                break;
        }
    }

    /// <summary>Handles the CheckedChanged event of the CheckboxHeader control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckboxHeader_Click(object sender, EventArgs e)
    {
        var checkBox = (CheckBox)sender;

        for (int i = 0; i < _entries.Count; ++i)
        {
            ContentFileExplorerDirectoryEntry row = _entries[i];

            if (!row.IsVisible)
            {
                continue;
            }

            for (int j = 0; j < row.Files.Count; j++)
            {
                ContentFileExplorerFileEntry file = row.Files[j];

                if (!file.IsVisible)
                {
                    continue;
                }

                file.IsSelected = checkBox.Checked;
            }
        }
    }

    /// <summary>Handles the SelectionChanged event of the GridFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void GridFiles_SelectionChanged(object sender, EventArgs e)
    {
        _currentDirectory = string.Empty;

        if (GridFiles.SelectedRows.Count == 0)
        {
            FileEntriesFocused?.Invoke(this, new ContentFileEntriesFocusedArgs(null));
            return;
        }            

        var fileList = new List<ContentFileExplorerFileEntry>();
        var args = new ContentFileEntriesFocusedArgs(fileList);            

        foreach(DataGridViewRow row in GridFiles.SelectedRows.OfType<DataGridViewRow>().Reverse())
        {
            if (_rowFilesXref.TryGetValue(row, out ContentFileExplorerFileEntry fileEntry))
            {
                if (!MultiSelect)
                {
                    _currentDirectory = Path.GetDirectoryName(fileEntry.FullPath).FormatDirectory('/');
                }
                fileList.Add(fileEntry);
            }
            else if ((!MultiSelect) && (_rowDirsXref.TryGetValue(row, out ContentFileExplorerDirectoryEntry dirEntry)))
            {
                _currentDirectory = dirEntry.FullPath;
            }
        }

        FileEntriesFocused?.Invoke(this, args);
    }

    /// <summary>Handles the ColumnHeaderMouseClick event of the GridFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DataGridViewCellMouseEventArgs"/> instance containing the event data.</param>
    private void GridFiles_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        ColumnLocation.HeaderCell.SortGlyphDirection = ColumnLocation.HeaderCell.SortGlyphDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        GridFiles.Sort(_fileComparer);
    }

    /// <summary>
    /// Function to determine if a row represents a directory or not.
    /// </summary>
    /// <param name="row">The row to evaluate.</param>
    /// <returns><b>true</b> if the row represents a directory, <b>false</b> if not.</returns>
    private bool IsDirectoryRow(DataGridViewRow row) => row.Cells[ColumnDirectory.Index].Value.IfNull(false);


    /// <summary>Handles the CellMouseLeave event of the GridFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
    private void GridFiles_CellMouseLeave(object sender, DataGridViewCellEventArgs e) => TipFullPath.Hide(GridFiles);

    /// <summary>Handles the CellMouseEnter event of the GridFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
    private void GridFiles_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        Point mousePos = GridFiles.PointToClient(Cursor.Position);

        DataGridView.HitTestInfo hitTest = GridFiles.HitTest(mousePos.X, mousePos.Y);

        if ((hitTest is null) || (hitTest.ColumnIndex != ColumnLocation.Index) || (hitTest.RowIndex < 0))
        {
            return;
        }

        DataGridViewRow row = GridFiles.Rows[hitTest.RowIndex];

        string path;
        if (_rowDirsXref.TryGetValue(row, out ContentFileExplorerDirectoryEntry dirEntry))
        {
            path = dirEntry.FullPath;
        }
        else if (_rowFilesXref.TryGetValue(row, out ContentFileExplorerFileEntry fileEntry))
        {
            path = fileEntry.FullPath;
        }
        else
        {
            return;
        }

        TipFullPath.SetToolTip(GridFiles, path);
    }

    /// <summary>Handles the CellMouseDoubleClick event of the GridFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DataGridViewCellMouseEventArgs"/> instance containing the event data.</param>
    private void GridFiles_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex == ColumnSelected.Index)
        {
            return;
        }

        DataGridViewRow row = GridFiles.Rows[e.RowIndex];

        if (!_rowDirsXref.TryGetValue(row, out ContentFileExplorerDirectoryEntry dirEntry))
        {
            return;
        }

        if (!MultiSelect)
        {
            _currentDirectory = dirEntry.FullPath;
            dirEntry.IsExpanded = !dirEntry.IsExpanded;
            return;
        }

        IEnumerable<ContentFileExplorerFileEntry> files = dirEntry.Files;

        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            files = _entries.Where(item => item.FullPath.StartsWith(dirEntry.FullPath, StringComparison.OrdinalIgnoreCase))
                            .SelectMany(item => item.Files);
        }

        bool checkState = !files.All(item => item.IsSelected);

        foreach (ContentFileExplorerFileEntry entry in files)
        {
            if (entry.IsVisible)
            {
                entry.IsSelected = checkState;
            }
        }
    }

    /// <summary>
    /// Function to set the header checkbox state.
    /// </summary>
    private void SetCheckState()
    {
        if ((_checkBoxHeader is null) || (!MultiSelect))
        {
            return;
        }

        int checkCount = 0;
        int fileCount = 0;

        for (int i = 0; i < GridFiles.Rows.Count; ++i)
        {
            DataGridViewRow row = GridFiles.Rows[i];

            if (IsDirectoryRow(row))
            {
                continue;
            }

            if ((bool?)row.Cells[ColumnSelected.Index].Value ?? false)
            {
                checkCount++;
            }

            fileCount++;
        }

        if (checkCount == fileCount)
        {
            _checkBoxHeader.CheckState = CheckState.Checked;
            return;
        }

        if (checkCount > 0)
        {
            _checkBoxHeader.CheckState = CheckState.Indeterminate;
            return;
        }

        _checkBoxHeader.CheckState = CheckState.Unchecked;
    }

    /// <summary>Handles the CellMouseDown event of the GridFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DataGridViewCellMouseEventArgs"/> instance containing the event data.</param>
    private void GridFiles_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (((MultiSelect) && (e.ColumnIndex != ColumnSelected.Index)) || (e.RowIndex < 0))
        {
            return;
        }

        _currentDirectory = string.Empty;
        DataGridViewRow row = GridFiles.Rows[e.RowIndex];

        if ((IsDirectoryRow(row)) && (_rowDirsXref.TryGetValue(row, out ContentFileExplorerDirectoryEntry dirEntry)))
        {                
            if (!MultiSelect)
            {
                _currentDirectory = dirEntry.FullPath;
            }

            if (e.ColumnIndex == 0)
            {
                dirEntry.IsExpanded = !dirEntry.IsExpanded;
                return;
            }
        }

        if (!MultiSelect)
        {
            foreach (DataGridViewRow otherRow in GridFiles.Rows)
            {
                if (!_rowFilesXref.TryGetValue(otherRow, out ContentFileExplorerFileEntry otherFileEntry))
                {
                    continue;
                }

                otherFileEntry.IsSelected = false;
            }                
        }

        if (_rowFilesXref.TryGetValue(row, out ContentFileExplorerFileEntry fileEntry))
        {
            if (GridFiles.SelectedRows.Count == 1) 
            {
                if (!MultiSelect)
                {
                    _currentDirectory = Path.GetDirectoryName(fileEntry.FullPath).FormatDirectory('/');
                }

                fileEntry.IsSelected = !fileEntry.IsSelected;
                SetCheckState();

                return;
            }
        }

        if (fileEntry is null)
        {
            return;
        }

        bool checkValue = (!MultiSelect) || (!fileEntry.IsSelected);

        foreach (DataGridViewRow selectedRow in GridFiles.SelectedRows)
        {
            if (!_rowFilesXref.TryGetValue(selectedRow, out fileEntry))
            {
                continue;
            }

            if ((!MultiSelect) && (checkValue))
            {
                _currentDirectory = Path.GetDirectoryName(fileEntry.FullPath).FormatDirectory('/');
            }

            fileEntry.IsSelected = checkValue;
        }
        SetCheckState();
    }

    /// <summary>
    /// Function to draw a row containing a file.
    /// </summary>
    /// <param name="file">The file entry.</param>
    /// <param name="dpiScaling">The scaling factor based on DPI.</param>
    /// <param name="e">The event parameters from the cell paint event.</param>
    private void DrawFileRow(ContentFileExplorerFileEntry file, float dpiScaling, DataGridViewCellPaintingEventArgs e)
    {
        e.Handled = true;

        // Get the default icon.
        Image icon = Resources.generic_file_20x20;

        e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);
        e.CellStyle.Padding = new Padding((int)(8 * dpiScaling), 0, 0, 0);

        void DrawText() => TextRenderer.DrawText(e.Graphics,
                              file.Name,
                              Font,
                              new Rectangle(e.CellBounds.X + 24 + e.CellStyle.Padding.Left, e.CellBounds.Y, e.CellBounds.Width - 48 - e.CellStyle.Padding.Left * 2, e.CellBounds.Height),
                              ForeColor,
                              TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

        // No icon?  Then draw the generic one.
        if ((string.IsNullOrWhiteSpace(file.AssociationType))
            || (!_icons.TryGetValue(file.AssociationType, out icon)))
        {
            e.Graphics.DrawImage(icon ?? Resources.generic_file_20x20, new Rectangle(e.CellBounds.X + e.CellStyle.Padding.Left, e.CellBounds.Y + (e.CellBounds.Height / 2 - 10), 20, 20));
            DrawText();

            return;
        }

        e.Graphics.DrawImage(icon, new Rectangle(e.CellBounds.X + e.CellStyle.Padding.Left, e.CellBounds.Y + (e.CellBounds.Height / 2 - 10), 20, 20));
        DrawText();
    }

    /// <summary>
    /// Function to draw a row containing a directory.
    /// </summary>
    /// <param name="dir">The directory entry.</param>
    /// <param name="e">The event parameters from the cell paint event.</param>
    private void DrawDirectoryRow(ContentFileExplorerDirectoryEntry dir, DataGridViewCellPaintingEventArgs e)
    {
        e.Handled = true;

        e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);
        e.Graphics.DrawImage(Resources.folder_20x20, new Rectangle(e.CellBounds.X + e.CellStyle.Padding.Left, e.CellBounds.Y + (e.CellBounds.Height / 2 - 10), 20, 20));
        TextRenderer.DrawText(e.Graphics,
                              dir.Name,
                              _dirFont,
                              new Rectangle(e.CellBounds.X + 24, e.CellBounds.Y, e.CellBounds.Width - 48 - e.CellStyle.Padding.Left * 2, e.CellBounds.Height),
                              ForeColor,
                              TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
    }

    /// <summary>
    /// Function to draw the expander arror for a directory row.
    /// </summary>
    /// <param name="dir">The directory entry.</param>
    /// <param name="dpiScaling">The scaling factor based on DPI.</param>
    /// <param name="e">The event parameters from the cell paint event.</param>
    private void DrawExpando(ContentFileExplorerDirectoryEntry dir, float dpiScaling, DataGridViewCellPaintingEventArgs e)
    {
        e.Handled = true;
        Image image = dir.IsExpanded ? Resources.expanded_8x8 : Resources.collapsed_8x8;
        var scaledSize = new SizeF(image.Width * dpiScaling, image.Height * dpiScaling);
        e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);
        e.Graphics.DrawImage(image, new RectangleF(e.CellBounds.X + e.CellBounds.Width / 2 - scaledSize.Width / 2,
            e.CellBounds.Y + e.CellBounds.Height / 2 - scaledSize.Height / 2,
            scaledSize.Width,
            scaledSize.Height));
    }

    /// <summary>
    /// Function to draw the file selector checkbox.
    /// </summary>
    /// <param name="file">The file entry.</param>
    /// <param name="dpiScaling">The scaling factor based on DPI.</param>
    /// <param name="e">The event parameters from the cell paint event.</param>
    private void DrawFileSelector(ContentFileExplorerFileEntry file, float dpiScaling, DataGridViewCellPaintingEventArgs e)
    {
        e.Handled = true;
        var checkSize = new SizeF(Resources.check_8x8.Width * dpiScaling, Resources.check_8x8.Height * dpiScaling);

        Rectangle bounds = e.CellBounds;
        e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);

        if (!MultiSelect)
        {
            return;
        }

        bounds.Height = bounds.Width = (int)(12 * dpiScaling);
        bounds.X = e.CellBounds.X + e.CellBounds.Width / 2 - bounds.Width / 2;
        bounds.Y = e.CellBounds.Y + e.CellBounds.Height / 2 - bounds.Height / 2;

        using (var p = new Pen(Color.White, 2))
        {
            e.Graphics.DrawRectangle(p, bounds);
        }

        if (!file.IsSelected)
        {
            return;
        }

        e.Graphics.DrawImage(Resources.check_8x8, new RectangleF(e.CellBounds.X + e.CellBounds.Width / 2 - checkSize.Width / 2,
                                                                    e.CellBounds.Y + e.CellBounds.Height / 2 - checkSize.Height / 2,
                                                                    checkSize.Width,
                                                                    checkSize.Height));
    }

    /// <summary>Handles the CellPainting event of the GridFiles control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DataGridViewCellPaintingEventArgs"/> instance containing the event data.</param>
    private void GridFiles_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (((e.State & DataGridViewElementStates.Visible) != DataGridViewElementStates.Visible)
            || (e.RowIndex < 0))
        {
            return;
        }

        float dpiScale = e.Graphics.DpiY / 96.0f;
        DataGridViewRow row = GridFiles.Rows[e.RowIndex];
        ContentFileExplorerDirectoryEntry dirEntry = null;

        // Handle missing linkage.  We should never see this unless I messed up.
        if (!_rowFilesXref.TryGetValue(row, out ContentFileExplorerFileEntry fileEntry))
        {
            if (!_rowDirsXref.TryGetValue(row, out dirEntry))
            {
                e.PaintBackground(e.CellBounds, false);
                if (e.ColumnIndex == 0)
                {
                    e.Graphics.DrawImage(Resources.error_16x16, new Rectangle(e.CellBounds.X, e.CellBounds.Y, 16, 16));
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, Resources.GOREDIT_ERR_BROKEN_FILEROW, Font, e.CellBounds, Color.Red, TextFormatFlags.Left);
                }

                e.Handled = true;
                return;
            }
        }

        if (e.ColumnIndex == 0)
        {
            if (dirEntry is not null)
            {
                DrawExpando(dirEntry, dpiScale, e);
            }
            else
            {
                DrawFileSelector(fileEntry, dpiScale, e);
            }
            return;
        }

        if (fileEntry is not null)
        {
            DrawFileRow(fileEntry, dpiScale, e);
        }
        else
        {
            DrawDirectoryRow(dirEntry, e);
        }
    }

    /// <summary>
    /// Function used to add unbound row data to the grid.
    /// </summary>
    /// <param name="entry">The values used to populate the cells.</param>
    /// <returns>The new grid row.</returns>
    private DataGridViewRow CreateRow(ContentFileExplorerDirectoryEntry entry)
    {
        var newRow = new DataGridViewRow();
        newRow.CreateCells(GridFiles, [
            entry.IsExpanded,
            entry.FullPath,
            entry.Name,
            entry.FullPath,
            true
            ]);
        newRow.Visible = entry.IsVisible;
        return newRow;
    }

    /// <summary>
    /// Function used to add unbound row data to the grid.
    /// </summary>
    /// <param name="entry">The values used to populate the cells.</param>
    /// <returns>The new grid row.</returns>
    private DataGridViewRow CreateRow(ContentFileExplorerFileEntry entry)
    {
        var newRow = new DataGridViewRow();
        newRow.CreateCells(GridFiles, [
            entry.IsSelected,
            entry.Parent.FullPath,
            entry.Name,
            entry.FullPath,
            false
            ]);
        newRow.Visible = entry.IsVisible;
        return newRow;
    }

    /// <summary>
    /// Function to send the search event.
    /// </summary>
    /// <param name="e">The arguments for the event.</param>
    protected virtual void OnSearch(GorgonSearchEventArgs e)
    {
        EventHandler<GorgonSearchEventArgs> handler = Search;
        handler?.Invoke(this, e);
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if ((LicenseManager.UsageMode == LicenseUsageMode.Designtime) || (IsDesignTime))
        {
            return;
        }

        GetCheckboxHeader();

        FillGrid();

        if (GridFiles.Rows.Count == 0)
        {
            return;
        }

        // Set the default sorting.
        ColumnLocation.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
        GridFiles.Sort(_fileComparer);
        GridFiles.Select();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="ContentFileExplorer"/> class.</summary>
    public ContentFileExplorer()
    {
        InitializeComponent();

        _dirFont = new Font(Font, FontStyle.Bold);
        _fileComparer = new FileComparer(GridFiles, ColumnDirectory, ColumnDirName, ColumnLocation);
    }
    #endregion
}
