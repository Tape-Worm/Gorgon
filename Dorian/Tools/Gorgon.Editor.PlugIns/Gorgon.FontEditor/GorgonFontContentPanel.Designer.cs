namespace GorgonLibrary.Editor
{
    partial class GorgonFontContentPanel
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
            this.splitDisplay = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitDisplay)).BeginInit();
            this.splitDisplay.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitDisplay
            // 
            this.splitDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDisplay.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitDisplay.Location = new System.Drawing.Point(0, 0);
            this.splitDisplay.Name = "splitDisplay";
            this.splitDisplay.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitDisplay.Size = new System.Drawing.Size(691, 551);
            this.splitDisplay.SplitterDistance = 433;
            this.splitDisplay.TabIndex = 0;
            // 
            // GorgonFontContentPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitDisplay);
            this.Name = "GorgonFontContentPanel";
            this.Size = new System.Drawing.Size(691, 551);
            ((System.ComponentModel.ISupportInitialize)(this.splitDisplay)).EndInit();
            this.splitDisplay.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.SplitContainer splitDisplay;

    }
}
