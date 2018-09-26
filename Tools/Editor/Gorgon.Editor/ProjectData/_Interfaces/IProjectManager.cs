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
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// A project manager used to create, destroy, load and save a project.
    /// </summary>
    internal interface IProjectManager
    {
        /// <summary>
        /// Function to indicate that the selected directory already contains a project.
        /// </summary>
        /// <param name="path">The path to evaluate.</param>
        /// <returns>A flag to indicate whether a project is located in the directory, and the name of the project if there's one present.</returns>
        /// <remarks>
        /// <para>
        /// If this method returns <b>false</b>, but the <c>projectName</c> return value is populated, then an exception was thrown during the check. In this case, the <c>projectName</c> value should
        /// contain the exception error message.
        /// </para>
        /// </remarks>
        (bool hasProject, string projectName) HasProject(DirectoryInfo path);

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
        /// <returns>A new project.</returns>
        Task<IProject> OpenProjectAsync(string path, IFileSystemProviders providers);

        /// <summary>
        /// Function to close the project and clean up its working data.
        /// </summary>
        /// <param name="project">The project to close.</param>        
        void CloseProject(IProject project);
    }
}