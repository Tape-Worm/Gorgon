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
// Created: Tuesday, June 05, 2012 1:24:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// A folder object for the file system.
	/// </summary>
	class ProjectFolder
		: GorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the tree node for the project folder.
		/// </summary>
		public Node TreeNode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the owner of this folder.
		/// </summary>
		public ProjectFolder Owner
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the project that owns this folder.
		/// </summary>
		public Project Project
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the documents within this folder.
		/// </summary>
		public DocumentCollection Documents
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the folders within this folder.
		/// </summary>
		public ProjectFolderCollection Folders
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectFolder"/> struct.
		/// </summary>
		/// <param name="project">Project that owns this folder.</param>
		/// <param name="owner">Folder that owns this folder.</param>
		/// <param name="name">The name of the folder.</param>
		public ProjectFolder(Project project, ProjectFolder owner, string name)
			: base(name)
		{
			GorgonDebug.AssertNull<Project>(project, "project");

			Project = project;
			Documents = new DocumentCollection();
			Folders = new ProjectFolderCollection();
			Owner = owner;

			// Create the node.
			TreeNode = new Node(name);
			TreeNode.Tag = this;
			TreeNode.Image = Properties.Resources.folder_16x16;

			if (Owner != null)
				Owner.TreeNode.Nodes.Add(TreeNode);
		}
		#endregion
	}
}
