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
// Created: August 26, 2018 9:34:44 PM
// 

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The view model for the main window
/// </summary>
internal interface IMain
    : IViewModel
{
    /// <summary>
    /// Property to return the settings for the application.
    /// </summary>
    Editor.EditorSettings Settings
    {
        get;
    }

    /// <summary>
    /// Property to return a list of content plugins that can create their own content.
    /// </summary>
    IReadOnlyList<IContentPlugInMetadata> ContentCreators
    {
        get;
    }

    /// <summary>
    /// Property to return the current clipboard context.
    /// </summary>
    IClipboardHandler ClipboardContext
    {
        get;
    }

    /// <summary>
    /// Property to return the view model for the new project child view.
    /// </summary>
    INewProject NewProject
    {
        get;
    }

    /// <summary>
    /// Property to return the recent files view model for the recent files child view.
    /// </summary>
    IRecent RecentFiles
    {
        get;
    }

    /// <summary>
    /// Property to return the view model for the settings view.
    /// </summary>
    IEditorSettings SettingsViewModel
    {
        get;
    }

    /// <summary>
    /// Property to return the view model for the current project.
    /// </summary>
    IProjectEditor CurrentProject
    {
        get;
    }

    /// <summary>
    /// Property to return the text for the caption.
    /// </summary>
    string Text
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to open a project.
    /// </summary>
    IEditorCommand<object> BrowseProjectCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to open a packed file.
    /// </summary>
    IEditorCommand<object> OpenPackFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to query before closing the application.
    /// </summary>
    IEditorAsyncCommand<AppCloseArgs> AppClosingAsyncCommand
    {
        get;
    }
}
