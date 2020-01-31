namespace Gorgon.Editor.Views
{
    partial class ProjectContainer
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
                _fileExplorerContextChanged = null;
                _fileExplorerIsRenaming = null;
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
            this.PanelContent = new System.Windows.Forms.Panel();
            this.SplitMain = new System.Windows.Forms.SplitContainer();
            this.SplitFileSystem = new System.Windows.Forms.SplitContainer();
            this.FileExplorer = new Gorgon.Editor.Views.FileExploder();
            this.Preview = new Gorgon.Editor.Views.ContentPreview();
            ((System.ComponentModel.ISupportInitialize)(this.SplitMain)).BeginInit();
            this.SplitMain.Panel1.SuspendLayout();
            this.SplitMain.Panel2.SuspendLayout();
            this.SplitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitFileSystem)).BeginInit();
            this.SplitFileSystem.Panel1.SuspendLayout();
            this.SplitFileSystem.Panel2.SuspendLayout();
            this.SplitFileSystem.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelContent
            // 
            this.PanelContent.AllowDrop = true;
            this.PanelContent.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PanelContent.BackgroundImage = global::Gorgon.Editor.Properties.Resources.Gorgon_Logo_Full_Site;
            this.PanelContent.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.PanelContent.Location = new System.Drawing.Point(54, 279);
            this.PanelContent.Name = "PanelContent";
            this.PanelContent.Size = new System.Drawing.Size(417, 100);
            this.PanelContent.TabIndex = 0;
            this.PanelContent.DragDrop += new System.Windows.Forms.DragEventHandler(this.PanelContent_DragDrop);
            this.PanelContent.DragEnter += new System.Windows.Forms.DragEventHandler(this.PanelContent_DragEnter);
            this.PanelContent.Enter += new System.EventHandler(this.PanelContent_Enter);
            this.PanelContent.Leave += new System.EventHandler(this.PanelContent_Leave);
            // 
            // SplitMain
            // 
            this.SplitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.SplitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.SplitMain.Location = new System.Drawing.Point(0, 0);
            this.SplitMain.Name = "SplitMain";
            // 
            // SplitMain.Panel1
            // 
            this.SplitMain.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.SplitMain.Panel1.Controls.Add(this.PanelContent);
            this.SplitMain.Panel1MinSize = 320;
            // 
            // SplitMain.Panel2
            // 
            this.SplitMain.Panel2.Controls.Add(this.SplitFileSystem);
            this.SplitMain.Size = new System.Drawing.Size(1033, 658);
            this.SplitMain.SplitterDistance = 524;
            this.SplitMain.TabIndex = 2;
            this.SplitMain.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitMain_SplitterMoved);
            // 
            // SplitFileSystem
            // 
            this.SplitFileSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.SplitFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitFileSystem.Location = new System.Drawing.Point(0, 0);
            this.SplitFileSystem.Name = "SplitFileSystem";
            this.SplitFileSystem.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitFileSystem.Panel1
            // 
            this.SplitFileSystem.Panel1.Controls.Add(this.FileExplorer);
            // 
            // SplitFileSystem.Panel2
            // 
            this.SplitFileSystem.Panel2.Controls.Add(this.Preview);
            this.SplitFileSystem.Size = new System.Drawing.Size(505, 658);
            this.SplitFileSystem.SplitterDistance = 618;
            this.SplitFileSystem.TabIndex = 1;
            this.SplitFileSystem.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitFileSystem_SplitterMoved);
            // 
            // FileExplorer
            // 
            this.FileExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.FileExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FileExplorer.ForeColor = System.Drawing.Color.White;
            this.FileExplorer.Location = new System.Drawing.Point(0, 0);
            this.FileExplorer.Name = "FileExplorer";
            this.FileExplorer.Size = new System.Drawing.Size(505, 618);
            this.FileExplorer.TabIndex = 0;
            this.FileExplorer.ControlContextChanged += new System.EventHandler(this.FileExplorer_ControlContextChanged);
            this.FileExplorer.IsRenamingChanged += new System.EventHandler(this.FileExplorer_IsRenamingChanged);
            this.FileExplorer.Enter += new System.EventHandler(this.FileExplorer_Enter);
            this.FileExplorer.Leave += new System.EventHandler(this.FileExplorer_Leave);
            // 
            // Preview
            // 
            this.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.Preview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Preview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Preview.ForeColor = System.Drawing.Color.White;
            this.Preview.GraphicsContext = null;
            this.Preview.Location = new System.Drawing.Point(0, 0);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(505, 36);
            this.Preview.TabIndex = 1;
            // 
            // ProjectContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SplitMain);
            this.Name = "ProjectContainer";
            this.Size = new System.Drawing.Size(1033, 658);
            this.SplitMain.Panel1.ResumeLayout(false);
            this.SplitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitMain)).EndInit();
            this.SplitMain.ResumeLayout(false);
            this.SplitFileSystem.Panel1.ResumeLayout(false);
            this.SplitFileSystem.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitFileSystem)).EndInit();
            this.SplitFileSystem.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public FileExploder FileExplorer;
        private System.Windows.Forms.Panel PanelContent;
        private ContentPreview Preview;
        private System.Windows.Forms.SplitContainer SplitMain;
        private System.Windows.Forms.SplitContainer SplitFileSystem;
    }
}
