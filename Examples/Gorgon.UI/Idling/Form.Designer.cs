using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples;

    partial class Form
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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form));
        labelFPS = new Label();
        panelGraphics = new Panel();
        SuspendLayout();
        // 
        // labelFPS
        // 
        labelFPS.Dock = DockStyle.Top;
        labelFPS.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
        labelFPS.Location = new System.Drawing.Point(0, 0);
        labelFPS.Margin = new Padding(4, 0, 4, 0);
        labelFPS.Name = "labelFPS";
        labelFPS.Size = new System.Drawing.Size(733, 16);
        labelFPS.TabIndex = 0;
        labelFPS.Text = "FPS: 0";
        // 
        // panelGraphics
        // 
        panelGraphics.Dock = DockStyle.Fill;
        panelGraphics.Location = new System.Drawing.Point(0, 16);
        panelGraphics.Margin = new Padding(4);
        panelGraphics.Name = "panelGraphics";
        panelGraphics.Size = new System.Drawing.Size(733, 311);
        panelGraphics.TabIndex = 1;
        // 
        // Form
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
        ClientSize = new System.Drawing.Size(733, 327);
        Controls.Add(panelGraphics);
        Controls.Add(labelFPS);
        DoubleBuffered = true;
        ForeColor = System.Drawing.Color.White;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Margin = new Padding(4);
        Name = "Form";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "The Idle Loop";
        ResumeLayout(false);

    }



    private Label labelFPS;
        private Panel panelGraphics;
    }

