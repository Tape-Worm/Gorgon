namespace Fetze.WinFormsColor;

    partial class ColorPickerDialog
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
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorPickerDialog));
        radioHue = new System.Windows.Forms.RadioButton();
        radioSaturation = new System.Windows.Forms.RadioButton();
        radioValue = new System.Windows.Forms.RadioButton();
        radioRed = new System.Windows.Forms.RadioButton();
        radioBlue = new System.Windows.Forms.RadioButton();
        radioGreen = new System.Windows.Forms.RadioButton();
        numHue = new System.Windows.Forms.NumericUpDown();
        numSaturation = new System.Windows.Forms.NumericUpDown();
        numValue = new System.Windows.Forms.NumericUpDown();
        numRed = new System.Windows.Forms.NumericUpDown();
        numGreen = new System.Windows.Forms.NumericUpDown();
        numBlue = new System.Windows.Forms.NumericUpDown();
        textBoxHex = new System.Windows.Forms.TextBox();
        labelHex = new System.Windows.Forms.Label();
        labelOld = new System.Windows.Forms.Label();
        labelNew = new System.Windows.Forms.Label();
        buttonCancel = new System.Windows.Forms.Button();
        buttonOk = new System.Windows.Forms.Button();
        numAlpha = new System.Windows.Forms.NumericUpDown();
        labelAlpha = new System.Windows.Forms.Label();
        labelHueUnit = new System.Windows.Forms.Label();
        labelSaturationUnit = new System.Windows.Forms.Label();
        labelValueUnit = new System.Windows.Forms.Label();
        alphaSlider = new ColorSlider();
        colorShowBox = new ColorShowBox();
        colorSlider = new ColorSlider();
        colorPanel = new ColorPanel();
        ((System.ComponentModel.ISupportInitialize)numHue).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numSaturation).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numValue).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRed).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numGreen).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numBlue).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numAlpha).BeginInit();
        SuspendLayout();
        // 
        // radioHue
        // 
        radioHue.AutoSize = true;
        radioHue.Checked = true;
        radioHue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        radioHue.Location = new System.Drawing.Point(424, 95);
        radioHue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        radioHue.Name = "radioHue";
        radioHue.Size = new System.Drawing.Size(34, 19);
        radioHue.TabIndex = 3;
        radioHue.TabStop = true;
        radioHue.Text = "H";
        radioHue.UseVisualStyleBackColor = true;
        radioHue.CheckedChanged += radioHue_CheckedChanged;
        // 
        // radioSaturation
        // 
        radioSaturation.AutoSize = true;
        radioSaturation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        radioSaturation.Location = new System.Drawing.Point(424, 121);
        radioSaturation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        radioSaturation.Name = "radioSaturation";
        radioSaturation.Size = new System.Drawing.Size(31, 19);
        radioSaturation.TabIndex = 4;
        radioSaturation.Text = "S";
        radioSaturation.UseVisualStyleBackColor = true;
        radioSaturation.CheckedChanged += radioSaturation_CheckedChanged;
        // 
        // radioValue
        // 
        radioValue.AutoSize = true;
        radioValue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        radioValue.Location = new System.Drawing.Point(424, 148);
        radioValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        radioValue.Name = "radioValue";
        radioValue.Size = new System.Drawing.Size(32, 19);
        radioValue.TabIndex = 5;
        radioValue.Text = "V";
        radioValue.UseVisualStyleBackColor = true;
        radioValue.CheckedChanged += radioValue_CheckedChanged;
        // 
        // radioRed
        // 
        radioRed.AutoSize = true;
        radioRed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        radioRed.Location = new System.Drawing.Point(424, 183);
        radioRed.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        radioRed.Name = "radioRed";
        radioRed.Size = new System.Drawing.Size(32, 19);
        radioRed.TabIndex = 6;
        radioRed.Text = "R";
        radioRed.UseVisualStyleBackColor = true;
        radioRed.CheckedChanged += radioRed_CheckedChanged;
        // 
        // radioBlue
        // 
        radioBlue.AutoSize = true;
        radioBlue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        radioBlue.Location = new System.Drawing.Point(424, 237);
        radioBlue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        radioBlue.Name = "radioBlue";
        radioBlue.Size = new System.Drawing.Size(32, 19);
        radioBlue.TabIndex = 7;
        radioBlue.Text = "B";
        radioBlue.UseVisualStyleBackColor = true;
        radioBlue.CheckedChanged += radioBlue_CheckedChanged;
        // 
        // radioGreen
        // 
        radioGreen.AutoSize = true;
        radioGreen.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        radioGreen.Location = new System.Drawing.Point(424, 210);
        radioGreen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        radioGreen.Name = "radioGreen";
        radioGreen.Size = new System.Drawing.Size(33, 19);
        radioGreen.TabIndex = 8;
        radioGreen.Text = "G";
        radioGreen.UseVisualStyleBackColor = true;
        radioGreen.CheckedChanged += radioGreen_CheckedChanged;
        // 
        // numHue
        // 
        numHue.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        numHue.Location = new System.Drawing.Point(469, 95);
        numHue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        numHue.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
        numHue.Name = "numHue";
        numHue.Size = new System.Drawing.Size(63, 23);
        numHue.TabIndex = 9;
        numHue.ValueChanged += numHue_ValueChanged;
        // 
        // numSaturation
        // 
        numSaturation.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        numSaturation.Location = new System.Drawing.Point(469, 121);
        numSaturation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        numSaturation.Name = "numSaturation";
        numSaturation.Size = new System.Drawing.Size(63, 23);
        numSaturation.TabIndex = 10;
        numSaturation.ValueChanged += numSaturation_ValueChanged;
        // 
        // numValue
        // 
        numValue.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        numValue.Location = new System.Drawing.Point(469, 148);
        numValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        numValue.Name = "numValue";
        numValue.Size = new System.Drawing.Size(63, 23);
        numValue.TabIndex = 11;
        numValue.ValueChanged += numValue_ValueChanged;
        // 
        // numRed
        // 
        numRed.Location = new System.Drawing.Point(469, 183);
        numRed.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        numRed.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        numRed.Name = "numRed";
        numRed.Size = new System.Drawing.Size(63, 23);
        numRed.TabIndex = 12;
        numRed.ValueChanged += numRed_ValueChanged;
        // 
        // numGreen
        // 
        numGreen.Location = new System.Drawing.Point(469, 210);
        numGreen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        numGreen.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        numGreen.Name = "numGreen";
        numGreen.Size = new System.Drawing.Size(63, 23);
        numGreen.TabIndex = 13;
        numGreen.ValueChanged += numGreen_ValueChanged;
        // 
        // numBlue
        // 
        numBlue.Location = new System.Drawing.Point(469, 237);
        numBlue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        numBlue.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        numBlue.Name = "numBlue";
        numBlue.Size = new System.Drawing.Size(63, 23);
        numBlue.TabIndex = 14;
        numBlue.ValueChanged += numBlue_ValueChanged;
        // 
        // textBoxHex
        // 
        textBoxHex.Location = new System.Drawing.Point(37, 322);
        textBoxHex.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        textBoxHex.Name = "textBoxHex";
        textBoxHex.Size = new System.Drawing.Size(87, 23);
        textBoxHex.TabIndex = 16;
        textBoxHex.TextChanged += textBoxHex_TextChanged;
        // 
        // labelHex
        // 
        labelHex.AutoSize = true;
        labelHex.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        labelHex.Location = new System.Drawing.Point(14, 325);
        labelHex.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        labelHex.Name = "labelHex";
        labelHex.Size = new System.Drawing.Size(14, 15);
        labelHex.TabIndex = 17;
        labelHex.Text = "#";
        // 
        // labelOld
        // 
        labelOld.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        labelOld.Location = new System.Drawing.Point(493, 14);
        labelOld.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        labelOld.Name = "labelOld";
        labelOld.Size = new System.Drawing.Size(38, 30);
        labelOld.TabIndex = 18;
        labelOld.Text = "Old";
        labelOld.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // labelNew
        // 
        labelNew.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        labelNew.Location = new System.Drawing.Point(493, 42);
        labelNew.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        labelNew.Name = "labelNew";
        labelNew.Size = new System.Drawing.Size(38, 30);
        labelNew.TabIndex = 19;
        labelNew.Text = "New";
        labelNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // buttonCancel
        // 
        buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        buttonCancel.Location = new System.Drawing.Point(442, 320);
        buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        buttonCancel.Name = "buttonCancel";
        buttonCancel.Size = new System.Drawing.Size(108, 27);
        buttonCancel.TabIndex = 20;
        buttonCancel.Text = "Cancel";
        buttonCancel.UseVisualStyleBackColor = true;
        // 
        // buttonOk
        // 
        buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        buttonOk.Location = new System.Drawing.Point(327, 320);
        buttonOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        buttonOk.Name = "buttonOk";
        buttonOk.Size = new System.Drawing.Size(108, 27);
        buttonOk.TabIndex = 21;
        buttonOk.Text = "Ok";
        buttonOk.UseVisualStyleBackColor = true;
        buttonOk.Click += buttonOk_Click;
        // 
        // numAlpha
        // 
        numAlpha.Location = new System.Drawing.Point(469, 272);
        numAlpha.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        numAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        numAlpha.Name = "numAlpha";
        numAlpha.Size = new System.Drawing.Size(63, 23);
        numAlpha.TabIndex = 22;
        numAlpha.ValueChanged += numAlpha_ValueChanged;
        // 
        // labelAlpha
        // 
        labelAlpha.AutoSize = true;
        labelAlpha.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        labelAlpha.Location = new System.Drawing.Point(442, 275);
        labelAlpha.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        labelAlpha.Name = "labelAlpha";
        labelAlpha.Size = new System.Drawing.Size(15, 15);
        labelAlpha.TabIndex = 23;
        labelAlpha.Text = "A";
        // 
        // labelHueUnit
        // 
        labelHueUnit.AutoSize = true;
        labelHueUnit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        labelHueUnit.Location = new System.Drawing.Point(538, 97);
        labelHueUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        labelHueUnit.Name = "labelHueUnit";
        labelHueUnit.Size = new System.Drawing.Size(12, 15);
        labelHueUnit.TabIndex = 24;
        labelHueUnit.Text = "°";
        // 
        // labelSaturationUnit
        // 
        labelSaturationUnit.AutoSize = true;
        labelSaturationUnit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        labelSaturationUnit.Location = new System.Drawing.Point(539, 123);
        labelSaturationUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        labelSaturationUnit.Name = "labelSaturationUnit";
        labelSaturationUnit.Size = new System.Drawing.Size(17, 15);
        labelSaturationUnit.TabIndex = 25;
        labelSaturationUnit.Text = "%";
        // 
        // labelValueUnit
        // 
        labelValueUnit.AutoSize = true;
        labelValueUnit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        labelValueUnit.Location = new System.Drawing.Point(538, 150);
        labelValueUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        labelValueUnit.Name = "labelValueUnit";
        labelValueUnit.Size = new System.Drawing.Size(17, 15);
        labelValueUnit.TabIndex = 26;
        labelValueUnit.Text = "%";
        // 
        // alphaSlider
        // 
        alphaSlider.Location = new System.Drawing.Point(364, 8);
        alphaSlider.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        alphaSlider.Maximum = System.Drawing.Color.White;
        alphaSlider.Minimum = System.Drawing.Color.Transparent;
        alphaSlider.Name = "alphaSlider";
        alphaSlider.ShowInnerPicker = false;
        alphaSlider.Size = new System.Drawing.Size(37, 307);
        alphaSlider.TabIndex = 15;
        alphaSlider.PercentualValueChanged += alphaSlider_PercentualValueChanged;
        // 
        // colorShowBox
        // 
        colorShowBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        colorShowBox.Color = System.Drawing.Color.DarkRed;
        colorShowBox.Location = new System.Drawing.Point(424, 14);
        colorShowBox.LowerColor = System.Drawing.Color.Maroon;
        colorShowBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        colorShowBox.Name = "colorShowBox";
        colorShowBox.Size = new System.Drawing.Size(63, 57);
        colorShowBox.TabIndex = 2;
        colorShowBox.UpperColor = System.Drawing.Color.DarkRed;
        colorShowBox.UpperClick += colorShowBox_UpperClick;
        // 
        // colorSlider
        // 
        colorSlider.Location = new System.Drawing.Point(320, 8);
        colorSlider.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        colorSlider.Name = "colorSlider";
        colorSlider.ShowInnerPicker = false;
        colorSlider.Size = new System.Drawing.Size(37, 307);
        colorSlider.TabIndex = 1;
        colorSlider.PercentualValueChanged += colorSlider_PercentualValueChanged;
        // 
        // colorPanel
        // 
        colorPanel.BottomLeftColor = System.Drawing.Color.Black;
        colorPanel.BottomRightColor = System.Drawing.Color.Black;
        colorPanel.Location = new System.Drawing.Point(14, 14);
        colorPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        colorPanel.Name = "colorPanel";
        colorPanel.Size = new System.Drawing.Size(299, 295);
        colorPanel.TabIndex = 0;
        colorPanel.TopLeftColor = System.Drawing.Color.White;
        colorPanel.TopRightColor = System.Drawing.Color.Red;
        colorPanel.ValuePercentual = (System.Drawing.PointF)resources.GetObject("colorPanel.ValuePercentual");
        colorPanel.PercentualValueChanged += colorPanel_PercentualValueChanged;
        // 
        // ColorPickerDialog
        // 
        AcceptButton = buttonOk;
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        CancelButton = buttonCancel;
        ClientSize = new System.Drawing.Size(565, 360);
        Controls.Add(labelValueUnit);
        Controls.Add(labelSaturationUnit);
        Controls.Add(labelHueUnit);
        Controls.Add(labelAlpha);
        Controls.Add(numAlpha);
        Controls.Add(buttonOk);
        Controls.Add(buttonCancel);
        Controls.Add(labelNew);
        Controls.Add(labelOld);
        Controls.Add(labelHex);
        Controls.Add(textBoxHex);
        Controls.Add(alphaSlider);
        Controls.Add(numBlue);
        Controls.Add(numGreen);
        Controls.Add(numRed);
        Controls.Add(numValue);
        Controls.Add(numSaturation);
        Controls.Add(numHue);
        Controls.Add(radioGreen);
        Controls.Add(radioBlue);
        Controls.Add(radioRed);
        Controls.Add(radioValue);
        Controls.Add(radioSaturation);
        Controls.Add(radioHue);
        Controls.Add(colorShowBox);
        Controls.Add(colorSlider);
        Controls.Add(colorPanel);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ColorPickerDialog";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Choose a new color";
        FormClosed += ColorPickerDialog_FormClosed;
        ((System.ComponentModel.ISupportInitialize)numHue).EndInit();
        ((System.ComponentModel.ISupportInitialize)numSaturation).EndInit();
        ((System.ComponentModel.ISupportInitialize)numValue).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRed).EndInit();
        ((System.ComponentModel.ISupportInitialize)numGreen).EndInit();
        ((System.ComponentModel.ISupportInitialize)numBlue).EndInit();
        ((System.ComponentModel.ISupportInitialize)numAlpha).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private ColorPanel colorPanel;
        private ColorSlider colorSlider;
        private ColorShowBox colorShowBox;
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
        private ColorSlider alphaSlider;
        private System.Windows.Forms.TextBox textBoxHex;
        private System.Windows.Forms.Label labelHex;
        private System.Windows.Forms.Label labelOld;
        private System.Windows.Forms.Label labelNew;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.NumericUpDown numAlpha;
        private System.Windows.Forms.Label labelAlpha;
        private System.Windows.Forms.Label labelHueUnit;
        private System.Windows.Forms.Label labelSaturationUnit;
        private System.Windows.Forms.Label labelValueUnit;
    }
