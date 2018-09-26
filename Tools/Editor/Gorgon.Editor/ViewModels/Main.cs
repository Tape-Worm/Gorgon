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
using System.IO;
using Gorgon.Core;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

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
        private IEditorFileDialogService _openDialog;
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
        /// Property to return the command used to assign a project to the application.
        /// </summary>
        public IEditorCommand<IProject> AssignProjectCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the current clipboard context.
        /// </summary>
        public IClipboardHandler ClipboardContext => CurrentProject?.ClipboardContext;

        /// <summary>
        /// Property to return the command used to open a project.
        /// </summary>        
        public IEditorCommand<object> OpenProjectCommand
        {
            get;
        }
        #endregion

        #region Methods.
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
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(MainParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(MainParameters.BusyState), nameof(injectionParameters));
            NewProject = injectionParameters.NewProject ?? throw new ArgumentMissingException(nameof(MainParameters.NewProject), nameof(injectionParameters));
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
        /// Function to assign a project to the project view.
        /// </summary>
        /// <param name="project">The project to assign.</param>
        private void DoAssignProject(IProject project)
        {
            _busyService.SetBusy();

            try
            {
                CurrentProject = _viewModelFactory.CreateProjectViewModel(project, false);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_CREATE_PROJECT);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function called to open a project.
        /// </summary>
        private async void DoOpenProjectAsync()
        {
            try
            {
                string path = _openDialog.GetFilename();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                ShowWaitPanel(new WaitPanelActivateArgs(string.Format(Resources.GOREDIT_TEXT_OPENING_PROJECT, path.Ellipses(65, true)), null));

                (IProject project, bool hasMetadata, bool isUpgraded) = await _projectManager.OpenProjectAsync(path, NewProject.WorkspacePath);

                if (project == null)
                {
                    return;
                }

                IProjectVm projectVm = _viewModelFactory.CreateProjectViewModel(project, !hasMetadata);

                // The project should not be in a modified state.
                projectVm.ProjectState = ProjectState.Unmodified;

                CurrentProject = projectVm;

                _settings.LastOpenSavePath = Path.GetDirectoryName(path).FormatDirectory(Path.DirectorySeparatorChar);                
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_OPENING_PROJECT);
            }
            finally
            {
                HideWaitPanel();
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
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main()
        {
            AssignProjectCommand = new EditorCommand<IProject>(DoAssignProject, args => args != null);
            OpenProjectCommand = new EditorCommand<object>(DoOpenProjectAsync);
        }
        #endregion
    }
}
