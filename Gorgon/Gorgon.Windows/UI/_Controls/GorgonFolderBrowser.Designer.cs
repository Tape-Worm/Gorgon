namespace Gorgon.UI;

partial class GorgonFolderBrowser
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] { "a" }, "folder_documents_48x48.png", System.Drawing.Color.FromArgb(64, 64, 64), System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 9F));
        var listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] { "b" }, "folder_documents_48x48.png", System.Drawing.Color.FromArgb(64, 64, 64), System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 9F));
        var listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] { "c" }, "folder_documents_48x48.png", System.Drawing.Color.FromArgb(64, 64, 64), System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 9F));
        var listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] { "d" }, "folder_documents_48x48.png", System.Drawing.Color.FromArgb(64, 64, 64), System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 9F));
        LabelHeader = new System.Windows.Forms.Label();
        ListDirectories = new System.Windows.Forms.ListView();
        ColumnDirectoryName = new System.Windows.Forms.ColumnHeader();
        ColumnDirectoryDate = new System.Windows.Forms.ColumnHeader();
        ColumnSize = new System.Windows.Forms.ColumnHeader();
        Images = new System.Windows.Forms.ImageList(components);
        PanelCaption = new System.Windows.Forms.Panel();
        ButtonAddDir = new System.Windows.Forms.Button();
        ButtonDeleteDir = new System.Windows.Forms.Button();
        PanelDirectories = new System.Windows.Forms.Panel();
        PanelError = new System.Windows.Forms.Panel();
        LabelError = new System.Windows.Forms.Label();
        panel5 = new System.Windows.Forms.Panel();
        ButtonClose = new System.Windows.Forms.Button();
        LabelErrorIcon = new System.Windows.Forms.Label();
        PanelDirectoryName = new System.Windows.Forms.Panel();
        PanelBackground = new System.Windows.Forms.Panel();
        ButtonDirUp = new System.Windows.Forms.Button();
        TextDirectory = new System.Windows.Forms.TextBox();
        Tip = new System.Windows.Forms.ToolTip(components);
        DefaultImages = new System.Windows.Forms.ImageList(components);
        PanelCaption.SuspendLayout();
        PanelDirectories.SuspendLayout();
        PanelError.SuspendLayout();
        panel5.SuspendLayout();
        PanelDirectoryName.SuspendLayout();
        PanelBackground.SuspendLayout();
        SuspendLayout();
        // 
        // LabelHeader
        // 
        LabelHeader.AutoSize = true;
        LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
        LabelHeader.Location = new System.Drawing.Point(0, 0);
        LabelHeader.Name = "LabelHeader";
        LabelHeader.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
        LabelHeader.Size = new System.Drawing.Size(84, 25);
        LabelHeader.TabIndex = 0;
        LabelHeader.Text = "Select a folder.";
        // 
        // ListDirectories
        // 
        ListDirectories.BackColor = System.Drawing.Color.White;
        ListDirectories.BorderStyle = System.Windows.Forms.BorderStyle.None;
        ListDirectories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { ColumnDirectoryName, ColumnDirectoryDate, ColumnSize });
        ListDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
        ListDirectories.Font = new System.Drawing.Font("Segoe UI", 9F);
        ListDirectories.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
        ListDirectories.FullRowSelect = true;
        ListDirectories.Items.AddRange(new System.Windows.Forms.ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4 });
        ListDirectories.LabelEdit = true;
        ListDirectories.LabelWrap = false;
        ListDirectories.LargeImageList = Images;
        ListDirectories.Location = new System.Drawing.Point(0, 0);
        ListDirectories.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        ListDirectories.MultiSelect = false;
        ListDirectories.Name = "ListDirectories";
        ListDirectories.Size = new System.Drawing.Size(640, 371);
        ListDirectories.SmallImageList = Images;
        ListDirectories.TabIndex = 1;
        ListDirectories.UseCompatibleStateImageBehavior = false;
        ListDirectories.View = System.Windows.Forms.View.Details;
        ListDirectories.AfterLabelEdit += ListDirectories_AfterLabelEdit;
        ListDirectories.BeforeLabelEdit += ListDirectories_BeforeLabelEdit;
        ListDirectories.ColumnClick += ListDirectories_ColumnClick;
        ListDirectories.ItemSelectionChanged += ListDirectories_ItemSelectionChanged;
        ListDirectories.DoubleClick += ListDirectories_DoubleClick;
        ListDirectories.KeyDown += ListDirectories_KeyDown;
        ListDirectories.KeyUp += ListDirectories_KeyUp;
        ListDirectories.MouseUp += ListDirectories_MouseUp;
        // 
        // ColumnDirectoryName
        // 
        ColumnDirectoryName.Text = "Name";
        ColumnDirectoryName.Width = 180;
        // 
        // ColumnDirectoryDate
        // 
        ColumnDirectoryDate.Text = "Date";
        // 
        // ColumnSize
        // 
        ColumnSize.Text = "Size";
        // 
        // Images
        // 
        Images.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
        Images.ImageSize = new System.Drawing.Size(48, 48);
        Images.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // PanelCaption
        // 
        PanelCaption.AutoSize = true;
        PanelCaption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        PanelCaption.Controls.Add(ButtonAddDir);
        PanelCaption.Controls.Add(ButtonDeleteDir);
        PanelCaption.Controls.Add(LabelHeader);
        PanelCaption.Dock = System.Windows.Forms.DockStyle.Top;
        PanelCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
        PanelCaption.Location = new System.Drawing.Point(0, 0);
        PanelCaption.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        PanelCaption.Name = "PanelCaption";
        PanelCaption.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
        PanelCaption.Size = new System.Drawing.Size(640, 30);
        PanelCaption.TabIndex = 2;
        // 
        // ButtonAddDir
        // 
        ButtonAddDir.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        ButtonAddDir.BackColor = System.Drawing.Color.Transparent;
        ButtonAddDir.FlatAppearance.BorderSize = 0;
        ButtonAddDir.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DodgerBlue;
        ButtonAddDir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        ButtonAddDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        ButtonAddDir.Image = Windows.Properties.Resources.add_24x24;
        ButtonAddDir.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        ButtonAddDir.Location = new System.Drawing.Point(588, 0);
        ButtonAddDir.Name = "ButtonAddDir";
        ButtonAddDir.Size = new System.Drawing.Size(26, 26);
        ButtonAddDir.TabIndex = 5;
        Tip.SetToolTip(ButtonAddDir, "Add a new directory");
        ButtonAddDir.UseVisualStyleBackColor = false;
        ButtonAddDir.Click += ButtonAddDir_Click;
        // 
        // ButtonDeleteDir
        // 
        ButtonDeleteDir.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        ButtonDeleteDir.BackColor = System.Drawing.Color.Transparent;
        ButtonDeleteDir.FlatAppearance.BorderSize = 0;
        ButtonDeleteDir.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DodgerBlue;
        ButtonDeleteDir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        ButtonDeleteDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        ButtonDeleteDir.Image = Windows.Properties.Resources.remove_24x24;
        ButtonDeleteDir.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        ButtonDeleteDir.Location = new System.Drawing.Point(614, 0);
        ButtonDeleteDir.Name = "ButtonDeleteDir";
        ButtonDeleteDir.Size = new System.Drawing.Size(26, 26);
        ButtonDeleteDir.TabIndex = 4;
        Tip.SetToolTip(ButtonDeleteDir, "Delete the selected directory.");
        ButtonDeleteDir.UseVisualStyleBackColor = false;
        ButtonDeleteDir.Click += ButtonDeleteDir_Click;
        // 
        // PanelDirectories
        // 
        PanelDirectories.Controls.Add(PanelError);
        PanelDirectories.Controls.Add(ListDirectories);
        PanelDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
        PanelDirectories.Location = new System.Drawing.Point(0, 54);
        PanelDirectories.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        PanelDirectories.Name = "PanelDirectories";
        PanelDirectories.Size = new System.Drawing.Size(640, 371);
        PanelDirectories.TabIndex = 3;
        // 
        // PanelError
        // 
        PanelError.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        PanelError.AutoSize = true;
        PanelError.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        PanelError.BackColor = System.Drawing.SystemColors.Info;
        PanelError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        PanelError.Controls.Add(LabelError);
        PanelError.Controls.Add(panel5);
        PanelError.Controls.Add(LabelErrorIcon);
        PanelError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        PanelError.ForeColor = System.Drawing.SystemColors.InfoText;
        PanelError.Location = new System.Drawing.Point(334, 4);
        PanelError.Margin = new System.Windows.Forms.Padding(2);
        PanelError.MaximumSize = new System.Drawing.Size(281, 80);
        PanelError.MinimumSize = new System.Drawing.Size(281, 40);
        PanelError.Name = "PanelError";
        PanelError.Size = new System.Drawing.Size(281, 40);
        PanelError.TabIndex = 3;
        PanelError.Visible = false;
        // 
        // LabelError
        // 
        LabelError.AutoEllipsis = true;
        LabelError.AutoSize = true;
        LabelError.BackColor = System.Drawing.SystemColors.Info;
        LabelError.Dock = System.Windows.Forms.DockStyle.Fill;
        LabelError.ForeColor = System.Drawing.SystemColors.InfoText;
        LabelError.Location = new System.Drawing.Point(19, 0);
        LabelError.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        LabelError.MaximumSize = new System.Drawing.Size(245, 79);
        LabelError.Name = "LabelError";
        LabelError.Size = new System.Drawing.Size(0, 15);
        LabelError.TabIndex = 2;
        LabelError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // panel5
        // 
        panel5.Controls.Add(ButtonClose);
        panel5.Dock = System.Windows.Forms.DockStyle.Right;
        panel5.Location = new System.Drawing.Point(260, 0);
        panel5.Margin = new System.Windows.Forms.Padding(2);
        panel5.Name = "panel5";
        panel5.Size = new System.Drawing.Size(19, 38);
        panel5.TabIndex = 6;
        // 
        // ButtonClose
        // 
        ButtonClose.BackColor = System.Drawing.SystemColors.Info;
        ButtonClose.Dock = System.Windows.Forms.DockStyle.Top;
        ButtonClose.FlatAppearance.BorderSize = 0;
        ButtonClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Maroon;
        ButtonClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
        ButtonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        ButtonClose.Font = new System.Drawing.Font("Marlett", 9F);
        ButtonClose.ForeColor = System.Drawing.Color.Black;
        ButtonClose.ImeMode = System.Windows.Forms.ImeMode.NoControl;
        ButtonClose.Location = new System.Drawing.Point(0, 0);
        ButtonClose.Margin = new System.Windows.Forms.Padding(0);
        ButtonClose.Name = "ButtonClose";
        ButtonClose.Size = new System.Drawing.Size(19, 19);
        ButtonClose.TabIndex = 4;
        ButtonClose.Text = "r";
        Tip.SetToolTip(ButtonClose, "Close the error message.");
        ButtonClose.UseVisualStyleBackColor = false;
        ButtonClose.Click += ButtonClose_Click;
        // 
        // LabelErrorIcon
        // 
        LabelErrorIcon.BackColor = System.Drawing.SystemColors.Info;
        LabelErrorIcon.Dock = System.Windows.Forms.DockStyle.Left;
        LabelErrorIcon.Image = Windows.Properties.Resources.error_16x16;
        LabelErrorIcon.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
        LabelErrorIcon.Location = new System.Drawing.Point(0, 0);
        LabelErrorIcon.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        LabelErrorIcon.Name = "LabelErrorIcon";
        LabelErrorIcon.Size = new System.Drawing.Size(19, 38);
        LabelErrorIcon.TabIndex = 5;
        // 
        // PanelDirectoryName
        // 
        PanelDirectoryName.AutoSize = true;
        PanelDirectoryName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        PanelDirectoryName.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
        PanelDirectoryName.Controls.Add(PanelBackground);
        PanelDirectoryName.Dock = System.Windows.Forms.DockStyle.Top;
        PanelDirectoryName.Location = new System.Drawing.Point(0, 30);
        PanelDirectoryName.Name = "PanelDirectoryName";
        PanelDirectoryName.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
        PanelDirectoryName.Size = new System.Drawing.Size(640, 24);
        PanelDirectoryName.TabIndex = 4;
        // 
        // PanelBackground
        // 
        PanelBackground.AutoSize = true;
        PanelBackground.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        PanelBackground.BackColor = System.Drawing.Color.White;
        PanelBackground.Controls.Add(ButtonDirUp);
        PanelBackground.Controls.Add(TextDirectory);
        PanelBackground.Dock = System.Windows.Forms.DockStyle.Top;
        PanelBackground.Location = new System.Drawing.Point(0, 1);
        PanelBackground.Name = "PanelBackground";
        PanelBackground.Size = new System.Drawing.Size(640, 22);
        PanelBackground.TabIndex = 4;
        // 
        // ButtonDirUp
        // 
        ButtonDirUp.BackColor = System.Drawing.Color.FromArgb(204, 204, 204);
        ButtonDirUp.Dock = System.Windows.Forms.DockStyle.Right;
        ButtonDirUp.FlatAppearance.BorderSize = 0;
        ButtonDirUp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DodgerBlue;
        ButtonDirUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        ButtonDirUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        ButtonDirUp.Image = Windows.Properties.Resources.up_directory_16x16;
        ButtonDirUp.Location = new System.Drawing.Point(614, 0);
        ButtonDirUp.Name = "ButtonDirUp";
        ButtonDirUp.Size = new System.Drawing.Size(26, 22);
        ButtonDirUp.TabIndex = 3;
        Tip.SetToolTip(ButtonDirUp, "Go up one directory level.");
        ButtonDirUp.UseVisualStyleBackColor = false;
        ButtonDirUp.Click += ButtonDirUp_Click;
        // 
        // TextDirectory
        // 
        TextDirectory.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        TextDirectory.BorderStyle = System.Windows.Forms.BorderStyle.None;
        TextDirectory.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
        TextDirectory.Location = new System.Drawing.Point(0, 0);
        TextDirectory.Margin = new System.Windows.Forms.Padding(3, 3, 0, 6);
        TextDirectory.Name = "TextDirectory";
        TextDirectory.Size = new System.Drawing.Size(615, 16);
        TextDirectory.TabIndex = 2;
        TextDirectory.KeyUp += TextDirectory_KeyUp;
        // 
        // DefaultImages
        // 
        DefaultImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
        DefaultImages.ImageSize = new System.Drawing.Size(48, 48);
        DefaultImages.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // GorgonFolderBrowser
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.FromArgb(204, 204, 204);
        Controls.Add(PanelDirectories);
        Controls.Add(PanelDirectoryName);
        Controls.Add(PanelCaption);
        Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        Name = "GorgonFolderBrowser";
        Size = new System.Drawing.Size(640, 425);
        PanelCaption.ResumeLayout(false);
        PanelCaption.PerformLayout();
        PanelDirectories.ResumeLayout(false);
        PanelDirectories.PerformLayout();
        PanelError.ResumeLayout(false);
        PanelError.PerformLayout();
        panel5.ResumeLayout(false);
        PanelDirectoryName.ResumeLayout(false);
        PanelDirectoryName.PerformLayout();
        PanelBackground.ResumeLayout(false);
        PanelBackground.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label LabelHeader;
    private System.Windows.Forms.ListView ListDirectories;
    private System.Windows.Forms.Panel PanelCaption;
    private System.Windows.Forms.Panel PanelDirectoryName;
    private System.Windows.Forms.TextBox TextDirectory;
    private System.Windows.Forms.Panel PanelDirectories;
    private System.Windows.Forms.Button ButtonDirUp;
    private System.Windows.Forms.ImageList Images;
    private System.Windows.Forms.ColumnHeader ColumnDirectoryName;
    private System.Windows.Forms.ColumnHeader ColumnDirectoryDate;
    private System.Windows.Forms.Panel PanelError;
    private System.Windows.Forms.Button ButtonClose;
    private System.Windows.Forms.Label LabelError;
    private System.Windows.Forms.Label LabelErrorIcon;
    private System.Windows.Forms.ToolTip Tip;
    private System.Windows.Forms.ImageList DefaultImages;
    private System.Windows.Forms.Panel panel5;
    private System.Windows.Forms.Button ButtonDeleteDir;
    private System.Windows.Forms.Button ButtonAddDir;
    private System.Windows.Forms.Panel PanelBackground;
    private System.Windows.Forms.ColumnHeader ColumnSize;
}
