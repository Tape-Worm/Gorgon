
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
// Created: September 4, 2018 11:11:06 AM
// 


using System.ComponentModel;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The view model for interacting with the project data
/// </summary>
internal interface IProjectEditor
    : IViewModel
{

    /// <summary>
    /// Property to return the available tool plug in button definitions for the application.
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> ToolButtons
    {
        get;
    }

    /// <summary>
    /// Property to return the title for the project.
    /// </summary>
    string ProjectTitle
    {
        get;
    }

    /// <summary>
    /// Property to return the content file manager for managing content file systems through content plug ins.
    /// </summary>
    IContentFileManager ContentFileManager
    {
        get;
    }

    /// <summary>
    /// Property to return the file explorer view model for use with the file explorer subview.
    /// </summary>
    IFileExplorer FileExplorer
    {
        get;
    }

    /// <summary>
    /// Property to return the content previewer.
    /// </summary>
    IContentPreview ContentPreviewer
    {
        get;
    }

    /// <summary>
    /// Property to return the current command context.
    /// </summary>
    string CommandContext
    {
        get;
    }

    /// <summary>
    /// Property to return the current content file for the project.
    /// </summary>
    IEditorContent CurrentContent
    {
        get;
    }

    /// <summary>
    /// Property to set or return the current clipboard context depending on content.
    /// </summary>
    IClipboardHandler ClipboardContext
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command to execute when the project is closing.
    /// </summary>
    IEditorAsyncCommand<CancelEventArgs> BeforeCloseCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to execute after the project is closed.
    /// </summary>
    IEditorCommand<object> AfterCloseCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to save the project metadata.
    /// </summary>
    IEditorCommand<object> SaveProjectMetadataCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to save the project to a packed file.
    /// </summary>
    IEditorAsyncCommand<CancelEventArgs> SaveProjectToPackFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to create content.
    /// </summary>
    IEditorAsyncCommand<Guid> CreateContentCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command executed when the currently active content is closed.
    /// </summary>
    IEditorCommand<object> ContentClosedCommand
    {
        get;
    }

}
