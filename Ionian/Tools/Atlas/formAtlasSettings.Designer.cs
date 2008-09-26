#region MIT.
// 
// Atlas.
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
// Created: Tuesday, November 13, 2007 1:33:23 AM
// 
#endregion

namespace Atlas
{
	partial class formAtlasSettings
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formAtlasSettings));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.numericWidth = new System.Windows.Forms.NumericUpDown();
			this.numericHeight = new System.Windows.Forms.NumericUpDown();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.numericPadding = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numericWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericPadding)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Width:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(41, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Height:";
			// 
			// numericWidth
			// 
			this.numericWidth.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.numericWidth.Location = new System.Drawing.Point(67, 7);
			this.numericWidth.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.numericWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericWidth.Name = "numericWidth";
			this.numericWidth.Size = new System.Drawing.Size(109, 20);
			this.numericWidth.TabIndex = 2;
			this.numericWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// numericHeight
			// 
			this.numericHeight.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.numericHeight.Location = new System.Drawing.Point(67, 33);
			this.numericHeight.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.numericHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericHeight.Name = "numericHeight";
			this.numericHeight.Size = new System.Drawing.Size(109, 20);
			this.numericHeight.TabIndex = 3;
			this.numericHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(12, 85);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(110, 85);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// numericPadding
			// 
			this.numericPadding.Location = new System.Drawing.Point(67, 59);
			this.numericPadding.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericPadding.Name = "numericPadding";
			this.numericPadding.Size = new System.Drawing.Size(109, 20);
			this.numericPadding.TabIndex = 7;
			this.numericPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 61);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(49, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Padding:";
			// 
			// formAtlasSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(197, 115);
			this.Controls.Add(this.numericPadding);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.numericHeight);
			this.Controls.Add(this.numericWidth);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formAtlasSettings";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Atlas Settings";
			((System.ComponentModel.ISupportInitialize)(this.numericWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericPadding)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericWidth;
		private System.Windows.Forms.NumericUpDown numericHeight;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.NumericUpDown numericPadding;
		private System.Windows.Forms.Label label3;
	}
}