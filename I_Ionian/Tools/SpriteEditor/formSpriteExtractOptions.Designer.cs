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
	partial class formSpriteExtractOptions
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formSpriteExtractOptions));
			this.label1 = new System.Windows.Forms.Label();
			this.textPrefix = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.pictureMaskColor = new System.Windows.Forms.PictureBox();
			this.label3 = new System.Windows.Forms.Label();
			this.buttonColorSelect = new System.Windows.Forms.Button();
			this.checkAlpha = new System.Windows.Forms.CheckBox();
			this.checkColor = new System.Windows.Forms.CheckBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureMaskColor)).BeginInit();
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
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.pictureMaskColor);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.buttonColorSelect);
			this.groupBox1.Controls.Add(this.checkAlpha);
			this.groupBox1.Controls.Add(this.checkColor);
			this.groupBox1.Location = new System.Drawing.Point(13, 39);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(289, 94);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Masking";
			// 
			// pictureMaskColor
			// 
			this.pictureMaskColor.BackgroundImage = global::GorgonLibrary.Graphics.Tools.Properties.Resources.Pattern;
			this.pictureMaskColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureMaskColor.Location = new System.Drawing.Point(6, 57);
			this.pictureMaskColor.Name = "pictureMaskColor";
			this.pictureMaskColor.Size = new System.Drawing.Size(247, 23);
			this.pictureMaskColor.TabIndex = 11;
			this.pictureMaskColor.TabStop = false;
			this.pictureMaskColor.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureMaskColor_Paint);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(3, 39);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 18);
			this.label3.TabIndex = 10;
			this.label3.Text = "Masking color:";
			// 
			// buttonColorSelect
			// 
			this.buttonColorSelect.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.Color;
			this.buttonColorSelect.Location = new System.Drawing.Point(259, 57);
			this.buttonColorSelect.Name = "buttonColorSelect";
			this.buttonColorSelect.Size = new System.Drawing.Size(24, 23);
			this.buttonColorSelect.TabIndex = 9;
			this.buttonColorSelect.UseVisualStyleBackColor = true;
			this.buttonColorSelect.Click += new System.EventHandler(this.buttonColorSelect_Click);
			// 
			// checkAlpha
			// 
			this.checkAlpha.AutoSize = true;
			this.checkAlpha.Location = new System.Drawing.Point(143, 19);
			this.checkAlpha.Name = "checkAlpha";
			this.checkAlpha.Size = new System.Drawing.Size(134, 17);
			this.checkAlpha.TabIndex = 1;
			this.checkAlpha.Text = "Use alpha for masking.";
			this.checkAlpha.UseVisualStyleBackColor = true;
			this.checkAlpha.Click += new System.EventHandler(this.checkAlpha_Click);
			// 
			// checkColor
			// 
			this.checkColor.AutoSize = true;
			this.checkColor.Location = new System.Drawing.Point(6, 19);
			this.checkColor.Name = "checkColor";
			this.checkColor.Size = new System.Drawing.Size(131, 17);
			this.checkColor.TabIndex = 0;
			this.checkColor.Text = "Use color for masking.";
			this.checkColor.UseVisualStyleBackColor = true;
			this.checkColor.Click += new System.EventHandler(this.checkColor_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(277, 139);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 23);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(247, 139);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 23);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// formSpriteExtractOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(314, 174);
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
			this.Name = "formSpriteExtractOptions";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Extract Sprites";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formSpriteExtractOptions_KeyDown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureMaskColor)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textPrefix;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkAlpha;
		private System.Windows.Forms.CheckBox checkColor;
		private System.Windows.Forms.Button buttonColorSelect;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox pictureMaskColor;
	}
}