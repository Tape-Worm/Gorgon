#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Tuesday, March 12, 2013 9:22:29 PM
// 
#endregion

using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A treeview node for a directory.
	/// </summary>
	class FileSystemDirectoryNode
		: FileSystemTreeNode
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of node.
		/// </summary>
		public override NodeType NodeType
		{
			get
			{
				return NodeType.Directory;
			}
		}

		/// <summary>
		/// Property to return the directory associated with this node.
		/// </summary>
		public virtual GorgonFileSystemDirectory Directory
		{
			get
			{
#warning We need to link this to an editor folder object when we create one.
				return null; //ScratchArea.ScratchFiles == null ? null : ScratchArea.ScratchFiles.GetDirectory(Name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the node with new directory information.
		/// </summary>
		/// <param name="newDir">New directory info.</param>
		public void UpdateNode(GorgonFileSystemDirectory newDir)
		{
			Name = newDir.FullPath;
			Text = newDir.Name;

			if ((Nodes.Count > 0) && (IsExpanded))
			{
				Collapse();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemDirectoryNode"/> class.
		/// </summary>
		/// <param name="directory">The directory to associate with this node.</param>
		public FileSystemDirectoryNode(GorgonFileSystemDirectory directory)
		{
			Name = directory.FullPath;
			ExpandedImage = Resources.folder_open_16x16;
			CollapsedImage = Resources.folder_16x16;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemDirectoryNode"/> class.
		/// </summary>
		public FileSystemDirectoryNode()
		{
			ExpandedImage = Resources.folder_open_16x16;
			CollapsedImage = Resources.folder_16x16;
		}
		#endregion
	}
}
