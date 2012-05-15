namespace GorgonLibrary.GorgonEditor
{
	partial class formNewFont
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNewFont));
			this.label1 = new System.Windows.Forms.Label();
			this.checkBold = new System.Windows.Forms.CheckBox();
			this.checkItalic = new System.Windows.Forms.CheckBox();
			this.checkUnderline = new System.Windows.Forms.CheckBox();
			this.checkStrikeThrough = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.numericSize = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelPreview = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.comboAA = new System.Windows.Forms.ComboBox();
			this.numericTextureWidth = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.numericTextureHeight = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.comboSizeType = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.comboFonts = new GorgonLibrary.GorgonEditor.comboFonts();
			this.buttonOK = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericSize)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTextureWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericTextureHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 52);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Font:";
			// 
			// checkBold
			// 
			this.checkBold.Enabled = false;
			this.checkBold.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkBold.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkBold.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkBold.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkBold.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBold.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBold.Location = new System.Drawing.Point(16, 188);
			this.checkBold.Name = "checkBold";
			this.checkBold.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkBold.Size = new System.Drawing.Size(65, 22);
			this.checkBold.TabIndex = 7;
			this.checkBold.Text = "Bold";
			this.checkBold.UseVisualStyleBackColor = false;
			this.checkBold.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkItalic
			// 
			this.checkItalic.Enabled = false;
			this.checkItalic.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkItalic.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkItalic.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkItalic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkItalic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkItalic.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkItalic.Location = new System.Drawing.Point(87, 188);
			this.checkItalic.Name = "checkItalic";
			this.checkItalic.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkItalic.Size = new System.Drawing.Size(75, 22);
			this.checkItalic.TabIndex = 8;
			this.checkItalic.Text = "Italic";
			this.checkItalic.UseVisualStyleBackColor = false;
			this.checkItalic.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkUnderline
			// 
			this.checkUnderline.Enabled = false;
			this.checkUnderline.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkUnderline.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkUnderline.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkUnderline.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkUnderline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkUnderline.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkUnderline.Location = new System.Drawing.Point(168, 188);
			this.checkUnderline.Name = "checkUnderline";
			this.checkUnderline.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkUnderline.Size = new System.Drawing.Size(98, 22);
			this.checkUnderline.TabIndex = 9;
			this.checkUnderline.Text = "Underline";
			this.checkUnderline.UseVisualStyleBackColor = false;
			this.checkUnderline.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkStrikeThrough
			// 
			this.checkStrikeThrough.Enabled = false;
			this.checkStrikeThrough.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkStrikeThrough.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkStrikeThrough.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkStrikeThrough.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkStrikeThrough.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkStrikeThrough.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkStrikeThrough.Location = new System.Drawing.Point(272, 188);
			this.checkStrikeThrough.Name = "checkStrikeThrough";
			this.checkStrikeThrough.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkStrikeThrough.Size = new System.Drawing.Size(109, 22);
			this.checkStrikeThrough.TabIndex = 10;
			this.checkStrikeThrough.Text = "Strikethrough";
			this.checkStrikeThrough.UseVisualStyleBackColor = false;
			this.checkStrikeThrough.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 97);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(30, 15);
			this.label2.TabIndex = 7;
			this.label2.Text = "Size:";
			// 
			// numericSize
			// 
			this.numericSize.BackColor = System.Drawing.Color.White;
			this.numericSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericSize.DecimalPlaces = 2;
			this.numericSize.Location = new System.Drawing.Point(16, 115);
			this.numericSize.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
			this.numericSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericSize.Name = "numericSize";
			this.numericSize.Size = new System.Drawing.Size(110, 23);
			this.numericSize.TabIndex = 2;
			this.numericSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericSize.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
			this.numericSize.ValueChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(42, 15);
			this.label3.TabIndex = 9;
			this.label3.Text = "Name:";
			// 
			// textName
			// 
			this.textName.BackColor = System.Drawing.Color.White;
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textName.Location = new System.Drawing.Point(16, 26);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(365, 23);
			this.textName.TabIndex = 0;
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.labelPreview);
			this.panel1.Location = new System.Drawing.Point(13, 235);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(365, 61);
			this.panel1.TabIndex = 12;
			// 
			// labelPreview
			// 
			this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelPreview.Location = new System.Drawing.Point(0, 0);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(365, 61);
			this.labelPreview.TabIndex = 0;
			this.labelPreview.Text = "The quick brown fox jumps over the lazy dog.";
			this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonCancel.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(306, 302);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 24);
			this.buttonCancel.TabIndex = 12;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(238, 97);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(116, 15);
			this.label4.TabIndex = 13;
			this.label4.Text = "Anti-aliasing quality:";
			// 
			// comboAA
			// 
			this.comboAA.BackColor = System.Drawing.Color.White;
			this.comboAA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboAA.FormattingEnabled = true;
			this.comboAA.Items.AddRange(new object[] {
            "None",
            "Anti-Alias",
            "Anti-Alias (High Quality)"});
			this.comboAA.Location = new System.Drawing.Point(241, 115);
			this.comboAA.Name = "comboAA";
			this.comboAA.Size = new System.Drawing.Size(140, 23);
			this.comboAA.TabIndex = 4;
			// 
			// numericTextureWidth
			// 
			this.numericTextureWidth.BackColor = System.Drawing.Color.White;
			this.numericTextureWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericTextureWidth.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericTextureWidth.Location = new System.Drawing.Point(16, 159);
			this.numericTextureWidth.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericTextureWidth.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericTextureWidth.Name = "numericTextureWidth";
			this.numericTextureWidth.Size = new System.Drawing.Size(110, 23);
			this.numericTextureWidth.TabIndex = 5;
			this.numericTextureWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericTextureWidth.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(13, 141);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 15);
			this.label5.TabIndex = 16;
			this.label5.Text = "Texture Size:";
			// 
			// numericTextureHeight
			// 
			this.numericTextureHeight.BackColor = System.Drawing.Color.White;
			this.numericTextureHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericTextureHeight.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericTextureHeight.Location = new System.Drawing.Point(150, 159);
			this.numericTextureHeight.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericTextureHeight.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericTextureHeight.Name = "numericTextureHeight";
			this.numericTextureHeight.Size = new System.Drawing.Size(110, 23);
			this.numericTextureHeight.TabIndex = 6;
			this.numericTextureHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericTextureHeight.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(132, 161);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(12, 15);
			this.label6.TabIndex = 18;
			this.label6.Text = "x";
			// 
			// comboSizeType
			// 
			this.comboSizeType.BackColor = System.Drawing.Color.White;
			this.comboSizeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSizeType.FormattingEnabled = true;
			this.comboSizeType.Items.AddRange(new object[] {
            "Points",
            "Pixels"});
			this.comboSizeType.Location = new System.Drawing.Point(132, 115);
			this.comboSizeType.Name = "comboSizeType";
			this.comboSizeType.Size = new System.Drawing.Size(103, 23);
			this.comboSizeType.TabIndex = 3;
			this.comboSizeType.SelectedIndexChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(13, 217);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(51, 15);
			this.label7.TabIndex = 19;
			this.label7.Text = "Preview:";
			// 
			// comboFonts
			// 
			this.comboFonts.BackColor = System.Drawing.Color.White;
			this.comboFonts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFonts.FormattingEnabled = true;
			this.comboFonts.Location = new System.Drawing.Point(16, 70);
			this.comboFonts.Name = "comboFonts";
			this.comboFonts.Size = new System.Drawing.Size(365, 24);
			this.comboFonts.TabIndex = 1;
			this.comboFonts.SelectedIndexChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(225, 302);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 24);
			this.buttonOK.TabIndex = 11;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// formNewFont
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(400, 334);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.comboSizeType);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.numericTextureHeight);
			this.Controls.Add(this.numericTextureWidth);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.comboAA);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.numericSize);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.checkStrikeThrough);
			this.Controls.Add(this.checkUnderline);
			this.Controls.Add(this.checkItalic);
			this.Controls.Add(this.checkBold);
			this.Controls.Add(this.comboFonts);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formNewFont";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Font";
			((System.ComponentModel.ISupportInitialize)(this.numericSize)).EndInit();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericTextureWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericTextureHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private comboFonts comboFonts;
		private System.Windows.Forms.CheckBox checkBold;
		private System.Windows.Forms.CheckBox checkItalic;
		private System.Windows.Forms.CheckBox checkUnderline;
		private System.Windows.Forms.CheckBox checkStrikeThrough;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericSize;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelPreview;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboAA;
		private System.Windows.Forms.NumericUpDown numericTextureWidth;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numericTextureHeight;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox comboSizeType;
		private System.Windows.Forms.Label label7;
	}
}