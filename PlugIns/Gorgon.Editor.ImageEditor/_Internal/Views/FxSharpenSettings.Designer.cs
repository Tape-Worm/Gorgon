namespace Gorgon.Editor.ImageEditor
{
    partial class FxSharpenSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NumericSharpenAmount = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.PanelBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSharpenAmount)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.NumericSharpenAmount);
            this.PanelBody.Controls.Add(this.label1);
            this.PanelBody.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.PanelBody.Size = new System.Drawing.Size(265, 59);
            // 
            // NumericSharpenAmount
            // 
            this.NumericSharpenAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericSharpenAmount.Dock = System.Windows.Forms.DockStyle.Top;
            this.NumericSharpenAmount.Location = new System.Drawing.Point(3, 21);
            this.NumericSharpenAmount.Name = "NumericSharpenAmount";
            this.NumericSharpenAmount.Size = new System.Drawing.Size(259, 23);
            this.NumericSharpenAmount.TabIndex = 3;
            this.NumericSharpenAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericSharpenAmount.ValueChanged += new System.EventHandler(this.NumericSharpenAmount_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label1.Size = new System.Drawing.Size(98, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "Sharpen amount:";
            // 
            // FxSharpenSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "FxSharpenSettings";
            this.Size = new System.Drawing.Size(265, 116);
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSharpenAmount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown NumericSharpenAmount;
        private System.Windows.Forms.Label label1;
    }
}
