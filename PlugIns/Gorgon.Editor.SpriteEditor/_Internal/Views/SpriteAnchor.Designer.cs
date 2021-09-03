namespace Gorgon.Editor.SpriteEditor
{
    partial class SpriteAnchor
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
                DataContext?.Unload();
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Alignment = new Gorgon.UI.GorgonAlignmentPicker();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.NumericHorizontal = new System.Windows.Forms.NumericUpDown();
            this.NumericVertical = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.CheckRotate = new System.Windows.Forms.CheckBox();
            this.CheckScale = new System.Windows.Forms.CheckBox();
            this.PanelBody.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHorizontal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericVertical)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.tableLayoutPanel1);
            this.PanelBody.Size = new System.Drawing.Size(212, 251);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.Alignment, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CheckRotate, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.CheckScale, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(212, 251);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // Alignment
            // 
            this.Alignment.Alignment = Gorgon.UI.Alignment.None;
            this.Alignment.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Alignment.Location = new System.Drawing.Point(53, 41);
            this.Alignment.Name = "Alignment";
            this.Alignment.Size = new System.Drawing.Size(105, 105);
            this.Alignment.TabIndex = 1;
            this.Alignment.AlignmentChanged += new System.EventHandler(this.Alignment_AlignmentChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.NumericHorizontal, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.NumericVertical, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(10, 6);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(191, 29);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // NumericHorizontal
            // 
            this.NumericHorizontal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.NumericHorizontal.Location = new System.Drawing.Point(3, 3);
            this.NumericHorizontal.Maximum = new decimal(new int[] {
            262144,
            0,
            0,
            0});
            this.NumericHorizontal.Minimum = new decimal(new int[] {
            262144,
            0,
            0,
            -2147483648});
            this.NumericHorizontal.Name = "NumericHorizontal";
            this.NumericHorizontal.Size = new System.Drawing.Size(80, 23);
            this.NumericHorizontal.TabIndex = 0;
            this.NumericHorizontal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericHorizontal.ValueChanged += new System.EventHandler(this.NumericHorizontal_ValueChanged);
            // 
            // NumericVertical
            // 
            this.NumericVertical.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.NumericVertical.Location = new System.Drawing.Point(108, 3);
            this.NumericVertical.Maximum = new decimal(new int[] {
            262144,
            0,
            0,
            0});
            this.NumericVertical.Minimum = new decimal(new int[] {
            262144,
            0,
            0,
            -2147483648});
            this.NumericVertical.Name = "NumericVertical";
            this.NumericVertical.Size = new System.Drawing.Size(80, 23);
            this.NumericVertical.TabIndex = 1;
            this.NumericVertical.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericVertical.ValueChanged += new System.EventHandler(this.NumericVertical_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(89, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "x";
            // 
            // CheckRotate
            // 
            this.CheckRotate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckRotate.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckRotate.AutoSize = true;
            this.CheckRotate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.CheckRotate.FlatAppearance.CheckedBackColor = System.Drawing.Color.CornflowerBlue;
            this.CheckRotate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.CheckRotate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.CheckRotate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckRotate.Location = new System.Drawing.Point(3, 152);
            this.CheckRotate.Name = "CheckRotate";
            this.CheckRotate.Size = new System.Drawing.Size(206, 23);
            this.CheckRotate.TabIndex = 2;
            this.CheckRotate.Text = "Test Rotation";
            this.CheckRotate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckRotate.UseVisualStyleBackColor = true;
            this.CheckRotate.Click += new System.EventHandler(this.CheckRotate_Click);
            // 
            // CheckScale
            // 
            this.CheckScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckScale.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckScale.AutoSize = true;
            this.CheckScale.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.CheckScale.FlatAppearance.CheckedBackColor = System.Drawing.Color.CornflowerBlue;
            this.CheckScale.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.CheckScale.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.CheckScale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckScale.Location = new System.Drawing.Point(3, 181);
            this.CheckScale.Name = "CheckScale";
            this.CheckScale.Size = new System.Drawing.Size(206, 25);
            this.CheckScale.TabIndex = 3;
            this.CheckScale.Text = "Test Scaling";
            this.CheckScale.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckScale.UseVisualStyleBackColor = true;
            this.CheckScale.Click += new System.EventHandler(this.CheckScale_Click);
            // 
            // SpriteAnchor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "SpriteAnchor";
            this.Size = new System.Drawing.Size(212, 308);
            this.Text = "Sprite Anchor";
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHorizontal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericVertical)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Gorgon.UI.GorgonAlignmentPicker Alignment;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.NumericUpDown NumericHorizontal;
        private System.Windows.Forms.NumericUpDown NumericVertical;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox CheckRotate;
        private System.Windows.Forms.CheckBox CheckScale;
    }
}
