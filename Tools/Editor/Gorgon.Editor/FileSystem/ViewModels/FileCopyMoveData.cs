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
// Created: January 16, 2020 4:04:14 PM
// 

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// A data structure used to copy files
/// </summary>
internal class FileCopyMoveData
    : IFileCopyMoveData
{
    /// <summary>Gets or sets the destination directory.</summary>
    public string DestinationDirectory
    {
        get;
        set;
    }

    /// <summary>Gets or sets the operation.</summary>
    public CopyMoveOperation Operation
    {
        get;
        set;
    }

    /// <summary>Gets or sets the source files.</summary>
    public IReadOnlyList<string> SourceFiles
    {
        get;
        set;
    }

    /// <summary>Property to set or return whether any files were copied or not.</summary>
    public bool FilesCopied
    {
        get;
        set;
    }
}
