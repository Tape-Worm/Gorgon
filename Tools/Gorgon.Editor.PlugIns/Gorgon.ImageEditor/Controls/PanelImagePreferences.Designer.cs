namespace GorgonLibrary.Editor.ImageEditorPlugIn.Controls
{
	partial class PanelImagePreferences
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
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.checkShowActualSize = new System.Windows.Forms.CheckBox();
            this.checkShowAnimations = new System.Windows.Forms.CheckBox();
            this.labelImageEditorSettings = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.containerCodecs = new System.Windows.Forms.ToolStripContainer();
            this.listCodecs = new System.Windows.Forms.ListView();
            this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelCannotEdit = new System.Windows.Forms.Panel();
            this.labelCannotEdit = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.stripCodecs = new System.Windows.Forms.ToolStrip();
            this.buttonAddCodec = new System.Windows.Forms.ToolStripButton();
            this.buttonRemoveCodec = new System.Windows.Forms.ToolStripButton();
            this.labelImageCodecs = new System.Windows.Forms.Label();
            this.dialogOpen = new System.Windows.Forms.OpenFileDialog();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.labelScalingFilters = new System.Windows.Forms.Label();
            this.labelMipMapFilter = new System.Windows.Forms.Label();
            this.labelScaleFilter = new System.Windows.Forms.Label();
            this.comboMipMapFilter = new System.Windows.Forms.ComboBox();
            this.comboResizeFilter = new System.Windows.Forms.ComboBox();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.containerCodecs.ContentPanel.SuspendLayout();
            this.containerCodecs.TopToolStripPanel.SuspendLayout();
            this.containerCodecs.SuspendLayout();
            this.panelCannotEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.stripCodecs.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Controls.Add(this.labelImageEditorSettings);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(3, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(623, 75);
            this.panel5.TabIndex = 2;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.panel6.Controls.Add(this.checkShowActualSize);
            this.panel6.Controls.Add(this.checkShowAnimations);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(0, 18);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(623, 57);
            this.panel6.TabIndex = 1;
            // 
            // checkShowActualSize
            // 
            this.checkShowActualSize.AutoSize = true;
            this.checkShowActualSize.Location = new System.Drawing.Point(7, 31);
            this.checkShowActualSize.Name = "checkShowActualSize";
            this.checkShowActualSize.Size = new System.Drawing.Size(276, 19);
            this.checkShowActualSize.TabIndex = 1;
            this.checkShowActualSize.Text = "notlocalized show images as actual size on load";
            this.checkShowActualSize.UseVisualStyleBackColor = true;
            // 
            // checkShowAnimations
            // 
            this.checkShowAnimations.AutoSize = true;
            this.checkShowAnimations.Location = new System.Drawing.Point(7, 6);
            this.checkShowAnimations.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.checkShowAnimations.Name = "checkShowAnimations";
            this.checkShowAnimations.Size = new System.Drawing.Size(236, 19);
            this.checkShowAnimations.TabIndex = 0;
            this.checkShowAnimations.Text = "notlocalized show transition animations";
            this.checkShowAnimations.UseVisualStyleBackColor = true;
            // 
            // labelImageEditorSettings
            // 
            this.labelImageEditorSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelImageEditorSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelImageEditorSettings.Location = new System.Drawing.Point(0, 0);
            this.labelImageEditorSettings.Name = "labelImageEditorSettings";
            this.labelImageEditorSettings.Size = new System.Drawing.Size(623, 18);
            this.labelImageEditorSettings.TabIndex = 0;
            this.labelImageEditorSettings.Text = "not localized image editor settings";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.labelImageCodecs);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 78);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(623, 240);
            this.panel1.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.panel2.Controls.Add(this.containerCodecs);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 18);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(623, 222);
            this.panel2.TabIndex = 1;
            // 
            // containerCodecs
            // 
            // 
            // containerCodecs.ContentPanel
            // 
            this.containerCodecs.ContentPanel.Controls.Add(this.listCodecs);
            this.containerCodecs.ContentPanel.Controls.Add(this.panelCannotEdit);
            this.containerCodecs.ContentPanel.Size = new System.Drawing.Size(623, 197);
            this.containerCodecs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containerCodecs.Location = new System.Drawing.Point(0, 0);
            this.containerCodecs.Name = "containerCodecs";
            this.containerCodecs.Size = new System.Drawing.Size(623, 222);
            this.containerCodecs.TabIndex = 7;
            this.containerCodecs.Text = "toolStripContainer1";
            // 
            // containerCodecs.TopToolStripPanel
            // 
            this.containerCodecs.TopToolStripPanel.Controls.Add(this.stripCodecs);
            // 
            // listCodecs
            // 
            this.listCodecs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listCodecs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnName,
            this.columnDesc,
            this.columnPath});
            this.listCodecs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCodecs.FullRowSelect = true;
            this.listCodecs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listCodecs.Location = new System.Drawing.Point(0, 23);
            this.listCodecs.Name = "listCodecs";
            this.listCodecs.Size = new System.Drawing.Size(623, 174);
            this.listCodecs.TabIndex = 0;
            this.listCodecs.UseCompatibleStateImageBehavior = false;
            this.listCodecs.View = System.Windows.Forms.View.Details;
            this.listCodecs.SelectedIndexChanged += new System.EventHandler(this.listCodecs_SelectedIndexChanged);
            // 
            // columnName
            // 
            this.columnName.Text = "Not localized name";
            // 
            // columnDesc
            // 
            this.columnDesc.Text = "Not localized desc";
            // 
            // columnPath
            // 
            this.columnPath.Text = "Not localized path";
            // 
            // panelCannotEdit
            // 
            this.panelCannotEdit.AutoSize = true;
            this.panelCannotEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelCannotEdit.BackColor = System.Drawing.SystemColors.Info;
            this.panelCannotEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCannotEdit.Controls.Add(this.labelCannotEdit);
            this.panelCannotEdit.Controls.Add(this.pictureBox1);
            this.panelCannotEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCannotEdit.ForeColor = System.Drawing.SystemColors.InfoText;
            this.panelCannotEdit.Location = new System.Drawing.Point(0, 0);
            this.panelCannotEdit.Name = "panelCannotEdit";
            this.panelCannotEdit.Size = new System.Drawing.Size(623, 23);
            this.panelCannotEdit.TabIndex = 1;
            this.panelCannotEdit.Visible = false;
            // 
            // labelCannotEdit
            // 
            this.labelCannotEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCannotEdit.Location = new System.Drawing.Point(16, 0);
            this.labelCannotEdit.Name = "labelCannotEdit";
            this.labelCannotEdit.Size = new System.Drawing.Size(605, 21);
            this.labelCannotEdit.TabIndex = 1;
            this.labelCannotEdit.Text = "Not localized cannot edit";
            this.labelCannotEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.info_16x16;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 21);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // stripCodecs
            // 
            this.stripCodecs.Dock = System.Windows.Forms.DockStyle.None;
            this.stripCodecs.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripCodecs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddCodec,
            this.buttonRemoveCodec});
            this.stripCodecs.Location = new System.Drawing.Point(0, 0);
            this.stripCodecs.Name = "stripCodecs";
            this.stripCodecs.Size = new System.Drawing.Size(623, 25);
            this.stripCodecs.Stretch = true;
            this.stripCodecs.TabIndex = 0;
            // 
            // buttonAddCodec
            // 
            this.buttonAddCodec.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.image_add_16x16;
            this.buttonAddCodec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddCodec.Name = "buttonAddCodec";
            this.buttonAddCodec.Size = new System.Drawing.Size(154, 22);
            this.buttonAddCodec.Text = "Not localized add codec";
            this.buttonAddCodec.Click += new System.EventHandler(this.buttonAddCodec_Click);
            // 
            // buttonRemoveCodec
            // 
            this.buttonRemoveCodec.Enabled = false;
            this.buttonRemoveCodec.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.remove_image_16x16;
            this.buttonRemoveCodec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemoveCodec.Name = "buttonRemoveCodec";
            this.buttonRemoveCodec.Size = new System.Drawing.Size(174, 22);
            this.buttonRemoveCodec.Text = "Not localized remove codec";
            this.buttonRemoveCodec.Click += new System.EventHandler(this.buttonRemoveCodec_Click);
            // 
            // labelImageCodecs
            // 
            this.labelImageCodecs.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelImageCodecs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelImageCodecs.Location = new System.Drawing.Point(0, 0);
            this.labelImageCodecs.Name = "labelImageCodecs";
            this.labelImageCodecs.Size = new System.Drawing.Size(623, 18);
            this.labelImageCodecs.TabIndex = 0;
            this.labelImageCodecs.Text = "not localized image codecs";
            // 
            // dialogOpen
            // 
            this.dialogOpen.DefaultExt = "dll";
            this.dialogOpen.Title = "Not localized codec DLLs";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Controls.Add(this.labelScalingFilters);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 318);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(623, 75);
            this.panel3.TabIndex = 4;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.panel4.Controls.Add(this.comboResizeFilter);
            this.panel4.Controls.Add(this.comboMipMapFilter);
            this.panel4.Controls.Add(this.labelScaleFilter);
            this.panel4.Controls.Add(this.labelMipMapFilter);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 18);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(623, 57);
            this.panel4.TabIndex = 1;
            // 
            // labelScalingFilters
            // 
            this.labelScalingFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelScalingFilters.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelScalingFilters.Location = new System.Drawing.Point(0, 0);
            this.labelScalingFilters.Name = "labelScalingFilters";
            this.labelScalingFilters.Size = new System.Drawing.Size(623, 18);
            this.labelScalingFilters.TabIndex = 0;
            this.labelScalingFilters.Text = "not localized image scaling filtering";
            // 
            // labelMipMapFilter
            // 
            this.labelMipMapFilter.AutoSize = true;
            this.labelMipMapFilter.Location = new System.Drawing.Point(4, 3);
            this.labelMipMapFilter.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelMipMapFilter.Name = "labelMipMapFilter";
            this.labelMipMapFilter.Size = new System.Drawing.Size(247, 15);
            this.labelMipMapFilter.TabIndex = 0;
            this.labelMipMapFilter.Text = "not localized filtering for mip map generation";
            // 
            // labelScaleFilter
            // 
            this.labelScaleFilter.AutoSize = true;
            this.labelScaleFilter.Location = new System.Drawing.Point(189, 3);
            this.labelScaleFilter.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelScaleFilter.Name = "labelScaleFilter";
            this.labelScaleFilter.Size = new System.Drawing.Size(215, 15);
            this.labelScaleFilter.TabIndex = 1;
            this.labelScaleFilter.Text = "not localized filtering for image resizing";
            // 
            // comboMipMapFilter
            // 
            this.comboMipMapFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMipMapFilter.FormattingEnabled = true;
            this.comboMipMapFilter.Location = new System.Drawing.Point(7, 21);
            this.comboMipMapFilter.Name = "comboMipMapFilter";
            this.comboMipMapFilter.Size = new System.Drawing.Size(167, 23);
            this.comboMipMapFilter.TabIndex = 2;
            // 
            // comboResizeFilter
            // 
            this.comboResizeFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboResizeFilter.FormattingEnabled = true;
            this.comboResizeFilter.Location = new System.Drawing.Point(192, 21);
            this.comboResizeFilter.Name = "comboResizeFilter";
            this.comboResizeFilter.Size = new System.Drawing.Size(179, 23);
            this.comboResizeFilter.TabIndex = 3;
            // 
            // PanelImagePreferences
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel5);
            this.Name = "PanelImagePreferences";
            this.Size = new System.Drawing.Size(629, 442);
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.containerCodecs.ContentPanel.ResumeLayout(false);
            this.containerCodecs.ContentPanel.PerformLayout();
            this.containerCodecs.TopToolStripPanel.ResumeLayout(false);
            this.containerCodecs.TopToolStripPanel.PerformLayout();
            this.containerCodecs.ResumeLayout(false);
            this.containerCodecs.PerformLayout();
            this.panelCannotEdit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.stripCodecs.ResumeLayout(false);
            this.stripCodecs.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.CheckBox checkShowAnimations;
		private System.Windows.Forms.Label labelImageEditorSettings;
		private System.Windows.Forms.CheckBox checkShowActualSize;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label labelImageCodecs;
		private System.Windows.Forms.ToolStripContainer containerCodecs;
		private System.Windows.Forms.ListView listCodecs;
		private System.Windows.Forms.ColumnHeader columnName;
		private System.Windows.Forms.ColumnHeader columnDesc;
		private System.Windows.Forms.ColumnHeader columnPath;
		private System.Windows.Forms.ToolStrip stripCodecs;
		private System.Windows.Forms.ToolStripButton buttonAddCodec;
		private System.Windows.Forms.ToolStripButton buttonRemoveCodec;
		private System.Windows.Forms.OpenFileDialog dialogOpen;
		private System.Windows.Forms.Panel panelCannotEdit;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label labelCannotEdit;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label labelScalingFilters;
        private System.Windows.Forms.Label labelScaleFilter;
        private System.Windows.Forms.Label labelMipMapFilter;
        private System.Windows.Forms.ComboBox comboResizeFilter;
        private System.Windows.Forms.ComboBox comboMipMapFilter;

	}
}
