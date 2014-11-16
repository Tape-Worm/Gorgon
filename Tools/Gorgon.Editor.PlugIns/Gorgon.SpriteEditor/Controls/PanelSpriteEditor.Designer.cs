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
			this.panelOuter = new System.Windows.Forms.Panel();
			this.panelHScroll = new System.Windows.Forms.Panel();
			this.scrollHorizontal = new System.Windows.Forms.HScrollBar();
			this.panelVScroll = new System.Windows.Forms.Panel();
			this.scrollVertical = new System.Windows.Forms.VScrollBar();
			this.stripSprite = new System.Windows.Forms.ToolStrip();
			this.buttonSave = new System.Windows.Forms.ToolStripButton();
			this.buttonRevert = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.stripUIOptions = new System.Windows.Forms.ToolStrip();
			this.labelZoom = new System.Windows.Forms.ToolStripLabel();
			this.PanelDisplay.SuspendLayout();
			this.containerSprite.BottomToolStripPanel.SuspendLayout();
			this.containerSprite.ContentPanel.SuspendLayout();
			this.containerSprite.TopToolStripPanel.SuspendLayout();
			this.containerSprite.SuspendLayout();
			this.panelOuter.SuspendLayout();
			this.panelHScroll.SuspendLayout();
			this.panelVScroll.SuspendLayout();
			this.stripSprite.SuspendLayout();
			this.stripUIOptions.SuspendLayout();
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
			// stripUIOptions
			// 
			this.stripUIOptions.Dock = System.Windows.Forms.DockStyle.None;
			this.stripUIOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripUIOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelZoom});
			this.stripUIOptions.Location = new System.Drawing.Point(0, 0);
			this.stripUIOptions.Name = "stripUIOptions";
			this.stripUIOptions.Size = new System.Drawing.Size(806, 25);
			this.stripUIOptions.Stretch = true;
			this.stripUIOptions.TabIndex = 0;
			// 
			// labelZoom
			// 
			this.labelZoom.Name = "labelZoom";
			this.labelZoom.Size = new System.Drawing.Size(107, 22);
			this.labelZoom.Text = "not localized zoom";
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
			this.panelOuter.ResumeLayout(false);
			this.panelOuter.PerformLayout();
			this.panelHScroll.ResumeLayout(false);
			this.panelVScroll.ResumeLayout(false);
			this.stripSprite.ResumeLayout(false);
			this.stripSprite.PerformLayout();
			this.stripUIOptions.ResumeLayout(false);
			this.stripUIOptions.PerformLayout();
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
		private System.Windows.Forms.ToolStripLabel labelZoom;

	}
}
