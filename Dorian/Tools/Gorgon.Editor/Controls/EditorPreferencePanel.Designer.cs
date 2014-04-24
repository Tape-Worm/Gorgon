namespace GorgonLibrary.Editor
{
    partial class EditorPreferencePanel
    {
        private System.Windows.Forms.Button buttonScratch;
        private System.Windows.Forms.TextBox textScratchLocation;
        private System.Windows.Forms.Label label1;

        private void InitializeComponent()
        {
            this.textScratchLocation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonScratch = new System.Windows.Forms.Button();
            this.miniToolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonPlugInLocation = new System.Windows.Forms.Button();
            this.textPlugInLocation = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dialogPlugInLocation = new System.Windows.Forms.FolderBrowserDialog();
            this.checkAutoLoadFile = new System.Windows.Forms.CheckBox();
            this.labelFontEditorSettings = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.comboImageEditor = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // textScratchLocation
            // 
            this.textScratchLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textScratchLocation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textScratchLocation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.textScratchLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textScratchLocation.Location = new System.Drawing.Point(6, 39);
            this.textScratchLocation.Name = "textScratchLocation";
            this.textScratchLocation.Size = new System.Drawing.Size(695, 23);
            this.textScratchLocation.TabIndex = 0;
            this.toolHelp.SetToolTip(this.textScratchLocation, "Selects a new working area for the content in the editor.\r\n\r\nChanging this value " +
        "will require a restart of the application.");
            this.textScratchLocation.Enter += new System.EventHandler(this.textScratchLocation_Enter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "scratch data location";
            // 
            // buttonScratch
            // 
            this.buttonScratch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScratch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonScratch.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonScratch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonScratch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonScratch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonScratch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonScratch.ForeColor = System.Drawing.Color.White;
            this.buttonScratch.Image = global::GorgonLibrary.Editor.Properties.Resources.folder_open_16x16;
            this.buttonScratch.Location = new System.Drawing.Point(707, 38);
            this.buttonScratch.Name = "buttonScratch";
            this.buttonScratch.Size = new System.Drawing.Size(26, 26);
            this.buttonScratch.TabIndex = 1;
            this.buttonScratch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolHelp.SetToolTip(this.buttonScratch, "Selects a new working area for the content in the editor.\r\n\r\nChanging this value " +
        "will require a restart of the application.");
            this.buttonScratch.UseVisualStyleBackColor = false;
            this.buttonScratch.Click += new System.EventHandler(this.buttonScratch_Click);
            // 
            // miniToolStrip
            // 
            this.miniToolStrip.AutoSize = false;
            this.miniToolStrip.CanOverflow = false;
            this.miniToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.miniToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.miniToolStrip.Location = new System.Drawing.Point(98, 3);
            this.miniToolStrip.Name = "miniToolStrip";
            this.miniToolStrip.Size = new System.Drawing.Size(627, 25);
            this.miniToolStrip.Stretch = true;
            this.miniToolStrip.TabIndex = 1;
            this.toolHelp.SetToolTip(this.miniToolStrip, "Manages plug-ins that write out the editor content items into a\r\nspecific file fo" +
        "rmat (e.g. Zip)");
            // 
            // buttonPlugInLocation
            // 
            this.buttonPlugInLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlugInLocation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonPlugInLocation.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonPlugInLocation.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonPlugInLocation.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonPlugInLocation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonPlugInLocation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPlugInLocation.ForeColor = System.Drawing.Color.White;
            this.buttonPlugInLocation.Image = global::GorgonLibrary.Editor.Properties.Resources.folder_open_16x16;
            this.buttonPlugInLocation.Location = new System.Drawing.Point(707, 82);
            this.buttonPlugInLocation.Name = "buttonPlugInLocation";
            this.buttonPlugInLocation.Size = new System.Drawing.Size(26, 26);
            this.buttonPlugInLocation.TabIndex = 3;
            this.buttonPlugInLocation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolHelp.SetToolTip(this.buttonPlugInLocation, "Selects a location for plug-ins to use with the editor.\r\n\r\nChanging this value wi" +
        "ll require a restart of the application.");
            this.buttonPlugInLocation.UseVisualStyleBackColor = false;
            this.buttonPlugInLocation.Click += new System.EventHandler(this.buttonPlugInLocation_Click);
            // 
            // textPlugInLocation
            // 
            this.textPlugInLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPlugInLocation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textPlugInLocation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.textPlugInLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textPlugInLocation.Location = new System.Drawing.Point(6, 83);
            this.textPlugInLocation.Name = "textPlugInLocation";
            this.textPlugInLocation.Size = new System.Drawing.Size(695, 23);
            this.textPlugInLocation.TabIndex = 2;
            this.toolHelp.SetToolTip(this.textPlugInLocation, "Selects a location for plug-ins to use with the editor.\r\n\r\nChanging this value wi" +
        "ll require a restart of the application.");
            this.textPlugInLocation.Enter += new System.EventHandler(this.textPlugInLocation_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 15);
            this.label2.TabIndex = 16;
            this.label2.Text = "plug in location";
            // 
            // dialogPlugInLocation
            // 
            this.dialogPlugInLocation.Description = "Select a plug-in location";
            this.dialogPlugInLocation.ShowNewFolderButton = false;
            // 
            // checkAutoLoadFile
            // 
            this.checkAutoLoadFile.AutoSize = true;
            this.checkAutoLoadFile.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.checkAutoLoadFile.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.checkAutoLoadFile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.checkAutoLoadFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.checkAutoLoadFile.ForeColor = System.Drawing.Color.White;
            this.checkAutoLoadFile.Location = new System.Drawing.Point(6, 69);
            this.checkAutoLoadFile.Name = "checkAutoLoadFile";
            this.checkAutoLoadFile.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.checkAutoLoadFile.Size = new System.Drawing.Size(195, 19);
            this.checkAutoLoadFile.TabIndex = 1;
            this.checkAutoLoadFile.Text = "load last opened file on start up";
            // 
            // labelFontEditorSettings
            // 
            this.labelFontEditorSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelFontEditorSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFontEditorSettings.Location = new System.Drawing.Point(0, 0);
            this.labelFontEditorSettings.Name = "labelFontEditorSettings";
            this.labelFontEditorSettings.Size = new System.Drawing.Size(739, 18);
            this.labelFontEditorSettings.TabIndex = 18;
            this.labelFontEditorSettings.Text = "paths";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.panel1.Controls.Add(this.labelFontEditorSettings);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(739, 18);
            this.panel1.TabIndex = 19;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.textScratchLocation);
            this.panel2.Controls.Add(this.buttonPlugInLocation);
            this.panel2.Controls.Add(this.buttonScratch);
            this.panel2.Controls.Add(this.textPlugInLocation);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(739, 116);
            this.panel2.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.comboImageEditor);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Controls.Add(this.checkAutoLoadFile);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 119);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(739, 413);
            this.panel3.TabIndex = 1;
            // 
            // comboImageEditor
            // 
            this.comboImageEditor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboImageEditor.FormattingEnabled = true;
            this.comboImageEditor.Location = new System.Drawing.Point(6, 40);
            this.comboImageEditor.Name = "comboImageEditor";
            this.comboImageEditor.Size = new System.Drawing.Size(314, 23);
            this.comboImageEditor.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 15);
            this.label4.TabIndex = 21;
            this.label4.Text = "image editor plug-in";
            this.toolHelp.SetToolTip(this.label4, "The default image editor plug-in.\r\n\r\nPlug-ins requiring the image editor function" +
        "ality will use the image \r\neditor selected here.  Only a single image editor may" +
        " be active at a \r\ntime.");
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.panel4.Controls.Add(this.label3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(739, 18);
            this.panel4.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(739, 18);
            this.label3.TabIndex = 18;
            this.label3.Text = "options";
            // 
            // EditorPreferencePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Name = "EditorPreferencePanel";
            this.Size = new System.Drawing.Size(745, 535);
            this.Text = "Editor Preferences";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.ToolStrip miniToolStrip;
        private System.Windows.Forms.Button buttonPlugInLocation;
        private System.Windows.Forms.TextBox textPlugInLocation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FolderBrowserDialog dialogPlugInLocation;
        private System.Windows.Forms.CheckBox checkAutoLoadFile;
        private System.Windows.Forms.Label labelFontEditorSettings;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ComboBox comboImageEditor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label3;
    }
}
