namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
	partial class FormResizeCrop
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormResizeCrop));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelDesc = new System.Windows.Forms.Label();
			this.radioCrop = new System.Windows.Forms.RadioButton();
			this.labelFilePath = new System.Windows.Forms.Label();
			this.labelSourceDimensions = new System.Windows.Forms.Label();
			this.labelDestDimensions = new System.Windows.Forms.Label();
			this.radioResize = new System.Windows.Forms.RadioButton();
			this.labelFilter = new System.Windows.Forms.Label();
			this.comboFilter = new System.Windows.Forms.ComboBox();
			this.checkPreserveAspect = new System.Windows.Forms.CheckBox();
			this.labelAnchor = new System.Windows.Forms.Label();
			this.panelAnchor = new System.Windows.Forms.Panel();
			this.radioBottomRight = new System.Windows.Forms.RadioButton();
			this.radioBottomCenter = new System.Windows.Forms.RadioButton();
			this.radioBottomLeft = new System.Windows.Forms.RadioButton();
			this.radioMiddleRight = new System.Windows.Forms.RadioButton();
			this.radioCenter = new System.Windows.Forms.RadioButton();
			this.radioMiddleLeft = new System.Windows.Forms.RadioButton();
			this.radioTopRight = new System.Windows.Forms.RadioButton();
			this.radioTopCenter = new System.Windows.Forms.RadioButton();
			this.radioTopLeft = new System.Windows.Forms.RadioButton();
			this.panel1.SuspendLayout();
			this.panel3.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.panelAnchor.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(356, 6);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(263, 6);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonOK);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(1, 262);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(448, 38);
			this.panel1.TabIndex = 16;
			// 
			// panel3
			// 
			this.panel3.AutoSize = true;
			this.panel3.Controls.Add(this.tableLayoutPanel1);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(1, 25);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(448, 237);
			this.panel3.TabIndex = 18;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanel1.Controls.Add(this.labelDesc, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.radioCrop, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.labelFilePath, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelSourceDimensions, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelDestDimensions, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.radioResize, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.labelFilter, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.comboFilter, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.checkPreserveAspect, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.labelAnchor, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.panelAnchor, 1, 5);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 9;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(448, 234);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// labelDesc
			// 
			this.labelDesc.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.labelDesc, 2);
			this.labelDesc.ForeColor = System.Drawing.Color.White;
			this.labelDesc.Location = new System.Drawing.Point(3, 5);
			this.labelDesc.Margin = new System.Windows.Forms.Padding(3, 5, 3, 8);
			this.labelDesc.Name = "labelDesc";
			this.labelDesc.Size = new System.Drawing.Size(422, 45);
			this.labelDesc.TabIndex = 0;
			this.labelDesc.Text = "NOT LOCALIZED!The image dimensions do not match the buffer dimensions.  Please se" +
    "lect crop to clip the image to the buffer dimensions, or resize to stretch/squis" +
    "h the image to the buffer dimensions.";
			// 
			// radioCrop
			// 
			this.radioCrop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.radioCrop.ForeColor = System.Drawing.Color.White;
			this.radioCrop.Location = new System.Drawing.Point(3, 106);
			this.radioCrop.Name = "radioCrop";
			this.radioCrop.Size = new System.Drawing.Size(257, 19);
			this.radioCrop.TabIndex = 4;
			this.radioCrop.TabStop = true;
			this.radioCrop.Text = "crop not localized";
			this.radioCrop.UseVisualStyleBackColor = true;
			this.radioCrop.CheckedChanged += new System.EventHandler(this.radioCrop_CheckedChanged);
			// 
			// labelFilePath
			// 
			this.labelFilePath.AutoEllipsis = true;
			this.tableLayoutPanel1.SetColumnSpan(this.labelFilePath, 2);
			this.labelFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelFilePath.ForeColor = System.Drawing.Color.White;
			this.labelFilePath.Location = new System.Drawing.Point(3, 58);
			this.labelFilePath.Name = "labelFilePath";
			this.labelFilePath.Size = new System.Drawing.Size(442, 15);
			this.labelFilePath.TabIndex = 1;
			this.labelFilePath.Text = "Image file: not localized";
			// 
			// labelSourceDimensions
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.labelSourceDimensions, 2);
			this.labelSourceDimensions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelSourceDimensions.ForeColor = System.Drawing.Color.White;
			this.labelSourceDimensions.Location = new System.Drawing.Point(3, 73);
			this.labelSourceDimensions.Name = "labelSourceDimensions";
			this.labelSourceDimensions.Size = new System.Drawing.Size(442, 15);
			this.labelSourceDimensions.TabIndex = 2;
			this.labelSourceDimensions.Text = "Image file dimensions: not localized";
			// 
			// labelDestDimensions
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.labelDestDimensions, 2);
			this.labelDestDimensions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelDestDimensions.ForeColor = System.Drawing.Color.White;
			this.labelDestDimensions.Location = new System.Drawing.Point(3, 88);
			this.labelDestDimensions.Name = "labelDestDimensions";
			this.labelDestDimensions.Size = new System.Drawing.Size(442, 15);
			this.labelDestDimensions.TabIndex = 3;
			this.labelDestDimensions.Text = "Buffer dimensions: not localized";
			// 
			// radioResize
			// 
			this.radioResize.Dock = System.Windows.Forms.DockStyle.Fill;
			this.radioResize.ForeColor = System.Drawing.Color.White;
			this.radioResize.Location = new System.Drawing.Point(3, 131);
			this.radioResize.Name = "radioResize";
			this.radioResize.Size = new System.Drawing.Size(257, 19);
			this.radioResize.TabIndex = 5;
			this.radioResize.TabStop = true;
			this.radioResize.Text = "resize not localized";
			this.radioResize.UseVisualStyleBackColor = true;
			this.radioResize.CheckedChanged += new System.EventHandler(this.radioCrop_CheckedChanged);
			// 
			// labelFilter
			// 
			this.labelFilter.AutoSize = true;
			this.labelFilter.Enabled = false;
			this.labelFilter.ForeColor = System.Drawing.Color.White;
			this.labelFilter.Location = new System.Drawing.Point(24, 153);
			this.labelFilter.Margin = new System.Windows.Forms.Padding(24, 0, 3, 0);
			this.labelFilter.Name = "labelFilter";
			this.labelFilter.Size = new System.Drawing.Size(101, 15);
			this.labelFilter.TabIndex = 6;
			this.labelFilter.Text = "not localized filter";
			// 
			// comboFilter
			// 
			this.comboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFilter.Enabled = false;
			this.comboFilter.FormattingEnabled = true;
			this.comboFilter.Location = new System.Drawing.Point(24, 171);
			this.comboFilter.Margin = new System.Windows.Forms.Padding(24, 3, 3, 3);
			this.comboFilter.Name = "comboFilter";
			this.comboFilter.Size = new System.Drawing.Size(196, 23);
			this.comboFilter.TabIndex = 7;
			// 
			// checkPreserveAspect
			// 
			this.checkPreserveAspect.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.checkPreserveAspect.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkPreserveAspect.Enabled = false;
			this.checkPreserveAspect.Location = new System.Drawing.Point(24, 200);
			this.checkPreserveAspect.Margin = new System.Windows.Forms.Padding(24, 3, 3, 3);
			this.checkPreserveAspect.Name = "checkPreserveAspect";
			this.checkPreserveAspect.Size = new System.Drawing.Size(236, 31);
			this.checkPreserveAspect.TabIndex = 8;
			this.checkPreserveAspect.Text = "Not localized aspect ratio";
			this.checkPreserveAspect.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.checkPreserveAspect.UseVisualStyleBackColor = true;
			// 
			// labelAnchor
			// 
			this.labelAnchor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelAnchor.ForeColor = System.Drawing.Color.White;
			this.labelAnchor.Location = new System.Drawing.Point(266, 106);
			this.labelAnchor.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.labelAnchor.Name = "labelAnchor";
			this.labelAnchor.Size = new System.Drawing.Size(179, 22);
			this.labelAnchor.TabIndex = 9;
			this.labelAnchor.Text = "not localized anchor";
			this.labelAnchor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelAnchor
			// 
			this.panelAnchor.Controls.Add(this.radioBottomRight);
			this.panelAnchor.Controls.Add(this.radioBottomCenter);
			this.panelAnchor.Controls.Add(this.radioBottomLeft);
			this.panelAnchor.Controls.Add(this.radioMiddleRight);
			this.panelAnchor.Controls.Add(this.radioCenter);
			this.panelAnchor.Controls.Add(this.radioMiddleLeft);
			this.panelAnchor.Controls.Add(this.radioTopRight);
			this.panelAnchor.Controls.Add(this.radioTopCenter);
			this.panelAnchor.Controls.Add(this.radioTopLeft);
			this.panelAnchor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelAnchor.Location = new System.Drawing.Point(266, 131);
			this.panelAnchor.Name = "panelAnchor";
			this.tableLayoutPanel1.SetRowSpan(this.panelAnchor, 4);
			this.panelAnchor.Size = new System.Drawing.Size(179, 100);
			this.panelAnchor.TabIndex = 10;
			// 
			// radioBottomRight
			// 
			this.radioBottomRight.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioBottomRight.BackColor = System.Drawing.SystemColors.Control;
			this.radioBottomRight.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioBottomRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioBottomRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioBottomRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioBottomRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioBottomRight.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_se_16x16;
			this.radioBottomRight.Location = new System.Drawing.Point(110, 67);
			this.radioBottomRight.Name = "radioBottomRight";
			this.radioBottomRight.Size = new System.Drawing.Size(29, 26);
			this.radioBottomRight.TabIndex = 8;
			this.radioBottomRight.UseVisualStyleBackColor = false;
			// 
			// radioBottomCenter
			// 
			this.radioBottomCenter.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioBottomCenter.BackColor = System.Drawing.SystemColors.Control;
			this.radioBottomCenter.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioBottomCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioBottomCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioBottomCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioBottomCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioBottomCenter.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_down_16x16;
			this.radioBottomCenter.Location = new System.Drawing.Point(75, 67);
			this.radioBottomCenter.Name = "radioBottomCenter";
			this.radioBottomCenter.Size = new System.Drawing.Size(29, 26);
			this.radioBottomCenter.TabIndex = 7;
			this.radioBottomCenter.UseVisualStyleBackColor = false;
			// 
			// radioBottomLeft
			// 
			this.radioBottomLeft.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioBottomLeft.BackColor = System.Drawing.SystemColors.Control;
			this.radioBottomLeft.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioBottomLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioBottomLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioBottomLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioBottomLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioBottomLeft.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_sw_16x16;
			this.radioBottomLeft.Location = new System.Drawing.Point(40, 67);
			this.radioBottomLeft.Name = "radioBottomLeft";
			this.radioBottomLeft.Size = new System.Drawing.Size(29, 26);
			this.radioBottomLeft.TabIndex = 6;
			this.radioBottomLeft.UseVisualStyleBackColor = false;
			// 
			// radioMiddleRight
			// 
			this.radioMiddleRight.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioMiddleRight.BackColor = System.Drawing.SystemColors.Control;
			this.radioMiddleRight.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioMiddleRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioMiddleRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioMiddleRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioMiddleRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioMiddleRight.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_right_16x16;
			this.radioMiddleRight.Location = new System.Drawing.Point(110, 35);
			this.radioMiddleRight.Name = "radioMiddleRight";
			this.radioMiddleRight.Size = new System.Drawing.Size(29, 26);
			this.radioMiddleRight.TabIndex = 5;
			this.radioMiddleRight.UseVisualStyleBackColor = false;
			// 
			// radioCenter
			// 
			this.radioCenter.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioCenter.BackColor = System.Drawing.SystemColors.Control;
			this.radioCenter.Checked = true;
			this.radioCenter.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioCenter.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.center_16x16;
			this.radioCenter.Location = new System.Drawing.Point(75, 35);
			this.radioCenter.Name = "radioCenter";
			this.radioCenter.Size = new System.Drawing.Size(29, 26);
			this.radioCenter.TabIndex = 4;
			this.radioCenter.TabStop = true;
			this.radioCenter.UseVisualStyleBackColor = false;
			// 
			// radioMiddleLeft
			// 
			this.radioMiddleLeft.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioMiddleLeft.BackColor = System.Drawing.SystemColors.Control;
			this.radioMiddleLeft.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioMiddleLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioMiddleLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioMiddleLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioMiddleLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioMiddleLeft.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_left_16x16;
			this.radioMiddleLeft.Location = new System.Drawing.Point(40, 35);
			this.radioMiddleLeft.Name = "radioMiddleLeft";
			this.radioMiddleLeft.Size = new System.Drawing.Size(29, 26);
			this.radioMiddleLeft.TabIndex = 3;
			this.radioMiddleLeft.UseVisualStyleBackColor = false;
			// 
			// radioTopRight
			// 
			this.radioTopRight.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioTopRight.BackColor = System.Drawing.SystemColors.Control;
			this.radioTopRight.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioTopRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioTopRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioTopRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioTopRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioTopRight.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_ne_16x16;
			this.radioTopRight.Location = new System.Drawing.Point(110, 3);
			this.radioTopRight.Name = "radioTopRight";
			this.radioTopRight.Size = new System.Drawing.Size(29, 26);
			this.radioTopRight.TabIndex = 2;
			this.radioTopRight.UseVisualStyleBackColor = false;
			// 
			// radioTopCenter
			// 
			this.radioTopCenter.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioTopCenter.BackColor = System.Drawing.SystemColors.Control;
			this.radioTopCenter.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioTopCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioTopCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioTopCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioTopCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioTopCenter.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_up_16x16;
			this.radioTopCenter.Location = new System.Drawing.Point(75, 3);
			this.radioTopCenter.Name = "radioTopCenter";
			this.radioTopCenter.Size = new System.Drawing.Size(29, 26);
			this.radioTopCenter.TabIndex = 1;
			this.radioTopCenter.UseVisualStyleBackColor = false;
			// 
			// radioTopLeft
			// 
			this.radioTopLeft.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioTopLeft.BackColor = System.Drawing.SystemColors.Control;
			this.radioTopLeft.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioTopLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioTopLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioTopLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioTopLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioTopLeft.Image = global::GorgonLibrary.Editor.ImageEditorPlugIn.Properties.Resources.arrow_nw_16x16;
			this.radioTopLeft.Location = new System.Drawing.Point(40, 3);
			this.radioTopLeft.Name = "radioTopLeft";
			this.radioTopLeft.Size = new System.Drawing.Size(29, 26);
			this.radioTopLeft.TabIndex = 0;
			this.radioTopLeft.UseVisualStyleBackColor = false;
			// 
			// FormResizeCrop
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(450, 301);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(500, 500);
			this.MinimizeBox = false;
			this.Name = "FormResizeCrop";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Not localized";
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.panel3, 0);
			this.panel1.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panelAnchor.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioCrop;
		private System.Windows.Forms.Label labelDesc;
		private System.Windows.Forms.Label labelDestDimensions;
		private System.Windows.Forms.Label labelSourceDimensions;
		private System.Windows.Forms.Label labelFilePath;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.RadioButton radioResize;
		private System.Windows.Forms.Label labelFilter;
		private System.Windows.Forms.ComboBox comboFilter;
		private System.Windows.Forms.CheckBox checkPreserveAspect;
		private System.Windows.Forms.Label labelAnchor;
		private System.Windows.Forms.Panel panelAnchor;
		private System.Windows.Forms.RadioButton radioBottomRight;
		private System.Windows.Forms.RadioButton radioBottomCenter;
		private System.Windows.Forms.RadioButton radioBottomLeft;
		private System.Windows.Forms.RadioButton radioMiddleRight;
		private System.Windows.Forms.RadioButton radioCenter;
		private System.Windows.Forms.RadioButton radioMiddleLeft;
		private System.Windows.Forms.RadioButton radioTopRight;
		private System.Windows.Forms.RadioButton radioTopCenter;
		private System.Windows.Forms.RadioButton radioTopLeft;
	}
}