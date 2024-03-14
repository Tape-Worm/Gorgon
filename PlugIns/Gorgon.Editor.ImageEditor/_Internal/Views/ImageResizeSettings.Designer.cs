namespace Gorgon.Editor.ImageEditor;

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



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageResizeSettings));
        this.TableBody = new System.Windows.Forms.TableLayoutPanel();
        this.LabelImageFilter = new System.Windows.Forms.Label();
        this.AlignmentPicker = new Gorgon.UI.GorgonAlignmentPicker();
        this.panelAnchor = new System.Windows.Forms.Panel();
        this.ComboImageFilter = new System.Windows.Forms.ComboBox();
        this.CheckPreserveAspect = new System.Windows.Forms.CheckBox();
        this.LabelAnchor = new System.Windows.Forms.Label();
        this.LabelDesc = new System.Windows.Forms.Label();
        this.LabelImportImageName = new System.Windows.Forms.Label();
        this.LabelImportImageDimensions = new System.Windows.Forms.Label();
        this.LabelTargetImageDimensions = new System.Windows.Forms.Label();
        this.RadioCrop = new System.Windows.Forms.RadioButton();
        this.RadioResize = new System.Windows.Forms.RadioButton();
        this.PanelOptionsCaption = new System.Windows.Forms.Panel();
        this.PanelBody.SuspendLayout();
        this.TableBody.SuspendLayout();
        this.panelAnchor.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableBody);
        this.PanelBody.Size = new System.Drawing.Size(382, 441);
        // 
        // TableBody
        // 
        this.TableBody.AutoSize = true;
        this.TableBody.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableBody.ColumnCount = 2;
        this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableBody.Controls.Add(this.LabelImageFilter, 0, 8);
        this.TableBody.Controls.Add(this.AlignmentPicker, 0, 6);
        this.TableBody.Controls.Add(this.panelAnchor, 0, 9);
        this.TableBody.Controls.Add(this.LabelAnchor, 0, 5);
        this.TableBody.Controls.Add(this.LabelDesc, 0, 0);
        this.TableBody.Controls.Add(this.LabelImportImageName, 0, 1);
        this.TableBody.Controls.Add(this.LabelImportImageDimensions, 0, 2);
        this.TableBody.Controls.Add(this.LabelTargetImageDimensions, 0, 3);
        this.TableBody.Controls.Add(this.RadioCrop, 0, 4);
        this.TableBody.Controls.Add(this.RadioResize, 0, 7);
        this.TableBody.Controls.Add(this.PanelOptionsCaption, 1, 4);
        this.TableBody.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableBody.Location = new System.Drawing.Point(0, 0);
        this.TableBody.Name = "TableBody";
        this.TableBody.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
        this.TableBody.RowCount = 11;
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableBody.Size = new System.Drawing.Size(382, 441);
        this.TableBody.TabIndex = 1;
        // 
        // LabelImageFilter
        // 
        this.LabelImageFilter.AutoSize = true;
        this.LabelImageFilter.Location = new System.Drawing.Point(24, 343);
        this.LabelImageFilter.Margin = new System.Windows.Forms.Padding(24, 0, 3, 0);
        this.LabelImageFilter.Name = "LabelImageFilter";
        this.LabelImageFilter.Size = new System.Drawing.Size(84, 15);
        this.LabelImageFilter.TabIndex = 17;
        this.LabelImageFilter.Text = "Image filtering";
        // 
        // AlignmentPicker
        // 
        this.AlignmentPicker.Location = new System.Drawing.Point(24, 210);
        this.AlignmentPicker.Margin = new System.Windows.Forms.Padding(24, 3, 3, 3);
        this.AlignmentPicker.Name = "AlignmentPicker";
        this.AlignmentPicker.Size = new System.Drawing.Size(105, 105);
        this.AlignmentPicker.TabIndex = 16;
        this.AlignmentPicker.Visible = false;
        this.AlignmentPicker.AlignmentChanged += new System.EventHandler(this.AlignmentPicker_AlignmentChanged);
        // 
        // panelAnchor
        // 
        this.panelAnchor.AutoSize = true;
        this.panelAnchor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableBody.SetColumnSpan(this.panelAnchor, 2);
        this.panelAnchor.Controls.Add(this.ComboImageFilter);
        this.panelAnchor.Controls.Add(this.CheckPreserveAspect);
        this.panelAnchor.Location = new System.Drawing.Point(24, 361);
        this.panelAnchor.Margin = new System.Windows.Forms.Padding(24, 3, 3, 3);
        this.panelAnchor.Name = "panelAnchor";
        this.panelAnchor.Size = new System.Drawing.Size(235, 54);
        this.panelAnchor.TabIndex = 15;
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
        this.ComboImageFilter.Size = new System.Drawing.Size(221, 23);
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
        // LabelAnchor
        // 
        this.LabelAnchor.AutoSize = true;
        this.LabelAnchor.Location = new System.Drawing.Point(24, 192);
        this.LabelAnchor.Margin = new System.Windows.Forms.Padding(24, 0, 3, 0);
        this.LabelAnchor.Name = "LabelAnchor";
        this.LabelAnchor.Size = new System.Drawing.Size(63, 15);
        this.LabelAnchor.TabIndex = 14;
        this.LabelAnchor.Text = "Alignment";
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
        this.RadioResize.Location = new System.Drawing.Point(3, 321);
        this.RadioResize.Name = "RadioResize";
        this.RadioResize.Size = new System.Drawing.Size(107, 19);
        this.RadioResize.TabIndex = 5;
        this.RadioResize.Text = "Resize to {0}x{1}";
        this.RadioResize.UseVisualStyleBackColor = true;
        this.RadioResize.Click += new System.EventHandler(this.RadioResize_Click);
        // 
        // PanelOptionsCaption
        // 
        this.PanelOptionsCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.PanelOptionsCaption.AutoSize = true;
        this.PanelOptionsCaption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelOptionsCaption.Location = new System.Drawing.Point(153, 189);
        this.PanelOptionsCaption.Name = "PanelOptionsCaption";
        this.PanelOptionsCaption.Size = new System.Drawing.Size(0, 0);
        this.PanelOptionsCaption.TabIndex = 13;
        // 
        // ImageResizeSettings
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ForeColor = System.Drawing.Color.White;
        this.Name = "ImageResizeSettings";
        this.Size = new System.Drawing.Size(382, 498);
        this.Text = "Resize Image";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.TableBody.ResumeLayout(false);
        this.TableBody.PerformLayout();
        this.panelAnchor.ResumeLayout(false);
        this.panelAnchor.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.TableLayoutPanel TableBody;
    private System.Windows.Forms.Label LabelDesc;
    private System.Windows.Forms.Label LabelImportImageName;
    private System.Windows.Forms.Label LabelImportImageDimensions;
    private System.Windows.Forms.Label LabelTargetImageDimensions;
    private System.Windows.Forms.RadioButton RadioCrop;
    private System.Windows.Forms.RadioButton RadioResize;
    private System.Windows.Forms.Panel PanelOptionsCaption;
    private System.Windows.Forms.Label LabelImageFilter;
    private Gorgon.UI.GorgonAlignmentPicker AlignmentPicker;
    private System.Windows.Forms.Label LabelAnchor;
    private System.Windows.Forms.Panel panelAnchor;
    private System.Windows.Forms.ComboBox ComboImageFilter;
    private System.Windows.Forms.CheckBox CheckPreserveAspect;
}
