namespace Gorgon.Editor.Views
{
    partial class ContentPreviewPanel
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
                _previewTexture?.Dispose();
                _previewTexture = null;
                CleanupResources();
                DataContext?.OnUnload();
                UnassignEvents();
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
            this.PanelDisplay = new Gorgon.UI.GorgonSelectablePanel();
            this.SuspendLayout();
            // 
            // PanelDisplay
            // 
            this.PanelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelDisplay.Location = new System.Drawing.Point(0, 0);
            this.PanelDisplay.Name = "PanelDisplay";
            this.PanelDisplay.ShowFocus = false;
            this.PanelDisplay.Size = new System.Drawing.Size(600, 468);
            this.PanelDisplay.TabIndex = 0;
            this.PanelDisplay.TabStop = false;
            // 
            // ContentPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelDisplay);
            this.Name = "ContentPreview";
            this.ResumeLayout(false);

        }

        #endregion

        private Gorgon.UI.GorgonSelectablePanel PanelDisplay;
    }
}
