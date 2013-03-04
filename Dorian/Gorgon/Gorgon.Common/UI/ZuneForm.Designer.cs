namespace GorgonLibrary.UI
{
	partial class ZuneForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZuneForm));
			this.labelClose = new System.Windows.Forms.Label();
			this.labelMaxRestore = new System.Windows.Forms.Label();
			this.labelMinimize = new System.Windows.Forms.Label();
			this.labelCaption = new System.Windows.Forms.Label();
			this.panelCaptionArea = new System.Windows.Forms.Panel();
			this.panelCaptionArea.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelClose
			// 
			this.labelClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelClose.Font = new System.Drawing.Font("Marlett", 11.25F);
			this.labelClose.Location = new System.Drawing.Point(460, 5);
			this.labelClose.Name = "labelClose";
			this.labelClose.Size = new System.Drawing.Size(22, 15);
			this.labelClose.TabIndex = 0;
			this.labelClose.Text = "r";
			this.labelClose.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelClose_MouseDown);
			this.labelClose.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelClose.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelClose.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			// 
			// labelMaxRestore
			// 
			this.labelMaxRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelMaxRestore.Font = new System.Drawing.Font("Marlett", 11.25F);
			this.labelMaxRestore.Location = new System.Drawing.Point(432, 5);
			this.labelMaxRestore.Name = "labelMaxRestore";
			this.labelMaxRestore.Size = new System.Drawing.Size(22, 15);
			this.labelMaxRestore.TabIndex = 1;
			this.labelMaxRestore.Text = "2";
			this.labelMaxRestore.Click += new System.EventHandler(this.labelMaxRestore_Click);
			this.labelMaxRestore.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelMaxRestore.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelMaxRestore.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			// 
			// labelMinimize
			// 
			this.labelMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelMinimize.Font = new System.Drawing.Font("Marlett", 11.25F);
			this.labelMinimize.Location = new System.Drawing.Point(404, 5);
			this.labelMinimize.Name = "labelMinimize";
			this.labelMinimize.Size = new System.Drawing.Size(22, 15);
			this.labelMinimize.TabIndex = 2;
			this.labelMinimize.Text = "0";
			this.labelMinimize.Click += new System.EventHandler(this.labelMinimize_Click);
			this.labelMinimize.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelMinimize.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelMinimize.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			// 
			// labelCaption
			// 
			this.labelCaption.AutoSize = true;
			this.labelCaption.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCaption.Location = new System.Drawing.Point(0, 0);
			this.labelCaption.Name = "labelCaption";
			this.labelCaption.Size = new System.Drawing.Size(61, 20);
			this.labelCaption.TabIndex = 3;
			this.labelCaption.Text = "Caption";
			this.labelCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseDown);
			this.labelCaption.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseMove);
			// 
			// panelCaptionArea
			// 
			this.panelCaptionArea.Controls.Add(this.labelCaption);
			this.panelCaptionArea.Controls.Add(this.labelMinimize);
			this.panelCaptionArea.Controls.Add(this.labelMaxRestore);
			this.panelCaptionArea.Controls.Add(this.labelClose);
			this.panelCaptionArea.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelCaptionArea.Location = new System.Drawing.Point(0, 0);
			this.panelCaptionArea.Name = "panelCaptionArea";
			this.panelCaptionArea.Size = new System.Drawing.Size(487, 24);
			this.panelCaptionArea.TabIndex = 4;
			this.panelCaptionArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseDown);
			this.panelCaptionArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseMove);
			// 
			// ZuneForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(487, 456);
			this.Controls.Add(this.panelCaptionArea);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "ZuneForm";
			this.Text = "Form";
			this.panelCaptionArea.ResumeLayout(false);
			this.panelCaptionArea.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelClose;
		private System.Windows.Forms.Label labelMaxRestore;
		private System.Windows.Forms.Label labelMinimize;
		private System.Windows.Forms.Label labelCaption;
		private System.Windows.Forms.Panel panelCaptionArea;
	}
}