
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
// Created: May 6, 2019 10:14:55 AM
// 

using Gorgon.Core;
using Gorgon.IO;

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// An file system directory entry for the <see cref="ContentFileExplorer"/>
/// </summary>
public class ContentFileExplorerDirectoryEntry
    : PropertyMonitor, IContentFileExplorerSearchEntry
{

    // Flag to indicate that this entry is visible.
    private bool _visible = true;
    // Flag to indicate that the entry is selected.
    private bool _isExpanded = true;

    /// <summary>
    /// Property to return the parent of this entry.
    /// </summary>
    public ContentFileExplorerDirectoryEntry Parent
    {
        get;
    }

    /// <summary>
    /// Property to return the files for the directory.
    /// </summary>
    public IReadOnlyList<ContentFileExplorerFileEntry> Files
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether this entry is visible in the list or not.
    /// </summary>
    public bool IsVisible
    {
        get => _visible;
        set
        {
            if (_visible == value)
            {
                return;
            }

            OnPropertyChanging();
            _visible = value;
            OnPropertyChanged();

            NotifyChildren();
        }
    }

    /// <summary>
    /// Property to set or return whether the entry is expanded or not.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
            {
                return;
            }

            OnPropertyChanging();
            _isExpanded = value;
            OnPropertyChanged();

            NotifyChildren();
        }
    }

    /// <summary>
    /// Property to return the name of the entry.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the full path to the entry.
    /// </summary>
    public string FullPath
    {
        get;
    }

    /// <summary>Property to return whether or not this entry is a directory.</summary>
    bool IContentFileExplorerSearchEntry.IsDirectory => true;

    /// <summary>
    /// Function to notify the file entries that visibility or expansion has changed.
    /// </summary>
    private void NotifyChildren()
    {
        foreach (ContentFileExplorerFileEntry entry in Files)
        {
            entry.NotifyPropertyChanged(nameof(ContentFileExplorerFileEntry.IsVisible));
        }
    }

    /// <summary>Initializes a new instance of the <see cref="ContentFileExplorerDirectoryEntry"/> class.</summary>
    /// <param name="fullPath">The full path to the directory.</param>
    /// <param name="files">The files under this directory.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fullPath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fullPath"/> parameter is empty.</exception>
    public ContentFileExplorerDirectoryEntry(string fullPath, IReadOnlyList<ContentFileExplorerFileEntry> files)
    {
        if (fullPath is null)
        {
            throw new ArgumentNullException(nameof(fullPath));
        }

        if (string.IsNullOrWhiteSpace(fullPath))
        {
            throw new ArgumentEmptyException(nameof(fullPath));
        }

        Files = files ?? [];
        FullPath = fullPath.FormatDirectory('/');
        Name = FullPath;
    }
}
