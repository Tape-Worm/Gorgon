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
// Created: November 2, 2018 10:46:16 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// A comparer used to sort the nodes.
    /// </summary>
    internal class FileSystemNodeComparer
        : IComparer<KryptonTreeNode>, IComparer
    {
        #region Variables.
        // The lookup used to relate tree nodes to file explorer nodes.
        private readonly IReadOnlyDictionary<KryptonTreeNode, IFileExplorerNodeVm> _nodeLookup;
        #endregion

        #region IComparer<EditorTreeNode> Members
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int Compare(KryptonTreeNode x, KryptonTreeNode y)
        {
            if (x == y)
            {
                return 0;
            }

            if ((!_nodeLookup.TryGetValue(x, out IFileExplorerNodeVm leftNode))
                || (!_nodeLookup.TryGetValue(y, out IFileExplorerNodeVm rightNode)))
            {
                return 0;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((leftNode.NodeType == NodeType.Directory) && (rightNode.NodeType != NodeType.Directory))
            {
                return -1;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return ((leftNode.NodeType != NodeType.Directory) && (rightNode.NodeType == NodeType.Directory)) ? 1 : string.Compare(x.Text, y.Text, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region IComparer Members
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(object x, object y)
        {
#pragma warning disable IDE0019 // Use pattern matching
            var xNode = x as KryptonTreeNode;
            var yNode = y as KryptonTreeNode;
#pragma warning restore IDE0019 // Use pattern matching

            return ((xNode == null) || (yNode == null)) ? 0 : Compare(xNode, yNode);
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the FileSystemNodeComparer class.</summary>
        /// <param name="nodeLookup">The node lookup.</param>
        /// <param name="recycleNode">The recycle bin node.</param>
        public FileSystemNodeComparer(IReadOnlyDictionary<KryptonTreeNode, IFileExplorerNodeVm> nodeLookup) => _nodeLookup = nodeLookup;
        #endregion
    }
}
