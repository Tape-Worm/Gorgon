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
                _listViewHeaderForeColor?.Dispose();
                _listViewHeaderBackColor?.Dispose();
                _nodeLinks.Clear();
                _revNodeLinks.Clear();

                TreeFileSystem.TreeView.MouseUp -= TreeFileSystem_MouseUp;

                DataContext?.OnUnload();
                UnassignEvents();

                _excludedFont?.Dispose();
                _openFont?.Dispose();
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
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Node4");
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Node5");
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Node6");
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Node0", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Node1");
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Node7");
			System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Node8");
			System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Node2", new System.Windows.Forms.TreeNode[] {
            treeNode6,
            treeNode7});
			System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Node3");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileExploder));
			this.TreeFileSystem = new ComponentFactory.Krypton.Toolkit.KryptonTreeView();
			this.MenuOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MenuItemOpenContent = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuSepContent = new System.Windows.Forms.ToolStripSeparator();
			this.MenuItemExportTo = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuSepEdit = new System.Windows.Forms.ToolStripSeparator();
			this.MenuItemCut = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuSepOrganize = new System.Windows.Forms.ToolStripSeparator();
			this.MenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuSepNew = new System.Windows.Forms.ToolStripSeparator();
			this.MenuItemCreateDirectory = new System.Windows.Forms.ToolStripMenuItem();
			this.TreeImages = new System.Windows.Forms.ImageList(this.components);
			this.MenuItemCopyTo = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuItemMoveTo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuItemCancel = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuCopyMove = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.PanelSearch = new System.Windows.Forms.Panel();
			this.TextSearch = new Gorgon.UI.GorgonSearchBox();
			this.ListSearchResults = new System.Windows.Forms.ListView();
			this.ColumnFileNode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnFilePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.MenuOptions.SuspendLayout();
			this.MenuCopyMove.SuspendLayout();
			this.PanelSearch.SuspendLayout();
			this.SuspendLayout();
			// 
			// TreeFileSystem
			// 
			this.TreeFileSystem.AllowDrop = true;
			this.TreeFileSystem.AlwaysActive = false;
			this.TreeFileSystem.BackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.ButtonAlternate;
			this.TreeFileSystem.BorderStyle = ComponentFactory.Krypton.Toolkit.PaletteBorderStyle.ButtonAlternate;
			this.TreeFileSystem.ContextMenuStrip = this.MenuOptions;
			this.TreeFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeFileSystem.FullRowSelect = true;
			this.TreeFileSystem.HideSelection = false;
			this.TreeFileSystem.ImageIndex = 0;
			this.TreeFileSystem.ImageList = this.TreeImages;
			this.TreeFileSystem.ItemHeight = 24;
			this.TreeFileSystem.LabelEdit = true;
			this.TreeFileSystem.Location = new System.Drawing.Point(0, 24);
			this.TreeFileSystem.Name = "TreeFileSystem";
			treeNode1.Name = "Node4";
			treeNode1.Text = "Node4";
			treeNode2.Name = "Node5";
			treeNode2.Text = "Node5";
			treeNode3.Name = "Node6";
			treeNode3.Text = "Node6";
			treeNode4.Name = "Node0";
			treeNode4.Text = "Node0";
			treeNode5.Name = "Node1";
			treeNode5.Text = "Node1";
			treeNode6.Name = "Node7";
			treeNode6.Text = "Node7";
			treeNode7.Name = "Node8";
			treeNode7.Text = "Node8";
			treeNode8.Name = "Node2";
			treeNode8.Text = "Node2";
			treeNode9.Name = "Node3";
			treeNode9.Text = "Node3";
			this.TreeFileSystem.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode4,
            treeNode5,
            treeNode8,
            treeNode9});
			this.TreeFileSystem.PathSeparator = "/";
			this.TreeFileSystem.SelectedImageIndex = 0;
			this.TreeFileSystem.ShowLines = false;
			this.TreeFileSystem.Size = new System.Drawing.Size(600, 444);
			this.TreeFileSystem.Sorted = true;
			this.TreeFileSystem.TabIndex = 0;
			this.TreeFileSystem.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterCollapse);
			this.TreeFileSystem.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterExpand);
			this.TreeFileSystem.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeFileSystem_AfterLabelEdit);
			this.TreeFileSystem.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterSelect);
			this.TreeFileSystem.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeCollapse);
			this.TreeFileSystem.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeExpand);
			this.TreeFileSystem.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeFileSystem_BeforeLabelEdit);
			this.TreeFileSystem.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeSelect);
			this.TreeFileSystem.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.TreeFileSystem_ItemDrag);
			this.TreeFileSystem.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeFileSystem_NodeMouseDoubleClick);
			this.TreeFileSystem.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeFileSystem_DragDrop);
			this.TreeFileSystem.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeFileSystem_DragEnter);
			this.TreeFileSystem.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeFileSystem_DragOver);
			this.TreeFileSystem.DragLeave += new System.EventHandler(this.TreeFileSystem_DragLeave);
			this.TreeFileSystem.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TreeFileSystem_KeyUp);
			// 
			// MenuOptions
			// 
			this.MenuOptions.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.MenuOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemOpenContent,
            this.MenuSepContent,
            this.MenuItemExportTo,
            this.MenuSepEdit,
            this.MenuItemCut,
            this.MenuItemCopy,
            this.MenuItemPaste,
            this.MenuSepOrganize,
            this.MenuItemDelete,
            this.MenuItemRename,
            this.MenuSepNew,
            this.MenuItemCreateDirectory});
			this.MenuOptions.Name = "MenuOptions";
			this.MenuOptions.Size = new System.Drawing.Size(168, 204);
			this.MenuOptions.Opening += new System.ComponentModel.CancelEventHandler(this.MenuOperations_Opening);
			// 
			// MenuItemOpenContent
			// 
			this.MenuItemOpenContent.Name = "MenuItemOpenContent";
			this.MenuItemOpenContent.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.MenuItemOpenContent.Size = new System.Drawing.Size(167, 22);
			this.MenuItemOpenContent.Text = "Open";
			this.MenuItemOpenContent.Click += new System.EventHandler(this.MenuItemOpenContent_Click);
			// 
			// MenuSepContent
			// 
			this.MenuSepContent.Name = "MenuSepContent";
			this.MenuSepContent.Size = new System.Drawing.Size(164, 6);
			// 
			// MenuItemExportTo
			// 
			this.MenuItemExportTo.Name = "MenuItemExportTo";
			this.MenuItemExportTo.Size = new System.Drawing.Size(167, 22);
			this.MenuItemExportTo.Text = "E&xport to...";
			this.MenuItemExportTo.Click += new System.EventHandler(this.MenuItemExportTo_Click);
			// 
			// MenuSepEdit
			// 
			this.MenuSepEdit.Name = "MenuSepEdit";
			this.MenuSepEdit.Size = new System.Drawing.Size(164, 6);
			// 
			// MenuItemCut
			// 
			this.MenuItemCut.Name = "MenuItemCut";
			this.MenuItemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.MenuItemCut.Size = new System.Drawing.Size(167, 22);
			this.MenuItemCut.Text = "Cu&t";
			this.MenuItemCut.Click += new System.EventHandler(this.MenuItemCut_Click);
			// 
			// MenuItemCopy
			// 
			this.MenuItemCopy.Name = "MenuItemCopy";
			this.MenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.MenuItemCopy.Size = new System.Drawing.Size(167, 22);
			this.MenuItemCopy.Text = "C&opy";
			this.MenuItemCopy.Click += new System.EventHandler(this.MenuItemCopy_Click);
			// 
			// MenuItemPaste
			// 
			this.MenuItemPaste.Name = "MenuItemPaste";
			this.MenuItemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.MenuItemPaste.Size = new System.Drawing.Size(167, 22);
			this.MenuItemPaste.Text = "&Paste";
			this.MenuItemPaste.Click += new System.EventHandler(this.MenuItemPaste_Click);
			// 
			// MenuSepOrganize
			// 
			this.MenuSepOrganize.Name = "MenuSepOrganize";
			this.MenuSepOrganize.Size = new System.Drawing.Size(164, 6);
			// 
			// MenuItemDelete
			// 
			this.MenuItemDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.MenuItemDelete.Name = "MenuItemDelete";
			this.MenuItemDelete.ShortcutKeyDisplayString = "Del";
			this.MenuItemDelete.Size = new System.Drawing.Size(167, 22);
			this.MenuItemDelete.Text = "&Delete...";
			this.MenuItemDelete.Click += new System.EventHandler(this.ItemDelete_Click);
			// 
			// MenuItemRename
			// 
			this.MenuItemRename.Name = "MenuItemRename";
			this.MenuItemRename.ShortcutKeys = System.Windows.Forms.Keys.F2;
			this.MenuItemRename.Size = new System.Drawing.Size(167, 22);
			this.MenuItemRename.Text = "&Rename...";
			this.MenuItemRename.Click += new System.EventHandler(this.ItemRename_Click);
			// 
			// MenuSepNew
			// 
			this.MenuSepNew.Name = "MenuSepNew";
			this.MenuSepNew.Size = new System.Drawing.Size(164, 6);
			// 
			// MenuItemCreateDirectory
			// 
			this.MenuItemCreateDirectory.Name = "MenuItemCreateDirectory";
			this.MenuItemCreateDirectory.Size = new System.Drawing.Size(167, 22);
			this.MenuItemCreateDirectory.Text = "&Create directory...";
			this.MenuItemCreateDirectory.Click += new System.EventHandler(this.ItemCreateDirectory_Click);
			// 
			// TreeImages
			// 
			this.TreeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TreeImages.ImageStream")));
			this.TreeImages.TransparentColor = System.Drawing.Color.Transparent;
			this.TreeImages.Images.SetKeyName(0, "folder_20x20.png");
			this.TreeImages.Images.SetKeyName(1, "generic_file_20x20.png");
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
			// PanelSearch
			// 
			this.PanelSearch.AutoSize = true;
			this.PanelSearch.Controls.Add(this.TextSearch);
			this.PanelSearch.Dock = System.Windows.Forms.DockStyle.Top;
			this.PanelSearch.Location = new System.Drawing.Point(0, 0);
			this.PanelSearch.Name = "PanelSearch";
			this.PanelSearch.Size = new System.Drawing.Size(600, 24);
			this.PanelSearch.TabIndex = 1;
			// 
			// TextSearch
			// 
			this.TextSearch.AutoSize = true;
			this.TextSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.TextSearch.Dock = System.Windows.Forms.DockStyle.Top;
			this.TextSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.TextSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.TextSearch.Location = new System.Drawing.Point(0, 0);
			this.TextSearch.Name = "TextSearch";
			this.TextSearch.Size = new System.Drawing.Size(600, 24);
			this.TextSearch.TabIndex = 2;
			this.TextSearch.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.TextSearch_Search);
			// 
			// ListSearchResults
			// 
			this.ListSearchResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ListSearchResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ListSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnFileNode,
            this.ColumnFilePath});
			this.ListSearchResults.ContextMenuStrip = this.MenuOptions;
			this.ListSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListSearchResults.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.ListSearchResults.FullRowSelect = true;
			this.ListSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ListSearchResults.HideSelection = false;
			this.ListSearchResults.LabelEdit = true;
			this.ListSearchResults.LabelWrap = false;
			this.ListSearchResults.LargeImageList = this.TreeImages;
			this.ListSearchResults.Location = new System.Drawing.Point(0, 0);
			this.ListSearchResults.MultiSelect = false;
			this.ListSearchResults.Name = "ListSearchResults";
			this.ListSearchResults.OwnerDraw = true;
			this.ListSearchResults.Size = new System.Drawing.Size(600, 468);
			this.ListSearchResults.SmallImageList = this.TreeImages;
			this.ListSearchResults.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.ListSearchResults.TabIndex = 2;
			this.ListSearchResults.UseCompatibleStateImageBehavior = false;
			this.ListSearchResults.View = System.Windows.Forms.View.Details;
			this.ListSearchResults.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.ListSearchResults_AfterLabelEdit);
			this.ListSearchResults.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.ListSearchResults_BeforeLabelEdit);
			this.ListSearchResults.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ListSearchResults_DrawColumnHeader);
			this.ListSearchResults.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListSearchResults_DrawItem);
			this.ListSearchResults.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ListSearchResults_DrawSubItem);
			this.ListSearchResults.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ListSearchResults_ItemDrag);
			this.ListSearchResults.SelectedIndexChanged += new System.EventHandler(this.ListSearchResults_SelectedIndexChanged);
			this.ListSearchResults.DoubleClick += new System.EventHandler(this.ListSearchResults_DoubleClick);
			this.ListSearchResults.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ListSearchResults_KeyUp);
			// 
			// ColumnFileNode
			// 
			this.ColumnFileNode.Text = "File";
			// 
			// ColumnFilePath
			// 
			this.ColumnFilePath.Text = "Path";
			// 
			// FileExploder
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.TreeFileSystem);
			this.Controls.Add(this.PanelSearch);
			this.Controls.Add(this.ListSearchResults);
			this.Name = "FileExploder";
			this.MenuOptions.ResumeLayout(false);
			this.MenuCopyMove.ResumeLayout(false);
			this.PanelSearch.ResumeLayout(false);
			this.PanelSearch.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTreeView TreeFileSystem;
        private System.Windows.Forms.ImageList TreeImages;
        private System.Windows.Forms.ContextMenuStrip MenuOptions;
        private System.Windows.Forms.ToolStripMenuItem MenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator MenuSepOrganize;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRename;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCreateDirectory;
        private System.Windows.Forms.ToolStripSeparator MenuSepNew;
        private System.Windows.Forms.ToolStripSeparator MenuSepEdit;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCut;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCopy;
        private System.Windows.Forms.ToolStripMenuItem MenuItemPaste;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCopyTo;
        private System.Windows.Forms.ToolStripMenuItem MenuItemMoveTo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCancel;
        private System.Windows.Forms.ContextMenuStrip MenuCopyMove;
        private System.Windows.Forms.ToolStripMenuItem MenuItemExportTo;
        private System.Windows.Forms.Panel PanelSearch;
        private Gorgon.UI.GorgonSearchBox TextSearch;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpenContent;
        private System.Windows.Forms.ToolStripSeparator MenuSepContent;
        private System.Windows.Forms.ListView ListSearchResults;
        private System.Windows.Forms.ColumnHeader ColumnFileNode;
        private System.Windows.Forms.ColumnHeader ColumnFilePath;
    }
}
