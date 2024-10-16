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
// Created: October 29, 2018 1:00:20 PM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;
using Gorgon.UI;
using Microsoft.IO;

namespace Gorgon.Editor.PlugIns;

/// <summary>
/// Defines a plug-in used to generate content in the editor
/// </summary>    
/// <remarks>
/// <para>
/// A content editor plug-in is used to create editors that will be used create or update content within the host application. Custom content editors can be used to create/update any type of content 
/// that the user desires. For example, a tile map editor, shader editor, etc... 
/// </para>
/// <para>
/// The content editor plug-in provides an editor object that the host application integrates into its UI. And that editor provided must implement the <see cref="IEditorContent"/> interface. The 
/// host application will pass along the required objects to manipulate the file system, access the graphics interface, and other sets of functionality
/// </para>
/// </remarks>
/// <seealso cref="IEditorContent"/>
/// <remarks>Initializes a new instance of the <see cref="ContentPlugIn"/> class.</remarks>
/// <param name="description">Optional description of the plugin.</param>
public abstract class ContentPlugIn(string description)
        : EditorPlugIn(description)
{

    // Flag to indicate that the plugin is initialized.
    private int _initialized;

    /// <summary>
    /// Property to return the default file extension used by files generated by this content plug-in.
    /// </summary>
    /// <remarks>
    /// Plug in developers can override this to default the file name extension for their content when creating new content with <see cref="GetDefaultContentAsync(string, HashSet{string})"/>.
    /// </remarks>
    protected virtual GorgonFileExtension DefaultFileExtension
    {
        get;
    }

    /// <summary>
    /// Property to return the services from the host application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plug in developers that implement a common plug-in type based on this base type, should assign this value to allow access to the common content services supplied by the host application.
    /// </para>
    /// <para>
    /// This will be assigned during the initialization of the plug-in.
    /// </para>
    /// </remarks>
    /// <seealso cref="IHostServices"/>
    protected IHostContentServices HostContentServices
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the file system used to hold temporary file data.
    /// </summary>
    /// <remarks>
    /// Plug ins can use this to write temporary working data, which is deleted after the project unloads.
    /// </remarks>
    protected IGorgonFileSystemWriter<Stream> TemporaryFileSystem
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the manager used to manage files in the project file system.
    /// </summary>
    /// <remarks>
    /// Plug ins can use this to create their own directories and files within the confines of project file system.
    /// </remarks>
    protected IContentFileManager ContentFileManager
    {
        get;
        private set;
    }

    /// <summary>Property to return the type of this plug-in.</summary>
    /// <remarks>The <see cref="PlugIns.PlugInType"/> returned for this property indicates the general plug-in functionality.</remarks>
    /// <seealso cref="PlugIns.PlugInType" />
    public sealed override PlugInType PlugInType => PlugInType.Content;

    /// <summary>
    /// Property to return whether or not the plugin is capable of creating content.
    /// </summary>
    /// <remarks>
    /// When plugin authors return <b>true</b> for this property, they should also override the <see cref="GetDefaultContentAsync(string, HashSet{string})"/> method to pre-populate the content with default data.
    /// </remarks>
    public abstract bool CanCreateContent
    {
        get;
    }

    /// <summary>
    /// Property to return the ID for the type of content produced by this plug-in.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The content type ID is defined by the plug-in author and is used to match up content with the plug-in. This value should be as unique as possible.
    /// </para>
    /// <para>
    /// If the plug-in creates an editor that is used to create some kind of common content type like an Image, or Sprite, the user can elect to use the <see cref="CommonEditorContentTypes"/>. 
    /// If the plug-in author uses this list of types to return the content type ID, then they should disable the content plug-ins that come with Gorgon and have the same ID.
    /// </para>
    /// </remarks>
    public abstract string ContentTypeID
    {
        get;
    }

    /// <summary>
    /// Function to provide initialization for the plugin.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is only called when the plugin is loaded at startup.
    /// </para>
    /// </remarks>
    protected virtual void OnInitialize()
    {
    }

    /// <summary>
    /// Function to provide clean up for the plugin.
    /// </summary>
    protected virtual void OnShutdown()
    {

    }

    /// <summary>
    /// Function to open a content object from this plugin.
    /// </summary>
    /// <param name="file">The file that contains the content.</param>
    /// <param name="fileManager">The file manager used to access other content files.</param>
    /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
    /// <param name="undoService">The undo service for the plug-in.</param>
    /// <returns>A new <see cref="IEditorContent"/> object.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="scratchArea"/> parameter is the file system where temporary files to store transitory information for the plug-in is stored. This file system is destroyed when the 
    /// application or plug-in is shut down, and is not stored with the project.
    /// </para>
    /// </remarks>
    protected abstract Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService);

    /// <summary>
    /// Function to open a content object in place from this plugin.
    /// </summary>
    /// <param name="file">The file that contains the content.</param>
    /// <param name="current">The currently open content.</param>
    /// <param name="undoService">The undo service to use when correcting mistakes.</param>
    /// <returns>A new <see cref="IEditorContent"/> object.</returns>
    protected virtual void OnOpenInPlace(IContentFile file, IEditorContent current, IUndoService undoService)
    {
    }

    /// <summary>
    /// Function to register plug-in specific search keywords with the system search.
    /// </summary>
    /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="searchService">The search service to use for registration.</param>
    protected abstract void OnRegisterSearchKeywords<T>(ISearchService<T> searchService) where T : IGorgonNamedObject;

    /// <summary>
    /// Function to allow custom plug-ins to implement custom actions when a project is created/opened.
    /// </summary>
    protected virtual void OnProjectOpened()
    {

    }

    /// <summary>
    /// Function to allow custom plug-ins to implement custom actions when a project is closed.
    /// </summary>
    protected virtual void OnProjectClosed()
    {

    }

    /// <summary>Function to retrieve the default content name, and data.</summary>
    /// <param name="generatedName">A default name generated by the application.</param>
    /// <param name="metadata">Custom metadata for the content.</param>
    /// <returns>The default content name along with the content data serialized as a byte array. If either the name or data are <b>null</b>, then the user cancelled..</returns>
    /// <remarks>
    /// <para>
    /// Plug in authors may override this method so a custom UI can be presented when creating new content, or return a default set of data and a default name, or whatever they wish. 
    /// </para>
    /// <para>
    /// If an empty string (or whitespace) is returned for the name, then the <paramref name="generatedName"/> will be used.
    /// </para>
    /// </remarks>
    protected virtual Task<(string name, RecyclableMemoryStream data)> OnGetDefaultContentAsync(string generatedName, ProjectItemMetadata metadata) => Task.FromResult<(string, RecyclableMemoryStream)>((generatedName, null));

    /// <summary>
    /// Function to register plug-in specific search keywords with the system search.
    /// </summary>
    /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="searchService">The search service to use for registration.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="searchService"/> parameter is <b>null</b>.</exception>
    public void RegisterSearchKeywords<T>(ISearchService<T> searchService)
        where T : IGorgonNamedObject
    {
        if (searchService is null)
        {
            throw new ArgumentNullException(nameof(searchService));
        }

        OnRegisterSearchKeywords(searchService);
    }

    /// <summary>Function to retrieve the default content name, and data.</summary>
    /// <param name="generatedName">A default name generated by the application.</param>
    /// <param name="existingNames">The existing content file names in the same location as the new content file.</param>
    /// <returns>The content name along with the content data serialized as a byte array, and the metadata for the content file. If either the name or data are <b>null</b>, then the user cancelled the operation.</returns>
    /// <remarks>
    /// <para>
    /// If an empty string (or whitespace) is returned for the name, then the <paramref name="generatedName"/> will be used.
    /// </para>
    /// </remarks>
    public async Task<(string name, RecyclableMemoryStream data, ProjectItemMetadata metadata)> GetDefaultContentAsync(string generatedName, HashSet<string> existingNames)
    {
        // First try to ensure the generated name is available.
        int count = 0;
        string defaultExtension = (string.IsNullOrWhiteSpace(generatedName)) || (string.IsNullOrWhiteSpace(DefaultFileExtension.Extension)) ? string.Empty : ("." + DefaultFileExtension.Extension);

        if (!string.IsNullOrWhiteSpace(generatedName))
        {
            string fileName = (generatedName?.FormatFileName() ?? string.Empty) + defaultExtension;

            while (existingNames.Contains(fileName))
            {
                fileName = $"{generatedName.FormatFileName()} ({++count})" + defaultExtension;
            }

            generatedName = fileName;
        }

        do
        {
            ProjectItemMetadata metadata = new()
            {
                ContentMetadata = this as IContentPlugInMetadata,
                PlugInName = Name,
                Thumbnail = null
            };

            (string name, RecyclableMemoryStream data) = await OnGetDefaultContentAsync(generatedName, metadata);

            if ((string.IsNullOrEmpty(name)) && (!string.IsNullOrWhiteSpace(generatedName)))
            {
                name = generatedName;
            }

            name = name.FormatFileName();

            // If at this point, we don't have a name, then cancel out.
            if (string.IsNullOrWhiteSpace(name))
            {
                return (null, null, null);
            }

            if (existingNames.Contains(name))
            {
                GorgonDialogs.ErrorBox(GorgonApplication.MainForm, string.Format(Resources.GOREDIT_ERR_CONTENT_ALREADY_EXISTS, name));
                continue;
            }

            return (name, data, metadata);
        }
        while (true);
    }

    /// <summary>
    /// Function to determine if a file can be loaded in-place.
    /// </summary>
    /// <param name="file">The file to evaluate.</param>
    /// <param name="currentContent">The currently loaded content.</param>
    /// <returns><b>true</b> if it can be opened in-place, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Developers can override this method to implement the correct checking for content information for their plug-ins.
    /// </para>
    /// </remarks>
    protected virtual bool OnCanOpenInPlace(IContentFile file, IEditorContent currentContent) => false;

    /// <summary>
    /// Function to determine if a file can be opened in place, instead of closing and reopening the content document.
    /// </summary>
    /// <param name="file">The file containing the content to evaluate.</param>
    /// <param name="currentContent">The currently loaded content.</param>
    /// <returns><b>true</b> if it can be opened in-place, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
    public bool CanOpenInPlace(IContentFile file, IEditorContent currentContent)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (currentContent is null)
        {
            return false;
        }

        return OnCanOpenInPlace(file, currentContent);
    }

    /// <summary>
    /// Function to open the file in-place.
    /// </summary>
    /// <param name="file">The file that contains the content.</param>
    /// <param name="current">The current content.</param>
    /// <param name="undoService">The undo service to use when correcting mistakes.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/>, or the <paramref name="current"/> parameter is <b>null</b>.</exception>
    public void OpenInPlace(IContentFile file, IEditorContent current, IUndoService undoService)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (current is null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        // Ensure the temp directory contents are up to date.
        TemporaryFileSystem.FileSystem.Refresh();

        current.File.IsOpen = false;

        OnOpenInPlace(file, current, undoService);

        current.File.IsOpen = true;

        // Reset the content state.
        current.ContentState = ContentState.Unmodified;
    }

    /// <summary>
    /// Function to open a content object from this plugin.
    /// </summary>        
    /// <param name="file">The file that contains the content.</param>
    /// <param name="fileManager">The file manager used to access other content files.</param>
    /// <param name="project">The project information.</param>
    /// <param name="undoService">The undo service for the plugin.</param>
    /// <returns>A new <see cref="IEditorContent"/> object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/>, <paramref name="fileManager"/>, or the <paramref name="project"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <see cref="OnOpenContentAsync"/> method returns <b>null</b>.</exception>
    public async Task<IEditorContent> OpenContentAsync(IContentFile file, IContentFileManager fileManager, IProject project, IUndoService undoService)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (fileManager is null)
        {
            throw new ArgumentNullException(nameof(fileManager));
        }

        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        // Ensure the temp directory contents are up to date.
        TemporaryFileSystem.FileSystem.Refresh();

        IEditorContent content = await OnOpenContentAsync(file, fileManager, TemporaryFileSystem, undoService) ?? throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NO_CONTENT_FROM_PLUGIN, Name));

        // Reset the content state.
        content.ContentState = ContentState.Unmodified;

        return content;
    }

    /// <summary>
    /// Function to perform any required clean up for the plugin.
    /// </summary>
    public void Shutdown()
    {
        if (Interlocked.Exchange(ref _initialized, 0) == 0)
        {
            return;
        }

        OnShutdown();
        ProjectClosed();
    }

    /// <summary>
    /// Function called when a project is loaded/created.
    /// </summary>
    /// <param name="fileManager">The file manager for the project.</param>
    /// <param name="tempFileSystem">The file system used to hold temporary working data.</param>
    public void ProjectOpened(IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> tempFileSystem)
    {
        TemporaryFileSystem = tempFileSystem;
        ContentFileManager = fileManager;
        OnProjectOpened();
    }

    /// <summary>
    /// Function called when a project is unloaded.
    /// </summary>
    public void ProjectClosed()
    {
        OnProjectClosed();
        ContentFileManager = null;
        TemporaryFileSystem = null;
        HostServices = null;
    }

    /// <summary>
    /// Function to perform any required initialization for the plugin.
    /// </summary>
    /// <param name="hostServices">The services passed from the host application to the content plug-in.</param>                
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="hostServices"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This method is only called when the plugin is loaded at startup.
    /// </para>
    /// </remarks>
    public void Initialize(IHostContentServices hostServices)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return;
        }

        HostServices = HostContentServices = hostServices ?? throw new ArgumentNullException(nameof(hostServices));
        HostContentServices.Log.Print($"Initializing {Name}...", LoggingLevel.Simple);

        OnInitialize();
    }
}
