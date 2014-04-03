namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    partial class GorgonFontContentPanel
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

            if (!_disposed)
            {
                if (_kernComboFont != null)
                {
                    _kernComboFont.Dispose();
                }

	            if (_zoomFont != null)
	            {
					_zoomFont.Dispose();
	            }

				this.MouseWheel -= PanelDisplay_MouseWheel;

	            if (_rawKeyboard != null)
	            {
		            _rawKeyboard.KeyDown -= GorgonFontContentPanel_KeyDown;
					_rawKeyboard.KeyUp -= GorgonFontContentPanel_KeyUp;
		            _rawKeyboard.Dispose();
	            }

	            if (_rawMouse != null)
	            {
		            _rawMouse.Dispose();
	            }

                if (_pattern != null)
                {
                    _pattern.Dispose();
                }

                _kernComboFont = null;
                _zoomFont = null;
	            _rawMouse = null;
	            _rawKeyboard = null;
                _pattern = null;
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GorgonFontContentPanel));
			this.panelTextures = new GorgonLibrary.UI.GorgonSelectablePanel();
			this.panelGlyphEdit = new System.Windows.Forms.Panel();
			this.panelKerningPairs = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.numericKerningOffset = new System.Windows.Forms.NumericUpDown();
			this.buttonKernCancel = new System.Windows.Forms.Button();
			this.buttonKernOK = new System.Windows.Forms.Button();
			this.labelKerningOffset = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.comboSecondGlyph = new System.Windows.Forms.ComboBox();
			this.labelSecondaryGlyph = new System.Windows.Forms.Label();
			this.panelGlyphAdvance = new System.Windows.Forms.Panel();
			this.buttonResetGlyphOffset = new System.Windows.Forms.Button();
			this.buttonResetGlyphAdvance = new System.Windows.Forms.Button();
			this.numericGlyphAdvance = new System.Windows.Forms.NumericUpDown();
			this.numericOffsetY = new System.Windows.Forms.NumericUpDown();
			this.numericOffsetX = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.panelGlyphClip = new System.Windows.Forms.Panel();
			this.buttonGlyphClipCancel = new System.Windows.Forms.Button();
			this.buttonGlyphClipOK = new System.Windows.Forms.Button();
			this.checkZoomSnap = new System.Windows.Forms.CheckBox();
			this.numericZoomAmount = new System.Windows.Forms.NumericUpDown();
			this.numericZoomWindowSize = new System.Windows.Forms.NumericUpDown();
			this.numericGlyphHeight = new System.Windows.Forms.NumericUpDown();
			this.numericGlyphWidth = new System.Windows.Forms.NumericUpDown();
			this.numericGlyphTop = new System.Windows.Forms.NumericUpDown();
			this.numericGlyphLeft = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.labelZoomWindowSize = new System.Windows.Forms.Label();
			this.labelGlyphHeight = new System.Windows.Forms.Label();
			this.labelGlyphWidth = new System.Windows.Forms.Label();
			this.labelGlyphTop = new System.Windows.Forms.Label();
			this.labelGlyphLeft = new System.Windows.Forms.Label();
			this.panelText = new System.Windows.Forms.Panel();
			this.splitContent = new System.Windows.Forms.Splitter();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel5 = new System.Windows.Forms.Panel();
			this.panelInnerDisplay = new System.Windows.Forms.Panel();
			this.panelHorzScroll = new System.Windows.Forms.Panel();
			this.scrollHorizontal = new System.Windows.Forms.HScrollBar();
			this.panelVertScroll = new System.Windows.Forms.Panel();
			this.scrollVertical = new System.Windows.Forms.VScrollBar();
			this.panelControls = new System.Windows.Forms.Panel();
			this.stripFontDisplay = new System.Windows.Forms.ToolStrip();
			this.buttonPrevTexture = new System.Windows.Forms.ToolStripButton();
			this.labelTextureCount = new System.Windows.Forms.ToolStripLabel();
			this.buttonNextTexture = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.dropDownZoom = new System.Windows.Forms.ToolStripDropDownButton();
			this.menuItem1600 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem800 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem400 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem200 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem100 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem75 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem50 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem25 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemToWindow = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.labelSelectedGlyphInfo = new System.Windows.Forms.ToolStripLabel();
			this.separatorGlyphInfo = new System.Windows.Forms.ToolStripSeparator();
			this.labelHoverGlyphInfo = new System.Windows.Forms.ToolStripLabel();
			this.panelToolbar = new System.Windows.Forms.Panel();
			this.stripGlyphs = new System.Windows.Forms.ToolStrip();
			this.buttonEditGlyph = new System.Windows.Forms.ToolStripButton();
			this.sepGlyphSpacing = new System.Windows.Forms.ToolStripSeparator();
			this.buttonGlyphKern = new System.Windows.Forms.ToolStripButton();
			this.buttonGlyphTools = new System.Windows.Forms.ToolStripSplitButton();
			this.menuItemSetGlyph = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemLoadGlyphImage = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRemoveGlyphImage = new System.Windows.Forms.ToolStripMenuItem();
			this.stripCommands = new System.Windows.Forms.ToolStrip();
			this.menuTextColor = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemSampleTextForeground = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSampleTextBackground = new System.Windows.Forms.ToolStripMenuItem();
			this.menuShadow = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemPreviewShadowEnable = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.itemShadowOpacity = new System.Windows.Forms.ToolStripMenuItem();
			this.itemShadowOffset = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.textPreviewText = new System.Windows.Forms.ToolStripTextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.imageFileBrowser = new GorgonLibrary.Editor.EditorFileBrowser();
			this.tipButtons = new System.Windows.Forms.ToolTip(this.components);
			this.PanelDisplay.SuspendLayout();
			this.panelGlyphEdit.SuspendLayout();
			this.panelKerningPairs.SuspendLayout();
			this.panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericKerningOffset)).BeginInit();
			this.panel3.SuspendLayout();
			this.panelGlyphAdvance.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphAdvance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericOffsetY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericOffsetX)).BeginInit();
			this.panelGlyphClip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomAmount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomWindowSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphTop)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphLeft)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panelInnerDisplay.SuspendLayout();
			this.panelHorzScroll.SuspendLayout();
			this.panelVertScroll.SuspendLayout();
			this.panelControls.SuspendLayout();
			this.stripFontDisplay.SuspendLayout();
			this.panelToolbar.SuspendLayout();
			this.stripGlyphs.SuspendLayout();
			this.stripCommands.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelDisplay
			// 
			this.PanelDisplay.Controls.Add(this.splitContent);
			this.PanelDisplay.Controls.Add(this.panel1);
			this.PanelDisplay.Controls.Add(this.panel2);
			this.PanelDisplay.Size = new System.Drawing.Size(806, 606);
			// 
			// panelTextures
			// 
			this.panelTextures.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelTextures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panelTextures.Location = new System.Drawing.Point(0, 0);
			this.panelTextures.Name = "panelTextures";
			this.panelTextures.ShowFocus = false;
			this.panelTextures.Size = new System.Drawing.Size(630, 331);
			this.panelTextures.TabIndex = 0;
			this.panelTextures.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GorgonFontContentPanel_MouseClick);
			this.panelTextures.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseDoubleClick);
			this.panelTextures.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseDown);
			this.panelTextures.MouseEnter += new System.EventHandler(this.panelTextures_MouseEnter);
			this.panelTextures.MouseLeave += new System.EventHandler(this.panelTextures_MouseLeave);
			this.panelTextures.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseMove);
			this.panelTextures.Resize += new System.EventHandler(this.TextureDisplayResize);
			// 
			// panelGlyphEdit
			// 
			this.panelGlyphEdit.Controls.Add(this.panelKerningPairs);
			this.panelGlyphEdit.Controls.Add(this.panelGlyphAdvance);
			this.panelGlyphEdit.Controls.Add(this.panelGlyphClip);
			this.panelGlyphEdit.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelGlyphEdit.Location = new System.Drawing.Point(646, 0);
			this.panelGlyphEdit.Name = "panelGlyphEdit";
			this.panelGlyphEdit.Size = new System.Drawing.Size(160, 347);
			this.panelGlyphEdit.TabIndex = 4;
			this.panelGlyphEdit.Visible = false;
			// 
			// panelKerningPairs
			// 
			this.panelKerningPairs.Controls.Add(this.panel4);
			this.panelKerningPairs.Controls.Add(this.panel3);
			this.panelKerningPairs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelKerningPairs.Location = new System.Drawing.Point(0, 0);
			this.panelKerningPairs.Name = "panelKerningPairs";
			this.panelKerningPairs.Size = new System.Drawing.Size(160, 347);
			this.panelKerningPairs.TabIndex = 5;
			this.panelKerningPairs.Visible = false;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.numericKerningOffset);
			this.panel4.Controls.Add(this.buttonKernCancel);
			this.panel4.Controls.Add(this.buttonKernOK);
			this.panel4.Controls.Add(this.labelKerningOffset);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 44);
			this.panel4.Name = "panel4";
			this.panel4.Padding = new System.Windows.Forms.Padding(3);
			this.panel4.Size = new System.Drawing.Size(160, 73);
			this.panel4.TabIndex = 5;
			// 
			// numericKerningOffset
			// 
			this.numericKerningOffset.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericKerningOffset.Dock = System.Windows.Forms.DockStyle.Top;
			this.numericKerningOffset.Location = new System.Drawing.Point(3, 18);
			this.numericKerningOffset.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.numericKerningOffset.Minimum = new decimal(new int[] {
            2048,
            0,
            0,
            -2147483648});
			this.numericKerningOffset.Name = "numericKerningOffset";
			this.numericKerningOffset.Size = new System.Drawing.Size(154, 23);
			this.numericKerningOffset.TabIndex = 1;
			this.numericKerningOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericKerningOffset.ValueChanged += new System.EventHandler(this.numericKerningOffset_ValueChanged);
			// 
			// buttonKernCancel
			// 
			this.buttonKernCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonKernCancel.Enabled = false;
			this.buttonKernCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonKernCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonKernCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonKernCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonKernCancel.ForeColor = System.Drawing.Color.White;
			this.buttonKernCancel.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonKernCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonKernCancel.Location = new System.Drawing.Point(82, 44);
			this.buttonKernCancel.Name = "buttonKernCancel";
			this.buttonKernCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonKernCancel.TabIndex = 3;
			this.buttonKernCancel.Text = "Reset";
			this.buttonKernCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.tipButtons.SetToolTip(this.buttonKernCancel, "Cancel kerning pair update.");
			this.buttonKernCancel.UseVisualStyleBackColor = false;
			// 
			// buttonKernOK
			// 
			this.buttonKernOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonKernOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonKernOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonKernOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonKernOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonKernOK.ForeColor = System.Drawing.Color.White;
			this.buttonKernOK.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonKernOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonKernOK.Location = new System.Drawing.Point(3, 44);
			this.buttonKernOK.Name = "buttonKernOK";
			this.buttonKernOK.Size = new System.Drawing.Size(75, 23);
			this.buttonKernOK.TabIndex = 2;
			this.buttonKernOK.Text = "Update";
			this.buttonKernOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.tipButtons.SetToolTip(this.buttonKernOK, "Updating kerning value.");
			this.buttonKernOK.UseVisualStyleBackColor = false;
			this.buttonKernOK.Click += new System.EventHandler(this.buttonKernOK_Click);
			// 
			// labelKerningOffset
			// 
			this.labelKerningOffset.AutoSize = true;
			this.labelKerningOffset.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelKerningOffset.Location = new System.Drawing.Point(3, 3);
			this.labelKerningOffset.Name = "labelKerningOffset";
			this.labelKerningOffset.Size = new System.Drawing.Size(69, 15);
			this.labelKerningOffset.TabIndex = 1;
			this.labelKerningOffset.Text = "Kern Offset:";
			// 
			// panel3
			// 
			this.panel3.AutoSize = true;
			this.panel3.Controls.Add(this.comboSecondGlyph);
			this.panel3.Controls.Add(this.labelSecondaryGlyph);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Padding = new System.Windows.Forms.Padding(3);
			this.panel3.Size = new System.Drawing.Size(160, 44);
			this.panel3.TabIndex = 4;
			// 
			// comboSecondGlyph
			// 
			this.comboSecondGlyph.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboSecondGlyph.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSecondGlyph.FormattingEnabled = true;
			this.comboSecondGlyph.Location = new System.Drawing.Point(3, 18);
			this.comboSecondGlyph.Name = "comboSecondGlyph";
			this.comboSecondGlyph.Size = new System.Drawing.Size(154, 23);
			this.comboSecondGlyph.TabIndex = 0;
			this.comboSecondGlyph.SelectedIndexChanged += new System.EventHandler(this.comboSecondGlyph_SelectedIndexChanged);
			// 
			// labelSecondaryGlyph
			// 
			this.labelSecondaryGlyph.AutoSize = true;
			this.labelSecondaryGlyph.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelSecondaryGlyph.Location = new System.Drawing.Point(3, 3);
			this.labelSecondaryGlyph.Name = "labelSecondaryGlyph";
			this.labelSecondaryGlyph.Size = new System.Drawing.Size(83, 15);
			this.labelSecondaryGlyph.TabIndex = 0;
			this.labelSecondaryGlyph.Text = "Second Glyph:";
			// 
			// panelGlyphAdvance
			// 
			this.panelGlyphAdvance.Controls.Add(this.buttonResetGlyphOffset);
			this.panelGlyphAdvance.Controls.Add(this.buttonResetGlyphAdvance);
			this.panelGlyphAdvance.Controls.Add(this.numericGlyphAdvance);
			this.panelGlyphAdvance.Controls.Add(this.numericOffsetY);
			this.panelGlyphAdvance.Controls.Add(this.numericOffsetX);
			this.panelGlyphAdvance.Controls.Add(this.label5);
			this.panelGlyphAdvance.Controls.Add(this.label6);
			this.panelGlyphAdvance.Controls.Add(this.label7);
			this.panelGlyphAdvance.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelGlyphAdvance.Location = new System.Drawing.Point(0, 0);
			this.panelGlyphAdvance.Name = "panelGlyphAdvance";
			this.panelGlyphAdvance.Size = new System.Drawing.Size(160, 347);
			this.panelGlyphAdvance.TabIndex = 1;
			this.panelGlyphAdvance.Visible = false;
			// 
			// buttonResetGlyphOffset
			// 
			this.buttonResetGlyphOffset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonResetGlyphOffset.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonResetGlyphOffset.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonResetGlyphOffset.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonResetGlyphOffset.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonResetGlyphOffset.ForeColor = System.Drawing.Color.White;
			this.buttonResetGlyphOffset.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonResetGlyphOffset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonResetGlyphOffset.Location = new System.Drawing.Point(83, 91);
			this.buttonResetGlyphOffset.Name = "buttonResetGlyphOffset";
			this.buttonResetGlyphOffset.Size = new System.Drawing.Size(68, 23);
			this.buttonResetGlyphOffset.TabIndex = 2;
			this.buttonResetGlyphOffset.Text = "Reset";
			this.buttonResetGlyphOffset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.tipButtons.SetToolTip(this.buttonResetGlyphOffset, "Reset the current glyph offset to the original values.");
			this.buttonResetGlyphOffset.UseVisualStyleBackColor = false;
			this.buttonResetGlyphOffset.Click += new System.EventHandler(this.buttonResetGlyphOffset_Click);
			// 
			// buttonResetGlyphAdvance
			// 
			this.buttonResetGlyphAdvance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonResetGlyphAdvance.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonResetGlyphAdvance.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonResetGlyphAdvance.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonResetGlyphAdvance.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonResetGlyphAdvance.ForeColor = System.Drawing.Color.White;
			this.buttonResetGlyphAdvance.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonResetGlyphAdvance.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonResetGlyphAdvance.Location = new System.Drawing.Point(83, 167);
			this.buttonResetGlyphAdvance.Name = "buttonResetGlyphAdvance";
			this.buttonResetGlyphAdvance.Size = new System.Drawing.Size(68, 23);
			this.buttonResetGlyphAdvance.TabIndex = 4;
			this.buttonResetGlyphAdvance.Text = "Reset";
			this.buttonResetGlyphAdvance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.tipButtons.SetToolTip(this.buttonResetGlyphAdvance, "Reset the current glyph advance to the original value.");
			this.buttonResetGlyphAdvance.UseVisualStyleBackColor = false;
			this.buttonResetGlyphAdvance.Click += new System.EventHandler(this.buttonResetGlyphAdvance_Click);
			// 
			// numericGlyphAdvance
			// 
			this.numericGlyphAdvance.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericGlyphAdvance.Location = new System.Drawing.Point(9, 138);
			this.numericGlyphAdvance.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.numericGlyphAdvance.Minimum = new decimal(new int[] {
            2048,
            0,
            0,
            -2147483648});
			this.numericGlyphAdvance.Name = "numericGlyphAdvance";
			this.numericGlyphAdvance.Size = new System.Drawing.Size(142, 23);
			this.numericGlyphAdvance.TabIndex = 3;
			this.numericGlyphAdvance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericGlyphAdvance.ValueChanged += new System.EventHandler(this.numericGlyphAdvance_ValueChanged);
			// 
			// numericOffsetY
			// 
			this.numericOffsetY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericOffsetY.Location = new System.Drawing.Point(9, 62);
			this.numericOffsetY.Maximum = new decimal(new int[] {
            20000000,
            0,
            0,
            0});
			this.numericOffsetY.Name = "numericOffsetY";
			this.numericOffsetY.Size = new System.Drawing.Size(142, 23);
			this.numericOffsetY.TabIndex = 1;
			this.numericOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericOffsetY.ValueChanged += new System.EventHandler(this.numericOffsetX_ValueChanged);
			// 
			// numericOffsetX
			// 
			this.numericOffsetX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericOffsetX.Location = new System.Drawing.Point(9, 18);
			this.numericOffsetX.Maximum = new decimal(new int[] {
            20000000,
            0,
            0,
            0});
			this.numericOffsetX.Name = "numericOffsetX";
			this.numericOffsetX.Size = new System.Drawing.Size(142, 23);
			this.numericOffsetX.TabIndex = 0;
			this.numericOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericOffsetX.ValueChanged += new System.EventHandler(this.numericOffsetX_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 120);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(90, 15);
			this.label5.TabIndex = 2;
			this.label5.Text = "Glyph Advance:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 44);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(66, 15);
			this.label6.TabIndex = 1;
			this.label6.Text = "Top Offset:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(65, 15);
			this.label7.TabIndex = 0;
			this.label7.Text = "Left Offset:";
			// 
			// panelGlyphClip
			// 
			this.panelGlyphClip.Controls.Add(this.buttonGlyphClipCancel);
			this.panelGlyphClip.Controls.Add(this.buttonGlyphClipOK);
			this.panelGlyphClip.Controls.Add(this.checkZoomSnap);
			this.panelGlyphClip.Controls.Add(this.numericZoomAmount);
			this.panelGlyphClip.Controls.Add(this.numericZoomWindowSize);
			this.panelGlyphClip.Controls.Add(this.numericGlyphHeight);
			this.panelGlyphClip.Controls.Add(this.numericGlyphWidth);
			this.panelGlyphClip.Controls.Add(this.numericGlyphTop);
			this.panelGlyphClip.Controls.Add(this.numericGlyphLeft);
			this.panelGlyphClip.Controls.Add(this.label1);
			this.panelGlyphClip.Controls.Add(this.labelZoomWindowSize);
			this.panelGlyphClip.Controls.Add(this.labelGlyphHeight);
			this.panelGlyphClip.Controls.Add(this.labelGlyphWidth);
			this.panelGlyphClip.Controls.Add(this.labelGlyphTop);
			this.panelGlyphClip.Controls.Add(this.labelGlyphLeft);
			this.panelGlyphClip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelGlyphClip.Location = new System.Drawing.Point(0, 0);
			this.panelGlyphClip.Name = "panelGlyphClip";
			this.panelGlyphClip.Size = new System.Drawing.Size(160, 347);
			this.panelGlyphClip.TabIndex = 0;
			this.panelGlyphClip.Visible = false;
			// 
			// buttonGlyphClipCancel
			// 
			this.buttonGlyphClipCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonGlyphClipCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonGlyphClipCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonGlyphClipCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonGlyphClipCancel.ForeColor = System.Drawing.Color.White;
			this.buttonGlyphClipCancel.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonGlyphClipCancel.Location = new System.Drawing.Point(126, 313);
			this.buttonGlyphClipCancel.Name = "buttonGlyphClipCancel";
			this.buttonGlyphClipCancel.Size = new System.Drawing.Size(28, 28);
			this.buttonGlyphClipCancel.TabIndex = 8;
			this.buttonGlyphClipCancel.UseVisualStyleBackColor = false;
			this.buttonGlyphClipCancel.Click += new System.EventHandler(this.buttonGlyphClipCancel_Click);
			// 
			// buttonGlyphClipOK
			// 
			this.buttonGlyphClipOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonGlyphClipOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonGlyphClipOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonGlyphClipOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonGlyphClipOK.ForeColor = System.Drawing.Color.White;
			this.buttonGlyphClipOK.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonGlyphClipOK.Location = new System.Drawing.Point(92, 313);
			this.buttonGlyphClipOK.Name = "buttonGlyphClipOK";
			this.buttonGlyphClipOK.Size = new System.Drawing.Size(28, 28);
			this.buttonGlyphClipOK.TabIndex = 7;
			this.buttonGlyphClipOK.UseVisualStyleBackColor = false;
			this.buttonGlyphClipOK.Click += new System.EventHandler(this.buttonGlyphClipOK_Click);
			// 
			// checkZoomSnap
			// 
			this.checkZoomSnap.Location = new System.Drawing.Point(9, 196);
			this.checkZoomSnap.Name = "checkZoomSnap";
			this.checkZoomSnap.Size = new System.Drawing.Size(143, 34);
			this.checkZoomSnap.TabIndex = 6;
			this.checkZoomSnap.Text = "Snap zoom window to corners";
			this.checkZoomSnap.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.checkZoomSnap.UseVisualStyleBackColor = true;
			this.checkZoomSnap.Click += new System.EventHandler(this.checkZoomSnap_Click);
			// 
			// numericZoomAmount
			// 
			this.numericZoomAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericZoomAmount.DecimalPlaces = 1;
			this.numericZoomAmount.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.numericZoomAmount.Location = new System.Drawing.Point(9, 167);
			this.numericZoomAmount.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.numericZoomAmount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericZoomAmount.Name = "numericZoomAmount";
			this.numericZoomAmount.Size = new System.Drawing.Size(143, 23);
			this.numericZoomAmount.TabIndex = 5;
			this.numericZoomAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericZoomAmount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numericZoomAmount.ValueChanged += new System.EventHandler(this.numericZoomAmount_ValueChanged);
			this.numericZoomAmount.Enter += new System.EventHandler(this.numericGlyphLeft_Enter);
			this.numericZoomAmount.Leave += new System.EventHandler(this.numericGlyphLeft_Leave);
			// 
			// numericZoomWindowSize
			// 
			this.numericZoomWindowSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericZoomWindowSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericZoomWindowSize.Location = new System.Drawing.Point(9, 123);
			this.numericZoomWindowSize.Maximum = new decimal(new int[] {
            384,
            0,
            0,
            0});
			this.numericZoomWindowSize.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericZoomWindowSize.Name = "numericZoomWindowSize";
			this.numericZoomWindowSize.Size = new System.Drawing.Size(143, 23);
			this.numericZoomWindowSize.TabIndex = 4;
			this.numericZoomWindowSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericZoomWindowSize.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericZoomWindowSize.ValueChanged += new System.EventHandler(this.numericZoomWindowSize_ValueChanged);
			this.numericZoomWindowSize.Enter += new System.EventHandler(this.numericGlyphLeft_Enter);
			this.numericZoomWindowSize.Leave += new System.EventHandler(this.numericGlyphLeft_Leave);
			// 
			// numericGlyphHeight
			// 
			this.numericGlyphHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericGlyphHeight.Location = new System.Drawing.Point(84, 62);
			this.numericGlyphHeight.Maximum = new decimal(new int[] {
            20000000,
            0,
            0,
            0});
			this.numericGlyphHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericGlyphHeight.Name = "numericGlyphHeight";
			this.numericGlyphHeight.Size = new System.Drawing.Size(68, 23);
			this.numericGlyphHeight.TabIndex = 3;
			this.numericGlyphHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericGlyphHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericGlyphHeight.ValueChanged += new System.EventHandler(this.numericGlyphLeft_ValueChanged);
			this.numericGlyphHeight.Enter += new System.EventHandler(this.numericGlyphLeft_Enter);
			this.numericGlyphHeight.Leave += new System.EventHandler(this.numericGlyphLeft_Leave);
			// 
			// numericGlyphWidth
			// 
			this.numericGlyphWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericGlyphWidth.Location = new System.Drawing.Point(9, 62);
			this.numericGlyphWidth.Maximum = new decimal(new int[] {
            20000000,
            0,
            0,
            0});
			this.numericGlyphWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericGlyphWidth.Name = "numericGlyphWidth";
			this.numericGlyphWidth.Size = new System.Drawing.Size(68, 23);
			this.numericGlyphWidth.TabIndex = 2;
			this.numericGlyphWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericGlyphWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericGlyphWidth.ValueChanged += new System.EventHandler(this.numericGlyphLeft_ValueChanged);
			this.numericGlyphWidth.Enter += new System.EventHandler(this.numericGlyphLeft_Enter);
			this.numericGlyphWidth.Leave += new System.EventHandler(this.numericGlyphLeft_Leave);
			// 
			// numericGlyphTop
			// 
			this.numericGlyphTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericGlyphTop.Location = new System.Drawing.Point(83, 18);
			this.numericGlyphTop.Maximum = new decimal(new int[] {
            20000000,
            0,
            0,
            0});
			this.numericGlyphTop.Name = "numericGlyphTop";
			this.numericGlyphTop.Size = new System.Drawing.Size(68, 23);
			this.numericGlyphTop.TabIndex = 1;
			this.numericGlyphTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericGlyphTop.ValueChanged += new System.EventHandler(this.numericGlyphLeft_ValueChanged);
			this.numericGlyphTop.Enter += new System.EventHandler(this.numericGlyphLeft_Enter);
			this.numericGlyphTop.Leave += new System.EventHandler(this.numericGlyphLeft_Leave);
			// 
			// numericGlyphLeft
			// 
			this.numericGlyphLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericGlyphLeft.Location = new System.Drawing.Point(9, 18);
			this.numericGlyphLeft.Maximum = new decimal(new int[] {
            20000000,
            0,
            0,
            0});
			this.numericGlyphLeft.Name = "numericGlyphLeft";
			this.numericGlyphLeft.Size = new System.Drawing.Size(68, 23);
			this.numericGlyphLeft.TabIndex = 0;
			this.numericGlyphLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericGlyphLeft.ValueChanged += new System.EventHandler(this.numericGlyphLeft_ValueChanged);
			this.numericGlyphLeft.Enter += new System.EventHandler(this.numericGlyphLeft_Enter);
			this.numericGlyphLeft.Leave += new System.EventHandler(this.numericGlyphLeft_Leave);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 149);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 15);
			this.label1.TabIndex = 7;
			this.label1.Text = "Zoom Amount:";
			// 
			// labelZoomWindowSize
			// 
			this.labelZoomWindowSize.AutoSize = true;
			this.labelZoomWindowSize.Location = new System.Drawing.Point(8, 105);
			this.labelZoomWindowSize.Name = "labelZoomWindowSize";
			this.labelZoomWindowSize.Size = new System.Drawing.Size(112, 15);
			this.labelZoomWindowSize.TabIndex = 5;
			this.labelZoomWindowSize.Text = "Zoom Window Size:";
			// 
			// labelGlyphHeight
			// 
			this.labelGlyphHeight.AutoSize = true;
			this.labelGlyphHeight.Location = new System.Drawing.Point(81, 44);
			this.labelGlyphHeight.Name = "labelGlyphHeight";
			this.labelGlyphHeight.Size = new System.Drawing.Size(46, 15);
			this.labelGlyphHeight.TabIndex = 3;
			this.labelGlyphHeight.Text = "Height:";
			// 
			// labelGlyphWidth
			// 
			this.labelGlyphWidth.AutoSize = true;
			this.labelGlyphWidth.Location = new System.Drawing.Point(6, 44);
			this.labelGlyphWidth.Name = "labelGlyphWidth";
			this.labelGlyphWidth.Size = new System.Drawing.Size(42, 15);
			this.labelGlyphWidth.TabIndex = 2;
			this.labelGlyphWidth.Text = "Width:";
			// 
			// labelGlyphTop
			// 
			this.labelGlyphTop.AutoSize = true;
			this.labelGlyphTop.Location = new System.Drawing.Point(80, 0);
			this.labelGlyphTop.Name = "labelGlyphTop";
			this.labelGlyphTop.Size = new System.Drawing.Size(31, 15);
			this.labelGlyphTop.TabIndex = 1;
			this.labelGlyphTop.Text = "Top:";
			// 
			// labelGlyphLeft
			// 
			this.labelGlyphLeft.AutoSize = true;
			this.labelGlyphLeft.Location = new System.Drawing.Point(6, 0);
			this.labelGlyphLeft.Name = "labelGlyphLeft";
			this.labelGlyphLeft.Size = new System.Drawing.Size(30, 15);
			this.labelGlyphLeft.TabIndex = 0;
			this.labelGlyphLeft.Text = "Left:";
			// 
			// panelText
			// 
			this.panelText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panelText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelText.ForeColor = System.Drawing.Color.Black;
			this.panelText.Location = new System.Drawing.Point(0, 25);
			this.panelText.Name = "panelText";
			this.panelText.Size = new System.Drawing.Size(806, 183);
			this.panelText.TabIndex = 1;
			this.panelText.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GorgonFontContentPanel_MouseClick);
			this.panelText.Resize += new System.EventHandler(this.panelText_Resize);
			// 
			// splitContent
			// 
			this.splitContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.splitContent.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitContent.Location = new System.Drawing.Point(0, 394);
			this.splitContent.MinExtra = 320;
			this.splitContent.MinSize = 150;
			this.splitContent.Name = "splitContent";
			this.splitContent.Size = new System.Drawing.Size(806, 4);
			this.splitContent.TabIndex = 2;
			this.splitContent.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panel1.Controls.Add(this.panel5);
			this.panel1.Controls.Add(this.panelControls);
			this.panel1.Controls.Add(this.panelToolbar);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(806, 398);
			this.panel1.TabIndex = 3;
			// 
			// panel5
			// 
			this.panel5.Controls.Add(this.panelInnerDisplay);
			this.panel5.Controls.Add(this.panelGlyphEdit);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel5.Location = new System.Drawing.Point(0, 25);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(806, 347);
			this.panel5.TabIndex = 0;
			// 
			// panelInnerDisplay
			// 
			this.panelInnerDisplay.Controls.Add(this.panelTextures);
			this.panelInnerDisplay.Controls.Add(this.panelHorzScroll);
			this.panelInnerDisplay.Controls.Add(this.panelVertScroll);
			this.panelInnerDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelInnerDisplay.Location = new System.Drawing.Point(0, 0);
			this.panelInnerDisplay.Name = "panelInnerDisplay";
			this.panelInnerDisplay.Size = new System.Drawing.Size(646, 347);
			this.panelInnerDisplay.TabIndex = 0;
			// 
			// panelHorzScroll
			// 
			this.panelHorzScroll.Controls.Add(this.scrollHorizontal);
			this.panelHorzScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelHorzScroll.Location = new System.Drawing.Point(0, 331);
			this.panelHorzScroll.Name = "panelHorzScroll";
			this.panelHorzScroll.Size = new System.Drawing.Size(630, 16);
			this.panelHorzScroll.TabIndex = 2;
			this.panelHorzScroll.Visible = false;
			// 
			// scrollHorizontal
			// 
			this.scrollHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scrollHorizontal.LargeChange = 100;
			this.scrollHorizontal.Location = new System.Drawing.Point(0, 0);
			this.scrollHorizontal.Name = "scrollHorizontal";
			this.scrollHorizontal.Size = new System.Drawing.Size(630, 16);
			this.scrollHorizontal.SmallChange = 10;
			this.scrollHorizontal.TabIndex = 0;
			this.scrollHorizontal.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrollVertical_Scroll);
			this.scrollHorizontal.ValueChanged += new System.EventHandler(this.ClipperOffsetScroll);
			// 
			// panelVertScroll
			// 
			this.panelVertScroll.BackColor = System.Drawing.SystemColors.Control;
			this.panelVertScroll.Controls.Add(this.scrollVertical);
			this.panelVertScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelVertScroll.Location = new System.Drawing.Point(630, 0);
			this.panelVertScroll.Name = "panelVertScroll";
			this.panelVertScroll.Size = new System.Drawing.Size(16, 347);
			this.panelVertScroll.TabIndex = 1;
			this.panelVertScroll.Visible = false;
			// 
			// scrollVertical
			// 
			this.scrollVertical.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.scrollVertical.LargeChange = 100;
			this.scrollVertical.Location = new System.Drawing.Point(0, 0);
			this.scrollVertical.Name = "scrollVertical";
			this.scrollVertical.Size = new System.Drawing.Size(17, 331);
			this.scrollVertical.SmallChange = 10;
			this.scrollVertical.TabIndex = 0;
			this.scrollVertical.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrollVertical_Scroll);
			this.scrollVertical.ValueChanged += new System.EventHandler(this.ClipperOffsetScroll);
			// 
			// panelControls
			// 
			this.panelControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.panelControls.Controls.Add(this.stripFontDisplay);
			this.panelControls.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelControls.Location = new System.Drawing.Point(0, 372);
			this.panelControls.Name = "panelControls";
			this.panelControls.Size = new System.Drawing.Size(806, 26);
			this.panelControls.TabIndex = 0;
			// 
			// stripFontDisplay
			// 
			this.stripFontDisplay.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripFontDisplay.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonPrevTexture,
            this.labelTextureCount,
            this.buttonNextTexture,
            this.toolStripSeparator2,
            this.dropDownZoom,
            this.toolStripSeparator3,
            this.labelSelectedGlyphInfo,
            this.separatorGlyphInfo,
            this.labelHoverGlyphInfo});
			this.stripFontDisplay.Location = new System.Drawing.Point(0, 0);
			this.stripFontDisplay.Name = "stripFontDisplay";
			this.stripFontDisplay.Size = new System.Drawing.Size(806, 25);
			this.stripFontDisplay.Stretch = true;
			this.stripFontDisplay.TabIndex = 0;
			this.stripFontDisplay.Text = "toolStrip1";
			// 
			// buttonPrevTexture
			// 
			this.buttonPrevTexture.AutoToolTip = false;
			this.buttonPrevTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.buttonPrevTexture.Font = new System.Drawing.Font("Marlett", 9F);
			this.buttonPrevTexture.Image = ((System.Drawing.Image)(resources.GetObject("buttonPrevTexture.Image")));
			this.buttonPrevTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPrevTexture.Name = "buttonPrevTexture";
			this.buttonPrevTexture.Size = new System.Drawing.Size(23, 22);
			this.buttonPrevTexture.Text = "3";
			this.buttonPrevTexture.Click += new System.EventHandler(this.buttonPrevTexture_Click);
			// 
			// labelTextureCount
			// 
			this.labelTextureCount.Name = "labelTextureCount";
			this.labelTextureCount.Size = new System.Drawing.Size(71, 22);
			this.labelTextureCount.Text = "Texture N/A";
			// 
			// buttonNextTexture
			// 
			this.buttonNextTexture.AutoToolTip = false;
			this.buttonNextTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.buttonNextTexture.Font = new System.Drawing.Font("Marlett", 9F);
			this.buttonNextTexture.Image = ((System.Drawing.Image)(resources.GetObject("buttonNextTexture.Image")));
			this.buttonNextTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNextTexture.Name = "buttonNextTexture";
			this.buttonNextTexture.Size = new System.Drawing.Size(23, 22);
			this.buttonNextTexture.Text = "4";
			this.buttonNextTexture.Click += new System.EventHandler(this.buttonNextTexture_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// dropDownZoom
			// 
			this.dropDownZoom.AutoToolTip = false;
			this.dropDownZoom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem1600,
            this.menuItem800,
            this.menuItem400,
            this.menuItem200,
            this.menuItem100,
            this.menuItem75,
            this.menuItem50,
            this.menuItem25,
            this.toolStripSeparator1,
            this.menuItemToWindow});
			this.dropDownZoom.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.zoom_16x16;
			this.dropDownZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.dropDownZoom.Name = "dropDownZoom";
			this.dropDownZoom.Size = new System.Drawing.Size(135, 22);
			this.dropDownZoom.Text = "Zoom: To Window";
			// 
			// menuItem1600
			// 
			this.menuItem1600.CheckOnClick = true;
			this.menuItem1600.Name = "menuItem1600";
			this.menuItem1600.Size = new System.Drawing.Size(135, 22);
			this.menuItem1600.Tag = "16";
			this.menuItem1600.Text = "1600%";
			this.menuItem1600.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem1600.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem800
			// 
			this.menuItem800.CheckOnClick = true;
			this.menuItem800.Name = "menuItem800";
			this.menuItem800.Size = new System.Drawing.Size(135, 22);
			this.menuItem800.Tag = "8";
			this.menuItem800.Text = "800%";
			this.menuItem800.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem800.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem400
			// 
			this.menuItem400.CheckOnClick = true;
			this.menuItem400.Name = "menuItem400";
			this.menuItem400.Size = new System.Drawing.Size(135, 22);
			this.menuItem400.Tag = "4";
			this.menuItem400.Text = "400%";
			this.menuItem400.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem400.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem200
			// 
			this.menuItem200.CheckOnClick = true;
			this.menuItem200.Name = "menuItem200";
			this.menuItem200.Size = new System.Drawing.Size(135, 22);
			this.menuItem200.Tag = "2";
			this.menuItem200.Text = "200%";
			this.menuItem200.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem200.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem100
			// 
			this.menuItem100.CheckOnClick = true;
			this.menuItem100.Name = "menuItem100";
			this.menuItem100.Size = new System.Drawing.Size(135, 22);
			this.menuItem100.Tag = "1";
			this.menuItem100.Text = "100%";
			this.menuItem100.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem100.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem75
			// 
			this.menuItem75.CheckOnClick = true;
			this.menuItem75.Name = "menuItem75";
			this.menuItem75.Size = new System.Drawing.Size(135, 22);
			this.menuItem75.Tag = "0.75";
			this.menuItem75.Text = "75%";
			this.menuItem75.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem50
			// 
			this.menuItem50.CheckOnClick = true;
			this.menuItem50.Name = "menuItem50";
			this.menuItem50.Size = new System.Drawing.Size(135, 22);
			this.menuItem50.Tag = "0.5";
			this.menuItem50.Text = "50%";
			this.menuItem50.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem25
			// 
			this.menuItem25.CheckOnClick = true;
			this.menuItem25.Name = "menuItem25";
			this.menuItem25.Size = new System.Drawing.Size(135, 22);
			this.menuItem25.Tag = "0.25";
			this.menuItem25.Text = "25%";
			this.menuItem25.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(132, 6);
			this.toolStripSeparator1.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItemToWindow
			// 
			this.menuItemToWindow.Checked = true;
			this.menuItemToWindow.CheckOnClick = true;
			this.menuItemToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItemToWindow.Name = "menuItemToWindow";
			this.menuItemToWindow.Size = new System.Drawing.Size(135, 22);
			this.menuItemToWindow.Tag = "-1";
			this.menuItemToWindow.Text = "To Window";
			this.menuItemToWindow.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItemToWindow.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// labelSelectedGlyphInfo
			// 
			this.labelSelectedGlyphInfo.Name = "labelSelectedGlyphInfo";
			this.labelSelectedGlyphInfo.Size = new System.Drawing.Size(10, 22);
			this.labelSelectedGlyphInfo.Text = " ";
			// 
			// separatorGlyphInfo
			// 
			this.separatorGlyphInfo.Name = "separatorGlyphInfo";
			this.separatorGlyphInfo.Size = new System.Drawing.Size(6, 25);
			this.separatorGlyphInfo.Visible = false;
			// 
			// labelHoverGlyphInfo
			// 
			this.labelHoverGlyphInfo.Name = "labelHoverGlyphInfo";
			this.labelHoverGlyphInfo.Size = new System.Drawing.Size(13, 22);
			this.labelHoverGlyphInfo.Text = "  ";
			// 
			// panelToolbar
			// 
			this.panelToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.panelToolbar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelToolbar.Controls.Add(this.stripGlyphs);
			this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelToolbar.Location = new System.Drawing.Point(0, 0);
			this.panelToolbar.Name = "panelToolbar";
			this.panelToolbar.Size = new System.Drawing.Size(806, 25);
			this.panelToolbar.TabIndex = 3;
			// 
			// stripGlyphs
			// 
			this.stripGlyphs.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripGlyphs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonEditGlyph,
            this.sepGlyphSpacing,
            this.buttonGlyphKern,
            this.buttonGlyphTools});
			this.stripGlyphs.Location = new System.Drawing.Point(0, 0);
			this.stripGlyphs.Name = "stripGlyphs";
			this.stripGlyphs.Size = new System.Drawing.Size(804, 25);
			this.stripGlyphs.Stretch = true;
			this.stripGlyphs.TabIndex = 0;
			this.stripGlyphs.Text = "GlyphEditing";
			// 
			// buttonEditGlyph
			// 
			this.buttonEditGlyph.AutoToolTip = false;
			this.buttonEditGlyph.CheckOnClick = true;
			this.buttonEditGlyph.Enabled = false;
			this.buttonEditGlyph.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.edit_16x16;
			this.buttonEditGlyph.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditGlyph.Name = "buttonEditGlyph";
			this.buttonEditGlyph.Size = new System.Drawing.Size(81, 22);
			this.buttonEditGlyph.Text = "&Edit Glyph";
			this.buttonEditGlyph.Click += new System.EventHandler(this.buttonEditGlyph_Click);
			// 
			// sepGlyphSpacing
			// 
			this.sepGlyphSpacing.Name = "sepGlyphSpacing";
			this.sepGlyphSpacing.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonGlyphKern
			// 
			this.buttonGlyphKern.CheckOnClick = true;
			this.buttonGlyphKern.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonGlyphKern.Enabled = false;
			this.buttonGlyphKern.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.glyph_kerning_16x16;
			this.buttonGlyphKern.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGlyphKern.Name = "buttonGlyphKern";
			this.buttonGlyphKern.Size = new System.Drawing.Size(23, 22);
			this.buttonGlyphKern.Text = "Sets the kerning values for the selected glyph.";
			this.buttonGlyphKern.Click += new System.EventHandler(this.buttonGlyphKern_Click);
			// 
			// buttonGlyphTools
			// 
			this.buttonGlyphTools.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonGlyphTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSetGlyph,
            this.toolStripSeparator8,
            this.menuItemLoadGlyphImage,
            this.menuItemRemoveGlyphImage});
			this.buttonGlyphTools.Enabled = false;
			this.buttonGlyphTools.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.edit_16x16;
			this.buttonGlyphTools.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGlyphTools.Name = "buttonGlyphTools";
			this.buttonGlyphTools.Size = new System.Drawing.Size(32, 22);
			this.buttonGlyphTools.Text = "Glyph tools";
			this.buttonGlyphTools.ToolTipText = "Glyph tools.";
			this.buttonGlyphTools.ButtonClick += new System.EventHandler(this.buttonGlyphTools_ButtonClick);
			// 
			// menuItemSetGlyph
			// 
			this.menuItemSetGlyph.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.menuItemSetGlyph.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.glyph_select_16x16;
			this.menuItemSetGlyph.Name = "menuItemSetGlyph";
			this.menuItemSetGlyph.Size = new System.Drawing.Size(224, 22);
			this.menuItemSetGlyph.Text = "&Set glyph";
			this.menuItemSetGlyph.Click += new System.EventHandler(this.menuItemSetGlyph_Click);
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(221, 6);
			// 
			// menuItemLoadGlyphImage
			// 
			this.menuItemLoadGlyphImage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.menuItemLoadGlyphImage.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.open_image_16x16;
			this.menuItemLoadGlyphImage.Name = "menuItemLoadGlyphImage";
			this.menuItemLoadGlyphImage.Size = new System.Drawing.Size(224, 22);
			this.menuItemLoadGlyphImage.Text = "&Load image for glyph...";
			this.menuItemLoadGlyphImage.Click += new System.EventHandler(this.menuItemLoadGlyphImage_Click);
			// 
			// menuItemRemoveGlyphImage
			// 
			this.menuItemRemoveGlyphImage.Enabled = false;
			this.menuItemRemoveGlyphImage.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.remove_image_16x16;
			this.menuItemRemoveGlyphImage.Name = "menuItemRemoveGlyphImage";
			this.menuItemRemoveGlyphImage.Size = new System.Drawing.Size(224, 22);
			this.menuItemRemoveGlyphImage.Text = "Remove image from glyph...";
			this.menuItemRemoveGlyphImage.Click += new System.EventHandler(this.menuItemRemoveGlyphImage_Click);
			// 
			// stripCommands
			// 
			this.stripCommands.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuTextColor,
            this.menuShadow,
            this.toolStripSeparator5,
            this.toolStripLabel1,
            this.textPreviewText});
			this.stripCommands.Location = new System.Drawing.Point(0, 0);
			this.stripCommands.Name = "stripCommands";
			this.stripCommands.Size = new System.Drawing.Size(806, 25);
			this.stripCommands.Stretch = true;
			this.stripCommands.TabIndex = 0;
			this.stripCommands.Text = "toolStrip1";
			// 
			// menuTextColor
			// 
			this.menuTextColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.menuTextColor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemSampleTextForeground,
            this.itemSampleTextBackground});
			this.menuTextColor.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Color;
			this.menuTextColor.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.menuTextColor.Name = "menuTextColor";
			this.menuTextColor.Size = new System.Drawing.Size(29, 22);
			this.menuTextColor.Text = "Sets the text display colors.";
			// 
			// itemSampleTextForeground
			// 
			this.itemSampleTextForeground.Name = "itemSampleTextForeground";
			this.itemSampleTextForeground.Size = new System.Drawing.Size(179, 22);
			this.itemSampleTextForeground.Text = "&Foreground Color...";
			this.itemSampleTextForeground.Click += new System.EventHandler(this.buttonTextColor_Click);
			// 
			// itemSampleTextBackground
			// 
			this.itemSampleTextBackground.Name = "itemSampleTextBackground";
			this.itemSampleTextBackground.Size = new System.Drawing.Size(179, 22);
			this.itemSampleTextBackground.Text = "&Background Color...";
			this.itemSampleTextBackground.Click += new System.EventHandler(this.itemSampleTextBackground_Click);
			// 
			// menuShadow
			// 
			this.menuShadow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.menuShadow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemPreviewShadowEnable,
            this.toolStripSeparator4,
            this.itemShadowOpacity,
            this.itemShadowOffset});
			this.menuShadow.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.shadow_16x16;
			this.menuShadow.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.menuShadow.Name = "menuShadow";
			this.menuShadow.Size = new System.Drawing.Size(29, 22);
			this.menuShadow.Text = "Sets display text shadowing options.";
			// 
			// itemPreviewShadowEnable
			// 
			this.itemPreviewShadowEnable.CheckOnClick = true;
			this.itemPreviewShadowEnable.Name = "itemPreviewShadowEnable";
			this.itemPreviewShadowEnable.Size = new System.Drawing.Size(179, 22);
			this.itemPreviewShadowEnable.Text = "&Enable Text Shadow";
			this.itemPreviewShadowEnable.Click += new System.EventHandler(this.itemPreviewShadowEnable_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(176, 6);
			// 
			// itemShadowOpacity
			// 
			this.itemShadowOpacity.Enabled = false;
			this.itemShadowOpacity.Name = "itemShadowOpacity";
			this.itemShadowOpacity.Size = new System.Drawing.Size(179, 22);
			this.itemShadowOpacity.Text = "Shadow &Opacity...";
			this.itemShadowOpacity.Click += new System.EventHandler(this.itemShadowOpacity_Click);
			// 
			// itemShadowOffset
			// 
			this.itemShadowOffset.Enabled = false;
			this.itemShadowOffset.Name = "itemShadowOffset";
			this.itemShadowOffset.Size = new System.Drawing.Size(179, 22);
			this.itemShadowOffset.Text = "Shadow O&ffset...";
			this.itemShadowOffset.Click += new System.EventHandler(this.itemShadowOffset_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(76, 22);
			this.toolStripLabel1.Text = "Preview Text:";
			// 
			// textPreviewText
			// 
			this.textPreviewText.AcceptsReturn = true;
			this.textPreviewText.AutoToolTip = true;
			this.textPreviewText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textPreviewText.MaxLength = 4096;
			this.textPreviewText.Name = "textPreviewText";
			this.textPreviewText.Size = new System.Drawing.Size(256, 25);
			this.textPreviewText.TextChanged += new System.EventHandler(this.textPreviewText_TextChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.panelText);
			this.panel2.Controls.Add(this.stripCommands);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 398);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(806, 208);
			this.panel2.TabIndex = 4;
			// 
			// imageFileBrowser
			// 
			this.imageFileBrowser.DefaultExtension = "png";
			this.imageFileBrowser.Filename = null;
			this.imageFileBrowser.StartDirectory = null;
			this.imageFileBrowser.Text = "Open Image";
			// 
			// GorgonFontContentPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "GorgonFontContentPanel";
			this.Size = new System.Drawing.Size(806, 636);
			this.Text = "Gorgon Font";
			this.Load += new System.EventHandler(this.GorgonFontContentPanel_Load);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GorgonFontContentPanel_MouseClick);
			this.PanelDisplay.ResumeLayout(false);
			this.panelGlyphEdit.ResumeLayout(false);
			this.panelKerningPairs.ResumeLayout(false);
			this.panelKerningPairs.PerformLayout();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericKerningOffset)).EndInit();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panelGlyphAdvance.ResumeLayout(false);
			this.panelGlyphAdvance.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphAdvance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericOffsetY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericOffsetX)).EndInit();
			this.panelGlyphClip.ResumeLayout(false);
			this.panelGlyphClip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomAmount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomWindowSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphTop)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphLeft)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.panelInnerDisplay.ResumeLayout(false);
			this.panelHorzScroll.ResumeLayout(false);
			this.panelVertScroll.ResumeLayout(false);
			this.panelControls.ResumeLayout(false);
			this.panelControls.PerformLayout();
			this.stripFontDisplay.ResumeLayout(false);
			this.stripFontDisplay.PerformLayout();
			this.panelToolbar.ResumeLayout(false);
			this.panelToolbar.PerformLayout();
			this.stripGlyphs.ResumeLayout(false);
			this.stripGlyphs.PerformLayout();
			this.stripCommands.ResumeLayout(false);
			this.stripCommands.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Splitter splitContent;
		private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelControls;
		internal GorgonLibrary.UI.GorgonSelectablePanel panelTextures;
        internal System.Windows.Forms.Panel panelText;
        private System.Windows.Forms.Panel panelToolbar;
		private System.Windows.Forms.ToolStrip stripFontDisplay;
        private System.Windows.Forms.ToolStripButton buttonPrevTexture;
        private System.Windows.Forms.ToolStripLabel labelTextureCount;
        private System.Windows.Forms.ToolStripButton buttonNextTexture;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel labelSelectedGlyphInfo;
		private System.Windows.Forms.ToolStrip stripCommands;
        private System.Windows.Forms.ToolStripLabel labelHoverGlyphInfo;
		private System.Windows.Forms.ToolStripSeparator separatorGlyphInfo;
		private System.Windows.Forms.ToolStripDropDownButton menuTextColor;
		private System.Windows.Forms.ToolStripMenuItem itemSampleTextForeground;
		private System.Windows.Forms.ToolStripMenuItem itemSampleTextBackground;
		private System.Windows.Forms.ToolStripDropDownButton menuShadow;
		private System.Windows.Forms.ToolStripMenuItem itemPreviewShadowEnable;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem itemShadowOpacity;
		private System.Windows.Forms.ToolStripMenuItem itemShadowOffset;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripTextBox textPreviewText;
		private System.Windows.Forms.ToolStrip stripGlyphs;
		private System.Windows.Forms.ToolStripButton buttonEditGlyph;
		private System.Windows.Forms.ToolStripButton buttonGlyphKern;
		private System.Windows.Forms.ToolStripSeparator sepGlyphSpacing;
		private EditorFileBrowser imageFileBrowser;
		private System.Windows.Forms.ToolStripDropDownButton dropDownZoom;
		private System.Windows.Forms.ToolStripMenuItem menuItem1600;
		private System.Windows.Forms.ToolStripMenuItem menuItem800;
		private System.Windows.Forms.ToolStripMenuItem menuItem400;
		private System.Windows.Forms.ToolStripMenuItem menuItem200;
		private System.Windows.Forms.ToolStripMenuItem menuItem100;
		private System.Windows.Forms.ToolStripMenuItem menuItem75;
		private System.Windows.Forms.ToolStripMenuItem menuItem50;
		private System.Windows.Forms.ToolStripMenuItem menuItem25;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem menuItemToWindow;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSplitButton buttonGlyphTools;
		private System.Windows.Forms.ToolStripMenuItem menuItemSetGlyph;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripMenuItem menuItemLoadGlyphImage;
        private System.Windows.Forms.Panel panelGlyphEdit;
		private System.Windows.Forms.Panel panelGlyphClip;
		private System.Windows.Forms.NumericUpDown numericGlyphHeight;
		private System.Windows.Forms.NumericUpDown numericGlyphWidth;
		private System.Windows.Forms.NumericUpDown numericGlyphTop;
		private System.Windows.Forms.NumericUpDown numericGlyphLeft;
		private System.Windows.Forms.Label labelGlyphHeight;
		private System.Windows.Forms.Label labelGlyphWidth;
		private System.Windows.Forms.Label labelGlyphTop;
        private System.Windows.Forms.Label labelGlyphLeft;
		private System.Windows.Forms.Button buttonGlyphClipOK;
		private System.Windows.Forms.Button buttonGlyphClipCancel;
        private System.Windows.Forms.ToolStripMenuItem menuItemRemoveGlyphImage;
        private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panelInnerDisplay;
		private System.Windows.Forms.Panel panelVertScroll;
		private System.Windows.Forms.VScrollBar scrollVertical;
		private System.Windows.Forms.Panel panelHorzScroll;
		private System.Windows.Forms.HScrollBar scrollHorizontal;
		private System.Windows.Forms.NumericUpDown numericZoomWindowSize;
		private System.Windows.Forms.Label labelZoomWindowSize;
		private System.Windows.Forms.NumericUpDown numericZoomAmount;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkZoomSnap;
		private System.Windows.Forms.Panel panelGlyphAdvance;
		private System.Windows.Forms.NumericUpDown numericGlyphAdvance;
		private System.Windows.Forms.NumericUpDown numericOffsetY;
		private System.Windows.Forms.NumericUpDown numericOffsetX;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button buttonResetGlyphOffset;
		private System.Windows.Forms.ToolTip tipButtons;
		private System.Windows.Forms.Button buttonResetGlyphAdvance;
        private System.Windows.Forms.Panel panelKerningPairs;
        private System.Windows.Forms.ComboBox comboSecondGlyph;
        private System.Windows.Forms.Button buttonKernCancel;
        private System.Windows.Forms.Button buttonKernOK;
        private System.Windows.Forms.NumericUpDown numericKerningOffset;
        private System.Windows.Forms.Label labelKerningOffset;
        private System.Windows.Forms.Label labelSecondaryGlyph;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Panel panel3;
    }
}
