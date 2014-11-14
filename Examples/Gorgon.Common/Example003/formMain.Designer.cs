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
            this.labelFPS = new System.Windows.Forms.Label();
            this.panelGraphics = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelFPS
            // 
            this.labelFPS.BackColor = System.Drawing.Color.Black;
            this.labelFPS.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelFPS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.labelFPS.Location = new System.Drawing.Point(0, 0);
            this.labelFPS.Name = "labelFPS";
            this.labelFPS.Size = new System.Drawing.Size(539, 15);
            this.labelFPS.TabIndex = 0;
            this.labelFPS.Text = "FPS: 0";
            // 
            // panelGraphics
            // 
            this.panelGraphics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGraphics.Location = new System.Drawing.Point(0, 15);
            this.panelGraphics.Name = "panelGraphics";
            this.panelGraphics.Size = new System.Drawing.Size(539, 216);
            this.panelGraphics.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Info;
            this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Image = global::GorgonLibrary.Examples.Properties.Resources.keyboardIcon;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(0, 231);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(539, 22);
            this.label1.TabIndex = 1;
            this.label1.Text = "         Press the space bar to change idle loops.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 253);
            this.Controls.Add(this.panelGraphics);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelFPS);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "formMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gorgon.Common Example #3 - Application Context.";
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelFPS;
		private System.Windows.Forms.Panel panelGraphics;
		private System.Windows.Forms.Label label1;
	}
}

