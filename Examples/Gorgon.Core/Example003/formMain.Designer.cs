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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(FormMain));
        labelFPS = new Label();
        panelGraphics = new Panel();
        label1 = new Label();
        panel1 = new Panel();
        panel1.SuspendLayout();
        SuspendLayout();
        // 
        // labelFPS
        // 
        labelFPS.AutoSize = true;
        labelFPS.Dock = DockStyle.Top;
        labelFPS.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
        labelFPS.Location = new System.Drawing.Point(0, 0);
        labelFPS.Name = "labelFPS";
        labelFPS.Size = new System.Drawing.Size(38, 15);
        labelFPS.TabIndex = 0;
        labelFPS.Text = "FPS: 0";
        // 
        // panelGraphics
        // 
        panelGraphics.Dock = DockStyle.Fill;
        panelGraphics.Location = new System.Drawing.Point(0, 15);
        panelGraphics.Name = "panelGraphics";
        panelGraphics.Size = new System.Drawing.Size(539, 216);
        panelGraphics.TabIndex = 1;
        // 
        // label1
        // 
        label1.BackColor = System.Drawing.SystemColors.Info;
        label1.Dock = DockStyle.Bottom;
        label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        label1.ForeColor = System.Drawing.Color.Black;
        label1.Image = Properties.Resources.keyboardIcon;
        label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        label1.Location = new System.Drawing.Point(0, 231);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(539, 22);
        label1.TabIndex = 1;
        label1.Text = "         Press the space bar to change idle loops.";
        label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // panel1
        // 
        panel1.AutoSize = true;
        panel1.BackColor = System.Drawing.Color.Black;
        panel1.Controls.Add(labelFPS);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new System.Drawing.Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new System.Drawing.Size(539, 15);
        panel1.TabIndex = 0;
        // 
        // FormMain
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(539, 253);
        Controls.Add(panelGraphics);
        Controls.Add(panel1);
        Controls.Add(label1);
        DoubleBuffered = true;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Name = "FormMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Example #3 - Application Context.";
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label labelFPS;
    private Panel panelGraphics;
    private Label label1;
    private Panel panel1;
}