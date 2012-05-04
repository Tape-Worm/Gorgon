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
			this.checkAntiAliased = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.numericSize = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.checkPoints = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelPreview = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.comboFonts = new GorgonLibrary.GorgonEditor.comboFonts();
			((System.ComponentModel.ISupportInitialize)(this.numericSize)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 51);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Font:";
			// 
			// checkBold
			// 
			this.checkBold.BackColor = System.Drawing.SystemColors.Control;
			this.checkBold.Enabled = false;
			this.checkBold.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBold.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBold.Location = new System.Drawing.Point(16, 152);
			this.checkBold.Name = "checkBold";
			this.checkBold.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkBold.Size = new System.Drawing.Size(365, 25);
			this.checkBold.TabIndex = 4;
			this.checkBold.Text = "Bold";
			this.checkBold.UseVisualStyleBackColor = false;
			this.checkBold.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkItalic
			// 
			this.checkItalic.BackColor = System.Drawing.SystemColors.Control;
			this.checkItalic.Enabled = false;
			this.checkItalic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkItalic.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkItalic.Location = new System.Drawing.Point(16, 183);
			this.checkItalic.Name = "checkItalic";
			this.checkItalic.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkItalic.Size = new System.Drawing.Size(365, 25);
			this.checkItalic.TabIndex = 5;
			this.checkItalic.Text = "Italic";
			this.checkItalic.UseVisualStyleBackColor = false;
			this.checkItalic.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkUnderline
			// 
			this.checkUnderline.BackColor = System.Drawing.SystemColors.Control;
			this.checkUnderline.Enabled = false;
			this.checkUnderline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkUnderline.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkUnderline.Location = new System.Drawing.Point(16, 214);
			this.checkUnderline.Name = "checkUnderline";
			this.checkUnderline.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkUnderline.Size = new System.Drawing.Size(365, 25);
			this.checkUnderline.TabIndex = 6;
			this.checkUnderline.Text = "Underline";
			this.checkUnderline.UseVisualStyleBackColor = false;
			this.checkUnderline.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkStrikeThrough
			// 
			this.checkStrikeThrough.BackColor = System.Drawing.SystemColors.Control;
			this.checkStrikeThrough.Enabled = false;
			this.checkStrikeThrough.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkStrikeThrough.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkStrikeThrough.Location = new System.Drawing.Point(16, 245);
			this.checkStrikeThrough.Name = "checkStrikeThrough";
			this.checkStrikeThrough.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkStrikeThrough.Size = new System.Drawing.Size(365, 25);
			this.checkStrikeThrough.TabIndex = 7;
			this.checkStrikeThrough.Text = "Strikethrough";
			this.checkStrikeThrough.UseVisualStyleBackColor = false;
			this.checkStrikeThrough.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkAntiAliased
			// 
			this.checkAntiAliased.BackColor = System.Drawing.SystemColors.Control;
			this.checkAntiAliased.Checked = true;
			this.checkAntiAliased.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAntiAliased.Enabled = false;
			this.checkAntiAliased.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkAntiAliased.Location = new System.Drawing.Point(16, 276);
			this.checkAntiAliased.Name = "checkAntiAliased";
			this.checkAntiAliased.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkAntiAliased.Size = new System.Drawing.Size(365, 25);
			this.checkAntiAliased.TabIndex = 8;
			this.checkAntiAliased.Text = "Anti-aliased";
			this.checkAntiAliased.UseVisualStyleBackColor = false;
			this.checkAntiAliased.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 101);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 17);
			this.label2.TabIndex = 7;
			this.label2.Text = "Size:";
			// 
			// numericSize
			// 
			this.numericSize.DecimalPlaces = 2;
			this.numericSize.Location = new System.Drawing.Point(16, 121);
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
			this.numericSize.Size = new System.Drawing.Size(120, 25);
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
			this.label3.Location = new System.Drawing.Point(13, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 17);
			this.label3.TabIndex = 9;
			this.label3.Text = "Name:";
			// 
			// textName
			// 
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textName.Location = new System.Drawing.Point(16, 30);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(365, 18);
			this.textName.TabIndex = 0;
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// checkPoints
			// 
			this.checkPoints.BackColor = System.Drawing.SystemColors.Control;
			this.checkPoints.Checked = true;
			this.checkPoints.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkPoints.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkPoints.Location = new System.Drawing.Point(142, 122);
			this.checkPoints.Name = "checkPoints";
			this.checkPoints.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkPoints.Size = new System.Drawing.Size(239, 24);
			this.checkPoints.TabIndex = 3;
			this.checkPoints.Text = "Use point size.";
			this.checkPoints.UseVisualStyleBackColor = false;
			this.checkPoints.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.labelPreview);
			this.panel1.Location = new System.Drawing.Point(16, 308);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(365, 45);
			this.panel1.TabIndex = 12;
			// 
			// labelPreview
			// 
			this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelPreview.Location = new System.Drawing.Point(0, 0);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(365, 45);
			this.labelPreview.TabIndex = 0;
			this.labelPreview.Text = "The quick brown fox jumps over the lazy dog.";
			this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Location = new System.Drawing.Point(225, 359);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 27);
			this.buttonOK.TabIndex = 9;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Location = new System.Drawing.Point(306, 359);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 27);
			this.buttonCancel.TabIndex = 10;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// comboFonts
			// 
			this.comboFonts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFonts.FormattingEnabled = true;
			this.comboFonts.Location = new System.Drawing.Point(16, 72);
			this.comboFonts.Name = "comboFonts";
			this.comboFonts.Size = new System.Drawing.Size(365, 26);
			this.comboFonts.TabIndex = 1;
			this.comboFonts.SelectedIndexChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// formNewFont
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDark;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(400, 395);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.checkPoints);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.numericSize);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.checkAntiAliased);
			this.Controls.Add(this.checkStrikeThrough);
			this.Controls.Add(this.checkUnderline);
			this.Controls.Add(this.checkItalic);
			this.Controls.Add(this.checkBold);
			this.Controls.Add(this.comboFonts);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
		private System.Windows.Forms.CheckBox checkAntiAliased;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericSize;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.CheckBox checkPoints;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelPreview;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
	}
}