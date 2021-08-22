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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.numAlpha = new System.Windows.Forms.NumericUpDown();
            this.textBoxHex = new System.Windows.Forms.TextBox();
            this.colorShowBox = new Fetze.WinFormsColor.ColorShowBox();
            this.colorPanel = new Fetze.WinFormsColor.ColorPanel();
            this.colorSlider = new Fetze.WinFormsColor.ColorSlider();
            this.alphaSlider = new Fetze.WinFormsColor.ColorSlider();
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
            resources.ApplyResources(this.radioHue, "radioHue");
            this.radioHue.Checked = true;
            this.radioHue.Name = "radioHue";
            this.radioHue.TabStop = true;
            this.radioHue.UseVisualStyleBackColor = true;
            this.radioHue.CheckedChanged += new System.EventHandler(this.RadioHue_CheckedChanged);
            // 
            // radioSaturation
            // 
            resources.ApplyResources(this.radioSaturation, "radioSaturation");
            this.radioSaturation.Name = "radioSaturation";
            this.radioSaturation.UseVisualStyleBackColor = true;
            this.radioSaturation.CheckedChanged += new System.EventHandler(this.RadioSaturation_CheckedChanged);
            // 
            // radioValue
            // 
            resources.ApplyResources(this.radioValue, "radioValue");
            this.radioValue.Name = "radioValue";
            this.radioValue.UseVisualStyleBackColor = true;
            this.radioValue.CheckedChanged += new System.EventHandler(this.RadioValue_CheckedChanged);
            // 
            // radioRed
            // 
            resources.ApplyResources(this.radioRed, "radioRed");
            this.radioRed.Name = "radioRed";
            this.radioRed.UseVisualStyleBackColor = true;
            this.radioRed.CheckedChanged += new System.EventHandler(this.RadioRed_CheckedChanged);
            // 
            // radioBlue
            // 
            resources.ApplyResources(this.radioBlue, "radioBlue");
            this.radioBlue.Name = "radioBlue";
            this.radioBlue.UseVisualStyleBackColor = true;
            this.radioBlue.CheckedChanged += new System.EventHandler(this.RadioBlue_CheckedChanged);
            // 
            // radioGreen
            // 
            resources.ApplyResources(this.radioGreen, "radioGreen");
            this.radioGreen.Name = "radioGreen";
            this.radioGreen.UseVisualStyleBackColor = true;
            this.radioGreen.CheckedChanged += new System.EventHandler(this.RadioGreen_CheckedChanged);
            // 
            // numHue
            // 
            resources.ApplyResources(this.numHue, "numHue");
            this.numHue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numHue.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numHue.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numHue.Name = "numHue";
            this.numHue.ValueChanged += new System.EventHandler(this.NumHue_ValueChanged);
            // 
            // numSaturation
            // 
            resources.ApplyResources(this.numSaturation, "numSaturation");
            this.numSaturation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numSaturation.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numSaturation.Name = "numSaturation";
            this.numSaturation.ValueChanged += new System.EventHandler(this.NumSaturation_ValueChanged);
            // 
            // numValue
            // 
            resources.ApplyResources(this.numValue, "numValue");
            this.numValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numValue.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numValue.Name = "numValue";
            this.numValue.ValueChanged += new System.EventHandler(this.NumValue_ValueChanged);
            // 
            // numRed
            // 
            resources.ApplyResources(this.numRed, "numRed");
            this.numRed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numRed.Name = "numRed";
            this.numRed.ValueChanged += new System.EventHandler(this.NumRed_ValueChanged);
            // 
            // numGreen
            // 
            resources.ApplyResources(this.numGreen, "numGreen");
            this.numGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numGreen.Name = "numGreen";
            this.numGreen.ValueChanged += new System.EventHandler(this.NumGreen_ValueChanged);
            // 
            // numBlue
            // 
            resources.ApplyResources(this.numBlue, "numBlue");
            this.numBlue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBlue.Name = "numBlue";
            this.numBlue.ValueChanged += new System.EventHandler(this.NumBlue_ValueChanged);
            // 
            // labelHex
            // 
            resources.ApplyResources(this.labelHex, "labelHex");
            this.labelHex.Name = "labelHex";
            // 
            // labelAlpha
            // 
            resources.ApplyResources(this.labelAlpha, "labelAlpha");
            this.labelAlpha.Name = "labelAlpha";
            // 
            // TableTop
            // 
            resources.ApplyResources(this.TableTop, "TableTop");
            this.TableTop.Controls.Add(this.tableLayoutPanel3, 4, 0);
            this.TableTop.Controls.Add(this.colorPanel, 1, 0);
            this.TableTop.Controls.Add(this.colorSlider, 2, 0);
            this.TableTop.Controls.Add(this.alphaSlider, 3, 0);
            this.TableTop.Name = "TableTop";
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.colorShowBox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // TableBottom
            // 
            resources.ApplyResources(this.TableBottom, "TableBottom");
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
            this.TableBottom.Name = "TableBottom";
            // 
            // numAlpha
            // 
            resources.ApplyResources(this.numAlpha, "numAlpha");
            this.numAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numAlpha.Name = "numAlpha";
            this.numAlpha.ValueChanged += new System.EventHandler(this.NumAlpha_ValueChanged);
            // 
            // textBoxHex
            // 
            resources.ApplyResources(this.textBoxHex, "textBoxHex");
            this.textBoxHex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxHex.Name = "textBoxHex";
            this.textBoxHex.TextChanged += new System.EventHandler(this.TextBoxHex_TextChanged);
            // 
            // colorShowBox
            // 
            resources.ApplyResources(this.colorShowBox, "colorShowBox");
            this.colorShowBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colorShowBox.Color = System.Drawing.Color.DarkRed;
            this.colorShowBox.LowerColor = System.Drawing.Color.Maroon;
            this.colorShowBox.Name = "colorShowBox";
            this.colorShowBox.UpperColor = System.Drawing.Color.DarkRed;
            this.colorShowBox.UpperClick += new System.EventHandler(this.ColorShowBox_UpperClick);
            // 
            // colorPanel
            // 
            resources.ApplyResources(this.colorPanel, "colorPanel");
            this.colorPanel.BottomLeftColor = System.Drawing.Color.Black;
            this.colorPanel.BottomRightColor = System.Drawing.Color.Black;
            this.colorPanel.Name = "colorPanel";
            this.colorPanel.TopLeftColor = System.Drawing.Color.White;
            this.colorPanel.TopRightColor = System.Drawing.Color.Red;
            this.colorPanel.ValuePercentual = ((System.Drawing.PointF)(resources.GetObject("colorPanel.ValuePercentual")));
            this.colorPanel.PercentualValueChanged += new System.EventHandler(this.ColorPanel_PercentualValueChanged);
            // 
            // colorSlider
            // 
            resources.ApplyResources(this.colorSlider, "colorSlider");
            this.colorSlider.Name = "colorSlider";
            this.colorSlider.ShowInnerPicker = false;
            this.colorSlider.PercentualValueChanged += new System.EventHandler(this.ColorSlider_PercentualValueChanged);
            // 
            // alphaSlider
            // 
            resources.ApplyResources(this.alphaSlider, "alphaSlider");
            this.alphaSlider.Maximum = System.Drawing.Color.White;
            this.alphaSlider.Minimum = System.Drawing.Color.Transparent;
            this.alphaSlider.Name = "alphaSlider";
            this.alphaSlider.ShowInnerPicker = false;
            this.alphaSlider.PercentualValueChanged += new System.EventHandler(this.AlphaSlider_PercentualValueChanged);
            // 
            // ColorPickerPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.TableBottom);
            this.Controls.Add(this.TableTop);
            this.Name = "ColorPickerPanel";
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