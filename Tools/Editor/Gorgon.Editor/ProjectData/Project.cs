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
// Created: August 29, 2018 8:19:12 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;
using Gorgon.Editor.Metadata;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// The project data.
    /// </summary>
    internal class Project
        : IProject
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the metadata file location.
        /// </summary>
        public FileInfo MetadataFile
        {
            get;
        }

        /// <summary>
        /// Property to set or return the name of the project.
        /// </summary>
        public string ProjectName
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the workspace used by the project.
        /// </summary>
        /// <remarks>
        /// A project workspace is a folder on the local file system that contains a copy of the project content. This folder is transitory, and will be cleaned up upon application exit.
        /// </remarks>
        public DirectoryInfo ProjectWorkSpace
        {
            get;
        }

        /// <summary>
        /// Property to return the list of excluded paths.
        /// </summary>
        public IProjectMetadata Metadata
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether file system objects like directories and files should be shown in the project even when they're not included.
        /// </summary>
        public bool ShowExternalItems
        {
            get;
            set;
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="metadata">The metadata file for the project.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectName"/>, or the <paramref name="metadata"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="projectName"/> parameter is empty.</exception>
        public Project(string projectName, FileInfo metadata)
        {
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));

            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentEmptyException(nameof(projectName));
            }

            MetadataFile = metadata ?? throw new ArgumentNullException(nameof(metadata));
            ProjectWorkSpace = metadata.Directory;
            Metadata = new ProjectMetadata();
        }
        #endregion
    }
}
