#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, June 05, 2012 1:29:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// A collection of project folders.
	/// </summary>
	class ProjectFolderCollection
		: GorgonBaseNamedObjectDictionary<ProjectFolder>
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a folder by its name.
		/// </summary>
		public ProjectFolder this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to dispose documents in the folder and any child folders.
		/// </summary>
		private void DisposeDocuments(ProjectFolder root)
		{
			foreach (var document in root.Documents)
				document.Dispose();

			root.Documents.Clear();

			foreach (var subFolder in root.Folders)
				DisposeDocuments(subFolder);

			root.Folders.Clear();
		}

		/// <summary>
		/// Function to add a folder to the collection.
		/// </summary>
		/// <param name="folder">Folder to add.</param>
		public void Add(ProjectFolder folder)
		{			
			AddItem(folder);
		}

		/// <summary>
		/// Function to remove a folder from the collection.
		/// </summary>
		/// <param name="folder">Folder to remove.</param>
		public void Remove(ProjectFolder folder)
		{
			RemoveItem(folder);
		}

		/// <summary>
		/// Function to clear the folders.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to delete a project folder.
		/// </summary>
		/// <param name="folder">Folder to delete.</param>
		public void DeleteFolder(ProjectFolder folder)
		{
			folder.TreeNode.Parent.Nodes.Remove(folder.TreeNode);
			DisposeDocuments(folder);
			Remove(folder);			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectFolderCollection"/> class.
		/// </summary>
		public ProjectFolderCollection()
			: base(false)
		{
		}
		#endregion
	}
}
