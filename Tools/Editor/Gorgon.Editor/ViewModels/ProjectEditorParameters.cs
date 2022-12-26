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
// Created: September 17, 2018 8:50:22 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Parameters for the <see cref="IProjectEditor"/> view model.
/// </summary>
internal class ProjectEditorParameters
    : ViewModelCommonParameters
{
    /// <summary>
    /// Property to set or return the file explorer view model.
    /// </summary>
    public IFileExplorer FileExplorer
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the file manager for content plug ins.
    /// </summary>
    public IContentFileManager ContentFileManager
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the content previewer for content files.
    /// </summary>
    public IContentPreview ContentPreviewer
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the dialog for saving projects.
    /// </summary>
    public EditorFileSaveDialogService SaveDialog
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the list of content creator plug ins.
    /// </summary>
    public IReadOnlyList<IContentPlugInMetadata> ContentCreators
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectEditorParameters" /> class.
    /// </summary>
    /// <param name="hostServices">The services from the host application.</param>
    /// <param name="viewModelFactory">The view model factory.</param>
    public ProjectEditorParameters(IHostContentServices hostServices, ViewModelFactory viewModelFactory)
        : base(hostServices, viewModelFactory)
    {        
    }
}
