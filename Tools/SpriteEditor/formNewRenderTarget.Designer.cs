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
// Created: Tuesday, May 08, 2007 9:59:47 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class formNewRenderTarget
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNewRenderTarget));
			this.label1 = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.numericWidth = new System.Windows.Forms.NumericUpDown();
			this.numericHeight = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.comboFormats = new System.Windows.Forms.ComboBox();
			this.checkUseDepthBuffer = new System.Windows.Forms.CheckBox();
			this.checkUseStencilBuffer = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.numericWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name:";
			// 
			// textName
			// 
			this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textName.Location = new System.Drawing.Point(58, 13);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(287, 20);
			this.textName.TabIndex = 0;
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(287, 118);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(26, 23);
			this.buttonOK.TabIndex = 6;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(319, 118);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(26, 23);
			this.buttonCancel.TabIndex = 7;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(112, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(12, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "x";
			// 
			// numericWidth
			// 
			this.numericWidth.Location = new System.Drawing.Point(58, 39);
			this.numericWidth.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
			this.numericWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.numericWidth.Name = "numericWidth";
			this.numericWidth.Size = new System.Drawing.Size(48, 20);
			this.numericWidth.TabIndex = 1;
			this.numericWidth.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
			// 
			// numericHeight
			// 
			this.numericHeight.Location = new System.Drawing.Point(130, 39);
			this.numericHeight.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
			this.numericHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.numericHeight.Name = "numericHeight";
			this.numericHeight.Size = new System.Drawing.Size(48, 20);
			this.numericHeight.TabIndex = 2;
			this.numericHeight.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(14, 41);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(30, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Size:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(14, 71);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(42, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Format:";
			// 
			// comboFormats
			// 
			this.comboFormats.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.comboFormats.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.comboFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFormats.FormattingEnabled = true;
			this.comboFormats.Location = new System.Drawing.Point(58, 68);
			this.comboFormats.Name = "comboFormats";
			this.comboFormats.Size = new System.Drawing.Size(287, 21);
			this.comboFormats.TabIndex = 3;
			this.comboFormats.SelectedIndexChanged += new System.EventHandler(this.comboFormats_SelectedIndexChanged);
			// 
			// checkUseDepthBuffer
			// 
			this.checkUseDepthBuffer.AutoSize = true;
			this.checkUseDepthBuffer.Enabled = false;
			this.checkUseDepthBuffer.Location = new System.Drawing.Point(17, 97);
			this.checkUseDepthBuffer.Name = "checkUseDepthBuffer";
			this.checkUseDepthBuffer.Size = new System.Drawing.Size(120, 17);
			this.checkUseDepthBuffer.TabIndex = 4;
			this.checkUseDepthBuffer.Text = "Use a depth buffer?";
			this.checkUseDepthBuffer.UseVisualStyleBackColor = true;
			this.checkUseDepthBuffer.CheckedChanged += new System.EventHandler(this.checkUseDepthBuffer_CheckedChanged);
			// 
			// checkUseStencilBuffer
			// 
			this.checkUseStencilBuffer.AutoSize = true;
			this.checkUseStencilBuffer.Enabled = false;
			this.checkUseStencilBuffer.Location = new System.Drawing.Point(143, 97);
			this.checkUseStencilBuffer.Name = "checkUseStencilBuffer";
			this.checkUseStencilBuffer.Size = new System.Drawing.Size(123, 17);
			this.checkUseStencilBuffer.TabIndex = 5;
			this.checkUseStencilBuffer.Text = "Use a stencil buffer?";
			this.checkUseStencilBuffer.UseVisualStyleBackColor = true;
			// 
			// formNewRenderTarget
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(357, 153);
			this.Controls.Add(this.checkUseStencilBuffer);
			this.Controls.Add(this.checkUseDepthBuffer);
			this.Controls.Add(this.comboFormats);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.numericHeight);
			this.Controls.Add(this.numericWidth);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formNewRenderTarget";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New render target.";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formNewRenderTarget_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.numericWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericWidth;
		private System.Windows.Forms.NumericUpDown numericHeight;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboFormats;
		private System.Windows.Forms.CheckBox checkUseDepthBuffer;
		private System.Windows.Forms.CheckBox checkUseStencilBuffer;
	}
}