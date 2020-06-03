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
// Created: September 4, 2018 3:13:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using Gorgon.UI;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The context for the file explorer.
    /// </summary>
    internal enum FileExplorerContext
    {
        /// <summary>
        /// No item is focused.
        /// </summary>
        None = 0,
        /// <summary>
        /// The directory tree is focused.
        /// </summary>
        DirectoryTree = 1,
        /// <summary>
        /// The file list is focused.
        /// </summary>
        FileList = 2
    }

    /// <summary>
    /// The file explorer used to manipulate files for the project.
    /// </summary>
    internal partial class FileExploder
        : EditorBaseControl, IDataContext<IFileExplorer>
    {
        #region Constants
        // The name for a dummy node.
        private const string DummyNodeName = "*$__DUMMY__$*";
        #endregion

        #region Events.
        // The event fired when the control context is changed.
        private event EventHandler ControlContextChangedEvent;

        // The event fired when the rename state is changed.
        private event EventHandler IsRenamingChangedEvent;

        /// <summary>
        /// The event fired when the <see cref="ControlContext"/> property is changed.
        /// </summary>
        [Description("Event fired when the ControlContext property is changed."), Category("Property Changed")]
        public event EventHandler ControlContextChanged
        {
            add
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        ControlContextChangedEvent = null;
                        return;
                    }

                    ControlContextChangedEvent += value;
                }                
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        return;
                    }

                    ControlContextChangedEvent -= value;
                }
            }
        }

        /// <summary>
        /// The event fired when the <see cref="IsRenaming"/> property is changed.
        /// </summary>
        [Description("Event fired when the IsRenaming property is changed."), Category("Property Changed")]
        public event EventHandler IsRenamingChanged
        {
            add
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        IsRenamingChangedEvent = null;
                        return;
                    }

                    IsRenamingChangedEvent += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        return;
                    }

                    IsRenamingChangedEvent -= value;
                }
            }
        }
        #endregion

        #region Variables.
        // The synchronization lock for the control context changed event.
        private readonly object _eventLock = new object();
        // Flag to indicate that the events for the tree are hooked up.
        private int _treeEventsHooked = 0;
        // Flag to indicate that the events for the grid are hooked up.
        private int _gridEventsHooked = 0;
        // The nodes for a directory.
        private readonly Dictionary<string, DirectoryTreeNode> _directoryNodes = new Dictionary<string, DirectoryTreeNode>(StringComparer.OrdinalIgnoreCase);
        // The root node for the directory tree.
        private DirectoryTreeNode _rootNode;
        // The font used for open files.
        private readonly Font _openFileFont;
        // Arguments used when validating clipboard menu items.
        private readonly GetClipboardDataTypeArgs _clipboardValidationArgs = new GetClipboardDataTypeArgs();
        private readonly DeleteArgs _deleteValidationArgs = new DeleteArgs(null);
        private readonly CreateDirectoryArgs _createDirectoryValidationArgs = new CreateDirectoryArgs();
        private readonly string[] _validationFiles = { "Dummy " };
        // The styles for file states.
        private readonly DataGridViewCellStyle _cutCellStyle;
        private readonly DataGridViewCellStyle _openFileStyle;
        private readonly DataGridViewCellStyle _unknownFileStyle;
        // The drag data for importing Windows Explorer files/directories.
        private ExplorerImportData _explorerImportData;
        // The control context.
        private FileExplorerContext _controlContext = FileExplorerContext.None;
        // Flag to indicate if a rename operation is active.
        private bool _isRenaming;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        [Browsable(false)]
        public IFileExplorer DataContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current control context.
        /// </summary>
        [Browsable(false)]
        public FileExplorerContext ControlContext
        {
            get => _controlContext;
            private set
            {
                if (value == _controlContext)
                {
                    return;
                }

                _controlContext = value;
                OnControlContextChanged();
            }
        }

        /// <summary>
        /// Property to return if a directory or file is in the middle of a rename operation.
        /// </summary>
        [Browsable(false)]
        public bool IsRenaming
        {
            get => _isRenaming;
            private set
            {
                if (value == _isRenaming)
                {
                    return;
                }

                _isRenaming = value;
                OnIsRenamingChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to fire the <see cref="ControlContextChanged"/> event.
        /// </summary>
        private void OnControlContextChanged()
        {
            EventHandler handler = null;

            lock (_eventLock)
            {
                handler = ControlContextChangedEvent;
            }

            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to fire the <see cref="IsRenamingChanged"/> event.
        /// </summary>
        private void OnIsRenamingChanged()
        {
            EventHandler handler = null;

            lock (_eventLock)
            {
                handler = IsRenamingChangedEvent;
            }

            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to validate the menu items for the tree context menu.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void ValidateMenuItems(IFileExplorer dataContext)
        {
            // Disables all directory menu items.
            void DisableAllDirectoryItems()
            {
                foreach (ToolStripItem item in MenuDirectory.Items.OfType<ToolStripItem>())
                {
                    item.Available = false;
                }
            }

            // Disables all file menu items.
            void DisableAllFileItems()
            {
                foreach (ToolStripItem item in MenuFiles.Items.OfType<ToolStripItem>())
                {
                    item.Available = false;
                }
            }

            if (dataContext == null) 
            {
                DisableAllDirectoryItems();
                DisableAllFileItems();
                return;
            }

            if (dataContext.SelectedDirectory == null)
            {
                DisableAllDirectoryItems();
            }


            if ((dataContext.Clipboard?.GetClipboardDataTypeCommand != null) && (dataContext.Clipboard.GetClipboardDataTypeCommand.CanExecute(_clipboardValidationArgs)))
            {
                dataContext.Clipboard.GetClipboardDataTypeCommand.Execute(_clipboardValidationArgs);
            }

            void ValidateDirectoryItems()
            {
                MenuItemExportDirectoryTo.Available = dataContext.ExportDirectoryCommand?.CanExecute(null) ?? false;
                MenuItemCutDirectory.Available =
                MenuItemCopyDirectory.Available =
                MenuItemPasteDirectory.Available = (dataContext.Clipboard != null) &&  (TreeDirectories.SelectedNode != null) && (TreeDirectories.ContainsFocus);
                MenuItemDeleteDirectory.Available = dataContext.DeleteDirectoryCommand?.CanExecute(_deleteValidationArgs) ?? false;
                MenuItemRenameDirectory.Available = dataContext.RenameDirectoryCommand?.CanExecute(null) ?? false;
                MenuItemCreateDirectory.Available = dataContext.CreateDirectoryCommand?.CanExecute(_createDirectoryValidationArgs) ?? false;

                MenuItemCutDirectory.Enabled = (MenuItemCutDirectory.Available)
                                            && ((dataContext.Clipboard?.CopyDataCommand?.CanExecute(new DirectoryCopyMoveData
                                            {
                                                Operation = CopyMoveOperation.Move,
                                                SourceDirectory = dataContext.SelectedDirectory?.ID ?? string.Empty
                                            }) ?? false));
                MenuItemCopyDirectory.Enabled = (MenuItemCopyDirectory.Available)
                                            && ((dataContext.Clipboard?.CopyDataCommand?.CanExecute(new DirectoryCopyMoveData
                                            {
                                                Operation = CopyMoveOperation.Copy,
                                                SourceDirectory = dataContext.SelectedDirectory?.ID ?? string.Empty
                                            }) ?? false));
                MenuItemPasteDirectory.Enabled = (MenuItemPasteDirectory.Available) 
                                            && (dataContext.Clipboard?.PasteDataCommand?.CanExecute(null) ?? false);

                MenuSepDirEdit.Available = MenuItemExportDirectoryTo.Available;
                MenuSepDirOrganize.Available = (MenuItemCutDirectory.Available) || (MenuItemCopyDirectory.Available) || (MenuItemPasteDirectory.Available);
                MenuSepDirNew.Available = (MenuItemCreateDirectory.Available) && ((MenuItemRenameDirectory.Available) || (MenuItemDeleteDirectory.Available));
            }

            void ValidateFileItems()
            {
                MenuItemOpen.Available = dataContext.SelectedFiles.Count != 0;
                MenuItemExportFiles.Available = dataContext.ExportFilesCommand?.CanExecute(null) ?? false;
                MenuItemCutFiles.Available = 
                MenuItemCopyFiles.Available = (dataContext.SelectedFiles.Count > 0) && (dataContext.Clipboard != null) && (dataContext.SearchResults == null) && (GridFiles.ContainsFocus);
                MenuItemPasteFiles.Available = (dataContext.Clipboard != null) && (dataContext.SearchResults == null) && (GridFiles.ContainsFocus);
                MenuItemDeleteFiles.Available = dataContext.DeleteFileCommand?.CanExecute(_deleteValidationArgs) ?? false;
                MenuItemRenameFile.Available = dataContext.RenameFileCommand?.CanExecute(null) ?? false;

                MenuItemOpen.Enabled = (MenuItemOpen.Available) && (dataContext.OpenContentFileCommand?.CanExecute(null) ?? false);
                MenuItemExportFiles.Enabled = (dataContext.SelectedFiles.Count > 0);                
                MenuItemCutFiles.Enabled = (MenuItemCutFiles.Available)
                                            && ((dataContext.Clipboard?.CopyDataCommand?.CanExecute(new FileCopyMoveData
                                            {
                                                Operation = CopyMoveOperation.Move,
                                                SourceFiles = _validationFiles
                                            }) ?? false));
                MenuItemCopyFiles.Enabled = (MenuItemCopyFiles.Available)
                                            && ((dataContext.Clipboard?.CopyDataCommand?.CanExecute(new FileCopyMoveData
                                            {
                                                Operation = CopyMoveOperation.Copy,
                                                SourceFiles = _validationFiles
                                            }) ?? false));                
                MenuItemPasteFiles.Enabled = (MenuItemPasteFiles.Available)
                                            && (dataContext.Clipboard?.PasteDataCommand?.CanExecute(null) ?? false);
                MenuSepFileExport.Available = MenuItemOpen.Available;                
                MenuSepFileEdit.Available = MenuItemExportFiles.Available;
                MenuSepFileOrganize.Available = ((MenuItemRenameFile.Available) || (MenuItemDeleteFiles.Available))
                                            &&  ((MenuItemCutFiles.Available) || (MenuItemCopyFiles.Available) || (MenuItemPasteFiles.Available));                
            }

            if (dataContext.SelectedDirectory != null)
            {
                ValidateDirectoryItems();
            }

            ValidateFileItems();
        }

        /// <summary>
        /// Function to delete any selected files.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DeleteSelectedFilesAsync()
        {
            DisableGridEvents();

            try
            {
                var args = new DeleteArgs(null);
                if ((DataContext?.DeleteFileCommand == null) || (!DataContext.DeleteFileCommand.CanExecute(args)))
                {
                    return;
                }

                await DataContext.DeleteFileCommand.ExecuteAsync(args);
            }
            finally
            {
                EnableGridEvents();
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the Click event of the MenuItemCreateDirectory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCreateDirectory_Click(object sender, EventArgs e)
        {
            try
            {
                var args = new CreateDirectoryArgs();
                if ((DataContext?.CreateDirectoryCommand == null) || (!DataContext.CreateDirectoryCommand.CanExecute(args)))
                {
                    return;
                }

                DataContext.CreateDirectoryCommand.Execute(args);                

                // Since we created a new directory, we need to ensure it's visible on the tree so we can give it a new name.
                IDirectory directory = args.Directory;
                DirectoryTreeNode node = null;

                while (node == null)
                {
                    // Walk up the parents and keep expanding.
                    // This should never be more than 1 level as it's physically impossible to create a directory from the UI
                    // within a subdirectory of a collapsed parent.
                    if (_directoryNodes.TryGetValue(directory.ID, out node))
                    {
                        if (!node.IsExpanded)
                        {
                            node.Expand();
                        }
                        break;
                    }

                    directory = directory.Parent;
                }

                if (!_directoryNodes.TryGetValue(args.Directory.ID, out node))
                {
                    return;
                }

                // Select our new node.
                SelectNode(node);

                // Force a rename operation.
                MenuItemRenameDirectory.PerformClick();
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the Click event of the MenuItemDeleteDirectory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void MenuItemDeleteDirectory_Click(object sender, EventArgs e) => await DeleteDirectoryAsync();

        /// <summary>Handles the Click event of the MenuItemDeleteFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void MenuItemDeleteFiles_Click(object sender, EventArgs e) => await DeleteSelectedFilesAsync();

        /// <summary>Handles the Click event of the MenuItemRenameDirectory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemRenameDirectory_Click(object sender, EventArgs e)
        {
            if ((DataContext?.RenameDirectoryCommand == null) || (!DataContext.RenameDirectoryCommand.CanExecute(null)))
            {
                return;
            }

            TreeDirectories.BeginEdit();
            IsRenaming = true;
        }

        /// <summary>Handles the Click event of the MenuItemRenameFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemRenameFile_Click(object sender, EventArgs e)
        {
            if ((DataContext?.RenameFileCommand == null) || (!DataContext.RenameFileCommand.CanExecute(null)))
            {
                return;
            }

            GridFiles.BeginEdit(true);
            IsRenaming = true;
        }

        /// <summary>Handles the Click event of the MenuItemPasteFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemPasteFiles_Click(object sender, EventArgs e) => MenuItemPasteDirectory_Click(sender, e);

        /// <summary>Handles the Click event of the MenuItemExportDirectoryTo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void MenuItemExportDirectoryTo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ExportDirectoryCommand == null) || (!DataContext.ExportDirectoryCommand.CanExecute(null)))
            {
                return;
            }

            await DataContext.ExportDirectoryCommand.ExecuteAsync(null);
        }

        /// <summary>Handles the Click event of the MenuItemExportFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void MenuItemExportFiles_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ExportFilesCommand == null) || (!DataContext.ExportFilesCommand.CanExecute(null)))
            {
                return;
            }

            await DataContext.ExportFilesCommand.ExecuteAsync(null);
        }

        /// <summary>Handles the Click event of the MenuItemCopyFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCopyFiles_Click(object sender, EventArgs e) => CopyFileToClipboard(CopyMoveOperation.Copy);

        /// <summary>Handles the Click event of the MenuItemCutFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCutFiles_Click(object sender, EventArgs e) => CopyFileToClipboard(CopyMoveOperation.Move);

        /// <summary>Handles the Click event of the MenuItemCutDirectory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCutDirectory_Click(object sender, EventArgs e) => CopyDirectoryToClipboard(CopyMoveOperation.Move);

        /// <summary>Handles the Click event of the MenuItemCopyDirectory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCopyDirectory_Click(object sender, EventArgs e) => CopyDirectoryToClipboard(CopyMoveOperation.Copy);

        /// <summary>Handles the Click event of the MenuItemPasteDirectory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void MenuItemPasteDirectory_Click(object sender, EventArgs e)
        {
            if (DataContext?.Clipboard == null)
            {
                return;
            }

            try
            {
                IClipboardHandler handler = DataContext.Clipboard;
                if ((handler.PasteDataCommand == null) || (!handler.PasteDataCommand.CanExecute(null)))
                {
                    return;
                }

                await handler.PasteDataCommand.ExecuteAsync(null);
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the Click event of the MenuItemCopyTo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void MenuItemCopyTo_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            try
            {
                if ((MenuCopyMove.Tag is GridRowsDragData rowData) && (DataContext.CopyFileCommand != null) && (DataContext.CopyFileCommand.CanExecute(rowData)))
                {
                    await DataContext.CopyFileCommand.ExecuteAsync(rowData);
                }

                if (!(MenuCopyMove.Tag is TreeNodeDragData nodeData))
                {
                    MenuCopyMove.Tag = null;
                    return;
                }

                if ((DataContext.CopyDirectoryCommand != null) && (DataContext.CopyDirectoryCommand.CanExecute(nodeData)))
                {
                    await DataContext.CopyDirectoryCommand.ExecuteAsync(nodeData);
                }

                MenuCopyMove.Tag = null;
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the Click event of the MenuItemMoveTo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void MenuItemMoveTo_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            try
            {
                if ((MenuCopyMove.Tag is GridRowsDragData rowData) && (DataContext.MoveFileCommand != null) && (DataContext.MoveFileCommand.CanExecute(rowData)))
                {
                    await DataContext.MoveFileCommand.ExecuteAsync(rowData);
                }

                if (!(MenuCopyMove.Tag is TreeNodeDragData data))
                {
                    MenuCopyMove.Tag = null;
                    return;
                }

                if ((DataContext.MoveDirectoryCommand != null) && (DataContext.MoveDirectoryCommand.CanExecute(data)))
                {
                    await DataContext.MoveDirectoryCommand.ExecuteAsync(data);
                }

                MenuCopyMove.Tag = null;
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the Opening event of the MenuFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void MenuFiles_Opening(object sender, CancelEventArgs e) => ValidateMenuItems(DataContext);

        /// <summary>Handles the Opening event of the MenuDirectory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void MenuDirectory_Opening(object sender, CancelEventArgs e) => ValidateMenuItems(DataContext);

        /// <summary>Handles the Click event of the MenuItemOpen control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemOpen_Click(object sender, EventArgs e)
        {
            if ((DataContext?.OpenContentFileCommand == null) || (!DataContext.OpenContentFileCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OpenContentFileCommand.ExecuteAsync(null);
            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the CollectionChanged event of the SelectedFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void SelectedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            string id;

            DisableGridEvents();

            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        id = ((IFile)e.NewItems[0]).ID;

                        // Select the row that contains the file path.
                        for (int rowIndex = 0; rowIndex < GridFiles.Rows.Count; ++rowIndex)
                        {
                            string rowPath = GridFiles.Rows[rowIndex].Cells[ColumnID.Index].Value.IfNull(string.Empty);

                            if (string.Equals(id, rowPath, StringComparison.OrdinalIgnoreCase))
                            {
                                GridFiles.Rows[rowIndex].Selected = true;                                
                                break;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        id = ((IFile)e.OldItems[0]).ID;

                        // Select the row that contains the file path.
                        for (int rowIndex = 0; rowIndex < GridFiles.Rows.Count; ++rowIndex)
                        {
                            string rowPath = GridFiles.Rows[rowIndex].Cells[ColumnID.Index].Value.IfNull(string.Empty);

                            if (string.Equals(id, rowPath, StringComparison.OrdinalIgnoreCase))
                            {
                                GridFiles.Rows[rowIndex].Selected = false;
                                break;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        GridFiles.ClearSelection();
                        break;
                }
            }
            finally
            {
                EnableGridEvents();
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the CollectionChanged event of the Files control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IFile file;

            DisableGridEvents();

            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        file = e.NewItems.OfType<IFile>().FirstOrDefault();

                        if ((file == null) || (file.Parent != DataContext.SelectedDirectory))
                        {
                            break;
                        }

                        Image fileIcon = TreeNodeIcons.Images[1];

                        if (TreeNodeIcons.Images.ContainsKey(file.ImageName))
                        {
                            fileIcon = TreeNodeIcons.Images[file.ImageName];
                        }

                        var row = new DataGridViewRow();
                        row.CreateCells(GridFiles, file.ID, file, fileIcon, file.Name, file.Type, file.SizeInBytes);
                        GridFiles.Rows.Add(row);

                        file.PropertyChanged += File_PropertyChanged;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        file = e.OldItems.OfType<IFile>().FirstOrDefault();

                        if ((file == null) || (file.Parent != DataContext.SelectedDirectory))
                        {
                            break;
                        }

                        DataGridViewRow fileRow = FindFileRow(file);
                        if (fileRow != null)
                        {
                            GridFiles.Rows.Remove(fileRow);
                        }
                        file.PropertyChanged -= File_PropertyChanged;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        if (DataContext.SelectedDirectory.Files != sender)
                        {
                            break;
                        }

                        for (int i = 0; i < GridFiles.Rows.Count; ++i)
                        {
                            var removedFile = (IFile)GridFiles.Rows[i].Cells[ColumnFile.Index].Value;
                            removedFile.PropertyChanged -= File_PropertyChanged;
                        }

                        GridFiles.Rows.Clear();
                        break;
                }
            }
            finally
            {
                EnableGridEvents();
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the CollectionChanged event of the Directories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Directories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DirectoryTreeNode directoryNode = null;
            IDirectory directory = null;
            DirectoryTreeNode[] childNodes = null;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    directory = (IDirectory)e.NewItems[0];

                    if ((_directoryNodes.ContainsKey(directory.ID)) || (!_directoryNodes.TryGetValue(directory.Parent.ID, out directoryNode)))
                    {
                        break;
                    }

                    // If we're adding to a collapsed directory with no children, add a dummy node to indicate that there are children for the parent node.
                    if (!directoryNode.IsExpanded)
                    {
                        if (directoryNode.Nodes.Count == 0)
                        {
                            AddDummyNode(directoryNode.Nodes);
                        }
                        break;
                    }

                    var newNode = new DirectoryTreeNode
                    {
                        Text = directory.Name,
                        Name = directory.ID,
                        ImageKey = directory.ImageName,
                        Tag = directory
                    };

                    if (directory.Directories.Count > 0)
                    {
                        AddDummyNode(newNode.Nodes);
                    }

                    directoryNode.Nodes.Add(newNode);
                    _directoryNodes[directory.ID] = newNode;
                    newNode.SetDataContext(directory);
                    newNode.DataContext.OnLoad();

                    directory.Directories.CollectionChanged += Directories_CollectionChanged;
                    directory.PropertyChanged += Directory_PropertyChanged;
                    
                    break;
                case NotifyCollectionChangedAction.Remove:
                    directory = (IDirectory)e.OldItems[0];

                    if ((directory == DataContext.Root) || (!_directoryNodes.TryGetValue(directory.ID, out directoryNode)))
                    {
                        break;
                    }

                    directory.PropertyChanged -= Directory_PropertyChanged;
                    directory.Directories.CollectionChanged -= Directories_CollectionChanged;
                    directory.Files.CollectionChanged -= Files_CollectionChanged;                    

                    childNodes = _directoryNodes.Where(item => (item.Key.StartsWith(directoryNode.Name, StringComparison.OrdinalIgnoreCase)) && (item.Value.DataContext != null))
                                                .Select(item => item.Value)
                                                .ToArray();

                    foreach (DirectoryTreeNode node in childNodes)
                    {
                        node.DataContext.PropertyChanged -= Directory_PropertyChanged;
                        node.DataContext.Files.CollectionChanged -= Files_CollectionChanged;
                        node.DataContext.Directories.CollectionChanged -= Directories_CollectionChanged;
                        _directoryNodes.Remove(node.Name);
                    }

                    directoryNode.Remove();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    directoryNode = _directoryNodes.Values.FirstOrDefault(d => d.DataContext.Directories == sender);

                    if (directoryNode == null)
                    {
                        break;
                    }

                    childNodes = _directoryNodes.Where(item => (item.Value != directoryNode)  && (item.Value.DataContext != null)
                                                            && (item.Key.StartsWith(directoryNode.Name, StringComparison.OrdinalIgnoreCase)))
                                                .Select(item => item.Value)
                                                .ToArray();

                    foreach (DirectoryTreeNode node in childNodes)
                    {
                        node.DataContext.PropertyChanged -= Directory_PropertyChanged;
                        node.DataContext.Directories.CollectionChanged -= Directories_CollectionChanged;
                        node.DataContext.Files.CollectionChanged -= Files_CollectionChanged;

                        _directoryNodes.Remove(node.Name);
                    }

                    directoryNode.Nodes.Clear();
                    break;
            }

            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the PropertyChanged event of the Clipboard control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Clipboard_PropertyChanged(object sender, PropertyChangedEventArgs e) 
        {
            if (DataContext == null)
            {
                return;
            }

            ValidateMenuItems(DataContext);
        }

        /// <summary>
        /// Handles the PropertyChanging event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorer.SelectedFiles):
                    DataContext.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
                    break;
                case nameof(IFileExplorer.Clipboard):
                    if (DataContext.Clipboard != null)
                    {
                        DataContext.Clipboard.PropertyChanged -= Clipboard_PropertyChanged;
                    }
                    break;
                case nameof(IFileExplorer.SearchResults):
                    if (DataContext.SearchResults != null)
                    {
                        RemoveFileEvents(DataContext.SearchResults);
                    }
                    break;
                case nameof(IFileExplorer.SelectedDirectory):
                    if (DataContext.SelectedDirectory == null)
                    {
                        return;
                    }

                    RemoveFileEvents(DataContext.SelectedDirectory.Files);
                    DataContext.SelectedDirectory.Files.CollectionChanged -= Files_CollectionChanged;
                    break;
            }
        }

        /// <summary>
        /// Function to select rows in the file grid based on selected files in the data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SelectRows(IFileExplorer dataContext)
        {
            DisableGridEvents();
            
            try
            {
                GridFiles.ClearSelection();

                if (dataContext == null)
                {
                    return;
                }

                if (dataContext.SelectedFiles.Count == 0)
                {
                    return;
                }

                foreach (string id in dataContext.SelectedFiles.Select(item => item.ID))
                {
                    // Select the row that contains the file path.
                    for (int rowIndex = 0; rowIndex < GridFiles.Rows.Count; ++rowIndex)
                    {
                        string rowPath = GridFiles.Rows[rowIndex].Cells[ColumnID.Index].Value.IfNull(string.Empty);

                        if (string.Equals(id, rowPath, StringComparison.OrdinalIgnoreCase))
                        {
                            GridFiles.Rows[rowIndex].Selected = true;
                            break;
                        }
                    }
                }
            }
            finally
            {
                EnableGridEvents();
            }
        }        

        /// <summary>
        /// Handles the PropertyChanged event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorer.Clipboard):
                    if (DataContext.Clipboard != null)
                    {
                        DataContext.Clipboard.PropertyChanged += Clipboard_PropertyChanged;
                    }
                    break;
                case nameof(IFileExplorer.SearchResults):
                    if (DataContext.SearchResults != null)
                    {
                        SplitFileSystem.Panel1Collapsed = true;
                        TreeDirectories.Visible = false;
                        FillFiles(DataContext, DataContext.SelectedDirectory, DataContext.SearchResults);
                    }
                    else
                    {
                        SplitFileSystem.Panel1Collapsed = false;
                        TreeDirectories.Visible = true;
                        FillFiles(DataContext, DataContext.SelectedDirectory, DataContext.SelectedDirectory?.Files);
                    }                    
                    break;
                case nameof(IFileExplorer.SelectedFiles):
                    SelectRows(DataContext);
                    DataContext.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
                    break;
                case nameof(IFileExplorer.SelectedDirectory):

                    if (DataContext.SelectedDirectory != null)                             
                    {
                        if (_directoryNodes.TryGetValue(DataContext.SelectedDirectory.ID, out DirectoryTreeNode node))
                        {
                            if (TreeDirectories.SelectedNode != node)
                            {
                                TreeDirectories.SelectedNode = node;
                            }
                        }

                        DataContext.SelectedDirectory.Files.CollectionChanged += Files_CollectionChanged;
                    }

                    FillFiles(DataContext, DataContext.SelectedDirectory, DataContext.SelectedDirectory?.Files);
                    break;
            }
            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the PropertyChanged event of the Directory control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Directory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((!(sender is IDirectory directory)) || (!_directoryNodes.TryGetValue(directory.ID, out DirectoryTreeNode treeNode)))
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(IDirectory.IsCut):
                    if (directory.IsCut)
                    {
                        treeNode.ForeColor = DarkFormsRenderer.CutForeground;
                    }
                    else
                    {
                        treeNode.ForeColor = TreeDirectories.ForeColor;
                    }
                    break;
            }

            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the PropertyChanged event of the File control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var file = (IFile)sender;

            // If we've no directory selected, or the currently selected directory is not the same as the directory for the file, then we have nothing to look up and refresh.
            if ((!(TreeDirectories.SelectedNode is DirectoryTreeNode selectedDir)) || (file.Parent != selectedDir.DataContext))
            {
                return;
            }

            // We'll need to locate the row on the grid containing the file.
            DataGridViewRow row = FindFileRow(file);

            // The file isn't here.
            if (row == null)
            {
                return;
            }

            Image fileIcon;

            switch (e.PropertyName)
            {
                case nameof(IFile.Metadata):
                    fileIcon = TreeNodeIcons.Images[1];

                    if (TreeNodeIcons.Images.ContainsKey(file.ImageName))
                    {
                        fileIcon = TreeNodeIcons.Images[file.ImageName];
                    }

                    row.Cells[ColumnID.Index].Value = file.ID;
                    row.Cells[ColumnIcon.Index].Value = fileIcon;
                    row.Cells[ColumnType.Index].Value = file.Type;
                    row.Cells[ColumnSize.Index].Value = file.SizeInBytes;
                    break;
                case nameof(IFile.Type):
                    row.Cells[ColumnType.Index].Value = file.Type;
                    break;
                case nameof(IFile.IsCut):
                    GridFiles.InvalidateRow(row.Index);
                    break;
                case nameof(IFile.Name):
                    row.Cells[ColumnFilename.Index].Value = file.Name;
                    break;
                case nameof(IFile.Extension):
                    row.Cells[ColumnFilename.Index].Value = file.Extension;
                    break;
                case nameof(IFile.SizeInBytes):
                    row.Cells[ColumnSize.Index].Value = file.SizeInBytes;
                    break;
                case nameof(IFile.ImageName):
                    fileIcon = TreeNodeIcons.Images[1];

                    if (TreeNodeIcons.Images.ContainsKey(file.ImageName))
                    {
                        fileIcon = TreeNodeIcons.Images[file.ImageName];
                    }

                    row.Cells[ColumnIcon.Index].Value = fileIcon;
                    break;
                case nameof(IFile.IsOpen):
                    GridFiles.InvalidateRow(row.Index);
                    break;
            }
            ValidateMenuItems(DataContext);
        }

        /// <summary>
        /// Function to locate the specified file in the grid.
        /// </summary>
        /// <param name="file">The file to locate.</param>
        /// <returns>The cells for the row.</returns>
        private DataGridViewRow FindFileRow(IFile file)
        {
            if (file == null)
            {
                return null;
            }

            for (int i = 0; i < GridFiles.Rows.Count; ++i)
            {
                DataGridViewRow row = GridFiles.Rows[i];
                var gridFile = row.Cells[ColumnFile.Index].Value as IFile;

                if (gridFile == file)
                {
                    return row;
                }
            }

            return null;
        }

        /// <summary>
        /// Function to select a node on the tree.
        /// </summary>
        /// <param name="node">The node to select.</param>
        private void SelectNode(DirectoryTreeNode node)
        {
            if ((DataContext?.SelectDirectoryCommand == null) || (!DataContext.SelectDirectoryCommand.CanExecute(node?.Name ?? string.Empty)))
            {
                return;
            }

            if ((TreeDirectories.SelectedNode != null) && (TreeDirectories.SelectedNode != node))
            {
                var currentNode = (DirectoryTreeNode)TreeDirectories.SelectedNode;
                RemoveFileEvents(currentNode.DataContext.Files);
            }

            if (node == null)
            {
                TreeDirectories.SelectedNode = null;
            }
            else if (TreeDirectories.SelectedNode != node)
            {
                TreeDirectories.SelectedNode = node;
            }

            DataContext.SelectDirectoryCommand.Execute(node?.Name ?? string.Empty);

            FillFiles(DataContext, node?.DataContext, node?.DataContext.Files);

            ValidateMenuItems(DataContext);
        }

        /// <summary>
        /// Function to remove the file property change events from the files in the specified directory.
        /// </summary>
        /// <param name="files">The list of files to update.</param>
        private void RemoveFileEvents(IReadOnlyList<IFile> files)
        {
            if (files == null)
            {
                return;
            }

            for (int i = 0; i < files.Count; ++i)
            {
                files[i].PropertyChanged -= File_PropertyChanged;
            }
        }

        /// <summary>
        /// Function to reset the view back to its original state when the data context is reset.
        /// </summary>
        private void ResetDataContext()
        {
            if (DataContext == null)
            {
                return;
            }

            SelectRows(DataContext);

            if (TreeNodeIcons.Images.Count > 2)
            {
                while (TreeNodeIcons.Images.Count > 2)
                {
                    TreeNodeIcons.Images.RemoveAt(2);
                }
            }

            _directoryNodes.Clear();
            DataContext.OnUnload();

            foreach (DirectoryTreeNode node in TreeDirectories.Nodes.OfType<DirectoryTreeNode>().Traverse(n => n.Nodes.OfType<DirectoryTreeNode>()))
            {
                node.DataContext.PropertyChanged += Directory_PropertyChanged;
                node.DataContext.Directories.CollectionChanged -= Directories_CollectionChanged;
                node.DataContext.Files.CollectionChanged -= Files_CollectionChanged;

                RemoveFileEvents(node.DataContext.Files);

                node.SetDataContext(null);
            }

            GridFiles.Rows.Clear();
            TreeDirectories.Nodes.Clear();            
        }

        /// <summary>
        /// Function unassign events after the data context is unassigned.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            if (DataContext.Clipboard != null)
            {
                DataContext.Clipboard.PropertyChanged -= Clipboard_PropertyChanged;
            }

            DataContext.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;

            if (DataContext.SelectedDirectory != null)
            {
                DataContext.SelectedDirectory.PropertyChanged -= Directory_PropertyChanged;
                DataContext.SelectedDirectory.Directories.CollectionChanged -= Directories_CollectionChanged;
                DataContext.SelectedDirectory.Files.CollectionChanged -= Files_CollectionChanged;
                RemoveFileEvents(DataContext.SelectedDirectory.Files);
            }
                        
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }
        

        /// <summary>
        /// Function to remove a node, and it's child nodes from the node cache.
        /// </summary>
        /// <param name="nodeID">The ID for the node to look up.</param>
        /// <param name="includeSelf"><b>true</b> to remove the path refrerenced by the ID, <b>false</b> to leave it in the cache.</param>
        private void RemoveNodeFromCache(string nodeID, bool includeSelf)
        {
            if (!_directoryNodes.TryGetValue(nodeID, out DirectoryTreeNode dirNode))
            {
                return;
            }

            DirectoryTreeNode[] childNodes = dirNode.Nodes.OfType<DirectoryTreeNode>()
                                                          .Traverse(n => n.Nodes.OfType<DirectoryTreeNode>())
                                                          .ToArray();

            foreach (DirectoryTreeNode treeNode in childNodes)
            {
                if (treeNode.DataContext != null)
                {
                    treeNode.DataContext.PropertyChanged -= Directory_PropertyChanged;
                    treeNode.DataContext.Directories.CollectionChanged -= Directories_CollectionChanged;
                    if (TreeDirectories.SelectedNode == treeNode)
                    {
                        treeNode.DataContext.Files.CollectionChanged -= Files_CollectionChanged;
                    }
                }

                _directoryNodes.Remove(treeNode.Name);
            }

            if ((!includeSelf) || (!_directoryNodes.ContainsKey(nodeID)))
            {
                return;
            }

            if (dirNode.DataContext != null)
            {
                dirNode.DataContext.PropertyChanged -= Directory_PropertyChanged;
                dirNode.DataContext.Directories.CollectionChanged -= Directories_CollectionChanged;
                if (TreeDirectories.SelectedNode == dirNode)
                {
                    dirNode.DataContext.Files.CollectionChanged -= Files_CollectionChanged;
                }
            }

            _directoryNodes.Remove(nodeID);
        }

        /// <summary>
        /// Function to disable any events on the grid prior to updating.
        /// </summary>
        private void DisableGridEvents()
        {
            GridFiles.MouseDown -= GridFiles_MouseDown;
            GridFiles.CellValidating -= GridFiles_CellValidating;
            GridFiles.KeyUp -= GridFiles_KeyUp;
            GridFiles.KeyDown -= GridFiles_KeyDown;
            GridFiles.SelectionChanged -= GridFiles_SelectionChanged;
            Interlocked.Exchange(ref _gridEventsHooked, 0);
        }

        /// <summary>
        /// Function to enable any events on the grid prior to updating.
        /// </summary>
        private void EnableGridEvents()
        {
            if (Interlocked.Exchange(ref _gridEventsHooked, 1) == 1)
            {
                return;
            }

            GridFiles.SelectionChanged += GridFiles_SelectionChanged;
            GridFiles.KeyDown += GridFiles_KeyDown;
            GridFiles.KeyUp += GridFiles_KeyUp;
            GridFiles.CellValidating += GridFiles_CellValidating;
            GridFiles.MouseDown += GridFiles_MouseDown;
        }        

        /// <summary>
        /// Function to disable any events on the tree before updating.
        /// </summary>
        private void DisableTreeEvents()
        {
            TreeDirectories.AfterLabelEdit -= TreeDirectories_AfterLabelEdit;
            TreeDirectories.BeforeLabelEdit -= TreeDirectories_BeforeLabelEdit;
            TreeDirectories.AfterExpand -= TreeDirectories_AfterExpand;
            TreeDirectories.AfterCollapse -= TreeDirectories_AfterCollapse;
            TreeDirectories.BeforeExpand -= TreeDirectories_BeforeExpand;
            TreeDirectories.AfterSelect -= TreeDirectories_AfterSelect;

            Interlocked.Exchange(ref _treeEventsHooked, 0);
        }

        /// <summary>
        /// Function to enable events on the tree after updating.
        /// </summary>
        private void EnableTreeEvents()
        {
            // Do this to prevent over-subscription.
            if (Interlocked.Exchange(ref _treeEventsHooked, 1) == 1)
            {
                return;
            }

            TreeDirectories.AfterSelect += TreeDirectories_AfterSelect;
            TreeDirectories.BeforeExpand += TreeDirectories_BeforeExpand;
            TreeDirectories.AfterCollapse += TreeDirectories_AfterCollapse;
            TreeDirectories.AfterExpand += TreeDirectories_AfterExpand;
            TreeDirectories.BeforeLabelEdit += TreeDirectories_BeforeLabelEdit;
            TreeDirectories.AfterLabelEdit += TreeDirectories_AfterLabelEdit;
        }

        /// <summary>Handles the MouseDown event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void GridFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if ((DataContext == null) || (GridFiles.IsCurrentCellInEditMode))
            {
                return;
            }

            try
            {
                GridFiles.Focus();

                DataGridView.HitTestInfo hit = GridFiles.HitTest(e.X, e.Y);

                if ((hit.RowIndex < 0) || (hit.RowIndex >= GridFiles.Rows.Count) || (hit.ColumnIndex < 0) || (hit.ColumnIndex >= GridFiles.ColumnCount))
                {
                    GridFiles.CurrentCell = null;
                    GridFiles.ClearSelection();
                    return;
                }

                if (e.Button != MouseButtons.Right)
                {
                    return;
                }

                DataGridViewRow row = GridFiles.Rows[hit.RowIndex];
                if (row.Selected)
                {
                    if (GridFiles.CurrentCell != row.Cells[ColumnFilename.Index])
                    {
                        GridFiles.CurrentCell = row.Cells[ColumnFilename.Index];
                    }
                    return;
                }

                GridFiles.ClearSelection();
                row.Selected = true;
                GridFiles.CurrentCell = row.Cells[ColumnFilename.Index];
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the KeyDown event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void GridFiles_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.F2:
                case Keys.Delete:
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        /// <summary>Handles the KeyUp event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private async void GridFiles_KeyUp(object sender, KeyEventArgs e)
        {
            DisableGridEvents();

            try
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case Keys.F2:
                        if ((DataContext?.RenameFileCommand == null) || (!DataContext.RenameFileCommand.CanExecute(null)))
                        {
                            return;
                        }

                        if (GridFiles.CurrentRow != null)
                        {
                            DataGridViewCell currentCell = GridFiles.Rows[GridFiles.CurrentRow.Index].Cells[ColumnFilename.Index];
                            if (GridFiles.CurrentCell != currentCell)
                            {
                                GridFiles.CurrentCell = currentCell;
                            }
                        }

                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        GridFiles.BeginEdit(true);
                        IsRenaming = true;
                        break;
                    case Keys.Delete:
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        await DeleteSelectedFilesAsync();
                        break;
                }
            }
            finally
            {
                EnableGridEvents();
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the CellEndEdit event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void GridFiles_CellEndEdit(object sender, DataGridViewCellEventArgs e) => IsRenaming = false;

        /// <summary>Handles the CellValidating event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellValidatingEventArgs"/> instance containing the event data.</param>
        private void GridFiles_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                if ((DataContext?.RenameFileCommand == null) || (e.ColumnIndex != ColumnFilename.Index))
                {
                    return;
                }

                string newName = e.FormattedValue?.ToString() ?? string.Empty;
                var file = (IFile)GridFiles.Rows[e.RowIndex].Cells[ColumnFile.Index].Value;
                var args = new RenameArgs(file.Name, newName);

                if (!DataContext.RenameFileCommand.CanExecute(args))
                {
                    GridFiles.CancelEdit();
                    return;
                }

                DataContext.RenameFileCommand.Execute(args);

                if (args.Cancel)
                {
                    GridFiles.CancelEdit();
                }
            }
            finally
            {
                IsRenaming = false;
            }
        }

        /// <summary>Handles the SelectionChanged event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void GridFiles_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Ugh.  Of course, the selected rows are not sorted by the current sorting in the grid.
                // We have to make it match. Lovely.
                string[] ids = GridFiles.SelectedRows.OfType<DataGridViewRow>()
                                                     .Where(item => !item.Cells[ColumnID.Index].Value.IsNull())
                                                     .Reverse()
                                                     .Select(item => item.Cells[ColumnID.Index].Value.ToString())                                                     
                                                     .ToArray();

                if ((DataContext?.SelectFileCommand == null) || (!DataContext.SelectFileCommand.CanExecute(ids)))
                {
                    return;
                }

                DataContext.SelectFileCommand.Execute(ids);
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the CellFormatting event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellFormattingEventArgs"/> instance containing the event data.</param>
        private void GridFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = GridFiles.Rows[e.RowIndex];

            if (!(row.Cells[ColumnFile.Index].Value is IFile file))
            {
                return;
            }

            if (file.IsCut)
            {
                e.CellStyle = _cutCellStyle;
            }

            // For the size column, format as standard memory/file sizes.
            if (e.ColumnIndex == ColumnSize.Index)
            {
                e.Value = ((long)e.Value).FormatMemory();
                return;
            }

            if ((e.ColumnIndex != ColumnFilename.Index) || (file.IsCut))
            {
                return;
            }

            row.Cells[0].ToolTipText = row.Cells[1].ToolTipText = file.FullPath;                    
            e.CellStyle = GridFiles.DefaultCellStyle;
           
            if (!file.IsOpen)
            {
                if (file.Metadata?.ContentMetadata == null)
                {
                    e.CellStyle = _unknownFileStyle;
                }
            }
            else
            {
                e.CellStyle = _openFileStyle;
            }
        }

        /// <summary>Handles the RowsDrag event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowsDragEventArgs"/> instance containing the event data.</param>
        private void GridFiles_RowsDrag(object sender, RowsDragEventArgs e)
        {
            if (DataContext?.SearchResults != null)
            {
                return;
            }

            var dragData = new GridRowsDragData(e.DraggedRows, ColumnFile.Index, e.MouseButtons == MouseButtons.Right ? (CopyMoveOperation.Copy | CopyMoveOperation.Move) : CopyMoveOperation.Move);
            var data = new DataObject();            
            data.SetData(dragData);
            data.SetData(typeof(IContentFileDragData).FullName, true, dragData as IContentFileDragData);

            GridFiles.DoDragDrop(data, DragDropEffects.Move | DragDropEffects.Copy);

            if (dragData.TargetNode != null)
            {
                dragData.TargetNode.ForeColor = Color.Empty;
                dragData.TargetNode.BackColor = Color.Empty;
            }
        }

        /// <summary>Handles the DragEnter event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void GridFiles_DragEnter(object sender, DragEventArgs e)
        {
            if ((DataContext?.SelectedDirectory == null) || (!e.Data.GetDataPresent(DataFormats.FileDrop, false)))
            {
                return;
            }

            if (!_directoryNodes.TryGetValue(DataContext.SelectedDirectory.ID, out DirectoryTreeNode node))
            {
                return;
            }

            _explorerImportData = new ExplorerImportData(e.Data.GetData(DataFormats.FileDrop, false) as string[])
            {
                TargetNode = node
            };

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>Handles the DragDrop event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void GridFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (e.Data.GetDataPresent(typeof(GridRowsDragData)))
            {
                var data = e.Data.GetData(typeof(GridRowsDragData)) as GridRowsDragData;
                data.TargetNode = (DirectoryTreeNode)TreeDirectories.SelectedNode;
                data.Operation = CopyMoveOperation.Copy;
                FileRows_DragDrop(data, e);
                return;
            }

            if ((_explorerImportData == null) || (!e.Data.GetDataPresent(DataFormats.FileDrop, false)))
            {
                return;
            }

            Explorer_DragDropDirectory(e);
        }

        /// <summary>Handles the DragOver event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void GridFiles_DragOver(object sender, DragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (e.Data.GetDataPresent(typeof(GridRowsDragData)))
            {
                e.Effect = DragDropEffects.Copy;
                ValidateMenuItems(DataContext);
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.Copy;
                return;
            }

            e.Effect = DragDropEffects.None;
        }

        /// <summary>Handles the Enter event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void GridFiles_Enter(object sender, EventArgs e)
        {
            ControlContext = FileExplorerContext.FileList;
            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the Leave event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void GridFiles_Leave(object sender, EventArgs e) => ValidateMenuItems(DataContext);

        /// <summary>Handles the CellDoubleClick event of the GridFiles control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void GridFiles_CellDoubleClick(object sender, DataGridViewCellEventArgs e) => MenuItemOpen.PerformClick();

        /// <summary>Handles the EditCanceled event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_EditCanceled(object sender, EventArgs e) => IsRenaming = false;

        /// <summary>Handles the AfterLabelEdit event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            DisableTreeEvents();

            try
            {
                var node = e.Node as DirectoryTreeNode;

                string nodeID = node.Name;
                var args = new RenameArgs(node.Text, e.Label);
                DataContext?.RenameDirectoryCommand.Execute(args);

                if (args.Cancel)
                {
                    e.CancelEdit = true;
                }                
            }
            finally
            {
                IsRenaming = false;
                EnableTreeEvents();
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the BeforeLabelEdit event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            DisableTreeEvents();

            try
            {
                var node = e.Node as DirectoryTreeNode;

                var args = new RenameArgs(e.Label, node.Text);
                if ((DataContext?.RenameDirectoryCommand == null) || (!DataContext.RenameDirectoryCommand.CanExecute(args)))
                {
                    e.CancelEdit = true;
                    IsRenaming = false;
                }
                else
                {
                    IsRenaming = true;
                }
            }
            finally
            {
                EnableTreeEvents();
            }
        }

        /// <summary>Handles the AfterSelect event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string id = e.Node?.Name ?? string.Empty;

            try
            {
                if ((DataContext?.SelectDirectoryCommand == null) || (!DataContext.SelectDirectoryCommand.CanExecute(id)))
                {
                    return;
                }

                DataContext.SelectDirectoryCommand.Execute(id);
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the AfterCollapse event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node is DirectoryTreeNode node))
            {
                return;
            }

            DisableTreeEvents();

            try
            {
                IDirectory directory = node.DataContext;

                if (directory == null)
                {
                    return;
                }

                RemoveNodeFromCache(node.Name, false);
                node.Nodes.Clear();

                if (directory.Directories.Count > 0)
                {
                    AddDummyNode(node.Nodes);
                }
            }
            finally
            {
                EnableTreeEvents();
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the AfterExpand event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node is DirectoryTreeNode node))
            {
                return;
            }

            // Our refresh method clears the selection, so force it to select the expanded node.
            SelectNode(node);
        }

        /// <summary>Handles the BeforeExpand event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (!(e.Node is DirectoryTreeNode node))
            {
                return;
            }

            DisableTreeEvents();

            try
            {
                IDirectory directory = node.DataContext;

                if (directory == null)
                {
                    return;
                }                
                
                RefreshNodes(directory);
            }
            finally
            {
                ValidateMenuItems(DataContext);
                EnableTreeEvents();
            }
        }

        /// <summary>Handles the KeyUp event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private async void TreeDirectories_KeyUp(object sender, KeyEventArgs e)
        {
            if ((DataContext == null) || (TreeDirectories.SelectedNode == null))
            {
                return;
            }

            DisableTreeEvents();

            try
            {
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        await DeleteDirectoryAsync();
                        break;
                }
            }
            finally
            {
                EnableTreeEvents();
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>Handles the ItemDrag event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemDragEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!(e.Item is DirectoryTreeNode node))
            {
                return;
            }

            var dragData = new TreeNodeDragData(node, e.Button == MouseButtons.Right ? (CopyMoveOperation.Copy | CopyMoveOperation.Move) : CopyMoveOperation.Move);
            SelectNode(node);
            var data = new DataObject();
            data.SetData(dragData);

            TreeDirectories.DoDragDrop(data, DragDropEffects.Move | DragDropEffects.Copy);

            if (dragData.TargetNode != null)
            {
                dragData.TargetNode.ForeColor = Color.Empty;
                dragData.TargetNode.BackColor = Color.Empty;
            }
        }

        /// <summary>Handles the DragLeave event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_DragLeave(object sender, EventArgs e)
        {
            if (_explorerImportData?.TargetNode == null)
            {
                return;
            }

            _explorerImportData.TargetNode.ForeColor = Color.Empty;
            _explorerImportData.TargetNode.BackColor = Color.Empty;
        }

        /// <summary>
        /// Function to handle dropping a dragged directory node.
        /// </summary>
        /// <param name="data">The data being dragged.</param>
        /// <param name="e">The event arguments.</param>
        private async void DirectoryNodes_DragDrop(TreeNodeDragData data, DragEventArgs e)
        {
            if ((data == null) 
                || (e.Effect == DragDropEffects.None)
                || (DataContext == null))
            {
                return;
            }

            try
            {
                DirectoryTreeNode destNode = data.TargetNode;

                destNode.ForeColor = Color.Empty;
                destNode.BackColor = Color.Empty;

                SelectNode(destNode);

                switch (data.Operation)
                {
                    case CopyMoveOperation.Copy:
                        if ((DataContext.CopyDirectoryCommand != null) && (DataContext.CopyDirectoryCommand.CanExecute(data)))
                        {
                            await DataContext.CopyDirectoryCommand.ExecuteAsync(data);
                        }
                        break;
                    case CopyMoveOperation.Move:
                        if ((DataContext.MoveDirectoryCommand != null) && (DataContext.MoveDirectoryCommand.CanExecute(data)))
                        {
                            await DataContext.MoveDirectoryCommand.ExecuteAsync(data);
                        }
                        break;
                    default:
                        // If both operations are defined, then bring up a menu.
                        if (data.Operation == (CopyMoveOperation.Copy | CopyMoveOperation.Move))
                        {
                            MenuCopyMove.Tag = data;
                            MenuCopyMove.Show(this, PointToClient(Cursor.Position));
                            break;
                        }
                        break;
                }
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>
        /// Function to handle dragging a directory node over other directory nodes.
        /// </summary>
        /// <param name="data">The data being dragged.</param>
        /// <param name="e">The event arguments.</param>
        private void DirectoryNodes_DragOver(TreeNodeDragData data, DragEventArgs e)
        {
            if ((data == null) || (DataContext?.CopyDirectoryCommand == null) || (DataContext?.MoveDirectoryCommand == null))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            DirectoryTreeNode prevTargetNode = data.TargetNode;
            TreeViewHitTestInfo hitResult = TreeDirectories.HitTest(TreeDirectories.PointToClient(new Point(e.X, e.Y)));
            data.TargetNode = (DirectoryTreeNode)hitResult.Node;

            if (hitResult.Node == null)
            {
                data.TargetNode = _rootNode;
            }

            if (data.TargetNode != prevTargetNode)
            {
                if (prevTargetNode != null)
                {
                    prevTargetNode.ForeColor = Color.Empty;
                    prevTargetNode.BackColor = Color.Empty;
                }

                data.TargetNode.ForeColor = DarkFormsRenderer.FocusedForeground;
                data.TargetNode.BackColor = DarkFormsRenderer.FocusedBackground;                
            }

            if (((e.KeyState & 8) == 8) && (data.Operation == CopyMoveOperation.Move))
            {
                data.Operation = CopyMoveOperation.Copy;
            }
            else if (((e.KeyState & 8) != 8) && (data.Operation == CopyMoveOperation.Copy))
            {
                data.Operation = CopyMoveOperation.Move;
            }

            if ((data.Operation & CopyMoveOperation.Copy) == CopyMoveOperation.Copy)
            {
                if (!DataContext.CopyDirectoryCommand.CanExecute(data))
                {
                    e.Effect = DragDropEffects.None;
                    MenuItemCopyTo.Available = false;
                }
                else
                {
                    e.Effect = DragDropEffects.Copy;
                    MenuItemCopyTo.Available = true;
                }
            }

            if ((data.Operation & CopyMoveOperation.Move) == CopyMoveOperation.Move)
            {
                if (!DataContext.MoveDirectoryCommand.CanExecute(data))
                {
                    e.Effect = DragDropEffects.None;
                    MenuItemMoveTo.Available = false;
                }
                else
                {
                    e.Effect = DragDropEffects.Move;
                    MenuItemMoveTo.Available = true;
                }
            }
        }

        /// <summary>
        /// Function to handle dropping of explorer files/directories on a virtual directory.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        private async void Explorer_DragDropDirectory(DragEventArgs e)
        {
            try
            {
                if ((_explorerImportData == null) || (DataContext?.ImportCommand == null))
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                _explorerImportData.TargetNode.ForeColor = Color.Empty;
                _explorerImportData.TargetNode.BackColor = Color.Empty;

                SelectNode(_explorerImportData.TargetNode);

                await DataContext.ImportCommand.ExecuteAsync(_explorerImportData);
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>
        /// Function to handle dragging explorer file/directory paths over a directory node.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        private void Explorer_DragOverDirectory(DragEventArgs e)
        {
            if ((_explorerImportData == null) || (DataContext?.ImportCommand == null))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            DirectoryTreeNode prevTargetNode = _explorerImportData.TargetNode;
            TreeViewHitTestInfo hitResult = TreeDirectories.HitTest(TreeDirectories.PointToClient(new Point(e.X, e.Y)));

            _explorerImportData.TargetNode = (hitResult.Node as DirectoryTreeNode) ?? _rootNode;

            if (_explorerImportData.TargetNode != prevTargetNode)
            {
                if (prevTargetNode != null)
                {
                    prevTargetNode.ForeColor = Color.Empty;
                    prevTargetNode.BackColor = Color.Empty;
                }

                _explorerImportData.TargetNode.ForeColor = DarkFormsRenderer.FocusedForeground;
                _explorerImportData.TargetNode.BackColor = DarkFormsRenderer.FocusedBackground;
            }

            if (!DataContext.ImportCommand.CanExecute(_explorerImportData))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>Handles the Enter event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_Enter(object sender, EventArgs e)
        {
            ControlContext = FileExplorerContext.DirectoryTree;
            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the Leave event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_Leave(object sender, EventArgs e) => ValidateMenuItems(DataContext);

        /// <summary>
        /// Function to handle dropping dragged file rows on the file grid.
        /// </summary>
        /// <param name="data">The data being dragged.</param>
        /// <param name="e">The event arguments.</param>
        private async void FileRows_DragDrop(GridRowsDragData data, DragEventArgs e)
        {
            if ((data == null)
                || (e.Effect == DragDropEffects.None)
                || (DataContext == null))
            {
                return;
            }

            try
            {
                switch (data.Operation)
                {
                    case CopyMoveOperation.Copy:
                        if ((DataContext.CopyFileCommand != null) && (DataContext.CopyFileCommand.CanExecute(data)))
                        {
                            await DataContext.CopyFileCommand.ExecuteAsync(data);
                        }
                        break;
                    case CopyMoveOperation.Move:
                        if ((DataContext.MoveFileCommand != null) && (DataContext.MoveFileCommand.CanExecute(data)))
                        {
                            await DataContext.MoveFileCommand.ExecuteAsync(data);
                        }
                        break;
                    default:
                        // If both operations are defined, then bring up a menu.
                        if (data.Operation == (CopyMoveOperation.Copy | CopyMoveOperation.Move))
                        {
                            MenuCopyMove.Tag = data;
                            MenuCopyMove.Show(this, PointToClient(Cursor.Position));
                            return;
                        }
                        break;
                }
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>
        /// Function to handle dropping dragged file rows on the directory tree.
        /// </summary>
        /// <param name="data">The data being dragged.</param>
        /// <param name="e">The event arguments.</param>
        private async void FileRows_ToTree_DragDrop(GridRowsDragData data, DragEventArgs e)
        {
            if ((data == null)
                || (e.Effect == DragDropEffects.None)
                || (DataContext == null))
            {
                return;
            }

            try
            {
                DirectoryTreeNode destNode = data.TargetNode;

                destNode.ForeColor = Color.Empty;
                destNode.BackColor = Color.Empty;

                SelectNode(destNode);

                switch (data.Operation)
                {
                    case CopyMoveOperation.Copy:
                        if ((DataContext.CopyFileCommand != null) && (DataContext.CopyFileCommand.CanExecute(data)))
                        {
                            await DataContext.CopyFileCommand.ExecuteAsync(data);
                        }
                        break;
                    case CopyMoveOperation.Move:
                        if ((DataContext.MoveFileCommand != null) && (DataContext.MoveFileCommand.CanExecute(data)))
                        {
                            await DataContext.MoveFileCommand.ExecuteAsync(data);
                        }
                        break;
                    default:
                        // If both operations are defined, then bring up a menu.
                        if (data.Operation == (CopyMoveOperation.Copy | CopyMoveOperation.Move))
                        {
                            MenuCopyMove.Tag = data;
                            MenuCopyMove.Show(this, PointToClient(Cursor.Position));
                            return;
                        }
                        break;
                }
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>
        /// Function to handle dragging file grid rows over directory nodes.
        /// </summary>
        /// <param name="rowData">The grid rows being dragged.</param>
        /// <param name="e">The event arguments.</param>
        private void FileRows_ToTree_DragOver(GridRowsDragData rowData, DragEventArgs e)
        {
            if ((rowData == null) || (DataContext?.CopyFileCommand == null) || (DataContext?.MoveFileCommand == null))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            DirectoryTreeNode prevTargetNode = rowData.TargetNode;
            TreeViewHitTestInfo hitResult = TreeDirectories.HitTest(TreeDirectories.PointToClient(new Point(e.X, e.Y)));
            rowData.TargetNode = (DirectoryTreeNode)hitResult.Node;

            if (hitResult.Node == null)
            {
                rowData.TargetNode = _rootNode;
            }

            if (rowData.TargetNode != prevTargetNode)
            {
                if (prevTargetNode != null)
                {
                    prevTargetNode.ForeColor = Color.Empty;
                    prevTargetNode.BackColor = Color.Empty;
                }

                rowData.TargetNode.ForeColor = DarkFormsRenderer.FocusedForeground;
                rowData.TargetNode.BackColor = DarkFormsRenderer.FocusedBackground;
            }

            if (((e.KeyState & 8) == 8) && (rowData.Operation == CopyMoveOperation.Move))
            {
                rowData.Operation = CopyMoveOperation.Copy;
            }
            else if (((e.KeyState & 8) != 8) && (rowData.Operation == CopyMoveOperation.Copy))
            {
                rowData.Operation = CopyMoveOperation.Move;
            }

            if ((rowData.Operation & CopyMoveOperation.Copy) == CopyMoveOperation.Copy)
            {
                e.Effect = DragDropEffects.Copy;
                
                if (!DataContext.CopyFileCommand.CanExecute(rowData))
                {
                    e.Effect = DragDropEffects.None;
                    MenuItemCopyTo.Available = false;
                }
                else
                {
                    e.Effect = DragDropEffects.Copy;
                    MenuItemCopyTo.Available = true;
                }                
            }

            if ((rowData.Operation & CopyMoveOperation.Move) == CopyMoveOperation.Move)
            {
                e.Effect = DragDropEffects.Move;

                if (!DataContext.MoveFileCommand.CanExecute(rowData))
                {
                    e.Effect = DragDropEffects.None;
                    MenuItemMoveTo.Available = false;
                }
                else
                {
                    e.Effect = DragDropEffects.Move;
                    MenuItemMoveTo.Available = true;
                }
            }
        }

        /// <summary>Handles the DragOver event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_DragOver(object sender, DragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (e.Data.GetDataPresent(typeof(TreeNodeDragData)))
            {
                DirectoryNodes_DragOver(e.Data.GetData(typeof(TreeNodeDragData)) as TreeNodeDragData, e);
                return;
            }

            if (e.Data.GetDataPresent(typeof(GridRowsDragData)))
            {
                FileRows_ToTree_DragOver(e.Data.GetData(typeof(GridRowsDragData)) as GridRowsDragData, e);
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                Explorer_DragOverDirectory(e);
                return;
            }

            e.Effect = DragDropEffects.None;
        }

        /// <summary>Handles the DragEnter event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_DragEnter(object sender, DragEventArgs e)
        {
            if ((DataContext == null) || (!e.Data.GetDataPresent(DataFormats.FileDrop, false)))
            {
                return;
            }

            _explorerImportData = new ExplorerImportData(e.Data.GetData(DataFormats.FileDrop, false) as string[]);

            TreeViewHitTestInfo hitResult = TreeDirectories.HitTest(TreeDirectories.PointToClient(new Point(e.X, e.Y)));
            DirectoryTreeNode targetTreeNode = (hitResult.Node as DirectoryTreeNode) ?? _rootNode;

            if (targetTreeNode != null)
            {
                targetTreeNode.ForeColor = DarkFormsRenderer.FocusedForeground;
                targetTreeNode.BackColor = DarkFormsRenderer.FocusedBackground;
            }

            _explorerImportData.TargetNode = targetTreeNode;

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>Handles the DragDrop event of the TreeDirectories control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TreeDirectories_DragDrop(object sender, DragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (e.Data.GetDataPresent(typeof(TreeNodeDragData)))
            {
                DirectoryNodes_DragDrop(e.Data.GetData(typeof(TreeNodeDragData)) as TreeNodeDragData, e);
                return;
            }

            if (e.Data.GetDataPresent(typeof(GridRowsDragData)))
            {
                FileRows_ToTree_DragDrop(e.Data.GetData(typeof(GridRowsDragData)) as GridRowsDragData, e);
                return;
            }

            if ((_explorerImportData == null) || (!e.Data.GetDataPresent(DataFormats.FileDrop, false)))
            {
                return;
            }

            Explorer_DragDropDirectory(e);
        }

        /// <summary>
        /// Function to send the search command to the view model.
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        private void SendSearchCommand(string searchText)
        {
            if ((DataContext?.SearchCommand == null) || (!DataContext.SearchCommand.CanExecute(searchText)))
            {
                ValidateMenuItems(DataContext);
                return;
            }

            DataContext.SearchCommand.Execute(searchText);
            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the Enter event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextSearch_Enter(object sender, EventArgs e)
        {
            ControlContext = FileExplorerContext.None;
            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the KeyUp event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void TextSearch_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    SendSearchCommand(TextSearch.Text);
                    break;
                case Keys.Escape:
                    SendSearchCommand(null);
                    break;
            }
        }

        /// <summary>Handles the Search event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:Gorgon.UI.GorgonSearchEventArgs"/> instance containing the event data.</param>
        private void TextSearch_Search(object sender, GorgonSearchEventArgs e) => SendSearchCommand(e.SearchText);
        
        /// <summary>
        /// Function to delete the directory specified by the path.
        /// </summary>
        private async Task DeleteDirectoryAsync()
        {
            if (DataContext == null)
            {
                return;
            }

            try
            {
                var args = new DeleteArgs(null);
                if (DataContext?.DeleteDirectoryCommand != null)
                {
                    if (!DataContext.DeleteDirectoryCommand.CanExecute(args))
                    {
                        return;
                    }

                    await DataContext.DeleteDirectoryCommand.ExecuteAsync(args);
                }

                if (!args.ItemsDeleted)
                {
                    return;
                }

                if (TreeDirectories.SelectedNode is DirectoryTreeNode selectedNode)
                {
                    FillFiles(DataContext, selectedNode.DataContext, selectedNode.DataContext.Files);
                }
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>
        /// Function to add a dummy node for a parent node.
        /// </summary>
        /// <param name="parentNodes">The node collection that will contain the the dummy node.</param>
        private void AddDummyNode(TreeNodeCollection parentNodes)
        {
            parentNodes.Clear();
            parentNodes.Add(new TreeNode(DummyNodeName)
            {
                Name = DummyNodeName
            });
        }

        /// <summary>
        /// Function to fill the file list using the current directory.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        /// <param name="directory">The directory to fill.</param>
        /// <param name="files">The files to populate to the grid.</param>
        private void FillFiles(IFileExplorer dataContext, IDirectory directory, IReadOnlyList<IFile> files)
        {
            var selectedRows = new List<DataGridViewRow>();
            DisableGridEvents();
            try
            {
                DataGridViewColumn sortColumn = GridFiles.SortedColumn;
                SortOrder sortOrder = GridFiles.SortOrder;

                GridFiles.Rows.Clear();

                if ((directory == null) || (files == null) || (files.Count == 0))
                {
                    return;
                }

                var rows = new DataGridViewRow[files.Count];

                RemoveFileEvents(files);
                                
                for (int i = 0; i < files.Count; ++i)
                {
                    IFile file = files[i];

                    Image fileIcon = TreeNodeIcons.Images[1];

                    if (TreeNodeIcons.Images.ContainsKey(file.ImageName))
                    {
                        fileIcon = TreeNodeIcons.Images[file.ImageName];
                    }                    
                    
                    var row = new DataGridViewRow();
                    row.CreateCells(GridFiles, file.ID, file, fileIcon, file.Name, file.Type, file.SizeInBytes, file.Parent.FullPath);
                    rows[i] = row;
                    
                    if (dataContext.SelectedFiles.Any(item => string.Equals(item.FullPath, file.FullPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        selectedRows.Add(row);
                    }

                    file.PropertyChanged += File_PropertyChanged;
                }

                GridFiles.Rows.AddRange(rows);

                GridFiles.ClearSelection();
                foreach (DataGridViewRow selected in selectedRows)
                {
                    selected.Selected = true;
                }

                if (sortColumn != null)
                {
                    GridFiles.Sort(sortColumn, sortOrder == SortOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                }

                ColumnPath.Visible = dataContext?.SearchResults != null;
                GridFiles.MultiSelect = dataContext?.SearchResults == null;
            }
            finally
            {
                EnableGridEvents();
                ValidateMenuItems(dataContext);
            }
        }

        /// <summary>
        /// Function to refresh all the tree nodes under the specified directory, and the specified directory itself.
        /// </summary>
        /// <param name="parentDirectory"></param>
        private void RefreshNodes(IDirectory parentDirectory)
        {
            // If we cannot find the parent node, then there's nothing to refresh.
            if (!_directoryNodes.TryGetValue(parentDirectory.ID, out DirectoryTreeNode parentNode))
            {
                return;
            }

            DisableTreeEvents();            

            TreeDirectories.BeginUpdate();

            try
            {
                SelectNode(null);

                parentDirectory.PropertyChanged -= Directory_PropertyChanged;
                parentDirectory.Directories.CollectionChanged -= Directories_CollectionChanged;

                // First, remove all nodes under the current node from the cache.
                if (_directoryNodes.Count > 0)
                {
                    RemoveNodeFromCache(parentDirectory.ID, false);
                }

                // Second, remove the view models from the existing nodes.
                foreach (DirectoryTreeNode subDirNode in parentNode.Nodes.OfType<DirectoryTreeNode>().Traverse(n => n.Nodes.OfType<DirectoryTreeNode>()))
                {
                    RemoveFileEvents(subDirNode.DataContext.Files);

                    subDirNode.DataContext.Directories.CollectionChanged -= Directories_CollectionChanged;
                    subDirNode.DataContext.PropertyChanged -= Directory_PropertyChanged;
                    subDirNode.SetDataContext(null);                    
                }                

                // Third, clear the nodes from the tree.
                parentNode.Nodes.Clear();

                // Finally, add directory nodes.
                foreach (IDirectory subDir in parentDirectory.Directories)
                {
                    var node = new DirectoryTreeNode
                    {
                        Text = subDir.Name,
                        Name = subDir.ID,
                        ImageKey = subDir.ImageName,
                        Tag = subDir
                    };
                    
                    if (subDir.Directories.Count > 0)
                    {
                        AddDummyNode(node.Nodes);
                    }

                    parentNode.Nodes.Add(node);
                    _directoryNodes[subDir.ID] = node;
                    node.SetDataContext(subDir);
                    node.DataContext.OnLoad();

                    subDir.Directories.CollectionChanged += Directories_CollectionChanged;
                    subDir.PropertyChanged += Directory_PropertyChanged;
                }

                // If this is a root node, then ensure it gets tacked on the tree if it's not already in there.
                if ((parentDirectory.Parent == null) && (!TreeDirectories.Nodes.ContainsKey(parentDirectory.ID)))
                {
                    // Root nodes should stay expanded.
                    parentNode.Expand();
                    TreeDirectories.Nodes.Add(parentNode);
                }                
            }
            finally
            {
                parentDirectory.Directories.CollectionChanged += Directories_CollectionChanged;
                parentDirectory.PropertyChanged += Directory_PropertyChanged;
                TreeDirectories.EndUpdate();
                EnableTreeEvents();                
            }
        }

        /// <summary>
        /// Function to load the icons used in the tree.
        /// </summary>
        /// <param name="images">The list of images to load.</param>
        private void LoadTreeNodeIcons(IReadOnlyDictionary<Guid, Image> images)
        {
            foreach (KeyValuePair<Guid, Image> image in images)
            {
                string key = image.Key.ToString("N");

                if (TreeNodeIcons.Images.ContainsKey(key))
                {
                    continue;
                }

                TreeNodeIcons.Images.Add(key, image.Value);
            }
        }

        /// <summary>
        /// Function to initialize the view with the data context.
        /// </summary>
        /// <param name="dataContext">The data context to initialize with.</param>
        private void InitializeFromDataContext(IFileExplorer dataContext)
        {
            try
            {
                ResetDataContext();

                if (dataContext == null)
                {                    
                    return;
                }

                // Always remove any dummy nodes.
                TreeDirectories.Nodes.Clear();

                // Refresh all nodes including, and under the parent.
                _rootNode = new DirectoryTreeNode
                {
                    Text = dataContext.Root.Name,
                    Name = dataContext.Root.ID,
                    ImageKey = dataContext.Root.ImageName,
                    Tag = dataContext.Root
                };
                _rootNode.SetDataContext(dataContext.Root);
                _rootNode.DataContext.OnLoad();
                _directoryNodes[dataContext.Root.ID] = _rootNode;

                LoadTreeNodeIcons(dataContext.PlugInMetadata.ToDictionary(k => k.SmallIconID, v => v.GetSmallIcon()));
                RefreshNodes(dataContext.Root);
                FillFiles(dataContext, dataContext.Root, dataContext.Root.Files);

                DirectoryTreeNode selectedNode = null;
                if (dataContext.SelectedDirectory != null)
                {
                    dataContext.SelectedDirectory.Files.CollectionChanged += Files_CollectionChanged;
                    _directoryNodes.TryGetValue(dataContext.SelectedDirectory.ID, out selectedNode);
                }
                TreeDirectories.SelectedNode = selectedNode;
                SelectRows(dataContext);                
            }
            finally
            {
                ValidateMenuItems(dataContext);
            }
        }

        /// <summary>Raises the <see cref="UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }

            DataContext?.OnLoad();

            ControlContext = FileExplorerContext.FileList;
            if (GridFiles.Rows.Count > 0)
            {
                GridFiles.Select();
            }
            else
            {
                TreeDirectories.Select();
            }
        }

        /// <summary>Raises the <see cref="Enter"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnter(EventArgs e) 
        {
            ValidateMenuItems(DataContext);
            base.OnEnter(e);
        }

        /// <summary>Raises the <see cref="VisibleChanged"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e) 
        {
            ValidateMenuItems(DataContext);
            base.OnVisibleChanged(e);
        }

        /// <summary>
        /// Function to assign a data context to the view as a view model.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
        /// </para>
        /// </remarks>
        public void SetDataContext(IFileExplorer dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if ((IsDesignTime) || (DataContext == null))
            {
                return;
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;
            DataContext.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
            if (DataContext.Clipboard != null)
            {
                DataContext.Clipboard.PropertyChanged += Clipboard_PropertyChanged;
            }
        }

        /// <summary>
        /// Function to create a directory on the file system.
        /// </summary>
        public void CreateDirectory() => MenuItemCreateDirectory.PerformClick();

        /// <summary>
        /// Function to rename the selected directory.
        /// </summary>
        public void RenameDirectory() => MenuItemRenameDirectory.PerformClick();

        /// <summary>
        /// Function to rename the selected file.
        /// </summary>
        public void RenameFile() => MenuItemRenameFile.PerformClick();

        /// <summary>
        /// Function to delete the selected directory.
        /// </summary>
        public void DeleteDirectory() => MenuItemDeleteDirectory.PerformClick();

        /// <summary>
        /// Function to delete the selected files.
        /// </summary>
        public void DeleteFiles() => MenuItemDeleteFiles.PerformClick();

        /// <summary>
        /// Function to export the selected directory to the physical file system.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        public async Task ExportDirectoryAsync()
        {
            if ((DataContext?.ExportDirectoryCommand == null)
                || (!DataContext.ExportDirectoryCommand.CanExecute(null)))
            {
                return;
            }

            await DataContext.ExportDirectoryCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// Function to export the selected file(s) to the physical file system.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        public async Task ExportFilesAsync()
        {
            if ((DataContext?.ExportFilesCommand == null)
                || (!DataContext.ExportFilesCommand.CanExecute(null)))
            {
                return;
            }

            await DataContext.ExportFilesCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// Function to copy data to the clipboard.
        /// </summary>
        /// <param name="operation">The clipboard copy operation type.</param>
        public void CopyDirectoryToClipboard(CopyMoveOperation operation)
        {
            IDirectory selectedDirectory = DataContext.SelectedDirectory;

            if ((DataContext?.Clipboard == null) || (selectedDirectory == null))
            {
                return;
            }

            try
            {
                IClipboardHandler handler = DataContext.Clipboard;

                var directoryCopyData = new DirectoryCopyMoveData
                {
                    Operation = operation,
                    SourceDirectory = selectedDirectory.ID
                };

                if ((handler.CopyDataCommand == null) || (!handler.CopyDataCommand.CanExecute(directoryCopyData)))
                {
                    return;
                }

                handler.CopyDataCommand.Execute(directoryCopyData);
                selectedDirectory.IsCut = operation == CopyMoveOperation.Move;
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>
        /// Function to copy data to the clipboard.
        /// </summary>
        /// <param name="operation">The clipboard copy operation type.</param>
        public void CopyFileToClipboard(CopyMoveOperation operation)
        {
            IReadOnlyList<IFile> selectedFiles = DataContext.SelectedFiles;

            if ((DataContext?.Clipboard == null) || (selectedFiles.Count == 0))
            {
                return;
            }

            try
            {
                IClipboardHandler handler = DataContext.Clipboard;

                var fileCopyData = new FileCopyMoveData
                {
                    Operation = operation,
                    SourceFiles = selectedFiles.Select(item => item.ID).ToArray()
                };

                if ((handler.CopyDataCommand == null) || (!handler.CopyDataCommand.CanExecute(fileCopyData)))
                {
                    return;
                }

                handler.CopyDataCommand.Execute(fileCopyData);

                if (operation != CopyMoveOperation.Move)
                {
                    return;
                }

                foreach (IFile file in selectedFiles)
                {
                    file.IsCut = true;
                }
            }
            finally
            {
                ValidateMenuItems(DataContext);
            }
        }

        /// <summary>
        /// Function to import files into the file system.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        public async Task ImportAsync()
        {
            if (DataContext?.ImportCommand == null)
            {
                return;
            }

            IImportData importData = new ImportData
            {
                Destination = DataContext.SelectedDirectory
            };

            if ((DataContext?.ImportCommand == null) || (!DataContext.ImportCommand.CanExecute(importData)))
            {
                return;
            }

            await DataContext.ImportCommand.ExecuteAsync(importData);

            if ((!importData.ItemsImported)
                || (!_directoryNodes.TryGetValue(DataContext.SelectedDirectory.ID, out DirectoryTreeNode node))
                || ((DataContext?.SelectDirectoryCommand != null) && (DataContext.SelectDirectoryCommand.CanExecute(node.Name))))
            {                
                return;
            }

            if ((DataContext.SelectedDirectory.Directories.Count > 0) && (!node.IsExpanded))
            {
                node.Expand();
            }

            DataContext.SelectDirectoryCommand.Execute(node.Name);            
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExploder"/> class.
        /// </summary>
        public FileExploder()
        {
            InitializeComponent();

            if (IsDesignTime)
            {
                return;
            }

            _openFileFont = new Font(Font, FontStyle.Bold);
            TreeDirectories.TreeViewNodeSorter = new FileSystemNodeComparer();
            _cutCellStyle = new DataGridViewCellStyle(GridFiles.DefaultCellStyle)
            {
                SelectionBackColor = DarkFormsRenderer.MenuHilightBackground,
                SelectionForeColor = DarkFormsRenderer.FocusedForeground,
                BackColor = GridFiles.BackgroundColor,
                ForeColor = DarkFormsRenderer.CutForeground
            };            

            _openFileStyle = new DataGridViewCellStyle(GridFiles.DefaultCellStyle)
            {
                SelectionBackColor = DarkFormsRenderer.MenuHilightBackground,
                SelectionForeColor = DarkFormsRenderer.ForeColor,
                BackColor = GridFiles.BackgroundColor,
                ForeColor = DarkFormsRenderer.OpenFileForeground,
                Font = _openFileFont
            };

            _unknownFileStyle = new DataGridViewCellStyle(GridFiles.DefaultCellStyle)
            {
                SelectionBackColor = DarkFormsRenderer.MenuHilightBackground,
                SelectionForeColor = DarkFormsRenderer.DisabledColor,
                BackColor = GridFiles.BackgroundColor,
                ForeColor = DarkFormsRenderer.DisabledColor,
            };
        }
        #endregion
    }
}
