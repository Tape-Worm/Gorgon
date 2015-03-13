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
// Created: Thursday, March 12, 2015 11:31:18 PM
// 
#endregion

using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A treeview node for the root of the file system.
	/// </summary>
	class FileSystemRootNode
		: FileSystemDirectoryNode
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of node.
		/// </summary>
		public override NodeType NodeType
		{
			get
			{
				return NodeType.Root;
			}
		}

		/// <summary>
		/// Property to return the directory associated with this node.
		/// </summary>
#warning Again, we need to return something other than a file system directory here.  Perhaps we should separate this out from the directory node and have it as a standalone.
		public override GorgonFileSystemDirectory Directory
		{
			get
			{
				return null; //ScratchArea.ScratchFiles == null ? null : ScratchArea.ScratchFiles.RootDirectory;
			}
		}

		/// <summary>
		/// Gets or sets the name of the tree node.
		/// </summary>
		/// <returns>A <see cref="T:System.String" /> that represents the name of the tree node.</returns>
		public override string Text
		{
			get
			{
				return "TODO: Return file system file name, somehow";//FileManagement.Filename;
			}
			set
			{
				// Nothing.
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemRootNode"/> class.
		/// </summary>
		public FileSystemRootNode()
		{
			ExpandedImage = CollapsedImage = Resources.file_system_root_node_16x16;
		}
		#endregion
	}
}