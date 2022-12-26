namespace Gorgon.Editor.TextureAtlasTool;

partial class FormSpriteSelector
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
            ShutdownGraphics();
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSpriteSelector));
        this.TableFileSelection = new System.Windows.Forms.TableLayoutPanel();
        this.panel1 = new System.Windows.Forms.Panel();
        this.ContentFileExplorer = new Gorgon.Editor.UI.Controls.ContentFileExplorer();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.ButtonLoad = new System.Windows.Forms.Button();
        this.panel2 = new System.Windows.Forms.Panel();
        this.ButtonLabelMultiSprite = new System.Windows.Forms.Button();
        this.SplitFileSelector = new System.Windows.Forms.SplitContainer();
        this.PanelPreviewRender = new System.Windows.Forms.Panel();
        this.PanelSpritePreview = new System.Windows.Forms.Panel();
        this.LabelPreview = new System.Windows.Forms.Label();
        this.PanelDialogControls = new System.Windows.Forms.Panel();
        this.TableFileSelection.SuspendLayout();
        this.panel1.SuspendLayout();
        this.panel2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SplitFileSelector)).BeginInit();
        this.SplitFileSelector.Panel1.SuspendLayout();
        this.SplitFileSelector.Panel2.SuspendLayout();
        this.SplitFileSelector.SuspendLayout();
        this.PanelSpritePreview.SuspendLayout();
        this.PanelDialogControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // TableFileSelection
        // 
        this.TableFileSelection.ColumnCount = 2;
        this.TableFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableFileSelection.Controls.Add(this.panel1, 0, 0);
        this.TableFileSelection.Controls.Add(this.panel2, 0, 1);
        this.TableFileSelection.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableFileSelection.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
        this.TableFileSelection.Location = new System.Drawing.Point(0, 0);
        this.TableFileSelection.Name = "TableFileSelection";
        this.TableFileSelection.RowCount = 2;
        this.TableFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableFileSelection.Size = new System.Drawing.Size(512, 518);
        this.TableFileSelection.TabIndex = 0;
        // 
        // panel1
        // 
        this.panel1.AutoSize = true;
        this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableFileSelection.SetColumnSpan(this.panel1, 2);
        this.panel1.Controls.Add(this.ContentFileExplorer);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel1.Location = new System.Drawing.Point(3, 0);
        this.panel1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(506, 493);
        this.panel1.TabIndex = 0;
        // 
        // ContentFileExplorer
        // 
        this.ContentFileExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.ContentFileExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ContentFileExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.ContentFileExplorer.ForeColor = System.Drawing.Color.White;
        this.ContentFileExplorer.Location = new System.Drawing.Point(0, 0);
        this.ContentFileExplorer.Margin = new System.Windows.Forms.Padding(0);
        this.ContentFileExplorer.Name = "ContentFileExplorer";
        this.ContentFileExplorer.Size = new System.Drawing.Size(506, 493);
        this.ContentFileExplorer.TabIndex = 0;
        this.ContentFileExplorer.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.ContentFileExplorer_Search);
        this.ContentFileExplorer.FileEntriesFocused += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntriesFocusedArgs>(this.ContentFileExplorer_FileEntriesFocused);
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(666, 6);
        this.ButtonCancel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(106, 31);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
        // 
        // ButtonLoad
        // 
        this.ButtonLoad.AutoSize = true;
        this.ButtonLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonLoad.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonLoad.Enabled = false;
        this.ButtonLoad.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonLoad.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonLoad.Location = new System.Drawing.Point(554, 6);
        this.ButtonLoad.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
        this.ButtonLoad.Name = "ButtonLoad";
        this.ButtonLoad.Size = new System.Drawing.Size(106, 31);
        this.ButtonLoad.TabIndex = 0;
        this.ButtonLoad.Text = "&Load";
        this.ButtonLoad.UseVisualStyleBackColor = false;
        this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
        // 
        // panel2
        // 
        this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableFileSelection.SetColumnSpan(this.panel2, 2);
        this.panel2.Controls.Add(this.ButtonLabelMultiSprite);
        this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.panel2.Location = new System.Drawing.Point(0, 493);
        this.panel2.Margin = new System.Windows.Forms.Padding(0);
        this.panel2.Name = "panel2";
        this.panel2.Size = new System.Drawing.Size(512, 25);
        this.panel2.TabIndex = 1;
        // 
        // ButtonLabelMultiSprite
        // 
        this.ButtonLabelMultiSprite.AutoSize = true;
        this.ButtonLabelMultiSprite.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonLabelMultiSprite.Dock = System.Windows.Forms.DockStyle.Right;
        this.ButtonLabelMultiSprite.FlatAppearance.BorderSize = 0;
        this.ButtonLabelMultiSprite.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.ButtonLabelMultiSprite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.ButtonLabelMultiSprite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonLabelMultiSprite.Image = global::Gorgon.Editor.TextureAtlasTool.Properties.Resources.info_16x16;
        this.ButtonLabelMultiSprite.Location = new System.Drawing.Point(289, 0);
        this.ButtonLabelMultiSprite.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.ButtonLabelMultiSprite.Name = "ButtonLabelMultiSprite";
        this.ButtonLabelMultiSprite.Size = new System.Drawing.Size(223, 25);
        this.ButtonLabelMultiSprite.TabIndex = 0;
        this.ButtonLabelMultiSprite.TabStop = false;
        this.ButtonLabelMultiSprite.Text = "More than 1 sprite must be selected.";
        this.ButtonLabelMultiSprite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.ButtonLabelMultiSprite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.ButtonLabelMultiSprite.UseMnemonic = false;
        this.ButtonLabelMultiSprite.UseVisualStyleBackColor = true;
        // 
        // SplitFileSelector
        // 
        this.SplitFileSelector.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SplitFileSelector.Location = new System.Drawing.Point(0, 0);
        this.SplitFileSelector.Name = "SplitFileSelector";
        // 
        // SplitFileSelector.Panel1
        // 
        this.SplitFileSelector.Panel1.Controls.Add(this.TableFileSelection);
        this.SplitFileSelector.Panel1MinSize = 512;
        // 
        // SplitFileSelector.Panel2
        // 
        this.SplitFileSelector.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.SplitFileSelector.Panel2.Controls.Add(this.PanelPreviewRender);
        this.SplitFileSelector.Panel2.Controls.Add(this.PanelSpritePreview);
        this.SplitFileSelector.Panel2MinSize = 128;
        this.SplitFileSelector.Size = new System.Drawing.Size(784, 518);
        this.SplitFileSelector.SplitterDistance = 512;
        this.SplitFileSelector.TabIndex = 0;
        // 
        // PanelPreviewRender
        // 
        this.PanelPreviewRender.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelPreviewRender.Location = new System.Drawing.Point(0, 20);
        this.PanelPreviewRender.Name = "PanelPreviewRender";
        this.PanelPreviewRender.Size = new System.Drawing.Size(268, 498);
        this.PanelPreviewRender.TabIndex = 1;
        // 
        // PanelSpritePreview
        // 
        this.PanelSpritePreview.AutoSize = true;
        this.PanelSpritePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.PanelSpritePreview.Controls.Add(this.LabelPreview);
        this.PanelSpritePreview.Dock = System.Windows.Forms.DockStyle.Top;
        this.PanelSpritePreview.Location = new System.Drawing.Point(0, 0);
        this.PanelSpritePreview.Name = "PanelSpritePreview";
        this.PanelSpritePreview.Size = new System.Drawing.Size(268, 20);
        this.PanelSpritePreview.TabIndex = 0;
        // 
        // LabelPreview
        // 
        this.LabelPreview.AutoSize = true;
        this.LabelPreview.Dock = System.Windows.Forms.DockStyle.Top;
        this.LabelPreview.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.LabelPreview.Location = new System.Drawing.Point(0, 0);
        this.LabelPreview.Name = "LabelPreview";
        this.LabelPreview.Size = new System.Drawing.Size(60, 20);
        this.LabelPreview.TabIndex = 0;
        this.LabelPreview.Text = "Preview";
        // 
        // PanelDialogControls
        // 
        this.PanelDialogControls.AutoSize = true;
        this.PanelDialogControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelDialogControls.Controls.Add(this.ButtonCancel);
        this.PanelDialogControls.Controls.Add(this.ButtonLoad);
        this.PanelDialogControls.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.PanelDialogControls.Location = new System.Drawing.Point(0, 518);
        this.PanelDialogControls.Name = "PanelDialogControls";
        this.PanelDialogControls.Size = new System.Drawing.Size(784, 43);
        this.PanelDialogControls.TabIndex = 1;
        // 
        // FormSpriteSelector
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.Controls.Add(this.SplitFileSelector);
        this.Controls.Add(this.PanelDialogControls);
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ForeColor = System.Drawing.Color.White;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "FormSpriteSelector";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Load Sprites";
        this.TableFileSelection.ResumeLayout(false);
        this.TableFileSelection.PerformLayout();
        this.panel1.ResumeLayout(false);
        this.panel2.ResumeLayout(false);
        this.panel2.PerformLayout();
        this.SplitFileSelector.Panel1.ResumeLayout(false);
        this.SplitFileSelector.Panel2.ResumeLayout(false);
        this.SplitFileSelector.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SplitFileSelector)).EndInit();
        this.SplitFileSelector.ResumeLayout(false);
        this.PanelSpritePreview.ResumeLayout(false);
        this.PanelSpritePreview.PerformLayout();
        this.PanelDialogControls.ResumeLayout(false);
        this.PanelDialogControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel TableFileSelection;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.Button ButtonLoad;
    private UI.Controls.ContentFileExplorer ContentFileExplorer;
    private System.Windows.Forms.Button ButtonLabelMultiSprite;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.SplitContainer SplitFileSelector;
    private System.Windows.Forms.Panel PanelSpritePreview;
    private System.Windows.Forms.Label LabelPreview;
    private System.Windows.Forms.Panel PanelPreviewRender;
    private System.Windows.Forms.Panel PanelDialogControls;
}