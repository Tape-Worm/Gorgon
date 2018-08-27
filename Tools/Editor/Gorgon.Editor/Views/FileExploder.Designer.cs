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
                _nodeLinks.Clear();
                _revNodeLinks.Clear();

                TreeFileSystem.TreeView.MouseUp -= TreeFileSystem_MouseUp;

                DataContext?.OnUnload();
                UnassignEvents();
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
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Node4");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Node5");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Node6");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Node0", new System.Windows.Forms.TreeNode[] {
            treeNode10,
            treeNode11,
            treeNode12});
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("Node1");
            System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("Node7");
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("Node8");
            System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("Node2", new System.Windows.Forms.TreeNode[] {
            treeNode15,
            treeNode16});
            System.Windows.Forms.TreeNode treeNode18 = new System.Windows.Forms.TreeNode("Node3");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileExploder));
            this.TreeFileSystem = new ComponentFactory.Krypton.Toolkit.KryptonTreeView();
            this.TreeImages = new System.Windows.Forms.ImageList(this.components);
            this.MenuOperations = new ComponentFactory.Krypton.Toolkit.KryptonContextMenu();
            this.MenuItems = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItems();
            this.ItemIncludeInProject = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.MenuSep1 = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuSeparator();
            this.ItemCreateDirectory = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.ItemRename = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.ItemDelete = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem();
            this.MenuSep2 = new ComponentFactory.Krypton.Toolkit.KryptonContextMenuSeparator();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeFileSystem
            // 
            this.TreeFileSystem.AlwaysActive = false;
            this.TreeFileSystem.BackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.ButtonAlternate;
            this.TreeFileSystem.BorderStyle = ComponentFactory.Krypton.Toolkit.PaletteBorderStyle.ButtonAlternate;
            this.TreeFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeFileSystem.FullRowSelect = true;
            this.TreeFileSystem.HideSelection = false;
            this.TreeFileSystem.ImageIndex = 0;
            this.TreeFileSystem.ImageList = this.TreeImages;
            this.TreeFileSystem.ItemHeight = 24;
            this.TreeFileSystem.LabelEdit = true;
            this.TreeFileSystem.Location = new System.Drawing.Point(0, 20);
            this.TreeFileSystem.Name = "TreeFileSystem";
            treeNode10.Name = "Node4";
            treeNode10.Text = "Node4";
            treeNode11.Name = "Node5";
            treeNode11.Text = "Node5";
            treeNode12.Name = "Node6";
            treeNode12.Text = "Node6";
            treeNode13.Name = "Node0";
            treeNode13.Text = "Node0";
            treeNode14.Name = "Node1";
            treeNode14.Text = "Node1";
            treeNode15.Name = "Node7";
            treeNode15.Text = "Node7";
            treeNode16.Name = "Node8";
            treeNode16.Text = "Node8";
            treeNode17.Name = "Node2";
            treeNode17.Text = "Node2";
            treeNode18.Name = "Node3";
            treeNode18.Text = "Node3";
            this.TreeFileSystem.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode13,
            treeNode14,
            treeNode17,
            treeNode18});
            this.TreeFileSystem.PathSeparator = "/";
            this.TreeFileSystem.SelectedImageIndex = 0;
            this.TreeFileSystem.ShowLines = false;
            this.TreeFileSystem.Size = new System.Drawing.Size(600, 448);
            this.TreeFileSystem.TabIndex = 0;
            this.TreeFileSystem.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterCollapse);
            this.TreeFileSystem.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeFileSystem_AfterLabelEdit);
            this.TreeFileSystem.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterSelect);
            this.TreeFileSystem.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeExpand);
            this.TreeFileSystem.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeFileSystem_BeforeLabelEdit);
            this.TreeFileSystem.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TreeFileSystem_KeyUp);
            // 
            // TreeImages
            // 
            this.TreeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TreeImages.ImageStream")));
            this.TreeImages.TransparentColor = System.Drawing.Color.Transparent;
            this.TreeImages.Images.SetKeyName(0, "folder_20x20.png");
            this.TreeImages.Images.SetKeyName(1, "generic_file_20x20.png");
            // 
            // MenuOperations
            // 
            this.MenuOperations.Items.AddRange(new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItemBase[] {
            this.MenuItems});
            this.MenuOperations.Opening += new System.ComponentModel.CancelEventHandler(this.MenuOperations_Opening);
            // 
            // MenuItems
            // 
            this.MenuItems.Items.AddRange(new ComponentFactory.Krypton.Toolkit.KryptonContextMenuItemBase[] {
            this.ItemIncludeInProject,
            this.MenuSep1,
            this.ItemDelete,
            this.ItemRename,
            this.MenuSep2,
            this.ItemCreateDirectory});
            // 
            // ItemIncludeInProject
            // 
            this.ItemIncludeInProject.CheckOnClick = true;
            this.ItemIncludeInProject.Text = "In project?";
            this.ItemIncludeInProject.Click += new System.EventHandler(this.ItemIncludeInProject_Click);
            // 
            // ItemCreateDirectory
            // 
            this.ItemCreateDirectory.Text = "Create directory...";
            this.ItemCreateDirectory.Visible = false;
            this.ItemCreateDirectory.Click += new System.EventHandler(this.ItemCreateDirectory_Click);
            // 
            // ItemRename
            // 
            this.ItemRename.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.ItemRename.Text = "Rename...";
            this.ItemRename.Click += new System.EventHandler(this.ItemRename_Click);
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.BackColor = System.Drawing.Color.SteelBlue;
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.panel3.Size = new System.Drawing.Size(600, 20);
            this.panel3.TabIndex = 6;
            // 
            // panel4
            // 
            this.panel4.AutoSize = true;
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.panel4.Controls.Add(this.LabelHeader);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(6, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(594, 20);
            this.panel4.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.Custom3;
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(594, 20);
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Values.Text = "File system";
            // 
            // ItemDelete
            // 
            this.ItemDelete.Text = "Delete...";
            this.ItemDelete.Visible = false;
            this.ItemDelete.Click += new System.EventHandler(this.ItemDelete_Click);
            // 
            // FileExploder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TreeFileSystem);
            this.Controls.Add(this.panel3);
            this.Name = "FileExploder";
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTreeView TreeFileSystem;
        private System.Windows.Forms.ImageList TreeImages;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelHeader;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenu MenuOperations;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem ItemRename;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItems MenuItems;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem ItemIncludeInProject;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuSeparator MenuSep1;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem ItemCreateDirectory;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuItem ItemDelete;
        private ComponentFactory.Krypton.Toolkit.KryptonContextMenuSeparator MenuSep2;
    }
}
