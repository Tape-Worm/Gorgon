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
/// Event arguments for the <see cref="IGorgonFileSystemWriter{T}.VirtualFileOpened"/> event
/// </summary>
public class VirtualFileOpenedEventArgs
    : EventArgs
{
    /// <summary>
    /// Property to return the virtual file that is being opened.
    /// </summary>
    /// <remarks>
    /// If this value is <b>null</b>, then the file system does not have an entry for it. This can happen when a file is being created by the file system writer.
    /// </remarks>
    public IGorgonVirtualFile? VirtualFile
    {
        get;
    }

    /// <summary>
    /// Property to return the stream for the file.
    /// </summary>
    public Stream Stream
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="VirtualFileOpenedEventArgs"/> class.</summary>
    /// <param name="file">The virtual file that was opened.</param>
    /// <param name="stream">The stream for the opened file.</param>
    internal VirtualFileOpenedEventArgs(IGorgonVirtualFile? file, Stream stream)
    {
        VirtualFile = file;
        Stream = stream;
    }
}
