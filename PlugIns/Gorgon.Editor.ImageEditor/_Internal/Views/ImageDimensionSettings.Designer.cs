namespace Gorgon.Editor.ImageEditor
{
    partial class ImageDimensionSettings
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
            this.TableBody = new System.Windows.Forms.TableLayoutPanel();
            this.NumericMipLevels = new System.Windows.Forms.NumericUpDown();
            this.NumericDepthOrArray = new System.Windows.Forms.NumericUpDown();
            this.NumericHeight = new System.Windows.Forms.NumericUpDown();
            this.AlignmentPicker = new Gorgon.UI.GorgonAlignmentPicker();
            this.RadioCrop = new System.Windows.Forms.RadioButton();
            this.RadioResize = new System.Windows.Forms.RadioButton();
            this.LabelWidth = new System.Windows.Forms.Label();
            this.LabelHeight = new System.Windows.Forms.Label();
            this.LabelDepthOrArray = new System.Windows.Forms.Label();
            this.LabelMipLevels = new System.Windows.Forms.Label();
            this.NumericWidth = new System.Windows.Forms.NumericUpDown();
            this.LabelAnchor = new System.Windows.Forms.Label();
            this.PanelOptionsCaption = new System.Windows.Forms.Panel();
            this.LabelImageFilter = new System.Windows.Forms.Label();
            this.ComboImageFilter = new System.Windows.Forms.ComboBox();
            this.PanelBody.SuspendLayout();
            this.TableBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericMipLevels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericDepthOrArray)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.TableBody);
            this.PanelBody.Size = new System.Drawing.Size(299, 299);
            // 
            // TableBody
            // 
            this.TableBody.AutoSize = true;
            this.TableBody.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableBody.ColumnCount = 3;
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.TableBody.Controls.Add(this.NumericMipLevels, 1, 6);
            this.TableBody.Controls.Add(this.NumericDepthOrArray, 1, 4);
            this.TableBody.Controls.Add(this.NumericHeight, 1, 2);
            this.TableBody.Controls.Add(this.AlignmentPicker, 0, 9);
            this.TableBody.Controls.Add(this.RadioCrop, 0, 7);
            this.TableBody.Controls.Add(this.RadioResize, 1, 7);
            this.TableBody.Controls.Add(this.LabelWidth, 0, 0);
            this.TableBody.Controls.Add(this.LabelHeight, 0, 2);
            this.TableBody.Controls.Add(this.LabelDepthOrArray, 0, 4);
            this.TableBody.Controls.Add(this.LabelMipLevels, 0, 6);
            this.TableBody.Controls.Add(this.NumericWidth, 1, 0);
            this.TableBody.Controls.Add(this.LabelAnchor, 0, 8);
            this.TableBody.Controls.Add(this.PanelOptionsCaption, 0, 11);
            this.TableBody.Controls.Add(this.LabelImageFilter, 1, 8);
            this.TableBody.Controls.Add(this.ComboImageFilter, 1, 9);
            this.TableBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableBody.Location = new System.Drawing.Point(0, 0);
            this.TableBody.Name = "TableBody";
            this.TableBody.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.TableBody.RowCount = 12;
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.Size = new System.Drawing.Size(299, 299);
            this.TableBody.TabIndex = 1;
            // 
            // NumericMipLevels
            // 
            this.NumericMipLevels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericMipLevels.Location = new System.Drawing.Point(132, 95);
            this.NumericMipLevels.Margin = new System.Windows.Forms.Padding(0);
            this.NumericMipLevels.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.NumericMipLevels.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericMipLevels.Name = "NumericMipLevels";
            this.NumericMipLevels.Size = new System.Drawing.Size(151, 23);
            this.NumericMipLevels.TabIndex = 25;
            this.NumericMipLevels.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericMipLevels.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericMipLevels.ValueChanged += new System.EventHandler(this.NumericMipLevels_ValueChanged);
            // 
            // NumericDepthOrArray
            // 
            this.NumericDepthOrArray.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericDepthOrArray.Location = new System.Drawing.Point(132, 66);
            this.NumericDepthOrArray.Margin = new System.Windows.Forms.Padding(0);
            this.NumericDepthOrArray.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.NumericDepthOrArray.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericDepthOrArray.Name = "NumericDepthOrArray";
            this.NumericDepthOrArray.Size = new System.Drawing.Size(151, 23);
            this.NumericDepthOrArray.TabIndex = 24;
            this.NumericDepthOrArray.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericDepthOrArray.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericDepthOrArray.ValueChanged += new System.EventHandler(this.NumericDepthOrArray_ValueChanged);
            // 
            // NumericHeight
            // 
            this.NumericHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericHeight.Location = new System.Drawing.Point(132, 37);
            this.NumericHeight.Margin = new System.Windows.Forms.Padding(0);
            this.NumericHeight.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.NumericHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericHeight.Name = "NumericHeight";
            this.NumericHeight.Size = new System.Drawing.Size(151, 23);
            this.NumericHeight.TabIndex = 23;
            this.NumericHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericHeight.ValueChanged += new System.EventHandler(this.NumericHeight_ValueChanged);
            // 
            // AlignmentPicker
            // 
            this.AlignmentPicker.Enabled = false;
            this.AlignmentPicker.Location = new System.Drawing.Point(3, 174);
            this.AlignmentPicker.Name = "AlignmentPicker";
            this.AlignmentPicker.Size = new System.Drawing.Size(105, 105);
            this.AlignmentPicker.TabIndex = 16;
            this.AlignmentPicker.AlignmentChanged += new System.EventHandler(this.AlignmentPicker_AlignmentChanged);
            // 
            // RadioCrop
            // 
            this.RadioCrop.AutoSize = true;
            this.RadioCrop.Checked = true;
            this.RadioCrop.Location = new System.Drawing.Point(3, 134);
            this.RadioCrop.Margin = new System.Windows.Forms.Padding(3, 16, 3, 3);
            this.RadioCrop.Name = "RadioCrop";
            this.RadioCrop.Size = new System.Drawing.Size(87, 19);
            this.RadioCrop.TabIndex = 4;
            this.RadioCrop.TabStop = true;
            this.RadioCrop.Text = "Crop image";
            this.RadioCrop.UseVisualStyleBackColor = true;
            this.RadioCrop.Click += new System.EventHandler(this.RadioCrop_Click);
            // 
            // RadioResize
            // 
            this.RadioResize.AutoSize = true;
            this.RadioResize.Location = new System.Drawing.Point(135, 134);
            this.RadioResize.Margin = new System.Windows.Forms.Padding(3, 16, 3, 3);
            this.RadioResize.Name = "RadioResize";
            this.RadioResize.Size = new System.Drawing.Size(93, 19);
            this.RadioResize.TabIndex = 5;
            this.RadioResize.Text = "Resize image";
            this.RadioResize.UseVisualStyleBackColor = true;
            this.RadioResize.Click += new System.EventHandler(this.RadioResize_Click);
            // 
            // LabelWidth
            // 
            this.LabelWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelWidth.AutoSize = true;
            this.LabelWidth.Location = new System.Drawing.Point(3, 11);
            this.LabelWidth.Margin = new System.Windows.Forms.Padding(3);
            this.LabelWidth.Name = "LabelWidth";
            this.LabelWidth.Size = new System.Drawing.Size(42, 17);
            this.LabelWidth.TabIndex = 18;
            this.LabelWidth.Text = "Width:";
            this.LabelWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelHeight
            // 
            this.LabelHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelHeight.AutoSize = true;
            this.LabelHeight.Location = new System.Drawing.Point(3, 40);
            this.LabelHeight.Margin = new System.Windows.Forms.Padding(3);
            this.LabelHeight.Name = "LabelHeight";
            this.LabelHeight.Size = new System.Drawing.Size(46, 17);
            this.LabelHeight.TabIndex = 19;
            this.LabelHeight.Text = "Height:";
            this.LabelHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelDepthOrArray
            // 
            this.LabelDepthOrArray.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelDepthOrArray.AutoSize = true;
            this.LabelDepthOrArray.Location = new System.Drawing.Point(3, 69);
            this.LabelDepthOrArray.Margin = new System.Windows.Forms.Padding(3);
            this.LabelDepthOrArray.Name = "LabelDepthOrArray";
            this.LabelDepthOrArray.Size = new System.Drawing.Size(75, 17);
            this.LabelDepthOrArray.TabIndex = 20;
            this.LabelDepthOrArray.Text = "Depth/Array:";
            this.LabelDepthOrArray.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelMipLevels
            // 
            this.LabelMipLevels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelMipLevels.AutoSize = true;
            this.LabelMipLevels.Location = new System.Drawing.Point(3, 98);
            this.LabelMipLevels.Margin = new System.Windows.Forms.Padding(3);
            this.LabelMipLevels.Name = "LabelMipLevels";
            this.LabelMipLevels.Size = new System.Drawing.Size(63, 17);
            this.LabelMipLevels.TabIndex = 21;
            this.LabelMipLevels.Text = "Mip levels:";
            this.LabelMipLevels.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NumericWidth
            // 
            this.NumericWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericWidth.Location = new System.Drawing.Point(132, 8);
            this.NumericWidth.Margin = new System.Windows.Forms.Padding(0);
            this.NumericWidth.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.NumericWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericWidth.Name = "NumericWidth";
            this.NumericWidth.Size = new System.Drawing.Size(151, 23);
            this.NumericWidth.TabIndex = 22;
            this.NumericWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericWidth.ValueChanged += new System.EventHandler(this.NumericWidth_ValueChanged);
            // 
            // LabelAnchor
            // 
            this.LabelAnchor.AutoSize = true;
            this.LabelAnchor.Enabled = false;
            this.LabelAnchor.Location = new System.Drawing.Point(3, 156);
            this.LabelAnchor.Name = "LabelAnchor";
            this.LabelAnchor.Size = new System.Drawing.Size(63, 15);
            this.LabelAnchor.TabIndex = 14;
            this.LabelAnchor.Text = "Alignment";
            // 
            // PanelOptionsCaption
            // 
            this.PanelOptionsCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PanelOptionsCaption.AutoSize = true;
            this.PanelOptionsCaption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelOptionsCaption.Location = new System.Drawing.Point(3, 296);
            this.PanelOptionsCaption.Name = "PanelOptionsCaption";
            this.PanelOptionsCaption.Size = new System.Drawing.Size(0, 0);
            this.PanelOptionsCaption.TabIndex = 13;
            // 
            // LabelImageFilter
            // 
            this.LabelImageFilter.AutoSize = true;
            this.TableBody.SetColumnSpan(this.LabelImageFilter, 2);
            this.LabelImageFilter.Enabled = false;
            this.LabelImageFilter.Location = new System.Drawing.Point(135, 156);
            this.LabelImageFilter.Name = "LabelImageFilter";
            this.LabelImageFilter.Size = new System.Drawing.Size(146, 15);
            this.LabelImageFilter.TabIndex = 17;
            this.LabelImageFilter.Text = "Image filtering (only WxH)";
            // 
            // ComboImageFilter
            // 
            this.ComboImageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TableBody.SetColumnSpan(this.ComboImageFilter, 2);
            this.ComboImageFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboImageFilter.Enabled = false;
            this.ComboImageFilter.FormattingEnabled = true;
            this.ComboImageFilter.Location = new System.Drawing.Point(135, 174);
            this.ComboImageFilter.Margin = new System.Windows.Forms.Padding(3, 3, 16, 3);
            this.ComboImageFilter.Name = "ComboImageFilter";
            this.ComboImageFilter.Size = new System.Drawing.Size(148, 23);
            this.ComboImageFilter.TabIndex = 8;
            this.ComboImageFilter.SelectedValueChanged += new System.EventHandler(this.ComboImageFilter_SelectedValueChanged);
            // 
            // ImageDimensionSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ImageDimensionSettings";
            this.Size = new System.Drawing.Size(299, 356);
            this.Text = "Image Dimensions";
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            this.TableBody.ResumeLayout(false);
            this.TableBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericMipLevels)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericDepthOrArray)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel TableBody;
        private System.Windows.Forms.RadioButton RadioCrop;
        private System.Windows.Forms.RadioButton RadioResize;
        private System.Windows.Forms.Panel PanelOptionsCaption;
        private System.Windows.Forms.Label LabelImageFilter;
        private Gorgon.UI.GorgonAlignmentPicker AlignmentPicker;
        private System.Windows.Forms.Label LabelAnchor;
        private System.Windows.Forms.ComboBox ComboImageFilter;
        private System.Windows.Forms.Label LabelWidth;
        private System.Windows.Forms.Label LabelHeight;
        private System.Windows.Forms.Label LabelDepthOrArray;
        private System.Windows.Forms.Label LabelMipLevels;
        private System.Windows.Forms.NumericUpDown NumericMipLevels;
        private System.Windows.Forms.NumericUpDown NumericDepthOrArray;
        private System.Windows.Forms.NumericUpDown NumericHeight;
        private System.Windows.Forms.NumericUpDown NumericWidth;
    }
}
