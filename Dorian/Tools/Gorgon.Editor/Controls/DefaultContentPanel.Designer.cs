namespace GorgonLibrary.Editor
{
	partial class DefaultContentPanel
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
			this.checkPulse = new System.Windows.Forms.CheckBox();
			this.numericPulseRate = new System.Windows.Forms.NumericUpDown();
			this.panelOptions = new System.Windows.Forms.Panel();
			this.labelClosePanel = new System.Windows.Forms.Label();
			this.PanelDisplay.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericPulseRate)).BeginInit();
			this.panelOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelDisplay
			// 
			this.PanelDisplay.Controls.Add(this.panelOptions);
			this.PanelDisplay.Location = new System.Drawing.Point(0, 0);
			this.PanelDisplay.Size = new System.Drawing.Size(522, 393);
			// 
			// checkPulse
			// 
			this.checkPulse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPulse.AutoSize = true;
			this.checkPulse.ForeColor = System.Drawing.Color.White;
			this.checkPulse.Location = new System.Drawing.Point(296, -24);
			this.checkPulse.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.checkPulse.Name = "checkPulse";
			this.checkPulse.Size = new System.Drawing.Size(119, 21);
			this.checkPulse.TabIndex = 1;
			this.checkPulse.Text = "Animated Logo:";
			this.checkPulse.UseVisualStyleBackColor = true;
			this.checkPulse.Visible = false;
			this.checkPulse.Click += new System.EventHandler(this.checkPulse_Click);
			// 
			// numericPulseRate
			// 
			this.numericPulseRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.numericPulseRate.DecimalPlaces = 3;
			this.numericPulseRate.Increment = new decimal(new int[] {
            25,
            0,
            0,
            196608});
			this.numericPulseRate.Location = new System.Drawing.Point(421, -25);
			this.numericPulseRate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.numericPulseRate.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericPulseRate.Name = "numericPulseRate";
			this.numericPulseRate.Size = new System.Drawing.Size(75, 25);
			this.numericPulseRate.TabIndex = 2;
			this.numericPulseRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericPulseRate.Visible = false;
			this.numericPulseRate.ValueChanged += new System.EventHandler(this.numericPulseRate_ValueChanged);
			// 
			// panelOptions
			// 
			this.panelOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.panelOptions.Controls.Add(this.numericPulseRate);
			this.panelOptions.Controls.Add(this.checkPulse);
			this.panelOptions.Controls.Add(this.labelClosePanel);
			this.panelOptions.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelOptions.Location = new System.Drawing.Point(0, 389);
			this.panelOptions.Name = "panelOptions";
			this.panelOptions.Size = new System.Drawing.Size(522, 4);
			this.panelOptions.TabIndex = 3;
			this.panelOptions.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelOptions_MouseDown);
			this.panelOptions.MouseEnter += new System.EventHandler(this.panelOptions_MouseEnter);
			this.panelOptions.MouseLeave += new System.EventHandler(this.panelOptions_MouseLeave);
			// 
			// labelClosePanel
			// 
			this.labelClosePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelClosePanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.labelClosePanel.Font = new System.Drawing.Font("Marlett", 9.75F);
			this.labelClosePanel.ForeColor = System.Drawing.Color.Silver;
			this.labelClosePanel.Location = new System.Drawing.Point(502, 0);
			this.labelClosePanel.Name = "labelClosePanel";
			this.labelClosePanel.Size = new System.Drawing.Size(20, 4);
			this.labelClosePanel.TabIndex = 3;
			this.labelClosePanel.Text = "r";
			this.labelClosePanel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelClosePanel.Visible = false;
			this.labelClosePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelClosePanel_MouseDown);
			this.labelClosePanel.MouseEnter += new System.EventHandler(this.labelClosePanel_MouseEnter);
			this.labelClosePanel.MouseLeave += new System.EventHandler(this.labelClosePanel_MouseLeave);
			this.labelClosePanel.MouseHover += new System.EventHandler(this.labelClosePanel_MouseEnter);
			this.labelClosePanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelClosePanel_MouseMove);
			// 
			// DefaultContentPanel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionVisible = false;
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "DefaultContentPanel";
			this.Size = new System.Drawing.Size(522, 393);
			this.PanelDisplay.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericPulseRate)).EndInit();
			this.panelOptions.ResumeLayout(false);
			this.panelOptions.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelOptions;
		private System.Windows.Forms.Label labelClosePanel;
		private System.Windows.Forms.CheckBox checkPulse;
		private System.Windows.Forms.NumericUpDown numericPulseRate;
	}
}
