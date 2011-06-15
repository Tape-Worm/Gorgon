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
// Created: Tuesday, December 09, 2008 12:05:38 AM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class formBackImageEdit
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBackImageEdit));
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.labelImageData = new System.Windows.Forms.Label();
			this.numericX = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.numericY = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(172, 37);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(26, 23);
			this.buttonOK.TabIndex = 3;
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(204, 37);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(26, 23);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// labelImageData
			// 
			this.labelImageData.AutoSize = true;
			this.labelImageData.Location = new System.Drawing.Point(13, 13);
			this.labelImageData.Name = "labelImageData";
			this.labelImageData.Size = new System.Drawing.Size(65, 13);
			this.labelImageData.TabIndex = 5;
			this.labelImageData.Text = "Image Data:";
			// 
			// numericX
			// 
			this.numericX.Location = new System.Drawing.Point(84, 11);
			this.numericX.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
			this.numericX.Minimum = new decimal(new int[] {
            32767,
            0,
            0,
            -2147483648});
			this.numericX.Name = "numericX";
			this.numericX.Size = new System.Drawing.Size(61, 20);
			this.numericX.TabIndex = 6;
			this.numericX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(151, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(12, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "x";
			// 
			// numericY
			// 
			this.numericY.Location = new System.Drawing.Point(169, 11);
			this.numericY.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
			this.numericY.Minimum = new decimal(new int[] {
            32767,
            0,
            0,
            -2147483648});
			this.numericY.Name = "numericY";
			this.numericY.Size = new System.Drawing.Size(61, 20);
			this.numericY.TabIndex = 8;
			this.numericY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// formBackImageEdit
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(248, 71);
			this.Controls.Add(this.numericY);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numericX);
			this.Controls.Add(this.labelImageData);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formBackImageEdit";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "...";
			((System.ComponentModel.ISupportInitialize)(this.numericX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericY)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		internal System.Windows.Forms.Label labelImageData;
		private System.Windows.Forms.Label label1;
		internal System.Windows.Forms.NumericUpDown numericX;
		internal System.Windows.Forms.NumericUpDown numericY;
	}
}