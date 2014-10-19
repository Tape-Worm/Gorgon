namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    partial class GorgonImageContentPanel
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
		        if (_backgroundTexture != null)
		        {
			        _backgroundTexture.Dispose();
		        }

		        if (_texture != null)
		        {
			        _texture.Dispose();
		        }

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
			this.imageFileBrowser = new GorgonLibrary.Editor.EditorFileDialog();
			this.panelTextureDisplay = new System.Windows.Forms.Panel();
			this.stripPanelImageEditor = new System.Windows.Forms.ToolStripContainer();
			this.stripImageEditor = new System.Windows.Forms.ToolStrip();
			this.labelImageInfo = new System.Windows.Forms.ToolStripLabel();
			this.sepMip = new System.Windows.Forms.ToolStripSeparator();
			this.buttonPrevMipLevel = new System.Windows.Forms.ToolStripButton();
			this.labelMipLevel = new System.Windows.Forms.ToolStripLabel();
			this.buttonNextMipLevel = new System.Windows.Forms.ToolStripButton();
			this.sepArray = new System.Windows.Forms.ToolStripSeparator();
			this.buttonPrevArrayIndex = new System.Windows.Forms.ToolStripButton();
			this.labelArrayIndex = new System.Windows.Forms.ToolStripLabel();
			this.buttonNextArrayIndex = new System.Windows.Forms.ToolStripButton();
			this.buttonPrevDepthSlice = new System.Windows.Forms.ToolStripButton();
			this.labelDepthSlice = new System.Windows.Forms.ToolStripLabel();
			this.buttonNextDepthSlice = new System.Windows.Forms.ToolStripButton();
			this.PanelDisplay.SuspendLayout();
			this.stripPanelImageEditor.BottomToolStripPanel.SuspendLayout();
			this.stripPanelImageEditor.ContentPanel.SuspendLayout();
			this.stripPanelImageEditor.SuspendLayout();
			this.stripImageEditor.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelDisplay
			// 
			this.PanelDisplay.Controls.Add(this.stripPanelImageEditor);
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
			this.panelTextureDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTextureDisplay.Location = new System.Drawing.Point(0, 0);
			this.panelTextureDisplay.Name = "panelTextureDisplay";
			this.panelTextureDisplay.Size = new System.Drawing.Size(806, 581);
			this.panelTextureDisplay.TabIndex = 0;
			// 
			// stripPanelImageEditor
			// 
			// 
			// stripPanelImageEditor.BottomToolStripPanel
			// 
			this.stripPanelImageEditor.BottomToolStripPanel.Controls.Add(this.stripImageEditor);
			// 
			// stripPanelImageEditor.ContentPanel
			// 
			this.stripPanelImageEditor.ContentPanel.Controls.Add(this.panelTextureDisplay);
			this.stripPanelImageEditor.ContentPanel.Size = new System.Drawing.Size(806, 581);
			this.stripPanelImageEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stripPanelImageEditor.LeftToolStripPanelVisible = false;
			this.stripPanelImageEditor.Location = new System.Drawing.Point(0, 0);
			this.stripPanelImageEditor.Name = "stripPanelImageEditor";
			this.stripPanelImageEditor.RightToolStripPanelVisible = false;
			this.stripPanelImageEditor.Size = new System.Drawing.Size(806, 606);
			this.stripPanelImageEditor.TabIndex = 1;
			this.stripPanelImageEditor.Text = "toolStripContainer1";
			this.stripPanelImageEditor.TopToolStripPanelVisible = false;
			// 
			// stripImageEditor
			// 
			this.stripImageEditor.Dock = System.Windows.Forms.DockStyle.None;
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
            this.buttonNextArrayIndex});
			this.stripImageEditor.Location = new System.Drawing.Point(0, 0);
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
			this.buttonPrevMipLevel.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.back_16x16png1;
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
			this.buttonNextMipLevel.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.forward_16x161;
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
			// buttonPrevArrayIndex
			// 
			this.buttonPrevArrayIndex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPrevArrayIndex.Enabled = false;
			this.buttonPrevArrayIndex.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.back_16x16png1;
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
			this.buttonNextArrayIndex.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.forward_16x161;
			this.buttonNextArrayIndex.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNextArrayIndex.Name = "buttonNextArrayIndex";
			this.buttonNextArrayIndex.Size = new System.Drawing.Size(23, 22);
			this.buttonNextArrayIndex.Text = "this was not set in localization";
			this.buttonNextArrayIndex.Click += new System.EventHandler(this.buttonNextArrayIndex_Click);
			// 
			// buttonPrevDepthSlice
			// 
			this.buttonPrevDepthSlice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPrevDepthSlice.Enabled = false;
			this.buttonPrevDepthSlice.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.back_16x16png1;
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
			this.buttonNextDepthSlice.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.forward_16x161;
			this.buttonNextDepthSlice.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNextDepthSlice.Name = "buttonNextDepthSlice";
			this.buttonNextDepthSlice.Size = new System.Drawing.Size(23, 22);
			this.buttonNextDepthSlice.Text = "this was not set in localization";
			this.buttonNextDepthSlice.Visible = false;
			this.buttonNextDepthSlice.Click += new System.EventHandler(this.buttonNextDepthSlice_Click);
			// 
			// GorgonImageContentPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "GorgonImageContentPanel";
			this.Size = new System.Drawing.Size(806, 636);
			this.Text = "Image";
			this.PanelDisplay.ResumeLayout(false);
			this.stripPanelImageEditor.BottomToolStripPanel.ResumeLayout(false);
			this.stripPanelImageEditor.BottomToolStripPanel.PerformLayout();
			this.stripPanelImageEditor.ContentPanel.ResumeLayout(false);
			this.stripPanelImageEditor.ResumeLayout(false);
			this.stripPanelImageEditor.PerformLayout();
			this.stripImageEditor.ResumeLayout(false);
			this.stripImageEditor.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

		private EditorFileDialog imageFileBrowser;
		internal System.Windows.Forms.Panel panelTextureDisplay;
        private System.Windows.Forms.ToolStripContainer stripPanelImageEditor;
        private System.Windows.Forms.ToolStrip stripImageEditor;
        private System.Windows.Forms.ToolStripLabel labelMipLevel;
        private System.Windows.Forms.ToolStripButton buttonPrevMipLevel;
        private System.Windows.Forms.ToolStripButton buttonNextMipLevel;
        private System.Windows.Forms.ToolStripSeparator sepArray;
        private System.Windows.Forms.ToolStripButton buttonPrevArrayIndex;
        private System.Windows.Forms.ToolStripLabel labelArrayIndex;
        private System.Windows.Forms.ToolStripButton buttonNextArrayIndex;
		private System.Windows.Forms.ToolStripLabel labelImageInfo;
		private System.Windows.Forms.ToolStripSeparator sepMip;
		private System.Windows.Forms.ToolStripButton buttonPrevDepthSlice;
		private System.Windows.Forms.ToolStripLabel labelDepthSlice;
		private System.Windows.Forms.ToolStripButton buttonNextDepthSlice;

    }
}
