namespace Gorgon.Editor.ImageEditor
{
    partial class ImageSettingsPanel
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
                DataContext?.Unload();
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
            this.TipSettings = new System.Windows.Forms.ToolTip(this.components);
            this.ButtonPath = new System.Windows.Forms.Button();
            this.ButtonClear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.TextPath = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.LabelDesc = new System.Windows.Forms.Label();
            this.DialogFileOpen = new System.Windows.Forms.OpenFileDialog();
            this.PanelBody.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.tableLayoutPanel1);
            // 
            // TipSettings
            // 
            this.TipSettings.AutoPopDelay = 10000;
            this.TipSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TipSettings.ForeColor = System.Drawing.Color.White;
            this.TipSettings.InitialDelay = 500;
            this.TipSettings.ReshowDelay = 100;
            this.TipSettings.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.TipSettings.ToolTipTitle = "Info";
            // 
            // ButtonPath
            // 
            this.ButtonPath.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ButtonPath.AutoSize = true;
            this.ButtonPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonPath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonPath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonPath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonPath.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.open_folder_16x16;
            this.ButtonPath.Location = new System.Drawing.Point(543, 24);
            this.ButtonPath.Name = "ButtonPath";
            this.ButtonPath.Size = new System.Drawing.Size(24, 24);
            this.ButtonPath.TabIndex = 2;
            this.TipSettings.SetToolTip(this.ButtonPath, "Locate the application EXE file.");
            this.ButtonPath.UseVisualStyleBackColor = false;
            this.ButtonPath.Click += new System.EventHandler(this.ButtonPath_Click);
            // 
            // ButtonClear
            // 
            this.ButtonClear.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ButtonClear.AutoSize = true;
            this.ButtonClear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonClear.Enabled = false;
            this.ButtonClear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonClear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonClear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonClear.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.delete_16x16;
            this.ButtonClear.Location = new System.Drawing.Point(573, 24);
            this.ButtonClear.Name = "ButtonClear";
            this.ButtonClear.Size = new System.Drawing.Size(24, 24);
            this.ButtonClear.TabIndex = 3;
            this.TipSettings.SetToolTip(this.ButtonClear, "Clear the path");
            this.ButtonClear.UseVisualStyleBackColor = false;
            this.ButtonClear.Click += new System.EventHandler(this.ButtonClear_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label1, 2);
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(225, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to external image editor application:";
            // 
            // TextPath
            // 
            this.TextPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TextPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextPath.Location = new System.Drawing.Point(3, 24);
            this.TextPath.Name = "TextPath";
            this.TextPath.ReadOnly = true;
            this.TextPath.Size = new System.Drawing.Size(534, 23);
            this.TextPath.TabIndex = 1;
            this.TextPath.TextChanged += new System.EventHandler(this.TextPath_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ButtonPath, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.TextPath, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.LabelDesc, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.ButtonClear, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 445);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // LabelDesc
            // 
            this.LabelDesc.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.LabelDesc, 2);
            this.LabelDesc.Location = new System.Drawing.Point(3, 59);
            this.LabelDesc.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.LabelDesc.Name = "LabelDesc";
            this.LabelDesc.Size = new System.Drawing.Size(563, 30);
            this.LabelDesc.TabIndex = 4;
            this.LabelDesc.Text = "Leaving the path empty will result in the editor choosing the editor based on win" +
    "dows file associations for PNG files.";
            // 
            // DialogFileOpen
            // 
            this.DialogFileOpen.DefaultExt = "exe";
            this.DialogFileOpen.Filter = "Application Executables (*.exe)|*.exe";
            this.DialogFileOpen.Title = "Select the image editor EXE file";
            // 
            // ImageSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ImageSettingsPanel";
            this.Text = "Image Editor Settings";
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip TipSettings;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonPath;
        private System.Windows.Forms.TextBox TextPath;
        private System.Windows.Forms.Label LabelDesc;
        private System.Windows.Forms.Button ButtonClear;
        private System.Windows.Forms.OpenFileDialog DialogFileOpen;
    }
}
