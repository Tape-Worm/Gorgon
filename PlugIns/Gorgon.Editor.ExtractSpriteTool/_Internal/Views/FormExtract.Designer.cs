using Gorgon.UI;

namespace Gorgon.Editor.ExtractSpriteTool;

partial class FormExtract
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
            PanelRender.MouseWheel -= PanelRender_MouseWheel;
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormExtract));
        this.TableExtractControls = new System.Windows.Forms.TableLayoutPanel();
        this.PanelSkip = new System.Windows.Forms.Panel();
        this.CheckSkipEmpty = new System.Windows.Forms.CheckBox();
        this.TableExtractParams = new System.Windows.Forms.TableLayoutPanel();
        this.NumericOffsetX = new System.Windows.Forms.NumericUpDown();
        this.label2 = new System.Windows.Forms.Label();
        this.label1 = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.NumericCellWidth = new System.Windows.Forms.NumericUpDown();
        this.NumericCellHeight = new System.Windows.Forms.NumericUpDown();
        this.label4 = new System.Windows.Forms.Label();
        this.label5 = new System.Windows.Forms.Label();
        this.label6 = new System.Windows.Forms.Label();
        this.NumericOffsetY = new System.Windows.Forms.NumericUpDown();
        this.label12 = new System.Windows.Forms.Label();
        this.label13 = new System.Windows.Forms.Label();
        this.label14 = new System.Windows.Forms.Label();
        this.NumericColumnCount = new System.Windows.Forms.NumericUpDown();
        this.NumericRowCount = new System.Windows.Forms.NumericUpDown();
        this.LabelArrayRange = new System.Windows.Forms.Label();
        this.LabelArrayStart = new System.Windows.Forms.Label();
        this.NumericArrayIndex = new System.Windows.Forms.NumericUpDown();
        this.LabelArrayCount = new System.Windows.Forms.Label();
        this.NumericArrayCount = new System.Windows.Forms.NumericUpDown();
        this.panel3 = new System.Windows.Forms.Panel();
        this.LabelTextureHeader = new System.Windows.Forms.Label();
        this.panel4 = new System.Windows.Forms.Panel();
        this.label11 = new System.Windows.Forms.Label();
        this.PanelRender = new Gorgon.UI.GorgonSelectablePanel();
        this.ButtonGenerate = new System.Windows.Forms.Button();
        this.TableSpritePreview = new System.Windows.Forms.TableLayoutPanel();
        this.LabelSprite = new System.Windows.Forms.Label();
        this.ButtonPrevSprite = new System.Windows.Forms.Button();
        this.ButtonNextSprite = new System.Windows.Forms.Button();
        this.TableDialogControls = new System.Windows.Forms.TableLayoutPanel();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.ButtonOk = new System.Windows.Forms.Button();
        this.TableSkipMaskColor = new System.Windows.Forms.TableLayoutPanel();
        this.label7 = new System.Windows.Forms.Label();
        this.SkipColor = new Fetze.WinFormsColor.ColorShowBox();
        this.PanelSpritePreview = new System.Windows.Forms.Panel();
        this.CheckPreviewSprites = new System.Windows.Forms.CheckBox();
        this.TipInstructions = new System.Windows.Forms.ToolTip(this.components);
        this.TableExtractControls.SuspendLayout();
        this.PanelSkip.SuspendLayout();
        this.TableExtractParams.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericOffsetX)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericCellWidth)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericCellHeight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericOffsetY)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericColumnCount)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericRowCount)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericArrayIndex)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericArrayCount)).BeginInit();
        this.panel3.SuspendLayout();
        this.panel4.SuspendLayout();
        this.TableSpritePreview.SuspendLayout();
        this.TableDialogControls.SuspendLayout();
        this.TableSkipMaskColor.SuspendLayout();
        this.PanelSpritePreview.SuspendLayout();
        this.SuspendLayout();
        // 
        // TableExtractControls
        // 
        this.TableExtractControls.AutoSize = true;
        this.TableExtractControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableExtractControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.TableExtractControls.ColumnCount = 2;
        this.TableExtractControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableExtractControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableExtractControls.Controls.Add(this.PanelSkip, 1, 1);
        this.TableExtractControls.Controls.Add(this.TableExtractParams, 1, 0);
        this.TableExtractControls.Controls.Add(this.PanelRender, 0, 0);
        this.TableExtractControls.Controls.Add(this.ButtonGenerate, 1, 3);
        this.TableExtractControls.Controls.Add(this.TableSpritePreview, 1, 5);
        this.TableExtractControls.Controls.Add(this.TableDialogControls, 1, 7);
        this.TableExtractControls.Controls.Add(this.TableSkipMaskColor, 1, 2);
        this.TableExtractControls.Controls.Add(this.PanelSpritePreview, 1, 4);
        this.TableExtractControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableExtractControls.Location = new System.Drawing.Point(0, 0);
        this.TableExtractControls.Name = "TableExtractControls";
        this.TableExtractControls.RowCount = 8;
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableExtractControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractControls.Size = new System.Drawing.Size(1008, 729);
        this.TableExtractControls.TabIndex = 0;
        // 
        // PanelSkip
        // 
        this.PanelSkip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PanelSkip.AutoSize = true;
        this.PanelSkip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.PanelSkip.Controls.Add(this.CheckSkipEmpty);
        this.PanelSkip.Location = new System.Drawing.Point(679, 248);
        this.PanelSkip.Margin = new System.Windows.Forms.Padding(0);
        this.PanelSkip.Name = "PanelSkip";
        this.PanelSkip.Size = new System.Drawing.Size(329, 21);
        this.PanelSkip.TabIndex = 3;
        // 
        // CheckSkipEmpty
        // 
        this.CheckSkipEmpty.AutoSize = true;
        this.CheckSkipEmpty.Checked = true;
        this.CheckSkipEmpty.CheckState = System.Windows.Forms.CheckState.Checked;
        this.CheckSkipEmpty.Dock = System.Windows.Forms.DockStyle.Top;
        this.CheckSkipEmpty.Font = new System.Drawing.Font("Segoe UI", 9.75F);
        this.CheckSkipEmpty.Location = new System.Drawing.Point(0, 0);
        this.CheckSkipEmpty.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
        this.CheckSkipEmpty.Name = "CheckSkipEmpty";
        this.CheckSkipEmpty.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
        this.CheckSkipEmpty.Size = new System.Drawing.Size(329, 21);
        this.CheckSkipEmpty.TabIndex = 0;
        this.CheckSkipEmpty.Text = "Skip Empty";
        this.TipInstructions.SetToolTip(this.CheckSkipEmpty, "Sets whether empty regions on the texture are skipped when generating sprites.\r\n\r" +
    "\nAn empty region has all of its pixel values matching the color/alpha value spec" +
    "ified by the user.");
        this.CheckSkipEmpty.UseVisualStyleBackColor = false;
        this.CheckSkipEmpty.Click += new System.EventHandler(this.CheckSkipEmpty_Click);
        // 
        // TableExtractParams
        // 
        this.TableExtractParams.AutoSize = true;
        this.TableExtractParams.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableExtractParams.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableExtractParams.ColumnCount = 4;
        this.TableExtractParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableExtractParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableExtractParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableExtractParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableExtractParams.Controls.Add(this.NumericOffsetX, 1, 4);
        this.TableExtractParams.Controls.Add(this.label2, 0, 2);
        this.TableExtractParams.Controls.Add(this.label1, 0, 1);
        this.TableExtractParams.Controls.Add(this.label3, 2, 2);
        this.TableExtractParams.Controls.Add(this.NumericCellWidth, 1, 2);
        this.TableExtractParams.Controls.Add(this.NumericCellHeight, 3, 2);
        this.TableExtractParams.Controls.Add(this.label4, 0, 3);
        this.TableExtractParams.Controls.Add(this.label5, 0, 4);
        this.TableExtractParams.Controls.Add(this.label6, 2, 4);
        this.TableExtractParams.Controls.Add(this.NumericOffsetY, 3, 4);
        this.TableExtractParams.Controls.Add(this.label12, 0, 5);
        this.TableExtractParams.Controls.Add(this.label13, 0, 6);
        this.TableExtractParams.Controls.Add(this.label14, 2, 6);
        this.TableExtractParams.Controls.Add(this.NumericColumnCount, 1, 6);
        this.TableExtractParams.Controls.Add(this.NumericRowCount, 3, 6);
        this.TableExtractParams.Controls.Add(this.LabelArrayRange, 0, 8);
        this.TableExtractParams.Controls.Add(this.LabelArrayStart, 0, 9);
        this.TableExtractParams.Controls.Add(this.NumericArrayIndex, 1, 9);
        this.TableExtractParams.Controls.Add(this.LabelArrayCount, 2, 9);
        this.TableExtractParams.Controls.Add(this.NumericArrayCount, 3, 9);
        this.TableExtractParams.Controls.Add(this.panel3, 0, 7);
        this.TableExtractParams.Controls.Add(this.panel4, 0, 0);
        this.TableExtractParams.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableExtractParams.Location = new System.Drawing.Point(679, 0);
        this.TableExtractParams.Margin = new System.Windows.Forms.Padding(0);
        this.TableExtractParams.Name = "TableExtractParams";
        this.TableExtractParams.RowCount = 13;
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableExtractParams.Size = new System.Drawing.Size(329, 248);
        this.TableExtractParams.TabIndex = 2;
        // 
        // NumericOffsetX
        // 
        this.NumericOffsetX.Location = new System.Drawing.Point(69, 98);
        this.NumericOffsetX.Maximum = new decimal(new int[] {
        16383,
        0,
        0,
        0});
        this.NumericOffsetX.Name = "NumericOffsetX";
        this.NumericOffsetX.Size = new System.Drawing.Size(100, 23);
        this.NumericOffsetX.TabIndex = 2;
        this.NumericOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericOffsetX.ValueChanged += new System.EventHandler(this.NumericOffsetX_ValueChanged);
        // 
        // label2
        // 
        this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(24, 50);
        this.label2.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(39, 15);
        this.label2.TabIndex = 0;
        this.label2.Text = "Width";
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.TableExtractParams.SetColumnSpan(this.label1, 2);
        this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
        this.label1.Location = new System.Drawing.Point(8, 23);
        this.label1.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(85, 17);
        this.label1.TabIndex = 0;
        this.label1.Text = "Grid Cell Size";
        this.TipInstructions.SetToolTip(this.label1, "Sets the size of the cells, in pixels, for the grid.\r\n\r\nAll cells are uniformly s" +
    "ized, so this will affect all cells in the grid.");
        // 
        // label3
        // 
        this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(177, 50);
        this.label3.Margin = new System.Windows.Forms.Padding(3);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(43, 15);
        this.label3.TabIndex = 1;
        this.label3.Text = "Height";
        // 
        // NumericCellWidth
        // 
        this.NumericCellWidth.Location = new System.Drawing.Point(69, 46);
        this.NumericCellWidth.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericCellWidth.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericCellWidth.Name = "NumericCellWidth";
        this.NumericCellWidth.Size = new System.Drawing.Size(100, 23);
        this.NumericCellWidth.TabIndex = 0;
        this.NumericCellWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericCellWidth.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericCellWidth.ValueChanged += new System.EventHandler(this.NumericCellWidth_ValueChanged);
        // 
        // NumericCellHeight
        // 
        this.NumericCellHeight.Location = new System.Drawing.Point(226, 46);
        this.NumericCellHeight.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericCellHeight.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericCellHeight.Name = "NumericCellHeight";
        this.NumericCellHeight.Size = new System.Drawing.Size(100, 23);
        this.NumericCellHeight.TabIndex = 1;
        this.NumericCellHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericCellHeight.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericCellHeight.ValueChanged += new System.EventHandler(this.NumericCellHeight_ValueChanged);
        // 
        // label4
        // 
        this.label4.AutoSize = true;
        this.TableExtractParams.SetColumnSpan(this.label4, 2);
        this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F);
        this.label4.Location = new System.Drawing.Point(8, 75);
        this.label4.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(72, 17);
        this.label4.TabIndex = 4;
        this.label4.Text = "Grid Offset";
        this.TipInstructions.SetToolTip(this.label4, "Sets where to start capturing in the texture. \r\n\r\nFor example, specifying 10x10 w" +
    "ill place the grid at 10 pixels from the left, and 10 pixels from the top.");
        // 
        // label5
        // 
        this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label5.AutoSize = true;
        this.label5.Location = new System.Drawing.Point(49, 102);
        this.label5.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label5.Name = "label5";
        this.label5.Size = new System.Drawing.Size(14, 15);
        this.label5.TabIndex = 5;
        this.label5.Text = "X";
        // 
        // label6
        // 
        this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label6.AutoSize = true;
        this.label6.Location = new System.Drawing.Point(206, 102);
        this.label6.Margin = new System.Windows.Forms.Padding(3);
        this.label6.Name = "label6";
        this.label6.Size = new System.Drawing.Size(14, 15);
        this.label6.TabIndex = 7;
        this.label6.Text = "Y";
        // 
        // NumericOffsetY
        // 
        this.NumericOffsetY.Location = new System.Drawing.Point(226, 98);
        this.NumericOffsetY.Maximum = new decimal(new int[] {
        16383,
        0,
        0,
        0});
        this.NumericOffsetY.Name = "NumericOffsetY";
        this.NumericOffsetY.Size = new System.Drawing.Size(100, 23);
        this.NumericOffsetY.TabIndex = 3;
        this.NumericOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericOffsetY.ValueChanged += new System.EventHandler(this.NumericOffsetY_ValueChanged);
        // 
        // label12
        // 
        this.label12.AutoSize = true;
        this.TableExtractParams.SetColumnSpan(this.label12, 2);
        this.label12.Font = new System.Drawing.Font("Segoe UI", 9.75F);
        this.label12.Location = new System.Drawing.Point(8, 127);
        this.label12.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label12.Name = "label12";
        this.label12.Size = new System.Drawing.Size(104, 17);
        this.label12.TabIndex = 13;
        this.label12.Text = "Grid Dimensions";
        this.TipInstructions.SetToolTip(this.label12, "Sets the number of columns and rows in the grid.");
        // 
        // label13
        // 
        this.label13.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label13.AutoSize = true;
        this.label13.Location = new System.Drawing.Point(8, 154);
        this.label13.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label13.Name = "label13";
        this.label13.Size = new System.Drawing.Size(55, 15);
        this.label13.TabIndex = 14;
        this.label13.Text = "Columns";
        // 
        // label14
        // 
        this.label14.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label14.AutoSize = true;
        this.label14.Location = new System.Drawing.Point(185, 154);
        this.label14.Margin = new System.Windows.Forms.Padding(3);
        this.label14.Name = "label14";
        this.label14.Size = new System.Drawing.Size(35, 15);
        this.label14.TabIndex = 14;
        this.label14.Text = "Rows";
        // 
        // NumericColumnCount
        // 
        this.NumericColumnCount.Location = new System.Drawing.Point(69, 150);
        this.NumericColumnCount.Maximum = new decimal(new int[] {
        32768,
        0,
        0,
        0});
        this.NumericColumnCount.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericColumnCount.Name = "NumericColumnCount";
        this.NumericColumnCount.Size = new System.Drawing.Size(100, 23);
        this.NumericColumnCount.TabIndex = 4;
        this.NumericColumnCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericColumnCount.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericColumnCount.ValueChanged += new System.EventHandler(this.NumericColumnCount_ValueChanged);
        // 
        // NumericRowCount
        // 
        this.NumericRowCount.Location = new System.Drawing.Point(226, 150);
        this.NumericRowCount.Maximum = new decimal(new int[] {
        32768,
        0,
        0,
        0});
        this.NumericRowCount.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericRowCount.Name = "NumericRowCount";
        this.NumericRowCount.Size = new System.Drawing.Size(100, 23);
        this.NumericRowCount.TabIndex = 5;
        this.NumericRowCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericRowCount.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericRowCount.ValueChanged += new System.EventHandler(this.NumericRowCount_ValueChanged);
        // 
        // LabelArrayRange
        // 
        this.LabelArrayRange.AutoSize = true;
        this.TableExtractParams.SetColumnSpan(this.LabelArrayRange, 2);
        this.LabelArrayRange.Font = new System.Drawing.Font("Segoe UI", 9.75F);
        this.LabelArrayRange.Location = new System.Drawing.Point(8, 199);
        this.LabelArrayRange.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.LabelArrayRange.Name = "LabelArrayRange";
        this.LabelArrayRange.Size = new System.Drawing.Size(80, 17);
        this.LabelArrayRange.TabIndex = 10;
        this.LabelArrayRange.Text = "Array Range";
        this.TipInstructions.SetToolTip(this.LabelArrayRange, resources.GetString("LabelArrayRange.ToolTip"));
        this.LabelArrayRange.Visible = false;
        // 
        // LabelArrayStart
        // 
        this.LabelArrayStart.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelArrayStart.AutoSize = true;
        this.LabelArrayStart.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.LabelArrayStart.Location = new System.Drawing.Point(32, 226);
        this.LabelArrayStart.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.LabelArrayStart.Name = "LabelArrayStart";
        this.LabelArrayStart.Size = new System.Drawing.Size(31, 15);
        this.LabelArrayStart.TabIndex = 11;
        this.LabelArrayStart.Text = "Start";
        this.LabelArrayStart.Visible = false;
        // 
        // NumericArrayIndex
        // 
        this.NumericArrayIndex.Location = new System.Drawing.Point(69, 222);
        this.NumericArrayIndex.Maximum = new decimal(new int[] {
        4095,
        0,
        0,
        0});
        this.NumericArrayIndex.Name = "NumericArrayIndex";
        this.NumericArrayIndex.Size = new System.Drawing.Size(100, 23);
        this.NumericArrayIndex.TabIndex = 6;
        this.NumericArrayIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericArrayIndex.Visible = false;
        this.NumericArrayIndex.ValueChanged += new System.EventHandler(this.NumericArrayIndex_ValueChanged);
        // 
        // LabelArrayCount
        // 
        this.LabelArrayCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelArrayCount.AutoSize = true;
        this.LabelArrayCount.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.LabelArrayCount.Location = new System.Drawing.Point(180, 226);
        this.LabelArrayCount.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.LabelArrayCount.Name = "LabelArrayCount";
        this.LabelArrayCount.Size = new System.Drawing.Size(40, 15);
        this.LabelArrayCount.TabIndex = 11;
        this.LabelArrayCount.Text = "Count";
        this.LabelArrayCount.Visible = false;
        // 
        // NumericArrayCount
        // 
        this.NumericArrayCount.Location = new System.Drawing.Point(226, 222);
        this.NumericArrayCount.Maximum = new decimal(new int[] {
        4096,
        0,
        0,
        0});
        this.NumericArrayCount.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericArrayCount.Name = "NumericArrayCount";
        this.NumericArrayCount.Size = new System.Drawing.Size(100, 23);
        this.NumericArrayCount.TabIndex = 7;
        this.NumericArrayCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericArrayCount.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericArrayCount.Visible = false;
        this.NumericArrayCount.ValueChanged += new System.EventHandler(this.NumericArrayCount_ValueChanged);
        // 
        // panel3
        // 
        this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.panel3.AutoSize = true;
        this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.TableExtractParams.SetColumnSpan(this.panel3, 4);
        this.panel3.Controls.Add(this.LabelTextureHeader);
        this.panel3.Location = new System.Drawing.Point(0, 176);
        this.panel3.Margin = new System.Windows.Forms.Padding(0);
        this.panel3.Name = "panel3";
        this.panel3.Size = new System.Drawing.Size(329, 20);
        this.panel3.TabIndex = 15;
        // 
        // LabelTextureHeader
        // 
        this.LabelTextureHeader.AutoSize = true;
        this.LabelTextureHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.LabelTextureHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
        this.LabelTextureHeader.Location = new System.Drawing.Point(0, 0);
        this.LabelTextureHeader.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
        this.LabelTextureHeader.Name = "LabelTextureHeader";
        this.LabelTextureHeader.Size = new System.Drawing.Size(60, 20);
        this.LabelTextureHeader.TabIndex = 9;
        this.LabelTextureHeader.Text = "Texture";
        // 
        // panel4
        // 
        this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.panel4.AutoSize = true;
        this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.TableExtractParams.SetColumnSpan(this.panel4, 4);
        this.panel4.Controls.Add(this.label11);
        this.panel4.Location = new System.Drawing.Point(0, 0);
        this.panel4.Margin = new System.Windows.Forms.Padding(0);
        this.panel4.Name = "panel4";
        this.panel4.Size = new System.Drawing.Size(329, 20);
        this.panel4.TabIndex = 16;
        // 
        // label11
        // 
        this.label11.AutoSize = true;
        this.label11.Dock = System.Windows.Forms.DockStyle.Top;
        this.label11.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
        this.label11.Location = new System.Drawing.Point(0, 0);
        this.label11.Margin = new System.Windows.Forms.Padding(3);
        this.label11.Name = "label11";
        this.label11.Size = new System.Drawing.Size(38, 20);
        this.label11.TabIndex = 12;
        this.label11.Text = "Grid";
        // 
        // PanelRender
        // 
        this.PanelRender.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelRender.Location = new System.Drawing.Point(3, 3);
        this.PanelRender.Name = "PanelRender";
        this.TableExtractControls.SetRowSpan(this.PanelRender, 8);
        this.PanelRender.ShowFocus = false;
        this.PanelRender.Size = new System.Drawing.Size(673, 723);
        this.PanelRender.TabIndex = 0;
        this.PanelRender.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelRender_PreviewKeyDown);
        // 
        // ButtonGenerate
        // 
        this.ButtonGenerate.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.ButtonGenerate.AutoSize = true;
        this.ButtonGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonGenerate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonGenerate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonGenerate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonGenerate.Location = new System.Drawing.Point(773, 302);
        this.ButtonGenerate.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
        this.ButtonGenerate.Name = "ButtonGenerate";
        this.ButtonGenerate.Size = new System.Drawing.Size(141, 32);
        this.ButtonGenerate.TabIndex = 5;
        this.ButtonGenerate.Text = "&Generate Sprites";
        this.ButtonGenerate.UseVisualStyleBackColor = false;
        this.ButtonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
        // 
        // TableSpritePreview
        // 
        this.TableSpritePreview.AutoSize = true;
        this.TableSpritePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableSpritePreview.ColumnCount = 5;
        this.TableSpritePreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableSpritePreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableSpritePreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        this.TableSpritePreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableSpritePreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableSpritePreview.Controls.Add(this.LabelSprite, 2, 0);
        this.TableSpritePreview.Controls.Add(this.ButtonPrevSprite, 1, 0);
        this.TableSpritePreview.Controls.Add(this.ButtonNextSprite, 3, 0);
        this.TableSpritePreview.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableSpritePreview.Location = new System.Drawing.Point(679, 366);
        this.TableSpritePreview.Margin = new System.Windows.Forms.Padding(0);
        this.TableSpritePreview.Name = "TableSpritePreview";
        this.TableSpritePreview.RowCount = 1;
        this.TableSpritePreview.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableSpritePreview.Size = new System.Drawing.Size(329, 29);
        this.TableSpritePreview.TabIndex = 6;
        this.TableSpritePreview.Visible = false;
        // 
        // LabelSprite
        // 
        this.LabelSprite.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelSprite.AutoSize = true;
        this.LabelSprite.Location = new System.Drawing.Point(135, 7);
        this.LabelSprite.Margin = new System.Windows.Forms.Padding(16, 3, 16, 3);
        this.LabelSprite.Name = "LabelSprite";
        this.LabelSprite.Size = new System.Drawing.Size(57, 15);
        this.LabelSprite.TabIndex = 1;
        this.LabelSprite.Text = "Sprite x/y";
        // 
        // ButtonPrevSprite
        // 
        this.ButtonPrevSprite.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonPrevSprite.AutoSize = true;
        this.ButtonPrevSprite.Enabled = false;
        this.ButtonPrevSprite.FlatAppearance.BorderSize = 0;
        this.ButtonPrevSprite.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonPrevSprite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevSprite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevSprite.Image = global::Gorgon.Editor.ExtractSpriteTool.Properties.Resources.left_16x16;
        this.ButtonPrevSprite.Location = new System.Drawing.Point(58, 3);
        this.ButtonPrevSprite.Name = "ButtonPrevSprite";
        this.ButtonPrevSprite.Size = new System.Drawing.Size(23, 23);
        this.ButtonPrevSprite.TabIndex = 0;
        this.ButtonPrevSprite.UseVisualStyleBackColor = true;
        this.ButtonPrevSprite.Click += new System.EventHandler(this.ButtonPrevSprite_Click);
        // 
        // ButtonNextSprite
        // 
        this.ButtonNextSprite.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonNextSprite.AutoSize = true;
        this.ButtonNextSprite.Enabled = false;
        this.ButtonNextSprite.FlatAppearance.BorderSize = 0;
        this.ButtonNextSprite.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonNextSprite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextSprite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextSprite.Image = global::Gorgon.Editor.ExtractSpriteTool.Properties.Resources.right_16x16;
        this.ButtonNextSprite.Location = new System.Drawing.Point(247, 3);
        this.ButtonNextSprite.Name = "ButtonNextSprite";
        this.ButtonNextSprite.Size = new System.Drawing.Size(23, 23);
        this.ButtonNextSprite.TabIndex = 2;
        this.ButtonNextSprite.UseVisualStyleBackColor = true;
        this.ButtonNextSprite.Click += new System.EventHandler(this.ButtonNextSprite_Click);
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
        this.TableDialogControls.Location = new System.Drawing.Point(682, 688);
        this.TableDialogControls.Name = "TableDialogControls";
        this.TableDialogControls.RowCount = 1;
        this.TableDialogControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableDialogControls.Size = new System.Drawing.Size(323, 38);
        this.TableDialogControls.TabIndex = 7;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(229, 3);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(91, 32);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
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
        this.ButtonOk.Location = new System.Drawing.Point(132, 3);
        this.ButtonOk.Name = "ButtonOk";
        this.ButtonOk.Size = new System.Drawing.Size(91, 32);
        this.ButtonOk.TabIndex = 0;
        this.ButtonOk.Text = "&Save";
        this.ButtonOk.UseVisualStyleBackColor = false;
        this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
        // 
        // TableSkipMaskColor
        // 
        this.TableSkipMaskColor.AutoSize = true;
        this.TableSkipMaskColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableSkipMaskColor.ColumnCount = 2;
        this.TableSkipMaskColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableSkipMaskColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableSkipMaskColor.Controls.Add(this.label7, 0, 0);
        this.TableSkipMaskColor.Controls.Add(this.SkipColor, 1, 0);
        this.TableSkipMaskColor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableSkipMaskColor.Location = new System.Drawing.Point(679, 269);
        this.TableSkipMaskColor.Margin = new System.Windows.Forms.Padding(0);
        this.TableSkipMaskColor.Name = "TableSkipMaskColor";
        this.TableSkipMaskColor.RowCount = 1;
        this.TableSkipMaskColor.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableSkipMaskColor.Size = new System.Drawing.Size(329, 30);
        this.TableSkipMaskColor.TabIndex = 4;
        // 
        // label7
        // 
        this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.label7.AutoSize = true;
        this.label7.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.label7.Location = new System.Drawing.Point(26, 7);
        this.label7.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
        this.label7.Name = "label7";
        this.label7.Size = new System.Drawing.Size(36, 15);
        this.label7.TabIndex = 11;
        this.label7.Text = "Color";
        this.label7.Click += new System.EventHandler(this.SkipColor_Click);
        // 
        // SkipColor
        // 
        this.SkipColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.SkipColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.SkipColor.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        this.SkipColor.Location = new System.Drawing.Point(73, 3);
        this.SkipColor.LowerColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        this.SkipColor.Margin = new System.Windows.Forms.Padding(8, 3, 8, 3);
        this.SkipColor.Name = "SkipColor";
        this.SkipColor.Size = new System.Drawing.Size(248, 24);
        this.SkipColor.TabIndex = 0;
        this.TipInstructions.SetToolTip(this.SkipColor, "The color used to determine an empty region on the texture.\r\n\r\nClicking on this c" +
    "ontrol will let you modify the color to use.");
        this.SkipColor.UpperColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        this.SkipColor.Click += new System.EventHandler(this.SkipColor_Click);
        // 
        // PanelSpritePreview
        // 
        this.PanelSpritePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PanelSpritePreview.AutoSize = true;
        this.PanelSpritePreview.Controls.Add(this.CheckPreviewSprites);
        this.PanelSpritePreview.Location = new System.Drawing.Point(679, 342);
        this.PanelSpritePreview.Margin = new System.Windows.Forms.Padding(0);
        this.PanelSpritePreview.Name = "PanelSpritePreview";
        this.PanelSpritePreview.Size = new System.Drawing.Size(329, 24);
        this.PanelSpritePreview.TabIndex = 9;
        // 
        // CheckPreviewSprites
        // 
        this.CheckPreviewSprites.AutoSize = true;
        this.CheckPreviewSprites.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.CheckPreviewSprites.Dock = System.Windows.Forms.DockStyle.Top;
        this.CheckPreviewSprites.Enabled = false;
        this.CheckPreviewSprites.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F);
        this.CheckPreviewSprites.Location = new System.Drawing.Point(0, 0);
        this.CheckPreviewSprites.Margin = new System.Windows.Forms.Padding(8, 16, 3, 3);
        this.CheckPreviewSprites.Name = "CheckPreviewSprites";
        this.CheckPreviewSprites.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
        this.CheckPreviewSprites.Size = new System.Drawing.Size(329, 24);
        this.CheckPreviewSprites.TabIndex = 5;
        this.CheckPreviewSprites.Text = "Sprite Preview";
        this.TipInstructions.SetToolTip(this.CheckPreviewSprites, "Shows the currently captured sprites.");
        this.CheckPreviewSprites.UseVisualStyleBackColor = false;
        this.CheckPreviewSprites.CheckedChanged += new System.EventHandler(this.CheckPreviewSprites_CheckedChanged);
        // 
        // TipInstructions
        // 
        this.TipInstructions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.TipInstructions.ForeColor = System.Drawing.Color.White;
        this.TipInstructions.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
        this.TipInstructions.ToolTipTitle = "Help";
        // 
        // FormExtract
        // 
        this.AcceptButton = this.ButtonOk;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(1008, 729);
        this.Controls.Add(this.TableExtractControls);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "FormExtract";
        this.RenderControl = this.PanelRender;
        this.Text = "Extract Sprites by Grid";
        this.TableExtractControls.ResumeLayout(false);
        this.TableExtractControls.PerformLayout();
        this.PanelSkip.ResumeLayout(false);
        this.PanelSkip.PerformLayout();
        this.TableExtractParams.ResumeLayout(false);
        this.TableExtractParams.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericOffsetX)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericCellWidth)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericCellHeight)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericOffsetY)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericColumnCount)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericRowCount)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericArrayIndex)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericArrayCount)).EndInit();
        this.panel3.ResumeLayout(false);
        this.panel3.PerformLayout();
        this.panel4.ResumeLayout(false);
        this.panel4.PerformLayout();
        this.TableSpritePreview.ResumeLayout(false);
        this.TableSpritePreview.PerformLayout();
        this.TableDialogControls.ResumeLayout(false);
        this.TableDialogControls.PerformLayout();
        this.TableSkipMaskColor.ResumeLayout(false);
        this.TableSkipMaskColor.PerformLayout();
        this.PanelSpritePreview.ResumeLayout(false);
        this.PanelSpritePreview.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private System.Windows.Forms.TableLayoutPanel TableExtractControls;
    private Gorgon.UI.GorgonSelectablePanel PanelRender;
    private System.Windows.Forms.ToolTip TipInstructions;
    private System.Windows.Forms.TableLayoutPanel TableExtractParams;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.NumericUpDown NumericOffsetX;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown NumericCellWidth;
    private System.Windows.Forms.NumericUpDown NumericCellHeight;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.NumericUpDown NumericOffsetY;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.NumericUpDown NumericColumnCount;
    private System.Windows.Forms.NumericUpDown NumericRowCount;
    private System.Windows.Forms.Label LabelTextureHeader;
    private System.Windows.Forms.Label LabelArrayRange;
    private System.Windows.Forms.Label LabelArrayStart;
    private System.Windows.Forms.NumericUpDown NumericArrayIndex;
    private System.Windows.Forms.Label LabelArrayCount;
    private System.Windows.Forms.NumericUpDown NumericArrayCount;
    private System.Windows.Forms.Button ButtonGenerate;
    private System.Windows.Forms.CheckBox CheckPreviewSprites;
    private System.Windows.Forms.TableLayoutPanel TableSpritePreview;
    private System.Windows.Forms.Label LabelSprite;
    private System.Windows.Forms.Button ButtonPrevSprite;
    private System.Windows.Forms.Button ButtonNextSprite;
    private System.Windows.Forms.TableLayoutPanel TableDialogControls;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.Button ButtonOk;
    private System.Windows.Forms.CheckBox CheckSkipEmpty;
    private System.Windows.Forms.TableLayoutPanel TableSkipMaskColor;
    private System.Windows.Forms.Label label7;
    private Fetze.WinFormsColor.ColorShowBox SkipColor;
    private System.Windows.Forms.Panel PanelSkip;
    private System.Windows.Forms.Panel PanelSpritePreview;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel4;
}
