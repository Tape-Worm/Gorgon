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

using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
	/// <summary>
	/// A treeview node for the root of the file system.
	/// </summary>
	class FileSystemRootNode
		: FileSystemTreeNode
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of node.
		/// </summary>
		public override NodeType NodeType => NodeType.Root;

		/// <summary>
		/// Gets or sets the text displayed in the label of the tree node.
		/// </summary>
		public override string Text
		{
			get
			{
				return base.Text;
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
				// Intentionally left blank.
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemRootNode"/> class.
		/// </summary>
		/// <param name="fileSystemName">The name of the file system.</param>
		public FileSystemRootNode(string fileSystemName)
		{
			Name = fileSystemName;
			base.Text = fileSystemName;
			ExpandedImage = CollapsedImage = Resources.file_system_root_node_16x16;
		}
		#endregion
	}
}