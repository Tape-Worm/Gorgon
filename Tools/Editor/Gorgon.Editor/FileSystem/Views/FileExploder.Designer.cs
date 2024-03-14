namespace Gorgon.Editor.Views;

partial class FileExploder
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
        if (disposing)
        {
            IsRenamingChangedEvent = null;
            ControlContextChangedEvent = null;
            ViewModel?.Unload();
            UnassignEvents();

            _openFileFont?.Dispose();
        }

        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        var dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        var dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
        var dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
        TreeNodeIcons = new System.Windows.Forms.ImageList(components);
        SplitFileSystem = new System.Windows.Forms.SplitContainer();
        TreeDirectories = new TreeEx();
        MenuDirectory = new System.Windows.Forms.ContextMenuStrip(components);
        MenuItemExportDirectoryTo = new System.Windows.Forms.ToolStripMenuItem();
        MenuSepDirEdit = new System.Windows.Forms.ToolStripSeparator();
        MenuItemCutDirectory = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemCopyDirectory = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemPasteDirectory = new System.Windows.Forms.ToolStripMenuItem();
        MenuSepDirOrganize = new System.Windows.Forms.ToolStripSeparator();
        MenuItemDeleteDirectory = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemRenameDirectory = new System.Windows.Forms.ToolStripMenuItem();
        MenuSepDirNew = new System.Windows.Forms.ToolStripSeparator();
        MenuItemCreateDirectory = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemDirCreateContent = new System.Windows.Forms.ToolStripMenuItem();
        MenuSepExclude = new System.Windows.Forms.ToolStripSeparator();
        MenuItemExcludeFromPackfile = new System.Windows.Forms.ToolStripMenuItem();
        GridFiles = new DataGridViewEx();
        ColumnID = new System.Windows.Forms.DataGridViewTextBoxColumn();
        ColumnFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
        ColumnIcon = new System.Windows.Forms.DataGridViewImageColumn();
        ColumnFilename = new System.Windows.Forms.DataGridViewTextBoxColumn();
        ColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
        ColumnSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
        ColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
        ColumnDummy = new System.Windows.Forms.DataGridViewTextBoxColumn();
        MenuFiles = new System.Windows.Forms.ContextMenuStrip(components);
        MenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemFileCreateContent = new System.Windows.Forms.ToolStripMenuItem();
        MenuSepFileExport = new System.Windows.Forms.ToolStripSeparator();
        MenuItemExportFiles = new System.Windows.Forms.ToolStripMenuItem();
        MenuSepFileEdit = new System.Windows.Forms.ToolStripSeparator();
        MenuItemCutFiles = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemCopyFiles = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemPasteFiles = new System.Windows.Forms.ToolStripMenuItem();
        MenuSepFileOrganize = new System.Windows.Forms.ToolStripSeparator();
        MenuItemDeleteFiles = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemRenameFile = new System.Windows.Forms.ToolStripMenuItem();
        panel1 = new System.Windows.Forms.Panel();
        TextSearch = new Gorgon.UI.GorgonSearchBox();
        MenuCopyMove = new System.Windows.Forms.ContextMenuStrip(components);
        MenuItemCopyTo = new System.Windows.Forms.ToolStripMenuItem();
        MenuItemMoveTo = new System.Windows.Forms.ToolStripMenuItem();
        toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
        MenuItemCancel = new System.Windows.Forms.ToolStripMenuItem();
        ((System.ComponentModel.ISupportInitialize)SplitFileSystem).BeginInit();
        SplitFileSystem.Panel1.SuspendLayout();
        SplitFileSystem.Panel2.SuspendLayout();
        SplitFileSystem.SuspendLayout();
        MenuDirectory.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)GridFiles).BeginInit();
        MenuFiles.SuspendLayout();
        panel1.SuspendLayout();
        MenuCopyMove.SuspendLayout();
        SuspendLayout();
        // 
        // TreeNodeIcons
        // 
        TreeNodeIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
        TreeNodeIcons.ImageSize = new System.Drawing.Size(20, 20);
        TreeNodeIcons.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // SplitFileSystem
        // 
        SplitFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
        SplitFileSystem.ForeColor = System.Drawing.Color.White;
        SplitFileSystem.Location = new System.Drawing.Point(0, 27);
        SplitFileSystem.Name = "SplitFileSystem";
        SplitFileSystem.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // SplitFileSystem.Panel1
        // 
        SplitFileSystem.Panel1.Controls.Add(TreeDirectories);
        // 
        // SplitFileSystem.Panel2
        // 
        SplitFileSystem.Panel2.Controls.Add(GridFiles);
        SplitFileSystem.Size = new System.Drawing.Size(600, 441);
        SplitFileSystem.SplitterDistance = 145;
        SplitFileSystem.TabIndex = 0;
        SplitFileSystem.SplitterMoved += SplitFileSystem_SplitterMoved;
        // 
        // TreeDirectories
        // 
        TreeDirectories.AllowDrop = true;
        TreeDirectories.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
        TreeDirectories.BorderStyle = System.Windows.Forms.BorderStyle.None;
        TreeDirectories.ContextMenuStrip = MenuDirectory;
        TreeDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
        TreeDirectories.ForeColor = System.Drawing.Color.White;
        TreeDirectories.ImageIndex = 0;
        TreeDirectories.ImageList = TreeNodeIcons;
        TreeDirectories.Location = new System.Drawing.Point(0, 0);
        TreeDirectories.Name = "TreeDirectories";
        TreeDirectories.PathSeparator = "/";
        TreeDirectories.SelectedImageIndex = 0;
        TreeDirectories.Size = new System.Drawing.Size(600, 145);
        TreeDirectories.TabIndex = 0;
        TreeDirectories.EditCanceled += TreeDirectories_EditCanceled;
        TreeDirectories.ItemDrag += TreeDirectories_ItemDrag;
        TreeDirectories.DragDrop += TreeDirectories_DragDrop;
        TreeDirectories.DragEnter += TreeDirectories_DragEnter;
        TreeDirectories.DragOver += TreeDirectories_DragOver;
        TreeDirectories.DragLeave += TreeDirectories_DragLeave;
        TreeDirectories.Enter += TreeDirectories_Enter;
        TreeDirectories.KeyUp += TreeDirectories_KeyUp;
        TreeDirectories.Leave += TreeDirectories_Leave;
        // 
        // MenuDirectory
        // 
        MenuDirectory.BackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        MenuDirectory.Font = new System.Drawing.Font("Segoe UI", 9F);
        MenuDirectory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItemExportDirectoryTo, MenuSepDirEdit, MenuItemCutDirectory, MenuItemCopyDirectory, MenuItemPasteDirectory, MenuSepDirOrganize, MenuItemDeleteDirectory, MenuItemRenameDirectory, MenuSepDirNew, MenuItemCreateDirectory, MenuItemDirCreateContent, MenuSepExclude, MenuItemExcludeFromPackfile });
        MenuDirectory.Name = "MenuFileExplorer";
        MenuDirectory.Size = new System.Drawing.Size(212, 226);
        MenuDirectory.Opening += MenuDirectory_Opening;
        // 
        // MenuItemExportDirectoryTo
        // 
        MenuItemExportDirectoryTo.Name = "MenuItemExportDirectoryTo";
        MenuItemExportDirectoryTo.Size = new System.Drawing.Size(211, 22);
        MenuItemExportDirectoryTo.Text = "E&xport to...";
        MenuItemExportDirectoryTo.Click += MenuItemExportDirectoryTo_Click;
        // 
        // MenuSepDirEdit
        // 
        MenuSepDirEdit.Name = "MenuSepDirEdit";
        MenuSepDirEdit.Size = new System.Drawing.Size(208, 6);
        // 
        // MenuItemCutDirectory
        // 
        MenuItemCutDirectory.Name = "MenuItemCutDirectory";
        MenuItemCutDirectory.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
        MenuItemCutDirectory.Size = new System.Drawing.Size(211, 22);
        MenuItemCutDirectory.Text = "Cu&t";
        MenuItemCutDirectory.Click += MenuItemCutDirectory_Click;
        // 
        // MenuItemCopyDirectory
        // 
        MenuItemCopyDirectory.Name = "MenuItemCopyDirectory";
        MenuItemCopyDirectory.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
        MenuItemCopyDirectory.Size = new System.Drawing.Size(211, 22);
        MenuItemCopyDirectory.Text = "C&opy";
        MenuItemCopyDirectory.Click += MenuItemCopyDirectory_Click;
        // 
        // MenuItemPasteDirectory
        // 
        MenuItemPasteDirectory.Name = "MenuItemPasteDirectory";
        MenuItemPasteDirectory.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
        MenuItemPasteDirectory.Size = new System.Drawing.Size(211, 22);
        MenuItemPasteDirectory.Text = "&Paste";
        MenuItemPasteDirectory.Click += MenuItemPasteDirectory_Click;
        // 
        // MenuSepDirOrganize
        // 
        MenuSepDirOrganize.Name = "MenuSepDirOrganize";
        MenuSepDirOrganize.Size = new System.Drawing.Size(208, 6);
        // 
        // MenuItemDeleteDirectory
        // 
        MenuItemDeleteDirectory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        MenuItemDeleteDirectory.Name = "MenuItemDeleteDirectory";
        MenuItemDeleteDirectory.ShortcutKeyDisplayString = "Del";
        MenuItemDeleteDirectory.Size = new System.Drawing.Size(211, 22);
        MenuItemDeleteDirectory.Text = "&Delete...";
        MenuItemDeleteDirectory.Click += MenuItemDeleteDirectory_Click;
        // 
        // MenuItemRenameDirectory
        // 
        MenuItemRenameDirectory.Name = "MenuItemRenameDirectory";
        MenuItemRenameDirectory.ShortcutKeys = System.Windows.Forms.Keys.F2;
        MenuItemRenameDirectory.Size = new System.Drawing.Size(211, 22);
        MenuItemRenameDirectory.Text = "&Rename...";
        MenuItemRenameDirectory.Click += MenuItemRenameDirectory_Click;
        // 
        // MenuSepDirNew
        // 
        MenuSepDirNew.Name = "MenuSepDirNew";
        MenuSepDirNew.Size = new System.Drawing.Size(208, 6);
        // 
        // MenuItemCreateDirectory
        // 
        MenuItemCreateDirectory.Name = "MenuItemCreateDirectory";
        MenuItemCreateDirectory.Size = new System.Drawing.Size(211, 22);
        MenuItemCreateDirectory.Text = "&Create directory...";
        MenuItemCreateDirectory.Click += MenuItemCreateDirectory_Click;
        // 
        // MenuItemDirCreateContent
        // 
        MenuItemDirCreateContent.Name = "MenuItemDirCreateContent";
        MenuItemDirCreateContent.Size = new System.Drawing.Size(211, 22);
        MenuItemDirCreateContent.Text = "Create...";
        // 
        // MenuSepExclude
        // 
        MenuSepExclude.Name = "MenuSepExclude";
        MenuSepExclude.Size = new System.Drawing.Size(208, 6);
        // 
        // MenuItemExcludeFromPackfile
        // 
        MenuItemExcludeFromPackfile.CheckOnClick = true;
        MenuItemExcludeFromPackfile.Name = "MenuItemExcludeFromPackfile";
        MenuItemExcludeFromPackfile.Size = new System.Drawing.Size(211, 22);
        MenuItemExcludeFromPackfile.Text = "Excluded from packed file";
        MenuItemExcludeFromPackfile.Click += MenuItemExcludeFromPackfile_Click;
        // 
        // GridFiles
        // 
        GridFiles.AllowDrop = true;
        GridFiles.AllowUserToAddRows = false;
        GridFiles.AllowUserToDeleteRows = false;
        GridFiles.AllowUserToResizeRows = false;
        GridFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        GridFiles.BackgroundColor = System.Drawing.Color.FromArgb(28, 28, 28);
        GridFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
        GridFiles.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
        GridFiles.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
        GridFiles.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        GridFiles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        GridFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        GridFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { ColumnID, ColumnFile, ColumnIcon, ColumnFilename, ColumnType, ColumnSize, ColumnPath, ColumnDummy });
        GridFiles.ContextMenuStrip = MenuFiles;
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
        dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        GridFiles.DefaultCellStyle = dataGridViewCellStyle2;
        GridFiles.Dock = System.Windows.Forms.DockStyle.Fill;
        GridFiles.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
        GridFiles.EnableHeadersVisualStyles = false;
        GridFiles.GridColor = System.Drawing.Color.FromArgb(28, 28, 28);
        GridFiles.Location = new System.Drawing.Point(0, 0);
        GridFiles.Name = "GridFiles";
        GridFiles.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
        dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        GridFiles.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
        GridFiles.RowHeadersVisible = false;
        GridFiles.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
        dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        GridFiles.RowsDefaultCellStyle = dataGridViewCellStyle4;
        GridFiles.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        GridFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        GridFiles.ShowCellErrors = false;
        GridFiles.ShowEditingIcon = false;
        GridFiles.ShowRowErrors = false;
        GridFiles.Size = new System.Drawing.Size(600, 292);
        GridFiles.StandardTab = true;
        GridFiles.TabIndex = 0;
        GridFiles.RowsDrag += GridFiles_RowsDrag;
        GridFiles.CellDoubleClick += GridFiles_CellDoubleClick;
        GridFiles.CellEndEdit += GridFiles_CellEndEdit;
        GridFiles.CellFormatting += GridFiles_CellFormatting;
        GridFiles.DragDrop += GridFiles_DragDrop;
        GridFiles.DragEnter += GridFiles_DragEnter;
        GridFiles.DragOver += GridFiles_DragOver;
        GridFiles.Enter += GridFiles_Enter;
        GridFiles.Leave += GridFiles_Leave;
        // 
        // ColumnID
        // 
        ColumnID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        ColumnID.DataPropertyName = "ID";
        ColumnID.HeaderText = "ID";
        ColumnID.Name = "ColumnID";
        ColumnID.Visible = false;
        // 
        // ColumnFile
        // 
        ColumnFile.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        ColumnFile.HeaderText = "File";
        ColumnFile.Name = "ColumnFile";
        ColumnFile.Visible = false;
        // 
        // ColumnIcon
        // 
        ColumnIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        ColumnIcon.DataPropertyName = "Image";
        ColumnIcon.HeaderText = "";
        ColumnIcon.Image = Properties.Resources.generic_file_20x20;
        ColumnIcon.Name = "ColumnIcon";
        ColumnIcon.ReadOnly = true;
        ColumnIcon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        ColumnIcon.Width = 20;
        // 
        // ColumnFilename
        // 
        ColumnFilename.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        ColumnFilename.DataPropertyName = "Filename";
        ColumnFilename.FillWeight = 80F;
        ColumnFilename.HeaderText = "Filename";
        ColumnFilename.MaxInputLength = 250;
        ColumnFilename.MinimumWidth = 100;
        ColumnFilename.Name = "ColumnFilename";
        ColumnFilename.Width = 300;
        // 
        // ColumnType
        // 
        ColumnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        ColumnType.DataPropertyName = "Type";
        ColumnType.FillWeight = 10F;
        ColumnType.HeaderText = "Type";
        ColumnType.Name = "ColumnType";
        ColumnType.ReadOnly = true;
        ColumnType.Width = 55;
        // 
        // ColumnSize
        // 
        ColumnSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        ColumnSize.DataPropertyName = "Size";
        ColumnSize.FillWeight = 10F;
        ColumnSize.HeaderText = "Size";
        ColumnSize.Name = "ColumnSize";
        ColumnSize.ReadOnly = true;
        ColumnSize.Width = 51;
        // 
        // ColumnPath
        // 
        ColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        ColumnPath.FillWeight = 10F;
        ColumnPath.HeaderText = "Directory";
        ColumnPath.Name = "ColumnPath";
        ColumnPath.ReadOnly = true;
        ColumnPath.Visible = false;
        // 
        // ColumnDummy
        // 
        ColumnDummy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        ColumnDummy.FillWeight = 1F;
        ColumnDummy.HeaderText = "";
        ColumnDummy.MinimumWidth = 2;
        ColumnDummy.Name = "ColumnDummy";
        ColumnDummy.ReadOnly = true;
        ColumnDummy.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        ColumnDummy.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
        // 
        // MenuFiles
        // 
        MenuFiles.BackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        MenuFiles.Font = new System.Drawing.Font("Segoe UI", 9F);
        MenuFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItemOpen, MenuItemFileCreateContent, MenuSepFileExport, MenuItemExportFiles, MenuSepFileEdit, MenuItemCutFiles, MenuItemCopyFiles, MenuItemPasteFiles, MenuSepFileOrganize, MenuItemDeleteFiles, MenuItemRenameFile });
        MenuFiles.Name = "MenuFileExplorer";
        MenuFiles.Size = new System.Drawing.Size(147, 198);
        MenuFiles.Opening += MenuFiles_Opening;
        // 
        // MenuItemOpen
        // 
        MenuItemOpen.Name = "MenuItemOpen";
        MenuItemOpen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
        MenuItemOpen.Size = new System.Drawing.Size(146, 22);
        MenuItemOpen.Text = "Open";
        MenuItemOpen.Click += MenuItemOpen_Click;
        // 
        // MenuItemFileCreateContent
        // 
        MenuItemFileCreateContent.Name = "MenuItemFileCreateContent";
        MenuItemFileCreateContent.Size = new System.Drawing.Size(146, 22);
        MenuItemFileCreateContent.Text = "Create...";
        // 
        // MenuSepFileExport
        // 
        MenuSepFileExport.Name = "MenuSepFileExport";
        MenuSepFileExport.Size = new System.Drawing.Size(143, 6);
        // 
        // MenuItemExportFiles
        // 
        MenuItemExportFiles.Name = "MenuItemExportFiles";
        MenuItemExportFiles.Size = new System.Drawing.Size(146, 22);
        MenuItemExportFiles.Text = "E&xport to...";
        MenuItemExportFiles.Click += MenuItemExportFiles_Click;
        // 
        // MenuSepFileEdit
        // 
        MenuSepFileEdit.Name = "MenuSepFileEdit";
        MenuSepFileEdit.Size = new System.Drawing.Size(143, 6);
        // 
        // MenuItemCutFiles
        // 
        MenuItemCutFiles.Name = "MenuItemCutFiles";
        MenuItemCutFiles.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
        MenuItemCutFiles.Size = new System.Drawing.Size(146, 22);
        MenuItemCutFiles.Text = "Cu&t";
        MenuItemCutFiles.Click += MenuItemCutFiles_Click;
        // 
        // MenuItemCopyFiles
        // 
        MenuItemCopyFiles.Name = "MenuItemCopyFiles";
        MenuItemCopyFiles.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
        MenuItemCopyFiles.Size = new System.Drawing.Size(146, 22);
        MenuItemCopyFiles.Text = "C&opy";
        MenuItemCopyFiles.Click += MenuItemCopyFiles_Click;
        // 
        // MenuItemPasteFiles
        // 
        MenuItemPasteFiles.Name = "MenuItemPasteFiles";
        MenuItemPasteFiles.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
        MenuItemPasteFiles.Size = new System.Drawing.Size(146, 22);
        MenuItemPasteFiles.Text = "&Paste";
        MenuItemPasteFiles.Click += MenuItemPasteFiles_Click;
        // 
        // MenuSepFileOrganize
        // 
        MenuSepFileOrganize.Name = "MenuSepFileOrganize";
        MenuSepFileOrganize.Size = new System.Drawing.Size(143, 6);
        // 
        // MenuItemDeleteFiles
        // 
        MenuItemDeleteFiles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        MenuItemDeleteFiles.Name = "MenuItemDeleteFiles";
        MenuItemDeleteFiles.ShortcutKeyDisplayString = "Del";
        MenuItemDeleteFiles.Size = new System.Drawing.Size(146, 22);
        MenuItemDeleteFiles.Text = "&Delete...";
        MenuItemDeleteFiles.Click += MenuItemDeleteFiles_Click;
        // 
        // MenuItemRenameFile
        // 
        MenuItemRenameFile.Name = "MenuItemRenameFile";
        MenuItemRenameFile.ShortcutKeys = System.Windows.Forms.Keys.F2;
        MenuItemRenameFile.Size = new System.Drawing.Size(146, 22);
        MenuItemRenameFile.Text = "&Rename...";
        MenuItemRenameFile.Click += MenuItemRenameFile_Click;
        // 
        // panel1
        // 
        panel1.AutoSize = true;
        panel1.BackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        panel1.Controls.Add(TextSearch);
        panel1.Dock = System.Windows.Forms.DockStyle.Top;
        panel1.Location = new System.Drawing.Point(0, 0);
        panel1.Name = "panel1";
        panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
        panel1.Size = new System.Drawing.Size(600, 27);
        panel1.TabIndex = 0;
        // 
        // TextSearch
        // 
        TextSearch.AutoSize = true;
        TextSearch.BackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        TextSearch.Dock = System.Windows.Forms.DockStyle.Top;
        TextSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
        TextSearch.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
        TextSearch.Location = new System.Drawing.Point(0, 0);
        TextSearch.Name = "TextSearch";
        TextSearch.Size = new System.Drawing.Size(600, 24);
        TextSearch.TabIndex = 1;
        TextSearch.ToolTipBackColor = System.Drawing.SystemColors.Info;
        TextSearch.ToolTipForeColor = System.Drawing.SystemColors.InfoText;
        TextSearch.ToolTipIcon = System.Windows.Forms.ToolTipIcon.None;
        TextSearch.ToolTipText = "";
        TextSearch.ToolTipTitle = "Search";
        TextSearch.Search += TextSearch_Search;
        TextSearch.Enter += TextSearch_Enter;
        TextSearch.KeyUp += TextSearch_KeyUp;
        // 
        // MenuCopyMove
        // 
        MenuCopyMove.Font = new System.Drawing.Font("Segoe UI", 9F);
        MenuCopyMove.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItemCopyTo, MenuItemMoveTo, toolStripSeparator3, MenuItemCancel });
        MenuCopyMove.Name = "MenuOptions";
        MenuCopyMove.Size = new System.Drawing.Size(136, 76);
        // 
        // MenuItemCopyTo
        // 
        MenuItemCopyTo.Name = "MenuItemCopyTo";
        MenuItemCopyTo.Size = new System.Drawing.Size(135, 22);
        MenuItemCopyTo.Text = "Copy here";
        MenuItemCopyTo.Click += MenuItemCopyTo_Click;
        // 
        // MenuItemMoveTo
        // 
        MenuItemMoveTo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        MenuItemMoveTo.Name = "MenuItemMoveTo";
        MenuItemMoveTo.Size = new System.Drawing.Size(135, 22);
        MenuItemMoveTo.Text = "Move here";
        MenuItemMoveTo.Click += MenuItemMoveTo_Click;
        // 
        // toolStripSeparator3
        // 
        toolStripSeparator3.Name = "toolStripSeparator3";
        toolStripSeparator3.Size = new System.Drawing.Size(132, 6);
        // 
        // MenuItemCancel
        // 
        MenuItemCancel.Name = "MenuItemCancel";
        MenuItemCancel.Size = new System.Drawing.Size(135, 22);
        MenuItemCancel.Text = "Cancel";
        // 
        // FileExploder
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        BackColor = System.Drawing.Color.FromArgb(35, 35, 37);
        Controls.Add(SplitFileSystem);
        Controls.Add(panel1);
        Name = "FileExploder";
        SplitFileSystem.Panel1.ResumeLayout(false);
        SplitFileSystem.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)SplitFileSystem).EndInit();
        SplitFileSystem.ResumeLayout(false);
        MenuDirectory.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)GridFiles).EndInit();
        MenuFiles.ResumeLayout(false);
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        MenuCopyMove.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }


    private TreeEx TreeDirectories;
    private System.Windows.Forms.ImageList TreeNodeIcons;
    private System.Windows.Forms.SplitContainer SplitFileSystem;
    private DataGridViewEx GridFiles;
    private System.Windows.Forms.ContextMenuStrip MenuCopyMove;
    private System.Windows.Forms.ToolStripMenuItem MenuItemCopyTo;
    private System.Windows.Forms.ToolStripMenuItem MenuItemMoveTo;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripMenuItem MenuItemCancel;
    private System.Windows.Forms.ToolStripMenuItem MenuItemOpen;
    private System.Windows.Forms.ToolStripSeparator MenuSepFileExport;
    private System.Windows.Forms.ToolStripMenuItem MenuItemExportFiles;
    private System.Windows.Forms.ToolStripSeparator MenuSepFileEdit;
    private System.Windows.Forms.ToolStripMenuItem MenuItemCutFiles;
    private System.Windows.Forms.ToolStripMenuItem MenuItemCopyFiles;
    private System.Windows.Forms.ToolStripSeparator MenuSepFileOrganize;
    private System.Windows.Forms.ToolStripMenuItem MenuItemDeleteFiles;
    private System.Windows.Forms.ToolStripMenuItem MenuItemRenameFile;
    private System.Windows.Forms.ToolStripMenuItem MenuItemExportDirectoryTo;
    private System.Windows.Forms.ToolStripSeparator MenuSepDirEdit;
    private System.Windows.Forms.ToolStripMenuItem MenuItemCutDirectory;
    private System.Windows.Forms.ToolStripMenuItem MenuItemCopyDirectory;
    private System.Windows.Forms.ToolStripMenuItem MenuItemPasteDirectory;
    private System.Windows.Forms.ToolStripSeparator MenuSepDirOrganize;
    private System.Windows.Forms.ToolStripMenuItem MenuItemDeleteDirectory;
    private System.Windows.Forms.ToolStripMenuItem MenuItemRenameDirectory;
    private System.Windows.Forms.ToolStripSeparator MenuSepDirNew;
    private System.Windows.Forms.ToolStripMenuItem MenuItemPasteFiles;
    private System.Windows.Forms.Panel panel1;
    private Gorgon.UI.GorgonSearchBox TextSearch;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnID;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFile;
    private System.Windows.Forms.DataGridViewImageColumn ColumnIcon;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFilename;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnType;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSize;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPath;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDummy;
    private System.Windows.Forms.ToolStripMenuItem MenuItemExcludeFromPackfile;
    private System.Windows.Forms.ToolStripSeparator MenuSepExclude;
    internal System.Windows.Forms.ContextMenuStrip MenuFiles;
    private System.Windows.Forms.ContextMenuStrip MenuDirectory;
    private System.Windows.Forms.ToolStripMenuItem MenuItemCreateDirectory;
    internal System.Windows.Forms.ToolStripMenuItem MenuItemDirCreateContent;
    internal System.Windows.Forms.ToolStripMenuItem MenuItemFileCreateContent;
}
