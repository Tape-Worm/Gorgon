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
using Aga.Controls.Tree;
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
		private Font _unSavedFont = null;									// Font for unsaved documents.
		private bool _wasSaved = false;										// Flag to indicate that the project was previously saved.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
        /// <summary>
        /// Function to validate the controls on the display.
        /// </summary>
        private void ValidateControls()
        {
			itemImport.Enabled = Program.ContentPlugIns.Any(item => item.Value.SupportsImport);
			itemExport.Enabled = Program.ContentPlugIns.Any(item => item.Value.SupportsExport);

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
                itemAdd.Enabled = true;
                dropNewContent.Enabled = true;
                popupItemAdd.Enabled = true;
                itemDelete.Visible = false;
                toolStripSeparator4.Visible = false;
                itemRenameFolder.Visible = false;
            }
            else
            {
                var node = treeFiles.SelectedNode.Tag as Node;
                                
                if (node.Tag is GorgonFileSystemDirectory)
                {
                    itemAdd.Enabled = true;
                    dropNewContent.Enabled = true;
                    popupItemAdd.Enabled = true;
                    itemOpen.Enabled = false;
                    itemCreateFolder.Enabled = true;
                    itemCreateFolder.Visible = true;
                    if (node.Tag != Program.ScratchFiles.RootDirectory)
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

                if (node.Tag is GorgonFileSystemFileEntry)
                {
                    GorgonFileSystemFileEntry file = (GorgonFileSystemFileEntry)node.Tag;

					buttonDeleteContent.Enabled = true;
					buttonEditContent.Enabled = itemEdit.Visible = itemEdit.Enabled = Program.ContentPlugIns.Any(item => item.Value.FileExtensions.ContainsKey(file.Extension.ToLower()));
                    itemDelete.Enabled = true;
                    itemRenameFolder.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the treeFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void treeFiles_SelectionChanged(object sender, EventArgs e)
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
		/// <param name="content">Content object to load in.</param>
		private void LoadContentPane(ref ContentObject content)
		{
			Control control = null;

			// Turn off rendering.
			Gorgon.ApplicationIdleLoopMethod = null;

			if (content == null)
			{
				content = new DefaultContent();				
			}

			// Create the content resources and such.
			control = content.InitializeContent();

			// We couldn't get an interface component, fall back to the default display.
			if (control == null)
			{
				content.Dispose();
				content = new DefaultContent();
				control = content.InitializeContent();
			}

			control.Dock = DockStyle.Fill;

			// If we have content loaded, ensure we get a chance to save it.
			if (Program.CurrentContent != null)
			{
				if (!Program.CurrentContent.Close())
				{
					if (content != null)
					{
						content.Dispose();
						content = null;
					}

					return;
				}

				// Destroy the previous content.
				Program.CurrentContent.Dispose();
				Program.CurrentContent = null;
			}

			// Add to the main interface.
			Program.CurrentContent = content;
			splitEdit.Panel1.Controls.Add(control);			

			// If the current content has a renderer, then activate it.
			// Otherwise, turn it off to conserve cycles.
			if (content.HasRenderer)
			{
				Gorgon.ApplicationIdleLoopMethod = Idle;
			}
		}

		/// <summary>
		/// Function to load content into the interface from a plug-in.
		/// </summary>
		/// <param name="plugIn">Plug-in containing the content to interface.</param>
		internal void LoadContentPane(ContentPlugIn plugIn)
		{
			var result = plugIn.CreateContentObject();

			LoadContentPane(ref result);
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
				if ((Program.CurrentContent != null) && (Program.CurrentContent.HasChanges))
				{
					ConfirmationResult result = ConfirmationResult.None;

					result = GorgonDialogs.ConfirmBox(this, "The " + Program.CurrentContent.ContentType + " '" + Program.CurrentContent.Name + "' has changes.  Would you like to save it?", true, false);

					if (result == ConfirmationResult.Yes)
					{
						// TODO:
						// Program.CurrentContent.Save();
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
				
				if (_unSavedFont != null)
				{
					_unSavedFont.Dispose();
					_unSavedFont = null;
				}

				_nodeText.DrawText -= new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(_nodeText_DrawText);

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
		/// Handles the Collapsed event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewAdvEventArgs"/> instance containing the event data.</param>
		private void treeFiles_Collapsed(object sender, TreeViewAdvEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				Node node = e.Node.Tag as Node;

				if (node == null)
				{
					return;
				}

				if (node.Tag is GorgonFileSystemDirectory)
				{
					if (node.Tag != Program.ScratchFiles.RootDirectory)
					{
						node.Image = Properties.Resources.folder_16x16;
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
			}
		}

		/// <summary>
		/// Handles the Expanding event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewAdvEventArgs"/> instance containing the event data.</param>
		private void treeFiles_Expanding(object sender, TreeViewAdvEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				Node node = e.Node.Tag as Node;

				if (node == null)
				{
					return;
				}

				// Expand sub folders.
				if (node.Tag is GorgonFileSystemDirectory)
				{
					GetFolders(node);
					if (node.Tag != Program.ScratchFiles.RootDirectory)
					{
						node.Image = Properties.Resources.folder_open_16x16;
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
			}
		}

		/// <summary>
		/// Handles the DrawText event of the _nodeText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Aga.Controls.Tree.NodeControls.DrawEventArgs"/> instance containing the event data.</param>
		private void _nodeText_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			Node node = e.Node.Tag as Node;

			e.TextColor = Color.White;

			if (node != null)
			{
				var file = node.Tag as GorgonFileSystemFileEntry;

				// Check to see if we have any content editors that can open this type of file.
				if (file != null)
				{
					if (!Program.ContentPlugIns.Any(item => item.Value.FileExtensions.ContainsKey(file.Extension.ToLower())))
					{
						e.TextColor = DarkFormsRenderer.DisabledColor;
					}
					return;
				}
			}

			/*if (document != null)
			{
				if (!document.CanOpen)
					e.TextColor = Color.Black;

				if ((document.NeedsSave) && (document.CanSave))
					e.Font = _unSavedFont;
			}*/			
		}

		/// <summary>
		/// Function to retrieve the folder nodes.
		/// </summary>
		/// <param name="rootNode">Node to add folder information into.</param>
		private void GetFolders(Node rootNode)
		{
			GorgonFileSystemDirectory directory = rootNode.Tag as GorgonFileSystemDirectory;

			// Get the sub directories.
			rootNode.Nodes.Clear();

			foreach (var subDirectory in directory.Directories.OrderBy(item => item.Name))
			{
				Node subNode = new Node(subDirectory.Name);
				subNode.Tag = subDirectory;
				subNode.Image = Properties.Resources.folder_16x16;				

				if ((subDirectory.Directories.Count > 0) || (subDirectory.Files.Count > 0))
				{
					subNode.Nodes.Add(new Node("DummyNode"));
				}

				rootNode.Nodes.Add(subNode);
			}

			// Add file nodes.
			foreach (var file in directory.Files.OrderBy(item => item.Name))
			{
				Node fileNode = new Node(file.Name);
				fileNode.Tag = file;
				fileNode.Image = Properties.Resources.unknown_document_16x16;

				// Look through content providers to get content icon.
				if (!string.IsNullOrWhiteSpace(file.Extension))
				{
					var contentPlugIn = (from plugIn in Program.ContentPlugIns
										where plugIn.Value.FileExtensions.ContainsKey(file.Extension.ToLower())
										select plugIn.Value).FirstOrDefault();

					if (contentPlugIn != null)
					{
						fileNode.Image = contentPlugIn.GetContentIcon();
					}
				}


				rootNode.Nodes.Add(fileNode);
			}
		}

		/// <summary>
		/// Function to initialize the files tree.
		/// </summary>
		private void InitializeTree()
		{
			Node rootNode = new Node(Program.ProjectFile);
			rootNode.Image = Properties.Resources.project_node_16x16;
			rootNode.Tag = Program.ScratchFiles.RootDirectory;

			// If we have files or sub directories, dump them in here.
			if ((Program.ScratchFiles.RootDirectory.Directories.Count > 0) || (Program.ScratchFiles.RootDirectory.Files.Count > 0))
			{
				rootNode.Nodes.Add(new Node("DummyNode"));
			}

			_nodeText.DrawText += new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(_nodeText_DrawText);
			treeFiles.Model = new TreeModel();

			treeFiles.BeginUpdate();
			((TreeModel)treeFiles.Model).Nodes.Add(rootNode);
			treeFiles.EndUpdate();

			treeFiles.Root.Children[0].Expand();
		}

		/// <summary>
		/// Function to add content to the interface.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddContent(object sender, EventArgs e)
		{			
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
				LoadContentPane(plugIn);

				if (!Program.CurrentContent.CreateNew())
				{
					LoadContentPane<DefaultContent>();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);

				// Load the default pane.
				LoadContentPane<DefaultContent>();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
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

			// Force the splitter width to stay at 4 pixels!
			splitEdit.SplitterWidth = 4;

			_unSavedFont = new Font(this.Font, FontStyle.Bold);
		}
		#endregion
	}
}
