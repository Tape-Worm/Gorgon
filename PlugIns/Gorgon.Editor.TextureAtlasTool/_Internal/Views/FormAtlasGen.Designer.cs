using Gorgon.UI;

namespace Gorgon.Editor.TextureAtlasTool
{
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
                _spriteSelector?.Dispose();
                UnassignEvents();
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
			this.ButtonLoadSprites = new System.Windows.Forms.Button();
			this.LabelSpriteCount = new System.Windows.Forms.Label();
			this.TextBaseTextureName = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.TextOutputFolder = new System.Windows.Forms.TextBox();
			this.ButtonCalculateSize = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.ButtonGenerate = new System.Windows.Forms.Button();
			this.NumericPadding = new System.Windows.Forms.NumericUpDown();
			this.ButtonOk = new System.Windows.Forms.Button();
			this.PanelRender = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label11 = new System.Windows.Forms.Label();
			this.LabelTextureHeader = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.TableExtractControls = new System.Windows.Forms.TableLayoutPanel();
			this.TableDialogControls = new System.Windows.Forms.TableLayoutPanel();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericArrayIndex)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumericPadding)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.TableExtractControls.SuspendLayout();
			this.TableDialogControls.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
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
			this.ButtonNextArray.Image = global::Gorgon.Editor.TextureAtlasTool.Properties.Resources.right_16x16;
			this.ButtonNextArray.Location = new System.Drawing.Point(519, 5);
			this.ButtonNextArray.Name = "ButtonNextArray";
			this.ButtonNextArray.Size = new System.Drawing.Size(22, 22);
			this.ButtonNextArray.TabIndex = 1;
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
			this.ButtonPrevArray.Image = global::Gorgon.Editor.TextureAtlasTool.Properties.Resources.left_16x16;
			this.ButtonPrevArray.Location = new System.Drawing.Point(3, 5);
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
			this.LabelArray.Location = new System.Drawing.Point(80, 9);
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
			this.LabelArrayRange.Location = new System.Drawing.Point(52, 147);
			this.LabelArrayRange.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
			this.LabelArrayRange.Name = "LabelArrayRange";
			this.LabelArrayRange.Size = new System.Drawing.Size(27, 15);
			this.LabelArrayRange.TabIndex = 10;
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
			this.label6.TabIndex = 11;
			this.label6.Text = "Output Folder";
			this.TipInstructions.SetToolTip(this.label6, "Defines where to place the texture atlas and sprites when generating.");
			// 
			// ButtonFolderBrowse
			// 
			this.ButtonFolderBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.ButtonFolderBrowse.AutoSize = true;
			this.ButtonFolderBrowse.FlatAppearance.BorderSize = 0;
			this.ButtonFolderBrowse.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonFolderBrowse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonFolderBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonFolderBrowse.Image = global::Gorgon.Editor.TextureAtlasTool.Properties.Resources.folder_20x20;
			this.ButtonFolderBrowse.Location = new System.Drawing.Point(248, 3);
			this.ButtonFolderBrowse.Name = "ButtonFolderBrowse";
			this.ButtonFolderBrowse.Size = new System.Drawing.Size(26, 26);
			this.ButtonFolderBrowse.TabIndex = 1;
			this.TipInstructions.SetToolTip(this.ButtonFolderBrowse, "Selects a folder to copy the texture atlas and sprites into.");
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
			this.NumericTextureHeight.Location = new System.Drawing.Point(189, 143);
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
			this.NumericTextureHeight.TabIndex = 2;
			this.NumericTextureHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TipInstructions.SetToolTip(this.NumericTextureHeight, "Specifies the maximum texture size for the textures.");
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
			this.NumericTextureWidth.Location = new System.Drawing.Point(85, 143);
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
			this.NumericTextureWidth.TabIndex = 1;
			this.NumericTextureWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TipInstructions.SetToolTip(this.NumericTextureWidth, "Specifies the maximum texture size for the textures.");
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
			this.LabelArrayCount.Location = new System.Drawing.Point(8, 179);
			this.LabelArrayCount.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
			this.LabelArrayCount.Name = "LabelArrayCount";
			this.LabelArrayCount.Size = new System.Drawing.Size(71, 15);
			this.LabelArrayCount.TabIndex = 11;
			this.LabelArrayCount.Text = "Array Count";
			this.TipInstructions.SetToolTip(this.LabelArrayCount, "Specifies the maximum number of array indices to generate in the textures.");
			// 
			// NumericArrayIndex
			// 
			this.NumericArrayIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.NumericArrayIndex.Location = new System.Drawing.Point(85, 175);
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
			this.NumericArrayIndex.TabIndex = 3;
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
			this.label3.Location = new System.Drawing.Point(171, 147);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(12, 15);
			this.label3.TabIndex = 10;
			this.label3.Text = "x";
			this.TipInstructions.SetToolTip(this.label3, "Specifies the maximum texture size for the textures.");
			// 
			// ButtonLoadSprites
			// 
			this.ButtonLoadSprites.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ButtonLoadSprites.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.ButtonLoadSprites, 4);
			this.ButtonLoadSprites.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonLoadSprites.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonLoadSprites.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonLoadSprites.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonLoadSprites.Location = new System.Drawing.Point(68, 50);
			this.ButtonLoadSprites.Margin = new System.Windows.Forms.Padding(16, 3, 16, 3);
			this.ButtonLoadSprites.Name = "ButtonLoadSprites";
			this.ButtonLoadSprites.Size = new System.Drawing.Size(141, 27);
			this.ButtonLoadSprites.TabIndex = 0;
			this.ButtonLoadSprites.Text = "&Load Sprites";
			this.TipInstructions.SetToolTip(this.ButtonLoadSprites, "Loads the sprites that will be used to generate the atlas.");
			this.ButtonLoadSprites.UseVisualStyleBackColor = true;
			this.ButtonLoadSprites.Click += new System.EventHandler(this.ButtonLoadSprites_Click);
			// 
			// LabelSpriteCount
			// 
			this.LabelSpriteCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.LabelSpriteCount.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.LabelSpriteCount, 4);
			this.LabelSpriteCount.Location = new System.Drawing.Point(16, 29);
			this.LabelSpriteCount.Margin = new System.Windows.Forms.Padding(16, 3, 3, 3);
			this.LabelSpriteCount.Name = "LabelSpriteCount";
			this.LabelSpriteCount.Size = new System.Drawing.Size(101, 15);
			this.LabelSpriteCount.TabIndex = 0;
			this.LabelSpriteCount.Text = "{0} Sprites Loaded";
			this.TipInstructions.SetToolTip(this.LabelSpriteCount, "The number of sprites that will be placed on the texture atlas.");
			// 
			// TextBaseTextureName
			// 
			this.TextBaseTextureName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBaseTextureName.BackColor = System.Drawing.Color.White;
			this.TextBaseTextureName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextBaseTextureName.ForeColor = System.Drawing.Color.Black;
			this.TextBaseTextureName.Location = new System.Drawing.Point(95, 35);
			this.TextBaseTextureName.Name = "TextBaseTextureName";
			this.TextBaseTextureName.Size = new System.Drawing.Size(147, 23);
			this.TextBaseTextureName.TabIndex = 2;
			this.TextBaseTextureName.Text = "Base Name";
			this.TipInstructions.SetToolTip(this.TextBaseTextureName, "Defines the base name of the texture atlas texture(s).");
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
			this.label7.TabIndex = 12;
			this.label7.Text = "Base Name";
			this.TipInstructions.SetToolTip(this.label7, "Defines the base name of the texture atlas texture(s).\r\n\r\nThe atlas names will be" +
        " formatted as \"basename_n\", where \"n\" is the \r\ntexture index number.");
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
			this.TextOutputFolder.Size = new System.Drawing.Size(147, 23);
			this.TextOutputFolder.TabIndex = 0;
			this.TextOutputFolder.Text = "Folder Name";
			this.TipInstructions.SetToolTip(this.TextOutputFolder, "Defines where to place the texture atlas and sprites when generating.");
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
			this.ButtonCalculateSize.Image = global::Gorgon.Editor.TextureAtlasTool.Properties.Resources.calculate_22x22;
			this.ButtonCalculateSize.Location = new System.Drawing.Point(214, 172);
			this.ButtonCalculateSize.Margin = new System.Windows.Forms.Padding(3, 3, 8, 3);
			this.ButtonCalculateSize.Name = "ButtonCalculateSize";
			this.ButtonCalculateSize.Size = new System.Drawing.Size(30, 30);
			this.ButtonCalculateSize.TabIndex = 4;
			this.TipInstructions.SetToolTip(this.ButtonCalculateSize, "Calculates the best size and array count for the loaded sprites.");
			this.ButtonCalculateSize.UseVisualStyleBackColor = true;
			this.ButtonCalculateSize.Click += new System.EventHandler(this.ButtonCalculateSize_Click);
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.label2.Location = new System.Drawing.Point(28, 87);
			this.label2.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(51, 15);
			this.label2.TabIndex = 11;
			this.label2.Text = "Padding";
			this.TipInstructions.SetToolTip(this.label2, "Specifies how much blank space to add around each sprite on the atlas.");
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Controls.Add(this.LabelArray, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.ButtonPrevArray, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.ButtonNextArray, 2, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 562);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(544, 33);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// ButtonGenerate
			// 
			this.ButtonGenerate.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.ButtonGenerate.AutoSize = true;
			this.ButtonGenerate.Enabled = false;
			this.ButtonGenerate.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonGenerate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonGenerate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonGenerate.Location = new System.Drawing.Point(621, 328);
			this.ButtonGenerate.Name = "ButtonGenerate";
			this.ButtonGenerate.Size = new System.Drawing.Size(141, 27);
			this.ButtonGenerate.TabIndex = 3;
			this.ButtonGenerate.Text = "&Generate Atlas";
			this.TipInstructions.SetToolTip(this.ButtonGenerate, resources.GetString("ButtonGenerate.ToolTip"));
			this.ButtonGenerate.UseVisualStyleBackColor = true;
			this.ButtonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
			// 
			// NumericPadding
			// 
			this.NumericPadding.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.NumericPadding.Location = new System.Drawing.Point(85, 83);
			this.NumericPadding.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.NumericPadding.Name = "NumericPadding";
			this.NumericPadding.Size = new System.Drawing.Size(80, 23);
			this.NumericPadding.TabIndex = 1;
			this.NumericPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TipInstructions.SetToolTip(this.NumericPadding, "The amount of padding (in pixels) around each sprite on the atlas. \r\n\r\nThis is us" +
        "ed to help prevent texture bleed when using filtering.");
			this.NumericPadding.ValueChanged += new System.EventHandler(this.NumericPadding_ValueChanged);
			// 
			// ButtonOk
			// 
			this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOk.AutoSize = true;
			this.ButtonOk.Enabled = false;
			this.ButtonOk.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonOk.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonOk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonOk.Location = new System.Drawing.Point(86, 3);
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.Size = new System.Drawing.Size(91, 27);
			this.ButtonOk.TabIndex = 0;
			this.ButtonOk.Text = "&Save";
			this.TipInstructions.SetToolTip(this.ButtonOk, "Writes the texture atlas and remapped sprites back to the file system.\r\n\r\nNote th" +
        "at any sprites loaded will be replaced with the remapped sprites.");
			this.ButtonOk.UseVisualStyleBackColor = true;
			this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
			// 
			// PanelRender
			// 
			this.PanelRender.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelRender.Location = new System.Drawing.Point(3, 3);
			this.PanelRender.Name = "PanelRender";
			this.TableExtractControls.SetRowSpan(this.PanelRender, 6);
			this.PanelRender.Size = new System.Drawing.Size(544, 553);
			this.PanelRender.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.NumericTextureHeight, 3, 5);
			this.tableLayoutPanel1.Controls.Add(this.NumericTextureWidth, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.label11, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.LabelTextureHeader, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.LabelArrayRange, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.LabelArrayCount, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.NumericArrayIndex, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.label3, 2, 5);
			this.tableLayoutPanel1.Controls.Add(this.ButtonLoadSprites, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.LabelSpriteCount, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.ButtonCalculateSize, 3, 6);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.NumericPadding, 1, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(553, 3);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(277, 205);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// label11
			// 
			this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
			this.label11.Location = new System.Drawing.Point(3, 3);
			this.label11.Margin = new System.Windows.Forms.Padding(3);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(55, 20);
			this.label11.TabIndex = 12;
			this.label11.Text = "Sprites";
			// 
			// LabelTextureHeader
			// 
			this.LabelTextureHeader.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.LabelTextureHeader, 2);
			this.LabelTextureHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
			this.LabelTextureHeader.Location = new System.Drawing.Point(3, 117);
			this.LabelTextureHeader.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.LabelTextureHeader.Name = "LabelTextureHeader";
			this.LabelTextureHeader.Size = new System.Drawing.Size(60, 20);
			this.LabelTextureHeader.TabIndex = 9;
			this.LabelTextureHeader.Text = "Texture";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
			this.label1.Location = new System.Drawing.Point(553, 219);
			this.label1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 20);
			this.label1.TabIndex = 10;
			this.label1.Text = "Output";
			// 
			// TableExtractControls
			// 
			this.TableExtractControls.AutoSize = true;
			this.TableExtractControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TableExtractControls.ColumnCount = 2;
			this.TableExtractControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableExtractControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableExtractControls.Controls.Add(this.TableDialogControls, 1, 6);
			this.TableExtractControls.Controls.Add(this.tableLayoutPanel1, 1, 0);
			this.TableExtractControls.Controls.Add(this.PanelRender, 0, 0);
			this.TableExtractControls.Controls.Add(this.tableLayoutPanel2, 0, 6);
			this.TableExtractControls.Controls.Add(this.label1, 1, 1);
			this.TableExtractControls.Controls.Add(this.tableLayoutPanel3, 1, 2);
			this.TableExtractControls.Controls.Add(this.ButtonGenerate, 1, 4);
			this.TableExtractControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableExtractControls.Location = new System.Drawing.Point(0, 0);
			this.TableExtractControls.Name = "TableExtractControls";
			this.TableExtractControls.RowCount = 7;
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TableExtractControls.Size = new System.Drawing.Size(833, 598);
			this.TableExtractControls.TabIndex = 0;
			// 
			// TableDialogControls
			// 
			this.TableDialogControls.AutoSize = true;
			this.TableDialogControls.ColumnCount = 2;
			this.TableDialogControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableDialogControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableDialogControls.Controls.Add(this.ButtonCancel, 0, 0);
			this.TableDialogControls.Controls.Add(this.ButtonOk, 0, 0);
			this.TableDialogControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableDialogControls.Location = new System.Drawing.Point(553, 562);
			this.TableDialogControls.Name = "TableDialogControls";
			this.TableDialogControls.RowCount = 1;
			this.TableDialogControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableDialogControls.Size = new System.Drawing.Size(277, 33);
			this.TableDialogControls.TabIndex = 8;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.AutoSize = true;
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonCancel.Location = new System.Drawing.Point(183, 3);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(91, 27);
			this.ButtonCancel.TabIndex = 1;
			this.ButtonCancel.Text = "&Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.ColumnCount = 3;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.Controls.Add(this.ButtonFolderBrowse, 2, 0);
			this.tableLayoutPanel3.Controls.Add(this.TextBaseTextureName, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.label6, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.label7, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.TextOutputFolder, 1, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(553, 245);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.Size = new System.Drawing.Size(277, 61);
			this.tableLayoutPanel3.TabIndex = 2;
			// 
			// FormAtlasGen
			// 
			this.AcceptButton = this.ButtonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(833, 598);
			this.Controls.Add(this.TableExtractControls);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "FormAtlasGen";
			this.RenderControl = this.PanelRender;
			this.Text = "Generate Texture Atlas";
			((System.ComponentModel.ISupportInitialize)(this.NumericTextureHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericTextureWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericArrayIndex)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumericPadding)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.TableExtractControls.ResumeLayout(false);
			this.TableExtractControls.PerformLayout();
			this.TableDialogControls.ResumeLayout(false);
			this.TableDialogControls.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip TipInstructions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label LabelArray;
        private System.Windows.Forms.Button ButtonPrevArray;
        private System.Windows.Forms.Button ButtonNextArray;
        private System.Windows.Forms.Button ButtonGenerate;
        private System.Windows.Forms.Panel PanelRender;
        private System.Windows.Forms.TableLayoutPanel TableExtractControls;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label LabelSpriteCount;
        private System.Windows.Forms.Button ButtonLoadSprites;
        private System.Windows.Forms.NumericUpDown NumericTextureHeight;
        private System.Windows.Forms.NumericUpDown NumericTextureWidth;
        private System.Windows.Forms.Label LabelTextureHeader;
        private System.Windows.Forms.Label LabelArrayRange;
        private System.Windows.Forms.Label LabelArrayCount;
        private System.Windows.Forms.NumericUpDown NumericArrayIndex;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
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
    }
}