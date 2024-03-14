using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.FontEditor;

partial class FontTextureBrushView
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
        this.ComboWrapMode = new System.Windows.Forms.ComboBox();
        this.LabelTextureWrap = new System.Windows.Forms.Label();
        this.LabelTop = new System.Windows.Forms.Label();
        this.LabelLeft = new System.Windows.Forms.Label();
        this.LabelRight = new System.Windows.Forms.Label();
        this.LabelBottom = new System.Windows.Forms.Label();
        this.NumericLeft = new System.Windows.Forms.NumericUpDown();
        this.NumericTop = new System.Windows.Forms.NumericUpDown();
        this.NumericRight = new System.Windows.Forms.NumericUpDown();
        this.NumericBottom = new System.Windows.Forms.NumericUpDown();
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
        this.label1 = new System.Windows.Forms.Label();
        this.PanelBody.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).BeginInit();
        this.TableControls.SuspendLayout();
        this.tableLayoutPanel2.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableControls);
        this.PanelBody.Size = new System.Drawing.Size(364, 623);
        // 
        // ComboWrapMode
        // 
        this.ComboWrapMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ComboWrapMode.BackColor = System.Drawing.Color.White;
        this.ComboWrapMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.ComboWrapMode.ForeColor = System.Drawing.Color.Black;
        this.ComboWrapMode.FormattingEnabled = true;
        this.ComboWrapMode.Location = new System.Drawing.Point(3, 180);
        this.ComboWrapMode.Name = "ComboWrapMode";
        this.ComboWrapMode.Size = new System.Drawing.Size(358, 23);
        this.ComboWrapMode.TabIndex = 4;
        // 
        // LabelTextureWrap
        // 
        this.LabelTextureWrap.AutoSize = true;
        this.LabelTextureWrap.Location = new System.Drawing.Point(3, 159);
        this.LabelTextureWrap.Margin = new System.Windows.Forms.Padding(3);
        this.LabelTextureWrap.Name = "LabelTextureWrap";
        this.LabelTextureWrap.Size = new System.Drawing.Size(62, 15);
        this.LabelTextureWrap.TabIndex = 3;
        this.LabelTextureWrap.Text = "Wrapping:";
        // 
        // LabelTop
        // 
        this.LabelTop.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        this.LabelTop.AutoSize = true;
        this.LabelTop.Location = new System.Drawing.Point(132, 3);
        this.LabelTop.Margin = new System.Windows.Forms.Padding(3);
        this.LabelTop.Name = "LabelTop";
        this.LabelTop.Size = new System.Drawing.Size(26, 15);
        this.LabelTop.TabIndex = 0;
        this.LabelTop.Text = "Top";
        // 
        // LabelLeft
        // 
        this.LabelLeft.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelLeft.AutoSize = true;
        this.LabelLeft.Location = new System.Drawing.Point(3, 57);
        this.LabelLeft.Margin = new System.Windows.Forms.Padding(3);
        this.LabelLeft.Name = "LabelLeft";
        this.LabelLeft.Size = new System.Drawing.Size(27, 15);
        this.LabelLeft.TabIndex = 2;
        this.LabelLeft.Text = "Left";
        // 
        // LabelRight
        // 
        this.LabelRight.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelRight.AutoSize = true;
        this.LabelRight.Location = new System.Drawing.Point(261, 57);
        this.LabelRight.Margin = new System.Windows.Forms.Padding(3);
        this.LabelRight.Name = "LabelRight";
        this.LabelRight.Size = new System.Drawing.Size(35, 15);
        this.LabelRight.TabIndex = 6;
        this.LabelRight.Text = "Right";
        // 
        // LabelBottom
        // 
        this.LabelBottom.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.LabelBottom.AutoSize = true;
        this.LabelBottom.Location = new System.Drawing.Point(122, 111);
        this.LabelBottom.Margin = new System.Windows.Forms.Padding(3);
        this.LabelBottom.Name = "LabelBottom";
        this.LabelBottom.Size = new System.Drawing.Size(47, 15);
        this.LabelBottom.TabIndex = 4;
        this.LabelBottom.Text = "Bottom";
        // 
        // NumericLeft
        // 
        this.NumericLeft.BackColor = System.Drawing.Color.White;
        this.NumericLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericLeft.Location = new System.Drawing.Point(36, 53);
        this.NumericLeft.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericLeft.Name = "NumericLeft";
        this.NumericLeft.Size = new System.Drawing.Size(69, 23);
        this.NumericLeft.TabIndex = 3;
        this.NumericLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // NumericTop
        // 
        this.NumericTop.BackColor = System.Drawing.Color.White;
        this.NumericTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericTop.Location = new System.Drawing.Point(111, 24);
        this.NumericTop.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTop.Name = "NumericTop";
        this.NumericTop.Size = new System.Drawing.Size(69, 23);
        this.NumericTop.TabIndex = 1;
        this.NumericTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // NumericRight
        // 
        this.NumericRight.BackColor = System.Drawing.Color.White;
        this.NumericRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericRight.Location = new System.Drawing.Point(186, 53);
        this.NumericRight.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericRight.Name = "NumericRight";
        this.NumericRight.Size = new System.Drawing.Size(69, 23);
        this.NumericRight.TabIndex = 7;
        this.NumericRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // NumericBottom
        // 
        this.NumericBottom.BackColor = System.Drawing.Color.White;
        this.NumericBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericBottom.Location = new System.Drawing.Point(111, 82);
        this.NumericBottom.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericBottom.Name = "NumericBottom";
        this.NumericBottom.Size = new System.Drawing.Size(69, 23);
        this.NumericBottom.TabIndex = 5;
        this.NumericBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // TableControls
        // 
        this.TableControls.AutoSize = true;
        this.TableControls.ColumnCount = 1;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableControls.Controls.Add(this.tableLayoutPanel2, 0, 1);
        this.TableControls.Controls.Add(this.label1, 0, 0);
        this.TableControls.Controls.Add(this.LabelTextureWrap, 0, 2);
        this.TableControls.Controls.Add(this.ComboWrapMode, 0, 3);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(0, 0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 4;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.Size = new System.Drawing.Size(364, 623);
        this.TableControls.TabIndex = 0;
        // 
        // tableLayoutPanel2
        // 
        this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.tableLayoutPanel2.AutoSize = true;
        this.tableLayoutPanel2.ColumnCount = 5;
        this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel2.Controls.Add(this.NumericTop, 2, 1);
        this.tableLayoutPanel2.Controls.Add(this.NumericLeft, 1, 2);
        this.tableLayoutPanel2.Controls.Add(this.LabelRight, 4, 2);
        this.tableLayoutPanel2.Controls.Add(this.LabelBottom, 2, 4);
        this.tableLayoutPanel2.Controls.Add(this.LabelLeft, 0, 2);
        this.tableLayoutPanel2.Controls.Add(this.NumericBottom, 2, 3);
        this.tableLayoutPanel2.Controls.Add(this.NumericRight, 3, 2);
        this.tableLayoutPanel2.Controls.Add(this.LabelTop, 2, 0);
        this.tableLayoutPanel2.Location = new System.Drawing.Point(32, 24);
        this.tableLayoutPanel2.Name = "tableLayoutPanel2";
        this.tableLayoutPanel2.RowCount = 5;
        this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel2.Size = new System.Drawing.Size(299, 129);
        this.tableLayoutPanel2.TabIndex = 1;
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 3);
        this.label1.Margin = new System.Windows.Forms.Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(88, 15);
        this.label1.TabIndex = 0;
        this.label1.Text = "Texture Region:";
        // 
        // FontTextureBrushView
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Name = "FontTextureBrushView";
        this.Size = new System.Drawing.Size(364, 680);
        this.Text = "Font Brush - Texture";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).EndInit();
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.tableLayoutPanel2.ResumeLayout(false);
        this.tableLayoutPanel2.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.Label LabelTextureWrap;
    private System.Windows.Forms.ComboBox ComboWrapMode;
    private System.Windows.Forms.NumericUpDown NumericBottom;
    private System.Windows.Forms.NumericUpDown NumericRight;
    private System.Windows.Forms.NumericUpDown NumericTop;
    private System.Windows.Forms.NumericUpDown NumericLeft;
    private System.Windows.Forms.Label LabelBottom;
    private System.Windows.Forms.Label LabelRight;
    private System.Windows.Forms.Label LabelLeft;
    private System.Windows.Forms.Label LabelTop;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel TableControls;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
}
