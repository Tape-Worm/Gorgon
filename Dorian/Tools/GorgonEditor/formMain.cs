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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Aga.Controls.Tree;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;
using System.Collections;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Main application object.
	/// </summary>
	public partial class formMain
		: Form
	{
		#region Classes.
		/// <summary>
		/// Sorter for our file tree nodes.
		/// </summary>
		class FileNodeComparer
			: IComparer
		{
			#region IComparer Members
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
			/// </returns>
			/// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
			public int Compare(object x, object y)
			{
				Node left = (Node)x;
				Node right = (Node)y;
				
				if ((left.Tag is ProjectFolder) && (right.Tag is Document))
					return -1;

				if ((left.Tag is Document) && (right.Tag is ProjectFolder))
					return 1;

				if (((left.Tag is Document) && (right.Tag is Document)) || ((left.Tag is ProjectFolder) && (right.Tag is ProjectFolder)))
					return string.Compare(left.Text, right.Text);

				return 0;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private SortedTreeModel _treeModel = null;							// Tree model.
		private DefaultDocument _defaultDocument = null;					// Our default page.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the itemImport control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemImport_Click(object sender, EventArgs e)
		{
			formImport importer = null;
			ProjectFolder selectedFolder = null;
			Document selectedDocument = null;
			string lastPath = Program.Settings.ImportLastProjectFolder;

			try
			{
				// Get the current destination from the tree if we've got a node selected.
				if ((treeFiles.SelectedNodes.Count == 1) && (treeFiles.SelectedNode != null))
				{
					selectedFolder = ((Node)treeFiles.SelectedNode.Tag).Tag as ProjectFolder;
					selectedDocument = ((Node)treeFiles.SelectedNode.Tag).Tag as Document;

					if (selectedDocument != null)
					{
						if (selectedDocument.Folder != null)
							lastPath = selectedDocument.Folder.Path;
						else
							lastPath = "/";
					}
					else if (selectedFolder != null)
						lastPath = selectedFolder.Path;
				}

				importer = new formImport();
				importer.DestinationPath = lastPath;
				importer.LastFilePath = Program.Settings.ImportLastFilePath;

				if (importer.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					// Import the documents.
					Program.Project.ImportDocuments(importer.Files, importer.Folder);

					Program.Settings.ImportLastFilePath = importer.LastFilePath;
					Program.Settings.ImportLastProjectFolder = importer.DestinationPath;
					Program.Settings.Save();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				if (importer != null)
					importer.Dispose();
			}
		}

		/// <summary>
		/// Handles the Selected event of the tabDocuments control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.TabControlEventArgs"/> instance containing the event data.</param>
		private void tabDocuments_Selected(object sender, TabControlEventArgs e)
		{
			try
			{
				var document = Program.Documents[e.TabPage as KRBTabControl.TabPageEx];

				if (document != null)
				{
					if (Program.CurrentDocument != null)
						Program.CurrentDocument.TerminateRendering();

					Program.CurrentDocument = document;

					Program.CurrentDocument.InitializeRendering();

					if (!Program.CurrentDocument.HasProperties)
					{
						if (tabDocumentManager.TabPages.Contains(pageProperties))
							tabDocumentManager.TabPages.Remove(pageProperties);
					}
					else
					{
						if (!tabDocumentManager.TabPages.Contains(pageProperties))
							tabDocumentManager.TabPages.Add(pageProperties);
					}

					propertyItem.SelectedObject = document.TypeDescriptor;
					propertyItem.Refresh();
				}
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
		/// Handles the TabPageClosing event of the tabDocuments control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KRBTabControl.KRBTabControl.SelectedIndexChangingEventArgs"/> instance containing the event data.</param>
		private void tabDocuments_TabPageClosing(object sender, KRBTabControl.KRBTabControl.SelectedIndexChangingEventArgs e)
		{
			try
			{
				var document = Program.Documents[e.TabPage];

				if (document != null)
				{
					ConfirmationResult result = ConfirmationResult.None;
					if (document.NeedsSave)
					{
						result = GorgonDialogs.ConfirmBox(this, "The " + document.DocumentType + " '" + document.Name + "' has changed.  Would you like to save it?", true, false);

						switch (result)
						{
							case ConfirmationResult.Cancel:
								e.Cancel = true;
								return;
						}
					}

					document.PropertyUpdated -= new EventHandler(FontUpdated);
					Program.Documents.Remove(document);
					document.Dispose();
				}
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
		/// Handles the Click event of the buttonDeleteItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonDeleteItem_Click(object sender, EventArgs e)
		{
			ConfirmationResult result = ConfirmationResult.None;

			try
			{

			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
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
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			Text = Program.Project.Name + " - Gorgon Editor";

			if (Program.Project.GetProjectState())
			{
				Text += "*";
				itemSaveAs.Enabled = itemSave.Enabled = true;
			}
			else
			{
				itemSaveAs.Enabled = itemSave.Enabled = false;
			}

			itemExport.Enabled = Program.CurrentDocument.CanSave;
		}

		/// <summary>
		/// Handles the ContextMenuShown event of the tabDocuments control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KRBTabControl.KRBTabControl.ContextMenuShownEventArgs"/> instance containing the event data.</param>
		private void tabDocuments_ContextMenuShown(object sender, KRBTabControl.KRBTabControl.ContextMenuShownEventArgs e)
		{
			if (tabDocuments.Controls.Count < 2)
			{
				var item = (from items in e.ContextMenu.Items.Cast<ToolStripMenuItem>()
							where string.Compare(items.Text, "available tab pages", true) == 0
							select items).FirstOrDefault();

				item.Visible = false;
			}
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
				_nodeText.DrawText -= new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(_nodeText_DrawText);

				// Assign events.
				((controlDefault)_defaultDocument.Control).buttonCreateFont.Click -= new EventHandler(itemNewFont_Click);

				foreach (var document in Program.Documents.ToArray())
					document.Dispose();

				Program.Documents.Clear();

				if (this.WindowState != FormWindowState.Minimized)
					Program.Settings.FormState = this.WindowState;
				if (this.WindowState != FormWindowState.Normal)
					Program.Settings.WindowDimensions = this.RestoreBounds;
				else
					Program.Settings.WindowDimensions = this.DesktopBounds;

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
		/// Function to close a document.
		/// </summary>
		/// <param name="document">Document to close.</param>
		private void CloseDocument(Document document)
		{
			TabPage page = document.Tab;
			document.Dispose();
			tabDocuments.TabPages.Remove(page);
		}

		/// <summary>
		/// Function called when the font property gets updated.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		private void FontUpdated(object sender, EventArgs e)
		{
			DocumentFont document = sender as DocumentFont;

			if (sender == null)
				return;

			try
			{
				document.Update();
			}
			finally
			{
				propertyItem.Refresh();
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemNewFont control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemNewFont_Click(object sender, EventArgs e)
		{
			formNewFont newFont = null;
			DocumentFont document = null;

			try
			{
				newFont = new formNewFont();
				if (newFont.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					Cursor.Current = Cursors.WaitCursor;
					document = OpenDocument<DocumentFont>(newFont.FontName, true);
					document.FontFamily = newFont.FontFamilyName;
					document.FontSize = newFont.FontSize;
					document.FontStyle = newFont.FontStyle;
					Program.Settings.FontSizeType = document.UsePointSize = newFont.FontHeightMode;
					Program.Settings.FontTextureSize = document.FontTextureSize = newFont.FontTextureSize;
					Program.Settings.FontAntiAliasMode = document.FontAntiAliasMode = newFont.FontAntiAliasMode;

					document.Update();
					document.SetDefaults();
					propertyItem.Refresh();

					document.PropertyUpdated += new EventHandler(FontUpdated);

					Program.Settings.Save();
				}
			}
			catch (Exception ex)
			{
				if (document != null)
					CloseDocument(document);

				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				if (newFont != null)
					newFont.Dispose();
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to open a document.
		/// </summary>
		/// <param name="documentName">Name of the document to open.</param>
		/// <param name="allowClose">TRUE to allow the document to close, FALSE to disallow.</param>
		private T OpenDocument<T>(string documentName, bool allowClose)
			where T : Document
		{
			T document = null;
			Type type = typeof(T);

			document = (T)Activator.CreateInstance(type, new object[] { documentName, allowClose, null });
			document.InitializeDocument();
			document.PropertyGrid = propertyItem;
			Program.Documents.Add(document);
			document.Tab.Font = this.Font;
			tabDocuments.TabPages.Add(document.Tab);
			tabDocuments.SelectedTab = document.Tab;
			document.InitializeRendering();

			propertyItem.SelectedObject = document.TypeDescriptor;
			propertyItem.Refresh();

			Program.CurrentDocument = document;

			if (!Program.CurrentDocument.HasProperties)
			{
				if (tabDocumentManager.TabPages.Contains(pageProperties))
					tabDocumentManager.TabPages.Remove(pageProperties);
			}
			else
			{
				if (!tabDocumentManager.TabPages.Contains(pageProperties))
					tabDocumentManager.TabPages.Add(pageProperties);
			}

			return document;
		}

		/// <summary>
		/// Function for idle time.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <returns>TRUE to continue, FALSE to exit.</returns>
		private bool Idle(GorgonFrameRate timing)
		{
			if (Program.CurrentDocument == null)
				return true;

			Program.CurrentDocument.RenderMethod(timing);

			return true;
		}

		/// <summary>
		/// Handles the DrawText event of the _nodeText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Aga.Controls.Tree.NodeControls.DrawEventArgs"/> instance containing the event data.</param>
		private void _nodeText_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			Document document = ((Node)e.Node.Tag).Tag as Document;

			e.TextColor = Color.White;

			if ((document != null) && (!document.CanOpen))
				e.TextColor = Color.Black;
		}

		/// <summary>
		/// Function to initialize the files tree.
		/// </summary>
		private void InitializeTree()
		{
			_nodeText.DrawText += new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(_nodeText_DrawText);
			_treeModel = new SortedTreeModel(new TreeModel());
			_treeModel.Comparer = new FileNodeComparer();
			treeFiles.Model = _treeModel;

			treeFiles.BeginUpdate();
			((TreeModel)_treeModel.InnerModel).Nodes.Add(Program.Project.RootNode);
			treeFiles.EndUpdate();

			treeFiles.Root.Children[0].Expand();
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
				ToolStripManager.Renderer = new MetroDarkRenderer();

				// Adjust main window.
				Visible = true;
				this.DesktopBounds = Program.Settings.WindowDimensions;

				// If this window can't be placed on a monitor, then shift it to the primary.
				if (Screen.AllScreens.Count(item => item.Bounds.Contains(this.Location)) == 0)
					this.Location = Screen.PrimaryScreen.Bounds.Location;

				this.WindowState = Program.Settings.FormState;
				this.tabDocuments.Focus();

				InitializeTree();

				_defaultDocument = OpenDocument<DefaultDocument>("Gorgon Editor", false);

				// Assign events.
				((controlDefault)_defaultDocument.Control).buttonCreateFont.Click += new EventHandler(itemNewFont_Click);

				Gorgon.ApplicationIdleLoopMethod = Idle;
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
