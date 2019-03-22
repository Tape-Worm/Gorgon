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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for the project editor interface.
    /// </summary>
    internal class ProjectVm
        : ViewModelBase<ProjectVmParameters>, IProjectVm, IDragDropHandler<IContentFile>
    {
        #region Constants.
        // The version of the window layout XML.
        private const string WindowLayoutVersion = "1.0-alpha";
        #endregion

        #region Variables.
        // The factory used to create view models.
        private ViewModelFactory _viewModelFactory;
        // The project data for the view model.
        private IProject _projectData;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // The current project state.
        private ProjectState _state = ProjectState.Unmodified;
        // The file explorer view model.
        private IFileExplorerVm _fileExplorer;
        // The current clipboard handler context.
        private IClipboardHandler _clipboardContext;
        // The current undo handler context.
        private IUndoHandler _undoContext;
        // The application project manager.
        private IProjectManager _projectManager;
        // The content plugin service.
        private IContentPluginManagerService _contentPlugins;        
        // The currently active content.
        private IEditorContent _currentContent;
        // The window layout XML.
        private byte[] _layout;
        // The title for the project.
        private string _projectTitle = Resources.GOREDIT_NEW_PROJECT;
        // The content previewer view model.
        private IContentPreviewVm _contentPreviewer;
        // The file manager used to manage content through content plug ins.
        private IContentFileManager _contentFileManager;
        // The event triggered when the project metadata is being saved.
        private Task _saveEvent;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the layout for the window.
        /// </summary>
        public byte[] Layout
        {
            get => _layout;
            set
            {
                OnPropertyChanging();
                _layout = value;
                if ((value == null) || (value.Length == 0))
                {
                    _viewModelFactory.Settings.WindowLayout = null;
                }
                else
                {
                    _viewModelFactory.Settings.WindowLayout = Encoding.UTF8.GetString(value);
                }
                _viewModelFactory.Settings.WindowLayoutVersion = WindowLayoutVersion;
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
                    CurrentContent.File.DependenciesUpdated -= ContentFile_DependenciesUpdated;
                    CurrentContent.PropertyChanged -= CurrentContent_PropertyChanged;
                    CurrentContent.CloseContent -= CurrentContent_CloseContent;
                    CurrentContent.WaitPanelActivated -= FileExplorer_WaitPanelActivated;
                    CurrentContent.WaitPanelDeactivated -= FileExplorer_WaitPanelDeactivated;
                    CurrentContent.ProgressUpdated -= FileExplorer_ProgressUpdated;
                    CurrentContent.ProgressDeactivated -= FileExplorer_ProgressDeactivated;                    
                }

                OnPropertyChanging();
                _currentContent = value;
                OnPropertyChanged();

                if (_currentContent != null)
                {
                    CurrentContent.PropertyChanged += CurrentContent_PropertyChanged;
                    CurrentContent.WaitPanelActivated += FileExplorer_WaitPanelActivated;
                    CurrentContent.WaitPanelDeactivated += FileExplorer_WaitPanelDeactivated;
                    CurrentContent.ProgressUpdated += FileExplorer_ProgressUpdated;
                    CurrentContent.ProgressDeactivated += FileExplorer_ProgressDeactivated;
                    CurrentContent.CloseContent += CurrentContent_CloseContent;
                    CurrentContent.File.DependenciesUpdated += ContentFile_DependenciesUpdated;
                }
            }
        }

        /// <summary>Property to set or return the active undo handler context.</summary>
        public IUndoHandler UndoContext
        {
            get => _undoContext;
            set
            {
                if (_undoContext == value)
                {
                    return;
                }

                OnPropertyChanging();
                _undoContext = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the active clipboard handler context.
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

        /// <summary>
        /// Property to set or return the content previewer.
        /// </summary>
        public IContentPreviewVm ContentPreviewer
        {
            get => _contentPreviewer;
            set
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
            set
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
        public IFileExplorerVm FileExplorer
        {
            get => _fileExplorer;
            set
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
        /// Property to set or return the current state of the project.
        /// </summary>
        public ProjectState ProjectState
        {
            get => _state;
            set
            {
                if ((_state == value) || ((_state == ProjectState.New) && (value != ProjectState.Unmodified)))
                {
                    return;
                }

                OnPropertyChanging();
                _state = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the title for the project.
        /// </summary>
        public string ProjectTitle
        {
            get => _projectTitle;
            private set
            {
                if (string.Equals(_projectTitle, value, StringComparison.CurrentCulture))
                {
                    return;
                }

                OnPropertyChanging();
                _projectTitle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command to execute when the application is closing.
        /// </summary>
        public IEditorAsyncCommand<CancelEventArgs> BeforeCloseCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the CloseContent event of the CurrentContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void CurrentContent_CloseContent(object sender, EventArgs e)
        {
            try
            {
                CurrentContent = null;
                ProjectState = ProjectState.Unmodified;
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_CLOSING_CONTENT);
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

        /// <summary>
        /// Handles the FileSystemChanged event of the FileExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void FileExplorer_FileSystemChanged(object sender, EventArgs e)
        {
            try
            {
                await SaveProjectMetadataAsync();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_SAVING_PROJECT));
            }
        }


        /// <summary>Handles the PropertyChanged event of the CurrentContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private async void CurrentContent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IEditorContent.ContentState):
                    switch (CurrentContent.ContentState)
                    {
                        case ContentState.Modified:
                        case ContentState.New:
                            ProjectState = ProjectState.Modified;
                            break;
                        case ContentState.Unmodified:
                            // If the state turns to unmodified, then refresh the thumbnail.
                            if ((ContentPreviewer?.RefreshPreviewCommand != null) && (ContentPreviewer.RefreshPreviewCommand.CanExecute(CurrentContent.File)))
                            {
                                await ContentPreviewer.RefreshPreviewCommand.ExecuteAsync(CurrentContent.File);
                            }
                            break;
                    }
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
            FileExplorer.FileSystemChanged += FileExplorer_FileSystemChanged;
        }

        /// <summary>
        /// Function to unassign events from the child view models.
        /// </summary>
        private void UnassignEvents()
        {
            if (_currentContent != null)
            {
                CurrentContent.CloseContent -= CurrentContent_CloseContent;
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
            FileExplorer.FileSystemChanged -= FileExplorer_FileSystemChanged;
        }

        /// <summary>
        /// Function to determine whether the content can be opened or not.
        /// </summary>
        /// <param name="file">The node being opened.</param>
        /// <returns><b>true</b> if the node can be opened, <b>false</b> if not.</returns>
        private bool CanOpenContent(IContentFile file) => file?.Metadata?.ContentMetadata != null;

        /// <summary>
        /// Function to persist the changed content (if any).
        /// </summary>
        /// <returns><b>true</b> if saved or skipped, <b>false</b> if cancelled.</returns>
        private async Task<bool> UpdateChangedContentAsync()
        {
            if ((CurrentContent == null)
                || (CurrentContent.ContentState == ContentState.Unmodified)
                || (CurrentContent.SaveContentCommand == null)
                || (!CurrentContent.SaveContentCommand.CanExecute(null)))
            {
                return true;
            }

            MessageResponse response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_SAVE_CONTENT, CurrentContent.File.Name), allowCancel: true);

            switch (response)
            {
                case MessageResponse.Yes:
                    await CurrentContent.SaveContentCommand.ExecuteAsync(null);
                    break;
                case MessageResponse.Cancel:
                    return false;
            }

            return true;
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
                bool result = await UpdateChangedContentAsync();
                args.Cancel = !result;
            }
            catch (Exception ex)
            {
                Log.Print("Error closing the application.", LoggingLevel.Simple);
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to open a file node as content.
        /// </summary>
        /// <param name="file">The file to open.</param>
        private async void DoOpenContent(IContentFile file)
        {
            try
            {
                bool continueOpen = await UpdateChangedContentAsync();

                if (!continueOpen)
                {
                    return;
                }

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_LOADING_CONTENT, file.Name));

                if (file.ContentPlugin == null)
                {
                    // Reset back to unassigned.                    
                    file.Metadata.PluginName = null;

                    // If we don't have a content plug in, then try to find one now.
                    // If that fails (i.e. the assignment won't change), then tell the user we can't open.
                    _contentPlugins.AssignContentPlugin(file, ContentFileManager, false);

                    if (file.ContentPlugin == null)
                    {
                        _messageService.ShowError(string.Format(Resources.GOREDIT_ERR_NO_PLUGIN_FOR_CONTENT, file.Path));
                        return;
                    }
                    else
                    {
                        // If we updated the content plug in for a content file, then the project is now in a modified state.
                        ProjectState = ProjectState.Modified;
                    }
                }

                // Close the current content. It should be saved at this point.
                if (CurrentContent != null)
                {                    
                    CurrentContent.OnUnload();
                    CurrentContent = null;                    
                }

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_OPENING, file.Name));

                // Create a content object.                
                IEditorContent content = await file.ContentPlugin.OpenContentAsync(file, _contentFileManager, _viewModelFactory, _projectData, new UndoService(Log)); 

                if (content == null)
                {
                    return;
                }

                // Always generate a thumbnail now so we don't have to later, this also serves to refresh the thumbnail.
                if ((ContentPreviewer?.RefreshPreviewCommand != null) && (ContentPreviewer.RefreshPreviewCommand.CanExecute(file)))
                {
                    await ContentPreviewer.RefreshPreviewCommand.ExecuteAsync(file);
                }

                // Load the content.
                file.IsOpen = true;                
                CurrentContent = content;
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_OPEN_CONTENT, file.Path));
            }
            finally
            {
                HideWaitPanel();
            }
        }

        /// <summary>Handles the DependenciesUpdated event of the ContentFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ContentFile_DependenciesUpdated(object sender, EventArgs e)
        {
            ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_SAVING, ProjectTitle));

            try
            {
                // When we update the dependencies on content, we need to persist those changes to the file system as soon as possible.
                await SaveProjectMetadataAsync();                
            }
            catch (Exception ex)
            {
                // If this happens, we have problems.
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_SAVING_METADATA, CurrentContent?.File?.Name ?? string.Empty));
            }
            finally
            {
                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function used to initialize the view model with dependencies.
        /// </summary>
        /// <param name="projectData">The project backing data store.</param>
        /// <param name="messageService">The message display service.</param>
        /// <param name="busyService">The busy state indicator service.</param>
        /// <exception cref="ArgumentMissingException">Thrown if any argument is <b>null</b>.</exception>
        protected override void OnInitialize(ProjectVmParameters injectionParameters)
        {
            _viewModelFactory = injectionParameters.ViewModelFactory;
            _projectManager = injectionParameters.ProjectManager ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.ProjectManager), nameof(injectionParameters));
            _projectData = injectionParameters.Project ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.Project), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.BusyService), nameof(injectionParameters));            
            _contentPlugins = injectionParameters.ContentPlugins ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.ContentPlugins), nameof(injectionParameters));

            if (_projectData.ProjectWorkSpace == null)
            {
                _projectTitle = Resources.GOREDIT_NEW_PROJECT;
            }
            else
            {                
                _projectTitle = _projectData.ProjectWorkSpace.Name;
            }

            FileExplorer.OpenContentFileCommand = new EditorCommand<IContentFile>(DoOpenContent, CanOpenContent);

            if (string.IsNullOrWhiteSpace(_viewModelFactory.Settings.WindowLayout))
            {
                return;
            }

            // If we have a mismatch for the window layout, then reset to default since the krypton docking manager causes major issues when the layout schema changes.
            // (It's really a piece of crap and is very poorly written, but we're stuck with it for now).
            if (!string.Equals(WindowLayoutVersion, _viewModelFactory.Settings.WindowLayoutVersion, StringComparison.OrdinalIgnoreCase))
            {
                _layout = null;
                return;
            }

            _layout = Encoding.UTF8.GetBytes(_viewModelFactory.Settings.WindowLayout);
        }

        /// <summary>
        /// Function to create a new content item.
        /// </summary>
        /// <param name="metadata">The metadata for the plug in associated with the content.</param>
        /// <param name="plugin">The plug in used to create the content.</param>
        /// <returns>A new content file containing the content data, or <b>null</b> if the content creation was cancelled.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/>, or the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public async Task<IContentFile> CreateNewContentItemAsync(IContentPluginMetadata metadata, ContentPlugin plugin)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            // TODO: Add prompt if we have opened, unsaved changes.
            var args = new CreateContentFileArgs
            {
                Name = metadata.ContentType
            };

            if ((_fileExplorer?.CreateContentFileCommand == null) || (!_fileExplorer.CreateContentFileCommand.CanExecute(args)))
            {
                return null;
            }
            
            _fileExplorer.CreateContentFileCommand.Execute(args);

            if (args.Cancel)
            {
                return null;
            }

            Stream contentStream = null;

            try
            {
                // Now that we have a file, we need to populate it with default data from the content plugin.
                contentStream = args.ContentFile.OpenWrite();

                byte[] contentData = await plugin.GetDefaultContentAsync(args.Name);

                contentStream.Write(contentData, 0, contentData.Length);
                contentStream.Dispose();

                args.Node.Refresh();

                // Since we already know our plug in, we can assign it here.
                _viewModelFactory.ContentPlugins.AssignContentPlugin(args.ContentFile, _contentFileManager, metadata);

                // Mark this item as new.
                args.ContentFile.Metadata.Attributes[CommonEditorConstants.IsNewAttr] = "true";

                await SaveProjectMetadataAsync();

                // Always generate a thumbnail now so we don't have to later, this also serves to refresh the thumbnail.
                if ((ContentPreviewer?.RefreshPreviewCommand != null) && (ContentPreviewer.RefreshPreviewCommand.CanExecute(args.ContentFile)))
                {                    
                    await ContentPreviewer.RefreshPreviewCommand.ExecuteAsync(args.ContentFile);
                }
            }
            catch (Exception)
            {
                // If we fail, for any reason, destroy the node.
                if (args.Node != null)
                {
                    args.Node.Parent.Children.Remove(args.Node);
                    File.Delete(args.Node.PhysicalPath);
                }

                throw;
            }
            finally
            {
                contentStream?.Dispose();
            }

            return args.ContentFile;
        }

        /// <summary>
        /// Function to persist the project metadata to the disk.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        public async Task SaveProjectMetadataAsync()
        {
            if (_saveEvent != null) 
            {
                // Wait for our previous task to finish, or wait for 30 seconds for the previous save operation to complete (if it takes this long, we're in big trouble).
                Task finishedTask = await Task.WhenAny(_saveEvent, Task.Delay(30000));

                // We timed out, try again later.                
                if (finishedTask != _saveEvent)
                {
                    Log.Print("[WARNING] A previous project metadata task was scheduled and has not yet completed after 30 seconds.  The current save project metadata task will be abandoned.", LoggingLevel.Intermediate);
                    return;
                }                
            }

            _saveEvent = Task.Run(() =>
                                {
                                    // Rebuild the project item metadata list.
                                    _projectData.ProjectItems.Clear();
                                    foreach (IFileExplorerNodeVm node in _fileExplorer.RootNode.Children.Traverse(n => n.Children).Where(n => n.Metadata != null))
                                    {
                                        node.Metadata.Dependencies.Clear();
                                    }

                                    foreach (IFileExplorerNodeVm node in _fileExplorer.RootNode.Children.Traverse(n => n.Children).Where(n => n.Metadata != null))
                                    {
                                        foreach (IFileExplorerNodeVm depNode in node.Dependencies)
                                        {
                                            if (!string.IsNullOrWhiteSpace(node.Metadata.ContentMetadata?.ContentTypeID))
                                            {
                                                depNode.Metadata.Dependencies[node.Metadata.ContentMetadata.ContentTypeID] = node.FullPath;
                                            }                                            
                                        }

                                        _projectData.ProjectItems[node.FullPath] = node.Metadata;
                                    }

                                    _projectManager.PersistMetadata(_projectData);
                                });

            await _saveEvent;
        }

        /// <summary>
        /// Function to persist the project data to a file.
        /// </summary>
        /// <param name="path">A path to the file that will hold the project data.</param>
        /// <param name="writer">The plug in used to write the project data.</param>
        /// <param name="progressCallback">The callback method that reports the saving progress to the UI.</param>
        /// <param name="cancelToken">The token used for cancellation of the operation.</param>        
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/>, or the <paramref name="writer"/> parameter is <b>null</b>.</exception>
        public Task SaveToPackFileAsync(FileInfo path, FileWriterPlugin writer, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            Program.Log.Print($"Saving packed file '{path.FullName}'...", LoggingLevel.Verbose);
            return _projectManager.SavePackedFileAsync(_projectData, path, writer, progressCallback, cancelToken);
        }

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        public override void OnLoad()
        {
            _busyService.SetBusy();

            try
            {
                // Ensure the project is locked from outside interference (Gorgon editor instances only).
                _projectManager.LockProject(_projectData);

                AssignEvents();                
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public override void OnUnload()
        {
            // Wait for 30 seconds if we've not finished our save operation.
            if (_saveEvent != null)
            {
                _saveEvent.Wait(30000);
            }

            // TODO: This should probably be placed in a command.
            if (_projectData != null)
            {
                _projectManager.CloseProject(_projectData);
            }

            HideWaitPanel();
            HideProgress();
            UnassignEvents();

            CurrentContent = null;
        }

        /// <summary>Function to determine if an object can be dropped.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <returns>
        ///   <c>true</c> if this instance can drop the specified drag data; otherwise, <c>false</c>.</returns>
        bool IDragDropHandler<IContentFile>.CanDrop(IContentFile dragData) => (dragData != null) && (!dragData.IsOpen) && (CanOpenContent(dragData));

        /// <summary>Function to drop the payload for a drag drop operation.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <param name="afterDrop">[Optional] The method to execute after the drop operation is completed.</param>
        void IDragDropHandler<IContentFile>.Drop(IContentFile dragData, Action afterDrop) => DoOpenContent(dragData);
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ViewModels.ProjectVm"/> class.</summary>
        public ProjectVm() => BeforeCloseCommand = new EditorAsyncCommand<CancelEventArgs>(DoBeforeCloseAsync);
        #endregion
    }
}
