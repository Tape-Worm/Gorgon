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
// Created: Friday, May 25, 2012 9:55:00 AM
// 
#endregion

namespace GorgonLibrary.UI
{
	partial class formGorgonFileDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formGorgonFileDialog));
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.treeDirectories = new System.Windows.Forms.TreeView();
			this.imagesTree = new System.Windows.Forms.ImageList(this.components);
			this.listFiles = new System.Windows.Forms.ListView();
			this.columnFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imagesLargeIcons = new System.Windows.Forms.ImageList(this.components);
			this.stripContainer = new System.Windows.Forms.ToolStripContainer();
			this.statusMain = new System.Windows.Forms.StatusStrip();
			this.panel1 = new System.Windows.Forms.Panel();
			this.comboExtension = new System.Windows.Forms.ComboBox();
			this.textFileName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOpen = new System.Windows.Forms.Button();
			this.stripMenu = new System.Windows.Forms.ToolStrip();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemDetails = new System.Windows.Forms.ToolStripMenuItem();
			this.itemList = new System.Windows.Forms.ToolStripMenuItem();
			this.itemIcons = new System.Windows.Forms.ToolStripMenuItem();
			this.menuFile = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemNewFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonUp = new System.Windows.Forms.ToolStripButton();
			this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.stripContainer.BottomToolStripPanel.SuspendLayout();
			this.stripContainer.ContentPanel.SuspendLayout();
			this.stripContainer.TopToolStripPanel.SuspendLayout();
			this.stripContainer.SuspendLayout();
			this.panel1.SuspendLayout();
			this.stripMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitMain
			// 
			this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitMain.Location = new System.Drawing.Point(0, 0);
			this.splitMain.Name = "splitMain";
			// 
			// splitMain.Panel1
			// 
			this.splitMain.Panel1.Controls.Add(this.treeDirectories);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.Controls.Add(this.listFiles);
			this.splitMain.Size = new System.Drawing.Size(682, 305);
			this.splitMain.SplitterDistance = 151;
			this.splitMain.SplitterWidth = 2;
			this.splitMain.TabIndex = 0;
			// 
			// treeDirectories
			// 
			this.treeDirectories.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeDirectories.FullRowSelect = true;
			this.treeDirectories.HideSelection = false;
			this.treeDirectories.ImageIndex = 0;
			this.treeDirectories.ImageList = this.imagesTree;
			this.treeDirectories.Location = new System.Drawing.Point(0, 0);
			this.treeDirectories.Name = "treeDirectories";
			this.treeDirectories.SelectedImageIndex = 0;
			this.treeDirectories.ShowLines = false;
			this.treeDirectories.ShowNodeToolTips = true;
			this.treeDirectories.ShowRootLines = false;
			this.treeDirectories.Size = new System.Drawing.Size(151, 305);
			this.treeDirectories.TabIndex = 0;
			this.treeDirectories.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeDirectories_BeforeCollapse);
			this.treeDirectories.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeDirectories_BeforeExpand);
			this.treeDirectories.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeDirectories_BeforeSelect);
			this.treeDirectories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeDirectories_AfterSelect);
			// 
			// imagesTree
			// 
			this.imagesTree.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imagesTree.ImageSize = new System.Drawing.Size(16, 16);
			this.imagesTree.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// listFiles
			// 
			this.listFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFileName,
            this.columnSize,
            this.columnDate,
            this.columnType});
			this.listFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFiles.FullRowSelect = true;
			this.listFiles.LabelEdit = true;
			this.listFiles.LargeImageList = this.imagesLargeIcons;
			this.listFiles.Location = new System.Drawing.Point(0, 0);
			this.listFiles.Name = "listFiles";
			this.listFiles.Size = new System.Drawing.Size(529, 305);
			this.listFiles.SmallImageList = this.imagesTree;
			this.listFiles.TabIndex = 1;
			this.listFiles.UseCompatibleStateImageBehavior = false;
			this.listFiles.View = System.Windows.Forms.View.Details;
			this.listFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listFiles_ColumnClick);
			this.listFiles.DoubleClick += new System.EventHandler(this.listFiles_DoubleClick);
			this.listFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listFiles_KeyDown);
			// 
			// columnFileName
			// 
			this.columnFileName.Text = "File name";
			// 
			// columnSize
			// 
			this.columnSize.Text = "Size";
			// 
			// columnDate
			// 
			this.columnDate.Text = "Date Modified";
			// 
			// columnType
			// 
			this.columnType.Text = "Type";
			// 
			// imagesLargeIcons
			// 
			this.imagesLargeIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imagesLargeIcons.ImageSize = new System.Drawing.Size(32, 32);
			this.imagesLargeIcons.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// stripContainer
			// 
			// 
			// stripContainer.BottomToolStripPanel
			// 
			this.stripContainer.BottomToolStripPanel.Controls.Add(this.statusMain);
			// 
			// stripContainer.ContentPanel
			// 
			this.stripContainer.ContentPanel.Controls.Add(this.splitMain);
			this.stripContainer.ContentPanel.Controls.Add(this.panel1);
			this.stripContainer.ContentPanel.Size = new System.Drawing.Size(682, 368);
			this.stripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stripContainer.Location = new System.Drawing.Point(0, 0);
			this.stripContainer.Name = "stripContainer";
			this.stripContainer.Size = new System.Drawing.Size(682, 415);
			this.stripContainer.TabIndex = 0;
			this.stripContainer.Text = "toolStripContainer1";
			// 
			// stripContainer.TopToolStripPanel
			// 
			this.stripContainer.TopToolStripPanel.Controls.Add(this.stripMenu);
			// 
			// statusMain
			// 
			this.statusMain.Dock = System.Windows.Forms.DockStyle.None;
			this.statusMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.statusMain.Location = new System.Drawing.Point(0, 0);
			this.statusMain.Name = "statusMain";
			this.statusMain.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			this.statusMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.statusMain.Size = new System.Drawing.Size(682, 22);
			this.statusMain.TabIndex = 2;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.comboExtension);
			this.panel1.Controls.Add(this.textFileName);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Controls.Add(this.buttonOpen);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 305);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(682, 63);
			this.panel1.TabIndex = 2;
			// 
			// comboExtension
			// 
			this.comboExtension.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboExtension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboExtension.FormattingEnabled = true;
			this.comboExtension.Location = new System.Drawing.Point(486, 7);
			this.comboExtension.Name = "comboExtension";
			this.comboExtension.Size = new System.Drawing.Size(182, 23);
			this.comboExtension.TabIndex = 6;
			// 
			// textFileName
			// 
			this.textFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textFileName.Location = new System.Drawing.Point(191, 7);
			this.textFileName.Name = "textFileName";
			this.textFileName.Size = new System.Drawing.Size(289, 23);
			this.textFileName.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(124, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 15);
			this.label1.TabIndex = 4;
			this.label1.Text = "File name:";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = ((System.Drawing.Image)(resources.GetObject("buttonCancel.Image")));
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(581, 33);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOpen
			// 
			this.buttonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOpen.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOpen.Enabled = false;
			this.buttonOpen.Image = ((System.Drawing.Image)(resources.GetObject("buttonOpen.Image")));
			this.buttonOpen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOpen.Location = new System.Drawing.Point(488, 33);
			this.buttonOpen.Name = "buttonOpen";
			this.buttonOpen.Size = new System.Drawing.Size(87, 27);
			this.buttonOpen.TabIndex = 2;
			this.buttonOpen.Text = "&Open";
			this.buttonOpen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOpen.UseVisualStyleBackColor = true;
			// 
			// stripMenu
			// 
			this.stripMenu.Dock = System.Windows.Forms.DockStyle.None;
			this.stripMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton2,
            this.menuFile,
            this.buttonUp,
            this.buttonRefresh});
			this.stripMenu.Location = new System.Drawing.Point(0, 0);
			this.stripMenu.Name = "stripMenu";
			this.stripMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.stripMenu.Size = new System.Drawing.Size(682, 25);
			this.stripMenu.Stretch = true;
			this.stripMenu.TabIndex = 0;
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemDetails,
            this.itemList,
            this.itemIcons});
			this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(29, 22);
			this.toolStripButton2.Text = "Change View";
			// 
			// itemDetails
			// 
			this.itemDetails.Checked = true;
			this.itemDetails.CheckOnClick = true;
			this.itemDetails.CheckState = System.Windows.Forms.CheckState.Checked;
			this.itemDetails.Name = "itemDetails";
			this.itemDetails.Size = new System.Drawing.Size(109, 22);
			this.itemDetails.Text = "&Details";
			this.itemDetails.Click += new System.EventHandler(this.itemDetails_Click);
			// 
			// itemList
			// 
			this.itemList.CheckOnClick = true;
			this.itemList.Name = "itemList";
			this.itemList.Size = new System.Drawing.Size(109, 22);
			this.itemList.Text = "&List";
			this.itemList.Click += new System.EventHandler(this.itemDetails_Click);
			// 
			// itemIcons
			// 
			this.itemIcons.CheckOnClick = true;
			this.itemIcons.Name = "itemIcons";
			this.itemIcons.Size = new System.Drawing.Size(109, 22);
			this.itemIcons.Text = "&Icons";
			this.itemIcons.Click += new System.EventHandler(this.itemDetails_Click);
			// 
			// menuFile
			// 
			this.menuFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNewFolder,
            this.toolStripSeparator1,
            this.deleteFolderToolStripMenuItem,
            this.renameToolStripMenuItem});
			this.menuFile.Image = ((System.Drawing.Image)(resources.GetObject("menuFile.Image")));
			this.menuFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.menuFile.Name = "menuFile";
			this.menuFile.Size = new System.Drawing.Size(38, 22);
			this.menuFile.Text = "&File";
			// 
			// itemNewFolder
			// 
			this.itemNewFolder.Enabled = false;
			this.itemNewFolder.Name = "itemNewFolder";
			this.itemNewFolder.Size = new System.Drawing.Size(143, 22);
			this.itemNewFolder.Text = "&New Folder...";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(140, 6);
			// 
			// deleteFolderToolStripMenuItem
			// 
			this.deleteFolderToolStripMenuItem.Enabled = false;
			this.deleteFolderToolStripMenuItem.Name = "deleteFolderToolStripMenuItem";
			this.deleteFolderToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
			this.deleteFolderToolStripMenuItem.Text = "&Delete...";
			// 
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Enabled = false;
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
			this.renameToolStripMenuItem.Text = "Rename...";
			// 
			// buttonUp
			// 
			this.buttonUp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.buttonUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonUp.Enabled = false;
			this.buttonUp.Image = global::GorgonLibrary.FileSystem.Properties.Resources.up_directory_16x16;
			this.buttonUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonUp.Name = "buttonUp";
			this.buttonUp.Size = new System.Drawing.Size(23, 22);
			this.buttonUp.Text = "Go up one directory.";
			this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.buttonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRefresh.Image = global::GorgonLibrary.FileSystem.Properties.Resources.Refresh_16x16;
			this.buttonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.Size = new System.Drawing.Size(23, 22);
			this.buttonRefresh.Text = "Refresh the selected directory.";
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
			// 
			// formGorgonFileDialog
			// 
			this.AcceptButton = this.buttonOpen;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(682, 415);
			this.Controls.Add(this.stripContainer);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formGorgonFileDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Open File";
			this.splitMain.Panel1.ResumeLayout(false);
			this.splitMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
			this.splitMain.ResumeLayout(false);
			this.stripContainer.BottomToolStripPanel.ResumeLayout(false);
			this.stripContainer.BottomToolStripPanel.PerformLayout();
			this.stripContainer.ContentPanel.ResumeLayout(false);
			this.stripContainer.TopToolStripPanel.ResumeLayout(false);
			this.stripContainer.TopToolStripPanel.PerformLayout();
			this.stripContainer.ResumeLayout(false);
			this.stripContainer.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.stripMenu.ResumeLayout(false);
			this.stripMenu.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitMain;
		private System.Windows.Forms.TreeView treeDirectories;
		private System.Windows.Forms.ToolStripContainer stripContainer;
		private System.Windows.Forms.ToolStrip stripMenu;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOpen;
		private System.Windows.Forms.ComboBox comboExtension;
		private System.Windows.Forms.TextBox textFileName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView listFiles;
		private System.Windows.Forms.StatusStrip statusMain;
		private System.Windows.Forms.ToolStripDropDownButton toolStripButton2;
		private System.Windows.Forms.ToolStripMenuItem itemDetails;
		private System.Windows.Forms.ToolStripMenuItem itemList;
		private System.Windows.Forms.ToolStripMenuItem itemIcons;
		private System.Windows.Forms.ColumnHeader columnFileName;
		private System.Windows.Forms.ColumnHeader columnSize;
		private System.Windows.Forms.ColumnHeader columnDate;
		private System.Windows.Forms.ImageList imagesTree;
		private System.Windows.Forms.ToolStripDropDownButton menuFile;
		private System.Windows.Forms.ToolStripMenuItem itemNewFolder;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem deleteFolderToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ImageList imagesLargeIcons;
		private System.Windows.Forms.ColumnHeader columnType;
		private System.Windows.Forms.ToolStripButton buttonUp;
		private System.Windows.Forms.ToolStripButton buttonRefresh;
	}
}