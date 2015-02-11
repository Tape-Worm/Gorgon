namespace GorgonLibrary.Editor
{
	partial class FormSplash
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSplash));
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelVersion = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackgroundImage = global::GorgonLibrary.Editor.Properties.Resources.Gorgon_2_Logo_Full;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.labelVersion);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(622, 150);
			this.panel1.TabIndex = 2;
			// 
			// labelVersion
			// 
			this.labelVersion.BackColor = System.Drawing.Color.White;
			this.labelVersion.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.labelVersion.Location = new System.Drawing.Point(0, 125);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(620, 23);
			this.labelVersion.TabIndex = 2;
			this.labelVersion.Text = "Version: ";
			this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// formSplash
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(622, 150);
			this.ControlBox = false;
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formSplash";
			this.Opacity = 0.02D;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Gorgon Editor";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelVersion;

	}
}