#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Sunday, April 01, 2007 3:51:52 PM
// 
#endregion

namespace GorgonLibrary.FileSystems.Tools
{
	partial class formFileSystemWindow
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
				// Remove event.
				if (_fileSystem != null)
				{
					_fileSystem.FileWrite -= new FileSystemReadWriteHandler(fileSystem_FileWrite);

					// Unmount the file system.
					if ((Gorgon.IsInitialized) && (FileSystemCache.FileSystems.Contains(_fileSystem.Name)))
						_fileSystem.Dispose();

					_fileSystem = null;
				}
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formFileSystemWindow));
			this.splitTree = new System.Windows.Forms.SplitContainer();
			this.treePaths = new System.Windows.Forms.TreeView();
			this.menuTreePopup = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemAddPath = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRemovePath = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemAddFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemCopyPathToExplorer = new System.Windows.Forms.ToolStripMenuItem();
			this.treeImages = new System.Windows.Forms.ImageList(this.components);
			this.splitDetailView = new System.Windows.Forms.SplitContainer();
			this.viewFiles = new System.Windows.Forms.ListView();
			this.headerFileName = new System.Windows.Forms.ColumnHeader();
			this.headerExtension = new System.Windows.Forms.ColumnHeader();
			this.headerDateTime = new System.Windows.Forms.ColumnHeader();
			this.headerSize = new System.Windows.Forms.ColumnHeader();
			this.menuFilePopup = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemEditComment = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemView = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSmallIcons = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemLargeIcons = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemList = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDetails = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemViewAddFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRemoveFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemCopyToExplorer = new System.Windows.Forms.ToolStripMenuItem();
			this.panelPreview = new System.Windows.Forms.Panel();
			this.htmlPreview = new System.Windows.Forms.WebBrowser();
			this.labelPreview = new System.Windows.Forms.Label();
			this.textPreview = new System.Windows.Forms.TextBox();
			this.pictureImage = new System.Windows.Forms.PictureBox();
			this.textComment = new System.Windows.Forms.TextBox();
			this.labelComment = new System.Windows.Forms.Label();
			this.textProperties = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textLocation = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textFileName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.pictureIcon = new System.Windows.Forms.PictureBox();
			this.dialogAddFile = new System.Windows.Forms.OpenFileDialog();
			this.stripMenu = new System.Windows.Forms.ToolStrip();
			this.buttonChangeAuth = new System.Windows.Forms.ToolStripButton();
			this.buttonPurge = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonCopyToExplorer = new System.Windows.Forms.ToolStripButton();
			this.dialogFolders = new System.Windows.Forms.FolderBrowserDialog();
			this.splitTree.Panel1.SuspendLayout();
			this.splitTree.Panel2.SuspendLayout();
			this.splitTree.SuspendLayout();
			this.menuTreePopup.SuspendLayout();
			this.splitDetailView.Panel1.SuspendLayout();
			this.splitDetailView.Panel2.SuspendLayout();
			this.splitDetailView.SuspendLayout();
			this.menuFilePopup.SuspendLayout();
			this.panelPreview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).BeginInit();
			this.stripMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitTree
			// 
			this.splitTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitTree.Location = new System.Drawing.Point(0, 25);
			this.splitTree.Name = "splitTree";
			// 
			// splitTree.Panel1
			// 
			this.splitTree.Panel1.Controls.Add(this.treePaths);
			// 
			// splitTree.Panel2
			// 
			this.splitTree.Panel2.Controls.Add(this.splitDetailView);
			this.splitTree.Size = new System.Drawing.Size(572, 400);
			this.splitTree.SplitterDistance = 190;
			this.splitTree.TabIndex = 0;
			// 
			// treePaths
			// 
			this.treePaths.AllowDrop = true;
			this.treePaths.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treePaths.ContextMenuStrip = this.menuTreePopup;
			this.treePaths.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treePaths.FullRowSelect = true;
			this.treePaths.HideSelection = false;
			this.treePaths.ImageIndex = 0;
			this.treePaths.ImageList = this.treeImages;
			this.treePaths.LabelEdit = true;
			this.treePaths.Location = new System.Drawing.Point(0, 0);
			this.treePaths.Name = "treePaths";
			this.treePaths.SelectedImageIndex = 0;
			this.treePaths.Size = new System.Drawing.Size(190, 400);
			this.treePaths.TabIndex = 0;
			this.treePaths.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treePaths_AfterCollapse);
			this.treePaths.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treePaths_AfterLabelEdit);
			this.treePaths.DragDrop += new System.Windows.Forms.DragEventHandler(this.treePaths_DragDrop);
			this.treePaths.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treePaths_AfterSelect);
			this.treePaths.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treePaths_MouseDown);
			this.treePaths.DragEnter += new System.Windows.Forms.DragEventHandler(this.treePaths_DragEnter);
			this.treePaths.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treePaths_BeforeLabelEdit);
			this.treePaths.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treePaths_KeyDown);
			this.treePaths.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treePaths_AfterExpand);
			this.treePaths.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treePaths_ItemDrag);
			this.treePaths.DragOver += new System.Windows.Forms.DragEventHandler(this.treePaths_DragOver);
			// 
			// menuTreePopup
			// 
			this.menuTreePopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAddPath,
            this.menuItemRemovePath,
            this.toolStripMenuItem1,
            this.menuItemAddFiles,
            this.toolStripSeparator4,
            this.menuItemCopyPathToExplorer});
			this.menuTreePopup.Name = "contextMenuStrip1";
			this.menuTreePopup.Size = new System.Drawing.Size(204, 104);
			// 
			// menuItemAddPath
			// 
			this.menuItemAddPath.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_add;
			this.menuItemAddPath.Name = "menuItemAddPath";
			this.menuItemAddPath.Size = new System.Drawing.Size(203, 22);
			this.menuItemAddPath.Text = "Add path...";
			this.menuItemAddPath.Click += new System.EventHandler(this.menuItemAddPath_Click);
			// 
			// menuItemRemovePath
			// 
			this.menuItemRemovePath.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_delete;
			this.menuItemRemovePath.Name = "menuItemRemovePath";
			this.menuItemRemovePath.Size = new System.Drawing.Size(203, 22);
			this.menuItemRemovePath.Text = "Remove path...";
			this.menuItemRemovePath.Click += new System.EventHandler(this.menuItemRemovePath_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItemAddFiles
			// 
			this.menuItemAddFiles.Name = "menuItemAddFiles";
			this.menuItemAddFiles.Size = new System.Drawing.Size(203, 22);
			this.menuItemAddFiles.Text = "Add file(s)...";
			this.menuItemAddFiles.Click += new System.EventHandler(this.menuItemAddFiles_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItemCopyPathToExplorer
			// 
			this.menuItemCopyPathToExplorer.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_into;
			this.menuItemCopyPathToExplorer.Name = "menuItemCopyPathToExplorer";
			this.menuItemCopyPathToExplorer.Size = new System.Drawing.Size(203, 22);
			this.menuItemCopyPathToExplorer.Text = "Copy to external folder...";
			this.menuItemCopyPathToExplorer.ToolTipText = "Copies the selected path from the virtual file system on to the harddrive.";
			this.menuItemCopyPathToExplorer.Click += new System.EventHandler(this.menuItemCopyToExplorer_Click);
			// 
			// treeImages
			// 
			this.treeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImages.ImageStream")));
			this.treeImages.TransparentColor = System.Drawing.Color.Transparent;
			this.treeImages.Images.SetKeyName(0, "folder_closed.png");
			this.treeImages.Images.SetKeyName(1, "folder.png");
			// 
			// splitDetailView
			// 
			this.splitDetailView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitDetailView.Location = new System.Drawing.Point(0, 0);
			this.splitDetailView.Name = "splitDetailView";
			this.splitDetailView.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitDetailView.Panel1
			// 
			this.splitDetailView.Panel1.Controls.Add(this.viewFiles);
			// 
			// splitDetailView.Panel2
			// 
			this.splitDetailView.Panel2.Controls.Add(this.panelPreview);
			this.splitDetailView.Size = new System.Drawing.Size(378, 400);
			this.splitDetailView.SplitterDistance = 260;
			this.splitDetailView.TabIndex = 0;
			// 
			// viewFiles
			// 
			this.viewFiles.AllowDrop = true;
			this.viewFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.viewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerFileName,
            this.headerExtension,
            this.headerDateTime,
            this.headerSize});
			this.viewFiles.ContextMenuStrip = this.menuFilePopup;
			this.viewFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.viewFiles.LabelEdit = true;
			this.viewFiles.Location = new System.Drawing.Point(0, 0);
			this.viewFiles.Name = "viewFiles";
			this.viewFiles.Size = new System.Drawing.Size(378, 260);
			this.viewFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.viewFiles.TabIndex = 0;
			this.viewFiles.UseCompatibleStateImageBehavior = false;
			this.viewFiles.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.viewFiles_AfterLabelEdit);
			this.viewFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.viewFiles_DragDrop);
			this.viewFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.viewFiles_ColumnClick);
			this.viewFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.viewFiles_MouseDown);
			this.viewFiles.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.viewFiles_ItemSelectionChanged);
			this.viewFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.viewFiles_DragEnter);
			this.viewFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.viewFiles_KeyDown);
			this.viewFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.viewFiles_ItemDrag);
			this.viewFiles.DragOver += new System.Windows.Forms.DragEventHandler(this.viewFiles_DragOver);
			// 
			// headerFileName
			// 
			this.headerFileName.Text = "Filename";
			// 
			// headerExtension
			// 
			this.headerExtension.Text = "Extension";
			// 
			// headerDateTime
			// 
			this.headerDateTime.DisplayIndex = 3;
			this.headerDateTime.Text = "Date";
			// 
			// headerSize
			// 
			this.headerSize.DisplayIndex = 2;
			this.headerSize.Text = "File size";
			// 
			// menuFilePopup
			// 
			this.menuFilePopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemEditComment,
            this.toolStripSeparator2,
            this.menuItemView,
            this.toolStripSeparator1,
            this.menuItemViewAddFiles,
            this.menuItemRemoveFiles,
            this.toolStripSeparator3,
            this.menuItemCopyToExplorer});
			this.menuFilePopup.Name = "contextMenuStrip1";
			this.menuFilePopup.Size = new System.Drawing.Size(204, 132);
			// 
			// menuItemEditComment
			// 
			this.menuItemEditComment.Enabled = false;
			this.menuItemEditComment.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.document_dirty;
			this.menuItemEditComment.Name = "menuItemEditComment";
			this.menuItemEditComment.Size = new System.Drawing.Size(203, 22);
			this.menuItemEditComment.Text = "Edit comment...";
			this.menuItemEditComment.Click += new System.EventHandler(this.menuItemEditComment_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItemView
			// 
			this.menuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSmallIcons,
            this.menuItemLargeIcons,
            this.menuItemList,
            this.menuItemDetails});
			this.menuItemView.Name = "menuItemView";
			this.menuItemView.Size = new System.Drawing.Size(203, 22);
			this.menuItemView.Text = "View";
			// 
			// menuItemSmallIcons
			// 
			this.menuItemSmallIcons.CheckOnClick = true;
			this.menuItemSmallIcons.Name = "menuItemSmallIcons";
			this.menuItemSmallIcons.Size = new System.Drawing.Size(134, 22);
			this.menuItemSmallIcons.Text = "Small Icons";
			this.menuItemSmallIcons.Click += new System.EventHandler(this.menuItemSmallIcons_Click);
			// 
			// menuItemLargeIcons
			// 
			this.menuItemLargeIcons.Checked = true;
			this.menuItemLargeIcons.CheckOnClick = true;
			this.menuItemLargeIcons.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItemLargeIcons.Name = "menuItemLargeIcons";
			this.menuItemLargeIcons.Size = new System.Drawing.Size(134, 22);
			this.menuItemLargeIcons.Text = "Large Icons";
			this.menuItemLargeIcons.Click += new System.EventHandler(this.menuItemLargeIcons_Click);
			// 
			// menuItemList
			// 
			this.menuItemList.CheckOnClick = true;
			this.menuItemList.Name = "menuItemList";
			this.menuItemList.Size = new System.Drawing.Size(134, 22);
			this.menuItemList.Text = "List";
			this.menuItemList.Click += new System.EventHandler(this.menuItemList_Click);
			// 
			// menuItemDetails
			// 
			this.menuItemDetails.CheckOnClick = true;
			this.menuItemDetails.Name = "menuItemDetails";
			this.menuItemDetails.Size = new System.Drawing.Size(134, 22);
			this.menuItemDetails.Text = "Details";
			this.menuItemDetails.Click += new System.EventHandler(this.menuItemDetails_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItemViewAddFiles
			// 
			this.menuItemViewAddFiles.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.add;
			this.menuItemViewAddFiles.Name = "menuItemViewAddFiles";
			this.menuItemViewAddFiles.Size = new System.Drawing.Size(203, 22);
			this.menuItemViewAddFiles.Text = "Add file(s)...";
			this.menuItemViewAddFiles.Click += new System.EventHandler(this.menuItemAddFiles_Click);
			// 
			// menuItemRemoveFiles
			// 
			this.menuItemRemoveFiles.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.delete1;
			this.menuItemRemoveFiles.Name = "menuItemRemoveFiles";
			this.menuItemRemoveFiles.Size = new System.Drawing.Size(203, 22);
			this.menuItemRemoveFiles.Text = "Remove file(s)...";
			this.menuItemRemoveFiles.Click += new System.EventHandler(this.menuItemRemoveFiles_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItemCopyToExplorer
			// 
			this.menuItemCopyToExplorer.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_into;
			this.menuItemCopyToExplorer.Name = "menuItemCopyToExplorer";
			this.menuItemCopyToExplorer.Size = new System.Drawing.Size(203, 22);
			this.menuItemCopyToExplorer.Text = "Copy to external folder...";
			this.menuItemCopyToExplorer.ToolTipText = "Copies the selected files from the virtual file system on to the harddrive.";
			this.menuItemCopyToExplorer.Click += new System.EventHandler(this.menuItemCopyToExplorer_Click);
			// 
			// panelPreview
			// 
			this.panelPreview.AutoScroll = true;
			this.panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelPreview.Controls.Add(this.htmlPreview);
			this.panelPreview.Controls.Add(this.labelPreview);
			this.panelPreview.Controls.Add(this.textPreview);
			this.panelPreview.Controls.Add(this.pictureImage);
			this.panelPreview.Controls.Add(this.textComment);
			this.panelPreview.Controls.Add(this.labelComment);
			this.panelPreview.Controls.Add(this.textProperties);
			this.panelPreview.Controls.Add(this.label3);
			this.panelPreview.Controls.Add(this.textLocation);
			this.panelPreview.Controls.Add(this.label2);
			this.panelPreview.Controls.Add(this.textFileName);
			this.panelPreview.Controls.Add(this.label1);
			this.panelPreview.Controls.Add(this.pictureIcon);
			this.panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPreview.Location = new System.Drawing.Point(0, 0);
			this.panelPreview.Name = "panelPreview";
			this.panelPreview.Size = new System.Drawing.Size(378, 136);
			this.panelPreview.TabIndex = 0;
			// 
			// htmlPreview
			// 
			this.htmlPreview.AllowWebBrowserDrop = false;
			this.htmlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.htmlPreview.IsWebBrowserContextMenuEnabled = false;
			this.htmlPreview.Location = new System.Drawing.Point(13, 275);
			this.htmlPreview.MinimumSize = new System.Drawing.Size(20, 20);
			this.htmlPreview.Name = "htmlPreview";
			this.htmlPreview.ScriptErrorsSuppressed = true;
			this.htmlPreview.Size = new System.Drawing.Size(340, 92);
			this.htmlPreview.TabIndex = 24;
			this.htmlPreview.Visible = false;
			this.htmlPreview.WebBrowserShortcutsEnabled = false;
			// 
			// labelPreview
			// 
			this.labelPreview.AutoSize = true;
			this.labelPreview.Location = new System.Drawing.Point(8, 259);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(48, 13);
			this.labelPreview.TabIndex = 23;
			this.labelPreview.Text = "Preview:";
			this.labelPreview.Visible = false;
			// 
			// textPreview
			// 
			this.textPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textPreview.BackColor = System.Drawing.SystemColors.Control;
			this.textPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textPreview.Location = new System.Drawing.Point(12, 275);
			this.textPreview.Multiline = true;
			this.textPreview.Name = "textPreview";
			this.textPreview.ReadOnly = true;
			this.textPreview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textPreview.Size = new System.Drawing.Size(340, 112);
			this.textPreview.TabIndex = 22;
			this.textPreview.Visible = false;
			this.textPreview.WordWrap = false;
			// 
			// pictureImage
			// 
			this.pictureImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pictureImage.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.pictureImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureImage.Location = new System.Drawing.Point(12, 275);
			this.pictureImage.Name = "pictureImage";
			this.pictureImage.Size = new System.Drawing.Size(341, 109);
			this.pictureImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureImage.TabIndex = 21;
			this.pictureImage.TabStop = false;
			this.pictureImage.Visible = false;
			// 
			// textComment
			// 
			this.textComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textComment.BackColor = System.Drawing.SystemColors.Control;
			this.textComment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textComment.Location = new System.Drawing.Point(11, 163);
			this.textComment.Multiline = true;
			this.textComment.Name = "textComment";
			this.textComment.ReadOnly = true;
			this.textComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textComment.Size = new System.Drawing.Size(341, 93);
			this.textComment.TabIndex = 20;
			this.textComment.Visible = false;
			// 
			// labelComment
			// 
			this.labelComment.AutoSize = true;
			this.labelComment.Location = new System.Drawing.Point(9, 147);
			this.labelComment.Name = "labelComment";
			this.labelComment.Size = new System.Drawing.Size(54, 13);
			this.labelComment.TabIndex = 19;
			this.labelComment.Text = "Comment:";
			this.labelComment.Visible = false;
			// 
			// textProperties
			// 
			this.textProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textProperties.BackColor = System.Drawing.SystemColors.Control;
			this.textProperties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textProperties.Location = new System.Drawing.Point(11, 102);
			this.textProperties.Multiline = true;
			this.textProperties.Name = "textProperties";
			this.textProperties.ReadOnly = true;
			this.textProperties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textProperties.Size = new System.Drawing.Size(341, 42);
			this.textProperties.TabIndex = 18;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 86);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 13);
			this.label3.TabIndex = 17;
			this.label3.Text = "Properties:";
			// 
			// textLocation
			// 
			this.textLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textLocation.BackColor = System.Drawing.SystemColors.Control;
			this.textLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textLocation.Location = new System.Drawing.Point(12, 63);
			this.textLocation.Multiline = true;
			this.textLocation.Name = "textLocation";
			this.textLocation.ReadOnly = true;
			this.textLocation.Size = new System.Drawing.Size(287, 20);
			this.textLocation.TabIndex = 16;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 47);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(51, 13);
			this.label2.TabIndex = 15;
			this.label2.Text = "Location:";
			// 
			// textFileName
			// 
			this.textFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textFileName.BackColor = System.Drawing.SystemColors.Control;
			this.textFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textFileName.Location = new System.Drawing.Point(12, 24);
			this.textFileName.Multiline = true;
			this.textFileName.Name = "textFileName";
			this.textFileName.ReadOnly = true;
			this.textFileName.Size = new System.Drawing.Size(287, 20);
			this.textFileName.TabIndex = 14;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 13);
			this.label1.TabIndex = 13;
			this.label1.Text = "Filename:";
			// 
			// pictureIcon
			// 
			this.pictureIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureIcon.BackColor = System.Drawing.Color.Transparent;
			this.pictureIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureIcon.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.disk_blue1;
			this.pictureIcon.InitialImage = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.disk_blue1;
			this.pictureIcon.Location = new System.Drawing.Point(304, 8);
			this.pictureIcon.Name = "pictureIcon";
			this.pictureIcon.Size = new System.Drawing.Size(48, 48);
			this.pictureIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureIcon.TabIndex = 12;
			this.pictureIcon.TabStop = false;
			// 
			// dialogAddFile
			// 
			this.dialogAddFile.Filter = "All files (*.*)|*.*";
			this.dialogAddFile.InitialDirectory = ".\\";
			this.dialogAddFile.Multiselect = true;
			// 
			// stripMenu
			// 
			this.stripMenu.AutoSize = false;
			this.stripMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonChangeAuth,
            this.buttonPurge,
            this.toolStripSeparator5,
            this.buttonCopyToExplorer});
			this.stripMenu.Location = new System.Drawing.Point(0, 0);
			this.stripMenu.Name = "stripMenu";
			this.stripMenu.Size = new System.Drawing.Size(572, 25);
			this.stripMenu.TabIndex = 2;
			this.stripMenu.Text = "toolStrip1";
			// 
			// buttonChangeAuth
			// 
			this.buttonChangeAuth.Enabled = false;
			this.buttonChangeAuth.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.key1;
			this.buttonChangeAuth.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonChangeAuth.Name = "buttonChangeAuth";
			this.buttonChangeAuth.Size = new System.Drawing.Size(135, 22);
			this.buttonChangeAuth.Text = "Assign authorization";
			this.buttonChangeAuth.Click += new System.EventHandler(this.buttonChangeAuth_Click);
			// 
			// buttonPurge
			// 
			this.buttonPurge.CheckOnClick = true;
			this.buttonPurge.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.delete2;
			this.buttonPurge.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPurge.Name = "buttonPurge";
			this.buttonPurge.Size = new System.Drawing.Size(58, 22);
			this.buttonPurge.Text = "Purge";
			this.buttonPurge.ToolTipText = "When this option is selected, deleted files and paths will be purged from this fo" +
				"lder file system when this file system is saved.";
			this.buttonPurge.Click += new System.EventHandler(this.buttonPurge_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonCopyToExplorer
			// 
			this.buttonCopyToExplorer.Enabled = false;
			this.buttonCopyToExplorer.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_into;
			this.buttonCopyToExplorer.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonCopyToExplorer.Name = "buttonCopyToExplorer";
			this.buttonCopyToExplorer.Size = new System.Drawing.Size(206, 22);
			this.buttonCopyToExplorer.Text = "Copy file system to external folder";
			this.buttonCopyToExplorer.ToolTipText = "Copies the contents of the file system to the harddrive.";
			this.buttonCopyToExplorer.Click += new System.EventHandler(this.menuItemCopyToExplorer_Click);
			// 
			// dialogFolders
			// 
			this.dialogFolders.Description = "Select the folder to copy the selected path/files into.";
			// 
			// formFileSystemWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(572, 425);
			this.Controls.Add(this.splitTree);
			this.Controls.Add(this.stripMenu);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "formFileSystemWindow";
			this.Text = "File System";
			this.splitTree.Panel1.ResumeLayout(false);
			this.splitTree.Panel2.ResumeLayout(false);
			this.splitTree.ResumeLayout(false);
			this.menuTreePopup.ResumeLayout(false);
			this.splitDetailView.Panel1.ResumeLayout(false);
			this.splitDetailView.Panel2.ResumeLayout(false);
			this.splitDetailView.ResumeLayout(false);
			this.menuFilePopup.ResumeLayout(false);
			this.panelPreview.ResumeLayout(false);
			this.panelPreview.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
			this.stripMenu.ResumeLayout(false);
			this.stripMenu.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitTree;
		private System.Windows.Forms.TreeView treePaths;
		private System.Windows.Forms.SplitContainer splitDetailView;
		private System.Windows.Forms.ListView viewFiles;
		private System.Windows.Forms.Panel panelPreview;
		private System.Windows.Forms.ImageList treeImages;
		private System.Windows.Forms.ContextMenuStrip menuTreePopup;
		private System.Windows.Forms.ToolStripMenuItem menuItemAddPath;
		private System.Windows.Forms.ToolStripMenuItem menuItemRemovePath;
		private System.Windows.Forms.ContextMenuStrip menuFilePopup;
		private System.Windows.Forms.ToolStripMenuItem menuItemView;
		private System.Windows.Forms.ToolStripMenuItem menuItemSmallIcons;
		private System.Windows.Forms.ToolStripMenuItem menuItemLargeIcons;
		private System.Windows.Forms.ToolStripMenuItem menuItemList;
		private System.Windows.Forms.ToolStripMenuItem menuItemDetails;
		private System.Windows.Forms.ColumnHeader headerFileName;
		private System.Windows.Forms.ColumnHeader headerExtension;
		private System.Windows.Forms.ColumnHeader headerSize;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuItemAddFiles;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem menuItemViewAddFiles;
		private System.Windows.Forms.ToolStripMenuItem menuItemRemoveFiles;
		private System.Windows.Forms.OpenFileDialog dialogAddFile;
		private System.Windows.Forms.ColumnHeader headerDateTime;
		private System.Windows.Forms.ToolStripMenuItem menuItemEditComment;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.Label labelPreview;
		private System.Windows.Forms.TextBox textPreview;
		private System.Windows.Forms.PictureBox pictureImage;
		private System.Windows.Forms.TextBox textComment;
		private System.Windows.Forms.Label labelComment;
		private System.Windows.Forms.TextBox textProperties;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textLocation;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textFileName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureIcon;
		private System.Windows.Forms.WebBrowser htmlPreview;
        private System.Windows.Forms.ToolStrip stripMenu;
        private System.Windows.Forms.ToolStripButton buttonChangeAuth;
		private System.Windows.Forms.ToolStripButton buttonPurge;
		private System.Windows.Forms.ToolStripMenuItem menuItemCopyToExplorer;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem menuItemCopyPathToExplorer;
		private System.Windows.Forms.FolderBrowserDialog dialogFolders;
		private System.Windows.Forms.ToolStripButton buttonCopyToExplorer;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
	}
}