namespace Gorgon.Editor.ImageEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRibbon));
            this.RibbonImageContent = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.TabImage = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.GroupImageFile = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonSaveImage = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupSeparator1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.GroupCodec = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonImport = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonExport = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuCodecs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.GroupEdit = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonEditInApp = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupSeparator2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.kryptonRibbonGroupLines2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonImageUndo = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonImageRedo = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupImage = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonDimensions = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonGenerateMipMaps = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupImageFormat = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonImageFormat = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuImageFormats = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ButtonImageType = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuImageType = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Item2DImage = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemCubeMap = new System.Windows.Forms.ToolStripMenuItem();
            this.Item3DImage = new System.Windows.Forms.ToolStripMenuItem();
            this.kryptonRibbonGroupSeparator3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.kryptonRibbonGroupLines3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonZoom = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
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
            this.RibbonImageContent.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
            this.TabImage});
            this.RibbonImageContent.SelectedContext = null;
            this.RibbonImageContent.SelectedTab = this.TabImage;
            this.RibbonImageContent.Size = new System.Drawing.Size(1293, 115);
            this.RibbonImageContent.TabIndex = 0;
            // 
            // TabImage
            // 
            this.TabImage.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
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
            this.GroupImageFile.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple1,
            this.kryptonRibbonGroupSeparator1,
            this.GroupCodec});
            this.GroupImageFile.KeyTipGroup = "F";
            this.GroupImageFile.TextLine1 = "File";
            // 
            // kryptonRibbonGroupTriple1
            // 
            this.kryptonRibbonGroupTriple1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonSaveImage});
            this.kryptonRibbonGroupTriple1.MinimumSize = ComponentFactory.Krypton.Ribbon.GroupItemSize.Large;
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
            this.GroupCodec.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
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
            this.ButtonExport.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonExport.ContextMenuStrip = this.MenuCodecs;
            this.ButtonExport.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.export_image_48x48;
            this.ButtonExport.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.export_image_16x16;
            this.ButtonExport.KeyTip = "E";
            this.ButtonExport.TextLine1 = "Export";
            this.ButtonExport.ToolTipBody = "Exports the image to a file on the disk as the specified codec.\r\n\r\nOnly codecs th" +
    "at support the current pixel format, mip level count, array level count \r\nand im" +
    "age type will appear in this list.";
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
            this.GroupEdit.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple3,
            this.kryptonRibbonGroupSeparator2,
            this.kryptonRibbonGroupLines2});
            this.GroupEdit.KeyTipGroup = "E";
            this.GroupEdit.TextLine1 = "Edit";
            // 
            // kryptonRibbonGroupTriple3
            // 
            this.kryptonRibbonGroupTriple3.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonEditInApp});
            // 
            // ButtonEditInApp
            // 
            this.ButtonEditInApp.Enabled = false;
            this.ButtonEditInApp.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.edit_image_48x48;
            this.ButtonEditInApp.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.edit_image_16x16;
            this.ButtonEditInApp.KeyTip = "E";
            this.ButtonEditInApp.TextLine1 = "Edit In";
            this.ButtonEditInApp.TextLine2 = "Application";
            this.ButtonEditInApp.ToolTipBody = "Launches the associated application and opens the image for editing.\r\n\r\nWhen the " +
    "image editor application is launched, Gorgon\'s UI will remain locked until the a" +
    "pplication exits.\r\n";
            this.ButtonEditInApp.ToolTipTitle = "Edit In Application";
            this.ButtonEditInApp.Click += new System.EventHandler(this.ButtonEditInApp_Click);
            // 
            // kryptonRibbonGroupLines2
            // 
            this.kryptonRibbonGroupLines2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
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
            this.GroupImage.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple2,
            this.GroupImageFormat,
            this.kryptonRibbonGroupSeparator3,
            this.kryptonRibbonGroupLines3});
            this.GroupImage.KeyTipGroup = "I";
            this.GroupImage.TextLine1 = "Image";
            // 
            // kryptonRibbonGroupTriple2
            // 
            this.kryptonRibbonGroupTriple2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonDimensions,
            this.ButtonGenerateMipMaps});
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
    "umber of mip-map levels for the image.";
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
            // GroupImageFormat
            // 
            this.GroupImageFormat.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonImageFormat,
            this.ButtonImageType});
            this.GroupImageFormat.MinimumSize = ComponentFactory.Krypton.Ribbon.GroupItemSize.Medium;
            // 
            // ButtonImageFormat
            // 
            this.ButtonImageFormat.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
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
            this.ButtonImageType.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
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
            // kryptonRibbonGroupLines3
            // 
            this.kryptonRibbonGroupLines3.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonZoom});
            // 
            // ButtonZoom
            // 
            this.ButtonZoom.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonZoom.ContextMenuStrip = this.MenuZoom;
            this.ButtonZoom.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.zoom_48x48;
            this.ButtonZoom.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.zoom_16x16;
            this.ButtonZoom.KeyTip = "Z";
            this.ButtonZoom.TextLine1 = "Zoom";
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
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabImage;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupImageFile;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveImage;
        internal ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonImageContent;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupEdit;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines GroupCodec;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator1;
        private System.Windows.Forms.ContextMenuStrip MenuCodecs;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines GroupImageFormat;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageFormat;
        private System.Windows.Forms.ContextMenuStrip MenuImageFormats;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupImage;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonDimensions;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageType;
        private System.Windows.Forms.ContextMenuStrip MenuImageType;
        private System.Windows.Forms.ToolStripMenuItem Item2DImage;
        private System.Windows.Forms.ToolStripMenuItem ItemCubeMap;
        private System.Windows.Forms.ToolStripMenuItem Item3DImage;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonGenerateMipMaps;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonEditInApp;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImport;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageUndo;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageRedo;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonExport;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonZoom;
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
    }
}