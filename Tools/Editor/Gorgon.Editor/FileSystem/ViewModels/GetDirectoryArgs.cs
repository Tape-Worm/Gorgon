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
// Created: August 22, 2020 4:21:39 PM
// 

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Arguments for the <see cref="IFileExplorer.GetDirectoryCommand"/>
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="GetDirectoryArgs"/> class.</remarks>
/// <param name="path">The path to the directory to retrieve.</param>
internal class GetDirectoryArgs(string path)
{
    /// <summary>
    /// Property to return the path for the directory to retrieve.
    /// </summary>
    public string Path
    {
        get;
    } = path;

    /// <summary>
    /// Property to set or return the <see cref="IDirectory"/> for the supplied <see cref="Path"/>.
    /// </summary>
    public IDirectory Directory
    {
        get;
        set;
    }
}
