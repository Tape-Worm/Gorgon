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
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.itemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.itemNewProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.itemNewFont = new System.Windows.Forms.ToolStripMenuItem();
			this.itemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.projectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.fontToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSaveProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.itemSaveFont = new System.Windows.Forms.ToolStripMenuItem();
			this.splitEdit = new System.Windows.Forms.SplitContainer();
			this.panelEditor = new System.Windows.Forms.Panel();
			this.tabDocuments = new KRBTabControl.KRBTabControl();
			this.propertyItem = new System.Windows.Forms.PropertyGrid();
			this.popupProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.itemResetValue = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.menuMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).BeginInit();
			this.splitEdit.Panel1.SuspendLayout();
			this.splitEdit.Panel2.SuspendLayout();
			this.splitEdit.SuspendLayout();
			this.panelEditor.SuspendLayout();
			this.popupProperties.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitMain
			// 
			this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitMain.IsSplitterFixed = true;
			this.splitMain.Location = new System.Drawing.Point(0, 0);
			this.splitMain.Name = "splitMain";
			// 
			// splitMain.Panel1
			// 
			this.splitMain.Panel1.BackColor = System.Drawing.Color.DimGray;
			this.splitMain.Panel1.Controls.Add(this.menuMain);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.Controls.Add(this.splitEdit);
			this.splitMain.Size = new System.Drawing.Size(944, 613);
			this.splitMain.SplitterDistance = 110;
			this.splitMain.SplitterWidth = 1;
			this.splitMain.TabIndex = 1;
			// 
			// menuMain
			// 
			this.menuMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNew,
            this.itemOpen,
            this.itemSave});
			this.menuMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(110, 613);
			this.menuMain.TabIndex = 0;
			this.menuMain.Text = "menuStrip1";
			// 
			// itemNew
			// 
			this.itemNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNewProject,
            this.toolStripMenuItem1,
            this.itemNewFont});
			this.itemNew.Name = "itemNew";
			this.itemNew.Size = new System.Drawing.Size(103, 19);
			this.itemNew.Text = "&New";
			this.itemNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// itemNewProject
			// 
			this.itemNewProject.Name = "itemNewProject";
			this.itemNewProject.Size = new System.Drawing.Size(152, 22);
			this.itemNewProject.Text = "&Project...";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
			// 
			// itemNewFont
			// 
			this.itemNewFont.Name = "itemNewFont";
			this.itemNewFont.Size = new System.Drawing.Size(152, 22);
			this.itemNewFont.Text = "Font...";
			this.itemNewFont.Click += new System.EventHandler(this.itemNewFont_Click);
			// 
			// itemOpen
			// 
			this.itemOpen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem1,
            this.toolStripMenuItem2,
            this.fontToolStripMenuItem1});
			this.itemOpen.Name = "itemOpen";
			this.itemOpen.Size = new System.Drawing.Size(103, 19);
			this.itemOpen.Text = "&Open";
			this.itemOpen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// projectToolStripMenuItem1
			// 
			this.projectToolStripMenuItem1.Name = "projectToolStripMenuItem1";
			this.projectToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
			this.projectToolStripMenuItem1.Text = "&Project...";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(149, 6);
			// 
			// fontToolStripMenuItem1
			// 
			this.fontToolStripMenuItem1.Name = "fontToolStripMenuItem1";
			this.fontToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
			this.fontToolStripMenuItem1.Text = "&Font...";
			// 
			// itemSave
			// 
			this.itemSave.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemSaveProject,
            this.toolStripMenuItem3,
            this.itemSaveFont});
			this.itemSave.Enabled = false;
			this.itemSave.Name = "itemSave";
			this.itemSave.Size = new System.Drawing.Size(103, 19);
			this.itemSave.Text = "&Save";
			this.itemSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// itemSaveProject
			// 
			this.itemSaveProject.Name = "itemSaveProject";
			this.itemSaveProject.Size = new System.Drawing.Size(152, 22);
			this.itemSaveProject.Text = "&Project...";
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(149, 6);
			// 
			// itemSaveFont
			// 
			this.itemSaveFont.Enabled = false;
			this.itemSaveFont.Name = "itemSaveFont";
			this.itemSaveFont.Size = new System.Drawing.Size(152, 22);
			this.itemSaveFont.Text = "&Font...";
			// 
			// splitEdit
			// 
			this.splitEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitEdit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitEdit.Location = new System.Drawing.Point(0, 0);
			this.splitEdit.Name = "splitEdit";
			// 
			// splitEdit.Panel1
			// 
			this.splitEdit.Panel1.Controls.Add(this.panelEditor);
			this.splitEdit.Panel1.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// splitEdit.Panel2
			// 
			this.splitEdit.Panel2.Controls.Add(this.propertyItem);
			this.splitEdit.Size = new System.Drawing.Size(833, 613);
			this.splitEdit.SplitterDistance = 604;
			this.splitEdit.TabIndex = 0;
			// 
			// panelEditor
			// 
			this.panelEditor.Controls.Add(this.tabDocuments);
			this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelEditor.Location = new System.Drawing.Point(3, 0);
			this.panelEditor.Name = "panelEditor";
			this.panelEditor.Size = new System.Drawing.Size(601, 613);
			this.panelEditor.TabIndex = 0;
			// 
			// tabDocuments
			// 
			this.tabDocuments.AllowDrop = true;
			this.tabDocuments.BackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
			this.tabDocuments.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabDocuments.BorderColor = System.Drawing.SystemColors.ControlDark;
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
			this.tabDocuments.Size = new System.Drawing.Size(601, 613);
			this.tabDocuments.TabGradient.ColorEnd = System.Drawing.SystemColors.Control;
			this.tabDocuments.TabGradient.ColorStart = System.Drawing.SystemColors.Control;
			this.tabDocuments.TabHOffset = 2;
			this.tabDocuments.TabIndex = 0;
			this.tabDocuments.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
			this.tabDocuments.TabPageClosing += new System.EventHandler<KRBTabControl.KRBTabControl.SelectedIndexChangingEventArgs>(this.tabDocuments_TabPageClosing);
			this.tabDocuments.ContextMenuShown += new System.EventHandler<KRBTabControl.KRBTabControl.ContextMenuShownEventArgs>(this.tabDocuments_ContextMenuShown);
			this.tabDocuments.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabDocuments_Selected);
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
			this.propertyItem.HelpBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.propertyItem.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.propertyItem.Location = new System.Drawing.Point(0, 0);
			this.propertyItem.Name = "propertyItem";
			this.propertyItem.Size = new System.Drawing.Size(225, 613);
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
			this.popupProperties.Size = new System.Drawing.Size(153, 48);
			this.popupProperties.Opening += new System.ComponentModel.CancelEventHandler(this.popupProperties_Opening);
			// 
			// itemResetValue
			// 
			this.itemResetValue.Name = "itemResetValue";
			this.itemResetValue.Size = new System.Drawing.Size(152, 22);
			this.itemResetValue.Text = "&Reset Value";
			this.itemResetValue.Click += new System.EventHandler(this.itemResetValue_Click);
			// 
			// formMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.ClientSize = new System.Drawing.Size(944, 613);
			this.Controls.Add(this.splitMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "formMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Editor";
			this.splitMain.Panel1.ResumeLayout(false);
			this.splitMain.Panel1.PerformLayout();
			this.splitMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
			this.splitMain.ResumeLayout(false);
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.splitEdit.Panel1.ResumeLayout(false);
			this.splitEdit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).EndInit();
			this.splitEdit.ResumeLayout(false);
			this.panelEditor.ResumeLayout(false);
			this.popupProperties.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitMain;
		private System.Windows.Forms.SplitContainer splitEdit;
		private System.Windows.Forms.PropertyGrid propertyItem;
		private System.Windows.Forms.Panel panelEditor;
		private KRBTabControl.KRBTabControl tabDocuments;
		private System.Windows.Forms.MenuStrip menuMain;
		private System.Windows.Forms.ToolStripMenuItem itemNew;
		private System.Windows.Forms.ToolStripMenuItem itemNewProject;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem itemNewFont;
		private System.Windows.Forms.ToolStripMenuItem itemOpen;
		private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem fontToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem itemSave;
		private System.Windows.Forms.ToolStripMenuItem itemSaveProject;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem itemSaveFont;
		private System.Windows.Forms.ContextMenuStrip popupProperties;
		private System.Windows.Forms.ToolStripMenuItem itemResetValue;
	}
}

