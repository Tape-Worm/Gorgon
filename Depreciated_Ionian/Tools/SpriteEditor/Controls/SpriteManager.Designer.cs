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
// Created: Wednesday, May 30, 2007 3:30:07 PM
// 
#endregion

using System;
using System.ComponentModel;

namespace GorgonLibrary.Graphics.Tools.Controls
{
	partial class SpriteManager
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.popupSprites = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemAddSprite = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemUpdate = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDeleteSprites = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemClearAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemClone = new System.Windows.Forms.ToolStripMenuItem();
			this.menuFlip = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemHorizontal = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemVertical = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemBind = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemUnbind = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemAutoResize = new System.Windows.Forms.ToolStripMenuItem();
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
			this.popupPropGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuReset = new System.Windows.Forms.ToolStripMenuItem();
			this.dialogOpen = new System.Windows.Forms.OpenFileDialog();
			this.dialogSave = new System.Windows.Forms.SaveFileDialog();
			this.tabSpriteExtra = new System.Windows.Forms.TabControl();
			this.pageProperties = new System.Windows.Forms.TabPage();
			this.gridSpriteProperties = new System.Windows.Forms.PropertyGrid();
			this.pageAnimations = new System.Windows.Forms.TabPage();
			this.containerAnimation = new System.Windows.Forms.ToolStripContainer();
			this.panelAnimation = new System.Windows.Forms.Panel();
			this.listAnimations = new System.Windows.Forms.ListView();
			this.headerAnimationName = new System.Windows.Forms.ColumnHeader();
			this.headerAnimationLength = new System.Windows.Forms.ColumnHeader();
			this.headerLooped = new System.Windows.Forms.ColumnHeader();
			this.popupAnimations = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemAddAnimation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemUpdateAnimation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDeleteAnimation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemClearAnimations = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemAnimationEditor = new System.Windows.Forms.ToolStripMenuItem();
			this.stripAnimation = new System.Windows.Forms.ToolStrip();
			this.buttonNewAnimation = new System.Windows.Forms.ToolStripButton();
			this.buttonEditAnimation = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonDeleteAnimation = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonAnimationEditor = new System.Windows.Forms.ToolStripButton();
			this.stripSpriteManager = new System.Windows.Forms.ToolStrip();
			this.buttonCreateSprite = new System.Windows.Forms.ToolStripButton();
			this.buttonOpenSprite = new System.Windows.Forms.ToolStripButton();
			this.buttonSaveSprite = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEditSprite = new System.Windows.Forms.ToolStripButton();
			this.buttonRemoveSprites = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonSpriteImport = new System.Windows.Forms.ToolStripDropDownButton();
			this.menuItemNvidiaImport = new System.Windows.Forms.ToolStripMenuItem();
			this.listSprites = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.containerSpriteManager = new System.Windows.Forms.ToolStripContainer();
			this.splitSpriteList = new System.Windows.Forms.SplitContainer();
			this.popupSprites.SuspendLayout();
			this.popupPropGrid.SuspendLayout();
			this.tabSpriteExtra.SuspendLayout();
			this.pageProperties.SuspendLayout();
			this.pageAnimations.SuspendLayout();
			this.containerAnimation.ContentPanel.SuspendLayout();
			this.containerAnimation.TopToolStripPanel.SuspendLayout();
			this.containerAnimation.SuspendLayout();
			this.panelAnimation.SuspendLayout();
			this.popupAnimations.SuspendLayout();
			this.stripAnimation.SuspendLayout();
			this.stripSpriteManager.SuspendLayout();
			this.containerSpriteManager.ContentPanel.SuspendLayout();
			this.containerSpriteManager.TopToolStripPanel.SuspendLayout();
			this.containerSpriteManager.SuspendLayout();
			this.splitSpriteList.Panel1.SuspendLayout();
			this.splitSpriteList.Panel2.SuspendLayout();
			this.splitSpriteList.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelManager
			// 
			this.labelManager.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.labelManager.Size = new System.Drawing.Size(326, 19);
			this.labelManager.Text = "Sprite Manager";
			// 
			// labelManagerClose
			// 
			this.labelManagerClose.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			// 
			// popupSprites
			// 
			this.popupSprites.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAddSprite,
            this.menuItemUpdate,
            this.menuItemDeleteSprites,
            this.menuItemClearAll,
            this.toolStripSeparator4,
            this.menuItemClone,
            this.menuFlip,
            this.menuItemBind,
            this.menuItemUnbind,
            this.menuItemAutoResize});
			this.popupSprites.Name = "popupSprites";
			this.popupSprites.Size = new System.Drawing.Size(172, 208);
			this.popupSprites.Opening += new System.ComponentModel.CancelEventHandler(this.popupSprites_Opening);
			// 
			// menuItemAddSprite
			// 
			this.menuItemAddSprite.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_dirty;
			this.menuItemAddSprite.Name = "menuItemAddSprite";
			this.menuItemAddSprite.Size = new System.Drawing.Size(171, 22);
			this.menuItemAddSprite.Text = "Add sprite...";
			this.menuItemAddSprite.Click += new System.EventHandler(this.buttonCreateSprite_Click);
			// 
			// menuItemUpdate
			// 
			this.menuItemUpdate.Enabled = false;
			this.menuItemUpdate.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.menuItemUpdate.Name = "menuItemUpdate";
			this.menuItemUpdate.Size = new System.Drawing.Size(171, 22);
			this.menuItemUpdate.Text = "Update sprite...";
			this.menuItemUpdate.Click += new System.EventHandler(this.buttonEditSprite_Click);
			// 
			// menuItemDeleteSprites
			// 
			this.menuItemDeleteSprites.Enabled = false;
			this.menuItemDeleteSprites.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.menuItemDeleteSprites.Name = "menuItemDeleteSprites";
			this.menuItemDeleteSprites.Size = new System.Drawing.Size(171, 22);
			this.menuItemDeleteSprites.Text = "Delete sprite(s)...";
			this.menuItemDeleteSprites.Click += new System.EventHandler(this.buttonRemoveSprites_Click);
			// 
			// menuItemClearAll
			// 
			this.menuItemClearAll.Enabled = false;
			this.menuItemClearAll.Name = "menuItemClearAll";
			this.menuItemClearAll.Size = new System.Drawing.Size(171, 22);
			this.menuItemClearAll.Text = "Clear all sprites...";
			this.menuItemClearAll.Click += new System.EventHandler(this.menuItemClearAll_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(168, 6);
			// 
			// menuItemClone
			// 
			this.menuItemClone.Enabled = false;
			this.menuItemClone.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.copy;
			this.menuItemClone.Name = "menuItemClone";
			this.menuItemClone.Size = new System.Drawing.Size(171, 22);
			this.menuItemClone.Text = "Clone";
			this.menuItemClone.Click += new System.EventHandler(this.menuItemClone_Click);
			// 
			// menuFlip
			// 
			this.menuFlip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemHorizontal,
            this.menuItemVertical});
			this.menuFlip.Enabled = false;
			this.menuFlip.Name = "menuFlip";
			this.menuFlip.Size = new System.Drawing.Size(171, 22);
			this.menuFlip.Text = "Flip";
			// 
			// menuItemHorizontal
			// 
			this.menuItemHorizontal.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.leftright;
			this.menuItemHorizontal.Name = "menuItemHorizontal";
			this.menuItemHorizontal.Size = new System.Drawing.Size(133, 22);
			this.menuItemHorizontal.Text = "Horizontal";
			this.menuItemHorizontal.Click += new System.EventHandler(this.menuItemHorizontal_Click);
			// 
			// menuItemVertical
			// 
			this.menuItemVertical.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.updown;
			this.menuItemVertical.Name = "menuItemVertical";
			this.menuItemVertical.Size = new System.Drawing.Size(133, 22);
			this.menuItemVertical.Text = "Vertical";
			this.menuItemVertical.Click += new System.EventHandler(this.menuItemVertical_Click);
			// 
			// menuItemBind
			// 
			this.menuItemBind.Enabled = false;
			this.menuItemBind.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.photo_scenery;
			this.menuItemBind.Name = "menuItemBind";
			this.menuItemBind.Size = new System.Drawing.Size(171, 22);
			this.menuItemBind.Text = "Bind image(s)...";
			this.menuItemBind.Click += new System.EventHandler(this.menuItemBind_Click);
			// 
			// menuItemUnbind
			// 
			this.menuItemUnbind.Enabled = false;
			this.menuItemUnbind.Name = "menuItemUnbind";
			this.menuItemUnbind.Size = new System.Drawing.Size(171, 22);
			this.menuItemUnbind.Text = "Unbind image(s)";
			this.menuItemUnbind.Click += new System.EventHandler(this.menuItemUnbind_Click);
			// 
			// menuItemAutoResize
			// 
			this.menuItemAutoResize.Enabled = false;
			this.menuItemAutoResize.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_exchange;
			this.menuItemAutoResize.Name = "menuItemAutoResize";
			this.menuItemAutoResize.Size = new System.Drawing.Size(171, 22);
			this.menuItemAutoResize.Text = "Auto resize...";
			this.menuItemAutoResize.Click += new System.EventHandler(this.menuItemAutoResize_Click);
			// 
			// BottomToolStripPanel
			// 
			this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.BottomToolStripPanel.Name = "BottomToolStripPanel";
			this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// TopToolStripPanel
			// 
			this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.TopToolStripPanel.Name = "TopToolStripPanel";
			this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// RightToolStripPanel
			// 
			this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.RightToolStripPanel.Name = "RightToolStripPanel";
			this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// LeftToolStripPanel
			// 
			this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.LeftToolStripPanel.Name = "LeftToolStripPanel";
			this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// ContentPanel
			// 
			this.ContentPanel.Size = new System.Drawing.Size(473, 507);
			// 
			// popupPropGrid
			// 
			this.popupPropGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuReset});
			this.popupPropGrid.Name = "popupPropGrid";
			this.popupPropGrid.Size = new System.Drawing.Size(114, 26);
			this.popupPropGrid.Opening += new System.ComponentModel.CancelEventHandler(this.popupPropGrid_Opening);
			// 
			// menuReset
			// 
			this.menuReset.Name = "menuReset";
			this.menuReset.Size = new System.Drawing.Size(113, 22);
			this.menuReset.Text = "Reset";
			this.menuReset.Click += new System.EventHandler(this.menuReset_Click);
			// 
			// dialogOpen
			// 
			this.dialogOpen.DefaultExt = "gorSprite";
			this.dialogOpen.Filter = "Gorgon Sprite (*.gorSprite)|*.gorSprite|XML Sprite (*.xml)|*.xml|All files (*.*)|" +
				"*.*";
			this.dialogOpen.InitialDirectory = ".\\";
			this.dialogOpen.Multiselect = true;
			this.dialogOpen.Title = "Open a sprite...";
			// 
			// dialogSave
			// 
			this.dialogSave.DefaultExt = "gorSprite";
			this.dialogSave.Filter = "Gorgon Sprite (*.gorSprite)|*.gorSprite|XML Sprite (*.xml)|*.xml|All files (*.*)|" +
				"*.*";
			this.dialogSave.InitialDirectory = ".\\";
			this.dialogSave.Title = "Save sprite as...";
			// 
			// tabSpriteExtra
			// 
			this.tabSpriteExtra.Controls.Add(this.pageProperties);
			this.tabSpriteExtra.Controls.Add(this.pageAnimations);
			this.tabSpriteExtra.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabSpriteExtra.Location = new System.Drawing.Point(0, 0);
			this.tabSpriteExtra.Name = "tabSpriteExtra";
			this.tabSpriteExtra.SelectedIndex = 0;
			this.tabSpriteExtra.Size = new System.Drawing.Size(355, 192);
			this.tabSpriteExtra.TabIndex = 0;
			this.tabSpriteExtra.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabSpriteExtra_Selected);
			// 
			// pageProperties
			// 
			this.pageProperties.Controls.Add(this.gridSpriteProperties);
			this.pageProperties.Location = new System.Drawing.Point(4, 22);
			this.pageProperties.Name = "pageProperties";
			this.pageProperties.Padding = new System.Windows.Forms.Padding(3);
			this.pageProperties.Size = new System.Drawing.Size(347, 166);
			this.pageProperties.TabIndex = 0;
			this.pageProperties.Text = "Properties";
			this.pageProperties.UseVisualStyleBackColor = true;
			// 
			// gridSpriteProperties
			// 
			this.gridSpriteProperties.BackColor = System.Drawing.SystemColors.Control;
			this.gridSpriteProperties.CommandsBackColor = System.Drawing.SystemColors.Window;
			this.gridSpriteProperties.ContextMenuStrip = this.popupPropGrid;
			this.gridSpriteProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridSpriteProperties.Location = new System.Drawing.Point(3, 3);
			this.gridSpriteProperties.Name = "gridSpriteProperties";
			this.gridSpriteProperties.Size = new System.Drawing.Size(341, 160);
			this.gridSpriteProperties.TabIndex = 1;
			// 
			// pageAnimations
			// 
			this.pageAnimations.Controls.Add(this.containerAnimation);
			this.pageAnimations.Location = new System.Drawing.Point(4, 22);
			this.pageAnimations.Name = "pageAnimations";
			this.pageAnimations.Padding = new System.Windows.Forms.Padding(3);
			this.pageAnimations.Size = new System.Drawing.Size(347, 166);
			this.pageAnimations.TabIndex = 1;
			this.pageAnimations.Text = "Animations";
			this.pageAnimations.UseVisualStyleBackColor = true;
			// 
			// containerAnimation
			// 
			this.containerAnimation.BottomToolStripPanelVisible = false;
			// 
			// containerAnimation.ContentPanel
			// 
			this.containerAnimation.ContentPanel.Controls.Add(this.panelAnimation);
			this.containerAnimation.ContentPanel.Size = new System.Drawing.Size(341, 135);
			this.containerAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerAnimation.LeftToolStripPanelVisible = false;
			this.containerAnimation.Location = new System.Drawing.Point(3, 3);
			this.containerAnimation.Name = "containerAnimation";
			this.containerAnimation.RightToolStripPanelVisible = false;
			this.containerAnimation.Size = new System.Drawing.Size(341, 160);
			this.containerAnimation.TabIndex = 0;
			this.containerAnimation.Text = "toolStripContainer1";
			// 
			// containerAnimation.TopToolStripPanel
			// 
			this.containerAnimation.TopToolStripPanel.Controls.Add(this.stripAnimation);
			// 
			// panelAnimation
			// 
			this.panelAnimation.Controls.Add(this.listAnimations);
			this.panelAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelAnimation.Location = new System.Drawing.Point(0, 0);
			this.panelAnimation.Name = "panelAnimation";
			this.panelAnimation.Size = new System.Drawing.Size(341, 135);
			this.panelAnimation.TabIndex = 1;
			// 
			// listAnimations
			// 
			this.listAnimations.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listAnimations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerAnimationName,
            this.headerAnimationLength,
            this.headerLooped});
			this.listAnimations.ContextMenuStrip = this.popupAnimations;
			this.listAnimations.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listAnimations.FullRowSelect = true;
			this.listAnimations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listAnimations.HideSelection = false;
			this.listAnimations.LabelEdit = true;
			this.listAnimations.Location = new System.Drawing.Point(0, 0);
			this.listAnimations.Name = "listAnimations";
			this.listAnimations.Size = new System.Drawing.Size(341, 135);
			this.listAnimations.TabIndex = 5;
			this.listAnimations.UseCompatibleStateImageBehavior = false;
			this.listAnimations.View = System.Windows.Forms.View.Details;
			this.listAnimations.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listAnimations_AfterLabelEdit);
			this.listAnimations.SelectedIndexChanged += new System.EventHandler(this.listAnimations_SelectedIndexChanged);
			this.listAnimations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listAnimations_KeyDown);
			// 
			// headerAnimationName
			// 
			this.headerAnimationName.Text = "Name";
			// 
			// headerAnimationLength
			// 
			this.headerAnimationLength.Text = "Animation length";
			// 
			// headerLooped
			// 
			this.headerLooped.Text = "Looped?";
			this.headerLooped.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// popupAnimations
			// 
			this.popupAnimations.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAddAnimation,
            this.menuItemUpdateAnimation,
            this.menuItemDeleteAnimation,
            this.menuItemClearAnimations,
            this.toolStripMenuItem1,
            this.menuItemAnimationEditor});
			this.popupAnimations.Name = "popupSprites";
			this.popupAnimations.Size = new System.Drawing.Size(191, 120);
			this.popupAnimations.Opening += new System.ComponentModel.CancelEventHandler(this.popupAnimations_Opening);
			// 
			// menuItemAddAnimation
			// 
			this.menuItemAddAnimation.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_dirty;
			this.menuItemAddAnimation.Name = "menuItemAddAnimation";
			this.menuItemAddAnimation.Size = new System.Drawing.Size(190, 22);
			this.menuItemAddAnimation.Text = "Add animation...";
			this.menuItemAddAnimation.Click += new System.EventHandler(this.buttonNewAnimation_Click);
			// 
			// menuItemUpdateAnimation
			// 
			this.menuItemUpdateAnimation.Enabled = false;
			this.menuItemUpdateAnimation.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.menuItemUpdateAnimation.Name = "menuItemUpdateAnimation";
			this.menuItemUpdateAnimation.Size = new System.Drawing.Size(190, 22);
			this.menuItemUpdateAnimation.Text = "Update animation...";
			this.menuItemUpdateAnimation.Click += new System.EventHandler(this.buttonNewAnimation_Click);
			// 
			// menuItemDeleteAnimation
			// 
			this.menuItemDeleteAnimation.Enabled = false;
			this.menuItemDeleteAnimation.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.menuItemDeleteAnimation.Name = "menuItemDeleteAnimation";
			this.menuItemDeleteAnimation.Size = new System.Drawing.Size(190, 22);
			this.menuItemDeleteAnimation.Text = "Delete animation(s)...";
			this.menuItemDeleteAnimation.Click += new System.EventHandler(this.buttonDeleteAnimation_Click);
			// 
			// menuItemClearAnimations
			// 
			this.menuItemClearAnimations.Enabled = false;
			this.menuItemClearAnimations.Name = "menuItemClearAnimations";
			this.menuItemClearAnimations.Size = new System.Drawing.Size(190, 22);
			this.menuItemClearAnimations.Text = "Clear all animations...";
			this.menuItemClearAnimations.Click += new System.EventHandler(this.menuItemClearAnimations_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(187, 6);
			// 
			// menuItemAnimationEditor
			// 
			this.menuItemAnimationEditor.Enabled = false;
			this.menuItemAnimationEditor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.movie;
			this.menuItemAnimationEditor.Name = "menuItemAnimationEditor";
			this.menuItemAnimationEditor.Size = new System.Drawing.Size(190, 22);
			this.menuItemAnimationEditor.Text = "Animation editor...";
			this.menuItemAnimationEditor.Click += new System.EventHandler(this.buttonAnimationEditor_Click);
			// 
			// stripAnimation
			// 
			this.stripAnimation.Dock = System.Windows.Forms.DockStyle.None;
			this.stripAnimation.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripAnimation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNewAnimation,
            this.buttonEditAnimation,
            this.toolStripSeparator2,
            this.buttonDeleteAnimation,
            this.toolStripButton1,
            this.buttonAnimationEditor});
			this.stripAnimation.Location = new System.Drawing.Point(0, 0);
			this.stripAnimation.Name = "stripAnimation";
			this.stripAnimation.Size = new System.Drawing.Size(341, 25);
			this.stripAnimation.Stretch = true;
			this.stripAnimation.TabIndex = 0;
			// 
			// buttonNewAnimation
			// 
			this.buttonNewAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNewAnimation.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_dirty;
			this.buttonNewAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNewAnimation.Name = "buttonNewAnimation";
			this.buttonNewAnimation.Size = new System.Drawing.Size(23, 22);
			this.buttonNewAnimation.Text = "Add a new animation.";
			this.buttonNewAnimation.Click += new System.EventHandler(this.buttonNewAnimation_Click);
			// 
			// buttonEditAnimation
			// 
			this.buttonEditAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditAnimation.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.buttonEditAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditAnimation.Name = "buttonEditAnimation";
			this.buttonEditAnimation.Size = new System.Drawing.Size(23, 22);
			this.buttonEditAnimation.Text = "Edit selected animation.";
			this.buttonEditAnimation.Click += new System.EventHandler(this.buttonNewAnimation_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonDeleteAnimation
			// 
			this.buttonDeleteAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonDeleteAnimation.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.buttonDeleteAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonDeleteAnimation.Name = "buttonDeleteAnimation";
			this.buttonDeleteAnimation.Size = new System.Drawing.Size(23, 22);
			this.buttonDeleteAnimation.Text = "Remove selected animation(s).";
			this.buttonDeleteAnimation.Click += new System.EventHandler(this.buttonDeleteAnimation_Click);
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonAnimationEditor
			// 
			this.buttonAnimationEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonAnimationEditor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.movie;
			this.buttonAnimationEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonAnimationEditor.Name = "buttonAnimationEditor";
			this.buttonAnimationEditor.Size = new System.Drawing.Size(23, 22);
			this.buttonAnimationEditor.Text = "Open animation editor.";
			this.buttonAnimationEditor.Click += new System.EventHandler(this.buttonAnimationEditor_Click);
			// 
			// stripSpriteManager
			// 
			this.stripSpriteManager.Dock = System.Windows.Forms.DockStyle.None;
			this.stripSpriteManager.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripSpriteManager.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonCreateSprite,
            this.buttonOpenSprite,
            this.buttonSaveSprite,
            this.toolStripSeparator1,
            this.buttonEditSprite,
            this.buttonRemoveSprites,
            this.toolStripSeparator3,
            this.buttonSpriteImport});
			this.stripSpriteManager.Location = new System.Drawing.Point(0, 0);
			this.stripSpriteManager.Name = "stripSpriteManager";
			this.stripSpriteManager.Size = new System.Drawing.Size(355, 25);
			this.stripSpriteManager.Stretch = true;
			this.stripSpriteManager.TabIndex = 6;
			// 
			// buttonCreateSprite
			// 
			this.buttonCreateSprite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonCreateSprite.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_dirty;
			this.buttonCreateSprite.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonCreateSprite.Name = "buttonCreateSprite";
			this.buttonCreateSprite.Size = new System.Drawing.Size(23, 22);
			this.buttonCreateSprite.Text = "Create a new sprite.";
			this.buttonCreateSprite.Click += new System.EventHandler(this.buttonCreateSprite_Click);
			// 
			// buttonOpenSprite
			// 
			this.buttonOpenSprite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonOpenSprite.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.folder_out;
			this.buttonOpenSprite.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonOpenSprite.Name = "buttonOpenSprite";
			this.buttonOpenSprite.Size = new System.Drawing.Size(23, 22);
			this.buttonOpenSprite.Text = "Open a sprite.";
			this.buttonOpenSprite.Click += new System.EventHandler(this.buttonOpenSprite_Click);
			// 
			// buttonSaveSprite
			// 
			this.buttonSaveSprite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSaveSprite.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.disk_blue;
			this.buttonSaveSprite.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSaveSprite.Name = "buttonSaveSprite";
			this.buttonSaveSprite.Size = new System.Drawing.Size(23, 22);
			this.buttonSaveSprite.Text = "Save selected sprite(s).";
			this.buttonSaveSprite.Click += new System.EventHandler(this.buttonSaveSprite_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonEditSprite
			// 
			this.buttonEditSprite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditSprite.Enabled = false;
			this.buttonEditSprite.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.buttonEditSprite.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditSprite.Name = "buttonEditSprite";
			this.buttonEditSprite.Size = new System.Drawing.Size(23, 22);
			this.buttonEditSprite.Text = "Update the selected sprite.";
			this.buttonEditSprite.Click += new System.EventHandler(this.buttonEditSprite_Click);
			// 
			// buttonRemoveSprites
			// 
			this.buttonRemoveSprites.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRemoveSprites.Enabled = false;
			this.buttonRemoveSprites.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.buttonRemoveSprites.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRemoveSprites.Name = "buttonRemoveSprites";
			this.buttonRemoveSprites.Size = new System.Drawing.Size(23, 22);
			this.buttonRemoveSprites.Text = "Remove the selected sprite(s).";
			this.buttonRemoveSprites.Click += new System.EventHandler(this.buttonRemoveSprites_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonSpriteImport
			// 
			this.buttonSpriteImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSpriteImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNvidiaImport});
			this.buttonSpriteImport.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.import1;
			this.buttonSpriteImport.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSpriteImport.Name = "buttonSpriteImport";
			this.buttonSpriteImport.Size = new System.Drawing.Size(29, 22);
			this.buttonSpriteImport.Text = "Import Sprites...";
			// 
			// menuItemNvidiaImport
			// 
			this.menuItemNvidiaImport.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.nvicon;
			this.menuItemNvidiaImport.Name = "menuItemNvidiaImport";
			this.menuItemNvidiaImport.Size = new System.Drawing.Size(278, 22);
			this.menuItemNvidiaImport.Text = "Import from nVidia Texture Atlas tool....";
			this.menuItemNvidiaImport.Click += new System.EventHandler(this.menuItemNvidiaImport_Click);
			// 
			// listSprites
			// 
			this.listSprites.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listSprites.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
			this.listSprites.ContextMenuStrip = this.popupSprites;
			this.listSprites.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listSprites.FullRowSelect = true;
			this.listSprites.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listSprites.HideSelection = false;
			this.listSprites.LabelEdit = true;
			this.listSprites.Location = new System.Drawing.Point(0, 0);
			this.listSprites.Name = "listSprites";
			this.listSprites.Size = new System.Drawing.Size(355, 192);
			this.listSprites.TabIndex = 4;
			this.listSprites.UseCompatibleStateImageBehavior = false;
			this.listSprites.View = System.Windows.Forms.View.Details;
			this.listSprites.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listSprites_AfterLabelEdit);
			this.listSprites.SelectedIndexChanged += new System.EventHandler(this.listSprites_SelectedIndexChanged);
			this.listSprites.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listSprites_MouseUp);
			this.listSprites.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listSprites_KeyDown);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Updated";
			this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Dimensions";
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Bound to image";
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Image is render target?";
			this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// containerSpriteManager
			// 
			// 
			// containerSpriteManager.ContentPanel
			// 
			this.containerSpriteManager.ContentPanel.Controls.Add(this.splitSpriteList);
			this.containerSpriteManager.ContentPanel.Margin = new System.Windows.Forms.Padding(2);
			this.containerSpriteManager.ContentPanel.Size = new System.Drawing.Size(355, 388);
			this.containerSpriteManager.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerSpriteManager.Location = new System.Drawing.Point(0, 19);
			this.containerSpriteManager.Margin = new System.Windows.Forms.Padding(2);
			this.containerSpriteManager.Name = "containerSpriteManager";
			this.containerSpriteManager.Size = new System.Drawing.Size(355, 413);
			this.containerSpriteManager.TabIndex = 9;
			this.containerSpriteManager.Text = "toolStripContainer1";
			// 
			// containerSpriteManager.TopToolStripPanel
			// 
			this.containerSpriteManager.TopToolStripPanel.Controls.Add(this.stripSpriteManager);
			// 
			// splitSpriteList
			// 
			this.splitSpriteList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitSpriteList.Location = new System.Drawing.Point(0, 0);
			this.splitSpriteList.Name = "splitSpriteList";
			this.splitSpriteList.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitSpriteList.Panel1
			// 
			this.splitSpriteList.Panel1.Controls.Add(this.listSprites);
			// 
			// splitSpriteList.Panel2
			// 
			this.splitSpriteList.Panel2.Controls.Add(this.tabSpriteExtra);
			this.splitSpriteList.Size = new System.Drawing.Size(355, 388);
			this.splitSpriteList.SplitterDistance = 192;
			this.splitSpriteList.TabIndex = 5;
			// 
			// SpriteManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.containerSpriteManager);
			this.Margin = new System.Windows.Forms.Padding(3);
			this.Name = "SpriteManager";
			this.Size = new System.Drawing.Size(355, 432);
			this.Controls.SetChildIndex(this.containerSpriteManager, 0);
			this.popupSprites.ResumeLayout(false);
			this.popupPropGrid.ResumeLayout(false);
			this.tabSpriteExtra.ResumeLayout(false);
			this.pageProperties.ResumeLayout(false);
			this.pageAnimations.ResumeLayout(false);
			this.containerAnimation.ContentPanel.ResumeLayout(false);
			this.containerAnimation.TopToolStripPanel.ResumeLayout(false);
			this.containerAnimation.TopToolStripPanel.PerformLayout();
			this.containerAnimation.ResumeLayout(false);
			this.containerAnimation.PerformLayout();
			this.panelAnimation.ResumeLayout(false);
			this.popupAnimations.ResumeLayout(false);
			this.stripAnimation.ResumeLayout(false);
			this.stripAnimation.PerformLayout();
			this.stripSpriteManager.ResumeLayout(false);
			this.stripSpriteManager.PerformLayout();
			this.containerSpriteManager.ContentPanel.ResumeLayout(false);
			this.containerSpriteManager.TopToolStripPanel.ResumeLayout(false);
			this.containerSpriteManager.TopToolStripPanel.PerformLayout();
			this.containerSpriteManager.ResumeLayout(false);
			this.containerSpriteManager.PerformLayout();
			this.splitSpriteList.Panel1.ResumeLayout(false);
			this.splitSpriteList.Panel2.ResumeLayout(false);
			this.splitSpriteList.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip popupSprites;
		private System.Windows.Forms.ToolStripMenuItem menuItemAddSprite;
		private System.Windows.Forms.ToolStripMenuItem menuItemUpdate;
		private System.Windows.Forms.ToolStripMenuItem menuItemDeleteSprites;
		private System.Windows.Forms.ToolStripMenuItem menuItemClearAll;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem menuItemBind;
		private System.Windows.Forms.ToolStripMenuItem menuItemUnbind;
		private System.Windows.Forms.OpenFileDialog dialogOpen;
		private System.Windows.Forms.SaveFileDialog dialogSave;
		private System.Windows.Forms.ContextMenuStrip popupPropGrid;
		private System.Windows.Forms.ToolStripMenuItem menuReset;
		private System.Windows.Forms.ToolStripMenuItem menuItemClone;
		private System.Windows.Forms.ToolStripMenuItem menuFlip;
		private System.Windows.Forms.ToolStripMenuItem menuItemHorizontal;
		private System.Windows.Forms.ToolStripMenuItem menuItemVertical;
		private System.Windows.Forms.ToolStripMenuItem menuItemAutoResize;
		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
		private System.Windows.Forms.TabControl tabSpriteExtra;
		private System.Windows.Forms.TabPage pageProperties;
		private System.Windows.Forms.PropertyGrid gridSpriteProperties;
		private System.Windows.Forms.TabPage pageAnimations;
		private System.Windows.Forms.ToolStrip stripSpriteManager;
		private System.Windows.Forms.ToolStripButton buttonCreateSprite;
		private System.Windows.Forms.ToolStripButton buttonOpenSprite;
		private System.Windows.Forms.ToolStripButton buttonSaveSprite;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton buttonEditSprite;
		private System.Windows.Forms.ToolStripButton buttonRemoveSprites;
		private System.Windows.Forms.ListView listSprites;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ToolStripContainer containerSpriteManager;
		private System.Windows.Forms.SplitContainer splitSpriteList;
		private System.Windows.Forms.ToolStripContainer containerAnimation;
		private System.Windows.Forms.ToolStrip stripAnimation;
		private System.Windows.Forms.ToolStripButton buttonNewAnimation;
		private System.Windows.Forms.ToolStripButton buttonEditAnimation;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton buttonDeleteAnimation;
		private System.Windows.Forms.ContextMenuStrip popupAnimations;
		private System.Windows.Forms.ToolStripMenuItem menuItemAddAnimation;
		private System.Windows.Forms.ToolStripMenuItem menuItemUpdateAnimation;
		private System.Windows.Forms.ToolStripMenuItem menuItemDeleteAnimation;
		private System.Windows.Forms.ToolStripMenuItem menuItemClearAnimations;
		private System.Windows.Forms.Panel panelAnimation;
		private System.Windows.Forms.ListView listAnimations;
		private System.Windows.Forms.ColumnHeader headerAnimationName;
		private System.Windows.Forms.ColumnHeader headerAnimationLength;
		private System.Windows.Forms.ColumnHeader headerLooped;
		private System.Windows.Forms.ToolStripSeparator toolStripButton1;
		private System.Windows.Forms.ToolStripButton buttonAnimationEditor;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuItemAnimationEditor;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripDropDownButton buttonSpriteImport;
		private System.Windows.Forms.ToolStripMenuItem menuItemNvidiaImport;
	}
}
