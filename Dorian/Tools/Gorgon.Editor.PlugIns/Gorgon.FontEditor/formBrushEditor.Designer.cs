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
			this.labelInfo = new System.Windows.Forms.Label();
			this.buttonOpen = new System.Windows.Forms.Button();
			this.panelTextureDisplay = new System.Windows.Forms.Panel();
			this.comboWrapMode = new System.Windows.Forms.ComboBox();
			this.numericHeight = new System.Windows.Forms.NumericUpDown();
			this.numericWidth = new System.Windows.Forms.NumericUpDown();
			this.numericY = new System.Windows.Forms.NumericUpDown();
			this.numericX = new System.Windows.Forms.NumericUpDown();
			this.labelWrapMode = new System.Windows.Forms.Label();
			this.labelTexHeight = new System.Windows.Forms.Label();
			this.labelTexWidth = new System.Windows.Forms.Label();
			this.labelTexTop = new System.Windows.Forms.Label();
			this.labelTexLeft = new System.Windows.Forms.Label();
			this.pageGradient = new KRBTabControl.TabPageEx();
			this.gradEditor = new GorgonLibrary.Editor.FontEditorPlugIn.Controls.panelGradient();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.comboBrushType = new System.Windows.Forms.ComboBox();
			this.labelBrushType = new System.Windows.Forms.Label();
			this.imageFileBrowser = new GorgonLibrary.Editor.EditorFileBrowser();
			this.tabBrushEditor.SuspendLayout();
			this.pageSolid.SuspendLayout();
			this.pageTexture.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
			this.pageGradient.SuspendLayout();
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
			this.buttonCancel.Text = "&Cancel";
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
			this.buttonOK.Text = "&OK";
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
			this.tabBrushEditor.Controls.Add(this.pageGradient);
			this.tabBrushEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabBrushEditor.IsCaptionVisible = false;
			this.tabBrushEditor.IsDocumentTabStyle = true;
			this.tabBrushEditor.IsDrawHeader = false;
			this.tabBrushEditor.IsUserInteraction = false;
			this.tabBrushEditor.ItemSize = new System.Drawing.Size(0, 28);
			this.tabBrushEditor.Location = new System.Drawing.Point(1, 56);
			this.tabBrushEditor.Name = "tabBrushEditor";
			this.tabBrushEditor.SelectedIndex = 2;
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
			this.pageTexture.Controls.Add(this.labelInfo);
			this.pageTexture.Controls.Add(this.buttonOpen);
			this.pageTexture.Controls.Add(this.panelTextureDisplay);
			this.pageTexture.Controls.Add(this.comboWrapMode);
			this.pageTexture.Controls.Add(this.numericHeight);
			this.pageTexture.Controls.Add(this.numericWidth);
			this.pageTexture.Controls.Add(this.numericY);
			this.pageTexture.Controls.Add(this.numericX);
			this.pageTexture.Controls.Add(this.labelWrapMode);
			this.pageTexture.Controls.Add(this.labelTexHeight);
			this.pageTexture.Controls.Add(this.labelTexWidth);
			this.pageTexture.Controls.Add(this.labelTexTop);
			this.pageTexture.Controls.Add(this.labelTexLeft);
			this.pageTexture.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageTexture.IsClosable = false;
			this.pageTexture.Location = new System.Drawing.Point(1, 1);
			this.pageTexture.Name = "pageTexture";
			this.pageTexture.Size = new System.Drawing.Size(608, 303);
			this.pageTexture.TabIndex = 0;
			this.pageTexture.Text = "Texture";
			// 
			// labelInfo
			// 
			this.labelInfo.Location = new System.Drawing.Point(326, 117);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(259, 183);
			this.labelInfo.TabIndex = 7;
			this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			this.comboWrapMode.Location = new System.Drawing.Point(326, 85);
			this.comboWrapMode.Name = "comboWrapMode";
			this.comboWrapMode.Size = new System.Drawing.Size(259, 23);
			this.comboWrapMode.TabIndex = 5;
			this.comboWrapMode.SelectedIndexChanged += new System.EventHandler(this.comboWrapMode_SelectedIndexChanged);
			// 
			// numericHeight
			// 
			this.numericHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
			this.numericWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
			this.numericY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
			this.numericX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
			// labelWrapMode
			// 
			this.labelWrapMode.AutoSize = true;
			this.labelWrapMode.Location = new System.Drawing.Point(323, 67);
			this.labelWrapMode.Name = "labelWrapMode";
			this.labelWrapMode.Size = new System.Drawing.Size(96, 15);
			this.labelWrapMode.TabIndex = 5;
			this.labelWrapMode.Text = "Wrapping mode:";
			// 
			// labelTexHeight
			// 
			this.labelTexHeight.AutoSize = true;
			this.labelTexHeight.Location = new System.Drawing.Point(469, 34);
			this.labelTexHeight.Name = "labelTexHeight";
			this.labelTexHeight.Size = new System.Drawing.Size(46, 15);
			this.labelTexHeight.TabIndex = 4;
			this.labelTexHeight.Text = "Height:";
			// 
			// labelTexWidth
			// 
			this.labelTexWidth.AutoSize = true;
			this.labelTexWidth.Location = new System.Drawing.Point(473, 5);
			this.labelTexWidth.Name = "labelTexWidth";
			this.labelTexWidth.Size = new System.Drawing.Size(42, 15);
			this.labelTexWidth.TabIndex = 3;
			this.labelTexWidth.Text = "Width:";
			// 
			// labelTexTop
			// 
			this.labelTexTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTexTop.AutoSize = true;
			this.labelTexTop.Location = new System.Drawing.Point(323, 34);
			this.labelTexTop.Name = "labelTexTop";
			this.labelTexTop.Size = new System.Drawing.Size(17, 15);
			this.labelTexTop.TabIndex = 2;
			this.labelTexTop.Text = "Y:";
			// 
			// labelTexLeft
			// 
			this.labelTexLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTexLeft.AutoSize = true;
			this.labelTexLeft.Location = new System.Drawing.Point(323, 5);
			this.labelTexLeft.Name = "labelTexLeft";
			this.labelTexLeft.Size = new System.Drawing.Size(17, 15);
			this.labelTexLeft.TabIndex = 1;
			this.labelTexLeft.Text = "X:";
			// 
			// pageGradient
			// 
			this.pageGradient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.pageGradient.Controls.Add(this.gradEditor);
			this.pageGradient.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.pageGradient.IsClosable = false;
			this.pageGradient.Location = new System.Drawing.Point(1, 1);
			this.pageGradient.Name = "pageGradient";
			this.pageGradient.Size = new System.Drawing.Size(608, 303);
			this.pageGradient.TabIndex = 2;
			this.pageGradient.Text = "Gradient";
			// 
			// gradEditor
			// 
			this.gradEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradEditor.Location = new System.Drawing.Point(0, 0);
			this.gradEditor.Name = "gradEditor";
			this.gradEditor.Size = new System.Drawing.Size(608, 303);
			this.gradEditor.TabIndex = 0;
			this.gradEditor.BrushChanged += new System.EventHandler(this.gradEditor_BrushChanged);
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
			this.panel2.Controls.Add(this.labelBrushType);
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
			// labelBrushType
			// 
			this.labelBrushType.AutoSize = true;
			this.labelBrushType.Location = new System.Drawing.Point(3, 8);
			this.labelBrushType.Name = "labelBrushType";
			this.labelBrushType.Size = new System.Drawing.Size(66, 15);
			this.labelBrushType.TabIndex = 0;
			this.labelBrushType.Text = "Brush type:";
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
			this.pageGradient.ResumeLayout(false);
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
        private System.Windows.Forms.Label labelBrushType;
        private Fetze.WinFormsColor.ColorPickerPanel colorSolidBrush;
		private System.Windows.Forms.Label labelWrapMode;
		private System.Windows.Forms.Label labelTexHeight;
		private System.Windows.Forms.Label labelTexWidth;
		private System.Windows.Forms.Label labelTexTop;
		private System.Windows.Forms.Label labelTexLeft;
		private System.Windows.Forms.ComboBox comboWrapMode;
		private System.Windows.Forms.NumericUpDown numericHeight;
		private System.Windows.Forms.NumericUpDown numericWidth;
		private System.Windows.Forms.NumericUpDown numericY;
		private System.Windows.Forms.NumericUpDown numericX;
		private System.Windows.Forms.Panel panelTextureDisplay;
		private System.Windows.Forms.Button buttonOpen;
		private EditorFileBrowser imageFileBrowser;
		private System.Windows.Forms.Label labelInfo;
		private KRBTabControl.TabPageEx pageGradient;
		private Controls.panelGradient gradEditor;
    }
}