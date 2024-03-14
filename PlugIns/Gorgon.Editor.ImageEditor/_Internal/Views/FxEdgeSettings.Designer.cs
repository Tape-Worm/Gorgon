namespace Gorgon.Editor.ImageEditor;

partial class FxEdgeSettings
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FxEdgeSettings));
        this.label1 = new System.Windows.Forms.Label();
        this.TrackThreshold = new System.Windows.Forms.TrackBar();
        this.label2 = new System.Windows.Forms.Label();
        this.NumericOffset = new System.Windows.Forms.NumericUpDown();
        this.PickerLineColor = new Fetze.WinFormsColor.ColorPanel();
        this.SliderAlpha = new Fetze.WinFormsColor.ColorSlider();
        this.ColorPreview = new Fetze.WinFormsColor.ColorShowBox();
        this.label3 = new System.Windows.Forms.Label();
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.TableColor = new System.Windows.Forms.TableLayoutPanel();
        this.LabelColorValue = new System.Windows.Forms.Label();
        this.CheckOverlay = new System.Windows.Forms.CheckBox();
        this.PanelBody.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TrackThreshold)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericOffset)).BeginInit();
        this.TableControls.SuspendLayout();
        this.TableColor.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableControls);
        this.PanelBody.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.PanelBody.Size = new System.Drawing.Size(332, 492);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 0);
        this.label1.Name = "label1";
        this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.label1.Size = new System.Drawing.Size(62, 21);
        this.label1.TabIndex = 0;
        this.label1.Text = "Threshold:";
        this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // TrackThreshold
        // 
        this.TrackThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TrackThreshold.Location = new System.Drawing.Point(3, 24);
        this.TrackThreshold.Maximum = 100;
        this.TrackThreshold.Name = "TrackThreshold";
        this.TrackThreshold.Size = new System.Drawing.Size(320, 45);
        this.TrackThreshold.TabIndex = 1;
        this.TrackThreshold.TickFrequency = 10;
        this.TrackThreshold.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
        this.TrackThreshold.Value = 50;
        this.TrackThreshold.ValueChanged += new System.EventHandler(this.TrackThreshold_ValueChanged);
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(3, 72);
        this.label2.Name = "label2";
        this.label2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.label2.Size = new System.Drawing.Size(42, 21);
        this.label2.TabIndex = 2;
        this.label2.Text = "Offset:";
        this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // NumericOffset
        // 
        this.NumericOffset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericOffset.AutoSize = true;
        this.NumericOffset.BackColor = System.Drawing.Color.White;
        this.NumericOffset.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericOffset.DecimalPlaces = 3;
        this.NumericOffset.ForeColor = System.Drawing.Color.Black;
        this.NumericOffset.Increment = new decimal(new int[] {
        1,
        0,
        0,
        196608});
        this.NumericOffset.Location = new System.Drawing.Point(3, 96);
        this.NumericOffset.Maximum = new decimal(new int[] {
        4,
        0,
        0,
        0});
        this.NumericOffset.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        196608});
        this.NumericOffset.Name = "NumericOffset";
        this.NumericOffset.Size = new System.Drawing.Size(320, 23);
        this.NumericOffset.TabIndex = 3;
        this.NumericOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericOffset.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericOffset.ValueChanged += new System.EventHandler(this.NumericOffsetAmount_ValueChanged);
        // 
        // PickerLineColor
        // 
        this.PickerLineColor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PickerLineColor.Location = new System.Drawing.Point(3, 3);
        this.PickerLineColor.Name = "PickerLineColor";
        this.PickerLineColor.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
        this.PickerLineColor.Size = new System.Drawing.Size(264, 220);
        this.PickerLineColor.TabIndex = 0;        
        this.PickerLineColor.ValueChanged += new System.EventHandler(this.PickerLineColor_ValueChanged);
        // 
        // SliderAlpha
        // 
        this.SliderAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
        this.SliderAlpha.Location = new System.Drawing.Point(277, 3);
        this.SliderAlpha.Maximum = System.Drawing.Color.Black;
        this.SliderAlpha.Minimum = System.Drawing.Color.Transparent;
        this.SliderAlpha.Name = "SliderAlpha";
        this.SliderAlpha.Size = new System.Drawing.Size(36, 220);
        this.SliderAlpha.TabIndex = 1;
        this.SliderAlpha.ValuePercentual = 1F;
        this.SliderAlpha.ValueChanged += new System.EventHandler(this.SliderAlpha_ValueChanged);
        // 
        // ColorPreview
        // 
        this.ColorPreview.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.ColorPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.ColorPreview.Color = System.Drawing.Color.Black;
        this.ColorPreview.Location = new System.Drawing.Point(273, 229);
        this.ColorPreview.LowerColor = System.Drawing.Color.Black;
        this.ColorPreview.Name = "ColorPreview";
        this.ColorPreview.Size = new System.Drawing.Size(44, 32);
        this.ColorPreview.TabIndex = 3;
        this.ColorPreview.UpperColor = System.Drawing.Color.Black;
        // 
        // label3
        // 
        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(3, 122);
        this.label3.Name = "label3";
        this.label3.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.label3.Size = new System.Drawing.Size(62, 21);
        this.label3.TabIndex = 4;
        this.label3.Text = "Line color:";
        this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // TableControls
        // 
        this.TableControls.ColumnCount = 1;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableControls.Controls.Add(this.label1, 0, 0);
        this.TableControls.Controls.Add(this.TrackThreshold, 0, 1);
        this.TableControls.Controls.Add(this.label3, 0, 4);
        this.TableControls.Controls.Add(this.label2, 0, 2);
        this.TableControls.Controls.Add(this.NumericOffset, 0, 3);
        this.TableControls.Controls.Add(this.TableColor, 0, 5);
        this.TableControls.Controls.Add(this.CheckOverlay, 0, 6);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(3, 0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 7;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableControls.Size = new System.Drawing.Size(326, 492);
        this.TableControls.TabIndex = 0;
        // 
        // TableColor
        // 
        this.TableColor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TableColor.ColumnCount = 2;
        this.TableColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableColor.Controls.Add(this.LabelColorValue, 0, 1);
        this.TableColor.Controls.Add(this.SliderAlpha, 1, 0);
        this.TableColor.Controls.Add(this.PickerLineColor, 0, 0);
        this.TableColor.Controls.Add(this.ColorPreview, 1, 1);
        this.TableColor.Location = new System.Drawing.Point(3, 146);
        this.TableColor.Name = "TableColor";
        this.TableColor.RowCount = 3;
        this.TableColor.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableColor.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableColor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
        this.TableColor.Size = new System.Drawing.Size(320, 273);
        this.TableColor.TabIndex = 5;
        // 
        // LabelColorValue
        // 
        this.LabelColorValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelColorValue.AutoSize = true;
        this.LabelColorValue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.LabelColorValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.LabelColorValue.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.LabelColorValue.Location = new System.Drawing.Point(3, 234);
        this.LabelColorValue.Name = "LabelColorValue";
        this.LabelColorValue.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.LabelColorValue.Size = new System.Drawing.Size(264, 21);
        this.LabelColorValue.TabIndex = 2;
        this.LabelColorValue.Text = "Red: 255 Green: 255 Blue: 255 Alpha: 255";
        this.LabelColorValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // CheckOverlay
        // 
        this.CheckOverlay.AutoSize = true;
        this.CheckOverlay.Checked = true;
        this.CheckOverlay.CheckState = System.Windows.Forms.CheckState.Checked;
        this.CheckOverlay.Location = new System.Drawing.Point(3, 425);
        this.CheckOverlay.Name = "CheckOverlay";
        this.CheckOverlay.Size = new System.Drawing.Size(201, 19);
        this.CheckOverlay.TabIndex = 6;
        this.CheckOverlay.Text = "Overlay edges on original image?";
        this.CheckOverlay.UseVisualStyleBackColor = true;
        this.CheckOverlay.Click += new System.EventHandler(this.CheckOverlay_Click);
        // 
        // FxEdgeSettings
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "FxEdgeSettings";
        this.Size = new System.Drawing.Size(332, 549);
        this.PanelBody.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.TrackThreshold)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericOffset)).EndInit();
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.TableColor.ResumeLayout(false);
        this.TableColor.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TrackBar TrackThreshold;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown NumericOffset;
    private Fetze.WinFormsColor.ColorPanel PickerLineColor;
    private Fetze.WinFormsColor.ColorSlider SliderAlpha;
    private System.Windows.Forms.Label label3;
    private Fetze.WinFormsColor.ColorShowBox ColorPreview;
    private System.Windows.Forms.TableLayoutPanel TableControls;
    private System.Windows.Forms.TableLayoutPanel TableColor;
    private System.Windows.Forms.Label LabelColorValue;
    private System.Windows.Forms.CheckBox CheckOverlay;
}
