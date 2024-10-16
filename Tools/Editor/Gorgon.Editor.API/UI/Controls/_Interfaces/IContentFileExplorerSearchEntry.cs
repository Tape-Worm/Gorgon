﻿
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
// Created: May 6, 2019 12:35:46 PM
// 

using Gorgon.Core;

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// Defines a content file explorer entry that can be searched
/// </summary>
public interface IContentFileExplorerSearchEntry
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to set or return whether the entry is visible or not.
    /// </summary>
    bool IsVisible
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the full path for the entry.
    /// </summary>
    string FullPath
    {
        get;
    }

    /// <summary>
    /// Property to return whether or not this entry is a directory.
    /// </summary>
    bool IsDirectory
    {
        get;
    }
}
