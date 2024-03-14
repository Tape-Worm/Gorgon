
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
// Created: December 2, 2019 8:14:23 AM
// 


using System.Collections.ObjectModel;
using Gorgon.Core;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Flags to indicate what actions can be taken on a directory
/// </summary>
[Flags]
internal enum DirectoryActions
{
    /// <summary>
    /// No actions can be taken.
    /// </summary>
    None = 0,
    /// <summary>
    /// The directory can be copied.
    /// </summary>
    Copy = 1,
    /// <summary>
    /// The directory can be moved.
    /// </summary>
    Move = 2,
    /// <summary>
    /// The directory can be renamed.
    /// </summary>
    Rename = 4,
    /// <summary>
    /// The directory can be deleted.
    /// </summary>
    Delete = 8,
    /// <summary>
    /// The directory can be excluded from a packed file.
    /// </summary>
    ExcludeFromPackedFile = 16
}

/// <summary>
/// A virtual directory for the <see cref="IFileExplorerVm"/> view model
/// </summary>
internal interface IDirectory
    : IViewModel, IGorgonNamedObject
{

    /// <summary>
    /// Property to return the actions that can be performed on this directory.
    /// </summary>
    DirectoryActions AvailableActions
    {
        get;
    }

    /// <summary>
    /// Property to return the ID for the directory.
    /// </summary>
    string ID
    {
        get;
    }

    /// <summary>
    /// Property to return the full path name for this directory.
    /// </summary>
    string FullPath
    {
        get;
    }

    /// <summary>
    /// Property to return the path to the directory on the physical file system.
    /// </summary>
    string PhysicalPath
    {
        get;
    }

    /// <summary>
    /// Property to return the image name to use for an icon for the directory.
    /// </summary>
    string ImageName
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
    /// Property to return the directories that exist under the root directory.
    /// </summary>
    ObservableCollection<IDirectory> Directories
    {
        get;
    }

    /// <summary>
    /// Property to return the files that exist under the root directory.
    /// </summary>
    ObservableCollection<IFile> Files
    {
        get;
    }

    /// <summary>
    /// Property to return the parent of this directory.
    /// </summary>
    IDirectory Parent
    {
        get;
    }

    /// <summary>
    /// Property to return the command to retrieve total size, in bytes, of all files in this directory, and any subdirectories under this directory.
    /// </summary>
    IEditorCommand<GetSizeInBytesCommandArgs> GetSizeInBytesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to update all children of this directory if it's been renamed.
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

}
