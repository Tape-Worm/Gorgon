namespace Gorgon.Editor
{
    partial class EditorPreferencePanel
    {
        private System.Windows.Forms.Button buttonScratch;
        private System.Windows.Forms.TextBox textScratchLocation;
        private System.Windows.Forms.Label labelScratchLocation;

        private void InitializeComponent()
        {
			this.textScratchLocation = new System.Windows.Forms.TextBox();
			this.labelScratchLocation = new System.Windows.Forms.Label();
			this.buttonScratch = new System.Windows.Forms.Button();
			this.miniToolStrip = new System.Windows.Forms.ToolStrip();
			this.buttonPlugInLocation = new System.Windows.Forms.Button();
			this.textPlugInLocation = new System.Windows.Forms.TextBox();
			this.labelPlugInLocation = new System.Windows.Forms.Label();
			this.dialogPlugInLocation = new System.Windows.Forms.FolderBrowserDialog();
			this.checkAutoLoadFile = new System.Windows.Forms.CheckBox();
			this.labelPaths = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.imagePlugInHelp = new System.Windows.Forms.PictureBox();
			this.imageScratchHelp = new System.Windows.Forms.PictureBox();
			this.panel3 = new System.Windows.Forms.Panel();
			this.numericAnimateSpeed = new System.Windows.Forms.NumericUpDown();
			this.labelAnimateSpeed = new System.Windows.Forms.Label();
			this.checkAnimateLogo = new System.Windows.Forms.CheckBox();
			this.imageImageEditorHelp = new System.Windows.Forms.PictureBox();
			this.comboImageEditor = new System.Windows.Forms.ComboBox();
			this.labelImageEditor = new System.Windows.Forms.Label();
			this.panel4 = new System.Windows.Forms.Panel();
			this.labelOptions = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.imagePlugInHelp)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imageScratchHelp)).BeginInit();
			this.panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimateSpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imageImageEditorHelp)).BeginInit();
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
			this.textScratchLocation.Size = new System.Drawing.Size(676, 23);
			this.textScratchLocation.TabIndex = 0;
			this.textScratchLocation.Enter += new System.EventHandler(this.textScratchLocation_Enter);
			// 
			// labelScratchLocation
			// 
			this.labelScratchLocation.AutoSize = true;
			this.labelScratchLocation.Location = new System.Drawing.Point(3, 21);
			this.labelScratchLocation.Name = "labelScratchLocation";
			this.labelScratchLocation.Size = new System.Drawing.Size(117, 15);
			this.labelScratchLocation.TabIndex = 6;
			this.labelScratchLocation.Text = "scratch data location";
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
			this.buttonScratch.Image = global::Gorgon.Editor.Properties.Resources.folder_open_16x16;
			this.buttonScratch.Location = new System.Drawing.Point(688, 37);
			this.buttonScratch.Name = "buttonScratch";
			this.buttonScratch.Size = new System.Drawing.Size(26, 26);
			this.buttonScratch.TabIndex = 1;
			this.buttonScratch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
			this.buttonPlugInLocation.Image = global::Gorgon.Editor.Properties.Resources.folder_open_16x16;
			this.buttonPlugInLocation.Location = new System.Drawing.Point(688, 81);
			this.buttonPlugInLocation.Name = "buttonPlugInLocation";
			this.buttonPlugInLocation.Size = new System.Drawing.Size(26, 26);
			this.buttonPlugInLocation.TabIndex = 3;
			this.buttonPlugInLocation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
			this.textPlugInLocation.Size = new System.Drawing.Size(676, 23);
			this.textPlugInLocation.TabIndex = 2;
			this.textPlugInLocation.Enter += new System.EventHandler(this.textPlugInLocation_Enter);
			// 
			// labelPlugInLocation
			// 
			this.labelPlugInLocation.AutoSize = true;
			this.labelPlugInLocation.Location = new System.Drawing.Point(3, 65);
			this.labelPlugInLocation.Name = "labelPlugInLocation";
			this.labelPlugInLocation.Size = new System.Drawing.Size(90, 15);
			this.labelPlugInLocation.TabIndex = 16;
			this.labelPlugInLocation.Text = "plug in location";
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
			// labelPaths
			// 
			this.labelPaths.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelPaths.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelPaths.Location = new System.Drawing.Point(0, 0);
			this.labelPaths.Name = "labelPaths";
			this.labelPaths.Size = new System.Drawing.Size(739, 18);
			this.labelPaths.TabIndex = 18;
			this.labelPaths.Text = "paths";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panel1.Controls.Add(this.labelPaths);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(739, 18);
			this.panel1.TabIndex = 19;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.imagePlugInHelp);
			this.panel2.Controls.Add(this.imageScratchHelp);
			this.panel2.Controls.Add(this.panel1);
			this.panel2.Controls.Add(this.labelScratchLocation);
			this.panel2.Controls.Add(this.textScratchLocation);
			this.panel2.Controls.Add(this.buttonPlugInLocation);
			this.panel2.Controls.Add(this.buttonScratch);
			this.panel2.Controls.Add(this.textPlugInLocation);
			this.panel2.Controls.Add(this.labelPlugInLocation);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(3, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(739, 116);
			this.panel2.TabIndex = 0;
			// 
			// imagePlugInHelp
			// 
			this.imagePlugInHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.imagePlugInHelp.Image = global::Gorgon.Editor.Properties.Resources.info_16x16;
			this.imagePlugInHelp.Location = new System.Drawing.Point(720, 87);
			this.imagePlugInHelp.Name = "imagePlugInHelp";
			this.imagePlugInHelp.Size = new System.Drawing.Size(16, 16);
			this.imagePlugInHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.imagePlugInHelp.TabIndex = 23;
			this.imagePlugInHelp.TabStop = false;
			// 
			// imageScratchHelp
			// 
			this.imageScratchHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.imageScratchHelp.Image = global::Gorgon.Editor.Properties.Resources.info_16x16;
			this.imageScratchHelp.Location = new System.Drawing.Point(720, 43);
			this.imageScratchHelp.Name = "imageScratchHelp";
			this.imageScratchHelp.Size = new System.Drawing.Size(16, 16);
			this.imageScratchHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.imageScratchHelp.TabIndex = 22;
			this.imageScratchHelp.TabStop = false;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.numericAnimateSpeed);
			this.panel3.Controls.Add(this.labelAnimateSpeed);
			this.panel3.Controls.Add(this.checkAnimateLogo);
			this.panel3.Controls.Add(this.imageImageEditorHelp);
			this.panel3.Controls.Add(this.comboImageEditor);
			this.panel3.Controls.Add(this.labelImageEditor);
			this.panel3.Controls.Add(this.panel4);
			this.panel3.Controls.Add(this.checkAutoLoadFile);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(3, 119);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(739, 313);
			this.panel3.TabIndex = 1;
			// 
			// numericAnimateSpeed
			// 
			this.numericAnimateSpeed.DecimalPlaces = 3;
			this.numericAnimateSpeed.Increment = new decimal(new int[] {
            25,
            0,
            0,
            196608});
			this.numericAnimateSpeed.Location = new System.Drawing.Point(31, 139);
			this.numericAnimateSpeed.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericAnimateSpeed.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            196608});
			this.numericAnimateSpeed.Name = "numericAnimateSpeed";
			this.numericAnimateSpeed.Size = new System.Drawing.Size(92, 23);
			this.numericAnimateSpeed.TabIndex = 3;
			this.numericAnimateSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericAnimateSpeed.Value = new decimal(new int[] {
            25,
            0,
            0,
            196608});
			// 
			// labelAnimateSpeed
			// 
			this.labelAnimateSpeed.AutoSize = true;
			this.labelAnimateSpeed.Location = new System.Drawing.Point(28, 120);
			this.labelAnimateSpeed.Name = "labelAnimateSpeed";
			this.labelAnimateSpeed.Size = new System.Drawing.Size(95, 15);
			this.labelAnimateSpeed.TabIndex = 25;
			this.labelAnimateSpeed.Text = "animation speed";
			// 
			// checkAnimateLogo
			// 
			this.checkAnimateLogo.AutoSize = true;
			this.checkAnimateLogo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkAnimateLogo.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
			this.checkAnimateLogo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.checkAnimateLogo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.checkAnimateLogo.ForeColor = System.Drawing.Color.White;
			this.checkAnimateLogo.Location = new System.Drawing.Point(6, 94);
			this.checkAnimateLogo.Name = "checkAnimateLogo";
			this.checkAnimateLogo.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkAnimateLogo.Size = new System.Drawing.Size(161, 19);
			this.checkAnimateLogo.TabIndex = 2;
			this.checkAnimateLogo.Text = "animate the gorgon logo";
			this.checkAnimateLogo.Click += new System.EventHandler(this.checkAnimateLogo_Click);
			// 
			// imageImageEditorHelp
			// 
			this.imageImageEditorHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.imageImageEditorHelp.Image = global::Gorgon.Editor.Properties.Resources.info_16x16;
			this.imageImageEditorHelp.Location = new System.Drawing.Point(326, 44);
			this.imageImageEditorHelp.Name = "imageImageEditorHelp";
			this.imageImageEditorHelp.Size = new System.Drawing.Size(16, 16);
			this.imageImageEditorHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.imageImageEditorHelp.TabIndex = 23;
			this.imageImageEditorHelp.TabStop = false;
			// 
			// comboImageEditor
			// 
			this.comboImageEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboImageEditor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboImageEditor.FormattingEnabled = true;
			this.comboImageEditor.Location = new System.Drawing.Point(6, 40);
			this.comboImageEditor.Name = "comboImageEditor";
			this.comboImageEditor.Size = new System.Drawing.Size(314, 23);
			this.comboImageEditor.TabIndex = 0;
			// 
			// labelImageEditor
			// 
			this.labelImageEditor.AutoSize = true;
			this.labelImageEditor.Location = new System.Drawing.Point(3, 21);
			this.labelImageEditor.Name = "labelImageEditor";
			this.labelImageEditor.Size = new System.Drawing.Size(116, 15);
			this.labelImageEditor.TabIndex = 21;
			this.labelImageEditor.Text = "image editor plug-in";
			this.toolHelp.SetToolTip(this.labelImageEditor, "The default image editor plug-in.\r\n\r\nPlug-ins requiring the image editor function" +
        "ality will use the image \r\neditor selected here.  Only a single image editor may" +
        " be active at a \r\ntime.");
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panel4.Controls.Add(this.labelOptions);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(739, 18);
			this.panel4.TabIndex = 20;
			// 
			// labelOptions
			// 
			this.labelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelOptions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOptions.Location = new System.Drawing.Point(0, 0);
			this.labelOptions.Name = "labelOptions";
			this.labelOptions.Size = new System.Drawing.Size(739, 18);
			this.labelOptions.TabIndex = 18;
			this.labelOptions.Text = "options";
			// 
			// EditorPreferencePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel2);
			this.Name = "EditorPreferencePanel";
			this.Size = new System.Drawing.Size(745, 435);
			this.Text = "Editor Preferences";
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.imagePlugInHelp)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imageScratchHelp)).EndInit();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimateSpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imageImageEditorHelp)).EndInit();
			this.panel4.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        private System.Windows.Forms.ToolStrip miniToolStrip;
        private System.Windows.Forms.Button buttonPlugInLocation;
        private System.Windows.Forms.TextBox textPlugInLocation;
        private System.Windows.Forms.Label labelPlugInLocation;
        private System.Windows.Forms.FolderBrowserDialog dialogPlugInLocation;
        private System.Windows.Forms.CheckBox checkAutoLoadFile;
        private System.Windows.Forms.Label labelPaths;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ComboBox comboImageEditor;
        private System.Windows.Forms.Label labelImageEditor;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label labelOptions;
		private System.Windows.Forms.PictureBox imageScratchHelp;
		private System.Windows.Forms.PictureBox imagePlugInHelp;
		private System.Windows.Forms.PictureBox imageImageEditorHelp;
		private System.Windows.Forms.CheckBox checkAnimateLogo;
		private System.Windows.Forms.NumericUpDown numericAnimateSpeed;
		private System.Windows.Forms.Label labelAnimateSpeed;
    }
}
