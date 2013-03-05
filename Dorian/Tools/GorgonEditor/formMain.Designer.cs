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
			this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.itemImport = new System.Windows.Forms.ToolStripMenuItem();
			this.itemExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.itemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.splitEdit = new System.Windows.Forms.SplitContainer();
			this.containerContent = new System.Windows.Forms.ToolStripContainer();
			this.panelContent = new System.Windows.Forms.Panel();
			this.tabDocumentManager = new KRBTabControl.KRBTabControl();
			this.pageItems = new KRBTabControl.TabPageEx();
			this.containerFiles = new System.Windows.Forms.ToolStripContainer();
			this.treeFiles = new Aga.Controls.Tree.TreeViewAdv();
			this.columnFiles = new Aga.Controls.Tree.TreeColumn();
			this._nodeImage = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
			this._nodeText = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.pageProperties = new KRBTabControl.TabPageEx();
			this.propertyItem = new System.Windows.Forms.PropertyGrid();
			this.popupProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.itemResetValue = new System.Windows.Forms.ToolStripMenuItem();
			this.dialogExport = new System.Windows.Forms.SaveFileDialog();
			this.panelEditor = new System.Windows.Forms.Panel();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.stripStatus = new System.Windows.Forms.StatusStrip();
			this.menuMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).BeginInit();
			this.splitEdit.Panel1.SuspendLayout();
			this.splitEdit.Panel2.SuspendLayout();
			this.splitEdit.SuspendLayout();
			this.containerContent.ContentPanel.SuspendLayout();
			this.containerContent.SuspendLayout();
			this.tabDocumentManager.SuspendLayout();
			this.pageItems.SuspendLayout();
			this.containerFiles.ContentPanel.SuspendLayout();
			this.containerFiles.SuspendLayout();
			this.pageProperties.SuspendLayout();
			this.popupProperties.SuspendLayout();
			this.panelEditor.SuspendLayout();
			this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuMain
			// 
			this.menuMain.Dock = System.Windows.Forms.DockStyle.None;
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(1080, 24);
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
			this.itemNew.Name = "itemNew";
			this.itemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.itemNew.Size = new System.Drawing.Size(155, 22);
			this.itemNew.Text = "&New...";
			// 
			// itemOpen
			// 
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
			this.itemImport.Name = "itemImport";
			this.itemImport.Size = new System.Drawing.Size(155, 22);
			this.itemImport.Text = "Import...";
			// 
			// itemExport
			// 
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
			this.splitEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.splitEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitEdit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitEdit.Location = new System.Drawing.Point(0, 0);
			this.splitEdit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.splitEdit.Name = "splitEdit";
			// 
			// splitEdit.Panel1
			// 
			this.splitEdit.Panel1.Controls.Add(this.containerContent);
			// 
			// splitEdit.Panel2
			// 
			this.splitEdit.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.splitEdit.Panel2.Controls.Add(this.tabDocumentManager);
			this.splitEdit.Size = new System.Drawing.Size(1080, 687);
			this.splitEdit.SplitterDistance = 840;
			this.splitEdit.SplitterWidth = 5;
			this.splitEdit.TabIndex = 0;
			// 
			// containerContent
			// 
			// 
			// containerContent.ContentPanel
			// 
			this.containerContent.ContentPanel.Controls.Add(this.panelContent);
			this.containerContent.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.containerContent.ContentPanel.Size = new System.Drawing.Size(840, 654);
			this.containerContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerContent.Location = new System.Drawing.Point(0, 0);
			this.containerContent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.containerContent.Name = "containerContent";
			this.containerContent.Size = new System.Drawing.Size(840, 687);
			this.containerContent.TabIndex = 3;
			this.containerContent.Text = "toolStripContainer1";
			// 
			// panelContent
			// 
			this.panelContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContent.Location = new System.Drawing.Point(0, 0);
			this.panelContent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panelContent.Name = "panelContent";
			this.panelContent.Size = new System.Drawing.Size(840, 654);
			this.panelContent.TabIndex = 0;
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
			this.tabDocumentManager.Size = new System.Drawing.Size(235, 687);
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
			this.pageItems.Margin = new System.Windows.Forms.Padding(3, 116, 3, 116);
			this.pageItems.Name = "pageItems";
			this.pageItems.Size = new System.Drawing.Size(233, 652);
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
			this.containerFiles.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 116, 3, 116);
			this.containerFiles.ContentPanel.Size = new System.Drawing.Size(233, 619);
			this.containerFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerFiles.Location = new System.Drawing.Point(0, 0);
			this.containerFiles.Margin = new System.Windows.Forms.Padding(3, 116, 3, 116);
			this.containerFiles.Name = "containerFiles";
			this.containerFiles.Size = new System.Drawing.Size(233, 652);
			this.containerFiles.TabIndex = 0;
			this.containerFiles.Text = "toolStripContainer1";
			// 
			// treeFiles
			// 
			this.treeFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.treeFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeFiles.Columns.Add(this.columnFiles);
			this.treeFiles.DefaultToolTipProvider = null;
			this.treeFiles.DisplayDraggingNodes = true;
			this.treeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeFiles.DragDropMarkColor = System.Drawing.Color.Black;
			this.treeFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeFiles.ForeColor = System.Drawing.Color.White;
			this.treeFiles.LineColor = System.Drawing.Color.Black;
			this.treeFiles.Location = new System.Drawing.Point(0, 0);
			this.treeFiles.Margin = new System.Windows.Forms.Padding(3, 116, 3, 116);
			this.treeFiles.Model = null;
			this.treeFiles.Name = "treeFiles";
			this.treeFiles.NodeControls.Add(this._nodeImage);
			this.treeFiles.NodeControls.Add(this._nodeText);
			this.treeFiles.SelectedNode = null;
			this.treeFiles.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.Multi;
			this.treeFiles.ShiftFirstNode = true;
			this.treeFiles.ShowLines = false;
			this.treeFiles.Size = new System.Drawing.Size(233, 619);
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
			// pageProperties
			// 
			this.pageProperties.BackColor = System.Drawing.Color.DimGray;
			this.pageProperties.Controls.Add(this.propertyItem);
			this.pageProperties.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageProperties.ImageIndex = 3;
			this.pageProperties.IsClosable = false;
			this.pageProperties.Location = new System.Drawing.Point(1, 1);
			this.pageProperties.Margin = new System.Windows.Forms.Padding(3, 116, 3, 116);
			this.pageProperties.Name = "pageProperties";
			this.pageProperties.Size = new System.Drawing.Size(267, 889);
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
			this.propertyItem.Margin = new System.Windows.Forms.Padding(3, 116, 3, 116);
			this.propertyItem.Name = "propertyItem";
			this.propertyItem.SelectedItemWithFocusBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.propertyItem.SelectedItemWithFocusForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.propertyItem.Size = new System.Drawing.Size(267, 889);
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
			this.panelEditor.Controls.Add(this.toolStripContainer1);
			this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelEditor.Location = new System.Drawing.Point(6, 30);
			this.panelEditor.Name = "panelEditor";
			this.panelEditor.Size = new System.Drawing.Size(1080, 733);
			this.panelEditor.TabIndex = 4;
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.BottomToolStripPanel
			// 
			this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.stripStatus);
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.splitEdit);
			this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1080, 687);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(1080, 733);
			this.toolStripContainer1.TabIndex = 0;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuMain);
			// 
			// stripStatus
			// 
			this.stripStatus.Dock = System.Windows.Forms.DockStyle.None;
			this.stripStatus.Location = new System.Drawing.Point(0, 0);
			this.stripStatus.Name = "stripStatus";
			this.stripStatus.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.stripStatus.Size = new System.Drawing.Size(1080, 22);
			this.stripStatus.TabIndex = 0;
			// 
			// formMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.ClientSize = new System.Drawing.Size(1092, 769);
			this.Controls.Add(this.panelEditor);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "formMain";
			this.Padding = new System.Windows.Forms.Padding(6);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Editor";
			this.Controls.SetChildIndex(this.panelEditor, 0);
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.splitEdit.Panel1.ResumeLayout(false);
			this.splitEdit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).EndInit();
			this.splitEdit.ResumeLayout(false);
			this.containerContent.ContentPanel.ResumeLayout(false);
			this.containerContent.ResumeLayout(false);
			this.containerContent.PerformLayout();
			this.tabDocumentManager.ResumeLayout(false);
			this.pageItems.ResumeLayout(false);
			this.containerFiles.ContentPanel.ResumeLayout(false);
			this.containerFiles.ResumeLayout(false);
			this.containerFiles.PerformLayout();
			this.pageProperties.ResumeLayout(false);
			this.popupProperties.ResumeLayout(false);
			this.panelEditor.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitEdit;
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
		private System.Windows.Forms.ToolStripContainer containerContent;
		internal System.Windows.Forms.Panel panelContent;
		private System.Windows.Forms.Panel panelEditor;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.StatusStrip stripStatus;
	}
}

