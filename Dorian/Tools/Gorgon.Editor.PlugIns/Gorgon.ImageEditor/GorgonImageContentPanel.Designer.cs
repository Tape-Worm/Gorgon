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
			this.imageFileBrowser = new GorgonLibrary.Editor.EditorFileBrowser();
			this.panelTextureDisplay = new System.Windows.Forms.Panel();
			this.PanelDisplay.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelDisplay
			// 
			this.PanelDisplay.Controls.Add(this.panelTextureDisplay);
			this.PanelDisplay.Size = new System.Drawing.Size(806, 606);
			// 
			// imageFileBrowser
			// 
			this.imageFileBrowser.DefaultExtension = "png";
			this.imageFileBrowser.Filename = null;
			this.imageFileBrowser.StartDirectory = null;
			this.imageFileBrowser.Text = "Open Image";
			// 
			// panelTextureDisplay
			// 
			this.panelTextureDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTextureDisplay.Location = new System.Drawing.Point(0, 0);
			this.panelTextureDisplay.Name = "panelTextureDisplay";
			this.panelTextureDisplay.Size = new System.Drawing.Size(806, 606);
			this.panelTextureDisplay.TabIndex = 0;
			// 
			// GorgonImageContentPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "GorgonImageContentPanel";
			this.Size = new System.Drawing.Size(806, 636);
			this.Text = "Image";
			this.PanelDisplay.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		private EditorFileBrowser imageFileBrowser;
		internal System.Windows.Forms.Panel panelTextureDisplay;

    }
}
