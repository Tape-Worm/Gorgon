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
// Created: December 4, 2019 10:23:15 AM
// 
#endregion

using Gorgon.Core;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// A virtual file for the <see cref="IFileExplorer"/> view model.
/// </summary>
internal interface IFile
    : IViewModel, IGorgonNamedObject
{
    /// <summary>
    /// Property to return the ID for the file.
    /// </summary>
    string ID
    {
        get;
    }

    /// <summary>
    /// Property to return the parent directory for this file.
    /// </summary>
    IDirectory Parent
    {
        get;
    }

    /// <summary>
    /// Property to return the type of file.
    /// </summary>
    string Type
    {
        get;
    }

    /// <summary>
    /// Property to return the size of the file, in bytes.
    /// </summary>
    long SizeInBytes
    {
        get;
    }

    /// <summary>
    /// Property to return the full path to the file in the virtual file system.
    /// </summary>
    string FullPath
    {
        get;
    }

    /// <summary>
    /// Property to return the full path of the file on the physical file system.
    /// </summary>
    string PhysicalPath
    {
        get;
    }

    /// <summary>
    /// Property to return the extension name for the file.
    /// </summary>
    string Extension
    {
        get;
    }

    /// <summary>
    /// Property to return the name of the file (without file extension).
    /// </summary>
    string BaseName
    {
        get;
    }

    /// <summary>
    /// Property to return the command that is executed when the parent directory is renamed.
    /// </summary>
    IEditorCommand<object> ParentRenamedCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to rename this directory.
    /// </summary>
    IEditorCommand<RenameArgs> RenameCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the name of the image to use for the file icon.
    /// </summary>
    string ImageName
    {
        get;
    }

    /// <summary>Property to set or return the metadata associated with the file.</summary>
    ProjectItemMetadata Metadata
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return whether the file is open or not.
    /// </summary>
    bool IsOpen
    {
        get;
    }

    /// <summary>
    /// Property to return whether the file is changed or not.
    /// </summary>
    bool IsChanged
    {
        get;
    }

    /// <summary>
    /// Property to set or return a flag to indicate whether the directory was marked for a cut operation.
    /// </summary>
    bool IsCut
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command used to refresh the file data.
    /// </summary>
    IEditorCommand<object> RefreshCommand
    {
        get;
    }
}
