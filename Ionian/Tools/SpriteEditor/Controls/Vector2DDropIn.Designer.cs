#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, May 03, 2008 8:12:15 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class Vector2DDropIn
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
			this.numericX = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.numericY = new System.Windows.Forms.NumericUpDown();
			this.containerAnimation.ContentPanel.SuspendLayout();
			this.containerAnimation.SuspendLayout();
			this.splitAnimation.Panel1.SuspendLayout();
			this.splitAnimation.Panel2.SuspendLayout();
			this.splitAnimation.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
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
			this.splitAnimation.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitAnimation.IsSplitterFixed = true;
			// 
			// splitAnimation.Panel1
			// 
			this.splitAnimation.Panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseMove);
			this.splitAnimation.Panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseDown);
			this.splitAnimation.Panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseUp);
			// 
			// splitAnimation.Panel2
			// 
			this.splitAnimation.Panel2.Controls.Add(this.label2);
			this.splitAnimation.Panel2.Controls.Add(this.numericY);
			this.splitAnimation.Panel2.Controls.Add(this.label1);
			this.splitAnimation.Panel2.Controls.Add(this.numericX);
			this.splitAnimation.Panel2.Controls.Add(this.labelName);
			this.splitAnimation.SplitterDistance = 374;
			// 
			// panelRender
			// 
			this.panelRender.Size = new System.Drawing.Size(623, 368);
			this.panelRender.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseMove);
			this.panelRender.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseDown);
			this.panelRender.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseUp);
			// 
			// labelName
			// 
			this.labelName.Location = new System.Drawing.Point(8, 9);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(85, 13);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "Property Name:";
			// 
			// numericX
			// 
			this.numericX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericX.DecimalPlaces = 6;
			this.numericX.Location = new System.Drawing.Point(122, 7);
			this.numericX.Name = "numericX";
			this.numericX.Size = new System.Drawing.Size(95, 20);
			this.numericX.TabIndex = 0;
			this.numericX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericX.ThousandsSeparator = true;
			this.numericX.ValueChanged += new System.EventHandler(this.numericFloat_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(99, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "X:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(223, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Y:";
			// 
			// numericY
			// 
			this.numericY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericY.DecimalPlaces = 6;
			this.numericY.Location = new System.Drawing.Point(246, 7);
			this.numericY.Name = "numericY";
			this.numericY.Size = new System.Drawing.Size(95, 20);
			this.numericY.TabIndex = 1;
			this.numericY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericY.ThousandsSeparator = true;
			this.numericY.ValueChanged += new System.EventHandler(this.numericFloat_ValueChanged);
			// 
			// Vector2DDropIn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "Vector2DDropIn";
			this.containerAnimation.ContentPanel.ResumeLayout(false);
			this.containerAnimation.ResumeLayout(false);
			this.containerAnimation.PerformLayout();
			this.splitAnimation.Panel1.ResumeLayout(false);
			this.splitAnimation.Panel2.ResumeLayout(false);
			this.splitAnimation.Panel2.PerformLayout();
			this.splitAnimation.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericY)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown numericX;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericY;
		private System.Windows.Forms.Label label1;
	}
}
