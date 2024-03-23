
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 27, 2019 10:49:22 PM
// 

using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Newtonsoft.Json;

namespace Gorgon.Editor.Support;

/// <summary>
/// The project data for an editor project
/// </summary>        
internal class EditorProjectMetadata31
    : IProjectMetadata
{
    /// <summary>Property to return the version for the project file.</summary>                
    [JsonProperty]
    public string Version
    {
        get;
        private set;
    } = CommonEditorConstants.EditorCurrentProjectVersion;

    /// <summary>Property to return the list of project items.</summary>        
    [JsonProperty]
    public Dictionary<string, ProjectItemMetadata> ProjectItems
    {
        get;
    } = new Dictionary<string, ProjectItemMetadata>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initializes a new instance of the <see cref="EditorProjectMetadata31"/> class.</summary>
    [JsonConstructor]
    public EditorProjectMetadata31()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="EditorProjectMetadata31"/> class.</summary>
    /// <param name="oldVersion">The old version of the project data.</param>
    public EditorProjectMetadata31(EditorProjectMetadata30 oldVersion)
    {
        foreach (KeyValuePair<string, ProjectItemMetadata30> item in oldVersion.ProjectItems)
        {
            ProjectItemMetadata newItem = new(item.Value);
            ProjectItems[item.Key] = newItem;
        }
    }
}
