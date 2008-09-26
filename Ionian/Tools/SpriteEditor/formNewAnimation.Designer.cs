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
// Created: Sunday, July 08, 2007 11:33:14 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class formNewAnimation
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNewAnimation));
			this.label1 = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.numericLength = new System.Windows.Forms.NumericUpDown();
			this.labelUnit = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.checkLoop = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.numericFrameRate = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.labelTimeCalculation = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numericLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameRate)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name:";
			// 
			// textName
			// 
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textName.Location = new System.Drawing.Point(78, 13);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(194, 20);
			this.textName.TabIndex = 0;
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 67);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Length:";
			// 
			// numericLength
			// 
			this.numericLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericLength.Increment = new decimal(new int[] {
            30,
            0,
            0,
            0});
			this.numericLength.Location = new System.Drawing.Point(78, 65);
			this.numericLength.Maximum = new decimal(new int[] {
            86400000,
            0,
            0,
            0});
			this.numericLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericLength.Name = "numericLength";
			this.numericLength.Size = new System.Drawing.Size(89, 20);
			this.numericLength.TabIndex = 2;
			this.numericLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericLength.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
			this.numericLength.ValueChanged += new System.EventHandler(this.numericLength_ValueChanged);
			// 
			// labelUnit
			// 
			this.labelUnit.Location = new System.Drawing.Point(173, 67);
			this.labelUnit.Name = "labelUnit";
			this.labelUnit.Size = new System.Drawing.Size(108, 18);
			this.labelUnit.TabIndex = 4;
			this.labelUnit.Text = "frames";
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(248, 139);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 23);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(218, 139);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 23);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// checkLoop
			// 
			this.checkLoop.AutoSize = true;
			this.checkLoop.Location = new System.Drawing.Point(16, 91);
			this.checkLoop.Name = "checkLoop";
			this.checkLoop.Size = new System.Drawing.Size(122, 17);
			this.checkLoop.TabIndex = 3;
			this.checkLoop.Text = "Loop the animation?";
			this.checkLoop.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(173, 41);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(108, 18);
			this.label3.TabIndex = 8;
			this.label3.Text = "Frames/second";
			// 
			// numericFrameRate
			// 
			this.numericFrameRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericFrameRate.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.numericFrameRate.Location = new System.Drawing.Point(78, 39);
			this.numericFrameRate.Maximum = new decimal(new int[] {
            86400000,
            0,
            0,
            0});
			this.numericFrameRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericFrameRate.Name = "numericFrameRate";
			this.numericFrameRate.Size = new System.Drawing.Size(89, 20);
			this.numericFrameRate.TabIndex = 1;
			this.numericFrameRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericFrameRate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
			this.numericFrameRate.ValueChanged += new System.EventHandler(this.numericLength_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 41);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Frame rate:";
			// 
			// labelTimeCalculation
			// 
			this.labelTimeCalculation.Location = new System.Drawing.Point(12, 115);
			this.labelTimeCalculation.Name = "labelTimeCalculation";
			this.labelTimeCalculation.Size = new System.Drawing.Size(260, 21);
			this.labelTimeCalculation.TabIndex = 9;
			this.labelTimeCalculation.Text = "Animation will be 0 secs long.";
			// 
			// formNewAnimation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 174);
			this.Controls.Add(this.labelTimeCalculation);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.numericFrameRate);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.checkLoop);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.labelUnit);
			this.Controls.Add(this.numericLength);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formNewAnimation";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Animation";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formNewAnimation_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.numericLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameRate)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericLength;
		private System.Windows.Forms.Label labelUnit;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.CheckBox checkLoop;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numericFrameRate;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelTimeCalculation;
	}
}