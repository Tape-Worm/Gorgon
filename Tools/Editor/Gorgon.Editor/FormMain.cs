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
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DX = SharpDX;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.Editor.Views;
using Gorgon.Timing;
using Gorgon.Editor.Rendering;
using Gorgon.Math;
using System.Threading.Tasks;
using System.Diagnostics;
using ComponentFactory.Krypton.Ribbon;
using System.Linq;
using Gorgon.Editor.Content;
using System.Collections.Generic;
using Gorgon.Editor.Plugins;
using System.Collections.Specialized;

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
        // The current action to use when cancelling an operation in progress.
        private Action _progressCancelAction;
        // The timer used to indicate how fast messages can be sent to the progress meter.
        private IGorgonTimer _progressTimer;
        // The current synchronization context.
        private readonly SynchronizationContext _syncContext;
        // The context for a clipboard handler object.
        private IClipboardHandler _clipboardContext;
        // The flag to indicate that the application is already closing.
        private CloseStates _closeFlag;
        // The ribbon merger for the main ribbon.
        private readonly IRibbonMerger _ribbonMerger;
		// The currently selected tab prior to showing a context tab.
        private KryptonRibbonTab _prevTabBeforeContext;
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
        /// <summary>Handles the Click event of the ButtonOpenContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonOpenContent_Click(object sender, EventArgs e)
        {
            IProjectVm project = DataContext?.CurrentProject;
            IFileExplorerVm fileExplorer = project?.FileExplorer;
            var currentNode = fileExplorer?.SelectedNode as IContentFile;

            if ((fileExplorer?.OpenContentFileCommand == null) || (!fileExplorer.OpenContentFileCommand.CanExecute(currentNode)))
            {
                return;
            }

            fileExplorer.OpenContentFileCommand.Execute(currentNode);
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
        private void ButtonExport_Click(object sender, EventArgs e)
        {
            try
            {
                IFileExplorerVm fileExplorer = DataContext?.CurrentProject?.FileExplorer;

                if ((fileExplorer?.ExportNodeToCommand == null) 
                    || (!fileExplorer.ExportNodeToCommand.CanExecute(fileExplorer.SelectedNode ?? fileExplorer.RootNode)))
                {
                    return;
                }

                fileExplorer.ExportNodeToCommand.Execute(fileExplorer.SelectedNode ?? fileExplorer.RootNode);
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
                await PanelProject.FileExplorer.ImportFilesAsync();
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
            IFileExplorerVm fileExplorer = DataContext?.CurrentProject?.FileExplorer;

            try
            {
                if ((fileExplorer?.DeleteFileSystemCommand == null) || (!fileExplorer.DeleteFileSystemCommand.CanExecute(null)))
                {
                    return;
                }

                fileExplorer.DeleteFileSystemCommand.Execute(null);
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

        /// <summary>
        /// Handles the RenameBegin event of the PanelProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelProject_RenameBegin(object sender, EventArgs e) => ButtonFileSystemRename.Enabled = false;

        /// <summary>
        /// Handles the RenameEnd event of the PanelProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelProject_RenameEnd(object sender, EventArgs e) => ValidateRibbonButtons();

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
            if (DataContext == null)
            {
                return;
            }

            var args = new SavePackFileArgs(DataContext.CurrentProject);

            if ((DataContext?.SavePackFileCommand == null) || (!DataContext.SavePackFileCommand.CanExecute(args)))
            {
                NavigateToProjectView(DataContext);
                return;
            }

            await DataContext.SavePackFileCommand.ExecuteAsync(args);

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
            IProjectVm project = DataContext?.CurrentProject;
            IFileExplorerVm fileExplorer = project?.FileExplorer;

            if ((project == null) || (fileExplorer == null))
            {
                TabFileSystem.Visible = false;                
                return;
            }

            TabFileSystem.Visible = true;

            ButtonFileSystemNewDirectory.Enabled = (fileExplorer.CreateNodeCommand != null)
                                                && (fileExplorer.CreateNodeCommand.CanExecute(null));

            ButtonFileSystemRename.Enabled = (fileExplorer.RenameNodeCommand != null)
                                            && (fileExplorer.SelectedNode != null);

            ButtonExpand.Enabled = (fileExplorer.SelectedNode != null) && (fileExplorer.SelectedNode.Children.Count > 0);
            ButtonCollapse.Enabled = (fileExplorer.SelectedNode != null) && (fileExplorer.SelectedNode.Children.Count > 0);
                        
            ButtonFileSystemDelete.Visible = fileExplorer.SelectedNode != null;
            ButtonFileSystemDelete.Enabled = (fileExplorer.DeleteNodeCommand != null)
                                            && (fileExplorer.DeleteNodeCommand.CanExecute(null));
            ButtonFileSystemDeleteAll.Visible = fileExplorer.SelectedNode == null;
            ButtonFileSystemDeleteAll.Enabled = fileExplorer.DeleteFileSystemCommand?.CanExecute(null) ?? false;

            ButtonImport.Enabled = fileExplorer.ImportIntoNodeCommand?.CanExecute(fileExplorer.SelectedNode ?? fileExplorer.RootNode) ?? false;
            ButtonExport.Enabled = fileExplorer.ExportNodeToCommand?.CanExecute(fileExplorer.SelectedNode ?? fileExplorer.RootNode) ?? false;

            ButtonOpenContent.Enabled = fileExplorer.OpenContentFileCommand?.CanExecute(fileExplorer.SelectedNode as IContentFile) ?? false;

            ButtonFileSystemCopy.Enabled = _clipboardContext?.CanCopy() ?? false;
            ButtonFileSystemCut.Enabled = _clipboardContext?.CanCut() ?? false;
            ButtonFileSystemPaste.Enabled = _clipboardContext?.CanPaste() ?? false;

            GroupCreate.Visible = SepCreate.Visible = (DataContext.CreateContentCommand != null) 
                                                        && (DataContext.CreateContentCommand.CanExecute(null));
        }

        /// <summary>
        /// Function to ensure that the wait panel stays on top if it is active.
        /// </summary>
        private void KeepWaitPanelOnTop()
        {
            if (ProgressScreen.Visible)
            {
                ProgressScreen.BringToFront();
            }

            if (!WaitScreen.Visible)
            {
                return;
            }

            WaitScreen.BringToFront();
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
            if (DataContext?.SavePackFileCommand != null)
            {
                var saveAsArgs = new SavePackFileArgs(DataContext.CurrentProject);
                Stage.CanSaveAs = DataContext.SavePackFileCommand.CanExecute(saveAsArgs);
            }
            else
            {
                Stage.CanSaveAs = false;
            }

            Text = string.Empty;
            Stage.IsStartup = false;
            Stage.CanOpen = (DataContext.BrowseProjectCommand != null) && (DataContext.BrowseProjectCommand.CanExecute(null));

            Stage.Visible = true;
            _clipboardContext = null;
            RibbonMain.Visible = false;
            PanelWorkSpace.Visible = false;

            Stage.BringToFront();

            KeepWaitPanelOnTop();
        }

        /// <summary>
        /// Handles the ProgressDeactivated event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DataContext_ProgressDeactivated(object sender, EventArgs e)
        {
            _progressCancelAction = null;
            ProgressScreen.Visible = false;
            ProgressScreen.OperationCancelled -= ProgressScreen_OperationCancelled;
            _progressTimer = null;
        }

        /// <summary>
        /// Datas the context progress updated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DataContext_ProgressUpdated(object sender, ProgressPanelUpdateArgs e)
        {
            // Drop the message if it's been less than 10 milliseconds since our last call.
            // This will keep us from flooding the window message queue with very fast operations.            
            if ((_progressTimer != null) && (_progressTimer.Milliseconds < 10) && (e.PercentageComplete < 1) && (e.PercentageComplete > 0))
            {
                return;
            }

            // The actual method to execute on the main thread.
            void UpdateProgressPanel(ProgressPanelUpdateArgs args)
            {
                ProgressScreen.ProgressTitle = e.Title ?? string.Empty;
                ProgressScreen.ProgressMessage = e.Message ?? string.Empty;
                ProgressScreen.CurrentValue = e.PercentageComplete;
                ProgressScreen.AllowCancellation = e.CancelAction != null;
                _progressCancelAction = args.CancelAction;

                bool isMarquee = ProgressScreen.MeterStyle == ProgressBarStyle.Marquee;

                if (ProgressScreen.Visible)
                {
                    if (isMarquee == e.IsMarquee)
                    {
                        _progressTimer?.Reset();
                        return;
                    }

                    // If we change to a marquee format, then hide the previous panel.
                    DataContext_ProgressDeactivated(this, EventArgs.Empty);
                }

                _progressTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerQpc() : new GorgonTimerMultimedia();                
                ProgressScreen.OperationCancelled += ProgressScreen_OperationCancelled;
                ProgressScreen.MeterStyle = e.IsMarquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                ProgressScreen.Visible = true;
                ProgressScreen.BringToFront();
                ProgressScreen.Invalidate();
            }

            if (InvokeRequired)
            {
                _syncContext.Send(args => UpdateProgressPanel((ProgressPanelUpdateArgs)args), e);
                return;
            }

            UpdateProgressPanel(e);
        }

        /// <summary>
        /// Handles the OperationCancelled event of the ProgressScreen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ProgressScreen_OperationCancelled(object sender, EventArgs e) => _progressCancelAction?.Invoke();

        /// <summary>
        /// Handles the WaitPanelDeactivated event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DataContext_WaitPanelDeactivated(object sender, EventArgs e) => WaitScreen.Visible = false;

        /// <summary>
        /// Handles the WaitPanelActivated event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WaitPanelActivateArgs"/> instance containing the event data.</param>
        private void DataContext_WaitPanelActivated(object sender, WaitPanelActivateArgs e)
        {
            WaitScreen.WaitMessage = e.Message;

            if (!string.IsNullOrWhiteSpace(e.Title))
            {
                WaitScreen.WaitTitle = e.Title;
            }

            if (WaitScreen.Visible)
            {
                return;
            }

            WaitScreen.Visible = true;
            WaitScreen.BringToFront();
            WaitScreen.Invalidate();
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
                    RibbonMain.SelectedContext = string.Empty;

                    if (DataContext.CurrentProject != null)
                    {
                        DataContext.CurrentProject.PropertyChanging -= CurrentProject_PropertyChanging;
                        DataContext.CurrentProject.PropertyChanged -= CurrentProject_PropertyChanged;
                        if (DataContext.CurrentProject.FileExplorer != null)
                        {
                            DataContext.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
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
                case nameof(IProjectVm.CommandContext):
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
                case nameof(IProjectVm.CommandContext):
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
        /// Handles the PropertyChanged event of the FileExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void FileExplorer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorerVm.SearchResults):
                case nameof(IUndoHandler.UndoCommand):
                case nameof(IUndoHandler.RedoCommand):
                case nameof(IFileExplorerVm.SelectedNode):
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
                    if (_clipboardContext != null)
                    {
                        _clipboardContext.DataUpdated -= ClipboardContext_DataUpdated;
                    }

                    _clipboardContext = DataContext.ClipboardContext;

                    if (_clipboardContext != null)
                    {
                        _clipboardContext.DataUpdated += ClipboardContext_DataUpdated;
                    }
                    break;
                case nameof(IMain.CurrentProject):
                    NavigateToProjectView(DataContext);

                    if (DataContext.CurrentProject != null)
                    {
                        DataContext.CurrentProject.PropertyChanging += CurrentProject_PropertyChanging;
                        DataContext.CurrentProject.PropertyChanged += CurrentProject_PropertyChanged;

                        if (DataContext.CurrentProject.FileExplorer != null)
                        {
                            DataContext.CurrentProject.FileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
                        }

                        RibbonMain.SelectedContext = DataContext.CurrentProject.CommandContext;
                    }
                    break;
            }

            ValidateRibbonButtons();
        }

        /// <summary>
        /// Function to update the icons used for the "new" buttons.
        /// </summary>
        /// <param name="metadata">The metadata for plug ins that can create content.</param>
        private void AddNewIcons(IEnumerable<IContentPluginMetadata> metadata)
        {
            if (metadata == null)
            {                
                return;
            }

            foreach (IContentPluginMetadata item in metadata)
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
                    Name = id
                };
                menuItem.Click += NewItem_Click;

                MenuCreate.Items.Add(menuItem);                
            }

            ValidateRibbonButtons();
        }
        
        /// <summary>
        /// Function to update the icons used for the "new" buttons.
        /// </summary>
        /// <param name="metadata">The metadata for plug ins that can create content.</param>
        private void RemoveNewIcons(IEnumerable<IContentPluginMetadata> metadata)
        {
            if (metadata == null)
            {
                return;
            }

            foreach (IContentPluginMetadata item in metadata)
            {
                string id = item.NewIconID.ToString("N");

                if (!MenuCreate.Items.ContainsKey(id))
                {
                    continue;
                }

                ToolStripItem menuItem = MenuCreate.Items[id];
                MenuCreate.Items.RemoveByKey(id);
                menuItem.Click -= NewItem_Click;
                menuItem.Dispose();
            }

            ValidateRibbonButtons();
        }

        /// <summary>
        /// Function to clear the new icons list.
        /// </summary>
        private void ClearNewIcons()
        {
            ToolStripItem[] items = MenuCreate.Items.OfType<ToolStripItem>().ToArray();
            MenuCreate.Items.Clear();

            for (int i = 0; i < items.Length; ++i)
            {
                items[i]?.Dispose();
            }

            ValidateRibbonButtons();
        }

        /// <summary>Handles the Click event of the NewItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NewItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;

            if ((DataContext?.CreateContentCommand == null) || (!DataContext.CreateContentCommand.CanExecute(item.Name)))
            {
                return;
            }

            DataContext.CreateContentCommand.Execute(item.Name);
        }

        /// <summary>
        /// Function to initialize the form from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(IMain dataContext)
        {
            if (dataContext == null)
            {
                _clipboardContext = null;
                Text = Resources.GOREDIT_CAPTION_NO_FILE;
                return;
            }

            Text = string.Empty;
            _clipboardContext = dataContext.CurrentProject?.ClipboardContext;

            if (_clipboardContext != null)
            {
                _clipboardContext.DataUpdated += ClipboardContext_DataUpdated;
            }

            if (dataContext.CurrentProject != null)
            {
                RibbonMain.SelectedContext = dataContext.CurrentProject.CommandContext;
            }

            AddNewIcons(dataContext.ContentCreators);            
        }

        /// <summary>Handles the CollectionChanged event of the ContentCreators control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void ContentCreators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNewIcons(e.NewItems.OfType<IContentPluginMetadata>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveNewIcons(e.OldItems.OfType<IContentPluginMetadata>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearNewIcons();
                    break;
            }
        }

        /// <summary>
        /// Function to unassign the events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            _progressCancelAction = null;
            ProgressScreen.OperationCancelled -= ProgressScreen_OperationCancelled;

            if (DataContext == null)
            {
                return;
            }

            if (DataContext.ContentCreators != null)
            {
                DataContext.ContentCreators.CollectionChanged -= ContentCreators_CollectionChanged;
            }

            if (_clipboardContext != null)
            {
                _clipboardContext.DataUpdated -= ClipboardContext_DataUpdated;
            }

            if (DataContext.CurrentProject != null)
            {
                DataContext.CurrentProject.PropertyChanging -= CurrentProject_PropertyChanging;
                DataContext.CurrentProject.PropertyChanged -= CurrentProject_PropertyChanged;

                if (DataContext.CurrentProject.FileExplorer != null)
                {
                    DataContext.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
                }
            }

            DataContext.ProgressUpdated -= DataContext_ProgressUpdated;
            DataContext.ProgressDeactivated -= DataContext_ProgressDeactivated;
            DataContext.WaitPanelActivated -= DataContext_WaitPanelActivated;
            DataContext.WaitPanelDeactivated -= DataContext_WaitPanelDeactivated;
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
                PanelProject.FileExplorer.RenameNode();
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
                PanelProject.FileExplorer.Delete();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonExpand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonExpand_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.Expand();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonCollapse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCollapse_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.Collapse();
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
            if (_clipboardContext == null)
            {
                return;
            }

            _clipboardContext.Cut();
        }


        /// <summary>
        /// Handles the Click event of the ButtonFileSystemPaste control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemPaste_Click(object sender, EventArgs e)
        {
            if (_clipboardContext == null)
            {
                return;
            }

            _clipboardContext.Paste();
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemCopy_Click(object sender, EventArgs e)
        {
            if (_clipboardContext == null)
            {
                return;
            }

            _clipboardContext.Copy();
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

            Stage.CanOpen = (DataContext.BrowseProjectCommand != null) && (DataContext.BrowseProjectCommand.CanExecute(null));            
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
                Stage.IsStartup = true;

                InitializeFromDataContext(dataContext);
                DataContext = dataContext;

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
                    }
                }

                DataContext.ContentCreators.CollectionChanged += ContentCreators_CollectionChanged;
                DataContext.ProgressUpdated += DataContext_ProgressUpdated;
                DataContext.ProgressDeactivated += DataContext_ProgressDeactivated;
                DataContext.WaitPanelActivated += DataContext_WaitPanelActivated;
                DataContext.WaitPanelDeactivated += DataContext_WaitPanelDeactivated;
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
            }

            _syncContext = SynchronizationContext.Current;            
            RibbonMain.AllowFormIntegrate = false;
            PanelProject.MainRibbon = RibbonMain;
        }
        #endregion
    }
}
