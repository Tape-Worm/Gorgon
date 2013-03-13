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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using KRBTabControl;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application object.
	/// </summary>
	public partial class formMain
		: ZuneForm
    {
        #region Variables.
        private RootNodeDirectory _rootNode = null;             // Our root node for the tree.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether we need the save as... dialog or not.
        /// </summary>
        private bool IsSaveAs
        {
            get
            {
                return string.IsNullOrWhiteSpace(Program.EditorFilePath);
            }
        }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to validate the controls on the display.
        /// </summary>
        private void ValidateControls()
        {
            Text = Program.EditorFile + " - Gorgon Editor";

            itemOpen.Enabled = Program.ScratchFiles.Providers.Count > 0;
            itemSaveAs.Enabled = (Program.WriterPlugIns.Count > 0) && (Program.ChangedItems.Count > 0);
            itemSave.Enabled = !string.IsNullOrWhiteSpace(Program.EditorFilePath) && (Program.ChangedItems.Count > 0);

            // Ensure we have plug-ins that can import.
			itemImport.Enabled = Program.ContentPlugIns.Any(item => item.Value.SupportsImport);
            // Check to see if the current content can export.
            itemExport.Enabled = ((Program.CurrentContent != null) && (Program.CurrentContent.CanExport));

            itemAdd.Enabled = false;
            popupItemAdd.Enabled = false;
            dropNewContent.Enabled = false;
            itemDelete.Visible = true;
            itemDelete.Enabled = false;
            itemDelete.Text = "Delete...";
            itemEdit.Visible = false;
            toolStripSeparator4.Visible = true;
            itemRenameFolder.Visible = true;
            itemRenameFolder.Enabled = false;
            itemRenameFolder.Text = "Rename...";
            itemCreateFolder.Visible = false;
			buttonEditContent.Enabled = false;
			buttonDeleteContent.Enabled = false;

            // No node is the same as selecting the root.
            if (treeFiles.SelectedNode == null)
            {
                itemAdd.Enabled = itemAdd.DropDownItems.Count > 0;
                popupItemAdd.Enabled = itemAdd.Enabled;
                dropNewContent.Enabled = dropNewContent.DropDownItems.Count > 0;
                itemDelete.Visible = false;
                toolStripSeparator4.Visible = false;
                itemRenameFolder.Visible = false;
            }
            else
            {
				var node = treeFiles.SelectedNode as EditorTreeNode;
                                
                if (node is TreeNodeDirectory)
                {
                    itemAdd.Enabled = itemAdd.DropDownItems.Count > 0;
                    popupItemAdd.Enabled = itemAdd.Enabled;
                    dropNewContent.Enabled = dropNewContent.DropDownItems.Count > 0;
                    itemOpen.Enabled = false;
                    itemCreateFolder.Enabled = true;
                    itemCreateFolder.Visible = true;
                    if (node != _rootNode)
                    {
                        itemDelete.Enabled = true;
                        itemDelete.Text = "Delete Folder...";
                        itemRenameFolder.Enabled = true;
                        itemRenameFolder.Text = "Rename Folder...";
                    }
                    else
                    {
                        itemDelete.Visible = false;
                        toolStripSeparator4.Visible = false;
                        itemRenameFolder.Visible = false;
                    }                    
                }

                if (node is TreeNodeFile)
                {
					GorgonFileSystemFileEntry file = ((TreeNodeFile)node).File;

					buttonDeleteContent.Enabled = true;
					buttonEditContent.Enabled = itemEdit.Visible = itemEdit.Enabled = Program.ContentPlugIns.Any(item => item.Value.FileExtensions.ContainsKey(file.Extension.ToLower()));
                    itemDelete.Enabled = true;
                    itemRenameFolder.Enabled = true;
                }
            }
        }

		/// <summary>
		/// Function to handle the content "open/edit" event.
		/// </summary>
		private void ContentOpen()
		{
			TreeNodeFile fileNode = null;
			
			// If we have no node selected, then assume it's the top of the chain.
			if ((treeFiles.SelectedNode != null) && (!(treeFiles.SelectedNode is TreeNodeDirectory)))
			{
				fileNode = treeFiles.SelectedNode as TreeNodeFile;
			}
			else
			{
				return;
			}

			// Otherwise, we need to open this file.
			if (fileNode.PlugIn == null)
			{
				throw new IOException("Cannot open '" + fileNode.File.FullPath + "'.  There are no content plug-ins loaded that can open '" + fileNode.File.Extension + "' files.");
			}

			var content = fileNode.PlugIn.CreateContentObject();

			// Open the content from the file system.
			content.OpenContent(fileNode.File);

			LoadContentPane(ref content);

			fileNode.Redraw();
		}

		/// <summary>
		/// Handles the NodeMouseDoubleClick event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeNodeMouseClickEventArgs"/> instance containing the event data.</param>
		private void treeFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				ContentOpen();
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
		/// Handles the KeyDown event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void treeFiles_KeyDown(object sender, KeyEventArgs e)
		{			
			switch(e.KeyCode)
			{
				case Keys.Enter:
					{
						Cursor.Current = Cursors.WaitCursor;

						try
						{
							ContentOpen();
						}
						catch (Exception ex)
						{
							GorgonDialogs.ErrorBox(this, ex);
						}
						finally
						{
							Cursor.Current = Cursors.Default;
						}

						break;
					}
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
				if ((propertyItem.SelectedObject == null) || (propertyItem.SelectedGridItem == null))
					return;

				propertyItem.SelectedGridItem.PropertyDescriptor.ResetValue(propertyItem.SelectedObject);
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
			if ((propertyItem.SelectedObject == null) || (propertyItem.SelectedGridItem == null))
			{
				itemResetValue.Enabled = false;
				return;
			}

			itemResetValue.Enabled = (propertyItem.SelectedGridItem.PropertyDescriptor.CanResetValue(propertyItem.SelectedObject));
		}

		/// <summary>
		/// Function to load a content object into the main interface.
		/// </summary>
		/// <param name="contentPreLoad">Content object to load in.</param>
		private void LoadContentPane(ref ContentObject contentPreLoad)
		{
			ContentObject content = contentPreLoad;
			Control control = null;

			// Turn off rendering.
			Gorgon.ApplicationIdleLoopMethod = null;

			if (content == null)
			{
				content = new DefaultContent();		
			}

			// If we have content loaded, ensure we get a chance to save it.
			if (Program.CurrentContent != null)
			{
				if (!Program.CurrentContent.Close())
				{
					if (contentPreLoad != null)
					{
						contentPreLoad.Dispose();
						contentPreLoad = null;
					}

					return;
				}

				// Destroy the previous content.
				Program.CurrentContent.Dispose();
				Program.CurrentContent = null;

				treeFiles.Refresh();
			}

			// Create the content resources and such.
			control = content.InitializeContent();

			// We couldn't get an interface component, fall back to the default display.
			if (control == null)
			{
				content.Dispose();
				content = new DefaultContent();
				control = content.InitializeContent();
				contentPreLoad = null;
			}

			control.Dock = DockStyle.Fill;

			// Add to the main interface.
			Program.CurrentContent = content;
			splitPanel1.Controls.Add(control);			

			// If the current content has a renderer, then activate it.
			// Otherwise, turn it off to conserve cycles.
			if (content.HasRenderer)
			{
				Gorgon.ApplicationIdleLoopMethod = Idle;
			}
		}

		/// <summary>
		/// Function to load content into the interface.
		/// </summary>
		/// <typeparam name="T">Type of content object to load.</typeparam>
		internal void LoadContentPane<T>()
			where T : ContentObject, new()
		{
			// Load the content.
			ContentObject result = new T();
			LoadContentPane(ref result);
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
				if (Program.ChangedItems.Count > 0)
				{
					ConfirmationResult result = ConfirmationResult.None;

					result = GorgonDialogs.ConfirmBox(this, "The editor file '" + Program.EditorFile + "' has unsaved changes.  Would you like to save these changes?", true, false);

					if (result == ConfirmationResult.Yes)
					{
                        // If we have content open and it hasn't been persisted to the file system, 
                        // then persist those changes.
                        if ((Program.CurrentContent != null) && (Program.CurrentContent.HasChanges))
                        {
                            if (!Program.CurrentContent.Close())
                            {
                                e.Cancel = true;
                                return;
                            }

                        }
                        
                        // TODO: We need to update this to show a save as dialog (or re-use the current file name).
                        //       We also need to remember the last plug-in used to save this file.
                        Program.SaveEditorFile(Path.GetFullPath(@"..\..\..\..\Resources\FileSystems\BZipFileSystem.gorPack"));
					}

					if (result == ConfirmationResult.Cancel)
					{
						e.Cancel = true;
						return;
					}

					// Destroy the current content.
					Program.CurrentContent.Dispose();
					Program.CurrentContent = null;
				}
				
				if (this.WindowState != FormWindowState.Minimized)
				{
					Program.Settings.FormState = this.WindowState;
				}

				if (this.WindowState != FormWindowState.Normal)
				{
					Program.Settings.WindowDimensions = this.RestoreBounds;
				}
				else
				{
					Program.Settings.WindowDimensions = this.DesktopBounds;
				}
                
                // Remember the last file we had open.
                if (!string.IsNullOrWhiteSpace(Program.EditorFilePath))
                {
                    Program.Settings.LastEditorFile = Program.EditorFilePath;
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
		/// Function for idle time.
		/// </summary>
		/// <returns>TRUE to continue, FALSE to exit.</returns>
		private bool Idle()
		{
			Program.CurrentContent.Draw();

			return true;
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
				TreeNodeDirectory directoryNode = e.Node as TreeNodeDirectory;

				// Expand sub folders.
				if (directoryNode != null)
				{
					GetFolders(directoryNode);
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
				TreeNodeDirectory subNode = new TreeNodeDirectory(subDirectory);

				if ((subDirectory.Directories.Count > 0) || (subDirectory.Files.Count > 0))
				{
					subNode.Nodes.Add(new TreeNode("DummyNode"));
				}

				rootNode.Nodes.Add(subNode);
			}

			// Add file nodes.
			foreach (var file in rootNode.Directory.Files.OrderBy(item => item.Name))
			{
				TreeNodeFile fileNode = new TreeNodeFile(file);
				rootNode.Nodes.Add(fileNode);
			}
		}

		/// <summary>
		/// Function to initialize the files tree.
		/// </summary>
		private void InitializeTree()
		{
			_rootNode = new RootNodeDirectory();

			// If we have files or sub directories, dump them in here.
			if ((Program.ScratchFiles.RootDirectory.Directories.Count > 0) || (Program.ScratchFiles.RootDirectory.Files.Count > 0))
			{
				_rootNode.Nodes.Add(new TreeNode("DummyNode"));
			}

			treeFiles.BeginUpdate();
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
        private TreeNodeDirectory GetDirectoryFromNode()
        {
			TreeNodeDirectory directory = null;

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
        /// Function to create a new file node in the tree.
        /// </summary>
        /// <param name="content">Content object to use.</param>
        private void CreateNewFileNode(ContentObject content)
        {
            var directoryNode = GetDirectoryFromNode();
            GorgonFileSystemFileEntry file = null;
            TreeNodeFile newNode = null;
            string extension = Path.GetExtension(content.Name).ToLower();

			// Write the file.
			file = Program.ScratchFiles.WriteFile(directoryNode.Directory.FullPath + content.Name, null);
			newNode = new TreeNodeFile(file);

			content.HasChanges = true;
			content.Persist(file);

            // Add to our changed item list.
            // We set this to true to indicate that this is a new file.
            Program.ChangedItems[file.FullPath.ToLower()] = true;

			// Add to tree and select.
			directoryNode.Nodes.Add(newNode);
            directoryNode.IsUpdated = true;
			treeFiles.SelectedNode = newNode;

			newNode.Redraw();
		}

		/// <summary>
		/// Function to add content to the interface.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddContent(object sender, EventArgs e)
		{		
			ContentObject content = null;
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			ContentPlugIn plugIn = null;

			if (item == null)
			{
				return;
			}

			plugIn = item.Tag as ContentPlugIn;

			if (plugIn == null)
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				content = plugIn.CreateContentObject();

				// Create the content settings.
				if (!content.CreateNew())
				{
					content.Dispose();
					content = null;
					return;
				}

                // Reset to a wait cursor.
                Cursor.Current = Cursors.WaitCursor;

                // Show the content in the editor.
				LoadContentPane(ref content);

				if (content != null)
				{
					// Create the node in the tree.
					CreateNewFileNode(content);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);

				// Load the default pane.
				LoadContentPane<DefaultContent>();

				if (content != null)
				{
					content.Dispose();
					content = null;
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
                ValidateControls();
			}
		}

        /// <summary>
        /// Handles the Click event of the itemOpen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemOpen_Click(object sender, EventArgs e)
        {
            StringBuilder extensions = new StringBuilder(512);

            try
            {
                // TODO: Check to see if the current file needs saving and prompt to save.

                if (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile))
                {
                    dialogOpenFile.InitialDirectory = Path.GetDirectoryName(Program.Settings.LastEditorFile);
                }

                // Add extensions from file system providers.
                foreach (var provider in Program.ScratchFiles.Providers)
                {
                    foreach (var extension in provider.PreferredExtensions)
                    {
                        if (extensions.Length > 0)
                        {
                            extensions.Append("|");
                        }
                        extensions.Append(extension);
                    }
                }
                                
                dialogOpenFile.Filter = extensions.ToString();

                if (dialogOpenFile.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    Program.OpenEditorFile(dialogOpenFile.FileName);
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
		/// Function to initialize the global interface commands for each content plug-in.
		/// </summary>
		private void InitializeInterface()
		{
			foreach (var plugIn in Program.ContentPlugIns)
			{
				// Get the menu item.
				var createItem = plugIn.Value.GetCreateMenuItem();

				if (createItem != null)
				{
					// Add to the 3 "Add" loctaions.
					popupAddContentMenu.Items.Add(createItem);

					// Click event.
					createItem.Click += AddContent;
				}
			}

			// Enable the add items if we have anything new.
			popupItemAdd.Enabled = dropNewContent.Enabled = itemAdd.Enabled = itemAdd.DropDownItems.Count > 0;
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

				this.Location = Program.Settings.WindowDimensions.Location;
				this.Size = Program.Settings.WindowDimensions.Size;

				// If this window can't be placed on a monitor, then shift it to the primary.
				if (!Screen.AllScreens.Any(item => item.Bounds.Contains(this.Location)))
				{
					this.Location = Screen.PrimaryScreen.Bounds.Location;
				}

				this.WindowState = Program.Settings.FormState;

				InitializeInterface();

				InitializeTree();
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
		}
		#endregion
	}
}
