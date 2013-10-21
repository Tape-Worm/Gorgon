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

namespace GorgonLibrary.Editor
{
	partial class formEditorFileSelector
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formEditorFileSelector));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.splitMainPanes = new System.Windows.Forms.SplitContainer();
			this.splitFiles = new System.Windows.Forms.SplitContainer();
			this.treeDirectories = new GorgonLibrary.Editor.EditorTreeView();
			this.listFiles = new System.Windows.Forms.ListView();
			this.columnFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imagesFilesLarge = new System.Windows.Forms.ImageList(this.components);
			this.imagesFilesSmall = new System.Windows.Forms.ImageList(this.components);
			this.panelTopControls = new System.Windows.Forms.Panel();
			this.panelTools = new System.Windows.Forms.Panel();
			this.stripFileFunctions = new System.Windows.Forms.ToolStrip();
			this.buttonGoUp = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonView = new System.Windows.Forms.ToolStripDropDownButton();
			this.itemViewDetails = new System.Windows.Forms.ToolStripMenuItem();
			this.itemViewLarge = new System.Windows.Forms.ToolStripMenuItem();
			this.panelSearch = new System.Windows.Forms.Panel();
			this.buttonSearch = new System.Windows.Forms.Button();
			this.textSearch = new GorgonLibrary.Editor.formEditorFileSelector.CuedTextBox();
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
			this.buttonCancel.Image = global::GorgonLibrary.Editor.Properties.Resources.cancel_16x16;
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
			this.buttonOK.Image = global::GorgonLibrary.Editor.Properties.Resources.ok_16x16;
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
			this.splitFiles.Panel2.Controls.Add(this.panelTopControls);
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
			this.listFiles.LargeImageList = this.imagesFilesLarge;
			this.listFiles.Location = new System.Drawing.Point(0, 25);
			this.listFiles.Name = "listFiles";
			this.listFiles.Size = new System.Drawing.Size(604, 366);
			this.listFiles.SmallImageList = this.imagesFilesSmall;
			this.listFiles.TabIndex = 0;
			this.listFiles.UseCompatibleStateImageBehavior = false;
			this.listFiles.View = System.Windows.Forms.View.Details;
			this.listFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listFiles_ColumnClick);
			this.listFiles.DoubleClick += new System.EventHandler(this.listFiles_DoubleClick);
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
            this.buttonGoUp,
            this.toolStripSeparator1,
            this.buttonView});
			this.stripFileFunctions.Location = new System.Drawing.Point(0, 0);
			this.stripFileFunctions.Name = "stripFileFunctions";
			this.stripFileFunctions.Size = new System.Drawing.Size(404, 25);
			this.stripFileFunctions.Stretch = true;
			this.stripFileFunctions.TabIndex = 0;
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
			this.buttonView.Image = global::GorgonLibrary.Editor.Properties.Resources.choose_view_16x16;
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
			this.buttonSearch.Image = global::GorgonLibrary.Editor.Properties.Resources.find_16x16;
			this.buttonSearch.Location = new System.Drawing.Point(174, 0);
			this.buttonSearch.Name = "buttonSearch";
			this.buttonSearch.Size = new System.Drawing.Size(24, 23);
			this.buttonSearch.TabIndex = 1;
			this.buttonSearch.TabStop = false;
			this.buttonSearch.UseVisualStyleBackColor = false;
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
			this.comboFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.comboFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboFile.FormattingEnabled = true;
			this.comboFile.Location = new System.Drawing.Point(46, 9);
			this.comboFile.Name = "comboFile";
			this.comboFile.Size = new System.Drawing.Size(543, 23);
			this.comboFile.TabIndex = 9;
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
			// formEditorFileSelector
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
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formEditorFileSelector";
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

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.SplitContainer splitMainPanes;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.SplitContainer splitFiles;
		private EditorTreeView treeDirectories;
		private System.Windows.Forms.ListView listFiles;
		private System.Windows.Forms.ToolStrip stripFileFunctions;
		private System.Windows.Forms.Panel panelTopControls;
		private System.Windows.Forms.Panel panelTools;
		private System.Windows.Forms.Panel panelSearch;
		private System.Windows.Forms.Button buttonSearch;
		private GorgonLibrary.Editor.formEditorFileSelector.CuedTextBox textSearch;
		private System.Windows.Forms.ComboBox comboFile;
		private System.Windows.Forms.ColumnHeader columnFileName;
		private System.Windows.Forms.ColumnHeader columnDate;
		private System.Windows.Forms.ColumnHeader columnSize;
		private System.Windows.Forms.Panel panelFilters;
		private System.Windows.Forms.ComboBox comboFilters;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ImageList imagesFilesSmall;
		private System.Windows.Forms.ImageList imagesFilesLarge;
		private System.Windows.Forms.ToolStripDropDownButton buttonView;
		private System.Windows.Forms.ToolStripMenuItem itemViewDetails;
		private System.Windows.Forms.ToolStripMenuItem itemViewLarge;
		private System.Windows.Forms.ToolStripButton buttonGoUp;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

	}
}