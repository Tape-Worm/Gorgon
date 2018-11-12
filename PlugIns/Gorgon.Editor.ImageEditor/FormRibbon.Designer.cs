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
            this.ButtonCodec = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuCodecs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ButtonImport = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupImage = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonGenerateMipMaps = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonEditInApp = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupLines2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.kryptonRibbonGroupButton6 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonRedo = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupSeparator2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.GroupImageFormat = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonImageFormat = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuImageFormats = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.kryptonRibbonGroupButton2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuImageType = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dImageCubeMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dImageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.GroupDmensions = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.kryptonRibbonGroupButton1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupLines1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.LabelArrayCount = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLabel();
            this.NumericArrayCount = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupNumericUpDown();
            this.kryptonRibbonGroupLabel1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLabel();
            this.NumericMipCount = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.RibbonImageContent)).BeginInit();
            this.MenuImageType.SuspendLayout();
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
            this.RibbonImageContent.Size = new System.Drawing.Size(1161, 115);
            this.RibbonImageContent.TabIndex = 0;
            // 
            // TabImage
            // 
            this.TabImage.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
            this.GroupImageFile,
            this.GroupImage,
            this.GroupDmensions});
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
            // 
            // GroupCodec
            // 
            this.GroupCodec.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonCodec,
            this.ButtonImport});
            // 
            // ButtonCodec
            // 
            this.ButtonCodec.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonCodec.ContextMenuStrip = this.MenuCodecs;
            this.ButtonCodec.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.codec_48x48;
            this.ButtonCodec.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.codec_16x16;
            this.ButtonCodec.KeyTip = "C";
            this.ButtonCodec.TextLine1 = "Codec";
            this.ButtonCodec.ToolTipBody = "Sets the codec to use when writing the image back to the file system.";
            this.ButtonCodec.ToolTipTitle = "Image Codec";
            // 
            // MenuCodecs
            // 
            this.MenuCodecs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuCodecs.Name = "MenuCodecs";
            this.MenuCodecs.Size = new System.Drawing.Size(61, 4);
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
            // 
            // GroupImage
            // 
            this.GroupImage.AllowCollapsed = false;
            this.GroupImage.DialogBoxLauncher = false;
            this.GroupImage.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple3,
            this.kryptonRibbonGroupLines2,
            this.kryptonRibbonGroupSeparator2,
            this.GroupImageFormat});
            this.GroupImage.KeyTipGroup = "E";
            this.GroupImage.TextLine1 = "Edit";
            // 
            // kryptonRibbonGroupTriple3
            // 
            this.kryptonRibbonGroupTriple3.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonGenerateMipMaps,
            this.ButtonEditInApp});
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
            // 
            // ButtonEditInApp
            // 
            this.ButtonEditInApp.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.edit_image_48x48;
            this.ButtonEditInApp.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.edit_image_16x16;
            this.ButtonEditInApp.KeyTip = "E";
            this.ButtonEditInApp.TextLine1 = "Edit In";
            this.ButtonEditInApp.TextLine2 = "Application";
            this.ButtonEditInApp.ToolTipBody = "Launches the associated application and opens the image for editing.\r\n\r\nWhen the " +
    "image editor application is launched, Gorgon\'s UI will remain locked until the a" +
    "pplication exits.\r\n";
            this.ButtonEditInApp.ToolTipTitle = "Edit In Application";
            // 
            // kryptonRibbonGroupLines2
            // 
            this.kryptonRibbonGroupLines2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.kryptonRibbonGroupButton6,
            this.ButtonRedo});
            // 
            // kryptonRibbonGroupButton6
            // 
            this.kryptonRibbonGroupButton6.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.undo_48x48;
            this.kryptonRibbonGroupButton6.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.undo_16x16;
            this.kryptonRibbonGroupButton6.KeyTip = "Z";
            this.kryptonRibbonGroupButton6.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.kryptonRibbonGroupButton6.TextLine1 = "Undo";
            this.kryptonRibbonGroupButton6.ToolTipBody = "Reverts the previous change made to the image.";
            this.kryptonRibbonGroupButton6.ToolTipTitle = "Undo";
            // 
            // ButtonRedo
            // 
            this.ButtonRedo.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.redo_48x48;
            this.ButtonRedo.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.redo_16x16;
            this.ButtonRedo.KeyTip = "Y";
            this.ButtonRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.ButtonRedo.TextLine1 = "Redo";
            this.ButtonRedo.ToolTipBody = "Restores the next change made to the image.";
            this.ButtonRedo.ToolTipTitle = "Redo";
            // 
            // GroupImageFormat
            // 
            this.GroupImageFormat.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonImageFormat,
            this.kryptonRibbonGroupButton2});
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
            // kryptonRibbonGroupButton2
            // 
            this.kryptonRibbonGroupButton2.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.kryptonRibbonGroupButton2.ContextMenuStrip = this.MenuImageType;
            this.kryptonRibbonGroupButton2.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_dimensions_48x48;
            this.kryptonRibbonGroupButton2.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_dimensions_16x16;
            this.kryptonRibbonGroupButton2.KeyTip = "T";
            this.kryptonRibbonGroupButton2.TextLine1 = "Type";
            this.kryptonRibbonGroupButton2.ToolTipBody = resources.GetString("kryptonRibbonGroupButton2.ToolTipBody");
            this.kryptonRibbonGroupButton2.ToolTipTitle = "Image Type";
            // 
            // MenuImageType
            // 
            this.MenuImageType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuImageType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dImageToolStripMenuItem,
            this.dImageCubeMapToolStripMenuItem,
            this.dImageToolStripMenuItem1});
            this.MenuImageType.Name = "MenuImageType";
            this.MenuImageType.Size = new System.Drawing.Size(130, 70);
            // 
            // dImageToolStripMenuItem
            // 
            this.dImageToolStripMenuItem.Checked = true;
            this.dImageToolStripMenuItem.CheckOnClick = true;
            this.dImageToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dImageToolStripMenuItem.Name = "dImageToolStripMenuItem";
            this.dImageToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.dImageToolStripMenuItem.Text = "2D Image";
            // 
            // dImageCubeMapToolStripMenuItem
            // 
            this.dImageCubeMapToolStripMenuItem.CheckOnClick = true;
            this.dImageCubeMapToolStripMenuItem.Name = "dImageCubeMapToolStripMenuItem";
            this.dImageCubeMapToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.dImageCubeMapToolStripMenuItem.Text = "Cube Map";
            // 
            // dImageToolStripMenuItem1
            // 
            this.dImageToolStripMenuItem1.CheckOnClick = true;
            this.dImageToolStripMenuItem1.Name = "dImageToolStripMenuItem1";
            this.dImageToolStripMenuItem1.Size = new System.Drawing.Size(129, 22);
            this.dImageToolStripMenuItem1.Text = "3D Image";
            // 
            // GroupDmensions
            // 
            this.GroupDmensions.AllowCollapsed = false;
            this.GroupDmensions.DialogBoxLauncher = false;
            this.GroupDmensions.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple2,
            this.kryptonRibbonGroupLines1});
            this.GroupDmensions.KeyTipGroup = "D";
            this.GroupDmensions.TextLine1 = "Dimensions";
            // 
            // kryptonRibbonGroupTriple2
            // 
            this.kryptonRibbonGroupTriple2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.kryptonRibbonGroupButton1});
            // 
            // kryptonRibbonGroupButton1
            // 
            this.kryptonRibbonGroupButton1.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_size_48x48;
            this.kryptonRibbonGroupButton1.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.image_size_16x16;
            this.kryptonRibbonGroupButton1.KeyTip = "Z";
            this.kryptonRibbonGroupButton1.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Z)));
            this.kryptonRibbonGroupButton1.TextLine1 = "Size";
            this.kryptonRibbonGroupButton1.ToolTipTitle = "Image Size";
            // 
            // kryptonRibbonGroupLines1
            // 
            this.kryptonRibbonGroupLines1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.LabelArrayCount,
            this.NumericArrayCount,
            this.kryptonRibbonGroupLabel1,
            this.NumericMipCount});
            this.kryptonRibbonGroupLines1.MaximumSize = ComponentFactory.Krypton.Ribbon.GroupItemSize.Medium;
            // 
            // LabelArrayCount
            // 
            this.LabelArrayCount.TextLine1 = "Array Count:";
            // 
            // NumericArrayCount
            // 
            this.NumericArrayCount.MaximumSize = new System.Drawing.Size(80, 0);
            this.NumericArrayCount.MinimumSize = new System.Drawing.Size(80, 0);
            this.NumericArrayCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // kryptonRibbonGroupLabel1
            // 
            this.kryptonRibbonGroupLabel1.TextLine1 = "Mip Count:";
            // 
            // NumericMipCount
            // 
            this.NumericMipCount.MaximumSize = new System.Drawing.Size(80, 0);
            this.NumericMipCount.MinimumSize = new System.Drawing.Size(80, 0);
            this.NumericMipCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // FormRibbon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1161, 296);
            this.Controls.Add(this.RibbonImageContent);
            this.Name = "FormRibbon";
            this.Text = "FormRibbon";
            ((System.ComponentModel.ISupportInitialize)(this.RibbonImageContent)).EndInit();
            this.MenuImageType.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabImage;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupImageFile;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveImage;
        internal ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonImageContent;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupImage;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines GroupCodec;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonCodec;
        private System.Windows.Forms.ContextMenuStrip MenuCodecs;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines GroupImageFormat;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImageFormat;
        private System.Windows.Forms.ContextMenuStrip MenuImageFormats;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupDmensions;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton kryptonRibbonGroupButton1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton kryptonRibbonGroupButton2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLabel LabelArrayCount;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupNumericUpDown NumericArrayCount;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLabel kryptonRibbonGroupLabel1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupNumericUpDown NumericMipCount;
        private System.Windows.Forms.ContextMenuStrip MenuImageType;
        private System.Windows.Forms.ToolStripMenuItem dImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dImageCubeMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dImageToolStripMenuItem1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonGenerateMipMaps;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonEditInApp;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImport;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton kryptonRibbonGroupButton6;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonRedo;
    }
}