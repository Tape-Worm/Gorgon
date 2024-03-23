
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
/// Event arguments for the <see cref="IGorgonFileSystemWriter{T}.VirtualFileRenamed"/> event
/// </summary>
public class VirtualFileRenamedEventArgs
    : EventArgs
{
    /// <summary>
    /// Property to return the virtual directory that represents the physical directory.
    /// </summary>
    public IGorgonVirtualFile VirtualFile
    {
        get;
    }

    /// <summary>
    /// Property to return the new name for the directory.
    /// </summary>
    public string NewName => VirtualFile.Name;

    /// <summary>
    /// Property to return the old name for the directory.
    /// </summary>
    public string OldName
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="VirtualFileRenamedEventArgs"/> class.</summary>
    /// <param name="directory">The virtual directory that was renamed.</param>
    /// <param name="oldName">The original name for the directory.</param>
    internal VirtualFileRenamedEventArgs(IGorgonVirtualFile directory, string oldName)
    {
        VirtualFile = directory;
        OldName = oldName;
    }
}
