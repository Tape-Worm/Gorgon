namespace Gorgon.Editor.UI.Controls
{
    partial class RecentFilesControl
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
                if (_recentItems != null)
                {
                    _recentItems.CollectionChanged -= RecentItems_CollectionChanged;
                }

                ClearItems();
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
            this.PanelRecentItems = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // PanelRecentItems
            // 
            this.PanelRecentItems.AutoScroll = true;
            this.PanelRecentItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRecentItems.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelRecentItems.Location = new System.Drawing.Point(0, 0);
            this.PanelRecentItems.Name = "PanelRecentItems";
            this.PanelRecentItems.Size = new System.Drawing.Size(546, 418);
            this.PanelRecentItems.TabIndex = 0;
            this.PanelRecentItems.WrapContents = false;
            this.PanelRecentItems.Layout += new System.Windows.Forms.LayoutEventHandler(this.PanelRecentItems_Layout);
            // 
            // RecentFilesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Controls.Add(this.PanelRecentItems);
            this.Name = "GorgonRecentFilesControl";
            this.Size = new System.Drawing.Size(546, 418);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel PanelRecentItems;
    }
}
