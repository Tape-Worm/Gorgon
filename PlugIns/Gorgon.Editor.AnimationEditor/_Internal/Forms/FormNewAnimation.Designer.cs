namespace Gorgon.Editor.AnimationEditor;

partial class FormNewAnimation
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
            PictureSpritePreview.Image = null;
            _previewImage?.Dispose();
            _previewSprite?.Dispose();
            _cancelTextureSource?.Dispose();
            _cancelSpriteSource?.Dispose();
        }

        base.Dispose(disposing);
    }



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewAnimation));
        this.TableName = new System.Windows.Forms.TableLayoutPanel();
        this.PanelName = new System.Windows.Forms.Panel();
        this.TextName = new Gorgon.UI.GorgonCueTextBox();
        this.PanelUnderline = new System.Windows.Forms.Panel();
        this.LabelLength = new System.Windows.Forms.Label();
        this.NumericLength = new System.Windows.Forms.NumericUpDown();
        this.NumericFps = new System.Windows.Forms.NumericUpDown();
        this.LabelFps = new System.Windows.Forms.Label();
        this.panel1 = new System.Windows.Forms.Panel();
        this.LabelFrameCount = new System.Windows.Forms.Label();
        this.PanelButtons = new System.Windows.Forms.Panel();
        this.ButtonOK = new System.Windows.Forms.Button();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.FilePrimarySprite = new Gorgon.Editor.UI.Controls.ContentFileExplorer();
        this.PictureSpritePreview = new System.Windows.Forms.PictureBox();
        this.TableTextureSelection = new System.Windows.Forms.TableLayoutPanel();
        this.FileBgTextures = new Gorgon.Editor.UI.Controls.ContentFileExplorer();
        this.PictureTexturePreview = new System.Windows.Forms.PictureBox();
        this.TableName.SuspendLayout();
        this.PanelName.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericLength)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericFps)).BeginInit();
        this.panel1.SuspendLayout();
        this.PanelButtons.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PictureSpritePreview)).BeginInit();
        this.TableTextureSelection.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PictureTexturePreview)).BeginInit();
        this.SuspendLayout();
        // 
        // TableName
        // 
        this.TableName.AutoSize = true;
        this.TableName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableName.ColumnCount = 5;
        this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableName.Controls.Add(this.PanelName, 0, 0);
        this.TableName.Controls.Add(this.PanelUnderline, 0, 1);
        this.TableName.Controls.Add(this.LabelLength, 0, 2);
        this.TableName.Controls.Add(this.NumericLength, 1, 2);
        this.TableName.Controls.Add(this.NumericFps, 3, 2);
        this.TableName.Controls.Add(this.LabelFps, 2, 2);
        this.TableName.Controls.Add(this.panel1, 4, 2);
        this.TableName.Dock = System.Windows.Forms.DockStyle.Top;
        this.TableName.Location = new System.Drawing.Point(0, 0);
        this.TableName.Margin = new System.Windows.Forms.Padding(0);
        this.TableName.Name = "TableName";
        this.TableName.RowCount = 3;
        this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
        this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableName.Size = new System.Drawing.Size(784, 54);
        this.TableName.TabIndex = 0;
        // 
        // PanelName
        // 
        this.PanelName.AutoSize = true;
        this.TableName.SetColumnSpan(this.PanelName, 5);
        this.PanelName.Controls.Add(this.TextName);
        this.PanelName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelName.Location = new System.Drawing.Point(3, 0);
        this.PanelName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.PanelName.Name = "PanelName";
        this.PanelName.Size = new System.Drawing.Size(778, 22);
        this.PanelName.TabIndex = 0;
        // 
        // TextName
        // 
        this.TextName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TextName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TextName.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.TextName.CueText = "Enter a name for the {0}...";
        this.TextName.Font = new System.Drawing.Font("Segoe UI", 12F);
        this.TextName.ForeColor = System.Drawing.Color.White;
        this.TextName.Location = new System.Drawing.Point(0, 0);
        this.TextName.Margin = new System.Windows.Forms.Padding(0);
        this.TextName.MaxLength = 100;
        this.TextName.Name = "TextName";
        this.TextName.Size = new System.Drawing.Size(778, 22);
        this.TextName.TabIndex = 0;
        this.TextName.TextChanged += new System.EventHandler(this.TextName_TextChanged);
        this.TextName.Enter += new System.EventHandler(this.TextName_Enter);
        this.TextName.Leave += new System.EventHandler(this.TextName_Leave);
        this.TextName.MouseEnter += new System.EventHandler(this.TextName_MouseEnter);
        this.TextName.MouseLeave += new System.EventHandler(this.TextName_MouseLeave);
        // 
        // PanelUnderline
        // 
        this.PanelUnderline.BackColor = System.Drawing.Color.Black;
        this.TableName.SetColumnSpan(this.PanelUnderline, 5);
        this.PanelUnderline.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelUnderline.Location = new System.Drawing.Point(3, 22);
        this.PanelUnderline.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.PanelUnderline.Name = "PanelUnderline";
        this.PanelUnderline.Size = new System.Drawing.Size(778, 3);
        this.PanelUnderline.TabIndex = 0;
        // 
        // LabelLength
        // 
        this.LabelLength.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelLength.AutoSize = true;
        this.LabelLength.Location = new System.Drawing.Point(3, 32);
        this.LabelLength.Name = "LabelLength";
        this.LabelLength.Size = new System.Drawing.Size(101, 15);
        this.LabelLength.TabIndex = 1;
        this.LabelLength.Text = "Length (seconds):";
        this.LabelLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // NumericLength
        // 
        this.NumericLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericLength.AutoSize = true;
        this.NumericLength.BackColor = System.Drawing.Color.White;
        this.NumericLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericLength.DecimalPlaces = 1;
        this.NumericLength.ForeColor = System.Drawing.Color.Black;
        this.NumericLength.Location = new System.Drawing.Point(110, 28);
        this.NumericLength.Maximum = new decimal(new int[] {
        7200,
        0,
        0,
        0});
        this.NumericLength.MaximumSize = new System.Drawing.Size(113, 0);
        this.NumericLength.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        65536});
        this.NumericLength.MinimumSize = new System.Drawing.Size(110, 0);
        this.NumericLength.Name = "NumericLength";
        this.NumericLength.Size = new System.Drawing.Size(110, 23);
        this.NumericLength.TabIndex = 2;
        this.NumericLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericLength.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericLength.ValueChanged += new System.EventHandler(this.NumericLength_ValueChanged);
        // 
        // NumericFps
        // 
        this.NumericFps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericFps.AutoSize = true;
        this.NumericFps.BackColor = System.Drawing.Color.White;
        this.NumericFps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericFps.DecimalPlaces = 6;
        this.NumericFps.ForeColor = System.Drawing.Color.Black;
        this.NumericFps.Location = new System.Drawing.Point(341, 28);
        this.NumericFps.Maximum = new decimal(new int[] {
        1000,
        0,
        0,
        0});
        this.NumericFps.MaximumSize = new System.Drawing.Size(113, 0);
        this.NumericFps.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.NumericFps.MinimumSize = new System.Drawing.Size(110, 0);
        this.NumericFps.Name = "NumericFps";
        this.NumericFps.Size = new System.Drawing.Size(110, 23);
        this.NumericFps.TabIndex = 4;
        this.NumericFps.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericFps.Value = new decimal(new int[] {
        60,
        0,
        0,
        0});
        this.NumericFps.ValueChanged += new System.EventHandler(this.NumericLength_ValueChanged);
        // 
        // LabelFps
        // 
        this.LabelFps.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelFps.AutoSize = true;
        this.LabelFps.Location = new System.Drawing.Point(226, 32);
        this.LabelFps.Name = "LabelFps";
        this.LabelFps.Size = new System.Drawing.Size(109, 15);
        this.LabelFps.TabIndex = 3;
        this.LabelFps.Text = "Frames per second:";
        this.LabelFps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // panel1
        // 
        this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.panel1.AutoSize = true;
        this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.panel1.Controls.Add(this.LabelFrameCount);
        this.panel1.Location = new System.Drawing.Point(454, 25);
        this.panel1.Margin = new System.Windows.Forms.Padding(0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(330, 29);
        this.panel1.TabIndex = 5;
        // 
        // LabelFrameCount
        // 
        this.LabelFrameCount.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelFrameCount.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.LabelFrameCount.Location = new System.Drawing.Point(0, 0);
        this.LabelFrameCount.Name = "LabelFrameCount";
        this.LabelFrameCount.Size = new System.Drawing.Size(330, 29);
        this.LabelFrameCount.TabIndex = 6;
        this.LabelFrameCount.Text = "Frame Count: 300";
        this.LabelFrameCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // PanelButtons
        // 
        this.PanelButtons.AutoSize = true;
        this.PanelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.PanelButtons.Controls.Add(this.ButtonOK);
        this.PanelButtons.Controls.Add(this.ButtonCancel);
        this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.PanelButtons.Location = new System.Drawing.Point(0, 507);
        this.PanelButtons.Name = "PanelButtons";
        this.PanelButtons.Size = new System.Drawing.Size(784, 54);
        this.PanelButtons.TabIndex = 2;
        // 
        // ButtonOK
        // 
        this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonOK.AutoSize = true;
        this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonOK.Enabled = false;
        this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOK.Location = new System.Drawing.Point(615, 13);
        this.ButtonOK.Name = "ButtonOK";
        this.ButtonOK.Size = new System.Drawing.Size(80, 29);
        this.ButtonOK.TabIndex = 0;
        this.ButtonOK.Text = "&OK";
        this.ButtonOK.UseVisualStyleBackColor = false;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(701, 13);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(80, 29);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        // 
        // FilePrimarySprite
        // 
        this.FilePrimarySprite.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FilePrimarySprite.AutoSize = true;
        this.FilePrimarySprite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.FilePrimarySprite.FileColumnText = "Primary sprite";
        this.FilePrimarySprite.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FilePrimarySprite.ForeColor = System.Drawing.Color.White;
        this.FilePrimarySprite.Location = new System.Drawing.Point(3, 0);
        this.FilePrimarySprite.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.FilePrimarySprite.MinimumSize = new System.Drawing.Size(320, 240);
        this.FilePrimarySprite.MultiSelect = false;
        this.FilePrimarySprite.Name = "FilePrimarySprite";
        this.FilePrimarySprite.Size = new System.Drawing.Size(386, 313);
        this.FilePrimarySprite.TabIndex = 0;
        this.FilePrimarySprite.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.FilePrimarySprite_Search);
        this.FilePrimarySprite.FileEntrySelected += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntrySelectedEventArgs>(this.FilePrimarySprite_FileEntrySelected);
        this.FilePrimarySprite.FileEntryUnselected += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntrySelectedEventArgs>(this.FilePrimarySprite_FileEntryUnselected);
        // 
        // PictureSpritePreview
        // 
        this.PictureSpritePreview.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.PictureSpritePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.PictureSpritePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.PictureSpritePreview.Location = new System.Drawing.Point(132, 319);
        this.PictureSpritePreview.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
        this.PictureSpritePreview.MaximumSize = new System.Drawing.Size(128, 128);
        this.PictureSpritePreview.MinimumSize = new System.Drawing.Size(128, 128);
        this.PictureSpritePreview.Name = "PictureSpritePreview";
        this.PictureSpritePreview.Size = new System.Drawing.Size(128, 128);
        this.PictureSpritePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.PictureSpritePreview.TabIndex = 9;
        this.PictureSpritePreview.TabStop = false;
        // 
        // TableTextureSelection
        // 
        this.TableTextureSelection.AutoSize = true;
        this.TableTextureSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableTextureSelection.ColumnCount = 2;
        this.TableTextureSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableTextureSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableTextureSelection.Controls.Add(this.FileBgTextures, 0, 0);
        this.TableTextureSelection.Controls.Add(this.FilePrimarySprite, 0, 0);
        this.TableTextureSelection.Controls.Add(this.PictureSpritePreview, 0, 1);
        this.TableTextureSelection.Controls.Add(this.PictureTexturePreview, 1, 1);
        this.TableTextureSelection.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableTextureSelection.Location = new System.Drawing.Point(0, 54);
        this.TableTextureSelection.Name = "TableTextureSelection";
        this.TableTextureSelection.RowCount = 2;
        this.TableTextureSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableTextureSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableTextureSelection.Size = new System.Drawing.Size(784, 453);
        this.TableTextureSelection.TabIndex = 1;
        // 
        // FileBgTextures
        // 
        this.FileBgTextures.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FileBgTextures.AutoSize = true;
        this.FileBgTextures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.FileBgTextures.FileColumnText = "Background texture";
        this.FileBgTextures.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FileBgTextures.ForeColor = System.Drawing.Color.White;
        this.FileBgTextures.Location = new System.Drawing.Point(395, 0);
        this.FileBgTextures.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.FileBgTextures.MinimumSize = new System.Drawing.Size(320, 240);
        this.FileBgTextures.MultiSelect = false;
        this.FileBgTextures.Name = "FileBgTextures";
        this.FileBgTextures.Size = new System.Drawing.Size(386, 313);
        this.FileBgTextures.TabIndex = 10;
        this.FileBgTextures.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.FileTextures_Search);
        this.FileBgTextures.FileEntrySelected += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntrySelectedEventArgs>(this.FileTextures_FileEntrySelected);
        this.FileBgTextures.FileEntryUnselected += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntrySelectedEventArgs>(this.FileTextures_FileEntryUnselected);
        // 
        // PictureTexturePreview
        // 
        this.PictureTexturePreview.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.PictureTexturePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.PictureTexturePreview.BackgroundImage = global::Gorgon.Editor.AnimationEditor.Properties.Resources.Transparency_Pattern;
        this.PictureTexturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.PictureTexturePreview.Location = new System.Drawing.Point(524, 319);
        this.PictureTexturePreview.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
        this.PictureTexturePreview.MaximumSize = new System.Drawing.Size(128, 128);
        this.PictureTexturePreview.MinimumSize = new System.Drawing.Size(128, 128);
        this.PictureTexturePreview.Name = "PictureTexturePreview";
        this.PictureTexturePreview.Size = new System.Drawing.Size(128, 128);
        this.PictureTexturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.PictureTexturePreview.TabIndex = 11;
        this.PictureTexturePreview.TabStop = false;
        // 
        // FormNewAnimation
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.Controls.Add(this.TableTextureSelection);
        this.Controls.Add(this.TableName);
        this.Controls.Add(this.PanelButtons);
        
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "FormNewAnimation";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "New Animation";
        this.TableName.ResumeLayout(false);
        this.TableName.PerformLayout();
        this.PanelName.ResumeLayout(false);
        this.PanelName.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericLength)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericFps)).EndInit();
        this.panel1.ResumeLayout(false);
        this.PanelButtons.ResumeLayout(false);
        this.PanelButtons.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PictureSpritePreview)).EndInit();
        this.TableTextureSelection.ResumeLayout(false);
        this.TableTextureSelection.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.PictureTexturePreview)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private Gorgon.UI.GorgonCueTextBox TextName;
    private System.Windows.Forms.TableLayoutPanel TableName;
    private System.Windows.Forms.Panel PanelName;
    private System.Windows.Forms.Panel PanelUnderline;
    private System.Windows.Forms.Label LabelLength;
    private System.Windows.Forms.Label LabelFps;
    private System.Windows.Forms.NumericUpDown NumericLength;
    private System.Windows.Forms.NumericUpDown NumericFps;
    private System.Windows.Forms.Panel PanelButtons;
    private System.Windows.Forms.Button ButtonOK;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.TableLayoutPanel TableTextureSelection;
    private System.Windows.Forms.PictureBox PictureSpritePreview;
    private UI.Controls.ContentFileExplorer FilePrimarySprite;
    private UI.Controls.ContentFileExplorer FileBgTextures;
    private System.Windows.Forms.PictureBox PictureTexturePreview;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label LabelFrameCount;
}
