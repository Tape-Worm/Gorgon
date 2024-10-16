﻿
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
// Created: April 18, 2019 8:48:54 AM
// 

using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.IO;

namespace Gorgon.Editor.PlugIns;

/// <summary>
/// Base class for a plug-in that provides basic utility functionality
/// </summary>
/// <remarks>
/// <para>
/// Tool plug-ins, unlike content plug-ins, are independent plug-ins that provide their own UI (if necessary) that do various jobs in the editor. They will be placed into a tab on the ribbon 
/// called "Tools" and provide global functionality across the editor and don't necessarily have to restrict themselves to one type of content
/// </para>
/// <para>
/// A typical use of a tool plug-in would be to display a dialog that allows mass actions on content files.  For example, a dialog that sequentially renames a list of content files so that they're 
/// postfixed with "original_name (number)" would be a use case for a tool plug-in
/// </para>
/// <para>
/// Because these plug-ins are independent, there is no interaction with the editor UI beyond providing information to display a button on the ribbon
/// </para>
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="ToolPlugIn"/> class.</remarks>
/// <param name="description">Optional description of the plugin.</param>
public abstract class ToolPlugIn(string description)
        : EditorPlugIn(description)
{

    // Flag to indicate that the plugin is initialized.
    private int _initialized;
    // Tool services passed in from the host application.
    private IHostContentServices _hostToolServices;

    /// <summary>
    /// Property to return the services from the host application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plug in developers that implement a common plug-in type based on this base type, should assign this value to allow access to the common tool services supplied by the host application.
    /// </para>
    /// <para>
    /// This will be assigned during the initialization of the plug-in.
    /// </para>
    /// </remarks>
    /// <seealso cref="IHostServices"/>
    protected IHostContentServices HostToolServices
    {
        get => _hostToolServices;
        private set => HostServices = _hostToolServices = value;
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

    /// <summary>
    /// Property to return the file system used to hold temporary file data.
    /// </summary>
    /// <remarks>
    /// Importer plug-ins can use this to write temporary working data, which is deleted after the project unloads, for use during the import process.
    /// </remarks>
    protected IGorgonFileSystemWriter<Stream> TemporaryFileSystem
    {
        get;
        private set;
    }

    /// <summary>Property to return the type of this plug-in.</summary>
    public sealed override PlugInType PlugInType => PlugInType.Tool;

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
    /// Function to retrieve the ribbon button for the tool.
    /// </summary>
    /// <returns>A new tool ribbon button instance.</returns>
    /// <remarks>
    /// <para>
    /// Tool plug-in developers must override this method to return the button which is inserted on the application ribbon, under the "Tools" tab. If the method returns <b>null</b>, then the tool is 
    /// ignored.
    /// </para>
    /// <para>
    /// The resulting data structure will contain the means to handle the click event for the tool, and as such, is the only means of communication between the main UI and the plug-in.
    /// </para>
    /// </remarks>
    protected abstract IToolPlugInRibbonButton OnGetToolButton();

    /// <summary>
    /// Function called when a project is loaded/created.
    /// </summary>
    /// <param name="fileManager">The file manager for the project file system.</param>
    /// <param name="tempFileSystem">The file system used to hold temporary working data.</param>
    public void ProjectOpened(IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> tempFileSystem)
    {
        ContentFileManager = fileManager;
        TemporaryFileSystem = tempFileSystem;
        OnProjectOpened();
    }

    /// <summary>
    /// Function called when a project is unloaded.
    /// </summary>
    public void ProjectClosed()
    {
        OnProjectClosed();
        TemporaryFileSystem = null;
        ContentFileManager = null;
    }

    /// <summary>
    /// Function to retrieve the ribbon button for the tool.
    /// </summary>
    /// <returns>A new tool ribbon button instance.</returns>
    /// <remarks>
    /// <para>
    /// This will return data to describe a new button for the tool in the plug-in. If the return value is <b>null</b>, then the tool will not be available on the ribbon.
    /// </para>
    /// </remarks>
    public IToolPlugInRibbonButton GetToolButton() => OnGetToolButton();

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

        HostToolServices = null;
    }

    /// <summary>
    /// Function to perform any required initialization for the plugin.
    /// </summary>
    /// <param name="hostServices">The services from the host application.</param>
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

        HostToolServices = hostServices ?? throw new ArgumentNullException(nameof(hostServices));
        HostServices.Log.Print($"Initializing {Name}...", LoggingLevel.Simple);

        OnInitialize();
    }
}
