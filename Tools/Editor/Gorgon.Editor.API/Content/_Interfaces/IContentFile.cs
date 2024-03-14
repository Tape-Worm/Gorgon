
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: October 30, 2018 7:11:23 PM
// 


using Gorgon.Core;
using Gorgon.Editor.Metadata;

namespace Gorgon.Editor.Content;

/// <summary>
/// A data structure representing a file containing content
/// </summary>
public interface IContentFile
    : IGorgonNamedObject
{

    /// <summary>
    /// Event triggered if this content file was renamed.
    /// </summary>
    event EventHandler<ContentFileRenamedEventArgs> Renamed;



    /// <summary>
    /// Property to set or return whether the file has changes.
    /// </summary>
    bool IsChanged
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the path to the file.
    /// </summary>
    string Path
    {
        get;
    }

    /// <summary>
    /// Property to return the extension for the file.
    /// </summary>
    string Extension
    {
        get;
    }

    /// <summary>
    /// Property to return the metadata associated with the file.
    /// </summary>
    ProjectItemMetadata Metadata
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether the file is open for editing or not.
    /// </summary>
    bool IsOpen
    {
        get;
        set;
    }



    /// <summary>
    /// Function to link a content file to be dependant upon this content.
    /// </summary>
    /// <param name="file">The file to link to this content.</param>
    void LinkContent(IContentFile file);

    /// <summary>
    /// Function to unlink a content file from being dependant upon this content.
    /// </summary>
    /// <param name="file">The file to unlink from this content.</param>
    void UnlinkContent(IContentFile file);

    /// <summary>
    /// Function to remove all child dependency links from this content.
    /// </summary>
    void ClearLinks();

    /// <summary>
    /// Function to notify that the metadata should be refreshed.
    /// </summary>
    void RefreshMetadata();

    /// <summary>Function called to refresh the information about the file.</summary>
    void Refresh();

}
