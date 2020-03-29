namespace Gorgon.Editor.SpriteEditor
{
    partial class FormNewSprite
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
                PicturePreview.Image = null;
                _previewImage?.Dispose();
                _cancelSource?.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewSprite));
            this.TableName = new System.Windows.Forms.TableLayoutPanel();
            this.PanelName = new System.Windows.Forms.Panel();
            this.TextName = new Gorgon.UI.GorgonCueTextBox();
            this.PanelUnderline = new System.Windows.Forms.Panel();
            this.LabelWidth = new System.Windows.Forms.Label();
            this.NumericWidth = new System.Windows.Forms.NumericUpDown();
            this.NumericHeight = new System.Windows.Forms.NumericUpDown();
            this.LabelHeight = new System.Windows.Forms.Label();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.FileTextures = new Gorgon.Editor.UI.Controls.ContentFileExplorer();
            this.PicturePreview = new System.Windows.Forms.PictureBox();
            this.TableTextureSelection = new System.Windows.Forms.TableLayoutPanel();
            this.TableName.SuspendLayout();
            this.PanelName.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHeight)).BeginInit();
            this.PanelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicturePreview)).BeginInit();
            this.TableTextureSelection.SuspendLayout();
            this.SuspendLayout();
            // 
            // TableName
            // 
            this.TableName.AutoSize = true;
            this.TableName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TableName.ColumnCount = 4;
            this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableName.Controls.Add(this.PanelName, 0, 0);
            this.TableName.Controls.Add(this.PanelUnderline, 0, 1);
            this.TableName.Controls.Add(this.LabelWidth, 0, 2);
            this.TableName.Controls.Add(this.NumericWidth, 1, 2);
            this.TableName.Controls.Add(this.NumericHeight, 3, 2);
            this.TableName.Controls.Add(this.LabelHeight, 2, 2);
            this.TableName.Dock = System.Windows.Forms.DockStyle.Top;
            this.TableName.Location = new System.Drawing.Point(0, 0);
            this.TableName.Margin = new System.Windows.Forms.Padding(0);
            this.TableName.Name = "TableName";
            this.TableName.RowCount = 3;
            this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
            this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableName.Size = new System.Drawing.Size(624, 54);
            this.TableName.TabIndex = 0;
            // 
            // PanelName
            // 
            this.PanelName.AutoSize = true;
            this.TableName.SetColumnSpan(this.PanelName, 4);
            this.PanelName.Controls.Add(this.TextName);
            this.PanelName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelName.Location = new System.Drawing.Point(3, 0);
            this.PanelName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.PanelName.Name = "PanelName";
            this.PanelName.Size = new System.Drawing.Size(618, 22);
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
            this.TextName.Size = new System.Drawing.Size(618, 22);
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
            this.TableName.SetColumnSpan(this.PanelUnderline, 4);
            this.PanelUnderline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelUnderline.Location = new System.Drawing.Point(3, 22);
            this.PanelUnderline.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.PanelUnderline.Name = "PanelUnderline";
            this.PanelUnderline.Size = new System.Drawing.Size(618, 3);
            this.PanelUnderline.TabIndex = 0;
            // 
            // LabelWidth
            // 
            this.LabelWidth.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelWidth.AutoSize = true;
            this.LabelWidth.Location = new System.Drawing.Point(3, 32);
            this.LabelWidth.Name = "LabelWidth";
            this.LabelWidth.Size = new System.Drawing.Size(42, 15);
            this.LabelWidth.TabIndex = 1;
            this.LabelWidth.Text = "Width:";
            this.LabelWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NumericWidth
            // 
            this.NumericWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericWidth.AutoSize = true;
            this.NumericWidth.BackColor = System.Drawing.Color.White;
            this.NumericWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericWidth.ForeColor = System.Drawing.Color.Black;
            this.NumericWidth.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.NumericWidth.Location = new System.Drawing.Point(51, 28);
            this.NumericWidth.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.NumericWidth.MaximumSize = new System.Drawing.Size(113, 0);
            this.NumericWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericWidth.MinimumSize = new System.Drawing.Size(110, 0);
            this.NumericWidth.Name = "NumericWidth";
            this.NumericWidth.Size = new System.Drawing.Size(110, 23);
            this.NumericWidth.TabIndex = 2;
            this.NumericWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericWidth.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // NumericHeight
            // 
            this.NumericHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericHeight.AutoSize = true;
            this.NumericHeight.BackColor = System.Drawing.Color.White;
            this.NumericHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericHeight.ForeColor = System.Drawing.Color.Black;
            this.NumericHeight.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.NumericHeight.Location = new System.Drawing.Point(219, 28);
            this.NumericHeight.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.NumericHeight.MaximumSize = new System.Drawing.Size(113, 0);
            this.NumericHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericHeight.MinimumSize = new System.Drawing.Size(110, 0);
            this.NumericHeight.Name = "NumericHeight";
            this.NumericHeight.Size = new System.Drawing.Size(113, 23);
            this.NumericHeight.TabIndex = 4;
            this.NumericHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericHeight.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // LabelHeight
            // 
            this.LabelHeight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelHeight.AutoSize = true;
            this.LabelHeight.Location = new System.Drawing.Point(167, 32);
            this.LabelHeight.Name = "LabelHeight";
            this.LabelHeight.Size = new System.Drawing.Size(46, 15);
            this.LabelHeight.TabIndex = 3;
            this.LabelHeight.Text = "Height:";
            this.LabelHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelButtons
            // 
            this.PanelButtons.AutoSize = true;
            this.PanelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.PanelButtons.Controls.Add(this.ButtonOK);
            this.PanelButtons.Controls.Add(this.ButtonCancel);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelButtons.Location = new System.Drawing.Point(0, 387);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(624, 54);
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
            this.ButtonOK.Location = new System.Drawing.Point(455, 13);
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
            this.ButtonCancel.Location = new System.Drawing.Point(541, 13);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(80, 29);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "&Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = false;
            // 
            // FileTextures
            // 
            this.FileTextures.AutoSize = true;
            this.FileTextures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FileTextures.FileColumnText = "Texture:";
            this.FileTextures.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FileTextures.ForeColor = System.Drawing.Color.White;
            this.FileTextures.Location = new System.Drawing.Point(3, 0);
            this.FileTextures.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.FileTextures.MinimumSize = new System.Drawing.Size(344, 327);
            this.FileTextures.MultiSelect = false;
            this.FileTextures.Name = "FileTextures";
            this.FileTextures.Size = new System.Drawing.Size(344, 327);
            this.FileTextures.TabIndex = 0;
            this.FileTextures.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.FileTextures_Search);
            this.FileTextures.FileEntrySelected += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntrySelectedEventArgs>(this.FileTextures_FileEntrySelected);
            this.FileTextures.FileEntryUnselected += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntrySelectedEventArgs>(this.FileTextures_FileEntryUnselected);
            // 
            // PicturePreview
            // 
            this.PicturePreview.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PicturePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.PicturePreview.BackgroundImage = global::Gorgon.Editor.SpriteEditor.Properties.Resources.Transparency_Pattern;
            this.PicturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PicturePreview.Location = new System.Drawing.Point(359, 38);
            this.PicturePreview.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.PicturePreview.MaximumSize = new System.Drawing.Size(256, 256);
            this.PicturePreview.MinimumSize = new System.Drawing.Size(256, 256);
            this.PicturePreview.Name = "PicturePreview";
            this.PicturePreview.Size = new System.Drawing.Size(256, 256);
            this.PicturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PicturePreview.TabIndex = 9;
            this.PicturePreview.TabStop = false;
            // 
            // TableTextureSelection
            // 
            this.TableTextureSelection.AutoSize = true;
            this.TableTextureSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TableTextureSelection.ColumnCount = 2;
            this.TableTextureSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableTextureSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableTextureSelection.Controls.Add(this.PicturePreview, 1, 0);
            this.TableTextureSelection.Controls.Add(this.FileTextures, 0, 0);
            this.TableTextureSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableTextureSelection.Location = new System.Drawing.Point(0, 54);
            this.TableTextureSelection.Name = "TableTextureSelection";
            this.TableTextureSelection.RowCount = 1;
            this.TableTextureSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTextureSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 333F));
            this.TableTextureSelection.Size = new System.Drawing.Size(624, 333);
            this.TableTextureSelection.TabIndex = 1;
            // 
            // FormNewSprite
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.TableTextureSelection);
            this.Controls.Add(this.TableName);
            this.Controls.Add(this.PanelButtons);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "FormNewSprite";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Sprite";
            this.TableName.ResumeLayout(false);
            this.TableName.PerformLayout();
            this.PanelName.ResumeLayout(false);
            this.PanelName.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHeight)).EndInit();
            this.PanelButtons.ResumeLayout(false);
            this.PanelButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicturePreview)).EndInit();
            this.TableTextureSelection.ResumeLayout(false);
            this.TableTextureSelection.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Gorgon.UI.GorgonCueTextBox TextName;
        private System.Windows.Forms.TableLayoutPanel TableName;
        private System.Windows.Forms.Panel PanelName;
        private System.Windows.Forms.Panel PanelUnderline;
        private System.Windows.Forms.Label LabelWidth;
        private System.Windows.Forms.Label LabelHeight;
        private System.Windows.Forms.NumericUpDown NumericWidth;
        private System.Windows.Forms.NumericUpDown NumericHeight;
        private System.Windows.Forms.Panel PanelButtons;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.TableLayoutPanel TableTextureSelection;
        private System.Windows.Forms.PictureBox PicturePreview;
        private UI.Controls.ContentFileExplorer FileTextures;
    }
}