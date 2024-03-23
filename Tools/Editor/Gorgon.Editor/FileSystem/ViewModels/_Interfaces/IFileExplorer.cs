
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
// Created: September 4, 2018 10:16:21 PM
// 

using System.Collections.ObjectModel;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The view model for our file explorer
/// </summary>
internal interface IFileExplorer
    : IViewModel
{
    /// <summary>
    /// Event triggered when the file system has been updated.
    /// </summary>
    event EventHandler FileSystemUpdated;

    /// <summary>
    /// Property to return the clipboard handler for this view model.
    /// </summary>
    IClipboardHandler Clipboard
    {
        get;
    }

    /// <summary>
    /// Property to return the metadata for the content plug ins.
    /// </summary>
    IReadOnlyList<IContentPlugInMetadata> PlugInMetadata
    {
        get;
    }

    /// <summary>
    /// Property to return the currently selected directory.
    /// </summary>
    IDirectory SelectedDirectory
    {
        get;
    }

    /// <summary>
    /// Property to return the currently selected file(s).
    /// </summary>
    ObservableCollection<IFile> SelectedFiles
    {
        get;
    }

    /// <summary>
    /// Property to return the root directory for the file system.
    /// </summary>
    IDirectory Root
    {
        get;
    }

    /// <summary>
    /// Property to return the command to execute when a directory is selected.
    /// </summary>
    IEditorCommand<string> SelectDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to execute when a file is selected.
    /// </summary>
    IEditorCommand<IReadOnlyList<string>> SelectFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to rename a directory.
    /// </summary>
    IEditorCommand<RenameArgs> RenameDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to rename a file.
    /// </summary>
    IEditorCommand<RenameArgs> RenameFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to create a new directory.
    /// </summary>
    IEditorCommand<CreateDirectoryArgs> CreateDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to delete a directory.
    /// </summary>
    IEditorAsyncCommand<DeleteArgs> DeleteDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to copy a file (or files).
    /// </summary>
    IEditorAsyncCommand<IFileCopyMoveData> CopyFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to move a file (or files).
    /// </summary>
    IEditorAsyncCommand<IFileCopyMoveData> MoveFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to copy a directory.
    /// </summary>
    IEditorAsyncCommand<IDirectoryCopyMoveData> CopyDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to move a directory.
    /// </summary>
    IEditorAsyncCommand<IDirectoryCopyMoveData> MoveDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to delete a file, or multiple files.
    /// </summary>
    IEditorAsyncCommand<DeleteArgs> DeleteFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to export the selected directory, and its contents to the physical filesystem.
    /// </summary>
    IEditorAsyncCommand<object> ExportDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to export the selected file(s) to the physical filesystem.
    /// </summary>
    IEditorAsyncCommand<object> ExportFilesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to perform a search for files.
    /// </summary>
    IEditorCommand<string> SearchCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to import files and directories from the physical file system.
    /// </summary>
    IEditorAsyncCommand<IImportData> ImportCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the list of search results for a filtered node list.
    /// </summary>
    IReadOnlyList<IFile> SearchResults
    {
        get;
    }

    /// <summary>
    /// Property to set or return the command to execute when a content node is opened.
    /// </summary>
    IEditorAsyncCommand<object> OpenContentFileCommand
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command to refresh the file system.
    /// </summary>
    IEditorAsyncCommand<object> RefreshCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to retrieve a directory object by path.
    /// </summary>
    IEditorCommand<GetDirectoryArgs> GetDirectoryCommand
    {
        get;
    }
}
