#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Sunday, April 01, 2007 3:51:26 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Drawing = System.Drawing;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;
using Dialogs;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form used to edit and display the file system.
	/// </summary>
	public partial class formFileSystemWindow : Form
	{
		#region Classes.
		/// <summary>
		/// Object to handle sorting of the file view.
		/// </summary>
		private class ListViewCompare
			: IComparer
		{
			#region Variables.
			private int _column = 0;		// Column to sort by.
			#endregion

			#region Constructor.
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="column">Column to sort by.</param>
			public ListViewCompare(int column)
			{
				_column = column;
			}
			#endregion

			#region IComparer<ListViewItem> Members
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
			/// </returns>
			public int Compare(object x, object y)
			{
				if (((ListViewItem)x).ListView.Sorting == SortOrder.Ascending)
					return String.Compare(((ListViewItem)x).SubItems[_column].Text, ((ListViewItem)y).SubItems[_column].Text);
				else
					return String.Compare(((ListViewItem)y).SubItems[_column].Text, ((ListViewItem)x).SubItems[_column].Text);
			}
			#endregion
		}
		#endregion

		#region Value Types.
		/// <summary>
		/// Value type containing tree drag and drop data.
		/// </summary>
		private struct TreeDragDropData
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
			public TreeDragDropData(FileSystem fileSystem, TreeNode node)
			{
				SourceFileSystem = fileSystem;
				SourceNode = node;
			}
			#endregion
		}

		/// <summary>
		/// Value type containing list view drag and drop data.
		/// </summary>
		private struct ListViewDragDropData
		{
			#region Variables.
			/// <summary>
			/// File system the node comes from.
			/// </summary>
			public FileSystem SourceFileSystem;
			/// <summary>
			/// List view items.
			/// </summary>
			public ListViewItem[] SourceItems;
			#endregion

			#region Constructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="fileSystem">Source file system.</param>
			/// <param name="items">Items being dragged.</param>
			public ListViewDragDropData(FileSystem fileSystem, ListViewItem[] items)
			{
				SourceFileSystem = fileSystem;
				SourceItems = items;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private FileSystem _fileSystem;										// File system.
		private string _rootPath = string.Empty;							// File system root path.		
		private formMain _parentWindow = null;								// Parent window.
		private bool _isChanged = false;									// Flag to indicate a change.
		private View _view = View.LargeIcon;								// View state.
		private ListViewCompare[] _sorters = null;							// List view sorters.
		private bool _isNew = false;										// File system is new flag.		
		private ConfirmationResult _copyWrite = ConfirmationResult.None;	// Result used by recursive copy.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the file system is new or not.
		/// </summary>
		public bool IsNew
		{
			get
			{
				return _isNew;
			}
			set
			{
				_isNew = value;
				if (value)
					_isChanged = true;
			}
		}

		/// <summary>
		/// Property to return whether or not there's been a change to a file system.
		/// </summary>
		public bool IsChanged
		{
			get
			{
				return _isChanged;
			}
			set
			{
				_isChanged = value;
				UpdateCaption();

				if (_parentWindow != null)
					_parentWindow.ValidateForm();
			}
		}

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
                if (value == null)
                    throw new ArgumentNullException("value");

                _fileSystem = value;

				_fileSystem.FileWrite += new FileSystemReadWriteHandler(fileSystem_FileWrite);
				UpdateCaption();
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
				if (value == null)
					value = string.Empty;

				_rootPath = value;

				UpdateCaption();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the tags for the file system path nodes.
		/// </summary>
		/// <param name="root">Root node collection on the tree.</param>
		private void UpdateNodeTags(TreeNodeCollection root)
		{
			FileSystemPath path = null;

			foreach (TreeNode node in root)
			{
				if (node.Nodes.Count > 0)
					UpdateNodeTags(node.Nodes);

				path = _fileSystem.GetPath(FixNodePath(node));
				node.Tag = path;
			}
		}

		/// <summary>
		/// Function to copy a list of files to a specified directory.
		/// </summary>
		/// <param name="files">List of files to copy.</param>
		/// <param name="destination">Folder to copy into.</param>
		/// <param name="copyFolders">TRUE to copy the folders, FALSE to just copy files.</param>
		/// <param name="copyFS">TRUE to copy the file system entirely, FALSE to copy selections.</param>
		private void CopyFiles(List<FileSystemFile> files, string destination, bool copyFolders, bool copyFS)
		{
			Stream stream = null;
			ConfirmationResult result = ConfirmationResult.None;

			if (string.IsNullOrEmpty(destination))
				throw new ArgumentNullException("destination");
			if (files == null)
				throw new ArgumentNullException("files");

			try
			{
				_parentWindow.InitFileImport(files.Count, true);
				foreach (FileSystemFile file in files)
				{
					byte[] fileData = _fileSystem.ReadFile(file.FullPath);
					string fileName = destination + Path.DirectorySeparatorChar.ToString();
					string directory = string.Empty;

					if ((copyFolders) || (copyFS))
					{
						string filePath = file.FullPath;
						FileSystemPath fsPath = null;
						if (!copyFS)
							fsPath = treePaths.SelectedNode.Tag as FileSystemPath;
						else
							fsPath = _fileSystem.Paths;

						if (filePath.StartsWith(fsPath.FullPath, StringComparison.CurrentCultureIgnoreCase))
							filePath = filePath.Substring(fsPath.FullPath.Length);

						fileName += filePath;
						directory = Path.GetDirectoryName(fileName);

						if (!Directory.Exists(directory))
							Directory.CreateDirectory(directory);
					}
					else
						fileName += file.Name;

					if ((File.Exists(fileName)) && ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll))
					{
						result = UI.ConfirmBox(this, "The file '" + fileName + "' already exists at this location.  Overwrite it?", true, true);
						if (result == ConfirmationResult.Cancel)
							return;
					}

					if ((result & ConfirmationResult.No) != ConfirmationResult.No)
					{
						stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
						stream.Write(fileData, 0, fileData.Length);
						stream.Close();
					}
					_parentWindow.UpdateFileImport(file.Filename);
					Application.DoEvents();
				}
			}
			finally
			{
				_parentWindow.EndFileImport();

				if (stream != null)
					stream.Dispose();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemCopyToExplorer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemCopyToExplorer_Click(object sender, EventArgs e)
		{
			List<FileSystemFile> files = new List<FileSystemFile>();

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				dialogFolders.SelectedPath = _parentWindow.LastExportDirectory;
				if (dialogFolders.ShowDialog() == DialogResult.OK)
				{
					if (sender == menuItemCopyToExplorer)
					{
						foreach (ListViewItem item in viewFiles.SelectedItems)
							files.Add(item.Tag as FileSystemFile);
					}
					else
					{
						FileSystemPath path = null;
						if (sender == menuItemCopyPathToExplorer)
							path = treePaths.SelectedNode.Tag as FileSystemPath;
						else
							path = _fileSystem.Paths;

						files.AddRange(path.GetFiles());
					}

					if (files.Count == 0)
					{
						UI.ErrorBox(this, "There are no files under the selected path that can be exported.");
						return;
					}

					CopyFiles(files, dialogFolders.SelectedPath, sender != menuItemCopyToExplorer, sender == buttonCopyToExplorer);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error copying files on to the disk.", ex);
			}
			finally
			{
				_parentWindow.LastExportDirectory = dialogFolders.SelectedPath;
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonPurge control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonPurge_Click(object sender, EventArgs e)
		{
			if (_fileSystem != null)
				_fileSystem.PurgeDeletedFiles = buttonPurge.Checked;
		}

		/// <summary>
		/// Function to check to see if a file should be overwritten.
		/// </summary>
		/// <param name="fileName">File to check.</param>
		/// <param name="lastResult">Last confirmation result.</param>
		/// <returns>A confirmation dialog result.</returns>
		private ConfirmationResult ConfirmOverwrite(string fileName, ConfirmationResult lastResult)
		{
			ConfirmationResult result = lastResult;		// Result.			

			// If we selected 'To All' then skip.
			if ((result & ConfirmationResult.ToAll) == ConfirmationResult.ToAll)
				return result;

			if (_fileSystem.FileExists(fileName))
				result = UI.ConfirmBox(this, "The file '" + fileName + "' already exists.\nDo you wish to overwrite it?", string.Empty, true, true);

			return result;
		}

		/// <summary>
		/// Handles the FileWrite event of the fileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.FileSystems.FileSystemReadWriteEventArgs"/> instance containing the event data.</param>
		private void fileSystem_FileWrite(object sender, FileSystemReadWriteEventArgs e)
		{
			// Update the progress meter.
			if (e.File.Size != 0)
				_parentWindow.UpdateFileImport(e.File.Filename + e.File.Extension);
		}

		/// <summary>
		/// Function to update the caption.
		/// </summary>
		private void UpdateCaption()
		{
            if ((_fileSystem != null) && (_fileSystem.Provider.IsEncrypted))
                buttonChangeAuth.Enabled = true;
            else
                buttonChangeAuth.Enabled = false;

			if (_fileSystem != null)
			{
				buttonPurge.Enabled = !_fileSystem.Provider.IsPackedFile;
				buttonPurge.Checked = _fileSystem.PurgeDeletedFiles;
			}
			else
			{
				buttonPurge.Checked = false;
				buttonPurge.Visible = false;
			}

			if (_fileSystem.FileCount != 0)
				buttonCopyToExplorer.Enabled = true;
			else
				buttonCopyToExplorer.Enabled = false;

			if ((_fileSystem != null) && (_rootPath != string.Empty))
				Text = _fileSystem.Provider.Description + " - " + _rootPath;
			else
				Text = "File System";

			if (_isChanged)
				Text += "*";
		}

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
			TreeNode newNode = null;			// New tree node.
			FileSystemPath pathNode = null;		// File system path node.

			try
			{
				Cursor.Current = Cursors.WaitCursor;
				treePaths.BeginUpdate();

				// If we don't specify a root node, then get the root path node.
				if (rootNode == null)
				{
					pathNode = _fileSystem.Paths;
					treePaths.Nodes.Clear();
					parentNode = treePaths.Nodes.Add(@"\", @"\");

					parentNode.Tag = pathNode;

					// Add a dummy node if we have paths under this node.
					if (pathNode.ChildPaths.Count > 0)
						parentNode.Nodes.Add("$DUMMYNODE$");
				}
				else
				{
					pathNode = _fileSystem.GetPath(rootNode.FullPath);
					rootNode.Nodes.Clear();

					parentNode = rootNode;

					// Add nodes.
					foreach (FileSystemPath path in pathNode.ChildPaths)
					{
						newNode = parentNode.Nodes.Add(path.Name.ToLower(), path.Name);
						newNode.Tag = path;

						if (path.ChildPaths.Count > 0)
							newNode.Nodes.Add("$DUMMYNODE$");
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				treePaths.EndUpdate();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to refresh the tree view.
		/// </summary>
		private void RefreshTree()
		{
			GetNodes(null);

			// Expand the first node.
			treePaths.Nodes[@"\"].Expand();
		}

		/// <summary>
		/// Function to validate the popup menu.
		/// </summary>
		private void ValidatePopup()
		{
			menuItemAddPath.Enabled = false;
			menuItemRemovePath.Enabled = false;
			menuItemAddFiles.Enabled = false;
			menuItemRemoveFiles.Enabled = false;
			menuItemEditComment.Enabled = false;
			menuItemCopyToExplorer.Enabled = false;
			menuItemCopyPathToExplorer.Enabled = false;

			if (treePaths.SelectedNode != null)
			{
				menuItemAddPath.Enabled = true;
				menuItemRemovePath.Enabled = true;
				menuItemAddFiles.Enabled = true;
				if (_fileSystem.FileCount != 0)
					menuItemCopyPathToExplorer.Enabled = true;
			}

			if (viewFiles.SelectedItems.Count > 0)
			{
				menuItemRemoveFiles.Enabled = true;
				if (_fileSystem.FileCount != 0)
					menuItemCopyToExplorer.Enabled = true;
			}

			if (viewFiles.SelectedItems.Count == 1)
				menuItemEditComment.Enabled = true;
		}

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
		/// Handles the Click event of the menuItemAddPath control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemAddPath_Click(object sender, EventArgs e)
		{
			formPathNameInput nameInput = null;		// Path name input.
			TreeNode node = null;					// Newly added node.
			FileSystemPath newPath = null;			// New file system path.
			FileSystemPath path = null;				// File system path.

			try
			{
				nameInput = new formPathNameInput();

				// Add a dummy entry to the file system.
				if (nameInput.ShowDialog(this) == DialogResult.OK)
				{
					path = treePaths.SelectedNode.Tag as FileSystemPath;
					if (!path.ChildPaths.Contains(nameInput.PathName))
						newPath = ((FileSystemPath)treePaths.SelectedNode.Tag).ChildPaths.Add(nameInput.PathName);
					else
					{
						UI.ErrorBox(this, "The path '" + nameInput.PathName + "' already exists.");
						return;
					}

					// Add the node.
					node = treePaths.SelectedNode.Nodes.Add(newPath.Name.ToLower(), newPath.Name);
					node.Tag = newPath;

					// Select the node.
					treePaths.SelectedNode = node;

					IsChanged = true;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				if (nameInput != null)
					nameInput.Dispose();
				nameInput = null;
			}
		}

		/// <summary>
		/// Function to retrieve the icon for a file.
		/// </summary>
		/// <param name="fileItem">Item in the list to alter.</param>
		/// <param name="file">File to retrieve the icon for.</param>
		private void GetFileIcon(ListViewItem fileItem, FileSystemFile file)
		{
			string extension = string.Empty;	// File extension.

			extension = file.Extension.ToLower();
			if (extension == string.Empty)
				extension = "no_extension";

			// Get icons by extension.
			if (!viewFiles.SmallImageList.Images.ContainsKey(extension))
				viewFiles.SmallImageList.Images.Add(extension, Utilities.IconFromExtension(extension, ShellIconSize.Small).ToBitmap());

			if (!viewFiles.LargeImageList.Images.ContainsKey(extension))
                viewFiles.LargeImageList.Images.Add(extension, Utilities.IconFromExtension(extension, ShellIconSize.Large).ToBitmap());

			fileItem.ImageKey = extension;
		}

		/// <summary>
		/// Function to add a file system entry to the view list.
		/// </summary>
		/// <param name="file">File to add.</param>
		private void AddFileEntry(FileSystemFile file)
		{
			ListViewItem item = null;			// List view item.
			double compressPercent = 0;			// Compression percentage.
			IComparer lastCompare;				// Last list view comparer.
			string extension = string.Empty;	// File extension.

			lastCompare = viewFiles.ListViewItemSorter;
			viewFiles.ListViewItemSorter = null;

			extension = file.Extension.ToLower();
			if (extension == string.Empty)
				extension = "no_extension";

			item = viewFiles.Items.Add(file.Filename + file.Extension, extension);
			item.Tag = file;
			GetFileIcon(item, file);

			// Change to only use the file name when in details mode.
			if (_view == View.Details)
			{
				// Add details.
				item.SubItems.Add(file.Extension);
				item.SubItems.Add(file.DateTime.ToString());
				item.SubItems.Add(Utilities.FormatByteUnits((ulong)file.Size));
                item.ForeColor = Color.Black;
                if (_fileSystem.Provider.IsEncrypted)
                {
                    item.ForeColor = Color.Green;
                    if (!viewFiles.Columns.ContainsKey("headerEncrypted"))
                        viewFiles.Columns.Add("headerEncrypted", "Encrypted?");
                    item.SubItems.Add(file.IsEncrypted.ToString());
                }

				// Add compression information.
				if (_fileSystem.Provider.IsCompressed)
				{
                    item.ForeColor = Color.Blue;
					if (!viewFiles.Columns.ContainsKey("headerCompressed"))
						viewFiles.Columns.Add("headerCompressed", "Compressed?");
					if (!viewFiles.Columns.ContainsKey("headerCompressedSize"))
						viewFiles.Columns.Add("headerCompressedSize", "Compressed size");
					if (!viewFiles.Columns.ContainsKey("headerCompressedPercent"))
						viewFiles.Columns.Add("headerCompressedPercent", "Reduction (%)");

					item.SubItems.Add(file.IsCompressed.ToString());
					if (file.IsCompressed)
					{
						compressPercent = (1.0 - ((double)file.CompressedSize / (double)file.Size)) * 100.0;
						item.SubItems.Add(Utilities.FormatByteUnits((ulong)file.CompressedSize));
						item.SubItems.Add(compressPercent.ToString("0.0") + "%");
					}
					else
					{
						item.SubItems.Add("N/A");
						item.SubItems.Add("N/A");
					}
				}

				if (file.Comment != string.Empty)
				{
					if (!viewFiles.Columns.ContainsKey("headerComment"))
						viewFiles.Columns.Add("headerComment", "Comment");

					item.SubItems.Add((file.Comment.Length > 25) ? file.Comment.Substring(0, 25) + "..." : file.Comment);
				}

				viewFiles.ListViewItemSorter = lastCompare;
			}
		}

		/// <summary>
		/// Handles the AfterSelect event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
		private void treePaths_AfterSelect(object sender, TreeViewEventArgs e)
		{
			// Refresh the view.
			GetFiles(treePaths.SelectedNode);
			if (e.Node != null)
				e.Node.EndEdit(true);
		}

		/// <summary>
		/// Function to fill in the file view for a given node.
		/// </summary>
		/// <param name="node">Node to get files for.</param>
		private void GetFiles(TreeNode node)
		{
			FileSystemPath path = null;		// File system path.

			try
			{
				Cursor.Current = Cursors.WaitCursor;


				if (_fileSystem == null)
					return;

				// Select the first node if we pass in null.
				if (node == null)
					node = treePaths.Nodes[@"\"];

				viewFiles.Items.Clear();
				viewFiles.BeginUpdate();

				// Get the path.
				path = (FileSystemPath)node.Tag;

				// Remove comment column.
				if (viewFiles.Columns.ContainsKey("headerComment"))
					viewFiles.Columns.RemoveByKey("headerComment");

				// Refresh the right hand view.
				foreach (FileSystemFile file in path.Files)
					AddFileEntry(file);

				// Retrieve file properties.
				GetFileProperties();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				// Resize to view.
				viewFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				viewFiles.EndUpdate();
				Cursor.Current = Cursors.Default;
			}
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
		/// Function to copy a path, its files and all child paths.
		/// </summary>
		/// <param name="sourceFS">Source file system.</param>
		/// <param name="destFS">Destination file system.</param>
		/// <param name="source">Path to copy.</param>
		/// <param name="destination">Destination for the files.</param>
		/// <param name="topLevel">TRUE if we're at the top level of the recursion stack, FALSE if not.</param>
		/// <returns>TRUE to continue, FALSE to cancel.</returns>
		private bool CopyPaths(FileSystem sourceFS, FileSystem destFS, FileSystemPath source, FileSystemPath destination, bool topLevel)
		{
			FileSystemPath newPath = null;						// New path.

			// Stupid limitation, but it'll do for now.
			if ((source.Name == @"\") && (destination.Name != @"\"))
			{
				UI.ErrorBox(this, "Cannot copy the root into a sub path.");
				return false;
			}

			if (((destination.Name != source.Name) || (destination.Name != @"\")) && (topLevel))
			{
				if (!destination.ChildPaths.Contains(source.Name))
					destination = destination.ChildPaths.Add(source.Name);
				else
					destination = destination.ChildPaths[source.Name];
			}

			// Copy children.
			foreach (FileSystemPath child in source.ChildPaths)
			{
				if (!destination.ChildPaths.Contains(child.Name))
					newPath = destination.ChildPaths.Add(child.Name);
				else
					newPath = destination.ChildPaths[child.Name];

				if (!CopyPaths(sourceFS, destFS, child, newPath, false))
					return false;
			}

			// Copy files.
			foreach (FileSystemFile file in source.Files)
			{
				_copyWrite = ConfirmOverwrite(destination.FullPath + file.Filename + file.Extension, _copyWrite);

				if (_copyWrite == ConfirmationResult.Cancel)
					return false;

				if ((_copyWrite & ConfirmationResult.No) == 0)
					destFS.WriteFile(destination.FullPath + file.Filename + file.Extension, sourceFS.ReadFile(file.FullPath));

				Application.DoEvents();
			}

			return true;
		}

		/// <summary>
		/// Function to move a path to a new path.
		/// </summary>
		/// <param name="source">Source path/name.</param>
		/// <param name="destination">Destination path/name.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		private FileSystemPath MovePath(string source, string destination)
		{
			FileSystemPath path = null;								// Path to move.
			FileSystemPath destPath = null;							// Destination path.
			FileSystemPath oldParent = null;						// Previous parent.
			FileSystemPath newPath = null;							// New path copy.

			// Get source.
			source = FileSystem.FullPathName(source);

			if (!FileSystemPath.ValidPath(source))
                throw new IOException("The source path contains invalid characters.");

			destination = FileSystem.FullPathName(destination);

			if (!FileSystemPath.ValidPath(destination))
				throw new IOException("The destination path contains invalid characters.");

			// Don't allow us to move into the same node.
			if (source.ToLower() == destination.ToLower())
				throw new IOException("Cannot move the entries.  The source and destination path are the same.");

			// Move the node.
			path = _fileSystem.GetPath(source);
			oldParent = path.Parent;

			if (!_fileSystem.PathExists(destination))
				_fileSystem.CreatePath(destination);

			// Get the destination path.
			destPath = _fileSystem.GetPath(destination);

			// Add the path to the destination.
			if (!destPath.ChildPaths.Contains(path.Name))
			{
				newPath = path.Copy(path.Name);
				destPath.ChildPaths.Add(newPath);
			}
			else
			{
				UI.ErrorBox(this, "The path '" + path.Name + "' already exists at the destination.");
				return null;
			}

			// Remove from the source parent.
			_fileSystem.DeletePath(path.FullPath);

			IsChanged = true;

			return newPath;
		}

		/// <summary>
		/// Function to rename a path.
		/// </summary>
		/// <param name="oldName">Old name of the path.</param>
		/// <param name="newName">New name of the path.</param>
		/// <returns>The full path of the renamed node.</returns>
		private FileSystemPath RenamePath(string oldName, string newName)
		{
			FileSystemPath path = null;								// Path to move.
			string newPathName = newName;							// New path name.

			// Get source.
			oldName = FileSystem.FullPathName(oldName);

			if (!FileSystemPath.ValidPath(oldName))
				throw new IOException("The source path contains invalid characters.");

			newName = FileSystem.FullPathName(newName);

			if (!FileSystemPath.ValidPath(newName))
                throw new IOException("The destination path contains invalid characters.");

			// Don't allow us to move into the same node.
			if (newName.ToLower() == oldName.ToLower())
				return _fileSystem.GetPath(oldName);

			// Ensure the path doesn't already exist.
			if (_fileSystem.PathExists(newName))
                throw new IOException("There is already a path with the name '" + newName + "'.");

			// Move the node.				
			path = _fileSystem.GetPath(oldName);
			// Rename the path.
			FileSystemPath parentPath = path.Parent;
			FileSystemPath pathCopy = path.Copy(newPathName);
			path.Parent.ChildPaths.Add(pathCopy);
			_fileSystem.DeletePath(oldName);			

			IsChanged = true;

			return pathCopy;
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
			if ((!FileSystemPath.ValidPath(e.Label)) || (!FileSystemFile.ValidFilename(e.Label)))
			{
				string invalidChars = string.Empty;	// Invalid characters.

				// Get the list of invalid file characters.
				foreach (char c in FileSystemPath.InvalidCharacters)
				{
					if (c > 32)
					{
						if (invalidChars != string.Empty)
							invalidChars += ", ";
						invalidChars += "'" + c.ToString() + "'";
					}
				}

				invalidChars += ", '\\', '/'";

				UI.ErrorBox(this, "The new filename contains invalid characters.\nThe characters:\n" + invalidChars + "\nAre not allowed.");
				e.CancelEdit = true;
				return;
			}

			// There is the possibility that we can miss a slash.  In renaming a path node, using a slash would cause an invalid filename exception.
			if (e.Label.Replace("/", @"\").IndexOf(@"\") > -1)
			{
				UI.ErrorBox(this, "The new filename contains invalid characters.\n'/' or '\\' are not allowed.");
				e.CancelEdit = true;
				return;
			}

			try
			{
				// Move the node.
				e.Node.Tag = RenamePath(((FileSystemPath)e.Node.Tag).FullPath, e.Label);

				// Turn off editing.
				treePaths.LabelEdit = false;

				IsChanged = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
				e.CancelEdit = true;
			}
			finally
			{
				_fileSystem.FileWrite -= new FileSystemReadWriteHandler(fileSystem_FileWrite);
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

			// Remove event.
			_fileSystem.FileWrite -= new FileSystemReadWriteHandler(fileSystem_FileWrite);
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
					e.Handled = true;
					return;
				}

				// Delete a path.
				if (e.KeyCode == Keys.Delete)
				{
					menuItemRemovePath_Click(this, EventArgs.Empty);
					e.Handled = true;
				}
			}
		}

		/// <summary>
		/// Handles the DragOver event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void treePaths_DragOver(object sender, DragEventArgs e)
		{
			TreeDragDropData data;			// Data from drag and drop operation.
			TreeNode hitNode = null;		// Hit test node.	
			TreeNode searchNode = null;		// Search node.

			// Default to no action.
			e.Effect = DragDropEffects.None;

			// Destination node.
			hitNode = treePaths.HitTest(treePaths.PointToClient(new Point(e.X, e.Y))).Node;

			// Anything from explorer will be added, we will deal with validation upon drop.
			if ((e.Data.GetDataPresent(DataFormats.FileDrop)) && (hitNode != null))
			{
				e.Effect = DragDropEffects.Copy;
				treePaths.SelectedNode = hitNode;
				return;
			}

			// Handle tree drag data.
			if (e.Data.GetDataPresent(typeof(TreeDragDropData)))
			{
				data = (TreeDragDropData)e.Data.GetData(typeof(TreeDragDropData));

				// If we're dragging the root, then ensure we only drag to another file system.
				if ((data.SourceNode.Text == @"\") && (data.SourceFileSystem == _fileSystem))
					return;

				if ((hitNode != null) && (data.SourceNode != hitNode))
				{
					// Find parent.
					if (data.SourceFileSystem == _fileSystem)
					{
						searchNode = hitNode.Parent;
						while ((searchNode != data.SourceNode) && (searchNode != null))
							searchNode = searchNode.Parent;
					}
					else
						searchNode = null;

					if (searchNode == null)
					{
						e.Effect = DragDropEffects.All;
						treePaths.SelectedNode = hitNode;
						hitNode.Expand();
					}
				}
			}

			// Handle list view drag data.
			if ((e.Data.GetDataPresent(typeof(ListViewDragDropData))) && (hitNode != null))
			{
				e.Effect = DragDropEffects.All;
				treePaths.SelectedNode = hitNode;
				hitNode.Expand();
			}
		}

		/// <summary>
		/// Function to retrieve the list of files under a folder in explorer.
		/// </summary>
		/// <param name="info">Directory to start with.</param>
		/// <returns>The list of files.</returns>
		private List<string> GetExplorerFiles(DirectoryInfo info)
		{
			List<string> result = new List<string>();		// File list.
			DirectoryInfo[] dirs = null;					// Directory list.
			FileInfo[] files = null;						// File list.

			// Get the directories and files.
			dirs = info.GetDirectories();
			files = info.GetFiles();

			// Process sub directories.
			foreach (DirectoryInfo dir in dirs)
			{
				List<string> subFiles = null;		// Sub directory file list.
				subFiles = GetExplorerFiles(dir);

				// Add files to our list.
				foreach (string file in subFiles)
					result.Add(file);
			}

			// Add file information.
			foreach (FileInfo file in files)
				result.Add(file.FullName);

			return result;
		}

		/// <summary>
		/// Function to add to the selected node.
		/// </summary>
		/// <param name="node">Node to place the data under.</param>
		/// <param name="fileList">List of files to add.</param>
		private void AddToTreeFromExplorer(TreeNode node, string[] fileList)
		{
			byte[] fileData = null;								// File data.
			Stream stream = null;								// File stream.
			ConfirmationResult write = ConfirmationResult.Yes;	// Write flag.
			DirectoryInfo info = null;							// File information.
			List<string> files = null;							// List of files.
			string parentDirectory = string.Empty;				// Parent directory.
			string relativeName = string.Empty;					// Relative file name.

			try
			{
				Cursor.Current = Cursors.WaitCursor;

				if (node == null)
					node = treePaths.Nodes[@"\"];

				// Create file list.
				files = new List<string>();

				// Begin adding files.
				foreach (string fileName in fileList)
				{
					// Get the parent directory info.
					if (parentDirectory == string.Empty)
						parentDirectory = Path.GetDirectoryName(fileName) + @"\";

					info = new DirectoryInfo(fileName);

					// Extract the contents of the directory and its sub directories.
					if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
					{
						List<string> dirFiles = null;		// Directory files.

						dirFiles = GetExplorerFiles(info);

						// Add files to the result list.
						foreach (string file in dirFiles)
						{
							relativeName = FixNodePath(node) + file.Substring(parentDirectory.Length);

							if (_fileSystem.FileExists(relativeName))
							{
								// Ask again.
								if ((write & ConfirmationResult.ToAll) == 0)
									write = UI.ConfirmBox(this, "The file '" + relativeName + "' already exists.\nDo you wish to overwrite it?", string.Empty, true, true);

								if ((write & ConfirmationResult.Yes) == ConfirmationResult.Yes)
									files.Add(file);

								if (write == ConfirmationResult.Cancel)
									return;
							}
							else
								files.Add(file);

						}
					}
					else
					{
						relativeName = FixNodePath(node) + fileName.Substring(parentDirectory.Length);

						if (_fileSystem.FileExists(relativeName))
						{
							// Ask again.
							if ((write & ConfirmationResult.ToAll) == 0)
								write = UI.ConfirmBox(this, "The file '" + relativeName + "' already exists.\nDo you wish to overwrite it?", string.Empty, true, true);

							if ((write & ConfirmationResult.Yes) == ConfirmationResult.Yes)
								files.Add(fileName);

							if (write == ConfirmationResult.Cancel)
								return;
						}
						else
							files.Add(fileName);
					}
				}

				_parentWindow.InitFileImport(files.Count, false);

				// Begin adding files.
				foreach (string fileName in files)
				{
					relativeName = fileName;		// Relative file name.

					// Open the file and get its data.
					relativeName = FixNodePath(node) + fileName.Substring(parentDirectory.Length);
					stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
					fileData = new byte[stream.Length];
					stream.Read(fileData, 0, fileData.Length);

					// Close.
					if (stream != null)
						stream.Dispose();
					stream = null;

					_fileSystem.WriteFile(relativeName, fileData);
					Application.DoEvents();
				}

				// Set the changed flag.
				GetNodes(node);
				GetFiles(node);
				IsChanged = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				_parentWindow.EndFileImport();

				Cursor.Current = Cursors.Default;
				if (stream != null)
					stream.Dispose();

				stream = null;
			}
		}

		/// <summary>
		/// Handles the DragDrop event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void treePaths_DragDrop(object sender, DragEventArgs e)
		{
			string destNodePath = string.Empty;					// Destination node path.			
			TreeDragDropData data;								// Drag and drop data.
			ConfirmationResult write = ConfirmationResult.None;	// Confirmation result.
			FileSystemPath path = null;							// File system path.
			FileSystemPath sourcePath = null;					// Source path.

			if (treePaths.SelectedNode == null)
				return;

			try
			{
				// If we're dragging from explorer, add the file(s).
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					AddToTreeFromExplorer(treePaths.SelectedNode, (string[])e.Data.GetData(DataFormats.FileDrop));
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				// Get destination node path.
				path = (FileSystemPath)treePaths.SelectedNode.Tag;

				if (e.Data.GetDataPresent(typeof(TreeDragDropData)))
				{
					// Get the drag and drop data.
					data = (TreeDragDropData)e.Data.GetData(typeof(TreeDragDropData));

					// Get the source.
					sourcePath = (FileSystemPath)data.SourceNode.Tag;

					// If we're within the same file system, move the nodes.
					if (data.SourceFileSystem == _fileSystem)
					{
						// Move the data.
						FileSystemPath newPath = MovePath(sourcePath.FullPath, path.FullPath);
						if (newPath == null)
							return;

						// Remove from the node list.
						data.SourceNode.Remove();

						if ((!data.SourceNode.IsExpanded) && (sourcePath.ChildPaths.Count > 0))
							data.SourceNode.Nodes.Add("$DUMMYNODE$");

						// Re-add to new parent.
						data.SourceNode.Tag = newPath;
						treePaths.SelectedNode.Nodes.Add(data.SourceNode);
					}
					else
					{
						// Copy the paths.
						_copyWrite = ConfirmationResult.None;
						_parentWindow.InitFileImport(sourcePath.GetFiles().Count, false);
						CopyPaths(data.SourceFileSystem, _fileSystem, sourcePath, path, true);

						// Refresh the other tree.
						GetNodes(treePaths.SelectedNode);
						IsChanged = _isChanged;
					}
				}

				// Handle list view drag data.
				if (e.Data.GetDataPresent(typeof(ListViewDragDropData)))
				{
					FileSystemFile file = null;			// File.
					ListViewDragDropData dragData;		// List view drag data.

					dragData = (ListViewDragDropData)e.Data.GetData(typeof(ListViewDragDropData));

					_parentWindow.InitFileImport(dragData.SourceItems.Length, false);

					// Copy the items to the path in the destination file system.
					foreach (ListViewItem item in dragData.SourceItems)
					{
						file = (FileSystemFile)item.Tag;

						// Get the original path.
						sourcePath = file.Owner;

						write = ConfirmOverwrite(path.FullPath + file.Filename + file.Extension, write);

						if (write == ConfirmationResult.Cancel)
							return;

						if ((write & ConfirmationResult.No) == 0)
						{
							// Copy between file systems, otherwise if the same FS, then move.							
							if (dragData.SourceFileSystem == _fileSystem)
							{
								_fileSystem.WriteFile(path.FullPath + file.Filename + file.Extension, _fileSystem.ReadFile(file.FullPath));
								_fileSystem.Delete(file.FullPath);
							}
							else
								_fileSystem.WriteFile(path.FullPath + file.Filename + file.Extension, dragData.SourceFileSystem.ReadFile(file.FullPath));

							_isChanged = true;
						}

						Application.DoEvents();
					}

					IsChanged = IsChanged;
				}

				// Get the files.
				GetFiles(treePaths.SelectedNode);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				_parentWindow.EndFileImport();
				Cursor.Current = Cursors.Default;
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
			treePaths.DoDragDrop(new TreeDragDropData(_fileSystem, treePaths.SelectedNode), DragDropEffects.All);
		}

		/// <summary>
		/// Handles the Click event of the menuItemRemovePath control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemRemovePath_Click(object sender, EventArgs e)
		{
			TreeNode nextNode = null;		// Node to select next.
			try
			{
				if (treePaths.SelectedNode == null)
					return;

				if (treePaths.SelectedNode.Text != @"\")
				{
					// Confirm.
					if (UI.ConfirmBox(this, "You are about to delete '" + FixNodePath(treePaths.SelectedNode) + "' and all objects underneath it.\nAre you sure you wish to do this?") != ConfirmationResult.Yes)
						return;
				}
				else
				{
					// Confirm.
					if (UI.ConfirmBox(this, "You are about to delete all the files in the file system.\nAre you sure you wish to do this?") != ConfirmationResult.Yes)
						return;
				}

				// Destroy the node.
				_fileSystem.DeletePath(treePaths.SelectedNode.FullPath);

				// Select the next node.
				nextNode = treePaths.SelectedNode.NextNode;

				// No next node?  Select the previous.
				if ((treePaths.SelectedNode.PrevNode != null) && (nextNode == null))
					nextNode = treePaths.SelectedNode.PrevNode;

				// Still no node, select its parent.
				if ((treePaths.SelectedNode.Parent != null) && (nextNode == null))
					nextNode = treePaths.SelectedNode.Parent;

				// Remove the node if it's not the root.
				if (treePaths.SelectedNode.Text != @"\")
				{
					treePaths.SelectedNode.Remove();

					// Select the next node.
					treePaths.SelectedNode = nextNode;
				}
				else
					treePaths.SelectedNode.Nodes.Clear();

				// Get the files.
				GetFiles(treePaths.SelectedNode);

				IsChanged = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the DragEnter event of the treePaths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void treePaths_DragEnter(object sender, DragEventArgs e)
		{
			// Bring a file from explorer.
			try
			{
				if ((e.Data.GetDataPresent(DataFormats.FileDrop, false)) || (e.Data.GetDataPresent(typeof(ListViewDragDropData))))
					e.Effect = DragDropEffects.All;
				else
					e.Effect = DragDropEffects.None;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to set the viewing state for the icons.
		/// </summary>
		/// <param name="viewState">View state to set.</param>
		private void SetViewState(View viewState)
		{
			menuItemDetails.Checked = false;
			menuItemSmallIcons.Checked = false;
			menuItemLargeIcons.Checked = false;
			menuItemList.Checked = false;

			// Determine which item is checked.
			_view = viewState;
			viewFiles.View = _view;
			switch (_view)
			{
				case View.Details:
					menuItemDetails.Checked = true;
					break;
				case View.LargeIcon:
					menuItemLargeIcons.Checked = true;
					break;
				case View.SmallIcon:
					menuItemSmallIcons.Checked = true;
					break;
				case View.List:
					menuItemList.Checked = true;
					break;
			}

			_parentWindow.LastView = _view.ToString();

			// Refresh the file list.
			GetFiles(treePaths.SelectedNode);
		}

		/// <summary>
		/// Handles the Click event of the menuItemSmallIcons control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSmallIcons_Click(object sender, EventArgs e)
		{
			SetViewState(View.SmallIcon);
		}

		/// <summary>
		/// Handles the Click event of the menuItemLargeIcons control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemLargeIcons_Click(object sender, EventArgs e)
		{
			SetViewState(View.LargeIcon);
		}

		/// <summary>
		/// Handles the Click event of the menuItemList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemList_Click(object sender, EventArgs e)
		{
			SetViewState(View.List);
		}

		/// <summary>
		/// Handles the Click event of the menuItemDetails control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemDetails_Click(object sender, EventArgs e)
		{
			SetViewState(View.Details);
		}

		/// <summary>
		/// Handles the ColumnClick event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ColumnClickEventArgs"/> instance containing the event data.</param>
		private void viewFiles_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			// Same column?  Change order.
			if (_sorters[e.Column] != viewFiles.ListViewItemSorter)
				viewFiles.ListViewItemSorter = _sorters[e.Column];

			if (viewFiles.Sorting == SortOrder.Ascending)
				viewFiles.Sorting = SortOrder.Descending;
			else
				viewFiles.Sorting = SortOrder.Ascending;

			viewFiles.Sort();
		}

		/// <summary>
		/// Handles the DragEnter event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void viewFiles_DragEnter(object sender, DragEventArgs e)
		{
			// Bring a file from explorer.
			try
			{
				if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
					e.Effect = DragDropEffects.All;
				else
					e.Effect = DragDropEffects.None;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the DragDrop event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void viewFiles_DragDrop(object sender, DragEventArgs e)
		{
			ListViewDragDropData dragData;						// List view drag & drop data.
			ConfirmationResult write = ConfirmationResult.None;	// Confirmation dialog result.

			if (treePaths.SelectedNode == null)
				return;

			try
			{
				// If we're dragging from explorer, add the file(s).
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					AddToTreeFromExplorer(treePaths.SelectedNode, (string[])e.Data.GetData(DataFormats.FileDrop));
					return;
				}

				// Handle transferring between two file systems.
				if (e.Data.GetDataPresent(typeof(ListViewDragDropData)))
				{
					dragData = (ListViewDragDropData)e.Data.GetData(typeof(ListViewDragDropData));

					_parentWindow.InitFileImport(dragData.SourceItems.Length, false);

					// Copy the items to the path in the destination file system.
					foreach (ListViewItem item in dragData.SourceItems)
					{
						FileSystemFile file = null;				// File.

						file = (FileSystemFile)item.Tag;

						write = ConfirmOverwrite(((FileSystemPath)treePaths.SelectedNode.Tag).FullPath + file.Filename + file.Extension, write);

						if (write == ConfirmationResult.Cancel)
							return;

						if ((write & ConfirmationResult.No) == 0)
						{
							if (_fileSystem != dragData.SourceFileSystem)
								_fileSystem.WriteFile(((FileSystemPath)treePaths.SelectedNode.Tag).FullPath + file.Filename + file.Extension, dragData.SourceFileSystem.ReadFile(file.FullPath));
							else
								_fileSystem.WriteFile(((FileSystemPath)treePaths.SelectedNode.Tag).FullPath + file.Filename + file.Extension, _fileSystem.ReadFile(file.FullPath));
							_isChanged = true;
						}

						Application.DoEvents();
					}

					GetFiles(treePaths.SelectedNode);
					IsChanged = _isChanged;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				_parentWindow.EndFileImport();
			}
		}

		/// <summary>
		/// Handles the ItemDrag event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ItemDragEventArgs"/> instance containing the event data.</param>
		private void viewFiles_ItemDrag(object sender, ItemDragEventArgs e)
		{
			ListViewItem[] items = null;		// Selected list view items.

			// Get selected items.
			items = new ListViewItem[viewFiles.SelectedItems.Count];

			for (int i = 0; i < viewFiles.SelectedItems.Count; i++)
				items[i] = viewFiles.SelectedItems[i];

			// Don't drag the root node.
			treePaths.DoDragDrop(new ListViewDragDropData(_fileSystem, items), DragDropEffects.All);
		}

		/// <summary>
		/// Handles the DragOver event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
		private void viewFiles_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.All;

			if (e.Data.GetDataPresent(typeof(ListViewDragDropData)))
			{
				if (((ListViewDragDropData)e.Data.GetData(typeof(ListViewDragDropData))).SourceFileSystem != _fileSystem)
					e.Effect = DragDropEffects.All;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemAddFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemAddFiles_Click(object sender, EventArgs e)
		{
			try
			{
				dialogAddFile.InitialDirectory = _parentWindow.LastOpenPath;

				// Begin add file process.
				if (dialogAddFile.ShowDialog(this) == DialogResult.OK)
				{
					AddFiles(treePaths.SelectedNode, dialogAddFile.FileNames);

					// Store the last used path.
					_parentWindow.LastOpenPath = Path.GetDirectoryName(dialogAddFile.FileNames[0]);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to add the files to the file system from the disk.
		/// </summary>
		/// <param name="nodePath">Path to place the files under.</param>
		/// <param name="files">List of files to add.</param>
		private void AddFiles(TreeNode nodePath, string[] files)
		{
			string newPath = FixNodePath(nodePath);					// New path.
			string newFileName = string.Empty;						// File name.
			byte[] fileData = null;									// File data.
			BinaryReaderEx reader = null;							// Binary reader.
			ConfirmationResult write = ConfirmationResult.None;		// Confirmation dialog result.

			try
			{
				_parentWindow.InitFileImport(files.Length, false);

				// Load the files.
				foreach (string file in files)
				{
					// Read in the file.
					reader = new BinaryReaderEx(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read), false);

					fileData = reader.ReadBytes((int)reader.BaseStream.Length);

					reader.Close();
					reader = null;

					// Write to the file system.
					newFileName = newPath + Path.GetFileName(file);
					if (_fileSystem.FileExists(newFileName))
					{
						if ((write & ConfirmationResult.ToAll) == 0)
							write = UI.ConfirmBox(this, "The file '" + newFileName + "' already exists.  Overwrite it?", string.Empty, true, true);

						if (write == ConfirmationResult.Cancel)
							return;

						if ((write & ConfirmationResult.Yes) == ConfirmationResult.Yes)
							_fileSystem.WriteFile(newFileName, fileData);
					}
					else
						_fileSystem.WriteFile(newFileName, fileData);

					Application.DoEvents();
				}

				// Refresh the view.
				GetFiles(nodePath);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (reader != null)
					reader.Close();

				reader = null;
				_parentWindow.EndFileImport();
			}
		}

		/// <summary>
		/// Handles the MouseDown event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void viewFiles_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (viewFiles.HitTest(e.X, e.Y).Item != null)
					viewFiles.HitTest(e.X, e.Y).Item.Selected = true;
				else
					viewFiles.SelectedItems.Clear();
			}

			ValidatePopup();
		}

		/// <summary>
		/// Handles the AfterLabelEdit event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.LabelEditEventArgs"/> instance containing the event data.</param>
		private void viewFiles_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			FileSystemFile file = null;			// Selected file.
			FileSystemFile newFile = null;		// Newly renamed file.

			try
			{
				file = (FileSystemFile)viewFiles.Items[e.Item].Tag;

				// Don't allow an empty name.
				if ((e.Label == string.Empty) || (e.Label == null))
				{
					e.CancelEdit = true;
					return;
				}

				// Ensure we have a valid filename.
				if (!FileSystemFile.ValidFilename(e.Label))
				{
					string invalidChars = string.Empty;	// Invalid characters.

					// Get the list of invalid file characters.
					foreach (char c in FileSystemFile.InvalidCharacters)
					{
						if (c > 32)
						{
							if (invalidChars != string.Empty)
								invalidChars += ", ";
							invalidChars += "'" + c.ToString() + "'";
						}
					}

					UI.ErrorBox(this, "The new filename contains invalid characters.\nThe characters:\n" + invalidChars + "\nAre not allowed.");
					e.CancelEdit = true;
					return;
				}

				// Do nothing if we haven't changed the name.
				if (e.Label.ToLower() == file.Filename.ToLower() + file.Extension.ToLower())
					return;

				// Don't use the same name as another file.
				if (file.Owner.Files.Contains(e.Label))
				{
					e.CancelEdit = true;
					UI.ErrorBox(this, "Unable to rename the file, a file with the name '" + e.Label + "' already exists.");
					return;
				}

				// Add the file with the new name.
				newFile = file.Owner.Files.Add(e.Label, file.Data, file.Size, file.CompressedSize, file.DateTime, file.IsEncrypted);
				newFile.Comment = file.Comment;

				// Remove the old file.
				_fileSystem.Delete(file.FullPath);
				//file.Owner.Files.Remove(file.Filename + file.Extension);
			}
			catch (Exception ex)
			{
				e.CancelEdit = true;
				UI.ErrorBox(this, ex);
			}
			finally
			{
				try
				{
					if (newFile != null)
					{
						// Replace the tag.
						viewFiles.Items[e.Item].Tag = newFile;
						// Update file extension.
						viewFiles.Items[e.Item].SubItems[1].Text = newFile.Extension;

						// Update icon.
						GetFileIcon(viewFiles.Items[e.Item], newFile);

						// Retrieve the file properties again.
						GetFileProperties();
					}
				}
				catch (Exception ex)
				{
					UI.ErrorBox(this, ex);
				}

				ValidatePopup();
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void viewFiles_KeyDown(object sender, KeyEventArgs e)
		{
			if (viewFiles.SelectedItems.Count > 0)
			{
				// Rename file.
				if (e.KeyCode == Keys.F2)
				{
					viewFiles.SelectedItems[0].BeginEdit();
					e.Handled = true;
					return;
				}

				// Delete files.
				if (e.KeyCode == Keys.Delete)
				{
					menuItemRemoveFiles_Click(this, EventArgs.Empty);
					e.Handled = true;
				}
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemRemoveFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemRemoveFiles_Click(object sender, EventArgs e)
		{
			try
			{
				FileSystemFile file = null;								// Selected file.
				ConfirmationResult delete = ConfirmationResult.None;	// Deletion confirmation.

				// Go through each file and remove it.
				foreach (ListViewItem item in viewFiles.SelectedItems)
				{
					if ((delete & ConfirmationResult.ToAll) == 0)
						delete = UI.ConfirmBox(this, "Are you sure you wish to delete the file '" + item.Text + "'?", true, true);

					if (delete == ConfirmationResult.Cancel)
						return;

					// Remove the file.
					if ((delete & ConfirmationResult.No) == 0)
					{
						file = (FileSystemFile)item.Tag;

						_fileSystem.Delete(file.FullPath);
						_isChanged = true;
					}
				}

				// Refresh the file list.
				IsChanged = _isChanged;
				GetFiles(treePaths.SelectedNode);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidatePopup();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemEditComment control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemEditComment_Click(object sender, EventArgs e)
		{
			formComment comment = null;		// Comment.
			FileSystemFile file = null;		// Selected file.

			try
			{
				comment = new formComment();
				file = (FileSystemFile)viewFiles.SelectedItems[0].Tag;

				comment.Text = "Comment - " + file.FullPath;
				comment.Comment = file.Comment;

				if (comment.ShowDialog(this) == DialogResult.OK)
				{
					file.Comment = comment.Comment;
					file.DateTime = DateTime.Now;
					IsChanged = true;

					// Refresh the file list.
					GetFiles(treePaths.SelectedNode);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidatePopup();
			}
		}
       
        /// <summary>
		/// Function to retrieve file properties.
		/// </summary>
		private void GetFileProperties()
		{
			FileSystemFile file = null;				// Selected file.
			string properties = string.Empty;		// Property list.
			bool compressedFiles = false;			// Flag to indicate compressed files.
			bool encryptedFiles = false;			// Flag to indicate encrypted files.
			string previewFile = string.Empty;		// Filename + path for preview image.

			// Turn off other items.
			labelComment.Visible = false;
			textComment.Visible = false;

			labelPreview.Visible = false;
			labelPreview.Top = 259;
			textPreview.Visible = false;
			textPreview.Top = 275;
			pictureImage.Visible = false;
			pictureImage.Top = 275;
			htmlPreview.Top = 275;
			htmlPreview.Visible = false;

			// Reset icon image.
			pictureIcon.Image = pictureIcon.InitialImage;

			// Reset properties.
			textFileName.Text = "No file(s) selected.";
			if (treePaths.SelectedNode == null)
				textLocation.Text = "No path.";
			else
				textLocation.Text = ((FileSystemPath)treePaths.SelectedNode.Tag).FullPath;

			textProperties.Text = "None.";

			// One item selected.
			if (viewFiles.SelectedItems.Count == 1)
			{
				// Move the preview if the comment is not visible.
				if (!labelComment.Visible)
				{
					labelPreview.Top = labelComment.Top;
					htmlPreview.Top = textComment.Top;
					pictureImage.Top = textComment.Top;
					textPreview.Top = textComment.Top;
				}

				properties = string.Empty;

				// Get file information.
				file = (FileSystemFile)viewFiles.SelectedItems[0].Tag;

				textFileName.Text = file.Name;
				pictureIcon.Image = _parentWindow.iconImages32x32.Images[viewFiles.SelectedItems[0].ImageKey];

				if (file.IsEncrypted)
					properties += "File is encrypted.\r\n";

				if (file.IsCompressed)
					properties += "File is compressed.\r\n";

				if (properties != string.Empty)
					textProperties.Text = properties;

				// Get comment if one exists.
				if (file.Comment != string.Empty)
				{
					labelComment.Visible = true;
					textComment.Visible = true;
					textComment.Text = file.Comment;
				}

				switch (file.Extension.ToLower())
				{
					case ".html":
						htmlPreview.DocumentText = Encoding.Default.GetString(_fileSystem.ReadFile(file.FullPath));
						htmlPreview.Visible = true;
						labelPreview.Visible = true;
						htmlPreview.Height = htmlPreview.Width;
						break;
					case ".xml":
					case ".txt":
					case ".fx":
					case ".fxh":
					case ".ini":
					case ".cs":
					case ".vb":
					case ".cpp":
					case ".c":
					case ".h":
						labelPreview.Visible = true;
						textPreview.Visible = true;
						// Get the string from the file system.
						textPreview.Text = Encoding.UTF8.GetString(_fileSystem.ReadFile(file.FullPath));
						textPreview.Height = textPreview.Width;
						break;
					case ".png":
					case ".bmp":
					case ".jpg":
					case ".jpeg":
						using (Stream stream = _fileSystem.OpenFileStream(file.FullPath))
						{
							pictureImage.Image = Image.FromStream(stream);

							// Make the image square.
							pictureImage.Height = pictureImage.Width;
						}
                        pictureImage.Visible = true;
						break;
				}
				return;
			}

			// Multiple selection.
			if (viewFiles.SelectedItems.Count > 1)
			{
				// Add multiple files.
				textFileName.Text = string.Empty;
				foreach (ListViewItem item in viewFiles.SelectedItems)
				{
					// Get file information.
					file = (FileSystemFile)item.Tag;

					if (textFileName.Text != string.Empty)
						textFileName.Text += ";";

					textFileName.Text += item.Text;

					if (file.IsCompressed)
						compressedFiles = true;
					if (file.IsEncrypted)
						encryptedFiles = true;
				}

				if (compressedFiles)
					properties += "Some/all files are compressed.\r\n";
				if (encryptedFiles)
					properties += "Files are encrypted.\r\n";

				// Update properties box.
				if (properties != string.Empty)
					textProperties.Text = properties;
				return;
			}
		}

		/// <summary>
		/// Handles the ItemSelectionChanged event of the viewFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ListViewItemSelectionChangedEventArgs"/> instance containing the event data.</param>
		private void viewFiles_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				GetFileProperties();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Cannot retrieve file properties.", ex);
			}
		}

        /// <summary>
        /// Handles the Click event of the buttonChangeAuth control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonChangeAuth_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileSystem.CreateAuthorization(this) != 0)
                    _isChanged = true;
                UpdateCaption();
            }
            catch (Exception ex)
            {
                UI.ErrorBox(this, "Error trying to alter the authentication.", ex);
            }
        }
        
        /// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RefreshTree();

			// Create sorter list.
			_sorters = new ListViewCompare[10];

			// Create sorters.
			for (int i = 0; i < 10; i++)
				_sorters[i] = new ListViewCompare(i);

			UpdateCaption();
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
				// Check to see why we're closing.
				if ((e.CloseReason == CloseReason.MdiFormClosing) && (_parentWindow.InFileOperation))
				{
					e.Cancel = true;
					return;
				}

				// Cancel?
				if (e.Cancel)
					return;

				// Do not close until we have a chance to save.
				if (IsChanged)
				{
					// Confirm overwrite.
					if ((_parentWindow.OverwriteResult & ConfirmationResult.ToAll) == 0)
						_parentWindow.OverwriteResult = UI.ConfirmBox(this, "The file system '" + Text + "' has changes.\nDo you wish to save it?", "Unsaved changes.", true, true);

					if (_parentWindow.OverwriteResult == ConfirmationResult.Cancel)
					{
						e.Cancel = true;
						return;
					}

					if ((_parentWindow.OverwriteResult & ConfirmationResult.Yes) == ConfirmationResult.Yes)
						_parentWindow.SaveWindow(this);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:ParentChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (MdiParent != null)
			{
				_parentWindow = (formMain)MdiParent;
				_parentWindow.ValidateForm();

				// Get image lists.
				viewFiles.LargeImageList = _parentWindow.iconImages32x32;
				viewFiles.SmallImageList = _parentWindow.iconImages16x16;

				// Get the last view setting.
				SetViewState((View)Enum.Parse(typeof(View), _parentWindow.LastView));
			}
			else
				_parentWindow = null;
		}

        /// <summary>
        /// Function to change the authentication.
        /// </summary>
        public void ChangeAuthentication()
        {
            buttonChangeAuth.PerformClick();
        }

		/// <summary>
		/// Function to save the file system.
		/// </summary>
		/// <param name="filePath">Path or file to save the file system into.</param>
		public void Save(string filePath)
		{
            if ((_fileSystem.Provider.IsEncrypted) && (_fileSystem.AuthenticationData == null))
            {
                if (_fileSystem.CreateAuthorization(this) == 0)
                {
                    UI.ErrorBox(this, "The file system is encrypted and requires authorization.  Please provide credentials.");
                    return;
                }
            }

			// Save to the path.
			_fileSystem.Save(filePath);

			// Update flags and set the root path.
			_rootPath = filePath;
			IsNew = false;
			IsChanged = false;

			// Write the settings file.
			_parentWindow.WriteSettings();

			// Refresh the tree in case we've updated the structure.
			UpdateNodeTags(treePaths.Nodes);
			SetViewState(_view);
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
	}
}