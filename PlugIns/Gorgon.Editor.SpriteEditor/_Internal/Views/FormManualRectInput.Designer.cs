namespace Gorgon.Editor.SpriteEditor
{
    partial class FormManualRectInput
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

            if (disposing)
            {                
                UnassignEvents();
                DataContext?.OnUnload();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManualRectInput));
            this.PanelRect = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.NumericBottom = new System.Windows.Forms.NumericUpDown();
            this.NumericTop = new System.Windows.Forms.NumericUpDown();
            this.NumericRight = new System.Windows.Forms.NumericUpDown();
            this.NumericLeft = new System.Windows.Forms.NumericUpDown();
            this.panel3 = new System.Windows.Forms.Panel();
            this.PanelRect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelRect
            // 
            this.PanelRect.Controls.Add(this.label4);
            this.PanelRect.Controls.Add(this.label3);
            this.PanelRect.Controls.Add(this.label2);
            this.PanelRect.Controls.Add(this.label1);
            this.PanelRect.Controls.Add(this.NumericBottom);
            this.PanelRect.Controls.Add(this.NumericTop);
            this.PanelRect.Controls.Add(this.NumericRight);
            this.PanelRect.Controls.Add(this.NumericLeft);
            this.PanelRect.Controls.Add(this.panel3);
            this.PanelRect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRect.Location = new System.Drawing.Point(0, 0);
            this.PanelRect.Name = "PanelRect";
            this.PanelRect.Size = new System.Drawing.Size(349, 263);
            this.PanelRect.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(151, 216);
            this.label4.Margin = new System.Windows.Forms.Padding(3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Bottom";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(270, 97);
            this.label3.Margin = new System.Windows.Forms.Padding(3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Right";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 97);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Left";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(161, 30);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Top";
            // 
            // NumericBottom
            // 
            this.NumericBottom.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.NumericBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericBottom.Location = new System.Drawing.Point(120, 187);
            this.NumericBottom.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.NumericBottom.Minimum = new decimal(new int[] {
            16384,
            0,
            0,
            -2147483648});
            this.NumericBottom.Name = "NumericBottom";
            this.NumericBottom.Size = new System.Drawing.Size(108, 23);
            this.NumericBottom.TabIndex = 4;
            this.NumericBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericBottom.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
            // 
            // NumericTop
            // 
            this.NumericTop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.NumericTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericTop.Location = new System.Drawing.Point(120, 52);
            this.NumericTop.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.NumericTop.Minimum = new decimal(new int[] {
            16384,
            0,
            0,
            -2147483648});
            this.NumericTop.Name = "NumericTop";
            this.NumericTop.Size = new System.Drawing.Size(108, 23);
            this.NumericTop.TabIndex = 3;
            this.NumericTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericTop.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
            // 
            // NumericRight
            // 
            this.NumericRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.NumericRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericRight.Location = new System.Drawing.Point(234, 118);
            this.NumericRight.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.NumericRight.Minimum = new decimal(new int[] {
            16384,
            0,
            0,
            -2147483648});
            this.NumericRight.Name = "NumericRight";
            this.NumericRight.Size = new System.Drawing.Size(108, 23);
            this.NumericRight.TabIndex = 2;
            this.NumericRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericRight.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
            // 
            // NumericLeft
            // 
            this.NumericLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.NumericLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericLeft.Location = new System.Drawing.Point(6, 118);
            this.NumericLeft.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.NumericLeft.Minimum = new decimal(new int[] {
            16384,
            0,
            0,
            -2147483648});
            this.NumericLeft.Name = "NumericLeft";
            this.NumericLeft.Size = new System.Drawing.Size(108, 23);
            this.NumericLeft.TabIndex = 1;
            this.NumericLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericLeft.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Location = new System.Drawing.Point(120, 81);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(108, 100);
            this.panel3.TabIndex = 0;
            // 
            // FormManualRectInput
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(349, 263);
            this.Controls.Add(this.PanelRect);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormManualRectInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Manual Sprite Clipping";
            this.PanelRect.ResumeLayout(false);
            this.PanelRect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelRect;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown NumericBottom;
        private System.Windows.Forms.NumericUpDown NumericTop;
        private System.Windows.Forms.NumericUpDown NumericRight;
        private System.Windows.Forms.NumericUpDown NumericLeft;
        private System.Windows.Forms.Panel panel3;
    }
}