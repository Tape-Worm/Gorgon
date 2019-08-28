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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.FileSystem;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.IO;
using Gorgon.IO.Providers;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A factory for generating view models and their dependencies.
    /// </summary>
    internal class ViewModelFactory
    {
        #region Variables.
        // The common services for the application and plug ins.
        private readonly IViewModelInjection _viewModelInjection;
        // The project manager for the application.
        private readonly ProjectManager _projectManager;
        // The clip board service to use.
        private readonly ClipboardService _clipboard;
        // The directory locator service.
        private readonly DirectoryLocateService _dirLocator;
        // The service used to scan content files for content plug in associations.
        private readonly FileScanService _fileScanService;
        // A string builder used to construct path names.
        private readonly StringBuilder _pathBuilder = new StringBuilder(512);
        // The file system folder browser.
        private readonly IEditorFileSystemFolderBrowseService _folderBrowser;
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
        /// Property to return the file scanning service used to scan content files for content plugin associations.
        /// </summary>
        public IFileScanService FileScanService => _fileScanService;

        /// <summary>
        /// Property to return the file system providers used to read/write project files.
        /// </summary>
        public IFileSystemProviders FileSystemProviders
        {
            get;
        }

        /// <summary>
        /// Property to return the content plugins for the application.
        /// </summary>
        public IContentPlugInManagerService ContentPlugIns
        {
            get;
        }

        /// <summary>
        /// Property tor eturn the tool plug ins for the application.
        /// </summary>
        public IToolPlugInManagerService ToolPlugIns
        {
            get;
        }

        /// <summary>
        /// Property to return the service that displays messages on the UI.
        /// </summary>
        public IMessageDisplayService MessageDisplay => _viewModelInjection.MessageDisplay;

        /// <summary>
        /// Property to return the busy service used to indicate that the UI is locked down.
        /// </summary>
        public IBusyStateService BusyService => _viewModelInjection.BusyService;

        /// <summary>
        /// Property to return the project manager interface.
        /// </summary>
        public IProjectManager ProjectManager => _projectManager;

        /// <summary>
        /// Property to return the cilpboard access service.
        /// </summary>
        public IClipboardService Clipboard => _clipboard;

        /// <summary>
        /// Property to return the directory locator service used to select directories on the physical file system.
        /// </summary>
        public IDirectoryLocateService DirectoryLocator => _dirLocator;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a file explorer node view model for a file.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service used to manipulate the underlying physical file system.</param>
        /// <param name="parent">The parent for the node.</param>
        /// <param name="file">The file system file to wrap in the view model.</param>
        /// <param name="metaData">[Optional] The metadata for the file.</param>
        /// <returns>The new file explorer node view model.</returns>
        private IFileExplorerNodeVm DoCreateFileExplorerFileNode(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parent, FileInfo file, ProjectItemMetadata metaData, IReadOnlyList<IFileExplorerNodeVm> openFiles)
        {
            var result = new FileExplorerFileNodeVm();

            // If this node already exists, then don't recreate it and just send it back.
            // This way, we don't need to worry about running around trying to update changed nodes.
            _pathBuilder.Length = 0;
            _pathBuilder.Append(parent.FullPath);
            _pathBuilder.Append(file.Name);
            string newPath = _pathBuilder.ToString();

            IFileExplorerNodeVm openFile = openFiles?.FirstOrDefault(item => string.Equals(item.FullPath, newPath, StringComparison.OrdinalIgnoreCase));
            if (openFile != null)
            {
                if (parent.Children.All(item => !string.Equals(item.FullPath, newPath, StringComparison.OrdinalIgnoreCase)))
                {
                    parent.Children.Add(openFile);
                    openFile.Refresh();
                    (openFile as IContentFile)?.RefreshMetadata();
                }
                return openFile;
            }

            result.Initialize(new FileExplorerNodeParameters(file.FullName, project, this, fileSystemService)
            {
                Parent = parent,
                Metadata = metaData
            });

            if (result.Metadata == null)
            {
                if (project.ProjectItems.TryGetValue(result.FullPath, out ProjectItemMetadata existingMetaData))
                {
                    result.Metadata = existingMetaData;
                }
                else
                {
                    result.Metadata = new ProjectItemMetadata();
                }
            }

            parent.Children.Add(result);

            return result;
        }

        /// <summary>
        /// Function to enumerate the physical file system to build the node hierarchy.
        /// </summary>
        /// <param name="project">The project being evaluated.</param>
        /// <param name="fileSystemService">The file system service used to retrieve file system data.</param>
        /// <param name="parent">The parent of the nodes.</param>
        /// <param name="openFiles">A list of files that are open in the editor.</param>
        private void DoEnumerateFileSystemObjects(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parent, IReadOnlyList<IFileExplorerNodeVm> openFiles = null)
        {
            var directoryNodes = new Dictionary<string, IFileExplorerNodeVm>(StringComparer.OrdinalIgnoreCase);
            string parentPhysicalPath;

            if (parent.Parent == null)
            {
                parentPhysicalPath = project.FileSystemDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
                directoryNodes[parentPhysicalPath] = parent;
            }
            else
            {
                parentPhysicalPath = parent.PhysicalPath.FormatDirectory(Path.DirectorySeparatorChar);
                directoryNodes[parentPhysicalPath] = parent;
            }

            foreach (DirectoryInfo directory in fileSystemService.GetDirectories(parentPhysicalPath).OrderBy(item => item.FullName.Length))
            {
                string directoryParentPath = directory.Parent.FullName.FormatDirectory(Path.DirectorySeparatorChar);
                string directoryPath = directory.FullName.FormatDirectory(Path.DirectorySeparatorChar);
                if (!directoryNodes.TryGetValue(directoryParentPath, out IFileExplorerNodeVm parentNode))
                {
                    Program.Log.Print($"ERROR: Directory '{directoryParentPath}' is the parent of '{directory.Name}', but is not found in the index list.", LoggingLevel.Simple);
                    continue;
                }

                IFileExplorerNodeVm node = CreateFileExplorerDirectoryNodeVm(project, fileSystemService, parentNode, directory, true);
                directoryNodes[directoryPath] = node;

                // Get files for this directory.
                foreach (FileInfo file in fileSystemService.GetFiles(directory.FullName, false))
                {
                    DoCreateFileExplorerFileNode(project, fileSystemService, node, file, null, openFiles);
                }
            }

            // Get files for this directory.
            foreach (FileInfo file in fileSystemService.GetFiles(parentPhysicalPath, false))
            {
                DoCreateFileExplorerFileNode(project, fileSystemService, parent, file, null, openFiles);
            }
        }

        /// <summary>
        /// Function to enumerate the physical file system to build the node hierarchy.
        /// </summary>
        /// <param name="project">The project being evaluated.</param>
        /// <param name="fileSystemService">The file system service used to retrieve file system data.</param>
        /// <param name="parent">The parent of the nodes.</param>
        /// <param name="openFiles">A list of files that are open in the editor.</param>
        /// <returns>A hierarchy of nodes representing the physical file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        public void EnumerateFileSystemObjects(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parent, IReadOnlyList<IFileExplorerNodeVm> openFiles)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            DoEnumerateFileSystemObjects(project, fileSystemService, parent, openFiles);
        }

        /// <summary>
        /// Function to enumerate the content plug ins that can create their own content.
        /// </summary>
        /// <returns>A list of plugins that can create their own content.</returns>
        private IReadOnlyList<IContentPlugInMetadata> EnumerateContentCreators()
        {
            IEnumerable<IContentPlugInMetadata> filteredPlugIns = ContentPlugIns.PlugIns.Where(item => item.Value.CanCreateContent)
                                                                                        .Select(item => item.Value)
                                                                                        .OfType<IContentPlugInMetadata>();
            return new List<IContentPlugInMetadata>(filteredPlugIns);
        }

        /// <summary>
        /// Function to retrieve a plug in list item view model based on the plug in passed in.
        /// </summary>
        /// <param name="plugin">The plug in to retrieve data from.</param>
        /// <returns>The view model.</returns>
        private ISettingsPlugInListItem GetPlugInListItem(EditorPlugIn plugin)
        {
            var result = new SettingsPlugInListItem();
            result.Initialize(new SettingsPlugInListItemParameters(plugin, _viewModelInjection));
            return result;
        }

        /// <summary>
        /// Function to retrieve a plug in list item view model based on the plug in passed in.
        /// </summary>
        /// <param name="plugin">The plug in to retrieve data from.</param>
        /// <returns>The view model.</returns>
        private ISettingsPlugInListItem GetPlugInListItem(IGorgonFileSystemProvider plugin)
        {
            var result = new SettingsPlugInListItem();
            result.Initialize(new SettingsPlugInListItemParameters(plugin, _viewModelInjection));
            return result;
        }

        /// <summary>
        /// Function to retrieve a plug in list item view model based on the plug in passed in.
        /// </summary>
        /// <param name="plugin">The plug in to retrieve data from.</param>
        /// <returns>The view model.</returns>
        private ISettingsPlugInListItem GetPlugInListItem(IDisabledPlugIn plugin)
        {
            var result = new SettingsPlugInListItem();
            result.Initialize(new SettingsPlugInListItemParameters(plugin, _viewModelInjection));
            return result;
        }

        /// <summary>
        /// Function to retrieve the list of plug ins view model.
        /// </summary>
        /// <returns>The plug ins list view model.</returns>
        private ISettingsPlugInsList GetPlugInListViewModel()
        {
            IEnumerable<ISettingsPlugInListItem> plugins = FileSystemProviders.Readers
                .Select(item => GetPlugInListItem(item.Value))
                .Concat(FileSystemProviders.Writers.Select(item => GetPlugInListItem(item.Value)))
                .Concat(ContentPlugIns.PlugIns.Select(item => GetPlugInListItem(item.Value)))
                .Concat(ContentPlugIns.Importers.Select(item => GetPlugInListItem(item.Value)))
                .Concat(ToolPlugIns.PlugIns.Select(item => GetPlugInListItem(item.Value)))
                .Concat(FileSystemProviders.DisabledPlugIns.Select(item => GetPlugInListItem(item.Value)))
                .Concat(ContentPlugIns.DisabledPlugIns.Select(item => GetPlugInListItem(item.Value)))
                .Concat(ToolPlugIns.DisabledPlugIns.Select(item => GetPlugInListItem(item.Value)));

            var result = new SettingsPlugInsList();
            result.Initialize(new SettingsPlugInsListParameters(plugins, _viewModelInjection));
            return result;
        }

        /// <summary>
        /// Function to retrieve the list of settings categories from loaded plug ins.
        /// </summary>
        /// <returns>The list of categories.</returns>
        private IEnumerable<ISettingsCategoryViewModel> GetPlugInSettingsCategories()
        {
            var result = new List<ISettingsCategoryViewModel>();

            IEnumerable<EditorPlugIn> plugins = FileSystemProviders.Writers.Select(item => (EditorPlugIn)item.Value)
                .Concat(ContentPlugIns.PlugIns.Select(item => item.Value))
                .Concat(ContentPlugIns.Importers.Select(item => item.Value))
                .Concat(ToolPlugIns.PlugIns.Select(item => item.Value));

            foreach (EditorPlugIn plugin in plugins)
            {
                ISettingsCategoryViewModel settings = plugin.GetPlugInSettings();

                if (settings == null)
                {
                    continue;
                }

                result.Add(settings);
            }

            return result;
        }

        /// <summary>
        /// Function to create the main view model and any child view models.
        /// </summary>
        /// <param name="gpuName">The name of the GPU used by the application.</param>
        /// <returns>A new instance of the main view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
        public IMain CreateMainViewModel(string gpuName)
        {
            ISettingsPlugInsList pluginList = GetPlugInListViewModel();
            var settingsVm = new EditorSettingsVm();
            IEnumerable<ISettingsCategoryViewModel> categories = GetPlugInSettingsCategories();
            settingsVm.Initialize(new EditorSettingsParameters(new[] { pluginList }.Concat(categories), pluginList, MessageDisplay, BusyService));

            var newProjectVm = new StageNewVm
            {
                GPUName = gpuName
            };
            var recentFilesVm = new RecentVm();

            var mainVm = new Main();

            newProjectVm.Initialize(new StageNewVmParameters(this));
            recentFilesVm.Initialize(new RecentVmParameters(this));

            mainVm.Initialize(new MainParameters(newProjectVm,
                                                recentFilesVm,
                                                settingsVm,
                                                EnumerateContentCreators(),
                                                this,
                                                new EditorFileOpenDialogService(Settings, FileSystemProviders),
                                                new EditorFileSaveDialogService(Settings, FileSystemProviders)));

            return mainVm;
        }

        /// <summary>
        /// Function to create a file explorer node view model for a file.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service used to manipulate the underlying physical file system.</param>
        /// <param name="parent">The parent for the node.</param>
        /// <param name="file">The file system file to wrap in the view model.</param>
        /// <param name="metaData">[Optional] The metadata for the file.</param>
        /// <returns>The new file explorer node view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="metadataManager"/>, <paramref name="fileSystemService"/> or the <paramref name="file"/> parameter is <b>null</b>.</exception>
        public IFileExplorerNodeVm CreateFileExplorerFileNodeVm(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parent, FileInfo file, ProjectItemMetadata metaData = null)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            return DoCreateFileExplorerFileNode(project, fileSystemService, parent, file, metaData, null);
        }

        /// <summary>
        /// Function to create a file explorer node view model for a directory.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service used to manipulate the underlying physical file system.</param>
        /// <param name="parentNode">The parent for the node.</param>
        /// <param name="newNodeName">The name of the new node.</param>
        /// <returns>The new file explorer node view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="fileSystemService"/>, <paramref name="parentNode"/>, or the <paramref name="directory"/> parameter is <b>null</b>.</exception>
        public IFileExplorerNodeVm CreateNewDirectoryNode(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parentNode, string newNodeName)
        {
            var parent = new DirectoryInfo(parentNode.PhysicalPath);
            DirectoryInfo directory = string.IsNullOrWhiteSpace(newNodeName) ? fileSystemService.CreateDirectory(parent) : fileSystemService.CreateDirectory(parent, newNodeName);

            return CreateFileExplorerDirectoryNodeVm(project, fileSystemService, parentNode, directory, true);
        }

        /// <summary>
        /// Function to create a file explorer node view model for a directory.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service used to manipulate the underlying physical file system.</param>
        /// <param name="parentNode">The parent for the node.</param>
        /// <param name="metadataManager">The metadata manager to use.</param>
        /// <param name="directory">The file system directory to wrap in the view model.</param>
        /// <param name="addToParent"><b>true</b> to automatically add the node to the parent, <b>false</b> to just return the node without adding to the parent.</param>
        /// <returns>The new file explorer node view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="fileSystemService"/>, <paramref name="parentNode"/>, or the <paramref name="directory"/> parameter is <b>null</b>.</exception>
        public IFileExplorerNodeVm CreateFileExplorerDirectoryNodeVm(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parentNode, DirectoryInfo directory, bool addToParent)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            if (parentNode == null)
            {
                throw new ArgumentNullException(nameof(parentNode));
            }

            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            var result = new FileExplorerDirectoryNodeVm();

            var children = new ObservableCollection<IFileExplorerNodeVm>();

            result.Initialize(new FileExplorerNodeParameters(directory.FullName, project, this, fileSystemService)
            {
                Metadata = new ProjectItemMetadata(),
                Parent = parentNode,
                Children = children
            });

            if (addToParent)
            {
                parentNode.Children.Add(result);
            }

            return result;
        }

        /// <summary>
        /// Function to create a file explorer view model.
        /// </summary>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service for the project.</param>
        /// <returns>The file explorer view model.</returns>
        private FileExplorerVm CreateFileExplorerViewModel(IProject project, IFileSystemService fileSystemService)
        {
            project.FileSystemDirectory.Refresh();
            if (!project.FileSystemDirectory.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, project.FileSystemDirectory.FullName));
            }

            var result = new FileExplorerVm();
            _folderBrowser.FileSystem = result;
            var root = new FileExplorerRootNode();

            // This is a special node, used internally.
            root.Initialize(new FileExplorerNodeParameters(project.FileSystemDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar), project, this, fileSystemService));

            DoEnumerateFileSystemObjects(project, fileSystemService, root);

            var search = new FileSystemSearchSystem(root);

            result.Initialize(new FileExplorerParameters(fileSystemService,
                                                        search,
                                                        root,
                                                        project,
                                                        this));

            // Walk through the content plug ins and register custom search keywords.
            foreach (ContentPlugIn plugin in ContentPlugIns.PlugIns.Values)
            {
                plugin.RegisterSearchKeywords(search);
            }

            return result;
        }

        /// <summary>
        /// Function to create a new dependency node.
        /// </summary>
        /// <param name="project">The project that contains the file system.</param>
        /// <param name="fileSystemService">The file system service for the project.</param>
        /// <param name="parentNode">The node to use as a the parent of the node.</param>
        /// <param name="content">The content object referenced by the dependency node.</param>
        /// <returns>A new dependency node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public DependencyNode CreateDependencyNode(IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm parentNode, IContentFile content)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (parentNode == null)
            {
                throw new ArgumentNullException(nameof(parentNode));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var fileNode = (IFileExplorerNodeVm)content;

            var dependNode = new DependencyNode(parentNode, (IFileExplorerNodeVm)content);
            dependNode.Initialize(new FileExplorerNodeParameters(fileNode.PhysicalPath, project, this, fileSystemService));

            return dependNode;
        }

        /// <summary>
        /// Function to create a project view model.
        /// </summary>
        /// <param name="projectData">The project data to assign to the project view model.</param>
        /// <returns>The project view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectData"/> parameter is <b>null</b>.</exception>
        public async Task<IProjectVm> CreateProjectViewModelAsync(IProject projectData)
        {
            if (projectData == null)
            {
                throw new ArgumentNullException(nameof(projectData));
            }

            var result = new ProjectVm();
            var fileSystemService = new FileSystemService(projectData.FileSystemDirectory);

            await Task.Run(() =>
            {
                FileExplorerVm fileExplorer = CreateFileExplorerViewModel(projectData, fileSystemService);
                result.ContentFileManager = fileExplorer;
                result.FileExplorer = fileExplorer;
            });

            var previewer = new ContentPreviewVm();
            var thumbDirectory = new DirectoryInfo(Path.Combine(projectData.TempDirectory.FullName, "thumbs"));
            if (!thumbDirectory.Exists)
            {
                thumbDirectory.Create();
                thumbDirectory.Refresh();
            }
            previewer.Initialize(new ContentPreviewVmParameters(result.FileExplorer, result.ContentFileManager, thumbDirectory, this));

            result.ContentPreviewer = previewer;
            result.Initialize(new ProjectVmParameters(projectData, this));

            // Empty this list, it will be rebuilt when we save, and having it lying around is a waste.
            projectData.ProjectItems.Clear();

            return result;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelFactory"/> class.
        /// </summary>
        /// <param name="settings">The settings for the application.</param>
        /// <param name="providers">The providers used to open/save files.</param>
        /// <param name="contentPlugIns">The plugins used for content.</param>
        /// <param name="toolPlugIns">The plugins used for application tools.</param>
        /// <param name="projectManager">The application project manager.</param>
        /// <param name="viewModelInjection">The common services for the application.</param>        
        /// <param name="clipboardService">The application clipboard service.</param>
        /// <param name="dirLocatorService">The directory locator service.</param>
        /// <param name="fileScanService">The file scanning service used to scan content files for content plugin associations.</param>
        /// <param name="folderBrowser">The file system folder browser service.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public ViewModelFactory(EditorSettings settings,
                                FileSystemProviders providers,
                                ContentPlugInService contentPlugIns,
                                ToolPlugInService toolPlugIns,
                                ProjectManager projectManager,
                                IViewModelInjection viewModelInjection,
                                ClipboardService clipboardService,
                                DirectoryLocateService dirLocatorService,
                                FileScanService fileScanService,
                                IEditorFileSystemFolderBrowseService folderBrowser)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            FileSystemProviders = providers ?? throw new ArgumentNullException(nameof(providers));
            ContentPlugIns = contentPlugIns ?? throw new ArgumentNullException(nameof(contentPlugIns));
            ToolPlugIns = toolPlugIns ?? throw new ArgumentNullException(nameof(toolPlugIns));
            _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            _viewModelInjection = viewModelInjection ?? throw new ArgumentNullException(nameof(viewModelInjection));
            _clipboard = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _dirLocator = dirLocatorService ?? throw new ArgumentNullException(nameof(dirLocatorService));
            _fileScanService = fileScanService ?? throw new ArgumentNullException(nameof(fileScanService));
            _folderBrowser = folderBrowser ?? throw new ArgumentNullException(nameof(folderBrowser));
        }
        #endregion
    }
}
