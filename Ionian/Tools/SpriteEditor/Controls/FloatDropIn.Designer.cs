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
// Created: Saturday, May 03, 2008 8:12:29 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class FloatDropIn
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
			this.numericFloat.DecimalPlaces = 6;
			this.numericFloat.Location = new System.Drawing.Point(117, 7);
			this.numericFloat.Name = "numericFloat";
			this.numericFloat.Size = new System.Drawing.Size(120, 20);
			this.numericFloat.TabIndex = 1;
			this.numericFloat.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericFloat.ThousandsSeparator = true;
			this.numericFloat.ValueChanged += new System.EventHandler(this.numericFloat_ValueChanged);
			// 
			// FloatDropIn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "FloatDropIn";
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
