using System.ComponentModel;

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
			this.panelMenu = new System.Windows.Forms.Panel();
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panelFooter = new System.Windows.Forms.Panel();
			this.statusMain = new System.Windows.Forms.StatusStrip();
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.panelContentHost = new System.Windows.Forms.Panel();
			this.panelExplorer = new System.Windows.Forms.Panel();
			this.ContentArea.SuspendLayout();
			this.panelMenu.SuspendLayout();
			this.menuMain.SuspendLayout();
			this.panelFooter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// ContentArea
			// 
			this.ContentArea.Controls.Add(this.splitMain);
			this.ContentArea.Controls.Add(this.panelFooter);
			this.ContentArea.Controls.Add(this.panelMenu);
			resources.ApplyResources(this.ContentArea, "ContentArea");
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
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
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
			resources.ApplyResources(this.panelExplorer, "panelExplorer");
			this.panelExplorer.Name = "panelExplorer";
			// 
			// FormMain
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Border = true;
			this.Name = "FormMain";
			this.ResizeHandleSize = 4;
			this.ContentArea.ResumeLayout(false);
			this.ContentArea.PerformLayout();
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
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelFooter;
		private System.Windows.Forms.StatusStrip statusMain;
		private System.Windows.Forms.Panel panelMenu;
		private System.Windows.Forms.MenuStrip menuMain;
		private System.Windows.Forms.SplitContainer splitMain;
		private System.Windows.Forms.Panel panelContentHost;
		private System.Windows.Forms.Panel panelExplorer;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;


	}
}

