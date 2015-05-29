using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.UI;

namespace Gorgon.Editor.SpriteEditorPlugIn.Controls
{
	partial class PanelSpriteEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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

			if ((!_disposed) && (disposing))
			{
				panelSprite.MouseWheel -= PanelSprite_MouseWheel;

				if (_zoomFont != null)
				{
					_zoomFont.Dispose();
					_zoomFont = null;
				}

				if (_anchorImage != null)
				{
					_anchorImage.Dispose();
					_anchorImage = null;
				}
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelSprite = new Gorgon.UI.GorgonSelectablePanel();
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
			this.labelMouseInfo = new System.Windows.Forms.ToolStripLabel();
			this.panelOuter = new System.Windows.Forms.Panel();
			this.panelHScroll = new System.Windows.Forms.Panel();
			this.scrollHorizontal = new System.Windows.Forms.HScrollBar();
			this.panelVScroll = new System.Windows.Forms.Panel();
			this.buttonCenter = new System.Windows.Forms.Button();
			this.scrollVertical = new System.Windows.Forms.VScrollBar();
			this.panelZoomControls = new System.Windows.Forms.Panel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.labelZoomAmount = new System.Windows.Forms.Label();
			this.numericZoomAmount = new System.Windows.Forms.NumericUpDown();
			this.labelZoomSize = new System.Windows.Forms.Label();
			this.numericZoomWindowSize = new System.Windows.Forms.NumericUpDown();
			this.checkZoomSnap = new System.Windows.Forms.CheckBox();
			this.stripSprite = new System.Windows.Forms.ToolStrip();
			this.buttonSave = new System.Windows.Forms.ToolStripButton();
			this.buttonRevert = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonClip = new System.Windows.Forms.ToolStripButton();
			this.buttonAutoClip = new System.Windows.Forms.ToolStripButton();
			this.PanelDisplay.SuspendLayout();
			this.containerSprite.BottomToolStripPanel.SuspendLayout();
			this.containerSprite.ContentPanel.SuspendLayout();
			this.containerSprite.TopToolStripPanel.SuspendLayout();
			this.containerSprite.SuspendLayout();
			this.stripUIOptions.SuspendLayout();
			this.panelOuter.SuspendLayout();
			this.panelHScroll.SuspendLayout();
			this.panelVScroll.SuspendLayout();
			this.panelZoomControls.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomAmount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomWindowSize)).BeginInit();
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
			this.panelSprite.Size = new System.Drawing.Size(785, 510);
			this.panelSprite.TabIndex = 0;
			this.panelSprite.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelSprite_MouseDown);
			this.panelSprite.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelSprite_MouseMove);
			this.panelSprite.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelSprite_MouseUp);
			this.panelSprite.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelSprite_PreviewKeyDown);
			this.panelSprite.Resize += new System.EventHandler(this.panelSprite_Resize);
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
			this.containerSprite.ContentPanel.Controls.Add(this.panelZoomControls);
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
            this.dropDownZoom,
            this.labelMouseInfo});
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
			this.dropDownZoom.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.zoom_16x16;
			this.dropDownZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.dropDownZoom.Name = "dropDownZoom";
			this.dropDownZoom.Size = new System.Drawing.Size(128, 22);
			this.dropDownZoom.Text = "zoom: to window";
			// 
			// menuItem1600
			// 
			this.menuItem1600.CheckOnClick = true;
			this.menuItem1600.Name = "menuItem1600";
			this.menuItem1600.Size = new System.Drawing.Size(130, 22);
			this.menuItem1600.Tag = "16";
			this.menuItem1600.Text = "1600%";
			this.menuItem1600.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem1600.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem800
			// 
			this.menuItem800.CheckOnClick = true;
			this.menuItem800.Name = "menuItem800";
			this.menuItem800.Size = new System.Drawing.Size(130, 22);
			this.menuItem800.Tag = "8";
			this.menuItem800.Text = "800%";
			this.menuItem800.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem800.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem400
			// 
			this.menuItem400.CheckOnClick = true;
			this.menuItem400.Name = "menuItem400";
			this.menuItem400.Size = new System.Drawing.Size(130, 22);
			this.menuItem400.Tag = "4";
			this.menuItem400.Text = "400%";
			this.menuItem400.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem400.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem200
			// 
			this.menuItem200.CheckOnClick = true;
			this.menuItem200.Name = "menuItem200";
			this.menuItem200.Size = new System.Drawing.Size(130, 22);
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
			this.menuItem100.Size = new System.Drawing.Size(130, 22);
			this.menuItem100.Tag = "1";
			this.menuItem100.Text = "100%";
			this.menuItem100.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItem100.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem75
			// 
			this.menuItem75.CheckOnClick = true;
			this.menuItem75.Name = "menuItem75";
			this.menuItem75.Size = new System.Drawing.Size(130, 22);
			this.menuItem75.Tag = "0.75";
			this.menuItem75.Text = "75%";
			this.menuItem75.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem50
			// 
			this.menuItem50.CheckOnClick = true;
			this.menuItem50.Name = "menuItem50";
			this.menuItem50.Size = new System.Drawing.Size(130, 22);
			this.menuItem50.Tag = "0.5";
			this.menuItem50.Text = "50%";
			this.menuItem50.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// menuItem25
			// 
			this.menuItem25.CheckOnClick = true;
			this.menuItem25.Name = "menuItem25";
			this.menuItem25.Size = new System.Drawing.Size(130, 22);
			this.menuItem25.Tag = "0.25";
			this.menuItem25.Text = "25%";
			this.menuItem25.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(127, 6);
			// 
			// menuItemToWindow
			// 
			this.menuItemToWindow.CheckOnClick = true;
			this.menuItemToWindow.Name = "menuItemToWindow";
			this.menuItemToWindow.Size = new System.Drawing.Size(130, 22);
			this.menuItemToWindow.Tag = "-1";
			this.menuItemToWindow.Text = "to window";
			this.menuItemToWindow.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			this.menuItemToWindow.Click += new System.EventHandler(this.zoomItem_Click);
			// 
			// labelMouseInfo
			// 
			this.labelMouseInfo.Name = "labelMouseInfo";
			this.labelMouseInfo.Size = new System.Drawing.Size(65, 22);
			this.labelMouseInfo.Text = "mouse pos";
			// 
			// panelOuter
			// 
			this.panelOuter.Controls.Add(this.panelSprite);
			this.panelOuter.Controls.Add(this.panelHScroll);
			this.panelOuter.Controls.Add(this.panelVScroll);
			this.panelOuter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOuter.Location = new System.Drawing.Point(0, 0);
			this.panelOuter.Name = "panelOuter";
			this.panelOuter.Size = new System.Drawing.Size(806, 527);
			this.panelOuter.TabIndex = 1;
			// 
			// panelHScroll
			// 
			this.panelHScroll.AutoSize = true;
			this.panelHScroll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelHScroll.Controls.Add(this.scrollHorizontal);
			this.panelHScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelHScroll.Location = new System.Drawing.Point(0, 510);
			this.panelHScroll.Name = "panelHScroll";
			this.panelHScroll.Size = new System.Drawing.Size(785, 17);
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
			this.scrollHorizontal.TabStop = true;
			// 
			// panelVScroll
			// 
			this.panelVScroll.AutoSize = true;
			this.panelVScroll.BackColor = System.Drawing.SystemColors.Control;
			this.panelVScroll.Controls.Add(this.buttonCenter);
			this.panelVScroll.Controls.Add(this.scrollVertical);
			this.panelVScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelVScroll.Location = new System.Drawing.Point(785, 0);
			this.panelVScroll.Name = "panelVScroll";
			this.panelVScroll.Size = new System.Drawing.Size(21, 527);
			this.panelVScroll.TabIndex = 0;
			this.panelVScroll.Visible = false;
			// 
			// buttonCenter
			// 
			this.buttonCenter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.buttonCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCenter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCenter.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.pan_21x16;
			this.buttonCenter.Location = new System.Drawing.Point(0, 511);
			this.buttonCenter.Margin = new System.Windows.Forms.Padding(0);
			this.buttonCenter.Name = "buttonCenter";
			this.buttonCenter.Size = new System.Drawing.Size(21, 16);
			this.buttonCenter.TabIndex = 1;
			this.buttonCenter.TabStop = false;
			this.buttonCenter.UseVisualStyleBackColor = false;
			this.buttonCenter.Click += new System.EventHandler(this.buttonCenter_Click);
			// 
			// scrollVertical
			// 
			this.scrollVertical.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scrollVertical.Location = new System.Drawing.Point(0, 0);
			this.scrollVertical.Name = "scrollVertical";
			this.scrollVertical.Size = new System.Drawing.Size(17, 510);
			this.scrollVertical.TabIndex = 0;
			this.scrollVertical.TabStop = true;
			// 
			// panelZoomControls
			// 
			this.panelZoomControls.AutoSize = true;
			this.panelZoomControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelZoomControls.Controls.Add(this.flowLayoutPanel1);
			this.panelZoomControls.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelZoomControls.Location = new System.Drawing.Point(0, 527);
			this.panelZoomControls.Name = "panelZoomControls";
			this.panelZoomControls.Size = new System.Drawing.Size(806, 29);
			this.panelZoomControls.TabIndex = 0;
			this.panelZoomControls.Visible = false;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.Controls.Add(this.labelZoomAmount);
			this.flowLayoutPanel1.Controls.Add(this.numericZoomAmount);
			this.flowLayoutPanel1.Controls.Add(this.labelZoomSize);
			this.flowLayoutPanel1.Controls.Add(this.numericZoomWindowSize);
			this.flowLayoutPanel1.Controls.Add(this.checkZoomSnap);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(806, 29);
			this.flowLayoutPanel1.TabIndex = 5;
			// 
			// labelZoomAmount
			// 
			this.labelZoomAmount.AutoSize = true;
			this.labelZoomAmount.Location = new System.Drawing.Point(3, 7);
			this.labelZoomAmount.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
			this.labelZoomAmount.Name = "labelZoomAmount";
			this.labelZoomAmount.Size = new System.Drawing.Size(163, 15);
			this.labelZoomAmount.TabIndex = 7;
			this.labelZoomAmount.Text = "zoom amount (not localized):";
			// 
			// numericZoomAmount
			// 
			this.numericZoomAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericZoomAmount.DecimalPlaces = 1;
			this.numericZoomAmount.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.numericZoomAmount.Location = new System.Drawing.Point(172, 3);
			this.numericZoomAmount.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.numericZoomAmount.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numericZoomAmount.Name = "numericZoomAmount";
			this.numericZoomAmount.Size = new System.Drawing.Size(84, 23);
			this.numericZoomAmount.TabIndex = 8;
			this.numericZoomAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericZoomAmount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numericZoomAmount.ValueChanged += new System.EventHandler(this.numericZoomAmount_ValueChanged);
			// 
			// labelZoomSize
			// 
			this.labelZoomSize.AutoSize = true;
			this.labelZoomSize.Location = new System.Drawing.Point(262, 7);
			this.labelZoomSize.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
			this.labelZoomSize.Name = "labelZoomSize";
			this.labelZoomSize.Size = new System.Drawing.Size(185, 15);
			this.labelZoomSize.TabIndex = 5;
			this.labelZoomSize.Text = "zoom window size (not localized):";
			// 
			// numericZoomWindowSize
			// 
			this.numericZoomWindowSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericZoomWindowSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericZoomWindowSize.Location = new System.Drawing.Point(453, 3);
			this.numericZoomWindowSize.Maximum = new decimal(new int[] {
            384,
            0,
            0,
            0});
			this.numericZoomWindowSize.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericZoomWindowSize.Name = "numericZoomWindowSize";
			this.numericZoomWindowSize.Size = new System.Drawing.Size(95, 23);
			this.numericZoomWindowSize.TabIndex = 6;
			this.numericZoomWindowSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericZoomWindowSize.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericZoomWindowSize.ValueChanged += new System.EventHandler(this.numericZoomWindowSize_ValueChanged);
			// 
			// checkZoomSnap
			// 
			this.checkZoomSnap.AutoSize = true;
			this.checkZoomSnap.Location = new System.Drawing.Point(554, 7);
			this.checkZoomSnap.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
			this.checkZoomSnap.Name = "checkZoomSnap";
			this.checkZoomSnap.Size = new System.Drawing.Size(185, 19);
			this.checkZoomSnap.TabIndex = 9;
			this.checkZoomSnap.Text = "snap zoom window to corners";
			this.checkZoomSnap.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.checkZoomSnap.UseVisualStyleBackColor = true;
			this.checkZoomSnap.Click += new System.EventHandler(this.checkZoomSnap_Click);
			// 
			// stripSprite
			// 
			this.stripSprite.Dock = System.Windows.Forms.DockStyle.None;
			this.stripSprite.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripSprite.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSave,
            this.buttonRevert,
            this.toolStripSeparator1,
            this.buttonClip,
            this.buttonAutoClip});
			this.stripSprite.Location = new System.Drawing.Point(0, 0);
			this.stripSprite.Name = "stripSprite";
			this.stripSprite.Size = new System.Drawing.Size(806, 25);
			this.stripSprite.Stretch = true;
			this.stripSprite.TabIndex = 0;
			// 
			// buttonSave
			// 
			this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSave.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.save_16x16;
			this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(23, 22);
			this.buttonSave.Text = "not localized save";
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// buttonRevert
			// 
			this.buttonRevert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRevert.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.revert_16x16;
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
			// buttonClip
			// 
			this.buttonClip.CheckOnClick = true;
			this.buttonClip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonClip.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.sprite_16x16;
			this.buttonClip.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonClip.Name = "buttonClip";
			this.buttonClip.Size = new System.Drawing.Size(23, 22);
			this.buttonClip.Text = "not localized clip";
			this.buttonClip.Click += new System.EventHandler(this.buttonClip_Click);
			// 
			// buttonAutoClip
			// 
			this.buttonAutoClip.CheckOnClick = true;
			this.buttonAutoClip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonAutoClip.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.auto_clip_16x16;
			this.buttonAutoClip.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonAutoClip.Name = "buttonAutoClip";
			this.buttonAutoClip.Size = new System.Drawing.Size(23, 22);
			this.buttonAutoClip.Text = "auto clip not localized";
			this.buttonAutoClip.Click += new System.EventHandler(this.buttonAutoClip_Click);
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
			this.containerSprite.ContentPanel.PerformLayout();
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
			this.panelZoomControls.ResumeLayout(false);
			this.panelZoomControls.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomAmount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomWindowSize)).EndInit();
			this.stripSprite.ResumeLayout(false);
			this.stripSprite.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal GorgonSelectablePanel panelSprite;
		private ToolStripContainer containerSprite;
		private ToolStrip stripSprite;
		private ToolStripButton buttonSave;
		private ToolStripButton buttonRevert;
		private ToolStripSeparator toolStripSeparator1;
		private Panel panelOuter;
		private Panel panelHScroll;
		private Panel panelVScroll;
		private HScrollBar scrollHorizontal;
		private VScrollBar scrollVertical;
		private ToolStrip stripUIOptions;
		private ToolStripDropDownButton dropDownZoom;
		private ToolStripMenuItem menuItem1600;
		private ToolStripMenuItem menuItem800;
		private ToolStripMenuItem menuItem400;
		private ToolStripMenuItem menuItem200;
		private ToolStripMenuItem menuItem100;
		private ToolStripMenuItem menuItem75;
		private ToolStripMenuItem menuItem50;
		private ToolStripMenuItem menuItem25;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem menuItemToWindow;
		private Button buttonCenter;
		private ToolStripButton buttonClip;
		private Panel panelZoomControls;
		private FlowLayoutPanel flowLayoutPanel1;
		private Label labelZoomAmount;
		private NumericUpDown numericZoomAmount;
		private Label labelZoomSize;
		private NumericUpDown numericZoomWindowSize;
		private CheckBox checkZoomSnap;
		private ToolStripLabel labelMouseInfo;
		private ToolStripButton buttonAutoClip;

	}
}
