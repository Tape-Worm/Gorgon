namespace Gorgon.Editor.UI.Views
{
    partial class ContentBaseControl
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
                Shutdown();
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
            this.PanelRenderer = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            ((System.ComponentModel.ISupportInitialize)(this.PanelRenderer)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelRenderer
            // 
            this.PanelRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRenderer.Location = new System.Drawing.Point(0, 0);
            this.PanelRenderer.Name = "PanelRenderer";
            this.PanelRenderer.Size = new System.Drawing.Size(600, 468);
            this.PanelRenderer.TabIndex = 0;
            // 
            // ContentBaseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelRenderer);
            this.Name = "ContentBaseControl";
            ((System.ComponentModel.ISupportInitialize)(this.PanelRenderer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        [System.Runtime.CompilerServices.AccessedThroughProperty("RenderPanel")]
        private ComponentFactory.Krypton.Toolkit.KryptonPanel PanelRenderer;
    }
}
