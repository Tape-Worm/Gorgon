﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 2, 2020 1:14:24 PM
// 

namespace Gorgon.IO;

/// <summary>
/// Event arguments for the <see cref="IGorgonFileSystemWriter{T}.VirtualDirectoryMoved"/> and <see cref="IGorgonFileSystemWriter{T}.VirtualDirectoryCopied"/> event
/// </summary>
public class VirtualDirectoryCopiedMovedEventArgs
    : EventArgs
{
    /// <summary>
    /// Property to return the virtual directories that were moved.
    /// </summary>
    public IReadOnlyList<(IGorgonVirtualDirectory src, IGorgonVirtualDirectory dest)> VirtualDirectories
    {
        get;
    }

    /// <summary>
    /// Property to return the virtual files that were moved.
    /// </summary>
    public IReadOnlyList<(IGorgonVirtualFile src, IGorgonVirtualFile dest)> VirtualFiles
    {
        get;
    }

    /// <summary>
    /// Property to return the destination directory for the copy operation.
    /// </summary>
    public IGorgonVirtualDirectory Destination
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="VirtualDirectoryCopiedMovedEventArgs"/> class.</summary>
    /// <param name="dest">The destination directory for the copy.</param>
    /// <param name="directories">The list of virtual directories that were moved.</param>
    /// <param name="files">The list of virtual files that were moved.</param>
    internal VirtualDirectoryCopiedMovedEventArgs(IGorgonVirtualDirectory dest,
                                            IReadOnlyList<(IGorgonVirtualDirectory src, IGorgonVirtualDirectory dest)> directories,
                                            IReadOnlyList<(IGorgonVirtualFile src, IGorgonVirtualFile dest)> files)
    {
        Destination = dest;
        VirtualDirectories = directories;
        VirtualFiles = files;
    }
}
