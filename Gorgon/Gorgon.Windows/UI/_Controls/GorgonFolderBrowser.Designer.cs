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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GorgonFolderBrowser));
        this.LabelHeader = new System.Windows.Forms.Label();
        this.ListDirectories = new System.Windows.Forms.ListView();
        this.ColumnDirectoryName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.ColumnDirectoryDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.ColumnSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.Images = new System.Windows.Forms.ImageList(this.components);
        this.PanelCaption = new System.Windows.Forms.Panel();
        this.ButtonAddDir = new System.Windows.Forms.Button();
        this.ButtonDeleteDir = new System.Windows.Forms.Button();
        this.PanelDirectories = new System.Windows.Forms.Panel();
        this.PanelError = new System.Windows.Forms.Panel();
        this.LabelError = new System.Windows.Forms.Label();
        this.panel5 = new System.Windows.Forms.Panel();
        this.ButtonClose = new System.Windows.Forms.Button();
        this.LabelErrorIcon = new System.Windows.Forms.Label();
        this.PanelDirectoryName = new System.Windows.Forms.Panel();
        this.PanelBackground = new System.Windows.Forms.Panel();
        this.ButtonDirUp = new System.Windows.Forms.Button();
        this.TextDirectory = new System.Windows.Forms.TextBox();
        this.Tip = new System.Windows.Forms.ToolTip(this.components);
        this.DefaultImages = new System.Windows.Forms.ImageList(this.components);
        this.PanelCaption.SuspendLayout();
        this.PanelDirectories.SuspendLayout();
        this.PanelError.SuspendLayout();
        this.panel5.SuspendLayout();
        this.PanelDirectoryName.SuspendLayout();
        this.PanelBackground.SuspendLayout();
        this.SuspendLayout();
        // 
        // LabelHeader
        // 
        resources.ApplyResources(this.LabelHeader, "LabelHeader");
        this.LabelHeader.Name = "LabelHeader";
        // 
        // ListDirectories
        // 
        this.ListDirectories.BackColor = System.Drawing.Color.White;
        this.ListDirectories.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.ListDirectories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.ColumnDirectoryName,
        this.ColumnDirectoryDate,
        this.ColumnSize});
        resources.ApplyResources(this.ListDirectories, "ListDirectories");
        this.ListDirectories.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.ListDirectories.FullRowSelect = true;
        this.ListDirectories.HideSelection = false;
        this.ListDirectories.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
        ((System.Windows.Forms.ListViewItem)(resources.GetObject("ListDirectories.Items"))),
        ((System.Windows.Forms.ListViewItem)(resources.GetObject("ListDirectories.Items1"))),
        ((System.Windows.Forms.ListViewItem)(resources.GetObject("ListDirectories.Items2"))),
        ((System.Windows.Forms.ListViewItem)(resources.GetObject("ListDirectories.Items3")))});
        this.ListDirectories.LabelEdit = true;
        this.ListDirectories.LargeImageList = this.Images;
        this.ListDirectories.MultiSelect = false;
        this.ListDirectories.Name = "ListDirectories";
        this.ListDirectories.SmallImageList = this.Images;
        this.ListDirectories.UseCompatibleStateImageBehavior = false;
        this.ListDirectories.View = System.Windows.Forms.View.Details;
        this.ListDirectories.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.ListDirectories_AfterLabelEdit);
        this.ListDirectories.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.ListDirectories_BeforeLabelEdit);
        this.ListDirectories.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListDirectories_ColumnClick);
        this.ListDirectories.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ListDirectories_ItemSelectionChanged);
        this.ListDirectories.DoubleClick += new System.EventHandler(this.ListDirectories_DoubleClick);
        this.ListDirectories.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListDirectories_KeyDown);
        this.ListDirectories.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ListDirectories_KeyUp);
        this.ListDirectories.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListDirectories_MouseUp);
        // 
        // ColumnDirectoryName
        // 
        resources.ApplyResources(this.ColumnDirectoryName, "ColumnDirectoryName");
        // 
        // ColumnDirectoryDate
        // 
        resources.ApplyResources(this.ColumnDirectoryDate, "ColumnDirectoryDate");
        // 
        // ColumnSize
        // 
        resources.ApplyResources(this.ColumnSize, "ColumnSize");
        // 
        // Images
        // 
        this.Images.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
        resources.ApplyResources(this.Images, "Images");
        this.Images.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // PanelCaption
        // 
        resources.ApplyResources(this.PanelCaption, "PanelCaption");
        this.PanelCaption.Controls.Add(this.ButtonAddDir);
        this.PanelCaption.Controls.Add(this.ButtonDeleteDir);
        this.PanelCaption.Controls.Add(this.LabelHeader);
        this.PanelCaption.Name = "PanelCaption";
        // 
        // ButtonAddDir
        // 
        resources.ApplyResources(this.ButtonAddDir, "ButtonAddDir");
        this.ButtonAddDir.BackColor = System.Drawing.Color.Transparent;
        this.ButtonAddDir.FlatAppearance.BorderSize = 0;
        this.ButtonAddDir.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DodgerBlue;
        this.ButtonAddDir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonAddDir.Image = global::Gorgon.Windows.Properties.Resources.add_24x24;
        this.ButtonAddDir.Name = "ButtonAddDir";
        this.Tip.SetToolTip(this.ButtonAddDir, resources.GetString("ButtonAddDir.ToolTip"));
        this.ButtonAddDir.UseVisualStyleBackColor = false;
        this.ButtonAddDir.Click += new System.EventHandler(this.ButtonAddDir_Click);
        // 
        // ButtonDeleteDir
        // 
        resources.ApplyResources(this.ButtonDeleteDir, "ButtonDeleteDir");
        this.ButtonDeleteDir.BackColor = System.Drawing.Color.Transparent;
        this.ButtonDeleteDir.FlatAppearance.BorderSize = 0;
        this.ButtonDeleteDir.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DodgerBlue;
        this.ButtonDeleteDir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonDeleteDir.Image = global::Gorgon.Windows.Properties.Resources.remove_24x24;
        this.ButtonDeleteDir.Name = "ButtonDeleteDir";
        this.Tip.SetToolTip(this.ButtonDeleteDir, resources.GetString("ButtonDeleteDir.ToolTip"));
        this.ButtonDeleteDir.UseVisualStyleBackColor = false;
        this.ButtonDeleteDir.Click += new System.EventHandler(this.ButtonDeleteDir_Click);
        // 
        // PanelDirectories
        // 
        this.PanelDirectories.Controls.Add(this.PanelError);
        this.PanelDirectories.Controls.Add(this.ListDirectories);
        resources.ApplyResources(this.PanelDirectories, "PanelDirectories");
        this.PanelDirectories.Name = "PanelDirectories";
        // 
        // PanelError
        // 
        resources.ApplyResources(this.PanelError, "PanelError");
        this.PanelError.BackColor = System.Drawing.SystemColors.Info;
        this.PanelError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.PanelError.Controls.Add(this.LabelError);
        this.PanelError.Controls.Add(this.panel5);
        this.PanelError.Controls.Add(this.LabelErrorIcon);
        this.PanelError.ForeColor = System.Drawing.SystemColors.InfoText;
        this.PanelError.Name = "PanelError";
        // 
        // LabelError
        // 
        this.LabelError.AutoEllipsis = true;
        resources.ApplyResources(this.LabelError, "LabelError");
        this.LabelError.BackColor = System.Drawing.SystemColors.Info;
        this.LabelError.ForeColor = System.Drawing.SystemColors.InfoText;
        this.LabelError.Name = "LabelError";
        // 
        // panel5
        // 
        this.panel5.Controls.Add(this.ButtonClose);
        resources.ApplyResources(this.panel5, "panel5");
        this.panel5.Name = "panel5";
        // 
        // ButtonClose
        // 
        this.ButtonClose.BackColor = System.Drawing.SystemColors.Info;
        resources.ApplyResources(this.ButtonClose, "ButtonClose");
        this.ButtonClose.FlatAppearance.BorderSize = 0;
        this.ButtonClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Maroon;
        this.ButtonClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
        this.ButtonClose.ForeColor = System.Drawing.Color.Black;
        this.ButtonClose.Name = "ButtonClose";
        this.Tip.SetToolTip(this.ButtonClose, resources.GetString("ButtonClose.ToolTip"));
        this.ButtonClose.UseVisualStyleBackColor = false;
        this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
        // 
        // LabelErrorIcon
        // 
        this.LabelErrorIcon.BackColor = System.Drawing.SystemColors.Info;
        resources.ApplyResources(this.LabelErrorIcon, "LabelErrorIcon");
        this.LabelErrorIcon.Image = global::Gorgon.Windows.Properties.Resources.error_16x16;
        this.LabelErrorIcon.Name = "LabelErrorIcon";
        // 
        // PanelDirectoryName
        // 
        resources.ApplyResources(this.PanelDirectoryName, "PanelDirectoryName");
        this.PanelDirectoryName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.PanelDirectoryName.Controls.Add(this.PanelBackground);
        this.PanelDirectoryName.Name = "PanelDirectoryName";
        // 
        // PanelBackground
        // 
        resources.ApplyResources(this.PanelBackground, "PanelBackground");
        this.PanelBackground.BackColor = System.Drawing.Color.White;
        this.PanelBackground.Controls.Add(this.ButtonDirUp);
        this.PanelBackground.Controls.Add(this.TextDirectory);
        this.PanelBackground.Name = "PanelBackground";
        // 
        // ButtonDirUp
        // 
        this.ButtonDirUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
        resources.ApplyResources(this.ButtonDirUp, "ButtonDirUp");
        this.ButtonDirUp.FlatAppearance.BorderSize = 0;
        this.ButtonDirUp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DodgerBlue;
        this.ButtonDirUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonDirUp.Image = global::Gorgon.Windows.Properties.Resources.up_directory_16x16;
        this.ButtonDirUp.Name = "ButtonDirUp";
        this.Tip.SetToolTip(this.ButtonDirUp, resources.GetString("ButtonDirUp.ToolTip"));
        this.ButtonDirUp.UseVisualStyleBackColor = false;
        this.ButtonDirUp.Click += new System.EventHandler(this.ButtonDirUp_Click);
        // 
        // TextDirectory
        // 
        resources.ApplyResources(this.TextDirectory, "TextDirectory");
        this.TextDirectory.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.TextDirectory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.TextDirectory.Name = "TextDirectory";
        this.TextDirectory.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextDirectory_KeyUp);
        // 
        // DefaultImages
        // 
        this.DefaultImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
        resources.ApplyResources(this.DefaultImages, "DefaultImages");
        this.DefaultImages.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // GorgonFolderBrowser
        // 
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
        this.Controls.Add(this.PanelDirectories);
        this.Controls.Add(this.PanelDirectoryName);
        this.Controls.Add(this.PanelCaption);
        this.Name = "GorgonFolderBrowser";
        this.PanelCaption.ResumeLayout(false);
        this.PanelCaption.PerformLayout();
        this.PanelDirectories.ResumeLayout(false);
        this.PanelDirectories.PerformLayout();
        this.PanelError.ResumeLayout(false);
        this.PanelError.PerformLayout();
        this.panel5.ResumeLayout(false);
        this.PanelDirectoryName.ResumeLayout(false);
        this.PanelDirectoryName.PerformLayout();
        this.PanelBackground.ResumeLayout(false);
        this.PanelBackground.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

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
