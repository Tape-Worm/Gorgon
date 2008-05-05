#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, May 03, 2008 2:23:22 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class ImageDropIn
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
			this.listFrames = new System.Windows.Forms.ListView();
			this.imageSprites = new System.Windows.Forms.ImageList(this.components);
			this.containerAnimation.ContentPanel.SuspendLayout();
			this.containerAnimation.SuspendLayout();
			this.splitAnimation.Panel1.SuspendLayout();
			this.splitAnimation.Panel2.SuspendLayout();
			this.splitAnimation.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerAnimation
			// 
			this.containerAnimation.BottomToolStripPanelVisible = true;
			// 
			// containerAnimation.ContentPanel
			// 
			this.containerAnimation.ContentPanel.Size = new System.Drawing.Size(629, 402);
			this.containerAnimation.LeftToolStripPanelVisible = true;
			this.containerAnimation.RightToolStripPanelVisible = true;
			// 
			// splitAnimation
			// 
			// 
			// splitAnimation.Panel2
			// 
			this.splitAnimation.Panel2.Controls.Add(this.listFrames);
			this.splitAnimation.SplitterDistance = 210;
			// 
			// panelRender
			// 
			this.panelRender.Size = new System.Drawing.Size(623, 204);
			this.panelRender.DragOver += new System.Windows.Forms.DragEventHandler(this.panelRender_DragOver);
			this.panelRender.DragDrop += new System.Windows.Forms.DragEventHandler(this.panelRender_DragDrop);
			// 
			// listFrames
			// 
			this.listFrames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listFrames.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFrames.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listFrames.HideSelection = false;
			this.listFrames.LargeImageList = this.imageSprites;
			this.listFrames.Location = new System.Drawing.Point(3, 3);
			this.listFrames.Name = "listFrames";
			this.listFrames.ShowItemToolTips = true;
			this.listFrames.Size = new System.Drawing.Size(623, 183);
			this.listFrames.SmallImageList = this.imageSprites;
			this.listFrames.TabIndex = 0;
			this.listFrames.TileSize = new System.Drawing.Size(66, 66);
			this.listFrames.UseCompatibleStateImageBehavior = false;
			this.listFrames.SelectedIndexChanged += new System.EventHandler(this.listFrames_SelectedIndexChanged);
			this.listFrames.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listFrames_ItemDrag);
			// 
			// imageSprites
			// 
			this.imageSprites.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageSprites.ImageSize = new System.Drawing.Size(64, 64);
			this.imageSprites.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// ImageDropIn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "ImageDropIn";
			this.containerAnimation.ContentPanel.ResumeLayout(false);
			this.containerAnimation.ResumeLayout(false);
			this.containerAnimation.PerformLayout();
			this.splitAnimation.Panel1.ResumeLayout(false);
			this.splitAnimation.Panel2.ResumeLayout(false);
			this.splitAnimation.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ListView listFrames;
		private System.Windows.Forms.ImageList imageSprites;
	}
}
