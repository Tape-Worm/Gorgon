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
// Created: September 4, 2018 12:46:15 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The type of project item stored in the project metadata.
    /// </summary>
    internal enum ProjectItemType
    {
        /// <summary>
        /// The project item is a file.
        /// </summary>
        File = 0,
        /// <summary>
        /// The project item is a directory.
        /// </summary>
        Directory = 1
    }

    /// <summary>
    /// The view model for the project editor interface.
    /// </summary>
    internal class ProjectEditor
        : ViewModelBase<ProjectEditorParameters, IHostContentServices>, IProjectEditor
    {
        #region Constants.        
        /// <summary>
        /// Metadata naming for the project item type attribute.
        /// </summary>
        public const string ProjectItemTypeAttrName = "ProjectItemType";
        #endregion

        #region Variables.
        // The factory used to create view models.
        private ViewModelFactory _viewModelFactory;
        // The project data for the view model.
        private IProject _projectData;
        // The file explorer view model.
        private IFileExplorer _fileExplorer;
        // The application project manager.
        private ProjectManager _projectManager;
        // The currently active content.
        private IEditorContent _currentContent;
        // The content previewer view model.
        private IContentPreview _contentPreviewer;
        // The file manager used to manage content through content plug ins.
        private IContentFileManager _contentFileManager;
        // The list of tool buttons.
        private Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> _toolButtons = new Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>>(StringComparer.CurrentCultureIgnoreCase);
        // The settings for the application.
        private Editor.EditorSettings _settings;
        // The project save dialog service.
        private EditorFileSaveDialogService _saveDialog;
        // The list of plug ins that can create content.
        private IReadOnlyList<IContentPlugInMetadata> _contentCreators;
        // The current clipboard context.
        private IClipboardHandler _clipboardContext;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the available tool plug in button definitions for the application.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> ToolButtons => _toolButtons;

        /// <summary>
        /// Property to set or return the current clipboard context depending on content.
        /// </summary>
        public IClipboardHandler ClipboardContext
        {
            get => _clipboardContext;
            set
            {
                if (_clipboardContext == value)
                {
                    return;
                }

                OnPropertyChanging();
                _clipboardContext = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the current content for the project.</summary>
        public IEditorContent CurrentContent
        {
            get => _currentContent;
            private set
            {
                if (_currentContent == value)
                {
                    return;
                }

                if (_currentContent != null)
                {
                    _currentContent.PropertyChanging -= CurrentContent_PropertyChanging;
                    _currentContent.PropertyChanged -= CurrentContent_PropertyChanged;
                    _currentContent.WaitPanelActivated -= FileExplorer_WaitPanelActivated;
                    _currentContent.WaitPanelDeactivated -= FileExplorer_WaitPanelDeactivated;
                    _currentContent.ProgressUpdated -= FileExplorer_ProgressUpdated;
                    _currentContent.ProgressDeactivated -= FileExplorer_ProgressDeactivated;
                }

                OnPropertyChanging();
                _currentContent = value;
                OnPropertyChanged();

                if (_currentContent != null)
                {
                    _currentContent.PropertyChanging += CurrentContent_PropertyChanging;
                    _currentContent.PropertyChanged += CurrentContent_PropertyChanged;
                    _currentContent.WaitPanelActivated += FileExplorer_WaitPanelActivated;
                    _currentContent.WaitPanelDeactivated += FileExplorer_WaitPanelDeactivated;
                    _currentContent.ProgressUpdated += FileExplorer_ProgressUpdated;
                    _currentContent.ProgressDeactivated += FileExplorer_ProgressDeactivated;
                }

                NotifyPropertyChanged(nameof(CommandContext));
            }
        }

        /// <summary>
        /// Property to set or return the content previewer.
        /// </summary>
        public IContentPreview ContentPreviewer
        {
            get => _contentPreviewer;
            private set
            {
                if (_contentPreviewer == value)
                {
                    return;
                }

                UnassignEvents();

                OnPropertyChanging();
                _contentPreviewer = value;
                OnPropertyChanged();

                AssignEvents();
            }
        }

        /// <summary>
        /// Property to set or return the content file manager for managing content file systems through content plug ins.
        /// </summary>
        public IContentFileManager ContentFileManager
        {
            get => _contentFileManager;
            private set
            {
                if (_contentFileManager == value)
                {
                    return;
                }

                OnPropertyChanging();
                _contentFileManager = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the file explorer view model for use with the file explorer subview.
        /// </summary>
        public IFileExplorer FileExplorer
        {
            get => _fileExplorer;
            private set
            {
                if (_fileExplorer == value)
                {
                    return;
                }

                UnassignEvents();

                OnPropertyChanging();
                _fileExplorer = value;
                OnPropertyChanged();

                AssignEvents();
            }
        }

        /// <summary>
        /// Property to set or return the title for the project.
        /// </summary>
        public string ProjectTitle { get; private set; } = Resources.GOREDIT_NEW_PROJECT;

        /// <summary>Property to return the current command context.</summary>
        public string CommandContext => CurrentContent?.CommandContext?.Name ?? string.Empty;

        /// <summary>
        /// Property to return the command to execute when the application is closing.
        /// </summary>
        public IEditorAsyncCommand<CancelEventArgs> BeforeCloseCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute after the project is closed.
        /// </summary>
        public IEditorCommand<object> AfterCloseCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to save the project metadata.
        /// </summary>
        public IEditorCommand<object> SaveProjectMetadataCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the file explorer is shown or not.
        /// </summary>
        public bool ShowFileExplorer
        {
            get => _settings.ShowFileExplorer;
            set
            {
                if (_settings.ShowFileExplorer == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.ShowFileExplorer = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return whether the content preview is shown or not.
        /// </summary>
        public bool ShowContentPreview
        {
            get => _settings.ShowContentPreview;
            set
            {
                if (_settings.ShowContentPreview == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.ShowContentPreview = value;

                if (ContentPreviewer != null)
                {
                    ContentPreviewer.IsEnabled = _settings.ShowContentPreview;
                }
                OnPropertyChanged();

                if (ContentPreviewer == null)
                {
                    return;
                }

                void ResetContentPreviewer()
                {
                    if ((ContentPreviewer.ResetPreviewCommand != null) && (ContentPreviewer.ResetPreviewCommand.CanExecute(null)))
                    {
                        ContentPreviewer.ResetPreviewCommand.Execute(null);
                    }
                }

                try
                {
                    if ((FileExplorer == null) || (FileExplorer.SelectedFiles.Count == 0))
                    {
                        ResetContentPreviewer();
                        return;
                    }

                    if (value)
                    {
                        RefreshFilePreview(FileExplorer.SelectedFiles[0]?.FullPath);
                    }
                    else
                    {                            
                        ResetContentPreviewer();
                    }
                }
                catch(Exception ex)
                {
                    HostServices.Log.Print("Error loading preview", LoggingLevel.Simple);
                    HostServices.Log.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Property to set or return distance for the file explorer and main area.
        /// </summary>
        public double FileExplorerDistance
        {
            get => _settings.SplitMainDistance.Min(0.90).Max(0.05);
            set
            {
                if (_settings.SplitMainDistance.EqualsEpsilon(value))
                {
                    return;
                }

                OnPropertyChanging();
                _settings.SplitMainDistance = value.Min(0.90).Max(0.05);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the distance for the preview and the file explorer.
        /// </summary>
        public double PreviewDistance
        {
            get => _settings.SplitPreviewDistance.Min(0.90).Max(0.05);
            set
            {
                if (_settings.SplitPreviewDistance.EqualsEpsilon(value))
                {
                    return;
                }

                OnPropertyChanging();
                _settings.SplitPreviewDistance = value.Min(0.90).Max(0.05);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command used to save the project to a packed file.
        /// </summary>
        public IEditorAsyncCommand<CancelEventArgs> SaveProjectToPackFileCommand
        {
            get;            
        }


        /// <summary>
        /// Property to return the command used to create content.
        /// </summary>
        public IEditorAsyncCommand<Guid> CreateContentCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command executed when the currently active content is closed.
        /// </summary>
        public IEditorCommand<object> ContentClosedCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to persist the project metadata to the disk.
        /// </summary>
        private void SaveProjectMetadata()
        {
            // Ensure we have a blank slate.
            _projectData.ProjectItems.Clear();

            // Persists the file metadata for a given directory.
            void SaveMetadata(IDirectory directory)
            {
                ProjectItemMetadata metadata;

                // There is no reason to persist the root, it is there, it is everwhere at once.
                if (directory != _fileExplorer.Root)
                {
                    // For now, we don't have any directory metadata.
                    metadata = new ProjectItemMetadata();
                    metadata.Attributes[ProjectItemTypeAttrName] = ProjectItemType.Directory.ToString();
                    _projectData.ProjectItems[directory.FullPath] = metadata;
                }

                // Rebuild item metdata list.
                foreach (IFile file in directory.Files)
                {
                    metadata = new ProjectItemMetadata(file.Metadata);
                    metadata.Attributes[ProjectItemTypeAttrName] = ProjectItemType.File.ToString();
                    _projectData.ProjectItems[file.FullPath] = metadata;

                    // Copy the dependency data for each file.
                    foreach (KeyValuePair<string, List<string>> dependency in file.Metadata.DependsOn)
                    {
                        foreach (string path in dependency.Value)
                        {
                            // Ensure that the file we are dependent upon is still available, if it's not, then there's no need to record it.
                            if ((!string.IsNullOrWhiteSpace(path)) && (ContentFileManager.FileExists(path)))
                            {
                                metadata.DependsOn[dependency.Key] = dependency.Value;
                            }
                        }
                    }
                }
            }

            // Spit out the root directory files.
            SaveMetadata(_fileExplorer.Root);

            // Evaluate each directory in the file system.
            foreach (IDirectory directory in _fileExplorer.Root.Directories.Traverse(d => d.Directories))
            {
                SaveMetadata(directory);
            }

            // Finally, persist to the database.
            _projectManager.PersistMetadata(_projectData, CancellationToken.None);
        }

        /// <summary>
        /// Function to reset the content property and persist the project metadata.
        /// </summary>
        private void ResetContent()
        {
            SaveProjectMetadata();
            CurrentContent?.OnUnload();
            CurrentContent = null;
        }

        /// <summary>
        /// Function to rebuild the list of sorted ribbon buttons.
        /// </summary>
        private void GetTools()
        {
            NotifyPropertyChanging(nameof(ToolButtons));

            var result = new Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>>(StringComparer.CurrentCultureIgnoreCase);

            foreach (KeyValuePair<string, ToolPlugIn> plugin in HostServices.ToolPlugInService.PlugIns)
            {
                IToolPlugInRibbonButton button = plugin.Value.GetToolButton(_projectData, _contentFileManager);
                button.ValidateButton();

                List<IToolPlugInRibbonButton> buttons;
                if (result.TryGetValue(button.GroupName, out IReadOnlyList<IToolPlugInRibbonButton> roButtons))
                {
                    // This is safe because this is the implementation.
                    buttons = (List<IToolPlugInRibbonButton>)roButtons;
                }
                else
                {
                    result[button.GroupName] = buttons = new List<IToolPlugInRibbonButton>();
                }

                buttons.Add(button);
            }

            _toolButtons = result;

            NotifyPropertyChanged(nameof(ToolButtons));
        }

        /// <summary>
        /// Function to save the metadata for the project when the file system changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void FileExplorer_FileSystemUpdated(object sender, EventArgs e) => DoSaveProjectMetadata();

        /// <summary>
        /// Function to perform actions after the current content is closed.
        /// </summary>
        private void DoContentClosed()
        {
            HostServices.BusyService.SetBusy();
            try
            {
                if (CurrentContent?.File != null)
                {
                    RefreshFilePreview(CurrentContent.File.Path);
                }

                ResetContent();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CLOSING_CONTENT);
            }
            finally
            {
                HostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Handles the ProgressDeactivated event of the FileExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FileExplorer_ProgressDeactivated(object sender, EventArgs e) => HideProgress();

        /// <summary>
        /// Function called when the progress panel is shown or updated.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void FileExplorer_ProgressUpdated(object sender, ProgressPanelUpdateArgs e) => UpdateProgress(e);

        /// <summary>
        /// Function called to deactivate the wait panel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event parameters.</param>
        private void FileExplorer_WaitPanelDeactivated(object sender, EventArgs e) => HideWaitPanel();

        /// <summary>
        /// Function called to activate the wait panel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event parameters.</param>
        private void FileExplorer_WaitPanelActivated(object sender, WaitPanelActivateArgs e) => ShowWaitPanel(e.Message, e.Title);

        /// <summary>Handles the PropertyChanging event of the CurrentContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void CurrentContent_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IEditorContent.CommandContext):
                    NotifyPropertyChanging(nameof(CommandContext));
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the CurrentContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentContent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IEditorContent.ContentState):
                    if (CurrentContent.ContentState == ContentState.Unmodified)
                    {
                        // If the state turns to unmodified, then refresh the thumbnail.
                        RefreshFilePreview(CurrentContent.File.Path);                            
                    }
                    break;
                case nameof(IEditorContent.CommandContext):
                    NotifyPropertyChanged(nameof(CommandContext));
                    break;
            }
        }

        /// <summary>
        /// Function to assign events for the child view models.
        /// </summary>
        private void AssignEvents()
        {
            UnassignEvents();

            if (FileExplorer == null)
            {
                return;
            }

            FileExplorer.WaitPanelActivated += FileExplorer_WaitPanelActivated;
            FileExplorer.WaitPanelDeactivated += FileExplorer_WaitPanelDeactivated;
            FileExplorer.ProgressUpdated += FileExplorer_ProgressUpdated;
            FileExplorer.ProgressDeactivated += FileExplorer_ProgressDeactivated;
        }

        /// <summary>
        /// Function to unassign events from the child view models.
        /// </summary>
        private void UnassignEvents()
        {
            if (_currentContent != null)
            {
                CurrentContent.WaitPanelActivated -= FileExplorer_WaitPanelActivated;
                CurrentContent.WaitPanelDeactivated -= FileExplorer_WaitPanelDeactivated;
                CurrentContent.ProgressUpdated -= FileExplorer_ProgressUpdated;
                CurrentContent.ProgressDeactivated -= FileExplorer_ProgressDeactivated;
            }

            if (FileExplorer == null)
            {
                return;
            }

            FileExplorer.ProgressUpdated -= FileExplorer_ProgressUpdated;
            FileExplorer.ProgressDeactivated -= FileExplorer_ProgressDeactivated;
            FileExplorer.WaitPanelActivated -= FileExplorer_WaitPanelActivated;
            FileExplorer.WaitPanelDeactivated -= FileExplorer_WaitPanelDeactivated;
        }

        /// <summary>
        /// Function to force a refresh of the specified file preview.
        /// </summary>
        /// <param name="filePath">The path to the file to refresh.</param>
        private void RefreshFilePreview(string filePath)
        {
            if (!ShowContentPreview)
            {
                return;
            }

            if ((ContentPreviewer.RefreshPreviewCommand != null) && (ContentPreviewer.RefreshPreviewCommand.CanExecute(filePath)))
            {
                ContentPreviewer.RefreshPreviewCommand.Execute(filePath);
            }
        }

        /// <summary>
        /// Function to persist the changed content (if any).
        /// </summary>
        /// <param name="saveReason">The reason why the content is being saved.</param>
        /// <returns><b>true</b> if saved or skipped, <b>false</b> if cancelled.</returns>
        private async Task<bool> UpdateChangedContentAsync(SaveReason saveReason)
        {
            if ((CurrentContent == null)
                || (CurrentContent.SaveContentCommand == null)
                || (!CurrentContent.SaveContentCommand.CanExecute(saveReason)))
            {
                return true;
            }

            MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_SAVE_CONTENT, CurrentContent.File.Name), allowCancel: true);

            switch (response)
            {
                case MessageResponse.Yes:
                    if ((CurrentContent.SaveContentCommand != null) && (CurrentContent.SaveContentCommand.CanExecute(saveReason)))
                    {
                        await CurrentContent.SaveContentCommand.ExecuteAsync(saveReason);

                        if (_contentPreviewer != null)
                        {
                            // Wait for the previewer to finish its load operation (if the app is closing it'll destroy the area where the thumbnails are saved, so we'll need to ensure
                            // it doesn't wipe those directories away until after the preview is complete).
                            await _contentPreviewer.LoadingTask;
                        }

                        // Refresh the preview after the user has saved, or the content pane was closed. Don't bother refreshing when closing the project itself, there's no need.
                        if (saveReason != SaveReason.AppProjectShutdown)
                        {
                            RefreshFilePreview(CurrentContent.File.Path);
                        }
                    }

                    break;
                case MessageResponse.Cancel:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Function to save the project metadata.
        /// </summary>
        private void DoSaveProjectMetadata()
        {
            HostServices.BusyService.SetBusy();

            try
            {
                SaveProjectMetadata();
            }
            catch (Exception ex)
            {
                HostServices.Log.Print("[ERROR] Could not save the project metadata due to an exception!!", LoggingLevel.Simple);
                HostServices.Log.LogException(ex);
            }
            finally
            {
                HostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Function called when the application is shutting down.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoBeforeCloseAsync(CancelEventArgs args)
        {
            try
            {
                bool result = await UpdateChangedContentAsync(SaveReason.AppProjectShutdown);
                
                SaveProjectMetadata();

                args.Cancel = !result;
            }
            catch (Exception ex)
            {
                HostServices.Log.Print("Error closing the application.", LoggingLevel.Simple);
                HostServices.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function called after the project is closed.
        /// </summary>
        private void DoAfterClose()
        {
            try
            {
                _projectManager.CloseProject(_projectData);
            }
            catch (Exception ex)
            {
                HostServices.Log.Print("Error closing the application.", LoggingLevel.Simple);
                HostServices.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to determine whether the content can be opened or not.
        /// </summary>
        /// <param name="filePath">The path for the file being opened.</param>
        /// <returns><b>true</b> if the node can be opened, <b>false</b> if not.</returns>
        private bool CanOpenContent()
        {
            if ((FileExplorer == null) || (FileExplorer.SelectedFiles.Count == 0))
            {
                return false;
            }

            IContentFile file = _contentFileManager.GetFile(FileExplorer.SelectedFiles[0].FullPath);
            return file.Metadata.ContentMetadata != null;
        }

        /// <summary>
        /// Function to open a file node as content.
        /// </summary>
        private async Task DoOpenContentAsync()
        {
            IContentFile file = null;

            try
            {
                bool continueOpen = await UpdateChangedContentAsync(SaveReason.ContentShutdown);

                if (!continueOpen)
                {
                    return;
                }

                file = _contentFileManager.GetFile(FileExplorer.SelectedFiles[0].FullPath);

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_LOADING_CONTENT, file.Name));

                // Close the current content. It should be saved at this point.
                ResetContent();

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_OPENING, file.Name));

                // Find the associated plug in.
                if (!HostServices.ContentPlugInService.PlugIns.TryGetValue(file.Metadata.PlugInName, out ContentPlugIn plugIn))
                {
                    HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_NO_PLUGIN_FOR_CONTENT, file.Name));
                    return;
                }

                // Create a new instance of an undo service. Undo services are separate between content types, thus we need to create new instances.
                IUndoService undoService = new UndoService(HostServices.Log);

                // Create a content object.                
                IEditorContent content = await plugIn.OpenContentAsync(file, _contentFileManager, _projectData, undoService);

                if (content == null)
                {
                    return;
                }
                // Always generate a thumbnail now so we don't have to later, this also serves to refresh the thumbnail.
                RefreshFilePreview(file.Path);

                // Load the content.
                file.IsOpen = true;
                CurrentContent = content;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_OPEN_CONTENT, file?.Name));
            }
            finally
            {
                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function to determine if the project can be saved to a packed file.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the project can be saved, <b>false</b> if not.</returns>
        private bool CanSaveProjectToPackFile(CancelEventArgs args) => (_saveDialog != null) && (_saveDialog.Providers.Writers.Count > 0);

        /// <summary>
        /// Function to save the current project to a packed file.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private async Task DoSaveProjectToPackFile(CancelEventArgs args)
        {
            var cancelSource = new CancellationTokenSource();
            FileWriterPlugIn writer = null;

            try
            {
                // Function used to cancel the save operation.
                void CancelOperation() => cancelSource.Cancel();

                var lastSaveDir = new DirectoryInfo(_settings.LastOpenSavePath);

                if (!lastSaveDir.Exists)
                {
                    lastSaveDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                }

                _saveDialog.InitialDirectory = lastSaveDir;
                _saveDialog.InitialFilePath = string.Empty;

                string path = _saveDialog.GetFilename();

                if (string.IsNullOrWhiteSpace(path))
                {
                    args.Cancel = true;
                    return;
                }

                path = Path.GetFullPath(path);
                writer = _saveDialog.CurrentWriter;

                Debug.Assert(writer != null, "Must have a writer plug in.");

                HostServices.Log.Print($"File writer plug in is: {writer.Name}.", LoggingLevel.Verbose);
                HostServices.Log.Print($"Saving to '{path}'...", LoggingLevel.Simple);

                var panelUpdateArgs = new ProgressPanelUpdateArgs
                {
                    Title = Resources.GOREDIT_TEXT_PLEASE_WAIT,
                    Message = string.Format(Resources.GOREDIT_TEXT_SAVING, ProjectTitle)
                };

                UpdateProgress(panelUpdateArgs);

                // Function used to update the progress meter display.
                void SaveProgress(int currentItem, int totalItems, bool allowCancellation)
                {
                    panelUpdateArgs.CancelAction = allowCancellation ? CancelOperation : (Action)null;
                    panelUpdateArgs.PercentageComplete = (float)currentItem / totalItems;
                    UpdateProgress(panelUpdateArgs);
                }

                SaveProjectMetadata();
                HostServices.Log.Print($"Saving packed file '{path}'...", LoggingLevel.Verbose);
                await _projectManager.SavePackedFileAsync(_projectData, path, writer, SaveProgress, cancelSource.Token);

                if (cancelSource.Token.IsCancellationRequested)
                {
                    args.Cancel = true;
                    return;
                }

                // Update the current project with the updated info.               
                args.Cancel = cancelSource.Token.IsCancellationRequested;

                if (args.Cancel)
                {
                    return;
                }

                _settings.LastOpenSavePath = Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar);

                HostServices.Log.Print($"Saved project '{ProjectTitle}' to '{path}'.", LoggingLevel.Simple);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_SAVING_PROJECT);
                args.Cancel = true;
            }
            finally
            {
                HideProgress();
                cancelSource?.Dispose();
            }
        }

        /// <summary>
        /// Function to determine if content can be created or not.
        /// </summary>
        /// <param name="id">The ID for the content to create, based on the new icon ID.</param>
        /// <returns><b>true</b> if content can be created, <b>false</b> if not.</returns>
        private bool CanCreateContent(Guid id) => (_contentCreators.Count > 0) && (_contentCreators.Any(item => item.NewIconID == id));

        /// <summary>
        /// Function to create content.
        /// </summary>
        /// <param name="id">The ID for the content to create, based on the new icon ID.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoCreateContentAsync(Guid id)
        {
            IContentFile file = null;
            IDirectory directory = null;
            Stream contentStream = null;

            try
            {                
                IContentPlugInMetadata metadata = _contentCreators.FirstOrDefault(item => id == item.NewIconID);

                Debug.Assert(metadata != null, $"Could not locate the content plugin metadata for {id}.");

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_CREATING_CONTENT, metadata.ContentType));                                
                
                ContentPlugIn plugin = HostServices.ContentPlugInService.PlugIns.FirstOrDefault(item => item.Value == metadata).Value;

                Debug.Assert(plugin != null, $"Could not locate the content plug in for {id}.");
                
                directory = _fileExplorer.SelectedDirectory ?? _fileExplorer.Root;


                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_CREATING_CONTENT, metadata.ContentType));

                // Ensure we don't wipe out any changes.
                if ((CurrentContent?.SaveContentCommand != null) && (CurrentContent.ContentState != ContentState.Unmodified))
                {
                    MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_SAVE_CONTENT, CurrentContent.File.Name));

                    switch (response)
                    {
                        case MessageResponse.Cancel:
                            return;
                        case MessageResponse.Yes:
                            // Save with a content shutdown state. This will be used to handle any errors, at this call site, during save.
                            if (CurrentContent.SaveContentCommand.CanExecute(SaveReason.ContentShutdown))
                            {
                                await CurrentContent.SaveContentCommand.ExecuteAsync(SaveReason.ContentShutdown);
                            }
                            break;
                    }

                    // Shut down the current stuff.
                    ResetContent();
                }

                // Get a new name (and any default data).
                (string contentName, byte[] contentData, ProjectItemMetadata contentMetadata) = await plugin.GetDefaultContentAsync(plugin.ContentTypeID, directory.Files.Select(item => item.Name).ToHashSet(StringComparer.OrdinalIgnoreCase));

                if ((contentName == null) || (contentData == null))
                {
                    return;
                }

                // Now that we have a file, we need to populate it with default data from the content plugin.
                string path = $"{directory.FullPath}{contentName.FormatFileName()}";
                contentStream = ContentFileManager.OpenStream(path, FileMode.Create);
                contentStream.Write(contentData, 0, contentData.Length);
                contentStream.Dispose();

                file = ContentFileManager.GetFile(path);
                Debug.Assert(file != null, $"File {path} was not found!");

                // Copy the attribute and dependency metadata to the actual file object.
                file.Metadata.Attributes.Clear();
                file.Metadata.DependsOn.Clear();
                foreach (KeyValuePair<string, string> attr in contentMetadata.Attributes)
                {
                    file.Metadata.Attributes[attr.Key] = attr.Value;
                }

                foreach (KeyValuePair<string, List<string>> dependency in contentMetadata.DependsOn)
                {
                    file.Metadata.DependsOn[dependency.Key] = new List<string>(dependency.Value);
                }                
                
                // Indicate that this file is new.
                file.Metadata.Attributes[CommonEditorConstants.IsNewAttr] = bool.TrueString;
                file.RefreshMetadata();

                SaveProjectMetadata();                    
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CONTENT_CREATION);

                // If we fail, for any reason, destroy the file.
                if ((file != null) && (ContentFileManager.FileExists(file.Path)))
                {
                    ContentFileManager.DeleteFile(file.Path);
                }
                return;
            }
            finally
            {
                contentStream?.Dispose();   
                HideWaitPanel();
            }

            // From here on out, our file is ready. We only need open it. We do this outside of the try-catch-finally because the commands
            // that are executed here are already wrapped in their own exception handlers.

            // Ensure we're in the correct directory.
            if (FileExplorer.SelectedDirectory != directory)
            {
                if ((FileExplorer.SelectDirectoryCommand != null) && (FileExplorer.SelectDirectoryCommand.CanExecute(directory.FullPath)))
                {
                    FileExplorer.SelectDirectoryCommand.Execute(directory.FullPath);
                }

                // If we failed for any reason, then don't bother continuing.
                if (FileExplorer.SelectedDirectory != directory)
                {
                    return;
                }
            }

            // Get the file from the virtual file system so we can retrieve its ID.
            IFile virtualFile = directory.Files.FirstOrDefault(item => string.Equals(item.FullPath, file.Path, StringComparison.OrdinalIgnoreCase));

            // There should be no chance of this happening.
            Debug.Assert(virtualFile != null, $"File not {file.Path} found in file system.");

            IReadOnlyList<string> selectedFile = new[] { virtualFile.ID };
            if ((FileExplorer.SelectFileCommand != null) && (FileExplorer.SelectFileCommand.CanExecute(selectedFile)))
            {
                FileExplorer.SelectFileCommand.Execute(selectedFile);
            }

            if ((FileExplorer.SelectedFiles.Count == 0)
                || (FileExplorer.SelectedFiles[0] != virtualFile)
                || (FileExplorer?.OpenContentFileCommand == null)
                || (!FileExplorer.OpenContentFileCommand.CanExecute(null)))
            {
                return;
            }

            // Open the new content file.
            await FileExplorer.OpenContentFileCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// Function used to initialize the view model with dependencies.
        /// </summary>
        /// <param name="projectData">The project backing data store.</param>
        /// <param name="messageService">The message display service.</param>
        /// <param name="busyService">The busy state indicator service.</param>
        /// <exception cref="ArgumentMissingException">Thrown if any argument is <b>null</b>.</exception>
        protected override void OnInitialize(ProjectEditorParameters injectionParameters)
        {
            _settings = injectionParameters.EditorSettings;
            _viewModelFactory = injectionParameters.ViewModelFactory;
            _projectManager = injectionParameters.ProjectManager;
            _projectData = injectionParameters.Project;
            _fileExplorer = injectionParameters.FileExplorer;
            _contentFileManager = injectionParameters.ContentFileManager;
            _contentPreviewer = injectionParameters.ContentPreviewer;
            _saveDialog = injectionParameters.SaveDialog;
            _contentCreators = injectionParameters.ContentCreators;
            
            AssignEvents();

            ProjectTitle = _projectData.ProjectWorkSpace.Name;

            FileExplorer.OpenContentFileCommand = new EditorAsyncCommand<object>(DoOpenContentAsync, CanOpenContent);

            GetTools();
        }

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        public override void OnLoad()
        {
            HostServices.BusyService.SetBusy();

            try
            {
                if (FileExplorer != null)
                {
                    FileExplorer.OnLoad();

                    FileExplorer.FileSystemUpdated += FileExplorer_FileSystemUpdated;
                }
                
                if (ContentPreviewer != null)
                {
                    ContentPreviewer.OnLoad();
                    ContentPreviewer.IsEnabled = _settings.ShowContentPreview;
                }
                
                AssignEvents();                
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex);
            }
            finally
            {
                HostServices.BusyService.SetIdle();
            }
        }        

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public override void OnUnload()
        {
            if (FileExplorer != null)
            {
                FileExplorer.FileSystemUpdated -= FileExplorer_FileSystemUpdated;
            }

            ContentPreviewer?.OnUnload();
            FileExplorer?.OnUnload();

            HideWaitPanel();
            HideProgress();
            UnassignEvents();

            CurrentContent = null;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="ProjectEditor"/> class.</summary>
        public ProjectEditor() 
        {
            BeforeCloseCommand = new EditorAsyncCommand<CancelEventArgs>(DoBeforeCloseAsync);
            AfterCloseCommand = new EditorCommand<object>(DoAfterClose);
            SaveProjectToPackFileCommand = new EditorAsyncCommand<CancelEventArgs>(DoSaveProjectToPackFile, CanSaveProjectToPackFile);
            SaveProjectMetadataCommand = new EditorCommand<object>(DoSaveProjectMetadata);
            CreateContentCommand = new EditorAsyncCommand<Guid>(DoCreateContentAsync, CanCreateContent);
            ContentClosedCommand = new EditorCommand<object>(DoContentClosed);
        }
        #endregion
    }
}
