namespace Gorgon.Editor.ImageEditor
{
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
                _panels.Clear();
                _ribbonForm.ImageZoomed -= RibbonForm_ImageZoomed;
                _ribbonForm?.Dispose();                
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
            this.PanelBottomBar = new System.Windows.Forms.Panel();
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
            this.TableViewControls = new System.Windows.Forms.TableLayoutPanel();
            this.ScrollHorizontal = new System.Windows.Forms.HScrollBar();
            this.ScrollVertical = new System.Windows.Forms.VScrollBar();
            this.PanelImage = new Gorgon.Windows.UI.GorgonSelectablePanel();
            this.GenMipMapSettings = new Gorgon.Editor.ImageEditor.GenMipMapSettings();
            this.DimensionSettings = new Gorgon.Editor.ImageEditor.ImageDimensionSettings();
            this.CropResizeSettings = new Gorgon.Editor.ImageEditor.ImageResizeSettings();
            this.ButtonCenter = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PresentationPanel)).BeginInit();
            this.PresentationPanel.SuspendLayout();
            this.PanelBottomBar.SuspendLayout();
            this.PanelImageViewControls.SuspendLayout();
            this.TableViewControls.SuspendLayout();
            this.PanelImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // PresentationPanel
            // 
            this.PresentationPanel.Controls.Add(this.TableViewControls);
            this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
            this.PresentationPanel.Size = new System.Drawing.Size(1211, 748);
            // 
            // PanelBottomBar
            // 
            this.PanelBottomBar.AutoSize = true;
            this.PanelBottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelBottomBar.Controls.Add(this.PanelImageViewControls);
            this.PanelBottomBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelBottomBar.Location = new System.Drawing.Point(0, 769);
            this.PanelBottomBar.Name = "PanelBottomBar";
            this.PanelBottomBar.Size = new System.Drawing.Size(1211, 28);
            this.PanelBottomBar.TabIndex = 1;
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
            this.PanelImageViewControls.Size = new System.Drawing.Size(1211, 28);
            this.PanelImageViewControls.TabIndex = 2;
            this.PanelImageViewControls.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
            // 
            // LabelImageSize
            // 
            this.LabelImageSize.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelImageSize.AutoSize = true;
            this.LabelImageSize.Location = new System.Drawing.Point(3, 0);
            this.LabelImageSize.Name = "LabelImageSize";
            this.LabelImageSize.Size = new System.Drawing.Size(93, 28);
            this.LabelImageSize.TabIndex = 11;
            this.LabelImageSize.Text = "Image size: WxH";
            this.LabelImageSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelImageSize.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
            // 
            // LabelDepthSliceDetails
            // 
            this.LabelDepthSliceDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelDepthSliceDetails.AutoSize = true;
            this.LabelDepthSliceDetails.Location = new System.Drawing.Point(1109, 0);
            this.LabelDepthSliceDetails.MinimumSize = new System.Drawing.Size(55, 0);
            this.LabelDepthSliceDetails.Name = "LabelDepthSliceDetails";
            this.LabelDepthSliceDetails.Size = new System.Drawing.Size(55, 28);
            this.LabelDepthSliceDetails.TabIndex = 9;
            this.LabelDepthSliceDetails.Text = "1/n";
            this.LabelDepthSliceDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LabelDepthSliceDetails.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
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
            this.ButtonPrevDepthSlice.Location = new System.Drawing.Point(1081, 3);
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
            this.ButtonNextDepthSlice.Location = new System.Drawing.Point(1170, 3);
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
            this.LabelDepthSlice.Location = new System.Drawing.Point(1007, 0);
            this.LabelDepthSlice.Name = "LabelDepthSlice";
            this.LabelDepthSlice.Size = new System.Drawing.Size(68, 28);
            this.LabelDepthSlice.TabIndex = 3;
            this.LabelDepthSlice.Text = "Depth slice:";
            this.LabelDepthSlice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelDepthSlice.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
            // 
            // LabelArrayIndexDetails
            // 
            this.LabelArrayIndexDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelArrayIndexDetails.AutoSize = true;
            this.LabelArrayIndexDetails.Location = new System.Drawing.Point(918, 0);
            this.LabelArrayIndexDetails.MinimumSize = new System.Drawing.Size(55, 0);
            this.LabelArrayIndexDetails.Name = "LabelArrayIndexDetails";
            this.LabelArrayIndexDetails.Size = new System.Drawing.Size(55, 28);
            this.LabelArrayIndexDetails.TabIndex = 9;
            this.LabelArrayIndexDetails.Text = "1/n";
            this.LabelArrayIndexDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LabelArrayIndexDetails.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
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
            this.ButtonPrevArrayIndex.Location = new System.Drawing.Point(890, 3);
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
            this.ButtonNextArrayIndex.Location = new System.Drawing.Point(979, 3);
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
            this.LabelArrayIndex.Location = new System.Drawing.Point(815, 0);
            this.LabelArrayIndex.Name = "LabelArrayIndex";
            this.LabelArrayIndex.Size = new System.Drawing.Size(69, 28);
            this.LabelArrayIndex.TabIndex = 3;
            this.LabelArrayIndex.Text = "Array index:";
            this.LabelArrayIndex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelArrayIndex.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
            // 
            // LabelMipDetails
            // 
            this.LabelMipDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelMipDetails.AutoSize = true;
            this.LabelMipDetails.Location = new System.Drawing.Point(666, 0);
            this.LabelMipDetails.MinimumSize = new System.Drawing.Size(115, 0);
            this.LabelMipDetails.Name = "LabelMipDetails";
            this.LabelMipDetails.Size = new System.Drawing.Size(115, 28);
            this.LabelMipDetails.TabIndex = 6;
            this.LabelMipDetails.Text = "1/n (WxH)";
            this.LabelMipDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LabelMipDetails.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
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
            this.ButtonPrevMip.Location = new System.Drawing.Point(638, 3);
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
            this.ButtonNextMip.Location = new System.Drawing.Point(787, 3);
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
            this.LabelMipLevel.Location = new System.Drawing.Point(574, 0);
            this.LabelMipLevel.Name = "LabelMipLevel";
            this.LabelMipLevel.Size = new System.Drawing.Size(58, 28);
            this.LabelMipLevel.TabIndex = 2;
            this.LabelMipLevel.Text = "Mip level:";
            this.LabelMipLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelMipLevel.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
            // 
            // TableViewControls
            // 
            this.TableViewControls.ColumnCount = 2;
            this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableViewControls.Controls.Add(this.ScrollHorizontal, 0, 1);
            this.TableViewControls.Controls.Add(this.ScrollVertical, 1, 0);
            this.TableViewControls.Controls.Add(this.PanelImage, 0, 0);
            this.TableViewControls.Controls.Add(this.ButtonCenter, 1, 1);
            this.TableViewControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableViewControls.Location = new System.Drawing.Point(0, 0);
            this.TableViewControls.Name = "TableViewControls";
            this.TableViewControls.RowCount = 2;
            this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableViewControls.Size = new System.Drawing.Size(1211, 748);
            this.TableViewControls.TabIndex = 0;
            // 
            // ScrollHorizontal
            // 
            this.ScrollHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScrollHorizontal.Enabled = false;
            this.ScrollHorizontal.Location = new System.Drawing.Point(0, 726);
            this.ScrollHorizontal.Maximum = 110;
            this.ScrollHorizontal.Minimum = -100;
            this.ScrollHorizontal.Name = "ScrollHorizontal";
            this.ScrollHorizontal.Size = new System.Drawing.Size(1189, 22);
            this.ScrollHorizontal.TabIndex = 0;
            this.ScrollHorizontal.ValueChanged += new System.EventHandler(this.ScrollVertical_ValueChanged);
            // 
            // ScrollVertical
            // 
            this.ScrollVertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScrollVertical.Enabled = false;
            this.ScrollVertical.Location = new System.Drawing.Point(1189, 0);
            this.ScrollVertical.Maximum = 110;
            this.ScrollVertical.Minimum = -100;
            this.ScrollVertical.Name = "ScrollVertical";
            this.ScrollVertical.Size = new System.Drawing.Size(22, 726);
            this.ScrollVertical.TabIndex = 1;
            this.ScrollVertical.ValueChanged += new System.EventHandler(this.ScrollVertical_ValueChanged);
            // 
            // PanelImage
            // 
            this.PanelImage.Controls.Add(this.GenMipMapSettings);
            this.PanelImage.Controls.Add(this.DimensionSettings);
            this.PanelImage.Controls.Add(this.CropResizeSettings);
            this.PanelImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelImage.Location = new System.Drawing.Point(3, 3);
            this.PanelImage.Name = "PanelImage";
            this.PanelImage.ShowFocus = false;
            this.PanelImage.Size = new System.Drawing.Size(1183, 720);
            this.PanelImage.TabIndex = 2;
            this.PanelImage.RenderToBitmap += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.PanelImage_RenderToBitmap);
            this.PanelImage.DoubleClick += new System.EventHandler(this.PanelImage_DoubleClick);
            this.PanelImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelImage_MouseDown);
            this.PanelImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelImage_MouseMove);
            this.PanelImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelImage_MouseUp);
            this.PanelImage.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
            // 
            // GenMipMapSettings
            // 
            this.GenMipMapSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.GenMipMapSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.GenMipMapSettings.ForeColor = System.Drawing.Color.White;
            this.GenMipMapSettings.Location = new System.Drawing.Point(53, 178);
            this.GenMipMapSettings.Name = "GenMipMapSettings";
            this.GenMipMapSettings.Size = new System.Drawing.Size(299, 165);
            this.GenMipMapSettings.TabIndex = 2;
            this.GenMipMapSettings.Visible = false;
            // 
            // DimensionSettings
            // 
            this.DimensionSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.DimensionSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.DimensionSettings.ForeColor = System.Drawing.Color.White;
            this.DimensionSettings.Location = new System.Drawing.Point(358, 178);
            this.DimensionSettings.Name = "DimensionSettings";
            this.DimensionSettings.Size = new System.Drawing.Size(299, 428);
            this.DimensionSettings.TabIndex = 1;
            this.DimensionSettings.Visible = false;
            // 
            // CropResizeSettings
            // 
            this.CropResizeSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CropResizeSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.CropResizeSettings.ForeColor = System.Drawing.Color.White;
            this.CropResizeSettings.Location = new System.Drawing.Point(683, 178);
            this.CropResizeSettings.Name = "CropResizeSettings";
            this.CropResizeSettings.Size = new System.Drawing.Size(461, 428);
            this.CropResizeSettings.TabIndex = 0;
            this.CropResizeSettings.Visible = false;
            // 
            // ButtonCenter
            // 
            this.ButtonCenter.AutoSize = true;
            this.ButtonCenter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonCenter.FlatAppearance.BorderSize = 0;
            this.ButtonCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
            this.ButtonCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCenter.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.center_16x16;
            this.ButtonCenter.Location = new System.Drawing.Point(1189, 726);
            this.ButtonCenter.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonCenter.Name = "ButtonCenter";
            this.ButtonCenter.Size = new System.Drawing.Size(22, 22);
            this.ButtonCenter.TabIndex = 3;
            this.ButtonCenter.Click += new System.EventHandler(this.ButtonCenter_Click);
            // 
            // ImageEditorView
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelBottomBar);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ImageEditorView";
            this.RenderControl = this.PanelImage;
            this.Size = new System.Drawing.Size(1211, 797);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ImageEditorView_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ImageEditorView_DragEnter);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelImage_PreviewKeyDown);
            this.Controls.SetChildIndex(this.PanelBottomBar, 0);
            this.Controls.SetChildIndex(this.PresentationPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.PresentationPanel)).EndInit();
            this.PresentationPanel.ResumeLayout(false);
            this.PanelBottomBar.ResumeLayout(false);
            this.PanelBottomBar.PerformLayout();
            this.PanelImageViewControls.ResumeLayout(false);
            this.PanelImageViewControls.PerformLayout();
            this.TableViewControls.ResumeLayout(false);
            this.TableViewControls.PerformLayout();
            this.PanelImage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PanelBottomBar;
        private System.Windows.Forms.TableLayoutPanel PanelImageViewControls;
        private System.Windows.Forms.TableLayoutPanel TableViewControls;
        private System.Windows.Forms.HScrollBar ScrollHorizontal;
        private System.Windows.Forms.VScrollBar ScrollVertical;
        private Gorgon.Windows.UI.GorgonSelectablePanel PanelImage;
        private System.Windows.Forms.Button ButtonCenter;
        private System.Windows.Forms.Label LabelArrayIndex;
        private System.Windows.Forms.Label LabelMipLevel;
        private System.Windows.Forms.Button ButtonPrevMip;
        private System.Windows.Forms.Button ButtonNextMip;
        private System.Windows.Forms.Label LabelMipDetails;
        private System.Windows.Forms.Button ButtonNextArrayIndex;
        private System.Windows.Forms.Label LabelArrayIndexDetails;
        private System.Windows.Forms.Button ButtonPrevArrayIndex;
        private System.Windows.Forms.Label LabelDepthSliceDetails;
        private System.Windows.Forms.Button ButtonPrevDepthSlice;
        private System.Windows.Forms.Button ButtonNextDepthSlice;
        private System.Windows.Forms.Label LabelDepthSlice;
        private ImageResizeSettings CropResizeSettings;
        private System.Windows.Forms.Label LabelImageSize;
        private ImageDimensionSettings DimensionSettings;
        private GenMipMapSettings GenMipMapSettings;
    }
}
