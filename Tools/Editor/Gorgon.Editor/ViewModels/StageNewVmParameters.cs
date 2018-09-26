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
// Created: September 17, 2018 8:44:42 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IStageNewVm"/> view model.
    /// </summary>
    internal class StageNewVmParameters
        : ViewModelCommonParameters
    {
        /// <summary>
        /// Property to return the work space directory to use.
        /// </summary>
        public DirectoryInfo WorkspaceDirectory
        {
            get;
        }

        /// <summary>
        /// Property to return the project manager.
        /// </summary>
        public IProjectManager ProjectManager
        {
            get;
        }

        /// <summary>
        /// Property to return the editor settings.
        /// </summary>
        public EditorSettings EditorSettings
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageNewVmParameters"/> class.
        /// </summary>
        /// <param name="projectManager">The project manager for the application.</param>
        /// <param name="workspaceDirectory">The workspace to directory to use.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <param name="settings">The settings for the editor.</param>
        /// <param name="messageDisplay">The message display service to use.</param>
        /// <param name="busyService">The busy state service to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public StageNewVmParameters(IProjectManager projectManager, DirectoryInfo workspaceDirectory, EditorSettings settings, ViewModelFactory viewModelFactory, IMessageDisplayService messageDisplay, IBusyStateService busyService)
            : base(viewModelFactory, messageDisplay, busyService)
        {
            ProjectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            WorkspaceDirectory = workspaceDirectory ?? throw new ArgumentNullException(nameof(workspaceDirectory));
            EditorSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
    }
}
