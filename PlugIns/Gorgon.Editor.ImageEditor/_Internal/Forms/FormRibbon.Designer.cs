namespace Gorgon.Editor.ImageEditor;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRibbon));
        this.RibbonImageContent = new Krypton.Ribbon.KryptonRibbon();
        this.ContextFx = new Krypton.Ribbon.KryptonRibbonContext();
        this.TabImage = new Krypton.Ribbon.KryptonRibbonTab();
        this.GroupImageFile = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple1 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonSaveImage = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator1 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.GroupCodec = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonImport = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonExport = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuCodecs = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.GroupEdit = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple3 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonEditInApp = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFx = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator2 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.kryptonRibbonGroupLines2 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonImageUndo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonImageRedo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupImage = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple2 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonDimensions = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonGenerateMipMaps = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSetAlpha = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupImageFormat = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonImageFormat = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuImageFormats = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.ButtonImageType = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuImageType = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.Item2DImage = new System.Windows.Forms.ToolStripMenuItem();
        this.ItemCubeMap = new System.Windows.Forms.ToolStripMenuItem();
        this.Item3DImage = new System.Windows.Forms.ToolStripMenuItem();
        this.CheckPremultipliedAlpha = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator3 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
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
        this.TabEffects = new Krypton.Ribbon.KryptonRibbonTab();
        this.GroupFinalize = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple5 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonFxApply = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFxCancel = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupImageFilters = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple4 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonGaussBlur = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFxSharpen = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupLines4 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonFxEmboss = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFxEdgeDetect = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupImageColor = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple6 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonGrayScale = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFxInvert = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupLines1 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonFxBurn = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFxDodge = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFxOneBit = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFxPosterize = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.MenuExternalEdit = new System.Windows.Forms.ContextMenuStrip(this.components);
        ((System.ComponentModel.ISupportInitialize)(this.RibbonImageContent)).BeginInit();
        this.MenuImageType.SuspendLayout();
        this.MenuZoom.SuspendLayout();
        this.SuspendLayout();
        // 
        // RibbonImageContent
        // 
        this.RibbonImageContent.AllowFormIntegrate = true;
        this.RibbonImageContent.InDesignHelperMode = true;
        this.RibbonImageContent.Name = "RibbonImageContent";
        this.RibbonImageContent.RibbonContexts.AddRange(new Krypton.Ribbon.KryptonRibbonContext[] {
        this.ContextFx});
        this.RibbonImageContent.RibbonTabs.AddRange(new Krypton.Ribbon.KryptonRibbonTab[] {
        this.TabImage,
        this.TabEffects});
        this.RibbonImageContent.SelectedContext = null;
        this.RibbonImageContent.SelectedTab = this.TabImage;
        this.RibbonImageContent.Size = new System.Drawing.Size(1293, 115);
        this.RibbonImageContent.TabIndex = 0;
        // 
        // ContextFx
        // 
        this.ContextFx.ContextColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
        this.ContextFx.ContextName = "ContextFx";
        this.ContextFx.ContextTitle = "FX";
        // 
        // TabImage
        // 
        this.TabImage.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.GroupImageFile,
        this.GroupEdit,
        this.GroupImage});
        this.TabImage.KeyTip = "I";
        this.TabImage.Text = "Image";
        // 
        // GroupImageFile
        // 
        this.GroupImageFile.AllowCollapsed = false;
        this.GroupImageFile.DialogBoxLauncher = false;
        this.GroupImageFile.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple1,
        this.kryptonRibbonGroupSeparator1,
        this.GroupCodec});
        this.GroupImageFile.KeyTipGroup = "F";
        this.GroupImageFile.TextLine1 = "File";
        // 
        // kryptonRibbonGroupTriple1
        // 
        this.kryptonRibbonGroupTriple1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonSaveImage});
        this.kryptonRibbonGroupTriple1.MinimumSize = Krypton.Ribbon.GroupItemSize.Large;
        // 
        // ButtonSaveImage
        // 
        this.ButtonSaveImage.Enabled = false;
        this.ButtonSaveImage.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.save_content_48x48;
        this.ButtonSaveImage.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.save_content_16x16;
        this.ButtonSaveImage.KeyTip = "S";
        this.ButtonSaveImage.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
        this.ButtonSaveImage.TextLine1 = "Save";
        this.ButtonSaveImage.TextLine2 = " Image";
        this.ButtonSaveImage.ToolTipBody = "Updates the image file in the file system with the current changes.";
        this.ButtonSaveImage.ToolTipTitle = "Save Image";
        this.ButtonSaveImage.Click += new System.EventHandler(this.ButtonSaveImage_Click);
        // 
        // GroupCodec
        // 
        this.GroupCodec.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonImport,
        this.ButtonExport});
        // 
        // ButtonImport
        // 
        this.ButtonImport.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.import_image_48x48;
        this.ButtonImport.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.import_image_16x16;
        this.ButtonImport.KeyTip = "I";
        this.ButtonImport.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        | System.Windows.Forms.Keys.I)));
        this.ButtonImport.TextLine1 = "Import";
        this.ButtonImport.ToolTipBody = resources.GetString("ButtonImport.ToolTipBody");
        this.ButtonImport.ToolTipTitle = "Import Image";
        this.ButtonImport.Click += new System.EventHandler(this.ButtonImport_Click);
        // 
        // ButtonExport
        // 
        this.ButtonExport.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.ButtonExport.ContextMenuStrip = this.MenuCodecs;
        this.ButtonExport.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.export_image_48x48;
        this.ButtonExport.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.export_image_16x16;
        this.ButtonExport.KeyTip = "E";
        this.ButtonExport.TextLine1 = "Export";
        this.ButtonExport.ToolTipBody = resources.GetString("ButtonExport.ToolTipBody");
        this.ButtonExport.ToolTipTitle = "Export Image";
        // 
        // MenuCodecs
        // 
        this.MenuCodecs.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MenuCodecs.Name = "MenuCodecs";
        this.MenuCodecs.Size = new System.Drawing.Size(61, 4);
        // 
        // GroupEdit
        // 
        this.GroupEdit.AllowCollapsed = false;
        this.GroupEdit.DialogBoxLauncher = false;
        this.GroupEdit.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple3,
        this.kryptonRibbonGroupSeparator2,
        this.kryptonRibbonGroupLines2});
        this.GroupEdit.KeyTipGroup = "E";
        this.GroupEdit.TextLine1 = "Edit";
        // 
        // kryptonRibbonGroupTriple3
        // 
        this.kryptonRibbonGroupTriple3.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonEditInApp,
        this.ButtonFx});
        // 
        // ButtonEditInApp
        // 
        this.ButtonEditInApp.ButtonType = Krypton.Ribbon.GroupButtonType.Split;
        this.ButtonEditInApp.ContextMenuStrip = this.MenuExternalEdit;
        this.ButtonEditInApp.Enabled = false;
        this.ButtonEditInApp.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.edit_image_48x48;
        this.ButtonEditInApp.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.edit_image_16x16;
        this.ButtonEditInApp.KeyTip = "E";
        this.ButtonEditInApp.TextLine1 = "Edit In";
        this.ButtonEditInApp.TextLine2 = "Application";
        this.ButtonEditInApp.ToolTipBody = "Launches the associated application and opens the image \r\nfor editing.\r\n\r\nWhen th" +
"e image editor application is launched, Gorgon\'s \r\nUI will remain locked until t" +
"he application exits.\r\n";
        this.ButtonEditInApp.ToolTipTitle = "Edit In Application";
        this.ButtonEditInApp.Click += new System.EventHandler(this.ButtonEditInApp_Click);
        // 
        // ButtonFx
        // 
        this.ButtonFx.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_effect_48x48;
        this.ButtonFx.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_effect_16x16;
        this.ButtonFx.KeyTip = "F";
        this.ButtonFx.TextLine1 = "FX";
        this.ButtonFx.ToolTipBody = "Provides effects to apply to the current image array/depth/mip level.";
        this.ButtonFx.ToolTipTitle = "FX";
        this.ButtonFx.Click += new System.EventHandler(this.ButtonFx_Click);
        // 
        // kryptonRibbonGroupLines2
        // 
        this.kryptonRibbonGroupLines2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonImageUndo,
        this.ButtonImageRedo});
        // 
        // ButtonImageUndo
        // 
        this.ButtonImageUndo.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.undo_48x48;
        this.ButtonImageUndo.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.undo_16x16;
        this.ButtonImageUndo.KeyTip = "Z";
        this.ButtonImageUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
        this.ButtonImageUndo.TextLine1 = "Undo";
        this.ButtonImageUndo.ToolTipBody = "Reverts the previous change made to the image.";
        this.ButtonImageUndo.ToolTipTitle = "Undo";
        this.ButtonImageUndo.Click += new System.EventHandler(this.ButtonImageUndo_Click);
        // 
        // ButtonImageRedo
        // 
        this.ButtonImageRedo.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.redo_48x48;
        this.ButtonImageRedo.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.redo_16x16;
        this.ButtonImageRedo.KeyTip = "Y";
        this.ButtonImageRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
        this.ButtonImageRedo.TextLine1 = "Redo";
        this.ButtonImageRedo.ToolTipBody = "Restores the next change made to the image.";
        this.ButtonImageRedo.ToolTipTitle = "Redo";
        this.ButtonImageRedo.Click += new System.EventHandler(this.ButtonImageRedo_Click);
        // 
        // GroupImage
        // 
        this.GroupImage.AllowCollapsed = false;
        this.GroupImage.DialogBoxLauncher = false;
        this.GroupImage.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple2,
        this.GroupImageFormat,
        this.kryptonRibbonGroupSeparator3,
        this.kryptonRibbonGroupLines3});
        this.GroupImage.KeyTipGroup = "I";
        this.GroupImage.TextLine1 = "Image";
        // 
        // kryptonRibbonGroupTriple2
        // 
        this.kryptonRibbonGroupTriple2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonDimensions,
        this.ButtonGenerateMipMaps,
        this.ButtonSetAlpha});
        // 
        // ButtonDimensions
        // 
        this.ButtonDimensions.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_size_48x48;
        this.ButtonDimensions.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_size_16x16;
        this.ButtonDimensions.KeyTip = "D";
        this.ButtonDimensions.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        | System.Windows.Forms.Keys.D)));
        this.ButtonDimensions.TextLine1 = "Dimensions";
        this.ButtonDimensions.ToolTipBody = "Alters the width, height (and depth if applicable) of the image.  This is also \r\n" +
"used to update the number of array indices (for 2D and cube images), and \r\nthe n" +
"umber of mip map levels for the image.";
        this.ButtonDimensions.ToolTipTitle = "Image Dimensions";
        this.ButtonDimensions.Click += new System.EventHandler(this.ButtonDimensions_Click);
        // 
        // ButtonGenerateMipMaps
        // 
        this.ButtonGenerateMipMaps.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.generate_mip_maps_48x48;
        this.ButtonGenerateMipMaps.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.generate_mip_maps_16x16;
        this.ButtonGenerateMipMaps.KeyTip = "G";
        this.ButtonGenerateMipMaps.TextLine1 = "Generate";
        this.ButtonGenerateMipMaps.TextLine2 = "Mip Maps";
        this.ButtonGenerateMipMaps.ToolTipBody = "Automatically generates the image data for each mip map level in the image.";
        this.ButtonGenerateMipMaps.ToolTipTitle = "Generate Mip Map Levels";
        this.ButtonGenerateMipMaps.Click += new System.EventHandler(this.ButtonGenerateMipMaps_Click);
        // 
        // ButtonSetAlpha
        // 
        this.ButtonSetAlpha.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.opacity_48x48;
        this.ButtonSetAlpha.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.opactiy_16x16;
        this.ButtonSetAlpha.KeyTip = "A";
        this.ButtonSetAlpha.TextLine1 = "Set Alpha";
        this.ButtonSetAlpha.TextLine2 = "Channel";
        this.ButtonSetAlpha.ToolTipBody = "Sets the alpha channel for the image to a specified value.";
        this.ButtonSetAlpha.ToolTipTitle = "Set Alpha";
        this.ButtonSetAlpha.Click += new System.EventHandler(this.ButtonSetAlpha_Click);
        // 
        // GroupImageFormat
        // 
        this.GroupImageFormat.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonImageFormat,
        this.ButtonImageType,
        this.CheckPremultipliedAlpha});
        this.GroupImageFormat.MinimumSize = Krypton.Ribbon.GroupItemSize.Medium;
        // 
        // ButtonImageFormat
        // 
        this.ButtonImageFormat.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.ButtonImageFormat.ContextMenuStrip = this.MenuImageFormats;
        this.ButtonImageFormat.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.pixel_format_48x48;
        this.ButtonImageFormat.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.pixel_format_16x16;
        this.ButtonImageFormat.KeyTip = "F";
        this.ButtonImageFormat.TextLine1 = "Format";
        this.ButtonImageFormat.ToolTipBody = "Changes the layout of the pixel components in the image data. \r\n\r\nCertain codecs " +
"can only use specific pixel layouts, and as such, this list will change dependin" +
"g on the codec used.";
        this.ButtonImageFormat.ToolTipTitle = "Image Pixel Format";
        // 
        // MenuImageFormats
        // 
        this.MenuImageFormats.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MenuImageFormats.Name = "MenuImageFormats";
        this.MenuImageFormats.Size = new System.Drawing.Size(61, 4);
        // 
        // ButtonImageType
        // 
        this.ButtonImageType.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.ButtonImageType.ContextMenuStrip = this.MenuImageType;
        this.ButtonImageType.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_dimensions_48x48;
        this.ButtonImageType.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_dimensions_16x16;
        this.ButtonImageType.KeyTip = "T";
        this.ButtonImageType.TextLine1 = "Type";
        this.ButtonImageType.ToolTipBody = resources.GetString("ButtonImageType.ToolTipBody");
        this.ButtonImageType.ToolTipTitle = "Image Type";
        // 
        // MenuImageType
        // 
        this.MenuImageType.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MenuImageType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.Item2DImage,
        this.ItemCubeMap,
        this.Item3DImage});
        this.MenuImageType.Name = "MenuImageType";
        this.MenuImageType.Size = new System.Drawing.Size(130, 70);
        // 
        // Item2DImage
        // 
        this.Item2DImage.Checked = true;
        this.Item2DImage.CheckOnClick = true;
        this.Item2DImage.CheckState = System.Windows.Forms.CheckState.Checked;
        this.Item2DImage.Name = "Item2DImage";
        this.Item2DImage.Size = new System.Drawing.Size(129, 22);
        this.Item2DImage.Text = "2D Image";
        this.Item2DImage.Click += new System.EventHandler(this.ItemImageType_Click);
        // 
        // ItemCubeMap
        // 
        this.ItemCubeMap.CheckOnClick = true;
        this.ItemCubeMap.Name = "ItemCubeMap";
        this.ItemCubeMap.Size = new System.Drawing.Size(129, 22);
        this.ItemCubeMap.Text = "Cube Map";
        this.ItemCubeMap.Click += new System.EventHandler(this.ItemImageType_Click);
        // 
        // Item3DImage
        // 
        this.Item3DImage.CheckOnClick = true;
        this.Item3DImage.Name = "Item3DImage";
        this.Item3DImage.Size = new System.Drawing.Size(129, 22);
        this.Item3DImage.Text = "3D Image";
        this.Item3DImage.Click += new System.EventHandler(this.ItemImageType_Click);
        // 
        // CheckPremultipliedAlpha
        // 
        this.CheckPremultipliedAlpha.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.CheckPremultipliedAlpha.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.alpha_48x48;
        this.CheckPremultipliedAlpha.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.alpha_16x16;
        this.CheckPremultipliedAlpha.KeyTip = "P";
        this.CheckPremultipliedAlpha.TextLine1 = "Premultiplied";
        this.CheckPremultipliedAlpha.TextLine2 = "Alpha";
        this.CheckPremultipliedAlpha.ToolTipBody = "Indicates that this texture should use premultiplied alpha.";
        this.CheckPremultipliedAlpha.ToolTipTitle = "Premultiplied Alpha";
        this.CheckPremultipliedAlpha.Click += new System.EventHandler(this.CheckPremultipliedAlpha_Click);
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
        this.ButtonZoom.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.zoom_48x48;
        this.ButtonZoom.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.zoom_16x16;
        this.ButtonZoom.KeyTip = "Z";
        this.ButtonZoom.TextLine1 = "Zoom";
        this.ButtonZoom.ToolTipBody = resources.GetString("ButtonZoom.ToolTipBody");
        this.ButtonZoom.ToolTipTitle = "Zoom";
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
        // TabEffects
        // 
        this.TabEffects.ContextName = "ContextFx";
        this.TabEffects.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.GroupFinalize,
        this.GroupImageFilters,
        this.GroupImageColor});
        this.TabEffects.KeyTip = "E";
        this.TabEffects.Text = "Image Effects";
        // 
        // GroupFinalize
        // 
        this.GroupFinalize.AllowCollapsed = false;
        this.GroupFinalize.DialogBoxLauncher = false;
        this.GroupFinalize.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple5});
        this.GroupFinalize.KeyTipGroup = "F";
        this.GroupFinalize.TextLine1 = "Finalize";
        // 
        // kryptonRibbonGroupTriple5
        // 
        this.kryptonRibbonGroupTriple5.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonFxApply,
        this.ButtonFxCancel});
        // 
        // ButtonFxApply
        // 
        this.ButtonFxApply.Enabled = false;
        this.ButtonFxApply.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.confirm_48x48;
        this.ButtonFxApply.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.confirm_16x16;
        this.ButtonFxApply.KeyTip = "A";
        this.ButtonFxApply.TextLine1 = "Apply";
        this.ButtonFxApply.ToolTipBody = "Applies the effects to the current image array/depth/mip level.";
        this.ButtonFxApply.ToolTipTitle = "Finish Applying Effects";
        this.ButtonFxApply.Click += new System.EventHandler(this.ButtonFxApply_Click);
        // 
        // ButtonFxCancel
        // 
        this.ButtonFxCancel.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.cancel_48x48;
        this.ButtonFxCancel.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.cancel_16x16;
        this.ButtonFxCancel.KeyTip = "C";
        this.ButtonFxCancel.TextLine1 = "Cancel";
        this.ButtonFxCancel.ToolTipBody = "Cancels the effect operations.";
        this.ButtonFxCancel.ToolTipTitle = "Cancel";
        this.ButtonFxCancel.Click += new System.EventHandler(this.ButtonFxCancel_Click);
        // 
        // GroupImageFilters
        // 
        this.GroupImageFilters.AllowCollapsed = false;
        this.GroupImageFilters.DialogBoxLauncher = false;
        this.GroupImageFilters.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple4,
        this.kryptonRibbonGroupLines4});
        this.GroupImageFilters.KeyTipGroup = "L";
        this.GroupImageFilters.TextLine1 = "Filters";
        // 
        // kryptonRibbonGroupTriple4
        // 
        this.kryptonRibbonGroupTriple4.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonGaussBlur,
        this.ButtonFxSharpen});
        // 
        // ButtonGaussBlur
        // 
        this.ButtonGaussBlur.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.gauss_blur_48x48;
        this.ButtonGaussBlur.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.gauss_blur_16x16;
        this.ButtonGaussBlur.TextLine1 = "Gaussian";
        this.ButtonGaussBlur.TextLine2 = "Blur";
        this.ButtonGaussBlur.ToolTipBody = "Blurs the image using a gaussian blur algorithm.";
        this.ButtonGaussBlur.ToolTipTitle = "Gaussian Blur";
        this.ButtonGaussBlur.Click += new System.EventHandler(this.ButtonFxGaussBlur_Click);
        // 
        // ButtonFxSharpen
        // 
        this.ButtonFxSharpen.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.sharpen_48x48;
        this.ButtonFxSharpen.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.sharpen_16x16;
        this.ButtonFxSharpen.KeyTip = "S";
        this.ButtonFxSharpen.TextLine1 = "Sharpen";
        this.ButtonFxSharpen.ToolTipBody = "Sharpens the image by making the edges between colors more \r\npronounced.";
        this.ButtonFxSharpen.ToolTipTitle = "Sharpen";
        this.ButtonFxSharpen.Click += new System.EventHandler(this.ButtonFxSharpen_Click);
        // 
        // kryptonRibbonGroupLines4
        // 
        this.kryptonRibbonGroupLines4.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonFxEmboss,
        this.ButtonFxEdgeDetect});
        // 
        // ButtonFxEmboss
        // 
        this.ButtonFxEmboss.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.emboss_48x48;
        this.ButtonFxEmboss.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.emboss_16x16;
        this.ButtonFxEmboss.KeyTip = "M";
        this.ButtonFxEmboss.TextLine1 = "Emboss";
        this.ButtonFxEmboss.ToolTipBody = "Applies an emboss effect to the image.";
        this.ButtonFxEmboss.ToolTipTitle = "Emboss";
        this.ButtonFxEmboss.Click += new System.EventHandler(this.ButtonFxEmboss_Click);
        // 
        // ButtonFxEdgeDetect
        // 
        this.ButtonFxEdgeDetect.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.edge_detect_48x48;
        this.ButtonFxEdgeDetect.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.edge_detect_16x16;
        this.ButtonFxEdgeDetect.KeyTip = "E";
        this.ButtonFxEdgeDetect.TextLine1 = "Edge";
        this.ButtonFxEdgeDetect.TextLine2 = "Detect";
        this.ButtonFxEdgeDetect.ToolTipBody = "Applies a filter to detect the edges of the image.";
        this.ButtonFxEdgeDetect.ToolTipTitle = "Edge Detect";
        this.ButtonFxEdgeDetect.Click += new System.EventHandler(this.ButtonFxEdgeDetect_Click);
        // 
        // GroupImageColor
        // 
        this.GroupImageColor.AllowCollapsed = false;
        this.GroupImageColor.DialogBoxLauncher = false;
        this.GroupImageColor.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple6,
        this.kryptonRibbonGroupLines1});
        this.GroupImageColor.KeyTipGroup = "C";
        this.GroupImageColor.TextLine1 = "Color";
        // 
        // kryptonRibbonGroupTriple6
        // 
        this.kryptonRibbonGroupTriple6.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonGrayScale,
        this.ButtonFxInvert});
        // 
        // ButtonGrayScale
        // 
        this.ButtonGrayScale.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.grayscale_48x48;
        this.ButtonGrayScale.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.grayscale_16x16;
        this.ButtonGrayScale.KeyTip = "G";
        this.ButtonGrayScale.TextLine1 = "Grayscale";
        this.ButtonGrayScale.ToolTipBody = "Removes all color information.";
        this.ButtonGrayScale.ToolTipTitle = "Grayscale";
        this.ButtonGrayScale.Click += new System.EventHandler(this.ButtonGrayScale_Click);
        // 
        // ButtonFxInvert
        // 
        this.ButtonFxInvert.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.invert_48x48;
        this.ButtonFxInvert.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.invert_16x16;
        this.ButtonFxInvert.KeyTip = "I";
        this.ButtonFxInvert.TextLine1 = "Invert";
        this.ButtonFxInvert.ToolTipBody = "Inverts the image colors.";
        this.ButtonFxInvert.ToolTipTitle = "Invert";
        this.ButtonFxInvert.Click += new System.EventHandler(this.ButtonFxInvert_Click);
        // 
        // kryptonRibbonGroupLines1
        // 
        this.kryptonRibbonGroupLines1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonFxBurn,
        this.ButtonFxDodge,
        this.ButtonFxOneBit,
        this.ButtonFxPosterize});
        // 
        // ButtonFxBurn
        // 
        this.ButtonFxBurn.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.burn_48x48;
        this.ButtonFxBurn.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.burn_16x16;
        this.ButtonFxBurn.TextLine1 = "Burn";
        this.ButtonFxBurn.ToolTipBody = "Apply a burn effect to the image.";
        this.ButtonFxBurn.ToolTipTitle = "Burn";
        this.ButtonFxBurn.Click += new System.EventHandler(this.ButtonFxBurn_Click);
        // 
        // ButtonFxDodge
        // 
        this.ButtonFxDodge.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.dodge_48x48;
        this.ButtonFxDodge.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.dodge_16x16;
        this.ButtonFxDodge.KeyTip = "D";
        this.ButtonFxDodge.TextLine1 = "Dodge";
        this.ButtonFxDodge.ToolTipBody = "Apply a dodge effect to the image.";
        this.ButtonFxDodge.ToolTipTitle = "Dodge";
        this.ButtonFxDodge.Click += new System.EventHandler(this.ButtonFxDodge_Click);
        // 
        // ButtonFxOneBit
        // 
        this.ButtonFxOneBit.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.onebit_48x48;
        this.ButtonFxOneBit.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.onebit_16x16;
        this.ButtonFxOneBit.KeyTip = "1";
        this.ButtonFxOneBit.TextLine1 = "1 bit";
        this.ButtonFxOneBit.ToolTipBody = "Converts the image to simulate 1 bit (2 colors) per pixel.";
        this.ButtonFxOneBit.ToolTipTitle = "One bit";
        this.ButtonFxOneBit.Click += new System.EventHandler(this.ButtonFxOneBit_Click);
        // 
        // ButtonFxPosterize
        // 
        this.ButtonFxPosterize.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.posterize_48x48;
        this.ButtonFxPosterize.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.posterize_16x16;
        this.ButtonFxPosterize.KeyTip = "P";
        this.ButtonFxPosterize.TextLine1 = "Posterize";
        this.ButtonFxPosterize.ToolTipBody = "Applies a posterize effect to the image.";
        this.ButtonFxPosterize.ToolTipTitle = "Posterize";
        this.ButtonFxPosterize.Click += new System.EventHandler(this.ButtonFxPosterize_Click);
        // 
        // MenuExternalEdit
        // 
        this.MenuExternalEdit.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MenuExternalEdit.Name = "MenuExternalEdit";
        this.MenuExternalEdit.Size = new System.Drawing.Size(61, 4);
        // 
        // FormRibbon
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1293, 399);
        this.Controls.Add(this.RibbonImageContent);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "FormRibbon";
        this.Text = "FormRibbon";
        ((System.ComponentModel.ISupportInitialize)(this.RibbonImageContent)).EndInit();
        this.MenuImageType.ResumeLayout(false);
        this.MenuZoom.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private Krypton.Ribbon.KryptonRibbonTab TabImage;
    private Krypton.Ribbon.KryptonRibbonGroup GroupImageFile;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveImage;
    internal Krypton.Ribbon.KryptonRibbon RibbonImageContent;
    private Krypton.Ribbon.KryptonRibbonGroup GroupEdit;
    private Krypton.Ribbon.KryptonRibbonGroupLines GroupCodec;
    private System.Windows.Forms.ContextMenuStrip MenuCodecs;
    private Krypton.Ribbon.KryptonRibbonGroupLines GroupImageFormat;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageFormat;
    private System.Windows.Forms.ContextMenuStrip MenuImageFormats;
    private Krypton.Ribbon.KryptonRibbonGroup GroupImage;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonDimensions;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageType;
    private System.Windows.Forms.ContextMenuStrip MenuImageType;
    private System.Windows.Forms.ToolStripMenuItem Item2DImage;
    private System.Windows.Forms.ToolStripMenuItem ItemCubeMap;
    private System.Windows.Forms.ToolStripMenuItem Item3DImage;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple3;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonGenerateMipMaps;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonEditInApp;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonImport;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageUndo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageRedo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonExport;
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
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator1;
    private Krypton.Ribbon.KryptonRibbonGroupButton CheckPremultipliedAlpha;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSetAlpha;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFx;
    private Krypton.Ribbon.KryptonRibbonGroup GroupImageFilters;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple4;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonGaussBlur;
    private Krypton.Ribbon.KryptonRibbonContext ContextFx;
    private Krypton.Ribbon.KryptonRibbonGroup GroupFinalize;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple5;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxApply;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxCancel;
    internal Krypton.Ribbon.KryptonRibbonTab TabEffects;
    private Krypton.Ribbon.KryptonRibbonGroup GroupImageColor;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple6;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonGrayScale;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxBurn;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxDodge;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxInvert;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxPosterize;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxOneBit;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines4;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxEdgeDetect;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxEmboss;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFxSharpen;
    private System.Windows.Forms.ContextMenuStrip MenuExternalEdit;
}