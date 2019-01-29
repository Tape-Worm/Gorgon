namespace Gorgon.Editor.ImageEditor
{
    partial class ImageResizeSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageResizeSettings));
            this.PanelCaptionBorder = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.LabelCaption = new System.Windows.Forms.Label();
            this.TableBody = new System.Windows.Forms.TableLayoutPanel();
            this.LabelDesc = new System.Windows.Forms.Label();
            this.LabelImportImageName = new System.Windows.Forms.Label();
            this.LabelImportImageDimensions = new System.Windows.Forms.Label();
            this.LabelTargetImageDimensions = new System.Windows.Forms.Label();
            this.RadioCrop = new System.Windows.Forms.RadioButton();
            this.RadioResize = new System.Windows.Forms.RadioButton();
            this.panelAnchor = new System.Windows.Forms.Panel();
            this.AlignmentPicker = new Gorgon.Windows.UI.GorgonAlignmentPicker();
            this.ComboImageFilter = new System.Windows.Forms.ComboBox();
            this.CheckPreserveAspect = new System.Windows.Forms.CheckBox();
            this.PanelOptionsCaption = new System.Windows.Forms.Panel();
            this.LabelAnchor = new System.Windows.Forms.Label();
            this.LabelImageFilter = new System.Windows.Forms.Label();
            this.PanelConfirmCancel = new System.Windows.Forms.Panel();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.PanelCaptionBorder.SuspendLayout();
            this.panel2.SuspendLayout();
            this.TableBody.SuspendLayout();
            this.panelAnchor.SuspendLayout();
            this.PanelOptionsCaption.SuspendLayout();
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
            this.PanelCaptionBorder.Size = new System.Drawing.Size(382, 21);
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
            this.panel2.Size = new System.Drawing.Size(380, 21);
            this.panel2.TabIndex = 0;
            // 
            // LabelCaption
            // 
            this.LabelCaption.AutoSize = true;
            this.LabelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCaption.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.LabelCaption.Location = new System.Drawing.Point(0, 0);
            this.LabelCaption.Name = "LabelCaption";
            this.LabelCaption.Size = new System.Drawing.Size(138, 21);
            this.LabelCaption.TabIndex = 0;
            this.LabelCaption.Text = "Resize/crop image";
            // 
            // TableBody
            // 
            this.TableBody.AutoSize = true;
            this.TableBody.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableBody.ColumnCount = 2;
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableBody.Controls.Add(this.LabelDesc, 0, 0);
            this.TableBody.Controls.Add(this.LabelImportImageName, 0, 1);
            this.TableBody.Controls.Add(this.LabelImportImageDimensions, 0, 2);
            this.TableBody.Controls.Add(this.LabelTargetImageDimensions, 0, 3);
            this.TableBody.Controls.Add(this.RadioCrop, 0, 4);
            this.TableBody.Controls.Add(this.RadioResize, 0, 5);
            this.TableBody.Controls.Add(this.panelAnchor, 1, 5);
            this.TableBody.Controls.Add(this.PanelOptionsCaption, 1, 4);
            this.TableBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableBody.Location = new System.Drawing.Point(0, 21);
            this.TableBody.Name = "TableBody";
            this.TableBody.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.TableBody.RowCount = 7;
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableBody.Size = new System.Drawing.Size(382, 320);
            this.TableBody.TabIndex = 1;
            // 
            // LabelDesc
            // 
            this.LabelDesc.AutoEllipsis = true;
            this.LabelDesc.AutoSize = true;
            this.TableBody.SetColumnSpan(this.LabelDesc, 2);
            this.LabelDesc.Location = new System.Drawing.Point(3, 13);
            this.LabelDesc.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.LabelDesc.Name = "LabelDesc";
            this.LabelDesc.Size = new System.Drawing.Size(370, 75);
            this.LabelDesc.TabIndex = 0;
            this.LabelDesc.Text = resources.GetString("LabelDesc.Text");
            // 
            // LabelImportImageName
            // 
            this.LabelImportImageName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelImportImageName.AutoSize = true;
            this.TableBody.SetColumnSpan(this.LabelImportImageName, 2);
            this.LabelImportImageName.Location = new System.Drawing.Point(3, 94);
            this.LabelImportImageName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.LabelImportImageName.Name = "LabelImportImageName";
            this.LabelImportImageName.Size = new System.Drawing.Size(376, 15);
            this.LabelImportImageName.TabIndex = 1;
            this.LabelImportImageName.Text = "Import image name: {0}";
            // 
            // LabelImportImageDimensions
            // 
            this.LabelImportImageDimensions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelImportImageDimensions.AutoSize = true;
            this.TableBody.SetColumnSpan(this.LabelImportImageDimensions, 2);
            this.LabelImportImageDimensions.Location = new System.Drawing.Point(3, 115);
            this.LabelImportImageDimensions.Margin = new System.Windows.Forms.Padding(3);
            this.LabelImportImageDimensions.Name = "LabelImportImageDimensions";
            this.LabelImportImageDimensions.Size = new System.Drawing.Size(376, 15);
            this.LabelImportImageDimensions.TabIndex = 2;
            this.LabelImportImageDimensions.Text = "Import image dimensions: {0}x{1}";
            // 
            // LabelTargetImageDimensions
            // 
            this.LabelTargetImageDimensions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelTargetImageDimensions.AutoSize = true;
            this.TableBody.SetColumnSpan(this.LabelTargetImageDimensions, 2);
            this.LabelTargetImageDimensions.Location = new System.Drawing.Point(3, 136);
            this.LabelTargetImageDimensions.Margin = new System.Windows.Forms.Padding(3);
            this.LabelTargetImageDimensions.Name = "LabelTargetImageDimensions";
            this.LabelTargetImageDimensions.Size = new System.Drawing.Size(376, 15);
            this.LabelTargetImageDimensions.TabIndex = 3;
            this.LabelTargetImageDimensions.Text = "Target image dimensions: {0}x{1}";
            // 
            // RadioCrop
            // 
            this.RadioCrop.AutoSize = true;
            this.RadioCrop.Checked = true;
            this.RadioCrop.Location = new System.Drawing.Point(3, 170);
            this.RadioCrop.Margin = new System.Windows.Forms.Padding(3, 16, 3, 3);
            this.RadioCrop.Name = "RadioCrop";
            this.RadioCrop.Size = new System.Drawing.Size(101, 19);
            this.RadioCrop.TabIndex = 4;
            this.RadioCrop.TabStop = true;
            this.RadioCrop.Text = "Crop to {0}x{1}";
            this.RadioCrop.UseVisualStyleBackColor = true;
            this.RadioCrop.Click += new System.EventHandler(this.RadioCrop_Click);
            // 
            // RadioResize
            // 
            this.RadioResize.AutoSize = true;
            this.RadioResize.Location = new System.Drawing.Point(3, 195);
            this.RadioResize.Name = "RadioResize";
            this.RadioResize.Size = new System.Drawing.Size(107, 19);
            this.RadioResize.TabIndex = 5;
            this.RadioResize.Text = "Resize to {0}x{1}";
            this.RadioResize.UseVisualStyleBackColor = true;
            this.RadioResize.Click += new System.EventHandler(this.RadioResize_Click);
            // 
            // panelAnchor
            // 
            this.panelAnchor.AutoSize = true;
            this.panelAnchor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelAnchor.Controls.Add(this.AlignmentPicker);
            this.panelAnchor.Controls.Add(this.ComboImageFilter);
            this.panelAnchor.Controls.Add(this.CheckPreserveAspect);
            this.panelAnchor.Location = new System.Drawing.Point(153, 195);
            this.panelAnchor.Name = "panelAnchor";
            this.panelAnchor.Size = new System.Drawing.Size(226, 106);
            this.panelAnchor.TabIndex = 12;
            // 
            // AlignmentPicker
            // 
            this.AlignmentPicker.Location = new System.Drawing.Point(3, -2);
            this.AlignmentPicker.Name = "AlignmentPicker";
            this.AlignmentPicker.Size = new System.Drawing.Size(105, 105);
            this.AlignmentPicker.TabIndex = 0;
            this.AlignmentPicker.Visible = false;
            this.AlignmentPicker.AlignmentChanged += new System.EventHandler(this.AlignmentPicker_AlignmentChanged);
            // 
            // ComboImageFilter
            // 
            this.ComboImageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboImageFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboImageFilter.FormattingEnabled = true;
            this.ComboImageFilter.Location = new System.Drawing.Point(3, 3);
            this.ComboImageFilter.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.ComboImageFilter.Name = "ComboImageFilter";
            this.ComboImageFilter.Size = new System.Drawing.Size(212, 23);
            this.ComboImageFilter.TabIndex = 8;
            this.ComboImageFilter.SelectedValueChanged += new System.EventHandler(this.ComboImageFilter_SelectedValueChanged);
            // 
            // CheckPreserveAspect
            // 
            this.CheckPreserveAspect.AutoSize = true;
            this.CheckPreserveAspect.Checked = true;
            this.CheckPreserveAspect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckPreserveAspect.Location = new System.Drawing.Point(3, 32);
            this.CheckPreserveAspect.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.CheckPreserveAspect.Name = "CheckPreserveAspect";
            this.CheckPreserveAspect.Size = new System.Drawing.Size(229, 19);
            this.CheckPreserveAspect.TabIndex = 9;
            this.CheckPreserveAspect.Text = "Preserve the aspect ratio of the image?";
            this.CheckPreserveAspect.UseVisualStyleBackColor = true;
            this.CheckPreserveAspect.Click += new System.EventHandler(this.CheckPreserveAspect_Click);
            // 
            // PanelOptionsCaption
            // 
            this.PanelOptionsCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PanelOptionsCaption.AutoSize = true;
            this.PanelOptionsCaption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelOptionsCaption.Controls.Add(this.LabelAnchor);
            this.PanelOptionsCaption.Controls.Add(this.LabelImageFilter);
            this.PanelOptionsCaption.Location = new System.Drawing.Point(153, 174);
            this.PanelOptionsCaption.Name = "PanelOptionsCaption";
            this.PanelOptionsCaption.Size = new System.Drawing.Size(87, 15);
            this.PanelOptionsCaption.TabIndex = 13;
            // 
            // LabelAnchor
            // 
            this.LabelAnchor.AutoSize = true;
            this.LabelAnchor.Location = new System.Drawing.Point(0, 0);
            this.LabelAnchor.Name = "LabelAnchor";
            this.LabelAnchor.Size = new System.Drawing.Size(63, 15);
            this.LabelAnchor.TabIndex = 7;
            this.LabelAnchor.Text = "Alignment";
            // 
            // LabelImageFilter
            // 
            this.LabelImageFilter.AutoSize = true;
            this.LabelImageFilter.Location = new System.Drawing.Point(0, 0);
            this.LabelImageFilter.Name = "LabelImageFilter";
            this.LabelImageFilter.Size = new System.Drawing.Size(84, 15);
            this.LabelImageFilter.TabIndex = 8;
            this.LabelImageFilter.Text = "Image filtering";
            // 
            // PanelConfirmCancel
            // 
            this.PanelConfirmCancel.AutoSize = true;
            this.PanelConfirmCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelConfirmCancel.Controls.Add(this.ButtonOK);
            this.PanelConfirmCancel.Controls.Add(this.ButtonCancel);
            this.PanelConfirmCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelConfirmCancel.Location = new System.Drawing.Point(0, 341);
            this.PanelConfirmCancel.Name = "PanelConfirmCancel";
            this.PanelConfirmCancel.Size = new System.Drawing.Size(382, 36);
            this.PanelConfirmCancel.TabIndex = 11;
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.AutoSize = true;
            this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Enabled = false;
            this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonOK.Location = new System.Drawing.Point(173, 3);
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
            this.ButtonCancel.Location = new System.Drawing.Point(279, 3);
            this.ButtonCancel.MinimumSize = new System.Drawing.Size(100, 30);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(100, 30);
            this.ButtonCancel.TabIndex = 11;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ImageResizeSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TableBody);
            this.Controls.Add(this.PanelConfirmCancel);
            this.Controls.Add(this.PanelCaptionBorder);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ImageResizeSettings";
            this.Size = new System.Drawing.Size(382, 377);
            this.PanelCaptionBorder.ResumeLayout(false);
            this.PanelCaptionBorder.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.TableBody.ResumeLayout(false);
            this.TableBody.PerformLayout();
            this.panelAnchor.ResumeLayout(false);
            this.panelAnchor.PerformLayout();
            this.PanelOptionsCaption.ResumeLayout(false);
            this.PanelOptionsCaption.PerformLayout();
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
        private System.Windows.Forms.Label LabelDesc;
        private System.Windows.Forms.Label LabelImportImageName;
        private System.Windows.Forms.Label LabelImportImageDimensions;
        private System.Windows.Forms.Label LabelTargetImageDimensions;
        private System.Windows.Forms.RadioButton RadioCrop;
        private System.Windows.Forms.RadioButton RadioResize;
        private System.Windows.Forms.ComboBox ComboImageFilter;
        private System.Windows.Forms.CheckBox CheckPreserveAspect;
        private System.Windows.Forms.Panel PanelConfirmCancel;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Panel panelAnchor;
        private Windows.UI.GorgonAlignmentPicker AlignmentPicker;
        private System.Windows.Forms.Panel PanelOptionsCaption;
        private System.Windows.Forms.Label LabelImageFilter;
        private System.Windows.Forms.Label LabelAnchor;
    }
}
