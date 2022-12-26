namespace Gorgon.Editor.ImageEditor;

partial class FxBlurSettings
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.label1 = new System.Windows.Forms.Label();
        this.NumericBlurAmount = new System.Windows.Forms.NumericUpDown();
        this.PanelBody.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericBlurAmount)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.NumericBlurAmount);
        this.PanelBody.Controls.Add(this.label1);
        this.PanelBody.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.PanelBody.Size = new System.Drawing.Size(321, 60);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Dock = System.Windows.Forms.DockStyle.Top;
        this.label1.Location = new System.Drawing.Point(3, 0);
        this.label1.Name = "label1";
        this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.label1.Size = new System.Drawing.Size(76, 21);
        this.label1.TabIndex = 0;
        this.label1.Text = "Blur amount:";
        // 
        // NumericBlurAmount
        // 
        this.NumericBlurAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericBlurAmount.Dock = System.Windows.Forms.DockStyle.Top;
        this.NumericBlurAmount.Location = new System.Drawing.Point(3, 21);
        this.NumericBlurAmount.Maximum = new decimal(new int[] {
        200,
        0,
        0,
        0});
        this.NumericBlurAmount.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericBlurAmount.Name = "NumericBlurAmount";
        this.NumericBlurAmount.Size = new System.Drawing.Size(315, 23);
        this.NumericBlurAmount.TabIndex = 1;
        this.NumericBlurAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericBlurAmount.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericBlurAmount.ValueChanged += new System.EventHandler(this.NumericBlurAmount_ValueChanged);
        // 
        // FxBlurSettings
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "FxBlurSettings";
        this.Size = new System.Drawing.Size(321, 117);
        this.Text = "Gaussian Blur Settings";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericBlurAmount)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.NumericUpDown NumericBlurAmount;
    private System.Windows.Forms.Label label1;
}
