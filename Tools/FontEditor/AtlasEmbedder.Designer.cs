namespace GorgonLibrary.Graphics.Tools
{
	partial class AtlasEmbedder
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AtlasEmbedder));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.stripLocator = new System.Windows.Forms.StatusStrip();
			this.labelPosition = new System.Windows.Forms.ToolStripStatusLabel();
			this.buttonReset = new System.Windows.Forms.Button();
			this.panelZoom = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.panelImage = new System.Windows.Forms.Panel();
			this.scrollHorizontal = new System.Windows.Forms.HScrollBar();
			this.scrollVertical = new System.Windows.Forms.VScrollBar();
			this.labelScrollSpace = new System.Windows.Forms.Label();
			this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.stripLocator.SuspendLayout();
			this.panelImage.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.BottomToolStripPanel
			// 
			this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.stripLocator);
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.buttonReset);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.panelZoom);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.label1);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.buttonCancel);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.buttonOK);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.panelImage);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(544, 383);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.LeftToolStripPanelVisible = false;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.RightToolStripPanelVisible = false;
			this.toolStripContainer1.Size = new System.Drawing.Size(544, 405);
			this.toolStripContainer1.TabIndex = 0;
			this.toolStripContainer1.Text = "toolStripContainer1";
			this.toolStripContainer1.TopToolStripPanelVisible = false;
			// 
			// stripLocator
			// 
			this.stripLocator.Dock = System.Windows.Forms.DockStyle.None;
			this.stripLocator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelPosition});
			this.stripLocator.Location = new System.Drawing.Point(0, 0);
			this.stripLocator.Name = "stripLocator";
			this.stripLocator.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.stripLocator.Size = new System.Drawing.Size(544, 22);
			this.stripLocator.TabIndex = 0;
			// 
			// labelPosition
			// 
			this.labelPosition.Name = "labelPosition";
			this.labelPosition.Size = new System.Drawing.Size(68, 17);
			this.labelPosition.Text = "Position: 0x0";
			// 
			// buttonReset
			// 
			this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonReset.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.ImageRemove;
			this.buttonReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonReset.Location = new System.Drawing.Point(388, 179);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(147, 23);
			this.buttonReset.TabIndex = 13;
			this.buttonReset.Text = "Reset Position";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// panelZoom
			// 
			this.panelZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelZoom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelZoom.Location = new System.Drawing.Point(388, 23);
			this.panelZoom.Name = "panelZoom";
			this.panelZoom.Size = new System.Drawing.Size(147, 147);
			this.panelZoom.TabIndex = 12;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(385, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(105, 13);
			this.label1.TabIndex = 11;
			this.label1.Text = "Precision placement:";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(514, 356);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 24);
			this.buttonCancel.TabIndex = 9;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(484, 356);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 24);
			this.buttonOK.TabIndex = 8;
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// panelImage
			// 
			this.panelImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelImage.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelImage.Controls.Add(this.scrollHorizontal);
			this.panelImage.Controls.Add(this.scrollVertical);
			this.panelImage.Controls.Add(this.labelScrollSpace);
			this.panelImage.Location = new System.Drawing.Point(0, 0);
			this.panelImage.Name = "panelImage";
			this.panelImage.Size = new System.Drawing.Size(382, 382);
			this.panelImage.TabIndex = 0;
			this.panelImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelImage_MouseDown);
			this.panelImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelImage_MouseMove);
			this.panelImage.Paint += new System.Windows.Forms.PaintEventHandler(this.panelImage_Paint);
			// 
			// scrollHorizontal
			// 
			this.scrollHorizontal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.scrollHorizontal.LargeChange = 50;
			this.scrollHorizontal.Location = new System.Drawing.Point(-1, 364);
			this.scrollHorizontal.Maximum = 32000;
			this.scrollHorizontal.Name = "scrollHorizontal";
			this.scrollHorizontal.Size = new System.Drawing.Size(366, 16);
			this.scrollHorizontal.SmallChange = 10;
			this.scrollHorizontal.TabIndex = 13;
			this.scrollHorizontal.Visible = false;
			this.scrollHorizontal.ValueChanged += new System.EventHandler(this.scrollHorizontal_ValueChanged);
			// 
			// scrollVertical
			// 
			this.scrollVertical.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.scrollVertical.LargeChange = 50;
			this.scrollVertical.Location = new System.Drawing.Point(365, 0);
			this.scrollVertical.Maximum = 32000;
			this.scrollVertical.Name = "scrollVertical";
			this.scrollVertical.Size = new System.Drawing.Size(16, 365);
			this.scrollVertical.SmallChange = 10;
			this.scrollVertical.TabIndex = 12;
			this.scrollVertical.Visible = false;
			this.scrollVertical.ValueChanged += new System.EventHandler(this.scrollVertical_ValueChanged);
			// 
			// labelScrollSpace
			// 
			this.labelScrollSpace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelScrollSpace.BackColor = System.Drawing.SystemColors.Control;
			this.labelScrollSpace.Location = new System.Drawing.Point(364, 365);
			this.labelScrollSpace.Name = "labelScrollSpace";
			this.labelScrollSpace.Size = new System.Drawing.Size(17, 16);
			this.labelScrollSpace.TabIndex = 14;
			this.labelScrollSpace.Text = " ";
			this.labelScrollSpace.Visible = false;
			// 
			// AtlasEmbedder
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(544, 405);
			this.Controls.Add(this.toolStripContainer1);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.Name = "AtlasEmbedder";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Place font.";
			this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.ContentPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.stripLocator.ResumeLayout(false);
			this.stripLocator.PerformLayout();
			this.panelImage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.StatusStrip stripLocator;
		private System.Windows.Forms.ToolStripStatusLabel labelPosition;
		private System.Windows.Forms.Panel panelImage;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.HScrollBar scrollHorizontal;
		private System.Windows.Forms.VScrollBar scrollVertical;
		private System.Windows.Forms.Label labelScrollSpace;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panelZoom;
		private System.Windows.Forms.Button buttonReset;
	}
}