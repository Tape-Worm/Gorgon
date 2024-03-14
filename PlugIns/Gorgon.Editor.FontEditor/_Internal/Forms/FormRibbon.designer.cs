namespace Gorgon.Editor.FontEditor;

partial class FormRibbon
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

        if (disposing)
        {
            UnassignEvents();

            if (_contentRenderer != null)
            {
                _contentRenderer.ZoomScaleChanged -= ContentRenderer_ZoomScale;
            }

            ViewModel = null;
        }

        base.Dispose(disposing);
    }



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRibbon));
        this.RibbonTextContent = new Krypton.Ribbon.KryptonRibbon();
        this.ContextTextureEditor = new Krypton.Ribbon.KryptonRibbonContext();
        this.TabFont = new Krypton.Ribbon.KryptonRibbonTab();
        this.GroupImageFile = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple1 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonNew = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSaveFont = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator2 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.GroupTextEdit = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple3 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonOutline = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupLines5 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonTexture = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonCharacterSelection = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator1 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.kryptonRibbonGroupLines2 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonTextUndo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonTextRedo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupFont = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple2 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.LabelFontFamily = new Krypton.Ribbon.KryptonRibbonGroupLabel();
        this.CustomFonts = new Krypton.Ribbon.KryptonRibbonGroupCustomControl();
        this.ComboFonts = new Gorgon.Editor.FontEditor.ComboFonts();
        this.kryptonRibbonGroupSeparator3 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.kryptonRibbonGroupLines1 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.LabelSize = new Krypton.Ribbon.KryptonRibbonGroupLabel();
        this.CustomControlSize = new Krypton.Ribbon.KryptonRibbonGroupCustomControl();
        this.NumericSize = new System.Windows.Forms.NumericUpDown();
        this.RadioPointUnits = new Krypton.Ribbon.KryptonRibbonGroupRadioButton();
        this.RadioPixelUnits = new Krypton.Ribbon.KryptonRibbonGroupRadioButton();
        this.kryptonRibbonGroupLines4 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.CheckBold = new Krypton.Ribbon.KryptonRibbonGroupCheckBox();
        this.CheckItalics = new Krypton.Ribbon.KryptonRibbonGroupCheckBox();
        this.CheckAntiAlias = new Krypton.Ribbon.KryptonRibbonGroupCheckBox();
        this.kryptonRibbonGroupLines3 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonZoom = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuZoom = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.ItemZoomToWindow = new System.Windows.Forms.ToolStripMenuItem();
        this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
        this.Item12Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item25Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item50Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item100Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item200Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item400Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item800Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item1600Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item3200Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.Item6400Percent = new System.Windows.Forms.ToolStripMenuItem();
        this.TabTexture = new Krypton.Ribbon.KryptonRibbonTab();
        this.GroupTexture = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple4 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.kryptonRibbonGroupButton1 = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuBrushes = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.MenuItemSolid = new System.Windows.Forms.ToolStripMenuItem();
        this.MenuItemPattern = new System.Windows.Forms.ToolStripMenuItem();
        this.MenuItemGradient = new System.Windows.Forms.ToolStripMenuItem();
        this.MenuItemTextured = new System.Windows.Forms.ToolStripMenuItem();
        this.kryptonRibbonGroupLines6 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonResizeTexture = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonGlyphPad = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.CheckPremultiply = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupTextureNav = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupLines7 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonPrevTexture = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonNextTexture = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonfirstTexture = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonLastTexture = new Krypton.Ribbon.KryptonRibbonGroupButton();
        ((System.ComponentModel.ISupportInitialize)(this.RibbonTextContent)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericSize)).BeginInit();
        this.MenuZoom.SuspendLayout();
        this.MenuBrushes.SuspendLayout();
        this.SuspendLayout();
        // 
        // RibbonTextContent
        // 
        this.RibbonTextContent.AllowFormIntegrate = true;
        this.RibbonTextContent.InDesignHelperMode = true;
        this.RibbonTextContent.Name = "RibbonTextContent";
        this.RibbonTextContent.RibbonContexts.AddRange(new Krypton.Ribbon.KryptonRibbonContext[] {
        this.ContextTextureEditor});
        this.RibbonTextContent.RibbonTabs.AddRange(new Krypton.Ribbon.KryptonRibbonTab[] {
        this.TabFont,
        this.TabTexture});
        this.RibbonTextContent.SelectedContext = null;
        this.RibbonTextContent.SelectedTab = this.TabFont;
        this.RibbonTextContent.Size = new System.Drawing.Size(1437, 115);
        this.RibbonTextContent.TabIndex = 0;
        // 
        // ContextTextureEditor
        // 
        this.ContextTextureEditor.ContextColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
        this.ContextTextureEditor.ContextName = "TextureEditor";
        this.ContextTextureEditor.ContextTitle = "Texture Tools";
        // 
        // TabFont
        // 
        this.TabFont.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.GroupImageFile,
        this.GroupTextEdit,
        this.GroupFont});
        this.TabFont.KeyTip = "O";
        this.TabFont.Text = "Font";
        // 
        // GroupImageFile
        // 
        this.GroupImageFile.AllowCollapsed = false;
        this.GroupImageFile.DialogBoxLauncher = false;
        this.GroupImageFile.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple1,
        this.kryptonRibbonGroupSeparator2});
        this.GroupImageFile.KeyTipGroup = "F";
        this.GroupImageFile.TextLine1 = "File";
        // 
        // kryptonRibbonGroupTriple1
        // 
        this.kryptonRibbonGroupTriple1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonNew,
        this.ButtonSaveFont});
        this.kryptonRibbonGroupTriple1.MinimumSize = Krypton.Ribbon.GroupItemSize.Large;
        // 
        // ButtonNew
        // 
        this.ButtonNew.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.font_48x48;
        this.ButtonNew.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.font_16x16;
        this.ButtonNew.KeyTip = "N";
        this.ButtonNew.TextLine1 = "New";
        this.ButtonNew.TextLine2 = "Font";
        this.ButtonNew.ToolTipValues.Description = "Creates a new font using the current font settings as a base.";
        this.ButtonNew.ToolTipValues.Heading = "New Font";
        this.ButtonNew.Click += new System.EventHandler(this.ButtonNew_Click);
        // 
        // ButtonSaveFont
        // 
        this.ButtonSaveFont.Enabled = false;
        this.ButtonSaveFont.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.save_content_48x48;
        this.ButtonSaveFont.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.save_content_16x16;
        this.ButtonSaveFont.KeyTip = "S";
        this.ButtonSaveFont.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
        this.ButtonSaveFont.TextLine1 = "Save";
        this.ButtonSaveFont.TextLine2 = "Font";
        this.ButtonSaveFont.ToolTipValues.Description = "Saves the font changes to the file system.";
        this.ButtonSaveFont.ToolTipValues.Heading = "Save";
        this.ButtonSaveFont.Click += new System.EventHandler(this.ButtonSave_Click);
        // 
        // GroupTextEdit
        // 
        this.GroupTextEdit.AllowCollapsed = false;
        this.GroupTextEdit.DialogBoxLauncher = false;
        this.GroupTextEdit.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple3,
        this.kryptonRibbonGroupLines5,
        this.kryptonRibbonGroupSeparator1,
        this.kryptonRibbonGroupLines2});
        this.GroupTextEdit.KeyTipGroup = "E";
        this.GroupTextEdit.TextLine1 = "Edit";
        // 
        // kryptonRibbonGroupTriple3
        // 
        this.kryptonRibbonGroupTriple3.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonOutline});
        // 
        // ButtonOutline
        // 
        this.ButtonOutline.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonOutline.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.outlined_48x48;
        this.ButtonOutline.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.outlined_16x16;
        this.ButtonOutline.KeyTip = "O";
        this.ButtonOutline.TextLine1 = "Outline";
        this.ButtonOutline.ToolTipValues.Description = "Settings for providing an outline around the glyphs of the font.";
        this.ButtonOutline.ToolTipValues.Heading = "Outline";
        this.ButtonOutline.Click += new System.EventHandler(this.ButtonOutline_Click);
        // 
        // kryptonRibbonGroupLines5
        // 
        this.kryptonRibbonGroupLines5.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonTexture,
        this.ButtonCharacterSelection});
        // 
        // ButtonTexture
        // 
        this.ButtonTexture.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonTexture.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.image_dimensions_48x48;
        this.ButtonTexture.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.image_dimensions_16x16;
        this.ButtonTexture.KeyTip = "T";
        this.ButtonTexture.TextLine1 = "Texture";
        this.ButtonTexture.TextLine2 = "Settings";
        this.ButtonTexture.ToolTipValues.Description = "Settings for the backing textures that contain the font glyphs.";
        this.ButtonTexture.ToolTipValues.Heading = "Texture Settings";
        this.ButtonTexture.Click += new System.EventHandler(this.ButtonTexture_Click);
        // 
        // ButtonCharacterSelection
        // 
        this.ButtonCharacterSelection.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.char_list_48x48;
        this.ButtonCharacterSelection.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.char_list_16x16;
        this.ButtonCharacterSelection.KeyTip = "L";
        this.ButtonCharacterSelection.TextLine1 = "Character";
        this.ButtonCharacterSelection.TextLine2 = "List";
        this.ButtonCharacterSelection.ToolTipValues.Description = "Provides the selection of letters, numbers, symbols that can be used as \r\nglyphs " +
"in the font.";
        this.ButtonCharacterSelection.ToolTipValues.Heading = "Character List";
        this.ButtonCharacterSelection.Click += new System.EventHandler(this.ButtonCharacterSelection_Click);
        // 
        // kryptonRibbonGroupLines2
        // 
        this.kryptonRibbonGroupLines2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonTextUndo,
        this.ButtonTextRedo});
        // 
        // ButtonTextUndo
        // 
        this.ButtonTextUndo.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.undo_48x48;
        this.ButtonTextUndo.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.undo_16x16;
        this.ButtonTextUndo.KeyTip = "Z";
        this.ButtonTextUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
        this.ButtonTextUndo.TextLine1 = "Undo";
        this.ButtonTextUndo.ToolTipValues.Description = "Restores the font back to the previous state.";
        this.ButtonTextUndo.ToolTipValues.Heading = "Undo";
        this.ButtonTextUndo.Click += new System.EventHandler(this.ButtonFontUndo_Click);
        // 
        // ButtonTextRedo
        // 
        this.ButtonTextRedo.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.redo_48x48;
        this.ButtonTextRedo.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.redo_16x16;
        this.ButtonTextRedo.KeyTip = "Y";
        this.ButtonTextRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
        this.ButtonTextRedo.TextLine1 = "Redo";
        this.ButtonTextRedo.ToolTipValues.Description = "Resets the font to the next state.";
        this.ButtonTextRedo.ToolTipValues.Heading = "Redo";
        this.ButtonTextRedo.Click += new System.EventHandler(this.ButtonFontRedo_Click);
        // 
        // GroupFont
        // 
        this.GroupFont.AllowCollapsed = false;
        this.GroupFont.DialogBoxLauncher = false;
        this.GroupFont.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple2,
        this.kryptonRibbonGroupSeparator3,
        this.kryptonRibbonGroupLines1,
        this.kryptonRibbonGroupLines4,
        this.kryptonRibbonGroupLines3});
        this.GroupFont.KeyTipGroup = "F";
        this.GroupFont.TextLine1 = "Font";
        // 
        // kryptonRibbonGroupTriple2
        // 
        this.kryptonRibbonGroupTriple2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.LabelFontFamily,
        this.CustomFonts});
        this.kryptonRibbonGroupTriple2.MaximumSize = Krypton.Ribbon.GroupItemSize.Medium;
        this.kryptonRibbonGroupTriple2.MinimumSize = Krypton.Ribbon.GroupItemSize.Medium;
        // 
        // LabelFontFamily
        // 
        this.LabelFontFamily.TextLine1 = "Font";
        this.LabelFontFamily.TextLine2 = "Family:";
        this.LabelFontFamily.ToolTipValues.Description = "The list of True Type fonts that can be used to generate the bitmap font.";
        this.LabelFontFamily.ToolTipValues.Heading = "Font Family";
        // 
        // CustomFonts
        // 
        this.CustomFonts.CustomControl = this.ComboFonts;
        this.CustomFonts.KeyTip = "F";
        // 
        // ComboFonts
        // 
        this.ComboFonts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.ComboFonts.ForeColor = System.Drawing.Color.Black;
        this.ComboFonts.FormattingEnabled = true;
        this.ComboFonts.Location = new System.Drawing.Point(594, 26);
        this.ComboFonts.Name = "ComboFonts";
        this.ComboFonts.Size = new System.Drawing.Size(355, 24);
        this.ComboFonts.TabIndex = 1;
        this.ComboFonts.TabStop = false;
        // 
        // kryptonRibbonGroupLines1
        // 
        this.kryptonRibbonGroupLines1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.LabelSize,
        this.CustomControlSize,
        this.RadioPointUnits,
        this.RadioPixelUnits});
        // 
        // LabelSize
        // 
        this.LabelSize.TextLine1 = "Size:";
        this.LabelSize.ToolTipValues.Description = "The height of the font.\r\n\r\nThe units for the height are assigned via the radio bu" +
"ttons below.";
        this.LabelSize.ToolTipValues.Heading = "Size";
        // 
        // CustomControlSize
        // 
        this.CustomControlSize.CustomControl = this.NumericSize;
        this.CustomControlSize.KeyTip = "0";
        // 
        // NumericSize
        // 
        this.NumericSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericSize.BackColor = System.Drawing.Color.White;
        this.NumericSize.DecimalPlaces = 2;
        this.NumericSize.ForeColor = System.Drawing.Color.Black;
        this.NumericSize.Increment = new decimal(new int[] {
        25,
        0,
        0,
        131072});
        this.NumericSize.Location = new System.Drawing.Point(1012, 11);
        this.NumericSize.Maximum = new decimal(new int[] {
        144,
        0,
        0,
        0});
        this.NumericSize.Minimum = new decimal(new int[] {
        225,
        0,
        0,
        131072});
        this.NumericSize.Name = "NumericSize";
        this.NumericSize.Size = new System.Drawing.Size(56, 23);
        this.NumericSize.TabIndex = 1;
        this.NumericSize.TabStop = false;
        this.NumericSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericSize.Value = new decimal(new int[] {
        9,
        0,
        0,
        0});
        // 
        // RadioPointUnits
        // 
        this.RadioPointUnits.Checked = true;
        this.RadioPointUnits.KeyTip = "P";
        this.RadioPointUnits.TextLine1 = "Points";
        this.RadioPointUnits.ToolTipValues.Description = "Sets the font height units to Points.\r\n\r\nThis is useful for making the font respe" +
"ct the current DPI.";
        this.RadioPointUnits.ToolTipValues.Heading = "Font Height: Points";
        this.RadioPointUnits.CheckedChanged += new System.EventHandler(this.RadioPointUnits_CheckedChanged);
        // 
        // RadioPixelUnits
        // 
        this.RadioPixelUnits.KeyTip = "X";
        this.RadioPixelUnits.TextLine1 = "Pixels";
        this.RadioPixelUnits.ToolTipValues.Description = "Sets the font height units to Pixels.\r\n\r\nThis is useful for making the font fit a" +
"n exact height on the screen.";
        this.RadioPixelUnits.CheckedChanged += new System.EventHandler(this.RadioPixelUnits_CheckedChanged);
        // 
        // kryptonRibbonGroupLines4
        // 
        this.kryptonRibbonGroupLines4.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.CheckBold,
        this.CheckItalics,
        this.CheckAntiAlias});
        // 
        // CheckBold
        // 
        this.CheckBold.KeyTip = "B";
        this.CheckBold.TextLine1 = "Bold";
        this.CheckBold.ToolTipValues.Description = "Sets the glyph style to bold.";
        this.CheckBold.ToolTipValues.Heading = "Bold";
        this.CheckBold.CheckedChanged += new System.EventHandler(this.CheckBold_CheckedChanged);
        // 
        // CheckItalics
        // 
        this.CheckItalics.KeyTip = "I";
        this.CheckItalics.TextLine1 = "Italic";
        this.CheckItalics.ToolTipValues.Description = "Sets the glyph style to italic.";
        this.CheckItalics.CheckedChanged += new System.EventHandler(this.CheckItalics_CheckedChanged);
        // 
        // CheckAntiAlias
        // 
        this.CheckAntiAlias.KeyTip = "A";
        this.CheckAntiAlias.TextLine1 = "Antialiasing";
        this.CheckAntiAlias.ToolTipValues.Description = resources.GetString("CheckAntiAlias.ToolTipValues.Description");
        this.CheckAntiAlias.CheckedChanged += new System.EventHandler(this.CheckAntiAlias_CheckedChanged);
        // 
        // kryptonRibbonGroupLines3
        // 
        this.kryptonRibbonGroupLines3.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonZoom});
        // 
        // ButtonZoom
        // 
        this.ButtonZoom.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.ButtonZoom.ContextMenuStrip = this.MenuZoom;
        this.ButtonZoom.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.zoom_48x48;
        this.ButtonZoom.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.zoom_16x16;
        this.ButtonZoom.KeyTip = "Z";
        this.ButtonZoom.TextLine1 = "Zoom";
        this.ButtonZoom.ToolTipValues.Description = resources.GetString("ButtonZoom.ToolTipValues.Description");
        this.ButtonZoom.ToolTipValues.Heading = "Zoom";
        // 
        // MenuZoom
        // 
        this.MenuZoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MenuZoom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.ItemZoomToWindow,
        this.toolStripMenuItem1,
        this.Item12Percent,
        this.Item25Percent,
        this.Item50Percent,
        this.Item100Percent,
        this.Item200Percent,
        this.Item400Percent,
        this.Item800Percent,
        this.Item1600Percent,
        this.Item3200Percent,
        this.Item6400Percent});
        this.MenuZoom.Name = "ZoomMenu";
        this.MenuZoom.Size = new System.Drawing.Size(134, 252);
        // 
        // ItemZoomToWindow
        // 
        this.ItemZoomToWindow.Checked = true;
        this.ItemZoomToWindow.CheckOnClick = true;
        this.ItemZoomToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ItemZoomToWindow.Name = "ItemZoomToWindow";
        this.ItemZoomToWindow.Size = new System.Drawing.Size(133, 22);
        this.ItemZoomToWindow.Tag = "ToWindow";
        this.ItemZoomToWindow.Text = "To Window";
        this.ItemZoomToWindow.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // toolStripMenuItem1
        // 
        this.toolStripMenuItem1.Name = "toolStripMenuItem1";
        this.toolStripMenuItem1.Size = new System.Drawing.Size(130, 6);
        // 
        // Item12Percent
        // 
        this.Item12Percent.CheckOnClick = true;
        this.Item12Percent.Name = "Item12Percent";
        this.Item12Percent.Size = new System.Drawing.Size(133, 22);
        this.Item12Percent.Tag = "Percent12";
        this.Item12Percent.Text = "12.5%";
        this.Item12Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item25Percent
        // 
        this.Item25Percent.CheckOnClick = true;
        this.Item25Percent.Name = "Item25Percent";
        this.Item25Percent.Size = new System.Drawing.Size(133, 22);
        this.Item25Percent.Tag = "Percent25";
        this.Item25Percent.Text = "25%";
        this.Item25Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item50Percent
        // 
        this.Item50Percent.CheckOnClick = true;
        this.Item50Percent.Name = "Item50Percent";
        this.Item50Percent.Size = new System.Drawing.Size(133, 22);
        this.Item50Percent.Tag = "Percent50";
        this.Item50Percent.Text = "50%";
        this.Item50Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item100Percent
        // 
        this.Item100Percent.CheckOnClick = true;
        this.Item100Percent.Name = "Item100Percent";
        this.Item100Percent.Size = new System.Drawing.Size(133, 22);
        this.Item100Percent.Tag = "Percent100";
        this.Item100Percent.Text = "100%";
        this.Item100Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item200Percent
        // 
        this.Item200Percent.CheckOnClick = true;
        this.Item200Percent.Name = "Item200Percent";
        this.Item200Percent.Size = new System.Drawing.Size(133, 22);
        this.Item200Percent.Tag = "Percent200";
        this.Item200Percent.Text = "200%";
        this.Item200Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item400Percent
        // 
        this.Item400Percent.CheckOnClick = true;
        this.Item400Percent.Name = "Item400Percent";
        this.Item400Percent.Size = new System.Drawing.Size(133, 22);
        this.Item400Percent.Tag = "Percent400";
        this.Item400Percent.Text = "400%";
        this.Item400Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item800Percent
        // 
        this.Item800Percent.CheckOnClick = true;
        this.Item800Percent.Name = "Item800Percent";
        this.Item800Percent.Size = new System.Drawing.Size(133, 22);
        this.Item800Percent.Tag = "Percent800";
        this.Item800Percent.Text = "800%";
        this.Item800Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item1600Percent
        // 
        this.Item1600Percent.CheckOnClick = true;
        this.Item1600Percent.Name = "Item1600Percent";
        this.Item1600Percent.Size = new System.Drawing.Size(133, 22);
        this.Item1600Percent.Tag = "Percent1600";
        this.Item1600Percent.Text = "1600%";
        this.Item1600Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item3200Percent
        // 
        this.Item3200Percent.CheckOnClick = true;
        this.Item3200Percent.Name = "Item3200Percent";
        this.Item3200Percent.Size = new System.Drawing.Size(133, 22);
        this.Item3200Percent.Tag = "Percent3200";
        this.Item3200Percent.Text = "3200%";
        this.Item3200Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // Item6400Percent
        // 
        this.Item6400Percent.CheckOnClick = true;
        this.Item6400Percent.Name = "Item6400Percent";
        this.Item6400Percent.Size = new System.Drawing.Size(133, 22);
        this.Item6400Percent.Tag = "Percent6400";
        this.Item6400Percent.Text = "6400%";
        this.Item6400Percent.Click += new System.EventHandler(this.ItemZoom_Click);
        // 
        // TabTexture
        // 
        this.TabTexture.ContextName = "TextureEditor";
        this.TabTexture.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.GroupTexture,
        this.GroupTextureNav});
        this.TabTexture.Text = "Font Texture";
        // 
        // GroupTexture
        // 
        this.GroupTexture.AllowCollapsed = false;
        this.GroupTexture.DialogBoxLauncher = false;
        this.GroupTexture.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple4,
        this.kryptonRibbonGroupLines6});
        this.GroupTexture.KeyTipGroup = "S";
        this.GroupTexture.TextLine1 = "Texture";
        // 
        // kryptonRibbonGroupTriple4
        // 
        this.kryptonRibbonGroupTriple4.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.kryptonRibbonGroupButton1});
        // 
        // kryptonRibbonGroupButton1
        // 
        this.kryptonRibbonGroupButton1.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.kryptonRibbonGroupButton1.ContextMenuStrip = this.MenuBrushes;
        this.kryptonRibbonGroupButton1.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.sprite_color_48x48;
        this.kryptonRibbonGroupButton1.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.sprite_color_16x16;
        this.kryptonRibbonGroupButton1.TextLine1 = "Change";
        this.kryptonRibbonGroupButton1.TextLine2 = "Brush";
        this.kryptonRibbonGroupButton1.ToolTipValues.Description = resources.GetString("kryptonRibbonGroupButton1.ToolTipValues.Description");
        this.kryptonRibbonGroupButton1.ToolTipValues.Heading = "Change Glyph Brush";
        // 
        // MenuBrushes
        // 
        this.MenuBrushes.BackColor = System.Drawing.Color.White;
        this.MenuBrushes.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MenuBrushes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.MenuItemSolid,
        this.MenuItemPattern,
        this.MenuItemGradient,
        this.MenuItemTextured});
        this.MenuBrushes.Name = "MenuBrushes";
        this.MenuBrushes.Size = new System.Drawing.Size(162, 92);
        // 
        // MenuItemSolid
        // 
        this.MenuItemSolid.Name = "MenuItemSolid";
        this.MenuItemSolid.Size = new System.Drawing.Size(161, 22);
        this.MenuItemSolid.Text = "&Solid Brush...";
        this.MenuItemSolid.Click += new System.EventHandler(this.MenuItemSolid_Click);
        // 
        // MenuItemPattern
        // 
        this.MenuItemPattern.Name = "MenuItemPattern";
        this.MenuItemPattern.Size = new System.Drawing.Size(161, 22);
        this.MenuItemPattern.Text = "&Pattern Brush...";
        this.MenuItemPattern.Click += new System.EventHandler(this.MenuItemPattern_Click);
        // 
        // MenuItemGradient
        // 
        this.MenuItemGradient.Name = "MenuItemGradient";
        this.MenuItemGradient.Size = new System.Drawing.Size(161, 22);
        this.MenuItemGradient.Text = "&Gradient Brush...";
        this.MenuItemGradient.Click += new System.EventHandler(this.MenuItemGradient_Click);
        // 
        // MenuItemTextured
        // 
        this.MenuItemTextured.Name = "MenuItemTextured";
        this.MenuItemTextured.Size = new System.Drawing.Size(161, 22);
        this.MenuItemTextured.Text = "&Textured Brush...";
        this.MenuItemTextured.Click += new System.EventHandler(this.MenuItemTextured_Click);
        // 
        // kryptonRibbonGroupLines6
        // 
        this.kryptonRibbonGroupLines6.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonResizeTexture,
        this.ButtonGlyphPad,
        this.CheckPremultiply});
        // 
        // ButtonResizeTexture
        // 
        this.ButtonResizeTexture.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.resize_all_48x48;
        this.ButtonResizeTexture.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.resize_all_16x16;
        this.ButtonResizeTexture.KeyTip = "R";
        this.ButtonResizeTexture.TextLine1 = "Resize";
        this.ButtonResizeTexture.TextLine2 = "Texture";
        this.ButtonResizeTexture.ToolTipValues.Description = resources.GetString("ButtonResizeTexture.ToolTipValues.Description");
        this.ButtonResizeTexture.ToolTipValues.Heading = "Resize Texture";
        this.ButtonResizeTexture.Click += new System.EventHandler(this.ButtonResizeTexture_Click);
        // 
        // ButtonGlyphPad
        // 
        this.ButtonGlyphPad.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.glyph_pad_48x48;
        this.ButtonGlyphPad.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.glyph_pad_16x16;
        this.ButtonGlyphPad.KeyTip = "P";
        this.ButtonGlyphPad.TextLine1 = "Pad";
        this.ButtonGlyphPad.TextLine2 = "Glyphs";
        this.ButtonGlyphPad.ToolTipValues.Description = "Provides a border around each glyph, in pixels.\r\n\r\nNote: Setting this value to le" +
"ss than 1 may cause issues with glyphs \"bleeding\" into \r\ntheir neighbours when r" +
"endering.";
        this.ButtonGlyphPad.ToolTipValues.Heading = "Pad Glyphs";
        this.ButtonGlyphPad.Click += new System.EventHandler(this.ButtonGlyphPad_Click);
        // 
        // CheckPremultiply
        // 
        this.CheckPremultiply.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.CheckPremultiply.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.alpha_48x48;
        this.CheckPremultiply.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.alpha_16x16;
        this.CheckPremultiply.KeyTip = "M";
        this.CheckPremultiply.TextLine1 = "Premultiply";
        this.CheckPremultiply.TextLine2 = "Alpha";
        this.CheckPremultiply.ToolTipValues.Description = "Multiplies the color of the pixel with the alpha value to use with a premultiplie" +
"d \r\nalpha blending state.";
        this.CheckPremultiply.ToolTipValues.Heading = "Premultiply Alpha";
        this.CheckPremultiply.Click += new System.EventHandler(this.CheckPremultiply_Click);
        // 
        // GroupTextureNav
        // 
        this.GroupTextureNav.AllowCollapsed = false;
        this.GroupTextureNav.DialogBoxLauncher = false;
        this.GroupTextureNav.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupLines7});
        this.GroupTextureNav.KeyTipGroup = "N";
        this.GroupTextureNav.TextLine1 = "Navigation";
        // 
        // kryptonRibbonGroupLines7
        // 
        this.kryptonRibbonGroupLines7.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonPrevTexture,
        this.ButtonNextTexture,
        this.ButtonfirstTexture,
        this.ButtonLastTexture});
        // 
        // ButtonPrevTexture
        // 
        this.ButtonPrevTexture.Enabled = false;
        this.ButtonPrevTexture.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_prev_key_48x48;
        this.ButtonPrevTexture.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_prev_key_16x16;
        this.ButtonPrevTexture.KeyTip = "P";
        this.ButtonPrevTexture.TextLine1 = "Previous";
        this.ButtonPrevTexture.TextLine2 = "Texture";
        this.ButtonPrevTexture.ToolTipValues.Description = "Switches to the previous texture on the screen.\r\n\r\nNote: Currently this provides " +
"no functional benefit other than to show the user \r\nwhat the texture looks like." +
"\r\n";
        this.ButtonPrevTexture.ToolTipValues.Heading = "Previous Texture";
        this.ButtonPrevTexture.Click += new System.EventHandler(this.ButtonPrevTexture_Click);
        // 
        // ButtonNextTexture
        // 
        this.ButtonNextTexture.Enabled = false;
        this.ButtonNextTexture.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_next_key_48x48;
        this.ButtonNextTexture.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_next_key_16x16;
        this.ButtonNextTexture.KeyTip = "N";
        this.ButtonNextTexture.TextLine1 = "Next";
        this.ButtonNextTexture.TextLine2 = "Texture";
        this.ButtonNextTexture.ToolTipValues.Description = "Switches to the next texture on the screen.\r\n\r\nNote: Currently this provides no f" +
"unctional benefit other than to show the user \r\nwhat the texture looks like.\r\n";
        this.ButtonNextTexture.ToolTipValues.Heading = "Next Texture";
        this.ButtonNextTexture.Click += new System.EventHandler(this.ButtonNextTexture_Click);
        // 
        // ButtonfirstTexture
        // 
        this.ButtonfirstTexture.Enabled = false;
        this.ButtonfirstTexture.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_first_key_48x48;
        this.ButtonfirstTexture.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_first_key_16x16;
        this.ButtonfirstTexture.KeyTip = "F";
        this.ButtonfirstTexture.TextLine1 = "First";
        this.ButtonfirstTexture.TextLine2 = "Texture";
        this.ButtonfirstTexture.ToolTipValues.Description = "Switches to the first texture on the screen.\r\n\r\nNote: Currently this provides no " +
"functional benefit other than to show the user \r\nwhat the texture looks like.\r\n";
        this.ButtonfirstTexture.ToolTipValues.Heading = "First Texture";
        this.ButtonfirstTexture.Click += new System.EventHandler(this.ButtonfirstTexture_Click);
        // 
        // ButtonLastTexture
        // 
        this.ButtonLastTexture.Enabled = false;
        this.ButtonLastTexture.ImageLarge = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_last_key_48x48;
        this.ButtonLastTexture.ImageSmall = global::Gorgon.Editor.FontEditor.Properties.Resources.anim_last_key_16x16;
        this.ButtonLastTexture.KeyTip = "L";
        this.ButtonLastTexture.TextLine1 = "Last";
        this.ButtonLastTexture.TextLine2 = "Texture";
        this.ButtonLastTexture.ToolTipValues.Description = "Switches to the last texture on the screen.\r\n\r\nNote: Currently this provides no f" +
"unctional benefit other than to show the user \r\nwhat the texture looks like.\r\n";
        this.ButtonLastTexture.Click += new System.EventHandler(this.ButtonLastTexture_Click);
        // 
        // FormRibbon
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1437, 399);
        this.Controls.Add(this.RibbonTextContent);
        
        this.Name = "FormRibbon";
        this.Text = "FormRibbon";
        ((System.ComponentModel.ISupportInitialize)(this.RibbonTextContent)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericSize)).EndInit();
        this.MenuZoom.ResumeLayout(false);
        this.MenuBrushes.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private Krypton.Ribbon.KryptonRibbonTab TabFont;
    private Krypton.Ribbon.KryptonRibbonGroup GroupImageFile;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveFont;
    internal Krypton.Ribbon.KryptonRibbon RibbonTextContent;
    private Krypton.Ribbon.KryptonRibbonGroup GroupFont;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator3;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines3;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonZoom;
    private System.Windows.Forms.ContextMenuStrip MenuZoom;
    private System.Windows.Forms.ToolStripMenuItem ItemZoomToWindow;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem Item12Percent;
    private System.Windows.Forms.ToolStripMenuItem Item25Percent;
    private System.Windows.Forms.ToolStripMenuItem Item50Percent;
    private System.Windows.Forms.ToolStripMenuItem Item100Percent;
    private System.Windows.Forms.ToolStripMenuItem Item200Percent;
    private System.Windows.Forms.ToolStripMenuItem Item400Percent;
    private System.Windows.Forms.ToolStripMenuItem Item800Percent;
    private System.Windows.Forms.ToolStripMenuItem Item1600Percent;
    private System.Windows.Forms.ToolStripMenuItem Item3200Percent;
    private System.Windows.Forms.ToolStripMenuItem Item6400Percent;
    private Krypton.Ribbon.KryptonRibbonGroup GroupTextEdit;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonTextUndo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonTextRedo;
    private Krypton.Ribbon.KryptonRibbonGroupCustomControl CustomFonts;
    private ComboFonts ComboFonts;
    private Krypton.Ribbon.KryptonRibbonGroupLabel LabelFontFamily;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
    private Krypton.Ribbon.KryptonRibbonGroupRadioButton RadioPointUnits;
    private Krypton.Ribbon.KryptonRibbonGroupRadioButton RadioPixelUnits;
    private Krypton.Ribbon.KryptonRibbonGroupLabel LabelSize;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines4;
    private Krypton.Ribbon.KryptonRibbonGroupCheckBox CheckBold;
    private Krypton.Ribbon.KryptonRibbonGroupCheckBox CheckItalics;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple3;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonOutline;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator1;
    private Krypton.Ribbon.KryptonRibbonGroupCheckBox CheckAntiAlias;
    private Krypton.Ribbon.KryptonRibbonGroupCustomControl CustomControlSize;
    private System.Windows.Forms.NumericUpDown NumericSize;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines5;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonTexture;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonCharacterSelection;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonNew;
    private Krypton.Ribbon.KryptonRibbonTab TabTexture;
    private Krypton.Ribbon.KryptonRibbonGroup GroupTexture;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines6;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonResizeTexture;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonGlyphPad;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple4;
    private Krypton.Ribbon.KryptonRibbonGroupButton kryptonRibbonGroupButton1;
    private Krypton.Ribbon.KryptonRibbonGroupButton CheckPremultiply;
    private Krypton.Ribbon.KryptonRibbonContext ContextTextureEditor;
    private Krypton.Ribbon.KryptonRibbonGroup GroupTextureNav;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines7;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonPrevTexture;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonNextTexture;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonfirstTexture;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonLastTexture;
    private System.Windows.Forms.ContextMenuStrip MenuBrushes;
    private System.Windows.Forms.ToolStripMenuItem MenuItemSolid;
    private System.Windows.Forms.ToolStripMenuItem MenuItemPattern;
    private System.Windows.Forms.ToolStripMenuItem MenuItemGradient;
    private System.Windows.Forms.ToolStripMenuItem MenuItemTextured;
}
