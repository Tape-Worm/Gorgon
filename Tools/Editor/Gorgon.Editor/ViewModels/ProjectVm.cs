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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Metadata;
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
        : ViewModelBase<ProjectVmParameters>, IProjectVm
    {
        #region Variables.
        // The project data for the view model.
        private IProject _projectData;
        // The metadata manager for the project.
        private IMetadataManager _metadataManager;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // The current project state.
        private ProjectState _state = ProjectState.New;
        // The file explorer view model.
        private IFileExplorerVm _fileExplorer;
        // The current clipboard handler context.
        private IClipboardHandler _clipboardContext;
        #endregion

        #region Properties.
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
        /// Property to set or return whether to show external items that are not included in the project file system.
        /// </summary>
        public bool ShowExternalItems
        {
            get => _projectData.ShowExternalItems;
            set
            {
                if (_projectData.ShowExternalItems == value)
                {
                    return;
                }

                OnPropertyChanging();
                _projectData.ShowExternalItems = value;
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
        /// Property to return the title for the project.
        /// </summary>
        public string ProjectTitle
        {
            get => _projectData.ProjectName;
            private set
            {
                if (string.Equals(_projectData.ProjectName, value, StringComparison.CurrentCulture))
                {
                    return;
                }

                OnPropertyChanging();
                _projectData.ProjectName = value;
                OnPropertyChanged();
            }
        }

        // TODO: Is this really necessary?
        /// <summary>
        /// Property to return the file information for the project if it was opened from a file.
        /// </summary>
        public FileInfo ProjectFile
        {
            // TODO:
            get;
            private set;
        }
        #endregion

        #region Methods.
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
        private void FileExplorer_FileSystemChanged(object sender, EventArgs e)
        {
            // Indicate that the file system has changes now.
            // This will only be triggered if a change is made to a file that is included in our project.  Excluded files don't matter as they don't get saved.
            if (_state != ProjectState.Unmodified)
            {
                return;
            }

            ProjectState = ProjectState.Modified;
        }

        /// <summary>
        /// Function to assign events for the child view models.
        /// </summary>
        private void AssignEvents()
        {
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
        /// Function used to initialize the view model with dependencies.
        /// </summary>
        /// <param name="projectData">The project backing data store.</param>
        /// <param name="messageService">The message display service.</param>
        /// <param name="busyService">The busy state indicator service.</param>
        /// <exception cref="ArgumentNullException">Thrown if any argument is <b>null</b>.</exception>
        protected override void OnInitialize(ProjectVmParameters injectionParameters)
        {
            _projectData = injectionParameters.Project ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.Project), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.BusyState), nameof(injectionParameters));
            _metadataManager = injectionParameters.MetadataManager ?? throw new ArgumentMissingException(nameof(ProjectVmParameters.MetadataManager), nameof(injectionParameters));
        }

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        public override void OnLoad()
        {
            _busyService.SetBusy();

            try
            {
                AssignEvents();

                // Load data from the metadata store.
                _metadataManager.Load();
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
            HideWaitPanel();
            HideProgress();
            UnassignEvents();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectVm"/> class.
        /// </summary>
        public ProjectVm()
        {
        }
        #endregion
    }
}
