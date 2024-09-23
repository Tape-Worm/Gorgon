namespace Fetze.WinFormsColor;

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
        this.radioHue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.radioHue.AutoSize = true;
        this.radioHue.Checked = true;
        this.radioHue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.radioHue.Location = new System.Drawing.Point(34, 5);
        this.radioHue.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
        this.radioHue.Name = "radioHue";
        this.radioHue.Size = new System.Drawing.Size(34, 19);
        this.radioHue.TabIndex = 0;
        this.radioHue.TabStop = true;
        this.radioHue.Text = "H";
        this.radioHue.UseVisualStyleBackColor = true;
        this.radioHue.CheckedChanged += this.RadioHue_CheckedChanged;
        // 
        // radioSaturation
        // 
        this.radioSaturation.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.radioSaturation.AutoSize = true;
        this.radioSaturation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.radioSaturation.Location = new System.Drawing.Point(34, 34);
        this.radioSaturation.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
        this.radioSaturation.Name = "radioSaturation";
        this.radioSaturation.Size = new System.Drawing.Size(31, 19);
        this.radioSaturation.TabIndex = 2;
        this.radioSaturation.Text = "S";
        this.radioSaturation.UseVisualStyleBackColor = true;
        this.radioSaturation.CheckedChanged += new System.EventHandler(this.RadioSaturation_CheckedChanged);
        // 
        // radioValue
        // 
        this.radioValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.radioValue.AutoSize = true;
        this.radioValue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.radioValue.Location = new System.Drawing.Point(34, 63);
        this.radioValue.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
        this.radioValue.Name = "radioValue";
        this.radioValue.Size = new System.Drawing.Size(32, 19);
        this.radioValue.TabIndex = 4;
        this.radioValue.Text = "V";
        this.radioValue.UseVisualStyleBackColor = true;
        this.radioValue.CheckedChanged += new System.EventHandler(this.RadioValue_CheckedChanged);
        // 
        // radioRed
        // 
        this.radioRed.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.radioRed.AutoSize = true;
        this.radioRed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.radioRed.Location = new System.Drawing.Point(149, 5);
        this.radioRed.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
        this.radioRed.Name = "radioRed";
        this.radioRed.Size = new System.Drawing.Size(32, 19);
        this.radioRed.TabIndex = 6;
        this.radioRed.Text = "R";
        this.radioRed.UseVisualStyleBackColor = true;
        this.radioRed.CheckedChanged += new System.EventHandler(this.RadioRed_CheckedChanged);
        // 
        // radioBlue
        // 
        this.radioBlue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.radioBlue.AutoSize = true;
        this.radioBlue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.radioBlue.Location = new System.Drawing.Point(149, 63);
        this.radioBlue.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
        this.radioBlue.Name = "radioBlue";
        this.radioBlue.Size = new System.Drawing.Size(32, 19);
        this.radioBlue.TabIndex = 10;
        this.radioBlue.Text = "B";
        this.radioBlue.UseVisualStyleBackColor = true;
        this.radioBlue.CheckedChanged += new System.EventHandler(this.RadioBlue_CheckedChanged);
        // 
        // radioGreen
        // 
        this.radioGreen.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.radioGreen.AutoSize = true;
        this.radioGreen.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.radioGreen.Location = new System.Drawing.Point(149, 34);
        this.radioGreen.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
        this.radioGreen.Name = "radioGreen";
        this.radioGreen.Size = new System.Drawing.Size(33, 19);
        this.radioGreen.TabIndex = 8;
        this.radioGreen.Text = "G";
        this.radioGreen.UseVisualStyleBackColor = true;
        this.radioGreen.CheckedChanged += new System.EventHandler(this.RadioGreen_CheckedChanged);
        // 
        // numHue
        // 
        this.numHue.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.numHue.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        this.numHue.Location = new System.Drawing.Point(74, 3);
        this.numHue.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
        this.numHue.Name = "numHue";
        this.numHue.Size = new System.Drawing.Size(65, 23);
        this.numHue.TabIndex = 1;
        this.numHue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numHue.ValueChanged += new System.EventHandler(this.NumHue_ValueChanged);
        // 
        // numSaturation
        // 
        this.numSaturation.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.numSaturation.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        this.numSaturation.Location = new System.Drawing.Point(74, 32);
        this.numSaturation.Name = "numSaturation";
        this.numSaturation.Size = new System.Drawing.Size(65, 23);
        this.numSaturation.TabIndex = 3;
        this.numSaturation.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numSaturation.ValueChanged += new System.EventHandler(this.NumSaturation_ValueChanged);
        // 
        // numValue
        // 
        this.numValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numValue.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        this.numValue.Location = new System.Drawing.Point(74, 61);
        this.numValue.Name = "numValue";
        this.numValue.Size = new System.Drawing.Size(65, 23);
        this.numValue.TabIndex = 5;
        this.numValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numValue.ValueChanged += new System.EventHandler(this.NumValue_ValueChanged);
        // 
        // numRed
        // 
        this.numRed.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.numRed.Location = new System.Drawing.Point(188, 3);
        this.numRed.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numRed.Name = "numRed";
        this.numRed.Size = new System.Drawing.Size(65, 23);
        this.numRed.TabIndex = 7;
        this.numRed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numRed.ValueChanged += new System.EventHandler(this.NumRed_ValueChanged);
        // 
        // numGreen
        // 
        this.numGreen.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numGreen.Location = new System.Drawing.Point(188, 32);
        this.numGreen.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numGreen.Name = "numGreen";
        this.numGreen.Size = new System.Drawing.Size(65, 23);
        this.numGreen.TabIndex = 9;
        this.numGreen.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numGreen.ValueChanged += new System.EventHandler(this.NumGreen_ValueChanged);
        // 
        // numBlue
        // 
        this.numBlue.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.numBlue.Location = new System.Drawing.Point(188, 61);
        this.numBlue.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numBlue.Name = "numBlue";
        this.numBlue.Size = new System.Drawing.Size(65, 23);
        this.numBlue.TabIndex = 11;
        this.numBlue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numBlue.ValueChanged += new System.EventHandler(this.NumBlue_ValueChanged);
        // 
        // labelHex
        // 
        this.labelHex.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.labelHex.AutoSize = true;
        this.labelHex.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.labelHex.Location = new System.Drawing.Point(271, 65);
        this.labelHex.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
        this.labelHex.Name = "labelHex";
        this.labelHex.Size = new System.Drawing.Size(31, 15);
        this.labelHex.TabIndex = 14;
        this.labelHex.Text = "Web";
        this.labelHex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // labelAlpha
        // 
        this.labelAlpha.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.labelAlpha.AutoSize = true;
        this.labelAlpha.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.labelAlpha.Location = new System.Drawing.Point(264, 7);
        this.labelAlpha.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
        this.labelAlpha.Name = "labelAlpha";
        this.labelAlpha.Size = new System.Drawing.Size(38, 15);
        this.labelAlpha.TabIndex = 12;
        this.labelAlpha.Text = "Alpha";
        this.labelAlpha.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // TableTop
        // 
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
        this.TableTop.Size = new System.Drawing.Size(400, 187);
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
        this.tableLayoutPanel3.Location = new System.Drawing.Point(308, 40);
        this.tableLayoutPanel3.MinimumSize = new System.Drawing.Size(64, 102);
        this.tableLayoutPanel3.Name = "tableLayoutPanel3";
        this.tableLayoutPanel3.RowCount = 3;
        this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel3.Size = new System.Drawing.Size(64, 106);
        this.tableLayoutPanel3.TabIndex = 9;
        // 
        // colorShowBox
        // 
        this.colorShowBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.colorShowBox.AutoSize = true;
        this.colorShowBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.colorShowBox.Color = System.Drawing.Color.DarkRed;
        this.colorShowBox.Location = new System.Drawing.Point(3, 24);
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
        this.label2.Location = new System.Drawing.Point(8, 88);
        this.label2.Margin = new System.Windows.Forms.Padding(3);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(47, 15);
        this.label2.TabIndex = 4;
        this.label2.Text = "Current";
        this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // label1
        // 
        this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.label1.AutoSize = true;
        this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        this.label1.Location = new System.Drawing.Point(19, 3);
        this.label1.Margin = new System.Windows.Forms.Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(26, 15);
        this.label1.TabIndex = 3;
        this.label1.Text = "Old";
        this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // colorPanel
        // 
        this.colorPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.colorPanel.BottomLeftColor = System.Drawing.Color.Black;
        this.colorPanel.BottomRightColor = System.Drawing.Color.Black;
        this.colorPanel.Location = new System.Drawing.Point(28, 6);
        this.colorPanel.Name = "colorPanel";
        this.colorPanel.Size = new System.Drawing.Size(198, 175);
        this.colorPanel.TabIndex = 0;
        this.colorPanel.TopLeftColor = System.Drawing.Color.White;
        this.colorPanel.TopRightColor = System.Drawing.Color.Red;
        this.colorPanel.PercentualValueChanged += new System.EventHandler(this.ColorPanel_PercentualValueChanged);
        // 
        // colorSlider
        // 
        this.colorSlider.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
        this.colorSlider.Location = new System.Drawing.Point(232, 3);
        this.colorSlider.Name = "colorSlider";
        this.colorSlider.ShowInnerPicker = false;
        this.colorSlider.Size = new System.Drawing.Size(32, 181);
        this.colorSlider.TabIndex = 1;
        this.colorSlider.PercentualValueChanged += new System.EventHandler(this.ColorSlider_PercentualValueChanged);
        // 
        // alphaSlider
        // 
        this.alphaSlider.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
        this.alphaSlider.Location = new System.Drawing.Point(270, 3);
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
        this.TableBottom.ColumnCount = 8;
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableBottom.Controls.Add(this.textBoxHex, 6, 2);
        this.TableBottom.Controls.Add(this.labelHex, 5, 2);
        this.TableBottom.Controls.Add(this.numAlpha, 6, 0);
        this.TableBottom.Controls.Add(this.numGreen, 4, 1);
        this.TableBottom.Controls.Add(this.labelAlpha, 5, 0);
        this.TableBottom.Controls.Add(this.radioBlue, 3, 2);
        this.TableBottom.Controls.Add(this.numBlue, 4, 2);
        this.TableBottom.Controls.Add(this.radioGreen, 3, 1);
        this.TableBottom.Controls.Add(this.numRed, 4, 0);
        this.TableBottom.Controls.Add(this.radioRed, 3, 0);
        this.TableBottom.Controls.Add(this.numValue, 2, 2);
        this.TableBottom.Controls.Add(this.radioValue, 1, 2);
        this.TableBottom.Controls.Add(this.numSaturation, 2, 1);
        this.TableBottom.Controls.Add(this.radioSaturation, 1, 1);
        this.TableBottom.Controls.Add(this.radioHue, 1, 0);
        this.TableBottom.Controls.Add(this.numHue, 2, 0);
        this.TableBottom.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableBottom.Location = new System.Drawing.Point(0, 187);
        this.TableBottom.Name = "TableBottom";
        this.TableBottom.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
        this.TableBottom.RowCount = 4;
        this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableBottom.Size = new System.Drawing.Size(400, 88);
        this.TableBottom.TabIndex = 0;
        // 
        // textBoxHex
        // 
        this.textBoxHex.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.textBoxHex.Location = new System.Drawing.Point(308, 61);
        this.textBoxHex.Name = "textBoxHex";
        this.textBoxHex.Size = new System.Drawing.Size(65, 23);
        this.textBoxHex.TabIndex = 15;
        this.textBoxHex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.textBoxHex.TextChanged += new System.EventHandler(this.TextBoxHex_TextChanged);
        // 
        // numAlpha
        // 
        this.numAlpha.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.numAlpha.Location = new System.Drawing.Point(308, 3);
        this.numAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numAlpha.Name = "numAlpha";
        this.numAlpha.Size = new System.Drawing.Size(65, 23);
        this.numAlpha.TabIndex = 13;
        this.numAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numAlpha.ValueChanged += new System.EventHandler(this.NumAlpha_ValueChanged);
        // 
        // ColorPickerPanel
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.TableBottom);
        this.Controls.Add(this.TableTop);
        this.Name = "ColorPickerPanel";
        this.Size = new System.Drawing.Size(400, 275);
        ((System.ComponentModel.ISupportInitialize)this.numHue).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numSaturation).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numValue).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numRed).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numGreen).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numBlue).EndInit();
        this.TableTop.ResumeLayout(false);
        this.TableTop.PerformLayout();
        this.tableLayoutPanel3.ResumeLayout(false);
        this.tableLayoutPanel3.PerformLayout();
        this.TableBottom.ResumeLayout(false);
        this.TableBottom.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)this.numAlpha).EndInit();
        this.ResumeLayout(false);
    }

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
