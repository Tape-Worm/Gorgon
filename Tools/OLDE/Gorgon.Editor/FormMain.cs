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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// Main application object.
	/// </summary>
	partial class FormMain
		: FlatForm
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
			/// <param name="isCut"><b>true</b> if the object is being cut instead of copied, <b>false</b> if not.</param>
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
		private bool _clipboardData;											// Flag to indicate that there's clipboard data.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return whether the current content has been changed.
        /// </summary>
	    private static bool HasContentChanged
	    {
	        get
	        {
	            return ContentManagement.Current != null && ContentManagement.Current.HasChanges;
	        }
	    }

		/// <summary>
		/// Property to return the file that is currently open.
		/// </summary>
		public GorgonFileSystemFileEntry CurrentOpenFile
		{
			get
			{
				return ContentManagement.ContentFile;
			}
			private set
			{
				ContentManagement.ContentFile = value;
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to include/exclude all files under a directory.
        /// </summary>
        /// <param name="directory">The directory containing the files.</param>
        /// <param name="exclude"><b>true</b> to exclude the files, <b>false</b> to include.</param>
	    private static void IncludeExcludeAll(GorgonFileSystemDirectory directory, bool exclude)
	    {
            IEnumerable<GorgonFileSystemFileEntry> files =
                ScratchArea.ScratchFiles.FindFiles(directory.FullPath, "*", true)
                .Where(item => !ScratchArea.IsBlocked(item));

            foreach (GorgonFileSystemFileEntry file in files)
            {
                if (!exclude)
                {
                    ContentManagement.IncludeItem(file);
                }
                else
                {
                    EditorMetaDataFile.Files[file.FullPath] = null;
                }
            }

            FileManagement.FileChanged = true;

            EditorMetaDataFile.Save();
	    }

        /// <summary>
        /// Handles the Click event of the popupItemIncludeAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void popupItemIncludeAll_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var dirNode = treeFiles.SelectedNode as TreeNodeDirectory;

                if (dirNode == null)
                {
                    return;
                }

                IncludeExcludeAll(dirNode.Directory, false);

                if (dirNode.IsExpanded)
                {
                    dirNode.Collapse();
                }

                if ((dirNode.Nodes.Count == 0)
                    && (dirNode.Directory.Files.Count(item => !ScratchArea.IsBlocked(item)) > 0))
                {
                    dirNode.Nodes.Add(new TreeNode("DummyNode"));
                }

                dirNode.Expand();
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
        /// Handles the Click event of the popupItemExcludeAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void popupItemExcludeAll_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var dirNode = treeFiles.SelectedNode as TreeNodeDirectory;

                if (dirNode == null)
                {
                    return;
                }

                IncludeExcludeAll(dirNode.Directory, true);

                if (dirNode.IsExpanded)
                {
                    dirNode.Collapse();
                }

                dirNode.Expand();
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
		/// Handles the CheckedChanged event of the buttonShowAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonShowAll_CheckedChanged(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// Refresh the tree.
				InitializeTree();
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
		/// Handles the Click event of the popupItemInclude control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void popupItemInclude_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				var fileNode = (TreeNodeFile)treeFiles.SelectedNode;
				GorgonFileSystemFileEntry file = fileNode.File;

				ContentManagement.IncludeItem(file);

				FileManagement.FileChanged = true;

				EditorMetaDataFile.Save();

				fileNode.UpdateFile(file);

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
		/// Handles the Click event of the popupItemExclude control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void popupItemExclude_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				var fileNode = (TreeNodeFile)treeFiles.SelectedNode;

				if (fileNode.EditorFile == null)
				{
					return;
				}

                EditorMetaDataFile.Files[fileNode.EditorFile.FilePath] = null;
                
                FileManagement.FileChanged = true;

                EditorMetaDataFile.Save();

				// Remove the node from the tree if we're not showing all files.
				if (!buttonShowAll.Checked)
				{
					treeFiles.SelectedNode = null;
					fileNode.Remove();
				}
				else
				{
					fileNode.UpdateFile(fileNode.File);
					treeFiles.Refresh();
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
        /// Handles the Click event of the itemPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemPreferences_Click(object sender, EventArgs e)
        {
            FormPreferences prefs = null;

            try
            {
	            prefs = new FormPreferences();
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

				ValidateControls();
            }        
        }

		/// <summary>
		/// Function to assign extensions to the file open/save dialogs.
		/// </summary>
		/// <param name="dialog">Dialog to update.</param>
		/// <param name="extensions">Extensions to use as a filter.</param>
		/// <param name="includeAllFiles"><b>true</b> to include all files in the filter, <b>false</b> to exclude.</param>
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

				filter.AppendFormat("{0}|*.*", Resources.GOREDIT_TEXT_ALL_FILES);
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
                AssignFilter(dialogImport, ContentManagement.GetContentExtensions(), true, Resources.GOREDIT_TEXT_ALL_CONTENT);

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
        /// <param name="importExport"><b>true</b> if an import, <b>false</b> if an export.</param>
        /// <param name="filesCopied">The number of files copied.</param>
        /// <param name="totalFiles">The total number of files to copy.</param>
	    private void FileImportExportCompleted(bool cancelled, bool importExport, int filesCopied, int totalFiles)
	    {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => FileImportExportCompleted(cancelled, importExport, filesCopied, totalFiles)));
	            return;
            }
            
            int skipped = totalFiles - filesCopied;

	        if (!cancelled)
	        {
				EditorMetaDataFile.Save();
		        return;
	        }

	        GorgonDialogs.WarningBox(this,
	                                 string.Format(
	                                               importExport
		                                               ? Resources.GOREDIT_DLG_IMPORT_CANCELLED
		                                               : Resources.GOREDIT_DLG_EXPORT_CANCELLED,
	                                               filesCopied,
	                                               totalFiles,
	                                               skipped));
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
		/// <returns><b>true</b> if the file can be imported or <b>false</b> if not.</returns>
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
					                                             string.Format(Resources.GOREDIT_DLG_IMPORT_FILE_OPEN_FOR_EDIT,
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
		        () =>
		        {
		            Cursor prevCursor = Cursor.Current;

		            result = GorgonDialogs.ConfirmBox(null,
		                                              string.Format(Resources.GOREDIT_DLG_OVERWRITE_FILE,
		                                                            Resources.GOREDIT_TEXT_FILE.ToLower(
		                                                                                                CultureInfo
		                                                                                                    .CurrentUICulture),
		                                                            filePath),
		                                              null,
		                                              totalFileCount > 1,
		                                              totalFileCount > 1);
		            Cursor.Current = prevCursor;
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
        private ConfirmationResult CopyConfirmFileOverwrite(string filePath, int totalFileCount)
        {
            var result = ConfirmationResult.None;
            Action invokeAction = () =>
                                  {
                                      Cursor current = Cursor.Current;

                                      result = GorgonDialogs.ConfirmBox(null,
                                                                        string.Format(Resources.GOREDIT_DLG_OVERWRITE_FILE,
                                                                                      Resources.GOREDIT_TEXT_FILE.ToLower(CultureInfo.CurrentUICulture),
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
                                                                        string.Format(Resources.GOREDIT_DLG_OVERWRITE_DIRECTORY,
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

                dialogExport.Description = Resources.GOREDIT_TEXT_SELECT_DIR_EXPORT;
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
					var fileNode = (TreeNodeFile)treeFiles.SelectedNode;
					GorgonFileSystemFileEntry file = fileNode.File;

					// If we have dependencies, then ask to export those as well.
					if ((fileNode.EditorFile != null) && (fileNode.EditorFile.DependsOn.Count > 0))
					{
						ConfirmationResult result = GorgonDialogs.ConfirmBox(this,
						                                                     string.Format(Resources.GOREDIT_DLG_EXPORT_DEPENDENCIES,
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
							foreach (Dependency dependency in fileNode.EditorFile.DependsOn)
							{
								if (!filePaths.Contains(dependency.EditorFile.FilePath.ToLowerInvariant()))
								{
									filePaths.Add(dependency.EditorFile.FilePath.ToLowerInvariant());
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
        /// Function to determine if a directory has excluded files.
        /// </summary>
        /// <param name="directory">Directory to evaluate.</param>
        /// <returns><b>true</b> if excluded files are present, <b>false</b> if not.</returns>
	    private static bool DirectoryHasExcludedFiles(GorgonFileSystemDirectory directory)
        {
            IEnumerable<GorgonFileSystemFileEntry> files = ScratchArea.ScratchFiles.FindFiles(directory.FullPath,
                                                                                              "*",
                                                                                              true);

            return files.Any(item => !EditorMetaDataFile.Files.Contains(item.FullPath));
        }

        /// <summary>
        /// Function to determine if a directory has included files.
        /// </summary>
        /// <param name="directory">Directory to evaluate.</param>
        /// <returns><b>true</b> if excluded files are present, <b>false</b> if not.</returns>
        private static bool DirectoryHasIncludedFiles(GorgonFileSystemDirectory directory)
        {
            IEnumerable<GorgonFileSystemFileEntry> files = ScratchArea.ScratchFiles.FindFiles(directory.FullPath,
                                                                                              "*",
                                                                                              true);

            return files.Any(item => EditorMetaDataFile.Files.Contains(item.FullPath));
        }


        /// <summary>
        /// Function to validate the controls on the display.
        /// </summary>
        private void ValidateControls()
        {
	        bool dependencies = false;
            TreeNodeEditor selectedNode = treeFiles.SelectedNode;

            Text = string.Format("{0} - {1}", FileManagement.Filename, Resources.GOREDIT_TEXT_GORGON_EDITOR);

			Debug.Assert(_rootNode != null, "Root node is NULL!");

			_rootNode.Redraw();
			
            itemOpen.Enabled = PlugIns.ReaderPlugIns.Count > 0;
            itemSaveAs.Enabled = (PlugIns.WriterPlugIns.Count > 0);
            itemSave.Enabled = !string.IsNullOrWhiteSpace(FileManagement.FilePath)
                                && (FileManagement.GetWriterPlugIn(FileManagement.FilePath) != null)
                                && ((FileManagement.FileChanged) 
									|| (HasContentChanged))
								&& itemSaveAs.Enabled;

            // Check to see if the current content can export.
	        itemExport.Enabled = (_rootNode != null) && (_rootNode.Nodes.Count > 1) && (treeFiles.SelectedNode != null);
            popupItemAddContent.Visible = true;
            itemAddContent.Enabled = false;
            popupItemAddContent.Enabled = false;
            dropNewContent.Enabled = false;
            itemDelete.Enabled = popupItemDelete.Enabled = false;
            itemDelete.Text = popupItemDelete.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_DELETE);
            itemDelete.Visible = popupItemDelete.Visible = true;
            popupItemEdit.Visible = false;
            toolStripSeparator5.Visible = true;
            popupItemRename.Visible = true;
            popupItemRename.Enabled = false;
            popupItemRename.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_RENAME);
            popupItemCreateFolder.Visible = false;
	        itemCreateFolder.Enabled = false;
			buttonEditContent.Enabled = false;
			buttonDeleteContent.Enabled = false;
	        popupItemExclude.Visible = false;
	        popupItemInclude.Visible = false;
			popupItemCut.Enabled = popupItemCopy.Enabled = popupItemPaste.Enabled = itemCopy.Enabled = itemCut.Enabled = itemPaste.Enabled = false;
	        toolStripSeparator10.Visible = false;
            popupItemIncludeAll.Visible = false;
            popupItemExcludeAll.Visible = false;

            menuRecentFiles.Enabled = Program.Settings.RecentFiles.Count > 0;

            // No node is the same as selecting the root.
            if (selectedNode == null)
            {
				selectedNode = treeFiles.SelectedNode = _rootNode;
            }

			if ((selectedNode.NodeType & NodeType.Directory) == NodeType.Directory)
			{
			    GorgonFileSystemDirectory directory = ((TreeNodeDirectory)selectedNode).Directory;

				// Don't check the root node.
		        if (treeFiles.SelectedNode != _rootNode)
		        {
			        dependencies = EditorMetaDataFile.HasFileLinks(directory);
		        }

		        toolStripSeparator4.Visible = true;
	            toolStripSeparator10.Visible = true;

			    popupItemExcludeAll.Text = string.Format(Resources.GOREDIT_TEXT_EXCLUDE_ALL, directory.Name);
                popupItemIncludeAll.Text = string.Format(Resources.GOREDIT_TEXT_INCLUDE_ALL, directory.Name);

			    popupItemIncludeAll.Visible = DirectoryHasExcludedFiles(directory);
			    popupItemExcludeAll.Visible = DirectoryHasIncludedFiles(directory);
                popupItemAddContent.Visible = itemAddContent.Enabled = itemAddContent.DropDownItems.Count > 0;
		        popupItemAddContent.Enabled = itemAddContent.Enabled;
		        dropNewContent.Enabled = dropNewContent.DropDownItems.Count > 0;
		        buttonDeleteContent.Enabled = !dependencies;
		        popupItemCreateFolder.Enabled = itemCreateFolder.Enabled = true;
		        popupItemCreateFolder.Visible = true;
		        itemDelete.Enabled = popupItemDelete.Enabled = (!dependencies) && (tabDocumentManager.SelectedTab == pageItems);
		        itemDelete.Text = popupItemDelete.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_DELETE_FOLDER);
		        popupItemPaste.Enabled = itemPaste.Enabled = _clipboardData;

		        if (treeFiles.SelectedNode != _rootNode)
		        {
					popupItemCut.Enabled = popupItemCopy.Enabled = itemCopy.Enabled = itemCut.Enabled = true;
				    popupItemRename.Enabled = !dependencies;
				    popupItemRename.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_RENAME_FOLDER);
		        }
		        else
		        {
			        if (_rootNode.Nodes.Count == 0)
			        {
				        buttonDeleteContent.Enabled = false;
				        popupItemDelete.Visible = false;
				        itemDelete.Enabled = false;
				        itemDelete.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_DELETE);
			        }
			        else
			        {
				        itemDelete.Text = popupItemDelete.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_DELETE_ALL_FILES);
			        }

			        toolStripSeparator5.Visible = false;
			        popupItemRename.Visible = false;
		        }

		        return;
	        }

	        GorgonFileSystemFileEntry file = ((TreeNodeFile)selectedNode).File;
	        EditorFile editorFile = ((TreeNodeFile)selectedNode).EditorFile;

			// If this is a dependency node, then we have very limited options.
	        if ((selectedNode.NodeType & NodeType.Dependency) == NodeType.Dependency)
	        {
		        toolStripSeparator4.Visible =
			        buttonEditContent.Enabled =
			        popupItemEdit.Visible = popupItemEdit.Enabled = ContentManagement.CanOpenContent(editorFile);
				popupItemAddContent.Visible = false;
		        return;
	        }

	        dependencies = EditorMetaDataFile.HasFileLinks(editorFile);

			buttonEditContent.Enabled =
				popupItemEdit.Visible = popupItemEdit.Enabled = ContentManagement.CanOpenContent(editorFile) && !dependencies;
			popupItemAddContent.Visible = false;
			popupItemPaste.Enabled = itemPaste.Enabled = _clipboardData;
	        popupItemCopy.Enabled = itemCopy.Enabled = true;
			
			popupItemExclude.Text = string.Format(Resources.GOREDIT_TEXT_EXCLUDE_FILE, file.Name);
			popupItemInclude.Text = string.Format(Resources.GOREDIT_TEXT_INCLUDE_FILE, file.Name);

			popupItemInclude.Visible = editorFile == null;
			popupItemExclude.Visible = editorFile != null && !dependencies;
			toolStripSeparator10.Visible = buttonEditContent.Enabled;
			toolStripSeparator4.Visible = !dependencies || buttonEditContent.Enabled;

	        if (dependencies)
	        {
				// Disable exclusion of items that are linked to other objects.
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

			if ((!HasContentChanged)
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
			switch (result)
			{
				case ConfirmationResult.Yes:
					ContentManagement.Save();
					return result;
				default:
					return result;
			}
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
				EditorFile editorFile;

				EditorMetaDataFile.Files.TryGetValue(fileNode.File.FullPath, out editorFile);

				if (editorFile == null)
				{
					if (GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_DLG_FILE_NEEDS_IMPORT, fileNode.File.Name)) == ConfirmationResult.No)
					{
						return;
					}

					Cursor.Current = Cursors.WaitCursor;

					editorFile = ContentManagement.IncludeItem(fileNode.File);

					EditorMetaDataFile.Save();
				}

				ContentPlugIn plugIn = ContentManagement.GetContentPlugInForFile(editorFile);

				// Ensure we can actually open this type.
				if (plugIn == null)
				{
					GorgonDialogs.ErrorBox(this, string.Format(Resources.GOREDIT_DLG_NO_CONTENT_PLUG_IN_FOR_FILE, fileNode.File.Name, fileNode.File.Extension));
					return;
				}

				ConfirmationResult result = ConfirmSaveContent();

				if (result == ConfirmationResult.Cancel)
				{
					return;
				}

                ContentManagement.Load(editorFile, fileNode.File, plugIn);
                treeFiles.Refresh();

				if (ContentManagement.Current.HasProperties)
				{
					propertyItem.Refresh();
				}
			}
			catch (Exception ex)
			{
				ContentManagement.LoadDefaultContentPane();
                GorgonDialogs.ErrorBox(this, string.Format(Resources.GOREDIT_DLG_OPEN_ERROR, fileNode.File.Name, ex.Message), null, ex);
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
		/// Function called when the name property of the content has been changed.
		/// </summary>
		/// <param name="newName">The new name for the content.</param>
		/// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
		private void ContentNamePropertyChanged(string newName)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				TreeNodeFile node = treeFiles.GetCurrentContentNode(CurrentOpenFile);

				if (node == null)
				{
					return;
				}

				if (string.Equals(node.File.Name, newName, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				RenameFileNode(node, newName);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
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

			if (currentNode == null)
			{
				return;
			}

			currentNode.UpdateFile(CurrentOpenFile);
			GetDependencyNodes(currentNode);
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
					GorgonDialogs.ErrorBox(this, Resources.GOREDIT_DLG_PASTE_MUST_BE_DIRECTORY);
					return;
				}

				// If we're moving, there are some restrictions.
				if ((currentNode == node) && (cutCopyObject.IsCut))
				{
					GorgonDialogs.ErrorBox(this, Resources.GOREDIT_DLG_FILE_SOURCE_SAME_AS_DEST);
					return;
				}

				if ((cutCopyObject.IsCut) && (currentNode.IsAncestorOf(node)))
				{
					GorgonDialogs.ErrorBox(this, Resources.GOREDIT_DLG_FILE_CANNOT_MOVE_TO_CHILD);
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
				_clipboardData = true;
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
		/// <returns><b>true</b> if canceled, <b>false</b> if not.</returns>
		private bool ConfirmSave()
		{
			// Ensure both the content and the file change flags are checked.
			if (((!FileManagement.FileChanged) &&
			     (!HasContentChanged)) 
				 || (PlugIns.WriterPlugIns.Count == 0))
			{
				return false;
			}

			var result = GorgonDialogs.ConfirmBox(this,
			                                      string.Format(Resources.GOREDIT_DLG_FILE_HAS_CHANGES,
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
				ContentManagement.ContentPropertyStateChanged = null;
				ContentManagement.ContentRenamed = null;
				ContentManagement.ContentPanelUnloadAction = null;
				ContentManagement.ContentEnumerateProperties = null;
				ContentManagement.ContentInitializedAction = null;
				ContentManagement.OnGetDependency = null;
				ContentManagement.DependencyNotFound = null;
				ContentManagement.ContentSaved = null;
				ContentManagement.ContentPropertyChanged = null;

                // Unhook from file management functionality.
			    ScratchArea.ImportExportFileConflictFunction = null;
                ScratchArea.ImportExportFileCompleteAction = null;
				ScratchArea.FileImported = null;
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

				Program.Settings.DefaultImageEditor = PlugIns.DefaultImageEditorPlugIn;

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
					|| (fileNode.EditorFile == null)
					|| (fileNode.EditorFile.DependsOn.Count == 0))
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

			if (fileNode.EditorFile == null)
			{
				return;
			}

			foreach (Dependency dependencyFile in fileNode.EditorFile.DependsOn)
			{
				GorgonFileSystemFileEntry fileEntry = ScratchArea.ScratchFiles.GetFile(dependencyFile.EditorFile.FilePath);
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
		private void GetFolders(TreeNodeDirectory rootNode)
		{
			// Get the sub directories.
			rootNode.Nodes.Clear();

			foreach (var subDirectory in rootNode.Directory.Directories.OrderBy(item => item.Name))
			{
				var subNode = new TreeNodeDirectory(subDirectory)
				              {
								  ForeColor = Color.White,
								  Text = subDirectory.Name
				              };
				

				if ((subDirectory.Directories.Count > 0) 
                    || (CanShowDirectoryFiles(subDirectory)))
				{
					subNode.Nodes.Add(new TreeNode("DummyNode"));
				}

				rootNode.Nodes.Add(subNode);
			}

			// Add file nodes.
			foreach (var fileEntry in from file in rootNode.Directory.Files.OrderBy(item => item.Name)
			                          where ((buttonShowAll.Checked) && (!ScratchArea.IsBlocked(file))) || (EditorMetaDataFile.Files.Contains(file.FullPath))
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
        /// <returns><b>true</b> if some files can be shown, <b>false</b> if not.</returns>
        private bool CanShowDirectoryFiles(GorgonFileSystemDirectory directory)
        {
	        return directory.Files.Count != 0 &&
	               (((buttonShowAll.Checked) && (!directory.Files.All(ScratchArea.IsBlocked))) ||
	                directory.Files.Any(item => EditorMetaDataFile.Files.Contains(item.FullPath)));
        }

		/// <summary>
		/// Function to initialize the files tree.
		/// </summary>
		private void InitializeTree()
		{
			_rootNode = new RootNodeDirectory
			            {
							ForeColor = Color.White,
							Text = @"/"
			            };

			// If we have files or sub directories, dump them in here.
			if ((ScratchArea.ScratchFiles.RootDirectory.Directories.Count > 0)
				|| (CanShowDirectoryFiles(ScratchArea.ScratchFiles.RootDirectory)))
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
        /// <param name="expandDirectory"><b>true</b> to expand the directory for the selected node, <b>false</b> to leave alone.</param>
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

                // Create the file in the scratch area.
                newFile = ScratchArea.CreateFile(content.Name, content.ContentType, directoryNode.Directory);

                // The file did not get created, then display an error to that effect.
                if (newFile == null)
			    {
					throw new IOException(string.Format(Resources.GOREDIT_ERR_CONTENT_CANNOT_CREATE_FILE, content.Name));
			    }

				content.EditorFile = new EditorFile(newFile.FullPath)
				                     {
										 PlugInType = plugIn.GetType().FullName
				                     };

				// Load the content.
				// We're doing this first because in order to save the file, the content needs be to active in the editor.
				ContentManagement.LoadContentPane(content);

				// Persist the content to the file.
				CurrentOpenFile = newFile;
				ContentManagement.Save(false);

				// Store the file link.
				EditorMetaDataFile.Files[content.EditorFile.FilePath] = content.EditorFile;

				// Get the attribute information for the file.
				using (Stream stream = newFile.OpenStream(false))
				{
					plugIn.GetEditorFileAttributes(stream, content.EditorFile.Attributes);
				}

				// Create the file node in our tree.
                newNode = new TreeNodeFile();
				newNode.UpdateFile(newFile);

                // Add to tree and select.
                directoryNode.Nodes.Add(newNode);
                treeFiles.SelectedNode = newNode;

				content.OnContentReady();

				FileManagement.FileChanged = true;
			}
			catch (Exception ex)
			{
                // Load the default pane.
                ContentManagement.LoadDefaultContentPane();
				
				if (content != null)
				{
					if ((content.EditorFile != null)
						&& (EditorMetaDataFile.Files.Contains(content.EditorFile)))
					{
						// Remove this entry.
						EditorMetaDataFile.Files[content.EditorFile.FilePath] = null;
						EditorMetaDataFile.Save();
					}

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
						EditorLogging.CatchException(innerEx);
					}
#else
					catch
					{
						// Intentionally left blank for release mode.
					}
#endif
					finally
					{
						GorgonException.Logs = Gorgon.Log;
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
				// Persist the meta data.
				EditorMetaDataFile.Save();

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
				if ((CurrentOpenFile != null) && (HasContentChanged))
				{
					ContentManagement.Save();
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
					_clipboardData = false;
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

                    if (GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_DLG_DELETE_DIRECTORY, sourcePath)) == ConfirmationResult.No)
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

					// Remove links to files that fall under this path.
					var files = EditorMetaDataFile.Files.Where(item => item.FilePath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase)).ToArray();

					foreach (var file in files)
					{
						EditorMetaDataFile.Files[file.FilePath] = null;
					}

					EditorMetaDataFile.Save();


                    PruneTree(directoryNode);
				}

                if ((selectedNode.NodeType & NodeType.File) == NodeType.File)
				{
					var fileNode = (TreeNodeFile)treeFiles.SelectedNode;
					string sourceFilePath = fileNode.File.FullPath;

					if (GorgonDialogs.ConfirmBox(this, string.Format(Resources.GOREDIT_DLG_DELETE_FILE, sourceFilePath)) == ConfirmationResult.No)
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

					if (fileNode.EditorFile != null)
					{
						EditorMetaDataFile.Files[fileNode.EditorFile.FilePath] = null;
					}

					EditorMetaDataFile.Save();

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
				if ((CurrentOpenFile != null) && (HasContentChanged))
				{
					ContentManagement.Save();
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
				
				if ((node == null) || (node == _rootNode) || (node.NodeType == NodeType.Dependency))
				{
					return;
				}

				var file = node as TreeNodeFile;
				if (file != null)
				{
					treeFiles.DoDragDrop(new DragFile(file, e.Button), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
					return;
				}

				var directory = node as TreeNodeDirectory;
				if (directory != null)
				{
					treeFiles.DoDragDrop(new DragDirectory(directory, e.Button), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);	
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
				var treeNode = new TreeNodeDirectory(newDirectory)
				               {
					               ForeColor = Color.White,
					               Text = newDirectory.Name
				               };

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
				string defaultName = Resources.GOREDIT_TEXT_UNTITLED;
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
                    defaultName = string.Format("{0} ({1})", Resources.GOREDIT_TEXT_UNTITLED, ++nameIndex);
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
	        string sourcePath = sourceDirNode.Directory.FullPath;
            string openContentPath = string.Empty;

            if ((CurrentOpenFile != null) && (CurrentOpenFile.FullPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase)))
            {
                openContentPath = (CurrentOpenFile.Directory.FullPath.Replace(sourcePath,
                                                                              sourceDirNode.Directory.Parent.FullPath +
                                                                              newName)).FormatDirectory('/')
                                  + CurrentOpenFile.Name;
            }

			// Update meta data links.
			IEnumerable<GorgonFileSystemFileEntry> files = ScratchArea.ScratchFiles.FindFiles(sourcePath, "*", true);

            GorgonFileSystemDirectory newDirectory = ScratchArea.Rename(sourceDirNode.Directory, newName);

            if (newDirectory == null)
            {
                return;
            }

			var newFilePath = new StringBuilder();

	        foreach (GorgonFileSystemFileEntry file in files)
	        {
		        newFilePath.Length = 0;
		        newFilePath.Append(newDirectory.FullPath);
		        newFilePath.Append(file.Name);
		        UpdateEditorFile(file.FullPath, newFilePath.ToString(), false, false);
				//UpdateDependencyList(file.FullPath, newFilePath.ToString(), false, false);
	        }

			EditorMetaDataFile.Save();

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
			//UpdateDependencyList(sourcePath, newFile.FullPath, false, false);

			// Update the editor meta data link.
			UpdateEditorFile(sourcePath, newFile.FullPath, false, true);

			sourceFileNode.UpdateFile(newFile);

			FileManagement.FileChanged = true;

			if (!openContentRenamed)
			{
				return;
			}

			// Update the current name in the current content.
			ContentManagement.Current.Name = newFile.BaseFileName;
			CurrentOpenFile = newFile;
        }

		/// <summary>
		/// Function to copy files from windows explorer into our file system.
		/// </summary>
		/// <param name="destDir">Destination directory node.</param>
		/// <param name="files">Paths to the files/directories to copy.</param>
		private void AddFilesFromExplorer(TreeNodeDirectory destDir, IEnumerable<string> files)
		{
		    Cursor prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

		    try
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
		    catch (Exception ex)
		    {
		        GorgonDialogs.ErrorBox(this, ex);
		    }
		    finally
		    {
		        Cursor.Current = prevCursor;
		    }
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
				    var dropData = ((IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop))
				        .Where(item =>
				               !item.StartsWith(ScratchArea.ScratchFiles.WriteLocation,
				                                StringComparison.OrdinalIgnoreCase))
				        .ToArray();

					if (dropData.Length > 0)
					{
                        // Schedule the copy operation to occur after the drag/drop is complete.
					    BeginInvoke(new Action(() => AddFilesFromExplorer(destDir, dropData)));
					}					
					return;
				}

				// Handle nodes.
				TreeNodeEditor sourceNode = null;
				bool isCopy = false;

				if (e.Data.GetDataPresent(typeof(DragFile)))
				{
					var data = (DragFile)e.Data.GetData(typeof(DragFile));

					sourceNode = data.FileNode;
					isCopy = data.MouseButton != MouseButtons.Left;
				}

				if (e.Data.GetDataPresent(typeof(DragDirectory)))
				{
					var data = (DragDirectory)e.Data.GetData(typeof(DragDirectory));

					sourceNode = data.DirectoryNode;
					isCopy = data.MouseButton != MouseButtons.Left;
				}

				if (sourceNode == null)
				{
					return;
				}

			    BeginInvoke(new Action(() => MoveCopyNode(sourceNode, destDir, isCopy)));
			}
			catch (Exception ex)
			{
                BeginInvoke(new Action(() => GorgonDialogs.ErrorBox(this, ex)));
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
        /// <param name="isCopy"><b>true</b> to copy the directory, <b>false</b> to move.</param>
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

				// Update meta data links.
				IEnumerable<GorgonFileSystemFileEntry> files = ScratchArea.ScratchFiles.FindFiles(sourcePath, "*", true);

	            GorgonFileSystemDirectory newDirectory = isCopy
		                                                     ? ScratchArea.Copy(sourceNode.Directory,
		                                                                        destinationNode.Directory.FullPath)
		                                                     : ScratchArea.Move(sourceNode.Directory,
		                                                                        destinationNode.Directory.FullPath);

                if (newDirectory == null)
                {
                    return null;
                }

				var newFilePath = new StringBuilder();

				foreach (GorgonFileSystemFileEntry file in files)
				{
					newFilePath.Length = 0;
					newFilePath.Append(newDirectory.FullPath);
					newFilePath.Append(file.Name);
					UpdateEditorFile(file.FullPath, newFilePath.ToString(), isCopy, false);
					//UpdateDependencyList(file.FullPath, newFilePath.ToString(), isCopy, false);
				}

				EditorMetaDataFile.Save();

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
		/// Function to update the file meta data list when a file is renamed, moved or copied.
		/// </summary>
		/// <param name="sourceFile">Source file that contains the dependencies.</param>
		/// <param name="destFile">Destination file to contain the dependencies.</param>
		/// <param name="isCopy"><b>true</b> if the update is from a copy operation, <b>false</b> if it's from a cut operation.</param>
		/// <param name="saveMetaData"><b>true</b> to save the meta data file, <b>false</b> to leave alone.</param>
		private static void UpdateEditorFile(string sourceFile, string destFile, bool isCopy, bool saveMetaData)
		{
			EditorFile file;

			if (!EditorMetaDataFile.Files.TryGetValue(sourceFile, out file))
			{
				return;
			}

			if (!isCopy)
			{
				// If we have the content open for editing, then rename the copy of the file.
				if ((ContentManagement.Current != null) && (ContentManagement.Current.EditorFile != null)
				    && (string.Equals(file.FilePath, ContentManagement.Current.EditorFile.FilePath, StringComparison.OrdinalIgnoreCase)))
				{
					ContentManagement.Current.EditorFile.Rename(destFile);
				}

				file.Rename(destFile);
				EditorMetaDataFile.Files[sourceFile] = file;

				if (saveMetaData)
				{
					EditorMetaDataFile.Save();
				}
				return;
			}

			var copyFile = new EditorFile(destFile)
			               {
							   PlugInType = file.PlugInType
			               };

			copyFile.DependsOn.CopyFrom(file.DependsOn);

			foreach (var attrib in file.Attributes)
			{
				copyFile.Attributes.Add(attrib.Key, attrib.Value);
			}

			EditorMetaDataFile.Files[copyFile.FilePath] = copyFile;

			if (!saveMetaData)
			{
				return;
			}

			EditorMetaDataFile.Save();
		}
		/*
		/// <summary>
		/// Function to update the dependency list when a file is renamed or moved.
		/// </summary>
		/// <param name="sourceFile">Source file that contains the dependencies.</param>
		/// <param name="destFile">Destination file to contain the dependencies.</param>
		/// <param name="isCopy"><b>true</b> if the update is from a copy operation, <b>false</b> if it's from a cut operation.</param>
		/// <param name="saveMetaData"><b>true</b> to save the meta data file, <b>false</b> to leave alone.</param>
		private static void UpdateDependencyList(string sourceFile, string destFile, bool isCopy, bool saveMetaData)
		{
			if (!EditorMetaDataFile.Dependencies.ContainsKey(sourceFile))
			{
				return;
			}

			DependencyCollection dependencies = EditorMetaDataFile.Dependencies[sourceFile];

			if (!isCopy)
			{
				EditorMetaDataFile.Dependencies.Remove(sourceFile);
				EditorMetaDataFile.Dependencies[destFile] = dependencies;
				EditorMetaDataFile.Save();
				return;
			}

			EditorMetaDataFile.Dependencies[destFile] = new DependencyCollection();

			// Copy the dependency list.
			foreach (Dependency dependency in dependencies)
			{
				var newDependency = new Dependency(dependency.Path, dependency.Type);

				foreach (DependencyProperty property in dependency.Properties)
				{
					newDependency.Properties[property.Name] = property;
				}

				EditorMetaDataFile.Dependencies[destFile][newDependency.Path, newDependency.Type] = newDependency;
			}

			if (!saveMetaData)
			{
				return;
			}

			EditorMetaDataFile.Save();
		}
		*/
		/// <summary>
		/// Function to move or copy a file node.
		/// </summary>
		/// <param name="sourceNode">Source node file to move/copy.</param>
		/// <param name="destinationNode">The destination directory for the copied/moved node.</param>
		/// <param name="isCopy"><b>true</b> to copy the file, <b>false</b> to move.</param>
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

				//UpdateDependencyList(sourcePath, newFile.FullPath, isCopy, false);

				UpdateEditorFile(sourcePath, newFile.FullPath, isCopy, true);

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
		/// <param name="isCopy"><b>true</b> if the function should copy instead of move.</param>
		/// <param name="cutCopyObject">The reference to the object being cut in a cut operation.</param>
		private void MoveCopyNode(TreeNodeEditor sourceNode, TreeNodeDirectory destinationNode, bool isCopy, CutCopyObject? cutCopyObject = null)
		{
			TreeNodeEditor newNode = null;
			string sourcePath = sourceNode.FullPath;
            
			if (destinationNode == null)
			{
				return;
			}

		    Cursor prevCursor = Cursor.Current;
		    Cursor.Current = Cursors.WaitCursor;

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
					_clipboardData = false;
				}

			    Cursor.Current = prevCursor;
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

				// Don't drag over a file.
				if (destFile != null)
				{
					return;
				}

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

				var currentMouseButton = MouseButtons.None;

				if (e.Data.GetDataPresent(typeof(DragFile)))
				{
					var dragData = (DragFile)e.Data.GetData(typeof(DragFile));

					// Don't allow a drag-drop on the immediate parent of this node.
					if (overNode == dragData.FileNode.Parent)
					{
						return;
					}

					currentMouseButton = dragData.MouseButton;
				}

				if (e.Data.GetDataPresent(typeof(DragDirectory)))
				{
					var dragData = (DragDirectory)e.Data.GetData(typeof(DragDirectory));

					// Don't allow a drag-drop on the immediate parent of this node or on to itself.
					if ((overNode == dragData.DirectoryNode.Parent)
						|| (overNode == dragData.DirectoryNode))
					{
						return;
					}

					currentMouseButton = dragData.MouseButton;
				}

				if (currentMouseButton == MouseButtons.None)
				{
					return;
				}

				treeFiles.SelectedNode = overNode;
				e.Effect = currentMouseButton == MouseButtons.Left ? DragDropEffects.Move : DragDropEffects.Copy;
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

                AssignFilter(dialogOpenFile, FileManagement.GetReaderExtensions(), true, Resources.GOREDIT_TEXT_GORGON_FILES);

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
	                EditorLogging.CatchException(ex);
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
			Text = Resources.GOREDIT_TEXT_GORGON_EDITOR;

			pageItems.Text = Resources.GOREDIT_TEXT_CONTENT_FILES;
			pageProperties.Text = Resources.GOREDIT_TEXT_PROPERTIES;

			menuFile.Text = Resources.GOREDIT_ACC_TEXT_FILE;
			popupItemEdit.Text = menuEdit.Text = Resources.GOREDIT_ACC_TEXT_EDIT;
			menuRecentFiles.Text = Resources.GOREDIT_ACC_TEXT_RECENT_FILES;
			
			itemNew.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_NEW);
			itemOpen.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_OPEN);
			popupItemAddContent.Text = itemAddContent.Text = Resources.GOREDIT_ACC_TEXT_ADD_CONTENT;
			popupItemCreateFolder.Text = itemCreateFolder.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_CREATE_FOLDER);
			itemSave.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_SAVE);
			itemSaveAs.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_SAVE_AS);
			itemImport.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_IMPORT);
			itemExport.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_EXPORT);
			itemExit.Text = Resources.GOREDIT_ACC_TEXT_EXIT;

			popupItemCut.Text = itemCut.Text = Resources.GOREDIT_ACC_TEXT_CUT;
			popupItemCopy.Text = itemCopy.Text = Resources.GOREDIT_ACC_TEXT_COPY;
			popupItemPaste.Text = itemPaste.Text = Resources.GOREDIT_ACC_TEXT_PASTE;
			popupItemDelete.Text = itemDelete.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_DELETE);
			popupItemInclude.Text = Resources.GOREDIT_TEXT_INCLUDE_FILE;
			popupItemExclude.Text = Resources.GOREDIT_TEXT_EXCLUDE_FILE;
			itemPreferences.Text = Resources.GOREDIT_ACC_TEXT_PREFERENCES;
			
			popupItemRename.Text = string.Format("{0}...", Resources.GOREDIT_ACC_TEXT_RENAME);

			dropNewContent.Text = dropNewContent.ToolTipText = Resources.GOREDIT_TEXT_NEW_CONTENT;
			buttonEditContent.Text = buttonEditContent.ToolTipText = Resources.GOREDIT_TEXT_EDIT_CONTENT;
			buttonDeleteContent.Text = buttonDeleteContent.ToolTipText = Resources.GOREDIT_TEXT_DELETE_CONTENT;

			buttonShowAll.Text = Resources.GOREDIT_TEXT_SHOW_ALL;

			itemResetValue.Text = Resources.GOREDIT_ACC_TEXT_RESET_VALUE;
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
		/// Initializes a new instance of the <see cref="FormMain"/> class.
		/// </summary>
		public FormMain()
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
				if (hasProperties)
				{
					pageProperties.Enabled = true;
					propertyItem.SelectedObject = ContentManagement.Current.TypeDescriptor;
					propertyItem.Refresh();
					tabDocumentManager.SelectedTab = pageProperties;
				}
				else
				{                
					tabDocumentManager.SelectedTab = pageItems;
					pageProperties.Enabled = false;
				}
			};
			ContentManagement.ContentInitializedAction = control =>
			                                             {
				                                             try
				                                             {
																 control.Dock = DockStyle.Fill;
																 control.Parent = splitPanel1;
																 control.ContentClosed += OnContentClose;
				                                             }
				                                             catch (Exception ex)
				                                             {
					                                             GorgonDialogs.ErrorBox(null, ex);
				                                             }
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
				                                                                string.Format(Resources.GOREDIT_DLG_CANNOT_FIND_DEPENDENCY,
				                                                                              sourceFile),
				                                                                null,
				                                                                displayText.ToString());
			                                       };

			ContentManagement.ContentRenamed = ContentNamePropertyChanged;
			ContentManagement.ContentPropertyStateChanged = () => propertyItem.Refresh();
			ContentManagement.ContentSaved = () => FileManagement.FileChanged = true;
			ContentManagement.ContentPropertyChanged = ValidateControls;

            // Assign file management linkage.
			ScratchArea.CanImportFunction = EvaluateFileImport;
			ScratchArea.ExceptionAction = FileCopyException;
            ScratchArea.ImportExportFileConflictFunction = ImportConfirmFileOverwrite;
            ScratchArea.ImportExportFileCompleteAction = FileImportExportCompleted;
			ScratchArea.FileImported = file => ContentManagement.IncludeItem(file);
		    ScratchArea.CreateFileConflictFunction = (fileName, fileType) => GorgonDialogs.ConfirmBox(null,string.Format(Resources.GOREDIT_DLG_OVERWRITE_FILE,
		                                                                                                        fileType,
		                                                                                                        fileName));
		    ScratchArea.CopyFileConflictFunction = CopyConfirmFileOverwrite;
            ScratchArea.CopyDirectoryConflictFunction = CopyConfirmDirectoryOverwrite;
		}
		#endregion
	}
}
