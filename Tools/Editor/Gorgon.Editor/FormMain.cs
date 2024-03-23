
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
// Created: August 26, 2018 8:51:04 PM
// 


using System.Collections.Specialized;
using System.ComponentModel;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.Editor.Views;
using Gorgon.Graphics;
using Krypton.Ribbon;
using Krypton.Toolkit;

namespace Gorgon.Editor;

/// <summary>
/// The main application form
/// </summary>
internal partial class FormMain
    : KryptonForm, IDataContext<IMain>
{
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



    // The context for a clipboard handler object.
    private IClipboardHandler _clipboardContext;
    // The flag to indicate that the application is already closing.
    private CloseStates _closeFlag;
    // The ribbon merger for the main ribbon.
    private readonly RibbonMerger _ribbonMerger;
    // The currently selected tab prior to showing a context tab.
    private KryptonRibbonTab _prevTabBeforeContext;
    // The list of groups on the tools tab.
    private readonly Dictionary<string, KryptonRibbonGroup> _toolGroups = new(StringComparer.OrdinalIgnoreCase);
    // The list of line groups for the tools tab.
    private readonly List<KryptonRibbonGroupLines> _toolLines = [];
    // The list of triple groups for the tools tab.
    private readonly List<KryptonRibbonGroupTriple> _toolTriples = [];
    // The list of buttons for the tools tab.
    private readonly Dictionary<KryptonRibbonGroupButton, KryptonRibbonGroup> _toolGroupButtons = [];
    private readonly Dictionary<string, KryptonRibbonGroupButton> _toolButtons = new(StringComparer.OrdinalIgnoreCase);
    // The list of separators for the tools tab.
    private readonly List<KryptonRibbonGroupSeparator> _toolSeparators = [];
    // Default event arguments for button validation.
    private readonly CreateDirectoryArgs _createDirArgs = new();
    private readonly DeleteArgs _deleteValidationArgs = new(null);
    private DeleteArgs _deleteAllValidationArgs;
    private readonly ImportData _defaultImportData = new();
    // The wait panel form.
    private readonly WaitPanelDisplay _waitForm;
    // The progress panel form.
    private readonly ProgressPanelDisplay _progressForm;
    // The application settings.
    private readonly EditorSettings _settings;



    /// <summary>
    /// Property to return the data context assigned to this view.
    /// </summary>
    [Browsable(false)]
    public IMain ViewModel
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
        if ((buttons is null) || (buttons.Count == 0))
        {
            ReleaseToolRibbonItems();
            return;
        }

        foreach (KeyValuePair<string, IReadOnlyList<IToolPlugInRibbonButton>> buttonItem in buttons)
        {
            if ((buttonItem.Value is null) || (buttonItem.Value.Count == 0))
            {
                continue;
            }

            if (!_toolGroups.TryGetValue(buttonItem.Key, out KryptonRibbonGroup ribGroup))
            {
                ribGroup = new KryptonRibbonGroup
                {
                    TextLine1 = buttonItem.Key,
                    AllowCollapsed = false,
                    KeyTipGroup = buttonItem.Key[..],
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
                    KryptonRibbonGroupSeparator sep = new();
                    _toolSeparators.Add(sep);
                    ribGroup.Items.Add(sep);
                }

                // Figure out which subgroup type to create.
                if ((triple is null) && (lines is null))
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
                else if ((triple is not null) && (button.IsSmall))
                {
                    lines = new KryptonRibbonGroupLines();
                    lines.Items.Clear();
                    _toolLines.Add(lines);
                    ribGroup.Items.Add(lines);
                    triple = null;
                }
                else if ((lines is not null) && (!button.IsSmall))
                {
                    triple = new KryptonRibbonGroupTriple();
                    triple.Items.Clear();
                    _toolTriples.Add(triple);
                    ribGroup.Items.Add(triple);
                    lines = null;
                }

                KryptonRibbonGroupButton newButton = new()
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
                    newButton.ToolTipValues.Heading = button.DisplayText;
                    newButton.ToolTipValues.Description = button.Description;
                }

                newButton.Click += ToolButton_Click;

                _toolGroupButtons[newButton] = ribGroup;
                _toolButtons[button.Name] = newButton;

                // Add the button.
                if (triple is not null)
                {
                    triple.Items.Add(newButton);
                }
                else
                {
                    lines?.Items.Add(newButton);
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
    private async void ToolButton_Click(object sender, EventArgs e)
    {
        if (ViewModel?.CurrentProject is null)
        {
            return;
        }

        KryptonRibbonGroupButton button = (KryptonRibbonGroupButton)sender;

        string name = button.Tag?.ToString() ?? string.Empty;

        if (!_toolGroupButtons.TryGetValue(button, out KryptonRibbonGroup ribbonGroup))
        {
            return;
        }

        if (!ViewModel.CurrentProject.ToolButtons.TryGetValue(ribbonGroup.TextLine1, out IReadOnlyList<IToolPlugInRibbonButton> buttons))
        {
            return;
        }

        IToolPlugInRibbonButton toolButton = buttons.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));

        toolButton?.ClickCallback();

        IFile selectedFile = null;
        if (ViewModel.CurrentProject.FileExplorer is null)
        {
            return;
        }

        if (ViewModel.CurrentProject.FileExplorer.SelectedFiles.Count > 0)
        {
            selectedFile = ViewModel.CurrentProject.FileExplorer.SelectedFiles[0];
        }

        if ((ViewModel.CurrentProject.ContentPreviewer?.ResetPreviewCommand is not null)
            && (ViewModel.CurrentProject.ContentPreviewer.ResetPreviewCommand.CanExecute(null)))
        {
            await ViewModel.CurrentProject.ContentPreviewer.ResetPreviewCommand.ExecuteAsync(null);
        }

        if (selectedFile is null)
        {
            return;
        }

        if ((ViewModel.CurrentProject.ContentPreviewer?.RefreshPreviewCommand is null)
            || (!ViewModel.CurrentProject.ContentPreviewer.RefreshPreviewCommand.CanExecute(selectedFile.FullPath)))
        {
            return;
        }

        await ViewModel.CurrentProject.ContentPreviewer.RefreshPreviewCommand.ExecuteAsync(selectedFile.FullPath);
    }

    /// <summary>Handles the Click event of the ButtonOpenContent control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonOpenContent_Click(object sender, EventArgs e)
    {
        IProjectEditor project = ViewModel?.CurrentProject;
        IFileExplorer fileExplorer = project?.FileExplorer;
        string currentFilePath = fileExplorer?.SelectedFiles[0].FullPath;

        if ((fileExplorer?.OpenContentFileCommand is null) || (!fileExplorer.OpenContentFileCommand.CanExecute(currentFilePath)))
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
        IProjectEditor project = ViewModel?.CurrentProject;
        IFileExplorer fileExplorer = project?.FileExplorer;
        IContentPreview preview = project?.ContentPreviewer;

        if ((fileExplorer?.RefreshCommand is null) || (!fileExplorer.RefreshCommand.CanExecute(null)))
        {
            return;
        }

        await fileExplorer.RefreshCommand.ExecuteAsync(null);

        if ((project.SaveProjectMetadataCommand is not null) && (project.SaveProjectMetadataCommand.CanExecute(null)))
        {
            project.SaveProjectMetadataCommand.Execute(null);
        }

        if ((preview?.ResetPreviewCommand is null) || (preview?.RefreshPreviewCommand is null))
        {
            return;
        }

        if (preview.ResetPreviewCommand.CanExecute(null))
        {
            await preview.ResetPreviewCommand.ExecuteAsync(null);
        }

        IFile file = fileExplorer.SelectedFiles.Count > 0 ? fileExplorer.SelectedFiles[0] : null;

        if ((file is null) || (!preview.RefreshPreviewCommand.CanExecute(file.FullPath)))
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

        if ((e.Ribbon is not null) && (e.Ribbon.RibbonTabs.Count > 0))
        {
            firstTab = e.Ribbon.RibbonTabs[0];
        }

        _ribbonMerger.Merge(e.Ribbon);

        // Default to the first tab on the joined ribbon.
        if (firstTab is not null)
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
        IFileExplorer fileExplorer = ViewModel?.CurrentProject?.FileExplorer;

        try
        {
            DeleteArgs args = new(fileExplorer.Root.ID);
            if ((fileExplorer?.DeleteDirectoryCommand is null) || (!fileExplorer.DeleteDirectoryCommand.CanExecute(args)))
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
        if ((ViewModel?.OpenPackFileCommand is null) || (!ViewModel.OpenPackFileCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.OpenPackFileCommand.Execute(null);
    }

    /// <summary>Handles the BrowseClicked event of the StageLive control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [EventArgs] instance containing the event data.</param>
    private void StageLive_BrowseClicked(object sender, EventArgs e)
    {
        if ((ViewModel?.BrowseProjectCommand is null) || (!ViewModel.BrowseProjectCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.BrowseProjectCommand.Execute(null);
    }

    /// <summary>
    /// Handles the Activated event of the FormMain control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void FormMain_Activated(object sender, EventArgs e)
    {
        if (ViewModel is null)
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
        if (ViewModel?.CurrentProject is null)
        {
            return;
        }

        SaveEventArgs args = new(false);
        StageLive_Save(this, args);
    }

    /// <summary>Handles the Click event of the ButtonFileSystemPanel control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonFileSystemPanel_Click(object sender, EventArgs e)
    {
        if (ViewModel?.CurrentProject is null)
        {
            return;
        }

        PanelProject.SetFileExplorerVisibility(ButtonFileSystemPanel.Checked);
        ValidateRibbonButtons();
    }

    /// <summary>Handles the Click event of the ButtonFileSystemPreview control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonFileSystemPreview_Click(object sender, EventArgs e)
    {
        if (ViewModel?.CurrentProject is null)
        {
            return;
        }

        PanelProject.SetPreviewVisibility(ButtonFileSystemPreview.Checked);
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
    private void StageLive_BackClicked(object sender, EventArgs e) => NavigateToProjectView(ViewModel);

    /// <summary>
    /// Handles the Save event of the StageLive control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SaveEventArgs" /> instance containing the event data.</param>
    private async void StageLive_Save(object sender, SaveEventArgs e)
    {
        if (ViewModel?.CurrentProject?.SaveProjectToPackFileCommand is null)
        {
            return;
        }

        CancelEventArgs args = new();

        if (!ViewModel.CurrentProject.SaveProjectToPackFileCommand.CanExecute(args))
        {
            NavigateToProjectView(ViewModel);
            return;
        }

        await ViewModel.CurrentProject.SaveProjectToPackFileCommand.ExecuteAsync(args);

        NavigateToProjectView(ViewModel);
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
        IProjectEditor project = ViewModel?.CurrentProject;
        IFileExplorer fileExplorer = project?.FileExplorer;

        if ((project is null) || (fileExplorer is null))
        {
            TabFileSystem.Visible = false;
            RibbonTabEditorTools.Visible = false;
            return;
        }

        TabFileSystem.Visible = true;
        _defaultImportData.Destination = fileExplorer.SelectedDirectory;

        ButtonFileSystemPanel.Enabled = project is not null;
        ButtonFileSystemPreview.Enabled = (ButtonFileSystemPanel.Enabled) && (_settings.ShowFileExplorer);
        ButtonFileSystemNewDirectory.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.CreateDirectoryCommand?.CanExecute(_createDirArgs) ?? false);
        ButtonImport.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.ImportCommand?.CanExecute(_defaultImportData) ?? false);
        ButtonOpenContent.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.OpenContentFileCommand?.CanExecute(null) ?? false);
        PanelProject.FileExplorer.MenuItemDirCreateContent.Available =
        PanelProject.FileExplorer.MenuItemFileCreateContent.Available =
        GroupCreate.Visible = SepCreate.Visible = ViewModel.ContentCreators.Count > 0;
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
                ButtonFileSystemDeleteAll.Visible = (fileExplorer.SelectedDirectory is null) || (fileExplorer.SelectedDirectory == fileExplorer.Root);
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
                PanelProject.FileExplorer.MenuItemDirCreateContent.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer.SelectedDirectory is not null);
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
                PanelProject.FileExplorer.MenuItemFileCreateContent.Enabled = (!PanelProject.FileExplorer.IsRenaming) && (fileExplorer?.SelectedDirectory is not null);
                break;
        }

        foreach (IToolPlugInRibbonButton button in project.ToolButtons.SelectMany(item => item.Value))
        {
            if (!_toolButtons.TryGetValue(button.Name, out KryptonRibbonGroupButton ribButton))
            {
                continue;
            }

            if (button.CanExecute is not null)
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
    /// Function to navigate to the project view control.
    /// </summary>
    /// <param name="dataContext">The data context to use.</param>
    private void NavigateToProjectView(IMain dataContext)
    {
        Stage.Visible = false;
        RibbonMain.Visible = true;
        PanelWorkSpace.Visible = true;
        Text = ViewModel.Text;

        PanelWorkSpace.BringToFront();

        if (PanelProject.ViewModel != dataContext?.CurrentProject)
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
        if (ViewModel?.CurrentProject?.SaveProjectToPackFileCommand is not null)
        {
            CancelEventArgs saveAsArgs = new();
            Stage.CanSavePackedFile = ViewModel.CurrentProject.SaveProjectToPackFileCommand.CanExecute(saveAsArgs);
        }
        else
        {
            Stage.CanSavePackedFile = false;
        }

        Text = string.Empty;
        Stage.IsStartup = false;
        Stage.CanOpen = (ViewModel.OpenPackFileCommand is not null) && (ViewModel.OpenPackFileCommand.CanExecute(null));

        Stage.Visible = true;
        _clipboardContext = null;
        RibbonMain.Visible = false;
        PanelWorkSpace.Visible = false;

        Stage.BringToFront();
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

                if (ViewModel.CurrentProject is not null)
                {
                    ViewModel.CurrentProject.PropertyChanging -= CurrentProject_PropertyChanging;
                    ViewModel.CurrentProject.PropertyChanged -= CurrentProject_PropertyChanged;
                    if (ViewModel.CurrentProject.FileExplorer is not null)
                    {
                        ViewModel.CurrentProject.FileExplorer.FileSystemUpdated -= FileExplorer_FileSystemUpdated;
                        ViewModel.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
                        ViewModel.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
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
                if (string.IsNullOrWhiteSpace(ViewModel.CurrentProject.CommandContext))
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
                RibbonMain.SelectedContext = ViewModel.CurrentProject.CommandContext;

                if (string.IsNullOrWhiteSpace(ViewModel.CurrentProject.CommandContext))
                {
                    if (_prevTabBeforeContext is not null)
                    {
                        RibbonMain.SelectedTab = _prevTabBeforeContext;
                    }
                }
                else
                {
                    // Find the first tab associated with this context.
                    KryptonRibbonTab contextTab = RibbonMain.RibbonTabs.FirstOrDefault(item => string.Equals(ViewModel.CurrentProject.CommandContext, item.ContextName, StringComparison.OrdinalIgnoreCase));
                    if (contextTab is not null)
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
                Text = ViewModel.Text;
                ValidateRibbonButtons();
                break;
            case nameof(IMain.ClipboardContext):
                _clipboardContext = ViewModel.ClipboardContext;
                break;
            case nameof(IMain.CurrentProject):
                NavigateToProjectView(ViewModel);

                if (ViewModel.CurrentProject is null)
                {
                    break;
                }

                ViewModel.CurrentProject.PropertyChanging += CurrentProject_PropertyChanging;
                ViewModel.CurrentProject.PropertyChanged += CurrentProject_PropertyChanged;

                if (ViewModel.CurrentProject.FileExplorer is not null)
                {
                    ViewModel.CurrentProject.FileExplorer.FileSystemUpdated += FileExplorer_FileSystemUpdated;
                    ViewModel.CurrentProject.FileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
                    ViewModel.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
                }

                RibbonMain.SelectedContext = ViewModel.CurrentProject.CommandContext;

                TabFileSystem.Visible = true;
                UpdateToolsTab(ViewModel.CurrentProject.ToolButtons);

                _deleteAllValidationArgs = new DeleteArgs(ViewModel.CurrentProject.FileExplorer.Root.ID);
                ButtonFileSystemPanel.Checked = _settings.ShowFileExplorer;
                ButtonFileSystemPreview.Checked = _settings.ShowContentPreview;
                break;
        }

        ValidateRibbonButtons();
    }

    /// <summary>
    /// Function to remove any icons that create content.
    /// </summary>
    private void RemoveNewIcons()
    {
        ToolStripMenuItem[] newItems = MenuCreate.Items.OfType<ToolStripMenuItem>()
                                       .Concat(PanelProject.FileExplorer.MenuItemDirCreateContent.DropDown.Items.OfType<ToolStripMenuItem>())
                                       .Concat(PanelProject.FileExplorer.MenuItemFileCreateContent.DropDown.Items.OfType<ToolStripMenuItem>())
                                       .ToArray();

        foreach (ToolStripItem item in newItems)
        {
            item.Click -= NewItem_Click;
            item.Dispose();
        }
    }

    /// <summary>
    /// Function to update the icons used for the "new" buttons.
    /// </summary>
    /// <param name="metadata">The metadata for plug ins that can create content.</param>
    private void AddNewIcons(IEnumerable<IContentPlugInMetadata> metadata)
    {
        if (metadata is null)
        {
            return;
        }

        foreach (IContentPlugInMetadata item in metadata.OrderBy(item => item.ContentType))
        {
            string id = item.NewIconID.ToString("N");
            Image icon = item.GetNewIcon();

            if ((MenuCreate.Items.ContainsKey(id))
                || (icon is null))
            {
                continue;
            }

            ToolStripMenuItem menuItem = new(string.Format(Resources.GOREDIT_CREATE_NEW, item.ContentType), icon)
            {
                Name = id,
                Tag = item.NewIconID
            };
            menuItem.Click += NewItem_Click;

            ToolStripMenuItem dirCreateMenuItem = new(string.Format(Resources.GOREDIT_CREATE_NEW, item.ContentType), icon)
            {
                Name = id,
                Tag = item.NewIconID
            };
            dirCreateMenuItem.Click += NewItem_Click;

            ToolStripMenuItem fileCreateMenuItem = new(string.Format(Resources.GOREDIT_CREATE_NEW, item.ContentType), icon)
            {
                Name = id,
                Tag = item.NewIconID
            };
            fileCreateMenuItem.Click += NewItem_Click;

            MenuCreate.Items.Add(menuItem);
            PanelProject.FileExplorer.MenuItemDirCreateContent.DropDown.Items.Add(dirCreateMenuItem);
            PanelProject.FileExplorer.MenuItemFileCreateContent.DropDown.Items.Add(fileCreateMenuItem);
        }

        ValidateRibbonButtons();
    }

    /// <summary>Handles the Click event of the NewItem control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void NewItem_Click(object sender, EventArgs e)
    {
        ToolStripItem item = (ToolStripItem)sender;
        Guid id = (Guid)item.Tag;

        if ((ViewModel?.CurrentProject?.CreateContentCommand is null) || (!ViewModel.CurrentProject.CreateContentCommand.CanExecute(id)))
        {
            return;
        }

        ViewModel.CurrentProject.CreateContentCommand.Execute(id);
    }

    /// <summary>
    /// Function to initialize the form from the data context.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    private void InitializeFromDataContext(IMain dataContext)
    {
        if (dataContext is null)
        {
            _deleteAllValidationArgs = null;
            _clipboardContext = null;
            Text = Resources.GOREDIT_CAPTION_NO_FILE;
            RemoveNewIcons();
            ReleaseToolRibbonItems();
            return;
        }

        Text = string.Empty;
        _clipboardContext = dataContext.CurrentProject?.ClipboardContext;

        if (dataContext.CurrentProject is not null)
        {
            RibbonMain.SelectedContext = dataContext.CurrentProject.CommandContext;
            ButtonFileSystemPanel.Checked = _settings.ShowFileExplorer;
            ButtonFileSystemPreview.Checked = _settings.ShowContentPreview;
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

        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.CurrentProject is not null)
        {
            ViewModel.CurrentProject.PropertyChanging -= CurrentProject_PropertyChanging;
            ViewModel.CurrentProject.PropertyChanged -= CurrentProject_PropertyChanged;

            if (ViewModel.CurrentProject.FileExplorer is not null)
            {
                ViewModel.CurrentProject.FileExplorer.FileSystemUpdated -= FileExplorer_FileSystemUpdated;
                ViewModel.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
                ViewModel.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
            }
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
        ViewModel.PropertyChanging -= DataContext_PropertyChanging;
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
            switch (PanelProject.FileExplorer.ControlContext)
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
        if (_clipboardContext?.CopyDataCommand is null)
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
        if ((_clipboardContext?.PasteDataCommand is null)
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
        if (_clipboardContext?.CopyDataCommand is null)
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
    /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnResizeEnd(EventArgs e)
    {
        if ((WindowState != FormWindowState.Normal) || (ViewModel is null))
        {
            return;
        }

        ViewModel.Settings.WindowBounds = new GorgonRectangle(DesktopBounds.X, DesktopBounds.Y, DesktopBounds.Width, DesktopBounds.Height);
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.</summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Load();

        Stage.CanOpen = (ViewModel.OpenPackFileCommand is not null) && (ViewModel.OpenPackFileCommand.CanExecute(null));

        Focus();
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs" /> that contains the event data. </param>
    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (ViewModel is null)
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

                GorgonRectangle windowDimensions;
                int windowState;
                switch (WindowState)
                {
                    case FormWindowState.Normal:
                    case FormWindowState.Maximized:
                        windowDimensions = new GorgonRectangle(Location.X, Location.Y, Size.Width, Size.Height);
                        windowState = (int)WindowState;
                        break;
                    default:
                        windowDimensions = new GorgonRectangle(RestoreBounds.X, RestoreBounds.Y, RestoreBounds.Width, RestoreBounds.Height);
                        windowState = (int)FormWindowState.Normal;
                        break;
                }

                AppCloseArgs args = new(windowDimensions, windowState);

                // If we don't have anything to handle the shut down, then just shut it all down.
                if ((ViewModel.AppClosingAsyncCommand is null) || (!ViewModel.AppClosingAsyncCommand.CanExecute(args)))
                {
                    _closeFlag = CloseStates.NotClosing;
                    e.Cancel = false;
                    return;
                }

                await ViewModel.AppClosingAsyncCommand.ExecuteAsync(args);

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
            ViewModel = dataContext;

            _waitForm.SetDataContext(ViewModel);
            _progressForm.SetDataContext(ViewModel);

            if (ViewModel is null)
            {
                return;
            }

            if (ViewModel.CurrentProject is not null)
            {
                ViewModel.CurrentProject.PropertyChanging += CurrentProject_PropertyChanging;
                ViewModel.CurrentProject.PropertyChanged += CurrentProject_PropertyChanged;

                if (ViewModel.CurrentProject.FileExplorer is not null)
                {
                    ViewModel.CurrentProject.FileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
                    ViewModel.CurrentProject.FileExplorer.SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
                    ViewModel.CurrentProject.FileExplorer.FileSystemUpdated += FileExplorer_FileSystemUpdated;
                }

                _deleteAllValidationArgs = new DeleteArgs(ViewModel.CurrentProject.FileExplorer.Root.ID);
            }

            ViewModel.PropertyChanged += DataContext_PropertyChanged;
            ViewModel.PropertyChanging += DataContext_PropertyChanging;
        }
        finally
        {
            ValidateRibbonButtons();
        }
    }



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
        _ribbonMerger.FixGroupWidths();
    }

    /// <summary>Initializes a new instance of the <see cref="FormMain"/> class.</summary>
    /// <param name="settings">The settings for the application.</param>
    public FormMain(EditorSettings settings)
        : this() => PanelProject.Settings = _settings = settings;
}
