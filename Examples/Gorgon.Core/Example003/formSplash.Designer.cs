using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples;

partial class FormSplash
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
        var resources = new ComponentResourceManager(typeof(FormSplash));
        panel1 = new Panel();
        labelText = new Label();
        SuspendLayout();
        // 
        // panel1
        // 
        panel1.BackgroundImage = Properties.Resources.Gorgon_2_x_Logo_Full;
        panel1.BackgroundImageLayout = ImageLayout.Stretch;
        panel1.BorderStyle = BorderStyle.FixedSingle;
        panel1.Dock = DockStyle.Fill;
        panel1.Location = new System.Drawing.Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new System.Drawing.Size(622, 123);
        panel1.TabIndex = 2;
        // 
        // labelText
        // 
        labelText.BackColor = System.Drawing.Color.White;
        labelText.Dock = DockStyle.Bottom;
        labelText.Location = new System.Drawing.Point(0, 123);
        labelText.Name = "labelText";
        labelText.Size = new System.Drawing.Size(622, 27);
        labelText.TabIndex = 2;
        labelText.Text = "Here's some splash screen text!";
        labelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // FormSplash
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        BackgroundImageLayout = ImageLayout.Center;
        ClientSize = new System.Drawing.Size(622, 150);
        ControlBox = false;
        Controls.Add(panel1);
        Controls.Add(labelText);
        FormBorderStyle = FormBorderStyle.None;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormSplash";
        Opacity = 0.02D;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Editor";
        ResumeLayout(false);
    }



    private Panel panel1;
    private Label labelText;

}