
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 29, 2018 8:19:12 PM
// 


using Gorgon.Editor.Metadata;
using Newtonsoft.Json;

namespace Gorgon.Editor.ProjectData;

/// <summary>
/// The project data
/// </summary>
internal class Project
    : IProject
{





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
    /// Property to return the workspace used by the project.
    /// </summary>
    /// <remarks>
    /// A project work space is a folder on the local file system that contains a copy of the project content. This is the location of data required for the project, but should not be part of the file 
    /// system.
    /// </remarks>
    [JsonIgnore]
    public DirectoryInfo ProjectWorkSpace
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the temporary directory for the project.
    /// </summary>
    /// <remarks>
    /// The temporary directory is for plug ins and the editor to store transitory data that only needs to exist during the lifetime of the application or plug in. Nothing in this directory is saved 
    /// into the project.
    /// </remarks>
    [JsonIgnore]
    public DirectoryInfo TempDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the directory for original files that may have been converted during the import process.
    /// </summary>
    [JsonIgnore]
    public DirectoryInfo SourceDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the directory that houses the file system for the project files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This directory contains all the files and directories imported by the user into the project.  
    /// </para>
    /// <para>
    /// Files/directories added, or deleted from this via the operating system (e.g. windows explorer), are not tracked and may cause issues. Do not manipulate the data in this directory outside of the 
    /// application.
    /// </para>
    /// </remarks>
    [JsonIgnore]
    public DirectoryInfo FileSystemDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the list of project items.
    /// </summary>
    public Dictionary<string, ProjectItemMetadata> ProjectItems
    {
        get;
        private set;
    } = new Dictionary<string, ProjectItemMetadata>(StringComparer.OrdinalIgnoreCase);


        
    /// <summary>Initializes a new instance of the Project class.</summary>
    [JsonConstructor]
    public Project()
    {
        // Used by JSON.Net for deserialization.
    }

    /// <summary>Initializes a new instance of the <see cref="Project"/> class.</summary>
    /// <param name="project">The project to upgrade.</param>        
    public Project(Project30 project)
    {
        foreach (KeyValuePair<string, Project30ItemMetadata> item in project.ProjectItems)
        {
            var newItem = new ProjectItemMetadata
            {
                PlugInName = item.Value.PlugInName
            };

            foreach (KeyValuePair<string, string> attr in item.Value.Attributes)
            {
                newItem.Attributes[attr.Key] = attr.Value;
            }

            foreach (KeyValuePair<string, string> dependency in item.Value.DependsOn)
            {
                if (!newItem.DependsOn.TryGetValue(dependency.Key, out List<string> items))
                {
                    newItem.DependsOn[dependency.Key] = items = [];
                }

                if (!items.Contains(dependency.Value))
                {
                    items.Add(dependency.Value);
                }
            }

            ProjectItems[item.Key] = newItem;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Project"/> class.
    /// </summary>
    /// <param name="workspace">The work space directory for the project.</param>
    /// <param name="tempDir">The temporary scratch directory for the project.</param>
    /// <param name="fileSystemDir">The directory used to store the file/directory information for the project.</param>
    /// <param name="srcDirectory">The directory used to sure the original file(s) for imported and converted file data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/>, <paramref name="tempDir"/>, <paramref name="fileSystemDir"/>, or the <paramref name="srcDirectory"/> parameter is <b>null</b>.</exception>        
    public Project(DirectoryInfo workspace, DirectoryInfo tempDir, DirectoryInfo fileSystemDir, DirectoryInfo srcDirectory)
    {
        ProjectWorkSpace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        TempDirectory = tempDir ?? throw new ArgumentNullException(nameof(tempDir));
        FileSystemDirectory = fileSystemDir ?? throw new ArgumentNullException(nameof(fileSystemDir));
        SourceDirectory = srcDirectory ?? throw new ArgumentNullException(nameof(srcDirectory));
    }

}
