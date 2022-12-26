namespace Gorgon.UI;

partial class GorgonSearchBox
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
        this.components = new System.ComponentModel.Container();
        this.ButtonClearSearch = new System.Windows.Forms.Button();
        this.TextSearch = new Gorgon.UI.GorgonCueTextBox();
        this.TipSearch = new System.Windows.Forms.ToolTip(this.components);
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.tableLayoutPanel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // ButtonClearSearch
        // 
        this.ButtonClearSearch.AutoSize = true;
        this.ButtonClearSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonClearSearch.BackColor = System.Drawing.Color.Transparent;
        this.ButtonClearSearch.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ButtonClearSearch.FlatAppearance.BorderSize = 0;
        this.ButtonClearSearch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonClearSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(86)))), ((int)(((byte)(86)))));
        this.ButtonClearSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonClearSearch.Font = new System.Drawing.Font("Marlett", 9F);
        this.ButtonClearSearch.Location = new System.Drawing.Point(259, 0);
        this.ButtonClearSearch.Margin = new System.Windows.Forms.Padding(0);
        this.ButtonClearSearch.Name = "ButtonClearSearch";
        this.ButtonClearSearch.Size = new System.Drawing.Size(30, 24);
        this.ButtonClearSearch.TabIndex = 1;
        this.ButtonClearSearch.Text = "r";
        this.ButtonClearSearch.UseVisualStyleBackColor = false;
        this.ButtonClearSearch.Visible = false;
        this.ButtonClearSearch.Click += new System.EventHandler(this.ButtonClearSearch_Click);
        // 
        // TextSearch
        // 
        this.TextSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TextSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.TextSearch.CueText = "Search...";
        this.TextSearch.Location = new System.Drawing.Point(3, 3);
        this.TextSearch.Name = "TextSearch";
        this.TextSearch.Size = new System.Drawing.Size(253, 16);
        this.TextSearch.TabIndex = 0;
        this.TextSearch.TextChanged += new System.EventHandler(this.TextSearch_TextChanged);
        this.TextSearch.Enter += new System.EventHandler(this.TextSearch_Enter);
        this.TextSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextSearch_KeyUp);
        // 
        // TipSearch
        // 
        this.TipSearch.AutoPopDelay = 15000;
        this.TipSearch.InitialDelay = 2500;
        this.TipSearch.ReshowDelay = 1000;
        this.TipSearch.ToolTipTitle = "Search";
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.AutoSize = true;
        this.tableLayoutPanel1.ColumnCount = 2;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.Controls.Add(this.ButtonClearSearch, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.TextSearch, 0, 0);
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 1;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(289, 24);
        this.tableLayoutPanel1.TabIndex = 1;
        // 
        // GorgonSearchBox
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.White;
        this.Controls.Add(this.tableLayoutPanel1);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "GorgonSearchBox";
        this.Size = new System.Drawing.Size(289, 24);
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private GorgonCueTextBox TextSearch;
    private System.Windows.Forms.Button ButtonClearSearch;
    private System.Windows.Forms.ToolTip TipSearch;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
}
