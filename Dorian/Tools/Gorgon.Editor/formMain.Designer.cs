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
// Created: Monday, April 30, 2012 6:28:36 PM
// 
#endregion

namespace GorgonLibrary.Editor
{
	partial class formMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.itemNew = new System.Windows.Forms.ToolStripMenuItem();
            this.itemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.itemAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.popupAddContentMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.popupItemAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
            this.itemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.itemImport = new System.Windows.Forms.ToolStripMenuItem();
            this.itemExport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.itemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.dropNewContent = new System.Windows.Forms.ToolStripDropDownButton();
            this.splitEdit = new System.Windows.Forms.SplitContainer();
            this.tabDocumentManager = new KRBTabControl.KRBTabControl();
            this.pageItems = new KRBTabControl.TabPageEx();
            this.containerFiles = new System.Windows.Forms.ToolStripContainer();
            this.treeFiles = new Aga.Controls.Tree.TreeViewAdv();
            this.columnFiles = new Aga.Controls.Tree.TreeColumn();
            this.popupFileSystem = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.itemCreateFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.itemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.itemRenameFolder = new System.Windows.Forms.ToolStripMenuItem();
            this._nodeImage = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
            this._nodeText = new Aga.Controls.Tree.NodeControls.NodeTextBox();
            this.stripContent = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonEditContent = new System.Windows.Forms.ToolStripButton();
            this.buttonDeleteContent = new System.Windows.Forms.ToolStripButton();
            this.pageProperties = new KRBTabControl.TabPageEx();
            this.propertyItem = new System.Windows.Forms.PropertyGrid();
            this.popupProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemResetValue = new System.Windows.Forms.ToolStripMenuItem();
            this.dialogExport = new System.Windows.Forms.SaveFileDialog();
            this.panelEditor = new System.Windows.Forms.Panel();
            this.stripStatus = new System.Windows.Forms.StatusStrip();
            this.menuMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitEdit)).BeginInit();
            this.splitEdit.Panel2.SuspendLayout();
            this.splitEdit.SuspendLayout();
            this.tabDocumentManager.SuspendLayout();
            this.pageItems.SuspendLayout();
            this.containerFiles.ContentPanel.SuspendLayout();
            this.containerFiles.TopToolStripPanel.SuspendLayout();
            this.containerFiles.SuspendLayout();
            this.popupFileSystem.SuspendLayout();
            this.stripContent.SuspendLayout();
            this.pageProperties.SuspendLayout();
            this.popupProperties.SuspendLayout();
            this.panelEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(1084, 24);
            this.menuMain.TabIndex = 0;
            this.menuMain.Text = "menuStrip1";
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNew,
            this.itemOpen,
            this.toolStripSeparator1,
            this.itemAdd,
            this.toolStripSeparator3,
            this.itemSave,
            this.itemSaveAs,
            this.toolStripMenuItem5,
            this.itemImport,
            this.itemExport,
            this.toolStripSeparator2,
            this.itemExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 20);
            this.menuFile.Text = "File";
            // 
            // itemNew
            // 
            this.itemNew.Name = "itemNew";
            this.itemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.itemNew.Size = new System.Drawing.Size(155, 22);
            this.itemNew.Text = "&New...";
            // 
            // itemOpen
            // 
            this.itemOpen.Enabled = false;
            this.itemOpen.Name = "itemOpen";
            this.itemOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.itemOpen.Size = new System.Drawing.Size(155, 22);
            this.itemOpen.Text = "&Open...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // itemAdd
            // 
            this.itemAdd.DropDown = this.popupAddContentMenu;
            this.itemAdd.Enabled = false;
            this.itemAdd.Name = "itemAdd";
            this.itemAdd.Size = new System.Drawing.Size(155, 22);
            this.itemAdd.Text = "Add...";
            // 
            // popupAddContentMenu
            // 
            this.popupAddContentMenu.Name = "popupAddContentMenu";
            this.popupAddContentMenu.OwnerItem = this.dropNewContent;
            this.popupAddContentMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // popupItemAdd
            // 
            this.popupItemAdd.DropDown = this.popupAddContentMenu;
            this.popupItemAdd.Enabled = false;
            this.popupItemAdd.Name = "popupItemAdd";
            this.popupItemAdd.Size = new System.Drawing.Size(181, 22);
            this.popupItemAdd.Text = "Add...";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(152, 6);
            // 
            // itemSave
            // 
            this.itemSave.Enabled = false;
            this.itemSave.Name = "itemSave";
            this.itemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.itemSave.Size = new System.Drawing.Size(155, 22);
            this.itemSave.Text = "&Save...";
            // 
            // itemSaveAs
            // 
            this.itemSaveAs.Enabled = false;
            this.itemSaveAs.Name = "itemSaveAs";
            this.itemSaveAs.Size = new System.Drawing.Size(155, 22);
            this.itemSaveAs.Text = "Save &as...";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(152, 6);
            // 
            // itemImport
            // 
            this.itemImport.Enabled = false;
            this.itemImport.Name = "itemImport";
            this.itemImport.Size = new System.Drawing.Size(155, 22);
            this.itemImport.Text = "Import...";
            // 
            // itemExport
            // 
            this.itemExport.Enabled = false;
            this.itemExport.Name = "itemExport";
            this.itemExport.Size = new System.Drawing.Size(155, 22);
            this.itemExport.Text = "&Export...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(152, 6);
            // 
            // itemExit
            // 
            this.itemExit.Name = "itemExit";
            this.itemExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.itemExit.Size = new System.Drawing.Size(155, 22);
            this.itemExit.Text = "E&xit";
            this.itemExit.Click += new System.EventHandler(this.itemExit_Click);
            // 
            // dropNewContent
            // 
            this.dropNewContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.dropNewContent.DropDown = this.popupAddContentMenu;
            this.dropNewContent.Enabled = false;
            this.dropNewContent.Image = global::GorgonLibrary.Editor.Properties.Resources.new_item_16x16;
            this.dropNewContent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.dropNewContent.Name = "dropNewContent";
            this.dropNewContent.Size = new System.Drawing.Size(29, 22);
            this.dropNewContent.Text = "New content item";
            // 
            // splitEdit
            // 
            this.splitEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.splitEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitEdit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitEdit.Location = new System.Drawing.Point(0, 24);
            this.splitEdit.Name = "splitEdit";
            // 
            // splitEdit.Panel1
            // 
            this.splitEdit.Panel1.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
            // 
            // splitEdit.Panel2
            // 
            this.splitEdit.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.splitEdit.Panel2.Controls.Add(this.tabDocumentManager);
            this.splitEdit.Size = new System.Drawing.Size(1084, 691);
            this.splitEdit.SplitterDistance = 850;
            this.splitEdit.TabIndex = 0;
            // 
            // tabDocumentManager
            // 
            this.tabDocumentManager.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabDocumentManager.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
            this.tabDocumentManager.AllowDrop = true;
            this.tabDocumentManager.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabDocumentManager.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
            this.tabDocumentManager.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabDocumentManager.Controls.Add(this.pageItems);
            this.tabDocumentManager.Controls.Add(this.pageProperties);
            this.tabDocumentManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDocumentManager.IsCaptionVisible = false;
            this.tabDocumentManager.IsDocumentTabStyle = true;
            this.tabDocumentManager.IsDrawHeader = false;
            this.tabDocumentManager.IsUserInteraction = false;
            this.tabDocumentManager.ItemSize = new System.Drawing.Size(0, 28);
            this.tabDocumentManager.Location = new System.Drawing.Point(0, 0);
            this.tabDocumentManager.Name = "tabDocumentManager";
            this.tabDocumentManager.SelectedIndex = 0;
            this.tabDocumentManager.Size = new System.Drawing.Size(230, 691);
            this.tabDocumentManager.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabDocumentManager.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabDocumentManager.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabDocumentManager.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.White;
            this.tabDocumentManager.TabGradient.TabPageTextColor = System.Drawing.Color.White;
            this.tabDocumentManager.TabHOffset = -1;
            this.tabDocumentManager.TabIndex = 1;
            this.tabDocumentManager.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
            // 
            // pageItems
            // 
            this.pageItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.pageItems.Controls.Add(this.containerFiles);
            this.pageItems.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageItems.ForeColor = System.Drawing.Color.White;
            this.pageItems.IsClosable = false;
            this.pageItems.Location = new System.Drawing.Point(1, 1);
            this.pageItems.Margin = new System.Windows.Forms.Padding(3, 11587824, 3, 11587824);
            this.pageItems.Name = "pageItems";
            this.pageItems.Size = new System.Drawing.Size(228, 656);
            this.pageItems.TabIndex = 1;
            this.pageItems.Text = "Items";
            // 
            // containerFiles
            // 
            // 
            // containerFiles.ContentPanel
            // 
            this.containerFiles.ContentPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.containerFiles.ContentPanel.Controls.Add(this.treeFiles);
            this.containerFiles.ContentPanel.ForeColor = System.Drawing.Color.White;
            this.containerFiles.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 11587824, 3, 11587824);
            this.containerFiles.ContentPanel.Size = new System.Drawing.Size(228, 631);
            this.containerFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containerFiles.Location = new System.Drawing.Point(0, 0);
            this.containerFiles.Margin = new System.Windows.Forms.Padding(3, 11587824, 3, 11587824);
            this.containerFiles.Name = "containerFiles";
            this.containerFiles.Size = new System.Drawing.Size(228, 656);
            this.containerFiles.TabIndex = 0;
            this.containerFiles.Text = "toolStripContainer1";
            // 
            // containerFiles.TopToolStripPanel
            // 
            this.containerFiles.TopToolStripPanel.Controls.Add(this.stripContent);
            // 
            // treeFiles
            // 
            this.treeFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.treeFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeFiles.Columns.Add(this.columnFiles);
            this.treeFiles.ContextMenuStrip = this.popupFileSystem;
            this.treeFiles.DefaultToolTipProvider = null;
            this.treeFiles.DisplayDraggingNodes = true;
            this.treeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeFiles.DragDropMarkColor = System.Drawing.Color.DodgerBlue;
            this.treeFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeFiles.ForeColor = System.Drawing.Color.White;
            this.treeFiles.LineColor = System.Drawing.Color.Black;
            this.treeFiles.Location = new System.Drawing.Point(0, 0);
            this.treeFiles.Margin = new System.Windows.Forms.Padding(3, 276, 3, 276);
            this.treeFiles.Model = null;
            this.treeFiles.Name = "treeFiles";
            this.treeFiles.NodeControls.Add(this._nodeImage);
            this.treeFiles.NodeControls.Add(this._nodeText);
            this.treeFiles.SelectedNode = null;
            this.treeFiles.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.Multi;
            this.treeFiles.ShowLines = false;
            this.treeFiles.Size = new System.Drawing.Size(228, 631);
            this.treeFiles.TabIndex = 0;
            this.treeFiles.SelectionChanged += new System.EventHandler(this.treeFiles_SelectionChanged);
            this.treeFiles.Collapsed += new System.EventHandler<Aga.Controls.Tree.TreeViewAdvEventArgs>(this.treeFiles_Collapsed);
            this.treeFiles.Expanding += new System.EventHandler<Aga.Controls.Tree.TreeViewAdvEventArgs>(this.treeFiles_Expanding);
            // 
            // columnFiles
            // 
            this.columnFiles.Header = "";
            this.columnFiles.SortOrder = System.Windows.Forms.SortOrder.None;
            this.columnFiles.TooltipText = null;
            // 
            // popupFileSystem
            // 
            this.popupFileSystem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemEdit,
            this.toolStripSeparator5,
            this.popupItemAdd,
            this.itemCreateFolder,
            this.itemDelete,
            this.toolStripSeparator4,
            this.itemRenameFolder});
            this.popupFileSystem.Name = "popupFileSystem";
            this.popupFileSystem.Size = new System.Drawing.Size(182, 148);
            // 
            // itemEdit
            // 
            this.itemEdit.Enabled = false;
            this.itemEdit.Name = "itemEdit";
            this.itemEdit.Size = new System.Drawing.Size(181, 22);
            this.itemEdit.Text = "Open...";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(178, 6);
            // 
            // itemCreateFolder
            // 
            this.itemCreateFolder.Enabled = false;
            this.itemCreateFolder.Name = "itemCreateFolder";
            this.itemCreateFolder.Size = new System.Drawing.Size(181, 22);
            this.itemCreateFolder.Text = "Create Folder...";
            // 
            // itemDelete
            // 
            this.itemDelete.Enabled = false;
            this.itemDelete.Name = "itemDelete";
            this.itemDelete.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D)));
            this.itemDelete.Size = new System.Drawing.Size(181, 22);
            this.itemDelete.Text = "Delete...";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(178, 6);
            // 
            // itemRenameFolder
            // 
            this.itemRenameFolder.Enabled = false;
            this.itemRenameFolder.Name = "itemRenameFolder";
            this.itemRenameFolder.Size = new System.Drawing.Size(181, 22);
            this.itemRenameFolder.Text = "Rename Folder...";
            // 
            // _nodeImage
            // 
            this._nodeImage.DataPropertyName = "Image";
            this._nodeImage.LeftMargin = 0;
            this._nodeImage.ParentColumn = null;
            this._nodeImage.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
            // 
            // _nodeText
            // 
            this._nodeText.DataPropertyName = "Text";
            this._nodeText.EditEnabled = true;
            this._nodeText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._nodeText.IncrementalSearchEnabled = true;
            this._nodeText.LeftMargin = 3;
            this._nodeText.ParentColumn = null;
            // 
            // stripContent
            // 
            this.stripContent.Dock = System.Windows.Forms.DockStyle.None;
            this.stripContent.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripContent.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dropNewContent,
            this.toolStripButton1,
            this.buttonEditContent,
            this.buttonDeleteContent});
            this.stripContent.Location = new System.Drawing.Point(0, 0);
            this.stripContent.Name = "stripContent";
            this.stripContent.Size = new System.Drawing.Size(228, 25);
            this.stripContent.Stretch = true;
            this.stripContent.TabIndex = 0;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonEditContent
            // 
            this.buttonEditContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonEditContent.Enabled = false;
            this.buttonEditContent.Image = global::GorgonLibrary.Editor.Properties.Resources.edit_16x16;
            this.buttonEditContent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonEditContent.Name = "buttonEditContent";
            this.buttonEditContent.Size = new System.Drawing.Size(23, 22);
            this.buttonEditContent.Text = "Edit selected content";
            // 
            // buttonDeleteContent
            // 
            this.buttonDeleteContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonDeleteContent.Enabled = false;
            this.buttonDeleteContent.Image = global::GorgonLibrary.Editor.Properties.Resources.delete_item_16x16;
            this.buttonDeleteContent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonDeleteContent.Name = "buttonDeleteContent";
            this.buttonDeleteContent.Size = new System.Drawing.Size(23, 22);
            this.buttonDeleteContent.Text = "Delete selected content";
            // 
            // pageProperties
            // 
            this.pageProperties.BackColor = System.Drawing.Color.DimGray;
            this.pageProperties.Controls.Add(this.propertyItem);
            this.pageProperties.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageProperties.ImageIndex = 3;
            this.pageProperties.IsClosable = false;
            this.pageProperties.Location = new System.Drawing.Point(1, 1);
            this.pageProperties.Margin = new System.Windows.Forms.Padding(3, 11587824, 3, 11587824);
            this.pageProperties.Name = "pageProperties";
            this.pageProperties.Size = new System.Drawing.Size(261, 886);
            this.pageProperties.TabIndex = 0;
            this.pageProperties.Text = "Properties";
            // 
            // propertyItem
            // 
            this.propertyItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.propertyItem.CategoryForeColor = System.Drawing.Color.White;
            this.propertyItem.CategorySplitterColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(85)))), ((int)(((byte)(90)))));
            this.propertyItem.CommandsActiveLinkColor = System.Drawing.Color.Lavender;
            this.propertyItem.CommandsBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.propertyItem.CommandsDisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.propertyItem.CommandsForeColor = System.Drawing.Color.White;
            this.propertyItem.CommandsLinkColor = System.Drawing.Color.SteelBlue;
            this.propertyItem.ContextMenuStrip = this.popupProperties;
            this.propertyItem.DisabledItemForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.propertyItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyItem.HelpBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.propertyItem.HelpBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.propertyItem.HelpForeColor = System.Drawing.Color.White;
            this.propertyItem.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.propertyItem.Location = new System.Drawing.Point(0, 0);
            this.propertyItem.Margin = new System.Windows.Forms.Padding(3, 11587824, 3, 11587824);
            this.propertyItem.Name = "propertyItem";
            this.propertyItem.SelectedItemWithFocusBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.propertyItem.SelectedItemWithFocusForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.propertyItem.Size = new System.Drawing.Size(261, 886);
            this.propertyItem.TabIndex = 0;
            this.propertyItem.ToolbarVisible = false;
            this.propertyItem.ViewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.propertyItem.ViewBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.propertyItem.ViewForeColor = System.Drawing.Color.White;
            // 
            // popupProperties
            // 
            this.popupProperties.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemResetValue});
            this.popupProperties.Name = "popupProperties";
            this.popupProperties.Size = new System.Drawing.Size(135, 26);
            this.popupProperties.Opening += new System.ComponentModel.CancelEventHandler(this.popupProperties_Opening);
            // 
            // itemResetValue
            // 
            this.itemResetValue.Name = "itemResetValue";
            this.itemResetValue.Size = new System.Drawing.Size(134, 22);
            this.itemResetValue.Text = "&Reset Value";
            this.itemResetValue.Click += new System.EventHandler(this.itemResetValue_Click);
            // 
            // dialogExport
            // 
            this.dialogExport.Filter = "All files (*.*)|*.*";
            this.dialogExport.Title = "Export current document.";
            // 
            // panelEditor
            // 
            this.panelEditor.Controls.Add(this.splitEdit);
            this.panelEditor.Controls.Add(this.stripStatus);
            this.panelEditor.Controls.Add(this.menuMain);
            this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEditor.Location = new System.Drawing.Point(4, 28);
            this.panelEditor.Name = "panelEditor";
            this.panelEditor.Size = new System.Drawing.Size(1084, 737);
            this.panelEditor.TabIndex = 4;
            // 
            // stripStatus
            // 
            this.stripStatus.Location = new System.Drawing.Point(0, 715);
            this.stripStatus.Name = "stripStatus";
            this.stripStatus.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.stripStatus.Size = new System.Drawing.Size(1084, 22);
            this.stripStatus.TabIndex = 0;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.BorderWidth = 4;
            this.ClientSize = new System.Drawing.Size(1092, 769);
            this.Controls.Add(this.panelEditor);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "formMain";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Gorgon Editor";
            this.Controls.SetChildIndex(this.panelEditor, 0);
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            this.splitEdit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitEdit)).EndInit();
            this.splitEdit.ResumeLayout(false);
            this.tabDocumentManager.ResumeLayout(false);
            this.pageItems.ResumeLayout(false);
            this.containerFiles.ContentPanel.ResumeLayout(false);
            this.containerFiles.TopToolStripPanel.ResumeLayout(false);
            this.containerFiles.TopToolStripPanel.PerformLayout();
            this.containerFiles.ResumeLayout(false);
            this.containerFiles.PerformLayout();
            this.popupFileSystem.ResumeLayout(false);
            this.stripContent.ResumeLayout(false);
            this.stripContent.PerformLayout();
            this.pageProperties.ResumeLayout(false);
            this.popupProperties.ResumeLayout(false);
            this.panelEditor.ResumeLayout(false);
            this.panelEditor.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PropertyGrid propertyItem;
		private System.Windows.Forms.MenuStrip menuMain;
		private System.Windows.Forms.ContextMenuStrip popupProperties;
		private System.Windows.Forms.ToolStripMenuItem itemResetValue;
		private System.Windows.Forms.ToolStripMenuItem menuFile;
		private System.Windows.Forms.ToolStripMenuItem itemNew;
		private System.Windows.Forms.ToolStripMenuItem itemOpen;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem itemSave;
		private System.Windows.Forms.ToolStripMenuItem itemSaveAs;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem itemExit;
		private KRBTabControl.KRBTabControl tabDocumentManager;
		private KRBTabControl.TabPageEx pageProperties;
		private KRBTabControl.TabPageEx pageItems;
		private System.Windows.Forms.ToolStripContainer containerFiles;
		private Aga.Controls.Tree.TreeViewAdv treeFiles;
		private Aga.Controls.Tree.TreeColumn columnFiles;
		private Aga.Controls.Tree.NodeControls.NodeTextBox _nodeText;
		private Aga.Controls.Tree.NodeControls.NodeStateIcon _nodeImage;
		private System.Windows.Forms.ToolStripMenuItem itemImport;
		private System.Windows.Forms.ToolStripMenuItem itemExport;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.SaveFileDialog dialogExport;
        private System.Windows.Forms.Panel panelEditor;
		private System.Windows.Forms.StatusStrip stripStatus;
		private System.Windows.Forms.SplitContainer splitEdit;
		private System.Windows.Forms.ToolStripMenuItem itemAdd;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ContextMenuStrip popupFileSystem;
		private System.Windows.Forms.ToolStripMenuItem popupItemAdd;
		private System.Windows.Forms.ToolStrip stripContent;
		private System.Windows.Forms.ToolStripDropDownButton dropNewContent;
		private System.Windows.Forms.ToolStripSeparator toolStripButton1;
		private System.Windows.Forms.ToolStripButton buttonEditContent;
		private System.Windows.Forms.ToolStripButton buttonDeleteContent;
		private System.Windows.Forms.ContextMenuStrip popupAddContentMenu;
		private System.Windows.Forms.ToolStripMenuItem itemEdit;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem itemDelete;
        private System.Windows.Forms.ToolStripMenuItem itemCreateFolder;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem itemRenameFolder;
	}
}

