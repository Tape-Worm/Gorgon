namespace GorgonLibrary.Examples
{
	partial class formMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
			this.labelMouse = new System.Windows.Forms.Label();
			this.panelDisplay = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelKeyboard = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// labelMouse
			// 
			this.labelMouse.AutoSize = true;
			this.labelMouse.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelMouse.Location = new System.Drawing.Point(0, 0);
			this.labelMouse.Name = "labelMouse";
			this.labelMouse.Size = new System.Drawing.Size(10, 13);
			this.labelMouse.TabIndex = 0;
			this.labelMouse.Text = " ";
			// 
			// panelDisplay
			// 
			this.panelDisplay.BackColor = System.Drawing.Color.White;
			this.panelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelDisplay.Location = new System.Drawing.Point(0, 13);
			this.panelDisplay.Name = "panelDisplay";
			this.panelDisplay.Size = new System.Drawing.Size(778, 523);
			this.panelDisplay.TabIndex = 1;
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.labelKeyboard);
			this.panel1.Controls.Add(this.pictureBox1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 536);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(778, 18);
			this.panel1.TabIndex = 0;
			// 
			// labelKeyboard
			// 
			this.labelKeyboard.BackColor = System.Drawing.SystemColors.Info;
			this.labelKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelKeyboard.Location = new System.Drawing.Point(22, 0);
			this.labelKeyboard.Name = "labelKeyboard";
			this.labelKeyboard.Size = new System.Drawing.Size(754, 16);
			this.labelKeyboard.TabIndex = 0;
			this.labelKeyboard.Text = "Keyboard";
			this.labelKeyboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.SystemColors.Info;
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
			this.pictureBox1.Image = global::GorgonLibrary.Examples.Properties.Resources.keyboard_icon;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(22, 16);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// formMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(778, 554);
			this.Controls.Add(this.panelDisplay);
			this.Controls.Add(this.labelMouse);
			this.Controls.Add(this.panel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "formMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Gorgon Example - Input Devices";
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelMouse;
		private System.Windows.Forms.Panel panelDisplay;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelKeyboard;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}

