namespace Gorgon.Editor.ImageSplitTool;

partial class FormImageSelector
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

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImageSelector));
        this.panel1 = new System.Windows.Forms.Panel();
        this.ContentFileExplorer = new Gorgon.Editor.UI.Controls.ContentFileExplorer();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.ButtonSplit = new System.Windows.Forms.Button();
        this.SplitFileSelector = new System.Windows.Forms.SplitContainer();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.PanelPreviewRender = new System.Windows.Forms.Panel();
        this.PanelSpritePreview = new System.Windows.Forms.Panel();
        this.LabelPreview = new System.Windows.Forms.Label();
        this.TableOutput = new System.Windows.Forms.TableLayoutPanel();
        this.ButtonFolderBrowse = new System.Windows.Forms.Button();
        this.label6 = new System.Windows.Forms.Label();
        this.TextOutputFolder = new System.Windows.Forms.TextBox();
        this.PanelDialogControls = new System.Windows.Forms.Panel();
        this.LabelProcessing = new System.Windows.Forms.Label();
        this.panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SplitFileSelector)).BeginInit();
        this.SplitFileSelector.Panel1.SuspendLayout();
        this.SplitFileSelector.Panel2.SuspendLayout();
        this.SplitFileSelector.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        this.PanelSpritePreview.SuspendLayout();
        this.TableOutput.SuspendLayout();
        this.PanelDialogControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // panel1
        // 
        this.panel1.AutoSize = true;
        this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.panel1.Controls.Add(this.ContentFileExplorer);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel1.Location = new System.Drawing.Point(0, 0);
        this.panel1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(512, 486);
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
        this.ContentFileExplorer.Size = new System.Drawing.Size(512, 486);
        this.ContentFileExplorer.TabIndex = 0;
        this.ContentFileExplorer.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.ContentFileExplorer_Search);
        this.ContentFileExplorer.FileEntriesFocused += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntriesFocusedArgs>(this.ContentFileExplorer_FileEntriesFocused);
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
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
        // ButtonSplit
        // 
        this.ButtonSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonSplit.AutoSize = true;
        this.ButtonSplit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonSplit.Enabled = false;
        this.ButtonSplit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonSplit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonSplit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonSplit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonSplit.Location = new System.Drawing.Point(554, 6);
        this.ButtonSplit.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
        this.ButtonSplit.Name = "ButtonSplit";
        this.ButtonSplit.Size = new System.Drawing.Size(106, 31);
        this.ButtonSplit.TabIndex = 0;
        this.ButtonSplit.Text = "&Split";
        this.ButtonSplit.UseVisualStyleBackColor = false;
        this.ButtonSplit.Click += new System.EventHandler(this.ButtonSplit_Click);
        // 
        // SplitFileSelector
        // 
        this.SplitFileSelector.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SplitFileSelector.Location = new System.Drawing.Point(0, 0);
        this.SplitFileSelector.Name = "SplitFileSelector";
        // 
        // SplitFileSelector.Panel1
        // 
        this.SplitFileSelector.Panel1.Controls.Add(this.panel1);
        this.SplitFileSelector.Panel1MinSize = 512;
        // 
        // SplitFileSelector.Panel2
        // 
        this.SplitFileSelector.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.SplitFileSelector.Panel2.Controls.Add(this.tableLayoutPanel1);
        this.SplitFileSelector.Panel2MinSize = 128;
        this.SplitFileSelector.Size = new System.Drawing.Size(784, 486);
        this.SplitFileSelector.SplitterDistance = 512;
        this.SplitFileSelector.TabIndex = 0;
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.ColumnCount = 1;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.Controls.Add(this.PanelPreviewRender, 0, 1);
        this.tableLayoutPanel1.Controls.Add(this.PanelSpritePreview, 0, 0);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 2;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.Size = new System.Drawing.Size(268, 486);
        this.tableLayoutPanel1.TabIndex = 0;
        // 
        // PanelPreviewRender
        // 
        this.PanelPreviewRender.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelPreviewRender.Location = new System.Drawing.Point(0, 20);
        this.PanelPreviewRender.Margin = new System.Windows.Forms.Padding(0);
        this.PanelPreviewRender.Name = "PanelPreviewRender";
        this.PanelPreviewRender.Size = new System.Drawing.Size(268, 466);
        this.PanelPreviewRender.TabIndex = 1;
        // 
        // PanelSpritePreview
        // 
        this.PanelSpritePreview.AutoSize = true;
        this.PanelSpritePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.PanelSpritePreview.Controls.Add(this.LabelPreview);
        this.PanelSpritePreview.Dock = System.Windows.Forms.DockStyle.Top;
        this.PanelSpritePreview.Location = new System.Drawing.Point(0, 0);
        this.PanelSpritePreview.Margin = new System.Windows.Forms.Padding(0);
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
        // TableOutput
        // 
        this.TableOutput.AutoSize = true;
        this.TableOutput.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableOutput.ColumnCount = 3;
        this.TableOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableOutput.Controls.Add(this.ButtonFolderBrowse, 2, 0);
        this.TableOutput.Controls.Add(this.label6, 0, 0);
        this.TableOutput.Controls.Add(this.TextOutputFolder, 1, 0);
        this.TableOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.TableOutput.Location = new System.Drawing.Point(0, 486);
        this.TableOutput.Name = "TableOutput";
        this.TableOutput.RowCount = 1;
        this.TableOutput.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOutput.Size = new System.Drawing.Size(784, 32);
        this.TableOutput.TabIndex = 1;
        // 
        // ButtonFolderBrowse
        // 
        this.ButtonFolderBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.ButtonFolderBrowse.AutoSize = true;
        this.ButtonFolderBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonFolderBrowse.FlatAppearance.BorderSize = 0;
        this.ButtonFolderBrowse.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonFolderBrowse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonFolderBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonFolderBrowse.Image = global::Gorgon.Editor.ImageSplitTool.Properties.Resources.folder_20x20;
        this.ButtonFolderBrowse.Location = new System.Drawing.Point(752, 3);
        this.ButtonFolderBrowse.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
        this.ButtonFolderBrowse.Name = "ButtonFolderBrowse";
        this.ButtonFolderBrowse.Size = new System.Drawing.Size(26, 26);
        this.ButtonFolderBrowse.TabIndex = 2;
        this.ButtonFolderBrowse.UseVisualStyleBackColor = true;
        this.ButtonFolderBrowse.Click += new System.EventHandler(this.ButtonFolderBrowse_Click);
        // 
        // label6
        // 
        this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.label6.AutoSize = true;
        this.label6.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.label6.Location = new System.Drawing.Point(3, 8);
        this.label6.Name = "label6";
        this.label6.Size = new System.Drawing.Size(176, 15);
        this.label6.TabIndex = 0;
        this.label6.Text = "Image and Sprite Output Folder:";
        this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // TextOutputFolder
        // 
        this.TextOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.TextOutputFolder.BackColor = System.Drawing.Color.White;
        this.TextOutputFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.TextOutputFolder.ForeColor = System.Drawing.Color.Black;
        this.TextOutputFolder.Location = new System.Drawing.Point(185, 4);
        this.TextOutputFolder.Name = "TextOutputFolder";
        this.TextOutputFolder.ReadOnly = true;
        this.TextOutputFolder.Size = new System.Drawing.Size(561, 23);
        this.TextOutputFolder.TabIndex = 1;
        this.TextOutputFolder.Text = "Folder Name";
        // 
        // PanelDialogControls
        // 
        this.PanelDialogControls.AutoSize = true;
        this.PanelDialogControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelDialogControls.Controls.Add(this.ButtonCancel);
        this.PanelDialogControls.Controls.Add(this.ButtonSplit);
        this.PanelDialogControls.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.PanelDialogControls.Location = new System.Drawing.Point(0, 518);
        this.PanelDialogControls.Name = "PanelDialogControls";
        this.PanelDialogControls.Size = new System.Drawing.Size(784, 43);
        this.PanelDialogControls.TabIndex = 2;
        // 
        // LabelProcessing
        // 
        this.LabelProcessing.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelProcessing.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.LabelProcessing.Location = new System.Drawing.Point(0, 0);
        this.LabelProcessing.Name = "LabelProcessing";
        this.LabelProcessing.Size = new System.Drawing.Size(784, 561);
        this.LabelProcessing.TabIndex = 0;
        this.LabelProcessing.Text = "Processing image \'{0}\'....";
        this.LabelProcessing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.LabelProcessing.Visible = false;
        // 
        // FormImageSelector
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.Controls.Add(this.SplitFileSelector);
        this.Controls.Add(this.TableOutput);
        this.Controls.Add(this.PanelDialogControls);
        this.Controls.Add(this.LabelProcessing);
        
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "FormImageSelector";
        this.RenderControl = this.PanelPreviewRender;
        this.Text = "Split Atlas Images";
        this.panel1.ResumeLayout(false);
        this.SplitFileSelector.Panel1.ResumeLayout(false);
        this.SplitFileSelector.Panel1.PerformLayout();
        this.SplitFileSelector.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitFileSelector)).EndInit();
        this.SplitFileSelector.ResumeLayout(false);
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        this.PanelSpritePreview.ResumeLayout(false);
        this.PanelSpritePreview.PerformLayout();
        this.TableOutput.ResumeLayout(false);
        this.TableOutput.PerformLayout();
        this.PanelDialogControls.ResumeLayout(false);
        this.PanelDialogControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.Button ButtonSplit;
    private UI.Controls.ContentFileExplorer ContentFileExplorer;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.SplitContainer SplitFileSelector;
    private System.Windows.Forms.Panel PanelSpritePreview;
    private System.Windows.Forms.Label LabelPreview;
    private System.Windows.Forms.Panel PanelPreviewRender;
    private System.Windows.Forms.Panel PanelDialogControls;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TableLayoutPanel TableOutput;
    private System.Windows.Forms.Button ButtonFolderBrowse;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox TextOutputFolder;
    private System.Windows.Forms.Label LabelProcessing;
}
