namespace Gorgon.Editor.ImageEditor;

partial class FormImagePicker
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

            _progressPanel?.SetDataContext(null);
            _progressPanel?.Dispose();
            _waitPanel?.Dispose();

            _textureParameters?.Dispose();
            _imageShader2D?.Dispose();
            _imageShader3D?.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImagePicker));
        this.PanelDialogButtons = new System.Windows.Forms.Panel();
        this.ButtonOK = new System.Windows.Forms.Button();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.LabelImportDesc = new System.Windows.Forms.Label();
        this.ListImages = new System.Windows.Forms.ListView();
        this.ImageFiles = new System.Windows.Forms.ImageList(this.components);
        this.TableArrayDepthControls = new System.Windows.Forms.TableLayoutPanel();
        this.ButtonPrevMip = new System.Windows.Forms.Button();
        this.ButtonNextMip = new System.Windows.Forms.Button();
        this.ButtonImport = new System.Windows.Forms.Button();
        this.LabelMipMapLevel = new System.Windows.Forms.Label();
        this.LabelArrayDepthIndex = new System.Windows.Forms.Label();
        this.ButtonPrevArrayDepth = new System.Windows.Forms.Button();
        this.ButtonRestore = new System.Windows.Forms.Button();
        this.ButtonNextArrayDepth = new System.Windows.Forms.Button();
        this.LabelMipMaps = new System.Windows.Forms.Label();
        this.LabelArrayDepth = new System.Windows.Forms.Label();
        this.PanelArrayDepth = new System.Windows.Forms.Panel();
        this.TipsButtons = new System.Windows.Forms.ToolTip(this.components);
        this.ButtonPrevSrcMip = new System.Windows.Forms.Button();
        this.ButtonNextSrcMip = new System.Windows.Forms.Button();
        this.ButtonPrevSrcArray = new System.Windows.Forms.Button();
        this.ButtonNextSrcArray = new System.Windows.Forms.Button();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.ButtonCropResizeOk = new System.Windows.Forms.Button();
        this.ButtonCropResizeCancel = new System.Windows.Forms.Button();
        this.LabelImageFilter = new System.Windows.Forms.Label();
        this.LabelDesc = new System.Windows.Forms.Label();
        this.LabelImportImageDimensions = new System.Windows.Forms.Label();
        this.LabelTargetImageDimensions = new System.Windows.Forms.Label();
        this.RadioCrop = new System.Windows.Forms.RadioButton();
        this.AlignmentPicker = new Gorgon.UI.GorgonAlignmentPicker();
        this.LabelAnchor = new System.Windows.Forms.Label();
        this.RadioResize = new System.Windows.Forms.RadioButton();
        this.PanelFilter = new System.Windows.Forms.Panel();
        this.ComboImageFilter = new System.Windows.Forms.ComboBox();
        this.CheckPreserveAspect = new System.Windows.Forms.CheckBox();
        this.TableResize = new System.Windows.Forms.TableLayoutPanel();
        this.LabelResizeAnchor = new System.Windows.Forms.Label();
        this.AlignmentResize = new Gorgon.UI.GorgonAlignmentPicker();
        this.TablePickSourceImage = new System.Windows.Forms.TableLayoutPanel();
        this.LabelSourceImport = new System.Windows.Forms.Label();
        this.TableSourceArrayDepth = new System.Windows.Forms.TableLayoutPanel();
        this.LabelSrcArray = new System.Windows.Forms.Label();
        this.LabelSourceArrayDepth = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.LabelSrcMips = new System.Windows.Forms.Label();
        this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
        this.ButtonSrcImport = new System.Windows.Forms.Button();
        this.ButtonSrcCancel = new System.Windows.Forms.Button();
        this.PanelSourceImage = new System.Windows.Forms.Panel();
        this.TipsFiles = new System.Windows.Forms.ToolTip(this.components);
        this.PanelDialogButtons.SuspendLayout();
        this.TableControls.SuspendLayout();
        this.TableArrayDepthControls.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        this.PanelFilter.SuspendLayout();
        this.TableResize.SuspendLayout();
        this.TablePickSourceImage.SuspendLayout();
        this.TableSourceArrayDepth.SuspendLayout();
        this.tableLayoutPanel2.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelDialogButtons
        // 
        this.PanelDialogButtons.AutoSize = true;
        this.PanelDialogButtons.Controls.Add(this.ButtonOK);
        this.PanelDialogButtons.Controls.Add(this.ButtonCancel);
        this.PanelDialogButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.PanelDialogButtons.Location = new System.Drawing.Point(0, 523);
        this.PanelDialogButtons.Name = "PanelDialogButtons";
        this.PanelDialogButtons.Size = new System.Drawing.Size(784, 38);
        this.PanelDialogButtons.TabIndex = 1;
        // 
        // ButtonOK
        // 
        this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonOK.AutoSize = true;
        this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonOK.Enabled = false;
        this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOK.Location = new System.Drawing.Point(606, 3);
        this.ButtonOK.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
        this.ButtonOK.Name = "ButtonOK";
        this.ButtonOK.Size = new System.Drawing.Size(80, 29);
        this.ButtonOK.TabIndex = 0;
        this.ButtonOK.Text = "&OK";
        this.ButtonOK.UseVisualStyleBackColor = false;
        this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(692, 3);
        this.ButtonCancel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(80, 29);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        // 
        // TableControls
        // 
        this.TableControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableControls.ColumnCount = 2;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableControls.Controls.Add(this.LabelImportDesc, 0, 1);
        this.TableControls.Controls.Add(this.ListImages, 0, 0);
        this.TableControls.Controls.Add(this.TableArrayDepthControls, 0, 3);
        this.TableControls.Controls.Add(this.PanelArrayDepth, 0, 2);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(0, 0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 4;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.Size = new System.Drawing.Size(176, 523);
        this.TableControls.TabIndex = 0;
        // 
        // LabelImportDesc
        // 
        this.LabelImportDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
        this.LabelImportDesc.AutoSize = true;
        this.LabelImportDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.LabelImportDesc.Location = new System.Drawing.Point(3, 279);
        this.LabelImportDesc.Margin = new System.Windows.Forms.Padding(3);
        this.LabelImportDesc.Name = "LabelImportDesc";
        this.LabelImportDesc.Size = new System.Drawing.Size(163, 30);
        this.LabelImportDesc.TabIndex = 0;
        this.LabelImportDesc.Text = "Drag an image into the panel below to import into {0}.";
        // 
        // ListImages
        // 
        this.ListImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ListImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.ListImages.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.ListImages.ForeColor = System.Drawing.Color.White;
        this.ListImages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
        this.ListImages.HideSelection = false;
        this.ListImages.LargeImageList = this.ImageFiles;
        this.ListImages.Location = new System.Drawing.Point(3, 3);
        this.ListImages.MinimumSize = new System.Drawing.Size(0, 270);
        this.ListImages.MultiSelect = false;
        this.ListImages.Name = "ListImages";
        this.ListImages.Size = new System.Drawing.Size(170, 270);
        this.ListImages.SmallImageList = this.ImageFiles;
        this.ListImages.Sorting = System.Windows.Forms.SortOrder.Ascending;
        this.ListImages.TabIndex = 1;
        this.ListImages.UseCompatibleStateImageBehavior = false;
        this.ListImages.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ListImages_ItemDrag);
        this.ListImages.ItemMouseHover += new System.Windows.Forms.ListViewItemMouseHoverEventHandler(this.ListImages_ItemMouseHover);
        this.ListImages.SelectedIndexChanged += new System.EventHandler(this.ListImages_SelectedIndexChanged);
        this.ListImages.DoubleClick += new System.EventHandler(this.ListImages_DoubleClick);
        this.ListImages.Leave += new System.EventHandler(this.ListImages_Leave);
        this.ListImages.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListImages_MouseUp);
        // 
        // ImageFiles
        // 
        this.ImageFiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
        this.ImageFiles.ImageSize = new System.Drawing.Size(128, 128);
        this.ImageFiles.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // TableArrayDepthControls
        // 
        this.TableArrayDepthControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.TableArrayDepthControls.AutoSize = true;
        this.TableArrayDepthControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.TableArrayDepthControls.ColumnCount = 10;
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableArrayDepthControls.Controls.Add(this.ButtonPrevMip, 7, 0);
        this.TableArrayDepthControls.Controls.Add(this.ButtonNextMip, 9, 0);
        this.TableArrayDepthControls.Controls.Add(this.ButtonImport, 0, 0);
        this.TableArrayDepthControls.Controls.Add(this.LabelMipMapLevel, 8, 0);
        this.TableArrayDepthControls.Controls.Add(this.LabelArrayDepthIndex, 4, 0);
        this.TableArrayDepthControls.Controls.Add(this.ButtonPrevArrayDepth, 3, 0);
        this.TableArrayDepthControls.Controls.Add(this.ButtonRestore, 1, 0);
        this.TableArrayDepthControls.Controls.Add(this.ButtonNextArrayDepth, 5, 0);
        this.TableArrayDepthControls.Controls.Add(this.LabelMipMaps, 6, 0);
        this.TableArrayDepthControls.Controls.Add(this.LabelArrayDepth, 2, 0);
        this.TableArrayDepthControls.Location = new System.Drawing.Point(3, 485);
        this.TableArrayDepthControls.Name = "TableArrayDepthControls";
        this.TableArrayDepthControls.RowCount = 1;
        this.TableArrayDepthControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableArrayDepthControls.Size = new System.Drawing.Size(170, 35);
        this.TableArrayDepthControls.TabIndex = 5;
        // 
        // ButtonPrevMip
        // 
        this.ButtonPrevMip.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.ButtonPrevMip.AutoSize = true;
        this.ButtonPrevMip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevMip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonPrevMip.Enabled = false;
        this.ButtonPrevMip.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonPrevMip.FlatAppearance.BorderSize = 2;
        this.ButtonPrevMip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonPrevMip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevMip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevMip.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.left_16x16;
        this.ButtonPrevMip.Location = new System.Drawing.Point(-88, 4);
        this.ButtonPrevMip.Name = "ButtonPrevMip";
        this.ButtonPrevMip.Size = new System.Drawing.Size(26, 26);
        this.ButtonPrevMip.TabIndex = 5;
        this.TipsButtons.SetToolTip(this.ButtonPrevMip, "Go to previous mip level.");
        this.ButtonPrevMip.UseVisualStyleBackColor = false;
        this.ButtonPrevMip.Click += new System.EventHandler(this.ButtonPrevMip_Click);
        // 
        // ButtonNextMip
        // 
        this.ButtonNextMip.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonNextMip.AutoSize = true;
        this.ButtonNextMip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextMip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonNextMip.Enabled = false;
        this.ButtonNextMip.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonNextMip.FlatAppearance.BorderSize = 2;
        this.ButtonNextMip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonNextMip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextMip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextMip.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.right_16x16;
        this.ButtonNextMip.Location = new System.Drawing.Point(141, 4);
        this.ButtonNextMip.Name = "ButtonNextMip";
        this.ButtonNextMip.Size = new System.Drawing.Size(26, 26);
        this.ButtonNextMip.TabIndex = 6;
        this.TipsButtons.SetToolTip(this.ButtonNextMip, "Go to next mip level.");
        this.ButtonNextMip.UseVisualStyleBackColor = false;
        this.ButtonNextMip.Click += new System.EventHandler(this.ButtonNextMip_Click);
        // 
        // ButtonImport
        // 
        this.ButtonImport.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.ButtonImport.AutoSize = true;
        this.ButtonImport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonImport.Enabled = false;
        this.ButtonImport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonImport.FlatAppearance.BorderSize = 2;
        this.ButtonImport.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonImport.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonImport.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.save_content_16x16;
        this.ButtonImport.Location = new System.Drawing.Point(16, 3);
        this.ButtonImport.Margin = new System.Windows.Forms.Padding(16, 3, 16, 3);
        this.ButtonImport.Name = "ButtonImport";
        this.ButtonImport.Size = new System.Drawing.Size(73, 29);
        this.ButtonImport.TabIndex = 3;
        this.ButtonImport.Text = "&Import";
        this.ButtonImport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.TipsButtons.SetToolTip(this.ButtonImport, "Import the current image into the current image subresource.");
        this.ButtonImport.UseVisualStyleBackColor = false;
        this.ButtonImport.Click += new System.EventHandler(this.ButtonImport_Click);
        // 
        // LabelMipMapLevel
        // 
        this.LabelMipMapLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelMipMapLevel.AutoSize = true;
        this.LabelMipMapLevel.Location = new System.Drawing.Point(-53, 10);
        this.LabelMipMapLevel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.LabelMipMapLevel.MinimumSize = new System.Drawing.Size(115, 0);
        this.LabelMipMapLevel.Name = "LabelMipMapLevel";
        this.LabelMipMapLevel.Size = new System.Drawing.Size(185, 15);
        this.LabelMipMapLevel.TabIndex = 7;
        this.LabelMipMapLevel.Text = "99999/99999 (99999x99999x99999)";
        this.LabelMipMapLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // LabelArrayDepthIndex
        // 
        this.LabelArrayDepthIndex.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelArrayDepthIndex.AutoSize = true;
        this.LabelArrayDepthIndex.Location = new System.Drawing.Point(-274, 10);
        this.LabelArrayDepthIndex.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.LabelArrayDepthIndex.MinimumSize = new System.Drawing.Size(55, 0);
        this.LabelArrayDepthIndex.Name = "LabelArrayDepthIndex";
        this.LabelArrayDepthIndex.Size = new System.Drawing.Size(72, 15);
        this.LabelArrayDepthIndex.TabIndex = 9;
        this.LabelArrayDepthIndex.Text = "99999/99999";
        this.LabelArrayDepthIndex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ButtonPrevArrayDepth
        // 
        this.ButtonPrevArrayDepth.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.ButtonPrevArrayDepth.AutoSize = true;
        this.ButtonPrevArrayDepth.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevArrayDepth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonPrevArrayDepth.Enabled = false;
        this.ButtonPrevArrayDepth.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonPrevArrayDepth.FlatAppearance.BorderSize = 2;
        this.ButtonPrevArrayDepth.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonPrevArrayDepth.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevArrayDepth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevArrayDepth.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.left_16x16;
        this.ButtonPrevArrayDepth.Location = new System.Drawing.Point(-309, 4);
        this.ButtonPrevArrayDepth.Name = "ButtonPrevArrayDepth";
        this.ButtonPrevArrayDepth.Size = new System.Drawing.Size(26, 26);
        this.ButtonPrevArrayDepth.TabIndex = 10;
        this.TipsButtons.SetToolTip(this.ButtonPrevArrayDepth, "Go to previous array/depth slice.");
        this.ButtonPrevArrayDepth.UseVisualStyleBackColor = false;
        this.ButtonPrevArrayDepth.Click += new System.EventHandler(this.ButtonPrevArrayDepth_Click);
        // 
        // ButtonRestore
        // 
        this.ButtonRestore.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.ButtonRestore.AutoSize = true;
        this.ButtonRestore.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonRestore.Enabled = false;
        this.ButtonRestore.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonRestore.FlatAppearance.BorderSize = 2;
        this.ButtonRestore.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonRestore.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonRestore.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.undo_16x16;
        this.ButtonRestore.Location = new System.Drawing.Point(108, 3);
        this.ButtonRestore.Margin = new System.Windows.Forms.Padding(3, 3, 16, 3);
        this.ButtonRestore.Name = "ButtonRestore";
        this.ButtonRestore.Size = new System.Drawing.Size(76, 29);
        this.ButtonRestore.TabIndex = 4;
        this.ButtonRestore.Text = "&Restore";
        this.ButtonRestore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.TipsButtons.SetToolTip(this.ButtonRestore, "Reset this back to the original image sub resource.");
        this.ButtonRestore.UseVisualStyleBackColor = false;
        this.ButtonRestore.Click += new System.EventHandler(this.ButtonRestore_Click);
        // 
        // ButtonNextArrayDepth
        // 
        this.ButtonNextArrayDepth.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonNextArrayDepth.AutoSize = true;
        this.ButtonNextArrayDepth.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextArrayDepth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonNextArrayDepth.Enabled = false;
        this.ButtonNextArrayDepth.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonNextArrayDepth.FlatAppearance.BorderSize = 2;
        this.ButtonNextArrayDepth.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonNextArrayDepth.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextArrayDepth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextArrayDepth.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.right_16x16;
        this.ButtonNextArrayDepth.Location = new System.Drawing.Point(-193, 4);
        this.ButtonNextArrayDepth.Name = "ButtonNextArrayDepth";
        this.ButtonNextArrayDepth.Size = new System.Drawing.Size(26, 26);
        this.ButtonNextArrayDepth.TabIndex = 8;
        this.TipsButtons.SetToolTip(this.ButtonNextArrayDepth, "Go to next array/depth slice.");
        this.ButtonNextArrayDepth.UseVisualStyleBackColor = false;
        this.ButtonNextArrayDepth.Click += new System.EventHandler(this.ButtonNextArrayDepth_Click);
        // 
        // LabelMipMaps
        // 
        this.LabelMipMaps.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelMipMaps.AutoSize = true;
        this.LabelMipMaps.Location = new System.Drawing.Point(-158, 10);
        this.LabelMipMaps.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.LabelMipMaps.Name = "LabelMipMaps";
        this.LabelMipMaps.Size = new System.Drawing.Size(61, 15);
        this.LabelMipMaps.TabIndex = 11;
        this.LabelMipMaps.Text = "Mip Level:";
        this.LabelMipMaps.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // LabelArrayDepth
        // 
        this.LabelArrayDepth.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelArrayDepth.AutoSize = true;
        this.LabelArrayDepth.Location = new System.Drawing.Point(206, 10);
        this.LabelArrayDepth.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.LabelArrayDepth.Name = "LabelArrayDepth";
        this.LabelArrayDepth.Size = new System.Drawing.Size(1, 15);
        this.LabelArrayDepth.TabIndex = 12;
        this.LabelArrayDepth.Text = "Array Index:";
        this.LabelArrayDepth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // PanelArrayDepth
        // 
        this.PanelArrayDepth.AllowDrop = true;
        this.PanelArrayDepth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.PanelArrayDepth.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelArrayDepth.Location = new System.Drawing.Point(3, 315);
        this.PanelArrayDepth.Name = "PanelArrayDepth";
        this.PanelArrayDepth.Size = new System.Drawing.Size(170, 164);
        this.PanelArrayDepth.TabIndex = 4;
        this.TipsButtons.SetToolTip(this.PanelArrayDepth, "Drop file here.");
        this.PanelArrayDepth.DragDrop += new System.Windows.Forms.DragEventHandler(this.PanelArrayDepth_DragDrop);
        this.PanelArrayDepth.DragOver += new System.Windows.Forms.DragEventHandler(this.PanelArrayDepth_DragOver);
        // 
        // TipsButtons
        // 
        this.TipsButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.TipsButtons.ForeColor = System.Drawing.Color.White;
        // 
        // ButtonPrevSrcMip
        // 
        this.ButtonPrevSrcMip.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.ButtonPrevSrcMip.AutoSize = true;
        this.ButtonPrevSrcMip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevSrcMip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonPrevSrcMip.Enabled = false;
        this.ButtonPrevSrcMip.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonPrevSrcMip.FlatAppearance.BorderSize = 2;
        this.ButtonPrevSrcMip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonPrevSrcMip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevSrcMip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevSrcMip.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.left_16x16;
        this.ButtonPrevSrcMip.Location = new System.Drawing.Point(82, 35);
        this.ButtonPrevSrcMip.Name = "ButtonPrevSrcMip";
        this.ButtonPrevSrcMip.Size = new System.Drawing.Size(26, 26);
        this.ButtonPrevSrcMip.TabIndex = 5;
        this.TipsButtons.SetToolTip(this.ButtonPrevSrcMip, "Go to previous mip level.");
        this.ButtonPrevSrcMip.UseVisualStyleBackColor = false;
        this.ButtonPrevSrcMip.Click += new System.EventHandler(this.ButtonPrevSrcMip_Click);
        // 
        // ButtonNextSrcMip
        // 
        this.ButtonNextSrcMip.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonNextSrcMip.AutoSize = true;
        this.ButtonNextSrcMip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextSrcMip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonNextSrcMip.Enabled = false;
        this.ButtonNextSrcMip.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonNextSrcMip.FlatAppearance.BorderSize = 2;
        this.ButtonNextSrcMip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonNextSrcMip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextSrcMip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextSrcMip.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.right_16x16;
        this.ButtonNextSrcMip.Location = new System.Drawing.Point(269, 35);
        this.ButtonNextSrcMip.Name = "ButtonNextSrcMip";
        this.ButtonNextSrcMip.Size = new System.Drawing.Size(26, 26);
        this.ButtonNextSrcMip.TabIndex = 6;
        this.TipsButtons.SetToolTip(this.ButtonNextSrcMip, "Go to next mip level.");
        this.ButtonNextSrcMip.UseVisualStyleBackColor = false;
        this.ButtonNextSrcMip.Click += new System.EventHandler(this.ButtonNextSrcMip_Click);
        // 
        // ButtonPrevSrcArray
        // 
        this.ButtonPrevSrcArray.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.ButtonPrevSrcArray.AutoSize = true;
        this.ButtonPrevSrcArray.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevSrcArray.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonPrevSrcArray.Enabled = false;
        this.ButtonPrevSrcArray.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonPrevSrcArray.FlatAppearance.BorderSize = 2;
        this.ButtonPrevSrcArray.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonPrevSrcArray.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevSrcArray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevSrcArray.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.left_16x16;
        this.ButtonPrevSrcArray.Location = new System.Drawing.Point(82, 3);
        this.ButtonPrevSrcArray.Name = "ButtonPrevSrcArray";
        this.ButtonPrevSrcArray.Size = new System.Drawing.Size(26, 26);
        this.ButtonPrevSrcArray.TabIndex = 10;
        this.TipsButtons.SetToolTip(this.ButtonPrevSrcArray, "Go to previous array/depth slice.");
        this.ButtonPrevSrcArray.UseVisualStyleBackColor = false;
        this.ButtonPrevSrcArray.Click += new System.EventHandler(this.ButtonPrevSrcArray_Click);
        // 
        // ButtonNextSrcArray
        // 
        this.ButtonNextSrcArray.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonNextSrcArray.AutoSize = true;
        this.ButtonNextSrcArray.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextSrcArray.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonNextSrcArray.Enabled = false;
        this.ButtonNextSrcArray.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonNextSrcArray.FlatAppearance.BorderSize = 2;
        this.ButtonNextSrcArray.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonNextSrcArray.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextSrcArray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextSrcArray.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.right_16x16;
        this.ButtonNextSrcArray.Location = new System.Drawing.Point(269, 3);
        this.ButtonNextSrcArray.Name = "ButtonNextSrcArray";
        this.ButtonNextSrcArray.Size = new System.Drawing.Size(26, 26);
        this.ButtonNextSrcArray.TabIndex = 8;
        this.TipsButtons.SetToolTip(this.ButtonNextSrcArray, "Go to next array/depth slice.");
        this.ButtonNextSrcArray.UseVisualStyleBackColor = false;
        this.ButtonNextSrcArray.Click += new System.EventHandler(this.ButtonNextSrcArray_Click);
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.tableLayoutPanel1.ColumnCount = 2;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.Controls.Add(this.ButtonCropResizeOk, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.ButtonCropResizeCancel, 1, 0);
        this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 540);
        this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 1;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(298, 1);
        this.tableLayoutPanel1.TabIndex = 11;
        // 
        // ButtonCropResizeOk
        // 
        this.ButtonCropResizeOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCropResizeOk.AutoSize = true;
        this.ButtonCropResizeOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCropResizeOk.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCropResizeOk.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCropResizeOk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonCropResizeOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCropResizeOk.Location = new System.Drawing.Point(126, 3);
        this.ButtonCropResizeOk.Name = "ButtonCropResizeOk";
        this.ButtonCropResizeOk.Size = new System.Drawing.Size(80, 1);
        this.ButtonCropResizeOk.TabIndex = 0;
        this.ButtonCropResizeOk.Text = "&Update";
        this.ButtonCropResizeOk.UseVisualStyleBackColor = false;
        this.ButtonCropResizeOk.Click += new System.EventHandler(this.ButtonCropResizeOk_Click);
        // 
        // ButtonCropResizeCancel
        // 
        this.ButtonCropResizeCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCropResizeCancel.AutoSize = true;
        this.ButtonCropResizeCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCropResizeCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCropResizeCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCropResizeCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonCropResizeCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCropResizeCancel.Location = new System.Drawing.Point(212, 3);
        this.ButtonCropResizeCancel.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        this.ButtonCropResizeCancel.Name = "ButtonCropResizeCancel";
        this.ButtonCropResizeCancel.Size = new System.Drawing.Size(80, 1);
        this.ButtonCropResizeCancel.TabIndex = 1;
        this.ButtonCropResizeCancel.Text = "&Cancel";
        this.ButtonCropResizeCancel.UseVisualStyleBackColor = false;
        this.ButtonCropResizeCancel.Click += new System.EventHandler(this.ButtonCropResizeCancel_Click);
        // 
        // LabelImageFilter
        // 
        this.LabelImageFilter.AutoSize = true;
        this.LabelImageFilter.Location = new System.Drawing.Point(24, 339);
        this.LabelImageFilter.Margin = new System.Windows.Forms.Padding(24, 0, 3, 0);
        this.LabelImageFilter.Name = "LabelImageFilter";
        this.LabelImageFilter.Size = new System.Drawing.Size(84, 15);
        this.LabelImageFilter.TabIndex = 7;
        this.LabelImageFilter.Text = "Image filtering";
        this.LabelImageFilter.Visible = false;
        // 
        // LabelDesc
        // 
        this.LabelDesc.AutoEllipsis = true;
        this.LabelDesc.AutoSize = true;
        this.LabelDesc.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelDesc.Location = new System.Drawing.Point(3, 5);
        this.LabelDesc.Margin = new System.Windows.Forms.Padding(3, 5, 3, 16);
        this.LabelDesc.MaximumSize = new System.Drawing.Size(300, 300);
        this.LabelDesc.Name = "LabelDesc";
        this.LabelDesc.Size = new System.Drawing.Size(298, 90);
        this.LabelDesc.TabIndex = 0;
        this.LabelDesc.Text = resources.GetString("LabelDesc.Text");
        // 
        // LabelImportImageDimensions
        // 
        this.LabelImportImageDimensions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelImportImageDimensions.AutoSize = true;
        this.LabelImportImageDimensions.Location = new System.Drawing.Point(3, 114);
        this.LabelImportImageDimensions.Margin = new System.Windows.Forms.Padding(3);
        this.LabelImportImageDimensions.Name = "LabelImportImageDimensions";
        this.LabelImportImageDimensions.Size = new System.Drawing.Size(298, 15);
        this.LabelImportImageDimensions.TabIndex = 1;
        this.LabelImportImageDimensions.Text = "Import image dimensions: {0}x{1}";
        // 
        // LabelTargetImageDimensions
        // 
        this.LabelTargetImageDimensions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelTargetImageDimensions.AutoSize = true;
        this.LabelTargetImageDimensions.Location = new System.Drawing.Point(3, 135);
        this.LabelTargetImageDimensions.Margin = new System.Windows.Forms.Padding(3);
        this.LabelTargetImageDimensions.Name = "LabelTargetImageDimensions";
        this.LabelTargetImageDimensions.Size = new System.Drawing.Size(298, 15);
        this.LabelTargetImageDimensions.TabIndex = 2;
        this.LabelTargetImageDimensions.Text = "Target image dimensions: {0}x{1}";
        // 
        // RadioCrop
        // 
        this.RadioCrop.AutoSize = true;
        this.RadioCrop.Checked = true;
        this.RadioCrop.Location = new System.Drawing.Point(3, 169);
        this.RadioCrop.Margin = new System.Windows.Forms.Padding(3, 16, 3, 3);
        this.RadioCrop.Name = "RadioCrop";
        this.RadioCrop.Size = new System.Drawing.Size(102, 19);
        this.RadioCrop.TabIndex = 3;
        this.RadioCrop.TabStop = true;
        this.RadioCrop.Text = "Crop to {0}x{1}";
        this.RadioCrop.UseVisualStyleBackColor = true;
        this.RadioCrop.CheckedChanged += new System.EventHandler(this.RadioCrop_CheckedChanged);
        // 
        // AlignmentPicker
        // 
        this.AlignmentPicker.Location = new System.Drawing.Point(24, 209);
        this.AlignmentPicker.Margin = new System.Windows.Forms.Padding(24, 3, 3, 0);
        this.AlignmentPicker.Name = "AlignmentPicker";
        this.AlignmentPicker.Size = new System.Drawing.Size(105, 105);
        this.AlignmentPicker.TabIndex = 5;
        this.AlignmentPicker.Visible = false;
        this.AlignmentPicker.AlignmentChanged += new System.EventHandler(this.AlignmentPicker_AlignmentChanged);
        // 
        // LabelAnchor
        // 
        this.LabelAnchor.AutoSize = true;
        this.LabelAnchor.Location = new System.Drawing.Point(24, 191);
        this.LabelAnchor.Margin = new System.Windows.Forms.Padding(24, 0, 3, 0);
        this.LabelAnchor.Name = "LabelAnchor";
        this.LabelAnchor.Size = new System.Drawing.Size(63, 15);
        this.LabelAnchor.TabIndex = 4;
        this.LabelAnchor.Text = "Alignment";
        this.LabelAnchor.Visible = false;
        // 
        // RadioResize
        // 
        this.RadioResize.AutoSize = true;
        this.RadioResize.Location = new System.Drawing.Point(3, 317);
        this.RadioResize.Name = "RadioResize";
        this.RadioResize.Size = new System.Drawing.Size(108, 19);
        this.RadioResize.TabIndex = 6;
        this.RadioResize.Text = "Resize to {0}x{1}";
        this.RadioResize.UseVisualStyleBackColor = true;
        this.RadioResize.CheckedChanged += new System.EventHandler(this.RadioResize_CheckedChanged);
        // 
        // PanelFilter
        // 
        this.PanelFilter.AutoSize = true;
        this.PanelFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelFilter.Controls.Add(this.ComboImageFilter);
        this.PanelFilter.Controls.Add(this.CheckPreserveAspect);
        this.PanelFilter.Location = new System.Drawing.Point(24, 357);
        this.PanelFilter.Margin = new System.Windows.Forms.Padding(24, 3, 3, 0);
        this.PanelFilter.Name = "PanelFilter";
        this.PanelFilter.Size = new System.Drawing.Size(235, 54);
        this.PanelFilter.TabIndex = 8;
        this.PanelFilter.Visible = false;
        // 
        // ComboImageFilter
        // 
        this.ComboImageFilter.Dock = System.Windows.Forms.DockStyle.Top;
        this.ComboImageFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.ComboImageFilter.FormattingEnabled = true;
        this.ComboImageFilter.Location = new System.Drawing.Point(0, 0);
        this.ComboImageFilter.Name = "ComboImageFilter";
        this.ComboImageFilter.Size = new System.Drawing.Size(235, 23);
        this.ComboImageFilter.TabIndex = 0;
        this.ComboImageFilter.SelectedIndexChanged += new System.EventHandler(this.ComboImageFilter_SelectedIndexChanged);
        // 
        // CheckPreserveAspect
        // 
        this.CheckPreserveAspect.AutoSize = true;
        this.CheckPreserveAspect.Checked = true;
        this.CheckPreserveAspect.CheckState = System.Windows.Forms.CheckState.Checked;
        this.CheckPreserveAspect.Location = new System.Drawing.Point(3, 32);
        this.CheckPreserveAspect.Name = "CheckPreserveAspect";
        this.CheckPreserveAspect.Size = new System.Drawing.Size(229, 19);
        this.CheckPreserveAspect.TabIndex = 1;
        this.CheckPreserveAspect.Text = "Preserve the aspect ratio of the image?";
        this.CheckPreserveAspect.UseVisualStyleBackColor = true;
        this.CheckPreserveAspect.CheckedChanged += new System.EventHandler(this.CheckPreserveAspect_CheckedChanged);
        // 
        // TableResize
        // 
        this.TableResize.AutoSize = true;
        this.TableResize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableResize.ColumnCount = 1;
        this.TableResize.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableResize.Controls.Add(this.LabelResizeAnchor, 0, 9);
        this.TableResize.Controls.Add(this.AlignmentResize, 0, 10);
        this.TableResize.Controls.Add(this.PanelFilter, 0, 8);
        this.TableResize.Controls.Add(this.RadioResize, 0, 6);
        this.TableResize.Controls.Add(this.LabelAnchor, 0, 4);
        this.TableResize.Controls.Add(this.AlignmentPicker, 0, 5);
        this.TableResize.Controls.Add(this.RadioCrop, 0, 3);
        this.TableResize.Controls.Add(this.LabelTargetImageDimensions, 0, 2);
        this.TableResize.Controls.Add(this.LabelImportImageDimensions, 0, 1);
        this.TableResize.Controls.Add(this.LabelDesc, 0, 0);
        this.TableResize.Controls.Add(this.LabelImageFilter, 0, 7);
        this.TableResize.Controls.Add(this.tableLayoutPanel1, 0, 11);
        this.TableResize.Dock = System.Windows.Forms.DockStyle.Right;
        this.TableResize.Location = new System.Drawing.Point(176, 0);
        this.TableResize.Name = "TableResize";
        this.TableResize.RowCount = 12;
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableResize.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableResize.Size = new System.Drawing.Size(304, 523);
        this.TableResize.TabIndex = 6;
        this.TableResize.Visible = false;
        // 
        // LabelResizeAnchor
        // 
        this.LabelResizeAnchor.AutoSize = true;
        this.LabelResizeAnchor.Location = new System.Drawing.Point(24, 411);
        this.LabelResizeAnchor.Margin = new System.Windows.Forms.Padding(24, 0, 3, 0);
        this.LabelResizeAnchor.Name = "LabelResizeAnchor";
        this.LabelResizeAnchor.Size = new System.Drawing.Size(63, 15);
        this.LabelResizeAnchor.TabIndex = 9;
        this.LabelResizeAnchor.Text = "Alignment";
        this.LabelResizeAnchor.Visible = false;
        // 
        // AlignmentResize
        // 
        this.AlignmentResize.Location = new System.Drawing.Point(24, 429);
        this.AlignmentResize.Margin = new System.Windows.Forms.Padding(24, 3, 3, 0);
        this.AlignmentResize.Name = "AlignmentResize";
        this.AlignmentResize.Size = new System.Drawing.Size(105, 105);
        this.AlignmentResize.TabIndex = 10;
        this.AlignmentResize.Visible = false;
        this.AlignmentResize.AlignmentChanged += new System.EventHandler(this.AlignmentPicker_AlignmentChanged);
        // 
        // TablePickSourceImage
        // 
        this.TablePickSourceImage.ColumnCount = 1;
        this.TablePickSourceImage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TablePickSourceImage.Controls.Add(this.LabelSourceImport, 0, 0);
        this.TablePickSourceImage.Controls.Add(this.TableSourceArrayDepth, 0, 2);
        this.TablePickSourceImage.Controls.Add(this.tableLayoutPanel2, 0, 3);
        this.TablePickSourceImage.Controls.Add(this.PanelSourceImage, 0, 1);
        this.TablePickSourceImage.Dock = System.Windows.Forms.DockStyle.Right;
        this.TablePickSourceImage.Location = new System.Drawing.Point(480, 0);
        this.TablePickSourceImage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        this.TablePickSourceImage.Name = "TablePickSourceImage";
        this.TablePickSourceImage.RowCount = 4;
        this.TablePickSourceImage.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TablePickSourceImage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 256F));
        this.TablePickSourceImage.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TablePickSourceImage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TablePickSourceImage.Size = new System.Drawing.Size(304, 523);
        this.TablePickSourceImage.TabIndex = 0;
        this.TablePickSourceImage.Visible = false;
        // 
        // LabelSourceImport
        // 
        this.LabelSourceImport.AutoEllipsis = true;
        this.LabelSourceImport.AutoSize = true;
        this.LabelSourceImport.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelSourceImport.Location = new System.Drawing.Point(3, 5);
        this.LabelSourceImport.Margin = new System.Windows.Forms.Padding(3, 5, 3, 16);
        this.LabelSourceImport.MaximumSize = new System.Drawing.Size(300, 300);
        this.LabelSourceImport.Name = "LabelSourceImport";
        this.LabelSourceImport.Size = new System.Drawing.Size(298, 75);
        this.LabelSourceImport.TabIndex = 13;
        this.LabelSourceImport.Text = "The source image \'{0}\' has multiple array indices/depth slices and/or multiple mi" +
"p map levels.\r\n\r\nPlease select an index/slice to import from the source image.";
        // 
        // TableSourceArrayDepth
        // 
        this.TableSourceArrayDepth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.TableSourceArrayDepth.AutoSize = true;
        this.TableSourceArrayDepth.ColumnCount = 4;
        this.TableSourceArrayDepth.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableSourceArrayDepth.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableSourceArrayDepth.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableSourceArrayDepth.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableSourceArrayDepth.Controls.Add(this.LabelSrcArray, 2, 0);
        this.TableSourceArrayDepth.Controls.Add(this.ButtonPrevSrcArray, 1, 0);
        this.TableSourceArrayDepth.Controls.Add(this.LabelSourceArrayDepth, 0, 0);
        this.TableSourceArrayDepth.Controls.Add(this.label3, 0, 1);
        this.TableSourceArrayDepth.Controls.Add(this.ButtonPrevSrcMip, 1, 1);
        this.TableSourceArrayDepth.Controls.Add(this.LabelSrcMips, 2, 1);
        this.TableSourceArrayDepth.Controls.Add(this.ButtonNextSrcArray, 3, 0);
        this.TableSourceArrayDepth.Controls.Add(this.ButtonNextSrcMip, 3, 1);
        this.TableSourceArrayDepth.Location = new System.Drawing.Point(3, 355);
        this.TableSourceArrayDepth.Name = "TableSourceArrayDepth";
        this.TableSourceArrayDepth.RowCount = 2;
        this.TableSourceArrayDepth.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableSourceArrayDepth.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableSourceArrayDepth.Size = new System.Drawing.Size(298, 64);
        this.TableSourceArrayDepth.TabIndex = 12;
        // 
        // LabelSrcArray
        // 
        this.LabelSrcArray.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelSrcArray.AutoSize = true;
        this.LabelSrcArray.Location = new System.Drawing.Point(152, 8);
        this.LabelSrcArray.MinimumSize = new System.Drawing.Size(55, 0);
        this.LabelSrcArray.Name = "LabelSrcArray";
        this.LabelSrcArray.Size = new System.Drawing.Size(72, 15);
        this.LabelSrcArray.TabIndex = 9;
        this.LabelSrcArray.Text = "99999/99999";
        this.LabelSrcArray.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // LabelSourceArrayDepth
        // 
        this.LabelSourceArrayDepth.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelSourceArrayDepth.AutoSize = true;
        this.LabelSourceArrayDepth.Location = new System.Drawing.Point(6, 8);
        this.LabelSourceArrayDepth.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
        this.LabelSourceArrayDepth.Name = "LabelSourceArrayDepth";
        this.LabelSourceArrayDepth.Size = new System.Drawing.Size(70, 15);
        this.LabelSourceArrayDepth.TabIndex = 12;
        this.LabelSourceArrayDepth.Text = "Array Index:";
        this.LabelSourceArrayDepth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label3
        // 
        this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(15, 40);
        this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(61, 15);
        this.label3.TabIndex = 11;
        this.label3.Text = "Mip Level:";
        this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // LabelSrcMips
        // 
        this.LabelSrcMips.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelSrcMips.AutoSize = true;
        this.LabelSrcMips.Location = new System.Drawing.Point(114, 40);
        this.LabelSrcMips.MinimumSize = new System.Drawing.Size(115, 0);
        this.LabelSrcMips.Name = "LabelSrcMips";
        this.LabelSrcMips.Size = new System.Drawing.Size(149, 15);
        this.LabelSrcMips.TabIndex = 7;
        this.LabelSrcMips.Text = "99/99";
        this.LabelSrcMips.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // tableLayoutPanel2
        // 
        this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.tableLayoutPanel2.AutoSize = true;
        this.tableLayoutPanel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.tableLayoutPanel2.ColumnCount = 2;
        this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel2.Controls.Add(this.ButtonSrcImport, 0, 0);
        this.tableLayoutPanel2.Controls.Add(this.ButtonSrcCancel, 1, 0);
        this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 428);
        this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        this.tableLayoutPanel2.Name = "tableLayoutPanel2";
        this.tableLayoutPanel2.RowCount = 1;
        this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 115F));
        this.tableLayoutPanel2.Size = new System.Drawing.Size(298, 92);
        this.tableLayoutPanel2.TabIndex = 10;
        // 
        // ButtonSrcImport
        // 
        this.ButtonSrcImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonSrcImport.AutoSize = true;
        this.ButtonSrcImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonSrcImport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonSrcImport.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonSrcImport.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonSrcImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonSrcImport.Location = new System.Drawing.Point(126, 3);
        this.ButtonSrcImport.Name = "ButtonSrcImport";
        this.ButtonSrcImport.Size = new System.Drawing.Size(80, 29);
        this.ButtonSrcImport.TabIndex = 0;
        this.ButtonSrcImport.Text = "Se&lect";
        this.ButtonSrcImport.UseVisualStyleBackColor = false;
        this.ButtonSrcImport.Click += new System.EventHandler(this.ButtonSrcImport_Click);
        // 
        // ButtonSrcCancel
        // 
        this.ButtonSrcCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonSrcCancel.AutoSize = true;
        this.ButtonSrcCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonSrcCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonSrcCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonSrcCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonSrcCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonSrcCancel.Location = new System.Drawing.Point(212, 3);
        this.ButtonSrcCancel.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        this.ButtonSrcCancel.Name = "ButtonSrcCancel";
        this.ButtonSrcCancel.Size = new System.Drawing.Size(80, 29);
        this.ButtonSrcCancel.TabIndex = 1;
        this.ButtonSrcCancel.Text = "&Cancel";
        this.ButtonSrcCancel.UseVisualStyleBackColor = false;
        this.ButtonSrcCancel.Click += new System.EventHandler(this.ButtonSrcCancel_Click);
        // 
        // PanelSourceImage
        // 
        this.PanelSourceImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PanelSourceImage.Location = new System.Drawing.Point(3, 99);
        this.PanelSourceImage.MinimumSize = new System.Drawing.Size(256, 256);
        this.PanelSourceImage.Name = "PanelSourceImage";
        this.PanelSourceImage.Size = new System.Drawing.Size(298, 256);
        this.PanelSourceImage.TabIndex = 11;
        // 
        // TipsFiles
        // 
        this.TipsFiles.AutoPopDelay = 5000;
        this.TipsFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.TipsFiles.ForeColor = System.Drawing.Color.White;
        this.TipsFiles.InitialDelay = 100;
        this.TipsFiles.ReshowDelay = 100;
        this.TipsFiles.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
        this.TipsFiles.ToolTipTitle = "Image Information";
        // 
        // FormImagePicker
        // 
        this.AcceptButton = this.ButtonOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.Controls.Add(this.TableControls);
        this.Controls.Add(this.TableResize);
        this.Controls.Add(this.TablePickSourceImage);
        this.Controls.Add(this.PanelDialogButtons);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.ForeColor = System.Drawing.Color.White;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.KeyPreview = true;
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "FormImagePicker";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Select images for import into {0}";
        this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormImagePicker_KeyDown);
        this.PanelDialogButtons.ResumeLayout(false);
        this.PanelDialogButtons.PerformLayout();
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.TableArrayDepthControls.ResumeLayout(false);
        this.TableArrayDepthControls.PerformLayout();
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        this.PanelFilter.ResumeLayout(false);
        this.PanelFilter.PerformLayout();
        this.TableResize.ResumeLayout(false);
        this.TableResize.PerformLayout();
        this.TablePickSourceImage.ResumeLayout(false);
        this.TablePickSourceImage.PerformLayout();
        this.TableSourceArrayDepth.ResumeLayout(false);
        this.TableSourceArrayDepth.PerformLayout();
        this.tableLayoutPanel2.ResumeLayout(false);
        this.tableLayoutPanel2.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel PanelDialogButtons;
    private System.Windows.Forms.Button ButtonOK;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.TableLayoutPanel TableControls;
    private System.Windows.Forms.Label LabelImportDesc;
    private System.Windows.Forms.Panel PanelArrayDepth;
    private System.Windows.Forms.ToolTip TipsButtons;
    private System.Windows.Forms.TableLayoutPanel TableArrayDepthControls;
    private System.Windows.Forms.ListView ListImages;
    private System.Windows.Forms.ImageList ImageFiles;
    private System.Windows.Forms.Button ButtonPrevMip;
    private System.Windows.Forms.Button ButtonNextMip;
    private System.Windows.Forms.Button ButtonImport;
    private System.Windows.Forms.Button ButtonRestore;
    private System.Windows.Forms.Label LabelMipMapLevel;
    private System.Windows.Forms.Button ButtonNextArrayDepth;
    private System.Windows.Forms.Label LabelArrayDepthIndex;
    private System.Windows.Forms.Button ButtonPrevArrayDepth;
    private System.Windows.Forms.Label LabelMipMaps;
    private System.Windows.Forms.Label LabelArrayDepth;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Button ButtonCropResizeOk;
    private System.Windows.Forms.Button ButtonCropResizeCancel;
    private System.Windows.Forms.Label LabelImageFilter;
    private System.Windows.Forms.Label LabelDesc;
    private System.Windows.Forms.Label LabelImportImageDimensions;
    private System.Windows.Forms.Label LabelTargetImageDimensions;
    private System.Windows.Forms.RadioButton RadioCrop;
    private Gorgon.UI.GorgonAlignmentPicker AlignmentPicker;
    private System.Windows.Forms.Label LabelAnchor;
    private System.Windows.Forms.RadioButton RadioResize;
    private System.Windows.Forms.Panel PanelFilter;
    private System.Windows.Forms.ComboBox ComboImageFilter;
    private System.Windows.Forms.CheckBox CheckPreserveAspect;
    private System.Windows.Forms.TableLayoutPanel TableResize;
    private System.Windows.Forms.TableLayoutPanel TablePickSourceImage;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Button ButtonSrcImport;
    private System.Windows.Forms.Button ButtonSrcCancel;
    private System.Windows.Forms.Panel PanelSourceImage;
    private System.Windows.Forms.TableLayoutPanel TableSourceArrayDepth;
    private System.Windows.Forms.Button ButtonPrevSrcMip;
    private System.Windows.Forms.Button ButtonNextSrcMip;
    private System.Windows.Forms.Label LabelSrcMips;
    private System.Windows.Forms.Label LabelSrcArray;
    private System.Windows.Forms.Button ButtonPrevSrcArray;
    private System.Windows.Forms.Button ButtonNextSrcArray;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label LabelSourceArrayDepth;
    private System.Windows.Forms.Label LabelSourceImport;
    private System.Windows.Forms.Label LabelResizeAnchor;
    private Gorgon.UI.GorgonAlignmentPicker AlignmentResize;
    private System.Windows.Forms.ToolTip TipsFiles;
}