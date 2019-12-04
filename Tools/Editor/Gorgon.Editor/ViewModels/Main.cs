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
// Created: August 26, 2018 9:43:30 PM
// 
#endregion

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Converters;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;
using Newtonsoft.Json;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for the main window.
    /// </summary>
    internal class Main
        : ViewModelBase<MainParameters>, IMain
    {
        #region Variables.
        // The project manager used to handle project data.
        private IProjectManager _projectManager;
        // The factory used to create view models.
        private ViewModelFactory _viewModelFactory;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The currently active project.
        private IProjectVm _currentProject;
        // The project open dialog service.
        private IEditorFileOpenDialogService _openDialog;
        // The project save dialog service.
        private IEditorFileSaveAsDialogService _saveDialog;
        // The directory locator service.
        private IDirectoryLocateService _directoryLocator;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the settings for the application.
        /// </summary>
        public EditorSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a list of content plugins that can create their own content.
        /// </summary>
        public ObservableCollection<IContentPlugInMetadata> ContentCreators
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the view model for the settings view.
        /// </summary>
        public IEditorSettingsVm SettingsViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the view model for the new project child view.
        /// </summary>
        public IStageNewVm NewProject
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the recent files view model for the recent files child view.
        /// </summary>
        public IRecentVm RecentFiles
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the view model for the current project.
        /// </summary>
        public IProjectVm CurrentProject
        {
            get => _currentProject;
            private set
            {
                if (_currentProject == value)
                {
                    return;
                }

                if (_currentProject != null)
                {
                    _currentProject.WaitPanelActivated -= CurrentProject_WaitPanelActivated;
                    _currentProject.WaitPanelDeactivated -= CurrentProject_WaitPanelDeactivated;
                    _currentProject.ProgressDeactivated -= CurrentProject_ProgressDeactivated;
                    _currentProject.ProgressUpdated -= CurrentProject_ProgressUpdated;
                    _currentProject.PropertyChanged -= CurrentProject_PropertyChanged;
                }

                OnPropertyChanging();
                _currentProject = value;
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(Text));
                NotifyPropertyChanged(nameof(ClipboardContext));

                if (_currentProject == null)
                {
                    return;
                }

                _currentProject.PropertyChanged += CurrentProject_PropertyChanged;
                _currentProject.WaitPanelActivated += CurrentProject_WaitPanelActivated;
                _currentProject.WaitPanelDeactivated += CurrentProject_WaitPanelDeactivated;
                _currentProject.ProgressDeactivated += CurrentProject_ProgressDeactivated;
                _currentProject.ProgressUpdated += CurrentProject_ProgressUpdated;
            }
        }

        /// <summary>
        /// Property to return the text for the caption.
        /// </summary>
        public string Text => _currentProject == null
                    ? Resources.GOREDIT_CAPTION_NO_FILE
                    : _currentProject.ProjectState == ProjectState.Unmodified
                           ? string.Format(Resources.GOREDIT_CAPTION_FILE, _currentProject.ProjectTitle)
                           : string.Format(Resources.GOREDIT_CAPTION_FILE, _currentProject.ProjectTitle) + "*";

        /// <summary>
        /// Property to return the current clipboard context.
        /// </summary>
        public Olde_IClipboardHandler ClipboardContext => CurrentProject?.ClipboardContext;

        /// <summary>
        /// Property to return the command used to open a project.
        /// </summary>        
        public IEditorCommand<object> BrowseProjectCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to open a packed file.
        /// </summary>
        public IEditorCommand<object> OpenPackFileCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to save a project as a packed file.
        /// </summary>
        public IEditorAsyncCommand<SavePackFileArgs> SavePackFileCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to query before closing the application.
        /// </summary>
        public IEditorAsyncCommand<AppCloseArgs> AppClosingAsyncCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to create content.
        /// </summary>
        public IEditorCommand<string> CreateContentCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to persist the settings back to the file system.
        /// </summary>
        private void PersistSettings()
        {
            StreamWriter writer = null;
#if DEBUG
            var settingsFile = new FileInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, $"Gorgon.Editor.Settings.DEBUG.json"));
#else
            var settingsFile = new FileInfo(Path.Combine(Program.ApplicationUserDirectory.FullName, $"Gorgon.Editor.Settings.json"));
#endif

            try
            {
                writer = new StreamWriter(settingsFile.FullName, false, Encoding.UTF8);
                writer.Write(JsonConvert.SerializeObject(Settings, new JsonSharpDxRectConverter()));
            }
            catch (Exception ex)
            {
                Debug.Print($"Error saving settings.\n{ex.Message}");
            }
            finally
            {
                writer?.Dispose();
            }
        }

        /// <summary>
        /// Function to determine if a projects can be opened.
        /// </summary>
        /// <returns><b>true</b> if projects can be opened, <b>false</b> if not.</returns>
        private bool CanOpenProjects() => _openDialog.Providers.Readers.Count > 0;

        /// <summary>
        /// Function to determine if a project can be saved.
        /// </summary>
        /// <param name="args">The arguments for the save command.</param>
        /// <returns><b>true</b> if the project can be saved, <b>false</b> if not.</returns>
        private bool CanSaveProject(SavePackFileArgs args) => (_saveDialog.Providers.Writers.Count > 0) && (args?.CurrentProject != null);

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(MainParameters injectionParameters)
        {
            Settings = injectionParameters.Settings ?? throw new ArgumentNullException(nameof(MainParameters.Settings), nameof(injectionParameters));
            _projectManager = injectionParameters.ProjectManager ?? throw new ArgumentMissingException(nameof(MainParameters.ProjectManager), nameof(injectionParameters));
            _viewModelFactory = injectionParameters.ViewModelFactory ?? throw new ArgumentMissingException(nameof(MainParameters.ViewModelFactory), nameof(injectionParameters));
            _openDialog = injectionParameters.OpenDialog ?? throw new ArgumentMissingException(nameof(MainParameters.OpenDialog), nameof(injectionParameters));
            _saveDialog = injectionParameters.SaveDialog ?? throw new ArgumentMissingException(nameof(MainParameters.SaveDialog), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(MainParameters.MessageDisplay), nameof(injectionParameters));
            NewProject = injectionParameters.NewProject ?? throw new ArgumentMissingException(nameof(MainParameters.NewProject), nameof(injectionParameters));
            RecentFiles = injectionParameters.RecentFiles ?? throw new ArgumentMissingException(nameof(MainParameters.RecentFiles), nameof(injectionParameters));
            SettingsViewModel = injectionParameters.SettingsVm ?? throw new ArgumentMissingException(nameof(MainParameters.SettingsVm), nameof(injectionParameters));

            _directoryLocator = injectionParameters.ViewModelFactory.DirectoryLocator;

            ContentCreators = new ObservableCollection<IContentPlugInMetadata>(injectionParameters.ContentCreators ?? throw new ArgumentMissingException(nameof(MainParameters.ContentCreators), nameof(injectionParameters)));
            RecentFiles.OpenProjectCommand = new EditorCommand<RecentItem>(DoOpenRecentAsync, CanOpenRecent);
            NewProject.CreateProjectCommand = new EditorCommand<object>(DoCreateProjectAsync, CanCreateProject);
        }

        /// <summary>
        /// Handles the PropertyChanged event of the CurrentProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentProject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IProjectVm.ClipboardContext):
                    NotifyPropertyChanged(nameof(ClipboardContext));
                    break;
                case nameof(IProjectVm.ProjectTitle):
                case nameof(IProjectVm.ProjectState):
                    NotifyPropertyChanged(nameof(Text));
                    break;
            }
        }

        /// <summary>
        /// Function called when the progress panel is shown or updated.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void CurrentProject_ProgressUpdated(object sender, ProgressPanelUpdateArgs e) => UpdateProgress(e);

        /// <summary>
        /// Handles the ProgressDeactivated event of the CurrentProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CurrentProject_ProgressDeactivated(object sender, EventArgs e) => HideProgress();

        /// <summary>
        /// Handles the WaitPanelDeactivated event of the CurrentProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CurrentProject_WaitPanelDeactivated(object sender, EventArgs e) => HideWaitPanel();

        /// <summary>
        /// Function called when the wait panel is requested to activate.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void CurrentProject_WaitPanelActivated(object sender, WaitPanelActivateArgs e) => ShowWaitPanel(e.Message, e.Title);

        /// <summary>
        /// Handles the WaitPanelDeactivated event of the NewProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NewProject_WaitPanelDeactivated(object sender, EventArgs e) => HideWaitPanel();

        /// <summary>
        /// Function called when the new project view model requests that a wait panel be shown.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewProject_WaitPanelActivated(object sender, WaitPanelActivateArgs e) => ShowWaitPanel(e.Message, e.Title);

        /// <summary>
        /// Function to open a project.
        /// </summary>
        /// <param name="path">The path to the project.</param>
        /// <param name="forceImport"><b>true</b> force the import of the files in the project directory, <b>false</b> to load as-is.</param>
        private async Task OpenProjectAsync(DirectoryInfo path, bool forceImport)
        {
            IProject project;

            // Check for content/project changes.
            var args = new CancelEventArgs();
            if ((CurrentProject?.BeforeCloseCommand != null) && (CurrentProject.BeforeCloseCommand.CanExecute(args)))
            {
                await CurrentProject.BeforeCloseCommand.ExecuteAsync(args);

                if (args.Cancel)
                {
                    return;
                }
            }

            // Check for the existence of the directory.
            if (!path.Exists)
            {
                if (_messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_NO_PROJECT_DIR, path.FullName)) == MessageResponse.Yes)
                {
                    HideWaitPanel();
                    await CreateProjectAsync(path);
                    return;
                }
            }
            else if (!_projectManager.IsGorgonProject(path))
            {
                if (_messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_NOT_GOREDIT_PROJ, path.FullName)) == MessageResponse.Yes)
                {
                    HideWaitPanel();
                    await CreateProjectAsync(path);
                    return;
                }
            }

            // Ensure that our metadata is up to date in the current project.
            if (CurrentProject != null)
            {
                await CurrentProject.SaveProjectMetadataAsync();
            }

            project = await Task.Run(() => _projectManager.OpenProject(path));

            if (project == null)
            {
                Program.Log.Print("ERROR: No project was returned from the project manager.", LoggingLevel.Simple);
                return;
            }

            IProjectVm projectVm = await _viewModelFactory.CreateProjectViewModelAsync(project);

            // The project should not be in a modified state.
            projectVm.ProjectState = ProjectState.Unmodified;

            // Close the current project.
            CurrentProject = null;

            // Begin file scanning.
            HideWaitPanel();

            UpdateProgress(new ProgressPanelUpdateArgs
            {
                CancelAction = null,
                IsMarquee = false,
                Title = Resources.GOREDIT_TEXT_SCANNING
            });

            // Re-import any files needed.
            if (forceImport)
            {
                await projectVm.FileExplorer.RunImportersAsync(CancellationToken.None);
            }

            void UpdateScanProgress(string node, int fileNumber, int totalFileCount)
            {
                float percentComplete = (float)fileNumber / totalFileCount;
                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    PercentageComplete = percentComplete,
                    CancelAction = null,
                    IsMarquee = false,
                    Title = Resources.GOREDIT_TEXT_SCANNING,
                    Message = node.Ellipses(65, true)
                });
            }

            await Task.Run(() => _viewModelFactory.FileScanService.Scan(projectVm.FileExplorer.RootNode, projectVm.ContentFileManager, UpdateScanProgress, true, true));

            // Update the metadata for the most up-to-date info.
            await projectVm.SaveProjectMetadataAsync();
            CurrentProject = projectVm;

            Settings.LastProjectWorkingDirectory = project.ProjectWorkSpace.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            RecentItem dupe = RecentFiles.Files.FirstOrDefault(item => string.Equals(Settings.LastProjectWorkingDirectory,
                item.FilePath.FormatDirectory(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase));

            if (dupe != null)
            {
                RecentFiles.Files.Remove(dupe);
            }

            RecentFiles.Files.Add(new RecentItem
            {
                FilePath = path.FullName,
                LastUsedDate = DateTime.Now
            });

            Program.Log.Print($"Project '{projectVm.ProjectTitle}' was loaded from '{path}'.", LoggingLevel.Simple);
        }

        /// <summary>
        /// Function to determine if a recent project can be opened or not.
        /// </summary>
        /// <param name="recentItem">The recent item to open.</param>
        /// <returns><b>true</b> if a recent project can be opened, or <b>false</b> if not.</returns>
        private bool CanOpenRecent(RecentItem recentItem) => (recentItem != null) && (!string.IsNullOrWhiteSpace(recentItem.FilePath));

        /// <summary>
        /// Function to open a recent project item.
        /// </summary>
        /// <param name="recentItem">The recent item to open.</param>
        private async void DoOpenRecentAsync(RecentItem recentItem)
        {
            try
            {
                // Always save the metadata.
                if (CurrentProject != null)
                {
                    await CurrentProject.SaveProjectMetadataAsync();
                }

                var projectDir = new DirectoryInfo(recentItem.FilePath);
                Program.Log.Print($"Opening '{projectDir.FullName}'...", LoggingLevel.Simple);

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, projectDir.FullName.Ellipses(65, true)), null));

                await OpenProjectAsync(projectDir, false);
            }
            catch (DirectoryNotFoundException dex)
            {
                _messageService.ShowError(dex);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_OPENING_PROJECT);
            }
            finally
            {
                PersistSettings();
                HideWaitPanel();
                HideProgress();
            }
        }

        /// <summary>
        /// Function to open a pack file.
        /// </summary>
        private async void DoOpenPackFileAsync()
        {
            try
            {
                string path = _openDialog.GetFilename();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                DirectoryInfo target = _directoryLocator.GetDirectory(GetInitialProjectDirectory(), Resources.GOREDIT_CAPTION_SELECT_PROJECT_DIR);

                if (target == null)
                {
                    return;
                }

                target = new DirectoryInfo(Path.Combine(target.FullName, Path.GetFileName(path)));

                // If the target directory already exists, and it has items in it, then prompt the user.
                if ((target.Exists) && (target.GetFileSystemInfos().Length > 0))
                {
                    if (_messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_WORKSPACE_PATH_EXISTS, target.FullName)) == MessageResponse.No)
                    {
                        return;
                    }
                }

                // Check for content/project changes.
                var args = new CancelEventArgs();
                if ((CurrentProject?.BeforeCloseCommand != null) && (CurrentProject.BeforeCloseCommand.CanExecute(args)))
                {
                    await CurrentProject.BeforeCloseCommand.ExecuteAsync(args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, path.Ellipses(65, true)), null));

                Program.Log.Print($"Opening '{path}'...", LoggingLevel.Simple);

                // Ensure that our metadata is up to date in the current project.
                if (CurrentProject != null)
                {
                    await CurrentProject.SaveProjectMetadataAsync();
                }

                // Create the project by copying data into the folder structure.
                await _projectManager.OpenPackFileProjectAsync(new FileInfo(path), target);
                target.Refresh();

                if (!target.Exists)
                {
                    Program.Log.Print("ERROR: No project was returned from the project manager.", LoggingLevel.Simple);
                    return;
                }

                await OpenProjectAsync(target, true);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_OPENING_PROJECT);
            }
            finally
            {
                PersistSettings();
                HideWaitPanel();
                HideProgress();
            }
        }

        /// <summary>
        /// Function to retrieve the initial directory for the directory locator service.
        /// </summary>
        /// <returns>The initial directory.</returns>
        private DirectoryInfo GetInitialProjectDirectory()
        {
            var dir = new DirectoryInfo(string.IsNullOrEmpty(Settings.LastProjectWorkingDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Settings.LastProjectWorkingDirectory);
            dir.Refresh();

            if (!dir.Exists)
            {
                dir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }

            return dir;
        }

        /// <summary>
        /// Function called to open a project.
        /// </summary>
        private async void DoOpenProjectAsync()
        {
            try
            {
                DirectoryInfo path = _directoryLocator.GetDirectory(GetInitialProjectDirectory(), Resources.GOREDIT_CAPTION_OPEN_PROJECT);

                if (path == null)
                {
                    return;
                }

                Program.Log.Print($"Opening '{path.FullName}'...", LoggingLevel.Simple);

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, path.FullName.Ellipses(65, true)), null));

                await OpenProjectAsync(path, false);
            }
            catch (DirectoryNotFoundException dex)
            {
                _messageService.ShowError(dex);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_OPENING_PROJECT);
            }
            finally
            {
                PersistSettings();
                HideWaitPanel();
                HideProgress();
            }
        }

        /// <summary>
        /// Function to save the current project to a packed file.
        /// </summary>
        /// <param name="args">The save project arguments.</param>
        /// <returns><b>true</b> if saved successfully, <b>false</b> if cancelled.</returns>
        private async Task<bool> CreateSaveProjectToPackFileTask(SavePackFileArgs args)
        {
            var cancelSource = new CancellationTokenSource();
            FileInfo projectFile = null;
            FileWriterPlugIn writer = null;

            try
            {
                // Function used to cancel the save operation.
                void CancelOperation() => cancelSource.Cancel();

                var lastSaveDir = new DirectoryInfo(Settings.LastOpenSavePath);

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
                    return false;
                }

                projectFile = new FileInfo(path);
                writer = _saveDialog.CurrentWriter;

                Debug.Assert(writer != null, "Must have a writer plug in.");

                Program.Log.Print($"File writer plug in is: {writer.Name}.", LoggingLevel.Verbose);
                Program.Log.Print($"Saving to '{projectFile.FullName}'...", LoggingLevel.Simple);

                var panelUpdateArgs = new ProgressPanelUpdateArgs
                {
                    Title = Resources.GOREDIT_TEXT_PLEASE_WAIT,
                    Message = string.Format(Resources.GOREDIT_TEXT_SAVING, CurrentProject.ProjectTitle)
                };

                UpdateProgress(panelUpdateArgs);

                // Function used to update the progress meter display.
                void SaveProgress(int currentItem, int totalItems, bool allowCancellation)
                {
                    panelUpdateArgs.CancelAction = allowCancellation ? CancelOperation : (Action)null;
                    panelUpdateArgs.PercentageComplete = (float)currentItem / totalItems;
                    UpdateProgress(panelUpdateArgs);
                }

                await CurrentProject.SaveProjectMetadataAsync();
                await CurrentProject.SaveToPackFileAsync(projectFile, writer, SaveProgress, cancelSource.Token);

                if (cancelSource.Token.IsCancellationRequested)
                {
                    args.Cancel = true;
                    return false;
                }

                // Update the current project with the updated info.               
                args.Cancel = cancelSource.Token.IsCancellationRequested;

                if (args.Cancel)
                {
                    return false;
                }

                Settings.LastOpenSavePath = projectFile.DirectoryName.FormatDirectory(Path.DirectorySeparatorChar);

                Program.Log.Print($"Saved project '{CurrentProject.ProjectTitle}' to '{projectFile.FullName}'.", LoggingLevel.Simple);
                return true;
            }
            finally
            {
                HideProgress();
                cancelSource?.Dispose();
            }
        }

        /// <summary>
        /// Function to save a project file to a packed file.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private async Task DoSaveProjectToPackFileAsync(SavePackFileArgs args)
        {
            try
            {
                await CreateSaveProjectToPackFileTask(args);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_SAVING_PROJECT);
                args.Cancel = true;
            }
        }

        /// <summary>
        /// Function called to determine if a project can be created.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the project can be created, <b>false</b> if not.</returns>
        private bool CanCreateProject() => (string.IsNullOrWhiteSpace(NewProject.InvalidPathReason)) && (!string.IsNullOrWhiteSpace(NewProject.Title)) && (NewProject.WorkspacePath != null);

        /// <summary>
        /// Function to create a project.
        /// </summary>
        /// <param name="directory">The directory that will hold the project files.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task CreateProjectAsync(DirectoryInfo directory)
        {
            ShowWaitPanel(Resources.GOREDIT_TEXT_CREATING_PROJECT);

            try
            {
                // Ensure that our metadata is up to date in the current project.
                if (CurrentProject != null)
                {
                    await CurrentProject.SaveProjectMetadataAsync();
                }

                IProject project = await Task.Run(() => _projectManager.CreateProject(directory));

                // Unload the current project.
                CurrentProject = null;
                Settings.LastProjectWorkingDirectory = string.Empty;

                CurrentProject = await _viewModelFactory.CreateProjectViewModelAsync(project);
                Settings.LastProjectWorkingDirectory = project.ProjectWorkSpace.FullName.FormatDirectory(Path.DirectorySeparatorChar);

                RecentItem dupe = RecentFiles.Files.FirstOrDefault(item => string.Equals(item.FilePath.FormatDirectory(Path.DirectorySeparatorChar),
                    Settings.LastProjectWorkingDirectory,
                    StringComparison.OrdinalIgnoreCase));

                if (dupe != null)
                {
                    RecentFiles.Files.Remove(dupe);
                }

                RecentFiles.Files.Add(new RecentItem
                {
                    FilePath = Settings.LastProjectWorkingDirectory,
                    LastUsedDate = DateTime.Now
                });
            }
            finally
            {
                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function to create a new project.
        /// </summary>
        /// <param name="args">The arguments for creating the project.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async void DoCreateProjectAsync()
        {
            try
            {
                // Check for content/project changes.
                var args = new CancelEventArgs();
                if ((CurrentProject?.BeforeCloseCommand != null) && (CurrentProject.BeforeCloseCommand.CanExecute(args)))
                {
                    await CurrentProject.BeforeCloseCommand.ExecuteAsync(args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                await CreateProjectAsync(NewProject.WorkspacePath);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_CREATE_PROJECT);
            }
            finally
            {
                PersistSettings();
                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function to determine if content can be created.
        /// </summary>
        /// <param name="notUsed">Not used.</param>
        /// <returns><b>true</b> if the content can be created, <b>false</b> if not.</returns>
        private bool CanCreateContent(string notUsed) => CurrentProject?.FileExplorer != null && ContentCreators != null && ContentCreators.Count > 0;

        /// <summary>
        /// Function to create new content.
        /// </summary>
        /// <param name="contentID">The ID of the content type based on its new icon ID.</param>
        private async void DoCreateContentAsync(string contentID)
        {
            IContentFile contentFile = null;

            try
            {
                if (!Guid.TryParse(contentID, out Guid guid))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_INVALID_CONTENT_TYPE_ID);
                }

                IContentPlugInMetadata metaData = ContentCreators.FirstOrDefault(item => guid == item.NewIconID);

                Debug.Assert(metaData != null, $"Could not locate the content plugin metadata for {contentID}.");

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_CREATING_CONTENT, metaData.ContentType));

                ContentPlugIn plugin = _viewModelFactory.ContentPlugIns.PlugIns.FirstOrDefault(item => item.Value == metaData).Value;

                Debug.Assert(plugin != null, $"Could not locate the content plug in for {contentID}.");

                contentFile = await CurrentProject.CreateNewContentItemAsync(metaData, plugin);

                if (contentFile == null)
                {
                    return;
                }

                HideWaitPanel();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_CONTENT_CREATION);
            }
            finally
            {
                HideWaitPanel();
            }

            // We have our content, we can now open it as we normally would.
            if (contentFile != null)
            {
                CurrentProject.FileExplorer.OpenContentFileCommand.Execute(contentFile);
            }
        }

        /// <summary>Function called when the application is closing.</summary>
        /// <param name="args">The arguments for the command.</param>
        private async Task DoAppClose(AppCloseArgs args)
        {
            try
            {
                // Function to save the application settings.
                void SaveSettings()
                {
                    try
                    {
                        if (args.WindowState == 0)
                        {
                            // Only store the window boundaries when we're in normal window state.
                            Settings.WindowBounds = args.WindowDimensions;
                        }
                        Settings.WindowState = args.WindowState;

                        PersistSettings();
                    }
                    catch (Exception ex)
                    {
                        // If we can't save, then don't stop the close operation.
                        Program.Log.Print("Error saving application settings.", LoggingLevel.Simple);
                        Program.Log.LogException(ex);
                    }
                }

                // Save the project if one is open.
                if ((CurrentProject?.BeforeCloseCommand != null) && (CurrentProject.BeforeCloseCommand.CanExecute(args)))
                {
                    await CurrentProject.BeforeCloseCommand.ExecuteAsync(args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                OnUnload();
                SaveSettings();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_CLOSING);
                args.Cancel = true;
            }
        }

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        public override void OnLoad()
        {
            base.OnLoad();

            NewProject.WaitPanelActivated += NewProject_WaitPanelActivated;
            NewProject.WaitPanelDeactivated += NewProject_WaitPanelDeactivated;
            RecentFiles.WaitPanelActivated += NewProject_WaitPanelActivated;
            RecentFiles.WaitPanelDeactivated += NewProject_WaitPanelDeactivated;
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public override void OnUnload()
        {
            HideWaitPanel();

            CurrentProject = null;

            NewProject.WaitPanelActivated -= NewProject_WaitPanelActivated;
            NewProject.WaitPanelDeactivated -= NewProject_WaitPanelDeactivated;
            RecentFiles.WaitPanelActivated -= NewProject_WaitPanelActivated;
            RecentFiles.WaitPanelDeactivated -= NewProject_WaitPanelDeactivated;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main()
        {
            BrowseProjectCommand = new EditorCommand<object>(DoOpenProjectAsync);
            OpenPackFileCommand = new EditorCommand<object>(DoOpenPackFileAsync, CanOpenProjects);
            SavePackFileCommand = new EditorAsyncCommand<SavePackFileArgs>(DoSaveProjectToPackFileAsync, CanSaveProject);
            AppClosingAsyncCommand = new EditorAsyncCommand<AppCloseArgs>(DoAppClose);
            CreateContentCommand = new EditorCommand<string>(DoCreateContentAsync, CanCreateContent);
        }
        #endregion
    }
}
