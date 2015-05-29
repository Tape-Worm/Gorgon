using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
    partial class EditorPreferencePanel
    {
        private Button buttonScratch;
        private TextBox textScratchLocation;
        private Label labelScratchLocation;

        private void InitializeComponent()
        {
			this.textScratchLocation = new TextBox();
			this.labelScratchLocation = new Label();
			this.buttonScratch = new Button();
			this.miniToolStrip = new ToolStrip();
			this.buttonPlugInLocation = new Button();
			this.textPlugInLocation = new TextBox();
			this.labelPlugInLocation = new Label();
			this.dialogPlugInLocation = new FolderBrowserDialog();
			this.checkAutoLoadFile = new CheckBox();
			this.labelPaths = new Label();
			this.panel1 = new Panel();
			this.panel2 = new Panel();
			this.imagePlugInHelp = new PictureBox();
			this.imageScratchHelp = new PictureBox();
			this.panel3 = new Panel();
			this.numericAnimateSpeed = new NumericUpDown();
			this.labelAnimateSpeed = new Label();
			this.checkAnimateLogo = new CheckBox();
			this.imageImageEditorHelp = new PictureBox();
			this.comboImageEditor = new ComboBox();
			this.labelImageEditor = new Label();
			this.panel4 = new Panel();
			this.labelOptions = new Label();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((ISupportInitialize)(this.imagePlugInHelp)).BeginInit();
			((ISupportInitialize)(this.imageScratchHelp)).BeginInit();
			this.panel3.SuspendLayout();
			((ISupportInitialize)(this.numericAnimateSpeed)).BeginInit();
			((ISupportInitialize)(this.imageImageEditorHelp)).BeginInit();
			this.panel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// textScratchLocation
			// 
			this.textScratchLocation.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
			this.textScratchLocation.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			this.textScratchLocation.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
			this.textScratchLocation.BorderStyle = BorderStyle.FixedSingle;
			this.textScratchLocation.Location = new Point(6, 39);
			this.textScratchLocation.Name = "textScratchLocation";
			this.textScratchLocation.Size = new Size(676, 23);
			this.textScratchLocation.TabIndex = 0;
			this.textScratchLocation.Enter += new EventHandler(this.textScratchLocation_Enter);
			// 
			// labelScratchLocation
			// 
			this.labelScratchLocation.AutoSize = true;
			this.labelScratchLocation.Location = new Point(3, 21);
			this.labelScratchLocation.Name = "labelScratchLocation";
			this.labelScratchLocation.Size = new Size(117, 15);
			this.labelScratchLocation.TabIndex = 6;
			this.labelScratchLocation.Text = "scratch data location";
			// 
			// buttonScratch
			// 
			this.buttonScratch.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.buttonScratch.BackColor = Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonScratch.FlatAppearance.CheckedBackColor = Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonScratch.FlatAppearance.MouseDownBackColor = Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonScratch.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonScratch.FlatStyle = FlatStyle.Popup;
			this.buttonScratch.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
			this.buttonScratch.ForeColor = Color.White;
			this.buttonScratch.Image = Resources.folder_open_16x16;
			this.buttonScratch.Location = new Point(688, 37);
			this.buttonScratch.Name = "buttonScratch";
			this.buttonScratch.Size = new Size(26, 26);
			this.buttonScratch.TabIndex = 1;
			this.buttonScratch.TextAlign = ContentAlignment.MiddleRight;
			this.buttonScratch.UseVisualStyleBackColor = false;
			this.buttonScratch.Click += new EventHandler(this.buttonScratch_Click);
			// 
			// miniToolStrip
			// 
			this.miniToolStrip.AutoSize = false;
			this.miniToolStrip.CanOverflow = false;
			this.miniToolStrip.Dock = DockStyle.None;
			this.miniToolStrip.GripStyle = ToolStripGripStyle.Hidden;
			this.miniToolStrip.Location = new Point(98, 3);
			this.miniToolStrip.Name = "miniToolStrip";
			this.miniToolStrip.Size = new Size(627, 25);
			this.miniToolStrip.Stretch = true;
			this.miniToolStrip.TabIndex = 1;
			this.toolHelp.SetToolTip(this.miniToolStrip, "Manages plug-ins that write out the editor content items into a\r\nspecific file fo" +
        "rmat (e.g. Zip)");
			// 
			// buttonPlugInLocation
			// 
			this.buttonPlugInLocation.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.buttonPlugInLocation.BackColor = Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonPlugInLocation.FlatAppearance.CheckedBackColor = Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonPlugInLocation.FlatAppearance.MouseDownBackColor = Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonPlugInLocation.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonPlugInLocation.FlatStyle = FlatStyle.Popup;
			this.buttonPlugInLocation.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
			this.buttonPlugInLocation.ForeColor = Color.White;
			this.buttonPlugInLocation.Image = Resources.folder_open_16x16;
			this.buttonPlugInLocation.Location = new Point(688, 81);
			this.buttonPlugInLocation.Name = "buttonPlugInLocation";
			this.buttonPlugInLocation.Size = new Size(26, 26);
			this.buttonPlugInLocation.TabIndex = 3;
			this.buttonPlugInLocation.TextAlign = ContentAlignment.MiddleRight;
			this.buttonPlugInLocation.UseVisualStyleBackColor = false;
			this.buttonPlugInLocation.Click += new EventHandler(this.buttonPlugInLocation_Click);
			// 
			// textPlugInLocation
			// 
			this.textPlugInLocation.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
			this.textPlugInLocation.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			this.textPlugInLocation.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
			this.textPlugInLocation.BorderStyle = BorderStyle.FixedSingle;
			this.textPlugInLocation.Location = new Point(6, 83);
			this.textPlugInLocation.Name = "textPlugInLocation";
			this.textPlugInLocation.Size = new Size(676, 23);
			this.textPlugInLocation.TabIndex = 2;
			this.textPlugInLocation.Enter += new EventHandler(this.textPlugInLocation_Enter);
			// 
			// labelPlugInLocation
			// 
			this.labelPlugInLocation.AutoSize = true;
			this.labelPlugInLocation.Location = new Point(3, 65);
			this.labelPlugInLocation.Name = "labelPlugInLocation";
			this.labelPlugInLocation.Size = new Size(90, 15);
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
			this.checkAutoLoadFile.FlatAppearance.BorderColor = Color.Black;
			this.checkAutoLoadFile.FlatAppearance.CheckedBackColor = Color.DimGray;
			this.checkAutoLoadFile.FlatAppearance.MouseDownBackColor = Color.SteelBlue;
			this.checkAutoLoadFile.FlatAppearance.MouseOverBackColor = Color.SteelBlue;
			this.checkAutoLoadFile.ForeColor = Color.White;
			this.checkAutoLoadFile.Location = new Point(6, 69);
			this.checkAutoLoadFile.Name = "checkAutoLoadFile";
			this.checkAutoLoadFile.Padding = new Padding(3, 0, 0, 0);
			this.checkAutoLoadFile.Size = new Size(195, 19);
			this.checkAutoLoadFile.TabIndex = 1;
			this.checkAutoLoadFile.Text = "load last opened file on start up";
			// 
			// labelPaths
			// 
			this.labelPaths.Dock = DockStyle.Fill;
			this.labelPaths.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
			this.labelPaths.Location = new Point(0, 0);
			this.labelPaths.Name = "labelPaths";
			this.labelPaths.Size = new Size(739, 18);
			this.labelPaths.TabIndex = 18;
			this.labelPaths.Text = "paths";
			// 
			// panel1
			// 
			this.panel1.BackColor = Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panel1.Controls.Add(this.labelPaths);
			this.panel1.Dock = DockStyle.Top;
			this.panel1.Location = new Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(739, 18);
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
			this.panel2.Dock = DockStyle.Top;
			this.panel2.Location = new Point(3, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new Size(739, 116);
			this.panel2.TabIndex = 0;
			// 
			// imagePlugInHelp
			// 
			this.imagePlugInHelp.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.imagePlugInHelp.Image = Resources.info_16x16;
			this.imagePlugInHelp.Location = new Point(720, 87);
			this.imagePlugInHelp.Name = "imagePlugInHelp";
			this.imagePlugInHelp.Size = new Size(16, 16);
			this.imagePlugInHelp.SizeMode = PictureBoxSizeMode.AutoSize;
			this.imagePlugInHelp.TabIndex = 23;
			this.imagePlugInHelp.TabStop = false;
			// 
			// imageScratchHelp
			// 
			this.imageScratchHelp.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.imageScratchHelp.Image = Resources.info_16x16;
			this.imageScratchHelp.Location = new Point(720, 43);
			this.imageScratchHelp.Name = "imageScratchHelp";
			this.imageScratchHelp.Size = new Size(16, 16);
			this.imageScratchHelp.SizeMode = PictureBoxSizeMode.AutoSize;
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
			this.panel3.Dock = DockStyle.Fill;
			this.panel3.Location = new Point(3, 119);
			this.panel3.Name = "panel3";
			this.panel3.Size = new Size(739, 313);
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
			this.numericAnimateSpeed.Location = new Point(31, 139);
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
			this.numericAnimateSpeed.Size = new Size(92, 23);
			this.numericAnimateSpeed.TabIndex = 3;
			this.numericAnimateSpeed.TextAlign = HorizontalAlignment.Right;
			this.numericAnimateSpeed.Value = new decimal(new int[] {
            25,
            0,
            0,
            196608});
			// 
			// labelAnimateSpeed
			// 
			this.labelAnimateSpeed.AutoSize = true;
			this.labelAnimateSpeed.Location = new Point(28, 120);
			this.labelAnimateSpeed.Name = "labelAnimateSpeed";
			this.labelAnimateSpeed.Size = new Size(95, 15);
			this.labelAnimateSpeed.TabIndex = 25;
			this.labelAnimateSpeed.Text = "animation speed";
			// 
			// checkAnimateLogo
			// 
			this.checkAnimateLogo.AutoSize = true;
			this.checkAnimateLogo.FlatAppearance.BorderColor = Color.Black;
			this.checkAnimateLogo.FlatAppearance.CheckedBackColor = Color.DimGray;
			this.checkAnimateLogo.FlatAppearance.MouseDownBackColor = Color.SteelBlue;
			this.checkAnimateLogo.FlatAppearance.MouseOverBackColor = Color.SteelBlue;
			this.checkAnimateLogo.ForeColor = Color.White;
			this.checkAnimateLogo.Location = new Point(6, 94);
			this.checkAnimateLogo.Name = "checkAnimateLogo";
			this.checkAnimateLogo.Padding = new Padding(3, 0, 0, 0);
			this.checkAnimateLogo.Size = new Size(161, 19);
			this.checkAnimateLogo.TabIndex = 2;
			this.checkAnimateLogo.Text = "animate the gorgon logo";
			this.checkAnimateLogo.Click += new EventHandler(this.checkAnimateLogo_Click);
			// 
			// imageImageEditorHelp
			// 
			this.imageImageEditorHelp.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.imageImageEditorHelp.Image = Resources.info_16x16;
			this.imageImageEditorHelp.Location = new Point(326, 44);
			this.imageImageEditorHelp.Name = "imageImageEditorHelp";
			this.imageImageEditorHelp.Size = new Size(16, 16);
			this.imageImageEditorHelp.SizeMode = PictureBoxSizeMode.AutoSize;
			this.imageImageEditorHelp.TabIndex = 23;
			this.imageImageEditorHelp.TabStop = false;
			// 
			// comboImageEditor
			// 
			this.comboImageEditor.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
			this.comboImageEditor.DropDownStyle = ComboBoxStyle.DropDownList;
			this.comboImageEditor.FormattingEnabled = true;
			this.comboImageEditor.Location = new Point(6, 40);
			this.comboImageEditor.Name = "comboImageEditor";
			this.comboImageEditor.Size = new Size(314, 23);
			this.comboImageEditor.TabIndex = 0;
			// 
			// labelImageEditor
			// 
			this.labelImageEditor.AutoSize = true;
			this.labelImageEditor.Location = new Point(3, 21);
			this.labelImageEditor.Name = "labelImageEditor";
			this.labelImageEditor.Size = new Size(116, 15);
			this.labelImageEditor.TabIndex = 21;
			this.labelImageEditor.Text = "image editor plug-in";
			this.toolHelp.SetToolTip(this.labelImageEditor, "The default image editor plug-in.\r\n\r\nPlug-ins requiring the image editor function" +
        "ality will use the image \r\neditor selected here.  Only a single image editor may" +
        " be active at a \r\ntime.");
			// 
			// panel4
			// 
			this.panel4.BackColor = Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panel4.Controls.Add(this.labelOptions);
			this.panel4.Dock = DockStyle.Top;
			this.panel4.Location = new Point(0, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new Size(739, 18);
			this.panel4.TabIndex = 20;
			// 
			// labelOptions
			// 
			this.labelOptions.Dock = DockStyle.Fill;
			this.labelOptions.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
			this.labelOptions.Location = new Point(0, 0);
			this.labelOptions.Name = "labelOptions";
			this.labelOptions.Size = new Size(739, 18);
			this.labelOptions.TabIndex = 18;
			this.labelOptions.Text = "options";
			// 
			// EditorPreferencePanel
			// 
			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel2);
			this.Name = "EditorPreferencePanel";
			this.Size = new Size(745, 435);
			this.Text = "Editor Preferences";
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((ISupportInitialize)(this.imagePlugInHelp)).EndInit();
			((ISupportInitialize)(this.imageScratchHelp)).EndInit();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			((ISupportInitialize)(this.numericAnimateSpeed)).EndInit();
			((ISupportInitialize)(this.imageImageEditorHelp)).EndInit();
			this.panel4.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        private ToolStrip miniToolStrip;
        private Button buttonPlugInLocation;
        private TextBox textPlugInLocation;
        private Label labelPlugInLocation;
        private FolderBrowserDialog dialogPlugInLocation;
        private CheckBox checkAutoLoadFile;
        private Label labelPaths;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private ComboBox comboImageEditor;
        private Label labelImageEditor;
        private Panel panel4;
        private Label labelOptions;
		private PictureBox imageScratchHelp;
		private PictureBox imagePlugInHelp;
		private PictureBox imageImageEditorHelp;
		private CheckBox checkAnimateLogo;
		private NumericUpDown numericAnimateSpeed;
		private Label labelAnimateSpeed;
    }
}
