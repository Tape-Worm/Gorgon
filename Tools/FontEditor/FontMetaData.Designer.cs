#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Thursday, August 03, 2006 4:13:46 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class FontMetaData
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontMetaData));
			this.label1 = new System.Windows.Forms.Label();
			this.comboFontList = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.numericFontSize = new System.Windows.Forms.NumericUpDown();
			this.checkBold = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textFontName = new System.Windows.Forms.TextBox();
			this.checkItalics = new System.Windows.Forms.CheckBox();
			this.checkUnderline = new System.Windows.Forms.CheckBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.dialogOpenImage = new System.Windows.Forms.OpenFileDialog();
			this.numericImageWidth = new System.Windows.Forms.NumericUpDown();
			this.numericImageHeight = new System.Windows.Forms.NumericUpDown();
			this.labelImageSize = new System.Windows.Forms.Label();
			this.labelBy = new System.Windows.Forms.Label();
			this.checkAntiAlias = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.labelPreview = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numericFontSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageHeight)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(55, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Font face:";
			// 
			// comboFontList
			// 
			this.comboFontList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFontList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboFontList.FormattingEnabled = true;
			this.comboFontList.Location = new System.Drawing.Point(9, 32);
			this.comboFontList.Name = "comboFontList";
			this.comboFontList.Size = new System.Drawing.Size(218, 21);
			this.comboFontList.TabIndex = 0;
			this.comboFontList.SelectedIndexChanged += new System.EventHandler(this.comboFontList_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(230, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(30, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Size:";
			// 
			// numericFontSize
			// 
			this.numericFontSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericFontSize.DecimalPlaces = 1;
			this.numericFontSize.Location = new System.Drawing.Point(233, 32);
			this.numericFontSize.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.numericFontSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericFontSize.Name = "numericFontSize";
			this.numericFontSize.Size = new System.Drawing.Size(46, 20);
			this.numericFontSize.TabIndex = 1;
			this.numericFontSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericFontSize.Value = new decimal(new int[] {
            90,
            0,
            0,
            65536});
			this.numericFontSize.ValueChanged += new System.EventHandler(this.numericFontSize_ValueChanged);
			// 
			// checkBold
			// 
			this.checkBold.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBold.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBold.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBold.Location = new System.Drawing.Point(76, 59);
			this.checkBold.Name = "checkBold";
			this.checkBold.Size = new System.Drawing.Size(29, 27);
			this.checkBold.TabIndex = 2;
			this.checkBold.Text = "B";
			this.checkBold.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBold.UseVisualStyleBackColor = true;
			this.checkBold.CheckedChanged += new System.EventHandler(this.checkBold_CheckedChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(38, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Name:";
			// 
			// textFontName
			// 
			this.textFontName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textFontName.Location = new System.Drawing.Point(12, 25);
			this.textFontName.Name = "textFontName";
			this.textFontName.Size = new System.Drawing.Size(287, 20);
			this.textFontName.TabIndex = 0;
			this.textFontName.TextChanged += new System.EventHandler(this.textFontName_TextChanged);
			// 
			// checkItalics
			// 
			this.checkItalics.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkItalics.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkItalics.Font = new System.Drawing.Font("Times New Roman", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkItalics.Location = new System.Drawing.Point(111, 59);
			this.checkItalics.Name = "checkItalics";
			this.checkItalics.Size = new System.Drawing.Size(29, 27);
			this.checkItalics.TabIndex = 3;
			this.checkItalics.Text = "I";
			this.checkItalics.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkItalics.UseVisualStyleBackColor = true;
			this.checkItalics.CheckedChanged += new System.EventHandler(this.checkItalics_CheckedChanged);
			// 
			// checkUnderline
			// 
			this.checkUnderline.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkUnderline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkUnderline.Font = new System.Drawing.Font("Times New Roman", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkUnderline.Location = new System.Drawing.Point(146, 59);
			this.checkUnderline.Name = "checkUnderline";
			this.checkUnderline.Size = new System.Drawing.Size(29, 27);
			this.checkUnderline.TabIndex = 4;
			this.checkUnderline.Text = "U";
			this.checkUnderline.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkUnderline.UseVisualStyleBackColor = true;
			this.checkUnderline.CheckedChanged += new System.EventHandler(this.checkUnderline_CheckedChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(275, 264);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 24);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(245, 264);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 24);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// dialogOpenImage
			// 
			this.dialogOpenImage.DefaultExt = "png";
			this.dialogOpenImage.Filter = "Portable Network Graphics (*.png)|*.png|Targa (*.tga)|*.tga";
			this.dialogOpenImage.Title = "Select an image to use for the font.";
			// 
			// numericImageWidth
			// 
			this.numericImageWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericImageWidth.Increment = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageWidth.Location = new System.Drawing.Point(12, 64);
			this.numericImageWidth.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.numericImageWidth.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageWidth.Name = "numericImageWidth";
			this.numericImageWidth.Size = new System.Drawing.Size(52, 20);
			this.numericImageWidth.TabIndex = 1;
			this.numericImageWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericImageWidth.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// numericImageHeight
			// 
			this.numericImageHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericImageHeight.Increment = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageHeight.Location = new System.Drawing.Point(75, 64);
			this.numericImageHeight.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.numericImageHeight.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageHeight.Name = "numericImageHeight";
			this.numericImageHeight.Size = new System.Drawing.Size(56, 20);
			this.numericImageHeight.TabIndex = 2;
			this.numericImageHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericImageHeight.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// labelImageSize
			// 
			this.labelImageSize.AutoSize = true;
			this.labelImageSize.Location = new System.Drawing.Point(9, 48);
			this.labelImageSize.Name = "labelImageSize";
			this.labelImageSize.Size = new System.Drawing.Size(87, 13);
			this.labelImageSize.TabIndex = 14;
			this.labelImageSize.Text = "Font texture size:";
			// 
			// labelBy
			// 
			this.labelBy.AutoSize = true;
			this.labelBy.BackColor = System.Drawing.Color.Transparent;
			this.labelBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelBy.Location = new System.Drawing.Point(64, 66);
			this.labelBy.Name = "labelBy";
			this.labelBy.Size = new System.Drawing.Size(13, 13);
			this.labelBy.TabIndex = 15;
			this.labelBy.Text = "x";
			// 
			// checkAntiAlias
			// 
			this.checkAntiAlias.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkAntiAlias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkAntiAlias.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.AntiAlias;
			this.checkAntiAlias.Location = new System.Drawing.Point(182, 59);
			this.checkAntiAlias.Name = "checkAntiAlias";
			this.checkAntiAlias.Size = new System.Drawing.Size(29, 27);
			this.checkAntiAlias.TabIndex = 5;
			this.checkAntiAlias.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.labelPreview);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.checkAntiAlias);
			this.groupBox1.Controls.Add(this.comboFontList);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.numericFontSize);
			this.groupBox1.Controls.Add(this.checkBold);
			this.groupBox1.Controls.Add(this.checkUnderline);
			this.groupBox1.Controls.Add(this.checkItalics);
			this.groupBox1.Location = new System.Drawing.Point(12, 90);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(287, 168);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Font";
			// 
			// labelPreview
			// 
			this.labelPreview.BackColor = System.Drawing.Color.White;
			this.labelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelPreview.Location = new System.Drawing.Point(9, 108);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(270, 57);
			this.labelPreview.TabIndex = 7;
			this.labelPreview.Text = "[Fontname] [pointsize]pt";
			this.labelPreview.UseMnemonic = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 95);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Preview:";
			// 
			// FontMetaData
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(311, 300);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelImageSize);
			this.Controls.Add(this.numericImageHeight);
			this.Controls.Add(this.numericImageWidth);
			this.Controls.Add(this.textFontName);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.labelBy);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FontMetaData";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Font metadata";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FontMetaData_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.numericFontSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageHeight)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboFontList;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericFontSize;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textFontName;
		internal System.Windows.Forms.CheckBox checkItalics;
		internal System.Windows.Forms.CheckBox checkUnderline;
		internal System.Windows.Forms.CheckBox checkBold;
		private System.Windows.Forms.OpenFileDialog dialogOpenImage;
		internal System.Windows.Forms.NumericUpDown numericImageWidth;
		internal System.Windows.Forms.NumericUpDown numericImageHeight;
		internal System.Windows.Forms.Label labelImageSize;
		internal System.Windows.Forms.Label labelBy;
		private System.Windows.Forms.CheckBox checkAntiAlias;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelPreview;
	}
}