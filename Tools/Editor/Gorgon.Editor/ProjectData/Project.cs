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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gorgon.Core;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Newtonsoft.Json;

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
        /// Property to return the version for the project file.
        /// </summary>
        [JsonProperty]
        public string Version
        {
            get;
            private set;
        } = CommonEditorConstants.EditorCurrentProjectVersion;

        /// <summary>
        /// Property to set or return the workspace used by the project.
        /// </summary>
        /// <remarks>
        /// A project workspace is a folder on the local file system that contains a copy of the project content. This folder is transitory, and will be cleaned up upon application exit.
        /// </remarks>
        [JsonIgnore]
        public DirectoryInfo ProjectWorkSpace
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the scratch space directory for the project.
        /// </summary>
        /// <remarks>
        /// The scratch space directory is a temporary directory used for plug ins to store transitory data that only needs to exist during the lifetime of the application or plug in. Nothing in this 
        /// directory is saved into the project.
        /// </remarks>
        [JsonIgnore]
        public DirectoryInfo ProjectScratchSpace
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether file system objects like directories and files should be shown in the project even when they're not included.
        /// </summary>
        public bool ShowExternalItems
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the writer used to write to the project file.
        /// </summary>
        [JsonIgnore]
        public FileWriterPlugin Writer
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the name of the plugin used to write the project file.
        /// </summary>
        [JsonProperty]
        public string WriterPluginName
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the list of project items.
        /// </summary>
        public Dictionary<string, ProjectItemMetadata> ProjectItems
        {
            get;
            private set;
        } = new Dictionary<string, ProjectItemMetadata>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign a file system writer to the project.
        /// </summary>
        /// <param name="plugin">The plug in used to write the project file.</param>
        public void AssignWriter(FileWriterPlugin plugin)
        {
            Writer = plugin;
            WriterPluginName = plugin?.Name;
        }
        #endregion

        #region Constructor/Finalizer.        
        /// <summary>Initializes a new instance of the Project class.</summary>
        [JsonConstructor]
        public Project()
        {
            // Used by JSON.Net for deserialization.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="workspace">The work space directory for the project.</param>
        /// <param name="scratchDir">The temporary scratch directory for the project.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/>, or the <paramref name="scratchDir"/> parameter is <b>null</b>.</exception>        
        public Project(DirectoryInfo workspace, DirectoryInfo scratchDir)
        {
            ProjectWorkSpace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            ProjectScratchSpace = scratchDir ?? throw new ArgumentNullException(nameof(scratchDir));
        }
        #endregion
    }
}
