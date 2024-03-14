namespace Gorgon.Editor.ImageEditor;

partial class FxPosterizeSettings
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



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.TableOptions = new System.Windows.Forms.TableLayoutPanel();
        this.TrackAmount = new System.Windows.Forms.TrackBar();
        this.label1 = new System.Windows.Forms.Label();
        this.NumericAmount = new System.Windows.Forms.NumericUpDown();
        this.PanelBody.SuspendLayout();
        this.TableOptions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TrackAmount)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericAmount)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableOptions);
        this.PanelBody.Size = new System.Drawing.Size(300, 114);
        // 
        // TableOptions
        // 
        this.TableOptions.ColumnCount = 1;
        this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableOptions.Controls.Add(this.TrackAmount, 0, 2);
        this.TableOptions.Controls.Add(this.label1, 0, 0);
        this.TableOptions.Controls.Add(this.NumericAmount, 0, 1);
        this.TableOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableOptions.Location = new System.Drawing.Point(0, 0);
        this.TableOptions.Name = "TableOptions";
        this.TableOptions.RowCount = 3;
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableOptions.Size = new System.Drawing.Size(300, 114);
        this.TableOptions.TabIndex = 0;
        // 
        // TrackAmount
        // 
        this.TrackAmount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TrackAmount.LargeChange = 2;
        this.TrackAmount.Location = new System.Drawing.Point(3, 53);
        this.TrackAmount.Maximum = 255;
        this.TrackAmount.Minimum = 2;
        this.TrackAmount.Name = "TrackAmount";
        this.TrackAmount.Size = new System.Drawing.Size(294, 45);
        this.TrackAmount.TabIndex = 2;
        this.TrackAmount.TickFrequency = 16;
        this.TrackAmount.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
        this.TrackAmount.Value = 255;
        this.TrackAmount.ValueChanged += new System.EventHandler(this.TrackAmount_ValueChanged);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 3);
        this.label1.Margin = new System.Windows.Forms.Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(84, 15);
        this.label1.TabIndex = 0;
        this.label1.Text = "Posterize level:";
        // 
        // NumericAmount
        // 
        this.NumericAmount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericAmount.AutoSize = true;
        this.NumericAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericAmount.Location = new System.Drawing.Point(3, 24);
        this.NumericAmount.Maximum = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericAmount.Minimum = new decimal(new int[] {
        2,
        0,
        0,
        0});
        this.NumericAmount.Name = "NumericAmount";
        this.NumericAmount.Size = new System.Drawing.Size(294, 23);
        this.NumericAmount.TabIndex = 1;
        this.NumericAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericAmount.Value = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericAmount.ValueChanged += new System.EventHandler(this.NumericAmount_ValueChanged);
        // 
        // FxPosterizeSettings
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "FxPosterizeSettings";
        this.Size = new System.Drawing.Size(300, 171);
        this.PanelBody.ResumeLayout(false);
        this.TableOptions.ResumeLayout(false);
        this.TableOptions.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TrackAmount)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericAmount)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.TableLayoutPanel TableOptions;
    private System.Windows.Forms.TrackBar TrackAmount;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown NumericAmount;
}
