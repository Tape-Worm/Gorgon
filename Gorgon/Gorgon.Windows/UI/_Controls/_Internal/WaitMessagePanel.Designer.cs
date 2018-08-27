namespace Gorgon.UI
{
    partial class WaitMessagePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitMessagePanel));
            this.PanelWait = new System.Windows.Forms.TableLayoutPanel();
            this.LabelWaitMessage = new System.Windows.Forms.Label();
            this.ImageWait = new System.Windows.Forms.PictureBox();
            this.LabelWaitTitle = new System.Windows.Forms.Label();
            this.PanelWait.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageWait)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelWait
            // 
            resources.ApplyResources(this.PanelWait, "PanelWait");
            this.PanelWait.Controls.Add(this.LabelWaitMessage, 1, 1);
            this.PanelWait.Controls.Add(this.ImageWait, 0, 0);
            this.PanelWait.Controls.Add(this.LabelWaitTitle, 1, 0);
            this.PanelWait.Name = "PanelWait";
            // 
            // LabelWaitMessage
            // 
            resources.ApplyResources(this.LabelWaitMessage, "LabelWaitMessage");
            this.LabelWaitMessage.ForeColor = System.Drawing.Color.Black;
            this.LabelWaitMessage.Name = "LabelWaitMessage";
            // 
            // ImageWait
            // 
            resources.ApplyResources(this.ImageWait, "ImageWait");
            this.ImageWait.Image = global::Gorgon.Windows.Properties.Resources.wait_48x48;
            this.ImageWait.Name = "ImageWait";
            this.PanelWait.SetRowSpan(this.ImageWait, 2);
            this.ImageWait.TabStop = false;
            // 
            // LabelWaitTitle
            // 
            resources.ApplyResources(this.LabelWaitTitle, "LabelWaitTitle");
            this.LabelWaitTitle.ForeColor = System.Drawing.Color.DimGray;
            this.LabelWaitTitle.Name = "LabelWaitTitle";
            // 
            // WaitMessagePanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.PanelWait);
            this.DoubleBuffered = true;
            this.Name = "WaitMessagePanel";
            this.PanelWait.ResumeLayout(false);
            this.PanelWait.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageWait)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Label LabelWaitMessage;
        public System.Windows.Forms.PictureBox ImageWait;
        public System.Windows.Forms.Label LabelWaitTitle;
        public System.Windows.Forms.TableLayoutPanel PanelWait;
    }
}
