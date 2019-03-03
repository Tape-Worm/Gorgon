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
// Created: September 17, 2018 8:29:14 AM
// 
#endregion

using System;
using System.Collections.ObjectModel;
using Gorgon.Core;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters to inject into a <see cref="IFileExplorerNodeVm"/> concrete type.
    /// </summary>
    internal class FileExplorerNodeParameters
        : ViewModelCommonParameters
    {
        /// <summary>
        /// Property to return the file system service used to manipulate the underlying file system.
        /// </summary>
        public IFileSystemService FileSystemService
        {
            get;
        }

        /// <summary>
        /// Property to set or return the physical path for the node.
        /// </summary>
        public string PhysicalPath
        {
            get;
        }

        /// <summary>
        /// Property to set or return the child nodes for the view model.
        /// </summary>
        /// <remarks>
        /// This parameter is optional.
        /// </remarks>
        public ObservableCollection<IFileExplorerNodeVm> Children
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the parent of the file system node.
        /// </summary>
        /// <remarks>
        /// This parameter is optional for <see cref="FileExplorerDirectoryNodeVm"/> types, but required for <see cref="FileExplorerFileNodeVm"/>.
        /// </remarks>
        public IFileExplorerNodeVm Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the metadata to assign to the node.
        /// </summary>
        public ProjectItemMetadata Metadata
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerNodeParameters"/> class.
        /// </summary>
        /// <param name="physicalPath">The physical file system object that the node represents.</param>
        /// <param name="project">The project data.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <param name="fileSystemService">The file system service used to manipulate the underlying file system.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public FileExplorerNodeParameters(string physicalPath, IProject project, ViewModelFactory viewModelFactory, IFileSystemService fileSystemService)
            : base(viewModelFactory)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));

            PhysicalPath = physicalPath ?? throw new ArgumentNullException(nameof(physicalPath));

            if (string.IsNullOrWhiteSpace(PhysicalPath))
            {
                throw new ArgumentEmptyException(nameof(physicalPath));
            }

            FileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }
    }
}
