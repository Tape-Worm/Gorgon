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
// Created: December 23, 2018 1:59:47 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Parameters for the <see cref="IContentPreview"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ContentPreviewParameters"/> class.</remarks>
/// <param name="hostServices">The services from the host application.</param>
/// <param name="viewModelFactory">The view model factory for creating view models.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class ContentPreviewParameters(IHostContentServices hostServices, ViewModelFactory viewModelFactory)
        : ViewModelCommonParameters(hostServices, viewModelFactory)
{
    #region Properties.
    /// <summary>
    /// Property to set or return the file explorer view model.
    /// </summary>
    public IFileExplorer FileExplorer
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the file manager for content.
    /// </summary>
    public IContentFileManager ContentFileManager
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the file system to the temporary area in the project.
    /// </summary>
    public IGorgonFileSystemWriter<Stream> TempFileSystem
    {
        get;
        set;
    }

    #endregion
}
