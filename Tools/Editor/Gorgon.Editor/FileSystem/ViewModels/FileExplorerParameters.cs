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
// Created: September 17, 2018 8:53:59 AM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Parameters for the <see cref="IFileExplorer"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FileExplorerParameters"/> class.</remarks>
/// <param name="hostServices">The services from the host application.</param>
/// <param name="viewModelFactory">The view model factory.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class FileExplorerParameters(IHostContentServices hostServices, ViewModelFactory viewModelFactory)
        : ViewModelCommonParameters(hostServices, viewModelFactory)
{
    /// <summary>
    /// Property to set or return the root directory for the file system.
    /// </summary>
    public IDirectory Root
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the project file system.
    /// </summary>
    public IGorgonFileSystemWriter<FileStream> FileSystem
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the service used to search the file system.
    /// </summary>
    public ISearchService<IFile> SearchService
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the directory locator service.
    /// </summary>
    public IDirectoryLocateService DirectoryLocator
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the synchronization context for the main thread.
    /// </summary>
    public SynchronizationContext SyncContext
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the clipboard handler.
    /// </summary>
    public IClipboardHandler Clipboard
    {
        get;
        set;
    }
}
