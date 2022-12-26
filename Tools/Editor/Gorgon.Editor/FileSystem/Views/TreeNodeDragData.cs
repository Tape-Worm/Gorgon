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

using System;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// Defines which tree node that is currently being dragged.
/// </summary>
internal class TreeNodeDragData
    : IDirectoryCopyMoveData
{
    #region Properties.
    /// <summary>
    /// Property to return the tree node being dragged.
    /// </summary>        
    public DirectoryTreeNode TreeNode
    {
        get;
        private set;
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

    /// <summary>Property to return the path of the directory being dragged.</summary>
    public string SourceDirectory => TreeNode.Name;

    /// <summary>Property to return the path of the directory that is the target for the drop operation.</summary>
    public string DestinationDirectory => TargetNode?.Name ?? string.Empty;

    /// <summary>
    /// Property to set to return whether any directories or files were copied.
    /// </summary>
    public bool DirectoriesCopied
    {
        get;
        set;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeNodeDragData"/> class.
    /// </summary>
    /// <param name="sourceNode">The tree node being dragged.</param>
    /// <param name="dragOperation">The desired drag operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceNode"/> parameter is <b>null</b>.</exception>
    public TreeNodeDragData(DirectoryTreeNode sourceNode, CopyMoveOperation dragOperation)
    {
        TreeNode = sourceNode ?? throw new ArgumentNullException(nameof(sourceNode));
        Operation = dragOperation;
    }
    #endregion
}
