#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: May 7, 2019 10:27:55 AM
// 
#endregion

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// Event arguments for content file selection or unselection.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ContentFileEntrySelectedEventArgs"/> class.</remarks>
/// <param name="entry">The file entry that was selected or unselected.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="entry"/> parameter is <b>null</b>.</exception>
public class ContentFileEntrySelectedEventArgs(ContentFileExplorerFileEntry entry)
        : EventArgs
{
    /// <summary>
    /// Property to set or return the file entry that was selected or unselected.
    /// </summary>
    public ContentFileExplorerFileEntry FileEntry
    {
        get;
    } = entry ?? throw new ArgumentNullException(nameof(entry));
}
