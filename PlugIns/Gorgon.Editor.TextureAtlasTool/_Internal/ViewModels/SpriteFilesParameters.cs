
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
// Created: May 7, 2019 12:12:47 AM
// 

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.Controls;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.IO;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The parameters for the <see cref="ISpriteFiles"/> view model
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SpriteFilesParameters"/> class.</remarks>
/// <param name="entries">The file system sprite entries.</param>
/// <param name="tempFileSystem">The temporary file system used to write temporary data.</param>
/// <param name="searchService">The search service used to search through the sprite entries.</param>        
/// <param name="hostServices">The services from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class SpriteFilesParameters(IReadOnlyList<ContentFileExplorerDirectoryEntry> entries, IGorgonFileSystemWriter<Stream> tempFileSystem, ISearchService<IContentFileExplorerSearchEntry> searchService, IHostContentServices hostServices)
        : ViewModelInjection<IHostContentServices>(hostServices)
{
    /// <summary>
    /// Property to reeturn the service to search through the content files.
    /// </summary>
    public ISearchService<IContentFileExplorerSearchEntry> SearchService
    {
        get;
    } = searchService ?? throw new ArgumentNullException(nameof(searchService));

    /// <summary>
    /// Property to return the entries for the file system.
    /// </summary>
    public IReadOnlyList<ContentFileExplorerDirectoryEntry> Entries
    {
        get;
    } = entries ?? throw new ArgumentNullException(nameof(entries));

    /// <summary>
    /// Property to return the temporary file system used to write data.
    /// </summary>
    public IGorgonFileSystemWriter<Stream> TempFileSystem
    {
        get;
    } = tempFileSystem ?? throw new ArgumentNullException(nameof(tempFileSystem));
}
