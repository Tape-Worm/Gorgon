﻿namespace GorgonLibrary.UI
{
	partial class FlatForm
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
				if (_iconImage != null)
				{
					_iconImage.Dispose();
					_iconImage = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlatForm));
			this.labelClose = new System.Windows.Forms.Label();
			this.labelMaxRestore = new System.Windows.Forms.Label();
			this.labelMinimize = new System.Windows.Forms.Label();
			this.labelCaption = new System.Windows.Forms.Label();
			this.panelCaptionArea = new System.Windows.Forms.Panel();
			this.panelWinIcons = new System.Windows.Forms.FlowLayoutPanel();
			this.pictureIcon = new System.Windows.Forms.PictureBox();
			this.popupSysMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.itemRestore = new System.Windows.Forms.ToolStripMenuItem();
			this.itemMove = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSize = new System.Windows.Forms.ToolStripMenuItem();
			this.itemMinimize = new System.Windows.Forms.ToolStripMenuItem();
			this.itemMaximize = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.itemClose = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._panelContent = new System.Windows.Forms.Panel();
			this.panelCaptionArea.SuspendLayout();
			this.panelWinIcons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).BeginInit();
			this.popupSysMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelClose
			// 
			this.labelClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelClose.AutoSize = true;
			this.labelClose.Font = new System.Drawing.Font("Marlett", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
			this.labelClose.Location = new System.Drawing.Point(65, 0);
			this.labelClose.Name = "labelClose";
			this.labelClose.Size = new System.Drawing.Size(25, 17);
			this.labelClose.TabIndex = 2;
			this.labelClose.Text = "r";
			this.labelClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.toolTip.SetToolTip(this.labelClose, global::GorgonLibrary.Properties.Resources.GOR_ZUNE_CLOSE_TIP);
			this.labelClose.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelClose_MouseDown);
			this.labelClose.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelClose.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelClose.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			// 
			// labelMaxRestore
			// 
			this.labelMaxRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelMaxRestore.AutoSize = true;
			this.labelMaxRestore.Font = new System.Drawing.Font("Marlett", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
			this.labelMaxRestore.Location = new System.Drawing.Point(34, 0);
			this.labelMaxRestore.Name = "labelMaxRestore";
			this.labelMaxRestore.Size = new System.Drawing.Size(25, 17);
			this.labelMaxRestore.TabIndex = 1;
			this.labelMaxRestore.Text = "2";
			this.labelMaxRestore.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.toolTip.SetToolTip(this.labelMaxRestore, global::GorgonLibrary.Properties.Resources.GOR_ZUNE_MAXIMIZE_TIP);
			this.labelMaxRestore.Click += new System.EventHandler(this.labelMaxRestore_Click);
			this.labelMaxRestore.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelMaxRestore.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelMaxRestore.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			// 
			// labelMinimize
			// 
			this.labelMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelMinimize.AutoSize = true;
			this.labelMinimize.Font = new System.Drawing.Font("Marlett", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
			this.labelMinimize.Location = new System.Drawing.Point(3, 0);
			this.labelMinimize.Name = "labelMinimize";
			this.labelMinimize.Size = new System.Drawing.Size(25, 17);
			this.labelMinimize.TabIndex = 0;
			this.labelMinimize.Text = "0";
			this.labelMinimize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.toolTip.SetToolTip(this.labelMinimize, global::GorgonLibrary.Properties.Resources.GOR_ZUNE_MINIMIZE_ZIP);
			this.labelMinimize.Click += new System.EventHandler(this.labelMinimize_Click);
			this.labelMinimize.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelMinimize.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelMinimize.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			// 
			// labelCaption
			// 
			this.labelCaption.AutoSize = true;
			this.labelCaption.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelCaption.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCaption.Location = new System.Drawing.Point(24, 0);
			this.labelCaption.Name = "labelCaption";
			this.labelCaption.Size = new System.Drawing.Size(0, 25);
			this.labelCaption.TabIndex = 3;
			this.labelCaption.DoubleClick += new System.EventHandler(this.panelCaptionArea_DoubleClick);
			this.labelCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseDown);
			this.labelCaption.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseMove);
			// 
			// panelCaptionArea
			// 
			this.panelCaptionArea.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelCaptionArea.Controls.Add(this.panelWinIcons);
			this.panelCaptionArea.Controls.Add(this.labelCaption);
			this.panelCaptionArea.Controls.Add(this.pictureIcon);
			this.panelCaptionArea.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelCaptionArea.Location = new System.Drawing.Point(0, 0);
			this.panelCaptionArea.Margin = new System.Windows.Forms.Padding(0);
			this.panelCaptionArea.Name = "panelCaptionArea";
			this.panelCaptionArea.Size = new System.Drawing.Size(487, 24);
			this.panelCaptionArea.TabIndex = 4;
			this.panelCaptionArea.DoubleClick += new System.EventHandler(this.panelCaptionArea_DoubleClick);
			this.panelCaptionArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseDown);
			this.panelCaptionArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseMove);
			// 
			// panelWinIcons
			// 
			this.panelWinIcons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelWinIcons.AutoSize = true;
			this.panelWinIcons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelWinIcons.Controls.Add(this.labelMinimize);
			this.panelWinIcons.Controls.Add(this.labelMaxRestore);
			this.panelWinIcons.Controls.Add(this.labelClose);
			this.panelWinIcons.Location = new System.Drawing.Point(394, 0);
			this.panelWinIcons.Margin = new System.Windows.Forms.Padding(0);
			this.panelWinIcons.Name = "panelWinIcons";
			this.panelWinIcons.Size = new System.Drawing.Size(93, 17);
			this.panelWinIcons.TabIndex = 5;
			this.panelWinIcons.WrapContents = false;
			this.panelWinIcons.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelWinIcons_MouseDown);
			this.panelWinIcons.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelWinIcons_MouseMove);
			// 
			// pictureIcon
			// 
			this.pictureIcon.Dock = System.Windows.Forms.DockStyle.Left;
			this.pictureIcon.Location = new System.Drawing.Point(0, 0);
			this.pictureIcon.Margin = new System.Windows.Forms.Padding(0);
			this.pictureIcon.Name = "pictureIcon";
			this.pictureIcon.Size = new System.Drawing.Size(24, 24);
			this.pictureIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureIcon.TabIndex = 4;
			this.pictureIcon.TabStop = false;
			this.pictureIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureIcon_MouseDown);
			// 
			// popupSysMenu
			// 
			this.popupSysMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.popupSysMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemRestore,
            this.itemMove,
            this.itemSize,
            this.itemMinimize,
            this.itemMaximize,
            this.toolStripMenuItem1,
            this.itemClose});
			this.popupSysMenu.Name = "popupSysMenu";
			this.popupSysMenu.Size = new System.Drawing.Size(178, 166);
			// 
			// itemRestore
			// 
			this.itemRestore.Image = global::GorgonLibrary.Properties.Resources.Restore;
			this.itemRestore.Name = "itemRestore";
			this.itemRestore.Size = new System.Drawing.Size(177, 26);
			this.itemRestore.Text = global::GorgonLibrary.Properties.Resources.GOR_ZUNE_RESTORE;
			this.itemRestore.Click += new System.EventHandler(this.itemRestore_Click);
			// 
			// itemMove
			// 
			this.itemMove.Name = "itemMove";
			this.itemMove.Size = new System.Drawing.Size(177, 26);
			this.itemMove.Text = global::GorgonLibrary.Properties.Resources.GOR_ZUNE_MOVE;
			this.itemMove.Click += new System.EventHandler(this.itemMove_Click);
			// 
			// itemSize
			// 
			this.itemSize.Name = "itemSize";
			this.itemSize.Size = new System.Drawing.Size(177, 26);
			this.itemSize.Text = global::GorgonLibrary.Properties.Resources.GOR_ZUNE_SIZE;
			this.itemSize.Click += new System.EventHandler(this.itemSize_Click);
			// 
			// itemMinimize
			// 
			this.itemMinimize.Image = ((System.Drawing.Image)(resources.GetObject("itemMinimize.Image")));
			this.itemMinimize.Name = "itemMinimize";
			this.itemMinimize.Size = new System.Drawing.Size(177, 26);
			this.itemMinimize.Text = global::GorgonLibrary.Properties.Resources.GOR_ZUNE_MINIMIZE;
			this.itemMinimize.Click += new System.EventHandler(this.itemMinimize_Click);
			// 
			// itemMaximize
			// 
			this.itemMaximize.Image = ((System.Drawing.Image)(resources.GetObject("itemMaximize.Image")));
			this.itemMaximize.Name = "itemMaximize";
			this.itemMaximize.Size = new System.Drawing.Size(177, 26);
			this.itemMaximize.Text = global::GorgonLibrary.Properties.Resources.GOR_ZUNE_MAXIMIZE;
			this.itemMaximize.Click += new System.EventHandler(this.itemMaximize_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(174, 6);
			// 
			// itemClose
			// 
			this.itemClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.itemClose.Image = global::GorgonLibrary.Properties.Resources.Close;
			this.itemClose.Name = "itemClose";
			this.itemClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.itemClose.Size = new System.Drawing.Size(177, 26);
			this.itemClose.Text = global::GorgonLibrary.Properties.Resources.GOR_ZUNE_CLOSE;
			this.itemClose.Click += new System.EventHandler(this.itemClose_Click);
			// 
			// toolTip
			// 
			this.toolTip.AutoPopDelay = 5000;
			this.toolTip.BackColor = System.Drawing.Color.White;
			this.toolTip.InitialDelay = 1500;
			this.toolTip.ReshowDelay = 100;
			// 
			// _panelContent
			// 
			this._panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this._panelContent.Location = new System.Drawing.Point(0, 24);
			this._panelContent.Name = "_panelContent";
			this._panelContent.Size = new System.Drawing.Size(487, 432);
			this._panelContent.TabIndex = 5;
			this._panelContent.MouseDown += new System.Windows.Forms.MouseEventHandler(this._panelContent_MouseDown);
			this._panelContent.MouseMove += new System.Windows.Forms.MouseEventHandler(this._panelContent_MouseMove);
			// 
			// FlatForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(487, 456);
			this.Controls.Add(this._panelContent);
			this.Controls.Add(this.panelCaptionArea);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "FlatForm";
			this.Activated += new System.EventHandler(this.ZuneForm_Activated);
			this.Deactivate += new System.EventHandler(this.ZuneForm_Deactivate);
			this.Load += new System.EventHandler(this.ZuneForm_Load);
			this.PaddingChanged += new System.EventHandler(this.ZuneForm_PaddingChanged);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ZuneForm_Paint);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ZuneForm_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ZuneForm_MouseMove);
			this.Resize += new System.EventHandler(this.ZuneForm_Resize);
			this.panelCaptionArea.ResumeLayout(false);
			this.panelCaptionArea.PerformLayout();
			this.panelWinIcons.ResumeLayout(false);
			this.panelWinIcons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
			this.popupSysMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelClose;
		private System.Windows.Forms.Label labelMaxRestore;
		private System.Windows.Forms.Label labelMinimize;
		private System.Windows.Forms.Label labelCaption;
		private System.Windows.Forms.Panel panelCaptionArea;
		private System.Windows.Forms.PictureBox pictureIcon;
		private System.Windows.Forms.ToolStripMenuItem itemRestore;
		private System.Windows.Forms.ToolStripMenuItem itemMove;
		private System.Windows.Forms.ToolStripMenuItem itemSize;
		private System.Windows.Forms.ToolStripMenuItem itemMinimize;
		private System.Windows.Forms.ToolStripMenuItem itemMaximize;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem itemClose;
		private System.Windows.Forms.ContextMenuStrip popupSysMenu;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.FlowLayoutPanel panelWinIcons;
	}
}