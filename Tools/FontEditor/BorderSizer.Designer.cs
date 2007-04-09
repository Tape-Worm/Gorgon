namespace GorgonLibrary.Graphics.Tools
{
	partial class BorderSizer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BorderSizer));
			this.numericTop = new System.Windows.Forms.NumericUpDown();
			this.numericLeft = new System.Windows.Forms.NumericUpDown();
			this.numericRight = new System.Windows.Forms.NumericUpDown();
			this.numericBottom = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.numericTop)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericLeft)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericRight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericBottom)).BeginInit();
			this.SuspendLayout();
			// 
			// numericTop
			// 
			this.numericTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericTop.Location = new System.Drawing.Point(70, 12);
			this.numericTop.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.numericTop.Name = "numericTop";
			this.numericTop.Size = new System.Drawing.Size(58, 20);
			this.numericTop.TabIndex = 0;
			this.numericTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericTop.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
			// 
			// numericLeft
			// 
			this.numericLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericLeft.Location = new System.Drawing.Point(6, 31);
			this.numericLeft.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.numericLeft.Name = "numericLeft";
			this.numericLeft.Size = new System.Drawing.Size(58, 20);
			this.numericLeft.TabIndex = 3;
			this.numericLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericLeft.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
			// 
			// numericRight
			// 
			this.numericRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericRight.Location = new System.Drawing.Point(134, 31);
			this.numericRight.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.numericRight.Name = "numericRight";
			this.numericRight.Size = new System.Drawing.Size(58, 20);
			this.numericRight.TabIndex = 1;
			this.numericRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericRight.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
			// 
			// numericBottom
			// 
			this.numericBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericBottom.Location = new System.Drawing.Point(70, 52);
			this.numericBottom.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.numericBottom.Name = "numericBottom";
			this.numericBottom.Size = new System.Drawing.Size(58, 20);
			this.numericBottom.TabIndex = 2;
			this.numericBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericBottom.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
			// 
			// BorderSizer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(201, 79);
			this.Controls.Add(this.numericBottom);
			this.Controls.Add(this.numericRight);
			this.Controls.Add(this.numericLeft);
			this.Controls.Add(this.numericTop);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "BorderSizer";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Resize borders.";
			((System.ComponentModel.ISupportInitialize)(this.numericTop)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericLeft)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericRight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericBottom)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown numericTop;
		private System.Windows.Forms.NumericUpDown numericLeft;
		private System.Windows.Forms.NumericUpDown numericRight;
		private System.Windows.Forms.NumericUpDown numericBottom;


	}
}