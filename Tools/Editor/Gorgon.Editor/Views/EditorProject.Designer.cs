namespace Gorgon.Editor.Views
{
    partial class EditorProject
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
			this.DockSpace = new ComponentFactory.Krypton.Docking.KryptonDockableWorkspace();
			this.FileExplorer = new Gorgon.Editor.Views.FileExploder();
			this.DockManager = new ComponentFactory.Krypton.Docking.KryptonDockingManager();
			this.kryptonPage2 = new ComponentFactory.Krypton.Navigator.KryptonPage();
			this.kryptonPage3 = new ComponentFactory.Krypton.Navigator.KryptonPage();
			this.PanelContent = new System.Windows.Forms.Panel();
			this.Preview = new Gorgon.Editor.Views.ContentPreview();
			((System.ComponentModel.ISupportInitialize)(this.DockSpace)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonPage2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonPage3)).BeginInit();
			this.SuspendLayout();
			// 
			// DockSpace
			// 
			this.DockSpace.AutoHiddenHost = false;
			this.DockSpace.CompactFlags = ((ComponentFactory.Krypton.Workspace.CompactFlags)(((ComponentFactory.Krypton.Workspace.CompactFlags.RemoveEmptyCells | ComponentFactory.Krypton.Workspace.CompactFlags.RemoveEmptySequences) 
            | ComponentFactory.Krypton.Workspace.CompactFlags.PromoteLeafs)));
			this.DockSpace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DockSpace.Location = new System.Drawing.Point(0, 0);
			this.DockSpace.Name = "DockSpace";
			// 
			// 
			// 
			this.DockSpace.Root.UniqueName = "D313416FEECA4A58B8AF8BFBE6EF84D5";
			this.DockSpace.Root.WorkspaceControl = this.DockSpace;
			this.DockSpace.ShowMaximizeButton = false;
			this.DockSpace.Size = new System.Drawing.Size(976, 596);
			this.DockSpace.TabIndex = 0;
			this.DockSpace.TabStop = true;
			// 
			// FileExplorer
			// 
			this.FileExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.FileExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FileExplorer.Location = new System.Drawing.Point(0, 0);
			this.FileExplorer.Name = "FileExplorer";
			this.FileExplorer.Size = new System.Drawing.Size(245, 192);
			this.FileExplorer.TabIndex = 0;
			this.FileExplorer.Enter += new System.EventHandler(this.FileExplorer_Enter);
			this.FileExplorer.Leave += new System.EventHandler(this.FileExplorer_Leave);
			// 
			// DockManager
			// 
			this.DockManager.DefaultCloseRequest = ComponentFactory.Krypton.Docking.DockingCloseRequest.None;
			this.DockManager.DockspaceAdding += new System.EventHandler<ComponentFactory.Krypton.Docking.DockspaceEventArgs>(this.Dock_DockspaceAdding);
			// 
			// kryptonPage2
			// 
			this.kryptonPage2.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.kryptonPage2.Flags = 65534;
			this.kryptonPage2.LastVisibleSet = true;
			this.kryptonPage2.MinimumSize = new System.Drawing.Size(50, 50);
			this.kryptonPage2.Name = "kryptonPage2";
			this.kryptonPage2.Size = new System.Drawing.Size(100, 100);
			this.kryptonPage2.Text = "kryptonPage2";
			this.kryptonPage2.ToolTipTitle = "Page ToolTip";
			this.kryptonPage2.UniqueName = "07FFC2A616D74D8729B9B3673C313097";
			// 
			// kryptonPage3
			// 
			this.kryptonPage3.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.kryptonPage3.Flags = 65534;
			this.kryptonPage3.LastVisibleSet = true;
			this.kryptonPage3.MinimumSize = new System.Drawing.Size(50, 50);
			this.kryptonPage3.Name = "kryptonPage3";
			this.kryptonPage3.Size = new System.Drawing.Size(100, 100);
			this.kryptonPage3.Text = "kryptonPage3";
			this.kryptonPage3.ToolTipTitle = "Page ToolTip";
			this.kryptonPage3.UniqueName = "4F06F41F32E94EABF69FF7075971C345";
			// 
			// PanelContent
			// 
			this.PanelContent.AllowDrop = true;
			this.PanelContent.BackgroundImage = global::Gorgon.Editor.Properties.Resources.Gorgon_Logo_Full_Site;
			this.PanelContent.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.PanelContent.Location = new System.Drawing.Point(293, 243);
			this.PanelContent.Name = "PanelContent";
			this.PanelContent.Size = new System.Drawing.Size(417, 100);
			this.PanelContent.TabIndex = 0;
			this.PanelContent.DragDrop += new System.Windows.Forms.DragEventHandler(this.PanelContent_DragDrop);
			this.PanelContent.DragEnter += new System.Windows.Forms.DragEventHandler(this.PanelContent_DragEnter);
			this.PanelContent.Enter += new System.EventHandler(this.PanelContent_Enter);
			this.PanelContent.Leave += new System.EventHandler(this.PanelContent_Leave);
			// 
			// Preview
			// 
			this.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.Preview.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Preview.GraphicsContext = null;
			this.Preview.Location = new System.Drawing.Point(256, 0);
			this.Preview.Name = "Preview";
			this.Preview.Size = new System.Drawing.Size(256, 256);
			this.Preview.TabIndex = 1;
			// 
			// EditorProject
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Preview);
			this.Controls.Add(this.PanelContent);
			this.Controls.Add(this.FileExplorer);
			this.Controls.Add(this.DockSpace);
			this.Name = "EditorProject";
			this.Size = new System.Drawing.Size(976, 596);
			((System.ComponentModel.ISupportInitialize)(this.DockSpace)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonPage2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.kryptonPage3)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion
        public FileExploder FileExplorer;
        private ComponentFactory.Krypton.Docking.KryptonDockingManager DockManager;
        private ComponentFactory.Krypton.Docking.KryptonDockableWorkspace DockSpace;
        private ComponentFactory.Krypton.Navigator.KryptonPage kryptonPage2;
        private ComponentFactory.Krypton.Navigator.KryptonPage kryptonPage3;
        private System.Windows.Forms.Panel PanelContent;
        private ContentPreview Preview;
    }
}
