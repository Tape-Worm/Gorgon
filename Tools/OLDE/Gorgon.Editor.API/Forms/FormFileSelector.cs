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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Design;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// A file selector window for retrieving files from the mounted scratch area.
	/// </summary>
	partial class FormEditorFileSelector 
		: FlatForm
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
			[Browsable(true), LocalCategory(typeof(APIResources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(APIResources), "PROP_CUETEXT_DESC")]
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

		/// <summary>
		/// A comparer for file dialog entries.
		/// </summary>
		private class DialogFileEntryEqualityComparer
			: IEqualityComparer<DialogFileEntry>
		{
			#region IEqualityComparer<DialogFileEntry> Members
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type <see cref="DialogFileEntry"/> to compare.</param>
			/// <param name="y">The second object of type <see cref="DialogFileEntry"/> to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			public bool Equals(DialogFileEntry x, DialogFileEntry y)
			{
				return x.File == y.File || string.Equals(x.File.FilePath, y.File.FilePath, StringComparison.OrdinalIgnoreCase);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <param name="obj">The object.</param>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public int GetHashCode(DialogFileEntry obj)
			{
				return obj.GetHashCode();
			}
			#endregion
		}
		#endregion

		#region Value Types.
		/// <summary>
		/// A file system entry for the dialog.
		/// </summary>
		private struct DialogFileEntry
		{
			/// <summary>
			/// Editor file for this entry.
			/// </summary>
			public readonly EditorFile File;
			/// <summary>
			/// Physical file for the dialog file entry.
			/// </summary>
			public readonly GorgonFileSystemFileEntry PhysicalFile;
			/// <summary>
			/// Date/time of file creation.
			/// </summary>
			public readonly DateTime CreateDate;
			/// <summary>
			/// Size of the file, in bytes.
			/// </summary>
			public readonly long Size;

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode()
			{
				return File == null ? 0 : File.FilePath.GetHashCode();
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="DialogFileEntry"/> struct.
			/// </summary>
			/// <param name="entry">The file system entry to evaluate.</param>
			public DialogFileEntry(GorgonFileSystemFileEntry entry)
			{
				PhysicalFile = entry;
				File = EditorMetaDataFile.Files[entry.FullPath];
				Size = entry.Size;
				CreateDate = entry.CreateDate;
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
		private readonly static List<string> _fileComboList = new List<string>();						// List of previously selected files.
		private readonly static List<string> _searchAutoComplete = new List<string>();					// Auto complete for our search text box.
		private TreeNodeDirectory _rootDirectory;														// The root directory node.
		private TreeNodeDirectory _selectedDirectory;													// The currently selected directory node.
		private int _fillTreeLock;																		// Lock used to keep the call to fill the tree from being re-entrant.
		private SortOrder _sort = SortOrder.Ascending;													// Current sort direction.
		private SortColumn _currentSortColumn = SortColumn.Name;										// Currently sorted column.
		private readonly IList<string> _fileTypes;														// The list of file types to filter.
		private readonly HashSet<DialogFileEntry> _thumbNailFiles;										// Thumbnails awaiting loading.
		private CancellationTokenSource _cancelSource;													// Cancellation source.
		private readonly List<GorgonFileSystemDirectory> _paths = new List<GorgonFileSystemDirectory>();// The selected paths.
		private Size _thumbNailSize;																	// Thumbnail size.
		private int _pathIndex;																			// The current path index.
		private readonly List<DialogFileEntry> _selectedFiles;											// The list of selected files.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the default file type to use.
		/// </summary>
		public string DefaultFileType
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the default file name.
        /// </summary>
	    public string DefaultFilename
	    {
	        get;
	        set;
	    }

		/// <summary>
		/// Property to set or return the currently selected file view mode.
		/// </summary>
		public FileViews CurrentView
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

        /// <summary>
        /// Property to set or return whether to allow multiple selection.
        /// </summary>
	    public bool AllowMultipleSelection
	    {
            get
            {
                return listFiles.MultiSelect;
            }
            set
            {
                listFiles.MultiSelect = value;
            }
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the TextChanged event of the comboFile control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboFile_TextChanged(object sender, EventArgs e)
		{
			listFiles.ItemSelectionChanged -= listFiles_ItemSelectionChanged;

			try
			{
				listFiles.SelectedItems.Clear();
				ValidateControls();
			}
			finally
			{
				listFiles.ItemSelectionChanged += listFiles_ItemSelectionChanged;
			}
		}

		/// <summary>
        /// Function to close the search dialog.
        /// </summary>
	    private void CloseSearch()
        {
            labelNoFilesFound.Visible = false;
            splitFiles.Panel1Collapsed = false;
            panelSearchLabel.Visible = false;
            FillFiles(_selectedDirectory == null ? _rootDirectory.Directory : _selectedDirectory.Directory);
	        textSearch.Text = string.Empty;
        }

		/// <summary>
		/// Handles the KeyUp event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void listFiles_KeyUp(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode != Keys.Escape)
				|| (!panelSearchLabel.Visible))
			{
				return;
			}

			try
			{
				CloseSearch();
				CancelButton = buttonCancel;
			}
			finally			
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Enter event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void listFiles_Enter(object sender, EventArgs e)
		{
			if (!panelSearchLabel.Visible)
			{
				return;
			}

			CancelButton = null;
		}

		/// <summary>
		/// Handles the Leave event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void listFiles_Leave(object sender, EventArgs e)
		{
			CancelButton = buttonCancel;
		}

        /// <summary>
        /// Handles the Click event of the buttonCloseSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonCloseSearch_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                CloseSearch();
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
		/// Handles the Click event of the buttonSearch control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonSearch_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				FillFilesSearch(_selectedDirectory.Directory);
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
		/// Handles the Enter event of the textSearch control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void textSearch_Enter(object sender, EventArgs e)
		{
			AcceptButton = null;
			if (sender != buttonSearch)
			{
				CancelButton = null;
			}
		}

		/// <summary>
		/// Handles the Leave event of the textSearch control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void textSearch_Leave(object sender, EventArgs e)
		{
			AcceptButton = buttonOK;
			CancelButton = buttonCancel;
		}

		/// <summary>
		/// Handles the KeyUp event of the textSearch control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void textSearch_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Enter:
					Cursor.Current = Cursors.WaitCursor;

					try
					{
						FillFilesSearch(_selectedDirectory.Directory);
					}
					catch (Exception ex)
					{
						GorgonDialogs.ErrorBox(this, ex);
					}
					finally
					{
						e.Handled = true;
						Cursor.Current = Cursors.Default;
						ValidateControls();
					}
					break;
				case Keys.Escape:
					if (panelSearchLabel.Visible)
					{
						CloseSearch();
					}
					else
					{
						textSearch.Text = string.Empty;
					}
					listFiles.Focus();
					break;
			}
		}

		/// <summary>
		/// Function to populate the list view with file entries.
		/// </summary>
		/// <param name="files">File entries used to populate the list view.</param>
		/// <param name="doNotClear"><b>true</b> to disable the clearing of items in the list, <b>false</b> to clear the items in the list.</param>
		private void FillListView(IEnumerable<DialogFileEntry> files, bool doNotClear)
		{
			if (!doNotClear)
			{
				listFiles.BeginUpdate();
				listFiles.Items.Clear();
			}

			try
			{
				comboFile.AutoCompleteCustomSource.Clear();

				// Add to the list view.
				foreach (var file in files)
				{
					var item = new ListViewItem(file.File.Filename)
					           {
						           Name = file.File.FilePath,
						           ImageKey = @"unknown_file",
						           Tag = file
					           };

					// Add the files in this directory to the auto complete of the filename textbox.
					comboFile.AutoCompleteCustomSource.Add(file.File.Filename);

					if (CurrentView != FileViews.Large)
					{
						using (ContentPlugIn plugIn = ContentManagement.GetContentPlugInForFile(file.File))
						{
							if (plugIn != null)
							{
								item.ImageKey = plugIn.Name;
							}

							item.SubItems.Add(file.CreateDate.ToString(CultureInfo.CurrentUICulture.DateTimeFormat));
							item.SubItems.Add(file.Size.FormatMemory());
						}
					}
					else
					{
						// Assign the full path to the tool tips.
						if (panelSearchLabel.Visible)
						{
							listFiles.ShowItemToolTips = true;
							item.ToolTipText = file.File.FilePath;
						}
						else
						{
							listFiles.ShowItemToolTips = false;
						}

						// This will queue up files to extract thumbnails from.
						// The thumbnails will be extracted via a background thread which will load the content file
						// and fill in the list view item image when the thumbnail creation is complete.
						// This thread will run for a maximum of 10 minutes.  If all thumbnails are not generated in that time,
						// then they will be generated on the next refresh (as they'll still be in the queue).
						if (imagesFilesLarge.Images.ContainsKey(file.File.FilePath))
						{
							// We already loaded this guy, so let's not add it to the queue.
							item.ImageKey = file.File.FilePath;
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
				if (!doNotClear)
				{
					listFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
					listFiles.EndUpdate();
				}
			}
		}

        /// <summary>
        /// Function to clear the thumbnail cache.
        /// </summary>
	    private void ClearThumbNailCache()
        {
            // If we have over 256 large images cached, then clear the cache.
            if (imagesFilesLarge.Images.Count <= 256)
            {
                return;
            }

            while (imagesFilesLarge.Images.Count > 3)
            {
                imagesFilesLarge.Images[imagesFilesLarge.Images.Count - 1].Dispose();
                imagesFilesLarge.Images.RemoveAt(imagesFilesLarge.Images.Count - 1);
            }
        }

	    /// <summary>
        /// Function to look up files using the search dialog.
        /// </summary>
        /// <param name="rootDirectory">The directory that be the starting point of the search.</param>
	    private async void FillFilesSearch(GorgonFileSystemDirectory rootDirectory)
        {
			if (textSearch.TextLength == 0)
			{
				if (panelSearchLabel.Visible)
				{
					CloseSearch();
				}
				return;
			}

	        if (rootDirectory == null)
	        {
		        rootDirectory = _rootDirectory.Directory;
	        }

			// Turn off the tree.
			splitFiles.Panel1Collapsed = true;
			panelSearchLabel.Visible = true;
			textSearch.AutoCompleteCustomSource.Add(textSearch.Text);

            // If we have over 256 large images cached, then clear the cache.
            ClearThumbNailCache();

			// Sort and filter our files.
		    IEnumerable<DialogFileEntry> files = GetSortedFiles(ScratchArea.ScratchFiles.FindFiles(rootDirectory.FullPath, textSearch.Text, true));
			FillListView(files, false);

	        if (listFiles.Items.Count == 0)
	        {
		        labelNoFilesFound.Visible = true;
		        labelNoFilesFound.Text = string.Format(APIResources.GOREDIT_TEXT_SEARCH_NO_FILES, textSearch.Text);
		        labelNoFilesFound.BringToFront();
	        }
	        else
	        {
				labelNoFilesFound.Visible = false;
	        }

	        listFiles.Focus();

			// Launch a thread to retrieve the thumbnails.
			if ((CurrentView != FileViews.Large) || (_cancelSource != null) || (listFiles.Items.Count == 0))
			{
				return;
			}

			// Get the thumbnails on a separate thread.  This way loading the thumbnails won't be a painfully slow process.
			_cancelSource = new CancellationTokenSource(600000);
			await Task.Factory.StartNew(GetThumbNails, _cancelSource.Token);
        }

        /// <summary>
		/// Function to find the file system directory attached to the tree node.
		/// </summary>
		/// <param name="directory">Directory to look up.</param>
		/// <returns>The node for the directory in the tree.</returns>
		private TreeNodeDirectory FindDirectoryNode(GorgonFileSystemDirectory directory)
		{
		    if (directory == null)
		    {
		        return _rootDirectory;
            }

			var parents = directory.GetParents();
			var parentNode = _rootDirectory;

			if (parents == null)
			{
				return _rootDirectory;
			}

			// Scan the root directory first.
			if (!parentNode.IsExpanded)
			{
				parentNode.Expand();
			}

			var currentNode = parentNode.Nodes.Cast<TreeNodeDirectory>().FirstOrDefault(item => item.Directory == directory);

			if (currentNode != null)
			{
				return currentNode;
			}

			// Get the parent directories.
			foreach(var parentDirectory in parents.Where(item => item != _rootDirectory.Directory).Reverse())
			{
				// Find the parent directory.
				parentNode = parentNode.Nodes.Cast<TreeNodeDirectory>().FirstOrDefault(item => item.Directory == parentDirectory);

				if (parentNode == null)
				{
					break;
				}

				if (!parentNode.IsExpanded)
				{
					parentNode.Expand();
				}

				currentNode = parentNode.Nodes.Cast<TreeNodeDirectory>().FirstOrDefault(item => item.Directory == directory);

				if (currentNode != null)
				{
					return currentNode;
				}
			}

			return null;
		}

		/// <summary>
		/// Handles the Click event of the buttonForward control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonForward_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				treeDirectories.SelectedNode = _selectedDirectory = FindDirectoryNode(_paths[++_pathIndex]);

				// Clear the selected file if selecting a new directory.
				comboFile.Text = string.Empty;
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
		/// Handles the Click event of the buttonBack control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonBack_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				treeDirectories.SelectedNode = _selectedDirectory = FindDirectoryNode(_paths[--_pathIndex]);
				
				// Clear the selected file if selecting a new directory.
				comboFile.Text = string.Empty;
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
			AddPath(_selectedDirectory);
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
		/// Handles the ItemSelectionChanged event of the listFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ListViewItemSelectionChangedEventArgs"/> instance containing the event data.</param>
		private void listFiles_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			comboFile.TextChanged -= comboFile_TextChanged;

			try
			{
				var currentFiles = (from ListViewItem item in listFiles.SelectedItems select item.Tag)
					.OfType<DialogFileEntry>()
					.Select(fileEntry => !panelSearchLabel.Visible ? fileEntry.File.Filename : fileEntry.File.FilePath)
					.ToArray();

				if (currentFiles.Length == 0)
				{
					return;
				}

				var fileText = new StringBuilder(512);

				// Build the combo box list.
				foreach (var fileName in currentFiles)
				{
					if (fileText.Length > 0)
					{
						fileText.Append(" ");
					}

					if (currentFiles.Length > 1)
					{
						fileText.AppendFormat("\"{0}\"", fileName);
					}
					else
					{
						fileText.Append(fileName);
					}
				}

				comboFile.Text = fileText.ToString();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				comboFile.TextChanged += comboFile_TextChanged;
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

			comboFile.TextChanged -= comboFile_TextChanged;

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

				var file = (DialogFileEntry)item.Item.Tag;

				comboFile.Text = !panelSearchLabel.Visible ? file.File.Filename : file.File.FilePath;

				buttonOK.PerformClick();
			}
			catch (Exception ex)
			{

				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				comboFile.TextChanged += comboFile_TextChanged;
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

			    CurrentView = itemViewDetails.Checked ? FileViews.Details : FileViews.Large;

				if (!panelSearchLabel.Visible)
				{
					FillFiles(_selectedDirectory.Directory);
				}
				else
				{
					FillFilesSearch(_selectedDirectory == null ? _rootDirectory.Directory : _selectedDirectory.Directory);
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
		/// Function to add a path to the back/forward list.
		/// </summary>
		/// <param name="node">Node containing the directory.</param>
		private void AddPath(TreeNodeDirectory node)
		{
			if (node == null)
			{
				node = _rootDirectory;
			}

			// If we're beyond 2k worth of paths, then drop the first one.
			if (_paths.Count > 0)
			{
				if (_paths.Count + 1 > 2048)
				{
					_paths.RemoveAt(0);
				}

				// Truncate the list if our current path index is not at the end.
				while (_pathIndex != _paths.Count - 1)
				{
					_paths.RemoveAt(_paths.Count - 1);
				}
			}

			_paths.Add(node.Directory);
			_pathIndex = _paths.Count - 1;
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
					if ((e.Action == TreeViewAction.ByKeyboard)
					    || (e.Action == TreeViewAction.ByMouse))
					{
						AddPath(_selectedDirectory);
					}
					return;
				}

				_selectedDirectory = (TreeNodeDirectory)e.Node;
				FillFiles(_selectedDirectory.Directory);
				if ((e.Action == TreeViewAction.ByKeyboard)
					|| (e.Action == TreeViewAction.ByMouse))
				{
					AddPath(_selectedDirectory);
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
		/// Handles the BeforeExpand event of the treeDirectories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeDirectories_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				FillDirectoryTree((TreeNodeEditor)e.Node, false);
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
			buttonGoUp.Enabled = _selectedDirectory != null && _selectedDirectory != _rootDirectory && !panelSearchLabel.Visible;
			buttonBack.Enabled = _paths.Count > 1 && _pathIndex > 0 && !panelSearchLabel.Visible;
			buttonForward.Enabled = _paths.Count > 1 && _pathIndex < _paths.Count - 1 && !panelSearchLabel.Visible;
			comboFilters.Enabled = !panelSearchLabel.Visible;
		}

		/// <summary>
		/// Function to perform localization text substitution on any strings in the dialog.
		/// </summary>
		private void LocalizeStrings()
		{
			textSearch.CueText = APIResources.GOREDIT_TEXT_SEARCH;
			label1.Text = string.Format("{0}:", APIResources.GOREDIT_TEXT_FILE);
			itemViewDetails.Text = APIResources.GOREDIT_TEXT_DETAIL_VIEW;
			itemViewLarge.Text = APIResources.GOREDIT_TEXT_THUMBNAIL_VIEW;
			columnFileName.Text = APIResources.GOREDIT_TEXT_NAME;
			columnDate.Text = APIResources.GOREDIT_TEXT_DATE;
			columnSize.Text = APIResources.GOREDIT_TEXT_SIZE;
			buttonView.Text = APIResources.GOREDIT_TEXT_CHANGEVIEW;
			buttonGoUp.Text = APIResources.GOREDIT_TEXT_GO_UP_TO_PARENT;
			buttonOK.Text = APIResources.GOREDIT_ACC_TEXT_OK;
			buttonBack.Text = APIResources.GOREDIT_TEXT_BACK;
			buttonForward.Text = APIResources.GOREDIT_TEXT_GO_FORWARD;
			buttonCancel.Text = APIResources.GOREDIT_ACC_TEXT_CANCEL;
			labelSearchBanner.Text = APIResources.GOREDIT_TEXT_SEARCH_RESULTS;
		}

		/// <summary>
		/// Function to fill in the directory tree.
		/// </summary>
		/// <param name="root">The root node of the tree to fill.</param>
		/// <param name="setInitialSelected"><b>true</b> to set the initially selected node, <b>false</b> to sjip it.</param>
		private void FillDirectoryTree(TreeNodeEditor root, bool setInitialSelected)
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
				}

				if (setInitialSelected)
				{
				    if (!string.IsNullOrWhiteSpace(StartDirectory))
				    {
				        selectedNode = FindDirectoryNode(ScratchArea.ScratchFiles.GetDirectory(StartDirectory));
				    }

					treeDirectories.SelectedNode = _selectedDirectory = selectedNode;
					AddPath(selectedNode);
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
		/// <param name="directoryFiles">Directory containing the files.</param>
		/// <returns>A sorted list of files.</returns>
		private IEnumerable<DialogFileEntry> GetSortedFiles(IEnumerable<GorgonFileSystemFileEntry> directoryFiles)
		{
			string fileType = string.Empty;
			var editorFileList = from file in directoryFiles
			                     where EditorMetaDataFile.Files.Contains(file.FullPath)
			                     select new DialogFileEntry(file);

			if (!panelSearchLabel.Visible)
			{
				if (comboFilters.SelectedItem != null)
				{
					fileType = comboFilters.SelectedItem.ToString();
				}

				// Apply file type filter.
				if ((!string.IsNullOrWhiteSpace(fileType))
				    && (!string.Equals(fileType, APIResources.GOREDIT_TEXT_ALL_FILES, StringComparison.CurrentCultureIgnoreCase)))
				{
					editorFileList = from file in editorFileList
					                 where file.File.Attributes.ContainsKey("Type")
					                       && string.Equals(file.File.Attributes["Type"], fileType, StringComparison.CurrentCultureIgnoreCase)
					                 select file;
				}
			}

			if (listFiles.View == View.Details)
			{
				switch (_currentSortColumn)
				{
					case SortColumn.Name:
						switch (_sort)
						{
							case SortOrder.Ascending:
								return from file in editorFileList
								       orderby file.File.Filename.ToLower(CultureInfo.CurrentUICulture)
								       select file;
							case SortOrder.Descending:
								return from file in editorFileList
									   orderby file.File.Filename.ToLower(CultureInfo.CurrentUICulture) descending
								       select file;
						}
						break;
					case SortColumn.Date:
						switch (_sort)
						{
							case SortOrder.Ascending:
								return from file in editorFileList
								       orderby file.CreateDate
								       select file;
							case SortOrder.Descending:
								return from file in editorFileList
								       orderby file.CreateDate descending
								       select file;
						}
						break;
					case SortColumn.Size:
						switch (_sort)
						{
							case SortOrder.Ascending:
								return from file in editorFileList
								       orderby file.Size
								       select file;
							case SortOrder.Descending:
								return from file in editorFileList
								       orderby file.Size descending
								       select file;
						}
						break;
				}
			}
			else
			{
				return editorFileList.OrderBy(item => item.File.Filename.ToLower(CultureInfo.CurrentUICulture));
			}

			return editorFileList;
		}

		/// <summary>
		/// Function to retrieve the thumbnail for content.
		/// </summary>
		/// <param name="file">File that holds the content.</param>
		/// <returns>The thumbnail bitmap for the content.</returns>
		private Image GetContentThumbNail(DialogFileEntry file)
		{
			// If we have this open in the editor, then use that so we get unsaved changes as well.
			if ((ContentManagement.ContentFile == file.PhysicalFile) && (ContentManagement.Current != null))
			{
				return ContentManagement.Current.HasThumbnail ? (Image)ContentManagement.Current.GetThumbNailImage().Clone() : null;
			}

			ContentPlugIn plugIn = ContentManagement.GetContentPlugInForFile(file.File);

			if (plugIn == null)
			{
				return null;
			}

			ContentSettings settings = plugIn.GetContentSettings();
			settings.Name = file.File.Filename;
			settings.CreateContent = false;

			// We need to load the content so we can generate a thumbnail (or retrieve the cached version).
			using (ContentObject content = plugIn.CreateContentObject(settings))
			{
				if (!content.HasThumbnail)
				{
					return null;
				}

				// Bind to the editor file information.
				content.EditorFile = file.File;

				using (Stream contentStream = file.PhysicalFile.OpenStream(false))
				{
					content.Read(contentStream);
				}

				Image scaledThumbNail;

				using (Image result = content.GetThumbNailImage())
				{
					// If the image isn't the correct size then we need to rescale it.
					if ((result.Width == _thumbNailSize.Width) && (result.Height == _thumbNailSize.Height))
					{
						return (Image)result.Clone();
					}

					scaledThumbNail = new Bitmap(_thumbNailSize.Width, _thumbNailSize.Height, PixelFormat.Format32bppArgb);

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
				DialogFileEntry item = _thumbNailFiles.First();

				try
				{
					if (_cancelSource.Token.IsCancellationRequested)
					{
						return;
					}

					try
					{
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

														 // Attempt to remove this item.
								                         if (!_thumbNailFiles.Contains(item))
								                         {
									                         return;
								                         }

								                         _thumbNailFiles.Remove(item);

								                         if (!imagesFilesLarge.Images.ContainsKey(item.File.FilePath))
								                         {
									                         imagesFilesLarge.Images.Add(item.File.FilePath, thumbNail);
								                         }

								                         // If the list view no longer contains this item, then we should dump it.
								                         // It'll still be cached in our image list for later.
								                         if (listFiles.Items.ContainsKey(item.File.FilePath))
								                         {
									                         listFiles.Items[item.File.FilePath].ImageKey = item.File.FilePath;
								                         }
							                         }));
						}
					}
#if DEBUG
					catch (Exception ex)
#else
					catch
#endif
					{
						if (_cancelSource.Token.IsCancellationRequested)
						{
							return;
						}

						Invoke(new MethodInvoker(() =>
						                         {
#if DEBUG
							                         GorgonDialogs.ErrorBox(this, string.Format(APIResources.GOREDIT_DLG_COULD_NOT_LOAD_THUMBNAIL,
							                                                              item.File.FilePath), null, ex);
#endif

													 if (!imagesFilesLarge.Images.ContainsKey(item.File.FilePath))
													 {
														 imagesFilesLarge.Images.Add(item.File.FilePath, (Bitmap)APIResources.image_missing_128x128.Clone());
													 }

													 if (listFiles.Items.ContainsKey(item.File.FilePath))
							                         {
														 listFiles.Items[item.File.FilePath].ImageKey = item.File.FilePath;
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

			    listFiles.View = itemViewDetails.Checked ? View.Details : View.LargeIcon;

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

				IEnumerable<DialogFileEntry> sortedFiles = GetSortedFiles(directory.Files);

				// If we have over 256 large images cached, then clear the cache.
                ClearThumbNailCache();
			
				// Add the files
				FillListView(sortedFiles, true);
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
			
			if (_fileTypes.Count == 0)
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
					foreach (string filter in _fileTypes)
					{
						comboFilters.Items.Add(filter);

						if (string.Equals(filter, DefaultFileType, StringComparison.CurrentCultureIgnoreCase))
						{
							comboFilters.Text = filter;
						}
					}
					
					// Determine maximum combo box drop down width.
					maxWidth = _fileTypes.Aggregate(maxWidth,
					                                (current, fileType) =>
					                                current.Max(TextRenderer.MeasureText(g, fileType, comboFilters.Font)
					                                                        .Width));

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
		/// Function to retrieve the selected files from the view.
		/// </summary>
		/// <returns><b>true</b> if the selected files exist, <b>false</b> if not.</returns>
		private bool GetSelectedFiles()
		{
			if (string.IsNullOrWhiteSpace(comboFile.Text))
			{
				return true;
			}

			IEnumerable<string> files = GetFilenamesFromComboBox(comboFile.Text);
			
			_selectedFiles.Clear();

			foreach (var file in files)
			{
				string directoryName = Path.GetDirectoryName(file).FormatDirectory('/');
				string fileName = Path.GetFileName(file).FormatFileName();

				if (string.IsNullOrWhiteSpace(fileName))
				{
					GorgonDialogs.ErrorBox(this, string.Format(APIResources.GOREDIT_DLG_NO_FILE_IN_PATH, directoryName));
					return false;
				}

				if (!string.IsNullOrWhiteSpace(directoryName))
				{
					GorgonFileSystemDirectory directoryEntry = ScratchArea.ScratchFiles.GetDirectory(directoryName);

					if (directoryEntry == null)
					{
						GorgonDialogs.ErrorBox(this, string.Format(APIResources.GOREDIT_DLG_NO_SUCH_DIRECTORY, directoryName));
						return false;
					}
				}
				else
				{
					if (!panelSearchLabel.Visible)
					{
						directoryName = _selectedDirectory.Directory.FullPath;
					}
					else
					{
						string selectedFileName = file;

						directoryName = (from item in listFiles.Items.Cast<ListViewItem>() where item.Tag is DialogFileEntry select item.Tag)
							.OfType<DialogFileEntry>()
							.Where(item => (string.Equals(item.File.Filename, fileName, StringComparison.OrdinalIgnoreCase)
							                || string.Equals(item.File.FilePath, selectedFileName, StringComparison.OrdinalIgnoreCase)))
							.Select(item => item.PhysicalFile.Directory.FullPath).FirstOrDefault();

						// If we can't find the directory, then default to the root.
						if (string.IsNullOrWhiteSpace(directoryName))
						{
							directoryName = "/";
						}
					}
				}

				GorgonFileSystemFileEntry fileEntry = ScratchArea.ScratchFiles.GetFile(directoryName + fileName);

				if (fileEntry == null)
				{
					GorgonDialogs.ErrorBox(this, string.Format(APIResources.GOREDIT_DLG_NO_SUCH_FILE, directoryName, fileName));
					return false;
				}

				_selectedFiles.Add(new DialogFileEntry(fileEntry));
			}

			foreach (var file in _selectedFiles.Where(file => string.IsNullOrWhiteSpace(_fileComboList
				                                                                            .Find(searchValue =>
				                                                                                  string.Equals(searchValue,
				                                                                                                file.File.FilePath,
				                                                                                                StringComparison.OrdinalIgnoreCase)))))
			{
				_fileComboList.Add(file.File.FilePath);
			}

			return true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if ((DialogResult == DialogResult.OK) && (!GetSelectedFiles()))
			{
				e.Cancel = true;
				return;
			}
			
			comboFilters.SelectedIndexChanged -= comboFilters_SelectedIndexChanged;

			_searchAutoComplete.Clear();
			foreach (string item in textSearch.AutoCompleteCustomSource)
			{
				_searchAutoComplete.Add(item);
			}

			// Retrieve the selected extension.
			if (comboFilters.SelectedItem != null)
			{
				DefaultFileType = comboFilters.SelectedItem.ToString();
			}

			try
			{
			    if (_cancelSource == null)
			    {
			        return;
			    }

			    Cursor.Current = Cursors.WaitCursor;

			    _cancelSource.Cancel();
                
			    // Ensure that the thumbnail thread is exited.
			    while (_cancelSource != null)
			    {
			        Thread.Sleep(0);
			    }
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to retrieve the filenames specified in the file name box.
		/// </summary>
		/// <param name="fileNameList">The list of file names.</param>
		/// <returns>An array containing the selected file names.</returns>
		private static IEnumerable<string> GetFilenamesFromComboBox(string fileNameList)
		{
			if (string.IsNullOrWhiteSpace(fileNameList))
			{
				return new string[0];
			}

			var fileNameString = new StringBuilder(fileNameList.Trim());
			var fileNames = new List<string>();
			string fileName;

			if (fileNameString.Length == 0)
			{
				return fileNames;
			}

			// Get rid of any weird things like double quotes.
			// This could potentially leave a single quote in the middle of the string.
			// The parsing code below will filter these out.
			while (fileNameString.IndexOf("\"\"", StringComparison.Ordinal) != -1)
			{
				fileNameString.Replace("\"\"", "\"");
			}

			// We've specified multiple files.
			if (fileNameString.IndexOf("\" ", StringComparison.Ordinal) != -1)
			{
				// Split on spaces.
				fileName = fileNameString.ToString();
				fileNames.AddRange(fileName.Split(new[]
				                                  {
					                                  "\" "
				                                  },
				                                  StringSplitOptions.RemoveEmptyEntries));

				// Sanitize our file names.
				for (int i = 0; i < fileNames.Count; ++i)
				{
					fileNameString.Length = 0;
					fileNameString.Append(fileNames[i]);
					fileNameString.Replace("\"", string.Empty);

					fileNames[i] = fileNameString.ToString().Trim();
				}

				return fileNames.Where(item => !string.IsNullOrWhiteSpace(item));
			}

			// Return the sanitized single file name.
			fileNameString.Replace("\"", string.Empty);
			fileName = fileNameString.ToString();

			// Do not allow an empty file name after sanitization.
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return fileNames;
			}

			return new[]
				    {
					    fileName
				    };
		}

        /// <summary>
        /// Function to set the default file name.
        /// </summary>
	    private void SetDefaultFilename()
        {
	        if (string.IsNullOrWhiteSpace(DefaultFilename))
            {
                return;
            }

			var fileText = new StringBuilder(DefaultFilename.Length);

			// If we have multiple file names, then we need to find the files in the selected directory.
			// Note, that if we have multiple directories selected, then the first directory will be selected.
			// (Provided there is a directory to select).
	        string[] fileNames = GetFilenamesFromComboBox(DefaultFilename.Trim()).ToArray();

	        if (fileNames.Length == 0)
	        {
		        return;
	        }

			// Get the first directory.
			string directoryName = Path.GetDirectoryName(fileNames[0]).FormatDirectory('/');
			GorgonFileSystemDirectory directory = null;

			if (!string.IsNullOrWhiteSpace(directoryName))
			{
				directory = ScratchArea.ScratchFiles.GetDirectory(directoryName);
			}

			listFiles.ItemSelectionChanged -= listFiles_ItemSelectionChanged;
	        try
	        {
		        foreach (var fileItem in fileNames)
		        {
			        if (fileText.Length > 0)
			        {
				        fileText.Append(" ");
			        }

			        if (fileNames.Length > 1)
			        {
				        fileText.AppendFormat("\"{0}\"", fileItem);
			        }
			        else
			        {
				        fileText.Append(fileItem);
			        }

			        string fileName = Path.GetFileName(fileItem).FormatFileName();
			        GorgonFileSystemFileEntry file = null;

			        if (!string.IsNullOrWhiteSpace(fileName))
			        {
				        file = ScratchArea.ScratchFiles.GetFile(directoryName + fileName);
			        }

			        if ((directory != null) && ((_selectedDirectory == null) || (directory != _selectedDirectory.Directory)))
			        {
				        treeDirectories.SelectedNode = _selectedDirectory = FindDirectoryNode(directory);
			        }

			        if (file == null)
			        {
				        break;
			        }

			        var selectedItem = listFiles.Items.Cast<ListViewItem>()
			                                    .FirstOrDefault(item => string.Equals(item.Text,
			                                                                          file.Name,
			                                                                          StringComparison.OrdinalIgnoreCase));

			        if (selectedItem != null)
			        {
				        selectedItem.Selected = true;
			        }
		        }

		        comboFile.TextChanged -= comboFile_TextChanged;
		        comboFile.Text = fileText.ToString();
		        comboFile.TextChanged += comboFile_TextChanged;
	        }
	        finally
	        {
		        listFiles.ItemSelectionChanged += listFiles_ItemSelectionChanged;
	        }
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
                if (CurrentView == FileViews.Details)
                {
                    itemViewDetails.Checked = true;
                    itemViewLarge.Checked = false;
                }
                else
                {
                    itemViewDetails.Checked = false;
                    itemViewLarge.Checked = true;
                }

				// Copy previously selected files into the combo box if the file exists in the current file system.
				foreach (var item in _fileComboList.Where(item => ScratchArea.ScratchFiles.GetFile(item) != null))
				{
					comboFile.Items.Add(item);
				}

				// Copy the auto complete back into the search box.
				foreach (var item in _searchAutoComplete)
				{
					textSearch.AutoCompleteCustomSource.Add(item);
				}

				columnFileName.Tag = SortColumn.Name;
				columnDate.Tag = SortColumn.Date;
				columnSize.Tag = SortColumn.Size;

				FillIconList();

				FillFilterList();

				// Default to sorted by name.
				listFiles.SetSortIcon(0, SortOrder.Ascending);

				FillDirectoryTree(null, true);

                SetDefaultFilename();
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
        /// Function to return the list of selected file names.
        /// </summary>
        /// <returns>An array containing the paths of all the selected files.</returns>
        public EditorFile[] GetFilenames()
        {
            return _selectedFiles.Select(item => item.File).ToArray();
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormEditorFileSelector"/> class.
		/// </summary>
		/// <param name="extensions">File extensions used to filter the file list.</param>
		/// <param name="defaultFileType">The default file type to use.</param>
		public FormEditorFileSelector(IList<string> extensions, string defaultFileType)
		{
			InitializeComponent();

			CurrentView = FileViews.Details;
			DefaultFileType = defaultFileType;
			_fileTypes = extensions;
			_thumbNailFiles = new HashSet<DialogFileEntry>(new DialogFileEntryEqualityComparer());
			_thumbNailSize = new Size(128, 128);
			_selectedFiles = new List<DialogFileEntry>(); 
		}
		#endregion
	}
}
