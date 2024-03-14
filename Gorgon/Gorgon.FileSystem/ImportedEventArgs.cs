#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: January 2, 2020 1:14:24 PM
// 
#endregion

namespace Gorgon.IO;

/// <summary>
/// Event arguments for the <see cref="IGorgonFileSystemWriter{T}.Imported"/> event.
/// </summary>
public class ImportedEventArgs
    : EventArgs
{
    /// <summary>
    /// Property to return the virtual directories that were imported.
    /// </summary>
    public IReadOnlyList<IGorgonVirtualDirectory> VirtualDirectories
    {
        get;
    }

    /// <summary>
    /// Property to return the virtual files that were imported.
    /// </summary>
    public IReadOnlyList<IGorgonVirtualFile> VirtualFiles
    {
        get;
    }

    /// <summary>
    /// Property to return the destination directory for the import operation.
    /// </summary>
    public IGorgonVirtualDirectory Destination
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="ImportedEventArgs"/> class.</summary>
    /// <param name="dest">The destination directory for the import.</param>
    /// <param name="directories">The list of virtual directories that were imported.</param>
    /// <param name="files">The list of virtual files that were imported.</param>
    internal ImportedEventArgs(IGorgonVirtualDirectory dest,
                                            IReadOnlyList<IGorgonVirtualDirectory> directories,
                                            IReadOnlyList<IGorgonVirtualFile> files)
    {
        Destination = dest;
        VirtualDirectories = directories;
        VirtualFiles = files;
    }
}
