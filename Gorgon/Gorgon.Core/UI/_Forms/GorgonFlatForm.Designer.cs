using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.UI
{
	partial class GorgonFlatForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GorgonFlatForm));
			this.labelClose = new System.Windows.Forms.Label();
			this.labelMaxRestore = new System.Windows.Forms.Label();
			this.labelMinimize = new System.Windows.Forms.Label();
			this.labelCaption = new System.Windows.Forms.Label();
			this.panelCaptionArea = new System.Windows.Forms.Panel();
			this.panelWinIcons = new System.Windows.Forms.TableLayoutPanel();
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
			resources.ApplyResources(this.labelClose, "labelClose");
			this.labelClose.Name = "labelClose";
			this.toolTip.SetToolTip(this.labelClose, resources.GetString("labelClose.ToolTip"));
			this.labelClose.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelClose_MouseDown);
			this.labelClose.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelClose.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelClose.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			// 
			// labelMaxRestore
			// 
			resources.ApplyResources(this.labelMaxRestore, "labelMaxRestore");
			this.labelMaxRestore.Name = "labelMaxRestore";
			this.toolTip.SetToolTip(this.labelMaxRestore, resources.GetString("labelMaxRestore.ToolTip"));
			this.labelMaxRestore.Click += new System.EventHandler(this.labelMaxRestore_Click);
			this.labelMaxRestore.MouseEnter += new System.EventHandler(this.labelMinimize_MouseEnter);
			this.labelMaxRestore.MouseLeave += new System.EventHandler(this.labelMinimize_MouseLeave);
			this.labelMaxRestore.MouseHover += new System.EventHandler(this.labelMinimize_MouseEnter);
			// 
			// labelMinimize
			// 
			resources.ApplyResources(this.labelMinimize, "labelMinimize");
			this.labelMinimize.Name = "labelMinimize";
			this.toolTip.SetToolTip(this.labelMinimize, resources.GetString("labelMinimize.ToolTip"));
			this.labelMinimize.Click += new System.EventHandler(this.labelMinimize_Click);
			this.labelMinimize.MouseEnter += new System.EventHandler(this.labelMinimize_MouseEnter);
			this.labelMinimize.MouseLeave += new System.EventHandler(this.labelMinimize_MouseLeave);
			this.labelMinimize.MouseHover += new System.EventHandler(this.labelMinimize_MouseEnter);
			// 
			// labelCaption
			// 
			resources.ApplyResources(this.labelCaption, "labelCaption");
			this.labelCaption.Name = "labelCaption";
			this.labelCaption.FontChanged += new System.EventHandler(this.labelCaption_TextChanged);
			this.labelCaption.TextChanged += new System.EventHandler(this.labelCaption_TextChanged);
			this.labelCaption.DoubleClick += new System.EventHandler(this.panelCaptionArea_DoubleClick);
			this.labelCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseDown);
			this.labelCaption.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseMove);
			// 
			// panelCaptionArea
			// 
			resources.ApplyResources(this.panelCaptionArea, "panelCaptionArea");
			this.panelCaptionArea.Controls.Add(this.labelCaption);
			this.panelCaptionArea.Controls.Add(this.panelWinIcons);
			this.panelCaptionArea.Controls.Add(this.pictureIcon);
			this.panelCaptionArea.Name = "panelCaptionArea";
			this.panelCaptionArea.DoubleClick += new System.EventHandler(this.panelCaptionArea_DoubleClick);
			this.panelCaptionArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseDown);
			this.panelCaptionArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseMove);
			// 
			// panelWinIcons
			// 
			resources.ApplyResources(this.panelWinIcons, "panelWinIcons");
			this.panelWinIcons.Controls.Add(this.labelClose, 2, 0);
			this.panelWinIcons.Controls.Add(this.labelMaxRestore, 1, 0);
			this.panelWinIcons.Controls.Add(this.labelMinimize, 0, 0);
			this.panelWinIcons.Name = "panelWinIcons";
			this.panelWinIcons.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelWinIcons_MouseDown);
			this.panelWinIcons.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelWinIcons_MouseMove);
			// 
			// pictureIcon
			// 
			resources.ApplyResources(this.pictureIcon, "pictureIcon");
			this.pictureIcon.Name = "pictureIcon";
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
			resources.ApplyResources(this.popupSysMenu, "popupSysMenu");
			// 
			// itemRestore
			// 
			this.itemRestore.Image = global::Gorgon.Core.Properties.Resources.Restore;
			this.itemRestore.Name = "itemRestore";
			resources.ApplyResources(this.itemRestore, "itemRestore");
			this.itemRestore.Click += new System.EventHandler(this.itemRestore_Click);
			// 
			// itemMove
			// 
			this.itemMove.Name = "itemMove";
			resources.ApplyResources(this.itemMove, "itemMove");
			this.itemMove.Click += new System.EventHandler(this.itemMove_Click);
			// 
			// itemSize
			// 
			this.itemSize.Name = "itemSize";
			resources.ApplyResources(this.itemSize, "itemSize");
			this.itemSize.Click += new System.EventHandler(this.itemSize_Click);
			// 
			// itemMinimize
			// 
			resources.ApplyResources(this.itemMinimize, "itemMinimize");
			this.itemMinimize.Name = "itemMinimize";
			this.itemMinimize.Click += new System.EventHandler(this.itemMinimize_Click);
			// 
			// itemMaximize
			// 
			resources.ApplyResources(this.itemMaximize, "itemMaximize");
			this.itemMaximize.Name = "itemMaximize";
			this.itemMaximize.Click += new System.EventHandler(this.itemMaximize_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
			// 
			// itemClose
			// 
			resources.ApplyResources(this.itemClose, "itemClose");
			this.itemClose.Image = global::Gorgon.Core.Properties.Resources.Close;
			this.itemClose.Name = "itemClose";
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
			resources.ApplyResources(this._panelContent, "_panelContent");
			this._panelContent.Name = "_panelContent";
			this._panelContent.MouseDown += new System.Windows.Forms.MouseEventHandler(this._panelContent_MouseDown);
			this._panelContent.MouseMove += new System.Windows.Forms.MouseEventHandler(this._panelContent_MouseMove);
			// 
			// GorgonFlatForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._panelContent);
			this.Controls.Add(this.panelCaptionArea);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "GorgonFlatForm";
			this.Activated += new System.EventHandler(this.Form_Activated);
			this.Deactivate += new System.EventHandler(this.Form_Deactivate);
			this.Load += new System.EventHandler(this.Form_Load);
			this.EnabledChanged += new System.EventHandler(this.FlatForm_EnabledChanged);
			this.PaddingChanged += new System.EventHandler(this.Form_PaddingChanged);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form_Paint);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_MouseMove);
			this.Resize += new System.EventHandler(this.Form_Resize);
			this.panelCaptionArea.ResumeLayout(false);
			this.panelCaptionArea.PerformLayout();
			this.panelWinIcons.ResumeLayout(false);
			this.panelWinIcons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
			this.popupSysMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Label labelClose;
		private Label labelMaxRestore;
		private Label labelMinimize;
		private Label labelCaption;
		private Panel panelCaptionArea;
		private PictureBox pictureIcon;
		private ToolStripMenuItem itemRestore;
		private ToolStripMenuItem itemMove;
		private ToolStripMenuItem itemSize;
		private ToolStripMenuItem itemMinimize;
		private ToolStripMenuItem itemMaximize;
		private ToolStripSeparator toolStripMenuItem1;
		private ToolStripMenuItem itemClose;
		private ContextMenuStrip popupSysMenu;
		private ToolTip toolTip;
		private TableLayoutPanel panelWinIcons;
	}
}