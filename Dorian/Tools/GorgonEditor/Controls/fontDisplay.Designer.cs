namespace GorgonLibrary.GorgonEditor
{
	partial class fontDisplay
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fontDisplay));
			this.panelDisplay = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.stripFont = new System.Windows.Forms.ToolStrip();
			this.itemZoom = new System.Windows.Forms.ToolStripMenuItem();
			this.itemZoom300 = new System.Windows.Forms.ToolStripMenuItem();
			this.itemZoom250 = new System.Windows.Forms.ToolStripMenuItem();
			this.itemZoom200 = new System.Windows.Forms.ToolStripMenuItem();
			this.itemZoom150 = new System.Windows.Forms.ToolStripMenuItem();
			this.itemZoom100 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.itemToWindow = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonPrevious = new System.Windows.Forms.ToolStripButton();
			this.labelTextureCounter = new System.Windows.Forms.ToolStripLabel();
			this.buttonNext = new System.Windows.Forms.ToolStripButton();
			this.panelText = new System.Windows.Forms.Panel();
			this.labelEdit = new System.Windows.Forms.Label();
			this.tipEditor = new System.Windows.Forms.ToolTip(this.components);
			this.panel2.SuspendLayout();
			this.stripFont.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelDisplay
			// 
			this.panelDisplay.AutoScroll = true;
			this.panelDisplay.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelDisplay.Location = new System.Drawing.Point(0, 0);
			this.panelDisplay.Name = "panelDisplay";
			this.panelDisplay.Size = new System.Drawing.Size(810, 450);
			this.panelDisplay.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
			this.panel2.Controls.Add(this.stripFont);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 507);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(810, 26);
			this.panel2.TabIndex = 1;
			// 
			// stripFont
			// 
			this.stripFont.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.stripFont.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripFont.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemZoom,
            this.buttonPrevious,
            this.labelTextureCounter,
            this.buttonNext});
			this.stripFont.Location = new System.Drawing.Point(0, 1);
			this.stripFont.Name = "stripFont";
			this.stripFont.Size = new System.Drawing.Size(810, 25);
			this.stripFont.Stretch = true;
			this.stripFont.TabIndex = 0;
			this.stripFont.Text = "toolStrip1";
			// 
			// itemZoom
			// 
			this.itemZoom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemZoom300,
            this.itemZoom250,
            this.itemZoom200,
            this.itemZoom150,
            this.itemZoom100,
            this.toolStripMenuItem1,
            this.itemToWindow});
			this.itemZoom.Name = "itemZoom";
			this.itemZoom.Size = new System.Drawing.Size(116, 25);
			this.itemZoom.Text = "Zoom: To window";
			// 
			// itemZoom300
			// 
			this.itemZoom300.CheckOnClick = true;
			this.itemZoom300.Name = "itemZoom300";
			this.itemZoom300.Size = new System.Drawing.Size(133, 22);
			this.itemZoom300.Tag = "300";
			this.itemZoom300.Text = "300%";
			this.itemZoom300.Click += new System.EventHandler(this.itemZoom100_Click);
			// 
			// itemZoom250
			// 
			this.itemZoom250.CheckOnClick = true;
			this.itemZoom250.Name = "itemZoom250";
			this.itemZoom250.Size = new System.Drawing.Size(133, 22);
			this.itemZoom250.Tag = "250";
			this.itemZoom250.Text = "250%";
			this.itemZoom250.Click += new System.EventHandler(this.itemZoom100_Click);
			// 
			// itemZoom200
			// 
			this.itemZoom200.CheckOnClick = true;
			this.itemZoom200.Name = "itemZoom200";
			this.itemZoom200.Size = new System.Drawing.Size(133, 22);
			this.itemZoom200.Tag = "200";
			this.itemZoom200.Text = "200%";
			this.itemZoom200.Click += new System.EventHandler(this.itemZoom100_Click);
			// 
			// itemZoom150
			// 
			this.itemZoom150.CheckOnClick = true;
			this.itemZoom150.Name = "itemZoom150";
			this.itemZoom150.Size = new System.Drawing.Size(133, 22);
			this.itemZoom150.Tag = "150";
			this.itemZoom150.Text = "150%";
			this.itemZoom150.Click += new System.EventHandler(this.itemZoom100_Click);
			// 
			// itemZoom100
			// 
			this.itemZoom100.CheckOnClick = true;
			this.itemZoom100.Name = "itemZoom100";
			this.itemZoom100.Size = new System.Drawing.Size(133, 22);
			this.itemZoom100.Tag = "100";
			this.itemZoom100.Text = "100%";
			this.itemZoom100.Click += new System.EventHandler(this.itemZoom100_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(130, 6);
			// 
			// itemToWindow
			// 
			this.itemToWindow.Checked = true;
			this.itemToWindow.CheckOnClick = true;
			this.itemToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
			this.itemToWindow.Name = "itemToWindow";
			this.itemToWindow.Size = new System.Drawing.Size(133, 22);
			this.itemToWindow.Tag = "-1";
			this.itemToWindow.Text = "To window";
			this.itemToWindow.Click += new System.EventHandler(this.itemZoom100_Click);
			// 
			// buttonPrevious
			// 
			this.buttonPrevious.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.buttonPrevious.Enabled = false;
			this.buttonPrevious.Image = ((System.Drawing.Image)(resources.GetObject("buttonPrevious.Image")));
			this.buttonPrevious.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPrevious.Name = "buttonPrevious";
			this.buttonPrevious.Size = new System.Drawing.Size(23, 22);
			this.buttonPrevious.Text = "←";
			this.buttonPrevious.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonPrevious.ToolTipText = "Move to the previous texture.";
			this.buttonPrevious.Click += new System.EventHandler(this.buttonPrevious_Click);
			// 
			// labelTextureCounter
			// 
			this.labelTextureCounter.Name = "labelTextureCounter";
			this.labelTextureCounter.Size = new System.Drawing.Size(69, 22);
			this.labelTextureCounter.Text = "Texture: 0/0";
			// 
			// buttonNext
			// 
			this.buttonNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.buttonNext.Enabled = false;
			this.buttonNext.Image = ((System.Drawing.Image)(resources.GetObject("buttonNext.Image")));
			this.buttonNext.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNext.Name = "buttonNext";
			this.buttonNext.Size = new System.Drawing.Size(23, 22);
			this.buttonNext.Text = "→";
			this.buttonNext.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonNext.ToolTipText = "Move to the next texture.";
			this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
			// 
			// panelText
			// 
			this.panelText.BackColor = System.Drawing.Color.White;
			this.panelText.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.panelText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelText.Location = new System.Drawing.Point(0, 465);
			this.panelText.Name = "panelText";
			this.panelText.Size = new System.Drawing.Size(810, 42);
			this.panelText.TabIndex = 2;
			this.tipEditor.SetToolTip(this.panelText, "Double click here to edit this text.");
			this.panelText.Click += new System.EventHandler(this.panelText_Click);
			this.panelText.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelText_MouseDoubleClick);
			// 
			// labelEdit
			// 
			this.labelEdit.BackColor = System.Drawing.SystemColors.Info;
			this.labelEdit.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelEdit.Location = new System.Drawing.Point(0, 450);
			this.labelEdit.Name = "labelEdit";
			this.labelEdit.Size = new System.Drawing.Size(810, 15);
			this.labelEdit.TabIndex = 3;
			this.labelEdit.Text = "Press ESC to exit edit mode.";
			this.labelEdit.Visible = false;
			// 
			// fontDisplay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.DimGray;
			this.Controls.Add(this.panelText);
			this.Controls.Add(this.labelEdit);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panelDisplay);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "fontDisplay";
			this.Size = new System.Drawing.Size(810, 533);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.stripFont.ResumeLayout(false);
			this.stripFont.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel2;
		public System.Windows.Forms.Panel panelDisplay;
		private System.Windows.Forms.ToolStrip stripFont;
		private System.Windows.Forms.ToolStripMenuItem itemZoom;
		private System.Windows.Forms.ToolStripMenuItem itemZoom300;
		private System.Windows.Forms.ToolStripMenuItem itemZoom250;
		private System.Windows.Forms.ToolStripMenuItem itemZoom200;
		private System.Windows.Forms.ToolStripMenuItem itemZoom150;
		private System.Windows.Forms.ToolStripMenuItem itemZoom100;
		private System.Windows.Forms.ToolStripButton buttonPrevious;
		private System.Windows.Forms.ToolStripLabel labelTextureCounter;
		private System.Windows.Forms.ToolStripButton buttonNext;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem itemToWindow;
		public System.Windows.Forms.Panel panelText;
		private System.Windows.Forms.Label labelEdit;
		private System.Windows.Forms.ToolTip tipEditor;
	}
}
