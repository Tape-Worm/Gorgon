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
		private IDictionary<string, string> _fileTypes = null;		// File types.
		private Tuple<ColumnHeader, bool> _selectedHeader = null;	// Selected header.
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

		/// <summary>
		/// Porperty to set or return the list of file extensions to filter.
		/// </summary>
		internal GorgonFileExtensionCollection FileExtensions
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyDown event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void listFiles_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Back) && (buttonUp.Enabled))
				buttonUp.PerformClick();
		}

		/// <summary>
		/// Handles the Click event of the buttonRefresh control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonRefresh_Click(object sender, EventArgs e)
		{
			RefreshCurrentDirectory();
		}

		/// <summary>
		/// Handles the Click event of the buttonUp control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonUp_Click(object sender, EventArgs e)
		{
			try
			{
				treeDirectories.SelectedNode = treeDirectories.SelectedNode.Parent;
				if (treeDirectories.SelectedNode != treeDirectories.Nodes[0])
					treeDirectories.SelectedNode.Collapse();
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
		/// Handles the DoubleClick event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void listFiles_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				Point clientPosition = listFiles.PointToClient(Cursor.Position);

				ListViewHitTestInfo info = listFiles.HitTest(clientPosition);

				if ((info == null) || (info.Item == null))
					return;

				GorgonFileSystemDirectory directory = info.Item.Tag as GorgonFileSystemDirectory;

				if (directory != null)
				{
					// Find the tree node and select it.
					if ((treeDirectories.SelectedNode.Nodes.Count > 0) && (!treeDirectories.SelectedNode.IsExpanded))
						treeDirectories.SelectedNode.Expand();

					var node = (from treeNode in treeDirectories.SelectedNode.Nodes.Cast<TreeNode>()
								where treeNode.Tag == directory
								select treeNode).SingleOrDefault();

					if (node != null)
						treeDirectories.SelectedNode = node;
					return;
				}
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
		/// Handles the BeforeSelect event of the treeDirectories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeDirectories_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Node == null)
				e.Cancel = true;
		}

		/// <summary>
		/// Handles the AfterSelect event of the treeDirectories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
		private void treeDirectories_AfterSelect(object sender, TreeViewEventArgs e)
		{
			RefreshCurrentDirectory();
		}

		/// <summary>
		/// Handles the Click event of the itemDetails control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemDetails_Click(object sender, EventArgs e)
		{
			if (sender == itemDetails)
			{
				if (!itemDetails.Checked)
				{
					itemDetails.Checked = true;
					return;
				}

				listFiles.View = View.Details;
				itemList.Checked = false;
				itemIcons.Checked = false;
			}
			if (sender == itemList)
			{
				if (!itemList.Checked)
				{
					itemList.Checked = true;
					return;
				}
				listFiles.View = View.List;
				itemDetails.Checked = false;
				itemIcons.Checked = false;
			}
			if (sender == itemIcons)
			{
				if (!itemIcons.Checked)
				{
					itemIcons.Checked = true;
					return;
				}

				listFiles.View = View.LargeIcon;
				itemDetails.Checked = false;
				itemList.Checked = false;
			}

			RefreshCurrentDirectory();
		}

		/// <summary>
		/// Handles the BeforeCollapse event of the treeDirectories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeDirectories_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				GorgonFileSystemDirectory dir = e.Node.Tag as GorgonFileSystemDirectory;

				if ((dir == null) || (dir == FileSystem.RootDirectory))
				{
					e.Cancel = true;
					return;
				}

				e.Node.ImageIndex = e.Node.SelectedImageIndex = 0;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the BeforeExpand event of the treeDirectories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeDirectories_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (e.Node.Nodes.Count == 0)
				{
					e.Cancel = true;
					return;
				}

				GetDirectories(e.Node);

				e.Node.ImageIndex = e.Node.SelectedImageIndex = 1;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to retrieve the file system directory list.
		/// </summary>
		/// <param name="root">Root node.</param>
		private void GetDirectories(TreeNode root)
		{
			GorgonFileSystemDirectory dir = null;

			treeDirectories.BeginUpdate();

			if (root == null)
				root = treeDirectories.Nodes[0];

			dir = root.Tag as GorgonFileSystemDirectory;
			
			root.Nodes.Clear();

			foreach (var subDir in dir.Directories)
			{
				TreeNode subDirNode = new TreeNode(subDir.Name);
				TreeNode dummy = new TreeNode("DummyNode");
				subDirNode.Name = subDir.FullPath;
				subDirNode.Tag = subDir;
				subDirNode.ImageKey = subDirNode.SelectedImageKey = "Default_Folder_Closed";

				if (subDir.Directories.Count > 0)
					subDirNode.Nodes.Add(dummy);
				
				root.Nodes.Add(subDirNode);
			}

			treeDirectories.SelectedNode = root;
			treeDirectories.EndUpdate();
		}


		/// <summary>
		/// Function to sort the files based on the header.
		/// </summary>
		/// <param name="files">Files to sort.</param>
		/// <returns>The sorted files.</returns>
		private IEnumerable<GorgonFileSystemFileEntry> GetSortedFiles(IEnumerable<GorgonFileSystemFileEntry> files)
		{
			if ((_selectedHeader.Item1 == null) || (_selectedHeader.Item1 == columnFileName))
			{
				if (_selectedHeader.Item2)
					return files.OrderBy(item => item.Name);
				else
					return files.OrderByDescending(item => item.Name);
			}

			if (_selectedHeader.Item1 == columnDate)
			{
				if (_selectedHeader.Item2)
					return files.OrderBy(item => item.CreateDate);
				else
					return files.OrderByDescending(item => item.CreateDate);
			}

			if (_selectedHeader.Item1 == columnSize)
			{
				if (_selectedHeader.Item2)
					return files.OrderBy(item => item.Size);
				else
					return files.OrderByDescending(item => item.Size);
			}

			if (_selectedHeader.Item1 == columnType)
			{
				if (_selectedHeader.Item2)
				{
					return from file in files
						   join fileType in _fileTypes on file.Extension equals fileType.Key
						   orderby fileType.Value
						   select file;
				}
				else
				{
					return from file in files
						   join fileType in _fileTypes on file.Extension equals fileType.Key
						   orderby fileType.Value descending 
						   select file;
				}
			}

			return null;
		}

		/// <summary>
		/// Function to retrieve the list of files for a directory.
		/// </summary>
		/// <param name="directory">Directory to look in.</param>
		private void GetFiles(GorgonFileSystemDirectory directory)
		{
			GorgonFileExtension extension = default(GorgonFileExtension);

			if (directory == null)
				directory = FileSystem.RootDirectory;

			listFiles.BeginUpdate();
			listFiles.Items.Clear();

			IEnumerable<GorgonFileSystemDirectory> sortedDirs = null;

			if ((_selectedHeader.Item1 == columnFileName) && (!_selectedHeader.Item2))
				sortedDirs = directory.Directories.OrderByDescending(item => item.Name);
			else
				sortedDirs = directory.Directories.OrderBy(item => item.Name);

			foreach (var subDir in sortedDirs)
			{
				ListViewItem directoryItem = new ListViewItem();
				directoryItem.Text = subDir.Name;
				directoryItem.Tag = subDir;
				directoryItem.Name = subDir.FullPath;
				directoryItem.ImageKey = "Default_Folder_Closed";
				directoryItem.SubItems.Add("");
				directoryItem.SubItems.Add("");
				directoryItem.SubItems.Add("Directory");
				listFiles.Items.Add(directoryItem);
			}

			// Filter extensions.
			IList<GorgonFileSystemFileEntry> fileList = null;

			extension = (GorgonFileExtension)comboExtension.SelectedItem;

			foreach (var fileExtension in FileExtensions)
			{
				foreach (var mask in fileExtension.Extension)
				{
					if ((mask != "*.*") && (mask != "*"))
						fileList = new List<GorgonFileSystemFileEntry>(FileSystem.FindFiles(directory.FullPath, mask, false));
					else
						fileList = new List<GorgonFileSystemFileEntry>(directory.Files);
				}
			}

			foreach (var file in GetSortedFiles(fileList.Distinct()))
			{
				ListViewItem fileItem = new ListViewItem();
				fileItem.Text = file.Name;
				fileItem.Tag = file;
				fileItem.Name = file.FullPath;
				fileItem.ImageKey = "Unknown_File";
				fileItem.SubItems.Add(file.Size.FormatMemory());
				fileItem.SubItems.Add(file.CreateDate.ToString());

				if (!_fileTypes.ContainsKey(file.Extension))
				{
					string fileType = Win32API.GetFileType(file.Extension);

					if (!string.IsNullOrEmpty(fileType))
						_fileTypes.Add(file.Extension, fileType);
					else
						_fileTypes.Add(file.Extension, file.Extension + " file");							
				}

				fileItem.SubItems.Add(_fileTypes[file.Extension]);

				if (!imagesTree.Images.ContainsKey(file.Extension))
				{
					imagesTree.Images.Add(file.Extension, Win32API.GetFileIcon(file.Extension, false));
					imagesLargeIcons.Images.Add(file.Extension, Win32API.GetFileIcon(file.Extension, true));
				}
				fileItem.ImageKey = file.Extension;
				listFiles.Items.Add(fileItem);
			}

			listFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			listFiles.EndUpdate();
		}

		/// <summary>
		/// Function to refresh the current directory.
		/// </summary>
		private void RefreshCurrentDirectory()
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				GetFiles(treeDirectories.SelectedNode.Tag as GorgonFileSystemDirectory);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateButtons();
			}
		}

		/// <summary>
		/// Function to validate the buttons on the form.
		/// </summary>
		private void ValidateButtons()
		{			
			this.buttonOpen.Enabled = !string.IsNullOrEmpty(textFileName.Text);
			this.buttonUp.Enabled = treeDirectories.SelectedNode.Parent != null;
		}

		/// <summary>
		/// Function to initialize the directory tree.
		/// </summary>
		private void InitializeTree()
		{
			TreeNode root = new TreeNode(FileSystem.RootDirectory.Name);
			root.Name = FileSystem.RootDirectory.FullPath;
			root.Tag = FileSystem.RootDirectory;
			
			treeDirectories.BeginUpdate();
			treeDirectories.Nodes.Clear();
			treeDirectories.Nodes.Add(root);
			treeDirectories.EndUpdate();

			GetDirectories(null);

			root.Expand();
			root.SelectedImageKey = root.ImageKey = "Default_Folder_Open";
		}

		/// <summary>
		/// Function to retrieve shell icons.
		/// </summary>
		private void GetShellIcons()
		{
			imagesTree.Images.Add("Default_Folder_Closed", Win32API.GetFolderIcon(false, false));
			imagesTree.Images.Add("Default_Folder_Open", Win32API.GetFolderIcon(true, false));
			imagesLargeIcons.Images.Add("Default_Folder_Closed", Win32API.GetFolderIcon(false, true));
		}

		/// <summary>
		/// Function to fill the extension list.
		/// </summary>
		private void FillExtensions()
		{
			comboExtension.Items.Clear();

			if ((FileExtensions != null) && (FileExtensions.Count > 0))
			{
				foreach (var extension in FileExtensions.OrderBy(item => item.Description))
					comboExtension.Items.Add(extension);

				comboExtension.SelectedIndex = 0;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				FillExtensions();
				GetShellIcons();
				InitializeTree();
				GetFiles(null);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);

				if (!IsDisposed)
					Close();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateButtons();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formGorgonFileDialog"/> class.
		/// </summary>
		public formGorgonFileDialog()
		{
			InitializeComponent();

			_fileTypes = new Dictionary<string, string>();
			FileExtensions = new GorgonFileExtensionCollection();
			FileExtensions.Add(new GorgonFileExtension("*.*", "All Files"));
			_selectedHeader = new Tuple<ColumnHeader,bool>(columnFileName, true);
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
				menuFile.Visible = false;
				treeDirectories.LabelEdit = false;
				listFiles.LabelEdit = false;
			}
			else
			{
				buttonOpen.Text = "Save";
				Text = "Save file...";
				menuFile.Visible = true;
				treeDirectories.LabelEdit = true;
				listFiles.LabelEdit = true;
			}
		}
		#endregion

		/// <summary>
		/// Handles the ColumnClick event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ColumnClickEventArgs"/> instance containing the event data.</param>
		private void listFiles_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (e.Column < 0)
					return;

				bool direction = _selectedHeader.Item2;

				if (_selectedHeader.Item1 == listFiles.Columns[e.Column])
					direction = !direction;
				else
					direction = true;

				_selectedHeader = new Tuple<ColumnHeader, bool>(listFiles.Columns[e.Column], direction);
				GetFiles(treeDirectories.SelectedNode.Tag as GorgonFileSystemDirectory);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}
	}
}
