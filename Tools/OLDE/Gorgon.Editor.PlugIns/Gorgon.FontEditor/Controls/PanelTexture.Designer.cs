namespace Gorgon.Editor.FontEditorPlugIn.Controls
{
	partial class PanelTexture
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
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.panelTextureDisplay = new System.Windows.Forms.Panel();
            this.comboWrapMode = new System.Windows.Forms.ComboBox();
            this.numericHeight = new System.Windows.Forms.NumericUpDown();
            this.numericWidth = new System.Windows.Forms.NumericUpDown();
            this.numericY = new System.Windows.Forms.NumericUpDown();
            this.numericX = new System.Windows.Forms.NumericUpDown();
            this.labelWrapMode = new System.Windows.Forms.Label();
            this.labelTexHeight = new System.Windows.Forms.Label();
            this.labelTexWidth = new System.Windows.Forms.Label();
            this.labelTexTop = new System.Windows.Forms.Label();
            this.labelTexLeft = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.imageFileBrowser = new Gorgon.Editor.EditorFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.numericHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfo.Location = new System.Drawing.Point(8, 117);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(269, 192);
            this.labelInfo.TabIndex = 7;
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonOpen
            // 
            this.buttonOpen.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOpen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOpen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonOpen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonOpen.Image = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.open_image_16x16;
            this.buttonOpen.Location = new System.Drawing.Point(14, 278);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(256, 28);
            this.buttonOpen.TabIndex = 1;
            this.buttonOpen.Text = "open texture";
            this.buttonOpen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonOpen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonOpen.UseVisualStyleBackColor = false;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // panelTextureDisplay
            // 
            this.panelTextureDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTextureDisplay.Location = new System.Drawing.Point(14, 16);
            this.panelTextureDisplay.Name = "panelTextureDisplay";
            this.panelTextureDisplay.Size = new System.Drawing.Size(256, 256);
            this.panelTextureDisplay.TabIndex = 0;
            this.panelTextureDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseDown);
            this.panelTextureDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseMove);
            this.panelTextureDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelTextureDisplay_MouseUp);
            // 
            // comboWrapMode
            // 
            this.comboWrapMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboWrapMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWrapMode.FormattingEnabled = true;
            this.comboWrapMode.Location = new System.Drawing.Point(109, 64);
            this.comboWrapMode.Name = "comboWrapMode";
            this.comboWrapMode.Size = new System.Drawing.Size(168, 21);
            this.comboWrapMode.TabIndex = 4;
            this.comboWrapMode.SelectedIndexChanged += new System.EventHandler(this.comboWrapMode_SelectedIndexChanged);
            // 
            // numericHeight
            // 
            this.numericHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericHeight.Location = new System.Drawing.Point(190, 29);
            this.numericHeight.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.numericHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericHeight.Name = "numericHeight";
            this.numericHeight.Size = new System.Drawing.Size(87, 20);
            this.numericHeight.TabIndex = 3;
            this.numericHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericHeight.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
            // 
            // numericWidth
            // 
            this.numericWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericWidth.Location = new System.Drawing.Point(190, 3);
            this.numericWidth.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.numericWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericWidth.Name = "numericWidth";
            this.numericWidth.Size = new System.Drawing.Size(87, 20);
            this.numericWidth.TabIndex = 2;
            this.numericWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericWidth.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
            // 
            // numericY
            // 
            this.numericY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericY.Location = new System.Drawing.Point(42, 29);
            this.numericY.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.numericY.Name = "numericY";
            this.numericY.Size = new System.Drawing.Size(87, 20);
            this.numericY.TabIndex = 1;
            this.numericY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericY.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
            // 
            // numericX
            // 
            this.numericX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericX.Location = new System.Drawing.Point(42, 3);
            this.numericX.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.numericX.Name = "numericX";
            this.numericX.Size = new System.Drawing.Size(87, 20);
            this.numericX.TabIndex = 0;
            this.numericX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericX.ValueChanged += new System.EventHandler(this.numericX_ValueChanged);
            // 
            // labelWrapMode
            // 
            this.labelWrapMode.AutoSize = true;
            this.labelWrapMode.Location = new System.Drawing.Point(5, 67);
            this.labelWrapMode.Name = "labelWrapMode";
            this.labelWrapMode.Size = new System.Drawing.Size(79, 13);
            this.labelWrapMode.TabIndex = 18;
            this.labelWrapMode.Text = "wrapping mode";
            // 
            // labelTexHeight
            // 
            this.labelTexHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTexHeight.AutoSize = true;
            this.labelTexHeight.Location = new System.Drawing.Point(143, 31);
            this.labelTexHeight.Name = "labelTexHeight";
            this.labelTexHeight.Size = new System.Drawing.Size(13, 13);
            this.labelTexHeight.TabIndex = 16;
            this.labelTexHeight.Text = "h";
            // 
            // labelTexWidth
            // 
            this.labelTexWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTexWidth.AutoSize = true;
            this.labelTexWidth.Location = new System.Drawing.Point(143, 5);
            this.labelTexWidth.Name = "labelTexWidth";
            this.labelTexWidth.Size = new System.Drawing.Size(15, 13);
            this.labelTexWidth.TabIndex = 14;
            this.labelTexWidth.Text = "w";
            // 
            // labelTexTop
            // 
            this.labelTexTop.AutoSize = true;
            this.labelTexTop.Location = new System.Drawing.Point(5, 31);
            this.labelTexTop.Name = "labelTexTop";
            this.labelTexTop.Size = new System.Drawing.Size(12, 13);
            this.labelTexTop.TabIndex = 12;
            this.labelTexTop.Text = "y";
            // 
            // labelTexLeft
            // 
            this.labelTexLeft.AutoSize = true;
            this.labelTexLeft.Location = new System.Drawing.Point(5, 5);
            this.labelTexLeft.Name = "labelTexLeft";
            this.labelTexLeft.Size = new System.Drawing.Size(12, 13);
            this.labelTexLeft.TabIndex = 10;
            this.labelTexLeft.Text = "x";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelTextureDisplay);
            this.panel1.Controls.Add(this.buttonOpen);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(285, 321);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.numericX);
            this.panel2.Controls.Add(this.labelTexLeft);
            this.panel2.Controls.Add(this.labelInfo);
            this.panel2.Controls.Add(this.labelTexTop);
            this.panel2.Controls.Add(this.comboWrapMode);
            this.panel2.Controls.Add(this.labelTexWidth);
            this.panel2.Controls.Add(this.numericHeight);
            this.panel2.Controls.Add(this.labelTexHeight);
            this.panel2.Controls.Add(this.numericWidth);
            this.panel2.Controls.Add(this.labelWrapMode);
            this.panel2.Controls.Add(this.numericY);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(285, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(280, 321);
            this.panel2.TabIndex = 20;
            // 
            // imageFileBrowser
            // 
            this.imageFileBrowser.Filename = null;
            this.imageFileBrowser.StartDirectory = null;
            this.imageFileBrowser.Text = "Open Image";
            // 
            // PanelTexture
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "PanelTexture";
            this.Size = new System.Drawing.Size(565, 321);
            ((System.ComponentModel.ISupportInitialize)(this.numericHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericX)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.Button buttonOpen;
		private System.Windows.Forms.Panel panelTextureDisplay;
		private System.Windows.Forms.ComboBox comboWrapMode;
		private System.Windows.Forms.NumericUpDown numericHeight;
		private System.Windows.Forms.NumericUpDown numericWidth;
		private System.Windows.Forms.NumericUpDown numericY;
		private System.Windows.Forms.NumericUpDown numericX;
		private System.Windows.Forms.Label labelWrapMode;
		private System.Windows.Forms.Label labelTexHeight;
		private System.Windows.Forms.Label labelTexWidth;
		private System.Windows.Forms.Label labelTexTop;
		private System.Windows.Forms.Label labelTexLeft;
		private EditorFileDialog imageFileBrowser;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
	}
}
