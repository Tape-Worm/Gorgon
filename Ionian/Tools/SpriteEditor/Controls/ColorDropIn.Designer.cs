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
// Created: Saturday, May 03, 2008 5:30:43 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class ColorDropIn
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
			this.labelBlue = new System.Windows.Forms.Label();
			this.numericB = new System.Windows.Forms.NumericUpDown();
			this.labelGreen = new System.Windows.Forms.Label();
			this.numericG = new System.Windows.Forms.NumericUpDown();
			this.labelRed = new System.Windows.Forms.Label();
			this.numericR = new System.Windows.Forms.NumericUpDown();
			this.labelAlpha = new System.Windows.Forms.Label();
			this.numericA = new System.Windows.Forms.NumericUpDown();
			this.pictureColor = new System.Windows.Forms.PictureBox();
			this.buttonSelectColor = new System.Windows.Forms.Button();
			this.checkAutoKey = new System.Windows.Forms.CheckBox();
			this.tipInfo = new System.Windows.Forms.ToolTip(this.components);
			this.containerAnimation.ContentPanel.SuspendLayout();
			this.containerAnimation.SuspendLayout();
			this.splitAnimation.Panel1.SuspendLayout();
			this.splitAnimation.Panel2.SuspendLayout();
			this.splitAnimation.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericG)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericR)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericA)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureColor)).BeginInit();
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
			this.splitAnimation.Panel2.Controls.Add(this.checkAutoKey);
			this.splitAnimation.Panel2.Controls.Add(this.buttonSelectColor);
			this.splitAnimation.Panel2.Controls.Add(this.labelBlue);
			this.splitAnimation.Panel2.Controls.Add(this.numericB);
			this.splitAnimation.Panel2.Controls.Add(this.labelGreen);
			this.splitAnimation.Panel2.Controls.Add(this.numericG);
			this.splitAnimation.Panel2.Controls.Add(this.labelRed);
			this.splitAnimation.Panel2.Controls.Add(this.numericR);
			this.splitAnimation.Panel2.Controls.Add(this.labelAlpha);
			this.splitAnimation.Panel2.Controls.Add(this.numericA);
			this.splitAnimation.Panel2.Controls.Add(this.pictureColor);
			this.splitAnimation.Size = new System.Drawing.Size(629, 427);
			this.splitAnimation.SplitterDistance = 345;
			// 
			// panelRender
			// 
			this.panelRender.Size = new System.Drawing.Size(623, 339);
			// 
			// labelBlue
			// 
			this.labelBlue.AutoSize = true;
			this.labelBlue.Location = new System.Drawing.Point(183, 18);
			this.labelBlue.Name = "labelBlue";
			this.labelBlue.Size = new System.Drawing.Size(28, 13);
			this.labelBlue.TabIndex = 20;
			this.labelBlue.Text = "Blue";
			// 
			// numericB
			// 
			this.numericB.Location = new System.Drawing.Point(186, 33);
			this.numericB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericB.Name = "numericB";
			this.numericB.Size = new System.Drawing.Size(54, 20);
			this.numericB.TabIndex = 3;
			this.numericB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelGreen
			// 
			this.labelGreen.AutoSize = true;
			this.labelGreen.Location = new System.Drawing.Point(123, 18);
			this.labelGreen.Name = "labelGreen";
			this.labelGreen.Size = new System.Drawing.Size(36, 13);
			this.labelGreen.TabIndex = 18;
			this.labelGreen.Text = "Green";
			// 
			// numericG
			// 
			this.numericG.Location = new System.Drawing.Point(126, 33);
			this.numericG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericG.Name = "numericG";
			this.numericG.Size = new System.Drawing.Size(54, 20);
			this.numericG.TabIndex = 2;
			this.numericG.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelRed
			// 
			this.labelRed.AutoSize = true;
			this.labelRed.Location = new System.Drawing.Point(65, 18);
			this.labelRed.Name = "labelRed";
			this.labelRed.Size = new System.Drawing.Size(27, 13);
			this.labelRed.TabIndex = 16;
			this.labelRed.Text = "Red";
			// 
			// numericR
			// 
			this.numericR.Location = new System.Drawing.Point(66, 34);
			this.numericR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericR.Name = "numericR";
			this.numericR.Size = new System.Drawing.Size(54, 20);
			this.numericR.TabIndex = 1;
			this.numericR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelAlpha
			// 
			this.labelAlpha.AutoSize = true;
			this.labelAlpha.Location = new System.Drawing.Point(3, 18);
			this.labelAlpha.Name = "labelAlpha";
			this.labelAlpha.Size = new System.Drawing.Size(34, 13);
			this.labelAlpha.TabIndex = 14;
			this.labelAlpha.Text = "Alpha";
			// 
			// numericA
			// 
			this.numericA.Location = new System.Drawing.Point(6, 34);
			this.numericA.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericA.Name = "numericA";
			this.numericA.Size = new System.Drawing.Size(54, 20);
			this.numericA.TabIndex = 0;
			this.numericA.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// pictureColor
			// 
			this.pictureColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureColor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.TransGridBack1;
			this.pictureColor.Location = new System.Drawing.Point(330, 6);
			this.pictureColor.Name = "pictureColor";
			this.pictureColor.Size = new System.Drawing.Size(293, 67);
			this.pictureColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureColor.TabIndex = 12;
			this.pictureColor.TabStop = false;
			this.pictureColor.DoubleClick += new System.EventHandler(this.buttonSelectColor_Click);
			this.pictureColor.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureColor_Paint);
			// 
			// buttonSelectColor
			// 
			this.buttonSelectColor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.Color;
			this.buttonSelectColor.Location = new System.Drawing.Point(246, 31);
			this.buttonSelectColor.Name = "buttonSelectColor";
			this.buttonSelectColor.Size = new System.Drawing.Size(29, 23);
			this.buttonSelectColor.TabIndex = 4;
			this.tipInfo.SetToolTip(this.buttonSelectColor, "Select a color.  A key will be created automatically if the auto-key button is se" +
					"lected.");
			this.buttonSelectColor.UseVisualStyleBackColor = true;
			this.buttonSelectColor.Click += new System.EventHandler(this.buttonSelectColor_Click);
			// 
			// checkAutoKey
			// 
			this.checkAutoKey.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkAutoKey.Checked = true;
			this.checkAutoKey.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAutoKey.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.key1_preferences;
			this.checkAutoKey.Location = new System.Drawing.Point(282, 31);
			this.checkAutoKey.Name = "checkAutoKey";
			this.checkAutoKey.Size = new System.Drawing.Size(29, 23);
			this.checkAutoKey.TabIndex = 21;
			this.tipInfo.SetToolTip(this.checkAutoKey, "Auto-create the key after using the color selector button.");
			this.checkAutoKey.UseVisualStyleBackColor = true;
			this.checkAutoKey.Click += new System.EventHandler(this.checkAutoKey_Click);
			// 
			// ColorDropIn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "ColorDropIn";
			this.containerAnimation.ContentPanel.ResumeLayout(false);
			this.containerAnimation.ResumeLayout(false);
			this.containerAnimation.PerformLayout();
			this.splitAnimation.Panel1.ResumeLayout(false);
			this.splitAnimation.Panel2.ResumeLayout(false);
			this.splitAnimation.Panel2.PerformLayout();
			this.splitAnimation.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericG)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericR)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericA)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureColor)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelBlue;
		private System.Windows.Forms.NumericUpDown numericB;
		private System.Windows.Forms.Label labelGreen;
		private System.Windows.Forms.NumericUpDown numericG;
		private System.Windows.Forms.Label labelRed;
		private System.Windows.Forms.NumericUpDown numericR;
		private System.Windows.Forms.Label labelAlpha;
		private System.Windows.Forms.NumericUpDown numericA;
		private System.Windows.Forms.PictureBox pictureColor;
		private System.Windows.Forms.Button buttonSelectColor;
		private System.Windows.Forms.CheckBox checkAutoKey;
		private System.Windows.Forms.ToolTip tipInfo;
	}
}
