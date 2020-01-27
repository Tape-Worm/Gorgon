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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Native;
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
        // A string builder used to construct path names.
        private readonly StringBuilder _pathBuilder = new StringBuilder(512);
        // The buffer to hold directory paths.
        private readonly Dictionary<string, IDirectory> _directoryBuffer = new Dictionary<string, IDirectory>(StringComparer.OrdinalIgnoreCase);
        // The host content services.
        private readonly HostContentServices _hostContentServices;
        // The file system providers for reading/writing file systems.
        private readonly FileSystemProviders _fileSystemProviders;
        // The settings for the editor.
        private readonly EditorSettings _settings;
        // The project manager.
        private readonly ProjectManager _projectManager;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to send the item specified in the path to the recycle bin.
        /// </summary>
        /// <param name="path">The path to the item to recycle.</param>
        /// <returns><b>true</b> if the item was recycled, <b>false</b> if not.</returns>
        private static bool RecycleFileSystemItem(string path)
        {
            bool isDirectory = (System.IO.File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;

#pragma warning disable IDE0046 // Convert to conditional expression
            if (!Shell32.SendToRecycleBin(path, Shell32.FileOperationFlags.FOF_SILENT | Shell32.FileOperationFlags.FOF_NOCONFIRMATION | Shell32.FileOperationFlags.FOF_WANTNUKEWARNING))
            {
                return false;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return !(isDirectory ? System.IO.Directory.Exists(path) : System.IO.File.Exists(path));
        }

        /// <summary>
        /// Function to enumerate the content plug ins that can create their own content.
        /// </summary>
        /// <returns>A list of plugins that can create their own content.</returns>
        private IReadOnlyList<IContentPlugInMetadata> EnumerateContentCreators() => _hostContentServices.ContentPlugInService.PlugIns
                                                                                                            .Where(item => item.Value.CanCreateContent)
                                                                                                            .Select(item => item.Value)
                                                                                                            .OfType<IContentPlugInMetadata>()
                                                                                                            .ToArray();

        /// <summary>
        /// Function to retrieve a plug in list item view model based on the plug in passed in.
        /// </summary>
        /// <param name="plugin">The plug in to retrieve data from.</param>
        /// <returns>The view model.</returns>
        private ISettingsPlugInListItem CreatePlugInListItem(EditorPlugIn plugin)
        {
            var result = new SettingsPlugInListItem();
            result.Initialize(new SettingsPlugInListItemParameters(plugin, _hostContentServices));
            return result;
        }

        /// <summary>
        /// Function to retrieve a plug in list item view model based on the plug in passed in.
        /// </summary>
        /// <param name="plugin">The plug in to retrieve data from.</param>
        /// <returns>The view model.</returns>
        private ISettingsPlugInListItem CreatePlugInListItem(IGorgonFileSystemProvider plugin)
        {
            var result = new SettingsPlugInListItem();
            result.Initialize(new SettingsPlugInListItemParameters(plugin, _hostContentServices));
            return result;
        }

        /// <summary>
        /// Function to retrieve a plug in list item view model based on the plug in passed in.
        /// </summary>
        /// <param name="plugin">The plug in to retrieve data from.</param>
        /// <returns>The view model.</returns>
        private ISettingsPlugInListItem CreatePlugInListItem(IDisabledPlugIn plugin)
        {
            var result = new SettingsPlugInListItem();
            result.Initialize(new SettingsPlugInListItemParameters(plugin, _hostContentServices));
            return result;
        }

        /// <summary>
        /// Function to retrieve the list of plug ins view model.
        /// </summary>
        /// <returns>The plug ins list view model.</returns>
        private ISettingsPlugInsList CreatePlugInListViewModel()
        {
            IEnumerable<ISettingsPlugInListItem> plugins = _fileSystemProviders.Readers
                .Select(item => CreatePlugInListItem(item.Value))
                .Concat(_fileSystemProviders.Writers.Select(item => CreatePlugInListItem(item.Value)))
                .Concat(_hostContentServices.ContentPlugInService.PlugIns.Select(item => CreatePlugInListItem(item.Value)))
                .Concat(_hostContentServices.ContentPlugInService.Importers.Select(item => CreatePlugInListItem(item.Value)))
                .Concat(_hostContentServices.ToolPlugInService.PlugIns.Select(item => CreatePlugInListItem(item.Value)))
                .Concat(_fileSystemProviders.DisabledPlugIns.Select(item => CreatePlugInListItem(item.Value)))
                .Concat(_hostContentServices.ContentPlugInService.DisabledPlugIns.Select(item => CreatePlugInListItem(item.Value)))
                .Concat(_hostContentServices.ToolPlugInService.DisabledPlugIns.Select(item => CreatePlugInListItem(item.Value)));

            var result = new SettingsPlugInsList();
            result.Initialize(new SettingsPlugInsListParameters(_hostContentServices)
            {
                PlugIns = plugins
            });
            return result;
        }

        /// <summary>
        /// Function to retrieve the list of settings categories from loaded plug ins.
        /// </summary>
        /// <returns>The list of categories.</returns>
        private IEnumerable<ISettingsCategoryViewModel> GetPlugInSettingsCategories()
        {
            var result = new List<ISettingsCategoryViewModel>();

            IEnumerable<EditorPlugIn> plugins = _fileSystemProviders.Writers.Select(item => (EditorPlugIn)item.Value)
                .Concat(_hostContentServices.ContentPlugInService.PlugIns.Select(item => item.Value))
                .Concat(_hostContentServices.ContentPlugInService.Importers.Select(item => item.Value))
                .Concat(_hostContentServices.ToolPlugInService.PlugIns.Select(item => item.Value));

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
        /// Function to create the file explorer view model.
        /// </summary>
        /// <param name="project">The current project.</param>
        /// <param name="fileSystem">The file system for the project.</param>
        /// <param name="tempWriter">The file system for temporary data.</param>
        /// <returns>A new file explorer view model.</returns>
        private FileExplorer CreateFileExplorer(IProject project, IGorgonFileSystemWriter<FileStream> fileSystem, IGorgonFileSystemWriter<Stream> tempWriter)
        {
            var root = new RootDirectory();
            root.Initialize(new RootDirectoryParameters(this)
            {
                RootDirectory = fileSystem.FileSystem.RootDirectory,
                Project = project,
                Path = project.FileSystemDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar)
            });

            // Create view models for all directories/files.
            EnumerateFileSystemObjects(fileSystem.FileSystem, root, project);

            var result = new FileExplorer();
            var clipboardHandler = new FileSystemClipboardHandler(result, _hostContentServices.Clipboard, _hostContentServices.Log);
            result.Initialize(new FileExplorerParameters(this)
            {
                Root = root,
                Project = project,
                FileSystem = fileSystem,
                Clipboard = clipboardHandler,
                SearchService = new FileSystemSearchSystem(root),
                DirectoryLocator = new DirectoryLocateService(),
                BusyService = _hostContentServices.BusyService,
                MessageDisplay = _hostContentServices.MessageDisplay,
                Log = _hostContentServices.Log,
                ContentPlugInService = _hostContentServices.ContentPlugInService,
                EditorSettings = _settings
            });

            return result;
        }

        /// <summary>
        /// Function to duplicate a file view model.
        /// </summary>
        /// <param name="sourceFile">The file view model to duplicate.</param>
        /// <param name="destFile">The virtual file to wrap in new view model.</param>
        /// <param name="parent">The initial parent directory for the files.</param>
        /// <returns>The list of file view models.</returns>
        public IFile DuplicateFile(IFile sourceFile, IGorgonVirtualFile destFile, IDirectory parent)
        {
            _directoryBuffer.Clear();
            _directoryBuffer[parent.FullPath] = parent;

            foreach (IDirectory dir in parent.Directories.Traverse(d => d.Directories))
            {
                _directoryBuffer[dir.FullPath] = dir;
            }

            if (!_directoryBuffer.TryGetValue(destFile.Directory.FullPath, out IDirectory parentDir))
            {
                throw new DirectoryNotFoundException();
            }

            var newFile = new File();
            newFile.Initialize(new FileParameters(this)
            {
                VirtualFile = destFile,
                Parent = parentDir,
                Metadata = new ProjectItemMetadata(sourceFile.Metadata)
            });
            return newFile;
        }

        /// <summary>
        /// Function to create a file view model based on a virtual file object.
        /// </summary>
        /// <param name="file">The virtual file to wrap.</param>
        /// <param name="parent">The initial parent directory for the files.</param>
        /// <param name="project">The current project containing the file system.</param>
        /// <returns>The file view model.</returns>
        public IFile CreateFile(IGorgonVirtualFile file, IDirectory parent)
        {
            var newFile = new File();
            newFile.Initialize(new FileParameters(this)
            {
                VirtualFile = file,
                Parent = parent,
                Metadata = new ProjectItemMetadata()
            });
            return newFile;
        }

        /// <summary>
        /// Function to create a list of file view models based on a list of virtual file objects.
        /// </summary>
        /// <param name="files">The virtual files to wrap.</param>
        /// <param name="parent">The initial parent directory for the files.</param>
        /// <param name="project">The current project containing the file system.</param>
        /// <returns>The list of file view models.</returns>
        public IReadOnlyList<IFile> CreateFiles(IEnumerable<IGorgonVirtualFile> files, IDirectory parent)
        {
            _directoryBuffer.Clear();
            _directoryBuffer[parent.FullPath] = parent;

            foreach (IDirectory dir in parent.Directories.Traverse(d => d.Directories))
            {
                _directoryBuffer[dir.FullPath] = dir;
            }

            var result = new List<IFile>();

            foreach (IGorgonVirtualFile file in files)
            {
                if (!_directoryBuffer.TryGetValue(file.Directory.FullPath, out IDirectory parentDir))
                {
                    throw new DirectoryNotFoundException();
                }

                var newFile = new File();
                newFile.Initialize(new FileParameters(this)
                {
                    VirtualFile = file,
                    Parent = parentDir,
                    Metadata = new ProjectItemMetadata()
                });
                result.Add(newFile);
            }

            return result;
        }

        /// <summary>
        /// Function to create a new directory view model.
        /// </summary>
        /// <param name="directory">The directory wrapped by the view model.</param>
        /// <param name="parent">The parent directory containing this directory.</param>        
        /// <returns>A new directory view model.</returns>
        public IDirectory CreateDirectory(IGorgonVirtualDirectory directory, IDirectory parent)
        {
            var newDir = new Directory();
            newDir.Initialize(new DirectoryParameters(this)
            {
                VirtualDirectory = directory,
                Parent = parent,
                PhysicalPath = Path.Combine(parent.PhysicalPath, directory.Name).FormatDirectory(Path.DirectorySeparatorChar)
            });
            parent.Directories.Add(newDir);
            return newDir;
        }

        /// <summary>
        /// Function to create a directory hierarchy from a list of virtual directory objects.
        /// </summary>
        /// <param name="directories">The list of virtual directories.</param>
        /// <param name="parent">The parent directory for the new directories.</param>
        /// <returns>The hierarchial list of virtual directory view models.</returns>
        public IReadOnlyList<IDirectory> CreateDirectories(IEnumerable<IGorgonVirtualDirectory> directories, IDirectory parent)
        {
            _directoryBuffer.Clear();
            _directoryBuffer[parent.FullPath] = parent;

            // Ensure all children are present so we have the proper parent directory when creating the new directory.
            // For example, if we create A/B/C and A/B already exists, then C's parent should return A/B as it should already exist.
            foreach (IDirectory child in parent.Directories.Traverse(d => d.Directories))
            {
                _directoryBuffer[child.FullPath] = child;
            }

            var result = new List<IDirectory>();

            foreach (IGorgonVirtualDirectory virtDir in directories.OrderBy(item => item.FullPath.Length))
            {
                if (!_directoryBuffer.TryGetValue(virtDir.Parent.FullPath, out IDirectory parentDirectory))
                {
                    continue;
                }

                var newDir = new Directory();
                newDir.Initialize(new DirectoryParameters(this)
                {
                    VirtualDirectory = virtDir,
                    Parent = parentDirectory,
                    PhysicalPath = Path.Combine(parentDirectory.PhysicalPath, virtDir.Name).FormatDirectory(Path.DirectorySeparatorChar)
                });

                _directoryBuffer[newDir.FullPath] = newDir;
                result.Add(newDir);
            }

            return result;
        }

        /// <summary>
        /// Function to enumerate the file system to retrieve the directories and files for the specified parent directory.
        /// </summary>
        /// <param name="fileSystem">The file system containing the directories to enumerate.</param>
        /// <param name="parent">The parent directory to start evaluating.</param>
        /// <param name="project">The project being evaluated.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public void EnumerateFileSystemObjects(IGorgonFileSystem fileSystem, IDirectory parent, IProject project)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var directories = new Dictionary<string, IDirectory>(StringComparer.OrdinalIgnoreCase)
            {
                [parent.FullPath] = parent
            };

            IGorgonVirtualDirectory parentVirtDir = fileSystem.GetDirectory(parent.FullPath);

            if (parentVirtDir == null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, parent.FullPath));
            }

            var subDirs = new List<IGorgonVirtualDirectory>
            {
                parentVirtDir
            };
            subDirs.AddRange(parentVirtDir.Directories.Traverse(d => d.Directories));

            for (int i = 0; i < subDirs.Count; ++i)
            {
                IGorgonVirtualDirectory subDir = subDirs[i];
                IDirectory subDirParent = null;
                IDirectory fileDir = null;

                if (subDir.Parent != null)
                {
                    if (!directories.TryGetValue(subDir.Parent.FullPath, out subDirParent))
                    {
                        // For some reason, the parent isn't in here, so skip this guy.
                        continue;
                    }

                    var newDir = new Directory();
                    newDir.Initialize(new DirectoryParameters(this)
                    {
                        VirtualDirectory = subDir,
                        Parent = subDirParent,
                        Project = project,
                        PhysicalPath = Path.Combine(subDirParent.PhysicalPath, subDir.Name).FormatDirectory(Path.DirectorySeparatorChar)
                    });

                    directories[subDir.FullPath] = newDir;
                    subDirParent.Directories.Add(newDir);
                    fileDir = newDir;
                }
                else
                {
                    subDir = parentVirtDir;
                    fileDir = parent;
                }

                // Add file view models.
                foreach (IGorgonVirtualFile file in subDir.Files.OrderBy(item => item.Name))
                {
                    project.ProjectItems.TryGetValue(file.FullPath, out ProjectItemMetadata metaData);

                    var newFile = new File();
                    newFile.Initialize(new FileParameters(this)
                    {
                        VirtualFile = file,
                        Parent = fileDir,
                        Metadata = metaData ?? new ProjectItemMetadata()
                    });
                    fileDir.Files.Add(newFile);
                }
            }
        }

        /// <summary>
        /// Function to create the main view model and any child view models.
        /// </summary>
        /// <param name="gpuName">The name of the GPU used by the application.</param>
        /// <returns>A new instance of the main view model.</returns>
        public IMain CreateMainViewModel(string gpuName)
        {
            IDirectoryLocateService dirLocator = new DirectoryLocateService();
            ISettingsPlugInsList pluginList = CreatePlugInListViewModel();
            var settingsVm = new EditorSettingsVm();
            IEnumerable<ISettingsCategoryViewModel> categories = GetPlugInSettingsCategories();
            settingsVm.Initialize(new EditorSettingsParameters
            {
                Categories = new[] { pluginList }.Concat(categories),
                PlugInsList = pluginList,
                BusyService = _hostContentServices.BusyService,
                MessageDisplay = _hostContentServices.MessageDisplay,
                Log = _hostContentServices.Log
            });

            var newProjectVm = new NewProject
            {
                GPUName = gpuName,                
            };
            var recentFilesVm = new Recent();

            var mainVm = new Main();

            newProjectVm.Initialize(new NewProjectParameters(this)
            {
                EditorSettings = _settings,
                DirectoryLocator = dirLocator,
                ProjectManager = _projectManager,
                MessageDisplay = _hostContentServices.MessageDisplay,
                Log = _hostContentServices.Log
            });
            recentFilesVm.Initialize(new RecentParameters(this)
            {
                EditorSettings = _settings,
                MessageDisplay = _hostContentServices.MessageDisplay,
                ProjectManager = _projectManager,
                Log = _hostContentServices.Log
            });

            mainVm.Initialize(new MainParameters(this)
            {
                BusyService = _hostContentServices.BusyService,
                ContentPlugIns = _hostContentServices.ContentPlugInService,
                ContentCreators = EnumerateContentCreators(),
                Log = _hostContentServices.Log,
                MessageDisplay = _hostContentServices.MessageDisplay,
                EditorSettings = _settings,
                DirectoryLocater = dirLocator,
                NewProject = newProjectVm,
                Settings = settingsVm,
                RecentFiles = recentFilesVm,
                ProjectManager = _projectManager,                
                OpenDialog = new EditorFileOpenDialogService(_settings, _fileSystemProviders),
                SaveDialog = new EditorFileSaveDialogService(_settings, _fileSystemProviders)                
            });

            return mainVm;
        }

        /// <summary>
        /// Function to create a project view model.
        /// </summary>
        /// <param name="projectData">The project data to assign to the project view model.</param>
        /// <returns>The project view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectData"/> parameter is <b>null</b>.</exception>
        public async Task<IProjectEditor> CreateProjectViewModelAsync(IProject projectData)
        {
            if (projectData == null)
            {
                throw new ArgumentNullException(nameof(projectData));
            }

            _hostContentServices.ContentPlugInService.ProjectDeactivated();

            FileExplorer fileExplorer = null;
            var result = new ProjectEditor();
            var tempFileSystem = new GorgonFileSystem(_hostContentServices.Log);
            IGorgonFileSystemWriter<Stream> tempWriter = null;

            await Task.Run(() =>
            {
                // Create the temporary file system.
                string writeLocation = projectData.TempDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar);

                tempWriter = new GorgonFileSystemWriter(tempFileSystem, tempFileSystem, writeLocation);
                tempFileSystem.Mount(projectData.TempDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar), "/");
                tempWriter.Mount();
                var fileSystem = new GorgonFileSystem(_hostContentServices.Log);
                IGorgonFileSystemWriter<FileStream> writer = new GorgonFileSystemWriter(fileSystem,
                                                                                        fileSystem,
                                                                                        projectData.FileSystemDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar),
                                                                                        RecycleFileSystemItem);
                fileSystem.Mount(projectData.FileSystemDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar));
                writer.Mount();

                fileExplorer = CreateFileExplorer(projectData, writer, tempWriter);
            });

            var previewer = new ContentPreviewVm();
            previewer.Initialize(new ContentPreviewVmParameters(this)
            {
                FileExplorer = fileExplorer,
                Log = _hostContentServices.Log,
                MessageDisplay = _hostContentServices.MessageDisplay,
                ContentFileManager = fileExplorer,
                TempFileSystem = tempWriter,
                BusyService = _hostContentServices.BusyService
            });

            result.Initialize(new ProjectEditorParameters(this)
            {
                FileExplorer = fileExplorer,
                ContentPreviewer = previewer,                
                BusyService = _hostContentServices.BusyService,
                MessageDisplay = _hostContentServices.MessageDisplay,                
                Log = _hostContentServices.Log,
                ContentPlugIns = _hostContentServices.ContentPlugInService,
                ToolPlugIns = _hostContentServices.ToolPlugInService,
                EditorSettings = _settings,
                Project = projectData,
                ProjectManager = _projectManager
            });

            _hostContentServices.ContentPlugInService.ProjectActivated(fileExplorer, tempWriter);

            // Empty this list, it will be rebuilt when we save, and having it lying around is a waste.
            projectData.ProjectItems.Clear();

            return result;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="ViewModelFactory"/> class.</summary>
        /// <param name="settings">The settings for the editor.</param>
        /// <param name="projectManager">The project manager for managing the project file.</param>
        /// <param name="fileSystemProviders">The file system providers used to read/write file systems.</param>
        /// <param name="contentServices">Common host services to pass into plug ins.</param>        
        public ViewModelFactory(EditorSettings settings, ProjectManager projectManager, FileSystemProviders fileSystemProviders, HostContentServices contentServices)
        {
            _settings = settings;
            _projectManager = projectManager;
            _fileSystemProviders = fileSystemProviders;
            _hostContentServices = contentServices;
        }
        #endregion
    }
}
