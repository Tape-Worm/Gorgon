namespace GorgonLibrary.Graphics.Tools
{
	partial class ImageSize
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageSize));
			this.labelImageSize = new System.Windows.Forms.Label();
			this.numericImageHeight = new System.Windows.Forms.NumericUpDown();
			this.numericImageWidth = new System.Windows.Forms.NumericUpDown();
			this.labelBy = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericImageHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageWidth)).BeginInit();
			this.SuspendLayout();
			// 
			// labelImageSize
			// 
			this.labelImageSize.AutoSize = true;
			this.labelImageSize.Location = new System.Drawing.Point(12, 9);
			this.labelImageSize.Name = "labelImageSize";
			this.labelImageSize.Size = new System.Drawing.Size(87, 13);
			this.labelImageSize.TabIndex = 18;
			this.labelImageSize.Text = "Font texture size:";
			// 
			// numericImageHeight
			// 
			this.numericImageHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericImageHeight.Increment = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageHeight.Location = new System.Drawing.Point(78, 25);
			this.numericImageHeight.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.numericImageHeight.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageHeight.Name = "numericImageHeight";
			this.numericImageHeight.Size = new System.Drawing.Size(56, 20);
			this.numericImageHeight.TabIndex = 17;
			this.numericImageHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericImageHeight.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// numericImageWidth
			// 
			this.numericImageWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericImageWidth.Increment = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageWidth.Location = new System.Drawing.Point(15, 25);
			this.numericImageWidth.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.numericImageWidth.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.numericImageWidth.Name = "numericImageWidth";
			this.numericImageWidth.Size = new System.Drawing.Size(52, 20);
			this.numericImageWidth.TabIndex = 16;
			this.numericImageWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericImageWidth.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// labelBy
			// 
			this.labelBy.AutoSize = true;
			this.labelBy.BackColor = System.Drawing.Color.Transparent;
			this.labelBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelBy.Location = new System.Drawing.Point(67, 27);
			this.labelBy.Name = "labelBy";
			this.labelBy.Size = new System.Drawing.Size(13, 13);
			this.labelBy.TabIndex = 19;
			this.labelBy.Text = "x";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(110, 55);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 24);
			this.buttonCancel.TabIndex = 21;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(80, 55);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 24);
			this.buttonOK.TabIndex = 20;
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// ImageSize
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(146, 91);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.labelImageSize);
			this.Controls.Add(this.numericImageHeight);
			this.Controls.Add(this.numericImageWidth);
			this.Controls.Add(this.labelBy);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImageSize";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Set font image size";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImageSize_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.numericImageHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageWidth)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.Label labelImageSize;
		internal System.Windows.Forms.NumericUpDown numericImageHeight;
		internal System.Windows.Forms.NumericUpDown numericImageWidth;
		internal System.Windows.Forms.Label labelBy;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
	}
}