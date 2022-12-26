namespace Gorgon.Editor.ImageEditor;

partial class Fx1BitSettings
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
        this.TableOptions = new System.Windows.Forms.TableLayoutPanel();
        this.TrackMaxThreshold = new System.Windows.Forms.TrackBar();
        this.CheckInvert = new System.Windows.Forms.CheckBox();
        this.LabelMinThreshold = new System.Windows.Forms.Label();
        this.LabelMaxThreshold = new System.Windows.Forms.Label();
        this.NumericMinThreshold = new System.Windows.Forms.NumericUpDown();
        this.NumericMaxThreshold = new System.Windows.Forms.NumericUpDown();
        this.TrackMinThreshold = new System.Windows.Forms.TrackBar();
        this.PanelBody.SuspendLayout();
        this.TableOptions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TrackMaxThreshold)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMinThreshold)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMaxThreshold)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.TrackMinThreshold)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableOptions);
        this.PanelBody.Size = new System.Drawing.Size(300, 229);
        // 
        // TableOptions
        // 
        this.TableOptions.AutoSize = true;
        this.TableOptions.ColumnCount = 1;
        this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableOptions.Controls.Add(this.TrackMaxThreshold, 0, 5);
        this.TableOptions.Controls.Add(this.CheckInvert, 0, 6);
        this.TableOptions.Controls.Add(this.LabelMinThreshold, 0, 0);
        this.TableOptions.Controls.Add(this.LabelMaxThreshold, 0, 3);
        this.TableOptions.Controls.Add(this.NumericMinThreshold, 0, 1);
        this.TableOptions.Controls.Add(this.NumericMaxThreshold, 0, 4);
        this.TableOptions.Controls.Add(this.TrackMinThreshold, 0, 2);
        this.TableOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableOptions.Location = new System.Drawing.Point(0, 0);
        this.TableOptions.Name = "TableOptions";
        this.TableOptions.RowCount = 7;
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.Size = new System.Drawing.Size(300, 229);
        this.TableOptions.TabIndex = 0;
        // 
        // TrackMaxThreshold
        // 
        this.TrackMaxThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TrackMaxThreshold.LargeChange = 16;
        this.TrackMaxThreshold.Location = new System.Drawing.Point(3, 151);
        this.TrackMaxThreshold.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.TrackMaxThreshold.Maximum = 255;
        this.TrackMaxThreshold.Name = "TrackMaxThreshold";
        this.TrackMaxThreshold.Size = new System.Drawing.Size(294, 45);
        this.TrackMaxThreshold.TabIndex = 5;
        this.TrackMaxThreshold.TickFrequency = 16;
        this.TrackMaxThreshold.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
        this.TrackMaxThreshold.Value = 255;
        this.TrackMaxThreshold.ValueChanged += new System.EventHandler(this.TrackMaxThresold_ValueChanged);
        // 
        // CheckInvert
        // 
        this.CheckInvert.AutoSize = true;
        this.CheckInvert.Location = new System.Drawing.Point(3, 199);
        this.CheckInvert.Name = "CheckInvert";
        this.CheckInvert.Size = new System.Drawing.Size(56, 19);
        this.CheckInvert.TabIndex = 6;
        this.CheckInvert.Text = "Invert";
        this.CheckInvert.UseVisualStyleBackColor = true;
        this.CheckInvert.Click += new System.EventHandler(this.CheckInvert_Click);
        // 
        // LabelMinThreshold
        // 
        this.LabelMinThreshold.AutoSize = true;
        this.LabelMinThreshold.Location = new System.Drawing.Point(3, 3);
        this.LabelMinThreshold.Margin = new System.Windows.Forms.Padding(3);
        this.LabelMinThreshold.Name = "LabelMinThreshold";
        this.LabelMinThreshold.Size = new System.Drawing.Size(111, 15);
        this.LabelMinThreshold.TabIndex = 0;
        this.LabelMinThreshold.Text = "Min. thresold value:";
        // 
        // LabelMaxThreshold
        // 
        this.LabelMaxThreshold.AutoSize = true;
        this.LabelMaxThreshold.Location = new System.Drawing.Point(3, 101);
        this.LabelMaxThreshold.Margin = new System.Windows.Forms.Padding(3);
        this.LabelMaxThreshold.Name = "LabelMaxThreshold";
        this.LabelMaxThreshold.Size = new System.Drawing.Size(120, 15);
        this.LabelMaxThreshold.TabIndex = 3;
        this.LabelMaxThreshold.Text = "Max. threshold value:";
        // 
        // NumericMinThreshold
        // 
        this.NumericMinThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericMinThreshold.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericMinThreshold.Location = new System.Drawing.Point(3, 24);
        this.NumericMinThreshold.Maximum = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericMinThreshold.Name = "NumericMinThreshold";
        this.NumericMinThreshold.Size = new System.Drawing.Size(294, 23);
        this.NumericMinThreshold.TabIndex = 1;
        this.NumericMinThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericMinThreshold.Value = new decimal(new int[] {
        127,
        0,
        0,
        0});
        this.NumericMinThreshold.ValueChanged += new System.EventHandler(this.NumericMinThreshold_ValueChanged);
        // 
        // NumericMaxThreshold
        // 
        this.NumericMaxThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericMaxThreshold.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericMaxThreshold.Location = new System.Drawing.Point(3, 122);
        this.NumericMaxThreshold.Maximum = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericMaxThreshold.Name = "NumericMaxThreshold";
        this.NumericMaxThreshold.Size = new System.Drawing.Size(294, 23);
        this.NumericMaxThreshold.TabIndex = 4;
        this.NumericMaxThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericMaxThreshold.Value = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericMaxThreshold.ValueChanged += new System.EventHandler(this.NumericMaxThreshold_ValueChanged);
        // 
        // TrackMinThreshold
        // 
        this.TrackMinThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TrackMinThreshold.LargeChange = 16;
        this.TrackMinThreshold.Location = new System.Drawing.Point(3, 53);
        this.TrackMinThreshold.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.TrackMinThreshold.Maximum = 255;
        this.TrackMinThreshold.Name = "TrackMinThreshold";
        this.TrackMinThreshold.Size = new System.Drawing.Size(294, 45);
        this.TrackMinThreshold.TabIndex = 2;
        this.TrackMinThreshold.TickFrequency = 16;
        this.TrackMinThreshold.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
        this.TrackMinThreshold.Value = 127;
        this.TrackMinThreshold.ValueChanged += new System.EventHandler(this.TrackMinThreshold_ValueChanged);
        // 
        // Fx1BitSettings
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "Fx1BitSettings";
        this.Size = new System.Drawing.Size(300, 286);
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.TableOptions.ResumeLayout(false);
        this.TableOptions.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TrackMaxThreshold)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMinThreshold)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMaxThreshold)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.TrackMinThreshold)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel TableOptions;
    private System.Windows.Forms.CheckBox CheckInvert;
    private System.Windows.Forms.Label LabelMinThreshold;
    private System.Windows.Forms.Label LabelMaxThreshold;
    private System.Windows.Forms.NumericUpDown NumericMinThreshold;
    private System.Windows.Forms.NumericUpDown NumericMaxThreshold;
    private System.Windows.Forms.TrackBar TrackMinThreshold;
    private System.Windows.Forms.TrackBar TrackMaxThreshold;
}
