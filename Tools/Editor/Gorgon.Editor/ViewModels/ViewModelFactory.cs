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
// Created: August 28, 2018 12:43:55 PM
// 
#endregion

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A factory for generating view models and their dependencies.
    /// </summary>
    internal class ViewModelFactory
    {
        #region Variables.
        // The settings for the application.
        private readonly EditorSettings _settings;
        // The service for displaying message boxes.
        private readonly MessageBoxService _messageBoxService;
        // The service for setting busy state by setting a wait cursor.
        private readonly WaitCursorBusyState _waitCursorService;
        // The project manager for the application.
        private readonly ProjectManager _projectManager;
        #endregion
        
        #region Methods.
        /// <summary>
        /// Function to create the main view model and any child view models.
        /// </summary>
        /// <returns>A new instance of the main view model.</returns>
        public IMain CreateMainViewModel()
        {
            var newProjectVm = new NewProject();
            var mainVm = new Main();

            newProjectVm.Initialize(new NewProjectParameters(_projectManager, 
                                                            new WorkspaceTester(_projectManager), 
                                                            _settings, 
                                                            this, 
                                                            _messageBoxService, 
                                                            _waitCursorService));

            mainVm.Initialize(new MainParameters(_projectManager, 
                                                newProjectVm, 
                                                this, 
                                                _messageBoxService, 
                                                _waitCursorService));

            return mainVm;
        }

        /// <summary>
        /// Function to create a file explorer node view model for a file.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="parent">The parent for the node.</param>
        /// <param name="file">The file system file to wrap in the view model.</param>
        /// <returns>The new file explorer node view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, or the <paramref name="file"/> parameter is <b>null</b>.</exception>
        public IFileExplorerNodeVm CreateFileExplorerFileNodeVm(IProject project, IFileExplorerNodeVm parent, FileInfo file)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var result = new FileExplorerFileNodeVm();

            // TODO: Add links as children.
            result.Initialize(new FileExplorerNodeParameters(project, file, this, _messageBoxService, _waitCursorService)
            {
                Parent = parent
            });

            return result;
        }

        /// <summary>
        /// Function to create a file explorer node view model for a directory.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="parentNode">The parent for the node.</param>
        /// <param name="metadataManager">The metadata manager to use.</param>
        /// <param name="directory">The file system directory to wrap in the view model.</param>
        /// <param name="rootDirectory">The root directory.</param>
        /// <returns>The new file explorer node view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="parentNode"/>, or the <paramref name="directory"/> parameter is <b>null</b>.</exception>
        public IFileExplorerNodeVm CreateFileExplorerDirectoryNodeVm(IProject project, IFileExplorerNodeVm parentNode, IMetadataManager metadataManager, DirectoryInfo directory)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (parentNode == null)
            {
                throw new ArgumentNullException(nameof(parentNode));
            }

            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (metadataManager == null)
            {
                throw new ArgumentNullException(nameof(metadataManager));
            }

            var result = new FileExplorerDirectoryNodeVm();

            var children = new ObservableCollection<IFileExplorerNodeVm>();

            foreach (DirectoryInfo subDir in metadataManager.GetIncludedDirectories(directory.FullName))
            {
                children.Add(CreateFileExplorerDirectoryNodeVm(project, result, metadataManager, subDir));
            }

            foreach (FileInfo file in metadataManager.GetIncludedFiles(directory.FullName))
            {
                children.Add(CreateFileExplorerFileNodeVm(project, result, file));
            }

            result.Initialize(new FileExplorerNodeParameters(project, directory, this, _messageBoxService, _waitCursorService)
            {
                Parent = parentNode,
                Children = children
            });            

            return result;
        }

        /// <summary>
        /// Function to create a file explorer view model.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="metadataManager">The metadata manager to use.</param>
        /// <param name="fileSystemService">The file system service for the project.</param>
        /// <returns>The file explorer view model.</returns>
        private IFileExplorerVm CreateFileExplorerViewModel(IProject project, IMetadataManager metadataManager, IFileSystemService fileSystemService)
        {
            project.ProjectWorkSpace.Refresh();
            if (!project.ProjectWorkSpace.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            var result = new FileExplorerVm();
            var nodes = new ObservableCollection<IFileExplorerNodeVm>();

            var root = new FileExplorerDirectoryNodeVm();

            foreach (DirectoryInfo rootDir in metadataManager.GetIncludedDirectories(project.ProjectWorkSpace.FullName))
            {
                nodes.Add(CreateFileExplorerDirectoryNodeVm(project, root, metadataManager, rootDir));
            }

            foreach (FileInfo file in metadataManager.GetIncludedFiles(project.ProjectWorkSpace.FullName))
            {
                nodes.Add(CreateFileExplorerFileNodeVm(project, root, file));
            }

            // This is a special node, used internally.
            root.Initialize(new FileExplorerNodeParameters(project, project.ProjectWorkSpace, this, _messageBoxService, _waitCursorService)
            {                
                Children = nodes
            });

            result.Initialize(new FileExplorerParameters(project,
                                                        fileSystemService,
                                                        metadataManager,
                                                        root,
                                                        this, 
                                                        _messageBoxService, 
                                                        _waitCursorService));

            return result;
        }

        /// <summary>
        /// Function to create a project view model.
        /// </summary>
        /// <param name="projectData">The project data to assign to the project view model.</param>
        /// <returns>The project view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectData"/> parameter is <b>null</b>.</exception>
        public IProjectVm CreateProjectViewModel(IProject projectData)
        {
            if (projectData == null)
            {
                throw new ArgumentNullException(nameof(projectData));
            }
            
            var result = new ProjectVm();
            var fileSystemService = new FileSystemService(projectData.ProjectWorkSpace);

            var metaDataManager = new MetadataManager(projectData, new SqliteMetadataProvider(projectData.MetadataFile));

            result.FileExplorer = CreateFileExplorerViewModel(projectData, metaDataManager, fileSystemService);
            result.Initialize(new ProjectVmParameters(projectData,
                                                    metaDataManager,
                                                    this,
                                                    _messageBoxService,
                                                    _waitCursorService));

            return result;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelFactory"/> class.
        /// </summary>
        /// <param name="settings">The settings for the application.</param>
        /// <param name="projectManager">The application project manager.</param>
        /// <param name="messages">The message dialog service.</param>
        /// <param name="waitState">The wait state service.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public ViewModelFactory(EditorSettings settings, ProjectManager projectManager, MessageBoxService messages, WaitCursorBusyState waitState)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            _messageBoxService = messages ?? throw new ArgumentNullException(nameof(messages));
            _waitCursorService = waitState ?? throw new ArgumentNullException(nameof(waitState));
        }
        #endregion
    }
}
