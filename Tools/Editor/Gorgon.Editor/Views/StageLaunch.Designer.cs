namespace Gorgon.Editor.Views
{
    partial class StageLaunch
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
            this.components = new System.ComponentModel.Container();
            this.ButtonOpenProject = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.CheckNewProject = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.CheckRecent = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.ButtonGroup = new ComponentFactory.Krypton.Toolkit.KryptonCheckSet(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.StageNewProject = new Gorgon.Editor.Views.StageNew();
            this.StageRecent = new Gorgon.Editor.Views.StageRecent();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonGroup)).BeginInit();
            this.panel2.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonOpenProject
            // 
            this.ButtonOpenProject.AutoSize = true;
            this.ButtonOpenProject.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.ButtonOpenProject.Location = new System.Drawing.Point(0, 202);
            this.ButtonOpenProject.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonOpenProject.Name = "ButtonOpenProject";
            this.ButtonOpenProject.Size = new System.Drawing.Size(148, 100);
            this.ButtonOpenProject.TabIndex = 2;
            this.ButtonOpenProject.Values.Image = global::Gorgon.Editor.Properties.Resources.openproject_48x48;
            this.ButtonOpenProject.Values.Text = "Open";
            this.ButtonOpenProject.Click += new System.EventHandler(this.ButtonOpenProject_Click);
            // 
            // CheckNewProject
            // 
            this.CheckNewProject.AutoSize = true;
            this.CheckNewProject.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.CheckNewProject.Checked = true;
            this.CheckNewProject.Location = new System.Drawing.Point(0, 101);
            this.CheckNewProject.Margin = new System.Windows.Forms.Padding(0);
            this.CheckNewProject.Name = "CheckNewProject";
            this.CheckNewProject.Size = new System.Drawing.Size(148, 100);
            this.CheckNewProject.TabIndex = 1;
            this.CheckNewProject.Values.Image = global::Gorgon.Editor.Properties.Resources.newproject_48x48;
            this.CheckNewProject.Values.Text = "New";
            // 
            // CheckRecent
            // 
            this.CheckRecent.AutoSize = true;
            this.CheckRecent.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.CheckRecent.Location = new System.Drawing.Point(0, 0);
            this.CheckRecent.Margin = new System.Windows.Forms.Padding(0);
            this.CheckRecent.Name = "CheckRecent";
            this.CheckRecent.Size = new System.Drawing.Size(148, 100);
            this.CheckRecent.TabIndex = 0;
            this.CheckRecent.Values.Image = global::Gorgon.Editor.Properties.Resources.recent_48x48;
            this.CheckRecent.Values.Text = "Recent";
            // 
            // ButtonGroup
            // 
            this.ButtonGroup.CheckButtons.Add(this.CheckNewProject);
            this.ButtonGroup.CheckButtons.Add(this.CheckRecent);
            this.ButtonGroup.CheckedButton = this.CheckNewProject;
            this.ButtonGroup.CheckedButtonChanged += new System.EventHandler(this.ButtonGroup_CheckedButtonChanged);
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel2.Controls.Add(this.PanelButtons);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(0, 0, 1, 0);
            this.panel2.Size = new System.Drawing.Size(149, 604);
            this.panel2.TabIndex = 0;
            // 
            // PanelButtons
            // 
            this.PanelButtons.AutoSize = true;
            this.PanelButtons.Controls.Add(this.CheckRecent);
            this.PanelButtons.Controls.Add(this.CheckNewProject);
            this.PanelButtons.Controls.Add(this.ButtonOpenProject);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelButtons.Location = new System.Drawing.Point(0, 0);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(148, 604);
            this.PanelButtons.TabIndex = 1;
            // 
            // StageNewProject
            // 
            this.StageNewProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StageNewProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StageNewProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.StageNewProject.Location = new System.Drawing.Point(149, 0);
            this.StageNewProject.Name = "StageNewProject";
            this.StageNewProject.Size = new System.Drawing.Size(675, 604);
            this.StageNewProject.TabIndex = 1;
            // 
            // StageRecent
            // 
            this.StageRecent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StageRecent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StageRecent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.StageRecent.Location = new System.Drawing.Point(149, 0);
            this.StageRecent.Name = "StageRecent";
            this.StageRecent.Padding = new System.Windows.Forms.Padding(6);
            this.StageRecent.Size = new System.Drawing.Size(675, 604);
            this.StageRecent.TabIndex = 1;
            this.StageRecent.Visible = false;
            // 
            // StageLaunch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Controls.Add(this.StageNewProject);
            this.Controls.Add(this.StageRecent);
            this.Controls.Add(this.panel2);
            this.Name = "StageLaunch";
            this.Size = new System.Drawing.Size(824, 604);
            ((System.ComponentModel.ISupportInitialize)(this.ButtonGroup)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.PanelButtons.ResumeLayout(false);
            this.PanelButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonOpenProject;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton CheckNewProject;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton CheckRecent;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckSet ButtonGroup;
        private StageRecent StageRecent;
        public StageNew StageNewProject;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel PanelButtons;
    }
}
