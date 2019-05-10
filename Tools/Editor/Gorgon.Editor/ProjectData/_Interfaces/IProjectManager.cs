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
using Gorgon.Editor.PlugIns;
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
        /// Function to determine if the project workspace directory is locked.
        /// </summary>
        /// <param name="workspace">The directory to use as a project workspace.</param>
        /// <returns><b>true</b> if the project is locked, <b>false</b> if not.</returns>
        bool IsDirectoryLocked(DirectoryInfo workspace);

        /// <summary>
        /// Function to determine if a directory is a Gorgon project directory or not.
        /// </summary>
        /// <param name="directory">The directory to examine.</param>
        /// <returns><b>true</b> if the directory is a Gorgon directory, <b>false</b> if it is not.</returns>
        bool IsGorgonProject(DirectoryInfo directory);

        /// <summary>
        /// Function to create a project object.
        /// </summary>
        /// <param name="workspace">The directory used as a work space location.</param>
        /// <returns>A new project.</returns>
        IProject CreateProject(DirectoryInfo workspace);

        /// <summary>
        /// Function to open a project workspace directory.
        /// </summary>
        /// <param name="projectWorkspace">The directory pointing at the project workspace.</param>
        /// <returns>The project information for the project data in the work space.</returns>
        IProject OpenProject(DirectoryInfo projectWorkspace);

        /// <summary>
        /// Function to open a project from a packed file on the disk.
        /// </summary>
        /// <param name="path">The path to the project file.</param>
        /// <param name="workspace">The workspace directory that will receive the files from the project file.</param>
        /// <returns>A new project based on the file that was read.</returns>
        Task OpenPackFileProjectAsync(FileInfo path, DirectoryInfo workspace);

        /// <summary>
        /// Function to save a project to a packed file on the disk.
        /// </summary>
        /// <param name="project">The project to save.</param>
        /// <param name="path">The path to the project file.</param>
        /// <param name="writer">The writer plug in used to write the file data.</param>
        /// <param name="progressCallback">The callback method that reports the saving progress to the UI.</param>
        /// <param name="cancelToken">The token used for cancellation of the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        Task SavePackedFileAsync(IProject project, FileInfo path, FileWriterPlugIn writer, Action<int, int, bool> progressCallback, CancellationToken cancelToken);

        /// <summary>
        /// Function to persist out the metadata for the project.
        /// </summary>
        /// <param name="project">The project to write out.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        void PersistMetadata(IProject project, CancellationToken cancelToken);

        /// <summary>
        /// Function to close the project and clean up its working data.
        /// </summary>
        /// <param name="project">The project to close.</param>        
        void CloseProject(IProject project);

        /// <summary>
        /// Function used to lock the project to this instance of the application only.
        /// </summary>
        /// <param name="project">The project to lock.</param>
        /// <remarks>
        /// <para>
        /// This method locks down the project working directory so that only this instance of the editor may use it. All other instances will exception if an attempt to open the directory is made.
        /// </para>
        /// </remarks>
        void LockProject(IProject project);
        #endregion
    }
}