﻿using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples
{
	partial class formMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
            this.SuspendLayout();
            // 
            // labelFPS
            // 
            this.labelFPS.BackColor = System.Drawing.Color.Black;
            this.labelFPS.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelFPS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.labelFPS.Location = new System.Drawing.Point(0, 0);
            this.labelFPS.Name = "labelFPS";
            this.labelFPS.Size = new System.Drawing.Size(550, 13);
            this.labelFPS.TabIndex = 0;
            this.labelFPS.Text = "FPS: 0";
            // 
            // panelGraphics
            // 
            this.panelGraphics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGraphics.Location = new System.Drawing.Point(0, 13);
            this.panelGraphics.Name = "panelGraphics";
            this.panelGraphics.Size = new System.Drawing.Size(550, 253);
            this.panelGraphics.TabIndex = 1;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 266);
            this.Controls.Add(this.panelGraphics);
            this.Controls.Add(this.labelFPS);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "formMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gorgon.Common Example #2 - Idle loop.";
            this.ResumeLayout(false);

		}

		#endregion

		private Label labelFPS;
		private Panel panelGraphics;
	}
}

