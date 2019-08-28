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
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;

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
        /// Property to return the recent files view model.
        /// </summary>
        public IRecentVm RecentFiles
        {
            get;
        }

        /// <summary>
        /// Property to return the settings view model.
        /// </summary>
        public IEditorSettingsVm SettingsVm
        {
            get;
        }

        /// <summary>
        /// Property to return the dialog for opening projects.
        /// </summary>
        public IEditorFileOpenDialogService OpenDialog
        {
            get;
        }

        /// <summary>
        /// Property to return the dialog for saving projects.
        /// </summary>
        public IEditorFileSaveAsDialogService SaveDialog
        {
            get;
        }

        /// <summary>
        /// Property to return the undo service for the application.
        /// </summary>
        public IUndoService UndoService
        {
            get;
        }

        /// <summary>
        /// Property to return a list of content plug ins that can create their own content.
        /// </summary>
        public IReadOnlyList<IContentPlugInMetadata> ContentCreators
        {
            get;
        }

        /// <summary>Initializes a new instance of the MainParameters class.</summary>
        /// <param name="newProject">The new project view model.</param>
        /// <param name="recent">The recent files view model.</param>
        /// <param name="contentCreators">A list of content plug ins that can create their own content.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <param name="openDialog">A dialog service used for opening files.</param>
        /// <param name="saveDialog">A dialog service used for saving files.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public MainParameters(IStageNewVm newProject,
            IRecentVm recent,
            IEditorSettingsVm editorSettings,
            IReadOnlyList<IContentPlugInMetadata> contentCreators,
            ViewModelFactory viewModelFactory,
            IEditorFileOpenDialogService openDialog,
            IEditorFileSaveAsDialogService saveDialog)
            : base(viewModelFactory)
        {
            OpenDialog = openDialog ?? throw new ArgumentNullException(nameof(openDialog));
            SaveDialog = saveDialog ?? throw new ArgumentNullException(nameof(saveDialog));
            NewProject = newProject ?? throw new ArgumentNullException(nameof(newProject));
            RecentFiles = recent ?? throw new ArgumentNullException(nameof(recent));
            SettingsVm = editorSettings ?? throw new ArgumentNullException(nameof(editorSettings));
            ContentCreators = contentCreators ?? throw new ArgumentNullException(nameof(contentCreators));
        }
    }
}
