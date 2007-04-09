#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Sunday, April 01, 2007 3:51:52 PM
// 
#endregion

namespace GorgonLibrary.FileSystems.Tools
{
	partial class formFileSystemWindow
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formFileSystemWindow));
			this.splitTree = new System.Windows.Forms.SplitContainer();
			this.treePaths = new System.Windows.Forms.TreeView();
			this.menuTreePopup = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemAddPath = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRemovePath = new System.Windows.Forms.ToolStripMenuItem();
			this.treeImages = new System.Windows.Forms.ImageList(this.components);
			this.splitDetailView = new System.Windows.Forms.SplitContainer();
			this.viewFiles = new System.Windows.Forms.ListView();
			this.viewImagesLg = new System.Windows.Forms.ImageList(this.components);
			this.viewImages = new System.Windows.Forms.ImageList(this.components);
			this.panelPreview = new System.Windows.Forms.Panel();
			this.splitTree.Panel1.SuspendLayout();
			this.splitTree.Panel2.SuspendLayout();
			this.splitTree.SuspendLayout();
			this.menuTreePopup.SuspendLayout();
			this.splitDetailView.Panel1.SuspendLayout();
			this.splitDetailView.Panel2.SuspendLayout();
			this.splitDetailView.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitTree
			// 
			this.splitTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitTree.Location = new System.Drawing.Point(0, 0);
			this.splitTree.Name = "splitTree";
			// 
			// splitTree.Panel1
			// 
			this.splitTree.Panel1.Controls.Add(this.treePaths);
			// 
			// splitTree.Panel2
			// 
			this.splitTree.Panel2.Controls.Add(this.splitDetailView);
			this.splitTree.Size = new System.Drawing.Size(572, 425);
			this.splitTree.SplitterDistance = 190;
			this.splitTree.TabIndex = 0;
			// 
			// treePaths
			// 
			this.treePaths.AllowDrop = true;
			this.treePaths.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treePaths.ContextMenuStrip = this.menuTreePopup;
			this.treePaths.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treePaths.FullRowSelect = true;
			this.treePaths.HideSelection = false;
			this.treePaths.ImageIndex = 0;
			this.treePaths.ImageList = this.treeImages;
			this.treePaths.LabelEdit = true;
			this.treePaths.Location = new System.Drawing.Point(0, 0);
			this.treePaths.Name = "treePaths";
			this.treePaths.SelectedImageIndex = 0;
			this.treePaths.Size = new System.Drawing.Size(190, 425);
			this.treePaths.TabIndex = 0;
			this.treePaths.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treePaths_AfterCollapse);
			this.treePaths.DragDrop += new System.Windows.Forms.DragEventHandler(this.treePaths_DragDrop);
			this.treePaths.DragOver += new System.Windows.Forms.DragEventHandler(this.treePaths_DragOver);
			this.treePaths.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treePaths_AfterLabelEdit);
			this.treePaths.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treePaths_AfterSelect);
			this.treePaths.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treePaths_BeforeLabelEdit);
			this.treePaths.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treePaths_KeyDown);
			this.treePaths.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treePaths_AfterExpand);
			this.treePaths.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treePaths_ItemDrag);
			this.treePaths.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treePaths_MouseDown);
			// 
			// menuTreePopup
			// 
			this.menuTreePopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAddPath,
            this.menuItemRemovePath});
			this.menuTreePopup.Name = "contextMenuStrip1";
			this.menuTreePopup.Size = new System.Drawing.Size(154, 48);
			// 
			// menuItemAddPath
			// 
			this.menuItemAddPath.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_add;
			this.menuItemAddPath.Name = "menuItemAddPath";
			this.menuItemAddPath.Size = new System.Drawing.Size(153, 22);
			this.menuItemAddPath.Text = "Add path...";
			this.menuItemAddPath.Click += new System.EventHandler(this.menuItemAddPath_Click);
			// 
			// menuItemRemovePath
			// 
			this.menuItemRemovePath.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_delete;
			this.menuItemRemovePath.Name = "menuItemRemovePath";
			this.menuItemRemovePath.Size = new System.Drawing.Size(153, 22);
			this.menuItemRemovePath.Text = "Remove path...";
			// 
			// treeImages
			// 
			this.treeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImages.ImageStream")));
			this.treeImages.TransparentColor = System.Drawing.Color.Transparent;
			this.treeImages.Images.SetKeyName(0, "folder_closed.png");
			this.treeImages.Images.SetKeyName(1, "folder.png");
			// 
			// splitDetailView
			// 
			this.splitDetailView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitDetailView.Location = new System.Drawing.Point(0, 0);
			this.splitDetailView.Name = "splitDetailView";
			this.splitDetailView.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitDetailView.Panel1
			// 
			this.splitDetailView.Panel1.Controls.Add(this.viewFiles);
			// 
			// splitDetailView.Panel2
			// 
			this.splitDetailView.Panel2.Controls.Add(this.panelPreview);
			this.splitDetailView.Size = new System.Drawing.Size(378, 425);
			this.splitDetailView.SplitterDistance = 226;
			this.splitDetailView.TabIndex = 0;
			// 
			// viewFiles
			// 
			this.viewFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.viewFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.viewFiles.LargeImageList = this.viewImagesLg;
			this.viewFiles.Location = new System.Drawing.Point(0, 0);
			this.viewFiles.Name = "viewFiles";
			this.viewFiles.Size = new System.Drawing.Size(378, 226);
			this.viewFiles.SmallImageList = this.viewImages;
			this.viewFiles.TabIndex = 0;
			this.viewFiles.UseCompatibleStateImageBehavior = false;
			// 
			// viewImagesLg
			// 
			this.viewImagesLg.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("viewImagesLg.ImageStream")));
			this.viewImagesLg.TransparentColor = System.Drawing.Color.Transparent;
			this.viewImagesLg.Images.SetKeyName(0, "GorgonGeneric");
			this.viewImagesLg.Images.SetKeyName(1, "GorgonFont");
			this.viewImagesLg.Images.SetKeyName(2, "Image");
			this.viewImagesLg.Images.SetKeyName(3, "Document");
			this.viewImagesLg.Images.SetKeyName(4, "Binary");
			this.viewImagesLg.Images.SetKeyName(5, "XML");
			// 
			// viewImages
			// 
			this.viewImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("viewImages.ImageStream")));
			this.viewImages.TransparentColor = System.Drawing.Color.Transparent;
			this.viewImages.Images.SetKeyName(0, "GorgonGeneric");
			this.viewImages.Images.SetKeyName(1, "GorgonFont");
			this.viewImages.Images.SetKeyName(2, "Document");
			this.viewImages.Images.SetKeyName(3, "Image");
			this.viewImages.Images.SetKeyName(4, "Binary");
			this.viewImages.Images.SetKeyName(5, "XML");
			// 
			// panelPreview
			// 
			this.panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPreview.Location = new System.Drawing.Point(0, 0);
			this.panelPreview.Name = "panelPreview";
			this.panelPreview.Size = new System.Drawing.Size(378, 195);
			this.panelPreview.TabIndex = 0;
			// 
			// formFileSystemWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(572, 425);
			this.Controls.Add(this.splitTree);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "formFileSystemWindow";
			this.Text = "File System";
			this.splitTree.Panel1.ResumeLayout(false);
			this.splitTree.Panel2.ResumeLayout(false);
			this.splitTree.ResumeLayout(false);
			this.menuTreePopup.ResumeLayout(false);
			this.splitDetailView.Panel1.ResumeLayout(false);
			this.splitDetailView.Panel2.ResumeLayout(false);
			this.splitDetailView.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitTree;
		private System.Windows.Forms.TreeView treePaths;
		private System.Windows.Forms.SplitContainer splitDetailView;
		private System.Windows.Forms.ListView viewFiles;
		private System.Windows.Forms.Panel panelPreview;
		private System.Windows.Forms.ImageList treeImages;
		private System.Windows.Forms.ContextMenuStrip menuTreePopup;
		private System.Windows.Forms.ToolStripMenuItem menuItemAddPath;
		private System.Windows.Forms.ToolStripMenuItem menuItemRemovePath;
		private System.Windows.Forms.ImageList viewImages;
		private System.Windows.Forms.ImageList viewImagesLg;
	}
}