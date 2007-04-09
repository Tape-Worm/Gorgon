#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Sunday, April 01, 2007 3:51:26 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SharpUtilities;
using SharpUtilities.Utility;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form used to edit and display the file system.
	/// </summary>
	public partial class formFileSystemWindow : Form
	{
		#region Constants.
		private const string _dummyFile = "$$_TEMPFILE_$$_DONOT_USE_$$";		// Dummy file name.
		#endregion

		#region Value Types.
		/// <summary>
		/// Value type containing drag and drop data.
		/// </summary>
		private struct DragDropData
		{
			#region Variables.
			/// <summary>
			/// File system the node comes from.
			/// </summary>
			public FileSystem SourceFileSystem;
			/// <summary>
			/// The node being moved/copied.
			/// </summary>
			public TreeNode SourceNode;
			#endregion

			#region Constructor.			
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="fileSystem">Source file system.</param>
			/// <param name="node">Source node.</param>
			public DragDropData(FileSystem fileSystem, TreeNode node)
			{
				SourceFileSystem = fileSystem;
				SourceNode = node;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private FileSystem _fileSystem;							// File system.
		private string _rootPath = string.Empty;				// File system root path.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the file system for this window.
		/// </summary>
		public FileSystem FileSystem
		{
			get
			{
				return _fileSystem;
			}
			set
			{
				_fileSystem = value;
				if (value != null)
					Text = "File System - " + _rootPath + " [" + _fileSystem.GetType().Name + "]";
			}
		}

		/// <summary>
		/// Property to set or return the root path for the file system.
		/// </summary>
		public string RootPath
		{
			get
			{
				return _rootPath;
			}
			set
			{
				_rootPath = value;

				if (value != null)
					Text = "File System - \"" + _rootPath + "\" [" + _fileSystem.GetType().Name + "]";
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to fix the tree node full path property.
		/// </summary>
		/// <param name="node">Node to correct.</param>
		/// <returns>The fixed path.</returns>
		private string FixNodePath(TreeNode node)
		{
			try
			{
				if (node.Text == @"\")
					return @"\";
				else
					return node.FullPath.Replace(@"\\", @"\") + @"\";
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Function to retrieve the nodes under a root.
		/// </summary>
		/// <param name="rootNode">Parent node to receive children.</param>
		private void GetNodes(TreeNode rootNode)
		{
			TreeNode parentNode = null;			// Parent node.
			string path = string.Empty;			// Path to the entry.
			string destPath = string.Empty;		// Destination node path.
			string pathParent = string.Empty;	// Parent to the path.

			try
			{
				// If no node, then start at the root.
				if (rootNode == null)
				{
					// Remove previous nodes.
					treePaths.Nodes.Clear();

					// Add root node.
					parentNode = treePaths.Nodes.Add(@"\");

					// Add a dummy node.
					parentNode.Nodes.Add("$DUMMYNODE$");
				}
				else
				{
					// Remove child nodes.
					rootNode.Nodes.Clear();

					destPath = Path.GetDirectoryName(FixNodePath(rootNode).ToLower());
					if ((destPath == null) || (destPath == string.Empty))
						destPath = @"\";

					// Add file system entries under the node.
					foreach (FileSystemEntry entry in _fileSystem)
					{
						// Build the path.
						path = entry.Path.Substring(0, entry.Path.Length - 1).ToLower();
						if (path == string.Empty)
							path = @"\";

						// Get the parent of the path.
						pathParent = Path.GetDirectoryName(path);
						if (pathParent == string.Empty)
							pathParent = @"\";

						// Add the entry.
						if ((pathParent != null) && (pathParent.ToLower() == destPath))
						{
							// Get the path.
							path = Path.GetDirectoryName(entry.Path);
							path = path.Substring(path.LastIndexOf(@"\") + 1);
							if (!rootNode.Nodes.ContainsKey(path))
							{
								parentNode = rootNode.Nodes.Add(path, path);

								// Add a dummy node.
								parentNode.Nodes.Add("$DUMMYNODE$");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to refresh the tree view.
		/// </summary>
		private void RefreshTree()
		{
			GetNodes(null);

			// Expand the first node.
			treePaths.Nodes[0].Expand();
		}

		/// <summary>
		/// Function to validate the popup menu.
		/// </summary>
		private void ValidatePopup()
		{
			menuItemAddPath.Enabled = false;
			menuItemRemovePath.Enabled = false;

			if (treePaths.SelectedNode == null)
				return;

			menuItemAddPath.Enabled = true;
			
			// Allow add, but not delete on root level.
			if (treePaths.SelectedNode.Text != @"\")
				menuItemRemovePath.Enabled = true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RefreshTree();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formFileSystemWindow()
		{
			InitializeComponent();
		}
		#endregion

		/// <summary>
		/// Handles the MouseDown event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void treePaths_MouseDown(object sender, MouseEventArgs e)
		{
			TreeNode hitNode = null;		// Node from hit test.

			// Select the node if the right mouse button is clicked.
			if (e.Button == MouseButtons.Right)
			{
				hitNode = treePaths.HitTest(e.X, e.Y).Node;
				if (hitNode != null)
					treePaths.SelectedNode = hitNode;
			}
			
			ValidatePopup();			
		}

		/// <summary>
		/// Function to add a path.
		/// </summary>
		/// <param name="parent">Parent node.</param>
		/// <param name="pathName">Path to add.</param>
		private void AddPath(TreeNode parent, string pathName)
		{
			TreeNode node = null;					// Newly added node.

			try
			{
				_fileSystem.WriteFile(FixNodePath(parent) + pathName + @"\" + _dummyFile, Encoding.UTF8.GetBytes("This file will be removed before the file system is committed."));

				// Add the node.
				node = parent.Nodes.Add(pathName);
				// Add the dummy.
				node.Nodes.Add("$$DUMMYNODE$$");

				// Select the node.
				treePaths.SelectedNode = node;
			}
			catch (EntryExistsException eEx)
			{
				UI.ErrorBox(this, "The path '" + pathName + "' already exists.", eEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemAddPath control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemAddPath_Click(object sender, EventArgs e)
		{
			formPathNameInput nameInput = null;		// Path name input.

			try
			{
				nameInput = new formPathNameInput();

				// Add a dummy entry to the file system.
				if (nameInput.ShowDialog(this) == DialogResult.OK)
					AddPath(treePaths.SelectedNode, nameInput.PathName);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (nameInput != null)
					nameInput.Dispose();
				nameInput = null;
			}
		}

		/// <summary>
		/// Function to add a file system entry to the view list.
		/// </summary>
		/// <param name="entry">Entry to add.</param>
		private void AddFileEntry(FileSystemEntry entry)
		{
			switch (entry.Extension.ToLower())
			{
				case ".gorsprite":
					viewFiles.Items.Add(entry.Filename + entry.Extension, "GorgonGeneric");
					break;
				case ".gorfont":
					viewFiles.Items.Add(entry.Filename + entry.Extension, "GorgonFont");
					break;
				case ".xml":
					viewFiles.Items.Add(entry.Filename + entry.Extension, "XML");
					break;
				case ".txt":
					viewFiles.Items.Add(entry.Filename + entry.Extension, "Document");
					break;
				case ".png":
				case ".tga":
				case ".dib":
				case ".bmp":
				case ".gif":
				case ".jpg":
				case ".jpeg":
				case ".pfm":
				case ".ppm":
				case ".dds":
					viewFiles.Items.Add(entry.Filename + entry.Extension, "Image");
					break;
				default:
					viewFiles.Items.Add(entry.Filename + entry.Extension, "Binary");
					break;
			}
		}

		/// <summary>
		/// Handles the AfterSelect event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
		private void treePaths_AfterSelect(object sender, TreeViewEventArgs e)
		{
			viewFiles.Items.Clear();

			// Refresh the right hand view.
			foreach (FileSystemEntry entry in _fileSystem)
			{
				// Add the files to the list view.
				if (entry.Path.ToLower() == FixNodePath(e.Node).ToLower())
				{
					// Don't add the dummy file.
					if (entry.Filename != _dummyFile)
						AddFileEntry(entry);
				}
			}

			e.Node.EndEdit(true);
		}

		/// <summary>
		/// Handles the AfterCollapse event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
		private void treePaths_AfterCollapse(object sender, TreeViewEventArgs e)
		{
			e.Node.SelectedImageIndex = e.Node.ImageIndex = 0;
		}

		/// <summary>
		/// Handles the AfterExpand event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
		private void treePaths_AfterExpand(object sender, TreeViewEventArgs e)
		{
			// Get the nodes.
			GetNodes(e.Node);
			e.Node.SelectedImageIndex = e.Node.ImageIndex = 1;
		}

		/// <summary>
		/// Function to move a node to a new path.
		/// </summary>
		/// <param name="node">Node to move.</param>
		/// <param name="path">Path to move into.</param>
		private void MovePathNode(TreeNode node, string path)
		{
			string previousPath = string.Empty;		// Previous path.
			string newPath = string.Empty;			// New path.

			// Get the old name.
			previousPath = FixNodePath(node);
			previousPath = Path.GetDirectoryName(previousPath);
			if ((previousPath == null) || (previousPath == string.Empty))
				previousPath = @"\";

			// Extract the old name and replace with the new name.
			newPath = path;

			// Move the node.
			_fileSystem.MovePath(previousPath, newPath);
		}

		/// <summary>
		/// Handles the AfterLabelEdit event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.NodeLabelEditEventArgs"/> instance containing the event data.</param>
		private void treePaths_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			string previousPath = string.Empty;		// Previous path.
			string newPath = string.Empty;			// New path.

			// If no change, then do nothing.
			if (e.Label == null)
			{
				e.CancelEdit = true;
				return;
			}

			// Don't allow us to use root.
			if (e.Label.Replace("/", @"\") == @"\")
			{
				UI.ErrorBox(this, "Cannot rename to root.");
				e.CancelEdit = true;
				return;
			}

			// Don't allow invalid characters.
			if (!FileSystem.ValidateFilename(e.Label, true))
			{
				UI.ErrorBox(this, "Invalid path name.");
				e.CancelEdit = true;
				return;
			}

			if (e.Label.Replace("/", @"\").IndexOf(@"\") > -1)
			{
				UI.ErrorBox(this, "Invalid path name.");
				e.CancelEdit = true;
				return;
			}

			try
			{
				// Move the node.
				MovePathNode(e.Node, FixNodePath(e.Node.Parent) + e.Label);

				// Turn off editing.
				treePaths.LabelEdit = false;
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
				e.CancelEdit = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
				e.CancelEdit = true;
			}
		}

		/// <summary>
		/// Handles the BeforeLabelEdit event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.NodeLabelEditEventArgs"/> instance containing the event data.</param>
		private void treePaths_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			// Don't change the root.
			if (e.Node.Text == @"\")
			{				
				e.CancelEdit = true;
				return;
			}

			// Don't allow empty names.
			if (e.Label == string.Empty)
			{
				e.CancelEdit = true;
				return;
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void treePaths_KeyDown(object sender, KeyEventArgs e)
		{
			if (treePaths.SelectedNode != null)
			{
				if (e.KeyCode == Keys.F2)
				{
					treePaths.LabelEdit = true;
					treePaths.SelectedNode.BeginEdit();
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				// Unmount the file system.
				Gorgon.FileSystems.Remove(_fileSystem);
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the DragOver event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void treePaths_DragOver(object sender, DragEventArgs e)
		{
			DragDropData data;				// Data from drag and drop operation.
			TreeNode hitNode = null;		// Hit test node.	
			TreeNode searchNode = null;		// Search node.

			data = (DragDropData)e.Data.GetData(typeof(DragDropData));

			// Destination node.
			hitNode = treePaths.HitTest(treePaths.PointToClient(new Point(e.X, e.Y))).Node;
			if ((hitNode != null) && (data.SourceNode != hitNode))
			{
				// Find parent.
				searchNode = hitNode.Parent;
				while ((searchNode != data.SourceNode) && (searchNode != null))
					searchNode = searchNode.Parent;
				
				if (searchNode == null)
				{
					e.Effect = DragDropEffects.All;
					treePaths.SelectedNode = hitNode;
					hitNode.Expand();
				}
				else
				{
					e.Effect = DragDropEffects.None;
					treePaths.SelectedNode = null;
				}
			}
			else
			{
				e.Effect = DragDropEffects.None;
				treePaths.SelectedNode = null;
			}
		}

		/// <summary>
		/// Handles the DragDrop event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void treePaths_DragDrop(object sender, DragEventArgs e)
		{
			string destNodePath = string.Empty;		// Destination node path.			
			DragDropData data;						// Drag and drop data.

			if (treePaths.SelectedNode == null)
				return;

			try
			{
				// Get the drag and drop data.
				data = (DragDropData)e.Data.GetData(typeof(DragDropData));

				// Get destination node path.
				destNodePath = FixNodePath(treePaths.SelectedNode);

				// If we're within the same file system, move the nodes.
				if (data.SourceFileSystem == _fileSystem)
				{
					// Move the data.
					MovePathNode(data.SourceNode, destNodePath + data.SourceNode.Text);
					// Remove from the node list.
					if (!data.SourceNode.IsExpanded)
						data.SourceNode.Nodes.Add("$$DUMMYNODE$$");

					// Re-add to new parent.
					treePaths.SelectedNode.Nodes.Add(data.SourceNode);
					data.SourceNode.Remove();				
				}
				else
				{
					// Build a list of files that are to be copied from the source file system.
					foreach(FileSystemEntry entry in data.SourceFileSystem)
					{
						if (entry.Path.ToLower().StartsWith(FixNodePath(data.SourceNode).ToLower()))
						{
							string newPath = string.Empty;	

							newPath = destNodePath + data.SourceNode.Text + @"\" + entry.FullPath.ToLower().Replace(FixNodePath(data.SourceNode).ToLower(), string.Empty);
							if (!_fileSystem.FileExists(newPath))
								_fileSystem.WriteFile(newPath, entry.ObjectData);
						}
					}

					// Refresh the other tree.
					GetNodes(treePaths.SelectedNode);
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the ItemDrag event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ItemDragEventArgs"/> instance containing the event data.</param>
		private void treePaths_ItemDrag(object sender, ItemDragEventArgs e)
		{
			treePaths.SelectedNode = (TreeNode)e.Item;

			// Don't drag the root node.
			if (treePaths.SelectedNode.Text != @"\")
				treePaths.DoDragDrop(new DragDropData(_fileSystem,treePaths.SelectedNode), DragDropEffects.All);
		}
	}
}