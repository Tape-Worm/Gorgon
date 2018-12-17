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
// Created: August 29, 2018 8:16:04 PM
// 
#endregion

using System.Collections.Generic;
using System.IO;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Newtonsoft.Json;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// The project data used by the editor.
    /// </summary>
    public interface IProject
    {
        #region Properties.
        /// <summary>
        /// Property to return the version for the project file.
        /// </summary>
        string Version
        {
            get;
        }

        /// <summary>
        /// Property to return the workspace used by the project.
        /// </summary>
        /// <remarks>
        /// A project work space is a folder on the local file system that contains a copy of the project content. This folder is transitory, and will be cleaned up upon application exit.
        /// </remarks>
        [JsonIgnore]
        DirectoryInfo ProjectWorkSpace
        {
            get;
        }

        /// <summary>
        /// Property to return the scratch space directory for the project.
        /// </summary>
        /// <remarks>
        /// The scratch space directory is a temporary directory used for plug ins to store transitory data that only needs to exist during the lifetime of the application or plug in. Nothing in this 
        /// directory is saved into the project.
        /// </remarks>
        [JsonIgnore]
        DirectoryInfo ProjectScratchSpace
        {
            get;
        }

        /// <summary>
        /// Property to set or return the writer used to write to the project file.
        /// </summary>
        [JsonIgnore]
        FileWriterPlugin Writer
        {
            get;
        }

        /// <summary>
        /// Property to return the name of the writer plug in used to write the project file.
        /// </summary>
        [JsonProperty]
        string WriterPluginName
        {
            get;
        }

        /// <summary>
        /// Property to return the list of project items.
        /// </summary>
        Dictionary<string, ProjectItemMetadata> ProjectItems
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign a file system writer to the project.
        /// </summary>
        /// <param name="plugin">The plug in used to write the project file.</param>
        void AssignWriter(FileWriterPlugin plugin);
        #endregion
    }
}
