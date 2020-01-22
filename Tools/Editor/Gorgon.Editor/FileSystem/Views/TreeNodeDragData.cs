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
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.Content;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// Defines which tree node and file system node that is currently being dragged.
    /// </summary>
    internal class TreeNodeDragData
        : IFileExplorerNodeDragData, IContentFileDragData
    {
        #region Properties.
        /// <summary>
        /// Property to return the file system node being dragged.
        /// </summary>
        public IFileExplorerNodeVm Node
        {
            get;
        }

        /// <summary>
        /// Property to return the tree node being dragged.
        /// </summary>
        public KryptonTreeNode TreeNode
        {
            get;
        }

        /// <summary>
        /// Property to return the type of operation to be performed when the drag is finished.
        /// </summary>
        public DragOperation DragOperation
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the node that is the target for the drop operation.
        /// </summary>
        public IFileExplorerNodeVm TargetNode
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the tree node that is the target for the drop operation.
        /// </summary>
        public KryptonTreeNode TargetTreeNode
        {
            get;
            set;
        }

        /// <summary>Property to return the content file being dragged and dropped.</summary>
        OLDE_IContentFile IContentFileDragData.File => Node as OLDE_IContentFile;

        /// <summary>Property to set or return whether to cancel the drag/drop operation.</summary>
        bool IContentFileDragData.Cancel
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeDragData"/> class.
        /// </summary>
        /// <param name="treeNode">The tree node being dragged.</param>
        /// <param name="node">The file system node being dragged.</param>
        /// <param name="dragOperation">The desired drag operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="treeNode"/>, or the <paramref name="node"/> parameter is <b>null</b>.</exception>
        public TreeNodeDragData(KryptonTreeNode treeNode, IFileExplorerNodeVm node, DragOperation dragOperation)
        {
            TreeNode = treeNode ?? throw new ArgumentNullException(nameof(treeNode));
            Node = node ?? throw new ArgumentNullException(nameof(node));
            DragOperation = dragOperation;
        }
        #endregion
    }
}
