namespace TestClient
{
    partial class ColorizerDialog
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
            this.redTrackBar = new System.Windows.Forms.TrackBar();
            this.greenTrackBar = new System.Windows.Forms.TrackBar();
            this.blueTrackBar = new System.Windows.Forms.TrackBar();
            this.alphaTrackBar = new System.Windows.Forms.TrackBar();
            this.lblRed = new System.Windows.Forms.Label();
            this.lblGreen = new System.Windows.Forms.Label();
            this.lblBlue = new System.Windows.Forms.Label();
            this.lblAlpha = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.redTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // redTrackBar
            // 
            this.redTrackBar.Location = new System.Drawing.Point(40, 35);
            this.redTrackBar.Maximum = 255;
            this.redTrackBar.Name = "redTrackBar";
            this.redTrackBar.Size = new System.Drawing.Size(365, 45);
            this.redTrackBar.TabIndex = 0;
            this.redTrackBar.TickFrequency = 8;
            this.redTrackBar.ValueChanged += new System.EventHandler(this.TRACKBAR_ValueChanged);
            // 
            // greenTrackBar
            // 
            this.greenTrackBar.Location = new System.Drawing.Point(40, 94);
            this.greenTrackBar.Maximum = 255;
            this.greenTrackBar.Name = "greenTrackBar";
            this.greenTrackBar.Size = new System.Drawing.Size(365, 45);
            this.greenTrackBar.TabIndex = 1;
            this.greenTrackBar.TickFrequency = 8;
            this.greenTrackBar.ValueChanged += new System.EventHandler(this.TRACKBAR_ValueChanged);
            // 
            // blueTrackBar
            // 
            this.blueTrackBar.Location = new System.Drawing.Point(40, 153);
            this.blueTrackBar.Maximum = 255;
            this.blueTrackBar.Name = "blueTrackBar";
            this.blueTrackBar.Size = new System.Drawing.Size(365, 45);
            this.blueTrackBar.TabIndex = 2;
            this.blueTrackBar.TickFrequency = 8;
            this.blueTrackBar.ValueChanged += new System.EventHandler(this.TRACKBAR_ValueChanged);
            // 
            // alphaTrackBar
            // 
            this.alphaTrackBar.Location = new System.Drawing.Point(40, 212);
            this.alphaTrackBar.Maximum = 255;
            this.alphaTrackBar.Minimum = 50;
            this.alphaTrackBar.Name = "alphaTrackBar";
            this.alphaTrackBar.Size = new System.Drawing.Size(365, 45);
            this.alphaTrackBar.TabIndex = 3;
            this.alphaTrackBar.TickFrequency = 8;
            this.alphaTrackBar.Value = 50;
            this.alphaTrackBar.ValueChanged += new System.EventHandler(this.TRACKBAR_ValueChanged);
            // 
            // lblRed
            // 
            this.lblRed.AutoSize = true;
            this.lblRed.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblRed.ForeColor = System.Drawing.Color.Red;
            this.lblRed.Location = new System.Drawing.Point(37, 19);
            this.lblRed.Name = "lblRed";
            this.lblRed.Size = new System.Drawing.Size(35, 13);
            this.lblRed.TabIndex = 4;
            this.lblRed.Text = "Red:";
            // 
            // lblGreen
            // 
            this.lblGreen.AutoSize = true;
            this.lblGreen.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGreen.ForeColor = System.Drawing.Color.Green;
            this.lblGreen.Location = new System.Drawing.Point(37, 78);
            this.lblGreen.Name = "lblGreen";
            this.lblGreen.Size = new System.Drawing.Size(50, 13);
            this.lblGreen.TabIndex = 5;
            this.lblGreen.Text = "Green:";
            // 
            // lblBlue
            // 
            this.lblBlue.AutoSize = true;
            this.lblBlue.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblBlue.ForeColor = System.Drawing.Color.Blue;
            this.lblBlue.Location = new System.Drawing.Point(37, 137);
            this.lblBlue.Name = "lblBlue";
            this.lblBlue.Size = new System.Drawing.Size(39, 13);
            this.lblBlue.TabIndex = 6;
            this.lblBlue.Text = "Blue:";
            // 
            // lblAlpha
            // 
            this.lblAlpha.AutoSize = true;
            this.lblAlpha.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAlpha.Location = new System.Drawing.Point(37, 196);
            this.lblAlpha.Name = "lblAlpha";
            this.lblAlpha.Size = new System.Drawing.Size(48, 13);
            this.lblAlpha.TabIndex = 7;
            this.lblAlpha.Text = "Alpha:";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(40, 284);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 25);
            this.button1.TabIndex = 8;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(149, 284);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(103, 25);
            this.button2.TabIndex = 9;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // ColorizerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 327);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblAlpha);
            this.Controls.Add(this.lblBlue);
            this.Controls.Add(this.lblGreen);
            this.Controls.Add(this.lblRed);
            this.Controls.Add(this.alphaTrackBar);
            this.Controls.Add(this.blueTrackBar);
            this.Controls.Add(this.greenTrackBar);
            this.Controls.Add(this.redTrackBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorizerDialog";
            this.Text = "Colorizer Dialog";
            this.Load += new System.EventHandler(this.ColorizerDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.redTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar redTrackBar;
        private System.Windows.Forms.TrackBar greenTrackBar;
        private System.Windows.Forms.TrackBar blueTrackBar;
        private System.Windows.Forms.TrackBar alphaTrackBar;
        private System.Windows.Forms.Label lblRed;
        private System.Windows.Forms.Label lblGreen;
        private System.Windows.Forms.Label lblBlue;
        private System.Windows.Forms.Label lblAlpha;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}