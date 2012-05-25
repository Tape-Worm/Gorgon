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
// Created: Friday, May 25, 2012 9:54:57 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.FileSystem;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// Dialog box for files.
	/// </summary>
	public partial class formGorgonFileDialog : Form
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether this is an open file dialog or save file dialog.
		/// </summary>
		public bool IsOpenDialog
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the file system for the dialog.
		/// </summary>
		public GorgonFileSystem FileSystem
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the file system directory list.
		/// </summary>
		private void GetDirectories(GorgonFileSystemDirectory parent)
		{
			TreeNode root = null;

			if (parent == null)
			{
				treeDirectories.Nodes.Clear();
				parent = FileSystem.Directories["/"];

				root = new TreeNode();
				root.Text = parent.Name;
				root.Name = parent.Name;
				root.Tag = parent;
			}
			else
			{
			}

			treeDirectories.BeginUpdate();
			treeDirectories.Nodes.Add(root);
			treeDirectories.EndUpdate();
		}

		private void GetFiles(GorgonFileSystemDirectory directory)
		{
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);


		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formGorgonFileDialog"/> class.
		/// </summary>
		public formGorgonFileDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="formGorgonFileDialog"/> class.
		/// </summary>
		/// <param name="fileSystem">The file system to bind to the dialog.</param>
		/// <param name="isOpenDialog">TRUE to use as an open file dialog, FALSE to use as a save file dialog.</param>
		public formGorgonFileDialog(GorgonFileSystem fileSystem, bool isOpenDialog)
			: this()
		{			
			IsOpenDialog = isOpenDialog;
			FileSystem = fileSystem;

			if (IsOpenDialog)
			{
				buttonOpen.Text = "Open";
				Text = "Open file...";
			}
			else
			{
				buttonOpen.Text = "Save";
				Text = "Save file...";
			}
		}
		#endregion
	}
}
