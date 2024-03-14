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
        this.labelMessage = new System.Windows.Forms.Label();
        this.panelControllers = new System.Windows.Forms.Panel();
        this.panelController0 = new System.Windows.Forms.Panel();
        this.labelController0 = new System.Windows.Forms.Label();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.panelController1 = new System.Windows.Forms.Panel();
        this.labelController1 = new System.Windows.Forms.Label();
        this.pictureBox2 = new System.Windows.Forms.PictureBox();
        this.panelController2 = new System.Windows.Forms.Panel();
        this.labelController2 = new System.Windows.Forms.Label();
        this.pictureBox3 = new System.Windows.Forms.PictureBox();
        this.panelController3 = new System.Windows.Forms.Panel();
        this.labelController3 = new System.Windows.Forms.Label();
        this.pictureBox4 = new System.Windows.Forms.PictureBox();
        this.panelDisplay.SuspendLayout();
        this.panelControllers.SuspendLayout();
        this.panelController0.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.panelController1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
        this.panelController2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
        this.panelController3.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
        this.SuspendLayout();
        // 
        // panelDisplay
        // 
        this.panelDisplay.Controls.Add(this.labelMessage);
        this.panelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelDisplay.Location = new System.Drawing.Point(0, 0);
        this.panelDisplay.Name = "panelDisplay";
        this.panelDisplay.Size = new System.Drawing.Size(1262, 581);
        this.panelDisplay.TabIndex = 0;
        // 
        // labelMessage
        // 
        this.labelMessage.BackColor = System.Drawing.Color.White;
        this.labelMessage.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelMessage.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelMessage.Location = new System.Drawing.Point(0, 0);
        this.labelMessage.Name = "labelMessage";
        this.labelMessage.Size = new System.Drawing.Size(1262, 581);
        this.labelMessage.TabIndex = 0;
        this.labelMessage.Text = "No XInput controllers are connected.\r\n\r\nPlease connect an XInput controller to us" +
"e this application.";
        this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.labelMessage.Visible = false;
        // 
        // panelControllers
        // 
        this.panelControllers.AutoSize = true;
        this.panelControllers.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.panelControllers.Controls.Add(this.panelController0);
        this.panelControllers.Controls.Add(this.panelController1);
        this.panelControllers.Controls.Add(this.panelController2);
        this.panelControllers.Controls.Add(this.panelController3);
        this.panelControllers.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelControllers.Location = new System.Drawing.Point(0, 581);
        this.panelControllers.Name = "panelControllers";
        this.panelControllers.Size = new System.Drawing.Size(1262, 92);
        this.panelControllers.TabIndex = 1;
        // 
        // panelController0
        // 
        this.panelController0.BackColor = System.Drawing.SystemColors.Info;
        this.panelController0.Controls.Add(this.labelController0);
        this.panelController0.Controls.Add(this.pictureBox1);
        this.panelController0.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelController0.Location = new System.Drawing.Point(0, 0);
        this.panelController0.Name = "panelController0";
        this.panelController0.Size = new System.Drawing.Size(1262, 23);
        this.panelController0.TabIndex = 2;
        this.panelController0.Visible = false;
        // 
        // labelController0
        // 
        this.labelController0.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelController0.Location = new System.Drawing.Point(23, 0);
        this.labelController0.Name = "labelController0";
        this.labelController0.Size = new System.Drawing.Size(1239, 23);
        this.labelController0.TabIndex = 0;
        this.labelController0.Text = "XBox Controller";
        this.labelController0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.labelController0.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // pictureBox1
        // 
        this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
        this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
        this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(23, 23);
        this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox1.TabIndex = 1;
        this.pictureBox1.TabStop = false;
        this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // panelController1
        // 
        this.panelController1.BackColor = System.Drawing.SystemColors.Info;
        this.panelController1.Controls.Add(this.labelController1);
        this.panelController1.Controls.Add(this.pictureBox2);
        this.panelController1.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelController1.Location = new System.Drawing.Point(0, 23);
        this.panelController1.Name = "panelController1";
        this.panelController1.Size = new System.Drawing.Size(1262, 23);
        this.panelController1.TabIndex = 3;
        this.panelController1.Visible = false;
        // 
        // labelController1
        // 
        this.labelController1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelController1.Location = new System.Drawing.Point(23, 0);
        this.labelController1.Name = "labelController1";
        this.labelController1.Size = new System.Drawing.Size(1239, 23);
        this.labelController1.TabIndex = 0;
        this.labelController1.Text = "XBox Controller";
        this.labelController1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.labelController1.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // pictureBox2
        // 
        this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Left;
        this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
        this.pictureBox2.Location = new System.Drawing.Point(0, 0);
        this.pictureBox2.Name = "pictureBox2";
        this.pictureBox2.Size = new System.Drawing.Size(23, 23);
        this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox2.TabIndex = 1;
        this.pictureBox2.TabStop = false;
        this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // panelController2
        // 
        this.panelController2.BackColor = System.Drawing.SystemColors.Info;
        this.panelController2.Controls.Add(this.labelController2);
        this.panelController2.Controls.Add(this.pictureBox3);
        this.panelController2.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelController2.Location = new System.Drawing.Point(0, 46);
        this.panelController2.Name = "panelController2";
        this.panelController2.Size = new System.Drawing.Size(1262, 23);
        this.panelController2.TabIndex = 4;
        this.panelController2.Visible = false;
        // 
        // labelController2
        // 
        this.labelController2.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelController2.Location = new System.Drawing.Point(23, 0);
        this.labelController2.Name = "labelController2";
        this.labelController2.Size = new System.Drawing.Size(1239, 23);
        this.labelController2.TabIndex = 0;
        this.labelController2.Text = "XBox Controller";
        this.labelController2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.labelController2.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // pictureBox3
        // 
        this.pictureBox3.Dock = System.Windows.Forms.DockStyle.Left;
        this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
        this.pictureBox3.Location = new System.Drawing.Point(0, 0);
        this.pictureBox3.Name = "pictureBox3";
        this.pictureBox3.Size = new System.Drawing.Size(23, 23);
        this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox3.TabIndex = 1;
        this.pictureBox3.TabStop = false;
        this.pictureBox3.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // panelController3
        // 
        this.panelController3.BackColor = System.Drawing.SystemColors.Info;
        this.panelController3.Controls.Add(this.labelController3);
        this.panelController3.Controls.Add(this.pictureBox4);
        this.panelController3.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelController3.Location = new System.Drawing.Point(0, 69);
        this.panelController3.Name = "panelController3";
        this.panelController3.Size = new System.Drawing.Size(1262, 23);
        this.panelController3.TabIndex = 5;
        this.panelController3.Visible = false;
        // 
        // labelController3
        // 
        this.labelController3.Dock = System.Windows.Forms.DockStyle.Fill;
        this.labelController3.Location = new System.Drawing.Point(23, 0);
        this.labelController3.Name = "labelController3";
        this.labelController3.Size = new System.Drawing.Size(1239, 23);
        this.labelController3.TabIndex = 0;
        this.labelController3.Text = "XBox Controller";
        this.labelController3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.labelController3.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // pictureBox4
        // 
        this.pictureBox4.Dock = System.Windows.Forms.DockStyle.Left;
        this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
        this.pictureBox4.Location = new System.Drawing.Point(0, 0);
        this.pictureBox4.Name = "pictureBox4";
        this.pictureBox4.Size = new System.Drawing.Size(23, 23);
        this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox4.TabIndex = 1;
        this.pictureBox4.TabStop = false;
        this.pictureBox4.Paint += new System.Windows.Forms.PaintEventHandler(this.ControllerControlsPaint);
        // 
        // Form
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1262, 673);
        this.Controls.Add(this.panelDisplay);
        this.Controls.Add(this.panelControllers);
        
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "Form";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Gorgon Example - XInput";
        this.panelDisplay.ResumeLayout(false);
        this.panelControllers.ResumeLayout(false);
        this.panelController0.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.panelController1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
        this.panelController2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
        this.panelController3.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

        }

        #endregion

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

