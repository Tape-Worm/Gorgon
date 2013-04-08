namespace GorgonLibrary.Editor
{
	partial class formAlphaPicker
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formAlphaPicker));
			this.sliderAlpha = new Fetze.WinFormsColor.ColorSlider();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.numericAlphaValue = new System.Windows.Forms.NumericUpDown();
			this.colorPreview = new Fetze.WinFormsColor.ColorShowBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numericAlphaValue)).BeginInit();
			this.SuspendLayout();
			// 
			// sliderAlpha
			// 
			this.sliderAlpha.Location = new System.Drawing.Point(0, 0);
			this.sliderAlpha.Maximum = System.Drawing.Color.White;
			this.sliderAlpha.Minimum = System.Drawing.Color.Empty;
			this.sliderAlpha.Name = "sliderAlpha";
			this.sliderAlpha.Size = new System.Drawing.Size(201, 173);
			this.sliderAlpha.TabIndex = 0;
			this.sliderAlpha.ValueChanged += new System.EventHandler(this.sliderAlpha_ValueChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(221, 80);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "Alpha:";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.Location = new System.Drawing.Point(152, 181);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 27);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "&OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(245, 181);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// numericAlphaValue
			// 
			this.numericAlphaValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numericAlphaValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericAlphaValue.Location = new System.Drawing.Point(268, 78);
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
			this.colorPreview.Location = new System.Drawing.Point(207, 12);
			this.colorPreview.LowerColor = System.Drawing.Color.Black;
			this.colorPreview.Name = "colorPreview";
			this.colorPreview.Size = new System.Drawing.Size(54, 50);
			this.colorPreview.TabIndex = 5;
			this.colorPreview.UpperColor = System.Drawing.Color.White;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(267, 17);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "Old Alpha";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(267, 44);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(65, 15);
			this.label3.TabIndex = 7;
			this.label3.Text = "New Alpha";
			// 
			// formAlphaPicker
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(344, 220);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.colorPreview);
			this.Controls.Add(this.numericAlphaValue);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.sliderAlpha);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formAlphaPicker";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Pick an alpha value";
			((System.ComponentModel.ISupportInitialize)(this.numericAlphaValue)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Fetze.WinFormsColor.ColorSlider sliderAlpha;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.NumericUpDown numericAlphaValue;
		private Fetze.WinFormsColor.ColorShowBox colorPreview;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
	}
}