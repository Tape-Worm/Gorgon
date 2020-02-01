namespace Gorgon.Editor.Views
{
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
                DataContext?.OnUnload();
                UnassignEvents();

                _openFileFont?.Dispose();
            }

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileExploder));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.TreeNodeIcons = new System.Windows.Forms.ImageList(this.components);
            this.SplitFileSystem = new System.Windows.Forms.SplitContainer();
            this.TreeDirectories = new Gorgon.Editor.Views.TreeEx();
            this.MenuDirectory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemExportDirectoryTo = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSepDirEdit = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemCutDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemCopyDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemPasteDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSepDirOrganize = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemDeleteDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemRenameDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSepDirNew = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemCreateDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.GridFiles = new Gorgon.Editor.Views.DataGridViewEx();
            this.ColumnID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIcon = new System.Windows.Forms.DataGridViewImageColumn();
            this.ColumnFilename = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDummy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MenuFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSepFileExport = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemExportFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSepFileEdit = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemCutFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemCopyFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemPasteFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSepFileOrganize = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemDeleteFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemRenameFile = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TextSearch = new Gorgon.UI.GorgonSearchBox();
            this.MenuCopyMove = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemCopyTo = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemMoveTo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemCancel = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.SplitFileSystem)).BeginInit();
            this.SplitFileSystem.Panel1.SuspendLayout();
            this.SplitFileSystem.Panel2.SuspendLayout();
            this.SplitFileSystem.SuspendLayout();
            this.MenuDirectory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridFiles)).BeginInit();
            this.MenuFiles.SuspendLayout();
            this.panel1.SuspendLayout();
            this.MenuCopyMove.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeNodeIcons
            // 
            this.TreeNodeIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TreeNodeIcons.ImageStream")));
            this.TreeNodeIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.TreeNodeIcons.Images.SetKeyName(0, "folder_20x20.png");
            this.TreeNodeIcons.Images.SetKeyName(1, "generic_file_20x20.png");
            // 
            // SplitFileSystem
            // 
            this.SplitFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitFileSystem.ForeColor = System.Drawing.Color.White;
            this.SplitFileSystem.Location = new System.Drawing.Point(0, 27);
            this.SplitFileSystem.Name = "SplitFileSystem";
            this.SplitFileSystem.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitFileSystem.Panel1
            // 
            this.SplitFileSystem.Panel1.Controls.Add(this.TreeDirectories);
            this.SplitFileSystem.Panel1.TabIndex = 2;
            // 
            // SplitFileSystem.Panel2
            // 
            this.SplitFileSystem.Panel2.Controls.Add(this.GridFiles);
            this.SplitFileSystem.Panel2.TabIndex = 0;
            this.SplitFileSystem.Size = new System.Drawing.Size(600, 441);
            this.SplitFileSystem.SplitterDistance = 145;
            this.SplitFileSystem.TabIndex = 0;
            // 
            // TreeDirectories
            // 
            this.TreeDirectories.AllowDrop = true;
            this.TreeDirectories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.TreeDirectories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TreeDirectories.ContextMenuStrip = this.MenuDirectory;
            this.TreeDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeDirectories.ForeColor = System.Drawing.Color.White;
            this.TreeDirectories.ImageIndex = 0;
            this.TreeDirectories.ImageList = this.TreeNodeIcons;
            this.TreeDirectories.Location = new System.Drawing.Point(0, 0);
            this.TreeDirectories.Name = "TreeDirectories";
            this.TreeDirectories.PathSeparator = "/";
            this.TreeDirectories.SelectedImageIndex = 0;
            this.TreeDirectories.Size = new System.Drawing.Size(600, 145);
            this.TreeDirectories.TabIndex = 0;
            this.TreeDirectories.EditCanceled += new System.EventHandler(this.TreeDirectories_EditCanceled);
            this.TreeDirectories.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.TreeDirectories_ItemDrag);
            this.TreeDirectories.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeDirectories_DragDrop);
            this.TreeDirectories.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeDirectories_DragEnter);
            this.TreeDirectories.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeDirectories_DragOver);
            this.TreeDirectories.DragLeave += new System.EventHandler(this.TreeDirectories_DragLeave);
            this.TreeDirectories.Enter += new System.EventHandler(this.TreeDirectories_Enter);
            this.TreeDirectories.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TreeDirectories_KeyUp);
            this.TreeDirectories.Leave += new System.EventHandler(this.TreeDirectories_Leave);
            // 
            // MenuDirectory
            // 
            this.MenuDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.MenuDirectory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuDirectory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemExportDirectoryTo,
            this.MenuSepDirEdit,
            this.MenuItemCutDirectory,
            this.MenuItemCopyDirectory,
            this.MenuItemPasteDirectory,
            this.MenuSepDirOrganize,
            this.MenuItemDeleteDirectory,
            this.MenuItemRenameDirectory,
            this.MenuSepDirNew,
            this.MenuItemCreateDirectory});
            this.MenuDirectory.Name = "MenuFileExplorer";
            this.MenuDirectory.Size = new System.Drawing.Size(168, 176);
            this.MenuDirectory.Opening += new System.ComponentModel.CancelEventHandler(this.MenuDirectory_Opening);
            // 
            // MenuItemExportDirectoryTo
            // 
            this.MenuItemExportDirectoryTo.Name = "MenuItemExportDirectoryTo";
            this.MenuItemExportDirectoryTo.Size = new System.Drawing.Size(167, 22);
            this.MenuItemExportDirectoryTo.Text = "E&xport to...";
            this.MenuItemExportDirectoryTo.Click += new System.EventHandler(this.MenuItemExportDirectoryTo_Click);
            // 
            // MenuSepDirEdit
            // 
            this.MenuSepDirEdit.Name = "MenuSepDirEdit";
            this.MenuSepDirEdit.Size = new System.Drawing.Size(164, 6);
            // 
            // MenuItemCutDirectory
            // 
            this.MenuItemCutDirectory.Name = "MenuItemCutDirectory";
            this.MenuItemCutDirectory.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.MenuItemCutDirectory.Size = new System.Drawing.Size(167, 22);
            this.MenuItemCutDirectory.Text = "Cu&t";
            this.MenuItemCutDirectory.Click += new System.EventHandler(this.MenuItemCutDirectory_Click);
            // 
            // MenuItemCopyDirectory
            // 
            this.MenuItemCopyDirectory.Name = "MenuItemCopyDirectory";
            this.MenuItemCopyDirectory.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.MenuItemCopyDirectory.Size = new System.Drawing.Size(167, 22);
            this.MenuItemCopyDirectory.Text = "C&opy";
            this.MenuItemCopyDirectory.Click += new System.EventHandler(this.MenuItemCopyDirectory_Click);
            // 
            // MenuItemPasteDirectory
            // 
            this.MenuItemPasteDirectory.Name = "MenuItemPasteDirectory";
            this.MenuItemPasteDirectory.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.MenuItemPasteDirectory.Size = new System.Drawing.Size(167, 22);
            this.MenuItemPasteDirectory.Text = "&Paste";
            this.MenuItemPasteDirectory.Click += new System.EventHandler(this.MenuItemPasteDirectory_Click);
            // 
            // MenuSepDirOrganize
            // 
            this.MenuSepDirOrganize.Name = "MenuSepDirOrganize";
            this.MenuSepDirOrganize.Size = new System.Drawing.Size(164, 6);
            // 
            // MenuItemDeleteDirectory
            // 
            this.MenuItemDeleteDirectory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.MenuItemDeleteDirectory.Name = "MenuItemDeleteDirectory";
            this.MenuItemDeleteDirectory.ShortcutKeyDisplayString = "Del";
            this.MenuItemDeleteDirectory.Size = new System.Drawing.Size(167, 22);
            this.MenuItemDeleteDirectory.Text = "&Delete...";
            this.MenuItemDeleteDirectory.Click += new System.EventHandler(this.MenuItemDeleteDirectory_Click);
            // 
            // MenuItemRenameDirectory
            // 
            this.MenuItemRenameDirectory.Name = "MenuItemRenameDirectory";
            this.MenuItemRenameDirectory.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.MenuItemRenameDirectory.Size = new System.Drawing.Size(167, 22);
            this.MenuItemRenameDirectory.Text = "&Rename...";
            this.MenuItemRenameDirectory.Click += new System.EventHandler(this.MenuItemRenameDirectory_Click);
            // 
            // MenuSepDirNew
            // 
            this.MenuSepDirNew.Name = "MenuSepDirNew";
            this.MenuSepDirNew.Size = new System.Drawing.Size(164, 6);
            // 
            // MenuItemCreateDirectory
            // 
            this.MenuItemCreateDirectory.Name = "MenuItemCreateDirectory";
            this.MenuItemCreateDirectory.Size = new System.Drawing.Size(167, 22);
            this.MenuItemCreateDirectory.Text = "&Create directory...";
            this.MenuItemCreateDirectory.Click += new System.EventHandler(this.MenuItemCreateDirectory_Click);
            // 
            // GridFiles
            // 
            this.GridFiles.AllowDrop = true;
            this.GridFiles.AllowUserToAddRows = false;
            this.GridFiles.AllowUserToDeleteRows = false;
            this.GridFiles.AllowUserToResizeRows = false;
            this.GridFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.GridFiles.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.GridFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.GridFiles.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.GridFiles.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.GridFiles.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.GridFiles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.GridFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnID,
            this.ColumnFile,
            this.ColumnIcon,
            this.ColumnFilename,
            this.ColumnType,
            this.ColumnSize,
            this.ColumnPath,
            this.ColumnDummy});
            this.GridFiles.ContextMenuStrip = this.MenuFiles;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.SteelBlue;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.GridFiles.DefaultCellStyle = dataGridViewCellStyle2;
            this.GridFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridFiles.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.GridFiles.EnableHeadersVisualStyles = false;
            this.GridFiles.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.GridFiles.Location = new System.Drawing.Point(0, 0);
            this.GridFiles.Name = "GridFiles";
            this.GridFiles.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SteelBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.GridFiles.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.GridFiles.RowHeadersVisible = false;
            this.GridFiles.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.SteelBlue;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.GridFiles.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.GridFiles.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.GridFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.GridFiles.ShowCellErrors = false;
            this.GridFiles.ShowEditingIcon = false;
            this.GridFiles.ShowRowErrors = false;
            this.GridFiles.Size = new System.Drawing.Size(600, 292);
            this.GridFiles.StandardTab = true;
            this.GridFiles.TabIndex = 0;
            this.GridFiles.RowsDrag += new System.EventHandler<Gorgon.Editor.Views.RowsDragEventArgs>(this.GridFiles_RowsDrag);
            this.GridFiles.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridFiles_CellDoubleClick);
            this.GridFiles.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridFiles_CellEndEdit);
            this.GridFiles.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GridFiles_CellFormatting);
            this.GridFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridFiles_DragDrop);
            this.GridFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.GridFiles_DragEnter);
            this.GridFiles.DragOver += new System.Windows.Forms.DragEventHandler(this.GridFiles_DragOver);
            this.GridFiles.Enter += new System.EventHandler(this.GridFiles_Enter);
            this.GridFiles.Leave += new System.EventHandler(this.GridFiles_Leave);
            // 
            // ColumnID
            // 
            this.ColumnID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnID.DataPropertyName = "ID";
            this.ColumnID.HeaderText = "ID";
            this.ColumnID.Name = "ColumnID";
            this.ColumnID.Visible = false;
            // 
            // ColumnFile
            // 
            this.ColumnFile.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnFile.HeaderText = "File";
            this.ColumnFile.Name = "ColumnFile";
            this.ColumnFile.Visible = false;
            // 
            // ColumnIcon
            // 
            this.ColumnIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnIcon.DataPropertyName = "Image";
            this.ColumnIcon.HeaderText = "";
            this.ColumnIcon.Image = global::Gorgon.Editor.Properties.Resources.generic_file_20x20;
            this.ColumnIcon.Name = "ColumnIcon";
            this.ColumnIcon.ReadOnly = true;
            this.ColumnIcon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnIcon.Width = 20;
            // 
            // ColumnFilename
            // 
            this.ColumnFilename.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnFilename.DataPropertyName = "Filename";
            this.ColumnFilename.FillWeight = 80F;
            this.ColumnFilename.HeaderText = "Filename";
            this.ColumnFilename.MaxInputLength = 250;
            this.ColumnFilename.MinimumWidth = 100;
            this.ColumnFilename.Name = "ColumnFilename";
            this.ColumnFilename.Width = 300;
            // 
            // ColumnType
            // 
            this.ColumnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnType.DataPropertyName = "Type";
            this.ColumnType.FillWeight = 10F;
            this.ColumnType.HeaderText = "Type";
            this.ColumnType.Name = "ColumnType";
            this.ColumnType.ReadOnly = true;
            this.ColumnType.Width = 55;
            // 
            // ColumnSize
            // 
            this.ColumnSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnSize.DataPropertyName = "Size";
            this.ColumnSize.FillWeight = 10F;
            this.ColumnSize.HeaderText = "Size";
            this.ColumnSize.Name = "ColumnSize";
            this.ColumnSize.ReadOnly = true;
            this.ColumnSize.Width = 51;
            // 
            // ColumnPath
            // 
            this.ColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnPath.FillWeight = 10F;
            this.ColumnPath.HeaderText = "Directory";
            this.ColumnPath.Name = "ColumnPath";
            this.ColumnPath.ReadOnly = true;
            this.ColumnPath.Visible = false;
            // 
            // ColumnDummy
            // 
            this.ColumnDummy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnDummy.FillWeight = 1F;
            this.ColumnDummy.HeaderText = "";
            this.ColumnDummy.MinimumWidth = 2;
            this.ColumnDummy.Name = "ColumnDummy";
            this.ColumnDummy.ReadOnly = true;
            this.ColumnDummy.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnDummy.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // MenuFiles
            // 
            this.MenuFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.MenuFiles.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemOpen,
            this.MenuSepFileExport,
            this.MenuItemExportFiles,
            this.MenuSepFileEdit,
            this.MenuItemCutFiles,
            this.MenuItemCopyFiles,
            this.MenuItemPasteFiles,
            this.MenuSepFileOrganize,
            this.MenuItemDeleteFiles,
            this.MenuItemRenameFile});
            this.MenuFiles.Name = "MenuFileExplorer";
            this.MenuFiles.Size = new System.Drawing.Size(147, 176);
            this.MenuFiles.Opening += new System.ComponentModel.CancelEventHandler(this.MenuFiles_Opening);
            // 
            // MenuItemOpen
            // 
            this.MenuItemOpen.Name = "MenuItemOpen";
            this.MenuItemOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.MenuItemOpen.Size = new System.Drawing.Size(146, 22);
            this.MenuItemOpen.Text = "Open";
            this.MenuItemOpen.Click += new System.EventHandler(this.MenuItemOpen_Click);
            // 
            // MenuSepFileExport
            // 
            this.MenuSepFileExport.Name = "MenuSepFileExport";
            this.MenuSepFileExport.Size = new System.Drawing.Size(143, 6);
            // 
            // MenuItemExportFiles
            // 
            this.MenuItemExportFiles.Name = "MenuItemExportFiles";
            this.MenuItemExportFiles.Size = new System.Drawing.Size(146, 22);
            this.MenuItemExportFiles.Text = "E&xport to...";
            this.MenuItemExportFiles.Click += new System.EventHandler(this.MenuItemExportFiles_Click);
            // 
            // MenuSepFileEdit
            // 
            this.MenuSepFileEdit.Name = "MenuSepFileEdit";
            this.MenuSepFileEdit.Size = new System.Drawing.Size(143, 6);
            // 
            // MenuItemCutFiles
            // 
            this.MenuItemCutFiles.Name = "MenuItemCutFiles";
            this.MenuItemCutFiles.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.MenuItemCutFiles.Size = new System.Drawing.Size(146, 22);
            this.MenuItemCutFiles.Text = "Cu&t";
            this.MenuItemCutFiles.Click += new System.EventHandler(this.MenuItemCutFiles_Click);
            // 
            // MenuItemCopyFiles
            // 
            this.MenuItemCopyFiles.Name = "MenuItemCopyFiles";
            this.MenuItemCopyFiles.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.MenuItemCopyFiles.Size = new System.Drawing.Size(146, 22);
            this.MenuItemCopyFiles.Text = "C&opy";
            this.MenuItemCopyFiles.Click += new System.EventHandler(this.MenuItemCopyFiles_Click);
            // 
            // MenuItemPasteFiles
            // 
            this.MenuItemPasteFiles.Name = "MenuItemPasteFiles";
            this.MenuItemPasteFiles.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.MenuItemPasteFiles.Size = new System.Drawing.Size(146, 22);
            this.MenuItemPasteFiles.Text = "&Paste";
            this.MenuItemPasteFiles.Click += new System.EventHandler(this.MenuItemPasteFiles_Click);
            // 
            // MenuSepFileOrganize
            // 
            this.MenuSepFileOrganize.Name = "MenuSepFileOrganize";
            this.MenuSepFileOrganize.Size = new System.Drawing.Size(143, 6);
            // 
            // MenuItemDeleteFiles
            // 
            this.MenuItemDeleteFiles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.MenuItemDeleteFiles.Name = "MenuItemDeleteFiles";
            this.MenuItemDeleteFiles.ShortcutKeyDisplayString = "Del";
            this.MenuItemDeleteFiles.Size = new System.Drawing.Size(146, 22);
            this.MenuItemDeleteFiles.Text = "&Delete...";
            this.MenuItemDeleteFiles.Click += new System.EventHandler(this.MenuItemDeleteFiles_Click);
            // 
            // MenuItemRenameFile
            // 
            this.MenuItemRenameFile.Name = "MenuItemRenameFile";
            this.MenuItemRenameFile.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.MenuItemRenameFile.Size = new System.Drawing.Size(146, 22);
            this.MenuItemRenameFile.Text = "&Rename...";
            this.MenuItemRenameFile.Click += new System.EventHandler(this.MenuItemRenameFile_Click);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.panel1.Controls.Add(this.TextSearch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.panel1.Size = new System.Drawing.Size(600, 27);
            this.panel1.TabIndex = 0;
            // 
            // TextSearch
            // 
            this.TextSearch.AutoSize = true;
            this.TextSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TextSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.TextSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.TextSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.TextSearch.Location = new System.Drawing.Point(0, 0);
            this.TextSearch.Name = "TextSearch";
            this.TextSearch.Size = new System.Drawing.Size(600, 24);
            this.TextSearch.TabIndex = 1;
            this.TextSearch.ToolTipBackColor = System.Drawing.SystemColors.Info;
            this.TextSearch.ToolTipForeColor = System.Drawing.SystemColors.InfoText;
            this.TextSearch.ToolTipIcon = System.Windows.Forms.ToolTipIcon.None;
            this.TextSearch.ToolTipText = "";
            this.TextSearch.ToolTipTitle = "Search";
            this.TextSearch.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.TextSearch_Search);
            this.TextSearch.Enter += new System.EventHandler(this.TextSearch_Enter);
            this.TextSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextSearch_KeyUp);
            // 
            // MenuCopyMove
            // 
            this.MenuCopyMove.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuCopyMove.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemCopyTo,
            this.MenuItemMoveTo,
            this.toolStripSeparator3,
            this.MenuItemCancel});
            this.MenuCopyMove.Name = "MenuOptions";
            this.MenuCopyMove.Size = new System.Drawing.Size(136, 76);
            // 
            // MenuItemCopyTo
            // 
            this.MenuItemCopyTo.Name = "MenuItemCopyTo";
            this.MenuItemCopyTo.Size = new System.Drawing.Size(135, 22);
            this.MenuItemCopyTo.Text = "Copy here";
            this.MenuItemCopyTo.Click += new System.EventHandler(this.MenuItemCopyTo_Click);
            // 
            // MenuItemMoveTo
            // 
            this.MenuItemMoveTo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MenuItemMoveTo.Name = "MenuItemMoveTo";
            this.MenuItemMoveTo.Size = new System.Drawing.Size(135, 22);
            this.MenuItemMoveTo.Text = "Move here";
            this.MenuItemMoveTo.Click += new System.EventHandler(this.MenuItemMoveTo_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(132, 6);
            // 
            // MenuItemCancel
            // 
            this.MenuItemCancel.Name = "MenuItemCancel";
            this.MenuItemCancel.Size = new System.Drawing.Size(135, 22);
            this.MenuItemCancel.Text = "Cancel";
            // 
            // FileExploder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.Controls.Add(this.SplitFileSystem);
            this.Controls.Add(this.panel1);
            this.Name = "FileExploder";
            this.SplitFileSystem.Panel1.ResumeLayout(false);
            this.SplitFileSystem.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitFileSystem)).EndInit();
            this.SplitFileSystem.ResumeLayout(false);
            this.MenuDirectory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridFiles)).EndInit();
            this.MenuFiles.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.MenuCopyMove.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private TreeEx TreeDirectories;
        private System.Windows.Forms.ImageList TreeNodeIcons;
        private System.Windows.Forms.SplitContainer SplitFileSystem;
        private DataGridViewEx GridFiles;
        private System.Windows.Forms.ContextMenuStrip MenuCopyMove;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCopyTo;
        private System.Windows.Forms.ToolStripMenuItem MenuItemMoveTo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCancel;
        private System.Windows.Forms.ContextMenuStrip MenuFiles;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpen;
        private System.Windows.Forms.ToolStripSeparator MenuSepFileExport;
        private System.Windows.Forms.ToolStripMenuItem MenuItemExportFiles;
        private System.Windows.Forms.ToolStripSeparator MenuSepFileEdit;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCutFiles;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCopyFiles;
        private System.Windows.Forms.ToolStripSeparator MenuSepFileOrganize;
        private System.Windows.Forms.ToolStripMenuItem MenuItemDeleteFiles;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRenameFile;
        private System.Windows.Forms.ContextMenuStrip MenuDirectory;
        private System.Windows.Forms.ToolStripMenuItem MenuItemExportDirectoryTo;
        private System.Windows.Forms.ToolStripSeparator MenuSepDirEdit;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCutDirectory;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCopyDirectory;
        private System.Windows.Forms.ToolStripMenuItem MenuItemPasteDirectory;
        private System.Windows.Forms.ToolStripSeparator MenuSepDirOrganize;
        private System.Windows.Forms.ToolStripMenuItem MenuItemDeleteDirectory;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRenameDirectory;
        private System.Windows.Forms.ToolStripSeparator MenuSepDirNew;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCreateDirectory;
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
    }
}
