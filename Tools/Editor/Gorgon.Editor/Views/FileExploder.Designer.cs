﻿namespace Gorgon.Editor.Views
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
            this.MenuItemIncludeInProject = new System.Windows.Forms.ToolStripMenuItem();
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
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.MenuItemCopyTo = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemMoveTo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemCancel = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuCopyMove = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuOptions.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.MenuCopyMove.SuspendLayout();
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
            this.TreeFileSystem.Location = new System.Drawing.Point(0, 26);
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
            this.TreeFileSystem.Size = new System.Drawing.Size(600, 442);
            this.TreeFileSystem.TabIndex = 0;
            this.TreeFileSystem.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterCollapse);
            this.TreeFileSystem.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterExpand);
            this.TreeFileSystem.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeFileSystem_AfterLabelEdit);
            this.TreeFileSystem.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeFileSystem_AfterSelect);
            this.TreeFileSystem.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeCollapse);
            this.TreeFileSystem.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeExpand);
            this.TreeFileSystem.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeFileSystem_BeforeLabelEdit);
            this.TreeFileSystem.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.TreeFileSystem_ItemDrag);
            this.TreeFileSystem.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeFileSystem_DragDrop);
            this.TreeFileSystem.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeFileSystem_DragOver);
            this.TreeFileSystem.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TreeFileSystem_KeyUp);
            // 
            // MenuOptions
            // 
            this.MenuOptions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemIncludeInProject,
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
            this.MenuOptions.Size = new System.Drawing.Size(168, 176);
            this.MenuOptions.Opening += new System.ComponentModel.CancelEventHandler(this.MenuOperations_Opening);
            // 
            // MenuItemIncludeInProject
            // 
            this.MenuItemIncludeInProject.CheckOnClick = true;
            this.MenuItemIncludeInProject.Name = "MenuItemIncludeInProject";
            this.MenuItemIncludeInProject.Size = new System.Drawing.Size(167, 22);
            this.MenuItemIncludeInProject.Text = "In project?";
            this.MenuItemIncludeInProject.Click += new System.EventHandler(this.ItemIncludeInProject_Click);
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
            this.MenuItemCut.Text = "Cut";
            this.MenuItemCut.Click += new System.EventHandler(this.MenuItemCut_Click);
            // 
            // MenuItemCopy
            // 
            this.MenuItemCopy.Name = "MenuItemCopy";
            this.MenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.MenuItemCopy.Size = new System.Drawing.Size(167, 22);
            this.MenuItemCopy.Text = "Copy";
            this.MenuItemCopy.Click += new System.EventHandler(this.MenuItemCopy_Click);
            // 
            // MenuItemPaste
            // 
            this.MenuItemPaste.Name = "MenuItemPaste";
            this.MenuItemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.MenuItemPaste.Size = new System.Drawing.Size(167, 22);
            this.MenuItemPaste.Text = "Paste";
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
            this.MenuItemCreateDirectory.Text = "Create directory...";
            this.MenuItemCreateDirectory.Click += new System.EventHandler(this.ItemCreateDirectory_Click);
            // 
            // TreeImages
            // 
            this.TreeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TreeImages.ImageStream")));
            this.TreeImages.TransparentColor = System.Drawing.Color.Transparent;
            this.TreeImages.Images.SetKeyName(0, "folder_20x20.png");
            this.TreeImages.Images.SetKeyName(1, "generic_file_20x20.png");
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
            this.panel3.Size = new System.Drawing.Size(600, 26);
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
            this.panel4.Size = new System.Drawing.Size(594, 26);
            this.panel4.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.Custom3;
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(594, 26);
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Values.Text = "File system";
            // 
            // MenuItemCopyTo
            // 
            this.MenuItemCopyTo.Name = "MenuItemCopyTo";
            this.MenuItemCopyTo.Size = new System.Drawing.Size(180, 22);
            this.MenuItemCopyTo.Text = "Copy here";
            this.MenuItemCopyTo.Click += new System.EventHandler(this.MenuItemCopyTo_Click);
            // 
            // MenuItemMoveTo
            // 
            this.MenuItemMoveTo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MenuItemMoveTo.Name = "MenuItemMoveTo";
            this.MenuItemMoveTo.Size = new System.Drawing.Size(180, 22);
            this.MenuItemMoveTo.Text = "Move here";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // MenuItemCancel
            // 
            this.MenuItemCancel.Name = "MenuItemCancel";
            this.MenuItemCancel.Size = new System.Drawing.Size(180, 22);
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
            this.MenuCopyMove.Size = new System.Drawing.Size(181, 98);
            // 
            // FileExploder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TreeFileSystem);
            this.Controls.Add(this.panel3);
            this.Name = "FileExploder";
            this.MenuOptions.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.MenuCopyMove.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTreeView TreeFileSystem;
        private System.Windows.Forms.ImageList TreeImages;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelHeader;
        private System.Windows.Forms.ContextMenuStrip MenuOptions;
        private System.Windows.Forms.ToolStripMenuItem MenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator MenuSepOrganize;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRename;
        private System.Windows.Forms.ToolStripMenuItem MenuItemIncludeInProject;
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
    }
}
