using Gorgon.UI;

namespace Gorgon.Editor.ImageAtlasTool;

partial class FormAtlasGen
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
            _imageSelector?.Dispose();
        }

        base.Dispose(disposing);
    }



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAtlasGen));
        this.TipInstructions = new System.Windows.Forms.ToolTip(this.components);
        this.ButtonNextArray = new System.Windows.Forms.Button();
        this.ButtonPrevArray = new System.Windows.Forms.Button();
        this.LabelArray = new System.Windows.Forms.Label();
        this.LabelArrayRange = new System.Windows.Forms.Label();
        this.label6 = new System.Windows.Forms.Label();
        this.ButtonFolderBrowse = new System.Windows.Forms.Button();
        this.NumericTextureHeight = new System.Windows.Forms.NumericUpDown();
        this.NumericTextureWidth = new System.Windows.Forms.NumericUpDown();
        this.LabelArrayCount = new System.Windows.Forms.Label();
        this.NumericArrayIndex = new System.Windows.Forms.NumericUpDown();
        this.label3 = new System.Windows.Forms.Label();
        this.TextBaseTextureName = new System.Windows.Forms.TextBox();
        this.label7 = new System.Windows.Forms.Label();
        this.TextOutputFolder = new System.Windows.Forms.TextBox();
        this.ButtonCalculateSize = new System.Windows.Forms.Button();
        this.label2 = new System.Windows.Forms.Label();
        this.ButtonGenerate = new System.Windows.Forms.Button();
        this.NumericPadding = new System.Windows.Forms.NumericUpDown();
        this.ButtonOk = new System.Windows.Forms.Button();
        this.ButtonLoadImages = new System.Windows.Forms.Button();
        this.LabelSpriteCount = new System.Windows.Forms.Label();
        this.TablePreviewControls = new System.Windows.Forms.TableLayoutPanel();
        this.PanelRender = new Gorgon.UI.GorgonSelectablePanel();
        this.TableAtlasParameters = new System.Windows.Forms.TableLayoutPanel();
        this.panel3 = new System.Windows.Forms.Panel();
        this.label11 = new System.Windows.Forms.Label();
        this.panel2 = new System.Windows.Forms.Panel();
        this.LabelTextureHeader = new System.Windows.Forms.Label();
        this.label1 = new System.Windows.Forms.Label();
        this.TableExtractControls = new System.Windows.Forms.TableLayoutPanel();
        this.panel4 = new System.Windows.Forms.Panel();
        this.TableOutputControls = new System.Windows.Forms.TableLayoutPanel();
        this.CheckCreateSprites = new System.Windows.Forms.CheckBox();
        this.TableDialogControls = new System.Windows.Forms.TableLayoutPanel();
        this.ButtonCancel = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericArrayIndex)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericPadding)).BeginInit();
        this.TablePreviewControls.SuspendLayout();
        this.TableAtlasParameters.SuspendLayout();
        this.panel3.SuspendLayout();
        this.panel2.SuspendLayout();
        this.TableExtractControls.SuspendLayout();
        this.panel4.SuspendLayout();
        this.TableOutputControls.SuspendLayout();
        this.TableDialogControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // TipInstructions
        // 
        this.TipInstructions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.TipInstructions.ForeColor = System.Drawing.Color.White;
        // 
        // ButtonNextArray
        // 
        this.ButtonNextArray.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonNextArray.AutoSize = true;
        this.ButtonNextArray.Enabled = false;
        this.ButtonNextArray.FlatAppearance.BorderSize = 0;
        this.ButtonNextArray.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonNextArray.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextArray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextArray.Image = global::Gorgon.Editor.ImageAtlasTool.Properties.Resources.right_16x16;
        this.ButtonNextArray.Location = new System.Drawing.Point(530, 3);
        this.ButtonNextArray.Name = "ButtonNextArray";
        this.ButtonNextArray.Size = new System.Drawing.Size(22, 22);
        this.ButtonNextArray.TabIndex = 2;
        this.TipInstructions.SetToolTip(this.ButtonNextArray, "Move to the next texture index or array index.");
        this.ButtonNextArray.UseVisualStyleBackColor = true;
        this.ButtonNextArray.Visible = false;
        this.ButtonNextArray.Click += new System.EventHandler(this.ButtonNextArray_Click);
        // 
        // ButtonPrevArray
        // 
        this.ButtonPrevArray.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonPrevArray.AutoSize = true;
        this.ButtonPrevArray.Enabled = false;
        this.ButtonPrevArray.FlatAppearance.BorderSize = 0;
        this.ButtonPrevArray.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonPrevArray.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevArray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevArray.Image = global::Gorgon.Editor.ImageAtlasTool.Properties.Resources.left_16x16;
        this.ButtonPrevArray.Location = new System.Drawing.Point(3, 3);
        this.ButtonPrevArray.Name = "ButtonPrevArray";
        this.ButtonPrevArray.Size = new System.Drawing.Size(23, 22);
        this.ButtonPrevArray.TabIndex = 0;
        this.TipInstructions.SetToolTip(this.ButtonPrevArray, "Move to the previous texture index or array index.");
        this.ButtonPrevArray.UseVisualStyleBackColor = true;
        this.ButtonPrevArray.Visible = false;
        this.ButtonPrevArray.Click += new System.EventHandler(this.ButtonPrevArray_Click);
        // 
        // LabelArray
        // 
        this.LabelArray.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelArray.AutoSize = true;
        this.LabelArray.Location = new System.Drawing.Point(86, 6);
        this.LabelArray.Margin = new System.Windows.Forms.Padding(16, 3, 16, 3);
        this.LabelArray.Name = "LabelArray";
        this.LabelArray.Size = new System.Drawing.Size(384, 15);
        this.LabelArray.TabIndex = 1;
        this.LabelArray.Text = "No atlas generated.  Click the \"Generate Atlas\" button to create an atlas.";
        this.TipInstructions.SetToolTip(this.LabelArray, resources.GetString("LabelArray.ToolTip"));
        // 
        // LabelArrayRange
        // 
        this.LabelArrayRange.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelArrayRange.AutoSize = true;
        this.LabelArrayRange.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.LabelArrayRange.Location = new System.Drawing.Point(52, 117);
        this.LabelArrayRange.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.LabelArrayRange.Name = "LabelArrayRange";
        this.LabelArrayRange.Size = new System.Drawing.Size(27, 15);
        this.LabelArrayRange.TabIndex = 6;
        this.LabelArrayRange.Text = "Size";
        this.TipInstructions.SetToolTip(this.LabelArrayRange, "Specifies the maximum texture size for the textures.");
        // 
        // label6
        // 
        this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label6.AutoSize = true;
        this.label6.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.label6.Location = new System.Drawing.Point(8, 8);
        this.label6.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label6.Name = "label6";
        this.label6.Size = new System.Drawing.Size(81, 15);
        this.label6.TabIndex = 0;
        this.label6.Text = "Output Folder";
        this.TipInstructions.SetToolTip(this.label6, "Defines where to place the image atlas and sprites when generating.");
        // 
        // ButtonFolderBrowse
        // 
        this.ButtonFolderBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonFolderBrowse.AutoSize = true;
        this.ButtonFolderBrowse.FlatAppearance.BorderSize = 0;
        this.ButtonFolderBrowse.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonFolderBrowse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonFolderBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonFolderBrowse.Image = global::Gorgon.Editor.ImageAtlasTool.Properties.Resources.folder_20x20;
        this.ButtonFolderBrowse.Location = new System.Drawing.Point(249, 3);
        this.ButtonFolderBrowse.Name = "ButtonFolderBrowse";
        this.ButtonFolderBrowse.Size = new System.Drawing.Size(26, 26);
        this.ButtonFolderBrowse.TabIndex = 2;
        this.TipInstructions.SetToolTip(this.ButtonFolderBrowse, "Selects a folder to copy the image atlas and, optionally, any new sprites into.");
        this.ButtonFolderBrowse.UseVisualStyleBackColor = true;
        this.ButtonFolderBrowse.Click += new System.EventHandler(this.ButtonFolderBrowse_Click);
        // 
        // NumericTextureHeight
        // 
        this.NumericTextureHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.NumericTextureHeight.Increment = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureHeight.Location = new System.Drawing.Point(190, 113);
        this.NumericTextureHeight.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTextureHeight.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureHeight.Name = "NumericTextureHeight";
        this.NumericTextureHeight.Size = new System.Drawing.Size(80, 23);
        this.NumericTextureHeight.TabIndex = 9;
        this.NumericTextureHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.TipInstructions.SetToolTip(this.NumericTextureHeight, "Specifies the maximum image height for the image atlas.");
        this.NumericTextureHeight.Value = new decimal(new int[] {
        1024,
        0,
        0,
        0});
        this.NumericTextureHeight.ValueChanged += new System.EventHandler(this.NumericTextureHeight_ValueChanged);
        // 
        // NumericTextureWidth
        // 
        this.NumericTextureWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.NumericTextureWidth.Increment = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureWidth.Location = new System.Drawing.Point(85, 113);
        this.NumericTextureWidth.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericTextureWidth.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericTextureWidth.Name = "NumericTextureWidth";
        this.NumericTextureWidth.Size = new System.Drawing.Size(80, 23);
        this.NumericTextureWidth.TabIndex = 7;
        this.NumericTextureWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.TipInstructions.SetToolTip(this.NumericTextureWidth, "Specifies the maximum image width for the image atlas.");
        this.NumericTextureWidth.Value = new decimal(new int[] {
        1024,
        0,
        0,
        0});
        this.NumericTextureWidth.ValueChanged += new System.EventHandler(this.NumericTextureWidth_ValueChanged);
        // 
        // LabelArrayCount
        // 
        this.LabelArrayCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelArrayCount.AutoSize = true;
        this.LabelArrayCount.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.LabelArrayCount.Location = new System.Drawing.Point(8, 149);
        this.LabelArrayCount.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.LabelArrayCount.Name = "LabelArrayCount";
        this.LabelArrayCount.Size = new System.Drawing.Size(71, 15);
        this.LabelArrayCount.TabIndex = 10;
        this.LabelArrayCount.Text = "Array Count";
        this.TipInstructions.SetToolTip(this.LabelArrayCount, "Specifies the maximum number of array indices to generate in the image atlas.");
        // 
        // NumericArrayIndex
        // 
        this.NumericArrayIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.NumericArrayIndex.Location = new System.Drawing.Point(85, 145);
        this.NumericArrayIndex.Maximum = new decimal(new int[] {
        4096,
        0,
        0,
        0});
        this.NumericArrayIndex.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericArrayIndex.Name = "NumericArrayIndex";
        this.NumericArrayIndex.Size = new System.Drawing.Size(80, 23);
        this.NumericArrayIndex.TabIndex = 11;
        this.NumericArrayIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.TipInstructions.SetToolTip(this.NumericArrayIndex, "Specifies the maximum number of array indices to generate in the textures.");
        this.NumericArrayIndex.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericArrayIndex.ValueChanged += new System.EventHandler(this.NumericArrayIndex_ValueChanged);
        // 
        // label3
        // 
        this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.label3.AutoSize = true;
        this.label3.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.label3.Location = new System.Drawing.Point(171, 117);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(13, 15);
        this.label3.TabIndex = 8;
        this.label3.Text = "x";
        this.TipInstructions.SetToolTip(this.label3, "Specifies the maximum texture size for the textures.");
        // 
        // TextBaseTextureName
        // 
        this.TextBaseTextureName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.TextBaseTextureName.BackColor = System.Drawing.Color.White;
        this.TextBaseTextureName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.TextBaseTextureName.ForeColor = System.Drawing.Color.Black;
        this.TextBaseTextureName.Location = new System.Drawing.Point(95, 35);
        this.TextBaseTextureName.Name = "TextBaseTextureName";
        this.TextBaseTextureName.Size = new System.Drawing.Size(148, 23);
        this.TextBaseTextureName.TabIndex = 4;
        this.TextBaseTextureName.Text = "Base Name";
        this.TipInstructions.SetToolTip(this.TextBaseTextureName, "Defines the base name of the image atlas image(s).");
        this.TextBaseTextureName.TextChanged += new System.EventHandler(this.TextBaseTextureName_TextChanged);
        // 
        // label7
        // 
        this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label7.AutoSize = true;
        this.label7.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.label7.Location = new System.Drawing.Point(23, 39);
        this.label7.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label7.Name = "label7";
        this.label7.Size = new System.Drawing.Size(66, 15);
        this.label7.TabIndex = 3;
        this.label7.Text = "Base Name";
        this.TipInstructions.SetToolTip(this.label7, "Defines the base name of the image atlas image(s).\r\n\r\nThe atlas names will be for" +
    "matted as \"basename_n\", where \"n\" is the \r\nimage index number.");
        // 
        // TextOutputFolder
        // 
        this.TextOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.TextOutputFolder.BackColor = System.Drawing.Color.White;
        this.TextOutputFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.TextOutputFolder.ForeColor = System.Drawing.Color.Black;
        this.TextOutputFolder.Location = new System.Drawing.Point(95, 4);
        this.TextOutputFolder.Name = "TextOutputFolder";
        this.TextOutputFolder.ReadOnly = true;
        this.TextOutputFolder.Size = new System.Drawing.Size(148, 23);
        this.TextOutputFolder.TabIndex = 1;
        this.TextOutputFolder.Text = "Folder Name";
        this.TipInstructions.SetToolTip(this.TextOutputFolder, "Defines where to place the image atlas and sprites when generating.");
        // 
        // ButtonCalculateSize
        // 
        this.ButtonCalculateSize.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.ButtonCalculateSize.AutoSize = true;
        this.ButtonCalculateSize.Enabled = false;
        this.ButtonCalculateSize.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.ButtonCalculateSize.FlatAppearance.BorderSize = 0;
        this.ButtonCalculateSize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonCalculateSize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCalculateSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCalculateSize.Image = global::Gorgon.Editor.ImageAtlasTool.Properties.Resources.calculate_22x22;
        this.ButtonCalculateSize.Location = new System.Drawing.Point(215, 142);
        this.ButtonCalculateSize.Margin = new System.Windows.Forms.Padding(3, 3, 8, 3);
        this.ButtonCalculateSize.Name = "ButtonCalculateSize";
        this.ButtonCalculateSize.Size = new System.Drawing.Size(30, 30);
        this.ButtonCalculateSize.TabIndex = 12;
        this.TipInstructions.SetToolTip(this.ButtonCalculateSize, "Calculates the best size and array count for the loaded images.");
        this.ButtonCalculateSize.UseVisualStyleBackColor = true;
        this.ButtonCalculateSize.Click += new System.EventHandler(this.ButtonCalculateSize_Click);
        // 
        // label2
        // 
        this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label2.AutoSize = true;
        this.label2.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.label2.Location = new System.Drawing.Point(28, 58);
        this.label2.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(51, 15);
        this.label2.TabIndex = 3;
        this.label2.Text = "Padding";
        this.TipInstructions.SetToolTip(this.label2, "Specifies how much blank space to add around each sprite on the atlas.");
        // 
        // ButtonGenerate
        // 
        this.ButtonGenerate.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.ButtonGenerate.AutoSize = true;
        this.ButtonGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonGenerate.Enabled = false;
        this.ButtonGenerate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonGenerate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonGenerate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonGenerate.Location = new System.Drawing.Point(646, 287);
        this.ButtonGenerate.Name = "ButtonGenerate";
        this.ButtonGenerate.Size = new System.Drawing.Size(95, 27);
        this.ButtonGenerate.TabIndex = 5;
        this.ButtonGenerate.Text = "&Generate Atlas";
        this.TipInstructions.SetToolTip(this.ButtonGenerate, resources.GetString("ButtonGenerate.ToolTip"));
        this.ButtonGenerate.UseVisualStyleBackColor = false;
        this.ButtonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
        // 
        // NumericPadding
        // 
        this.NumericPadding.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.NumericPadding.Location = new System.Drawing.Point(85, 54);
        this.NumericPadding.Maximum = new decimal(new int[] {
        8,
        0,
        0,
        0});
        this.NumericPadding.Name = "NumericPadding";
        this.NumericPadding.Size = new System.Drawing.Size(80, 23);
        this.NumericPadding.TabIndex = 4;
        this.NumericPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.TipInstructions.SetToolTip(this.NumericPadding, "The amount of padding (in pixels) around each image on the atlas. \r\n\r\nThis is use" +
    "d to help prevent texture bleed when using filtering.");
        this.NumericPadding.ValueChanged += new System.EventHandler(this.NumericPadding_ValueChanged);
        // 
        // ButtonOk
        // 
        this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonOk.AutoSize = true;
        this.ButtonOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOk.Enabled = false;
        this.ButtonOk.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOk.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonOk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOk.Location = new System.Drawing.Point(642, 3);
        this.ButtonOk.Name = "ButtonOk";
        this.ButtonOk.Size = new System.Drawing.Size(91, 27);
        this.ButtonOk.TabIndex = 0;
        this.ButtonOk.Text = "&Save";
        this.TipInstructions.SetToolTip(this.ButtonOk, "Writes the texture atlas and optionally, any new sprites, back to the file system" +
    ".");
        this.ButtonOk.UseVisualStyleBackColor = false;
        this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
        // 
        // ButtonLoadImages
        // 
        this.ButtonLoadImages.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.ButtonLoadImages.AutoSize = true;
        this.ButtonLoadImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonLoadImages.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonLoadImages.FlatAppearance.BorderSize = 0;
        this.ButtonLoadImages.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonLoadImages.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonLoadImages.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonLoadImages.Image = global::Gorgon.Editor.ImageAtlasTool.Properties.Resources.load_image_48x48;
        this.ButtonLoadImages.Location = new System.Drawing.Point(203, 28);
        this.ButtonLoadImages.Margin = new System.Windows.Forms.Padding(3, 3, 8, 3);
        this.ButtonLoadImages.Name = "ButtonLoadImages";
        this.TableAtlasParameters.SetRowSpan(this.ButtonLoadImages, 2);
        this.ButtonLoadImages.Size = new System.Drawing.Size(54, 54);
        this.ButtonLoadImages.TabIndex = 2;
        this.TipInstructions.SetToolTip(this.ButtonLoadImages, "Loads the images that will be used to generate the atlas.");
        this.ButtonLoadImages.UseVisualStyleBackColor = false;
        this.ButtonLoadImages.Click += new System.EventHandler(this.ButtonLoadImages_Click);
        // 
        // LabelSpriteCount
        // 
        this.LabelSpriteCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelSpriteCount.AutoSize = true;
        this.TableAtlasParameters.SetColumnSpan(this.LabelSpriteCount, 2);
        this.LabelSpriteCount.Location = new System.Drawing.Point(61, 28);
        this.LabelSpriteCount.Margin = new System.Windows.Forms.Padding(16, 3, 3, 3);
        this.LabelSpriteCount.Name = "LabelSpriteCount";
        this.LabelSpriteCount.Size = new System.Drawing.Size(104, 15);
        this.LabelSpriteCount.TabIndex = 2;
        this.LabelSpriteCount.Text = "{0} Images Loaded";
        this.TipInstructions.SetToolTip(this.LabelSpriteCount, "The number of images that will be placed on the texture atlas.");
        // 
        // TablePreviewControls
        // 
        this.TablePreviewControls.AutoSize = true;
        this.TablePreviewControls.ColumnCount = 3;
        this.TablePreviewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TablePreviewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TablePreviewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TablePreviewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TablePreviewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TablePreviewControls.Controls.Add(this.LabelArray, 1, 0);
        this.TablePreviewControls.Controls.Add(this.ButtonPrevArray, 0, 0);
        this.TablePreviewControls.Controls.Add(this.ButtonNextArray, 2, 0);
        this.TablePreviewControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TablePreviewControls.Location = new System.Drawing.Point(0, 537);
        this.TablePreviewControls.Margin = new System.Windows.Forms.Padding(0);
        this.TablePreviewControls.Name = "TablePreviewControls";
        this.TablePreviewControls.RowCount = 1;
        this.TablePreviewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TablePreviewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this.TablePreviewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this.TablePreviewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this.TablePreviewControls.Size = new System.Drawing.Size(555, 28);
        this.TablePreviewControls.TabIndex = 1;
        // 
        // PanelRender
        // 
        this.PanelRender.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelRender.Location = new System.Drawing.Point(3, 3);
        this.PanelRender.Name = "PanelRender";
        this.TableExtractControls.SetRowSpan(this.PanelRender, 5);
        this.PanelRender.ShowFocus = false;
        this.PanelRender.Size = new System.Drawing.Size(549, 531);
        this.PanelRender.TabIndex = 0;
        // 
        // TableAtlasParameters
        // 
        this.TableAtlasParameters.AutoSize = true;
        this.TableAtlasParameters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableAtlasParameters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableAtlasParameters.ColumnCount = 4;
        this.TableAtlasParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableAtlasParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableAtlasParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableAtlasParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableAtlasParameters.Controls.Add(this.panel3, 0, 0);
        this.TableAtlasParameters.Controls.Add(this.LabelSpriteCount, 0, 1);
        this.TableAtlasParameters.Controls.Add(this.label2, 0, 2);
        this.TableAtlasParameters.Controls.Add(this.NumericPadding, 1, 2);
        this.TableAtlasParameters.Controls.Add(this.ButtonLoadImages, 3, 1);
        this.TableAtlasParameters.Controls.Add(this.panel2, 0, 3);
        this.TableAtlasParameters.Controls.Add(this.NumericTextureHeight, 3, 4);
        this.TableAtlasParameters.Controls.Add(this.NumericTextureWidth, 1, 4);
        this.TableAtlasParameters.Controls.Add(this.LabelArrayRange, 0, 4);
        this.TableAtlasParameters.Controls.Add(this.LabelArrayCount, 0, 5);
        this.TableAtlasParameters.Controls.Add(this.NumericArrayIndex, 1, 5);
        this.TableAtlasParameters.Controls.Add(this.label3, 2, 4);
        this.TableAtlasParameters.Controls.Add(this.ButtonCalculateSize, 3, 5);
        this.TableAtlasParameters.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableAtlasParameters.Location = new System.Drawing.Point(555, 0);
        this.TableAtlasParameters.Margin = new System.Windows.Forms.Padding(0);
        this.TableAtlasParameters.Name = "TableAtlasParameters";
        this.TableAtlasParameters.RowCount = 7;
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableAtlasParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableAtlasParameters.Size = new System.Drawing.Size(278, 175);
        this.TableAtlasParameters.TabIndex = 2;
        // 
        // panel3
        // 
        this.panel3.AutoSize = true;
        this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.TableAtlasParameters.SetColumnSpan(this.panel3, 4);
        this.panel3.Controls.Add(this.label11);
        this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel3.Location = new System.Drawing.Point(0, 0);
        this.panel3.Margin = new System.Windows.Forms.Padding(0);
        this.panel3.Name = "panel3";
        this.panel3.Size = new System.Drawing.Size(278, 25);
        this.panel3.TabIndex = 0;
        // 
        // label11
        // 
        this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.label11.AutoSize = true;
        this.label11.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
        this.label11.Location = new System.Drawing.Point(3, 2);
        this.label11.Margin = new System.Windows.Forms.Padding(3);
        this.label11.Name = "label11";
        this.label11.Size = new System.Drawing.Size(57, 20);
        this.label11.TabIndex = 0;
        this.label11.Text = "Images";
        // 
        // panel2
        // 
        this.panel2.AutoSize = true;
        this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.TableAtlasParameters.SetColumnSpan(this.panel2, 4);
        this.panel2.Controls.Add(this.LabelTextureHeader);
        this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel2.Location = new System.Drawing.Point(0, 85);
        this.panel2.Margin = new System.Windows.Forms.Padding(0);
        this.panel2.Name = "panel2";
        this.panel2.Size = new System.Drawing.Size(278, 25);
        this.panel2.TabIndex = 5;
        // 
        // LabelTextureHeader
        // 
        this.LabelTextureHeader.AutoSize = true;
        this.LabelTextureHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
        this.LabelTextureHeader.Location = new System.Drawing.Point(3, 2);
        this.LabelTextureHeader.Margin = new System.Windows.Forms.Padding(3);
        this.LabelTextureHeader.Name = "LabelTextureHeader";
        this.LabelTextureHeader.Size = new System.Drawing.Size(92, 20);
        this.LabelTextureHeader.TabIndex = 0;
        this.LabelTextureHeader.Text = "Image Atlas ";
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
        this.label1.Location = new System.Drawing.Point(6, 0);
        this.label1.Margin = new System.Windows.Forms.Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(57, 20);
        this.label1.TabIndex = 0;
        this.label1.Text = "Output";
        // 
        // TableExtractControls
        // 
        this.TableExtractControls.AutoSize = true;
        this.TableExtractControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableExtractControls.ColumnCount = 2;
        this.TableExtractControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableExtractControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableExtractControls.Controls.Add(this.PanelRender, 0, 0);
        this.TableExtractControls.Controls.Add(this.TablePreviewControls, 0, 5);
        this.TableExtractControls.Controls.Add(this.TableAtlasParameters, 1, 0);
        this.TableExtractControls.Controls.Add(this.panel4, 1, 1);
        this.TableExtractControls.Controls.Add(this.TableOutputControls, 1, 2);
        this.TableExtractControls.Controls.Add(this.ButtonGenerate, 1, 3);
        this.TableExtractControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableExtractControls.Location = new System.Drawing.Point(0, 0);
        this.TableExtractControls.Margin = new System.Windows.Forms.Padding(0);
        this.TableExtractControls.Name = "TableExtractControls";
        this.TableExtractControls.RowCount = 6;
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableExtractControls.Size = new System.Drawing.Size(833, 565);
        this.TableExtractControls.TabIndex = 0;
        // 
        // panel4
        // 
        this.panel4.AutoSize = true;
        this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.panel4.Controls.Add(this.label1);
        this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel4.Location = new System.Drawing.Point(555, 175);
        this.panel4.Margin = new System.Windows.Forms.Padding(0);
        this.panel4.Name = "panel4";
        this.panel4.Size = new System.Drawing.Size(278, 23);
        this.panel4.TabIndex = 3;
        // 
        // TableOutputControls
        // 
        this.TableOutputControls.AutoSize = true;
        this.TableOutputControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableOutputControls.ColumnCount = 3;
        this.TableOutputControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableOutputControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableOutputControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableOutputControls.Controls.Add(this.CheckCreateSprites, 0, 2);
        this.TableOutputControls.Controls.Add(this.ButtonFolderBrowse, 2, 0);
        this.TableOutputControls.Controls.Add(this.TextBaseTextureName, 1, 1);
        this.TableOutputControls.Controls.Add(this.label6, 0, 0);
        this.TableOutputControls.Controls.Add(this.label7, 0, 1);
        this.TableOutputControls.Controls.Add(this.TextOutputFolder, 1, 0);
        this.TableOutputControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableOutputControls.Location = new System.Drawing.Point(555, 198);
        this.TableOutputControls.Margin = new System.Windows.Forms.Padding(0);
        this.TableOutputControls.Name = "TableOutputControls";
        this.TableOutputControls.RowCount = 3;
        this.TableOutputControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOutputControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOutputControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOutputControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableOutputControls.Size = new System.Drawing.Size(278, 86);
        this.TableOutputControls.TabIndex = 4;
        // 
        // CheckCreateSprites
        // 
        this.CheckCreateSprites.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.CheckCreateSprites.AutoSize = true;
        this.CheckCreateSprites.Checked = true;
        this.CheckCreateSprites.CheckState = System.Windows.Forms.CheckState.Checked;
        this.TableOutputControls.SetColumnSpan(this.CheckCreateSprites, 2);
        this.CheckCreateSprites.Location = new System.Drawing.Point(3, 64);
        this.CheckCreateSprites.Name = "CheckCreateSprites";
        this.CheckCreateSprites.Size = new System.Drawing.Size(195, 19);
        this.CheckCreateSprites.TabIndex = 5;
        this.CheckCreateSprites.Text = "Generate New Sprites With Atlas";
        this.CheckCreateSprites.UseVisualStyleBackColor = true;
        this.CheckCreateSprites.Click += new System.EventHandler(this.CheckCreateSprites_Click);
        // 
        // TableDialogControls
        // 
        this.TableDialogControls.AutoSize = true;
        this.TableDialogControls.ColumnCount = 2;
        this.TableDialogControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableDialogControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableDialogControls.Controls.Add(this.ButtonCancel, 0, 0);
        this.TableDialogControls.Controls.Add(this.ButtonOk, 0, 0);
        this.TableDialogControls.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.TableDialogControls.Location = new System.Drawing.Point(0, 565);
        this.TableDialogControls.Margin = new System.Windows.Forms.Padding(0);
        this.TableDialogControls.Name = "TableDialogControls";
        this.TableDialogControls.RowCount = 1;
        this.TableDialogControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableDialogControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
        this.TableDialogControls.Size = new System.Drawing.Size(833, 33);
        this.TableDialogControls.TabIndex = 6;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(739, 3);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(91, 27);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
        // 
        // FormAtlasGen
        // 
        this.AcceptButton = this.ButtonOk;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.ClientSize = new System.Drawing.Size(833, 598);
        this.Controls.Add(this.TableExtractControls);
        this.Controls.Add(this.TableDialogControls);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "FormAtlasGen";
        this.RenderControl = this.PanelRender;
        this.Text = "Generate Image Atlas";
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericArrayIndex)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericPadding)).EndInit();
        this.TablePreviewControls.ResumeLayout(false);
        this.TablePreviewControls.PerformLayout();
        this.TableAtlasParameters.ResumeLayout(false);
        this.TableAtlasParameters.PerformLayout();
        this.panel3.ResumeLayout(false);
        this.panel3.PerformLayout();
        this.panel2.ResumeLayout(false);
        this.panel2.PerformLayout();
        this.TableExtractControls.ResumeLayout(false);
        this.TableExtractControls.PerformLayout();
        this.panel4.ResumeLayout(false);
        this.panel4.PerformLayout();
        this.TableOutputControls.ResumeLayout(false);
        this.TableOutputControls.PerformLayout();
        this.TableDialogControls.ResumeLayout(false);
        this.TableDialogControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private System.Windows.Forms.ToolTip TipInstructions;
    private System.Windows.Forms.TableLayoutPanel TablePreviewControls;
    private System.Windows.Forms.Label LabelArray;
    private System.Windows.Forms.Button ButtonPrevArray;
    private System.Windows.Forms.Button ButtonNextArray;
    private System.Windows.Forms.Button ButtonGenerate;
    private GorgonSelectablePanel PanelRender;
    private System.Windows.Forms.TableLayoutPanel TableExtractControls;
    private System.Windows.Forms.TableLayoutPanel TableAtlasParameters;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.NumericUpDown NumericTextureHeight;
    private System.Windows.Forms.NumericUpDown NumericTextureWidth;
    private System.Windows.Forms.Label LabelTextureHeader;
    private System.Windows.Forms.Label LabelArrayRange;
    private System.Windows.Forms.Label LabelArrayCount;
    private System.Windows.Forms.NumericUpDown NumericArrayIndex;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel TableOutputControls;
    private System.Windows.Forms.TextBox TextBaseTextureName;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox TextOutputFolder;
    private System.Windows.Forms.Button ButtonFolderBrowse;
    private System.Windows.Forms.Button ButtonCalculateSize;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown NumericPadding;
    private System.Windows.Forms.TableLayoutPanel TableDialogControls;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.Button ButtonOk;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel4;
    private System.Windows.Forms.Label LabelSpriteCount;
    private System.Windows.Forms.Button ButtonLoadImages;
    private System.Windows.Forms.CheckBox CheckCreateSprites;
}
