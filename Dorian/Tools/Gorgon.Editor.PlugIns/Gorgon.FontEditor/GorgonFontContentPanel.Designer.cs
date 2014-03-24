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
	            if (_zoomFont != null)
	            {
					_zoomFont.Dispose();
		            _zoomFont = null;
	            }

				this.MouseWheel -= PanelDisplay_MouseWheel;

	            if (_rawKeyboard != null)
	            {
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

	            if (_editGlyph != null)
	            {
		            _editGlyph.Dispose();
	            }

	            _rawMouse = null;
	            _rawKeyboard = null;
	            _editGlyph = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GorgonFontContentPanel));
			this.panelTextures = new GorgonLibrary.UI.GorgonSelectablePanel();
			this.panelGlyphSet = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.numericGlyphHeight = new System.Windows.Forms.NumericUpDown();
			this.numericGlyphWidth = new System.Windows.Forms.NumericUpDown();
			this.numericGlyphTop = new System.Windows.Forms.NumericUpDown();
			this.numericGlyphLeft = new System.Windows.Forms.NumericUpDown();
			this.labelGlyphHeight = new System.Windows.Forms.Label();
			this.labelGlyphWidth = new System.Windows.Forms.Label();
			this.labelGlyphTop = new System.Windows.Forms.Label();
			this.labelGlyphLeft = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.buttonGlyphClipOK = new System.Windows.Forms.Button();
			this.buttonGlyphClipCancel = new System.Windows.Forms.Button();
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
			this.buttonGlyphSizeSpace = new System.Windows.Forms.ToolStripButton();
			this.buttonGlyphKern = new System.Windows.Forms.ToolStripButton();
			this.sepGlyphTools = new System.Windows.Forms.ToolStripSeparator();
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
			this.PanelDisplay.SuspendLayout();
			this.panelGlyphSet.SuspendLayout();
			this.panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphTop)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphLeft)).BeginInit();
			this.panel3.SuspendLayout();
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
			this.panelTextures.Size = new System.Drawing.Size(790, 285);
			this.panelTextures.TabIndex = 0;
			this.panelTextures.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GorgonFontContentPanel_MouseClick);
			this.panelTextures.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseDoubleClick);
			this.panelTextures.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseDown);
			this.panelTextures.MouseEnter += new System.EventHandler(this.panelTextures_MouseEnter);
			this.panelTextures.MouseLeave += new System.EventHandler(this.panelTextures_MouseLeave);
			this.panelTextures.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseMove);
			this.panelTextures.Resize += new System.EventHandler(this.TextureDisplayResize);
			// 
			// panelGlyphSet
			// 
			this.panelGlyphSet.Controls.Add(this.panel4);
			this.panelGlyphSet.Controls.Add(this.panel3);
			this.panelGlyphSet.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelGlyphSet.Location = new System.Drawing.Point(0, 301);
			this.panelGlyphSet.Name = "panelGlyphSet";
			this.panelGlyphSet.Size = new System.Drawing.Size(806, 46);
			this.panelGlyphSet.TabIndex = 4;
			this.panelGlyphSet.Visible = false;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.numericGlyphHeight);
			this.panel4.Controls.Add(this.numericGlyphWidth);
			this.panel4.Controls.Add(this.numericGlyphTop);
			this.panel4.Controls.Add(this.numericGlyphLeft);
			this.panel4.Controls.Add(this.labelGlyphHeight);
			this.panel4.Controls.Add(this.labelGlyphWidth);
			this.panel4.Controls.Add(this.labelGlyphTop);
			this.panel4.Controls.Add(this.labelGlyphLeft);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new System.Drawing.Point(0, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(735, 46);
			this.panel4.TabIndex = 7;
			// 
			// numericGlyphHeight
			// 
			this.numericGlyphHeight.Location = new System.Drawing.Point(257, 18);
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
			// 
			// numericGlyphWidth
			// 
			this.numericGlyphWidth.Location = new System.Drawing.Point(183, 18);
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
			// 
			// numericGlyphTop
			// 
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
			// 
			// numericGlyphLeft
			// 
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
			// 
			// labelGlyphHeight
			// 
			this.labelGlyphHeight.AutoSize = true;
			this.labelGlyphHeight.Location = new System.Drawing.Point(254, 0);
			this.labelGlyphHeight.Name = "labelGlyphHeight";
			this.labelGlyphHeight.Size = new System.Drawing.Size(46, 15);
			this.labelGlyphHeight.TabIndex = 3;
			this.labelGlyphHeight.Text = "Height:";
			// 
			// labelGlyphWidth
			// 
			this.labelGlyphWidth.AutoSize = true;
			this.labelGlyphWidth.Location = new System.Drawing.Point(180, 0);
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
			// panel3
			// 
			this.panel3.Controls.Add(this.buttonGlyphClipOK);
			this.panel3.Controls.Add(this.buttonGlyphClipCancel);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel3.Location = new System.Drawing.Point(735, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(71, 46);
			this.panel3.TabIndex = 8;
			// 
			// buttonGlyphClipOK
			// 
			this.buttonGlyphClipOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonGlyphClipOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonGlyphClipOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonGlyphClipOK.ForeColor = System.Drawing.Color.White;
			this.buttonGlyphClipOK.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonGlyphClipOK.Location = new System.Drawing.Point(6, 13);
			this.buttonGlyphClipOK.Name = "buttonGlyphClipOK";
			this.buttonGlyphClipOK.Size = new System.Drawing.Size(28, 28);
			this.buttonGlyphClipOK.TabIndex = 1;
			this.buttonGlyphClipOK.UseVisualStyleBackColor = false;
			this.buttonGlyphClipOK.Click += new System.EventHandler(this.buttonGlyphClipOK_Click);
			// 
			// buttonGlyphClipCancel
			// 
			this.buttonGlyphClipCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonGlyphClipCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonGlyphClipCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonGlyphClipCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonGlyphClipCancel.ForeColor = System.Drawing.Color.White;
			this.buttonGlyphClipCancel.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonGlyphClipCancel.Location = new System.Drawing.Point(40, 13);
			this.buttonGlyphClipCancel.Name = "buttonGlyphClipCancel";
			this.buttonGlyphClipCancel.Size = new System.Drawing.Size(28, 28);
			this.buttonGlyphClipCancel.TabIndex = 4;
			this.buttonGlyphClipCancel.UseVisualStyleBackColor = false;
			this.buttonGlyphClipCancel.Click += new System.EventHandler(this.buttonGlyphClipCancel_Click);
			// 
			// panelText
			// 
			this.panelText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panelText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelText.ForeColor = System.Drawing.Color.Black;
			this.panelText.Location = new System.Drawing.Point(0, 25);
			this.panelText.Name = "panelText";
			this.panelText.Size = new System.Drawing.Size(806, 183);
			this.panelText.TabIndex = 0;
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
			this.panel5.Controls.Add(this.panelGlyphSet);
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
			this.panelInnerDisplay.Size = new System.Drawing.Size(806, 301);
			this.panelInnerDisplay.TabIndex = 0;
			// 
			// panelHorzScroll
			// 
			this.panelHorzScroll.Controls.Add(this.scrollHorizontal);
			this.panelHorzScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelHorzScroll.Location = new System.Drawing.Point(0, 285);
			this.panelHorzScroll.Name = "panelHorzScroll";
			this.panelHorzScroll.Size = new System.Drawing.Size(790, 16);
			this.panelHorzScroll.TabIndex = 2;
			this.panelHorzScroll.Visible = false;
			// 
			// scrollHorizontal
			// 
			this.scrollHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scrollHorizontal.LargeChange = 100;
			this.scrollHorizontal.Location = new System.Drawing.Point(0, 0);
			this.scrollHorizontal.Name = "scrollHorizontal";
			this.scrollHorizontal.Size = new System.Drawing.Size(790, 16);
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
			this.panelVertScroll.Location = new System.Drawing.Point(790, 0);
			this.panelVertScroll.Name = "panelVertScroll";
			this.panelVertScroll.Size = new System.Drawing.Size(16, 301);
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
			this.scrollVertical.Size = new System.Drawing.Size(17, 285);
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
			this.stripFontDisplay.TabIndex = 4;
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
            this.buttonGlyphSizeSpace,
            this.buttonGlyphKern,
            this.sepGlyphTools,
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
			// buttonGlyphSizeSpace
			// 
			this.buttonGlyphSizeSpace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonGlyphSizeSpace.Enabled = false;
			this.buttonGlyphSizeSpace.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.glyph_sizing_16x16;
			this.buttonGlyphSizeSpace.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGlyphSizeSpace.Name = "buttonGlyphSizeSpace";
			this.buttonGlyphSizeSpace.Size = new System.Drawing.Size(23, 22);
			this.buttonGlyphSizeSpace.Text = "Sets the advancement placement values for the selected glyph.";
			// 
			// buttonGlyphKern
			// 
			this.buttonGlyphKern.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonGlyphKern.Enabled = false;
			this.buttonGlyphKern.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.glyph_kerning_16x16;
			this.buttonGlyphKern.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGlyphKern.Name = "buttonGlyphKern";
			this.buttonGlyphKern.Size = new System.Drawing.Size(23, 22);
			this.buttonGlyphKern.Text = "Sets the kerning values for the selected glyph.";
			// 
			// sepGlyphTools
			// 
			this.sepGlyphTools.Name = "sepGlyphTools";
			this.sepGlyphTools.Size = new System.Drawing.Size(6, 25);
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
			this.panelGlyphSet.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphTop)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericGlyphLeft)).EndInit();
			this.panel3.ResumeLayout(false);
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
		private System.Windows.Forms.ToolStripButton buttonGlyphSizeSpace;
		private System.Windows.Forms.ToolStripButton buttonEditGlyph;
		private System.Windows.Forms.ToolStripButton buttonGlyphKern;
		private System.Windows.Forms.ToolStripSeparator sepGlyphSpacing;
		private System.Windows.Forms.ToolStripSeparator sepGlyphTools;
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
        private System.Windows.Forms.Panel panelGlyphSet;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.NumericUpDown numericGlyphHeight;
		private System.Windows.Forms.NumericUpDown numericGlyphWidth;
		private System.Windows.Forms.NumericUpDown numericGlyphTop;
		private System.Windows.Forms.NumericUpDown numericGlyphLeft;
		private System.Windows.Forms.Label labelGlyphHeight;
		private System.Windows.Forms.Label labelGlyphWidth;
		private System.Windows.Forms.Label labelGlyphTop;
		private System.Windows.Forms.Label labelGlyphLeft;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button buttonGlyphClipOK;
		private System.Windows.Forms.Button buttonGlyphClipCancel;
        private System.Windows.Forms.ToolStripMenuItem menuItemRemoveGlyphImage;
        private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panelInnerDisplay;
		private System.Windows.Forms.Panel panelVertScroll;
		private System.Windows.Forms.VScrollBar scrollVertical;
		private System.Windows.Forms.Panel panelHorzScroll;
		private System.Windows.Forms.HScrollBar scrollHorizontal;
    }
}
