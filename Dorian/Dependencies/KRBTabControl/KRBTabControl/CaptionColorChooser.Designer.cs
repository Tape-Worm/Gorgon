namespace KRBTabControl
{
    partial class KRBTabControl
    {
        partial class CaptionColorChooser
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
                if (disposing)
                {
                    if (components != null)
                        components.Dispose();

                    foreach (System.Windows.Forms.Control control in Controls)
                        control.Dispose();
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
                this.Captions = new MyPanel();
                this.button1 = new System.Windows.Forms.Button();
                this.button2 = new System.Windows.Forms.Button();
                this.nmrRed = new System.Windows.Forms.NumericUpDown();
                this.nmrGreen = new System.Windows.Forms.NumericUpDown();
                this.nmrBlue = new System.Windows.Forms.NumericUpDown();
                this.nmrAlpha = new System.Windows.Forms.NumericUpDown();
                this.label1 = new System.Windows.Forms.Label();
                this.label2 = new System.Windows.Forms.Label();
                this.label3 = new System.Windows.Forms.Label();
                this.label4 = new System.Windows.Forms.Label();
                this.groupBox1 = new System.Windows.Forms.GroupBox();
                ((System.ComponentModel.ISupportInitialize)(this.nmrRed)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.nmrGreen)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.nmrBlue)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.nmrAlpha)).BeginInit();
                this.groupBox1.SuspendLayout();
                this.SuspendLayout();
                // 
                // Captions
                // 
                this.Captions.Location = new System.Drawing.Point(19, 19);
                this.Captions.Margin = new System.Windows.Forms.Padding(10);
                this.Captions.Name = "Captions";
                this.Captions.Size = new System.Drawing.Size(369, 70);
                this.Captions.TabIndex = 0;
                this.Captions.Paint += new System.Windows.Forms.PaintEventHandler(this.Captions_Paint);
                // 
                // button1
                // 
                this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.button1.Location = new System.Drawing.Point(125, 358);
                this.button1.Name = "button1";
                this.button1.Size = new System.Drawing.Size(75, 23);
                this.button1.TabIndex = 1;
                this.button1.Text = "Apply";
                this.button1.UseVisualStyleBackColor = true;
                this.button1.Click += new System.EventHandler(this.button1_Click);
                // 
                // button2
                // 
                this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.button2.Location = new System.Drawing.Point(206, 358);
                this.button2.Name = "button2";
                this.button2.Size = new System.Drawing.Size(75, 23);
                this.button2.TabIndex = 2;
                this.button2.Text = "Cancel";
                this.button2.UseVisualStyleBackColor = true;
                // 
                // nmrRed
                // 
                this.nmrRed.Location = new System.Drawing.Point(18, 41);
                this.nmrRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
                this.nmrRed.Name = "nmrRed";
                this.nmrRed.Size = new System.Drawing.Size(120, 20);
                this.nmrRed.TabIndex = 3;
                this.nmrRed.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
                // 
                // nmrGreen
                // 
                this.nmrGreen.Location = new System.Drawing.Point(18, 84);
                this.nmrGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
                this.nmrGreen.Name = "nmrGreen";
                this.nmrGreen.Size = new System.Drawing.Size(120, 20);
                this.nmrGreen.TabIndex = 4;
                this.nmrGreen.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
                // 
                // nmrBlue
                // 
                this.nmrBlue.Location = new System.Drawing.Point(18, 128);
                this.nmrBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
                this.nmrBlue.Name = "nmrBlue";
                this.nmrBlue.Size = new System.Drawing.Size(120, 20);
                this.nmrBlue.TabIndex = 5;
                this.nmrBlue.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
                // 
                // nmrAlpha
                // 
                this.nmrAlpha.Location = new System.Drawing.Point(18, 171);
                this.nmrAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
                this.nmrAlpha.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
                this.nmrAlpha.Name = "nmrAlpha";
                this.nmrAlpha.Size = new System.Drawing.Size(120, 20);
                this.nmrAlpha.TabIndex = 6;
                this.nmrAlpha.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
                this.nmrAlpha.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
                // 
                // label1
                // 
                this.label1.AutoSize = true;
                this.label1.ForeColor = System.Drawing.Color.Red;
                this.label1.Location = new System.Drawing.Point(15, 25);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(30, 13);
                this.label1.TabIndex = 7;
                this.label1.Text = "Red:";
                // 
                // label2
                // 
                this.label2.AutoSize = true;
                this.label2.ForeColor = System.Drawing.Color.Green;
                this.label2.Location = new System.Drawing.Point(15, 68);
                this.label2.Name = "label2";
                this.label2.Size = new System.Drawing.Size(39, 13);
                this.label2.TabIndex = 8;
                this.label2.Text = "Green:";
                // 
                // label3
                // 
                this.label3.AutoSize = true;
                this.label3.ForeColor = System.Drawing.Color.Blue;
                this.label3.Location = new System.Drawing.Point(15, 112);
                this.label3.Name = "label3";
                this.label3.Size = new System.Drawing.Size(31, 13);
                this.label3.TabIndex = 9;
                this.label3.Text = "Blue:";
                // 
                // label4
                // 
                this.label4.AutoSize = true;
                this.label4.Location = new System.Drawing.Point(15, 155);
                this.label4.Name = "label4";
                this.label4.Size = new System.Drawing.Size(37, 13);
                this.label4.TabIndex = 10;
                this.label4.Text = "Alpha:";
                // 
                // groupBox1
                // 
                this.groupBox1.Controls.Add(this.label1);
                this.groupBox1.Controls.Add(this.label4);
                this.groupBox1.Controls.Add(this.nmrRed);
                this.groupBox1.Controls.Add(this.label3);
                this.groupBox1.Controls.Add(this.nmrGreen);
                this.groupBox1.Controls.Add(this.label2);
                this.groupBox1.Controls.Add(this.nmrBlue);
                this.groupBox1.Controls.Add(this.nmrAlpha);
                this.groupBox1.Location = new System.Drawing.Point(46, 113);
                this.groupBox1.Name = "groupBox1";
                this.groupBox1.Size = new System.Drawing.Size(157, 211);
                this.groupBox1.TabIndex = 11;
                this.groupBox1.TabStop = false;
                this.groupBox1.Text = "Color components";
                // 
                // CaptionColorChooser
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(407, 394);
                this.Controls.Add(this.groupBox1);
                this.Controls.Add(this.button2);
                this.Controls.Add(this.button1);
                this.Controls.Add(this.Captions);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
                this.Name = "CaptionColorChooser";
                this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                this.Text = "Caption Color Chooser";
                this.Load += new System.EventHandler(this.CaptionColorChooser_Load);
                ((System.ComponentModel.ISupportInitialize)(this.nmrRed)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(this.nmrGreen)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(this.nmrBlue)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(this.nmrAlpha)).EndInit();
                this.groupBox1.ResumeLayout(false);
                this.groupBox1.PerformLayout();
                this.ResumeLayout(false);

            }

            #endregion

            private MyPanel Captions;
            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.Button button2;
            private System.Windows.Forms.NumericUpDown nmrRed;
            private System.Windows.Forms.NumericUpDown nmrGreen;
            private System.Windows.Forms.NumericUpDown nmrBlue;
            private System.Windows.Forms.NumericUpDown nmrAlpha;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.Label label3;
            private System.Windows.Forms.Label label4;
            private System.Windows.Forms.GroupBox groupBox1;
        }
    }
}