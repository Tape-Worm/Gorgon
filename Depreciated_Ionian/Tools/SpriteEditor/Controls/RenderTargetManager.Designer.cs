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
// Created: Sunday, June 10, 2007 3:33:16 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools.Controls
{
	partial class RenderTargetManager
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
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
			this.toolsRenderTargetManager = new System.Windows.Forms.ToolStrip();
			this.buttonNewTarget = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEditTarget = new System.Windows.Forms.ToolStripButton();
			this.buttonDeleteTarget = new System.Windows.Forms.ToolStripButton();
			this.listTargets = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.popupRenderTargetManager = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuPopupItemUpdateTarget = new System.Windows.Forms.ToolStripMenuItem();
			this.menuPopupItemDeleteTarget = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.toolsRenderTargetManager.SuspendLayout();
			this.popupRenderTargetManager.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelManager
			// 
			this.labelManager.Text = "Render Target Manager";
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
			this.ContentPanel.Size = new System.Drawing.Size(464, 402);
			// 
			// toolsRenderTargetManager
			// 
			this.toolsRenderTargetManager.Dock = System.Windows.Forms.DockStyle.None;
			this.toolsRenderTargetManager.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolsRenderTargetManager.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNewTarget,
            this.toolStripSeparator1,
            this.buttonEditTarget,
            this.buttonDeleteTarget});
			this.toolsRenderTargetManager.Location = new System.Drawing.Point(0, 0);
			this.toolsRenderTargetManager.Name = "toolsRenderTargetManager";
			this.toolsRenderTargetManager.Size = new System.Drawing.Size(464, 25);
			this.toolsRenderTargetManager.Stretch = true;
			this.toolsRenderTargetManager.TabIndex = 1;
			// 
			// buttonNewTarget
			// 
			this.buttonNewTarget.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNewTarget.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_dirty;
			this.buttonNewTarget.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNewTarget.Name = "buttonNewTarget";
			this.buttonNewTarget.Size = new System.Drawing.Size(23, 22);
			this.buttonNewTarget.Text = "buttonNewTarget";
			this.buttonNewTarget.ToolTipText = "Create a new render target.";
			this.buttonNewTarget.Click += new System.EventHandler(this.buttonNewTarget_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonEditTarget
			// 
			this.buttonEditTarget.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditTarget.Enabled = false;
			this.buttonEditTarget.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.buttonEditTarget.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditTarget.Name = "buttonEditTarget";
			this.buttonEditTarget.Size = new System.Drawing.Size(23, 22);
			this.buttonEditTarget.Text = "toolStripButton3";
			this.buttonEditTarget.ToolTipText = "Modify the selected render target.";
			this.buttonEditTarget.Click += new System.EventHandler(this.buttonEditTarget_Click);
			// 
			// buttonDeleteTarget
			// 
			this.buttonDeleteTarget.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonDeleteTarget.Enabled = false;
			this.buttonDeleteTarget.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.buttonDeleteTarget.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonDeleteTarget.Name = "buttonDeleteTarget";
			this.buttonDeleteTarget.Size = new System.Drawing.Size(23, 22);
			this.buttonDeleteTarget.Text = "buttonDelete";
			this.buttonDeleteTarget.ToolTipText = "Delete the selected render targets.";
			this.buttonDeleteTarget.Click += new System.EventHandler(this.buttonDeleteTarget_Click);
			// 
			// listTargets
			// 
			this.listTargets.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listTargets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
			this.listTargets.ContextMenuStrip = this.popupRenderTargetManager;
			this.listTargets.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listTargets.FullRowSelect = true;
			this.listTargets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listTargets.HideSelection = false;
			this.listTargets.LabelEdit = true;
			this.listTargets.Location = new System.Drawing.Point(0, 0);
			this.listTargets.Name = "listTargets";
			this.listTargets.Size = new System.Drawing.Size(464, 383);
			this.listTargets.TabIndex = 0;
			this.listTargets.UseCompatibleStateImageBehavior = false;
			this.listTargets.View = System.Windows.Forms.View.Details;
			this.listTargets.SelectedIndexChanged += new System.EventHandler(this.listTargets_SelectedIndexChanged);
			this.listTargets.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listTargets_KeyDown);
			this.listTargets.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listTargets_AfterLabelEdit);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Dimensions";
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Format";
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Using depth buffer?";
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Using stencil buffer?";
			// 
			// popupRenderTargetManager
			// 
			this.popupRenderTargetManager.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPopupItemUpdateTarget,
            this.menuPopupItemDeleteTarget});
			this.popupRenderTargetManager.Name = "popupRenderTargetManager";
			this.popupRenderTargetManager.Size = new System.Drawing.Size(201, 48);
			// 
			// menuPopupItemUpdateTarget
			// 
			this.menuPopupItemUpdateTarget.Enabled = false;
			this.menuPopupItemUpdateTarget.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.menuPopupItemUpdateTarget.Name = "menuPopupItemUpdateTarget";
			this.menuPopupItemUpdateTarget.Size = new System.Drawing.Size(200, 22);
			this.menuPopupItemUpdateTarget.Text = "Modify render target...";
			this.menuPopupItemUpdateTarget.Click += new System.EventHandler(this.buttonEditTarget_Click);
			// 
			// menuPopupItemDeleteTarget
			// 
			this.menuPopupItemDeleteTarget.Enabled = false;
			this.menuPopupItemDeleteTarget.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.menuPopupItemDeleteTarget.Name = "menuPopupItemDeleteTarget";
			this.menuPopupItemDeleteTarget.Size = new System.Drawing.Size(200, 22);
			this.menuPopupItemDeleteTarget.Text = "Delete render target(s)...";
			this.menuPopupItemDeleteTarget.Click += new System.EventHandler(this.buttonDeleteTarget_Click);
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.listTargets);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(464, 383);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.LeftToolStripPanelVisible = false;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 19);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.RightToolStripPanelVisible = false;
			this.toolStripContainer1.Size = new System.Drawing.Size(464, 408);
			this.toolStripContainer1.TabIndex = 4;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolsRenderTargetManager);
			// 
			// RenderTargetManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "RenderTargetManager";
			this.Controls.SetChildIndex(this.toolStripContainer1, 0);
			this.toolsRenderTargetManager.ResumeLayout(false);
			this.toolsRenderTargetManager.PerformLayout();
			this.popupRenderTargetManager.ResumeLayout(false);
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
		private System.Windows.Forms.ToolStrip toolsRenderTargetManager;
		private System.Windows.Forms.ToolStripButton buttonNewTarget;
		private System.Windows.Forms.ToolStripButton buttonEditTarget;
		private System.Windows.Forms.ToolStripButton buttonDeleteTarget;
		private System.Windows.Forms.ListView listTargets;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ContextMenuStrip popupRenderTargetManager;
		private System.Windows.Forms.ToolStripMenuItem menuPopupItemUpdateTarget;
		private System.Windows.Forms.ToolStripMenuItem menuPopupItemDeleteTarget;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

	}
}
