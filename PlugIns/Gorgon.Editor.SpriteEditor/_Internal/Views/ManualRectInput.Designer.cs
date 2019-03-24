namespace Gorgon.Editor.SpriteEditor
{
    partial class ManualRectInput
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PanelCaptionBorder = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.ButtonClose = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.LabelCaption = new System.Windows.Forms.Label();
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
            this.PanelCaptionBorder.SuspendLayout();
            this.panel2.SuspendLayout();
            this.PanelRect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelCaptionBorder
            // 
            this.PanelCaptionBorder.AutoSize = true;
            this.PanelCaptionBorder.BackColor = System.Drawing.Color.SteelBlue;
            this.PanelCaptionBorder.Controls.Add(this.panel2);
            this.PanelCaptionBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelCaptionBorder.Location = new System.Drawing.Point(0, 0);
            this.PanelCaptionBorder.Name = "PanelCaptionBorder";
            this.PanelCaptionBorder.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.PanelCaptionBorder.Size = new System.Drawing.Size(349, 21);
            this.PanelCaptionBorder.TabIndex = 0;
            this.PanelCaptionBorder.Visible = false;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.panel2.Controls.Add(this.ButtonClose);
            this.panel2.Controls.Add(this.LabelCaption);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(347, 21);
            this.panel2.TabIndex = 0;
            // 
            // ButtonClose
            // 
            this.ButtonClose.AutoSize = true;
            this.ButtonClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonClose.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.FormClose;
            this.ButtonClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.ButtonClose.Location = new System.Drawing.Point(326, 0);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(21, 21);
            this.ButtonClose.StateCommon.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
            this.ButtonClose.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ButtonClose.StateCommon.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
            this.ButtonClose.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Marlett", 9F);
            this.ButtonClose.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonClose.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonClose.StateNormal.Content.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ButtonClose.TabIndex = 1;
            this.ButtonClose.TabStop = false;
            this.ButtonClose.Values.Text = "r";
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // LabelCaption
            // 
            this.LabelCaption.AutoSize = true;
            this.LabelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCaption.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.LabelCaption.Location = new System.Drawing.Point(0, 0);
            this.LabelCaption.Name = "LabelCaption";
            this.LabelCaption.Size = new System.Drawing.Size(169, 21);
            this.LabelCaption.TabIndex = 0;
            this.LabelCaption.Text = "Manual Sprite Clipping";
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
            this.PanelRect.Location = new System.Drawing.Point(0, 21);
            this.PanelRect.Name = "PanelRect";
            this.PanelRect.Size = new System.Drawing.Size(349, 271);
            this.PanelRect.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(151, 221);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Bottom";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(271, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Right";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Left";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(161, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Top";
            // 
            // NumericBottom
            // 
            this.NumericBottom.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.NumericBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericBottom.Location = new System.Drawing.Point(120, 195);
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
            this.NumericRight.Location = new System.Drawing.Point(234, 122);
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
            this.NumericLeft.Location = new System.Drawing.Point(6, 122);
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
            this.panel3.Size = new System.Drawing.Size(108, 108);
            this.panel3.TabIndex = 0;
            // 
            // ManualRectInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelRect);
            this.Controls.Add(this.PanelCaptionBorder);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ManualRectInput";
            this.Size = new System.Drawing.Size(349, 292);
            this.PanelCaptionBorder.ResumeLayout(false);
            this.PanelCaptionBorder.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.PanelRect.ResumeLayout(false);
            this.PanelRect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PanelCaptionBorder;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label LabelCaption;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonClose;
        private System.Windows.Forms.Panel PanelRect;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.NumericUpDown NumericBottom;
        private System.Windows.Forms.NumericUpDown NumericTop;
        private System.Windows.Forms.NumericUpDown NumericRight;
        private System.Windows.Forms.NumericUpDown NumericLeft;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
    }
}
