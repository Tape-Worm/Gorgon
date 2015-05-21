namespace Gorgon.Editor
{
	partial class AlphaChannelDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlphaChannelDialog));
			this.sliderAlpha = new Fetze.WinFormsColor.ColorSlider();
			this.labelAlpha = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.numericAlphaValue = new System.Windows.Forms.NumericUpDown();
			this.colorPreview = new Fetze.WinFormsColor.ColorShowBox();
			this.labelOldAlpha = new System.Windows.Forms.Label();
			this.labelNewAlpha = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numericAlphaValue)).BeginInit();
			this.SuspendLayout();
			// 
			// sliderAlpha
			// 
			this.sliderAlpha.Location = new System.Drawing.Point(4, 31);
			this.sliderAlpha.Maximum = System.Drawing.Color.White;
			this.sliderAlpha.Minimum = System.Drawing.Color.Empty;
			this.sliderAlpha.Name = "sliderAlpha";
			this.sliderAlpha.Size = new System.Drawing.Size(201, 173);
			this.sliderAlpha.TabIndex = 0;
			this.sliderAlpha.ValueChanged += new System.EventHandler(this.sliderAlpha_ValueChanged);
			// 
			// labelAlpha
			// 
			this.labelAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelAlpha.AutoSize = true;
			this.labelAlpha.ForeColor = System.Drawing.Color.White;
			this.labelAlpha.Location = new System.Drawing.Point(208, 112);
			this.labelAlpha.Name = "labelAlpha";
			this.labelAlpha.Size = new System.Drawing.Size(36, 15);
			this.labelAlpha.TabIndex = 1;
			this.labelAlpha.Text = "alpha";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::Gorgon.Editor.Properties.APIResources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(151, 208);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 27);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "ok";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::Gorgon.Editor.Properties.APIResources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(244, 208);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// numericAlphaValue
			// 
			this.numericAlphaValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numericAlphaValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericAlphaValue.Location = new System.Drawing.Point(211, 130);
			this.numericAlphaValue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericAlphaValue.Name = "numericAlphaValue";
			this.numericAlphaValue.Size = new System.Drawing.Size(59, 23);
			this.numericAlphaValue.TabIndex = 1;
			this.numericAlphaValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericAlphaValue.ValueChanged += new System.EventHandler(this.numericAlphaValue_ValueChanged);
			// 
			// colorPreview
			// 
			this.colorPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.colorPreview.Color = System.Drawing.Color.White;
			this.colorPreview.Location = new System.Drawing.Point(211, 43);
			this.colorPreview.LowerColor = System.Drawing.Color.Black;
			this.colorPreview.Name = "colorPreview";
			this.colorPreview.Size = new System.Drawing.Size(54, 50);
			this.colorPreview.TabIndex = 5;
			this.colorPreview.UpperColor = System.Drawing.Color.White;
			// 
			// labelOldAlpha
			// 
			this.labelOldAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelOldAlpha.AutoSize = true;
			this.labelOldAlpha.ForeColor = System.Drawing.Color.White;
			this.labelOldAlpha.Location = new System.Drawing.Point(270, 49);
			this.labelOldAlpha.Name = "labelOldAlpha";
			this.labelOldAlpha.Size = new System.Drawing.Size(56, 15);
			this.labelOldAlpha.TabIndex = 6;
			this.labelOldAlpha.Text = "old alpha";
			// 
			// labelNewAlpha
			// 
			this.labelNewAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelNewAlpha.AutoSize = true;
			this.labelNewAlpha.ForeColor = System.Drawing.Color.White;
			this.labelNewAlpha.Location = new System.Drawing.Point(270, 76);
			this.labelNewAlpha.Name = "labelNewAlpha";
			this.labelNewAlpha.Size = new System.Drawing.Size(61, 15);
			this.labelNewAlpha.TabIndex = 7;
			this.labelNewAlpha.Text = "new alpha";
			// 
			// AlphaChannelDialog
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(344, 248);
			this.Controls.Add(this.labelNewAlpha);
			this.Controls.Add(this.labelOldAlpha);
			this.Controls.Add(this.colorPreview);
			this.Controls.Add(this.numericAlphaValue);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.labelAlpha);
			this.Controls.Add(this.sliderAlpha);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AlphaChannelDialog";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.ResizeHandleSize = 1;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "pick alpha value";
			this.Controls.SetChildIndex(this.sliderAlpha, 0);
			this.Controls.SetChildIndex(this.labelAlpha, 0);
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.numericAlphaValue, 0);
			this.Controls.SetChildIndex(this.colorPreview, 0);
			this.Controls.SetChildIndex(this.labelOldAlpha, 0);
			this.Controls.SetChildIndex(this.labelNewAlpha, 0);
			((System.ComponentModel.ISupportInitialize)(this.numericAlphaValue)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Fetze.WinFormsColor.ColorSlider sliderAlpha;
		private System.Windows.Forms.Label labelAlpha;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.NumericUpDown numericAlphaValue;
		private Fetze.WinFormsColor.ColorShowBox colorPreview;
		private System.Windows.Forms.Label labelOldAlpha;
		private System.Windows.Forms.Label labelNewAlpha;
	}
}