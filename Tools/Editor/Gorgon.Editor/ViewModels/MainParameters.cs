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
// Created: September 17, 2018 8:07:42 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters used for injection on the <see cref="Main"/> view model.
    /// </summary>
    internal class MainParameters
        : ViewModelCommonParameters
    {
        /// <summary>
        /// Property to return the new project view model.
        /// </summary>
        public IStageNewVm NewProject
        {
            get;
        }

        /// <summary>
        /// Property to return the project manager used to manage a project.
        /// </summary>
        public IProjectManager ProjectManager
        {
            get;
        }

        /// <summary>
        /// Property to return the dialog for opening projects.
        /// </summary>
        public IEditorFileDialogService OpenDialog
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainParameters"/> class.
        /// </summary>
        /// <param name="settings">The settings for the application.</param>
        /// <param name="projectManager">The project manager for the application.</param>
        /// <param name="newProject">The new project view model.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <param name="openDialog">A dialog service used for opening files.</param>
        /// <param name="messageDisplay">The message display service to use.</param>
        /// <param name="busyService">The busy state service to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public MainParameters(EditorSettings settings, IProjectManager projectManager, IStageNewVm newProject, ViewModelFactory viewModelFactory, IEditorFileDialogService openDialog, IMessageDisplayService messageDisplay, IBusyStateService busyService)
            : base(viewModelFactory, messageDisplay, busyService)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            OpenDialog = openDialog ?? throw new ArgumentNullException(nameof(openDialog));
            ProjectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            NewProject = newProject ?? throw new ArgumentNullException(nameof(newProject));
        }
    }
}
