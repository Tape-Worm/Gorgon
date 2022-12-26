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

using System.IO;
using Newtonsoft.Json;

// TODO:
//Add functionality to add plugin names to the project indicating which plugins were available when the project was created.
//If no names are present - Do a full scan on loading the file system.
//If all plug in names match in count and names - Quick load.
//If plug in names differ, or a plug in is missing, or a new plug in was added - Do a full scan on loading the file system.

namespace Gorgon.Editor.ProjectData;

/// <summary>
/// The project data used by the editor.
/// </summary>
public interface IProject
    : IProjectMetadata
{
    #region Properties.
    /// <summary>
    /// Property to return the workspace used by the project.
    /// </summary>
    /// <remarks>
    /// A project work space is a folder on the local file system that contains a copy of the project content. This is the location of data required for the project, but should not be part of the file 
    /// system.
    /// </remarks>
    [JsonIgnore]
    DirectoryInfo ProjectWorkSpace
    {
        get;
    }

    /// <summary>
    /// Property to return the temporary directory for the project.
    /// </summary>
    /// <remarks>
    /// The temporary directory is for plug ins and the editor to store transitory data that only needs to exist during the lifetime of the application or plug in. Nothing in this directory is saved 
    /// into the project.
    /// </remarks>
    [JsonIgnore]
    DirectoryInfo TempDirectory
    {
        get;
    }

    /// <summary>
    /// Property to return the directory for original files that may have been converted during the import process.
    /// </summary>
    [JsonIgnore]
    DirectoryInfo SourceDirectory
    {
        get;
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
    DirectoryInfo FileSystemDirectory
    {
        get;
    }
    #endregion
}
