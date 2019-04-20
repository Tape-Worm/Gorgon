namespace Gorgon.Editor.Views
{
    partial class EditorSettingsPanel
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
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.LabelHeader = new System.Windows.Forms.Label();
			this.SplitSettingsNav = new System.Windows.Forms.SplitContainer();
			this.ListCategories = new System.Windows.Forms.ListBox();
			this.PluginsPanel_00000000000000000000000000000000 = new Gorgon.Editor.Views.PluginListPanel();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitSettingsNav)).BeginInit();
			this.SplitSettingsNav.Panel1.SuspendLayout();
			this.SplitSettingsNav.Panel2.SuspendLayout();
			this.SplitSettingsNav.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel2
			// 
			this.panel2.AutoSize = true;
			this.panel2.BackColor = System.Drawing.Color.SteelBlue;
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 1);
			this.panel2.Size = new System.Drawing.Size(856, 33);
			this.panel2.TabIndex = 3;
			// 
			// panel3
			// 
			this.panel3.AutoSize = true;
			this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
			this.panel3.Controls.Add(this.LabelHeader);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.panel3.Location = new System.Drawing.Point(3, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(853, 32);
			this.panel3.TabIndex = 1;
			// 
			// LabelHeader
			// 
			this.LabelHeader.AutoSize = true;
			this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelHeader.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelHeader.Location = new System.Drawing.Point(0, 0);
			this.LabelHeader.Name = "LabelHeader";
			this.LabelHeader.Size = new System.Drawing.Size(101, 32);
			this.LabelHeader.TabIndex = 1;
			this.LabelHeader.Text = "Settings";
			this.LabelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SplitSettingsNav
			// 
			this.SplitSettingsNav.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitSettingsNav.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.SplitSettingsNav.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.SplitSettingsNav.Location = new System.Drawing.Point(0, 33);
			this.SplitSettingsNav.Name = "SplitSettingsNav";
			// 
			// SplitSettingsNav.Panel1
			// 
			this.SplitSettingsNav.Panel1.Controls.Add(this.ListCategories);
			this.SplitSettingsNav.Panel1.Padding = new System.Windows.Forms.Padding(3);
			// 
			// SplitSettingsNav.Panel2
			// 
			this.SplitSettingsNav.Panel2.AutoScroll = true;
			this.SplitSettingsNav.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.SplitSettingsNav.Panel2.Controls.Add(this.PluginsPanel_00000000000000000000000000000000);
			this.SplitSettingsNav.Panel2.Padding = new System.Windows.Forms.Padding(4);
			this.SplitSettingsNav.Size = new System.Drawing.Size(856, 589);
			this.SplitSettingsNav.SplitterDistance = 256;
			this.SplitSettingsNav.TabIndex = 4;
			// 
			// ListCategories
			// 
			this.ListCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ListCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ListCategories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListCategories.Font = new System.Drawing.Font("Segoe UI", 11F);
			this.ListCategories.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.ListCategories.FormattingEnabled = true;
			this.ListCategories.ItemHeight = 20;
			this.ListCategories.Items.AddRange(new object[] {
            "Plug ins"});
			this.ListCategories.Location = new System.Drawing.Point(3, 3);
			this.ListCategories.Name = "ListCategories";
			this.ListCategories.Size = new System.Drawing.Size(250, 583);
			this.ListCategories.TabIndex = 0;
			// 
			// PluginsPanel_00000000000000000000000000000000
			// 
			this.PluginsPanel_00000000000000000000000000000000.AutoSize = true;
			this.PluginsPanel_00000000000000000000000000000000.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.PluginsPanel_00000000000000000000000000000000.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PluginsPanel_00000000000000000000000000000000.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.PluginsPanel_00000000000000000000000000000000.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.PluginsPanel_00000000000000000000000000000000.Location = new System.Drawing.Point(4, 4);
			this.PluginsPanel_00000000000000000000000000000000.Name = "PluginsPanel_00000000000000000000000000000000";
			this.PluginsPanel_00000000000000000000000000000000.Size = new System.Drawing.Size(588, 581);
			this.PluginsPanel_00000000000000000000000000000000.TabIndex = 0;
			this.PluginsPanel_00000000000000000000000000000000.Text = "Plug ins";
			// 
			// EditorSettingsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SplitSettingsNav);
			this.Controls.Add(this.panel2);
			this.Name = "EditorSettingsPanel";
			this.Size = new System.Drawing.Size(856, 622);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.SplitSettingsNav.Panel1.ResumeLayout(false);
			this.SplitSettingsNav.Panel2.ResumeLayout(false);
			this.SplitSettingsNav.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitSettingsNav)).EndInit();
			this.SplitSettingsNav.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label LabelHeader;
        private System.Windows.Forms.SplitContainer SplitSettingsNav;
        private System.Windows.Forms.ListBox ListCategories;
        private PluginListPanel PluginsPanel_00000000000000000000000000000000;
    }
}
