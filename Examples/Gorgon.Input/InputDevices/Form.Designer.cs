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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
        this.panelDisplay = new System.Windows.Forms.Panel();
        this.panel1 = new System.Windows.Forms.Panel();
        this.panelMouse = new System.Windows.Forms.Panel();
        this.labelMouse = new System.Windows.Forms.Label();
        this.pictureBox3 = new System.Windows.Forms.PictureBox();
        this.panelKeyboard = new System.Windows.Forms.Panel();
        this.labelKeyboard = new System.Windows.Forms.Label();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.panelJoystick = new System.Windows.Forms.Panel();
        this.labelJoystick = new System.Windows.Forms.Label();
        this.pictureBox2 = new System.Windows.Forms.PictureBox();
        this.panel1.SuspendLayout();
        this.panelMouse.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
        this.panelKeyboard.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.panelJoystick.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
        this.SuspendLayout();
        // 
        // panelDisplay
        // 
        this.panelDisplay.BackColor = System.Drawing.Color.White;
        this.panelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelDisplay.Location = new System.Drawing.Point(0, 0);
        this.panelDisplay.Name = "panelDisplay";
        this.panelDisplay.Size = new System.Drawing.Size(1262, 604);
        this.panelDisplay.TabIndex = 1;
        // 
        // panel1
        // 
        this.panel1.AutoSize = true;
        this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.panel1.Controls.Add(this.panelMouse);
        this.panel1.Controls.Add(this.panelKeyboard);
        this.panel1.Controls.Add(this.panelJoystick);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panel1.Location = new System.Drawing.Point(0, 604);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(1262, 69);
        this.panel1.TabIndex = 0;
        // 
        // panelMouse
        // 
        this.panelMouse.Controls.Add(this.labelMouse);
        this.panelMouse.Controls.Add(this.pictureBox3);
        this.panelMouse.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelMouse.Location = new System.Drawing.Point(0, 0);
        this.panelMouse.Name = "panelMouse";
        this.panelMouse.Size = new System.Drawing.Size(1262, 23);
        this.panelMouse.TabIndex = 6;
        // 
        // labelMouse
        // 
        this.labelMouse.BackColor = System.Drawing.SystemColors.Info;
        this.labelMouse.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelMouse.Location = new System.Drawing.Point(26, 0);
        this.labelMouse.Name = "labelMouse";
        this.labelMouse.Size = new System.Drawing.Size(1236, 23);
        this.labelMouse.TabIndex = 0;
        this.labelMouse.Text = "Mouse";
        this.labelMouse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.labelMouse.Paint += new System.Windows.Forms.PaintEventHandler(this.DevicePanelsPaint);
        // 
        // pictureBox3
        // 
        this.pictureBox3.BackColor = System.Drawing.SystemColors.Info;
        this.pictureBox3.Dock = System.Windows.Forms.DockStyle.Left;
        this.pictureBox3.Image = global::Gorgon.Examples.Properties.Resources.device_mouse_16x16;
        this.pictureBox3.Location = new System.Drawing.Point(0, 0);
        this.pictureBox3.Name = "pictureBox3";
        this.pictureBox3.Size = new System.Drawing.Size(26, 23);
        this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox3.TabIndex = 1;
        this.pictureBox3.TabStop = false;
        this.pictureBox3.Paint += new System.Windows.Forms.PaintEventHandler(this.DevicePanelsPaint);
        // 
        // panelKeyboard
        // 
        this.panelKeyboard.Controls.Add(this.labelKeyboard);
        this.panelKeyboard.Controls.Add(this.pictureBox1);
        this.panelKeyboard.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelKeyboard.Location = new System.Drawing.Point(0, 23);
        this.panelKeyboard.Name = "panelKeyboard";
        this.panelKeyboard.Size = new System.Drawing.Size(1262, 23);
        this.panelKeyboard.TabIndex = 5;
        // 
        // labelKeyboard
        // 
        this.labelKeyboard.BackColor = System.Drawing.SystemColors.Info;
        this.labelKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelKeyboard.Location = new System.Drawing.Point(26, 0);
        this.labelKeyboard.Name = "labelKeyboard";
        this.labelKeyboard.Size = new System.Drawing.Size(1236, 23);
        this.labelKeyboard.TabIndex = 0;
        this.labelKeyboard.Text = "Keyboard";
        this.labelKeyboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // pictureBox1
        // 
        this.pictureBox1.BackColor = System.Drawing.SystemColors.Info;
        this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
        this.pictureBox1.Image = global::Gorgon.Examples.Properties.Resources.device_keyboard_16x16;
        this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(26, 23);
        this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox1.TabIndex = 1;
        this.pictureBox1.TabStop = false;
        // 
        // panelJoystick
        // 
        this.panelJoystick.Controls.Add(this.labelJoystick);
        this.panelJoystick.Controls.Add(this.pictureBox2);
        this.panelJoystick.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelJoystick.Location = new System.Drawing.Point(0, 46);
        this.panelJoystick.Name = "panelJoystick";
        this.panelJoystick.Size = new System.Drawing.Size(1262, 23);
        this.panelJoystick.TabIndex = 4;
        this.panelJoystick.Visible = false;
        // 
        // labelJoystick
        // 
        this.labelJoystick.BackColor = System.Drawing.SystemColors.Info;
        this.labelJoystick.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelJoystick.Location = new System.Drawing.Point(26, 0);
        this.labelJoystick.Name = "labelJoystick";
        this.labelJoystick.Size = new System.Drawing.Size(1236, 23);
        this.labelJoystick.TabIndex = 2;
        this.labelJoystick.Text = "Joystick";
        this.labelJoystick.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // pictureBox2
        // 
        this.pictureBox2.BackColor = System.Drawing.SystemColors.Info;
        this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Left;
        this.pictureBox2.Image = global::Gorgon.Examples.Properties.Resources.device_gamepad_16x16;
        this.pictureBox2.Location = new System.Drawing.Point(0, 0);
        this.pictureBox2.Name = "pictureBox2";
        this.pictureBox2.Size = new System.Drawing.Size(26, 23);
        this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox2.TabIndex = 3;
        this.pictureBox2.TabStop = false;
        // 
        // Form
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1262, 673);
        this.Controls.Add(this.panelDisplay);
        this.Controls.Add(this.panel1);
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "Form";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Gorgon Example - Input Devices";
        this.panel1.ResumeLayout(false);
        this.panelMouse.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
        this.panelKeyboard.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.panelJoystick.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

		}

		#endregion

    private Panel panelDisplay;
		private Panel panel1;
		private Label labelKeyboard;
    private PictureBox pictureBox1;
    private Panel panelKeyboard;
    private Panel panelJoystick;
    private Label labelJoystick;
    private PictureBox pictureBox2;
    private Panel panelMouse;
    private Label labelMouse;
    private PictureBox pictureBox3;
	}

