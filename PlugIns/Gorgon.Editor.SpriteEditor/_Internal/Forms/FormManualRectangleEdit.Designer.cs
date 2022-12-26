namespace Gorgon.Editor.SpriteEditor;

partial class FormManualRectangleEdit
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManualRectangleEdit));
        this.PanelRect = new System.Windows.Forms.Panel();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.label1 = new System.Windows.Forms.Label();
        this.NumericTop = new System.Windows.Forms.NumericUpDown();
        this.LabelBottom = new System.Windows.Forms.Label();
        this.NumericBottom = new System.Windows.Forms.NumericUpDown();
        this.panel1 = new System.Windows.Forms.Panel();
        this.LabelRight = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.NumericRight = new System.Windows.Forms.NumericUpDown();
        this.NumericLeft = new System.Windows.Forms.NumericUpDown();
        this.PanelRect.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelRect
        // 
        this.PanelRect.Controls.Add(this.tableLayoutPanel1);
        this.PanelRect.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelRect.Location = new System.Drawing.Point(0, 0);
        this.PanelRect.Name = "PanelRect";
        this.PanelRect.Size = new System.Drawing.Size(358, 243);
        this.PanelRect.TabIndex = 2;
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.AutoSize = true;
        this.tableLayoutPanel1.ColumnCount = 3;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.Controls.Add(this.label1, 1, 1);
        this.tableLayoutPanel1.Controls.Add(this.NumericTop, 1, 2);
        this.tableLayoutPanel1.Controls.Add(this.LabelBottom, 1, 7);
        this.tableLayoutPanel1.Controls.Add(this.NumericBottom, 1, 6);
        this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 3);
        this.tableLayoutPanel1.Controls.Add(this.LabelRight, 2, 3);
        this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
        this.tableLayoutPanel1.Controls.Add(this.NumericRight, 2, 4);
        this.tableLayoutPanel1.Controls.Add(this.NumericLeft, 0, 4);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 9;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.357143F));
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.64286F));
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.64286F));
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.357143F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(358, 243);
        this.tableLayoutPanel1.TabIndex = 9;
        // 
        // label1
        // 
        this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(165, 9);
        this.label1.Margin = new System.Windows.Forms.Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(26, 15);
        this.label1.TabIndex = 5;
        this.label1.Text = "Top";
        // 
        // NumericTop
        // 
        this.NumericTop.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.NumericTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericTop.Location = new System.Drawing.Point(124, 30);
        this.NumericTop.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTop.Name = "NumericTop";
        this.NumericTop.Size = new System.Drawing.Size(108, 23);
        this.NumericTop.TabIndex = 3;
        this.NumericTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericTop.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
        // 
        // LabelBottom
        // 
        this.LabelBottom.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelBottom.AutoSize = true;
        this.LabelBottom.Location = new System.Drawing.Point(155, 217);
        this.LabelBottom.Margin = new System.Windows.Forms.Padding(3);
        this.LabelBottom.Name = "LabelBottom";
        this.LabelBottom.Size = new System.Drawing.Size(47, 15);
        this.LabelBottom.TabIndex = 8;
        this.LabelBottom.Text = "Bottom";
        // 
        // NumericBottom
        // 
        this.NumericBottom.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        this.NumericBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericBottom.Location = new System.Drawing.Point(124, 188);
        this.NumericBottom.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericBottom.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericBottom.Name = "NumericBottom";
        this.NumericBottom.Size = new System.Drawing.Size(108, 23);
        this.NumericBottom.TabIndex = 4;
        this.NumericBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericBottom.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericBottom.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
        // 
        // panel1
        // 
        this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.panel1.Location = new System.Drawing.Point(117, 59);
        this.panel1.MaximumSize = new System.Drawing.Size(123, 123);
        this.panel1.Name = "panel1";
        this.tableLayoutPanel1.SetRowSpan(this.panel1, 3);
        this.panel1.Size = new System.Drawing.Size(123, 123);
        this.panel1.TabIndex = 9;
        // 
        // LabelRight
        // 
        this.LabelRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        this.LabelRight.AutoSize = true;
        this.LabelRight.Location = new System.Drawing.Point(283, 88);
        this.LabelRight.Margin = new System.Windows.Forms.Padding(3);
        this.LabelRight.Name = "LabelRight";
        this.LabelRight.Size = new System.Drawing.Size(35, 15);
        this.LabelRight.TabIndex = 7;
        this.LabelRight.Text = "Right";
        // 
        // label2
        // 
        this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(43, 88);
        this.label2.Margin = new System.Windows.Forms.Padding(3);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(27, 15);
        this.label2.TabIndex = 6;
        this.label2.Text = "Left";
        // 
        // NumericRight
        // 
        this.NumericRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.NumericRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericRight.Location = new System.Drawing.Point(247, 109);
        this.NumericRight.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericRight.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericRight.Name = "NumericRight";
        this.NumericRight.Size = new System.Drawing.Size(108, 23);
        this.NumericRight.TabIndex = 2;
        this.NumericRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericRight.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericRight.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
        // 
        // NumericLeft
        // 
        this.NumericLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.NumericLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericLeft.Location = new System.Drawing.Point(3, 109);
        this.NumericLeft.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericLeft.Name = "NumericLeft";
        this.NumericLeft.Size = new System.Drawing.Size(108, 23);
        this.NumericLeft.TabIndex = 1;
        this.NumericLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericLeft.ValueChanged += new System.EventHandler(this.NumericLeft_ValueChanged);
        // 
        // FormManualRectangleEdit
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.AutoSize = true;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.ClientSize = new System.Drawing.Size(358, 243);
        this.Controls.Add(this.PanelRect);
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FormManualRectangleEdit";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Manual Sprite Clipping";
        this.PanelRect.ResumeLayout(false);
        this.PanelRect.PerformLayout();
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTop)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericBottom)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericRight)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericLeft)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel PanelRect;
    private System.Windows.Forms.Label LabelBottom;
    private System.Windows.Forms.Label LabelRight;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown NumericBottom;
    private System.Windows.Forms.NumericUpDown NumericTop;
    private System.Windows.Forms.NumericUpDown NumericRight;
    private System.Windows.Forms.NumericUpDown NumericLeft;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Panel panel1;
}