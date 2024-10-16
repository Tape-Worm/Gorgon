﻿
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
// Created: October 29, 2018 1:04:03 PM
// 

using System.Text.Json.Serialization;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.IO;

namespace Gorgon.Editor.Services;

/// <summary>
/// The state for the plugin when associated with an included item
/// </summary>
public enum MetadataPlugInState
{
    /// <summary>
    /// No plugin was ever assigned.
    /// </summary>
    Unassigned,
    /// <summary>
    /// The plugin was assigned.
    /// </summary>
    Assigned,
    /// <summary>
    /// The plugin was not found.
    /// </summary>
    NotFound
}

/// <summary>
/// Provides access to the various content specific plugins in the application
/// </summary>
public interface IContentPlugInService
    : IDisabledPlugInService
{
    /// <summary>
    /// Property to return the list of content plugins loaded in to the application.
    /// </summary>
    IReadOnlyDictionary<string, ContentPlugIn> PlugIns
    {
        get;
    }

    /// <summary>
    /// Property to return the list of content importer plug-ins loaded into the application.
    /// </summary>
    IReadOnlyDictionary<string, ContentImportPlugIn> Importers
    {
        get;
    }

    /// <summary>
    /// Property to set or return the currently active content file manager to pass to any plug-ins.
    /// </summary>
    IContentFileManager ContentFileManager
    {
        get;
        set;
    }

    /// <summary>
    /// Function to retrieve the appropriate content importer for the file specified.
    /// </summary>
    /// <param name="filePath">The path to the file to evaluate.</param>
    /// <returns>A <see cref="IEditorContentImporter"/>, or <b>null</b> if none was found.</returns>
    /// <remarks>
    /// <para>
    /// Since the content importers are meant for importing into the project virtual file system, the <paramref name="filePath"/> must point to a file on the physical file system. 
    /// </para>
    /// </remarks>
    IEditorContentImporter GetContentImporter(string filePath);

    /// <summary>
    /// Function to retrieve the actual plug-in based on the name associated with the project metadata item.
    /// </summary>
    /// <param name="metadata">The metadata item to evaluate.</param>
    /// <returns>The plug-in, and the <see cref="MetadataPlugInState"/> used to evaluate whether a deep inspection is required.</returns>
    (ContentPlugIn plugin, MetadataPlugInState state) GetContentPlugIn(ProjectItemMetadata metadata);

    /// <summary>
    /// Function called when a project is loaded/created.
    /// </summary>
    /// <param name="projectFileSystem">The read only file system used by the project.</param>
    /// <param name="fileManager">The content file manager for the project.</param>
    /// <param name="temporaryFileSystem">The file system used to hold temporary working data.</param>
    void ProjectActivated(IGorgonFileSystem projectFileSystem, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> temporaryFileSystem);

    /// <summary>
    /// Function called when a project is unloaded.
    /// </summary>
    void ProjectDeactivated();

    /// <summary>
    /// Funcion to read the settings for a content plug-in from a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="converters">A list of JSON data converters.</param>
    /// <returns>The settings object for the plug-in, or <b>null</b> if no settings file was found for the plug-in.</returns>
    /// <remarks>
    /// <para>
    /// This will read in the settings for a content plug from the same location where the editor stores its application settings file.
    /// </para>
    /// </remarks>
    T ReadContentSettings<T>(string name, params JsonConverter[] converters) where T : class;

    /// <summary>
    /// Function to write out the settings for a content plug-in as a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="contentSettings">The content settings to persist as JSON file.</param>
    /// <param name="converters">A list of JSON converters.</param>
    /// <remarks>
    /// <para>
    /// This will write out the settings for a content plug-in to the same location where the editor stores its application settings file.
    /// </para>
    /// </remarks>
    void WriteContentSettings<T>(string name, T contentSettings, params JsonConverter[] converters) where T : class;

}
