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
// Created: Thursday, March 12, 2015 10:44:59 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.IO;

namespace Gorgon.Editor
{
	/// <summary>
	/// Extension method for the tree node collection.
	/// </summary>
	static class FileSystemTreeNodeCollectionExtension
	{
		/// <summary>
		/// Function to return all nodes under this collection.
		/// </summary>
		/// <param name="nodes">Source collection.</param>
		/// <returns>An enumerator for the nodes.</returns>
		public static IEnumerable<FileSystemTreeNode> AllNodes(this TreeNodeCollection nodes)
		{
			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (FileSystemTreeNode node in nodes.Cast<TreeNode>().Where(nodeItem => nodeItem is FileSystemTreeNode))
			{
				yield return node;

				// Skip if there aren't any children.
				if (node.Nodes.Count == 0)
				{
					continue;
				}

				// Gather the children for this node.
				foreach (FileSystemTreeNode childNode in AllNodes(node.Nodes))
				{
					yield return childNode;
				}
			}
		}

		/// <summary>
		/// Function to add a directory to the tree.
		/// </summary>
		/// <param name="nodes">Source collection.</param>
		/// <param name="directory">Directory to add.</param>
		/// <param name="autoExpand">TRUE to expand the new node (if it has children) after it's been added, FALSE to leave collapsed.</param>
		/// <returns>The new node.</returns>
		public static FileSystemDirectoryNode AddDirectory(this TreeNodeCollection nodes, GorgonFileSystemDirectory directory, bool autoExpand)
		{
			if (directory == null)
			{
				throw new ArgumentNullException("directory");
			}

			// Find check to ensure the node is unique.
			FileSystemDirectoryNode result = (from node in nodes.Cast<TreeNode>()
			                            let dirNode = node as FileSystemDirectoryNode
			                            where dirNode != null
			                                  && string.Equals(node.Name, directory.FullPath, StringComparison.OrdinalIgnoreCase)
			                            select dirNode).FirstOrDefault();

			if (result != null)
			{
				return result;
			}

			result = new FileSystemDirectoryNode(directory)
			         {
				         ForeColor = Color.White,
				         Text = directory.Name
			         };

			if ((directory.Directories.Count > 0) || (directory.Files.Count > 0))
			{
				// Add a dummy node to indicate that there are children.
				result.Nodes.Add("DummyNode");
			}

			nodes.Add(result);

			// Expand the parent.
			if ((result.Parent != null)
			    && (!result.Parent.IsExpanded))
			{
				TreeNode parent = result.Parent;
				parent.Expand();
				result = (FileSystemDirectoryNode)parent.Nodes[result.Name];
			}

			// Expand the node if necessary.
			if ((autoExpand)
			    && (result.Nodes.Count > 0))
			{
				result.Expand();
			}

			return result;
		}

		/// <summary>
		/// Function to add a file to the tree.
		/// </summary>
		/// <param name="nodes">Source collection.</param>
		/// <param name="file">File to add.</param>
		/// <returns>The new node.</returns>
		public static FileSystemFileNode AddFile(this TreeNodeCollection nodes, GorgonFileSystemFileEntry file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}

			// Find check to ensure the node is unique.
			FileSystemFileNode result = (from node in nodes.Cast<TreeNode>()
			                       let dirNode = node as FileSystemFileNode
			                       where dirNode != null
			                             && string.Equals(node.Name, file.FullPath, StringComparison.OrdinalIgnoreCase)
			                       select dirNode).FirstOrDefault();

			if (result != null)
			{
				return result;
			}

			result = new FileSystemFileNode();
			result.UpdateFile(file);

			nodes.Add(result);

			if ((result.Parent != null)
			    && (!result.Parent.IsExpanded))
			{
				result.Parent.Expand();
			}

			return result;
		}
	}
}