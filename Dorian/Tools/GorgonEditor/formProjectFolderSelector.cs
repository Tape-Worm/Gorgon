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
// Created: Wednesday, June 06, 2012 8:20:08 AM
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
using GorgonLibrary.UI;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Project folder selection tree.
	/// </summary>
	public partial class formProjectFolderSelector : Form
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the selected path.
		/// </summary>
		public string SelectedPath
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the AfterSelect event of the treeFolders control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
		private void treeFolders_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				if (e.Node == null)
					return;

				if (e.Node.Tag == null)
					SelectedPath = "/";
				else
					SelectedPath = ((ProjectFolder)e.Node.Tag).Path;				
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateButtons();
			}
		}

		/// <summary>
		/// Function to validate the buttons on the form.
		/// </summary>
		private void ValidateButtons()
		{
			buttonOK.Enabled = treeFolders.SelectedNode != null;

			if (!string.IsNullOrEmpty(SelectedPath))
				labelPath.Text = "Selected path: " + SelectedPath;
			else
				labelPath.Text = "Selected path: ";
		}

		/// <summary>
		/// Handles the BeforeCollapse event of the treeFolders control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeFolders_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			try
			{
				if (e.Node == treeFolders.Nodes[0])
				{
					e.Cancel = true;
					return;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to enumerate the folders.
		/// </summary>
		/// <param name="parent">Parent node.</param>
		/// <param name="folders">Folders to enumerate.</param>
		private void EnumerateFolders(TreeNode parent, IEnumerable<ProjectFolder> folders)
		{
			foreach (var folder in folders)
			{
				TreeNode node = new TreeNode(folder.Name);
				node.SelectedImageIndex = node.ImageIndex = 0;
				node.Tag = folder;

				parent.Nodes.Add(node);

				// If we have this path selected, then hilight it.
				if (string.Compare(folder.Path, SelectedPath, true) == 0)
					treeFolders.SelectedNode = node;

				if (folder.Folders.Count > 0)
					EnumerateFolders(node, folder.Folders.OrderBy((ProjectFolder item) => item.Name));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			TreeNode root = null;

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (SelectedPath == null)
					SelectedPath = string.Empty;

				root = new TreeNode("/");
				root.SelectedImageIndex = root.ImageIndex = 0;

				// Enumerate folders.
				treeFolders.BeginUpdate();
				treeFolders.Nodes.Clear();
				treeFolders.Nodes.Add(root);
				EnumerateFolders(root, Program.Project.Folders.OrderBy((ProjectFolder item) => item.Name));

				if (treeFolders.SelectedNode == null)
					treeFolders.SelectedNode = root;
				root.Expand();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				DialogResult = System.Windows.Forms.DialogResult.Cancel;
			}
			finally
			{
				treeFolders.EndUpdate();
				Cursor.Current = Cursors.Default;
				ValidateButtons();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formProjectFolderSelector"/> class.
		/// </summary>
		public formProjectFolderSelector()
		{
			InitializeComponent();
		}
		#endregion
	}
}
