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
			this.itemNewProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.itemNewFont = new System.Windows.Forms.ToolStripMenuItem();
			this.itemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSaveAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.itemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.splitEdit = new System.Windows.Forms.SplitContainer();
			this.panelEditor = new System.Windows.Forms.Panel();
			this.tabDocuments = new KRBTabControl.KRBTabControl();
			this.tabDocumentManager = new KRBTabControl.KRBTabControl();
			this.pageFiles = new KRBTabControl.TabPageEx();
			this.treeFiles = new System.Windows.Forms.TreeView();
			this.imageFiles = new System.Windows.Forms.ImageList(this.components);
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
            this.itemSaveAll,
            this.toolStripMenuItem5,
            this.itemExit});
			this.menuFile.Name = "menuFile";
			this.menuFile.Size = new System.Drawing.Size(37, 20);
			this.menuFile.Text = "File";
			// 
			// itemNew
			// 
			this.itemNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNewProject,
            this.toolStripMenuItem4,
            this.itemNewFont});
			this.itemNew.Name = "itemNew";
			this.itemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.itemNew.Size = new System.Drawing.Size(155, 22);
			this.itemNew.Text = "&New...";
			// 
			// itemNewProject
			// 
			this.itemNewProject.Name = "itemNewProject";
			this.itemNewProject.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.P)));
			this.itemNewProject.Size = new System.Drawing.Size(184, 22);
			this.itemNewProject.Text = "&Project...";
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(181, 6);
			// 
			// itemNewFont
			// 
			this.itemNewFont.Name = "itemNewFont";
			this.itemNewFont.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.F)));
			this.itemNewFont.Size = new System.Drawing.Size(184, 22);
			this.itemNewFont.Text = "&Font...";
			this.itemNewFont.Click += new System.EventHandler(this.itemNewFont_Click);
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
			this.itemSave.Size = new System.Drawing.Size(155, 22);
			this.itemSave.Text = "&Save...";
			// 
			// itemSaveAs
			// 
			this.itemSaveAs.Name = "itemSaveAs";
			this.itemSaveAs.Size = new System.Drawing.Size(155, 22);
			this.itemSaveAs.Text = "Save &as...";
			// 
			// itemSaveAll
			// 
			this.itemSaveAll.Name = "itemSaveAll";
			this.itemSaveAll.Size = new System.Drawing.Size(155, 22);
			this.itemSaveAll.Text = "Save a&ll";
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(152, 6);
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
			this.tabDocumentManager.IsCaptionVisible = false;
			this.tabDocumentManager.IsDocumentTabStyle = true;
			this.tabDocumentManager.IsDrawTabSeparator = true;
			this.tabDocumentManager.IsUserInteraction = false;
			this.tabDocumentManager.ItemSize = new System.Drawing.Size(0, 24);
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
			this.pageFiles.Controls.Add(this.treeFiles);
			this.pageFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageFiles.IsClosable = false;
			this.pageFiles.Location = new System.Drawing.Point(1, 1);
			this.pageFiles.Name = "pageFiles";
			this.pageFiles.Size = new System.Drawing.Size(223, 558);
			this.pageFiles.TabIndex = 1;
			this.pageFiles.Text = "Files";
			// 
			// treeFiles
			// 
			this.treeFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeFiles.ImageIndex = 4;
			this.treeFiles.ImageList = this.imageFiles;
			this.treeFiles.Location = new System.Drawing.Point(0, 0);
			this.treeFiles.Name = "treeFiles";
			this.treeFiles.SelectedImageIndex = 4;
			this.treeFiles.ShowLines = false;
			this.treeFiles.ShowRootLines = false;
			this.treeFiles.Size = new System.Drawing.Size(223, 558);
			this.treeFiles.TabIndex = 0;
			// 
			// imageFiles
			// 
			this.imageFiles.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageFiles.ImageStream")));
			this.imageFiles.TransparentColor = System.Drawing.Color.Transparent;
			this.imageFiles.Images.SetKeyName(0, "folder_closed_16x16.png");
			this.imageFiles.Images.SetKeyName(1, "folder_open_16x16.png");
			this.imageFiles.Images.SetKeyName(2, "font_16x16.png");
			this.imageFiles.Images.SetKeyName(3, "unknown_document_16x16.png");
			this.imageFiles.Images.SetKeyName(4, "GorgonProject_16x16.png");
			this.imageFiles.Images.SetKeyName(5, "image_16x16.png");
			// 
			// pageProperties
			// 
			this.pageProperties.BackColor = System.Drawing.Color.DimGray;
			this.pageProperties.Controls.Add(this.propertyItem);
			this.pageProperties.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageProperties.IsClosable = false;
			this.pageProperties.Location = new System.Drawing.Point(1, 1);
			this.pageProperties.Name = "pageProperties";
			this.pageProperties.Size = new System.Drawing.Size(223, 558);
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
			this.propertyItem.Size = new System.Drawing.Size(223, 558);
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
		private System.Windows.Forms.ToolStripMenuItem itemNewProject;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem itemNewFont;
		private System.Windows.Forms.ToolStripMenuItem itemOpen;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem itemSave;
		private System.Windows.Forms.ToolStripMenuItem itemSaveAs;
		private System.Windows.Forms.ToolStripMenuItem itemSaveAll;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem itemExit;
		private KRBTabControl.KRBTabControl tabDocumentManager;
		private KRBTabControl.TabPageEx pageProperties;
		private KRBTabControl.TabPageEx pageFiles;
		private System.Windows.Forms.TreeView treeFiles;
		private System.Windows.Forms.ImageList imageFiles;
	}
}

