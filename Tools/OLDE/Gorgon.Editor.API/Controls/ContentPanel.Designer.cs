namespace Gorgon.Editor
{
	partial class ContentPanel
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

			if (disposing)
			{
				if (RawInput != null)
				{
					RawInput.Dispose();
					RawInput = null;
				}
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
			this.panelCaption = new System.Windows.Forms.Panel();
			this._panelContentDisplay = new System.Windows.Forms.Panel();
			this.labelClose = new System.Windows.Forms.Label();
			this.labelCaption = new System.Windows.Forms.Label();
			this.panelCaption.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelCaption
			// 
			this.panelCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(88)))), ((int)(((byte)(88)))));
			this.panelCaption.Controls.Add(this.labelCaption);
			this.panelCaption.Controls.Add(this.labelClose);
			this.panelCaption.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelCaption.ForeColor = System.Drawing.Color.White;
			this.panelCaption.Location = new System.Drawing.Point(0, 0);
			this.panelCaption.Name = "panelCaption";
			this.panelCaption.Size = new System.Drawing.Size(572, 30);
			this.panelCaption.TabIndex = 0;
			// 
			// panelContentDisplay
			// 
			this._panelContentDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this._panelContentDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this._panelContentDisplay.Location = new System.Drawing.Point(0, 30);
			this._panelContentDisplay.Name = "panelContentDisplay";
			this._panelContentDisplay.Size = new System.Drawing.Size(572, 425);
			this._panelContentDisplay.TabIndex = 1;
			// 
			// labelClose
			// 
			this.labelClose.Dock = System.Windows.Forms.DockStyle.Right;
			this.labelClose.Font = new System.Drawing.Font("Marlett", 11.25F);
			this.labelClose.ForeColor = System.Drawing.Color.Silver;
			this.labelClose.Location = new System.Drawing.Point(547, 0);
			this.labelClose.Name = "labelClose";
			this.labelClose.Size = new System.Drawing.Size(25, 30);
			this.labelClose.TabIndex = 0;
			this.labelClose.Text = "r";
			this.labelClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelClose.Click += new System.EventHandler(this.labelClose_Click);
			this.labelClose.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelClose.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelClose.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelClose.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelClose_MouseMove);
			// 
			// labelCaption
			// 
			this.labelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelCaption.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCaption.Location = new System.Drawing.Point(0, 0);
			this.labelCaption.Name = "labelCaption";
			this.labelCaption.Size = new System.Drawing.Size(547, 30);
			this.labelCaption.TabIndex = 1;
			this.labelCaption.Text = "Content Caption";
			this.labelCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ContentPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Controls.Add(this._panelContentDisplay);
			this.Controls.Add(this.panelCaption);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ContentPanel";
			this.Size = new System.Drawing.Size(572, 455);
			this.panelCaption.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelCaption;
		[System.Runtime.CompilerServices.AccessedThroughProperty("PanelDisplay")]
		private System.Windows.Forms.Panel _panelContentDisplay;
		private System.Windows.Forms.Label labelCaption;
		private System.Windows.Forms.Label labelClose;
	}
}
