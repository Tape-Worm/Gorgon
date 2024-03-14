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
// Created: June 1, 2020 12:09:23 AM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// Event arguments for the <see cref="ContentFileExplorer.FileEntriesFocused"/> event.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ContentFileEntriesFocusedArgs"/> class.</remarks>
/// <param name="focusedFiles">The focused files.</param>
public class ContentFileEntriesFocusedArgs(IReadOnlyList<ContentFileExplorerFileEntry> focusedFiles)
        : EventArgs
{
    /// <summary>
    /// Property to return the files that were focused in the file selector.
    /// </summary>
    public IReadOnlyList<ContentFileExplorerFileEntry> FocusedFiles
    {
        get;
    } = focusedFiles ?? [];
}
