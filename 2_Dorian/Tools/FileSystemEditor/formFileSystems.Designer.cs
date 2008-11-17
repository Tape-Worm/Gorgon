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
// Created: Tuesday, April 24, 2007 2:32:43 AM
// 
#endregion

namespace GorgonLibrary.FileSystems.Tools
{
	partial class formFileSystems
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formFileSystems));
			this.stripPlugIns = new System.Windows.Forms.ToolStrip();
			this.buttonAddPlugIn = new System.Windows.Forms.ToolStripButton();
			this.buttonRemovePlugIn = new System.Windows.Forms.ToolStripButton();
			this.containerPlugIns = new System.Windows.Forms.ToolStripContainer();
			this.listPlugIns = new System.Windows.Forms.ListView();
			this.headerName = new System.Windows.Forms.ColumnHeader();
			this.headerDescription = new System.Windows.Forms.ColumnHeader();
			this.dialogPlugInPath = new System.Windows.Forms.OpenFileDialog();
			this.statusPlugIns = new System.Windows.Forms.StatusStrip();
			this.labelDescription = new System.Windows.Forms.ToolStripStatusLabel();
			this.stripPlugIns.SuspendLayout();
			this.containerPlugIns.BottomToolStripPanel.SuspendLayout();
			this.containerPlugIns.ContentPanel.SuspendLayout();
			this.containerPlugIns.TopToolStripPanel.SuspendLayout();
			this.containerPlugIns.SuspendLayout();
			this.statusPlugIns.SuspendLayout();
			this.SuspendLayout();
			// 
			// stripPlugIns
			// 
			this.stripPlugIns.AutoSize = false;
			this.stripPlugIns.Dock = System.Windows.Forms.DockStyle.None;
			this.stripPlugIns.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripPlugIns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddPlugIn,
            this.buttonRemovePlugIn});
			this.stripPlugIns.Location = new System.Drawing.Point(0, 0);
			this.stripPlugIns.Name = "stripPlugIns";
			this.stripPlugIns.Size = new System.Drawing.Size(528, 25);
			this.stripPlugIns.Stretch = true;
			this.stripPlugIns.TabIndex = 0;
			this.stripPlugIns.Text = "toolStrip1";
			// 
			// buttonAddPlugIn
			// 
			this.buttonAddPlugIn.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.add;
			this.buttonAddPlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonAddPlugIn.Name = "buttonAddPlugIn";
			this.buttonAddPlugIn.Size = new System.Drawing.Size(91, 22);
			this.buttonAddPlugIn.Text = "Add plug-in";
			this.buttonAddPlugIn.Click += new System.EventHandler(this.buttonAddPlugIn_Click);
			// 
			// buttonRemovePlugIn
			// 
			this.buttonRemovePlugIn.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.delete1;
			this.buttonRemovePlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRemovePlugIn.Name = "buttonRemovePlugIn";
			this.buttonRemovePlugIn.Size = new System.Drawing.Size(125, 22);
			this.buttonRemovePlugIn.Text = "Remove plug-in(s)";
			this.buttonRemovePlugIn.Click += new System.EventHandler(this.buttonRemovePlugIn_Click);
			// 
			// containerPlugIns
			// 
			// 
			// containerPlugIns.BottomToolStripPanel
			// 
			this.containerPlugIns.BottomToolStripPanel.Controls.Add(this.statusPlugIns);
			// 
			// containerPlugIns.ContentPanel
			// 
			this.containerPlugIns.ContentPanel.Controls.Add(this.listPlugIns);
			this.containerPlugIns.ContentPanel.Size = new System.Drawing.Size(528, 306);
			this.containerPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerPlugIns.LeftToolStripPanelVisible = false;
			this.containerPlugIns.Location = new System.Drawing.Point(0, 0);
			this.containerPlugIns.Name = "containerPlugIns";
			this.containerPlugIns.RightToolStripPanelVisible = false;
			this.containerPlugIns.Size = new System.Drawing.Size(528, 353);
			this.containerPlugIns.TabIndex = 1;
			this.containerPlugIns.Text = "toolStripContainer1";
			// 
			// containerPlugIns.TopToolStripPanel
			// 
			this.containerPlugIns.TopToolStripPanel.Controls.Add(this.stripPlugIns);
			// 
			// listPlugIns
			// 
			this.listPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerName,
            this.headerDescription});
			this.listPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listPlugIns.FullRowSelect = true;
			this.listPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listPlugIns.Location = new System.Drawing.Point(0, 0);
			this.listPlugIns.Name = "listPlugIns";
			this.listPlugIns.Size = new System.Drawing.Size(528, 306);
			this.listPlugIns.TabIndex = 1;
			this.listPlugIns.UseCompatibleStateImageBehavior = false;
			this.listPlugIns.View = System.Windows.Forms.View.Details;
			this.listPlugIns.SelectedIndexChanged += new System.EventHandler(this.listPlugIns_SelectedIndexChanged);
			this.listPlugIns.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listPlugIns_ItemSelectionChanged);
			// 
			// headerName
			// 
			this.headerName.Text = "Plug-in";
			this.headerName.Width = 49;
			// 
			// headerDescription
			// 
			this.headerDescription.Text = "Description";
			this.headerDescription.Width = 67;
			// 
			// dialogPlugInPath
			// 
			this.dialogPlugInPath.Filter = "Plug-ins (*.dll)|*.dll|All files (*.*)|*.*";
			this.dialogPlugInPath.InitialDirectory = ".\\";
			this.dialogPlugInPath.Multiselect = true;
			this.dialogPlugInPath.Title = "Select plug-in(s) to load.";
			// 
			// statusPlugIns
			// 
			this.statusPlugIns.Dock = System.Windows.Forms.DockStyle.None;
			this.statusPlugIns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelDescription});
			this.statusPlugIns.Location = new System.Drawing.Point(0, 0);
			this.statusPlugIns.Name = "statusPlugIns";
			this.statusPlugIns.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.statusPlugIns.Size = new System.Drawing.Size(528, 22);
			this.statusPlugIns.TabIndex = 0;
			// 
			// labelDescription
			// 
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(0, 17);
			// 
			// formFileSystems
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(528, 353);
			this.Controls.Add(this.containerPlugIns);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formFileSystems";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "File System Plug-ins.";
			this.Resize += new System.EventHandler(this.formFileSystems_Resize);
			this.stripPlugIns.ResumeLayout(false);
			this.stripPlugIns.PerformLayout();
			this.containerPlugIns.BottomToolStripPanel.ResumeLayout(false);
			this.containerPlugIns.BottomToolStripPanel.PerformLayout();
			this.containerPlugIns.ContentPanel.ResumeLayout(false);
			this.containerPlugIns.TopToolStripPanel.ResumeLayout(false);
			this.containerPlugIns.ResumeLayout(false);
			this.containerPlugIns.PerformLayout();
			this.statusPlugIns.ResumeLayout(false);
			this.statusPlugIns.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStrip stripPlugIns;
		private System.Windows.Forms.ToolStripButton buttonAddPlugIn;
		private System.Windows.Forms.ToolStripContainer containerPlugIns;
		private System.Windows.Forms.ToolStripButton buttonRemovePlugIn;
		private System.Windows.Forms.ListView listPlugIns;
		private System.Windows.Forms.ColumnHeader headerName;
		private System.Windows.Forms.ColumnHeader headerDescription;
		private System.Windows.Forms.OpenFileDialog dialogPlugInPath;
		private System.Windows.Forms.StatusStrip statusPlugIns;
		private System.Windows.Forms.ToolStripStatusLabel labelDescription;
	}
}