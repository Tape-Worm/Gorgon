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
using Gorgon.Collections;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A factory for generating view models and their dependencies.
    /// </summary>
    internal class ViewModelFactory
    {

        #region Variables.
        // The service for displaying message boxes.
        private readonly MessageBoxService _messageBoxService;
        // The service for setting busy state by setting a wait cursor.
        private readonly WaitCursorBusyState _waitCursorService;
        // The project manager for the application.
        private readonly ProjectManager _projectManager;
        // The clip board service to use.
        private readonly ClipboardService _clipboard;
        // The graphics context to use.
        private readonly GraphicsContext _graphicsContext;
        // The directory locator service.
        private readonly DirectoryLocateService _dirLocator;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the settings for the application.
        /// </summary>
        public EditorSettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the file system providers used to read/write project files.
        /// </summary>
        public IFileSystemProviders FileSystemProviders
        {
            get;
        }

        /// <summary>
        /// Property to return the service that displays messages on the UI.
        /// </summary>
        public IMessageDisplayService MessageDisplay => _messageBoxService;

        /// <summary>
        /// Property to return the busy service used to indicate that the UI is locked down.
        /// </summary>
        public IBusyStateService BusyService => _waitCursorService;

        /// <summary>
        /// Property to return the project manager interface.
        /// </summary>
        public IProjectManager ProjectManager => _projectManager;

        /// <summary>
        /// Property to return the cilpboard access service.
        /// </summary>
        public IClipboardService Clipboard => _clipboard;

        /// <summary>
        /// Property to return the graphics context for the application.
        /// </summary>
        public IGraphicsContext Graphics => _graphicsContext;

        /// <summary>
        /// Property to return the directory locator service used to select directories on the physical file system.
        /// </summary>
        public IDirectoryLocateService DirectoryLocator => _dirLocator;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create the main view model and any child view models.
        /// </summary>
        /// <param name="workspace">The directory to use for the workspace.</param>
        /// <returns>A new instance of the main view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
        public IMain CreateMainViewModel(DirectoryInfo workspace)
        {
            var newProjectVm = new StageNewVm
            {
                GPUName = _graphicsContext.Graphics.VideoAdapter.Name
            };
            var mainVm = new Main();

            newProjectVm.Initialize(new StageNewVmParameters(workspace, this));

            mainVm.Initialize(new MainParameters(newProjectVm, 
                                                this, 
                                                new EditorFileOpenDialogService(Settings, FileSystemProviders)));

            return mainVm;
        }

        /// <summary>
        /// Function to create a file explorer node view model for a file.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service used to manipulate the underlying physical file system.</param>
        /// <param name="parent">The parent for the node.</param>
        /// <param name="file">The file system file to wrap in the view model.</param>
        /// <returns>The new file explorer node view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="fileSystemService"/> or the <paramref name="file"/> parameter is <b>null</b>.</exception>
        public IFileExplorerNodeVm CreateFileExplorerFileNodeVm(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parent, FileInfo file)
        {
            var result = new FileExplorerFileNodeVm();

            // TODO: Add links as children.
            result.Initialize(new FileExplorerNodeParameters(file.Name, file.FullName, project, this, fileSystemService)
            {
                Parent = parent
            });

            return result;
        }

        /// <summary>
        /// Function to create a file explorer node view model for a directory.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service used to manipulate the underlying physical file system.</param>
        /// <param name="parentNode">The parent for the node.</param>
        /// <param name="metadataManager">The metadata manager to use.</param>
        /// <param name="directory">The file system directory to wrap in the view model.</param>
        /// <param name="rootDirectory">The root directory.</param>
        /// <returns>The new file explorer node view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="fileSystemService"/>, <paramref name="parentNode"/>, or the <paramref name="directory"/> parameter is <b>null</b>.</exception>
        public IFileExplorerNodeVm CreateFileExplorerDirectoryNodeVm(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parentNode, IMetadataManager metadataManager, DirectoryInfo directory)
        {
            var result = new FileExplorerDirectoryNodeVm();

            var children = new ObservableCollection<IFileExplorerNodeVm>();

            foreach (DirectoryInfo subDir in metadataManager.GetIncludedDirectories(directory.FullName))
            {
                children.Add(CreateFileExplorerDirectoryNodeVm(project, fileSystemService, result, metadataManager, subDir));
            }

            foreach (FileInfo file in metadataManager.GetIncludedFiles(directory.FullName))
            {
                children.Add(CreateFileExplorerFileNodeVm(project, fileSystemService, result, file));
            }

            result.Initialize(new FileExplorerNodeParameters(directory.Name, directory.FullName.FormatDirectory(Path.DirectorySeparatorChar), project, this, fileSystemService)
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
        /// <param name="autoInclude"><b>true</b> to automatically include any and all file system objects in the project, or <b>false</b> to only use what is in the metadata.</param>
        /// <returns>The file explorer view model.</returns>
        private IFileExplorerVm CreateFileExplorerViewModel(IProject project, IMetadataManager metadataManager, IFileSystemService fileSystemService, bool autoInclude)
        {
            project.ProjectWorkSpace.Refresh();
            if (!project.ProjectWorkSpace.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            var result = new FileExplorerVm();
            var nodes = new ObservableCollection<IFileExplorerNodeVm>();

            var root = new FileExplorerDirectoryNodeVm
            {
                IsExpanded = true,
                Included = true
            };

            bool showExternal = project.ShowExternalItems;
            project.ShowExternalItems = autoInclude;

            foreach (DirectoryInfo rootDir in metadataManager.GetIncludedDirectories(project.ProjectWorkSpace.FullName))
            {
                nodes.Add(CreateFileExplorerDirectoryNodeVm(project, fileSystemService, root, metadataManager, rootDir));
            }

            foreach (FileInfo file in metadataManager.GetIncludedFiles(project.ProjectWorkSpace.FullName))
            {
                nodes.Add(CreateFileExplorerFileNodeVm(project, fileSystemService, root, file));
            }

            if (autoInclude)
            {
                foreach (IFileExplorerNodeVm node in nodes.Traverse(p => p.Children))
                {
                    node.Included = true;
                    project.Metadata.IncludedPaths.Add(new IncludedFileSystemPathMetadata(node.FullPath));
                }
            }

            project.ShowExternalItems = showExternal;                        

            // This is a special node, used internally.
            root.Initialize(new FileExplorerNodeParameters("/", project.ProjectWorkSpace.FullName.FormatDirectory(Path.DirectorySeparatorChar), project, this, fileSystemService)
            {                
                Children = nodes
            });

            result.Initialize(new FileExplorerParameters(fileSystemService,
                                                        metadataManager,
                                                        root,
                                                        project,
                                                        this));

            return result;
        }

        /// <summary>
        /// Function to create a project view model.
        /// </summary>
        /// <param name="projectData">The project data to assign to the project view model.</param>
        /// <param name="autoInclude"><b>true</b> to automatically include any and all file system objects in the project, or <b>false</b> to only use what is in the metadata.</param>
        /// <returns>The project view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectData"/> parameter is <b>null</b>.</exception>
        public IProjectVm CreateProjectViewModel(IProject projectData, bool autoInclude)
        {
            if (projectData == null)
            {
                throw new ArgumentNullException(nameof(projectData));
            }
            
            var result = new ProjectVm();
            var fileSystemService = new FileSystemService(projectData.ProjectWorkSpace);

            var metaDataManager = new MetadataManager(projectData, new SqliteMetadataProvider(projectData.MetadataFile));
            metaDataManager.Load();

            result.FileExplorer = CreateFileExplorerViewModel(projectData, metaDataManager, fileSystemService, autoInclude);
            result.Initialize(new ProjectVmParameters(projectData, metaDataManager, this));

            return result;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelFactory"/> class.
        /// </summary>
        /// <param name="settings">The settings for the application.</param>
        /// <param name="graphics">The graphics context for the application.</param>
        /// <param name="providers">The providers used to open/save files.</param>
        /// <param name="projectManager">The application project manager.</param>
        /// <param name="messages">The message dialog service.</param>
        /// <param name="waitState">The wait state service.</param>
        /// <param name="clipboardService">The application clipboard service.</param>
        ///<param name="dirLocatorService">The directory locator service.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public ViewModelFactory(EditorSettings settings, 
                                GraphicsContext graphics, 
                                FileSystemProviders providers, 
                                ProjectManager projectManager, 
                                MessageBoxService messages, 
                                WaitCursorBusyState waitState, 
                                ClipboardService clipboardService, 
                                DirectoryLocateService dirLocatorService)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _graphicsContext = graphics ?? throw new ArgumentNullException(nameof(graphics));
            FileSystemProviders = providers ?? throw new ArgumentNullException(nameof(providers));
            _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            _messageBoxService = messages ?? throw new ArgumentNullException(nameof(messages));
            _waitCursorService = waitState ?? throw new ArgumentNullException(nameof(waitState));
            _clipboard = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _dirLocator = dirLocatorService ?? throw new ArgumentNullException(nameof(dirLocatorService));
        }
        #endregion
    }
}
