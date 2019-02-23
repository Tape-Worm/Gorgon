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
            this.PanelCaptionBorder = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.LabelCaption = new System.Windows.Forms.Label();
            this.TableBody = new System.Windows.Forms.TableLayoutPanel();
            this.NumericMipLevels = new System.Windows.Forms.NumericUpDown();
            this.LabelMipLevels = new System.Windows.Forms.Label();
            this.ComboMipFilter = new System.Windows.Forms.ComboBox();
            this.LabelImageFilter = new System.Windows.Forms.Label();
            this.PanelConfirmCancel = new System.Windows.Forms.Panel();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.PanelCaptionBorder.SuspendLayout();
            this.panel2.SuspendLayout();
            this.TableBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericMipLevels)).BeginInit();
            this.PanelConfirmCancel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelCaptionBorder
            // 
            this.PanelCaptionBorder.AutoSize = true;
            this.PanelCaptionBorder.BackColor = System.Drawing.Color.SteelBlue;
            this.PanelCaptionBorder.Controls.Add(this.panel2);
            this.PanelCaptionBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelCaptionBorder.Location = new System.Drawing.Point(0, 0);
            this.PanelCaptionBorder.Name = "PanelCaptionBorder";
            this.PanelCaptionBorder.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.PanelCaptionBorder.Size = new System.Drawing.Size(299, 21);
            this.PanelCaptionBorder.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.panel2.Controls.Add(this.LabelCaption);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(297, 21);
            this.panel2.TabIndex = 0;
            // 
            // LabelCaption
            // 
            this.LabelCaption.AutoSize = true;
            this.LabelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCaption.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.LabelCaption.Location = new System.Drawing.Point(0, 0);
            this.LabelCaption.Name = "LabelCaption";
            this.LabelCaption.Size = new System.Drawing.Size(151, 21);
            this.LabelCaption.TabIndex = 0;
            this.LabelCaption.Text = "Mip map generation";
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
            this.TableBody.Location = new System.Drawing.Point(0, 21);
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
            // PanelConfirmCancel
            // 
            this.PanelConfirmCancel.AutoSize = true;
            this.PanelConfirmCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelConfirmCancel.Controls.Add(this.ButtonOK);
            this.PanelConfirmCancel.Controls.Add(this.ButtonCancel);
            this.PanelConfirmCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelConfirmCancel.Location = new System.Drawing.Point(0, 197);
            this.PanelConfirmCancel.Name = "PanelConfirmCancel";
            this.PanelConfirmCancel.Size = new System.Drawing.Size(299, 36);
            this.PanelConfirmCancel.TabIndex = 11;
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.AutoSize = true;
            this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonOK.Location = new System.Drawing.Point(90, 3);
            this.ButtonOK.MinimumSize = new System.Drawing.Size(100, 30);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(100, 30);
            this.ButtonOK.TabIndex = 10;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.AutoSize = true;
            this.ButtonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCancel.Location = new System.Drawing.Point(196, 3);
            this.ButtonCancel.MinimumSize = new System.Drawing.Size(100, 30);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(100, 30);
            this.ButtonCancel.TabIndex = 11;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // GenMipMapSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TableBody);
            this.Controls.Add(this.PanelConfirmCancel);
            this.Controls.Add(this.PanelCaptionBorder);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "GenMipMapSettings";
            this.Size = new System.Drawing.Size(299, 233);
            this.PanelCaptionBorder.ResumeLayout(false);
            this.PanelCaptionBorder.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.TableBody.ResumeLayout(false);
            this.TableBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericMipLevels)).EndInit();
            this.PanelConfirmCancel.ResumeLayout(false);
            this.PanelConfirmCancel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PanelCaptionBorder;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label LabelCaption;
        private System.Windows.Forms.TableLayoutPanel TableBody;
        private System.Windows.Forms.Panel PanelConfirmCancel;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Label LabelMipLevels;
        private System.Windows.Forms.NumericUpDown NumericMipLevels;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.ComboBox ComboMipFilter;
        private System.Windows.Forms.Label LabelImageFilter;
    }
}
