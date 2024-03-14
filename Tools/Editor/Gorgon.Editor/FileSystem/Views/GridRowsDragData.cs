#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 27, 2018 10:11:34 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// Defines which grid rows that are currently being dragged.
/// </summary>    
internal class GridRowsDragData
    : IFileCopyMoveData, IContentFileDragData
{
    #region Variables.
    // The list of paths for the files being dragged.
    private readonly string[] _filePaths;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the grid rows being dragged.
    /// </summary>
    public IReadOnlyList<DataGridViewRow> GridRows
    {
        get;
    }

    /// <summary>
    /// Property to return the type of operation to be performed when the drag is finished.
    /// </summary>
    public CopyMoveOperation Operation
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the node that is the target for the drop operation.
    /// </summary>
    public DirectoryTreeNode TargetNode
    {
        get;
        set;
    }

    /// <summary>Property to return the list of files being copied/moved.</summary>
    public IReadOnlyList<string> SourceFiles
    {
        get;
    }

    /// <summary>Property to return the path of the directory that is the target for the drop operation.</summary>
    public string DestinationDirectory => TargetNode?.Name ?? string.Empty;

    /// <summary>
    /// Property to set or return whether any files were copied or not.
    /// </summary>
    public bool FilesCopied
    {
        get;
        set;
    }

    /// <summary>Property to return the path to the content file being dragged and dropped.</summary>
    IReadOnlyList<string> IContentFileDragData.FilePaths => _filePaths;

    /// <summary>Property to set or return whether to cancel the drag/drop operation.</summary>
    bool IContentFileDragData.Cancel
    {
        get;
        set;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GridRowsDragData"/> class.
    /// </summary>
    /// <param name="fileRows">The grid rows being dragged.</param>
    /// <param name="fileColumnIndex">The index of the cell that holds the file information.</param>
    /// <param name="dragOperation">The desired drag operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileRows"/> parameter is <b>null</b>.</exception>
    public GridRowsDragData(IReadOnlyList<DataGridViewRow> fileRows, int fileColumnIndex, CopyMoveOperation dragOperation)
    {
        IFile[] files = fileRows.OfType<DataGridViewRow>()
                                .Select(item => ((IFile)item.Cells[fileColumnIndex].Value))
                                .ToArray();

        GridRows = fileRows ?? throw new ArgumentNullException(nameof(fileRows));
        SourceFiles = files.Select(item => item.ID).ToArray();
        _filePaths = files.Select(item => item.FullPath).ToArray();
        Operation = dragOperation;
    }
    #endregion
}
