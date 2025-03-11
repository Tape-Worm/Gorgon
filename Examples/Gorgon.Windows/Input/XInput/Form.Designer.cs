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
        panelDisplay = new Panel();
        labelMessage = new Label();
        panelControllers = new Panel();
        panelController0 = new Panel();
        labelController0 = new Label();
        pictureBox1 = new PictureBox();
        panelController1 = new Panel();
        labelController1 = new Label();
        pictureBox2 = new PictureBox();
        panelController2 = new Panel();
        labelController2 = new Label();
        pictureBox3 = new PictureBox();
        panelController3 = new Panel();
        labelController3 = new Label();
        pictureBox4 = new PictureBox();
        panelDisplay.SuspendLayout();
        panelControllers.SuspendLayout();
        panelController0.SuspendLayout();
        ((ISupportInitialize)pictureBox1).BeginInit();
        panelController1.SuspendLayout();
        ((ISupportInitialize)pictureBox2).BeginInit();
        panelController2.SuspendLayout();
        ((ISupportInitialize)pictureBox3).BeginInit();
        panelController3.SuspendLayout();
        ((ISupportInitialize)pictureBox4).BeginInit();
        SuspendLayout();
        // 
        // panelDisplay
        // 
        panelDisplay.Controls.Add(labelMessage);
        panelDisplay.Dock = DockStyle.Fill;
        panelDisplay.Location = new Point(0, 0);
        panelDisplay.Name = "panelDisplay";
        panelDisplay.Size = new Size(1262, 581);
        panelDisplay.TabIndex = 0;
        // 
        // labelMessage
        // 
        labelMessage.BackColor = Color.White;
        labelMessage.Dock = DockStyle.Fill;
        labelMessage.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelMessage.Location = new Point(0, 0);
        labelMessage.Name = "labelMessage";
        labelMessage.Size = new Size(1262, 581);
        labelMessage.TabIndex = 0;
        labelMessage.Text = "No XInput controllers are connected.\r\n\r\nPlease connect an XInput controller to use this application.";
        labelMessage.TextAlign = ContentAlignment.MiddleCenter;
        labelMessage.Visible = false;
        // 
        // panelControllers
        // 
        panelControllers.AutoSize = true;
        panelControllers.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        panelControllers.BackColor = Color.FromArgb(30, 30, 30);
        panelControllers.Controls.Add(panelController0);
        panelControllers.Controls.Add(panelController1);
        panelControllers.Controls.Add(panelController2);
        panelControllers.Controls.Add(panelController3);
        panelControllers.Dock = DockStyle.Bottom;
        panelControllers.ForeColor = Color.White;
        panelControllers.Location = new Point(0, 581);
        panelControllers.Name = "panelControllers";
        panelControllers.Size = new Size(1262, 92);
        panelControllers.TabIndex = 1;
        // 
        // panelController0
        // 
        panelController0.Controls.Add(labelController0);
        panelController0.Controls.Add(pictureBox1);
        panelController0.Dock = DockStyle.Bottom;
        panelController0.Location = new Point(0, 0);
        panelController0.Name = "panelController0";
        panelController0.Size = new Size(1262, 23);
        panelController0.TabIndex = 2;
        panelController0.Visible = false;
        // 
        // labelController0
        // 
        labelController0.Dock = DockStyle.Fill;
        labelController0.Location = new Point(23, 0);
        labelController0.Name = "labelController0";
        labelController0.Size = new Size(1239, 23);
        labelController0.TabIndex = 0;
        labelController0.Text = "XBox Controller";
        labelController0.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pictureBox1
        // 
        pictureBox1.Dock = DockStyle.Left;
        pictureBox1.Image = Properties.Resources.device_gamepad_16x16;
        pictureBox1.Location = new Point(0, 0);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(23, 23);
        pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox1.TabIndex = 1;
        pictureBox1.TabStop = false;
        // 
        // panelController1
        // 
        panelController1.Controls.Add(labelController1);
        panelController1.Controls.Add(pictureBox2);
        panelController1.Dock = DockStyle.Bottom;
        panelController1.Location = new Point(0, 23);
        panelController1.Name = "panelController1";
        panelController1.Size = new Size(1262, 23);
        panelController1.TabIndex = 3;
        panelController1.Visible = false;
        // 
        // labelController1
        // 
        labelController1.Dock = DockStyle.Fill;
        labelController1.Location = new Point(23, 0);
        labelController1.Name = "labelController1";
        labelController1.Size = new Size(1239, 23);
        labelController1.TabIndex = 0;
        labelController1.Text = "XBox Controller";
        labelController1.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pictureBox2
        // 
        pictureBox2.Dock = DockStyle.Left;
        pictureBox2.Image = Properties.Resources.device_gamepad_16x16;
        pictureBox2.Location = new Point(0, 0);
        pictureBox2.Name = "pictureBox2";
        pictureBox2.Size = new Size(23, 23);
        pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox2.TabIndex = 1;
        pictureBox2.TabStop = false;
        // 
        // panelController2
        // 
        panelController2.Controls.Add(labelController2);
        panelController2.Controls.Add(pictureBox3);
        panelController2.Dock = DockStyle.Bottom;
        panelController2.Location = new Point(0, 46);
        panelController2.Name = "panelController2";
        panelController2.Size = new Size(1262, 23);
        panelController2.TabIndex = 4;
        panelController2.Visible = false;
        // 
        // labelController2
        // 
        labelController2.Dock = DockStyle.Fill;
        labelController2.Location = new Point(23, 0);
        labelController2.Name = "labelController2";
        labelController2.Size = new Size(1239, 23);
        labelController2.TabIndex = 0;
        labelController2.Text = "XBox Controller";
        labelController2.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pictureBox3
        // 
        pictureBox3.Dock = DockStyle.Left;
        pictureBox3.Image = Properties.Resources.device_gamepad_16x16;
        pictureBox3.Location = new Point(0, 0);
        pictureBox3.Name = "pictureBox3";
        pictureBox3.Size = new Size(23, 23);
        pictureBox3.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox3.TabIndex = 1;
        pictureBox3.TabStop = false;
        // 
        // panelController3
        // 
        panelController3.Controls.Add(labelController3);
        panelController3.Controls.Add(pictureBox4);
        panelController3.Dock = DockStyle.Bottom;
        panelController3.Location = new Point(0, 69);
        panelController3.Name = "panelController3";
        panelController3.Size = new Size(1262, 23);
        panelController3.TabIndex = 5;
        panelController3.Visible = false;
        // 
        // labelController3
        // 
        labelController3.Dock = DockStyle.Fill;
        labelController3.Location = new Point(23, 0);
        labelController3.Name = "labelController3";
        labelController3.Size = new Size(1239, 23);
        labelController3.TabIndex = 0;
        labelController3.Text = "XBox Controller";
        labelController3.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pictureBox4
        // 
        pictureBox4.Dock = DockStyle.Left;
        pictureBox4.Image = Properties.Resources.device_gamepad_16x16;
        pictureBox4.Location = new Point(0, 0);
        pictureBox4.Name = "pictureBox4";
        pictureBox4.Size = new Size(23, 23);
        pictureBox4.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox4.TabIndex = 1;
        pictureBox4.TabStop = false;
        // 
        // Form
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1262, 673);
        Controls.Add(panelDisplay);
        Controls.Add(panelControllers);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "Form";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Example - XInput";
        KeyUp += Form_KeyUp;
        panelDisplay.ResumeLayout(false);
        panelControllers.ResumeLayout(false);
        panelController0.ResumeLayout(false);
        ((ISupportInitialize)pictureBox1).EndInit();
        panelController1.ResumeLayout(false);
        ((ISupportInitialize)pictureBox2).EndInit();
        panelController2.ResumeLayout(false);
        ((ISupportInitialize)pictureBox3).EndInit();
        panelController3.ResumeLayout(false);
        ((ISupportInitialize)pictureBox4).EndInit();
        ResumeLayout(false);
        PerformLayout();

    }

    private Panel panelDisplay;
    private Panel panelControllers;
    private Label labelController0;
    private Panel panelController0;
    private PictureBox pictureBox1;
    private Panel panelController1;
    private Label labelController1;
    private PictureBox pictureBox2;
    private Panel panelController2;
    private Label labelController2;
    private PictureBox pictureBox3;
    private Panel panelController3;
    private Label labelController3;
    private PictureBox pictureBox4;
    private Label labelMessage;
}