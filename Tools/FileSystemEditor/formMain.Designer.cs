#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Sunday, April 01, 2007 12:06:01 PM
// 
#endregion

namespace GorgonLibrary.FileSystems.Tools
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
			this.menuItemFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemFileSystems = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemWindow = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTileHorizontal = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTileVertical = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemCascade = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemArrangeIcons = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.statusMain = new System.Windows.Forms.StatusStrip();
			this.labelStats = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelFileName = new System.Windows.Forms.ToolStripStatusLabel();
			this.progressFileImport = new System.Windows.Forms.ToolStripProgressBar();
			this.iconImages16x16 = new System.Windows.Forms.ImageList(this.components);
			this.iconImages32x32 = new System.Windows.Forms.ImageList(this.components);
			this.dialogFileSave = new System.Windows.Forms.SaveFileDialog();
			this.dialogFolderSave = new System.Windows.Forms.FolderBrowserDialog();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.buttonNewFS = new System.Windows.Forms.ToolStripButton();
			this.buttonOpenFS = new System.Windows.Forms.ToolStripButton();
			this.buttonSaveFS = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton4 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEditFS = new System.Windows.Forms.ToolStripButton();
			this.dialogOpenFileSystem = new System.Windows.Forms.OpenFileDialog();
			this.menuMain.SuspendLayout();
			this.statusMain.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuMain
			// 
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemFile,
            this.menuItemWindow,
            this.menuItemHelp});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.MdiWindowListItem = this.menuItemWindow;
			this.menuMain.Name = "menuMain";
			this.menuMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.menuMain.Size = new System.Drawing.Size(685, 24);
			this.menuMain.TabIndex = 3;
			// 
			// menuItemFile
			// 
			this.menuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNew,
            this.menuItemOpen,
            this.menuItemSave,
            this.menuItemSaveAs,
            this.toolStripSeparator1,
            this.menuItemFileSystems,
            this.toolStripSeparator2,
            this.menuItemExit});
			this.menuItemFile.Name = "menuItemFile";
			this.menuItemFile.Size = new System.Drawing.Size(37, 20);
			this.menuItemFile.Text = "&File";
			// 
			// menuItemNew
			// 
			this.menuItemNew.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.document_plain_new;
			this.menuItemNew.Name = "menuItemNew";
			this.menuItemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.menuItemNew.Size = new System.Drawing.Size(187, 22);
			this.menuItemNew.Text = "&New...";
			this.menuItemNew.Click += new System.EventHandler(this.menuItemNew_Click);
			// 
			// menuItemOpen
			// 
			this.menuItemOpen.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_out;
			this.menuItemOpen.Name = "menuItemOpen";
			this.menuItemOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.menuItemOpen.Size = new System.Drawing.Size(187, 22);
			this.menuItemOpen.Text = "&Open...";
			this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);
			// 
			// menuItemSave
			// 
			this.menuItemSave.Enabled = false;
			this.menuItemSave.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.disk_blue;
			this.menuItemSave.Name = "menuItemSave";
			this.menuItemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.menuItemSave.Size = new System.Drawing.Size(187, 22);
			this.menuItemSave.Text = "&Save";
			this.menuItemSave.Click += new System.EventHandler(this.menuItemSave_Click);
			// 
			// menuItemSaveAs
			// 
			this.menuItemSaveAs.Enabled = false;
			this.menuItemSaveAs.Name = "menuItemSaveAs";
			this.menuItemSaveAs.Size = new System.Drawing.Size(187, 22);
			this.menuItemSaveAs.Text = "Save &As...";
			this.menuItemSaveAs.Click += new System.EventHandler(this.menuItemSaveAs_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
			// 
			// menuItemFileSystems
			// 
			this.menuItemFileSystems.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_cubes;
			this.menuItemFileSystems.Name = "menuItemFileSystems";
			this.menuItemFileSystems.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
			this.menuItemFileSystems.Size = new System.Drawing.Size(187, 22);
			this.menuItemFileSystems.Text = "File Systems...";
			this.menuItemFileSystems.Click += new System.EventHandler(this.menuItemFileSystems_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(184, 6);
			// 
			// menuItemExit
			// 
			this.menuItemExit.Name = "menuItemExit";
			this.menuItemExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.menuItemExit.Size = new System.Drawing.Size(187, 22);
			this.menuItemExit.Text = "E&xit";
			this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
			// 
			// menuItemWindow
			// 
			this.menuItemWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemTileHorizontal,
            this.menuItemTileVertical,
            this.menuItemCascade,
            this.menuItemArrangeIcons,
            this.toolStripMenuItem1});
			this.menuItemWindow.Name = "menuItemWindow";
			this.menuItemWindow.Size = new System.Drawing.Size(63, 20);
			this.menuItemWindow.Text = "Window";
			// 
			// menuItemTileHorizontal
			// 
			this.menuItemTileHorizontal.Name = "menuItemTileHorizontal";
			this.menuItemTileHorizontal.Size = new System.Drawing.Size(212, 22);
			this.menuItemTileHorizontal.Text = "Tile Windows Horizontally";
			this.menuItemTileHorizontal.Click += new System.EventHandler(this.menuItemTileHorizontal_Click);
			// 
			// menuItemTileVertical
			// 
			this.menuItemTileVertical.Name = "menuItemTileVertical";
			this.menuItemTileVertical.Size = new System.Drawing.Size(212, 22);
			this.menuItemTileVertical.Text = "Tile Windows Vertically";
			this.menuItemTileVertical.Click += new System.EventHandler(this.menuItemTileVertical_Click);
			// 
			// menuItemCascade
			// 
			this.menuItemCascade.Name = "menuItemCascade";
			this.menuItemCascade.Size = new System.Drawing.Size(212, 22);
			this.menuItemCascade.Text = "Cascade Windows";
			this.menuItemCascade.Click += new System.EventHandler(this.menuItemCascade_Click);
			// 
			// menuItemArrangeIcons
			// 
			this.menuItemArrangeIcons.Name = "menuItemArrangeIcons";
			this.menuItemArrangeIcons.Size = new System.Drawing.Size(212, 22);
			this.menuItemArrangeIcons.Text = "Arrange Icons";
			this.menuItemArrangeIcons.Click += new System.EventHandler(this.menuItemArrangeIcons_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(209, 6);
			// 
			// menuItemHelp
			// 
			this.menuItemHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAbout});
			this.menuItemHelp.Name = "menuItemHelp";
			this.menuItemHelp.Size = new System.Drawing.Size(44, 20);
			this.menuItemHelp.Text = "&Help";
			// 
			// menuItemAbout
			// 
			this.menuItemAbout.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.GorFS_16;
			this.menuItemAbout.Name = "menuItemAbout";
			this.menuItemAbout.Size = new System.Drawing.Size(116, 22);
			this.menuItemAbout.Text = "&About...";
			this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
			// 
			// statusMain
			// 
			this.statusMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStats,
            this.labelFileName,
            this.progressFileImport});
			this.statusMain.Location = new System.Drawing.Point(0, 498);
			this.statusMain.Name = "statusMain";
			this.statusMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.statusMain.Size = new System.Drawing.Size(685, 22);
			this.statusMain.TabIndex = 5;
			// 
			// labelStats
			// 
			this.labelStats.Name = "labelStats";
			this.labelStats.Size = new System.Drawing.Size(348, 17);
			this.labelStats.Spring = true;
			this.labelStats.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelStats.Visible = false;
			// 
			// labelFileName
			// 
			this.labelFileName.AutoSize = false;
			this.labelFileName.BackColor = System.Drawing.Color.Transparent;
			this.labelFileName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.labelFileName.Name = "labelFileName";
			this.labelFileName.Size = new System.Drawing.Size(200, 17);
			this.labelFileName.Text = "File:";
			this.labelFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelFileName.Visible = false;
			// 
			// progressFileImport
			// 
			this.progressFileImport.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.progressFileImport.Name = "progressFileImport";
			this.progressFileImport.Size = new System.Drawing.Size(120, 16);
			this.progressFileImport.Visible = false;
			// 
			// iconImages16x16
			// 
			this.iconImages16x16.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.iconImages16x16.ImageSize = new System.Drawing.Size(16, 16);
			this.iconImages16x16.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// iconImages32x32
			// 
			this.iconImages32x32.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.iconImages32x32.ImageSize = new System.Drawing.Size(32, 32);
			this.iconImages32x32.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// dialogFileSave
			// 
			this.dialogFileSave.DefaultExt = "gorPack";
			this.dialogFileSave.Filter = "Gorgon pack file (*.gorPack)|*.gorPack|All files (*.*)|*.*";
			this.dialogFileSave.InitialDirectory = ".\\";
			this.dialogFileSave.Title = "Save file as";
			// 
			// dialogFolderSave
			// 
			this.dialogFolderSave.Description = "Select a folder to save the file system into.";
			this.dialogFolderSave.SelectedPath = ".\\";
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNewFS,
            this.buttonOpenFS,
            this.buttonSaveFS,
            this.toolStripButton4,
            this.buttonEditFS});
			this.toolStrip1.Location = new System.Drawing.Point(0, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(685, 25);
			this.toolStrip1.Stretch = true;
			this.toolStrip1.TabIndex = 7;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// buttonNewFS
			// 
			this.buttonNewFS.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNewFS.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.document_plain_new;
			this.buttonNewFS.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNewFS.Name = "buttonNewFS";
			this.buttonNewFS.Size = new System.Drawing.Size(23, 22);
			this.buttonNewFS.Text = "Create a new file system.";
			this.buttonNewFS.Click += new System.EventHandler(this.menuItemNew_Click);
			// 
			// buttonOpenFS
			// 
			this.buttonOpenFS.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonOpenFS.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_out;
			this.buttonOpenFS.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonOpenFS.Name = "buttonOpenFS";
			this.buttonOpenFS.Size = new System.Drawing.Size(23, 22);
			this.buttonOpenFS.Text = "Open a file system.";
			this.buttonOpenFS.Click += new System.EventHandler(this.menuItemOpen_Click);
			// 
			// buttonSaveFS
			// 
			this.buttonSaveFS.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSaveFS.Enabled = false;
			this.buttonSaveFS.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.disk_blue;
			this.buttonSaveFS.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSaveFS.Name = "buttonSaveFS";
			this.buttonSaveFS.Size = new System.Drawing.Size(23, 22);
			this.buttonSaveFS.Text = "Save the file system.";
			this.buttonSaveFS.Click += new System.EventHandler(this.buttonSaveFS_Click);
			// 
			// toolStripButton4
			// 
			this.toolStripButton4.Name = "toolStripButton4";
			this.toolStripButton4.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonEditFS
			// 
			this.buttonEditFS.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditFS.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_cubes;
			this.buttonEditFS.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditFS.Name = "buttonEditFS";
			this.buttonEditFS.Size = new System.Drawing.Size(23, 22);
			this.buttonEditFS.Text = "Load file system plug-in(s).";
			this.buttonEditFS.Click += new System.EventHandler(this.menuItemFileSystems_Click);
			// 
			// dialogOpenFileSystem
			// 
			this.dialogOpenFileSystem.DefaultExt = "gorPack";
			this.dialogOpenFileSystem.Filter = "Gorgon packed file systems (*.gorPack)|*.gorPack|Gorgon folder file system.|heade" +
				"r.folderSystem";
			this.dialogOpenFileSystem.InitialDirectory = ".\\";
			this.dialogOpenFileSystem.Multiselect = true;
			this.dialogOpenFileSystem.Title = "Open a file system...";
			// 
			// formMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(245)))), ((int)(((byte)(222)))));
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(685, 520);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.statusMain);
			this.Controls.Add(this.menuMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.MainMenuStrip = this.menuMain;
			this.Name = "formMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "File System Editor";
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.statusMain.ResumeLayout(false);
			this.statusMain.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuMain;
		private System.Windows.Forms.ToolStripMenuItem menuItemFile;
		private System.Windows.Forms.ToolStripMenuItem menuItemNew;
		private System.Windows.Forms.ToolStripMenuItem menuItemOpen;
		private System.Windows.Forms.ToolStripMenuItem menuItemSave;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem menuItemExit;
		private System.Windows.Forms.StatusStrip statusMain;
		private System.Windows.Forms.ToolStripMenuItem menuItemHelp;
		private System.Windows.Forms.ToolStripMenuItem menuItemAbout;
		private System.Windows.Forms.ToolStripMenuItem menuItemWindow;
		private System.Windows.Forms.ToolStripMenuItem menuItemTileHorizontal;
		private System.Windows.Forms.ToolStripMenuItem menuItemCascade;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuItemTileVertical;
		private System.Windows.Forms.ToolStripMenuItem menuItemArrangeIcons;
		private System.Windows.Forms.ToolStripMenuItem menuItemFileSystems;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		internal System.Windows.Forms.ImageList iconImages16x16;
		internal System.Windows.Forms.ImageList iconImages32x32;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveAs;
		private System.Windows.Forms.ToolStripStatusLabel labelFileName;
		private System.Windows.Forms.ToolStripProgressBar progressFileImport;
		private System.Windows.Forms.ToolStripStatusLabel labelStats;
		private System.Windows.Forms.SaveFileDialog dialogFileSave;
		private System.Windows.Forms.FolderBrowserDialog dialogFolderSave;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton buttonNewFS;
		private System.Windows.Forms.ToolStripButton buttonOpenFS;
		private System.Windows.Forms.ToolStripButton buttonSaveFS;
		private System.Windows.Forms.ToolStripSeparator toolStripButton4;
		private System.Windows.Forms.ToolStripButton buttonEditFS;
		private System.Windows.Forms.OpenFileDialog dialogOpenFileSystem;
	}
}

