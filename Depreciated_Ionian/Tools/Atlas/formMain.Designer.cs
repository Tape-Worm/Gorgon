namespace Atlas
{
	partial class formMain
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
			this.containerMain = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.labelPreviousAtlas = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelCurrentAtlas = new System.Windows.Forms.ToolStripStatusLabel();
			this.labelNextAtlas = new System.Windows.Forms.ToolStripStatusLabel();
			this.pictureAtlas = new System.Windows.Forms.PictureBox();
			this.stripMenu = new System.Windows.Forms.ToolStrip();
			this.buttonClear = new System.Windows.Forms.ToolStripButton();
			this.buttonLoadImage = new System.Windows.Forms.ToolStripButton();
			this.buttonSaveAtlas = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonAtlasSettings = new System.Windows.Forms.ToolStripButton();
			this.dialogOpen = new System.Windows.Forms.OpenFileDialog();
			this.dialogSave = new System.Windows.Forms.SaveFileDialog();
			this.containerMain.BottomToolStripPanel.SuspendLayout();
			this.containerMain.ContentPanel.SuspendLayout();
			this.containerMain.TopToolStripPanel.SuspendLayout();
			this.containerMain.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureAtlas)).BeginInit();
			this.stripMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerMain
			// 
			// 
			// containerMain.BottomToolStripPanel
			// 
			this.containerMain.BottomToolStripPanel.Controls.Add(this.statusStrip1);
			// 
			// containerMain.ContentPanel
			// 
			this.containerMain.ContentPanel.BackgroundImage = global::Atlas.Properties.Resources.Pattern;
			this.containerMain.ContentPanel.Controls.Add(this.pictureAtlas);
			this.containerMain.ContentPanel.Size = new System.Drawing.Size(513, 299);
			this.containerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerMain.LeftToolStripPanelVisible = false;
			this.containerMain.Location = new System.Drawing.Point(0, 0);
			this.containerMain.Name = "containerMain";
			this.containerMain.RightToolStripPanelVisible = false;
			this.containerMain.Size = new System.Drawing.Size(513, 346);
			this.containerMain.TabIndex = 0;
			this.containerMain.Text = "toolStripContainer1";
			// 
			// containerMain.TopToolStripPanel
			// 
			this.containerMain.TopToolStripPanel.Controls.Add(this.stripMenu);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelPreviousAtlas,
            this.labelCurrentAtlas,
            this.labelNextAtlas});
			this.statusStrip1.Location = new System.Drawing.Point(0, 0);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(513, 22);
			this.statusStrip1.TabIndex = 0;
			// 
			// labelPreviousAtlas
			// 
			this.labelPreviousAtlas.Enabled = false;
			this.labelPreviousAtlas.Name = "labelPreviousAtlas";
			this.labelPreviousAtlas.Size = new System.Drawing.Size(15, 17);
			this.labelPreviousAtlas.Text = "<";
			this.labelPreviousAtlas.MouseEnter += new System.EventHandler(this.labelPreviousAtlas_MouseEnter);
			this.labelPreviousAtlas.MouseLeave += new System.EventHandler(this.labelPreviousAtlas_MouseLeave);
			this.labelPreviousAtlas.Click += new System.EventHandler(this.labelPreviousAtlas_Click);
			// 
			// labelCurrentAtlas
			// 
			this.labelCurrentAtlas.Name = "labelCurrentAtlas";
			this.labelCurrentAtlas.Size = new System.Drawing.Size(24, 17);
			this.labelCurrentAtlas.Text = "0/0";
			// 
			// labelNextAtlas
			// 
			this.labelNextAtlas.Enabled = false;
			this.labelNextAtlas.Name = "labelNextAtlas";
			this.labelNextAtlas.Size = new System.Drawing.Size(15, 17);
			this.labelNextAtlas.Text = ">";
			this.labelNextAtlas.MouseEnter += new System.EventHandler(this.labelNextAtlas_MouseEnter);
			this.labelNextAtlas.MouseLeave += new System.EventHandler(this.labelNextAtlas_MouseLeave);
			this.labelNextAtlas.Click += new System.EventHandler(this.labelNextAtlas_Click);
			// 
			// pictureAtlas
			// 
			this.pictureAtlas.BackColor = System.Drawing.Color.Transparent;
			this.pictureAtlas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureAtlas.Location = new System.Drawing.Point(0, 0);
			this.pictureAtlas.Name = "pictureAtlas";
			this.pictureAtlas.Size = new System.Drawing.Size(513, 299);
			this.pictureAtlas.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureAtlas.TabIndex = 0;
			this.pictureAtlas.TabStop = false;
			// 
			// stripMenu
			// 
			this.stripMenu.Dock = System.Windows.Forms.DockStyle.None;
			this.stripMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonClear,
            this.buttonLoadImage,
            this.buttonSaveAtlas,
            this.toolStripSeparator1,
            this.buttonAtlasSettings});
			this.stripMenu.Location = new System.Drawing.Point(0, 0);
			this.stripMenu.Name = "stripMenu";
			this.stripMenu.Size = new System.Drawing.Size(513, 25);
			this.stripMenu.Stretch = true;
			this.stripMenu.TabIndex = 0;
			// 
			// buttonClear
			// 
			this.buttonClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonClear.Image = global::Atlas.Properties.Resources.document_dirty;
			this.buttonClear.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(23, 22);
			this.buttonClear.Text = "Clear atlas and images.";
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
			// 
			// buttonLoadImage
			// 
			this.buttonLoadImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonLoadImage.Image = global::Atlas.Properties.Resources.folder_add;
			this.buttonLoadImage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonLoadImage.Name = "buttonLoadImage";
			this.buttonLoadImage.Size = new System.Drawing.Size(23, 22);
			this.buttonLoadImage.Text = "Load images...";
			this.buttonLoadImage.Click += new System.EventHandler(this.buttonLoadImage_Click);
			// 
			// buttonSaveAtlas
			// 
			this.buttonSaveAtlas.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSaveAtlas.Enabled = false;
			this.buttonSaveAtlas.Image = global::Atlas.Properties.Resources.save_as;
			this.buttonSaveAtlas.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSaveAtlas.Name = "buttonSaveAtlas";
			this.buttonSaveAtlas.Size = new System.Drawing.Size(23, 22);
			this.buttonSaveAtlas.Text = "Save atlas image...";
			this.buttonSaveAtlas.Click += new System.EventHandler(this.buttonSaveAtlas_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonAtlasSettings
			// 
			this.buttonAtlasSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonAtlasSettings.Image = global::Atlas.Properties.Resources.preferences;
			this.buttonAtlasSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonAtlasSettings.Name = "buttonAtlasSettings";
			this.buttonAtlasSettings.Size = new System.Drawing.Size(23, 22);
			this.buttonAtlasSettings.Text = "Atlas Settings...";
			this.buttonAtlasSettings.Click += new System.EventHandler(this.buttonAtlasSettings_Click);
			// 
			// dialogOpen
			// 
			this.dialogOpen.InitialDirectory = ".\\";
			this.dialogOpen.Multiselect = true;
			// 
			// dialogSave
			// 
			this.dialogSave.InitialDirectory = ".\\";
			// 
			// formMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(513, 346);
			this.Controls.Add(this.containerMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "formMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Atlas";
			this.containerMain.BottomToolStripPanel.ResumeLayout(false);
			this.containerMain.BottomToolStripPanel.PerformLayout();
			this.containerMain.ContentPanel.ResumeLayout(false);
			this.containerMain.TopToolStripPanel.ResumeLayout(false);
			this.containerMain.TopToolStripPanel.PerformLayout();
			this.containerMain.ResumeLayout(false);
			this.containerMain.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureAtlas)).EndInit();
			this.stripMenu.ResumeLayout(false);
			this.stripMenu.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer containerMain;
		private System.Windows.Forms.PictureBox pictureAtlas;
		private System.Windows.Forms.ToolStrip stripMenu;
		private System.Windows.Forms.ToolStripButton buttonLoadImage;
		private System.Windows.Forms.OpenFileDialog dialogOpen;
		private System.Windows.Forms.ToolStripButton buttonClear;
		private System.Windows.Forms.ToolStripButton buttonSaveAtlas;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton buttonAtlasSettings;
		private System.Windows.Forms.SaveFileDialog dialogSave;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel labelPreviousAtlas;
		private System.Windows.Forms.ToolStripStatusLabel labelCurrentAtlas;
		private System.Windows.Forms.ToolStripStatusLabel labelNextAtlas;
	}
}

