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
// Created: Saturday, May 03, 2008 8:12:29 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class Int32DropIn
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
			this.labelName = new System.Windows.Forms.Label();
			this.numericFloat = new System.Windows.Forms.NumericUpDown();
			this.containerAnimation.ContentPanel.SuspendLayout();
			this.containerAnimation.SuspendLayout();
			this.splitAnimation.Panel1.SuspendLayout();
			this.splitAnimation.Panel2.SuspendLayout();
			this.splitAnimation.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFloat)).BeginInit();
			this.SuspendLayout();
			// 
			// containerAnimation
			// 
			this.containerAnimation.BottomToolStripPanelVisible = true;
			this.containerAnimation.LeftToolStripPanelVisible = true;
			this.containerAnimation.RightToolStripPanelVisible = true;
			// 
			// splitAnimation
			// 
			this.splitAnimation.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitAnimation.IsSplitterFixed = true;
			// 
			// splitAnimation.Panel2
			// 
			this.splitAnimation.Panel2.Controls.Add(this.numericFloat);
			this.splitAnimation.Panel2.Controls.Add(this.labelName);
			this.splitAnimation.Size = new System.Drawing.Size(629, 427);
			this.splitAnimation.SplitterDistance = 385;
			// 
			// panelRender
			// 
			this.panelRender.Size = new System.Drawing.Size(623, 379);
			// 
			// labelName
			// 
			this.labelName.Location = new System.Drawing.Point(8, 9);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(103, 13);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "Property Name:";
			// 
			// numericFloat
			// 
			this.numericFloat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericFloat.Location = new System.Drawing.Point(117, 7);
			this.numericFloat.Name = "numericFloat";
			this.numericFloat.Size = new System.Drawing.Size(120, 20);
			this.numericFloat.TabIndex = 1;
			this.numericFloat.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericFloat.ThousandsSeparator = true;
			this.numericFloat.ValueChanged += new System.EventHandler(this.numericFloat_ValueChanged);
			// 
			// Int32DropIn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "Int32DropIn";
			this.containerAnimation.ContentPanel.ResumeLayout(false);
			this.containerAnimation.ResumeLayout(false);
			this.containerAnimation.PerformLayout();
			this.splitAnimation.Panel1.ResumeLayout(false);
			this.splitAnimation.Panel2.ResumeLayout(false);
			this.splitAnimation.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericFloat)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown numericFloat;
		private System.Windows.Forms.Label labelName;
	}
}
