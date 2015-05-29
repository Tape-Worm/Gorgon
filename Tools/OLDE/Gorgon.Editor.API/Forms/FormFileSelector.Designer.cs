#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, October 16, 2013 3:09:59 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor
{
	partial class FormEditorFileSelector
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
				if (_cancelSource != null)
				{
					_cancelSource.Dispose();
				}
				_cancelSource = null;
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
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("/");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditorFileSelector));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.splitMainPanes = new System.Windows.Forms.SplitContainer();
			this.splitFiles = new System.Windows.Forms.SplitContainer();
			this.treeDirectories = new Gorgon.Editor.EditorTreeView();
			this.listFiles = new System.Windows.Forms.ListView();
			this.columnFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imagesFilesLarge = new System.Windows.Forms.ImageList(this.components);
			this.imagesFilesSmall = new System.Windows.Forms.ImageList(this.components);
			this.panelSearchLabel = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.labelSearchBanner = new System.Windows.Forms.Label();
			this.buttonCloseSearch = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panelTopControls = new System.Windows.Forms.Panel();
			this.panelTools = new System.Windows.Forms.Panel();
			this.stripFileFunctions = new System.Windows.Forms.ToolStrip();
			this.buttonBack = new System.Windows.Forms.ToolStripButton();
			this.buttonGoUp = new System.Windows.Forms.ToolStripButton();
			this.buttonForward = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonView = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemViewDetails = new System.Windows.Forms.ToolStripMenuItem();
			this.itemViewLarge = new System.Windows.Forms.ToolStripMenuItem();
			this.panelSearch = new System.Windows.Forms.Panel();
			this.buttonSearch = new System.Windows.Forms.Button();
			this.textSearch = new Gorgon.Editor.FormEditorFileSelector.CuedTextBox();
			this.labelNoFilesFound = new System.Windows.Forms.Label();
			this.panelFilters = new System.Windows.Forms.Panel();
			this.comboFilters = new System.Windows.Forms.ComboBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.comboFile = new System.Windows.Forms.ComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.splitMainPanes)).BeginInit();
			this.splitMainPanes.Panel1.SuspendLayout();
			this.splitMainPanes.Panel2.SuspendLayout();
			this.splitMainPanes.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitFiles)).BeginInit();
			this.splitFiles.Panel1.SuspendLayout();
			this.splitFiles.Panel2.SuspendLayout();
			this.splitFiles.SuspendLayout();
			this.panelSearchLabel.SuspendLayout();
			this.panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.panelTopControls.SuspendLayout();
			this.panelTools.SuspendLayout();
			this.stripFileFunctions.SuspendLayout();
			this.panelSearch.SuspendLayout();
			this.panelFilters.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCancel.Image = global::Gorgon.Editor.Properties.APIResources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(696, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 7;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonOK.Image = global::Gorgon.Editor.Properties.APIResources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(603, 3);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 6;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			// 
			// splitMainPanes
			// 
			this.splitMainPanes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitMainPanes.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitMainPanes.IsSplitterFixed = true;
			this.splitMainPanes.Location = new System.Drawing.Point(4, 28);
			this.splitMainPanes.Name = "splitMainPanes";
			this.splitMainPanes.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitMainPanes.Panel1
			// 
			this.splitMainPanes.Panel1.Controls.Add(this.splitFiles);
			// 
			// splitMainPanes.Panel2
			// 
			this.splitMainPanes.Panel2.Controls.Add(this.panelFilters);
			this.splitMainPanes.Panel2.Controls.Add(this.panel2);
			this.splitMainPanes.Panel2.Controls.Add(this.panel1);
			this.splitMainPanes.Size = new System.Drawing.Size(795, 471);
			this.splitMainPanes.SplitterDistance = 391;
			this.splitMainPanes.TabIndex = 8;
			// 
			// splitFiles
			// 
			this.splitFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitFiles.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitFiles.Location = new System.Drawing.Point(0, 0);
			this.splitFiles.Name = "splitFiles";
			// 
			// splitFiles.Panel1
			// 
			this.splitFiles.Panel1.Controls.Add(this.treeDirectories);
			// 
			// splitFiles.Panel2
			// 
			this.splitFiles.Panel2.Controls.Add(this.listFiles);
			this.splitFiles.Panel2.Controls.Add(this.panelSearchLabel);
			this.splitFiles.Panel2.Controls.Add(this.panelTopControls);
			this.splitFiles.Panel2.Controls.Add(this.labelNoFilesFound);
			this.splitFiles.Size = new System.Drawing.Size(795, 391);
			this.splitFiles.SplitterDistance = 187;
			this.splitFiles.TabIndex = 0;
			// 
			// treeDirectories
			// 
			this.treeDirectories.AllowDrop = true;
			this.treeDirectories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.treeDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeDirectories.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeDirectories.ForeColor = System.Drawing.Color.White;
			this.treeDirectories.FullRowSelect = true;
			this.treeDirectories.HideSelection = false;
			this.treeDirectories.Location = new System.Drawing.Point(0, 0);
			this.treeDirectories.Name = "treeDirectories";
			treeNode1.Name = "nodeDummy";
			treeNode1.Text = "/";
			this.treeDirectories.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
			this.treeDirectories.SelectedNode = null;
			this.treeDirectories.ShowLines = false;
			this.treeDirectories.Size = new System.Drawing.Size(187, 391);
			this.treeDirectories.Sorted = true;
			this.treeDirectories.TabIndex = 1;
			this.treeDirectories.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeDirectories_BeforeExpand);
			this.treeDirectories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeDirectories_AfterSelect);
			// 
			// listFiles
			// 
			this.listFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.listFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFileName,
            this.columnDate,
            this.columnSize});
			this.listFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listFiles.ForeColor = System.Drawing.Color.White;
			this.listFiles.FullRowSelect = true;
			this.listFiles.HideSelection = false;
			this.listFiles.LargeImageList = this.imagesFilesLarge;
			this.listFiles.Location = new System.Drawing.Point(0, 49);
			this.listFiles.Name = "listFiles";
			this.listFiles.Size = new System.Drawing.Size(604, 342);
			this.listFiles.SmallImageList = this.imagesFilesSmall;
			this.listFiles.TabIndex = 0;
			this.listFiles.UseCompatibleStateImageBehavior = false;
			this.listFiles.View = System.Windows.Forms.View.Details;
			this.listFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listFiles_ColumnClick);
			this.listFiles.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listFiles_ItemSelectionChanged);
			this.listFiles.DoubleClick += new System.EventHandler(this.listFiles_DoubleClick);
			this.listFiles.Enter += new System.EventHandler(this.listFiles_Enter);
			this.listFiles.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listFiles_KeyUp);
			this.listFiles.Leave += new System.EventHandler(this.listFiles_Leave);
			// 
			// columnFileName
			// 
			this.columnFileName.Text = "Name";
			// 
			// columnDate
			// 
			this.columnDate.Text = "Date";
			// 
			// columnSize
			// 
			this.columnSize.Text = "Size";
			// 
			// imagesFilesLarge
			// 
			this.imagesFilesLarge.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagesFilesLarge.ImageStream")));
			this.imagesFilesLarge.TransparentColor = System.Drawing.Color.Transparent;
			this.imagesFilesLarge.Images.SetKeyName(0, "directory");
			this.imagesFilesLarge.Images.SetKeyName(1, "directory_wfiles");
			this.imagesFilesLarge.Images.SetKeyName(2, "unknown_file");
			// 
			// imagesFilesSmall
			// 
			this.imagesFilesSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagesFilesSmall.ImageStream")));
			this.imagesFilesSmall.TransparentColor = System.Drawing.Color.Transparent;
			this.imagesFilesSmall.Images.SetKeyName(0, "directory");
			this.imagesFilesSmall.Images.SetKeyName(1, "directory_wfiles");
			this.imagesFilesSmall.Images.SetKeyName(2, "unknown_file");
			// 
			// panelSearchLabel
			// 
			this.panelSearchLabel.Controls.Add(this.panel4);
			this.panelSearchLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelSearchLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panelSearchLabel.Location = new System.Drawing.Point(0, 25);
			this.panelSearchLabel.Name = "panelSearchLabel";
			this.panelSearchLabel.Size = new System.Drawing.Size(604, 24);
			this.panelSearchLabel.TabIndex = 4;
			this.panelSearchLabel.Visible = false;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.SystemColors.Info;
			this.panel4.Controls.Add(this.labelSearchBanner);
			this.panel4.Controls.Add(this.buttonCloseSearch);
			this.panel4.Controls.Add(this.pictureBox1);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.ForeColor = System.Drawing.SystemColors.InfoText;
			this.panel4.Location = new System.Drawing.Point(0, 0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(604, 24);
			this.panel4.TabIndex = 0;
			// 
			// labelSearchBanner
			// 
			this.labelSearchBanner.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelSearchBanner.ForeColor = System.Drawing.SystemColors.InfoText;
			this.labelSearchBanner.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelSearchBanner.Location = new System.Drawing.Point(24, 0);
			this.labelSearchBanner.Name = "labelSearchBanner";
			this.labelSearchBanner.Size = new System.Drawing.Size(555, 24);
			this.labelSearchBanner.TabIndex = 3;
			this.labelSearchBanner.Text = "Files returned from search";
			this.labelSearchBanner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonCloseSearch
			// 
			this.buttonCloseSearch.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonCloseSearch.FlatAppearance.BorderSize = 0;
			this.buttonCloseSearch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
			this.buttonCloseSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
			this.buttonCloseSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCloseSearch.Font = new System.Drawing.Font("Marlett", 8.25F);
			this.buttonCloseSearch.Location = new System.Drawing.Point(579, 0);
			this.buttonCloseSearch.Name = "buttonCloseSearch";
			this.buttonCloseSearch.Size = new System.Drawing.Size(25, 24);
			this.buttonCloseSearch.TabIndex = 0;
			this.buttonCloseSearch.Text = "r";
			this.buttonCloseSearch.Click += new System.EventHandler(this.buttonCloseSearch_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
			this.pictureBox1.Image = global::Gorgon.Editor.Properties.APIResources.find_16x16;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(24, 24);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			// 
			// panelTopControls
			// 
			this.panelTopControls.AutoSize = true;
			this.panelTopControls.Controls.Add(this.panelTools);
			this.panelTopControls.Controls.Add(this.panelSearch);
			this.panelTopControls.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTopControls.Location = new System.Drawing.Point(0, 0);
			this.panelTopControls.Name = "panelTopControls";
			this.panelTopControls.Size = new System.Drawing.Size(604, 25);
			this.panelTopControls.TabIndex = 2;
			// 
			// panelTools
			// 
			this.panelTools.AutoSize = true;
			this.panelTools.Controls.Add(this.stripFileFunctions);
			this.panelTools.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTools.Location = new System.Drawing.Point(0, 0);
			this.panelTools.Name = "panelTools";
			this.panelTools.Size = new System.Drawing.Size(404, 25);
			this.panelTools.TabIndex = 0;
			// 
			// stripFileFunctions
			// 
			this.stripFileFunctions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripFileFunctions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonBack,
            this.buttonGoUp,
            this.buttonForward,
            this.toolStripSeparator1,
            this.buttonView});
			this.stripFileFunctions.Location = new System.Drawing.Point(0, 0);
			this.stripFileFunctions.Name = "stripFileFunctions";
			this.stripFileFunctions.Size = new System.Drawing.Size(404, 25);
			this.stripFileFunctions.Stretch = true;
			this.stripFileFunctions.TabIndex = 0;
			// 
			// buttonBack
			// 
			this.buttonBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonBack.Enabled = false;
			this.buttonBack.Image = global::Gorgon.Editor.Properties.APIResources.back_16x16png;
			this.buttonBack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonBack.Name = "buttonBack";
			this.buttonBack.Size = new System.Drawing.Size(23, 22);
			this.buttonBack.Text = "Back";
			this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
			// 
			// buttonGoUp
			// 
			this.buttonGoUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonGoUp.Enabled = false;
			this.buttonGoUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonGoUp.Image")));
			this.buttonGoUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGoUp.Name = "buttonGoUp";
			this.buttonGoUp.Size = new System.Drawing.Size(23, 22);
			this.buttonGoUp.Text = "Go up.";
			this.buttonGoUp.Click += new System.EventHandler(this.buttonGoUp_Click);
			// 
			// buttonForward
			// 
			this.buttonForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonForward.Enabled = false;
			this.buttonForward.Image = global::Gorgon.Editor.Properties.APIResources.forward_16x16;
			this.buttonForward.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonForward.Name = "buttonForward";
			this.buttonForward.Size = new System.Drawing.Size(23, 22);
			this.buttonForward.Text = "Foreward";
			this.buttonForward.Click += new System.EventHandler(this.buttonForward_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonView
			// 
			this.buttonView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemViewDetails,
            this.itemViewLarge});
			this.buttonView.Image = global::Gorgon.Editor.Properties.APIResources.choose_view_16x16;
			this.buttonView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonView.Name = "buttonView";
			this.buttonView.Size = new System.Drawing.Size(29, 22);
			this.buttonView.Text = "Change file views.";
			// 
			// itemViewDetails
			// 
			this.itemViewDetails.Checked = true;
			this.itemViewDetails.CheckState = System.Windows.Forms.CheckState.Checked;
			this.itemViewDetails.Name = "itemViewDetails";
			this.itemViewDetails.Size = new System.Drawing.Size(109, 22);
			this.itemViewDetails.Text = "&Details";
			this.itemViewDetails.Click += new System.EventHandler(this.itemViewLarge_Click);
			// 
			// itemViewLarge
			// 
			this.itemViewLarge.Name = "itemViewLarge";
			this.itemViewLarge.Size = new System.Drawing.Size(109, 22);
			this.itemViewLarge.Text = "Lar&ge";
			this.itemViewLarge.Click += new System.EventHandler(this.itemViewLarge_Click);
			// 
			// panelSearch
			// 
			this.panelSearch.BackColor = System.Drawing.Color.White;
			this.panelSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSearch.Controls.Add(this.buttonSearch);
			this.panelSearch.Controls.Add(this.textSearch);
			this.panelSearch.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelSearch.Location = new System.Drawing.Point(404, 0);
			this.panelSearch.Name = "panelSearch";
			this.panelSearch.Size = new System.Drawing.Size(200, 25);
			this.panelSearch.TabIndex = 1;
			// 
			// buttonSearch
			// 
			this.buttonSearch.BackColor = System.Drawing.Color.White;
			this.buttonSearch.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonSearch.FlatAppearance.BorderSize = 0;
			this.buttonSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.buttonSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonSearch.Image = global::Gorgon.Editor.Properties.APIResources.find_16x16;
			this.buttonSearch.Location = new System.Drawing.Point(174, 0);
			this.buttonSearch.Name = "buttonSearch";
			this.buttonSearch.Size = new System.Drawing.Size(24, 23);
			this.buttonSearch.TabIndex = 1;
			this.buttonSearch.UseVisualStyleBackColor = false;
			this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
			this.buttonSearch.Enter += new System.EventHandler(this.textSearch_Enter);
			this.buttonSearch.Leave += new System.EventHandler(this.textSearch_Leave);
			// 
			// textSearch
			// 
			this.textSearch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.textSearch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.textSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textSearch.CueText = "Search or something";
			this.textSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textSearch.Location = new System.Drawing.Point(3, 4);
			this.textSearch.Name = "textSearch";
			this.textSearch.Size = new System.Drawing.Size(172, 16);
			this.textSearch.TabIndex = 0;
			this.textSearch.Enter += new System.EventHandler(this.textSearch_Enter);
			this.textSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textSearch_KeyUp);
			this.textSearch.Leave += new System.EventHandler(this.textSearch_Leave);
			// 
			// labelNoFilesFound
			// 
			this.labelNoFilesFound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelNoFilesFound.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNoFilesFound.Location = new System.Drawing.Point(0, 0);
			this.labelNoFilesFound.Name = "labelNoFilesFound";
			this.labelNoFilesFound.Size = new System.Drawing.Size(604, 391);
			this.labelNoFilesFound.TabIndex = 5;
			this.labelNoFilesFound.Text = "No files";
			this.labelNoFilesFound.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelNoFilesFound.Visible = false;
			// 
			// panelFilters
			// 
			this.panelFilters.Controls.Add(this.comboFilters);
			this.panelFilters.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelFilters.Location = new System.Drawing.Point(595, 0);
			this.panelFilters.Name = "panelFilters";
			this.panelFilters.Size = new System.Drawing.Size(200, 38);
			this.panelFilters.TabIndex = 13;
			// 
			// comboFilters
			// 
			this.comboFilters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFilters.DropDownWidth = 360;
			this.comboFilters.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboFilters.FormattingEnabled = true;
			this.comboFilters.Location = new System.Drawing.Point(8, 9);
			this.comboFilters.Name = "comboFilters";
			this.comboFilters.Size = new System.Drawing.Size(180, 23);
			this.comboFilters.TabIndex = 11;
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.comboFile);
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(597, 38);
			this.panel2.TabIndex = 12;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(28, 15);
			this.label1.TabIndex = 8;
			this.label1.Text = "File:";
			// 
			// comboFile
			// 
			this.comboFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.comboFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.comboFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboFile.Location = new System.Drawing.Point(46, 9);
			this.comboFile.Name = "comboFile";
			this.comboFile.Size = new System.Drawing.Size(543, 23);
			this.comboFile.TabIndex = 9;
			this.comboFile.TextChanged += new System.EventHandler(this.comboFile_TextChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonOK);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 38);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(795, 38);
			this.panel1.TabIndex = 11;
			// 
			// FormEditorFileSelector
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(803, 503);
			this.Controls.Add(this.splitMainPanes);
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEditorFileSelector";
			this.Padding = new System.Windows.Forms.Padding(4);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "File Selection";
			this.Controls.SetChildIndex(this.splitMainPanes, 0);
			this.splitMainPanes.Panel1.ResumeLayout(false);
			this.splitMainPanes.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitMainPanes)).EndInit();
			this.splitMainPanes.ResumeLayout(false);
			this.splitFiles.Panel1.ResumeLayout(false);
			this.splitFiles.Panel2.ResumeLayout(false);
			this.splitFiles.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitFiles)).EndInit();
			this.splitFiles.ResumeLayout(false);
			this.panelSearchLabel.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.panelTopControls.ResumeLayout(false);
			this.panelTopControls.PerformLayout();
			this.panelTools.ResumeLayout(false);
			this.panelTools.PerformLayout();
			this.stripFileFunctions.ResumeLayout(false);
			this.stripFileFunctions.PerformLayout();
			this.panelSearch.ResumeLayout(false);
			this.panelSearch.PerformLayout();
			this.panelFilters.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Button buttonCancel;
		private Button buttonOK;
		private SplitContainer splitMainPanes;
		private Label label1;
		private SplitContainer splitFiles;
		private EditorTreeView treeDirectories;
		private ListView listFiles;
		private ToolStrip stripFileFunctions;
		private Panel panelTopControls;
		private Panel panelTools;
		private Panel panelSearch;
		private Button buttonSearch;
		private CuedTextBox textSearch;
		private ComboBox comboFile;
		private ColumnHeader columnFileName;
		private ColumnHeader columnDate;
		private ColumnHeader columnSize;
		private Panel panelFilters;
		private ComboBox comboFilters;
		private Panel panel2;
		private Panel panel1;
		private ImageList imagesFilesSmall;
		private ImageList imagesFilesLarge;
		private ToolStripDropDownButton buttonView;
		private ToolStripMenuItem itemViewDetails;
		private ToolStripMenuItem itemViewLarge;
		private ToolStripButton buttonGoUp;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton buttonBack;
		private ToolStripButton buttonForward;
        private Panel panelSearchLabel;
        private Panel panel4;
        private Label labelSearchBanner;
        private Button buttonCloseSearch;
        private PictureBox pictureBox1;
		private Label labelNoFilesFound;

	}
}