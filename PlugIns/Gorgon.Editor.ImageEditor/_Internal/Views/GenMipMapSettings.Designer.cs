namespace Gorgon.Editor.ImageEditor
{
    partial class GenMipMapSettings
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
            this.LabelMipLevels = new System.Windows.Forms.Label();
            this.ComboMipFilter = new System.Windows.Forms.ComboBox();
            this.LabelImageFilter = new System.Windows.Forms.Label();
            this.PanelBody.SuspendLayout();
            this.TableBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericMipLevels)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.TableBody);
            this.PanelBody.Size = new System.Drawing.Size(299, 176);
            // 
            // TableBody
            // 
            this.TableBody.AutoSize = true;
            this.TableBody.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableBody.ColumnCount = 3;
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.TableBody.Controls.Add(this.NumericMipLevels, 1, 0);
            this.TableBody.Controls.Add(this.LabelMipLevels, 0, 0);
            this.TableBody.Controls.Add(this.ComboMipFilter, 1, 1);
            this.TableBody.Controls.Add(this.LabelImageFilter, 0, 1);
            this.TableBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableBody.Location = new System.Drawing.Point(0, 0);
            this.TableBody.Name = "TableBody";
            this.TableBody.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.TableBody.RowCount = 6;
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableBody.Size = new System.Drawing.Size(299, 176);
            this.TableBody.TabIndex = 1;
            // 
            // NumericMipLevels
            // 
            this.NumericMipLevels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericMipLevels.Location = new System.Drawing.Point(135, 11);
            this.NumericMipLevels.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.NumericMipLevels.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.NumericMipLevels.Name = "NumericMipLevels";
            this.NumericMipLevels.Size = new System.Drawing.Size(145, 23);
            this.NumericMipLevels.TabIndex = 25;
            this.NumericMipLevels.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericMipLevels.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.NumericMipLevels.ValueChanged += new System.EventHandler(this.NumericMipLevels_ValueChanged);
            // 
            // LabelMipLevels
            // 
            this.LabelMipLevels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelMipLevels.AutoSize = true;
            this.LabelMipLevels.Location = new System.Drawing.Point(3, 11);
            this.LabelMipLevels.Margin = new System.Windows.Forms.Padding(3);
            this.LabelMipLevels.Name = "LabelMipLevels";
            this.LabelMipLevels.Size = new System.Drawing.Size(63, 23);
            this.LabelMipLevels.TabIndex = 21;
            this.LabelMipLevels.Text = "Mip levels:";
            this.LabelMipLevels.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ComboMipFilter
            // 
            this.ComboMipFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboMipFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboMipFilter.FormattingEnabled = true;
            this.ComboMipFilter.Location = new System.Drawing.Point(135, 40);
            this.ComboMipFilter.Name = "ComboMipFilter";
            this.ComboMipFilter.Size = new System.Drawing.Size(145, 23);
            this.ComboMipFilter.TabIndex = 8;
            this.ComboMipFilter.SelectedValueChanged += new System.EventHandler(this.ComboImageFilter_SelectedValueChanged);
            // 
            // LabelImageFilter
            // 
            this.LabelImageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelImageFilter.AutoSize = true;
            this.LabelImageFilter.Location = new System.Drawing.Point(3, 37);
            this.LabelImageFilter.Name = "LabelImageFilter";
            this.LabelImageFilter.Size = new System.Drawing.Size(102, 29);
            this.LabelImageFilter.TabIndex = 17;
            this.LabelImageFilter.Text = "Mip level filtering:";
            this.LabelImageFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GenMipMapSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "GenMipMapSettings";
            this.Size = new System.Drawing.Size(299, 233);
            this.Text = "Generate Mip Maps";
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            this.TableBody.ResumeLayout(false);
            this.TableBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericMipLevels)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel TableBody;
        private System.Windows.Forms.Label LabelMipLevels;
        private System.Windows.Forms.NumericUpDown NumericMipLevels;
        private System.Windows.Forms.ComboBox ComboMipFilter;
        private System.Windows.Forms.Label LabelImageFilter;
    }
}
