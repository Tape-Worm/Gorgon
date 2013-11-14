namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    partial class formBrushEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBrushEditor));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tabBrushEditor = new KRBTabControl.KRBTabControl();
            this.pageSolid = new KRBTabControl.TabPageEx();
            this.label6 = new System.Windows.Forms.Label();
            this.numericAlpha = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericBlue = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericGreen = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericRed = new System.Windows.Forms.NumericUpDown();
            this.sliderAlpha = new Fetze.WinFormsColor.ColorSlider();
            this.sliderColor = new Fetze.WinFormsColor.ColorSlider();
            this.panelColor = new Fetze.WinFormsColor.ColorPanel();
            this.boxCurrentColor = new Fetze.WinFormsColor.ColorShowBox();
            this.pageTexture = new KRBTabControl.TabPageEx();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBrushType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabBrushEditor.SuspendLayout();
            this.pageSolid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericAlpha)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRed)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
            this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonCancel.Location = new System.Drawing.Point(511, 9);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 28);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonCancel.UseVisualStyleBackColor = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonOK.ForeColor = System.Drawing.Color.White;
            this.buttonOK.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
            this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonOK.Location = new System.Drawing.Point(418, 9);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 28);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonOK.UseVisualStyleBackColor = false;
            // 
            // tabBrushEditor
            // 
            this.tabBrushEditor.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabBrushEditor.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
            this.tabBrushEditor.AllowDrop = true;
            this.tabBrushEditor.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabBrushEditor.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
            this.tabBrushEditor.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabBrushEditor.Controls.Add(this.pageSolid);
            this.tabBrushEditor.Controls.Add(this.pageTexture);
            this.tabBrushEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabBrushEditor.HeaderVisibility = true;
            this.tabBrushEditor.IsCaptionVisible = false;
            this.tabBrushEditor.IsDocumentTabStyle = true;
            this.tabBrushEditor.IsDrawHeader = false;
            this.tabBrushEditor.IsUserInteraction = false;
            this.tabBrushEditor.ItemSize = new System.Drawing.Size(0, 28);
            this.tabBrushEditor.Location = new System.Drawing.Point(1, 56);
            this.tabBrushEditor.Multiline = true;
            this.tabBrushEditor.Name = "tabBrushEditor";
            this.tabBrushEditor.SelectedIndex = 0;
            this.tabBrushEditor.Size = new System.Drawing.Size(610, 338);
            this.tabBrushEditor.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabBrushEditor.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabBrushEditor.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabBrushEditor.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.White;
            this.tabBrushEditor.TabGradient.TabPageTextColor = System.Drawing.Color.White;
            this.tabBrushEditor.TabHOffset = -1;
            this.tabBrushEditor.TabIndex = 0;
            this.tabBrushEditor.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
            this.tabBrushEditor.SelectedIndexChanged += new System.EventHandler(this.tabBrushEditor_SelectedIndexChanged);
            // 
            // pageSolid
            // 
            this.pageSolid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.pageSolid.Controls.Add(this.label6);
            this.pageSolid.Controls.Add(this.numericAlpha);
            this.pageSolid.Controls.Add(this.label5);
            this.pageSolid.Controls.Add(this.numericBlue);
            this.pageSolid.Controls.Add(this.label4);
            this.pageSolid.Controls.Add(this.numericGreen);
            this.pageSolid.Controls.Add(this.label3);
            this.pageSolid.Controls.Add(this.numericRed);
            this.pageSolid.Controls.Add(this.sliderAlpha);
            this.pageSolid.Controls.Add(this.sliderColor);
            this.pageSolid.Controls.Add(this.panelColor);
            this.pageSolid.Controls.Add(this.boxCurrentColor);
            this.pageSolid.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageSolid.ForeColor = System.Drawing.Color.White;
            this.pageSolid.IsClosable = false;
            this.pageSolid.Location = new System.Drawing.Point(1, 1);
            this.pageSolid.Name = "pageSolid";
            this.pageSolid.Size = new System.Drawing.Size(608, 336);
            this.pageSolid.TabIndex = 1;
            this.pageSolid.Text = "Solid";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(429, 228);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 14;
            this.label6.Text = "Alpha:";
            // 
            // numericAlpha
            // 
            this.numericAlpha.Location = new System.Drawing.Point(476, 226);
            this.numericAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericAlpha.Name = "numericAlpha";
            this.numericAlpha.Size = new System.Drawing.Size(66, 23);
            this.numericAlpha.TabIndex = 13;
            this.numericAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAlpha.ValueChanged += new System.EventHandler(this.NumericValueUpdated);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(429, 199);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 15);
            this.label5.TabIndex = 12;
            this.label5.Text = "Blue:";
            // 
            // numericBlue
            // 
            this.numericBlue.Location = new System.Drawing.Point(476, 197);
            this.numericBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericBlue.Name = "numericBlue";
            this.numericBlue.Size = new System.Drawing.Size(66, 23);
            this.numericBlue.TabIndex = 11;
            this.numericBlue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericBlue.ValueChanged += new System.EventHandler(this.NumericValueUpdated);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(429, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 15);
            this.label4.TabIndex = 10;
            this.label4.Text = "Green:";
            // 
            // numericGreen
            // 
            this.numericGreen.Location = new System.Drawing.Point(476, 168);
            this.numericGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericGreen.Name = "numericGreen";
            this.numericGreen.Size = new System.Drawing.Size(66, 23);
            this.numericGreen.TabIndex = 9;
            this.numericGreen.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericGreen.ValueChanged += new System.EventHandler(this.NumericValueUpdated);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(429, 141);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Red:";
            // 
            // numericRed
            // 
            this.numericRed.Location = new System.Drawing.Point(476, 139);
            this.numericRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericRed.Name = "numericRed";
            this.numericRed.Size = new System.Drawing.Size(66, 23);
            this.numericRed.TabIndex = 7;
            this.numericRed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericRed.ValueChanged += new System.EventHandler(this.NumericValueUpdated);
            // 
            // sliderAlpha
            // 
            this.sliderAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sliderAlpha.Location = new System.Drawing.Point(381, 5);
            this.sliderAlpha.Maximum = System.Drawing.Color.White;
            this.sliderAlpha.Minimum = System.Drawing.Color.Empty;
            this.sliderAlpha.Name = "sliderAlpha";
            this.sliderAlpha.PickerSize = 7;
            this.sliderAlpha.Size = new System.Drawing.Size(42, 326);
            this.sliderAlpha.TabIndex = 5;
            this.sliderAlpha.ValueChanged += new System.EventHandler(this.panelColor_ValueChanged);
            // 
            // sliderColor
            // 
            this.sliderColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sliderColor.Location = new System.Drawing.Point(333, 5);
            this.sliderColor.Name = "sliderColor";
            this.sliderColor.PickerSize = 7;
            this.sliderColor.Size = new System.Drawing.Size(42, 326);
            this.sliderColor.TabIndex = 4;
            this.sliderColor.ValueChanged += new System.EventHandler(this.panelColor_ValueChanged);
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BottomLeftColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panelColor.BottomRightColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panelColor.Location = new System.Drawing.Point(5, 5);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(322, 326);
            this.panelColor.TabIndex = 3;
            this.panelColor.TopLeftColor = System.Drawing.Color.White;
            this.panelColor.TopRightColor = System.Drawing.Color.White;
            this.panelColor.ValuePercentual = ((System.Drawing.PointF)(resources.GetObject("panelColor.ValuePercentual")));
            this.panelColor.ValueChanged += new System.EventHandler(this.panelColor_ValueChanged);
            // 
            // boxCurrentColor
            // 
            this.boxCurrentColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boxCurrentColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.boxCurrentColor.Color = System.Drawing.Color.Transparent;
            this.boxCurrentColor.Location = new System.Drawing.Point(429, 5);
            this.boxCurrentColor.LowerColor = System.Drawing.Color.Transparent;
            this.boxCurrentColor.Name = "boxCurrentColor";
            this.boxCurrentColor.Size = new System.Drawing.Size(168, 113);
            this.boxCurrentColor.TabIndex = 1;
            this.boxCurrentColor.UpperColor = System.Drawing.Color.Transparent;
            // 
            // pageTexture
            // 
            this.pageTexture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.pageTexture.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageTexture.IsClosable = false;
            this.pageTexture.Location = new System.Drawing.Point(1, 1);
            this.pageTexture.Name = "pageTexture";
            this.pageTexture.Size = new System.Drawing.Size(608, 336);
            this.pageTexture.TabIndex = 0;
            this.pageTexture.Text = "Texture";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.buttonOK);
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(1, 394);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.panel1.Size = new System.Drawing.Size(610, 44);
            this.panel1.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.comboBrushType);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(1, 25);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(610, 31);
            this.panel2.TabIndex = 6;
            // 
            // comboBrushType
            // 
            this.comboBrushType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBrushType.FormattingEnabled = true;
            this.comboBrushType.Location = new System.Drawing.Point(75, 5);
            this.comboBrushType.Name = "comboBrushType";
            this.comboBrushType.Size = new System.Drawing.Size(165, 23);
            this.comboBrushType.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Brush type:";
            // 
            // formBrushEditor
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.Border = true;
            this.BorderColor = System.Drawing.Color.SteelBlue;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(612, 439);
            this.Controls.Add(this.tabBrushEditor);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.Silver;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formBrushEditor";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Resizable = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Brush Editor";
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.panel2, 0);
            this.Controls.SetChildIndex(this.tabBrushEditor, 0);
            this.tabBrushEditor.ResumeLayout(false);
            this.pageSolid.ResumeLayout(false);
            this.pageSolid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericAlpha)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRed)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private KRBTabControl.KRBTabControl tabBrushEditor;
        private KRBTabControl.TabPageEx pageSolid;
        private KRBTabControl.TabPageEx pageTexture;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ComboBox comboBrushType;
		private System.Windows.Forms.Label label1;
		private Fetze.WinFormsColor.ColorShowBox boxCurrentColor;
		private Fetze.WinFormsColor.ColorSlider sliderAlpha;
		private Fetze.WinFormsColor.ColorSlider sliderColor;
		private Fetze.WinFormsColor.ColorPanel panelColor;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numericAlpha;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numericBlue;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericGreen;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numericRed;
    }
}