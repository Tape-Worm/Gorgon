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
using System.IO;
using System.Linq;
using System.Text;
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
        : ViewModelBase<ProjectEditorParameters>, IProjectEditor
    {
        #region Constants.        
        /// <summary>
        /// Metadata naming for the project item type attribute.
        /// </summary>
        public const string ProjectItemTypeAttrName = "ProjectItemType";
        #endregion

        #region Variables.
        // The list of missing metadata links.
        private readonly List<string> _missingMetadataLinks = new List<string>();
        // The factory used to create view models.
        private ViewModelFactory _viewModelFactory;
        // The project data for the view model.
        private IProject _projectData;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // The file explorer view model.
        private IFileExplorer _fileExplorer;
        // The current clipboard handler context.
        private IClipboardHandler _clipboardContext;
        // The application project manager.
        private ProjectManager _projectManager;
        // The content plugin service.
        private IContentPlugInService _contentPlugIns;
        // The currently active content.
        private IEditorContent _currentContent;
        // The title for the project.
        private string _projectTitle = Resources.GOREDIT_NEW_PROJECT;
        // The content previewer view model.
        private IContentPreviewVm _contentPreviewer;
        // The file manager used to manage content through content plug ins.
        private IContentFileManager _contentFileManager;
        // The list of tools available to the application.
        private IToolPlugInService _toolPlugIns;
        // The list of tool buttons.
        private Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> _toolButtons = new Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>>(StringComparer.CurrentCultureIgnoreCase);
        // The settings for the application.
        private EditorSettings _settings;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the available tool plug in button definitions for the application.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> ToolButtons => _toolButtons;

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
                    _currentContent.File.DependenciesUpdated -= ContentFile_DependenciesUpdated;
                    _currentContent.PropertyChanging -= CurrentContent_PropertyChanging;
                    _currentContent.PropertyChanged -= CurrentContent_PropertyChanged;
                    _currentContent.CloseContent -= CurrentContent_CloseContent;
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
                    _currentContent.CloseContent += CurrentContent_CloseContent;
                    _currentContent.File.DependenciesUpdated += ContentFile_DependenciesUpdated;
                }

                NotifyPropertyChanged(nameof(CommandContext));
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
        public string ProjectTitle => _projectTitle;

        /// <summary>Property to return the current command context.</summary>
        public string CommandContext => CurrentContent?.CommandContext ?? string.Empty;

        /// <summary>
        /// Property to return the command to execute when the application is closing.
        /// </summary>
        public IEditorAsyncCommand<CancelEventArgs> BeforeCloseCommand
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
                    // TODO: Evaluate dependency linkages.
                    metadata = new ProjectItemMetadata(file.Metadata);
                    metadata.Attributes[ProjectItemTypeAttrName] = ProjectItemType.File.ToString();
                    _projectData.ProjectItems[file.FullPath] = metadata;
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
        /// Function to rebuild the list of sorted ribbon buttons.
        /// </summary>
        private void GetTools()
        {
            NotifyPropertyChanging(nameof(ToolButtons));

            var result = new Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>>(StringComparer.CurrentCultureIgnoreCase);

            foreach (KeyValuePair<string, ToolPlugIn> plugin in _toolPlugIns.PlugIns)
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

        /// <summary>Handles the CloseContent event of the CurrentContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void CurrentContent_CloseContent(object sender, EventArgs e)
        {
            try
            {
                if (CurrentContent?.File != null)
                {
                    RefreshFilePreview(CurrentContent.File);
                }
                CurrentContent = null;
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

        /// <summary>Handles the PropertyChanging event of the CurrentContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void CurrentContent_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IEditorContent.File):
                    if (CurrentContent.File != null)
                    {
                        CurrentContent.File.DependenciesUpdated -= ContentFile_DependenciesUpdated;
                    }
                    break;
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
                        RefreshFilePreview(CurrentContent.File);                            
                    }
                    break;
                case nameof(IEditorContent.File):
                    if (CurrentContent.File != null)
                    {
                        CurrentContent.File.DependenciesUpdated += ContentFile_DependenciesUpdated;
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
#warning FIX THIS - Convert to a command that gets called when the file system is updated.
            //FileExplorer.FileSystemChanged += FileExplorer_FileSystemChanged;
        }

        /// <summary>
        /// Function to unassign events from the child view models.
        /// </summary>
        private void UnassignEvents()
        {
            if (_currentContent != null)
            {
                if (CurrentContent.File != null)
                {
                    CurrentContent.File.DependenciesUpdated -= ContentFile_DependenciesUpdated;
                }
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
#warning FIX THIS - Convert to a command that gets called when the file system is updated.
            //FileExplorer.FileSystemChanged -= FileExplorer_FileSystemChanged;
        }

        /// <summary>
        /// Function to force a refresh of the specified file preview.
        /// </summary>
        /// <param name="file">The file to refresh.</param>
        private void RefreshFilePreview(IContentFile file)
        {
            if ((ContentPreviewer.RefreshPreviewCommand != null) && (ContentPreviewer.RefreshPreviewCommand.CanExecute(file)))
            {
                ContentPreviewer.RefreshPreviewCommand.Execute(file);
            }
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

            MessageResponse response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_SAVE_CONTENT, CurrentContent.File.Name), allowCancel: true);

            switch (response)
            {
                case MessageResponse.Yes:
                    if ((CurrentContent.SaveContentCommand != null) && (CurrentContent.SaveContentCommand.CanExecute(saveReason)))
                    {
                        await CurrentContent.SaveContentCommand.ExecuteAsync(saveReason);

                        // Refresh the preview after we've saved.
                        RefreshFilePreview(CurrentContent.File);
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
            _busyService.SetBusy();

            try
            {
                SaveProjectMetadata();
            }
            catch (Exception ex)
            {
                Log.Print("[ERROR] Could not save the project metadata due to an exception!!", LoggingLevel.Simple);
                Log.LogException(ex);
            }
            finally
            {
                _busyService.SetIdle();
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
#warning FIX THIS
                /*bool continueOpen = await UpdateChangedContentAsync(SaveReason.ContentShutdown);

                if (!continueOpen)
                {
                    return;
                }

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_LOADING_CONTENT, file.Name));

                // Locate the node for the content file.
                IFileExplorerNodeVm dirNode = _fileExplorer.FindNode(Path.GetDirectoryName(file.Path));

                if (dirNode == null)
                {
                    Log.Print($"[ERROR] Content file '{file.Path}' directory has no node.", LoggingLevel.Verbose);
                    throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, file.Path));
                }

                IFileExplorerNodeVm fileNode = dirNode.Children.FirstOrDefault(item => string.Equals(item.Name, file.Name, StringComparison.OrdinalIgnoreCase));

                if (fileNode == null)
                {
                    Log.Print($"[ERROR] Content file '{file.Path}' has no associated file node.", LoggingLevel.Verbose);
                    throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, file.Path));
                }

                if (fileNode != file)
                {
                    file = (IContentFile)fileNode;
                }

                // If we're on a dependency node, then go to the actual node that we're working on.
                if (_fileExplorer.SelectedNode != fileNode)
                {
                    dirNode.IsExpanded = true;
                    _fileExplorer.SelectNodeCommand?.Execute(fileNode);
                }

                if (file.ContentPlugIn == null)
                {
                    // Reset back to unassigned.                    
                    file.Metadata.PlugInName = null;

                    // If we don't have a content plug in, then try to find one now.
                    // If that fails (i.e. the assignment won't change), then tell the user we can't open.
                    _contentPlugIns.AssignContentPlugIn(file, ContentFileManager, false);

                    if (file.ContentPlugIn == null)
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
                IEditorContent content = await file.ContentPlugIn.OpenContentAsync(file, _contentFileManager, _projectData, new UndoService(Log));

                if (content == null)
                {
                    return;
                }

                // Always generate a thumbnail now so we don't have to later, this also serves to refresh the thumbnail.
                RefreshFilePreview(file);

                // Load the content.
                file.IsOpen = true;
                CurrentContent = content;*/
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
        private void ContentFile_DependenciesUpdated(object sender, EventArgs e)
        {
            ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_SAVING, ProjectTitle));

            try
            {
                // When we update the dependencies on content, we need to persist those changes to the file system as soon as possible.
                SaveProjectMetadata();
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
        protected override void OnInitialize(ProjectEditorParameters injectionParameters)
        {
            _settings = injectionParameters.EditorSettings;
            _viewModelFactory = injectionParameters.ViewModelFactory;
            _projectManager = injectionParameters.ProjectManager;
            _projectData = injectionParameters.Project;
            _messageService = injectionParameters.MessageDisplay;
            _busyService = injectionParameters.BusyService;
            _contentPlugIns = injectionParameters.ContentPlugIns;
            _fileExplorer = injectionParameters.FileExplorer;
            _contentFileManager = injectionParameters.ContentManager;
            _contentPreviewer = injectionParameters.ContentPreviewer;

            AssignEvents();

            // Get the tool plug ins.
            _toolPlugIns = injectionParameters.ToolPlugIns;

            _projectTitle = _projectData.ProjectWorkSpace.Name;

            FileExplorer.OpenContentFileCommand = new EditorCommand<IContentFile>(DoOpenContent, CanOpenContent);

            GetTools();
        }

        /// <summary>
        /// Function to create a new content item.
        /// </summary>
        /// <param name="metadata">The metadata for the plug in associated with the content.</param>
        /// <param name="plugin">The plug in used to create the content.</param>
        /// <returns>A new content file containing the content data, or <b>null</b> if the content creation was cancelled.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/>, or the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        public async Task<IContentFile> CreateNewContentItemAsync(IContentPlugInMetadata metadata, ContentPlugIn plugin)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }
#warning FIX THIS - This should not be public - Turn into a command.
            /*
                        IFileExplorerNodeVm parentNode = _fileExplorer.SelectedNode ?? _fileExplorer.RootNode;
                        Stream contentStream = null;
                        var args = new CreateContentFileArgs(parentNode);

                        try
                        {
                            ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_CREATING_CONTENT, metadata.ContentType));

                            // Ensure we don't wipe out any changes.
                            if ((CurrentContent?.SaveContentCommand != null) && (CurrentContent.ContentState != ContentState.Unmodified))
                            {
                                MessageResponse response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_SAVE_CONTENT, CurrentContent.File.Name));

                                switch (response)
                                {
                                    case MessageResponse.Cancel:
                                        return null;
                                    case MessageResponse.Yes:
                                        if (CurrentContent.SaveContentCommand.CanExecute(SaveReason.ContentShutdown))
                                        {
                                            await CurrentContent.SaveContentCommand.ExecuteAsync(SaveReason.ContentShutdown);
                                        }
                                        break;
                                }

                                // Shut down the current stuff.
                                CurrentContent = null;
                            }

                            // Create the actual file.
                            args.Name = plugin.ContentTypeID;

                            // Get a new name (and any default data).
                            (string contentName, byte[] contentData) = await plugin.GetDefaultContentAsync(args.Name, parentNode.Children.Select(item => item.Name).ToHashSet(StringComparer.OrdinalIgnoreCase));

                            if ((contentName == null) || (contentData == null))
                            {
                                return null;
                            }

                            args.Name = contentName;

                            if ((_fileExplorer?.CreateContentFileCommand == null) || (!_fileExplorer.CreateContentFileCommand.CanExecute(args)))
                            {
                                return null;
                            }

                            _fileExplorer.CreateContentFileCommand.Execute(args);

                            if (args.Cancel)
                            {
                                return null;
                            }

                            // Now that we have a file, we need to populate it with default data from the content plugin.
                            contentStream = args.ContentFile.OpenWrite();
                            contentStream.Write(contentData, 0, contentData.Length);
                            contentStream.Dispose();

                            args.Node.Refresh();

                            // Since we already know our plug in, we can assign it here.
                            _viewModelFactory.ContentPlugIns.AssignContentPlugIn(args.ContentFile, _contentFileManager, metadata);

                            // Mark this item as new.
                            args.ContentFile.Metadata.Attributes[CommonEditorConstants.IsNewAttr] = "true";

                            await SaveProjectMetadataAsync();

                            // Always generate a thumbnail now so we don't have to later, this also serves to refresh the thumbnail.
                            RefreshFilePreview(args.ContentFile);
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
                            HideWaitPanel();
                        }

                        return args.ContentFile;*/
            return null;
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
        public Task SaveToPackFileAsync(FileInfo path, FileWriterPlugIn writer, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
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

                if (FileExplorer != null)
                {
                    FileExplorer.OnLoad();

                    FileExplorer.FileSystemUpdated += FileExplorer_FileSystemUpdated;
                }
                
                ContentPreviewer?.OnLoad();

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
            if (FileExplorer != null)
            {
                FileExplorer.FileSystemUpdated -= FileExplorer_FileSystemUpdated;
            }

            if (_projectData != null)
            {
                _projectManager.CloseProject(_projectData);
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
            SaveProjectMetadataCommand = new EditorCommand<object>(DoSaveProjectMetadata);
        }
        #endregion
    }
}
