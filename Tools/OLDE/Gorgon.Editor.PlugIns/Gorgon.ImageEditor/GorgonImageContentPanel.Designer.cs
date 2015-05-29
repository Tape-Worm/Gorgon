using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditorPlugIn
{
    partial class GorgonImageContentPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
                DestroyCubeAnimTarget();

		        if (_keyboard != null)
		        {
			        _keyboard.Dispose();
		        }

		        if (_backgroundTexture != null)
		        {
			        _backgroundTexture.Dispose();
		        }

		        if (_font != null)
		        {
			        _font.Dispose();
		        }

		        if (_texture != null)
		        {
			        _texture.Dispose();
		        }

		        _keyboard = null;
		        _backgroundTexture = null;
		        _texture = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GorgonImageContentPanel));
			this.imageFileBrowser = new Gorgon.Editor.EditorFileDialog();
			this.panelTextureDisplay = new Gorgon.UI.GorgonSelectablePanel();
			this.stripImageEditor = new System.Windows.Forms.ToolStrip();
			this.labelImageInfo = new System.Windows.Forms.ToolStripLabel();
			this.sepMip = new System.Windows.Forms.ToolStripSeparator();
			this.buttonPrevMipLevel = new System.Windows.Forms.ToolStripButton();
			this.labelMipLevel = new System.Windows.Forms.ToolStripLabel();
			this.buttonNextMipLevel = new System.Windows.Forms.ToolStripButton();
			this.sepArray = new System.Windows.Forms.ToolStripSeparator();
			this.buttonPrevDepthSlice = new System.Windows.Forms.ToolStripButton();
			this.labelDepthSlice = new System.Windows.Forms.ToolStripLabel();
			this.buttonNextDepthSlice = new System.Windows.Forms.ToolStripButton();
			this.buttonPrevArrayIndex = new System.Windows.Forms.ToolStripButton();
			this.labelArrayIndex = new System.Windows.Forms.ToolStripLabel();
			this.buttonNextArrayIndex = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonActualSize = new System.Windows.Forms.ToolStripButton();
			this.panelTextureArea = new System.Windows.Forms.Panel();
			this.panelHorzScroll = new System.Windows.Forms.Panel();
			this.scrollHorz = new System.Windows.Forms.HScrollBar();
			this.panelVertScroll = new System.Windows.Forms.Panel();
			this.scrollVert = new System.Windows.Forms.VScrollBar();
			this.stripTexture = new System.Windows.Forms.ToolStrip();
			this.buttonSave = new System.Windows.Forms.ToolStripButton();
			this.buttonRevert = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEditFileExternal = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonImport = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemImportFileDisk = new System.Windows.Forms.ToolStripMenuItem();
			this.itemImportFileSystem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonGenerateMips = new System.Windows.Forms.ToolStripButton();
			this.dialogOpenImage = new System.Windows.Forms.OpenFileDialog();
			this.dialogImportImage = new Gorgon.Editor.EditorFileDialog();
			this.PanelDisplay.SuspendLayout();
			this.stripImageEditor.SuspendLayout();
			this.panelTextureArea.SuspendLayout();
			this.panelHorzScroll.SuspendLayout();
			this.panelVertScroll.SuspendLayout();
			this.stripTexture.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelDisplay
			// 
			this.PanelDisplay.Controls.Add(this.panelTextureArea);
			this.PanelDisplay.Controls.Add(this.stripImageEditor);
			this.PanelDisplay.Controls.Add(this.stripTexture);
			this.PanelDisplay.Size = new System.Drawing.Size(806, 606);
			// 
			// imageFileBrowser
			// 
			this.imageFileBrowser.AllowAllFiles = false;
			this.imageFileBrowser.DefaultFileType = "";
			this.imageFileBrowser.Filename = null;
			this.imageFileBrowser.StartDirectory = null;
			this.imageFileBrowser.Text = "Open Image";
			// 
			// panelTextureDisplay
			// 
			this.panelTextureDisplay.AllowDrop = true;
			this.panelTextureDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelTextureDisplay.Location = new System.Drawing.Point(0, 0);
			this.panelTextureDisplay.Name = "panelTextureDisplay";
			this.panelTextureDisplay.ShowFocus = false;
			this.panelTextureDisplay.Size = new System.Drawing.Size(789, 539);
			this.panelTextureDisplay.TabIndex = 0;
			this.panelTextureDisplay.DragDrop += new System.Windows.Forms.DragEventHandler(this.panelTextureDisplay_DragDrop);
			this.panelTextureDisplay.DragEnter += new System.Windows.Forms.DragEventHandler(this.panelTextureDisplay_DragEnter);
			this.panelTextureDisplay.DragOver += new System.Windows.Forms.DragEventHandler(this.panelTextureDisplay_DragOver);
			this.panelTextureDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseDown);
			this.panelTextureDisplay.MouseLeave += new System.EventHandler(this.panelTextureDisplay_MouseLeave);
			this.panelTextureDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseMove);
			this.panelTextureDisplay.Resize += new System.EventHandler(this.panelTextureDisplay_Resize);
			// 
			// stripImageEditor
			// 
			this.stripImageEditor.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.stripImageEditor.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripImageEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelImageInfo,
            this.sepMip,
            this.buttonPrevMipLevel,
            this.labelMipLevel,
            this.buttonNextMipLevel,
            this.sepArray,
            this.buttonPrevDepthSlice,
            this.labelDepthSlice,
            this.buttonNextDepthSlice,
            this.buttonPrevArrayIndex,
            this.labelArrayIndex,
            this.buttonNextArrayIndex,
            this.toolStripSeparator3,
            this.buttonActualSize});
			this.stripImageEditor.Location = new System.Drawing.Point(0, 581);
			this.stripImageEditor.Name = "stripImageEditor";
			this.stripImageEditor.Size = new System.Drawing.Size(806, 25);
			this.stripImageEditor.Stretch = true;
			this.stripImageEditor.TabIndex = 0;
			// 
			// labelImageInfo
			// 
			this.labelImageInfo.Name = "labelImageInfo";
			this.labelImageInfo.Size = new System.Drawing.Size(64, 22);
			this.labelImageInfo.Text = "Image info";
			// 
			// sepMip
			// 
			this.sepMip.Name = "sepMip";
			this.sepMip.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonPrevMipLevel
			// 
			this.buttonPrevMipLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPrevMipLevel.Enabled = false;
			this.buttonPrevMipLevel.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.back_16x16png1;
			this.buttonPrevMipLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPrevMipLevel.Name = "buttonPrevMipLevel";
			this.buttonPrevMipLevel.Size = new System.Drawing.Size(23, 22);
			this.buttonPrevMipLevel.Text = "this was not set in localization";
			this.buttonPrevMipLevel.Click += new System.EventHandler(this.buttonPrevMipLevel_Click);
			// 
			// labelMipLevel
			// 
			this.labelMipLevel.AutoSize = false;
			this.labelMipLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.labelMipLevel.Image = ((System.Drawing.Image)(resources.GetObject("labelMipLevel.Image")));
			this.labelMipLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.labelMipLevel.Name = "labelMipLevel";
			this.labelMipLevel.Size = new System.Drawing.Size(160, 22);
			this.labelMipLevel.Text = "mip level: N/A";
			// 
			// buttonNextMipLevel
			// 
			this.buttonNextMipLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNextMipLevel.Enabled = false;
			this.buttonNextMipLevel.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.forward_16x161;
			this.buttonNextMipLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNextMipLevel.Name = "buttonNextMipLevel";
			this.buttonNextMipLevel.Size = new System.Drawing.Size(23, 22);
			this.buttonNextMipLevel.Text = "this was not set in localization";
			this.buttonNextMipLevel.Click += new System.EventHandler(this.buttonNextMipLevel_Click);
			// 
			// sepArray
			// 
			this.sepArray.Name = "sepArray";
			this.sepArray.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonPrevDepthSlice
			// 
			this.buttonPrevDepthSlice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPrevDepthSlice.Enabled = false;
			this.buttonPrevDepthSlice.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.back_16x16png1;
			this.buttonPrevDepthSlice.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPrevDepthSlice.Name = "buttonPrevDepthSlice";
			this.buttonPrevDepthSlice.Size = new System.Drawing.Size(23, 22);
			this.buttonPrevDepthSlice.Text = "this was not set in localization";
			this.buttonPrevDepthSlice.Visible = false;
			this.buttonPrevDepthSlice.Click += new System.EventHandler(this.buttonPrevDepthSlice_Click);
			// 
			// labelDepthSlice
			// 
			this.labelDepthSlice.AutoSize = false;
			this.labelDepthSlice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.labelDepthSlice.Image = ((System.Drawing.Image)(resources.GetObject("labelDepthSlice.Image")));
			this.labelDepthSlice.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.labelDepthSlice.Name = "labelDepthSlice";
			this.labelDepthSlice.Size = new System.Drawing.Size(110, 22);
			this.labelDepthSlice.Text = "array index: N/A";
			this.labelDepthSlice.Visible = false;
			// 
			// buttonNextDepthSlice
			// 
			this.buttonNextDepthSlice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNextDepthSlice.Enabled = false;
			this.buttonNextDepthSlice.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.forward_16x161;
			this.buttonNextDepthSlice.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNextDepthSlice.Name = "buttonNextDepthSlice";
			this.buttonNextDepthSlice.Size = new System.Drawing.Size(23, 22);
			this.buttonNextDepthSlice.Text = "this was not set in localization";
			this.buttonNextDepthSlice.Visible = false;
			this.buttonNextDepthSlice.Click += new System.EventHandler(this.buttonNextDepthSlice_Click);
			// 
			// buttonPrevArrayIndex
			// 
			this.buttonPrevArrayIndex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPrevArrayIndex.Enabled = false;
			this.buttonPrevArrayIndex.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.back_16x16png1;
			this.buttonPrevArrayIndex.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPrevArrayIndex.Name = "buttonPrevArrayIndex";
			this.buttonPrevArrayIndex.Size = new System.Drawing.Size(23, 22);
			this.buttonPrevArrayIndex.Text = "this was not set in localization";
			this.buttonPrevArrayIndex.Click += new System.EventHandler(this.buttonPrevArrayIndex_Click);
			// 
			// labelArrayIndex
			// 
			this.labelArrayIndex.AutoSize = false;
			this.labelArrayIndex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.labelArrayIndex.Image = ((System.Drawing.Image)(resources.GetObject("labelArrayIndex.Image")));
			this.labelArrayIndex.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.labelArrayIndex.Name = "labelArrayIndex";
			this.labelArrayIndex.Size = new System.Drawing.Size(110, 22);
			this.labelArrayIndex.Text = "array index: N/A";
			// 
			// buttonNextArrayIndex
			// 
			this.buttonNextArrayIndex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNextArrayIndex.Enabled = false;
			this.buttonNextArrayIndex.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.forward_16x161;
			this.buttonNextArrayIndex.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNextArrayIndex.Name = "buttonNextArrayIndex";
			this.buttonNextArrayIndex.Size = new System.Drawing.Size(23, 22);
			this.buttonNextArrayIndex.Text = "this was not set in localization";
			this.buttonNextArrayIndex.Click += new System.EventHandler(this.buttonNextArrayIndex_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonActualSize
			// 
			this.buttonActualSize.CheckOnClick = true;
			this.buttonActualSize.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.zoom_16x16;
			this.buttonActualSize.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonActualSize.Name = "buttonActualSize";
			this.buttonActualSize.Size = new System.Drawing.Size(94, 22);
			this.buttonActualSize.Text = "not localized";
			this.buttonActualSize.Click += new System.EventHandler(this.buttonActualSize_Click);
			// 
			// panelTextureArea
			// 
			this.panelTextureArea.Controls.Add(this.panelTextureDisplay);
			this.panelTextureArea.Controls.Add(this.panelHorzScroll);
			this.panelTextureArea.Controls.Add(this.panelVertScroll);
			this.panelTextureArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTextureArea.Location = new System.Drawing.Point(0, 25);
			this.panelTextureArea.Name = "panelTextureArea";
			this.panelTextureArea.Size = new System.Drawing.Size(806, 556);
			this.panelTextureArea.TabIndex = 1;
			// 
			// panelHorzScroll
			// 
			this.panelHorzScroll.AutoSize = true;
			this.panelHorzScroll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelHorzScroll.Controls.Add(this.scrollHorz);
			this.panelHorzScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelHorzScroll.Location = new System.Drawing.Point(0, 539);
			this.panelHorzScroll.Name = "panelHorzScroll";
			this.panelHorzScroll.Size = new System.Drawing.Size(789, 17);
			this.panelHorzScroll.TabIndex = 4;
			this.panelHorzScroll.Visible = false;
			// 
			// scrollHorz
			// 
			this.scrollHorz.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scrollHorz.LargeChange = 100;
			this.scrollHorz.Location = new System.Drawing.Point(0, 0);
			this.scrollHorz.Name = "scrollHorz";
			this.scrollHorz.Size = new System.Drawing.Size(789, 17);
			this.scrollHorz.SmallChange = 10;
			this.scrollHorz.TabIndex = 2;
			// 
			// panelVertScroll
			// 
			this.panelVertScroll.AutoSize = true;
			this.panelVertScroll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelVertScroll.BackColor = System.Drawing.SystemColors.Control;
			this.panelVertScroll.Controls.Add(this.scrollVert);
			this.panelVertScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelVertScroll.Location = new System.Drawing.Point(789, 0);
			this.panelVertScroll.Name = "panelVertScroll";
			this.panelVertScroll.Size = new System.Drawing.Size(17, 556);
			this.panelVertScroll.TabIndex = 3;
			this.panelVertScroll.Visible = false;
			// 
			// scrollVert
			// 
			this.scrollVert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.scrollVert.LargeChange = 100;
			this.scrollVert.Location = new System.Drawing.Point(0, 0);
			this.scrollVert.Name = "scrollVert";
			this.scrollVert.Size = new System.Drawing.Size(17, 539);
			this.scrollVert.SmallChange = 10;
			this.scrollVert.TabIndex = 1;
			// 
			// stripTexture
			// 
			this.stripTexture.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripTexture.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSave,
            this.buttonRevert,
            this.toolStripSeparator1,
            this.buttonEditFileExternal,
            this.toolStripSeparator2,
            this.buttonImport,
            this.buttonGenerateMips});
			this.stripTexture.Location = new System.Drawing.Point(0, 0);
			this.stripTexture.Name = "stripTexture";
			this.stripTexture.Size = new System.Drawing.Size(806, 25);
			this.stripTexture.Stretch = true;
			this.stripTexture.TabIndex = 0;
			// 
			// buttonSave
			// 
			this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSave.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.save_16x16;
			this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(23, 22);
			this.buttonSave.Text = "not localized";
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// buttonRevert
			// 
			this.buttonRevert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRevert.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.revert_16x16;
			this.buttonRevert.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRevert.Name = "buttonRevert";
			this.buttonRevert.Size = new System.Drawing.Size(23, 22);
			this.buttonRevert.Text = "not localized";
			this.buttonRevert.Click += new System.EventHandler(this.buttonRevert_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonEditFileExternal
			// 
			this.buttonEditFileExternal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditFileExternal.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.pencil_16x16;
			this.buttonEditFileExternal.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditFileExternal.Name = "buttonEditFileExternal";
			this.buttonEditFileExternal.Size = new System.Drawing.Size(23, 22);
			this.buttonEditFileExternal.Text = "no localization";
			this.buttonEditFileExternal.Click += new System.EventHandler(this.buttonEditFileExternal_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonImport
			// 
			this.buttonImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemImportFileDisk,
            this.itemImportFileSystem});
			this.buttonImport.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.import_image_filesystem_16x16;
			this.buttonImport.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonImport.Name = "buttonImport";
			this.buttonImport.Size = new System.Drawing.Size(103, 22);
			this.buttonImport.Text = "not localized";
			// 
			// itemImportFileDisk
			// 
			this.itemImportFileDisk.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.import_disk_16x16;
			this.itemImportFileDisk.Name = "itemImportFileDisk";
			this.itemImportFileDisk.Size = new System.Drawing.Size(141, 22);
			this.itemImportFileDisk.Text = "not localized";
			this.itemImportFileDisk.Click += new System.EventHandler(this.itemImportFileDisk_Click);
			// 
			// itemImportFileSystem
			// 
			this.itemImportFileSystem.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.import_16x16;
			this.itemImportFileSystem.Name = "itemImportFileSystem";
			this.itemImportFileSystem.Size = new System.Drawing.Size(141, 22);
			this.itemImportFileSystem.Text = "not localized";
			this.itemImportFileSystem.Click += new System.EventHandler(this.itemImportFileSystem_Click);
			// 
			// buttonGenerateMips
			// 
			this.buttonGenerateMips.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.gen_mips_16x16;
			this.buttonGenerateMips.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGenerateMips.Name = "buttonGenerateMips";
			this.buttonGenerateMips.Size = new System.Drawing.Size(94, 22);
			this.buttonGenerateMips.Text = "not localized";
			this.buttonGenerateMips.Click += new System.EventHandler(this.buttonGenerateMips_Click);
			// 
			// dialogOpenImage
			// 
			this.dialogOpenImage.Title = "Not localized";
			// 
			// dialogImportImage
			// 
			this.dialogImportImage.AllowAllFiles = false;
			this.dialogImportImage.DefaultFileType = "";
			this.dialogImportImage.Filename = null;
			this.dialogImportImage.StartDirectory = null;
			this.dialogImportImage.Text = "Select a file";
			// 
			// GorgonImageContentPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "GorgonImageContentPanel";
			this.Size = new System.Drawing.Size(806, 636);
			this.Text = "Image";
			this.Load += new System.EventHandler(this.GorgonImageContentPanel_Load);
			this.PanelDisplay.ResumeLayout(false);
			this.PanelDisplay.PerformLayout();
			this.stripImageEditor.ResumeLayout(false);
			this.stripImageEditor.PerformLayout();
			this.panelTextureArea.ResumeLayout(false);
			this.panelTextureArea.PerformLayout();
			this.panelHorzScroll.ResumeLayout(false);
			this.panelVertScroll.ResumeLayout(false);
			this.stripTexture.ResumeLayout(false);
			this.stripTexture.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

		private EditorFileDialog imageFileBrowser;
		internal GorgonSelectablePanel panelTextureDisplay;
        private ToolStrip stripImageEditor;
        private ToolStripLabel labelMipLevel;
        private ToolStripButton buttonPrevMipLevel;
        private ToolStripButton buttonNextMipLevel;
        private ToolStripSeparator sepArray;
        private ToolStripButton buttonPrevArrayIndex;
        private ToolStripLabel labelArrayIndex;
        private ToolStripButton buttonNextArrayIndex;
		private ToolStripLabel labelImageInfo;
		private ToolStripSeparator sepMip;
		private ToolStripButton buttonPrevDepthSlice;
		private ToolStripLabel labelDepthSlice;
		private ToolStripButton buttonNextDepthSlice;
		private ToolStrip stripTexture;
		private ToolStripButton buttonEditFileExternal;
		private ToolStripButton buttonRevert;
		private ToolStripButton buttonSave;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripDropDownButton buttonImport;
		private ToolStripMenuItem itemImportFileDisk;
		private ToolStripMenuItem itemImportFileSystem;
		private OpenFileDialog dialogOpenImage;
		private EditorFileDialog dialogImportImage;
		private ToolStripButton buttonGenerateMips;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton buttonActualSize;
        private Panel panelTextureArea;
        private HScrollBar scrollHorz;
        private VScrollBar scrollVert;
        private Panel panelHorzScroll;
        private Panel panelVertScroll;

    }
}
