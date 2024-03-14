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
// Created: August 17, 2020 2:02:30 PM
// 
#endregion

using System.ComponentModel;
using Gorgon.Editor.Tools;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageSplitTool;

/// <summary>
/// The view model used for selecting images to split.
/// </summary>
internal interface ISplit
    : IEditorTool
{
    /// <summary>
    /// Property to set or return the output directory for the resulting images/sprites.
    /// </summary>
    string OutputDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the progress for the splitting operation.
    /// </summary>
    string Progress
    {
        get;
    }

    /// <summary>
    /// Property to return the sprite file entries.
    /// </summary>
    IReadOnlyList<ContentFileExplorerDirectoryEntry> SpriteFileEntries
    {
        get;
    }

    /// <summary>
    /// Property to return the list of selected files.
    /// </summary>
    IReadOnlyList<ContentFileExplorerFileEntry> SelectedFiles
    {
        get;
    }

    /// <summary>Property to return the image used for previewing.</summary>
    IGorgonImage PreviewImage
    {
        get;
    }

    /// <summary>
    /// Property to return the command that will refresh the sprite preview data.
    /// </summary>
    IEditorAsyncCommand<IReadOnlyList<ContentFileExplorerFileEntry>> RefreshSpritePreviewCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to search through the file list.
    /// </summary>
    IEditorCommand<string> SearchCommand
    {
        get;
    }

    /// <summary>Property to return the folder selection command.</summary>
    IEditorCommand<object> SelectFolderCommand
    {
        get;
    }

    /// <summary>
    /// Function to split the images and write them to the file system.
    /// </summary>
    IEditorAsyncCommand<object> SplitImageCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to cancel the operation.
    /// </summary>
    IEditorAsyncCommand<CancelEventArgs> CancelCommand
    {
        get;
    }
}
