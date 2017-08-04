namespace SobelComputeEngine
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.label1 = new System.Windows.Forms.Label();
            this.TextImagePath = new System.Windows.Forms.TextBox();
            this.ButtonImagePath = new System.Windows.Forms.Button();
            this.PanelDisplay = new System.Windows.Forms.Panel();
            this.DialogOpenPng = new System.Windows.Forms.OpenFileDialog();
            this.TrackThreshold = new System.Windows.Forms.TrackBar();
            this.TrackThickness = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ContentArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThickness)).BeginInit();
            this.SuspendLayout();
            // 
            // ContentArea
            // 
            this.ContentArea.BackColor = System.Drawing.SystemColors.Window;
            this.ContentArea.Controls.Add(this.label3);
            this.ContentArea.Controls.Add(this.label2);
            this.ContentArea.Controls.Add(this.TrackThickness);
            this.ContentArea.Controls.Add(this.TrackThreshold);
            this.ContentArea.Controls.Add(this.PanelDisplay);
            this.ContentArea.Controls.Add(this.ButtonImagePath);
            this.ContentArea.Controls.Add(this.TextImagePath);
            this.ContentArea.Controls.Add(this.label1);
            this.ContentArea.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ContentArea.Location = new System.Drawing.Point(1, 33);
            this.ContentArea.Size = new System.Drawing.Size(1278, 766);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Image Path:";
            // 
            // TextImagePath
            // 
            this.TextImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextImagePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextImagePath.Location = new System.Drawing.Point(94, 14);
            this.TextImagePath.Name = "TextImagePath";
            this.TextImagePath.ReadOnly = true;
            this.TextImagePath.Size = new System.Drawing.Size(602, 23);
            this.TextImagePath.TabIndex = 1;
            // 
            // ButtonImagePath
            // 
            this.ButtonImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonImagePath.Image = global::Gorgon.Graphics.Example.Properties.Resources.loadimage_16x16;
            this.ButtonImagePath.Location = new System.Drawing.Point(702, 14);
            this.ButtonImagePath.Name = "ButtonImagePath";
            this.ButtonImagePath.Size = new System.Drawing.Size(23, 23);
            this.ButtonImagePath.TabIndex = 2;
            this.ButtonImagePath.UseVisualStyleBackColor = true;
            this.ButtonImagePath.Click += new System.EventHandler(this.ButtonImagePath_Click);
            // 
            // PanelDisplay
            // 
            this.PanelDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelDisplay.BackColor = System.Drawing.Color.Black;
            this.PanelDisplay.Location = new System.Drawing.Point(15, 105);
            this.PanelDisplay.Name = "PanelDisplay";
            this.PanelDisplay.Size = new System.Drawing.Size(1251, 649);
            this.PanelDisplay.TabIndex = 3;
            // 
            // DialogOpenPng
            // 
            this.DialogOpenPng.Filter = "Portable Network Graphics File (*.png)|*.png";
            this.DialogOpenPng.Title = "Load a PNG file as a texture";
            // 
            // TrackThreshold
            // 
            this.TrackThreshold.Enabled = false;
            this.TrackThreshold.Location = new System.Drawing.Point(94, 43);
            this.TrackThreshold.Maximum = 90;
            this.TrackThreshold.Minimum = 1;
            this.TrackThreshold.Name = "TrackThreshold";
            this.TrackThreshold.Size = new System.Drawing.Size(289, 56);
            this.TrackThreshold.TabIndex = 4;
            this.TrackThreshold.TickFrequency = 10;
            this.TrackThreshold.Value = 25;
            this.TrackThreshold.ValueChanged += new System.EventHandler(this.TrackThreshold_ValueChanged);
            // 
            // TrackThickness
            // 
            this.TrackThickness.Enabled = false;
            this.TrackThickness.Location = new System.Drawing.Point(464, 43);
            this.TrackThickness.Maximum = 5;
            this.TrackThickness.Minimum = 1;
            this.TrackThickness.Name = "TrackThickness";
            this.TrackThickness.Size = new System.Drawing.Size(289, 56);
            this.TrackThickness.TabIndex = 5;
            this.TrackThickness.Value = 2;
            this.TrackThickness.ValueChanged += new System.EventHandler(this.TrackThreshold_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Threshold:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(389, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Thickness:";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormMain";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowBorder = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sobel Shader - Compute Engine";
            this.ContentArea.ResumeLayout(false);
            this.ContentArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThickness)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox TextImagePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonImagePath;
        private System.Windows.Forms.Panel PanelDisplay;
        private System.Windows.Forms.OpenFileDialog DialogOpenPng;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar TrackThickness;
        private System.Windows.Forms.TrackBar TrackThreshold;
    }
}

