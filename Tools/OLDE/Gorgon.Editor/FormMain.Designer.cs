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

using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor
{
	partial class FormMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.itemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.itemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.itemAddContent = new System.Windows.Forms.ToolStripMenuItem();
			this.popupAddContentMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.dropNewContent = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemCreateFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.itemImport = new System.Windows.Forms.ToolStripMenuItem();
			this.itemExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuRecentFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.itemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.itemCut = new System.Windows.Forms.ToolStripMenuItem();
			this.itemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.itemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.itemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.itemPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemAddContent = new System.Windows.Forms.ToolStripMenuItem();
			this.tabDocumentManager = new KRBTabControl.KRBTabControl();
			this.pageItems = new KRBTabControl.TabPageEx();
			this.containerFiles = new System.Windows.Forms.ToolStripContainer();
			this.treeFiles = new Gorgon.Editor.EditorTreeView();
			this.popupFileSystem = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.popupItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemCreateFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.popupItemInclude = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemExclude = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemIncludeAll = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemExcludeAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.popupItemCut = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.popupItemRename = new System.Windows.Forms.ToolStripMenuItem();
			this.stripContent = new System.Windows.Forms.ToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEditContent = new System.Windows.Forms.ToolStripButton();
			this.buttonDeleteContent = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonShowAll = new System.Windows.Forms.ToolStripButton();
			this.pageProperties = new KRBTabControl.TabPageEx();
			this.propertyItem = new System.Windows.Forms.PropertyGrid();
			this.popupProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.itemResetValue = new System.Windows.Forms.ToolStripMenuItem();
			this.panelEditor = new System.Windows.Forms.Panel();
			this.splitPanelContainer = new System.Windows.Forms.Panel();
			this.splitPanel1 = new System.Windows.Forms.Panel();
			this.splitEditor = new System.Windows.Forms.Splitter();
			this.splitPanel2 = new System.Windows.Forms.Panel();
			this.stripStatus = new System.Windows.Forms.StatusStrip();
			this.dialogOpenFile = new System.Windows.Forms.OpenFileDialog();
			this.dialogSaveFile = new System.Windows.Forms.SaveFileDialog();
			this.dialogImport = new System.Windows.Forms.OpenFileDialog();
			this.dialogExport = new System.Windows.Forms.FolderBrowserDialog();
			this.menuMain.SuspendLayout();
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
			this.splitPanelContainer.SuspendLayout();
			this.splitPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// ContentArea
			// 
			this.ContentArea.BackColor = System.Drawing.SystemColors.Window;
			this.ContentArea.ForeColor = System.Drawing.Color.Silver;
			this.ContentArea.Location = new System.Drawing.Point(4, 36);
			this.ContentArea.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ContentArea.Size = new System.Drawing.Size(1084, 729);
			// 
			// menuMain
			// 
			this.menuMain.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuEdit});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(1084, 28);
			this.menuMain.TabIndex = 0;
			this.menuMain.Text = "menuStrip1";
			// 
			// menuFile
			// 
			this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNew,
            this.itemOpen,
            this.toolStripSeparator1,
            this.itemAddContent,
            this.itemCreateFolder,
            this.toolStripSeparator3,
            this.itemSave,
            this.itemSaveAs,
            this.toolStripMenuItem5,
            this.itemImport,
            this.itemExport,
            this.toolStripSeparator2,
            this.menuRecentFiles,
            this.toolStripSeparator8,
            this.itemExit});
			this.menuFile.Name = "menuFile";
			this.menuFile.Size = new System.Drawing.Size(44, 24);
			this.menuFile.Text = "&File";
			// 
			// itemNew
			// 
			this.itemNew.Name = "itemNew";
			this.itemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.itemNew.Size = new System.Drawing.Size(176, 24);
			this.itemNew.Text = "&New...";
			this.itemNew.Click += new System.EventHandler(this.itemNew_Click);
			// 
			// itemOpen
			// 
			this.itemOpen.Enabled = false;
			this.itemOpen.Name = "itemOpen";
			this.itemOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.itemOpen.Size = new System.Drawing.Size(176, 24);
			this.itemOpen.Text = "&Open...";
			this.itemOpen.Click += new System.EventHandler(this.itemOpen_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(173, 6);
			// 
			// itemAddContent
			// 
			this.itemAddContent.DropDown = this.popupAddContentMenu;
			this.itemAddContent.Enabled = false;
			this.itemAddContent.Name = "itemAddContent";
			this.itemAddContent.Size = new System.Drawing.Size(176, 24);
			this.itemAddContent.Text = "Add Content";
			// 
			// popupAddContentMenu
			// 
			this.popupAddContentMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.popupAddContentMenu.Name = "popupAddContentMenu";
			this.popupAddContentMenu.OwnerItem = this.popupItemAddContent;
			this.popupAddContentMenu.Size = new System.Drawing.Size(61, 4);
			// 
			// dropNewContent
			// 
			this.dropNewContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.dropNewContent.DropDown = this.popupAddContentMenu;
			this.dropNewContent.Enabled = false;
			this.dropNewContent.Image = global::Gorgon.Editor.Properties.Resources.new_item_16x16;
			this.dropNewContent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.dropNewContent.Name = "dropNewContent";
			this.dropNewContent.Size = new System.Drawing.Size(34, 24);
			this.dropNewContent.Text = "New content item";
			// 
			// itemCreateFolder
			// 
			this.itemCreateFolder.Enabled = false;
			this.itemCreateFolder.Name = "itemCreateFolder";
			this.itemCreateFolder.Size = new System.Drawing.Size(176, 24);
			this.itemCreateFolder.Text = "Create folder...";
			this.itemCreateFolder.Click += new System.EventHandler(this.itemCreateFolder_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(173, 6);
			// 
			// itemSave
			// 
			this.itemSave.Enabled = false;
			this.itemSave.Name = "itemSave";
			this.itemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.itemSave.Size = new System.Drawing.Size(176, 24);
			this.itemSave.Text = "&Save...";
			this.itemSave.Click += new System.EventHandler(this.itemSave_Click);
			// 
			// itemSaveAs
			// 
			this.itemSaveAs.Enabled = false;
			this.itemSaveAs.Name = "itemSaveAs";
			this.itemSaveAs.Size = new System.Drawing.Size(176, 24);
			this.itemSaveAs.Text = "Save &as...";
			this.itemSaveAs.Click += new System.EventHandler(this.itemSaveAs_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(173, 6);
			// 
			// itemImport
			// 
			this.itemImport.Name = "itemImport";
			this.itemImport.Size = new System.Drawing.Size(176, 24);
			this.itemImport.Text = "Import...";
			this.itemImport.Click += new System.EventHandler(this.itemImport_Click);
			// 
			// itemExport
			// 
			this.itemExport.Enabled = false;
			this.itemExport.Name = "itemExport";
			this.itemExport.Size = new System.Drawing.Size(176, 24);
			this.itemExport.Text = "&Export...";
			this.itemExport.Click += new System.EventHandler(this.itemExport_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(173, 6);
			// 
			// menuRecentFiles
			// 
			this.menuRecentFiles.Enabled = false;
			this.menuRecentFiles.Name = "menuRecentFiles";
			this.menuRecentFiles.Size = new System.Drawing.Size(176, 24);
			this.menuRecentFiles.Text = "Recent Files";
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(173, 6);
			// 
			// itemExit
			// 
			this.itemExit.Name = "itemExit";
			this.itemExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.itemExit.Size = new System.Drawing.Size(176, 24);
			this.itemExit.Text = "E&xit";
			this.itemExit.Click += new System.EventHandler(this.itemExit_Click);
			// 
			// menuEdit
			// 
			this.menuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemCut,
            this.itemCopy,
            this.itemPaste,
            this.toolStripSeparator6,
            this.itemDelete,
            this.toolStripSeparator7,
            this.itemPreferences});
			this.menuEdit.Name = "menuEdit";
			this.menuEdit.Size = new System.Drawing.Size(47, 24);
			this.menuEdit.Text = "&Edit";
			this.menuEdit.DropDownOpening += new System.EventHandler(this.menuEdit_DropDownOpening);
			// 
			// itemCut
			// 
			this.itemCut.Enabled = false;
			this.itemCut.Name = "itemCut";
			this.itemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.itemCut.Size = new System.Drawing.Size(242, 24);
			this.itemCut.Text = "C&ut";
			this.itemCut.Click += new System.EventHandler(this.itemCut_Click);
			// 
			// itemCopy
			// 
			this.itemCopy.Enabled = false;
			this.itemCopy.Name = "itemCopy";
			this.itemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.itemCopy.Size = new System.Drawing.Size(242, 24);
			this.itemCopy.Text = "&Copy";
			this.itemCopy.Click += new System.EventHandler(this.itemCut_Click);
			// 
			// itemPaste
			// 
			this.itemPaste.Enabled = false;
			this.itemPaste.Name = "itemPaste";
			this.itemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.itemPaste.Size = new System.Drawing.Size(242, 24);
			this.itemPaste.Text = "Paste";
			this.itemPaste.Click += new System.EventHandler(this.itemPaste_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(239, 6);
			// 
			// itemDelete
			// 
			this.itemDelete.Enabled = false;
			this.itemDelete.Name = "itemDelete";
			this.itemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.itemDelete.Size = new System.Drawing.Size(242, 24);
			this.itemDelete.Text = "Delete";
			this.itemDelete.Click += new System.EventHandler(this.itemDelete_Click);
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(239, 6);
			// 
			// itemPreferences
			// 
			this.itemPreferences.Name = "itemPreferences";
			this.itemPreferences.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.P)));
			this.itemPreferences.Size = new System.Drawing.Size(242, 24);
			this.itemPreferences.Text = "&Preferences...";
			this.itemPreferences.Click += new System.EventHandler(this.itemPreferences_Click);
			// 
			// popupItemAddContent
			// 
			this.popupItemAddContent.DropDown = this.popupAddContentMenu;
			this.popupItemAddContent.Enabled = false;
			this.popupItemAddContent.Name = "popupItemAddContent";
			this.popupItemAddContent.Size = new System.Drawing.Size(211, 24);
			this.popupItemAddContent.Text = "Add Content";
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
			this.tabDocumentManager.Size = new System.Drawing.Size(234, 679);
			this.tabDocumentManager.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
			this.tabDocumentManager.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
			this.tabDocumentManager.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
			this.tabDocumentManager.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.White;
			this.tabDocumentManager.TabGradient.TabPageTextColor = System.Drawing.Color.White;
			this.tabDocumentManager.TabHOffset = -1;
			this.tabDocumentManager.TabIndex = 0;
			this.tabDocumentManager.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
			this.tabDocumentManager.SelectedIndexChanged += new System.EventHandler(this.tabDocumentManager_SelectedIndexChanged);
			// 
			// pageItems
			// 
			this.pageItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.pageItems.Controls.Add(this.containerFiles);
			this.pageItems.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageItems.ForeColor = System.Drawing.Color.White;
			this.pageItems.IsClosable = false;
			this.pageItems.Location = new System.Drawing.Point(1, 1);
			this.pageItems.Name = "pageItems";
			this.pageItems.Size = new System.Drawing.Size(232, 644);
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
			this.containerFiles.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 365819531, 3, 365819531);
			this.containerFiles.ContentPanel.Size = new System.Drawing.Size(232, 617);
			this.containerFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerFiles.Location = new System.Drawing.Point(0, 0);
			this.containerFiles.Margin = new System.Windows.Forms.Padding(3, 365819531, 3, 365819531);
			this.containerFiles.Name = "containerFiles";
			this.containerFiles.Size = new System.Drawing.Size(232, 644);
			this.containerFiles.TabIndex = 0;
			this.containerFiles.Text = "toolStripContainer1";
			// 
			// containerFiles.TopToolStripPanel
			// 
			this.containerFiles.TopToolStripPanel.Controls.Add(this.stripContent);
			// 
			// treeFiles
			// 
			this.treeFiles.AllowDrop = true;
			this.treeFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.treeFiles.ContextMenuStrip = this.popupFileSystem;
			this.treeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeFiles.ForeColor = System.Drawing.Color.White;
			this.treeFiles.FullRowSelect = true;
			this.treeFiles.HideSelection = false;
			this.treeFiles.Location = new System.Drawing.Point(0, 0);
			this.treeFiles.Name = "treeFiles";
			this.treeFiles.SelectedNode = null;
			this.treeFiles.ShowLines = false;
			this.treeFiles.Size = new System.Drawing.Size(232, 617);
			this.treeFiles.Sorted = true;
			this.treeFiles.TabIndex = 0;
			this.treeFiles.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeFiles_BeforeLabelEdit);
			this.treeFiles.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeFiles_AfterLabelEdit);
			this.treeFiles.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeFiles_BeforeExpand);
			this.treeFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeFiles_ItemDrag);
			this.treeFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeFiles_AfterSelect);
			this.treeFiles.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeFiles_NodeMouseDoubleClick);
			this.treeFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeFiles_DragDrop);
			this.treeFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeFiles_DragEnter);
			this.treeFiles.DragOver += new System.Windows.Forms.DragEventHandler(this.treeFiles_DragOver);
			this.treeFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeFiles_KeyDown);
			// 
			// popupFileSystem
			// 
			this.popupFileSystem.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.popupFileSystem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.popupItemAddContent,
            this.popupItemEdit,
            this.popupItemCreateFolder,
            this.toolStripSeparator10,
            this.popupItemInclude,
            this.popupItemExclude,
            this.popupItemIncludeAll,
            this.popupItemExcludeAll,
            this.toolStripSeparator4,
            this.popupItemCut,
            this.popupItemCopy,
            this.popupItemPaste,
            this.popupItemDelete,
            this.toolStripSeparator5,
            this.popupItemRename});
			this.popupFileSystem.Name = "popupFileSystem";
			this.popupFileSystem.Size = new System.Drawing.Size(212, 310);
			this.popupFileSystem.Opening += new System.ComponentModel.CancelEventHandler(this.popupFileSystem_Opening);
			// 
			// popupItemEdit
			// 
			this.popupItemEdit.Enabled = false;
			this.popupItemEdit.Name = "popupItemEdit";
			this.popupItemEdit.Size = new System.Drawing.Size(211, 24);
			this.popupItemEdit.Text = "&Edit";
			this.popupItemEdit.Click += new System.EventHandler(this.itemEdit_Click);
			// 
			// popupItemCreateFolder
			// 
			this.popupItemCreateFolder.Enabled = false;
			this.popupItemCreateFolder.Name = "popupItemCreateFolder";
			this.popupItemCreateFolder.Size = new System.Drawing.Size(211, 24);
			this.popupItemCreateFolder.Text = "&Create Folder...";
			this.popupItemCreateFolder.Click += new System.EventHandler(this.itemCreateFolder_Click);
			// 
			// toolStripSeparator10
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(208, 6);
			// 
			// popupItemInclude
			// 
			this.popupItemInclude.Name = "popupItemInclude";
			this.popupItemInclude.Size = new System.Drawing.Size(211, 24);
			this.popupItemInclude.Text = "no local";
			this.popupItemInclude.Click += new System.EventHandler(this.popupItemInclude_Click);
			// 
			// popupItemExclude
			// 
			this.popupItemExclude.Name = "popupItemExclude";
			this.popupItemExclude.Size = new System.Drawing.Size(211, 24);
			this.popupItemExclude.Text = "no local";
			this.popupItemExclude.Click += new System.EventHandler(this.popupItemExclude_Click);
			// 
			// popupItemIncludeAll
			// 
			this.popupItemIncludeAll.Name = "popupItemIncludeAll";
			this.popupItemIncludeAll.Size = new System.Drawing.Size(211, 24);
			this.popupItemIncludeAll.Text = "no localization";
			this.popupItemIncludeAll.Click += new System.EventHandler(this.popupItemIncludeAll_Click);
			// 
			// popupItemExcludeAll
			// 
			this.popupItemExcludeAll.Name = "popupItemExcludeAll";
			this.popupItemExcludeAll.Size = new System.Drawing.Size(211, 24);
			this.popupItemExcludeAll.Text = "no localization";
			this.popupItemExcludeAll.Click += new System.EventHandler(this.popupItemExcludeAll_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(208, 6);
			// 
			// popupItemCut
			// 
			this.popupItemCut.Enabled = false;
			this.popupItemCut.Name = "popupItemCut";
			this.popupItemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.popupItemCut.Size = new System.Drawing.Size(211, 24);
			this.popupItemCut.Text = "C&ut";
			this.popupItemCut.Click += new System.EventHandler(this.itemCut_Click);
			// 
			// popupItemCopy
			// 
			this.popupItemCopy.Enabled = false;
			this.popupItemCopy.Name = "popupItemCopy";
			this.popupItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.popupItemCopy.Size = new System.Drawing.Size(211, 24);
			this.popupItemCopy.Text = "&Copy";
			this.popupItemCopy.Click += new System.EventHandler(this.itemCut_Click);
			// 
			// popupItemPaste
			// 
			this.popupItemPaste.Enabled = false;
			this.popupItemPaste.Name = "popupItemPaste";
			this.popupItemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.popupItemPaste.Size = new System.Drawing.Size(211, 24);
			this.popupItemPaste.Text = "Paste";
			this.popupItemPaste.Click += new System.EventHandler(this.itemPaste_Click);
			// 
			// popupItemDelete
			// 
			this.popupItemDelete.Enabled = false;
			this.popupItemDelete.Name = "popupItemDelete";
			this.popupItemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.popupItemDelete.Size = new System.Drawing.Size(211, 24);
			this.popupItemDelete.Text = "&Delete...";
			this.popupItemDelete.Click += new System.EventHandler(this.itemDelete_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(208, 6);
			// 
			// popupItemRename
			// 
			this.popupItemRename.Enabled = false;
			this.popupItemRename.Name = "popupItemRename";
			this.popupItemRename.ShortcutKeys = System.Windows.Forms.Keys.F2;
			this.popupItemRename.Size = new System.Drawing.Size(211, 24);
			this.popupItemRename.Text = "&Rename Folder...";
			this.popupItemRename.Click += new System.EventHandler(this.itemRenameFolder_Click);
			// 
			// stripContent
			// 
			this.stripContent.Dock = System.Windows.Forms.DockStyle.None;
			this.stripContent.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripContent.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.stripContent.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dropNewContent,
            this.toolStripButton1,
            this.buttonEditContent,
            this.buttonDeleteContent,
            this.toolStripSeparator9,
            this.buttonShowAll});
			this.stripContent.Location = new System.Drawing.Point(0, 0);
			this.stripContent.Name = "stripContent";
			this.stripContent.Size = new System.Drawing.Size(232, 27);
			this.stripContent.Stretch = true;
			this.stripContent.TabIndex = 0;
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(6, 27);
			// 
			// buttonEditContent
			// 
			this.buttonEditContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditContent.Enabled = false;
			this.buttonEditContent.Image = global::Gorgon.Editor.Properties.Resources.edit_16x16;
			this.buttonEditContent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditContent.Name = "buttonEditContent";
			this.buttonEditContent.Size = new System.Drawing.Size(24, 24);
			this.buttonEditContent.Text = "Edit selected content";
			this.buttonEditContent.Click += new System.EventHandler(this.buttonEditContent_Click);
			// 
			// buttonDeleteContent
			// 
			this.buttonDeleteContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonDeleteContent.Enabled = false;
			this.buttonDeleteContent.Image = global::Gorgon.Editor.Properties.Resources.delete_item_16x16;
			this.buttonDeleteContent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonDeleteContent.Name = "buttonDeleteContent";
			this.buttonDeleteContent.Size = new System.Drawing.Size(24, 24);
			this.buttonDeleteContent.Text = "Delete selected content";
			this.buttonDeleteContent.Click += new System.EventHandler(this.itemDelete_Click);
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(6, 27);
			// 
			// buttonShowAll
			// 
			this.buttonShowAll.CheckOnClick = true;
			this.buttonShowAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonShowAll.Image = global::Gorgon.Editor.Properties.Resources.show_all_16x16;
			this.buttonShowAll.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonShowAll.Name = "buttonShowAll";
			this.buttonShowAll.Size = new System.Drawing.Size(24, 24);
			this.buttonShowAll.Text = "no local";
			this.buttonShowAll.CheckedChanged += new System.EventHandler(this.buttonShowAll_CheckedChanged);
			// 
			// pageProperties
			// 
			this.pageProperties.BackColor = System.Drawing.Color.DimGray;
			this.pageProperties.Controls.Add(this.propertyItem);
			this.pageProperties.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageProperties.ImageIndex = 3;
			this.pageProperties.IsClosable = false;
			this.pageProperties.Location = new System.Drawing.Point(1, 1);
			this.pageProperties.Name = "pageProperties";
			this.pageProperties.Size = new System.Drawing.Size(232, 651);
			this.pageProperties.TabIndex = 0;
			this.pageProperties.Text = "Properties";
			// 
			// propertyItem
			// 
			this.propertyItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.propertyItem.CanShowVisualStyleGlyphs = false;
			this.propertyItem.CategoryForeColor = System.Drawing.Color.White;
			this.propertyItem.CategorySplitterColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(85)))), ((int)(((byte)(90)))));
			this.propertyItem.CommandsActiveLinkColor = System.Drawing.Color.Lavender;
			this.propertyItem.CommandsBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.propertyItem.CommandsDisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.propertyItem.CommandsForeColor = System.Drawing.Color.White;
			this.propertyItem.CommandsLinkColor = System.Drawing.Color.SteelBlue;
			this.propertyItem.ContextMenuStrip = this.popupProperties;
			this.propertyItem.DisabledItemForeColor = System.Drawing.Color.Black;
			this.propertyItem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.propertyItem.HelpBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.propertyItem.HelpBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.propertyItem.HelpForeColor = System.Drawing.Color.White;
			this.propertyItem.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.propertyItem.Location = new System.Drawing.Point(0, 0);
			this.propertyItem.Name = "propertyItem";
			this.propertyItem.SelectedItemWithFocusBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.propertyItem.SelectedItemWithFocusForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.propertyItem.Size = new System.Drawing.Size(232, 651);
			this.propertyItem.TabIndex = 0;
			this.propertyItem.ToolbarVisible = false;
			this.propertyItem.ViewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.propertyItem.ViewBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.propertyItem.ViewForeColor = System.Drawing.Color.White;
			// 
			// popupProperties
			// 
			this.popupProperties.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.popupProperties.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemResetValue});
			this.popupProperties.Name = "popupProperties";
			this.popupProperties.Size = new System.Drawing.Size(176, 56);
			this.popupProperties.Opening += new System.ComponentModel.CancelEventHandler(this.popupProperties_Opening);
			// 
			// itemResetValue
			// 
			this.itemResetValue.Name = "itemResetValue";
			this.itemResetValue.Size = new System.Drawing.Size(175, 24);
			this.itemResetValue.Text = "&Reset Value";
			this.itemResetValue.Click += new System.EventHandler(this.itemResetValue_Click);
			// 
			// panelEditor
			// 
			this.panelEditor.Controls.Add(this.splitPanelContainer);
			this.panelEditor.Controls.Add(this.menuMain);
			this.panelEditor.Controls.Add(this.stripStatus);
			this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelEditor.Location = new System.Drawing.Point(4, 36);
			this.panelEditor.Name = "panelEditor";
			this.panelEditor.Size = new System.Drawing.Size(1084, 729);
			this.panelEditor.TabIndex = 4;
			// 
			// splitPanelContainer
			// 
			this.splitPanelContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.splitPanelContainer.Controls.Add(this.splitPanel1);
			this.splitPanelContainer.Controls.Add(this.splitEditor);
			this.splitPanelContainer.Controls.Add(this.splitPanel2);
			this.splitPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitPanelContainer.Location = new System.Drawing.Point(0, 28);
			this.splitPanelContainer.Name = "splitPanelContainer";
			this.splitPanelContainer.Size = new System.Drawing.Size(1084, 679);
			this.splitPanelContainer.TabIndex = 2;
			// 
			// splitPanel1
			// 
			this.splitPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitPanel1.Location = new System.Drawing.Point(0, 0);
			this.splitPanel1.Name = "splitPanel1";
			this.splitPanel1.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
			this.splitPanel1.Size = new System.Drawing.Size(846, 679);
			this.splitPanel1.TabIndex = 4;
			// 
			// splitEditor
			// 
			this.splitEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.splitEditor.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitEditor.Location = new System.Drawing.Point(846, 0);
			this.splitEditor.MinExtra = 640;
			this.splitEditor.MinSize = 4;
			this.splitEditor.Name = "splitEditor";
			this.splitEditor.Size = new System.Drawing.Size(4, 679);
			this.splitEditor.TabIndex = 2;
			this.splitEditor.TabStop = false;
			// 
			// splitPanel2
			// 
			this.splitPanel2.Controls.Add(this.tabDocumentManager);
			this.splitPanel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitPanel2.Location = new System.Drawing.Point(850, 0);
			this.splitPanel2.Name = "splitPanel2";
			this.splitPanel2.Size = new System.Drawing.Size(234, 679);
			this.splitPanel2.TabIndex = 3;
			// 
			// stripStatus
			// 
			this.stripStatus.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.stripStatus.Location = new System.Drawing.Point(0, 707);
			this.stripStatus.Name = "stripStatus";
			this.stripStatus.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.stripStatus.Size = new System.Drawing.Size(1084, 22);
			this.stripStatus.SizingGrip = false;
			this.stripStatus.TabIndex = 0;
			// 
			// dialogOpenFile
			// 
			this.dialogOpenFile.Title = "Select an editor file...";
			// 
			// dialogSaveFile
			// 
			this.dialogSaveFile.Title = "Save Gorgon  editor file as...";
			// 
			// dialogImport
			// 
			this.dialogImport.Multiselect = true;
			this.dialogImport.Title = "Import";
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ShowBorder = true;
			this.ClientSize = new System.Drawing.Size(1092, 769);
			this.Controls.Add(this.panelEditor);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(128, 32);
			this.Name = "FormMain";
			this.Padding = new System.Windows.Forms.Padding(4);
			this.ResizeHandleSize = 4;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Gorgon Editor";
			this.Theme.ForeColor = System.Drawing.Color.Silver;
			this.Controls.SetChildIndex(this.ContentArea, 0);
			this.Controls.SetChildIndex(this.panelEditor, 0);
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
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
			this.splitPanelContainer.ResumeLayout(false);
			this.splitPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private PropertyGrid propertyItem;
		private MenuStrip menuMain;
		private ContextMenuStrip popupProperties;
		private ToolStripMenuItem itemResetValue;
		private ToolStripMenuItem menuFile;
		private ToolStripMenuItem itemNew;
		private ToolStripMenuItem itemOpen;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem itemSave;
		private ToolStripMenuItem itemSaveAs;
		private ToolStripSeparator toolStripMenuItem5;
		private ToolStripMenuItem itemExit;
		private KRBTabControl.KRBTabControl tabDocumentManager;
		private KRBTabControl.TabPageEx pageProperties;
		private KRBTabControl.TabPageEx pageItems;
		private ToolStripContainer containerFiles;
		private ToolStripMenuItem itemImport;
		private ToolStripMenuItem itemExport;
        private ToolStripSeparator toolStripSeparator2;
        private Panel panelEditor;
		private StatusStrip stripStatus;
		private ToolStripMenuItem itemAddContent;
		private ToolStripSeparator toolStripSeparator3;
		private ContextMenuStrip popupFileSystem;
		private ToolStripMenuItem popupItemAddContent;
		private ToolStrip stripContent;
		private ToolStripDropDownButton dropNewContent;
		private ToolStripSeparator toolStripButton1;
		private ToolStripButton buttonEditContent;
		private ToolStripButton buttonDeleteContent;
		private ContextMenuStrip popupAddContentMenu;
		private ToolStripMenuItem popupItemEdit;
        private ToolStripMenuItem popupItemDelete;
        private ToolStripMenuItem popupItemCreateFolder;
		private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem popupItemRename;
		private Panel splitPanelContainer;
		private Panel splitPanel1;
		private Splitter splitEditor;
		private Panel splitPanel2;
		private EditorTreeView treeFiles;
        private OpenFileDialog dialogOpenFile;
		private SaveFileDialog dialogSaveFile;
        private ToolStripMenuItem menuEdit;
        private ToolStripMenuItem itemCut;
        private ToolStripMenuItem itemCopy;
        private ToolStripMenuItem itemPaste;
        private ToolStripMenuItem popupItemCut;
        private ToolStripMenuItem popupItemCopy;
        private ToolStripMenuItem popupItemPaste;
        private ToolStripSeparator toolStripSeparator5;
		private ToolStripMenuItem itemDelete;
        private OpenFileDialog dialogImport;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem itemPreferences;
        private ToolStripMenuItem menuRecentFiles;
		private ToolStripSeparator toolStripSeparator8;
        private FolderBrowserDialog dialogExport;
		private ToolStripMenuItem itemCreateFolder;
		private ToolStripSeparator toolStripSeparator9;
		private ToolStripButton buttonShowAll;
		private ToolStripSeparator toolStripSeparator10;
		private ToolStripMenuItem popupItemInclude;
		private ToolStripMenuItem popupItemExclude;
        private ToolStripMenuItem popupItemIncludeAll;
		private ToolStripMenuItem popupItemExcludeAll;
	}
}

