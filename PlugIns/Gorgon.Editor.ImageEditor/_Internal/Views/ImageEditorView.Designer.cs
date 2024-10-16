﻿namespace Gorgon.Editor.ImageEditor;

partial class ImageEditorView
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
            if (ViewModel != null)
            {
                ViewModel.ImagePicker.PropertyChanged -= ImagePicker_PropertyChanged;
            }

            _imagePickerForm?.Dispose();
            _ribbonForm?.Dispose();
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.PanelImageViewControls = new System.Windows.Forms.TableLayoutPanel();
        this.LabelImageSize = new System.Windows.Forms.Label();
        this.LabelDepthSliceDetails = new System.Windows.Forms.Label();
        this.ButtonPrevDepthSlice = new System.Windows.Forms.Button();
        this.ButtonNextDepthSlice = new System.Windows.Forms.Button();
        this.LabelDepthSlice = new System.Windows.Forms.Label();
        this.LabelArrayIndexDetails = new System.Windows.Forms.Label();
        this.ButtonPrevArrayIndex = new System.Windows.Forms.Button();
        this.ButtonNextArrayIndex = new System.Windows.Forms.Button();
        this.LabelArrayIndex = new System.Windows.Forms.Label();
        this.LabelMipDetails = new System.Windows.Forms.Label();
        this.ButtonPrevMip = new System.Windows.Forms.Button();
        this.ButtonNextMip = new System.Windows.Forms.Button();
        this.LabelMipLevel = new System.Windows.Forms.Label();
        this.Fx1BitSettings = new Gorgon.Editor.ImageEditor.Fx1BitSettings();
        this.FxPosterizeSettings = new Gorgon.Editor.ImageEditor.FxPosterizeSettings();
        this.FxEmbossSettings = new Gorgon.Editor.ImageEditor.FxEmbossSettings();
        this.FxSharpenSettings = new Gorgon.Editor.ImageEditor.FxSharpenSettings();
        this.FxBlurSettings = new Gorgon.Editor.ImageEditor.FxBlurSettings();
        this.FxEdgeSettings = new Gorgon.Editor.ImageEditor.FxEdgeSettings();
        this.CropResizeSettings = new Gorgon.Editor.ImageEditor.ImageResizeSettings();
        this.DimensionSettings = new Gorgon.Editor.ImageEditor.ImageDimensionSettings();
        this.SetAlpha = new Gorgon.Editor.ImageEditor.SetAlphaSettings();
        this.GenMipMapSettings = new Gorgon.Editor.ImageEditor.GenMipMapSettings();
        this.StatusPanel.SuspendLayout();
        this.HostPanel.SuspendLayout();
        this.HostPanelControls.SuspendLayout();
        this.PanelImageViewControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // StatusPanel
        // 
        this.StatusPanel.AutoSize = true;
        this.StatusPanel.Controls.Add(this.PanelImageViewControls);
        this.StatusPanel.Location = new System.Drawing.Point(0, 769);
        this.StatusPanel.Size = new System.Drawing.Size(823, 28);
        // 
        // PresentationPanel
        // 
        this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
        this.PresentationPanel.Size = new System.Drawing.Size(823, 748);
        // 
        // HostPanel
        // 
        this.HostPanel.Location = new System.Drawing.Point(823, 21);
        this.HostPanel.Size = new System.Drawing.Size(388, 776);
        // 
        // HostPanelControls
        // 
        this.HostPanelControls.Controls.Add(this.Fx1BitSettings);
        this.HostPanelControls.Controls.Add(this.FxPosterizeSettings);
        this.HostPanelControls.Controls.Add(this.FxEmbossSettings);
        this.HostPanelControls.Controls.Add(this.FxSharpenSettings);
        this.HostPanelControls.Controls.Add(this.FxBlurSettings);
        this.HostPanelControls.Controls.Add(this.FxEdgeSettings);
        this.HostPanelControls.Controls.Add(this.CropResizeSettings);
        this.HostPanelControls.Controls.Add(this.DimensionSettings);
        this.HostPanelControls.Controls.Add(this.SetAlpha);
        this.HostPanelControls.Controls.Add(this.GenMipMapSettings);
        this.HostPanelControls.MinimumSize = new System.Drawing.Size(300, 0);
        this.HostPanelControls.Size = new System.Drawing.Size(387, 775);
        // 
        // PanelImageViewControls
        // 
        this.PanelImageViewControls.AutoSize = true;
        this.PanelImageViewControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelImageViewControls.ColumnCount = 15;
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
        this.PanelImageViewControls.Controls.Add(this.LabelImageSize, 0, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelDepthSliceDetails, 12, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonPrevDepthSlice, 11, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonNextDepthSlice, 13, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelDepthSlice, 10, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelArrayIndexDetails, 8, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonPrevArrayIndex, 7, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonNextArrayIndex, 9, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelArrayIndex, 6, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelMipDetails, 4, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonPrevMip, 3, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonNextMip, 5, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelMipLevel, 2, 0);
        this.PanelImageViewControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelImageViewControls.Location = new System.Drawing.Point(0, 0);
        this.PanelImageViewControls.MinimumSize = new System.Drawing.Size(0, 26);
        this.PanelImageViewControls.Name = "PanelImageViewControls";
        this.PanelImageViewControls.RowCount = 1;
        this.PanelImageViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.PanelImageViewControls.Size = new System.Drawing.Size(823, 28);
        this.PanelImageViewControls.TabIndex = 3;
        // 
        // LabelImageSize
        // 
        this.LabelImageSize.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelImageSize.AutoSize = true;
        this.LabelImageSize.Location = new System.Drawing.Point(3, 0);
        this.LabelImageSize.Name = "LabelImageSize";
        this.LabelImageSize.Size = new System.Drawing.Size(94, 28);
        this.LabelImageSize.TabIndex = 11;
        this.LabelImageSize.Text = "Image size: WxH";
        this.LabelImageSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // LabelDepthSliceDetails
        // 
        this.LabelDepthSliceDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelDepthSliceDetails.AutoSize = true;
        this.LabelDepthSliceDetails.Location = new System.Drawing.Point(721, 0);
        this.LabelDepthSliceDetails.MinimumSize = new System.Drawing.Size(55, 0);
        this.LabelDepthSliceDetails.Name = "LabelDepthSliceDetails";
        this.LabelDepthSliceDetails.Size = new System.Drawing.Size(55, 28);
        this.LabelDepthSliceDetails.TabIndex = 9;
        this.LabelDepthSliceDetails.Text = "1/n";
        this.LabelDepthSliceDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ButtonPrevDepthSlice
        // 
        this.ButtonPrevDepthSlice.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonPrevDepthSlice.AutoSize = true;
        this.ButtonPrevDepthSlice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevDepthSlice.Enabled = false;
        this.ButtonPrevDepthSlice.FlatAppearance.BorderSize = 0;
        this.ButtonPrevDepthSlice.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonPrevDepthSlice.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevDepthSlice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevDepthSlice.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.left_16x16;
        this.ButtonPrevDepthSlice.Location = new System.Drawing.Point(693, 3);
        this.ButtonPrevDepthSlice.Name = "ButtonPrevDepthSlice";
        this.ButtonPrevDepthSlice.Size = new System.Drawing.Size(22, 22);
        this.ButtonPrevDepthSlice.TabIndex = 8;
        this.ButtonPrevDepthSlice.Click += new System.EventHandler(this.ButtonPrevDepthSlice_Click);
        // 
        // ButtonNextDepthSlice
        // 
        this.ButtonNextDepthSlice.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonNextDepthSlice.AutoSize = true;
        this.ButtonNextDepthSlice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextDepthSlice.Enabled = false;
        this.ButtonNextDepthSlice.FlatAppearance.BorderSize = 0;
        this.ButtonNextDepthSlice.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonNextDepthSlice.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextDepthSlice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextDepthSlice.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.right_16x16;
        this.ButtonNextDepthSlice.Location = new System.Drawing.Point(782, 3);
        this.ButtonNextDepthSlice.Name = "ButtonNextDepthSlice";
        this.ButtonNextDepthSlice.Size = new System.Drawing.Size(22, 22);
        this.ButtonNextDepthSlice.TabIndex = 10;
        this.ButtonNextDepthSlice.Click += new System.EventHandler(this.ButtonNextDepthSlice_Click);
        // 
        // LabelDepthSlice
        // 
        this.LabelDepthSlice.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelDepthSlice.AutoSize = true;
        this.LabelDepthSlice.Location = new System.Drawing.Point(619, 0);
        this.LabelDepthSlice.Name = "LabelDepthSlice";
        this.LabelDepthSlice.Size = new System.Drawing.Size(68, 28);
        this.LabelDepthSlice.TabIndex = 3;
        this.LabelDepthSlice.Text = "Depth slice:";
        this.LabelDepthSlice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // LabelArrayIndexDetails
        // 
        this.LabelArrayIndexDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelArrayIndexDetails.AutoSize = true;
        this.LabelArrayIndexDetails.Location = new System.Drawing.Point(530, 0);
        this.LabelArrayIndexDetails.MinimumSize = new System.Drawing.Size(55, 0);
        this.LabelArrayIndexDetails.Name = "LabelArrayIndexDetails";
        this.LabelArrayIndexDetails.Size = new System.Drawing.Size(55, 28);
        this.LabelArrayIndexDetails.TabIndex = 9;
        this.LabelArrayIndexDetails.Text = "1/n";
        this.LabelArrayIndexDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ButtonPrevArrayIndex
        // 
        this.ButtonPrevArrayIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonPrevArrayIndex.AutoSize = true;
        this.ButtonPrevArrayIndex.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevArrayIndex.Enabled = false;
        this.ButtonPrevArrayIndex.FlatAppearance.BorderSize = 0;
        this.ButtonPrevArrayIndex.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonPrevArrayIndex.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevArrayIndex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevArrayIndex.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.left_16x16;
        this.ButtonPrevArrayIndex.Location = new System.Drawing.Point(502, 3);
        this.ButtonPrevArrayIndex.Name = "ButtonPrevArrayIndex";
        this.ButtonPrevArrayIndex.Size = new System.Drawing.Size(22, 22);
        this.ButtonPrevArrayIndex.TabIndex = 8;
        this.ButtonPrevArrayIndex.Click += new System.EventHandler(this.ButtonPrevArrayIndex_Click);
        // 
        // ButtonNextArrayIndex
        // 
        this.ButtonNextArrayIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonNextArrayIndex.AutoSize = true;
        this.ButtonNextArrayIndex.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextArrayIndex.Enabled = false;
        this.ButtonNextArrayIndex.FlatAppearance.BorderSize = 0;
        this.ButtonNextArrayIndex.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonNextArrayIndex.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextArrayIndex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextArrayIndex.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.right_16x16;
        this.ButtonNextArrayIndex.Location = new System.Drawing.Point(591, 3);
        this.ButtonNextArrayIndex.Name = "ButtonNextArrayIndex";
        this.ButtonNextArrayIndex.Size = new System.Drawing.Size(22, 22);
        this.ButtonNextArrayIndex.TabIndex = 10;
        this.ButtonNextArrayIndex.Click += new System.EventHandler(this.ButtonNextArrayIndex_Click);
        // 
        // LabelArrayIndex
        // 
        this.LabelArrayIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelArrayIndex.AutoSize = true;
        this.LabelArrayIndex.Location = new System.Drawing.Point(426, 0);
        this.LabelArrayIndex.Name = "LabelArrayIndex";
        this.LabelArrayIndex.Size = new System.Drawing.Size(70, 28);
        this.LabelArrayIndex.TabIndex = 3;
        this.LabelArrayIndex.Text = "Array index:";
        this.LabelArrayIndex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // LabelMipDetails
        // 
        this.LabelMipDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelMipDetails.AutoSize = true;
        this.LabelMipDetails.Location = new System.Drawing.Point(277, 0);
        this.LabelMipDetails.MinimumSize = new System.Drawing.Size(115, 0);
        this.LabelMipDetails.Name = "LabelMipDetails";
        this.LabelMipDetails.Size = new System.Drawing.Size(115, 28);
        this.LabelMipDetails.TabIndex = 6;
        this.LabelMipDetails.Text = "1/n (WxH)";
        this.LabelMipDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ButtonPrevMip
        // 
        this.ButtonPrevMip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonPrevMip.AutoSize = true;
        this.ButtonPrevMip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevMip.Enabled = false;
        this.ButtonPrevMip.FlatAppearance.BorderSize = 0;
        this.ButtonPrevMip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonPrevMip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevMip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevMip.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.left_16x16;
        this.ButtonPrevMip.Location = new System.Drawing.Point(249, 3);
        this.ButtonPrevMip.Name = "ButtonPrevMip";
        this.ButtonPrevMip.Size = new System.Drawing.Size(22, 22);
        this.ButtonPrevMip.TabIndex = 5;
        this.ButtonPrevMip.Click += new System.EventHandler(this.ButtonPrevMip_Click);
        // 
        // ButtonNextMip
        // 
        this.ButtonNextMip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonNextMip.AutoSize = true;
        this.ButtonNextMip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextMip.Enabled = false;
        this.ButtonNextMip.FlatAppearance.BorderSize = 0;
        this.ButtonNextMip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonNextMip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextMip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextMip.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.right_16x16;
        this.ButtonNextMip.Location = new System.Drawing.Point(398, 3);
        this.ButtonNextMip.Name = "ButtonNextMip";
        this.ButtonNextMip.Size = new System.Drawing.Size(22, 22);
        this.ButtonNextMip.TabIndex = 7;
        this.ButtonNextMip.Click += new System.EventHandler(this.ButtonNextMip_Click);
        // 
        // LabelMipLevel
        // 
        this.LabelMipLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelMipLevel.AutoSize = true;
        this.LabelMipLevel.Location = new System.Drawing.Point(185, 0);
        this.LabelMipLevel.Name = "LabelMipLevel";
        this.LabelMipLevel.Size = new System.Drawing.Size(58, 28);
        this.LabelMipLevel.TabIndex = 2;
        this.LabelMipLevel.Text = "Mip level:";
        this.LabelMipLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // Fx1BitSettings
        // 
        this.Fx1BitSettings.AutoSize = true;
        this.Fx1BitSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.Fx1BitSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Fx1BitSettings.ForeColor = System.Drawing.Color.White;
        this.Fx1BitSettings.Location = new System.Drawing.Point(0, 192);
        this.Fx1BitSettings.MinimumSize = new System.Drawing.Size(300, 120);
        this.Fx1BitSettings.Name = "Fx1BitSettings";
        this.Fx1BitSettings.Size = new System.Drawing.Size(300, 304);
        this.Fx1BitSettings.TabIndex = 0;
        this.Fx1BitSettings.Text = "1-Bit Settings";
        this.Fx1BitSettings.Visible = false;
        // 
        // FxPosterizeSettings
        // 
        this.FxPosterizeSettings.AutoSize = true;
        this.FxPosterizeSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.FxPosterizeSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FxPosterizeSettings.ForeColor = System.Drawing.Color.White;
        this.FxPosterizeSettings.Location = new System.Drawing.Point(0, 168);
        this.FxPosterizeSettings.MinimumSize = new System.Drawing.Size(300, 170);
        this.FxPosterizeSettings.Name = "FxPosterizeSettings";
        this.FxPosterizeSettings.Size = new System.Drawing.Size(300, 170);
        this.FxPosterizeSettings.TabIndex = 0;
        this.FxPosterizeSettings.Text = "Posterize Settings";
        this.FxPosterizeSettings.Visible = false;
        // 
        // FxEmbossSettings
        // 
        this.FxEmbossSettings.AutoSize = true;
        this.FxEmbossSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.FxEmbossSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FxEmbossSettings.ForeColor = System.Drawing.Color.White;
        this.FxEmbossSettings.Location = new System.Drawing.Point(0, 144);
        this.FxEmbossSettings.MinimumSize = new System.Drawing.Size(300, 120);
        this.FxEmbossSettings.Name = "FxEmbossSettings";
        this.FxEmbossSettings.Size = new System.Drawing.Size(300, 120);
        this.FxEmbossSettings.TabIndex = 0;
        this.FxEmbossSettings.Text = "Emboss Settings";
        this.FxEmbossSettings.Visible = false;
        // 
        // FxSharpenSettings
        // 
        this.FxSharpenSettings.AutoSize = true;
        this.FxSharpenSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.FxSharpenSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FxSharpenSettings.ForeColor = System.Drawing.Color.White;
        this.FxSharpenSettings.Location = new System.Drawing.Point(0, 120);
        this.FxSharpenSettings.MinimumSize = new System.Drawing.Size(300, 120);
        this.FxSharpenSettings.Name = "FxSharpenSettings";
        this.FxSharpenSettings.Size = new System.Drawing.Size(300, 120);
        this.FxSharpenSettings.TabIndex = 0;
        this.FxSharpenSettings.Text = "Sharpen Settings";
        this.FxSharpenSettings.Visible = false;
        // 
        // FxBlurSettings
        // 
        this.FxBlurSettings.AutoSize = true;
        this.FxBlurSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.FxBlurSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FxBlurSettings.ForeColor = System.Drawing.Color.White;
        this.FxBlurSettings.Location = new System.Drawing.Point(0, 96);
        this.FxBlurSettings.MinimumSize = new System.Drawing.Size(300, 120);
        this.FxBlurSettings.Name = "FxBlurSettings";
        this.FxBlurSettings.Size = new System.Drawing.Size(300, 120);
        this.FxBlurSettings.TabIndex = 0;
        this.FxBlurSettings.Text = "Gaussian Blur Settings";
        this.FxBlurSettings.Visible = false;
        // 
        // FxEdgeSettings
        // 
        this.FxEdgeSettings.AutoSize = true;
        this.FxEdgeSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.FxEdgeSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FxEdgeSettings.ForeColor = System.Drawing.Color.White;
        this.FxEdgeSettings.Location = new System.Drawing.Point(0, 72);
        this.FxEdgeSettings.MinimumSize = new System.Drawing.Size(300, 520);
        this.FxEdgeSettings.Name = "FxEdgeSettings";
        this.FxEdgeSettings.Size = new System.Drawing.Size(300, 520);
        this.FxEdgeSettings.TabIndex = 0;
        this.FxEdgeSettings.Text = "Edge Detection Settings";
        this.FxEdgeSettings.Visible = false;
        // 
        // CropResizeSettings
        // 
        this.CropResizeSettings.AutoSize = true;
        this.CropResizeSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CropResizeSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.CropResizeSettings.ForeColor = System.Drawing.Color.White;
        this.CropResizeSettings.Location = new System.Drawing.Point(0, 48);
        this.CropResizeSettings.MaximumSize = new System.Drawing.Size(384, 0);
        this.CropResizeSettings.MinimumSize = new System.Drawing.Size(384, 400);
        this.CropResizeSettings.Name = "CropResizeSettings";
        this.CropResizeSettings.Size = new System.Drawing.Size(384, 400);
        this.CropResizeSettings.TabIndex = 0;
        this.CropResizeSettings.Text = "Resize Image";
        this.CropResizeSettings.Visible = false;
        // 
        // DimensionSettings
        // 
        this.DimensionSettings.AutoSize = true;
        this.DimensionSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.DimensionSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.DimensionSettings.ForeColor = System.Drawing.Color.White;
        this.DimensionSettings.Location = new System.Drawing.Point(0, 48);
        this.DimensionSettings.MinimumSize = new System.Drawing.Size(300, 345);
        this.DimensionSettings.Name = "DimensionSettings";
        this.DimensionSettings.Size = new System.Drawing.Size(300, 345);
        this.DimensionSettings.TabIndex = 0;
        this.DimensionSettings.Text = "Image Dimensions";
        this.DimensionSettings.Visible = false;
        // 
        // SetAlpha
        // 
        this.SetAlpha.AutoSize = true;
        this.SetAlpha.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.SetAlpha.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.SetAlpha.ForeColor = System.Drawing.Color.White;
        this.SetAlpha.Location = new System.Drawing.Point(0, 24);
        this.SetAlpha.MinimumSize = new System.Drawing.Size(300, 320);
        this.SetAlpha.Name = "SetAlpha";
        this.SetAlpha.Size = new System.Drawing.Size(300, 320);
        this.SetAlpha.TabIndex = 0;
        this.SetAlpha.Text = "Set Alpha";
        this.SetAlpha.Visible = false;
        // 
        // GenMipMapSettings
        // 
        this.GenMipMapSettings.AutoSize = true;
        this.GenMipMapSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.GenMipMapSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.GenMipMapSettings.ForeColor = System.Drawing.Color.White;
        this.GenMipMapSettings.Location = new System.Drawing.Point(0, 0);
        this.GenMipMapSettings.MinimumSize = new System.Drawing.Size(300, 150);
        this.GenMipMapSettings.Name = "GenMipMapSettings";
        this.GenMipMapSettings.Size = new System.Drawing.Size(300, 150);
        this.GenMipMapSettings.TabIndex = 0;
        this.GenMipMapSettings.Text = "Generate Mip Maps";
        this.GenMipMapSettings.Visible = false;
        // 
        // ImageEditorView
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "ImageEditorView";
        this.Size = new System.Drawing.Size(1211, 797);
        this.StatusPanel.ResumeLayout(false);
        this.StatusPanel.PerformLayout();
        this.HostPanel.ResumeLayout(false);
        this.HostPanel.PerformLayout();
        this.HostPanelControls.ResumeLayout(false);
        this.HostPanelControls.PerformLayout();
        this.PanelImageViewControls.ResumeLayout(false);
        this.PanelImageViewControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.TableLayoutPanel PanelImageViewControls;
    private System.Windows.Forms.Label LabelImageSize;
    private System.Windows.Forms.Label LabelDepthSliceDetails;
    private System.Windows.Forms.Button ButtonPrevDepthSlice;
    private System.Windows.Forms.Button ButtonNextDepthSlice;
    private System.Windows.Forms.Label LabelDepthSlice;
    private System.Windows.Forms.Label LabelArrayIndexDetails;
    private System.Windows.Forms.Button ButtonPrevArrayIndex;
    private System.Windows.Forms.Button ButtonNextArrayIndex;
    private System.Windows.Forms.Label LabelArrayIndex;
    private System.Windows.Forms.Label LabelMipDetails;
    private System.Windows.Forms.Button ButtonPrevMip;
    private System.Windows.Forms.Button ButtonNextMip;
    private System.Windows.Forms.Label LabelMipLevel;
    private SetAlphaSettings SetAlpha;
    private GenMipMapSettings GenMipMapSettings;
    private ImageDimensionSettings DimensionSettings;
    private ImageResizeSettings CropResizeSettings;
    private FxBlurSettings FxBlurSettings;
    private FxSharpenSettings FxSharpenSettings;
    private FxEmbossSettings FxEmbossSettings;
    private FxEdgeSettings FxEdgeSettings;
    private Fx1BitSettings Fx1BitSettings;
    private FxPosterizeSettings FxPosterizeSettings;
}
