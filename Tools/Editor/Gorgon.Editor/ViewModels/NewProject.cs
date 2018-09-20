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
// Created: August 27, 2018 8:51:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A view model for the new project interface.
    /// </summary>
    internal class NewProject
        : ViewModelBase<NewProjectParameters>, INewProject
    {
        #region Variables.
        // The title for the project.
        private string _title = Resources.GOREDIT_NEW_PROJECT;
        // The director for the workspace.
        private DirectoryInfo _workspaceDirectory;
        // The workspace locator.
        private IWorkspaceTester _workspaceTester;
        // The settings for the editor.
        private EditorSettings _settings;
        // The reason for workspace suitablity failure.
        private string _workspaceSuitability;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // The project manager used to build up project info.
        private IProjectManager _projectManager;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the reason why a workspace directory is not suitable.
        /// </summary>
        public string WorkspaceNotSuitableReason
        {
            get => _workspaceSuitability;
            set
            {
                if (string.Equals(_workspaceSuitability, value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _workspaceSuitability = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the title for the project.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (string.Equals(_title, value, StringComparison.CurrentCulture))
                {
                    return;
                }

                OnPropertyChanging();
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the workspace path.
        /// </summary>
        public DirectoryInfo WorkspacePath
        {
            get => _workspaceDirectory;
            set
            {
                if (string.Equals(_workspaceDirectory?.FullName, value?.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _workspaceDirectory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command to execute when a folder is selected for a workspace.
        /// </summary>
        public IEditorCommand<WorkspaceSelectedArgs> WorkspaceSelectedCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when the project should be created.
        /// </summary>
        public IEditorAsyncCommand<ProjectCreateArgs> CreateProjectCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when a new folder is selected.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoWorkspaceSelected(WorkspaceSelectedArgs args)
        {
            _busyService.SetBusy();
            
            try
            {
                WorkspaceNotSuitableReason = string.Empty;

                (bool isAcceptable, string reason) = _workspaceTester.TestForAccessibility(args.WorkspaceLocation);

                if (!isAcceptable)
                {
                    WorkspaceNotSuitableReason = reason;
                }
                else
                {
                    WorkspacePath = args.WorkspaceLocation;
                }
            }
            catch (Exception ex)
            {
                WorkspaceNotSuitableReason = $"{Resources.GOREDIT_ERR_ERROR}: {ex.Message}";
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function called to determine if a project can be created.
        /// </summary>
        /// <returns><b>true</b> if the project can be created, <b>false</b> if not.</returns>
        private bool CanCreateProject() => (!string.IsNullOrWhiteSpace(Title)) && (WorkspacePath != null) && (WorkspacePath.Exists);

        /// <summary>
        /// Function called when a project should be created.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        private async Task DoCreateProjectCommandAsync(ProjectCreateArgs args)
        {
            try
            {
                ShowWaitPanel("Creating project...", "Please wait");

                (bool hasProject, string existingProjectName) = _projectManager.HasProject(WorkspacePath);

                if (hasProject)
                {
                    // If the selected working directory contains a project already, let the user know and give them the option 
                    // to blow it away.
                    if (_messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_EXISTING_PROJECT, existingProjectName)) != MessageResponse.Yes)
                    {
                        return;
                    }
                } else if (existingProjectName != null)
                {
                    // Display the error message if the has project method failed.
                    _messageService.ShowError(existingProjectName);
                    return;
                }

                _settings.LastWorkSpacePath = WorkspacePath.FullName.FormatDirectory(Path.DirectorySeparatorChar);

                args.Project = await Task.Run(() =>_projectManager.CreateProject(WorkspacePath, Title));
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_CREATE_PROJECT);
            }
            finally
            {
                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(NewProjectParameters injectionParameters)
        {
            _workspaceTester = injectionParameters.WorkspaceTester ?? throw new ArgumentMissingException(nameof(NewProjectParameters.WorkspaceTester), nameof(injectionParameters));
            _projectManager = injectionParameters.ProjectManager ?? throw new ArgumentMissingException(nameof(NewProjectParameters.ProjectManager), nameof(injectionParameters));
            _settings = injectionParameters.EditorSettings ?? throw new ArgumentMissingException(nameof(NewProjectParameters.EditorSettings), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(NewProjectParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(NewProjectParameters.BusyState), nameof(injectionParameters));
        }

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        public override void OnLoad()
        {
            base.OnLoad();

            _busyService.SetBusy();

            try
            {
                DirectoryInfo lastWorkSpace;

                if (string.IsNullOrWhiteSpace(_settings.LastWorkSpacePath))
                {
                    lastWorkSpace = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                }
                else
                {
                    lastWorkSpace = new DirectoryInfo(_settings.LastWorkSpacePath);

                    // It's unlikely that this exact directory would exist, so move up to the parent level.
                    if (!lastWorkSpace.Exists)
                    {
                        if ((lastWorkSpace.Parent != null) && (lastWorkSpace.Parent.Exists))
                        {
                            lastWorkSpace = lastWorkSpace.Parent;
                        }
                        else
                        {
                            lastWorkSpace = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                        }
                    }
                }

                WorkspacePath = lastWorkSpace;
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
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="NewProject"/> class.
        /// </summary>
        public NewProject()
        {
            WorkspaceSelectedCommand = new EditorCommand<WorkspaceSelectedArgs>(DoWorkspaceSelected);
            CreateProjectCommand = new EditorAsyncCommand<ProjectCreateArgs>(DoCreateProjectCommandAsync, _ => CanCreateProject());
        }
        #endregion
    }
}
