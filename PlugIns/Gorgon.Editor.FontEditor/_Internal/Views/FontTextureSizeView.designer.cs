using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.FontEditor;

partial class FontTextureSizeView
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
            ViewModel?.Unload();
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
        this.LabelTextureWidth = new System.Windows.Forms.Label();
        this.LabelTextureHeight = new System.Windows.Forms.Label();
        this.NumericTextureWidth = new System.Windows.Forms.NumericUpDown();
        this.NumericTextureHeight = new System.Windows.Forms.NumericUpDown();
        this.PanelBody.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.tableLayoutPanel1);
        this.PanelBody.Size = new System.Drawing.Size(254, 120);
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.ColumnCount = 2;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel1.Controls.Add(this.NumericTextureHeight, 1, 1);
        this.tableLayoutPanel1.Controls.Add(this.LabelTextureWidth, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.NumericTextureWidth, 0, 1);
        this.tableLayoutPanel1.Controls.Add(this.LabelTextureHeight, 1, 0);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 2;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(254, 120);
        this.tableLayoutPanel1.TabIndex = 0;
        // 
        // LabelTextureWidth
        // 
        this.LabelTextureWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.LabelTextureWidth.AutoSize = true;
        this.LabelTextureWidth.Location = new System.Drawing.Point(3, 3);
        this.LabelTextureWidth.Margin = new System.Windows.Forms.Padding(3);
        this.LabelTextureWidth.Name = "LabelTextureWidth";
        this.LabelTextureWidth.Size = new System.Drawing.Size(79, 15);
        this.LabelTextureWidth.TabIndex = 1;
        this.LabelTextureWidth.Text = "Texure Width:";
        // 
        // LabelTextureHeight
        // 
        this.LabelTextureHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.LabelTextureHeight.AutoSize = true;
        this.LabelTextureHeight.Location = new System.Drawing.Point(96, 3);
        this.LabelTextureHeight.Margin = new System.Windows.Forms.Padding(3);
        this.LabelTextureHeight.Name = "LabelTextureHeight";
        this.LabelTextureHeight.Size = new System.Drawing.Size(87, 15);
        this.LabelTextureHeight.TabIndex = 2;
        this.LabelTextureHeight.Text = "Texture Height:";
        // 
        // NumericTextureWidth
        // 
        this.NumericTextureWidth.BackColor = System.Drawing.Color.White;
        this.NumericTextureWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericTextureWidth.ForeColor = System.Drawing.Color.Black;
        this.NumericTextureWidth.Increment = new decimal(new int[] {
        128,
        0,
        0,
        0});
        this.NumericTextureWidth.Location = new System.Drawing.Point(3, 24);
        this.NumericTextureWidth.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTextureWidth.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureWidth.Name = "NumericTextureWidth";
        this.NumericTextureWidth.Size = new System.Drawing.Size(87, 23);
        this.NumericTextureWidth.TabIndex = 3;
        this.NumericTextureWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericTextureWidth.Value = new decimal(new int[] {
        512,
        0,
        0,
        0});
        // 
        // NumericTextureHeight
        // 
        this.NumericTextureHeight.BackColor = System.Drawing.Color.White;
        this.NumericTextureHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericTextureHeight.ForeColor = System.Drawing.Color.Black;
        this.NumericTextureHeight.Increment = new decimal(new int[] {
        128,
        0,
        0,
        0});
        this.NumericTextureHeight.Location = new System.Drawing.Point(96, 24);
        this.NumericTextureHeight.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTextureHeight.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureHeight.Name = "NumericTextureHeight";
        this.NumericTextureHeight.Size = new System.Drawing.Size(87, 23);
        this.NumericTextureHeight.TabIndex = 4;
        this.NumericTextureHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericTextureHeight.Value = new decimal(new int[] {
        512,
        0,
        0,
        0});
        // 
        // FontTextureSizeView
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        this.Name = "FontTextureSizeView";
        this.Size = new System.Drawing.Size(254, 177);
        this.Text = "Font Texture Size";
        this.PanelBody.ResumeLayout(false);
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label LabelTextureHeight;
    private System.Windows.Forms.Label LabelTextureWidth;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.NumericUpDown NumericTextureHeight;
    private System.Windows.Forms.NumericUpDown NumericTextureWidth;
}
