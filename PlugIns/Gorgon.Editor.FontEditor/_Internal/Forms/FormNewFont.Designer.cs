namespace Gorgon.Editor.FontEditor;

    partial class FormNewFont
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



        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewFont));
        this.LabelFontFamily = new System.Windows.Forms.Label();
        this.CheckBold = new System.Windows.Forms.CheckBox();
        this.CheckItalics = new System.Windows.Forms.CheckBox();
        this.LabelFontName = new System.Windows.Forms.Label();
        this.TextFontName = new System.Windows.Forms.TextBox();
        this.PanelSampleText = new System.Windows.Forms.Panel();
        this.labelPreview = new System.Windows.Forms.Label();
        this.NumericTextureWidth = new System.Windows.Forms.NumericUpDown();
        this.LabelTextureSize = new System.Windows.Forms.Label();
        this.NumericTextureHeight = new System.Windows.Forms.NumericUpDown();
        this.label6 = new System.Windows.Forms.Label();
        this.LabelPreviewHeader = new System.Windows.Forms.Label();
        this.ButtonOK = new System.Windows.Forms.Button();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.ButtonCharacters = new System.Windows.Forms.Button();
        this.NumericFontSize = new System.Windows.Forms.NumericUpDown();
        this.LabelSize = new System.Windows.Forms.Label();
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.ComboFontFamilies = new Gorgon.Editor.FontEditor.ComboFonts();
        this.TableFontOptions = new System.Windows.Forms.TableLayoutPanel();
        this.CheckAntiAliased = new System.Windows.Forms.CheckBox();
        this.RadioPixels = new System.Windows.Forms.RadioButton();
        this.RadioPoints = new System.Windows.Forms.RadioButton();
        this.LabelStyle = new System.Windows.Forms.Label();
        this.TableTextureOptions = new System.Windows.Forms.TableLayoutPanel();
        this.PanelButtons = new System.Windows.Forms.Panel();
        this.PanelSampleText.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericFontSize)).BeginInit();
        this.TableControls.SuspendLayout();
        this.TableFontOptions.SuspendLayout();
        this.TableTextureOptions.SuspendLayout();
        this.PanelButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // LabelFontFamily
        // 
        this.LabelFontFamily.AutoSize = true;
        this.LabelFontFamily.Location = new System.Drawing.Point(3, 50);
        this.LabelFontFamily.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.LabelFontFamily.Name = "LabelFontFamily";
        this.LabelFontFamily.Size = new System.Drawing.Size(72, 15);
        this.LabelFontFamily.TabIndex = 2;
        this.LabelFontFamily.Text = "Font Family:";
        // 
        // CheckBold
        // 
        this.CheckBold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.CheckBold.AutoSize = true;
        this.CheckBold.Enabled = false;
        this.CheckBold.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        this.CheckBold.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
        this.CheckBold.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.CheckBold.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.CheckBold.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.CheckBold.ForeColor = System.Drawing.Color.White;
        this.CheckBold.Location = new System.Drawing.Point(293, 21);
        this.CheckBold.Name = "CheckBold";
        this.CheckBold.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
        this.CheckBold.Size = new System.Drawing.Size(58, 21);
        this.CheckBold.TabIndex = 3;
        this.CheckBold.Text = "Bold";
        this.CheckBold.Click += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // CheckItalics
        // 
        this.CheckItalics.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.CheckItalics.AutoSize = true;
        this.CheckItalics.Enabled = false;
        this.CheckItalics.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        this.CheckItalics.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
        this.CheckItalics.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.CheckItalics.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.CheckItalics.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.CheckItalics.ForeColor = System.Drawing.Color.White;
        this.CheckItalics.Location = new System.Drawing.Point(357, 22);
        this.CheckItalics.Name = "CheckItalics";
        this.CheckItalics.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
        this.CheckItalics.Size = new System.Drawing.Size(56, 21);
        this.CheckItalics.TabIndex = 4;
        this.CheckItalics.Text = "Italic";
        this.CheckItalics.Click += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // LabelFontName
        // 
        this.LabelFontName.AutoSize = true;
        this.LabelFontName.Location = new System.Drawing.Point(3, 3);
        this.LabelFontName.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.LabelFontName.Name = "LabelFontName";
        this.LabelFontName.Size = new System.Drawing.Size(42, 15);
        this.LabelFontName.TabIndex = 0;
        this.LabelFontName.Text = "Name:";
        // 
        // TextFontName
        // 
        this.TextFontName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.TextFontName.BackColor = System.Drawing.Color.White;
        this.TextFontName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.TextFontName.Location = new System.Drawing.Point(3, 21);
        this.TextFontName.Name = "TextFontName";
        this.TextFontName.Size = new System.Drawing.Size(510, 23);
        this.TextFontName.TabIndex = 1;
        this.TextFontName.TextChanged += new System.EventHandler(this.TextFontName_TextChanged);
        // 
        // PanelSampleText
        // 
        this.PanelSampleText.BackColor = System.Drawing.Color.White;
        this.PanelSampleText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.PanelSampleText.Controls.Add(this.labelPreview);
        this.PanelSampleText.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelSampleText.Location = new System.Drawing.Point(3, 214);
        this.PanelSampleText.Name = "PanelSampleText";
        this.PanelSampleText.Size = new System.Drawing.Size(510, 96);
        this.PanelSampleText.TabIndex = 6;
        // 
        // labelPreview
        // 
        this.labelPreview.AutoEllipsis = true;
        this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelPreview.ForeColor = System.Drawing.Color.Black;
        this.labelPreview.Location = new System.Drawing.Point(0, 0);
        this.labelPreview.Name = "labelPreview";
        this.labelPreview.Size = new System.Drawing.Size(508, 94);
        this.labelPreview.TabIndex = 0;
        this.labelPreview.Text = "The quick brown fox jumps over the lazy dog.";
        this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // NumericTextureWidth
        // 
        this.NumericTextureWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericTextureWidth.BackColor = System.Drawing.Color.White;
        this.NumericTextureWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericTextureWidth.Increment = new decimal(new int[] {
        128,
        0,
        0,
        0});
        this.NumericTextureWidth.Location = new System.Drawing.Point(3, 23);
        this.NumericTextureWidth.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTextureWidth.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureWidth.Name = "NumericTextureWidth";
        this.NumericTextureWidth.Size = new System.Drawing.Size(100, 23);
        this.NumericTextureWidth.TabIndex = 1;
        this.NumericTextureWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericTextureWidth.Value = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureWidth.ValueChanged += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // LabelTextureSize
        // 
        this.LabelTextureSize.AutoSize = true;
        this.LabelTextureSize.ForeColor = System.Drawing.Color.White;
        this.LabelTextureSize.Location = new System.Drawing.Point(3, 3);
        this.LabelTextureSize.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.LabelTextureSize.Name = "LabelTextureSize";
        this.LabelTextureSize.Size = new System.Drawing.Size(71, 15);
        this.LabelTextureSize.TabIndex = 0;
        this.LabelTextureSize.Text = "Texture Size:";
        // 
        // NumericTextureHeight
        // 
        this.NumericTextureHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericTextureHeight.BackColor = System.Drawing.Color.White;
        this.NumericTextureHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericTextureHeight.Increment = new decimal(new int[] {
        128,
        0,
        0,
        0});
        this.NumericTextureHeight.Location = new System.Drawing.Point(128, 23);
        this.NumericTextureHeight.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTextureHeight.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureHeight.Name = "NumericTextureHeight";
        this.NumericTextureHeight.Size = new System.Drawing.Size(100, 23);
        this.NumericTextureHeight.TabIndex = 3;
        this.NumericTextureHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericTextureHeight.Value = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureHeight.ValueChanged += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // label6
        // 
        this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.label6.AutoSize = true;
        this.label6.Location = new System.Drawing.Point(109, 27);
        this.label6.Name = "label6";
        this.label6.Size = new System.Drawing.Size(13, 15);
        this.label6.TabIndex = 2;
        this.label6.Text = "x";
        // 
        // LabelPreviewHeader
        // 
        this.LabelPreviewHeader.AutoSize = true;
        this.LabelPreviewHeader.ForeColor = System.Drawing.Color.White;
        this.LabelPreviewHeader.Location = new System.Drawing.Point(3, 196);
        this.LabelPreviewHeader.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.LabelPreviewHeader.Name = "LabelPreviewHeader";
        this.LabelPreviewHeader.Size = new System.Drawing.Size(51, 15);
        this.LabelPreviewHeader.TabIndex = 6;
        this.LabelPreviewHeader.Text = "Preview:";
        // 
        // ButtonOK
        // 
        this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonOK.AutoSize = true;
        this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonOK.Enabled = false;
        this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOK.Location = new System.Drawing.Point(332, 3);
        this.ButtonOK.MinimumSize = new System.Drawing.Size(87, 0);
        this.ButtonOK.Name = "ButtonOK";
        this.ButtonOK.Size = new System.Drawing.Size(87, 27);
        this.ButtonOK.TabIndex = 0;
        this.ButtonOK.Text = "OK";
        this.ButtonOK.UseVisualStyleBackColor = false;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.ForeColor = System.Drawing.Color.White;
        this.ButtonCancel.Location = new System.Drawing.Point(425, 3);
        this.ButtonCancel.MinimumSize = new System.Drawing.Size(87, 0);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(87, 27);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        // 
        // ButtonCharacters
        // 
        this.ButtonCharacters.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.ButtonCharacters.AutoSize = true;
        this.ButtonCharacters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonCharacters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCharacters.Enabled = false;
        this.ButtonCharacters.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCharacters.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCharacters.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCharacters.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCharacters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCharacters.Location = new System.Drawing.Point(312, 21);
        this.ButtonCharacters.Name = "ButtonCharacters";
        this.ButtonCharacters.Size = new System.Drawing.Size(122, 27);
        this.ButtonCharacters.TabIndex = 4;
        this.ButtonCharacters.Text = "Font character list...";
        this.ButtonCharacters.UseVisualStyleBackColor = false;
        this.ButtonCharacters.Click += new System.EventHandler(this.ButtonCharacters_Click);
        // 
        // NumericFontSize
        // 
        this.NumericFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericFontSize.BackColor = System.Drawing.Color.White;
        this.NumericFontSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericFontSize.DecimalPlaces = 2;
        this.NumericFontSize.Location = new System.Drawing.Point(3, 21);
        this.NumericFontSize.Maximum = new decimal(new int[] {
        512,
        0,
        0,
        0});
        this.NumericFontSize.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericFontSize.Name = "NumericFontSize";
        this.NumericFontSize.Size = new System.Drawing.Size(100, 23);
        this.NumericFontSize.TabIndex = 1;
        this.NumericFontSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericFontSize.Value = new decimal(new int[] {
        9,
        0,
        0,
        0});
        this.NumericFontSize.ValueChanged += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // LabelSize
        // 
        this.LabelSize.AutoSize = true;
        this.LabelSize.Location = new System.Drawing.Point(3, 3);
        this.LabelSize.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.LabelSize.Name = "LabelSize";
        this.LabelSize.Size = new System.Drawing.Size(30, 15);
        this.LabelSize.TabIndex = 0;
        this.LabelSize.Text = "Size:";
        // 
        // TableControls
        // 
        this.TableControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableControls.ColumnCount = 2;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableControls.Controls.Add(this.LabelFontName, 0, 0);
        this.TableControls.Controls.Add(this.TextFontName, 0, 1);
        this.TableControls.Controls.Add(this.PanelSampleText, 0, 7);
        this.TableControls.Controls.Add(this.LabelPreviewHeader, 0, 6);
        this.TableControls.Controls.Add(this.LabelFontFamily, 0, 2);
        this.TableControls.Controls.Add(this.ComboFontFamilies, 0, 3);
        this.TableControls.Controls.Add(this.TableFontOptions, 0, 4);
        this.TableControls.Controls.Add(this.TableTextureOptions, 0, 5);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(0, 0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 8;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.Size = new System.Drawing.Size(516, 313);
        this.TableControls.TabIndex = 0;
        // 
        // ComboFontFamilies
        // 
        this.ComboFontFamilies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.ComboFontFamilies.BackColor = System.Drawing.Color.White;
        this.ComboFontFamilies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.ComboFontFamilies.FormattingEnabled = true;
        this.ComboFontFamilies.Location = new System.Drawing.Point(3, 68);
        this.ComboFontFamilies.Name = "ComboFontFamilies";
        this.ComboFontFamilies.Size = new System.Drawing.Size(510, 24);
        this.ComboFontFamilies.TabIndex = 3;
        this.ComboFontFamilies.SelectedIndexChanged += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // TableFontOptions
        // 
        this.TableFontOptions.AutoSize = true;
        this.TableFontOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableFontOptions.ColumnCount = 7;
        this.TableFontOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableFontOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableFontOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableFontOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableFontOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableFontOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableFontOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableFontOptions.Controls.Add(this.CheckAntiAliased, 6, 1);
        this.TableFontOptions.Controls.Add(this.CheckItalics, 5, 1);
        this.TableFontOptions.Controls.Add(this.CheckBold, 4, 1);
        this.TableFontOptions.Controls.Add(this.LabelSize, 0, 0);
        this.TableFontOptions.Controls.Add(this.NumericFontSize, 0, 1);
        this.TableFontOptions.Controls.Add(this.RadioPixels, 2, 1);
        this.TableFontOptions.Controls.Add(this.RadioPoints, 1, 1);
        this.TableFontOptions.Controls.Add(this.LabelStyle, 4, 0);
        this.TableFontOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableFontOptions.Location = new System.Drawing.Point(0, 95);
        this.TableFontOptions.Margin = new System.Windows.Forms.Padding(0);
        this.TableFontOptions.Name = "TableFontOptions";
        this.TableFontOptions.RowCount = 2;
        this.TableFontOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableFontOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableFontOptions.Size = new System.Drawing.Size(516, 47);
        this.TableFontOptions.TabIndex = 4;
        // 
        // CheckAntiAliased
        // 
        this.CheckAntiAliased.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.CheckAntiAliased.AutoSize = true;
        this.CheckAntiAliased.Checked = true;
        this.CheckAntiAliased.CheckState = System.Windows.Forms.CheckState.Checked;
        this.CheckAntiAliased.Location = new System.Drawing.Point(419, 23);
        this.CheckAntiAliased.Name = "CheckAntiAliased";
        this.CheckAntiAliased.Size = new System.Drawing.Size(94, 19);
        this.CheckAntiAliased.TabIndex = 5;
        this.CheckAntiAliased.Text = "Anti-aliased?";
        this.CheckAntiAliased.UseVisualStyleBackColor = true;
        // 
        // RadioPixels
        // 
        this.RadioPixels.AutoSize = true;
        this.RadioPixels.Location = new System.Drawing.Point(173, 21);
        this.RadioPixels.Name = "RadioPixels";
        this.RadioPixels.Size = new System.Drawing.Size(55, 19);
        this.RadioPixels.TabIndex = 7;
        this.RadioPixels.Text = "Pixels";
        this.RadioPixels.UseVisualStyleBackColor = true;
        this.RadioPixels.Click += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // RadioPoints
        // 
        this.RadioPoints.AutoSize = true;
        this.RadioPoints.Checked = true;
        this.RadioPoints.Location = new System.Drawing.Point(109, 21);
        this.RadioPoints.Name = "RadioPoints";
        this.RadioPoints.Size = new System.Drawing.Size(58, 19);
        this.RadioPoints.TabIndex = 6;
        this.RadioPoints.TabStop = true;
        this.RadioPoints.Text = "Points";
        this.RadioPoints.UseVisualStyleBackColor = true;
        this.RadioPoints.Click += new System.EventHandler(this.ComboFontFamilies_SelectedIndexChanged);
        // 
        // LabelStyle
        // 
        this.LabelStyle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.LabelStyle.AutoSize = true;
        this.TableFontOptions.SetColumnSpan(this.LabelStyle, 2);
        this.LabelStyle.Location = new System.Drawing.Point(293, 3);
        this.LabelStyle.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.LabelStyle.Name = "LabelStyle";
        this.LabelStyle.Size = new System.Drawing.Size(35, 15);
        this.LabelStyle.TabIndex = 8;
        this.LabelStyle.Text = "Style:";
        // 
        // TableTextureOptions
        // 
        this.TableTextureOptions.AutoSize = true;
        this.TableTextureOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableTextureOptions.ColumnCount = 4;
        this.TableTextureOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableTextureOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableTextureOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableTextureOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.TableTextureOptions.Controls.Add(this.ButtonCharacters, 3, 1);
        this.TableTextureOptions.Controls.Add(this.LabelTextureSize, 0, 0);
        this.TableTextureOptions.Controls.Add(this.NumericTextureWidth, 0, 1);
        this.TableTextureOptions.Controls.Add(this.label6, 1, 1);
        this.TableTextureOptions.Controls.Add(this.NumericTextureHeight, 2, 1);
        this.TableTextureOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableTextureOptions.Location = new System.Drawing.Point(0, 142);
        this.TableTextureOptions.Margin = new System.Windows.Forms.Padding(0);
        this.TableTextureOptions.Name = "TableTextureOptions";
        this.TableTextureOptions.RowCount = 2;
        this.TableTextureOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableTextureOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableTextureOptions.Size = new System.Drawing.Size(516, 51);
        this.TableTextureOptions.TabIndex = 5;
        // 
        // PanelButtons
        // 
        this.PanelButtons.AutoSize = true;
        this.PanelButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelButtons.Controls.Add(this.ButtonCancel);
        this.PanelButtons.Controls.Add(this.ButtonOK);
        this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.PanelButtons.Location = new System.Drawing.Point(0, 313);
        this.PanelButtons.Name = "PanelButtons";
        this.PanelButtons.Size = new System.Drawing.Size(516, 33);
        this.PanelButtons.TabIndex = 1;
        // 
        // FormNewFont
        // 
        this.AcceptButton = this.ButtonOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(516, 346);
        this.Controls.Add(this.TableControls);
        this.Controls.Add(this.PanelButtons);
        
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FormNewFont";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "New Font";
        this.PanelSampleText.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericFontSize)).EndInit();
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.TableFontOptions.ResumeLayout(false);
        this.TableFontOptions.PerformLayout();
        this.TableTextureOptions.ResumeLayout(false);
        this.TableTextureOptions.PerformLayout();
        this.PanelButtons.ResumeLayout(false);
        this.PanelButtons.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

        }

    

        private System.Windows.Forms.Label LabelFontFamily;
        private ComboFonts ComboFontFamilies;
        private System.Windows.Forms.CheckBox CheckBold;
        private System.Windows.Forms.CheckBox CheckItalics;
        private System.Windows.Forms.Label LabelFontName;
        private System.Windows.Forms.TextBox TextFontName;
        private System.Windows.Forms.Panel PanelSampleText;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.NumericUpDown NumericTextureWidth;
        private System.Windows.Forms.Label LabelTextureSize;
        private System.Windows.Forms.NumericUpDown NumericTextureHeight;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label LabelPreviewHeader;
        private System.Windows.Forms.Button ButtonCharacters;
    private System.Windows.Forms.TableLayoutPanel TableControls;
    private System.Windows.Forms.TableLayoutPanel TableTextureOptions;
    private System.Windows.Forms.Panel PanelButtons;
    private System.Windows.Forms.TableLayoutPanel TableFontOptions;
    private System.Windows.Forms.Label LabelSize;
    private System.Windows.Forms.NumericUpDown NumericFontSize;
    private System.Windows.Forms.CheckBox CheckAntiAliased;
    private System.Windows.Forms.RadioButton RadioPixels;
    private System.Windows.Forms.RadioButton RadioPoints;
    private System.Windows.Forms.Label LabelStyle;
}
