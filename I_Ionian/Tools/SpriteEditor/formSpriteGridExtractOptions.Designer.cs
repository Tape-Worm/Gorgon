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
// Created: Saturday, June 23, 2007 8:17:40 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class formSpriteGridExtractOptions
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

			if ((disposing) && (!DesignMode))
			{
				if (_finder != null)
					_finder.Dispose();
				_finder = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formSpriteGridExtractOptions));
			this.label1 = new System.Windows.Forms.Label();
			this.textPrefix = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.numericCellHeight = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.numericCellWidth = new System.Windows.Forms.NumericUpDown();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.numericSpacingY = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.numericSpacingX = new System.Windows.Forms.NumericUpDown();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.numericTop = new System.Windows.Forms.NumericUpDown();
			this.label10 = new System.Windows.Forms.Label();
			this.numericLeft = new System.Windows.Forms.NumericUpDown();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.numericConstraintHeight = new System.Windows.Forms.NumericUpDown();
			this.label13 = new System.Windows.Forms.Label();
			this.numericConstraintWidth = new System.Windows.Forms.NumericUpDown();
			this.statusInfo = new System.Windows.Forms.StatusStrip();
			this.labelConstraint = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelSpriteCount = new System.Windows.Forms.Label();
			this.checkDirection = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericCellHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericCellWidth)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericSpacingY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericSpacingX)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTop)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericLeft)).BeginInit();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericConstraintHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericConstraintWidth)).BeginInit();
			this.statusInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(102, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Sprite naming prefix:";
			// 
			// textPrefix
			// 
			this.textPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textPrefix.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textPrefix.Location = new System.Drawing.Point(121, 11);
			this.textPrefix.Name = "textPrefix";
			this.textPrefix.Size = new System.Drawing.Size(180, 20);
			this.textPrefix.TabIndex = 0;
			this.textPrefix.Leave += new System.EventHandler(this.textPrefix_Leave);
			this.textPrefix.TextChanged += new System.EventHandler(this.textPrefix_TextChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.numericCellHeight);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.numericCellWidth);
			this.groupBox1.Location = new System.Drawing.Point(12, 104);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(142, 65);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Grid Cell Settings";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(64, 37);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(12, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "x";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(80, 19);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(38, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Height";
			// 
			// numericCellHeight
			// 
			this.numericCellHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericCellHeight.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.numericCellHeight.Location = new System.Drawing.Point(83, 35);
			this.numericCellHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericCellHeight.Name = "numericCellHeight";
			this.numericCellHeight.Size = new System.Drawing.Size(52, 20);
			this.numericCellHeight.TabIndex = 1;
			this.numericCellHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericCellHeight.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.numericCellHeight.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 19);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Width";
			// 
			// numericCellWidth
			// 
			this.numericCellWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericCellWidth.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.numericCellWidth.Location = new System.Drawing.Point(6, 35);
			this.numericCellWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericCellWidth.Name = "numericCellWidth";
			this.numericCellWidth.Size = new System.Drawing.Size(52, 20);
			this.numericCellWidth.TabIndex = 0;
			this.numericCellWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericCellWidth.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.numericCellWidth.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(276, 222);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 23);
			this.buttonCancel.TabIndex = 6;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(246, 222);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 23);
			this.buttonOK.TabIndex = 5;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.numericSpacingY);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.numericSpacingX);
			this.groupBox2.Location = new System.Drawing.Point(159, 104);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(141, 65);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Cell Spacing Settings";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(65, 37);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(12, 13);
			this.label5.TabIndex = 19;
			this.label5.Text = "x";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(80, 19);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(42, 13);
			this.label6.TabIndex = 18;
			this.label6.Text = "Vertical";
			// 
			// numericSpacingY
			// 
			this.numericSpacingY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericSpacingY.Location = new System.Drawing.Point(83, 35);
			this.numericSpacingY.Name = "numericSpacingY";
			this.numericSpacingY.Size = new System.Drawing.Size(52, 20);
			this.numericSpacingY.TabIndex = 1;
			this.numericSpacingY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericSpacingY.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(4, 19);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(54, 13);
			this.label7.TabIndex = 16;
			this.label7.Text = "Horizontal";
			// 
			// numericSpacingX
			// 
			this.numericSpacingX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericSpacingX.Location = new System.Drawing.Point(7, 35);
			this.numericSpacingX.Name = "numericSpacingX";
			this.numericSpacingX.Size = new System.Drawing.Size(52, 20);
			this.numericSpacingX.TabIndex = 0;
			this.numericSpacingX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericSpacingX.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label8);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Controls.Add(this.numericTop);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.numericLeft);
			this.groupBox3.Location = new System.Drawing.Point(12, 37);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(142, 61);
			this.groupBox3.TabIndex = 1;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Origin Settings";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(65, 34);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(12, 13);
			this.label8.TabIndex = 14;
			this.label8.Text = "x";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(80, 17);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(26, 13);
			this.label9.TabIndex = 13;
			this.label9.Text = "Top";
			// 
			// numericTop
			// 
			this.numericTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericTop.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericTop.Location = new System.Drawing.Point(83, 32);
			this.numericTop.Name = "numericTop";
			this.numericTop.Size = new System.Drawing.Size(52, 20);
			this.numericTop.TabIndex = 1;
			this.numericTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericTop.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(3, 17);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(25, 13);
			this.label10.TabIndex = 12;
			this.label10.Text = "Left";
			// 
			// numericLeft
			// 
			this.numericLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericLeft.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericLeft.Location = new System.Drawing.Point(6, 32);
			this.numericLeft.Name = "numericLeft";
			this.numericLeft.Size = new System.Drawing.Size(52, 20);
			this.numericLeft.TabIndex = 0;
			this.numericLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericLeft.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.label11);
			this.groupBox4.Controls.Add(this.label12);
			this.groupBox4.Controls.Add(this.numericConstraintHeight);
			this.groupBox4.Controls.Add(this.label13);
			this.groupBox4.Controls.Add(this.numericConstraintWidth);
			this.groupBox4.Location = new System.Drawing.Point(159, 37);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(141, 61);
			this.groupBox4.TabIndex = 2;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Constraint Settings";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(64, 34);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(12, 13);
			this.label11.TabIndex = 14;
			this.label11.Text = "x";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(80, 17);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(38, 13);
			this.label12.TabIndex = 13;
			this.label12.Text = "Height";
			// 
			// numericConstraintHeight
			// 
			this.numericConstraintHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericConstraintHeight.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericConstraintHeight.Location = new System.Drawing.Point(83, 32);
			this.numericConstraintHeight.Name = "numericConstraintHeight";
			this.numericConstraintHeight.Size = new System.Drawing.Size(52, 20);
			this.numericConstraintHeight.TabIndex = 1;
			this.numericConstraintHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericConstraintHeight.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(3, 17);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(35, 13);
			this.label13.TabIndex = 12;
			this.label13.Text = "Width";
			// 
			// numericConstraintWidth
			// 
			this.numericConstraintWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericConstraintWidth.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericConstraintWidth.Location = new System.Drawing.Point(6, 32);
			this.numericConstraintWidth.Name = "numericConstraintWidth";
			this.numericConstraintWidth.Size = new System.Drawing.Size(52, 20);
			this.numericConstraintWidth.TabIndex = 0;
			this.numericConstraintWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericConstraintWidth.ValueChanged += new System.EventHandler(this.numericLeft_ValueChanged);
			// 
			// statusInfo
			// 
			this.statusInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelConstraint});
			this.statusInfo.Location = new System.Drawing.Point(0, 248);
			this.statusInfo.Name = "statusInfo";
			this.statusInfo.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.statusInfo.Size = new System.Drawing.Size(314, 22);
			this.statusInfo.SizingGrip = false;
			this.statusInfo.TabIndex = 7;
			// 
			// labelConstraint
			// 
			this.labelConstraint.AutoToolTip = true;
			this.labelConstraint.Name = "labelConstraint";
			this.labelConstraint.Size = new System.Drawing.Size(269, 17);
			this.labelConstraint.Text = "Left: 32767 Top: 32767 Right: 32767 Bottom: 32767";
			this.labelConstraint.ToolTipText = "Constraints.";
			// 
			// labelSpriteCount
			// 
			this.labelSpriteCount.AutoSize = true;
			this.labelSpriteCount.Location = new System.Drawing.Point(12, 207);
			this.labelSpriteCount.Name = "labelSpriteCount";
			this.labelSpriteCount.Size = new System.Drawing.Size(114, 13);
			this.labelSpriteCount.TabIndex = 8;
			this.labelSpriteCount.Text = "Estimated sprite count:";
			// 
			// checkDirection
			// 
			this.checkDirection.AutoSize = true;
			this.checkDirection.Location = new System.Drawing.Point(12, 176);
			this.checkDirection.Name = "checkDirection";
			this.checkDirection.Size = new System.Drawing.Size(140, 17);
			this.checkDirection.TabIndex = 9;
			this.checkDirection.Text = "Read from top to bottom";
			this.checkDirection.UseVisualStyleBackColor = true;
			// 
			// formSpriteGridExtractOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(314, 270);
			this.Controls.Add(this.checkDirection);
			this.Controls.Add(this.labelSpriteCount);
			this.Controls.Add(this.statusInfo);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.textPrefix);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formSpriteGridExtractOptions";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Extract Sprites by Grid";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formSpriteGridExtractOptions_KeyDown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericCellHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericCellWidth)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericSpacingY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericSpacingX)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTop)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericLeft)).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericConstraintHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericConstraintWidth)).EndInit();
			this.statusInfo.ResumeLayout(false);
			this.statusInfo.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textPrefix;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numericCellHeight;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericCellWidth;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numericSpacingY;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown numericSpacingX;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown numericTop;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.NumericUpDown numericLeft;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.NumericUpDown numericConstraintHeight;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.NumericUpDown numericConstraintWidth;
		private System.Windows.Forms.StatusStrip statusInfo;
		private System.Windows.Forms.ToolStripStatusLabel labelConstraint;
		private System.Windows.Forms.Label labelSpriteCount;
		private System.Windows.Forms.CheckBox checkDirection;
	}
}