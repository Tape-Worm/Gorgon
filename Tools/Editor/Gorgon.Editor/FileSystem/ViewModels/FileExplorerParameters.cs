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
// Created: September 17, 2018 8:53:59 AM
// 
#endregion

using System;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IFileExplorerVm"/> view model.
    /// </summary>
    internal class FileExplorerParameters
        : ViewModelCommonParameters
    {
        /// <summary>
        /// Property to return the content plugins for the file explorer.
        /// </summary>
        public IContentPlugInManagerService ContentPlugIns => ViewModelFactory.ContentPlugIns;

        /// <summary>
        /// Property to return the directory locator service used to select a directory.
        /// </summary>
        public IDirectoryLocateService DirectoryLocator => ViewModelFactory.DirectoryLocator;

        /// <summary>
        /// Property to set or return the file system service.
        /// </summary>
        public IFileSystemService FileSystemService
        {
            get;
        }

        /// <summary>
        /// Property to return the root node for the file system.
        /// </summary>
        public IFileExplorerNodeVm RootNode
        {
            get;
        }

        /// <summary>
        /// Property to return the service used to search through the file system.
        /// </summary>
        public ISearchService<IFileExplorerNodeVm> FileSearch
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerParameters" /> class.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use for manipulating the virtual file system.</param>
        /// <param name="fileSearch">The service used to search for files in the file explorer.</param>
        /// <param name="rootNode">The root node for the file system tree.</param>
        /// <param name="viewModelFactory">The view model factory.</param>
        /// <param name="project">The project data.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public FileExplorerParameters(IFileSystemService fileSystemService, ISearchService<IFileExplorerNodeVm> fileSearch, IFileExplorerNodeVm rootNode, IProject project, ViewModelFactory viewModelFactory)
            : base(viewModelFactory)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            FileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            RootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
            FileSearch = fileSearch ?? throw new ArgumentNullException(nameof(fileSearch));
        }
    }
}
