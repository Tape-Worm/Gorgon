using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples;

partial class Splash
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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Splash));
        labelText = new Label();
        pictureBox1 = new PictureBox();
        tableLayoutPanel1 = new TableLayoutPanel();
        ((ISupportInitialize)pictureBox1).BeginInit();
        tableLayoutPanel1.SuspendLayout();
        SuspendLayout();
        // 
        // labelText
        // 
        labelText.Anchor = AnchorStyles.Left;
        labelText.AutoSize = true;
        labelText.Location = new System.Drawing.Point(3, 125);
        labelText.Name = "labelText";
        labelText.Size = new System.Drawing.Size(171, 15);
        labelText.TabIndex = 2;
        labelText.Text = "Here's some splash screen text!";
        labelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // pictureBox1
        // 
        pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pictureBox1.Image = Properties.Resources.Gorgon_2_x_Logo_Full;
        pictureBox1.Location = new System.Drawing.Point(0, 0);
        pictureBox1.Margin = new Padding(0);
        pictureBox1.MaximumSize = new System.Drawing.Size(620, 124);
        pictureBox1.MinimumSize = new System.Drawing.Size(620, 124);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new System.Drawing.Size(620, 124);
        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox1.TabIndex = 0;
        pictureBox1.TabStop = false;
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.AutoSize = true;
        tableLayoutPanel1.ColumnCount = 1;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
        tableLayoutPanel1.Controls.Add(labelText, 0, 1);
        tableLayoutPanel1.Dock = DockStyle.Fill;
        tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 2;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Size = new System.Drawing.Size(620, 142);
        tableLayoutPanel1.TabIndex = 3;
        // 
        // FormSplash
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(620, 142);
        ControlBox = false;
        Controls.Add(tableLayoutPanel1);
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
        ((ISupportInitialize)pictureBox1).EndInit();
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label labelText;
    private PictureBox pictureBox1;
    private TableLayoutPanel tableLayoutPanel1;
}