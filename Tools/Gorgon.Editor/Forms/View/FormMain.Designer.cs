using System.ComponentModel;
using System.Windows.Forms;
using KRBTabControl;

namespace Gorgon.Editor
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

			if (disposing)
			{
				if (_contentManager != null)
				{
					_contentManager.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.toolsFiles = new System.Windows.Forms.ToolStripContainer();
			this.stripFiles = new System.Windows.Forms.ToolStrip();
			this.dropNewContent = new System.Windows.Forms.ToolStripDropDownButton();
			this.popupAddContentMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEditContent = new System.Windows.Forms.ToolStripButton();
			this.buttonDeleteContent = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonShowAll = new System.Windows.Forms.ToolStripButton();
			this.popupItemAddContent = new System.Windows.Forms.ToolStripMenuItem();
			this.panelMenu = new System.Windows.Forms.Panel();
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.itemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.itemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.itemAddContent = new System.Windows.Forms.ToolStripMenuItem();
			this.itemCreateFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.itemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.itemImport = new System.Windows.Forms.ToolStripMenuItem();
			this.itemExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.menuRecentFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.itemCut = new System.Windows.Forms.ToolStripMenuItem();
			this.itemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.itemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.itemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.itemPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.panelFooter = new System.Windows.Forms.Panel();
			this.statusMain = new System.Windows.Forms.StatusStrip();
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.panelContentHost = new System.Windows.Forms.Panel();
			this.panelExplorer = new System.Windows.Forms.Panel();
			this.tabPages = new KRBTabControl.KRBTabControl();
			this.pageFiles = new KRBTabControl.TabPageEx();
			this.pageProperties = new KRBTabControl.TabPageEx();
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.popupProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.itemResetValue = new System.Windows.Forms.ToolStripMenuItem();
			this.popupFileSystem = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.popupItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemCreateFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.popupItemInclude = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemExclude = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemIncludeAll = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemExcludeAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.popupItemCut = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.popupItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.popupItemRename = new System.Windows.Forms.ToolStripMenuItem();
			this.dialogOpenFile = new System.Windows.Forms.OpenFileDialog();
			this.dialogSaveFile = new System.Windows.Forms.SaveFileDialog();
			this.dialogImport = new System.Windows.Forms.OpenFileDialog();
			this.dialogExport = new System.Windows.Forms.FolderBrowserDialog();
			this.labelUnCollapse = new System.Windows.Forms.Label();
			this.tipMainWindow = new System.Windows.Forms.ToolTip(this.components);
			this.treeFileSystem = new Gorgon.Editor.FileSystemTreeView();
			this.ContentArea.SuspendLayout();
			this.toolsFiles.ContentPanel.SuspendLayout();
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
			this.pageProperties.SuspendLayout();
			this.popupProperties.SuspendLayout();
			this.popupFileSystem.SuspendLayout();
			this.SuspendLayout();
			// 
			// ContentArea
			// 
			this.ContentArea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.ContentArea.Controls.Add(this.splitMain);
			this.ContentArea.Controls.Add(this.labelUnCollapse);
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
			this.toolsFiles.ContentPanel.Controls.Add(this.treeFileSystem);
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
            this.dropNewContent,
            this.toolStripSeparator9,
            this.buttonEditContent,
            this.buttonDeleteContent,
            this.toolStripButton1,
            this.buttonShowAll});
			this.stripFiles.Name = "stripFiles";
			this.stripFiles.Stretch = true;
			// 
			// dropNewContent
			// 
			this.dropNewContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.dropNewContent.DropDown = this.popupAddContentMenu;
			resources.ApplyResources(this.dropNewContent, "dropNewContent");
			this.dropNewContent.Image = global::Gorgon.Editor.Properties.Resources.new_item_16x16;
			this.dropNewContent.Name = "dropNewContent";
			// 
			// popupAddContentMenu
			// 
			this.popupAddContentMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.popupAddContentMenu.Name = "popupAddContentMenu";
			this.popupAddContentMenu.OwnerItem = this.popupItemAddContent;
			resources.ApplyResources(this.popupAddContentMenu, "popupAddContentMenu");
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			resources.ApplyResources(this.toolStripSeparator9, "toolStripSeparator9");
			// 
			// buttonEditContent
			// 
			this.buttonEditContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.buttonEditContent, "buttonEditContent");
			this.buttonEditContent.Image = global::Gorgon.Editor.Properties.Resources.edit_16x16;
			this.buttonEditContent.Name = "buttonEditContent";
			// 
			// buttonDeleteContent
			// 
			this.buttonDeleteContent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.buttonDeleteContent, "buttonDeleteContent");
			this.buttonDeleteContent.Image = global::Gorgon.Editor.Properties.Resources.delete_item_16x16;
			this.buttonDeleteContent.Name = "buttonDeleteContent";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.Name = "toolStripButton1";
			resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
			// 
			// buttonShowAll
			// 
			this.buttonShowAll.CheckOnClick = true;
			this.buttonShowAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonShowAll.Image = global::Gorgon.Editor.Properties.Resources.show_all_16x16;
			resources.ApplyResources(this.buttonShowAll, "buttonShowAll");
			this.buttonShowAll.Name = "buttonShowAll";
			// 
			// popupItemAddContent
			// 
			this.popupItemAddContent.DropDown = this.popupAddContentMenu;
			resources.ApplyResources(this.popupItemAddContent, "popupItemAddContent");
			this.popupItemAddContent.Name = "popupItemAddContent";
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
            this.menuFile,
            this.menuEdit});
			resources.ApplyResources(this.menuMain, "menuMain");
			this.menuMain.Name = "menuMain";
			// 
			// menuFile
			// 
			this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNew,
            this.itemOpen,
            this.toolStripSeparator3,
            this.itemAddContent,
            this.itemCreateFolder,
            this.toolStripMenuItem1,
            this.itemSave,
            this.itemSaveAs,
            this.toolStripMenuItem2,
            this.itemImport,
            this.itemExport,
            this.toolStripMenuItem3,
            this.menuRecentFiles,
            this.toolStripMenuItem4,
            this.exitToolStripMenuItem});
			this.menuFile.Name = "menuFile";
			resources.ApplyResources(this.menuFile, "menuFile");
			// 
			// itemNew
			// 
			this.itemNew.Name = "itemNew";
			resources.ApplyResources(this.itemNew, "itemNew");
			this.itemNew.Click += new System.EventHandler(this.itemNew_Click);
			// 
			// itemOpen
			// 
			resources.ApplyResources(this.itemOpen, "itemOpen");
			this.itemOpen.Name = "itemOpen";
			this.itemOpen.Click += new System.EventHandler(this.itemOpen_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			// 
			// itemAddContent
			// 
			resources.ApplyResources(this.itemAddContent, "itemAddContent");
			this.itemAddContent.Name = "itemAddContent";
			// 
			// itemCreateFolder
			// 
			resources.ApplyResources(this.itemCreateFolder, "itemCreateFolder");
			this.itemCreateFolder.Name = "itemCreateFolder";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
			// 
			// itemSave
			// 
			resources.ApplyResources(this.itemSave, "itemSave");
			this.itemSave.Name = "itemSave";
			// 
			// itemSaveAs
			// 
			resources.ApplyResources(this.itemSaveAs, "itemSaveAs");
			this.itemSaveAs.Name = "itemSaveAs";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
			// 
			// itemImport
			// 
			this.itemImport.Name = "itemImport";
			resources.ApplyResources(this.itemImport, "itemImport");
			// 
			// itemExport
			// 
			resources.ApplyResources(this.itemExport, "itemExport");
			this.itemExport.Name = "itemExport";
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
			// 
			// menuRecentFiles
			// 
			resources.ApplyResources(this.menuRecentFiles, "menuRecentFiles");
			this.menuRecentFiles.Name = "menuRecentFiles";
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// menuEdit
			// 
			this.menuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemCut,
            this.itemCopy,
            this.itemPaste,
            this.toolStripSeparator6,
            this.itemDelete,
            this.toolStripSeparator7,
            this.itemPreferences});
			this.menuEdit.Name = "menuEdit";
			resources.ApplyResources(this.menuEdit, "menuEdit");
			// 
			// itemCut
			// 
			resources.ApplyResources(this.itemCut, "itemCut");
			this.itemCut.Name = "itemCut";
			// 
			// itemCopy
			// 
			resources.ApplyResources(this.itemCopy, "itemCopy");
			this.itemCopy.Name = "itemCopy";
			// 
			// itemPaste
			// 
			resources.ApplyResources(this.itemPaste, "itemPaste");
			this.itemPaste.Name = "itemPaste";
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
			// 
			// itemDelete
			// 
			resources.ApplyResources(this.itemDelete, "itemDelete");
			this.itemDelete.Name = "itemDelete";
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
			// 
			// itemPreferences
			// 
			this.itemPreferences.Name = "itemPreferences";
			resources.ApplyResources(this.itemPreferences, "itemPreferences");
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
			this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitMain.Name = "splitMain";
			// 
			// splitMain.Panel1
			// 
			this.splitMain.Panel1.Controls.Add(this.panelContentHost);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.Controls.Add(this.panelExplorer);
			this.splitMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.splitMain_MouseDoubleClick);
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
			this.pageProperties.Controls.Add(this.propertyGrid);
			this.pageProperties.IsClosable = false;
			resources.ApplyResources(this.pageProperties, "pageProperties");
			this.pageProperties.Name = "pageProperties";
			// 
			// propertyGrid
			// 
			this.propertyGrid.CanShowVisualStyleGlyphs = false;
			this.propertyGrid.ContextMenuStrip = this.popupProperties;
			resources.ApplyResources(this.propertyGrid, "propertyGrid");
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.ToolbarVisible = false;
			// 
			// popupProperties
			// 
			this.popupProperties.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.popupProperties.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemResetValue});
			this.popupProperties.Name = "popupProperties";
			resources.ApplyResources(this.popupProperties, "popupProperties");
			// 
			// itemResetValue
			// 
			this.itemResetValue.Name = "itemResetValue";
			resources.ApplyResources(this.itemResetValue, "itemResetValue");
			// 
			// popupFileSystem
			// 
			this.popupFileSystem.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.popupFileSystem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.popupItemAddContent,
            this.popupItemEdit,
            this.popupItemCreateFolder,
            this.toolStripSeparator10,
            this.popupItemInclude,
            this.popupItemExclude,
            this.popupItemIncludeAll,
            this.popupItemExcludeAll,
            this.toolStripSeparator4,
            this.popupItemCut,
            this.popupItemCopy,
            this.popupItemPaste,
            this.popupItemDelete,
            this.toolStripSeparator5,
            this.popupItemRename});
			this.popupFileSystem.Name = "popupFileSystem";
			resources.ApplyResources(this.popupFileSystem, "popupFileSystem");
			// 
			// popupItemEdit
			// 
			resources.ApplyResources(this.popupItemEdit, "popupItemEdit");
			this.popupItemEdit.Name = "popupItemEdit";
			// 
			// popupItemCreateFolder
			// 
			resources.ApplyResources(this.popupItemCreateFolder, "popupItemCreateFolder");
			this.popupItemCreateFolder.Name = "popupItemCreateFolder";
			// 
			// toolStripSeparator10
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
			// 
			// popupItemInclude
			// 
			this.popupItemInclude.Name = "popupItemInclude";
			resources.ApplyResources(this.popupItemInclude, "popupItemInclude");
			// 
			// popupItemExclude
			// 
			this.popupItemExclude.Name = "popupItemExclude";
			resources.ApplyResources(this.popupItemExclude, "popupItemExclude");
			// 
			// popupItemIncludeAll
			// 
			this.popupItemIncludeAll.Name = "popupItemIncludeAll";
			resources.ApplyResources(this.popupItemIncludeAll, "popupItemIncludeAll");
			// 
			// popupItemExcludeAll
			// 
			this.popupItemExcludeAll.Name = "popupItemExcludeAll";
			resources.ApplyResources(this.popupItemExcludeAll, "popupItemExcludeAll");
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
			// 
			// popupItemCut
			// 
			resources.ApplyResources(this.popupItemCut, "popupItemCut");
			this.popupItemCut.Name = "popupItemCut";
			// 
			// popupItemCopy
			// 
			resources.ApplyResources(this.popupItemCopy, "popupItemCopy");
			this.popupItemCopy.Name = "popupItemCopy";
			// 
			// popupItemPaste
			// 
			resources.ApplyResources(this.popupItemPaste, "popupItemPaste");
			this.popupItemPaste.Name = "popupItemPaste";
			// 
			// popupItemDelete
			// 
			resources.ApplyResources(this.popupItemDelete, "popupItemDelete");
			this.popupItemDelete.Name = "popupItemDelete";
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
			// 
			// popupItemRename
			// 
			resources.ApplyResources(this.popupItemRename, "popupItemRename");
			this.popupItemRename.Name = "popupItemRename";
			// 
			// dialogOpenFile
			// 
			resources.ApplyResources(this.dialogOpenFile, "dialogOpenFile");
			// 
			// dialogSaveFile
			// 
			resources.ApplyResources(this.dialogSaveFile, "dialogSaveFile");
			// 
			// dialogImport
			// 
			this.dialogImport.Multiselect = true;
			resources.ApplyResources(this.dialogImport, "dialogImport");
			// 
			// labelUnCollapse
			// 
			this.labelUnCollapse.Cursor = System.Windows.Forms.Cursors.Hand;
			resources.ApplyResources(this.labelUnCollapse, "labelUnCollapse");
			this.labelUnCollapse.Image = global::Gorgon.Editor.Properties.Resources.expandprops_16x16;
			this.labelUnCollapse.Name = "labelUnCollapse";
			this.tipMainWindow.SetToolTip(this.labelUnCollapse, resources.GetString("labelUnCollapse.ToolTip"));
			this.labelUnCollapse.Click += new System.EventHandler(this.labelUnCollapse_Click);
			this.labelUnCollapse.MouseEnter += new System.EventHandler(this.labelUnCollapse_MouseEnter);
			this.labelUnCollapse.MouseLeave += new System.EventHandler(this.labelUnCollapse_MouseLeave);
			// 
			// treeFileSystem
			// 
			this.treeFileSystem.BackColor = System.Drawing.SystemColors.Window;
			this.treeFileSystem.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.treeFileSystem, "treeFileSystem");
			this.treeFileSystem.ForeColor = System.Drawing.SystemColors.WindowText;
			this.treeFileSystem.Name = "treeFileSystem";
			this.treeFileSystem.SelectedNode = null;
			this.treeFileSystem.Sorted = true;
			// 
			// FormMain
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Border = true;
			this.BorderSize = 3;
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
			this.Theme.MenuCheckDisabledImage = global::Gorgon.Editor.Properties.Resources.Check_Disabled1;
			this.Theme.MenuCheckEnabledImage = global::Gorgon.Editor.Properties.Resources.Check_Enabled1;
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
			this.Controls.SetChildIndex(this.ContentArea, 0);
			this.ContentArea.ResumeLayout(false);
			this.ContentArea.PerformLayout();
			this.toolsFiles.ContentPanel.ResumeLayout(false);
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
			this.pageProperties.ResumeLayout(false);
			this.popupProperties.ResumeLayout(false);
			this.popupFileSystem.ResumeLayout(false);
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
		private ToolStripMenuItem menuFile;
		private ToolStripMenuItem itemNew;
		private KRBTabControl.KRBTabControl tabPages;
		private TabPageEx pageFiles;
		private ToolStripContainer toolsFiles;
		private ToolStrip stripFiles;
		private TabPageEx pageProperties;
		private ToolStripMenuItem itemOpen;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem itemAddContent;
		private ToolStripMenuItem itemCreateFolder;
		private ToolStripSeparator toolStripMenuItem1;
		private ToolStripMenuItem itemSave;
		private ToolStripMenuItem itemSaveAs;
		private ToolStripSeparator toolStripMenuItem2;
		private ToolStripMenuItem itemImport;
		private ToolStripMenuItem itemExport;
		private ToolStripSeparator toolStripMenuItem3;
		private ToolStripMenuItem menuRecentFiles;
		private ToolStripSeparator toolStripMenuItem4;
		private ToolStripMenuItem exitToolStripMenuItem;
		private ContextMenuStrip popupProperties;
		private ToolStripMenuItem itemResetValue;
		private ContextMenuStrip popupFileSystem;
		private ToolStripMenuItem popupItemAddContent;
		private ContextMenuStrip popupAddContentMenu;
		private ToolStripMenuItem popupItemEdit;
		private ToolStripMenuItem popupItemCreateFolder;
		private ToolStripSeparator toolStripSeparator10;
		private ToolStripMenuItem popupItemInclude;
		private ToolStripMenuItem popupItemExclude;
		private ToolStripMenuItem popupItemIncludeAll;
		private ToolStripMenuItem popupItemExcludeAll;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripMenuItem popupItemCut;
		private ToolStripMenuItem popupItemCopy;
		private ToolStripMenuItem popupItemPaste;
		private ToolStripMenuItem popupItemDelete;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripMenuItem popupItemRename;
		private OpenFileDialog dialogOpenFile;
		private SaveFileDialog dialogSaveFile;
		private OpenFileDialog dialogImport;
		private FolderBrowserDialog dialogExport;
		private ToolStripMenuItem menuEdit;
		private ToolStripMenuItem itemCut;
		private ToolStripMenuItem itemCopy;
		private ToolStripMenuItem itemPaste;
		private ToolStripSeparator toolStripSeparator6;
		private ToolStripMenuItem itemDelete;
		private ToolStripSeparator toolStripSeparator7;
		private ToolStripMenuItem itemPreferences;
		private ToolStripDropDownButton dropNewContent;
		private ToolStripSeparator toolStripSeparator9;
		private ToolStripButton buttonEditContent;
		private ToolStripButton buttonDeleteContent;
		private ToolStripSeparator toolStripButton1;
		private ToolStripButton buttonShowAll;
		private PropertyGrid propertyGrid;
		private Label labelUnCollapse;
		private ToolTip tipMainWindow;
		private FileSystemTreeView treeFileSystem;


	}
}

