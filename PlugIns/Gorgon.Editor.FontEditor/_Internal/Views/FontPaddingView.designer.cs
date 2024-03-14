using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.FontEditor;

partial class FontPaddingView
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



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.LabelTextureWidth = new System.Windows.Forms.Label();
        this.NumericPadding = new System.Windows.Forms.NumericUpDown();
        this.PanelBody.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericPadding)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.tableLayoutPanel1);
        this.PanelBody.Size = new System.Drawing.Size(254, 120);
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.ColumnCount = 1;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.Controls.Add(this.LabelTextureWidth, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.NumericPadding, 0, 1);
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
        this.LabelTextureWidth.Size = new System.Drawing.Size(194, 15);
        this.LabelTextureWidth.TabIndex = 1;
        this.LabelTextureWidth.Text = "Padding Around Glyphs on Texture:";
        // 
        // NumericPadding
        // 
        this.NumericPadding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericPadding.BackColor = System.Drawing.Color.White;
        this.NumericPadding.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericPadding.ForeColor = System.Drawing.Color.Black;
        this.NumericPadding.Location = new System.Drawing.Point(3, 24);
        this.NumericPadding.Maximum = new decimal(new int[] {
        8,
        0,
        0,
        0});
        this.NumericPadding.Name = "NumericPadding";
        this.NumericPadding.Size = new System.Drawing.Size(248, 23);
        this.NumericPadding.TabIndex = 3;
        this.NumericPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericPadding.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        // 
        // FontPaddingView
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        this.Name = "FontPaddingView";
        this.Size = new System.Drawing.Size(254, 177);
        this.Text = "Font Glyph Texture Padding";
        this.PanelBody.ResumeLayout(false);
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericPadding)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private System.Windows.Forms.Label LabelTextureWidth;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.NumericUpDown NumericPadding;
}
