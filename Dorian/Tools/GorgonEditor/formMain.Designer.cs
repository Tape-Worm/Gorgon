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

namespace GorgonLibrary.GorgonEditor
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
			this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.itemImport = new System.Windows.Forms.ToolStripMenuItem();
			this.itemExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.itemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.splitEdit = new System.Windows.Forms.SplitContainer();
			this.panelEditor = new System.Windows.Forms.Panel();
			this.tabDocuments = new KRBTabControl.KRBTabControl();
			this.imageFiles = new System.Windows.Forms.ImageList(this.components);
			this.tabDocumentManager = new KRBTabControl.KRBTabControl();
			this.pageFiles = new KRBTabControl.TabPageEx();
			this.containerFiles = new System.Windows.Forms.ToolStripContainer();
			this.treeFiles = new Aga.Controls.Tree.TreeViewAdv();
			this.columnFiles = new Aga.Controls.Tree.TreeColumn();
			this._nodeImage = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
			this._nodeText = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.stripFiles = new System.Windows.Forms.ToolStrip();
			this.buttonNewItem = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemNewFont = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonEditItem = new System.Windows.Forms.ToolStripButton();
			this.buttonDeleteItem = new System.Windows.Forms.ToolStripButton();
			this.pageProperties = new KRBTabControl.TabPageEx();
			this.propertyItem = new System.Windows.Forms.PropertyGrid();
			this.popupProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.itemResetValue = new System.Windows.Forms.ToolStripMenuItem();
			this.menuMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).BeginInit();
			this.splitEdit.Panel1.SuspendLayout();
			this.splitEdit.Panel2.SuspendLayout();
			this.splitEdit.SuspendLayout();
			this.panelEditor.SuspendLayout();
			this.tabDocumentManager.SuspendLayout();
			this.pageFiles.SuspendLayout();
			this.containerFiles.ContentPanel.SuspendLayout();
			this.containerFiles.TopToolStripPanel.SuspendLayout();
			this.containerFiles.SuspendLayout();
			this.stripFiles.SuspendLayout();
			this.pageProperties.SuspendLayout();
			this.popupProperties.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuMain
			// 
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(944, 24);
			this.menuMain.TabIndex = 0;
			this.menuMain.Text = "menuStrip1";
			// 
			// menuFile
			// 
			this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNew,
            this.itemOpen,
            this.toolStripSeparator1,
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
			this.itemNew.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.new_project_16x16;
			this.itemNew.Name = "itemNew";
			this.itemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.itemNew.Size = new System.Drawing.Size(155, 22);
			this.itemNew.Text = "&New...";
			// 
			// itemOpen
			// 
			this.itemOpen.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.open_project_16x16;
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
			// itemSave
			// 
			this.itemSave.Name = "itemSave";
			this.itemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.itemSave.Size = new System.Drawing.Size(155, 22);
			this.itemSave.Text = "&Save...";
			// 
			// itemSaveAs
			// 
			this.itemSaveAs.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.save_as_16x16;
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
			this.itemImport.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.import_16x16;
			this.itemImport.Name = "itemImport";
			this.itemImport.Size = new System.Drawing.Size(155, 22);
			this.itemImport.Text = "Import...";
			this.itemImport.Click += new System.EventHandler(this.itemImport_Click);
			// 
			// itemExport
			// 
			this.itemExport.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.export_16x16;
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
			// splitEdit
			// 
			this.splitEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitEdit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitEdit.Location = new System.Drawing.Point(0, 24);
			this.splitEdit.Name = "splitEdit";
			// 
			// splitEdit.Panel1
			// 
			this.splitEdit.Panel1.Controls.Add(this.panelEditor);
			// 
			// splitEdit.Panel2
			// 
			this.splitEdit.Panel2.Controls.Add(this.tabDocumentManager);
			this.splitEdit.Size = new System.Drawing.Size(944, 589);
			this.splitEdit.SplitterDistance = 715;
			this.splitEdit.TabIndex = 0;
			// 
			// panelEditor
			// 
			this.panelEditor.Controls.Add(this.tabDocuments);
			this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelEditor.Location = new System.Drawing.Point(0, 0);
			this.panelEditor.Name = "panelEditor";
			this.panelEditor.Size = new System.Drawing.Size(715, 589);
			this.panelEditor.TabIndex = 0;
			// 
			// tabDocuments
			// 
			this.tabDocuments.AllowDrop = true;
			this.tabDocuments.BackgroundColor = System.Drawing.Color.DimGray;
			this.tabDocuments.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabDocuments.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.tabDocuments.CaptionButtons.InactiveCaptionButtonsColor = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabDocuments.GradientCaption.ActiveCaptionColorEnd = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.GradientCaption.ActiveCaptionColorStart = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.GradientCaption.ActiveCaptionFontStyle = System.Drawing.FontStyle.Bold;
			this.tabDocuments.GradientCaption.ActiveCaptionTextColor = System.Drawing.Color.White;
			this.tabDocuments.GradientCaption.InactiveCaptionColorEnd = System.Drawing.SystemColors.ControlDarkDark;
			this.tabDocuments.GradientCaption.InactiveCaptionColorStart = System.Drawing.SystemColors.ControlDarkDark;
			this.tabDocuments.GradientCaption.InactiveCaptionTextColor = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.ImageList = this.imageFiles;
			this.tabDocuments.IsCaptionVisible = false;
			this.tabDocuments.IsDocumentTabStyle = true;
			this.tabDocuments.IsDrawTabSeparator = true;
			this.tabDocuments.IsUserInteraction = false;
			this.tabDocuments.ItemSize = new System.Drawing.Size(0, 26);
			this.tabDocuments.Location = new System.Drawing.Point(0, 0);
			this.tabDocuments.Name = "tabDocuments";
			this.tabDocuments.Size = new System.Drawing.Size(715, 589);
			this.tabDocuments.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.tabDocuments.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.tabDocuments.TabHOffset = 2;
			this.tabDocuments.TabIndex = 0;
			this.tabDocuments.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
			this.tabDocuments.TabPageClosing += new System.EventHandler<KRBTabControl.KRBTabControl.SelectedIndexChangingEventArgs>(this.tabDocuments_TabPageClosing);
			this.tabDocuments.ContextMenuShown += new System.EventHandler<KRBTabControl.KRBTabControl.ContextMenuShownEventArgs>(this.tabDocuments_ContextMenuShown);
			this.tabDocuments.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabDocuments_Selected);
			// 
			// imageFiles
			// 
			this.imageFiles.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageFiles.ImageStream")));
			this.imageFiles.TransparentColor = System.Drawing.Color.Transparent;
			this.imageFiles.Images.SetKeyName(0, "gorgon_icon");
			this.imageFiles.Images.SetKeyName(1, "font_icon");
			this.imageFiles.Images.SetKeyName(2, "files_tab");
			this.imageFiles.Images.SetKeyName(3, "properties_tab");
			// 
			// tabDocumentManager
			// 
			this.tabDocumentManager.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabDocumentManager.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
			this.tabDocumentManager.AllowDrop = true;
			this.tabDocumentManager.BackgroundColor = System.Drawing.Color.DimGray;
			this.tabDocumentManager.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabDocumentManager.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.tabDocumentManager.Controls.Add(this.pageFiles);
			this.tabDocumentManager.Controls.Add(this.pageProperties);
			this.tabDocumentManager.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabDocumentManager.ImageList = this.imageFiles;
			this.tabDocumentManager.IsCaptionVisible = false;
			this.tabDocumentManager.IsDocumentTabStyle = true;
			this.tabDocumentManager.IsDrawTabSeparator = true;
			this.tabDocumentManager.IsUserInteraction = false;
			this.tabDocumentManager.ItemSize = new System.Drawing.Size(0, 28);
			this.tabDocumentManager.Location = new System.Drawing.Point(0, 0);
			this.tabDocumentManager.Name = "tabDocumentManager";
			this.tabDocumentManager.SelectedIndex = 0;
			this.tabDocumentManager.Size = new System.Drawing.Size(225, 589);
			this.tabDocumentManager.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.tabDocumentManager.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.tabDocumentManager.TabHOffset = -1;
			this.tabDocumentManager.TabIndex = 1;
			this.tabDocumentManager.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
			// 
			// pageFiles
			// 
			this.pageFiles.BackColor = System.Drawing.Color.DimGray;
			this.pageFiles.Controls.Add(this.containerFiles);
			this.pageFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageFiles.ImageIndex = 2;
			this.pageFiles.IsClosable = false;
			this.pageFiles.Location = new System.Drawing.Point(1, 1);
			this.pageFiles.Name = "pageFiles";
			this.pageFiles.Size = new System.Drawing.Size(223, 554);
			this.pageFiles.TabIndex = 1;
			this.pageFiles.Text = "Files";
			// 
			// containerFiles
			// 
			// 
			// containerFiles.ContentPanel
			// 
			this.containerFiles.ContentPanel.Controls.Add(this.treeFiles);
			this.containerFiles.ContentPanel.Size = new System.Drawing.Size(223, 529);
			this.containerFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerFiles.Location = new System.Drawing.Point(0, 0);
			this.containerFiles.Name = "containerFiles";
			this.containerFiles.Size = new System.Drawing.Size(223, 554);
			this.containerFiles.TabIndex = 0;
			this.containerFiles.Text = "toolStripContainer1";
			// 
			// containerFiles.TopToolStripPanel
			// 
			this.containerFiles.TopToolStripPanel.Controls.Add(this.stripFiles);
			// 
			// treeFiles
			// 
			this.treeFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.treeFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeFiles.Columns.Add(this.columnFiles);
			this.treeFiles.DefaultToolTipProvider = null;
			this.treeFiles.DisplayDraggingNodes = true;
			this.treeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeFiles.DragDropMarkColor = System.Drawing.Color.Black;
			this.treeFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeFiles.ForeColor = System.Drawing.Color.White;
			this.treeFiles.LineColor = System.Drawing.Color.Black;
			this.treeFiles.Location = new System.Drawing.Point(0, 0);
			this.treeFiles.Model = null;
			this.treeFiles.Name = "treeFiles";
			this.treeFiles.NodeControls.Add(this._nodeImage);
			this.treeFiles.NodeControls.Add(this._nodeText);
			this.treeFiles.SelectedNode = null;
			this.treeFiles.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.Multi;
			this.treeFiles.ShiftFirstNode = true;
			this.treeFiles.ShowLines = false;
			this.treeFiles.Size = new System.Drawing.Size(223, 529);
			this.treeFiles.TabIndex = 0;
			// 
			// columnFiles
			// 
			this.columnFiles.Header = "";
			this.columnFiles.SortOrder = System.Windows.Forms.SortOrder.None;
			this.columnFiles.TooltipText = null;
			// 
			// _nodeImage
			// 
			this._nodeImage.DataPropertyName = "Image";
			this._nodeImage.LeftMargin = 1;
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
			// stripFiles
			// 
			this.stripFiles.Dock = System.Windows.Forms.DockStyle.None;
			this.stripFiles.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNewItem,
            this.buttonEditItem,
            this.buttonDeleteItem});
			this.stripFiles.Location = new System.Drawing.Point(0, 0);
			this.stripFiles.Name = "stripFiles";
			this.stripFiles.Size = new System.Drawing.Size(223, 25);
			this.stripFiles.Stretch = true;
			this.stripFiles.TabIndex = 0;
			// 
			// buttonNewItem
			// 
			this.buttonNewItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNewItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNewFont});
			this.buttonNewItem.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.new_item_16x16;
			this.buttonNewItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNewItem.Name = "buttonNewItem";
			this.buttonNewItem.Size = new System.Drawing.Size(29, 22);
			this.buttonNewItem.Text = "New item";
			this.buttonNewItem.ToolTipText = "New item...";
			// 
			// itemNewFont
			// 
			this.itemNewFont.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.font_16x16;
			this.itemNewFont.Name = "itemNewFont";
			this.itemNewFont.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
			this.itemNewFont.Size = new System.Drawing.Size(147, 22);
			this.itemNewFont.Text = "Font...";
			// 
			// buttonEditItem
			// 
			this.buttonEditItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditItem.Enabled = false;
			this.buttonEditItem.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.edit_16x16;
			this.buttonEditItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditItem.Name = "buttonEditItem";
			this.buttonEditItem.Size = new System.Drawing.Size(23, 22);
			this.buttonEditItem.Text = "Edit";
			// 
			// buttonDeleteItem
			// 
			this.buttonDeleteItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonDeleteItem.Enabled = false;
			this.buttonDeleteItem.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.delete_item_16x16;
			this.buttonDeleteItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonDeleteItem.Name = "buttonDeleteItem";
			this.buttonDeleteItem.Size = new System.Drawing.Size(23, 22);
			this.buttonDeleteItem.Text = "Delete";
			this.buttonDeleteItem.Click += new System.EventHandler(this.buttonDeleteItem_Click);
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
			this.pageProperties.Size = new System.Drawing.Size(223, 554);
			this.pageProperties.TabIndex = 0;
			this.pageProperties.Text = "Properties";
			// 
			// propertyItem
			// 
			this.propertyItem.BackColor = System.Drawing.Color.DimGray;
			this.propertyItem.CategoryForeColor = System.Drawing.Color.White;
			this.propertyItem.CommandsActiveLinkColor = System.Drawing.Color.Lavender;
			this.propertyItem.CommandsDisabledLinkColor = System.Drawing.Color.Black;
			this.propertyItem.CommandsForeColor = System.Drawing.Color.White;
			this.propertyItem.CommandsLinkColor = System.Drawing.Color.SteelBlue;
			this.propertyItem.ContextMenuStrip = this.popupProperties;
			this.propertyItem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.propertyItem.HelpBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.propertyItem.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.propertyItem.Location = new System.Drawing.Point(0, 0);
			this.propertyItem.Name = "propertyItem";
			this.propertyItem.Size = new System.Drawing.Size(223, 554);
			this.propertyItem.TabIndex = 0;
			this.propertyItem.ToolbarVisible = false;
			this.propertyItem.ViewBackColor = System.Drawing.Color.DimGray;
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
			// formMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.ClientSize = new System.Drawing.Size(944, 613);
			this.Controls.Add(this.splitEdit);
			this.Controls.Add(this.menuMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "formMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Editor";
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.splitEdit.Panel1.ResumeLayout(false);
			this.splitEdit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).EndInit();
			this.splitEdit.ResumeLayout(false);
			this.panelEditor.ResumeLayout(false);
			this.tabDocumentManager.ResumeLayout(false);
			this.pageFiles.ResumeLayout(false);
			this.containerFiles.ContentPanel.ResumeLayout(false);
			this.containerFiles.TopToolStripPanel.ResumeLayout(false);
			this.containerFiles.TopToolStripPanel.PerformLayout();
			this.containerFiles.ResumeLayout(false);
			this.containerFiles.PerformLayout();
			this.stripFiles.ResumeLayout(false);
			this.stripFiles.PerformLayout();
			this.pageProperties.ResumeLayout(false);
			this.popupProperties.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitEdit;
		private System.Windows.Forms.PropertyGrid propertyItem;
		private System.Windows.Forms.Panel panelEditor;
		private KRBTabControl.KRBTabControl tabDocuments;
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
		private KRBTabControl.TabPageEx pageFiles;
		private System.Windows.Forms.ImageList imageFiles;
		private System.Windows.Forms.ToolStripContainer containerFiles;
		private System.Windows.Forms.ToolStrip stripFiles;
		private System.Windows.Forms.ToolStripDropDownButton buttonNewItem;
		private System.Windows.Forms.ToolStripMenuItem itemNewFont;
		private System.Windows.Forms.ToolStripButton buttonEditItem;
		private System.Windows.Forms.ToolStripButton buttonDeleteItem;
		private Aga.Controls.Tree.TreeViewAdv treeFiles;
		private Aga.Controls.Tree.TreeColumn columnFiles;
		private Aga.Controls.Tree.NodeControls.NodeTextBox _nodeText;
		private Aga.Controls.Tree.NodeControls.NodeStateIcon _nodeImage;
		private System.Windows.Forms.ToolStripMenuItem itemImport;
		private System.Windows.Forms.ToolStripMenuItem itemExport;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
	}
}

