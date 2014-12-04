namespace GorgonLibrary.Editor.SpriteEditorPlugIn.Controls
{
	partial class PanelSpriteEditor
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
			this.panelSprite = new System.Windows.Forms.Panel();
			this.containerSprite = new System.Windows.Forms.ToolStripContainer();
			this.stripUIOptions = new System.Windows.Forms.ToolStrip();
			this.dropDownZoom = new System.Windows.Forms.ToolStripDropDownButton();
			this.menuItem1600 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem800 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem400 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem200 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem100 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem75 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem50 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem25 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemToWindow = new System.Windows.Forms.ToolStripMenuItem();
			this.panelOuter = new System.Windows.Forms.Panel();
			this.panelHScroll = new System.Windows.Forms.Panel();
			this.scrollHorizontal = new System.Windows.Forms.HScrollBar();
			this.panelVScroll = new System.Windows.Forms.Panel();
			this.scrollVertical = new System.Windows.Forms.VScrollBar();
			this.stripSprite = new System.Windows.Forms.ToolStrip();
			this.buttonSave = new System.Windows.Forms.ToolStripButton();
			this.buttonRevert = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.PanelDisplay.SuspendLayout();
			this.containerSprite.BottomToolStripPanel.SuspendLayout();
			this.containerSprite.ContentPanel.SuspendLayout();
			this.containerSprite.TopToolStripPanel.SuspendLayout();
			this.containerSprite.SuspendLayout();
			this.stripUIOptions.SuspendLayout();
			this.panelOuter.SuspendLayout();
			this.panelHScroll.SuspendLayout();
			this.panelVScroll.SuspendLayout();
			this.stripSprite.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelDisplay
			// 
			this.PanelDisplay.Controls.Add(this.containerSprite);
			this.PanelDisplay.Size = new System.Drawing.Size(806, 606);
			// 
			// panelSprite
			// 
			this.panelSprite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panelSprite.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSprite.Location = new System.Drawing.Point(0, 0);
			this.panelSprite.Name = "panelSprite";
			this.panelSprite.Size = new System.Drawing.Size(789, 539);
			this.panelSprite.TabIndex = 0;
			// 
			// containerSprite
			// 
			// 
			// containerSprite.BottomToolStripPanel
			// 
			this.containerSprite.BottomToolStripPanel.Controls.Add(this.stripUIOptions);
			// 
			// containerSprite.ContentPanel
			// 
			this.containerSprite.ContentPanel.Controls.Add(this.panelOuter);
			this.containerSprite.ContentPanel.Size = new System.Drawing.Size(806, 556);
			this.containerSprite.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerSprite.Location = new System.Drawing.Point(0, 0);
			this.containerSprite.Name = "containerSprite";
			this.containerSprite.Size = new System.Drawing.Size(806, 606);
			this.containerSprite.TabIndex = 1;
			this.containerSprite.Text = "toolStripContainer1";
			// 
			// containerSprite.TopToolStripPanel
			// 
			this.containerSprite.TopToolStripPanel.Controls.Add(this.stripSprite);
			// 
			// stripUIOptions
			// 
			this.stripUIOptions.Dock = System.Windows.Forms.DockStyle.None;
			this.stripUIOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripUIOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dropDownZoom});
			this.stripUIOptions.Location = new System.Drawing.Point(0, 0);
			this.stripUIOptions.Name = "stripUIOptions";
			this.stripUIOptions.Size = new System.Drawing.Size(806, 25);
			this.stripUIOptions.Stretch = true;
			this.stripUIOptions.TabIndex = 0;
			// 
			// dropDownZoom
			// 
			this.dropDownZoom.AutoToolTip = false;
			this.dropDownZoom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem1600,
            this.menuItem800,
            this.menuItem400,
            this.menuItem200,
            this.menuItem100,
            this.menuItem75,
            this.menuItem50,
            this.menuItem25,
            this.toolStripSeparator2,
            this.menuItemToWindow});
			this.dropDownZoom.Image = global::GorgonLibrary.Editor.SpriteEditorPlugIn.Properties.Resources.zoom_16x16;
			this.dropDownZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.dropDownZoom.Name = "dropDownZoom";
			this.dropDownZoom.Size = new System.Drawing.Size(128, 22);
			this.dropDownZoom.Text = "zoom: to window";
			// 
			// menuItem1600
			// 
			this.menuItem1600.CheckOnClick = true;
			this.menuItem1600.Name = "menuItem1600";
			this.menuItem1600.Size = new System.Drawing.Size(152, 22);
			this.menuItem1600.Tag = "16";
			this.menuItem1600.Text = "1600%";
			this.menuItem1600.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem1600.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem800
			// 
			this.menuItem800.CheckOnClick = true;
			this.menuItem800.Name = "menuItem800";
			this.menuItem800.Size = new System.Drawing.Size(152, 22);
			this.menuItem800.Tag = "8";
			this.menuItem800.Text = "800%";
			this.menuItem800.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem800.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem400
			// 
			this.menuItem400.CheckOnClick = true;
			this.menuItem400.Name = "menuItem400";
			this.menuItem400.Size = new System.Drawing.Size(152, 22);
			this.menuItem400.Tag = "4";
			this.menuItem400.Text = "400%";
			this.menuItem400.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem400.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem200
			// 
			this.menuItem200.CheckOnClick = true;
			this.menuItem200.Name = "menuItem200";
			this.menuItem200.Size = new System.Drawing.Size(152, 22);
			this.menuItem200.Tag = "2";
			this.menuItem200.Text = "200%";
			this.menuItem200.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem200.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem100
			// 
			this.menuItem100.Checked = true;
			this.menuItem100.CheckOnClick = true;
			this.menuItem100.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem100.Name = "menuItem100";
			this.menuItem100.Size = new System.Drawing.Size(152, 22);
			this.menuItem100.Tag = "1";
			this.menuItem100.Text = "100%";
			this.menuItem100.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem100.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem75
			// 
			this.menuItem75.CheckOnClick = true;
			this.menuItem75.Name = "menuItem75";
			this.menuItem75.Size = new System.Drawing.Size(152, 22);
			this.menuItem75.Tag = "0.75";
			this.menuItem75.Text = "75%";
			this.menuItem75.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem50
			// 
			this.menuItem50.CheckOnClick = true;
			this.menuItem50.Name = "menuItem50";
			this.menuItem50.Size = new System.Drawing.Size(152, 22);
			this.menuItem50.Tag = "0.5";
			this.menuItem50.Text = "50%";
			this.menuItem50.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem25
			// 
			this.menuItem25.CheckOnClick = true;
			this.menuItem25.Name = "menuItem25";
			this.menuItem25.Size = new System.Drawing.Size(152, 22);
			this.menuItem25.Tag = "0.25";
			this.menuItem25.Text = "25%";
			this.menuItem25.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
			// 
			// menuItemToWindow
			// 
			this.menuItemToWindow.CheckOnClick = true;
			this.menuItemToWindow.Name = "menuItemToWindow";
			this.menuItemToWindow.Size = new System.Drawing.Size(152, 22);
			this.menuItemToWindow.Tag = "-1";
			this.menuItemToWindow.Text = "to window";
			this.menuItemToWindow.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItemToWindow.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// panelOuter
			// 
			this.panelOuter.Controls.Add(this.panelSprite);
			this.panelOuter.Controls.Add(this.panelHScroll);
			this.panelOuter.Controls.Add(this.panelVScroll);
			this.panelOuter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOuter.Location = new System.Drawing.Point(0, 0);
			this.panelOuter.Name = "panelOuter";
			this.panelOuter.Size = new System.Drawing.Size(806, 556);
			this.panelOuter.TabIndex = 1;
			// 
			// panelHScroll
			// 
			this.panelHScroll.AutoSize = true;
			this.panelHScroll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelHScroll.Controls.Add(this.scrollHorizontal);
			this.panelHScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelHScroll.Location = new System.Drawing.Point(0, 539);
			this.panelHScroll.Name = "panelHScroll";
			this.panelHScroll.Size = new System.Drawing.Size(789, 17);
			this.panelHScroll.TabIndex = 1;
			this.panelHScroll.Visible = false;
			// 
			// scrollHorizontal
			// 
			this.scrollHorizontal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scrollHorizontal.Location = new System.Drawing.Point(0, 0);
			this.scrollHorizontal.Name = "scrollHorizontal";
			this.scrollHorizontal.Size = new System.Drawing.Size(789, 17);
			this.scrollHorizontal.TabIndex = 0;
			// 
			// panelVScroll
			// 
			this.panelVScroll.AutoSize = true;
			this.panelVScroll.BackColor = System.Drawing.SystemColors.Control;
			this.panelVScroll.Controls.Add(this.scrollVertical);
			this.panelVScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelVScroll.Location = new System.Drawing.Point(789, 0);
			this.panelVScroll.Name = "panelVScroll";
			this.panelVScroll.Size = new System.Drawing.Size(17, 556);
			this.panelVScroll.TabIndex = 0;
			this.panelVScroll.Visible = false;
			// 
			// scrollVertical
			// 
			this.scrollVertical.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scrollVertical.Location = new System.Drawing.Point(0, 0);
			this.scrollVertical.Name = "scrollVertical";
			this.scrollVertical.Size = new System.Drawing.Size(17, 539);
			this.scrollVertical.TabIndex = 0;
			// 
			// stripSprite
			// 
			this.stripSprite.Dock = System.Windows.Forms.DockStyle.None;
			this.stripSprite.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripSprite.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSave,
            this.buttonRevert,
            this.toolStripSeparator1});
			this.stripSprite.Location = new System.Drawing.Point(0, 0);
			this.stripSprite.Name = "stripSprite";
			this.stripSprite.Size = new System.Drawing.Size(806, 25);
			this.stripSprite.Stretch = true;
			this.stripSprite.TabIndex = 0;
			// 
			// buttonSave
			// 
			this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSave.Image = global::GorgonLibrary.Editor.SpriteEditorPlugIn.Properties.Resources.save_16x16;
			this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(23, 22);
			this.buttonSave.Text = "not localized save";
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// buttonRevert
			// 
			this.buttonRevert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRevert.Image = global::GorgonLibrary.Editor.SpriteEditorPlugIn.Properties.Resources.revert_16x16;
			this.buttonRevert.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRevert.Name = "buttonRevert";
			this.buttonRevert.Size = new System.Drawing.Size(23, 22);
			this.buttonRevert.Text = "not localized revert";
			this.buttonRevert.Click += new System.EventHandler(this.buttonRevert_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// PanelSpriteEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.Name = "PanelSpriteEditor";
			this.Size = new System.Drawing.Size(806, 636);
			this.Text = "not localized sprite";
			this.PanelDisplay.ResumeLayout(false);
			this.containerSprite.BottomToolStripPanel.ResumeLayout(false);
			this.containerSprite.BottomToolStripPanel.PerformLayout();
			this.containerSprite.ContentPanel.ResumeLayout(false);
			this.containerSprite.TopToolStripPanel.ResumeLayout(false);
			this.containerSprite.TopToolStripPanel.PerformLayout();
			this.containerSprite.ResumeLayout(false);
			this.containerSprite.PerformLayout();
			this.stripUIOptions.ResumeLayout(false);
			this.stripUIOptions.PerformLayout();
			this.panelOuter.ResumeLayout(false);
			this.panelOuter.PerformLayout();
			this.panelHScroll.ResumeLayout(false);
			this.panelVScroll.ResumeLayout(false);
			this.stripSprite.ResumeLayout(false);
			this.stripSprite.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.Panel panelSprite;
		private System.Windows.Forms.ToolStripContainer containerSprite;
		private System.Windows.Forms.ToolStrip stripSprite;
		private System.Windows.Forms.ToolStripButton buttonSave;
		private System.Windows.Forms.ToolStripButton buttonRevert;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.Panel panelOuter;
		private System.Windows.Forms.Panel panelHScroll;
		private System.Windows.Forms.Panel panelVScroll;
		private System.Windows.Forms.HScrollBar scrollHorizontal;
		private System.Windows.Forms.VScrollBar scrollVertical;
		private System.Windows.Forms.ToolStrip stripUIOptions;
		private System.Windows.Forms.ToolStripDropDownButton dropDownZoom;
		private System.Windows.Forms.ToolStripMenuItem menuItem1600;
		private System.Windows.Forms.ToolStripMenuItem menuItem800;
		private System.Windows.Forms.ToolStripMenuItem menuItem400;
		private System.Windows.Forms.ToolStripMenuItem menuItem200;
		private System.Windows.Forms.ToolStripMenuItem menuItem100;
		private System.Windows.Forms.ToolStripMenuItem menuItem75;
		private System.Windows.Forms.ToolStripMenuItem menuItem50;
		private System.Windows.Forms.ToolStripMenuItem menuItem25;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem menuItemToWindow;

	}
}
