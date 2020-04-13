namespace Gorgon.Editor.SpriteEditor
{
    partial class FormManualVertexEdit
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManualVertexEdit));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.NumericX = new System.Windows.Forms.NumericUpDown();
			this.NumericY = new System.Windows.Forms.NumericUpDown();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.NumericX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericY)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "X Offset:";
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(187, 27);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(52, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "Y Offset:";
			// 
			// NumericX
			// 
			this.NumericX.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.NumericX.BackColor = System.Drawing.Color.White;
			this.NumericX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NumericX.ForeColor = System.Drawing.Color.Black;
			this.NumericX.Location = new System.Drawing.Point(61, 23);
			this.NumericX.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
			this.NumericX.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
			this.NumericX.Name = "NumericX";
			this.NumericX.Size = new System.Drawing.Size(120, 23);
			this.NumericX.TabIndex = 2;
			this.NumericX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NumericX.ValueChanged += new System.EventHandler(this.NumericOffset_ValueChanged);
			// 
			// NumericY
			// 
			this.NumericY.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.NumericY.BackColor = System.Drawing.Color.White;
			this.NumericY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NumericY.ForeColor = System.Drawing.Color.Black;
			this.NumericY.Location = new System.Drawing.Point(245, 23);
			this.NumericY.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
			this.NumericY.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
			this.NumericY.Name = "NumericY";
			this.NumericY.Size = new System.Drawing.Size(120, 23);
			this.NumericY.TabIndex = 3;
			this.NumericY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NumericY.ValueChanged += new System.EventHandler(this.NumericOffset_ValueChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.NumericY, 3, 1);
			this.tableLayoutPanel1.Controls.Add(this.NumericX, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 2, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(371, 68);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// FormManualVertexEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ClientSize = new System.Drawing.Size(371, 68);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.White;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormManualVertexEdit";
			this.Text = "Manual Corner Editing";
			((System.ComponentModel.ISupportInitialize)(this.NumericX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericY)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown NumericX;
        private System.Windows.Forms.NumericUpDown NumericY;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}