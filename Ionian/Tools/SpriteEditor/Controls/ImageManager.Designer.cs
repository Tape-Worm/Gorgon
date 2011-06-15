namespace GorgonLibrary.Graphics.Tools.Controls
{
	partial class ImageManager
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

			if ((!DesignMode) && (disposing))
			{
				if (_image != null)
					_image.Dispose();
				_image = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageManager));
			this.containerImageManager = new System.Windows.Forms.ToolStripContainer();
			this.splitImageList = new System.Windows.Forms.SplitContainer();
			this.listImages = new System.Windows.Forms.ListView();
			this.headerName = new System.Windows.Forms.ColumnHeader();
			this.headerSize = new System.Windows.Forms.ColumnHeader();
			this.headerFormat = new System.Windows.Forms.ColumnHeader();
			this.popupListMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRemoveImage = new System.Windows.Forms.ToolStripMenuItem();
			this.picturePreview = new System.Windows.Forms.PictureBox();
			this.stripImage = new System.Windows.Forms.ToolStrip();
			this.buttonOpenImage = new System.Windows.Forms.ToolStripButton();
			this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEditImage = new System.Windows.Forms.ToolStripButton();
			this.buttonRemoveImages = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonExtractSprites = new System.Windows.Forms.ToolStripButton();
			this.buttonGridExtract = new System.Windows.Forms.ToolStripButton();
			this.dialogOpenImage = new System.Windows.Forms.OpenFileDialog();
			this.containerImageManager.ContentPanel.SuspendLayout();
			this.containerImageManager.TopToolStripPanel.SuspendLayout();
			this.containerImageManager.SuspendLayout();
			this.splitImageList.Panel1.SuspendLayout();
			this.splitImageList.Panel2.SuspendLayout();
			this.splitImageList.SuspendLayout();
			this.popupListMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picturePreview)).BeginInit();
			this.stripImage.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelManager
			// 
			this.labelManager.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.labelManager.Text = "Image Manager";
			// 
			// containerImageManager
			// 
			// 
			// containerImageManager.ContentPanel
			// 
			this.containerImageManager.ContentPanel.Controls.Add(this.splitImageList);
			this.containerImageManager.ContentPanel.Margin = new System.Windows.Forms.Padding(2);
			this.containerImageManager.ContentPanel.Size = new System.Drawing.Size(464, 383);
			this.containerImageManager.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerImageManager.Location = new System.Drawing.Point(0, 19);
			this.containerImageManager.Margin = new System.Windows.Forms.Padding(2);
			this.containerImageManager.Name = "containerImageManager";
			this.containerImageManager.Size = new System.Drawing.Size(464, 408);
			this.containerImageManager.TabIndex = 4;
			this.containerImageManager.Text = "toolStripContainer1";
			// 
			// containerImageManager.TopToolStripPanel
			// 
			this.containerImageManager.TopToolStripPanel.Controls.Add(this.stripImage);
			// 
			// splitImageList
			// 
			this.splitImageList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitImageList.Location = new System.Drawing.Point(0, 0);
			this.splitImageList.Margin = new System.Windows.Forms.Padding(2);
			this.splitImageList.Name = "splitImageList";
			this.splitImageList.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitImageList.Panel1
			// 
			this.splitImageList.Panel1.Controls.Add(this.listImages);
			// 
			// splitImageList.Panel2
			// 
			this.splitImageList.Panel2.Controls.Add(this.picturePreview);
			this.splitImageList.Size = new System.Drawing.Size(464, 383);
			this.splitImageList.SplitterDistance = 255;
			this.splitImageList.SplitterWidth = 3;
			this.splitImageList.TabIndex = 0;
			this.splitImageList.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitImageList_SplitterMoving);
			this.splitImageList.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitImageList_SplitterMoved);
			// 
			// listImages
			// 
			this.listImages.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listImages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerName,
            this.headerSize,
            this.headerFormat});
			this.listImages.ContextMenuStrip = this.popupListMenu;
			this.listImages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listImages.FullRowSelect = true;
			this.listImages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listImages.HideSelection = false;
			this.listImages.Location = new System.Drawing.Point(0, 0);
			this.listImages.Name = "listImages";
			this.listImages.Size = new System.Drawing.Size(464, 255);
			this.listImages.TabIndex = 3;
			this.listImages.UseCompatibleStateImageBehavior = false;
			this.listImages.View = System.Windows.Forms.View.Details;
			this.listImages.SelectedIndexChanged += new System.EventHandler(this.listImages_SelectedIndexChanged);
			// 
			// headerName
			// 
			this.headerName.Text = "Name";
			// 
			// headerSize
			// 
			this.headerSize.Text = "Size";
			// 
			// headerFormat
			// 
			this.headerFormat.Text = "Format";
			// 
			// popupListMenu
			// 
			this.popupListMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemEdit,
            this.menuItemRemoveImage});
			this.popupListMenu.Name = "popupListMenu";
			this.popupListMenu.Size = new System.Drawing.Size(247, 70);
			// 
			// menuItemEdit
			// 
			this.menuItemEdit.Enabled = false;
			this.menuItemEdit.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.menuItemEdit.Name = "menuItemEdit";
			this.menuItemEdit.Size = new System.Drawing.Size(246, 22);
			this.menuItemEdit.Text = "Edit with registered application...";
			this.menuItemEdit.Click += new System.EventHandler(this.buttonEditImage_Click);
			// 
			// menuItemRemoveImage
			// 
			this.menuItemRemoveImage.Enabled = false;
			this.menuItemRemoveImage.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.menuItemRemoveImage.Name = "menuItemRemoveImage";
			this.menuItemRemoveImage.Size = new System.Drawing.Size(246, 22);
			this.menuItemRemoveImage.Text = "Remove image(s)...";
			this.menuItemRemoveImage.Click += new System.EventHandler(this.buttonRemoveImages_Click);
			// 
			// picturePreview
			// 
			this.picturePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.picturePreview.InitialImage = global::GorgonLibrary.Graphics.Tools.Properties.Resources.GorSprite_64;
			this.picturePreview.Location = new System.Drawing.Point(0, 0);
			this.picturePreview.Name = "picturePreview";
			this.picturePreview.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.picturePreview.Size = new System.Drawing.Size(464, 125);
			this.picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.picturePreview.TabIndex = 1;
			this.picturePreview.TabStop = false;
			// 
			// stripImage
			// 
			this.stripImage.Dock = System.Windows.Forms.DockStyle.None;
			this.stripImage.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonOpenImage,
            this.buttonRefresh,
            this.toolStripSeparator1,
            this.buttonEditImage,
            this.buttonRemoveImages,
            this.toolStripSeparator2,
            this.buttonExtractSprites,
            this.buttonGridExtract});
			this.stripImage.Location = new System.Drawing.Point(0, 0);
			this.stripImage.Name = "stripImage";
			this.stripImage.Size = new System.Drawing.Size(464, 25);
			this.stripImage.Stretch = true;
			this.stripImage.TabIndex = 2;
			// 
			// buttonOpenImage
			// 
			this.buttonOpenImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonOpenImage.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.folder_out;
			this.buttonOpenImage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonOpenImage.Name = "buttonOpenImage";
			this.buttonOpenImage.Size = new System.Drawing.Size(23, 22);
			this.buttonOpenImage.Text = "Load image(s).";
			this.buttonOpenImage.Click += new System.EventHandler(this.buttonOpenImage_Click);
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRefresh.Enabled = false;
			this.buttonRefresh.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.recycle;
			this.buttonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.Size = new System.Drawing.Size(23, 22);
			this.buttonRefresh.Text = "Reload the selected image(s).";
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonEditImage
			// 
			this.buttonEditImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonEditImage.Enabled = false;
			this.buttonEditImage.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.edit;
			this.buttonEditImage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEditImage.Name = "buttonEditImage";
			this.buttonEditImage.Size = new System.Drawing.Size(23, 22);
			this.buttonEditImage.Text = "Edit the image with the associated application.";
			this.buttonEditImage.Click += new System.EventHandler(this.buttonEditImage_Click);
			// 
			// buttonRemoveImages
			// 
			this.buttonRemoveImages.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRemoveImages.Enabled = false;
			this.buttonRemoveImages.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.buttonRemoveImages.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRemoveImages.Name = "buttonRemoveImages";
			this.buttonRemoveImages.Size = new System.Drawing.Size(23, 22);
			this.buttonRemoveImages.Text = "Remove selected image(s).";
			this.buttonRemoveImages.Click += new System.EventHandler(this.buttonRemoveImages_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonExtractSprites
			// 
			this.buttonExtractSprites.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonExtractSprites.Enabled = false;
			this.buttonExtractSprites.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.box_out;
			this.buttonExtractSprites.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonExtractSprites.Name = "buttonExtractSprites";
			this.buttonExtractSprites.Size = new System.Drawing.Size(23, 22);
			this.buttonExtractSprites.Text = "Extract sprites from image...";
			this.buttonExtractSprites.Click += new System.EventHandler(this.buttonExtractSprites_Click);
			// 
			// buttonGridExtract
			// 
			this.buttonGridExtract.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonGridExtract.Enabled = false;
			this.buttonGridExtract.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.extract_grid;
			this.buttonGridExtract.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGridExtract.Name = "buttonGridExtract";
			this.buttonGridExtract.Size = new System.Drawing.Size(23, 22);
			this.buttonGridExtract.Text = "Extract sprites by grid.";
			this.buttonGridExtract.Click += new System.EventHandler(this.buttonGridExtract_Click);
			// 
			// dialogOpenImage
			// 
			this.dialogOpenImage.Filter = resources.GetString("dialogOpenImage.Filter");
			this.dialogOpenImage.InitialDirectory = ".\\";
			this.dialogOpenImage.Multiselect = true;
			this.dialogOpenImage.RestoreDirectory = true;
			this.dialogOpenImage.Title = "Select an image to load...";
			// 
			// ImageManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.containerImageManager);
			this.Name = "ImageManager";
			this.Controls.SetChildIndex(this.containerImageManager, 0);
			this.containerImageManager.ContentPanel.ResumeLayout(false);
			this.containerImageManager.TopToolStripPanel.ResumeLayout(false);
			this.containerImageManager.TopToolStripPanel.PerformLayout();
			this.containerImageManager.ResumeLayout(false);
			this.containerImageManager.PerformLayout();
			this.splitImageList.Panel1.ResumeLayout(false);
			this.splitImageList.Panel2.ResumeLayout(false);
			this.splitImageList.ResumeLayout(false);
			this.popupListMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.picturePreview)).EndInit();
			this.stripImage.ResumeLayout(false);
			this.stripImage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer containerImageManager;
		private System.Windows.Forms.ToolStrip stripImage;
		private System.Windows.Forms.ToolStripButton buttonOpenImage;
		private System.Windows.Forms.ToolStripButton buttonEditImage;
		private System.Windows.Forms.ToolStripButton buttonRemoveImages;
		private System.Windows.Forms.SplitContainer splitImageList;
		private System.Windows.Forms.ListView listImages;
		private System.Windows.Forms.ColumnHeader headerName;
		private System.Windows.Forms.ColumnHeader headerSize;
		private System.Windows.Forms.ColumnHeader headerFormat;
		private System.Windows.Forms.PictureBox picturePreview;
		private System.Windows.Forms.OpenFileDialog dialogOpenImage;
		private System.Windows.Forms.ContextMenuStrip popupListMenu;
		private System.Windows.Forms.ToolStripMenuItem menuItemEdit;
		private System.Windows.Forms.ToolStripMenuItem menuItemRemoveImage;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton buttonExtractSprites;
		private System.Windows.Forms.ToolStripButton buttonRefresh;
		private System.Windows.Forms.ToolStripButton buttonGridExtract;
	}
}
