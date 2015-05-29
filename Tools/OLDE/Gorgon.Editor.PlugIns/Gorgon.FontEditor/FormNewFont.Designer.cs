using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor.FontEditorPlugIn
{
	partial class FormNewFont
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewFont));
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
			this.label4 = new System.Windows.Forms.Label();
			this.comboAA = new System.Windows.Forms.ComboBox();
			this.numericTextureWidth = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.numericTextureHeight = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.comboSizeType = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.tipInfo = new System.Windows.Forms.ToolTip(this.components);
			this.comboFonts = new Gorgon.Editor.FontEditorPlugIn.ComboFonts();
			this.buttonCharacterList = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericSize)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTextureWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericTextureHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(8, 72);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Font Family:";
			this.tipInfo.SetToolTip(this.label1, "The font family.\r\n\r\nThis is the base TrueType font that is used to generate the G" +
        "orgon font.");
			// 
			// checkBold
			// 
			this.checkBold.Enabled = false;
			this.checkBold.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkBold.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkBold.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkBold.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkBold.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBold.ForeColor = System.Drawing.Color.White;
			this.checkBold.Location = new System.Drawing.Point(11, 208);
			this.checkBold.Name = "checkBold";
			this.checkBold.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkBold.Size = new System.Drawing.Size(65, 22);
			this.checkBold.TabIndex = 8;
			this.checkBold.Text = "Bold";
			this.tipInfo.SetToolTip(this.checkBold, "The font style.\r\n\r\nGenerates bold, italicized, underlined and/or strike through c" +
        "haracters.");
			this.checkBold.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkItalic
			// 
			this.checkItalic.Enabled = false;
			this.checkItalic.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkItalic.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkItalic.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkItalic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkItalic.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkItalic.ForeColor = System.Drawing.Color.White;
			this.checkItalic.Location = new System.Drawing.Point(82, 208);
			this.checkItalic.Name = "checkItalic";
			this.checkItalic.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkItalic.Size = new System.Drawing.Size(75, 22);
			this.checkItalic.TabIndex = 9;
			this.checkItalic.Text = "Italic";
			this.tipInfo.SetToolTip(this.checkItalic, "The font style.\r\n\r\nGenerates bold, italicized, underlined and/or strike through c" +
        "haracters.");
			this.checkItalic.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkUnderline
			// 
			this.checkUnderline.Enabled = false;
			this.checkUnderline.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkUnderline.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkUnderline.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkUnderline.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkUnderline.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkUnderline.ForeColor = System.Drawing.Color.White;
			this.checkUnderline.Location = new System.Drawing.Point(167, 208);
			this.checkUnderline.Name = "checkUnderline";
			this.checkUnderline.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkUnderline.Size = new System.Drawing.Size(98, 22);
			this.checkUnderline.TabIndex = 10;
			this.checkUnderline.Text = "Underline";
			this.tipInfo.SetToolTip(this.checkUnderline, "The font style.\r\n\r\nGenerates bold, italicized, underlined and/or strike through c" +
        "haracters.");
			this.checkUnderline.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// checkStrikeThrough
			// 
			this.checkStrikeThrough.Enabled = false;
			this.checkStrikeThrough.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkStrikeThrough.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkStrikeThrough.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkStrikeThrough.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkStrikeThrough.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkStrikeThrough.ForeColor = System.Drawing.Color.White;
			this.checkStrikeThrough.Location = new System.Drawing.Point(271, 208);
			this.checkStrikeThrough.Name = "checkStrikeThrough";
			this.checkStrikeThrough.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkStrikeThrough.Size = new System.Drawing.Size(109, 22);
			this.checkStrikeThrough.TabIndex = 11;
			this.checkStrikeThrough.Text = "Strikethrough";
			this.tipInfo.SetToolTip(this.checkStrikeThrough, "The font style.\r\n\r\nGenerates bold, italicized, underlined and/or strike through c" +
        "haracters.");
			this.checkStrikeThrough.Click += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(8, 117);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(30, 15);
			this.label2.TabIndex = 7;
			this.label2.Text = "Size:";
			this.tipInfo.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
			// 
			// numericSize
			// 
			this.numericSize.BackColor = System.Drawing.Color.White;
			this.numericSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericSize.DecimalPlaces = 2;
			this.numericSize.Location = new System.Drawing.Point(11, 135);
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
			this.numericSize.Size = new System.Drawing.Size(117, 23);
			this.numericSize.TabIndex = 2;
			this.numericSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tipInfo.SetToolTip(this.numericSize, resources.GetString("numericSize.ToolTip"));
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
			this.label3.ForeColor = System.Drawing.Color.White;
			this.label3.Location = new System.Drawing.Point(8, 28);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(42, 15);
			this.label3.TabIndex = 9;
			this.label3.Text = "Name:";
			this.tipInfo.SetToolTip(this.label3, "The name of the font.  \r\n\r\nThis will be used in the file name for the font, and s" +
        "hould use valid file name characters.");
			// 
			// textName
			// 
			this.textName.BackColor = System.Drawing.Color.White;
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textName.Location = new System.Drawing.Point(11, 46);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(376, 23);
			this.textName.TabIndex = 0;
			this.tipInfo.SetToolTip(this.textName, "The name of the font.  \r\n\r\nThis will be used in the file name for the font, and s" +
        "hould use valid file name characters.");
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.labelPreview);
			this.panel1.Location = new System.Drawing.Point(11, 251);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(376, 61);
			this.panel1.TabIndex = 12;
			// 
			// labelPreview
			// 
			this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelPreview.ForeColor = System.Drawing.Color.Black;
			this.labelPreview.Location = new System.Drawing.Point(0, 0);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(376, 61);
			this.labelPreview.TabIndex = 0;
			this.labelPreview.Text = "The quick brown fox jumps over the lazy dog.";
			this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.ForeColor = System.Drawing.Color.White;
			this.label4.Location = new System.Drawing.Point(244, 117);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(116, 15);
			this.label4.TabIndex = 13;
			this.label4.Text = "Anti-aliasing quality:";
			this.tipInfo.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
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
			this.comboAA.Location = new System.Drawing.Point(247, 135);
			this.comboAA.Name = "comboAA";
			this.comboAA.Size = new System.Drawing.Size(140, 23);
			this.comboAA.TabIndex = 4;
			this.tipInfo.SetToolTip(this.comboAA, resources.GetString("comboAA.ToolTip"));
			this.comboAA.SelectedIndexChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
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
			this.numericTextureWidth.Location = new System.Drawing.Point(11, 179);
			this.numericTextureWidth.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericTextureWidth.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericTextureWidth.Name = "numericTextureWidth";
			this.numericTextureWidth.Size = new System.Drawing.Size(69, 23);
			this.numericTextureWidth.TabIndex = 5;
			this.numericTextureWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tipInfo.SetToolTip(this.numericTextureWidth, resources.GetString("numericTextureWidth.ToolTip"));
			this.numericTextureWidth.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.numericTextureWidth.ValueChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.ForeColor = System.Drawing.Color.White;
			this.label5.Location = new System.Drawing.Point(8, 161);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 15);
			this.label5.TabIndex = 16;
			this.label5.Text = "Texture Size:";
			this.tipInfo.SetToolTip(this.label5, resources.GetString("label5.ToolTip"));
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
			this.numericTextureHeight.Location = new System.Drawing.Point(105, 179);
			this.numericTextureHeight.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericTextureHeight.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericTextureHeight.Name = "numericTextureHeight";
			this.numericTextureHeight.Size = new System.Drawing.Size(69, 23);
			this.numericTextureHeight.TabIndex = 6;
			this.numericTextureHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tipInfo.SetToolTip(this.numericTextureHeight, resources.GetString("numericTextureHeight.ToolTip"));
			this.numericTextureHeight.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.numericTextureHeight.ValueChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(87, 181);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(12, 15);
			this.label6.TabIndex = 18;
			this.label6.Text = "x";
			this.tipInfo.SetToolTip(this.label6, resources.GetString("label6.ToolTip"));
			// 
			// comboSizeType
			// 
			this.comboSizeType.BackColor = System.Drawing.Color.White;
			this.comboSizeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSizeType.FormattingEnabled = true;
			this.comboSizeType.Items.AddRange(new object[] {
            "Points",
            "Pixels"});
			this.comboSizeType.Location = new System.Drawing.Point(134, 135);
			this.comboSizeType.Name = "comboSizeType";
			this.comboSizeType.Size = new System.Drawing.Size(107, 23);
			this.comboSizeType.TabIndex = 3;
			this.tipInfo.SetToolTip(this.comboSizeType, resources.GetString("comboSizeType.ToolTip"));
			this.comboSizeType.SelectedIndexChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.ForeColor = System.Drawing.Color.White;
			this.label7.Location = new System.Drawing.Point(8, 233);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(51, 15);
			this.label7.TabIndex = 19;
			this.label7.Text = "Preview:";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(212, 324);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 12;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(305, 324);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 13;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// tipInfo
			// 
			this.tipInfo.AutoPopDelay = 5000;
			this.tipInfo.BackColor = System.Drawing.Color.White;
			this.tipInfo.ForeColor = System.Drawing.Color.Black;
			this.tipInfo.InitialDelay = 1500;
			this.tipInfo.ReshowDelay = 500;
			this.tipInfo.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.tipInfo.ToolTipTitle = "Font Help";
			// 
			// comboFonts
			// 
			this.comboFonts.BackColor = System.Drawing.Color.White;
			this.comboFonts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFonts.FormattingEnabled = true;
			this.comboFonts.Location = new System.Drawing.Point(11, 90);
			this.comboFonts.Name = "comboFonts";
			this.comboFonts.Size = new System.Drawing.Size(376, 24);
			this.comboFonts.TabIndex = 1;
			this.tipInfo.SetToolTip(this.comboFonts, "The font family.\r\n\r\nThis is the base TrueType font that is used to generate the G" +
        "orgon font.");
			this.comboFonts.SelectedIndexChanged += new System.EventHandler(this.comboFonts_SelectedIndexChanged);
			// 
			// buttonCharacterList
			// 
			this.buttonCharacterList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCharacterList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCharacterList.Enabled = false;
			this.buttonCharacterList.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCharacterList.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCharacterList.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCharacterList.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCharacterList.ForeColor = System.Drawing.Color.White;
			this.buttonCharacterList.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCharacterList.Location = new System.Drawing.Point(201, 179);
			this.buttonCharacterList.Name = "buttonCharacterList";
			this.buttonCharacterList.Size = new System.Drawing.Size(186, 23);
			this.buttonCharacterList.TabIndex = 7;
			this.buttonCharacterList.Text = "Font character list...";
			this.tipInfo.SetToolTip(this.buttonCharacterList, resources.GetString("buttonCharacterList.ToolTip"));
			this.buttonCharacterList.UseVisualStyleBackColor = false;
			this.buttonCharacterList.Click += new System.EventHandler(this.buttonCharacterList_Click);
			// 
			// FormNewFont
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(400, 359);
			this.Controls.Add(this.buttonCharacterList);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.comboSizeType);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.numericTextureHeight);
			this.Controls.Add(this.numericTextureWidth);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.comboAA);
			this.Controls.Add(this.label4);
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
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormNewFont";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.ResizeHandleSize = 1;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Font";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.comboFonts, 0);
			this.Controls.SetChildIndex(this.checkBold, 0);
			this.Controls.SetChildIndex(this.checkItalic, 0);
			this.Controls.SetChildIndex(this.checkUnderline, 0);
			this.Controls.SetChildIndex(this.checkStrikeThrough, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.numericSize, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.textName, 0);
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.comboAA, 0);
			this.Controls.SetChildIndex(this.label5, 0);
			this.Controls.SetChildIndex(this.numericTextureWidth, 0);
			this.Controls.SetChildIndex(this.numericTextureHeight, 0);
			this.Controls.SetChildIndex(this.label6, 0);
			this.Controls.SetChildIndex(this.comboSizeType, 0);
			this.Controls.SetChildIndex(this.label7, 0);
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.buttonCharacterList, 0);
			((System.ComponentModel.ISupportInitialize)(this.numericSize)).EndInit();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericTextureWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericTextureHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Label label1;
		private ComboFonts comboFonts;
		private CheckBox checkBold;
		private CheckBox checkItalic;
		private CheckBox checkUnderline;
		private CheckBox checkStrikeThrough;
		private Label label2;
		private NumericUpDown numericSize;
		private Label label3;
		private TextBox textName;
		private Panel panel1;
		private Label labelPreview;
		private Button buttonOK;
		private Button buttonCancel;
		private Label label4;
		private ComboBox comboAA;
		private NumericUpDown numericTextureWidth;
		private Label label5;
		private NumericUpDown numericTextureHeight;
		private Label label6;
		private ComboBox comboSizeType;
		private Label label7;
		private ToolTip tipInfo;
		private Button buttonCharacterList;
	}
}