using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.FontEditor;

partial class FontOutlineView
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
        this.LabelSize = new System.Windows.Forms.Label();
        this.LabelStartColor = new System.Windows.Forms.Label();
        this.LabelEndColor = new System.Windows.Forms.Label();
        this.ColorEnd = new Gorgon.Editor.UI.Controls.ColorPicker();
        this.ColorStart = new Gorgon.Editor.UI.Controls.ColorPicker();
        this.NumericSize = new System.Windows.Forms.NumericUpDown();
        this.PanelBody.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericSize)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.tableLayoutPanel1);
        this.PanelBody.Size = new System.Drawing.Size(364, 734);
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.ColumnCount = 1;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.Controls.Add(this.LabelSize, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.LabelStartColor, 0, 2);
        this.tableLayoutPanel1.Controls.Add(this.LabelEndColor, 0, 4);
        this.tableLayoutPanel1.Controls.Add(this.ColorEnd, 0, 5);
        this.tableLayoutPanel1.Controls.Add(this.ColorStart, 0, 3);
        this.tableLayoutPanel1.Controls.Add(this.NumericSize, 0, 1);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 7;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(364, 734);
        this.tableLayoutPanel1.TabIndex = 0;
        // 
        // LabelSize
        // 
        this.LabelSize.AutoSize = true;
        this.LabelSize.Location = new System.Drawing.Point(3, 3);
        this.LabelSize.Margin = new System.Windows.Forms.Padding(3);
        this.LabelSize.Name = "LabelSize";
        this.LabelSize.Size = new System.Drawing.Size(113, 15);
        this.LabelSize.TabIndex = 0;
        this.LabelSize.Text = "Outline Size (Pixels):";
        // 
        // LabelStartColor
        // 
        this.LabelStartColor.AutoSize = true;
        this.LabelStartColor.Enabled = false;
        this.LabelStartColor.Location = new System.Drawing.Point(3, 47);
        this.LabelStartColor.Margin = new System.Windows.Forms.Padding(3);
        this.LabelStartColor.Name = "LabelStartColor";
        this.LabelStartColor.Size = new System.Drawing.Size(66, 15);
        this.LabelStartColor.TabIndex = 2;
        this.LabelStartColor.Text = "Start Color:";
        // 
        // LabelEndColor
        // 
        this.LabelEndColor.AutoSize = true;
        this.LabelEndColor.Enabled = false;
        this.LabelEndColor.Location = new System.Drawing.Point(3, 362);
        this.LabelEndColor.Margin = new System.Windows.Forms.Padding(3);
        this.LabelEndColor.Name = "LabelEndColor";
        this.LabelEndColor.Size = new System.Drawing.Size(62, 15);
        this.LabelEndColor.TabIndex = 4;
        this.LabelEndColor.Text = "End Color:";
        // 
        // ColorEnd
        // 
        this.ColorEnd.AutoSize = true;
        this.ColorEnd.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ColorEnd.Enabled = false;
        this.ColorEnd.Location = new System.Drawing.Point(0, 383);
        this.ColorEnd.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.ColorEnd.Name = "ColorEnd";
        this.ColorEnd.Size = new System.Drawing.Size(364, 288);
        this.ColorEnd.TabIndex = 5;
        // 
        // ColorStart
        // 
        this.ColorStart.AutoSize = true;
        this.ColorStart.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ColorStart.Enabled = false;
        this.ColorStart.Location = new System.Drawing.Point(0, 68);
        this.ColorStart.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.ColorStart.Name = "ColorStart";
        this.ColorStart.Size = new System.Drawing.Size(364, 288);
        this.ColorStart.TabIndex = 3;
        // 
        // NumericSize
        // 
        this.NumericSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.NumericSize.BackColor = System.Drawing.Color.White;
        this.NumericSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericSize.ForeColor = System.Drawing.Color.Black;
        this.NumericSize.Location = new System.Drawing.Point(3, 21);
        this.NumericSize.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.NumericSize.Maximum = new decimal(new int[] {
        9,
        0,
        0,
        0});
        this.NumericSize.Name = "NumericSize";
        this.NumericSize.Size = new System.Drawing.Size(120, 23);
        this.NumericSize.TabIndex = 1;
        this.NumericSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // FontOutlineView
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        this.IsModal = false;
        this.Name = "FontOutlineView";
        this.Size = new System.Drawing.Size(364, 755);
        this.Text = "Font Outline";
        this.PanelBody.ResumeLayout(false);
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericSize)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label LabelSize;
    private System.Windows.Forms.Label LabelStartColor;
    private System.Windows.Forms.Label LabelEndColor;
    private ColorPicker ColorStart;
    private ColorPicker ColorEnd;
    private System.Windows.Forms.NumericUpDown NumericSize;
}
