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
// Created: Monday, April 30, 2012 6:28:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application object.
	/// </summary>
	partial class formMain
		: ZuneForm
	{
		#region Value Types.
		/// <summary>
		/// The object being cut or copied.
		/// </summary>
		private struct CutCopyObject
		{
			#region Variables.
			/// <summary>
			/// Node being copied or cut.
			/// </summary>
			public readonly EditorTreeNode Node;

			/// <summary>
			/// Object being copied or cut.
			/// </summary>
			public readonly object Object;

			/// <summary>
			/// Flag to indicate that the object is being cut.
			/// </summary>
			public readonly bool IsCut;
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="CutCopyObject"/> struct.
			/// </summary>
			/// <param name="node">The node.</param>
			/// <param name="copyObject">The copy object.</param>
			/// <param name="isCut">TRUE if the object is being cut instead of copied, FALSE if not.</param>
			public CutCopyObject(EditorTreeNode node, object copyObject, bool isCut)
			{
				Node = node;
				Object = copyObject;
				IsCut = isCut;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private static int _syncCounter;										// Synchronization counter for multiple threads.
		private RootNodeDirectory _rootNode;									// Our root node for the tree.
		private readonly char[] _fileChars = Path.GetInvalidFileNameChars();	// Invalid filename characters.
		private CutCopyObject? _cutCopyObject;									// Cut/copy object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the file that is currently open.
		/// </summary>
		public GorgonFileSystemFileEntry CurrentOpenFile
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Called when [property value changed].
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="e">The <see cref="PropertyValueChangedEventArgs"/> instance containing the event data.</param>
		private void OnPropertyValueChanged(object o, PropertyValueChangedEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				ContentManagement.UpdateProperties(e);
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
        /// Handles the Click event of the itemPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemPreferences_Click(object sender, EventArgs e)
        {
            formPrefs prefs = null;

            try
            {
                prefs = new formPrefs();
                prefs.ShowDialog(this);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                if (prefs != null)
                {
                    prefs.Dispose();
                }
            }        
        }

		/// <summary>
		/// Function to assign extensions to the file open/save dialogs.
		/// </summary>
		/// <param name="dialog">Dialog to update.</param>
		/// <param name="extensions">Extensions to use as a filter.</param>
		/// <param name="includeAllFiles">TRUE to include all files in the filter, FALSE to exclude.</param>
		/// <param name="allSupportedDesc">The description to apply to the all supported files line.  Setting this value will add a filter for all supported file types.</param>
		/// <param name="defaultExt">The default extension to append if none is given.</param>
		/// <param name="defaultFilterExpr">The expression used to determine the default filter.</param>
		private static void AssignFilter(FileDialog dialog, 
								IEnumerable<GorgonFileExtension> extensions, 
								bool includeAllFiles = false, 
								string allSupportedDesc = null, 
								GorgonFileExtension? defaultExt = null, 
								Func<GorgonFileExtension, bool> defaultFilterExpr = null)
		{
			var allTypes = new StringBuilder(256);
			var filter = new StringBuilder(256);
			int currentIndex = 0;

			dialog.Filter = string.Empty;
			dialog.DefaultExt = string.Empty;
			dialog.FilterIndex = 0;

			// Build the extension filter(s).
		    if (extensions != null)
		    {
		        foreach (var extension in extensions)
		        {
		            if (filter.Length > 0)
		            {
		                filter.Append("|");
		            }

		            if ((!string.IsNullOrWhiteSpace(allSupportedDesc))
		                && (allTypes.Length > 0))
		            {
		                allTypes.Append(";");
		            }

		            // If we supply a default filter expression, then apply it to the dialog box.
		            if (defaultFilterExpr != null)
		            {
		                if (defaultFilterExpr(extension))
		                {
		                    dialog.FilterIndex = currentIndex + 1;
		                    dialog.DefaultExt = extension.Extension;
		                }
		            }
		            else
		            {
		                // Assign the default extension if requested.
		                if ((defaultExt != null)
		                    && (defaultExt.Value == extension))
		                {
		                    dialog.DefaultExt = defaultExt.Value.Extension;
		                }
		            }

		            filter.Append(extension.GetFilter());

		            if (!string.IsNullOrWhiteSpace(allSupportedDesc))
		            {
		                allTypes.AppendFormat("*.{0}", extension.Extension);
		            }

		            currentIndex++;
		        }
		    }

		    if (includeAllFiles)
			{
			    if (filter.Length > 0)
			    {
			        filter.Append("|");
			    }

				filter.AppendFormat("{0}|*.*", Resources.GOREDIT_ALL_FILES);
			}

			// If we do have multiple file types, and a description, then update.
			if ((!string.IsNullOrWhiteSpace(allSupportedDesc)) && (allTypes.ToString().Contains(";")))
			{
				dialog.Filter = string.Format("{0}|{1}|{2}", allSupportedDesc, allTypes, filter);
			}
			else
			{
				dialog.Filter = filter.ToString();
			}
		}

        /// <summary>
        /// Handles the Click event of the itemImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemImport_Click(object sender, EventArgs e)
        {
            try
            {
                AssignFilter(dialogImport, ContentManagement.GetContentExtensions(), true, Resources.GOREDIT_ALL_CONTENT);

                dialogImport.InitialDirectory = Program.Settings.ImportLastFilePath;                

                // Perform import.
	            if (dialogImport.ShowDialog(this) != DialogResult.OK)
	            {
		            return;
	            }

	            TreeNodeDirectory directoryNode;

	            // Find our destination directory node.
	            if (treeFiles.SelectedNode == null)
	            {
		            directoryNode = _rootNode;
	            }
	            else
	            {
		            var parentNode = treeFiles.SelectedNode.Parent as EditorTreeNode;
		            directoryNode = treeFiles.SelectedNode as TreeNodeDirectory;
                                                
		            while ((directoryNode == null) && (parentNode != null))
		            {                            
			            directoryNode = parentNode as TreeNodeDirectory;
			            parentNode = parentNode.Parent as EditorTreeNode;
		            }
	            }

				Debug.Assert(directoryNode == null, "Directory for select node is NULL!");
                    
	            Cursor.Current = Cursors.WaitCursor;
	            AddFilesFromExplorer(directoryNode, dialogImport.FileNames);
                    
	            Program.Settings.ImportLastFilePath = Path.GetDirectoryName(dialogImport.FileNames[0]).FormatDirectory(Path.DirectorySeparatorChar);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                ValidateControls();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Function called when a file export is completed.
        /// </summary>
        /// <param name="cancelled">Flag to indicate whether the operation was cancelled or not.</param>
        /// <param name="importExport">TRUE if an import, FALSE if an export.</param>
        /// <param name="filesCopied">The number of files copied.</param>
        /// <param name="totalFiles">The total number of files to copy.</param>
	    private void FileImportExportCompleted(bool cancelled, bool importExport, int filesCopied, int totalFiles)
	    {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => FileImportExportCompleted(cancelled, importExport, filesCopied, totalFiles)));
            }
            else
            {
                int skipped = totalFiles - filesCopied;

                if (cancelled)
                {
	                GorgonDialogs.WarningBox(this,
	                                         string.Format(
	                                                       importExport
		                                                       ? Resources.GOREDIT_IMPORT_CANCELLED
		                                                       : Resources.GOREDIT_EXPORT_CANCELLED,
	                                                       filesCopied,
	                                                       totalFiles,
	                                                       skipped));
                }
                else
                {
	                GorgonDialogs.InfoBox(this,
	                                      string.Format(
	                                                    importExport
		                                                    ? Resources.GOREDIT_IMPORT_SUCCESS
		                                                    : Resources.GOREDIT_EXPORT_SUCCESSFUL,
	                                                    filesCopied,
	                                                    totalFiles,
	                                                    skipped));
                }
            }
	    }

		/// <summary>
		/// Function to display an error box if an exception occurs during a file copy operation.
		/// </summary>
		/// <param name="ex">Exception to be displayed.</param>
		private void FileCopyException(Exception ex)
		{
			Action invokeAction = () => GorgonDialogs.ErrorBox(null, ex);

			if (InvokeRequired)
			{
				Invoke(new MethodInvoker(invokeAction));
			}
			else
			{
				invokeAction();
			}
		}

		/// <summary>
		/// Function to determine if a file is able to be imported or not.
		/// </summary>
		/// <param name="file">File to evaluate.</param>
		/// <returns>TRUE if the file can be imported or FALSE if not.</returns>
		private bool EvaluateFileImport(GorgonFileSystemFileEntry file)
		{
			bool result = false;
			Action invokeAction = () =>
			                      {
				                      result = (CurrentOpenFile != file);

									  // Display an error if we hit a file we can't import.
				                      if (!result)
				                      {
					                      GorgonDialogs.ErrorBox(null,
					                                             string.Format(Resources.GOREDIT_IMPORT_FILE_OPEN_FOR_EDIT,
					                                                           file.FullPath));
				                      }
			                      };

			if (InvokeRequired)
			{
				Invoke(new MethodInvoker(invokeAction));
			}
			else
			{
				invokeAction();
			}

			return result;
		}

        /// <summary>
        /// Function to confirm a file overwrite operation.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="totalFileCount">The total number of files.</param>
        /// <returns>The result of the dialog operation.</returns>
	    private ConfirmationResult ImportConfirmFileOverwrite(string filePath, int totalFileCount)
	    {
            var result = ConfirmationResult.None;
            Action invokeAction =() => result = GorgonDialogs.ConfirmBox(null, string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
                                                                      Resources.GOREDIT_FILE_DEFAULT_TYPE,
                                                                      filePath),
                                                                totalFileCount > 1,
                                                                totalFileCount > 1);
	        if (InvokeRequired)
	        {
                Invoke(new MethodInvoker(invokeAction));
	        }
            else
	        {
	            invokeAction();
	        }

            return result;
	    }

        /// <summary>
        /// Function to confirm a file overwrite operation.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="totalFileCount">The total number of files.</param>
        /// <returns>The result of the dialog operation.</returns>
        private ConfirmationResult CopyConfirmFileOverwrite(string filePath, int totalFileCount)
        {
            var result = ConfirmationResult.None;
            Action invokeAction = () => result = GorgonDialogs.ConfirmBox(null, string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
                                                                      Resources.GOREDIT_FILE_DEFAULT_TYPE,
                                                                      filePath),
                                                                true,
                                                                totalFileCount > 1);
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(invokeAction));
            }
            else
            {
                invokeAction();
            }

            return result;
        }

        /// <summary>
        /// Function to confirm a directory overwrite operation.
        /// </summary>
        /// <param name="directoryPath">Path to the directory.</param>
        /// <param name="totalDirectoryCount">The total number of directories.</param>
        /// <returns>The result of the dialog operation.</returns>
        private ConfirmationResult CopyConfirmDirectoryOverwrite(string directoryPath, int totalDirectoryCount)
        {
            var result = ConfirmationResult.None;
            Action invokeAction = () => result = GorgonDialogs.ConfirmBox(null, string.Format(Resources.GOREDIT_OVERWRITE_DIRECTORY_PROMPT,
                                                                      directoryPath),
                                                                true,
                                                                totalDirectoryCount > 1);
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(invokeAction));
            }
            else
            {
                invokeAction();
            }

            return result;
        }

        /// <summary>
        /// Handles the Click event of the itemExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemExport_Click(object sender, EventArgs e)
        {
            try
            {
                var directoryNode = treeFiles.SelectedNode as TreeNodeDirectory;
                var fileNode = treeFiles.SelectedNode as TreeNodeFile;

                if ((directoryNode == null)
                    && (fileNode == null))
                {
                    return;
                }

                dialogExport.Description = Resources.GOREDIT_EXPORT_DLG_TITLE;
                dialogExport.SelectedPath = Program.Settings.ExportLastFilePath;
                dialogExport.ShowNewFolderButton = true;

                // Export the content data.
	            if (dialogExport.ShowDialog(this) != DialogResult.OK)
	            {
		            return;
	            }

	            Cursor.Current = Cursors.WaitCursor;

                if (fileNode != null)
                {
                    ScratchArea.Export(fileNode.File, dialogExport.SelectedPath, false);
                }
                else
                {
                    ScratchArea.Export(directoryNode.Directory, dialogExport.SelectedPath);
                }
                Program.Settings.ExportLastFilePath = dialogExport.SelectedPath.FormatDirectory(Path.DirectorySeparatorChar);
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
        /// Function to validate the controls on the display.
        /// </summary>
        private void ValidateControls()
        {
            Text = string.Format("{0} - {1}", FileManagement.Filename, Resources.GOREDIT_DEFAULT_TITLE);

			Debug.Assert(_rootNode != null, "Root node is NULL!");

			_rootNode.Redraw();

            itemOpen.Enabled = PlugIns.ReaderPlugIns.Count > 0;
            itemSaveAs.Enabled = (PlugIns.WriterPlugIns.Count > 0);
            itemSave.Enabled = !string.IsNullOrWhiteSpace(FileManagement.FilePath)
                                && (FileManagement.GetWriterPlugIn(FileManagement.FilePath) != null)
                                && ((FileManagement.FileChanged) 
									|| (ContentManagement.Changed))
								&& itemSaveAs.Enabled;

            // Check to see if the current content can export.
	        itemExport.Enabled = (_rootNode != null) && (_rootNode.Nodes.Count > 1) && (treeFiles.SelectedNode != null);
            popupItemAddContent.Visible = true;
            itemAddContent.Enabled = false;
            popupItemAddContent.Enabled = false;
            dropNewContent.Enabled = false;
            itemDelete.Enabled = popupItemDelete.Enabled = false;
            itemDelete.Text = popupItemDelete.Text = string.Format("{0}...", Resources.GOREDIT_MENU_DELETE_DEFAULT);
            itemDelete.Visible = popupItemDelete.Visible = true;
            popupItemEdit.Visible = false;
            toolStripSeparator5.Visible = true;
            popupItemRename.Visible = true;
            popupItemRename.Enabled = false;
            popupItemRename.Text = string.Format("{0}...", Resources.GOREDIT_MENU_RENAME_DEFAULT);
            popupItemCreateFolder.Visible = false;
	        itemCreateFolder.Enabled = false;
			buttonEditContent.Enabled = false;
			buttonDeleteContent.Enabled = false;
			popupItemCut.Enabled = popupItemCopy.Enabled = popupItemPaste.Enabled = itemCopy.Enabled = itemCut.Enabled = itemPaste.Enabled = false;

            menuRecentFiles.Enabled = Program.Settings.RecentFiles.Count > 0;

            var node = treeFiles.SelectedNode as EditorTreeNode;

            // No node is the same as selecting the root.
            if (treeFiles.SelectedNode == null)
            {
                node = _rootNode;
            }

            var fileNode = node as TreeNodeFile;
                                
            if (node is TreeNodeDirectory)
            {
                toolStripSeparator4.Visible = true;
                popupItemAddContent.Visible = itemAddContent.Enabled = itemAddContent.DropDownItems.Count > 0;
                popupItemAddContent.Enabled = itemAddContent.Enabled;
                dropNewContent.Enabled = dropNewContent.DropDownItems.Count > 0;
				buttonDeleteContent.Enabled = true;
				popupItemCreateFolder.Enabled = itemCreateFolder.Enabled = true;
				popupItemCreateFolder.Visible = true;
				itemDelete.Enabled = popupItemDelete.Enabled = (tabDocumentManager.SelectedTab == pageItems);
				itemDelete.Text = popupItemDelete.Text = string.Format("{0}...", Resources.GOREDIT_MENU_DELETE_FOLDER);
				popupItemPaste.Enabled = itemPaste.Enabled = (_cutCopyObject != null);
					
				if (node != _rootNode)
                {
					popupItemCut.Enabled = popupItemCopy.Enabled = itemCopy.Enabled = itemCut.Enabled = true;
					popupItemRename.Enabled = true;
                    popupItemRename.Text = string.Format("{0}...", Resources.GOREDIT_MENU_RENAME_FOLDER);
                }
                else
                {                        
					if (_rootNode.Nodes.Count == 0)
					{
                        buttonDeleteContent.Enabled = false;
						popupItemDelete.Visible = false;
						itemDelete.Enabled = false;
						itemDelete.Text = string.Format("{0}...", Resources.GOREDIT_MENU_DELETE_DEFAULT);
					}
					else
					{
						itemDelete.Text = popupItemDelete.Text = string.Format("{0}...", Resources.GOREDIT_MENU_DELETE_ALL);
					}

                    toolStripSeparator5.Visible = false;
                    popupItemRename.Visible = false;
                }                    
            }

            if (fileNode == null)
            {
                return;
            }

            GorgonFileSystemFileEntry file = fileNode.File;

            popupItemAddContent.Visible = false;
            popupItemPaste.Enabled = itemPaste.Enabled = (_cutCopyObject != null);
            popupItemCut.Enabled = popupItemCopy.Enabled = itemCopy.Enabled = itemCut.Enabled = true;
            buttonDeleteContent.Enabled = true;
            toolStripSeparator4.Visible = buttonEditContent.Enabled = popupItemEdit.Visible = popupItemEdit.Enabled = ContentManagement.CanOpenContent(file.Extension);
            itemDelete.Enabled = popupItemDelete.Enabled = (tabDocumentManager.SelectedTab == pageItems);
            popupItemRename.Enabled = true;
        }

        /// <summary>
        /// Handles the Click event of the itemEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemEdit_Click(object sender, EventArgs e)
        {
            if (treeFiles.SelectedNode == null)
            {
                return;
            }

            treeFiles_NodeMouseDoubleClick(this, new TreeNodeMouseClickEventArgs(treeFiles.SelectedNode,
                                                                           MouseButtons.Left,
                                                                           2,
                                                                           treeFiles.SelectedNode.Bounds.X,
                                                                           treeFiles.SelectedNode.Bounds.Y));
        }
        
        /// <summary>
		/// Handles the NodeMouseDoubleClick event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeNodeMouseClickEventArgs"/> instance containing the event data.</param>
		private void treeFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
            var fileNode = treeFiles.SelectedNode as TreeNodeFile;

            if (fileNode == null)
            {
                return;
            }

			// Do not re-open the file.
	        if (fileNode.File == CurrentOpenFile)
	        {
		        return;
	        }

            Cursor.Current = Cursors.WaitCursor;

			try
			{
                ContentManagement.Load(fileNode.File);
                CurrentOpenFile = fileNode.File;
                treeFiles.Refresh();
			}
			catch (Exception ex)
			{
			    CurrentOpenFile = null;
                GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void treeFiles_KeyDown(object sender, KeyEventArgs e)
		{			
			switch(e.KeyCode)
			{
				case Keys.Enter:
			        if (treeFiles.SelectedNode == null)
			        {
			            return;
			        }

			        treeFiles_NodeMouseDoubleClick(this, new TreeNodeMouseClickEventArgs(treeFiles.SelectedNode,
                                                                                   MouseButtons.Left, 2, treeFiles.SelectedNode.Bounds.X, treeFiles.SelectedNode.Bounds.Y));
			        break;
				case Keys.F2:
					if ((treeFiles.SelectedNode != null) && (popupItemRename.Enabled))
					{
						treeFiles.SelectedNode.BeginEdit();
					}
					break;
			}
		}

		/// <summary>
		/// Handles the AfterSelect event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
		private void treeFiles_AfterSelect(object sender, TreeViewEventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonEditContent control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonEditContent_Click(object sender, EventArgs e)
		{
            if (treeFiles.SelectedNode == null)
            {
                return;
            }

            treeFiles_NodeMouseDoubleClick(this, new TreeNodeMouseClickEventArgs(treeFiles.SelectedNode,
                                                                           MouseButtons.Left,
                                                                           2,
                                                                           treeFiles.SelectedNode.Bounds.X,
                                                                           treeFiles.SelectedNode.Bounds.Y));
        }
        
        /// <summary>
		/// Handles the Click event of the itemExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemExit_Click(object sender, EventArgs e)
		{
			try
			{				
				Close();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the itemResetValue control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemResetValue_Click(object sender, EventArgs e)
		{
			try
			{
				if ((propertyItem.SelectedObject == null) || (propertyItem.SelectedGridItem == null)
					|| (propertyItem.SelectedGridItem.PropertyDescriptor == null))
				{
					return;
				}

				propertyItem.ResetSelectedProperty();
				propertyItem.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Opening event of the popupProperties control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void popupProperties_Opening(object sender, CancelEventArgs e)
		{
			if ((propertyItem.SelectedObject == null) 
				|| (propertyItem.SelectedGridItem == null) 
				|| (propertyItem.SelectedGridItem.PropertyDescriptor == null))
			{
				itemResetValue.Enabled = false;
				return;
			}

			itemResetValue.Enabled = (propertyItem.SelectedGridItem.PropertyDescriptor.CanResetValue(propertyItem.SelectedObject));
		}

        /// <summary>
        /// Handles the ContentPropertyChanged event of the CurrentContent control.
        /// </summary>
        /// <param name="changedArgs">Information about the property that changed.</param>
        private void OnContentPropertyChanged(ContentPropertyChangedEventArgs changedArgs)
        {
			try
			{
				// If we change the name of an item, we must rename it.
				if (changedArgs.PropertyName.Equals("name", StringComparison.OrdinalIgnoreCase))
				{
					// Find our node that corresponds to this content.
					var node = (from treeNode in treeFiles.AllNodes()
					            where string.Equals(treeNode.Name, CurrentOpenFile.FullPath, StringComparison.OrdinalIgnoreCase)
					            select treeNode).FirstOrDefault() as TreeNodeFile;

					// If the node isn't here, then it may not have been enumerated, so it's OK if we get out.
					if (node == null)
					{
						return;
					}

					var destNode = node.Parent as TreeNodeDirectory;

					if (destNode != null)
					{
						CopyFileNode(node, destNode, changedArgs.Value.ToString(), true);
					}
				}

				FileManagement.FileChanged = true;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
			}
        }

		/// <summary>
		/// Function called when the content window is closed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void OnContentClose(object sender, EventArgs e)
		{
			var panel = (ContentPanel)sender;

			// Turn off the event.
			panel.ContentClosed -= OnContentClose;

			if ((CurrentOpenFile == null)
				|| (!ContentManagement.Changed))
			{
				return;
			}

			ContentManagement.Save(CurrentOpenFile);

			CurrentOpenFile = null;
		}

		/// <summary>
		/// Handles the Click event of the itemPaste control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemPaste_Click(object sender, EventArgs e)
		{
			try
			{
				// Use a lockless pattern to keep this method from becoming reentrant.
			    if (Interlocked.Increment(ref _syncCounter) > 1)
			    {
			        return;
			    }

			    Cursor.Current = Cursors.WaitCursor;

				try
				{
					if ((_cutCopyObject == null) || (_cutCopyObject.Value.Object == null) || (_cutCopyObject.Value.Node == null))
					{
						return;
					}

					// If we're cutting/copying some tree node, then find the current node
					// and perform the operation.
					var node = _cutCopyObject.Value.Node;

					node.IsCut = false;
					node.Redraw();

					var dest = treeFiles.SelectedNode as TreeNodeDirectory;

					if (dest == null)
					{
						GorgonDialogs.ErrorBox(this, Resources.GOREDIT_PASTE_MUST_BE_DIRECTORY);
						return;
					}

					// If we're moving, there are some restrictions.
					if ((dest == node) && (_cutCopyObject.Value.IsCut))
					{
						GorgonDialogs.ErrorBox(this, Resources.GOREDIT_FILE_SOURCE_SAME_AS_DEST);
						return;
					}

					if ((_cutCopyObject.Value.IsCut) && (dest.IsAncestorOf(node)))
					{
						GorgonDialogs.ErrorBox(this, Resources.GOREDIT_FILE_CANNOT_MOVE_TO_CHILD);
						return;
					}

					var directory = node as TreeNodeDirectory;

					if (directory != null)
					{
						CopyDirectoryNode(directory, dest, directory.Text, _cutCopyObject.Value.IsCut);
						return;
					}

					var file = node as TreeNodeFile;

					if (file == null)
					{
						return;
					}

					// Don't copy over the same file.
					if ((dest == file.Parent) && (_cutCopyObject.Value.IsCut))
					{
						return;
					}

					CopyFileNode(file, dest, file.Text, _cutCopyObject.Value.IsCut);
				}
				catch (Exception ex)
				{
					GorgonDialogs.ErrorBox(this, ex);
				}
				finally
				{
					_cutCopyObject = null;
					ValidateControls();
					Cursor.Current = Cursors.Default;
				}
			}
			finally
			{
				Interlocked.Decrement(ref _syncCounter);
			}
		}

		/// <summary>
		/// Handles the Click event of the itemCut control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemCut_Click(object sender, EventArgs e)
		{
			try
			{
				if ((treeFiles.SelectedNode == null) || (treeFiles.SelectedNode == _rootNode))
				{
					return;
				}

				var node = (EditorTreeNode)treeFiles.SelectedNode;
				node.IsCut = ((sender == itemCut) || (sender == popupItemCut));
				node.Redraw();

				_cutCopyObject = new CutCopyObject(node, node.Name, node.IsCut);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemNew control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemNew_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// Save any changes we have.
				if (ConfirmSave())
				{
					return;
				}

				FileManagement.New();

				// Rebuild our tree.
				InitializeTree();
				
				// Load the default content panel.
				ContentManagement.LoadDefaultContentPane();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to pop up the save confirmation dialog.
		/// </summary>
		/// <returns>TRUE if canceled, FALSE if not.</returns>
		private bool ConfirmSave()
		{
			// Ensure both the content and the file change flags are checked.
			if (((!FileManagement.FileChanged) &&
			     (!ContentManagement.Changed)) 
				 || (PlugIns.WriterPlugIns.Count == 0))
			{
				return false;
			}

			var result = GorgonDialogs.ConfirmBox(this,
			                                      string.Format(Resources.GOREDIT_FILE_HAS_CHANGES_CONFIRM,
			                                                    FileManagement.Filename), true, false);

			if (result == ConfirmationResult.Cancel)
			{
				return true;
			}

			if (result != ConfirmationResult.Yes)
			{
				return false;
			}

			// If we have content open and it hasn't been persisted to the file system, 
			// then persist those changes.
			if ((CurrentOpenFile != null) && (ContentManagement.Changed))
			{
				ContentManagement.Save(CurrentOpenFile);
			}

			// If we haven't saved the file yet, then prompt us with a file name.
			if (string.IsNullOrWhiteSpace(FileManagement.FilePath))
			{
				itemSaveAs_Click(this, EventArgs.Empty);
			}
			else
			{
				itemSave_Click(this, EventArgs.Empty);
			}

			return false;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				if (ConfirmSave())
				{
					e.Cancel = true;
					return;
				}

				// Unhook from the content functionality.
				ContentManagement.ContentPropertyChanged = null;
				ContentManagement.ContentPaneUnloadAction = null;
				ContentManagement.ContentEnumerateProperties = null;
				ContentManagement.ContentInitializedAction = null;


                // Unhook from file management functionality.
			    ScratchArea.ImportExportFileConflictFunction = null;
                ScratchArea.ImportExportFileCompleteAction = null;
			    ScratchArea.ImportExportFileCopyExceptionAction = null;
			    ScratchArea.CanImportFunction = null;
			    ScratchArea.CreateFileConflictFunction = null;
			    ScratchArea.CopyFileConflictFunction = null;
			    ScratchArea.CopyDirectoryConflictFunction = null;

				// Unload any current content.
				ContentManagement.UnloadCurrentContent();
				
				if (WindowState != FormWindowState.Minimized)
				{
					Program.Settings.FormState = WindowState;
				}

				Program.Settings.WindowDimensions = WindowState != FormWindowState.Normal ? RestoreBounds : DesktopBounds;
                
                // Remember the last file we had open.
                if (!string.IsNullOrWhiteSpace(FileManagement.FilePath))
                {
                    Program.Settings.LastEditorFile = FileManagement.FilePath;
                }

				Program.Settings.Save();
			}
#if DEBUG
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
#else
			catch
			{
				// Eat this exception if in release.
#endif
			}
		}

		/// <summary>
		/// Handles the BeforeExpand event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				var directoryNode = e.Node as TreeNodeDirectory;

				// Expand sub folders.
				if (directoryNode != null)
				{
					GetFolders(directoryNode);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to retrieve the folder nodes.
		/// </summary>
		/// <param name="rootNode">Node to add folder information into.</param>
		private static void GetFolders(TreeNodeDirectory rootNode)
		{
			// Get the sub directories.
			rootNode.Nodes.Clear();

			foreach (var subDirectory in rootNode.Directory.Directories.OrderBy(item => item.Name))
			{
				var subNode = new TreeNodeDirectory(subDirectory);

				if ((subDirectory.Directories.Count > 0) 
                    || ((subDirectory.Files.Count > 0)
                    && (CanShowDirectoryFiles(subDirectory))))
				{
					subNode.Nodes.Add(new TreeNode("DummyNode"));
				}

				rootNode.Nodes.Add(subNode);
			}

			// Add file nodes.
			foreach (var fileNode in from file in rootNode.Directory.Files.OrderBy(item => item.Name) 
                                     where !FileManagement.IsBlocked(file) 
                                     select new TreeNodeFile(file))
			{
			    rootNode.Nodes.Add(fileNode);
			}
		}

        /// <summary>
        /// Function to determine if all the files in this directory are blocked or not.
        /// </summary>
        /// <param name="directory">Directory to evaluate.</param>
        /// <returns>TRUE if some files can be shown, FALSE if not.</returns>
        private static bool CanShowDirectoryFiles(GorgonFileSystemDirectory directory)
        {
			return !directory.Files.All(FileManagement.IsBlocked);
        }

		/// <summary>
		/// Function to initialize the files tree.
		/// </summary>
		private void InitializeTree()
		{            
			_rootNode = new RootNodeDirectory();

			// If we have files or sub directories, dump them in here.
			if ((ScratchArea.ScratchFiles.RootDirectory.Directories.Count > 0)
				|| ((ScratchArea.ScratchFiles.RootDirectory.Files.Count > 0)
				&& (CanShowDirectoryFiles(ScratchArea.ScratchFiles.RootDirectory))))
			{
                _rootNode.Nodes.Add(new TreeNode("DummyNode"));
			}

			treeFiles.BeginUpdate();
            treeFiles.Nodes.Clear();
			treeFiles.Nodes.Add(_rootNode);
            if (_rootNode.Nodes.Count > 0)
            {
                _rootNode.Expand();
            }
			treeFiles.EndUpdate();
		}

        /// <summary>
        /// Function to retrieve the directory from the selected node.
        /// </summary>
        /// <returns>The selected node.</returns>
        private TreeNodeDirectory GetSelectedDirectoryNode()
        {
			TreeNodeDirectory directory;

            if (treeFiles.SelectedNode != null)
            {
				directory = treeFiles.SelectedNode as TreeNodeDirectory;		

				if (directory == null)
                {					
                    // If we've got a file hilighted, then add to the same directory that we're in.
					TreeNode parentNode = treeFiles.SelectedNode.Parent;

					while (!(parentNode is TreeNodeDirectory))
					{
						parentNode = parentNode.Parent;

						if (parentNode == null)
						{
							break;
						}
					}

					directory = (TreeNodeDirectory)parentNode;
                }

                Debug.Assert(directory != null, "Directory should not be NULL.");

				directory.Expand();
            }
            else
            {
				treeFiles.Nodes[0].Expand();
				directory = ((TreeNodeDirectory)treeFiles.Nodes[0]);
            }

            return directory;
        }

		/// <summary>
		/// Function to add content to the interface.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddContent(object sender, EventArgs e)
		{
		    TreeNodeDirectory directoryNode = GetSelectedDirectoryNode();
			TreeNodeFile newNode = null;
		    ContentObject content = null;
			var item = sender as ToolStripMenuItem;

			if (item == null)
			{
				return;
			}

            var plugIn = item.Tag as ContentPlugIn;

			if (plugIn == null)
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			try
			{
                // Create the content object.
                content = ContentManagement.Create(plugIn);

			    if (content == null)
			    {
			        return;
			    }

				// If the file already exists, then ask the user if they want to overwrite it.
				if ((directoryNode.Directory.Files.Contains(content.Name))
					&& (ImportConfirmFileOverwrite(directoryNode.Directory.FullPath + content.Name, 1) == ConfirmationResult.No))
				{
					return;
				}

                // Reset to a wait cursor.
                Cursor.Current = Cursors.WaitCursor;

                // Create the file in the scratch area.
                CurrentOpenFile = ScratchArea.CreateFile(content.Name, content.ContentType, directoryNode.Directory);

                // The file did not get created, leave.
                if (CurrentOpenFile == null)
			    {
			        content.Dispose();
			        return;
			    }

                // Create the file node in our tree.
                newNode = new TreeNodeFile(CurrentOpenFile);

                // Add to tree and select.
                directoryNode.Nodes.Add(newNode);
                treeFiles.SelectedNode = newNode;

			    ContentManagement.LoadContentPane(content);

                // We set this to true to indicate that this is a new file.
                FileManagement.FileChanged = true;
			}
			catch (Exception ex)
			{
                // Load the default pane.
                ContentManagement.LoadDefaultContentPane();
				
				// Roll back the changes to the file system.
				if (CurrentOpenFile != null)
				{
					try
					{
						// Destroy this file.
						ScratchArea.ScratchFiles.DeleteFile(CurrentOpenFile);
					}
#if DEBUG
					catch (Exception innerEx)
					{
						GorgonException.Log = Program.LogFile;
						GorgonException.Catch(innerEx);
					}
#else
					catch
					{
						// Intentionally left blank for release mode.
					}
#endif
					finally
					{
						GorgonException.Log = Gorgon.Log;
					}
				}

				if (newNode != null)
				{
					newNode.Remove();
				}

			    CurrentOpenFile = null;

                GorgonDialogs.ErrorBox(this, ex);

				if (content != null)
				{
					content.Dispose();
				}
			}
			finally
			{
				treeFiles.Refresh();
				Cursor.Current = Cursors.Default;
                ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSave_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
                // Save outstanding edits on the content.
				if ((CurrentOpenFile != null) && (ContentManagement.Changed))
				{
					ContentManagement.Save(CurrentOpenFile);
				}

			    FileWriterPlugIn plugIn = FileManagement.GetWriterPlugIn(FileManagement.FilePath);

			    FileManagement.Save(FileManagement.FilePath, plugIn);

                AddToRecent(FileManagement.FilePath);

				treeFiles.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to delete a directory and all the files and subdirectories underneath it or a single file.
		/// </summary>
		/// <param name="node">Node that contained the deleted file/driectory</param>
		private void PruneTree(TreeNode node)
		{
			Cursor.Current = Cursors.WaitCursor;

            // Turn off pasting ability if we delete the object being held on the clip board.
            if ((_cutCopyObject != null)
                && (_cutCopyObject.Value.Node == node))
            {
                _cutCopyObject = null;
            }

			// If we've selected the root node, then we need to destroy everything.
			if (node == _rootNode)
			{
				_rootNode.Nodes.Clear();
				return;
			}

            // If there's a single node contained within the selected node, then collapse the parent and
            // remove the node so that the child node indicator goes away.
			if (node.Parent.Nodes.Count == 1)
			{
				node.Parent.Collapse();
			}
			node.Remove();
			treeFiles.Refresh();
		}

		/// <summary>
		/// Handles the Click event of the itemDelete control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemDelete_Click(object sender, EventArgs e)
		{
			try
			{
				var directoryNode = treeFiles.SelectedNode as TreeNodeDirectory;

                if (treeFiles.SelectedNode == null)
                {
                    directoryNode = _rootNode;
                }

				if (directoryNode != null)
				{
                    if (GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_FILE_DELETE_DIRECTORY_CONFIRM, directoryNode.Directory.FullPath)) == ConfirmationResult.No)
                    {
                        return;
                    }

				    GorgonFileSystemDirectory directory = directoryNode.Directory;
			
                    // If the currently open file is within the directory being deleted, then close it.
				    if ((CurrentOpenFile != null)
				        && ((directory == ScratchArea.ScratchFiles.RootDirectory) || (directory.Contains(CurrentOpenFile))))
				    {
				        ContentManagement.LoadDefaultContentPane();
				    }

                    ScratchArea.ScratchFiles.DeleteDirectory(directory);

                    PruneTree(directoryNode);

				    FileManagement.FileChanged = true;
				    return;
				}
				
				var fileNode = treeFiles.SelectedNode as TreeNodeFile;

				if (fileNode == null)
				{
					return;
				}

				if (GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_FILE_DELETE_FILE_CONFIRM, fileNode.File.FullPath)) == ConfirmationResult.No)
				{
					return;
				}

			    GorgonFileSystemFileEntry file = fileNode.File;

                // Close the currently open file.
			    if (CurrentOpenFile == file)
			    {
			        ContentManagement.LoadDefaultContentPane();
			    }

                ScratchArea.ScratchFiles.DeleteFile(file);

				PruneTree(fileNode);
			    FileManagement.FileChanged = true;
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
		/// Handles the Click event of the itemSaveAs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSaveAs_Click(object sender, EventArgs e)
		{
			var plugIns = new List<FileWriterPlugIn>();

			try
			{
				AssignFilter(dialogSaveFile, FileManagement.GetWriterExtensions(), false, null, null, extension =>
				{
					FileWriterPlugIn extPlugIn = FileManagement.GetWriterPlugIn(extension.Extension, true);

					plugIns.Add(extPlugIn);

					return (FileManagement.GetWriterPlugIn() == extPlugIn);
				});

				if (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile))
				{
					dialogSaveFile.FileName = Path.GetFileName(Program.Settings.LastEditorFile);
				}

				// Open dialog.
				if (dialogSaveFile.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				// Save outstanding edits on the content.
				if ((CurrentOpenFile != null) && (ContentManagement.Changed))
				{
					ContentManagement.Save(CurrentOpenFile);
				}

				// Get the plug-in and write out the file.
				FileWriterPlugIn plugIn = plugIns[dialogSaveFile.FilterIndex - 1];
				FileManagement.SetWriterPlugIn(plugIn);
				FileManagement.Save(dialogSaveFile.FileName, plugIn);

				treeFiles.Refresh();

				AddToRecent(dialogSaveFile.FileName);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the ItemDrag event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ItemDragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				var node = e.Item as EditorTreeNode;
				
				if ((node != null) && (node != _rootNode))
				{
					treeFiles.DoDragDrop(new Tuple<EditorTreeNode, MouseButtons>(node, e.Button), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the AfterLabelEdit event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
		private void treeFiles_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			var node = e.Node as EditorTreeNode;
			var currentState = NodeEditState.None;
			string label = e.Label;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				e.CancelEdit = true;

				// This node isn't an editor node, we should get rid of it.
				if (node == null)
				{
					if (e.Node != null)
					{
						e.Node.Remove();
					}
					return;
				}

				// Ensure that whatever we type will work in the file system.
				if ((label.IndexOfAny(_fileChars) > -1) || (label.IndexOf('/') > -1) || (label.IndexOf('\\') > -1))
				{
					GorgonDialogs.ErrorBox(this,string.Format(Resources.GOREDIT_FILE_PATH_HAS_INVALID_CHARS,
					                                     string.Join(" ", _fileChars.Where(item => item > 32))));
					node.BeginEdit();
					return;
				}

				if ((node.EditState == NodeEditState.CreateDirectory) || (node.EditState == NodeEditState.RenameDirectory))
				{
					currentState = node.EditState;

					// Create the directory in the scratch area.
					if (currentState == NodeEditState.CreateDirectory)
					{
						// Empty strings can't be used.
						if (string.IsNullOrWhiteSpace(label))
						{
							e.Node.Remove();
							return;
						}

						label = label.FormatDirectory('/');

						var parentNode = node.Parent as TreeNodeDirectory;

						Debug.Assert(parentNode != null, "Parent node is NULL!");

						var newDirectory = ScratchArea.ScratchFiles.CreateDirectory(parentNode.Directory.FullPath + label);

						// Set up a new node for the directory since our current node is here as a proxy.
						var treeNode = new TreeNodeDirectory(newDirectory);

						int nodeIndex = parentNode.Nodes.IndexOf(e.Node);
						parentNode.Nodes.Insert(nodeIndex, treeNode);
						// Remove proxy node.						
						e.Node.Remove();

						FileManagement.FileChanged = true;
					}
					else
					{
						if (string.IsNullOrWhiteSpace(label))
						{
							return;
						}

						// Rename the directory by moving it.
						var selectedNode = node as TreeNodeDirectory;

						Debug.Assert(selectedNode != null, "Directory node is not a directory!");

						var parentNode = selectedNode.Parent as TreeNodeDirectory;

                        CopyDirectoryNode(selectedNode, parentNode, label, true);
					}
				}
				else
				{
					if (string.IsNullOrWhiteSpace(label))
					{
						return;
					}
					
					// Rename the file by moving it.
					var selectedNode = node as TreeNodeFile;
					Debug.Assert(selectedNode != null, "File node is not a file!");
					var parentNode = selectedNode.Parent as TreeNodeDirectory;

                    CopyFileNode(selectedNode, parentNode, label, true);
				}
			}
			catch (Exception ex)
			{
				// If we get an error, just remove any new nodes.
				if (((currentState == NodeEditState.CreateDirectory) || (currentState == NodeEditState.CreateFile))
					&& (node != null))
				{
					node.Remove();
				}

				e.CancelEdit = true;
				GorgonDialogs.ErrorBox(this, ex);

				if (ContentManagement.Current == null)
				{
					ContentManagement.LoadDefaultContentPane();
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the BeforeLabelEdit event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
		private void treeFiles_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			try
			{
				if (e.Node == null)
				{
					return;
				}

				// We're being clever, don't allow that.
				if (string.IsNullOrWhiteSpace(e.Label))
				{					
					e.CancelEdit = true;
					e.Node.Remove();
					return;
				}

				// Do not attempt to rename the top level.
				if ((e.Node == _rootNode) || (e.Node.Parent == null))
				{
					e.CancelEdit = true;
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
		/// Handles the Click event of the itemRenameFolder control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemRenameFolder_Click(object sender, EventArgs e)
		{
			var selectedNode = treeFiles.SelectedNode as EditorTreeNode;

			try
			{
				if (selectedNode == null)
				{
					return;
				}

				if (selectedNode is TreeNodeDirectory)
				{
					selectedNode.EditState = NodeEditState.RenameDirectory;
				}
				else
				{
					selectedNode.EditState = NodeEditState.RenameFile;
				}

				selectedNode.BeginEdit();
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
		/// Function to add a directory to the after the last directory in the parent, but before any files.
		/// </summary>
		/// <param name="parent">Parent node.</param>		
		/// <param name="newNode">New Node to add.</param>
		private static void AddAfterLastFolder(TreeNode parent, TreeNode newNode)
		{
			// Add after other folders.
			var lastFolder = parent.Nodes.Cast<TreeNode>().LastOrDefault(item => item is TreeNodeDirectory);

			if (lastFolder != null)
			{
				int index = parent.Nodes.IndexOf(lastFolder);
				if (index + 1 < parent.Nodes.Count)
				{
					parent.Nodes.Insert(index + 1, newNode);
				}
				else
				{
					parent.Nodes.Add(newNode);
				}
			}
			else
			{
				parent.Nodes.Insert(0, newNode);
			}			
		}

		/// <summary>
		/// Handles the Click event of the itemCreateFolder control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemCreateFolder_Click(object sender, EventArgs e)
		{
			bool expandDisabled = false;
			var tempNode = new EditorTreeNode();

			try
			{
				int nameIndex = -1;
				string defaultName = Resources.GOREDIT_FILE_DEFAULT_DIRECTORY_NAME;
				var selectedNode = treeFiles.SelectedNode as TreeNodeDirectory;

				if (selectedNode == null)
				{
					return;
				}

				// Expand the node if it's not already done.
				if (!selectedNode.IsExpanded)
				{
					selectedNode.Expand();
				}

				tempNode.CollapsedImage = Resources.folder_16x16;

				// Update the name.
				while ((selectedNode.Directory.Directories.Contains(defaultName))
						|| (selectedNode.Directory.Files.Contains(defaultName)))
				{
					nameIndex++;
					defaultName = string.Format("{0}_{1}", Resources.GOREDIT_FILE_DEFAULT_DIRECTORY_NAME, nameIndex);
				}

				tempNode.Text = defaultName;

				AddAfterLastFolder(selectedNode, tempNode);

				if ((!selectedNode.IsExpanded) && (selectedNode.Nodes.Count == 1))
				{
					treeFiles.BeforeExpand -= treeFiles_BeforeExpand;
					expandDisabled = true;
				}

				selectedNode.Expand();

				tempNode.BeginEdit();
				tempNode.EditState = NodeEditState.CreateDirectory;
			}
			catch (Exception ex)
			{
				tempNode.EndEdit(true);

				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				if (expandDisabled)
				{
					treeFiles.BeforeExpand += treeFiles_BeforeExpand;
				}
				Cursor.Current = Cursors.Default;
			}
		}

        /// <summary>
        /// Function to copy a file node to another location.
        /// </summary>
        /// <param name="sourceFile">The file to copy.</param>
        /// <param name="destDirectory">The destination directory.</param>
        /// <param name="name">The new name for the file.</param>
        /// <param name="deleteSource">TRUE to delete the source file, FALSE to leave alone.</param>
        private void CopyFileNode(TreeNodeFile sourceFile, TreeNodeDirectory destDirectory, string name, bool deleteSource)
        {
	        if (string.IsNullOrWhiteSpace(name))
            {
                name = sourceFile.File.Name;
            }

            // If we dropped the extension, then replace it.
            if (!name.EndsWith(sourceFile.File.Extension, StringComparison.OrdinalIgnoreCase))
            {
                name += sourceFile.File.Extension;
            }

            var newFilePath = destDirectory.Directory.FullPath + name.FormatFileName();

            if (ScratchArea.ScratchFiles.GetFile(newFilePath) != null)
            {
                // If the file exists and we're renaming (not moving), then throw up an error and leave.
                if (deleteSource)
                {
                    if (!string.Equals(name, sourceFile.File.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        GorgonDialogs.ErrorBox(this, string.Format(Resources.GOREDIT_FILE_ALREADY_EXISTS, Resources.GOREDIT_FILE_DEFAULT_TYPE, newFilePath));
                    }

	                // We're trying to copy over ourselves, do nothing.
	                return;
                }

	            var result = GorgonDialogs.ConfirmBox(this,
	                                                  string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
	                                                                Resources.GOREDIT_FILE_DEFAULT_TYPE, newFilePath), true,
	                                                  false);

                if (result == ConfirmationResult.Cancel)
                {
                    return;
                }

                // If we specified no, then we have to create a new name.
				if (result == ConfirmationResult.No)
				{
					int counter = 1;
					string newName = sourceFile.File.BaseFileName + " (" + counter + ")" + sourceFile.File.Extension;

					while (ScratchArea.ScratchFiles.GetFile(newName) != null)
					{
						newName = sourceFile.File.BaseFileName + " (" + (++counter) + ")" + sourceFile.File.Extension;
					}

					newFilePath = destDirectory.Directory.FullPath + newName;
				}
				else
				{
					// Do not copy over ourselves.
					if (string.Equals(newFilePath, sourceFile.File.FullPath, StringComparison.InvariantCultureIgnoreCase))
					{
						return;
					}
				}
            }

			Cursor.Current = Cursors.WaitCursor;

            using (var inputStream = ScratchArea.ScratchFiles.OpenStream(sourceFile.File, false))
            {
                using (var outputStream = ScratchArea.ScratchFiles.OpenStream(newFilePath, true))
                {
                    inputStream.CopyTo(outputStream);
                }
            }

			// Reroute any cut/copy operation.
			if (deleteSource)
			{
				// If this file is open, then update its handle.
				if ((ContentManagement.Current != null)
					&& (CurrentOpenFile == sourceFile.File))
				{
					var newFile = ScratchArea.ScratchFiles.GetFile(newFilePath);
					CurrentOpenFile = newFile;
					ContentManagement.Current.Name = newFile.Name;
				}
			}

            destDirectory.Collapse();

            if (destDirectory.Nodes.Count == 0)
            {
                destDirectory.Nodes.Add(new TreeNode("DUMMYNODE"));
            }

            if (deleteSource)
            {
                DeleteFile(sourceFile);
            }

            destDirectory.Expand();

            treeFiles.SelectedNode = destDirectory.Nodes[newFilePath];

            FileManagement.FileChanged = true;
        }

        /// <summary>
        /// Function to copy a directory node to another location.
        /// </summary>
        /// <param name="sourceDirectory">Directory to copy.</param>
        /// <param name="destDirectory">Directory to copy into.</param>
        /// <param name="name">The new name for the directory.</param>
        /// <param name="deleteSource">TRUE to delete the source directory, FALSE to leave alone.</param>
        private void CopyDirectoryNode(TreeNodeDirectory sourceDirectory, TreeNodeDirectory destDirectory, string name, bool deleteSource)
        {
            var result = ConfirmationResult.None;
            bool wasExpanded = sourceDirectory.IsExpanded;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = sourceDirectory.Directory.Name;
            }

            // We're moving to the same place... leave.
            if ((deleteSource) && (sourceDirectory.Parent == destDirectory) && (string.Equals(name, sourceDirectory.Directory.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            name = (destDirectory.Directory.FullPath + name).FormatDirectory('/');

            // We have a directory with this new name already, throw an error.
            if ((deleteSource) 
				&& (!string.Equals(name, sourceDirectory.Directory.Name, StringComparison.OrdinalIgnoreCase)) 
				&& (ScratchArea.ScratchFiles.GetDirectory(name) != null))
            {
                GorgonDialogs.ErrorBox(this, string.Format(Resources.GOREDIT_FILE_ALREADY_EXISTS, Resources.GOREDIT_NODE_DIRECTORY, name));
                return;
            }

            var directories = new List<GorgonFileSystemDirectory>(ScratchArea.ScratchFiles.FindDirectories(sourceDirectory.Directory.FullPath, "*", true))
	            {
		            sourceDirectory.Directory
	            };

	        foreach (var directory in directories)
            {
                // Update the path to point at the new parent.
                var newDirPath = name + directory.FullPath.Substring(directory.FullPath.Length);
                
				ScratchArea.ScratchFiles.CreateDirectory(newDirPath);

                // Copy each file.
                foreach (var file in directory.Files)
                {
                    var newFilePath = newDirPath + file.Name;

                    // The file exists in the destination.  Ask the user what to do.
                    if (ScratchArea.ScratchFiles.GetFile(newFilePath) != null)
                    {
                        if ((result & ConfirmationResult.ToAll) == 0)
                        {
	                        result = GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
	                                                                              Resources.GOREDIT_FILE_DEFAULT_TYPE,
	                                                                              newFilePath), true, true);
							Cursor.Current = Cursors.WaitCursor;
                        }

						if (result == ConfirmationResult.Cancel)
						{
							break;
						}

                        // If we specified no, then we have to create a new name.
						if ((result & ConfirmationResult.No) == ConfirmationResult.No)
						{
							int counter = 1;
							string newName = file.BaseFileName + " (" + counter + ")" + file.Extension;

							while (ScratchArea.ScratchFiles.GetFile(newName) != null)
							{
								newName = file.BaseFileName + " (" + (++counter) + ")" + file.Extension;
							}

							newFilePath = newDirPath + newName;
						}
						else
						{
							// Don't overwrite ourselves.
							if (string.Equals(newFilePath, file.FullPath, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
						}                            
                    }

                    using (var inputStream = ScratchArea.ScratchFiles.OpenStream(file, false))
                    {
                        using (var outputStream = ScratchArea.ScratchFiles.OpenStream(newFilePath, true))
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }

	                if (!deleteSource)
	                {
		                continue;
	                }

	                // If we have this file open, and we're moving the file, then relink it.
	                if (file != CurrentOpenFile)
	                {
		                continue;
	                }

	                var newFile = ScratchArea.ScratchFiles.GetFile(newFilePath);
	                CurrentOpenFile = newFile;
                }

                // We cancelled our copy, so leave.
                if (result == ConfirmationResult.Cancel)
                {
                    break;
                }
            }
			
            destDirectory.Collapse();
            if (destDirectory.Nodes.Count == 0)
            {
                destDirectory.Nodes.Add(new TreeNode("DUMMYNODE"));
            }

            // Wipe out the source.
            if (deleteSource)
            {
                PruneTree(sourceDirectory);
            }

            // Copy is done, update our tree nodes.            
            destDirectory.Expand();

            sourceDirectory = (TreeNodeDirectory)destDirectory.Nodes[name];
            treeFiles.SelectedNode = sourceDirectory;

            if (wasExpanded)
            {
                // Get the node and expand it if it was previously open.
                destDirectory.Nodes[name].Expand();
            }

            FileManagement.FileChanged = true;
        }

		/// <summary>
		/// Function to copy files from windows explorer into our file system.
		/// </summary>
		/// <param name="destDir">Destination directory node.</param>
		/// <param name="files">Paths to the files/directories to copy.</param>
		private static void AddFilesFromExplorer(TreeNodeDirectory destDir, IEnumerable<string> files)
		{
			if (ScratchArea.Import(files, destDir.Directory) == 0)
			{
				return;
			}

			FileManagement.FileChanged = true;

			if (destDir.IsExpanded)
			{
				destDir.Collapse();
			}

			if (destDir.Nodes.Count == 0)
			{
				destDir.Nodes.Add(new TreeNode("DUMMYNODE"));
			}

			destDir.Expand();
		}

		/// <summary>
		/// Handles the DragDrop event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				if (e.Effect == DragDropEffects.None)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				var overNode = (EditorTreeNode)treeFiles.GetNodeAt(treeFiles.PointToClient(new Point(e.X, e.Y)));
				var destDir = overNode as TreeNodeDirectory;

				// Handle explorer files.
				if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				{
					var files = new List<string>((IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop));
					var excludedFiles = files.Where(item => item.StartsWith(ScratchArea.ScratchFiles.WriteLocation, StringComparison.OrdinalIgnoreCase));

					// Don't allow files in our write path to be imported.
					foreach(var filePath in excludedFiles)
					{
						files.Remove(filePath);
					}

					if (files.Count > 0)
					{
						AddFilesFromExplorer(destDir, files);						
					}					
					return;
				}

				// If we're moving one of our directories or files, then process those items.
			    if (!e.Data.GetDataPresent(typeof(Tuple<EditorTreeNode, MouseButtons>)))
			    {
			        return;
			    }

			    var data = (Tuple<EditorTreeNode, MouseButtons>)e.Data.GetData(typeof(Tuple<EditorTreeNode, MouseButtons>));

			    // Perform a move.
			    var directory = data.Item1 as TreeNodeDirectory;

			    // Our source data is a directory, so move it.
			    if ((directory != null) && (destDir != null))
			    {
			        CopyDirectoryNode(directory, destDir, directory.Directory.Name, (data.Item2 == MouseButtons.Left));
			        return;
			    }

			    // We didn't have a directory, so move the file.
			    var file = data.Item1 as TreeNodeFile;

			    if ((destDir == null)
			        || (file == null))
			    {
			        return;
			    }

				CopyFileNode(file, destDir, file.Name, data.Item2 == MouseButtons.Left);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);

				if (ContentManagement.Current == null)
				{
					ContentManagement.LoadDefaultContentPane();
				}
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the DragEnter event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				e.Effect = DragDropEffects.None;

				if (e.Data.GetDataPresent(DataFormats.FileDrop, false)) 
				{
					e.Effect = DragDropEffects.Copy;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the DragOver event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				var overNode = treeFiles.GetNodeAt(treeFiles.PointToClient(new Point(e.X, e.Y))) as EditorTreeNode;
				var destDirectory = overNode as TreeNodeDirectory;
				var destFile = overNode as TreeNodeFile;

				e.Effect = DragDropEffects.None;

				// If we're over nothing, then assume we're over the root node.
				if (overNode == null)
				{
					overNode = _rootNode;
				}

				// Handle files from explorer.
				if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				{
					if (destDirectory == null)
					{
						return;
					}

					e.Effect = DragDropEffects.Copy;

					return;
				}

			    if (!e.Data.GetDataPresent(typeof(Tuple<EditorTreeNode, MouseButtons>)))
			    {
			        return;
			    }

			    // Get our source data.
			    var dragData = (Tuple<EditorTreeNode, MouseButtons>)e.Data.GetData(typeof(Tuple<EditorTreeNode, MouseButtons>));
			    var sourceDirectory = dragData.Item1 as TreeNodeDirectory;
			    var sourceFile = dragData.Item1 as TreeNodeFile;

			    // Don't drag into ourselves, that's just dumb.
			    // Likewise, if we're over our current parent, do nothing.
			    if ((sourceDirectory == overNode) || (overNode == dragData.Item1.Parent))
			    {
			        return;
			    }

			    // If we drag a directory over a file, then we can't do use that.
			    if (destFile != null)
			    {
			        if (sourceDirectory != null)
			        {
			            return;
			        }

			        // In the future, we may allow file linking, but we'll need to test for it.
			        // Until that time, just disable the drag/drop.
			        if (sourceFile != null)
			        {
			            return;
			        }
			    }

			    treeFiles.SelectedNode = overNode;
			    e.Effect = dragData.Item2 == MouseButtons.Left ? DragDropEffects.Move : DragDropEffects.Copy;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

        /// <summary>
        /// Handles the Click event of the itemOpen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemOpen_Click(object sender, EventArgs e)
        {
            try
            {
				// Save the file if it's changed.
				if (ConfirmSave())
				{
					return;
				}

                if (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile))
                {
                    dialogOpenFile.InitialDirectory = Path.GetDirectoryName(Program.Settings.LastEditorFile);
                }

                AssignFilter(dialogOpenFile, FileManagement.GetReaderExtensions(), true, Resources.GOREDIT_ALL_FILE_TYPES);

                if (dialogOpenFile.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                OpenFile(dialogOpenFile.FileName);
            }
            catch (Exception ex)
            {
				ContentManagement.LoadDefaultContentPane();
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                ValidateControls();
            }
        }

        /// <summary>
        /// Function to add a file to the recent files list.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        private void AddToRecent(string filePath)
        {
			if (string.IsNullOrWhiteSpace(filePath))
			{
				return;
			}

	        var existing =
		        Program.Settings.RecentFiles.FirstOrDefault(
			        item =>
			        !string.IsNullOrWhiteSpace(item) &&
			        string.Equals(item, filePath, StringComparison.OrdinalIgnoreCase));
	        

			// It already exists, move it to the top of the list.
			if (!string.IsNullOrWhiteSpace(existing))
			{
				Program.Settings.RecentFiles.Remove(existing);
				Program.Settings.RecentFiles.Insert(0, filePath);
				return;
			}

			Program.Settings.RecentFiles.Insert(0, filePath);

			FillRecent();
			ValidateControls();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the tabDocumentManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tabDocumentManager_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateControls();
        }
        
        /// <summary>
		/// Function to initialize the global interface commands for each content plug-in.
		/// </summary>
		private void InitializeInterface()
		{
			foreach (var plugIn in PlugIns.ContentPlugIns)
			{
				// Get the menu item.
				var createItem = plugIn.Value.GetCreateMenuItem();

			    if (createItem == null)
			    {
			        continue;
			    }

			    // Add to the 3 "Add" loctaions.
			    popupAddContentMenu.Items.Add(createItem);

			    // Click event.
			    createItem.Click += AddContent;
			}

			// Enable the add items if we have anything new.
			popupItemAddContent.Enabled = dropNewContent.Enabled = itemAddContent.Enabled = itemAddContent.DropDownItems.Count > 0;
		}

        /// <summary>
        /// Function to fill the recent files list.
        /// </summary>
        private void FillRecent()
        {
            foreach (ToolStripMenuItem item in menuRecentFiles.DropDownItems)
            {
                item.Click -= recentItem_Click;
            }

            menuRecentFiles.DropDownItems.Clear();

            for (int i = 0; i < Program.Settings.RecentFiles.Count.Min(10); i++)
            {
                try
                {
					if (string.IsNullOrWhiteSpace(Program.Settings.RecentFiles[i]))
					{
						continue;
					}

                    var file = Path.GetFullPath(Program.Settings.RecentFiles[i]);
                    var directory = Path.GetDirectoryName(file).FormatDirectory(Path.DirectorySeparatorChar);
                    var fileName = Path.GetFileName(file);
                    var root = Path.GetPathRoot(directory);
                    var item = new ToolStripMenuItem();

                    // This shouldn't be.
                    Debug.Assert(root != null, "Directory is relative or NULL!!");

                    directory = directory.Substring(root.Length);
                    if (directory.Length > 0)
                    {
                        directory = directory.Ellipses(35, true);
                    }

                    item.Text = string.Format("{0} {1}{2}{3}", i + 1,  root, directory, fileName);

                    item.Tag = file;
                    item.AutoToolTip = true;
                    item.ToolTipText = file;

                    menuRecentFiles.DropDownItems.Add(item);

                    item.Click += recentItem_Click;
                }
                catch(Exception ex)
                {
                    GorgonException.Log = Program.LogFile;
                    GorgonException.Catch(ex);
                    GorgonException.Log = Gorgon.Log;
                }
            }
        }

        /// <summary>
        /// Function to open a file in the editor.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        private void OpenFile(string filePath)
        {
            // If this file is already opened, then do nothing.
            if (string.Equals(FileManagement.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Close the current content.
            ContentManagement.LoadDefaultContentPane();

            // Open the file.
            FileManagement.Open(filePath);

            // Update the tree.
            InitializeTree();

            AddToRecent(filePath);
            FillRecent();
        }

        /// <summary>
        /// Handles the Click event of the recentItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void recentItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;

            if (item == null)
            {
                return;
            }
            
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // Save the file if it's changed.
                if (ConfirmSave())
                {
                    return;
                }

                if ((item.Tag == null) || (string.IsNullOrWhiteSpace(item.Tag.ToString())))
                {
                    return;
                }

                OpenFile((string)item.Tag);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                ValidateControls();
                Cursor.Current = Cursors.Default;
            }
        }

		/// <summary>
		/// Function to localize the controls.
		/// </summary>
		private void LocalizeControls()
		{
			Text = Resources.GOREDIT_DEFAULT_TITLE;

			pageItems.Text = Resources.GOREDIT_TAB_CONTENTFILES;
			pageProperties.Text = Resources.GOREDIT_TAB_PROPERTIES;

			menuFile.Text = Resources.GOREDIT_MENU_FILE;
			popupItemEdit.Text = menuEdit.Text = Resources.GOREDIT_MENU_EDIT;
			menuRecentFiles.Text = Resources.GOREDIT_MENU_RECENTFILES;
			
			itemNew.Text = string.Format("{0}...", Resources.GOREDIT_MENU_NEW);
			itemOpen.Text = string.Format("{0}...", Resources.GOREDIT_MENU_OPEN);
			popupItemAddContent.Text = itemAddContent.Text = Resources.GOREDIT_MENU_ADDCONTENT;
			popupItemCreateFolder.Text = itemCreateFolder.Text = string.Format("{0}...", Resources.GOREDIT_MENU_CREATEFOLDER);
			itemSave.Text = string.Format("{0}...", Resources.GOREDIT_MENU_SAVE);
			itemSaveAs.Text = string.Format("{0}...", Resources.GOREDIT_MENU_SAVEAS);
			itemImport.Text = string.Format("{0}...", Resources.GOREDIT_MENU_IMPORT);
			itemExport.Text = string.Format("{0}...", Resources.GOREDIT_MENU_EXPORT);
			itemExit.Text = Resources.GOREDIT_MENU_EXIT;

			popupItemCut.Text = itemCut.Text = Resources.GOREDIT_MENU_CUT;
			popupItemCopy.Text = itemCopy.Text = Resources.GOREDIT_MENU_COPY;
			popupItemPaste.Text = itemPaste.Text = Resources.GOREDIT_MENU_PASTE;
			popupItemDelete.Text = itemDelete.Text = string.Format("{0}...", Resources.GOREDIT_MENU_DELETE_DEFAULT);
			itemPreferences.Text = Resources.GOREDIT_MENU_PREFERENCES;
			
			popupItemRename.Text = string.Format("{0}...", Resources.GOREDIT_MENU_RENAME_DEFAULT);

			dropNewContent.Text = dropNewContent.ToolTipText = Resources.GOREDIT_BUTTON_NEWCONTENT;
			buttonEditContent.Text = buttonEditContent.ToolTipText = Resources.GOREDIT_BUTTON_EDIT;
			buttonDeleteContent.Text = buttonDeleteContent.ToolTipText = Resources.GOREDIT_BUTTON_DELETE;

			itemResetValue.Text = Resources.GOREDIT_MENU_RESETVALUE;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				ToolStripManager.Renderer = new DarkFormsRenderer();

				Location = Program.Settings.WindowDimensions.Location;
				Size = Program.Settings.WindowDimensions.Size;

				// If this window can't be placed on a monitor, then shift it to the primary.
				if (!Screen.AllScreens.Any(item => item.Bounds.Contains(Location)))
				{
					Location = Screen.PrimaryScreen.Bounds.Location;
				}

				WindowState = Program.Settings.FormState;

				InitializeInterface();

				InitializeTree();

                // Check recent files.
                for (int i = 0; i < Program.Settings.RecentFiles.Count; i++)
                {
                    try
                    {
	                    if (string.IsNullOrWhiteSpace(Program.Settings.RecentFiles[i]))
	                    {
		                    continue;
	                    }

	                    var file = Path.GetFullPath(Program.Settings.RecentFiles[i]);
                        var directory = Path.GetDirectoryName(file).FormatDirectory(Path.DirectorySeparatorChar);

                        if ((!Directory.Exists(directory)) || (!File.Exists(file)))
                        {
                            Program.Settings.RecentFiles[i] = string.Empty;
                        }
                    }
                    catch
                    {
                        // We don't want files that cause exceptions to show up in our recent list.                    
                        Program.Settings.RecentFiles[i] = string.Empty;
                    }
                }

                // Remove all empty entries.
                Program.Settings.RecentFiles.Remove(string.Empty);

                FillRecent();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain"/> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();

			LocalizeControls();

			// Set up linkage to the content management interface.
			ContentManagement.ContentPaneUnloadAction = () => treeFiles.Refresh();
			ContentManagement.ContentEnumerateProperties = hasProperties =>
			{
				if (hasProperties)
				{
					pageProperties.Enabled = true;
					propertyItem.SelectedObject = ContentManagement.Current.TypeDescriptor;
					propertyItem.Refresh();
					tabDocumentManager.SelectedTab = pageProperties;
    
					// Set up event.
					ContentManagement.ContentPropertyChanged = OnContentPropertyChanged;

					propertyItem.PropertyValueChanged += OnPropertyValueChanged;
				}
				else
				{                
					tabDocumentManager.SelectedTab = pageItems;
					pageProperties.Enabled = false;
					ContentManagement.ContentPropertyChanged = null;
					propertyItem.PropertyValueChanged -= OnPropertyValueChanged;
				}
			};
			ContentManagement.ContentInitializedAction = control =>
			                                             {
															control.Dock = DockStyle.Fill;
															control.Parent = splitPanel1;
				                                            control.ContentClosed += OnContentClose;
			                                             };

            // Assign file management linkage.
			ScratchArea.CanImportFunction = EvaluateFileImport;
			ScratchArea.ImportExportFileCopyExceptionAction = FileCopyException;
            ScratchArea.ImportExportFileConflictFunction = ImportConfirmFileOverwrite;
            ScratchArea.ImportExportFileCompleteAction = FileImportExportCompleted;
		    ScratchArea.CreateFileConflictFunction = (fileName, fileType) => GorgonDialogs.ConfirmBox(null,string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
		                                                                                                        fileType,
		                                                                                                        fileName));
		    ScratchArea.CopyFileConflictFunction = CopyConfirmFileOverwrite;
            ScratchArea.CopyFileConflictFunction = CopyConfirmDirectoryOverwrite;
		}
		#endregion
	}
}
