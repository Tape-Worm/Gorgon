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
// Created: Monday, May 07, 2007 4:53:23 PM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics.Tools
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

			if (disposing)
			{
				if (_spriteManager != null)
					_spriteManager.Docking -= new EventHandler(_spriteManager_Docking);
			}

			_spriteManager = null;
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
			this.containerMain = new System.Windows.Forms.ToolStripContainer();
			this.stripMain = new System.Windows.Forms.StatusStrip();
			this.labelName = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelSpriteStart = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelX = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelSpriteSize = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelMiscInfo = new System.Windows.Forms.ToolStripStatusLabel();
			this.panelMain = new System.Windows.Forms.Panel();
			this.scrollHorizontal = new System.Windows.Forms.HScrollBar();
			this.panelGorgon = new System.Windows.Forms.Panel();
			this.panelVScroll = new System.Windows.Forms.Panel();
			this.scrollVertical = new System.Windows.Forms.VScrollBar();
			this.splitRenderTargetManager = new System.Windows.Forms.Splitter();
			this.panelRenderTargetManager = new System.Windows.Forms.Panel();
			this.splitSpriteManager = new System.Windows.Forms.Splitter();
			this.panelSpriteManager = new System.Windows.Forms.Panel();
			this.splitImageManager = new System.Windows.Forms.Splitter();
			this.panelImageManager = new System.Windows.Forms.Panel();
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemNewProject = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemOpenProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemSaveProject = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSaveProjectAs = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSaveAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemSaveSprite = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSaveSpriteAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemPrefs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTools = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemImageManager = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTargetManager = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSpriteManager = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.stripSpriteClip = new System.Windows.Forms.ToolStrip();
			this.buttonKeyboardMode = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonFixedSize = new System.Windows.Forms.ToolStripButton();
			this.textFixedWidth = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.textFixedHeight = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonFollowCursor = new System.Windows.Forms.ToolStripButton();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.textWindowSize = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonBackgroundColor = new System.Windows.Forms.ToolStripButton();
			this.dialogOpen = new System.Windows.Forms.OpenFileDialog();
			this.dialogSave = new System.Windows.Forms.SaveFileDialog();
			this.containerMain.BottomToolStripPanel.SuspendLayout();
			this.containerMain.ContentPanel.SuspendLayout();
			this.containerMain.TopToolStripPanel.SuspendLayout();
			this.containerMain.SuspendLayout();
			this.stripMain.SuspendLayout();
			this.panelMain.SuspendLayout();
			this.panelVScroll.SuspendLayout();
			this.menuMain.SuspendLayout();
			this.stripSpriteClip.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerMain
			// 
			// 
			// containerMain.BottomToolStripPanel
			// 
			this.containerMain.BottomToolStripPanel.Controls.Add(this.stripMain);
			// 
			// containerMain.ContentPanel
			// 
			this.containerMain.ContentPanel.Controls.Add(this.panelMain);
			this.containerMain.ContentPanel.Controls.Add(this.splitRenderTargetManager);
			this.containerMain.ContentPanel.Controls.Add(this.panelRenderTargetManager);
			this.containerMain.ContentPanel.Controls.Add(this.splitSpriteManager);
			this.containerMain.ContentPanel.Controls.Add(this.panelSpriteManager);
			this.containerMain.ContentPanel.Controls.Add(this.splitImageManager);
			this.containerMain.ContentPanel.Controls.Add(this.panelImageManager);
			this.containerMain.ContentPanel.Margin = new System.Windows.Forms.Padding(4);
			this.containerMain.ContentPanel.Size = new System.Drawing.Size(843, 485);
			this.containerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerMain.LeftToolStripPanelVisible = false;
			this.containerMain.Location = new System.Drawing.Point(0, 0);
			this.containerMain.Margin = new System.Windows.Forms.Padding(4);
			this.containerMain.Name = "containerMain";
			this.containerMain.RightToolStripPanelVisible = false;
			this.containerMain.Size = new System.Drawing.Size(843, 556);
			this.containerMain.TabIndex = 1;
			this.containerMain.Text = "toolStripContainer1";
			// 
			// containerMain.TopToolStripPanel
			// 
			this.containerMain.TopToolStripPanel.Controls.Add(this.menuMain);
			this.containerMain.TopToolStripPanel.Controls.Add(this.stripSpriteClip);
			// 
			// stripMain
			// 
			this.stripMain.Dock = System.Windows.Forms.DockStyle.None;
			this.stripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelName,
            this.labelSpriteStart,
            this.labelX,
            this.labelSpriteSize,
            this.labelMiscInfo});
			this.stripMain.Location = new System.Drawing.Point(0, 0);
			this.stripMain.Name = "stripMain";
			this.stripMain.Size = new System.Drawing.Size(843, 22);
			this.stripMain.TabIndex = 0;
			// 
			// labelName
			// 
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(73, 17);
			this.labelName.Text = "Sprite name:";
			this.labelName.Visible = false;
			// 
			// labelSpriteStart
			// 
			this.labelSpriteStart.Name = "labelSpriteStart";
			this.labelSpriteStart.Size = new System.Drawing.Size(25, 17);
			this.labelSpriteStart.Text = "0, 0";
			this.labelSpriteStart.Visible = false;
			// 
			// labelX
			// 
			this.labelX.Name = "labelX";
			this.labelX.Size = new System.Drawing.Size(12, 17);
			this.labelX.Text = "x";
			this.labelX.Visible = false;
			// 
			// labelSpriteSize
			// 
			this.labelSpriteSize.Name = "labelSpriteSize";
			this.labelSpriteSize.Size = new System.Drawing.Size(25, 17);
			this.labelSpriteSize.Text = "0, 0";
			this.labelSpriteSize.Visible = false;
			// 
			// labelMiscInfo
			// 
			this.labelMiscInfo.Name = "labelMiscInfo";
			this.labelMiscInfo.Size = new System.Drawing.Size(828, 17);
			this.labelMiscInfo.Spring = true;
			this.labelMiscInfo.Text = "Misc.";
			this.labelMiscInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelMiscInfo.Visible = false;
			// 
			// panelMain
			// 
			this.panelMain.Controls.Add(this.scrollHorizontal);
			this.panelMain.Controls.Add(this.panelGorgon);
			this.panelMain.Controls.Add(this.panelVScroll);
			this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMain.Location = new System.Drawing.Point(169, 0);
			this.panelMain.Name = "panelMain";
			this.panelMain.Size = new System.Drawing.Size(500, 343);
			this.panelMain.TabIndex = 19;
			// 
			// scrollHorizontal
			// 
			this.scrollHorizontal.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.scrollHorizontal.Location = new System.Drawing.Point(0, 326);
			this.scrollHorizontal.Name = "scrollHorizontal";
			this.scrollHorizontal.Size = new System.Drawing.Size(484, 17);
			this.scrollHorizontal.TabIndex = 19;
			this.scrollHorizontal.Visible = false;
			// 
			// panelGorgon
			// 
			this.panelGorgon.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelGorgon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelGorgon.Location = new System.Drawing.Point(0, 0);
			this.panelGorgon.Margin = new System.Windows.Forms.Padding(4);
			this.panelGorgon.Name = "panelGorgon";
			this.panelGorgon.Size = new System.Drawing.Size(484, 343);
			this.panelGorgon.TabIndex = 18;
			this.panelGorgon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelGorgon_MouseMove);
			this.panelGorgon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelGorgon_MouseDown);
			this.panelGorgon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelGorgon_MouseUp);
			// 
			// panelVScroll
			// 
			this.panelVScroll.Controls.Add(this.scrollVertical);
			this.panelVScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelVScroll.Location = new System.Drawing.Point(484, 0);
			this.panelVScroll.Margin = new System.Windows.Forms.Padding(0);
			this.panelVScroll.Name = "panelVScroll";
			this.panelVScroll.Size = new System.Drawing.Size(16, 343);
			this.panelVScroll.TabIndex = 20;
			this.panelVScroll.Visible = false;
			// 
			// scrollVertical
			// 
			this.scrollVertical.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.scrollVertical.Location = new System.Drawing.Point(0, 0);
			this.scrollVertical.Name = "scrollVertical";
			this.scrollVertical.Size = new System.Drawing.Size(16, 326);
			this.scrollVertical.TabIndex = 18;
			// 
			// splitRenderTargetManager
			// 
			this.splitRenderTargetManager.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitRenderTargetManager.Location = new System.Drawing.Point(169, 343);
			this.splitRenderTargetManager.Name = "splitRenderTargetManager";
			this.splitRenderTargetManager.Size = new System.Drawing.Size(500, 4);
			this.splitRenderTargetManager.TabIndex = 20;
			this.splitRenderTargetManager.TabStop = false;
			this.splitRenderTargetManager.SplitterMoving += new System.Windows.Forms.SplitterEventHandler(this.splitRenderTargetManager_SplitterMoving);
			this.splitRenderTargetManager.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitRenderTargetManager_SplitterMoved);
			// 
			// panelRenderTargetManager
			// 
			this.panelRenderTargetManager.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelRenderTargetManager.Location = new System.Drawing.Point(169, 347);
			this.panelRenderTargetManager.Name = "panelRenderTargetManager";
			this.panelRenderTargetManager.Size = new System.Drawing.Size(500, 138);
			this.panelRenderTargetManager.TabIndex = 21;
			// 
			// splitSpriteManager
			// 
			this.splitSpriteManager.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitSpriteManager.Location = new System.Drawing.Point(669, 0);
			this.splitSpriteManager.Margin = new System.Windows.Forms.Padding(4);
			this.splitSpriteManager.MinExtra = 160;
			this.splitSpriteManager.MinSize = 120;
			this.splitSpriteManager.Name = "splitSpriteManager";
			this.splitSpriteManager.Size = new System.Drawing.Size(5, 485);
			this.splitSpriteManager.TabIndex = 7;
			this.splitSpriteManager.TabStop = false;
			this.splitSpriteManager.SplitterMoving += new System.Windows.Forms.SplitterEventHandler(this.splitSpriteManager_SplitterMoving);
			this.splitSpriteManager.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitSpriteManager_SplitterMoved);
			// 
			// panelSpriteManager
			// 
			this.panelSpriteManager.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelSpriteManager.Location = new System.Drawing.Point(674, 0);
			this.panelSpriteManager.Margin = new System.Windows.Forms.Padding(4);
			this.panelSpriteManager.Name = "panelSpriteManager";
			this.panelSpriteManager.Size = new System.Drawing.Size(169, 485);
			this.panelSpriteManager.TabIndex = 8;
			// 
			// splitImageManager
			// 
			this.splitImageManager.Location = new System.Drawing.Point(165, 0);
			this.splitImageManager.Name = "splitImageManager";
			this.splitImageManager.Size = new System.Drawing.Size(4, 485);
			this.splitImageManager.TabIndex = 18;
			this.splitImageManager.TabStop = false;
			this.splitImageManager.SplitterMoving += new System.Windows.Forms.SplitterEventHandler(this.splitImageManager_SplitterMoving);
			this.splitImageManager.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitImageManager_SplitterMoved);
			// 
			// panelImageManager
			// 
			this.panelImageManager.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelImageManager.Location = new System.Drawing.Point(0, 0);
			this.panelImageManager.Margin = new System.Windows.Forms.Padding(4);
			this.panelImageManager.Name = "panelImageManager";
			this.panelImageManager.Size = new System.Drawing.Size(165, 485);
			this.panelImageManager.TabIndex = 8;
			// 
			// menuMain
			// 
			this.menuMain.Dock = System.Windows.Forms.DockStyle.None;
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.menuItemTools,
            this.menuItemHelp});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(843, 24);
			this.menuMain.TabIndex = 0;
			this.menuMain.Text = "menuMain";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNewProject,
            this.menuItemOpenProject,
            this.toolStripSeparator4,
            this.menuItemSaveProject,
            this.menuItemSaveProjectAs,
            this.menuItemSaveAll,
            this.toolStripSeparator1,
            this.menuItemSaveSprite,
            this.menuItemSaveSpriteAs,
            this.toolStripSeparator5,
            this.menuItemPrefs,
            this.toolStripMenuItem2,
            this.menuItemExit});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// menuItemNewProject
			// 
			this.menuItemNewProject.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_dirty;
			this.menuItemNewProject.Name = "menuItemNewProject";
			this.menuItemNewProject.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.menuItemNewProject.Size = new System.Drawing.Size(195, 22);
			this.menuItemNewProject.Text = "&New project...";
			this.menuItemNewProject.Click += new System.EventHandler(this.menuItemNewProject_Click);
			// 
			// menuItemOpenProject
			// 
			this.menuItemOpenProject.Enabled = false;
			this.menuItemOpenProject.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.folder_out;
			this.menuItemOpenProject.Name = "menuItemOpenProject";
			this.menuItemOpenProject.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.menuItemOpenProject.Size = new System.Drawing.Size(195, 22);
			this.menuItemOpenProject.Text = "&Open project...";
			this.menuItemOpenProject.Click += new System.EventHandler(this.menuItemOpenProject_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(192, 6);
			// 
			// menuItemSaveProject
			// 
			this.menuItemSaveProject.Enabled = false;
			this.menuItemSaveProject.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.disk_blue;
			this.menuItemSaveProject.Name = "menuItemSaveProject";
			this.menuItemSaveProject.Size = new System.Drawing.Size(195, 22);
			this.menuItemSaveProject.Text = "Save project";
			this.menuItemSaveProject.Click += new System.EventHandler(this.menuItemSaveProject_Click);
			// 
			// menuItemSaveProjectAs
			// 
			this.menuItemSaveProjectAs.Name = "menuItemSaveProjectAs";
			this.menuItemSaveProjectAs.Size = new System.Drawing.Size(195, 22);
			this.menuItemSaveProjectAs.Text = "Save project &as...";
			this.menuItemSaveProjectAs.Click += new System.EventHandler(this.menuItemSaveProjectAs_Click);
			// 
			// menuItemSaveAll
			// 
			this.menuItemSaveAll.Enabled = false;
			this.menuItemSaveAll.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.disks;
			this.menuItemSaveAll.Name = "menuItemSaveAll";
			this.menuItemSaveAll.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
						| System.Windows.Forms.Keys.S)));
			this.menuItemSaveAll.Size = new System.Drawing.Size(195, 22);
			this.menuItemSaveAll.Text = "Save a&ll";
			this.menuItemSaveAll.Click += new System.EventHandler(this.menuItemSaveAll_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(192, 6);
			// 
			// menuItemSaveSprite
			// 
			this.menuItemSaveSprite.Enabled = false;
			this.menuItemSaveSprite.Name = "menuItemSaveSprite";
			this.menuItemSaveSprite.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.menuItemSaveSprite.Size = new System.Drawing.Size(195, 22);
			this.menuItemSaveSprite.Text = "Save sprite";
			this.menuItemSaveSprite.Click += new System.EventHandler(this.menuItemSaveSprite_Click);
			// 
			// menuItemSaveSpriteAs
			// 
			this.menuItemSaveSpriteAs.Enabled = false;
			this.menuItemSaveSpriteAs.Name = "menuItemSaveSpriteAs";
			this.menuItemSaveSpriteAs.Size = new System.Drawing.Size(195, 22);
			this.menuItemSaveSpriteAs.Text = "Save &sprite as...";
			this.menuItemSaveSpriteAs.Click += new System.EventHandler(this.menuItemSaveSpriteAs_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(192, 6);
			// 
			// menuItemPrefs
			// 
			this.menuItemPrefs.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.preferences;
			this.menuItemPrefs.Name = "menuItemPrefs";
			this.menuItemPrefs.Size = new System.Drawing.Size(195, 22);
			this.menuItemPrefs.Text = "Preferences...";
			this.menuItemPrefs.Click += new System.EventHandler(this.menuItemPrefs_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(192, 6);
			// 
			// menuItemExit
			// 
			this.menuItemExit.Name = "menuItemExit";
			this.menuItemExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.menuItemExit.Size = new System.Drawing.Size(195, 22);
			this.menuItemExit.Text = "E&xit";
			this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
			// 
			// menuItemTools
			// 
			this.menuItemTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemImageManager,
            this.menuItemTargetManager,
            this.menuItemSpriteManager});
			this.menuItemTools.Name = "menuItemTools";
			this.menuItemTools.Size = new System.Drawing.Size(48, 20);
			this.menuItemTools.Text = "&Tools";
			// 
			// menuItemImageManager
			// 
			this.menuItemImageManager.CheckOnClick = true;
			this.menuItemImageManager.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.photo_scenery;
			this.menuItemImageManager.Name = "menuItemImageManager";
			this.menuItemImageManager.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
			this.menuItemImageManager.Size = new System.Drawing.Size(204, 22);
			this.menuItemImageManager.Text = "&Image manager...";
			// 
			// menuItemTargetManager
			// 
			this.menuItemTargetManager.CheckOnClick = true;
			this.menuItemTargetManager.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.target;
			this.menuItemTargetManager.Name = "menuItemTargetManager";
			this.menuItemTargetManager.Size = new System.Drawing.Size(204, 22);
			this.menuItemTargetManager.Text = "&Render target manager...";
			// 
			// menuItemSpriteManager
			// 
			this.menuItemSpriteManager.CheckOnClick = true;
			this.menuItemSpriteManager.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.GorSprite_16;
			this.menuItemSpriteManager.Name = "menuItemSpriteManager";
			this.menuItemSpriteManager.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
			this.menuItemSpriteManager.Size = new System.Drawing.Size(204, 22);
			this.menuItemSpriteManager.Text = "&Sprite manager...";
			// 
			// menuItemHelp
			// 
			this.menuItemHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAbout});
			this.menuItemHelp.Name = "menuItemHelp";
			this.menuItemHelp.Size = new System.Drawing.Size(44, 20);
			this.menuItemHelp.Text = "Help";
			// 
			// menuItemAbout
			// 
			this.menuItemAbout.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.GorSprite_16;
			this.menuItemAbout.Name = "menuItemAbout";
			this.menuItemAbout.Size = new System.Drawing.Size(116, 22);
			this.menuItemAbout.Text = "About...";
			this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
			// 
			// stripSpriteClip
			// 
			this.stripSpriteClip.Dock = System.Windows.Forms.DockStyle.None;
			this.stripSpriteClip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripSpriteClip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonKeyboardMode,
            this.toolStripSeparator2,
            this.buttonFixedSize,
            this.textFixedWidth,
            this.toolStripLabel1,
            this.textFixedHeight,
            this.toolStripSeparator3,
            this.buttonFollowCursor,
            this.toolStripLabel2,
            this.textWindowSize,
            this.toolStripLabel3,
            this.toolStripSeparator6,
            this.buttonBackgroundColor});
			this.stripSpriteClip.Location = new System.Drawing.Point(0, 24);
			this.stripSpriteClip.Name = "stripSpriteClip";
			this.stripSpriteClip.Size = new System.Drawing.Size(843, 25);
			this.stripSpriteClip.Stretch = true;
			this.stripSpriteClip.TabIndex = 1;
			this.stripSpriteClip.Visible = false;
			// 
			// buttonKeyboardMode
			// 
			this.buttonKeyboardMode.CheckOnClick = true;
			this.buttonKeyboardMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonKeyboardMode.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.keyboard2;
			this.buttonKeyboardMode.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonKeyboardMode.Name = "buttonKeyboardMode";
			this.buttonKeyboardMode.Size = new System.Drawing.Size(23, 22);
			this.buttonKeyboardMode.Text = "Enable/disable keyboard mode. (Space)";
			this.buttonKeyboardMode.Click += new System.EventHandler(this.buttonKeyboardMode_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonFixedSize
			// 
			this.buttonFixedSize.CheckOnClick = true;
			this.buttonFixedSize.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.lock2;
			this.buttonFixedSize.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonFixedSize.Name = "buttonFixedSize";
			this.buttonFixedSize.Size = new System.Drawing.Size(76, 22);
			this.buttonFixedSize.Text = "Fixed size";
			this.buttonFixedSize.Click += new System.EventHandler(this.buttonFixedSize_Click);
			// 
			// textFixedWidth
			// 
			this.textFixedWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textFixedWidth.Enabled = false;
			this.textFixedWidth.Name = "textFixedWidth";
			this.textFixedWidth.Size = new System.Drawing.Size(40, 25);
			this.textFixedWidth.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textFixedWidth.Leave += new System.EventHandler(this.textFixedWidth_Leave);
			this.textFixedWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textFixedWidth_KeyDown);
			this.textFixedWidth.Validating += new System.ComponentModel.CancelEventHandler(this.textFixedWidth_Validating);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(12, 22);
			this.toolStripLabel1.Text = "x";
			// 
			// textFixedHeight
			// 
			this.textFixedHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textFixedHeight.Enabled = false;
			this.textFixedHeight.Name = "textFixedHeight";
			this.textFixedHeight.Size = new System.Drawing.Size(40, 25);
			this.textFixedHeight.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textFixedHeight.Leave += new System.EventHandler(this.textFixedHeight_Leave);
			this.textFixedHeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textFixedHeight_KeyDown);
			this.textFixedHeight.Validating += new System.ComponentModel.CancelEventHandler(this.textFixedHeight_Validating);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonFollowCursor
			// 
			this.buttonFollowCursor.Checked = true;
			this.buttonFollowCursor.CheckOnClick = true;
			this.buttonFollowCursor.CheckState = System.Windows.Forms.CheckState.Checked;
			this.buttonFollowCursor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonFollowCursor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.FollowCursor;
			this.buttonFollowCursor.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonFollowCursor.Name = "buttonFollowCursor";
			this.buttonFollowCursor.Size = new System.Drawing.Size(23, 22);
			this.buttonFollowCursor.Text = "Follow cursor";
			this.buttonFollowCursor.Click += new System.EventHandler(this.buttonFollowCursor_Click);
			// 
			// toolStripLabel2
			// 
			this.toolStripLabel2.Name = "toolStripLabel2";
			this.toolStripLabel2.Size = new System.Drawing.Size(77, 22);
			this.toolStripLabel2.Text = "Window Size:";
			// 
			// textWindowSize
			// 
			this.textWindowSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textWindowSize.Name = "textWindowSize";
			this.textWindowSize.Size = new System.Drawing.Size(50, 25);
			this.textWindowSize.Leave += new System.EventHandler(this.textWindowSize_Leave);
			this.textWindowSize.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textWindowSize_KeyDown);
			this.textWindowSize.Validating += new System.ComponentModel.CancelEventHandler(this.textWindowSize_Validating);
			// 
			// toolStripLabel3
			// 
			this.toolStripLabel3.Name = "toolStripLabel3";
			this.toolStripLabel3.Size = new System.Drawing.Size(36, 22);
			this.toolStripLabel3.Text = "pixels";
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonBackgroundColor
			// 
			this.buttonBackgroundColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonBackgroundColor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.Color;
			this.buttonBackgroundColor.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonBackgroundColor.Name = "buttonBackgroundColor";
			this.buttonBackgroundColor.Size = new System.Drawing.Size(23, 22);
			this.buttonBackgroundColor.Text = "Background color";
			this.buttonBackgroundColor.Click += new System.EventHandler(this.buttonBackgroundColor_Click);
			// 
			// dialogOpen
			// 
			this.dialogOpen.DefaultExt = "gorSprite";
			this.dialogOpen.Filter = "Gorgon Sprite (*.gorSprite)|*.gorSprite|XML Sprite (*.xml)|*.xml|All Files (*.*)|" +
				"*.*";
			this.dialogOpen.InitialDirectory = ".\\";
			this.dialogOpen.Multiselect = true;
			this.dialogOpen.Title = "Open sprite.";
			// 
			// dialogSave
			// 
			this.dialogSave.DefaultExt = "gorSprite";
			this.dialogSave.Filter = "Gorgon Sprite (*.gorSprite)|*.gorSprite|XML Sprite (*.xml)|*.xml";
			this.dialogSave.InitialDirectory = ".\\";
			this.dialogSave.Title = "Save sprite as.";
			// 
			// formMain
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(843, 556);
			this.Controls.Add(this.containerMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuMain;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(851, 583);
			this.Name = "formMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Sprite Editor";
			this.containerMain.BottomToolStripPanel.ResumeLayout(false);
			this.containerMain.BottomToolStripPanel.PerformLayout();
			this.containerMain.ContentPanel.ResumeLayout(false);
			this.containerMain.TopToolStripPanel.ResumeLayout(false);
			this.containerMain.TopToolStripPanel.PerformLayout();
			this.containerMain.ResumeLayout(false);
			this.containerMain.PerformLayout();
			this.stripMain.ResumeLayout(false);
			this.stripMain.PerformLayout();
			this.panelMain.ResumeLayout(false);
			this.panelVScroll.ResumeLayout(false);
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.stripSpriteClip.ResumeLayout(false);
			this.stripSpriteClip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer containerMain;
		private System.Windows.Forms.MenuStrip menuMain;
		private System.Windows.Forms.StatusStrip stripMain;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menuItemNewProject;
		private System.Windows.Forms.ToolStripMenuItem menuItemOpenProject;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveSpriteAs;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveSprite;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveAll;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem menuItemExit;
		private System.Windows.Forms.OpenFileDialog dialogOpen;
		private System.Windows.Forms.ToolStripMenuItem menuItemTools;
		private System.Windows.Forms.ToolStripMenuItem menuItemImageManager;
		private System.Windows.Forms.ToolStripMenuItem menuItemTargetManager;
		private System.Windows.Forms.ToolStripMenuItem menuItemSpriteManager;
		private System.Windows.Forms.ToolStripStatusLabel labelName;
		private System.Windows.Forms.ToolStripStatusLabel labelSpriteStart;
		private System.Windows.Forms.ToolStripStatusLabel labelX;
		private System.Windows.Forms.ToolStripStatusLabel labelSpriteSize;
		private System.Windows.Forms.ToolStrip stripSpriteClip;
		private System.Windows.Forms.ToolStripButton buttonKeyboardMode;
		private System.Windows.Forms.ToolStripStatusLabel labelMiscInfo;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton buttonFixedSize;
		private System.Windows.Forms.ToolStripTextBox textFixedWidth;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripTextBox textFixedHeight;
		private System.Windows.Forms.ToolStripButton buttonFollowCursor;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.SaveFileDialog dialogSave;
		private System.Windows.Forms.Splitter splitSpriteManager;
		private System.Windows.Forms.Splitter splitImageManager;
		private System.Windows.Forms.Panel panelSpriteManager;
		private System.Windows.Forms.Panel panelImageManager;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveProject;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveProjectAs;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Panel panelGorgon;
		private System.Windows.Forms.HScrollBar scrollHorizontal;
		private System.Windows.Forms.Panel panelVScroll;
		private System.Windows.Forms.VScrollBar scrollVertical;
		private System.Windows.Forms.Splitter splitRenderTargetManager;
		private System.Windows.Forms.Panel panelRenderTargetManager;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem menuItemPrefs;
		private System.Windows.Forms.ToolStripMenuItem menuItemHelp;
		private System.Windows.Forms.ToolStripMenuItem menuItemAbout;
		private System.Windows.Forms.ToolStripButton buttonBackgroundColor;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStripTextBox textWindowSize;
		private System.Windows.Forms.ToolStripLabel toolStripLabel3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
	}
}

