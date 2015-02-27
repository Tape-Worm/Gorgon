using System.ComponentModel;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
	partial class FormMain
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
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.toolsFiles = new System.Windows.Forms.ToolStripContainer();
			this.stripFiles = new System.Windows.Forms.ToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
			this.panelMenu = new System.Windows.Forms.Panel();
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panelFooter = new System.Windows.Forms.Panel();
			this.statusMain = new System.Windows.Forms.StatusStrip();
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.panelContentHost = new System.Windows.Forms.Panel();
			this.panelExplorer = new System.Windows.Forms.Panel();
			this.tabPages = new KRBTabControl.KRBTabControl();
			this.pageFiles = new KRBTabControl.TabPageEx();
			this.pageProperties = new KRBTabControl.TabPageEx();
			this.ContentArea.SuspendLayout();
			this.toolsFiles.TopToolStripPanel.SuspendLayout();
			this.toolsFiles.SuspendLayout();
			this.stripFiles.SuspendLayout();
			this.panelMenu.SuspendLayout();
			this.menuMain.SuspendLayout();
			this.panelFooter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.panelExplorer.SuspendLayout();
			this.tabPages.SuspendLayout();
			this.pageFiles.SuspendLayout();
			this.SuspendLayout();
			// 
			// ContentArea
			// 
			this.ContentArea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.ContentArea.Controls.Add(this.splitMain);
			this.ContentArea.Controls.Add(this.panelFooter);
			this.ContentArea.Controls.Add(this.panelMenu);
			this.ContentArea.ForeColor = System.Drawing.Color.Silver;
			resources.ApplyResources(this.ContentArea, "ContentArea");
			// 
			// toolsFiles
			// 
			// 
			// toolsFiles.ContentPanel
			// 
			resources.ApplyResources(this.toolsFiles.ContentPanel, "toolsFiles.ContentPanel");
			resources.ApplyResources(this.toolsFiles, "toolsFiles");
			this.toolsFiles.Name = "toolsFiles";
			// 
			// toolsFiles.TopToolStripPanel
			// 
			this.toolsFiles.TopToolStripPanel.Controls.Add(this.stripFiles);
			// 
			// stripFiles
			// 
			resources.ApplyResources(this.stripFiles, "stripFiles");
			this.stripFiles.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripFiles.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.stripFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.toolStripButton2,
            this.toolStripButton3,
            this.toolStripSeparator2,
            this.toolStripButton4});
			this.stripFiles.Name = "stripFiles";
			this.stripFiles.Stretch = true;
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
			this.toolStripButton1.Name = "toolStripButton1";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButton2, "toolStripButton2");
			this.toolStripButton2.Name = "toolStripButton2";
			// 
			// toolStripButton3
			// 
			this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButton3, "toolStripButton3");
			this.toolStripButton3.Name = "toolStripButton3";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// toolStripButton4
			// 
			this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButton4, "toolStripButton4");
			this.toolStripButton4.Name = "toolStripButton4";
			// 
			// panelMenu
			// 
			resources.ApplyResources(this.panelMenu, "panelMenu");
			this.panelMenu.Controls.Add(this.menuMain);
			this.panelMenu.Name = "panelMenu";
			// 
			// menuMain
			// 
			this.menuMain.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
			resources.ApplyResources(this.menuMain, "menuMain");
			this.menuMain.Name = "menuMain";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Checked = true;
			this.newToolStripMenuItem.CheckOnClick = true;
			this.newToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			resources.ApplyResources(this.newToolStripMenuItem, "newToolStripMenuItem");
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
			// 
			// panelFooter
			// 
			resources.ApplyResources(this.panelFooter, "panelFooter");
			this.panelFooter.Controls.Add(this.statusMain);
			this.panelFooter.Name = "panelFooter";
			// 
			// statusMain
			// 
			this.statusMain.ImageScalingSize = new System.Drawing.Size(20, 20);
			resources.ApplyResources(this.statusMain, "statusMain");
			this.statusMain.Name = "statusMain";
			this.statusMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.statusMain.SizingGrip = false;
			// 
			// splitMain
			// 
			resources.ApplyResources(this.splitMain, "splitMain");
			this.splitMain.Name = "splitMain";
			// 
			// splitMain.Panel1
			// 
			this.splitMain.Panel1.Controls.Add(this.panelContentHost);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.Controls.Add(this.panelExplorer);
			// 
			// panelContentHost
			// 
			resources.ApplyResources(this.panelContentHost, "panelContentHost");
			this.panelContentHost.Name = "panelContentHost";
			// 
			// panelExplorer
			// 
			this.panelExplorer.Controls.Add(this.tabPages);
			resources.ApplyResources(this.panelExplorer, "panelExplorer");
			this.panelExplorer.Name = "panelExplorer";
			// 
			// tabPages
			// 
			resources.ApplyResources(this.tabPages, "tabPages");
			this.tabPages.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
			this.tabPages.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabPages.BorderColor = System.Drawing.Color.Magenta;
			this.tabPages.Controls.Add(this.pageFiles);
			this.tabPages.Controls.Add(this.pageProperties);
			this.tabPages.IsCaptionVisible = false;
			this.tabPages.IsDocumentTabStyle = true;
			this.tabPages.IsDrawHeader = false;
			this.tabPages.IsUserInteraction = false;
			this.tabPages.Name = "tabPages";
			this.tabPages.SelectedIndex = 0;
			this.tabPages.TabBorderColor = System.Drawing.Color.Green;
			this.tabPages.TabGradient.ColorEnd = System.Drawing.Color.Magenta;
			this.tabPages.TabGradient.ColorStart = System.Drawing.Color.Magenta;
			this.tabPages.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.Yellow;
			this.tabPages.TabGradient.TabPageTextColor = System.Drawing.Color.Yellow;
			this.tabPages.TabHOffset = -2;
			this.tabPages.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
			// 
			// pageFiles
			// 
			this.pageFiles.Controls.Add(this.toolsFiles);
			this.pageFiles.IsClosable = false;
			resources.ApplyResources(this.pageFiles, "pageFiles");
			this.pageFiles.Name = "pageFiles";
			// 
			// pageProperties
			// 
			resources.ApplyResources(this.pageProperties, "pageProperties");
			this.pageProperties.Name = "pageProperties";
			this.pageProperties.IsClosable = false;
			// 
			// FormMain
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Border = true;
			this.BorderSize = 2;
			this.Name = "FormMain";
			this.ResizeHandleSize = 4;
			this.Theme.CheckBoxBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.Theme.CheckBoxBackColorHilight = System.Drawing.Color.SteelBlue;
			this.Theme.ContentPanelBackground = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Theme.DisabledColor = System.Drawing.Color.Black;
			this.Theme.DropDownBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(88)))), ((int)(((byte)(88)))));
			this.Theme.ForeColor = System.Drawing.Color.Silver;
			this.Theme.ForeColorInactive = System.Drawing.Color.Black;
			this.Theme.HilightBackColor = System.Drawing.Color.SteelBlue;
			this.Theme.HilightForeColor = System.Drawing.Color.White;
			this.Theme.MenuCheckDisabledImage = global::GorgonLibrary.Editor.Properties.Resources.Check_Disabled1;
			this.Theme.MenuCheckEnabledImage = global::GorgonLibrary.Editor.Properties.Resources.Check_Enabled1;
			this.Theme.ToolStripArrowColor = System.Drawing.Color.White;
			this.Theme.ToolStripBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.Theme.WindowBackground = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.Theme.WindowBorderActive = System.Drawing.Color.SteelBlue;
			this.Theme.WindowBorderInactive = System.Drawing.Color.Black;
			this.Theme.WindowCloseIconForeColor = System.Drawing.Color.White;
			this.Theme.WindowCloseIconForeColorHilight = System.Drawing.Color.White;
			this.Theme.WindowSizeIconsBackColorHilight = System.Drawing.Color.SteelBlue;
			this.Theme.WindowSizeIconsForeColor = System.Drawing.Color.White;
			this.Theme.WindowSizeIconsForeColorHilight = System.Drawing.Color.White;
			this.ContentArea.ResumeLayout(false);
			this.ContentArea.PerformLayout();
			this.toolsFiles.TopToolStripPanel.ResumeLayout(false);
			this.toolsFiles.TopToolStripPanel.PerformLayout();
			this.toolsFiles.ResumeLayout(false);
			this.toolsFiles.PerformLayout();
			this.stripFiles.ResumeLayout(false);
			this.stripFiles.PerformLayout();
			this.panelMenu.ResumeLayout(false);
			this.panelMenu.PerformLayout();
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.panelFooter.ResumeLayout(false);
			this.panelFooter.PerformLayout();
			this.splitMain.Panel1.ResumeLayout(false);
			this.splitMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
			this.splitMain.ResumeLayout(false);
			this.panelExplorer.ResumeLayout(false);
			this.tabPages.ResumeLayout(false);
			this.pageFiles.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Panel panelFooter;
		private StatusStrip statusMain;
		private Panel panelMenu;
		private MenuStrip menuMain;
		private SplitContainer splitMain;
		private Panel panelContentHost;
		private Panel panelExplorer;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem editToolStripMenuItem;
		private ToolStripMenuItem newToolStripMenuItem;
		private KRBTabControl.KRBTabControl tabPages;
		private KRBTabControl.TabPageEx pageFiles;
		private ToolStripContainer toolsFiles;
		private ToolStrip stripFiles;
		private ToolStripDropDownButton toolStripButton1;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton toolStripButton2;
		private ToolStripButton toolStripButton3;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripButton toolStripButton4;
		private KRBTabControl.TabPageEx pageProperties;


	}
}

