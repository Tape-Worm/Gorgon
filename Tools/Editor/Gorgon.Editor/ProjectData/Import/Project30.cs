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
using Gorgon.Editor.Metadata;
using Newtonsoft.Json;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// The project data.
    /// </summary>
    internal class Project30
    {
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
        /// Property to return the list of project items.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, Project30ItemMetadata> ProjectItems
        {
            get;
            private set;
        } = new Dictionary<string, Project30ItemMetadata>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Constructor/Finalizer.        
        /// <summary>Initializes a new instance of the Project class.</summary>
        [JsonConstructor]
        public Project30()
        {
            // Used by JSON.Net for deserialization.
        }
        #endregion
    }
}
