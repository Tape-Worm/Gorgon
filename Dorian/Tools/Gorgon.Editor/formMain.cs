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
		// ReSharper disable FieldCanBeMadeReadOnly.Local
		/// <summary>
		/// The object being cut or copied.
		/// </summary>
		[Serializable]
		private struct CutCopyObject
		{
			#region Variables.
			/// <summary>
			/// Full path of the item being copied/cut.
			/// </summary>
			public string FullPath;

			/// <summary>
			/// Flag to indicate that the object is being cut.
			/// </summary>
			public bool IsCut;
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="CutCopyObject"/> struct.
			/// </summary>
			/// <param name="fullPath">Path to the object being cut/copied.</param>
			/// <param name="isCut">TRUE if the object is being cut instead of copied, FALSE if not.</param>
			public CutCopyObject(string fullPath, bool isCut)
			{
				FullPath = fullPath;
				IsCut = isCut;
			}
			#endregion
		}
		// ReSharper restore FieldCanBeMadeReadOnly.Local
		#endregion

		#region Variables.
		private static int _syncCounter;										// Synchronization counter for multiple threads.
		private RootNodeDirectory _rootNode;									// Our root node for the tree.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the file that is currently open.
		/// </summary>
		public GorgonFileSystemFileEntry CurrentOpenFile
		{
			get
			{
				return ScratchArea.CurrentOpenFile;
			}
			private set
			{
				ScratchArea.CurrentOpenFile = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Opening event of the popupFileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
		private void popupFileSystem_Opening(object sender, CancelEventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Handles the DropDownOpening event of the menuEdit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void menuEdit_DropDownOpening(object sender, EventArgs e)
		{
			ValidateControls();
		}

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
			var extensionList = new StringBuilder(512);
			int currentIndex = 0;

			dialog.Filter = string.Empty;
			dialog.DefaultExt = string.Empty;
			dialog.FilterIndex = 0;

			// Build the extension filter(s).
		    if (extensions != null)
		    {
			    var groupedExtensions = extensions.GroupBy(item => item.Description, StringComparer.OrdinalIgnoreCase);

		        foreach (var extensionGroup in groupedExtensions)
		        {
			        extensionList.Length = 0;

					if (filter.Length > 0)
					{
						filter.Append("|");
					}

			        filter.Append(extensionGroup.Key);

					foreach (var extension in extensionGroup)
			        {
				        if ((!string.IsNullOrWhiteSpace(allSupportedDesc))
				            && (allTypes.Length > 0))
				        {
					        allTypes.Append(";");
				        }

				        if (extensionList.Length > 0)
				        {
					        extensionList.Append(";");
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

				        extensionList.AppendFormat("*.{0}", extension.Extension);

				        if (!string.IsNullOrWhiteSpace(allSupportedDesc))
				        {
					        allTypes.AppendFormat("*.{0}", extension.Extension);
				        }
			        }

			        filter.AppendFormat("|{0}", extensionList);
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

	            TreeNodeDirectory directoryNode = GetSelectedDirectoryNode(false);

				Debug.Assert(directoryNode != null, "Directory for select node is NULL!");
                    
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
	        Action invokeAction =
		        () => result = GorgonDialogs.ConfirmBox(null,
		                                                string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
		                                                              Resources.GOREDIT_FILE_DEFAULT_TYPE,
		                                                              filePath),
		                                                null,
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
            Action invokeAction = () =>
                                  {
                                      Cursor current = Cursor.Current;

                                      result = GorgonDialogs.ConfirmBox(null,
                                                                        string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
                                                                                      Resources.GOREDIT_FILE_DEFAULT_TYPE,
                                                                                      filePath),
																		null,
                                                                        true,
                                                                        totalFileCount > 1);

                                      Cursor.Current = current;
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
        /// Function to confirm a directory overwrite operation.
        /// </summary>
        /// <param name="directoryPath">Path to the directory.</param>
        /// <param name="totalDirectoryCount">The total number of directories.</param>
        /// <returns>The result of the dialog operation.</returns>
        private ConfirmationResult CopyConfirmDirectoryOverwrite(string directoryPath, int totalDirectoryCount)
        {
            var result = ConfirmationResult.None;
            Action invokeAction = () =>
                                  {
                                      Cursor current = Cursor.Current;

                                      result = GorgonDialogs.ConfirmBox(null,
                                                                        string.Format(Resources.GOREDIT_OVERWRITE_DIRECTORY_PROMPT,
                                                                                      directoryPath),
																		null,
                                                                        true,
                                                                        totalDirectoryCount > 1);

                                      Cursor.Current = current;
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
        /// Handles the Click event of the itemExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemExport_Click(object sender, EventArgs e)
        {
            try
            {
                TreeNodeEditor selectedNode = treeFiles.SelectedNode;

                dialogExport.Description = Resources.GOREDIT_EXPORT_DLG_TITLE;
                dialogExport.SelectedPath = Program.Settings.ExportLastFilePath;
                dialogExport.ShowNewFolderButton = true;

                // Export the content data.
	            if (dialogExport.ShowDialog(this) != DialogResult.OK)
	            {
		            return;
	            }
				
	            Cursor.Current = Cursors.WaitCursor;

				if ((selectedNode.NodeType & NodeType.File) == NodeType.File)
				{
					GorgonFileSystemFileEntry file = ((TreeNodeFile)treeFiles.SelectedNode).File;

					// If we have dependencies, then ask to export those as well.
					if (Program.EditorMetaData.Dependencies.ContainsKey(file.FullPath))
					{
						ConfirmationResult result = GorgonDialogs.ConfirmBox(this,
						                                                     string.Format(Resources.GOREDIT_EXPORT_EXPORT_DEPENDENCIES,
						                                                                   file.Name), null, true);

						if (result == ConfirmationResult.Cancel)
						{
							return;
						}

						if (result == ConfirmationResult.Yes)
						{
							var filePaths = new HashSet<string>();
							
							// Merge file paths together in a hash set so we don't get attempts to overwrite the same
							// file over and over.  This can happen because the same dependency can appear multiple
							// times for a file.
							foreach (Dependency dependency in Program.EditorMetaData.Dependencies[file.FullPath])
							{
								if (!filePaths.Contains(dependency.Path.ToLowerInvariant()))
								{
									filePaths.Add(dependency.Path.ToLowerInvariant());
								}
							}

							// Export dependency files.
							foreach (string path in filePaths)
							{
								GorgonFileSystemFileEntry dependFile = ScratchArea.ScratchFiles.GetFile(path);

								if (dependFile == null)
								{
									continue;
								}

								ScratchArea.Export(dependFile, dialogExport.SelectedPath, false, filePaths.Count);
							}
						}
					}

                    ScratchArea.Export(file, dialogExport.SelectedPath, false);
                } 
                
                if ((selectedNode.NodeType & NodeType.Directory) == NodeType.Directory)
                {
					ScratchArea.Export(((TreeNodeDirectory)treeFiles.SelectedNode).Directory, dialogExport.SelectedPath);
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
	        bool dependencies = false;
            TreeNodeEditor selectedNode = treeFiles.SelectedNode;

            Text = string.Format("{0} - {1}", FileManagement.Filename, Resources.GOREDIT_DEFAULT_TITLE);

			Debug.Assert(_rootNode != null, "Root node is NULL!");

			_rootNode.Redraw();

	        IDataObject clipboardData = Clipboard.GetDataObject();

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

            // No node is the same as selecting the root.
            if (selectedNode == null)
            {
				selectedNode = treeFiles.SelectedNode = _rootNode;
            }

			if ((selectedNode.NodeType & NodeType.Directory) == NodeType.Directory)
	        {
				// Don't check the root node.
		        if (treeFiles.SelectedNode != _rootNode)
		        {
			        dependencies = Program.EditorMetaData.HasFileLinks(((TreeNodeDirectory)selectedNode).Directory);
		        }

		        toolStripSeparator4.Visible = true;
		        popupItemAddContent.Visible = itemAddContent.Enabled = itemAddContent.DropDownItems.Count > 0;
		        popupItemAddContent.Enabled = itemAddContent.Enabled;
		        dropNewContent.Enabled = dropNewContent.DropDownItems.Count > 0;
		        buttonDeleteContent.Enabled = !dependencies;
		        popupItemCreateFolder.Enabled = itemCreateFolder.Enabled = true;
		        popupItemCreateFolder.Visible = true;
		        itemDelete.Enabled = popupItemDelete.Enabled = (!dependencies) && (tabDocumentManager.SelectedTab == pageItems);
		        itemDelete.Text = popupItemDelete.Text = string.Format("{0}...", Resources.GOREDIT_MENU_DELETE_FOLDER);
		        popupItemPaste.Enabled = itemPaste.Enabled = clipboardData != null && clipboardData.GetDataPresent(typeof(CutCopyObject));

		        if (treeFiles.SelectedNode != _rootNode)
		        {
					popupItemCut.Enabled = popupItemCopy.Enabled = itemCopy.Enabled = itemCut.Enabled = true;
				    popupItemRename.Enabled = !dependencies;
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

	        GorgonFileSystemFileEntry file;

			// If this is a dependency node, then we have very limited options.
	        if ((selectedNode.NodeType & NodeType.Dependency) == NodeType.Dependency)
	        {
				file = ((TreeNodeFile)selectedNode).File;

				toolStripSeparator4.Visible =
					buttonEditContent.Enabled =
					popupItemEdit.Visible = popupItemEdit.Enabled = ContentManagement.CanOpenContent(file.Extension);
				popupItemAddContent.Visible = false;
		        return;
	        }

            if ((selectedNode.NodeType & NodeType.File) != NodeType.File)
            {
                return;
            }

            file = ((TreeNodeFile)selectedNode).File;

	        dependencies = Program.EditorMetaData.HasFileLinks(file);

			toolStripSeparator4.Visible =
				buttonEditContent.Enabled =
				popupItemEdit.Visible = popupItemEdit.Enabled = ContentManagement.CanOpenContent(file.Extension);
			popupItemAddContent.Visible = false;
			popupItemPaste.Enabled = itemPaste.Enabled = clipboardData != null && clipboardData.GetDataPresent(typeof(CutCopyObject));
	        popupItemCopy.Enabled = itemCopy.Enabled = true;

	        if (dependencies)
	        {
		        return;
	        }

	        popupItemCut.Enabled = itemCut.Enabled = true;
	        buttonDeleteContent.Enabled = true;
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
		/// Function to determine whether to save any changed content or not.
		/// </summary>
		private ConfirmationResult ConfirmSaveContent()
		{
			var result = ConfirmationResult.No;

			if ((!ContentManagement.Changed)
			    || (CurrentOpenFile == null))
			{
				return result;
			}

			result = GorgonDialogs.ConfirmBox(this,
			                                  string.Format(Resources.GOREDIT_DLG_CONTENT_CHANGED_SAVE,
			                                                ContentManagement.Current.ContentType,
			                                                ContentManagement.Current.Name),
			                                  null,
			                                  true);

			if (result != ConfirmationResult.Yes)
			{
				return result;
			}

			ContentManagement.Save(CurrentOpenFile);
			FileManagement.FileChanged = true;

			return result;
		}
        
        /// <summary>
		/// Handles the NodeMouseDoubleClick event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeNodeMouseClickEventArgs"/> instance containing the event data.</param>
		private void treeFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
	        var editNode = e.Node as TreeNodeEditor;
			Cursor.Current = Cursors.WaitCursor;
			
			// Only open files.
	        if ((editNode == null)
				|| (((editNode.NodeType & NodeType.File) != NodeType.File)
				&& ((editNode.NodeType & NodeType.Dependency) != NodeType.Dependency)))
	        {
		        return;
	        }
			
			// Do not re-open the file.
	        var fileNode = (TreeNodeFile)e.Node;

	        if (fileNode.File == CurrentOpenFile)
	        {
		        return;
	        }

            Cursor.Current = Cursors.WaitCursor;

			try
			{
				ContentPlugIn plugIn = ContentManagement.GetContentPlugInForFile(fileNode.File.Name);

				// Ensure we can actually open this type.
				if (plugIn == null)
				{
					GorgonDialogs.ErrorBox(this, string.Format(Resources.GOREDIT_NO_CONTENT_PLUG_IN_FOR_FILE, fileNode.File.Name, fileNode.File.Extension));
					return;
				}

				ConfirmationResult result = ConfirmSaveContent();

				if (result == ConfirmationResult.Cancel)
				{
					return;
				}

                ContentManagement.Load(fileNode.File, plugIn);
                CurrentOpenFile = fileNode.File;
                treeFiles.Refresh();

				if (ContentManagement.Current.HasProperties)
				{
					propertyItem.Refresh();
				}
			}
			catch (Exception ex)
			{
			    CurrentOpenFile = null;
				ContentManagement.LoadDefaultContentPane();
                GorgonDialogs.ErrorBox(this, string.Format(Resources.GOREDIT_CONTENT_OPEN_ERROR, fileNode.File.Name), null, ex);
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
                // Only repaint the property grid in this case.
                if (changedArgs.Repaint)
                {
                    propertyItem.Refresh();
                    return;
                }

                // If we change the name of an item, we must rename it.
				if (!changedArgs.PropertyName.Equals("name", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				// Find our node that corresponds to this content.
				var node = treeFiles.GetCurrentContentNode(CurrentOpenFile);

				// If the node isn't here, then it may not have been enumerated, so it's OK if we get out.
				if (node == null)
				{
					return;
				}

				RenameFileNode(node, changedArgs.Value.ToString());
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
		/// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
		private void OnContentClose(object sender, CancelEventArgs e)
		{
			var panel = (ContentPanel)sender;

			// Persist the content.
			ConfirmationResult result = ConfirmSaveContent();

			if (result == ConfirmationResult.Cancel)
			{
				e.Cancel = true;
				return;
			}

			// Turn off the event.
			panel.ContentClosed -= OnContentClose;

			// Update the node dependencies if any exist.
			TreeNodeFile currentNode = treeFiles.GetCurrentContentNode(CurrentOpenFile);

			if (currentNode != null)
			{
				GetDependencyNodes(currentNode);
			}
			
            CurrentOpenFile = null;
		}

		/// <summary>
		/// Handles the Click event of the itemPaste control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemPaste_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			
			try
			{
                // Use a lockless pattern to keep this method from becoming reentrant.
			    if (Interlocked.Increment(ref _syncCounter) > 1)
			    {
			        return;
			    }

				IDataObject clipData = Clipboard.GetDataObject();

			    if ((clipData == null)
					|| (!clipData.GetDataPresent(typeof(CutCopyObject))))
				{
					return;
				}

				var cutCopyObject = (CutCopyObject)clipData.GetData(typeof(CutCopyObject));

				if (cutCopyObject.FullPath == null)
				{
					return;
				}

				// Find the node that we're moving.
				TreeNodeEditor node = treeFiles.AllNodes()
				                               .FirstOrDefault(item =>
				                                               string.Equals(item.FullPath,
				                                                             cutCopyObject.FullPath,
				                                                             StringComparison.OrdinalIgnoreCase));

				if (node == null)
				{
					return;
				}

				// If we're cutting/copying some tree node, then find the current node
				// and perform the operation.
				node.IsCut = false;
				node.Redraw();

				TreeNodeDirectory currentNode = GetSelectedDirectoryNode(true);

				if (currentNode == null)
				{
					GorgonDialogs.ErrorBox(this, Resources.GOREDIT_PASTE_MUST_BE_DIRECTORY);
					return;
				}

				// If we're moving, there are some restrictions.
				if ((currentNode == node) && (cutCopyObject.IsCut))
				{
					GorgonDialogs.ErrorBox(this, Resources.GOREDIT_FILE_SOURCE_SAME_AS_DEST);
					return;
				}

				if ((cutCopyObject.IsCut) && (currentNode.IsAncestorOf(node)))
				{
					GorgonDialogs.ErrorBox(this, Resources.GOREDIT_FILE_CANNOT_MOVE_TO_CHILD);
					return;
				}

				MoveCopyNode(node, currentNode, !cutCopyObject.IsCut, cutCopyObject);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
                ValidateControls();
                Cursor.Current = Cursors.Default;
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

				treeFiles.SelectedNode.Redraw();

				var cutCopyObject = new CutCopyObject(treeFiles.SelectedNode.FullPath,
				                                      ((sender == itemCut) || (sender == popupItemCut)));

				Clipboard.SetDataObject(cutCopyObject);
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
			                                                    FileManagement.Filename),
			                                      null,
			                                      true);

			if (result == ConfirmationResult.Cancel)
			{
				return true;
			}

			if (result != ConfirmationResult.Yes)
			{
				return false;
			}

			// If we haven't saved the file yet, then prompt us with a file name.
			if (string.IsNullOrWhiteSpace(FileManagement.FilePath))
			{
				itemSaveAs.PerformClick();
			}
			else
			{
				itemSave.PerformClick();
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
				ContentManagement.ContentPanelUnloadAction = null;
				ContentManagement.ContentEnumerateProperties = null;
				ContentManagement.ContentInitializedAction = null;
				ContentManagement.OnGetDependency = null;
				ContentManagement.DependencyNotFound = null;


                // Unhook from file management functionality.
			    ScratchArea.ImportExportFileConflictFunction = null;
                ScratchArea.ImportExportFileCompleteAction = null;
			    ScratchArea.ExceptionAction = null;
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
			Point cursor = treeFiles.PointToClient(Cursor.Position);

			try
			{
				var directoryNode = e.Node as TreeNodeDirectory;

				// Expand sub folders.
				if (directoryNode != null)
				{
					GetFolders(directoryNode);
					return;
				}

				var fileNode = e.Node as TreeNodeFile;
				
				if ((fileNode == null)
					|| (fileNode.NodeType != NodeType.File)
					|| (fileNode.File == null)
				    || (fileNode.Nodes.Count == 0)
					|| (!Program.EditorMetaData.Dependencies.ContainsKey(fileNode.File.FullPath)))
				{
					return;
				}
				
				// Disable double click to expand the node.
				if ((e.Action == TreeViewAction.ByMouse)
					&& (cursor.X >= e.Node.Bounds.Left))
				{
					e.Cancel = true;
					return;
				}

				GetDependencyNodes(fileNode);
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
		/// Function to retrieve the dependency nodes.
		/// </summary>
		/// <param name="fileNode">Node containing the dependencies.</param>
		private static void GetDependencyNodes(TreeNodeFile fileNode)
		{
			fileNode.Nodes.Clear();

			if (!Program.EditorMetaData.Dependencies.ContainsKey(fileNode.File.FullPath))
			{
				return;
			}

			DependencyCollection dependencies = Program.EditorMetaData.Dependencies[fileNode.File.FullPath];

			foreach (Dependency dependencyFile in dependencies)
			{
				GorgonFileSystemFileEntry fileEntry = ScratchArea.ScratchFiles.GetFile(dependencyFile.Path);
				var dependencyNode = new TreeNodeDependency();

				if (fileEntry == null)
				{
					dependencyNode.UpdateBroken(dependencyFile);
					continue;
				}

				dependencyNode.UpdateFile(fileEntry);
				fileNode.Nodes.Add(dependencyNode);
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
			foreach (var fileEntry in from file in rootNode.Directory.Files.OrderBy(item => item.Name) 
                                     where !ScratchArea.IsBlocked(file) 
                                     select file)
			{
				var fileNode = new TreeNodeFile();
				fileNode.UpdateFile(fileEntry);
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
			return !directory.Files.All(ScratchArea.IsBlocked);
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
        /// <param name="expandDirectory">TRUE to expand the directory for the selected node, FALSE to leave alone.</param>
        /// <returns>The selected node.</returns>
        private TreeNodeDirectory GetSelectedDirectoryNode(bool expandDirectory)
        {
	        if (treeFiles.SelectedNode == null)
	        {
				Debug.Assert(_rootNode != null, "Root should not be NULL!");

		        if (expandDirectory)
		        {
			        _rootNode.Expand();
		        }

		        treeFiles.SelectedNode = _rootNode;

		        return _rootNode;
	        }

	        var directory = treeFiles.SelectedNode as TreeNodeDirectory;		

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

		        if (expandDirectory)
		        {
			        directory = (TreeNodeDirectory)parentNode;
		        }
	        }

	        Debug.Assert(directory != null, "Directory should not be NULL.");

	        if (expandDirectory)
	        {
		        directory.Expand();
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
		    TreeNodeDirectory directoryNode = GetSelectedDirectoryNode(true);
			TreeNodeFile newNode = null;
			GorgonFileSystemFileEntry newFile = null;
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
				if (ConfirmSaveContent() == ConfirmationResult.Cancel)
				{
					return;
				}

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

				// Load the content.
				ContentManagement.LoadContentPane(content);

                // Create the file in the scratch area.
                newFile = ScratchArea.CreateFile(content.Name, content.ContentType, directoryNode.Directory);

                // The file did not get created, then display an error to that effect.
                if (newFile == null)
			    {
					throw new IOException(string.Format(Resources.GOREDIT_CONTENT_CANNOT_CREATE_FILE, content.Name));
			    }
				
				// Persist the content to the file.
				ContentManagement.Save(newFile);

                // Create the file node in our tree.
                newNode = new TreeNodeFile();
				newNode.UpdateFile(newFile);

                // Add to tree and select.
                directoryNode.Nodes.Add(newNode);
                treeFiles.SelectedNode = newNode;

                // We set this to true to indicate that this is a new file.
                FileManagement.FileChanged = true;
				CurrentOpenFile = newFile;
			}
			catch (Exception ex)
			{
                // Load the default pane.
                ContentManagement.LoadDefaultContentPane();
				
				if (content != null)
				{
					content.Dispose();
				}

				// Roll back the changes to the file system.
				if (newFile != null)
				{
					try
					{
						// Destroy this file.
						ScratchArea.ScratchFiles.DeleteFile(newFile);
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

                GorgonDialogs.ErrorBox(this, ex);
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
			IDataObject clipData = Clipboard.GetDataObject();

			if ((clipData != null)
				&& (clipData.GetDataPresent(typeof(CutCopyObject))))
			{
				var cutCopyObject = (CutCopyObject)clipData.GetData(typeof(CutCopyObject));

				if ((cutCopyObject.FullPath != null)
				    && (string.Equals(cutCopyObject.FullPath, node.FullPath, StringComparison.OrdinalIgnoreCase)))
				{
					Clipboard.Clear();
				}
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
                if (treeFiles.SelectedNode == null)
                {
	                return;
                }

			    TreeNodeEditor selectedNode = treeFiles.SelectedNode;

				if ((selectedNode.NodeType & NodeType.Directory) == NodeType.Directory)
				{
					var directoryNode = (TreeNodeDirectory)treeFiles.SelectedNode;
					string sourcePath = directoryNode.Directory.FullPath;

                    if (GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_FILE_DELETE_DIRECTORY_CONFIRM, sourcePath)) == ConfirmationResult.No)
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

					var dependencies =
						Program.EditorMetaData.Dependencies.Where(item =>
																  item.Key.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase))
							   .Select(item => item.Key).ToArray();

					if (dependencies.Length > 0)
					{
						foreach (string dependency in dependencies)
						{
							Program.EditorMetaData.Dependencies.Remove(dependency);
						}

						Program.EditorMetaData.Save();
					}


                    PruneTree(directoryNode);
				}

                if ((selectedNode.NodeType & NodeType.File) == NodeType.File)
				{
					var fileNode = (TreeNodeFile)treeFiles.SelectedNode;
					string sourceFilePath = fileNode.File.FullPath;

					if (GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_FILE_DELETE_FILE_CONFIRM, sourceFilePath)) == ConfirmationResult.No)
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

					// Remove the dependency link.
					if (Program.EditorMetaData.Dependencies.ContainsKey(sourceFilePath))
					{
						Program.EditorMetaData.Dependencies.Remove(sourceFilePath);
						Program.EditorMetaData.Save();
					}

					PruneTree(fileNode);
				}

			    FileManagement.FileChanged = true;
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
				var node = e.Item as TreeNodeEditor;
				
				if ((node != null) && (node != _rootNode) && (node.NodeType != NodeType.Dependency))
				{
					treeFiles.DoDragDrop(new Tuple<TreeNodeEditor, MouseButtons>(node, e.Button), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
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
			var node = e.Node as TreeNodeEditor;
			string label = e.Label;


			e.CancelEdit = true;

			// If this is not an editor node, then cancel the editing process.
			if (node == null)
			{
				return;
			}

			// We can't have blank names.
			if (string.IsNullOrWhiteSpace(label))
			{
				// Destroy the new node if we've entered a blank name.
				if (node.EditState == NodeEditState.CreateDirectory)
				{
					e.Node.Remove();
				}

				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (node.EditState == NodeEditState.RenameFile)
				{
				    RenameFileNode((TreeNodeFile)node, label);
				}

			    if (node.EditState == NodeEditState.RenameDirectory)
			    {
			        RenameDirectoryNode((TreeNodeDirectory)node, label);
			    }
                
			    if (node.EditState != NodeEditState.CreateDirectory)
			    {
			        return;
			    }

			    var parentNode = node.Parent as TreeNodeDirectory;

			    Debug.Assert(parentNode != null, "Parent node is NULL!");
                 
			    var newDirectory = ScratchArea.CreateDirectory(parentNode.Directory, label);

			    // Set up a new node for the directory since our current node is here as a proxy.
			    var treeNode = new TreeNodeDirectory(newDirectory);

			    int nodeIndex = parentNode.Nodes.IndexOf(e.Node);
			    parentNode.Nodes.Insert(nodeIndex, treeNode);
			    // Remove proxy node.						
			    e.Node.Remove();

			    FileManagement.FileChanged = true;
			}
			catch (Exception ex)
			{
				// If we get an error, just remove any new nodes.
				if ((node.EditState == NodeEditState.CreateDirectory) || (node.EditState == NodeEditState.CreateFile))
				{
					node.Remove();
				}

				e.CancelEdit = true;
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				treeFiles.Refresh();
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
			try
			{
				if (treeFiles.SelectedNode == null)
				{
					return;
				}

				treeFiles.SelectedNode.EditState = (treeFiles.SelectedNode.NodeType & NodeType.Directory) == NodeType.Directory
					                                   ? NodeEditState.RenameDirectory
					                                   : NodeEditState.RenameFile;

				treeFiles.SelectedNode.BeginEdit();
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
		/// Handles the Click event of the itemCreateFolder control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemCreateFolder_Click(object sender, EventArgs e)
		{
			bool expandDisabled = false;
			var tempNode = new TreeNodeDirectory();

			try
			{
				int nameIndex = 0;
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
					defaultName = string.Format("{0} ({1})", Resources.GOREDIT_FILE_DEFAULT_DIRECTORY_NAME, ++nameIndex);
				}

				tempNode.Text = defaultName;

			    selectedNode.Nodes.Add(tempNode);

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
        /// Function to rename a directory node.
        /// </summary>
        /// <param name="sourceDirNode">The directory to rename.</param>
        /// <param name="newName">The new name for the directory.</param>
        private void RenameDirectoryNode(TreeNodeDirectory sourceDirNode, string newName)
        {
	        string[] files = null;
	        string sourcePath = sourceDirNode.Directory.FullPath;
            string openContentPath = string.Empty;

            if ((CurrentOpenFile != null) && (CurrentOpenFile.FullPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase)))
            {
                openContentPath = (CurrentOpenFile.Directory.FullPath.Replace(sourcePath,
                                                                              sourceDirNode.Directory.Parent.FullPath +
                                                                              newName)).FormatDirectory('/')
                                  + CurrentOpenFile.Name;
            }

	        if (Program.EditorMetaData.Dependencies.Any(item => item.Key.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase)))
	        {
		        files = ScratchArea.ScratchFiles.FindFiles(sourcePath, "*", true).Select(item => item.FullPath).ToArray();
	        }

            GorgonFileSystemDirectory newDirectory = ScratchArea.Rename(sourceDirNode.Directory, newName);

            if (newDirectory == null)
            {
                return;
            }

			// Update any dependencies.
			if ((files != null) && (files.Length > 0))
			{
				foreach (string file in files)
				{
					if (file.Length == sourcePath.Length)
					{
						continue;
					}

					string destPath = newDirectory.FullPath + file.Substring(sourcePath.Length);
					UpdateDependencyList(file, destPath, false, false);
				}
				Program.EditorMetaData.Save();
			}

            sourceDirNode.Text = newDirectory.Name;
            sourceDirNode.Name = newDirectory.FullPath;

            // Collapse and expand the node to refresh the nodes.
            if ((sourceDirNode.Nodes.Count > 0)
                && (sourceDirNode.IsExpanded))
            {
                sourceDirNode.Collapse();
                sourceDirNode.Expand();
            }

            FileManagement.FileChanged = true;

            if (string.IsNullOrWhiteSpace(openContentPath))
            {
                return;
            }

            // Update the current name in the current content.
            CurrentOpenFile = ScratchArea.ScratchFiles.GetFile(openContentPath);

            // If for some reason we cannot update the current open file, then close it.
            // We will lose the changes here, but it's unavoidable because something's gone quite wrong.
            if (CurrentOpenFile == null)
            {
                ContentManagement.LoadDefaultContentPane();
            }
        }


        /// <summary>
        /// Function to rename a file node.
        /// </summary>
        /// <param name="sourceFileNode">The file to rename.</param>
        /// <param name="newName">The new name for the file.</param>
        private void RenameFileNode(TreeNodeFile sourceFileNode, string newName)
        {
	        string sourcePath = sourceFileNode.File.FullPath;
	        bool openContentRenamed = sourceFileNode.File == CurrentOpenFile;
			
			GorgonFileSystemFileEntry newFile = ScratchArea.Rename(sourceFileNode.File, newName);

			if (newFile == null)
			{
				return;
			}

			// If we have this file in the dependency list, then rename it there too.
			UpdateDependencyList(sourcePath, newFile.FullPath, false, true);

			sourceFileNode.Text = newFile.Name;
			sourceFileNode.Name = newFile.FullPath;

			FileManagement.FileChanged = true;

			if (!openContentRenamed)
			{
				return;
			}

			// Update the current name in the current content.
			ContentManagement.Current.Name = newFile.Name;
			CurrentOpenFile = newFile;
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

				var overNode = (TreeNodeEditor)treeFiles.GetNodeAt(treeFiles.PointToClient(new Point(e.X, e.Y)));
				var destDir = overNode as TreeNodeDirectory;

				// Handle explorer files.
				if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				{
					var dropData = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
					var files = new List<string>(dropData.Where(item =>
					                                            !item.StartsWith(ScratchArea.ScratchFiles.WriteLocation,
					                                                             StringComparison.OrdinalIgnoreCase)));

					if (files.Count > 0)
					{
						AddFilesFromExplorer(destDir, files);						
					}					
					return;
				}

				// If we're moving one of our directories or files, then process those items.
			    if (!e.Data.GetDataPresent(typeof(Tuple<TreeNodeEditor, MouseButtons>)))
			    {
			        return;
			    }

			    var data = (Tuple<TreeNodeEditor, MouseButtons>)e.Data.GetData(typeof(Tuple<TreeNodeEditor, MouseButtons>));

				MoveCopyNode(data.Item1, destDir, data.Item2 != MouseButtons.Left);
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
        /// Function to move or copy a directory node.
        /// </summary>
        /// <param name="sourceNode">Source node directory to move/copy.</param>
        /// <param name="destinationNode">The destination directory for the copied/moved node.</param>
        /// <param name="isCopy">TRUE to copy the directory, FALSE to move.</param>
        /// <returns>The new node.</returns>
	    private TreeNodeEditor MoveCopyNodeDirectory(TreeNodeDirectory sourceNode,
	                                                 TreeNodeDirectory destinationNode,
	                                                 bool isCopy)
        {
            TreeNodeDirectory result;
            string openContent = string.Empty;

            if (sourceNode == null)
            {
                return null;
            }

            treeFiles.BeginUpdate();
            try
            {
	            string[] files = null;
	            string sourcePath = sourceNode.Directory.FullPath;

                // If this or one of the ancestor directories contains the file that is currently open, then save it and reopen it after the move.
                if ((!isCopy)
                    && (CurrentOpenFile != null)
					&& (CurrentOpenFile.FullPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase)))
                {
	                openContent = (CurrentOpenFile.Directory.FullPath.Replace(sourcePath,
	                                                                          destinationNode.Directory.FullPath +
	                                                                          sourceNode.Directory.Name)).FormatDirectory('/') +
	                              CurrentOpenFile.Name;
                }

	            if (Program.EditorMetaData.Dependencies.Any(item => item.Key.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase)))
	            {
		            files = ScratchArea.ScratchFiles.FindFiles(sourcePath, "*", true).Select(item => item.FullPath).ToArray();
	            }

	            GorgonFileSystemDirectory newDirectory = isCopy
		                                                     ? ScratchArea.Copy(sourceNode.Directory,
		                                                                        destinationNode.Directory.FullPath)
		                                                     : ScratchArea.Move(sourceNode.Directory,
		                                                                        destinationNode.Directory.FullPath);

                if (newDirectory == null)
                {
                    return null;
                }

				// Update any dependencies.
	            if ((files != null) && (files.Length > 0))
	            {
		            foreach (string file in files)
		            {
			            if (file.Length == sourcePath.Length)
			            {
				            continue;
			            }

			            string destPath = newDirectory.FullPath + file.Substring(sourcePath.Length);
						UpdateDependencyList(file, destPath, isCopy, false);
		            }
					Program.EditorMetaData.Save();
	            }

                result = destinationNode.Nodes.AddDirectory(newDirectory, sourceNode.IsExpanded);

                // Remove the file if this is a move operation.
                if (!isCopy)
                {
                    sourceNode.Remove();
                }

                FileManagement.FileChanged = true;

                // If we've moved the file that's already open, then update its location.
                if (string.IsNullOrWhiteSpace(openContent))
                {
                    return result;
                }

                CurrentOpenFile = ScratchArea.ScratchFiles.GetFile(openContent);
            }
            finally
            {
                treeFiles.EndUpdate();
            }
            
            return result;
        }
		
		/// <summary>
		/// Function to update the dependency list when a file is renamed or moved.
		/// </summary>
		/// <param name="sourceFile">Source file that contains the dependencies.</param>
		/// <param name="destFile">Destination file to contain the dependencies.</param>
		/// <param name="isCopy">TRUE if the update is from a copy operation, FALSE if it's from a cut operation.</param>
		/// <param name="saveMetaData">TRUE to save the meta data file, FALSE to leave alone.</param>
		private static void UpdateDependencyList(string sourceFile, string destFile, bool isCopy, bool saveMetaData)
		{
			if (!Program.EditorMetaData.Dependencies.ContainsKey(sourceFile))
			{
				return;
			}

			DependencyCollection dependencies = Program.EditorMetaData.Dependencies[sourceFile];

			if (!isCopy)
			{
				Program.EditorMetaData.Dependencies.Remove(sourceFile);
				Program.EditorMetaData.Dependencies[destFile] = dependencies;
				Program.EditorMetaData.Save();
				return;
			}

			Program.EditorMetaData.Dependencies[destFile] = new DependencyCollection();

			// Copy the dependency list.
			foreach (Dependency dependency in dependencies)
			{
				var newDependency = new Dependency(dependency.Path, dependency.Type);

				foreach (DependencyProperty property in dependency.Properties)
				{
					newDependency.Properties[property.Name] = property;
				}

				Program.EditorMetaData.Dependencies[destFile][newDependency.Path, newDependency.Type] = newDependency;
			}

			if (!saveMetaData)
			{
				return;
			}

			Program.EditorMetaData.Save();
		}

		/// <summary>
		/// Function to move or copy a file node.
		/// </summary>
		/// <param name="sourceNode">Source node file to move/copy.</param>
		/// <param name="destinationNode">The destination directory for the copied/moved node.</param>
		/// <param name="isCopy">TRUE to copy the file, FALSE to move.</param>
		/// <returns>The new node.</returns>
		private TreeNodeEditor MoveCopyNodeFile(TreeNodeFile sourceNode, TreeNodeDirectory destinationNode, bool isCopy)
		{
		    TreeNodeFile result;
			bool updateOpenContent = false;

			if (sourceNode == null)
			{
				return null;
			}

            treeFiles.BeginUpdate();
		    try
		    {
			    string sourcePath = sourceNode.File.FullPath;

		        if ((sourceNode.File == CurrentOpenFile)
		            && (!isCopy))
		        {
		            CurrentOpenFile = null;
		            updateOpenContent = true;
		        }

		        // Copy/move the file.
		        GorgonFileSystemFileEntry newFile = isCopy
		                                                ? ScratchArea.Copy(sourceNode.File,
		                                                                   destinationNode.Directory.FullPath,
		                                                                   false)
		                                                : ScratchArea.Move(sourceNode.File,
		                                                                   destinationNode.Directory.FullPath,
		                                                                   false);

		        if (newFile == null)
		        {
		            return null;
		        }

				UpdateDependencyList(sourcePath, newFile.FullPath, isCopy, true);

		        result = destinationNode.Nodes.AddFile(newFile);

		        // Remove the file if this is a move operation.
		        if (!isCopy)
		        {
		            sourceNode.Remove();
		        }

		        FileManagement.FileChanged = true;
                
		        // If we've moved the file that's already open, then update its location.
		        if (!updateOpenContent)
		        {
		            return result;
		        }

                CurrentOpenFile = newFile;
		    }
		    finally
		    {
                treeFiles.EndUpdate();
		    }

			return result;
		}

		/// <summary>
		/// Function to perform a move or a copy on a tree node.
		/// </summary>
		/// <param name="sourceNode">Source node to move/copy.</param>
		/// <param name="destinationNode">Destination directory node that will receive the copied/moved node.</param>
		/// <param name="isCopy">TRUE if the function should copy instead of move.</param>
		/// <param name="cutCopyObject">The reference to the object being cut in a cut operation.</param>
		private void MoveCopyNode(TreeNodeEditor sourceNode, TreeNodeDirectory destinationNode, bool isCopy, CutCopyObject? cutCopyObject = null)
		{
			TreeNodeEditor newNode = null;
			string sourcePath = sourceNode.FullPath;

			if (destinationNode == null)
			{
				return;
			}

			try
			{
				// Our source data is a directory, so move it.
				if ((sourceNode.NodeType & NodeType.Directory) == NodeType.Directory)
				{
				    var directory = sourceNode as TreeNodeDirectory;
				    newNode = MoveCopyNodeDirectory(directory, destinationNode, isCopy);
				}

				if ((sourceNode.NodeType & NodeType.File) == NodeType.File)
				{
					// We didn't have a directory, so move the file.
					var file = sourceNode as TreeNodeFile;
					newNode = MoveCopyNodeFile(file, destinationNode, isCopy);
				}
				
				// Do not select any node if no node is returned.
				if (newNode == null)
				{
					return;
				}

				// Select the moved/copied file.
                //treeFiles.Sort();
				treeFiles.SelectedNode = newNode;
			}
			finally
			{
				// If this object was marked for a cut operation, then remove it from the "clipboard".
				if ((cutCopyObject != null) && (cutCopyObject.Value.FullPath != null) 
					&& (string.Equals(cutCopyObject.Value.FullPath, sourcePath, StringComparison.OrdinalIgnoreCase)) 
					&& (!isCopy))
				{
					Clipboard.Clear();
				}
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
				var overNode = treeFiles.GetNodeAt(treeFiles.PointToClient(new Point(e.X, e.Y))) as TreeNodeEditor;
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

			    if (!e.Data.GetDataPresent(typeof(Tuple<TreeNodeEditor, MouseButtons>)))
			    {
			        return;
			    }

			    // Get our source data.
			    var dragData = (Tuple<TreeNodeEditor, MouseButtons>)e.Data.GetData(typeof(Tuple<TreeNodeEditor, MouseButtons>));
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
			if (DesignMode)
			{
				base.OnLoad(e);
				return;
			}

			LocalizeControls();

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

			// Set up linkage to the content management interface.
			ContentManagement.ContentPanelUnloadAction = () =>
			                                             {
															 // Switch back to the content item list.
				                                             tabDocumentManager.SelectedTab = pageItems;
				                                             pageProperties.Enabled = false;

															 // Unassign the currently selected object from the property
															 // pane.
				                                             propertyItem.SelectedObject = null;
															 propertyItem.Refresh();
				                                             treeFiles.Refresh();
			                                             };
			ContentManagement.ContentEnumerateProperties = hasProperties =>
			{
				propertyItem.PropertyValueChanged -= OnPropertyValueChanged;
				ContentManagement.ContentPropertyChanged = null;

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
				}
			};
			ContentManagement.ContentInitializedAction = control =>
			                                             {
															control.Dock = DockStyle.Fill;
															control.Parent = splitPanel1;
				                                            control.ContentClosed += OnContentClose;
			                                             };
			ContentManagement.OnGetDependency = pathToDependency => ScratchArea.ScratchFiles.GetFile(pathToDependency);
			ContentManagement.DependencyNotFound = (sourceFile, dependencyList) =>
			                                       {
				                                       var displayText = new StringBuilder(512);

				                                       foreach (string dependencyPath in dependencyList)
				                                       {
					                                       if (displayText.Length > 0)
					                                       {
						                                       displayText.Append("\n");
					                                       }

					                                       displayText.Append(dependencyPath);
				                                       }

				                                       displayText.Insert(0, "\n");

				                                       GorgonDialogs.WarningBox(this,
				                                                                string.Format(Resources.GOREDIT_CANNOT_FIND_DEPENDENCY_WARN,
				                                                                              sourceFile),
				                                                                null,
				                                                                displayText.ToString());
			                                       };

            // Assign file management linkage.
			ScratchArea.CanImportFunction = EvaluateFileImport;
			ScratchArea.ExceptionAction = FileCopyException;
            ScratchArea.ImportExportFileConflictFunction = ImportConfirmFileOverwrite;
            ScratchArea.ImportExportFileCompleteAction = FileImportExportCompleted;
		    ScratchArea.CreateFileConflictFunction = (fileName, fileType) => GorgonDialogs.ConfirmBox(null,string.Format(Resources.GOREDIT_OVERWRITE_FILE_PROMPT,
		                                                                                                        fileType,
		                                                                                                        fileName));
		    ScratchArea.CopyFileConflictFunction = CopyConfirmFileOverwrite;
            ScratchArea.CopyDirectoryConflictFunction = CopyConfirmDirectoryOverwrite;
		}
		#endregion
	}
}
