namespace Gorgon.Examples
{
	partial class ExamplePanel
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
			this.pictureIcon = new System.Windows.Forms.PictureBox();
			this.panelText = new System.Windows.Forms.Panel();
			this.labelText = new System.Windows.Forms.Label();
			this.labelCaption = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).BeginInit();
			this.panelText.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureIcon
			// 
			this.pictureIcon.BackColor = System.Drawing.Color.White;
			this.pictureIcon.Dock = System.Windows.Forms.DockStyle.Left;
			this.pictureIcon.Image = global::Gorgon.Examples.Properties.Resources.Default_128x128;
			this.pictureIcon.Location = new System.Drawing.Point(0, 0);
			this.pictureIcon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureIcon.Name = "pictureIcon";
			this.pictureIcon.Size = new System.Drawing.Size(130, 130);
			this.pictureIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureIcon.TabIndex = 0;
			this.pictureIcon.TabStop = false;
			this.pictureIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureIcon_MouseDown);
			this.pictureIcon.MouseEnter += new System.EventHandler(this.panelText_MouseEnter);
			this.pictureIcon.MouseLeave += new System.EventHandler(this.panelText_MouseLeave);
			this.pictureIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureIcon_MouseUp);
			// 
			// panelText
			// 
			this.panelText.BackColor = System.Drawing.Color.White;
			this.panelText.Controls.Add(this.labelText);
			this.panelText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelText.Location = new System.Drawing.Point(130, 0);
			this.panelText.Name = "panelText";
			this.panelText.Size = new System.Drawing.Size(445, 130);
			this.panelText.TabIndex = 1;
			this.panelText.MouseEnter += new System.EventHandler(this.panelText_MouseEnter);
			this.panelText.MouseLeave += new System.EventHandler(this.panelText_MouseLeave);
			// 
			// labelText
			// 
			this.labelText.AutoSize = true;
			this.labelText.BackColor = System.Drawing.Color.White;
			this.labelText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelText.Location = new System.Drawing.Point(0, 0);
			this.labelText.Margin = new System.Windows.Forms.Padding(0);
			this.labelText.MinimumSize = new System.Drawing.Size(0, 130);
			this.labelText.Name = "labelText";
			this.labelText.Size = new System.Drawing.Size(42, 130);
			this.labelText.TabIndex = 1;
			this.labelText.Text = "Text";
			this.labelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureIcon_MouseDown);
			this.labelText.MouseEnter += new System.EventHandler(this.panelText_MouseEnter);
			this.labelText.MouseLeave += new System.EventHandler(this.panelText_MouseLeave);
			this.labelText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureIcon_MouseUp);
			// 
			// labelCaption
			// 
			this.labelCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
			this.labelCaption.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelCaption.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCaption.Location = new System.Drawing.Point(0, 0);
			this.labelCaption.Name = "labelCaption";
			this.labelCaption.Size = new System.Drawing.Size(575, 21);
			this.labelCaption.TabIndex = 0;
			this.labelCaption.Text = "Caption";
			this.labelCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureIcon_MouseDown);
			this.labelCaption.MouseEnter += new System.EventHandler(this.panelText_MouseEnter);
			this.labelCaption.MouseLeave += new System.EventHandler(this.panelText_MouseLeave);
			this.labelCaption.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureIcon_MouseUp);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.pictureIcon);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(130, 130);
			this.panel1.TabIndex = 2;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.White;
			this.panel2.Controls.Add(this.panelText);
			this.panel2.Controls.Add(this.panel1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 21);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(575, 130);
			this.panel2.TabIndex = 2;
			// 
			// ExamplePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.labelCaption);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimumSize = new System.Drawing.Size(2, 151);
			this.Name = "ExamplePanel";
			this.Size = new System.Drawing.Size(575, 151);
			this.ParentChanged += new System.EventHandler(this.ExamplePanel_ParentChanged);
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
			this.panelText.ResumeLayout(false);
			this.panelText.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureIcon;
		private System.Windows.Forms.Panel panelText;
		private System.Windows.Forms.Label labelText;
		private System.Windows.Forms.Label labelCaption;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
	}
}
