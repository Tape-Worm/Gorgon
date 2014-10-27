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
			this.panel2 = new System.Windows.Forms.Panel();
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
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
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
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonOK);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(1, 237);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(448, 38);
			this.panel1.TabIndex = 16;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(1, 25);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(448, 212);
			this.panel2.TabIndex = 17;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.tableLayoutPanel1);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(448, 212);
			this.panel3.TabIndex = 18;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.labelDesc, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.radioCrop, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.labelFilePath, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelSourceDimensions, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelDestDimensions, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.radioResize, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.labelFilter, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.comboFilter, 0, 7);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(448, 212);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// labelDesc
			// 
			this.labelDesc.AutoSize = true;
			this.labelDesc.ForeColor = System.Drawing.Color.White;
			this.labelDesc.Location = new System.Drawing.Point(3, 0);
			this.labelDesc.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
			this.labelDesc.Name = "labelDesc";
			this.labelDesc.Size = new System.Drawing.Size(422, 45);
			this.labelDesc.TabIndex = 0;
			this.labelDesc.Text = "NOT LOCALIZED!The image dimensions do not match the buffer dimensions.  Please se" +
    "lect crop to clip the image to the buffer dimensions, or resize to stretch/squis" +
    "h the image to the buffer dimensions.";
			// 
			// radioCrop
			// 
			this.radioCrop.AutoSize = true;
			this.radioCrop.ForeColor = System.Drawing.Color.White;
			this.radioCrop.Location = new System.Drawing.Point(3, 106);
			this.radioCrop.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.radioCrop.Name = "radioCrop";
			this.radioCrop.Size = new System.Drawing.Size(119, 19);
			this.radioCrop.TabIndex = 4;
			this.radioCrop.TabStop = true;
			this.radioCrop.Text = "crop not localized";
			this.radioCrop.UseVisualStyleBackColor = true;
			this.radioCrop.CheckedChanged += new System.EventHandler(this.radioCrop_CheckedChanged);
			// 
			// labelFilePath
			// 
			this.labelFilePath.AutoSize = true;
			this.labelFilePath.ForeColor = System.Drawing.Color.White;
			this.labelFilePath.Location = new System.Drawing.Point(3, 53);
			this.labelFilePath.Name = "labelFilePath";
			this.labelFilePath.Size = new System.Drawing.Size(132, 15);
			this.labelFilePath.TabIndex = 1;
			this.labelFilePath.Text = "Image file: not localized";
			// 
			// labelSourceDimensions
			// 
			this.labelSourceDimensions.AutoSize = true;
			this.labelSourceDimensions.ForeColor = System.Drawing.Color.White;
			this.labelSourceDimensions.Location = new System.Drawing.Point(3, 68);
			this.labelSourceDimensions.Name = "labelSourceDimensions";
			this.labelSourceDimensions.Size = new System.Drawing.Size(196, 15);
			this.labelSourceDimensions.TabIndex = 2;
			this.labelSourceDimensions.Text = "Image file dimensions: not localized";
			// 
			// labelDestDimensions
			// 
			this.labelDestDimensions.AutoSize = true;
			this.labelDestDimensions.ForeColor = System.Drawing.Color.White;
			this.labelDestDimensions.Location = new System.Drawing.Point(3, 83);
			this.labelDestDimensions.Name = "labelDestDimensions";
			this.labelDestDimensions.Size = new System.Drawing.Size(176, 15);
			this.labelDestDimensions.TabIndex = 3;
			this.labelDestDimensions.Text = "Buffer dimensions: not localized";
			// 
			// radioResize
			// 
			this.radioResize.AutoSize = true;
			this.radioResize.ForeColor = System.Drawing.Color.White;
			this.radioResize.Location = new System.Drawing.Point(3, 131);
			this.radioResize.Name = "radioResize";
			this.radioResize.Size = new System.Drawing.Size(124, 19);
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
			this.comboFilter.SelectedIndexChanged += new System.EventHandler(this.comboFilter_SelectedIndexChanged);
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
			this.ClientSize = new System.Drawing.Size(450, 276);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormResizeCrop";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Not localized";
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.panel2, 0);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
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
	}
}