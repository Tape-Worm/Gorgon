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
            this.CheckOpenProject = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.CheckNewProject = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.CheckRecent = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.ButtonGroup = new ComponentFactory.Krypton.Toolkit.KryptonCheckSet(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.PanelBack = new System.Windows.Forms.Panel();
            this.ButtonBack = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.StageNewProject = new Gorgon.Editor.Views.StageNew();
            this.StageRecent = new Gorgon.Editor.Views.StageRecent();
            this.kryptonBorderEdge1 = new ComponentFactory.Krypton.Toolkit.KryptonBorderEdge();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonGroup)).BeginInit();
            this.panel2.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            this.PanelBack.SuspendLayout();
            this.SuspendLayout();
            // 
            // CheckOpenProject
            // 
            this.CheckOpenProject.AutoSize = true;
            this.CheckOpenProject.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.CheckOpenProject.Location = new System.Drawing.Point(0, 202);
            this.CheckOpenProject.Margin = new System.Windows.Forms.Padding(0);
            this.CheckOpenProject.Name = "CheckOpenProject";
            this.CheckOpenProject.Size = new System.Drawing.Size(148, 100);
            this.CheckOpenProject.TabIndex = 2;
            this.CheckOpenProject.Values.Image = global::Gorgon.Editor.Properties.Resources.openproject_48x48;
            this.CheckOpenProject.Values.Text = "Open";
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
            this.ButtonGroup.CheckButtons.Add(this.CheckOpenProject);
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
            this.panel2.Controls.Add(this.PanelBack);
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
            this.PanelButtons.Controls.Add(this.CheckOpenProject);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelButtons.Location = new System.Drawing.Point(0, 47);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(148, 557);
            this.PanelButtons.TabIndex = 1;
            // 
            // PanelBack
            // 
            this.PanelBack.AutoSize = true;
            this.PanelBack.Controls.Add(this.kryptonBorderEdge1);
            this.PanelBack.Controls.Add(this.ButtonBack);
            this.PanelBack.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelBack.Location = new System.Drawing.Point(0, 0);
            this.PanelBack.Margin = new System.Windows.Forms.Padding(0);
            this.PanelBack.Name = "PanelBack";
            this.PanelBack.Padding = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.PanelBack.Size = new System.Drawing.Size(148, 47);
            this.PanelBack.TabIndex = 0;
            this.PanelBack.Visible = false;
            // 
            // ButtonBack
            // 
            this.ButtonBack.AutoSize = true;
            this.ButtonBack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonBack.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom3;
            this.ButtonBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonBack.Location = new System.Drawing.Point(3, 3);
            this.ButtonBack.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonBack.Name = "ButtonBack";
            this.ButtonBack.Size = new System.Drawing.Size(142, 38);
            this.ButtonBack.StateCommon.Content.Padding = new System.Windows.Forms.Padding(6, 6, -1, 6);
            this.ButtonBack.TabIndex = 0;
            this.ButtonBack.Values.Image = global::Gorgon.Editor.Properties.Resources.back_icon_24x24;
            this.ButtonBack.Values.Text = "Go back";
            this.ButtonBack.Click += new System.EventHandler(this.ButtonBack_Click);
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
            // kryptonBorderEdge1
            // 
            this.kryptonBorderEdge1.BorderStyle = ComponentFactory.Krypton.Toolkit.PaletteBorderStyle.ContextMenuSeparator;
            this.kryptonBorderEdge1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.kryptonBorderEdge1.Location = new System.Drawing.Point(3, 40);
            this.kryptonBorderEdge1.Name = "kryptonBorderEdge1";
            this.kryptonBorderEdge1.Size = new System.Drawing.Size(142, 1);
            this.kryptonBorderEdge1.StateCommon.Width = 1;
            this.kryptonBorderEdge1.Text = "kryptonBorderEdge1";
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
            this.PanelBack.ResumeLayout(false);
            this.PanelBack.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton CheckOpenProject;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton CheckNewProject;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton CheckRecent;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckSet ButtonGroup;
        private StageRecent StageRecent;
        public StageNew StageNewProject;
        private System.Windows.Forms.Panel panel2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonBack;
        private System.Windows.Forms.Panel PanelButtons;
        private System.Windows.Forms.Panel PanelBack;
        private ComponentFactory.Krypton.Toolkit.KryptonBorderEdge kryptonBorderEdge1;
    }
}
