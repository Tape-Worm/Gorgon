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
            this.textBoxHex = new System.Windows.Forms.TextBox();
            this.labelHex = new System.Windows.Forms.Label();
            this.numAlpha = new System.Windows.Forms.NumericUpDown();
            this.labelAlpha = new System.Windows.Forms.Label();
            this.alphaSlider = new Fetze.WinFormsColor.ColorSlider();
            this.colorSlider = new Fetze.WinFormsColor.ColorSlider();
            this.colorPanel = new Fetze.WinFormsColor.ColorPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.colorShowBox = new Fetze.WinFormsColor.ColorShowBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.numHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAlpha)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioHue
            // 
            resources.ApplyResources(this.radioHue, "radioHue");
            this.radioHue.Checked = true;
            this.radioHue.Name = "radioHue";
            this.radioHue.TabStop = true;
            this.radioHue.UseVisualStyleBackColor = true;
            this.radioHue.CheckedChanged += new System.EventHandler(this.radioHue_CheckedChanged);
            // 
            // radioSaturation
            // 
            resources.ApplyResources(this.radioSaturation, "radioSaturation");
            this.radioSaturation.Name = "radioSaturation";
            this.radioSaturation.UseVisualStyleBackColor = true;
            this.radioSaturation.CheckedChanged += new System.EventHandler(this.radioSaturation_CheckedChanged);
            // 
            // radioValue
            // 
            resources.ApplyResources(this.radioValue, "radioValue");
            this.radioValue.Name = "radioValue";
            this.radioValue.UseVisualStyleBackColor = true;
            this.radioValue.CheckedChanged += new System.EventHandler(this.radioValue_CheckedChanged);
            // 
            // radioRed
            // 
            resources.ApplyResources(this.radioRed, "radioRed");
            this.radioRed.Name = "radioRed";
            this.radioRed.UseVisualStyleBackColor = true;
            this.radioRed.CheckedChanged += new System.EventHandler(this.radioRed_CheckedChanged);
            // 
            // radioBlue
            // 
            resources.ApplyResources(this.radioBlue, "radioBlue");
            this.radioBlue.Name = "radioBlue";
            this.radioBlue.UseVisualStyleBackColor = true;
            this.radioBlue.CheckedChanged += new System.EventHandler(this.radioBlue_CheckedChanged);
            // 
            // radioGreen
            // 
            resources.ApplyResources(this.radioGreen, "radioGreen");
            this.radioGreen.Name = "radioGreen";
            this.radioGreen.UseVisualStyleBackColor = true;
            this.radioGreen.CheckedChanged += new System.EventHandler(this.radioGreen_CheckedChanged);
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
            this.numHue.ValueChanged += new System.EventHandler(this.numHue_ValueChanged);
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
            this.numSaturation.ValueChanged += new System.EventHandler(this.numSaturation_ValueChanged);
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
            this.numValue.ValueChanged += new System.EventHandler(this.numValue_ValueChanged);
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
            this.numRed.ValueChanged += new System.EventHandler(this.numRed_ValueChanged);
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
            this.numGreen.ValueChanged += new System.EventHandler(this.numGreen_ValueChanged);
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
            this.numBlue.ValueChanged += new System.EventHandler(this.numBlue_ValueChanged);
            // 
            // textBoxHex
            // 
            resources.ApplyResources(this.textBoxHex, "textBoxHex");
            this.textBoxHex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxHex.Name = "textBoxHex";
            this.textBoxHex.TextChanged += new System.EventHandler(this.textBoxHex_TextChanged);
            // 
            // labelHex
            // 
            resources.ApplyResources(this.labelHex, "labelHex");
            this.labelHex.Name = "labelHex";
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
            this.numAlpha.ValueChanged += new System.EventHandler(this.numAlpha_ValueChanged);
            // 
            // labelAlpha
            // 
            resources.ApplyResources(this.labelAlpha, "labelAlpha");
            this.labelAlpha.Name = "labelAlpha";
            // 
            // alphaSlider
            // 
            resources.ApplyResources(this.alphaSlider, "alphaSlider");
            this.alphaSlider.Maximum = System.Drawing.Color.White;
            this.alphaSlider.Minimum = System.Drawing.Color.Transparent;
            this.alphaSlider.Name = "alphaSlider";
            this.alphaSlider.PercentualValueChanged += new System.EventHandler(this.alphaSlider_PercentualValueChanged);
            // 
            // colorSlider
            // 
            resources.ApplyResources(this.colorSlider, "colorSlider");
            this.colorSlider.Name = "colorSlider";
            this.colorSlider.PercentualValueChanged += new System.EventHandler(this.colorSlider_PercentualValueChanged);
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
            this.colorPanel.PercentualValueChanged += new System.EventHandler(this.colorPanel_PercentualValueChanged);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.colorPanel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.colorSlider, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.alphaSlider, 3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
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
            // colorShowBox
            // 
            resources.ApplyResources(this.colorShowBox, "colorShowBox");
            this.colorShowBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colorShowBox.Color = System.Drawing.Color.DarkRed;
            this.colorShowBox.LowerColor = System.Drawing.Color.Maroon;
            this.colorShowBox.Name = "colorShowBox";
            this.colorShowBox.UpperColor = System.Drawing.Color.DarkRed;
            this.colorShowBox.UpperClick += new System.EventHandler(this.colorShowBox_UpperClick);
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.textBoxHex, 6, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelHex, 5, 2);
            this.tableLayoutPanel2.Controls.Add(this.numAlpha, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.numGreen, 4, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelAlpha, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.radioBlue, 3, 2);
            this.tableLayoutPanel2.Controls.Add(this.numBlue, 4, 2);
            this.tableLayoutPanel2.Controls.Add(this.radioGreen, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.numRed, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.radioRed, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.numValue, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.radioValue, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.numSaturation, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.radioSaturation, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.radioHue, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.numHue, 2, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.colorShowBox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // ColorPickerPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ColorPickerPanel";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.numHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAlpha)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
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
		private System.Windows.Forms.TextBox textBoxHex;
		private System.Windows.Forms.Label labelHex;
		private System.Windows.Forms.NumericUpDown numAlpha;
        private System.Windows.Forms.Label labelAlpha;
        private ColorSlider alphaSlider;
        private ColorSlider colorSlider;
        private ColorPanel colorPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ColorShowBox colorShowBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}