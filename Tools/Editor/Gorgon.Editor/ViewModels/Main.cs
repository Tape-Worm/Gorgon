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
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for the main window.
    /// </summary>
    internal class Main
        : ViewModelBase, IMain
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
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the view model for the new project child view.
        /// </summary>
        public INewProject NewProject
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
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the PropertyChanged event of the CurrentProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentProject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
                CurrentProject = _viewModelFactory.CreateProjectViewModel(project);
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

        /// <summary>
        /// Function to perform first time initialization of a view model.
        /// </summary>
        /// <param name="newProject">The new project view model to inject.</param>
        /// <param name="viewModelFactory">The factory used to build view models.</param>
        /// <param name="projectManager">The project manager interface.</param>
        /// <param name="messageService">The message display service.</param>
        /// <param name="busyService">The busy state service.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="newProject"/>, <paramref name="viewModelFactory"/>, <paramref name="projectManager"/>, <paramref name="messageService"/> or the <paramref name="busyService"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// Applications should use this do any initial set up of the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up.
        /// </para>
        /// </remarks>
        public void Initialize(INewProject newProject, ViewModelFactory viewModelFactory, IProjectManager projectManager, IMessageDisplayService messageService, IBusyStateService busyService)
        {
            _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            NewProject = newProject ?? throw new ArgumentNullException(nameof(newProject));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main() => AssignProjectCommand = new EditorCommand<IProject>(DoAssignProject, args => args != null);
        #endregion
    }
}
