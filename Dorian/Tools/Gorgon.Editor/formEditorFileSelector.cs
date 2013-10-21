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
// Created: Wednesday, October 16, 2013 3:08:44 PM
// 
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A file selector window for retrieving files from the mounted scratch area.
	/// </summary>
	partial class formEditorFileSelector 
		: ZuneForm
	{
		#region Classes
		/// <summary>
		/// A textbox that displays cue text when the text is empty.
		/// </summary>
		internal class CuedTextBox
			: TextBox
		{
			#region Constants.
			private const int WM_PAINT = 0x000F;			// Paint message.
			#endregion

			#region Variables.
			private string _cueText = string.Empty;				// Text for the cue.
			private Font _cueFont;								// Font used to draw the cue.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the text for the textbox cue.
			/// </summary>
			[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(Resources), "PROP_CUETEXT_DESC")]
			public string CueText
			{
				get
				{
					return _cueText;
				}
				set
				{
					if (value == null)
					{
						value = string.Empty;
					}

					_cueText = value;
					Invalidate();
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Processes Windows messages.
			/// </summary>
			/// <param name="m">A Windows Message object.</param>
			protected override void WndProc(ref Message m)
			{
				switch (m.Msg)
				{
					case WM_PAINT:
						base.WndProc(ref m);

						if ((!string.IsNullOrEmpty(Text))
							|| (string.IsNullOrWhiteSpace(_cueText))
							|| (Focused))
						{
							break;
						}

						using (var g = CreateGraphics())
						{
							// If the font has not been created, then create it now.
							if (_cueFont == null)
							{
								_cueFont = new Font(Font, FontStyle.Italic);
							}
							using (var brush = new SolidBrush(BackColor))
							{
								g.FillRectangle(brush, ClientRectangle);
								TextRenderer.DrawText(g, _cueText, _cueFont, Point.Empty, Color.FromKnownColor(KnownColor.GrayText));
							}
						}
						break;
					default:
						base.WndProc(ref m);
						break;
				}
			}

			/// <summary>
			/// Raises the <see cref="E:System.Windows.Forms.Control.Leave" /> event.
			/// </summary>
			/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
			protected override void OnLeave(EventArgs e)
			{
				Invalidate();
				base.OnLeave(e);
			}

			/// <summary>
			/// Raises the <see cref="E:System.Windows.Forms.Control.Enter" /> event.
			/// </summary>
			/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
			protected override void OnEnter(EventArgs e)
			{
				Invalidate();
				if (!string.IsNullOrEmpty(Text))
				{
					SelectAll();
				}
				base.OnEnter(e);
			}

			/// <summary>
			/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.TextBox" /> and optionally releases the managed resources.
			/// </summary>
			/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (_cueFont != null)
					{
						_cueFont.Dispose();
						_cueFont = null;
					}
				}

				base.Dispose(disposing);
			}
			#endregion
		}
		#endregion

		#region Value Types.
		/// <summary>
		/// An item for the filter combo.
		/// </summary>
		private struct FilterItem
		{
			/// <summary>
			/// The extension for the filter.
			/// </summary>
			public readonly GorgonFileExtension Extension;

			/// <summary>
			/// Returns a <see cref="System.String" /> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String" /> that represents this instance.
			/// </returns>
			public override string ToString()
			{
				return Extension.Description;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="FilterItem"/> struct.
			/// </summary>
			/// <param name="extension">The extension.</param>
			public FilterItem(GorgonFileExtension extension)
			{
				Extension = extension;
			}
		}
		#endregion

		#region Enumerations.
		/// <summary>
		/// The sorting column.
		/// </summary>
		private enum SortColumn
		{
			/// <summary>
			/// No column.
			/// </summary>
			None = 0,
			/// <summary>
			/// File name.
			/// </summary>
			Name = 1,
			/// <summary>
			/// Date.
			/// </summary>
			Date = 2,
			/// <summary>
			/// File size.
			/// </summary>
			Size = 3
		}
		#endregion

		#region Variables.
		private TreeNodeDirectory _rootDirectory;														// The root directory node.
		private TreeNodeDirectory _selectedDirectory;													// The currently selected directory node.
		private int _fillTreeLock;																		// Lock used to keep the call to fill the tree from being re-entrant.
		private SortOrder _sort = SortOrder.Ascending;													// Current sort direction.
		private SortColumn _currentSortColumn = SortColumn.Name;										// Currently sorted column.
		private readonly GorgonFileExtensionCollection _fileExtensions;									// The list of file extensions to filter.
		private readonly HashSet<GorgonFileSystemFileEntry> _thumbNailFiles;							// Thumbnails awaiting loading.
		private CancellationTokenSource _cancelSource;													// Cancellation source.
		private Size _thumbNailSize;																	// Thumbnail size.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the currently selected file view mode.
		/// </summary>
		public FileViews CurrentView
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default extension.
		/// </summary>
		public string DefaultExtension
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the directory that is used as the start directory.
		/// </summary>
		public string StartDirectory
		{
			get;
			set;
		}
		#endregion

		#region Methods.

		/// <summary>
		/// Function to switch the current directory and update the tree.
		/// </summary>
		/// <param name="directory">Directory to switch to.</param>
		private void SwitchDirectory(GorgonFileSystemDirectory directory)
		{
			if (_selectedDirectory.Directory.Parent == directory)
			{
				_selectedDirectory = (TreeNodeDirectory)_selectedDirectory.Parent;
			}
			else
			{
				// Locate the tree node for the parent of this directory.
				if ((_selectedDirectory.Nodes.Count > 0) && (!_selectedDirectory.IsExpanded))
				{
					_selectedDirectory.Expand();
				}

				_selectedDirectory = _selectedDirectory.Nodes.Cast<TreeNodeDirectory>().First(item => item.Directory == directory);
			}

			treeDirectories.SelectedNode = _selectedDirectory;
		}

		/// <summary>
		/// Handles the Click event of the buttonGoUp control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonGoUp_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				SwitchDirectory(_selectedDirectory.Directory.Parent);

				if ((_selectedDirectory != _rootDirectory) && (_selectedDirectory.IsExpanded))
				{
					_selectedDirectory.Collapse();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the DoubleClick event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void listFiles_DoubleClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				ListViewHitTestInfo item = listFiles.HitTest(listFiles.PointToClient(Cursor.Position));

				// We didn't click on anything, leave.
				if (item.Item == null)
				{
					return;
				}

				var directory = item.Item.Tag as GorgonFileSystemDirectory;

				if (directory != null)
				{
					SwitchDirectory(directory);

					if ((_selectedDirectory.Nodes.Count > 0) && (!_selectedDirectory.IsExpanded))
					{
						_selectedDirectory.Expand();
					}
					return;
				}
			}
			catch (Exception ex)
			{

				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the ColumnClick event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ColumnClickEventArgs"/> instance containing the event data.</param>
		private void listFiles_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			var selectedColumn = (SortColumn)listFiles.Columns[e.Column].Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				// Turn off sort if we've clicked the same column a 3rd time.
				if ((_currentSortColumn == selectedColumn) && (_sort == SortOrder.Descending))
				{
					listFiles.SetSortIcon(e.Column, SortOrder.None);
					_sort = SortOrder.None;
					_currentSortColumn = SortColumn.None;
					return;
				}

				// Reset the sort if we switch columns.
				if (_currentSortColumn != selectedColumn)
				{
					_sort = SortOrder.None;
				}

				for (int i = 0; i < listFiles.Columns.Count; ++i)
				{
					listFiles.SetSortIcon(i, SortOrder.None);
				}
				_currentSortColumn = selectedColumn;

				switch (_sort)
				{
					case SortOrder.None:
						listFiles.SetSortIcon(e.Column, SortOrder.Ascending);
						_sort = SortOrder.Ascending;
						break;
					case SortOrder.Ascending:
						listFiles.SetSortIcon(e.Column, SortOrder.Descending);
						_sort = SortOrder.Descending;
						break;
				}

				
				FillFiles(_selectedDirectory.Directory);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemViewLarge control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemViewLarge_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				var item = (ToolStripMenuItem)sender;

				foreach (ToolStripMenuItem oldItem in buttonView.DropDownItems)
				{
					oldItem.Checked = false;
				}
				item.Checked = true;

				if (item == itemViewDetails)
				{
					listFiles.View = View.Details;
					CurrentView = FileViews.Details;
				}
				else
				{
					listFiles.View = View.LargeIcon;
					CurrentView = FileViews.Large;
				}

				FillFiles(_selectedDirectory.Directory);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboFilters control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboFilters_SelectedIndexChanged(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				FillFiles(_selectedDirectory.Directory);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the AfterSelect event of the treeDirectories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
		private void treeDirectories_AfterSelect(object sender, TreeViewEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (e.Node == null)
				{
					treeDirectories.SelectedNode = _selectedDirectory = _rootDirectory;
					return;
				}

				_selectedDirectory = (TreeNodeDirectory)e.Node;
				FillFiles(_selectedDirectory.Directory);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the BeforeExpand event of the treeDirectories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeDirectories_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				FillDirectoryTree((EditorTreeNode)e.Node, false);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			buttonOK.Enabled = comboFile.Text.Length > 0;
			buttonGoUp.Enabled = _selectedDirectory != null && _selectedDirectory != _rootDirectory;
		}

		/// <summary>
		/// Function to perform localization text substitution on any strings in the dialog.
		/// </summary>
		private void LocalizeStrings()
		{
			textSearch.CueText = Resources.GOREDIT_DLG_SEARCH_TEXT;
			label1.Text = string.Format("{0}:", Resources.GOREDIT_DLG_FILE_LABEL);
			itemViewDetails.Text = Resources.GOREDIT_DLG_MENU_DETAILS;
			itemViewLarge.Text = Resources.GOREDIT_DLG_MENU_LARGE;
			columnFileName.Text = Resources.GOREDIT_DLG_COLUMN_NAME;
			columnDate.Text = Resources.GOREDIT_DLG_COLUMN_DATE;
			columnSize.Text = Resources.GOREDIT_DLG_COLUMN_SIZE;
			buttonView.Text = Resources.GOREDIT_DLG_CHANGEVIEW_TEXT;
			buttonGoUp.Text = Resources.GOREDIT_DLG_GOUP_BUTTON_TEXT;
			buttonOK.Text = Resources.GOREDIT_BTN_OK;
			buttonCancel.Text = Resources.GOREDIT_BTN_CANCEL;
		}

		/// <summary>
		/// Function to fill in the directory tree.
		/// </summary>
		/// <param name="root">The root node of the tree to fill.</param>
		/// <param name="setInitialSelected">TRUE to set the initially selected node, FALSE to sjip it.</param>
		private void FillDirectoryTree(EditorTreeNode root, bool setInitialSelected)
		{
			TreeNodeDirectory selectedNode = null;
		
			try
			{
				if (Interlocked.Increment(ref _fillTreeLock) > 1)
				{
					return;
				}

				treeDirectories.BeginUpdate();

				if (root == null)
				{
					treeDirectories.Nodes.Clear();
					root = selectedNode = _rootDirectory = new RootNodeDirectory();
					treeDirectories.Nodes.Add(root);
				}

				root.Nodes.Clear();

				// Get the file system directory.
				GorgonFileSystemDirectory directory = ((TreeNodeDirectory)root).Directory;

				foreach (var subDirectory in directory.Directories)
				{
					TreeNodeDirectory node = root.Nodes.AddDirectory(subDirectory, false);

					// Because AddDirectory detects files, it will return with child nodes.
					// We only want directories.
					if (subDirectory.Directories.Count < 1)
					{
						node.Nodes.Clear();
					}

					if ((!string.IsNullOrEmpty(StartDirectory)) && (string.Equals(StartDirectory, directory.FullPath)))
					{
						selectedNode = node;
					}
				}

				if (setInitialSelected)
				{
					treeDirectories.SelectedNode = _selectedDirectory = selectedNode;
				}

				if ((selectedNode == _rootDirectory) && (selectedNode != null))
				{
					selectedNode.Expand();
				}
			}
			finally
			{
				if (Interlocked.Decrement(ref _fillTreeLock) == 0)
				{
					treeDirectories.EndUpdate();					
				}
			}
		}

		/// <summary>
		/// Function to return a list of sorted files.
		/// </summary>
		/// <param name="directory">Directory containing the files.</param>
		/// <returns>A sorted list of files.</returns>
		private IEnumerable<GorgonFileSystemFileEntry> GetSortedFiles(GorgonFileSystemDirectory directory)
		{
			FilterItem filter = default(FilterItem);

			if (comboFilters.SelectedItem != null)
			{
				filter = (FilterItem)comboFilters.SelectedItem;
			}

			// If we select all files, the just use the default.
			if (filter.Extension.Extension == "*")
			{
				filter = default(FilterItem);
			}

			// Filter the files.
			var sortedFiles = directory.Files.Where(item => !ScratchArea.IsBlocked(item)
			                                                && ((filter.Extension.IsEmpty)
			                                                    || (filter.Extension.Equals(new GorgonFileExtension(item.Extension)))));

			if (listFiles.View == View.Details)
			{
				switch (_currentSortColumn)
				{
					case SortColumn.Name:
						switch (_sort)
						{
							case SortOrder.Ascending:
								return from file in sortedFiles
								       orderby file.Name.ToLower(CultureInfo.CurrentUICulture)
								       select file;
							case SortOrder.Descending:
								return from file in sortedFiles
								       orderby file.Name.ToLower(CultureInfo.CurrentUICulture) descending
								       select file;
						}
						break;
					case SortColumn.Date:
						switch (_sort)
						{
							case SortOrder.Ascending:
								return from file in sortedFiles
								       orderby file.CreateDate
								       select file;
							case SortOrder.Descending:
								return from file in sortedFiles
								       orderby file.CreateDate descending
								       select file;
						}
						break;
					case SortColumn.Size:
						switch (_sort)
						{
							case SortOrder.Ascending:
								return from file in sortedFiles
								       orderby file.Size
								       select file;
							case SortOrder.Descending:
								return from file in sortedFiles
								       orderby file.Size descending
								       select file;
						}
						break;
				}
			}
			else
			{
				return sortedFiles.OrderBy(item => item.Name.ToLower(CultureInfo.CurrentUICulture));
			}

			return sortedFiles;
		}

		/// <summary>
		/// Function to retrieve the thumbnail for content.
		/// </summary>
		/// <param name="file">File that holds the content.</param>
		/// <returns>The thumbnail bitmap for the content.</returns>
		private Image GetContentThumbNail(GorgonFileSystemFileEntry file)
		{
			// If we have this open in the editor, then use that so we get unsaved changes as well.
			if ((ScratchArea.CurrentOpenFile == file) && (ContentManagement.Current != null))
			{
				return ContentManagement.Current.HasThumbnail ? ContentManagement.Current.GetThumbNailImage() : null;
			}

			ContentPlugIn plugIn = ContentManagement.GetContentPlugInForFile(file.Extension);

			if (plugIn == null)
			{
				return null;
			}

			ContentSettings settings = plugIn.GetContentSettings();
			settings.Name = file.Name;
			settings.CreateContent = false;

			// We need to load the content so we can generate a thumbnail (or retrieve cached version).
			using (ContentObject content = plugIn.CreateContentObject(settings))
			{
				if (!content.HasThumbnail)
				{
					return null;
				}

				EditorMetaDataFile metaData = plugIn.GetMetaData(file.FullPath);
				content.MetaData = metaData;

				using (Stream contentStream = file.OpenStream(false))
				{
					content.Read(contentStream);
				}

				Image result = content.GetThumbNailImage();

				// If the image isn't the correct size then we need to rescale it.
				if ((result.Width == _thumbNailSize.Width) && (result.Height == _thumbNailSize.Height))
				{
					return result;
				}

				var scaledThumbNail = new Bitmap(_thumbNailSize.Width, _thumbNailSize.Height, PixelFormat.Format32bppArgb);

				try
				{
					using (var graphics = System.Drawing.Graphics.FromImage(scaledThumbNail))
					{
						float aspect = (float)result.Width / result.Height;
						SizeF size = _thumbNailSize;
						PointF location;

						if (aspect > 1.0f)
						{
							size.Height = size.Height / aspect;
							location = new PointF(0, _thumbNailSize.Height / 2 - size.Height / 2.0f);	
						}
						else
						{
							size.Width = size.Width * aspect;
							location = new PointF(_thumbNailSize.Width / 2 - size.Width / 2.0f, 0);
						}

						// Scale the image down.
						graphics.SmoothingMode = SmoothingMode.HighQuality;
						graphics.DrawImage(result,
						                   new RectangleF(location, size),
						                   new RectangleF(0, 0, result.Width, result.Height),
						                   GraphicsUnit.Pixel);
					}
				}
				finally
				{
					result.Dispose();
				}

				return scaledThumbNail;
			}
		}

		/// <summary>
		/// Function to retrieve the thumb nails for the file view when in thumbnail mode on a separate thread.
		/// </summary>
		private void GetThumbNails()
		{
			// Get the first item that needs a thumbnail.
			while ((_thumbNailFiles.Count > 0) && (!_cancelSource.Token.IsCancellationRequested))
			{
				GorgonFileSystemFileEntry item = _thumbNailFiles.First();

				try
				{
					if (_cancelSource.Token.IsCancellationRequested)
					{
						return;
					}

					Image thumbNail = GetContentThumbNail(item);

					// Attach the thumbnail to the proper list view item.  Do this on the form thread or else
					// bad things will happen.
					if ((thumbNail != null) && (!_cancelSource.Token.IsCancellationRequested))
					{
						Invoke(new MethodInvoker(() =>
						                         {
							                         // Just a last minute check to ensure we aren't touching disposed objects.
							                         if (IsDisposed)
							                         {
								                         return;
							                         }

							                         if (!imagesFilesLarge.Images.ContainsKey(item.FullPath))
							                         {
								                         imagesFilesLarge.Images.Add(item.FullPath, thumbNail);
							                         }

							                         // Attempt to remove this item.
							                         _thumbNailFiles.Remove(item);

							                         // If the list view no longer contains this item, then we should dump it.
							                         // It'll still be cached in our image list for later.
							                         if (listFiles.Items.ContainsKey(item.FullPath))
							                         {
								                         listFiles.Items[item.FullPath].ImageKey = item.FullPath;
							                         }
						                         }));
					}
				}
				finally
				{
					// Attempt to remove this item.
					if (_thumbNailFiles.Contains(item))
					{
						_thumbNailFiles.Remove(item);
					}
				}
			}

			if (_cancelSource == null)
			{
				return;
			}

			_cancelSource.Dispose();
			_cancelSource = null;
		}

		/// <summary>
		/// Function to fill in the file list.
		/// </summary>
		/// <param name="directory">Directory containing the files.</param>
		private async void FillFiles(GorgonFileSystemDirectory directory)
		{
			try
			{
				listFiles.BeginUpdate();
				listFiles.Items.Clear();
				
				// Add directories first and sort by name.
				IEnumerable<GorgonFileSystemDirectory> sortedDirectories;

				switch (_sort)
				{
					case SortOrder.Ascending:
						sortedDirectories = from subDir in directory.Directories
						                    orderby subDir.Name.ToLower(CultureInfo.CurrentUICulture)
						                    select subDir;
						break;
					case SortOrder.Descending:
						sortedDirectories = from subDir in directory.Directories
						                    orderby subDir.Name.ToLower(CultureInfo.CurrentUICulture) descending
						                    select subDir;
						break;
					default:
						sortedDirectories = directory.Directories;
						break;
				}

				// Add our items to the list.
				foreach (var subDirectory in sortedDirectories)
				{
					var item = new ListViewItem(subDirectory.Name)
					           {
						           ImageKey = subDirectory.Files.Count > 0 || subDirectory.Directories.Count > 0 ? @"directory_wfiles" : @"directory",
								   Tag = subDirectory
					           };

					listFiles.Items.Add(item);
				}

				IEnumerable<GorgonFileSystemFileEntry> sortedFiles = GetSortedFiles(directory);

				// If we have over 256 large images cached, then clear the cache.
				if (imagesFilesLarge.Images.Count > 256)
				{
					while (imagesFilesLarge.Images.Count > 3)
					{
						imagesFilesLarge.Images[imagesFilesLarge.Images.Count - 1].Dispose();
						imagesFilesLarge.Images.RemoveAt(imagesFilesLarge.Images.Count - 1);
					}
				}
			
				// Add the files.
				foreach (var file in sortedFiles)
				{
					var item = new ListViewItem(file.Name)
					           {
								   Name = file.FullPath,
						           ImageKey = @"unknown_file",
								   Tag = file
					           };

					if (CurrentView != FileViews.Large)
					{
						ContentPlugIn plugIn = ContentManagement.GetContentPlugInForFile(file.Extension);

						if (plugIn != null)
						{
							item.ImageKey = plugIn.Name;
						}

						item.SubItems.Add(file.CreateDate.ToString(CultureInfo.CurrentUICulture.DateTimeFormat));
						item.SubItems.Add(file.Size.FormatMemory());
					}
					else
					{
						// This will queue up files to extract thumbnails from.
						// The thumbnails will be extracted via a background thread which will load the content file
						// and fill in the list view item image when the thumbnail creation is complete.
						// This thread will run for a maximum of 10 minutes.  If all thumbnails are not generated in that time,
						// then they will be generated on the next refresh (as they'll still be in the queue).
						if (imagesFilesLarge.Images.ContainsKey(file.FullPath))
						{
							// We already loaded this guy, so let's not add it to the queue.
							item.ImageKey = file.FullPath;
						}
						else
						{
							if (!_thumbNailFiles.Contains(file))
							{
								_thumbNailFiles.Add(file);
							}
						}
					}

					listFiles.Items.Add(item);
				}
			}
			finally 
			{
				listFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				listFiles.EndUpdate();

				// The drawing gets messed up because of the shell stuff.
				treeDirectories.Refresh();
			}

			// Launch a thread to retrieve the thumbnails.
			if ((CurrentView != FileViews.Large) || (_cancelSource != null))
			{
				return;
			}

			// Get the thumbnails on a separate thread.  This way loading the thumbnails won't be a painfully slow process.
			_cancelSource = new CancellationTokenSource(600000);
			await Task.Factory.StartNew(GetThumbNails, _cancelSource.Token);
		}

		/// <summary>
		/// Function to fill the filter list for the files.
		/// </summary>
		private void FillFilterList()
		{
			comboFilters.Items.Clear();

			comboFilters.SelectedIndexChanged -= comboFilters_SelectedIndexChanged;
			
			if (_fileExtensions.Count == 0)
			{
				panelFilters.Visible = false;
				return;
			}

			try
			{
				int maxWidth = int.MinValue;

				// Measure the text to get the appropriate combo box width.
				using (System.Drawing.Graphics g = comboFilters.CreateGraphics())
				{
					panelFilters.Visible = true;

					// Add extensions to the list.
					foreach (var extension in _fileExtensions)
					{
						comboFilters.Items.Add(new FilterItem(extension));

						// Capture the default extension.
						if (string.Equals(extension.Extension, DefaultExtension, StringComparison.OrdinalIgnoreCase))
						{
							comboFilters.Text = extension.Description;
						}

						maxWidth = maxWidth.Max(TextRenderer.MeasureText(g, extension.Description, comboFilters.Font).Width);
					}

					comboFilters.DropDownWidth = maxWidth.Max(comboFilters.Width);

					if (string.IsNullOrWhiteSpace(comboFilters.Text))
					{
						comboFilters.Text = comboFilters.Items[0].ToString();
					}
				}
			}
			finally
			{
				comboFilters.SelectedIndexChanged += comboFilters_SelectedIndexChanged;
			}
		}

		/// <summary>
		/// Function to fill the icon lists for the list/detail view.
		/// </summary>
		private void FillIconList()
		{
			// Match up the files that can be opened by the editor.
			foreach (var plugIn in PlugIns.ContentPlugIns)
			{
				imagesFilesSmall.Images.Add(plugIn.Value.Name, plugIn.Value.GetContentIcon());
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				if (_cancelSource != null)
				{
					Cursor.Current = Cursors.WaitCursor;

					_cancelSource.Cancel();

					// Ensure that the thumbnail thread is exited.
					while (_cancelSource != null)
					{
						Thread.Sleep(0);
					}
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}

			comboFilters.SelectedIndexChanged -= comboFilters_SelectedIndexChanged;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			LocalizeStrings();

			base.OnLoad(e);

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				columnFileName.Tag = SortColumn.Name;
				columnDate.Tag = SortColumn.Date;
				columnSize.Tag = SortColumn.Size;

				FillIconList();

				FillFilterList();

				// Default to sorted by name.
				listFiles.SetSortIcon(0, SortOrder.Ascending);

				FillDirectoryTree(null, false);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formEditorFileSelector"/> class.
		/// </summary>
		/// <param name="extensions">File extensions used to filter the file list.</param>
		public formEditorFileSelector(GorgonFileExtensionCollection extensions)
		{
			InitializeComponent();

			CurrentView = FileViews.Details;
			_fileExtensions = extensions;
			_thumbNailFiles = new HashSet<GorgonFileSystemFileEntry>();
			_thumbNailSize = new Size(128, 128);
		}
		#endregion
	}
}
