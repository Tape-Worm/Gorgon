﻿namespace Gorgon.Editor.SpriteEditor;

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
                ContentRenderer.ZoomScaleChanged -= ContentRenderer_ZoomScale;
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRibbon));
        this.RibbonSpriteContent = new Krypton.Ribbon.KryptonRibbon();
        this.ContextClipSprite = new Krypton.Ribbon.KryptonRibbonContext();
        this.ContextSpriteCornerOffsets = new Krypton.Ribbon.KryptonRibbonContext();
        this.ContextPickSprite = new Krypton.Ribbon.KryptonRibbonContext();
        this.TabSprite = new Krypton.Ribbon.KryptonRibbonTab();
        this.GroupSpriteFile = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple1 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonNewSprite = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSaveSprite = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupSpriteEdit = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple2 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonClipSprite = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonPickSprite = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator2 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.kryptonRibbonGroupLines2 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonSpriteUndo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpriteRedo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupSprite = new Krypton.Ribbon.KryptonRibbonGroup();
        this.TripleSprite1 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonSpriteColor = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpriteAnchor = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpriteVertexOffsets = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.LinesSprite1 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonSpriteTextureFilter = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuFiltering = new System.Windows.Forms.ContextMenuStrip();
        this.MenuItemSmooth = new System.Windows.Forms.ToolStripMenuItem();
        this.MenuItemPixelated = new System.Windows.Forms.ToolStripMenuItem();
        this.ButtonSpriteTextureWrap = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator1 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.LinesSprite2 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonZoomSprite = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuZoom = new System.Windows.Forms.ContextMenuStrip();
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
        this.RibbonTabSpriteManualInput = new Krypton.Ribbon.KryptonRibbonTab();
        this.RibbonGroupSpriteClipFinal = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple5 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonSpriteClipApply = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpriteClipCancel = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.RibbonGroupManualClipping = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple4 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonClipManualInput = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpriteClipFullSize = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator3 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.kryptonRibbonGroupTriple7 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonFixedSize = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupTriple8 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.LabelFixedWidth = new Krypton.Ribbon.KryptonRibbonGroupLabel();
        this.NumericFixedWidth = new Krypton.Ribbon.KryptonRibbonGroupNumericUpDown();
        this.kryptonRibbonGroupTriple9 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.LabelFixedHeight = new Krypton.Ribbon.KryptonRibbonGroupLabel();
        this.NumericFixedHeight = new Krypton.Ribbon.KryptonRibbonGroupNumericUpDown();
        this.RibbonTabCornerEditing = new Krypton.Ribbon.KryptonRibbonTab();
        this.RibbonGroupSpriteCornerOffsetFinal = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple6 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonSpriteCornerOffsetApply = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpriteCornerOffsetCancel = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.RibbonGroupSpriteVertexCorner = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple3 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonSpriteCornerManualInput = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpriteCornerReset = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.RibbonTabSpritePick = new Krypton.Ribbon.KryptonRibbonTab();
        this.RibbonGroupPickFinalize = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple10 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonSpritePickApply = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSpritePickCancel = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.RibbonGroupRegion = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple11 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonPickMaskColor = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupLines1 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.LabelPadding = new Krypton.Ribbon.KryptonRibbonGroupLabel();
        this.NumericPadding = new Krypton.Ribbon.KryptonRibbonGroupNumericUpDown();
        ((System.ComponentModel.ISupportInitialize)(this.RibbonSpriteContent)).BeginInit();
        this.MenuFiltering.SuspendLayout();
        this.MenuZoom.SuspendLayout();
        this.SuspendLayout();
        // 
        // RibbonSpriteContent
        // 
        this.RibbonSpriteContent.AllowFormIntegrate = true;
        this.RibbonSpriteContent.InDesignHelperMode = true;
        this.RibbonSpriteContent.Name = "RibbonSpriteContent";
        this.RibbonSpriteContent.RibbonContexts.AddRange(new Krypton.Ribbon.KryptonRibbonContext[] {
        this.ContextClipSprite,
        this.ContextSpriteCornerOffsets,
        this.ContextPickSprite});
        this.RibbonSpriteContent.RibbonTabs.AddRange(new Krypton.Ribbon.KryptonRibbonTab[] {
        this.TabSprite,
        this.RibbonTabSpriteManualInput,
        this.RibbonTabCornerEditing,
        this.RibbonTabSpritePick});
        this.RibbonSpriteContent.SelectedTab = this.TabSprite;
        this.RibbonSpriteContent.Size = new System.Drawing.Size(1594, 115);
        this.RibbonSpriteContent.TabIndex = 0;
        // 
        // ContextClipSprite
        // 
        this.ContextClipSprite.ContextName = "ContextSpriteClip";
        this.ContextClipSprite.ContextTitle = "Clip";
        // 
        // ContextSpriteCornerOffsets
        // 
        this.ContextSpriteCornerOffsets.ContextColor = System.Drawing.Color.ForestGreen;
        this.ContextSpriteCornerOffsets.ContextName = "ContextCornerOffsets";
        this.ContextSpriteCornerOffsets.ContextTitle = "Corner";
        // 
        // ContextPickSprite
        // 
        this.ContextPickSprite.ContextColor = System.Drawing.Color.Blue;
        this.ContextPickSprite.ContextName = "ContextSpritePick";
        this.ContextPickSprite.ContextTitle = "Pick";
        // 
        // TabSprite
        // 
        this.TabSprite.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.GroupSpriteFile,
        this.GroupSpriteEdit,
        this.GroupSprite});
        this.TabSprite.KeyTip = "I";
        this.TabSprite.Text = "Sprite";
        // 
        // GroupSpriteFile
        // 
        this.GroupSpriteFile.AllowCollapsed = false;
        this.GroupSpriteFile.DialogBoxLauncher = false;
        this.GroupSpriteFile.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple1});
        this.GroupSpriteFile.KeyTipGroup = "F";
        this.GroupSpriteFile.TextLine1 = "File";
        // 
        // kryptonRibbonGroupTriple1
        // 
        this.kryptonRibbonGroupTriple1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonNewSprite,
        this.ButtonSaveSprite});
        this.kryptonRibbonGroupTriple1.MinimumSize = Krypton.Ribbon.GroupItemSize.Large;
        // 
        // ButtonNewSprite
        // 
        this.ButtonNewSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_48x48;
        this.ButtonNewSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_16x16;
        this.ButtonNewSprite.KeyTip = "N";
        this.ButtonNewSprite.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
        this.ButtonNewSprite.TextLine1 = "New";
        this.ButtonNewSprite.TextLine2 = "Sprite";
        this.ButtonNewSprite.ToolTipValues.Description = "Creates a new sprite based on the current sprite being edited.\r\n\r\nAll sprite prop" +
"erties (except the name) will be copied to the \r\nnew sprite.";
        this.ButtonNewSprite.ToolTipValues.Heading = "New sprite";
        this.ButtonNewSprite.Click += new System.EventHandler(this.ButtonNewSprite_Click);
        // 
        // ButtonSaveSprite
        // 
        this.ButtonSaveSprite.Enabled = false;
        this.ButtonSaveSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.save_content_48x48;
        this.ButtonSaveSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.save_content_16x16;
        this.ButtonSaveSprite.KeyTip = "S";
        this.ButtonSaveSprite.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
        this.ButtonSaveSprite.TextLine1 = "Save";
        this.ButtonSaveSprite.TextLine2 = "Sprite";
        this.ButtonSaveSprite.ToolTipValues.Description = "Updates the sprite file in the file system with the current changes.";
        this.ButtonSaveSprite.ToolTipValues.Heading = "Save Sprite";
        this.ButtonSaveSprite.Click += new System.EventHandler(this.ButtonSaveSprite_Click);
        // 
        // GroupSpriteEdit
        // 
        this.GroupSpriteEdit.AllowCollapsed = false;
        this.GroupSpriteEdit.DialogBoxLauncher = false;
        this.GroupSpriteEdit.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple2,
        this.kryptonRibbonGroupSeparator2,
        this.kryptonRibbonGroupLines2});
        this.GroupSpriteEdit.KeyTipGroup = "E";
        this.GroupSpriteEdit.TextLine1 = "Edit";
        // 
        // kryptonRibbonGroupTriple2
        // 
        this.kryptonRibbonGroupTriple2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonClipSprite,
        this.ButtonPickSprite});
        // 
        // ButtonClipSprite
        // 
        this.ButtonClipSprite.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonClipSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.clip_sprite_48x48;
        this.ButtonClipSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.clip_sprite_16x16;
        this.ButtonClipSprite.KeyTip = "K";
        this.ButtonClipSprite.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
        this.ButtonClipSprite.TextLine1 = "Clip";
        this.ButtonClipSprite.TextLine2 = "Sprite";
        this.ButtonClipSprite.ToolTipValues.Description = "Use a rectangular selection to define the dimensions of the sprite.";
        this.ButtonClipSprite.ToolTipValues.Heading = "Clip sprite";
        this.ButtonClipSprite.Click += new System.EventHandler(this.ButtonClipSprite_Click);
        // 
        // ButtonPickSprite
        // 
        this.ButtonPickSprite.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonPickSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.pick_48x48;
        this.ButtonPickSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.pick_16x16;
        this.ButtonPickSprite.KeyTip = "P";
        this.ButtonPickSprite.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
        this.ButtonPickSprite.TextLine1 = "Pick";
        this.ButtonPickSprite.ToolTipValues.Description = "Defines a sprites dimensions by clicking on the image and having Gorgon determine" +
" the extents.\r\n\r\nThis will only work on images that can be converted to a 32 bit" +
" RGBA or BGRA pixel format.";
        this.ButtonPickSprite.ToolTipValues.Heading = "Pick sprite";
        this.ButtonPickSprite.Click += new System.EventHandler(this.ButtonPickSprite_Click);
        // 
        // kryptonRibbonGroupLines2
        // 
        this.kryptonRibbonGroupLines2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSpriteUndo,
        this.ButtonSpriteRedo});
        // 
        // ButtonSpriteUndo
        // 
        this.ButtonSpriteUndo.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.undo_48x48;
        this.ButtonSpriteUndo.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.undo_16x16;
        this.ButtonSpriteUndo.KeyTip = "U";
        this.ButtonSpriteUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
        this.ButtonSpriteUndo.TextLine1 = "Undo";
        this.ButtonSpriteUndo.ToolTipValues.Description = "Reverts the previous change made to the sprite\r\n.";
        this.ButtonSpriteUndo.ToolTipValues.Heading = "Undo";
        this.ButtonSpriteUndo.Click += new System.EventHandler(this.ButtonSpriteUndo_Click);
        // 
        // ButtonSpriteRedo
        // 
        this.ButtonSpriteRedo.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.redo_48x48;
        this.ButtonSpriteRedo.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.redo_16x16;
        this.ButtonSpriteRedo.KeyTip = "R";
        this.ButtonSpriteRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
        this.ButtonSpriteRedo.TextLine1 = "Redo";
        this.ButtonSpriteRedo.ToolTipValues.Description = "Restores the next change made to the sprite.";
        this.ButtonSpriteRedo.ToolTipValues.Heading = "Redo";
        this.ButtonSpriteRedo.Click += new System.EventHandler(this.ButtonSpriteRedo_Click);
        // 
        // GroupSprite
        // 
        this.GroupSprite.AllowCollapsed = false;
        this.GroupSprite.DialogBoxLauncher = false;
        this.GroupSprite.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.TripleSprite1,
        this.LinesSprite1,
        this.kryptonRibbonGroupSeparator1,
        this.LinesSprite2});
        this.GroupSprite.KeyTipGroup = "S";
        this.GroupSprite.TextLine1 = "Sprite";
        // 
        // TripleSprite1
        // 
        this.TripleSprite1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSpriteColor,
        this.ButtonSpriteAnchor,
        this.ButtonSpriteVertexOffsets});
        // 
        // ButtonSpriteColor
        // 
        this.ButtonSpriteColor.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_color_48x48;
        this.ButtonSpriteColor.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_color_16x16;
        this.ButtonSpriteColor.KeyTip = "C";
        this.ButtonSpriteColor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
        this.ButtonSpriteColor.TextLine1 = "Color";
        this.ButtonSpriteColor.ToolTipValues.Description = "Defines the color tint, and/or opacity to apply to the sprite.";
        this.ButtonSpriteColor.ToolTipValues.Heading = "Sprite color";
        this.ButtonSpriteColor.Click += new System.EventHandler(this.ButtonSpriteColor_Click);
        // 
        // ButtonSpriteAnchor
        // 
        this.ButtonSpriteAnchor.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.anchor_48x48;
        this.ButtonSpriteAnchor.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.anchor_16x16;
        this.ButtonSpriteAnchor.KeyTip = "A";
        this.ButtonSpriteAnchor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
        this.ButtonSpriteAnchor.TextLine1 = "Anchor";
        this.ButtonSpriteAnchor.ToolTipValues.Description = "Defines the anchor point on the sprite where rotations, scaling and positioning a" +
"ll center on.";
        this.ButtonSpriteAnchor.ToolTipValues.Heading = "Sprite anchor";
        this.ButtonSpriteAnchor.Click += new System.EventHandler(this.ButtonSpriteAnchor_Click);
        // 
        // ButtonSpriteVertexOffsets
        // 
        this.ButtonSpriteVertexOffsets.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonSpriteVertexOffsets.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.rectangle_edit_48x48;
        this.ButtonSpriteVertexOffsets.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.rectangle_edit_16x16;
        this.ButtonSpriteVertexOffsets.KeyTip = "V";
        this.ButtonSpriteVertexOffsets.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
        this.ButtonSpriteVertexOffsets.TextLine1 = "Corner";
        this.ButtonSpriteVertexOffsets.TextLine2 = "Offsets";
        this.ButtonSpriteVertexOffsets.ToolTipValues.Description = "Adds an offset to an individual corner on the sprite rectangle, to create a defor" +
"med sprite.";
        this.ButtonSpriteVertexOffsets.ToolTipValues.Heading = "Sprite vertex offsets";
        this.ButtonSpriteVertexOffsets.Click += new System.EventHandler(this.ButtonSpriteVertexOffsets_Click);
        // 
        // LinesSprite1
        // 
        this.LinesSprite1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSpriteTextureFilter,
        this.ButtonSpriteTextureWrap});
        // 
        // ButtonSpriteTextureFilter
        // 
        this.ButtonSpriteTextureFilter.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.ButtonSpriteTextureFilter.ContextMenuStrip = this.MenuFiltering;
        this.ButtonSpriteTextureFilter.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.filtering_48x48;
        this.ButtonSpriteTextureFilter.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.filtering_16x16;
        this.ButtonSpriteTextureFilter.KeyTip = "F";
        this.ButtonSpriteTextureFilter.TextLine1 = "Texture";
        this.ButtonSpriteTextureFilter.TextLine2 = "Filtering";
        this.ButtonSpriteTextureFilter.ToolTipValues.Description = "Defines the type of texture filtering to apply to the sprite when it is scaled/ro" +
"tated.";
        this.ButtonSpriteTextureFilter.ToolTipValues.ToolTipStyle = Krypton.Toolkit.LabelStyle.KeyTip;
        this.ButtonSpriteTextureFilter.ToolTipValues.Heading = "Texture filtering";
        // 
        // MenuFiltering
        // 
        this.MenuFiltering.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MenuFiltering.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.MenuItemSmooth,
        this.MenuItemPixelated});
        this.MenuFiltering.Name = "MenuFiltering";
        this.MenuFiltering.Size = new System.Drawing.Size(123, 48);
        // 
        // MenuItemSmooth
        // 
        this.MenuItemSmooth.Checked = true;
        this.MenuItemSmooth.CheckOnClick = true;
        this.MenuItemSmooth.CheckState = System.Windows.Forms.CheckState.Checked;
        this.MenuItemSmooth.Name = "MenuItemSmooth";
        this.MenuItemSmooth.Size = new System.Drawing.Size(122, 22);
        this.MenuItemSmooth.Text = "&Smooth";
        this.MenuItemSmooth.Click += new System.EventHandler(this.MenuItemSmooth_Click);
        // 
        // MenuItemPixelated
        // 
        this.MenuItemPixelated.CheckOnClick = true;
        this.MenuItemPixelated.Name = "MenuItemPixelated";
        this.MenuItemPixelated.Size = new System.Drawing.Size(122, 22);
        this.MenuItemPixelated.Text = "&Pixelated";
        this.MenuItemPixelated.Click += new System.EventHandler(this.MenuItemPixelated_Click);
        // 
        // ButtonSpriteTextureWrap
        // 
        this.ButtonSpriteTextureWrap.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.wrapping_48x48;
        this.ButtonSpriteTextureWrap.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.wrapping_16x16;
        this.ButtonSpriteTextureWrap.KeyTip = "W";
        this.ButtonSpriteTextureWrap.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
        this.ButtonSpriteTextureWrap.TextLine1 = "Texture";
        this.ButtonSpriteTextureWrap.TextLine2 = "Wrapping";
        this.ButtonSpriteTextureWrap.ToolTipValues.Description = "The type of wrapping to perform when the image texture size is smaller than the s" +
"prite dimensions.";
        this.ButtonSpriteTextureWrap.ToolTipValues.Heading = "Texture wrapping";
        this.ButtonSpriteTextureWrap.Click += new System.EventHandler(this.ButtonSpriteTextureWrap_Click);
        // 
        // LinesSprite2
        // 
        this.LinesSprite2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonZoomSprite});
        // 
        // ButtonZoomSprite
        // 
        this.ButtonZoomSprite.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.ButtonZoomSprite.ContextMenuStrip = this.MenuZoom;
        this.ButtonZoomSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.zoom_48x48;
        this.ButtonZoomSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.zoom_16x16;
        this.ButtonZoomSprite.KeyTip = "Z";
        this.ButtonZoomSprite.TextLine1 = "Zoom";
        this.ButtonZoomSprite.ToolTipValues.Description = resources.GetString("ButtonZoomSprite.ToolTipValues.Description");
        this.ButtonZoomSprite.ToolTipValues.Heading = "Zoom";
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
        // RibbonTabSpriteManualInput
        // 
        this.RibbonTabSpriteManualInput.ContextName = "ContextSpriteClip";
        this.RibbonTabSpriteManualInput.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.RibbonGroupSpriteClipFinal,
        this.RibbonGroupManualClipping});
        this.RibbonTabSpriteManualInput.KeyTip = "M";
        this.RibbonTabSpriteManualInput.Text = "Clipping Tools";
        // 
        // RibbonGroupSpriteClipFinal
        // 
        this.RibbonGroupSpriteClipFinal.AllowCollapsed = false;
        this.RibbonGroupSpriteClipFinal.DialogBoxLauncher = false;
        this.RibbonGroupSpriteClipFinal.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple5});
        this.RibbonGroupSpriteClipFinal.KeyTipGroup = "F";
        this.RibbonGroupSpriteClipFinal.TextLine1 = "Finalize";
        // 
        // kryptonRibbonGroupTriple5
        // 
        this.kryptonRibbonGroupTriple5.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSpriteClipApply,
        this.ButtonSpriteClipCancel});
        // 
        // ButtonSpriteClipApply
        // 
        this.ButtonSpriteClipApply.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.confirm_48x48;
        this.ButtonSpriteClipApply.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.confirm_16x16;
        this.ButtonSpriteClipApply.KeyTip = "A";
        this.ButtonSpriteClipApply.TextLine1 = "Apply";
        this.ButtonSpriteClipApply.ToolTipValues.Description = "Applies the current rectangle region to the sprite.";
        this.ButtonSpriteClipApply.ToolTipValues.Heading = "Apply";
        this.ButtonSpriteClipApply.Click += new System.EventHandler(this.ButtonSpriteClipApply_Click);
        // 
        // ButtonSpriteClipCancel
        // 
        this.ButtonSpriteClipCancel.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.cancel_48x48;
        this.ButtonSpriteClipCancel.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.cancel_16x16;
        this.ButtonSpriteClipCancel.KeyTip = "C";
        this.ButtonSpriteClipCancel.TextLine1 = "Cancel";
        this.ButtonSpriteClipCancel.ToolTipValues.Description = "Cancels this clipping operation.";
        this.ButtonSpriteClipCancel.ToolTipValues.Heading = "Cancel";
        this.ButtonSpriteClipCancel.Click += new System.EventHandler(this.ButtonSpriteClipCancel_Click);
        // 
        // RibbonGroupManualClipping
        // 
        this.RibbonGroupManualClipping.AllowCollapsed = false;
        this.RibbonGroupManualClipping.DialogBoxLauncher = false;
        this.RibbonGroupManualClipping.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple4,
        this.kryptonRibbonGroupSeparator3,
        this.kryptonRibbonGroupTriple7,
        this.kryptonRibbonGroupTriple8,
        this.kryptonRibbonGroupTriple9});
        this.RibbonGroupManualClipping.KeyTipGroup = "R";
        this.RibbonGroupManualClipping.MinimumWidth = 120;
        this.RibbonGroupManualClipping.TextLine1 = "Region";
        // 
        // kryptonRibbonGroupTriple4
        // 
        this.kryptonRibbonGroupTriple4.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonClipManualInput,
        this.ButtonSpriteClipFullSize});
        // 
        // ButtonClipManualInput
        // 
        this.ButtonClipManualInput.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonClipManualInput.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.manual_input_48x48;
        this.ButtonClipManualInput.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.manual_input_16x16;
        this.ButtonClipManualInput.KeyTip = "N";
        this.ButtonClipManualInput.TextLine1 = "Numeric";
        this.ButtonClipManualInput.TextLine2 = "Entry";
        this.ButtonClipManualInput.ToolTipValues.Description = "Allows for precise adjustment of the sprite clipping rectangle via numeric values" +
" typed in by the user.";
        this.ButtonClipManualInput.ToolTipValues.Heading = "Numeric entry";
        // 
        // ButtonSpriteClipFullSize
        // 
        this.ButtonSpriteClipFullSize.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.full_size_48x48;
        this.ButtonSpriteClipFullSize.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.full_size_16x16;
        this.ButtonSpriteClipFullSize.KeyTip = "F";
        this.ButtonSpriteClipFullSize.TextLine1 = "Full Size";
        this.ButtonSpriteClipFullSize.ToolTipValues.Description = "This will set the sprite dimensions to the full size of the sprite.";
        this.ButtonSpriteClipFullSize.ToolTipValues.Heading = "Full size";
        this.ButtonSpriteClipFullSize.Click += new System.EventHandler(this.ButtonSpriteClipFullSize_Click);
        // 
        // kryptonRibbonGroupTriple7
        // 
        this.kryptonRibbonGroupTriple7.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonFixedSize});
        // 
        // ButtonFixedSize
        // 
        this.ButtonFixedSize.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonFixedSize.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.fixed_size_48x48;
        this.ButtonFixedSize.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.fixed_size_16x16;
        this.ButtonFixedSize.KeyTip = "X";
        this.ButtonFixedSize.TextLine1 = "Fixed Size";
        this.ButtonFixedSize.ToolTipValues.Description = "Locks the width and height of the clipping rectangle \r\nto the respective values i" +
"n the text boxes.";
        this.ButtonFixedSize.ToolTipValues.Heading = "Fixed Size";
        this.ButtonFixedSize.Click += new System.EventHandler(this.ButtonFixedSize_Click);
        // 
        // kryptonRibbonGroupTriple8
        // 
        this.kryptonRibbonGroupTriple8.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.LabelFixedWidth,
        this.NumericFixedWidth});
        this.kryptonRibbonGroupTriple8.MaximumSize = Krypton.Ribbon.GroupItemSize.Medium;
        this.kryptonRibbonGroupTriple8.MinimumSize = Krypton.Ribbon.GroupItemSize.Medium;
        // 
        // LabelFixedWidth
        // 
        this.LabelFixedWidth.TextLine1 = "Width:";
        // 
        // NumericFixedWidth
        // 
        this.NumericFixedWidth.DecimalPlaces = 99;
        this.NumericFixedWidth.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericFixedWidth.MaximumSize = new System.Drawing.Size(64, 0);
        this.NumericFixedWidth.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericFixedWidth.MinimumSize = new System.Drawing.Size(64, 0);
        this.NumericFixedWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericFixedWidth.Value = new decimal(new int[] {
        32,
        0,
        0,
        0});
        this.NumericFixedWidth.ValueChanged += new System.EventHandler(this.NumericFixed_ValueChanged);
        // 
        // kryptonRibbonGroupTriple9
        // 
        this.kryptonRibbonGroupTriple9.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.LabelFixedHeight,
        this.NumericFixedHeight});
        this.kryptonRibbonGroupTriple9.MaximumSize = Krypton.Ribbon.GroupItemSize.Medium;
        this.kryptonRibbonGroupTriple9.MinimumSize = Krypton.Ribbon.GroupItemSize.Medium;
        // 
        // LabelFixedHeight
        // 
        this.LabelFixedHeight.TextLine1 = "Height:";
        // 
        // NumericFixedHeight
        // 
        this.NumericFixedHeight.DecimalPlaces = 99;
        this.NumericFixedHeight.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericFixedHeight.MaximumSize = new System.Drawing.Size(64, 0);
        this.NumericFixedHeight.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericFixedHeight.MinimumSize = new System.Drawing.Size(64, 0);
        this.NumericFixedHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericFixedHeight.Value = new decimal(new int[] {
        32,
        0,
        0,
        0});
        this.NumericFixedHeight.ValueChanged += new System.EventHandler(this.NumericFixed_ValueChanged);
        // 
        // RibbonTabCornerEditing
        // 
        this.RibbonTabCornerEditing.ContextName = "ContextCornerOffsets";
        this.RibbonTabCornerEditing.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.RibbonGroupSpriteCornerOffsetFinal,
        this.RibbonGroupSpriteVertexCorner});
        this.RibbonTabCornerEditing.KeyTip = "E";
        this.RibbonTabCornerEditing.Text = "Corner Tools";
        // 
        // RibbonGroupSpriteCornerOffsetFinal
        // 
        this.RibbonGroupSpriteCornerOffsetFinal.AllowCollapsed = false;
        this.RibbonGroupSpriteCornerOffsetFinal.DialogBoxLauncher = false;
        this.RibbonGroupSpriteCornerOffsetFinal.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple6});
        this.RibbonGroupSpriteCornerOffsetFinal.KeyTipGroup = "F";
        this.RibbonGroupSpriteCornerOffsetFinal.TextLine1 = "Finalize";
        // 
        // kryptonRibbonGroupTriple6
        // 
        this.kryptonRibbonGroupTriple6.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSpriteCornerOffsetApply,
        this.ButtonSpriteCornerOffsetCancel});
        // 
        // ButtonSpriteCornerOffsetApply
        // 
        this.ButtonSpriteCornerOffsetApply.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.confirm_48x48;
        this.ButtonSpriteCornerOffsetApply.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.confirm_16x16;
        this.ButtonSpriteCornerOffsetApply.KeyTip = "A";
        this.ButtonSpriteCornerOffsetApply.TextLine1 = "Apply";
        this.ButtonSpriteCornerOffsetApply.ToolTipValues.Description = "Applies the corner offset changes to the current sprite.\r\n";
        this.ButtonSpriteCornerOffsetApply.ToolTipValues.Heading = "Apply";
        this.ButtonSpriteCornerOffsetApply.Click += new System.EventHandler(this.ButtonSpriteCornerOffsetApply_Click);
        // 
        // ButtonSpriteCornerOffsetCancel
        // 
        this.ButtonSpriteCornerOffsetCancel.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.cancel_48x48;
        this.ButtonSpriteCornerOffsetCancel.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.cancel_16x16;
        this.ButtonSpriteCornerOffsetCancel.KeyTip = "C";
        this.ButtonSpriteCornerOffsetCancel.TextLine1 = "Cancel";
        this.ButtonSpriteCornerOffsetCancel.ToolTipValues.Description = "Cancels this corner offset operation.";
        this.ButtonSpriteCornerOffsetCancel.ToolTipValues.Heading = "Cancel";
        this.ButtonSpriteCornerOffsetCancel.Click += new System.EventHandler(this.ButtonSpriteCornerOffsetCancel_Click);
        // 
        // RibbonGroupSpriteVertexCorner
        // 
        this.RibbonGroupSpriteVertexCorner.AllowCollapsed = false;
        this.RibbonGroupSpriteVertexCorner.DialogBoxLauncher = false;
        this.RibbonGroupSpriteVertexCorner.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple3});
        this.RibbonGroupSpriteVertexCorner.KeyTipGroup = "O";
        this.RibbonGroupSpriteVertexCorner.MinimumWidth = 120;
        this.RibbonGroupSpriteVertexCorner.TextLine1 = "Offsets";
        // 
        // kryptonRibbonGroupTriple3
        // 
        this.kryptonRibbonGroupTriple3.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSpriteCornerManualInput,
        this.ButtonSpriteCornerReset});
        // 
        // ButtonSpriteCornerManualInput
        // 
        this.ButtonSpriteCornerManualInput.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonSpriteCornerManualInput.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.manual_input_48x48;
        this.ButtonSpriteCornerManualInput.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.manual_input_16x16;
        this.ButtonSpriteCornerManualInput.KeyTip = "N";
        this.ButtonSpriteCornerManualInput.TextLine1 = "Numeric";
        this.ButtonSpriteCornerManualInput.TextLine2 = "Entry";
        this.ButtonSpriteCornerManualInput.ToolTipValues.Description = resources.GetString("ButtonSpriteCornerManualInput.ToolTipValues.Description");
        this.ButtonSpriteCornerManualInput.ToolTipValues.Heading = "Numeric entry";
        // 
        // ButtonSpriteCornerReset
        // 
        this.ButtonSpriteCornerReset.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.reset_48x48;
        this.ButtonSpriteCornerReset.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.reset_16x16;
        this.ButtonSpriteCornerReset.KeyTip = "R";
        this.ButtonSpriteCornerReset.TextLine1 = "Reset";
        this.ButtonSpriteCornerReset.ToolTipValues.Description = "Resets the selected corner back to 0x0.";
        this.ButtonSpriteCornerReset.ToolTipValues.Heading = "Reset";
        this.ButtonSpriteCornerReset.Click += new System.EventHandler(this.ButtonSpriteCornerReset_Click);
        // 
        // RibbonTabSpritePick
        // 
        this.RibbonTabSpritePick.ContextName = "ContextSpritePick";
        this.RibbonTabSpritePick.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.RibbonGroupPickFinalize,
        this.RibbonGroupRegion});
        this.RibbonTabSpritePick.KeyTip = "P";
        this.RibbonTabSpritePick.Text = "Picking Tools";
        // 
        // RibbonGroupPickFinalize
        // 
        this.RibbonGroupPickFinalize.AllowCollapsed = false;
        this.RibbonGroupPickFinalize.DialogBoxLauncher = false;
        this.RibbonGroupPickFinalize.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple10});
        this.RibbonGroupPickFinalize.KeyTipGroup = "F";
        this.RibbonGroupPickFinalize.TextLine1 = "Finalize";
        // 
        // kryptonRibbonGroupTriple10
        // 
        this.kryptonRibbonGroupTriple10.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSpritePickApply,
        this.ButtonSpritePickCancel});
        // 
        // ButtonSpritePickApply
        // 
        this.ButtonSpritePickApply.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.confirm_48x48;
        this.ButtonSpritePickApply.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.confirm_16x16;
        this.ButtonSpritePickApply.KeyTip = "A";
        this.ButtonSpritePickApply.TextLine1 = "Apply";
        this.ButtonSpritePickApply.ToolTipValues.Description = "Applies the current rectangle region to the sprite.";
        this.ButtonSpritePickApply.ToolTipValues.Heading = "Apply";
        this.ButtonSpritePickApply.Click += new System.EventHandler(this.ButtonSpritePickApply_Click);
        // 
        // ButtonSpritePickCancel
        // 
        this.ButtonSpritePickCancel.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.cancel_48x48;
        this.ButtonSpritePickCancel.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.cancel_16x16;
        this.ButtonSpritePickCancel.KeyTip = "C";
        this.ButtonSpritePickCancel.TextLine1 = "Cancel";
        this.ButtonSpritePickCancel.ToolTipValues.Description = "Cancels this clipping operation.";
        this.ButtonSpritePickCancel.ToolTipValues.Heading = "Cancel";
        this.ButtonSpritePickCancel.Click += new System.EventHandler(this.ButtonSpritePickCancel_Click);
        // 
        // RibbonGroupRegion
        // 
        this.RibbonGroupRegion.AllowCollapsed = false;
        this.RibbonGroupRegion.DialogBoxLauncher = false;
        this.RibbonGroupRegion.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple11,
        this.kryptonRibbonGroupLines1});
        this.RibbonGroupRegion.KeyTipGroup = "R";
        this.RibbonGroupRegion.TextLine1 = "Region";
        // 
        // kryptonRibbonGroupTriple11
        // 
        this.kryptonRibbonGroupTriple11.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonPickMaskColor});
        // 
        // ButtonPickMaskColor
        // 
        this.ButtonPickMaskColor.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.ButtonPickMaskColor.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.color_pick_48x48;
        this.ButtonPickMaskColor.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.color_pick_16x16;
        this.ButtonPickMaskColor.KeyTip = "M";
        this.ButtonPickMaskColor.TextLine1 = "Masking";
        this.ButtonPickMaskColor.TextLine2 = "Color";
        this.ButtonPickMaskColor.ToolTipValues.Description = "Defines the color or alpha value used as the boundary for the sprite picking oper" +
"ation.\r\n\r\nThis color or alpha value is used to determine the extents of the spri" +
"te.";
        this.ButtonPickMaskColor.ToolTipValues.Heading = "Masking color";
        this.ButtonPickMaskColor.Click += new System.EventHandler(this.ButtonPickMaskColor_Click);
        // 
        // kryptonRibbonGroupLines1
        // 
        this.kryptonRibbonGroupLines1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.LabelPadding,
        this.NumericPadding});
        // 
        // LabelPadding
        // 
        this.LabelPadding.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.pick_padding_48x48;
        this.LabelPadding.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.pick_padding_16x16;
        this.LabelPadding.TextLine1 = "Padding";
        this.LabelPadding.ToolTipValues.Description = "Expands (or contracts) the selection rectangle by the number of pixels specified." +
"";
        this.LabelPadding.ToolTipValues.Heading = "Padding";
        // 
        // NumericPadding
        // 
        this.NumericPadding.Maximum = new decimal(new int[] {
        16,
        0,
        0,
        0});
        this.NumericPadding.MaximumSize = new System.Drawing.Size(96, 0);
        this.NumericPadding.MinimumSize = new System.Drawing.Size(96, 0);
        this.NumericPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericPadding.ValueChanged += new System.EventHandler(this.NumericPadding_ValueChanged);
        // 
        // FormRibbon
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1594, 302);
        this.Controls.Add(this.RibbonSpriteContent);
        
        this.Name = "FormRibbon";
        this.Text = "FormRibbon";
        ((System.ComponentModel.ISupportInitialize)(this.RibbonSpriteContent)).EndInit();
        this.MenuFiltering.ResumeLayout(false);
        this.MenuZoom.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private Krypton.Ribbon.KryptonRibbonTab TabSprite;
    private Krypton.Ribbon.KryptonRibbonGroup GroupSpriteFile;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveSprite;
    internal Krypton.Ribbon.KryptonRibbon RibbonSpriteContent;
    private Krypton.Ribbon.KryptonRibbonGroup GroupSpriteEdit;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteUndo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteRedo;
    private Krypton.Ribbon.KryptonRibbonGroupLines LinesSprite2;
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
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonNewSprite;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonClipSprite;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonPickSprite;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteColor;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteVertexOffsets;
    private Krypton.Ribbon.KryptonRibbonGroup GroupSprite;
    private Krypton.Ribbon.KryptonRibbonGroupTriple TripleSprite1;
    private Krypton.Ribbon.KryptonRibbonGroupLines LinesSprite1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteTextureFilter;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteTextureWrap;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteAnchor;
    private System.Windows.Forms.ToolStripMenuItem Item3200Percent;
    private System.Windows.Forms.ToolStripMenuItem Item6400Percent;
    private System.Windows.Forms.ContextMenuStrip MenuFiltering;
    private System.Windows.Forms.ToolStripMenuItem MenuItemSmooth;
    private System.Windows.Forms.ToolStripMenuItem MenuItemPixelated;
    private Krypton.Ribbon.KryptonRibbonContext ContextClipSprite;
    private Krypton.Ribbon.KryptonRibbonTab RibbonTabSpriteManualInput;
    private Krypton.Ribbon.KryptonRibbonGroup RibbonGroupManualClipping;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple4;
    private Krypton.Ribbon.KryptonRibbonTab RibbonTabCornerEditing;
    private Krypton.Ribbon.KryptonRibbonGroup RibbonGroupSpriteVertexCorner;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple3;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteCornerReset;
    private Krypton.Ribbon.KryptonRibbonContext ContextSpriteCornerOffsets;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator1;
    private Krypton.Ribbon.KryptonRibbonGroup RibbonGroupSpriteClipFinal;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple5;
    private Krypton.Ribbon.KryptonRibbonGroup RibbonGroupSpriteCornerOffsetFinal;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple6;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteCornerOffsetApply;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteCornerOffsetCancel;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteClipFullSize;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple7;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFixedSize;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple8;
    private Krypton.Ribbon.KryptonRibbonGroupLabel LabelFixedWidth;
    private Krypton.Ribbon.KryptonRibbonGroupNumericUpDown NumericFixedWidth;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple9;
    private Krypton.Ribbon.KryptonRibbonGroupLabel LabelFixedHeight;
    private Krypton.Ribbon.KryptonRibbonGroupNumericUpDown NumericFixedHeight;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator3;
    private Krypton.Ribbon.KryptonRibbonTab RibbonTabSpritePick;
    private Krypton.Ribbon.KryptonRibbonContext ContextPickSprite;
    private Krypton.Ribbon.KryptonRibbonGroup RibbonGroupPickFinalize;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple10;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpritePickApply;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpritePickCancel;
    private Krypton.Ribbon.KryptonRibbonGroup RibbonGroupRegion;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple11;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonPickMaskColor;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
    private Krypton.Ribbon.KryptonRibbonGroupLabel LabelPadding;
    private Krypton.Ribbon.KryptonRibbonGroupNumericUpDown NumericPadding;
    internal Krypton.Ribbon.KryptonRibbonGroupButton ButtonClipManualInput;
    internal Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteClipApply;
    internal Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteClipCancel;
    internal Krypton.Ribbon.KryptonRibbonGroupButton ButtonZoomSprite;
    internal Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteCornerManualInput;
}
