namespace Fetze.WinFormsColor
{
	partial class ColorPickerPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorPickerPanel));
            this.radioHue = new System.Windows.Forms.RadioButton();
            this.radioSaturation = new System.Windows.Forms.RadioButton();
            this.radioValue = new System.Windows.Forms.RadioButton();
            this.radioRed = new System.Windows.Forms.RadioButton();
            this.radioBlue = new System.Windows.Forms.RadioButton();
            this.radioGreen = new System.Windows.Forms.RadioButton();
            this.numHue = new System.Windows.Forms.NumericUpDown();
            this.numSaturation = new System.Windows.Forms.NumericUpDown();
            this.numValue = new System.Windows.Forms.NumericUpDown();
            this.numRed = new System.Windows.Forms.NumericUpDown();
            this.numGreen = new System.Windows.Forms.NumericUpDown();
            this.numBlue = new System.Windows.Forms.NumericUpDown();
            this.labelHex = new System.Windows.Forms.Label();
            this.labelAlpha = new System.Windows.Forms.Label();
            this.TableTop = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.colorShowBox = new Fetze.WinFormsColor.ColorShowBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.colorPanel = new Fetze.WinFormsColor.ColorPanel();
            this.colorSlider = new Fetze.WinFormsColor.ColorSlider();
            this.alphaSlider = new Fetze.WinFormsColor.ColorSlider();
            this.TableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxHex = new System.Windows.Forms.TextBox();
            this.numAlpha = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlue)).BeginInit();
            this.TableTop.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.TableBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAlpha)).BeginInit();
            this.SuspendLayout();
            // 
            // radioHue
            // 
            this.radioHue.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioHue.AutoSize = true;
            this.radioHue.Checked = true;
            this.radioHue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioHue.Location = new System.Drawing.Point(3, 4);
            this.radioHue.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.radioHue.Name = "radioHue";
            this.radioHue.Size = new System.Drawing.Size(33, 17);
            this.radioHue.TabIndex = 0;
            this.radioHue.TabStop = true;
            this.radioHue.Text = "H";
            this.radioHue.UseVisualStyleBackColor = true;
            this.radioHue.CheckedChanged += new System.EventHandler(this.RadioHue_CheckedChanged);
            // 
            // radioSaturation
            // 
            this.radioSaturation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioSaturation.AutoSize = true;
            this.radioSaturation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioSaturation.Location = new System.Drawing.Point(4, 27);
            this.radioSaturation.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.radioSaturation.Name = "radioSaturation";
            this.radioSaturation.Size = new System.Drawing.Size(32, 17);
            this.radioSaturation.TabIndex = 2;
            this.radioSaturation.Text = "S";
            this.radioSaturation.UseVisualStyleBackColor = true;
            this.radioSaturation.CheckedChanged += new System.EventHandler(this.RadioSaturation_CheckedChanged);
            // 
            // radioValue
            // 
            this.radioValue.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioValue.AutoSize = true;
            this.radioValue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioValue.Location = new System.Drawing.Point(4, 50);
            this.radioValue.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.radioValue.Name = "radioValue";
            this.radioValue.Size = new System.Drawing.Size(32, 17);
            this.radioValue.TabIndex = 4;
            this.radioValue.Text = "V";
            this.radioValue.UseVisualStyleBackColor = true;
            this.radioValue.CheckedChanged += new System.EventHandler(this.RadioValue_CheckedChanged);
            // 
            // radioRed
            // 
            this.radioRed.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioRed.AutoSize = true;
            this.radioRed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioRed.Location = new System.Drawing.Point(113, 4);
            this.radioRed.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.radioRed.Name = "radioRed";
            this.radioRed.Size = new System.Drawing.Size(33, 17);
            this.radioRed.TabIndex = 6;
            this.radioRed.Text = "R";
            this.radioRed.UseVisualStyleBackColor = true;
            this.radioRed.CheckedChanged += new System.EventHandler(this.RadioRed_CheckedChanged);
            // 
            // radioBlue
            // 
            this.radioBlue.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioBlue.AutoSize = true;
            this.radioBlue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioBlue.Location = new System.Drawing.Point(114, 50);
            this.radioBlue.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.radioBlue.Name = "radioBlue";
            this.radioBlue.Size = new System.Drawing.Size(32, 17);
            this.radioBlue.TabIndex = 10;
            this.radioBlue.Text = "B";
            this.radioBlue.UseVisualStyleBackColor = true;
            this.radioBlue.CheckedChanged += new System.EventHandler(this.RadioBlue_CheckedChanged);
            // 
            // radioGreen
            // 
            this.radioGreen.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioGreen.AutoSize = true;
            this.radioGreen.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioGreen.Location = new System.Drawing.Point(113, 27);
            this.radioGreen.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.radioGreen.Name = "radioGreen";
            this.radioGreen.Size = new System.Drawing.Size(33, 17);
            this.radioGreen.TabIndex = 8;
            this.radioGreen.Text = "G";
            this.radioGreen.UseVisualStyleBackColor = true;
            this.radioGreen.CheckedChanged += new System.EventHandler(this.RadioGreen_CheckedChanged);
            // 
            // numHue
            // 
            this.numHue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numHue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numHue.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numHue.Location = new System.Drawing.Point(42, 3);
            this.numHue.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.numHue.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numHue.Name = "numHue";
            this.numHue.Size = new System.Drawing.Size(65, 20);
            this.numHue.TabIndex = 1;
            this.numHue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numHue.ValueChanged += new System.EventHandler(this.NumHue_ValueChanged);
            // 
            // numSaturation
            // 
            this.numSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numSaturation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numSaturation.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numSaturation.Location = new System.Drawing.Point(42, 26);
            this.numSaturation.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.numSaturation.Name = "numSaturation";
            this.numSaturation.Size = new System.Drawing.Size(65, 20);
            this.numSaturation.TabIndex = 3;
            this.numSaturation.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSaturation.ValueChanged += new System.EventHandler(this.NumSaturation_ValueChanged);
            // 
            // numValue
            // 
            this.numValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numValue.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numValue.Location = new System.Drawing.Point(42, 49);
            this.numValue.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.numValue.Name = "numValue";
            this.numValue.Size = new System.Drawing.Size(65, 20);
            this.numValue.TabIndex = 5;
            this.numValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numValue.ValueChanged += new System.EventHandler(this.NumValue_ValueChanged);
            // 
            // numRed
            // 
            this.numRed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numRed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numRed.Location = new System.Drawing.Point(152, 3);
            this.numRed.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.numRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numRed.Name = "numRed";
            this.numRed.Size = new System.Drawing.Size(65, 20);
            this.numRed.TabIndex = 7;
            this.numRed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numRed.ValueChanged += new System.EventHandler(this.NumRed_ValueChanged);
            // 
            // numGreen
            // 
            this.numGreen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numGreen.Location = new System.Drawing.Point(152, 26);
            this.numGreen.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.numGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numGreen.Name = "numGreen";
            this.numGreen.Size = new System.Drawing.Size(65, 20);
            this.numGreen.TabIndex = 9;
            this.numGreen.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numGreen.ValueChanged += new System.EventHandler(this.NumGreen_ValueChanged);
            // 
            // numBlue
            // 
            this.numBlue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numBlue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numBlue.Location = new System.Drawing.Point(152, 49);
            this.numBlue.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.numBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBlue.Name = "numBlue";
            this.numBlue.Size = new System.Drawing.Size(65, 20);
            this.numBlue.TabIndex = 11;
            this.numBlue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numBlue.ValueChanged += new System.EventHandler(this.NumBlue_ValueChanged);
            // 
            // labelHex
            // 
            this.labelHex.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelHex.AutoSize = true;
            this.labelHex.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelHex.Location = new System.Drawing.Point(227, 52);
            this.labelHex.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelHex.Name = "labelHex";
            this.labelHex.Size = new System.Drawing.Size(30, 13);
            this.labelHex.TabIndex = 14;
            this.labelHex.Text = "Web";
            // 
            // labelAlpha
            // 
            this.labelAlpha.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelAlpha.AutoSize = true;
            this.labelAlpha.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelAlpha.Location = new System.Drawing.Point(223, 6);
            this.labelAlpha.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelAlpha.Name = "labelAlpha";
            this.labelAlpha.Size = new System.Drawing.Size(34, 13);
            this.labelAlpha.TabIndex = 12;
            this.labelAlpha.Text = "Alpha";
            // 
            // TableTop
            // 
            this.TableTop.AutoSize = true;
            this.TableTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableTop.ColumnCount = 6;
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableTop.Controls.Add(this.tableLayoutPanel3, 4, 0);
            this.TableTop.Controls.Add(this.colorPanel, 1, 0);
            this.TableTop.Controls.Add(this.colorSlider, 2, 0);
            this.TableTop.Controls.Add(this.alphaSlider, 3, 0);
            this.TableTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.TableTop.Location = new System.Drawing.Point(0, 0);
            this.TableTop.Name = "TableTop";
            this.TableTop.RowCount = 1;
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.Size = new System.Drawing.Size(363, 187);
            this.TableTop.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.colorShowBox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(289, 42);
            this.tableLayoutPanel3.MinimumSize = new System.Drawing.Size(64, 102);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(64, 102);
            this.tableLayoutPanel3.TabIndex = 9;
            // 
            // colorShowBox
            // 
            this.colorShowBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorShowBox.AutoSize = true;
            this.colorShowBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colorShowBox.Color = System.Drawing.Color.DarkRed;
            this.colorShowBox.Location = new System.Drawing.Point(3, 22);
            this.colorShowBox.LowerColor = System.Drawing.Color.Maroon;
            this.colorShowBox.Name = "colorShowBox";
            this.colorShowBox.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.colorShowBox.Size = new System.Drawing.Size(58, 58);
            this.colorShowBox.TabIndex = 2;
            this.colorShowBox.UpperColor = System.Drawing.Color.DarkRed;
            this.colorShowBox.UpperClick += new System.EventHandler(this.ColorShowBox_UpperClick);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(11, 86);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Current";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(20, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Old";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // colorPanel
            // 
            this.colorPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.colorPanel.BottomLeftColor = System.Drawing.Color.Black;
            this.colorPanel.BottomRightColor = System.Drawing.Color.Black;
            this.colorPanel.Location = new System.Drawing.Point(9, 6);
            this.colorPanel.Name = "colorPanel";
            this.colorPanel.Size = new System.Drawing.Size(198, 175);
            this.colorPanel.TabIndex = 0;
            this.colorPanel.TopLeftColor = System.Drawing.Color.White;
            this.colorPanel.TopRightColor = System.Drawing.Color.Red;
            this.colorPanel.ValuePercentual = ((System.Drawing.PointF)(resources.GetObject("colorPanel.ValuePercentual")));
            this.colorPanel.PercentualValueChanged += new System.EventHandler(this.ColorPanel_PercentualValueChanged);
            // 
            // colorSlider
            // 
            this.colorSlider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.colorSlider.Location = new System.Drawing.Point(213, 3);
            this.colorSlider.Name = "colorSlider";
            this.colorSlider.ShowInnerPicker = false;
            this.colorSlider.Size = new System.Drawing.Size(32, 181);
            this.colorSlider.TabIndex = 1;
            this.colorSlider.PercentualValueChanged += new System.EventHandler(this.ColorSlider_PercentualValueChanged);
            // 
            // alphaSlider
            // 
            this.alphaSlider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.alphaSlider.Location = new System.Drawing.Point(251, 3);
            this.alphaSlider.Maximum = System.Drawing.Color.White;
            this.alphaSlider.Minimum = System.Drawing.Color.Transparent;
            this.alphaSlider.Name = "alphaSlider";
            this.alphaSlider.ShowInnerPicker = false;
            this.alphaSlider.Size = new System.Drawing.Size(32, 181);
            this.alphaSlider.TabIndex = 2;
            this.alphaSlider.PercentualValueChanged += new System.EventHandler(this.AlphaSlider_PercentualValueChanged);
            // 
            // TableBottom
            // 
            this.TableBottom.AutoSize = true;
            this.TableBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableBottom.ColumnCount = 7;
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableBottom.Controls.Add(this.textBoxHex, 5, 2);
            this.TableBottom.Controls.Add(this.labelHex, 4, 2);
            this.TableBottom.Controls.Add(this.numAlpha, 5, 0);
            this.TableBottom.Controls.Add(this.numGreen, 3, 1);
            this.TableBottom.Controls.Add(this.labelAlpha, 4, 0);
            this.TableBottom.Controls.Add(this.radioBlue, 2, 2);
            this.TableBottom.Controls.Add(this.numBlue, 3, 2);
            this.TableBottom.Controls.Add(this.radioGreen, 2, 1);
            this.TableBottom.Controls.Add(this.numRed, 3, 0);
            this.TableBottom.Controls.Add(this.radioRed, 2, 0);
            this.TableBottom.Controls.Add(this.numValue, 1, 2);
            this.TableBottom.Controls.Add(this.radioValue, 0, 2);
            this.TableBottom.Controls.Add(this.numSaturation, 1, 1);
            this.TableBottom.Controls.Add(this.radioSaturation, 0, 1);
            this.TableBottom.Controls.Add(this.radioHue, 0, 0);
            this.TableBottom.Controls.Add(this.numHue, 1, 0);
            this.TableBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableBottom.Location = new System.Drawing.Point(0, 187);
            this.TableBottom.Name = "TableBottom";
            this.TableBottom.RowCount = 4;
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableBottom.Size = new System.Drawing.Size(363, 88);
            this.TableBottom.TabIndex = 0;
            // 
            // textBoxHex
            // 
            this.textBoxHex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxHex.Location = new System.Drawing.Point(263, 49);
            this.textBoxHex.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.textBoxHex.Name = "textBoxHex";
            this.textBoxHex.Size = new System.Drawing.Size(65, 20);
            this.textBoxHex.TabIndex = 15;
            this.textBoxHex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxHex.TextChanged += new System.EventHandler(this.TextBoxHex_TextChanged);
            // 
            // numAlpha
            // 
            this.numAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numAlpha.Location = new System.Drawing.Point(263, 3);
            this.numAlpha.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.numAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numAlpha.Name = "numAlpha";
            this.numAlpha.Size = new System.Drawing.Size(65, 20);
            this.numAlpha.TabIndex = 13;
            this.numAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numAlpha.ValueChanged += new System.EventHandler(this.NumAlpha_ValueChanged);
            // 
            // ColorPickerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.TableBottom);
            this.Controls.Add(this.TableTop);
            this.Name = "ColorPickerPanel";
            this.Size = new System.Drawing.Size(363, 275);
            ((System.ComponentModel.ISupportInitialize)(this.numHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlue)).EndInit();
            this.TableTop.ResumeLayout(false);
            this.TableTop.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.TableBottom.ResumeLayout(false);
            this.TableBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAlpha)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.RadioButton radioHue;
		private System.Windows.Forms.RadioButton radioSaturation;
		private System.Windows.Forms.RadioButton radioValue;
		private System.Windows.Forms.RadioButton radioRed;
		private System.Windows.Forms.RadioButton radioBlue;
		private System.Windows.Forms.RadioButton radioGreen;
		private System.Windows.Forms.NumericUpDown numHue;
		private System.Windows.Forms.NumericUpDown numSaturation;
		private System.Windows.Forms.NumericUpDown numValue;
		private System.Windows.Forms.NumericUpDown numRed;
		private System.Windows.Forms.NumericUpDown numGreen;
		private System.Windows.Forms.NumericUpDown numBlue;
		private System.Windows.Forms.Label labelHex;
        private System.Windows.Forms.Label labelAlpha;
        private ColorSlider alphaSlider;
        private ColorSlider colorSlider;
        private ColorPanel colorPanel;
        private System.Windows.Forms.TableLayoutPanel TableTop;
        private ColorShowBox colorShowBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel TableBottom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox textBoxHex;
        private System.Windows.Forms.NumericUpDown numAlpha;
    }
}