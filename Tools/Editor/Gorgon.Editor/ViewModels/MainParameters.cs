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
// Created: September 17, 2018 8:07:42 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Parameters used for injection on the <see cref="Main"/> view model.
/// </summary>
internal class MainParameters
    : ViewModelCommonParameters
{
    /// <summary>
    /// Property to set or return the new project view model.
    /// </summary>
    public INewProject NewProject
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the recent files view model.
    /// </summary>
    public IRecent RecentFiles
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the settings view model.
    /// </summary>
    public IEditorSettings Settings
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the dialog for opening projects.
    /// </summary>
    public EditorFileOpenDialogService OpenDialog
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the undo service for the application.
    /// </summary>
    public IUndoService UndoService
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return a list of content plug ins that can create their own content.
    /// </summary>
    public IReadOnlyList<IContentPlugInMetadata> ContentCreators
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the service used to browse directories on the physical file system.
    /// </summary>
    public IDirectoryLocateService DirectoryLocater
    {
        get;
        set;
    }

    /// <summary>Initializes a new instance of the MainParameters class.</summary>
    /// <param name="hostServices">The services from the host application.</param>
    /// <param name="viewModelFactory">The view model factory for creating view models.</param>
    public MainParameters(IHostContentServices hostServices, ViewModelFactory viewModelFactory)
        : base(hostServices, viewModelFactory)
    {            
    }
}
