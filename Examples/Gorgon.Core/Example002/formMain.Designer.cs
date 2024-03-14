using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples;

    partial class FormMain
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



        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
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
        this.labelFPS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.labelFPS.Name = "labelFPS";
        this.labelFPS.Size = new System.Drawing.Size(733, 16);
        this.labelFPS.TabIndex = 0;
        this.labelFPS.Text = "FPS: 0";
        // 
        // panelGraphics
        // 
        this.panelGraphics.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelGraphics.Location = new System.Drawing.Point(0, 16);
        this.panelGraphics.Margin = new System.Windows.Forms.Padding(4);
        this.panelGraphics.Name = "panelGraphics";
        this.panelGraphics.Size = new System.Drawing.Size(733, 311);
        this.panelGraphics.TabIndex = 1;
        // 
        // formMain
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(733, 327);
        this.Controls.Add(this.panelGraphics);
        this.Controls.Add(this.labelFPS);
        this.DoubleBuffered = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Margin = new System.Windows.Forms.Padding(4);
        this.Name = "formMain";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Gorgon Example #2 - Idle loop.";
        this.ResumeLayout(false);

        }

    

        private Label labelFPS;
        private Panel panelGraphics;
    }

