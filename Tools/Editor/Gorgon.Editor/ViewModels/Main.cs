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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Plugins;
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
        // The application busy state service.
        private IBusyStateService _busyService;
        // The currently active project.
        private IProjectVm _currentProject;
        // The project open dialog service.
        private IEditorFileOpenDialogService _openDialog;
        // The project save dialog service.
        private IEditorFileSaveAsDialogService _saveDialog;
        // The settings for the project.
        private EditorSettings _settings;
        #endregion

        #region Properties.
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
                NotifyPropertyChanged(nameof(UndoContext));

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
        public IClipboardHandler ClipboardContext => CurrentProject?.ClipboardContext;

        /// <summary>
        /// Property to return the current undo context.
        /// </summary>
        public IUndoHandler UndoContext => CurrentProject?.UndoContext;

        /// <summary>
        /// Property to return the command used to open a project.
        /// </summary>        
        public IEditorCommand<object> OpenProjectCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to save a project.
        /// </summary>
        public IEditorAsyncCommand<SaveProjectArgs> SaveProjectCommand
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
                writer.Write(JsonConvert.SerializeObject(_settings));
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
        private bool CanSaveProject(SaveProjectArgs args) => (_saveDialog.Providers.Writers.Count > 0) 
                                                            && (args?.CurrentProject != null) 
                                                            && ((args.SaveAs) || (args.CurrentProject.ProjectState != ProjectState.Unmodified));

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(MainParameters injectionParameters)
        {
            _settings = injectionParameters.Settings ?? throw new ArgumentNullException(nameof(MainParameters.Settings), nameof(injectionParameters));
            _projectManager = injectionParameters.ProjectManager ?? throw new ArgumentMissingException(nameof(MainParameters.ProjectManager), nameof(injectionParameters));
            _viewModelFactory = injectionParameters.ViewModelFactory ?? throw new ArgumentMissingException(nameof(MainParameters.ViewModelFactory), nameof(injectionParameters));
            _openDialog = injectionParameters.OpenDialog ?? throw new ArgumentMissingException(nameof(MainParameters.OpenDialog), nameof(injectionParameters));
            _saveDialog = injectionParameters.SaveDialog ?? throw new ArgumentMissingException(nameof(MainParameters.SaveDialog), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(MainParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(MainParameters.BusyService), nameof(injectionParameters));
            NewProject = injectionParameters.NewProject ?? throw new ArgumentMissingException(nameof(MainParameters.NewProject), nameof(injectionParameters));
            RecentFiles = injectionParameters.RecentFiles ?? throw new ArgumentMissingException(nameof(MainParameters.RecentFiles), nameof(injectionParameters));

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
                case nameof(IProjectVm.UndoContext):
                    NotifyPropertyChanged(nameof(UndoContext));
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
        /// <returns>The project view model.</returns>
        private async Task OpenProjectAsync(string path)
        {
            IProject project;

            project = await Task.Run(() => _projectManager.OpenProject(new DirectoryInfo(path)));            

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
                        
            await Task.Run(() => _viewModelFactory.FileScanService.Scan(projectVm.FileExplorer.RootNode, UpdateScanProgress, true, true));

            // Update the metadata for the most up-to-date info.
            await projectVm.SaveProjectMetadataAsync();
            CurrentProject = projectVm;
            
            _settings.LastProjectWorkingDirectory = project.ProjectWorkSpace.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            RecentItem dupe = RecentFiles.Files.FirstOrDefault(item => string.Equals(_settings.LastProjectWorkingDirectory, item.FilePath, StringComparison.OrdinalIgnoreCase));

            if (dupe != null)
            {
                RecentFiles.Files.Remove(dupe);
            }

            RecentFiles.Files.Add(new RecentItem
            {
                FilePath = path,
                LastUsedDate = DateTime.Now
            });

            Program.Log.Print($"Project '{projectVm.ProjectTitle}' was loaded from '{path}'.", LoggingLevel.Simple);
        }

        /// <summary>
        /// Function to perform the check for an unsaved project.
        /// </summary>
        /// <returns><b>true</b> if the project was saved, or did not need to be saved, or <b>false</b> if cancelled.</returns>
        private async Task<bool> CheckForUnsavedProjectAsync()
        {
            if ((CurrentProject == null) || (CurrentProject.ProjectState == ProjectState.Unmodified))
            {
                return true;
            }

            MessageResponse response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_UNSAVED_PROJ, CurrentProject.ProjectTitle), allowCancel: true);

            switch (response)
            {
                case MessageResponse.Yes:
                    bool saved = await CreateSaveProjectTask(new SaveProjectArgs(false, CurrentProject));                    
                    return saved;
                case MessageResponse.Cancel:
                    return false;
            }

            return true;
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
                bool isSaved = await CheckForUnsavedProjectAsync();

                if (!isSaved)
                {
                    return;
                }

                var projectDir = new DirectoryInfo(recentItem.FilePath);
                Program.Log.Print($"Opening '{projectDir.FullName}'...", LoggingLevel.Simple);

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, projectDir.FullName.Ellipses(65, true)), null));

                if (!projectDir.Exists)
                {
                    _messageService.ShowError(string.Format(Resources.GOREDIT_ERR_PROJECT_NOT_FOUND, projectDir.FullName));

                    try
                    {
                        RecentFiles.Files.Remove(recentItem);
                    }
                    catch (Exception ex)
                    {
                        Program.Log.Print($"Could not remove the recent item '{recentItem.FilePath}'.", LoggingLevel.Simple);
                        Program.Log.LogException(ex);
                    }

                    return;
                }

                await OpenProjectAsync(projectDir.FullName);
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
        /// Function called to open a project.
        /// </summary>
        private async void DoOpenProjectAsync()
        {
            try
            {
                bool isSaved = await CheckForUnsavedProjectAsync();

                if (!isSaved)
                {
                    return;
                }

                string path = _openDialog.GetFilename();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                Program.Log.Print($"Opening '{path}'...", LoggingLevel.Simple);

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, path.Ellipses(65, true)), null));

                await OpenProjectAsync(path);
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
        /// Function to save the current project.
        /// </summary>
        /// <param name="args">The save project arguments.</param>
        /// <returns><b>true</b> if saved successfully, <b>false</b> if cancelled.</returns>
        private async Task<bool> CreateSaveProjectTask(SaveProjectArgs args)
        {
            var cancelSource = new CancellationTokenSource();
#warning This used to be retrieved from the current project view model. No longer necessary.
            FileInfo projectFile = null;
            FileWriterPlugin writer = null;
            string projectTitle = CurrentProject.ProjectState == ProjectState.New ? string.Empty : CurrentProject.ProjectTitle;

            try
            {
                // Function used to cancel the save operation.
                void CancelOperation() => cancelSource.Cancel();

                string path = projectFile?.FullName;

                if ((args.SaveAs) || (string.IsNullOrWhiteSpace(path)) || (writer == null))
                {
                    if (projectFile != null)
                    {
                        _saveDialog.InitialDirectory = projectFile.Directory;
                        _saveDialog.InitialFilePath = projectFile.Name;
                    }
                    else
                    {
                        var lastSaveDir = new DirectoryInfo(_settings.LastOpenSavePath);

                        if (!lastSaveDir.Exists)
                        {
                            lastSaveDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                        }

                        _saveDialog.InitialDirectory = lastSaveDir;
                        _saveDialog.InitialFilePath = string.Empty;
                    }

                    _saveDialog.CurrentWriter = writer;

                    path = _saveDialog.GetFilename();

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        args.Cancel = true;
                        return false;
                    }

                    projectFile = new FileInfo(path);
                    projectTitle = Path.GetFileNameWithoutExtension(path);
                    writer = _saveDialog.CurrentWriter;

                    Debug.Assert(writer != null, "Must have a writer plug in.");

                    Program.Log.Print($"File writer plug in is now: {writer.Name}.", LoggingLevel.Verbose);
                }

                Program.Log.Print($"Saving to '{projectFile.FullName}'...", LoggingLevel.Simple);
                
                var panelUpdateArgs = new ProgressPanelUpdateArgs
                {
                    Title = Resources.GOREDIT_TEXT_PLEASE_WAIT,
                    Message = string.Format(Resources.GOREDIT_TEXT_SAVING, projectTitle)
                };

                UpdateProgress(panelUpdateArgs);

                // Function used to update the progress meter display.
                void SaveProgress(int currentItem, int totalItems, bool allowCancellation)
                {
                    panelUpdateArgs.CancelAction = allowCancellation ? CancelOperation : (Action)null;
                    panelUpdateArgs.PercentageComplete = (float)currentItem / totalItems;
                    UpdateProgress(panelUpdateArgs);
                }

#warning Disabled until Save As functionality is reinstated.
                //await CurrentProject.PersistProjectAsync(projectTitle, path, writer, SaveProgress, cancelSource.Token);

                if (cancelSource.Token.IsCancellationRequested)
                {
                    args.Cancel = true;
                    return false;
                }

                // Update the current project with the updated info.               
                args.Cancel = cancelSource.Token.IsCancellationRequested;

                if (!args.Cancel)
                {
                    CurrentProject.ProjectState = ProjectState.Unmodified;

                    RecentFiles.Files.Add(new RecentItem
                    {
                        FilePath = path,
                        LastUsedDate = DateTime.Now
                    });
                    Program.Log.Print($"Saved project '{projectTitle}' to '{projectFile.FullName}'...", LoggingLevel.Simple);
                }

                return !args.Cancel;
            }
            finally
            {
                HideProgress();
                cancelSource?.Dispose();
            }
        }

        /// <summary>
        /// Function to save a project file.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private async Task DoSaveProjectAsync(SaveProjectArgs args)
        {
            try
            {
                await CreateSaveProjectTask(args);
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
        /// Function to create a new project.
        /// </summary>
        /// <param name="args">The arguments for creating the project.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async void DoCreateProjectAsync()
        {
            try
            {
                bool saved = await CheckForUnsavedProjectAsync();

                if (!saved)
                {
                    return;
                }

                ShowWaitPanel("Creating project...", "Please wait");
                IProject project = await Task.Run(() => _projectManager.CreateProject(NewProject.WorkspacePath));

                // Unload the current project.
                CurrentProject = null;
                _settings.LastProjectWorkingDirectory = string.Empty;

                CurrentProject = await _viewModelFactory.CreateProjectViewModelAsync(project);
                _settings.LastProjectWorkingDirectory = project.ProjectWorkSpace.FullName.FormatDirectory(Path.DirectorySeparatorChar);

                RecentItem dupe = RecentFiles.Files.FirstOrDefault(item => string.Equals(item.FilePath, _settings.LastProjectWorkingDirectory, StringComparison.OrdinalIgnoreCase));

                if (dupe != null)
                {
                    RecentFiles.Files.Remove(dupe);
                }
                                
                RecentFiles.Files.Add(new RecentItem
                {
                    FilePath = _settings.LastProjectWorkingDirectory,
                    LastUsedDate = DateTime.Now
                });
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
                        _settings.WindowBounds = args.WindowDimensions;
                        _settings.WindowState = args.WindowState;

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
                if (CurrentProject != null)
                {
                    await CurrentProject.SaveProjectMetadataAsync();
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
            OpenProjectCommand = new EditorCommand<object>(DoOpenProjectAsync, CanOpenProjects);
            SaveProjectCommand = new EditorAsyncCommand<SaveProjectArgs>(DoSaveProjectAsync, CanSaveProject);
            AppClosingAsyncCommand = new EditorAsyncCommand<AppCloseArgs>(DoAppClose);
        }
        #endregion
    }
}
