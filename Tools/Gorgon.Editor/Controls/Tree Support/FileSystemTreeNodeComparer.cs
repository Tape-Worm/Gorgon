#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, March 12, 2015 10:43:32 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A comparer for node sorting.
	/// </summary>
	sealed class FileSystemTreeNodeComparer
		: IComparer<FileSystemTreeNode>, IComparer
	{
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
		public int Compare(FileSystemTreeNode x, FileSystemTreeNode y)
		{
			// For a root node always return it before other nodes.
			if ((x.NodeType & NodeType.Root) == NodeType.Root)
			{
				return -1;
			}

			if ((y.NodeType & NodeType.Root) == NodeType.Root)
			{
				return 1;
			}

			// If this is a directory, we need to place it before files.
			if (((x.NodeType & NodeType.Directory) == NodeType.Directory)
			    && ((y.NodeType & NodeType.Directory) != NodeType.Directory))
			{
				return -1;
			}

			if (((y.NodeType & NodeType.Directory) == NodeType.Directory)
			    && ((x.NodeType & NodeType.Directory) != NodeType.Directory))
			{
				return 1;
			}

			// Otherwise, sort by text.
			return string.Compare(x.Text, y.Text, StringComparison.OrdinalIgnoreCase);
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
			var xNode = x as FileSystemTreeNode;
			var yNode = y as FileSystemTreeNode;

			if ((xNode == null)
			    || (yNode == null))
			{
				return 0;
			}

			return Compare(xNode, yNode);
		}
		#endregion
	}
}