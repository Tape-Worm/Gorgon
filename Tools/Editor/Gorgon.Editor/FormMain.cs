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
// Created: August 26, 2018 8:51:04 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.Editor.Views;
using DX = SharpDX;

namespace Gorgon.Editor
{
    /// <summary>
    /// The main application form.
    /// </summary>
    internal partial class FormMain
        : KryptonForm, IDataContext<IMain>
    {
        #region Enumerations.
        /// <summary>
        /// States for the closing handler.
        /// </summary>
        private enum CloseStates
        {
            /// <summary>
            /// The application is not closing yet.
            /// </summary>
            NotClosing = 0,
            /// <summary>
            /// The application is currently closing.
            /// </summary>
            Closing = 1,
            /// <summary>
            /// The application is closed.
            /// </summary>
            Closed = 2
        }
        #endregion

        #region Variables.
        // The context for a clipboard handler object.
        private IClipboardHandler _clipboardContext;
        // The flag to indicate that the application is already closing.
        private CloseStates _closeFlag;
        // The ribbon merger for the main ribbon.
        private readonly RibbonMerger _ribbonMerger;
        // The currently selected tab prior to showing a context tab.
        private KryptonRibbonTab _prevTabBeforeContext;
        // The list of groups on the tools tab.
        private readonly Dictionary<string, KryptonRibbonGroup> _toolGroups = new Dictionary<string, KryptonRibbonGroup>(StringComparer.OrdinalIgnoreCase);
        // The list of line groups for the tools tab.
        private readonly List<KryptonRibbonGroupLines> _toolLines = new List<KryptonRibbonGroupLines>();
        // The list of triple groups for the tools tab.
        private readonly List<KryptonRibbonGroupTriple> _toolTriples = new List<KryptonRibbonGroupTriple>();
        // The list of buttons for the tools tab.
        private readonly Dictionary<KryptonRibbonGroupButton, KryptonRibbonGroup> _toolGroupButtons = new Dictionary<KryptonRibbonGroupButton, KryptonRibbonGroup>();
        private readonly Dictionary<string, KryptonRibbonGroupButton> _toolButtons = new Dictionary<string, KryptonRibbonGroupButton>(StringComparer.OrdinalIgnoreCase);
        // The list of separators for the tools tab.
        private readonly List<KryptonRibbonGroupSeparator> _toolSeparators = new List<KryptonRibbonGroupSeparator>();
        // Default event arguments for button validation.
        private readonly CreateDirectoryArgs _createDirArgs = new CreateDirectoryArgs();
        private readonly DeleteArgs _deleteValidationArgs = new DeleteArgs(null);
        private DeleteArgs _deleteAllValidationArgs;
        private readonly ImportData _defaultImportData = new ImportData();
        // The wait panel form.
        private readonly WaitPanelDisplay _waitForm;
        // The progress panel form.
        private readonly ProgressPanelDisplay _progressForm;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        [Browsable(false)]
        public IMain DataContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the application graphics context.
        /// </summary>
        [Browsable(false)]
        public IGraphicsContext GraphicsContext
        {
            get => PanelProject.GraphicsContext;
            set => PanelProject.GraphicsContext = value;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to release all tool ribbon items.
        /// </summary>
        private void ReleaseToolRibbonItems()
        {
            foreach (KryptonRibbonGroupButton button in _toolButtons.Values)
            {
                button.Click -= ToolButton_Click;
                button.Dispose();
            }

            foreach (KryptonRibbonGroupTriple triple in _toolTriples)
            {
                triple.Dispose();
            }

            foreach (KryptonRibbonGroupLines lines in _toolLines)
            {
                lines.Dispose();
            }

            foreach (KryptonRibbonGroupSeparator separators in _toolSeparators)
            {
                separators.Dispose();
            }

            foreach (KryptonRibbonGroup groups in _toolGroups.Values)
            {
                groups.Dispose();
            }

            _toolGroupButtons.Clear();
            _toolGroups.Clear();
            _toolSeparators.Clear();
            _toolLines.Clear();
            _toolTriples.Clear();
            _toolButtons.Clear();
        }

        /// <summary>
        /// Function to update the list of buttons, and groups on the tools tab.
        /// </summary>
        /// <param name="buttons">The buttons to display on the ribbon.</param>
        private void UpdateToolsTab(IReadOnlyDictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> buttons)
        {
            if ((buttons == null) || (buttons.Count == 0))
            {
                ReleaseToolRibbonItems();
                return;
            }

            foreach (KeyValuePair<string, IReadOnlyList<IToolPlugInRibbonButton>> buttonItem in buttons)
            {
                if ((buttonItem.Value == null) || (buttonItem.Value.Count == 0))
                {
                    continue;
                }

                if (!_toolGroups.TryGetValue(buttonItem.Key, out KryptonRibbonGroup ribGroup))
                {
                    ribGroup = new KryptonRibbonGroup
                    {
                        TextLine1 = buttonItem.Key,
                        AllowCollapsed = false,
                        KeyTipGroup = buttonItem.Key.Substring(0),
                        DialogBoxLauncher = false
                    };

                    _toolGroups[buttonItem.Key] = ribGroup;
                }

                KryptonRibbonGroupTriple triple = null;
                KryptonRibbonGroupLines lines = null;

                foreach (IToolPlugInRibbonButton button in buttonItem.Value)
                {
                    if (_toolButtons.ContainsKey(button.Name))
                    {
                        continue;
                    }

                    // Add a separator if ask for one (as long as it's not the first item in the group).
                    if ((button.IsSeparator) && (button != buttonItem.Value[0]))
                    {
                        var sep = new KryptonRibbonGroupSeparator();
                        _toolSeparators.Add(sep);
                        ribGroup.Items.Add(sep);
                    }

                    // Figure out which subgroup type to create.
                    if ((triple == null) && (lines == null))
                    {
                        if (button.IsSmall)
                        {
                            lines = new KryptonRibbonGroupLines();
                            lines.Items.Clear();
                            _toolLines.Add(lines);
                            ribGroup.Items.Add(lines);
                        }
                        else
                        {
                            triple = new KryptonRibbonGroupTriple();
                            triple.Items.Clear();
                            _toolTriples.Add(triple);
                            ribGroup.Items.Add(triple);
                        }
                    }
                    else if ((triple != null) && (button.IsSmall))
                    {
                        lines = new KryptonRibbonGroupLines();
                        lines.Items.Clear();
                        _toolLines.Add(lines);
                        ribGroup.Items.Add(lines);
                        triple = null;
                    }
                    else if ((lines != null) && (!button.IsSmall))
                    {
                        triple = new KryptonRibbonGroupTriple();
                        triple.Items.Clear();
                        _toolTriples.Add(triple);
                        ribGroup.Items.Add(triple);
                        lines = null;
                    }

                    var newButton = new KryptonRibbonGroupButton
                    {
                        TextLine1 = button.DisplayText,
                        ButtonType = GroupButtonType.Push,
                        Enabled = false,
                        Tag = button.Name,
                        ImageLarge = button.LargeIcon,
                        ImageSmall = button.SmallIcon
                    };

                    if (!string.IsNullOrWhiteSpace(button.Description))
                    {
                        newButton.ToolTipTitle = button.DisplayText;
                        newButton.ToolTipBody = button.Description;
                    }

                    newButton.Click += ToolButton_Click;

                    _toolGroupButtons[newButton] = ribGroup;
                    _toolButtons[button.Name] = newButton;

                    // Add the button.
                    if (triple != null)
                    {
                        triple.Items.Add(newButton);
                    }
                    else if (lines != null)
                    {
                        lines.Items.Add(newButton);
                    }
                }
            }

            if (_toolGroups.Count == 0)
            {
                RibbonTabEditorTools.Visible = false;
                return;
            }

            RibbonMain.SuspendLayout();
            RibbonTabEditorTools.Groups.Clear();

            foreach (KryptonRibbonGroup ribbonGroup in _toolGroups.Values)
            {
                RibbonTabEditorTools.Groups.Add(ribbonGroup);
            }

            RibbonTabEditorTools.Visible = true;

            RibbonMain.ResumeLayout();
            RibbonMain.CheckPerformLayout();
        }

        /// <summary>Handles the Click event of the ToolButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolButton_Click(object sender, EventArgs e)
        {
            if (DataContext?.CurrentProject == null)
            {
                return;
            }

            var button = (KryptonRibbonGroupButton)sender;

            string name = button.Tag?.ToString() ?? string.Empty;

            if (!_toolGroupButtons.TryGetValue(button, out KryptonRibbonGroup ribbonGroup))
            {
                return;
            }

            if (!DataContext.CurrentProject.ToolButtons.TryGetValue(ribbonGroup.TextLine1, out IReadOnlyList<IToolPlugInRibbonButton> buttons))
            {
                return;
            }

            IToolPlugInRibbonButton toolButton = buttons.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));

            toolButton?.ClickCallback();
        }

        /// <summary>Handles the Click event of the ButtonOpenContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonOpenContent_Click(object sender, EventArgs e)
        {
            IProjectEditor project = DataContext?.CurrentProject;
            IFileExplorer fileExplorer = project?.FileExplorer;
            string currentFilePath = fileExplorer?.SelectedFiles[0].FullPath;

            if ((fileExplorer?.OpenContentFileCommand == null) || (!fileExplorer.OpenContentFileCommand.CanExecute(currentFilePath)))
            {
                return;
            }

            fileExplorer.OpenContentFileCommand.Execute(currentFilePath);
        }


        /// <summary>Handles the Click event of the ButtonFileSystemRefresh control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonFileSystemRefresh_Click(object sender, EventArgs e)
        {
            IProjectEditor project = DataContext?.CurrentProject;
            IFileExplorer fileExplorer = project?.FileExplorer;
            IContentPreview preview = project?.ContentPreviewer;

            if ((fileExplorer?.RefreshCommand == null) || (!fileExplorer.RefreshCommand.CanExecute(null)))
            {
                return;
            }

            await fileExplorer.RefreshCommand.ExecuteAsync(null);

            if ((project.SaveProjectMetadataCommand != null) && (project.SaveProjectMetadataCommand.CanExecute(null)))
            {
                project.SaveProjectMetadataCommand.Execute(null);
            }

            if ((preview?.ResetPreviewCommand == null) || (preview?.RefreshPreviewCommand == null))
            {
                return;
            }

            if (preview.ResetPreviewCommand.CanExecute(null))
            {
                await preview.ResetPreviewCommand.ExecuteAsync(null);
            }

            IFile file = fileExplorer.SelectedFiles.Count > 0 ? fileExplorer.SelectedFiles[0] : null;

            if ((file == null) || (!preview.RefreshPreviewCommand.CanExecute(file.FullPath)))
            {
                return;
            }

            await preview.RefreshPreviewCommand.ExecuteAsync(file.FullPath);
        }

        /// <summary>Handles the RibbonAdded event of the PanelProject control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [ContentRibbonEventArgs] instance containing the event data.</param>
        private void PanelProject_RibbonAdded(object sender, ContentRibbonEventArgs e)
        {
            KryptonRibbonTab firstTab = null;

            RibbonMain.SuspendLayout();

            if ((e.Ribbon != null) && (e.Ribbon.RibbonTabs.Count > 0))
            {
                firstTab = e.Ribbon.RibbonTabs[0];
            }

            _ribbonMerger.Merge(e.Ribbon);

            // Default to the first tab on the joined ribbon.
            if (firstTab != null)
            {
                RibbonMain.SelectedTab = firstTab;
            }

            RibbonMain.ResumeLayout(true);
        }

        /// <summary>Handles the RibbonRemoved event of the PanelProject control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [ContentRibbonEventArgs] instance containing the event data.</param>
        private void PanelProject_RibbonRemoved(object sender, ContentRibbonEventArgs e) => _ribbonMerger.Unmerge(e.Ribbon);

        /// <summary>
        /// Handles the Click event of the ButtonExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonExport_Click(object sender, EventArgs e)
        {
            try
            {
                switch (PanelProject.FileExplorer.ControlContext)
                {
                    case FileExplorerContext.DirectoryTree:
                        await PanelProject.FileExplorer.ExportDirectoryAsync();
                        break;
                    case FileExplorerContext.FileList:
                        await PanelProject.FileExplorer.ExportFilesAsync();
                        break;
                }
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonImport_Click(object sender, EventArgs e)
        {
            try
            {
                await PanelProject.FileExplorer.ImportAsync();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemDeleteAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemDeleteAll_Click(object sender, EventArgs e)
        {
            IFileExplorer fileExplorer = DataContext?.CurrentProject?.FileExplorer;

            try
            {
                var args = new DeleteArgs(fileExplorer.Root.ID);
                if ((fileExplorer?.DeleteDirectoryCommand == null) || (!fileExplorer.DeleteDirectoryCommand.CanExecute(args)))
                {
                    return;
                }

                fileExplorer.DeleteDirectoryCommand.Execute(args);
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>Handles the OpenClicked event of the Stage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Stage_OpenClicked(object sender, EventArgs e)
        {
            if ((DataContext?.OpenPackFileCommand == null) || (!DataContext.OpenPackFileCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OpenPackFileCommand.Execute(null);
        }

        /// <summary>Handles the BrowseClicked event of the StageLive control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void StageLive_BrowseClicked(object sender, EventArgs e)
        {
            if ((DataContext?.BrowseProjectCommand == null) || (!DataContext.BrowseProjectCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.BrowseProjectCommand.Execute(null);
        }

        /// <summary>
        /// Handles the Activated event of the FormMain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormMain_Activated(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            ValidateRibbonButtons();
        }

        /// <summary>Handles the Click event of the ButtonSave control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (DataContext?.CurrentProject == null)
            {
                return;
            }

            var args = new SaveEventArgs(false);
            StageLive_Save(this, args);
        }

        /// <summary>Handles the Click event of the ButtonFileSystemPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemPanel_Click(object sender, EventArgs e)
        {
            if (DataContext?.CurrentProject == null)
            {
                return;
            }
            
            DataContext.CurrentProject.ShowFileExplorer = ButtonFileSystemPanel.Checked;
            ValidateRibbonButtons();
        }

        /// <summary>Handles the Click event of the ButtonFileSystemPreview control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemPreview_Click(object sender, EventArgs e)
        {
            if (DataContext?.CurrentProject == null)
            {
                return;
            }

            DataContext.CurrentProject.ShowContentPreview = ButtonFileSystemPreview.Checked;
            ValidateRibbonButtons();
        }

        /// <summary>Handles the FileExplorerIsRenaming event of the PanelProject control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelProject_FileExplorerIsRenaming(object sender, EventArgs e) => ValidateRibbonButtons();

        /// <summary>Handles the FileExplorerContextChanged event of the PanelProject control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelProject_FileExplorerContextChanged(object sender, EventArgs e) => ValidateRibbonButtons();

        /// <summary>
        /// Handles the BackClicked event of the StageLive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void StageLive_BackClicked(object sender, EventArgs e) => NavigateToProjectView(DataContext);

        /// <summary>
        /// Handles the Save event of the StageLive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SaveEventArgs" /> instance containing the event data.</param>
        private async void StageLive_Save(object sender, SaveEventArgs e)
        {
            if (DataContext?.CurrentProject?.SaveProjectToPackFileCommand == null)
            {
                return;
            }

            var args = new CancelEventArgs();

            if (!DataContext.CurrentProject.SaveProjectToPackFileCommand.CanExecute(args))
            {
                NavigateToProjectView(DataContext);
                return;
            }

            await DataContext.CurrentProject.SaveProjectToPackFileCommand.ExecuteAsync(args);

            NavigateToProjectView(DataContext);
        }

        /// <summary>
        /// Handles the AppButtonMenuOpening event of the RibbonMain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void RibbonMain_AppButtonMenuOpening(object sender, CancelEventArgs e)
        {
            NavigateToStagingView();
            e.Cancel = true;
        }

        /// <summary>
        /// Function to validate the state of the ribbon buttons.
        /// </summary>
        private void ValidateRibbonButtons()
        {
            IProjectEditor project = DataContext?.CurrentProject;
            IFileExplorer fileExplorer = project?.FileExplorer;

            if ((project == null) || (fileExplorer == null))
            {
                TabFileSystem.Visible = false;
                RibbonTabEditorTools.Visible = false;
                return;
            }

            TabFileSystem.Visible = true;
            _defaultImportData.Destination = fileExplorer.SelectedDirectory;

            ButtonFileSystemPanel.Enabled = project != null;
            ButtonFileSystemPreview.Enabled = (ButtonFileSystemPanel.Enabled) && (project.ShowFileExplorer);
            ButtonFileSystemNewDirectory.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.CreateDirectoryCommand?.CanExecute(_createDirArgs) ?? false);            
            ButtonImport.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.ImportCommand?.CanExecute(_defaultImportData) ?? false);            
            ButtonOpenContent.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.OpenContentFileCommand?.CanExecute(null) ?? false);
            GroupCreate.Visible = SepCreate.Visible = DataContext.ContentCreators.Count > 0;
            ButtonFileSystemRefresh.Enabled = !PanelProject.FileExplorer.IsRenaming;

            foreach (ToolStripItem item in MenuCreate.Items)
            {
                if (item.Tag is Guid id)
                {
                    item.Enabled = project.CreateContentCommand?.CanExecute(id) ?? false;
                }
            }

            switch (PanelProject.FileExplorer.ControlContext)
            {
                case FileExplorerContext.None:
                    ButtonExport.Enabled = 
                    ButtonFileSystemDelete.Enabled = 
                    ButtonFileSystemRename.Enabled = 
                    ButtonFileSystemPaste.Enabled =
                    ButtonFileSystemCopy.Enabled = 
                    ButtonFileSystemCut.Enabled = false;
                    break;
                case FileExplorerContext.DirectoryTree:
                    ButtonFileSystemDeleteAll.Visible = (fileExplorer.SelectedDirectory == null) || (fileExplorer.SelectedDirectory == fileExplorer.Root);
                    ButtonFileSystemDelete.Visible = !ButtonFileSystemDeleteAll.Visible;
                    ButtonExport.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.ExportDirectoryCommand?.CanExecute(null) ?? false);
                    ButtonFileSystemCopy.Enabled = (!PanelProject.FileExplorer.IsRenaming)
                                                && ((_clipboardContext?.CopyDataCommand?.CanExecute(new DirectoryCopyMoveData
                                                {
                                                    Operation = CopyMoveOperation.Copy,
                                                    SourceDirectory = fileExplorer.SelectedDirectory?.ID ?? string.Empty
                                                }) ?? false));
                    ButtonFileSystemCut.Enabled = (!PanelProject.FileExplorer.IsRenaming)
                                                && ((_clipboardContext?.CopyDataCommand?.CanExecute(new DirectoryCopyMoveData
                                                {
                                                    Operation = CopyMoveOperation.Copy,
                                                    SourceDirectory = fileExplorer.SelectedDirectory?.ID ?? string.Empty
                                                }) ?? false));
                    ButtonFileSystemPaste.Enabled = (!PanelProject.FileExplorer.IsRenaming)
                                                && (_clipboardContext?.PasteDataCommand?.CanExecute(null) ?? false);
                    ButtonFileSystemDeleteAll.Enabled = 
                    ButtonFileSystemDelete.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.DeleteDirectoryCommand?.CanExecute(_deleteValidationArgs) ?? false);
                    ButtonFileSystemDeleteAll.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.DeleteDirectoryCommand?.CanExecute(_deleteAllValidationArgs) ?? false);
                    ButtonFileSystemRename.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.RenameDirectoryCommand?.CanExecute(null) ?? false);
                    break;
                case FileExplorerContext.FileList:
                    string[] validationFiles = fileExplorer.SelectedFiles.Select(item => item.ID).ToArray();
                    ButtonFileSystemDeleteAll.Visible = false;
                    ButtonFileSystemDelete.Visible = true;
                    ButtonExport.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.ExportFilesCommand?.CanExecute(null) ?? false);
                    ButtonFileSystemCopy.Enabled = (!PanelProject.FileExplorer.IsRenaming)
                                                && ((_clipboardContext?.CopyDataCommand?.CanExecute(new FileCopyMoveData
                                                {
                                                    Operation = CopyMoveOperation.Copy,
                                                    SourceFiles = validationFiles
                                                }) ?? false))
                    ;
                    ButtonFileSystemCut.Enabled = (!PanelProject.FileExplorer.IsRenaming)
                                                && ((_clipboardContext?.CopyDataCommand?.CanExecute(new FileCopyMoveData
                                                {
                                                    Operation = CopyMoveOperation.Copy,
                                                    SourceFiles = validationFiles
                                                }) ?? false));
                    ButtonFileSystemPaste.Enabled = (!PanelProject.FileExplorer.IsRenaming)
                                                && (_clipboardContext?.PasteDataCommand?.CanExecute(null) ?? false);
                    ButtonFileSystemDeleteAll.Enabled = false;
                    ButtonFileSystemDelete.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.DeleteFileCommand?.CanExecute(_deleteValidationArgs) ?? false);
                    ButtonFileSystemRename.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.RenameFileCommand?.CanExecute(null) ?? false);
                    break;
            }

            foreach (IToolPlugInRibbonButton button in project.ToolButtons.SelectMany(item => item.Value))
            {
                if (!_toolButtons.TryGetValue(button.Name, out KryptonRibbonGroupButton ribButton))
                {
                    continue;
                }

                if (button.CanExecute != null)
                {
                    ribButton.Enabled = button.CanExecute();
                }
                else
                {
                    ribButton.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Function to ensure that the wait panel stays on top if it is active.
        /// </summary>
        private void KeepWaitPanelOnTop()
        {
            _waitForm.BringToFront();
            _progressForm.BringToFront();
        }

        /// <summary>
        /// Function to navigate to the project view control.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void NavigateToProjectView(IMain dataContext)
        {
            Stage.Visible = false;
            RibbonMain.Visible = true;
            PanelWorkSpace.Visible = true;
            Text = DataContext.Text;

            PanelWorkSpace.BringToFront();
            KeepWaitPanelOnTop();

            if (PanelProject.DataContext != dataContext?.CurrentProject)
            {
                PanelProject.SetDataContext(dataContext?.CurrentProject);
            }
            else
            {
                PanelProject.Focus();
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Function to navigate to the staging control.
        /// </summary>
        private void NavigateToStagingView()
        {
            if (DataContext?.CurrentProject?.SaveProjectToPackFileCommand != null)
            {
                var saveAsArgs = new CancelEventArgs();
                Stage.CanSavePackedFile = DataContext.CurrentProject.SaveProjectToPackFileCommand.CanExecute(saveAsArgs);
            }
            else
            {
                Stage.CanSavePackedFile = false;
            }

            Text = string.Empty;
            Stage.IsStartup = false;
            Stage.CanOpen = (DataContext.OpenPackFileCommand != null) && (DataContext.OpenPackFileCommand.CanExecute(null));

            Stage.Visible = true;
            _clipboardContext = null;
            RibbonMain.Visible = false;
            PanelWorkSpace.Visible = false;

            Stage.BringToFront();

            KeepWaitPanelOnTop();
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
                case nameof(IMain.CurrentProject):
                    ReleaseToolRibbonItems();

                    _deleteAllValidationArgs = null;
                    RibbonMain.SelectedContext = string.Empty;

                    if (DataContext.CurrentProject != null)
                    {
                        DataContext.CurrentProject.PropertyChanging -= CurrentProject_PropertyChanging;
                        DataContext.CurrentProject.PropertyChanged -= CurrentProject_PropertyChanged;
                        if (DataContext.CurrentProject.FileExplorer != null)
                        {
                            DataContext.CurrentProject.FileExplorer.FileSystemUpdated -= FileExplorer_FileSystemUpdated;
                            DataContext.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
                            DataContext.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
                        }
                    }

                    // Unload this view model since we're about to replace it.
                    PanelProject.SetDataContext(null);

                    ValidateRibbonButtons();
                    break;
            }
        }

        /// <summary>Handles the PropertyChanging event of the CurrentProject control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void CurrentProject_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IProjectEditor.CommandContext):
                    if (string.IsNullOrWhiteSpace(DataContext.CurrentProject.CommandContext))
                    {
                        _prevTabBeforeContext = RibbonMain.SelectedTab;
                    }
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the CurrentProject control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentProject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IProjectEditor.CommandContext):
                    RibbonMain.SelectedContext = DataContext.CurrentProject.CommandContext;

                    if (string.IsNullOrWhiteSpace(DataContext.CurrentProject.CommandContext))
                    {
                        if (_prevTabBeforeContext != null)
                        {
                            RibbonMain.SelectedTab = _prevTabBeforeContext;
                        }
                    }
                    else
                    {
                        // Find the first tab associated with this context.
                        KryptonRibbonTab contextTab = RibbonMain.RibbonTabs.FirstOrDefault(item => string.Equals(DataContext.CurrentProject.CommandContext, item.ContextName, StringComparison.OrdinalIgnoreCase));
                        if (contextTab != null)
                        {
                            RibbonMain.SelectedTab = contextTab;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to notify when the selected files are changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void SelectedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => ValidateRibbonButtons();

        /// <summary>Handles the FileSystemUpdated event of the FileExplorer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_FileSystemUpdated(object sender, EventArgs e) => ValidateRibbonButtons();

        /// <summary>
        /// Handles the PropertyChanged event of the FileExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void FileExplorer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorer.SearchResults):
                case nameof(IUndoHandler.UndoCommand):
                case nameof(IUndoHandler.RedoCommand):
                case nameof(IFileExplorer.SelectedFiles):
                case nameof(IFileExplorer.SelectedDirectory):
                    ValidateRibbonButtons();
                    break;
            }
        }

        /// <summary>
        /// Handles the DataUpdated event of the ClipboardContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ClipboardContext_DataUpdated(object sender, EventArgs e) => ValidateRibbonButtons();

        /// <summary>
        /// Handles the PropertyChanged event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IMain.Text):
                    Text = DataContext.Text;
                    ValidateRibbonButtons();
                    break;
                case nameof(IMain.ClipboardContext):
                    _clipboardContext = DataContext.ClipboardContext;
                    break;
                case nameof(IMain.CurrentProject):
                    NavigateToProjectView(DataContext);

                    if (DataContext.CurrentProject == null)
                    {
                        break;
                    }

                    DataContext.CurrentProject.PropertyChanging += CurrentProject_PropertyChanging;
                    DataContext.CurrentProject.PropertyChanged += CurrentProject_PropertyChanged;

                    if (DataContext.CurrentProject.FileExplorer != null)
                    {
                        DataContext.CurrentProject.FileExplorer.FileSystemUpdated += FileExplorer_FileSystemUpdated;
                        DataContext.CurrentProject.FileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
                        DataContext.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
                    }

                    RibbonMain.SelectedContext = DataContext.CurrentProject.CommandContext;

                    TabFileSystem.Visible = true;
                    UpdateToolsTab(DataContext.CurrentProject.ToolButtons);

                    _deleteAllValidationArgs = new DeleteArgs(DataContext.CurrentProject.FileExplorer.Root.ID);
                    ButtonFileSystemPanel.Checked = DataContext.CurrentProject.ShowFileExplorer;
                    ButtonFileSystemPreview.Checked = DataContext.CurrentProject.ShowContentPreview;
                    break;                
            }

            ValidateRibbonButtons();
        }

        /// <summary>
        /// Function to update the icons used for the "new" buttons.
        /// </summary>
        /// <param name="metadata">The metadata for plug ins that can create content.</param>
        private void AddNewIcons(IEnumerable<IContentPlugInMetadata> metadata)
        {
            if (metadata == null)
            {
                return;
            }

            foreach (IContentPlugInMetadata item in metadata)
            {
                string id = item.NewIconID.ToString("N");
                Image icon = item.GetNewIcon();

                if ((MenuCreate.Items.ContainsKey(id))
                    || (icon == null))
                {
                    continue;
                }

                var menuItem = new ToolStripMenuItem(string.Format(Resources.GOREDIT_CREATE_NEW, item.ContentType), icon)
                {
                    Name = id,
                    Tag = item.NewIconID
                };
                menuItem.Click += NewItem_Click;

                MenuCreate.Items.Add(menuItem);
            }

            ValidateRibbonButtons();
        }

        /// <summary>Handles the Click event of the NewItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NewItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var id = (Guid)item.Tag;

            if ((DataContext?.CurrentProject?.CreateContentCommand == null) || (!DataContext.CurrentProject.CreateContentCommand.CanExecute(id)))
            {
                return;
            }

            DataContext.CurrentProject.CreateContentCommand.Execute(id);
        }

        /// <summary>
        /// Function to initialize the form from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(IMain dataContext)
        {
            if (dataContext == null)
            {
                _deleteAllValidationArgs = null;
                _clipboardContext = null;
                Text = Resources.GOREDIT_CAPTION_NO_FILE;
                ReleaseToolRibbonItems();
                return;
            }

            Text = string.Empty;
            _clipboardContext = dataContext.CurrentProject?.ClipboardContext;

            if (dataContext.CurrentProject != null)
            {
                RibbonMain.SelectedContext = dataContext.CurrentProject.CommandContext;
                ButtonFileSystemPanel.Checked = dataContext.CurrentProject.ShowFileExplorer;
                ButtonFileSystemPreview.Checked = dataContext.CurrentProject.ShowContentPreview;
            }
            else
            {
                ButtonFileSystemPreview.Checked = false;
                ButtonFileSystemPanel.Checked = false;
            }

            AddNewIcons(dataContext.ContentCreators);
        }

        /// <summary>
        /// Function to unassign the events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            _progressForm.SetDataContext(null);

            if (DataContext == null)
            {
                return;
            }

            if (DataContext.CurrentProject != null)
            {
                DataContext.CurrentProject.PropertyChanging -= CurrentProject_PropertyChanging;
                DataContext.CurrentProject.PropertyChanged -= CurrentProject_PropertyChanged;

                if (DataContext.CurrentProject.FileExplorer != null)
                {
                    DataContext.CurrentProject.FileExplorer.FileSystemUpdated -= FileExplorer_FileSystemUpdated;
                    DataContext.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
                    DataContext.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
                }
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemNewDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemNewDirectory_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.CreateDirectory();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemRename control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemRename_Click(object sender, EventArgs e)
        {
            try
            {
                switch(PanelProject.FileExplorer.ControlContext)
                {
                    case FileExplorerContext.DirectoryTree:
                        PanelProject.FileExplorer.RenameDirectory();
                        break;
                    case FileExplorerContext.FileList:
                        PanelProject.FileExplorer.RenameFile();
                        break;
                }
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemDelete_Click(object sender, EventArgs e)
        {
            try
            {
                switch (PanelProject.FileExplorer.ControlContext)
                {
                    case FileExplorerContext.DirectoryTree:
                        PanelProject.FileExplorer.DeleteDirectory();
                        break;
                    case FileExplorerContext.FileList:
                        PanelProject.FileExplorer.DeleteFiles();
                        break;
                }
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemCut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemCut_Click(object sender, EventArgs e)
        {
            if (_clipboardContext?.CopyDataCommand == null)
            {
                return;
            }

            switch (PanelProject.FileExplorer.ControlContext)
            {
                case FileExplorerContext.DirectoryTree:
                    PanelProject.FileExplorer.CopyDirectoryToClipboard(CopyMoveOperation.Move);
                    break;
                case FileExplorerContext.FileList:
                    PanelProject.FileExplorer.CopyFileToClipboard(CopyMoveOperation.Move);
                    break;
            }

            ValidateRibbonButtons();
        }


        /// <summary>
        /// Handles the Click event of the ButtonFileSystemPaste control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonFileSystemPaste_Click(object sender, EventArgs e)
        {
            if ((_clipboardContext?.PasteDataCommand == null)
                || (!_clipboardContext.PasteDataCommand.CanExecute(null)))
            {
                return;
            }

            await _clipboardContext.PasteDataCommand.ExecuteAsync(null);
            ValidateRibbonButtons();
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemCopy_Click(object sender, EventArgs e) 
        {
            if (_clipboardContext?.CopyDataCommand == null)
            {
                return;
            }

            switch (PanelProject.FileExplorer.ControlContext)
            {
                case FileExplorerContext.DirectoryTree:
                    PanelProject.FileExplorer.CopyDirectoryToClipboard(CopyMoveOperation.Copy);
                    break;
                case FileExplorerContext.FileList:
                    PanelProject.FileExplorer.CopyFileToClipboard(CopyMoveOperation.Copy);
                    break;
            }

            ValidateRibbonButtons();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd"/> event.</summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            if ((WindowState != FormWindowState.Normal) || (DataContext == null))
            {
                return;
            }

            DataContext.Settings.WindowBounds = new DX.Rectangle(DesktopBounds.X, DesktopBounds.Y, DesktopBounds.Width, DesktopBounds.Height);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DataContext == null)
            {
                return;
            }

            DataContext.OnLoad();

            Stage.CanOpen = (DataContext.OpenPackFileCommand != null) && (DataContext.OpenPackFileCommand.CanExecute(null));

            Focus();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data. </param>
        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (DataContext == null)
            {
                return;
            }

            switch (_closeFlag)
            {
                case CloseStates.Closing:
                    // Don't hang on to this.
                    GraphicsContext = null;
                    e.Cancel = true;
                    return;
                case CloseStates.NotClosing:
                    _closeFlag = CloseStates.Closing;
                    e.Cancel = true;

                    DX.Rectangle windowDimensions;
                    int windowState;
                    switch (WindowState)
                    {
                        case FormWindowState.Normal:
                        case FormWindowState.Maximized:
                            windowDimensions = new DX.Rectangle(Location.X, Location.Y, Size.Width, Size.Height);
                            windowState = (int)WindowState;
                            break;
                        default:
                            windowDimensions = new DX.Rectangle(RestoreBounds.X, RestoreBounds.Y, RestoreBounds.Width, RestoreBounds.Height);
                            windowState = (int)FormWindowState.Normal;
                            break;
                    }

                    var args = new AppCloseArgs(windowDimensions, windowState);

                    // If we don't have anything to handle the shut down, then just shut it all down.
                    if ((DataContext.AppClosingAsyncCommand == null) || (!DataContext.AppClosingAsyncCommand.CanExecute(args)))
                    {
                        _closeFlag = CloseStates.NotClosing;
                        e.Cancel = false;
                        return;
                    }

                    await DataContext.AppClosingAsyncCommand.ExecuteAsync(args);

                    if (args.Cancel)
                    {
                        _closeFlag = CloseStates.NotClosing;
                        return;
                    }

                    await Task.Yield();

                    _closeFlag = CloseStates.Closed;
                    Close();
                    break;
                default:
                    e.Cancel = false;
                    break;
            }
        }

        /// <summary>
        /// Function to assign a data context to the view.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        public void SetDataContext(IMain dataContext)
        {
            try
            {
                UnassignEvents();

                // Assign the data context to the new view.
                Stage.NewProject.SetDataContext(dataContext.NewProject);
                Stage.Recent.SetDataContext(dataContext.RecentFiles);
                Stage.SettingsPanel.SetDataContext(dataContext.SettingsViewModel);
                Stage.IsStartup = true;

                InitializeFromDataContext(dataContext);
                DataContext = dataContext;

                _waitForm.SetDataContext(DataContext);
                _progressForm.SetDataContext(DataContext);

                if (DataContext == null)
                {
                    return;
                }

                if (DataContext.CurrentProject != null)
                {
                    DataContext.CurrentProject.PropertyChanging += CurrentProject_PropertyChanging;
                    DataContext.CurrentProject.PropertyChanged += CurrentProject_PropertyChanged;

                    if (DataContext.CurrentProject.FileExplorer != null)
                    {
                        DataContext.CurrentProject.FileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
                        DataContext.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
                        DataContext.CurrentProject.FileExplorer.FileSystemUpdated += FileExplorer_FileSystemUpdated;
                    }

                    _deleteAllValidationArgs = new DeleteArgs(DataContext.CurrentProject.FileExplorer.Root.ID);
                }

                DataContext.PropertyChanged += DataContext_PropertyChanged;
                DataContext.PropertyChanging += DataContext_PropertyChanging;
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormMain"/> class.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                _ribbonMerger = new RibbonMerger(RibbonMain);
                _waitForm = new WaitPanelDisplay(this);
                _progressForm = new ProgressPanelDisplay(this);
            }

            RibbonMain.AllowFormIntegrate = false;
            PanelProject.MainRibbon = RibbonMain;
        }
        #endregion
    }
}
