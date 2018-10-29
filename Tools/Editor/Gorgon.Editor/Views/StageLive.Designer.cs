namespace Gorgon.Editor.Views
{
    partial class StageLive
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.ButtonSaveAs = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.ButtonSave = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.kryptonBorderEdge1 = new ComponentFactory.Krypton.Toolkit.KryptonBorderEdge();
            this.ButtonBack = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.CheckRecent = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.CheckNewProject = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.ButtonOpenProject = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.ButtonGroup = new ComponentFactory.Krypton.Toolkit.KryptonCheckSet(this.components);
            this.StageNewProject = new Gorgon.Editor.Views.StageNew();
            this.StageRecent = new Gorgon.Editor.Views.StageRecent();
            this.panel2.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonGroup)).BeginInit();
            this.SuspendLayout();
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
            this.panel2.Size = new System.Drawing.Size(149, 719);
            this.panel2.TabIndex = 4;
            // 
            // PanelButtons
            // 
            this.PanelButtons.AutoSize = true;
            this.PanelButtons.Controls.Add(this.ButtonSaveAs);
            this.PanelButtons.Controls.Add(this.ButtonSave);
            this.PanelButtons.Controls.Add(this.kryptonBorderEdge1);
            this.PanelButtons.Controls.Add(this.ButtonBack);
            this.PanelButtons.Controls.Add(this.CheckRecent);
            this.PanelButtons.Controls.Add(this.CheckNewProject);
            this.PanelButtons.Controls.Add(this.ButtonOpenProject);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelButtons.Location = new System.Drawing.Point(0, 0);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(148, 719);
            this.PanelButtons.TabIndex = 1;
            // 
            // ButtonSaveAs
            // 
            this.ButtonSaveAs.AutoSize = true;
            this.ButtonSaveAs.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.ButtonSaveAs.Enabled = false;
            this.ButtonSaveAs.Location = new System.Drawing.Point(24, 254);
            this.ButtonSaveAs.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonSaveAs.Name = "ButtonSaveAs";
            this.ButtonSaveAs.Size = new System.Drawing.Size(124, 50);
            this.ButtonSaveAs.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonSaveAs.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonSaveAs.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonSaveAs.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonSaveAs.TabIndex = 6;
            this.ButtonSaveAs.Values.Text = "Save As";
            this.ButtonSaveAs.Click += new System.EventHandler(this.ButtonSaveAs_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.AutoSize = true;
            this.ButtonSave.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.ButtonSave.Enabled = false;
            this.ButtonSave.Location = new System.Drawing.Point(24, 204);
            this.ButtonSave.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(124, 50);
            this.ButtonSave.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonSave.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonSave.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonSave.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonSave.TabIndex = 5;
            this.ButtonSave.Values.Text = "Save";
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // kryptonBorderEdge1
            // 
            this.kryptonBorderEdge1.AutoSize = false;
            this.kryptonBorderEdge1.BorderStyle = ComponentFactory.Krypton.Toolkit.PaletteBorderStyle.FormMain;
            this.kryptonBorderEdge1.Location = new System.Drawing.Point(14, 200);
            this.kryptonBorderEdge1.Name = "kryptonBorderEdge1";
            this.kryptonBorderEdge1.Size = new System.Drawing.Size(120, 1);
            this.kryptonBorderEdge1.Text = "kryptonBorderEdge1";
            // 
            // ButtonBack
            // 
            this.ButtonBack.AutoSize = true;
            this.ButtonBack.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.ButtonBack.Location = new System.Drawing.Point(0, 0);
            this.ButtonBack.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonBack.Name = "ButtonBack";
            this.ButtonBack.Size = new System.Drawing.Size(148, 50);
            this.ButtonBack.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonBack.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonBack.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonBack.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonBack.TabIndex = 3;
            this.ButtonBack.Values.Image = global::Gorgon.Editor.Properties.Resources.back_icon_24x24;
            this.ButtonBack.Values.Text = "";
            this.ButtonBack.Click += new System.EventHandler(this.ButtonBack_Click);
            // 
            // CheckRecent
            // 
            this.CheckRecent.AutoSize = true;
            this.CheckRecent.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.CheckRecent.Location = new System.Drawing.Point(0, 50);
            this.CheckRecent.Margin = new System.Windows.Forms.Padding(0);
            this.CheckRecent.Name = "CheckRecent";
            this.CheckRecent.Size = new System.Drawing.Size(148, 50);
            this.CheckRecent.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.CheckRecent.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.CheckRecent.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.CheckRecent.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.CheckRecent.TabIndex = 0;
            this.CheckRecent.Values.Image = global::Gorgon.Editor.Properties.Resources.recent_24x24;
            this.CheckRecent.Values.Text = "Recent";
            // 
            // CheckNewProject
            // 
            this.CheckNewProject.AutoSize = true;
            this.CheckNewProject.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.CheckNewProject.Checked = true;
            this.CheckNewProject.Location = new System.Drawing.Point(0, 100);
            this.CheckNewProject.Margin = new System.Windows.Forms.Padding(0);
            this.CheckNewProject.Name = "CheckNewProject";
            this.CheckNewProject.Size = new System.Drawing.Size(148, 50);
            this.CheckNewProject.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.CheckNewProject.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.CheckNewProject.StateCommon.Content.ShortText.ImageAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
            this.CheckNewProject.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.CheckNewProject.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.CheckNewProject.TabIndex = 1;
            this.CheckNewProject.Values.Image = global::Gorgon.Editor.Properties.Resources.newproject_24x24;
            this.CheckNewProject.Values.Text = "New";
            // 
            // ButtonOpenProject
            // 
            this.ButtonOpenProject.AutoSize = true;
            this.ButtonOpenProject.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom1;
            this.ButtonOpenProject.Location = new System.Drawing.Point(0, 150);
            this.ButtonOpenProject.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonOpenProject.Name = "ButtonOpenProject";
            this.ButtonOpenProject.Size = new System.Drawing.Size(148, 50);
            this.ButtonOpenProject.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonOpenProject.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonOpenProject.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.ButtonOpenProject.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonOpenProject.TabIndex = 2;
            this.ButtonOpenProject.Values.Image = global::Gorgon.Editor.Properties.Resources.openproject_24x24;
            this.ButtonOpenProject.Values.Text = "Open";
            this.ButtonOpenProject.Click += new System.EventHandler(this.ButtonOpenProject_Click);
            // 
            // ButtonGroup
            // 
            this.ButtonGroup.CheckButtons.Add(this.CheckNewProject);
            this.ButtonGroup.CheckButtons.Add(this.CheckRecent);
            this.ButtonGroup.CheckedButton = this.CheckNewProject;
            this.ButtonGroup.CheckedButtonChanged += new System.EventHandler(this.ButtonGroup_CheckedButtonChanged);
            // 
            // StageNewProject
            // 
            this.StageNewProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StageNewProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StageNewProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.StageNewProject.Location = new System.Drawing.Point(149, 0);
            this.StageNewProject.Name = "StageNewProject";
            this.StageNewProject.Size = new System.Drawing.Size(879, 719);
            this.StageNewProject.TabIndex = 2;
            // 
            // StageRecent
            // 
            this.StageRecent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StageRecent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StageRecent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.StageRecent.Location = new System.Drawing.Point(149, 0);
            this.StageRecent.Name = "StageRecent";
            this.StageRecent.Padding = new System.Windows.Forms.Padding(6);
            this.StageRecent.Size = new System.Drawing.Size(879, 719);
            this.StageRecent.TabIndex = 3;
            // 
            // StageLive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.StageNewProject);
            this.Controls.Add(this.StageRecent);
            this.Controls.Add(this.panel2);
            this.Name = "StageLive";
            this.Size = new System.Drawing.Size(1028, 719);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.PanelButtons.ResumeLayout(false);
            this.PanelButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonGroup)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public StageNew StageNewProject;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel PanelButtons;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton CheckRecent;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton CheckNewProject;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonOpenProject;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckSet ButtonGroup;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonBack;
        private ComponentFactory.Krypton.Toolkit.KryptonBorderEdge kryptonBorderEdge1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonSaveAs;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonSave;
        internal StageRecent StageRecent;
    }
}
