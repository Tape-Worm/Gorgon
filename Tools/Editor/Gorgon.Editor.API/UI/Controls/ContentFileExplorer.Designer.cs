namespace Gorgon.Editor.UI.Controls;

partial class ContentFileExplorer
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
            if (_checkBoxHeader != null)
            {
                _checkBoxHeader.Click -= CheckboxHeader_Click;
            }
            UnassignEvents();
            _entries = null;
            BuildIconCache();
            _dirFont.Dispose();
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
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContentFileExplorer));
        this.GridFiles = new System.Windows.Forms.DataGridView();
        this.ColumnSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this.ColumnDirName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColumnLocation = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColumnFullFilePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColumnDirectory = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.PanelSearch = new System.Windows.Forms.Panel();
        this.TextSearch = new Gorgon.UI.GorgonSearchBox();
        this.TipFullPath = new System.Windows.Forms.ToolTip(this.components);
        this.LabelNoFiles = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.GridFiles)).BeginInit();
        this.tableLayoutPanel1.SuspendLayout();
        this.PanelSearch.SuspendLayout();
        this.SuspendLayout();
        // 
        // GridFiles
        // 
        this.GridFiles.AllowUserToAddRows = false;
        this.GridFiles.AllowUserToDeleteRows = false;
        this.GridFiles.AllowUserToResizeRows = false;
        this.GridFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.GridFiles.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.GridFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.GridFiles.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
        this.GridFiles.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
        this.GridFiles.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
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
        this.ColumnSelected,
        this.ColumnDirName,
        this.ColumnLocation,
        this.ColumnFullFilePath,
        this.ColumnDirectory});
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this.GridFiles.DefaultCellStyle = dataGridViewCellStyle2;
        this.GridFiles.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
        this.GridFiles.EnableHeadersVisualStyles = false;
        this.GridFiles.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.GridFiles.Location = new System.Drawing.Point(3, 33);
        this.GridFiles.Name = "GridFiles";
        this.GridFiles.RowHeadersVisible = false;
        this.GridFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.GridFiles.ShowCellErrors = false;
        this.GridFiles.ShowCellToolTips = false;
        this.GridFiles.ShowEditingIcon = false;
        this.GridFiles.ShowRowErrors = false;
        this.GridFiles.Size = new System.Drawing.Size(739, 554);
        this.GridFiles.StandardTab = true;
        this.GridFiles.TabIndex = 1;
        this.GridFiles.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridFiles_CellMouseDoubleClick);
        this.GridFiles.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridFiles_CellMouseDown);
        this.GridFiles.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridFiles_CellMouseEnter);
        this.GridFiles.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridFiles_CellMouseLeave);
        this.GridFiles.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.GridFiles_CellPainting);
        this.GridFiles.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridFiles_ColumnHeaderMouseClick);
        this.GridFiles.SelectionChanged += new System.EventHandler(this.GridFiles_SelectionChanged);
        // 
        // ColumnSelected
        // 
        this.ColumnSelected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
        this.ColumnSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ColumnSelected.HeaderText = "";
        this.ColumnSelected.MinimumWidth = 32;
        this.ColumnSelected.Name = "ColumnSelected";
        this.ColumnSelected.ReadOnly = true;
        this.ColumnSelected.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.ColumnSelected.Width = 32;
        // 
        // ColumnDirName
        // 
        this.ColumnDirName.HeaderText = "DirName";
        this.ColumnDirName.Name = "ColumnDirName";
        this.ColumnDirName.ReadOnly = true;
        this.ColumnDirName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.ColumnDirName.Visible = false;
        this.ColumnDirName.Width = 58;
        // 
        // ColumnLocation
        // 
        this.ColumnLocation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.ColumnLocation.HeaderText = "File";
        this.ColumnLocation.Name = "ColumnLocation";
        this.ColumnLocation.ReadOnly = true;
        this.ColumnLocation.Resizable = System.Windows.Forms.DataGridViewTriState.True;
        this.ColumnLocation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // ColumnFullFilePath
        // 
        this.ColumnFullFilePath.HeaderText = "FullPath";
        this.ColumnFullFilePath.Name = "ColumnFullFilePath";
        this.ColumnFullFilePath.ReadOnly = true;
        this.ColumnFullFilePath.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.ColumnFullFilePath.Visible = false;
        this.ColumnFullFilePath.Width = 73;
        // 
        // ColumnDirectory
        // 
        this.ColumnDirectory.HeaderText = "IsDirectory";
        this.ColumnDirectory.Name = "ColumnDirectory";
        this.ColumnDirectory.ReadOnly = true;
        this.ColumnDirectory.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.ColumnDirectory.Visible = false;
        this.ColumnDirectory.Width = 67;
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.ColumnCount = 1;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.Controls.Add(this.PanelSearch, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.GridFiles, 0, 1);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 2;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(745, 590);
        this.tableLayoutPanel1.TabIndex = 1;
        // 
        // PanelSearch
        // 
        this.PanelSearch.AutoSize = true;
        this.PanelSearch.Controls.Add(this.TextSearch);
        this.PanelSearch.Dock = System.Windows.Forms.DockStyle.Top;
        this.PanelSearch.Location = new System.Drawing.Point(3, 3);
        this.PanelSearch.Name = "PanelSearch";
        this.PanelSearch.Size = new System.Drawing.Size(739, 24);
        this.PanelSearch.TabIndex = 2;
        // 
        // TextSearch
        // 
        this.TextSearch.AutoSize = true;
        this.TextSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TextSearch.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TextSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.TextSearch.Location = new System.Drawing.Point(0, 0);
        this.TextSearch.Name = "TextSearch";
        this.TextSearch.Size = new System.Drawing.Size(739, 24);
        this.TextSearch.TabIndex = 0;
        this.TextSearch.ToolTipBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TextSearch.ToolTipForeColor = System.Drawing.Color.White;
        this.TextSearch.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
        this.TextSearch.ToolTipText = resources.GetString("TextSearch.ToolTipText");
        this.TextSearch.ToolTipTitle = "Search";
        this.TextSearch.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.TextSearch_Search);
        this.TextSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextSearch_KeyUp);
        // 
        // TipFullPath
        // 
        this.TipFullPath.AutoPopDelay = 5000;
        this.TipFullPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TipFullPath.ForeColor = System.Drawing.Color.White;
        this.TipFullPath.InitialDelay = 1000;
        this.TipFullPath.ReshowDelay = 1000;
        // 
        // LabelNoFiles
        // 
        this.LabelNoFiles.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.LabelNoFiles.AutoSize = true;
        this.LabelNoFiles.Font = new System.Drawing.Font("Segoe UI Semibold", 11.75F);
        this.LabelNoFiles.Location = new System.Drawing.Point(313, 288);
        this.LabelNoFiles.Name = "LabelNoFiles";
        this.LabelNoFiles.Size = new System.Drawing.Size(118, 21);
        this.LabelNoFiles.TabIndex = 1;
        this.LabelNoFiles.Text = "No files found.";
        this.LabelNoFiles.Visible = false;
        // 
        // ContentFileExplorer
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.LabelNoFiles);
        this.Controls.Add(this.tableLayoutPanel1);
        this.Name = "ContentFileExplorer";
        this.Size = new System.Drawing.Size(745, 590);
        ((System.ComponentModel.ISupportInitialize)(this.GridFiles)).EndInit();
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        this.PanelSearch.ResumeLayout(false);
        this.PanelSearch.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.DataGridView GridFiles;
    private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelected;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDirName;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLocation;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFullFilePath;
    private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnDirectory;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Panel PanelSearch;
    private Gorgon.UI.GorgonSearchBox TextSearch;
    private System.Windows.Forms.ToolTip TipFullPath;
    private System.Windows.Forms.Label LabelNoFiles;
}
