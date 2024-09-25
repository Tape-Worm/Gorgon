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
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorPickerPanel));
        this.radioHue = new RadioButton();
        this.radioSaturation = new RadioButton();
        this.radioValue = new RadioButton();
        this.radioRed = new RadioButton();
        this.radioBlue = new RadioButton();
        this.radioGreen = new RadioButton();
        this.numHue = new NumericUpDown();
        this.numSaturation = new NumericUpDown();
        this.numValue = new NumericUpDown();
        this.numRed = new NumericUpDown();
        this.numGreen = new NumericUpDown();
        this.numBlue = new NumericUpDown();
        this.labelHex = new Label();
        this.labelAlpha = new Label();
        this.TableTop = new TableLayoutPanel();
        this.tableLayoutPanel3 = new TableLayoutPanel();
        this.colorShowBox = new ColorShowBox();
        this.label2 = new Label();
        this.label1 = new Label();
        this.colorPanel = new ColorPanel();
        this.colorSlider = new ColorSlider();
        this.alphaSlider = new ColorSlider();
        this.TableBottom = new TableLayoutPanel();
        this.textBoxHex = new TextBox();
        this.numAlpha = new NumericUpDown();
        ((System.ComponentModel.ISupportInitialize)this.numHue).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numSaturation).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numValue).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numRed).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numGreen).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numBlue).BeginInit();
        this.TableTop.SuspendLayout();
        this.tableLayoutPanel3.SuspendLayout();
        this.TableBottom.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)this.numAlpha).BeginInit();
        this.SuspendLayout();
        // 
        // radioHue
        // 
        this.radioHue.Anchor = AnchorStyles.Left;
        this.radioHue.AutoSize = true;
        this.radioHue.Checked = true;
        this.radioHue.ImeMode = ImeMode.NoControl;
        this.radioHue.Location = new Point(34, 5);
        this.radioHue.Margin = new Padding(7, 3, 3, 3);
        this.radioHue.Name = "radioHue";
        this.radioHue.Size = new Size(34, 19);
        this.radioHue.TabIndex = 0;
        this.radioHue.TabStop = true;
        this.radioHue.Text = "H";
        this.radioHue.UseVisualStyleBackColor = true;
        this.radioHue.CheckedChanged += this.RadioHue_CheckedChanged;
        // 
        // radioSaturation
        // 
        this.radioSaturation.Anchor = AnchorStyles.Left;
        this.radioSaturation.AutoSize = true;
        this.radioSaturation.ImeMode = ImeMode.NoControl;
        this.radioSaturation.Location = new Point(34, 34);
        this.radioSaturation.Margin = new Padding(7, 3, 3, 3);
        this.radioSaturation.Name = "radioSaturation";
        this.radioSaturation.Size = new Size(31, 19);
        this.radioSaturation.TabIndex = 2;
        this.radioSaturation.Text = "S";
        this.radioSaturation.UseVisualStyleBackColor = true;
        this.radioSaturation.CheckedChanged += this.RadioSaturation_CheckedChanged;
        // 
        // radioValue
        // 
        this.radioValue.Anchor = AnchorStyles.Left;
        this.radioValue.AutoSize = true;
        this.radioValue.ImeMode = ImeMode.NoControl;
        this.radioValue.Location = new Point(34, 63);
        this.radioValue.Margin = new Padding(7, 3, 3, 3);
        this.radioValue.Name = "radioValue";
        this.radioValue.Size = new Size(32, 19);
        this.radioValue.TabIndex = 4;
        this.radioValue.Text = "V";
        this.radioValue.UseVisualStyleBackColor = true;
        this.radioValue.CheckedChanged += this.RadioValue_CheckedChanged;
        // 
        // radioRed
        // 
        this.radioRed.Anchor = AnchorStyles.Left;
        this.radioRed.AutoSize = true;
        this.radioRed.ImeMode = ImeMode.NoControl;
        this.radioRed.Location = new Point(149, 5);
        this.radioRed.Margin = new Padding(7, 3, 3, 3);
        this.radioRed.Name = "radioRed";
        this.radioRed.Size = new Size(32, 19);
        this.radioRed.TabIndex = 6;
        this.radioRed.Text = "R";
        this.radioRed.UseVisualStyleBackColor = true;
        this.radioRed.CheckedChanged += this.RadioRed_CheckedChanged;
        // 
        // radioBlue
        // 
        this.radioBlue.Anchor = AnchorStyles.Left;
        this.radioBlue.AutoSize = true;
        this.radioBlue.ImeMode = ImeMode.NoControl;
        this.radioBlue.Location = new Point(149, 63);
        this.radioBlue.Margin = new Padding(7, 3, 3, 3);
        this.radioBlue.Name = "radioBlue";
        this.radioBlue.Size = new Size(32, 19);
        this.radioBlue.TabIndex = 10;
        this.radioBlue.Text = "B";
        this.radioBlue.UseVisualStyleBackColor = true;
        this.radioBlue.CheckedChanged += this.RadioBlue_CheckedChanged;
        // 
        // radioGreen
        // 
        this.radioGreen.Anchor = AnchorStyles.Left;
        this.radioGreen.AutoSize = true;
        this.radioGreen.ImeMode = ImeMode.NoControl;
        this.radioGreen.Location = new Point(149, 34);
        this.radioGreen.Margin = new Padding(7, 3, 3, 3);
        this.radioGreen.Name = "radioGreen";
        this.radioGreen.Size = new Size(33, 19);
        this.radioGreen.TabIndex = 8;
        this.radioGreen.Text = "G";
        this.radioGreen.UseVisualStyleBackColor = true;
        this.radioGreen.CheckedChanged += this.RadioGreen_CheckedChanged;
        // 
        // numHue
        // 
        this.numHue.Anchor = AnchorStyles.None;
        this.numHue.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        this.numHue.Location = new Point(74, 3);
        this.numHue.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
        this.numHue.Name = "numHue";
        this.numHue.Size = new Size(65, 23);
        this.numHue.TabIndex = 1;
        this.numHue.TextAlign = HorizontalAlignment.Right;
        this.numHue.ValueChanged += this.NumHue_ValueChanged;
        // 
        // numSaturation
        // 
        this.numSaturation.Anchor = AnchorStyles.None;
        this.numSaturation.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        this.numSaturation.Location = new Point(74, 32);
        this.numSaturation.Name = "numSaturation";
        this.numSaturation.Size = new Size(65, 23);
        this.numSaturation.TabIndex = 3;
        this.numSaturation.TextAlign = HorizontalAlignment.Right;
        this.numSaturation.ValueChanged += this.NumSaturation_ValueChanged;
        // 
        // numValue
        // 
        this.numValue.Anchor = AnchorStyles.None;
        this.numValue.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        this.numValue.Location = new Point(74, 61);
        this.numValue.Name = "numValue";
        this.numValue.Size = new Size(65, 23);
        this.numValue.TabIndex = 5;
        this.numValue.TextAlign = HorizontalAlignment.Right;
        this.numValue.ValueChanged += this.NumValue_ValueChanged;
        // 
        // numRed
        // 
        this.numRed.Anchor = AnchorStyles.None;
        this.numRed.Location = new Point(188, 3);
        this.numRed.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numRed.Name = "numRed";
        this.numRed.Size = new Size(65, 23);
        this.numRed.TabIndex = 7;
        this.numRed.TextAlign = HorizontalAlignment.Right;
        this.numRed.ValueChanged += this.NumRed_ValueChanged;
        // 
        // numGreen
        // 
        this.numGreen.Anchor = AnchorStyles.None;
        this.numGreen.BorderStyle = BorderStyle.FixedSingle;
        this.numGreen.Location = new Point(188, 32);
        this.numGreen.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numGreen.Name = "numGreen";
        this.numGreen.Size = new Size(65, 23);
        this.numGreen.TabIndex = 9;
        this.numGreen.TextAlign = HorizontalAlignment.Right;
        this.numGreen.ValueChanged += this.NumGreen_ValueChanged;
        // 
        // numBlue
        // 
        this.numBlue.Anchor = AnchorStyles.None;
        this.numBlue.Location = new Point(188, 61);
        this.numBlue.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numBlue.Name = "numBlue";
        this.numBlue.Size = new Size(65, 23);
        this.numBlue.TabIndex = 11;
        this.numBlue.TextAlign = HorizontalAlignment.Right;
        this.numBlue.ValueChanged += this.NumBlue_ValueChanged;
        // 
        // labelHex
        // 
        this.labelHex.Anchor = AnchorStyles.Right;
        this.labelHex.AutoSize = true;
        this.labelHex.ImeMode = ImeMode.NoControl;
        this.labelHex.Location = new Point(271, 65);
        this.labelHex.Margin = new Padding(8, 0, 3, 0);
        this.labelHex.Name = "labelHex";
        this.labelHex.Size = new Size(31, 15);
        this.labelHex.TabIndex = 14;
        this.labelHex.Text = "Web";
        this.labelHex.TextAlign = ContentAlignment.MiddleRight;
        // 
        // labelAlpha
        // 
        this.labelAlpha.Anchor = AnchorStyles.Right;
        this.labelAlpha.AutoSize = true;
        this.labelAlpha.ImeMode = ImeMode.NoControl;
        this.labelAlpha.Location = new Point(264, 7);
        this.labelAlpha.Margin = new Padding(8, 0, 3, 0);
        this.labelAlpha.Name = "labelAlpha";
        this.labelAlpha.Size = new Size(38, 15);
        this.labelAlpha.TabIndex = 12;
        this.labelAlpha.Text = "Alpha";
        this.labelAlpha.TextAlign = ContentAlignment.MiddleRight;
        // 
        // TableTop
        // 
        this.TableTop.ColumnCount = 6;
        this.TableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        this.TableTop.ColumnStyles.Add(new ColumnStyle());
        this.TableTop.ColumnStyles.Add(new ColumnStyle());
        this.TableTop.ColumnStyles.Add(new ColumnStyle());
        this.TableTop.ColumnStyles.Add(new ColumnStyle());
        this.TableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        this.TableTop.Controls.Add(this.colorPanel, 1, 0);
        this.TableTop.Controls.Add(this.colorSlider, 2, 0);
        this.TableTop.Controls.Add(this.alphaSlider, 3, 0);
        this.TableTop.Controls.Add(this.tableLayoutPanel3, 4, 0);
        this.TableTop.Dock = DockStyle.Top;
        this.TableTop.Location = new Point(0, 0);
        this.TableTop.Name = "TableTop";
        this.TableTop.RowCount = 1;
        this.TableTop.RowStyles.Add(new RowStyle());
        this.TableTop.Size = new Size(400, 187);
        this.TableTop.TabIndex = 1;
        // 
        // tableLayoutPanel3
        // 
        this.tableLayoutPanel3.Anchor = AnchorStyles.Left;
        this.tableLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.tableLayoutPanel3.ColumnCount = 1;
        this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.tableLayoutPanel3.Controls.Add(this.colorShowBox, 0, 1);
        this.tableLayoutPanel3.Controls.Add(this.label2, 0, 2);
        this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
        this.tableLayoutPanel3.Location = new Point(308, 40);
        this.tableLayoutPanel3.MaximumSize = new Size(64, 106);
        this.tableLayoutPanel3.MinimumSize = new Size(64, 102);
        this.tableLayoutPanel3.Name = "tableLayoutPanel3";
        this.tableLayoutPanel3.RowCount = 3;
        this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
        this.tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
        this.tableLayoutPanel3.Size = new Size(64, 106);
        this.tableLayoutPanel3.TabIndex = 9;
        // 
        // colorShowBox
        // 
        this.colorShowBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.colorShowBox.BorderStyle = BorderStyle.FixedSingle;
        this.colorShowBox.Color = Color.DarkRed;
        this.colorShowBox.Location = new Point(3, 24);
        this.colorShowBox.LowerColor = Color.Maroon;
        this.colorShowBox.MaximumSize = new Size(58, 58);
        this.colorShowBox.MinimumSize = new Size(58, 58);
        this.colorShowBox.Name = "colorShowBox";
        this.colorShowBox.Padding = new Padding(0, 3, 0, 3);
        this.colorShowBox.Size = new Size(58, 58);
        this.colorShowBox.TabIndex = 2;
        this.colorShowBox.UpperColor = Color.DarkRed;
        this.colorShowBox.UpperClick += this.ColorShowBox_UpperClick;
        // 
        // label2
        // 
        this.label2.Anchor = AnchorStyles.None;
        this.label2.AutoSize = true;
        this.label2.ImeMode = ImeMode.NoControl;
        this.label2.Location = new Point(8, 88);
        this.label2.Margin = new Padding(3);
        this.label2.Name = "label2";
        this.label2.Size = new Size(47, 15);
        this.label2.TabIndex = 4;
        this.label2.Text = "Current";
        this.label2.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // label1
        // 
        this.label1.Anchor = AnchorStyles.None;
        this.label1.AutoSize = true;
        this.label1.ImeMode = ImeMode.NoControl;
        this.label1.Location = new Point(19, 3);
        this.label1.Margin = new Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new Size(26, 15);
        this.label1.TabIndex = 3;
        this.label1.Text = "Old";
        this.label1.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // colorPanel
        // 
        this.colorPanel.Anchor = AnchorStyles.None;
        this.colorPanel.BottomLeftColor = Color.Black;
        this.colorPanel.BottomRightColor = Color.Black;
        this.colorPanel.Location = new Point(28, 6);
        this.colorPanel.MaximumSize = new Size(198, 175);
        this.colorPanel.MinimumSize = new Size(198, 175);
        this.colorPanel.Name = "colorPanel";
        this.colorPanel.Size = new Size(198, 175);
        this.colorPanel.TabIndex = 0;
        this.colorPanel.TopLeftColor = Color.White;
        this.colorPanel.TopRightColor = Color.Red;
        this.colorPanel.ValuePercentual = (PointF)resources.GetObject("colorPanel.ValuePercentual");
        this.colorPanel.PercentualValueChanged += this.ColorPanel_PercentualValueChanged;
        // 
        // colorSlider
        // 
        this.colorSlider.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
        this.colorSlider.Location = new Point(232, 3);
        this.colorSlider.MaximumSize = new Size(32, 181);
        this.colorSlider.MinimumSize = new Size(32, 181);
        this.colorSlider.Name = "colorSlider";
        this.colorSlider.ShowInnerPicker = false;
        this.colorSlider.Size = new Size(32, 181);
        this.colorSlider.TabIndex = 1;
        this.colorSlider.PercentualValueChanged += this.ColorSlider_PercentualValueChanged;
        // 
        // alphaSlider
        // 
        this.alphaSlider.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
        this.alphaSlider.Location = new Point(270, 3);
        this.alphaSlider.Maximum = Color.White;
        this.alphaSlider.MaximumSize = new Size(32, 181);
        this.alphaSlider.Minimum = Color.Transparent;
        this.alphaSlider.MinimumSize = new Size(32, 181);
        this.alphaSlider.Name = "alphaSlider";
        this.alphaSlider.ShowInnerPicker = false;
        this.alphaSlider.Size = new Size(32, 181);
        this.alphaSlider.TabIndex = 2;
        this.alphaSlider.PercentualValueChanged += this.AlphaSlider_PercentualValueChanged;
        // 
        // TableBottom
        // 
        this.TableBottom.ColumnCount = 8;
        this.TableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        this.TableBottom.ColumnStyles.Add(new ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new ColumnStyle());
        this.TableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
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
        this.TableBottom.Dock = DockStyle.Fill;
        this.TableBottom.Location = new Point(0, 187);
        this.TableBottom.Name = "TableBottom";
        this.TableBottom.Padding = new Padding(3, 0, 0, 0);
        this.TableBottom.RowCount = 4;
        this.TableBottom.RowStyles.Add(new RowStyle());
        this.TableBottom.RowStyles.Add(new RowStyle());
        this.TableBottom.RowStyles.Add(new RowStyle());
        this.TableBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        this.TableBottom.Size = new Size(400, 88);
        this.TableBottom.TabIndex = 0;
        // 
        // textBoxHex
        // 
        this.textBoxHex.Anchor = AnchorStyles.None;
        this.textBoxHex.Location = new Point(308, 61);
        this.textBoxHex.Name = "textBoxHex";
        this.textBoxHex.Size = new Size(65, 23);
        this.textBoxHex.TabIndex = 15;
        this.textBoxHex.TextAlign = HorizontalAlignment.Right;
        this.textBoxHex.TextChanged += this.TextBoxHex_TextChanged;
        // 
        // numAlpha
        // 
        this.numAlpha.Anchor = AnchorStyles.None;
        this.numAlpha.Location = new Point(308, 3);
        this.numAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        this.numAlpha.Name = "numAlpha";
        this.numAlpha.Size = new Size(65, 23);
        this.numAlpha.TabIndex = 13;
        this.numAlpha.TextAlign = HorizontalAlignment.Right;
        this.numAlpha.ValueChanged += this.NumAlpha_ValueChanged;
        // 
        // ColorPickerPanel
        // 
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.Controls.Add(this.TableBottom);
        this.Controls.Add(this.TableTop);
        this.MaximumSize = new Size(400, 275);
        this.MinimumSize = new Size(400, 275);
        this.Name = "ColorPickerPanel";
        this.Size = new Size(400, 275);
        ((System.ComponentModel.ISupportInitialize)this.numHue).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numSaturation).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numValue).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numRed).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numGreen).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numBlue).EndInit();
        this.TableTop.ResumeLayout(false);
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
