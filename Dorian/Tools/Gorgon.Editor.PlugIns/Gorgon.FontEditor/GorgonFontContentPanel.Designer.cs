namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    partial class GorgonFontContentPanel
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

            if (!_disposed)
            {
                if (_pattern != null)
                {
                    _pattern.Dispose();
                }

                _pattern = null;
                _disposed = true;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GorgonFontContentPanel));
            this.panelTextures = new System.Windows.Forms.Panel();
            this.panelText = new System.Windows.Forms.Panel();
            this.splitContent = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelToolbar = new System.Windows.Forms.Panel();
            this.stripCommands = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.panelControls = new System.Windows.Forms.Panel();
            this.stripFontDisplay = new System.Windows.Forms.ToolStrip();
            this.buttonPrevTexture = new System.Windows.Forms.ToolStripButton();
            this.labelTextureCount = new System.Windows.Forms.ToolStripLabel();
            this.buttonNextTexture = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.dropDownZoom = new System.Windows.Forms.ToolStripDropDownButton();
            this.menuItem1600 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem800 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem400 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem200 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem100 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem75 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem50 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem25 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemToWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.labelSelectedGlyphInfo = new System.Windows.Forms.ToolStripLabel();
            this.labelHoverGlyphInfo = new System.Windows.Forms.ToolStripLabel();
            this.separatorGlyphInfo = new System.Windows.Forms.ToolStripSeparator();
            this.PanelDisplay.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelToolbar.SuspendLayout();
            this.stripCommands.SuspendLayout();
            this.panelControls.SuspendLayout();
            this.stripFontDisplay.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelDisplay
            // 
            this.PanelDisplay.Controls.Add(this.panel1);
            this.PanelDisplay.Controls.Add(this.splitContent);
            this.PanelDisplay.Controls.Add(this.panelText);
            this.PanelDisplay.Size = new System.Drawing.Size(806, 606);
            // 
            // panelTextures
            // 
            this.panelTextures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.panelTextures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTextures.Location = new System.Drawing.Point(0, 25);
            this.panelTextures.Name = "panelTextures";
            this.panelTextures.Size = new System.Drawing.Size(806, 401);
            this.panelTextures.TabIndex = 0;
            this.panelTextures.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseDown);
            this.panelTextures.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTextures_MouseMove);
            // 
            // panelText
            // 
            this.panelText.BackColor = System.Drawing.Color.White;
            this.panelText.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelText.ForeColor = System.Drawing.Color.Black;
            this.panelText.Location = new System.Drawing.Point(0, 456);
            this.panelText.Name = "panelText";
            this.panelText.Size = new System.Drawing.Size(806, 150);
            this.panelText.TabIndex = 0;
            this.panelText.Resize += new System.EventHandler(this.panelText_Resize);
            // 
            // splitContent
            // 
            this.splitContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.splitContent.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitContent.Location = new System.Drawing.Point(0, 452);
            this.splitContent.MinExtra = 320;
            this.splitContent.MinSize = 150;
            this.splitContent.Name = "splitContent";
            this.splitContent.Size = new System.Drawing.Size(806, 4);
            this.splitContent.TabIndex = 2;
            this.splitContent.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.panel1.Controls.Add(this.panelTextures);
            this.panel1.Controls.Add(this.panelToolbar);
            this.panel1.Controls.Add(this.panelControls);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(806, 452);
            this.panel1.TabIndex = 3;
            // 
            // panelToolbar
            // 
            this.panelToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.panelToolbar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelToolbar.Controls.Add(this.stripCommands);
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolbar.Enabled = false;
            this.panelToolbar.Location = new System.Drawing.Point(0, 0);
            this.panelToolbar.Name = "panelToolbar";
            this.panelToolbar.Size = new System.Drawing.Size(806, 25);
            this.panelToolbar.TabIndex = 3;
            // 
            // stripCommands
            // 
            this.stripCommands.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1});
            this.stripCommands.Location = new System.Drawing.Point(0, 0);
            this.stripCommands.Name = "stripCommands";
            this.stripCommands.Size = new System.Drawing.Size(804, 25);
            this.stripCommands.Stretch = true;
            this.stripCommands.TabIndex = 0;
            this.stripCommands.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(40, 22);
            this.toolStripLabel1.Text = "TODO";
            // 
            // panelControls
            // 
            this.panelControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.panelControls.Controls.Add(this.stripFontDisplay);
            this.panelControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelControls.Location = new System.Drawing.Point(0, 426);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(806, 26);
            this.panelControls.TabIndex = 0;
            // 
            // stripFontDisplay
            // 
            this.stripFontDisplay.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripFontDisplay.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonPrevTexture,
            this.labelTextureCount,
            this.buttonNextTexture,
            this.toolStripSeparator2,
            this.dropDownZoom,
            this.toolStripSeparator3,
            this.labelSelectedGlyphInfo,
            this.separatorGlyphInfo,
            this.labelHoverGlyphInfo});
            this.stripFontDisplay.Location = new System.Drawing.Point(0, 0);
            this.stripFontDisplay.Name = "stripFontDisplay";
            this.stripFontDisplay.Size = new System.Drawing.Size(806, 25);
            this.stripFontDisplay.Stretch = true;
            this.stripFontDisplay.TabIndex = 4;
            this.stripFontDisplay.Text = "toolStrip1";
            // 
            // buttonPrevTexture
            // 
            this.buttonPrevTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.buttonPrevTexture.Font = new System.Drawing.Font("Marlett", 9F);
            this.buttonPrevTexture.Image = ((System.Drawing.Image)(resources.GetObject("buttonPrevTexture.Image")));
            this.buttonPrevTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPrevTexture.Name = "buttonPrevTexture";
            this.buttonPrevTexture.Size = new System.Drawing.Size(23, 22);
            this.buttonPrevTexture.Text = "3";
            this.buttonPrevTexture.Click += new System.EventHandler(this.buttonPrevTexture_Click);
            // 
            // labelTextureCount
            // 
            this.labelTextureCount.Name = "labelTextureCount";
            this.labelTextureCount.Size = new System.Drawing.Size(71, 22);
            this.labelTextureCount.Text = "Texture N/A";
            // 
            // buttonNextTexture
            // 
            this.buttonNextTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.buttonNextTexture.Font = new System.Drawing.Font("Marlett", 9F);
            this.buttonNextTexture.Image = ((System.Drawing.Image)(resources.GetObject("buttonNextTexture.Image")));
            this.buttonNextTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonNextTexture.Name = "buttonNextTexture";
            this.buttonNextTexture.Size = new System.Drawing.Size(23, 22);
            this.buttonNextTexture.Text = "4";
            this.buttonNextTexture.Click += new System.EventHandler(this.buttonNextTexture_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // dropDownZoom
            // 
            this.dropDownZoom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem1600,
            this.menuItem800,
            this.menuItem400,
            this.menuItem200,
            this.menuItem100,
            this.menuItem75,
            this.menuItem50,
            this.menuItem25,
            this.toolStripSeparator1,
            this.menuItemToWindow});
            this.dropDownZoom.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.zoom_16x16;
            this.dropDownZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.dropDownZoom.Name = "dropDownZoom";
            this.dropDownZoom.Size = new System.Drawing.Size(135, 22);
            this.dropDownZoom.Text = "Zoom: To Window";
            // 
            // menuItem1600
            // 
            this.menuItem1600.CheckOnClick = true;
            this.menuItem1600.Name = "menuItem1600";
            this.menuItem1600.Size = new System.Drawing.Size(135, 22);
            this.menuItem1600.Tag = "16";
            this.menuItem1600.Text = "1600%";
            this.menuItem1600.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.menuItem1600.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItem800
            // 
            this.menuItem800.CheckOnClick = true;
            this.menuItem800.Name = "menuItem800";
            this.menuItem800.Size = new System.Drawing.Size(135, 22);
            this.menuItem800.Tag = "8";
            this.menuItem800.Text = "800%";
            this.menuItem800.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.menuItem800.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItem400
            // 
            this.menuItem400.CheckOnClick = true;
            this.menuItem400.Name = "menuItem400";
            this.menuItem400.Size = new System.Drawing.Size(135, 22);
            this.menuItem400.Tag = "4";
            this.menuItem400.Text = "400%";
            this.menuItem400.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.menuItem400.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItem200
            // 
            this.menuItem200.CheckOnClick = true;
            this.menuItem200.Name = "menuItem200";
            this.menuItem200.Size = new System.Drawing.Size(135, 22);
            this.menuItem200.Tag = "2";
            this.menuItem200.Text = "200%";
            this.menuItem200.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.menuItem200.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItem100
            // 
            this.menuItem100.CheckOnClick = true;
            this.menuItem100.Name = "menuItem100";
            this.menuItem100.Size = new System.Drawing.Size(135, 22);
            this.menuItem100.Tag = "1";
            this.menuItem100.Text = "100%";
            this.menuItem100.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.menuItem100.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItem75
            // 
            this.menuItem75.CheckOnClick = true;
            this.menuItem75.Name = "menuItem75";
            this.menuItem75.Size = new System.Drawing.Size(135, 22);
            this.menuItem75.Tag = "0.75";
            this.menuItem75.Text = "75%";
            this.menuItem75.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItem50
            // 
            this.menuItem50.CheckOnClick = true;
            this.menuItem50.Name = "menuItem50";
            this.menuItem50.Size = new System.Drawing.Size(135, 22);
            this.menuItem50.Tag = "0.5";
            this.menuItem50.Text = "50%";
            this.menuItem50.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItem25
            // 
            this.menuItem25.CheckOnClick = true;
            this.menuItem25.Name = "menuItem25";
            this.menuItem25.Size = new System.Drawing.Size(135, 22);
            this.menuItem25.Tag = "0.25";
            this.menuItem25.Text = "25%";
            this.menuItem25.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(132, 6);
            this.toolStripSeparator1.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // menuItemToWindow
            // 
            this.menuItemToWindow.Checked = true;
            this.menuItemToWindow.CheckOnClick = true;
            this.menuItemToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuItemToWindow.Name = "menuItemToWindow";
            this.menuItemToWindow.Size = new System.Drawing.Size(135, 22);
            this.menuItemToWindow.Tag = "-1";
            this.menuItemToWindow.Text = "To Window";
            this.menuItemToWindow.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.menuItemToWindow.Click += new System.EventHandler(this.zoomItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // labelSelectedGlyphInfo
            // 
            this.labelSelectedGlyphInfo.Name = "labelSelectedGlyphInfo";
            this.labelSelectedGlyphInfo.Size = new System.Drawing.Size(10, 22);
            this.labelSelectedGlyphInfo.Text = " ";
            // 
            // labelHoverGlyphInfo
            // 
            this.labelHoverGlyphInfo.Name = "labelHoverGlyphInfo";
            this.labelHoverGlyphInfo.Size = new System.Drawing.Size(13, 22);
            this.labelHoverGlyphInfo.Text = "  ";
            // 
            // separatorGlyphInfo
            // 
            this.separatorGlyphInfo.Name = "separatorGlyphInfo";
            this.separatorGlyphInfo.Size = new System.Drawing.Size(6, 25);
            this.separatorGlyphInfo.Visible = false;
            // 
            // GorgonFontContentPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "GorgonFontContentPanel";
            this.Size = new System.Drawing.Size(806, 636);
            this.Text = "Gorgon Font - Untitled";
            this.Resize += new System.EventHandler(this.GorgonFontContentPanel_Resize);
            this.PanelDisplay.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panelToolbar.ResumeLayout(false);
            this.panelToolbar.PerformLayout();
            this.stripCommands.ResumeLayout(false);
            this.stripCommands.PerformLayout();
            this.panelControls.ResumeLayout(false);
            this.panelControls.PerformLayout();
            this.stripFontDisplay.ResumeLayout(false);
            this.stripFontDisplay.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Splitter splitContent;
		private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelControls;
		internal System.Windows.Forms.Panel panelTextures;
        internal System.Windows.Forms.Panel panelText;
        private System.Windows.Forms.Panel panelToolbar;
        private System.Windows.Forms.ToolStrip stripFontDisplay;
        private System.Windows.Forms.ToolStripDropDownButton dropDownZoom;
        private System.Windows.Forms.ToolStripMenuItem menuItem1600;
        private System.Windows.Forms.ToolStripMenuItem menuItem800;
        private System.Windows.Forms.ToolStripMenuItem menuItem400;
        private System.Windows.Forms.ToolStripMenuItem menuItem200;
        private System.Windows.Forms.ToolStripMenuItem menuItem100;
        private System.Windows.Forms.ToolStripMenuItem menuItem75;
        private System.Windows.Forms.ToolStripMenuItem menuItem50;
        private System.Windows.Forms.ToolStripMenuItem menuItem25;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItemToWindow;
        private System.Windows.Forms.ToolStripButton buttonPrevTexture;
        private System.Windows.Forms.ToolStripLabel labelTextureCount;
        private System.Windows.Forms.ToolStripButton buttonNextTexture;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel labelSelectedGlyphInfo;
        private System.Windows.Forms.ToolStrip stripCommands;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel labelHoverGlyphInfo;
        private System.Windows.Forms.ToolStripSeparator separatorGlyphInfo;

    }
}
