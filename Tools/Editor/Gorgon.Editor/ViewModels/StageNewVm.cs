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
using System.IO;
using Gorgon.Diagnostics;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A view model for the new project interface.
    /// </summary>
    internal class StageNewVm
        : ViewModelBase<StageNewVmParameters>, IStageNewVm
    {
        #region Variables.
        // The title for the project.
        private string _title = Resources.GOREDIT_NEW_PROJECT;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // The project manager used to build up project info.
        private IProjectManager _projectManager;
        // The computer information object.
        private IGorgonComputerInfo _computerInfo;
        // The directory to use as the work space.
        private DirectoryInfo _workspace;
        // The currently active project.
        private IProjectVm _current;
        // The active GPU name.
        private string _gpuName = Resources.GOREDIT_TEXT_UNKNOWN;        
        #endregion

        #region Properties.
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
        /// Property to return the workspace path.
        /// </summary>
        public DirectoryInfo WorkspacePath
        {
            get => _workspace;
            set
            {
                if (string.Equals(_workspace?.FullName, value?.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                OnPropertyChanging();
                _workspace = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the command to execute when the project should be created.
        /// </summary>
        public IEditorCommand<object> CreateProjectCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the available RAM, in bytes.
        /// </summary>
        public ulong AvailableRam => (ulong)_computerInfo.AvailablePhysicalRAM;

        /// <summary>
        /// Property to return the available drive space on the drive that hosts the working folder.
        /// </summary>
        public ulong AvailableDriveSpace
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the active GPU name.
        /// </summary>
        public string GPUName
        {
            get => _gpuName;
            set
            {
                if (string.Equals(_gpuName, value, StringComparison.CurrentCulture))
                {
                    return;
                }

                OnPropertyChanging();
                _gpuName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the currently active project.
        /// </summary>
        public IProjectVm CurrentProject
        {
            get => _current;
            set
            {
                if (_current == value)
                {
                    return;
                }

                OnPropertyChanging();
                _current = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(StageNewVmParameters injectionParameters)
        {
            _workspace = injectionParameters.WorkspaceDirectory ?? throw new ArgumentMissingException(nameof(StageNewVmParameters.WorkspaceDirectory), nameof(injectionParameters));
            _projectManager = injectionParameters.ProjectManager ?? throw new ArgumentMissingException(nameof(StageNewVmParameters.ProjectManager), nameof(injectionParameters));            
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(StageNewVmParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(StageNewVmParameters.BusyState), nameof(injectionParameters));

            _computerInfo = new GorgonComputerInfo();

            AvailableDriveSpace = (ulong)(new DriveInfo(Path.GetPathRoot(_workspace.FullName))).AvailableFreeSpace;
        }
        #endregion
    }
}
