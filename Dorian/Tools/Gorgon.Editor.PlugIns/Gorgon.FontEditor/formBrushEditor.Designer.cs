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
			this.colorSolidBrush = new Fetze.WinFormsColor.ColorPickerPanel();
			this.pageTexture = new KRBTabControl.TabPageEx();
			this.buttonOpen = new System.Windows.Forms.Button();
			this.panelTextureDisplay = new System.Windows.Forms.Panel();
			this.comboWrapMode = new System.Windows.Forms.ComboBox();
			this.numericHeight = new System.Windows.Forms.NumericUpDown();
			this.numericWidth = new System.Windows.Forms.NumericUpDown();
			this.numericY = new System.Windows.Forms.NumericUpDown();
			this.numericX = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.comboBrushType = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.imageFileBrowser = new GorgonLibrary.Editor.EditorFileBrowser();
			this.tabBrushEditor.SuspendLayout();
			this.pageSolid.SuspendLayout();
			this.pageTexture.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
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
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
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
			this.tabBrushEditor.IsCaptionVisible = false;
			this.tabBrushEditor.IsDocumentTabStyle = true;
			this.tabBrushEditor.IsDrawHeader = false;
			this.tabBrushEditor.IsUserInteraction = false;
			this.tabBrushEditor.ItemSize = new System.Drawing.Size(0, 28);
			this.tabBrushEditor.Location = new System.Drawing.Point(1, 56);
			this.tabBrushEditor.Name = "tabBrushEditor";
			this.tabBrushEditor.SelectedIndex = 1;
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
			this.pageSolid.Controls.Add(this.colorSolidBrush);
			this.pageSolid.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageSolid.ForeColor = System.Drawing.Color.White;
			this.pageSolid.IsClosable = false;
			this.pageSolid.Location = new System.Drawing.Point(1, 1);
			this.pageSolid.Name = "pageSolid";
			this.pageSolid.Size = new System.Drawing.Size(608, 303);
			this.pageSolid.TabIndex = 1;
			this.pageSolid.Text = "Solid";
			// 
			// colorSolidBrush
			// 
			this.colorSolidBrush.AlphaEnabled = true;
			this.colorSolidBrush.Dock = System.Windows.Forms.DockStyle.Fill;
			this.colorSolidBrush.Location = new System.Drawing.Point(0, 0);
			this.colorSolidBrush.Name = "colorSolidBrush";
			this.colorSolidBrush.OldColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorSolidBrush.PrimaryAttribute = Fetze.WinFormsColor.ColorPickerPanel.PrimaryAttrib.Hue;
			this.colorSolidBrush.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorSolidBrush.Size = new System.Drawing.Size(608, 303);
			this.colorSolidBrush.TabIndex = 0;
			this.colorSolidBrush.ColorChanged += new System.EventHandler(this.colorSolidBrush_ColorChanged);
			// 
			// pageTexture
			// 
			this.pageTexture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.pageTexture.Controls.Add(this.buttonOpen);
			this.pageTexture.Controls.Add(this.panelTextureDisplay);
			this.pageTexture.Controls.Add(this.comboWrapMode);
			this.pageTexture.Controls.Add(this.numericHeight);
			this.pageTexture.Controls.Add(this.numericWidth);
			this.pageTexture.Controls.Add(this.numericY);
			this.pageTexture.Controls.Add(this.numericX);
			this.pageTexture.Controls.Add(this.label6);
			this.pageTexture.Controls.Add(this.label5);
			this.pageTexture.Controls.Add(this.label4);
			this.pageTexture.Controls.Add(this.label3);
			this.pageTexture.Controls.Add(this.label2);
			this.pageTexture.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageTexture.IsClosable = false;
			this.pageTexture.Location = new System.Drawing.Point(1, 1);
			this.pageTexture.Name = "pageTexture";
			this.pageTexture.Size = new System.Drawing.Size(608, 303);
			this.pageTexture.TabIndex = 0;
			this.pageTexture.Text = "Texture";
			// 
			// buttonOpen
			// 
			this.buttonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOpen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOpen.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOpen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOpen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOpen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOpen.ForeColor = System.Drawing.Color.White;
			this.buttonOpen.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.open_image_16x16;
			this.buttonOpen.Location = new System.Drawing.Point(40, 272);
			this.buttonOpen.Name = "buttonOpen";
			this.buttonOpen.Size = new System.Drawing.Size(256, 28);
			this.buttonOpen.TabIndex = 6;
			this.buttonOpen.Text = "Open Texture";
			this.buttonOpen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOpen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.buttonOpen.UseVisualStyleBackColor = false;
			this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
			// 
			// panelTextureDisplay
			// 
			this.panelTextureDisplay.BackColor = System.Drawing.Color.White;
			this.panelTextureDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTextureDisplay.Location = new System.Drawing.Point(40, 10);
			this.panelTextureDisplay.Name = "panelTextureDisplay";
			this.panelTextureDisplay.Size = new System.Drawing.Size(256, 256);
			this.panelTextureDisplay.TabIndex = 0;
			this.panelTextureDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseDown);
			this.panelTextureDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseMove);
			this.panelTextureDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseUp);
			// 
			// comboWrapMode
			// 
			this.comboWrapMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboWrapMode.FormattingEnabled = true;
			this.comboWrapMode.Location = new System.Drawing.Point(345, 85);
			this.comboWrapMode.Name = "comboWrapMode";
			this.comboWrapMode.Size = new System.Drawing.Size(240, 23);
			this.comboWrapMode.TabIndex = 5;
			this.comboWrapMode.SelectedIndexChanged += new System.EventHandler(this.comboWrapMode_SelectedIndexChanged);
			// 
			// numericHeight
			// 
			this.numericHeight.Location = new System.Drawing.Point(521, 32);
			this.numericHeight.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericHeight.Name = "numericHeight";
			this.numericHeight.Size = new System.Drawing.Size(64, 23);
			this.numericHeight.TabIndex = 4;
			this.numericHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericHeight.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
			// 
			// numericWidth
			// 
			this.numericWidth.Location = new System.Drawing.Point(521, 3);
			this.numericWidth.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericWidth.Name = "numericWidth";
			this.numericWidth.Size = new System.Drawing.Size(64, 23);
			this.numericWidth.TabIndex = 3;
			this.numericWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericWidth.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
			// 
			// numericY
			// 
			this.numericY.Location = new System.Drawing.Point(365, 32);
			this.numericY.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericY.Name = "numericY";
			this.numericY.Size = new System.Drawing.Size(64, 23);
			this.numericY.TabIndex = 2;
			this.numericY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericY.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
			// 
			// numericX
			// 
			this.numericX.Location = new System.Drawing.Point(365, 3);
			this.numericX.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.numericX.Name = "numericX";
			this.numericX.Size = new System.Drawing.Size(64, 23);
			this.numericX.TabIndex = 1;
			this.numericX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericX.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(342, 67);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 15);
			this.label6.TabIndex = 5;
			this.label6.Text = "Wrapping mode:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(469, 34);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(46, 15);
			this.label5.TabIndex = 4;
			this.label5.Text = "Height:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(473, 5);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(42, 15);
			this.label4.TabIndex = 3;
			this.label4.Text = "Width:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(342, 34);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(17, 15);
			this.label3.TabIndex = 2;
			this.label3.Text = "Y:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(342, 5);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "X:";
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
			this.comboBrushType.SelectedIndexChanged += new System.EventHandler(this.comboBrushType_SelectedIndexChanged);
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
			// imageFileBrowser
			// 
			this.imageFileBrowser.DefaultExtension = "png";
			this.imageFileBrowser.Filename = null;
			this.imageFileBrowser.StartDirectory = null;
			this.imageFileBrowser.Text = "Open Image";
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
			this.pageTexture.ResumeLayout(false);
			this.pageTexture.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericX)).EndInit();
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
        private Fetze.WinFormsColor.ColorPickerPanel colorSolidBrush;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboWrapMode;
		private System.Windows.Forms.NumericUpDown numericHeight;
		private System.Windows.Forms.NumericUpDown numericWidth;
		private System.Windows.Forms.NumericUpDown numericY;
		private System.Windows.Forms.NumericUpDown numericX;
		private System.Windows.Forms.Panel panelTextureDisplay;
		private System.Windows.Forms.Button buttonOpen;
		private EditorFileBrowser imageFileBrowser;
    }
}