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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using Microsoft.WindowsAPICodePack.Shell;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.Properties;
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

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="CuedTextBox"/> class.
			/// </summary>
			public CuedTextBox()
			{
				
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
		private TreeNodeDirectory _rootDirectory;								// The root directory node.
		private TreeNodeDirectory _selectedDirectory;							// The currently selected directory node.
		private int _fillTreeLock;												// Lock used to keep the call to fill the tree from being re-entrant.
		private readonly Dictionary<SortColumn, SortOrder> _sort;				// Sorted values.
		private SortColumn _currentSortColumn = SortColumn.Name;				// Currently sorted column.
		private FileViews _currentView = FileViews.Large;						// Currently selected view.
		private readonly GorgonFileExtensionCollection _fileExtensions;			// The list of file extensions to filter.
		#endregion

		#region Properties.
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
			Cursor.Current = Cursors.Default;
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
			}
		}

		/// <summary>
		/// Function to perform localization text substitution on any strings in the dialog.
		/// </summary>
		private void LocalizeStrings()
		{
			textSearch.CueText = Resources.GOREDIT_DLG_SEARCH_TEXT;
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
			IEnumerable<GorgonFileSystemFileEntry> sortedFiles = directory.Files;

			if (listFiles.View == View.Details)
			{
				SortOrder order = _sort[_currentSortColumn];

				switch (_currentSortColumn)
				{
					case SortColumn.Name:
						switch (order)
						{
							case SortOrder.Ascending:
								sortedFiles = from file in directory.Files
											  orderby file.Name.ToLower(CultureInfo.CurrentUICulture)
											  select file;
								break;
							case SortOrder.Descending:
								sortedFiles = from file in directory.Files
											  orderby file.Name.ToLower(CultureInfo.CurrentUICulture) descending
											  select file;
								break;
						}
						break;
					case SortColumn.Date:
						switch (order)
						{
							case SortOrder.Ascending:
								sortedFiles = from file in directory.Files
											  orderby file.CreateDate
											  select file;
								break;
							case SortOrder.Descending:
								sortedFiles = from file in directory.Files
											  orderby file.CreateDate descending
											  select file;
								break;
						}
						break;
					case SortColumn.Size:
						switch (order)
						{
							case SortOrder.Ascending:
								sortedFiles = from file in directory.Files
											  orderby file.Size
											  select file;
								break;
							case SortOrder.Descending:
								sortedFiles = from file in directory.Files
											  orderby file.Size descending
											  select file;
								break;
						}
						break;
				}
			}
			else
			{
				sortedFiles = directory.Files.OrderBy(item => item.Name.ToLower(CultureInfo.CurrentUICulture));
			}

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

			return sortedFiles.Where(item =>
			                         !ScratchArea.IsBlocked(item) &&
			                         ((filter.Extension.IsEmpty)
			                          || (filter.Extension.Equals(new GorgonFileExtension(item.Extension)))));
		}

		/// <summary>
		/// Function to retrieve the thumbnail for the file.
		/// </summary>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="size">The icon size.</param>
		/// <returns>The thumbnail for the file.</returns>
		private Bitmap GetThumbNail(ShellObject shellItem, Size size)
		{
			Bitmap newBitmap;

			if (_currentView != FileViews.Large)
			{
				shellItem.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			}

			Bitmap[] thumbs =
			{
				shellItem.Thumbnail.SmallBitmap,
				shellItem.Thumbnail.MediumBitmap,
				shellItem.Thumbnail.LargeBitmap,
				shellItem.Thumbnail.ExtraLargeBitmap
			};

			// Switch to icons if we can't get thumbnails.
			if (thumbs.All(item => item == null))
			{
				shellItem.Thumbnail.RetrievalOption = ShellThumbnailRetrievalOption.Default;
				shellItem.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				thumbs = new[]
					        {
							shellItem.Thumbnail.SmallBitmap,
							shellItem.Thumbnail.MediumBitmap,
							shellItem.Thumbnail.LargeBitmap,
							shellItem.Thumbnail.ExtraLargeBitmap
					        };

				// We can't get anything, so leave.
				if (thumbs.All(item => item == null))
				{
					return null;	
				}
			}

			Bitmap thumb = thumbs.First(item => item != null);

			try
			{
				newBitmap = new Bitmap(size.Width, size.Height, thumb.PixelFormat);

				int iconIndex = Array.IndexOf(thumbs, thumb);

				// Find the nearest icon size.
				while (iconIndex != 3)
				{
					if ((size.Width > thumb.Width) || (size.Height > thumb.Height))
					{
						thumb = thumbs[++iconIndex];
					}
					else
					{
						break;
					}
				}

				using (var graphics = System.Drawing.Graphics.FromImage(newBitmap))
				{
					RectangleF destRect;
					float aspect = (float)thumb.Width / thumb.Height;
					var newSize = new SizeF(size.Width, size.Height);

					if (aspect > 1)
					{
						newSize.Height = newSize.Height / aspect;
						destRect = new RectangleF(0, size.Height / 2.0f - newSize.Height / 2.0f, newSize.Width, newSize.Height);
					}
					else
					{
						newSize.Width = newSize.Width * aspect;
						destRect = new RectangleF(size.Width / 2.0f - newSize.Width / 2.0f, 0, newSize.Width, newSize.Height);
					}

					graphics.Clear(Color.Transparent);
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.DrawImage(thumb,
						                destRect,
						                new RectangleF(0, 0, thumb.Width, thumb.Height),
						                GraphicsUnit.Pixel);
				}
			}
			finally
			{
				foreach (Bitmap image in thumbs.Where(image => image != null))
				{
					image.Dispose();
				}
			}

			return newBitmap;
		}

		/// <summary>
		/// Function to fill in the file list.
		/// </summary>
		/// <param name="directory">Directory containing the files.</param>
		private void FillFiles(GorgonFileSystemDirectory directory)
		{
			try
			{
				listFiles.BeginUpdate();
				listFiles.Items.Clear();
				
				// Add directories first and sort by name.
				IEnumerable<GorgonFileSystemDirectory> sortedDirectories;

				switch (_sort[SortColumn.Name])
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
						           ImageKey = subDirectory.Files.Count > 0 || subDirectory.Directories.Count > 0 ? @"directory_wfiles" : @"directory"
					           };

					listFiles.Items.Add(item);
				}

				IEnumerable<GorgonFileSystemFileEntry> sortedFiles = GetSortedFiles(directory);

				var iconSize = new Size(16, 16);

				switch (_currentView)
				{
					case FileViews.Large:
						iconSize = new Size(128, 128);
						break;
				}

				// If we have over 256 large images cached, then clear the cache.
				if (imagesFilesLarge.Images.Count > 256)
				{
					while (imagesFilesLarge.Images.Count > 3)
					{
						imagesFilesLarge.Images[imagesFilesLarge.Images.Count - 1].Dispose();
						imagesFilesLarge.Images.RemoveAt(imagesFilesLarge.Images.Count - 1);
					}
				}

				if (imagesFilesSmall.Images.Count > 1024)
				{
					while (imagesFilesSmall.Images.Count > 3)
					{
						imagesFilesSmall.Images[imagesFilesSmall.Images.Count - 1].Dispose();
						imagesFilesSmall.Images.RemoveAt(imagesFilesSmall.Images.Count - 1);
					}
				}

				// Add the files.
				foreach (var file in sortedFiles)
				{
					var item = new ListViewItem(file.Name)
					           {
						           ImageKey = @"unknown_file"
					           };

					using (ShellObject shellItem = ShellObject.FromParsingName(file.PhysicalFileSystemPath))
					{
						string thumbKey = file.FullPath;

						// Assign a thumbnail/icon for the file.
						if (_currentView == FileViews.Large)
						{
							if (!imagesFilesLarge.Images.ContainsKey(thumbKey))
							{
								Bitmap thumbNail = GetThumbNail(shellItem, iconSize);

								if (thumbNail != null)
								{
									imagesFilesLarge.Images.Add(thumbKey, thumbNail);
								}
							}
						}
						else
						{
							if (!imagesFilesSmall.Images.ContainsKey(thumbKey))
							{
								Bitmap thumbNail = GetThumbNail(shellItem, iconSize);

								if (thumbNail != null)
								{
									imagesFilesSmall.Images.Add(thumbKey, thumbNail);
								}
							}
						}

						item.ImageKey = thumbKey;
					}

					if (listFiles.View == View.Details)
					{
						item.SubItems.Add(file.CreateDate.ToString(CultureInfo.CurrentUICulture.DateTimeFormat));
						item.SubItems.Add(file.Size.FormatMemory());
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

				using (System.Drawing.Graphics g = comboFilters.CreateGraphics())
				{
					panelFilters.Visible = true;

					foreach (var extension in _fileExtensions)
					{
						comboFilters.Items.Add(new FilterItem(extension));

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
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

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

			_fileExtensions = extensions;
			_sort = new Dictionary<SortColumn, SortOrder>
			        {
				        {
					        SortColumn.Name, SortOrder.Ascending
				        },
				        {
					        SortColumn.Date, SortOrder.Ascending
				        },
				        {
					        SortColumn.Size, SortOrder.Ascending
				        }
			        };
		}
		#endregion

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

				foreach(ToolStripMenuItem oldItem in buttonView.DropDownItems)
				{
					oldItem.Checked = false;
				}
				item.Checked = true;

				if (item == itemViewDetails)
				{
					listFiles.View = View.Details;
					_currentView = FileViews.Details;
				}
				else if (item == itemViewList)
				{
					listFiles.View = View.List;
					_currentView = FileViews.List;
				}
				else
				{
					listFiles.View = View.LargeIcon;
					_currentView = FileViews.Large;
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
			}
		}
	}
}
