namespace Gorgon.Editor.Views
{
    partial class Stage
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
            this.PanelFunctions = new System.Windows.Forms.Panel();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.CheckSettings = new System.Windows.Forms.RadioButton();
            this.ButtonOpenPackFile = new System.Windows.Forms.Button();
            this.ButtonSaveAs = new System.Windows.Forms.Button();
            this.PanelSepSave = new System.Windows.Forms.Panel();
            this.ButtonBrowse = new System.Windows.Forms.Button();
            this.CheckNew = new System.Windows.Forms.RadioButton();
            this.CheckRecent = new System.Windows.Forms.RadioButton();
            this.PanelLogo = new System.Windows.Forms.Panel();
            this.PictureLogo = new System.Windows.Forms.PictureBox();
            this.ButtonBack = new System.Windows.Forms.Button();
            this.PanelBack = new System.Windows.Forms.Panel();
            this.TipButtons = new System.Windows.Forms.ToolTip(this.components);
            this.SettingsPanel = new Gorgon.Editor.Views.EditorSettingsPanel();
            this.NewProject = new Gorgon.Editor.Views.StageNew();
            this.Recent = new Gorgon.Editor.Views.StageRecent();
            this.PanelFunctions.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            this.PanelLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureLogo)).BeginInit();
            this.PanelBack.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelFunctions
            // 
            this.PanelFunctions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.PanelFunctions.Controls.Add(this.PanelButtons);
            this.PanelFunctions.Controls.Add(this.PanelLogo);
            this.PanelFunctions.Dock = System.Windows.Forms.DockStyle.Right;
            this.PanelFunctions.Location = new System.Drawing.Point(656, 0);
            this.PanelFunctions.Name = "PanelFunctions";
            this.PanelFunctions.Size = new System.Drawing.Size(310, 613);
            this.PanelFunctions.TabIndex = 0;
            // 
            // PanelButtons
            // 
            this.PanelButtons.AutoScroll = true;
            this.PanelButtons.Controls.Add(this.CheckSettings);
            this.PanelButtons.Controls.Add(this.ButtonOpenPackFile);
            this.PanelButtons.Controls.Add(this.ButtonSaveAs);
            this.PanelButtons.Controls.Add(this.PanelSepSave);
            this.PanelButtons.Controls.Add(this.ButtonBrowse);
            this.PanelButtons.Controls.Add(this.CheckNew);
            this.PanelButtons.Controls.Add(this.CheckRecent);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelButtons.Location = new System.Drawing.Point(0, 0);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(310, 557);
            this.PanelButtons.TabIndex = 2;
            // 
            // CheckSettings
            // 
            this.CheckSettings.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckSettings.AutoSize = true;
            this.CheckSettings.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.CheckSettings.FlatAppearance.BorderSize = 0;
            this.CheckSettings.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
            this.CheckSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.CheckSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.CheckSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckSettings.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.CheckSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.CheckSettings.Image = global::Gorgon.Editor.Properties.Resources.settings_48x48;
            this.CheckSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CheckSettings.Location = new System.Drawing.Point(0, 500);
            this.CheckSettings.Name = "CheckSettings";
            this.CheckSettings.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.CheckSettings.Size = new System.Drawing.Size(310, 57);
            this.CheckSettings.TabIndex = 7;
            this.CheckSettings.Text = "&Settings";
            this.CheckSettings.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TipButtons.SetToolTip(this.CheckSettings, "Change the settings for the application and/or various plug ins.");
            this.CheckSettings.UseVisualStyleBackColor = true;
            this.CheckSettings.Click += new System.EventHandler(this.CheckSettings_Click);
            // 
            // ButtonOpenPackFile
            // 
            this.ButtonOpenPackFile.AutoSize = true;
            this.ButtonOpenPackFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOpenPackFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonOpenPackFile.FlatAppearance.BorderSize = 0;
            this.ButtonOpenPackFile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonOpenPackFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonOpenPackFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonOpenPackFile.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.ButtonOpenPackFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ButtonOpenPackFile.Image = global::Gorgon.Editor.Properties.Resources.open_pack_file_48x48;
            this.ButtonOpenPackFile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonOpenPackFile.Location = new System.Drawing.Point(0, 230);
            this.ButtonOpenPackFile.Name = "ButtonOpenPackFile";
            this.ButtonOpenPackFile.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.ButtonOpenPackFile.Size = new System.Drawing.Size(310, 57);
            this.ButtonOpenPackFile.TabIndex = 6;
            this.ButtonOpenPackFile.Text = "&Open package file...";
            this.ButtonOpenPackFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonOpenPackFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TipButtons.SetToolTip(this.ButtonOpenPackFile, "Opens a packaged file and creates a project from it.\r\n\r\nNote: This functionality " +
        "requires that the appropriate plugin support is available.");
            this.ButtonOpenPackFile.UseVisualStyleBackColor = true;
            this.ButtonOpenPackFile.Click += new System.EventHandler(this.ButtonOpenPackFile_Click);
            // 
            // ButtonSaveAs
            // 
            this.ButtonSaveAs.AutoSize = true;
            this.ButtonSaveAs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonSaveAs.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonSaveAs.FlatAppearance.BorderSize = 0;
            this.ButtonSaveAs.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonSaveAs.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonSaveAs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonSaveAs.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.ButtonSaveAs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ButtonSaveAs.Image = global::Gorgon.Editor.Properties.Resources.stage_save_as_48x48;
            this.ButtonSaveAs.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonSaveAs.Location = new System.Drawing.Point(0, 173);
            this.ButtonSaveAs.Name = "ButtonSaveAs";
            this.ButtonSaveAs.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.ButtonSaveAs.Size = new System.Drawing.Size(310, 57);
            this.ButtonSaveAs.TabIndex = 2;
            this.ButtonSaveAs.Text = "Save &as packaged file...";
            this.ButtonSaveAs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonSaveAs.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TipButtons.SetToolTip(this.ButtonSaveAs, "Save the current project as a packaged file.\r\n\r\nNote: This functionality requires" +
        " that the appropriate plugin support is available.\r\n");
            this.ButtonSaveAs.UseVisualStyleBackColor = true;
            this.ButtonSaveAs.Visible = false;
            this.ButtonSaveAs.Click += new System.EventHandler(this.ButtonSaveAs_Click);
            // 
            // PanelSepSave
            // 
            this.PanelSepSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PanelSepSave.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelSepSave.Location = new System.Drawing.Point(0, 171);
            this.PanelSepSave.Name = "PanelSepSave";
            this.PanelSepSave.Size = new System.Drawing.Size(310, 2);
            this.PanelSepSave.TabIndex = 5;
            // 
            // ButtonBrowse
            // 
            this.ButtonBrowse.AutoSize = true;
            this.ButtonBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonBrowse.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonBrowse.FlatAppearance.BorderSize = 0;
            this.ButtonBrowse.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonBrowse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonBrowse.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.ButtonBrowse.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ButtonBrowse.Image = global::Gorgon.Editor.Properties.Resources.openproject_48x48;
            this.ButtonBrowse.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBrowse.Location = new System.Drawing.Point(0, 114);
            this.ButtonBrowse.Name = "ButtonBrowse";
            this.ButtonBrowse.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.ButtonBrowse.Size = new System.Drawing.Size(310, 57);
            this.ButtonBrowse.TabIndex = 1;
            this.ButtonBrowse.Text = "&Browse for project...";
            this.ButtonBrowse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBrowse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TipButtons.SetToolTip(this.ButtonBrowse, "Locate a folder containing a Gorgon Editor project and open it.");
            this.ButtonBrowse.UseVisualStyleBackColor = true;
            this.ButtonBrowse.Click += new System.EventHandler(this.ButtonOpenProject_Click);
            // 
            // CheckNew
            // 
            this.CheckNew.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckNew.AutoSize = true;
            this.CheckNew.Checked = true;
            this.CheckNew.Dock = System.Windows.Forms.DockStyle.Top;
            this.CheckNew.FlatAppearance.BorderSize = 0;
            this.CheckNew.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
            this.CheckNew.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.CheckNew.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.CheckNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckNew.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.CheckNew.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.CheckNew.Image = global::Gorgon.Editor.Properties.Resources.newproject_48x48;
            this.CheckNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CheckNew.Location = new System.Drawing.Point(0, 57);
            this.CheckNew.Name = "CheckNew";
            this.CheckNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.CheckNew.Size = new System.Drawing.Size(310, 57);
            this.CheckNew.TabIndex = 0;
            this.CheckNew.TabStop = true;
            this.CheckNew.Text = "&New project";
            this.CheckNew.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TipButtons.SetToolTip(this.CheckNew, "Create a new project.");
            this.CheckNew.UseVisualStyleBackColor = true;
            this.CheckNew.Click += new System.EventHandler(this.CheckNew_Click);
            // 
            // CheckRecent
            // 
            this.CheckRecent.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckRecent.AutoSize = true;
            this.CheckRecent.Dock = System.Windows.Forms.DockStyle.Top;
            this.CheckRecent.FlatAppearance.BorderSize = 0;
            this.CheckRecent.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
            this.CheckRecent.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.CheckRecent.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.CheckRecent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckRecent.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.CheckRecent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.CheckRecent.Image = global::Gorgon.Editor.Properties.Resources.recent_48x48;
            this.CheckRecent.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CheckRecent.Location = new System.Drawing.Point(0, 0);
            this.CheckRecent.Name = "CheckRecent";
            this.CheckRecent.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.CheckRecent.Size = new System.Drawing.Size(310, 57);
            this.CheckRecent.TabIndex = 4;
            this.CheckRecent.Text = "&Recent projects";
            this.CheckRecent.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.TipButtons.SetToolTip(this.CheckRecent, "Show and open projects with recent activity.");
            this.CheckRecent.UseVisualStyleBackColor = true;
            this.CheckRecent.Click += new System.EventHandler(this.CheckRecent_Click);
            // 
            // PanelLogo
            // 
            this.PanelLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.PanelLogo.Controls.Add(this.PictureLogo);
            this.PanelLogo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelLogo.Location = new System.Drawing.Point(0, 557);
            this.PanelLogo.Name = "PanelLogo";
            this.PanelLogo.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.PanelLogo.Size = new System.Drawing.Size(310, 56);
            this.PanelLogo.TabIndex = 0;
            // 
            // PictureLogo
            // 
            this.PictureLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PictureLogo.Image = global::Gorgon.Editor.Properties.Resources.Gorgon_Logo_Small;
            this.PictureLogo.Location = new System.Drawing.Point(0, 8);
            this.PictureLogo.Name = "PictureLogo";
            this.PictureLogo.Size = new System.Drawing.Size(310, 48);
            this.PictureLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureLogo.TabIndex = 0;
            this.PictureLogo.TabStop = false;
            // 
            // ButtonBack
            // 
            this.ButtonBack.AutoSize = true;
            this.ButtonBack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonBack.FlatAppearance.BorderSize = 0;
            this.ButtonBack.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonBack.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.ButtonBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ButtonBack.Image = global::Gorgon.Editor.Properties.Resources.back_icon_24x24;
            this.ButtonBack.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBack.Location = new System.Drawing.Point(0, 0);
            this.ButtonBack.Name = "ButtonBack";
            this.ButtonBack.Size = new System.Drawing.Size(30, 613);
            this.ButtonBack.TabIndex = 5;
            this.ButtonBack.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBack.UseVisualStyleBackColor = false;
            this.ButtonBack.Click += new System.EventHandler(this.ButtonBack_Click);
            // 
            // PanelBack
            // 
            this.PanelBack.AutoSize = true;
            this.PanelBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.PanelBack.Controls.Add(this.ButtonBack);
            this.PanelBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.PanelBack.Location = new System.Drawing.Point(0, 0);
            this.PanelBack.Name = "PanelBack";
            this.PanelBack.Size = new System.Drawing.Size(30, 613);
            this.PanelBack.TabIndex = 6;
            this.PanelBack.Visible = false;
            // 
            // TipButtons
            // 
            this.TipButtons.BackColor = System.Drawing.Color.White;
            this.TipButtons.ForeColor = System.Drawing.Color.Black;
            this.TipButtons.IsBalloon = true;
            this.TipButtons.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.TipButtons.ToolTipTitle = "Info";
            // 
            // SettingsPanel
            // 
            this.SettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsPanel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.SettingsPanel.Location = new System.Drawing.Point(30, 0);
            this.SettingsPanel.Name = "SettingsPanel";
            this.SettingsPanel.Size = new System.Drawing.Size(626, 613);
            this.SettingsPanel.TabIndex = 7;
            // 
            // NewProject
            // 
            this.NewProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.NewProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NewProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.NewProject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.NewProject.Location = new System.Drawing.Point(30, 0);
            this.NewProject.Name = "NewProject";
            this.NewProject.Padding = new System.Windows.Forms.Padding(6, 0, 2, 0);
            this.NewProject.Size = new System.Drawing.Size(626, 613);
            this.NewProject.TabIndex = 2;
            // 
            // Recent
            // 
            this.Recent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.Recent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Recent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Recent.ForeColor = System.Drawing.Color.White;
            this.Recent.Location = new System.Drawing.Point(30, 0);
            this.Recent.Name = "Recent";
            this.Recent.Padding = new System.Windows.Forms.Padding(6, 0, 2, 0);
            this.Recent.Size = new System.Drawing.Size(626, 613);
            this.Recent.TabIndex = 1;
            // 
            // Stage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SettingsPanel);
            this.Controls.Add(this.NewProject);
            this.Controls.Add(this.Recent);
            this.Controls.Add(this.PanelFunctions);
            this.Controls.Add(this.PanelBack);
            this.Name = "Stage";
            this.Size = new System.Drawing.Size(966, 613);
            this.PanelFunctions.ResumeLayout(false);
            this.PanelButtons.ResumeLayout(false);
            this.PanelButtons.PerformLayout();
            this.PanelLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureLogo)).EndInit();
            this.PanelBack.ResumeLayout(false);
            this.PanelBack.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PanelFunctions;
        private System.Windows.Forms.Panel PanelButtons;
        private System.Windows.Forms.Button ButtonSaveAs;
        private System.Windows.Forms.Button ButtonBrowse;
        private System.Windows.Forms.RadioButton CheckNew;
        private System.Windows.Forms.Panel PanelLogo;
        private System.Windows.Forms.RadioButton CheckRecent;
        private System.Windows.Forms.Button ButtonBack;
        private System.Windows.Forms.Panel PanelBack;
        public StageNew NewProject;
        public System.Windows.Forms.PictureBox PictureLogo;
        public StageRecent Recent;
        private System.Windows.Forms.Panel PanelSepSave;
        private System.Windows.Forms.ToolTip TipButtons;
        private System.Windows.Forms.Button ButtonOpenPackFile;
        private System.Windows.Forms.RadioButton CheckSettings;
        public EditorSettingsPanel SettingsPanel;
    }
}
