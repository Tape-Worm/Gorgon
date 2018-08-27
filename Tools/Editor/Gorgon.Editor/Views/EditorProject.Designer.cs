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
            if (disposing)
            {
                DataContext?.OnUnload();
                UnassignEvents();
            }

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
            this.SplitProject = new ComponentFactory.Krypton.Toolkit.KryptonSplitContainer();
            this.FileExplorer = new Gorgon.Editor.Views.FileExploder();
            ((System.ComponentModel.ISupportInitialize)(this.SplitProject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SplitProject.Panel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SplitProject.Panel2)).BeginInit();
            this.SplitProject.Panel2.SuspendLayout();
            this.SplitProject.SuspendLayout();
            this.SuspendLayout();
            // 
            // SplitProject
            // 
            this.SplitProject.Cursor = System.Windows.Forms.Cursors.Default;
            this.SplitProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitProject.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.SplitProject.Location = new System.Drawing.Point(0, 0);
            this.SplitProject.Name = "SplitProject";
            this.SplitProject.Panel1MinSize = 640;
            // 
            // SplitProject.Panel2
            // 
            this.SplitProject.Panel2.Controls.Add(this.FileExplorer);
            this.SplitProject.Panel2MinSize = 256;
            this.SplitProject.Size = new System.Drawing.Size(976, 596);
            this.SplitProject.SplitterDistance = 683;
            this.SplitProject.TabIndex = 0;
            // 
            // FileExplorer
            // 
            this.FileExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FileExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FileExplorer.Location = new System.Drawing.Point(0, 0);
            this.FileExplorer.Name = "FileExplorer";
            this.FileExplorer.Size = new System.Drawing.Size(288, 596);
            this.FileExplorer.TabIndex = 0;
            // 
            // EditorProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SplitProject);
            this.Name = "EditorProject";
            this.Size = new System.Drawing.Size(976, 596);
            ((System.ComponentModel.ISupportInitialize)(this.SplitProject.Panel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SplitProject.Panel2)).EndInit();
            this.SplitProject.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitProject)).EndInit();
            this.SplitProject.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonSplitContainer SplitProject;
        public FileExploder FileExplorer;
    }
}
