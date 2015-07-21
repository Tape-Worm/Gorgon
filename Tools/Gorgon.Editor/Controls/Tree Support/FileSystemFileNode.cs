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
// Created: Tuesday, March 12, 2013 9:44:49 PM
// 
#endregion

using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor
{
	/// <summary>
	/// A treeview node for a file.
	/// </summary>
	class FileSystemFileNode
		: FileSystemTreeNode
	{
		#region Variables.
		// Editor file attached to this node.
#warning We need to create this type, eventually.
		//private EditorFile _editorFile;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of node.
		/// </summary>
		public override NodeType NodeType => NodeType.File;

		/// <summary>
		/// Property to return the plug-in that is linked to the file attached to the node.
		/// </summary>
#warning Need to find a better way to get plug-in information.
		public IContentData PlugIn
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the file associated with this node.
		/// </summary>
#warning Need to return an editor file, not a file system file.
		public GorgonFileSystemFileEntry File => null;

		/// <summary>
		/// Property to return the editor file for this node.
		/// </summary>
#warning We can get rid of this if we get the above sorted out.
		public object EditorFile => null;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to get the file data.
		/// </summary>
		private void GetFileData()
		{
			ExpandedImage = Resources.unknown_document_16x16;
			CollapsedImage = ExpandedImage;
			PlugIn = null;

			/*
			EditorMetaDataFile.Files.TryGetValue(File.FullPath, out _editorFile);

			PlugIn = ContentManagement.GetContentPlugInForFile(_editorFile) ?? ContentManagement.GetContentPlugInForFile(File.Extension);

			if ((PlugIn != null)
				&& (_editorFile != null))
			{
				ExpandedImage = CollapsedImage = PlugIn.GetContentIcon();
			}*/
		}

		/// <summary>
		/// Function to update the file information.
		/// </summary>
		/// <param name="file">File system file entry to use.</param>
		public void UpdateFile(GorgonFileSystemFileEntry file)
		{
			Name = file.FullPath;
			Text = file.Name;
			GetFileData();

			// We have dependencies, so update.
			/*if ((_editorFile != null) && (_editorFile.DependsOn.Count > 0))
			{
				Nodes.Add(new TreeNode("DummyNode"));
			}*/
		}
		#endregion
	}
}
