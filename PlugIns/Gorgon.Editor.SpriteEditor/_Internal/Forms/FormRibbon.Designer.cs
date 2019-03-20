namespace Gorgon.Editor.SpriteEditor
{
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
                DataContext = null;
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
            this.components = new System.ComponentModel.Container();
            this.RibbonSpriteContent = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.TabSprite = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.GroupSpriteFile = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonNewSprite = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonSaveSprite = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupSpriteEdit = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonClipSprite = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupLines1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonPickSprite = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonSpriteVertexOffsets = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupSeparator2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.kryptonRibbonGroupLines2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonSpriteUndo = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonSpriteRedo = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupSprite = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.TripleSpriteColor = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonSpriteColor = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonSpriteAnchor = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.LinesSpriteVertex = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonSpriteTextureCoords = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonSpriteVertexColors = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupSpriteImage = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupSeparator3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.LinesImageSampling = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonSpriteImageFilter = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonSpriteImageWrap = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupLines3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonZoomSprite = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
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
            ((System.ComponentModel.ISupportInitialize)(this.RibbonSpriteContent)).BeginInit();
            this.MenuZoom.SuspendLayout();
            this.SuspendLayout();
            // 
            // RibbonSpriteContent
            // 
            this.RibbonSpriteContent.AllowFormIntegrate = true;
            this.RibbonSpriteContent.InDesignHelperMode = true;
            this.RibbonSpriteContent.Name = "RibbonSpriteContent";
            this.RibbonSpriteContent.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
            this.TabSprite});
            this.RibbonSpriteContent.SelectedTab = this.TabSprite;
            this.RibbonSpriteContent.Size = new System.Drawing.Size(1594, 115);
            this.RibbonSpriteContent.TabIndex = 0;
            // 
            // TabSprite
            // 
            this.TabSprite.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
            this.GroupSpriteFile,
            this.GroupSpriteEdit,
            this.GroupSprite,
            this.GroupSpriteImage});
            this.TabSprite.KeyTip = "I";
            this.TabSprite.Text = "Sprite";
            // 
            // GroupSpriteFile
            // 
            this.GroupSpriteFile.AllowCollapsed = false;
            this.GroupSpriteFile.DialogBoxLauncher = false;
            this.GroupSpriteFile.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple1});
            this.GroupSpriteFile.KeyTipGroup = "F";
            this.GroupSpriteFile.TextLine1 = "File";
            // 
            // kryptonRibbonGroupTriple1
            // 
            this.kryptonRibbonGroupTriple1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonNewSprite,
            this.ButtonSaveSprite});
            this.kryptonRibbonGroupTriple1.MinimumSize = ComponentFactory.Krypton.Ribbon.GroupItemSize.Large;
            // 
            // ButtonNewSprite
            // 
            this.ButtonNewSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_48x48;
            this.ButtonNewSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_16x16;
            this.ButtonNewSprite.KeyTip = "N";
            this.ButtonNewSprite.TextLine1 = "New";
            this.ButtonNewSprite.TextLine2 = "Sprite";
            this.ButtonNewSprite.ToolTipBody = "Creates a new sprite for the currently loaded image.";
            this.ButtonNewSprite.ToolTipTitle = "New sprite";
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
            this.ButtonSaveSprite.ToolTipBody = "Updates the sprite file in the file system with the current changes.";
            this.ButtonSaveSprite.ToolTipTitle = "Save Sprite";
            this.ButtonSaveSprite.Click += new System.EventHandler(this.ButtonSaveSprite_Click);
            // 
            // GroupSpriteEdit
            // 
            this.GroupSpriteEdit.AllowCollapsed = false;
            this.GroupSpriteEdit.DialogBoxLauncher = false;
            this.GroupSpriteEdit.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple2,
            this.kryptonRibbonGroupLines1,
            this.kryptonRibbonGroupSeparator2,
            this.kryptonRibbonGroupLines2});
            this.GroupSpriteEdit.KeyTipGroup = "E";
            this.GroupSpriteEdit.TextLine1 = "Edit";
            // 
            // kryptonRibbonGroupTriple2
            // 
            this.kryptonRibbonGroupTriple2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonClipSprite});
            // 
            // ButtonClipSprite
            // 
            this.ButtonClipSprite.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonClipSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.clip_sprite_48x48;
            this.ButtonClipSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.clip_sprite_16x16;
            this.ButtonClipSprite.KeyTip = "L";
            this.ButtonClipSprite.TextLine1 = "Clip";
            this.ButtonClipSprite.TextLine2 = "Sprite";
            this.ButtonClipSprite.ToolTipBody = "Use a rectangular selection to define the dimensions of the sprite.";
            this.ButtonClipSprite.ToolTipTitle = "Clip sprite";
            this.ButtonClipSprite.Click += new System.EventHandler(this.ButtonClipSprite_Click);
            // 
            // kryptonRibbonGroupLines1
            // 
            this.kryptonRibbonGroupLines1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonPickSprite,
            this.ButtonSpriteVertexOffsets});
            // 
            // ButtonPickSprite
            // 
            this.ButtonPickSprite.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonPickSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.pick_48x48;
            this.ButtonPickSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.pick_16x16;
            this.ButtonPickSprite.KeyTip = "P";
            this.ButtonPickSprite.TextLine1 = "Pick";
            this.ButtonPickSprite.ToolTipBody = "Defines a sprites dimensions by clicking on the image and having Gorgon determine" +
    " the extents.\r\n\r\nThis will only work on images that can be converted to a 32 bit" +
    " RGBA or BGRA pixel format.";
            this.ButtonPickSprite.ToolTipTitle = "Pick sprite";
            // 
            // ButtonSpriteVertexOffsets
            // 
            this.ButtonSpriteVertexOffsets.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonSpriteVertexOffsets.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.rectangle_edit_48x48;
            this.ButtonSpriteVertexOffsets.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.rectangle_edit_16x16;
            this.ButtonSpriteVertexOffsets.KeyTip = "V";
            this.ButtonSpriteVertexOffsets.TextLine1 = "Corner";
            this.ButtonSpriteVertexOffsets.TextLine2 = "Offsets";
            this.ButtonSpriteVertexOffsets.ToolTipBody = "Adds an offset to an individual corner on the sprite rectangle, to create a defor" +
    "med sprite.";
            this.ButtonSpriteVertexOffsets.ToolTipTitle = "Sprite vertex offsets";
            // 
            // kryptonRibbonGroupLines2
            // 
            this.kryptonRibbonGroupLines2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonSpriteUndo,
            this.ButtonSpriteRedo});
            // 
            // ButtonSpriteUndo
            // 
            this.ButtonSpriteUndo.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.undo_48x48;
            this.ButtonSpriteUndo.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.undo_16x16;
            this.ButtonSpriteUndo.KeyTip = "Z";
            this.ButtonSpriteUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.ButtonSpriteUndo.TextLine1 = "Undo";
            this.ButtonSpriteUndo.ToolTipBody = "Reverts the previous change made to the sprite\r\n.";
            this.ButtonSpriteUndo.ToolTipTitle = "Undo";
            this.ButtonSpriteUndo.Click += new System.EventHandler(this.ButtonSpriteUndo_Click);
            // 
            // ButtonSpriteRedo
            // 
            this.ButtonSpriteRedo.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.redo_48x48;
            this.ButtonSpriteRedo.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.redo_16x16;
            this.ButtonSpriteRedo.KeyTip = "Y";
            this.ButtonSpriteRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.ButtonSpriteRedo.TextLine1 = "Redo";
            this.ButtonSpriteRedo.ToolTipBody = "Restores the next change made to the sprite.";
            this.ButtonSpriteRedo.ToolTipTitle = "Redo";
            this.ButtonSpriteRedo.Click += new System.EventHandler(this.ButtonSpriteRedo_Click);
            // 
            // GroupSprite
            // 
            this.GroupSprite.AllowCollapsed = false;
            this.GroupSprite.DialogBoxLauncher = false;
            this.GroupSprite.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.TripleSpriteColor,
            this.LinesSpriteVertex});
            this.GroupSprite.KeyTipGroup = "S";
            this.GroupSprite.TextLine1 = "Sprite";
            // 
            // TripleSpriteColor
            // 
            this.TripleSpriteColor.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonSpriteColor,
            this.ButtonSpriteAnchor});
            // 
            // ButtonSpriteColor
            // 
            this.ButtonSpriteColor.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonSpriteColor.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_color_48x48;
            this.ButtonSpriteColor.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.sprite_color_16x16;
            this.ButtonSpriteColor.KeyTip = "C";
            this.ButtonSpriteColor.TextLine1 = "Color";
            this.ButtonSpriteColor.ToolTipBody = "Defines the color tint, and/or opacity to apply to the sprite.";
            this.ButtonSpriteColor.ToolTipTitle = "Sprite color";
            // 
            // ButtonSpriteAnchor
            // 
            this.ButtonSpriteAnchor.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonSpriteAnchor.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.anchor_48x48;
            this.ButtonSpriteAnchor.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.anchor_16x16;
            this.ButtonSpriteAnchor.KeyTip = "A";
            this.ButtonSpriteAnchor.TextLine1 = "Anchor";
            this.ButtonSpriteAnchor.ToolTipBody = "Defines the anchor point on the sprite where rotations, scaling and positioning a" +
    "ll center on.";
            this.ButtonSpriteAnchor.ToolTipTitle = "Sprite anchor";
            // 
            // LinesSpriteVertex
            // 
            this.LinesSpriteVertex.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonSpriteTextureCoords,
            this.ButtonSpriteVertexColors});
            // 
            // ButtonSpriteTextureCoords
            // 
            this.ButtonSpriteTextureCoords.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonSpriteTextureCoords.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.edit_texture_coordinates_48x48;
            this.ButtonSpriteTextureCoords.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.edit_texture_coordinates_16x16;
            this.ButtonSpriteTextureCoords.KeyTip = "T";
            this.ButtonSpriteTextureCoords.TextLine1 = "Texture";
            this.ButtonSpriteTextureCoords.TextLine2 = "Coordinates";
            this.ButtonSpriteTextureCoords.ToolTipBody = "Allows altering of the texture coordinates for a sprite so that the sprite can co" +
    "ver more, or less of an area on the texture without altering the sprite size.";
            this.ButtonSpriteTextureCoords.ToolTipTitle = "Sprite texture coordinates";
            // 
            // ButtonSpriteVertexColors
            // 
            this.ButtonSpriteVertexColors.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonSpriteVertexColors.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.edit_vertex_colors_48x48;
            this.ButtonSpriteVertexColors.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.edit_vertex_colors_16x16;
            this.ButtonSpriteVertexColors.KeyTip = "X";
            this.ButtonSpriteVertexColors.TextLine1 = "Corner";
            this.ButtonSpriteVertexColors.TextLine2 = "Colors";
            this.ButtonSpriteVertexColors.ToolTipBody = "Allows altering the tint color for a single corner in a sprite rectangle.";
            this.ButtonSpriteVertexColors.ToolTipTitle = "Sprite vertex colors";
            // 
            // GroupSpriteImage
            // 
            this.GroupSpriteImage.AllowCollapsed = false;
            this.GroupSpriteImage.DialogBoxLauncher = false;
            this.GroupSpriteImage.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupSeparator3,
            this.LinesImageSampling,
            this.kryptonRibbonGroupLines3});
            this.GroupSpriteImage.KeyTipGroup = "I";
            this.GroupSpriteImage.TextLine1 = "Image";
            // 
            // LinesImageSampling
            // 
            this.LinesImageSampling.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonSpriteImageFilter,
            this.ButtonSpriteImageWrap});
            // 
            // ButtonSpriteImageFilter
            // 
            this.ButtonSpriteImageFilter.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonSpriteImageFilter.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.filtering_48x48;
            this.ButtonSpriteImageFilter.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.filtering_16x16;
            this.ButtonSpriteImageFilter.KeyTip = "F";
            this.ButtonSpriteImageFilter.TextLine1 = "Image";
            this.ButtonSpriteImageFilter.TextLine2 = "Filtering";
            this.ButtonSpriteImageFilter.ToolTipBody = "Defines the type of image filtering to apply to the sprite when it is scaled/rota" +
    "ted.";
            this.ButtonSpriteImageFilter.ToolTipStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.KeyTip;
            this.ButtonSpriteImageFilter.ToolTipTitle = "Image filtering";
            // 
            // ButtonSpriteImageWrap
            // 
            this.ButtonSpriteImageWrap.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.Check;
            this.ButtonSpriteImageWrap.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.wrapping_48x48;
            this.ButtonSpriteImageWrap.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.wrapping_16x16;
            this.ButtonSpriteImageWrap.KeyTip = "W";
            this.ButtonSpriteImageWrap.TextLine1 = "Image";
            this.ButtonSpriteImageWrap.TextLine2 = "Wrapping";
            this.ButtonSpriteImageWrap.ToolTipBody = "The type of wrapping to perform when the image texture size is smaller than the s" +
    "prite dimensions.";
            this.ButtonSpriteImageWrap.ToolTipTitle = "Image wrapping";
            // 
            // kryptonRibbonGroupLines3
            // 
            this.kryptonRibbonGroupLines3.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonZoomSprite});
            // 
            // ButtonZoomSprite
            // 
            this.ButtonZoomSprite.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonZoomSprite.ContextMenuStrip = this.MenuZoom;
            this.ButtonZoomSprite.ImageLarge = global::Gorgon.Editor.SpriteEditor.Properties.Resources.zoom_48x48;
            this.ButtonZoomSprite.ImageSmall = global::Gorgon.Editor.SpriteEditor.Properties.Resources.zoom_16x16;
            this.ButtonZoomSprite.KeyTip = "Z";
            this.ButtonZoomSprite.TextLine1 = "Zoom";
            this.ButtonZoomSprite.ToolTipTitle = "Zoom";
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
            this.MenuZoom.Size = new System.Drawing.Size(181, 274);
            // 
            // ItemZoomToWindow
            // 
            this.ItemZoomToWindow.Checked = true;
            this.ItemZoomToWindow.CheckOnClick = true;
            this.ItemZoomToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ItemZoomToWindow.Name = "ItemZoomToWindow";
            this.ItemZoomToWindow.Size = new System.Drawing.Size(180, 22);
            this.ItemZoomToWindow.Tag = "ToWindow";
            this.ItemZoomToWindow.Text = "To Window";
            this.ItemZoomToWindow.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 6);
            // 
            // Item12Percent
            // 
            this.Item12Percent.CheckOnClick = true;
            this.Item12Percent.Name = "Item12Percent";
            this.Item12Percent.Size = new System.Drawing.Size(180, 22);
            this.Item12Percent.Tag = "Percent12";
            this.Item12Percent.Text = "12.5%";
            this.Item12Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item25Percent
            // 
            this.Item25Percent.CheckOnClick = true;
            this.Item25Percent.Name = "Item25Percent";
            this.Item25Percent.Size = new System.Drawing.Size(180, 22);
            this.Item25Percent.Tag = "Percent25";
            this.Item25Percent.Text = "25%";
            this.Item25Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item50Percent
            // 
            this.Item50Percent.CheckOnClick = true;
            this.Item50Percent.Name = "Item50Percent";
            this.Item50Percent.Size = new System.Drawing.Size(180, 22);
            this.Item50Percent.Tag = "Percent50";
            this.Item50Percent.Text = "50%";
            this.Item50Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item100Percent
            // 
            this.Item100Percent.CheckOnClick = true;
            this.Item100Percent.Name = "Item100Percent";
            this.Item100Percent.Size = new System.Drawing.Size(180, 22);
            this.Item100Percent.Tag = "Percent100";
            this.Item100Percent.Text = "100%";
            this.Item100Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item200Percent
            // 
            this.Item200Percent.CheckOnClick = true;
            this.Item200Percent.Name = "Item200Percent";
            this.Item200Percent.Size = new System.Drawing.Size(180, 22);
            this.Item200Percent.Tag = "Percent200";
            this.Item200Percent.Text = "200%";
            this.Item200Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item400Percent
            // 
            this.Item400Percent.CheckOnClick = true;
            this.Item400Percent.Name = "Item400Percent";
            this.Item400Percent.Size = new System.Drawing.Size(180, 22);
            this.Item400Percent.Tag = "Percent400";
            this.Item400Percent.Text = "400%";
            this.Item400Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item800Percent
            // 
            this.Item800Percent.CheckOnClick = true;
            this.Item800Percent.Name = "Item800Percent";
            this.Item800Percent.Size = new System.Drawing.Size(180, 22);
            this.Item800Percent.Tag = "Percent800";
            this.Item800Percent.Text = "800%";
            this.Item800Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item1600Percent
            // 
            this.Item1600Percent.CheckOnClick = true;
            this.Item1600Percent.Name = "Item1600Percent";
            this.Item1600Percent.Size = new System.Drawing.Size(180, 22);
            this.Item1600Percent.Tag = "Percent1600";
            this.Item1600Percent.Text = "1600%";
            this.Item1600Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item3200Percent
            // 
            this.Item3200Percent.Name = "Item3200Percent";
            this.Item3200Percent.Size = new System.Drawing.Size(180, 22);
            this.Item3200Percent.Tag = "Percent3200";
            this.Item3200Percent.Text = "3200%";
            this.Item3200Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item6400Percent
            // 
            this.Item6400Percent.Name = "Item6400Percent";
            this.Item6400Percent.Size = new System.Drawing.Size(180, 22);
            this.Item6400Percent.Tag = "Percent6400";
            this.Item6400Percent.Text = "6400%";
            this.Item6400Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // FormRibbon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1594, 302);
            this.Controls.Add(this.RibbonSpriteContent);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormRibbon";
            this.Text = "FormRibbon";
            ((System.ComponentModel.ISupportInitialize)(this.RibbonSpriteContent)).EndInit();
            this.MenuZoom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabSprite;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupSpriteFile;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveSprite;
        internal ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonSpriteContent;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupSpriteEdit;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupSpriteImage;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteUndo;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteRedo;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonZoomSprite;
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
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonNewSprite;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonClipSprite;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonPickSprite;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteColor;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteVertexOffsets;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteVertexColors;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupSprite;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple TripleSpriteColor;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines LinesSpriteVertex;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteTextureCoords;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteImageFilter;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteImageWrap;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSpriteAnchor;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines LinesImageSampling;
        private System.Windows.Forms.ToolStripMenuItem Item3200Percent;
        private System.Windows.Forms.ToolStripMenuItem Item6400Percent;
    }
}