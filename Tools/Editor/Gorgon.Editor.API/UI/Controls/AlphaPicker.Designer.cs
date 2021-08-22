namespace Gorgon.Editor.UI.Controls
{
    partial class AlphaPicker
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
                AlphaValueChangedEvent = null;
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
            this.TableAlpha = new System.Windows.Forms.TableLayoutPanel();
            this.PanelAlpha = new System.Windows.Forms.Panel();
            this.LabelAlpha = new System.Windows.Forms.Label();
            this.NumericAlpha = new System.Windows.Forms.NumericUpDown();
            this.PanelIndicator = new System.Windows.Forms.Panel();
            this.TableAlpha.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericAlpha)).BeginInit();
            this.SuspendLayout();
            // 
            // TableAlpha
            // 
            this.TableAlpha.ColumnCount = 2;
            this.TableAlpha.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableAlpha.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableAlpha.Controls.Add(this.PanelAlpha, 0, 0);
            this.TableAlpha.Controls.Add(this.LabelAlpha, 0, 2);
            this.TableAlpha.Controls.Add(this.NumericAlpha, 1, 2);
            this.TableAlpha.Controls.Add(this.PanelIndicator, 0, 1);
            this.TableAlpha.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableAlpha.Location = new System.Drawing.Point(0, 0);
            this.TableAlpha.Name = "TableAlpha";
            this.TableAlpha.RowCount = 4;
            this.TableAlpha.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableAlpha.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.TableAlpha.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableAlpha.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableAlpha.Size = new System.Drawing.Size(411, 168);
            this.TableAlpha.TabIndex = 1;
            // 
            // PanelAlpha
            // 
            this.PanelAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelAlpha.BackgroundImage = global::Gorgon.Editor.Properties.Resources.Transparency_Pattern;
            this.TableAlpha.SetColumnSpan(this.PanelAlpha, 2);
            this.PanelAlpha.Location = new System.Drawing.Point(5, 3);
            this.PanelAlpha.Margin = new System.Windows.Forms.Padding(5, 3, 5, 0);
            this.PanelAlpha.Name = "PanelAlpha";
            this.PanelAlpha.Size = new System.Drawing.Size(401, 29);
            this.PanelAlpha.TabIndex = 0;
            this.PanelAlpha.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelAlpha_Paint);
            this.PanelAlpha.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelAlpha_MouseDown);
            this.PanelAlpha.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelAlpha_MouseDown);
            // 
            // LabelAlpha
            // 
            this.LabelAlpha.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelAlpha.AutoSize = true;
            this.LabelAlpha.Location = new System.Drawing.Point(3, 57);
            this.LabelAlpha.Margin = new System.Windows.Forms.Padding(3);
            this.LabelAlpha.Name = "LabelAlpha";
            this.LabelAlpha.Size = new System.Drawing.Size(41, 15);
            this.LabelAlpha.TabIndex = 0;
            this.LabelAlpha.Text = "Alpha:";
            // 
            // NumericAlpha
            // 
            this.NumericAlpha.BackColor = System.Drawing.Color.White;
            this.NumericAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericAlpha.ForeColor = System.Drawing.Color.Black;
            this.NumericAlpha.Location = new System.Drawing.Point(50, 53);
            this.NumericAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.NumericAlpha.Name = "NumericAlpha";
            this.NumericAlpha.Size = new System.Drawing.Size(120, 23);
            this.NumericAlpha.TabIndex = 1;
            this.NumericAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericAlpha.ValueChanged += new System.EventHandler(this.NumericAlpha_ValueChanged);
            // 
            // PanelIndicator
            // 
            this.TableAlpha.SetColumnSpan(this.PanelIndicator, 2);
            this.PanelIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelIndicator.Location = new System.Drawing.Point(0, 32);
            this.PanelIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.PanelIndicator.Name = "PanelIndicator";
            this.PanelIndicator.Size = new System.Drawing.Size(411, 16);
            this.PanelIndicator.TabIndex = 2;
            this.PanelIndicator.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelIndicator_Paint);
            this.PanelIndicator.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelAlpha_MouseDown);
            this.PanelIndicator.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelAlpha_MouseDown);
            // 
            // AlphaPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.Controls.Add(this.TableAlpha);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "AlphaPicker";
            this.Size = new System.Drawing.Size(411, 168);
            this.TableAlpha.ResumeLayout(false);
            this.TableAlpha.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericAlpha)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel TableAlpha;
        private System.Windows.Forms.Label LabelAlpha;
        private System.Windows.Forms.NumericUpDown NumericAlpha;
        private System.Windows.Forms.Panel PanelAlpha;
        private System.Windows.Forms.Panel PanelIndicator;
    }
}
