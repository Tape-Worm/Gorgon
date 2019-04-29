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
// Created: August 28, 2018 1:49:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Windows.Properties;

namespace Gorgon.UI
{
    /// <summary>
    /// A file folder browser.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This control is meant as a drop in component to allow for easy selection of file folders on a drive. This can be considered a replacement for the built-in .NET folder browser dialog as the UI
    /// for this control is much easier to navigate around. 
    /// </para>
    /// <para>
    /// The control is patterned after the FileDialogBrowser control, but without needing to have a file name. It is also more customizable allowing users to supply their own icons for folders and
    /// drive types.
    /// </para>
    /// </remarks>
    public partial class GorgonFolderBrowser
        : UserControl
    {
        #region Variables.
        // The directory that we are currently viewing in the list.
        private string _activeDirectory;
        // Flag to indicate that we're currently filling the list.
        private int _fillLock;
        // A list of directories to remember.
        private readonly List<DirectoryInfo> _undoDirectories = new List<DirectoryInfo>();
        // The undo index.
        private int _undoIndex = -1;
        // The column that is currently being sorted.
        private ColumnHeader _sortColumn;
        // The sorting order.
        private SortOrder _sortOrder = SortOrder.Ascending;
        // The icon used for a directory.
        private Image _directoryIcon;
        // The fixed disk icon.
        private Image _fixedDiskIcon;
        // The removable disk icon.
        private Image _removableDiskIcon;
        // The cd rom icon.
        private Image _cdRomIcon;
        // A ram drive icon.
        private Image _ramDriveIcon;
        // A network drive icon.
        private Image _networkDriveIcon;
        // A file icon.
        private Image _fileIcon;
        // The item that is being edited.
        private ListViewItem _editItem;
		// The folder to use as the root of the file system.
        private DirectoryInfo _rootFolder;
		// The directory separator character to use.
        private char _directorySeparator = Path.DirectorySeparatorChar;
        // The current directory.
        private string _currentDirectory;
        #endregion

        #region Events.
		/// <summary>
        /// Event triggered when a folder is about to be deleted from the file system.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when a folder is about to be deleted from the file system.")]
        public event EventHandler<FolderDeleteArgs> FolderDeleting;

		/// <summary>
        /// Event triggered when a folder is about to be created on the file system.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when a folder is about to be created on the file system.")]
        public event EventHandler<FolderAddArgs> FolderAdding;

		/// <summary>
        /// Event triggered when a folder is about to be renamed on the file system.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when a folder is about to be renamed on the file system.")]
        public event EventHandler<FolderRenameArgs> FolderRenaming;

        /// <summary>
        /// Event triggered when a folder is selected.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when the folder was selected in the interface.")]
        public event EventHandler<FolderSelectedArgs> FolderSelected;

        /// <summary>
        /// Event triggered when a folder is entered.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when the folder was set to active by entering it in the interface.")]
        public event EventHandler<FolderSelectedArgs> FolderEntered;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the character to use as the directory separator.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public char DirectorySeparator
        {
            get => _directorySeparator;
            set
            {
                if (_directorySeparator == value)
                {
                    return;
                }

                _directorySeparator = value;
                FillList(_currentDirectory);
                Invalidate();
            }
        }

		/// <summary>
        /// Property to set or return the folder that should be used as the root of the file system.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DirectoryInfo RootFolder
        {
            get => _rootFolder;
            set
            {
                if (string.Equals(_rootFolder?.FullName, value?.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

				// Check for trailing slash.
                if ((value != null) && (!value.FullName.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    value = new DirectoryInfo(value.FullName.FormatDirectory(Path.DirectorySeparatorChar));
                }

                _rootFolder = value;
                SetCurrentDirectory(RootFolder.FullName, false);
                Invalidate();
            }
        }

        /// <summary>
        /// Property to return whether or not the control is in design mode.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDesignTime
        {
            get;
        }

		/// <summary>
        /// Property to return the current directory.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CurrentDirectory => FormatDirectoryPath(_currentDirectory);

        /// <summary>
        /// Property to set or return the font to use for the caption.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the font to use in the caption."),
         RefreshProperties(RefreshProperties.Repaint),
         DefaultValue(typeof(Font), "Segoe UI, 9pt")]
        public Font CaptionFont
        {
            get => PanelCaption.Font;
            set
            {
                PanelCaption.Font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Property to set or return the font to use for the directory list items.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the font to use in the directory list."),
         RefreshProperties(RefreshProperties.Repaint),
         DefaultValue(typeof(Font), "Segoe UI, 9pt")]
        public Font DirectoryListFont
        {
            get => ListDirectories.Font;
            set
            {
                ListDirectories.Font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Property to set or return the font to use for the directory name text box.
        /// </summary>
        [Browsable(true),
         Category("Appearance"),
         Description("Sets the font to use in the directory name box."),
         RefreshProperties(RefreshProperties.Repaint),
         DefaultValue(typeof(Font), "Segoe UI, 9pt")]
        public Font DirectoryNameFont
        {
            get => PanelDirectoryName.Font;
            set
            {
                PanelDirectoryName.Font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always),
        Bindable(false), Description("Sets the text for the control header."), Category("Appearance"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string Text
        {
            get => LabelHeader.Text;
            set => LabelHeader.Text = base.Text = value;
        }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        /// <summary>
        /// Property to set or return the image to use as the icon for a directory.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, the image used will be the default for the operating system.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the icon to use for a directory."), DefaultValue(null)]
        public Image DirectoryImage
        {
            get => _directoryIcon;
            set
            {
                if (_directoryIcon == value)
                {
                    return;
                }

                _directoryIcon = value;

                if (IsDesignTime)
                {
                    return;
                }

                UpdateIcon(0, _directoryIcon);
            }
        }

        /// <summary>
        /// Property to set or return the image to use as the icon for a directory.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, the image used will be the default for the operating system.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the icon to use for a file."), DefaultValue(null)]
        public Image FileImage
        {
            get => _fileIcon;
            set
            {
                if (_fileIcon == value)
                {
                    return;
                }

                _fileIcon = value;

                if (IsDesignTime)
                {
                    return;
                }

                UpdateIcon(6, _fileIcon);
            }
        }

        /// <summary>
        /// Property to set or return the image to use as the icon for a hard drive.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, the image used will be the default for the operating system.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the icon to use for a hard drive."), DefaultValue(null)]
        public Image HardDriveImage
        {
            get => _fixedDiskIcon;
            set
            {
                if (_fixedDiskIcon == value)
                {
                    return;
                }

                _fixedDiskIcon = value;

                if (IsDesignTime)
                {
                    return;
                }

                UpdateIcon(1, _fixedDiskIcon);
            }
        }

        /// <summary>
        /// Property to set or return the image to use as the icon for a hard drive.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, the image used will be the default for the operating system.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the icon to use for a CD-ROM drive."), DefaultValue(null)]
        public Image CdRomImage
        {
            get => _cdRomIcon;
            set
            {
                if (_cdRomIcon == value)
                {
                    return;
                }

                _cdRomIcon = value;

                if (IsDesignTime)
                {
                    return;
                }

                UpdateIcon(3, _cdRomIcon);
            }
        }

        /// <summary>
        /// Property to set or return the image to use as the icon for a hard drive.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, the image used will be the default for the operating system.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the icon to use for a removable drive like a floppy disk."), DefaultValue(null)]
        public Image RemovableDriveImage
        {
            get => _removableDiskIcon;
            set
            {
                if (_removableDiskIcon == value)
                {
                    return;
                }

                _removableDiskIcon = value;

                if (IsDesignTime)
                {
                    return;
                }

                UpdateIcon(2, _removableDiskIcon);
            }
        }

        /// <summary>
        /// Property to set or return the image to use as the icon for a hard drive.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, the image used will be the default for the operating system.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the icon to use for a RAM disk drive."), DefaultValue(null)]
        public Image RamDriveImage
        {
            get => _ramDriveIcon;
            set
            {
                if (_ramDriveIcon == value)
                {
                    return;
                }

                _ramDriveIcon = value;

                if (IsDesignTime)
                {
                    return;
                }

                UpdateIcon(4, _ramDriveIcon);
            }
        }

        /// <summary>
        /// Property to set or return the image to use as the icon for a hard drive.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, the image used will be the default for the operating system.
        /// </remarks>
        [Browsable(true), Category("Appearance"), Description("Sets the icon to use for a network mounted drive."), DefaultValue(null)]
        public Image NetworkDriveImage
        {
            get => _networkDriveIcon;
            set
            {
                if (_networkDriveIcon == value)
                {
                    return;
                }

                _networkDriveIcon = value;

                if (IsDesignTime)
                {
                    return;
                }

                UpdateIcon(5, _networkDriveIcon);
            }
        }

        /// <summary>
        /// Property to return whether or not the error pane is open on the control.
        /// </summary>
        [Browsable(false)]
        public bool IsErrorPaneOpen => PanelError.Visible;
        #endregion

        #region Methods.
		/// <summary>
        /// Function to format the directory path name to fit the settings of the control.
        /// </summary>
        /// <param name="directoryName">The name of the directory.</param>
        /// <returns>The formatted directory name.</returns>
        private string FormatDirectoryPath(string directoryName)
        {
            string dirSep = _directorySeparator.ToString();

            if (string.IsNullOrWhiteSpace(directoryName))
            {
                return RootFolder == null ? string.Empty : dirSep;
            }

            var path = new StringBuilder(directoryName.FormatDirectory(Path.DirectorySeparatorChar));

            if (RootFolder != null)
            {
                directoryName = directoryName.FormatDirectory(Path.DirectorySeparatorChar);

                if (!directoryName.StartsWith(RootFolder.FullName))
                {
                    return dirSep;
                }

                path.Replace(RootFolder.FullName, dirSep);				
            }

            return path.ToString().FormatDirectory(_directorySeparator);
        }

        /// <summary>
        /// Handles the BeforeLabelEdit event of the ListDirectories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LabelEditEventArgs"/> instance containing the event data.</param>
        private void ListDirectories_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            try
            {
                if ((e.Item < 0) || (_activeDirectory == null))
                {
                    e.CancelEdit = true;
                    return;
                }

                _editItem = ListDirectories.Items[e.Item];

                // Files don't get any love here.
                if ((_editItem.Tag == null) || (!(_editItem.Tag is DirectoryInfo)))
                {
                    _editItem = null;
                    e.CancelEdit = true;
                }
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonDeleteDir control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonDeleteDir_Click(object sender, EventArgs e)
        {
            try
            {
                var dir = new DirectoryInfo(_currentDirectory.FormatDirectory(Path.DirectorySeparatorChar));
                string dirPath = FormatDirectoryPath(dir.FullName);

                var args = new FolderDeleteArgs(dirPath);
                EventHandler<FolderDeleteArgs> handler = FolderDeleting;
                handler?.Invoke(this, args);

                if (args.DeleteTask != null)
                {
					Enabled = false;
                    await args.DeleteTask;
                    Enabled = true;
                }

                if (args.Cancel)
                {
                    return;
                }

                if ((dir == null) ||
					((!args.SuppressPrompt) && (GorgonDialogs.ConfirmBox(ParentForm, string.Format(Resources.GOR_CONFIRM_DIR_DELETE, dirPath)) == ConfirmationResult.No)))
                {
                    return;
                }

                dir.Refresh();
                if ((!dir.Exists) || (args.DeletionHandled))
                {
                    SetCurrentDirectory(_activeDirectory, true);
                    return;
                }

                dir.Delete(true);

                // Reset back to the active directory.
                SetCurrentDirectory(_activeDirectory, true);
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                Enabled = true;
                ValidateControls();
            }
        }

        /// <summary>
        /// Function to return a name based on the base name passed in.
        /// </summary>
        /// <returns>The updated name.</returns>
        private string GetNewName(string baseName)
        {
            string name = baseName;
            int count = 1;
            string path = Path.Combine(_activeDirectory, name).FormatDirectory(Path.DirectorySeparatorChar);

            while ((Directory.Exists(path)) || (File.Exists(path)))
            {
                name = $"{baseName} ({count++})";
                path = Path.Combine(_activeDirectory, name).FormatDirectory(Path.DirectorySeparatorChar);
            }

            return name;
        }

		/// <summary>
        /// Function to fire the specified folder event.
        /// </summary>
        /// <param name="handler">The event to trigger.</param>
        /// <param name="directory">The directory being entered or selected.</param>
        private void OnFolderSelectedOrEntered(EventHandler<FolderSelectedArgs> handler, string directory)
        {
            if (handler == null)
            {
                return;
            }

            string path = FormatDirectoryPath(directory);
            handler.Invoke(this, new FolderSelectedArgs(path));
        }

        /// <summary>
        /// Handles the Click event of the ButtonAddDir control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonAddDir_Click(object sender, EventArgs e)
        {
            ListDirectories.ItemSelectionChanged -= ListDirectories_ItemSelectionChanged;

            try
            {
                string dirName = GetNewName(Resources.GOR_NEW_DIR);
                var dir = new DirectoryInfo(Path.Combine(_activeDirectory, dirName).FormatDirectory(Path.DirectorySeparatorChar));

                var args = new FolderAddArgs(FormatDirectoryPath(dir.Parent?.FullName), FormatDirectoryPath(dir.FullName), dir.Name);
                EventHandler<FolderAddArgs> handler = FolderAdding;
                FolderAdding?.Invoke(this, args);

                if (args.Cancel)
                {
                    return;
                }

                if (!args.CreationHandled)
                {
                    dir.Create();
                }

                dir.Refresh();                

                ListDirectories.Select();

                ListViewItem item = ListDirectories.Items.Add(dirName);
                item.ImageIndex = 0;
                item.Tag = dir;
                item.SubItems.Add($@"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");

                ListDirectories.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.HeaderSize);

                item.EnsureVisible();
                item.BeginEdit();

                ListDirectories.SelectedItems.Clear();                
                OnFolderSelectedOrEntered(FolderSelected, _activeDirectory);

                _editItem = item;
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                ListDirectories.ItemSelectionChanged += ListDirectories_ItemSelectionChanged;
            }
        }

        /// <summary>
        /// Handles the AfterLabelEdit event of the ListDirectories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LabelEditEventArgs"/> instance containing the event data.</param>
        private void ListDirectories_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListViewItem editingItem = null;

            try
            {
                if (e.Item < 0)
                {
                    SetCurrentDirectory(_activeDirectory, true);
                    e.CancelEdit = true;
                    return;
                }

                editingItem = ListDirectories.Items[e.Item];
                string name = string.IsNullOrWhiteSpace(e.Label) ? editingItem.Text : e.Label;

                if (name.Any(item => Path.GetInvalidFileNameChars().Any(pathItem => pathItem == item)))
                {
                    SetErrorMessage(string.Format(Resources.GOR_ERR_DIR_ILLEGAL_CHARS, name));
                    e.CancelEdit = true;
                    editingItem.BeginEdit();
                    return;
                }

                var dir = new DirectoryInfo(Path.Combine(_activeDirectory, name).FormatDirectory(Path.DirectorySeparatorChar));

                if ((!string.Equals(name, editingItem.Text, StringComparison.OrdinalIgnoreCase)) && ((dir.Exists) || (File.Exists(dir.FullName))))
                {
                    SetErrorMessage(string.Format(Resources.GOR_ERR_DIR_ALREADY_EXISTS, name));
                    e.CancelEdit = true;
                    editingItem.BeginEdit();
                    return;
                }

                if ((string.IsNullOrWhiteSpace(e.Label))
                    || (string.Equals(e.Label, editingItem.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    editingItem.Selected = true;
                    e.CancelEdit = true;
                    return;
                }

                var originalDir = new DirectoryInfo(Path.Combine(_activeDirectory, editingItem.Text).FormatDirectory(Path.DirectorySeparatorChar));

                var args = new FolderRenameArgs(FormatDirectoryPath(originalDir.FullName), originalDir.Name, FormatDirectoryPath(dir.FullName), dir.Name);
                EventHandler<FolderRenameArgs> handler = FolderRenaming;
                handler?.Invoke(this, args);

                if (args.Cancel)
                {
                    e.CancelEdit = true;
                    return;
                }

                if (!args.RenameHandled)
                {
                    originalDir.MoveTo(dir.FullName);
                }

                originalDir.Refresh();
                dir.Refresh();

                editingItem.Tag = dir;
                editingItem.Selected = true;
            }
            catch (Exception ex)
            {
                SetError(ex);

                e.CancelEdit = true;
                editingItem?.Remove();
            }
            finally
            {
                _editItem = null;
                ValidateControls();
            }
        }

        /// <summary>
        /// Function to update an icon in the image list.
        /// </summary>
        /// <param name="index">The index of the icon to update.</param>
        /// <param name="icon">The icon to use.</param>
        private void UpdateIcon(int index, Image icon)
        {
            // Make a copy of the image.  We don't want to destroy something we don't own.
            Image iconImage = null;

            if (icon != null)
            {
                iconImage = (Image)icon.Clone();
            }

            // Force the handle to create so any images that need to be copied are copied now.
            _ = Images.Handle;

            // If we've not been initialized yet, then return.
            if (index >= Images.Images.Count)
            {
                return;
            }

            Images.Images[index] = iconImage ?? DefaultImages.Images[index];
        }

        /// <summary>
        /// Handles the ColumnClick event of the ListDirectories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ColumnClickEventArgs"/> instance containing the event data.</param>
        private void ListDirectories_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            try
            {
                int columnIndex = e.Column.Max(0).Min(ListDirectories.Columns.Count - 1);
                ColumnHeader sortColumn = ListDirectories.Columns[columnIndex];

                _sortColumn = sortColumn;
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

                FillList(_activeDirectory);
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonDirUp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonDirUp_Click(object sender, EventArgs e)
        {
            try
            {				
                DirectoryInfo parent = null;
                DirectoryInfo actDir = null;
                if (_activeDirectory != null)
                {
                    actDir = new DirectoryInfo(_activeDirectory.FormatDirectory(Path.DirectorySeparatorChar));
                }

                string parentPath = actDir?.Parent.FullName.FormatDirectory(Path.DirectorySeparatorChar);

                if (!string.IsNullOrWhiteSpace(parentPath))
                {
                    parent = new DirectoryInfo(parentPath);
                }                

                if (parent == null)
                {
                    _undoIndex = -1;
                    SetCurrentDirectory(null, false);
                    return;
                }

                int undoIndex = FindInUndoStack(parent);

                if (undoIndex != -1)
                {
                    _undoIndex = undoIndex;
                }
                else
                {
                    _undoIndex = _undoDirectories.Count;
                    _undoDirectories.Add(parent);
                }

                SetCurrentDirectory(parent.FullName, false);
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
        /// Handles the MouseUp event of the ListDirectories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void ListDirectories_MouseUp(object sender, MouseEventArgs e)
        {
            if (_undoDirectories.Count == 0)
            {
                return;
            }

            try
            {
                if (e.Button == MouseButtons.XButton1)
                {
                    if (_undoIndex > 0)
                    {
                        SetCurrentDirectory(_undoDirectories[--_undoIndex].FullName, false);
                    }
                    else
                    {
                        _undoIndex = -1;
                        SetCurrentDirectory(null, false);
                    }
                }

                if ((_undoIndex >= _undoDirectories.Count - 1) || (e.Button != MouseButtons.XButton2))
                {
                    return;
                }

                SetCurrentDirectory(_undoDirectories[++_undoIndex].FullName, false);
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>Handles the KeyDown event of the ListDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void ListDirectories_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if ((ListDirectories.SelectedItems.Count == 1) && (_editItem == null))
                    {
                        ListDirectories_DoubleClick(ListDirectories, EventArgs.Empty);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the ListDirectories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void ListDirectories_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.Back:
                        if (_editItem == null)
                        {
                            ButtonDirUp.PerformClick();
                        }
                        break;
                    case Keys.Delete:
                        if (_editItem == null)
                        {
                            ButtonDeleteDir.PerformClick();
                        }
                        break;
                    case Keys.F2:
                        if ((ListDirectories.SelectedItems.Count == 1) && (_editItem == null))
                        {
                            ListDirectories.SelectedItems[0].BeginEdit();
                        }
                        break;
                    case Keys.F5:
                        if (_editItem == null)
                        {
                            FillList(_activeDirectory);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
        /// Handles the DoubleClick event of the ListDirectories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ListDirectories_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (ListDirectories.SelectedItems.Count == 0)
                {
                    return;
                }

                if (ListDirectories.SelectedItems[0].Tag is DirectoryInfo dir)
                {
                    SetCurrentDirectory(dir.FullName, true);
                }
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the TextDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void TextDirectory_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode != Keys.Enter)
                {
                    return;
                }

                string path = TextDirectory.Text.FormatDirectory(_directorySeparator);

                if (RootFolder != null)
                {
                    if ((string.IsNullOrWhiteSpace(path)) || (path == _directorySeparator.ToString()))
                    {
                        path = RootFolder.FullName.FormatDirectory(Path.DirectorySeparatorChar);
                    }
                    else
                    {
                        if (path.StartsWith(_directorySeparator.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            path = path.Substring(1);
                        }

                        path = Path.Combine(_activeDirectory, path.Replace(_directorySeparator, Path.DirectorySeparatorChar)).FormatDirectory(Path.DirectorySeparatorChar);
                    }
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    SetCurrentDirectory(null, true);
                    return;
                }               

                var dir = new DirectoryInfo(path);

                if (!dir.Exists)
                {
                    if (GorgonDialogs.ConfirmBox(ParentForm, string.Format(Resources.GOR_CONFIRM_CREATE_DIR, dir.FullName)) == ConfirmationResult.Yes)
                    {
                        dir.Create();
                        dir.Refresh();
                    }
                    else
                    {
                        TextDirectory.Text = FormatDirectoryPath(_currentDirectory);
                        dir = new DirectoryInfo(_currentDirectory.FormatDirectory(Path.DirectorySeparatorChar));
                    }
                }

                SetCurrentDirectory(dir.FullName, true);

                TextDirectory.SelectAll();
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClose_Click(object sender, EventArgs e) => SetError(null);

        /// <summary>
        /// Lists the directories item selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ListViewItemSelectionChangedEventArgs" /> instance containing the event data.</param>
        private void ListDirectories_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                DirectoryInfo dir = null;

                if (_currentDirectory != null)
                {
                    dir = new DirectoryInfo(_currentDirectory);
                }

                if ((e.Item == null) || ((!e.IsSelected) && (e.Item != null)) || (ListDirectories.SelectedItems.Count == 0))
                {
                    // Don't go any higher than the active directory.
                    if (!string.Equals(_currentDirectory, _activeDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        _currentDirectory = (RootFolder == null) ? dir?.Parent.FullName : _activeDirectory;
                        TextDirectory.Text = FormatDirectoryPath(_currentDirectory);
                    }
                }
                else
                {
                    dir = e.Item.Tag as DirectoryInfo;

                    if (dir == null)
                    {
                        return;
                    }

                    _currentDirectory = dir.FullName.FormatDirectory(Path.DirectorySeparatorChar);
                    TextDirectory.Text = FormatDirectoryPath(_currentDirectory);
                }

                if (e.IsSelected)
                {
                    OnFolderSelectedOrEntered(FolderSelected, _currentDirectory);
                }
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
        /// Function to find the directory in the undo stack.
        /// </summary>
        /// <param name="dir">The directory to locate.</param>
        /// <returns>The index of the directory in the undo stack.</returns>
        private int FindInUndoStack(DirectoryInfo dir)
        {
            if (dir == null)
            {
                return -1;
            }

            for (int i = 0; i < _undoDirectories.Count; ++i)
            {
                if (string.Equals(dir.FullName, _undoDirectories[i].FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Function to set the current directory.
        /// </summary>
        /// <param name="dir">The directory to set.</param>
        /// <param name="updateUndoList"><b>true</b> to update the undo list at the current directory, or <b>false</b> to leave it alone.</param>
        private void SetCurrentDirectory(string dir, bool updateUndoList)
        {
            dir = dir?.FormatDirectory(Path.DirectorySeparatorChar);
            if ((RootFolder != null) && ((dir == null) || (!dir.StartsWith(RootFolder.FullName))))
            {
                dir = RootFolder.FullName;
            }

            if (((_activeDirectory == null) && (dir != null))
                || ((_activeDirectory != null) && (dir == null)))
            {
                _sortColumn = ColumnDirectoryName;
                _sortOrder = SortOrder.Ascending;
            }

            _currentDirectory = dir;

            if (updateUndoList)
            {
                // Remove duplicates, or deleted directories if we have any.
                int i = 0;
                while (i < _undoDirectories.Count)
                {
                    DirectoryInfo undoDir = _undoDirectories[i];
                    undoDir.Refresh();

                    if ((!undoDir.Exists) || (string.Equals(undoDir.FullName, dir, StringComparison.OrdinalIgnoreCase)))
                    {
                        _undoDirectories.RemoveAt(i);
                        continue;
                    }

                    ++i;
                }

                if ((_undoIndex >= 0) && (_undoIndex < _undoDirectories.Count - 1))
                {
                    while (_undoIndex < _undoDirectories.Count - 1)
                    {
                        _undoDirectories.RemoveAt(_undoDirectories.Count - 1);
                    }
                }
                else if (_undoIndex == -1)
                {
                    _undoDirectories.Clear();
                }

                _undoDirectories.Add(new DirectoryInfo(dir));
                _undoIndex = _undoDirectories.Count - 1;
            }

            TextDirectory.Text = FormatDirectoryPath(dir);

            SetError(null);
            try
            {
                FillList(dir);
            }
            catch (UnauthorizedAccessException)
            {
                // If we do not have the security to read this directory, then allow the control to navigate out to its parent.
                _activeDirectory = dir;
                ValidateControls();

                throw;
            }

            _activeDirectory = dir;
            ValidateControls();

            OnFolderSelectedOrEntered(FolderEntered, dir);
        }

        /// <summary>
        /// Function to set the error state.
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        private void SetError(Exception ex)
        {
            if ((GorgonApplication.Log != null) && (ex != null))
            {
                GorgonApplication.Log.LogException(ex);
            }

            string message = string.Empty;
            if (ex != null)
            {
                message = string.Format(Resources.GORWIN_ERR_DIR_ACCESS, ex.Message);
            }

            SetErrorMessage(message);
        }

        /// <summary>
        /// Function to validate the controls on the form.
        /// </summary>
        private void ValidateControls()
        {
            ButtonDirUp.Enabled = (_activeDirectory != null) && (_currentDirectory != null) 
				&& ((RootFolder == null) || (!string.Equals(RootFolder.FullName, _activeDirectory, StringComparison.OrdinalIgnoreCase)));
            ButtonAddDir.Enabled = (_activeDirectory != null) && (_currentDirectory != null) && (_editItem == null);
            ButtonDeleteDir.Enabled = (ButtonAddDir.Enabled) && (!string.Equals(_currentDirectory, _activeDirectory, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Function to retrieve the directories.
        /// </summary>
        /// <param name="dir">The directory to evaluate.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.ListViewItem+ListViewSubItemCollection.Add(System.String)", Justification = "You moron.  There's nothing to localize in here")]
        private void GetDirectories(string dir)
        {
            var dirEntry = new DirectoryInfo(dir.FormatDirectory(Path.DirectorySeparatorChar));

            IEnumerable<DirectoryInfo> directories = dirEntry.GetDirectories()
                                                        .Where(item => ((item.Attributes & FileAttributes.System) != FileAttributes.System)
                                                                       && ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden));
            IEnumerable<FileInfo> files = dirEntry.GetFiles()
                                             .Where(item => ((item.Attributes & FileAttributes.System) != FileAttributes.System)
                                                            && ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden));

            if (_sortOrder == SortOrder.Ascending)
            {
                directories = _sortColumn == ColumnDirectoryName ? directories.OrderBy(item => item.Name) : directories.OrderBy(item => item.LastWriteTime);
                files = _sortColumn == ColumnDirectoryName ? files.OrderBy(item => item.Name) : files.OrderBy(item => item.LastWriteTime);
            }
            else
            {
                directories = _sortColumn == ColumnDirectoryName
                                  ? directories.OrderByDescending(item => item.Name)
                                  : directories.OrderByDescending(item => item.LastWriteTime);
                files = _sortColumn == ColumnDirectoryName
                            ? files.OrderByDescending(item => item.Name)
                            : files.OrderByDescending(item => item.LastWriteTime);
            }

            ColumnDirectoryDate.Text = Resources.GOR_TEXT_MODIFIED_DATE;

            foreach (DirectoryInfo directory in directories)
            {
                var item = new ListViewItem(directory.Name)
                {
                    ImageIndex = 0,
                    Tag = new DirectoryInfo(directory.FullName.FormatDirectory(Path.DirectorySeparatorChar)),
                    Font = Font
                };

                item.SubItems.Add($@"{directory.LastWriteTime.ToShortDateString()} {directory.LastWriteTime.ToShortTimeString()}");

                ListDirectories.Items.Add(item);
            }

            foreach (FileInfo file in files)
            {
                var item = new ListViewItem(file.Name)
                {
                    ImageIndex = 6,
                    Tag = null,
                    Font = Font,
                    ForeColor = Color.FromKnownColor(KnownColor.DimGray)
                };

                item.SubItems.Add($@"{file.LastWriteTime.ToShortDateString()} {file.LastWriteTime.ToShortTimeString()}");
                item.SubItems.Add($"{file.Length.FormatMemory()}");

                ListDirectories.Items.Add(item);
            }
        }

        /// <summary>
        /// Function to fill the list with dummy values at design time.
        /// </summary>
        private void FillDummyList()
        {
            ColumnDirectoryDate.Text = Resources.GOR_TEXT_FREE_SPACE;

            ListDirectories.BeginUpdate();

            try
            {
                ListDirectories.Items.Clear();
                var driveTypes = (DriveType[])Enum.GetValues(typeof(DriveType));

                foreach (DriveType drive in driveTypes)
                {
                    string driveName;
                    int imageIndex;
                    float freeSpace;

                    switch (drive)
                    {
                        case DriveType.Network:
                            driveName = @"Z:\";
                            imageIndex = 5;
                            freeSpace = 2_122_233_889;
                            break;
                        case DriveType.Ram:
                            driveName = @"Y:\";
                            imageIndex = 4;
                            freeSpace = 512_000_000;
                            break;
                        case DriveType.CDRom:
                            driveName = @"E:\";
                            imageIndex = 3;
                            freeSpace = 0;
                            break;
                        case DriveType.Removable:
                            driveName = @"A:\";
                            imageIndex = 2;
                            freeSpace = 1509949.44f;
                            break;
                        case DriveType.Fixed:
                            driveName = @"C:\";
                            imageIndex = 1;
                            freeSpace = 1_024_000_000;
                            break;
                        default:
                            continue;
                    }

                    var item = new ListViewItem(driveName)
                    {
                        Tag = null,
                        Font = Font,
                        ImageIndex = imageIndex
                    };


                    item.SubItems.Add($@"{freeSpace.FormatMemory()}");
                    ListDirectories.Items.Add(item);
                }

                ListDirectories.Sorting = SortOrder.Ascending;
                ListDirectories.Sort();
            }
            finally
            {
                ColumnDirectoryName.Width = ClientSize.Width / 2;
                ColumnDirectoryDate.Width = ClientSize.Width / 4;
                ListDirectories.EndUpdate();
            }
        }

        /// <summary>
        /// Function to retrieve the available drives.
        /// </summary>
        private void GetDrives()
        {
            IEnumerable<DriveInfo> drives = DriveInfo
                                            .GetDrives().Where(item => item.DriveType != DriveType.Unknown);

            if (_sortOrder == SortOrder.Ascending)
            {
                drives = _sortColumn == ColumnDirectoryName ? drives.OrderBy(item => item.Name) : drives.OrderBy(item => item.AvailableFreeSpace);
            }
            else
            {
                drives = _sortColumn == ColumnDirectoryName
                             ? drives.OrderByDescending(item => item.Name)
                             : drives.OrderByDescending(item => item.AvailableFreeSpace);
            }

            ColumnDirectoryDate.Text = Resources.GOR_TEXT_FREE_SPACE;

            foreach (DriveInfo drive in drives)
            {
                var item = new ListViewItem(drive.Name)
                {
                    Tag = drive.RootDirectory,
                    Font = Font
                };

                switch (drive.DriveType)
                {
                    case DriveType.Network:
                        item.ImageIndex = 5;
                        break;
                    case DriveType.Ram:
                        item.ImageIndex = 4;
                        break;
                    case DriveType.CDRom:
                        item.ImageIndex = 3;
                        break;
                    case DriveType.Removable:
                        item.ImageIndex = 2;
                        break;
                    default:
                        item.ImageIndex = 1;
                        break;
                }


                item.SubItems.Add($@"{drive.AvailableFreeSpace.FormatMemory()}");

                ListDirectories.Items.Add(item);
            }
        }

        /// <summary>
        /// Function to fill the list with directory entries.
        /// </summary>
        /// <param name="dir">The directory to enumerate.</param>
        private void FillList(string dir)
        {
            if (Interlocked.Exchange(ref _fillLock, 1) == 1)
            {
                return;
            }

            ListDirectories.BeginUpdate();

            try
            {
                ListDirectories.Items.Clear();

                if ((dir == null) && (RootFolder == null))
                {
                    if (ListDirectories.Columns.Contains(ColumnSize))
                    {
                        ListDirectories.Columns.Remove(ColumnSize);
                    }
                    GetDrives();
                }
                else
                {
                    if (!ListDirectories.Columns.Contains(ColumnSize))
                    {
                        ListDirectories.Columns.Add(ColumnSize);
                    }
                    if (dir == null)
                    {
                        dir = RootFolder.FullName;
                    }

                    GetDirectories(dir);
                }

                if (dir == null)
                {
                    return;
                }

                ListViewItem selected = ListDirectories.Items
							.OfType<ListViewItem>()
							.FirstOrDefault(item => string.Equals((item.Tag as DirectoryInfo)?.FullName, dir, StringComparison.OrdinalIgnoreCase));

                if (selected != null)
                {
                    selected.Selected = true;
                }
            }
            finally
            {
                SetError(null);

                if (dir != null)
                {
                    ListDirectories.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
                else
                {
                    ColumnDirectoryName.Width = ClientSize.Width / 2;
                    ColumnDirectoryDate.Width = ClientSize.Width / 4;
                }

                ListDirectories.EndUpdate();
                ListDirectories.SetSortIcon(_sortColumn.Index, _sortOrder);

                ValidateControls();

                Interlocked.Exchange(ref _fillLock, 0);
            }
        }

        /// <summary>
        /// Function to set the initial directory.
        /// </summary>
        /// <param name="dir">The directory to assign.</param>
        public void AssignInitialDirectory(DirectoryInfo dir)
        {
            string dirPath = dir?.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            try
            {
                // We're already set to this directory, so leave.
                if (string.Equals(_currentDirectory, dirPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                while ((dir != null) && (!dir.Exists))
                {
                    dir = dir.Parent;
                }

                _undoDirectories.Clear();
                _undoIndex = -1;

                if (dir != null)
                {
                    DirectoryInfo parent = dir.Parent;

                    while (parent != null)
                    {
                        _undoDirectories.Insert(0, new DirectoryInfo(parent.FullName.FormatDirectory(Path.DirectorySeparatorChar)));
                        parent = parent.Parent;
                    }

                    _undoDirectories.Add(new DirectoryInfo(dirPath));
                    _undoIndex = _undoDirectories.Count - 1;
                }

                SetCurrentDirectory(dirPath, false);
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    SetError(ex);
                }
            }
        }

        /// <summary>
        /// Function to retrieve the default icons for use in the default image list.
        /// </summary>
        private void GetDefaultIcons()
        {
            // We've already got our defaults, so no need to get them again.
            if (DefaultImages.Images.Count > 0)
            {
                return;
            }

            DefaultImages.Images.Clear();
            Images.Images.Clear();

            Icon icon = ShellApi.ExtractShellIcon(StandardShellIcons.Folder);
            DefaultImages.Images.Add("folder_48x48", icon);
            Images.Images.Add("folder_48x48", icon);
            icon = ShellApi.ExtractShellIcon(StandardShellIcons.Fixed);
            DefaultImages.Images.Add("drive_48x48", icon);
            Images.Images.Add("drive_48x48", icon);
            icon = ShellApi.ExtractShellIcon(StandardShellIcons.Removable);
            DefaultImages.Images.Add("drive_remove_48x48", icon);
            Images.Images.Add("drive_remove_48x48", icon);
            icon = ShellApi.ExtractShellIcon(StandardShellIcons.CdRom);
            DefaultImages.Images.Add("drive_cdrom_48x48", icon);
            Images.Images.Add("drive_cdrom_48x48", icon);
            icon = ShellApi.ExtractShellIcon(StandardShellIcons.RamDisk);
            DefaultImages.Images.Add("drive_ram_48x48", icon);
            Images.Images.Add("drive_ram_48x48", icon);
            icon = ShellApi.ExtractShellIcon(StandardShellIcons.Network);
            DefaultImages.Images.Add("drive_network_48x48", icon);
            Images.Images.Add("drive_network_48x48", icon);
            icon = ShellApi.ExtractShellIcon(StandardShellIcons.Unknown);
            DefaultImages.Images.Add("file_48x48", icon);
            Images.Images.Add("file_48x48", icon);

            if (IsDesignTime)
            {
                return;
            }

            // Update the live image with our custom ones (if we've assigned them).
            UpdateIcon(0, _directoryIcon);
            UpdateIcon(1, _fixedDiskIcon);
            UpdateIcon(2, _removableDiskIcon);
            UpdateIcon(3, _cdRomIcon);
            UpdateIcon(4, _ramDriveIcon);
            UpdateIcon(5, _networkDriveIcon);
            UpdateIcon(6, _fileIcon);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                GetDefaultIcons();

                if (IsDesignTime)
                {
                    FillDummyList();
                }
                else
                {
                    FillList(_currentDirectory);
                }
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Function to show an error message for the folder selector.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <remarks>
        /// <para>
        /// Passing <b>null</b>, or an empty string to the <paramref name="message"/> parameter will reset and hide the message bar on the control.
        /// </para>
        /// </remarks>
        public void SetErrorMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Tip.SetToolTip(PanelError, string.Empty);
                Tip.SetToolTip(LabelError, string.Empty);
                Tip.SetToolTip(LabelErrorIcon, string.Empty);
                PanelError.Visible = false;
                return;
            }

            Tip.SetToolTip(PanelError, message);
            Tip.SetToolTip(LabelError, message);
            Tip.SetToolTip(LabelErrorIcon, message);

            LabelError.Text = message.Replace('\n', ' ').Replace('\r', ' ');
            PanelError.Visible = true;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFolderBrowser"/> class.
        /// </summary>
        public GorgonFolderBrowser()
        {
            IsDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            InitializeComponent();

            ListDirectories.Sorting = SortOrder.None;
            _sortColumn = ColumnDirectoryName;
        }
        #endregion
    }
}
