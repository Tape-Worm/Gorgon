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
// Created: September 1, 2018 9:43:22 PM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// A project manager used to create, destroy, load and save a project.
    /// </summary>
    internal interface IProjectManager
    {
        #region Properties.
        /// <summary>
        /// Property to return the provider service for handling reading and writing project files.
        /// </summary>
        IFileSystemProviders Providers
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a project object.
        /// </summary>
        /// <param name="workspace">The directory used as a work space location.</param>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>A new project.</returns>
        IProject CreateProject(DirectoryInfo workspace, string projectName);

        /// <summary>
        /// Function to open a project from a file on the disk.
        /// </summary>
        /// <param name="path">The path to the project file.</param>
        /// <param name="providers">The providers used to read the project file.</param>
        /// <param name="workspace">The workspace directory that will receive the files from the project file.</param>
        /// <param name="metadataManager">The metadata manager to use for accessing project metadata.</param>
        /// <returns>A new project based on the file that was read.</returns>
        Task<(IProject project, bool hasMetadata, bool isUpgraded)> OpenProjectAsync(string path, DirectoryInfo workspace);

        /// <summary>
        /// Function to save a project to a file on the disk.
        /// </summary>
        /// <param name="project">The project to save.</param>
        /// <param name="path">The path to the project file.</param>
        /// <param name="writer">The writer plug in used to write the file data.</param>
        /// <param name="progressCallback">The callback method that reports the saving progress to the UI.</param>
        /// <param name="cancelToken">The token used for cancellation of the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        Task SaveProjectAsync(IProject project, string path, FileWriterPlugin writer, Action<int, int, bool> progressCallback, CancellationToken cancelToken);

        /// <summary>
        /// Function to close the project and clean up its working data.
        /// </summary>
        /// <param name="project">The project to close.</param>        
        void CloseProject(IProject project);

        /// <summary>
        /// Function to purge old workspace directories if they were left over (e.g. debug break, crash, etc...)
        /// </summary>
        /// <param name="prevDirectory">The previously used directory path for the project.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="prevDirectory"/> parameter is <b>null</b>.</exception>
        void PurgeStaleDirectories(DirectoryInfo prevDirectory);
        #endregion
    }
}