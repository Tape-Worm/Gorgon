using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor;

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
        tableLayoutPanel1 = new TableLayoutPanel();
        flowLayoutPanel1 = new FlowLayoutPanel();
        labelVersion = new Label();
        labelVersionNumber = new Label();
        labelInfo = new Label();
        pictureBox1 = new PictureBox();
        tableLayoutPanel1.SuspendLayout();
        flowLayoutPanel1.SuspendLayout();
        ((ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.AutoSize = true;
        tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayoutPanel1.ColumnCount = 1;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
        tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
        tableLayoutPanel1.Dock = DockStyle.Fill;
        tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanel1.Margin = new Padding(0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 2;
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Size = new System.Drawing.Size(621, 158);
        tableLayoutPanel1.TabIndex = 0;
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowLayoutPanel1.Controls.Add(labelVersion);
        flowLayoutPanel1.Controls.Add(labelVersionNumber);
        flowLayoutPanel1.Controls.Add(labelInfo);
        flowLayoutPanel1.Dock = DockStyle.Fill;
        flowLayoutPanel1.Location = new System.Drawing.Point(0, 124);
        flowLayoutPanel1.Margin = new Padding(0);
        flowLayoutPanel1.MaximumSize = new System.Drawing.Size(827, 62);
        flowLayoutPanel1.MinimumSize = new System.Drawing.Size(3, 35);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new System.Drawing.Size(621, 35);
        flowLayoutPanel1.TabIndex = 5;
        flowLayoutPanel1.WrapContents = false;
        // 
        // labelVersion
        // 
        labelVersion.AutoSize = true;
        labelVersion.Location = new System.Drawing.Point(3, 0);
        labelVersion.Margin = new Padding(3, 0, 0, 0);
        labelVersion.MaximumSize = new System.Drawing.Size(167, 62);
        labelVersion.MinimumSize = new System.Drawing.Size(0, 35);
        labelVersion.Name = "labelVersion";
        labelVersion.Size = new System.Drawing.Size(48, 35);
        labelVersion.TabIndex = 2;
        labelVersion.Text = "Version:";
        labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // labelVersionNumber
        // 
        labelVersionNumber.AutoSize = true;
        labelVersionNumber.Location = new System.Drawing.Point(51, 0);
        labelVersionNumber.Margin = new Padding(0);
        labelVersionNumber.MaximumSize = new System.Drawing.Size(171, 62);
        labelVersionNumber.MinimumSize = new System.Drawing.Size(52, 35);
        labelVersionNumber.Name = "labelVersionNumber";
        labelVersionNumber.Size = new System.Drawing.Size(52, 35);
        labelVersionNumber.TabIndex = 3;
        labelVersionNumber.Text = "0.0.0.0";
        labelVersionNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // labelInfo
        // 
        labelInfo.AutoSize = true;
        labelInfo.Dock = DockStyle.Fill;
        labelInfo.Location = new System.Drawing.Point(106, 0);
        labelInfo.MaximumSize = new System.Drawing.Size(0, 62);
        labelInfo.MinimumSize = new System.Drawing.Size(0, 35);
        labelInfo.Name = "labelInfo";
        labelInfo.Size = new System.Drawing.Size(16, 35);
        labelInfo.TabIndex = 4;
        labelInfo.Text = "...";
        labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // pictureBox1
        // 
        pictureBox1.Dock = DockStyle.Fill;
        pictureBox1.Image = Properties.Resources.Gorgon_Logo_Full;
        pictureBox1.Location = new System.Drawing.Point(0, 0);
        pictureBox1.Margin = new Padding(0);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new System.Drawing.Size(621, 124);
        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureBox1.TabIndex = 0;
        pictureBox1.TabStop = false;
        // 
        // FormSplash
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        BackgroundImageLayout = ImageLayout.Center;
        ClientSize = new System.Drawing.Size(621, 158);
        ControlBox = false;
        Controls.Add(tableLayoutPanel1);
        FormBorderStyle = FormBorderStyle.None;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Margin = new Padding(3, 5, 3, 5);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormSplash";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Editor";
        tableLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.PerformLayout();
        ((ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }


    private TableLayoutPanel tableLayoutPanel1;
    private PictureBox pictureBox1;
    private FlowLayoutPanel flowLayoutPanel1;
    private Label labelVersion;
    private Label labelVersionNumber;
    private Label labelInfo;
}
