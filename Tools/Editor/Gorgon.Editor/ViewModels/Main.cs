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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Converters;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.Views;
using Gorgon.IO;
using Newtonsoft.Json;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for the main window.
    /// </summary>
    internal class Main
        : ViewModelBase<MainParameters, IHostContentServices>, IMain
    {
        #region Variables.
        // The project manager used to handle project data.
        private ProjectManager _projectManager;
        // The factory used to create view models.
        private ViewModelFactory _viewModelFactory;
        // The currently active project.
        private IProjectEditor _currentProject;
        // The project open dialog service.
        private EditorFileOpenDialogService _openDialog;
        // The directory locator service.
        private IDirectoryLocateService _directoryLocator;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the settings for the application.
        /// </summary>
        public Editor.EditorSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a list of content plugins that can create their own content.
        /// </summary>
        public IReadOnlyList<IContentPlugInMetadata> ContentCreators
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the view model for the settings view.
        /// </summary>
        public IEditorSettings SettingsViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the view model for the new project child view.
        /// </summary>
        public INewProject NewProject
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the recent files view model for the recent files child view.
        /// </summary>
        public IRecent RecentFiles
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the view model for the current project.
        /// </summary>
        public IProjectEditor CurrentProject
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
        public string Text => _currentProject == null ? Resources.GOREDIT_CAPTION_NO_FILE : string.Format(Resources.GOREDIT_CAPTION_FILE, _currentProject.ProjectTitle);

        /// <summary>
        /// Property to return the current clipboard context.
        /// </summary>
        public IClipboardHandler ClipboardContext => CurrentProject?.ClipboardContext;

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
        /// Property to return the command used to query before closing the application.
        /// </summary>
        public IEditorAsyncCommand<AppCloseArgs> AppClosingAsyncCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to save the project metadata.
        /// </summary>
        /// <param name="project">The project containing the metadata to save.</param>
        private void SaveProjectMetaData(IProjectEditor project)
        {
            // Ensure that our metadata is up to date in the current project.
            if ((project?.SaveProjectMetadataCommand == null) || (!project.SaveProjectMetadataCommand.CanExecute(null)))
            {
                return;
            }

            project.SaveProjectMetadataCommand.Execute(null);
        }
        
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
        /// Handles the PropertyChanged event of the CurrentProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentProject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IProjectEditor.ClipboardContext):
                    NotifyPropertyChanged(nameof(ClipboardContext));
                    break;
                case nameof(IProjectEditor.ProjectTitle):
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
        /// Function to import the project data.
        /// </summary>
        /// <param name="project">The project to import into.</param>
        /// <returns>The imported project view model.</returns>
        private async Task<IProjectEditor> ImportProjectDataAsync(IProject project)
        {
            IProjectEditor projectEditor;

            // Move the directory to a temporary physical location and we'll import from there.
            string tempDir = Path.Combine(project.TempDirectory.FullName, Guid.NewGuid().ToString("N")).FormatDirectory(Path.DirectorySeparatorChar);
            System.IO.Directory.Move(project.FileSystemDirectory.FullName, tempDir);
            System.IO.Directory.CreateDirectory(project.FileSystemDirectory.FullName);

            // Recreate the project.
            project.ProjectItems.Clear();
            projectEditor = await _viewModelFactory.CreateProjectViewModelAsync(project);

            IImportData importArgs = new ImportData
            {
                Destination = projectEditor.FileExplorer.Root
            };

            importArgs.PhysicalPaths.AddRange(System.IO.Directory.GetDirectories(tempDir, "*", SearchOption.TopDirectoryOnly)
                                                                .Select(item => item.FormatDirectory(Path.DirectorySeparatorChar))
                                                                .Concat(System.IO.Directory.GetFiles(tempDir, "*", SearchOption.TopDirectoryOnly)));

            // Temporary event handlers to relay progress from the project view model.
            void UpdateProgressEvent(object sender, ProgressPanelUpdateArgs e) => UpdateProgress(e);
            void HideProgressEvent(object sender, EventArgs e) => HideProgress();

            if (!projectEditor.FileExplorer.ImportCommand.CanExecute(importArgs))
            {
                return null;
            }

            try
            {
                projectEditor.ProgressUpdated += UpdateProgressEvent;
                projectEditor.ProgressDeactivated += HideProgressEvent;

                // Temporarily load in the view model so we can make use of events.
                projectEditor.OnLoad();
                await projectEditor.FileExplorer.ImportCommand.ExecuteAsync(importArgs);
            }
            finally
            {
                projectEditor.OnUnload();

                projectEditor.ProgressUpdated -= UpdateProgressEvent;
                projectEditor.ProgressDeactivated -= HideProgressEvent;

                HostServices.BusyService.SetBusy();
                if (System.IO.Directory.Exists(tempDir))
                {
                    try
                    {
                        System.IO.Directory.Delete(tempDir, true);
                    }
                    catch (Exception ex)
                    {
                        HostServices.Log.Print("[ERROR] Cannot delete import working directory.", LoggingLevel.Simple);
                        HostServices.Log.LogException(ex);
                    }
                }
                HostServices.BusyService.SetIdle();
            }

            return projectEditor;
        }

        /// <summary>
        /// Function to open a project.
        /// </summary>
        /// <param name="path">The path to the project.</param>
        /// <param name="importContent"><b>true</b> to import the content, <b>false</b> to leave as-is.</param>
        private async Task OpenProjectAsync(string path, bool importContent)
        {
            IProjectEditor current = CurrentProject;
            IProjectEditor projectEditor = null;
            IProject project;

            path = Path.GetFullPath(path).FormatDirectory(Path.DirectorySeparatorChar);

            // Check for content/project changes.
            var args = new CancelEventArgs();
            if ((current?.BeforeCloseCommand != null) && (current.BeforeCloseCommand.CanExecute(args)))
            {
                await current.BeforeCloseCommand.ExecuteAsync(args);

                if (args.Cancel)
                {
                    return;
                }
            }

            // Check for the existence of the directory.
            if (!System.IO.Directory.Exists(path))
            {
                if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_NO_PROJECT_DIR, path)) == MessageResponse.Yes)
                {
                    HideWaitPanel();
                    await CreateProjectAsync(path);
                    return;
                }
            }
            else if (!_projectManager.IsGorgonProject(path))
            {
                if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_NOT_GOREDIT_PROJ, path)) == MessageResponse.Yes)
                {
                    HideWaitPanel();
                    await CreateProjectAsync(path);
                    return;
                }
            }

            project = await Task.Run(() => _projectManager.OpenProject(path));

            if (project == null)
            {
                HostServices.Log.Print("[ERROR] No project was returned from the project manager.", LoggingLevel.Simple);
                return;
            }

            // Ensure that the project is completely closed.
            if ((current?.AfterCloseCommand != null) && (current.AfterCloseCommand.CanExecute(null)))
            {
                current.AfterCloseCommand.Execute(null);
            }

            // Close the current project.
            CurrentProject = null;

            // Begin file scanning.
            HideWaitPanel();

            // Perform a content import.
            if (importContent)
            {
                projectEditor = await ImportProjectDataAsync(project);

                if (projectEditor == null)
                {
                    HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_IMPORT, "/"));
                    return;
                }
            }
            else
            {
                projectEditor = await _viewModelFactory.CreateProjectViewModelAsync(project);
            }

            // Update the metadata for the most up-to-date info.
            // Create the new view model for the project.            
            SaveProjectMetaData(projectEditor);
            CurrentProject = projectEditor;

            RecentItem dupe = RecentFiles.Files.FirstOrDefault(item => string.Equals(path, item.FilePath.FormatDirectory(Path.DirectorySeparatorChar),
                                                                        StringComparison.OrdinalIgnoreCase));

            if (dupe != null)
            {
                RecentFiles.Files.Remove(dupe);
            }

            RecentFiles.Files.Add(new RecentItem
            {
                FilePath = path,
                LastUsedDate = DateTime.Now
            });

            HostServices.Log.Print($"Project '{projectEditor.ProjectTitle}' was loaded from '{path}'.", LoggingLevel.Simple);
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
                SaveProjectMetaData(CurrentProject);

                HostServices.Log.Print($"Opening '{recentItem.FilePath}'...", LoggingLevel.Simple);

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, recentItem.FilePath.Ellipses(45, true)), null));

                await OpenProjectAsync(recentItem.FilePath, false);
            }
            catch (DirectoryNotFoundException dex)
            {
                HostServices.MessageDisplay.ShowError(dex);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_OPENING_PROJECT);
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
                IProjectEditor current = CurrentProject;
                string path = _openDialog.GetFilename();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                DirectoryInfo dir = _directoryLocator.GetDirectory(GetInitialProjectDirectory(), Resources.GOREDIT_CAPTION_SELECT_PROJECT_DIR);

                if (dir== null)
                {
                    return;
                }

                string target = Path.Combine(dir.FullName, Path.GetFileName(path));

                // If the target directory already exists, and it has items in it, then prompt the user.
                if ((System.IO.Directory.Exists(target)) && (System.IO.Directory.GetFileSystemEntries(target).Length > 0))
                {
                    if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_WORKSPACE_PATH_EXISTS, target)) == MessageResponse.No)
                    {
                        return;
                    }
                }

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, path.Ellipses(65, true)), null));

                HostServices.Log.Print($"Opening '{path}'...", LoggingLevel.Simple);

                // Create the project by copying data into the folder structure.
                bool isEditorFile = await _projectManager.ExtractPackFileProjectAsync(path, target);

                if (!System.IO.Directory.Exists(target))
                {
                    HostServices.Log.Print("[ERROR] No project was returned from the project manager.", LoggingLevel.Simple);
                    return;
                }

                await OpenProjectAsync(target, true);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_OPENING_PROJECT);
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

                HostServices.Log.Print($"Opening '{path.FullName}'...", LoggingLevel.Simple);

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING, path.FullName.Ellipses(65, true)), null));

                await OpenProjectAsync(path.FullName, false);
            }
            catch (DirectoryNotFoundException dex)
            {
                HostServices.MessageDisplay.ShowError(dex);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_OPENING_PROJECT);
            }
            finally
            {
                PersistSettings();
                HideWaitPanel();
                HideProgress();
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
        private async Task CreateProjectAsync(string directory)
        {
            IProjectEditor current = CurrentProject;

            ShowWaitPanel(Resources.GOREDIT_TEXT_CREATING_PROJECT);            

            try
            {
                // Check for content/project changes.
                var args = new CancelEventArgs();
                if ((current?.BeforeCloseCommand != null) && (current.BeforeCloseCommand.CanExecute(args)))
                {
                    await current.BeforeCloseCommand.ExecuteAsync(args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }
                
                IProject project = await Task.Run(() => _projectManager.CreateProject(directory));
                CurrentProject = null;

                if ((current?.AfterCloseCommand != null) && (current.AfterCloseCommand.CanExecute(null)))
                {
                    current.AfterCloseCommand.Execute(null);
                }

                // Unload the current project.                
                CurrentProject = await _viewModelFactory.CreateProjectViewModelAsync(project);

                RecentItem dupe = RecentFiles.Files.FirstOrDefault(item => string.Equals(item.FilePath.FormatDirectory(Path.DirectorySeparatorChar),
                                                                                         Settings.LastProjectWorkingDirectory, StringComparison.OrdinalIgnoreCase));

                if (dupe != null)
                {
                    RecentFiles.Files.Remove(dupe);
                }

                RecentFiles.Files.Add(new RecentItem
                {
                    FilePath = directory,
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
                await CreateProjectAsync(NewProject.WorkspacePath.FullName);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CREATE_PROJECT);
            }
            finally
            {
                PersistSettings();
                HideWaitPanel();
            }
        }

        /// <summary>Function called when the application is closing.</summary>
        /// <param name="args">The arguments for the command.</param>
        private async Task DoAppCloseAsync(AppCloseArgs args)
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
                        HostServices.Log.Print("Error saving application settings.", LoggingLevel.Simple);
                        HostServices.Log.LogException(ex);
                    }
                }

                IProjectEditor current = CurrentProject;

                // Save the project if one is open.
                if ((current?.BeforeCloseCommand != null) && (current.BeforeCloseCommand.CanExecute(args)))
                {
                    await current.BeforeCloseCommand.ExecuteAsync(args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                try
                {
                    OnUnload();
                    SaveSettings();
                }
                catch (Exception ex)
                {
                    HostServices.Log.Print("[ERROR] Failed to unload main view model!", LoggingLevel.Simple);
                    HostServices.Log.LogException(ex);
                }

                // Ensure that the project is completely closed.
                if ((current?.AfterCloseCommand != null) && (current.AfterCloseCommand.CanExecute(null)))
                {
                    current.AfterCloseCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CLOSING);
                args.Cancel = true;
            }
        }

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(MainParameters injectionParameters)
        {
            Settings = injectionParameters.EditorSettings;
            _projectManager = injectionParameters.ProjectManager;
            _viewModelFactory = injectionParameters.ViewModelFactory;
            _openDialog = injectionParameters.OpenDialog;
            NewProject = injectionParameters.NewProject;
            RecentFiles = injectionParameters.RecentFiles;
            SettingsViewModel = injectionParameters.Settings;
            _directoryLocator = injectionParameters.DirectoryLocater;

            ContentCreators = injectionParameters.ContentCreators;
            RecentFiles.OpenProjectCommand = new EditorCommand<RecentItem>(DoOpenRecentAsync, CanOpenRecent);
            NewProject.CreateProjectCommand = new EditorCommand<object>(DoCreateProjectAsync, CanCreateProject);
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
            AppClosingAsyncCommand = new EditorAsyncCommand<AppCloseArgs>(DoAppCloseAsync);            
        }
        #endregion
    }
}
