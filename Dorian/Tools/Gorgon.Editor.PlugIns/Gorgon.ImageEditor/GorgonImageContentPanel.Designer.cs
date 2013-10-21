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
            this.SuspendLayout();
            // 
            // PanelDisplay
            // 
            this.PanelDisplay.Size = new System.Drawing.Size(806, 606);
            // 
            // imageFileBrowser
            // 
            this.imageFileBrowser.DefaultExtension = "png";
            this.imageFileBrowser.Text = "Open Image";
            // 
            // GorgonImageContentPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "GorgonImageContentPanel";
            this.Size = new System.Drawing.Size(806, 636);
            this.Text = "Image";
            this.ResumeLayout(false);

        }

        #endregion

        private EditorFileBrowser imageFileBrowser;

    }
}
